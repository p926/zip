using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Instadose.Data;
using System.Drawing;
using Mirion.DSD.GDS.API;
using System.Text.RegularExpressions;

public partial class Reports : System.Web.UI.Page
{
    // Create the database LINQ object(s).
    private InsDataContext idc = new InsDataContext();

    #region CREDIT CARD EXPIRATIONS LIST.
    private List<CCEListItem> CCEItemList;

    [Serializable()]
    public class CCEList
    {
        public CCEList()
        {
        }

        /// <summary>
        /// Collection of Credit Card Expirations List Items.
        /// </summary>
        public List<CCEListItem> CreditCardExpirations { get; set; }
    }

    [Serializable()]
    public class CCEListItem
    {
        public CCEListItem()
        {
        }

        /// <summary>
        /// Identifier of the Account.
        /// </summary>
        public string AccountID { get; set; }
        public string CompanyName { get; set; }
        public string CreditCardName { get; set; }
        public string NumberEncrypted { get; set; }
        public DateTime DT_ExpirationDate { get; set; }
        public string TypeOfAccount { get; set; }
    }
    #endregion

    #region DAILY BATCHES LIST.
    private List<DBListItem> DBItemList;

    [Serializable()]
    public class DBList
    {
        public DBList()
        {
        }

        /// <summary>
        /// Collection of Daily Batches List Items.
        /// </summary>
        public List<DBListItem> DailyBatches { get; set; }
    }

    [Serializable()]
    public class DBListItem
    {
        public DBListItem()
        {
        }

        /// <summary>
        /// Identifier of the Payment.
        /// </summary>
        public int PaymentID { get; set; }
        public string AccountID { get; set; }
        public DateTime DateOfPayment { get; set; }
        public string Amount { get; set; }
        public string CurrencyCode { get; set; }
        public string TransactionCode { get; set; }
        public int OrderID { get; set; }
        public string InvoiceNumber { get; set; }
        public string CompanyName { get; set; }
        public string CreditCardName { get; set; }
        public string NumberEncrypted { get; set; }
        public string ExpirationDate { get; set; }
        public string TypeOfAccount { get; set; }
        public string CreditCardInformation { get; set; }
    }
    #endregion

