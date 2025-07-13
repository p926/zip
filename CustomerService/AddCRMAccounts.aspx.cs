using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Instadose.Data;
using Newtonsoft.Json;

public partial class CustomerService_AddCRMAccounts : System.Web.UI.Page
{
    // Create the database reference
    InsDataContext idc = new InsDataContext();

    // String to hold the current Username (logged-in).
    string UserName = "Unknown";

    /// <summary>
    /// Limits access to the functionality based on Active Directory Groups.
    /// </summary>
    string[] ActiveDirecoryGroups = { "IRV-IT", "IRV-CustomerSvc", "IRV-Client Services" };

    protected void Page_Load(object sender, EventArgs e)
    {
        // Try to set the Username based on who is logged-in.
        try { this.UserName = User.Identity.Name.Split('\\')[1]; }
        catch { this.UserName = "Unknown"; }

        bool belongsToGroups = ActiveDirectoryQueries.UserExistsInAnyGroup(User.Identity.Name, ActiveDirecoryGroups);

        if (belongsToGroups)
        {
            DisplayCreditCardImages();
            TogglePONumberCreditCardControls();

            rdobtnPONumber.Attributes.Add("value", "2");
            rdobtnCreditCard.Attributes.Add("value", "1");

            if (Page.IsPostBack) return;

            PopulateReferralDDL();
            PopulateCustomerTypeDDL();
            PopulateUNIXCustomerTypesDDL();
            PopulateCreditCardExpirationYear();
            PopulateSecurityQuestionsDDL();
            PopulateDealersDDL();
            PopulateIndustryTypesDDL();

            this.hdnfldAddAccordionIndex.Value = "0";
            this.hdnfldEditAccordionIndex.Value = "0";

            PopulateAddLocationCountryDDLs();
            int countryIDAddBilling;
            int countryIDAddShipping;
            int.TryParse(this.ddlAddBillingCountry.SelectedValue, out countryIDAddBilling);
            int.TryParse(this.ddlAddShippingCountry.SelectedValue, out countryIDAddShipping);
            PopulateAddLocationBillingStateDDL(countryIDAddBilling);
            PopulateAddLocationShippingStateDDL(countryIDAddShipping);

            PopulateEditLocationCountryDDLs();
            int countryIDEditBilling;
            int countryIDEditShipping;
            int.TryParse(this.ddlEditBillingCountry.SelectedValue, out countryIDEditBilling);
            int.TryParse(this.ddlEditShippingCountry.SelectedValue, out countryIDEditShipping);
            PopulateEditLocationBillingStateDDL(countryIDEditBilling);
            PopulateEditLocationShippingStateDDL(countryIDEditShipping);

            int addProductGroupID = 0;
            PopulateAddOrderProductGroupIDDDL();
            int.TryParse(this.ddlAddProductGroupID.SelectedValue, out addProductGroupID);
            PopulateAddOrderProductIDDDL(addProductGroupID);

            int editProductGroupID = 0;
            PopulateEditOrderProductGroupIDDDL();
            int.TryParse(this.ddlEditProductGroupID.SelectedValue, out editProductGroupID);
            PopulateEditOrderProductIDDDL(editProductGroupID);
        }
        else
        {
            Response.Redirect("~/Default.aspx");
        }
    }

    /// <summary>
    /// Check to see if the Username entered is not already in use.
    /// </summary>
    /// <param name="username"></param>
    /// <returns></returns>
    [System.Web.Services.WebMethod]
    public static string CheckUserName(string username)
    {
        string value = string.Empty;

        try
        {
            // Create the database reference
            InsDataContext idc = new InsDataContext();

            int count = (from au in idc.AuthUsers
                         where au.UserName == username
                         && au.Active == true
                         select au.UserName).Count();

            if (count > 0) value = "false";
            else value = "true";
        }
        catch (Exception ex)
        {
            value = "error";
        }

        return value;
    }

    [System.Web.Services.WebMethod]
    public static void ClientSidePopulationBillingStateDDL(DropDownList bcddl, DropDownList bsddl)
    {
        int countryID = 0;
        int.TryParse(bcddl.SelectedValue, out countryID);

        // Create the database reference
        InsDataContext idc = new InsDataContext();

        var states = (from s in idc.States
                      where s.CountryID == countryID
                      select new
                      {
                          StateID = s.StateID,
                          StateName = s.StateName
                      });

        bsddl.DataSource = states;
        bsddl.DataTextField = "StateName";
        bsddl.DataValueField = "StateID";
        bsddl.DataBind();
    }

    #region Referral DDL
    /// <summary>
    /// Populate Referral DDL with SalesRepDistributors information.
    /// </summary>
    protected void PopulateReferralDDL()
    {
        this.ddlReferral.Items.Clear();

        var referrals = (from srd in idc.SalesRepDistributors
                         where srd.Active == true
                         orderby srd.SalesRepDistID
                         select new
                         {
                             SalesRepDistID = srd.SalesRepDistID,
                             SalesCompanyName = srd.SalesRepDistID.ToString() + " - " + srd.CompanyName.Trim()
                         }).Distinct();

        this.ddlReferral.DataSource = referrals;
        this.ddlReferral.DataTextField = "SalesCompanyName";
        this.ddlReferral.DataValueField = "SalesRepDistID";
        this.ddlReferral.DataBind();

        ListItem firstItem = new ListItem("-Select Referral-", "0");
        this.ddlReferral.Items.Insert(0, firstItem);
    }
    #endregion

