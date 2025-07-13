using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

using Instadose;
using Instadose.Data;
using Instadose.Integration;

public partial class InformationFinder_Details_EditBillingGroup : System.Web.UI.Page
{
    int AccountID;
    int BillingGroupID;
    string UserName = "Unknown";
    Account account;
    BillingGroup editBillingGroup;

    // Create the database reference
    AppDataContext adc = new AppDataContext();
    InsDataContext idc = new InsDataContext();

    protected void Page_Load(object sender, EventArgs e)
    {
        InvisibleErrors();

        if (!int.TryParse(Request.QueryString["AccountID"], out AccountID))
        {
            Page.Response.Redirect("../Default.aspx");
            return;
        }
        account = idc.Accounts.Where(r => r.AccountID == AccountID).FirstOrDefault();

        if (!int.TryParse(Request.QueryString["BillingGroupID"], out BillingGroupID))
        {
            BillingGroupID = 0;
        }

        if (BillingGroupID > 0)
        {
            editBillingGroup = idc.BillingGroups.Where(r => r.BillingGroupID == BillingGroupID).FirstOrDefault();
        }

        try
        {
            this.UserName = User.Identity.Name.Split('\\')[1];
        }
        catch { this.UserName = "Unknown"; }

        // Check if page is PostBack
        if (!IsPostBack)
        {            
            this.LoadFillPageInfo();
        }

    }    

