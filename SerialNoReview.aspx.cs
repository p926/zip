/*
 * File: SerialNoReview
 * Author: JA
 * Created: October 11, 2010
 * Version: 1.0
 * 
 * Maintain Instadose Financial Rate Details
 * 
 * Change History:
 *  > 1.0: Original release
 *  > 2.0: Upgrade to Instadose 2.0 database 12/02/11
 *  > 2.0: Modified by: TDO, on 8/29/2012. Enhanced and fixed the issue that the searching displays nothing.
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

public partial class InstaDose_TechOps_SerialNoReview : System.Web.UI.Page
{
  InsDataContext idc = new InsDataContext();

  protected Boolean goodCall = false;
  protected Int32 wkDeviceID;
  protected string wkSerialNo;
  protected Int32 wkCalibReadRID;
  
  protected void Page_Load(object sender, EventArgs e)
  {

    if (!IsPostBack)
    {        
        pnlSearch.Visible = true;
        pnlSelect.Visible = false;
        pnlReads.Visible = false;

        btnFind.Enabled = true;
        btnFind.Visible = true;

         SetFocus(txtSearchSerialNo);      
      
    }

    InvisibleErrors();
  }   
 
  protected void btnFind_Click(object sender, EventArgs e)
  {      
      ClearScreenFields();

      pnlSelect.Visible = true;
      pnlReads.Visible = true;
   
      GetEm();

  }

  protected void ClearScreenFields()
  {
    lblAtmosphere.Text = "";
    lblCalibDate.Text = "";  
    lblChamberNo.Text = "";
    lblChamberPressure.Text = "";
    lblColor.Text = "";
    lblCurrentGroup.Text = "";
    lblDateIMIInitialRead.Text = "";
    lblDatePostDrift.Text = "";
    lblDatePreDrift.Text = "";
    lblDatePressureRead1.Text = "";
    lblDatePressureRead2.Text = "";
    lblDateVerification.Text = "";
    lblDHcomp.Text = "";
    lblDHcomp.Text = "";
    lblDHDiff.Text = "";
    lblDHDoseIMIInitialRead.Text = "";
    lblDHDosePostDrift.Text = "";
    lblDHDosePreDrift.Text = "";
    lblDHDosePressureRead1.Text = "";
    lblDHDosePressureRead2.Text = "";
    lblDHDoseVerification.Text = "";
    lblDHIMIInitialRead.Text = "";
    lblDHPostDrift.Text = "";
    lblDHPreDrift.Text = "";
    lblDHPressureRead1.Text = "";
    lblDHPressureRead2.Text = "";
    lblDHtIMIInitialRead.Text = "";
    lblDHtPostDrift.Text = "";
    lblDHtPreDrift.Text = "";
    lblDHtPressureRead1.Text = "";
    lblDHtPressureRead2.Text = "";
    lblDHtVerification.Text = "";
    lblDHVerification.Text = "";
    lblDLcomp.Text = "";
    lblDLDiff.Text = "";
    lblDLDoseIMIInitialRead.Text = "";
    lblDLDosePostDrift.Text = "";
    lblDLDosePreDrift.Text = "";
    lblDLDosePressureRead1.Text = "";
    lblDLDosePressureRead2.Text = "";
    lblDLDoseVerification.Text = "";
    lblDLIMIInitialRead.Text = "";
    lblDLPostDrift.Text = "";
    lblDLPreDrift.Text = "";
    lblDLPressureRead1.Text = "";
    lblDLPressureRead2.Text = "";
    lblDLtIMIInitialRead.Text = "";
    lblDLtPostDrift.Text = "";
    lblDLtPreDrift.Text = "";
    lblDLtPressureRead1.Text = "";
    lblDLtPressureRead2.Text = "";
    lblDLtVerification.Text = "";
    lblDLVerification.Text = "";
    lblInitalGroup.Text = "";
    lblIrradiation.Text = "";
    lblSlotNo.Text = "";
    lblStatus.Text = "";
    lblZDH.Text = "";
    lblZDL.Text = "";

  }

  protected void GetEm()
  {      
      if (String.IsNullOrEmpty(txtSearchSerialNo.Text))
      {         
         this.VisibleErrors("Sorry, Serial# is required for lookup");
         pnlSelect.Visible = false;
         pnlReads.Visible = false;
         return;
      }
      else
      {

        if (!idc.DeviceInventories.Any(x => x.SerialNo == txtSearchSerialNo.Text.Trim()))
        {          
          this.VisibleErrors("No Device for " + txtSearchSerialNo.Text);
          pnlSelect.Visible = false;
          pnlReads.Visible = false;
          return;
        }

        else
        {
          try
          {
            var d = (from r in idc.DeviceInventories
                     join lgi in idc.DeviceGroups  on r.InitialGroupID equals lgi.DeviceGroupID 
                     join lgc in idc.DeviceGroups on r.CurrentGroupID equals lgc.DeviceGroupID
                     join c in idc.UserDeviceReads  on r.CalibrationReadID equals c.RID
                     join p in idc.Products on r.ProductID equals p.ProductID
                     join a in idc.DeviceAnalysisStatus on r.DeviceAnalysisStatusID equals a.DeviceAnalysisStatusID 
                     where r.SerialNo == txtSearchSerialNo.Text.Trim()
                     select new
                     {
                         r.DeviceID,
                         initialgroup = lgi.DeviceGroupName,
                         currentgroup = lgc.DeviceGroupName,
                         c.CreatedDate,
                         r.DeepLowDiff,
                         r.DeepHighDiff,
                         r.zDeepHighDiff,
                         r.zDeepLowDiff,
                         r.CalibrationReadID,
                         p.Color,
                         a.DeviceAnalysisName
                     }).ToList();

            if (d.Count > 0)
            {
                foreach (var di in d)
                {

                    lblInitalGroup.Text = di.initialgroup;
                    lblCurrentGroup.Text = di.currentgroup;
                    lblColor.Text = di.Color;
                    //lblStatus.Text = "di.Status";
                    lblStatus.Text = di.DeviceAnalysisName;
                    lblCalibDate.Text = di.CreatedDate.ToShortDateString();
                    lblDLDiff.Text = Convert.ToString(di.DeepLowDiff);
                    lblDHDiff.Text = Convert.ToString(di.DeepHighDiff);
                    lblZDL.Text = Convert.ToString(di.zDeepLowDiff);
                    lblZDH.Text = Convert.ToString(di.zDeepHighDiff);

                    wkDeviceID = di.DeviceID;
                    wkCalibReadRID = Convert.ToInt32(di.CalibrationReadID);

                    GetUserSessionDetails(wkDeviceID);
                    GetCalImiDeviceReads(wkDeviceID);
                    GetDriftAnalysis(wkDeviceID);
                }
            }
            else
            {
                this.VisibleErrors("Calibration Read does not exist.");
                pnlSelect.Visible = false;
                pnlReads.Visible = false;                
            }
            
          }
          catch
          {            
            this.VisibleErrors("problem with finding " + wkSerialNo);
          }
        }
              
      }
    
  }

  protected void GetUserSessionDetails(int inDeviceID)
  {
    Boolean DoIHaveVerificationRead = false;

    try
    {
      var q = from u in idc.UserDeviceReads
              where u.DeviceID == inDeviceID
              orderby u.ReadTypeID
              select new
              {
                u.RID,
                u.DeviceID,
                u.ReadTypeID,
                u.ExposureDate,
                u.DeepLow,
                u.DeepLowTemp,
                u.DeepLowDose,
                u.DeepHigh,
                u.DeepHighTemp,
                u.DeepHighDose

              };

      //iterate thru the extracted file
      foreach (var i in q)
      {
        GetDriftAnalysis(i.DeviceID);
         
        switch (i.ReadTypeID)
        {
        //Verification
        case 1:
          lblDateVerification.Text = i.ExposureDate.ToShortDateString();
          lblDLVerification.Text = Convert.ToString(i.DeepLow);
          lblDLtVerification.Text = Convert.ToString(i.DeepLowTemp);
          lblDLDoseVerification.Text = Convert.ToString(i.DeepLowDose);
          lblDHVerification.Text = Convert.ToString(i.DeepHigh);
          lblDHtVerification.Text = Convert.ToString(i.DeepHighTemp);
          lblDHDoseVerification.Text = Convert.ToString(i.DeepHighDose);

          DoIHaveVerificationRead = true;
          
          break;

        //PreDrift & Verification
        case 3:
          lblDatePreDrift.Text = i.ExposureDate.ToShortDateString();
          lblDLPreDrift.Text = Convert.ToString(i.DeepLow);
          lblDLtPreDrift.Text = Convert.ToString(i.DeepLowTemp);
          lblDLDosePreDrift.Text = Convert.ToString(i.DeepLowDose);
          lblDHPreDrift.Text = Convert.ToString(i.DeepHigh);
          lblDHtPreDrift.Text = Convert.ToString(i.DeepHighTemp);
          lblDHDosePreDrift.Text = Convert.ToString(i.DeepHighDose);

          if (!DoIHaveVerificationRead)
          {
            lblDateVerification.Text = i.ExposureDate.ToShortDateString();
            lblDLVerification.Text = Convert.ToString(i.DeepLow);
            lblDLtVerification.Text = Convert.ToString(i.DeepLowTemp);
            lblDLDoseVerification.Text = Convert.ToString(i.DeepLowDose);
            lblDHVerification.Text = Convert.ToString(i.DeepHigh);
            lblDHtVerification.Text = Convert.ToString(i.DeepHighTemp);
            lblDHDoseVerification.Text = Convert.ToString(i.DeepHighDose);
          }
         
          break;

        //PostDrift
        case 4:
          lblDatePostDrift.Text = i.ExposureDate.ToShortDateString();
          lblDLPostDrift.Text = Convert.ToString(i.DeepLow);
          lblDLtPostDrift.Text = Convert.ToString(i.DeepLowTemp);
          lblDLDosePostDrift.Text = Convert.ToString(i.DeepLowDose);
          lblDHPostDrift.Text = Convert.ToString(i.DeepHigh);
          lblDHtPostDrift.Text = Convert.ToString(i.DeepHighTemp);
          lblDHDosePostDrift.Text = Convert.ToString(i.DeepHighDose);
          
          break;

        //Calibrate from IMI
        case 5:

          GetPressureTestDetails(i.DeviceID);
          //GetPressureTestDetails(i.RID);
          //GetPressureTestDetails(wkCalibReadRID);
                     
          break;

        //IMI Initial Read
        case 6:
          lblDateIMIInitialRead.Text = i.ExposureDate.ToShortDateString();
          lblDLIMIInitialRead.Text = Convert.ToString(i.DeepLow);
          lblDLtIMIInitialRead.Text = Convert.ToString(i.DeepLowTemp);
          lblDLDoseIMIInitialRead.Text = Convert.ToString(i.DeepLowDose);
          lblDHIMIInitialRead.Text = Convert.ToString(i.DeepHigh);
          lblDHtIMIInitialRead.Text = Convert.ToString(i.DeepHighTemp);
          lblDHDoseIMIInitialRead.Text = Convert.ToString(i.DeepHighDose);
          
          break;

        //Pressure Test 1
        case 7:
          lblDatePressureRead1.Text = i.ExposureDate.ToShortDateString();
          lblDLPressureRead1.Text = Convert.ToString(i.DeepLow);
          lblDLtPressureRead1.Text = Convert.ToString(i.DeepLowTemp);
          lblDLDosePressureRead1.Text = Convert.ToString(i.DeepLowDose);
          lblDHPressureRead1.Text = Convert.ToString(i.DeepHigh);
          lblDHtPressureRead1.Text = Convert.ToString(i.DeepHighTemp);
          lblDHDosePressureRead1.Text = Convert.ToString(i.DeepHighDose);
          
          break;

        //Pressure Test 2
        case 8:
          lblDatePressureRead2.Text = i.ExposureDate.ToShortDateString();
          lblDLPressureRead2.Text = Convert.ToString(i.DeepLow);
          lblDLtPressureRead2.Text = Convert.ToString(i.DeepLowTemp);
          lblDLDosePressureRead2.Text = Convert.ToString(i.DeepLowDose);
          lblDHPressureRead2.Text = Convert.ToString(i.DeepHigh);
          lblDHtPressureRead2.Text = Convert.ToString(i.DeepHighTemp);
          lblDHDosePressureRead2.Text = Convert.ToString(i.DeepHighDose);
          
          break;

        default:
          
          break;
       
        }

      }
    }
    catch { }
  }

  protected void GetPressureTestDetails(int inDeviceID)
  {
    try
    {
      CalibPressureTestDetail ptd = (from p in idc.CalibPressureTestDetails
             where p.DeviceID == inDeviceID
             select p).First();
    
      GetPressureTestHeader(ptd.PressureTestHeaderID);

    }
    catch
    {
      
    }
        
  }

  protected void GetPressureTestHeader(int inPressureTestHeaderID)
  {
    try
    {
      CalibPressureTestHeader PTH = (from p in idc.CalibPressureTestHeaders
             where p.PressureTestHeaderID == inPressureTestHeaderID
             select p).First();

      lblIrradiation.Text = Convert.ToString(PTH.Irradiation);
      lblAtmosphere.Text = Convert.ToString(PTH.AtmospherePressure);
      lblChamberPressure.Text = Convert.ToString(PTH.ChamberPressure);
      lblChamberNo.Text = Convert.ToString(PTH.ChamberNumber);
      lblSlotNo.Text = Convert.ToString(PTH.SlotNumber);

    }
    catch
    {
    
    }

  }

  protected void GetCalImiDeviceReads(int inDeviceID)
  {
    try
    {
      CalibDeviceRead dr = (from p in idc.CalibDeviceReads
            where p.DeviceID == inDeviceID
            select p).First();
      
      lblDLcomp.Text = Convert.ToString(dr.DLComp);
      lblDHcomp.Text = Convert.ToString(dr.DHComp);

    }
    catch
    {
     
    }

  }

  protected void GetDriftAnalysis(int inDeviceID)
  {
    try
    {
      DriftAnalysi da = (from d in idc.DriftAnalysis
                         where d.DeviceID == inDeviceID
                         select d).First();

      lblDLDiff.Text = Convert.ToString(da.DeepLowDiff);
      lblZDL.Text = Convert.ToString(da.zDeepLow);
      lblDHDiff.Text = Convert.ToString(da.DeepHighDiff);
      lblZDH.Text = Convert.ToString(da.zDeepHigh);
      
    }
    catch
    {
      
    }        
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










  
