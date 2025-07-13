using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Instadose.Data;

public partial class InformationFinder_Details_AccountBadgesExport : System.Web.UI.Page
{
    // Create the database LINQ objects.
    InsDataContext idc = new InsDataContext();

    public static DataTable ExcelResults;

    private string currCode;
    private int accountID = 0;
    private int tmpInt = 0;
    private DateTime tmpDate;
    private string companyName;

    private List<BadgeListItem> ItemList;

    [Serializable()]
    public class BadgeList
    {
        public BadgeList()
        {
        }

        /// <summary>
        /// Collection of Account Renewals list items.
        /// </summary>
        public List<BadgeListItem> Badges { get; set; }
    }

    [Serializable()]
    public class BadgeListItem
    {
        public BadgeListItem()
        {
        }

        /// <summary>
        /// Identifier of the Account.
        /// </summary>
        public int AccountID { get; set; }
        public string CompanyName { get; set; }

        public string SerialNumber { get; set; }
        public int UserID { get; set; }

        public string FullName { get; set; }
        public string BodyRegion { get; set; }
        public string ProductColor { get; set; }
        public int OrderID { get; set; }
        public string Location { get; set; }
        public DateTime? ServiceStart { get; set; }
        public DateTime? ServiceEnd { get; set; }

        public string FormattedInitialized { get; set; }
        public string Active { get; set; }
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

        if (Page.IsPostBack) return;

        BindBodyRegionCombo();
        BindProductColorCombo();

        // Bind to GridView.
        BindAccountBadgesGrid();
    }

