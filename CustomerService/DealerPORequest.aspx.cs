using System;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;

using System.Net.Mail;

using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Text;
using System.Text.RegularExpressions;

using System.Data.SqlClient;

using Instadose;
using Instadose.Data;
using Instadose.Integration;
using Instadose.Processing;


public partial class InstaDose_CustomerService_DealerPORequest : System.Web.UI.Page
{  
    
    #region Event Methods

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            loadDealers();
            // Bind the grid.
            bindOpenOrdersGrid();
        }
    }

    ///// <summary>
    ///// When the column is being sorted.
    ///// </summary>
    ///// <param name="sender"></param>
    ///// <param name="e"></param>
    protected void gvOpenOrders_Sorting(object sender, GridViewSortEventArgs e)
    {
        gvOpenOrders.PageIndex = 0;
        bindOpenOrdersGrid();
    }

    /// <summary>
    /// Called from many objects, force binds the gridview.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void bindGridView(object sender, EventArgs e)
    {
        // Reset the page to 0
        gvOpenOrders.PageIndex = 0;

        // Bind the gridview
        bindOpenOrdersGrid();
    }

    /// <summary>
    /// Fired when the page has been changed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvOpenOrders_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvOpenOrders.PageIndex = e.NewPageIndex;
        bindOpenOrdersGrid();
    }

    /// <summary>
    /// Hide Renewal Record from grid view command
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvOpenOrders_RowCommand(object sender, GridViewCommandEventArgs e)
    {
    }

    protected void btnRefreshPage_Click(object sender, EventArgs e)
    {
        gvOpenOrders.DataBind();
    }

    protected void btnClose_container(object sender, EventArgs e)
    {        
        gvOpenOrders.DataBind();
    }
    
    protected void btnViewDealerPORequest_Click(object sender, EventArgs e)
    {
        var orderIDs = getSelectedOrderIDs();
        // ensure at least one was selected
        if (orderIDs.Count == 0) return;
        
        // Build the parameters to pass to the Report
        var p = new Dictionary<string, string>();
        p.Add("OrderIDs", string.Join(",", orderIDs));

        // Generate the PDFs
        var mpdf = new MirionPDFConverter();
        mpdf.WritePdfResponse("DealerPORequests.aspx", p, "DealerPORequests.pdf");
    }
   
    protected void btnBack_Click(object sender, EventArgs e)
    {
        bindOpenOrdersGrid();
    }

    protected void DateRange_TextChanged(object sender, EventArgs e)
    {
        // Exit/Return if no Text.
        TextBox txtBox = (TextBox)sender;

        if (txtBox.Text == "") return;

        DateTime dtFromDate = DateTime.Now;
        DateTime dtToDate = DateTime.Now;

        // Test to see if DateTimes are valid.
        if (!DateTime.TryParse(txtOrderDateFrom.Text, out dtFromDate)) return;
        if (!DateTime.TryParse(txtOrderDateTo.Text, out dtToDate)) return;

        // Test to ensure the From Date is LESS THAN the To Date.
        if (dtFromDate > dtToDate)
        {
            txtBox.Text = "";
            return;
        }

        // Reset the GridView page to 0.
        gvOpenOrders.PageIndex = 0;

        // Rebind the GridView.
        bindOpenOrdersGrid();
    }

    protected void lnkbtnClearFilters_Click(object sender, EventArgs e)
    {
        ddlDealers.SelectedIndex = 0;
        ddlOrderType.SelectedIndex = 0;
        txtOrderDateFrom.Text = "";
        txtOrderDateTo.Text = "";

        bindOpenOrdersGrid();
    }

    protected void BillingType_CheckedChanged(object sender, EventArgs e)
    {
        loadDealers();
        // Bind the grid.
        bindOpenOrdersGrid();
    }

    #endregion

    /// <summary>
    /// Return a list of Order IDs of the selected GridView rows.
    /// </summary>
    /// <returns></returns>
    private List<int> getSelectedOrderIDs()
    {
        List<int> orderIDs = new List<int>();

        // Loop through the grid to get the order Ids
        foreach (GridViewRow gvr in this.gvOpenOrders.Rows)
        {
            // Find the checkbox
            CheckBox cbRow = (CheckBox)gvr.Cells[0].FindControl("cbSelected");

            // Determine if the checkbox was checked.
            if (!cbRow.Checked) continue;

            HiddenField hfOrderID = (HiddenField)gvr.Cells[0].FindControl("hfOrderID");

            // Add the order to the list.
            orderIDs.Add(int.Parse(hfOrderID.Value));
        }

        return orderIDs;

    }
    
    /// <summary>
    /// Query the data source and bind the data grid.
    /// </summary>
    private void bindOpenOrdersGrid()
    {
        var idc = new InsDataContext();

        // Query the nessesary data and render it to the page.
        var orders = (from o in idc.Orders
                     join dealer in idc.Dealers on o.Account.UnixID equals dealer.DealerID.ToString()
                     where
                         o.PORequired &&
                         o.Account.Active &&
                         o.OrderStatusID <= 2 &&
                         o.Account.UseLocationBilling == (rbLocationBilling.Checked) && // filter for use location billing
                         o.OrderDetails.Count() > 0 // ensure the order has details.
                     select new
                     {
                         DealerID = dealer.DealerID,
                         o.Account.AccountName,
                         o.AccountID,
                         o.OrderID,
                         o.CreatedBy,
                         o.PONumber,
                         o.ReferralCode,
                         o.Location.BillingCompany,
                         o.Location.ShippingCompany,
                         o.CurrencyCode,
                         o.OrderDate,
                         o.OrderType,
                         PaymentMethod = o.Payments.Select(p=>p.PaymentMethod.PaymentMethodName == "Purchase Order" ? "PO" : "CC").FirstOrDefault(),
                         OrderTotal = o.OrderDetails.Sum(od => (int?)od.Quantity * (decimal?)od.Price) + o.ShippingCharge
                     });
        
        // If the order types are filtered...
        if (ddlOrderType.SelectedValue != "")
            orders = orders.Where(o => o.OrderType == ddlOrderType.SelectedValue);

        // If the dealers are filtered...
        if (ddlDealers.SelectedValue != "")
        {
            int dealerID = 0;
            int.TryParse(ddlDealers.SelectedValue, out dealerID);

            // Filter the orders for the dealer
            orders = orders.Where(o => o.DealerID == dealerID);
        }

        // if there is a date range...
        DateTime startDate = DateTime.MinValue;
        DateTime.TryParse(txtOrderDateFrom.Text, out startDate);

        // filter for a created date greater the start
        if (startDate > DateTime.MinValue)
            orders = orders.Where(o => o.OrderDate >= startDate);

        // filter for the end date if set.
        DateTime endDate = DateTime.MinValue;
        DateTime.TryParse(txtOrderDateTo.Text, out endDate);

        // filter for a created date less than the end.
        if (endDate > DateTime.MinValue)
            orders = orders.Where(o => o.OrderDate <= endDate.AddDays(1));

        // Order by
        var asc = gvOpenOrders.CurrentSortDirection == SortDirection.Ascending;

        // Sort the list based on the column
        switch (gvOpenOrders.CurrentSortedColumn)
        {
            case "OrderDate":
                orders = asc ? orders.OrderBy(o => o.OrderDate) : orders.OrderByDescending(o => o.OrderDate);
                break;
            case "AccountID":
                orders = asc ? orders.OrderBy(o => o.AccountID) : orders.OrderByDescending(o => o.AccountID);
                break;
            case "OrderID":
                orders = asc ? orders.OrderBy(o => o.OrderID) : orders.OrderByDescending(o => o.OrderID);
                break;
            case "OrderType":
                orders = asc ? orders.OrderBy(o => o.OrderType) : orders.OrderByDescending(o => o.OrderType);
                break;
            case "AccountName":
                orders = asc ? orders.OrderBy(o => o.AccountName) : orders.OrderByDescending(o => o.AccountName);
                break;
            case "BillingCompany":
                orders = asc ? orders.OrderBy(o => o.BillingCompany) : orders.OrderByDescending(o => o.BillingCompany);
                break;
            case "ReferralCode":
                orders = asc ? orders.OrderBy(o => o.ReferralCode) : orders.OrderByDescending(o => o.ReferralCode);
                break;
            case "PONumber":
                orders = asc ? orders.OrderBy(o => o.PONumber) : orders.OrderByDescending(o => o.PONumber);
                break;
            case "PaymentMethod":
                orders = asc ? orders.OrderBy(o => o.PaymentMethod) : orders.OrderByDescending(o => o.PaymentMethod);
                break;
            case "OrderTotal":
                orders = asc ? orders.OrderBy(o => o.OrderTotal) : orders.OrderByDescending(o => o.OrderTotal);
                break;
        }

        // Bind the results to the gridview
        gvOpenOrders.DataSource = orders;
        gvOpenOrders.DataBind();
    }

    private void loadDealers()
    {
        ddlDealers.Items.Clear();

        // Connect to the sql database
        InsDataContext idc = new InsDataContext();
        
        // Select a list of dealers based on open 
        // orders for active accounts under IC Care
        var dealerIDs = (from o in idc.Orders
                         where
                             o.OrderStatusID <= 2 &&
                             o.PORequired &&
                             o.Account.Active &&
                             o.Account.BrandSourceID == 3 &&
                             o.Account.UseLocationBilling == rbLocationBilling.Checked &&
                             o.Account.UnixID != null // Yes, Unix ID is the Dealer ID
                         select int.Parse(o.Account.UnixID)).ToList();

        // Get the list of dealers
        var dealers = (from d in idc.Dealers
                       where dealerIDs.Contains(d.DealerID)
                       orderby d.DealerName
                       select new
                       {
                           DisplayName = string.Format("{0} ({1})", d.DealerName, d.DealerID),
                           d.DealerID
                       }).ToList();

        // bind the list of dealers
        ddlDealers.DataSource = dealers;
        ddlDealers.DataBind();

        // Display a placeholder for no orders
        if (dealers.Count() == 0)
        {
            ddlDealers.Items.Add(new ListItem("-- There are no pending orders --", ""));
            return;
        }
    }

    protected void lbtnShowAll_Click(object sender, EventArgs e)
    {
        gvOpenOrders.PageSize = 10000;
        bindOpenOrdersGrid();
        lbtnShowAll.Visible = false;
    }
}
  