    private void LoadFillPageInfo()
    {
        string defaultCompanyName, defaultContactName;
        defaultCompanyName = account.CompanyName;
        defaultContactName = "";

        if (account.BillingGroup != null)
        {
            defaultCompanyName = account.BillingGroup.CompanyName;
            defaultContactName = account.BillingGroup.ContactName;
        }

        // ------------ load default info for inserting new billing group --------------------//
        lblBillingGroupID.Text = editBillingGroup != null ? editBillingGroup.BillingGroupID.ToString() : "";
        lblAccountNo.Text = AccountID.ToString();
        txtCompanyName.Text = defaultCompanyName;
        txtContactName.Text = defaultContactName;

        rowEDI.Visible = account.BrandSource.BrandName == "IC Care" ? true : false;

        LoadDDLCountry(ddlCountryB);
        ddlCountryB.SelectedValue = "1";
        LoadDDLState(ddlStateB, ddlCountryB.SelectedValue);

        if( (account.CustomerType.CustomerTypeCode == "DEMO" || account.CustomerType.CustomerTypeCode == "TEST")
            || (account.CustomerGroupID != null && (account.CustomerGroupID.Value == 3 || account.CustomerGroupID.Value == 4))
          )
        {
            chkBoxInvDeliveryPrintMail.Checked = false;
            chkBoxInvDeliveryDoNotSend.Checked = true;
            chkBoxInvDeliveryDoNotSend.Enabled = true;                    
        }
        else
        {
            chkBoxInvDeliveryPrintMail.Checked = true;
            chkBoxInvDeliveryDoNotSend.Checked = false;
            chkBoxInvDeliveryDoNotSend.Enabled = false;                   
        }

        // --------------- Load billing address ---------------------------------//
        LoadBillingAddress();

        // If edit a billing group, then load info of this billing group
        if (editBillingGroup != null)
        {
            // load billing info panel
            txtCompanyName.Text = editBillingGroup.CompanyName;
            txtContactName.Text = editBillingGroup.ContactName;
            txtPOno.Text = editBillingGroup.PONumber;

            FillAllAddressControls(editBillingGroup.Address);            

            // load invoice delivery method panel
            chkBoxInvDeliveryPrintMail.Checked = editBillingGroup.useMail;

            if (editBillingGroup.useEmail1 || editBillingGroup.useEmail2)
            {
                chkBoxInvDeliveryEmail.Checked = true;

                //txtInvDeliveryPrimaryEmail.Enabled = true;
                //txtInvDeliverySecondaryEmail.Enabled = true;

                //txtInvDeliveryPrimaryEmail.Text = editBillingGroup.Email1;
                //txtInvDeliverySecondaryEmail.Text = editBillingGroup.Email2;
            }
            else
            {
                chkBoxInvDeliveryEmail.Checked = false;

                //txtInvDeliveryPrimaryEmail.Enabled = false;
                //txtInvDeliverySecondaryEmail.Enabled = false;

                //txtInvDeliveryPrimaryEmail.Text = "";
                //txtInvDeliverySecondaryEmail.Text = "";
            }
            txtInvDeliveryPrimaryEmail.Text = string.IsNullOrEmpty(editBillingGroup.Email1) ? "" : editBillingGroup.Email1;
            txtInvDeliverySecondaryEmail.Text = string.IsNullOrEmpty(editBillingGroup.Email2) ? "" : editBillingGroup.Email2;

            if (editBillingGroup.useFax)
            {
                chkBoxInvDeliveryFax.Checked = true;

                //txtInvDeliveryPrimaryFax.Enabled = true;

                //txtInvDeliveryPrimaryFax.Text = editBillingGroup.Fax;
            }
            else
            {
                chkBoxInvDeliveryFax.Checked = false;

                //txtInvDeliveryPrimaryFax.Enabled = false;

                //txtInvDeliveryPrimaryFax.Text = "";
            }
            txtInvDeliveryPrimaryFax.Text = string.IsNullOrEmpty(editBillingGroup.Fax) ? "" : editBillingGroup.Fax;

            chkBoxInvDeliveryEDI.Checked = editBillingGroup.useEDI;
            string clientName = "";
            if (editBillingGroup.EDIClientID.HasValue)
            {
                clientName = idc.Clients.Where(d => d.ClientID == editBillingGroup.EDIClientID.Value).Select(d => d.ClientName).FirstOrDefault();
            }
            txtInvDeliveryEDIClientID.Text = clientName;

            if (editBillingGroup.useSpecialDelivery)
            {
                chkBoxInvDeliveryUpload.Checked = true;

                //fileUploadInvDeliveryUpload.Enabled = true;
                //txtInvDeliveryUploadInstruction.Enabled = true;

                //txtInvDeliveryUploadInstruction.Text = editBillingGroup.SpecialDeliveryText;
            }
            else
            {
                chkBoxInvDeliveryUpload.Checked = false;

                //fileUploadInvDeliveryUpload.Enabled = false;
                //txtInvDeliveryUploadInstruction.Enabled = false;

                //txtInvDeliveryUploadInstruction.Text = "";
            }
            txtInvDeliveryUploadInstruction.Text = string.IsNullOrEmpty(editBillingGroup.SpecialDeliveryText) ? "" : editBillingGroup.SpecialDeliveryText;

            chkBoxInvDeliveryDoNotSend.Checked = !editBillingGroup.DeliverInvoice;
            if ((account.CustomerType.CustomerTypeCode == "DEMO" || account.CustomerType.CustomerTypeCode == "TEST")
                || (account.CustomerGroupID != null && (account.CustomerGroupID.Value == 3 || account.CustomerGroupID.Value == 4))
                )
            {
                chkBoxInvDeliveryDoNotSend.Enabled = true;
            }
            else
            {
                chkBoxInvDeliveryDoNotSend.Enabled = false;
            }
            
        }
    }        

    protected void LoadDDLCountry(DropDownList DDLName)
    {
        //InsDataContext  idc = new InsDataContext ();
        DDLName.DataSource = (from a in idc.Countries
                              orderby a.CountryName
                              select a);
        DDLName.DataBind();
        ListItem firstItem = new ListItem("  -- Select Country --", "0");
        DDLName.Items.Insert(0, firstItem);
        DDLName.SelectedIndex = 0;
    }

    protected void LoadDDLState(DropDownList DDLName, string SelectedCountryID)
    {
        ListItem firstItem = new ListItem("  -- Select State --", "0");
        var states = (from a in idc.States
                      orderby a.CountryID, a.StateAbbrev
                      select a);
        var stateList = states.Select(a => new { a.StateID, StateAbbName = a.StateAbbrev.ToString() + " - " + a.StateName.ToString() }).ToList();
        if (SelectedCountryID != "0")
        {
            stateList = states.Where(a => a.CountryID == int.Parse(SelectedCountryID))
                             .Select(a => new { a.StateID, StateAbbName = a.StateAbbrev.ToString() + " - " + a.StateName.ToString() }).ToList();
        }
        DDLName.DataSource = stateList;
        DDLName.DataBind();
        DDLName.Items.Insert(0, firstItem);
    }

