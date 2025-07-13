using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using Instadose;
using Instadose.Data;

public partial class IT_DeleteOrders : System.Web.UI.Page
{
    public int orderID = 0;

    public static string orderTotal;
    public static string shippingTotal;

    InsDataContext idc = new InsDataContext();

    /// <summary>
    /// Limits access to the functionality based on groups.
    /// </summary>
    public string UserName = "Unknown";
    public string[] ActiveDirecoryGroups = { "IRV-IT" };

    protected void Page_Load(object sender, EventArgs e)
    {
        try { UserName = Page.User.Identity.Name.Split('\\')[1]; }
        catch { UserName = "Unknown"; }

        bool belongsToGroups = ActiveDirectoryQueries.UserExistsInAnyGroup(UserName, ActiveDirecoryGroups);
        // IF the User exists in the Required Group, THEN make the page content visible.
        if (belongsToGroups)
        {
            InvisibleErrors();
            InvisibleSuccesses();
            InvisibleModalErrors();

            if (Page.IsPostBack) return;

            this.fvOrderInformation.Visible = false;
            this.gvOrderDetails.Visible = false;
            this.gvPackageDetails.Visible = false;
        }
        else
        {
            Response.Redirect("~/Default.aspx");
        }
    }

    protected void btnConfirmOrderDeletion_Click(object sender, EventArgs e)
    {
        InvisibleErrors();
        InvisibleSuccesses();
        InvisibleModalErrors();

        int.TryParse(this.lblModalOrderID.Text, out orderID);

        if (this.lblModalOrderID.Text == "")
        {
            VisibleModalErrors("An Order # was not entered.");
            return;
        }

        List<string> tableNames = new List<string>();

        // Packages
        try
        {
            var packageRecords = idc.Packages.Any(pc => pc.OrderID == orderID);
            if (packageRecords)
                throw new Exception("Order has been packed and cannot be deleted.");

            // Orders
            var orderRecord = idc.Orders.Where(o => o.OrderID == orderID && o.OrderStatusID != 10).FirstOrDefault();
            if (orderRecord.OrderStatusID > 2)
                throw new Exception("Order cannot be cancelled.");
            orderRecord.OrderStatusID = 10;

            string successMessage = "The Order records has been successfully deleted. The following tables were effected; ";

            // Check each table affected by either OrderID or OrderDetailsID and delete the record.
            // This has to be done in a specific order so as to ensure no orphan records remain.

            // Cases
            var caseRecords = (from c in idc.Cases where c.OrderID == orderID select c).ToList();

            if (caseRecords.Count != 0)
            {
                foreach (var cases in caseRecords)
                {
                    idc.CaseNotes.DeleteAllOnSubmit(cases.CaseNotes);
                }
                idc.Cases.DeleteAllOnSubmit(caseRecords);
                tableNames.Add("Cases"); // Add Cases table to List.
            }

            // Documents
            var documentRecords = (from d in idc.Documents where d.OrderID == orderID select d).ToList();

            if (documentRecords.Count != 0)
            {
                foreach (var doc in documentRecords)
                {
                    doc.Active = false;
                }
                tableNames.Add("Documents"); // Add Documents table to List.
            }

            // Order User Assign
            var orderUserAssignRecords = (from oua in idc.OrderUserAssigns where oua.OrderID == orderID select oua).ToList();

            if (orderUserAssignRecords.Count != 0)
            {
                idc.OrderUserAssigns.DeleteAllOnSubmit(orderUserAssignRecords);

                tableNames.Add("OrderUserAssigns"); // Add OrderUserAssigns table to List.
            }


            // Renewal Billing Devices
            var renewalBillingDeviceRecords = (from rbd in idc.RenewalBillingDevices where rbd.OrderID == orderID select rbd).ToList();

            if (renewalBillingDeviceRecords.Count != 0)
            {
                idc.RenewalBillingDevices.DeleteAllOnSubmit(renewalBillingDeviceRecords);

                tableNames.Add("RenewalBillingDevices"); // Add RenewalBillingDevices table to List.
            }

            // Renewal Logs
            var renewalLogRecords = (from rl in idc.RenewalLogs where rl.OrderID == orderID select rl).ToList();

            if (renewalLogRecords.Count != 0)
            {
                idc.RenewalLogs.DeleteAllOnSubmit(renewalLogRecords);
                tableNames.Add("RenewalLogs"); // Add RenewalLogs table to List.
            }

            // Scheduled Billings
            var scheduledBillingRecords = (from sb in idc.ScheduledBillings where sb.OrderID == orderID select sb).ToList();

            if (scheduledBillingRecords.Count != 0)
            {
                idc.ScheduledBillings.DeleteAllOnSubmit(scheduledBillingRecords);

                tableNames.Add("ScheduledBillings"); // Add ScheduledBillings table to List.
            }

            // Account Device Credits
            var accountDeviceCreditRecords = (from adc in idc.AccountDeviceCredits
                                              where adc.OrderDetail.OrderID == orderID
                                              select adc).ToList();

            if (accountDeviceCreditRecords.Count != 0)
            {
                idc.AccountDeviceCredits.DeleteAllOnSubmit(accountDeviceCreditRecords);

                tableNames.Add("AccountDeviceCredits"); // Add AccountDeviceCredits table to List.
            }

            string listOfTableNames = string.Join(", ", tableNames);

            idc.SubmitChanges();
            VisibleSuccesses(successMessage + listOfTableNames);
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('confirmOrderDeletion')", true);

            if (Page.IsPostBack)
            {
                // Reset OrderID/Number Search TextBox.
                this.txtEnterOrderNumber.Text = string.Empty;

                // Make all information invisible.
                this.fvOrderInformation.Visible = false;
                this.gvOrderDetails.Visible = false;
                this.gvPackageDetails.Visible = false;

                // Reset Modal OrderID/Number label.
                this.lblModalOrderID.Text = string.Empty;

                // Reset Modal Error Message.
                InvisibleModalErrors();
            }
        }
        catch (Exception ex)
        {
            VisibleErrors(ex.ToString());
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('confirmOrderDeletion')", true);
            return;
        }
    }

