using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Collections;
using System.Globalization;
using System.Configuration;
using System.Data.SqlClient;

using Instadose;
using Instadose.Data;
using Instadose.Processing;
using Instadose.Integration;
using Instadose.Device;

using Instadose.Integration.Softrax;
using Instadose.Integration.SoftraxMessaging;
using Instadose.Security;

public partial class InstaDose_Finance_CreditMemo : System.Web.UI.Page
{
    
    private const String STR_FEDEX_URL =
      "http://fedex.com/Tracking?ascend_header=1&clienttype=dotcom&cntry_code=us&language=english&tracknumbers=";

    /// <summary>
    /// Limits access to the functionality based on groups.
    /// </summary>
    string[] activeDirecoryGroups = { "IRV-IT", "IRV-Accounting-RW", "IRV-LCDISMAS" };
    
    // Create data table for devices
    DataTable dtDeviceDataTable = null;

    // Create the database reference
    InsDataContext idc = new InsDataContext();
    AppDataContext adc = new AppDataContext();
    MASDataContext mdc = new MASDataContext();

    // String to hold the current username
    string UserName = "Unknown";

    //Query string
    private int accountID = 0;
    private string CurrCode;
    private Account account = null;
    
    private decimal amountUSD = 0;
    private decimal amountForeign = 0;
    private string amount = null;
    
    // The PayPal Environment goes here
    //private String _PayPalEnvironment = "SANDBOX";
    private String _PayPalEnvironment = Instadose.Basics.GetSetting("PayPalEnvironment");

    protected void Page_Load(object sender, EventArgs e)
    {
         // Query active directory to see if the current user belongs to a group.
        bool belongsToGroups = ActiveDirectoryQueries.UserExistsInAnyGroup(User.Identity.Name, activeDirecoryGroups);
        //this.btnCancel.PostBackUrl = "CreditMemoSearch.aspx";

        // If the user exists in the required group make the property visible.
        if (!belongsToGroups)
        { 
            Response.Write ("Not Authorized to View this page!");
            Response.End();
        }

        // Try to set the username
        try
        {
            this.UserName = User.Identity.Name.Split('\\')[1];
            
        }
        catch { this.UserName = "Unknown"; }

        // Try to parse the account ID       
        if (Request.QueryString["ID"] != null) // if AccountID not null, preform "Edit" order 
        {
            int.TryParse(Request.QueryString["ID"], out accountID);

            //AccountID = int.Parse(Request.QueryString["ID"].ToString());
            
            // Query the account
            account = (from a in idc.Accounts where a.AccountID == accountID select a).FirstOrDefault();

            // Ensure the account was found.
            if (account == null) throw new Exception("The account does not exist.");

            if (!IsPostBack)
            {
                //LoadDDLInvoice();
                //ResetDefaultValues();
                bindInvoicesGrid();
                // For selected Credit Memo type as default when a page load, set the following default
                rbtnCreditMemo_CheckedChanged(null, null);                
            }

            // For selected Credit Memo type as default when a page load, set the following default
            //SetDefaultStartDateEndDate();
            //gvOrderInvoiceDetails.Visible = false;       
        }
        else
            Response.Redirect("CreditMemoSearch.aspx");
    }

    // set default start date/end date by selected invoice
    private void SetDefaultStartDateEndDate()
    {
        if (rbtnCreditMemo.Checked || rbtnCreditAndRebill.Checked || rbtnPriceMatch.Checked || rbtnCreditWithBadges.Checked)
        {
            string serStartDate = "";
            string serEndDate = "";

            GetServiceStartDateEndDateByInvoiceNo(ddlInvoice.SelectedValue, ref serStartDate, ref serEndDate);
            //this.txtStartDate.Text = GetInvoiceDateByInvoiceNo(ddlInvoice.SelectedValue);
            this.txtStartDate.Text = (serStartDate != "") ? DateTime.Parse(serStartDate).ToShortDateString() : serStartDate;
            this.txtEndDate.Text = (serEndDate != "") ? DateTime.Parse(serEndDate).ToShortDateString() : serEndDate;     
        }          
    }

    // Error and Success Messages.
    private void InvisibleErrors()
    {
        this.divErrors.Visible = false;
        this.errorMsg.InnerText = "";
    }

    private void VisibleErrors(string error)
    {
        this.divErrors.Visible = true;
        this.errorMsg.InnerText = error;
    }

    /// <summary>
    /// Query Invoice History for this Account ID and bind the data grid.
    /// </summary>
    private void bindInvoicesGrid()
    {
        // Create the sql connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.MASConnectionString"].ConnectionString);

        //// Create the sql command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.Connection = sqlConn;
        sqlCmd.CommandType = CommandType.StoredProcedure;  //.Text;
        sqlCmd.CommandText = "sp_if_GetAllInvoiceHistoryByAccountNo";

        ////string filters = "";

        //// Set the filter parameter for order type
        if (accountID > 0)
        {
            sqlCmd.Parameters.Add(new SqlParameter("@AccountNo", accountID));
        }
             
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        
        SqlDataReader reader = sqlCmd.ExecuteReader();

        DataTable dtInvoices = new DataTable();

        // Create the review orders datatable to hold the contents of the order.
        dtInvoices = new DataTable("Renewal Table");

        // Create the columns for the review orders datatable.
        
        dtInvoices.Columns.Add("OrderID", Type.GetType("System.Int32"));
        dtInvoices.Columns.Add("InvoiceNo", Type.GetType("System.String"));
        dtInvoices.Columns.Add("InvoiceDate", Type.GetType("System.DateTime"));

        dtInvoices.Columns.Add("Amount", Type.GetType("System.String"));
        dtInvoices.Columns.Add("invoiceType", Type.GetType("System.String"));

        //select CONVERT(INT, AR_InvoiceHistoryHeader.FOB) as OrderID, InvoiceNo,
        //    CONVERT(DATETIME, AR_InvoiceHistoryHeader.InvoiceDate) as InvoiceDate,
        //    (nontaxableSalesAmt + FreightAmt + salesTaxamt)	 as Amount,
        //    invoiceType 

        decimal amount = 0;

        while (reader.Read())
        {
            // Create a new review order row.
            DataRow dr = dtInvoices.NewRow();

            // Fill row details.
  
            dr["OrderID"] = reader["OrderID"].ToString();
            dr["InvoiceNo"] = reader["InvoiceNo"].ToString();
            dr["InvoiceDate"] = reader["InvoiceDate"].ToString();

            amount = Convert.ToDecimal(reader["Amount"]);

            dr["Amount"] = Currencies.CurrencyToString(amount, account.CurrencyCode);
            dr["invoiceType"] = reader["invoiceType"].ToString();

            // Add row to the data table.
            dtInvoices.Rows.Add(dr);
        }

        gvInvoices.DataSource = dtInvoices;
        gvInvoices.DataBind();

        //fornmat currency amounts.  USD or foreign.
        //formatRenewalAmount();

        ////If PO number is required in Order table, then set txtPONumber enable to true (allow updates).
        ////if not, DO NOT ALLOW PO number updates.
        //updatePONumbersGrid();

    }

    protected void gvOrderInvoiceDetails_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            RadioButtonList btnList = (RadioButtonList)e.Row.FindControl("rbtnlstWHS");

