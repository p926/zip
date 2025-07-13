using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Instadose.Data;
using Telerik.Web.UI;
/// <summary>
/// Summary description for ReadAnalysisRadGrids
/// </summary>
public class ReadAnalysisRadGrids : RadGrid
{
    protected RadGrid radGrd;
    private InsDataContext idc = new InsDataContext();
    private bool isFiltered = false;
    public string accountIDs { get; set; }
    public string companyName { get; set; }
    public int logID { get; set; }
    public string chkbxName { get; set; }
    public string rcbName { get; set; }

    public ReadAnalysisRadGrids(string accountids, string companyname, int logid)
    {
        accountIDs = accountids;
        companyName = companyname;
        logID = logid;
        chkbxName = "chkbxCompleteReview_" + companyname + "Reads";
        rcbName = "rcbFailedTests_" + companyname + "Reads";
    }

    // ------------------------------ BEGIN :: FUNCTIONS THAT GENERATE THE RADGRID ------------------------------ //
    /// <summary>
    /// Returns a RadGrid that will have all the columns and functionality as required.
    /// </summary>
    /// <returns></returns>
	public RadGrid GenerateRadGrid()
	{
        // Begin :: Basic RadGrid Attributes
        radGrd = new RadGrid();
        radGrd.ID = "rg" + companyName +"Reads";
        radGrd.CssClass = "OTable";
        radGrd.AllowFilteringByColumn = true;
        radGrd.AllowPaging = false;
        radGrd.AllowSorting = true;
        radGrd.AutoGenerateColumns = false;
        radGrd.ItemCommand += new GridCommandEventHandler(RadGridView_ItemCommand);
        radGrd.PreRender += new EventHandler(RadGridView_PreRender);
        radGrd.ItemDataBound += new GridItemEventHandler(RadGridView_ItemDataBound);
        radGrd.Style.Add("border", "1px solid #CCCCCC");
        radGrd.ItemStyle.Width = Unit.Percentage(100);
        radGrd.EnableViewState = false;
        radGrd.GridLines = GridLines.None;
        radGrd.ShowFooter = true;
        radGrd.SkinID = "Default";
        // GroupingSettings
        radGrd.GroupingSettings.CaseSensitive = false;
        // ClientSettings
        radGrd.ClientSettings.EnableRowHoverStyle = false;
        radGrd.ClientSettings.Scrolling.AllowScroll = false;
        radGrd.ClientSettings.Scrolling.UseStaticHeaders = true;
        radGrd.ClientSettings.Selecting.AllowRowSelect = false;
        radGrd.ClientSettings.ClientEvents.OnFilterMenuShowing = "filterMenuShowing";
        // FilterMenu
        radGrd.FilterMenu.OnClientShown = "MenuShowing";
        radGrd.FilterMenu.EnableImageSprites = false;
        // MasterTableView
        radGrd.MasterTableView.DataKeyNames = new string[] { "RID" };
        radGrd.MasterTableView.TableLayout = GridTableLayout.Fixed;
        radGrd.MasterTableView.AllowMultiColumnSorting = true;
        // Begin :: Columns
        // CheckBox "All Passed"/"Failed Tests"
        GridTemplateColumn templateChckBxColumn = new GridTemplateColumn();
        templateChckBxColumn.ItemTemplate = new CheckBoxItemTemplate("RID", companyName);
        templateChckBxColumn.HeaderTemplate = new CheckBoxHeaderTemplate("RID", companyName, radGrd);
        templateChckBxColumn.DataField = "RID";
        templateChckBxColumn.UniqueName = "RID";
        templateChckBxColumn.AllowFiltering = false;
        templateChckBxColumn.HeaderStyle.Width = Unit.Pixel(50);
        templateChckBxColumn.FilterControlWidth = Unit.Pixel(50);
        templateChckBxColumn.ItemStyle.Width = Unit.Pixel(50);
        templateChckBxColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        templateChckBxColumn.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
        radGrd.MasterTableView.Columns.Add(templateChckBxColumn);
        // AccountID
        string accountIDColumnName = "Account ID";
        GridTemplateColumn templateAccountIDColumn = new GridTemplateColumn();
        templateAccountIDColumn.ItemTemplate = new HyperLinkTemplate("AccountID", companyName);
        templateAccountIDColumn.HeaderText = accountIDColumnName;
        templateAccountIDColumn.DataField = "AccountID";
        templateAccountIDColumn.UniqueName = "AccountID";
        templateAccountIDColumn.AllowFiltering = true;
        templateAccountIDColumn.HeaderStyle.Width = Unit.Pixel(100);
        templateAccountIDColumn.FilterControlWidth = Unit.Pixel(50);
        templateAccountIDColumn.ItemStyle.Width = Unit.Pixel(100);
        templateAccountIDColumn.CurrentFilterFunction = GridKnownFunction.EqualTo;
        templateAccountIDColumn.AutoPostBackOnFilter = true;
        templateAccountIDColumn.SortExpression = "AccountID";
        templateAccountIDColumn.FooterText = "Total: ";
        templateAccountIDColumn.Aggregate = GridAggregateFunction.Count;
        radGrd.MasterTableView.Columns.Add(templateAccountIDColumn);
        // SerialNumber
        string serialNumberColumnName = "Serial No.";
        GridTemplateColumn templateSerialNumberColumn = new GridTemplateColumn();
        templateSerialNumberColumn.ItemTemplate = new HyperLinkTemplate("SerialNumber", companyName);
        templateSerialNumberColumn.HeaderText = serialNumberColumnName;
        templateSerialNumberColumn.DataField = "SerialNumber";
        templateSerialNumberColumn.UniqueName = "SerialNumber";
        templateSerialNumberColumn.AllowFiltering = true;
        templateSerialNumberColumn.HeaderStyle.Width = Unit.Pixel(100);
        templateSerialNumberColumn.FilterControlWidth = Unit.Pixel(50);
        templateSerialNumberColumn.ItemStyle.Width = Unit.Pixel(100);
        templateSerialNumberColumn.CurrentFilterFunction = GridKnownFunction.EqualTo;
        templateSerialNumberColumn.AutoPostBackOnFilter = true;
        templateSerialNumberColumn.SortExpression = "SerialNumber";
        radGrd.MasterTableView.Columns.Add(templateSerialNumberColumn);
        // UserName
        string userNameColumnName = "Username";
        GridTemplateColumn templateUserNameColumn = new GridTemplateColumn();
        templateUserNameColumn.ItemTemplate = new HyperLinkTemplate("UserName", companyName);
        templateUserNameColumn.HeaderText = userNameColumnName;
        templateUserNameColumn.DataField = "UserName";
        templateUserNameColumn.UniqueName = "UserName";
        templateUserNameColumn.AllowFiltering = true;
        templateUserNameColumn.HeaderStyle.Width = Unit.Pixel(135);
        templateUserNameColumn.FilterControlWidth = Unit.Pixel(95);
        templateUserNameColumn.CurrentFilterFunction = GridKnownFunction.Contains;
        templateUserNameColumn.AutoPostBackOnFilter = true;
        templateUserNameColumn.SortExpression = "UserName";
        radGrd.MasterTableView.Columns.Add(templateUserNameColumn);
        // ExposureDate
        GridDateTimeColumn gdtcCreatedDateColumn = new GridDateTimeColumn();
        gdtcCreatedDateColumn.DataField = "CreatedDate";
        gdtcCreatedDateColumn.HeaderText = "Exposure Date";
        gdtcCreatedDateColumn.UniqueName = "CreatedDate";
        gdtcCreatedDateColumn.AllowFiltering = true;
        gdtcCreatedDateColumn.AllowSorting = true;
        gdtcCreatedDateColumn.HeaderStyle.Width = Unit.Pixel(135);
        gdtcCreatedDateColumn.FilterControlWidth = Unit.Pixel(95);
        gdtcCreatedDateColumn.SortExpression = "CreatedDate";
        radGrd.MasterTableView.Columns.Add(gdtcCreatedDateColumn);
        // Deep Low Dose Calculation
        GridBoundColumn gbcDLDCalcColumn = new GridBoundColumn();
        gbcDLDCalcColumn.DataField = "DLDCalc";
        gbcDLDCalcColumn.HeaderText = "DLD Calc.";
        gbcDLDCalcColumn.UniqueName = "DLDCalc";
        gbcDLDCalcColumn.AllowFiltering = false;
        gbcDLDCalcColumn.AllowSorting = true;
        gbcDLDCalcColumn.HeaderStyle.Width = Unit.Pixel(75);
        gbcDLDCalcColumn.FilterControlWidth = Unit.Pixel(75);
        gbcDLDCalcColumn.SortExpression = "DLDCalc";
        gbcDLDCalcColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        gbcDLDCalcColumn.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
        radGrd.MasterTableView.Columns.Add(gbcDLDCalcColumn);
        // Cumulative Dose
        GridBoundColumn gbcCumulDoseColumn = new GridBoundColumn();
        gbcCumulDoseColumn.DataField = "CumulDose";
        gbcCumulDoseColumn.HeaderText = "Cumul. Dose";
        gbcCumulDoseColumn.UniqueName = "CumulDose";
        gbcCumulDoseColumn.AllowFiltering = false;
        gbcCumulDoseColumn.AllowSorting = true;
        gbcCumulDoseColumn.HeaderStyle.Width = Unit.Pixel(75);
        gbcCumulDoseColumn.FilterControlWidth = Unit.Pixel(75);
        gbcCumulDoseColumn.SortExpression = "CumulDose";
        gbcCumulDoseColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        gbcCumulDoseColumn.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
        radGrd.MasterTableView.Columns.Add(gbcCumulDoseColumn);
        // Deep
        GridBoundColumn gbcDeepColumn = new GridBoundColumn();
        gbcDeepColumn.DataField = "Deep";
        gbcDeepColumn.HeaderText = "Deep";
        gbcDeepColumn.UniqueName = "Deep";
        gbcDeepColumn.AllowFiltering = false;
        gbcDeepColumn.AllowSorting = true;
        gbcDeepColumn.HeaderStyle.Width = Unit.Pixel(50);
        gbcDeepColumn.FilterControlWidth = Unit.Pixel(50);
        gbcDeepColumn.SortExpression = "Deep";
        gbcDeepColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        gbcDeepColumn.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
        radGrd.MasterTableView.Columns.Add(gbcDeepColumn);
        // Shallow
        GridBoundColumn gbcShallowColumn = new GridBoundColumn();
        gbcShallowColumn.DataField = "Shallow";
        gbcShallowColumn.HeaderText = "Shallow";
        gbcShallowColumn.UniqueName = "Shallow";
        gbcShallowColumn.AllowFiltering = false;
        gbcShallowColumn.AllowSorting = true;
        gbcShallowColumn.HeaderStyle.Width = Unit.Pixel(50);
        gbcShallowColumn.FilterControlWidth = Unit.Pixel(50);
        gbcShallowColumn.SortExpression = "Shallow";
        gbcShallowColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        gbcShallowColumn.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
        radGrd.MasterTableView.Columns.Add(gbcShallowColumn);
        // Eye
        GridBoundColumn gbcEyeColumn = new GridBoundColumn();
        gbcEyeColumn.DataField = "Eye";
        gbcEyeColumn.HeaderText = "Eye";
        gbcEyeColumn.UniqueName = "Eye";
        gbcEyeColumn.AllowFiltering = false;
        gbcEyeColumn.AllowSorting = true;
        gbcEyeColumn.HeaderStyle.Width = Unit.Pixel(50);
        gbcEyeColumn.FilterControlWidth = Unit.Pixel(50);
        gbcEyeColumn.SortExpression = "Eye";
        gbcEyeColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        gbcEyeColumn.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
        radGrd.MasterTableView.Columns.Add(gbcEyeColumn);
        // Failed Tests
        GridBoundColumn gbcFailedTestsColumn = new GridBoundColumn();
        gbcFailedTestsColumn.DataField = "FailedTests";
        gbcFailedTestsColumn.HeaderText = "Results";
        gbcFailedTestsColumn.UniqueName = "FailedTests";
        gbcFailedTestsColumn.AllowFiltering = false;
        gbcFailedTestsColumn.AllowSorting = false;
        //gbcFailedTestsColumn.FilterTemplate = new RCBFilterTemplate("FailedTests", companyName);
        gbcFailedTestsColumn.HeaderStyle.Width = Unit.Pixel(100);
        gbcFailedTestsColumn.FilterControlWidth = Unit.Pixel(100);
        gbcFailedTestsColumn.EmptyDataText = "All Passed";
        radGrd.MasterTableView.Columns.Add(gbcFailedTestsColumn);
        // Previous Review Status
        GridBoundColumn gbcPreviousReviewStatusColumn = new GridBoundColumn();
        gbcPreviousReviewStatusColumn.DataField = "PreviousReviewStatus";
        gbcPreviousReviewStatusColumn.UniqueName = "PreviousReviewStatus";
        gbcPreviousReviewStatusColumn.Visible = false;
        radGrd.MasterTableView.Columns.Add(gbcPreviousReviewStatusColumn);
        // Goto BadgeReview.aspx?DeviceID={0}&RID={1}
        GridTemplateColumn templateDeviceIDColumn = new GridTemplateColumn();
        templateDeviceIDColumn.ItemTemplate = new HyperLinkTemplate("DeviceID", companyName);
        templateDeviceIDColumn.DataField = "DeviceID";
        templateDeviceIDColumn.UniqueName = "DeviceID";
        templateDeviceIDColumn.AllowFiltering = false;
        templateDeviceIDColumn.HeaderStyle.Width = Unit.Pixel(100);
        templateDeviceIDColumn.FilterControlWidth = Unit.Pixel(100);
        templateDeviceIDColumn.ItemStyle.HorizontalAlign = HorizontalAlign.Center;
        templateDeviceIDColumn.HeaderStyle.HorizontalAlign = HorizontalAlign.Center;
        radGrd.MasterTableView.Columns.Add(templateDeviceIDColumn);

        radGrd.DataSource = GetRadGridData(accountIDs, companyName, logID);
        radGrd.DataBind();

        return radGrd;
	}

