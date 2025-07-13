using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

public partial class ReadAnalysisSummaryReports_TechOps : System.Web.UI.Page
{
    // Assign/Initialize ReadAnalysisLogDetailID.
    public int readAnalysisLogDetailID = 0;

    #region ON PAGE LOAD.
    protected void Page_Load(object sender, EventArgs e)
    {
        int.TryParse(Request.QueryString["LogID"], out readAnalysisLogDetailID);

        if (readAnalysisLogDetailID == 0) Response.Redirect("ReadAnalysis.aspx#SummaryReports_tab");

        if (Page.IsPostBack) return;

        Session["SummaryReport"] = null;

        rgExportToExcel.DataBind();
    }
    #endregion

    #region RGEXPORTTOEXCEL-RELATED FUNCTIONS.
    public void GetReadAnalysisSummaryReportDetails(int readanalysislogdetailid)
    {
        try
        {
            if (Session["SummaryReport"] == null)
            {
                // Create the SQL Connection from the connection string in the web.config
                SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
                    ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

                // Create SQL Command.
                SqlCommand sqlCmd = new SqlCommand();
                sqlCmd.CommandType = CommandType.StoredProcedure;
                sqlCmd.CommandText = "sp_ReadAnalysisSummaryReport";

                sqlCmd.Parameters.Add(new SqlParameter("@pReadAnalysisLogDetailID", readanalysislogdetailid));
                sqlCmd.Parameters.Add(new SqlParameter("@pCreatedDateFrom", DBNull.Value));
                sqlCmd.Parameters.Add(new SqlParameter("@pCreatedDateTo", DBNull.Value));
                sqlCmd.Parameters.Add(new SqlParameter("@pAnalyzedDateFrom", DBNull.Value));
                sqlCmd.Parameters.Add(new SqlParameter("@pAnalyzedDateTo", DBNull.Value));
                sqlCmd.Parameters.Add(new SqlParameter("@pReviewedDateFrom", DBNull.Value));
                sqlCmd.Parameters.Add(new SqlParameter("@pReviewedDateTo", DBNull.Value));
                sqlCmd.Parameters.Add(new SqlParameter("@pAnalyzedOrReviewedBy", DBNull.Value));
                sqlCmd.Parameters.Add(new SqlParameter("@pAccountID", DBNull.Value));
                sqlCmd.Parameters.Add(new SqlParameter("@pLocationID", DBNull.Value));
                sqlCmd.Parameters.Add(new SqlParameter("@pUserID", DBNull.Value));
                sqlCmd.Parameters.Add(new SqlParameter("@pSerialNumber", DBNull.Value));
                sqlCmd.Parameters.Add(new SqlParameter("@pReadTypeID", DBNull.Value));
                sqlCmd.Parameters.Add(new SqlParameter("@pInitialRead", DBNull.Value));
                sqlCmd.Parameters.Add(new SqlParameter("@pHasAnomaly", DBNull.Value));
                sqlCmd.Parameters.Add(new SqlParameter("@pAnalysisActionTypeID", DBNull.Value));
                sqlCmd.Parameters.Add(new SqlParameter("@pAnalysisTypeAbbrev", DBNull.Value));
                sqlCmd.Parameters.Add(new SqlParameter("@pDeepLowDoseFrom", DBNull.Value));
                sqlCmd.Parameters.Add(new SqlParameter("@pDeepLowDoseTo", DBNull.Value));
                sqlCmd.Parameters.Add(new SqlParameter("@pShallowLowDoseFrom", DBNull.Value));
                sqlCmd.Parameters.Add(new SqlParameter("@pShallowLowDoseTo", DBNull.Value));
                sqlCmd.Parameters.Add(new SqlParameter("@pEyeDoseFrom", DBNull.Value));
                sqlCmd.Parameters.Add(new SqlParameter("@pEyeDoseTo", DBNull.Value));
                sqlCmd.Parameters.Add(new SqlParameter("@pDeepLowDoseCalculationFrom", DBNull.Value));
                sqlCmd.Parameters.Add(new SqlParameter("@pDeepLowDoseCalculationTo", DBNull.Value));
                sqlCmd.Parameters.Add(new SqlParameter("@pCumulativeDoseFrom", DBNull.Value));
                sqlCmd.Parameters.Add(new SqlParameter("@pCumulativeDoseTo", DBNull.Value));

                // Pass the SQL Query String to the SQL Command.
                sqlCmd.Connection = sqlConn;
                bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
                if (!openConn) sqlCmd.Connection.Open();
                SqlDataReader sqlDtRdr = sqlCmd.ExecuteReader();

                // Create the DataTable to hold the contents/results.
                DataTable dtExportToExcel = new DataTable("Read Analysis Summary Report");

                dtExportToExcel.Columns.Add("ReadAnalysisLogDetailID", Type.GetType("System.Int32"));
                dtExportToExcel.Columns.Add("RID", Type.GetType("System.Int32"));
                dtExportToExcel.Columns.Add("SerialNumber", Type.GetType("System.String"));
                dtExportToExcel.Columns.Add("AccountID", Type.GetType("System.Int32"));
                dtExportToExcel.Columns.Add("CompanyName", Type.GetType("System.String"));
                dtExportToExcel.Columns.Add("LocationID", Type.GetType("System.Int32"));
                dtExportToExcel.Columns.Add("LocationName", Type.GetType("System.String"));
                dtExportToExcel.Columns.Add("UserID", Type.GetType("System.Int32"));
                dtExportToExcel.Columns.Add("AssignedTo", Type.GetType("System.String"));
                dtExportToExcel.Columns.Add("CreatedDate", Type.GetType("System.DateTime"));
                dtExportToExcel.Columns.Add("ReadTypeName", Type.GetType("System.String"));
                dtExportToExcel.Columns.Add("InitialRead", Type.GetType("System.String"));
                dtExportToExcel.Columns.Add("HasAnomaly", Type.GetType("System.String"));
                dtExportToExcel.Columns.Add("DLDCalculation", Type.GetType("System.Decimal"));
                dtExportToExcel.Columns.Add("CumulativeDose", Type.GetType("System.Decimal"));
                dtExportToExcel.Columns.Add("Deep", Type.GetType("System.String"));
                dtExportToExcel.Columns.Add("Shallow", Type.GetType("System.String"));
                dtExportToExcel.Columns.Add("Eye", Type.GetType("System.String"));
                dtExportToExcel.Columns.Add("AnalyzedDate", Type.GetType("System.DateTime"));
                dtExportToExcel.Columns.Add("AnalyzedBy", Type.GetType("System.String"));
                dtExportToExcel.Columns.Add("ReviewedDate", Type.GetType("System.DateTime"));
                dtExportToExcel.Columns.Add("ReviewedBy", Type.GetType("System.String"));
                dtExportToExcel.Columns.Add("FailedTests", Type.GetType("System.String"));
                dtExportToExcel.Columns.Add("ActionTaken", Type.GetType("System.String"));

                while (sqlDtRdr.Read())
                {
                    DataRow dtrw = dtExportToExcel.NewRow();

                    dtrw["ReadAnalysisLogDetailID"] = sqlDtRdr["ReadAnalysisLogDetailID"];
                    dtrw["RID"] = sqlDtRdr["RID"];
                    dtrw["SerialNumber"] = sqlDtRdr["SerialNumber"];
                    dtrw["AccountID"] = sqlDtRdr["AccountID"];
                    dtrw["CompanyName"] = sqlDtRdr["CompanyName"];
                    dtrw["LocationID"] = sqlDtRdr["LocationID"];
                    dtrw["LocationName"] = sqlDtRdr["LocationName"];
                    dtrw["UserID"] = sqlDtRdr["UserID"];
                    dtrw["AssignedTo"] = sqlDtRdr["AssignedTo"];
                    dtrw["CreatedDate"] = sqlDtRdr["CreatedDate"];
                    dtrw["ReadTypeName"] = sqlDtRdr["ReadTypeName"];
                    dtrw["InitialRead"] = sqlDtRdr["InitialRead"];
                    dtrw["HasAnomaly"] = sqlDtRdr["HasAnomaly"];
                    dtrw["DLDCalculation"] = sqlDtRdr["DLDCalculation"];
                    dtrw["CumulativeDose"] = sqlDtRdr["CumulativeDose"];
                    dtrw["Deep"] = sqlDtRdr["Deep"];
                    dtrw["Shallow"] = sqlDtRdr["Shallow"];
                    dtrw["Eye"] = sqlDtRdr["Eye"];
                    dtrw["AnalyzedDate"] = sqlDtRdr["AnalyzedDate"];
                    dtrw["AnalyzedBy"] = sqlDtRdr["AnalyzedBy"];
                    dtrw["ReviewedDate"] = sqlDtRdr["ReviewedDate"];
                    dtrw["ReviewedBy"] = sqlDtRdr["ReviewedBy"];
                    dtrw["FailedTests"] = sqlDtRdr["FailedTests"];
                    dtrw["ActionTaken"] = sqlDtRdr["ActionTaken"];

                    dtExportToExcel.Rows.Add(dtrw);
                }

                Session["SummaryReport"] = dtExportToExcel;

                sqlConn.Close();
                sqlDtRdr.Close();
            }

            // Bind the results to the RadGrid.
            this.rgExportToExcel.DataSource = Session["SummaryReport"];
        }
        catch (Exception ex)
        {
            VisibleErrors_MainPageContent(ex.ToString());
            return;
        }
    }

