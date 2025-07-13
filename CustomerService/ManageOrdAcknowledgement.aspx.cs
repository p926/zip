using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Instadose;
using Instadose.Data;
using System.IO;

public partial class InstaDose_CustomerService_ManageOrdAcknowledgement : System.Web.UI.Page
{
    // Create the database reference
    InsDataContext idc = new InsDataContext();
    
    // String to hold the current username
    string UserName = "Unknown";
    int AccountID = 0;
    // Build list of error messages
    List<string> ErrorMessageList = new List<string>();

    // Build list of error messages
    List<string> MessageList = new List<string>();

    protected void Page_Load(object sender, EventArgs e)
    {
        Page.Title = "Customer Services - Upload Order Acknowledgement";

        // Try to set the username
        try
        {
            this.UserName = User.Identity.Name.Split('\\')[1];
            //this.lblusername.Text = this.UserName;
        }
        catch { this.UserName = "Unknown"; }

        if (!IsPostBack)
        {
            UploadOrderAckDialog.Visible = false;

            if (Request.QueryString["ID"] != null)
            {
                txtAccountID.Text = Request.QueryString["ID"].ToString();

                this.btnUploadForm.Visible = true;
            }
            else
            {
                btnUploadForm.Visible = false;
            }
        }
        
    }

    /// <summary>
    /// btnSearch_Click
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnSearch_Click(object sender, EventArgs e)
    {
        int accountID = 0;

        try
        {
            int.TryParse(txtAccountID.Text, out accountID);

            if (accountID != 0)
            {
                HidAccountID.Value = accountID.ToString();

                bindAcknowledgementsGrid(accountID);
                btnUploadForm.Visible = true;
                //updtpnlAckfiles.Visible = true;
            }
            else 
            {
                btnUploadForm.Visible = false;  // no upload allowed without proper Acct#
            }

            //gvAck.DataBind();
        }
        catch (Exception ex)
        {
            //Add to error message list for screen display.
            ErrorMessageList.Add(ex.Message);

            this.bindErrorMessages();

            // Report the error to the message system.
            Basics.WriteLogEntry(ex.Message, Page.User.Identity.Name,
                "Portal.CustomerService.OrdAcknowledgment.btnView_Click", Basics.MessageLogType.Critical);

        }
    }

    /// <summary>
    /// BindRenewalReviewGrid
    /// </summary>
    /// <param name="RenewalNo"></param>
    private void bindAcknowledgementsGrid(int accountID)
    {
       gvAck.DataSource = from d in idc.Documents
                              where d.AccountID == accountID &&
                                d.Active == true &&
                                d.DocumentCategory == "Order Acknowledgement"
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

        gvAck.DataBind();

    }