    /// <summary>
    /// Returns a DataTable that will be passed in the DataSource for the RadGrid.
    /// AccountIDs are a string (formatted as "1234,5678,..."), this is how it is passed into the SP.
    /// The @pTabTitle is the truncated Company Name (i.e. Philips Healthcare is "Philips").
    /// </summary>
    /// <param name="accountids"></param>
    /// <param name="companyname"></param>
    /// <returns></returns>
    private DataTable GetRadGridData(string accountids, string companyname, int logid)
    {
        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.StoredProcedure;

        // Using the sp_ReadAnalysisReview.
        string sqlQuery = "sp_ReadAnalysisReview";

        sqlCmd.Parameters.Add(new SqlParameter("@pAccountIDs", accountids));
        sqlCmd.Parameters.Add(new SqlParameter("@pLinkTitle", companyname));
        sqlCmd.Parameters.Add(new SqlParameter("@pLogID", logid));

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable dtReadAnalysisResults = new DataTable("Get Read Analysis Results");

        // Create the columns for the DataTable.
        dtReadAnalysisResults.Columns.Add("RID", Type.GetType("System.Int32"));
        dtReadAnalysisResults.Columns.Add("DeviceID", Type.GetType("System.Int32"));
        dtReadAnalysisResults.Columns.Add("SerialNumber", Type.GetType("System.String"));
        dtReadAnalysisResults.Columns.Add("AccountID", Type.GetType("System.Int32"));
        dtReadAnalysisResults.Columns.Add("CompanyName", Type.GetType("System.String"));
        dtReadAnalysisResults.Columns.Add("UserID", Type.GetType("System.Int32"));
        dtReadAnalysisResults.Columns.Add("UserName", Type.GetType("System.String"));
        dtReadAnalysisResults.Columns.Add("CreatedDate", Type.GetType("System.DateTime"));
        dtReadAnalysisResults.Columns.Add("DLDCalc", Type.GetType("System.String"));
        dtReadAnalysisResults.Columns.Add("CumulDose", Type.GetType("System.String"));
        dtReadAnalysisResults.Columns.Add("Deep", Type.GetType("System.String"));
        dtReadAnalysisResults.Columns.Add("Shallow", Type.GetType("System.String"));
        dtReadAnalysisResults.Columns.Add("Eye", Type.GetType("System.String"));
        dtReadAnalysisResults.Columns.Add("FailedTests", Type.GetType("System.String"));
        dtReadAnalysisResults.Columns.Add("PreviousReviewStatus", Type.GetType("System.Int32"));

        while (sqlDataReader.Read())
        {
            DataRow drow = dtReadAnalysisResults.NewRow();

            // Fill row details.
            drow["RID"] = sqlDataReader["RID"];
            drow["DeviceID"] = sqlDataReader["DeviceID"];
            drow["SerialNumber"] = sqlDataReader["SerialNumber"];
            drow["AccountID"] = sqlDataReader["AccountID"];
            drow["CompanyName"] = sqlDataReader["CompanyName"];
            drow["UserID"] = sqlDataReader["UserID"];
            drow["UserName"] = sqlDataReader["UserName"];
            drow["CreatedDate"] = sqlDataReader["CreatedDate"];
            drow["DLDCalc"] = sqlDataReader["DLDCalc"];
            drow["CumulDose"] = sqlDataReader["CumulDose"];
            drow["Deep"] = sqlDataReader["Deep"];
            drow["Shallow"] = sqlDataReader["Shallow"];
            drow["Eye"] = sqlDataReader["Eye"];
            drow["FailedTests"] = sqlDataReader["FailedTests"];
            drow["PreviousReviewStatus"] = sqlDataReader["PreviousReviewStatus"];

            // Add rows to DataTable.
            dtReadAnalysisResults.Rows.Add(drow);
        }

        sqlConn.Close();
        sqlDataReader.Close();

        return dtReadAnalysisResults;
    }
    // ------------------------------ END :: FUNCTIONS THAT GENERATE THE RADGRID ------------------------------ //

