using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.UI.WebControls;
using Telerik.Web.UI;
using Instadose.Data;
using System.Collections.Generic;
using System.Web.UI;
using System.Threading;
using System.Drawing;
using Portal.Instadose;

// Modified:    December 22, 2015
// Modified By: Anuradha Nandi
// Time-Out Issue might be caused by the Unanalyzed Counter, temporaryly took that portion of code out.
// This exclusion does not effect any of the page's functionality.

public partial class TechOps_ReadAnalysisRecallList : System.Web.UI.Page
{
    // Declare Instadose Data Context.
    InsDataContext idc = new InsDataContext();
    AppDataContext adc = new AppDataContext();

    public int readAnalysisLogDetailID = 0;
    private int _defaultRIDStart = 141000000;

    #region ON PAGE LOAD.
    protected void Page_Load(object sender, EventArgs e)
    {
        ScriptManager.GetCurrent(this).AsyncPostBackTimeout = 600;

        if (Page.IsPostBack) return;

        // Set all the Sessions for each RadGrid.
        Session["RecallList"] = null;
        Session["Watchlist"] = null;
        Session["SummaryReports"] = null;
    }
    #endregion


    //#region RECALL LIST TAB.
    protected void rgRecallList_NeedDataSource(object source, GridNeedDataSourceEventArgs e)
    {
        if (Session["RecallList"] == null)
        {
            // Create the SQL Connection from the connection string in the web.config
            SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
                ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

            // Create the SQL Command.
            SqlCommand sqlCmd = new SqlCommand();
            sqlCmd.CommandTimeout = 360;  // 5 minutes
            sqlCmd.CommandType = CommandType.Text;

            var startRID = GetStartRID();
            // Modified by: TDO, modified date: 08/19/2019
            // SQL Statement gets the following records WHERE;
            // 1.) IsAnalyzed = 1 (TRUE)
            // 2.) AnalysisActionTypeID = 2 (RECALL)
            // 3.) And has never been generated recall by comparing (recentReviewedDate > recentRecalledDate)
            string sqlQuery = "SELECT DISTINCT " +
                              "DI.SerialNo AS [SerialNumber], " +
                              "UDR.DeviceID AS [DeviceID], " +
                              "ACCT.AccountID AS [AccountID], " +
                              "ACCT.CompanyName AS [CompanyName], " +
                              "CASE P.ProductGroupID " +
                              " WHEN 1 THEN 'ID1'" +
                              " WHEN 9 THEN 'ID1'" +
                              " WHEN 11 THEN 'ID2'" +
                              " WHEN 10 THEN 'ID Plus'" +
                              " WHEN 18 THEN 'ID VUE'" +
                              "END AS [ProductType], " +
                              "USRS.UserID AS [UserID], " +
                              "USRS.FirstName + ' ' + USRS.LastName AS [AssignedTo], " +
                              "(SELECT DISTINCT Color FROM Products AS P WHERE P.ProductID = DI.ProductID) AS [Color], " +
                              "CONVERT(NVARCHAR(20), (SELECT MAX(UDR_CreatedDate.CreatedDate) FROM UserDeviceReads AS UDR_CreatedDate WHERE UDR_CreatedDate.DeviceID = UDR.DeviceID), 101) AS [MostRecentExposureDate], " +
                              //"CONVERT(NVARCHAR(20), (SELECT MAX(UDR_ReviewedDate.ReviewedDate) FROM UserDeviceReads AS UDR_ReviewedDate WHERE UDR_ReviewedDate.DeviceID = UDR.DeviceID), 101) AS [MostRecentReviewedDate], " +
                              "(SELECT MAX(UDR_ReviewedDate.ReviewedDate) FROM UserDeviceReads AS UDR_ReviewedDate WHERE UDR_ReviewedDate.DeviceID = UDR.DeviceID) AS [MostRecentReviewedDate], " +
                              "ISNULL((SELECT top 1 R.CreatedDate FROM [dbo].[rma_Returns] AS R INNER JOIN [dbo].[rma_ReturnDevices] AS RD ON R.ReturnID = RD.ReturnID where R.AccountID = UDR.AccountID and R.ReturnTypeID = 3 And R.Active = 1 And RD.Active = 1 And RD.SerialNo = DI.SerialNo Order By R.CreatedDate desc), '01/01/1990') AS [MostRecentRecalledDate], " +
                              "UDR.ReviewedBy AS [ReviewedBy] " +
                              //"ISNULL(DATEDIFF(DAY, (SELECT MAX(UDR_ReviewedDate.ReviewedDate) FROM UserDeviceReads AS UDR_ReviewedDate WHERE UDR_ReviewedDate.DeviceID = UDR.DeviceID), GETDATE()), 0) AS [DaysSinceReviewed] " +
                              "FROM DeviceInventory AS DI with (nolock) " +
                                    "INNER JOIN UserDeviceReads AS UDR with (nolock) ON DI.DeviceID = UDR.DeviceID " +
                                    "INNER JOIN Accounts AS ACCT with (nolock) ON UDR.AccountID = ACCT.AccountID " +
                                    "INNER JOIN Users AS USRS with (nolock) ON UDR.UserID = USRS.UserID " +
                                    "INNER JOIN Products AS P with (nolock) ON P.ProductID = DI.ProductID " +
                              "WHERE UDR.RID > " + startRID + " " +
                                    "AND UDR.AnalysisActionTypeID = 2 " +
                                    "AND UDR.IsAnalyzed = 1 " +
                                    "AND UDR.ReviewedDate IS NOT NULL " +
                                    "AND UDR.ReviewedBy IS NOT NULL " +
                               "ORDER BY MostRecentReviewedDate	DESC";
            //"AND DI.SerialNo NOT IN (SELECT SerialNo FROM [dbo].[rma_Returns] AS R INNER JOIN [dbo].[rma_ReturnDevices] AS RD ON R.ReturnID = RD.ReturnID)" +
            //"AND DI.SerialNo NOT IN (SELECT SerialNo FROM [dbo].[rma_ReturnInventory] AS RI)";

            // Pass the SQL Query String to the SQL Command.
            sqlCmd.Connection = sqlConn;
            bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
            if (!openConn) sqlCmd.Connection.Open();
            sqlCmd.CommandText = sqlQuery;
            SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

            // Create a Date Table to hold the contents/results.
            DataTable dtRecallListDetails = new DataTable();

            dtRecallListDetails = new DataTable("Get Recall List Details");

            // Create the columns for the DataTable.
            dtRecallListDetails.Columns.Add("SerialNumber", Type.GetType("System.String"));
            dtRecallListDetails.Columns.Add("DeviceID", Type.GetType("System.Int32"));
            dtRecallListDetails.Columns.Add("AccountID", Type.GetType("System.Int32"));
            dtRecallListDetails.Columns.Add("CompanyName", Type.GetType("System.String"));
            dtRecallListDetails.Columns.Add("ProductType", Type.GetType("System.String"));
            dtRecallListDetails.Columns.Add("UserID", Type.GetType("System.Int32"));
            dtRecallListDetails.Columns.Add("AssignedTo", Type.GetType("System.String"));
            dtRecallListDetails.Columns.Add("Color", Type.GetType("System.String"));
            dtRecallListDetails.Columns.Add("MostRecentExposureDate", Type.GetType("System.DateTime"));
            dtRecallListDetails.Columns.Add("MostRecentReviewedDate", Type.GetType("System.DateTime"));
            dtRecallListDetails.Columns.Add("ReviewedBy", Type.GetType("System.String"));
            dtRecallListDetails.Columns.Add("DaysSinceReviewed", Type.GetType("System.String"));

            DateTime recentReviewedDate = DateTime.Now;
            DateTime recentRecalledDate;

            int totalNumberOfDays = 0;
            int totalYears = 0;
            int totalMonths = 0;
            int remainingDays = 0;
            int cnt = 0;
            int displayRow = 100;

            string displayTotalDuration = "";

            while (sqlDataReader.Read())
            {
                // Modified by: TDO, modified date: 08/19/2019
                DateTime.TryParse(sqlDataReader["MostRecentReviewedDate"].ToString(), out recentReviewedDate);
                DateTime.TryParse(sqlDataReader["MostRecentRecalledDate"].ToString(), out recentRecalledDate);

                // Only display a SN which has never been recalled after analysis reviewing
                if (recentReviewedDate > recentRecalledDate)
                {
                    DataRow drow = dtRecallListDetails.NewRow();

                    totalNumberOfDays = Convert.ToInt32((DateTime.Now - recentReviewedDate).TotalDays);
                    //int.TryParse(sqlDataReader["DaysSinceReviewed"].ToString(), out totalNumberOfDays);
                    totalYears = (totalNumberOfDays / 365);
                    totalMonths = ((totalNumberOfDays % 365) / 30);
                    remainingDays = ((totalNumberOfDays % 365) % 30);

                    displayTotalDuration = string.Format("{0}Y {1}M {2}D", totalYears, totalMonths, remainingDays);

                    // Fill row details.
                    drow["SerialNumber"] = sqlDataReader["SerialNumber"];
                    drow["DeviceID"] = sqlDataReader["DeviceID"];
                    drow["AccountID"] = sqlDataReader["AccountID"];
                    drow["CompanyName"] = sqlDataReader["CompanyName"];
                    drow["ProductType"] = sqlDataReader["ProductType"];
                    drow["UserID"] = sqlDataReader["UserID"];
                    drow["AssignedTo"] = sqlDataReader["AssignedTo"];
                    drow["Color"] = sqlDataReader["Color"];
                    drow["MostRecentExposureDate"] = sqlDataReader["MostRecentExposureDate"];
                    drow["MostRecentReviewedDate"] = sqlDataReader["MostRecentReviewedDate"];
                    drow["ReviewedBy"] = sqlDataReader["ReviewedBy"];
                    drow["DaysSinceReviewed"] = displayTotalDuration;

                    // Add rows to DataTable.
                    dtRecallListDetails.Rows.Add(drow);

                    cnt++;

                    // Display only the last 100 potential recall
                    if (cnt == displayRow) break;
                }
            }

            // Set the Data to Session.
            Session["RecallList"] = dtRecallListDetails;

            sqlConn.Close();
            sqlDataReader.Close();
        }

        // Bind the results to the RadGrid.
        this.rgRecallList.DataSource = Session["RecallList"];
    }

