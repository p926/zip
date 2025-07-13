using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;
using Portal.Instadose;

public partial class Manufacturing_AggregateShippingReport : System.Web.UI.Page
{

    protected void Page_Load(object sender, EventArgs e)
    {
        // Load chart data.

        if (Request.QueryString["chart"] != null)
        {
            loadChartData(Request.QueryString["chart"].ToLower());
            return;
        }

        if (IsPostBack) return;

        DateTime currentDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
        radPicker.SelectedDate = currentDate;

        // Load grid
        bindGrid();

        // Force the monthly to load.
        string loadScript = string.Format("getMonthlyData(new Date({0}, {1}, 0));", currentDate.Year, currentDate.Month);
        ScriptManager.RegisterStartupScript(this, typeof(Page), "loadMonChart", loadScript, true);
    }

    private void bindGrid()
    {

        if (!radPicker.SelectedDate.HasValue) return;

        DataLayer dl = new DataLayer();

        DateTime startPeriod = new DateTime(radPicker.SelectedDate.Value.Year, radPicker.SelectedDate.Value.Month, 1);
        DateTime endPeriod = startPeriod.AddMonths(1).AddDays(-1);

        List<SqlParameter> pars = new List<SqlParameter>();
        pars.Add(new SqlParameter("pStartDate", startPeriod));
        pars.Add(new SqlParameter("pEndDate", endPeriod));

        // Get the dataset
        DataSet dataSet = dl.GetDataSet("sp_GetShipmentSummary", CommandType.StoredProcedure, pars);

        shipGrid.DataSource = dataSet;
        shipGrid.DataBind();
    }

    /// <summary>
    /// Calling this method will clear the response and return ONLY JSON.
    /// </summary>
    /// <param name="chart"></param>
    private void loadChartData(string chart)
    {
        JavaScriptSerializer jss = new JavaScriptSerializer();
        HighChartData data = new HighChartData();
        DataLayer dl = new DataLayer();

        if (chart == "daily")
        {
            DateTime date = DateTime.Today;
            if (Request.QueryString["summary_date"] != null)
                DateTime.TryParse(Request.QueryString["summary_date"], out date);

            // Build the SQL parameters.
            List<SqlParameter> pars = new List<SqlParameter>();
            pars.Add(new SqlParameter("pDate", date));

            // Get the dataset
            DataSet dataSet = dl.GetDataSet("sp_GetDailyShipments", CommandType.StoredProcedure, pars);

            // Set the chart category
            string chartCategory = date.ToString("MM/dd/yyyy");
            if (date == DateTime.Today) chartCategory = "Today";
            else if (date == DateTime.Today.AddDays(-1)) chartCategory = "Yesterday";

            // Build the x axis category
            data.axis = new HighChartAxis(new string[] { chartCategory });
            HighChartSeries sBlack = new HighChartSeries("Black", new float[] { 0 });
            HighChartSeries sBlue = new HighChartSeries("Blue", new float[] { 0 });
            HighChartSeries sGreen = new HighChartSeries("Green", new float[] { 0 });
            HighChartSeries sPink = new HighChartSeries("Pink", new float[] { 0 });
            HighChartSeries sSilver = new HighChartSeries("Silver", new float[] { 0 });

            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                float value = 0;
                float.TryParse(row["Quantity"].ToString(), out value);

                string color = row["Color"].ToString();
                float[] seriesData = new float[] { value };

                switch (color)
                {
                    case "Black":
                        sBlack.data = seriesData;
                        break;
                    case "Blue":
                        sBlue.data = seriesData;
                        break;
                    case "Green":
                        sGreen.data = seriesData;
                        break;
                    case "Pink":
                        sPink.data = seriesData;
                        break;
                    case "Silver":
                        sSilver.data = seriesData;
                        break;
                }
            }

            data.series.Add(sBlack);
            data.series.Add(sBlue);
            data.series.Add(sGreen);
            data.series.Add(sPink);
            data.series.Add(sSilver);
        }
        else if (chart == "monthly")
        {
            DateTime startPeriod = DateTime.Now;
            DateTime endPeriod = DateTime.Now;

            if (Request.QueryString["beginPeriod"] != null)
                DateTime.TryParse(Request.QueryString["beginPeriod"], out startPeriod);

            // Remove the loose days on the start period and 1 month to the end period.
            startPeriod = new DateTime(startPeriod.Year, startPeriod.Month, 1);
            endPeriod = startPeriod.AddMonths(1).AddDays(-1);

            List<SqlParameter> pars = new List<SqlParameter>();
            pars.Add(new SqlParameter("pStartDate", startPeriod));
            pars.Add(new SqlParameter("pEndDate", endPeriod));

            // Get the dataset
            DataSet dataSet = dl.GetDataSet("sp_GetShipmentSummary", CommandType.StoredProcedure, pars);

            // If no data was returned exit.
            if (dataSet.Tables.Count == 0) return;

            // Build the axis.
            DataTable sumTable = dataSet.Tables[0];

            // Build the Axis array
            List<string> categories = new List<string>();
            List<float> series1 = new List<float>();
            List<float> series2 = new List<float>();
            List<float> series3 = new List<float>();
            List<float> series4 = new List<float>();
            List<float> series5 = new List<float>();

            foreach (DataRow row in sumTable.Rows)
            {
                DateTime actualDate = DateTime.Parse(row["DisplayDate"].ToString());

                categories.Add(actualDate.ToString("M/d"));
                series1.Add(int.Parse(row["Black"].ToString()));
                series2.Add(int.Parse(row["Blue"].ToString()));
                series3.Add(int.Parse(row["Green"].ToString()));
                series4.Add(int.Parse(row["Pink"].ToString()));
                series5.Add(int.Parse(row["Silver"].ToString()));
            }

            data.axis = new HighChartAxis(categories.ToArray());
            data.series.Add(new HighChartSeries("Black", series1.ToArray()));
            data.series.Add(new HighChartSeries("Blue", series2.ToArray()));
            data.series.Add(new HighChartSeries("Green", series3.ToArray()));
            data.series.Add(new HighChartSeries("Pink", series4.ToArray()));
            data.series.Add(new HighChartSeries("Silver", series5.ToArray()));
        }

        // Write the json content
        Response.Clear();
        Response.ContentType = "application/json; charset=utf-8";
        Response.Write(jss.Serialize(data));
        Response.End();
    }

    protected void radPicker_SelectedDateChanged(object sender, Telerik.Web.UI.Calendar.SelectedDateChangedEventArgs e)
    {
        bindGrid();

    }
}