    // ------------------------------ BEGIN :: RADGRID EVENTHANDLER FUNCTIONS ------------------------------ //
    protected void RadGridView_ItemCommand(object sender, GridCommandEventArgs e)
    {
        if (e.CommandName == RadGrid.FilterCommandName) isFiltered = true;
    }

    protected void RadGridView_PreRender(object sender, EventArgs e)
    {
        if (isFiltered)
        {
            string[] expressions = radGrd.MasterTableView.FilterExpression.Split(new string[] { "AND" }, StringSplitOptions.None);
            List<string> columnExpressions = new List<string>(expressions);
            foreach (string expression in columnExpressions)
            {
                if (expression.Contains("[\"FailedTests\"]"))
                {
                    columnExpressions.Remove(expression);
                    break;
                }
            }
            string finalExpression = string.Join("AND", columnExpressions);
            string typeFilterValue = (string)HttpContext.Current.Session["FailedTestFilterValue"];
            if (!string.IsNullOrEmpty(typeFilterValue))
            {
                if (!string.IsNullOrEmpty(finalExpression))
                {
                    finalExpression += " AND ";
                }
                finalExpression += typeFilterValue;
            }
            radGrd.MasterTableView.FilterExpression = finalExpression;
            radGrd.MasterTableView.Rebind();
        }
    }

