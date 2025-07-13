using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Instadose.Data;

public partial class CustomerService_EditCRMAccounts : System.Web.UI.Page
{
    int crmAccountID = 0;
    string commandName = "";

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
        int.TryParse(Request.QueryString["ID"].ToString(), out crmAccountID);

        if (crmAccountID != 0)
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
               
                hdnfldAccordionIndex.Value = "0";

                PopulateReferralDDL();
                PopulateCustomerTypeDDL();
                PopulateUNIXCustomerTypesDDL();
                PopulateCreditCardExpirationYear();
                PopulateSecurityQuestionsDDL();
                PopulateDealersDDL();
                PopulateIndustryTypesDDL();

                PopulateAddEditLocationCountryDDLs();
                int countryIDBilling;
                int countryIDShipping;
                int.TryParse(this.ddlBillingCountry.SelectedValue, out countryIDBilling);
                int.TryParse(this.ddlShippingCountry.SelectedValue, out countryIDShipping);
                PopulateAddEditLocationBillingStateDDL(countryIDBilling);
                PopulateAddEditLocationShippingStateDDL(countryIDShipping);

                int productGroupID = 0;
                PopulateAddEditOrderProductGroupIDDDL();
                int.TryParse(this.ddlProductGroupID.SelectedValue, out productGroupID);
                PopulateAddEditOrderProductIDDDL(productGroupID);

                GetLocations(crmAccountID);
                GetOrders(crmAccountID);