    protected void btnUpload_Click(object sender, EventArgs e)
    {
        int accountID = 0;
        string uploadResult = null;

        try
        {
            int.TryParse(txtAccountID.Text, out accountID);

            if (accountID != 0)
            {
                // Create upload attachment record to unique requestID
                //if (FileUpload1.HasFile)
                //    this.CommitUploadAttachFile(1, accountID);

                //if (FileUpload2.HasFile)
                //    this.CommitUploadAttachFile(2, accountID);
 
                bool? file1Success = null;
                bool? file2Success = null;

                uploadResult = "accountID: " + accountID.ToString();

                //Add data error to error message list for screen display.
                MessageList.Add(uploadResult);
  
                //VisibleSuccesses(uploadResult);
                this.bindMessages();

                // Upload file 1 if it exists.
                if (FileUpload1.HasFile)
                {
                    uploadResult = "Upload file1 begin.  ";
                    
                    //VisibleSuccesses(uploadResult);
                    //Add data error to error message list for screen display.
                    MessageList.Add(uploadResult);

                    //VisibleSuccesses(uploadResult);
                    this.bindMessages();

                    file1Success = this.commitUploadFile(FileUpload1.PostedFile, 
                            accountID, uploadResult);

                    //uploadResult = " Upload file1 end.";
                    //VisibleSuccesses(uploadResult);

                    if (file1Success.Value)
                    {
                        //uploadResult = "Attachment 1: " + ((file1Success.Value) ? "Success" : "Failed");
                        
                        uploadResult = "  Attachment 1: upload Succeeded.";
                        //VisibleSuccesses(uploadResult);

                        //Add data error to error message list for screen display.
                        MessageList.Add(uploadResult);

                        //VisibleSuccesses(uploadResult);
                        this.bindMessages();

                    }
                    else
                    {
                        //uploadResult = "Attachment 1: " + ((file1Success.Value) ? "Success" : "Failed");
                        
                        uploadResult = " Attachment 1: upload failed.";

                        //Add data error to error message list for screen display.
                        ErrorMessageList.Add(uploadResult);
                        this.bindErrorMessages();
  
                    }

                    //uploadResult = " Upload file1 end";
                    ////VisibleSuccesses(uploadResult);

                    ////Add data error to error message list for screen display.
                    //MessageList.Add(uploadResult);

                    ////VisibleSuccesses(uploadResult);
                    //this.bindMessages();

                }
                else
                {
                    VisibleErrors("No upload file1 found.");
                }

                // Upload file 2 if it exists.
                if (FileUpload2.HasFile)
                {
                    //uploadResult += "Upload file2 begin";
                    //VisibleSuccesses(uploadResult);

                    file2Success = this.commitUploadFile(FileUpload2.PostedFile, accountID, uploadResult);

                    if (file2Success.Value)
                    {
                        //uploadResult = "Attachment 2: " + ((file1Success.Value) ? "Success" : "Failed");
                        uploadResult = "Attachment 2: upload Succeeded.";
                        //VisibleSuccesses(uploadResult);

                        //Add data error to error message list for screen display.
                        MessageList.Add(uploadResult);

                        //VisibleSuccesses(uploadResult);
                        this.bindMessages();
                    }
                    else
                    {
                        //uploadResult = "Attachment 2: " + ((file1Success.Value) ? "Success" : "Failed");

                        uploadResult = "Attachment 2: upload failed.";
                        //VisibleErrors(uploadResult);

                        //Add data error to error message list for screen display.
                        ErrorMessageList.Add(uploadResult);
                        this.bindErrorMessages();
                    }

                    //uploadResult = "Upload file2 end";
                    ////VisibleSuccesses(uploadResult);
                    
                    ////Add data error to error message list for screen display.
                    //MessageList.Add(uploadResult);

                    ////VisibleSuccesses(uploadResult);
                    //this.bindMessages();
                
                }

                bindAcknowledgementsGrid(accountID);

            }
        }
        catch (Exception ex)
        {
            //VisibleErrors(ex.ToString());

            //VisibleErrors(uploadResult);

            //Add data error to error message list for screen display.
            ErrorMessageList.Add(ex.ToString());
            this.bindErrorMessages();

            Basics.WriteLogEntry(ex.Message, HttpContext.Current.User.Identity.Name,
                "Portal.CustomerService.OrdAcknowledgment.btnUpload_Click", Basics.MessageLogType.Critical);
            
            //return null;
            //Response.Write(ex);
        }

    }

    protected void btnCancel_Click(object sender, EventArgs e)
    {
        int accountID = 0;

        int.TryParse(txtAccountID.Text, out accountID);
          
        this.UploadOrderAckDialog.Visible = false;
        this.bindAcknowledgementsGrid(accountID);

        //Response.Redirect("default.aspx");
    }

    /// <summary>
    /// checkDuplicateAckFile
    /// </summary>
    /// <param name="accountID"></param>
    /// <param name="filename"></param>
    /// <returns></returns>
    private bool checkDuplicateAckFile(int accountID,
            string filename)
    {
        bool isDuplicate = false;

        // Look at the Document and determine if the file name has already been uploaded.
        IQueryable<Document> doc = (from d in idc.Documents
                                        where d.AccountID == accountID
                                        && d.FileName == filename
                                        select d).AsQueryable();

        //same file exists for this account?
        if (doc.Count() > 0) isDuplicate = true;

        return isDuplicate;
    }

    public void bindErrorMessages()
    {
        //Turn off message
        //plFormMessage.Visible = false;

        blstErrors.DataSource = ErrorMessageList;
        blstErrors.DataBind();

        //Display only errors are present.
        if (ErrorMessageList.Count() == 0)
        {
            blstErrors.Visible = false;
        }
        else
        {
            blstErrors.Visible = true;
        }
    }

    public void resetblstdErrorMessages()
    {
        //Turn off message
        //plFormMessage.Visible = false;

        blstErrors.ClearSelection();
        blstErrors.Visible = false;

    }

    public void bindMessages()
    {
        //Turn off message
        //plFormMessage.Visible = false;

        blstMessage.DataSource = MessageList;
        blstMessage.DataBind();

        //Display only errors are present.
        if (MessageList.Count() == 0)
        {
            blstMessage.Visible = false;
        }
        else
        {
            blstMessage.Visible = true;
        }
    }

    public void resetblstdMessages()
    {
        //Turn off message
        //plFormMessage.Visible = false;

        blstMessage.ClearSelection();
        blstMessage.Visible = false;

    }

