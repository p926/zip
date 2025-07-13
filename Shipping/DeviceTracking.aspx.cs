using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Instadose;
using Instadose.Data;
using Instadose.Device;
using System.Text;


public partial class Instadose_Shipping_DeviceTracking : System.Web.UI.Page
{
    // Turn on/off email to all others
    //bool DevelopmentServer = false;

    // Create the database reference
    InsDataContext idc = new InsDataContext();
    AppDataContext adc = new AppDataContext();

    // Create data table for devices
    //DataTable deviceTable = null;
    
    // String to hold the current username
    string UserName = "Unknown";

    protected void Page_Load(object sender, EventArgs e)
    {

        if (!IsPostBack)
        {

            var vwarehouse = (from p in idc.sp_getDeviceWareHouse() orderby p.warehouse select p);

            ddlActualWareHouse.DataSource = vwarehouse;
            ddlActualWareHouse.DataTextField = "warehouse";
            ddlActualWareHouse.DataValueField = "warehouse";
            ddlActualWareHouse.DataBind();
            ddlActualWareHouse.Items.Insert(0, "");


            var vwarehouse2 = (from p in idc.sp_getDeviceWareHouse() orderby p.warehouse select p);
            this.ddlsearchActualWarehouse.DataSource = vwarehouse2;
            this.ddlsearchActualWarehouse.DataTextField = "warehouse";
            this.ddlsearchActualWarehouse.DataValueField = "warehouse";
            this.ddlsearchActualWarehouse.DataBind();
            this.ddlsearchActualWarehouse.Items.Insert(0, "");



            if (hdnfldFlashRed.Value != "yes")
                lblErrmessage.Text = "";

            ViewState["sortExpression"] = "";
            ViewState["sortDirection"] = SortDirection.Ascending;


            LoadData();
        }
        else
        {
            lblErrmessage.Text = "";
            this.plInput.BackColor = System.Drawing.Color.LightYellow;

        }
    }

    private void LoadData()
    {

        List <DeviceTracking> deviceTracking = (from p in idc.DeviceTrackings orderby p.DeviceTrackingID descending  select p).ToList();

        gvDeviceInfo.DataSource = deviceTracking;
        gvDeviceInfo.DataBind();

        lbltotalCount.Text = deviceTracking.Count().ToString();

    }


    protected void btnSearch_Click(object sender, EventArgs e)
    {

        searchDevices();

    }

