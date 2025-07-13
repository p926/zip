using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Instadose.Data;
using Telerik.Web.UI;

public partial class CustomerService_CRMAccounts : System.Web.UI.Page
{
    InsDataContext idc = new InsDataContext();

    string UserName = "Unknown";

    int crmAccountID = 0;

    /// <summary>
    /// Limits access to the functionality based on groups.
    /// </summary>
    string[] activeDirecoryGroups = { "IRV-IT", "IRV-CustomerSvc", "IRV-Client Services" };

    #region ON PAGE LOAD
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Page.IsPostBack) return;

        this.hdnfldAccordionIndex.Value = "0";
        InvisibleError();
        InvisibleSuccess();
        ResetModalControls();
    }
    #endregion

    #region Toolbar Functions.
    /// <summary>
    /// Takes User to Add CRM Account form.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void lnkbtnAddCRMAccount_Click(object sender, EventArgs e)
    {
        Response.Redirect("AddCRMAccounts.aspx");
    }

    /// <summary>
    /// Clear all Filters in RadGrid (resets RadGrid, all records displayed).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void lnkbtnClearFilters_Click(object sender, EventArgs e)
    {
        foreach (GridColumn column in rgCRMAccounts.MasterTableView.OwnerGrid.Columns)
        {
            column.CurrentFilterFunction = GridKnownFunction.NoFilter;
            column.CurrentFilterValue = string.Empty;
        }
        this.rgCRMAccounts.MasterTableView.FilterExpression = string.Empty;
        this.rgCRMAccounts.Rebind();
    }
    #endregion

    #region Basic RadGrid Functions.
    protected void rgCRMAccounts_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
    {
        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        //// Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.Text;

        // Using the vw_CreditCardExpirationsListing.
        string sqlQuery = "SELECT * FROM [vw_CRMAccountsInformation]";

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader dreader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable dtCRMAccountsInformation = new DataTable();

        dtCRMAccountsInformation = new DataTable("CRM Accounts Information");

        // Create the columns for the DataTable.
        dtCRMAccountsInformation.Columns.Add("CRMAccountID", Type.GetType("System.Int32"));
        dtCRMAccountsInformation.Columns.Add("CreatedDate", Type.GetType("System.DateTime"));
        dtCRMAccountsInformation.Columns.Add("BrandName", Type.GetType("System.String"));
        dtCRMAccountsInformation.Columns.Add("AccountName", Type.GetType("System.String"));
        dtCRMAccountsInformation.Columns.Add("CompanyName", Type.GetType("System.String"));
        dtCRMAccountsInformation.Columns.Add("SRDCompanyName", Type.GetType("System.String"));
        dtCRMAccountsInformation.Columns.Add("IndustryName", Type.GetType("System.String"));
        dtCRMAccountsInformation.Columns.Add("CRMACCTFullName", Type.GetType("System.String"));
        dtCRMAccountsInformation.Columns.Add("LocationCount", Type.GetType("System.Int32"));
        dtCRMAccountsInformation.Columns.Add("OrderCount", Type.GetType("System.Int32"));
        dtCRMAccountsInformation.Columns.Add("CRMACCTStatus", Type.GetType("System.String"));
        dtCRMAccountsInformation.Columns.Add("Active", Type.GetType("System.Boolean"));

        while (dreader.Read())
        {
            DataRow drow = dtCRMAccountsInformation.NewRow();

            // Fill row details.
            drow["CRMAccountID"] = dreader["CRMAccountID"];
            drow["CreatedDate"] = dreader["CreatedDate"];
            drow["BrandName"] = dreader["BrandName"];
            drow["AccountName"] = dreader["AccountName"];
            drow["CompanyName"] = dreader["CompanyName"];
            drow["SRDCompanyName"] = dreader["SRDCompanyName"];
            drow["IndustryName"] = dreader["IndustryName"];
            drow["CRMACCTFullName"] = dreader["CRMACCTFullName"];
            drow["LocationCount"] = dreader["LocationCount"];
            drow["OrderCount"] = dreader["OrderCount"];
            drow["CRMACCTStatus"] = dreader["CRMACCTStatus"];
            drow["Active"] = dreader["Active"];

            // Add rows to DataTable.
            dtCRMAccountsInformation.Rows.Add(drow);
        }

        this.rgCRMAccounts.DataSource = dtCRMAccountsInformation;
    }

    /// <summary>
    /// When the Review button is clicked, the Approve/Decline CRM Account Modal displays.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void rgCRMAccounts_ItemCommand(object sender, Telerik.Web.UI.GridCommandEventArgs e)
    {
        // Must have the ReviewCRMAccount CommandName to continue.
        if (e.CommandName != "ReviewCRMAccount") return;

        // Parse through the contents of the CommandArgument to get the RateID.
        int.TryParse(e.CommandArgument.ToString(), out crmAccountID);

        if (crmAccountID == 0) return;

        this.hdnfldCRMAccountID.Value = Convert.ToString(crmAccountID);

        CRMAccounts crmacct = (from crma in idc.CRMAccounts
                               where crma.CRMAccountID == crmAccountID
                               && crma.Active == true
                               select crma).FirstOrDefault();

        if (crmacct == null) return;

        int brandSourceID = crmacct.BrandSourceID;
        // Get the image
        switch (brandSourceID)
        {
            case 2:
                this.imgBrandLogo.ImageUrl = "../images/logos/mirion.png";
                break;
            case 3:
                this.imgBrandLogo.ImageUrl = "../images/logos/iccare.png";
                break;
            default:
                this.imgBrandLogo.ImageUrl = "../images/logos/mirion.png";
                break;
        }

        // Bind DB values to Modal/Dialog controls.
        // Format Dealer Name.
        int? dealerID = crmacct.DealerID;
        string dealerName = "";
        if (dealerID != null)
        {

            dealerName = (from d in idc.Dealers
                          where d.DealerID == crmacct.DealerID
                          select d.DealerName).FirstOrDefault();

            this.lblDealerName.Text = dealerName;
            this.lblDealerName.Font.Italic = false;
            this.lblDealerName.ForeColor = System.Drawing.Color.Black;
        }
        else
        {
            this.lblDealerName.Text = "(none provided)";
            this.lblDealerName.Font.Italic = true;
            this.lblDealerName.ForeColor = System.Drawing.Color.Gray;
        }

        this.lblCompanyName.Text = crmacct.CompanyName;
        this.lblAccountName.Text = crmacct.AccountName;
        this.lblReferral.Text = crmacct.SalesRepDistID;
        this.lblIndustryType.Text = crmacct.Industry.IndustryName;
        this.lblCustomerType.Text = crmacct.CustomerType.CustomerTypeDesc + " " + crmacct.CustomerType.CustomerTypeCode;

        // Format Unix Customer Type.
        int? unixCustomerTypeID = crmacct.UnixCustomerTypeID;
        string unixCustomerType = "";
        if (unixCustomerTypeID != null)
        {
            unixCustomerType = (from uct in idc.UnixCustomerTypes
                                where uct.UnixCustomerTypeID == crmacct.UnixCustomerTypeID
                                select uct.UnixCustomerDescription).FirstOrDefault();

            this.lblUnixCustomerType.Text = unixCustomerType;
            this.lblUnixCustomerType.Font.Italic = false;
        }
        else
        {
            this.lblUnixCustomerType.Text = "(none provided)";
            this.lblUnixCustomerType.Font.Italic = true;
            this.lblUnixCustomerType.ForeColor = System.Drawing.Color.Gray;
        }

        // Format Contract Period (ServiceStartDate to ServiceEndDate).
        string serviceStartDate = string.Format("{0:MM/dd/yyyy}", crmacct.ServiceStartDate);
        string serviceEndDate = string.Format("{0:MM/dd/yyyy}", crmacct.ServiceEndDate);
        this.lblContractPeriod.Text = serviceStartDate + "-" + serviceEndDate;



        this.lblBillingFrequency.Text = crmacct.BillingTerm.BillingTermDesc;
        this.lblBillingMethod.Text = crmacct.BillingMethod;

        string billingMethod = crmacct.BillingMethod;

        // Display either PO or CC information based on BillingMethod value.
        if (billingMethod == "Purchase Order")
        {
            this.divPONumber.Visible = true;
            this.lblPONumber.Text = crmacct.PONumber;
            this.divCreditCardInformation.Visible = false;
        }
        else
        {
            this.divPONumber.Visible = false;

            string creditCardName = (from cct in idc.CreditCardTypes
                                     where cct.CreditCardTypeID == crmacct.CreditCardTypeID
                                     select cct.CreditCardName).FirstOrDefault();

            string numberEncrypted = crmacct.NumberEncrypted;

            switch (creditCardName)
            {
                case "Visa":
                    this.imgCreditCardType.ImageUrl = "../images/ccvisa.gif";
                    this.imgCreditCardType.AlternateText = "Visa";
                    this.imgCreditCardType.Width = 30;
                    break;
                case "MasterCard":
                    this.imgCreditCardType.ImageUrl = "../images/ccmastercard.gif";
                    this.imgCreditCardType.AlternateText = "MasterCard";
                    this.imgCreditCardType.Width = 30;
                    break;
                case "Discover":
                    this.imgCreditCardType.ImageUrl = "../images/ccdiscover.gif";
                    this.imgCreditCardType.AlternateText = "Discover";
                    this.imgCreditCardType.Width = 30;
                    break;
                case "Amex":
                    this.imgCreditCardType.ImageUrl = "../images/ccamex.gif";
                    this.imgCreditCardType.AlternateText = "American Express";
                    this.imgCreditCardType.Width = 30;
                    break;
                default:
                    break;
            }

            this.lblCreditCardNumber.Text = Common.MaskCreditCardNumber(numberEncrypted, creditCardName);
            this.lblNameOnCard.Text = crmacct.NameOnCard;
            this.lblCVC.Text = crmacct.SecurityCode;
            this.lblExpirationDate.Text = crmacct.ExpMonth.ToString() + "/" + crmacct.ExpYear.ToString();
            this.divCreditCardInformation.Visible = true;
        }

        this.lblUsername.Text = crmacct.Username;
        this.lblSecurityQuestion.Text = crmacct.SecurityQuestion.SecurityQuestionText;
        this.lblSecurityAnswer.Text = crmacct.SecurityAnswer1;

        // Format Contact Name.
        string prefix = crmacct.Prefix;
        string fullName = crmacct.FirstName + " " + crmacct.LastName;
        if (prefix == null) this.lblContactName.Text = fullName;
        else this.lblContactName.Text = prefix + " " + fullName;

        // Format Gender.
        string gender = crmacct.Gender;
        if (gender == "M") this.lblGender.Text = "Male";
        else this.lblGender.Text = "Female";

        this.lblEmail.Text = crmacct.EmailAddress;
        this.lblTelephone.Text = crmacct.Telephone;

        // Format Fax.
        string fax = crmacct.Fax;
        if (fax != null)
        {
            this.lblFax.Text = crmacct.Fax;
            this.lblFax.Font.Italic = false;
        }
        else
        {
            this.lblFax.Text = "(none provided)";
            this.lblFax.Font.Italic = true;
            this.lblFax.ForeColor = System.Drawing.Color.Gray;
        }

        GetLocations(crmAccountID);
        GetOrders(crmAccountID);

        // Send JavaScript to client to open/display the modal.
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('divApproveDeclineCRMAccount')", true);
    }

    /// <summary>
    /// Displays Review Button or Status Text depending on DB value of (P, A, or D).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void rgCRMAccounts_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem dataItem = (GridDataItem)e.Item;
            // Create TableCell object for the following Statuses:
            //  1.) Pending (blue)
            //  2.) Approved (green)
            //  3.) Declined (red)
            TableCell status = dataItem["CRMACCTStatus"];
            Label lblStatus = dataItem.FindControl("lblStatus") as Label;
            Button btnReview = dataItem.FindControl("btnReview") as Button;

            // STATUS.
            if (status.Text.Contains("P"))
            {
                lblStatus.Visible = false;
                btnReview.Visible = true;
            }

            if (status.Text.Contains("A"))
            {
                lblStatus.Visible = true;
                lblStatus.Text = "Approved";
                lblStatus.ForeColor = System.Drawing.Color.Green;
                btnReview.Visible = false;
            }

            if (status.Text.Contains("D"))
            {
                lblStatus.Visible = true;
                lblStatus.Text = "Declined";
                lblStatus.ForeColor = System.Drawing.Color.Red;
                btnReview.Visible = false;
            }
        }
    }
    #endregion

    #region Location(s) and Order(s) GridViews.
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

        // Using the vw_CreditCardExpirationsListing.
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
            else if(billingAddressLine2 != "" && billingAddressLine3 == "")
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
                                         where c.CountryID == billingCountryID
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

        // Using the vw_CreditCardExpirationsListing.
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

    #region Review Modal/Dialog Functions.
    // Approved Button Functionality.
    protected void btnApprove_Click(object sender, EventArgs e)
    {
        // Will flag CRMAccount record as "A" (Approved).
        var crmacct = (from acct in idc.CRMAccounts
                       where acct.CRMAccountID == Convert.ToInt32(hdnfldCRMAccountID.Value)
                       select acct).FirstOrDefault();

        if (crmacct == null) return;

        crmacct.Status = "A";
        crmacct.StatusSetBy = UserName;
        crmacct.StatusSetDate = DateTime.Now;

        try
        {
            idc.SubmitChanges();
            rgCRMAccounts.DataBind();
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divApproveDeclineCRMAccount')", true);
        }
        catch (Exception ex)
        {
            this.VisibleError(ex.ToString());
            return;
        }
    }

    // Declined Button Functionality.
    protected void btnDecline_Click(object sender, EventArgs e)
    {
        // Will flag CRMAccount record as "D" (Declined).
        var crmacct = (from acct in idc.CRMAccounts
                       where acct.CRMAccountID == Convert.ToInt32(hdnfldCRMAccountID.Value)
                       select acct).FirstOrDefault();

        if (crmacct == null) return;

        crmacct.Status = "D";
        crmacct.StatusSetBy = UserName;
        crmacct.StatusSetDate = DateTime.Now;

        try
        {
            idc.SubmitChanges();
            rgCRMAccounts.DataBind();
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divApproveDeclineCRMAccount')", true);
        }
        catch (Exception ex)
        {
            this.VisibleError(ex.ToString());
            return;
        }
    }

    // Edit Button Functionality.Redirect to EditCRMAccount.aspx
    protected void btnEdit_Click(object sender, EventArgs e)
    {
        Response.Redirect(string.Format("EditCRMAccounts.aspx?ID={0}", hdnfldCRMAccountID.Value));
    }

    protected void btnCancel_Click(object sender, EventArgs e)
    {
        ResetModalControls();
    }
    #endregion

    #region Reset Modal/Dialog Controls.
    private void ResetModalControls()
    {
        this.hdnfldAccordionIndex.Value = "0";
        InvisibleError();
        InvisibleSuccess();
        this.imgBrandLogo.ImageUrl = "~/images/logos/mirion.png";
        this.lblDealerName.Text = string.Empty;
        this.lblAccountName.Text = string.Empty;
        this.lblCompanyName.Text = string.Empty;
        this.lblReferral.Text = string.Empty;
        this.lblIndustryType.Text = string.Empty;
        this.lblCustomerType.Text = string.Empty;
        this.lblUnixCustomerType.Text = string.Empty;
        this.lblContractPeriod.Text = string.Empty;
        this.lblBillingFrequency.Text = string.Empty;
        this.lblBillingMethod.Text = string.Empty;
        this.lblPONumber.Text = string.Empty;
        this.imgCreditCardType.ImageUrl = "~/images/ccvisa.gif";
        this.lblNameOnCard.Text = string.Empty;
        this.lblCreditCardNumber.Text = string.Empty;
        this.lblExpirationDate.Text = string.Empty;
        this.lblCVC.Text = string.Empty;
        this.lblUsername.Text = string.Empty;
        this.lblSecurityQuestion.Text = string.Empty;
        this.lblSecurityAnswer.Text = string.Empty;
        this.lblContactName.Text = string.Empty;
        this.lblGender.Text = string.Empty;
        this.lblEmail.Text = string.Empty;
        this.lblTelephone.Text = string.Empty;
        this.lblFax.Text = string.Empty;
    }
    #endregion

    #region Error/Success Message(s).
    private void InvisibleError()
    {
        this.spnFormErrorMessage.InnerText = "";
        this.divFormError.Visible = false;
    }

    private void VisibleError(string errormessage)
    {
        this.spnFormErrorMessage.InnerText = errormessage;
        this.divFormError.Visible = true;
    }

    private void InvisibleSuccess()
    {
        this.spnFormSuccessMessage.InnerText = "";
        this.divFormSuccess.Visible = false;
    }

    private void VisibleSuccess(string successmessage)
    {
        this.spnFormSuccessMessage.InnerText = successmessage;
        this.divFormSuccess.Visible = true;
    }
    #endregion
}