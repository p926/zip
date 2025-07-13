using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Web.Script.Serialization;
using System.Drawing;
using System.Runtime.Serialization;

/// <summary>
/// High Charts Helper provides methods for converting .NET data types to High Charts JSON.
/// </summary>
public class HighChartHelper
{

    public static string DataTableToSeries(DataTable table)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder("[");

        int rowCount = 0;

        foreach (DataRow row in table.Rows)
        {
            if (table.Columns.Count > 1) sb.Append("[");
            for (int i = 0; i < table.Columns.Count; ++i)
            {
                if (table.Columns[i].DataType == typeof(string))
                    sb.AppendFormat("'{0}',", row[i]);
                else
                    sb.AppendFormat("{0},", row[i]);
            }
            sb.Remove(sb.Length - 1, 1);

            if (table.Columns.Count > 1) sb.Append("],");
            else sb.Append(",");

            rowCount++;
        }

        // Remove the extra comma.
        if (sb.Length > 1) sb.Remove(sb.Length - 1, 1);

        sb.Append("]");

        string series = sb.ToString();

        if (rowCount == 1)
            series = series.Substring(1, series.Length - 2);

        return series;
    }

    /// <summary>
    /// Converts a single column of data into JSON based string.
    /// </summary>
    /// <param name="table"></param>
    /// <param name="columnName"></param>
    /// <returns></returns>
    public static string ExtractSeriesFromDataTable(DataTable table, string columnName)
    {
        // Select the old column
        DataColumn col = table.Columns[columnName];

        // Build a new datatable with only the one column
        DataTable dt = new DataTable();
        dt.Columns.Add(new DataColumn(columnName, col.DataType));

        // Fill the new table with the data
        foreach (DataRow row in table.Rows)
        {
            DataRow r = dt.NewRow();
            r[columnName] = row[columnName];
            dt.Rows.Add(r);
        }

        // Convert the table to the series.
        return DataTableToSeries(dt);
    }
}

public class HighChartSeries
{
    [DataContract]
    public enum DashStyleType
    {
        [EnumMember(Value = "Solid")]
        Solid,
        [EnumMember(Value = "ShortDash")]
        ShortDash,
        [EnumMember(Value = "ShortDot")]
        ShortDot,
        [EnumMember(Value = "ShortDashDot")]
        ShortDashDot,
        [EnumMember(Value = "ShortDashDotDot")]
        ShortDashDotDot,
        [EnumMember(Value = "Dot")]
        Dot,
        [EnumMember(Value = "Dash")]
        Dash,
        [EnumMember(Value = "LongDash")]
        LongDash,
        [EnumMember(Value = "DashDot")]
        DashDot,
        [EnumMember(Value = "LongDashDot")]
        LongDashDot,
        [EnumMember(Value = "LongDashDotDot")]
        LongDashDotDot
    }

    public string name { get; set; }
    public string color { get; set; }
    public string dashStyle { get; set; }
    public float[] data { get; set; }
    public bool enableMouseTracking { get; set; }
    public bool showInLegend { get; set; }
    public int zIndex { get; set; }
    public bool redraw { get; set; }
    public int yAxis { get; set; }
    public HighChartMarker marker { get; set; }

    public HighChartSeries()
    {
        this.enableMouseTracking = true;
        this.showInLegend = true;
        this.zIndex = 0;
        this.marker = new HighChartMarker();
    }

    public HighChartSeries(string name) : this()
    {
        this.name = name;
    }

    public HighChartSeries(string name, float[] data) : this(name)
    {
        this.data = data;
    }
    public HighChartSeries(string name, float[] data, int yAxis) : this(name, data)
    {
        this.yAxis = yAxis;
    }

}

public class HighChartAxis
{
    public string[] categories { get; set; }

    public HighChartAxis()
    {
    }

    public HighChartAxis(string[] categories)
    {
        this.categories = categories;
    }
}

public class HighChartData
{
    public HighChartAxis axis { get; set; }
    public List<HighChartSeries> series { get; set; }

    public HighChartData()
    {
        axis = new HighChartAxis();
        series = new List<HighChartSeries>();
    }

    public HighChartData(string[] categories)
    {
        axis = new HighChartAxis(categories);
        series = new List<HighChartSeries>();
    }
}

public class HighChartMarker
{
    public bool enabled { get; set; }
    public string fillColor { get; set; }
    public string lineColor { get; set; }
    public int lineWidth { get; set; }
    public int radius { get; set; }
    public string symbol { get; set; }

    public HighChartMarker()
    {
        this.enabled = true;
        this.radius = 4;
    }
}