    /// <summary>
    /// Fired when the page has been changed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvAccountBadges_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvAccountBadges.PageIndex = e.NewPageIndex;
        BindAccountBadgesGrid();
    }

    /// <summary>
    /// Hides the review and completed panels and displays the renewals table.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnBack_Click(object sender, EventArgs e)
    {
        BindAccountBadgesGrid();
    }

    /// <summary>
    /// Called from many objects, force binds the gridview.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void GridViewBinder(object sender, EventArgs e)
    {
        // Reset the page to 0
        gvAccountBadges.PageIndex = 0;

        // Bind the gridview
        BindAccountBadgesGrid();
    }

    private void BindBodyRegionCombo()
    {
        ddlBodyRegion.DataSource = (from b in idc.BodyRegions
                                    orderby b.BodyRegionName
                                    //select new { a.DealerID, DealerIDName = a.DealerID.ToString() + " - " + a.DealerName.ToString() });
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

    private void BindProductColorCombo()
    {
        ddlProductColor.DataSource = (from p in idc.Products
                                      orderby p.Color
                                      select new
                                      {
                                          ColorID = p.Color,
                                          Color = p.Color
                                      }).Distinct().ToList();

        ddlProductColor.DataTextField = "Color";
        ddlProductColor.DataValueField = "ColorID";

        ddlProductColor.DataBind();
        ddlProductColor.Items.Insert(0, new ListItem("All", ""));
    }

    /// <summary>
    /// Query the data source and bind the data grid.
    /// </summary>
    private void BindAccountBadgesGrid()
    {
        try
        {
            // Create the sql connection from the connection string in the web.config.
            SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
                ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

            // Create the SQL command.
            SqlCommand sqlCmd = new SqlCommand();
            sqlCmd.CommandType = CommandType.Text;

            string query = "SELECT * FROM [vw_GetAccountBadgesExport]";
            string filters = "";

            filters += "([AccountID] = @AccountID)";

            sqlCmd.Parameters.Add(new SqlParameter("@AccountID", accountID));

            // Set the filter parameter for billing method
            if (!ddlBodyRegion.SelectedItem.Text.Contains("All"))
            {
                // Append the AND if needed.
                filters += " AND ";

                filters += "([BodyRegion] = @BodyRegion)";
                sqlCmd.Parameters.Add(new SqlParameter("BodyRegion", ddlBodyRegion.SelectedItem.Text));
            }

            // Set the filter parameter for billing term
            if (!ddlProductColor.SelectedItem.Text.Contains("All"))
            {
                // Append the AND if needed.
                if (filters != "") filters += " AND ";

                filters += "([ProductColor] = @ProductColor)";
                sqlCmd.Parameters.Add(new SqlParameter("ProductColor", ddlProductColor.SelectedItem.Text));
            }

            // Set the filter parameter for the brand
            if (!ddlInitailized.SelectedItem.Text.Contains("All"))
            {
                // Append the AND if needed.
                if (filters != "") filters += " AND ";

                if (ddlInitailized.SelectedItem.Text.Contains("Yes"))
                    filters += "([Initialized] IS NOT NULL) ";
                else
                    filters += "([Initialized] IS NULL) ";
            }

            // Set the filter parameter for the brand
            if (!ddlActive.SelectedItem.Text.Contains("All"))
            {
                // Append the AND if needed.
                if (filters != "") filters += " AND ";

                if (ddlActive.SelectedItem.Text.Contains("Yes"))
                    filters += "([Active] = 1) ";
                else
                    filters += "([Active] = 0) ";
            }

            // Add the filters to the query if needed.
            if (filters != "") query += " WHERE " + filters;

            // Add the order by and the direction
            query += " ORDER BY " + gvAccountBadges.CurrentSortedColumn + ((gvAccountBadges.CurrentSortDirection == SortDirection.Ascending) ? " ASC" : " DESC");

            // Pass the query string to the command

            sqlCmd.Connection = sqlConn;

            bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
            if (!openConn) sqlCmd.Connection.Open();

            sqlCmd.CommandText = query;

            SqlDataReader reader = sqlCmd.ExecuteReader();

            BadgeList badgeList = new BadgeList();
            badgeList.Badges = new List<BadgeListItem>();

            string isInitialized;
            bool isActive = true;

            while (reader.Read())
            {
                isInitialized = reader["Initialized"].ToString() != "" ? "Yes" : "No";

                isActive = bool.Parse(reader["Active"].ToString());

                BadgeListItem badgeItem = new BadgeListItem();

                badgeItem = new BadgeListItem()
                {
                    AccountID = int.Parse(reader["AccountID"].ToString()),
                    CompanyName = companyName,

                    SerialNumber = reader["SerialNumber"].ToString(),
                    FullName = reader["FullName"].ToString(),
                    Location = reader["Location"].ToString(),
                    BodyRegion = reader["BodyRegion"].ToString(),
                    ProductColor = reader["ProductColor"].ToString(),
                    FormattedInitialized = isInitialized,
                    Active = (isActive) ? "Yes" : "No",
                };

                // Fill row details.
                if (DateTime.TryParse(reader["ServiceStart"].ToString(), out tmpDate))
                    badgeItem.ServiceStart = tmpDate;

                if (DateTime.TryParse(reader["ServiceEnd"].ToString(), out tmpDate))
                    badgeItem.ServiceEnd = tmpDate;

                if (Int32.TryParse(reader["UserID"].ToString(), out tmpInt))
                    badgeItem.UserID = tmpInt;

                if (Int32.TryParse(reader["OrderID"].ToString(), out tmpInt))
                    badgeItem.OrderID = tmpInt;

                badgeList.Badges.Add(badgeItem);
            }

            ItemList = badgeList.Badges;

            // Bind the results to the GridView.
            gvAccountBadges.DataSource = ItemList;
            gvAccountBadges.DataBind();
        }
        catch
        {
            BindBodyRegionCombo();
            BindProductColorCombo();
            ddlInitailized.SelectedIndex = 0;
            ddlActive.SelectedIndex = 0;
        }
    }

    protected void gvAccountBadges_Sorting(object sender, GridViewSortEventArgs e)
    {
        SortAccountBadgesGrid();
    }

    /// <summary>
    /// Sort GridView based on which column was selected and by Ascending or Descending.
    /// </summary>
    private void SortAccountBadgesGrid()
    {
        BindAccountBadgesGrid();

        var itms = (from b in ItemList
                    select b).OrderByDescending(b => b.SerialNumber).ToList();

        // Sort the list based on the column.
        switch (gvAccountBadges.CurrentSortedColumn)
        {
            case "SerialNumber":
                itms.Sort(delegate(BadgeListItem i1, BadgeListItem i2) { return i1.SerialNumber.CompareTo(i2.SerialNumber); });
                break;

            case "FullName":
                itms.Sort(delegate(BadgeListItem i1, BadgeListItem i2) { return i1.FullName.CompareTo(i2.FullName); });
                break;

            case "BodyRegion":
                itms.Sort(delegate(BadgeListItem i1, BadgeListItem i2) { return i1.BodyRegion.CompareTo(i2.BodyRegion); });
                break;

            case "ProductColor":
                itms.Sort(delegate(BadgeListItem i1, BadgeListItem i2) { return i1.ProductColor.CompareTo(i2.ProductColor); });
                break;

            case "OrderID":
                itms.Sort(delegate(BadgeListItem i1, BadgeListItem i2) { return i1.OrderID.CompareTo(i2.OrderID); });
                break;

            case "Location":
                itms.Sort(delegate(BadgeListItem i1, BadgeListItem i2) { return i1.Location.CompareTo(i2.Location); });
                break;

            case "Initialized":
                itms.Sort(delegate(BadgeListItem i1, BadgeListItem i2) { return i1.FormattedInitialized.CompareTo(i2.FormattedInitialized); });
                break;

            case "Active":
                itms.Sort(delegate(BadgeListItem i1, BadgeListItem i2) { return i1.Active.CompareTo(i2.Active); });
                break;

            case "ServiceStart":
                itms.Sort(delegate(BadgeListItem i1, BadgeListItem i2)
                {
                    return (i1.ServiceStart.HasValue) ? i1.ServiceStart.Value.CompareTo((i2.ServiceStart.HasValue) ? i2.ServiceStart.Value : DateTime.MinValue) : 0;
                });
                break;
            case "ServiceEnd":
                itms.Sort(delegate(BadgeListItem i1, BadgeListItem i2)
                {
                    return (i1.ServiceEnd.HasValue) ? i1.ServiceEnd.Value.CompareTo((i2.ServiceEnd.HasValue) ? i2.ServiceEnd.Value : DateTime.MinValue) : 0;
                });
                break;
        }

        // Flip the list.
        if (gvAccountBadges.CurrentSortDirection == SortDirection.Descending) itms.Reverse();

        // Display the results to the GridView.
        gvAccountBadges.DataSource = itms;
        gvAccountBadges.DataBind();
    }

    /// <summary>
    /// Export grid data to Excel.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void lnkbtnExport_Click(object sender, EventArgs e)
    {
        // Bind the grid, to fill the Renewals list for Excel export.
        BindAccountBadgesGrid();

        // Create an data table used to export.
        DataTable exportTable = new DataTable();

        //Create columns for this DataTable.
        DataColumn colAccountID = new DataColumn("Account#");
        DataColumn colCompanyName = new DataColumn("Company Name");
        DataColumn colSerialNumber = new DataColumn("Serial #");
        DataColumn colAssignedTo = new DataColumn("Assigned To");
        DataColumn colBodyRegion = new DataColumn("Body Region");
        DataColumn colProductColor = new DataColumn("Product Color");
        DataColumn colOrderID = new DataColumn("Order#");
        DataColumn colServiceStart = new DataColumn("Service Start");
        DataColumn colServiceEnd = new DataColumn("Service End");
        DataColumn colLocation = new DataColumn("Location");
        DataColumn colInitialized = new DataColumn("Initialized");
        DataColumn colActive = new DataColumn("Active");

        //Define DataType of the Columns.
        colAccountID.DataType = System.Type.GetType("System.String");
        colCompanyName.DataType = System.Type.GetType("System.String");
        colSerialNumber.DataType = System.Type.GetType("System.String");
        colAssignedTo.DataType = System.Type.GetType("System.String");
        colBodyRegion.DataType = System.Type.GetType("System.String");
        colProductColor.DataType = System.Type.GetType("System.String");
        colOrderID.DataType = System.Type.GetType("System.String");
        colLocation.DataType = System.Type.GetType("System.String");
        colServiceStart.DataType = System.Type.GetType("System.String");
        colServiceEnd.DataType = System.Type.GetType("System.String");
        colInitialized.DataType = System.Type.GetType("System.String");
        colActive.DataType = System.Type.GetType("System.String");

        //Add All These Columns into exportTable.
        exportTable.Columns.Add(colAccountID);
        exportTable.Columns.Add(colCompanyName);
        exportTable.Columns.Add(colSerialNumber);
        exportTable.Columns.Add(colAssignedTo);
        exportTable.Columns.Add(colBodyRegion);
        exportTable.Columns.Add(colProductColor);
        exportTable.Columns.Add(colOrderID);
        exportTable.Columns.Add(colLocation);
        exportTable.Columns.Add(colServiceStart);
        exportTable.Columns.Add(colServiceEnd);
        exportTable.Columns.Add(colInitialized);
        exportTable.Columns.Add(colActive);

        //// Add the rows from the account renewals list
        foreach (BadgeListItem bl in ItemList)
        {
            // Create a new table row
            DataRow dr = exportTable.NewRow();
            dr[colAccountID] = bl.AccountID;
            dr[colCompanyName] = companyName;
            dr[colSerialNumber] = bl.SerialNumber;
            dr[colAssignedTo] = bl.FullName;
            dr[colBodyRegion] = bl.BodyRegion;
            dr[colProductColor] = bl.ProductColor;
            dr[colOrderID] = bl.OrderID;
            dr[colLocation] = bl.Location;
            dr[colServiceStart] = bl.ServiceStart.Value.ToShortDateString();
            dr[colServiceEnd] = bl.ServiceEnd.Value.ToShortDateString();

            if (bl.FormattedInitialized == "Yes")
                dr[colInitialized] = "Yes";
            else
                dr[colInitialized] = "No";

            if (bl.Active == "Yes")
                dr[colActive] = "Yes";
            else
                dr[colActive] = "No";

            // Add the row to the table.
            exportTable.Rows.Add(dr);
        }

        // Build the export table.
        TableExport tableExport = new TableExport(exportTable);

        try
        {
            // Read the CSS template from file.
            tableExport.Stylesheet = System.IO.File.ReadAllText(Server.MapPath("~/css/export/grids.css"));
        }
        catch (Exception ex)
        {
            throw new Exception(string.Format("Error: {0}", ex.Message));
        }

        try
        {
            // Create the export file based on the selected value.
            tableExport.Export("AccountBadges", "XLS");

            ExportFile file = tableExport.File;

            // Clear everything out.
            Response.Clear();
            Response.ClearHeaders();

            // Set the response headers.
            Response.ContentType = file.ContentType;
            Response.AddHeader("Content-Disposition", file.ContentDisposition);

            //Write to Excel file.
            if (file.Content.GetType() == typeof(byte[]))
                Response.BinaryWrite((byte[])file.Content);
            else
                Response.Write(file.Content.ToString());
        }
        catch (Exception ex)
        {
            throw new Exception(string.Format("Error: {0}", ex.Message));
        }

        Response.Flush();
        Response.End();
    }

    public override void VerifyRenderingInServerForm(Control control)
    {
        return;
    }

    protected void lnkbtnClearFilters_Click(object sender, EventArgs e)
    {
        BindBodyRegionCombo();
        BindProductColorCombo();
        ddlInitailized.SelectedIndex = 0;
        ddlActive.SelectedIndex = 0;

        BindAccountBadgesGrid();
    }

    /// <summary>
    /// Return back to Information Finder - Account details.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnReturn_Click(object sender, EventArgs e)
    {
        Response.Redirect(string.Format("~/InformationFinder/Details/Account.aspx?ID={0}#Badges_Tab", accountID));
    }
}