                // Fill in form controls.
                PopulateForm(crmAccountID);
            }
            else
            {
                Response.Redirect("~/Default.aspx");
            }
        }
        else
        {
            Response.Redirect("CRMAccounts.aspx");
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

    /// <summary>
    /// Populate form with CRM Account information.
    /// </summary>
    /// <param name="crmaccountid"></param>
    protected void PopulateForm(int crmaccountid)
    {
        var crmacct = (from acct in idc.CRMAccounts
                       where acct.CRMAccountID == crmaccountid
                       select acct).FirstOrDefault();

        if (crmacct == null) return;

        ddlBrandSource.SelectedValue = crmacct.BrandSourceID.ToString();
        if (crmacct.BrandSourceID == 3)
        {
            ddlDealer.SelectedValue = crmacct.DealerID.ToString();
        }
        txtAccountName.Text = crmacct.AccountName;
        txtCompanyName.Text = crmacct.CompanyName;
        ddlReferral.SelectedValue = crmacct.SalesRepDistID;
        ddlIndustryType.SelectedValue = crmacct.IndustryID.ToString();
        ddlCustomerType.SelectedValue = crmacct.CustomerTypeID.ToString();
        if (crmacct.UnixCustomerTypeID.HasValue)
        {
            ddlUnixCustomerType.SelectedValue = crmacct.UnixCustomerTypeID.ToString();
        }
        else
        {
            ddlUnixCustomerType.SelectedValue = "0";
        }
        txtServiceStartDate.Text = string.Format("{0:MM/dd/yyyy}", crmacct.ServiceStartDate);
        txtServiceEndDate.Text = string.Format("{0:MM/dd/yyyy}", crmacct.ServiceEndDate);
        ddlBillingFrequency.SelectedValue = crmacct.BillingTermID.ToString();
        if (crmacct.BillingMethod == "Purchase Order")
        {
            divPONumber.Visible = true;
            divCreditCardInformation.Visible = false;
            txtPONumber.Text = crmacct.PONumber;
        }
        else
        {
            divPONumber.Visible = false;
            divCreditCardInformation.Visible = true;
            rdobtnlstCreditCardType.SelectedValue = crmacct.CreditCardTypeID.ToString();
            string creditCardName = (from cc in idc.CreditCardTypes
                                     where cc.CreditCardTypeID == crmacct.CreditCardTypeID
                                     select cc.CreditCardName).FirstOrDefault();
            txtNameOnCard.Text = crmacct.NameOnCard;
            txtCreditCardNumber.Text = Common.MaskCreditCardNumber(crmacct.NumberEncrypted, creditCardName);
            ddlCCExpirationMonth.SelectedValue = crmacct.ExpMonth.ToString();
            ddlCCExpirationYear.SelectedValue = crmacct.ExpYear.ToString();
            txtCVC.Text = crmacct.SecurityCode;
        }
        txtUsername.Text = crmacct.Username;
        ddlSecurityQuestion.SelectedValue = crmacct.SecurityQuestionID1.ToString();
        txtSecurityAnswer.Text = crmacct.SecurityAnswer1;
        if (crmacct.Prefix != null) ddlPrefix.SelectedValue = crmacct.Prefix;
        else ddlPrefix.SelectedValue = "";
        txtFirstName.Text = crmacct.FirstName;
        txtLastName.Text = crmacct.LastName;
        rdobtnlstGender.SelectedValue = crmacct.Gender;
        txtEmail.Text = crmacct.EmailAddress;
        txtTelephone.Text = crmacct.Telephone;
        if (crmacct.Fax != null) txtFax.Text = crmacct.Fax;
        else txtFax.Text = "";
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

    #region Location & Order GridViews.
    /// <summary>
    /// Populate Locations GridView with formated information.
    /// </summary>
    /// <param name="crmaccountid"></param>
    protected void GetLocations(int crmaccountid)
    {
        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        //// Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.Text;

        string sqlQuery = "SELECT * FROM CRMLocations WHERE Active = 1 AND CRMAccountID = " + crmaccountid;

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader dreader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable dtCRMLocations = new DataTable();

        dtCRMLocations = new DataTable("CRM Locations");

        // Create the columns for the DataTable.
        dtCRMLocations.Columns.Add("CRMLocationID", Type.GetType("System.Int32"));
        dtCRMLocations.Columns.Add("CRMAccountID", Type.GetType("System.Int32"));
        // Billing Information.
        dtCRMLocations.Columns.Add("BillingCompanyName", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("BillingNamePrefix", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("BillingFirstName", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("BillingLastName", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("BillingEmailAddress", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("BillingTelephone", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("BillingFax", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("BillingAddress1", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("BillingAddress2", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("BillingAddress3", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("BillingCity", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("BillingStateID", Type.GetType("System.Int32"));
        dtCRMLocations.Columns.Add("BillingPostalCode", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("BillingCountryID", Type.GetType("System.String"));
        // Formatted Billing Information.
        dtCRMLocations.Columns.Add("BillingContactName", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("BillingAddress", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("BillingCityStatePostalCode", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("BillingCountry", Type.GetType("System.String"));

        // Shipping Information.
        dtCRMLocations.Columns.Add("ShippingCompanyName", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("ShippingNamePrefix", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("ShippingFirstName", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("ShippingLastName", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("ShippingEmailAddress", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("ShippingTelephone", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("ShippingFax", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("ShippingAddress1", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("ShippingAddress2", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("ShippingAddress3", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("ShippingCity", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("ShippingStateID", Type.GetType("System.Int32"));
        dtCRMLocations.Columns.Add("ShippingPostalCode", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("ShippingCountryID", Type.GetType("System.String"));
        // Formatted Billing Information.
        dtCRMLocations.Columns.Add("ShippingContactName", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("ShippingAddress", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("ShippingCityStatePostalCode", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("ShippingCountry", Type.GetType("System.String"));
        dtCRMLocations.Columns.Add("Active", Type.GetType("System.Boolean"));


        while (dreader.Read())
        {
            DataRow drow = dtCRMLocations.NewRow();

            string billingContactName = "";
            string billingNamePrefix = dreader["BillingNamePrefix"].ToString();
            if (billingNamePrefix != null)
            {
                billingContactName = billingNamePrefix + " " + dreader["BillingFirstName"].ToString() + " " + dreader["BillingLastName"].ToString();
            }
            else
            {
                billingContactName = dreader["BillingFirstName"].ToString() + " " + dreader["BillingLastName"].ToString();
            }
            string billingAddressLine2 = dreader["BillingAddress2"].ToString();
            string billingAddressLine3 = dreader["BillingAddress3"].ToString();
            string billingFax = dreader["BillingFax"].ToString();
            int billingStateID = 0;
            int.TryParse(dreader["BillingStateID"].ToString(), out billingStateID);
            string billingStateName = (from s in idc.States
                                       where s.StateID == billingStateID
                                       select s.StateName).FirstOrDefault();
            int billingCountryID = 0;
            int.TryParse(dreader["BillingCountryID"].ToString(), out billingCountryID);
            string billingCountryName = (from c in idc.Countries
                                         where c.CountryID == billingCountryID
                                         select c.CountryName).FirstOrDefault();
            string formattedBillingAddress = "";
            // Format Billing Address.
            if (billingAddressLine2 != "" && billingAddressLine3 != "")
            {
                formattedBillingAddress = dreader["BillingAddress1"].ToString();
                formattedBillingAddress += "<br />" + dreader["BillingAddress2"].ToString();
                formattedBillingAddress += "<br />" + dreader["BillingAddress3"].ToString();
            }
            else if (billingAddressLine2 != "" && billingAddressLine3 == "")
            {
                formattedBillingAddress = dreader["BillingAddress1"].ToString();
                formattedBillingAddress += "<br />" + dreader["BillingAddress2"].ToString();
            }
            else
            {
                formattedBillingAddress = dreader["BillingAddress1"].ToString();
            }
            // Format Billing City, State, and Postal Code.
            string billingCityStatePostalCode = dreader["BillingCity"].ToString() + ", " + billingStateName + " " + dreader["BillingPostalCode"].ToString();
            // Formate Billing Fax.
            if (billingFax != null)
            {
                billingFax = dreader["BillingFax"].ToString();
            }
            else
            {
                billingFax = "(none provided)";
            }

            string shippingContactName = "";
            string shippingNamePrefix = dreader["ShippingNamePrefix"].ToString();
            if (billingNamePrefix != null)
            {
                shippingContactName = shippingNamePrefix + " " + dreader["ShippingFirstName"].ToString() + " " + dreader["ShippingLastName"].ToString();
            }
            else
            {
                shippingContactName = dreader["ShippingFirstName"].ToString() + " " + dreader["ShippingLastName"].ToString();
            }
            string shippingAddressLine2 = dreader["ShippingAddress2"].ToString();
            string shippingAddressLine3 = dreader["ShippingAddress3"].ToString();
            string shippingEmailAddress = dreader["ShippingEmailAddress"].ToString();
            string shippingFax = dreader["ShippingFax"].ToString();
            int shippingStateID = 0;
            int.TryParse(dreader["ShippingStateID"].ToString(), out shippingStateID);
            string shippingStateName = (from s in idc.States
                                        where s.StateID == shippingStateID
                                        select s.StateName).FirstOrDefault();
            int shippingCountryID = 0;
            int.TryParse(dreader["ShippingCountryID"].ToString(), out shippingCountryID);
            string shippingCountryName = (from c in idc.Countries
                                          where c.CountryID == shippingCountryID
                                          select c.CountryName).FirstOrDefault();
            string formattedShippingAddress = "";
            // Format Shipping Address.
            if (shippingAddressLine2 != "" && shippingAddressLine3 != "")
            {
                formattedShippingAddress = dreader["ShippingAddress1"].ToString();
                formattedShippingAddress += "<br />" + dreader["ShippingAddress2"].ToString();
                formattedShippingAddress += "<br />" + dreader["ShippingAddress3"].ToString();
            }
            else if (shippingAddressLine2 != "" && shippingAddressLine3 == "")
            {
                formattedShippingAddress = dreader["ShippingAddress1"].ToString();
                formattedShippingAddress += "<br />" + dreader["ShippingAddress2"].ToString();
            }
            else
            {
                formattedShippingAddress = dreader["ShippingAddress1"].ToString();
            }
            // Format Shipping City, State, and Postal Code.
            string shippingCityStatePostalCode = dreader["ShippingCity"].ToString() + ", " + shippingStateName + " " + dreader["ShippingPostalCode"].ToString();
            // Formate Shipping Fax.
            if (shippingFax != null)
            {
                shippingFax = dreader["ShippingFax"].ToString();
            }
            else
            {
                shippingFax = "(none provided)";
            }
            // Format Shipping E-mail Address.
            if (shippingEmailAddress != null)
            {
                shippingEmailAddress = dreader["ShippingEmailAddress"].ToString();
            }
            else
            {
                shippingEmailAddress = "(none provided)";
            }

            // Fill row details.
            drow["CRMLocationID"] = dreader["CRMLocationID"];
            drow["CRMAccountID"] = dreader["CRMAccountID"];
            // Billing Information.
            drow["BillingCompanyName"] = dreader["BillingCompanyName"];
            drow["BillingContactName"] = billingContactName;
            drow["BillingEmailAddress"] = "E-mail: " + dreader["BillingEmailAddress"];
            drow["BillingTelephone"] = "Telephone: " + dreader["BillingTelephone"];
            drow["BillingFax"] = "Fax: " + billingFax;
            drow["BillingAddress"] = formattedBillingAddress;
            drow["BillingCityStatePostalCode"] = billingCityStatePostalCode;
            drow["BillingCountry"] = billingCountryName;
            // Shipping Information.
            drow["ShippingCompanyName"] = dreader["ShippingCompanyName"];
            drow["ShippingContactName"] = shippingContactName;
            drow["ShippingEmailAddress"] = "E-mail: " + shippingEmailAddress;
            drow["ShippingTelephone"] = "Telephone: " + dreader["ShippingTelephone"];
            drow["ShippingFax"] = "Fax: " + shippingFax;
            drow["ShippingAddress"] = formattedShippingAddress;
            drow["ShippingCityStatePostalCode"] = shippingCityStatePostalCode;
            drow["ShippingCountry"] = shippingCountryName;

            // Add rows to DataTable.
            dtCRMLocations.Rows.Add(drow);
        }

        this.gvLocations.DataSource = dtCRMLocations;
        this.gvLocations.DataBind();
    }

    /// <summary>
    /// Populate Orders GridView with formated information.
    /// </summary>
    /// <param name="crmaccountid"></param>
    protected void GetOrders(int crmaccountid)
    {
        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        //// Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.Text;

        string sqlQuery = "SELECT * FROM CRMOrders WHERE Active = 1 AND CRMAccountID = " + crmaccountid;

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader dreader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable dtCRMOrders = new DataTable();

        dtCRMOrders = new DataTable("CRM Orders");

        // Create the columns for the DataTable.
        dtCRMOrders.Columns.Add("CRMOrderID", Type.GetType("System.Int32"));
        dtCRMOrders.Columns.Add("CRMAccountID", Type.GetType("System.Int32"));
        dtCRMOrders.Columns.Add("ProductGroupID", Type.GetType("System.Int32"));
        dtCRMOrders.Columns.Add("ProductID", Type.GetType("System.Int32"));
        dtCRMOrders.Columns.Add("Color", Type.GetType("System.String"));
        dtCRMOrders.Columns.Add("ProductSKU", Type.GetType("System.String"));
        dtCRMOrders.Columns.Add("Quantity", Type.GetType("System.Int32"));
        dtCRMOrders.Columns.Add("UnitPrice", Type.GetType("System.Decimal"));
        dtCRMOrders.Columns.Add("Active", Type.GetType("System.Boolean"));
        // Formatted Columns.
        dtCRMOrders.Columns.Add("ProductGroupName", Type.GetType("System.String"));
        dtCRMOrders.Columns.Add("ProductName", Type.GetType("System.String"));


        while (dreader.Read())
        {
            DataRow drow = dtCRMOrders.NewRow();

            // Get ProductName from ProductID.
            int productID = 0;
            int.TryParse(dreader["ProductID"].ToString(), out productID);
            string productName = (from p in idc.Products
                                  where p.ProductID == productID
                                  select p.ProductName).FirstOrDefault();

            // Get ProductGroupName from ProductGroupID.
            int productGroupID = 0;
            int.TryParse(dreader["ProductGroupID"].ToString(), out productGroupID);
            string productGroupName = (from pg in idc.ProductGroups
                                       where pg.ProductGroupID == productGroupID
                                       select pg.ProductGroupName).FirstOrDefault();

            // Fill row details.
            drow["CRMOrderID"] = dreader["CRMOrderID"];
            drow["CRMAccountID"] = dreader["CRMAccountID"];
            drow["ProductGroupName"] = productGroupName;
            drow["ProductName"] = productName;
            drow["Color"] = dreader["Color"];
            drow["ProductSKU"] = dreader["ProductSKU"];
            drow["Quantity"] = dreader["Quantity"];
            drow["UnitPrice"] = dreader["UnitPrice"];

            // Add rows to DataTable.
            dtCRMOrders.Rows.Add(drow);
        }

        this.gvOrders.DataSource = dtCRMOrders;
        this.gvOrders.DataBind();
    }
    #endregion

    #region Add Location Functions.
    /// <summary>
    /// Open Add/Edit Location Information Form Modal/Dialog.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void lnkbtnAddLocation_Click(object sender, EventArgs e)
    {
        commandName = this.lnkbtnAddLocation.CommandName;
        hdnfldCommandName.Value = commandName;
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('divLocationInformationForm')", true);
    }

    /// <summary>
    /// Populate DDLs for Country for Add/Edit Location Information Modal/Dialog.
    /// </summary>
    private void PopulateAddEditLocationCountryDDLs()
    {
        var countries = (from c in idc.Countries
                         orderby c.CountryName
                         select new
                         {
                             c.CountryID,
                             c.CountryName
                         });

        // Add Billing Country DDL.
        this.ddlBillingCountry.DataSource = countries;
        this.ddlBillingCountry.DataTextField = "CountryName";
        this.ddlBillingCountry.DataValueField = "CountryID";
        this.ddlBillingCountry.DataBind();

        this.ddlBillingCountry.SelectedValue = "1"; // Default to USA.

        // Add Shipping Country DDL.
        this.ddlShippingCountry.DataSource = countries;
        this.ddlShippingCountry.DataTextField = "CountryName";
        this.ddlShippingCountry.DataValueField = "CountryID";
        this.ddlShippingCountry.DataBind();

        this.ddlShippingCountry.SelectedValue = "1"; // Default to USA.
    }

    /// <summary>
    /// Populate DDL for Billing State for Edit Location Information Modal/Dialog.
    /// </summary>
    /// <param name="countryid"></param>
    private void PopulateAddEditLocationBillingStateDDL(int countryid)
    {
        this.ddlBillingState.Items.Clear();

        var states = (from s in idc.States
                      where s.CountryID == countryid
                      orderby s.StateAbbrev
                      select new
                      {
                          s.StateID,
                          s.StateAbbrev
                      });

        // Billing State DDL.
        this.ddlBillingState.DataSource = states;
        this.ddlBillingState.DataTextField = "StateAbbrev";
        this.ddlBillingState.DataValueField = "StateID";
        this.ddlBillingState.DataBind();

        this.ddlBillingState.Items.Insert(0, new ListItem("-Select State-", "0"));
        this.ddlBillingState.SelectedIndex = 1;   // Default to Alaska (AK).
    }

    /// <summary>
    /// Populate DDL for Billing State for Edit Location Information Modal/Dialog.
    /// </summary>
    /// <param name="countryid"></param>
    private void PopulateAddEditLocationShippingStateDDL(int countryid)
    {
        this.ddlShippingState.Items.Clear();

        var states = (from s in idc.States
                      where s.CountryID == countryid
                      orderby s.StateAbbrev
                      select new
                      {
                          s.StateID,
                          s.StateAbbrev
                      });

        // Billing State DDL.
        this.ddlShippingState.DataSource = states;
        this.ddlShippingState.DataTextField = "StateAbbrev";
        this.ddlShippingState.DataValueField = "StateID";
        this.ddlShippingState.DataBind();

        this.ddlShippingState.Items.Insert(0, new ListItem("-Select State-", "0"));
        this.ddlShippingState.SelectedIndex = 1;   // Default to Alaska (AK).
    }

    /// <summary>
    /// For Add Location Information Modal/Dialog.
    /// Populate Billing State DDL based on CountryID selected from Billing Country DDL.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void ddlBillingCountry_SelectedIndexChanged(object sender, EventArgs e)
    {
        int countryID = 0;

        Int32.TryParse(this.ddlBillingCountry.SelectedValue, out countryID);

        if (countryID == 0) return;

        PopulateAddEditLocationBillingStateDDL(countryID);
    }

    /// <summary>
    /// For Add Location Information Modal/Dialog.
    /// Populate Shipping State DDL based on CountryID selected from Shipping Country DDL.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void ddlShippingCountry_SelectedIndexChanged(object sender, EventArgs e)
    {
        int countryID = 0;

        Int32.TryParse(this.ddlShippingCountry.SelectedValue, out countryID);

        if (countryID == 0) return;

        PopulateAddEditLocationShippingStateDDL(countryID);
    }

    /// <summary>
    /// Populates all Shipping Location fields with the same values as Billing Location fields.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void chkbxSameAsBillingInformation_CheckedChanged(object sender, EventArgs e)
    {
        // Populate with correct State, based on CountryID.
        int shippingCountryID;
        int.TryParse(ddlBillingCountry.SelectedValue, out shippingCountryID);
        PopulateAddEditLocationShippingStateDDL(shippingCountryID);

        if (this.chkbxSameAsBillingInformation.Checked == true)
        {
            // Set all Shipping Information controls to their respective Billing Information controls' values.
            txtShippingCompanyName.Text = txtBillingCompanyName.Text;
            ddlShippingPrefix.SelectedValue = ddlBillingPrefix.SelectedValue;
            txtShippingFirstName.Text = txtBillingFirstName.Text;
            txtShippingLastName.Text = txtBillingLastName.Text;
            txtShippingAddressLine1.Text = txtBillingAddressLine1.Text;
            txtShippingAddressLine2.Text = txtBillingAddressLine2.Text;
            txtShippingAddressLine3.Text = txtBillingAddressLine3.Text;
            ddlShippingCountry.SelectedValue = ddlBillingCountry.SelectedValue;
            txtShippingCity.Text = txtBillingCity.Text;
            txtShippingPostalCode.Text = txtBillingPostalCode.Text;
            // Populate with correct State, based on CountryID.
            ddlShippingState.SelectedValue = ddlBillingState.SelectedValue;
            txtShippingTelephone.Text = txtBillingTelephone.Text;
            txtShippingFax.Text = txtBillingFax.Text;
            txtShippingEmailAddress.Text = txtBillingEmailAddress.Text;
        }
        else
        {
            // All Shipping controls should be set to empty or default values.
            txtShippingCompanyName.Text = string.Empty;
            ddlShippingPrefix.SelectedValue = "";
            txtShippingFirstName.Text = string.Empty;
            txtShippingLastName.Text = string.Empty;
            txtShippingAddressLine1.Text = string.Empty;
            txtShippingAddressLine2.Text = string.Empty;
            txtShippingAddressLine3.Text = string.Empty;
            txtShippingCity.Text = string.Empty;
            txtShippingPostalCode.Text = string.Empty;
            ddlShippingCountry.SelectedValue = shippingCountryID.ToString();
            ddlShippingState.SelectedIndex = 1;
            txtShippingTelephone.Text = string.Empty;
            txtShippingFax.Text = string.Empty;
            txtShippingEmailAddress.Text = string.Empty;
        }
    }

    protected void btnSave_Location_Click(object sender, EventArgs e)
    {
        if (hdnfldCommandName.Value == "Add")  // ADD
        {
            // Create CRMAccount Object/Records.
            CRMLocations addcrmlocation = null;
            addcrmlocation = new CRMLocations();
            addcrmlocation.CRMAccountID = Convert.ToInt32(Request.QueryString["ID"].ToString());
            // Billing Location Information.
            addcrmlocation.BillingCompanyName = txtBillingCompanyName.Text.Trim().Replace("'", "''");
            if (ddlBillingPrefix.SelectedValue != "") addcrmlocation.BillingNamePrefix = ddlBillingPrefix.SelectedValue;
            else addcrmlocation.BillingNamePrefix = null;
            addcrmlocation.BillingFirstName = txtBillingFirstName.Text.Trim().Replace("'", "''");
            addcrmlocation.BillingLastName = txtBillingLastName.Text.Trim().Replace("'", "''");
            addcrmlocation.BillingAddress1 = txtBillingAddressLine1.Text.Trim().Replace("'", "''");
            if (txtBillingAddressLine2.Text != "") addcrmlocation.BillingAddress2 = txtBillingAddressLine2.Text.Trim().Replace("'", "''");
            else addcrmlocation.BillingAddress2 = null;
            if (txtBillingAddressLine3.Text != "") addcrmlocation.BillingAddress3 = txtBillingAddressLine3.Text.Trim().Replace("'", "''");
            else addcrmlocation.BillingAddress3 = null;
            addcrmlocation.BillingCountryID = Convert.ToInt32(ddlBillingCountry.SelectedValue);
            addcrmlocation.BillingCity = txtBillingCity.Text.Trim().Replace("'", "''");
            addcrmlocation.BillingStateID = Convert.ToInt32(ddlBillingState.SelectedValue);
            addcrmlocation.BillingPostalCode = txtBillingPostalCode.Text.Trim();
            addcrmlocation.BillingTelephone = txtBillingTelephone.Text.Trim();
            if (txtBillingFax.Text != "") addcrmlocation.BillingFax = txtBillingFax.Text.Trim();
            else addcrmlocation.BillingFax = null;
            addcrmlocation.BillingEmailAddress = txtBillingEmailAddress.Text;
            // Shipping Location Information.
            addcrmlocation.ShippingCompanyName = txtShippingCompanyName.Text.Trim().Replace("'", "''");
            if (ddlShippingPrefix.SelectedValue != "") addcrmlocation.ShippingNamePrefix = ddlShippingPrefix.SelectedValue;
            else addcrmlocation.ShippingNamePrefix = null;
            addcrmlocation.ShippingFirstName = txtShippingFirstName.Text.Trim().Replace("'", "''");
            addcrmlocation.ShippingLastName = txtShippingLastName.Text.Trim().Replace("'", "''");
            addcrmlocation.ShippingAddress1 = txtShippingAddressLine1.Text.Trim().Replace("'", "''");
            if (txtShippingAddressLine2.Text != "") addcrmlocation.ShippingAddress2 = txtShippingAddressLine2.Text.Trim().Replace("'", "''");
            else addcrmlocation.ShippingAddress2 = null;
            if (txtShippingAddressLine3.Text != "") addcrmlocation.ShippingAddress3 = txtShippingAddressLine3.Text.Trim().Replace("'", "''");
            else addcrmlocation.ShippingAddress3 = null;
            addcrmlocation.ShippingCountryID = Convert.ToInt32(ddlShippingCountry.SelectedValue);
            addcrmlocation.ShippingCity = txtShippingCity.Text.Trim().Replace("'", "''");
            addcrmlocation.ShippingStateID = Convert.ToInt32(ddlShippingState.SelectedValue);
            addcrmlocation.ShippingPostalCode = txtShippingPostalCode.Text.Trim();
            addcrmlocation.ShippingTelephone = txtShippingTelephone.Text.Trim();
            if (txtShippingFax.Text != "") addcrmlocation.ShippingFax = txtShippingFax.Text.Trim();
            else addcrmlocation.ShippingFax = null;
            if (txtShippingEmailAddress.Text != "") addcrmlocation.ShippingEmailAddress = txtShippingEmailAddress.Text;
            else addcrmlocation.ShippingEmailAddress = null;
            addcrmlocation.Active = true;

            idc.CRMLocations.InsertOnSubmit(addcrmlocation);

            try
            {
                // Insert record into DB.
                idc.SubmitChanges();
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divLocationInformationForm')", true);
            }
            catch (Exception ex)
            {
                this.VisibleModalErrorMessage(ex.ToString());
                return;
            }

            GetLocations(crmAccountID);
        }
        else if (hdnfldCommandName.Value == "Edit") // EDIT
        {
            var editcrmlocation = (from crmloc in idc.CRMLocations
                                   where crmloc.CRMLocationID == Convert.ToInt32(hdnfldCRMLocationID.Value)
                                   select crmloc).FirstOrDefault();

            if (editcrmlocation == null) return;

            // Billing Location Information.
            editcrmlocation.BillingCompanyName = txtBillingCompanyName.Text.Trim().Replace("'", "''");
            if (ddlBillingPrefix.SelectedValue != "") editcrmlocation.BillingNamePrefix = ddlBillingPrefix.SelectedValue;
            else editcrmlocation.BillingNamePrefix = null;
            editcrmlocation.BillingFirstName = txtBillingFirstName.Text.Trim().Replace("'", "''");
            editcrmlocation.BillingLastName = txtBillingLastName.Text.Trim().Replace("'", "''");
            editcrmlocation.BillingAddress1 = txtBillingAddressLine1.Text.Trim().Replace("'", "''");
            if (txtBillingAddressLine2.Text != "") editcrmlocation.BillingAddress2 = txtBillingAddressLine2.Text.Trim().Replace("'", "''");
            else editcrmlocation.BillingAddress2 = null;
            if (txtBillingAddressLine3.Text != "") editcrmlocation.BillingAddress3 = txtBillingAddressLine3.Text.Trim().Replace("'", "''");
            else editcrmlocation.BillingAddress3 = null;
            editcrmlocation.BillingCountryID = Convert.ToInt32(ddlBillingCountry.SelectedValue);
            editcrmlocation.BillingCity = txtBillingCity.Text.Trim().Replace("'", "''");
            editcrmlocation.BillingStateID = Convert.ToInt32(ddlBillingState.SelectedValue);
            editcrmlocation.BillingPostalCode = txtBillingPostalCode.Text.Trim();
            editcrmlocation.BillingTelephone = txtBillingTelephone.Text.Trim();
            if (txtBillingFax.Text != "") editcrmlocation.BillingFax = txtBillingFax.Text.Trim();
            else editcrmlocation.BillingFax = null;
            editcrmlocation.BillingEmailAddress = txtBillingEmailAddress.Text;
            // Shipping Location Information.
            editcrmlocation.ShippingCompanyName = txtShippingCompanyName.Text.Trim().Replace("'", "''");
            if (ddlShippingPrefix.SelectedValue != "") editcrmlocation.ShippingNamePrefix = ddlShippingPrefix.SelectedValue;
            else editcrmlocation.ShippingNamePrefix = null;
            editcrmlocation.ShippingFirstName = txtShippingFirstName.Text.Trim().Replace("'", "''");
            editcrmlocation.ShippingLastName = txtShippingLastName.Text.Trim().Replace("'", "''");
            editcrmlocation.ShippingAddress1 = txtShippingAddressLine1.Text.Trim().Replace("'", "''");
            if (txtShippingAddressLine2.Text != "") editcrmlocation.ShippingAddress2 = txtShippingAddressLine2.Text.Trim().Replace("'", "''");
            else editcrmlocation.ShippingAddress2 = null;
            if (txtShippingAddressLine3.Text != "") editcrmlocation.ShippingAddress3 = txtShippingAddressLine3.Text.Trim().Replace("'", "''");
            else editcrmlocation.ShippingAddress3 = null;
            editcrmlocation.ShippingCountryID = Convert.ToInt32(ddlShippingCountry.SelectedValue);
            editcrmlocation.ShippingCity = txtShippingCity.Text.Trim().Replace("'", "''");
            editcrmlocation.ShippingStateID = Convert.ToInt32(ddlShippingState.SelectedValue);
            editcrmlocation.ShippingPostalCode = txtShippingPostalCode.Text.Trim();
            editcrmlocation.ShippingTelephone = txtShippingTelephone.Text.Trim();
            if (txtShippingFax.Text != "") editcrmlocation.ShippingFax = txtShippingFax.Text.Trim();
            else editcrmlocation.ShippingFax = null;
            if (txtShippingEmailAddress.Text != "") editcrmlocation.ShippingEmailAddress = txtShippingEmailAddress.Text;
            else editcrmlocation.ShippingEmailAddress = null;

            try
            {
                // Update record into DB.
                idc.SubmitChanges();
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divLocationInformationForm')", true);
            }
            catch (Exception ex)
            {
                this.VisibleModalErrorMessage(ex.ToString());
                return;
            }

            GetLocations(crmAccountID);
        }
    }
    #endregion

    protected void lnkbtnDeleteLocation_Click(object sender, EventArgs e)
    {
        LinkButton lnkbtn = (LinkButton)sender;
        int crmLocationID = Convert.ToInt32(lnkbtn.CommandArgument);

        var crmLocation = (from crmloc in idc.CRMLocations
                           where crmloc.CRMLocationID == crmLocationID
                           select crmloc).FirstOrDefault();

        if (crmLocation == null) return;

        idc.CRMLocations.DeleteOnSubmit(crmLocation);

        try
        {
            idc.SubmitChanges();
            GetLocations(crmAccountID);
        }
        catch (Exception ex)
        {
            this.VisibleMainFormErrorMessage(ex.ToString());
            return;
        }
    }

    protected void lnkbtnEditLocation_Click(object sender, EventArgs e)
    {
        LinkButton lnkbtn = (LinkButton)sender;
        int crmLocationID = Convert.ToInt32(lnkbtn.CommandArgument);

        var crmLocation = (from crmloc in idc.CRMLocations
                           where crmloc.CRMLocationID == crmLocationID
                           select crmloc).FirstOrDefault();

        if (crmLocation == null) return;

        commandName = "Edit";
        hdnfldCommandName.Value = commandName;
        hdnfldCRMLocationID.Value = crmLocationID.ToString();

        PopulateAddEditLocationCountryDDLs();
        int countryIDBilling;
        int countryIDShipping;
        int.TryParse(crmLocation.BillingCountryID.ToString(), out countryIDBilling);
        int.TryParse(crmLocation.ShippingCountryID.ToString(), out countryIDShipping);
        PopulateAddEditLocationBillingStateDDL(countryIDBilling);
        PopulateAddEditLocationShippingStateDDL(countryIDShipping);

        // Billing Location Information.
        txtBillingCompanyName.Text = crmLocation.BillingCompanyName;
        if (crmLocation.BillingNamePrefix != null) ddlBillingPrefix.SelectedValue = crmLocation.BillingNamePrefix.Trim();
        else ddlBillingPrefix.SelectedValue = "";
        txtBillingFirstName.Text = crmLocation.BillingFirstName;
        txtBillingLastName.Text = crmLocation.BillingLastName;
        txtBillingAddressLine1.Text = crmLocation.BillingAddress1;
        if (crmLocation.BillingAddress2 != null) txtBillingAddressLine2.Text = crmLocation.BillingAddress2;
        else txtBillingAddressLine2.Text = "";
        if (crmLocation.BillingAddress3 != null) txtBillingAddressLine3.Text = crmLocation.BillingAddress3;
        else txtBillingAddressLine3.Text = "";
        ddlBillingCountry.SelectedValue = crmLocation.BillingCountryID.ToString();
        txtBillingCity.Text = crmLocation.BillingCity;
        ddlBillingState.SelectedValue = crmLocation.BillingStateID.ToString();
        txtBillingPostalCode.Text = crmLocation.BillingPostalCode;
        txtBillingTelephone.Text = crmLocation.BillingTelephone;
        if (crmLocation.BillingFax != null) txtBillingFax.Text = crmLocation.BillingFax;
        else txtBillingFax.Text = "";
        txtBillingEmailAddress.Text = crmLocation.BillingEmailAddress;
        // Shipping Location Information.
        txtShippingCompanyName.Text = crmLocation.ShippingCompanyName;
        if (crmLocation.ShippingNamePrefix != null) ddlShippingPrefix.SelectedValue = crmLocation.ShippingNamePrefix.Trim();
        else ddlShippingPrefix.SelectedValue = "";
        txtShippingFirstName.Text = crmLocation.ShippingFirstName;
        txtShippingLastName.Text = crmLocation.ShippingLastName;
        txtShippingAddressLine1.Text = crmLocation.ShippingAddress1;
        if (crmLocation.ShippingAddress2 != null) txtShippingAddressLine2.Text = crmLocation.ShippingAddress2;
        else txtShippingAddressLine2.Text = "";
        if (crmLocation.ShippingAddress3 != null) txtShippingAddressLine3.Text = crmLocation.ShippingAddress3;
        else txtShippingAddressLine3.Text = "";
        ddlShippingCountry.SelectedValue = crmLocation.ShippingCountryID.ToString();
        txtShippingCity.Text = crmLocation.ShippingCity;
        ddlShippingState.SelectedValue = crmLocation.ShippingStateID.ToString();
        txtShippingPostalCode.Text = crmLocation.ShippingPostalCode;
        txtShippingTelephone.Text = crmLocation.ShippingTelephone;
        if (crmLocation.ShippingFax != null) txtShippingFax.Text = crmLocation.ShippingFax;
        else txtShippingFax.Text = "";
        if (crmLocation.ShippingEmailAddress != null) txtShippingEmailAddress.Text = crmLocation.ShippingEmailAddress;
        else txtShippingEmailAddress.Text = "";

        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('divLocationInformationForm')", true);
    }

    /// <summary>
    /// Populate DDL for Add Product Group IDs based on Products table.
    /// </summary>
    protected void PopulateAddEditOrderProductGroupIDDDL()
    {
        var productsgroups = (from p in idc.Products
                              orderby p.ProductName ascending
                              select new
                              {
                                  ProductGroupID = p.ProductGroupID,
                                  ProductName = p.ProductName
                              }).Distinct();

        // Product ID(s) DDL.
        this.ddlProductGroupID.DataSource = productsgroups;
        this.ddlProductGroupID.DataTextField = "ProductName";
        this.ddlProductGroupID.DataValueField = "ProductGroupID";
        this.ddlProductGroupID.DataBind();

        this.ddlProductGroupID.SelectedIndex = 0;
    }

    /// <summary>
    /// Populate DDL for Add Product IDs based on Product Group ID.
    /// </summary>
    /// <param name="countryid"></param>
    private void PopulateAddEditOrderProductIDDDL(int productgroupid)
    {
        this.ddlProductID.Items.Clear();

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
        this.ddlProductID.DataSource = products;
        this.ddlProductID.DataTextField = "ProductDescription";
        this.ddlProductID.DataValueField = "ProductID";
        this.ddlProductID.DataBind();

        this.ddlProductID.Items.Insert(0, new ListItem("-Select-", "0"));
    }

    protected void lnkbtnAddOrder_Click(object sender, EventArgs e)
    {
        commandName = this.lnkbtnAddOrder.CommandName.ToString();
        hdnfldOrderCommandName.Value = commandName;
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('divOrderInformationForm')", true);
    }

    protected void ddlProductGroupID_SelectedIndexChanged(object sender, EventArgs e)
    {
        int productGroupID = 0;

        int.TryParse(this.ddlProductGroupID.SelectedValue, out productGroupID);

        if (productGroupID == 0) return;

        PopulateAddEditOrderProductIDDDL(productGroupID);
    }

    protected void lnkbtnDeleteOrder_Click(object sender, EventArgs e)
    {
        LinkButton lnkbtn = (LinkButton)sender;
        int crmOrderID = Convert.ToInt32(lnkbtn.CommandArgument);

        var crmOrder = (from crmord in idc.CRMOrders
                        where crmord.CRMOrderID == crmOrderID
                        select crmord).FirstOrDefault();

        if (crmOrder == null) return;

        idc.CRMOrders.DeleteOnSubmit(crmOrder);

        try
        {
            idc.SubmitChanges();
            GetOrders(crmAccountID);
        }
        catch (Exception ex)
        {
            this.VisibleMainFormErrorMessage(ex.ToString());
            return;
        }
    }

    protected void lnkbtnEditOrder_Click(object sender, EventArgs e)
    {
        LinkButton lnkbtn = (LinkButton)sender;
        int crmOrderID = Convert.ToInt32(lnkbtn.CommandArgument);

        var crmOrder = (from crmord in idc.CRMOrders
                        where crmord.CRMOrderID == crmOrderID
                        select crmord).FirstOrDefault();

        if (crmOrder == null) return;

        commandName = "Edit";
        hdnfldOrderCommandName.Value = commandName;
        hdnfldCRMOrderID.Value = crmOrderID.ToString();

        int productGroupID = 0;
        PopulateAddEditOrderProductGroupIDDDL();
        int.TryParse(crmOrder.ProductGroupID.ToString(), out productGroupID);
        PopulateAddEditOrderProductIDDDL(productGroupID);

        ddlProductGroupID.SelectedValue = crmOrder.ProductGroupID.ToString();
        ddlProductID.SelectedValue = crmOrder.ProductID.ToString();
        txtQuantity.Text = crmOrder.Quantity.ToString();
        txtUnitPrice.Text = crmOrder.UnitPrice.ToString();

        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('divOrderInformationForm')", true);
    }

    protected void btnSave_Order_Click(object sender, EventArgs e)
    {
        if (hdnfldOrderCommandName.Value == "Add")  // ADD
        {
            // Create CRMAccount Object/Records.
            CRMOrders addcrmOrder = null;
            addcrmOrder = new CRMOrders();
            addcrmOrder.CRMAccountID = Convert.ToInt32(Request.QueryString["ID"].ToString());
            addcrmOrder.ProductGroupID = Convert.ToInt32(ddlProductGroupID.SelectedValue);
            addcrmOrder.ProductID = Convert.ToInt32(ddlProductID.SelectedValue);
            var colorandsku = (from p in idc.Products
                               where p.ProductID == Convert.ToInt32(ddlProductID.SelectedValue)
                               select p).FirstOrDefault();
            addcrmOrder.Color = colorandsku.Color;
            addcrmOrder.ProductSKU = colorandsku.ProductSKU;
            addcrmOrder.Quantity = Convert.ToInt32(txtQuantity.Text);
            addcrmOrder.UnitPrice = Convert.ToDecimal(txtUnitPrice.Text);
            addcrmOrder.Active = true;

            idc.CRMOrders.InsertOnSubmit(addcrmOrder);

            try
            {
                // Insert record into DB.
                idc.SubmitChanges();
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divOrderInformationForm')", true);
            }
            catch (Exception ex)
            {
                this.VisibleModalErrorMessage(ex.ToString());
                return;
            }

            GetOrders(crmAccountID);
        }
        else if (hdnfldCommandName.Value == "Edit") // EDIT
        {
            var editcrmorder = (from crmord in idc.CRMOrders
                                where crmord.CRMOrderID == Convert.ToInt32(hdnfldCRMOrderID.Value)
                                select crmord).FirstOrDefault();

            if (editcrmorder == null) return;

            editcrmorder.ProductGroupID = Convert.ToInt32(ddlProductGroupID.SelectedValue);
            editcrmorder.ProductID = Convert.ToInt32(ddlProductID.SelectedValue);
            var colorandsku = (from p in idc.Products
                               where p.ProductID == Convert.ToInt32(ddlProductID.SelectedValue)
                               select p).FirstOrDefault();
            editcrmorder.Color = colorandsku.Color;
            editcrmorder.ProductSKU = colorandsku.ProductSKU;
            editcrmorder.Quantity = Convert.ToInt32(txtQuantity.Text);
            editcrmorder.UnitPrice = Convert.ToDecimal(txtUnitPrice.Text);

            try
            {
                // Update record into DB.
                idc.SubmitChanges();
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divLocationInformationForm')", true);
            }
            catch (Exception ex)
            {
                this.VisibleModalErrorMessage(ex.ToString());
                return;
            }

            GetOrders(crmAccountID);
        }
    }

    protected void btnEditCRMAccount_Click(object sender, EventArgs e)
    {
        DateTime serviceStartDate = DateTime.Now;
        DateTime serviceEndDate = DateTime.Now;
        DateTime.TryParse(txtServiceStartDate.Text, out serviceStartDate);
        DateTime.TryParse(txtServiceEndDate.Text, out serviceEndDate);

        // Create CRMAccount Object/Records.
        var crmacct = (from acct in idc.CRMAccounts
                       where acct.CRMAccountID == Convert.ToInt32(Request.QueryString["ID"].ToString())
                       select acct).FirstOrDefault();
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

        try
        {
            // Insert record into DB.
            idc.SubmitChanges();

            if (Page.IsValid)
            {
                Response.Redirect("CRMAccounts.aspx");
            }
        }
        catch (Exception ex)
        {
            this.VisibleMainFormErrorMessage(ex.ToString());
            return;
        }
    }

    protected void btnCancelCRMAccount_Click(object sender, EventArgs e)
    {
        Response.Redirect("CRMAccounts.aspx");
    }

    private void VisibleModalErrorMessage(string errormessage)
    {

    }

    private void VisibleMainFormErrorMessage(string errormessage)
    {

    }
}