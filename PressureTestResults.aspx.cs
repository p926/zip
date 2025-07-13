/*
 * File: PressureTestResults
 * Author: JA
 * Created: November 2010
 * Version: 1.0
 * 
 * Maintain Instadose Financial Rate Details
 * 
 * Change History:
 *  > 1.0: Original release
 * 
 *  Copyright (C) 2010 Mirion Technologies. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data.SqlClient;
using System.Net.Mail;

using Instadose;
using Instadose.Data;

public partial class InstaDose_TechOps_PressureTestResults : System.Web.UI.Page
{
  // LCDISBUS database
 

  InsDataContext idc = new InsDataContext();

  //AppDataContext adc = new AppDataContext();
  //protected string wkConnectionString = "LCDIS";
  //protected string wkConnectionString2 = "LCDISBUS";
  //protected Boolean goodCall = false;
  protected Int32 wkDeviceID;
  protected string wkSerialNo;
  protected Int32 wkCalibReadRID;
  protected string wkGroupName;
  protected int wkDeviceCount;
  
  protected void Page_Load(object sender, EventArgs e)
  {
    if (!IsPostBack)
    {      
      pnlSearch.Visible = true;
      pnlSelect.Visible = false;
      pnlReads.Visible = false;      

    }
    else{
        InvisibleErrors();
    }
    this.btnPrint.Visible = false;
  }

  protected void btnCancel_Click(object sender, EventArgs e)
  {    
   
    pnlSearch.Visible = true;
    pnlSelect.Visible = false;
    pnlReads.Visible = false;   
   
  }
 
  protected void btnPrint_Click(object sender, EventArgs e)
  {    
    Session["ctrl"] = PrintPanel;
    ClientScript.RegisterStartupScript(this.GetType(), 
      "onclick", 
      "<script language=javascript>window.open('PrintIt.aspx'," 
       + "'PrintMe','height=600px,width=900px,scrollbars=1');</script>"); 

    pnlSelect.Visible = true;
    pnlReads.Visible = true;
  
  }

  protected void ClearScreenFields()
  {
    lblInitialGroup.Text = "";
    lblReportDate.Text = "";
    lblQtyPassedP1.Text = "";
    lblQtyPassedP2.Text = "";
    lblQtyFailedP1.Text = "";
    lblQtyFailedP2.Text = "";
    lblNumberOfDevices.Text = "";
    lblIrradiationValue.Text = "";

  }

 protected void GetDeviceCounts(int groupNameID)
 {   

   try
   {
       var q = (from p in idc.CalibPressureTestDetails
                      join d in idc.CalibPressureTestHeaders on p.PressureTestHeaderID equals d.PressureTestHeaderID
                      where d.DeviceGroupID == groupNameID
                      select new { p.DeviceID, d.Irradiation}).Distinct();
       if (q != null)
       {
           lblNumberOfDevices.Text = Convert.ToString(q.Count());
           lblIrradiationValue.Text = Convert.ToString(q.First().Irradiation);
       }
     
   }
   catch
   {
    
   }

 } 

  protected void DDLSelectGroup_SelectedIndexChanged(object sender, EventArgs e)
  {
    String selectedGroupName = ((DropDownList)sender).SelectedItem.Text;
    String selectedGroupNameID = ((DropDownList)sender).SelectedItem.Value;

    if (selectedGroupNameID == "0")
    {
        pnlSelect.Visible = false;
        pnlReads.Visible = false;
        this.btnPrint.Visible = false;
    }
    else
    {
        wkGroupName = selectedGroupName;
        wkDeviceCount = 0;

        ClearScreenFields();
        lblInitialGroup.Text = wkGroupName;
        lblReportDate.Text = System.DateTime.Now.ToShortDateString();

        LoadGVPressureTestGridView(int.Parse(selectedGroupNameID));
        GetDeviceCounts(int.Parse(selectedGroupNameID));
        GetPressureTest1Subtotals(int.Parse(selectedGroupNameID));
        GetPressureTest2Subtotals(int.Parse(selectedGroupNameID));

        pnlSelect.Visible = true;
        pnlReads.Visible = true;
        this.btnPrint.Visible = true;
    }
       
  }

  protected void LoadGVPressureTestGridView(int groupNameID)
  {
      IQueryable pth = from p in idc.CalibPressureTestDetails
                       join d in idc.CalibPressureTestHeaders on p.PressureTestHeaderID equals d.PressureTestHeaderID
                       where d.DeviceGroupID == groupNameID
                       orderby d.CreatedDate
                       select new
                       {
                           d.ChamberNumber,
                           d.SlotNumber,
                           d.AtmospherePressure,
                           d.ChamberPressure,
                           p.DeviceInventory.SerialNo,
                           d.CreatedDate,
                           P1CreatedDate = (p.DeviceAnalysisStatus.DeviceAnalysisName == "Passed-P1" || p.DeviceAnalysisStatus.DeviceAnalysisName == "Failed-P1") ? d.CreatedDate.ToShortDateString() : null,
                           P2CreatedDate = (p.DeviceAnalysisStatus.DeviceAnalysisName == "Passed-P2" || p.DeviceAnalysisStatus.DeviceAnalysisName == "Failed-P2") ? d.CreatedDate.ToShortDateString() : null,
                           //P1CreatedDate = (p.DeviceAnalysisStatus.DeviceAnalysisName == "Passed-P1" || p.DeviceAnalysisStatus.DeviceAnalysisName == "Failed-P1") ? d.CreatedDate.ToShortDateString() : GetPressureTest1CreatedDate(p.DeviceID),
                           //P2CreatedDate = (p.DeviceAnalysisStatus.DeviceAnalysisName == "Passed-P2" || p.DeviceAnalysisStatus.DeviceAnalysisName == "Failed-P2") ? d.CreatedDate.ToShortDateString() : GetPressureTest2CreatedDate(p.DeviceID),
                           d.Irradiation,
                           Status = (p.DeviceAnalysisStatus.DeviceAnalysisName),
                           p.DeviceID,
                           ComparedDose = ConvertDose(p.ComparedDose),
                           p.DeliveredDose,
                           //P1DeliveredDose = (p.DeviceAnalysisStatus.DeviceAnalysisName == "Passed-P1" || p.DeviceAnalysisStatus.DeviceAnalysisName == "Failed-P1") ? ConvertDose(p.DeliveredDose.GetValueOrDefault()) : GetPressureTest1DeliveredDose(p.DeviceID),
                           //P2DeliveredDose = (p.DeviceAnalysisStatus.DeviceAnalysisName == "Passed-P2" || p.DeviceAnalysisStatus.DeviceAnalysisName == "Failed-P2") ? ConvertDose(p.DeliveredDose.GetValueOrDefault()) :  GetPressureTest2DeliveredDose(p.DeviceID)
                           P1DeliveredDose = (p.DeviceAnalysisStatus.DeviceAnalysisName == "Passed-P1" || p.DeviceAnalysisStatus.DeviceAnalysisName == "Failed-P1") ? ConvertDose(p.DeliveredDose.GetValueOrDefault()) : null,
                           P2DeliveredDose = (p.DeviceAnalysisStatus.DeviceAnalysisName == "Passed-P2" || p.DeviceAnalysisStatus.DeviceAnalysisName == "Failed-P2") ? ConvertDose(p.DeliveredDose.GetValueOrDefault()) : null
                       };

      gvPressureTests.DataSource = pth;
      gvPressureTests.DataBind();

  }

  protected string ConvertDose(double inDose)
  {
    string wkReturnDose = "";
    wkReturnDose = inDose.ToString("F");
    return wkReturnDose;

  }

  protected void GetPressureTest1Subtotals(int groupNameID)
  {
    int p1Passed = 0;
    int p1Failed = 0;

    try
    {
        var d = (from ptd in idc.CalibPressureTestDetails
                 join pth in idc.CalibPressureTestHeaders on ptd.PressureTestHeaderID equals pth.PressureTestHeaderID
                 where (ptd.DeviceAnalysisStatus.DeviceAnalysisName == "Passed-P1" || ptd.DeviceAnalysisStatus.DeviceAnalysisName == "Failed-P1")
                    && pth.DeviceGroupID == groupNameID
                 select ptd);


        foreach (var i in d)
        {
            switch (i.DeviceAnalysisStatus.DeviceAnalysisName)
            {
            case "Passed-P1":
                p1Passed += 1;
                break;

            case "Failed-P1":
                p1Failed += 1;
                break;

            default:
                break;
            }
        }
     
        lblQtyPassedP1.Text = Convert.ToString(p1Passed);
        lblQtyFailedP1.Text = Convert.ToString(p1Failed);
      
    }
    catch
    {
    }
    
  }

  protected void GetPressureTest2Subtotals(int groupNameID)
  {
    int p2Passed = 0;
    int p2Failed = 0;

    try
    {
        var d = (from ptd in idc.CalibPressureTestDetails
                 join pth in idc.CalibPressureTestHeaders on ptd.PressureTestHeaderID equals pth.PressureTestHeaderID
                 where (ptd.DeviceAnalysisStatus.DeviceAnalysisName == "Passed-P2" || ptd.DeviceAnalysisStatus.DeviceAnalysisName == "Failed-P2")
                    && pth.DeviceGroupID == groupNameID
                 select ptd);


        foreach (var i in d)
        {
            switch (i.DeviceAnalysisStatus.DeviceAnalysisName)
            {
            case "Passed-P2":
                p2Passed += 1;
                break;

            case "Failed-P2":
                p2Failed += 1;
                break;

            default:
                break;
            }
        }

        lblQtyPassedP2.Text = Convert.ToString(p2Passed);
        lblQtyFailedP2.Text = Convert.ToString(p2Failed);
    }
    catch
    {
    }

  }

  protected string GetPressureTest1CreatedDate(int inDeviceID)
  {
    string wkReturnDate = "";
   
    try
    {
      
      var d = (from ptd in idc.CalibPressureTestDetails 
               join pth in idc.CalibPressureTestHeaders on ptd.PressureTestHeaderID equals pth.PressureTestHeaderID
               where (ptd.DeviceAnalysisStatus.DeviceAnalysisName == "Passed-P1" || ptd.DeviceAnalysisStatus.DeviceAnalysisName == "Failed-P1")
                && ptd.DeviceID == inDeviceID
               orderby pth.CreatedDate
               select pth).FirstOrDefault();

      if (d != null)
      {
          //lblIrradiationValue.Text = Convert.ToString(d.Irradiation);
        wkReturnDate = d.CreatedDate.ToShortDateString();
      }

    }
    catch
    {
    }

    return wkReturnDate;

  }

  protected string GetPressureTest2CreatedDate(int inDeviceID)
  {
    string wkReturnDate = "";
   
    try
    {
        var d = (from ptd in idc.CalibPressureTestDetails
                 join pth in idc.CalibPressureTestHeaders on ptd.PressureTestHeaderID equals pth.PressureTestHeaderID
                 where (ptd.DeviceAnalysisStatus.DeviceAnalysisName == "Passed-P2" || ptd.DeviceAnalysisStatus.DeviceAnalysisName == "Failed-P2")
                  && ptd.DeviceID == inDeviceID
                 orderby pth.CreatedDate
                 select pth).FirstOrDefault();

        if (d != null)
        {
            //lblIrradiationValue.Text = Convert.ToString(d.Irradiation);
            wkReturnDate = d.CreatedDate.ToShortDateString();
        }
      
    }
    catch
    {
    }
    
    return wkReturnDate;

  }

  protected string GetPressureTest1DeliveredDose(int inDeviceID)
  {
    string wkReturnDose = "";

    try
    {
        var d = (from ptd in idc.CalibPressureTestDetails
                 join pth in idc.CalibPressureTestHeaders on ptd.PressureTestHeaderID equals pth.PressureTestHeaderID
                 where (ptd.DeviceAnalysisStatus.DeviceAnalysisName == "Passed-P1" || ptd.DeviceAnalysisStatus.DeviceAnalysisName == "Failed-P1")
                  && ptd.DeviceID == inDeviceID
                 orderby pth.CreatedDate
                 select ptd).FirstOrDefault();

        if (d != null)
        {
            double D = d.DeliveredDose.GetValueOrDefault();
            wkReturnDose = D.ToString("F");
        }

    }
    catch
    {
    }

    return wkReturnDose;

  }

  protected string GetPressureTest2DeliveredDose(int inDeviceID)
  {
    string wkReturnDose = "";

    try
    {
        var d = (from ptd in idc.CalibPressureTestDetails
                 join pth in idc.CalibPressureTestHeaders on ptd.PressureTestHeaderID equals pth.PressureTestHeaderID
                 where (ptd.DeviceAnalysisStatus.DeviceAnalysisName == "Passed-P2" || ptd.DeviceAnalysisStatus.DeviceAnalysisName == "Failed-P2")
                  && ptd.DeviceID == inDeviceID
                 orderby pth.CreatedDate
                 select ptd).FirstOrDefault();

        if (d != null)
        {
            double D = d.DeliveredDose.GetValueOrDefault();
            wkReturnDose = D.ToString("F");
        }

    }
    catch
    {
    }

    return wkReturnDose;

  }

  private void InvisibleErrors()
  {
      // Reset submission form error message      
      this.errorMsg.InnerText = "";
      this.errors.Visible = false;
  }

  private void VisibleErrors(string error)
  {
      this.errorMsg.InnerText = error;
      this.errors.Visible = true;
  }



}










  
