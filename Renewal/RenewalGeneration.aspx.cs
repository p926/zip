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

public partial class InstaDose_Finance_Renewal_RenewalGeneration : System.Web.UI.Page
{
    // Create the database LINQ objects.
    InsDataContext idc = new InsDataContext();

    // Build list of error messages
    List<string> ErrorMessageList = new List<string>();

    string RenewalLogNo;
   
    //Add Processing message to Release Orders button and disable during
    //processing.
    protected override void OnInit(EventArgs e)
    {
        this.btnGenerate2.Attributes.Add("onclick", "javascript:" +
                      btnGenerate2.ClientID +
                      ".disabled=true;this.value = 'Processing...';" +
                      this.GetPostBackEventReference(btnGenerate2));
        InitializeComponent();
        base.OnInit(e);
    }

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.btnGenerate2.Click += new System.EventHandler(this.btnGenerate_Click);
        this.Load += new System.EventHandler(this.Page_Load);
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.QueryString["AccountID"] != null)
        {
            int accountID = 0;
            int.TryParse(Request.QueryString["AccountID"], out accountID);
                
            // Skip if the account is not passed.
            if (accountID <= 0) return;

        }

        // Prevent the page from redrawing the table if not a first time load.
        if (IsPostBack) return;
        

        //Load dropdowns.
        bindDropDowns();
    }

    #region Dropdown Methods

    /// <summary>
    /// Load and initialize dropdown filters.
    /// </summary>
    private void bindDropDowns()
    {
        this.ddlBillingMethod.DataSource = (from p in idc.PaymentMethods select p);
        this.ddlBillingMethod.DataBind();
        this.ddlBillingMethod.Items.Insert(0, new ListItem("All", ""));

        this.ddlBillingTerm.DataSource = (from b in idc.BillingTerms select b);
        this.ddlBillingTerm.DataBind();
        this.ddlBillingTerm.Items.Insert(0, new ListItem("All", ""));

        this.ddlBrand.DataSource = (from b in idc.BrandSources select b);
        this.ddlBrand.DataBind();
        this.ddlBrand.Items.Insert(0, new ListItem("All", ""));

        this.ddlCustomerType.DataSource = (from c in idc.CustomerTypes
                                           orderby c.CustomerTypeDesc
                                           select new
                                           {
                                               c.CustomerTypeID,
                                               CustomerTypeName = string.Format("{0} ({1})", c.CustomerTypeDesc, c.CustomerTypeCode)
                                           });
        this.ddlCustomerType.DataBind();
        this.ddlCustomerType.Items.Insert(0, new ListItem("All", ""));
    }

    #endregion
       
    #region Grid Methods
    
    /// <summary>
    /// Fired when the page has been changed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvUpcomingRenewals_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvUpcomingRenewals.PageIndex = e.NewPageIndex;
        bindRenewalGrid();

        UpdateRenewalGrid();

    }

    protected void gvUpcomingRenewals_SelectedIndexChanging(object sender, GridViewSelectEventArgs e)
    {
        UpdateRenewalGrid();
    }
    
    /// <summary>
    /// When the column is being sorted.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvUpcomingRenewals_Sorting(object sender, GridViewSortEventArgs e)
    {
        gvUpcomingRenewals.PageIndex = 0;
        bindRenewalGrid();
        UpdateRenewalGrid();
    }

    /// <summary>
    /// Query the data source and bind the Renewal Orders grid.
    /// </summary>
    private void bindRenewalGrid()
    {
        // Create the sql connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the sql command.
        SqlCommand sqlCmd = new SqlCommand();
        //sqlCmd.Connection = sqlConn;
        sqlCmd.CommandType = CommandType.Text; //.StoredProcedure;  //.Text;

        //Revised view has function to determine renewal code (U/E/P) for icon and checkbox display.
        string query = "SELECT * FROM [vw_UpcomingRenewalsBatch]";
        string filters = "";

        // Set the filter parameter for billing method
        if (ddlBillingMethod.SelectedItem != null && ddlBillingMethod.SelectedItem.Text != "All")
        {
            filters += "([BillingMethod] = @billingMethod)";
            sqlCmd.Parameters.Add(new SqlParameter("@billingMethod", ddlBillingMethod.SelectedItem.Text));
        }

        // Set the filter parameter for billing term
        if (ddlBillingTerm.SelectedItem != null && ddlBillingTerm.SelectedItem.Text != "All")
        {
            // Append the AND if needed.
            if (filters != "") filters += " AND ";

            filters += "([BillingTerm] = @billingTerm)";
            sqlCmd.Parameters.Add(new SqlParameter("@billingTerm", ddlBillingTerm.SelectedItem.Text));
        }

        // Set the filter parameter for the brand
        if (ddlCustomerType.SelectedItem != null && ddlCustomerType.SelectedItem.Text != "All")
        {
            // Append the AND if needed.
            if (filters != "") filters += " AND ";

            filters += "([CustomerTypeID] = @customerTypeID)";
            sqlCmd.Parameters.Add(new SqlParameter("@customerTypeID", Convert.ToInt32(ddlCustomerType.SelectedValue)));
        }

        // Set the filter parameter for the brand
        if (ddlBrand.SelectedItem != null && ddlBrand.SelectedItem.Text != "All")
        {
            // Append the AND if needed.
            if (filters != "") filters += " AND ";

            filters += "([BrandName] = @brandName)";
            sqlCmd.Parameters.Add(new SqlParameter("@brandName", ddlBrand.SelectedItem.Text));
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

                    filters += "([RenewalDate] BETWEEN @periodFrom AND @periodTo)";
                    sqlCmd.Parameters.Add(new SqlParameter("periodFrom", dtPeriodFrom));
                    sqlCmd.Parameters.Add(new SqlParameter("periodTo", dtPeriodTo));
                }
            }
        }


        // Append the AND if needed.

        // Add the filters to the query if needed.
        if (filters != "") query += " WHERE " + filters;

        // Add the order by and the direction
        query += " ORDER BY " +
        gvUpcomingRenewals.CurrentSortedColumn +
        ((gvUpcomingRenewals.CurrentSortDirection == SortDirection.Ascending) ? " ASC" : " DESC");

        // Pass the query string to the command

        sqlCmd.Connection = sqlConn;

        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();

        sqlCmd.CommandText = query;
        SqlDataReader reader = sqlCmd.ExecuteReader();

        DataTable dtReviewRenewals = new DataTable();

        // Create the review orders datatable to hold the contents of the order.
        dtReviewRenewals = new DataTable("Renewal Table");

        // Create the columns for the review orders datatable.
        dtReviewRenewals.Columns.Add("Process", Type.GetType("System.Boolean"));
        dtReviewRenewals.Columns.Add("AccountID", Type.GetType("System.Int32"));
        dtReviewRenewals.Columns.Add("AccountName", Type.GetType("System.String"));
        dtReviewRenewals.Columns.Add("BillingMethod", Type.GetType("System.String"));
        dtReviewRenewals.Columns.Add("RenewalDate", Type.GetType("System.DateTime"));
        dtReviewRenewals.Columns.Add("ContractEndDate", Type.GetType("System.DateTime"));
        dtReviewRenewals.Columns.Add("CustomerType", Type.GetType("System.String"));

        //***************************************************************************
        //12/5/12 WK - passed back as varchar(10) - string.  Not accurate to sort.
        //  Now includes LEFT OUTER JOIN - accounts with no orders.
        //***************************************************************************
        dtReviewRenewals.Columns.Add("LastBilled", Type.GetType("System.String"));
        //***************************************************************************

        dtReviewRenewals.Columns.Add("RenewalCode", Type.GetType("System.String"));

        while (reader.Read())
        {

            // Create a new review order row.
            DataRow dr = dtReviewRenewals.NewRow();

            // Fill row details.
            dr["Process"] = false;

            dr["AccountID"] = reader["AccountID"].ToString();
            dr["CustomerType"] = reader["CustomerType"].ToString();
            dr["AccountName"] = reader["AccountName"].ToString();

            //***************************************************************************
            //12/5/12 WK - passed back as varchar(10) - string.  Not accurate to sort.
            //  Now includes LEFT OUTER JOIN - accounts with no orders.
            //***************************************************************************
            dr["LastBilled"] = reader["LastBilled"].ToString();

            dr["RenewalDate"] = reader["RenewalDate"].ToString();
            dr["ContractEndDate"] = reader["ContractEndDate"].ToString();
            dr["BillingMethod"] = reader["BillingMethod"].ToString();
            dr["RenewalCode"] = reader["RenewalCode"].ToString();

            // Add row to the data table.
            dtReviewRenewals.Rows.Add(dr);
        }

        gvUpcomingRenewals.DataSource = dtReviewRenewals;
        gvUpcomingRenewals.DataBind();

        //If Account has been process then turn off checkbox.  Choice icon will be displayed
        //based on RenewalCode ("U" (unprocessed) , "E" (error), or "P" (already processed))
        UpdateRenewalGrid();

    }

    /// <summary>
    /// Bind Renewal Review Grid - processed renewal orders.
    /// </summary>
    /// <param name="RenewalNo"></param>
    private void BindRenewalReviewGrid(string RenewalNo)
    {
        gvReview.DataSource = (from r in idc.RenewalLogs
                               where r.RenewalNo == RenewalNo
                               select new
                               {
                                   r.AccountID,
                                   r.OrderID,
                                   OrderStatusName = r.OrderID.HasValue ? "Success" : "Failed",
                                   r.RenewalAmount,
                                   r.BrandSource.BrandName,
                                   r.BillingTerm.BillingTermDesc,
                                   r.PaymentMethod.PaymentMethodName,
                                   r.ErrorMessage
                               });
        gvReview.DataBind();

        pnlRenewalsTable.Visible = false;
        pnlReview.Visible = true;

    }
    
    /// <summary>
    /// Update Upcoming renewal grid order with Renewal Code and Icon.
    /// </summary>
    private void UpdateRenewalGrid()
    {
        HiddenField hidRenewalCode = null;
        string RenewalCode = null;
        gvUpcomingRenewals.Columns[9].Visible = true;

        foreach (GridViewRow myRow in gvUpcomingRenewals.Rows)
        {
            CheckBox cbProcess = (CheckBox)myRow.FindControl("cbProcess");
            hidRenewalCode = (HiddenField)myRow.FindControl("HidRenewalCode");

            Image imgUnprocessed = (Image)myRow.FindControl("imgUnProcess");
            Image imgError = (Image)myRow.FindControl("imgError");
            Image imgDone = (Image)myRow.FindControl("ImgDone");
            Image imgReprocess = (Image)myRow.FindControl("ImgReprocess");

            //If error in Renewal Generation - DO NOT RELEASE!
            switch (hidRenewalCode.Value)
            {
                case "U":    //Not processed yet.
                    cbProcess.Enabled = true;
                    imgError.Visible = false;
                    imgDone.Visible = false;
                    imgUnprocessed.Visible = false;
                    imgReprocess.Visible = false;
                    break;

                case "E":    //Latest processing had errors.
                    cbProcess.Enabled = true;

                    imgError.Visible = true;
                    imgUnprocessed.Visible = false;
                    imgDone.Visible = false;
                    imgReprocess.Visible = false;
                    break;

                case "R":    //Reprocessed as PO.
                    cbProcess.Enabled = true;

                    imgError.Visible = false;
                    imgUnprocessed.Visible = false;
                    imgDone.Visible = false;
                    imgReprocess.Visible = true;
                    break;

                case "P":   //Already processed.
                    cbProcess.Enabled = false;

                    imgDone.Visible = true;
                    imgError.Visible = false;
                    imgUnprocessed.Visible = false;
                    imgReprocess.Visible = false;
                    break;
            }
        }

        gvUpcomingRenewals.Columns[9].Visible = false;
    }

    #endregion

    #region Button Methods
    /// <summary>
    /// Generate the renewal.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnGenerate_Click(object sender, EventArgs e)
    {   
        // Generate the renewal
        List<Account> accounts = new List<Account>();

        //Get list of checked off Account IDs to process.
        List<int> accountIDs = getAccountIDs();
        var renewal = new Renewals();
        renewal.UserName = ActiveDirectoryQueries.GetUserName();

        //Are there account IDs left to process?
        if (accountIDs.Count > 0)
        {
            //Generate Renewal Number.
            RenewalLogNo = renewal.GenerateRenewalNo();
            lblRenewalNo.Text = RenewalLogNo;
        }
        
        // Perform the Renewal Hold for each of the selected accounts.
        foreach (var accountID in accountIDs)
        {
            var account = (from a in idc.Accounts where a.AccountID == accountID select a).FirstOrDefault();
            DateTime contractStartDate = account.ContractStartDate.Value;
            DateTime contractEndDate = account.ContractEndDate.Value;


            var status = renewal.CreateRenewalHold(accountID, null);
            if (status == null) continue;

            var order = idc.Orders.Where(o => o.OrderID == status.OrderID && o.OrderStatusID != 10).FirstOrDefault();
            Payment payment = (order == null) ? null : order.Payments.FirstOrDefault();

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
                RenewalNo = RenewalLogNo
            });

            // Add the record.
            idc.SubmitChanges();
        }

        UpdateRenewalGrid();

        BindRenewalReviewGrid(RenewalLogNo);

        // when errors are present, display an error message...
        if(ErrorMessageList.Count > 0)
        {
            // Render the errors as a bullets list.
            errorMessage.InnerHtml += "<p>The following errors occurred:</p><ul>";
            foreach (var error in ErrorMessageList)
                errorMessage.InnerHtml += string.Format("<li>{0}</li>", error);
            errorMessage.InnerHtml += "</ul>";

            // display the error message.
            divErrorMessage.Visible = true;
        }
    }

    /// <summary>
    /// Hides the review and completed panels and displays the renewals table.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnBack_Click(object sender, EventArgs e)
    {
        pnlCompleted.Visible = false;
        pnlRenewalsTable.Visible = true;
        pnlReview.Visible = false;

        // Force the screen to redraw.
        bindRenewalGrid();
        UpdateRenewalGrid();
    }
      
    protected void lnkbtnDisplay_Click(object sender, EventArgs e)
    {
        bindRenewalGrid();
        UpdateRenewalGrid();
    }
    
    protected void lnkbtnClear_Click(object sender, EventArgs e)
    {

        //Clear out dropdowns
        this.ddlBillingMethod.SelectedIndex = 0;
        this.ddlBillingTerm.SelectedIndex = 0;
        this.ddlBrand.SelectedIndex = 0;
        this.ddlCustomerType.SelectedIndex = 0;

        txtPeriodFrom.Text = "";
        txtPeriodTo.Text = "";

        // Reset the page to 0
        bindRenewalGrid();
    }

    #endregion
    
    #region Miscellaenous Function

    /// <summary>
    /// Get the order ids from grid view and fills a list.
    /// </summary>
    /// <returns>A list of order ids.</returns>
    private void LogProcesingError(int accountID, int orderID, string renewalNo,
        string errorMsg)
    {
        int OrderStatusID = 8; //RENEWAL HOLD;

        string errMsg = "Account ID:  " + Convert.ToString(accountID) + " - " + errorMsg;

        //get Account Info.
        Account accts = (from a in idc.Accounts where a.AccountID == accountID select a).FirstOrDefault();

        int paymentMethodID = (accts.BillingMethod == "Purchase Order") ? 2 : 1;

        //Add data error to error message list for screen display.
        ErrorMessageList.Add(errMsg);

        //Renewals.InsertRenewalLogsEntry(renewalNo, accountID, orderID, accts.BillingTermID.Value, 
        //    accts.BrandSourceID.Value, paymentMethodID, 0.0m, OrderStatusID, errMsg, 
        //    accts.ContractStartDate.Value, accts.ContractEndDate.Value, true, null);
    }

    /// <summary>
    /// Get the order ids from grid view and fills a list.
    /// </summary>
    /// <returns>A list of order ids.</returns>
    private List<int> getAccountIDs()
    {

        // Build list of orders to process
        List<int> accountIDs = new List<int>();

        // Loop through the grid to get the order Ids
        foreach (GridViewRow gvr in this.gvUpcomingRenewals.Rows)
        {
            // Find the checkbox
            CheckBox cbProcess = (CheckBox)gvr.Cells[0].FindControl("cbProcess");

            // Determine if the checkbox was checked.
            if (!cbProcess.Checked) continue;
            
            HiddenField hfAccountID = (HiddenField)gvr.Cells[1].FindControl("hfAccountID");
            int accountID = int.Parse(hfAccountID.Value);

            DateTime contractEndDate = Convert.ToDateTime(gvr.Cells[5].Text);

            var renewLog = (from r in idc.RenewalLogs
                            where r.AccountID == accountID && r.OrgContractEndDate == contractEndDate
                            select r).ToList();


            // Store checked account IDs only if not processed yet in RenewalLogs table.
            if (renewLog.Count == 0)
            {
                accountIDs.Add(accountID);
                cbProcess.Checked = false;
            }
        }

        return accountIDs;
    }
 
    #endregion
}