    #region Customer Type DDL
    /// <summary>
    /// Populate Customer Types DDL with CustomerTypes information.
    /// </summary>
    protected void PopulateCustomerTypeDDL()
    {
        this.ddlCustomerType.Items.Clear();

        var customertypes = (from ct in idc.CustomerTypes
                             where ct.Active == true
                             orderby ct.CustomerTypeDesc
                             select new
                             {
                                CustomerTypeID = ct.CustomerTypeID,
                                CustomerTypeCode = ct.CustomerTypeCode
                             }).Distinct();

        this.ddlCustomerType.DataSource = customertypes;
        this.ddlCustomerType.DataTextField = "CustomerTypeCode";
        this.ddlCustomerType.DataValueField = "CustomerTypeID";
        this.ddlCustomerType.DataBind();

        ListItem firstItem = new ListItem("-Select Customer Type-", "0");
        this.ddlCustomerType.Items.Insert(0, firstItem);
    }
    #endregion

    #region UNIX Customer Type DDL
    /// <summary>
    /// Populate UNIX Customer Types DDL with UnixCustomerTypes information.
    /// </summary>
    protected void PopulateUNIXCustomerTypesDDL()
    {
        this.ddlUnixCustomerType.Items.Clear();

        var unixcustomertypes = (from u in idc.UnixCustomerTypes
                                 where u.Active == true
                                 orderby u.UnixCustomerDescription
                                 select new 
                                 { 
                                    UnixCustomerTypeID = u.UnixCustomerTypeID,
                                    UnixCustomerDescription = u.UnixCustomerDescription
                                 }).Distinct();

        this.ddlUnixCustomerType.DataSource = unixcustomertypes;
        this.ddlUnixCustomerType.DataTextField = "UnixCustomerDescription";
        this.ddlUnixCustomerType.DataValueField = "UnixCustomerTypeID";
        this.ddlUnixCustomerType.DataBind();

        ListItem firstItem = new ListItem("-Select UNIX Customer Type-", "0");
        this.ddlUnixCustomerType.Items.Insert(0, firstItem);
    }
    #endregion

    #region Set Service Start Date to 1st of Month for ICCare.
    /// <summary>
    /// IF BrandSourceID = 3 (ICCare), then Service Start Date will be set to 1st of the Month.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void txtServiceStartDate_TextChanged(object sender, EventArgs e)
    {
        //IC Care Service Start Date MUST BE on the 1st of the month.
        if (this.ddlBrandSource.SelectedValue == "3")
            txtServiceStartDate.Text = string.Format("{0:MM/01/yyyy}", DateTime.Parse(this.txtServiceStartDate.Text));
    }
    #endregion

    #region PO Number/Credit Card Information Controls (toggle).
    /// <summary>
    /// Toggles between displaying PO Number or Credit Card Information controls.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void TogglePONumberCreditCardControls()
    {
        if (rdobtnPONumber.Checked == true)
        {
            divPONumber.Visible = true;
            divCreditCardInformation.Visible = false;
        }

        if (rdobtnCreditCard.Checked == true)
        {
            divPONumber.Visible = false;
            divCreditCardInformation.Visible = true;
        }
    }
    #endregion

    #region Credit Card Images
    /// <summary>
    /// Displays Credit Card Logo/Images based on CC Type.
    /// </summary>
    public void DisplayCreditCardImages()
    {
        rdobtnlstCreditCardType.Items[0].Text = "<img src='../images/ccvisa.gif' alt='Visa' width='30'>";
        rdobtnlstCreditCardType.Items[1].Text = "<img src='../images/ccmastercard.gif' alt='MasterCard' width='30'>";
        rdobtnlstCreditCardType.Items[2].Text = "<img src='../images/ccdiscover.gif' alt='Discover' width='30'>";
        rdobtnlstCreditCardType.Items[3].Text = "<img src='../images/ccamex.gif' alt='American Express' width='30'>";
    }
    #endregion

    #region Credit Card Expiration Year DDL
    /// <summary>
    /// Populate Credit Card Expiration Year with current year to + 6 out.
    /// </summary>
    protected void PopulateCreditCardExpirationYear()
    {
        ListItem firstItem = new ListItem("-Year-", "0");
        this.ddlCCExpirationYear.Items.Add(firstItem);
        int i;
        for (i = 0; i < 7; i++)
        {
            ListItem listItemYear = new ListItem((DateTime.Now.Year + i).ToString(), (DateTime.Now.Year + i).ToString());
            this.ddlCCExpirationYear.Items.Add(listItemYear);
        }
    }
    #endregion

    #region Sercurity Question
    /// <summary>
    /// Populate Security Question DDL with SecurityQuestions information.
    /// </summary>
    protected void PopulateSecurityQuestionsDDL()
    {
        this.ddlSecurityQuestion.Items.Clear();

        var securityquestions = (from sq in idc.SecurityQuestions
                                 where sq.Active == true
                                 orderby sq.SecurityQuestionID
                                 select new
                                 {
                                     SecurityQuestionID = sq.SecurityQuestionID,
                                     SecurityQuestionText = sq.SecurityQuestionText
                                 }).Distinct();

        this.ddlSecurityQuestion.DataSource = securityquestions;
        this.ddlSecurityQuestion.DataTextField = "SecurityQuestionText";
        this.ddlSecurityQuestion.DataValueField = "SecurityQuestionID";
        this.ddlSecurityQuestion.DataBind();

        ListItem firstItem = new ListItem("-Select Security Question-", "0");
        this.ddlSecurityQuestion.Items.Insert(0, firstItem);
    }
    #endregion

