using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Mail;
using Instadose.Data;
using Instadose.Security;
using System.Text.RegularExpressions;

public partial class InformationFinder_Details_EditLocation : System.Web.UI.Page
{
    private int locationID = 0;
    private int accountID = 0;

    private Account account;
    private Location location;

    private DateTime tmpDate;

    // Create the database reference
    InsDataContext idc = new InsDataContext();
    
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!int.TryParse(Request.QueryString["accountID"], out accountID))
        {
            Page.Response.Redirect("../Default.aspx");
            return;
        }
        if (!int.TryParse(Request.QueryString["locationID"], out locationID))
        {
            locationID = 0;
        }

        account = (from act in idc.Accounts
                     where act.AccountID == this.accountID
                     select act).FirstOrDefault();

        location = (from loc in idc.Locations
                      where loc.LocationID == this.locationID
                      select loc).FirstOrDefault();
                
        btnSave.Enabled = account.Active;
        

        ResetValidateNote();

        if (account.UseLocationBilling == true)
            //disable the copy account billing info checkbox
            chkCopyAccountBilling.Enabled = false;
        else
            chkCopyAccountBilling.Enabled = true;

        if (!this.IsPostBack)
        {
            InitiateAllControls();
            SetControlsDefault();
            SetValuesToControls();
            InitialSetDisableInputControls();            
        }
    }

    private void ResetValidateNote()
    {
        this.lblLocationNameValidate.Visible = false;
        this.lblTimeZoneValidate.Visible = false;

        /// 9/2013 W.Kakemoto - Billing company now required to CreateOrders for this location.
        /// 
        this.txtBCompanyValidate.Visible = false;

        this.lblBFirstNameValidate.Visible = false;
        this.lblBLastNameValidate.Visible = false;
        this.lblBTelephoneValidate.Visible = false;
        this.lblBCountryValidate.Visible = false;
        this.lblBAddress1Validate.Visible = false;
        this.lblBCityValidate.Visible = false;
        this.lblBStateValidate.Visible = false;
        this.lblBPostalCodeValidate.Visible = false;

        /// 9/2013 W.Kakemoto - Shipping company now required to CreateOrders for this location.
        /// 
        this.txtSCompanyValidate.Visible = false;

        this.lblSFirstNameValidate.Visible = false;
        this.lblSLastNameValidate.Visible = false;
        this.lblSTelephoneValidate.Visible = false;
        this.lblSCountryValidate.Visible = false;
        this.lblSAddress1Validate.Visible = false;
        this.lblSCityValidate.Visible = false;
        this.lblSStateValidate.Visible = false;
        this.lblSPostalCodeValidate.Visible = false;
        this.lblHDNValidate.Visible = false;
        this.lblProductGroupValidate.Visible  = false;
        this.ddlLabelValidate.Visible = false;

        this.error.Visible = false;
    }

    private void InitialSetEnableInputControls()
    {

    }

    private void InitialSetDisableInputControls()
    {
        this.HDNSection.Visible = account.HDNNotify;
        this.chkBoxIsDefaultLocation.Enabled = !(location != null && location.IsDefaultLocation);
        this.GroupSection.Visible = (this.locationID > 0);
        this.BillingGroupSection.Visible = account.UseLocationBilling;
    }

    private void SetControlsDefault()
    {
        try
        {
            this.chkBoxActive.Checked = true;
            this.ddlBCountry.SelectedValue = "1";
            this.ddlSCountry.SelectedValue = "1";
            this.ddlCountryB.SelectedValue = "1";

            // ------ Set default ProductGroupID and LabelID based upon what has been set in Account ------//
            var AccountPGDetail = (from acct in idc.AccountPGDetails  
                                            where acct.AccountID == this.accountID 
                                            select acct ).FirstOrDefault();
            if (AccountPGDetail != null)
            {
                 this.ddlProductGroup.SelectedValue = AccountPGDetail.ProductGroupID.ToString();
                 this.ddlProductGroup.DataBind();
                 this.ddlLabel.SelectedValue = AccountPGDetail.LabelID.ToString();
            }                                  

            InitiateBStateControl();
            InitiateSStateControl();
            
        }
        catch { }

    }

    private void InitiateAllControls()
    {
        // DONE by sqldatasource controls
        this.ddlBCountry.DataBind();
        this.ddlSCountry.DataBind();
        this.ddlCountryB.DataBind();

        this.ddlSchedule.DataBind();
        ListItem firstItem = new ListItem("  -- Select Schedule Name --", "0");
        ddlSchedule.Items.Insert(0, firstItem);
        ddlSchedule.SelectedIndex = 0;

        LoadBillingGroup();
    }

    private void LoadBillingGroup()
    {
        ListItem firstItem = new ListItem("  -- Select Billing Group --", "0");
        var billingGroups = (from a in idc.BillingGroups
                      join b in idc.Addresses on a.BillingAddressID equals b.AddressID
                      join c in idc.States on b.StateID equals c.StateID
                      join d in idc.Countries on b.CountryID equals d.CountryID
                      where a.AccountID == this.accountID
                      orderby a.ContactName
                      select new
                      {
                          a.BillingGroupID,
                          BillingGroupDetail = a.ContactName + ", " + a.CompanyName + ", " + b.Address1 + "," + b.City + "," + c.StateAbbrev + "," + d.CountryName
                      });
        
        ddlBillingGroup.DataSource = billingGroups;
        ddlBillingGroup.DataBind();
        ddlBillingGroup.Items.Insert(0, firstItem);
    }

    private void InitiateBStateControl()
    {
        try
        {
            this.ddlBState.DataBind();
        }
        catch { }
    }

    private void InitiateSStateControl()
    {
        try
        {
            this.ddlSState.DataBind();
        }
        catch { }
    }
   
    private void SetValuesToControls()
    {
        this.lblLocationID.Text = this.locationID == 0 ? "" : locationID.ToString();
        this.lblAccountID.Text = this.accountID == 0 ? "" : accountID.ToString();

        //1/2014 WK - if account has location billing - then display and load service dates for this location.
        if (account.UseLocationBilling)
        {
            divLocationServiceDates.Visible = true;
            txtServiceStartDate.Enabled = true;
            //txtServiceEndDate.Enabled = true;

            // Load Account BillingGroup by default @ add mode
            if (this.locationID == 0)
            {
                this.ddlBillingGroup.SelectedValue = account.BillingGroupID.HasValue ? account.BillingGroupID.Value.ToString() : "0";
                FillInvoiceDeliveryMethodControls();
            }
            else
            {
                if (location.BillingGroupID != null)
                {
                    this.ddlBillingGroup.SelectedValue = location.BillingGroupID.Value.ToString();
                    FillInvoiceDeliveryMethodControls();
                }
            }
        }
        else
        {
            divLocationServiceDates.Visible = false;
            txtServiceStartDate.Enabled = false;
            txtServiceEndDate.Enabled = false;
            this.ddlBillingGroup.SelectedValue = "0";
        }
        
        if (this.locationID == 0)    // adding mode
        {
            this.ddlLocationUOM.SelectedValue = (account.AccountUOM == "") ? "mrem" : account.AccountUOM;
            SetFocus(this.txtLocationName);            
        }
        else    // updating mode           
        {

            // If they have a sales rep and they are a reseller, then lock the controls
            if (account.SalesRepDistributor != null && location.IsDefaultLocation &&
                account.SalesRepDistributor.ChannelID == 8)
            {
                txtBAddress1.Enabled = false;
                txtBAddress2.Enabled = false;
                txtBAddress3.Enabled = false;
                txtBCity.Enabled = false;
                txtBCompany.Enabled = false;
                txtBEmailAddress.Enabled = false;
                txtBFax.Enabled = false;
                txtBFirstName.Enabled = false;
                txtBLastName.Enabled = false;
                txtBPostalCode.Enabled = false;
                txtBTelephone.Enabled = false;
                ddlBCountry.Enabled = false;
                ddlBPrefix.Enabled = false;
                ddlBState.Enabled = false;
            }

            this.chkBoxActive.Checked = location.Active;
            this.chkBoxIsDefaultLocation.Checked = location.IsDefaultLocation;
            this.txtLocationName.Text = location.LocationName;
            this.txtLocationCode.Text = location.LocationCode;
            this.ddlTimeZone.SelectedValue = (location.TimeZoneID == null ? "0" : location.TimeZoneID.ToString());
        
            //1/2014 WK - if account has location billing load service dates. 
            if (account.UseLocationBilling)
            {
                if (location.ContractStartDate != null)
                {
                    tmpDate = (DateTime)location.ContractStartDate;
                    txtServiceStartDate.Text = tmpDate.ToShortDateString();
                }

                if (location.ContractEndDate != null)
                {
                    tmpDate = (DateTime)location.ContractEndDate;
                    txtServiceEndDate.Text = tmpDate.ToShortDateString(); 
                }
            }
            
            this.ddlLocationUOM.SelectedValue = location.LocationUOM;

            this.txtBCompany.Text = (location.BillingCompany == null ? "" : location.BillingCompany.ToString());
            this.ddlBPrefix.SelectedValue = (location.BillingNamePrefix == null ? "" : location.BillingNamePrefix.ToString());
            this.txtBFirstName.Text = (location.BillingFirstName == null ? "" : location.BillingFirstName.ToString());
            this.txtBLastName.Text = (location.BillingLastName == null ? "" : location.BillingLastName.ToString());
            this.txtBTelephone.Text = (location.BillingTelephone == null ? "" : location.BillingTelephone.ToString());
            this.txtBFax.Text = (location.BillingFax == null ? "" : location.BillingFax.ToString());
            this.txtBEmailAddress.Text = (location.BillingEmailAddress == null ? "" : location.BillingEmailAddress);
            this.ddlBCountry.SelectedValue = location.BillingCountryID.ToString();
            this.txtBAddress1.Text = (location.BillingAddress1 == null ? "" : location.BillingAddress1.ToString());
            this.txtBAddress2.Text = (location.BillingAddress2 == null ? "" : location.BillingAddress2.ToString());
            this.txtBAddress3.Text = (location.BillingAddress3 == null ? "" : location.BillingAddress3.ToString());
            this.txtBCity.Text = (location.BillingCity == null ? "" : location.BillingCity.ToString());
            InitiateBStateControl();
            this.ddlBState.SelectedValue = location.BillingStateID.ToString();
            this.txtBPostalCode.Text = (location.BillingPostalCode == null ? "" : location.BillingPostalCode.ToString());

            this.txtSCompany.Text = (location.ShippingCompany == null ? "" : location.ShippingCompany.ToString());
            this.ddlSPrefix.SelectedValue = (location.ShippingNamePrefix == null ? "" : location.ShippingNamePrefix.ToString());
            this.txtSFirstName.Text = (location.ShippingFirstName == null ? "" : location.ShippingFirstName.ToString());
            this.txtSLastName.Text = (location.ShippingLastName == null ? "" : location.ShippingLastName.ToString());
            this.txtSTelephone.Text = (location.ShippingTelephone == null ? "" : location.ShippingTelephone.ToString());
            this.txtSFax.Text = (location.ShippingFax == null ? "" : location.ShippingFax.ToString());
            this.txtSEmailAddress.Text = (location.ShippingAddress1 == null ? "" : location.ShippingEmailAddress);
            this.ddlSCountry.SelectedValue = location.ShippingCountryID.ToString();
            this.txtSAddress1.Text = (location.ShippingAddress1 == null ? "" : location.ShippingAddress1.ToString());
            this.txtSAddress2.Text = (location.ShippingAddress2 == null ? "" : location.ShippingAddress2.ToString());
            this.txtSAddress3.Text = (location.ShippingAddress3 == null ? "" : location.ShippingAddress3.ToString());
            this.txtSCity.Text = (location.ShippingCity == null ? "" : location.ShippingCity.ToString());
            InitiateSStateControl();
            this.ddlSState.SelectedValue = location.ShippingStateID.ToString();
            this.txtSPostalCode.Text = (location.ShippingPostalCode == null ? "" : location.ShippingPostalCode.ToString());

            //this.chkBoxHasLabel.Checked = myLocation.HasLabel;
            this.ddlSchedule.SelectedValue = (location.ScheduleID  == null ? "0" : location.ScheduleID.ToString());

            LocationPGDetail myLocationPGDetail = (from locPGD in idc.LocationPGDetails
                                  where locPGD.LocationID == this.locationID && locPGD.AccountID == this.accountID
                                  select locPGD).FirstOrDefault();
            if (myLocationPGDetail != null)
            {
                this.ddlProductGroup.SelectedValue = myLocationPGDetail.ProductGroupID.ToString();
                this.ddlProductGroup.DataBind();
                this.ddlLabel.SelectedValue = myLocationPGDetail.LabelID.ToString();
            }

            //HDNConfig hdn = myLocation.HDNConfig;
            HDNConfig hdn = (from h in idc.HDNConfigs
                             join l in idc.Locations on h.HDNConfigID equals l.HDNConfigID
                             where l.LocationID == this.locationID
                             select h).FirstOrDefault();

            if (hdn != null)
            {
                string doseUOM = this.ddlLocationUOM.SelectedValue.ToLower();

                this.txtDLCurrent.Text = DoseToString(hdn.DeepCurrent, doseUOM);
                this.txtSLCurrent.Text = DoseToString(hdn.ShallowCurrent, doseUOM);
                this.txtEyeCurrent.Text = DoseToString(hdn.EyeCurrent, doseUOM);

                this.txtDLQuarter.Text = DoseToString(hdn.DeepQTD, doseUOM);
                this.txtSLQuarter.Text = DoseToString(hdn.ShallowQTD, doseUOM);
                this.txtEyeQuarter.Text = DoseToString(hdn.EyeQTD, doseUOM);

                this.txtDLYear.Text = DoseToString(hdn.DeepYTD, doseUOM);
                this.txtSLYear.Text = DoseToString(hdn.ShallowYTD, doseUOM);
                this.txtEyeYear.Text = DoseToString(hdn.EyeYTD, doseUOM);
            }            
            
            SetFocus(this.txtLocationName);
        }

    }

    /// <summary>
    /// Convert a dose number into a formatted string.
    /// </summary>
    /// <param name="dose"></param>
    /// <param name="uom"></param>
    /// <returns></returns>
    private string DoseToString(int? dose, string uom)
    {
        // Return is no dose is found.
        if (!dose.HasValue) return "0";

        // If no uom is not passed assume mrem.
        if (uom == null) uom = "mrem";

        // Convert the dose to mSv or keep mrem.
        double convertedDose = Instadose.Basics.ConvertDoseUnitOfMeasure((double)dose.Value, uom);

        // Return the formatted dose
        if (uom.ToLower() == "msv")
            return string.Format("{0:0.00}", convertedDose);
        else // mrem
            return string.Format("{0:0}", convertedDose);
    }

    protected void ddlBCountry_SelectedIndexChanged(object sender, EventArgs e)
    {
        InitiateBStateControl();
    }
    protected void ddlSCountry_SelectedIndexChanged(object sender, EventArgs e)
    {
        InitiateSStateControl();
    }

    protected void chkBoxSameAsBilling_CheckedChanged(object sender, EventArgs e)
    {
        if (chkBoxSameAsBilling.Checked)
        {
            this.txtSCompany.Text = this.txtBCompany.Text;
            this.ddlSPrefix.SelectedValue = this.ddlBPrefix.SelectedValue;
            this.txtSFirstName.Text = this.txtBFirstName.Text;
            this.txtSLastName.Text = this.txtBLastName.Text;
            this.txtSTelephone.Text = this.txtBTelephone.Text;
            this.txtSFax.Text = this.txtBFax.Text;
            this.txtSEmailAddress.Text = this.txtBEmailAddress.Text;
            this.ddlSCountry.SelectedValue = this.ddlBCountry.SelectedValue;
            this.txtSAddress1.Text = this.txtBAddress1.Text;
            this.txtSAddress2.Text = this.txtBAddress2.Text;
            this.txtSAddress3.Text = this.txtBAddress3.Text;
            this.txtSCity.Text = this.txtBCity.Text;
            InitiateSStateControl();
            this.ddlSState.SelectedValue = this.ddlBState.SelectedValue;
            this.txtSPostalCode.Text = this.txtBPostalCode.Text;
        }
    }

    protected void chkCopyAccountBilling_CheckedChanged(object sender, EventArgs e)
    {
        if (chkCopyAccountBilling.Checked)
        {
            try
            {
                //string str = this.accountID.ToString() + "  " + this.locationID.ToString();
                //Get location Info
                var loc = (from  lo in idc.Locations 
                                 where lo.AccountID == this.accountID && lo.IsDefaultLocation == true
                                 select lo).FirstOrDefault();
                
                if (loc != null && loc.LocationID > 0)
                {
                    if (loc.BillingCompany != null)
                        this.txtBCompany.Text = loc.BillingCompany.ToString();
                    if (loc.BillingNamePrefix != null)
                        this.ddlBPrefix.SelectedValue = loc.BillingNamePrefix.ToString();
                    if (loc.BillingFirstName != null)
                        this.txtBFirstName.Text = loc.BillingFirstName.ToString();
                    if (loc.BillingLastName != null)
                        this.txtBLastName.Text = loc.BillingLastName.ToString();
                    if (loc.BillingTelephone != null)
                        this.txtBTelephone.Text = loc.BillingTelephone.ToString();
                    if (loc.BillingFax != null)
                        this.txtBFax.Text = loc.BillingFax.ToString();
                    if (loc.BillingEmailAddress != null)
                        this.txtBEmailAddress.Text = loc.BillingEmailAddress.ToString();
                    //if (loc.BillingCountryID > 0)
                        this.ddlBCountry.SelectedValue = loc.BillingCountryID.ToString();
                    if (loc.BillingAddress1 != null)
                        this.txtBAddress1.Text = loc.BillingAddress1.ToString();
                    if (loc.BillingAddress2 != null)
                        this.txtBAddress2.Text = loc.BillingAddress2.ToString();
                    if (loc.BillingAddress3 != null)
                        this.txtBAddress3.Text = loc.BillingAddress3.ToString();
                    if (loc.BillingCity != null)
                        this.txtBCity.Text = loc.BillingCity.ToString();
                    //if (loc.BillingStateID > 0)
                        this.ddlBState.SelectedValue = loc.BillingStateID.ToString();
                    if (loc.BillingPostalCode != null)
                        this.txtBPostalCode.Text = loc.BillingPostalCode.ToString();

                }

            }
            catch
            {
                this.displayError("No such account found!");
            }

        }
    }  // end procedure

    protected void ddlBillingGroup_SelectedIndexChanged(object sender, EventArgs e)
    {
        FillInvoiceDeliveryMethodControls();                
    }

    protected void ddlCountryB_SelectedIndexChanged(object sender, EventArgs e)
    {
        LoadDDLState(ddlStateB, ddlCountryB.SelectedValue);
    }

    protected void LoadDDLState(DropDownList DDLName, string SelectedCountryID)
    {        
        ListItem firstItem = new ListItem("  -- Select State --", "0");
        var states = (from a in idc.States where a.CountryID == int.Parse(SelectedCountryID)
                      orderby a.StateAbbrev
                      select a);
        
        DDLName.DataSource = states;
        DDLName.DataBind();
        DDLName.Items.Insert(0, firstItem);
    }
    private void FillInvoiceDeliveryMethodControls()
    {
        int billingGroupID = int.Parse(this.ddlBillingGroup.SelectedValue);
        
        if (billingGroupID > 0)
        {
            BillingGroup billingGroup = idc.BillingGroups.Where(r => r.BillingGroupID == billingGroupID).FirstOrDefault();

            txtPOno.Text = billingGroup.PONumber;
            txtCompanyName.Text = billingGroup.CompanyName;
            txtContactName.Text = billingGroup.ContactName;

            txtAddress1B.Text = billingGroup.Address.Address1;
            txtAddress2B.Text = billingGroup.Address.Address2;
            txtAddress3B.Text = billingGroup.Address.Address3;
            ddlCountryB.SelectedValue = billingGroup.Address.CountryID.ToString();            
            LoadDDLState(ddlStateB, ddlCountryB.SelectedValue);
            txtCityB.Text = billingGroup.Address.City;
            ddlStateB.SelectedValue = billingGroup.Address.StateID.ToString();
            txtPostalB.Text = billingGroup.Address.PostalCode;

            // load invoice delivery method panel
            chkBoxInvDeliveryPrintMail.Checked = billingGroup.useMail;

            if (billingGroup.useEmail1 || billingGroup.useEmail2)
            {
                chkBoxInvDeliveryEmail.Checked = true;                
                //txtInvDeliveryPrimaryEmail.Text = billingGroup.Email1;
                //txtInvDeliverySecondaryEmail.Text = billingGroup.Email2;
            }
            else
            {
                chkBoxInvDeliveryEmail.Checked = false;                
                //txtInvDeliveryPrimaryEmail.Text = "";
                //txtInvDeliverySecondaryEmail.Text = "";
            }
            txtInvDeliveryPrimaryEmail.Text = string.IsNullOrEmpty(billingGroup.Email1) ? "" : billingGroup.Email1;
            txtInvDeliverySecondaryEmail.Text = string.IsNullOrEmpty(billingGroup.Email2) ? "" : billingGroup.Email2;

            if (billingGroup.useFax)
            {
                chkBoxInvDeliveryFax.Checked = true;                
                //txtInvDeliveryPrimaryFax.Text = billingGroup.Fax;
            }
            else
            {
                chkBoxInvDeliveryFax.Checked = false;                
                //txtInvDeliveryPrimaryFax.Text = "";
            }
            txtInvDeliveryPrimaryFax.Text = string.IsNullOrEmpty(billingGroup.Fax) ? "" : billingGroup.Fax;

            chkBoxInvDeliveryEDI.Checked = billingGroup.useEDI;
            string clientName = "";
            if (billingGroup.EDIClientID.HasValue)
            {
                clientName = idc.Clients.Where(d => d.ClientID == billingGroup.EDIClientID.Value).Select(d => d.ClientName).FirstOrDefault();
            }
            txtInvDeliveryEDIClientID.Text = clientName;

            if (billingGroup.useSpecialDelivery)
            {
                chkBoxInvDeliveryUpload.Checked = true;                
                //txtInvDeliveryUploadInstruction.Text = billingGroup.SpecialDeliveryText;
            }
            else
            {
                chkBoxInvDeliveryUpload.Checked = false;                
                //txtInvDeliveryUploadInstruction.Text = "";
            }
            txtInvDeliveryUploadInstruction.Text = string.IsNullOrEmpty(billingGroup.SpecialDeliveryText) ? "" : billingGroup.SpecialDeliveryText;

            chkBoxInvDeliveryDoNotSend.Checked = !billingGroup.DeliverInvoice;                        
        }
        else
        {
            txtPOno.Text = "";
            txtCompanyName.Text = "";
            txtContactName.Text = "";

            txtAddress1B.Text = "";
            txtAddress2B.Text = "";
            txtAddress3B.Text = "";
            ddlCountryB.SelectedValue = "1";
            LoadDDLState(ddlStateB, ddlCountryB.SelectedValue);
            txtCityB.Text = "";
            ddlStateB.SelectedValue = "0";
            txtPostalB.Text = "";

            // load invoice delivery method panel
            chkBoxInvDeliveryPrintMail.Checked =false;

            chkBoxInvDeliveryEmail.Checked = false;
            txtInvDeliveryPrimaryEmail.Text = "";
            txtInvDeliverySecondaryEmail.Text = "";

            chkBoxInvDeliveryFax.Checked = false;            
            txtInvDeliveryPrimaryFax.Text = "";            

            chkBoxInvDeliveryEDI.Checked = false;            
            txtInvDeliveryEDIClientID.Text = "";

            chkBoxInvDeliveryUpload.Checked = false;
            fileUploadInvDeliveryUpload.Dispose();            
            txtInvDeliveryUploadInstruction.Text = "";
           
            chkBoxInvDeliveryDoNotSend.Checked = false;
                        
        }
    }

    private Boolean PassValidation()
    {
        double myDouble;
        string errorString = "";

        if (this.txtLocationName.Text.Trim().Length == 0)
        {
            errorString = "Location Name is required.";
            this.lblLocationNameValidate.Visible = true;
            this.lblLocationNameValidate.InnerText = errorString;
            this.displayError(errorString);
            SetFocus(this.txtLocationName);
            return false;
        }

        /// 9/2013 W.Kakemoto - Billing company now required to CreateOrders for this location.
        /// 
        if (this.txtBCompany.Text.Trim().Length == 0)
        {
            errorString = "Billing Company is required.";
            this.txtBCompanyValidate.Visible = true;
            this.txtBCompanyValidate.InnerText = errorString;
            this.displayError(errorString);
            SetFocus(this.txtBCompany);
            return false;
        }

        if (this.txtBFirstName.Text.Trim().Length == 0)
        {
            if (this.account.BrandSource.BrandName != "IC Care")
            {
                errorString = "Billing First Name is required.";
                this.lblBFirstNameValidate.Visible = true;
                this.lblBFirstNameValidate.InnerText = errorString;
                this.displayError(errorString);
                SetFocus(this.txtBFirstName);
                return false;
            }          
        }

        if (this.txtBLastName.Text.Trim().Length == 0)
        {
            if (this.account.BrandSource.BrandName != "IC Care")
            {
                errorString = "Billing Last Name is required.";
                this.lblBLastNameValidate.Visible = true;
                this.lblBLastNameValidate.InnerText = errorString;
                this.displayError(errorString);
                SetFocus(this.txtBLastName);
                return false;
            }            
        }
        if (this.txtBTelephone.Text.Trim().Length == 0)
        {
            errorString = "Billing Phone Number is required.";
            this.lblBTelephoneValidate.Visible = true;
            this.lblBTelephoneValidate.InnerText = errorString;
            this.displayError(errorString);
            SetFocus(this.txtBTelephone);
            return false;
        }
        if (this.txtBAddress1.Text.Trim().Length == 0)
        {
            errorString = "Billing Addresse is required.";
            this.lblBAddress1Validate.Visible = true;
            this.lblBAddress1Validate.InnerText = errorString;
            this.displayError(errorString);
            SetFocus(this.txtBAddress1);
            return false;
        }
        if (this.txtBCity.Text.Trim().Length == 0)
        {
            errorString = "Billing City is required.";
            this.lblBCityValidate.Visible = true;
            this.lblBCityValidate.InnerText = errorString;
            this.displayError(errorString);
            SetFocus(this.txtBCity);
            return false;
        }
        if (this.txtBPostalCode.Text.Trim().Length == 0)
        {
            errorString = "Billing Zip Code is required.";
            this.lblBPostalCodeValidate.Visible = true;
            this.lblBPostalCodeValidate.InnerText = errorString;
            this.displayError(errorString);
            SetFocus(this.txtBPostalCode);
            return false;
        }

        /// 9/2013 W.Kakemoto - Shipping company now required to CreateOrders for this location.
        /// 
        if (this.txtSCompany.Text.Trim().Length == 0)
        {
            errorString = "Shipping Company is required.";
            this.txtSCompanyValidate.Visible = true;
            this.txtSCompanyValidate.InnerText = errorString;
            this.displayError(errorString);
            SetFocus(this.txtSCompany);
            return false;
        }

        if (this.txtSFirstName.Text.Trim().Length == 0)
        {
            errorString = "Shipping First Name is required.";
            this.lblSFirstNameValidate.Visible = true;
            this.lblSFirstNameValidate.InnerText = errorString;
            this.displayError(errorString);
            SetFocus(this.txtSFirstName);
            return false;
        }
        if (this.txtSLastName.Text.Trim().Length == 0)
        {
            errorString = "Shipping Last Name is required.";
            this.lblSLastNameValidate.Visible = true;
            this.lblSLastNameValidate.InnerText = errorString;
            this.displayError(errorString);
            SetFocus(this.txtSLastName);
            return false;
        }
        if (this.txtSTelephone.Text.Trim().Length == 0)
        {
            errorString = "Shipping Phone Number is required.";
            this.lblSTelephoneValidate.Visible = true;
            this.lblSTelephoneValidate.InnerText = errorString;
            this.displayError(errorString);
            SetFocus(this.txtSTelephone);
            return false;
        }
        if (this.txtSAddress1.Text.Trim().Length == 0)
        {
            errorString = "Shipping Addresse is required.";
            this.lblSAddress1Validate.Visible = true;
            this.lblSAddress1Validate.InnerText = errorString;
            this.displayError(errorString);
            SetFocus(this.txtSAddress1);
            return false;
        }
        if (this.txtSCity.Text.Trim().Length == 0)
        {
            errorString = "Shipping City is required.";
            this.lblSCityValidate.Visible = true;
            this.lblSCityValidate.InnerText = errorString;
            this.displayError(errorString);
            SetFocus(this.txtSCity);
            return false;
        }
        if (this.txtSPostalCode.Text.Trim().Length == 0)
        {
            errorString = "Shipping Zip Code is required.";
            this.lblSPostalCodeValidate.Visible = true;
            this.lblSPostalCodeValidate.InnerText = errorString;
            this.displayError(errorString);
            SetFocus(this.txtSPostalCode);
            return false;
        }

        if (this.ddlLabel.SelectedItem == null || this.ddlLabel.SelectedItem.Text.Length == 0 || this.ddlLabel.SelectedValue == "0")
        {
            errorString = "Label is required.";
            this.ddlLabelValidate.Visible = true;
            this.ddlLabelValidate.InnerText = errorString;
            this.displayError(errorString);
            SetFocus(this.ddlLabel);
            return false;
        }

        if (account.HDNNotify)
        {
           
            if (double.TryParse(this.txtDLCurrent.Text, out myDouble))
            {
                if (myDouble < 0)
                {
                    errorString = "Deep, Shallow, and Eye parameters can not be a less than 0 number.";
                    this.lblHDNValidate.Visible = true;
                    this.lblHDNValidate.InnerText = errorString;
                    this.displayError(errorString);
                    SetFocus(this.txtDLCurrent);
                    return false;
                }
            }
            if (double.TryParse(this.txtSLCurrent.Text, out myDouble))
            {
                if (myDouble < 0)
                {
                    errorString = "Deep, Shallow, and Eye parameters can not be a less than 0 number.";
                    this.lblHDNValidate.Visible = true;
                    this.lblHDNValidate.InnerText = errorString;
                    this.displayError(errorString);
                    SetFocus(this.txtSLCurrent);
                    return false;
                }
            }
            if (double.TryParse(this.txtEyeCurrent.Text, out myDouble))
            {
                if (myDouble < 0)
                {
                    errorString = "Deep, Shallow, and Eye parameters can not be a less than 0 number.";
                    this.lblHDNValidate.Visible = true;
                    this.lblHDNValidate.InnerText = errorString;
                    this.displayError(errorString);
                    SetFocus(this.txtEyeCurrent);
                    return false;
                }
            }
            if (double.TryParse(this.txtDLQuarter.Text, out myDouble))
            {
                if (myDouble < 0)
                {
                    errorString = "Deep, Shallow, and Eye parameters can not be a less than 0 number.";
                    this.lblHDNValidate.Visible = true;
                    this.lblHDNValidate.InnerText = errorString;
                    this.displayError(errorString);
                    SetFocus(this.txtDLQuarter);
                    return false;
                }
            }
            if (double.TryParse(this.txtSLQuarter.Text, out myDouble))
            {
                if (myDouble < 0)
                {
                    errorString = "Deep, Shallow, and Eye parameters can not be a less than 0 number.";
                    this.lblHDNValidate.Visible = true;
                    this.lblHDNValidate.InnerText = errorString;
                    this.displayError(errorString);
                    SetFocus(this.txtSLQuarter);
                    return false;
                }
            }
            if (double.TryParse(this.txtEyeQuarter.Text, out myDouble))
            {
                if (myDouble < 0)
                {
                    errorString = "Deep, Shallow, and Eye parameters can not be a less than 0 number.";
                    this.lblHDNValidate.Visible = true;
                    this.lblHDNValidate.InnerText = errorString;
                    this.displayError(errorString);
                    SetFocus(this.txtEyeQuarter);
                    return false;
                }
            }
            if (double.TryParse(this.txtDLYear.Text, out myDouble))
            {
                if (myDouble < 0)
                {
                    errorString = "Deep, Shallow, and Eye parameters can not be a less than 0 number.";
                    this.lblHDNValidate.Visible = true;
                    this.lblHDNValidate.InnerText = errorString;
                    this.displayError(errorString);
                    SetFocus(this.txtDLYear);
                    return false;
                }
            }
            if (double.TryParse(this.txtSLYear.Text, out myDouble))
            {
                if (myDouble < 0)
                {
                    errorString = "Deep, Shallow, and Eye parameters can not be a less than 0 number.";
                    this.lblHDNValidate.Visible = true;
                    this.lblHDNValidate.InnerText = errorString;
                    this.displayError(errorString);
                    SetFocus(this.txtSLYear);
                    return false;
                }
            }
            if (double.TryParse(this.txtEyeYear.Text, out myDouble))
            {
                if (myDouble < 0)
                {
                    errorString = "Deep, Shallow, and Eye parameters can not be a less than 0 number.";
                    this.lblHDNValidate.Visible = true;
                    this.lblHDNValidate.InnerText = errorString;
                    this.displayError(errorString);
                    SetFocus(this.txtEyeYear);
                    return false;
                }
            }
        }

        return true;
    }

    protected void CloseWindow()
    {
        ClientScript.RegisterStartupScript(this.GetType(), "closePage", "<script type='text/JavaScript'>window.close();</script>");
    }

    protected void btnBack_Click(object sender, EventArgs e)
    {
        Page.Response.Redirect("../Details/Account.aspx?ID=" + this.accountID.ToString() + "#Location_tab");
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (PassValidation())
        {
            string myStringNull = null;
            Int32? myIntNull = null;

            int myLocationID = this.locationID;

            Boolean myActive = this.chkBoxActive.Checked;
            Boolean myIsDefaultLocation = this.chkBoxIsDefaultLocation.Checked;
            string myLocationName = (this.txtLocationName.Text.Trim().Length == 0 ? myStringNull : this.txtLocationName.Text.Trim());
            string myLocationCode = (this.txtLocationCode.Text.Trim().Length == 0 ? myStringNull : this.txtLocationCode.Text.Trim());
            string myLocationUOM = this.ddlLocationUOM.SelectedValue;
            int? myTimeZone = (int.Parse(this.ddlTimeZone.SelectedValue) == 0 ? myIntNull : int.Parse(this.ddlTimeZone.SelectedValue));

            //1/2014 WK - if account has locaton billing, then save service dates (contract dates).
            DateTime? myContractStartDate = null;
            DateTime? myContractEndDate = null;
            if (divLocationServiceDates.Visible)
            {
                if ((txtServiceStartDate.Text).Trim().Length > 0)
                    myContractStartDate = Convert.ToDateTime(txtServiceStartDate.Text);
                if (txtServiceEndDate.Text.Trim().Length > 0)
                    myContractEndDate = Convert.ToDateTime(txtServiceEndDate.Text);
            }

            string myBCompany = (this.txtBCompany.Text.Trim().Length == 0 ? myStringNull : this.txtBCompany.Text.Trim());
            string myBPrefix = (this.ddlBPrefix.SelectedValue == "" ? myStringNull : this.ddlBPrefix.SelectedValue);
            string myBFirstName = (this.txtBFirstName.Text.Trim().Length == 0 ? myStringNull : this.txtBFirstName.Text.Trim());
            string myBLastName = (this.txtBLastName.Text.Trim().Length == 0 ? myStringNull : this.txtBLastName.Text.Trim());
            string myBTelephone = (this.txtBTelephone.Text.Trim().Length == 0 ? myStringNull : this.txtBTelephone.Text.Trim());
            string myBFax = (this.txtBFax.Text.Trim().Length == 0 ? myStringNull : this.txtBFax.Text.Trim());
            string myBEmailAddress = txtBEmailAddress.Text.Trim();
            int? myBCountry = (int.Parse(this.ddlBCountry.SelectedValue) == 0 ? myIntNull : int.Parse(this.ddlBCountry.SelectedValue));
            string myBAddress1 = (this.txtBAddress1.Text.Trim().Length == 0 ? myStringNull : this.txtBAddress1.Text.Trim());
            string myBAddress2 = (this.txtBAddress2.Text.Trim().Length == 0 ? myStringNull : this.txtBAddress2.Text.Trim());
            string myBAddress3 = (this.txtBAddress3.Text.Trim().Length == 0 ? myStringNull : this.txtBAddress3.Text.Trim());
            string myBCity = (this.txtBCity.Text.Trim().Length == 0 ? myStringNull : this.txtBCity.Text.Trim());
            int? myBState = (int.Parse(this.ddlBState.SelectedValue) == 0 ? myIntNull : int.Parse(this.ddlBState.SelectedValue));
            string myBPostalCode = (this.txtBPostalCode.Text.Trim().Length == 0 ? myStringNull : this.txtBPostalCode.Text.Trim());

            string mySCompany = (this.txtSCompany.Text.Trim().Length == 0 ? myStringNull : this.txtSCompany.Text.Trim());
            string mySPrefix = (this.ddlSPrefix.SelectedValue == "" ? myStringNull : this.ddlSPrefix.SelectedValue);
            string mySFirstName = (this.txtSFirstName.Text.Trim().Length == 0 ? myStringNull : this.txtSFirstName.Text.Trim());
            string mySLastName = (this.txtSLastName.Text.Trim().Length == 0 ? myStringNull : this.txtSLastName.Text.Trim());
            string mySTelephone = (this.txtSTelephone.Text.Trim().Length == 0 ? myStringNull : this.txtSTelephone.Text.Trim());
            string mySFax = (this.txtSFax.Text.Trim().Length == 0 ? myStringNull : this.txtSFax.Text.Trim());
            string mySEmailAddress = txtSEmailAddress.Text.Trim();
            int? mySCountry = (int.Parse(this.ddlSCountry.SelectedValue) == 0 ? myIntNull : int.Parse(this.ddlSCountry.SelectedValue));
            string mySAddress1 = (this.txtSAddress1.Text.Trim().Length == 0 ? myStringNull : this.txtSAddress1.Text.Trim());
            string mySAddress2 = (this.txtSAddress2.Text.Trim().Length == 0 ? myStringNull : this.txtSAddress2.Text.Trim());
            string mySAddress3 = (this.txtSAddress3.Text.Trim().Length == 0 ? myStringNull : this.txtSAddress3.Text.Trim());
            string mySCity = (this.txtSCity.Text.Trim().Length == 0 ? myStringNull : this.txtSCity.Text.Trim());
            int? mySState = (int.Parse(this.ddlSState.SelectedValue) == 0 ? myIntNull : int.Parse(this.ddlSState.SelectedValue));
            string mySPostalCode = (this.txtSPostalCode.Text.Trim().Length == 0 ? myStringNull : this.txtSPostalCode.Text.Trim());

            //Boolean myHasLabel = this.chkBoxHasLabel.Checked;
            Boolean myHasLabel = false;
            int? mySchedule = (int.Parse(this.ddlSchedule.SelectedValue) == 0 ? myIntNull : int.Parse(this.ddlSchedule.SelectedValue));
            int myProductGroupID = int.Parse(this.ddlProductGroup.SelectedValue);
            int myLabelID = int.Parse(this.ddlLabel.SelectedValue);
            

            double myDLCurrent = ((this.txtDLCurrent.Text.Trim().Length == 0) || (this.txtDLCurrent.Text.Trim() == "0") ? 0 : double.Parse (this.txtDLCurrent.Text.Trim()));
            double mySLCurrent = ((this.txtSLCurrent.Text.Trim().Length == 0) || (this.txtSLCurrent.Text.Trim() == "0") ? 0 : double.Parse (this.txtSLCurrent.Text.Trim()));
            double myEyeCurrent = ((this.txtEyeCurrent.Text.Trim().Length == 0) || (this.txtEyeCurrent.Text.Trim() == "0") ? 0 : double.Parse (this.txtEyeCurrent.Text.Trim()));
            double myDLQuarter = ((this.txtDLQuarter.Text.Trim().Length == 0) || (this.txtDLQuarter.Text.Trim() == "0") ? 0 : double.Parse (this.txtDLQuarter.Text.Trim()));
            double mySLQuarter = ((this.txtSLQuarter.Text.Trim().Length == 0) || (this.txtSLQuarter.Text.Trim() == "0") ? 0 : double.Parse (this.txtSLQuarter.Text.Trim()));
            double myEyeQuarter = ((this.txtEyeQuarter.Text.Trim().Length == 0) || (this.txtEyeQuarter.Text.Trim() == "0") ? 0 : double.Parse (this.txtEyeQuarter.Text.Trim()));
            double myDLYear = ((this.txtDLYear.Text.Trim().Length == 0) || (this.txtDLYear.Text.Trim() == "0") ? 0 : double.Parse (this.txtDLYear.Text.Trim()));
            double mySLYear = ((this.txtSLYear.Text.Trim().Length == 0) || (this.txtSLYear.Text.Trim() == "0") ? 0 : double.Parse (this.txtSLYear.Text.Trim()));
            double myEyeYear = ((this.txtEyeYear.Text.Trim().Length == 0) || (this.txtEyeYear.Text.Trim() == "0") ? 0 : double.Parse (this.txtEyeYear.Text.Trim()));

            int? myBillingGroupID = (int.Parse(this.ddlBillingGroup.SelectedValue) == 0 ? myIntNull : int.Parse(this.ddlBillingGroup.SelectedValue));

            // Convert data points to mrem if in mSv.
            if (myLocationUOM.ToLower() == "msv")
            {
                myDLCurrent = myDLCurrent * 100;
                mySLCurrent = mySLCurrent * 100;
                myEyeCurrent = myEyeCurrent * 100;

                myDLQuarter = myDLQuarter * 100;
                mySLQuarter = mySLQuarter * 100;
                myEyeQuarter = myEyeQuarter * 100;

                myDLYear = myDLYear * 100;
                mySLYear = mySLYear * 100;
                myEyeYear = myEyeYear * 100;
            }

            int myHDNConfigID = 0;
      
            if (myLocationID == 0)  // add new location
            {
                try
                {
                    // reset IsDefaultLocation flag of all other locations of an account to false if the new location will be the default one.
                    if (myIsDefaultLocation)
                    {
                        var existingLocations = (from loc in idc.Locations where loc.AccountID == this.accountID select loc).AsQueryable();
                        foreach (var myLoc in existingLocations)
                        {
                            myLoc.IsDefaultLocation = false;
                        }
                        // Save changes
                        idc.SubmitChanges();
                    }


                    // set HDNConfig if there is any parameters set.
                    if (account.HDNNotify)
                    {
                        if (myDLCurrent != 0 || myDLQuarter != 0 || myDLYear != 0 || mySLCurrent != 0 || mySLQuarter != 0 || mySLYear != 0 || myEyeCurrent != 0 || myEyeQuarter != 0 || myEyeYear != 0 )
                        {
                            HDNConfig hdn = new HDNConfig
                            {                          
                                DeepCurrent = (myDLCurrent != 0) ? (int?)Math.Round(myDLCurrent, 0) : null,
                                DeepQTD = (myDLQuarter != 0) ? (int?)Math.Round(myDLQuarter, 0) : null,
                                DeepYTD = (myDLYear != 0) ? (int?)Math.Round(myDLYear, 0) : null,

                                ShallowCurrent = (mySLCurrent != 0) ? (int?)Math.Round(mySLCurrent, 0) : null,
                                ShallowQTD = (mySLQuarter != 0) ? (int?)Math.Round(mySLQuarter, 0) : null,
                                ShallowYTD = (mySLYear != 0) ? (int?)Math.Round(mySLYear, 0) : null,

                                EyeCurrent = (myEyeCurrent != 0) ? (int?)Math.Round(myEyeCurrent, 0) : null,
                                EyeQTD = (myEyeQuarter != 0) ? (int?)Math.Round(myEyeQuarter, 0) : null,
                                EyeYTD = (myEyeYear != 0) ? (int?)Math.Round(myEyeYear, 0) : null
                            };
                            idc.HDNConfigs.InsertOnSubmit(hdn);
                            idc.SubmitChanges();
                            myHDNConfigID = hdn.HDNConfigID;
                        }
                        
                    }

                    
                    Location d = new Location
                    {
                        AccountID = this.accountID,
                        Active  = myActive,
                        IsDefaultLocation= myIsDefaultLocation,
                        LocationName= myLocationName,
                        LocationCode= myLocationCode,
                        LocationUOM= myLocationUOM,
                        TimeZoneID= myTimeZone,

                        BillingCompany= myBCompany,
                        BillingNamePrefix= myBPrefix,
                        BillingFirstName= myBFirstName,
                        BillingLastName= myBLastName,
                        BillingTelephone= myBTelephone,
                        BillingFax= myBFax,
                        BillingEmailAddress = myBEmailAddress,
                        BillingCountryID= Convert.ToInt16 (myBCountry),
                        BillingAddress1= myBAddress1,
                        BillingAddress2= myBAddress2,
                        BillingAddress3= myBAddress3,
                        BillingCity= myBCity,
                        BillingStateID = Convert.ToInt16(myBState),
                        BillingPostalCode= myBPostalCode,

                        ShippingCompany= mySCompany,
                        ShippingNamePrefix= mySPrefix,
                        ShippingFirstName= mySFirstName,
                        ShippingLastName= mySLastName,
                        ShippingTelephone= mySTelephone,
                        ShippingFax = mySFax,
                        ShippingEmailAddress = mySEmailAddress,
                        ShippingCountryID = Convert.ToInt16(mySCountry),
                        ShippingAddress1= mySAddress1,
                        ShippingAddress2= mySAddress2,
                        ShippingAddress3= mySAddress3,
                        ShippingCity= mySCity,
                        ShippingStateID = Convert.ToInt16(mySState),
                        ShippingPostalCode= mySPostalCode,

                        HasLabel = myHasLabel,
                        ScheduleID = mySchedule,

                        ContractStartDate = myContractStartDate,
                        ContractEndDate = myContractEndDate,

                        HDNConfigID = (myHDNConfigID > 0) ? (int?)myHDNConfigID : null,

                        BillingGroupID = myBillingGroupID

                    };
                    idc.Locations.InsertOnSubmit(d);
                    idc.SubmitChanges();
                    myLocationID = d.LocationID;

                   // Insert LocationPGDetails
                    LocationPGDetail lpgd = new LocationPGDetail
                    {
                        LocationID = myLocationID,
                        ProductGroupID = myProductGroupID,
                        AccountID = this.accountID,
                        LabelID = myLabelID
                    };
                    idc.LocationPGDetails.InsertOnSubmit(lpgd);
                    idc.SubmitChanges();

                    // close window
                    //CloseWindow();
                    btnBack_Click(sender, e);

                }
                catch (Exception ex)
                {
                    // Display the system generated error message.
                    this.displayError(ex.Message);
                }
            }
            else    // update location
            {
                try
                {
                    // reset IsDefaultLocation flag of all other locations of an account to false if the new location will be the default one.
                    if (myIsDefaultLocation)
                    {
                        var existingLocations = (from loc in idc.Locations where loc.AccountID == this.accountID select loc).AsQueryable();
                        foreach (var myLoc in existingLocations)
                        {
                            myLoc.IsDefaultLocation = false;
                        }
                        // Save changes
                        idc.SubmitChanges();
                    }


                    // set HDNConfig if there is any parameters set.
                    if (account.HDNNotify)
                    {
                        myHDNConfigID = (location.HDNConfigID == null) ? 0 : Convert.ToInt16(location.HDNConfigID);

                        if (myHDNConfigID > 0)    // update HDNConfig record if a location has HDNConfigID
                        {
                            HDNConfig existHDNConfig =
                                                      (from eHDNConfig in idc.HDNConfigs
                                                       where eHDNConfig.HDNConfigID == myHDNConfigID
                                                       select eHDNConfig).First();

                            existHDNConfig.DeepCurrent = (myDLCurrent != 0) ? (int?)Math.Round(myDLCurrent, 0) : null;
                            existHDNConfig.DeepQTD = (myDLQuarter != 0) ? (int?)Math.Round(myDLQuarter, 0) : null;
                            existHDNConfig.DeepYTD = (myDLYear != 0) ? (int?)Math.Round(myDLYear, 0) : null;

                            existHDNConfig.ShallowCurrent = (mySLCurrent != 0) ? (int?)Math.Round(mySLCurrent, 0) : null;
                            existHDNConfig.ShallowQTD = (mySLQuarter != 0) ? (int?)Math.Round(mySLQuarter, 0) : null;
                            existHDNConfig.ShallowYTD = (mySLYear != 0) ? (int?)Math.Round(mySLYear, 0) : null;

                            existHDNConfig.EyeCurrent = (myEyeCurrent != 0) ? (int?)Math.Round(myEyeCurrent, 0) : null;
                            existHDNConfig.EyeQTD = (myEyeQuarter != 0) ? (int?)Math.Round(myEyeQuarter, 0) : null;
                            existHDNConfig.EyeYTD = (myEyeYear != 0) ? (int?)Math.Round(myEyeYear, 0) : null;

                            idc.SubmitChanges();
                        }
                        else                     // add new HDNConfig record if a location has never had HDNConfigID and user enters deep, shallow, and eye values
                        {
                            if (myDLCurrent != 0 || myDLQuarter != 0 || myDLYear != 0 || mySLCurrent != 0 || mySLQuarter != 0 || mySLYear != 0 || myEyeCurrent != 0 || myEyeQuarter != 0 || myEyeYear != 0)
                            {
                                HDNConfig hdn = new HDNConfig
                                {
                                    DeepCurrent = (myDLCurrent != 0) ? (int?)Math.Round(myDLCurrent, 0) : null,
                                    DeepQTD = (myDLQuarter != 0) ? (int?)Math.Round(myDLQuarter, 0) : null,
                                    DeepYTD = (myDLYear != 0) ? (int?)Math.Round(myDLYear, 0) : null,

                                    ShallowCurrent = (mySLCurrent != 0) ? (int?)Math.Round(mySLCurrent, 0) : null,
                                    ShallowQTD = (mySLQuarter != 0) ? (int?)Math.Round(mySLQuarter, 0) : null,
                                    ShallowYTD = (mySLYear != 0) ? (int?)Math.Round(mySLYear, 0) : null,

                                    EyeCurrent = (myEyeCurrent != 0) ? (int?)Math.Round(myEyeCurrent, 0) : null,
                                    EyeQTD = (myEyeQuarter != 0) ? (int?)Math.Round(myEyeQuarter, 0) : null,
                                    EyeYTD = (myEyeYear != 0) ? (int?)Math.Round(myEyeYear, 0) : null
                                };
                                idc.HDNConfigs.InsertOnSubmit(hdn);
                                idc.SubmitChanges();
                                myHDNConfigID = hdn.HDNConfigID;
                            }
                        }
                        
                    }

                    // Save location
                    Location c =
                      (from loc in idc.Locations
                       where loc.LocationID == myLocationID
                       select loc).First();

                    var oldSched = c.ScheduleID;
                    c.AccountID = this.accountID;
                    c.Active = myActive;
                    c.IsDefaultLocation = myIsDefaultLocation;
                    c.LocationName = myLocationName;
                    c.LocationCode = myLocationCode;
                    c.LocationUOM = myLocationUOM;
                    var currentTimezoneID = c.TimeZoneID;
                    c.TimeZoneID = myTimeZone;
                    if (currentTimezoneID != c.TimeZoneID)
                    {
                        var devices = c.AccountDevices.Where(ad => ad.Active);
                        foreach (var device in devices)
                        {
                            // do not update device inventory for ID3 devices
                            var deviceInventory = idc.DeviceInventories.FirstOrDefault(di => di.DeviceID == device.DeviceID);
                            if (deviceInventory.Product.ProductGroupID != Instadose.Device.CONSTS.INSTADOSE_3)
                            {
                                device.DeviceInventory.ScheduleSyncDate = null;
                                device.DeviceInventory.ScheduleSyncStatus = "UPDATE";
                            }                                
                        }
                    }

                    c.BillingCompany = myBCompany;
                    c.BillingNamePrefix = myBPrefix;
                    c.BillingFirstName = myBFirstName;
                    c.BillingLastName = myBLastName;
                    c.BillingTelephone = myBTelephone;
                    c.BillingFax = myBFax;
                    c.BillingEmailAddress = myBEmailAddress;
                    c.BillingCountryID = Convert.ToInt16(myBCountry);
                    c.BillingAddress1 = myBAddress1;
                    c.BillingAddress2 = myBAddress2;
                    c.BillingAddress3 = myBAddress3;
                    c.BillingCity = myBCity;
                    c.BillingStateID = Convert.ToInt16(myBState);
                    c.BillingPostalCode = myBPostalCode;

                    c.ShippingCompany = mySCompany;
                    c.ShippingNamePrefix = mySPrefix;
                    c.ShippingFirstName = mySFirstName;
                    c.ShippingLastName = mySLastName;
                    c.ShippingTelephone = mySTelephone;
                    c.ShippingFax = mySFax;
                    c.ShippingEmailAddress = mySEmailAddress;
                    c.ShippingCountryID = Convert.ToInt16(mySCountry);
                    c.ShippingAddress1 = mySAddress1;
                    c.ShippingAddress2 = mySAddress2;
                    c.ShippingAddress3 = mySAddress3;
                    c.ShippingCity = mySCity;
                    c.ShippingStateID = Convert.ToInt16(mySState);
                    c.ShippingPostalCode = mySPostalCode;
                    c.HDNConfigID = (myHDNConfigID > 0) ? (int?)myHDNConfigID : null;

                    c.HasLabel = myHasLabel;

                    c.ScheduleID = mySchedule;

                    c.ContractStartDate = myContractStartDate;
                    c.ContractEndDate = myContractEndDate;

                    c.BillingGroupID = myBillingGroupID;

                    idc.SubmitChanges();

                    //if (c.ScheduleID != oldSched)
                    //{
                        var userDevs = (from u in idc.Users
                                             join ud in idc.UserDevices on u.UserID equals ud.UserID
                                             where u.AccountID == this.accountID && u.LocationID == myLocationID && u.Active == true && ud.Active == true
                                             select new
                                             {
                                                 ud.DeviceID
                                             }).Distinct().AsQueryable();


                        //var acctDevs = (from ad in idc.AccountDevices where ad.AccountID == this.accountID && ad.LocationID == myLocationID && ad.Active == true select ad).AsQueryable();
                        var hasChanges = false;
                        foreach (var userDev in userDevs)
                        {
                            var schedDev = (from di in idc.DeviceInventories where di.DeviceID == userDev.DeviceID select di).FirstOrDefault();
                            if (schedDev.ScheduleID != mySchedule && schedDev.Product.ProductGroupID != Instadose.Device.CONSTS.INSTADOSE_3)
                            {
                                schedDev.ScheduleSyncStatus = "PENDING";
                                schedDev.ScheduleID = mySchedule;
                                SetDeviceProfile(schedDev);
                                hasChanges = true;
                            }
                        }

                        if (hasChanges)
                            idc.SubmitChanges();
                    //}

                    // ------------------ save LocationPGDetail -------------------------//
                    LocationPGDetail lpgd =
                      (from locPGD in idc.LocationPGDetails
                       where locPGD.LocationID == myLocationID && locPGD.AccountID == this.accountID
                       select locPGD).FirstOrDefault();
                    if (lpgd != null)
                    {
                        // Delete the old LocationPGDetails 
                        idc.LocationPGDetails.DeleteOnSubmit(lpgd);
                        idc.SubmitChanges();
                    }

                    // Insert new LocationPGDetails                        
                    LocationPGDetail myLPGD = new LocationPGDetail
                    {
                        LocationID = myLocationID,
                        ProductGroupID = myProductGroupID,
                        AccountID = this.accountID,
                        LabelID = myLabelID
                    };
                    idc.LocationPGDetails.InsertOnSubmit(myLPGD);
                    idc.SubmitChanges();
                    // ------------------ save LocationPGDetail -------------------------//
                    
                    // close window
                    //CloseWindow();
                    btnBack_Click(sender, e);

                }
                catch (Exception ex)
                {
                    // Display the system generated error message.
                    this.displayError(ex.Message);
                }
            }

        }
    }

    protected void btnRefresh_Click(object sender, EventArgs e)
    {
        this.gvLocationGroup.DataBind();
    }

    public string YesNo(object boolean)
    {
        try
        {
            return Convert.ToBoolean(boolean) ? "Yes" : "No";
        }
        catch { return "No"; }
    }

    private void displayError(string error)
    {
        //errorMsg.InnerText = string.Format("An error was encountered: {0}", error);
        errorMsg.InnerText = error;
        this.error.Visible = true;
    }



    // ------------------------- Modal Dialog Section FUNCTIONS here ------------------------------------- //

    private void SetDeviceProfile(DeviceInventory device)
    {
        device.ScheduleSyncDate = null;
        device.ScheduleTimeOffset = null;
        device.AdvertTime = 30;
        device.ConnectionTime = 30;
        device.DeviceModeID = 3;
        device.CommRetries = 15;
        device.CommInterval = 15;
        device.DiagInterval = 15;
        device.DiagAdvInterval = 15;
        if (!string.IsNullOrEmpty(device.SerialNo))
        {
            var profileOffset = int.Parse(device.SerialNo.Substring(device.SerialNo.Length - 1));
            device.PendingDeviceProfileID = profileOffset == 0 ? 70 : 60 + profileOffset;
        }
    }

    private void InvisibleErrors_GroupDialog()
    {
        // Reset submission form error message      
        this.groupDialogErrorMsg.InnerText = "";
        this.groupDialogErrors.Visible = false;
    }

    private void VisibleErrors_GroupDialog(string error)
    {
        this.groupDialogErrorMsg.InnerText = error;
        this.groupDialogErrors.Visible = true;
    }

    private bool passInputsValidation_GroupDialog()
    {
        string errorString = "";

        if (this.txtGroupName.Text.Trim().Length == 0)
        {
            errorString = "Group is required.";
            this.VisibleErrors_GroupDialog(errorString);
            return false;
        }
        return true;
    }

    private void SetDefaultValues_GroupDialog()
    {
        this.txtGroupName.Text = "";
        this.chkBoxGroupActive.Checked = true;
    }

    private void SetValuesToControls_GroupDialog()
    {

        string locationName;
        locationName = (from loc in idc.Locations
                        where loc.LocationID == this.locationID
                        select loc.LocationName).FirstOrDefault();

        int groupID = (Session["selectedGroupID"] != null) ? Convert.ToInt32(Session["selectedGroupID"]) : 0; 

        this.lblGroupLocationID.Text = this.locationID == 0 ? "" : locationID.ToString();
        this.lblGroupID.Text = groupID == 0 ? "" : groupID.ToString();
        this.lblGroupLocationName.Text = locationName;
        SetFocus(this.txtGroupName);

        if (groupID > 0)    // edit mode
        {
            LocationGroup myGroup = (from locGrp in idc.LocationGroups
                                     where locGrp.GroupID == groupID
                                     select locGrp).FirstOrDefault();

            this.txtGroupName.Text = (myGroup.GroupName == null ? "" : myGroup.GroupName);
            this.chkBoxGroupActive.Checked = (myGroup.Active == null ? false : Convert.ToBoolean(myGroup.Active));
            this.ddlGroupSchedule.SelectedValue = (myGroup.ScheduleID == null ? "0" : myGroup.ScheduleID.ToString());           
        }
  
    }

    protected void btnNewGroup_Click(object sender, EventArgs e)
    {
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('groupDialog')", true);
    }

    protected void btnCancelGroup_Click(object sender, EventArgs e)
    {
        InvisibleErrors_GroupDialog();
        SetDefaultValues_GroupDialog();
        // delete session variable
        if (Session["selectedGroupID"] != null)
            Session.Remove("selectedGroupID");
    }

    protected void btnLoadGroup_Click(object sender, EventArgs e)
    {
        //InvisibleErrors_GroupDialog();
        this.ddlGroupSchedule.DataBind();
        ListItem firstItem = new ListItem("  -- Select Schedule Name --", "0");
        ddlGroupSchedule.Items.Insert(0, firstItem);
        ddlGroupSchedule.SelectedIndex = 0;

        SetDefaultValues_GroupDialog();
        SetValuesToControls_GroupDialog();
    }

    protected void btnEditGroup_Click(object sender, EventArgs e)
    {
        LinkButton btn = (LinkButton)sender;
        string myCmdname = btn.CommandName.ToString();
        Session["selectedGroupID"] = btn.CommandArgument.ToString();
        
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('groupDialog')", true);
       
    }

    protected void btnAddGroup_Click(object sender, EventArgs e)
    {
    
        if (passInputsValidation_GroupDialog())
        {
            string myStringNull = null;

            int myGroupID  = (Session["selectedGroupID"] != null) ? Convert.ToInt32(Session["selectedGroupID"]) : 0; 
            string myGroupName = (this.txtGroupName.Text.Trim().Length == 0 ? myStringNull : this.txtGroupName.Text.Trim());
            Boolean myActive = this.chkBoxGroupActive.Checked;
            int? myGroupSchedule = (int.Parse(this.ddlGroupSchedule.SelectedValue) == 0 ? (int?)null : int.Parse(this.ddlGroupSchedule.SelectedValue));

            if (myGroupID == 0)  // add new group
            {
                try
                {
                    LocationGroup d = new LocationGroup
                    {
                        LocationID = this.locationID,
                        GroupName = myGroupName,
                        Active = myActive,
                        ScheduleID = myGroupSchedule,
                    };

                    idc.LocationGroups.InsertOnSubmit(d);
                    idc.SubmitChanges();
                    myGroupID = d.GroupID;

                    // refresh gridview
                    gvLocationGroup.DataBind();
                   
                    ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('groupDialog')", true);

                }
                catch (Exception ex)
                {
                    // Display the system generated error message.
                    this.VisibleErrors_GroupDialog(string.Format("An error occurred: {0}", ex.Message));
                }
            }
            else    // update group
            {
                try
                {
                    LocationGroup c =
                        (from r in idc.LocationGroups
                        where r.GroupID == myGroupID
                        select r).First();

                    c.LocationID = this.locationID;
                    c.GroupName = myGroupName;
                    c.Active = myActive;
                    c.ScheduleID = myGroupSchedule;

                    idc.SubmitChanges();

                    // refresh gridview
                    gvLocationGroup.DataBind();
                    
                    ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('groupDialog')", true);

                }
                catch (Exception ex)
                {
                    // Display the system generated error message.
                    this.VisibleErrors_GroupDialog(string.Format("An error occurred: {0}", ex.Message));
                }
            }
                
        }
       
    }

    //1/2014 WK - If account has location billing then check length of contract (service) dates.
    //  IC Care accounts has 1 year contract length.
    protected void txtServiceStartDate_TextChanged(object sender, EventArgs e)
    {
        //IC Care service start must be on the 1st of the month!
        if ((int)account.BrandSourceID == 3)
            txtServiceStartDate.Text = string.Format("{0:MM/1/yyyy}",
                                            DateTime.Parse(txtServiceStartDate.Text));
        if (txtServiceStartDate.Text.Trim().Length > 5)
        {
            if (account.BillingTermID == 1)  // quarterly
                txtServiceEndDate.Text = string.Format("{0:MM/dd/yyyy}",
                                            DateTime.Parse(txtServiceStartDate.Text).AddMonths(3)  );
            if (account.BillingTermID == 2)  // yearly
                txtServiceEndDate.Text = string.Format("{0:MM/dd/yyyy}",
                                            DateTime.Parse(txtServiceStartDate.Text).AddYears(1));
        }

    }
    
    protected void txtServiceEndDate_TextChanged(object sender, EventArgs e)
    {

    }

    // ------------------------- Modal Dialog Section FUNCTIONS here ------------------------------------- //
    
}