    protected void RadGridView_ItemDataBound(object sender, GridItemEventArgs e)
    {
        // Color Code Records/Rows based All Passed/Failed Tests.
        ColorCodeRows(sender, e);

        int rID = 0;

        // Pre-Check all Records/Rows where "All Passed".
        if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;
            CheckBox chkBx = item.FindControl(chkbxName) as CheckBox;
            if (item["FailedTests"].Text == "All Passed")
            {
                chkBx.Checked = true;
            }

            // Tooltip that displays full name of Analysis Types/Tests (if failed).
            Int32.TryParse(item["RID"].Text, out rID);
            item["FailedTests"].ToolTip = GetAllFailedTestNames(rID);
        }

        if (e.Item is GridFilteringItem)
        {
            RadComboBox combo = (e.Item as GridFilteringItem).FindControl(rcbName) as RadComboBox;
            if (HttpContext.Current.Session["FailedTests"] != null)
            {
                foreach (string type in (List<string>)HttpContext.Current.Session["FailedTests"])
                {
                    combo.FindItemByText(type).Checked = true;
                }
            }
        }
    }
    // ------------------------------ END :: RADGRID EVENTHANDLER FUNCTIONS ------------------------------ //

    // ------------------------------ BEGIN :: RADGRID SUPPORTING FUNCTIONS ------------------------------ //
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

    /// <summary>
    /// Color codes each Data Row in the RadGrid based upon if the Read passed or failed the Read Analysis tests.
    /// LightGreen = All Passed -> No Failures.
    /// LightPink = Failures Occured (lists AnalysisTypeAbbrev's).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ColorCodeRows(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem colorCodedItem = (GridDataItem)e.Item;

            // For Failed Tests/Flagged Record(s).
            TableCell failedTestsCell = colorCodedItem["FailedTests"];

            // For Watchlisted Badge(s).
            // Since the AnalysisActionTypeID is being pulled, additional color-coding can be done in the future.
            TableCell watchlistedBadgesCell = colorCodedItem["PreviousReviewStatus"];

            // Read Analysis Tests that FAILED/FLAGGED.
            if (failedTestsCell.Text != "All Passed")
            {
                colorCodedItem.BackColor = System.Drawing.Color.LightPink;
            }

            // Previous Badge Review Status was set to Watchlist.
            if (watchlistedBadgesCell.Text == "3")
            {
                colorCodedItem.BackColor = System.Drawing.Color.LightBlue;
            }
        }
    }
    // ------------------------------ END :: RADGRID SUPPORTING FUNCTIONS ------------------------------ //

    // ------------------------------ BEGIN :: PRIVATE CLASSES FOR RADGRID TEMPLATE COLUMNS ------------------------------ //
    /// <summary>
    /// Generic HyperLink Template for RadGrid (used for AccountID, SerialNumber, and Username columns).
    /// </summary>
    private class HyperLinkTemplate : ITemplate
    {
        protected HyperLink hyprlnkValue;
        private string colName;
        private string compName;

        public HyperLinkTemplate(string colname, string compname)
        {
            colName = colname;
            compName = compname;
        }

        public void InstantiateIn(Control container)
        {
            hyprlnkValue = new HyperLink();
            hyprlnkValue.ID = "hyprlnk" + colName + "_" + compName + "Reads";
            hyprlnkValue.Enabled = true;
            hyprlnkValue.Target = "_blank";
            hyprlnkValue.DataBinding += new EventHandler(hyprlnkValue_DataBinding);
            container.Controls.Add(hyprlnkValue);
        }

        void hyprlnkValue_DataBinding(object sender, EventArgs e)
        {
            HyperLink hyprlnk = (HyperLink)sender;
            GridDataItem container = (GridDataItem)hyprlnk.NamingContainer;
            hyprlnk.Text = ((DataRowView)container.DataItem)[colName].ToString();

            switch (colName)
            {
                case "AccountID":
                    hyprlnk.NavigateUrl = String.Format("../InformationFinder/Details/Account.aspx?ID={0}", ((DataRowView)container.DataItem)[colName].ToString());
                    break;
                case "SerialNumber":
                    hyprlnk.NavigateUrl = String.Format("../InformationFinder/Details/Device.aspx?ID={0}&AccountID={1}", ((DataRowView)container.DataItem)[colName].ToString(), ((DataRowView)container.DataItem)["AccountID"].ToString());
                    break;
                case "UserName":
                    hyprlnk.NavigateUrl = String.Format("../InformationFinder/Details/UserMaintenance.aspx?AccountID={0}&UserID={1}", ((DataRowView)container.DataItem)["AccountID"].ToString(), ((DataRowView)container.DataItem)["UserID"].ToString());
                    break;
                case "DeviceID":
                    // Do not show DeviceID in Hyperlink text.
                    hyprlnk.Text = "Review";
                    hyprlnk.CssClass = "OButton";
                    hyprlnk.Style.Add("border", "1px solid #CCCCCC");
                    hyprlnk.Style.Add("background-color", "#EEEEEE");
                    hyprlnk.NavigateUrl = String.Format("BadgeReview.aspx?SerialNo={0}&RID={1}", ((DataRowView)container.DataItem)["SerialNumber"].ToString(), ((DataRowView)container.DataItem)["RID"].ToString());
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// CheckBox ItemTemplate for Marking for Review.
    /// Linked to RID and is Checked/Unchecked based on FailedTest value(s).
    /// "All Passed" == Checked | "[Failed Tests]" == Unchecked
    /// </summary>
    private class CheckBoxItemTemplate : ITemplate
    {
        protected CheckBox chkbxValue;
        private string colName;
        private string compName;

        public CheckBoxItemTemplate(string colname, string compname)
        {
            colName = colname;
            compName = compname;
        }

        public void InstantiateIn(Control container)
        {
            chkbxValue = new CheckBox();
            chkbxValue.ID = "chkbxCompleteReview_" + compName + "Reads";
            chkbxValue.Enabled = true;
            chkbxValue.DataBinding += new EventHandler(chkbxValue_DataBinding);
            container.Controls.Add(chkbxValue);
        }

        void chkbxValue_DataBinding(object sender, EventArgs e)
        {
            CheckBox chkbx = (CheckBox)sender;
            GridDataItem container = (GridDataItem)chkbx.NamingContainer;
            string failedTests = ((DataRowView)container.DataItem)["FailedTests"].ToString();
            if (failedTests == "")
            {
                chkbx.Checked = true;
                chkbx.Enabled = true;
            }
            else
            {
                chkbx.Checked = false;
                chkbx.Enabled = false;
            } 
        }
    }

    /// <summary>
    /// Checkbox HeaderTemplate used to Toggle all Checkbox ItemTemplates (Checked/Unchecked).
    /// </summary>
    private class CheckBoxHeaderTemplate : ITemplate
    {
        protected CheckBox headerChkBxValue;
        private string headerColName;
        private string headerCompName;
        private string headerChkBxName;
        private string itemChkBxName;
        private RadGrid headerRadGrd;

        public CheckBoxHeaderTemplate(string headercolname, string headercompname, RadGrid headerradgrd)
        {
            headerColName = headercolname;
            headerCompName = headercompname;
            headerChkBxName = "headerChkBx_" + headercompname + "Reads";
            itemChkBxName = "chkbxCompleteReview_" + headerCompName + "Reads";
            headerRadGrd = headerradgrd;
        }

        public void InstantiateIn(Control container)
        {
            headerChkBxValue = new CheckBox();
            headerChkBxValue.ID = headerChkBxName;
            headerChkBxValue.AutoPostBack = true;
            headerChkBxValue.Enabled = true;
            headerChkBxValue.DataBinding += new EventHandler(headerChkBox_Toggle);
            container.Controls.Add(headerChkBxValue);
        }

        protected void headerChkBox_Toggle(object sender, EventArgs e)
        {
            CheckBox headerChkBx = (CheckBox)sender;
            foreach (GridDataItem item in headerRadGrd.MasterTableView.Items)
            {
                (item.FindControl(itemChkBxName) as CheckBox).Checked = headerChkBx.Checked;
                item.Selected = headerChkBx.Checked;
            }
        }
    }

    /// <summary>
    /// RadComboBox Template used to Filter by Failed Tests.
    /// Inlcudes custome ImageButton to execute filter selections.
    /// </summary>
    private class RCBFilterTemplate : ITemplate
    {
        protected RadComboBox radCmbBxValue;
        protected ImageButton imgBtnValue;
        private string colName;
        private string compName;
        private bool filtered = false;

        public RCBFilterTemplate(string colname, string compname)
        {
            colName = colname;
            compName = compname;
        }

        public void InstantiateIn(Control container)
        {
            radCmbBxValue = new RadComboBox();
            radCmbBxValue.ID = "rcbFailedTests_" + compName + "Reads";
            radCmbBxValue.DataSourceID = "SQLDSAnalysisTypes";
            radCmbBxValue.DataTextField = "AnalysisTypeAbbrev";
            radCmbBxValue.DataValueField = "AnalysisTypeAbbrev";
            radCmbBxValue.AppendDataBoundItems = true;
            radCmbBxValue.CheckBoxes = true;
            radCmbBxValue.EmptyMessage = "-Select-";
            radCmbBxValue.SkinID = "Default";
            radCmbBxValue.AllowCustomText = false;
            radCmbBxValue.EnableCheckAllItemsCheckBox = true;
            radCmbBxValue.Width = Unit.Pixel(100);
            radCmbBxValue.Enabled = true;
            //radCmbBxValue.DataBinding += new EventHandler(radCmbBxValue_DataBinding);
            imgBtnValue = new ImageButton();
            imgBtnValue.ID = "imgBtnFilter_" + compName + "Reads";
            imgBtnValue.AlternateText = "Filter";
            imgBtnValue.DataBinding += new EventHandler(imgBtnValue_DataBinding);
            imgBtnValue.ImageUrl = "~/images/Filter.gif";
            imgBtnValue.Width = Unit.Pixel(22);
            imgBtnValue.Height = Unit.Pixel(22);
            container.Controls.Add(radCmbBxValue);
        }

        //protected void radCmbBxValue_DataBinding(object sender, EventArgs e)
        //{
        //    RadComboBox radCmbBx = (RadComboBox)sender;
        //    GridDataItem container = (GridDataItem)radCmbBx.NamingContainer;
        //    container.Controls.Add(radCmbBx);
        //}

        protected void imgBtnValue_DataBinding(object sender, EventArgs e)
        {
            ImageButton imgBtn = (ImageButton)sender;
            GridFilteringItem container = (GridFilteringItem)imgBtn.NamingContainer;
            container.Controls.Add(imgBtn);
            imgBtn.DataBinding += new EventHandler(imgBtnValue_Filter);
        }

        protected void imgBtnValue_Filter(object sender, EventArgs e)
        {
            GridFilteringItem filterItem = (sender as ImageButton).NamingContainer as GridFilteringItem;
            RadComboBox combo = filterItem.FindControl("rcbFailedTests_" + compName + "Reads") as RadComboBox;

            List<string> expressions = new List<string>();
            List<string> types = new List<string>();
            foreach (RadComboBoxItem item in combo.CheckedItems)
            {
                types.Add(item.Text);
                expressions.Add("(it[\"FailedTests\"].ToString().Contains(\"" + item.Text + "\"))");
            }
            filtered = true;
            string value = string.Join("OR", expressions);
            HttpContext.Current.Session["FailedTestFilterValue"] = string.IsNullOrEmpty(value) ? value : "(" + value + ")";
            HttpContext.Current.Session["FailedTests"] = types;
        }
    }
    // ------------------------------ END :: PRIVATE CLASSES FOR RADGRID TEMPLATE COLUMNS ------------------------------ //
}
