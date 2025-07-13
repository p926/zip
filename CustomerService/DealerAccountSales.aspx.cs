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
using System.Text;

using Instadose;
using Instadose.Data;

public partial class CustomerService_DealerAccountSales : System.Web.UI.Page
{
    // Create the DataBase references.
    InsDataContext idc = new InsDataContext();
    MASDataContext mas = new MASDataContext();

    string sqlConnStrInsta = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
    string sqlConnStrMAS = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.MASConnectionString"].ConnectionString;

    // String to hold the current Username.
    string _username = "Unknown";

    // DataTable results (Accounts Listing).
    public static DataTable ExcelExportedResults;
    private AccountsListingList list_al;
    private List<AccountsListingListItem> ItemList_AL;
    private InvoiceHistoryList list_ih;
    private List<InvoiceHistoryListItem> ItemList_IH;

    protected void Page_PreInit(object sender, EventArgs e)
    {
        Page.EnableEventValidation = false;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            this._username = User.Identity.Name.Split('\\')[1];

            if (!Page.IsPostBack)
            {
                hdnfldTabIndex.Value = "0";
                getDealersForDDL();
                bindAccountsListing();
                bindInvoiceHistory();
            }
        }
        catch { }
    }

    public override void VerifyRenderingInServerForm(Control control)
    {
        // Confirms that an HtmlForm control is rendered for the specified ASP.NET server control at runtime.
    }

    private void getDealersForDDL()
    {
        var dealers = (from d in idc.SalesRepDistributors
                       orderby d.SalesRepDistID ascending
                       select new
                       {
                         d.SalesRepDistID,
                         SalesRepCompanyName = d.SalesRepDistID + (d.CompanyName == null ? "" : " | " + d.CompanyName)
                       }).Distinct();

        // Commission Details (for Edit).
        this.ddlDealerID.DataSource = dealers;
        this.ddlDealerID.DataTextField = "SalesRepCompanyName";
        this.ddlDealerID.DataValueField = "SalesRepDistID";
        this.ddlDealerID.DataBind();
    }

    // Fired when the DropDownList's SelectedValue has been changed.
    protected void ddlDealerID_SelectedIndexChanged(object sender, EventArgs e)
    {
        bindAccountsListing();
        bindInvoiceHistory();
    }

    // ------------------------------------------------------------ ACCOUNTS LISTING FUNCTIONS ----------------------------------------------------------- //
    // Get all Accounting Listings for a given SalesRepDistID.
    private void bindAccountsListing()
    {
        // Create SQL Connection String.
        SqlConnection sqlConn = new SqlConnection(sqlConnStrInsta);

        // Create SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.StoredProcedure;

        sqlCmd.CommandText = "sp_GetAllAccountsBySalesRepDistID";

        if (ddlDealerID.SelectedValue != "")
        {
            string _salesrepdistid = ddlDealerID.SelectedValue;

            sqlCmd.Parameters.Add(new SqlParameter("@pSalesRepDistIDSearch", _salesrepdistid));
        }
        else
        {
            sqlCmd.Parameters.Add(new SqlParameter("@pSalesRepDistIDSearch", ""));
        }

        // Pass the Query String to the Command.
        sqlCmd.Connection = sqlConn;

        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();

        sqlCmd.CommandText = "sp_GetAllAccountsBySalesRepDistID";

        SqlDataReader dr = sqlCmd.ExecuteReader();

        AccountsListingList AccountsList = new AccountsListingList();
        AccountsList.AccountsListings = new List<AccountsListingListItem>();


        while (dr.Read())
        {
            AccountsListingListItem AccountsItem = new AccountsListingListItem()
            {
                SalesRepDistID = dr["SalesRepDistID"].ToString(),
                ReferralCode = dr["ReferralCode"].ToString(),
                SalesRepCompanyName = dr["SalesRepCompanyName"].ToString(),
                AccountID = int.Parse(dr["AccountID"].ToString()),
                CompanyName = dr["CompanyName"].ToString(),
                ContractStartDate = (DateTime.Parse(dr["ContractStartDate"].ToString()) == null ? DateTime.Now : DateTime.Parse(dr["ContractStartDate"].ToString())),
                ContractEndDate = (DateTime.Parse(dr["ContractEndDate"].ToString()) == null ? DateTime.Now.AddYears(1) : DateTime.Parse(dr["ContractEndDate"].ToString())),
                NumberOfActiveDevices = int.Parse(dr["NumberOfActiveDevices"].ToString()),
                Active = Boolean.Parse(dr["Active"].ToString())
            };
            AccountsList.AccountsListings.Add(AccountsItem);
        }

        ItemList_AL = AccountsList.AccountsListings;

        // Sort the List based on column.
        switch (gvAccountListing.CurrentSortedColumn)
        {
            case "SalesRepDistID":
                ItemList_AL.Sort(delegate(AccountsListingListItem i1, AccountsListingListItem i2) { return i1.SalesRepDistID.CompareTo(i2.SalesRepDistID); });
                break;
            case "ReferralCode":
                ItemList_AL.Sort(delegate(AccountsListingListItem i1, AccountsListingListItem i2) { return i1.ReferralCode.CompareTo(i2.ReferralCode); });
                break;
            case "SalesRepCompanyName":
                ItemList_AL.Sort(delegate(AccountsListingListItem i1, AccountsListingListItem i2) { return i1.SalesRepCompanyName.CompareTo(i2.SalesRepCompanyName); });
                break;
            case "AccountID":
                ItemList_AL.Sort(delegate(AccountsListingListItem i1, AccountsListingListItem i2) { return i1.AccountID.CompareTo(i2.AccountID); });
                break;
            case "CompanyName":
                ItemList_AL.Sort(delegate(AccountsListingListItem i1, AccountsListingListItem i2) { return i1.CompanyName.CompareTo(i2.CompanyName); });
                break;
            case "ContractStartDate":
                ItemList_AL.Sort(delegate(AccountsListingListItem i1, AccountsListingListItem i2) { return i1.ContractStartDate.CompareTo(i2.ContractStartDate); });
                break;
            case "ContractEndDate":
                ItemList_AL.Sort(delegate(AccountsListingListItem i1, AccountsListingListItem i2) { return i1.ContractEndDate.CompareTo(i2.ContractEndDate); });
                break;
            case "NumberOfActiveDevices":
                ItemList_AL.Sort(delegate(AccountsListingListItem i1, AccountsListingListItem i2) { return i1.NumberOfActiveDevices.CompareTo(i2.NumberOfActiveDevices); });
                break;
            case "Active":
                ItemList_AL.Sort(delegate(AccountsListingListItem i1, AccountsListingListItem i2) { return i1.Active.CompareTo(i2.Active); });
                break;
        }

        // Flip the GridView's listing order.
        if (gvAccountListing.CurrentSortDirection == SortDirection.Descending) ItemList_AL.Reverse();

        // Bind the results to the GridView.
        gvAccountListing.DataSource = AccountsList.AccountsListings;
        gvAccountListing.DataBind();
    }

    // Fired when the Account Listings GridView's page has been changed.
    protected void gvAccountListing_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvAccountListing.PageIndex = e.NewPageIndex;
        bindAccountsListing();
    }

    // Fired when the Account Listing GridView's column title has been clicked (for Sorting).
    protected void gvAccountListing_Sorting(object sender, GridViewSortEventArgs e)
    {
        gvAccountListing.PageIndex = 0;
        bindAccountsListing();
    }

    protected void btnExportToExcel_AccountListing_Click(object sender, EventArgs e)
    {
        // Bind the GridView to fill the Accounts Listing Excel Spreadsheet.
        bindAccountsListing();

        // Create a DataTable used for Exporting.
        DataTable dtAccountsListing = new DataTable();

        dtAccountsListing.Columns.Add("Dealer #", typeof(string));
        dtAccountsListing.Columns.Add("Dealer Company", typeof(string));
        dtAccountsListing.Columns.Add("Account #", typeof(int));
        dtAccountsListing.Columns.Add("Company Name", typeof(string));
        dtAccountsListing.Columns.Add("Contract Start", typeof(string));
        dtAccountsListing.Columns.Add("Contract End", typeof(string));
        dtAccountsListing.Columns.Add("Active Badges", typeof(int));
        dtAccountsListing.Columns.Add("Active", typeof(string));

        // Are any Accounts found? If NOT then return;.
        foreach (AccountsListingListItem al in ItemList_AL)
        {
            DataRow dr = dtAccountsListing.NewRow();
            dr[0] = al.SalesRepDistID;
            dr[1] = al.SalesRepCompanyName;
            dr[2] = al.AccountID;
            dr[3] = al.CompanyName;
            dr[4] = String.Format("{0:MM/dd/yyyy}", al.ContractStartDate);
            dr[5] = String.Format("{0:MM/dd/yyyy}", al.ContractEndDate);
            dr[6] = al.NumberOfActiveDevices;
            dr[7] = (al.Active) ? "Yes" : "No";

            // Add each Row to the DataTable.
            dtAccountsListing.Rows.Add(dr);
        }

        string title = "Complete Account List";

        // Create the Exported File(name) based on the SelectedValue of the DDL.
        if (ddlDealerID.SelectedValue != "")
        {
            title = string.Format("Account List for {0}", ddlDealerID.SelectedValue);
        }

        // Build the Export DataTable.
        TableExport teAccountsListing = new TableExport(dtAccountsListing);
        teAccountsListing.Stylesheet = File.ReadAllText(Server.MapPath("~/css/export/grids.css"));
        teAccountsListing.Header = title;

        // Perform the export.
        teAccountsListing.Export(title, "XLS");    
  
        // Clear everything.
        Response.Clear();
        Response.ClearHeaders();

        // Set the Response Headers.
        Response.ContentType = teAccountsListing.File.ContentType;
        Response.AddHeader("content-disposition", teAccountsListing.File.ContentDisposition);

        // Write to Excel file.
        if (teAccountsListing.File.Content.GetType() == typeof(byte[]))
        {
            Response.BinaryWrite((byte[])teAccountsListing.File.Content);
        }
        else
        {
            Response.Write(teAccountsListing.File.Content.ToString());
        }

        Response.Flush();
        Response.End();
    }
    // ------------------------------------------------------------ END ----------------------------------------------------------- //

    // ------------------------------------------------------------ INVOICE HISTORY FUNCTIONS ----------------------------------------------------------- //

    // Get entire Invoice History for a given SalesRepDistID.
    private void bindInvoiceHistory()
    {
        // Create SQL Connection String.
        SqlConnection sqlConn = new SqlConnection(sqlConnStrMAS);

        // Create SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.StoredProcedure;

        sqlCmd.CommandText = "sp_GetAllInvoicesBySalesPersonNo";

        if (ddlDealerID.SelectedValue != "")
        {
            string _salesrepdistid = ddlDealerID.SelectedValue;

            sqlCmd.Parameters.Add(new SqlParameter("@pSalesPersonNoSearch", _salesrepdistid));
        }
        else
        {
            sqlCmd.Parameters.Add(new SqlParameter("@pSalesPersonNoSearch", ""));
        }

        // Pass the Query String to the Command.
        sqlCmd.Connection = sqlConn;

        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();

        SqlDataReader dr = sqlCmd.ExecuteReader();

        InvoiceHistoryList InvoiceHistory = new InvoiceHistoryList();
        InvoiceHistory.InvoiceHistories = new List<InvoiceHistoryListItem>();


        while (dr.Read())
        {
            InvoiceHistoryListItem InvoiceItem = new InvoiceHistoryListItem()
            {
                SalesPersonNo = dr["SalesPersonNo"].ToString(),
                SalesPersonName = dr["SalesPersonName"].ToString(),
                CustomerNo = dr["CustomerNo"].ToString(),
                InvoiceNo = dr["InvoiceNo"].ToString(),
                BillToName = dr["BillToName"].ToString(),
                InvoiceDate = dr["InvoiceDate"].ToString(),
                InvoiceAmount = dr["InvoiceAmount"].ToString()
            };
            InvoiceHistory.InvoiceHistories.Add(InvoiceItem);
        }

        ItemList_IH = InvoiceHistory.InvoiceHistories;

        // Sort the List based on column.
        switch (gvInvoiceHistory.CurrentSortedColumn)
        {
            case "SalesPersonNo":
                ItemList_IH.Sort(delegate(InvoiceHistoryListItem i1, InvoiceHistoryListItem i2) { return i1.SalesPersonNo.CompareTo(i2.SalesPersonNo); });
                break;
            case "SalesPersonName":
                ItemList_IH.Sort(delegate(InvoiceHistoryListItem i1, InvoiceHistoryListItem i2) { return i1.SalesPersonName.CompareTo(i2.SalesPersonName); });
                break;
            case "CustomerNo":
                ItemList_IH.Sort(delegate(InvoiceHistoryListItem i1, InvoiceHistoryListItem i2) { return i1.CustomerNo.CompareTo(i2.CustomerNo); });
                break;
            case "InvoiceNo":
                ItemList_IH.Sort(delegate(InvoiceHistoryListItem i1, InvoiceHistoryListItem i2) { return i1.InvoiceNo.CompareTo(i2.InvoiceNo); });
                break;
            case "BillToName":
                ItemList_IH.Sort(delegate(InvoiceHistoryListItem i1, InvoiceHistoryListItem i2) { return i1.BillToName.CompareTo(i2.BillToName); });
                break;
            case "InvoiceDate":
                ItemList_IH.Sort(delegate(InvoiceHistoryListItem i1, InvoiceHistoryListItem i2) { return i1.InvoiceDate.CompareTo(i2.InvoiceDate); });
                break;
            case "InvoiceAmount":
                ItemList_IH.Sort(delegate(InvoiceHistoryListItem i1, InvoiceHistoryListItem i2) { return i1.InvoiceAmount.CompareTo(i2.InvoiceAmount); });
                break;
        }

        // Flip the GridView's listing order.
        if (gvInvoiceHistory.CurrentSortDirection == SortDirection.Descending) ItemList_IH.Reverse();

        // Bind the results to the GridView.
        gvInvoiceHistory.DataSource = InvoiceHistory.InvoiceHistories;
        gvInvoiceHistory.DataBind();
    }

    // Fired when the Invoice History GridView's page has been changed.
    protected void gvInvoiceHistory_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvInvoiceHistory.PageIndex = e.NewPageIndex;
        bindInvoiceHistory();
    }

    // Fired when the Invoice History GridView's column title has been clicked (for Sorting).
    protected void gvInvoiceHistory_Sorting(object sender, GridViewSortEventArgs e)
    {
        gvInvoiceHistory.PageIndex = 0;
        bindInvoiceHistory();
    }

    protected void btnExportToExcel_InvoiceHistory_Click(object sender, EventArgs e)
    {
        // Bind the GridView to fill the Invoice History Excel Spreadsheet.
        bindInvoiceHistory();

        // Create a DataTable used for Exporting.
        DataTable dtInvoiceHistory = new DataTable();

        dtInvoiceHistory.Columns.Add("Dealer #", typeof(string));
        dtInvoiceHistory.Columns.Add("Dealer Company", typeof(string));
        dtInvoiceHistory.Columns.Add("Customer #", typeof(string));
        dtInvoiceHistory.Columns.Add("Invoice #", typeof(string));
        dtInvoiceHistory.Columns.Add("Bill-To Name", typeof(string));
        dtInvoiceHistory.Columns.Add("Invoice Date", typeof(string));
        dtInvoiceHistory.Columns.Add("Invoice Amount", typeof(string));


        foreach (InvoiceHistoryListItem ih in ItemList_IH)
        {
            DataRow dr = dtInvoiceHistory.NewRow();
            dr[0] = ih.SalesPersonNo;
            dr[1] = ih.SalesPersonName;
            dr[2] = ih.CustomerNo;
            dr[3] = ih.InvoiceNo;
            dr[4] = ih.BillToName;
            dr[5] = String.Format("{0:MM/dd/yyyy}", ih.InvoiceDate);
            dr[6] = ih.InvoiceAmount;

            // Add each Row to the DataTable.
            dtInvoiceHistory.Rows.Add(dr);
        }

        string title = "Complete Invoice History";

        if (ddlDealerID.SelectedValue != "")
        {
            title = String.Format("Invoice History for {0}", ddlDealerID.SelectedValue);
        }

        // Build the Export DataTable.
        TableExport teInvoiceHistory = new TableExport(dtInvoiceHistory);
        teInvoiceHistory.Stylesheet = File.ReadAllText(Server.MapPath("~/css/export/grids.css"));
        teInvoiceHistory.Header = title;

        // Create the Exported File(name) based on the SelectedValue of the DDL.
        teInvoiceHistory.Export(title, "XLS");
            
        // Clear everything.
        Response.Clear();
        Response.ClearHeaders();

        // Set the Response Headers.
        Response.ContentType = teInvoiceHistory.File.ContentType;
        Response.AddHeader("content-disposition", teInvoiceHistory.File.ContentDisposition);

        // Write to Excel file.
        if (teInvoiceHistory.File.Content.GetType() == typeof(byte[]))
        {
            Response.BinaryWrite((byte[])teInvoiceHistory.File.Content);
        }
        else
        {
            Response.Write(teInvoiceHistory.File.Content.ToString());
        }
        Response.Flush();
        Response.End();
    }
    // ------------------------------------------------------------ END ----------------------------------------------------------- //
}