    private void LoadBillingAddress()
    {
        // Display All addresses of an account
        var billingAddressess = (from a in idc.Addresses
                                join s in idc.States on a.StateID equals s.StateID
                                join c in idc.Countries on a.CountryID equals c.CountryID
                                where a.AccountID == this.AccountID
                                && a.AXAddressType.AddressTypeName == "Billing"
                                orderby a.Address1
                                select new
                                {
                                    a.AddressID,                                    
                                    a.Address1,
                                    a.Address2,
                                    a.Address3,
                                    a.City,
                                    StateAbbName = s.StateAbbrev + " - " + s.StateName,
                                    c.CountryName,
                                    a.PostalCode
                                }).ToArray();

        gv_AddressList.DataSource = billingAddressess;
        gv_AddressList.DataBind();

    }

    private void FillAllAddressControls(Address pAddress)
    {
        if(pAddress != null)
        {
            txtAddress1B.Text = pAddress.Address1;
            txtAddress2B.Text = pAddress.Address2;
            txtAddress3B.Text = pAddress.Address3;
            ddlCountryB.SelectedValue = pAddress.CountryID.ToString();
            LoadDDLState(ddlStateB, ddlCountryB.SelectedValue);
            txtCityB.Text = pAddress.City;
            ddlStateB.SelectedValue = pAddress.StateID.ToString();
            txtPostalB.Text = pAddress.PostalCode;
        }        
    }

    protected void ddlCountryOnSelectedIndexChange(object sender, System.EventArgs e)
    {
        LoadDDLState(ddlStateB, ddlCountryB.SelectedValue);
    }

    protected void btnExistingAddress_Click(object sender, EventArgs e)
    {
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('assignedAddressDialog')", true);
    }

