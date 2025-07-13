using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Instadose;
using Instadose.Data;
using Instadose.Integration;
using Instadose.Processing;

public partial class InstaDose_Finance_Renewal_ScheduledBillings : System.Web.UI.Page
{
    // Create the database LINQ objects.
    private InsDataContext idc = new InsDataContext();

    // Build list of error messages
    private List<string> ErrorMessageList = new List<string>();
       
    //DataTable to store the processed billings data
    public DataTable dtReviewBillings = new DataTable();
    
    protected void Page_Load(object sender, EventArgs e)
    {
        // Prevent the page from redrawing the table if not a first time load.
        if (!IsPostBack)
        {            
            //Load dropdowns.
            bindDropDowns();
                
            // Bind the grid
            bindBillingsGrid();
        }
    }
    
    /// <summary>
    /// Load dropdowns - BillingMethod and Brand.
    /// </summary>
    private void bindDropDowns()
    {

        //*************************************************************************
        //                      Billing Method
        //**************************************************************************
        this.ddlBillingMethod.DataSource = (from idcBillingMethod in idc.PaymentMethods
                                            select new
                                            {
                                                DESC = idcBillingMethod.PaymentMethodID,
                                                ID = idcBillingMethod.PaymentMethodName
                                            }).ToList();

        this.ddlBillingMethod.DataTextField = "ID";
        this.ddlBillingMethod.DataValueField = "DESC";

        this.ddlBillingMethod.DataBind();
        ddlBillingMethod.Items.Insert(0, new ListItem("ALL", ""));
    
        //*************************************************************************
        //                      Brand
        //**************************************************************************
        this.ddlBrand.DataSource = (from idcBillingMethod in idc.BrandSources
                                    select new
                                    {
                                        DESC = idcBillingMethod.BrandSourceID,
                                        ID = idcBillingMethod.BrandName
                                    }).ToList();
        this.ddlBrand.DataTextField = "ID";
        this.ddlBrand.DataValueField = "DESC";
        this.ddlBrand.DataBind();
        ddlBrand.Items.Insert(0, new ListItem("ALL", ""));

    }

    /// <summary>
    /// When the text is change in the from period textbox.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void txtPeriodFrom_TextChange(object sender, EventArgs e)
    {
        // Clear the style.
        txtPeriodFrom.CssClass = "";

        // Exit if there is no text.
        if (txtPeriodFrom.Text == "") return;

        // Test the see if the date is valid
        DateTime fromDate = DateTime.Now;
        if (!DateTime.TryParse(txtPeriodFrom.Text, out fromDate))
        {
            txtPeriodFrom.CssClass = "invalidText";
            return;
        }

        // Test to ensure the date is less than To Period
        DateTime toDate = DateTime.Now;
        if (!DateTime.TryParse(txtPeriodTo.Text, out toDate)) return;

        // Ensure the from date is less than to date
        if (fromDate > toDate)
        {
            txtPeriodFrom.CssClass = "invalidText";
            return;
        }

        // Reset the page to 0
        gvUpcomingBillings.PageIndex = 0;

        // Rebind the renewals grid.
        bindBillingsGrid();
        //UpdateRenewalGrid();
    }

    /// <summary>
    /// When the text is change in the to period textbox.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void txtPeriodTo_TextChange(object sender, EventArgs e)
    {
        // Clear the style.
        txtPeriodTo.CssClass = "";

        // Exit if there is no text.
        if (txtPeriodTo.Text == "") return;

        // Test the see if the date is valid
        DateTime toDate = DateTime.Now;
        if (!DateTime.TryParse(txtPeriodTo.Text, out toDate))
        {
            txtPeriodTo.CssClass = "invalidText";
            return;
        }

        // Test to ensure the date is less than From Period
        DateTime fromDate = DateTime.Now;
        if (!DateTime.TryParse(txtPeriodFrom.Text, out fromDate)) return;

        // Ensure the from date is less than to date
        if (fromDate > toDate)
        {
            txtPeriodTo.CssClass = "invalidText";
            return;
        }

        // Reset the page to 0
        gvUpcomingBillings.PageIndex = 0;

        // Rebind the renewals grid.
        bindBillingsGrid();
        //UpdateRenewalGrid();
    }

