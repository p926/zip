using System;
using System.Data.Linq;
using System.Linq;
using System.Web.UI.WebControls;

using Instadose.Data;
using Instadose;
using System.Collections.Generic;
using Instadose.Processing;
using Instadose.Integration;
using System.Web.UI;

public partial class InstaDose_CustomerService_OrderQueue : System.Web.UI.Page
{
    // Create the Database Reference.
    private InsDataContext idc = new InsDataContext();

    // Error Message List.
    public List<string> errorMessages = new List<string>();
    public List<string> successfullyReleasedOrderIDs = new List<string>();

    protected void Page_Load(object sender, EventArgs e)
    {
        if (IsPostBack) return;

        BindControls();
        BindOpenOrdersGrid();
    }

    #region DROPDOWN METHODS
    private void BindControls()
    {
        this.ddlDealers.DataSource = (from d in idc.Dealers
                                 where d.Active
                                 orderby d.DealerName
                                 select new
                                 {
                                     DealerName = string.Format("{0} ({1})", d.DealerName, d.DealerID),
                                     d.DealerID
                                 });
        this.ddlDealers.DataBind();
        this.ddlDealers.Items.Insert(0, new ListItem("-- All Dealers --", "0"));

        // Hide ICCare Dealers from DropDownList.
        this.lblICCareDealers.Visible = false;
        this.ddlDealers.Visible = false;
    }

    protected void ddlBrand_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Remove any previous entries of "ADDON" for Order Type from DropDownList.
        this.lblICCareDealers.Visible = false;
        this.ddlDealers.Visible = false;

        // 8/2013 WK - IF ICCare Brand is selected from Brand DropDownList, add "ADDON" for Order Type DropDownList.
        if (this.ddlBrand.SelectedItem.Text == "IC Care")
        {
            // Show ICCare Dealers DropDownList.
            this.lblICCareDealers.Visible = true;
            this.ddlDealers.Visible = true;
        }