    protected void btnAssignedAddress_Click(object sender, EventArgs e)
    {
        foreach (GridViewRow row in this.gv_AddressList.Rows)
        {
            RadioButton radButton = (RadioButton)row.FindControl("radSelectAddress");

            if (radButton.Checked)
            {                
                Label lblAddressID = (Label)row.FindControl("lblAddressID");
                Address address = idc.Addresses.Where(r => r.AddressID == int.Parse(lblAddressID.Text)).FirstOrDefault();

                FillAllAddressControls(address);
                
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('assignedAddressDialog')", true);
                break;
            }
        }
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {        
        if (!chkBoxInvDeliveryPrintMail.Checked && !chkBoxInvDeliveryEmail.Checked && !chkBoxInvDeliveryFax.Checked && !chkBoxInvDeliveryUpload.Checked && !chkBoxInvDeliveryDoNotSend.Checked )
        {
            //=-=-=-= Show Error message in validation summary.
            CustomValidator val = new CustomValidator();
            val.ErrorMessage = string.Format("{0} - {1}", "Invoice Delivery Method", "Must check at least one of delivery method check box.");
            val.IsValid = false;
            val.ValidationGroup = "CSRegisterForm";
            this.Page.Validators.Add(val);
            return;
        }

        int returnBillingGroupID = SaveBillingGroup(this.BillingGroupID == 0? true:false, this.AccountID);

        if (returnBillingGroupID == 0)
        {
            this.VisibleErrors("Billing Group creation errors.");
            return;
        }
        
        // Redirect back to Account Detail page
        Response.Redirect("Account.aspx?ID=" + this.AccountID.ToString() + "#BillingGroup_Tab");        
    }

    protected void btnCancel_Click(object sender, EventArgs e)
    {
        Page.Response.Redirect("Account.aspx?ID=" + this.AccountID.ToString() + "#BillingGroup_Tab");
    }

    private void InvisibleErrors()
    {
        // Reset submission form error message      
        this.errorMsg.InnerHtml = "";
        this.errors.Visible = false;
    }

    private void VisibleErrors(string error)
    {
        this.errorMsg.InnerHtml = error;
        this.errors.Visible = true;
    }

    public int SaveBillingGroup(bool isNew, int AccountID)
    {
        int returnBillingGroupID = 0;

        try
        {           
            if (isNew)
            {
                int billingAddressID = 0;
                if (SaveAddress(ref billingAddressID, AccountID))
                {
                    BillingGroup BGroup = new BillingGroup();

                    BGroup.AccountID = AccountID;
                    BGroup.BillingAddressID = billingAddressID;
                    BGroup.CompanyName = txtCompanyName.Text.Trim();
                    BGroup.ContactName = txtContactName.Text.Trim();
                    BGroup.useMail = chkBoxInvDeliveryPrintMail.Checked;
                    BGroup.useEmail1 = chkBoxInvDeliveryEmail.Checked;
                    BGroup.Email1 = string.IsNullOrEmpty(txtInvDeliveryPrimaryEmail.Text) ? null : txtInvDeliveryPrimaryEmail.Text.Trim();
                    BGroup.useEmail2 = chkBoxInvDeliveryEmail.Checked && (!string.IsNullOrEmpty(txtInvDeliverySecondaryEmail.Text));
                    BGroup.Email2 = string.IsNullOrEmpty(txtInvDeliverySecondaryEmail.Text) ? null : txtInvDeliverySecondaryEmail.Text.Trim();
                    BGroup.useFax = chkBoxInvDeliveryFax.Checked;
                    BGroup.Fax = string.IsNullOrEmpty(txtInvDeliveryPrimaryFax.Text) ? null : txtInvDeliveryPrimaryFax.Text.Trim();
                    BGroup.useEDI = chkBoxInvDeliveryEDI.Checked;
                    //BGroup.EDIClientID = 1; manually update in the back end
                    BGroup.useSpecialDelivery = chkBoxInvDeliveryUpload.Checked;
                    BGroup.SpecialDeliveryText = string.IsNullOrEmpty(txtInvDeliveryUploadInstruction.Text) ? null : txtInvDeliveryUploadInstruction.Text.Trim();                    
                    BGroup.DeliverInvoice = !chkBoxInvDeliveryDoNotSend.Checked;
                    BGroup.PONumber = MAS.CleanString(this.txtPOno.Text.Trim(), 15);
                    BGroup.ACMSyncBit = 1;

                    bool uploadResult = UploadInvDeliveryMethodInstruction(AccountID);

                    if (uploadResult)
                    {
                        idc.BillingGroups.InsertOnSubmit(BGroup);
                        idc.SubmitChanges();
                        // get the @@identity 
                        returnBillingGroupID = BGroup.BillingGroupID;                        
                    }
                }
            }
            else
            {
                if (editBillingGroup != null)
                {
                    // update address table
                    int billingAddressID = editBillingGroup.BillingAddressID;
                    SaveAddress(ref billingAddressID, AccountID);

                    // update BillingGroup                    
                    editBillingGroup.CompanyName = txtCompanyName.Text.Trim();
                    editBillingGroup.ContactName = txtContactName.Text.Trim();
                    editBillingGroup.useMail = chkBoxInvDeliveryPrintMail.Checked;
                    editBillingGroup.useEmail1 = chkBoxInvDeliveryEmail.Checked;
                    editBillingGroup.Email1 = string.IsNullOrEmpty(txtInvDeliveryPrimaryEmail.Text) ? null : txtInvDeliveryPrimaryEmail.Text.Trim();
                    editBillingGroup.useEmail2 = chkBoxInvDeliveryEmail.Checked && (!string.IsNullOrEmpty(txtInvDeliverySecondaryEmail.Text));
                    editBillingGroup.Email2 = string.IsNullOrEmpty(txtInvDeliverySecondaryEmail.Text) ? null : txtInvDeliverySecondaryEmail.Text.Trim();
                    editBillingGroup.useFax = chkBoxInvDeliveryFax.Checked;
                    editBillingGroup.Fax = string.IsNullOrEmpty(txtInvDeliveryPrimaryFax.Text) ? null : txtInvDeliveryPrimaryFax.Text.Trim(); ;
                    editBillingGroup.useEDI = chkBoxInvDeliveryEDI.Checked;
                    //BGroup.EDIClientID = 1; manually update in the back end
                    editBillingGroup.useSpecialDelivery = chkBoxInvDeliveryUpload.Checked;
                    editBillingGroup.SpecialDeliveryText = string.IsNullOrEmpty(txtInvDeliveryUploadInstruction.Text) ? null : txtInvDeliveryUploadInstruction.Text.Trim();                    
                    editBillingGroup.DeliverInvoice = !chkBoxInvDeliveryDoNotSend.Checked;
                    editBillingGroup.PONumber = MAS.CleanString(this.txtPOno.Text.Trim(), 15);

                    bool uploadResult = UploadInvDeliveryMethodInstruction(AccountID);

                    // update account PO# if we are editting account level billing group
                    if(account.BillingGroupID == editBillingGroup.BillingGroupID)
                    {
                        account.RenewalPONumber = editBillingGroup.PONumber;
                    }

                    if (uploadResult)
                        idc.SubmitChanges();

                    returnBillingGroupID = editBillingGroup.BillingGroupID;
                }                
            }
        }
        catch
        {            
        }
        return returnBillingGroupID;
    }

    public bool SaveAddress(ref int pBillingAddressID, int pAccountID)
    {
        bool returnCommit = false;
        int billingAddressID = pBillingAddressID;
        try
        {
            if (billingAddressID > 0)
            {
                Address ABilling = (from a in idc.Addresses where a.AddressID == billingAddressID select a).FirstOrDefault();

                if (ABilling != null)
                {
                    ABilling.AccountID = pAccountID;
                    //ABilling.AddressTypeID = (from at in idc.AXAddressTypes where at.AddressTypeName == "Billing" select at.AddressTypeID).FirstOrDefault();
                    ABilling.Address1 = txtAddress1B.Text.Trim();
                    ABilling.Address2 = txtAddress2B.Text.Trim();
                    ABilling.Address3 = txtAddress3B.Text.Trim();
                    ABilling.City = txtCityB.Text.Trim();
                    ABilling.StateID = int.Parse(ddlStateB.SelectedValue.ToString());
                    ABilling.PostalCode = txtPostalB.Text.Trim();
                    ABilling.CountryID = int.Parse(ddlCountryB.SelectedValue.ToString());

                    idc.SubmitChanges();
                }
            }
            else
            {
                Address ABilling = new Address();

                ABilling.AccountID = pAccountID;
                ABilling.AddressTypeID = (from at in idc.AXAddressTypes where at.AddressTypeName == "Billing" select at.AddressTypeID).FirstOrDefault();
                ABilling.Address1 = txtAddress1B.Text.Trim();
                ABilling.Address2 = txtAddress2B.Text.Trim();
                ABilling.Address3 = txtAddress3B.Text.Trim();
                ABilling.City = txtCityB.Text.Trim();
                ABilling.StateID = int.Parse(ddlStateB.SelectedValue.ToString());
                ABilling.PostalCode = txtPostalB.Text.Trim();
                ABilling.CountryID = int.Parse(ddlCountryB.SelectedValue.ToString());

                idc.Addresses.InsertOnSubmit(ABilling);
                idc.SubmitChanges();
                // get the @@identity 
                pBillingAddressID = ABilling.AddressID;
            }
            returnCommit = true;
        }
        catch (Exception ex)
        {            
        }
        return returnCommit;
    }

    private bool UploadInvDeliveryMethodInstruction(int AccountID)
    {
        if (chkBoxInvDeliveryUpload.Checked && fileUploadInvDeliveryUpload.HasFile)
        {
            try
            {
                // Get Upload infor and convert to byte format        
                HttpPostedFile hpf = fileUploadInvDeliveryUpload.PostedFile;
                string attachmentFile = Path.GetFileName(hpf.FileName);

                // CREATE the Byte Array.
                Byte[] byteImage = new Byte[hpf.ContentLength];

                // Read uploaded file to byte array.
                hpf.InputStream.Read(byteImage, 0, hpf.ContentLength);
                var arrAttachment = attachmentFile.Split('.');
                string fileExtension = arrAttachment[arrAttachment.Length - 1];
                string mimeType = null;

                switch (fileExtension)
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
                    DocumentGUID = Guid.NewGuid(),
                    FileName = Path.GetFileName(hpf.FileName),
                    DocumentContent = byteImage,
                    MIMEType = mimeType,
                    DocumentCategory = "Invoice Delivery Instruction",
                    Description = string.IsNullOrEmpty(txtInvDeliveryUploadInstruction.Text) ? null : txtInvDeliveryUploadInstruction.Text.Trim(),
                    AccountID = AccountID,
                    CreatedBy = this.UserName,
                    CreatedDate = DateTime.Now,
                    Active = true
                };

                // SAVE the Document Record.
                idc.Documents.InsertOnSubmit(doc);
                idc.SubmitChanges();
            }
            catch
            {
                return false;
            }
        }
        return true;
    }

    
}