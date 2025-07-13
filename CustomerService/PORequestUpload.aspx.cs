using System;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Data.SqlClient;

using Instadose;
using Instadose.Data;
using Instadose.Processing;
using Instadose.Integration;
using System.IO;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;


public partial class Instadose_CustomerService_PORequestUpload : System.Web.UI.Page
{
    int OrderID = 0;
    int AccountID = 0;

    // Create the database reference
    InsDataContext idc = new InsDataContext();
    MASDataContext mdc = new MASDataContext();
    
    // String to hold the current username
    string UserName = "Unknown";
 

    protected void Page_Load(object sender, EventArgs e)
    {
        Session["Brand"] = "0";
        // Try to set the username
        try
        {
            this.UserName = User.Identity.Name.Split('\\')[1];
        }
        catch { this.UserName = "Unknown"; }

        if (!IsPostBack)
        {
            // Good OrderID?
            if (Request.QueryString["ID"] == null)
                return;

            // Grab the UploadID if it was passed it the query string
            if (Request.QueryString["ID"] != null)
                int.TryParse(Request.QueryString["ID"], out OrderID);

            //No OrderID or not valid order for PO Doc upload - return!
            if ((OrderID == 0) ||
                (!this.checkPORequestUploadOrder(OrderID)))
            {
                Response.Redirect("OrderQueue.aspx");
            }

            lblOLUploadOrderID.Text = OrderID.ToString();

            // Get AccountID for order. 
            Order ord = (from o in idc.Orders
                         where o.OrderID == OrderID
                         select o).FirstOrDefault();            
            
            if (ord.AccountID != null) 
                this.AccountID = ord.AccountID;

            bindPORequestGrid(OrderID);
        }

    }

    /// <summary>
    /// BindRenewalReviewGrid
    /// </summary>
    /// <param name="RenewalNo"></param>
    private void bindPORequestGrid(int orderID)
    {
        gvPORequest.DataSource = from d in idc.Documents
                           where d.OrderID == orderID &&
                             d.Active == true &&
                             d.DocumentCategory == "PO Request"
                           orderby d.CreatedDate
                           select new
                           {
                               d.DocumentID,
                               d.AccountID,
                               d.FileName,
                               d.OrderID,
                               d.CreatedBy,
                               d.CreatedDate
                           };

        gvPORequest.DataBind();

    }

    protected void btnClose_Click(object sender, EventArgs e)
    {
        Response.Redirect("OrderQueue.aspx");
    }

    //------------------------- Start Non-Modal Upload PO File(s) Section Functions here ------------------------- //

    private bool checkPORequestUploadOrder(int OrderID)
    {
        bool validPOUploadOrder = false;

        // Retrieve data from DB 
        Document doc = (from d in idc.Documents
                        where d.OrderID == OrderID 
                        && d.Description == "Original Order PO Request Form" 
                        select d).FirstOrDefault();

        if (doc != null)
        {
            this.lblOLUploadOrderID.Text = doc.OrderID.ToString();
            btnPOUpload.CommandArgument = OrderID.ToString();
            validPOUploadOrder = true;      
        }

        return validPOUploadOrder;
    }

    protected void btnPOUpload_Click(object sender, EventArgs e)
    {
        Button btn = (Button)sender;
        string btnCommandName = btn.CommandName.ToString();

        int orderID = 0;
        if (!int.TryParse(btn.CommandArgument.ToString(), out orderID)) return;
   
        lblOLUploadOrderID.Text = orderID.ToString();

        if (orderID == 0) return;

        int accountID = 0;

        // Get AccountID for order. 
        Order ord = (from o in idc.Orders
                     where o.OrderID == orderID
                     select o).FirstOrDefault();

        if (ord != null)
            accountID = ord.AccountID;

        bool? file1Success = null;
        bool? file2Success = null;

        Response.Write("accountID" + accountID);

        // Upload file 1 if it exists.
        if (attachmentUpload1.HasFile)
        {
            lblOlUploadResult.Text = "Upload file1 begin";

            file1Success = this.commitUploadFile(attachmentUpload1.PostedFile, 
                    orderID, accountID);

            lblOlUploadResult.Text = "Upload file1 end";
        }
        else
        {
            lblOlUploadResult.Text = "Upload file1 failed";
        }

        // Upload file 2 if it exists.
        if (attachmentUpload2.HasFile)
        {
            lblOlUploadResult.Text = "Upload file2 begin";
            file2Success = this.commitUploadFile(attachmentUpload2.PostedFile, orderID, accountID);
            lblOlUploadResult.Text = "Upload file2 end";
        }
        else
        {
            lblOlUploadResult.Text = "Upload file1 failed";
        }

        // Return results to the user.
        string results = "";
        if (file1Success.HasValue) results += "<br />Attachment 1: " + ((file1Success.Value) ? "Success" : "Failed");
        if (file2Success.HasValue) results += "<br />Attachment 2: " + ((file2Success.Value) ? "Success" : "Failed");

        // Output no upload response.
        if (results == "") results = "Nothing uploaded.";

        // Output the results.
        lblOlUploadResult.Text = "Upload results: " + results;
    }