    /// <summary>
    /// Close Modal/Dialog.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnModalCancel_Click(object sender, EventArgs e)
    {
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('confirmOrderDeletion')", true);
    }

    /// <summary>
    /// Populates FormView with (General) Order Information.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnSearchOrderNumber_Click(object sender, EventArgs e)
    {
        InvisibleErrors();
        InvisibleSuccesses();
        InvisibleModalErrors();

        if (this.txtEnterOrderNumber.Text == "") return;

        int.TryParse(this.txtEnterOrderNumber.Text, out orderID);

        var orderRecord = idc.Orders.Where(o => o.OrderID == orderID && o.OrderStatusID != 10).FirstOrDefault();

        if (orderRecord == null)
        {
            // Error Message.
            VisibleErrors("The Order # you entered has either been deleted or does not exist.");
            this.fvOrderInformation.Visible = false;
            this.gvOrderDetails.Visible = false;
            this.gvPackageDetails.Visible = false;
        }
        else
        {
            // Store the OrderID/OrderNo in the page's HiddenField.
            // Will used in the Order Deletion function.
            this.hdnfldOrderID.Value = orderID.ToString();

            // Create the SQL Connection from the connection string in the web.config
            SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
                ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

            // Create the SQL Command.
            SqlCommand sqlCmd = new SqlCommand();
            sqlCmd.CommandType = CommandType.StoredProcedure;

            // Using the sp_if_GetOrderHeaderByNo.
            string sqlQuery = "sp_if_GetOrderHeaderByNo";

            sqlCmd.Parameters.Add(new SqlParameter("@OrderNo", orderID));

            // Pass the SQL Query String to the SQL Command.
            sqlCmd.Connection = sqlConn;
            bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
            if (!openConn) sqlCmd.Connection.Open();
            sqlCmd.CommandText = sqlQuery;
            SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

            // Create a Date Table to hold the contents/results.
            DataTable dtOrderInformationTable = new DataTable();

            dtOrderInformationTable = new DataTable("Get Order Information");

            // Create the columns for the DataTable.
            dtOrderInformationTable.Columns.Add("OrderNo", Type.GetType("System.Int32"));
            dtOrderInformationTable.Columns.Add("AccountNo", Type.GetType("System.Int32"));
            dtOrderInformationTable.Columns.Add("AccountName", Type.GetType("System.String"));
            dtOrderInformationTable.Columns.Add("Status", Type.GetType("System.String"));
            dtOrderInformationTable.Columns.Add("Source", Type.GetType("System.String"));
            dtOrderInformationTable.Columns.Add("CreatedDate", Type.GetType("System.DateTime"));
            dtOrderInformationTable.Columns.Add("TrackingNumber", Type.GetType("System.String"));
            dtOrderInformationTable.Columns.Add("PaymentMethod", Type.GetType("System.String"));
            dtOrderInformationTable.Columns.Add("ShipDate", Type.GetType("System.String"));
            dtOrderInformationTable.Columns.Add("PONumber", Type.GetType("System.String"));
            dtOrderInformationTable.Columns.Add("Coupon", Type.GetType("System.String"));
            dtOrderInformationTable.Columns.Add("CurrencyCode", Type.GetType("System.String"));
            dtOrderInformationTable.Columns.Add("OrderShipping", Type.GetType("System.Decimal"));
            dtOrderInformationTable.Columns.Add("Referral", Type.GetType("System.String"));
            dtOrderInformationTable.Columns.Add("OrderTax", Type.GetType("System.Decimal"));
            dtOrderInformationTable.Columns.Add("SoftTraxIntegration", Type.GetType("System.Boolean"));
            dtOrderInformationTable.Columns.Add("MiscCredit", Type.GetType("System.Decimal"));
            dtOrderInformationTable.Columns.Add("OrderSubtotal", Type.GetType("System.Decimal"));
            dtOrderInformationTable.Columns.Add("SpecialInstructions", Type.GetType("System.String"));
            dtOrderInformationTable.Columns.Add("OrderTotal", Type.GetType("System.Decimal"));
            // Formatted Currency amounts.
            dtOrderInformationTable.Columns.Add("FormattedShipping", Type.GetType("System.String"));
            dtOrderInformationTable.Columns.Add("FormattedTax", Type.GetType("System.String"));
            dtOrderInformationTable.Columns.Add("FormattedCredit", Type.GetType("System.String"));
            dtOrderInformationTable.Columns.Add("FormattedSubtotal", Type.GetType("System.String"));
            dtOrderInformationTable.Columns.Add("FormattedTotal", Type.GetType("System.String"));

            while (sqlDataReader.Read())
            {
                DataRow drow = dtOrderInformationTable.NewRow();

                decimal shipping = 0;
                decimal tax = 0;
                decimal credit = 0;
                decimal subtotal = 0;
                decimal total = 0;

                decimal.TryParse(sqlDataReader["OrderShipping"].ToString(), out shipping);
                decimal.TryParse(sqlDataReader["OrderTax"].ToString(), out tax);
                decimal.TryParse(sqlDataReader["MiscCredit"].ToString(), out credit);
                decimal.TryParse(sqlDataReader["OrderSubtotal"].ToString(), out subtotal);
                decimal.TryParse(sqlDataReader["OrderTotal"].ToString(), out total);

                string currencyCode = sqlDataReader["CurrencyCode"].ToString();

                // Fill row details.
                drow["OrderNo"] = sqlDataReader["OrderNo"];
                drow["AccountNo"] = sqlDataReader["AccountNo"];
                drow["AccountName"] = sqlDataReader["AccountName"];
                drow["Status"] = sqlDataReader["Status"];
                drow["Source"] = sqlDataReader["Source"];
                drow["CreatedDate"] = sqlDataReader["CreatedDate"];
                drow["TrackingNumber"] = sqlDataReader["TrackingNumber"];
                drow["PaymentMethod"] = sqlDataReader["PaymentMethod"];
                drow["ShipDate"] = sqlDataReader["ShipDate"];
                drow["PONumber"] = sqlDataReader["PONumber"];
                drow["Coupon"] = sqlDataReader["Coupon"];
                drow["FormattedShipping"] = Currencies.CurrencyToString(shipping, currencyCode);
                drow["Referral"] = sqlDataReader["Referral"];
                drow["FormattedTax"] = Currencies.CurrencyToString(tax, currencyCode);
                drow["SoftTraxIntegration"] = sqlDataReader["SoftTraxIntegration"];
                drow["FormattedCredit"] = Currencies.CurrencyToString(credit, currencyCode);
                drow["FormattedSubtotal"] = Currencies.CurrencyToString(subtotal, currencyCode);
                drow["SpecialInstructions"] = sqlDataReader["SpecialInstructions"];
                drow["FormattedTotal"] = Currencies.CurrencyToString(total, currencyCode);

                // Add rows to DataTable.
                dtOrderInformationTable.Rows.Add(drow);
            }

            // Assign values to webpage controls (labels).
            this.fvOrderInformation.DataSource = dtOrderInformationTable;
            this.fvOrderInformation.DataBind();

            // Populate Modal/Dialog with OrderID value.
            lblModalOrderID.Text = orderID.ToString();

            this.fvOrderInformation.Visible = true;
            this.gvOrderDetails.Visible = true;
            this.gvPackageDetails.Visible = true;

            // Order Details GridView.
            BindOrderDetailsGridView(orderID);

            // Package Details GridView.
            BindPackageDetailsGridView(orderID);

            sqlConn.Close();
            sqlDataReader.Close();
        }
    }

