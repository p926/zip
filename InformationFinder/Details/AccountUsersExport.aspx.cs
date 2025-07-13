using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Instadose.Data;

using Instadose;

public partial class InformationFinder_Details_AccountUsersExport : System.Web.UI.Page
{

    // Create the database LINQ objects.
    InsDataContext idc = new InsDataContext();

    // DataTable results;
    public static DataTable ExcelResults;

    private int accountID = 0;
    private int tmpInt = 0;
    private string companyName;

    private List<UserListItem> ItemList;

    [Serializable()]
    public class UserList
    {
        public UserList()
        {
        }

        /// <summary>
        /// Collection of Account Renewals list items.
        /// </summary>
        public List<UserListItem> Users { get; set; }
    }

    [Serializable()]
    public class UserListItem
    {
        public UserListItem()
        {
        }

        /// <summary>
        /// Identifier of the Account.
        /// </summary>
        public int AccountID { get; set; }
        public string CompanyName { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public string UserRoleName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Location { get; set; }
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

        // Prevent the page from redrawing the table if not a first time load.
        if (Page.IsPostBack) return;

        BindUserRolesDropDownList();
        BindLocationDropDownList();

        // Bind the GridView.
        BindAccountUsersGridView();
    }

    public override void VerifyRenderingInServerForm(Control control)
    {
        return;
    }

    #region DROPDOWNLIST CONTROLS.
    /// <summary>
    /// Called from DropDownList controls, force binds the GridView.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void GridViewBinder(object sender, EventArgs e)
    {
        // Reset the page to 0.
        gvAccountUsers.PageIndex = 0;

        // Bind the GridView.
        BindAccountUsersGridView();
    }

    private void BindUserRolesDropDownList()
    {
        ddlUserRoles.DataSource = (from u in idc.UserRoles
                                   where u.Active
                                   orderby u.UserRoleName
                                   select new
                                   {
                                       UserRoleID = u.UserRoleID,
                                       UserRole = u.UserRoleName
                                   });

        ddlUserRoles.DataTextField = "UserRole";
        ddlUserRoles.DataValueField = "UserRoleID";

        ddlUserRoles.DataBind();

        ddlUserRoles.Items.Insert(0, new ListItem("All", ""));
    }

    private void BindLocationDropDownList()
    {

        ddlLocation.DataSource = (from l in idc.Locations
                                  where l.AccountID == accountID && l.Active
                                  orderby l.LocationName
                                  select new
                                  {
                                      LocationID = l.LocationID,
                                      Location = l.LocationName
                                  }).Distinct().ToList();

        ddlLocation.DataTextField = "Location";
        ddlLocation.DataValueField = "LocationID";

        ddlLocation.DataBind();
        ddlLocation.Items.Insert(0, new ListItem("All", ""));
    }
    #endregion

