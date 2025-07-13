using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

public partial class IT_Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        DateTime beginPeriod = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        DateTime endPeriod = beginPeriod.AddMonths(1);

        // Get the usage for this year
        DataTable table = GetUserUsagePerDayOfWeek(beginPeriod, endPeriod);
        
        // Build the categories
        lblUsagePerDay.Text = HighChartHelper.DataTableToSeries(table);

    }

    /// <summary>
    /// Load the Usage of instadose.com per day of week.
    /// </summary>
    /// <param name="beginPeriod"></param>
    /// <param name="endPeriod"></param>
    /// <returns></returns>
    private DataTable GetUserUsagePerDayOfWeek(DateTime beginPeriod, DateTime endPeriod)
    {
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ToString());
        SqlDataAdapter dataAdapter = new SqlDataAdapter("sp_GetUserUsagePerDayOfWeek", sqlConn);

        // Set the command type as StoredProcedure.
        dataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;

        // Create and add a parameter to Parameters collection for the stored procedure.
        dataAdapter.SelectCommand.Parameters.Add(new SqlParameter("@pBeginPeriod", SqlDbType.Date));
        dataAdapter.SelectCommand.Parameters["@pBeginPeriod"].Value = beginPeriod;

        dataAdapter.SelectCommand.Parameters.Add(new SqlParameter("@pEndPeriod", SqlDbType.Date));
        dataAdapter.SelectCommand.Parameters["@pEndPeriod"].Value = endPeriod;

        DataTable data = new DataTable();
        dataAdapter.Fill(data); // Fill the DataSet with the rows returned.

        dataAdapter.Dispose(); // Dispose of the DataAdapter.
        sqlConn.Close(); // Close the connection.

        return data;
    }

    protected void gvApplicationStatus_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        // Get the status image from the row.
        Image imgStatusIcon = (Image)e.Row.Cells[0].FindControl("imgStatusIcon");

        // Quick and dirty way to skip the Header and Footer rows.
        if (imgStatusIcon == null) return;

        // Get the data row
        DataRowView viewRow = (DataRowView)e.Row.DataItem;
        DateTime lastStatus = (DateTime)viewRow[2];

        // If the last status update was over 24 hours ago, present a warning.
        if (DateTime.Now > lastStatus.AddHours(24))
        {
            // Append a warning to the status message.
            imgStatusIcon.AlternateText += "This has not run in 24 hours.";

            // If the status is not an error, set it to a warning.
            if (imgStatusIcon.ImageUrl != "E") imgStatusIcon.ImageUrl = "W";
        }

        imgStatusIcon.AlternateText = imgStatusIcon.AlternateText.Replace("\r\n", "<br />");

        // Depending on the status
        switch (imgStatusIcon.ImageUrl)
        {
            case "E":
                imgStatusIcon.ImageUrl = "~/images/Fail.png";
                imgStatusIcon.AlternateText = string.Format("Application had an Error: {0}", imgStatusIcon.AlternateText);
                break;
            case "S":
                imgStatusIcon.ImageUrl = "~/images/Success.png";
                imgStatusIcon.AlternateText = "Last run was successful.";
                break;
            case "W":
                imgStatusIcon.ImageUrl = "~/images/Warning.png";
                imgStatusIcon.AlternateText = string.Format("Warning: {0}", imgStatusIcon.AlternateText);
                break;
        }
    }
}