    #region ON PAGE LOAD.
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Page.IsPostBack) return;

        this.divCreditCardExpirations.Visible = false;
        this.divDailyBatches.Visible = false;

        this.updtpnlCreditCardExpirations.Visible = false;
        this.updtpnlDailyBatches.Visible = false;

        // Make sure both GridViews are NULL/Clear.
        this.gvCreditCardExpirationsListing.DataSource = null;
        this.gvCreditCardExpirationsListing.DataBind();
        this.gvDailyBatchesListing.DataSource = null;
        this.gvDailyBatchesListing.DataBind();
    }
    #endregion

    #region GENERAL FUNCTIONS.
    /// <summary>
    /// Select the Type of Report to view.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void ddlTypeOfReport_SelectedIndexChanged(object sender, EventArgs e)
    {
        string selectedReport = this.ddlTypeOfReport.SelectedValue;

        if (selectedReport == "CCE")
        {
            this.divCreditCardExpirations.Visible = true;
            this.updtpnlCreditCardExpirations.Visible = true;
            this.divDailyBatches.Visible = false;
            this.updtpnlDailyBatches.Visible = false;
        }
        else if (selectedReport == "DB")
        {
            this.divCreditCardExpirations.Visible = false;
            this.updtpnlCreditCardExpirations.Visible = false;
            this.divDailyBatches.Visible = true;
            this.updtpnlDailyBatches.Visible = true;
        }
        else
        {
            this.divCreditCardExpirations.Visible = false;
            this.updtpnlCreditCardExpirations.Visible = false;
            this.divDailyBatches.Visible = false;
            this.updtpnlDailyBatches.Visible = false;
        }

        // Reset all Form Controls.
        // Credit Card Expirations.
        this.ddlTypeOfAccount_CCE.SelectedIndex = 0;
        this.txtAccountID_CCE.Text = string.Empty;
        this.txtCompanyName_CCE.Text = string.Empty;
        this.ddlTypeOfCreditCard.SelectedIndex = 0;
        this.txtFromExpirationDate.Text = string.Empty;
        this.gvCreditCardExpirationsListing.DataSource = null;
        this.gvCreditCardExpirationsListing.DataBind();

        // Daily Batches.
        this.ddlTypeOfAccount_DB.SelectedIndex = 0;
        this.txtAccountID_DB.Text = string.Empty;
        this.txtCompanyName_DB.Text = string.Empty;
        this.divOrderID.Visible = true;
        this.txtOrderID.Text = string.Empty;
        this.txtInvoiceNumber.Text = string.Empty;
        this.txtFromPaymentDate.Text = string.Empty;
        this.txtToPaymentDate.Text = string.Empty;
        this.gvDailyBatchesListing.DataSource = null;
        this.gvDailyBatchesListing.DataBind();
    }

    /// <summary>
    /// Reset Error Message.
    /// </summary>
    private void InvisibleError()
    {
        // Reset submission Form Error Message.
        this.spnError.InnerText = string.Empty;
        this.divError.Visible = false;
    }

    /// <summary>
    /// Set Error Message.
    /// </summary>
    /// <param name="errorMsg"></param>
    private void VisibleError(string errormessage)
    {
        this.spnError.InnerText = errormessage;
        this.divError.Visible = true;
    }

    protected void ddlTypeOfAccount_CCE_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (this.ddlTypeOfAccount_CCE.SelectedValue == "Instadose")
        {
            this.divCompanyName.Visible = true;
        }
        else
        {
            this.divCompanyName.Visible = false;
        }

        // Clear Credit Card Expirations form controls.
        this.txtAccountID_CCE.Text = string.Empty;
        this.txtCompanyName_CCE.Text = string.Empty;
        this.ddlTypeOfCreditCard.SelectedIndex = 0;
        this.txtFromExpirationDate.Text = string.Empty;
        this.txtToExpirationDate.Text = string.Empty;

        // Clear out the Credit Card Expirations GridView (only).
        this.gvCreditCardExpirationsListing.DataSource = null;
        this.gvCreditCardExpirationsListing.DataBind();
    }

    protected void ddlTypeOfAccount_DB_SelectedIndexChanged(object sender, EventArgs e)
    {
        int index = GetGVColumnIndexByColumnName(this.gvDailyBatchesListing, "Order #");

        if (this.ddlTypeOfAccount_DB.SelectedValue == "Instadose")
        {
            this.divOrderID.Visible = true;
            this.gvDailyBatchesListing.Columns[index].Visible = true;
        }
        else
        {
            this.divOrderID.Visible = false;
            this.gvDailyBatchesListing.Columns[index].Visible = false;
        }

        // Clear Daily Batches form controls.
        this.txtAccountID_DB.Text = string.Empty;
        this.txtCompanyName_DB.Text = string.Empty;
        this.txtOrderID.Text = string.Empty;
        this.txtInvoiceNumber.Text = string.Empty;
        this.txtFromPaymentDate.Text = string.Empty;
        this.txtToPaymentDate.Text = string.Empty;

        // Clear out the Daily Batches GridView (only).
        this.gvDailyBatchesListing.DataSource = null;
        this.gvDailyBatchesListing.DataBind();
    }

    /// <summary>
    /// If the Type of Account is Instadose goto the Instadose Portal.
    /// Else goto the CS Web Suite.
    /// </summary>
    /// <param name="accountid"></param>
    /// <returns></returns>
    public string GetNavigationURL(string accountid)
    {
        string navigationURL = "";
        string typeOfAccount = "";

        if (this.ddlTypeOfReport.SelectedValue == "CCE")
        {
            typeOfAccount = this.ddlTypeOfAccount_CCE.SelectedValue;
        }
        else
        {
            typeOfAccount = this.ddlTypeOfAccount_DB.SelectedValue;
        }

        if (typeOfAccount == "Instadose")
        {
            navigationURL = string.Format("~/InformationFinder/Details/Account.aspx?ID={0}", accountid);
        }
        else
        {
            string gdsPreURL = System.Web.Configuration.WebConfigurationManager.AppSettings["cswebsuite_webaddress"];
            navigationURL = string.Format("{0}Finder/AccountDetails.aspx?Account={1}", gdsPreURL, accountid);
        }

        return navigationURL;
    }

    public override void VerifyRenderingInServerForm(Control control)
    {
        return;
    }

    /// <summary>
    /// Return to the Finance Dashboard (Instadose Portal).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnReturnToFinanceDashboard_Click(object sender, EventArgs e)
    {
        Response.Redirect("Default.aspx");
    }
    #endregion

    #region CREDIT CARD EXPIRATIONS LISTING FUNCTIONS.
    /// <summary>
    /// Function that validates (TRUE/FALSE) of the Credit Card Expirations Listing criteria entered.
    /// </summary>
    /// <returns></returns>
    private bool ValidationForCCEGridView()
    {
        // Check to see if the Account # (if entered) is valid/exists.
        if (this.txtAccountID_CCE.Text != "")
        {
            if (this.ddlTypeOfAccount_CCE.SelectedValue == "Instadose")
            {
                int instadoseAccountID = 0;
                if (int.TryParse(this.txtAccountID_CCE.Text, out instadoseAccountID))
                {
                    var instadoseAccountIDExists = (from acc in idc.AccountCreditCards
                                                    where acc.AccountID == instadoseAccountID
                                                    select acc.AccountID).FirstOrDefault();

                    if (instadoseAccountIDExists == null)
                    {
                        this.VisibleError(" Account # does not have a Credit Card on file.");
                        return false;
                    }
                }
                else
                {
                    // The AccountID is not in the correct format.
                    this.VisibleError(" Account # is in an invalid format.");
                    return false;
                }
            }
            else
            {
                string unixAccountNumber = this.txtAccountID_CCE.Text.Trim();

                var unixAccountExists = (from acc in idc.AccountCreditCards
                                         where acc.GDSAccount.Contains(unixAccountNumber)
                                         select acc.GDSAccount).FirstOrDefault();

                if (unixAccountExists == null)
                {
                    this.VisibleError(" Account # does not have a Credit Card on file.");
                    return false;
                }
            }
        }

        // Check to make sure that the CC Expiration From Date is less than the CC Expiration To Date.
        if (this.txtFromExpirationDate.Text.Trim().Length != 0 && this.txtToExpirationDate.Text.Trim().Length != 0)
        {
            DateTime localFromDate;
            DateTime localToDate;

            // Ensure the DateTime values can be parsed.
            if (DateTime.TryParse(this.txtFromExpirationDate.Text, out localFromDate) && DateTime.TryParse(this.txtToExpirationDate.Text, out localToDate))
            {
                if (localFromDate > localToDate)
                {
                    this.VisibleError(" Expiration From Date must be before the Expiration To Date.");
                    return false;
                }
            }
            else
            {
                // The Dates are not in the correct format.
                this.VisibleError(" Dates are in an invalid format.");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// IF Validation = TRUE then bind to List of Credit Card Expirations to GridView.
    /// </summary>
    public void BindCreditCardExpirationsGrid()
    {
        if (this.ValidationForCCEGridView() == true)
        {
            // Reset Error Message (blank).
            this.InvisibleError();

            // Create the SQL Connection from the connection string in the web.config
            SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
                ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

            //// Create the SQL Command.
            SqlCommand sqlCmd = new SqlCommand();
            sqlCmd.CommandType = CommandType.Text;

            // Using the vw_CreditCardExpirationsListing.
            string sqlQuery = "SELECT * FROM [vw_CreditCardExpirationsListing]";
            string filters = "";

            // If TypeOfAccount is Instadose the AccountID is an Integer.
            // Else (for UNIX) the AccountID value is a String.
            // Set the filter parameter for AccountID.
            if (this.txtAccountID_CCE.Text != "")
            {
                // If TypeOfAccount is Instadose the AccountID is an Integer.
                // Else (for UNIX) the AccountID value is a String.
                if (this.ddlTypeOfAccount_CCE.SelectedValue == "Instadose")
                {
                    filters += "([AccountID] = @pAccountID)";
                    sqlCmd.Parameters.Add(new SqlParameter("@pAccountID", Convert.ToInt32(this.txtAccountID_CCE.Text)));
                }
                else
                {
                    string accountID = string.Format("%{0}%", this.txtAccountID_CCE.Text);
                    filters += "([AccountID] LIKE @pAccountID)";
                    sqlCmd.Parameters.Add(new SqlParameter("@pAccountID", accountID));
                }
            }

            // Set the filter parameter for CompanyName.
            if (this.txtCompanyName_CCE.Text != "")
            {
                string companyName = string.Format("%{0}%", this.txtCompanyName_CCE.Text);
                if (filters != "") filters += " AND ";
                filters += "([CompanyName] LIKE @pCompanyName)";
                sqlCmd.Parameters.Add(new SqlParameter("@pCompanyName", companyName));
            }

            // Set the filter parameter for CreditCardName(Type).
            if (this.ddlTypeOfCreditCard.SelectedValue != "")
            {
                if (filters != "") filters += " AND ";
                filters += "([CreditCardName] = @pCreditCardName)";
                sqlCmd.Parameters.Add(new SqlParameter("@pCreditCardName", this.ddlTypeOfCreditCard.SelectedValue));
            }

            // Set the filter parameter for TypeOfAccount (Instadose/Unix).
            if (this.ddlTypeOfAccount_CCE.SelectedValue != "")
            {
                // Append AND if needed.
                if (filters != "") filters += " AND ";
                filters += "([TypeOfAccount] = @pTypeOfAccount)";
                sqlCmd.Parameters.Add(new SqlParameter("@pTypeOfAccount", this.ddlTypeOfAccount_CCE.SelectedValue));
            }

            // Set the filter parameter for the ExpirationDate Range (From - To).
            if (this.txtFromExpirationDate.Text != "" && this.txtToExpirationDate.Text != "")
            {
                DateTime dtRangeFrom = new DateTime();
                DateTime dtRangeTo = new DateTime();

                DateTime.TryParse(this.txtFromExpirationDate.Text, out dtRangeFrom);
                DateTime.TryParse(this.txtToExpirationDate.Text, out dtRangeTo);

                DateTime from = new DateTime(dtRangeFrom.Year, dtRangeFrom.Month, 1).AddDays(0);
                DateTime to = new DateTime(dtRangeTo.Year, dtRangeTo.Month, 1).AddMonths(1).AddDays(-1);
                // Append AND if needed.
                if (filters != "") filters += " AND ";
                filters += "([DT_ExpirationDate] BETWEEN @pFromDate AND @pToDate)";
                sqlCmd.Parameters.Add(new SqlParameter("@pFromDate", from));
                sqlCmd.Parameters.Add(new SqlParameter("@pToDate", to));
            }

            // Add the filters to the query if needed.
            if (filters != "") sqlQuery += " WHERE " + filters;

            // Add the ORDER BY and DIRECTION.
            sqlQuery += " ORDER BY " + this.gvCreditCardExpirationsListing.CurrentSortedColumn + ((this.gvCreditCardExpirationsListing.CurrentSortDirection == System.Web.UI.WebControls.SortDirection.Ascending) ? " ASC" : " DESC");

            // Pass the SQL Query String to the SQL Command.
            sqlCmd.Connection = sqlConn;
            bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
            if (!openConn) sqlCmd.Connection.Open();
            sqlCmd.CommandText = sqlQuery;
            SqlDataReader dreader = sqlCmd.ExecuteReader();

            CCEList cceList = new CCEList();
            cceList.CreditCardExpirations = new List<CCEListItem>();

            string creditCardName = "";
            string numberEncrypted = "";
            string maskedCreditCardNumber = "";

            while (dreader.Read())
            {
                creditCardName = dreader["CreditCardName"].ToString();
                numberEncrypted = dreader["NumberEncrypted"].ToString();

                maskedCreditCardNumber = Common.MaskCreditCardNumber(numberEncrypted, creditCardName);

                CCEListItem cceItem = null;

                if (this.ddlTypeOfAccount_CCE.SelectedValue == "Instadose")
                {
                    cceItem = new CCEListItem()
                    {
                        AccountID = dreader["AccountID"].ToString(),
                        CompanyName = dreader["CompanyName"].ToString(),
                        CreditCardName = dreader["CreditCardName"].ToString(),
                        NumberEncrypted = maskedCreditCardNumber,
                        DT_ExpirationDate = DateTime.Parse(dreader["DT_ExpirationDate"].ToString()),
                        TypeOfAccount = dreader["TypeOfAccount"].ToString()
                    };
                }
                else
                {
                    string accountID = dreader["AccountID"].ToString();
                    string companyName = "";

                    // Query the AccountID.
                    AccountRequests ar = new AccountRequests();
                    Mirion.DSD.GDS.API.DataTypes.GAccountDetails unixAccountInformation = ar.GetAccountDetails(accountID);

                    if (unixAccountInformation == null) companyName = "NONE ON RECORD";
                    else companyName = unixAccountInformation.CompanyName.ToUpper();

                    cceItem = new CCEListItem()
                    {
                        AccountID = accountID,
                        CompanyName = companyName,
                        CreditCardName = dreader["CreditCardName"].ToString(),
                        NumberEncrypted = maskedCreditCardNumber,
                        DT_ExpirationDate = DateTime.Parse(dreader["DT_ExpirationDate"].ToString()),
                        TypeOfAccount = dreader["TypeOfAccount"].ToString()
                    };
                }

                cceList.CreditCardExpirations.Add(cceItem);
            }

            CCEItemList = cceList.CreditCardExpirations;

            // Bind the results to the GridView.
            this.gvCreditCardExpirationsListing.DataSource = CCEItemList;
            this.gvCreditCardExpirationsListing.DataBind();

            sqlConn.Close();
            dreader.Close();
        }
        else
        {
            this.gvCreditCardExpirationsListing.DataSource = null;
            this.gvCreditCardExpirationsListing.DataBind();
            return;
        }
    }

    /// <summary>
    /// On_Click of "View Results" on the Credit Card Expirations Listing button.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnViewCCEGridView_Click(object sender, EventArgs e)
    {
        this.updtpnlCreditCardExpirations.Visible = true;

        // Reset the page to 0
        this.gvCreditCardExpirationsListing.PageIndex = 0;

        // Bind the gridview
        this.BindCreditCardExpirationsGrid();
    }

    /// <summary>
    /// Clear the Controls/Criteria for the Credit Card Expirations Listing (Report).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnClearFilters_CCE_Click(object sender, EventArgs e)
    {
        // Clear form controls.
        this.ddlTypeOfAccount_CCE.SelectedIndex = 0;
        this.txtAccountID_CCE.Text = string.Empty;
        this.txtCompanyName_CCE.Text = string.Empty;
        this.ddlTypeOfCreditCard.SelectedValue = "";
        this.txtFromExpirationDate.Text = string.Empty;
        this.txtToExpirationDate.Text = string.Empty;

        this.gvCreditCardExpirationsListing.DataSource = null;
        this.gvCreditCardExpirationsListing.DataBind();
    }

    /// <summary>
    /// SORTING function for gvCreditCardExpirationsListing GridView.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvCreditCardExpirationsListing_Sorting(object sender, GridViewSortEventArgs e)
    {
        this.SortCreditCardExpirationsListingGridView();
    }

    /// <summary>
    /// Sort GridView based on which column was selected and by Ascending or Descending order.
    /// </summary>
    private void SortCreditCardExpirationsListingGridView()
    {
        this.BindCreditCardExpirationsGrid();

        var items = (from cce in CCEItemList
                     select cce).OrderByDescending(cce => cce.DT_ExpirationDate).ToList();

        // Sort the list based on the column
        switch (this.gvCreditCardExpirationsListing.CurrentSortedColumn)
        {
            case "AccountID":
                // Sort the list by AccountID.
                items.Sort(delegate(CCEListItem i1, CCEListItem i2) { return i1.AccountID.CompareTo(i2.AccountID); });
                break;

            case "CompanyName":
                // Sort the list by CompanyName.
                items.Sort(delegate(CCEListItem i1, CCEListItem i2) { return i1.CompanyName.CompareTo(i2.CompanyName); });
                break;

            case "CreditCardName":
                // Sort the list by CreditCardName.
                items.Sort(delegate(CCEListItem i1, CCEListItem i2) { return i1.CreditCardName.CompareTo(i2.CreditCardName); });
                break;

            case "NumberEncrypted":
                // Sort the list by NumberEncrypted.
                items.Sort(delegate(CCEListItem i1, CCEListItem i2) { return i1.NumberEncrypted.CompareTo(i2.NumberEncrypted); });
                break;

            case "DT_ExpirationDate":
                // Sort the list by CT_ExpirationDate.
                items.Sort(delegate(CCEListItem i1, CCEListItem i2) { return i1.DT_ExpirationDate.CompareTo(i2.DT_ExpirationDate); });
                break;

            case "TypeOfAccount":
                // Sort the list by Credits.
                items.Sort(delegate(CCEListItem i1, CCEListItem i2) { return i1.TypeOfAccount.CompareTo(i2.TypeOfAccount); });
                break;
        }

        // Flip the list.
        if (this.gvCreditCardExpirationsListing.CurrentSortDirection == SortDirection.Descending) items.Reverse();

        // Display the results to the GridView.
        this.gvCreditCardExpirationsListing.DataSource = items;
        this.gvCreditCardExpirationsListing.DataBind();
    }

    /// <summary>
    /// IF the Credit Card Expiration Date has passed, color the record RED.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvCreditCardExpirationsListing_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        // Set the beginning Month/Year of the current month.
        DateTime today = DateTime.Today;
        DateTime monthStart = new DateTime(today.Year, today.Month, 1);

        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            DateTime dateToCompare = new DateTime();
            DateTime.TryParse(e.Row.Cells[4].Text, out dateToCompare);

            foreach (TableCell cell in e.Row.Cells)
            {
                if (monthStart > dateToCompare)
                {
                    cell.ForeColor = Color.Red;
                }
            }
        }
    }

    /// <summary>
    /// PAGING function for gvCreditCardExpirationsListing GridView.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvCreditCardExpirationsListing_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        this.gvCreditCardExpirationsListing.PageIndex = e.NewPageIndex;
        this.BindCreditCardExpirationsGrid();
    }
    #endregion

    #region DAILY BATCHES LISTING FUNCTIONS.
    /// <summary>
    /// Function that validates (TRUE/FALSE) of the Daily Batches Listing criteria entered.
    /// </summary>
    /// <returns></returns>
    private bool ValidationForDBGridView()
    {
        // Check to see if the Account # (if entered) is valid/exists.
        if (this.txtAccountID_DB.Text != "")
        {
            if (this.ddlTypeOfAccount_DB.SelectedValue == "Instadose")
            {
                int instadoseAccountID = 0;
                if (int.TryParse(this.txtAccountID_DB.Text, out instadoseAccountID))
                {
                    var instadoseAccountIDExists = (from pay in idc.Payments
                                                    where pay.AccountCreditCard.AccountID == instadoseAccountID
                                                    select pay.AccountCreditCard.AccountID).FirstOrDefault();

                    if (instadoseAccountIDExists == null)
                    {
                        this.VisibleError(" Account # does not have a Payment record.");
                        return false;
                    }
                }
                else
                {
                    // The AccountID is not in the correct format.
                    this.VisibleError(" Account # is in an invalid format.");
                    return false;
                }
            }
            else
            {
                string unixAccountNumber = this.txtAccountID_DB.Text.Trim();

                var unixAccountExists = (from pay in idc.Payments
                                         where pay.AccountCreditCard.GDSAccount == unixAccountNumber
                                         select pay.AccountCreditCard.GDSAccount).FirstOrDefault();

                if (unixAccountExists == null)
                {
                    this.VisibleError(" Account # does not have a Payment record.");
                    return false;
                }
            }
        }

        // Check to see if the Order # (if entered) is valid/exists.
        if (this.txtOrderID.Text != "")
        {
            int orderID = 0;
            if (int.TryParse(this.txtOrderID.Text, out orderID))
            {
                var orderIDExists = (from doc in idc.Documents
                                     where doc.OrderID == orderID
                                     select doc.OrderID).FirstOrDefault();

                if (orderIDExists == null)
                {
                    this.VisibleError(" Order # does not have a Payment record.");
                    return false;
                }
            }
            else
            {
                // The OrderID is not in the correct format.
                this.VisibleError(" Order # is in an invalid format.");
                return false;
            }
        }

        // Check to see if the Invoice # (if entered) is valid/exists.
        if (this.txtInvoiceNumber.Text != "")
        {
            string invoiceNumber = this.txtInvoiceNumber.Text.Trim();
            // PLEASE NOTE: This table should be Payments, but will be changes once the DataContext is updated.
            var invoiceNumberExists = (from pay in idc.Payments
                                       where pay.InvoiceNo.Contains(invoiceNumber)
                                       select pay.InvoiceNo).FirstOrDefault();

            if (invoiceNumberExists == null)
            {
                this.VisibleError(" Invoice # does not have a Payment record.");
                return false;
            }
        }

        // Check to make sure that the CC Expiration From Date is less than the CC Expiration To Date.
        if (this.txtFromPaymentDate.Text.Trim().Length != 0 && this.txtToPaymentDate.Text.Trim().Length != 0)
        {
            DateTime localFromDate;
            DateTime localToDate;

            // Ensure the DateTime values can be parsed.
            if (DateTime.TryParse(this.txtFromPaymentDate.Text, out localFromDate) && DateTime.TryParse(this.txtToPaymentDate.Text, out localToDate))
            {
                if (localFromDate > localToDate)
                {
                    this.VisibleError(" Expiration From Date must be before the Expiration To Date.");
                    return false;
                }
            }
            else
            {
                // The Dates are not in the correct format.
                this.VisibleError(" Dates are in an invalid format.");
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// IF Validation = TRUE then bind to List of Daily Batches to GridView.
    /// </summary>
    public void BindDailyBatchesGrid()
    {
        if (this.ValidationForDBGridView() == true)
        {
            // Reset Error Message (blank).
            this.InvisibleError();

            // Create the SQL Connection from the connection string in the web.config
            SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
                ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

            //// Create the SQL Command.
            SqlCommand sqlCmd = new SqlCommand();
            sqlCmd.CommandType = CommandType.Text;

            // Using the vw_CreditCardExpirationsListing.
            string sqlQuery = "SELECT * FROM [vw_DailyBatchesListing]";
            string filters = "";

            // Set the filter parameter for AccountID.
            if (this.txtAccountID_DB.Text != "")
            {
                // If TypeOfAccount is Instadose the AccountID is an Integer.
                // Else (for UNIX) the AccountID value is a String.
                if (this.ddlTypeOfAccount_DB.SelectedValue == "Instadose")
                {
                    filters += "([AccountID] = @pAccountID)";
                    sqlCmd.Parameters.Add(new SqlParameter("@pAccountID", Convert.ToInt32(this.txtAccountID_DB.Text)));
                }
                else
                {
                    filters += "([AccountID] = @pAccountID)";
                    sqlCmd.Parameters.Add(new SqlParameter("@pAccountID", this.txtAccountID_DB.Text));
                }
            }

            // Set the filter parameter for CompanyName.
            if (this.txtCompanyName_DB.Text != "")
            {
                string companyName = string.Format("%{0}%", this.txtCompanyName_DB.Text);
                // Append AND if needed.
                if (filters != "") filters += " AND ";
                filters += "([CompanyName] LIKE @pCompanyName)";
                sqlCmd.Parameters.Add(new SqlParameter("@pCompanyName", companyName));
            }

            // Set the filter parameter for TypeOfAccount (Instadose/Unix).
            if (this.ddlTypeOfAccount_DB.SelectedValue != "")
            {
                // Append AND if needed.
                if (filters != "") filters += " AND ";
                filters += "([TypeOfAccount] = @pTypeOfAccount)";
                sqlCmd.Parameters.Add(new SqlParameter("@pTypeOfAccount", this.ddlTypeOfAccount_DB.SelectedValue));
            }

            // Set the filter parameter for the PaymentDate Range (From - To).
            if (this.txtFromPaymentDate.Text != "" && this.txtToPaymentDate.Text != "")
            {
                DateTime dtRangeFrom = new DateTime();
                DateTime dtRangeTo = new DateTime();

                DateTime.TryParse(this.txtFromPaymentDate.Text, out dtRangeFrom);
                DateTime.TryParse(this.txtToPaymentDate.Text, out dtRangeTo);

                DateTime from = new DateTime(dtRangeFrom.Year, dtRangeFrom.Month, dtRangeFrom.Day).AddDays(0);
                DateTime to = new DateTime(dtRangeTo.Year, dtRangeTo.Month, dtRangeTo.Day).AddDays(1);
                // Append AND if needed.
                if (filters != "") filters += " AND ";
                filters += "([DateOfPayment] BETWEEN @pFromDate AND @pToDate)";
                sqlCmd.Parameters.Add(new SqlParameter("@pFromDate", from));
                sqlCmd.Parameters.Add(new SqlParameter("@pToDate", to));
            }

            // Set the filter parameter for OrderID.
            if (this.txtOrderID.Text != "")
            {
                // Append AND if needed.
                if (filters != "") filters += " AND ";
                filters += "([OrderID] = @pOrderID)";
                sqlCmd.Parameters.Add(new SqlParameter("@pOrderID", Convert.ToInt32(this.txtOrderID.Text)));
            }

            // Set the filter parameter for InvoiceNumber.
            if (this.txtInvoiceNumber.Text != "")
            {
                string invoiceNumber = string.Format("%{0}%", this.txtInvoiceNumber.Text.Trim());
                // Append AND if needed.
                if (filters != "") filters += " AND ";
                filters += "([InvoiceNumber] LIKE @pInvoiceNumber)";
                sqlCmd.Parameters.Add(new SqlParameter("@pInvoiceNumber", invoiceNumber));
            }

            // Add the filters to the query if needed.
            if (filters != "") sqlQuery += " WHERE " + filters;

            // Add the ORDER BY and DIRECTION.
            sqlQuery += " ORDER BY " + this.gvDailyBatchesListing.CurrentSortedColumn + ((this.gvDailyBatchesListing.CurrentSortDirection == System.Web.UI.WebControls.SortDirection.Ascending) ? " ASC" : " DESC");

            // Pass the SQL Query String to the SQL Command.
            sqlCmd.Connection = sqlConn;
            bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
            if (!openConn) sqlCmd.Connection.Open();
            sqlCmd.CommandText = sqlQuery;
            SqlDataReader dreader = sqlCmd.ExecuteReader();

            DBList dbList = new DBList();
            dbList.DailyBatches = new List<DBListItem>();

            string creditCardName = "";
            string numberEncrypted = "";
            string expirationDate = "";
            string maskedCreditCardNumber = "";
            //string creditCardInformation = "";

            while (dreader.Read())
            {
                creditCardName = dreader["CreditCardName"].ToString();
                numberEncrypted = dreader["NumberEncrypted"].ToString();
                expirationDate = dreader["ExpirationDate"].ToString();

                maskedCreditCardNumber = Common.MaskCreditCardNumber(numberEncrypted, creditCardName);

                string newMaskedCreditCardNumberFormat = string.Format("** {0}", Regex.Replace(maskedCreditCardNumber, "[^0-9.]", ""));

                //creditCardInformation = string.Format("{0} ({1} {2})", creditCardName, maskedCreditCardNumber, expirationDate);

                DateTime dateOfPayment = DateTime.Now;
                DateTime.TryParse(dreader["DateOfPayment"].ToString(), out dateOfPayment);

                DBListItem dbItem = null;

                int paymentID = 0;
                int orderID = 0;

                if (this.ddlTypeOfAccount_DB.SelectedValue == "Instadose")
                {
                    dbItem = new DBListItem()
                    {
                        PaymentID = int.TryParse(dreader["PaymentID"].ToString(), out paymentID) ? paymentID : 0,
                        AccountID = dreader["AccountID"].ToString(),
                        DateOfPayment = dateOfPayment,
                        Amount = dreader["Amount"].ToString(),
                        CurrencyCode = dreader["CurrencyCode"].ToString(),
                        TransactionCode = dreader["TransactionCode"].ToString(),
                        OrderID = int.TryParse(dreader["OrderID"].ToString(), out orderID) ? orderID : 0,
                        InvoiceNumber = dreader["InvoiceNumber"].ToString(),
                        CompanyName = dreader["CompanyName"].ToString(),
                        TypeOfAccount = dreader["TypeOfAccount"].ToString(),
                        //CreditCardInformation = creditCardInformation
                        CreditCardName = dreader["CreditCardName"].ToString(),
                        NumberEncrypted = newMaskedCreditCardNumberFormat.ToString(),
                        ExpirationDate = dreader["ExpirationDate"].ToString()
                    };
                }
                else
                {
                    // Do not display Order # (does not apply for UNIX accounts).
                    dbItem = new DBListItem()
                    {
                        PaymentID = int.TryParse(dreader["PaymentID"].ToString(), out paymentID) ? paymentID : 0,
                        AccountID = dreader["AccountID"].ToString(),
                        DateOfPayment = dateOfPayment,
                        Amount = dreader["Amount"].ToString(),
                        CurrencyCode = dreader["CurrencyCode"].ToString(),
                        TransactionCode = dreader["TransactionCode"].ToString(),
                        InvoiceNumber = dreader["InvoiceNumber"].ToString(),
                        CompanyName = dreader["CompanyName"].ToString(),
                        TypeOfAccount = dreader["TypeOfAccount"].ToString(),
                        //CreditCardInformation = creditCardInformation
                        CreditCardName = dreader["CreditCardName"].ToString(),
                        NumberEncrypted = newMaskedCreditCardNumberFormat.ToString(),
                        ExpirationDate = dreader["ExpirationDate"].ToString()
                    };
                }

                dbList.DailyBatches.Add(dbItem);
            }

            DBItemList = dbList.DailyBatches;

            // Bind the results to the GridView.
            this.gvDailyBatchesListing.DataSource = DBItemList;
            this.gvDailyBatchesListing.DataBind();

            sqlConn.Close();
            dreader.Close();
        }
        else
        {
            // Return empty DataList.
            this.gvDailyBatchesListing.DataSource = null;
            this.gvDailyBatchesListing.DataBind();
            return;
        }
    }

    /// <summary>
    /// Display BLANK TEXT if OrderID passed is "0".
    /// </summary>
    /// <param name="orderid"></param>
    /// <returns></returns>
    public string DisplayBlankOrderID(string orderid)
    {
        string displayText = "";

        if (orderid == "0")
        {
            displayText = null;
        }
        else
        {
            displayText = orderid.ToString();
        }

        return displayText;
    }

    /// <summary>
    /// Get GridView Column Index by Name.
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    private int GetGVColumnIndexByColumnName(GridView gridview, string columnname)
    {
        foreach (DataControlField column in gridview.Columns)
        {
            if (column.HeaderText.Trim() == columnname.Trim())
            {
                return gridview.Columns.IndexOf(column);
            }
        }

        return -1;
    }

    /// <summary>
    /// Based upon TypeOfAccount (Instadose/UNIX), Account # column will go to Instadose Portal or CS WebSuite.
    /// Also, the Invoice # column will be a Label or LinkButton (currently, UNIX Invoices are not in DocuStore).
    /// Show/Hide OrderID Column based on if the TypeOfAccount is "Instadose" or "Unix".
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvDailyBatchesListing_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            Label lbl = (Label)e.Row.FindControl("lblInvoiceNumber");
            LinkButton lnkbtn = (LinkButton)e.Row.FindControl("lnkbtnInvoiceNumber");

            int index = GetGVColumnIndexByColumnName(this.gvDailyBatchesListing, "Order #");

            if (this.ddlTypeOfAccount_DB.SelectedValue == "Instadose")
            {
                lbl.Visible = false;
                lnkbtn.Visible = true;
                this.gvDailyBatchesListing.Columns[index].Visible = true;
            }
            else
            {
                lbl.Visible = true;
                lnkbtn.Visible = false;
                this.gvDailyBatchesListing.Columns[index].Visible = false;
            }
        }
    }

    /// <summary>
    /// Generate the Invoice based on the InvoiceNumber clicked within the GridView.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void lnkbtnInvoiceNumber_Click(object sender, EventArgs e)
    {
        LinkButton lnkbtn = (LinkButton)sender;

        ActuateReport report = new ActuateReport("rpt_InstadoseInv");
        report.Arguments.Add("InvoiceNum", lnkbtn.Text);

        // Generate the launch script.
        string script = string.Format("window.open(\"{0}\", \"{1}\", \"{2}\");", report.GetReportUri(), "_blank", "menubar=1,resizable=1,scrollbars=1,width=1024,height=600");

        // Launch the report.
        ScriptManager.RegisterClientScriptBlock(Page, typeof(Page), "OpenWindow", script, true);
    }

    /// <summary>
    /// On_Click of "View Results" on the Daily Batches Listing button.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnViewDBGridView_Click(object sender, EventArgs e)
    {
        this.updtpnlDailyBatches.Visible = true;

        // Reset the page to 0
        this.gvDailyBatchesListing.PageIndex = 0;

        // Bind the gridview
        this.BindDailyBatchesGrid();
    }

    /// <summary>
    /// Clear the Controls/Criteria for the Daily Batches Listing (Report).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnClearFilters_DB_Click(object sender, EventArgs e)
    {
        int index = GetGVColumnIndexByColumnName(this.gvDailyBatchesListing, "Order #");

        // Clear form controls.
        this.txtAccountID_DB.Text = string.Empty;
        this.txtCompanyName_DB.Text = string.Empty;
        this.ddlTypeOfAccount_DB.SelectedValue = "Instadose";
        this.txtOrderID.Visible = true;
        this.txtOrderID.Text = string.Empty;
        this.txtInvoiceNumber.Text = string.Empty;
        this.txtFromPaymentDate.Text = string.Empty;
        this.txtToPaymentDate.Text = string.Empty;

        this.gvDailyBatchesListing.Columns[index].Visible = true;

        this.gvDailyBatchesListing.DataSource = null;
        this.gvDailyBatchesListing.DataBind();
    }

    /// <summary>
    /// SORTING function for gvDailyBatchesListings GridView.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvDailyBatchesListing_Sorting(object sender, GridViewSortEventArgs e)
    {
        this.SortDailyBatchesListingGridView();
    }

    /// <summary>
    /// Sort GridView based on which column was selected and by Ascending or Descending order.
    /// </summary>
    private void SortDailyBatchesListingGridView()
    {
        this.BindDailyBatchesGrid();

        var items = (from db in DBItemList
                     select db).OrderByDescending(db => db.DateOfPayment).ToList();

        // Sort the list based on the column
        switch (this.gvDailyBatchesListing.CurrentSortedColumn)
        {
            case "PaymentID":
                // Sort the list by PaymentID.
                items.Sort(delegate(DBListItem i1, DBListItem i2) { return i1.PaymentID.CompareTo(i2.PaymentID); });
                break;

            case "AccountID":
                // Sort the list by AccountID.
                items.Sort(delegate(DBListItem i1, DBListItem i2) { return i1.AccountID.CompareTo(i2.AccountID); });
                break;

            case "DateOfPayment":
                // Sort the list by DateOfPayment.
                items.Sort(delegate(DBListItem i1, DBListItem i2) { return i1.DateOfPayment.CompareTo(i2.DateOfPayment); });
                break;

            case "Amount":
                // Sort the list by Amount.
                items.Sort(delegate(DBListItem i1, DBListItem i2) { return i1.Amount.CompareTo(i2.Amount); });
                break;

            case "TransactionCode":
                // Sort the list by TransactionCode.
                items.Sort(delegate(DBListItem i1, DBListItem i2) { return i1.TransactionCode.CompareTo(i2.TransactionCode); });
                break;

            case "OrderID":
                // Sort the list by OrderID.
                items.Sort(delegate(DBListItem i1, DBListItem i2) { return i1.OrderID.CompareTo(i2.OrderID); });
                break;

            case "InvoiceNumber":
                // Sort the list by OrderID.
                items.Sort(delegate(DBListItem i1, DBListItem i2) { return i1.InvoiceNumber.CompareTo(i2.InvoiceNumber); });
                break;

            case "CompanyName":
                // Sort the list by CompanyName.
                items.Sort(delegate(DBListItem i1, DBListItem i2) { return i1.CompanyName.CompareTo(i2.CompanyName); });
                break;

            case "TypeOfAccount":
                // Sort the list by Credits.
                items.Sort(delegate(DBListItem i1, DBListItem i2) { return i1.TypeOfAccount.CompareTo(i2.TypeOfAccount); });
                break;

            case "CreditCardInformation":
                // Sort the list by Credits.
                items.Sort(delegate(DBListItem i1, DBListItem i2) { return i1.CreditCardInformation.CompareTo(i2.CreditCardInformation); });
                break;
        }

        // Flip the list.
        if (this.gvDailyBatchesListing.CurrentSortDirection == SortDirection.Descending) items.Reverse();

        // Display the results to the GridView.
        this.gvDailyBatchesListing.DataSource = items;
        this.gvDailyBatchesListing.DataBind();
    }

    /// <summary>
    /// PAGING function for gvDailyBatchesListings GridView.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvDailyBatchesListing_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        this.gvDailyBatchesListing.PageIndex = e.NewPageIndex;
        this.BindDailyBatchesGrid();
    }
    #endregion

    #region EXPORT TO EXCEL.
    protected void btnExportToExcel_Click(object sender, EventArgs e)
    {
        string excelFileName = "";

        string selectedReport = this.ddlTypeOfReport.SelectedValue;

        // Create an data table used to export.
        DataTable exportTable = new DataTable();

        if (selectedReport == "CCE")
        {
            if (this.gvCreditCardExpirationsListing.Rows.Count > 0)
            {
                excelFileName = string.Format("{0}_{1}", "CreditCardExpirations", DateTime.Now.ToShortDateString());

                // Bind the GridView, to fill the Invoice list for Excel export.
                this.BindCreditCardExpirationsGrid();

                // Create columns for this DataTable.
                DataColumn colAccountID = new DataColumn("Account#");
                DataColumn colCompanyName = new DataColumn("Company Name");
                DataColumn colCreditCardName = new DataColumn("CC Type");
                DataColumn colCreditCardNumber = new DataColumn("CC #");
                DataColumn colExpirationDate = new DataColumn("Expiration Date");
                DataColumn colTypeOfAccount = new DataColumn("Acct. Type");

                // Define DataType of the columns.
                colAccountID.DataType = System.Type.GetType("System.String");
                colCompanyName.DataType = System.Type.GetType("System.String");
                colCreditCardName.DataType = System.Type.GetType("System.String");
                colCreditCardNumber.DataType = System.Type.GetType("System.String");
                colExpirationDate.DataType = System.Type.GetType("System.String");
                colTypeOfAccount.DataType = System.Type.GetType("System.String");

                // Add all of these columns into the exportTable.
                exportTable.Columns.Add(colAccountID);
                exportTable.Columns.Add(colCompanyName);
                exportTable.Columns.Add(colCreditCardName);
                exportTable.Columns.Add(colCreditCardNumber);
                exportTable.Columns.Add(colExpirationDate);
                exportTable.Columns.Add(colTypeOfAccount);

                // Add the rows from the Credit Card Expirations list.
                foreach (CCEListItem i in CCEItemList)
                {
                    // Create a new table row.
                    DataRow dr = exportTable.NewRow();

                    dr[colAccountID] = i.AccountID;
                    dr[colCompanyName] = i.CompanyName;
                    dr[colCreditCardName] = i.CreditCardName;
                    dr[colCreditCardNumber] = i.NumberEncrypted;
                    dr[colExpirationDate] = i.DT_ExpirationDate.ToString("MM/yyyy");
                    dr[colTypeOfAccount] = i.TypeOfAccount;

                    // Add the row to the table.
                    exportTable.Rows.Add(dr);
                }
            }
            else
            {
                VisibleError(" There are no records to Export to Excel.");
                return;
            }
        }
        else if (selectedReport == "DB")
        {
            if (this.gvDailyBatchesListing.Rows.Count > 0)
            {
                excelFileName = string.Format("{0}_{1}", "DailyBatches", DateTime.Now.ToShortDateString());

                // Bind the GridView, to fill the Invoice list for Excel export.
                this.BindDailyBatchesGrid();

                // Create columns for this DataTable.
                DataColumn colPaymentID = new DataColumn("Payment #");
                DataColumn colAccountID = new DataColumn("Acct. #");
                DataColumn colCompanyName = new DataColumn("Company");
                DataColumn colDateOfPayment = new DataColumn("Payment Date");
                DataColumn colAmount = new DataColumn("Amount");
                DataColumn colCurrencyCode = new DataColumn("Currency Code");
                DataColumn colTransactionCode = new DataColumn("Trans. Code");
                DataColumn colOrderID = new DataColumn("Order #");
                DataColumn colInvoiceNumber = new DataColumn("Invoice #");
                DataColumn colCreditCardName = new DataColumn("CC Type");
                DataColumn colNumberEncrypted = new DataColumn("CC Number");
                DataColumn colExpirationDate = new DataColumn("CC Expiration Date");
                //DataColumn colCreditCardInformation = new DataColumn("CC Info.");
                DataColumn colTypeOfAccount = new DataColumn("Acct. Type");

                // Define DataType of the columns.
                colPaymentID.DataType = System.Type.GetType("System.String");
                colAccountID.DataType = System.Type.GetType("System.String");
                colCompanyName.DataType = System.Type.GetType("System.String");
                colDateOfPayment.DataType = System.Type.GetType("System.String");
                colAmount.DataType = System.Type.GetType("System.String");
                colCurrencyCode.DataType = System.Type.GetType("System.String");
                colTransactionCode.DataType = System.Type.GetType("System.String");
                colOrderID.DataType = System.Type.GetType("System.String");
                colInvoiceNumber.DataType = System.Type.GetType("System.String");
                colCreditCardName.DataType = System.Type.GetType("System.String");
                colNumberEncrypted.DataType = System.Type.GetType("System.String");
                colExpirationDate.DataType = System.Type.GetType("System.String");
                //colCreditCardInformation.DataType = System.Type.GetType("System.String");
                colTypeOfAccount.DataType = System.Type.GetType("System.String");

                // Add all of these columns into the exportTable.
                exportTable.Columns.Add(colPaymentID);
                exportTable.Columns.Add(colAccountID);
                exportTable.Columns.Add(colCompanyName);
                exportTable.Columns.Add(colDateOfPayment);
                exportTable.Columns.Add(colAmount);
                exportTable.Columns.Add(colCurrencyCode);
                exportTable.Columns.Add(colTransactionCode);
                exportTable.Columns.Add(colOrderID);
                exportTable.Columns.Add(colInvoiceNumber);
                exportTable.Columns.Add(colCreditCardName);
                exportTable.Columns.Add(colNumberEncrypted);
                exportTable.Columns.Add(colExpirationDate);
                //exportTable.Columns.Add(colCreditCardInformation);
                exportTable.Columns.Add(colTypeOfAccount);

                // Add the rows from the Daily Batches list.
                foreach (DBListItem i in DBItemList)
                {
                    // Create a new table row.
                    DataRow dr = exportTable.NewRow();

                    dr[colPaymentID] = i.PaymentID;
                    dr[colAccountID] = i.AccountID;
                    dr[colCompanyName] = i.CompanyName;
                    dr[colDateOfPayment] = i.DateOfPayment;
                    dr[colAmount] = i.Amount;
                    dr[colCurrencyCode] = i.CurrencyCode;
                    dr[colTransactionCode] = i.TransactionCode;
                    if (this.ddlTypeOfAccount_DB.SelectedValue == "Instadose")
                    {
                        dr[colOrderID] = i.OrderID;
                    }
                    else
                    {
                        dr[colOrderID] = null;
                    }
                    dr[colInvoiceNumber] = i.InvoiceNumber;
                    dr[colCreditCardName] = i.CreditCardName;
                    dr[colNumberEncrypted] = i.NumberEncrypted;
                    dr[colExpirationDate] = i.ExpirationDate;
                    //dr[colCreditCardInformation] = i.CreditCardInformation;
                    dr[colTypeOfAccount] = i.TypeOfAccount;

                    // Add the row to the table.
                    exportTable.Rows.Add(dr);
                }
            }
            else
            {
                VisibleError(" There are no records to Export to Excel.");
                return;
            }
        }
        else
        {
            VisibleError(" Please select a Report Type and click the 'View Results' button before proceeding.");
            this.ddlTypeOfReport.Focus();
            return;
        }

        // Build the export table.
        TableExport tableExport = new TableExport(exportTable);

        try
        {
            // Read the CSS template from file
            tableExport.Stylesheet = System.IO.File.ReadAllText(Server.MapPath("~/css/export/grids.css"));
        }
        catch (Exception ex)
        {
            VisibleError(ex.ToString());
            return;
        }

        try
        {
            // Create the export file based on the selected value.
            tableExport.Export(excelFileName, "XLS");

            ExportFile file = tableExport.File;

            // Clear everything out.
            Response.Clear();
            Response.ClearHeaders();

            // Set the response headers.
            Response.ContentType = file.ContentType;
            Response.AddHeader("Content-Disposition", file.ContentDisposition);

            // Write to Excel file.
            if (file.Content.GetType() == typeof(byte[]))
                Response.BinaryWrite((byte[])file.Content);
            else
            {
                Response.Write(file.Content.ToString());
            }
        }
        catch (Exception ex)
        {
            VisibleError(ex.ToString());
            return;
        }

        Response.Flush();
        Response.End();
    }
    #endregion

    #region ONTEXTCHANGED FOR TEXTBOXES.
    protected void txtFromPaymentDate_TextChanged(object sender, EventArgs e)
    {
        TextBox txtbxFromPaymentDate = sender as TextBox;

        if (txtToPaymentDate != null)
        {
            DateTime fromDate = DateTime.Now;
            DateTime toDate = DateTime.Now;

            DateTime.TryParse(this.txtFromPaymentDate.Text, out fromDate);
            toDate = fromDate.AddDays(1);

            this.txtToPaymentDate.Text = toDate.ToShortDateString();
        }
    }

    protected void txtFromExpirationDate_TextChanged(object sender, EventArgs e)
    {
        TextBox txtbxFromExpirationDate = sender as TextBox;

        if (txtbxFromExpirationDate != null)
        {
            DateTime fromDate = DateTime.Now;

            DateTime.TryParse(this.txtFromExpirationDate.Text, out fromDate);
            DateTime toDate = new DateTime(fromDate.Year, fromDate.Month, DateTime.DaysInMonth(fromDate.Year, fromDate.Month));

            this.txtToExpirationDate.Text = toDate.ToShortDateString();
        }
    }
    #endregion

    public void GetInvoices(string account, string invoiceno = null)
    {
        FinanceRequests fr = new FinanceRequests();
        var invoices = fr.GetCCInvoices(account, invoiceno);

    }
}
