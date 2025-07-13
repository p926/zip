using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Instadose.Data;

public partial class InformationFinder_Details_AccountReadsExport : System.Web.UI.Page
{

    // Create the database LINQ objects.
    InsDataContext idc = new InsDataContext();

    // DataTable results.
    public static DataTable ExcelResults;

    private int accountID = 0;
    private DateTime tmpDate;
    private DateTime tmpDate2;

    private string companyName;

    private List<ReadListItem> ItemList;

    [Serializable()]
    public class ReadList
    {
        public ReadList()
        {
        }

        /// <summary>
        /// Collection of Account Reads ListItem.
        /// </summary>
        public List<ReadListItem> Reads { get; set; }

    }

    [Serializable()]
    public class ReadListItem
    {
        public ReadListItem()
        {
        }

        /// <summary>
        /// Identifier of the Account.
        /// </summary>
        public int AccountID { get; set; }
        public string CompanyName { get; set; }

        public DateTime? ExposureDate { get; set; }
        public string TimeZoneDesc { get; set; }
        public DateTime? CompleteExposureDate { get; set; }

        public string UserName { get; set; }
        public string FullName { get; set; }
        public string SerialNumber { get; set; }

        public string BodyRegion { get; set; }
        public string Deep { get; set; }
        public string Shallow { get; set; }
        public string Eye { get; set; }
        public string UOMLocation { get; set; }
        public string Anomaly { get; set; }
    }

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