        this.gvOpenOrders.PageIndex = 0;
        BindOpenOrdersGrid();
    }

    protected void ddlOrderType_SelectedIndexChanged(object sender, EventArgs e)
    {
        this.gvOpenOrders.PageIndex = 0;
        BindOpenOrdersGrid();
    }
    #endregion

    #region DATAGRID METHODS
    /// <summary>
    /// Query the Database and Bind the DataGrid.
    /// </summary>
    private void BindOpenOrdersGrid()
    {
        var openOrders = (from o in idc.Orders                          
                          where o.OrderStatusID <= 2
                          select new
                          {
                              o.OrderID,
                              o.AccountID,
                              o.Account.AccountName,
                              o.BrandSourceID,
                              OrderTotal = (decimal?)o.OrderDetails.Sum(od => od.Price * od.Quantity) + o.ShippingCharge,
                              o.CurrencyCode,
                              o.PONumber,
                              o.PORequired,
                              o.Location.BillingCompany,
                              o.Location.ShippingCompany,
                              CreatedDate = o.OrderDate,
                              PaymentType = o.Payments.FirstOrDefault().PaymentMethod.PaymentMethodName,
                              o.ReferralCode,
                              o.OrderType,
                              DealerID = o.Account.UnixID,
                              IsAccountActive = o.Account.Active
                          });

        // Filter the Order Types.
        if (this.ddlOrderType.SelectedItem.Value != "")
            openOrders = openOrders.Where(o => o.OrderType == this.ddlOrderType.SelectedValue);

        // Filter the Brands.
        if (this.ddlBrand.SelectedItem.Value != "0")
            openOrders = openOrders.Where(o => o.BrandSourceID == int.Parse(this.ddlBrand.SelectedValue));

        // Filter the Dealers.
        if (this.ddlDealers.SelectedItem != null && this.ddlBrand.SelectedItem.Value == "3" & this.ddlDealers.SelectedItem.Value != "0")
            openOrders = openOrders.Where(o => o.DealerID == this.ddlDealers.SelectedValue);

        var asc = gvOpenOrders.CurrentSortDirection == SortDirection.Ascending;

        // Sort the List based on the Column selected.
        switch (this.gvOpenOrders.CurrentSortedColumn)
        {
            case "CreatedDate":
                openOrders = asc ? openOrders.OrderBy(o => o.CreatedDate) : openOrders.OrderByDescending(o => o.CreatedDate);
                break;
            case "AccountID":
                openOrders = asc ? openOrders.OrderBy(o => o.AccountID) : openOrders.OrderByDescending(o => o.AccountID);
                break;
            case "OrderID":
                openOrders = asc ? openOrders.OrderBy(o => o.OrderID) : openOrders.OrderByDescending(o => o.OrderID);
                break;
            case "OrderType":
                openOrders = asc ? openOrders.OrderBy(o => o.OrderType) : openOrders.OrderByDescending(o => o.OrderType);
                break;
            case "AccountName":
                openOrders = asc ? openOrders.OrderBy(o => o.AccountName) : openOrders.OrderByDescending(o => o.AccountName);
                break;
            case "BillingCompany":
                openOrders = asc ? openOrders.OrderBy(o => o.BillingCompany) : openOrders.OrderByDescending(o => o.BillingCompany);
                break;
            case "ReferralCode":
                openOrders = asc ? openOrders.OrderBy(o => o.ReferralCode) : openOrders.OrderByDescending(o => o.ReferralCode);
                break;
            case "PaymentType":
                openOrders = asc ? openOrders.OrderBy(o => o.PaymentType) : openOrders.OrderByDescending(o => o.PaymentType);
                break;
        }
                
        // Bind the results to the Data GridView.
        this.gvOpenOrders.DataSource = openOrders;
        this.gvOpenOrders.DataBind();

        // IF no Open Orders, DISABLE the Release Button.
        btnReleaseOrders.Visible = openOrders.Count() > 0;
    }

    /// <summary>
    /// When the column(s) is/are being sorted.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvOpenOrders_Sorting(object sender, GridViewSortEventArgs e)
    {
        this.gvOpenOrders.PageIndex = 0;
        BindOpenOrdersGrid();
    }

    /// <summary>
    /// Fired when the page has been changed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvOpenOrders_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        this.gvOpenOrders.PageIndex = e.NewPageIndex;
        BindOpenOrdersGrid();
    }
    #endregion

    #region BUTTON METHODS
    /// <summary>
    /// Release selected orders - IC Care and/or non-ICCare and process.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnReleaseOrders_Click(object sender, EventArgs e)
    {
        //Turn off delete messages if any.
        var processedOrderIDs = new List<int>();

        // Loop through the Open Orders to build a List of the Orders to process.
        foreach(GridViewRow row in this.gvOpenOrders.Rows)
        {
            CheckBox cbSelected = (CheckBox)row.FindControl("cbSelected");
            HiddenField hfOrderID = (HiddenField)row.FindControl("hfOrderID");
            TextBox txtPONumber = (TextBox)row.FindControl("txtPONumber");
            string poNumber = (txtPONumber.Text == "NEED PO") ? "" : txtPONumber.Text;

            if (!cbSelected.Checked) continue;

            // Load the Order record.
            int orderID = int.Parse(hfOrderID.Value);
            var order = idc.Orders.Where(o => o.OrderID == orderID && o.OrderStatusID != 10).FirstOrDefault();

            // Skip Orders that have already been processed.
            if (order.OrderStatusID > 2) continue;

            // Add the processed OrderID.
            processedOrderIDs.Add(orderID);

            // Ensure the PO Number was provided for IC Care Orders.
            if (order.BrandSourceID == 3 && poNumber == "")
            {
                errorMessages.Add(string.Format("Order# {0} requires a PO Number.", order.OrderID));
                continue;
            }

            // Updated On:  08/05/2016
            // Updated By:  Anuradha Nandi
            // Description: If the PO# is "RENEWAL", then DO NOT ALLOW the release of the Order!
            if (order.BrandSourceID == 3 && poNumber == "RENEWAL")
            {
                errorMessages.Add(string.Format("Order# {0} cannot be released because PO# is RENEWAL.", order.OrderID));
                continue;
            }

            // Handle IC Care orders that require a PO.
            if (order.BrandSourceID == 3 && order.PORequired)
            {
                // WHY IS THIS A RENEWAL?!
                var r = new Renewals();
                var status = r.CompleteICCareOrder(orderID, poNumber);

                if(!status.RenewalComplete)
                {
                    errorMessages.Add(string.Format("Order# {0} {1}", orderID, status.ErrorMessage));
                    continue;
                }
            }
            else
            {
                // Successful MAS migration - Release Order.
                //order.OrderStatusID = 3;
                order.OrderStatusID = order.OrderType == "NO REPLACEMENT" ? 4 : 3;

                // Update the PO Number if needed.
                if (poNumber != "")
                {
                    order.PONumber = poNumber;
                    order.PORequired = false;
                }

                // Save all changes to the Orders.
                idc.SubmitChanges();

                // Add the OrderID to the Success Message (formatted).
                successfullyReleasedOrderIDs.Add(string.Format("Order# {0}", orderID));

                // Otherwise treat as a normal Order.
                if(!MAS.SendOrderToMAS(orderID, ActiveDirectoryQueries.GetUserName()))
                {
                    errorMessages.Add(string.Format("Order# {0} Could not be sent to MAS.", orderID));
                    continue;
                }
            }
        }

        gvReview.DataSource = (from o in idc.Orders
                               where processedOrderIDs.Contains(o.OrderID)
                               select new
                               {
                                   o.OrderID,
                                   OrderStatusName = o.OrderStatusID < 3 ? "Failed" : "Released",
                                   o.AccountID,
                                   o.Account.AccountName,
                                   o.Location.BillingCompany,
                                   o.Account.BrandSource.BrandName,
                                   o.Account.BillingTerm.BillingTermDesc,
                                   o.Payments.First().PaymentMethod.PaymentMethodName
                               });

        this.gvReview.DataBind();

        this.pnlOrdersTable.Visible = false;

        // Display all Success/Error(s).
        if (errorMessages.Count == 0)
        {
            this.divSuccessMessage.Visible = true;
            this.spnSuccessMessage.InnerText = string.Format("The following Order(s) have been successfully released;");
            this.blltlstSuccesses.DataSource = successfullyReleasedOrderIDs;
            this.blltlstSuccesses.DataBind();
        }
        else
        {
            this.divErrorMessage.Visible = true;
            this.spnErrorMessage.InnerText = string.Format("The following errors have occurred;");
            this.blltlstErrors.DataSource = errorMessages;
            this.blltlstErrors.DataBind();
        }

        this.pnlReview.Visible = true;
    }

    protected void btnRefreshPage_Click(object sender, EventArgs e)
    {
        this.gvOpenOrders.DataBind();
    }

    /// <summary>
    /// FUNCTIONALITY REMOVED PER CRAIG YUROSKO ON 11/10/2014.
    /// Removed By: Anuradha Nandi
    /// Reason:     Eliminate possibilty of CS from accidentally deleting Orders directly from Instadose Portal.
    ///             Delete Order - used only if order has NOT been shipped, fullfilled, etc.!
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    //protected void btnDelete_Click(object sender, ImageClickEventArgs e)
    //{
    //    ImageButton btnDelete = (ImageButton)sender;

    //    int orderID = 0;
    //    if (!int.TryParse(this.btnDelete.CommandArgument, out orderID)) return;

    //    int orderStatusID = (from o in idc.Orders
    //                         where o.OrderID == orderID
    //                         select o.OrderStatusID).FirstOrDefault();

    //    // IF order has been processed - Fullfilled; Shipped, etc. - CANNOT DELETE!
    //    if (orderStatusID > 2) return;

    //    // Delete Order in question since it has not been Shipped/Fulfilled, etc.
    //    OrderHelpers.DeleteOrder(orderID);
                
    //    BindOpenOrdersGrid();
    //}

    /// <summary>
    /// Update PO Numbers on grid based on PORequired value in Order table.  
    /// If yes, then DO ALLOW updated to the PO number.
    /// </summary>
    protected void gvOpenOrders_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow) return;

        dynamic data = e.Row.DataItem;

        CheckBox cbSelected = (CheckBox)e.Row.FindControl("cbSelected");
        TextBox txtPONumber = (TextBox)e.Row.FindControl("txtPONumber");
        Label lblPONumber = (Label)e.Row.FindControl("lblPONumber");
        Label lblOrderTotal = (Label)e.Row.FindControl("lblOrderTotal");
        Label lblPaymentType = (Label)e.Row.FindControl("lblPaymentType");
        Label lblDealerID = (Label)e.Row.FindControl("lblDealerID");

        if (data.PORequired && data.PONumber == "") 
            txtPONumber.Text = "NEED PO";

        txtPONumber.Visible = data.IsAccountActive ? data.PORequired : false;
        lblPONumber.Visible = data.IsAccountActive ? !data.PORequired : true;
        lblPaymentType.Text = data.PaymentType == "Credit Card" ? "CC" : "PO";
        lblDealerID.Text = data.DealerID == "0" ? "" : data.DealerID;

        try
        {
            lblOrderTotal.Text = Currencies.CurrencyToString(data.OrderTotal, data.CurrencyCode);
        }
        catch
        {
            cbSelected.Enabled = false;
            lblOrderTotal.Text = "Currency Error";
            lblOrderTotal.Style.Add("color", "red");
        }

        
    }

    /// <summary>
    /// Update RenewalLogs Entry for updated OrderStatusID (4) if successful.
    /// </summary>
    /// <returns></returns>
    private void updateRenewalLogsOrderStatus(int orderID, string errorMsg,
        bool isError)
    {

        // Query the database for the row to be updated.
        RenewalLogs renew =
            (from r in idc.RenewalLogs
            where r.OrderID == orderID
            select r).FirstOrDefault();

        //Update RenewalLogs based on error or not.
        if (isError)
        {
            // set OrderID to null and update to error 
            // message.
             if (renew != null)
            {
                renew.OrderStatusID = 1;
                renew.ErrorMessage = errorMsg;
            }
        }
        else
        {
            // Update message to successful - Status - Shipped (4).
            if (renew != null)
            {
                renew.OrderStatusID = 4;
                renew.ErrorMessage = null;
            }
        }

        // Update RenewalLogs for this order.
        idc.SubmitChanges();
    }

    protected void btnBack_Click(object sender, EventArgs e)
    {
        this.pnlReview.Visible = false;

        // Refresh Open Orders Data GridView.
        this.pnlOrdersTable.Visible = true;
        BindOpenOrdersGrid();

        // Reset both List<string> successfullyReleasedOrderIDs and List<string> errorMessages.
        this.successfullyReleasedOrderIDs.Clear();
        this.errorMessages.Clear();

        // Hide Error/Success Message(s) Layer (<div>).
        // Success Message(s).
        this.divSuccessMessage.Visible = false;
        this.spnSuccessMessage.InnerText = string.Empty;
        // Error Message(s).
        this.divErrorMessage.Visible = false;
        this.spnErrorMessage.InnerText = string.Empty;
    }
    #endregion
}