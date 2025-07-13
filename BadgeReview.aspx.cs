using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Instadose;
using Instadose.Data;
using Mirion.DSD.Utilities.Constants;
using Portal.Instadose;
using Telerik.Web.UI;

public partial class TechOps_BadgeReview : System.Web.UI.Page
{
    private InsDataContext idc = new InsDataContext();
    private AppDataContext adc = new AppDataContext();
    private DeviceInventory deviceInventory = new DeviceInventory();
    private List<DeviceBaseline> deviceBaseline;
    private UserDeviceRead userDeviceRead = new UserDeviceRead();

    public string serialNumber = "";
    public int rID = 0;
    public DateTime startDate;
    public DateTime endDate;
    private int option;
    private bool includeSoftReads = false;

    // Variables for General Information Tab (Warren Kakemoto(?)).
    protected Boolean goodCall = false;
    protected Int32 wkDeviceID;
    protected Int32 wkCalibReadRID;

    // String to hold the current Username.
    private string UserName = "Unknown";

    // Variables to be used in getting RID Details for Review Tab.
    private int? recentlyReviewedRID = null;
    private DateTime reviewedDate = DateTime.Now;
    private string reviewedBy = "";
    private int analysisActionTypeID = 0;
    private string csNotes = "";
    private string allFailedTestNames = "";

    private string emailToList = "";
    private string shippingAddress = "";
    private string errorMessage = "";

    private int accountID = 0;
    private int locationID = 0;
    private int userID = 0;

    // NOTE: All ExposureDate varibles are actually CreatedDate values.

    #region ON PAGE LOAD.
    protected void Page_Load(object sender, EventArgs e)
    {
        try { this.UserName = User.Identity.Name.Split('\\')[1]; }
        catch { this.UserName = "Unknown"; }

        serialNumber = Request.QueryString["SerialNo"] ?? "";
        Int32.TryParse(Request.QueryString["RID"], out rID);

        // Corrected on 03/11/2015 by Anuradha Nandi.
        // The IF Statement included "|| rID = 0" which should not cause the page to return to the Serial No. Search page.
        if (string.IsNullOrEmpty(serialNumber)) Response.Redirect("SerialNoSearch.aspx");

        // Set a default start and end date.
        startDate = getEarliestExposureDate(serialNumber);
        endDate = DateTime.Now.AddDays(1);

        if (Page.IsPostBack) return;

        var productGroupID = GetProductGroupIDBySerialNumber(serialNumber);
        var IDVueProductGroupIDs = new List<int>()
        {
            ProductGroupIDConstants.InstadoseVue,
            ProductGroupIDConstants.InstadoseVueBeta,
            ProductGroupIDConstants.InstadoseVueNeutron
        };

        bool isIDVue = IDVueProductGroupIDs.Contains(productGroupID);

        ViewState["IsIDVue"] = isIDVue;
        ViewState["ProductGroupID"] = productGroupID;

        Session["IncludedReadType"] = "User Read";

        hyprlnkRecall.NavigateUrl = string.Format("ReturnAddNewDeviceRMA.aspx?SerialNo={0}", serialNumber);
        hyprlnkRecall.ToolTip = string.Format("Recall #{0}", serialNumber);

        // If the page loads with the chart querystring param, only send JSON data.
        if (Request.QueryString["Chart"] != null)
        {
            DateTime.TryParse(Request.QueryString["StartDate"], out startDate);
            DateTime.TryParse(Request.QueryString["EndDate"], out endDate);
            Int32.TryParse(Request.QueryString["Option"], out option);
            Boolean.TryParse(Request.QueryString["IncludeSoftReads"], out includeSoftReads);

            string chartTitle = Request.QueryString["Chart"];

            switch (chartTitle)
            {
                case "Baseline":
                    getBaselineTempCalibraChartData(serialNumber, rID);
                    break;
                case "Dose":
                    getDoseRangeChartData(serialNumber, rID, startDate, endDate, option, includeSoftReads);
                    break;
                case "Rough":
                    getRoughDoseChartData(serialNumber, rID, startDate, endDate, option, includeSoftReads);
                    break;
                case "Temp":
                    if (isIDVue)
                        getIDVueTemperatureRangeChartData(serialNumber, startDate, endDate);
                    else
                        getTemperatureRangeChartData(serialNumber, rID, startDate, endDate, option, includeSoftReads);                    
                    break;
                case "Difference":
                    getTemperatureDifferenceChartData(serialNumber, rID, startDate, endDate, option, includeSoftReads);
                    break;
                case "Cumulative":
                    getCumulativeDoseChartData(serialNumber, rID, startDate, endDate, includeSoftReads);
                    break;
                case "DLTDHT":
                    getDLTMinusDHTChartData(serialNumber, rID, startDate, endDate, includeSoftReads);
                    break;
                case "BatteryPercent":
                    getBatteryPercentChartData(serialNumber, rID, startDate, endDate, includeSoftReads);
                    break;
                default:
                    break;
            }

            // Prevent anything else from occurring.
            return;
        }

        if (!IsRIDValid(rID, serialNumber) || rID == 0)
            lnkbtnCompleteReviewProcess.Visible = false;
        else
            lnkbtnCompleteReviewProcess.Visible = true;

        lblBadgeSerialNumber.Text = serialNumber;
        PopulateAnalysisActionTypeRBL();
        lblReadID.Text = rID.ToString();
        pnlEmailForm.Visible = false;

        // Used in Baseline Tab.
        if (isIDVue)
        {
            populateIDVueCalCoefficient(serialNumber);
            getIDVueBLDac(serialNumber);
        }
        else
        {
            populateSensitivityAndCoefficient(serialNumber, startDate, endDate, 1);
            getBaselineDataTable(serialNumber);
        }

        Tab7.Visible = isIDVue;
        Tab8.Visible = isIDVue;

        Motions_tab.Visible = isIDVue;
        Exceptions_tab.Visible = isIDVue;

        //Tab1.Visible = !isIDVue;
        divBaseline.Visible = !isIDVue;
        divVueBaseline.Visible = isIDVue;

        // Tabs (respectively).
        //rgAccounts_Parent.DataBind();
        //rgReads_Parent.DataBind();
        //rgTechnicalNotes.DataBind();
        //rgBadgeReviewHistory.DataBind();




    }
    #endregion

    #region GENERAL FUNCTIONS.
    private bool IsRIDValid(int rid, string serialnumber)
    {
        var getReadRecord = (from udr in idc.UserDeviceReads
                             where udr.RID == rid
                             && udr.DeviceInventory.SerialNo == serialnumber
                             && udr.IsAnalyzed == true
                             && udr.AnalysisActionTypeID == null
                             select udr).FirstOrDefault();

        if (getReadRecord != null) return true;
        else return false;
    }

    private int GetProductGroupIDBySerialNumber(string serialNumber)
    {
        var productGroupID = (from di in idc.DeviceInventories
                              join p in idc.Products on di.ProductID equals p.ProductID
                              where di.SerialNo == serialNumber
                              select p.ProductGroupID).FirstOrDefault();

        return productGroupID;
    }

    /// <summary>
    /// Function gets the earliest Exposure Date for a given Badge (Serial Number).
    /// This is then set as the Starting Date and then End Date is defaulted to DateTime.Now.
    /// </summary>
    /// <param name="serialNumber"></param>
    /// <returns></returns>
    private DateTime getEarliestExposureDate(string serialnumber)
    {
        var firstRead = (from udr in idc.UserDeviceReads
                                 where udr.DeviceInventory.SerialNo == serialnumber
                                 orderby udr.CreatedDate
                                 select udr).FirstOrDefault();

        if (firstRead != null)
            return firstRead.CreatedDate;
        else
            return DateTime.Now;
    }

    /// <summary>
    /// Populate RBL for AnalysisActionTypes with DB values (dynamically).
    /// </summary>
    private void PopulateAnalysisActionTypeRBL()
    {
        var analysisActionTypes = (from aat in idc.AnalysisActionTypes
                                   select new
                                   {
                                       aat.AnalysisActionTypeID,
                                       aat.AnalysisActionName
                                   }).Distinct();

        rdobtnlstAnalysisActionType.DataSource = analysisActionTypes;
        this.rdobtnlstAnalysisActionType.DataTextField = "AnalysisActionName";
        this.rdobtnlstAnalysisActionType.DataValueField = "AnalysisActionTypeID";
        this.rdobtnlstAnalysisActionType.DataBind();
    }

    /// <summary>
    /// Populate the Wearer, Location, and Account (IsDefault/Main) Shipping Addresses (CheckBoxLists).
    /// </summary>
    /// <param name="serialnumber"></param>
    private void GetAllShippingAddresses(string serialnumber)
    {
        // Get UserID, LocationID, and AccountID based on Serial No.
        var idInformation = (from ud in idc.UserDevices
                             where ud.DeviceInventory.SerialNo == serialnumber
                             orderby ud.AssignmentDate descending
                             select new
                             {
                                 AccountID = ud.User.AccountID,
                                 LocationID = ud.User.LocationID,
                                 UserID = ud.UserID
                             }).FirstOrDefault();

        if (idInformation == null) return;

        // Assign UserID, LocationID, and AccountID to Global Variables.
        userID = idInformation.UserID;
        locationID = idInformation.LocationID;
        accountID = idInformation.AccountID;

        var userAddresses = (from u in idc.Users
                             where u.UserID == userID
                             && u.Active == true
                             select u).ToList();

        if (userAddresses == null)
        {
            lblWearerAddressNotAvailable.Visible = true;
        }
        else
        {
            lblWearerAddressNotAvailable.Visible = false;

            string fullUserAddress = "";

            foreach (var uAddress in userAddresses)
            {
                fullUserAddress = (((uAddress.Address1 != "") ? uAddress.Address1 + "<br />" : String.Empty) +
                                  ((uAddress.Address2 != "") ? uAddress.Address2 + "<br />" : String.Empty) +
                                  ((uAddress.Address3 != "") ? uAddress.Address3 + "<br />" : String.Empty) +
                                  ((uAddress.City != "") ? uAddress.City + ", " : String.Empty) +
                                  ((uAddress.StateID.HasValue) ? uAddress.State.StateAbbrev + " " : String.Empty) +
                                  ((uAddress.PostalCode != "") ? uAddress.PostalCode + "<br />" : String.Empty) +
                                  ((uAddress.CountryID.HasValue) ? uAddress.Country.CountryName : String.Empty)).ToUpper();

                chkbxlstWearerAddresses.Items.Add(fullUserAddress);
            }
        }

        var locationAddresses = (from l in idc.Locations
                                 where l.LocationID == locationID
                                 && l.Active == true
                                 select l).ToList();

        if (locationAddresses == null)
        {
            lblLocationAddressNotAvailable.Visible = true;
        }
        else
        {
            lblLocationAddressNotAvailable.Visible = false;

            string fullLocationAddress = "";

            foreach (var lAddress in locationAddresses)
            {
                fullLocationAddress = (((lAddress.ShippingAddress1 != "") ? lAddress.ShippingAddress1 + "<br />" : String.Empty) +
                                      ((lAddress.ShippingAddress2 != "") ? lAddress.ShippingAddress2 + "<br />" : String.Empty) +
                                      ((lAddress.ShippingAddress3 != "") ? lAddress.ShippingAddress3 + "<br />" : String.Empty) +
                                      ((lAddress.ShippingCity != "") ? lAddress.ShippingCity + ", " : String.Empty) +
                                      ((lAddress.ShippingStateID != null) ? lAddress.ShippingState.StateAbbrev + " " : String.Empty) +
                                      ((lAddress.ShippingPostalCode != "") ? lAddress.ShippingPostalCode + "<br />" : String.Empty) +
                                      ((lAddress.ShippingCountryID != null) ? lAddress.ShippingCountry.CountryName : String.Empty)).ToUpper();

                chkbxlstLocationAddresses.Items.Add(fullLocationAddress);
            }
        }

        var accountMainAddresses = (from l in idc.Locations
                                    where l.AccountID == accountID
                                    && l.IsDefaultLocation == true
                                    && l.Active == true
                                    select l).ToList();

        if (accountMainAddresses == null)
        {
            lblAccountAddressNotAvailable.Visible = true;
        }
        else
        {
            lblAccountAddressNotAvailable.Visible = false;

            string fullAccountAddress = "";

            foreach (var aAddress in accountMainAddresses)
            {
                fullAccountAddress = (((aAddress.ShippingAddress1 != "") ? aAddress.ShippingAddress1 + "<br />" : String.Empty) +
                                     ((aAddress.ShippingAddress2 != "") ? aAddress.ShippingAddress2 + "<br />" : String.Empty) +
                                     ((aAddress.ShippingAddress3 != "") ? aAddress.ShippingAddress3 + "<br />" : String.Empty) +
                                     ((aAddress.ShippingCity != "") ? aAddress.ShippingCity + ", " : String.Empty) +
                                     ((aAddress.ShippingStateID != null) ? aAddress.ShippingState.StateAbbrev + " " : String.Empty) +
                                     ((aAddress.ShippingPostalCode != "") ? aAddress.ShippingPostalCode + "<br />" : String.Empty) +
                                     ((aAddress.ShippingCountryID != null) ? aAddress.ShippingCountry.CountryName : String.Empty)).ToUpper();

                chkbxlstAccountAddresses.Items.Add(fullAccountAddress);
            }
        }
    }

    /// <summary>
    /// Populate the Wearer, Location Admin. and Account Admin. E-mails (CheckBoxLists).
    /// </summary>
    /// <param name="serialnumber"></param>
    private void GetAllEMailAddresses(string serialnumber)
    {
        var idInformation = (from u in idc.Users
                             join ud in idc.UserDevices on u.UserID equals ud.UserID
                             where ud.DeviceInventory.SerialNo == serialnumber
                             && ud.Active == true && u.Active == true
                             select new { u, ud }).FirstOrDefault();

        if (idInformation == null) return;

        int accountID = idInformation.u.AccountID;
        int locationID = idInformation.u.LocationID;
        int userID = idInformation.ud.UserID;

        // Get Wearer/User-Level E-Mail.
        // UserRoleID = 1
        var wearerEmails = (from u in idc.Users
                            where u.UserID == userID
                            && u.UserRoleID == 1
                            && u.OkToEmail == true
                            && u.Active == true
                            select u.Email).ToList();

        if (wearerEmails.Count() == 0)
        {
            lblWearerEmailsNotAvailable.Visible = true;
        }
        else
        {
            lblWearerEmailsNotAvailable.Visible = false;

            foreach (string wEmail in wearerEmails)
            {
                chkbxlstWearerEmailAddresses.Items.Add(wEmail.ToLower());
            }
        }

        // Get Location Administrator-Level E-Mail(s).
        // UserRoleID = 2
        var locationAdminEmails = (from u in idc.Users
                                   where u.LocationID == locationID
                                   && u.UserRoleID == 2
                                   && u.OkToEmail == true
                                   && u.Active == true
                                   select u.Email).ToList();

        if (locationAdminEmails.Count() == 0)
        {
            lblLocationAdminEmailsNotAvailable.Visible = true;
        }
        else
        {
            lblLocationAdminEmailsNotAvailable.Visible = false;

            foreach (string laEmail in locationAdminEmails)
            {
                chkbxlstLocationAdminEmailAddresses.Items.Add(laEmail.ToLower());
            }
        }

        // Get Account Administrator-Level E-Mail(s).
        // UserRoleID = 4
        var accountAdminEmails = (from u in idc.Users
                                  where u.AccountID == accountID
                                  && u.UserRoleID == 4
                                  && u.OkToEmail == true
                                  && u.Active == true
                                  select u.Email).ToList();

        if (accountAdminEmails.Count() == 0)
        {
            lblAccountAdminEmailsNotAvailable.Visible = true;
        }
        else
        {
            lblAccountAdminEmailsNotAvailable.Visible = false;

            foreach (string aaEmail in accountAdminEmails)
            {
                chkbxlstAccountAdminEmailAddresses.Items.Add(aaEmail.ToLower());
            }
        }
    }