    private void searchDevices()
    {

        string slocation = (txtsearchLocation.Text.Trim() == "") ? "" : txtsearchLocation.Text.Trim();
        string sactualWarehouse = (ddlsearchActualWarehouse.SelectedValue == "") ? "" : ddlsearchActualWarehouse.SelectedValue;
        string sscanSerial = (txtsearchScanSerial.Text.Trim() == "") ? "" : txtsearchScanSerial.Text.Trim();
        string suser = (txtsearchUser.Text.Trim() == "") ? "" : txtsearchUser.Text.Trim();

        if (txtScanSerial.Text.Trim().Length == 0)
            lblErrmessage.Text = "";

        List<DeviceTracking> deviceTracking =
            (from p in idc.DeviceTrackings
             where p.Location.Contains(slocation) &&
                p.ActualWareHouse.Contains(sactualWarehouse) &&
                p.ScanSerial.Contains(sscanSerial) &&
                p.UserName.Contains(suser)
             orderby p.DeviceTrackingID descending
             select p).ToList();


        //var asc = gvDeviceInfo.SortDirection == SortDirection.Ascending;
        var asc = gvDeviceInfo.SortDirection == (SortDirection)ViewState["sortDirection"];
        

        // Sort the List based on the Column selected.
        //switch (this.gvDeviceInfo.SortExpression)

        switch (ViewState["sortExpression"].ToString())
        {
            case "Createddate":
                deviceTracking =  ( asc) ? (List<DeviceTracking>) deviceTracking.OrderBy (o => o.Createddate).ToList() : (List<DeviceTracking>) deviceTracking.OrderByDescending(o => o.Createddate).ToList();
                break;
            case "Location":
                deviceTracking = (asc) ? deviceTracking.OrderBy(p => p.Location).ToList() : deviceTracking.OrderByDescending(p => p.Location).ToList();
                break;
            case "ActualWareHouse":
                deviceTracking = (asc) ? (List<DeviceTracking>)deviceTracking.OrderBy(o => o.ActualWareHouse).ToList() : (List<DeviceTracking>)deviceTracking.OrderByDescending(o => o.ActualWareHouse).ToList();
                break;
            case "ScanSerial":
                deviceTracking = (asc) ? (List<DeviceTracking>)deviceTracking.OrderBy(o => o.ScanSerial).ToList() : (List<DeviceTracking>)deviceTracking.OrderByDescending(o => o.ScanSerial).ToList();
                break;
            case "UserName":
                deviceTracking = (asc) ? (List<DeviceTracking>)deviceTracking.OrderBy(o => o.UserName).ToList() : (List<DeviceTracking>)deviceTracking.OrderByDescending(o => o.UserName).ToList();
                break;
            case "ScannedWareHouse":
                deviceTracking = (asc) ? (List<DeviceTracking>)deviceTracking.OrderBy(o => o.ScannedWareHouse).ToList() : (List<DeviceTracking>)deviceTracking.OrderByDescending(o => o.ScannedWareHouse).ToList();
                break;
            case "DeviceID":
                deviceTracking = (asc) ? (List<DeviceTracking>)deviceTracking.OrderBy(o => o.DeviceID).ToList() : (List<DeviceTracking>)deviceTracking.OrderByDescending(o => o.DeviceID).ToList();
                break;
        }


        gvDeviceInfo.DataSource = deviceTracking;
        gvDeviceInfo.DataBind();

        lbltotalCount.Text = deviceTracking.Count().ToString();


    }

  
    private DeviceInventory getDeviceInventory(string serialNumber)
    {
        // Create the object to return.
        DeviceInventory devIn = null;
        try
        {
            // Find the inventory for the devices and the device info.
            devIn = (from di in idc.DeviceInventories
                     where di.SerialNo == serialNumber
                     select di).FirstOrDefault();
        }
        catch
        {
            // Set the object to null
            devIn = null;
        }

        // Return the object
        return devIn;
    }

    // 1/22/2015 tdo
    private void CleanPreviousRMARecordsBySerialno(int pReturnID, string pSerialNo)
    {
        try
        {
            // Get all RMA ReturnDevice in the past
            IQueryable<rma_ReturnDevice> rdList = (from rd in adc.rma_ReturnDevices
                                                   where rd.SerialNo == pSerialNo && rd.ReturnID < pReturnID
                                                   select rd).AsQueryable();
            foreach (rma_ReturnDevice rd in rdList)
            {
                if (!rd.Received)
                {
                    rd.Received = true;
                    rd.Status = 7;          // Device is received and inspected by Tech .
                    rd.DepartmentID = 8;    // Device is in Tech Receiving location                
                    adc.SubmitChanges();
                }                
            }

            // Get all RMA ReturnInventory in the past
            IQueryable<rma_ReturnInventory> riList = (from ri in adc.rma_ReturnInventories
                                                      where ri.SerialNo == pSerialNo && ri.ReturnID < pReturnID
                                                      select ri).AsQueryable();
            foreach (rma_ReturnInventory ri in riList)
            {
                if (!ri.Completed)
                {
                    ri.TechOpsNotes = "System filled";
                    ri.TechOpsReviewer = this.UserName;
                    ri.TechOpsReviewDate = DateTime.Now;
                    ri.CommittedDate = DateTime.Now;

                    ri.VisualInspectPass = true;
                    ri.DataInspectPass = true;
                    ri.TechOpsApproved = true;
                    ri.Approved = true;
                    ri.Active = true;
                    ri.Completed = true;
                    ri.CommittedToFinance = true;

                    adc.SubmitChanges();
                }                
            }
        }
        catch { }
        
    }

