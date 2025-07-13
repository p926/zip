using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Instadose.Data;
using Portal.Instadose;
using Telerik.Web.UI;

namespace portal_instadose_com_v3.CustomerService
{
    public partial class DealerDocuments : System.Web.UI.Page
    {
        #region GLOBAL VARIABLES.
        InsDataContext idc = new InsDataContext();
        int dealerID = 0;
        string poNumber = "";
        string userName = "Unknown";
        public string errorMessage = "";
        public string successMessage = "";
        #endregion

        #region PAGE LOAD.
        protected void Page_Load(object sender, EventArgs e)
        {
            // Get DealerID from QueryString.
            if (Request.QueryString["DealerID"] != "0")
            {
                int.TryParse(Request.QueryString["DealerID"], out dealerID);
            }
            else
            {
                dealerID = 0;
            }

            // Get UserName.
            try { userName = User.Identity.Name.Split('\\')[1]; }
            catch { userName = "Unknown"; }

            if (Page.IsPostBack) return;

            PopulateDealerDDL(this.ddlUploadDealer);
            PopulateDealerDDL(this.ddlEditDealer);
            PopulateCategoryDDL(this.ddlUploadCategory);
            PopulateCategoryDDL(this.ddlEditCategory);

            // Disable both form's PO Number Required Field Validator controls.
            this.spnUploadPONumberRequired.Visible = false;
            this.reqfldvalUploadPONumber.Enabled = false;
            this.spnEditPONumberRequired.Visible = false;
            this.reqfldvalEditPONumber.Enabled = false;

            this.rgDealerDocuments.Rebind();
        }
        #endregion

        #region PONUMBER SELECTED ACTION.
        /// <summary>
        /// When selecting Document Category, if "Purchase Order" is selected, PO Number field will be required.
        /// Purchase Order is Category value 1 in the Database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ddlUploadCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.ddlUploadCategory.SelectedValue == "1")
            {
                this.spnUploadPONumberRequired.Visible = true;
                this.reqfldvalUploadPONumber.Enabled = true;
            }
            else
            {
                this.spnUploadPONumberRequired.Visible = false;
                this.reqfldvalUploadPONumber.Enabled = false;
            }   
        }