    /// <summary>
    /// Uncheckes all CheckBoxList and Checkbox controls.
    /// </summary>
    /// <param name="pnlname"></param>
    /// <param name="chkbxlstname"></param>
    private void UncheckCheckBoxes(Panel pnlname, CheckBoxList chkbxlstname)
    {
        for (int i = 0; i < pnlname.Controls.Count; i++)
        {
            if (pnlname.Controls[i].GetType() == chkbxlstname.GetType())
            {
                CheckBoxList chkbxlst = (CheckBoxList)pnlname.Controls[i];
                foreach (ListItem item in chkbxlst.Items)
                {
                    item.Selected = false;
                }
            }
        }

        if (chkbxOtherEmailAddress.Checked || txtOtherEmailAddress.Text != "")
        {
            chkbxOtherEmailAddress.Checked = false;
            txtOtherEmailAddress.Text = String.Empty;
        }
    }
    #endregion

    #region ACCOUNT TAB FUNCTIONS.
    // -------------------- BEGIN :: ACCOUNT TAB FUNCTIONS -------------------- //
    /// <summary>
    /// The following functions are used to populate the Nested RadGrid for the Account Tab.
    /// The Global Variable, serialNumber is used for the Parent RadGrid to get the associated AccountID(s).
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    /// 
    protected void rgAccounts_Parent_NeedDataSource(object source, GridNeedDataSourceEventArgs e)
    {
        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.Text;

        string sqlQuery = "SELECT DISTINCT ACCT.AccountID AS [AccountID], " +
                          "ACCT.GDSAccount, " +
                          "ACCT.AccountName AS [CompanyName], " +
                          "(CASE WHEN AD.Active = 1 THEN 'YES' ELSE 'NO' END) AS [Active] " +
                          "FROM AccountDevices AS AD " +
                          "INNER JOIN Accounts AS ACCT ON AD.AccountID = ACCT.AccountID " +
                          "INNER JOIN DeviceInventory AS DI ON AD.DeviceID = DI.DeviceID " +
                          "WHERE DI.SerialNo = '" + serialNumber + "' AND ACCT.AccountID <> 2 " +
                          "ORDER BY ACCT.AccountID ASC";

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable dtParentDateTable = new DataTable();

        dtParentDateTable = new DataTable("Get All Account-Linked Badges");

        // Create the columns for the DataTable.
        dtParentDateTable.Columns.Add("AccountID", Type.GetType("System.String"));
        dtParentDateTable.Columns.Add("CompanyName", Type.GetType("System.String"));
        dtParentDateTable.Columns.Add("Active", Type.GetType("System.String"));

        while (sqlDataReader.Read())
        {
            DataRow drow = dtParentDateTable.NewRow();

            // Fill row details.
            drow["AccountID"] = sqlDataReader["GDSAccount"] == DBNull.Value? sqlDataReader["AccountID"].ToString() : sqlDataReader["GDSAccount"];
            drow["CompanyName"] = sqlDataReader["CompanyName"];
            drow["Active"] = sqlDataReader["Active"];

            // Add rows to DataTable.
            dtParentDateTable.Rows.Add(drow);
        }

        // Bind the results to the gridview.
        if (dtParentDateTable.Rows.Count == 0)
        {
            rgAccounts_Parent.PageSize = dtParentDateTable.Rows.Count + 1;
        }
        else
        {
            rgAccounts_Parent.PageSize = dtParentDateTable.Rows.Count;
        }

        rgAccounts_Parent.DataSource = dtParentDateTable;

        sqlConn.Close();
        sqlDataReader.Close();
    }

    protected void rgAccounts_Child_NeedDataSource(object source, GridNeedDataSourceEventArgs e)
    {
        GridDataItem parentItem = ((source as RadGrid).NamingContainer as GridNestedViewItem).ParentItem as GridDataItem;

        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.Text;

        // Using the vw_GetAllBadgesByAccountID.
        string sqlQuery = "SELECT v.*, a.GDSAccount FROM [vw_GetAllBadgesByAccountID] v join Accounts a on a.AccountID = v.AccountID";
        string filters = "";

        // Set the filter parameter for the Serial Number.
        if (serialNumber != null)
        {
            var accountIdString = parentItem.GetDataKeyValue("AccountID").ToString();

            if (int.TryParse(accountIdString, out _))
                filters += "(v.[AccountID] = @pAccountID or a.GDSAccount = @pAccountID)";
            else
                filters += "a.GDSAccount = @pAccountID";

            sqlCmd.Parameters.Add(new SqlParameter("@pAccountID", accountIdString));
        }

        // Add the filters to the query if needed.
        if (filters != "") sqlQuery += " WHERE " + filters;

        // Add the ORDER BY and DIRECTION.
        sqlQuery += " ORDER BY v.FullName ASC";

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable dtBadgesLinkedToAccountID = new DataTable("Account Badges");

        // Create the columns for the DataTable.
        dtBadgesLinkedToAccountID.Columns.Add("AccountID", Type.GetType("System.Int32"));
        dtBadgesLinkedToAccountID.Columns.Add("GDSAccount", Type.GetType("System.String"));
        dtBadgesLinkedToAccountID.Columns.Add("SerialNumber", Type.GetType("System.String"));
        dtBadgesLinkedToAccountID.Columns.Add("FullName", Type.GetType("System.String"));
        dtBadgesLinkedToAccountID.Columns.Add("UserID", Type.GetType("System.Int32"));
        dtBadgesLinkedToAccountID.Columns.Add("Location", Type.GetType("System.String"));
        dtBadgesLinkedToAccountID.Columns.Add("BodyRegion", Type.GetType("System.String"));
        dtBadgesLinkedToAccountID.Columns.Add("Color", Type.GetType("System.String"));
        dtBadgesLinkedToAccountID.Columns.Add("Initialized", Type.GetType("System.String"));
        dtBadgesLinkedToAccountID.Columns.Add("Active", Type.GetType("System.String"));

        while (sqlDataReader.Read())
        {
            DataRow drow = dtBadgesLinkedToAccountID.NewRow();

            // Fill row details.
            drow["AccountID"] = sqlDataReader["AccountID"];
            drow["GDSAccount"] = sqlDataReader["GDSAccount"];
            drow["SerialNumber"] = sqlDataReader["SerialNumber"];
            drow["FullName"] = sqlDataReader["FullName"];
            drow["UserID"] = sqlDataReader["UserID"];
            drow["Location"] = sqlDataReader["Location"];
            drow["BodyRegion"] = sqlDataReader["BodyRegion"];
            drow["Color"] = sqlDataReader["Color"];
            drow["Initialized"] = sqlDataReader["Initialized"];
            drow["Active"] = sqlDataReader["Active"];

            // Add rows to DataTable.
            dtBadgesLinkedToAccountID.Rows.Add(drow);
        }

        // Bind the results to the gridview.
        (source as RadGrid).DataSource = dtBadgesLinkedToAccountID;

        sqlConn.Close();
        sqlDataReader.Close();
    }

    protected void rgAccounts_Parent_ItemCommand(object sender, GridCommandEventArgs e)
    {
        if (e.CommandName == RadGrid.ExpandCollapseCommandName && !e.Item.Expanded)
        {
            GridDataItem parentItem = e.Item as GridDataItem;
            RadGrid rg = parentItem.ChildItem.FindControl("rgAccounts_Child") as RadGrid;
            rg.Rebind();
        }

        if (e.CommandName == RadGrid.ExpandCollapseCommandName && e.Item is GridDataItem)
        {
            ((GridDataItem)e.Item).ChildItem.FindControl("InnerContainer").Visible = !e.Item.Expanded;
        }
    }

