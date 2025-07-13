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

public partial class TechOps_ReadAnalysis : System.Web.UI.Page
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
        Session["ReadAnalysisLogDetails"] = null;
        Session["RecallList"] = null;
        Session["Watchlist"] = null;
        Session["SummaryReports"] = null;
    }
    #endregion

    #region PERFORM READ ANALYSIS TAB.
    /// <summary>
    /// Refreshes/Rebinds RadGrid.
    /// Refreshes/Updates (if applicable) the "Unanalyzed Reads" and "Unreviewed Reads" counters.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void lnkbtnRefreshData_PerformReadAnalysis_Click(object sender, EventArgs e)
    {
        this.rgReadAnalysisLogDetails.Rebind();
    }

    /// <summary>
    /// Clear Filters on rgReadAnalysisLogDetails (RadGrid).
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    protected void lnkbtnClearFilters_PerformReadAnalysis_Click(object sender, EventArgs e)
    {
        foreach (GridColumn column in this.rgReadAnalysisLogDetails.MasterTableView.OwnerGrid.Columns)
        {
            column.CurrentFilterFunction = GridKnownFunction.NoFilter;
            column.CurrentFilterValue = string.Empty;
        }
        this.rgReadAnalysisLogDetails.MasterTableView.FilterExpression = string.Empty;
        this.rgReadAnalysisLogDetails.Rebind();
    }
    #endregion

    #region PERFORM READ ANALYSIS - RADGRID FUNCTIONS.
    /// <summary>
    /// SQL Connection and Statement that displays data in RadGrid.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    protected void rgReadAnalysisLogDetails_NeedDataSource(object source, GridNeedDataSourceEventArgs e)
    {
        if (Session["ReadAnalysisLogDetails"] == null)
        {
            // Create the SQL Connection from the connection string in the web.config
            SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
                ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

            // Create the SQL Command.
            SqlCommand sqlCmd = new SqlCommand();
            sqlCmd.CommandTimeout = 360;  // 5 minutes
            sqlCmd.CommandType = CommandType.Text;

            // SQL Statement gets the following records WHERE;
            //  1.) RID exists in AnalysisReadDetails table.
            //  2.) AccountID is not 2.
            //  3.) ReadAnalysisLogDetailID exists in both ReadAnalysisLogDetails and UserDeviceReads tables.

            string sqlQuery = "SELECT RALD.ReadAnalysisLogDetailID, " +
                              "CASE RALD.ProductGroupID " +
                              " WHEN 1 THEN 'ID1'" +
                              " WHEN 9 THEN 'ID1'" +
                              " WHEN 11 THEN 'ID2'" +
                              " WHEN 10 THEN 'ID Plus'" +
                              " WHEN 18 THEN 'ID VUE'" +
                              "END AS [ProductType], " +
                              "CONVERT(NVARCHAR(20), RALD.AnalyzedDate, 101) AS [RunDate], " +
                              "RALD.AnalyzedDate AS [RunTime], " +
                              "RALD.AnalyzedBy AS [RunBy], " +
                              "RunQuantity, " +
                              "NumberAnalyzed, " +
                              "NumberRemaining " +
                              "FROM ReadAnalysisLogDetails AS RALD " +
                              "WHERE RALD.IsCompleted = 0 AND RunQuantity > 0 " +
                              "ORDER BY RALD.AnalyzedDate DESC";

            // Pass the SQL Query String to the SQL Command.
            sqlCmd.Connection = sqlConn;
            bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
            if (!openConn) sqlCmd.Connection.Open();
            sqlCmd.CommandText = sqlQuery;

            // Create a Date Table to hold the contents/results.
            DataTable dtReadAnalysisLogDetails = new DataTable("Get Read Analysis Log Details");
            dtReadAnalysisLogDetails.Load(sqlCmd.ExecuteReader());

            // Set the Data Session to RadGrid.
            Session["ReadAnalysisLogDetails"] = dtReadAnalysisLogDetails;

            sqlConn.Close();

        }

        // Bind the results to the RadGrid.
        this.rgReadAnalysisLogDetails.DataSource = Session["ReadAnalysisLogDetails"];
    }

    /// <summary>
    /// Defines DateRange values and Rebinds to RadGrid upon filtering.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void rgReadAnalysisLogDetails_ItemCommand(object sender, Telerik.Web.UI.GridCommandEventArgs e)
    {
        this.rgReadAnalysisLogDetails.Rebind();
    }
    #endregion

    #region PERFORM READ ANALYSIS MODAL/DIALOG FUNCTIONS.
    /// <summary>
    /// Resets the Modal/Dialog control values to their defaults.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnCancel_RunReadAnalysis_Click(object sender, EventArgs e)
    {
        this.txtRecallLimit.Text = "-40";
        this.txtWatchLowLimit.Text = "-40";
        this.txtWatchHighLimit.Text = "-15";
        this.txtCumulativeDoseLimit.Text = "8000";
        this.txtExpirationYearsLimit.Text = "5";
    }
    #endregion

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

    #region RUN READ ANALYSIS.
    protected void btnRunReadAnalysis_RunReadAnalysis_Click(object sender, EventArgs e)
    {
        int productTypeId = 9;
        int numberToBeAnalyzed = 0;
        decimal dlDoseCalcLowRange = 0;
        decimal dlDoseCalcHighRange = 0;
        decimal cumulativeDoseMax = 0;
        int deviceExpirationYears = 0;
        int batteryPercentLimit = 0;
        string analyzedBy = "";

        int logID = 0;

        int.TryParse(this.ddlProductType.Text, out productTypeId);
        int.TryParse(this.txtNumberToAnalyze.Text, out numberToBeAnalyzed);
        decimal.TryParse(this.txtWatchLowLimit.Text, out dlDoseCalcLowRange);
        decimal.TryParse(this.txtWatchHighLimit.Text, out dlDoseCalcHighRange);
        decimal.TryParse(this.txtCumulativeDoseLimit.Text, out cumulativeDoseMax);
        int.TryParse(this.txtExpirationYearsLimit.Text, out deviceExpirationYears);
        int.TryParse(this.txtBatteryPercentLimit.Text, out batteryPercentLimit);

        if (User.Identity.Name.IndexOf('\\') > 0)
            analyzedBy = User.Identity.Name.Split('\\')[1];
        else
            analyzedBy = "Testing";

        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = null;

        sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);
        sqlConn.Open();

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand("sp_RunReadAnalysis", sqlConn);
        sqlCmd.CommandTimeout = 0;  // unlimited minutes
        sqlCmd.CommandType = CommandType.StoredProcedure;

        // Add Parameters.
        sqlCmd.Parameters.Add(new SqlParameter("@pProductGroupId", productTypeId));
        sqlCmd.Parameters.Add(new SqlParameter("@pNumberToBeAnalyzed", numberToBeAnalyzed));
        sqlCmd.Parameters.Add(new SqlParameter("@pDLDoseCalcLowRange", dlDoseCalcLowRange));
        sqlCmd.Parameters.Add(new SqlParameter("@pDLDoseCalcHighRange", dlDoseCalcHighRange));
        sqlCmd.Parameters.Add(new SqlParameter("@pCumulativeDoseMax", cumulativeDoseMax));
        sqlCmd.Parameters.Add(new SqlParameter("@pDeviceExpirationYears", deviceExpirationYears));
        sqlCmd.Parameters.Add(new SqlParameter("@pBatteryPercentLimit", batteryPercentLimit));
        sqlCmd.Parameters.Add(new SqlParameter("@pUserName", analyzedBy));

        SqlParameter returnedValue = sqlCmd.Parameters.Add("@opReadAnalysisLogDetailID", SqlDbType.Int);
        returnedValue.Direction = ParameterDirection.ReturnValue;

        try
        {
            sqlCmd.ExecuteNonQuery();

            logID = (int)returnedValue.Value;

            sqlConn.Close();

            // Get the ReadAnalysisLogID from the Stored Procedure and pass into QueryString of URL.
            Response.Redirect(string.Format("ReadAnalysisReview.aspx?LogID={0}", logID));
        }
        catch (Exception ex)
        {
            //ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divReadAnalyisDialog')", true);
            VisibleErrors(ex.ToString());
            return;
        }
    }
    #endregion
}