    protected void rgRecallList_ItemCommand(object sender, Telerik.Web.UI.GridCommandEventArgs e)
    {
        if (!this.rgRecallList.ExportSettings.IgnorePaging) this.rgRecallList.Rebind();
        this.rgRecallList.Rebind();
    }

    /// <summary>
    /// Marks Recall Records to either Create or View RMA.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void rgRecallList_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            string color = "";

            GridDataItem gdi = (GridDataItem)e.Item;

            // Get Color (value).
            TableCell tcColor = gdi["Color"];
            color = tcColor.Text;

            // Set Color to TableCell.
            tcColor.ForeColor = System.Drawing.Color.FromName(color);
        }
    }

    /// <summary>
    /// Export Recall List RadGrid to Excel Spreadsheet.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void lnkbtnExportToExcel_RecallList_Click(object sender, EventArgs e)
    {
        try
        {
            this.rgRecallList.ExportSettings.ExportOnlyData = true;
            this.rgRecallList.ExportSettings.IgnorePaging = true;
            this.rgRecallList.ExportSettings.OpenInNewWindow = true;
            this.rgRecallList.ExportSettings.FileName = string.Format("{0}_{1}", "RecallList", DateTime.Now.ToShortDateString());
            this.rgRecallList.MasterTableView.ExportToExcel();
        }
        catch (Exception ex)
        {
            VisibleErrors(ex.ToString());
            return;
        }
    }

    /// <summary>
    /// Refreshes/Rebinds rgRecallList (RadGrid).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void lnkbtnRefreshData_RecallList_Click(object sender, EventArgs e)
    {
        this.rgRecallList.Rebind();
    }

    /// <summary>
    /// Clear Filters on rgRecallList (RadGrid).
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    protected void lnkbtnClearFilters_RecallList_Click(object sender, EventArgs e)
    {
        foreach (GridColumn column in this.rgRecallList.MasterTableView.OwnerGrid.Columns)
        {
            column.CurrentFilterFunction = GridKnownFunction.NoFilter;
            column.CurrentFilterValue = string.Empty;
        }
        this.rgRecallList.MasterTableView.FilterExpression = string.Empty;
        this.rgRecallList.Rebind();
    }


    // Reset Error Message.

    private void InvisibleErrors()
    {
        this.spnErrorMessage.InnerText = "";
        this.divErrorMessage.Visible = false;
    }

    // Set Error Message.
    private void VisibleErrors(string errormessage)
    {
        this.spnErrorMessage.InnerText = errormessage;
        this.divErrorMessage.Visible = true;
    }

    private int GetStartRID()
    {
        var result = _defaultRIDStart;
        var startRIDAppSetting = idc.AppSettings.FirstOrDefault(x => x.AppKey == "StartRIDForReadAnalysis");
        if (startRIDAppSetting != null)
        {
            result = int.Parse(startRIDAppSetting.Value);
        }
        return result;
    }

}