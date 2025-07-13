using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

public partial class Finance_Renewal_ForecastChart : System.Web.UI.Page
{
    
    protected void Page_Load(object sender, EventArgs e)
    {
        DataTable table = GetYearToDateForecast();
        // Build the categories
        lblCategories.Text = HighChartHelper.ExtractSeriesFromDataTable(table, "RenewalMonth");
        lblResults.Text = HighChartHelper.ExtractSeriesFromDataTable(table, "MonthlyTotal");
    }

    /// <summary>
    /// Load the Usage of instadose.com per day of week.
    /// </summary>
    /// <param name="beginPeriod"></param>
    /// <param name="endPeriod"></param>
    /// <returns></returns>
    private DataTable GetYearToDateForecast()
    {
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ToString());
        SqlDataAdapter dataAdapter = new SqlDataAdapter("sp_GetYearToDateForecast", sqlConn);

        // Set the command type as StoredProcedure.
        dataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;

        DataTable data = new DataTable();
        dataAdapter.Fill(data); // Fill the DataSet with the rows returned.

        dataAdapter.Dispose(); // Dispose of the DataAdapter.
        sqlConn.Close(); // Close the connection.

        return data;
    }
}