using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;
using System.Net.Mail;

using Instadose;
using Instadose.Data;

using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

public partial class InstaDose_InformationFinder_EmailOrderAck2 : System.Web.UI.Page
{ 
    int AccountID = 0;
    string ResponseMessageStr = "";
    // String to hold the current username
    string UserName = "Unknown";
    ReportDocument cryRpt;

    // Create the database reference
    InsDataContext idc = new InsDataContext();
    
    protected void Page_Load(object sender, EventArgs e)
    {
        lblResponse.Text = "";
        btnSendEmail.Enabled = false;

        try
        {
            UserName = User.Identity.Name.Split('\\')[1];
        }
        catch { UserName = "Unknown"; }

        try
        {
            if (!IsPostBack && Request.QueryString["ID"] != null )
            {
                this.txtAccountID.Text = Request.QueryString["ID"];
                int.TryParse(this.txtAccountID.Text, out AccountID);

                var AcctInfo = (from a in idc.Accounts
                                join u in idc.Users
                                on a.AccountAdminID equals u.UserID
                                where a.AccountID == AccountID
                                select   u.Email  ).First();

                this.txtEmail.Text = AcctInfo;
                btnSendEmail.Enabled = true;
                btnCloseWindow.Visible = true;
                //btnSendEmail_Click(sender, e);
            }
        }
        catch (Exception ex)
        {
            ResponseMessageStr = "<br>" + ex.ToString();
        }

        
        lblResponse.Text = ResponseMessageStr;
    }


    protected void txtAccountID_OnTextChanged(object sender, EventArgs e)
    {
        int.TryParse(this.txtAccountID.Text, out AccountID);

        try 
        {
            var AcctEmail = (from a in idc.Accounts
                             join u in idc.Users
                             on a.AccountAdminID equals u.UserID
                             where a.AccountID == AccountID
                             select u.Email).First();

            this.txtEmail.Text = AcctEmail;
            btnSendEmail.Enabled = true;
        }
        catch
        {}
        
    }

    /// <summary>
    /// btnSendEmail_Click
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnSendEmail_Click(object sender, EventArgs e)
    {
       
        string EmailAddress = this.txtEmail.Text.Trim();
        int.TryParse(this.txtAccountID.Text, out AccountID);
       
        if (AccountID != 0 && EmailAddress!="")
        {
            ResponseMessageStr += "<br />Account ID:" + AccountID.ToString() 
                    + " Email: " + EmailAddress.ToString();
            try
            {
                // Grab the originial order 
                var order = (from o in idc.Orders
                             where o.AccountID == AccountID
                             orderby o.OrderID
                             select o).First();

                int originalOrderID = order.OrderID;

                //Generate OrderAck and save in database
                string AckOrdFileName = GenerateOrderAcknowledgement(AccountID, originalOrderID);
               
                try
                {
                    // Grab Account Admin First and Last name
                    var AdminInfo = (from u in idc.Users
                                     join a in idc.Accounts
                                     on u.UserID equals a.AccountAdminID
                                     where a.AccountID == AccountID && a.AccountAdminID != null
                                     select new
                                     {
                                         AdminName =
                                           ((u.FirstName != null) ? u.FirstName.ToString() : "")
                                           + " " +
                                           ((u.LastName != null) ? u.LastName.ToString() : "")
                                     }).ToArray();

                    string AdminName = (AdminInfo.Length == 0) ? "Administrator" : AdminInfo[0].AdminName.ToString();
                    bool SendEmailResponse = EmailOrderAcknowledgement(AdminName, EmailAddress, 
                            AccountID, order.OrderID, AckOrdFileName);
                    
                    if (SendEmailResponse == true)
                    {
                        ResponseMessageStr += "<br /> Email sent!";
                        // insert a Note to indicate email has been sent.
                        AccountNote AcctNote = null;
                        AcctNote = new AccountNote()
                        {
                            AccountID = AccountID,
                            CreatedDate = DateTime.Now,
                            CreatedBy = UserName,
                            NoteText = "Email Order Acknowledgement to " + EmailAddress,
                            Active = true
                        };
                        idc.AccountNotes.InsertOnSubmit(AcctNote);
                        idc.SubmitChanges();
                        ResponseMessageStr += "<br /> Note Added!";
                    }
                    else
                        ResponseMessageStr += "<br /> Email failed!";
                    
                }
                catch (Exception ex)
                {
                    //ResponseMessageStr += "<br />" + ex.ToString();
                    ResponseMessageStr += "<br /><font color='red'>Email failed, Check email address!</font>";
                }
               
            }
            catch (Exception ex)
            {
                ResponseMessageStr += "<br />Account Not found!";// +ex.ToString();
            }


            this.btnSendEmail.Enabled = false;
            this.txtAccountID.Text = "";
            this.txtEmail.Text = "";

            lblResponse.Text = ResponseMessageStr;
        }
    }