    /// <summary>
    /// Formats Boolean value 1/0 to Yes/No string.
    /// </summary>
    /// <param name="boolean"></param>
    /// <returns></returns>
    public string YesNo(object boolean)
    {
        try
        {
            return Convert.ToBoolean(boolean) ? "Yes" : "No";
        }
        catch { return "No"; }
    }

    /// <summary>
    /// Populate Order Details GridView.
    /// </summary>
    private void BindOrderDetailsGridView(int orderid)
    {
        string price = "";
        string amount = "";

        //find currency code.
        string currencyCode = (from o in idc.Orders
                               where o.OrderID == orderid
                               select o.CurrencyCode).FirstOrDefault();

        if (currencyCode == null) return;

        // Create the SQL Connection from the connection string in the web.config.
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.StoredProcedure;

        sqlCmd.Parameters.Add(new SqlParameter("@OrderNo", orderid));

        // Pass the query string to the command

        sqlCmd.Connection = sqlConn;

        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        //**********************************************************************************
        // 8/2013 WK - AccountDiscounts is in place but not used since it will NOT
        //             be accurate.   Product discounts need to be included in OrderDetails
        //             if it is be used.
        //**********************************************************************************
        sqlCmd.CommandText = "sp_if_GetOrderDetailsByOrderNo";

        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        DataTable dtOrderDetailsTable = new DataTable();

        // Create the Order Details DataTable to hold the contents of the Order Details.
        dtOrderDetailsTable = new DataTable("Order Details Table");

        // Create the columns for the Order Details DataTable.
        dtOrderDetailsTable.Columns.Add("SKU", Type.GetType("System.String"));
        dtOrderDetailsTable.Columns.Add("ProductName", Type.GetType("System.String"));
        dtOrderDetailsTable.Columns.Add("ProductVariant", Type.GetType("System.String"));
        dtOrderDetailsTable.Columns.Add("Price", Type.GetType("System.String"));
        dtOrderDetailsTable.Columns.Add("Quantity", Type.GetType("System.Int32"));
        dtOrderDetailsTable.Columns.Add("LineTotal", Type.GetType("System.String"));

        while (sqlDataReader.Read())
        {
            // Create a new review order row.
            DataRow dr = dtOrderDetailsTable.NewRow();

            // Fill row details.
            dr["SKU"] = sqlDataReader["SKU"].ToString();
            dr["ProductName"] = sqlDataReader["ProductName"].ToString();
            dr["ProductVariant"] = sqlDataReader["ProductVariant"].ToString();
            //Format price.
            price = sqlDataReader["Price"].ToString();
            price = Currencies.CurrencyToString(Convert.ToDecimal(price), currencyCode);
            dr["Price"] = price;
            dr["Quantity"] = sqlDataReader["Quantity"].ToString();
            //Format each product amount.
            amount = sqlDataReader["LineTotal"].ToString();
            amount = Currencies.CurrencyToString(Convert.ToDecimal(amount), currencyCode);
            dr["LineTotal"] = amount;

            // Add row to the data table.
            dtOrderDetailsTable.Rows.Add(dr);
        }

        this.gvOrderDetails.Columns[3].HeaderText = "Unit Price".PadLeft(10);

        this.gvOrderDetails.DataSource = dtOrderDetailsTable;
        this.gvOrderDetails.DataBind();

        sqlConn.Close();
        sqlDataReader.Close();
    }