    /// <summary>
    /// Display Attachment, accept DOC/DOCX, XLS/XLSX, PDF or JPG formats.
    /// </summary>
    /// <param name="ID">DocumentID</param>
    protected void lnkBtnView_Click(object sender, EventArgs e)
    {
        LinkButton btn = (LinkButton)sender;
        string btnCommandName = btn.CommandName.ToString();

        int ID = 0;
        if (!int.TryParse(btn.CommandArgument.ToString(), out ID)) return;

        if ((ID.ToString() != "") && (ID != 0))
        {

            // Retrieve data from DB 
            Document rsAttach = (from d in idc.Documents
                                 where d.DocumentID == ID
                                 select d).FirstOrDefault();

            if (rsAttach == null)
                throw new Exception("The document was not found. Please contact IT.");

            string mimeType = rsAttach.MIMEType;

            string strFilename = rsAttach.FileName.ToString();
            Byte[] bytFile = (System.Byte[])rsAttach.DocumentContent.ToArray();

            Response.ContentType = mimeType;

            switch (mimeType)
            {
                case "application/vnd.ms-word":
                    Response.ContentType = "application/vnd.ms-word";
                    Response.AddHeader("content-disposition", "attachment;filename=" + strFilename);
                    break;
                case "application/vnd.ms-excel":
                    Response.ContentType = "application/vnd.ms-excel";
                    Response.AddHeader("content-disposition", "attachment;filename=" + strFilename);
                    break;
                case "application/pdf":
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("content-disposition", "attachment;filename=" + strFilename);
                    break;
                default:
                    Response.ContentType = "image/jpeg";
                    break;
            }

            // display Attachment file
            Response.Charset = "";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);

            Response.Expires = 0;
            Response.Buffer = true;
            Response.Clear();
            Response.BinaryWrite(bytFile);
            Response.End();
        }

    }
    
    //******************************************************************************************
    //8/2012 - New Upload to Documents table.
    //******************************************************************************************
    private bool commitUploadFile(HttpPostedFile file, int orderID, int accountID)
    {
        try
        {
            //create byte array
            Byte[] bytimage = new Byte[file.ContentLength];

            //read upload file in byte array
            file.InputStream.Read(bytimage, 0, file.ContentLength);

            string strExtension = Path.GetFileName(file.FileName).Split('.')[1];
            string mimeType = null;

            switch (strExtension.ToUpper())
            {
                case "DOC":
                    mimeType = "application/vnd.ms-word";
                    break;
                case "DOCX":
                    mimeType = "application/vnd.ms-word";
                    break;
                case "XLS":
                    mimeType = "application/vnd.ms-excel";
                    break;
                case "XLSX":
                    mimeType = "application/vnd.ms-excel";
                    break;
                case "PDF":
                    mimeType = "application/pdf";
                    break;
                default:
                    mimeType = "image/jpeg";
                    break;
            }

            Document doc = new Document()
            {
                Active = true,
                AccountID = accountID,
                OrderID = orderID,
                CreatedBy = this.UserName,
                CreatedDate = DateTime.Now,
                Description = "PO Request Document Upload",
                DocumentCategory = "PO Request",
                DocumentGUID = Guid.NewGuid(),
                FileName = Path.GetFileName(file.FileName),
                DocumentContent = bytimage,
                MIMEType = mimeType,
            };

            // Get the current website environment
            string siteUrl = System.Configuration.ConfigurationManager.AppSettings["api_webaddress"];

            // Construct the download path for the user.
            string downloadUrl = string.Format("{0}Support/PublicDocument.aspx?GUID={1}&Ticks={2}",
                siteUrl, doc.DocumentGUID, doc.CreatedDate.Ticks);

            // Upload the document
            idc.Documents.InsertOnSubmit(doc);
            idc.SubmitChanges();
      
            return true;
        }
        catch (Exception ex)
        {
            // Report the error to the message system.
            Basics.WriteLogEntry(ex.Message, Page.User.Identity.Name,
                "Portal.CustomerService.PORequestUpload", Basics.MessageLogType.Critical);

            return false;
        }
    }
    
    public void OpenWindow(string url, string target, string features)
    {
        if (target == null) target = string.Empty;
        if (features == null) features = string.Empty;
        string script = string.Format("window.open(\"{0}\", \"{1}\", \"{2}\");", url, target, features);

        ScriptManager.RegisterClientScriptBlock(Page, typeof(Page), "OpenWindow", script, true);
    }
       
}