    /// <summary>
    /// GenerateOrderAcknowledgement
    ///     Check Document table.  Create and save original order acknowledgment document
    ///     if it does not exist in Document table.    
    /// </summary>
    /// <param name="accountID"></param>
    /// <param name="ordID"></param>
    /// <returns></returns>
    protected string GenerateOrderAcknowledgement(int accountID, int originalOrderID)
    {
        ReportDocument cryRpt;

        string[] rptParamName = new string[1];
        string[] rptParamVals = new string[1];
        string pdfFileName = "Ack_" + originalOrderID.ToString() + ".pdf";
        string crFileNamePath = Server.MapPath("../GenerateOrderAck.rpt");

        // Check to see if order acknowledgement document data exists for this order.  
        Document ordAck = (from d in idc.Documents
                           where d.AccountID == accountID && d.OrderID == originalOrderID
                                && d.Description == "Original Order Acknowledgement"
                                && d.FileName == pdfFileName   //.MIMEType == "application/pdf"
                           select d).FirstOrDefault();

        if (ordAck == null)
        {
            //No order acknowledgement exists for this order ID and account ID.
            //Create it and store in Document table.

            cryRpt = new ReportDocument();
            cryRpt.Load(crFileNamePath);

            cryRpt.SetParameterValue(0, originalOrderID);

            string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString.ToString();
            string myDatabase = Regex.Match(ConnectionString, "(Initial Catalog=|Database=){1}([A-Za-z0-9_]+)[;]?").Groups[2].ToString();
            string myServer = Regex.Match(ConnectionString, "(Data Source=){1}([\\w\\d\\.\\$\\-]+)[;]?").Groups[2].ToString();
            string myUserID = Regex.Match(ConnectionString, "(User ID=|UID=){1}([\\w\\d\\.]+)[;]?").Groups[2].ToString();
            string myPassword = Regex.Match(ConnectionString, "(Pwd=|Password=|PWD=){1}([\\w\\d\\.\\$]+)[;]?").Groups[2].ToString();

            cryRpt.DataSourceConnections[0].SetConnection(myServer, myDatabase, myUserID, myPassword);

            //Create PDF file and store in Memory
            Stream oStream = null;
            oStream = cryRpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            byte[] streamArray = new byte[oStream.Length];
            oStream.Read(streamArray, 0, Convert.ToInt32(oStream.Length - 1));
            // insert record to database
            try
            {
                // 8/2012 - New Documents table for all attachments for email, etc.
                // insert record to document table.
                Document doc = new Document()
                {
                    Active = true,
                    AccountID = accountID,
                    OrderID = originalOrderID,
                    CreatedBy = this.UserName,
                    CreatedDate = DateTime.Now,
                    Description = "Original Order Acknowledgement",
                    DocumentCategory = "Order Acknowledgement",
                    DocumentGUID = Guid.NewGuid(),
                    FileName = pdfFileName,
                    DocumentContent = streamArray,
                    MIMEType = "application/pdf",
                };

                // Get the current website environment
                string siteUrl = System.Configuration.ConfigurationManager.AppSettings["api_webaddress"];

                // Construct the download path for the user.
                string downloadUrl = string.Format("{0}Support/PublicDocument.aspx?GUID={1}&Ticks={2}",
                    siteUrl, doc.DocumentGUID, doc.CreatedDate.Ticks);

                // Upload the document
                idc.Documents.InsertOnSubmit(doc);
                idc.SubmitChanges();

            }
            catch (Exception ex)
            {
                // Report the error to the message system.
                Basics.WriteLogEntry(ex.Message, Page.User.Identity.Name,
                    "Portal.InstaDose_InformationFinder_EmailOrderAck", Basics.MessageLogType.Critical);

                Response.Write(ex);
            }

        }
        return pdfFileName;
    }

    /// <summary>
    /// EmailOrderAcknowledgement
    ///         Email original Order Acknowledgement to customer.
    /// </summary>
    /// <param name="AdminFLName"></param>
    /// <param name="EmailAddress"></param>
    /// <param name="AccountID"></param>
    /// <param name="OriginalOrderID"></param>
    /// <param name="AckFileName"></param>
    /// <returns></returns>
    protected bool EmailOrderAcknowledgement(string AdminFLName, string EmailAddress, 
            int AccountID, int orderID, string AckFileName)
    {
        bool RtnResponse = false;

        //**********************************************************************************
        // 8/2012 - New email code for Instadose 2 using new Document table
        //      and Instadose.Message class.
        //**********************************************************************************

        // Retrieve original email acknowledgement from Document table. 
        Document doc = (from d in idc.Documents
                        where d.AccountID == AccountID
                            && d.Description == "Original Order Acknowledgement" 
                            && d.OrderID == orderID //&& d.MIMEType == "application/pdf"
                        select d).FirstOrDefault();

        // Get the current website environment
        string siteUrl = System.Configuration.ConfigurationManager.AppSettings["api_webaddress"];

        // Construct the download path for the user.  GUID+Ticks link via Email.
        string downloadUrl = string.Format("{0}Support/PublicDocument.aspx?GUID={1}&Ticks={2}",
            siteUrl, doc.DocumentGUID, doc.CreatedDate.Ticks);

        // Stop if noting found 
        if (doc == null)
        {
            RtnResponse = false;
        }
        else
        {
            //// Create the template.
            MessageSystem msgSystem = new MessageSystem()
            {
                Application = "Instadose.com Order Acknowledgement",
                CreatedBy = this.UserName,
                FromAddress = "noreply@instadose.com",
                ToAddressList = new List<string>()
            };

            // Add the email
            msgSystem.ToAddressList.Add(EmailAddress);

            // Replace the fields in the template
            Dictionary<string, string> fields = new Dictionary<string, string>();

            // Add the fields to replace.
            // MUST BE EXACT with email template stored EmailTemplates table.  Check
            // description for fields that need to have values for your document.

            fields.Add("OrderID", orderID.ToString());
            fields.Add("FullName", AdminFLName);
            fields.Add("DownloadUrl", downloadUrl);

            // Send using the order acknowledgement template, with no brand. 
            // MUST BE EXACT with EmailTemplate (TemplateName)! 
            int response = msgSystem.Send("Order Acknowledgement", "", fields);

            if (response != 0) RtnResponse = true; //success!

        }

        return RtnResponse;
    }
    
}