    #region Brand Source ID DDL.
    /// <summary>
    /// Based on Mirion or ICCare BrandSourceID, populate and/or change form controls.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void ddlBrandSource_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddlBrandSource.SelectedValue == "3")
        {
            this.divDealerInformation.Visible = true;
            this.ddlBillingFrequency.SelectedValue = "2";
            this.ddlBillingFrequency.Enabled = false;
            this.rdobtnPONumber.Checked = true;
            this.rdobtnCreditCard.Checked = false;
            this.rdobtnCreditCard.Enabled = false;
        }
        else
        {
            this.divDealerInformation.Visible = false;
            this.ddlBillingFrequency.SelectedValue = "0";
            this.ddlBillingFrequency.Enabled = true;
            this.rdobtnPONumber.Checked = true;
            this.rdobtnPONumber.Enabled = true;
            this.rdobtnCreditCard.Enabled = true;
        }
    }
    #endregion

    #region Dealer ID DDL.
    /// <summary>
    /// Populate Dealer DDL.
    /// </summary>
    protected void PopulateDealersDDL()
    {
        this.ddlDealer.Items.Clear();

        var dealers = (from d in idc.Dealers
                       where d.Active == true
                       orderby d.DealerName ascending
                       select new
                       {
                            DealerID = d.DealerID,
                            DealerName = d.DealerName
                       });

        this.ddlDealer.DataSource = dealers;
        this.ddlDealer.DataTextField = "DealerName";
        this.ddlDealer.DataValueField = "DealerID";
        this.ddlDealer.DataBind();

        ListItem firstItem = new ListItem("-Select Dealer-", "0");
        this.ddlDealer.Items.Insert(0, firstItem);
    }
    #endregion

    #region Industry Type (ID) DDL.
    /// <summary>
    /// Populate Industry Type DDL.
    /// </summary>
    protected void PopulateIndustryTypesDDL()
    {
        this.ddlIndustryType.Items.Clear();

        var industries = (from i in idc.Industries
                          orderby i.IndustryName ascending
                          select new
                          {
                                IndustryID = i.IndustryID,
                                IndustryName = i.IndustryName
                          });

        this.ddlIndustryType.DataSource = industries;
        this.ddlIndustryType.DataTextField = "IndustryName";
        this.ddlIndustryType.DataValueField = "IndustryID";
        this.ddlIndustryType.DataBind();

        ListItem firstItem = new ListItem("-Select Industry-", "0");
        this.ddlIndustryType.Items.Insert(0, firstItem);
    }
    #endregion 

    #region Add & Edit Location Information Modal/Dialog functions.
    /// <summary>
    /// Populate DDLs for Country for Add Location Information Modal/Dialog.
    /// </summary>
    private void PopulateAddLocationCountryDDLs()
    {
        var countries = (from c in idc.Countries
                         orderby c.CountryName
                         select new
                         {
                             c.CountryID,
                             c.CountryName
                         });

        // Add Billing Country DDL.
        this.ddlAddBillingCountry.DataSource = countries;
        this.ddlAddBillingCountry.DataTextField = "CountryName";
        this.ddlAddBillingCountry.DataValueField = "CountryID";
        this.ddlAddBillingCountry.DataBind();

        this.ddlAddBillingCountry.SelectedValue = "1"; // Default to USA.

        // Add Shipping Country DDL.
        this.ddlAddShippingCountry.DataSource = countries;
        this.ddlAddShippingCountry.DataTextField = "CountryName";
        this.ddlAddShippingCountry.DataValueField = "CountryID";
        this.ddlAddShippingCountry.DataBind();

        this.ddlAddShippingCountry.SelectedValue = "1"; // Default to USA.
    }

    /// <summary>
    /// Populate DDLs for Country for Edit Location Information Modal/Dialog.
    /// </summary>
    private void PopulateEditLocationCountryDDLs()
    {
        var countries = (from c in idc.Countries
                         orderby c.CountryName
                         select new
                         {
                             c.CountryID,
                             c.CountryName
                         });

        // Edit Billing Country DDL.
        this.ddlEditBillingCountry.DataSource = countries;
        this.ddlEditBillingCountry.DataTextField = "CountryName";
        this.ddlEditBillingCountry.DataValueField = "CountryID";
        this.ddlEditBillingCountry.DataBind();

        this.ddlEditBillingCountry.SelectedValue = "1"; // Default to USA.

        // Edit Shipping Country DDL.
        this.ddlEditShippingCountry.DataSource = countries;
        this.ddlEditShippingCountry.DataTextField = "CountryName";
        this.ddlEditShippingCountry.DataValueField = "CountryID";
        this.ddlEditShippingCountry.DataBind();

        this.ddlEditShippingCountry.SelectedValue = "1"; // Default to USA.
    }

    /// <summary>
    /// Populate DDL for Billing State for Add Location Information Modal/Dialog.
    /// </summary>
    /// <param name="countryid"></param>
    private void PopulateAddLocationBillingStateDDL(int countryid)
    {
        this.ddlAddBillingState.Items.Clear();

        var states = (from s in idc.States
                      where s.CountryID == countryid
                      orderby s.StateAbbrev
                      select new
                      {
                          s.StateID,
                          s.StateAbbrev
                      });

        // Billing State DDL.
        this.ddlAddBillingState.DataSource = states;
        this.ddlAddBillingState.DataTextField = "StateAbbrev";
        this.ddlAddBillingState.DataValueField = "StateID";
        this.ddlAddBillingState.DataBind();

        this.ddlAddBillingState.Items.Insert(0, new ListItem("-Select State-", "0"));
        this.ddlAddBillingState.SelectedIndex = 1;   // Default to Alaska (AK).
    }

    /// <summary>
    /// Populate DDL for Billing State for Edit Location Information Modal/Dialog.
    /// </summary>
    /// <param name="countryid"></param>
    private void PopulateEditLocationBillingStateDDL(int countryid)
    {
        this.ddlEditBillingState.Items.Clear();

        var states = (from s in idc.States
                      where s.CountryID == countryid
                      orderby s.StateAbbrev
                      select new
                      {
                          s.StateID,
                          s.StateAbbrev
                      });

        // Billing State DDL.
        this.ddlEditBillingState.DataSource = states;
        this.ddlEditBillingState.DataTextField = "StateAbbrev";
        this.ddlEditBillingState.DataValueField = "StateID";
        this.ddlEditBillingState.DataBind();

        this.ddlEditBillingState.Items.Insert(0, new ListItem("-Select State-", "0"));
        this.ddlEditBillingState.SelectedIndex = 1;   // Default to Alaska (AK).
    }

    /// <summary>
    /// Populate DDL for Billing State for Edit Location Information Modal/Dialog.
    /// </summary>
    /// <param name="countryid"></param>
    private void PopulateEditLocationShippingStateDDL(int countryid)
    {
        this.ddlEditShippingState.Items.Clear();

        var states = (from s in idc.States
                      where s.CountryID == countryid
                      orderby s.StateAbbrev
                      select new
                      {
                          s.StateID,
                          s.StateAbbrev
                      });

        // Billing State DDL.
        this.ddlEditShippingState.DataSource = states;
        this.ddlEditShippingState.DataTextField = "StateAbbrev";
        this.ddlEditShippingState.DataValueField = "StateID";
        this.ddlEditShippingState.DataBind();

        this.ddlEditShippingState.Items.Insert(0, new ListItem("-Select State-", "0"));
        this.ddlEditShippingState.SelectedIndex = 1;   // Default to Alaska (AK).
    }

    /// <summary>
    /// Populate DDL for Shipping State for Add Location Information Modal/Dialog.
    /// </summary>
    /// <param name="countryid"></param>
    private void PopulateAddLocationShippingStateDDL(int countryid)
    {
        this.ddlAddShippingState.Items.Clear();

        var states = (from s in idc.States
                      where s.CountryID == countryid
                      orderby s.StateAbbrev
                      select new
                      {
                          s.StateID,
                          s.StateAbbrev
                      });

        // Shipping State DDL.
        this.ddlAddShippingState.DataSource = states;
        this.ddlAddShippingState.DataTextField = "StateAbbrev";
        this.ddlAddShippingState.DataValueField = "StateID";
        this.ddlAddShippingState.DataBind();

        this.ddlAddShippingState.Items.Insert(0, new ListItem("-Select State-", "0"));
        this.ddlAddShippingState.SelectedIndex = 1;   // Default to Alaska (AK).
    }

    /// <summary>
    /// For Add Location Information Modal/Dialog.
    /// Populate Billing State DDL based on CountryID selected from Billing Country DDL.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void ddlAddBillingCountry_SelectedIndexChanged(object sender, EventArgs e)
    {
        int countryID = 0;

        Int32.TryParse(this.ddlAddBillingCountry.SelectedValue, out countryID);

        if (countryID == 0) return;

        PopulateAddLocationBillingStateDDL(countryID);
    }

    /// <summary>
    /// For Add Location Information Modal/Dialog.
    /// Populate Shipping State DDL based on CountryID selected from Shipping Country DDL.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void ddlAddShippingCountry_SelectedIndexChanged(object sender, EventArgs e)
    {
        int countryID = 0;

        Int32.TryParse(this.ddlAddShippingCountry.SelectedValue, out countryID);

        if (countryID == 0) return;

        PopulateAddLocationShippingStateDDL(countryID);
    }

    /// <summary>
    /// If Add Shipping Information same as Add Billing Information checkbox checked/unchecked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void chkbxSameAsAddBillingInformation_CheckedChanged(object sender, EventArgs e)
    {
        // Populate with correct State, based on CountryID.
        int shippingCountryID;
        int.TryParse(ddlAddBillingCountry.SelectedValue, out shippingCountryID);
        PopulateAddLocationShippingStateDDL(shippingCountryID);

        if (this.chkbxSameAsAddBillingInformation.Checked == true)
        {
            // Set all Shipping Information controls to their respective Billing Information controls' values.
            txtAddShippingCompanyName.Text = txtAddBillingCompanyName.Text;
            ddlAddShippingPrefix.SelectedValue = ddlAddBillingPrefix.SelectedValue;
            txtAddShippingFirstName.Text = txtAddBillingFirstName.Text;
            txtAddShippingLastName.Text = txtAddBillingLastName.Text;
            txtAddShippingAddressLine1.Text = txtAddBillingAddressLine1.Text;
            txtAddShippingAddressLine2.Text = txtAddBillingAddressLine2.Text;
            txtAddShippingAddressLine3.Text = txtAddBillingAddressLine3.Text;
            ddlAddShippingCountry.SelectedValue = ddlAddBillingCountry.SelectedValue;
            txtAddShippingCity.Text = txtAddBillingCity.Text;
            txtAddShippingPostalCode.Text = txtAddBillingPostalCode.Text;
            // Populate with correct State, based on CountryID.
            ddlAddShippingState.SelectedValue = ddlAddBillingState.SelectedValue;
            txtAddShippingTelephone.Text = txtAddBillingTelephone.Text;
            txtAddShippingFax.Text = txtAddBillingFax.Text;
            txtAddShippingEmailAddress.Text = txtAddBillingEmailAddress.Text;
        }
        else
        {
            // All Shipping controls should be set to empty or default values.
            txtAddShippingCompanyName.Text = string.Empty;
            ddlAddShippingPrefix.SelectedValue = "";
            txtAddShippingFirstName.Text = string.Empty;
            txtAddShippingLastName.Text = string.Empty;
            txtAddShippingAddressLine1.Text = string.Empty;
            txtAddShippingAddressLine2.Text = string.Empty;
            txtAddShippingAddressLine3.Text = string.Empty;
            txtAddShippingCity.Text = string.Empty;
            txtAddShippingPostalCode.Text = string.Empty;
            ddlAddShippingCountry.SelectedValue = shippingCountryID.ToString();
            ddlAddShippingState.SelectedIndex = 1;
            txtAddShippingTelephone.Text = string.Empty;
            txtAddShippingFax.Text = string.Empty;
            txtAddShippingEmailAddress.Text = string.Empty;
        }
    }

    /// <summary>
    /// If Edit Shipping Information same as Edit Billing Information checkbox checked/unchecked.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void chkbxSameAsEditBillingInformation_CheckedChanged(object sender, EventArgs e)
    {
        // Populate with correct State, based on CountryID.
        int shippingCountryID;
        int.TryParse(ddlEditBillingCountry.SelectedValue, out shippingCountryID);
        PopulateEditLocationShippingStateDDL(shippingCountryID);

        if (this.chkbxSameAsEditBillingInformation.Checked == true)
        {
            // Set all Shipping Information controls to their respective Billing Information controls' values.
            txtEditShippingCompanyName.Text = txtEditBillingCompanyName.Text;
            ddlEditShippingPrefix.SelectedValue = ddlEditBillingPrefix.SelectedValue;
            txtEditShippingFirstName.Text = txtEditBillingFirstName.Text;
            txtEditShippingLastName.Text = txtEditBillingLastName.Text;
            txtEditShippingAddressLine1.Text = txtEditBillingAddressLine1.Text;
            txtEditShippingAddressLine2.Text = txtEditBillingAddressLine2.Text;
            txtEditShippingAddressLine3.Text = txtEditBillingAddressLine3.Text;
            ddlEditShippingCountry.SelectedValue = ddlEditBillingCountry.SelectedValue;
            txtEditShippingCity.Text = txtEditBillingCity.Text;
            txtEditShippingPostalCode.Text = txtEditBillingPostalCode.Text;
            // Populate with correct State, based on CountryID.
            ddlEditShippingState.SelectedValue = ddlEditBillingState.SelectedValue;
            txtEditShippingTelephone.Text = txtEditBillingTelephone.Text;
            txtEditShippingFax.Text = txtEditBillingFax.Text;
            txtEditShippingEmailAddress.Text = txtEditBillingEmailAddress.Text;
        }
        else
        {
            // All Shipping controls should be set to empty or default values.
            txtEditShippingCompanyName.Text = string.Empty;
            ddlEditShippingPrefix.SelectedValue = "";
            txtEditShippingFirstName.Text = string.Empty;
            txtEditShippingLastName.Text = string.Empty;
            txtEditShippingAddressLine1.Text = string.Empty;
            txtEditShippingAddressLine2.Text = string.Empty;
            txtEditShippingAddressLine3.Text = string.Empty;
            txtEditShippingCity.Text = string.Empty;
            txtEditShippingPostalCode.Text = string.Empty;
            ddlEditShippingCountry.SelectedValue = shippingCountryID.ToString();
            ddlEditShippingState.SelectedIndex = 1;
            txtEditShippingTelephone.Text = string.Empty;
            txtEditShippingFax.Text = string.Empty;
            txtEditShippingEmailAddress.Text = string.Empty;
        }
    }

    /// <summary>
    /// For Add Location Information Modal/Dialog.
    /// Populate Add State ID DDL based on Add Country ID DDL selected value.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void ddlEditBillingCountry_SelectedIndexChanged(object sender, EventArgs e)
    {
        int countryID = 0;

        Int32.TryParse(this.ddlEditBillingCountry.SelectedValue, out countryID);

        if (countryID == 0) return;

        PopulateEditLocationBillingStateDDL(countryID);
    }

    /// <summary>
    /// For Edit Location Information Modal/Dialog.
    /// Populate Edit State ID DDL based on Edit Country ID DDL selected value.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void ddlEditShippingCountry_SelectedIndexChanged(object sender, EventArgs e)
    {
        int countryID = 0;

        Int32.TryParse(this.ddlEditShippingCountry.SelectedValue, out countryID);

        if (countryID == 0) return;

        PopulateEditLocationShippingStateDDL(countryID);
    }
    #endregion

    #region Add & Edit Order Information Modal/Dialog functions.
    /// <summary>
    /// Populate DDL for Add Product Group IDs based on Products table.
    /// </summary>
    protected void PopulateAddOrderProductGroupIDDDL()
    {
        var productsgroups = (from p in idc.Products
                              orderby p.ProductName ascending
                              select new
                              {
                                  ProductGroupID = p.ProductGroupID,
                                  ProductName = p.ProductName
                              }).Distinct();

        // Product ID(s) DDL.
        this.ddlAddProductGroupID.DataSource = productsgroups;
        this.ddlAddProductGroupID.DataTextField = "ProductName";
        this.ddlAddProductGroupID.DataValueField = "ProductGroupID";
        this.ddlAddProductGroupID.DataBind();

        this.ddlAddProductGroupID.SelectedIndex = 0;
    }

    /// <summary>
    /// Populate DDL for Edit Product Group IDs based on Products table.
    /// </summary>
    protected void PopulateEditOrderProductGroupIDDDL()
    {
        var productsgroups = (from p in idc.Products
                              orderby p.ProductName ascending
                              select new
                              {
                                  ProductGroupID = p.ProductGroupID,
                                  ProductName = p.ProductName
                              }).Distinct();

        // Product ID(s) DDL.
        this.ddlEditProductGroupID.DataSource = productsgroups;
        this.ddlEditProductGroupID.DataTextField = "ProductName";
        this.ddlEditProductGroupID.DataValueField = "ProductGroupID";
        this.ddlEditProductGroupID.DataBind();

        this.ddlEditProductGroupID.SelectedIndex = 0;
    }

    /// <summary>
    /// Populate DDL for Add Product IDs based on Product Group ID.
    /// </summary>
    /// <param name="countryid"></param>
    private void PopulateAddOrderProductIDDDL(int productgroupid)
    {
        this.ddlAddProductID.Items.Clear();

        var products = (from p in idc.Products
                        where p.ProductGroupID == productgroupid
                        && p.ProductSKU != "/LOST BADGE"
                        && p.ProductSKU != "/RENEWAL"
                        && p.ProductSKU != "/CREDIT"
                        orderby p.ProductSKU ascending
                        select new
                        {
                            ProductID = p.ProductID,
                            ProductDescription = p.Color + "-" + p.ProductSKU
                        });

        // Product ID(s) DDL.
        this.ddlAddProductID.DataSource = products;
        this.ddlAddProductID.DataTextField = "ProductDescription";
        this.ddlAddProductID.DataValueField = "ProductID";
        this.ddlAddProductID.DataBind();

        this.ddlAddProductID.Items.Insert(0, new ListItem("-Select-", "0"));
    }

    /// <summary>
    /// Populate DDL for Edit Product IDs based on Product Group ID.
    /// </summary>
    /// <param name="countryid"></param>
    private void PopulateEditOrderProductIDDDL(int productgroupid)
    {
        this.ddlEditProductID.Items.Clear();

        var products = (from p in idc.Products
                        where p.ProductGroupID == productgroupid
                        && p.ProductSKU != "/LOST BADGE"
                        && p.ProductSKU != "/RENEWAL"
                        && p.ProductSKU != "/CREDIT"
                        orderby p.ProductSKU ascending
                        select new
                        {
                            ProductID = p.ProductID,
                            ProductDescription = p.Color + "-" + p.ProductSKU
                        });

        // Product ID(s) DDL.
        this.ddlEditProductID.DataSource = products;
        this.ddlEditProductID.DataTextField = "ProductDescription";
        this.ddlEditProductID.DataValueField = "ProductID";
        this.ddlEditProductID.DataBind();

        this.ddlEditProductID.Items.Insert(0, new ListItem("-Select-", "0"));
    }

    /// <summary>
    /// For Add Order Information Modal/Dialog.
    /// Populate Add Product ID DDL based on Add ProductGroupID selected value.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void ddlAddProductGroupID_SelectedIndexChanged(object sender, EventArgs e)
    {
        int productGroupID = 0;

        int.TryParse(this.ddlAddProductGroupID.SelectedValue, out productGroupID);

        if (productGroupID == 0) return;

        PopulateAddOrderProductIDDDL(productGroupID);
    }

    /// <summary>
    /// For Edit Order Information Modal/Dialog.
    /// Populate Edit Product ID DDL based on Edit ProductGroupID selected value.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void ddlEditProductGroupID_SelectedIndexChanged(object sender, EventArgs e)
    {
        int productGroupID = 0;

        int.TryParse(this.ddlEditProductGroupID.SelectedValue, out productGroupID);

        if (productGroupID == 0) return;

        PopulateEditOrderProductIDDDL(productGroupID);
    }
    # endregion

    #region Insert CRM Account Information (to include Locations and Orders).
    protected void InsertCRMAccountRecordIntoDB()
    {
        DateTime serviceStartDate = DateTime.Now;
        DateTime serviceEndDate = DateTime.Now;
        DateTime.TryParse(txtServiceStartDate.Text, out serviceStartDate);
        DateTime.TryParse(txtServiceEndDate.Text, out serviceEndDate); 

        // Create CRMAccount Object/Records.
        CRMAccounts crmacct = null;
        crmacct = new CRMAccounts();
        crmacct.CreatedDate = DateTime.Now;
        crmacct.CreatedBy = UserName;
        crmacct.AccountID = null;
        // Account Information.
        crmacct.BrandSourceID = Convert.ToInt32(ddlBrandSource.SelectedValue);
        if (ddlDealer.SelectedValue != "0") crmacct.DealerID = Convert.ToInt32(ddlDealer.SelectedValue);
        else crmacct.DealerID = null;
        crmacct.AccountName = txtAccountName.Text.Trim().Replace("'", "''");
        crmacct.CompanyName = txtCompanyName.Text.Trim().Replace("'", "''");
        crmacct.SalesRepDistID = ddlReferral.SelectedValue;
        crmacct.CustomerTypeID = Convert.ToInt32(ddlCustomerType.SelectedValue);
        crmacct.IndustryID = Convert.ToInt32(ddlIndustryType.SelectedValue);
        if (ddlUnixCustomerType.SelectedValue != "0") crmacct.UnixCustomerTypeID = Convert.ToInt32(ddlUnixCustomerType.SelectedValue);
        else crmacct.UnixCustomerTypeID = null;
        crmacct.ServiceStartDate = serviceStartDate;
        crmacct.ServiceEndDate = serviceEndDate;
        // Billing Method Information.
        crmacct.BillingTermID = Convert.ToInt32(ddlBillingFrequency.SelectedValue);
        if (rdobtnPONumber.Checked == true) crmacct.BillingMethod = "Purchase Order";
        if (rdobtnCreditCard.Checked == true) crmacct.BillingMethod = "Credit Card";
        if (txtPONumber.Text != "") crmacct.PONumber = txtPONumber.Text.Trim();
        else crmacct.PONumber = null;
        if (rdobtnCreditCard.Checked == true)
        {
            crmacct.CreditCardTypeID = Convert.ToInt32(rdobtnlstCreditCardType.SelectedValue);
            crmacct.NameOnCard = txtNameOnCard.Text.Trim().Replace("'", "''");
            crmacct.NumberEncrypted = Instadose.Security.TripleDES.Encrypt(txtCreditCardNumber.Text.Trim());
            crmacct.ExpMonth = Convert.ToInt32(Convert.ToInt32(ddlCCExpirationMonth.SelectedValue));
            crmacct.ExpYear = Convert.ToInt32(Convert.ToInt32(ddlCCExpirationYear.SelectedValue));
            crmacct.SecurityCode = txtCVC.Text.Trim();
        }
        else
        {
            crmacct.CreditCardTypeID = null;
            crmacct.NameOnCard = null;
            crmacct.NumberEncrypted = null;
            crmacct.SecurityCode = null;
            crmacct.ExpMonth = null;
            crmacct.ExpYear = null;
        }
        // Account Administrator Information.
        crmacct.Username = txtUsername.Text.Trim().Replace("'", "''");
        crmacct.SecurityQuestionID1 = Convert.ToInt32(ddlSecurityQuestion.SelectedValue);
        crmacct.SecurityAnswer1 = txtSecurityAnswer.Text.Trim().Replace("'", "''");
        if (ddlPrefix.SelectedValue != "") crmacct.Prefix = ddlPrefix.SelectedValue;
        else crmacct.Prefix = null;
        crmacct.FirstName = txtFirstName.Text.Trim().Replace("'", "''");
        crmacct.LastName = txtLastName.Text.Trim().Replace("'", "''");
        crmacct.Gender = rdobtnlstGender.SelectedValue;
        crmacct.EmailAddress = txtEmail.Text.Trim();
        crmacct.Telephone = txtTelephone.Text.Trim();
        if (txtFax.Text != "") crmacct.Fax = txtFax.Text.Trim();
        else crmacct.Fax = null;
        crmacct.Active = true;
        crmacct.Status = "P";

        idc.CRMAccounts.InsertOnSubmit(crmacct);

        try
        {
            // Insert record into DB.
            idc.SubmitChanges();

            int crmAccountID = 0;

            // Get just-created CRMAccountID.
            crmAccountID = (from crma in idc.CRMAccounts
                            orderby crma.CreatedDate descending
                            select crma.CRMAccountID).FirstOrDefault();

            if (hdnfldLocationJSONString.Value != "")
            {
                // Use CRMAccountID to insert Location Object(s)/Record(s).
                InsertCRMLocationRecords(crmAccountID);
            }

            if (hdnfldOrderJSONString.Value != "")
            {
                // Use CRMAccountID to insert Order Object(s)/Record(s).
                InsertCRMOrderRecords(crmAccountID);
            }
        }
        catch (Exception ex)
        {
            this.VisibleErrorMessage(ex.ToString());
            return;
        }
    }
    #endregion

    #region Insert Order Objects/Records into DB.
    public class AddOrderArray
    {
        public int index { get; set; }
        public string salesrepdistid { get; set; }
        public int productgroupid { get; set; }
        public int productid { get; set; }
        public string color { get; set; }
        public string productsku { get; set; }
        public int quantity { get; set; }
        public decimal unitprice { get; set; }
    }

    public class RootOrderObject
    {
        public List<AddOrderArray> addOrderArray { get; set; }
    }

    protected void InsertCRMOrderRecords(int crmaccountid)
    {
        string json = hdnfldOrderJSONString.Value;

        dynamic jsonObject = JsonConvert.DeserializeObject(json);

        foreach (var obj in jsonObject.addOrderArray)
        {
            // Create new CRMOrder object/record.
            CRMOrders crmo = null;
            crmo = new CRMOrders();
            crmo.CRMAccountID = crmaccountid;   // Will have to get this once the CRMAccount record is entered.
            crmo.ProductGroupID = obj.productgroupid;
            crmo.ProductID = obj.productid;
            crmo.Color = obj.color.ToString();
            crmo.ProductSKU = obj.productsku.ToString();
            crmo.Quantity = obj.quantity;
            crmo.UnitPrice = obj.unitprice;
            crmo.Active = true;

            idc.CRMOrders.InsertOnSubmit(crmo);

            try
            {
                idc.SubmitChanges();
            }
            catch (Exception ex)
            {
                VisibleErrorMessage(ex.ToString());
                return;
            }
        }
    }
    #endregion

    #region Insert Location Objects/Records into DB.
    public class AddLocationArray
    {
        public int index { get; set; }
        public string billingcompanyname { get; set; }
        public string billingprefix { get; set; }
        public string billingfirstname { get; set; }
        public string billinglastname { get; set; }
        public string billingaddressline1 { get; set; }
        public string billingaddressline2 { get; set; }
        public string billingaddressline3 { get; set; }
        public string billingcity { get; set; }
        public int billingstate { get; set; }
        public string billingpostalcode { get; set; }
        public int billingcountry { get; set; }
        public string billingtelephone { get; set; }
        public string billingfax { get; set; }
        public string billingemailaddress { get; set; }
        public string shippingcompanyname { get; set; }
        public string shippingprefix { get; set; }
        public string shippingfirstname { get; set; }
        public string shippinglastname { get; set; }
        public string shippingaddressline1 { get; set; }
        public string shippingaddressline2 { get; set; }
        public string shippingaddressline3 { get; set; }
        public string shippingcity { get; set; }
        public int shippingstate { get; set; }
        public string shippingpostalcode { get; set; }
        public int shippingcountry { get; set; }
        public string shippingtelephone { get; set; }
        public string shippingfax { get; set; }
        public string shippingemailaddress { get; set; }
    }

    public class RootLocationObject
    {
        public List<AddLocationArray> addLocationArray { get; set; }
    }

    protected void InsertCRMLocationRecords(int crmaccountid)
    {
        string json = hdnfldLocationJSONString.Value;

        dynamic jsonObject = JsonConvert.DeserializeObject(json);

        foreach (var obj in jsonObject.addLocationArray)
        {
            // Create new CRMOrder object/record.
            CRMLocations crml = null;
            crml = new CRMLocations();
            crml.CRMAccountID = crmaccountid;   // Will have to get this once the CRMAccount record is entered.
            // Billing Information.
            crml.BillingCompanyName = obj.billingcompanyname;
            crml.BillingNamePrefix = (obj.billingprefix != "" ? obj.billingprefix : null);
            crml.BillingFirstName = obj.billingfirstname;
            crml.BillingLastName = obj.billinglastname;
            crml.BillingEmailAddress = obj.billingemailaddress;
            crml.BillingTelephone = obj.billingtelephone;
            crml.BillingFax = (obj.billingfax != "" ? obj.billingfax : null);
            crml.BillingAddress1 = obj.billingaddressline1;
            crml.BillingAddress2 = (obj.billingaddressline2 != "" ? obj.billingaddressline2 : null);
            crml.BillingAddress3 = (obj.billingaddressline3 != "" ? obj.billingaddressline3 : null);
            crml.BillingCity = obj.billingcity;
            crml.BillingStateID = obj.billingstate;
            crml.BillingPostalCode = obj.billingpostalcode;
            crml.BillingCountryID = obj.billingcountry;
            // Shipping Information.
            crml.ShippingCompanyName = obj.shippingcompanyname;
            crml.ShippingNamePrefix = (obj.shippingprefix != "" ? obj.shippingprefix : null);
            crml.ShippingFirstName = obj.shippingfirstname;
            crml.ShippingLastName = obj.shippinglastname;
            crml.ShippingEmailAddress = (obj.shippingemailaddress != "" ? obj.shippingemailaddress : null);
            crml.ShippingTelephone = obj.shippingtelephone;
            crml.ShippingFax = (obj.shippingfax != "" ? obj.shippingfax : null);
            crml.ShippingAddress1 = obj.shippingaddressline1;
            crml.ShippingAddress2 = (obj.shippingaddressline2 != "" ? obj.shippingaddressline2 : null);
            crml.ShippingAddress3 = (obj.shippingaddressline3 != "" ? obj.shippingaddressline3 : null);
            crml.ShippingCity = obj.shippingcity;
            crml.ShippingStateID = obj.shippingstate;
            crml.ShippingPostalCode = obj.shippingpostalcode;
            crml.ShippingCountryID = obj.shippingcountry;
            crml.Active = true;

            idc.CRMLocations.InsertOnSubmit(crml);

            try
            {
                idc.SubmitChanges();
            }
            catch (Exception ex)
            {
                VisibleErrorMessage(ex.ToString());
                return;
            }
        }
    }
    #endregion

    protected void btnAddCRMAccount_Click(object sender, EventArgs e)
    {
        try
        {
            InsertCRMAccountRecordIntoDB();
        }
        catch (Exception ex)
        {
            VisibleErrorMessage(ex.ToString());
            return;
        }
    }

    protected void btnCancelCRMAccount_Click(object sender, EventArgs e)
    {
        Response.Redirect("CRMAccounts.aspx");
    }

    #region Error Message(s).
    // Reset Error Message.
    private void InvisibleErrorMessage()
    {
        this.spnErrorMessage.InnerText = "";
        this.divErrorMessage.Visible = false;
    }

    // Set Error Message.
    private void VisibleErrorMessage(string errormessage)
    {
        this.spnErrorMessage.InnerText = errormessage;
        this.divErrorMessage.Visible = true;
    }
    #endregion
}