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
using ReadWriteCsv;
using Mirion.DSD.Consumer;

public partial class TechOps_RequestTransferQAToWIP : System.Web.UI.Page
{
    // Turn on/off email to all others
    bool DevelopmentServer = false;

    string PO_ReceiptDDFileDirPath = @"\\irv-sapl-file1.mirion.local\POReceipt$\POReceiptStorage\";
    InsDataContext idc = new InsDataContext();
    DataTable myGridViewDT;
    DataTable myReportDT;
    string UserName = "Unknown";
    ReportDocument cryRpt = new ReportDocument();

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
            // Auto set if a development site
            if (Request.Url.Authority.ToString().Contains(".mirioncorp.com"))
                DevelopmentServer = true;

            InvisibleErrors();
            InvisibleMessages();
            this.UserName = User.Identity.Name.Split('\\')[1];

            if (!IsPostBack)
            {
                btnFind_Click(null, null);
            }

            if (gvQADevice.Rows.Count > 0)
                this.btnSubmit.Enabled = true;
            else
                this.btnSubmit.Enabled = false;

            // Clear the cache of this page
            //Response.Cache.SetCacheability(HttpCacheability.NoCache);
            //Response.Cache.SetExpires(DateTime.Now.AddSeconds(-1));
            //Response.Cache.SetNoStore();

        }
        catch (Exception ex)
        { Response.Write(ex); }
    }

    private void DefineGridDataTable()
    {
        myGridViewDT = new DataTable();
        myGridViewDT.Columns.Add("ReceiptID", typeof(string));
        myGridViewDT.Columns.Add("ReceiptDate", typeof(DateTime));
        myGridViewDT.Columns.Add("ShipmentID", typeof(string));

        myGridViewDT.Columns.Add("Vendor", typeof(string));
        myGridViewDT.Columns.Add("PONumber", typeof(string));
        myGridViewDT.Columns.Add("ItemNumber", typeof(string));
        myGridViewDT.Columns.Add("ReadyShip", typeof(string));
    }

    private void DefineReportDataTable()
    {
        myReportDT = new DataTable();
        myReportDT.Columns.Add("ReceiptID", typeof(string));
        myReportDT.Columns.Add("ReceiptDate", typeof(string));
        myReportDT.Columns.Add("ShipmentID", typeof(string));
        myReportDT.Columns.Add("Vendor", typeof(string));
        myReportDT.Columns.Add("PONumber", typeof(string));
        myReportDT.Columns.Add("ItemNumber", typeof(string));
        myReportDT.Columns.Add("RequestedQty", typeof(int));
        myReportDT.Columns.Add("AvailableQty", typeof(int));
        myReportDT.Columns.Add("RequestedTotal", typeof(int));
    }

    private void LoadGrid()
    {
        DataTable myDT = new DataTable();
        DataRow dtr;

        myDT = GetAllAvailableDeviceInQA(this.txtReceiptID.Text.Trim(), this.txtReceiptDate.Text.Trim());

        if (myDT != null && myDT.Rows.Count > 0)
        {
            this.gvQADevice.DataSource = myDT;
        }
        else
        {
            this.gvQADevice.DataSource = null;
        }

        this.gvQADevice.DataBind();

        DisplayAvailabilitySummary(myDT);
    }

    private void DisplayAvailabilitySummary(DataTable pSearchResult)
    {
        if (pSearchResult != null && pSearchResult.Rows.Count > 0)
        {
            string summaryMsg = "";
            summaryMsg = GenerateSummaryMsg(pSearchResult);
            this.Summary.InnerHtml = summaryMsg;
        }
        else
        {
            this.Summary.InnerHtml = "N/A";
        }
    }

    private string GenerateSummaryMsg(DataTable pSearchResult)
    {
        Hashtable Item_Qty_PairHash = new Hashtable();
        string curItemNumber = "";
        int curQtyAvail;
        int prevQtyAvail;

        foreach (DataRow row in pSearchResult.Rows)
        {
            curItemNumber = (string)row["ItemNumber"];
            curQtyAvail = (int)row["Remaining"];

            if (Item_Qty_PairHash.ContainsKey(curItemNumber))
            {
                prevQtyAvail = (int)Item_Qty_PairHash[curItemNumber];
                Item_Qty_PairHash[curItemNumber] = prevQtyAvail + curQtyAvail;
            }
            else
            {
                Item_Qty_PairHash.Add(curItemNumber, curQtyAvail);
            }
        }

        // Go through each pair to generate an available devices summary
        string summaryStr = "<ul type='circle'>";

        foreach (string key in Item_Qty_PairHash.Keys)
        {
            int value = (int)Item_Qty_PairHash[key];

            summaryStr += "<li>" + key + " : " + value + "</li>";
        }

        summaryStr += "</ul>";

        return summaryStr;
    }

    private bool PassValidation()
    {
        int availInQA;
        int total;
        int receiptID;
        string requestItemNumber;

        foreach (GridViewRow rowitem in gvQADevice.Rows)
        {
            TextBox RequestTotal = (TextBox)rowitem.FindControl("txtRequestTotal");

            if (!int.TryParse(RequestTotal.Text.Trim(), out total))
            {
                total = 0;
            }

            receiptID = int.Parse(rowitem.Cells[0].Text);
            availInQA = int.Parse(rowitem.Cells[7].Text);
            requestItemNumber = rowitem.Cells[5].Text;

            if (total > availInQA)
            {
                // Check request total for receiptID and Item#
                this.VisibleErrors("Request Total can not be greater than Available in QA for receiptID: " + receiptID + " and Item#: " + requestItemNumber);
                return false;
            }

            if (total > 0 && requestItemNumber == "N/A")
            {
                //Check upload file for receiptID, Item#
                this.VisibleErrors("Item# can not be N/A for receiptID: " + receiptID + ". Check the upload file in POReceiptStorage.");
                return false;
            }

        }

        return true;
    }

    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        int total = 0;
        int receiptID = 0;
        string requestItemNumber;
        int insertCount = 0;

        try
        {
            if (PassValidation())
            {
                DefineReportDataTable();

                foreach (GridViewRow rowitem in gvQADevice.Rows)
                {
                    TextBox RequestTotal = (TextBox)rowitem.FindControl("txtRequestTotal");

                    if (!int.TryParse(RequestTotal.Text.Trim(), out total))
                    {
                        total = 0;
                    }

                    receiptID = int.Parse(rowitem.Cells[0].Text);
                    requestItemNumber = rowitem.Cells[5].Text;

                    if (total > 0 && requestItemNumber.Length > 0 && requestItemNumber != "N/A")
                    {
                        // Commit dbo.QARequest record
                        QARequest rq = new QARequest
                        {
                            ReceiptID = receiptID,
                            RequestItemNumber = requestItemNumber,
                            RequestTotal = total,
                            RequestDate = DateTime.Now,
                            RequestBy = this.UserName
                        };

                        idc.QARequests.InsertOnSubmit(rq);
                        idc.SubmitChanges();
                        insertCount++;

                        // Insert record into data table for report
                        InsertReportDataTable(rowitem);
                    }
                }

                if (insertCount > 0)
                {
                    SendEmail();
                    //this.btnFind_Click(null, null);
                    //this.VisibleMessages("Submit successfully.");
                    GeneratePDF(myReportDT);
                }

            }
        }
        catch (Exception ex)
        {
            this.VisibleErrors(ex.ToString());
        }

    }

    private void InsertReportDataTable(GridViewRow pRowitem)
    {
        DataRow dtr;
        string receiptDate, shipmentID, vendor, poNumber, itemNumber, receiptID;
        int myAvaiBefore, myAvaiAfter, myRequestTotal, requestedQty, availableQty;

        myAvaiBefore = int.Parse(pRowitem.Cells[7].Text.Trim());

        TextBox RequestTotal = (TextBox)pRowitem.FindControl("txtRequestTotal");
        myRequestTotal = int.Parse(RequestTotal.Text.Trim());

        myAvaiAfter = myAvaiBefore - myRequestTotal;


        receiptID = pRowitem.Cells[0].Text.Trim();
        receiptDate = pRowitem.Cells[1].Text.Trim();
        shipmentID = pRowitem.Cells[2].Text.Trim();
        vendor = pRowitem.Cells[3].Text.Trim();
        poNumber = pRowitem.Cells[4].Text.Trim();
        itemNumber = pRowitem.Cells[5].Text.Trim();
        requestedQty = myRequestTotal;
        availableQty = myAvaiAfter;

        dtr = myReportDT.NewRow();

        dtr["ReceiptID"] = receiptID;
        dtr["ReceiptDate"] = receiptDate;
        dtr["ShipmentID"] = shipmentID;
        dtr["Vendor"] = vendor;
        dtr["PONumber"] = poNumber;
        dtr["ItemNumber"] = itemNumber;
        dtr["RequestedQty"] = requestedQty;
        dtr["AvailableQty"] = availableQty;
        dtr["RequestedTotal"] = 0;

        // Add the row to the DataTable.
        myReportDT.Rows.Add(dtr);
    }

    private int GetAvailInQAByItemNumber(string pReceiptID, string pItemNumber, DataTable pCopyDT)
    {
        int count = 0;
        try
        {
            foreach (DataRow row in pCopyDT.Rows)
            {
                if (row["ReceiptID"].ToString().Trim() == pReceiptID && row["ItemNumber"].ToString().Trim() == pItemNumber)
                    count++;
            }

            int totalPending = TotalPendingInRequestQueue(pReceiptID, pItemNumber);
            return count - totalPending;
        }
        catch
        {
            return 0;
        }
    }

    private int TotalPendingInRequestQueue(string pReceiptID, string pItemNumber)
    {
        int pendingTotal = 0;

        try
        {
            var qaRequestList = (from rq in idc.QARequests
                                 where rq.ReceiptID == int.Parse(pReceiptID)
                                     && rq.RequestItemNumber == pItemNumber
                                 select rq).ToList();

            if (qaRequestList != null && qaRequestList.Count > 0)
            {
                foreach (var v in qaRequestList)
                {
                    pendingTotal += v.RequestTotal;
                }
            }

            return pendingTotal;
        }
        catch
        {
            return pendingTotal;
        }
    }

    private string GetItemNumber(string pReceiptID, string pSerialNo)
    {
        string PO_ReceiptDDFilePath = PO_ReceiptDDFileDirPath + "PO_ReceiptDD_" + pReceiptID + ".csv";

        try
        {
            if (File.Exists(PO_ReceiptDDFilePath))
            {
                // Read sample data from CSV file
                using (CsvFileReader reader = new CsvFileReader(PO_ReceiptDDFilePath))
                {
                    CsvRow row = new CsvRow();
                    while (reader.ReadRow(row))
                    {
                        if (row[3] == pSerialNo)
                            return row[2];
                    }
                    return "N/A";
                }
            }
            else
            {
                return "N/A";
            }
        }
        catch
        {
            return "N/A";
        }

    }

    private DataTable GetAllAvailableDeviceInQA(string pReceiptID, string pReceiptDate)
    {
        try
        {
            String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            String cmdStr = "dbo.sp_GetAllAvailDeviceInQA";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;

            sqlCmd.Parameters.Add("@ReceiptID", SqlDbType.NVarChar, 10);
            sqlCmd.Parameters["@ReceiptID"].Value = pReceiptID;

            sqlCmd.Parameters.Add("@ReceiptDate", SqlDbType.NVarChar, 10);
            sqlCmd.Parameters["@ReceiptDate"].Value = pReceiptDate;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);

            sqlConn.Close();

            return dt;
        }
        catch { return null; }
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

    protected void btnFind_Click(object sender, EventArgs e)
    {
        //DefineGridDataTable();
        LoadGrid();
        if (gvQADevice.Rows.Count > 0)
            this.btnSubmit.Enabled = true;
        else
            this.btnSubmit.Enabled = false;
    }

    protected void GeneratePDF(DataTable pReportDT)
    {
        string myCRFileNamePath = Server.MapPath("RequestInstadoseTransferReport.rpt");

        MemoryStream oStream = new MemoryStream();
        try
        {
            cryRpt.Load(myCRFileNamePath);

            FillRequestedTotal(pReportDT);

            cryRpt.SetDataSource(pReportDT);

            //export to memory Stream            
            cryRpt.ExportToHttpResponse(ExportFormatType.PortableDocFormat, HttpContext.Current.Response, false, "request_transfer_qa_to_wip");
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

    private void SendEmail()
    {
        try
        {
            // Generate email content
            string bodyText = "<html><body>";

            bodyText += "<font size='-1'>A request of transfering Instadose from QA to WIP was made.</font>";

            bodyText += "<br><br><br><font size='1'>* This email is generated from user: <b>" + this.UserName + "</b></font>";

            bodyText += "</body></html>";

            // Send  email.
            string smtpServer = string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["SmtpServer"]) ? "10.200.2.16" : System.Configuration.ConfigurationManager.AppSettings["SmtpServer"];
            SmtpClient client = new SmtpClient(smtpServer);
            MailMessage mail = new MailMessage();
            mail.IsBodyHtml = true;

            mail.From = new MailAddress("noreply@instadose.com", "TechOps Portal Email");
            mail.Subject = "Request of Transfering Notification";
            mail.Body = bodyText;

            // email recipients To or CC or Bcc 

            if (DevelopmentServer == false)
            {
                mail.To.Add("rtee@mirion.com");                            
                mail.CC.Add("khindra@mirion.com");
                mail.CC.Add("gsturm@mirion.com");
            }
            else
            {
                string userEmail = this.UserName + "@mirion.com";
                mail.To.Add(userEmail);
            }

            client.Send(mail);

        }
        catch { }
    }

}