using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Instadose;
using Instadose.Data;
using Instadose.Device;

public partial class TechOps_DeviceWarehouseTransferReport : System.Web.UI.Page
{
    InsDataContext idc = new InsDataContext();
    protected void Page_Load(object sender, EventArgs e)
    {
        // Grab the ID if it was passed it the query string
        if (Request.QueryString["fromDate"] == null || Request.QueryString["toDate"] == null)
            return;        

        DateTime fromDate = DateTime.Parse(Request.QueryString["fromDate"]);
        DateTime toDate = DateTime.Parse(Request.QueryString["toDate"]).AddDays(1);

        //-------------------------------------------------------------------------------//
        var deviceTransferList = (from dmh in idc.DeviceMovementHistories
                                  join di in idc.DeviceInventories on dmh.DeviceID equals di.DeviceID
                                  join wms in idc.WarehouseMovementSettings on dmh.WarehouseMovementID equals wms.WarehouseMovementID
                                  join dasFrom in idc.DeviceAnalysisStatus on wms.FromDeviceAnalysisStatusID equals dasFrom.DeviceAnalysisStatusID
                                  join dasTo in idc.DeviceAnalysisStatus on wms.ToDeviceAnalysisStatusID equals dasTo.DeviceAnalysisStatusID
                                  where dmh.CreatedDate >= fromDate && dmh.CreatedDate < toDate
                                  orderby dmh.CreatedDate ascending
                                  select new
                                  {
                                      ScanDate = dmh.CreatedDate,
                                      SerialNo = di.SerialNo,
                                      FromWarehouse = dasFrom.Warehouse,
                                      FromStatus = dasFrom.DeviceAnalysisName,
                                      ToWarehouse = dasTo.Warehouse,
                                      ToStatus = dasTo.DeviceAnalysisName
                                  }).ToList();

        // Create an data table used to export.
        DataTable exportTable = new DataTable();

        //Create columns for this DataTable.
        DataColumn colScanDate = new DataColumn("Scan Date");
        DataColumn colSerialNo = new DataColumn("Serial No");
        DataColumn colFromWarehouse = new DataColumn("From Warehouse");
        DataColumn colFromStatus = new DataColumn("From Device Analysis Status");
        DataColumn colToWarehouse = new DataColumn("To Warehouse");
        DataColumn colToStatus = new DataColumn("To Device Analysis Status");

        //Define DataType of the Columns.
        colScanDate.DataType = System.Type.GetType("System.String");
        colSerialNo.DataType = System.Type.GetType("System.String");
        colFromWarehouse.DataType = System.Type.GetType("System.String");
        colFromStatus.DataType = System.Type.GetType("System.String");
        colToWarehouse.DataType = System.Type.GetType("System.String");
        colToStatus.DataType = System.Type.GetType("System.String");

        //Add All These Columns into exportTable.
        exportTable.Columns.Add(colScanDate);
        exportTable.Columns.Add(colSerialNo);
        exportTable.Columns.Add(colFromWarehouse);
        exportTable.Columns.Add(colFromStatus);
        exportTable.Columns.Add(colToWarehouse);
        exportTable.Columns.Add(colToStatus);

        //// Add the rows from the account renewals list
        foreach (var deviceTransfer in deviceTransferList)
        {
            // Create a new table row
            DataRow dr = exportTable.NewRow();

            dr[colScanDate] = deviceTransfer.ScanDate.ToString();
            dr[colSerialNo] = deviceTransfer.SerialNo;
            dr[colFromWarehouse] = deviceTransfer.FromWarehouse;
            dr[colFromStatus] = deviceTransfer.FromStatus;
            dr[colToWarehouse] = deviceTransfer.ToWarehouse;
            dr[colToStatus] = deviceTransfer.ToStatus;

            // Add the row to the table.
            exportTable.Rows.Add(dr);
        }

        // Build the export table.
        TableExport tableExport = new TableExport(exportTable);

        try
        {
            // Read the CSS template from file.
            tableExport.Stylesheet = System.IO.File.ReadAllText(Server.MapPath("~/css/export/grids.css"));
        }
        catch (Exception ex)
        {
            throw new Exception(string.Format("Error: {0}", ex.Message));
        }

        try
        {
            // Create the export file based on the selected value.
            tableExport.Export("WarehouseTransfer", "XLS");

            ExportFile file = tableExport.File;

            // Clear everything out.
            Response.Clear();
            Response.ClearHeaders();

            // Set the response headers.
            Response.ContentType = file.ContentType;
            Response.AddHeader("Content-Disposition", file.ContentDisposition);

            //Write to Excel file.
            if (file.Content.GetType() == typeof(byte[]))
                Response.BinaryWrite((byte[])file.Content);
            else
                Response.Write(file.Content.ToString());
        }
        catch (Exception ex)
        {
            throw new Exception(string.Format("Error: {0}", ex.Message));
        }

        Response.Flush();
        Response.End();
    }
}