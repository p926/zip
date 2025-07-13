using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Instadose;
using Mirion.DSD.DosimetryDocs;
using Mirion.DSD.DosimetryDocs.Entity;
using Telerik.Web.UI;

public partial class IT_DosimetryDocuments : System.Web.UI.Page
{
    private readonly string[] Categories = { "BadgeTrack", "Dose Data", "HP Assist", "Custom", "Other" };
    public string errorMessage = "";
    public string successMessage = "";

    # region On Page Load
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.QueryString["Guid"] != null)
        {
            // Download build
            DownloadUpdate(Request.QueryString["Guid"]);
            return;
        }

        if (IsPostBack) return;

        var ddc = new DocumentsDataContext();

        // Upload Document Modal/Dialog.
        this.ddlUploadApplication.DataSource = ddc.Applications.Select(a => new { a.ApplicationID, a.ApplicationName });
        this.ddlUploadApplication.DataTextField = "ApplicationName";
        this.ddlUploadApplication.DataValueField = "ApplicationID";
        this.ddlUploadApplication.DataBind();
        this.ddlUploadCategory.DataSource = Categories;
        this.ddlUploadCategory.DataBind();
        this.txtUploadPublishDate.Text = string.Format("{0:MM/dd/yyyy}", DateTime.Now);

        // Edit Document Modal/Dialog.
        this.ddlEditApplication.DataSource = ddc.Applications.Select(a => new { a.ApplicationID, a.ApplicationName });
        this.ddlEditApplication.DataTextField = "ApplicationName";
        this.ddlEditApplication.DataValueField = "ApplicationID";
        this.ddlEditApplication.DataBind();
        this.ddlEditCategory.DataSource = Categories;
        this.ddlEditCategory.DataBind();
        this.txtEditPublishDate.Text = string.Format("{0:MM/dd/yyyy}", DateTime.Now);

        // Rebind to RadGrid.
        this.rgDosimetryDocuments.Rebind();
    }
    # endregion

    # region Upload Document Modal/Dialog
    /// <summary>
    /// Upload (NEW) Document.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnUploadDocument_Click(object sender, EventArgs e)
    {
        // Reset Success/Error Messages.
        this.uploadSuccess.Visible = false;
        this.uploadError.Visible = false;
        this.editSuccess.Visible = false;
        this.editError.Visible = false;

        // Initiate/default all values to be saved.
        int applicationID = 0;
        string accountID = "";
        string documentTitle = "";
        string category = "";
        string description = "";
        string internalNotes = "";
        DateTime? publishedDate = null;
        DateTime tempPublishedDate;

        // Set values based on controls.
        int.TryParse(this.ddlUploadApplication.SelectedValue, out applicationID);
        accountID = this.txtUploadAccount.Text.Trim();
        documentTitle = this.txtUploadDocumentTitle.Text.Replace("'", "''").Trim();
        category = this.ddlUploadCategory.SelectedValue;

        // Read the file into a byte array and encrypt it.
        var fileBytes = ReadAllBytes(this.fuUploadBuildFile.FileContent);

        description = this.reUploadDescription.Content.Trim();

        // Place NULL value into dataabase if the following do not have values.
        if (this.reUploadInternalNotes.Content.Trim() == "")
            internalNotes = null;
        else
            internalNotes = this.reUploadInternalNotes.Content.Trim();

        if (DateTime.TryParse(this.txtUploadPublishDate.Text, out tempPublishedDate))
            publishedDate = tempPublishedDate;
        else
            publishedDate = DateTime.Now;   // This will be uploaded date (defaulted to DateTime.Now).

        // Create DB Object to save/insert record to.
        var ddc = new DocumentsDataContext();

        // Select the application.
        var application = (from a in ddc.Applications where a.ApplicationID == applicationID select a).FirstOrDefault();

        // Ensure the application was found.
        if (application == null)
        {
            errorMessage = "The application could not be found.";
            this.uploadError.Visible = true;
            this.uploadErrorMessage.InnerText = errorMessage;
            return;
        }

        // Add the document.
        var doc = new Document()
        {
            Active = true,
            Account = accountID,
            Application = application,
            Category = category,
            CreatedBy = User.Identity.Name,
            CreatedDate = DateTime.Now,
            DocumentContent = fileBytes,
            DocumentDesc = description,
            DocumentGuid = Guid.NewGuid(),
            DocumentTitle = documentTitle,
            FileName = fuUploadBuildFile.FileName,
            InternalNotes = internalNotes,
            PublishedDate = publishedDate,
            ContentType = fuUploadBuildFile.PostedFile.ContentType
        };

        try
        {
            // Save the new record.
            ddc.Documents.InsertOnSubmit(doc);
            ddc.SubmitChanges();

            this.uploadSuccess.Visible = true;

            // Send the e-mail if the document is published.
            if (doc.PublishedDate.HasValue)
            {
                SendNewDocumentEmail(doc.Category, doc.ApplicationID, doc.Application.ApplicationName, doc.Account, doc.DocumentGuid);
            }

            // Reset the controls on the Upload Document form.
            this.ddlUploadApplication.SelectedIndex = 0;
            this.txtUploadAccount.Text = "";
            this.txtUploadDocumentTitle.Text = "";
            this.ddlUploadCategory.SelectedIndex = 0;
            this.reUploadDescription.Content = "";
            this.reUploadInternalNotes.Content = "";
            this.txtUploadPublishDate.Text = string.Format("{0:MM/dd/yyyy}", DateTime.Now);

            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('uploadDocumentModal')", true);

            // Reload the grid.
            this.rgDosimetryDocuments.Rebind();
        }
        catch (Exception ex)
        {
            this.uploadError.Visible = true;
            this.uploadErrorMessage.InnerText = ex.Message;
            return;
        }
            
    }

    /// <summary>
    /// Cancel & Close Upload Document Modal/Dialog.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnUploadDocumentCancel_Click(object sender, EventArgs e)
    {
        // Reset all controls.
        this.ddlUploadApplication.SelectedIndex = 0;
        this.txtUploadAccount.Text = "";
        this.txtUploadDocumentTitle.Text = "";
        this.ddlUploadCategory.SelectedIndex = 0;
        // By default/security reason, FileUpload control will be cleared.
        this.reUploadDescription.Content = "";
        this.reUploadInternalNotes.Content = "";
        this.txtUploadPublishDate.Text = string.Format("{0:MM/dd/yyyy}", DateTime.Now);
        
        // Reset all Error and Success Messages.
        this.uploadSuccess.Visible = false;
        this.uploadError.Visible = false;
        this.editSuccess.Visible = false;
        this.editError.Visible = false;

        // Close Modal/Dialog.
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('uploadDocumentModal')", true);
    }
    # endregion

    # region Edit Document Modal/Dialog
    /// <summary>
    /// On Click of Edit Pencil Icon in RadGrid for each/respective record.
    /// Display in Edit Document Modal/Dialog the following values in each control as assigned.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void lnkbtnEdit_Click(object sender, EventArgs e)
    {
        // Reset Success/Error Messages.
        this.uploadSuccess.Visible = false;
        this.uploadError.Visible = false;
        this.editSuccess.Visible = false;
        this.editError.Visible = false;

        // Load the edit form.
        var lbtnEdit = (LinkButton)sender;
        int documentID = int.Parse(lbtnEdit.CommandArgument);

        this.hfDocumentID.Value = documentID.ToString();

        var ddc = new DocumentsDataContext();

        // load the record.
        var document = (from d in ddc.Documents where d.DocumentID == documentID select d).FirstOrDefault();

        this.txtEditAccount.Text = document.Account.ToString();
        this.txtEditDocumentTitle.Text = document.DocumentTitle.ToString();
        this.reEditDescription.Content = document.DocumentDesc.ToString();
        if (document.InternalNotes == null)
            this.reEditInternalNotes.Content = "";
        else
            this.reEditInternalNotes.Content = document.InternalNotes.ToString();

        this.txtEditPublishDate.Text = string.Format("{0:MM/dd/yyyy}", document.PublishedDate);
        this.lblEditUploadedDocument.Text = document.FileName.ToString();
        this.ddlEditApplication.SelectedIndex = -1;
        var lstItemApplication = this.ddlEditApplication.Items.FindByValue(document.ApplicationID.ToString());
        lstItemApplication.Selected = true;

        this.ddlEditCategory.SelectedIndex = -1;
        var lstItemCategory = this.ddlEditCategory.Items.FindByValue(document.Category);
        if (lstItemCategory == null)
            this.ddlEditCategory.Items.Insert(0, new ListItem(document.Category));
        else
            lstItemCategory.Selected = true;

        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('editDocumentModal')", true);
    }

    /// <summary>
    /// Update/Edit selected Document record.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnEditDocument_Click(object sender, EventArgs e)
    {
        // Reset Success/Error Messages.
        this.uploadSuccess.Visible = false;
        this.uploadError.Visible = false;
        this.editSuccess.Visible = false;
        this.editError.Visible = false;

        // Initiate/default all values to be updated/saved.
        int documentID = 0;
        bool active = true;
        int applicationID = 0;
        string accountID = "";
        string documentTitle = "";
        string category = "";
        string description = "";
        string internalNotes = "";
        DateTime? publishedDate = null;
        //DateTime tempPublishedDate;

        // Set the values for each control.
        int.TryParse(this.hfDocumentID.Value, out documentID);
        active = !cbEditDelete.Checked;
        int.TryParse(this.ddlEditApplication.SelectedValue, out applicationID);
        accountID = this.txtEditAccount.Text.Trim();
        documentTitle = this.txtEditDocumentTitle.Text.Replace("'", "''").Trim();
        category = this.ddlEditCategory.SelectedValue;
        description = this.reEditDescription.Content.Trim();
        internalNotes = this.reEditInternalNotes.Content.Trim();

        // Place NULL value into database if the following do not have a value.
        if (internalNotes == "")
            internalNotes = null;
        else
            internalNotes = this.reEditInternalNotes.Content.Trim();

        // Create DB Object to save/insert record to.
        var ddc = new DocumentsDataContext();

        // Select the application.
        var application = (from a in ddc.Applications where a.ApplicationID == applicationID select a).FirstOrDefault();

        // Ensure the application was found.
        if (application == null)
        {
            errorMessage = "The application could not be found.";
            this.editError.Visible = true;
            this.editErrorMessage.InnerText = errorMessage;
            return;
        }

        // Get Document record in database.
        var doc = (from d in ddc.Documents where d.DocumentID == documentID select d).FirstOrDefault();

        // If Document record does not exist, do not continue.
        if (doc == null) return;

        // If the Publish Date is NULL or string.Empty (""), then default to DateTime.Now.
        if (txtEditPublishDate.Text == "")
        {
            publishedDate = DateTime.Now;
        }
        else
        {
            publishedDate = Convert.ToDateTime(txtEditPublishDate.Text);
        }

        // Was the document published before?
        bool wasPublished = doc.PublishedDate.HasValue;

        // Update the record where needed.
        doc.Active = active;
        doc.ApplicationID = applicationID;
        doc.Account = accountID;
        doc.DocumentTitle = documentTitle;
        doc.Category = category;
        doc.DocumentDesc = description;
        doc.InternalNotes = internalNotes;
        doc.PublishedDate = publishedDate;

        try
        {
            // Save the new record.
            ddc.SubmitChanges();

            editSuccess.Visible = true;

            // send the email if the document wasn't published, but is now published.
            if (!wasPublished && doc.PublishedDate.HasValue)
            {
                SendNewDocumentEmail(doc.Category, doc.ApplicationID, doc.Application.ApplicationName, doc.Account, doc.DocumentGuid);
            }

            // Reset the controls on the Edit Document form.
            this.hfDocumentID.Value = "0";
            this.cbEditDelete.Checked = false;
            this.ddlEditApplication.SelectedIndex = 0;
            this.txtEditAccount.Text = "";
            this.txtEditDocumentTitle.Text = "";
            this.ddlEditCategory.SelectedIndex = 0;
            this.reEditDescription.Content = "";
            this.reEditInternalNotes.Content = "";
            this.txtEditPublishDate.Text = string.Format("{0:MM/dd/yyyy}", DateTime.Now);

            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('editDocumentModal')", true);

            // Reload the grid.
            rgDosimetryDocuments.Rebind();
        }
        catch (Exception ex)
        {
            this.editError.Visible = true;
            this.editErrorMessage.InnerText = ex.Message;
            return;
        }
    }

    /// <summary>
    /// Cancel & Close Edit Document Modal/Dialog.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnEditDocumentCancel_Click(object sender, EventArgs e)
    {
        // Reset all controls.
        this.hfDocumentID.Value = "0";
        this.cbEditDelete.Checked = false;
        this.ddlEditApplication.SelectedIndex = 0;
        this.txtEditAccount.Text = "";
        this.txtEditDocumentTitle.Text = "";
        this.ddlEditCategory.SelectedIndex = 0;
        this.reEditDescription.Content = "";
        this.reEditInternalNotes.Content = "";
        this.txtEditPublishDate.Text = string.Format("{0:MM/dd/yyyy}", DateTime.Now);

        // Reset all Error/Success Messages.
        this.uploadSuccess.Visible = false;
        this.uploadError.Visible = false;
        this.editSuccess.Visible = false;
        this.editError.Visible = false;

        // Close Modal/Dialog.
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('editDocumentModal')", true);
    }
    # endregion

    # region RadGrid Functions
    /// <summary>
    /// Get data from IRV_DosimetryDocs DB and bind to RadGrid. 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    protected void rgDosimetryDocuments_NeedDataSource(object source, GridNeedDataSourceEventArgs e)
    {
        var ddc = new DocumentsDataContext();
        // Bind the results to the RadGrid.
        rgDosimetryDocuments.DataSource = ddc.Documents.Select(d =>
        new
        {
            d.Account,
            d.Active,
            d.Application.ApplicationName,
            d.Category,
            d.CreatedBy,
            d.CreatedDate,
            d.DocumentDesc,
            d.DocumentGuid,
            d.DocumentID,
            d.DocumentTitle,
            d.ExpirationDate,
            d.FileName,
            d.InternalNotes,
            d.ContentType,
            d.PublishedDate,
            d.ReviewedBy,
            d.ReviewedDate
        }).Where(u => u.Active).OrderByDescending(d => d.CreatedDate);
    }

    /// <summary>
    /// Formats FileName in RadGrid.
    /// If, the FileName is more than 20 characters, it will be appended with "...".
    /// Else, the complete FileName will be displayed.
    /// </summary>
    /// <param name="titletext"></param>
    /// <returns></returns>
    public string FormatDocumentTitle(string titletext)
    {
        if (titletext.Length > 20)
        {
            return titletext.Substring(0, 20) + "...";
        }
        else
        {
            return titletext;
        }
    }

    protected void rgDosimetryDocuments_ItemCommand(object sender, Telerik.Web.UI.GridCommandEventArgs e)
    {
        rgDosimetryDocuments.Rebind();
    }
    # endregion

    # region General/Global Functions
    private void DownloadUpdate(string guid)
    {
        Guid documentGuid;
        if (!Guid.TryParse(guid, out documentGuid)) throw new Exception("The Guid in invalid.");

        var db = new DocumentsDataContext();
        var document = (from d in db.Documents where d.DocumentGuid == documentGuid select d).FirstOrDefault();

        if (document == null) throw new Exception("The document does not exist.");

        var content = document.DocumentContent.ToArray();

        // Clear everything out.
        Response.Clear();
        Response.ClearHeaders();

        // Set the response headers.
        Response.ContentType = document.ContentType;

        //Content-Disposition: attachment; filename=<file name.ext>. 
        Response.AddHeader("Content-Disposition", "attachment; filename=" + document.FileName);

        // Write the file to the response.
        Response.BinaryWrite(content);

        Response.Flush();
        Response.End();
    }

    private void SendNewDocumentEmail(string category, int appid, string appname, string account, Guid documentguid)
    {
        var db = new DocumentsDataContext();
        var toAddresses = (from u in db.Users
                           where u.Active &&
                                 u.AcceptsEmails &&
                                 u.Account == account &&
                                 u.ApplicationID == appid
                           select u.UserName).ToList();

        // build a list of fields.
        var fields = new Dictionary<string, string>();
        fields.Add("Category", category);
        fields.Add("AppIdentifier", appname);
        fields.Add("Account", account);
        fields.Add("DownloadUrl", string.Format("https://docs.dosimetry.com/Documents/View#{0}", documentguid));

        if (toAddresses.Count >= 1)
        {
            foreach (var email in toAddresses)
            {
                var mSys = new MessageSystem();
                mSys.Application = "Dosimetry Documents";
                mSys.CreatedBy = ActiveDirectoryQueries.GetUserName();
                mSys.FromAddress = "noreply@dosimetry.com";
                mSys.ToAddressList = new List<string>();
                mSys.ToAddressList.Add(email);

                // send the email.
                mSys.Send("new-dosimetry-doc", "", fields);
            }
        }
    }

    public static byte[] ReadAllBytes(Stream stream)
    {
        stream.Position = 0;
        byte[] buffer = new byte[stream.Length];
        for (int totalBytesCopied = 0; totalBytesCopied < stream.Length; )
            totalBytesCopied += stream.Read(buffer, totalBytesCopied, Convert.ToInt32(stream.Length) - totalBytesCopied);
        return buffer;
    }

    // Clears all filter values on RadGrid.
    protected void lnkbtnClearFilters_Click(object sender, EventArgs e)
    {
        foreach (GridColumn column in this.rgDosimetryDocuments.MasterTableView.OwnerGrid.Columns)
        {
            column.CurrentFilterFunction = GridKnownFunction.NoFilter;
            column.CurrentFilterValue = string.Empty;
        }
        this.rgDosimetryDocuments.MasterTableView.FilterExpression = string.Empty;
        this.rgDosimetryDocuments.Rebind();
    }
    # endregion
}