        /// <summary>
        /// When selecting Document Category, if "Purchase Order" is selected, PO Number field will be required.
        /// Purchase Order is Category value 1 in the Database.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ddlEditCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.ddlEditCategory.SelectedValue == "1")
            {
                this.spnEditPONumberRequired.Visible = true;
                this.reqfldvalEditPONumber.Enabled = true;
            }
            else
            {
                this.spnEditPONumberRequired.Visible = false;
                this.reqfldvalEditPONumber.Enabled = false;
            }
        }
        #endregion

        #region VALIDATE FORM CONTROLS.
        /// <summary>
        /// Checks/Validates Upload Document Form Controls for required fields/values.
        /// </summary>
        /// <returns></returns>
        //private bool PassInputsValidation_UploadDocument()
        //{
        //    // Dealer (DealerID) required.
        //    if (this.ddlUploadDealer.SelectedValue == "0")
        //    {
        //        this.VisibleUploadError("Dealer is required.");
        //        SetFocus(this.ddlUploadDealer);
        //        return false;
        //    }

        //    // Category (CategoryID) required.
        //    if (this.ddlUploadCategory.SelectedValue == "0")
        //    {
        //        this.VisibleUploadError("Category is required.");
        //        SetFocus(this.ddlUploadCategory);
        //        return false;
        //    }

        //    // PO # is required ONLY IF ddlUploadCategory.SelectedItem.Text is "Purchase Order".
        //    if (this.ddlUploadCategory.SelectedItem.Text == "Purchase Order" && this.txtUploadPONumber.Text.Trim().Length == 0)
        //    {
        //        this.VisibleUploadError("PO # is required.");
        //        SetFocus(this.txtUploadPONumber);
        //        return false;
        //    }

        //    // Select/Choose a File to Upload required.
        //    if (!this.fuUploadDocument.HasFile)
        //    {
        //        this.VisibleUploadError("File to Upload is required.");
        //        SetFocus(this.fuUploadDocument);
        //        return false;
        //    }

        //    // File Description required.
        //    if (this.reUploadDescription.Text.Trim().Length == 0)
        //    {
        //        this.VisibleUploadError("File Description is required.");
        //        SetFocus(this.reUploadDescription);
        //        return false;
        //    }

        //    // Received Date required.
        //    if (this.txtUploadReceivedDate.Text.Trim().Length == 0)
        //    {
        //        this.VisibleUploadError("Received Date is required.");
        //        SetFocus(this.txtUploadReceivedDate);
        //        return false;
        //    }
            
        //    return true;
        //}

        /// <summary>
        /// Checks/Validates Edit/Update Document Form Controls for required fields/values.
        /// </summary>
        /// <returns></returns>
        //private bool PassInputsValidation_EditDocument()
        //{
        //    // Dealer (DealerID) required.
        //    if (this.ddlEditDealer.SelectedValue == "0")
        //    {
        //        this.VisibleEditError("Dealer is required.");
        //        SetFocus(this.ddlEditDealer);
        //        return false;
        //    }

        //    // Category (CategoryID) required.
        //    if (this.ddlEditCategory.SelectedValue == "0")
        //    {
        //        this.VisibleEditError("Category is required.");
        //        SetFocus(this.ddlEditCategory);
        //        return false;
        //    }

        //    // PO # is required ONLY IF ddlUploadCategory.SelectedItem.Text is "Purchase Order".
        //    if (this.ddlEditCategory.SelectedItem.Text == "Purchase Order" && this.txtEditPONumber.Text.Trim().Length == 0)
        //    {
        //        this.VisibleEditError("PO # is required.");
        //        SetFocus(this.txtEditPONumber);
        //        return false;
        //    }

        //    // Select/Choose a File to Upload required.
        //    if (!this.fuEditDocument.HasFile)
        //    {
        //        this.VisibleEditError("File to Upload is required.");
        //        SetFocus(this.fuEditDocument);
        //        return false;
        //    }

        //    // File Description required.
        //    if (this.reEditDescription.Text.Trim().Length == 0)
        //    {
        //        this.VisibleEditError("File Description is required.");
        //        SetFocus(this.reEditDescription);
        //        return false;
        //    }

        //    // Received Date required.
        //    if (this.txtEditReceivedDate.Text.Trim().Length == 0)
        //    {
        //        this.VisibleEditError("Received Date is required.");
        //        SetFocus(this.txtEditReceivedDate);
        //        return false;
        //    }

        //    return true;
        //}
        #endregion

        #region POPULATE DROPDOWNLISTS.
        private void PopulateDealerDDL(DropDownList ddldealer)
        {
            ddldealer.DataSource = idc.Dealers.Select(d => new { d.DealerID, d.DealerName }).Distinct();
            ddldealer.DataTextField = "DealerName";
            ddldealer.DataValueField = "DealerID";
            ddldealer.DataBind();
        }

        private void PopulateCategoryDDL(DropDownList ddlcategory)
        {
            ddlcategory.DataSource = idc.DealerDocumentCategories.Select(ddc => new { ddc.DealerDocumentCategoryID, ddc.Category });
            ddlcategory.DataTextField = "Category";
            ddlcategory.DataValueField = "DealerDocumentCategoryID";
            ddlcategory.DataBind();
        }
        #endregion

        #region RADGRID FUNCTIONS.
        protected void rgDealerDocuments_NeedDataSource(object source, GridNeedDataSourceEventArgs e)
        {
            // Bind the results to the RadGrid.
            if (dealerID != 0)
            {
                var recordWithDealerID = (from dd in idc.DealerDocuments
                                          where dd.DealerID == dealerID && dd.Document.Active == true
                                          orderby dd.ReceivedDate descending
                                          select new
                                          {
                                              DealerID = dd.DealerID,
                                              DealerName = dd.Dealer.DealerName,
                                              DocumentID = dd.Document.DocumentID,
                                              DocumentGUID = dd.Document.DocumentGUID,
                                              Category = dd.DealerDocumentCategory.Category,
                                              PONumber = dd.PONumber,
                                              FileName = dd.Document.FileName,
                                              Description = dd.Document.Description,
                                              ReceivedDate = dd.ReceivedDate,
                                              CreatedBy = dd.Document.CreatedBy
                                          });

                this.rgDealerDocuments.DataSource = recordWithDealerID;
            }
            else
            {
                var recordWithoutDealerID = (from dd in idc.DealerDocuments
                                             where dd.Document.Active == true
                                             orderby dd.ReceivedDate descending
                                             select new
                                             {
                                                 DealerID = dd.DealerID,
                                                 DealerName = dd.Dealer.DealerName,
                                                 DocumentID = dd.Document.DocumentID,
                                                 DocumentGUID = dd.Document.DocumentGUID,
                                                 Category = dd.DealerDocumentCategory.Category,
                                                 PONumber = dd.PONumber,
                                                 FileName = dd.Document.FileName,
                                                 Description = dd.Document.Description,
                                                 ReceivedDate = dd.ReceivedDate,
                                                 CreatedBy = dd.Document.CreatedBy
                                             });

                this.rgDealerDocuments.DataSource = recordWithoutDealerID;
            }
        }

        protected void rgDealerDocuments_ItemCommand(object sender, Telerik.Web.UI.GridCommandEventArgs e)
        {
            this.rgDealerDocuments.Rebind();
        }

        /// <summary>
        /// Clear ALL filtered results from RadGrid (reset).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void lnkbtnClearFilters_Click(object sender, EventArgs e)
        {
            foreach (GridColumn column in this.rgDealerDocuments.MasterTableView.OwnerGrid.Columns)
            {
                column.CurrentFilterFunction = GridKnownFunction.NoFilter;
                column.CurrentFilterValue = string.Empty;
            }
            this.rgDealerDocuments.MasterTableView.FilterExpression = string.Empty;
            this.rgDealerDocuments.Rebind();
        }

        protected void lnkbtnEdit_Click(object sender, EventArgs e)
        {
            // Reset all Success/Error messages.
            InvisibleUploadError();
            InvisibleUploadSuccess();
            InvisibleEditError();
            InvisibleEditSuccess();

            // Load the Edit Form.
            var lnkbtn = (LinkButton)sender;
            int documentID = int.Parse(lnkbtn.CommandArgument);

            this.hfDocumentID.Value = documentID.ToString();

            var documentDealerRecord = (from d in idc.Documents
                                        join dd in idc.DealerDocuments on d.DocumentID equals dd.DocumentID
                                        where d.DocumentID == documentID
                                        select new
                                        {
                                            DealerID = dd.DealerID,
                                            CategoryID = dd.CategoryID,
                                            PONumber = dd.PONumber,
                                            FileName = d.FileName,
                                            Description = d.Description,
                                            ReceivedDate = dd.ReceivedDate
                                        }).FirstOrDefault();

            if (documentDealerRecord == null) return;

            // Populate controls with record values.
            this.cbEditDelete.Checked = false;
            this.ddlEditDealer.SelectedValue = documentDealerRecord.DealerID.ToString();
            this.ddlEditCategory.SelectedValue = documentDealerRecord.CategoryID.ToString();
            if (this.ddlEditCategory.SelectedValue == "1")
            {
                this.spnEditPONumberRequired.Visible = true;
                this.reqfldvalEditPONumber.Enabled = true;
            }
            else
            {
                this.spnEditPONumberRequired.Visible = false;
                this.reqfldvalEditPONumber.Enabled = false;
            }
            this.txtEditPONumber.Text = ((documentDealerRecord.PONumber == null ? "" : documentDealerRecord.PONumber));
            this.lblOldDocument.Text = documentDealerRecord.FileName;
            if (documentDealerRecord.Description != null)
                this.reEditDescription.Content = documentDealerRecord.Description;
            else
                this.reEditDescription.Content = "";
            this.txtEditReceivedDate.Text = string.Format("{0:MM/dd/yyyy}", documentDealerRecord.ReceivedDate);

            // Open Modal/Dialog.
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('editDocumentModal')", true);
        }
        #endregion

        #region UPLOAD DOCUMENT.
        protected void btnUploadDocument_Click(object sender, EventArgs e)
        {
            // Reset Error/Success Messages.
            InvisibleUploadError();
            InvisibleUploadSuccess();
            InvisibleEditError();
            InvisibleEditSuccess();

            if (this.Page.IsValid)
            {
                // Initiate default control values.
                int categoryID = 0;
                int documentID = 0;
                string poNumber = "";
                string description = "";
                DateTime receivedDate = DateTime.Now;

                // Set values based on controls.
                // DealerID.
                if (dealerID == 0)
                    int.TryParse(this.ddlUploadDealer.SelectedValue, out dealerID);
                else
                    int.TryParse(Request.QueryString["DealerID"], out dealerID);
                // CategoryID.
                int.TryParse(this.ddlUploadCategory.SelectedValue, out categoryID);
                // PO Number.
                if (this.txtUploadPONumber.Text.Trim() == "")
                    poNumber = null;
                else
                    poNumber = this.txtUploadPONumber.Text.Replace("'", "''").Trim();
                // Upload Document (Read the file into a byte array and encrypt it).
                byte[] fileBytes = ReadAllBytes(this.fuUploadDocument.FileContent);

                // Description.
                description = this.reUploadDescription.Text.Replace("'", "''").Trim();

                // Received Date.
                receivedDate = DateTime.ParseExact(this.txtUploadReceivedDate.Text, "MM/dd/yyyy", CultureInfo.InvariantCulture);

                // Create DB object to save/insert record to.
                Instadose.Data.Document documentRecord = new Instadose.Data.Document()
                {
                    DocumentGUID = Guid.NewGuid(),
                    FileName = Path.GetFileName(this.fuUploadDocument.FileName),
                    DocumentContent = fileBytes,
                    MIMEType = GetMIMEType(this.fuUploadDocument),
                    DocumentCategory = this.ddlUploadCategory.SelectedItem.Text,
                    Description = description,
                    AccountID = null,
                    OrderID = null,
                    CreatedBy = userName,
                    CreatedDate = receivedDate,
                    Active = true,
                    ReferenceKey = null,
                    GDSAccount = null
                };

                try
                {
                    // Submit all changes to the DB.
                    idc.Documents.InsertOnSubmit(documentRecord);
                    idc.SubmitChanges();

                    System.Threading.Thread.Sleep(5000);

                    var getNewlyCreatedDocumentRecord = (from d in idc.Documents orderby d.DocumentID descending select d).FirstOrDefault();

                    documentID = getNewlyCreatedDocumentRecord.DocumentID;

                    string insertStatement = "INSERT INTO DealerDocuments (DealerID, DocumentID, CategoryID, PONumber, ReceivedDate)" +
                                             " VALUES(" + dealerID + "," + documentID + "," + categoryID + ",'" + poNumber + "','" + this.txtUploadReceivedDate.Text + "')";

                    try
                    {
                        DataLayer dl = new DataLayer();
                        dl.ExecuteNonQuery(insertStatement, CommandType.Text, new List<SqlParameter>());

                        // Reset all Modal controls.
                        this.ddlUploadDealer.SelectedIndex = 0;
                        this.ddlUploadCategory.SelectedIndex = 0;
                        this.txtUploadPONumber.Text = string.Empty;
                        // By default/security reasons, FileUpload control will be cleared.
                        this.reUploadDescription.Content = "";
                        this.txtUploadReceivedDate.Text = string.Empty;

                        // Close Modal/Dialog.
                        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('uploadDocumentModal')", true);

                        // Rebind to RadGrid.
                        // Check to see if QueryString has DealerID, IF so, then Rebind() accordingly, ELSE show all records.
                        if (Request.QueryString["DealerID"] == null)
                            dealerID = 0;
                        else
                            dealerID = Convert.ToInt32(Request.QueryString["DealerID"]);

                        this.rgDealerDocuments.Rebind();

                        // Show Success Message.
                        VisibleUploadSuccess();

                        // Reset all Upload Modal/Dialog Controls.
                        ResetUploadControls();
                    }
                    catch (Exception ex)
                    {
                        // Delete Document record if DealerDocument record fails insertion.
                        var deleteDocumentRecord = (from ddr in idc.Documents
                                                    where ddr.DocumentID == documentID
                                                    select ddr).FirstOrDefault();

                        idc.Documents.DeleteOnSubmit(deleteDocumentRecord);
                        idc.SubmitChanges();

                        // Show Error Message.
                        VisibleUploadError(ex.Message.ToString());
                        return;
                    }
                }
                catch (Exception ex)
                {
                    VisibleUploadError(ex.Message.ToString());
                    return;
                }
            }
            else
            {
                return;
            }
        }

        protected void btnUploadDocumentCancel_Click(object sender, EventArgs e)
        {
            // Reset all messages.
            InvisibleUploadError();
            InvisibleUploadSuccess();
            InvisibleEditError();
            InvisibleEditSuccess();

            // Reset all controls.
            ResetUploadControls();
        }

        public void ResetUploadControls()
        {
            this.ddlUploadDealer.SelectedIndex = 0;
            this.ddlUploadCategory.SelectedIndex = 0;
            this.txtUploadPONumber.Text = string.Empty;
            // By default/security reasons, FileUpload control will be cleared.
            this.reUploadDescription.Content = "";
            this.txtUploadReceivedDate.Text = string.Empty;
        }
        #endregion

        #region EDIT/UPDATE DOCUMENT.
        protected void btnEditDocument_Click(object sender, EventArgs e)
        {
            // Reset Error/Success Messages.
            InvisibleUploadError();
            InvisibleUploadSuccess();
            InvisibleEditError();
            InvisibleEditSuccess();

            if (this.Page.IsValid)
            {
                int documentID = 0;

                // Set value to update/edit based on Control values.
                int.TryParse(this.ddlEditDealer.SelectedValue, out dealerID);
                int.TryParse(this.hfDocumentID.Value, out documentID);
                int categoryID = Convert.ToInt32(this.ddlEditCategory.SelectedValue);
                string poNumber = (this.txtEditPONumber.Text.Trim() == "") ? poNumber = null : poNumber = this.txtEditPONumber.Text.Trim();
                string description = this.reEditDescription.Content;
                DateTime receivedDate = Convert.ToDateTime(this.txtEditReceivedDate.Text);

                // Get Document record from DB.
                var documentRecord = (from d in idc.Documents
                                      where d.DocumentID == documentID
                                      select d).FirstOrDefault();

                if (documentRecord == null) return;

                // Get DealerDocument record from DB.
                var dealerDocumentRecord = (from dd in idc.DealerDocuments
                                            where dd.DocumentID == documentID && dd.DealerID == dealerID
                                            select dd).FirstOrDefault();

                if (dealerDocumentRecord == null) return;

                if (cbEditDelete.Checked == true)
                {
                    documentRecord.Active = false;
                }
                else
                {
                    // Update the Document and DealerDocument records were necessary.
                    dealerDocumentRecord.DealerID = dealerID;
                    documentRecord.DocumentCategory = this.ddlEditCategory.Text;
                    dealerDocumentRecord.CategoryID = categoryID;
                    dealerDocumentRecord.PONumber = poNumber;
                    if (this.fuEditDocument.HasFile)
                    {
                        documentRecord.FileName = this.fuEditDocument.FileName;
                        documentRecord.DocumentContent = ReadAllBytes(this.fuEditDocument.FileContent);
                        documentRecord.MIMEType = GetMIMEType(this.fuEditDocument);
                    }
                    documentRecord.Description = description;
                    dealerDocumentRecord.ReceivedDate = receivedDate;
                }

                try
                {
                    idc.SubmitChanges();

                    // Reset all Modal controls.
                    hfDocumentID.Value = "0";
                    cbEditDelete.Checked = false;
                    this.ddlEditDealer.SelectedIndex = 0;
                    this.ddlEditCategory.SelectedIndex = 0;
                    this.txtEditPONumber.Text = string.Empty;
                    this.lblOldDocument.Text = string.Empty;
                    // By default/security reasons, FileUpload control will be cleared.
                    this.reEditDescription.Content = "";
                    this.txtEditReceivedDate.Text = string.Empty;

                    // Close Modal/Dialog.
                    ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('editDocumentModal')", true);

                    // Rebind to RadGrid.
                    // Check to see if QueryString has DealerID, IF so, then Rebind() accordingly, ELSE show all records.
                    if (Request.QueryString["DealerID"] == null)
                        dealerID = 0;
                    else
                        dealerID = Convert.ToInt32(Request.QueryString["DealerID"]);

                    this.rgDealerDocuments.Rebind();

                    // Show Success Message.
                    VisibleEditSuccess();

                    // Reset all Edit Modal/Dialog Controls
                    ResetEditControls();
                }
                catch (Exception ex)
                {
                    VisibleEditError(ex.Message.ToString());
                    return;
                }
            }
            else
            {
                return;
            }
        }

        protected void btnEditDocumentCancel_Click(object sender, EventArgs e)
        {
            // Reset all messages.
            InvisibleUploadError();
            InvisibleUploadSuccess();
            InvisibleEditError();
            InvisibleEditSuccess();

            // Reset all controls.
            ResetEditControls();
        }

        public void ResetEditControls()
        {
            // Reset all controls.
            hfDocumentID.Value = "0";
            cbEditDelete.Checked = false;
            this.ddlEditDealer.SelectedIndex = 0;
            this.ddlEditCategory.SelectedIndex = 0;
            this.txtEditPONumber.Text = string.Empty;
            this.lblOldDocument.Text = string.Empty;
            // By default/security reasons, FileUpload control will be cleared.
            this.reEditDescription.Content = "";
            this.txtEditReceivedDate.Text = string.Empty;
        }
        #endregion

        #region ERROR/SUCCESS MESSAGE.
        private void InvisibleUploadError()
        {
            this.uploadError.Visible = false;
            this.uploadErrorMessage.InnerHtml = string.Empty;
        }

        private void VisibleUploadError(string errormessage)
        {
            this.uploadError.Visible = true;
            this.uploadErrorMessage.InnerHtml = errormessage;
        }

        private void InvisibleEditError()
        {
            this.editError.Visible = false;
            this.editErrorMessage.InnerHtml = string.Empty;
        }

        private void VisibleEditError(string errormessage)
        {
            this.editError.Visible = true;
            this.editErrorMessage.InnerHtml = errormessage;
        }

        private void InvisibleUploadSuccess()
        {
            this.uploadSuccess.Visible = false;
        }

        private void VisibleUploadSuccess()
        {
            this.uploadSuccess.Visible = true;
        }

        private void InvisibleEditSuccess()
        {
            this.editSuccess.Visible = false;
        }

        private void VisibleEditSuccess()
        {
            this.editSuccess.Visible = true;
        }
        #endregion

        #region GLOBAL FUNCTIONS.
        /// <summary>
        /// Returns the Byte Array for the documents being Uploaded/Edited.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] ReadAllBytes(Stream stream)
        {
            stream.Position = 0;
            byte[] buffer = new byte[stream.Length];
            for (int totalBytesCopied = 0; totalBytesCopied < stream.Length; )
                totalBytesCopied += stream.Read(buffer, totalBytesCopied, Convert.ToInt32(stream.Length) - totalBytesCopied);
            return buffer;
        }

        /// <summary>
        /// Returns MIME Type for documents that are being Uploaded/Edited.
        /// </summary>
        /// <param name="fu"></param>
        /// <returns></returns>
        protected string GetMIMEType(FileUpload fu)
        {
            HttpPostedFile hpf = fu.PostedFile;

            string fileExtension = Path.GetFileName(hpf.FileName).Split('.')[1];
            string mimeType = null;

            switch (fileExtension.ToLower())
            {
                case "doc":
                    mimeType = "application/vnd.ms-word";
                    break;
                case "docx":
                    mimeType = "application/vnd.ms-word";
                    break;
                case "xls":
                    mimeType = "application/vnd.ms-excel";
                    break;
                case "xlsx":
                    mimeType = "application/vnd.ms-excel";
                    break;
                case "pdf":
                    mimeType = "application/pdf";
                    break;
                default:
                    mimeType = "image/jpeg";
                    break;
            }

            return mimeType;
        }

        protected void btnReturnTo_Click(object sender, EventArgs e)
        {
            Response.Redirect("ICCareDealerMaintenance.aspx");
        }
        #endregion
    }
}