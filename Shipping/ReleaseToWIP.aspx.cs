using System;
using System.IO;
using System.Text.RegularExpressions;
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
using System.Collections;
using System.Diagnostics;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

using Instadose;
using Instadose.Data;

public partial class Shipping_ReleaseToWIP : System.Web.UI.Page
{
    InsDataContext idc = new InsDataContext();
    string UserName = "Unknown";
    ReportDocument cryRpt = new ReportDocument();
    DataTable myReportDT;

    protected void Page_Unload(object sender, EventArgs e)
    {
        try
        {
            cryRpt.Close();
            cryRpt.Dispose();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        catch { }
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            this.UserName = User.Identity.Name.Split('\\')[1];
            InvisibleErrors();
            InvisibleMessages();
        }
        catch
        {
            UserName = "Unknown";
        }        
    }

    private void DefineReportDataTable()
    {
        myReportDT = new DataTable();
        myReportDT.Columns.Add("ReceiptID", typeof(string));        
        myReportDT.Columns.Add("ItemNumber", typeof(string));
        myReportDT.Columns.Add("RequestedQty", typeof(int));
        myReportDT.Columns.Add("RequestBy", typeof(string));
        myReportDT.Columns.Add("RequestDate", typeof(string));
        myReportDT.Columns.Add("RequestedTotal", typeof(int));
    }

    private bool IsRequestAlreadyReleased(int pQARequestID)
    {        
        QARequest myQARequest =
          (from rq in idc.QARequests
           where rq.QARequestID == pQARequestID
           && rq.ReleaseBy != null && rq.ReleaseDate != null
           select rq).FirstOrDefault();

        if (myQARequest != null)
            return true;
        else
            return false;
    }

    protected void btnRelease_Click(object sender, EventArgs e)
    {
        int insertCount = 0;
        int curQARequestID = 0;

        try
        {
            DefineReportDataTable();

            foreach (GridViewRow rowitem in gvQADevice.Rows)
            {
                CheckBox chk = (CheckBox)rowitem.FindControl("cbRow");

                if (chk.Checked)
                {                   
                    curQARequestID = int.Parse(((HiddenField)rowitem.FindControl("HidQARequestID")).Value);

                    if (! IsRequestAlreadyReleased(curQARequestID))
                    {
                        // Save QARequest
                        QARequest myQARequest =
                          (from rq in idc.QARequests
                           where rq.QARequestID == curQARequestID
                           select rq).First();

                        myQARequest.ReleaseBy = UserName;
                        myQARequest.ReleaseDate = DateTime.Now;

                        idc.SubmitChanges();

                        insertCount++;

                        // Insert record into data table for report
                        InsertReportDataTable(rowitem);
                    }                    
                }               
            }

            if (insertCount > 0)
            {
                gvQADevice.DataBind();
                this.VisibleMessages("Release done.");
                GeneratePDF(myReportDT);   
            }
        }
        catch (Exception ex)
        {
            this.VisibleErrors(ex.ToString());
        }        
    }

    protected void gvQADevice_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            CheckBox chk = (CheckBox)e.Row.FindControl("cbRow");
            string releaseBy = e.Row.Cells[6].Text.Replace("&nbsp;", "").ToString().Trim();
            
            //DataTable dtItemNumbers = GetAllItemNumberByPONumber(this.ddlPONumber.SelectedItem.Value);
            if (string.IsNullOrEmpty(releaseBy))
                chk.Visible  = true;
            else
                chk.Visible  = false;            
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

    private void InvisibleMessages()
    {
        // Reset submission form error message      
        this.submitMsg.InnerText = "";
        this.messages.Visible = false;
    }

    private void VisibleMessages(string error)
    {
        this.submitMsg.InnerText = error;
        this.messages.Visible = true;
    }

    private void InsertReportDataTable(GridViewRow pRowitem)
    {
        DataRow dtr;
        string receiptID, itemNumber, requestBy, requestDate;  
        int  requestedQty;
        
        receiptID = pRowitem.Cells[1].Text.Trim();
        itemNumber = pRowitem.Cells[2].Text.Trim();
        requestedQty = int.Parse (pRowitem.Cells[3].Text.Trim());
        requestBy = pRowitem.Cells[4].Text.Trim();
        requestDate = pRowitem.Cells[5].Text.Trim();
        
        dtr = myReportDT.NewRow();        

        dtr["ReceiptID"] = receiptID;
        dtr["ItemNumber"] = itemNumber;
        dtr["RequestedQty"] = requestedQty;
        dtr["RequestBy"] = requestBy;
        dtr["RequestDate"] = requestDate;
        dtr["RequestedTotal"] = 0; 

        // Add the row to the DataTable.
        myReportDT.Rows.Add(dtr);
    }

    protected void GeneratePDF(DataTable pReportDT)
    {
        string myCRFileNamePath = Server.MapPath("ReleaseToWIPReport.rpt");

        Stream oStream = null;
        try
        {
            cryRpt.Load(myCRFileNamePath);

            FillRequestedTotal(pReportDT);

            cryRpt.SetDataSource(pReportDT);

            //export to memory Stream            
            cryRpt.ExportToHttpResponse(ExportFormatType.PortableDocFormat, HttpContext.Current.Response, false, "release_to_wip");
        }
        catch (Exception ex)
        {
            Response.Write(ex);
        }
        finally
        { 
            cryRpt.Close();
            cryRpt.Dispose();
        }

    }

    private void FillRequestedTotal(DataTable pReportDT)
    {
        try
        {
            int total = 0;
            foreach (DataRow row in pReportDT.Rows)
            {
                total += (int)row["RequestedQty"];
            }

            foreach (DataRow row in pReportDT.Rows)
            {
                row["RequestedTotal"] = total;
            }
        }
        catch
        { }
    }

    protected void btnRefresh_Click(object sender, EventArgs e)
    {
        gvQADevice.DataBind();
    }
}