    /// <summary>
    /// Query the data source and bind the data grid.
    /// </summary>
    private void bindBillingsGrid()
    {

        //Turn Error msg off.
        //this.renewalRowError.Visible = false;

        // Create the sql connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the sql command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.Connection = sqlConn;
        sqlCmd.CommandType = CommandType.Text;

        // Create the query for the view.
        string query = "SELECT * FROM [vw_UpcomingBillings]";
        string filters = "";

        // Set the filter parameter for billing method
        if (Convert.ToString(ddlBillingMethod.SelectedItem) != "ALL")
        {
            filters += "([BillingMethod] = @billingMethod)";
            sqlCmd.Parameters.Add(new SqlParameter("billingMethod", Convert.ToString(ddlBillingMethod.SelectedItem)));
        }

        // Set the filter parameter for the brand
        if (Convert.ToString(ddlBrand.SelectedItem) != "ALL")
        {
            // Append the AND if needed.
            if (filters != "") filters += " AND ";

            filters += "([BrandName] = @brandName)";
            sqlCmd.Parameters.Add(new SqlParameter("brandName", Convert.ToString(ddlBrand.SelectedItem)));
        }

        // Period From and To
        if (txtPeriodFrom.Text != "" && txtPeriodTo.Text != "")
        {
            DateTime dtPeriodFrom = DateTime.Now;
            DateTime dtPeriodTo = DateTime.Now;

            // Ensure the date time stuff can be parsed.
            if (DateTime.TryParse(txtPeriodFrom.Text, out dtPeriodFrom) && DateTime.TryParse(txtPeriodTo.Text, out dtPeriodTo))
            {
                if (dtPeriodFrom <= dtPeriodTo)
                {
                    // Append the AND if needed.
                    if (filters != "") filters += " AND ";

                    filters += "([WhenToBill] BETWEEN @periodFrom AND @periodTo)";
                    sqlCmd.Parameters.Add(new SqlParameter("periodFrom", dtPeriodFrom));
                    sqlCmd.Parameters.Add(new SqlParameter("periodTo", dtPeriodTo));
                }
                else
                {
                    // From date must be BEFORE the to date.
                }
            }
            else
            {
                // Dates are not in the correct format.
            }
        }

        // Append the AND if needed.

        // Add the filters to the query if needed.
        if (filters != "") query += " WHERE " + filters;

        // Add the order by and the direction
        query += " ORDER BY " + gvUpcomingBillings.CurrentSortedColumn + ((gvUpcomingBillings.CurrentSortDirection == SortDirection.Ascending) ? " ASC" : " DESC");

        // Pass the query string to the command
        sqlCmd.CommandText = query;

        // Pass the query string to the command
        sqlCmd.Connection = sqlConn;

        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();

        sqlCmd.CommandText = query;
        SqlDataReader reader = sqlCmd.ExecuteReader();

        DataTable dtScheduledBillings = new DataTable();

        // Create the review orders datatable to hold the contents of the order.
        dtScheduledBillings = new DataTable("Billings Table");

        // Create the columns for the review orders datatable.
        dtScheduledBillings.Columns.Add("Process", Type.GetType("System.Boolean"));

        dtScheduledBillings.Columns.Add("AccountID", typeof(int));
        dtScheduledBillings.Columns.Add("AccountName", typeof(string));
        dtScheduledBillings.Columns.Add("WhenToBill", typeof(DateTime));
        dtScheduledBillings.Columns.Add("BillingMethod", typeof(string));
        dtScheduledBillings.Columns.Add("ContractEndDate", typeof(DateTime));
        dtScheduledBillings.Columns.Add("CustomerType", typeof(string));
        dtScheduledBillings.Columns.Add("LastBilled", typeof(DateTime));
        dtScheduledBillings.Columns.Add("Result", typeof(string));

        while (reader.Read())
        {

            // Create a new review order row.
            DataRow dr = dtScheduledBillings.NewRow();

            // Fill row details.
            dr["Process"] = false;

            dr["AccountID"] = reader["AccountID"].ToString();
            dr["CustomerType"] = reader["CustomerType"].ToString();
            dr["AccountName"] = reader["AccountName"].ToString();

            dr["LastBilled"] = Convert.ToDateTime(reader["LastBilled"]).ToShortDateString();
            dr["WhenToBill"] = Convert.ToDateTime(reader["WhenToBill"]).ToShortDateString();
            dr["ContractEndDate"] = Convert.ToDateTime(reader["ContractEndDate"]).ToShortDateString();
            dr["BillingMethod"] = reader["BillingMethod"].ToString();

            // Add row to the data table.
            dtScheduledBillings.Rows.Add(dr);
        }

        gvUpcomingBillings.DataSource = dtScheduledBillings;
        gvUpcomingBillings.DataBind();

    }