    /// <summary>
    /// Populate Package Details GridView.
    /// </summary>
    /// <param name="orderid"></param>
    private void BindPackageDetailsGridView(int orderid)
    {
        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.Text;

        string sqlQuery = "SELECT * FROM Packages WHERE OrderID = " + orderid;

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable dtPackageDetailsDateTable = new DataTable();

        dtPackageDetailsDateTable = new DataTable("Get Package Details");

        // Create the columns for the DataTable.
        dtPackageDetailsDateTable.Columns.Add("PackageID", Type.GetType("System.Int32"));
        dtPackageDetailsDateTable.Columns.Add("OrderID", Type.GetType("System.Int32"));
        dtPackageDetailsDateTable.Columns.Add("TrackingNumber", Type.GetType("System.String"));
        dtPackageDetailsDateTable.Columns.Add("ShipDate", Type.GetType("System.DateTime"));

        while (sqlDataReader.Read())
        {
            DataRow drow = dtPackageDetailsDateTable.NewRow();

            // Fill row details.
            drow["PackageID"] = sqlDataReader["PackageID"];
            drow["OrderID"] = sqlDataReader["OrderID"];
            drow["TrackingNumber"] = sqlDataReader["TrackingNumber"];
            drow["ShipDate"] = sqlDataReader["ShipDate"];

            // Add rows to DataTable.
            dtPackageDetailsDateTable.Rows.Add(drow);
        }

        // Bind the results to the GridView.
        this.gvPackageDetails.DataSource = dtPackageDetailsDateTable;
        this.gvPackageDetails.DataBind();

        sqlConn.Close();
        sqlDataReader.Close();
    }

