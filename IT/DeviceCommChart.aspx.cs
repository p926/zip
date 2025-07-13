using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.UI.WebControls;
using Instadose.Data;
using Portal.Instadose;
using Telerik.Web.UI;

public partial class IT_DeviceCommChart : System.Web.UI.Page
{
    private InsDataContext idc = new InsDataContext();

    private string serialNumber;
    private string errorMsg = "No records found.";

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.QueryString["SerialNo"] == null) return;
        serialNumber = Request.QueryString["SerialNo"];

        if (Page.IsPostBack) return;

        // On initial PageLoad pass the chart QueryString parameter, only send JSON data.
        getDeviceCommunicationChartData(serialNumber);
    }

    // DEVICE COMMUNICATION DATA HIGHCHART (LOAD).
    private void getDeviceCommunicationChartData(string serialNumber)
    {
        // Query the Stored Procedure.
        DataLayer dlLayer_DeviceCommData = new DataLayer();

        // Create List of all Parameters.
        List<SqlParameter> parameters = new List<SqlParameter>();
        parameters.Add(new SqlParameter("pSerialNo", serialNumber));

        // Get the DataSet.
        DataSet dsResult_DeviceCommData = dlLayer_DeviceCommData.GetDataSet("sp_GetDeviceCommunicationBySerialNo", CommandType.StoredProcedure, parameters);

        // Get the 1st Table.
        DataTable dtDeviceCommData = dsResult_DeviceCommData.Tables[0];

        // If no data was returned exit.
        if (dsResult_DeviceCommData.Tables.Count == 0) return;

        // creating the data set for high charts
        HighChartData hcdData_DeviceCommData = new HighChartData();

        // Create ExposureDates, Series 1, 2, 3 and 4 Lists.
        List<string> readDates = new List<string>();
        List<float> dataSeries1 = new List<float>();
        List<float> dataSeries2 = new List<float>();
        List<float> dataSeries3 = new List<float>();
        List<float> dataSeries4 = new List<float>();
        foreach (DataRow row in dtDeviceCommData.Rows)
        {
            DateTime dtReadDate = DateTime.Parse(row["ReadDate"].ToString());

            readDates.Add(dtReadDate.ToShortDateString());
            dataSeries1.Add(float.Parse(row["BatteryPercentage"].ToString()));
            dataSeries2.Add(float.Parse(row["DoseRecord"].ToString()));
            dataSeries3.Add(float.Parse(row["AdvertCount"].ToString()));
            dataSeries4.Add(float.Parse(row["CPUTemp"].ToString()));
        }

        hcdData_DeviceCommData.axis = new HighChartAxis(readDates.ToArray());
        hcdData_DeviceCommData.series.Add(new HighChartSeries("Battery %", dataSeries1.ToArray())
        {
            marker = new HighChartMarker() { enabled = true },
            enableMouseTracking = true,
            showInLegend = true,
            color = "#333333",
            zIndex = 3
        });
        hcdData_DeviceCommData.series.Add(new HighChartSeries("Dose Record", dataSeries2.ToArray())
        {
            marker = new HighChartMarker() { enabled = false },
            enableMouseTracking = true,
            showInLegend = true,
            color = "#0066CC",
            zIndex = 2
        });
        hcdData_DeviceCommData.series.Add(new HighChartSeries("Advert Count", dataSeries3.ToArray())
        {
            marker = new HighChartMarker() { enabled = false },
            enableMouseTracking = true,
            showInLegend = true,
            color = "#993333",
            zIndex = 1
        });
        hcdData_DeviceCommData.series.Add(new HighChartSeries("Temp.", dataSeries4.ToArray())
        {
            marker = new HighChartMarker() { enabled = false },
            enableMouseTracking = true,
            showInLegend = true,
            color = "#339933",
            zIndex = 0
        });

        JavaScriptSerializer jss = new JavaScriptSerializer();
        // Write the high chart data to the screen content
        Response.Clear();
        Response.ContentType = "application/json; charset=utf-8";
        Response.Write(jss.Serialize(hcdData_DeviceCommData));
        Response.End();
    }
}