    /// <summary>
    /// When the text is change in the to period textbox.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
   

    /// <summary>
    /// Get the order ids from grid view and fills a list.
    /// </summary>
    /// <returns>A list of order ids.</returns>
    private List<int> GetAccountIDs()
    {

        // Generate the renewal screen.
        //Renewals renewal = null;
                   
        try
        {
            // Build list of orders to process
            List<int> ProcessAcctIDs = new List<int>();

            string renewalCode = null;
            DateTime contractEndDate;
            
            //gvUpcomingBillings.Columns[7].Visible = true;

            // Loop through the grid to get the order Ids
            foreach (GridViewRow gvr in this.gvUpcomingBillings.Rows)
            {
                // Find the checkbox
                CheckBox cbRow = (CheckBox)gvr.Cells[0].FindControl("cbRow");
             
                // Determine if the checkbox was checked.
                if (cbRow.Checked)
                {
                    //Get Renewal Code.

                    HiddenField hfRenewalCode = (HiddenField)gvr.Cells[7].FindControl("hidRenewalCode");
                    renewalCode = hfRenewalCode.Value.ToString();
                    
                    int accountID = 0;
                    HiddenField hfAccountID = (HiddenField)gvr.Cells[1].FindControl("HidACCID");
                    accountID = int.Parse(hfAccountID.Value);

                    contractEndDate = Convert.ToDateTime(gvr.Cells[5].Text);

                    ProcessAcctIDs.Add(accountID);
                    cbRow.Checked = false;
                    continue;
                }
            }
           
            return ProcessAcctIDs;
        }
        catch (Exception ex)
        {

            Basics.WriteLogEntry(ex.Message, HttpContext.Current.User.Identity.Name,
                "InstaDose_Admin_Renewal_RenewalGeneration_ProcessAcctIds", Basics.MessageLogType.Critical);

            return null;
        }
    }
    
