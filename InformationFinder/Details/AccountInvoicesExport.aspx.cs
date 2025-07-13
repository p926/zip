using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

using Instadose;
using Instadose.Data;

public partial class InformationFinder_Details_AccountInvoicesExport : System.Web.UI.Page
{

    // Create the database LINQ object(s).
    InsDataContext idc = new InsDataContext();

    // DataTable for results.
    public static DataTable ExcelResults;

    private string currCode;
    private decimal total = 0;
    private int accountID = 0;
    private int tmpInt = 0;
    private DateTime tmpDate;
    private DateTime tmpDate2;
    private string companyName;

    private List<InvoiceListItem> ItemList;

    [Serializable()]
    public class InvoiceList
    {
        public InvoiceList()
        {
        }

        /// <summary>
        /// Collection of Account Renewals list items.
        /// </summary>
        public List<InvoiceListItem> Invoices { get; set; }
    }

    [Serializable()]
    public class InvoiceListItem
    {
        public InvoiceListItem()
        {
        }

        /// <summary>
        /// Identifier of the Account.
        /// </summary>
        public int AccountID { get; set; }
        public string CompanyName { get; set; }

        // Create the columns for the DataTable.
        public string InvoiceNo { get; set; }
        public int OrderID { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public Decimal Amount { get; set; }
        public Decimal Payments { get; set; }
        public DateTime? PayDate { get; set; }
        public Decimal Credits { get; set; }
        public Decimal Balance { get; set; }
        public string CurrencyCode { get; set; }
    }

    #region PAGE LOAD.
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!int.TryParse(Request.QueryString["ID"], out accountID))
        {
            Page.Response.Redirect("../Default.aspx");
            return;
        }

        Account acct = new Account();
        acct = (from a in idc.Accounts where a.AccountID == accountID select a).FirstOrDefault();

        companyName = acct.CompanyName;

        if (Page.IsPostBack) return;

        txtInvoiceFrom.Text = "01/01/" + DateTime.Now.Year.ToString();
        txtInvoiceTo.Text = (DateTime.Now).ToShortDateString();