    protected void rgAccounts_Parent_ItemCreated(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridNestedViewItem && !IsPostBack)
        {
            e.Item.FindControl("InnerContainer").Visible = true;
            rgAccounts_Parent.MasterTableView.Items[0].Expanded = false;
        }
    }
    // -------------------- END :: ACCOUNT TAB FUNCTIONS -------------------- //
    #endregion

    #region BASELINE TAB FUNCTIONS.
    // -------------------- BEGIN :: BASELINE TAB FUNCTIONS -------------------- //
    /// <summary>
    /// This populates the Baseline RadGrid with Baseline Reads 1-5.
    /// </summary>
    /// <param name="serialno"></param>
    private void getBaselineDataTable(string serialnumber)
    {
        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.Text;

        // Using the vw_DeviceBaselinesBySerialNo.
        string sqlQuery = "SELECT * FROM [vw_DeviceBaselinesBySerialNo]";
        string filters = "";

        // Set the filter parameter for the Serial Number.
        if (serialnumber != null)
        {
            filters += "([Serial Number] = @pSerialNo)";
            sqlCmd.Parameters.Add(new SqlParameter("@pSerialNo", serialnumber));
        }

        // Add the filters to the query if needed.
        if (filters != "") sqlQuery += " WHERE " + filters;

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable baselineDataTable = new DataTable("Baseline Reads");

        // Create the columns for the DataTable.
        baselineDataTable.Columns.Add("Read Count", Type.GetType("System.Int32"));
        baselineDataTable.Columns.Add("Deep Low", Type.GetType("System.Int32"));
        baselineDataTable.Columns.Add("Deep Low Temp.", Type.GetType("System.Int32"));
        baselineDataTable.Columns.Add("Deep High", Type.GetType("System.Int32"));
        baselineDataTable.Columns.Add("Deep High Temp.", Type.GetType("System.Int32"));
        baselineDataTable.Columns.Add("DC Dose", Type.GetType("System.Int32"));

        while (sqlDataReader.Read())
        {
            DataRow drow = baselineDataTable.NewRow();

            // Fill row details.
            drow["Read Count"] = sqlDataReader["Read Count"];
            drow["Deep Low"] = sqlDataReader["Deep Low"];
            drow["Deep Low Temp."] = sqlDataReader["Deep Low Temp."];
            drow["Deep High"] = sqlDataReader["Deep High"];
            drow["Deep High Temp."] = sqlDataReader["Deep High Temp."];
            drow["DC Dose"] = sqlDataReader["DC Dose"];

            // Add rows to DataTable.
            baselineDataTable.Rows.Add(drow);
        }

        sqlConn.Close();
        sqlDataReader.Close();

        rgBaseline.DataSource = baselineDataTable;
        rgBaseline.DataBind();
    }

    /// <summary>
    /// Function gets the DL and DH & DLT and DHT Sensitivities and Coefficients (respectively).
    /// Uses a Stored Procedure (developed by Rebecca Gottlieb) named sp_BadgeReview.
    /// </summary>
    /// <param name="serialnumber"></param>
    private void populateSensitivityAndCoefficient(string serialnumber, DateTime startdate, DateTime enddate, int option)
    {
        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = null;
        SqlDataReader sqlDataReader = null;

        sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);
        sqlConn.Open();

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand("sp_BadgeReview", sqlConn);
        sqlCmd.CommandType = CommandType.StoredProcedure;

        // Add Parameters.
        sqlCmd.Parameters.Add(new SqlParameter("@pSerialNo", serialnumber));
        sqlCmd.Parameters.Add(new SqlParameter("@pRID", rID));
        sqlCmd.Parameters.Add(new SqlParameter("@pStartDate", startdate));
        sqlCmd.Parameters.Add(new SqlParameter("@pEndDate", enddate));
        sqlCmd.Parameters.Add(new SqlParameter("@pOption", option));

        // Execute SQL Command.
        sqlDataReader = sqlCmd.ExecuteReader();

        // Display "Age of Badge" in Years Months Days.
        int totalNumberOfDays = 0;
        int totalYears = 0;
        int totalMonths = 0;
        int remainingDays = 0;

        string displayTotalDuration = "";

        // Iterate through database results and display on respective ASP.NET Labels.
        while (sqlDataReader.Read())
        {
            int.TryParse(sqlDataReader["AgeOfBadge"].ToString(), out totalNumberOfDays);
            totalYears = (totalNumberOfDays / 365);
            totalMonths = ((totalNumberOfDays % 365) / 30);
            remainingDays = ((totalNumberOfDays % 365) % 30);

            displayTotalDuration = string.Format("{0}Y {1}M {2}D", totalYears, totalMonths, remainingDays);

            lblDLSensitivity.Text = String.Format("{0:n1}", sqlDataReader["DLSens"]);
            lblDHSensitivity.Text = String.Format("{0:n1}", sqlDataReader["DHSens"]);
            //lblDLTCoefficient.Text = String.Format("{0:n2}", sqlDataReader["DLTCoeff"]);
            //lblDHTCoefficient.Text = String.Format("{0:n2}", sqlDataReader["DHTCoeff"]);
            lblDLTCoefficientBL1and2.Text = String.Format("{0:n2}", sqlDataReader["DLTCoeffBL1And2"]);
            lblDLTCoefficientBL2and3.Text = String.Format("{0:n2}", sqlDataReader["DLTCoeffBL2And3"]);
            lblDHTCoefficientBL1and2.Text = String.Format("{0:n2}", sqlDataReader["DHTCoeffBL1And2"]);
            lblDHTCoefficientBL2and3.Text = String.Format("{0:n2}", sqlDataReader["DHTCoeffBL2And3"]);
            //lblAgeOfBadge.Text = String.Format("{0:n0}", sqlDataReader["AgeOfBadge"]);
            lblAgeOfBadge.Text = displayTotalDuration.ToString();
        }

        sqlConn.Close();
        sqlDataReader.Close();
    }

    // BASELINE TEMPERATURE CALIBRATION HIGHCHART.
    private void getBaselineTempCalibraChartData(string serialnumber, int rid)
    {
        // Query the Stored Procedure.
        DataLayer dlLayer_BTC = new DataLayer();

        // Create List of all Parameters.
        List<SqlParameter> parameters = new List<SqlParameter>();
        parameters.Add(new SqlParameter("@pSerialNo", serialnumber));
        parameters.Add(new SqlParameter("@pRID", rid));

        // Get the DataSet.
        DataSet dsResult_BTC = dlLayer_BTC.GetDataSet("sp_GetBaselineTemperatureCalibration", CommandType.StoredProcedure, parameters);

        // Get the 1st Table.
        DataTable dtTable_BTC = dsResult_BTC.Tables[0];

        // If no data was returned exit.
        if (dsResult_BTC.Tables.Count == 0) return;

        var chartData1 = new List<ChartData<int, float>>();
        var chartData2 = new List<ChartData<int, float>>();
        foreach (DataRow row in dtTable_BTC.Rows)
        {
            Int32 dtBaselineReadCount = Int32.Parse(row["BRC"].ToString());

            chartData1.Add(new ChartData<int, float>(dtBaselineReadCount, float.Parse(row["DLT"].ToString())));
            chartData2.Add(new ChartData<int, float>(dtBaselineReadCount, float.Parse(row["DHT"].ToString())));
        }

        JavaScriptSerializer jss = new JavaScriptSerializer();
        // Write the high chart data to the screen content
        Response.Clear();
        Response.ContentType = "application/json; charset=utf-8";
        Response.Write(jss.Serialize(new { dlt = chartData1, dht = chartData2 }));
        Response.End();
    }

    // -------------------- BEGIN :: BASELINE TAB FUNCTIONS -------------------- //
    /// <summary>
    /// This populates data from BLDac
    /// </summary>
    /// <param name="serialno"></param>
    private void getIDVueBLDac(string serialnumber)
    {
        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.ReplConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.Text;

        // Using the vw_DeviceBaselinesBySerialNo.
        string sqlQuery = "SELECT * FROM [BL_Dac] WHERE ID3SerialNumber = @pSerialNo";
        sqlCmd.Parameters.Add(new SqlParameter("@pSerialNo", serialnumber));

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        while (sqlDataReader.Read())
        {
            lblDac0.Text = String.Format("{0:n6}", sqlDataReader["Dac0"]);
            lblDac1.Text = String.Format("{0:n6}", sqlDataReader["Dac1"]);
            lblDac2.Text = String.Format("{0:n6}", sqlDataReader["Dac2"]);
            lblDac3.Text = String.Format("{0:n6}", sqlDataReader["Dac3"]);
            lblDac4.Text = String.Format("{0:n6}", sqlDataReader["Dac4"]);
            lblDac5.Text = String.Format("{0:n6}", sqlDataReader["Dac5"]);
            lblDac6.Text = String.Format("{0:n6}", sqlDataReader["Dac6"]);
            lblDac7.Text = String.Format("{0:n6}", sqlDataReader["Dac7"]);
            lblDac8.Text = String.Format("{0:n6}", sqlDataReader["Dac8"]);
            lblDac9.Text = String.Format("{0:n6}", sqlDataReader["Dac9"]);
            lblDac10.Text = String.Format("{0:n6}", sqlDataReader["Dac10"]);
            lblDac11.Text = String.Format("{0:n6}", sqlDataReader["Dac11"]);
            lblDac12.Text = String.Format("{0:n6}", sqlDataReader["Dac12"]);
            lblDac13.Text = String.Format("{0:n6}", sqlDataReader["Dac13"]);
            lblDac14.Text = String.Format("{0:n6}", sqlDataReader["Dac14"]);
            lblDac15.Text = String.Format("{0:n6}", sqlDataReader["Dac15"]);
            lblDac16.Text = String.Format("{0:n6}", sqlDataReader["Dac16"]);
            lblDac17.Text = String.Format("{0:n6}", sqlDataReader["Dac17"]);
            lblDac18.Text = String.Format("{0:n6}", sqlDataReader["Dac18"]);
            lblDac19.Text = String.Format("{0:n6}", sqlDataReader["Dac19"]);
        }

        sqlConn.Close();
        sqlDataReader.Close();

    }

    private int GetReadUserID(int RID)
    {
        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.Text;

        // Using the vw_DeviceBaselinesBySerialNo.
        string sqlQuery = "SELECT UserID FROM UserDeviceReads WHERE RID = @pRID";
        sqlCmd.Parameters.Add(new SqlParameter("@pRID", RID));

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        var result = Convert.ToInt32(sqlCmd.ExecuteScalar());
        sqlConn.Close();

        return result;
    }

    /// <summary>
    /// This populates data from BLDac
    /// </summary>
    /// <param name="serialno"></param>
    private void populateIDVueCalCoefficient(string serialnumber)
    {
        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.ReplConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.Text;

        // Using the vw_DeviceBaselinesBySerialNo.
        string sqlQuery = "SELECT * FROM [BL_CalCoeff] WHERE ID3SerialNumber = @pSerialNo";
        sqlCmd.Parameters.Add(new SqlParameter("@pSerialNo", serialnumber));

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable blCalCoeffDataTable = new DataTable("BL CalCoeff");

        // Create the columns for the DataTable.
        blCalCoeffDataTable.Columns.Add("Channel", Type.GetType("System.String"));
        blCalCoeffDataTable.Columns.Add("A", Type.GetType("System.Double"));
        blCalCoeffDataTable.Columns.Add("B", Type.GetType("System.Double"));
        blCalCoeffDataTable.Columns.Add("C", Type.GetType("System.Int32"));
        blCalCoeffDataTable.Columns.Add("Km0", Type.GetType("System.Double"));
        blCalCoeffDataTable.Columns.Add("Km1", Type.GetType("System.Double"));
        blCalCoeffDataTable.Columns.Add("Km2", Type.GetType("System.Double"));
        blCalCoeffDataTable.Columns.Add("Trefm", Type.GetType("System.Int32"));
        blCalCoeffDataTable.Columns.Add("Trefc", Type.GetType("System.Int32"));

        while (sqlDataReader.Read())
        {
            DataRow drow = blCalCoeffDataTable.NewRow();

            int channel = Convert.ToInt32(sqlDataReader["Channel"]);
            // Fill row details.
            drow["Channel"] = channel == 0 ? "DH" : "DL";
            drow["A"] = sqlDataReader["A"];
            drow["B"] = sqlDataReader["B"];
            drow["C"] = sqlDataReader["C"];
            drow["Km0"] = sqlDataReader["Km0"];
            drow["Km1"] = sqlDataReader["Km1"];
            drow["Km2"] = sqlDataReader["Km2"];
            drow["Trefm"] = sqlDataReader["Trefm"];
            drow["Trefc"] = sqlDataReader["Trefc"];

            // Add rows to DataTable.
            blCalCoeffDataTable.Rows.Add(drow);
        }

        sqlConn.Close();
        sqlDataReader.Close();

        rbVueBlCalCoeff.DataSource = blCalCoeffDataTable;
        rbVueBlCalCoeff.DataBind();
    }

    // -------------------- END :: BASELINE TAB FUNCTIONS -------------------- //
    #endregion

    #region DOSE READS TAB FUNCTIONS.
    // -------------------- BEGIN :: DOSE READS TAB FUNCTIONS -------------------- //
    /// <summary>
    /// The following functions are used to populate the Nested RadGrid for the Reads Tab.
    /// The Global Variable, serialNumber is used for the Parent RadGrid to get the associated AccountID(s).
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    protected void rgReads_Parent_NeedDataSource(object source, GridNeedDataSourceEventArgs e)
    {
        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.Text;

        Int32.TryParse(Request.QueryString["RID"], out rID);
        var readUserID = GetReadUserID(rID);

        string sqlQuery = "SELECT DISTINCT ACCT.AccountID AS [AccountID], " +
                          "ACCT.GDSAccount, " +
                          "ACCT.AccountName AS [CompanyName], " +
                          "(USRS.FirstName + ' ' + USRS.LastName) AS [FullName], " +
                          "USRS.UserID AS [UserID], " +
                          "(CASE WHEN USRS.Active = 1 THEN 'YES' ELSE 'NO' END) AS [Active], " +
                          "CASE WHEN USRS.UserID = " + readUserID.ToString() + " THEN 1 ELSE 0 END AS IsRIDUser " +
                          "FROM Accounts AS ACCT " +
                          "INNER JOIN Users AS USRS ON ACCT.AccountID = USRS.AccountID " +
                          "LEFT OUTER JOIN UserDeviceReads AS UDR ON USRS.UserID = UDR.UserID " +
                          "LEFT OUTER JOIN DeviceInventory AS DI ON UDR.DeviceID = DI.DeviceID " +
                          "WHERE DI.SerialNo = '" + serialNumber + "' " +
                          "ORDER BY ACCT.AccountID ASC";

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable dtParentDateTable = new DataTable();

        dtParentDateTable = new DataTable("Get All Account-Linked Badges");

        // Create the columns for the DataTable.
        dtParentDateTable.Columns.Add("AccountID", Type.GetType("System.Int32"));
        dtParentDateTable.Columns.Add("GDSAccount", Type.GetType("System.String"));
        dtParentDateTable.Columns.Add("CompanyName", Type.GetType("System.String"));
        dtParentDateTable.Columns.Add("FullName", Type.GetType("System.String"));
        dtParentDateTable.Columns.Add("UserID", Type.GetType("System.Int32"));
        dtParentDateTable.Columns.Add("Active", Type.GetType("System.String"));
        dtParentDateTable.Columns.Add("IsRIDUser", Type.GetType("System.Int32"));

        if (dtParentDateTable == null) return;

        while (sqlDataReader.Read())
        {
            DataRow drow = dtParentDateTable.NewRow();

            // Fill row details.
            drow["AccountID"] = sqlDataReader["AccountID"];
            drow["GDSAccount"] = sqlDataReader["GDSAccount"];
            drow["CompanyName"] = sqlDataReader["CompanyName"];
            drow["FullName"] = sqlDataReader["FullName"];
            drow["UserID"] = sqlDataReader["UserID"];
            drow["Active"] = sqlDataReader["Active"];
            drow["IsRIDUser"] = sqlDataReader["IsRIDUser"];

            // Add rows to DataTable.
            dtParentDateTable.Rows.Add(drow);
        }

        if (dtParentDateTable.Rows.Count == 0)
        {
            rgReads_Parent.PageSize = dtParentDateTable.Rows.Count + 1;
        }
        else
        {
            rgReads_Parent.PageSize = dtParentDateTable.Rows.Count;
        }

        rgReads_Parent.DataSource = dtParentDateTable;

        sqlConn.Close();
        sqlDataReader.Close();
    }

    protected void rgReads_Child_NeedDataSource(object source, GridNeedDataSourceEventArgs e)
    {
        GridDataItem parentItem = ((source as RadGrid).NamingContainer as GridNestedViewItem).ParentItem as GridDataItem;

        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.Text;

        // Using the vw_GetAllReadsByAccountID.
        string sqlQuery = "SELECT v.*, a.GDSAccount FROM [vw_GetAllReadsByAccountID] v join Accounts a on a.AccountID = v.AccountID";
        string filters = "";

        if (serialNumber == null) return;

        filters += "(v.[AccountID] = @pAccountID)";
        sqlCmd.Parameters.Add(new SqlParameter("@pAccountID", parentItem.GetDataKeyValue("AccountID").ToString()));

        if (filters != "") filters += " AND ";

        filters += "(v.[UserID] = @pUserID)";
        sqlCmd.Parameters.Add(new SqlParameter("@pUserID", parentItem.GetDataKeyValue("UserID").ToString()));

        if (Session["IncludedReadType"] == null || Session["IncludedReadType"].ToString() == "User Read")
            filters += " AND v.ReadType = 'User Read' ";
        else if (!string.IsNullOrEmpty(Session["IncludedReadType"].ToString()))
            filters += " AND v.ReadType = '" + Session["IncludedReadType"].ToString() + "' ";

        // Add the filters to the query if needed.
        if (filters != "") sqlQuery += " WHERE v.SerialNumber = '" + serialNumber + "' AND" + filters;

        // Add the ORDER BY and DIRECTION.
        sqlQuery += " ORDER BY v.CreatedDate DESC";

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable dtBadgeReadsByAccount = new DataTable("Badge Reads by Account");

        // Create the columns for the DataTable.
        dtBadgeReadsByAccount.Columns.Add("AccountID", Type.GetType("System.Int32"));
        dtBadgeReadsByAccount.Columns.Add("GDSAccount", Type.GetType("System.String"));
        dtBadgeReadsByAccount.Columns.Add("RID", Type.GetType("System.Int32"));
        dtBadgeReadsByAccount.Columns.Add("SerialNumber", Type.GetType("System.String"));
        dtBadgeReadsByAccount.Columns.Add("ReadType", Type.GetType("System.String"));
        dtBadgeReadsByAccount.Columns.Add("CreatedDate", Type.GetType("System.DateTime"));
        dtBadgeReadsByAccount.Columns.Add("InitialRead", Type.GetType("System.String"));
        dtBadgeReadsByAccount.Columns.Add("HasAnomaly", Type.GetType("System.String"));
        dtBadgeReadsByAccount.Columns.Add("Error", Type.GetType("System.String"));
        dtBadgeReadsByAccount.Columns.Add("DL", Type.GetType("System.Int32"));
        dtBadgeReadsByAccount.Columns.Add("DH", Type.GetType("System.Int32"));
        dtBadgeReadsByAccount.Columns.Add("DLT", Type.GetType("System.Int32"));
        dtBadgeReadsByAccount.Columns.Add("DHT", Type.GetType("System.Int32"));
        dtBadgeReadsByAccount.Columns.Add("DLTMinusDHT", Type.GetType("System.Int32"));
        dtBadgeReadsByAccount.Columns.Add("DLDCalc", Type.GetType("System.Int32"));
        dtBadgeReadsByAccount.Columns.Add("BkgdExp", Type.GetType("System.Decimal"));
        dtBadgeReadsByAccount.Columns.Add("CumulDose", Type.GetType("System.Int32"));
        dtBadgeReadsByAccount.Columns.Add("DLDose", Type.GetType("System.Int32"));
        dtBadgeReadsByAccount.Columns.Add("DeepLowCumulativeDose", Type.GetType("System.Int32"));

        var productGroupID = Convert.ToInt32(ViewState["ProductGroupID"]);

        while (sqlDataReader.Read())
        {

            DataRow drow = dtBadgeReadsByAccount.NewRow();

            // Fill row details.
            drow["AccountID"] = sqlDataReader["AccountID"];
            drow["GDSAccount"] = sqlDataReader["GDSAccount"];
            drow["RID"] = sqlDataReader["RID"];
            drow["SerialNumber"] = sqlDataReader["SerialNumber"];
            drow["ReadType"] = sqlDataReader["ReadType"];
            drow["CreatedDate"] = sqlDataReader["CreatedDate"];
            drow["InitialRead"] = sqlDataReader["InitialRead"];
            drow["HasAnomaly"] = sqlDataReader["HasAnomaly"];
            drow["Error"] = sqlDataReader["Error"];
            drow["DL"] = sqlDataReader["DL"];
            drow["DLT"] = sqlDataReader["DLT"];
            drow["DH"] = sqlDataReader["DH"];
            drow["DHT"] = sqlDataReader["DHT"];
            drow["DLTMinusDHT"] = sqlDataReader["DLTMinusDHT"];
            drow["DLDCalc"] = sqlDataReader["DLDCalc"];
            drow["BkgdExp"] = sqlDataReader["BkgdExp"];

            if (productGroupID == ProductGroupIDConstants.Instadose2New)
                drow["CumulDose"] = sqlDataReader["DeepLowCumulativeDose"];
            else
                drow["CumulDose"] = sqlDataReader["CumulDose"];

            drow["DLDose"] = sqlDataReader["DLDose"];

            // Add rows to DataTable.
            dtBadgeReadsByAccount.Rows.Add(drow);
        }

        if (dtBadgeReadsByAccount.Rows.Count == 0)
        {
            (source as RadGrid).PageSize = dtBadgeReadsByAccount.Rows.Count + 1;
        }
        else
        {
            (source as RadGrid).PageSize = dtBadgeReadsByAccount.Rows.Count;
        }

        (source as RadGrid).DataSource = dtBadgeReadsByAccount;
    }

    protected void rgReads_Child_PreRender(object sender, EventArgs e)
    {
        GridColumn column = (sender as RadGrid).MasterTableView.GetColumnSafe("ReadType");
        column.CurrentFilterFunction = GridKnownFunction.EqualTo;
        column.CurrentFilterValue = "User Read";
    }

    protected void rgReads_Child_IDVue_PreRender(object sender, EventArgs e)
    {
        GridColumn column = (sender as RadGrid).MasterTableView.GetColumnSafe("ReadType");
        column.CurrentFilterFunction = GridKnownFunction.EqualTo;
        column.CurrentFilterValue = "User Read";
    }
    protected void rgReads_Child_IDVue_NeedDataSource(object source, GridNeedDataSourceEventArgs e)
    {
        GridDataItem parentItem = ((source as RadGrid).NamingContainer as GridNestedViewItem).ParentItem as GridDataItem;

        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.Text;

        // Using the vw_GetAllReadsByAccountID.
        string sqlQuery = "SELECT udr.*, RT.ReadTypeName AS [ReadType], a.GDSAccount " +
            "FROM [vw_UserDeviceReads] udr " +
            "LEFT OUTER JOIN ReadTypes AS RT ON UDR.ReadTypeID = RT.ReadTypeID " +
            "INNER JOIN Accounts a on a.AccountID = UDR.AccountID";
        string filters = "";

        if (serialNumber == null) return;

        filters += "(udr.[AccountID] = @pAccountID)";
        sqlCmd.Parameters.Add(new SqlParameter("@pAccountID", parentItem.GetDataKeyValue("AccountID").ToString()));

        if (filters != "") filters += " AND ";

        filters += "(udr.[UserID] = @pUserID)";

        if (Session["IncludedReadType"] == null || Session["IncludedReadType"].ToString() == "User Read")
            filters += " AND RT.ReadTypeName = 'User Read'";
        else if (!string.IsNullOrEmpty(Session["IncludedReadType"].ToString()))
            filters += " AND RT.ReadTypeName = '" + Session["IncludedReadType"].ToString() + "' ";

        sqlCmd.Parameters.Add(new SqlParameter("@pUserID", parentItem.GetDataKeyValue("UserID").ToString()));

        // Add the filters to the query if needed.
        if (filters != "") sqlQuery += " WHERE udr.SerialNo = '" + serialNumber + "' AND" + filters;

        // Add the ORDER BY and DIRECTION.
        sqlQuery += " ORDER BY udr.CreatedDate DESC";

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable dtBadgeReadsByAccount = new DataTable("Badge Reads by Account");

        // Create the columns for the DataTable.
        dtBadgeReadsByAccount.Columns.Add("AccountID", Type.GetType("System.Int32"));
        dtBadgeReadsByAccount.Columns.Add("GDSAccount", Type.GetType("System.String"));
        dtBadgeReadsByAccount.Columns.Add("RID", Type.GetType("System.Int32"));
        dtBadgeReadsByAccount.Columns.Add("ReadTypeID", Type.GetType("System.Int32"));
        dtBadgeReadsByAccount.Columns.Add("ReadType", Type.GetType("System.String"));
        dtBadgeReadsByAccount.Columns.Add("CreatedDate", Type.GetType("System.DateTime"));
        dtBadgeReadsByAccount.Columns.Add("BatteryPercent", Type.GetType("System.Int32"));
        dtBadgeReadsByAccount.Columns.Add("DoseBatteryPercent", Type.GetType("System.Int32"));
        dtBadgeReadsByAccount.Columns.Add("InitRead", Type.GetType("System.String"));
        dtBadgeReadsByAccount.Columns.Add("HasAnomaly", Type.GetType("System.String"));
        dtBadgeReadsByAccount.Columns.Add("ErrorText", Type.GetType("System.String"));
        dtBadgeReadsByAccount.Columns.Add("DeepLowDose", Type.GetType("System.Decimal"));
        dtBadgeReadsByAccount.Columns.Add("CumulativeDose", Type.GetType("System.Decimal"));
        dtBadgeReadsByAccount.Columns.Add("DL_Rm", Type.GetType("System.Int32"));
        dtBadgeReadsByAccount.Columns.Add("DL_Tm", Type.GetType("System.Int32"));
        dtBadgeReadsByAccount.Columns.Add("DL_Tc", Type.GetType("System.Int32"));
        dtBadgeReadsByAccount.Columns.Add("DL_DAC", Type.GetType("System.Int16"));
        dtBadgeReadsByAccount.Columns.Add("TA_Pre", Type.GetType("System.Int32"));
        dtBadgeReadsByAccount.Columns.Add("TA_Post", Type.GetType("System.Int32"));

        dtBadgeReadsByAccount.Columns.Add("DeepHighDose", Type.GetType("System.Decimal"));
        dtBadgeReadsByAccount.Columns.Add("DH_DAC", Type.GetType("System.Int16"));
        dtBadgeReadsByAccount.Columns.Add("DH_Rm", Type.GetType("System.Int32"));
        dtBadgeReadsByAccount.Columns.Add("DH_Tm", Type.GetType("System.Int32"));
        dtBadgeReadsByAccount.Columns.Add("DH_Tc", Type.GetType("System.Int32"));
        dtBadgeReadsByAccount.Columns.Add("DeepLowDoseCalc", Type.GetType("System.Decimal"));
        dtBadgeReadsByAccount.Columns.Add("BackgroundExposure", Type.GetType("System.Decimal"));

        while (sqlDataReader.Read())
        {
            DataRow drow = dtBadgeReadsByAccount.NewRow();

            // Fill row details.
            drow["AccountID"] = sqlDataReader["AccountID"];
            drow["GDSAccount"] = sqlDataReader["GDSAccount"];
            drow["RID"] = sqlDataReader["RID"];
            drow["ReadTypeID"] = sqlDataReader["ReadTypeID"];
            drow["ReadType"] = sqlDataReader["ReadType"];
            drow["CreatedDate"] = sqlDataReader["CreatedDate"];
            drow["BatteryPercent"] = sqlDataReader["BatteryPercent"];
            drow["DoseBatteryPercent"] = sqlDataReader["DoseBatteryPercent"];
            drow["InitRead"] = Convert.ToBoolean(sqlDataReader["InitRead"]) ? "Yes" : "No";
            drow["HasAnomaly"] = Convert.ToBoolean(sqlDataReader["HasAnomaly"]) ? "Yes" : "No";
            drow["ErrorText"] = sqlDataReader["ErrorText"];
            drow["DeepLowDose"] = sqlDataReader["DeepLowDose"];
            drow["CumulativeDose"] = sqlDataReader["CumulativeDose"];
            drow["DL_Rm"] = sqlDataReader["DL_Rm"];
            drow["DL_Tm"] = sqlDataReader["DL_Tm"];
            drow["DL_Tc"] = sqlDataReader["DL_Tc"];
            drow["DL_DAC"] = sqlDataReader["DL_DAC"];
            drow["TA_Pre"] = sqlDataReader["TA_Pre"];
            drow["TA_Post"] = sqlDataReader["TA_Post"];

            drow["DeepHighDose"] = sqlDataReader["DeepHighDose"];
            drow["DH_DAC"] = sqlDataReader["DH_DAC"];
            drow["DH_Rm"] = sqlDataReader["DH_Rm"];
            drow["DH_Tm"] = sqlDataReader["DH_Tm"];
            drow["DH_Tc"] = sqlDataReader["DH_Tc"];
            drow["DeepLowDoseCalc"] = sqlDataReader["DeepLowDoseCalc"];
            drow["BackgroundExposure"] = sqlDataReader["BackgroundExposure"];
            // Add rows to DataTable.
            dtBadgeReadsByAccount.Rows.Add(drow);
        }

        if (dtBadgeReadsByAccount.Rows.Count == 0)
        {
            (source as RadGrid).PageSize = dtBadgeReadsByAccount.Rows.Count + 1;
        }
        else
        {
            (source as RadGrid).PageSize = dtBadgeReadsByAccount.Rows.Count;
        }

        (source as RadGrid).DataSource = dtBadgeReadsByAccount;
    }

    protected void rgReads_Parent_ItemCommand(object source, GridCommandEventArgs e)
    {
        bool isIDVue = Convert.ToBoolean(ViewState["IsIDVue"]);
        if (e.CommandName == RadGrid.ExpandCollapseCommandName && !e.Item.Expanded)
        {
            GridDataItem parentItem = e.Item as GridDataItem;
            var childGrid = isIDVue ? "rgReads_Child_IDVue" : "rgReads_Child";

            RadGrid rg = parentItem.ChildItem.FindControl(childGrid) as RadGrid;
            rg.Rebind();
        }

        if (e.CommandName == RadGrid.ExpandCollapseCommandName && e.Item is GridDataItem)
        {
            var innerPanel = isIDVue ? "pnlInnerDataContainerIDVue" : "pnlInnerDataContainer";

            ((GridDataItem)e.Item).ChildItem.FindControl(innerPanel).Visible = !e.Item.Expanded;
        }
    }

    protected void rgReads_Child_ItemDataBound(object sender, GridItemEventArgs e)
    {
        int getRID = 0;

        if (e.Item is GridDataItem)
        {
            // Highlights the Current Read to be Reviewed in Yellow.
            // Gives User the visual to see which RID/Read Record has been Analyzed and is in Review currently.
            GridDataItem currentRead = (GridDataItem)e.Item;
            TableCell currentRID = currentRead["RID"];
            Int32.TryParse(currentRID.Text, out getRID);

            if (getRID != 0 && getRID == Convert.ToInt32(Request.QueryString["RID"]))
            {
                currentRead.BackColor = System.Drawing.Color.Yellow;
            }

            GridDataItem childDataItem = (GridDataItem)e.Item;
            // Create TableCell object for the following:
            //  1.) User Reads (blue)
            //  2.) Anomalies (red)
            //  3.) Initializations (black)
            TableCell goodCustomerReadsCell = childDataItem["ReadType"];
            TableCell anomalyCell = childDataItem["HasAnomaly"];
            TableCell initializationCell = childDataItem["InitialRead"];

            // CUSTOMER READS.
            if (goodCustomerReadsCell.Text.Contains("User Read") && anomalyCell.Text == "No" && initializationCell.Text == "No")
            {
                childDataItem.ForeColor = System.Drawing.Color.Blue;
            }

            // Anomalies
            if ((anomalyCell.Text == "Yes"))
            {
                childDataItem.ForeColor = System.Drawing.Color.Red;
            }

            // Initializations
            if ((initializationCell.Text == "Yes" && anomalyCell.Text == "No"))
            {
                childDataItem.ForeColor = System.Drawing.Color.Black;
            }

            // Adjustments, Estimates, Lifetime, YTD, and QTD
            if ((goodCustomerReadsCell.Text == "Adjustment" || goodCustomerReadsCell.Text == "Estimate" ||
                 goodCustomerReadsCell.Text == "Lifetime" || goodCustomerReadsCell.Text == "Year to Date" ||
                 goodCustomerReadsCell.Text == "Quarter to Date") && anomalyCell.Text == "No" && initializationCell.Text == "No")
            {
                childDataItem.ForeColor = System.Drawing.Color.Green;
            }

            // Unknown
            if ((goodCustomerReadsCell.Text == "Unknown" && initializationCell.Text == "No" && anomalyCell.Text == "No"))
            {
                childDataItem.ForeColor = System.Drawing.Color.Gray;
            }

        }
    }

    protected void rgReads_Child_IDVue_ItemDataBound(object sender, GridItemEventArgs e)
    {
        int getRID = 0;

        if (e.Item is GridDataItem)
        {
            // Highlights the Current Read to be Reviewed in Yellow.
            // Gives User the visual to see which RID/Read Record has been Analyzed and is in Review currently.
            GridDataItem currentRead = (GridDataItem)e.Item;
            TableCell currentRID = currentRead["RID"];
            Int32.TryParse(currentRID.Text, out getRID);

            if (getRID != 0 && getRID == Convert.ToInt32(Request.QueryString["RID"]))
            {
                currentRead.BackColor = System.Drawing.Color.Yellow;
            }

            GridDataItem childDataItem = (GridDataItem)e.Item;
            // Create TableCell object for the following:
            //  1.) User Reads (blue)
            //  2.) Anomalies (red)
            //  3.) Initializations (black)
            TableCell goodCustomerReadsCell = childDataItem["ReadType"];
            TableCell anomalyCell = childDataItem["HasAnomaly"];
            TableCell initializationCell = childDataItem["InitialRead"];

            // CUSTOMER READS. 17 = User Read
            if (goodCustomerReadsCell.Text.Contains("User Read") && anomalyCell.Text == "No" && initializationCell.Text == "No")
            {
                childDataItem.ForeColor = System.Drawing.Color.Blue;
            }

            // Anomalies
            if ((anomalyCell.Text == "Yes"))
            {
                childDataItem.ForeColor = System.Drawing.Color.Red;
            }

            // Initializations
            if ((initializationCell.Text == "Yes" && anomalyCell.Text == "No"))
            {
                childDataItem.ForeColor = System.Drawing.Color.Black;
            }

            // Adjustments, Estimates, Lifetime, YTD, and QTD
            if ((goodCustomerReadsCell.Text == "Adjustment" || goodCustomerReadsCell.Text == "Estimate" ||
                 goodCustomerReadsCell.Text == "Lifetime" || goodCustomerReadsCell.Text == "Year to Date" ||
                 goodCustomerReadsCell.Text == "Quarter to Date") && anomalyCell.Text == "No" && initializationCell.Text == "No")
            {
                childDataItem.ForeColor = System.Drawing.Color.Green;
            }

            // Unknown
            if ((goodCustomerReadsCell.Text == "Unknown" && initializationCell.Text == "No" && anomalyCell.Text == "No"))
            {
                childDataItem.ForeColor = System.Drawing.Color.Gray;
            }
        }
    }

    protected void rgReads_Parent_ItemCreated(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridNestedViewItem && !IsPostBack)
        {
            bool isIDVue = Convert.ToBoolean(ViewState["IsIDVue"]);
            var innerPanel = isIDVue ? "pnlInnerDataContainerIDVue" : "pnlInnerDataContainer";
            e.Item.FindControl(innerPanel).Visible = true;
            rgReads_Parent.MasterTableView.Items[0].Expanded = false;
        }
    }

    protected void rgReads_Parent_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            // Highlights the Current Read to be Reviewed in Yellow.
            // Gives User the visual to see which RID/Read Record has been Analyzed and is in Review currently.
            GridDataItem currentRead = (GridDataItem)e.Item;
            TableCell tcIsRIDUser = currentRead["IsRIDUser"];
            int isRIDUser;
            Int32.TryParse(tcIsRIDUser.Text, out isRIDUser);

            if (isRIDUser == 1)
            {
                TableCell tcUserID = currentRead["FullName"];
                tcUserID.Font.Bold = true;
            }
        }
    }
    // -------------------- END :: DOSE READS TAB FUNCTIONS -------------------- //
    #endregion

    #region EXCEPTIONS TAB
    protected void rgExceptions_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
    {
        if (string.IsNullOrEmpty(serialNumber))
            return;

        (sender as RadGrid).DataSource = GetDeviceExceptions(serialNumber);
    }

    //protected void rgExceptions_OnItemDataBound(object sender, GridItemEventArgs e)
    //{
    //    if (e.Item is GridDataItem)
    //    {
    //        GridDataItem dataItem = (GridDataItem)e.Item;
    //        Int32.TryParse(DataBinder.Eval(dataItem.DataItem, "Code").ToString(), out int exceptionCode);

    //        if (exceptionCode == 20)// High Temp
    //        {
    //            e.Item.BackColor = System.Drawing.Color.LightPink;
    //        }

    //    }        
    //}

    private DataTable GetDeviceExceptions(string serialNumber)
    {
        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.Text;

        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();

        var query = "SELECT SerialNo,ExceptionDate,CreatedDate,Code" +
            ", case when Code in (3, 4, 5, 6) then ((AdditionalInfo/100) - 273.15)*9/5 + 32" +
            "   else AdditionalInfo end AdditionalInfo " +
            ",ExceptionDesc,InfoDesc" +
            " FROM [dbo].[vw_ID3Exceptions] e  WHERE SerialNo = {0} order by ExceptionDate desc";

        string sqlQuery = string.Format(query, serialNumber);
        sqlCmd.CommandText = sqlQuery;

        // Create a Date Table to hold the contents/results.
        DataTable dt = new DataTable("Get Device Exceptions");
        dt.Load(sqlCmd.ExecuteReader());

        return dt;
    }
    #endregion

    #region MOTIONS TAB
    protected void rgDailyMotions_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
    {
        if (string.IsNullOrEmpty(serialNumber))
            return;

        (sender as RadGrid).DataSource = GetDeviceDailyMotions(serialNumber);
    }

    protected void rgDailyMotions_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem dataItem = (GridDataItem)e.Item;

            for (int i = 0; i < 24; i++)
            {
                string fieldName = "Hour" + i;

                // Get the Hour0 value
                bool hourValue = Convert.ToBoolean(DataBinder.Eval(dataItem.DataItem, fieldName));

                // Find the cell corresponding to the Hour0 column (adjust index if needed)
                TableCell hourCell = dataItem[fieldName];  // Assuming the column's UniqueName is "Hour0"

                // Set the BackColor based on the value
                if (hourValue)
                {
                    hourCell.BackColor = System.Drawing.Color.LightPink;
                }
                else
                {
                    hourCell.BackColor = System.Drawing.Color.Transparent;
                }
            }

            
        }
    }


    private DataTable GetDeviceDailyMotions(string serialNumber)
    {
        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.Text;

        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();

        string sqlQuery = string.Format("SELECT * FROM [dbo].[ID3_DailyMotion] where SerialNo = {0}  order by MotionDetectedDate desc", serialNumber);
        sqlCmd.CommandText = sqlQuery;

        // Create a Date Table to hold the contents/results.
        DataTable dt = new DataTable("Get Device Exceptions");
        dt.Load(sqlCmd.ExecuteReader());

        return dt;
    }
    #endregion

    #region GRAPHS TAB FUNCTIONS.
    // -------------------- BEGIN :: ROUGH & DOSE RANGE(s) TAB FUNCTIONS -------------------- //
    // DOSE RANGE.
    private void getDoseRangeChartData(string serialNumber, int rID, DateTime startDate, DateTime endDate, int option, bool includeSoftReads)
    {
        // Query the Stored Procedure.
        DataLayer dlLayer_Dose = new DataLayer();

        // Create List of all Parameters.
        List<SqlParameter> parameters = new List<SqlParameter>();
        parameters.Add(new SqlParameter("@pSerialNumber", serialNumber));
        parameters.Add(new SqlParameter("@pRID", rID));
        parameters.Add(new SqlParameter("@pStartDate", startDate));
        parameters.Add(new SqlParameter("@pEndDate", endDate));
        parameters.Add(new SqlParameter("@pOption", option));
        parameters.Add(new SqlParameter("@pIncludeSoftReads", includeSoftReads));

        // Get the DataSet.
        DataSet dsResult_Dose = dlLayer_Dose.GetDataSet("sp_GetDoseRangeByExposureDate", CommandType.StoredProcedure, parameters);

        // Get the 1st Table.
        DataTable dtDoseRange = dsResult_Dose.Tables[0];

        // If no data was returned exit.
        if (dsResult_Dose.Tables.Count == 0) return;

        // Create ExposureDates, Series 1 and Series 2 Lists.
        var chartData1 = new List<ChartData<string, float>>();
        var chartData2 = new List<ChartData<string, float>>();
        foreach (DataRow row in dtDoseRange.Rows)
        {
            float dl = 0;
            float dh = 0;
            if (row["DL"] != DBNull.Value)
                dl = float.Parse(row["DL"].ToString());

            if (row["DH"] != DBNull.Value)
                dh = float.Parse(row["DH"].ToString());

            DateTime dtExposureDate = DateTime.Parse(row["ExposureDate"].ToString());

            chartData1.Add(new ChartData<string, float>(dtExposureDate.ToString("yyyy-MM-dd HH:mm"), dl));
            chartData2.Add(new ChartData<string, float>(dtExposureDate.ToString("yyyy-MM-dd HH:mm"), dh));
        }

        JavaScriptSerializer jss = new JavaScriptSerializer();
        // Write the high chart data to the screen content
        Response.Clear();
        Response.ContentType = "application/json; charset=utf-8";
        Response.Write(jss.Serialize(new { dl = chartData1, dh = chartData2 }));
        Response.End();
    }

    // ROUGH DOSE RANGE.
    private void getRoughDoseChartData(string serialNumber, int rID, DateTime startDate, DateTime endDate, int option, bool includeSoftReads)
    {
        // Query the Stored Procedure.
        DataLayer dlLayer_RoughDose = new DataLayer();

        // Create List of all Parameters.
        List<SqlParameter> parameters = new List<SqlParameter>();
        parameters.Add(new SqlParameter("@pSerialNo", serialNumber));
        parameters.Add(new SqlParameter("@pRID", rID));
        parameters.Add(new SqlParameter("@pStartDate", startDate));
        parameters.Add(new SqlParameter("@pEndDate", endDate));
        parameters.Add(new SqlParameter("@pOption", option));
        parameters.Add(new SqlParameter("@pIncludeSoftReads", includeSoftReads));

        // Get the DataSet.
        DataSet dsResult_RoughDose = dlLayer_RoughDose.GetDataSet("sp_BadgeReview", CommandType.StoredProcedure, parameters);

        // Get the 1st Table.
        DataTable dtRoughDoseRange = dsResult_RoughDose.Tables[0];

        // If no data was returned exit.
        if (dsResult_RoughDose.Tables.Count == 0) return;

        // Create ExposureDates, Series 1 and Series 2 Lists.
        var chartData1 = new List<ChartData<string, float>>();
        var chartData2 = new List<ChartData<string, float>>();
        foreach (DataRow row in dtRoughDoseRange.Rows)
        {
            DateTime dtExposureDate = DateTime.Parse(row["ExposureDate"].ToString());

            chartData1.Add(new ChartData<string, float>(dtExposureDate.ToString("yyyy-MM-dd HH:mm"), float.Parse(row["DLRoughDose"].ToString())));
            chartData2.Add(new ChartData<string, float>(dtExposureDate.ToString("yyyy-MM-dd HH:mm"), float.Parse(row["DHRoughDose"].ToString())));
        }

        JavaScriptSerializer jss = new JavaScriptSerializer();
        // Write the high chart data to the screen content
        Response.Clear();
        Response.ContentType = "application/json; charset=utf-8";
        Response.Write(jss.Serialize(new { dl = chartData1, dh = chartData2 }));
        Response.End();
    }
    // -------------------- END :: ROUGH & DOSE RANGE(s) TAB FUNCTIONS -------------------- //

    // -------------------- BEGIN :: TEMPERATURE RANGE & DIFFERENCES TAB FUNCTIONS -------------------- //
    private void getTemperatureRangeChartData(string serialNumber, int rID, DateTime startDate, DateTime endDate, int option, bool includeSoftReads)
    {
        // Query the Stored Procedure.
        DataLayer dlLayer_Temp = new DataLayer();

        // Create List of all Parameters.
        List<SqlParameter> parameters = new List<SqlParameter>();
        parameters.Add(new SqlParameter("@pSerialNumber", serialNumber));
        parameters.Add(new SqlParameter("@pRID", rID));
        parameters.Add(new SqlParameter("@pStartDate", startDate));
        parameters.Add(new SqlParameter("@pEndDate", endDate));
        parameters.Add(new SqlParameter("@pOption", option));
        parameters.Add(new SqlParameter("@pIncludeSoftReads", includeSoftReads));

        // Get the DataSet.
        DataSet dsResult_Temp = dlLayer_Temp.GetDataSet("sp_GetTemperatureRangeByExposureDate", CommandType.StoredProcedure, parameters);

        // Get the 1st Table.
        DataTable dtTempRange = dsResult_Temp.Tables[0];

        var productGroupID = Convert.ToInt32(ViewState["ProductGroupID"]);

        // If no data was returned exit.
        if (dsResult_Temp.Tables.Count == 0) return;

        var chartData1 = new List<ChartData<string, float>>();
        var chartData2 = new List<ChartData<string, float>>();
        var chartData3 = new List<ChartData<string, float>>();
        var chartData4 = new List<ChartData<string, float>>();

        foreach (DataRow row in dtTempRange.Rows)
        {
            DateTime dtExposureDate = DateTime.Parse(row["ExposureDate"].ToString());

            var dlt = float.Parse(row["DLT"].ToString());
            var dltBaseLine = float.Parse(row["DLTBaseline"].ToString());

            //var dlTemp = GetTemperatureValue(float.Parse(row["DLT"].ToString()), float.Parse(row["DLTBaseline"].ToString()));
            //var dhTemp = GetTemperatureValue(float.Parse(row["DHT"].ToString()), float.Parse(row["DHTBaseline"].ToString()));

            chartData1.Add(new ChartData<string, float>(dtExposureDate.ToString("yyyy-MM-dd HH:mm"), GetTemperatureValue(dlt, dltBaseLine, productGroupID)));
        }

        JavaScriptSerializer jss = new JavaScriptSerializer();
        // Write the high chart data to the screen content
        Response.Clear();
        Response.ContentType = "application/json; charset=utf-8";
        Response.Write(jss.Serialize(new { 
            dlt = chartData1
        }));
        Response.End();
    }

    private float GetTemperatureValue(float temp, float temp2, int productGroupID)
    {
        var result = temp;
        //var result = ((temp) / -165 + 22) * 9 / 5 + 32;
        if (productGroupID == ProductGroupIDConstants.InstadosePlus)
            result = (((temp - temp2) / -165 + 22) * 9 / 5 + 32);
        else
            result = ((temp - 31730) / -165 + 22) * 9 / 5 + 32;

        return result;
    }

    private void getIDVueTemperatureRangeChartData(string serialNumber, DateTime startDate, DateTime endDate)
    {
        // Query the Stored Procedure.
        DataLayer dlLayer_Temp = new DataLayer();

        // Create List of all Parameters.
        List<SqlParameter> parameters = new List<SqlParameter>();
        parameters.Add(new SqlParameter("@pSerialNumber", serialNumber));
        parameters.Add(new SqlParameter("@pStartDate", startDate));
        parameters.Add(new SqlParameter("@pEndDate", endDate));

        // Get the DataSet.
        DataSet dsResult_Temp = dlLayer_Temp.GetDataSet("sp_GetIDVueTemperatureRangeByExposureDate", CommandType.StoredProcedure, parameters);

        // Get the 1st Table.
        DataTable dtTempRange = dsResult_Temp.Tables[0];

        // If no data was returned exit.
        if (dsResult_Temp.Tables.Count == 0) return;

        var chartData = new List<ChartData<string, float>>();

        foreach (DataRow row in dtTempRange.Rows)
        {
            DateTime dtExposureDate = DateTime.Parse(row["ExposureDate"].ToString());

            chartData.Add(new ChartData<string, float>(dtExposureDate.ToString("yyyy-MM-dd HH:mm"), float.Parse(row["FarenHeightTemp"].ToString())));
        }
       
        JavaScriptSerializer jss = new JavaScriptSerializer();
        // Write the high chart data to the screen content
        Response.Clear();
        Response.ContentType = "application/json; charset=utf-8";
        Response.Write(jss.Serialize(chartData));
        Response.End();
    }

    private void getTemperatureDifferenceChartData(string serialNumber, int rID, DateTime startDate, DateTime endDate, int option, bool includeSoftReads)
    {
        // Query the Stored Procedure.
        DataLayer dlLayer_TempDifference = new DataLayer();

        // Create List of all Parameters.
        List<SqlParameter> parameters = new List<SqlParameter>();
        parameters.Add(new SqlParameter("@pSerialNo", serialNumber));
        parameters.Add(new SqlParameter("@pRID", rID));
        parameters.Add(new SqlParameter("@pStartDate", startDate));
        parameters.Add(new SqlParameter("@pEndDate", endDate));
        parameters.Add(new SqlParameter("@pOption", option));
        parameters.Add(new SqlParameter("@pIncludeSoftReads", includeSoftReads));

        // Get the DataSet.
        DataSet dsResult_TempDifference = dlLayer_TempDifference.GetDataSet("sp_BadgeReview", CommandType.StoredProcedure, parameters);

        // Get the 1st Table.
        DataTable dtTempDifference = dsResult_TempDifference.Tables[0];

        // If no data was returned exit.
        if (dsResult_TempDifference.Tables.Count == 0) return;

        // Creating the data set for high charts
        var chartData = new List<ChartData<string, float>>();
        foreach (DataRow row in dtTempDifference.Rows)
        {
            DateTime dtExposureDate = DateTime.Parse(row["ExposureDate"].ToString());

            chartData.Add(new ChartData<string, float>(dtExposureDate.ToString("yyyy-MM-dd HH:mm"), float.Parse(row["TempDiff"].ToString())));
        }

        JavaScriptSerializer jss = new JavaScriptSerializer();
        // Write the high chart data to the screen content
        Response.Clear();
        Response.ContentType = "application/json; charset=utf-8";
        Response.Write(jss.Serialize(chartData));
        Response.End();
    }
    // -------------------- END :: TEMPERATURE RANGE & DIFFERENCES TAB FUNCTIONS -------------------- //

    // -------------------- BEGIN :: DLT-DHT TAB FUNCTIONS -------------------- //
    private void getDLTMinusDHTChartData(string serialNumber, int rID, DateTime startDate, DateTime endDate, bool includeSoftReads)
    {
        // Query the Stored Procedure.
        DataLayer dlLayer_DLTMinusDHT = new DataLayer();

        // Create List of all Parameters.
        List<SqlParameter> parameters = new List<SqlParameter>();
        parameters.Add(new SqlParameter("@pSerialNumber", serialNumber));
        parameters.Add(new SqlParameter("@pRID", rID));
        parameters.Add(new SqlParameter("@pStartDate", startDate));
        parameters.Add(new SqlParameter("@pEndDate", endDate));
        parameters.Add(new SqlParameter("@pOption", option));
        parameters.Add(new SqlParameter("@pIncludeSoftReads", includeSoftReads));

        // Get the DataSet.
        DataSet dsResult_DLTMinusDHT = dlLayer_DLTMinusDHT.GetDataSet("sp_GetDLTMinusDHTByExposureDate", CommandType.StoredProcedure, parameters);

        // Get the 1st Table.
        DataTable dtDLTMinusDHT = dsResult_DLTMinusDHT.Tables[0];

        // If no data was returned exit.
        if (dsResult_DLTMinusDHT.Tables.Count == 0) return;

        var chartData = new List<ChartData<string, float>>();
        foreach (DataRow row in dtDLTMinusDHT.Rows)
        {
            DateTime dtExposureDate = DateTime.Parse(row["ExposureDate"].ToString());

            chartData.Add(new ChartData<string, float>(dtExposureDate.ToString("yyyy-MM-dd HH:mm"), float.Parse(row["DLTMinusDHT"].ToString())));
        }

        JavaScriptSerializer jss = new JavaScriptSerializer();
        // Write the high chart data to the screen content
        Response.Clear();
        Response.ContentType = "application/json; charset=utf-8";
        Response.Write(jss.Serialize(chartData));
        Response.End();
    }
    // -------------------- END :: DLT-DHT TAB FUNCTIONS -------------------- //

    // -------------------- BEGIN :: CUMULATIVE DOSE TAB FUNCTIONS -------------------- //
    private void getCumulativeDoseChartData(string serialNumber, int rID, DateTime startDate, DateTime endDate, bool includeSoftReads)
    {
        // Query the Stored Procedure.
        DataLayer dlLayer_CumulativeDose = new DataLayer();

        // Create List of all Parameters.
        List<SqlParameter> parameters = new List<SqlParameter>();
        parameters.Add(new SqlParameter("@pSerialNumber", serialNumber));
        parameters.Add(new SqlParameter("@pRID", rID));
        parameters.Add(new SqlParameter("@pStartDate", startDate));
        parameters.Add(new SqlParameter("@pEndDate", endDate));
        parameters.Add(new SqlParameter("@pOption", option));
        parameters.Add(new SqlParameter("@pIncludeSoftReads", includeSoftReads));

        // Get the DataSet.
        DataSet dsResult_CumulativeDose = dlLayer_CumulativeDose.GetDataSet("sp_GetCumulativeDoseByExposureDate", CommandType.StoredProcedure, parameters);

        // Get the 1st Table.
        DataTable dtCumulativeDose = dsResult_CumulativeDose.Tables[0];

        // If no data was returned exit.
        if (dsResult_CumulativeDose.Tables.Count == 0) return;

        // Creating the data set for high charts        

        // Create ExposureDates, Series 1 List.

        bool isIDVue = Convert.ToBoolean(ViewState["IsIDVue"]);

        var chartData1 = new List<ChartData<string, float>>();
        var chartData2 = new List<ChartData<string, float>>();

        foreach (DataRow row in dtCumulativeDose.Rows)
        {
            DateTime dtExposureDate = DateTime.Parse(row["ExposureDate"].ToString());

            if (isIDVue)
            {
                chartData1.Add(new ChartData<string, float>(dtExposureDate.ToString("yyyy-MM-dd HH:mm"), float.Parse(row["CumulativeDose"].ToString())));

                var isSoftRead = int.Parse(row["ReadTypeID"].ToString()) == 21; // For Deep High Commulative Dose, only show Read Type 17
                if (!isSoftRead)
                    chartData2.Add(new ChartData<string, float>(dtExposureDate.ToString("yyyy-MM-dd HH:mm"), float.Parse(row["CumulativeDoseDH"].ToString())));
            }
            else
            {
                chartData1.Add(new ChartData<string, float>(dtExposureDate.ToString("yyyy-MM-dd HH:mm"), float.Parse(row["CumulativeDose"].ToString())));
            }
        }

        JavaScriptSerializer jss = new JavaScriptSerializer();
        // Write the high chart data to the screen content
        Response.Clear();
        Response.ContentType = "application/json; charset=utf-8";
        if (isIDVue)
            Response.Write(jss.Serialize(new { dl = chartData1, dh = chartData2 }));
        else
            Response.Write(jss.Serialize(chartData1));

        Response.End();
    }
    // -------------------- END :: CUMULATIVE DOSE TAB FUNCTIONS -------------------- //

    // -------------------- BEGIN :: DLT-DHT TAB FUNCTIONS -------------------- //
    private void getBatteryPercentChartData(string serialNumber, int rID, DateTime startDate, DateTime endDate, bool includeSoftReads)
    {
        // Query the Stored Procedure.
        DataLayer dlLayer_DLTMinusDHT = new DataLayer();

        // Create List of all Parameters.
        List<SqlParameter> parameters = new List<SqlParameter>();
        parameters.Add(new SqlParameter("@pSerialNumber", serialNumber));
        parameters.Add(new SqlParameter("@pRID", rID));
        parameters.Add(new SqlParameter("@pStartDate", startDate));
        parameters.Add(new SqlParameter("@pEndDate", endDate));
        parameters.Add(new SqlParameter("@pOption", option));
        parameters.Add(new SqlParameter("@pIncludeSoftReads", includeSoftReads));

        // Get the DataSet.
        DataSet dsResult_BatteryPercent = dlLayer_DLTMinusDHT.GetDataSet("sp_GetBatteryPercentData", CommandType.StoredProcedure, parameters);

        // Get the 1st Table.
        DataTable dtBatteryPercent = dsResult_BatteryPercent.Tables[0];

        // If no data was returned exit.
        if (dsResult_BatteryPercent.Tables.Count == 0) return;

        var chartData1 = new List<ChartData<string, float>>();

        foreach (DataRow row in dtBatteryPercent.Rows)
        {
            DateTime dtExposureDate = DateTime.Parse(row["ExposureDate"].ToString());

            chartData1.Add(new ChartData<string, float>(dtExposureDate.ToString("yyyy-MM-dd HH:mm"), float.Parse(row["BatteryPercent"].ToString())));
        }

        JavaScriptSerializer jss = new JavaScriptSerializer();
        // Write the high chart data to the screen content
        Response.Clear();
        Response.ContentType = "application/json; charset=utf-8";
        Response.Write(jss.Serialize(chartData1));
        Response.End();
    }
    // -------------------- END :: DLT-DHT TAB FUNCTIONS -------------------- //
    #endregion

    #region TECHNICAL NOTES TAB FUNCTIONS.
    // -------------------- BEGIN :: TECHNICAL NOTES TAB FUNCTIONS -------------------- //
    protected void rgTechnicalNotes_NeedDataSource(object source, GridNeedDataSourceEventArgs e)
    {
        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.Text;

        string sqlQuery = "SELECT DTN.RID AS [RID], " +
                          "DTN.DeviceID AS [DeviceID], " +
                          "DTN.CreatedDate AS [CreatedDate], " +
                          "DTN.CreatedBy AS [CreatedBy], " +
                          "DTN.NoteText AS [NoteText]" +
                          "FROM DeviceTechnicalNotes AS DTN " +
                          "INNER JOIN DeviceInventory AS DI ON DTN.DeviceID = DI.DeviceID " +
                          "WHERE DI.SerialNo = '" + serialNumber + "'" +
                          "ORDER BY DTN.CreatedDate ASC";

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable dtTechnicalNotesTable = new DataTable();

        dtTechnicalNotesTable = new DataTable("Get All Badge-Linked Technical Notes");

        // Create the columns for the DataTable.
        dtTechnicalNotesTable.Columns.Add("RID", Type.GetType("System.Int32"));
        dtTechnicalNotesTable.Columns.Add("DeviceID", Type.GetType("System.Int32"));
        dtTechnicalNotesTable.Columns.Add("CreatedDate", Type.GetType("System.DateTime"));
        dtTechnicalNotesTable.Columns.Add("CreatedBy", Type.GetType("System.String"));
        dtTechnicalNotesTable.Columns.Add("NoteText", Type.GetType("System.String"));

        while (sqlDataReader.Read())
        {
            DataRow drow = dtTechnicalNotesTable.NewRow();

            // Fill row details.
            drow["RID"] = sqlDataReader["RID"];
            drow["DeviceID"] = sqlDataReader["DeviceID"];
            drow["CreatedDate"] = sqlDataReader["CreatedDate"];
            drow["CreatedBy"] = sqlDataReader["CreatedBy"];
            drow["NoteText"] = sqlDataReader["NoteText"];

            // Add rows to DataTable.
            dtTechnicalNotesTable.Rows.Add(drow);
        }

        // Set the Data Session to RadGrid.
        rgTechnicalNotes.DataSource = dtTechnicalNotesTable;

        sqlConn.Close();
        sqlDataReader.Close();
    }

    protected void rgTechnicalNotes_OnItemCommand(object sender, Telerik.Web.UI.GridCommandEventArgs e)
    {
        rgTechnicalNotes.Rebind();
    }

    protected void lnkbtnCreateNewTechnicalNote_Click(object sender, EventArgs e)
    {
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('divAddTechnicalNoteDialog')", true);
    }

    protected void btnAddTechnicalNote_Click(object sender, EventArgs e)
    {
        // Reset Form & Error Messages.
        InvisibleFormMessage_TechnicalNotes();
        InvisibleErrors_TechnicalNotes();

        int deviceID = (from di in idc.DeviceInventories
                        where di.SerialNo == serialNumber
                        select di.DeviceID).FirstOrDefault();

        // Check to see the value of the RID (QueryString).
        // IF it is 0 then pass in NULL for the RID value to the DB,
        // ELSE pass the QueryString value for the RID.
        int? tempRID = null;
        if (rID == 0) tempRID = null;
        else tempRID = rID;

        if (txtTechnicalNotes.Text != "")
        {
            // Create NEW DeviceTechnicalNotes object.
            DeviceTechnicalNotes btn = new DeviceTechnicalNotes
            {
                RID = tempRID,
                DeviceID = deviceID,
                CreatedDate = DateTime.Now,
                CreatedBy = UserName,
                NoteText = txtTechnicalNotes.Text,
                Active = true
            };

            // Add the NEW object to the DeviceTechnicalNotes collection.
            idc.DeviceTechnicalNotes.InsertOnSubmit(btn);

            // Submit the change to the database.
            try
            {
                idc.SubmitChanges();
                rgTechnicalNotes.Rebind();

                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divAddTechnicalNoteDialog')", true);

                string successMessage = "Technical Note was added successfully!";
                VisibleFormMessage_TechnicalNotes(successMessage);

                // Clear/Reset ASP.NET Controls.
                txtTechnicalNotes.Text = "";
            }
            catch (Exception ex)
            {
                string errorMessage = ex.ToString();
                VisibleErrors_TechnicalNotes(errorMessage);
                return;
            }
        }
        else
        {
            // Display Error Message and Reset controls.
            string errorMessage = "Technical Notes field cannot be blank.";
            VisibleErrors_TechnicalNotes(errorMessage);
            txtTechnicalNotes.Focus();
            return;
        }
    }

    protected void btnCancel_TN_Click(object sender, EventArgs e)
    {
        // Reset Form & Error Messages.
        InvisibleFormMessage_TechnicalNotes();
        InvisibleErrors_TechnicalNotes();

        // Reset Control(s).
        txtTechnicalNotes.Text = string.Empty;

        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divAddTechnicalNoteDialog')", true);
    }

    // Reset Form Message.
    private void InvisibleFormMessage_TechnicalNotes()
    {
        this.spnFormMessage_TechnicalNotes.InnerText = "";
        this.divFormMessage_TechnicalNotes.Visible = false;
    }

    // Set Form Message.
    private void VisibleFormMessage_TechnicalNotes(string error)
    {
        this.spnFormMessage_TechnicalNotes.InnerText = error;
        this.divFormMessage_TechnicalNotes.Visible = true;
    }

    // Reset Error Message.
    private void InvisibleErrors_TechnicalNotes()
    {
        this.spnErrorMessage_TechnicalNotes.InnerText = "";
        this.divErrorMessage_TechnicalNotes.Visible = false;
    }

    // Set Error Message.
    private void VisibleErrors_TechnicalNotes(string error)
    {
        this.spnErrorMessage_TechnicalNotes.InnerText = error;
        this.divErrorMessage_TechnicalNotes.Visible = true;
    }
    // -------------------- END :: TECHNICAL NOTES TAB FUNCTIONS -------------------- //
    #endregion

    #region REVIEW TAB FUNCTIONS.
    // -------------------- BEGIN :: REVIEW TAB FUNCTIONS -------------------- //
    #region BADGE REVEW HISTORY RADGRID FUNCTIONS.
    protected void rgBadgeReviewHistory_NeedDataSource(object source, GridNeedDataSourceEventArgs e)
    {
        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandTimeout = 360;  // 5 minutes
        sqlCmd.CommandType = CommandType.StoredProcedure;

        // Using the vw_CreditCardExpirationsListing.
        string sqlQuery = "sp_BadgeReviewHistory";

        sqlCmd.Parameters.Add(new SqlParameter("@pSerialNumber", serialNumber));

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable dtBadgeReviewHistoryTable = new DataTable();

        dtBadgeReviewHistoryTable = new DataTable("Get Badge Review History");

        // Create the columns for the DataTable.
        dtBadgeReviewHistoryTable.Columns.Add("RID", Type.GetType("System.Int32"));
        dtBadgeReviewHistoryTable.Columns.Add("DeviceID", Type.GetType("System.Int32"));
        dtBadgeReviewHistoryTable.Columns.Add("SerialNumber", Type.GetType("System.String"));
        dtBadgeReviewHistoryTable.Columns.Add("ReadTypeName", Type.GetType("System.String"));
        dtBadgeReviewHistoryTable.Columns.Add("DLTMinusDHT", Type.GetType("System.Int32"));
        dtBadgeReviewHistoryTable.Columns.Add("CreatedDate", Type.GetType("System.DateTime"));
        dtBadgeReviewHistoryTable.Columns.Add("ReviewedDate", Type.GetType("System.DateTime"));
        dtBadgeReviewHistoryTable.Columns.Add("FailedTests", Type.GetType("System.String"));
        dtBadgeReviewHistoryTable.Columns.Add("RecallLimit", Type.GetType("System.Int32"));
        dtBadgeReviewHistoryTable.Columns.Add("WatchlistLowLimit", Type.GetType("System.Int32"));
        dtBadgeReviewHistoryTable.Columns.Add("WatchlistHighLimit", Type.GetType("System.Int32"));
        dtBadgeReviewHistoryTable.Columns.Add("CumulativeDoseLimit", Type.GetType("System.Int32"));
        dtBadgeReviewHistoryTable.Columns.Add("ExpirationYearsLimit", Type.GetType("System.Int32"));
        dtBadgeReviewHistoryTable.Columns.Add("AnalysisActionTypeID", Type.GetType("System.Int32"));
        dtBadgeReviewHistoryTable.Columns.Add("IconURL", Type.GetType("System.String"));
        dtBadgeReviewHistoryTable.Columns.Add("AnalysisActionName", Type.GetType("System.String"));

        while (sqlDataReader.Read())
        {
            DataRow drow = dtBadgeReviewHistoryTable.NewRow();

            // Fill row details.
            drow["RID"] = sqlDataReader["RID"];
            drow["DeviceID"] = sqlDataReader["DeviceID"];
            drow["SerialNumber"] = sqlDataReader["SerialNumber"];
            drow["ReadTypeName"] = sqlDataReader["ReadTypeName"];
            drow["DLTMinusDHT"] = sqlDataReader["DLTMinusDHT"];
            drow["CreatedDate"] = sqlDataReader["CreatedDate"];
            drow["ReviewedDate"] = sqlDataReader["ReviewedDate"];
            drow["FailedTests"] = sqlDataReader["FailedTests"];
            drow["RecallLimit"] = sqlDataReader["RecallLimit"];
            drow["WatchlistLowLimit"] = sqlDataReader["WatchlistLowLimit"];
            drow["WatchlistHighLimit"] = sqlDataReader["WatchlistHighLimit"];
            drow["CumulativeDoseLimit"] = sqlDataReader["CumulativeDoseLimit"];
            drow["ExpirationYearsLimit"] = sqlDataReader["ExpirationYearsLimit"];
            drow["AnalysisActionTypeID"] = sqlDataReader["AnalysisActionTypeID"];
            drow["IconURL"] = sqlDataReader["IconURL"];
            drow["AnalysisActionName"] = sqlDataReader["AnalysisActionName"];

            // Add rows to DataTable.
            dtBadgeReviewHistoryTable.Rows.Add(drow);
        }

        rgBadgeReviewHistory.DataSource = dtBadgeReviewHistoryTable;

        sqlConn.Close();
        sqlDataReader.Close();
    }

    protected void rgBadgeReviewHistory_OnItemCommand(object sender, GridCommandEventArgs e)
    {
        rgBadgeReviewHistory.Rebind();
    }

    protected void rgBadgeReviewHistory_OnItemDataBound(object sender, GridItemEventArgs e)
    {
        int getRID = 0;
        int getAATID = 0;
        serialNumber = Request.QueryString["SerialNo"];

        if (e.Item is GridDataItem)
        {
            // Highlights the Current Read to be Reviewed in Yellow.
            GridDataItem currentRead = (GridDataItem)e.Item;
            TableCell currentRID = currentRead["RID"];
            Int32.TryParse(currentRID.Text, out getRID);

            if (getRID != 0 && getRID == Convert.ToInt32(Request.QueryString["RID"]))
            {
                currentRead.BackColor = System.Drawing.Color.Yellow;
            }

            // For Failed Tests/Flagged Record(s), appropriately color-codes them.
            GridDataItem colorStatus = (GridDataItem)e.Item;
            GridDataItem failedTestsTooltip = (GridDataItem)e.Item;

            TableCell analysisActionTypeID = colorStatus["AnalysisActionTypeID"];
            Int32.TryParse(analysisActionTypeID.Text, out getAATID);

            switch (getAATID)
            {
                case 1: // All Passed
                    colorStatus.ForeColor = System.Drawing.Color.Black;
                    break;
                case 2: // Recall
                    colorStatus.ForeColor = System.Drawing.Color.Red;
                    break;
                case 3: // Watchlist
                    colorStatus.ForeColor = System.Drawing.Color.MediumBlue;
                    break;
                default:
                    break;
            }

            // Tooltip that displays full name of Analysis Types/Tests (if failed).
            failedTestsTooltip["FailedTests"].ToolTip = GetAllFailedTestNames(Convert.ToInt32(failedTestsTooltip["RID"].Text)).ToString();

            // Checks the Status of the Badge's RMA in the process (Mfg.->Tech. Ops.) and displays link text accordingly.
            int rmaCount = 0;
            serialNumber = Request.QueryString["SerialNo"];

            HyperLink hyprlnk = e.Item.FindControl("hyprlnkRecallStatus") as HyperLink;

            if (getAATID == 2)
            {
                GetRMAStatusOfBadge(serialNumber, getRID, out rmaCount);

                hyprlnk.Visible = true;

                // RMA Process has not been initiated,
                if (rmaCount == 0)
                {
                    hyprlnk.Text = "Create RMA";
                }
                // RMA Process has been started and is in some stage between Receiving/Mfg. and Tech. Ops.
                else
                {
                    hyprlnk.Text = "View RMA";
                }
            }
            else
            {
                hyprlnk.Visible = false;
            }
        }
    }

    /// <summary>
    /// Get the Badge's RMA Status ("Create RMA" or "View RMA" will be displayed accordingly).
    /// if it has not been created after reviewing, display "Create RMA". Else displaying "View RMA"
    /// </summary>
    /// <param name="serialnumber"></param>
    /// <param name="mfgcount"></param>
    /// <param name="techopscount"></param>
    private void GetRMAStatusOfBadge(string serialnumber, int rid, out int rmaCount)
    {
        var udrAccountIDAndReviewDate = (from udr in idc.UserDeviceReads
                                         where udr.DeviceInventory.SerialNo == serialnumber
                                         && udr.AnalysisActionTypeID == 2
                                         && udr.RID == rid
                                         select new
                                         {
                                             udr.AccountID,
                                             udr.ReviewedDate
                                         }).FirstOrDefault();

        rmaCount = (from r in adc.rma_Returns
                    join rd in adc.rma_ReturnDevices on r.ReturnID equals rd.ReturnID
                    where r.AccountID == udrAccountIDAndReviewDate.AccountID && r.ReturnTypeID == 3 && r.Active == true
                    && rd.SerialNo == serialnumber && rd.Active == true
                    && r.CreatedDate >= udrAccountIDAndReviewDate.ReviewedDate
                    select r).Count();

        // Modified Date: 08/15/2019. Modified By: TDo
        //// LINQ cannot JOIN tables from multiple DataContexts, this is the workaround.
        //// UserDeviceReads Table.
        //var UDRTable = (from udr in idc.UserDeviceReads
        //                where udr.DeviceInventory.SerialNo == serialnumber
        //                && udr.AnalysisActionTypeID == 2
        //                && udr.RID == rid
        //                select udr.AccountID).ToList();

        //// rma_ReturnDevices Table.
        //var RDTable = (from rd in adc.rma_ReturnDevices
        //               where rd.SerialNo == serialnumber && rd.DepartmentID == 3
        //               select rd.ReturnID).ToList();

        //// rma_Returns Table.
        //var RTable = (from r in adc.rma_Returns
        //              where UDRTable.Contains(r.AccountID)
        //              && RDTable.Contains(r.ReturnID)
        //              select r).ToList();

        //rmaCount = RTable.Count;
    }

    /// <summary>
    /// Get all "Failed Test" full names (based on AnalysisTypeName).
    /// </summary>
    /// <param name="rid"></param>
    /// <returns></returns>
    private string GetAllFailedTestNames(int rid)
    {
        string allFailedTestNames = "";

        var listOfAllFailedTests = (from ard in idc.AnalysisReadDetails
                                    join at in idc.AnalysisTypes on ard.AnalysisTypeID equals at.AnalysisTypeID
                                    where ard.RID == rid && ard.Passed == false
                                    orderby at.AnalysisTypeAbbrev ascending
                                    select at.AnalysisTypeName).ToList();

        foreach (string test in listOfAllFailedTests)
        {
            // Format List (string).
            allFailedTestNames += test + "\r\n";
        }

        return allFailedTestNames;
    }
    #endregion

    protected void lnkbtnCompleteReviewProcess_Click(object sender, EventArgs e)
    {
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('divCompleteReviewProcessDialog')", true);
    }

    /// <summary>
    /// Populates E-Mail Template DDL with appropriate STRING value for each Template.
    /// </summary>
    /// <param name="analysisactiontypeid"></param>
    private void PopulateTemplateDDL(int analysisactiontypeid)
    {
        ddlEmailTemplates.Items.Clear();

        int? getBrandSourceID = (from ad in idc.AccountDevices
                                 where ad.DeviceInventory.SerialNo == serialNumber
                                 select ad.Account.BrandSourceID).FirstOrDefault();

        int brandSourceID = 0;

        if (!getBrandSourceID.HasValue) brandSourceID = 2;
        else brandSourceID = getBrandSourceID.Value;

        ddlEmailTemplates.Items.Insert(0, new ListItem("-Select-", "0"));

        // BrandSourceID = 2 --> Mirion
        // BrandSourceID = 3 --> IC Care

        switch (analysisactiontypeid)
        {
            case 1:
                // Leave open if E-Mail Template is created for this case.
                break;
            case 2:
                if (brandSourceID == 2) // Mirion
                {
                    ddlEmailTemplates.Items.Insert(1, new ListItem("Mirion Recall Notification", "Mirion Recall Notification"));
                    ddlEmailTemplates.Items.Insert(2, new ListItem("Mirion Recall Address Confirmation", "Mirion Recall Address Confirmation"));
                    ddlEmailTemplates.Items.Insert(3, new ListItem("Mirion Recall Unassign Address Confirmation", "Mirion Recall Unassign Address Confirmation"));
                }
                else // IC Care
                {
                    ddlEmailTemplates.Items.Insert(1, new ListItem("ICCare Recall Notification", "ICCare Recall Notification"));
                    ddlEmailTemplates.Items.Insert(2, new ListItem("ICCare Recall Address Confirmation", "ICCare Recall Address Confirmation"));
                    ddlEmailTemplates.Items.Insert(3, new ListItem("ICCare Recall Unassign Address Confirmation", "ICCare Recall Unassign Address Confirmation"));
                }
                break;
            case 3:
                if (brandSourceID == 2) // Mirion
                {
                    ddlEmailTemplates.Items.Insert(1, new ListItem("Mirion Watchlist Read Request", "Mirion Watchlist Read Request"));
                }
                else // IC Care
                {
                    ddlEmailTemplates.Items.Insert(1, new ListItem("ICCare Watchlist Read Request", "ICCare Watchlist Read Request"));
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// IF either "Recall" or "Watchlist" are selected, the Send E-mail Checkbox will be automatically checked.
    /// Based on this, LOAD only Shipping and E-Mail Addresses accordingly;
    /// None/Good --> Neither/None
    /// Recall --> Shipping & E-Mail Addresses
    /// Watchlist --> E-Mail Addresses
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void rdobtnlstAnalysisActionType_OnSelectedIndexChanged(object sender, EventArgs e)
    {
        int analysisActionTypeID = 0;
        Int32.TryParse(rdobtnlstAnalysisActionType.SelectedValue, out analysisActionTypeID);

        if (analysisActionTypeID != 1)
        {
            PopulateTemplateDDL(analysisActionTypeID);
            GetAllEMailAddresses(serialNumber);

            chkbxSendEmail.Enabled = true;
            chkbxSendEmail.Checked = true;
            pnlEmailForm.Visible = true;

            if (analysisActionTypeID == 2)
            {
                GetAllShippingAddresses(serialNumber);
                pnlShippingAddresses.Visible = true;
            }

            if (analysisActionTypeID == 3)
            {
                pnlShippingAddresses.Visible = false;
            }
        }
        else
        {
            chkbxSendEmail.Enabled = false;
            chkbxSendEmail.Checked = false;
            pnlEmailForm.Visible = false;
        }
    }

    /// <summary>
    /// IF the User checks "Send E-mail" then the E-mail form area will be visible (only).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void chkbxSendEmail_OnCheckedChanged(object sender, EventArgs e)
    {
        if (chkbxSendEmail.Checked == true)
        {
            pnlEmailForm.Visible = true;
        }
        else
        {
            pnlEmailForm.Visible = false;
        }
    }

    #region GET ALL E-MAIL ADDRESSES FROM CHECKBOX LIST TO: AND CC: CONTROLS.
    /// <summary>
    /// Checks to see which items have been checked and Populates To or Cc Textboxes.
    /// </summary>
    /// <returns></returns>
    private string PopulateToAndCcEmailAddress()
    {
        for (int i = 0; i < pnlEmailAddresses.Controls.Count; i++)
        {
            if (pnlEmailAddresses.Controls[i].GetType() == chkbxlstWearerEmailAddresses.GetType())
            {
                CheckBoxList chkbxlst = (CheckBoxList)pnlEmailAddresses.Controls[i];
                foreach (ListItem item in chkbxlst.Items)
                {
                    if (item.Selected)
                    {
                        emailToList += item + ";";
                    }
                }
            }
        }

        if (chkbxOtherEmailAddress.Checked)
        {
            emailToList += txtOtherEmailAddress.Text + ";";
        }

        return emailToList;
    }

    /// <summary>
    /// Populates the "To: " Textbox with all requested E-Mails (selected).
    /// This will also fire a Validation on "Other E-mail Address" Textbox value(s).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnTo_OnClick(object sender, EventArgs e)
    {
        txtTo.Text += PopulateToAndCcEmailAddress();

        // Uncheck ALL CheckBoxList/CheckBox controls.
        UncheckCheckBoxes(pnlEmailAddresses, chkbxlstWearerEmailAddresses);
    }

    protected void btnTo_ClearAll_OnClick(object sender, EventArgs e)
    {
        txtTo.Text = String.Empty;
    }

    /// <summary>
    /// Populates the "Cc: " Textbox with all requested E-Mails (selected).
    /// This will also fire a Validation on "Other E-mail Address" Textbox value(s).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnCc_OnClick(object sender, EventArgs e)
    {
        txtCc.Text += PopulateToAndCcEmailAddress();

        // Uncheck ALL CheckBoxList/CheckBox controls.
        UncheckCheckBoxes(pnlEmailAddresses, chkbxlstWearerEmailAddresses);
    }

    protected void btnCc_ClearAll_OnClick(object sender, EventArgs e)
    {
        txtCc.Text = String.Empty;
    }
    #endregion

    #region MODAL/DIALOG VALIDATION FUNCTION.
    /// <summary>
    /// Validates "Complete Read Analysis Review" Modal/Dialog controls.
    /// </summary>
    /// <returns></returns>
    private bool formIsValid()
    {
        // An Analysis Action Type must be selected.
        if (rdobtnlstAnalysisActionType.SelectedIndex == -1)
        {
            errorMessage = "Please select a Status/Action.";
            VisibleErrors_Review(errorMessage);
            rdobtnlstAnalysisActionType.Focus();
            return false;
        }

        // Validations specifically if E-Mail is being sent with Review Form.
        if (chkbxSendEmail.Checked)
        {
            if (ddlEmailTemplates.SelectedValue == "0")
            {
                errorMessage = "Please select an E-mail Template.";
                VisibleErrors_Review(errorMessage);
                txtOtherEmailAddress.Focus();
                return false;
            }

            // Only (1) Address can be selected.
            int checkedAddresses = 0;
            for (int i = 0; i < pnlShippingAddresses.Controls.Count; i++)
            {
                if (pnlShippingAddresses.Controls[i].GetType() == chkbxlstWearerAddresses.GetType())
                {
                    CheckBoxList chkbxlst = (CheckBoxList)pnlShippingAddresses.Controls[i];
                    foreach (ListItem item in chkbxlst.Items)
                    {
                        if (item.Selected)
                        {
                            checkedAddresses++;
                        }
                    }
                }
            }

            if (checkedAddresses > 1)
            {
                errorMessage = "Please select only (1) Shipping Address.";
                VisibleErrors_Review(errorMessage);
                return false;
            }

            if (chkbxOtherEmailAddress.Checked == true && txtOtherEmailAddress.Text == "")
            {
                errorMessage = "Please enter an 'Other' E-mail Address.";
                VisibleErrors_Review(errorMessage);
                txtOtherEmailAddress.Focus();
                return false;
            }

            // The "To:" Textbox should not be empty.
            if (txtTo.Text == "")
            {
                errorMessage = "Please select/enter a 'To' E-mail Address.";
                VisibleErrors_Review(errorMessage);
                txtTo.Focus();
                return false;
            }

            // Check formatting of each E-mail Address entered.
            Regex regexEmail = new Regex(@"^[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?");

            List<string> resultingToEmails = new List<string>(txtTo.Text.Replace(" ", "").Split(';'));
            List<string> resultingCcEmails = new List<string>(txtCc.Text.Replace(" ", "").Split(';'));

            if (resultingToEmails.Count() != 0)
            {
                resultingToEmails.RemoveAt(resultingToEmails.Count - 1);

                foreach (string toEmail in resultingToEmails)
                {
                    Match match = regexEmail.Match(toEmail);

                    if (!match.Success)
                    {
                        errorMessage = "Incorrect E-mail format for To: " + toEmail + " (ex. johnsmith@somecompany.com).";
                        VisibleErrors_Review(errorMessage);
                        return false;
                    }
                }
            }

            if (resultingCcEmails.Count() != 0)
            {
                resultingCcEmails.RemoveAt(resultingCcEmails.Count - 1);

                foreach (string ccEmail in resultingCcEmails)
                {
                    Match match = regexEmail.Match(ccEmail);

                    if (!match.Success)
                    {
                        errorMessage = "Incorrect E-mail format for Cc: " + ccEmail + " (ex. johnsmith@somecompany.com).";
                        VisibleErrors_Review(errorMessage);
                        return false;
                    }
                }
            }
        }

        return true;
    }
    #endregion

    /// <summary>
    /// OnClick, COMPLETES Review Process.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnCompleteReviewProcess_Click(object sender, EventArgs e)
    {
        // Reset all Success and Error Messages.
        InvisibleFormMessage_Review();
        InvisibleErrors_Review();

        // Get & Set AnalysisActionTypeID based on RadioButton selection.
        int analysisActionTypeID = 0;
        int.TryParse(rdobtnlstAnalysisActionType.SelectedValue, out analysisActionTypeID);

        // Get & Set CSNotes based on what is txtCSNotes control.
        string csNotes = "";
        if (txtCSNotes.Text != "") csNotes = txtCSNotes.Text.Replace("'", "''");
        else csNotes = null;

        // Passes Validation of "Complete Review Process" Form.
        if (formIsValid())
        {
            // UPDATE record in UserDeviceReads table (based on RID).
            var getUserDeviceReadRecord = (from udr in idc.UserDeviceReads
                                           where udr.RID == rID
                                           select udr).FirstOrDefault();

            if (getUserDeviceReadRecord == null) return;

            DateTime startDate = getUserDeviceReadRecord.CreatedDate;

            // Get & Set ReadAnalysisLogDetailID based on RID.
            int? readAnalysisLogDetailID = null;
            readAnalysisLogDetailID = getUserDeviceReadRecord.ReadAnalysisLogDetailID;

            if (readAnalysisLogDetailID == null) return;

            // Added 08/21/2013 by Anuradha Nandi
            // 1.) If the Device is marked as "Recall" (checked in btnCompleteReviewProcess_Click). 
            // 2.) Get all RIDs associated with the Device's Serial Number (greater than/equal to RID's CreatedDate).
            // 3.) Mark all RIDs (in this list) as follows;
            //     a.) ReviewedBy = [UserName]
            //     b.) ReviewDate = [DateTime.Now]
            //     c.) AnalysisActionTypeID = 2
            // NOTE: This will only apply to Reads that are currently in UserDeviceReads table.
            //       The User will have to re-mark as Recall for User Reads entered after this marking.
            //       Phase II of development will automate this with a Watchdog System.
            if (analysisActionTypeID == 2)
            {
                // Get ALL reads with the ReadAnalysisLogID (only). 
                var udrByReadAnalysisLogDetailID = (from udr in idc.UserDeviceReads
                                                    where udr.DeviceInventory.SerialNo == serialNumber
                                                    && udr.ReadAnalysisLogDetailID == readAnalysisLogDetailID
                                                    && udr.CreatedDate >= startDate
                                                    select udr).AsQueryable();

                // LOOP through each read and record/INSERT as Recall.
                foreach (var eachUDR in udrByReadAnalysisLogDetailID)
                {
                    eachUDR.ReviewedDate = DateTime.Now;
                    eachUDR.ReviewedBy = UserName;
                    eachUDR.AnalysisActionTypeID = 2;
                }
            }
            else
            {
                getUserDeviceReadRecord.ReviewedDate = DateTime.Now;
                getUserDeviceReadRecord.ReviewedBy = UserName;
                getUserDeviceReadRecord.AnalysisActionTypeID = analysisActionTypeID;
            }

            // INSERT CS Notes into AccountNotes table.
            if (csNotes != null)
            {
                AccountNote an = new AccountNote
                {
                    AccountID = getUserDeviceReadRecord.AccountID,
                    CreatedDate = DateTime.Now,
                    CreatedBy = UserName,
                    NoteText = csNotes,
                    Active = true
                };

                // Add the NEW object to the DeviceTechnicalNotes collection.
                idc.AccountNotes.InsertOnSubmit(an);
            }

            // Send Client E-Mail (functionality), IF "Send E-Mail" is Checked == true.
            if (chkbxSendEmail.Checked == true)
            {
                SendEmails();
            }

            // Submit the change to the Database and Send e-mail(s).
            try
            {
                idc.SubmitChanges();

                idc.sp_UpdateReadAnalysisLogDetailCounts(readAnalysisLogDetailID);

                Session["BadgeReviewHistory"] = null;
                rgBadgeReviewHistory.Rebind();
                lnkbtnCompleteReviewProcess.Visible = false;
                VisibleFormMessage_Review("Review was successfully recorded.");
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divCompleteReviewProcessDialog')", true);
            }
            catch (Exception ex)
            {
                this.VisibleErrors_Review(string.Format("An error occurred: {0}", ex.Message));
            }
        }
    }

    public void SendEmails()
    {
        List<string> toEmailsList = new List<string>(txtTo.Text.Replace(" ", "").Split(';'));
        List<string> ccEmailsList = new List<string>(txtCc.Text.Replace(" ", "").Split(';'));

        // "To:" E-mails fields cannot be empty, therefore, check only "Cc:" if it is empty.
        toEmailsList.RemoveAt(toEmailsList.Count - 1);
        if (ccEmailsList.Count != 0) ccEmailsList.RemoveAt(ccEmailsList.Count - 1);
        else ccEmailsList = null;

        // Get "To:" List of Name(s) (FirstName + LastName) from DB.
        string fullNameList = "";   // {SalutationName} Field in E-Mail Template.
        string firstEmail = toEmailsList.First();
        string lastEmail = toEmailsList.Last();

        foreach (string email in toEmailsList)
        {
            string fullName = "";

            var firstAndLastName = (from u in idc.Users
                                    where u.Email == email
                                    select new
                                    {
                                        u.FirstName,
                                        u.LastName
                                    }).FirstOrDefault();

            if (firstAndLastName != null) fullName = firstAndLastName.FirstName + " " + firstAndLastName.LastName;
            else fullName = email;

            // Formats List of E-mails (i.e. Name1, Name2, and Name3).
            if (toEmailsList.Count > 1)
            {
                if (object.ReferenceEquals(email, firstEmail)) fullNameList += fullName;
                if (!object.ReferenceEquals(email, firstEmail) && !object.ReferenceEquals(email, lastEmail)) fullNameList += ", " + fullName + ", ";
                if (object.ReferenceEquals(email, lastEmail)) fullNameList += " and " + fullName;
            }
            else fullNameList = fullName;
        }

        // Set-up E-mail Object and Values.
        var msgSystem = new MessageSystem()
        {
            Application = "Read Analysis",
            CreatedBy = UserName,
            FromAddress = "irv-tech-ops@mirion.com",
            ToAddressList = new List<string>(),
            CcAddressList = new List<string>()
        };

        msgSystem.ToAddressList = toEmailsList;
        msgSystem.CcAddressList = ccEmailsList;

        string wearerName = "";

        var getWearerName = (from ud in idc.UserDevices
                             where ud.DeviceInventory.SerialNo == serialNumber
                             select new
                             {
                                 ud.User.FirstName,
                                 ud.User.LastName
                             }).FirstOrDefault();

        if (getWearerName == null) wearerName = "(No Active Wearer is Assigned)";
        else wearerName = getWearerName.FirstName + " " + getWearerName.LastName;

        DateTime createdDate = DateTime.Now;

        DateTime lastReadDateTime = (from udr in idc.UserDeviceReads
                                     where udr.RID == rID
                                     select udr.CreatedDate).FirstOrDefault();

        if (lastReadDateTime == null) createdDate = DateTime.Now;
        else createdDate = lastReadDateTime;

        var emailFields = new Dictionary<string, string>();
        emailFields.Add("SalutationName", fullNameList);
        emailFields.Add("SerialNo", serialNumber);
        emailFields.Add("WearerName", wearerName);
        emailFields.Add("DateOfLastRead", createdDate.ToString());
        emailFields.Add("ShippingAddress", GetShippingAddress());

        string emailTemplateName = ddlEmailTemplates.SelectedValue;

        // SEND E-mail(s).
        msgSystem.Send(emailTemplateName, "", emailFields);

        // NOTE: For Phase II of application development, have e-mails save to Documents table.
    }

    /// <summary>
    /// Gets value of CheckBoxList Item CHECKED for Shipping Address.
    /// This value will be used in the MessageSystem.Send Field for {Adddress}.
    /// </summary>
    /// <returns></returns>
    private string GetShippingAddress()
    {
        for (int i = 0; i < pnlShippingAddresses.Controls.Count; i++)
        {
            if (pnlShippingAddresses.Controls[i].GetType() == chkbxlstWearerAddresses.GetType())
            {
                CheckBoxList chkbxlst = (CheckBoxList)pnlShippingAddresses.Controls[i];
                foreach (ListItem item in chkbxlst.Items)
                {
                    if (item.Selected)
                    {
                        shippingAddress += item;
                    }
                }
            }
        }

        if (string.IsNullOrEmpty(shippingAddress))
        {
            return "---";
        }
        else
        {
            return shippingAddress;
        }
    }

    /// <summary>
    /// Resets the Complete Read Analysis Review Modal/Dialog controls.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnCancel_CRP_Click(object sender, EventArgs e)
    {
        // Reset Form & Error Messages.
        InvisibleFormMessage_Review();
        InvisibleErrors_Review();

        // Clear/Reset ASP.NET Controls. The Review Form will still be Active/Visible.
        // This is case the User made an error in the Review Process and needs to make corrections/updates.
        rdobtnlstAnalysisActionType.SelectedIndex = -1;
        txtCSNotes.Text = string.Empty;
        chkbxSendEmail.Checked = false;
        pnlEmailForm.Visible = false;
        ddlEmailTemplates.SelectedValue = "0";
        // UNCHECK all CheckBoxLists.
        UncheckCheckBoxes(pnlShippingAddresses, chkbxlstWearerAddresses);
        UncheckCheckBoxes(pnlEmailAddresses, chkbxlstWearerEmailAddresses);
        txtTo.Text = string.Empty;
        txtCc.Text = string.Empty;

        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divCompleteReviewProcessDialog')", true);
    }

    #region SHOW/HIGH SUCCESS & ERROR MESSAGE FUNCTIONS.
    // Reset Form Message.
    private void InvisibleFormMessage_Review()
    {
        this.spnFormMessage_Review.InnerText = "";
        this.divFormMessage_Review.Visible = false;
    }

    // Set Form Message.
    private void VisibleFormMessage_Review(string error)
    {
        this.spnFormMessage_Review.InnerText = error;
        this.divFormMessage_Review.Visible = true;
    }

    // Reset Error Message.
    private void InvisibleErrors_Review()
    {
        this.spnErrorMessage_Review.InnerText = "";
        this.divErrorMessage_Review.Visible = false;
    }

    // Set Error Message.
    private void VisibleErrors_Review(string error)
    {
        this.spnErrorMessage_Review.InnerText = error;
        this.divErrorMessage_Review.Visible = true;
    }
    #endregion
    // -------------------- END :: REVIEW TAB FUNCTIONS -------------------- //
    #endregion

    protected void btnBackToSerialNumberSearch_OnClick(object sender, EventArgs e)
    {
        Response.Redirect("SerialNoSearch.aspx");
    }

    protected void rgReads_Child_ItemCreated(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridFilteringItem)
        {
            AddReadTypeDropdownFilter(e);
        }
    }

    protected void rgReads_Child_IDVue_ItemCreated(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridFilteringItem)
        {
            AddReadTypeDropdownFilter(e);
        }
    }

    protected void cmbReadTypeFilter_SelectedIndexChanged(object sender, EventArgs e)
    {
        var combo = (RadComboBox)sender;
        Session["IncludedReadType"] = combo.SelectedValue;
        var rg = GetParentOfType(combo, typeof(RadGrid)) as RadGrid;
        rg.Rebind();
    }

    private void AddReadTypeDropdownFilter(GridItemEventArgs e)
    {
        GridFilteringItem item = (GridFilteringItem)e.Item;
        item["ReadType"].Controls.Clear();
        RadComboBox combo = new RadComboBox();
        combo.ID = "cmbReadTypeFilter";

        combo.Items.Add(new RadComboBoxItem("All", ""));
        combo.Items.Add(new RadComboBoxItem("User Read", "User Read"));
        combo.Items.Add(new RadComboBoxItem("Soft User Read", "Soft User Read"));

        if (Session["IncludedReadType"] != null)
            combo.SelectedValue = Session["IncludedReadType"].ToString();
        else
            combo.SelectedValue = "User Read";

        combo.AutoPostBack = true;
        combo.EnableViewState = true;
        combo.Width = 50;
        combo.SelectedIndexChanged += new RadComboBoxSelectedIndexChangedEventHandler(cmbReadTypeFilter_SelectedIndexChanged);
        item["ReadType"].Controls.Add(combo);
    }

    public static Control GetParentOfType(Control childControl,
                                   Type parentType)
    {
        Control parent = childControl.Parent;
        while (parent.GetType() != parentType)
        {
            parent = parent.Parent;
        }
        if (parent.GetType() == parentType)
            return parent;

        throw new Exception("No control of expected type was found");
    }

    public class ChartData<XType, YType>
    {
        public XType x { get; set; }
        public YType y { get; set; }

        public ChartData(XType x1, YType y1)
        {
            x = x1;
            y = y1;
        }
    }
}