    /// <summary>
    /// SQL Connection and Statement that displays data in RadGrid.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    protected void rgExportToExcel_NeedDataSource(object source, GridNeedDataSourceEventArgs e)
    {
        GetReadAnalysisSummaryReportDetails(readAnalysisLogDetailID);
    }

    /// <summary>
    /// Rebinds to RadGrid upon filtering.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void rgExportToExcel_ItemCommand(object sender, Telerik.Web.UI.GridCommandEventArgs e)
    {
        if (!this.rgExportToExcel.ExportSettings.IgnorePaging) this.rgExportToExcel.Rebind();

        this.rgExportToExcel.Rebind();
    }

    /// <summary>
    /// Color-Code records based upon "Action" column value.
    /// None - Black (Default)
    /// Recall - Red
    /// Watchlist - Blue
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void rgExportToExcel_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem gdi = (GridDataItem)e.Item;
            TableCell action = gdi["ActionTaken"];

            // IsCompleted (TRUE/FALSE).
            if (action.Text == "Recall")
            {
                gdi.ForeColor = System.Drawing.Color.Red;
            }
            else if (action.Text == "Watchlist")
            {
                gdi.ForeColor = System.Drawing.Color.Blue;
            }
            else
            {
                gdi.ForeColor = System.Drawing.Color.Black;
            }
        }
    }
    #endregion

    #region EXPORT TO EXCEL FUNCTIONS.
    protected void lnkbtnExportToExcel_Click(object sender, EventArgs e)
    {
        // Rebind to the correct results.
        try
        {
            if (Request.QueryString["LogID"] != null || Request.QueryString["LogID"] != "0")
            {
                int.TryParse(Request.QueryString["LogID"], out readAnalysisLogDetailID);
                GetReadAnalysisSummaryReportDetails(readAnalysisLogDetailID);

                this.rgExportToExcel.ExportSettings.ExportOnlyData = true;

                this.rgExportToExcel.MasterTableView.ShowHeader = true;

                this.rgExportToExcel.ExportSettings.HideStructureColumns = true;

                this.rgExportToExcel.ExportSettings.FileName = "ReadAnalysisSummaryReport";

                this.rgExportToExcel.ExportSettings.IgnorePaging = true;

                this.rgExportToExcel.ExportSettings.OpenInNewWindow = true;

                this.rgExportToExcel.ExportSettings.Excel.Format = Telerik.Web.UI.GridExcelExportFormat.ExcelML;

                this.rgExportToExcel.MasterTableView.ExportToExcel();
            }
            else
            {
                return;
            }
        }
        catch (Exception ex)
        {
            VisibleErrors_MainPageContent(ex.ToString());
            return;
        }
    }

    protected void rgExportToExcel_ExportCellFormatting(object sender, ExportCellFormattingEventArgs e)
    {
        GridItem item = e.Cell.Parent as GridItem;
        e.Cell.CssClass = (item.ItemType == GridItemType.Item) ? "excelCss" : "excelAltCss";
    }

    protected void rgExportToExcel_HTMLExporting(object sender, GridHTMLExportingEventArgs e)
    {
        string excelCss = ".table { border: 1px solid #3A6A79; font-family: Arial, Helvetical, Sans-Serif; font-size: 10pt; } ";
        excelCss += ".row { background-color: white; } ";
        string excelAltCss = ".row-alternate { background-color: #D5E3E7; }";
        string excelHeaderCss = ".heading { font-family: Tahoma, Helvetical, Arial, Sans-Serif; background-color: #3A6A79; font-weight: bold; text-align: center; color: White; } ";
        e.Styles.AppendLine(excelCss);
        e.Styles.AppendLine(excelAltCss);
        e.Styles.AppendLine(excelHeaderCss);

        foreach (TableCell cell in this.rgExportToExcel.MasterTableView.GetItems(GridItemType.Header)[0].Cells)
        {
            cell.CssClass = "excelHeaderCss";
        }
    }
    #endregion

    /// <summary>
    /// Return to Read Analysis page.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnReturnToReadAnalysisPage_Click(object sender, EventArgs e)
    {
        Response.Redirect("ReadAnalysis.aspx#SummaryReports_tab");
    }

    #region ERROR MESSAGE(S).
    // Main Page Contnet: Reset Error Message.
    private void InvisibleErrors_MainPageContent()
    {
        this.spnErrorMessage_MainPageContent.InnerText = "";
        this.divErrorMessage_MainPageContent.Visible = false;
    }

    // Main Page Contnet: Set Error Message.
    private void VisibleErrors_MainPageContent(string errormessage)
    {
        this.spnErrorMessage_MainPageContent.InnerText = errormessage;
        this.divErrorMessage_MainPageContent.Visible = true;
    }
    #endregion
}