    #region GRIDVIEW.
    /// <summary>
    /// Query the DataSource and bind the GridView.
    /// </summary>
    private void BindAccountUsersGridView()
    {
        try
        {
            // Create the sql connection from the connection string in the web.config
            SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
                ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

            // Create the sql command.
            SqlCommand sqlCmd = new SqlCommand();
            sqlCmd.CommandType = CommandType.Text;

            string query = "SELECT * FROM [vw_GetAccountUsersExport]";
            string filters = "";

            filters += "([AccountID] = @AccountID)";

            sqlCmd.Parameters.Add(new SqlParameter("@AccountID", accountID));

            // Set the filter parameter for User Roles.
            if (!ddlUserRoles.SelectedItem.Text.Contains("All"))
            {
                // Append the AND if needed.
                filters += " AND ";

                filters += "([UserRoleName] = @UserRole)";
                sqlCmd.Parameters.Add(new SqlParameter("UserRole", ddlUserRoles.SelectedItem.Text));
            }

            // Set the filter parameter for billing term
            if (!ddlLocation.SelectedItem.Text.Contains("All"))
            {
                // Append the AND if needed.
                if (filters != "") filters += " AND ";

                filters += "([Location] = @Location)";
                sqlCmd.Parameters.Add(new SqlParameter("Location", ddlLocation.SelectedItem.Text));
            }

            // Set the filter parameter for the Active (status).
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

            // Add the ORDER BY and the SORT DIRECTION.
            query += " ORDER BY " + gvAccountUsers.CurrentSortedColumn + ((gvAccountUsers.CurrentSortDirection == SortDirection.Ascending) ? " ASC" : " DESC");

            // Pass the query string to the SQL Command.
            sqlCmd.Connection = sqlConn;

            bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
            if (!openConn) sqlCmd.Connection.Open();

            sqlCmd.CommandText = query;

            SqlDataReader reader = sqlCmd.ExecuteReader();

            UserList userList = new UserList();
            userList.Users = new List<UserListItem>();

            bool isActive = true;

            while (reader.Read())
            {

                isActive = bool.Parse(reader["Active"].ToString());

                UserListItem userItem = new UserListItem();

                userItem = new UserListItem()
                {
                    AccountID = int.Parse(reader["AccountID"].ToString()),
                    CompanyName = companyName,

                    UserName = reader["UserName"].ToString(),
                    FullName = reader["FullName"].ToString(),
                    Location = reader["Location"].ToString(),
                    Email = reader["Email"].ToString(),
                    UserRoleName = reader["UserRoleName"].ToString(),
                    Active = (isActive) ? "Yes" : "No",
                };

                // Fill row details.
                if (Int32.TryParse(reader["UserID"].ToString(), out tmpInt))
                    userItem.UserID = tmpInt;

                userList.Users.Add(userItem);
            }

            ItemList = userList.Users;

            // Bind the results to the GridView.
            gvAccountUsers.DataSource = ItemList;
            gvAccountUsers.DataBind();
        }
        catch
        {
            BindUserRolesDropDownList();
            BindLocationDropDownList();
            ddlUserRoles.SelectedIndex = 0;
            ddlLocation.SelectedIndex = 0;
            ddlActive.SelectedIndex = 0;
        }
    }

    /// <summary>
    /// Fired when the page has been changed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvAccountUsers_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvAccountUsers.PageIndex = e.NewPageIndex;
        BindAccountUsersGridView();
    }

    protected void gvAccountUsers_Sorting(object sender, GridViewSortEventArgs e)
    {
        sortAccountBadgesGrid();
    }

    /// <summary>
    /// SORT GridView based on which column was selected and by ASC or DESC.
    /// </summary>
    private void sortAccountBadgesGrid()
    {
        // Display all items.
        BindAccountUsersGridView();

        var itms = (from u in ItemList
                    select u).OrderByDescending(u => u.UserRoleName).ToList();

        // Sort the list based on the column.
        switch (gvAccountUsers.CurrentSortedColumn)
        {
            case "UserName":
                // Sort the list by UserName.
                itms.Sort(delegate(UserListItem i1, UserListItem i2) { return i1.UserName.CompareTo(i2.UserName); });
                break;

            case "UserRoleName":
                // Sort the list by UserRoleName.
                itms.Sort(delegate(UserListItem i1, UserListItem i2) { return i1.UserRoleName.CompareTo(i2.UserRoleName); });
                break;

            case "FullName":
                // Sort the list by FullName.
                itms.Sort(delegate(UserListItem i1, UserListItem i2) { return i1.FullName.CompareTo(i2.FullName); });
                break;

            case "Email":
                // Sort the list by Email.
                itms.Sort(delegate(UserListItem i1, UserListItem i2) { return i1.Email.CompareTo(i2.Email); });
                break;

            case "Location":
                // Sort the list by Location.
                itms.Sort(delegate(UserListItem i1, UserListItem i2) { return i1.Location.CompareTo(i2.Location); });
                break;

            case "Active":
                // Sort the list by Active (status).
                itms.Sort(delegate(UserListItem i1, UserListItem i2) { return i1.Active.CompareTo(i2.Active); });
                break;
        }

        // Flip the list.
        if (gvAccountUsers.CurrentSortDirection == SortDirection.Descending) itms.Reverse();

        // Display the results to the GridView.
        gvAccountUsers.DataSource = itms;
        gvAccountUsers.DataBind();
    }
    #endregion