    //private bool isWaitingScannedByTechOps(int pReturnID, string pSerialNo)
    //{
    //    // Get the Return Request devices count...
    //    int count = (from ri in adc.rma_ReturnInventories 
    //                               where ri.ReturnID == pReturnID && ri.SerialNo == pSerialNo
    //                                && ri.Active == true && ri.CommittedToFinance == null 
    //                              select ri).Count();
    //    if (count > 0)
    //        return true;
    //    else
    //        return false;
    //}

    //protected void txtScanSerial_TextChanged(object sender, EventArgs e)
    //{
      

    //}

    private void saveDeviceInfo()
    {

        this.hdnfldFlashRed.Value = "";

        lblErrmessage.Text = "";
        Boolean bvalid = true;

        // verify that input fields are filled

        if (txtLocation.Text.Trim() == "")
        {
            lblErrmessage.Text = "*** Error: the Location field is blank! ***";
            bvalid = false;
        }

        if (ddlActualWareHouse.SelectedValue.ToString() == "")
        {
            lblErrmessage.Text = lblErrmessage.Text + "<br>*** Error: the Actual Warehouse dropdown is blank! ***";
            bvalid = false;
        }


        if (txtUserName.Text.Trim() == "")
        {
            lblErrmessage.Text = lblErrmessage.Text + "<br>*** Error: the User field is blank! ***";
            bvalid = false;
        }



        if (txtScanSerial.Text.Trim() == "")
        {
            lblErrmessage.Text = lblErrmessage.Text + "<br>*** Error: the Scan Serial No field is blank! ***";
            bvalid = false;
        }


        if (bvalid == false)
            return;

        var vcheckDevice = (from p in idc.sp_verifyDeviceWarehouse(txtScanSerial.Text.ToString(), ddlActualWareHouse.SelectedValue.ToString()) select p.warehouse).FirstOrDefault();

        if (vcheckDevice == null || vcheckDevice.ToString() =="")
        {
            lblErrmessage.Text = "*** Error: There is no Warehouse  information for the scanned serial no. " + txtScanSerial.Text + "! ***";
            this.hdnfldFlashRed.Value = "";
            return;
        }
        else if ( vcheckDevice.ToString() != ddlActualWareHouse.SelectedValue.ToString())
        { 
            
            lblErrmessage.Text = "*** Error: The Actual Warehouse  for the scanned serial no. " + txtScanSerial.Text +  " is " + ddlActualWareHouse.Text.ToString()  +  " and it does not match the scanned warehouse which is " + vcheckDevice.ToString() +  "! ***";
            this.hdnfldFlashRed.Value = "yes";
            return;
        }


        var vcheckDuplicate = (from p in idc.sp_checkDeviceTrackingDuplicate(txtScanSerial.Text.ToString()) select p.reccount).FirstOrDefault();

        if (vcheckDuplicate == null || vcheckDuplicate.ToString() != "0")
        {
            lblErrmessage.Text = "*** Error: The scanned Serial No is already existed in the Device Tracking Table! ***";

            return;
        }

        try
        {


            idc.sp_insertDeviceTracking(txtLocation.Text, ddlActualWareHouse.SelectedValue, txtScanSerial.Text, txtUserName.Text);
            idc.SubmitChanges();
            lblErrmessage.Text = "Device successfully added!";
            plInput.BackColor = System.Drawing.Color.LightGreen;


            if (txtLocation.Text.Trim().Length > 0)
                txtsearchLocation.Text = txtLocation.Text.ToString();
            ddlsearchActualWarehouse.SelectedValue = "";
            txtsearchScanSerial.Text = "";
            txtsearchUser.Text = "";

            //txtScanSerial.text
            //LoadData();
            searchDevices();

            
        }
        catch (Exception err)
        {
            lblErrmessage.Text = err.Message;
        }

    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        //txtScanSerial_TextChanged(sender, e);
        saveDeviceInfo();
    }