        // Prevent the page from redrawing the table if not a first time load.
        if (!IsPostBack)
        {
            bindBodyRegionCombo();

            txtExposureFrom.Text = "01/01/" + DateTime.Now.Year.ToString();
            txtExposureTo.Text = DateTime.Now.ToShortDateString();

            // Bind to the GridView.
            bindAccountReadsGrid();
        }
    }

    /// <summary>
    /// Fired when the page has been changed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvAccountReads_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvAccountReads.PageIndex = e.NewPageIndex;
        bindAccountReadsGrid();
    }

    /// <summary>
    /// Called from many objects, force binds the gridview.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gridviewBinder(object sender, EventArgs e)
    {
        // Reset the page to 0
        gvAccountReads.PageIndex = 0;

        // Bind the gridview
        bindAccountReadsGrid();
    }

    protected void txtExposureFrom_TextChange(object sender, EventArgs e)
    {

        // Clear the style.
        txtExposureFrom.CssClass = "";

        // Exit if there is no text.
        if (txtExposureFrom.Text == "") return;

        // Test the see if the date is valid
        DateTime fromDate = DateTime.Now;
        if (!DateTime.TryParse(txtExposureFrom.Text, out fromDate))
        {
            VisibleError("Please enter valid date.");
            return;
        }

        // Test to ensure the date is less than To Period
        DateTime toDate = DateTime.Now;
        if (!DateTime.TryParse(txtExposureTo.Text, out toDate)) return;

        // Ensure the from date is less than to date
        if (fromDate > toDate)
        {
            VisibleError("From date is greater than To date.");
            return;
        }

        // Reset the page to 0
        gvAccountReads.PageIndex = 0;

        // Rebind the reads grid.
        bindAccountReadsGrid();
    }

    protected void txtExposureTo_TextChange(object sender, EventArgs e)
    {
        // Clear the style.
        txtExposureTo.CssClass = "";

        // Exit if there is no text.
        if (txtExposureTo.Text == "") return;

        // Test the see if the date is valid
        DateTime toDate = DateTime.Now;
        if (!DateTime.TryParse(txtExposureTo.Text, out toDate))
        {
            VisibleError("Please enter valid date.");
            return;
        }

        // Test to ensure the date is less than From Period
        DateTime fromDate = DateTime.Now;
        if (!DateTime.TryParse(txtExposureFrom.Text, out fromDate)) return;

        // Ensure the from date is less than to date
        if (fromDate > toDate)
        {
            VisibleError("From date is greater than To date.");
            return;
        }

        // Reset the page to 0
        gvAccountReads.PageIndex = 0;

        // Rebind the reads grid.
        bindAccountReadsGrid();
    }

    /// <summary>
    /// Bind to the Body Region DropDownList.
    /// </summary>
    private void bindBodyRegionCombo()
    {
        ddlBodyRegion.DataSource = (from b in idc.BodyRegions
                                    orderby b.BodyRegionName
                                    select new
                                    {
                                        BodyRegionID = b.BodyRegionID,
                                        BodyRegion = b.BodyRegionName
                                    });

        ddlBodyRegion.DataTextField = "BodyRegion";
        ddlBodyRegion.DataValueField = "BodyRegionID";

        ddlBodyRegion.DataBind();

        ddlBodyRegion.Items.Insert(0, new ListItem("All", ""));
    }

    /// <summary>
    /// Query the data source and bind the GridView.
    /// </summary>
    private void bindAccountReadsGrid()
    {
        try
        {
            // Create the sql connection from the connection string in the web.config
            SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
                ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

            // Create the SQL Command.
            SqlCommand sqlCmd = new SqlCommand();
            sqlCmd.CommandType = CommandType.StoredProcedure;

            // Switch back to revised view since cannot add sort criteria without re-writing
            // stored proc (stored will have to executed as a large text string.  Too cumbersome!).
            sqlCmd.CommandText = "sp_GetAccountReadsExport";

            sqlCmd.Parameters.Add(new SqlParameter("@AccountID", accountID));

            // Set the filter parameter for the Body Region.
            if (!ddlBodyRegion.SelectedItem.Text.Contains("All"))
            {
                sqlCmd.Parameters.Add(new SqlParameter("@BodyRegion", ddlBodyRegion.SelectedItem.Text));
            }

            // Period From and To.
            if (txtExposureFrom.Text != "" && txtExposureTo.Text != "")
            {
                tmpDate = DateTime.Now;
                tmpDate2 = DateTime.Now;

                if (DateTime.TryParse(txtExposureFrom.Text, out tmpDate) && DateTime.TryParse(txtExposureTo.Text, out tmpDate2))
                {
                    if (tmpDate <= tmpDate2)
                    {
                        sqlCmd.Parameters.Add(new SqlParameter("@ExposureDateFrom", tmpDate));
                        sqlCmd.Parameters.Add(new SqlParameter("@ExposureDateTo", tmpDate2));
                    }
                    else
                    {
                        // From date must be BEFORE the to date.
                        VisibleError("From date is greater than To date.");
                        return;
                    }
                }
                else
                {
                    // Dates are not in the correct format.
                    VisibleError("Please enter valid date.");
                    return;
                }
            }

            // Pass the query string to the SQL Command.
            sqlCmd.Connection = sqlConn;

            bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
            if (!openConn) sqlCmd.Connection.Open();

            SqlDataReader reader = sqlCmd.ExecuteReader();

            ReadList readList = new ReadList();
            readList.Reads = new List<ReadListItem>();

            bool anomaly = false;

            while (reader.Read())
            {
                anomaly = bool.Parse(reader["Anomaly"].ToString());

                ReadListItem readItem = new ReadListItem();

                readItem = new ReadListItem()
                {
                    AccountID = int.Parse(reader["AccountID"].ToString()),
                    CompanyName = companyName,
                    TimeZoneDesc = reader["TimeZoneDesc"].ToString(),
                    UserName = reader["UserName"].ToString(),
                    FullName = reader["FullName"].ToString(),
                    SerialNumber = reader["SerialNumber"].ToString(),
                    BodyRegion = reader["BodyRegion"].ToString(),
                    UOMLocation = reader["UOMLocation"].ToString(),
                };

                if (reader["Deep"].ToString() != "")
                    readItem.Deep = Convert.ToDecimal(reader["Deep"].ToString()).ToString("N0");
                else
                    readItem.Deep = "";

                if (reader["Shallow"].ToString() != "")
                    readItem.Shallow = Convert.ToDecimal(reader["Shallow"].ToString()).ToString("N0");
                else
                    readItem.Shallow = "";

                if (reader["Eye"].ToString() != "")
                    readItem.Eye = Convert.ToDecimal(reader["Eye"].ToString()).ToString("N0");
                else
                    readItem.Eye = "";

                if (DateTime.TryParse(reader["CompleteExposureDate"].ToString(), out tmpDate))
                    readItem.CompleteExposureDate = tmpDate;

                readList.Reads.Add(readItem);
            }

            ItemList = readList.Reads;

            // Bind the results to the GridView.
            gvAccountReads.DataSource = ItemList;
            gvAccountReads.DataBind();
        }
        catch
        {
            bindBodyRegionCombo();
            ddlBodyRegion.SelectedIndex = 0;
        }
    }

    /// <summary>
    /// Sort event for reads grid.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvAccountReads_Sorting(object sender, GridViewSortEventArgs e)
    {
        sortAccountBadgesGrid();
    }

    /// <summary>
    /// sort grid based on which column was selected and by ascending or descending.
    /// </summary>
    private void sortAccountBadgesGrid()
    {
        // Display all items created by loginUser
        bindAccountReadsGrid();

        var itms = (from r in ItemList
                    select r).OrderByDescending(r => r.CompleteExposureDate).ToList();

        // Sort the list based on the column
        switch (gvAccountReads.CurrentSortedColumn)
        {
            case "UserName":
                // Sort the list by id
                itms.Sort(delegate(ReadListItem i1, ReadListItem i2) { return i1.UserName.CompareTo(i2.UserName); });
                break;

            case "FullName":
                // Sort the list by id
                itms.Sort(delegate(ReadListItem i1, ReadListItem i2) { return i1.FullName.CompareTo(i2.FullName); });
                break;

            case "TimeZoneDesc":
                // Sort the list by id
                itms.Sort(delegate(ReadListItem i1, ReadListItem i2) { return i1.TimeZoneDesc.CompareTo(i2.TimeZoneDesc); });
                break;

            case "SerialNumber":
                // Sort the list by id
                itms.Sort(delegate(ReadListItem i1, ReadListItem i2) { return i1.SerialNumber.CompareTo(i2.SerialNumber); });
                break;

            case "BodyRegion":
                // Sort the list by id
                itms.Sort(delegate(ReadListItem i1, ReadListItem i2) { return i1.BodyRegion.CompareTo(i2.BodyRegion); });
                break;

            case "Deep":
                // Sort the list by id
                itms.Sort(delegate(ReadListItem i1, ReadListItem i2) { return i1.Deep.CompareTo(i2.Deep); });
                break;

            case "Shallow":
                // Sort the list by id
                itms.Sort(delegate(ReadListItem i1, ReadListItem i2) { return i1.Shallow.CompareTo(i2.Shallow); });
                break;

            case "Eye":
                // Sort the list by id
                itms.Sort(delegate(ReadListItem i1, ReadListItem i2) { return i1.Eye.CompareTo(i2.Eye); });
                break;

            case "UOMLocation":
                // Sort the list by id
                itms.Sort(delegate(ReadListItem i1, ReadListItem i2) { return i1.UOMLocation.CompareTo(i2.UOMLocation); });
                break;

            case "CompleteExposureDate":
                // Sort the list by the last read date
                itms.Sort(delegate(ReadListItem i1, ReadListItem i2)
                {
                    return (i1.CompleteExposureDate.HasValue) ? i1.CompleteExposureDate.Value.CompareTo((i2.CompleteExposureDate.HasValue) ? i2.CompleteExposureDate.Value : DateTime.MinValue) : 0;
                });
                break;
        }

        // Flip the list
        if (gvAccountReads.CurrentSortDirection == SortDirection.Descending) itms.Reverse();

        // Display the results to the gridview.
        gvAccountReads.DataSource = itms;
        gvAccountReads.DataBind();
    }


    /// <summary>
    /// Export grid data to Excel.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void lnkbtnExport_Click(object sender, EventArgs e)
    {
        // Bind the grid, to fill the Renewals list for Excel export.
        bindAccountReadsGrid();

        //// Create an data table used to export
        DataTable exportTable = new DataTable();

        //Create columns for this DataTable
        DataColumn colAccountID = new DataColumn("Account#");
        DataColumn colCompanyName = new DataColumn("Company Name");

        DataColumn colTimeZoneDesc = new DataColumn("Time Zone");
        DataColumn colCompleteExposureDate = new DataColumn("Exposure Date");
        DataColumn colUserName = new DataColumn("Name");
        DataColumn colFullName = new DataColumn("Full Name");
        DataColumn colSerialNumber = new DataColumn("Serial Number");

        DataColumn colBodyRegion = new DataColumn("Body Region");

        DataColumn colDeep = new DataColumn("Deep");
        DataColumn colShallow = new DataColumn("Shallow");
        DataColumn colEye = new DataColumn("Eye");

        DataColumn colUOMLocation = new DataColumn("UOM");

        //Define DataType of the Columns
        colAccountID.DataType = System.Type.GetType("System.String");
        colCompanyName.DataType = System.Type.GetType("System.String");

        colCompleteExposureDate.DataType = System.Type.GetType("System.String");

        colUserName.DataType = System.Type.GetType("System.String");
        colFullName.DataType = System.Type.GetType("System.String");
        colSerialNumber.DataType = System.Type.GetType("System.String");
        colBodyRegion.DataType = System.Type.GetType("System.String");

        colDeep.DataType = System.Type.GetType("System.String");
        colShallow.DataType = System.Type.GetType("System.String");
        colEye.DataType = System.Type.GetType("System.String");
        colUOMLocation.DataType = System.Type.GetType("System.String");

        //Add All These Columns into exportTable
        exportTable.Columns.Add(colAccountID);
        exportTable.Columns.Add(colCompanyName);

        exportTable.Columns.Add(colCompleteExposureDate);
        exportTable.Columns.Add(colTimeZoneDesc);
        exportTable.Columns.Add(colUserName);
        exportTable.Columns.Add(colFullName);

        exportTable.Columns.Add(colSerialNumber);
        exportTable.Columns.Add(colBodyRegion);
        exportTable.Columns.Add(colDeep);
        exportTable.Columns.Add(colShallow);
        exportTable.Columns.Add(colEye);

        exportTable.Columns.Add(colUOMLocation);

        //// Add the rows from the account renewals list
        foreach (ReadListItem rl in ItemList)
        {
            // Create a new table row
            DataRow dr = exportTable.NewRow();
            dr[colAccountID] = rl.AccountID;
            dr[colCompanyName] = companyName;

            dr[colCompleteExposureDate] = rl.CompleteExposureDate;
            dr[colTimeZoneDesc] = rl.TimeZoneDesc;

            dr[colUserName] = rl.UserName;
            dr[colFullName] = rl.FullName;

            dr[colSerialNumber] = rl.SerialNumber;

            dr[colBodyRegion] = rl.BodyRegion;
            dr[colDeep] = rl.Deep;
            dr[colShallow] = rl.Shallow;
            dr[colEye] = rl.Eye;
            dr[colUOMLocation] = rl.UOMLocation;

            // Add the row to the table
            exportTable.Rows.Add(dr);
        }

        // Build the export table
        TableExport tableExport = new TableExport(exportTable);

        try
        {
            // Read the CSS template from file
            tableExport.Stylesheet =
                System.IO.File.ReadAllText(Server.MapPath("~/css/export/grids.css"));
        }
        catch { }

        try
        {
            //// Create the export file based on the selected value
            tableExport.Export("AccountReads", "XLS");

            ExportFile file = tableExport.File;

            // Clear everything out
            Response.Clear();
            Response.ClearHeaders();

            // Set the response headers
            Response.ContentType = file.ContentType;
            Response.AddHeader("Content-Disposition", file.ContentDisposition);

            //Write to Excel file.
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

    public override void VerifyRenderingInServerForm(Control control)
    {
        return;
    }

    /// <summary>
    /// Clear all filters applied to GridView.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void lnkbtnClearFilters_Click(object sender, EventArgs e)
    {
        bindBodyRegionCombo();
        ddlBodyRegion.SelectedIndex = 0;
        txtExposureFrom.Text = "01/01/" + DateTime.Now.Year.ToString();
        txtExposureTo.Text = DateTime.Now.ToShortDateString();
        bindAccountReadsGrid();
    }

    /// <summary>
    /// Return back to Information Finder - Account Details page.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnReturn_Click(object sender, EventArgs e)
    {
        Response.Redirect(string.Format("~/InformationFinder/Details/Account.aspx?ID={0}#Reads_Tab", accountID));

    }

    #region ERROR MESSAGE.
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
}
