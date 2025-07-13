using System;
using System.Collections.Generic;
using System.Configuration;

using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

using Instadose;
using Instadose.Data;
using Instadose.Integration;
using Instadose.Processing;

public partial class InstaDose_InformationFinder_Details_ReviewOrder : System.Web.UI.Page
{

    public int OrderID = 0;

    public static string orderTotal;
    public static string shippingTotal;

    InsDataContext idc = new InsDataContext();
    MASDataContext mdc = new MASDataContext();

    private const String STR_FEDEX_URL =
      "http://fedex.com/Tracking?ascend_header=1&clienttype=dotcom&cntry_code=us&language=english&tracknumbers=";

   
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.QueryString["ID"] == null)
        {
            Response.Redirect("../InformationFinder/Default.aspx");
        }
        else
        {
            int.TryParse(Request.QueryString["ID"], out OrderID);
        }

        if (Page.IsPostBack) return;

        try
        {
            HyperLink hlTrackingNumber = (HyperLink)this.fvOrderHeader.FindControl("hlTrackingNumber");
            if (isNumber(hlTrackingNumber.Text))
                hlTrackingNumber.NavigateUrl = STR_FEDEX_URL + hlTrackingNumber.Text;

            //Display Totals - US and foreign (if appliacable).
            displayTotals(OrderID);

            // These are the current NEW ProductSKU's for Instadose 1.
            // Please add to this STRING ARRAY as necessary for future additions as needed.
            //string[] productSKUs = { "INSTA10", "INSTA10-B", "CASE CVR BLK", "CASE CVR BLUE", "CASE CVR GREEN", "CASE CVR PINK", "CASE CVR SLVR" };

            // Updated 07/06/2015 by Anuradha Nandi
            // Looks to see if the Order Details of a given OrderID has items listed for Instadose 1 IMI.
            // IF TRUE then IsConverted = TRUE
            // ELSE IsConverted = FALSE
            //int getRecordCount = (from od in idc.OrderDetails
            //                      join p in idc.Products on od.ProductID equals p.ProductID
            //                      where od.OrderID == OrderID
            //                      && productSKUs.Contains(p.ProductSKU)
            //                      select od.OrderID).Count();

            //// Display Order Details accordingly.
            //if (getRecordCount >= 1)
            //{
            //    bindOrderDetailGrid(OrderID, true);
            //}
            //else
            //{
            //    bindOrderDetailGrid(OrderID, false);
            //}

            BindOrderDetailsGridView(OrderID);
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }

    }
    public string YesNo(object boolean)
    {
        try
        {
            return Convert.ToBoolean(boolean) ? "Yes" : "No";
        }
        catch { return "No"; }
    }
    public bool isNumber(String value)
    {
        try
        {
            double test = double.Parse(value);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    public string GeneratePackShippingInfo(string strPackageID)
    {
        string returnStr = "";
        var pInfo = (from b in idc.Packages
                     join c in idc.States
                     on b.StateID equals c.StateID
                     join d in idc.Countries
                     on b.CountryID equals d.CountryID
                     where b.PackageID == int.Parse(strPackageID)
                     select new { b, c, d }).FirstOrDefault();

        if (pInfo != null)
        {
            returnStr = (pInfo.b.Company != null && pInfo.b.Company != "") ? pInfo.b.Company + "</br>" : "";
            returnStr += pInfo.b.FirstName + " ";
            returnStr += pInfo.b.LastName + "</br>";
            returnStr += pInfo.b.Address1 ;

            returnStr += (pInfo.b.Address2 != null && pInfo.b.Address2 != "") ?
                         ", " + pInfo.b.Address2 : "";

            returnStr += (pInfo.b.Address3 != null && pInfo.b.Address3 != "") ?
                         ", " + pInfo.b.Address3 : "";
            returnStr += "</br>";

            returnStr += pInfo.b.City + ", ";
            returnStr += pInfo.c.StateAbbrev + " ";
            returnStr += pInfo.b.PostalCode + "</br>";
            returnStr += pInfo.d.CountryName + "</br>";

        }

        return returnStr;
    }

    /// <summary>
    /// checkPODocExistence - check if original PO Request exist in Document table
    ///     for given order.
    /// </summary>
    /// <param name="ID"> ordID </param>
    /// <returns>True/False</returns>
    private Boolean checkForeignCurrency(int ordID)
    {
        bool fcBool = false;

        // Retrieve data from DB 
        var forCur = (from o in idc.Orders
                        where o.OrderID == ordID
                            && o.CurrencyCode != "USD"
                        select o).ToList();
        
        if (forCur.Count != 0)
            fcBool = true;

        return fcBool;

    }

    /// <summary>
    /// Display and format currency totals.
    /// </summary>
    /// <param name="orderID"></param>
    protected void displayTotals(int orderID)
    {
        
        Order order = idc.Orders.Where(o => o.OrderID == orderID && o.OrderStatusID != 10).FirstOrDefault();

        if (order == null)
        {
            VisibleErrors("The Order # you entered has been canceled.");
        }
        else
        {
            //find currency code.
            string currCode = order.CurrencyCode;

            //find currency code.
            string currSymbol = (from c in idc.Currencies where c.CurrencyCode == currCode select c.Symbol).FirstOrDefault();
            //-------------------------------------------------------------

            // Order can be edited.
            if (order.OrderStatusID < 3)
            {
                hlEditOrder.NavigateUrl = string.Format(hlEditOrder.NavigateUrl, orderID);
                editOrder.Visible = true;
            }

            // Order can be cancelled
            if (order.OrderStatusID <= 3)
            {
                btnDeleteOrder.Enabled = true;
            }

            int orderStatusID = order.OrderStatusID;
            int? brandSourceID = ((order.Account.BrandSourceID.HasValue) ? order.Account.BrandSourceID.Value : 0);

            bool allowOrderDeletion = false;
            string alertMessage = "";

            if (orderStatusID == 1 && brandSourceID == 3)
            {
                allowOrderDeletion = true;
            }

            if (orderStatusID == 2 && brandSourceID == 2)
            {
                allowOrderDeletion = true;
            }

            if (orderStatusID == 3)
            {
                allowOrderDeletion = true;
            }

            if (orderStatusID == 4 || orderStatusID == 5 || orderStatusID == 6 || orderStatusID == 7)
            {
                allowOrderDeletion = false;
            }

            if (orderStatusID == 8)
            {
                allowOrderDeletion = true;
            }
            //If this order contains ID+ or ID2 and order is in fulfillment do not allow cancel
            if (orderStatusID > 2 && order.OrderDetails.Any(ord => ord.Product.ProductGroupID > 9))
            {
                allowOrderDeletion = false;
            }
            if (orderStatusID >= 7)
            {
                allowOrderDeletion = false;
            }

            if (allowOrderDeletion == false)
            {
                btnDeleteOrder.Enabled = false;
            }
            else
            {
                btnDeleteOrder.Enabled = true;
            }

            decimal amount = 0;

            System.Web.UI.WebControls.Label lblSubTotal = null;
            System.Web.UI.WebControls.Label lblOrderTotal = null;
            System.Web.UI.WebControls.Label lblOrderTax = null;
            System.Web.UI.WebControls.Label lblShipping = null;
            System.Web.UI.WebControls.Label lblCredit = null;

            //Get current Item Qty and Total for each product from grid.
            lblSubTotal = (System.Web.UI.WebControls.Label)(fvOrderHeader.FindControl("OrderSubtotalLabel"));
            lblOrderTotal = (System.Web.UI.WebControls.Label)(fvOrderHeader.FindControl("OrderTotalLabel"));
            lblOrderTax = (System.Web.UI.WebControls.Label)(fvOrderHeader.FindControl("OrderTaxLabel"));
            lblShipping = (System.Web.UI.WebControls.Label)(fvOrderHeader.FindControl("OrderShippingLabel"));
            lblCredit = (System.Web.UI.WebControls.Label)(fvOrderHeader.FindControl("MiscCreditLabel"));

            lblSubTotal.Text = Currencies.RemoveCurrencySymbol(lblSubTotal.Text, "USD", 50);
            lblOrderTotal.Text = Currencies.RemoveCurrencySymbol(lblOrderTotal.Text, "USD", 50);
            lblOrderTax.Text = Currencies.RemoveCurrencySymbol(lblOrderTax.Text, "USD", 50);
            lblShipping.Text = Currencies.RemoveCurrencySymbol(lblShipping.Text, "USD", 50);
            lblCredit.Text = Currencies.RemoveCurrencySymbol(lblCredit.Text, "USD", 50);

            if (lblCredit.Text != "")
            {

                if (Convert.ToDecimal(lblCredit.Text) > 0)
                {
                    amount = Convert.ToDecimal(lblCredit.Text);

                    //Display foreign and converted US currency amounts.
                    lblCredit.Text = Currencies.CurrencyToString(amount, currCode);
                }
                else
                {
                    lblCredit.Text = lblCredit.Text + " USD";
                }
            }

            if (lblShipping.Text != "")
            {
                if (Convert.ToDecimal(lblShipping.Text) > 0)
                {
                    lblShipping.Text = Currencies.RemoveCurrencySymbol(lblShipping.Text, "USD", 50);
                    amount = Convert.ToDecimal(lblShipping.Text);

                    //Display foreign and converted US currency amounts.
                    lblShipping.Text = Currencies.CurrencyToString(amount, currCode);
                }
                else
                {
                    lblShipping.Text = lblShipping.Text + " USD";
                }
                shippingTotal = lblShipping.Text;
            }

            if (lblOrderTax.Text != "")
            {
                if (Convert.ToDecimal(lblOrderTax.Text) > 0)
                {
                    lblOrderTax.Text = Currencies.RemoveCurrencySymbol(lblOrderTax.Text, "USD", 50);
                    amount = Convert.ToDecimal(lblOrderTax.Text);

                    //Display foreign and converted US currency amounts.
                    lblOrderTax.Text = Currencies.CurrencyToString(amount, currCode);
                }
                else
                {
                    lblOrderTax.Text = lblOrderTax.Text + " USD";
                }
            }

            if (lblSubTotal.Text != "")
            {
                if (Convert.ToDecimal(lblSubTotal.Text) > 0)
                {
                    lblSubTotal.Text = Currencies.RemoveCurrencySymbol(lblSubTotal.Text, "USD", 50);
                    amount = Convert.ToDecimal(lblSubTotal.Text);

                    //Display foreign and converted US currency amounts.
                    lblSubTotal.Text = Currencies.CurrencyToString(amount, currCode);
                }
                else
                {
                    lblSubTotal.Text = lblSubTotal.Text + " USD";
                }
            }


            if (lblOrderTotal.Text != "")
            {
                if (Convert.ToDecimal(lblOrderTotal.Text) > 0)
                {
                    lblOrderTotal.Text = Currencies.RemoveCurrencySymbol(lblOrderTotal.Text, "USD", 50);
                    amount = Convert.ToDecimal(lblOrderTotal.Text);

                    //Display foreign and converted US currency amounts.
                    lblOrderTotal.Text = Currencies.CurrencyToString(amount, currCode);
                }
                else
                {
                    lblOrderTotal.Text = lblOrderTotal.Text + " USD";
                }

                orderTotal = lblOrderTotal.Text;
            }
        }
    }

    //#region FUNCTION -> BIND ORDER DETAILS TO GRIDVIEW.
    ///// <summary>
    ///// Updated on 07/06/2015 by Anuradha Nandi
    ///// Uses the NEW Stored Procedure to display either the Old Product SKU or New Product SKU depending if the Order has been Converted or not.
    ///// </summary>
    ///// <param name="orderid"></param>
    ///// <param name="isconverted"></param>
    //private void bindOrderDetailGrid(int orderid, bool isconverted)
    //{
    //    string unitPrice = "";
    //    int itemQuantity = 0;
    //    string subTotal = "";

    //    string currencyCode = "";

    //    // Create the SQL Connection from the Connection String in the web.config file.
    //    SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
    //        ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

    //    // Create the SQL Command.
    //    SqlCommand sqlCmd = new SqlCommand();
    //    sqlCmd.CommandType = CommandType.StoredProcedure;

    //    sqlCmd.Parameters.Add(new SqlParameter("@OrderID", orderid));
    //    sqlCmd.Parameters.Add(new SqlParameter("@IsConverted", isconverted));

    //    // Pass the query string to the command

    //    sqlCmd.Connection = sqlConn;

    //    bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
    //    if (!openConn) sqlCmd.Connection.Open();

    //    sqlCmd.CommandText = "sp_GetConvertedProductSKUOrderDetails";

    //    SqlDataReader sqlDtRdr = sqlCmd.ExecuteReader();

    //    DataTable dtOrderDetails = new DataTable();

    //    // Create the Order Details DataTable to hold the contents of the order.
    //    dtOrderDetails = new DataTable("Order Details");

    //    // Create the Columns for the Order Details DataTable.
    //    dtOrderDetails.Columns.Add("AccountID", Type.GetType("System.Int32"));
    //    dtOrderDetails.Columns.Add("OrderID", Type.GetType("System.Int32"));
    //    dtOrderDetails.Columns.Add("AccountName", Type.GetType("System.String"));
    //    dtOrderDetails.Columns.Add("Status", Type.GetType("System.String"));
    //    dtOrderDetails.Columns.Add("OrderDate", Type.GetType("System.DateTime"));
    //    dtOrderDetails.Columns.Add("CurrencyCode", Type.GetType("System.String"));
    //    dtOrderDetails.Columns.Add("ProductSKU", Type.GetType("System.String"));
    //    dtOrderDetails.Columns.Add("ProductName", Type.GetType("System.String"));
    //    dtOrderDetails.Columns.Add("UnitPrice", Type.GetType("System.String"));
    //    dtOrderDetails.Columns.Add("ItemQuantity", Type.GetType("System.Int32"));
    //    dtOrderDetails.Columns.Add("SubTotal", Type.GetType("System.String"));

    //    while (sqlDtRdr.Read())
    //    {
    //        currencyCode = sqlDtRdr["CurrencyCode"].ToString();

    //        // Create a NEW Order Details Row.
    //        DataRow dr = dtOrderDetails.NewRow();

    //        // Fill the Row Details.
    //        dr["ProductSKU"] = sqlDtRdr["ProductSKU"].ToString();
    //        dr["ProductName"] = sqlDtRdr["ProductName"].ToString();

    //        // Get & Format UnitPrice.
    //        unitPrice = sqlDtRdr["UnitPrice"].ToString();
    //        unitPrice = Currencies.CurrencyToString(Convert.ToDecimal(unitPrice), currencyCode);

    //        // Get ItemQuantity.
    //        int.TryParse(sqlDtRdr["ItemQuantity"].ToString(), out itemQuantity);

    //        // Get Sub-Total.
    //        subTotal = sqlDtRdr["SubTotal"].ToString();
    //        subTotal = Currencies.CurrencyToString(Convert.ToDecimal(subTotal), currencyCode);

    //        // Display UnitPrice, Quantity, and Sub-Total.
    //        dr["UnitPrice"] = unitPrice;
    //        dr["ItemQuantity"] = itemQuantity.ToString();
    //        dr["SubTotal"] = subTotal;

    //        // ADD Row to the DataTable.
    //        dtOrderDetails.Rows.Add(dr);
    //    }

    //    this.gvOrderDetails.DataSource = dtOrderDetails;
    //    this.gvOrderDetails.DataBind();
    //}
    //#endregion

    #region FUNCTION -> BIND ORDER DETAILS TO GRIDVIEW.
    /// <summary>
    /// Updated on 07/06/2015 by Anuradha Nandi
    /// Uses the NEW Stored Procedure to display either the Old Product SKU or New Product SKU depending if the Order has been Converted or not.
    /// </summary>
    /// <param name="orderid"></param>
    private void BindOrderDetailsGridView(int orderid)
    {
        string price = "";
        string amount = "";

        // Get CurrencyCode associated with OrderID.
        string currencyCode = (from o in idc.Orders
                               where o.OrderID == orderid
                               select o.CurrencyCode).FirstOrDefault();

        // Create the SQL Connection from the ConnectionString in the web.config file.
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.StoredProcedure;

        sqlCmd.Parameters.Add(new SqlParameter("@OrderNo", orderid));

        // Pass the QueryString to the SQL Command.
        sqlCmd.Connection = sqlConn;

        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();

        //***************************************************************************
        // 8/2013 WK - AccountDiscounts is in place but not used since it will NOT
        // be accurate.   Product discounts need to be included in OrderDetails
        // if it is be used.
        //***************************************************************************
        sqlCmd.CommandText = "sp_if_GetOrderDetailsByOrderNo";

        SqlDataReader reader = sqlCmd.ExecuteReader();

        DataTable dtOrderDetails = new DataTable();

        // Create the Review Order Details DataTable to hold the contents of the OrderDetails.
        dtOrderDetails = new DataTable("Order Details Table");

        // Create the Columns for the DataTable.
        dtOrderDetails.Columns.Add("SKU", Type.GetType("System.String"));
        dtOrderDetails.Columns.Add("ProductName", Type.GetType("System.String"));
        dtOrderDetails.Columns.Add("ProductVariant", Type.GetType("System.String"));
        dtOrderDetails.Columns.Add("Price", Type.GetType("System.String"));
        dtOrderDetails.Columns.Add("Quantity", Type.GetType("System.Int32"));
        dtOrderDetails.Columns.Add("LineTotal", Type.GetType("System.String"));

        while (reader.Read())
        {
            // Create a new Row.
            DataRow dr = dtOrderDetails.NewRow();

            // Fill Row's details.
            dr["SKU"] = reader["SKU"].ToString();
            dr["ProductName"] = reader["ProductName"].ToString();
            dr["ProductVariant"] = reader["ProductVariant"].ToString();

            //Format [Unit] Price.
            price = reader["Price"].ToString();
            price = Currencies.CurrencyToString(Convert.ToDecimal(price), currencyCode);
            dr["Price"] = price;

            dr["Quantity"] = reader["Quantity"].ToString();

            //Format each ProductSKU's Subtotal Amount.
            amount = reader["LineTotal"].ToString();
            amount = Currencies.CurrencyToString(Convert.ToDecimal(amount), currencyCode);

            dr["LineTotal"] = amount;

            // ADD Row to the DataTable.
            dtOrderDetails.Rows.Add(dr);
        }

        gvOrderDetails.Columns[3].HeaderText = "Unit Price".PadLeft(10);

        gvOrderDetails.DataSource = dtOrderDetails;
        gvOrderDetails.DataBind();
    }
    #endregion

    public string GeneratePackSerialNo(string strPackageID)
    {
        string returnStr = "";
        var Ser = (from a in idc.PackageOrderDetails
                   join b in idc.DeviceInventories
                   on a.DeviceID equals b.DeviceID
                   where a.PackageID == int.Parse(strPackageID)
                   select b.SerialNo).ToList();
        if (Ser.Count > 0)
        {
            returnStr += "Total: " + Ser.Count.ToString() + "</br>";
            
            foreach (var v in Ser)
            {
                returnStr += "#" + v.ToString() + ", ";
            }
            int lastIndex = returnStr.LastIndexOf(",");
            if (lastIndex > 0)
                returnStr = returnStr.Substring(0, lastIndex);
        }
        return returnStr;
    }

    protected void btnBack_Click(object sender, EventArgs e)
    {
        int.TryParse(Request.QueryString["ID"].ToString(), out OrderID);

        var thisOrder = (from o in idc.Orders
                         where o.OrderID == OrderID
                         select o).FirstOrDefault();

        if (thisOrder == null) return;

        Page.Response.Redirect("../InformationFinder/Details/Account.aspx?ID=" + thisOrder.AccountID.ToString() + "#Order_tab");
    }
    
    protected void lnkbtnPerforma_Click(object sender, EventArgs e)
    {
        Page.Response.Redirect("../InformationFinder/GenerateProformaInvoice.aspx?ID=" + this.OrderID.ToString());
    
    }
    protected void lnkbtnOrderAck_Click(object sender, EventArgs e)
    {
        Page.Response.Redirect("../InformationFinder/GenerateOrderAck.aspx?ID=" + this.OrderID.ToString()
            + "&OrderTotal=" + orderTotal + "&ShippingTotal=" + shippingTotal);
    }
    protected void gvOrderDetails_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    protected void btnDeleteOrder_Click(object sender, EventArgs e)
    {
        InvisibleErrors();
        InvisibleSuccesses();

        int orderID = 0;
        Label lblOderNo = (Label)this.fvOrderHeader.FindControl("OrderNoLabel");
        Label lblStatus = (Label)this.fvOrderHeader.FindControl("Label1");
        if (int.TryParse(lblOderNo.Text.Trim(), out orderID))
        {
            try
            {
                // Modified 09/29/2015 by Anuradha Nandi
                // Additional Instructions have been added to the Success Message once the Order has been deleted.
                // To be followed-up/through by User who is deleting the order.

                // GET OrderType, OrderStatusID, and BrandSourceID.
                var getOrderDetails = (from o in idc.Orders
                                       where o.OrderID == orderID
                                       select o).FirstOrDefault();
                if (getOrderDetails == null)
                    throw new Exception(string.Format("Order #{0} not found.", orderID.ToString()));

                string additionalInstructions = "";

                // SET Addition Instructions text based on the follow scenarios;
                // Modified 10/05/2015 by Anuradha Nandi
                // IF the Order is IC CARE in the Status of CREATED-NEEDS PO and Order Type is RENEWAL then no actions needs to be taken after deleting in Portal.
                // However, if the Order Type is ADDON, NEW, etc., THEN the Order can be deleted in Portal, but the User needs to make sure it is cleared/deleted in MAS200.
                // Also, update Efren German in Mfg. & Shipping of the change.
                if (getOrderDetails.OrderStatusID == 1 && getOrderDetails.BrandSourceID == 3)
                {
                    string orderTypeICCare = "";
                    orderTypeICCare = getOrderDetails.OrderType.ToUpper();

                    if (orderTypeICCare == "RENEWAL")
                    {
                        additionalInstructions = string.Format("Order #{0} is an IC CARE RENEWAL in the status of CREATED-NEEDS PO. No further action is needed.", orderID.ToString());
                    }
                    else
                    {
                        additionalInstructions = string.Format("Order #{0} is an IC CARE {1} in the status of CREATED-NEEDS PO.<br />Please make sure to delete the order in MAS200 and inform Efren German in Shipping & Manufacturing of the changes.", orderID.ToString(), orderTypeICCare);
                    }
                }

                // Modified 10/05/2015 by Anuradha Nandi
                // IF the Order is MIRION in the Status of CREATED-NO PO and Order Type is RENEWAL, ADDON, NEW, etc. 
                // THEN the Order can be deleted in Portal, but the User needs to make sure it is cleared/deleted in MAS200.
                // Also, update Efren German in Mfg. & Shipping of the change.
                if (getOrderDetails.OrderStatusID == 2 && getOrderDetails.BrandSourceID == 2)
                {
                    string orderTypeMirion = "";
                    orderTypeMirion = getOrderDetails.OrderType.ToUpper();

                    if (orderTypeMirion == "RENEWAL")
                    {
                        additionalInstructions = string.Format("Order #{0} is an MIRION RENEWAL in the status of CREATED-NO PO.<br />Please make sure to delete the order in MAS200 and inform Efren German in Shipping & Manufacturing of the changes.", orderID.ToString());
                    }
                    else
                    {
                        additionalInstructions = string.Format("Order #{0} is an MIRION {1} in the status of CREATED-NO PO.<br />Please make sure to delete the order in MAS200 and inform Efren German in Shipping & Manufacturing of the changes.", orderID.ToString(), orderTypeMirion);
                    }
                }

                if (getOrderDetails.OrderStatusID > 2 && getOrderDetails.OrderDetails.Any(ord => ord.Product.ProductGroupID > 9))
                {
                    additionalInstructions = string.Format("Order #{0} is an {1} {2} in the status of FULFILLMENT or later and cannot be canceled. No further action is needed.", orderID.ToString(), getOrderDetails.BrandSource.BrandName.ToUpper(), getOrderDetails.OrderType.ToUpper());
                    throw new Exception("Cannot cancel non-instadse 1 orders that are in the process of being fulfilled.");
                }
                else
                {
                    if (getOrderDetails.OrderStatusID == 3)
                    {
                        additionalInstructions = string.Format("Order #{0} is a {1} {2} in the status of FULFILLMENT.<br />Please make sure to delete the order in MAS200 and inform Efren German in Shipping & Manufacturing of the changes.", orderID.ToString(), getOrderDetails.BrandSource.BrandName.ToUpper(), getOrderDetails.OrderType.ToUpper());
                    }

                    if (getOrderDetails.OrderStatusID == 8)
                    {
                        additionalInstructions = string.Format("Order #{0} is an {1} {2} in the status of RENEWAL HOLD. No further action is needed.", orderID.ToString(), getOrderDetails.BrandSource.BrandName.ToUpper(), getOrderDetails.OrderType.ToUpper());
                    }

                    if (getOrderDetails.OrderStatusID >= 7)
                        throw new Exception("Cannot cancel instadose 1 orders that have been processed or fulfilled.");
                }

                List<string> tableNames = new List<string>();

                string successMessage = "The Order records has been successfully deleted. The following tables were effected; ";

                // Check each table affected by either OrderID or OrderDetailsID and delete the record.
                // This has to be done in a specific order so as to ensure no orphan records remain.

                // Modified 09/24/2015 by Thinh Do, Yong Lee, and Anuradha Nandi
                // After researching the Database Tables; RenewalLogs, RenewalThreadLog, and RenewalThreadLogDetail,
                // it was discovered that RenewalLog and RenewalThreadLogDetail has a 1:1 Relationhip, but
                // RenewalThreadLog and RenewalThreadLogDetail has a 1:N Relationship.
                // This why it could not delete the Renewal Orders properly when trying to delete all (3) related table.
                // Per Thinh Do, we will ignore deletion from the RenewalThreadLog table and only delete records from the following;
                // 1.) RenewalThreadLogDetail
                // 2.) RenewalLog

                // Cancel Order
                var orderRecords = idc.Orders.Where(o => o.OrderID == orderID && o.OrderStatusID != 10).FirstOrDefault();

                if (orderRecords != null)
                {
                    orderRecords.OrderStatusID = 10;

                    tableNames.Add("Orders"); // Add Orders table to List.
                }


                // Delete Renewal Thread Log Detail.
                var renewalThreadLogDetailRecords = (from rtld in idc.RenewalThreadLogDetails
                                                     join rl in idc.RenewalLogs on rtld.RenewalLogID equals rl.RenewalLogID
                                                     where rl.OrderID == orderID
                                                     select rtld).ToList();

                if (renewalThreadLogDetailRecords.Count != 0)
                {
                    idc.RenewalThreadLogDetails.DeleteAllOnSubmit(renewalThreadLogDetailRecords);

                    tableNames.Add("RenewalThreadLogDetail"); // Add RenewalThreadLogDetail table to List.
                }

                // Delete Renewal Logs.
                if (orderRecords.RenewalLogs.Count != 0)
                {
                    idc.RenewalLogs.DeleteAllOnSubmit(orderRecords.RenewalLogs);

                    tableNames.Add("RenewalLogs"); // Add RenewalLogs table to List.
                }

                // Delete ToMas_SO_SalesOrderDetail
                var toMas_SO_SalesOrderDetailRecords = (from sod in mdc.ToMas_SO_SalesOrderDetails where sod.SalesOrderNo == MAS.CleanString(orderID.ToString().PadLeft(7, '0'), 7) select sod).ToList();

                if (toMas_SO_SalesOrderDetailRecords.Count != 0)
                {
                    mdc.ToMas_SO_SalesOrderDetails.DeleteAllOnSubmit(toMas_SO_SalesOrderDetailRecords);

                    tableNames.Add("ToMas_SO_SalesOrderDetail"); // Add ToMas_SO_SalesOrderDetail table to List.
                }

                // Delete ToMAS_SO_SalesOrderHeader
                var toMAS_SO_SalesOrderHeaderRecords = (from soh in mdc.ToMAS_SO_SalesOrderHeaders where soh.FOB == orderID.ToString() select soh).ToList();

                if (toMAS_SO_SalesOrderHeaderRecords.Count != 0)
                {
                    mdc.ToMAS_SO_SalesOrderHeaders.DeleteAllOnSubmit(toMAS_SO_SalesOrderHeaderRecords);

                    tableNames.Add("ToMAS_SO_SalesOrderHeader"); // Add ToMAS_SO_SalesOrderHeader table to List.
                }

                // Delete related Cases
                var caseNotes = (from cn in idc.CaseNotes where cn.Case.OrderID == orderID select cn).ToList();
                if (orderRecords.Cases.Count != 0)
                {
                    idc.CaseNotes.DeleteAllOnSubmit(caseNotes);
                    idc.Cases.DeleteAllOnSubmit(orderRecords.Cases);

                    tableNames.Add("Cases"); // Add Cases table to List.
                }

                // Deactivate documents
                if (orderRecords.Documents.Count != 0)
                {
                    foreach (var documents in orderRecords.Documents)
                    {
                        documents.Active = false;
                    }

                    tableNames.Add("Documents"); // Add Documents table to List.
                }

                var orderUserAssignRecords = idc.OrderUserAssigns.Where(ord => ord.OrderID == orderID).ToList();
                // Delete OrderUserAssign
                if (orderUserAssignRecords.Count != 0)
                {
                    idc.OrderUserAssigns.DeleteAllOnSubmit(orderUserAssignRecords);

                    tableNames.Add("OrderUserAssigns"); // Add OrderUserAssigns table to List.
                }

                var payments = idc.Payments.Where(p => p.OrderID == orderID).ToList();
                // Delete Payments
                if (payments.Count != 0)
                {
                    idc.Payments.DeleteAllOnSubmit(payments);

                    tableNames.Add("Payments"); // Add Payments table to List.
                }

                var renewallBillingRecords = idc.RenewalBillingDevices.Where(rbd => rbd.OrderID == orderID).ToList();
                // Delete Renewal Billing Devices
                if (renewallBillingRecords.Count != 0)
                {
                    idc.RenewalBillingDevices.DeleteAllOnSubmit(renewallBillingRecords);

                    tableNames.Add("RenewalBillingDevices"); // Add RenewalBillingDevices table to List.
                }

                var scheduleBillings = idc.ScheduledBillings.Where(s => s.OrderID == orderID).ToList();
                // Delete Scheduled Billings
                if (scheduleBillings.Count != 0)
                {
                    idc.ScheduledBillings.DeleteAllOnSubmit(scheduleBillings);

                    tableNames.Add("ScheduledBillings"); // Add ScheduledBillings table to List.
                }

                // Delete Account Device Credits
                var accountDeviceCreditRecords = (from adc in idc.AccountDeviceCredits
                                                  where adc.OrderDetail.OrderID == orderID
                                                  select adc).ToList();

                if (accountDeviceCreditRecords.Count != 0)
                {
                    idc.AccountDeviceCredits.DeleteAllOnSubmit(accountDeviceCreditRecords);

                    tableNames.Add("AccountDeviceCredits"); // Add AccountDeviceCredits table to List.
                }

                var packages = idc.Packages.Where(p => p.OrderID == orderID).ToList();
                // Delete Packages
                if (packages.Count != 0)
                {
                    idc.Packages.DeleteAllOnSubmit(packages);
                    tableNames.Add("Packages"); // Add Packages table to List.
                }

                // Delete Package Order Details
                var packageOrderDetailRecords = idc.PackageOrderDetails.Where(pod => pod.OrderDetail.OrderID == orderID).ToList();

                if (packageOrderDetailRecords.Count != 0)
                {
                    idc.PackageOrderDetails.DeleteAllOnSubmit(packageOrderDetailRecords);

                    tableNames.Add("PackageOrderDetails"); // Add PackageOrderDetails table to List.
                }

                // Update InstaWorkOrders, InstaWorkOrderChunks, InstaWorkOrderChunkDetails
                var instaWorkOrders = idc.InstaWorkOrders.Where(iwo => iwo.OrderNo == orderID && iwo.InstaWorkOrderType == 1).ToList(); // 1 = INSTA
                if (instaWorkOrders.Count != 0)
                {
                    foreach (var instaWorkOrder in instaWorkOrders)
                    {
                        instaWorkOrder.isComplete = true;

                        var instaWorkOrderChunks = (from a in idc.InstaWorkOrders
                                                    join b in idc.InstaWorkOrderChunks
                                                    on a.InstaWorkOrderID equals b.InstaWorkOrderID
                                                    where a.OrderNo == orderID
                                                    select b).ToList();

                        foreach (var instaWorkOrderChunk in instaWorkOrderChunks)
                        {
                            instaWorkOrderChunk.isComplete = true;
                        }

                        tableNames.Add("InstaWorkOrderChunks"); // Add InstaWorkOrderChunks table to List.

                        var instaWorkOrderChunkDetails = idc.InstaWorkOrderChunkDetails.Where(iwo => iwo.OrderNo == orderID).ToList();
                        foreach (var instaWorkOrderChunkDetail in instaWorkOrderChunkDetails)
                        {
                            instaWorkOrderChunkDetail.isCompleted = true;
                        }

                        tableNames.Add("InstaWorkOrderChunkDetails"); // Add InstaWorkOrderChunkDetails table to List.

                    }

                    tableNames.Add("InstaWorkOrders"); // Add InstaWorkOrders table to List.
                }

                string listOfTableNames = string.Join(", ", tableNames);

                idc.SubmitChanges();
                VisibleSuccesses(successMessage + listOfTableNames + "<br /><u>Additional Instructions</u>: " + additionalInstructions);
                lblStatus.Text = "Order has been canceled.";
                btnDeleteOrder.Enabled = false;
            }
            catch (Exception ex)
            {
                VisibleErrors(ex.Message);
            }
        }
        else
        {
            VisibleErrors("Please enter a valid Order #.");
        }
    }

    # region Error & Success Messages.
    // Error and Success Messages.
    private void InvisibleErrors()
    {
        this.errorMsg.InnerHtml = "";
        divErrors.Visible = false;
    }

    private void VisibleErrors(string error)
    {
        this.errorMsg.InnerHtml = error;
        divErrors.Visible = true;
    }

    private void InvisibleSuccesses()
    {
        this.successMsg.InnerHtml = "";
        divSuccesses.Visible = false;
    }

    private void VisibleSuccesses(string success)
    {
        this.successMsg.InnerHtml = success;
        divSuccesses.Visible = true;
    }
    # endregion
}