            if (rbtnCreditMemo.Checked)
                btnList.SelectedValue = "";
            else if (rbtnPriceMatch.Checked)
                btnList.SelectedValue = "";
            else if (rbtnFreight.Checked)
                btnList.SelectedValue = "";
            else if (rbtnCreditAndRebill.Checked)
                btnList.SelectedValue = "INV";
            else if (rbtnCreditWithBadges.Checked)
                btnList.SelectedValue = "NRB";
            else if (rbtnBadDebt.Checked)
                btnList.SelectedValue = "SCP";
            
        }
    }

    /// <summary>
    /// Query Invoice History for this Account ID and bind the data grid.
    /// </summary>
    ///
    //private void bindInvoiceDetailsGrid()
    //{
    //    // Create the sql connection from the connection string in the web.config
    //    SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
    //        ["Instadose.Properties.Settings.MASConnectionString"].ConnectionString);

    //    //// Create the sql command.
    //    SqlCommand sqlCmd = new SqlCommand();
    //    sqlCmd.Connection = sqlConn;
    //    sqlCmd.CommandType = CommandType.StoredProcedure;  //.Text;
    //    sqlCmd.CommandText = "sp_if_GetOrderInvoiceRmaByOrderNo";

    //    ////string filters = "";

    //    //// Set the filter parameter for order type
    //    if (accountID > 0)
    //    {
    //        sqlCmd.Parameters.Add(new SqlParameter("@AccountNo", accountID));
    //    }

    //    bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
    //    if (!openConn) sqlCmd.Connection.Open();

    //    SqlDataReader reader = sqlCmd.ExecuteReader();

    //    DataTable dtInvoices = new DataTable();

    //    // Create the review orders datatable to hold the contents of the order.
    //    dtInvoices = new DataTable("Renewal Table");

    //    // Create the columns for the review orders datatable.

    //    dtInvoices.Columns.Add("OrderID", Type.GetType("System.Int32"));
    //    dtInvoices.Columns.Add("InvoiceNo", Type.GetType("System.String"));
    //    dtInvoices.Columns.Add("InvoiceDate", Type.GetType("System.DateTime"));

    //    dtInvoices.Columns.Add("Amount", Type.GetType("System.String"));
    //    dtInvoices.Columns.Add("invoiceType", Type.GetType("System.String"));

    //  //a.InvoiceNo, b.LotSerialNo , c.DeviceID, a.FOB ,
    //  //          case when f.returnid is not null and f.returnid > 0 then 1  else 0 end as inRMA,
    //  //          c.ProductID, 
    //  //          e.ServiceStartDate as serviceStart,
    //  //          e.ServiceEndDate as serviceEnd,
    //  //          case when h.PaymentMethodID =1  then 'CC' else 'PO' end  as PaymentMethod,
    //  //          i.Price,
    //  //          j.ProductSKU as SKU


    //    decimal amount = 0;

    //    while (reader.Read())
    //    {
    //        // Create a new review order row.
    //        DataRow dr = dtInvoices.NewRow();

    //        // Fill row details.

    //        dr["OrderID"] = reader["OrderID"].ToString();
    //        dr["InvoiceNo"] = reader["InvoiceNo"].ToString();
    //        dr["InvoiceDate"] = reader["InvoiceDate"].ToString();

    //        amount = Convert.ToDecimal(reader["Amount"]);

    //        dr["Amount"] = Currencies.CurrencyToString(amount, account.CurrencyCode);
    //        dr["invoiceType"] = reader["invoiceType"].ToString();

    //        // Add row to the data table.
    //        dtInvoices.Rows.Add(dr);
    //    }

    //    gvInvoices.DataSource = dtInvoices;
    //    gvInvoices.DataBind();

    //    //fornmat currency amounts.  USD or foreign.
    //    //formatRenewalAmount();

    //    ////If PO number is required in Order table, then set txtPONumber enable to true (allow updates).
    //    ////if not, DO NOT ALLOW PO number updates.
    //    //updatePONumbersGrid();

    //}

    private bool PassInputValidation()
    {
         if (this.txtComment.Text.Trim().Length > 30)
        {
            string errorMessage = "Comment can not be over 30 characters.";
            VisibleErrors(errorMessage);
            return false;
        }
        //  ----------------------- Input validation -----------------------------------------//
         if (rbtnCreditMemo.Checked || rbtnCreditAndRebill.Checked || rbtnPriceMatch.Checked 
             || (rbtnCreditWithBadges.Checked && !ddlInvoice.SelectedItem.Text.Contains("LOST REPLACEMENT") && !ddlInvoice.SelectedItem.Text.Contains("NO REPLACEMENT")))
        {
            DateTime myDate;

            if (this.txtStartDate.Text.Trim().Length == 0)
            {
                string errorMessage = "Service Start Date is required.";
                VisibleErrors(errorMessage);
                return false;
            }
            else if (!DateTime.TryParse(this.txtStartDate.Text, out myDate))
            {
                string errorMessage = "Service Start Date is invalid.";
                VisibleErrors(errorMessage);
                return false;
            }
            

            if (this.txtEndDate.Text.Trim().Length == 0)
            {
                string errorMessage = "Service End Date is required.";
                VisibleErrors(errorMessage);
                return false;
            }
            else if (!DateTime.TryParse(this.txtEndDate.Text, out myDate))
            {
                string errorMessage = "Service End Date is invalid.";
                VisibleErrors(errorMessage);
                return false;
            }            
        }        
       
        return true;
        //  ----------------------- Input validation -----------------------------------------//   
    }

    /// <summary>
    /// Credit Memo Main Process 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnConfirm_Click(object sender, EventArgs e)
    {
        int accountID = account.AccountID;  //int.Parse(Request.QueryString["ID"].ToString());
        int locationID = 0;
        int newOrderID = 0;
        int newPaymentID = 0;
        int originalOrderID = 0;
        string currencyCode = "USD";
        string refer = "0000";
        int brandSourceID = 0;
        string selectedInvoiceNo = "";
        // determin credit send to softrax
        bool CreditMemoToSoftrax;
        string CreditMemoType;

        InvisibleErrors();

        // ------------------------ determine credit memo type  -------------------------------//   
        
        if (rbtnCreditMemo.Checked)
        {
            CreditMemoType = "Credit Memo";
            CreditMemoToSoftrax = true;
        }
        else if (rbtnPriceMatch.Checked)
        {
            CreditMemoType = "Price Match";
            CreditMemoToSoftrax = true;
        }
        else if (rbtnCreditAndRebill.Checked)
        {
            CreditMemoType = "Credit and Rebill";
            CreditMemoToSoftrax = true;
        }
        else if (rbtnFreight.Checked)
        {
            CreditMemoType = "Freight";
            CreditMemoToSoftrax = false;
        }       
        else if (rbtnCreditWithBadges.Checked)
        {
            CreditMemoType = "Credit Non-Returned Badges";
            if (ddlInvoice.SelectedItem.Text.Contains("LOST REPLACEMENT") || ddlInvoice.SelectedItem.Text.Contains("NO REPLACEMENT"))
                CreditMemoToSoftrax = false;
            else
                CreditMemoToSoftrax = true;
        }
        else if (rbtnBadDebt.Checked)
        {
            CreditMemoType = "Bad Debt";
            CreditMemoToSoftrax = false;
        }
        else return;
        
        // determine what invoice# to credit on. 
        selectedInvoiceNo = ddlInvoice.SelectedValue.ToString();

        //Define datatable for selected rows (devices)
        declareSelectItemDataTable();

        // Populate DT by selected rows (devices)
        if (CreditMemoType == "Credit Non-Returned Badges" || CreditMemoType == "Bad Debt")
        {
            buildSelectItemDataTable(selectedInvoiceNo);
        } 
        // ------------------------ determine credit memo type  -------------------------------//  


        if (PassInputValidation())
        {
            decimal itemCredit = 0;
            decimal misCredit = 0;
            decimal shipCredit = 0;
            decimal totalCredit = 0;

            itemCredit = ToDecimal(txtCreditItem.Text);
            //misCredit = ToDecimal(txtCreditMisc.Text);
            shipCredit = ToDecimal(txtCreditShipping.Text);
            totalCredit = itemCredit + misCredit + shipCredit;

            
            var myAccountDetail = (from a in idc.Accounts
                                   where a.AccountID == accountID
                                   select a).FirstOrDefault();

            // Grab BrandSource
            brandSourceID = myAccountDetail.BrandSourceID.Value;

            // Grab OrderID associated with selected InvoiceNo
            var myOrgOrderId = (from a in mdc.AR_InvoiceHistoryHeaders
                                where a.InvoiceNo == selectedInvoiceNo
                                select new { a.FOB }).FirstOrDefault();

            if (myOrgOrderId != null && myOrgOrderId.FOB != "")
            {
                originalOrderID = int.Parse(myOrgOrderId.FOB);
                // Grab OriginalOrder info.             
                var myOrder = (from a in idc.Orders
                               where a.OrderID == originalOrderID
                               select new { a.LocationID, a.CurrencyCode, a.ReferralCode }).FirstOrDefault();

                if (myOrder != null)
                {
                    locationID = myOrder.LocationID;
                    currencyCode = myOrder.CurrencyCode.ToString();
                    refer = myOrder.ReferralCode.ToString();
                }
                else
                {
                    string errorMessage = "Could not find the order#: " + originalOrderID + " in the system.";
                    VisibleErrors(errorMessage);
                    return;
                }
            }
            else //Get the default locationID,currencyCode, refer of an account 
            {
                // Grab default Location
                var DefLoc = (from L in idc.Locations                              
                              where L.IsDefaultLocation == true
                              && L.AccountID == accountID
                              select L).FirstOrDefault();
                locationID = DefLoc.LocationID;
                currencyCode = myAccountDetail.CurrencyCode;
                refer = myAccountDetail.ReferralCode;
                originalOrderID = 0;               
            }


            // -------------------------------------
            //Step 1. Create a new Order --> return OrderID
            //Step 2. Payment, save to Orderdetails and AccoutDeviceCredits           
            //Step 3. Process CreditMemo To MAS (NO Credit card refund 6/10/2011 )
            // -------------------------------------

            // Step 1. -- create credit memo order
            newOrderID = saveToOrder(accountID, locationID, misCredit * (-1),
                    shipCredit * (-1), refer, currencyCode, brandSourceID, originalOrderID);

            //newOrderID = 12345;
            //Step 2.  -- Payment, save to Orderdetails and AccoutDeviceCredits
            if (newOrderID != 0)
            {
                // -- Create payment record
                int myPaymentMethodID = (from a in idc.PaymentMethods
                                         where a.PaymentMethodName == "Purchase Order"
                                         select a.PaymentMethodID).FirstOrDefault();

                newPaymentID = saveToPayment(newOrderID, (totalCredit * (-1)), myPaymentMethodID, currencyCode);

                saveToOrderDetailAccountDeviceCredit(newOrderID, itemCredit, accountID, CreditMemoType, this.ddlInvoice.SelectedItem.Text);

                //Step 3. -- Process CreditMemo to MAS & Softrax 

                // --process creditmemo to MAS                
                applyCreditMemoToMas(newOrderID, originalOrderID, itemCredit, shipCredit, selectedInvoiceNo, CreditMemoType, this.ddlInvoice.SelectedItem.Text);

                // Determine if the Softrax application setting is enabled.
                String SendToSoftrax = Basics.GetSetting("SoftraxActive");

                // And if credit to item ONLY amount > 0 (not include miscellaneous, shipping credits)
                // also, the Credit Memo for Devices W/Softtrax radio button checked 7/28/2011
                string strSoftraxProcess = "N/A";
                if (SendToSoftrax == "1" && itemCredit > 0 && CreditMemoToSoftrax)
                    strSoftraxProcess = sendSoftraxRecordsCredit(newOrderID, originalOrderID, itemCredit, CreditMemoType, this.ddlInvoice.SelectedItem.Text);

                //invisible the creditmemo content 
                divCreditMemoDetail.Visible = false;

                // visible result
                divCreditMemoResult.Visible = true;

                //Step 4. Display CreditMemo confirmation page
                displayConfirmation(newOrderID, accountID, itemCredit, misCredit, shipCredit,
                        totalCredit, selectedInvoiceNo, strSoftraxProcess);

            }
            
        }        

    }// END btnConfirm_Click

    /// <summary>
    /// Save Credit Order to LCDIS orders
    /// </summary>
    /// <param name="AccountID"></param>
    /// <param name="LocationID"></param>
    /// <param name="OrderItemCredit">Credit Amount</param>
    /// <param name="OrderMiscCredit">Misc Credit </param>
    /// <param name="OrderShippingCharge">Shipping Credit</param>
    /// <param name="Refer">Referral</param>
    /// <param name="Currency">Currency </param>
    /// <returns></returns>
    protected int saveToOrder(int accountID, int locationID, 
        decimal orderMiscCredit, decimal orderShippingCharge, string refer, string currency, 
        int brandSourceID, int parentOrderID)
    {        

        // orderType: New, Renewal, Unset, Lost Replacement, Recall Replacement
        int ReturnOrderID = 0;
        try
        {
            Order NewOrder = null;
            NewOrder = new Order();
            NewOrder.OrderType = "CREDIT MEMO";
            //NewOrder.PONumber = InputPOnumber;
            NewOrder.AccountID = accountID;
            NewOrder.LocationID = locationID;
            NewOrder.BrandSourceID = brandSourceID;
            NewOrder.ShippingMethodID = 0;
            NewOrder.OrderDate = DateTime.Now;
            NewOrder.CreatedBy = UserName;
            NewOrder.OrderStatusID = 5;
            NewOrder.ReferralCode = refer ;
            NewOrder.SoftraxStatus = false;
            NewOrder.ShippingCharge = orderShippingCharge;
            NewOrder.CurrencyCode = currency;
            NewOrder.MiscCredit = orderMiscCredit;
            NewOrder.OrderSource = "Business App";            
            NewOrder.PORequired = false;
            NewOrder.ParentOrderID = (parentOrderID > 0) ? parentOrderID : (int?) null;

            // Set Account ContractStartDate and ContractEndDate to Order.
            NewOrder.ContractStartDate = account.ContractStartDate.HasValue ? account.ContractStartDate.Value : DateTime.Now;
            NewOrder.ContractEndDate = account.ContractEndDate.HasValue ? account.ContractEndDate.Value : DateTime.Now;
            NewOrder.ACMIntegrationStatusID = 1;
            idc.Orders.InsertOnSubmit(NewOrder);
            idc.SubmitChanges();

            //get the @@identity OrderID
            ReturnOrderID = NewOrder.OrderID;

        }
        catch (Exception ex)
        {
            ReturnOrderID = 0;
        }

        return ReturnOrderID;
    }

    //payment record to be created.
    protected int saveToPayment(int newOrderID, decimal totalAmount, int paymentMethodID, string currencyCode)
    {
        int ReturnPaymentID = 0;
        try 
        {
            // Record payment header data.
            Payment newPayment = new Payment()
            {
                OrderID = newOrderID,
                PaymentDate = DateTime.Now,
                Amount = totalAmount,
                GatewayResponse = "POPAYMENT",
                //TransCode = "",
                Captured = true,
                Authorized = true,
                PaymentMethodID = paymentMethodID,
                CreatedBy = UserName ,
                ApplicationID = 2,
                CurrencyCode = currencyCode
            };
            // Insert the payment on submit.
            idc.Payments.InsertOnSubmit(newPayment);
            idc.SubmitChanges();
            //get the @@identity PaymentId
            ReturnPaymentID = newPayment.PaymentID;
        }
        catch {
            ReturnPaymentID = 0; 
        }

        return ReturnPaymentID;         
    }

    /// <summary>
    /// Insert OrderDetail for new credit memo order and AccountDeviceCredits for new credit memo OrderDetail
    /// </summary>
    /// <param name="newOrderID">New orderID generate for Credit Order</param>
    /// <param name="ItemCredit">Credit Amount</param>
    protected void saveToOrderDetailAccountDeviceCredit(int newOrderID, 
            decimal itemCredit, int ordAccountID, string creditMemoType, string orderTypeofInvoice)
    {

        if (creditMemoType == "Credit Non-Returned Badges" || creditMemoType == "Bad Debt")
        {
            //credit OrderDetails info
            //read from datatable 
            var ItemArray = (from r in dtDeviceDataTable.AsEnumerable()
                             select r).ToArray();

            // If user is selecting some devices to credit.
            if (ItemArray.Length > 0)
            {
                // calculate price 
                string strUintPrice = (itemCredit / ItemArray.Length).ToString("00.00");
                decimal TotalLoopCredit = 0;

                for (int i = 0; i < ItemArray.Length; i++)
                {

                    // ensure the sum of credit per device  equal the total itemCredit
                    // the last "Price" of device will be different from others (+/- 1 cent)
                    if (i == ItemArray.Length - 1)
                        strUintPrice = (itemCredit - TotalLoopCredit).ToString("00.00");
                    else
                        TotalLoopCredit += decimal.Parse(strUintPrice);

                    DateTime ArrstartDate = Convert.ToDateTime(ItemArray[i].ItemArray[4].ToString());
                    DateTime ArrendDate = Convert.ToDateTime(ItemArray[i].ItemArray[5].ToString());
                    int ArrRMA = int.Parse(ItemArray[i].ItemArray[6].ToString());
                    int ArrProductID = int.Parse(ItemArray[i].ItemArray[7].ToString());
                    //int ArrVariantID = int.Parse(ItemArray[i].ItemArray[8].ToString());
                    int ArrDeviceID = int.Parse(ItemArray[i]["DeviceID"].ToString());
                    string ArrWarehouse = ItemArray[i].ItemArray[10].ToString();
                    decimal ArrCreditRate = decimal.Parse(ItemArray[i].ItemArray[11].ToString());

                    // Insert INSTA10 OrderDetail for new credit memo Order
                    OrderDetail newOrdDetail = null;

                    newOrdDetail = new OrderDetail();

                    newOrdDetail.OrderID = newOrderID;
                    newOrdDetail.Price = decimal.Parse(strUintPrice);
                    newOrdDetail.Quantity = -1;
                    newOrdDetail.OrderDetailDate = DateTime.Now;
                    newOrdDetail.ProductID = ArrProductID;

                    idc.OrderDetails.InsertOnSubmit(newOrdDetail);                    
                    idc.SubmitChanges();

                    // get the @@identity order detail ID
                    int NewOrderDetailID = newOrdDetail.OrderDetailID;
                    
                    // Insert AccountDeviceCredit for new credit memo Order Detail
                    AccountDeviceCredit newAcctDevCredit = null;
                    newAcctDevCredit = new AccountDeviceCredit();

                    newAcctDevCredit.AccountID = ordAccountID;
                    newAcctDevCredit.DeviceID = ArrDeviceID;
                    newAcctDevCredit.CreditStartDate = ArrstartDate;

                    newAcctDevCredit.CreditEndDate = ArrendDate;
                    newAcctDevCredit.OrderDetailID = NewOrderDetailID;
                    newAcctDevCredit.CreditCardTypeID = 1; //1,2,3, or 4. Visa is 1
                    newAcctDevCredit.CreditRate = ArrCreditRate;
                    newAcctDevCredit.ReturnDevice = (ArrRMA == 0) ? false : true;
                    newAcctDevCredit.MASWarehouse = ArrWarehouse; // to what warehouse (SCP, TST, LST)

                    idc.AccountDeviceCredits.InsertOnSubmit(newAcctDevCredit);
                    idc.SubmitChanges();

                    // Tdo, 08/07/2015
                    // Insert CASE COVER OrderDetail for new credit memo Order if the selected product is INSTA10 or INSTA10-B or INSTA PLUS
                    // Since these products are non-color, I use "CASE CVR BLK" sku for INSTA10 or INSTA10-B by default
                    // I use "COLOR CAP BLUE" sku for INSTA PLUS by default.

                    Product myProduct = (from a in idc.Products
                                        where a.ProductID == ArrProductID
                                        select a).FirstOrDefault();

                    if (myProduct.ProductSKU == "INSTA10" || myProduct.ProductSKU == "INSTA10-B" || myProduct.ProductSKU == "INSTA PLUS")
                    {
                        string defaultCaseCoverProductSKU = "";

                        if (myProduct.ProductSKU == "INSTA10" || myProduct.ProductSKU == "INSTA10-B") defaultCaseCoverProductSKU = "CASE CVR BLK";
                        if (myProduct.ProductSKU == "INSTA PLUS") defaultCaseCoverProductSKU = "COLOR CAP BLUE";


                        int caseCoverProductID = (from p in idc.Products
                                                     where p.ProductSKU == defaultCaseCoverProductSKU
                                                     select p.ProductID).FirstOrDefault();

                        newOrdDetail = new OrderDetail();

                        newOrdDetail.OrderID = newOrderID;
                        newOrdDetail.Price = 0.00M;
                        newOrdDetail.Quantity = -1;
                        newOrdDetail.OrderDetailDate = DateTime.Now;
                        newOrdDetail.ProductID = caseCoverProductID;

                        idc.OrderDetails.InsertOnSubmit(newOrdDetail);
                        idc.SubmitChanges();
                    }
                    
                }
            }
            else   // If user is not selecting any devices to credit.
            {
                int ArrProductID = (from a in idc.Products
                                    where (a.ProductSKU == "/CREDIT" && a.ProductGroupID == 1)
                                    select a.ProductID).FirstOrDefault();

                OrderDetail newOrdDetail = null;
                newOrdDetail = new OrderDetail();

                newOrdDetail.OrderID = newOrderID;
                newOrdDetail.Price = itemCredit;
                newOrdDetail.Quantity = -1;
                newOrdDetail.OrderDetailDate = DateTime.Now;
                newOrdDetail.ProductID = ArrProductID;

                idc.OrderDetails.InsertOnSubmit(newOrdDetail);
                idc.SubmitChanges();
                // get the @@identity order detail ID
                int newOrderDetailID = newOrdDetail.OrderDetailID;

                // *********** NO NEED ACCOUNT DEVICE CREDIT DUE TO NO SPECIFIC DEVICES SELECTED TO CREDIT ***************
                //                                                                                                      //
                // *********** NO NEED ACCOUNT DEVICE CREDIT DUE TO NO SPECIFIC DEVICES SELECTED TO CREDIT ***************
            }
        }
        else if (creditMemoType == "Price Match" || creditMemoType == "Credit and Rebill" || creditMemoType == "Credit Memo" )        
        {           
            // submit order to business apps OrderDetails table

            int ArrProductID = (from a in idc.Products
                                where (a.ProductSKU == "/CREDIT" && a.ProductGroupID == 1)
                                select a.ProductID).FirstOrDefault();

            OrderDetail newOrdDetail = null;
            newOrdDetail = new OrderDetail();

            newOrdDetail.OrderID = newOrderID;
            newOrdDetail.Price = itemCredit;
            newOrdDetail.Quantity = -1;
            newOrdDetail.OrderDetailDate = DateTime.Now;
            newOrdDetail.ProductID = ArrProductID;

            idc.OrderDetails.InsertOnSubmit(newOrdDetail);
            idc.SubmitChanges();
            // get the @@identity order detail ID
            int newOrderDetailID = newOrdDetail.OrderDetailID;

            // *********** NO NEED ACCOUNT DEVICE CREDIT DUE TO NO SPECIFIC DEVICES SELECTED TO CREDIT ***************
            //                                                                                                      //
            // *********** NO NEED ACCOUNT DEVICE CREDIT DUE TO NO SPECIFIC DEVICES SELECTED TO CREDIT ***************

        }
        
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="CreditOrderID"></param>
    /// <param name="originalOrderID"></param>
    /// <param name="SelectedInvoiceNo"></param>
    /// <param name="CreditMemoType"></param>
    protected void applyCreditMemoToMas(int CreditOrderID, int originalOrderID, decimal itemCredit, decimal shipCredit, string selectedInvoiceNo, 
                                        string creditMemoType, string orderTypeofInvoice)
    {        
        // insert to 3 tables in MAS:        
        SO6_InvoiceDataEntryDetail SO6;
        SO_InvoiceTierDistribution SOInvTD;
        SO5_InvoiceDataEntryHeader SO5;

        string StrSKU;
        string StrLineType;
        string StrWareHouse = "";
        string LotSerialNo = "";
        int intDetailCounter= 1;
        List<string> selLotSerialNo = new List<string>();
                        
        //Boolean BoolReturnDevice =false ;

        //grab Invoice History Header
        string invoiceNumber = selectedInvoiceNo;
        //string orderNo = originalOrderID.ToString().PadLeft(7, '0');
       
        // Select itemcode to submit to MAS by Credit Types
        string itemCode = "";

        switch (creditMemoType)
        {
            case "Credit Memo":
                // if itemcode = itemsku then use /CREDIT, else use Original Invoice Itemcode
                itemCode = (from a in mdc.AR_InvoiceHistoryDetails
                            where a.InvoiceNo == invoiceNumber
                                && !a.ItemCode.StartsWith("INSTA")
                                && !a.ItemCode.StartsWith("CASE CVR")
                                && !a.ItemCode.StartsWith("COLOR CAP")
                                select a.ItemCode).FirstOrDefault();
                if (itemCode == null || itemCode.Length == 0)
                    itemCode = "/CREDIT";

                break;
            case "Credit and Rebill":
                // if itemcode = itemsku then use /CREDIT, else use Original Invoice Itemcode
                itemCode = (from a in mdc.AR_InvoiceHistoryDetails
                            where a.InvoiceNo == invoiceNumber
                                && !a.ItemCode.StartsWith("INSTA")
                                && !a.ItemCode.StartsWith("CASE CVR")
                                && !a.ItemCode.StartsWith("COLOR CAP")
                                select a.ItemCode).FirstOrDefault();
                if (itemCode == null || itemCode.Length == 0)
                    itemCode = "/CREDIT";

                break;
               
            case "Price Match":                
                itemCode = "/PRICEMATCH";    // try /CREDIT instead of /PRICEMATCH for testing purpose
                break;
            case "Credit Non-Returned Badges":
                itemCode = "/CREDIT";
                break;
            case "Bad Debt":                
                itemCode = "/BADDEBT";      // change itemcode from /BADDEBTW/O (charge) to /BADDEBT (Misc)
                break;
            case "Freight":
                itemCode = "/FREIGHT WOFF";    // change itemcode from /FRTWRITEOFF (charge) to /FREIGHT WOFF (Misc)                
                break;
            default:
                itemCode = "/CREDIT";
                break;
        }                

        //  ------------------------------ Create data entry deatail for every device selected" ------------------------------//
        if (creditMemoType == "Credit and Rebill")
        {
            // if ItemCredit > 0 then credit for all devices in invoice and put the device back to INV for ready to ship
            if (itemCredit != 0)
            {
                // 5/30/2014, Tdo. For renewal invoice, we do not need to send the badges back to INV warehouse by Gamalia
                if (! orderTypeofInvoice.Contains("RENEWAL"))
                {
                    var serialList = (from a in mdc.AR_InvoiceHistoryLotSerials
                                      join b in mdc.AR_InvoiceHistoryDetails on new { a.InvoiceNo, a.DetailSeqNo } equals new { b.InvoiceNo, b.DetailSeqNo }
                                      where a.InvoiceNo == invoiceNumber
                                      select new
                                      {
                                          LotSerialNo = a.LotSerialNo,
                                          ItemCode = b.ItemCode
                                      }).ToList().OrderBy(r => r.ItemCode);


                    if (serialList.Count() > 0)         // at least has one item to go through the loop
                    {

                        // get price per item
                        decimal UintPrice = (itemCredit / serialList.Count());
                        decimal TotalLoopCredit = 0;
                        int i = 0;

                        // Credit for all devices in invoice
                        foreach (var item in serialList)
                        {
                            // ensure the sum of credit per device  equal the total itemCredit
                            // the last "Price" of device will be different from others (+/- 1 cent)
                            i++;
                            if (i == serialList.Count())
                                UintPrice = (itemCredit - TotalLoopCredit);
                            else
                                TotalLoopCredit += UintPrice;


                            StrSKU = item.ItemCode.ToUpper();
                            StrLineType = "1"; // apply credit for each device
                            StrWareHouse = "INV"; // Assign to warehouse
                            LotSerialNo = item.LotSerialNo;                            

                            // save data to SO6_InvoiceDataEntryDetail
                            SO6 = null;
                            SO6 = new SO6_InvoiceDataEntryDetail()
                            {
                                InvoiceNumber = MAS.CleanString(invoiceNumber.PadLeft(7, '0'), 7),
                                ItemNumber = MAS.CleanString(StrSKU, 15),
                                LineType = char.Parse(StrLineType),
                                WhseCode = StrWareHouse,
                                QtyOrdered = 1, // has to be positive. Qty in Bcd.order is -ve
                                QtyShipped = 1, // has to be positive.
                                UnitPrice = UintPrice,
                                LineNumber = intDetailCounter.ToString()
                            };
                            mdc.SO6_InvoiceDataEntryDetails.InsertOnSubmit(SO6);
                            mdc.SubmitChanges();

                            // save data to SO_InvoiceTierDistribution
                            SOInvTD = null;
                            SOInvTD = new SO_InvoiceTierDistribution()
                            {
                                InvoiceNo = MAS.CleanString(invoiceNumber.PadLeft(7, '0'), 7),
                                LineKey = intDetailCounter.ToString(),
                                LotSerialNo = LotSerialNo,
                                TierType = Convert.ToChar(" "),
                                QuantityShipped = 1
                            };
                            mdc.SO_InvoiceTierDistributions.InsertOnSubmit(SOInvTD);
                            mdc.SubmitChanges();

                            intDetailCounter += 1;

                            //// tdo, 08/07/2015
                            //// Might include the code below for Case Cvr when a creditted device is INSTA10 or INSTA10-B if needed.
                            //// save data to SO6_InvoiceDataEntryDetail
                            //// Right now, since we have no wait to know a serialno associated with a color case cover,
                            //// I use "CASE CVR BLK" sku by default.
                            //SO6 = null;
                            //SO6 = new SO6_InvoiceDataEntryDetail()
                            //{
                            //    InvoiceNumber = MAS.CleanString(invoiceNumber.PadLeft(7, '0'), 7),
                            //    ItemNumber = MAS.CleanString("CASE CVR BLK", 15),
                            //    LineType = char.Parse(StrLineType),
                            //    WhseCode = StrWareHouse,
                            //    QtyOrdered = 1, // has to be positive. Qty in Bcd.order is -ve
                            //    QtyShipped = 1, // has to be positive.
                            //    UnitPrice = 0.00M,
                            //    LineNumber = intDetailCounter.ToString()
                            //};
                            //mdc.SO6_InvoiceDataEntryDetails.InsertOnSubmit(SO6);
                            //mdc.SubmitChanges();

                            //intDetailCounter += 1;

                            // 12/15/2015. Fixed device movement discrepancy between MAS and portal
                            // ------------------- Update warehouse for a device in portal -----------------------//
                            UpdateDeviceInventory(LotSerialNo, "Committed");

                        }
                    }
                }                
            }
            
        }
        else if (creditMemoType == "Credit Non-Returned Badges" || creditMemoType == "Bad Debt")
        {
            //// tdo, 08/07/2015
            // Get info of selected devices to credit
            // Make sure to exclude the Case Cvr from order detail
             var creditOrdDetail = (from O in idc.Orders
                               join OrdD in idc.OrderDetails on O.OrderID equals OrdD.OrderID
                               join ACrd in idc.AccountDeviceCredits on OrdD.OrderDetailID equals ACrd.OrderDetailID
                               join DevInv in idc.DeviceInventories on ACrd.DeviceID equals DevInv.DeviceID
                               join P in idc.Products on DevInv.ProductID equals P.ProductID
                               where OrdD.OrderID == CreditOrderID
                               && !P.ProductSKU.Contains("CASE CVR")
                               && !P.ProductSKU.Contains("COLOR CAP")
                               select new
                               {
                                   OrdD.OrderDetailID,
                                   OrdD.OrderID,
                                   OrdD.Price,
                                   OrdD.Quantity,
                                   ACrd.ReturnDevice,
                                   DevInv.SerialNo,
                                   P.ProductSKU,
                                   O.MiscCredit,
                                   ACrd.MASWarehouse
                               }).ToList().OrderBy(r => r.ProductSKU);             

             if (creditOrdDetail.Count() > 0)     
             {
                 // Credit for all selected devices.
                 // insert SO6_InvoiceDataEntryDetail  && SO_InvoiceTierDistribution for MAS
                 foreach (var v in creditOrdDetail)
                 {                     
                     StrWareHouse = (v.MASWarehouse == null) ? "TST" : v.MASWarehouse.ToString();
                     StrSKU = v.ProductSKU.ToUpper();
                     StrLineType = "1"; // apply credit for each device
                     LotSerialNo = v.SerialNo;

                     selLotSerialNo.Add(LotSerialNo);

                     // save data to SO6_InvoiceDataEntryDetail
                     SO6 = null;
                     SO6 = new SO6_InvoiceDataEntryDetail()
                     {
                         InvoiceNumber = MAS.CleanString(invoiceNumber.PadLeft(7, '0'), 7),
                         ItemNumber = MAS.CleanString(StrSKU, 15),
                         LineType = char.Parse(StrLineType),
                         WhseCode = StrWareHouse,
                         QtyOrdered = 1, // has to be positive. Qty in Bcd.order is -ve
                         QtyShipped = 1, // has to be positive.
                         UnitPrice = 0,
                         LineNumber = intDetailCounter.ToString()
                     };
                     mdc.SO6_InvoiceDataEntryDetails.InsertOnSubmit(SO6);
                     mdc.SubmitChanges();

                     // save data to SO_InvoiceTierDistribution
                     SOInvTD = null;
                     SOInvTD = new SO_InvoiceTierDistribution()
                     {
                         InvoiceNo = MAS.CleanString(invoiceNumber.PadLeft(7, '0'), 7),
                         LineKey = intDetailCounter.ToString(),
                         LotSerialNo = LotSerialNo,
                         TierType = Convert.ToChar(" "),
                         QuantityShipped = 1
                     };
                     mdc.SO_InvoiceTierDistributions.InsertOnSubmit(SOInvTD);
                     mdc.SubmitChanges();

                     intDetailCounter += 1;

                     //// tdo, 08/07/2015
                     //// Might include the code below for Case Cvr when a creditted device is INSTA10 or INSTA10-B if needed.
                     //// save data to SO6_InvoiceDataEntryDetail
                     //// Right now, since we have no way to know a serialno associated with a color case cover,
                     //// I use "CASE CVR BLK" sku by default.
                     //SO6 = null;
                     //SO6 = new SO6_InvoiceDataEntryDetail()
                     //{
                     //    InvoiceNumber = MAS.CleanString(invoiceNumber.PadLeft(7, '0'), 7),
                     //    ItemNumber = MAS.CleanString("CASE CVR BLK", 15),
                     //    LineType = char.Parse(StrLineType),
                     //    WhseCode = StrWareHouse,
                     //    QtyOrdered = 1, // has to be positive. Qty in Bcd.order is -ve
                     //    QtyShipped = 1, // has to be positive.
                     //    UnitPrice = 0.00M,
                     //    LineNumber = intDetailCounter.ToString()
                     //};
                     //mdc.SO6_InvoiceDataEntryDetails.InsertOnSubmit(SO6);
                     //mdc.SubmitChanges();

                     //intDetailCounter += 1;

                     // 12/15/2015. Fixed device movement discrepancy between MAS and portal
                     // ------------------- Update warehouse for a device in portal -----------------------//
                     string myDeviceAnalysisStatus = "";
                     switch (StrWareHouse)
                     {                         
                         case "SCP":
                            myDeviceAnalysisStatus = "Scrap";
                            break;
                         case "NRB":
                            myDeviceAnalysisStatus = "Non-Returned";      
                            break;
                         case "TST":
                            myDeviceAnalysisStatus = "Testing";
                            break;
                         default:
                            myDeviceAnalysisStatus = "";
                            break;
                     }
                     UpdateDeviceInventory(LotSerialNo, myDeviceAnalysisStatus);
                 }
             }              
        }
        //  ------------------------------ End create data entry deatail for every device selected" ------------------------------//

        //  ------------ Create an additional data entry deatail for every Credit Memo type except "Credit and Rebill" -----------//
        if (creditMemoType == "Credit Non-Returned Badges" || creditMemoType == "Price Match" || creditMemoType == "Credit Memo"
            || (creditMemoType == "Credit and Rebill" && orderTypeofInvoice.Contains("RENEWAL")))
        {
            StrSKU = itemCode;
            StrLineType = "5";  //apply credit to invoice
            StrWareHouse = "INV";

            // insert SO6_InvoiceDataEntryDetail  into MAS
            SO6 = null;
            SO6 = new SO6_InvoiceDataEntryDetail()
            {
                InvoiceNumber = MAS.CleanString(invoiceNumber.PadLeft(7, '0'), 7),
                ItemNumber = StrSKU,
                LineType = char.Parse(StrLineType),
                WhseCode = StrWareHouse,
                QtyOrdered = 1, // has to be positive
                QtyShipped = 1, // has to be positive
                UnitPrice = itemCredit,
                LineNumber = intDetailCounter.ToString()
            };
            mdc.SO6_InvoiceDataEntryDetails.InsertOnSubmit(SO6);
            mdc.SubmitChanges();

            intDetailCounter += 1;        
        }
        else if (creditMemoType == "Bad Debt" || creditMemoType == "Freight" )
        {
            Decimal totalCredit = itemCredit + shipCredit;

            StrSKU = itemCode;
            StrLineType = "5";  //apply credit to invoice
            StrWareHouse = "INV";

            // insert SO6_InvoiceDataEntryDetail  into MAS
            SO6 = null;
            SO6 = new SO6_InvoiceDataEntryDetail()
            {
                InvoiceNumber = MAS.CleanString(invoiceNumber.PadLeft(7, '0'), 7),
                ItemNumber = StrSKU,
                LineType = char.Parse(StrLineType),
                WhseCode = StrWareHouse,
                QtyOrdered = 1,    // has to be positive
                QtyShipped = 1,   // has to be positive
                UnitPrice = totalCredit,
                LineNumber = intDetailCounter.ToString()
            };
            mdc.SO6_InvoiceDataEntryDetails.InsertOnSubmit(SO6);
            mdc.SubmitChanges();

            intDetailCounter += 1;
        }
        //  ------------ Create an additional data entry deatail for every Credit Memo type except "Credit and Rebill" -----------//


        //  ----------------------------- Create an data entry header for every Credit Memo type ---------------------------------//        
        var ordHeader = (from Ord in idc.Orders
                         join Acct in idc.Accounts
                            on Ord.AccountID equals Acct.AccountID
                         join Loct in idc.Locations
                            on Ord.LocationID equals Loct.LocationID
                         join Pmt in idc.Payments on Ord.OrderID equals Pmt.OrderID
                         where Ord.OrderID == CreditOrderID
                         select new { Ord, Acct, Loct, Pmt }).FirstOrDefault();
       
        // save Credit Memo detail to SO5_InvoiceDataEntryHeader
        SO5 = new SO5_InvoiceDataEntryHeader();

        SO5.InvoiceNumber = MAS.CleanString(invoiceNumber.PadLeft(7, '0'), 7);        
        SO5.SalesOrderNumber = (originalOrderID > 0) ? MAS.CleanString(originalOrderID.ToString().PadLeft(7, '0'), 7) : "";

        SO5.OrderDate = DateTime.Parse(ordHeader.Ord.OrderDate.ToString()).ToString("MM/dd/yy");
        SO5.CustomerNumber = MAS.CleanString(ordHeader.Ord.AccountID.ToString().PadLeft(7, '0'), 7);

        SO5.BillToName = MAS.CleanString(ordHeader.Loct.BillingCompany, 30);
        SO5.BillToAddress1 = MAS.CleanString(ordHeader.Loct.BillingAddress1, 30);
        SO5.BillToAddress2 = (ordHeader.Loct.BillingAddress2 != null) ? MAS.CleanString(ordHeader.Loct.BillingAddress2, 30) : string.Empty;
        SO5.BillToAddress3 = (ordHeader.Loct.BillingAddress3 != null) ? MAS.CleanString(ordHeader.Loct.BillingAddress3, 30) : string.Empty;
        SO5.BillToCity = MAS.CleanString(ordHeader.Loct.BillingCity, 20);
        SO5.BillToState = (ordHeader != null && ordHeader.Loct != null && ordHeader.Loct.BillingStateID != null) ? (from a in idc.States where a.StateID == ordHeader.Loct.BillingStateID select a.StateAbbrev).First() : string.Empty;
        SO5.BillToZipCode = MAS.CleanString(ordHeader.Loct.BillingPostalCode, 10);
        SO5.BillToCountryCode = (ordHeader != null && ordHeader.Loct != null && ordHeader.Loct.BillingCountryID != null) ? (from a in idc.Countries where a.CountryID == ordHeader.Loct.BillingCountryID select a.CountryCode).First() : string.Empty;

        SO5.ShipToName = MAS.CleanString(ordHeader.Loct.ShippingFirstName + " " + ordHeader.Loct.ShippingLastName, 30);
        SO5.ShipToAddress1 = MAS.CleanString(ordHeader.Loct.ShippingAddress1, 30);
        SO5.ShipToAddress2 = (ordHeader.Loct.ShippingAddress2 != null) ? MAS.CleanString(ordHeader.Loct.ShippingAddress2, 30) : string.Empty;
        SO5.ShipToAddress3 = (ordHeader.Loct.ShippingAddress3 != null) ? MAS.CleanString(ordHeader.Loct.ShippingAddress3, 30) : string.Empty;
        SO5.ShipToCity = MAS.CleanString(ordHeader.Loct.ShippingCity, 20);
        SO5.ShipToState = (ordHeader != null && ordHeader.Loct != null && ordHeader.Loct.ShippingStateID != null) ? (from a in idc.States where a.StateID == ordHeader.Loct.ShippingStateID select a.StateAbbrev).First() : string.Empty;
        SO5.ShipToZipCode = MAS.CleanString(ordHeader.Loct.ShippingPostalCode, 10);
        SO5.ShipToCountryCode = (ordHeader != null && ordHeader.Loct != null && ordHeader.Loct.ShippingCountryID != null) ? (from a in idc.Countries where a.CountryID == ordHeader.Loct.ShippingCountryID select a.CountryCode).First() : string.Empty;

        SO5.FOB = MAS.CleanString(ordHeader.Ord.OrderID.ToString(), 7);  
        SO5.Comment = this.txtComment.Text;
        SO5.WhseCode = "INV";
        
        string strPaymentMethod = ordHeader.Pmt.PaymentMethod.PaymentMethodName;
        decimal orderTotal = 0.00m;
        decimal freightAmount = 0.00m;

        orderTotal = (decimal.Parse(ordHeader.Ord.ShippingCharge.ToString()) * (-1));    // why only ShippingCharge??? But it works so far.

        if (creditMemoType == "Credit Non-Returned Badges" || creditMemoType == "Price Match" || creditMemoType == "Credit Memo" || creditMemoType == "Credit and Rebill")
        {
            freightAmount = (decimal.Parse(ordHeader.Ord.ShippingCharge.ToString()) * (-1));
        }

        if (strPaymentMethod == "Purchase Order") //PO order
        {
            SO5.PaymentType = "CHECK";
            SO5.CustomerPONumber = "CREDITMEMO";
            SO5.DepAmount = 0;
        }
        else if (strPaymentMethod == "Credit Card") //Credit card order (Never use)
        {
            SO5.PaymentType = "CC";
            SO5.CustomerPONumber = "";
            SO5.DepAmount = orderTotal;
        }

        if (strPaymentMethod == "Purchase Order") // 30 days is default
        {
            SO5.TermsCode = "30";
        }
        else if (strPaymentMethod == "Credit Card")
        {
            SO5.TermsCode = "0";
        }

        SO5.ApplyToInvoiceNumberTypeCD = MAS.CleanString(invoiceNumber.PadLeft(7, '0'), 7);
        SO5.Division = "00";
        SO5.Weight = 0;
        SO5.TaxableAmount = 0;
        SO5.NonTaxableAmount = 0;
        SO5.FrghtAmount = freightAmount;        
        SO5.ShipWeight = "0";
        mdc.SO5_InvoiceDataEntryHeaders.InsertOnSubmit(SO5);
        mdc.SubmitChanges();        

        //  ----------------------------- End create an data entry header for every Credit Memo type ------------------------------//

    }// END ApplyCreditMemoToMas

    // 12/15/2015. Fixed device movement discrepancy between MAS and portal
    /// <summary>    
    /// Update warehouse and dis-associate from account on selected serial numbers in portal.
    /// </summary>
    /// <param name="pSelLotSerialNo">List of selected serial number to be creditted</param>
    /// <param name="pDeviceAnalysisStatus">New warehouse for a selected serial number </param>
    protected void UpdateDeviceInventory(string pSerialNo, string pDeviceAnalysisStatus)
    {
        try
        {
            DeviceInventory deviceInventory = (from di in idc.DeviceInventories where di.SerialNo == pSerialNo select di).FirstOrDefault();

            // ************* Update warehouse ********************** //
            DeviceAnalysisStatus myDeviceAnalysisStatus = (from s in idc.DeviceAnalysisStatus where s.DeviceAnalysisName == pDeviceAnalysisStatus select s).FirstOrDefault();
            if (myDeviceAnalysisStatus != null)
            {
                deviceInventory.DeviceAnalysisStatus = myDeviceAnalysisStatus;
                idc.SubmitChanges();

                // Insert DeviceInventoryAudit record
                DeviceManager myDeviceMnager = new DeviceManager();
                bool isFail = false;
                if (pDeviceAnalysisStatus == "Testing" || pDeviceAnalysisStatus == "Committed") isFail = false;
                else isFail = true;
                myDeviceMnager.InsertDeviceInventoryAudit(deviceInventory.DeviceID, deviceInventory.DeviceAnalysisStatus.DeviceAnalysisStatusID, isFail, "Credit Memo", "Portal");
            }
            // ************* Update warehouse ********************** //

            // ************* Deactivate UserDevice ********************** //
            UserDevice UDev = (from a in idc.UserDevices
                               join ad in idc.AccountDevices on a.DeviceID equals ad.DeviceID
                               where a.DeviceID == deviceInventory.DeviceID
                               && a.Active == true
                               && ad.CurrentDeviceAssign == true
                               select a).FirstOrDefault();
            if (UDev != null)
            {
                UDev.DeactivateDate = DateTime.Now;
                UDev.Active = false;
                idc.SubmitChanges();
            }
            // ************* Deactivate UserDevice ********************** //

            // ********** Deactivate AccountDevice ********************//
            IQueryable<AccountDevice> ADev = (from a in idc.AccountDevices
                                              where a.DeviceID == deviceInventory.DeviceID
                                              select a).AsQueryable();
            foreach (AccountDevice ad in ADev)
            {
                ad.Active = false;
                ad.DeactivateDate = DateTime.Now;
                idc.SubmitChanges();
            }
            // ********** Deactivate AccountDevice ********************//
        }
        catch {}        
    }

    /// <summary>
    /// sendSoftraxRecordsCredit
    /// </summary>
    /// <param name="creditOrderID"></param>
    /// <param name="originalOrderID"></param>
    /// <param name="creditTotalAmount"></param>
    /// <param name="creditMemoType"></param>
    /// <returns></returns>
    protected string sendSoftraxRecordsCredit(int creditOrderID, int originalOrderID,
            decimal creditTotalAmount, string creditMemoType, string orderTypeofInvoice)
    {
        string BoolSoftraxSubmit = "";
        try
        {
            InsDataContext insData = new InsDataContext();
            //StoreDataContext stoData = new StoreDataContext();
            // Set the server address if there is an appsetting to commit to 
            // * keep in mind the ServerAddress has a default to the live softrax server.
            if (ConfigurationManager.AppSettings["SoftraxServerAddress"] != null)
                Instadose.Integration.Softrax.Communications.ServerAddress = ConfigurationManager.AppSettings["SoftraxServerAddress"];

            // Query the order header
            Order order =
                (from o in insData.Orders
                 where o.OrderID == creditOrderID
                 select o).First();

            // Query the account
            Account account =
                (from a in insData.Accounts
                 where a.AccountID == order.AccountID
                 select a).First();

            // Query the default location
            Location location =
                (from l in insData.Locations
                 where l.IsDefaultLocation == true &&
                       l.AccountID == account.AccountID
                 select l).First();

            // Query the order details
            List<OrderDetail> orderDetails =
                (from od in insData.OrderDetails
                 where od.OrderID == creditOrderID
                 select od).ToList();


            // query the credit devices detail
            var CreditDevices = (from AcctDC in insData.AccountDeviceCredits
                                 join Ordt in insData.OrderDetails
                                 on AcctDC.OrderDetailID equals Ordt.OrderDetailID 
                                 where Ordt.OrderID == creditOrderID
                                 select new { AcctDC, Ordt }).ToList();

            // Query the Original order header
            var OrgOrderDevice =
                (from a in insData.AccountDevices
                 join b in insData.AccountDeviceDates
                 on a.AccountID equals b.AccountID  
                 where a.AccountID == account.AccountID
                 select new { b }).ToList();

            // Has order been submit to softTrax
            if (order.SoftraxStatus == true )
            {
                BoolSoftraxSubmit = "Successed";
                return BoolSoftraxSubmit;
            }


            // Set the currency code for the header and customer
            String tCurrencyCode = "USD";
            if (order.CurrencyCode != null)
            {
                if (order.CurrencyCode.Length == 3)
                    tCurrencyCode = order.CurrencyCode.ToUpper();
            }

            // 11/20/2012, TDO. The date on Service Start Date and Service End Date controls are used to send to SoftTrax by Craig.
            DateTime ServiceStartDate = DateTime.Parse(this.txtStartDate.Text.Trim());
            DateTime ServiceEndDate = DateTime.Parse(this.txtEndDate.Text.Trim());
            
            // Build the Softrax header.
            ContractHeader stx_head = new ContractHeader()
            {
                AccountID = account.AccountID,
                CurrancyCode = tCurrencyCode,
                OrderID = creditOrderID,
                ServiceEndDate = ServiceEndDate,
                ServiceStartDate = ServiceStartDate,
                NextBillDate = ServiceStartDate
            };

            // Obtain the territory/industry information for the customer.
            String tmpTerritory = "OTH";

            // Check to ensure the industry was found and fill the code to the territory.
            if (account.Industry != null)
                tmpTerritory = (account.Industry.IndustryName.ToUpper().Trim()).Substring(0, 3);            

            string BillingStateAbb = (from s in idc.States
                                      where s.StateID == location.BillingStateID
                                      select s.StateAbbrev).First();

            // Fixed by TDO, 3/17/2014. All fields values assigned to Customer obj should not be null.
            // Build the customer record.
            Instadose.Integration.Softrax.Customer stx_cust = new Instadose.Integration.Softrax.Customer()
            {
                AccountID = account.AccountID,
                AddressLine1 = location.BillingAddress1,
                AddressLine2 = string.IsNullOrEmpty(location.BillingAddress2) ? "" : location.BillingAddress2,
                City = location.BillingCity,
                CompanyName = account.AccountName,
                CurrencyCode = tCurrencyCode,
                Territory = (tmpTerritory == "MED" || tmpTerritory == "DEN") ? "OTH" : tmpTerritory,
                State = BillingStateAbb,
                ZipCode = location.BillingPostalCode,
                Country = location.BillingCountry.CountryCode
                //Country = DataUtilities.CountryNameToCode(location.BillingCountryID.ToString())
            };            

            // If creditting on specific SKUs such as INSTA10BLK, INSTA10BLU, .... then MyProductSKU = ""
            // Else MyProductSKU = "/CREDIT"
            string MyProductSKU = "";            

            if (creditMemoType == "Freight" || creditMemoType == "Price Match" || creditMemoType == "Credit Non-Returned Badges" || (creditMemoType == "Credit Memo" && (orderTypeofInvoice.Contains("NEW") || orderTypeofInvoice.Contains("ADDON") || orderTypeofInvoice.Contains("RENEWAL") || orderTypeofInvoice.Contains("UNSET") || orderTypeofInvoice.Contains("RECALL REPLACEMENT"))))
                    MyProductSKU = "/CREDIT";

            // Create and fill the summary and detail.
            Instadose.Integration.Softrax.SummaryDetail stx_csd = new Instadose.Integration.Softrax.SummaryDetail();

            stx_csd.AccountDeviceID = creditOrderID;
            stx_csd.Coupon = String.Empty;
            stx_csd.ProductRenewal = "INSTAYS";
            stx_csd.IsReplacement = false;
            stx_csd.OrderID = order.OrderID;
            stx_csd.ProductSKU = MyProductSKU;
            stx_csd.SerialNo = String.Empty;
            stx_csd.ServiceStartDate = ServiceStartDate;
            stx_csd.ServiceEndDate = ServiceEndDate;
            stx_csd.Price = (creditTotalAmount * (-1)).ToString();  // Full credit amount '-ve'

            // Create the detail/summary items for softrax
            List<Instadose.Integration.Softrax.SummaryDetail> stx_det = new List<Instadose.Integration.Softrax.SummaryDetail>();

            stx_det.Add(stx_csd);

            // orderwise Fill Contract
            OrderContract stx_oc = new OrderContract();

            // Set the header to the contract.
            stx_oc.customer = stx_cust;
            stx_oc.contractHeader = stx_head;
            stx_oc.contractSummaryDetails = stx_det;

            // Commit to Softrax
            bool resp = stx_oc.CommitToSoftrax();

            if (resp)
            {
                try
                {
                    // Update the order record to set Softrax integration to 1.
                    order.SoftraxStatus  = true;
                    insData.SubmitChanges();
                }
                catch { }

                BoolSoftraxSubmit = "Successed";
            }
            else
            {
                BoolSoftraxSubmit = "Failed";
            }
        }
        catch
        {
            BoolSoftraxSubmit = "Failed";
        }

        return BoolSoftraxSubmit;
    }

    protected void displayConfirmation(int newOrderID, int AccountID, decimal itemCredit, 
        decimal misCredit, decimal shipCredit, decimal totalCredit, 
        string selectedInvoiceNo, string strSoftraxProcess)
    {

        btnCancel.Visible = false;
        btnConfirm.Visible = false;

        lbl_CM_accountid.Text = AccountID.ToString();
        lbl_CM_orderid.Text = newOrderID.ToString();
        
        //lbl_CM_itemCredit.Text = ItemCredit.ToString("c");    
        //lbl_CM_misCredit.Text = MisCredit.ToString("c");
        //lbl_CM_ShippingCredit.Text = ShipCredit.ToString("c");
        //lbl_CM_totalCredit.Text = TotalCredit.ToString("c");

        lbl_CM_itemCredit.Text = Currencies.CurrencyToString(itemCredit, account.CurrencyCode);
        lbl_CM_misCredit.Text = Currencies.CurrencyToString(misCredit, account.CurrencyCode);
        lbl_CM_ShippingCredit.Text = Currencies.CurrencyToString(shipCredit, account.CurrencyCode);
        lbl_CM_totalCredit.Text = Currencies.CurrencyToString(totalCredit, account.CurrencyCode);

        lbl_CM_softwaxProcess.Text = strSoftraxProcess; 

        //read from datatable 
        var ItemArray = (from r in dtDeviceDataTable.AsEnumerable()
                         select r).ToArray();

        string OrderDetailString = "";

        if (ItemArray.Length > 0) // has item in item Table
        {
            OrderDetailString = "<table style='width:100%;border:0;borderStyle:solid' >";
            OrderDetailString += "<tr><td><u><b>Serial#</b></u></td><td><u><b>Invoice#</b></u></td><td><u><b>Warehouse</b></u></td></tr>";

            for (int i = 0; i < ItemArray.Length; i++)
            {

                string mySerial = ItemArray[i].ItemArray[2].ToString();
                //string myInvoice = ItemArray[i].ItemArray[0].ToString();
                string myInvoice = selectedInvoiceNo;
                string myWarehouse = ItemArray[i].ItemArray[10].ToString();

                OrderDetailString += "<tr><td>" + mySerial + "</td>";
                OrderDetailString += "<td>" + myInvoice + "</td>";
                OrderDetailString += "<td>" + myWarehouse + "</td></tr>";
            }

            OrderDetailString += "</table>";
        }
        lbl_CM_OrderDetail.Text = OrderDetailString;

        //this.btn_CM_Search.PostBackUrl = "creditmemosearch.aspx";

    }

    // Example pCurrencyString = 3.00 AUD, (3.11 USD). The function just returns 3.00
    private string getCreditRateAmount(string pCurrencyString)
    {
        try
        {
            string bal1 = pCurrencyString.Split(',')[0];
            string bal2 = bal1.Split(' ')[0];
            return bal2;
        }
        catch
        {
            return "0";
        }
    }

    private void declareSelectItemDataTable()
    {
        //Construct datatable for all selected items
        dtDeviceDataTable = new DataTable();
        dtDeviceDataTable.Columns.Add("InvoiceNo", typeof(string));
        dtDeviceDataTable.Columns.Add("InvoiceDate", typeof(string));
        dtDeviceDataTable.Columns.Add("SerialNo", typeof(string));
        dtDeviceDataTable.Columns.Add("ProductType", typeof(string));
        dtDeviceDataTable.Columns.Add("startDate", typeof(string));
        dtDeviceDataTable.Columns.Add("endDate", typeof(string));
        dtDeviceDataTable.Columns.Add("InRMA", typeof(int));
        dtDeviceDataTable.Columns.Add("ProductID", typeof(int));
        dtDeviceDataTable.Columns.Add("ProductVariantID", typeof(int));
        dtDeviceDataTable.Columns.Add("DeviceID", typeof(int));
        dtDeviceDataTable.Columns.Add("Warehouse", typeof(string));
        dtDeviceDataTable.Columns.Add("CreditRate", typeof(decimal));
    }

    private void buildSelectItemDataTable(string pSelectedInvoiceNo)
    {
        //find if checkbox being checked 
        //build selected itemlist
        for (int i = 0; i < this.gvOrderInvoiceDetails.Rows.Count; i++)
        {
            GridViewRow gvRow = gvOrderInvoiceDetails.Rows[i];
            CheckBox findChkBx = (CheckBox)gvRow.FindControl("chkbxSelect");
            bool SelectedSerial = findChkBx.Checked;

            if (SelectedSerial == true)
            {
                //string SelectedInvoiceNo = gvRow.Cells[1].Text.Replace("\n", "").ToString().Trim();
                string selectedInvoiceNo = pSelectedInvoiceNo;
                int selectedProductVariantID = 0;

                int selectedProductID = int.Parse(((HiddenField)gvRow.FindControl("HidProductID")).Value);                                
                int selectedDeviceID = int.Parse(((HiddenField)gvRow.FindControl("HidDeviceID")).Value);
                decimal selectedCreditRate = decimal.Parse(getCreditRateAmount(((HiddenField)gvRow.FindControl("HidCreditRate")).Value));

                string selectedInvoiceDate = gvRow.Cells[2].Text.Replace("\n", "").ToString().Trim();
                string selectedSerialNo = gvRow.Cells[3].Text.Replace("\n", "").ToString().Trim();
                string selectedProductType = gvRow.Cells[4].Text.Replace("\n", "").ToString().Trim();

                TextBox findtxtStartDate = (TextBox)gvRow.FindControl("txtGVStartDate");
                string selectedstartDate = findtxtStartDate.Text.Trim();

                TextBox findtxtEndDate = (TextBox)gvRow.FindControl("txtGVEndDate");
                string selectedendDate = findtxtEndDate.Text.Trim();               

                int selectedInRMA = 0;

                selectedInRMA = gvRow.Cells[7].Text.ToUpper() == "YES" ? 1 : 0;
                //selectedInRMA = Convert.ToInt32(((CheckBox)gvRow.FindControl("chkbxIsReturn")).Checked);
                
                string selectedWarehouse = "";
                selectedWarehouse = ((RadioButtonList)gvRow.FindControl("rbtnlstWHS")).SelectedValue.ToString();

                // Create the data row to be added to the data table.
                DataRow dr = dtDeviceDataTable.NewRow();
                dr["InvoiceNo"] = selectedInvoiceNo;
                dr["InvoiceDate"] = selectedInvoiceDate;
                dr["SerialNo"] = selectedSerialNo;
                dr["ProductType"] = selectedProductType;
                dr["startDate"] = selectedstartDate;
                dr["endDate"] = selectedendDate;
                dr["InRma"] = selectedInRMA;
                dr["ProductID"] = selectedProductID;
                dr["ProductVariantID"] = selectedProductVariantID;
                dr["DeviceID"] = selectedDeviceID;

                //if (CreditMemoType == "Price Match") // Set warehouse to "N/A" from price match 
                //    dr["Warehouse"] = "N/A";
                //else
                //    dr["Warehouse"] = SelectedWarehouse;
                dr["Warehouse"] = selectedWarehouse;
                dr["CreditRate"] = selectedCreditRate;
                dtDeviceDataTable.Rows.Add(dr);

            }
        }

        // Sort the contents of the data table.
        dtDeviceDataTable.DefaultView.Sort = "InvoiceNo, Serialno";
        
    }

    protected void ReadSelectedItemDataTable()
    { 
        //read from datatable 
        var ItemArray = (from r in dtDeviceDataTable.AsEnumerable()
                     select r).ToArray();
        for (int i = 0; i < ItemArray.Length; i++)
        {
            string Serialno = ItemArray[i].ItemArray[2].ToString();
            string mydate = ItemArray[i].ItemArray[4].ToString();
        }
    }

    #region OtherFuntions

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

    protected void SetEnableInputControls( bool pTrueFalse)
    {
        this.txtStartDate.Enabled = pTrueFalse;
        this.txtEndDate.Enabled = pTrueFalse;
        this.chkboxAllDate.Enabled = pTrueFalse;
        this.txtCreditItem.Enabled = pTrueFalse;
        //this.txtCreditMisc.Enabled = pTrueFalse;
        this.txtCreditShipping.Enabled = pTrueFalse;
    }

    protected void ResetDefaultValues()
    {
        txtCreditItem.Text = "0.00";
        lblItemCreditUSD.Text = "USD"; 

        //txtCreditMisc.Text = "0.00";
        //lblMiscellaneousCreditUSD.Text = "USD"; 

        txtCreditShipping.Text = "0.00";
        lblShippingCreditUSD.Text = "USD"; 

        txtTotalCredit.Text = "0.00";
        lblTotalCreditUSD.Text = "USD"; 

        txtStartDate.Text = "";
        this.txtEndDate.Text = "";

        this.txtComment.Text = "";

        this.chkboxAllDate.Checked = false;
        InvisibleErrors();
    }

    // calculate daily rate for device based on the order price
    public string calculateRate(string startDate, string endDate, string price)
    {
        string returnStr = "0.00";
        try
        {
            

            if ((endDate != null && startDate != null) && (endDate != "" && startDate != ""))
            {
                price = (price == "") ? "0" : price;
                double totalDay = 0.00;
                TimeSpan dateDif;

                dateDif = DateTime.Parse(endDate) - DateTime.Parse(startDate);
                totalDay = dateDif.Days;

                if (totalDay == 365) totalDay = 365.25;
                returnStr = (double.Parse(price) / totalDay).ToString("0.00");
            }
 
            returnStr = Currencies.CurrencyToString(Convert.ToDecimal(returnStr), account.CurrencyCode);

            return returnStr;
        }
        catch (Exception ex)
        {
            return returnStr;
        }        
    }

    /// <summary>
    /// Calculate day differnece: (end - start) dates.
    /// </summary>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <returns></returns>
    public string calculateDayDiff(string startDate, string endDate)
    {

        string returnStr = "";
        if ((endDate != null && startDate != null) && (endDate != "" && startDate != ""))
        {

            TimeSpan dateDif;
            dateDif = DateTime.Parse(endDate) - DateTime.Parse(startDate);
            returnStr = dateDif.Days.ToString();
        }

        return returnStr;

    }

    public string FuncDayRemaining(string endDate)
    {
        string RtrStr = "";
        if (endDate != null)
        {

            TimeSpan dateDif;
            dateDif = DateTime.Parse(endDate) - DateTime.Now;

            RtrStr = dateDif.Days.ToString();
        }

        return RtrStr;

    }
   
    public string DisplayQtyCountText(Object OrderId)
    {
        string DisplayText = "0";
        if (OrderId.ToString () != "")
        { 
             var vQ = (from a in idc.OrderDetails
                      where a.OrderID == int.Parse(OrderId.ToString())
                            select new { a.ProductID, a.Quantity}).ToList();

            if (vQ.Count() != 0)
            {
                int myQtyCount = 0;
                foreach (var v in vQ)
                {
                    myQtyCount += v.Quantity;
                }
                DisplayText = myQtyCount.ToString();
            }
        }
        return DisplayText;
    }

    private decimal ToDecimal(string Value)
    {
        if (Value.Length == 0)
            return 0;
        else
            return Decimal.Parse(Value.Replace(" ", ""), NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint | NumberStyles.AllowCurrencySymbol);
    }

    protected void LoadDDLInvoice()
    {     
        string creditMemoType= "";

        if(rbtnCreditMemo.Checked)
            creditMemoType = "Credit Memo";
        else if (rbtnPriceMatch.Checked )
            creditMemoType = "Price Match";
        else if (rbtnFreight.Checked )
            creditMemoType = "Freight";
        else if (rbtnCreditAndRebill.Checked )
            creditMemoType = "Credit and Rebill";
        else if (rbtnCreditWithBadges.Checked )
            creditMemoType = "Credit Non-Returned Badges";
        else if (rbtnBadDebt.Checked )
            creditMemoType = "Bad Debt";

        this.ddlInvoice.DataSource = GetInvoicesByAccountID(accountID, creditMemoType);

        ddlInvoice.DataBind();
        ddlInvoice.SelectedIndex = 0;
     
    }

    protected void rbtnCreditMemo_CheckedChanged(object sender, EventArgs e)
    {
        this.creditTypeDesc.InnerText = "This is a credit memo for badges that has been returned and received.";
        if (rbtnCreditMemo.Checked)
        {
            LoadDDLInvoice();

            this.gvOrderInvoiceDetails.DataBind();
            gvOrderInvoiceDetails.Visible = false;            

            ResetDefaultValues();
            SetEnableInputControls(true);
            //this.txtCreditMisc.Enabled = false;            

            //// Tdo, 5/23/2013
            //// When selecting Credit Memo type, enable ItemCredit only for NEW/RENEWAL/RECALLREPLACEMENT/UNSET order, disable the rest            
            //if (ddlInvoice.SelectedItem.Text.Contains("NEW") || ddlInvoice.SelectedItem.Text.Contains("ADDON") ||ddlInvoice.SelectedItem.Text.Contains("RENEWAL") || ddlInvoice.SelectedItem.Text.Contains("RECALL REPLACEMENT") || ddlInvoice.SelectedItem.Text.Contains("UNSET"))
            //    this.txtCreditItem.Enabled = true;
            //else
            //    this.txtCreditItem.Enabled = false;

            this.chkboxAllDate.Enabled = false;

            SetDefaultStartDateEndDate();
        } 
    }

    protected void rbtnPriceMatch_CheckedChanged(object sender, EventArgs e)
    {
        this.creditTypeDesc.InnerText = "This is a credit memo for Price Adjustments.";
        if (rbtnPriceMatch.Checked)
        {
            LoadDDLInvoice();

            this.gvOrderInvoiceDetails.DataBind();
            gvOrderInvoiceDetails.Visible = false;           

            ResetDefaultValues();
            SetEnableInputControls(true);
            //this.txtCreditMisc.Enabled = false;
            this.txtCreditShipping.Enabled = false;       

            this.chkboxAllDate.Enabled = false;                          
            
            SetDefaultStartDateEndDate();
        }
    }

    protected void rbtnCreditWithBadges_CheckedChanged(object sender, EventArgs e)
    {
        this.creditTypeDesc.InnerText = "This is a credit memo for Non-Returned badges.";
        if (rbtnCreditWithBadges.Checked)
        {
            LoadDDLInvoice();

            this.gvOrderInvoiceDetails.DataBind();
            gvOrderInvoiceDetails.Visible = true;

            ResetDefaultValues();

            //SetEnableInputControls(false);
            //this.txtCreditItem.Enabled = true;
            //this.txtCreditShipping.Enabled = true;

            //SetEnableInputControls(true );
            //this.chkboxAllDate.Enabled = false;
            //SetDefaultStartDateEndDate();

            if (ddlInvoice.SelectedItem.Text.Contains("LOST REPLACEMENT") || ddlInvoice.SelectedItem.Text.Contains("NO REPLACEMENT"))
            {
                SetEnableInputControls(false);
                this.txtCreditItem.Enabled = true;
                this.txtCreditShipping.Enabled = true;
            }
            else
            {
                SetEnableInputControls(true);
                this.chkboxAllDate.Enabled = false;
                SetDefaultStartDateEndDate();
            }                

        }
    }

    protected void rbtnFreight_CheckedChanged(object sender, EventArgs e)
    {
        this.creditTypeDesc.InnerText = "This is a credit/write off for shipping charge only.";
        if (rbtnFreight.Checked)
        {
            LoadDDLInvoice();

            this.gvOrderInvoiceDetails.DataBind();
            gvOrderInvoiceDetails.Visible = false;

            ResetDefaultValues();
            SetEnableInputControls(false);

            //this.txtCreditItem.Enabled = true;    // for testing 
            this.txtCreditShipping.Enabled = true; 
        }       
    }

    protected void rbtnCreditAndRebill_CheckedChanged(object sender, EventArgs e)
    {
        this.creditTypeDesc.InnerText = "This is a credit memo in full and the badges are back in INV.";
        if (rbtnCreditAndRebill.Checked)
        {
            LoadDDLInvoice();

            this.gvOrderInvoiceDetails.DataBind();
            gvOrderInvoiceDetails.Visible = false;

            ResetDefaultValues();
            SetEnableInputControls(true);
            //this.txtCreditMisc.Enabled = false;     

            this.chkboxAllDate.Enabled = false;
                  
            SetDefaultStartDateEndDate();
        }
    }    

    protected void rbtnBadDebt_CheckedChanged(object sender, EventArgs e)
    {
        this.creditTypeDesc.InnerText = "This is a credit for bad debt/write off and the badges are not returned.";
        if (rbtnBadDebt.Checked)
        {
            LoadDDLInvoice();

            this.gvOrderInvoiceDetails.DataBind();          
            gvOrderInvoiceDetails.Visible = true;

            ResetDefaultValues();
            SetEnableInputControls(false);
            this.txtCreditItem.Enabled = true;
            this.txtCreditShipping.Enabled = true;         
        }
    }

    #endregion
    
    protected void ddlInvoice_SelectedIndexChanged(object sender, EventArgs e)
    {
        // refresh grid if Credit with non-returned badges or BadDebt, others do not need to refresh since no need to show grid 
        if (this.rbtnCreditWithBadges.Checked || this.rbtnBadDebt.Checked)
        {
            this.gvOrderInvoiceDetails.DataBind();
        }
        
        //ResetDefaultValues();
        //SetDefaultStartDateEndDate();        


        ResetDefaultValues();

        if (rbtnCreditWithBadges.Checked)
        {                        
            if (ddlInvoice.SelectedItem.Text.Contains("LOST REPLACEMENT") || ddlInvoice.SelectedItem.Text.Contains("NO REPLACEMENT"))
            {
                SetEnableInputControls(false);
                this.txtCreditItem.Enabled = true;
                this.txtCreditShipping.Enabled = true;
            }
            else
            {
                SetEnableInputControls(true);
                this.chkboxAllDate.Enabled = false;
                SetDefaultStartDateEndDate();
            }
        }
        else
        {            
            SetDefaultStartDateEndDate();
        }
    }

    protected void btnBack_Click(object sender, EventArgs e)
    {
        btnConfirm.Visible = true;
        btnCancel.Visible = true;

        divCreditMemoDetail.Visible = true;
        divCreditMemoResult.Visible = false;

        this.gvOrderInvoiceDetails.DataBind();
        ResetDefaultValues();
    }

    protected void btnCancel_Click(object sender, EventArgs e)
    {
        Response.Redirect(string.Format("~/InformationFinder/Details/Account.aspx?ID={0}#Order_tab", account.AccountID));
    }

    protected void btnCreditMemoSearch_Click(object sender, EventArgs e)
    {
        Response.Redirect(string.Format("CreditMemoSearch.aspx"));
    }   

    private void GetServiceStartDateEndDateByInvoiceNo(string pInvoiceNo, ref string pServiceStartDate, ref string pServiceEndDate)
    {        
        for (int i = 0; i < this.gvInvoices.Rows.Count; i++)
        {
            GridViewRow gvRow = gvInvoices.Rows[i];

            string selectedInvoiceNo = gvRow.Cells[2].Text.Trim();
            string selectedInvoiceDate = gvRow.Cells[3].Text.Trim();            
            HyperLink findOrderNumber = (HyperLink)gvRow.FindControl("hyprlnkOrderNumber");
            string selectedOrderID = findOrderNumber.Text;

            if (selectedInvoiceNo == pInvoiceNo)
            {
                if (selectedOrderID != "")
                {
                    GetServiceStartDateEndDateByOrderID(int.Parse(selectedOrderID), ref pServiceStartDate, ref pServiceEndDate);
                    break;
                }                
            }
        }        
    }

    private string GetInvoiceDateByInvoiceNo(string pInvoiceNo)
    {
        //find if checkbox being checked 
        //build selected itemlist
        for (int i = 0; i < this.gvInvoices.Rows.Count; i++)
        {
            GridViewRow gvRow = gvInvoices.Rows[i];

            string selectedInvoiceNo = gvRow.Cells[2].Text.Trim();
            string selectedInvoiceDate = gvRow.Cells[3].Text.Trim();

            if (selectedInvoiceNo == pInvoiceNo)
            {
                return selectedInvoiceDate;
            }
        }
        return System.DateTime.Today.ToShortDateString();
    }

    private DataTable GetInvoicesByAccountID(int accountID, string creditMemoType)
    {
        try
        {
            String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            String cmdStr = "sp_GetInvoicesByAccountIDandCreditType";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;

            sqlCmd.Parameters.Add("@AccountID", SqlDbType.Int);
            sqlCmd.Parameters["@AccountID"].Value = accountID;

            sqlCmd.Parameters.Add("@CreditType", SqlDbType.VarChar, 30);
            sqlCmd.Parameters["@CreditType"].Value = creditMemoType;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);

            sqlConn.Close();

            return dt;
        }
        catch { return null; }
    }

    private void GetServiceStartDateEndDateByOrderID(int pOrderID, ref string pServiceStartDate, ref string pServiceEndDate)
    {
        try
        {
            String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            String cmdStr = "sp_GetServiceStartDateEndDateByOrderID";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.Add("@OrderID", SqlDbType.Int);

            sqlCmd.Parameters["@OrderID"].Value = pOrderID;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);

            sqlConn.Close();

            if (dt != null)
            {
                pServiceStartDate = (dt.Rows[0]["StartDt"] != null) ? dt.Rows[0]["StartDt"].ToString() : "";
                pServiceEndDate = (dt.Rows[0]["EndDt"] != null) ? dt.Rows[0]["EndDt"].ToString() : "";
            }
            else
            {
                pServiceStartDate = "";
                pServiceEndDate = "";
            }
        }
        catch
        {
            pServiceStartDate = "";
            pServiceEndDate = "";
        }

    }
    
}

