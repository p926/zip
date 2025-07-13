using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.IO;
using System.IO.MemoryMappedFiles;

using System.Data;

using System.Linq;
using System.Reflection;

[Serializable()]
public class ExportFile
{
    public Guid GUID { get; set; }
    public string ContentType { get; set; }
    public string ContentDisposition { get; set; }
    public object Content { get; set; }
}

/// <summary>
/// Export a data table to a file.
/// </summary>
public class TableExport
{
    public string Stylesheet { get; set; }
    public ExportFile File { get; set; }
    public string Header { get; set; }
    
    private DataTable _table;
    
    public TableExport(DataTable table)
    {
        _table = table;
    }

    public Guid Export(string filename, string type)
    {
        if (type == "XLS") return ExportToExcel(filename + ".xls");
        //else if (type == "PDF") return ExportToPDF(filename + ".pdf");
        else if (type == "CSV") return ExportToCSV(filename + ".csv");
        else return ExportToHtml(filename + ".html");
    }

    public Guid ExportToExcel(string filename)
    {
        File = new ExportFile()
        {
            ContentType = "application/vnd.ms-excel",
            ContentDisposition = "attachment;filename=\"" + filename + "\"",
            GUID = Guid.NewGuid()
        };

        StringWriter sw = new StringWriter();
        sw.WriteLine(@"<style> .textmode { mso-number-format:\@; } </style>");

        if (Stylesheet != string.Empty)
        {
            // Add the stylesheet
            sw.WriteLine(@"<style>{0}</style>", Stylesheet);
        }

        // Append the table
        sw.WriteLine(tableToHtml());

        // Set the file content
        File.Content = sw.ToString();
        
        // Return the file
        return File.GUID;
    }

    public Guid ExportToHtml(string filename)
    {
        File = new ExportFile()
        {
            ContentType = "application/force-download",
            ContentDisposition = "attachment;filename=\"" + filename + "\"",
            GUID = Guid.NewGuid()
        };


        StringWriter sw = new StringWriter();

        sw.WriteLine("<html>");
        sw.WriteLine("<head>");
        sw.WriteLine("  <meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-16\" />");
        if (Stylesheet != string.Empty)
        {
            // Add the stylesheet
            sw.WriteLine("  <style type=\"text/css\">\r\n{0}\r\n</style>", Stylesheet);
        }
        sw.WriteLine("</head>");
        sw.WriteLine("<body>");

        // Append the table
        sw.WriteLine(tableToHtml());

        sw.WriteLine("</body>");
        sw.WriteLine("</html>");

        // Set the file content
        File.Content = sw.ToString();


        // Return the file
        return File.GUID;
    }

    private string tableToHtml()
    {
        StringBuilder sb = new StringBuilder();

        if(Header != null && Header != string.Empty)
            sb.AppendFormat("<h2>{0}</h2>", Header);

        // Append the table open
        sb.AppendFormat("<table id=\"{0}\" class=\"table\">\r\n", _table.TableName);

        // Begin header row
        sb.AppendLine("  <tr class=\"heading\">");

        // Loop through each column in the hearder
        for (int k = 0; k < _table.Columns.Count; k++)
        {
            // Append the column name
            sb.AppendFormat("    <th>{0}</th>\r\n", _table.Columns[k].ColumnName);
        }

        // End header row
        sb.AppendLine("  </tr>");

        // Loop through each row
        for (int i = 0; i < _table.Rows.Count; i++)
        {
            // Set the alternate row color
            string rowStyle = "row" + ((i % 2 == 1) ? "-alternate" : "");

            // Loop through each cell
            sb.AppendFormat("  <tr class=\"{0}\">\r\n", rowStyle);
            for (int k = 0; k < _table.Columns.Count; k++)
            {
                // Append column
                sb.AppendFormat("    <td>{0}</td>\r\n", _table.Rows[i][k].ToString());
            }

            // Append close row end tag
            sb.Append("  </tr>\r\n");
        }

        // Append close table tag
        sb.Append("</table>\r\n");

        // Return the html
        return sb.ToString();
    }

    public Guid ExportToCSV(string filename)
    {
        File = new ExportFile()
        {
            ContentType = "application/octet-stream",
            ContentDisposition = "attachment;filename=\"" + filename + "\"",
            GUID = Guid.NewGuid()
        };

        StringBuilder sb = new StringBuilder();
        for (int k = 0; k < _table.Columns.Count; k++)
        {
            //add separator
            sb.Append(_table.Columns[k].ColumnName + ',');
        }
        // remove last comma
        sb.Remove(sb.Length - 1, 1);


        //append new line
        sb.Append("\r\n");

        for (int i = 0; i < _table.Rows.Count; i++)
        {
            for (int k = 0; k < _table.Columns.Count; k++)
            {
                //add separator
                sb.Append(_table.Rows[i][k].ToString().Replace(",", "") + ',');
            }
            // remove last comma
            sb.Remove(sb.Length - 1, 1);

            //append new line
            sb.Append("\r\n");
        }
        File.Content = sb.ToString();
        
        // Return the file
        return File.GUID;
    }
}