namespace com.instadose.portal.dealers
{

    //**********************************************************************************
    //  Created By: Anuradha Nandi
    //  Create Date: November 6, 2012
    //  New List Class for Accounts Listing
    //  Used In: CustomerService/DealerAccountSales.aspx.cs
    //**********************************************************************************
    public class AccountsListingList
    {
        public AccountsListingList()
        {
        }

        // Collection of Accounts Listing list items.
        public List<AccountsListingListItem> AccountsListings { get; set; }
    }

    [Serializable()]
    public class AccountsListingListItem
    {
        public AccountsListingListItem()
        {
        }
        // Identifier of the Account Listing.
        public string SalesRepDistID { get; set; }
        public string ReferralCode { get; set; }
        public string SalesRepCompanyName { get; set; }
        public int AccountID { get; set; }
        public string CompanyName { get; set; }
        public DateTime ContractStartDate { get; set; }
        public DateTime ContractEndDate { get; set; }
        public int NumberOfActiveDevices { get; set; }
        public bool Active { get; set; }
    }
    //**********************************************************************************
    //  Created By: Anuradha Nandi
    //  Create Date: November 7, 2012
    //  New List Class for Invoice History
    //  Used In: CustomerService/DealerAccountSales.aspx.cs
    //**********************************************************************************
    public class InvoiceHistoryList
    {
        public InvoiceHistoryList()
        {
        }

        // Collection of Accounts Listing list items.
        public List<InvoiceHistoryListItem> InvoiceHistories { get; set; }
    }

    [Serializable()]
    public class InvoiceHistoryListItem
    {
        public InvoiceHistoryListItem()
        {
        }

        // Identifier of the Account Listing.
        public string SalesPersonNo { get; set; }
        public string SalesPersonName { get; set; }
        public string CustomerNo { get; set; }
        public string InvoiceNo { get; set; }
        public string BillToName { get; set; }
        public string InvoiceDate { get; set; }
        public string InvoiceAmount { get; set; }
    }

}