    /// <summary>
    /// Populates Packing/Shipping Information GridView.
    /// Tracking Information, Shipped Date, and Shipping Address.
    /// </summary>
    /// <param name="packageid"></param>
    /// <returns></returns>
    public string GeneratePackShippingInfo(string packageid)
    {
        string returnPackingString = "";

        var packingInfo = (from b in idc.Packages
                           join c in idc.States
                           on b.StateID equals c.StateID
                           join d in idc.Countries
                           on b.CountryID equals d.CountryID
                           where b.PackageID == int.Parse(packageid)
                           select new { b, c, d }).FirstOrDefault();

        if (packingInfo == null) return "";

        returnPackingString = (packingInfo.b.Company != null && packingInfo.b.Company != "") ? packingInfo.b.Company + "</br>" : "";
        returnPackingString += packingInfo.b.FirstName + " ";
        returnPackingString += packingInfo.b.LastName + "</br>";
        returnPackingString += packingInfo.b.Address1;
        returnPackingString += (packingInfo.b.Address2 != null && packingInfo.b.Address2 != "") ?
                     ", " + packingInfo.b.Address2 : "";
        returnPackingString += (packingInfo.b.Address3 != null && packingInfo.b.Address3 != "") ?
                     ", " + packingInfo.b.Address3 : "";
        returnPackingString += "</br>";
        returnPackingString += packingInfo.b.City + ", ";
        returnPackingString += packingInfo.c.StateAbbrev + " ";
        returnPackingString += packingInfo.b.PostalCode + "</br>";
        returnPackingString += packingInfo.d.CountryName + "</br>";

        return returnPackingString;
    }

    /// <summary>
    /// Populates Packing/Shipping Information GridView.
    /// List of SerialNumbers (Badges/Devices) that were part of the Order.
    /// </summary>
    /// <param name="packageid"></param>
    /// <returns></returns>
    public string GeneratePackSerialNo(string packageid)
    {
        string returnSerialNumberList = "";

        var serialNumbers = (from a in idc.PackageOrderDetails
                             join b in idc.DeviceInventories
                             on a.DeviceID equals b.DeviceID
                             where a.PackageID == int.Parse(packageid)
                             select b.SerialNo).ToList();

        if (serialNumbers.Count == 0) return "";

        returnSerialNumberList += "Total: " + serialNumbers.Count.ToString() + "</br>";

        foreach (var v in serialNumbers)
        {
            returnSerialNumberList += "#" + v.ToString() + ", ";
        }
        int lastIndex = returnSerialNumberList.LastIndexOf(",");
        if (lastIndex > 0)
            returnSerialNumberList = returnSerialNumberList.Substring(0, lastIndex);

        return returnSerialNumberList;
    }

    # region Error & Success Messages.
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

    private void InvisibleSuccesses()
    {
        this.divSuccesses.Visible = false;
        this.successMsg.InnerText = "";
    }

    private void VisibleSuccesses(string success)
    {
        this.divSuccesses.Visible = true;
        this.successMsg.InnerText = success;
    }

    private void InvisibleModalErrors()
    {
        this.divModalError.Visible = false;
        this.modalErrorMessage.InnerText = "";
    }

    private void VisibleModalErrors(string error)
    {
        this.divModalError.Visible = true;
        this.modalErrorMessage.InnerText = error;
    }
    # endregion
}