    #region EXPORT TO EXCEL.
    /// <summary>
    /// Export GridView data to Excel file.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void lnkbtnExport_Click(object sender, EventArgs e)
    {
        // Bind the GridView for Excel export.
        BindAccountUsersGridView();

        // Create an DataTable to be used for export.
        DataTable exportTable = new DataTable();

        // Create columns for this DataTable.
        DataColumn colAccountID = new DataColumn("Account#");
        DataColumn colCompanyName = new DataColumn("Company Name");

        DataColumn colUserName = new DataColumn("Name");
        DataColumn colUserRole = new DataColumn("User Role");
        DataColumn colFullName = new DataColumn("Full Name");
        DataColumn colEmail = new DataColumn("Email");
        DataColumn colLocation = new DataColumn("Location");
        DataColumn colActive = new DataColumn("Active");

        // Define DataType of the columns.
        colAccountID.DataType = System.Type.GetType("System.String");
        colCompanyName.DataType = System.Type.GetType("System.String");

        colUserName.DataType = System.Type.GetType("System.String");
        colUserRole.DataType = System.Type.GetType("System.String");

        colFullName.DataType = System.Type.GetType("System.String");
        colEmail.DataType = System.Type.GetType("System.String");

        colLocation.DataType = System.Type.GetType("System.String");
        colActive.DataType = System.Type.GetType("System.String");

        // Add All these columns into exportTable object.
        exportTable.Columns.Add(colAccountID);
        exportTable.Columns.Add(colCompanyName);

        exportTable.Columns.Add(colUserName);
        exportTable.Columns.Add(colUserRole);
        exportTable.Columns.Add(colFullName);
        exportTable.Columns.Add(colEmail);
        exportTable.Columns.Add(colLocation);
        exportTable.Columns.Add(colActive);

        // Add the rows from the ItemList.
        foreach (UserListItem ul in ItemList)
        {
            // Create a new table row.
            DataRow dr = exportTable.NewRow();
            dr[colAccountID] = ul.AccountID;
            dr[colCompanyName] = companyName;

            dr[colUserName] = ul.UserName;
            dr[colUserRole] = ul.UserRoleName;
            dr[colFullName] = ul.FullName;
            dr[colEmail] = ul.Email;
            dr[colLocation] = ul.Location;

            if (ul.Active == "Yes")
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
            tableExport.Stylesheet =
                System.IO.File.ReadAllText(Server.MapPath("~/css/export/grids.css"));
        }
        catch (Exception ex)
        {
            throw ex;
        }

        try
        {
            // Create the export file based on the selected value.
            tableExport.Export("AccountUsers", "XLS");

            ExportFile file = tableExport.File;

            // Clear everything out.
            Response.Clear();
            Response.ClearHeaders();

            // Set the response headers.
            Response.ContentType = file.ContentType;
            Response.AddHeader("Content-Disposition", file.ContentDisposition);

            // Write to Excel file.
            if (file.Content.GetType() == typeof(byte[]))
            {
                Response.BinaryWrite((byte[])file.Content);
            }
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

    protected void lnkbtnClearFilters_Click(object sender, EventArgs e)
    {
        BindUserRolesDropDownList();
        BindLocationDropDownList();
        ddlUserRoles.SelectedIndex = 0;
        ddlLocation.SelectedIndex = 0;
        ddlActive.SelectedIndex = 0;

        BindAccountUsersGridView();
    }

    protected void btnReturn_Click(object sender, EventArgs e)
    {
        Response.Redirect(string.Format("~/InformationFinder/Details/Account.aspx?ID={0}#Users_Tab", accountID));
    }
}