    private void InvisibleErrors()
    {
        // Reset submission form error message.
        this.formErrors.Visible = false;
        //this.errorMsg.InnerText = "";
        blstErrors.Visible = false;

    }

    private void VisibleErrors(string error)
    {
        this.formErrors.Visible = true;
        //this.errorMsg.InnerText = error;
        this.blstErrors.Visible = true;
    }

    private void InvisibleSuccesses()
    {
        this.formSuccesses.Visible = false;
        ///this.successMsg.InnerText = "";
        blstMessage.Visible = false;

       
    }

    private void VisibleSuccesses(string success)
    {
        this.formSuccesses.Visible = true;
        //this.successMsg.InnerText = success;
        this.blstMessage.Visible = true;
    }
     

    //private void CommitUploadAttachFile(int UploadControlNo, int AccountID)
    //{
    //    // Get Upload infor and convert to byte format
    //    HttpPostedFile myUploadFile = null;
        
    //    if (UploadControlNo == 1)
    //    {
    //        myUploadFile = FileUpload1.PostedFile;
    //    }
    //    else if (UploadControlNo == 2)
    //    {
    //        myUploadFile = FileUpload2.PostedFile;
    //    }
        
    //    //find upload file length and convert it to byte array
    //    int intContentLength = myUploadFile.ContentLength;

    //    //create byte array
    //    Byte[] bytimage = new Byte[intContentLength];

    //    //read upload file in byte array
    //    myUploadFile.InputStream.Read(bytimage, 0, intContentLength);

    //    string myFileName = System.IO.Path.GetFileName(myUploadFile.FileName);
    //    string myFileExtension = myFileName.Split('.')[1];


    //    OrderAcknowledgement dbUploadFile = new OrderAcknowledgement()
    //    {
    //        AccountID = AccountID,
    //        AckFileName  = myFileName,
    //        AckFileExtension  = myFileExtension,
    //        AckFileContent = bytimage,
    //        CreatedBy = this.UserName,
    //        CreatedDate = DateTime.Now,
    //        Active = true
    //    };

    //    // Save the changes.
    //    idc.OrderAcknowledgements.InsertOnSubmit(dbUploadFile);
    //    idc.SubmitChanges();


    //}

    //******************************************************************************************
    //8/2012 - New Upload to Documents table.
    //******************************************************************************************

    /// <summary>
    /// commitUploadFile
    /// </summary>
    /// <param name="file"></param>
    /// <param name="accountID"></param>
    /// <returns></returns>
    private bool commitUploadFile(HttpPostedFile file, int accountID, string errorMessage)
    {
        string attachmentFile = Path.GetFileName(file.FileName);

        try
        {
            //has the file been already uploaded?
            if (checkDuplicateAckFile(accountID, attachmentFile))
            {
                errorMessage += attachmentFile + " has already been uploaded.";

                //Add data error to error message list for screen display.
                ErrorMessageList.Add(errorMessage);

                this.bindErrorMessages();


                return false;
            }

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
                CreatedBy = this.UserName,
                CreatedDate = DateTime.Now,
                Description = "Order Acknowledgement Document Upload",
                DocumentCategory = "Order Acknowledgement",
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
            //show error.
            VisibleErrors(ex.ToString());

            // Report the error to the message system.
            Basics.WriteLogEntry(ex.Message, Page.User.Identity.Name,
                "Portal.CustomerService.OrdAcknowledgment", Basics.MessageLogType.Critical);

            return false;
        }
    }

    public string FuncGenerateHTTPLink(string DocumentID)
    {
        // Generate HTTP URL for attachment, force program run in HTTP url
        string HttpURL = Request.Url.ToString();
        HttpURL = HttpURL.Replace("https", "http");
        HttpURL = HttpURL.Substring(0, HttpURL.LastIndexOf("/") + 1);
        HttpURL = HttpURL + "OrdAcknowledgementView.aspx?ID=" + DocumentID ;

        return HttpURL;

    }

    protected void txtAccountID_TextChanged(object sender, EventArgs e)
    {
        this.UploadOrderAckDialog.Visible = false;

        resetblstdErrorMessages();
        resetblstdMessages();
              
    }

   
    protected void btnUploadForm_Click(object sender, EventArgs e)
    {
       this.UploadOrderAckDialog.Visible = true;
    }

    protected void btnClear_Click(object sender, EventArgs e)
    {
        txtAccountID.Text = "0";
        resetblstdMessages();
        resetblstdErrorMessages();

        this.InvisibleErrors();
        this.InvisibleSuccesses();

        this.btnUploadForm.Visible = false;
        UploadOrderAckDialog.Visible = false;

        bindAcknowledgementsGrid(0);

    }
}