        // Bind the GridView.
        BindAccountInvoicesGridView();
    }
    #endregion

    #region GRIDVIEW FUNCTIONS.
    /// <summary>
    /// Query the data source and bind the data grid.
    /// </summary>
    private void BindAccountInvoicesGridView()
    {
        Int32.TryParse(Request.QueryString["ID"], out accountID);

        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.MASConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.StoredProcedure;

        // Using the sp_GetAccountInvoicesExport.
        string sqlQuery = "sp_GetAccountInvoicesExport";

        sqlCmd.Parameters.Add(new SqlParameter("@pAccountID", accountID));

        // Period From and To
        if (txtInvoiceFrom.Text != "" && txtInvoiceTo.Text != "")
        {
            tmpDate = DateTime.Now;
            tmpDate2 = DateTime.Now;

            // Ensure the date time stuff can be parsed.
            if (DateTime.TryParse(txtInvoiceFrom.Text, out tmpDate) && DateTime.TryParse(txtInvoiceTo.Text, out tmpDate2))
            {
                if (tmpDate <= tmpDate2)
                {
                    InvisibleError();
                    sqlCmd.Parameters.Add(new SqlParameter("@InvoiceDateFrom", tmpDate));
                    sqlCmd.Parameters.Add(new SqlParameter("@InvoiceDateTo", tmpDate2));
                }
                else
                {
                    VisibleError("From date is greater than To date.");
                    return;
                }
            }
            else
            {
                VisibleError("Please enter valid From and/or To date.");
                return;
            }
        }

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();

        sqlCmd.CommandText = sqlQuery;
        SqlDataReader reader = sqlCmd.ExecuteReader();

        decimal amount = 0;
        decimal payments = 0;
        decimal credits = 0;
        decimal balance = 0;

        string currencyCode = "";

        InvoiceList invoiceList = new InvoiceList();
        invoiceList.Invoices = new List<InvoiceListItem>();

        while (reader.Read())
        {
            // Get Currency Code (Example: USD).
            currencyCode = reader["CurrencyCode"].ToString();

            amount = FormatMoneyValues(reader["Amount"].ToString());
            payments = FormatMoneyValues(reader["Payments"].ToString());
            credits = FormatMoneyValues(reader["Credits"].ToString());
            balance = FormatMoneyValues(reader["Balance"].ToString());

            InvoiceListItem invoiceItem = new InvoiceListItem()
            {
                AccountID = int.Parse(reader["AccountID"].ToString()),
                CompanyName = companyName,
                InvoiceNo = reader["InvoiceNo"].ToString(),
                OrderID = int.Parse(reader["OrderID"].ToString()),

                Amount = amount,
                Payments = payments,

                Credits = credits,
                Balance = balance,
                CurrencyCode = currencyCode,
            };

            if (DateTime.TryParse(reader["InvoiceDate"].ToString(), out tmpDate))
                invoiceItem.InvoiceDate = tmpDate;

            if (DateTime.TryParse(reader["PayDate"].ToString(), out tmpDate))
                invoiceItem.PayDate = tmpDate;

            invoiceList.Invoices.Add(invoiceItem);
        }

        ItemList = invoiceList.Invoices;

        // Bind the results to the GridView.
        gvAccountInvoices.DataSource = ItemList;
        gvAccountInvoices.DataBind();

        sqlConn.Close();
        reader.Close();
    }

    // Example: string balance = 3.00 AUD, 3.11 USD. The function will return 3.00
    private decimal FormatMoneyValues(string balance)
    {
        try
        {
            string balance1 = balance.Split(',')[0];
            string balance2 = balance1.Split(' ')[0];
            return decimal.Parse(balance2);
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Fired when the page has been changed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvAccountInvoices_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvAccountInvoices.PageIndex = e.NewPageIndex;
        gvAccountInvoices.DataBind();
    }

    protected void gvAccountInvoices_Sorting(object sender, GridViewSortEventArgs e)
    {
        SortAccountInvoicesGridView();
    }

    /// <summary>
    /// Sort GridView based on which column was selected and by Ascending or Descending order.
    /// </summary>
    private void SortAccountInvoicesGridView()
    {
        BindAccountInvoicesGridView();

        var itms = (from b in ItemList
                    select b).OrderByDescending(b => b.InvoiceNo).ToList();

        // Sort the list based on the column
        switch (gvAccountInvoices.CurrentSortedColumn)
        {
            case "InvoiceNo":
                // Sort the list by InvoiceNo.
                itms.Sort(delegate(InvoiceListItem i1, InvoiceListItem i2) { return i1.InvoiceNo.CompareTo(i2.InvoiceNo); });
                break;

            case "OrderID":
                // Sort the list by OrderID.
                itms.Sort(delegate(InvoiceListItem i1, InvoiceListItem i2) { return i1.OrderID.CompareTo(i2.OrderID); });
                break;

            case "Amount":
                // Sort the list by Amount.
                itms.Sort(delegate(InvoiceListItem i1, InvoiceListItem i2) { return i1.Amount.CompareTo(i2.Amount); });
                break;

            case "Credits":
                // Sort the list by Credits.
                itms.Sort(delegate(InvoiceListItem i1, InvoiceListItem i2) { return i1.Credits.CompareTo(i2.Credits); });
                break;

            case "Balance":
                // Sort the list by Balance.
                itms.Sort(delegate(InvoiceListItem i1, InvoiceListItem i2) { return i1.Balance.CompareTo(i2.Balance); });
                break;

            case "InvoiceDate":
                // Sort the list by the InvoiceDate.
                itms.Sort(delegate(InvoiceListItem i1, InvoiceListItem i2)
                {
                    return (i1.InvoiceDate.HasValue) ? i1.InvoiceDate.Value.CompareTo((i2.InvoiceDate.HasValue) ? i2.InvoiceDate.Value : DateTime.MinValue) : 0;
                });
                break;

            case "PayDate":
                // Sort the list by the PayDate.
                itms.Sort(delegate(InvoiceListItem i1, InvoiceListItem i2)
                {
                    return (i1.PayDate.HasValue) ? i1.PayDate.Value.CompareTo((i2.PayDate.HasValue) ? i2.PayDate.Value : DateTime.MinValue) : 0;
                });
                break;
        }

        // Flip the list.
        if (gvAccountInvoices.CurrentSortDirection == SortDirection.Descending) itms.Reverse();

        // Display the results to the GridView.
        gvAccountInvoices.DataSource = itms;
        gvAccountInvoices.DataBind();
    }
    #endregion

    #region EXPORT TO EXCEL FUNCTION.
    /// <summary>
    /// Export GridView displayed data to an Excel file.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void lnkbtnExportToExcel_Click(object sender, EventArgs e)
    {
        // Bind the GridView, to fill the Invoice list for Excel export.
        BindAccountInvoicesGridView();

        // Create an data table used to export.
        DataTable exportTable = new DataTable();

        // Create columns for this DataTable.
        DataColumn colAccountID = new DataColumn("Account#");
        DataColumn colCompanyName = new DataColumn("Company Name");

        DataColumn colInvoiceNo = new DataColumn("Invoice#");
        DataColumn colOrderID = new DataColumn("Order#");
        DataColumn colInvoiceDate = new DataColumn("Invoice Date");
        DataColumn colAmount = new DataColumn("Amount");
        DataColumn colPayments = new DataColumn("Payments");

        DataColumn colPayDate = new DataColumn("Last Transaction");
        DataColumn colCredits = new DataColumn("Credits");
        DataColumn colBalance = new DataColumn("Balance");
        DataColumn colCurrencyCode = new DataColumn("Currency Code");

        // Define DataType of the columns.
        colAccountID.DataType = System.Type.GetType("System.String");
        colCompanyName.DataType = System.Type.GetType("System.String");

        colInvoiceNo.DataType = System.Type.GetType("System.String");
        colOrderID.DataType = System.Type.GetType("System.String");
        colInvoiceDate.DataType = System.Type.GetType("System.String");

        colAmount.DataType = System.Type.GetType("System.String");
        colPayments.DataType = System.Type.GetType("System.String");

        colPayDate.DataType = System.Type.GetType("System.String");
        colCredits.DataType = System.Type.GetType("System.String");
        colBalance.DataType = System.Type.GetType("System.String");
        colCurrencyCode.DataType = System.Type.GetType("System.String");

        // Add all of these columns into the exportTable.
        exportTable.Columns.Add(colAccountID);
        exportTable.Columns.Add(colCompanyName);

        exportTable.Columns.Add(colInvoiceNo);
        exportTable.Columns.Add(colOrderID);
        exportTable.Columns.Add(colInvoiceDate);
        exportTable.Columns.Add(colAmount);
        exportTable.Columns.Add(colPayments);

        exportTable.Columns.Add(colPayDate);
        exportTable.Columns.Add(colCredits);
        exportTable.Columns.Add(colBalance);
        exportTable.Columns.Add(colCurrencyCode);

        // Add the rows from the Account Renewals list.
        foreach (InvoiceListItem i in ItemList)
        {
            // Create a new table row.
            DataRow dr = exportTable.NewRow();
            dr[colAccountID] = i.AccountID;
            dr[colCompanyName] = companyName;

            dr[colInvoiceNo] = i.InvoiceNo;
            dr[colOrderID] = i.OrderID;
            dr[colInvoiceDate] = i.InvoiceDate.Value.ToShortDateString();
            dr[colAmount] = i.Amount;
            dr[colPayments] = i.Payments;

            if (i.PayDate != null)
                dr[colPayDate] = i.PayDate.Value.ToShortDateString();

            dr[colCredits] = i.Credits;
            dr[colBalance] = i.Balance;

            dr[colCurrencyCode] = i.CurrencyCode;

            // Add the row to the table.
            exportTable.Rows.Add(dr);
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
            throw ex;
        }

        try
        {
            // Create the export file based on the selected value.
            tableExport.Export("AccountInvoices", "XLS");

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
            throw ex;
        }

        Response.Flush();
        Response.End();
    }
    #endregion

    #region DATEPICKER TEXTCHANGED FUNCTIONS.
    protected void txtInvoiceFrom_TextChange(object sender, EventArgs e)
    {

        // Clear the style.
        txtInvoiceFrom.CssClass = "";

        // Exit if there is no text.
        if (txtInvoiceFrom.Text == "") return;

        // Test the see if the date is valid.
        DateTime fromDate = DateTime.Now;
        if (!DateTime.TryParse(txtInvoiceFrom.Text, out fromDate))
        {
            VisibleError("Please enter valid date.");
            return;
        }

        // Test to ensure the date is less than To Period.
        DateTime toDate = DateTime.Now;
        if (!DateTime.TryParse(txtInvoiceTo.Text, out toDate)) return;

        // Ensure the From date is LESS THAN the To date.
        if (fromDate > toDate)
        {
            VisibleError("From date is greater than To date.");
            return;
        }

        // Reset the page to 0.
        gvAccountInvoices.PageIndex = 0;

        // Rebind the Account Invoices GridView.
        BindAccountInvoicesGridView();
    }

    protected void txtInvoiceTo_TextChange(object sender, EventArgs e)
    {
        // Clear the style.
        txtInvoiceTo.CssClass = "";

        // Exit if there is no text.
        if (txtInvoiceTo.Text == "") return;

        // Test the see if the date is valid.
        DateTime toDate = DateTime.Now;
        if (!DateTime.TryParse(txtInvoiceTo.Text, out toDate))
        {
            VisibleError("Please enter valid date.");
            return;
        }

        DateTime fromDate = DateTime.Now;
        if (!DateTime.TryParse(txtInvoiceFrom.Text, out fromDate)) return;

        // Test to ensure the From date is LESS THAN To date.
        if (fromDate > toDate)
        {
            VisibleError("From date is greater than To date.");
            return;
        }

        // Reset the page to 0.
        gvAccountInvoices.PageIndex = 0;

        // Rebind the to the GridView.
        BindAccountInvoicesGridView();
    }
    #endregion

    #region CLEAR FILTERS FUNCTION.
    protected void lnkbtnClearFilters_Click(object sender, EventArgs e)
    {
        // Reset From and To dates.
        txtInvoiceFrom.Text = "01/01/" + DateTime.Now.Year.ToString();
        txtInvoiceTo.Text = (DateTime.Now).ToShortDateString();

        // Rebind the Account Invoices GridView.
        BindAccountInvoicesGridView();
    }
    #endregion

    #region RETURN TO ACCOUNT DETAILS PAGE.
    /// <summary>
    /// Return back to Information Finder - Account details.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnReturn_Click(object sender, EventArgs e)
    {
        Response.Redirect(string.Format("~/InformationFinder/Details/Account.aspx?ID={0}#Invoices_Tab", accountID));
    }
    #endregion

    #region ERROR MESSAGE FUNCTIONS.
    /// <summary>
    /// Reset Error Message.
    /// </summary>
    private void InvisibleError()
    {
        // Reset submission Form Error Message.
        this.spnFormErrorMessage.InnerText = string.Empty;
        this.divFormError.Visible = false;
    }

    /// <summary>
    /// Set Error Message.
    /// </summary>
    /// <param name="errorMsg"></param>
    private void VisibleError(string errorMsg)
    {
        this.spnFormErrorMessage.InnerText = errorMsg;
        this.divFormError.Visible = true;
    }
    #endregion

    public override void VerifyRenderingInServerForm(Control control)
    {
        return;
    }
}