    protected void gvDeviceInfo_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvDeviceInfo.PageIndex = e.NewPageIndex;
        //LoadData();
        searchDevices();
    }

    protected void btnExport_Click(object sender, EventArgs e)
    {

        string excelFileName = "export";

       
        if (gvDeviceInfo.Rows.Count > 0)
        {

            //List<DeviceTracking> devicesList = (from p in idc.DeviceTrackings orderby p.DeviceTrackingID descending select p).ToList();

            string slocation = (txtsearchLocation.Text.Trim() == "") ? "" : txtsearchLocation.Text.Trim();
            string sactualWarehouse = (ddlsearchActualWarehouse.SelectedValue == "") ? "" : ddlsearchActualWarehouse.SelectedValue;
            string sscanSerial = (txtsearchScanSerial.Text.Trim() == "") ? "" : txtsearchScanSerial.Text.Trim();
            string suser = (txtsearchUser.Text.Trim() == "") ? "" : txtsearchUser.Text.Trim();



            List<DeviceTracking> devicesList =
                (from p in idc.DeviceTrackings
                 where p.Location.Contains(slocation) &&
                    p.ActualWareHouse.Contains(sactualWarehouse) &&
                    p.ScanSerial.Contains(sscanSerial) &&
                    p.UserName.Contains(suser)
                 orderby p.DeviceTrackingID descending
                 select p).ToList();



            // Create an data table used to export
            DataTable exportTable = new DataTable();
            exportTable.Columns.Add("Created Date");
            exportTable.Columns.Add("Location");
            exportTable.Columns.Add("Actual Warehouse");
            exportTable.Columns.Add("Scan Serial NO");
            exportTable.Columns.Add("User");
            exportTable.Columns.Add("Scanned Warehouse");
            exportTable.Columns.Add("Device ID");

            // Add the rows from the devices list
            foreach (DeviceTracking dl in devicesList)
            //foreach ()
            {
                // Create a new table row
                DataRow dr = exportTable.NewRow();
                dr[0] = dl.Createddate;
                dr[1] = dl.Location;
                dr[2] = dl.ActualWareHouse;
                dr[3] = dl.ScanSerial;
                dr[4] = dl.UserName;
                dr[5] = dl.ScannedWareHouse;
                dr[6] = dl.DeviceID.ToString();

                // Add the row to the table
                exportTable.Rows.Add(dr);
            }

            // Build the export table
            TableExport tableExport = new TableExport(exportTable);

            try
            {
                // Read the CSS template from file
                tableExport.Stylesheet =
                    System.IO.File.ReadAllText(Server.MapPath("~/_templates/export/grids.css"));
            }
            catch { }


            try
            {
                // Create the export file based on the selected value.
                tableExport.Export(excelFileName, "XLS");
                //tableExport.Export(excelFileName, ddlExport.SelectedValue);

                ExportFile file = tableExport.File;

                // Clear everything out.
                Response.Clear();
                Response.ClearHeaders();

                // Set the response headers.
                Response.ContentType = file.ContentType;
                Response.AddHeader("Content-Disposition", file.ContentDisposition);

                // Write to Excel file.
                if (file.Content.GetType() == typeof(byte[]))
                    Response.BinaryWrite((byte[])file.Content);
                else
                {
                    Response.Write(file.Content.ToString());
                }
            }
            catch (Exception ex)
            {
                //VisibleError(ex.ToString());
                return;
            }

            Response.Flush();
            Response.End();

        }  // if gvDeviceTracking.rows.count > 0
    }

    protected void gvDeviceInfo_Sorting(object sender, GridViewSortEventArgs e)
    {
        ViewState["sortExpression"] = e.SortExpression;

        ViewState["sortDirection"] = ((SortDirection)ViewState["sortDirection"] == SortDirection.Ascending) ? SortDirection.Descending : SortDirection.Ascending;

        searchDevices();
    }
}