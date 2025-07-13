/*
 * File: SalesRepDistributors Maintenance
 * Author: Anuradha Nandi
 * Created: October 16, 2012
 * 
 *  Copyright (C) 2012 Mirion Technologies. All Rights Reserved.
 */

using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Instadose.Data;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.Linq.SqlClient;
using System.Net.Mail;

public partial class Sales_SalesRepDistributors : System.Web.UI.Page
{
    // Create the database references.
    InsDataContext idc = new InsDataContext();
    SalesRepDistributor srd = new SalesRepDistributor();
    CommissionSetup cs = new CommissionSetup();

    // Global Variables.
    string salesRepDistID = "";
    int channelID = 0;
    DateTime currentDate = DateTime.Now;
    int commissionSetupID = 0;
    string userName;
    const string COMMISSIONEVENT = "Payment";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Page.IsPostBack) return;

        hdnfldAccordionIndex.Value = "0";
        hdnfldAccordionIndex_New.Value = "0";
        hdnfldFormMode.Value = "";
        lblEmailRequired.Visible = true;
        lblFaxRequired.Visible = false;
        lblCommissionPercentageRequired.Visible = false;
        lblCommissionPercentageRequired_New.Visible = false;
        InvisibleErrors_SalesRepDistDialog();
        SetDefaultValues_SalesRepDistDialog();
        SetDefaultValues_AddCommissionDetailsDialog();
        InitiateDDLControlsInModals();
        divMasterInformation.Visible = true;
        divCommissionDetails.Visible = true;
        ShowHideSalesRepDistStatus(false);
    }

    /// <summary>
    /// Sets Text to Yes/No depending on Boolean.
    /// Used to display Active/Inactive status on Client-Side.
    /// </summary>
    /// <param name="boolean"></param>
    /// <returns></returns>
    public string YesNo(object boolean)
    {
        try
        {
            return Convert.ToBoolean(boolean) ? "Yes" : "No";
        }
        catch
        {
            return "No";
        }
    }

    private string GetUserName()
    {
        // Try to set the Username.
        try { userName = User.Identity.Name.Split('\\')[1]; }
        catch { userName = "Unknown"; }

        return userName;
    }

    private void InvisibleErrors_SalesRepDistDialog()
    {
        // Reset submission Form Error Message.
        this.salesRepDistributorDialogErrorMsg.InnerText = "";
        this.salesRepDistributorDialogErrors.Visible = false;
    }

    private void VisibleErrors_SalesRepDistDialog(string errorMsg)
    {
        this.salesRepDistributorDialogErrorMsg.InnerText = errorMsg;
        this.salesRepDistributorDialogErrors.Visible = true;
    }

    // Set Contract Start Date to Contract Sign Date (the Contract Start Date can be manually changed/independently).
    // Also, set the Contract End Date to 5 years (+) to Contract Sign Date/Contract Start Date (initially).
    // Add SALES REP. Modal/Dialog.
    protected void txtContractSignDate_OnTextChanged(object sender, EventArgs e)
    {
        DateTime localContractSignDate;
        DateTime.TryParse(txtContractSignDate.Text, out localContractSignDate);
        txtContractStartDate.Text = String.Format("{0:MM/dd/yyyy}", localContractSignDate);
        txtContractEndDate.Text = String.Format("{0:MM/dd/yyyy}", localContractSignDate.AddYears(5));
    }

    // Set Contract Start Date to Contract Sign Date (the Contract Start Date can be manually changed/independently).
    // Also, set the Contract End Date to 5 years (+) to Contract Sign Date/Contract Start Date (initially).
    // Add COMMISSION SETUP Modal/Dialog.
    protected void txtContractSignDate_New_OnTextChanged(object sender, EventArgs e)
    {
        DateTime localContractSignDate;
        DateTime.TryParse(txtContractSignDate_New.Text, out localContractSignDate);
        txtContractStartDate_New.Text = String.Format("{0:MM/dd/yyyy}", localContractSignDate);
        txtContractEndDate_New.Text = String.Format("{0:MM/dd/yyyy}", localContractSignDate.AddYears(5));
    }

    // Set Contract End Date to 5 year (+) to Contract Start Date.
    // This functionality is independant of Contract Sign Date (as above).
    // Add SALES REP. Modal/Dialog.
    protected void txtContractStartDate_OnTextChanged(object sender, EventArgs e)
    {
        DateTime localContractStartDate;
        DateTime.TryParse(txtContractStartDate.Text, out localContractStartDate);
        txtContractEndDate.Text = String.Format("{0:MM/dd/yyyy}", localContractStartDate.AddYears(5));
    }

    // Set Contract End Date to 5 year (+) to Contract Start Date.
    // This functionality is independant of Contract Sign Date (as above).
    // Add COMMISSION SETUP Modal/Dialog.
    protected void txtContractStartDate_New_OnTextChanged(object sender, EventArgs e)
    {
        DateTime localContractStartDate;
        DateTime.TryParse(txtContractStartDate_New.Text, out localContractStartDate);
        txtContractEndDate_New.Text = String.Format("{0:MM/dd/yyyy}", localContractStartDate.AddYears(5));
    }

    // Check to see if SalesRepDistID is already in DB.
    // Used to check for duplicates when a new Sales Rep. is being created.
    private int GetCountOfSalesRepID(string salesrepdistid)
    {
        int count = 0;

        count = (from s in idc.SalesRepDistributors
                 where s.SalesRepDistID == salesrepdistid
                 select s.SalesRepDistID).Count();

        return count;
    }

    // Check to see if the SalesRepDistID (Account) is Active/Inactive.
    private bool GetSalesRepDistStatus(string salesrepdistid)
    {
        bool status = true;

        status = (from s in idc.SalesRepDistributors
                  where s.SalesRepDistID == salesrepdistid
                  select s.Active).FirstOrDefault();

        return status;
    }

    // If SalesRepDistID (Account) is Active show all accordion panels.
    // Else, hide all accordion panels (except Activate/Deactivate Panel).
    private void StateOfFormBasedOnAccountStatus()
    {
        salesRepDistID = hdnfldSalesRepDistID.Value;

        if (GetSalesRepDistStatus(salesRepDistID) == true)
        {
            this.divMasterInformation.Visible = true;
            this.divCommissionDetails.Visible = true;
        }
        else
        {
            this.divMasterInformation.Visible = false;
            this.divCommissionDetails.Visible = false;
        }
    }

    /// <summary>
    /// This function will return a Sequenced Number (based on the DB).
    /// Used only for Reseller or Distributor SalesRepDistID(s).
    /// </summary>
    /// <returns></returns>
    private string SetResellerOrDistributorID(int channelid)
    {
        string resellerSeqID = "";
        string distrbutorSeqID = "";

        // Get newest Reseller (SalesRepDistID) from DB.
        resellerSeqID = (from s in idc.SalesRepDistributors
                         where SqlMethods.Like(s.SalesRepDistID, "R[0-9]%")
                         orderby s.SalesRepDistID descending
                         select s.SalesRepDistID).FirstOrDefault();

        // Get newest Distributor (SalesRepDistID) from DB.
        distrbutorSeqID = (from s in idc.SalesRepDistributors
                           where SqlMethods.Like(s.SalesRepDistID, "D[0-9]%")
                           orderby s.SalesRepDistID descending
                           select s.SalesRepDistID).FirstOrDefault();

        // Regular Expression to get Integer/Digits out of a String value.
        Regex regExp = new Regex(@"\d+");

        Match matchingReseller = regExp.Match(resellerSeqID);

        Match matchingDistributor = regExp.Match(distrbutorSeqID);

        // Get respective Sequence Number and add (1).
        int resellerSeqNum = 0;
        int distributorSeqNum = 0;

        if (matchingReseller.Success)
        {
            Int32.TryParse(matchingReseller.Value, out resellerSeqNum);
        }

        if (matchingDistributor.Success)
        {
            Int32.TryParse(matchingDistributor.Value, out distributorSeqNum);
        }

        resellerSeqNum = resellerSeqNum + 1;
        distributorSeqNum = distributorSeqNum + 1;

        // Format Sequence Number to ### (i.e. 001).
        resellerSeqID = resellerSeqNum.ToString("000");
        distrbutorSeqID = distributorSeqNum.ToString("000");

        switch (channelid)
        {
            case 7:
                salesRepDistID = "D" + distrbutorSeqID;
                break;
            case 8:
                salesRepDistID = "R" + resellerSeqID;
                break;
            default:
                break;
        }

        return salesRepDistID;
    }

    // Populate DDL for Sales Managers for Modal/Dialog.
    private void PopulateSalesManagerControl()
    {
        var salesManagers = (from sm in idc.SalesRepDistributors
                             orderby sm.SalesRepDistID
                             where sm.ChannelID == 1 || sm.ChannelID == 2
                             select new
                             {
                                 sm.SalesRepDistID,
                                 SalesManagerDetails = sm.SalesRepDistID + " | " + (sm.ChannelID == 1 ? "Inside Sales" : "Outside Sales") + ((sm.FirstName + " " + sm.LastName) == null ? "" : (" | " + sm.FirstName + " " + sm.LastName))
                                 //SalesManagerDetails = sm.SalesRepDistID + " | " + (sm.ChannelID == 1 ? "Inside Sales" : "Outside Sales")
                             });

        ddlSalesManagerID.DataSource = salesManagers;
        ddlSalesManagerID.DataTextField = "SalesManagerDetails";
        ddlSalesManagerID.DataValueField = "SalesRepDistID";
        ddlSalesManagerID.DataBind();
    }

    // Populate DDL for Agreement Type for Modal/Dialog.
    private void PopulateAgreementTypeControl()
    {
        var agreementtype = (from at in idc.CommissionSetups
                             orderby at.AgreementType
                             where at.AgreementType != null
                             select new
                             {
                                 at.AgreementType
                             }).Distinct();

        // Commission Details (for Edit).
        ddlAgreementType.DataSource = agreementtype;
        this.ddlAgreementType.DataTextField = "AgreementType";
        this.ddlAgreementType.DataValueField = "AgreementType";
        this.ddlAgreementType.DataBind();

        // Commission Details (for Add).
        ddlAgreementType_New.DataSource = agreementtype;
        ddlAgreementType_New.DataTextField = "AgreementType";
        ddlAgreementType_New.DataValueField = "AgreementType";
        ddlAgreementType_New.DataBind();
    }

    // Populate DDL for Currency Code for Modal/Dialog.
    private void PopulateCurrencyCodeControl()
    {
        var currency = from c in idc.Currencies
                       orderby c.CurrencyCode
                       select new
                       {
                           c.CurrencyCode,
                       };

        // Commission Details (for Edit).
        ddlResellerPriceCurrencyCode.DataSource = currency;
        ddlResellerPriceCurrencyCode.DataTextField = "CurrencyCode";
        ddlResellerPriceCurrencyCode.DataValueField = "CurrencyCode";
        ddlResellerPriceCurrencyCode.DataBind();

        // Commission Details (for Add).
        ddlResellerPriceCurrencyCode_New.DataSource = currency;
        ddlResellerPriceCurrencyCode_New.DataTextField = "CurrencyCode";
        ddlResellerPriceCurrencyCode_New.DataValueField = "CurrencyCode";
        ddlResellerPriceCurrencyCode_New.DataBind();
    }

    // Populate DDL for Country for Modal/Dialog.
    private void PopulateCountryDDL()
    {
        var countries = (from c in idc.Countries
                         orderby c.CountryName
                         select new
                         {
                             c.CountryID,
                             c.CountryName
                         });

        ddlCountry.DataSource = countries;
        ddlCountry.DataTextField = "CountryName";
        ddlCountry.DataValueField = "CountryID";
        ddlCountry.DataBind();
    }

    // Populate DDL for State for Modal/Dialog.
    private void PopulateStateDDL(int countryid)
    {
        ddlState.Items.Clear();

        var states = (from s in idc.States
                      where s.CountryID == countryid
                      orderby s.StateAbbrev
                      select new
                      {
                          s.StateID,
                          s.StateAbbrev
                      });

        ddlState.DataSource = states;
        ddlState.DataTextField = "StateAbbrev";
        ddlState.DataValueField = "StateID";
        ddlState.DataBind();

        ddlState.Items.Insert(0, new ListItem("---Select---", "0"));
    }

    // Populate State DDL based on CountryID selected from DDL.
    protected void ddlCountry_SelectedIndexChanged(object sender, EventArgs e)
    {
        int countryID = 0;

        Int32.TryParse(ddlCountry.SelectedValue, out countryID);

        if (countryID == 0) return;

        PopulateStateDDL(countryID);
    }

    protected void ddlChannelTypes_SelectedIndexChanged(object sender, EventArgs e)
    {
        Int32.TryParse(ddlChannelTypes.SelectedValue, out channelID);

        if (channelID == 0)
        {
            txtSalesRepDistID.Text = "";
            lblCommissionPercentageRequired.Visible = false;
        }

        // Assign Distributor and Reseller IDs based on sequential numbering in DB.
        // Example: Last Distributor had SalesRepDistributorID = D001,
        //          the next Distributor will have a SalesRepDistributorID = D002.
        txtSalesRepDistID.ReadOnly = true;
        txtSalesRepDistID.Text = SetResellerOrDistributorID(channelID);

        // If Distributor is selected, Commission % can be between 0 and 40 and REQUIRED.
        if (channelID == 7)
        {
            lblCommissionPercentageRequired.Visible = true;
            txtCommissionPercentage.Text = "";
            txtCommissionPercentage.ReadOnly = false;
            txtRenewalCommissionPercentage.ReadOnly = false;
            txtSalesMgrCommissionPercentage.ReadOnly = false;
            txtCommissionRenewalYears.ReadOnly = false;
        }

        // If Reseller is selected, Commission % (not required) defaults to 0 and the control is Read-Only.
        if (channelID == 8)
        {
            lblCommissionPercentageRequired.Visible = false;
            txtCommissionPercentage.Text = "0";
            txtCommissionPercentage.ReadOnly = true;
            txtRenewalCommissionPercentage.Text = "0";
            txtRenewalCommissionPercentage.ReadOnly = true;
            txtSalesMgrCommissionPercentage.Text = "0";
            txtSalesMgrCommissionPercentage.ReadOnly = true;
            txtCommissionRenewalYears.Text = "0";
            txtCommissionRenewalYears.ReadOnly = true;
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "preventBackspace()", true);

        }
    }

    // Change requirement "*" between Email and Fax depending on what is selected.
    protected void ddlNotificationPerference_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddlNotificationPerference.SelectedValue == "EMAIL")
        {
            lblEmailRequired.Visible = true;
            lblFaxRequired.Visible = false;
        }
        else
        {
            lblEmailRequired.Visible = false;
            lblFaxRequired.Visible = true;
        }
    }

    // Initiate ALL DDL Controls for page.
    public void InitiateDDLControlsInModals()
    {
        PopulateSalesManagerControl();
        PopulateAgreementTypeControl();
        PopulateCurrencyCodeControl();
        PopulateCountryDDL();
    }

    // -------- BEGIN :: SRD MODAL CONTROLS VALIDATION FUNCTIONS -------- //
    private bool PassesInputValidation_SalesRepDistributorDialog()
    {
        string errorMsg = "";
        decimal localDecimal = 0;
        int localInt = 0;
        DateTime localStartDate;
        DateTime localEndDate;
        DateTime localSignDate;
        string emailAddress = "";
        string phoneNumber = "";
        string zipCode = "";
        Regex regexEmail = new Regex(@"^[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?");
        Regex regexPhoneNumber = new Regex(@"^((\(\d{3}\) ?)|(\d{3}-))?\d{3}-\d{4}");
        Regex regexZipCode = new Regex(@"^(\d{5}(-\d{4}))?");
        Int32.TryParse(ddlChannelTypes.SelectedValue, out channelID);

        // -------- BEGIN :: Check to see if Master Sales Rep. Information are entered and/or valid. -------- //
        // Required Channel Type (selection).
        if (ddlChannelTypes.SelectedValue == "0")
        {
            errorMsg = "Sales Rep. Channel is required.";
            VisibleErrors_SalesRepDistDialog(errorMsg);
            SetFocus(ddlChannelTypes);
            return false;
        }

        // Required formatting for SaleRepDistID.
        // Though this is automatically filled, this makes sure there is a value.
        if (txtSalesRepDistID.Text.Trim().Length == 0)
        {
            errorMsg = "Sales Rep. # is required.";
            VisibleErrors_SalesRepDistDialog(errorMsg);
            SetFocus(txtSalesRepDistID);
            return false;
        }
        else if (txtSalesRepDistID.Text.Trim().Length > 5)
        {
            errorMsg = "Sales Rep. # must be 5 characters or less.";
            VisibleErrors_SalesRepDistDialog(errorMsg);
            SetFocus(txtSalesRepDistID);
            return false;
        }

        // Required Sales Rep. Manager selection.
        if (ddlSalesManagerID.SelectedValue == "0")
        {
            errorMsg = "Sales Rep. Manager is required.";
            VisibleErrors_SalesRepDistDialog(errorMsg);
            SetFocus(ddlSalesManagerID);
            return false;
        }

        // Required Company Name.
        if (txtCompanyName.Text.Trim().Length == 0)
        {
            errorMsg = "Company Name required.";
            VisibleErrors_SalesRepDistDialog(errorMsg);
            SetFocus(txtCompanyName);
            return false;
        }

        // If Notification Preference = "EMAIL", E-Mail is required.
        if (ddlNotificationPerference.SelectedItem.Value == "EMAIL" && txtEmail.Text == "")
        {
            errorMsg = "E-mail is required.";
            VisibleErrors_SalesRepDistDialog(errorMsg);
            SetFocus(txtEmail);
            return false;
        }

        // If Notification Preference = "FAX", Fax # is required.
        if (ddlNotificationPerference.SelectedItem.Value == "FAX" && txtFax.Text == "")
        {
            errorMsg = "Fax # is required.";
            VisibleErrors_SalesRepDistDialog(errorMsg);
            SetFocus(txtFax);
            return false;
        }

        // Validate E-Mail formatting.
        if (ddlNotificationPerference.SelectedItem.Value == "EMAIL")
        {
            if (txtEmail.Text.Trim().Length != 0)
            {
                emailAddress = txtEmail.Text;
                Match match = regexEmail.Match(emailAddress);
                if (!match.Success)
                {
                    errorMsg = "Incorrect E-mail format (ex. johnsmith@somecompany.com).";
                    VisibleErrors_SalesRepDistDialog(errorMsg);
                    SetFocus(txtEmail);
                    return false;
                }
            }
        }

        // Validate Fax # formatting.
        if (ddlNotificationPerference.SelectedItem.Value == "FAX" || txtFax.Text.Trim().Length != 0)
        {
            if (txtFax.Text.Trim().Length != 0)
            {
                phoneNumber = txtFax.Text;
                Match match = regexPhoneNumber.Match(phoneNumber);
                if (!match.Success)
                {
                    errorMsg = "Incorrect Fax # format (ex. (949) 555-1212).";
                    VisibleErrors_SalesRepDistDialog(errorMsg);
                    SetFocus(txtFax);
                    return false;
                }
            }
        }

        // Formatting for Phone #.
        if (txtPhone.Text.Trim().Length != 0)
        {
            phoneNumber = txtPhone.Text;
            Match match = regexPhoneNumber.Match(phoneNumber);
            if (!match.Success)
            {
                errorMsg = "Incorrect Phone # format (ex. (949) 555-1212).";
                VisibleErrors_SalesRepDistDialog(errorMsg);
                SetFocus(txtPhone);
                return false;
            }
        }

        // Formatting of Customer Service Phone #.
        if (txtCustServicePhone.Text.Trim().Length != 0)
        {
            phoneNumber = txtCustServicePhone.Text;
            Match match = regexPhoneNumber.Match(phoneNumber);
            if (!match.Success)
            {
                errorMsg = "Incorrect Customer Service Phone # format (ex. (949) 555-1212).";
                VisibleErrors_SalesRepDistDialog(errorMsg);
                SetFocus(txtCustServicePhone);
                return false;
            }
        }

        // Formatting of Zip Code.
        if (txtZipCode.Text.Trim().Length != 0)
        {
            zipCode = txtZipCode.Text;
            Match match = regexZipCode.Match(zipCode);
            if (!match.Success)
            {
                errorMsg = "Incorrect Zip Code format (ex. 12345).";
                VisibleErrors_SalesRepDistDialog(errorMsg);
                SetFocus(txtZipCode);
                return false;
            }
        }
        // -------- END :: Check to see if Master Sales Rep. Information are entered and/or valid. -------- //

        // ---------------- BEGIN :: Check to see if Commission Details are entered and/or valid. ---------------- //
        // Commission %.
        if (txtCommissionPercentage.Text.Trim().Length != 0)
        {
            decimal.TryParse(txtCommissionPercentage.Text, out localDecimal);

            if (localDecimal > 40)
            {
                errorMsg = "Commission % cannot be greater than 40.";
                VisibleErrors_SalesRepDistDialog(errorMsg);
                SetFocus(txtCommissionPercentage);
                return false;
            }
        }

        // If Distributor (ChannelID = 7)
        // Commission % is mandatory, but free form (% should be between 0-40).
        if (channelID == 7 && txtCommissionPercentage.Text.Trim().Length == 0)
        {
            errorMsg = "Please enter a Commission % between 0 to 40.";
            VisibleErrors_SalesRepDistDialog(errorMsg);
            SetFocus(txtCommissionPercentage);
            return false;
        }

        // Renewal Commission % is required regardless of ChannelID.
        if (txtRenewalCommissionPercentage.Text.Trim().Length == 0 || decimal.TryParse(txtRenewalCommissionPercentage.Text, out localDecimal) == false)
        {
            errorMsg = "Renewal Commission % is required and must be a number.";
            VisibleErrors_SalesRepDistDialog(errorMsg);
            SetFocus(txtRenewalCommissionPercentage);
            return false;
        }
        else if (decimal.TryParse(txtRenewalCommissionPercentage.Text, out localDecimal))
        {
            if (localDecimal < 0 || localDecimal > 10)
            {
                errorMsg = "Renewal Commission % cannot be less than 0 or greater than 10.";
                VisibleErrors_SalesRepDistDialog(errorMsg);
                SetFocus(txtRenewalCommissionPercentage);
                return false;
            }
        }

        // Sales Manager Commission % is required regardless of ChannelID.
        if (txtSalesMgrCommissionPercentage.Text.Trim().Length == 0 || decimal.TryParse(txtSalesMgrCommissionPercentage.Text, out localDecimal) == false)
        {
            errorMsg = "Sales Manager Commission % is required and must be a number.";
            VisibleErrors_SalesRepDistDialog(errorMsg);
            SetFocus(txtSalesMgrCommissionPercentage);
            return false;
        }
        else if (decimal.TryParse(txtSalesMgrCommissionPercentage.Text, out localDecimal))
        {
            if (localDecimal < 0 || localDecimal > 10)
            {
                errorMsg = "Sales Manager Commission % cannot be less than 0 or greater than 10.";
                VisibleErrors_SalesRepDistDialog(errorMsg);
                SetFocus(txtSalesMgrCommissionPercentage);
                return false;
            }
        }

        // Renewal Commission Years.
        if (txtCommissionRenewalYears.Text.Trim().Length == 0 || int.TryParse(txtCommissionRenewalYears.Text, out localInt) == false)
        {
            errorMsg = "Renewal Commission Years is required and must be a number.";
            VisibleErrors_SalesRepDistDialog(errorMsg);
            SetFocus(txtCommissionRenewalYears);
            return false;
        }
        else if (int.TryParse(txtCommissionRenewalYears.Text, out localInt))
        {
            if (localInt < 0)
            {
                errorMsg = "Renewal Commission Years cannot be less than 0.";
                VisibleErrors_SalesRepDistDialog(errorMsg);
                SetFocus(txtCommissionRenewalYears);
                return false;
            }
        }

        // Contract Sign Date is required.
        if (txtContractSignDate.Text.Trim().Length == 0)
        {
            errorMsg = "Contract Sign Date must be entered.";
            VisibleErrors_SalesRepDistDialog(errorMsg);
            SetFocus(txtContractStartDate);
            return false;
        }

        // Contract Start Date is required.
        if (txtContractStartDate.Text.Trim().Length == 0)
        {
            errorMsg = "Contract Start Date must be entered.";
            VisibleErrors_SalesRepDistDialog(errorMsg);
            SetFocus(txtContractStartDate);
            return false;
        }

        // Contract End Date is required.
        if (txtContractEndDate.Text.Trim().Length == 0)
        {
            errorMsg = "Contract End Date must be entered.";
            VisibleErrors_SalesRepDistDialog(errorMsg);
            SetFocus(txtContractEndDate);
            return false;
        }

        // Check to make sure that the Contract Start Date is less than the Contract End Date.
        // The Contract End Date is editable, but is defaulted to +5 years from Contract Start Date.
        if (txtContractStartDate.Text.Trim().Length != 0 && txtContractEndDate.Text.Trim().Length != 0)
        {
            DateTime.TryParse(txtContractStartDate.Text, out localStartDate);
            DateTime.TryParse(txtContractEndDate.Text, out localEndDate);

            if (localStartDate > localEndDate)
            {
                errorMsg = "Contract Start Date must be before the Contract End Date.";
                VisibleErrors_SalesRepDistDialog(errorMsg);
                SetFocus(txtContractStartDate);
                return false;
            }
        }

        // Check to make sure that the Contract Sign Date is less than/equal to the Contract Start Date.
        // The Contract Start Date is defaulted to the Contract Sign Date.
        if (txtContractStartDate.Text.Trim().Length != 0 && txtContractEndDate.Text.Trim().Length != 0)
        {
            DateTime.TryParse(txtContractStartDate.Text, out localStartDate);
            DateTime.TryParse(txtContractSignDate.Text, out localSignDate);

            if (localSignDate > localStartDate)
            {
                errorMsg = "Contract Sign Date must be on or before the Contract Start Date.";
                VisibleErrors_SalesRepDistDialog(errorMsg);
                SetFocus(txtContractSignDate);
                return false;
            }
        }

        // Formatting of Old Quantum Code.
        if (txtOldQuantumCode.Text.Trim().Length > 4)
        {
            errorMsg = "Old Quantum Code must be 4 characters or less.";
            VisibleErrors_SalesRepDistDialog(errorMsg);
            SetFocus(txtOldQuantumCode);
            return false;
        }

        // Formatting of Reseller Price.
        if (txtResellerPrice.Text.Trim().Length > 0 && decimal.TryParse(txtResellerPrice.Text, out localDecimal) == false)
        {
            errorMsg = "Reseller Price must be a number.";
            VisibleErrors_SalesRepDistDialog(errorMsg);
            SetFocus(txtResellerPrice);
            return false;
        }
        else if (decimal.TryParse(txtResellerPrice.Text, out localDecimal))
        {
            if (localDecimal < 0)
            {
                errorMsg = "Reseller Price cannot be less than 0.";
                VisibleErrors_SalesRepDistDialog(errorMsg);
                SetFocus(txtResellerPrice);
                return false;
            }
        }

        // Formatting of # of Trifolds.
        if (txtNumberOfTrifolds.Text.Trim().Length > 0 && int.TryParse(txtNumberOfTrifolds.Text, out localInt) == false)
        {
            errorMsg = "# of Trifolds must be a number.";
            VisibleErrors_SalesRepDistDialog(errorMsg);
            SetFocus(txtNumberOfTrifolds);
            return false;
        }
        else if (int.TryParse(txtNumberOfTrifolds.Text, out localInt))
        {
            if (localInt < 0)
            {
                errorMsg = "# of Trifolds cannot be less than 0.";
                VisibleErrors_SalesRepDistDialog(errorMsg);
                SetFocus(txtNumberOfTrifolds);
                return false;
            }
        }
        // -------- END :: Check to see if Commission Details are entered and/or valid. -------- //

        return true;
    }
    // -------- END :: SRD MODAL CONTROLS VALIDATION FUNCTIONS -------- //

    // Initiate Default Values on ALL of the Controls for the Sales Rep. Distributor Modal/Dialog.
    private void SetDefaultValues_SalesRepDistDialog()
    {
        InvisibleErrors_SalesRepDistDialog();
        hdnfldAccordionIndex.Value = "0";
        hdnfldSalesRepDistID.Value = "";
        txtSalesRepDistID.Text = "";
        txtSalesRepDistID.Enabled = true;
        ddlChannelTypes.SelectedValue = "0";
        ddlChannelTypes.Enabled = true;
        ddlSalesManagerID.SelectedValue = "0";
        ddlPrefix.SelectedValue = "0";
        txtFirstName.Text = "";
        txtMiddleName.Text = "";
        txtLastName.Text = "";
        txtPhone.Text = "";
        txtFax.Text = "";
        txtCustServicePhone.Text = "";
        ddlNotificationPerference.SelectedValue = "EMAIL";
        txtCompanyName.Text = "";
        txtContactName.Text = "";
        txtEmail.Text = "";
        txtAddress1.Text = "";
        txtAddress2.Text = "";
        txtCity.Text = "";
        ddlCountry.SelectedValue = "0";
        ddlState.SelectedIndex = 0;
        //PopulateStateDDL(1);
        txtZipCode.Text = "";
        // Commission Setup DDL list is cleared and control is not visible.
        // This control is used only if the Commission Detail is be edited.
        ddlCommissionSetupID.Items.Clear();
        ddlCommissionSetupID.Visible = false;
        // Set (default) values to Commission Details (% and Years controls).
        txtCommissionPercentage.Text = "";
        txtRenewalCommissionPercentage.Text = "0";
        txtSalesMgrCommissionPercentage.Text = "0";
        txtCommissionRenewalYears.Text = "3";
        // Set (default) Date to Today()/Now() for Contract Start Date.
        txtContractStartDate.Text = String.Format("{0:MM/dd/yyyy}", currentDate);
        txtContractStartDate.Attributes.Add("readonly", "readonly");
        // Set (default) Date to Today()/Now() + 5 years for Contract End Date.
        txtContractEndDate.Text = String.Format("{0:MM/dd/yyyy}", currentDate.AddYears(5));
        txtContractEndDate.Attributes.Add("readonly", "readonly");
        chkbxPORequired.Checked = false;
        divCommissionDetailsButtons.Visible = false;
        txtOldQuantumCode.Text = "";
        ddlAgreementType.SelectedValue = "0";
        txtResellerPrice.Text = "";
        ddlResellerPriceCurrencyCode.SelectedValue = "USD";
        // Set (default) Date to Today()/Now() for Contract Sign Date.
        txtContractSignDate.Text = String.Format("{0:MM/dd/yyyy}", currentDate);
        txtContractSignDate.Attributes.Add("readonly", "readonly");
        txtNumberOfTrifolds.Text = "";
        chkbxCodeOfEthicsYes.Checked = false;
        txtShipDate.Text = "";
        txtShipDate.Attributes.Add("readonly", "readonly");
        txtDataSheetSentDate.Text = "";
        txtDataSheetSentDate.Attributes.Add("readonly", "readonly");
        chkbxSpecialPriceOfTrifoldYes.Checked = false;
        txtNotes.Text = "";
    }

    // Used to Show/Hide "Deactivate|Activate Record" <div>.
    private void ShowHideSalesRepDistStatus(bool isvisible)
    {
        divToggleSalesRepDistributorStatus.Visible = isvisible;
    }

    // Add [NEW] Commission Details.
    protected void btnAddEditSalesRepDistributor_Click(object sender, EventArgs e)
    {
        // Set Modal/Dialog form mode to "ADD".
        hdnfldFormMode.Value = "ADD";

        SetDefaultValues_SalesRepDistDialog();
        ShowHideSalesRepDistStatus(false);
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('divSalesRepDistributorDialog')", true);
        ddlChannelTypes.Focus();
    }

    // Cancel Modal/Dialog Form.
    protected void btnCancel_Click(object sender, EventArgs e)
    {
        // Reset Modal/InvisibleErrors_Dialog form to BLANK.
        hdnfldFormMode.Value = "";
        SetDefaultValues_SalesRepDistDialog();
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divSalesRepDialog')", true);
    }

    // Get most recent CommissionSetupID based on SaleRepDistID.
    private int GetNewestCommissionSetupID(string salesrepdistid)
    {
        int csid = 0;

        csid = (from c in idc.CommissionSetups
                orderby c.ContractStartDate descending
                where c.SalesRepDistID == salesrepdistid
                select c.CommissionSetupID).FirstOrDefault();

        return csid;
    }

    // Depending on selection of Commission ID from DDL, respective fields in Modal/Dialog.
    protected void ddlCommissionSetupID_SelectedIndexChanged(object sender, EventArgs e)
    {
        salesRepDistID = hdnfldSalesRepDistID.Value;
        Int32.TryParse(ddlCommissionSetupID.SelectedValue, out commissionSetupID);

        ToggleCommissionSetupIDControls(commissionSetupID);

        var csid = (from c in idc.CommissionSetups
                    where c.SalesRepDistID == salesRepDistID && c.CommissionSetupID == commissionSetupID
                    select c).FirstOrDefault();

        // Commission Details controls.
        txtCommissionPercentage.Text = csid.CommissionPct.HasValue ? Convert.ToString(csid.CommissionPct) : "";
        txtRenewalCommissionPercentage.Text = csid.RenewalCommissionPct == null ? "0" : Convert.ToString(csid.RenewalCommissionPct);
        txtSalesMgrCommissionPercentage.Text = cs.SalesMgrCommissionPct == null ? "0" : Convert.ToString(csid.SalesMgrCommissionPct);
        txtCommissionRenewalYears.Text = csid.CommRenewalYears == null ? "0" : Convert.ToString(csid.CommRenewalYears);
        txtContractStartDate.Text = csid.ContractStartDate == null ? "" : String.Format("{0:MM/dd/yyyy}", csid.ContractStartDate);
        txtContractEndDate.Text = csid.ContractEndDate == null ? "" : String.Format("{0:MM/dd/yyyy}", csid.ContractEndDate);
        chkbxPORequired.Checked = csid.PORequired == true ? true : false;
        txtOldQuantumCode.Text = csid.OldQuantumCode == null ? "" : csid.OldQuantumCode;
        ddlAgreementType.SelectedValue = csid.AgreementType == null ? "0" : csid.AgreementType;
        txtResellerPrice.Text = csid.ResellerPrice.HasValue ? String.Format("{0:#0.00}", csid.ResellerPrice) : "0.00";
        ddlResellerPriceCurrencyCode.SelectedValue = csid.ResellerPriceCurrCode == null ? "0" : csid.ResellerPriceCurrCode;
        txtContractSignDate.Text = csid.ContractSignedDate.HasValue ? String.Format("{0:MM/dd/yyyy}", csid.ContractSignedDate) : "";
        txtNumberOfTrifolds.Text = csid.TrifoldQty.HasValue ? Convert.ToString(csid.TrifoldQty) : "";
        chkbxCodeOfEthicsYes.Checked = csid.IsCodeOfEthic == true ? true : false;
        txtShipDate.Text = csid.ShipDate.HasValue ? String.Format("{0:MM/dd/yyyy}", csid.ShipDate) : "";
        txtDataSheetSentDate.Text = csid.DataSheetSentDate.HasValue ? String.Format("{0:MM/dd/yyyy}", csid.DataSheetSentDate) : "";
        chkbxSpecialPriceOfTrifoldYes.Checked = csid.IsSpecialPriceTrifold == true ? true : false;
        txtNotes.Text = csid.Notes == null ? "" : csid.Notes;
    }

    // If Commission Setup ID is ACTIVE, then show "Deactivate" button. Else show "Activate" button.
    private void ToggleCommissionSetupIDControls(int commissionsetupid)
    {
        bool status = (from c in idc.CommissionSetups
                       where c.CommissionSetupID == commissionsetupid
                       select c.Active).FirstOrDefault();

        if (status == true)
        {
            btnCommissionDetailStatus.Text = "Deactivate";
            txtCommissionPercentage.Enabled = true;
            txtRenewalCommissionPercentage.Enabled = true;
            txtSalesMgrCommissionPercentage.Enabled = true;
            txtCommissionRenewalYears.Enabled = true;
            txtContractSignDate.Enabled = true;
            txtContractStartDate.Enabled = true;
            txtContractEndDate.Enabled = true;
            chkbxPORequired.Enabled = true;
            txtOldQuantumCode.Enabled = true;
            ddlAgreementType.Enabled = true;
            txtResellerPrice.Enabled = true;
            ddlResellerPriceCurrencyCode.Enabled = true;
            txtNumberOfTrifolds.Enabled = true;
            chkbxCodeOfEthicsYes.Enabled = true;
            txtShipDate.Enabled = true;
            txtDataSheetSentDate.Enabled = true;
            chkbxSpecialPriceOfTrifoldYes.Enabled = true;
            txtNotes.Enabled = true;
        }
        else
        {
            btnCommissionDetailStatus.Text = "Activate";
            txtCommissionPercentage.Enabled = false;
            txtRenewalCommissionPercentage.Enabled = false;
            txtSalesMgrCommissionPercentage.Enabled = false;
            txtCommissionRenewalYears.Enabled = false;
            txtContractSignDate.Enabled = false;
            txtContractStartDate.Enabled = false;
            txtContractEndDate.Enabled = false;
            chkbxPORequired.Enabled = false;
            txtOldQuantumCode.Enabled = false;
            ddlAgreementType.Enabled = false;
            txtResellerPrice.Enabled = false;
            ddlResellerPriceCurrencyCode.Enabled = false;
            txtNumberOfTrifolds.Enabled = false;
            chkbxCodeOfEthicsYes.Enabled = false;
            txtShipDate.Enabled = false;
            txtDataSheetSentDate.Enabled = false;
            chkbxSpecialPriceOfTrifoldYes.Enabled = false;
            txtNotes.Enabled = false;
        }
    }

    // IF ACTIVE, enable all related controls, ELSE disable all related controls.
    protected void btnCommissionDetailStatus_Click(object sender, EventArgs e)
    {
        string status = btnCommissionDetailStatus.Text;
        salesRepDistID = hdnfldSalesRepDistID.Value;
        Int32.TryParse(ddlCommissionSetupID.SelectedValue, out commissionSetupID);
        userName = GetUserName();

        cs = (from c in idc.CommissionSetups
              where c.SalesRepDistID == salesRepDistID && c.CommissionSetupID == commissionSetupID
              select c).FirstOrDefault();

        if (status == "Activate")
        {
            // Activate record in CommissionSetup table in DB.
            cs.Active = true;
            cs.DeactivateDate = currentDate;
            cs.DeactivatedBy = userName;
            ToggleCommissionSetupIDControls(commissionSetupID);
        }
        else if (status == "Deactivate")
        {
            // Deactivate record in CommissionSetup table in DB.
            cs.Active = false;
            cs.DeactivateDate = currentDate;
            cs.DeactivatedBy = userName;
            ToggleCommissionSetupIDControls(commissionSetupID);
        }

        idc.SubmitChanges();

        ToggleCommissionSetupIDControls(commissionSetupID);

        // Refresh GridView
        gvSalesRepDistributor.DataBind();
    }

    // See if Sales Rep. Distributor is Active or Inactive (BOOLEAN).
    // Used for Activate/Deactivate Accordion Panel.
    // If Active == true then Accordion Panel Label = "Deactivate Record" and Button Text = "Deactivate Record".
    // If Active == false then Accordion Panel Label = "Activate Record" and Button Text = "Activate Record".
    private void ToggleAndSetSalesRepDistAccordion(string salesrepid)
    {
        bool status = true;

        DataTable dtIsRecordActive = new DataTable("Is Record Active");

        dtIsRecordActive.Columns.Add("Active", Type.GetType("System.Boolean"));

        status = (from s in idc.SalesRepDistributors
                  where s.SalesRepDistID == salesrepid
                  select s.Active).FirstOrDefault();

        if (status == true)
        {
            lblChangeStatusOfSalesRepDistributor.Text = "Deactivate Record";
            btnDeactivateSaleRepDistributorRecord.Text = "Deactivate Record";
        }
        else
        {
            lblChangeStatusOfSalesRepDistributor.Text = "Activate Record";
            btnDeactivateSaleRepDistributorRecord.Text = "Activate Record";
        }
    }

    // Edit Commission Details (GridView CommandRow).
    protected void imgbtnEditSalesRepDistInfo_Click(object sender, EventArgs e)
    {
        ImageButton imgbtn = (ImageButton)sender;
        salesRepDistID = imgbtn.CommandArgument;
        hdnfldFormMode.Value = "EDIT";
        ShowHideSalesRepDistStatus(true);
        hdnfldSalesRepDistID.Value = salesRepDistID;    // Store Sales Rep. ID on page (Client Side).
        ToggleAndSetSalesRepDistAccordion(salesRepDistID);
        // Get CommissionSetupID.
        this.ddlCommissionSetupID.Visible = true;
        commissionSetupID = GetNewestCommissionSetupID(salesRepDistID);
        this.ddlCommissionSetupID.SelectedValue = Convert.ToString(commissionSetupID);
        this.divCommissionDetailsButtons.Visible = true;
        ToggleCommissionSetupIDControls(commissionSetupID);

        // If SalesRepDistID (Account) is Active, show ALL accordion panels.
        // Else, just show the Activate/Deactive accordion panel (User must activate Account first).
        StateOfFormBasedOnAccountStatus();

        // Get values for given Sales Rep. ID and Commission ID from DB.
        var srdcs = (from s in idc.SalesRepDistributors
                     join c in idc.CommissionSetups on s.SalesRepDistID equals c.SalesRepDistID
                     where s.SalesRepDistID == salesRepDistID && c.CommissionSetupID == commissionSetupID
                     select new { s, c }).FirstOrDefault();

        // General Information controls.
        txtSalesRepDistID.Text = salesRepDistID;
        txtSalesRepDistID.Enabled = false;
        ddlChannelTypes.SelectedValue = srdcs.s.ChannelID.ToString();
        ddlChannelTypes.Enabled = false;
        ddlSalesManagerID.SelectedValue = srdcs.c.SalesManagerID == null ? "0" : srdcs.c.SalesManagerID;
        ddlPrefix.SelectedValue = srdcs.s.Prefix == null ? "0" : srdcs.s.Prefix;
        txtFirstName.Text = srdcs.s.FirstName == null ? "" : srdcs.s.FirstName;
        txtMiddleName.Text = srdcs.s.MiddleName == null ? "" : srdcs.s.MiddleName;
        txtLastName.Text = srdcs.s.LastName == null ? "" : srdcs.s.LastName;
        txtPhone.Text = srdcs.s.Telephone == null ? "" : srdcs.s.Telephone;
        txtFax.Text = srdcs.s.Fax == null ? "" : srdcs.s.Fax;
        txtCustServicePhone.Text = srdcs.s.CustServiceTelephone == null ? "" : srdcs.s.CustServiceTelephone;
        ddlNotificationPerference.SelectedValue = srdcs.s.NotificationPref == null ? "0" : srdcs.s.NotificationPref.ToUpper();
        txtCompanyName.Text = srdcs.s.CompanyName == null ? "" : srdcs.s.CompanyName;
        txtContactName.Text = srdcs.s.ContactName == null ? "" : srdcs.s.ContactName;
        txtEmail.Text = srdcs.s.Email == null ? "" : srdcs.s.Email;
        txtAddress1.Text = srdcs.s.Address1 == null ? "" : srdcs.s.Address1;
        txtAddress2.Text = srdcs.s.Address2 == null ? "" : srdcs.s.Address2;
        txtCity.Text = srdcs.s.City == null ? "" : srdcs.s.City;
        ddlCountry.SelectedValue = srdcs.s.CountryID.HasValue ? srdcs.s.CountryID.ToString() : "0";
        PopulateStateDDL(Convert.ToInt32(ddlCountry.SelectedValue));
        ddlState.SelectedValue = srdcs.s.StateID.HasValue ? srdcs.s.StateID.ToString() : "0";
        txtZipCode.Text = srdcs.s.PostalCode == null ? "" : srdcs.s.PostalCode;
        // If Sales Rep. is a RESELLER (ChallelID == 8),
        // Commission %, Renewal Commission %, Sales Manager Commission %, and Commission Renewal Years should all default to 0.
        // Also, their respective controls should be read-only.
        if (ddlChannelTypes.SelectedValue == "8")
        {
            lblCommissionPercentageRequired.Visible = false;
            txtCommissionPercentage.Text = "0";
            txtCommissionPercentage.ReadOnly = true;
            txtRenewalCommissionPercentage.Text = "0";
            txtRenewalCommissionPercentage.ReadOnly = true;
            txtSalesMgrCommissionPercentage.Text = "0";
            txtSalesMgrCommissionPercentage.ReadOnly = true;
            txtCommissionRenewalYears.Text = "0";
            txtCommissionRenewalYears.ReadOnly = true;
        }
        else
        {
            lblCommissionPercentageRequired.Visible = true;
            txtCommissionPercentage.Text = srdcs.c.CommissionPct.HasValue ? srdcs.c.CommissionPct.ToString() : "0";
            txtCommissionPercentage.ReadOnly = false;
            txtRenewalCommissionPercentage.Text = srdcs.c.RenewalCommissionPct.ToString();
            txtRenewalCommissionPercentage.ReadOnly = false;
            txtSalesMgrCommissionPercentage.Text = srdcs.c.SalesMgrCommissionPct.ToString();
            txtSalesMgrCommissionPercentage.ReadOnly = false;
            txtCommissionRenewalYears.Text = srdcs.c.CommRenewalYears.ToString();
            txtCommissionRenewalYears.ReadOnly = false;
        }
        txtContractStartDate.Text = String.Format("{0:MM/dd/yyyy}", srdcs.c.ContractStartDate);
        txtContractEndDate.Text = String.Format("{0:MM/dd/yyyy}", srdcs.c.ContractEndDate);
        chkbxPORequired.Checked = srdcs.c.PORequired == true ? true : false;
        txtOldQuantumCode.Text = srdcs.c.OldQuantumCode == null ? "" : srdcs.c.OldQuantumCode;
        ddlAgreementType.SelectedValue = srdcs.c.AgreementType == null ? "0" : srdcs.c.AgreementType;
        txtResellerPrice.Text = srdcs.c.ResellerPrice.HasValue ? String.Format("{0:#0.00}", srdcs.c.ResellerPrice) : "0.00";
        ddlResellerPriceCurrencyCode.SelectedValue = srdcs.c.ResellerPriceCurrCode == null ? "0" : srdcs.c.ResellerPriceCurrCode;
        txtContractSignDate.Text = srdcs.c.ContractSignedDate.HasValue ? String.Format("{0:MM/dd/yyyy}", srdcs.c.ContractSignedDate) : String.Format("{0:MM/dd/yyyy}", srdcs.c.ContractStartDate);
        txtNumberOfTrifolds.Text = srdcs.c.TrifoldQty.HasValue ? srdcs.c.TrifoldQty.ToString() : "0";
        chkbxCodeOfEthicsYes.Checked = srdcs.c.IsCodeOfEthic == true ? true : false;
        txtShipDate.Text = srdcs.c.ShipDate.HasValue ? srdcs.c.ShipDate.ToString() : "";
        txtDataSheetSentDate.Text = srdcs.c.DataSheetSentDate.HasValue ? srdcs.c.DataSheetSentDate.ToString() : "";
        chkbxSpecialPriceOfTrifoldYes.Checked = srdcs.c.IsSpecialPriceTrifold == true ? true : false;
        txtNotes.Text = srdcs.c.Notes == null ? "" : srdcs.c.Notes;

        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('divSalesRepDistributorDialog')", true);
    }

    // Add or Edit Sales Rep. & Commission Details to Database.
    // Based upon Form Mode (ADD/EDIT value in HiddenField).
    protected void btnSave_Click(object sender, EventArgs e)
    {
        string errorMsg = "";
        string formMode = hdnfldFormMode.Value;
        bool doesFormPass = PassesInputValidation_SalesRepDistributorDialog();

        if (formMode == "") return;

        if (!doesFormPass) return;

        // Set Control values to a Variable.
        salesRepDistID = txtSalesRepDistID.Text.Trim().ToUpper();
        channelID = Convert.ToInt32(ddlChannelTypes.SelectedValue);
        string strSalesManagerID = (ddlSalesManagerID.SelectedValue == "0" ? null : ddlSalesManagerID.SelectedValue);
        string strPrefix = (ddlPrefix.SelectedValue == "0" ? null : ddlPrefix.SelectedValue);
        string strFirstName = (txtFirstName.Text.Trim().Length == 0 ? null : txtFirstName.Text.Trim().Replace("'", "''"));
        string strMiddleName = (txtMiddleName.Text.Trim().Length == 0 ? null : txtMiddleName.Text.Trim().Replace("'", "''"));
        string strLastName = (txtLastName.Text.Trim().Length == 0 ? null : txtLastName.Text.Trim().Replace("'", "''"));
        string strPhoneNo = (txtPhone.Text.Trim().Length == 0 ? null : txtPhone.Text.Trim());
        string strFaxNo = (txtFax.Text.Trim().Length == 0 ? null : txtFax.Text.Trim());
        string strCustomerServiceNo = (txtCustServicePhone.Text.Trim().Length == 0) ? null : txtCustServicePhone.Text.Trim();
        string strNotificationPreference = ddlNotificationPerference.SelectedValue;
        string strCompanyName = txtCompanyName.Text.Trim().Replace("'", "''");
        string strContactName = (txtContactName.Text.Trim().Length == 0 ? null : txtContactName.Text.Trim().Replace("'", "''"));
        string strEmail = (txtEmail.Text.Trim().Length == 0 ? null : txtEmail.Text.Trim());
        string strAddress1 = (txtAddress1.Text.Trim().Length == 0 ? null : txtAddress1.Text.Trim().Replace("'", "''"));
        string strAddress2 = (txtAddress2.Text.Trim().Length == 0 ? null : txtAddress2.Text.Trim().Replace("'", "''"));
        // Create a Nullable Integer variable.
        int? intCountryID = 0;
        if (ddlCountry.SelectedValue != "0")
        {
            intCountryID = Convert.ToInt32(ddlCountry.SelectedValue);
        }
        else
        {
            intCountryID = null;
        }
        int? intStateID = 0;
        if (ddlState.SelectedValue != "0")
        {
            intStateID = Convert.ToInt32(ddlState.SelectedValue);
        }
        else
        {
            intStateID = null;
        }
        string strCity = (txtCity.Text.Trim().Length == 0 ? null : txtCity.Text.Trim().Replace("'", "''"));
        string strZipCode = (txtZipCode.Text.Trim().Length == 0 ? null : txtZipCode.Text);
        decimal decCommissionPercentage;
        decimal? result_cp = null;
        if (decimal.TryParse(txtCommissionPercentage.Text, out decCommissionPercentage))
        {
            result_cp = decCommissionPercentage;
        }
        else
        {
            result_cp = null;
        }
        decimal decRenewalCommissionPercentage = (txtRenewalCommissionPercentage.Text.Trim().Length == 0 ? 0 : Convert.ToDecimal(txtRenewalCommissionPercentage.Text.Trim()));
        decimal decSalesMgrCommissionPercentage = (txtSalesMgrCommissionPercentage.Text.Trim().Length == 0 ? 0 : Convert.ToDecimal(txtSalesMgrCommissionPercentage.Text.Trim()));
        int intCommissionRenewalYears = (txtCommissionRenewalYears.Text.Trim().Length == 0 ? 0 : Convert.ToInt32(txtCommissionRenewalYears.Text.Trim()));
        DateTime dtContractStartDate = Convert.ToDateTime(txtContractStartDate.Text.Trim());
        DateTime dtContractEndDate = Convert.ToDateTime(txtContractEndDate.Text.Trim());
        int? intRateID = null;
        bool boolPORequired = (chkbxPORequired.Checked == true ? true : false);
        string strOldQuantumCode = (txtOldQuantumCode.Text.Trim().Length == 0 ? null : txtOldQuantumCode.Text.Trim());
        string strAgreementType = (ddlAgreementType.SelectedValue == "0" ? null : ddlAgreementType.SelectedValue);
        decimal decResellerPrice;
        decimal? result_rp = null;
        if (decimal.TryParse(txtResellerPrice.Text, out decResellerPrice))
        {
            result_rp = decResellerPrice;
        }
        else
        {
            result_rp = null;
        }
        DateTime? dtContractSignedDate;
        if (txtContractSignDate.Text.Trim().Length != 0)
        {
            dtContractSignedDate = Convert.ToDateTime(txtContractSignDate.Text.Trim());
        }
        else
        {
            dtContractSignedDate = null;
        }
        int intNumberOfTrifolds = (txtNumberOfTrifolds.Text.Trim().Length == 0 ? Convert.ToInt32("0") : Convert.ToInt32(txtNumberOfTrifolds.Text));
        bool boolCodeOfEthics = (chkbxCodeOfEthicsYes.Checked == true ? true : false);
        DateTime? dtShipDate;
        if (txtShipDate.Text.Trim().Length != 0)
        {
            dtShipDate = Convert.ToDateTime(txtShipDate.Text.Trim());
        }
        else
        {
            dtShipDate = null;
        }
        DateTime? dtDataSheetSentDate;
        if (txtDataSheetSentDate.Text.Trim().Length != 0)
        {
            dtDataSheetSentDate = Convert.ToDateTime(txtDataSheetSentDate.Text.Trim());
        }
        else
        {
            dtDataSheetSentDate = null;
        }
        bool boolSpecialPriceOfTrifold = (chkbxSpecialPriceOfTrifoldYes.Checked == true ? true : false);
        string strNotes = (txtNotes.Text.Trim().Length == 0 ? null : txtNotes.Text.Trim().Replace("'", "''"));
        string strResellerPriceCurrencyCode = (ddlResellerPriceCurrencyCode.SelectedValue == "0" ? null : ddlResellerPriceCurrencyCode.SelectedValue);

        int numberOfRecords = GetCountOfSalesRepID(salesRepDistID);

        switch (formMode)
        {
            case "ADD":
                // Sales Rep. ID does not currently exist in Database, OK to add record.
                if (numberOfRecords == 0)
                {
                    // Assign information to SalesRepDistributors table in DB.
                    srd = new SalesRepDistributor
                    {
                        SalesRepDistID = salesRepDistID,
                        ContactName = strContactName,
                        CompanyName = strCompanyName,
                        Prefix = strPrefix,
                        FirstName = strFirstName,
                        MiddleName = strMiddleName,
                        LastName = strLastName,
                        Address1 = strAddress1,
                        Address2 = strAddress2,
                        City = strCity,
                        StateID = intStateID,
                        PostalCode = strZipCode,
                        CountryID = intCountryID,
                        Email = strEmail,
                        Telephone = strPhoneNo,
                        Fax = strFaxNo,
                        CustServiceTelephone = strCustomerServiceNo,
                        NotificationPref = strNotificationPreference,
                        ChannelID = channelID,
                        Active = true
                    };

                    // Assign information to CommissionSetup table in DB.
                    cs = new CommissionSetup
                    {
                        SalesRepDistID = salesRepDistID,
                        SalesManagerID = strSalesManagerID,
                        CommissionPct = decCommissionPercentage,
                        SalesMgrCommissionPct = decSalesMgrCommissionPercentage,
                        RenewalCommissionPct = decRenewalCommissionPercentage,
                        PORequired = boolPORequired,
                        ContractStartDate = dtContractStartDate,
                        ContractEndDate = dtContractEndDate,
                        RateID = intRateID,
                        OldQuantumCode = strOldQuantumCode,
                        AgreementType = strAgreementType,
                        IsCodeOfEthic = boolCodeOfEthics,
                        ContractSignedDate = dtContractSignedDate,
                        ResellerPrice = decResellerPrice,
                        ResellerPriceCurrCode = strResellerPriceCurrencyCode,
                        TrifoldQty = intNumberOfTrifolds,
                        IsSpecialPriceTrifold = boolSpecialPriceOfTrifold,
                        ShipDate = dtShipDate,
                        DataSheetSentDate = dtDataSheetSentDate,
                        Notes = strNotes,
                        CommRenewalYears = intCommissionRenewalYears,
                        CommissionEvent = COMMISSIONEVENT,
                        Active = true
                    };

                    try
                    {
                        idc.SalesRepDistributors.InsertOnSubmit(srd);
                        idc.CommissionSetups.InsertOnSubmit(cs);
                        idc.SubmitChanges();

                        sendEmailAddNotice(salesRepDistID, strFirstName, strLastName);

                        // Refresh GridView.
                        gvSalesRepDistributor.DataBind();

                        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divSalesRepDistributorDialog')", true);
                    }
                    catch (Exception ex)
                    {
                        // Display the system-generated error message.
                        VisibleErrors_SalesRepDistDialog(String.Format("An error has occurred: {0}", ex.Message));
                        return;
                    }
                }
                // Sales Rep. ID already exists in Database, please enter a new Sales Rep. ID.
                else
                {
                    errorMsg = "Sales Rep. # already exists in database, please enter a new Sales Rep. #.";
                    VisibleErrors_SalesRepDistDialog(errorMsg);
                    SetFocus(txtSalesRepDistID);
                    return;
                }
                break;
            case "EDIT":
                Int32.TryParse(ddlCommissionSetupID.SelectedValue, out commissionSetupID);
                try
                {
                    srd = (from s in idc.SalesRepDistributors
                           where s.SalesRepDistID == salesRepDistID
                           select s).FirstOrDefault();

                    cs = (from c in idc.CommissionSetups
                          where c.CommissionSetupID == commissionSetupID
                          select c).FirstOrDefault();

                    // Update information to SaleRepDistributors_New table in DB.
                    if (strContactName != null) { srd.ContactName = strContactName.Trim().Replace("'", "''"); }
                    // Company Name is REQUIRED, cannot be NULL.
                    srd.CompanyName = strCompanyName.Trim().Replace("'", "''");
                    if (strPrefix != null) { srd.Prefix = strPrefix.Trim(); }
                    if (strFirstName != null) { srd.FirstName = strFirstName.Trim().Replace("'", "''"); }
                    if (strMiddleName != null) { srd.MiddleName = strMiddleName.Trim().Replace("'", "''"); }
                    if (strLastName != null) { srd.LastName = strLastName.Trim().Replace("'", "''"); }
                    if (strAddress1 != null) { srd.Address1 = strAddress1.Trim().Replace("'", "''"); }
                    if (strAddress2 != null) { srd.Address2 = strAddress2.Trim().Replace("'", "''"); }
                    if (strCity != null) { srd.City = strCity.Trim().Replace("'", "''"); }
                    if (intStateID != null) { srd.StateID = intStateID; }
                    if (strZipCode != null) { srd.PostalCode = strZipCode.Trim(); }
                    if (intCountryID != null) { srd.CountryID = intCountryID; }
                    if (strEmail != null) { srd.Email = strEmail.Trim().Replace("'", "''"); }
                    if (strPhoneNo != null) { srd.Telephone = strPhoneNo.Trim(); }
                    if (strFaxNo != null) { srd.Fax = strFaxNo.Trim(); }
                    if (strCustomerServiceNo != null) { srd.CustServiceTelephone = strCustomerServiceNo.Trim(); }
                    // Notification Preference is REQUIRED, cannot be NULL.
                    srd.NotificationPref = strNotificationPreference.Trim();
                    // Channel ID is REQUIRED, cannot be NULL.
                    srd.ChannelID = channelID;

                    // Update information to CommissionSetup table in DB.
                    if (strSalesManagerID != null) { cs.SalesManagerID = strSalesManagerID.Trim(); }
                    if (decCommissionPercentage != null) { cs.CommissionPct = Convert.ToDecimal(txtCommissionPercentage.Text); }
                    // Sales Manager Commission % is REQUIRED, cannot be NULL.
                    cs.SalesMgrCommissionPct = Convert.ToDecimal(txtSalesMgrCommissionPercentage.Text);
                    // Renewal Commission % is REQUIRED, cannot be NULL.
                    cs.RenewalCommissionPct = Convert.ToDecimal(txtRenewalCommissionPercentage.Text);
                    // PO Required Yes/No is REQUIRED, cannot be NULL.
                    cs.PORequired = boolPORequired;
                    // Contract Start Date is REQUIRED, cannot be NULL.
                    cs.ContractStartDate = Convert.ToDateTime(txtContractStartDate.Text);
                    // Contract End Date is REQUIRED, cannot be NULL.
                    cs.ContractEndDate = Convert.ToDateTime(txtContractEndDate.Text);
                    if (intRateID != null) { cs.RateID = intRateID; }
                    else cs.RateID = null;
                    if (strOldQuantumCode != null) { cs.OldQuantumCode = strOldQuantumCode.Trim(); }
                    if (boolCodeOfEthics == true) { cs.IsCodeOfEthic = true; }
                    else { cs.IsCodeOfEthic = false; }
                    if (dtContractSignedDate != null) { cs.ContractSignedDate = dtContractSignedDate; }
                    // Reseller Price DEFAULT VALUE is set to 0.00.
                    cs.ResellerPrice = decResellerPrice;
                    if (strResellerPriceCurrencyCode != null) { cs.ResellerPriceCurrCode = strResellerPriceCurrencyCode.Trim(); }
                    // Number of Trifolds DEFAULT VALUE is set to 0.
                    cs.TrifoldQty = intNumberOfTrifolds;
                    if (boolSpecialPriceOfTrifold == true) { cs.IsSpecialPriceTrifold = true; }
                    else { cs.IsSpecialPriceTrifold = false; }
                    cs.AgreementType = strAgreementType;
                    if (dtShipDate != null) { cs.ShipDate = dtShipDate; }
                    if (dtDataSheetSentDate != null) { cs.DataSheetSentDate = dtDataSheetSentDate; }
                    if (strNotes != null) { cs.Notes = strNotes.Trim().Replace("'", "''"); }
                    // Commission Renewal Years is REQUIRED, cannot be NULL.
                    cs.CommRenewalYears = Convert.ToInt32(txtCommissionRenewalYears.Text);
                    cs.CommissionEvent = COMMISSIONEVENT;

                    idc.SubmitChanges();

                    // Refresh GridView.
                    gvSalesRepDistributor.DataBind();

                    ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divSalesRepDistributorDialog')", true);
                }
                catch (Exception ex)
                {
                    // Display the system-generated error message.
                    VisibleErrors_SalesRepDistDialog(String.Format("An error has occurred: {0}", ex.Message));
                    return;
                }
                break;
            default:
                break;
        }
    }

    // Deactivates Sales Rep. Distributor's record in the Database.
    // For audit and review purposes, this functionality does not delete the record, but simply sets its Active value to FALSE.
    protected void btnDeactivateSaleRepDistributorRecord_Click(object sender, EventArgs e)
    {
        salesRepDistID = hdnfldSalesRepDistID.Value;
        string strStatus = btnDeactivateSaleRepDistributorRecord.Text;
        userName = GetUserName();

        try
        {
            srd = (from s in idc.SalesRepDistributors
                   where s.SalesRepDistID == salesRepDistID
                   select s).FirstOrDefault();

            var commissionsetup = (from c in idc.CommissionSetups
                                   where c.SalesRepDistID == salesRepDistID
                                   select c);

            if (strStatus == "Deactivate Record")
            {
                // Deactivate record in SaleRepDistributors_New table in DB.
                srd.Active = false;
                srd.DeactivateDate = currentDate;
                srd.DeactivatedBy = userName;

                // Deactivate record(s) in CommissionSetup table in DB.
                if (commissionsetup.Count() > 0)
                {
                    foreach (CommissionSetup x in commissionsetup)
                    {
                        x.Active = false;
                        x.DeactivateDate = currentDate;
                        x.DeactivatedBy = userName;
                    }
                }
            }
            else if (strStatus == "Activate Record")
            {
                // Activate record in SaleRepDistributors_New table in DB.
                // Note: If Sales Rep./Dist. Account is being ReActivated, only the account will be set to Active.
                //       The User must then ReActivate each Commission Setup individually.
                srd.Active = true;
                srd.DeactivateDate = currentDate;
                srd.DeactivatedBy = userName;
            }

            idc.SubmitChanges();

            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divSalesRepDistributorDialog')", true);

            // Refresh GridView
            gvSalesRepDistributor.DataBind();
        }
        catch (Exception ex)
        {
            // Display the system-generated error message.
            VisibleErrors_SalesRepDistDialog(String.Format("An error has occurred: {0}", ex.Message));
        }
    }

    // -------- BEGIN :: COMMISSION DETAILS (ADD) MODAL/DIALOG -------- //
    // Get ChannelID from ddlChannelTypes.SelectedValue and perform the following logic:
    //  1.) If Distributor set Commission % control to Editable, but must have value between 0 to 40.
    //  2.) If Reseller set Commission % to 0 and make control Read-Only (set focus on txtRenewalCommissionPercentage_New).
    protected void SetCommissionPercentageControl(int channelid)
    {
        if (channelid == 7)
        {
            lblCommissionPercentageRequired_New.Visible = true;
            txtCommissionPercentage_New.ReadOnly = false;
            txtCommissionPercentage_New.Text = "";
            txtCommissionPercentage_New.Focus();
        }
        else
        {
            lblCommissionPercentageRequired_New.Visible = false;
            txtCommissionPercentage_New.Text = "0";
            txtCommissionPercentage_New.ReadOnly = true;
            txtRenewalCommissionPercentage_New.Text = "0";
            txtRenewalCommissionPercentage_New.ReadOnly = true;
            txtSalesMgrCommissionPercentage_New.Text = "0";
            txtSalesMgrCommissionPercentage_New.ReadOnly = true;
            txtCommissionRenewalYears_New.Text = "0";
            txtCommissionRenewalYears_New.ReadOnly = true;
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "preventBackspace()", true);
        }
    }

    protected void btnAddCommissionDetails_Click(object sender, EventArgs e)
    {
        SetDefaultValues_AddCommissionDetailsDialog();
        lblSalesRepDistID.Text = hdnfldSalesRepDistID.Value;
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('divCommissionDetailsDialog')", true);

        int channelID = 0;
        Int32.TryParse(ddlChannelTypes.SelectedValue, out channelID);

        SetCommissionPercentageControl(channelID);
    }

    private void InvisibleErrors_AddCommissionDetailsDialog()
    {
        // Reset submission Form Error Message.
        addCommissionDetailsErrorMsg.InnerText = "";
        addCommissionDetailsDialogErrors.Visible = false;
    }

    private void VisibleErrors_AddCommissionDetailsDialog(string errorMsg)
    {
        addCommissionDetailsErrorMsg.InnerText = errorMsg;
        addCommissionDetailsDialogErrors.Visible = true;
    }

    private void SetDefaultValues_AddCommissionDetailsDialog()
    {
        InvisibleErrors_AddCommissionDetailsDialog();
        hdnfldAccordionIndex_New.Value = "0";
        // Set (default) values to Commission Details (% and Years controls).
        txtCommissionPercentage_New.Text = "";
        txtRenewalCommissionPercentage_New.Text = "0";
        txtSalesMgrCommissionPercentage_New.Text = "0";
        txtCommissionRenewalYears_New.Text = "3";
        txtContractSignDate_New.Text = String.Format("{0:MM/dd/yyyy}", currentDate);
        txtContractSignDate_New.Attributes.Add("readonly", "readonly");
        txtContractStartDate_New.Text = String.Format("{0:MM/dd/yyyy}", currentDate);
        txtContractStartDate_New.Attributes.Add("readonly", "readonly");
        txtContractEndDate_New.Text = String.Format("{0:MM/dd/yyyy}", currentDate.AddYears(5));
        txtContractEndDate_New.Attributes.Add("readonly", "readonly");
        chkbxPORequired_New.Checked = false;
        divCommissionDetailsButtons.Visible = true;
        txtOldQuantumCode_New.Text = "";
        ddlAgreementType_New.SelectedValue = "0";
        txtResellerPrice_New.Text = "";
        ddlResellerPriceCurrencyCode_New.SelectedValue = "USD";
        txtNumberOfTrifolds_New.Text = "";
        chkbxCodeOfEthics_New.Checked = false;
        txtShipDate_New.Text = "";
        txtShipDate_New.Attributes.Add("readonly", "readonly");
        txtDataSheetSentDate_New.Text = "";
        txtDataSheetSentDate_New.Attributes.Add("readonly", "readonly");
        chkbxSpecialPricesOfTrifolds_New.Checked = false;
        txtNotes_New.Text = "";
    }

    // -------- BEGIN :: Controls Validation -------- //
    private bool PassesInputValidation_AddCommissionDetailsDialog()
    {
        string errorMsg = "";
        decimal localDecimal = 0;
        int localInteger = 0;
        DateTime localStartDate;
        DateTime localEndDate;
        DateTime localSignDate;
        Int32.TryParse(ddlChannelTypes.SelectedValue, out channelID);

        // Commission %.
        if (txtCommissionPercentage_New.Text.Trim().Length != 0)
        {
            decimal.TryParse(txtCommissionPercentage_New.Text, out localDecimal);

            if (localDecimal > 40)
            {
                errorMsg = "Commission % cannot be greater than 40.";
                this.VisibleErrors_AddCommissionDetailsDialog(errorMsg);
                SetFocus(this.txtCommissionPercentage_New);
                return false;
            }
        }

        // Commission % is mandatory, but free form (% should be between 0-40).
        if (txtCommissionPercentage_New.Text.Trim().Length == 0)
        {
            errorMsg = "Please enter a Commission % between 0 to 40.";
            VisibleErrors_SalesRepDistDialog(errorMsg);
            SetFocus(txtCommissionPercentage_New);
            return false;
        }

        // Renewal Commission %.
        if (this.txtRenewalCommissionPercentage_New.Text.Trim().Length == 0 || decimal.TryParse(this.txtRenewalCommissionPercentage_New.Text, out localDecimal) == false)
        {
            errorMsg = "Renewal Commission % is required and must be a number.";
            this.VisibleErrors_AddCommissionDetailsDialog(errorMsg);
            SetFocus(this.txtRenewalCommissionPercentage_New);
            return false;
        }
        else if (decimal.TryParse(this.txtRenewalCommissionPercentage_New.Text, out localDecimal))
        {
            if (localDecimal < 0 || localDecimal > 10)
            {
                errorMsg = "Renewal Commission % cannot be less than 0 or greater than 10.";
                this.VisibleErrors_AddCommissionDetailsDialog(errorMsg);
                SetFocus(this.txtRenewalCommissionPercentage_New);
                return false;
            }
        }

        // Sales Manager Commission %.
        if (this.txtSalesMgrCommissionPercentage_New.Text.Trim().Length == 0 || decimal.TryParse(this.txtSalesMgrCommissionPercentage_New.Text, out localDecimal) == false)
        {
            errorMsg = "Sales Manager Commission % is required and must be a number.";
            this.VisibleErrors_AddCommissionDetailsDialog(errorMsg);
            SetFocus(this.txtSalesMgrCommissionPercentage_New);
            return false;
        }
        else if (decimal.TryParse(this.txtSalesMgrCommissionPercentage_New.Text, out localDecimal))
        {
            if (localDecimal < 0 || localDecimal > 10)
            {
                errorMsg = "Sales Manager Commission % cannot be less than 0.";
                this.VisibleErrors_AddCommissionDetailsDialog(errorMsg);
                SetFocus(this.txtSalesMgrCommissionPercentage_New);
                return false;
            }
        }

        // Renewal Commission Years.
        if (this.txtCommissionRenewalYears_New.Text.Trim().Length == 0 || int.TryParse(this.txtCommissionRenewalYears_New.Text, out localInteger) == false)
        {
            errorMsg = "Renewal Commission Years is required and must be a number.";
            this.VisibleErrors_AddCommissionDetailsDialog(errorMsg);
            SetFocus(this.txtCommissionRenewalYears_New);
            return false;
        }
        else if (int.TryParse(this.txtCommissionRenewalYears_New.Text, out localInteger))
        {
            if (localInteger < 0)
            {
                errorMsg = "Renewal Commission Years cannot be less than 0.";
                this.VisibleErrors_AddCommissionDetailsDialog(errorMsg);
                SetFocus(this.txtCommissionRenewalYears_New);
                return false;
            }
        }

        // Contract Sign Date is required.
        if (txtContractSignDate_New.Text.Trim().Length == 0)
        {
            errorMsg = "Contract Sign Date must be entered.";
            this.VisibleErrors_AddCommissionDetailsDialog(errorMsg);
            SetFocus(this.txtContractSignDate_New);
            return false;
        }

        // Contract Start Date is required.
        if (txtContractStartDate_New.Text.Trim().Length == 0)
        {
            errorMsg = "Contract Start Date must be entered.";
            this.VisibleErrors_AddCommissionDetailsDialog(errorMsg);
            SetFocus(this.txtContractStartDate_New);
            return false;
        }

        // Contract End Date is required.
        if (txtContractEndDate_New.Text.Trim().Length == 0)
        {
            errorMsg = "Contract End Date must be entered.";
            this.VisibleErrors_AddCommissionDetailsDialog(errorMsg);
            SetFocus(this.txtContractEndDate_New);
            return false;
        }

        // Check to make sure that the Contract Start Date is less than the Contract End Date.
        // The Contract End Date is editable, but is defaulted to +5 years from the Contract Start Date.
        if (txtContractStartDate_New.Text.Trim().Length != 0 && txtContractEndDate_New.Text.Trim().Length != 0)
        {
            DateTime.TryParse(txtContractStartDate_New.Text, out localStartDate);
            DateTime.TryParse(txtContractEndDate_New.Text, out localEndDate);

            if (localStartDate > localEndDate)
            {
                errorMsg = "Contract Start Date must be before Contract End Date.";
                this.VisibleErrors_AddCommissionDetailsDialog(errorMsg);
                SetFocus(txtContractStartDate_New);
                return false;
            }
        }

        // Check to make sure that the Contract Sign Date is less than/equal to the Contract Start Date.
        // The Contract Start Date is defaulted to the Contract Start Date.
        if (txtContractStartDate_New.Text.Trim().Length != 0 && txtContractSignDate_New.Text.Trim().Length != 0)
        {
            DateTime.TryParse(txtContractStartDate_New.Text, out localStartDate);
            DateTime.TryParse(txtContractSignDate_New.Text, out localSignDate);

            if (localSignDate > localStartDate)
            {
                errorMsg = "Contract Sign Date must be on or before the Contract Start Date.";
                this.VisibleErrors_AddCommissionDetailsDialog(errorMsg);
                SetFocus(txtContractSignDate_New);
                return false;
            }
        }

        // FORMATING of Old Quantum Code.
        if (this.txtOldQuantumCode_New.Text.Trim().Length > 4)
        {
            errorMsg = "Old Quantum Code must be 4 characters or less.";
            this.VisibleErrors_AddCommissionDetailsDialog(errorMsg);
            SetFocus(this.txtOldQuantumCode_New);
            return false;
        }

        // FORMATING of Reseller Price.
        if (this.txtResellerPrice_New.Text.Trim().Length > 0 && decimal.TryParse(this.txtResellerPrice_New.Text, out localDecimal) == false)
        {
            errorMsg = "Reseller Price must be a number.";
            this.VisibleErrors_AddCommissionDetailsDialog(errorMsg);
            SetFocus(this.txtResellerPrice_New);
            return false;
        }
        else if (decimal.TryParse(this.txtResellerPrice_New.Text, out localDecimal))
        {
            if (localDecimal < 0)
            {
                errorMsg = "Reseller Price cannot be less than 0.";
                this.VisibleErrors_AddCommissionDetailsDialog(errorMsg);
                SetFocus(this.txtResellerPrice_New);
                return false;
            }
        }

        // FORMATING if # of Trifolds.
        if (this.txtNumberOfTrifolds_New.Text.Trim().Length > 0 && int.TryParse(this.txtNumberOfTrifolds_New.Text, out localInteger) == false)
        {
            errorMsg = "# of Trifolds must be a number.";
            this.VisibleErrors_AddCommissionDetailsDialog(errorMsg);
            SetFocus(this.txtNumberOfTrifolds_New);
            return false;
        }
        else if (int.TryParse(this.txtNumberOfTrifolds_New.Text, out localInteger))
        {
            if (localInteger < 0)
            {
                errorMsg = "# of Trifolds cannot be less than 0.";
                this.VisibleErrors_AddCommissionDetailsDialog(errorMsg);
                SetFocus(this.txtNumberOfTrifolds_New);
                return false;
            }
        }

        return true;
    }
    // -------- END :: Control & Value Validation FUNCTIONS Here for Add Commission Details & Distributor Information Modal (only) -------- //

    protected void btnAddCommissionDetail_New_Click(object sender, EventArgs e)
    {
        string _salesrepdistid = lblSalesRepDistID.Text;
        InvisibleErrors_SalesRepDistDialog();

        if (PassesInputValidation_AddCommissionDetailsDialog())
        {
            string strSalesRepDistID = _salesrepdistid.ToUpper();
            string strSalesManagerID = (this.ddlSalesManagerID.SelectedValue == "0" ? null : this.ddlSalesManagerID.SelectedValue);
            // Assign Commission Details Controls to variables.
            string strCommissionEvent = COMMISSIONEVENT;
            decimal decCommissionPercentage;
            decimal? result_cp = null;
            if (decimal.TryParse(txtCommissionPercentage_New.Text, out decCommissionPercentage))
            {
                result_cp = decCommissionPercentage;
            }
            else
            {
                result_cp = null;
            }
            decimal decRenewalCommissionPercentage = (this.txtRenewalCommissionPercentage_New.Text.Trim().Length == 0 ? 0 : Convert.ToDecimal(this.txtRenewalCommissionPercentage_New.Text.Trim()));
            decimal decSalesMgrCommissionPercentage = (this.txtSalesMgrCommissionPercentage_New.Text.Trim().Length == 0 ? 0 : Convert.ToDecimal(this.txtSalesMgrCommissionPercentage_New.Text.Trim()));
            int intCommissionRenewalYears = (this.txtCommissionRenewalYears_New.Text.Trim().Length == 0 ? 0 : Convert.ToInt32(this.txtCommissionRenewalYears_New.Text.Trim()));
            DateTime dtContractStartDate = Convert.ToDateTime(this.txtContractStartDate_New.Text.Trim());
            DateTime dtContractEndDate = Convert.ToDateTime(this.txtContractEndDate_New.Text.Trim());
            bool boolPORequired = (this.chkbxPORequired_New.Checked == true ? true : false);
            string strOldQuantumCode = (this.txtOldQuantumCode_New.Text.Trim().Length == 0 ? null : this.txtOldQuantumCode_New.Text);
            string strAgreementType = (this.ddlAgreementType_New.SelectedValue == "0" ? null : this.ddlAgreementType_New.SelectedValue);
            decimal decResellerPrice;
            decimal? result_rp = null;
            if (decimal.TryParse(txtResellerPrice_New.Text, out decResellerPrice))
            {
                result_rp = decResellerPrice;
            }
            else
            {
                result_rp = null;
            }
            string strResellerPriceCurrencyCode = (this.ddlResellerPriceCurrencyCode_New.SelectedValue == "0" ? null : this.ddlResellerPriceCurrencyCode_New.SelectedValue);
            DateTime? dtContractSignedDate;
            if (this.txtContractSignDate_New.Text.Trim().Length != 0)
            {
                dtContractSignedDate = Convert.ToDateTime(this.txtContractSignDate_New.Text.Trim());
            }
            else
            {
                dtContractSignedDate = null;
            }
            int intNumberOfTrifolds = (this.txtNumberOfTrifolds_New.Text.Trim().Length == 0 ? Convert.ToInt32("0") : Convert.ToInt32(txtNumberOfTrifolds_New.Text));
            bool boolCodeOfEthics = (this.chkbxCodeOfEthics_New.Checked == true ? true : false);
            DateTime? dtShipDate;
            if (this.txtShipDate_New.Text.Trim().Length != 0)
            {
                dtShipDate = Convert.ToDateTime(txtShipDate_New.Text);
            }
            else
            {
                dtShipDate = null;
            }
            DateTime? dtDataSheetSentDate;
            if (this.txtDataSheetSentDate_New.Text.Trim().Length != 0)
            {
                dtDataSheetSentDate = Convert.ToDateTime(txtDataSheetSentDate_New.Text);
            }
            else
            {
                dtDataSheetSentDate = null;
            }
            bool boolSpecialPriceOfTrifold = (this.chkbxSpecialPricesOfTrifolds_New.Checked == true ? true : false);
            string strNotes = (this.txtNotes_New.Text.Trim().Length == 0 ? null : this.txtNotes_New.Text.Replace("'", "''"));

            try
            {
                // Assign information to CommissionSetup table in DB.
                CommissionSetup cs = new CommissionSetup
                {
                    SalesRepDistID = strSalesRepDistID,
                    SalesManagerID = strSalesManagerID,
                    CommissionPct = decCommissionPercentage,
                    SalesMgrCommissionPct = decSalesMgrCommissionPercentage,
                    RenewalCommissionPct = decRenewalCommissionPercentage,
                    PORequired = boolPORequired,
                    ContractStartDate = dtContractStartDate,
                    ContractEndDate = dtContractEndDate,
                    OldQuantumCode = strOldQuantumCode,
                    AgreementType = strAgreementType,
                    IsCodeOfEthic = boolCodeOfEthics,
                    ContractSignedDate = dtContractSignedDate,
                    ResellerPrice = decResellerPrice,
                    ResellerPriceCurrCode = strResellerPriceCurrencyCode,
                    TrifoldQty = intNumberOfTrifolds,
                    IsSpecialPriceTrifold = boolSpecialPriceOfTrifold,
                    ShipDate = dtShipDate,
                    DataSheetSentDate = dtDataSheetSentDate,
                    Notes = strNotes,
                    CommRenewalYears = intCommissionRenewalYears,
                    CommissionEvent = strCommissionEvent,
                    Active = true
                };

                if (Page.IsValid)
                {
                    idc.CommissionSetups.InsertOnSubmit(cs);
                    idc.SubmitChanges();

                    // Refresh CommissionSetupID DDL.
                    ddlCommissionSetupID.DataBind();

                    // Refresh GridView
                    gvSalesRepDistributor.DataBind();

                    ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divCommissionDetailsDialog')", true);
                    ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "window.opener.location.reload(true)", true);
                }
            }
            catch (Exception ex)
            {
                // Display the system-generated error message.
                this.VisibleErrors_SalesRepDistDialog(String.Format("An error has occurred: {0}", ex.Message));
            }
        }
    }

    protected void btnCancel_New_Click(object sender, EventArgs e)
    {
        SetDefaultValues_AddCommissionDetailsDialog();
    }

    // Send E-Mail as soon as a NEW Sales Rep. Distributor is added.
    private void sendEmailAddNotice(string salesrepdistid, string firstname, string lastname)
    {
        string errorMsg = "";
        if (firstname == null)
        {
            firstname = "None Given.";
        }

        if (lastname == null)
        {
            lastname = "None Given.";
        }

        // HTML Formatting.
        string htmlBodyText = "<html><head><title>New Sales Rep. Distributor</title></head><body>";
        htmlBodyText += "<table border='1' width='450' cellpadding='5' cellspacing='0'>";
        // Remove from HERE...
        htmlBodyText += "<tr>";
        htmlBodyText += "<td align='center' width='200' bgcolor='#EEEEEE' colspan='2'><strong>NEW SALES REP./DIST. INFORMATION</strong></td>";
        htmlBodyText += "</tr>";
        // ...to HERE.
        htmlBodyText += "<tr>";
        htmlBodyText += "<td align='left' width='200' bgcolor='#EEEEEE'>Added On:</td>";
        htmlBodyText += "<td align='right' width='250'><strong>" + String.Format("{0}", DateTime.Now.ToShortDateString()) + "</strong></td>";
        htmlBodyText += "</tr>";
        htmlBodyText += "<tr>";
        htmlBodyText += "<td align='left' width='200' bgcolor='#EEEEEE'>Sales Rep./Dist. ID:</td>";
        htmlBodyText += "<td align='right' width='250'><strong>" + salesrepdistid + "</strong></td>";
        htmlBodyText += "</tr>";
        htmlBodyText += "<tr>";
        htmlBodyText += "<td align='left' width='200' bgcolor='#EEEEEE'>First Name:</td>";
        htmlBodyText += "<td align='right' width='250'><strong>" + firstname + "</strong></td>";
        htmlBodyText += "</tr>";
        htmlBodyText += "<tr>";
        htmlBodyText += "<td align='left' width='200' bgcolor='#EEEEEE'>Last Name:</td>";
        htmlBodyText += "<td align='right' width='250'><strong>" + lastname + "</strong></td>";
        htmlBodyText += "</tr>";
        htmlBodyText += "</body></html>";

        // SEND the E-mail.
        string smtpServer = string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["SmtpServer"]) ? "10.200.2.16" : System.Configuration.ConfigurationManager.AppSettings["SmtpServer"];
        SmtpClient client = new SmtpClient(smtpServer);
        MailMessage mail = new MailMessage();
        mail.IsBodyHtml = true;

        mail.From = new MailAddress("noreply@instadose.com");
        mail.Subject = "New Sales Rep./Distributor Added.";
        mail.Body = htmlBodyText;

        // E-mail Recipients To and CC.
        mail.To.Add("cyurosko@mirion.com");
        // For testing, Anuradha Nandi will receive CC e-mail.
        // This will be changed once in Production.
        //mail.CC.Add("anandi@mirion.com");

        try
        {
            client.Send(mail);
        }
        catch (Exception ex)
        {
            // Display the system-generated error message.
            VisibleErrors_SalesRepDistDialog(String.Format("An error has occurred: {0}", ex.Message));
            return;
        }
    }
}