    /// <summary>
    /// Fired when the page has been changed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvUpcomingBillings_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvUpcomingBillings.PageIndex = e.NewPageIndex;
        bindBillingsGrid();
    }

    /// <summary>
    /// Generate the renewal.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnGenerate_Click(object sender, EventArgs e)
    {
        ProcessBillings();
    }

    /// <summary>
    /// Process scheudled billings for account IDs from grid.
    /// </summary>
    /// <returns>A list of order ids.</returns>
    private void ProcessBillings()
    {

        InvisibleErrors();
        var renewal = new Renewals();
        renewal.UserName = ActiveDirectoryQueries.GetUserName();
        //Generate Renewal Number.
        string renewalLogNo = renewal.GenerateRenewalNo();

        var errors = new List<ProcessError>();

        //-------------------------------------------------------------
        // Construct Scheduled Billing Table
        //DataTable to store the processed billings data
        DataTable dtReviewBillings = new DataTable("Review Billings");
        dtReviewBillings.Columns.Add("AccountID", typeof(int));
        dtReviewBillings.Columns.Add("AccountName", typeof(string));
        dtReviewBillings.Columns.Add("WhenToBill", typeof(string));
        dtReviewBillings.Columns.Add("BillingMethod", typeof(string));
        dtReviewBillings.Columns.Add("ContractEndDate", typeof(string));
        dtReviewBillings.Columns.Add("CustomerType", typeof(string));
        dtReviewBillings.Columns.Add("LastBilled", typeof(string));
        dtReviewBillings.Columns.Add("Result", typeof(string));

        // Loop through the grid to get the order Ids
        foreach (GridViewRow gvr in this.gvUpcomingBillings.Rows)
        {
            // Find the checkbox
            CheckBox cbRow = (CheckBox)gvr.Cells[0].FindControl("cbRow");

            // Determine if the checkbox was checked.
            if (!cbRow.Checked) continue;

            HiddenField hfAccountID = (HiddenField)gvr.Cells[1].FindControl("HidACCID");
            int accountID = int.Parse(hfAccountID.Value);

            var account = (from a in idc.Accounts where a.AccountID == accountID select a).FirstOrDefault();
            DateTime contractStartDate = account.ContractStartDate.Value;
            DateTime contractEndDate = account.ContractEndDate.Value;

            // Grab the status after the billing has been made.
            RenewalStatus status = renewal.CreateRenewalBilling(accountID, null);

            var order = idc.Orders.Where(o => o.OrderID == status.OrderID && o.OrderStatusID != 10).FirstOrDefault(); 
            Payment payment = (order == null) ? null : order.Payments.FirstOrDefault();

            // Add an error if it's there.
            if (!string.IsNullOrEmpty(status.ErrorMessage))
            {
                errors.Add(new ProcessError
                {
                    Account = hfAccountID.Value,
                    OrderID = order == null ? 0 : order.OrderID,
                    WhenToBill = gvr.Cells[3].Text,
                    Message = status.ErrorMessage
                });
            }

            // create a log for this renewal
            idc.RenewalLogs.InsertOnSubmit(new RenewalLogs()
            {
                AccountID = accountID,
                BillingTerm = (from bt in idc.BillingTerms where bt.BillingTermID == status.BillingTermID select bt).FirstOrDefault(),
                BrandSource = account.BrandSource,
                DateProcessed = DateTime.Now,
                ErrorMessage = status.ErrorMessage,
                Hide = false,
                LocationID = status.LocationID,
                Order = order,
                OrderStatus = (order == null) ? null : order.OrderStatus,
                OrgContractStartDate = contractStartDate,
                OrgContractEndDate = contractEndDate,
                PaymentMethod = (payment != null) ? payment.PaymentMethod : null,
                RenewalAmount = status.TotalAmount,
                RenewalNo = renewalLogNo
            });

            // Add the record.
            idc.SubmitChanges();

            // Create the data row to be added to the data table.
            DataRow dr = dtReviewBillings.NewRow();
            dr["AccountID"] = accountID.ToString();
            dr["AccountName"] = gvr.Cells[2].Text;
            dr["WhenToBill"] = gvr.Cells[3].Text;
            dr["BillingMethod"] = payment != null ? payment.PaymentMethod.PaymentMethodName : "";
            dr["ContractEndDate"] = gvr.Cells[5].Text;
            dr["CustomerType"] = gvr.Cells[6].Text;
            dr["LastBilled"] = gvr.Cells[7].Text;
            dr["Result"] = status.RenewalComplete ? "The billing was processed. " : "The billing failed.";

            dtReviewBillings.Rows.Add(dr);

            cbRow.Checked = false;
            pnlReview.Visible = false;
                
        }

        //Display Results Grid.
        pnlReview.Visible = true;

        if (errors.Count > 0)
            VisibleErrors(errors);
        
        pnlBillings.Visible = false;
        pnlReview.Visible = true;

        gvReview.DataSource = dtReviewBillings;
        gvReview.DataBind();

    }
    
    /// <summary>
    /// Hides the review and completed panels and displays the renewals table.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnBack_Click(object sender, EventArgs e)
    {
        pnlCompleted.Visible = false;
        pnlBillings.Visible = true;
        pnlReview.Visible = false;

        // Force the screen to redraw.
        this.bindBillingsGrid();
    }

    /// <summary>
    /// Called from many objects, force binds the gridview.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gridviewBinder(object sender, EventArgs e)
    {
        // Reset the page to 0
        gvUpcomingBillings.PageIndex = 0;

        // Bind the gridview
        bindBillingsGrid();
    }

    /// <summary>
    /// When the column is being sorted.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvUpcomingBillings_Sorting(object sender, GridViewSortEventArgs e)
    {
        gvUpcomingBillings.PageIndex = 0;
        bindBillingsGrid();
    }
       
    protected void gvUpcomingBillings_SelectedIndexChanging(object sender, EventArgs e)
    {

    }
    
    //**************************************************************************
    // Error Message handling.
    //**************************************************************************
    public void BindErrorMessages()
    {
        //Turn off message
        //plFormMessage.Visible = false;

        blstErrors.DataSource = ErrorMessageList;
        blstErrors.DataBind();

        //Display only errors are present.
        if (ErrorMessageList.Count() == 0)
        {
            plErrorMessage.Visible = false;
            //this.pnlReview.Visible = true;
        }
        else
        {
            //this.pnlReview.Visible = false;
            plErrorMessage.Visible = true;
        }    
    }

    private void VisibleErrors(List<ProcessError> errors)
    {
        plErrorMessage.Visible = true;
        foreach (var error in errors)
            blstErrors.Items.Add(string.Format("Account# {0} (Order #: {1}, Renew On {2}) - {3}", error.Account, error.OrderID.ToString(), error.WhenToBill, error.Message));
    }

    private void InvisibleErrors()
    {
        // clear error collection
        blstErrors.Items.Clear();

        // Reset submission form error message
        plErrorMessage.Visible = false;
    }

    private class ProcessError
    {
        public string Account { get; set; }
        public string WhenToBill { get; set; }
        public int OrderID { get; set; }
        public string Message { get; set; }
    }
}
