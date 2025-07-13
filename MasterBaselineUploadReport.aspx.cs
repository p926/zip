using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Instadose;
using Instadose.Data;
using System.Text.RegularExpressions;
using System.IO;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Collections;
using System.Configuration;
using System.Data.SqlClient;
using System.Net.Mail;

public partial class TechOps_MasterBaselineUploadReport : System.Web.UI.Page
{
    InsDataContext idc = new InsDataContext();
    ReportDocument cryRpt = new ReportDocument();

    // Turn on/off email to all others
    bool DevelopmentServer = false;
    string UserName = "Unknown";

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
        // Auto set if a development site
        if (Request.Url.Authority.ToString().Contains(".mirioncorp.com"))
            DevelopmentServer = true;

        this.UserName = User.Identity.Name.Split('\\')[1];

        // Grab the ID if it was passed it the query string
        if (Request.QueryString["receiptID"] == null)
            return;

        int receiptID = int.Parse(Request.QueryString["receiptID"]);
        string resultMessages = Request.QueryString["resultMessages"];

        GeneratePDF(receiptID, resultMessages, "MasterBaselineUploadReport.rpt");

    }

    private void SendEmail(int pReceiptID)
    {
        try
        {
            // Generate email content
            string bodyText = "<html><body>";

            bodyText += "<font size='-1'>An uploading of Master and Baseline files for receipt#: " + pReceiptID + " was completed.</font>";

            bodyText += "<br><br><br><font size='1'>* This email is generated from user: <b>" + this.UserName + "</b></font>";

            bodyText += "</body></html>";

            // Send  email.
            string smtpServer = string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["SmtpServer"]) ? "10.200.2.16" : System.Configuration.ConfigurationManager.AppSettings["SmtpServer"];
            SmtpClient client = new SmtpClient(smtpServer);
            MailMessage mail = new MailMessage();
            mail.IsBodyHtml = true;

            mail.From = new MailAddress("noreply@instadose.com", "TechOps Portal Email");
            mail.Subject = "M/BL Files Uploading Notification";   // will not work if using Master word in email subject.
            mail.Body = bodyText;

            // email recipients To or CC or Bcc 

            if (DevelopmentServer == false)
            {
                string userEmail = this.UserName + "@mirion.com";
                mail.To.Add(userEmail);
                mail.To.Add("khindra@mirion.com");               
                mail.CC.Add("tdo@mirion.com");
                mail.CC.Add("esantos@mirion.com");
                mail.CC.Add("gsubagyo@mirion.com");
                mail.CC.Add("njafari@mirion.com");
                mail.CC.Add("mrajab@mirion.com");
                //mail.To.Add("info@QuantumBadges.com");
                //mail.CC.Add("MWilliams@mirion.com");                
                //mail.Bcc.Add("gchu@mirion.com");
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

    protected void GeneratePDF(int pReceiptID, string pResultMessages, string pCrystalReportFileName)
    {
        string myCRFileNamePath = Server.MapPath(pCrystalReportFileName);
        MemoryStream oStream = new MemoryStream();
        try
        {
            string[] columns = pResultMessages.Split('_');

            string noReceived = columns[0]; // Number of devices received per receipt
            string noUpload = columns[1];   // Number of devices uploaded
            string noSkip = columns[2];     // Number of devices skipped because already uploaded            
            string noRFT = columns[3];      // Number of good devices uploaded, (Ready for Testing)
            string noMM = columns[4];       // Number of bad devices uploaded, (Missing Master record)
            string noMB = columns[5];       // Number of bad devices uploaded, (Missing Baseline records)
            string noBLTM = columns[6];     // Number of bad devices uploaded, (BLT malfunction)
            string noDOR = columns[7];      // Number of bad devices uploaded, (Dose Out of Range)
            string noMfgDateOutofDate = columns[8];      // Number of good devices uploaded, (but Manufacturing Date is out of date)
            //string noColorNotInSynched = columns[9];     // Number of colors not in-synched  between uploading files and Receipt Header Information   
            string colorNotInSynched = columns[9];     // Colors not in-synched  between uploading files and Receipt Header Information   


            int totalMASUploaded = 0;
            int repeatChecks = 0;
            int waitInMilliseconds = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(noUpload) / 1500) * 60000); // Assumming MAS will take 60" per 1500 device uploaded.

            // First, wait some amounts of milliseconds for all S/N uploaded to MAS successfully.
            System.Threading.Thread.Sleep(waitInMilliseconds);
            do
            {
                System.Threading.Thread.Sleep(15000); // periordically check every 15 seconds to see if uploading in MAS completely.
                totalMASUploaded = GetTotalSerialUploadedinMAS(pReceiptID);
                repeatChecks++;
            } while (totalMASUploaded <= 0 && repeatChecks < 8);   // make max 8 times loop and exit            

            //pResultMessages += totalMASUploaded + "_";            
            string noUploadedToMAS = totalMASUploaded.ToString();        // Total devices uploaded to MAS based upon provided Master file.                     

            // ------------ Send email right after sleeping and before crystal report -------------------//
            SendEmail(pReceiptID);
            // ------------------------------------------------------------------------------------------//

            cryRpt.Load(myCRFileNamePath);

            DataTable poInfoDT = GetPOReceiptInfo(pReceiptID);
            if (poInfoDT.Rows.Count > 0)
            {
                poInfoDT.Rows[0]["NoReceived"] = noReceived;
                poInfoDT.Rows[0]["NoUpload"] = noUpload;
                poInfoDT.Rows[0]["NoSkip"] = noSkip;
                poInfoDT.Rows[0]["NoRFT"] = noRFT;
                poInfoDT.Rows[0]["NoMM"] = noMM;
                poInfoDT.Rows[0]["NoMB"] = noMB;
                poInfoDT.Rows[0]["NoBLTM"] = noBLTM;
                poInfoDT.Rows[0]["NoDOR"] = noDOR;
                poInfoDT.Rows[0]["NoMfgDateOutofDate"] = noMfgDateOutofDate;
                poInfoDT.Rows[0]["NoColorNotInSynched"] = colorNotInSynched;
                poInfoDT.Rows[0]["NoUploadedToMAS"] = noUploadedToMAS;
            }

            cryRpt.SetDataSource(poInfoDT);

            //export to memory Stream            
            cryRpt.ExportToHttpResponse(ExportFormatType.PortableDocFormat, HttpContext.Current.Response, false, "master_baseline_upload");
        }
        catch (Exception e)
        {
            Response.Write(e);
        }
        finally
        {
            cryRpt.Close();
            cryRpt.Dispose();
        }
    }

    private string GetTotalReceived(int pReceiptID)
    {
        try
        {
            int sumReceived = 0;

            sumReceived = (from a in idc.POReceiptDetails
                           where a.ReceiptID == pReceiptID
                           select a.QtyRecd).Sum();

            return sumReceived.ToString();
        }
        catch
        {
            return "0";
        }
    }

    private DataTable GetPOReceiptInfo(int pReceiptID)
    {
        try
        {
            String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            String cmdStr = "dbo.sp_GetPOReceiptInfo";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.Add("@ReceiptID", SqlDbType.Int);

            sqlCmd.Parameters["@ReceiptID"].Value = pReceiptID;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);

            sqlConn.Close();

            return dt;
        }
        catch { return null; }
    }

    private int GetTotalSerialUploadedinMAS(int pReceiptID)
    {
        String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
        String cmdStr = "dbo.sp_GetAllSerialInMASByReceiptID";

        SqlConnection sqlConn = new SqlConnection(connStr);
        SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

        try
        {
            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.Add("@ReceiptID", SqlDbType.Int);

            sqlCmd.Parameters["@ReceiptID"].Value = pReceiptID;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);

            sqlConn.Close();
            sqlConn.Dispose();

            if (dt != null && dt.Rows.Count > 0)
            {
                return dt.Rows.Count;
            }
            else
            {
                return 0;
            }

        }
        catch
        {
            sqlConn.Close();
            sqlConn.Dispose();
            return 0;
        }
    }
}