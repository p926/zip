using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Collections;

using Instadose;
using Instadose.Data;
using Instadose.Device;

public partial class TechOps_DeviceWarehouseTransfer : System.Web.UI.Page
{
    InsDataContext idc = new InsDataContext();
    AppDataContext adc = new AppDataContext();
    DeviceInventory deviceInventory = null;
    string UserName = "Unknown";

    protected void Page_PreRenderComplete(object sender, EventArgs e)
    {
        // display user shipment count in Queue
        if (grdViewDeviceMovementHistory.Rows.Count == 0) grdViewDeviceMovementHistory.Style.Add("margin", "0px 0");
        else grdViewDeviceMovementHistory.Style.Add("margin", "-1px 0");        
    }   
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            //this.UserName = User.Identity.Name.Split('\\')[1];       
            this.UserName = "tdo";
           
            if (!this.IsPostBack)
            {
                resetControls();
            }
        }
        catch
        {
            this.UserName = "Unknown";
        }
    }
    private void resetSerialNo()
    {
        this.txtSerialNo.Text = "";
        // Placeholder will always be present.
        this.txtSerialNo.Attributes.Add("placeholder", "Enter Serial Number");
    }
    private void resetControls()
    {                
        resetSerialNo();

        WarehouseMovementSetting wmSetting = (from wms in idc.WarehouseMovementSettings where wms.Active == true orderby wms.ModDate descending select wms).FirstOrDefault();
        if (wmSetting == null)
        {
            SetFocus(this.btnAdminSetting);
            EnableDisableControls(false);
        }
        else
        {
            SetFocus(this.txtSerialNo);
            EnableDisableControls(true);
            LoadFromToWarehouse(wmSetting);
        }
    }
    protected void LoadDDLWarehouse(DropDownList DDLName)
    {
        var warehouse = (from a in idc.DeviceAnalysisStatus
                         where a.Warehouse != ""
                         orderby a.Warehouse ascending
                         select new
                         {
                             Warehouse = a.Warehouse
                         }).Distinct();

        DDLName.Items.Clear();

        DDLName.DataSource = warehouse;
        DDLName.DataBind();

        ListItem firstItem = new ListItem("  -- Warehouse --", "0");
        DDLName.Items.Insert(0, firstItem);
        DDLName.SelectedIndex = 0;
    }
    protected void LoadDDLStatus(DropDownList DDLName, string pWarehouse)
    {
        DDLName.Items.Clear();

        var statuses = (from a in idc.DeviceAnalysisStatus
                        where a.Warehouse == pWarehouse
                        orderby a.DeviceAnalysisName
                        select a);

        DDLName.DataSource = statuses;
        DDLName.DataBind();

        ListItem firstItem = new ListItem("  -- Status --", "0");
        DDLName.Items.Insert(0, firstItem);
        DDLName.SelectedIndex = 0;
    }
    protected void ddlFromWarehouse_SelectedIndexChanged(object sender, System.EventArgs e)
    {
        DropDownList ddlC = (DropDownList)sender;
        LoadDDLStatus(ddlFromStaus, ddlC.SelectedValue);
        EnableConfirmButton();
    }
    protected void ddlToWarehouse_SelectedIndexChanged(object sender, System.EventArgs e)
    {
        DropDownList ddlC = (DropDownList)sender;
        LoadDDLStatus(ddlToStatus, ddlC.SelectedValue);
        EnableConfirmButton();
    }
    protected void ddlFromStaus_SelectedIndexChanged(object sender, System.EventArgs e)
    {
        EnableConfirmButton();
    }
    protected void ddlToStatus_SelectedIndexChanged(object sender, System.EventArgs e)
    {
        EnableConfirmButton();
    }
    private void EnableDisableControls(bool pValue)
    {
        btnAccept.Enabled = pValue;
        txtSerialNo.Enabled = pValue;
    }
    private void LoadFromToWarehouse(WarehouseMovementSetting pWmSetting)
    {
        String fromWarehouse, fromStatus, ToWarehouse, ToStatus;
        fromWarehouse = pWmSetting.DeviceAnalysisStatus1.Warehouse;
        fromStatus = pWmSetting.DeviceAnalysisStatus1.DeviceAnalysisName;
        ToWarehouse = pWmSetting.DeviceAnalysisStatus.Warehouse;
        ToStatus = pWmSetting.DeviceAnalysisStatus.DeviceAnalysisName;
        lblFromWarehouse.InnerHtml = fromWarehouse;
        lblFromStatus.InnerHtml = fromStatus;
        lblToWarehouse.InnerHtml = ToWarehouse;
        lblToStatus.InnerHtml = ToStatus;
    }
    private void LoadScanErrorDialog(string pSerialNo, string pWarehouse, string pStatus)
    {
        string wsString = pWarehouse.ToUpper() + "/" + pStatus.ToUpper() + " MISMATCH";
        lblSerialNo.InnerHtml = pSerialNo;
        lblDeviceWarehouseStatus.InnerHtml = wsString;
        lblNote.InnerHtml = "Place in General Mismatch Bucket";
    }    
    private bool passInputsValidation()
    {
        string serialNo = this.txtSerialNo.Text.Trim();

        if (serialNo.Length == 0)
        {
            this.SetErrorMessage("SerialNo is required.");            
            return false;
        }

        deviceInventory = (from di in idc.DeviceInventories where di.SerialNo == serialNo select di).FirstOrDefault();
        if (deviceInventory == null)
        {
            this.SetErrorMessage("Device does not exist in the system.");            
            return false;
        }

        string deviceWarehouse = deviceInventory.DeviceAnalysisStatus.Warehouse;
        string deviceStatus = deviceInventory.DeviceAnalysisStatus.DeviceAnalysisName;
        string fromWarehouse = lblFromWarehouse.InnerHtml;
        string fromStatus = lblFromStatus.InnerHtml;

        if (deviceWarehouse != fromWarehouse || deviceStatus != fromStatus)
        {
            LoadScanErrorDialog(serialNo, deviceWarehouse, deviceStatus);
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), "scanErrorDialog", "openDialog('scanErrorDialog')", true);

            resetSerialNo();
            SetFocus(this.txtSerialNo);
            return false;
        }

        // Tdo, 07/09/2019
        // --------------- Check assembly item. They should be the same when transfering -----------------------//
        WarehouseMovementSetting curWmSetting = (from wms in idc.WarehouseMovementSettings where wms.Active == true orderby wms.ModDate descending select wms).FirstOrDefault();        

        string destAXAssemblyItem = curWmSetting.DeviceAnalysisStatus.AXAssemblyItem;
        string sourceAXAssemblyItem = deviceInventory.DeviceAnalysisStatus.AXAssemblyItem;
        if (!destAXAssemblyItem.Contains(sourceAXAssemblyItem))            
        {            
            this.SetErrorMessage("Device is not allowed to transfer to: " + curWmSetting.DeviceAnalysisStatus.Warehouse.ToUpper() + "/" + curWmSetting.DeviceAnalysisStatus.DeviceAnalysisName.ToUpper() + " due to assembly item mismatch");
            return false;
        }        
        
        // ---------------- Check if it was already scanned ----------------------- //
        int cnt = (from dmh in idc.DeviceMovementHistories
                   where dmh.WarehouseMovementID == curWmSetting.WarehouseMovementID
                   && dmh.DeviceID == deviceInventory.DeviceID
                   && dmh.CreatedDate.Date == DateTime.Today
                   select dmh).Count();        
        if(cnt > 0)
        {
            this.SetErrorMessage("Device is already scanned.");            
            return false;
        }
        
        return true;
    }
    private bool passSettingInputsValidation()
    {
        if (ddlFromWarehouse.SelectedIndex == 0)
        {
            this.SetSettingErrorMessage("From Warehouse is required.");
            SetFocus(this.ddlFromWarehouse);
            return false;
        }

        if (ddlFromStaus.SelectedIndex == 0)
        {
            this.SetSettingErrorMessage("From Status is required.");
            SetFocus(this.ddlFromStaus);
            return false;
        }

        if (ddlToWarehouse.SelectedIndex == 0)
        {
            this.SetSettingErrorMessage("To Warehouse is required.");
            SetFocus(this.ddlToWarehouse);
            return false;
        }

        if (ddlToStatus.SelectedIndex == 0)
        {
            this.SetSettingErrorMessage("To Status is required.");
            SetFocus(this.ddlToStatus);
            return false;
        }
        
        if (int.Parse(ddlFromStaus.SelectedValue) == int.Parse(ddlToStatus.SelectedValue))
        {
            this.SetSettingErrorMessage("Transfer to the same warehouse/status is not allowed!!!");
            SetFocus(this.ddlToStatus);
            return false;
        }

        return true;
    }
    private bool passPasswordInputsValidation()
    {
        if (string.IsNullOrEmpty(this.txtPassword.Text))
        {
            this.SetPasswordErrorMessage("Please enter a password.");
            SetFocus(this.txtPassword);
            return false;
        }

        string page = @"/TechOps/DeviceWarehouseTransfer.aspx";
        string password = (from ap in idc.AuthPages where ap.Page == page && ap.Active == true select ap.Password).FirstOrDefault();

        if (this.txtPassword.Text != password)
        {
            this.SetPasswordErrorMessage("Incorrect Password. Please re-enter password");
            SetFocus(this.txtPassword);
            return false;
        }
       
        return true;
    }
    private bool passExportValidation()
    {        
        if (string.IsNullOrEmpty(this.txtFrom.Text))
        {
            this.SetErrorMessage("Scan From is required.");            
            return false;
        }

        if (string.IsNullOrEmpty(this.txtTo.Text))
        {
            this.SetErrorMessage("Scan To is required.");            
            return false;
        }

        DateTime fromDate,toDate;

        if (! DateTime.TryParse(this.txtFrom.Text, out fromDate))
        {
            this.SetErrorMessage("Scan From date is wrong format.");            
            return false;
        }

        if (!DateTime.TryParse(this.txtTo.Text, out toDate))
        {
            this.SetErrorMessage("Scan To date is wrong format.");            
            return false;
        }

        if (fromDate > toDate)
        {
            this.SetErrorMessage("Scan To date can not be prior Scan From date.");           
            return false;
        }
        
        return true;
    }
    private void EnableConfirmButton()
    {
        this.btnWarehouseMovementSetting.Enabled = true;

        if (ddlFromWarehouse.SelectedIndex == 0 || ddlFromStaus.SelectedIndex == 0 || ddlToWarehouse.SelectedIndex == 0 || ddlToStatus.SelectedIndex == 0)
        {
            this.btnWarehouseMovementSetting.Enabled = false;
        }
    }    
    private void SetErrorMessage(string error)
    {
        //this.errorMsg.InnerHtml = error;
        //this.error.Visible = !String.IsNullOrEmpty(error);
        this.error.Visible = true;
        this.errorMsg.InnerHtml = error;
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), "mainErrorDialog", "openDialog('mainErrorDialog')", true);
    }
    private void SetSettingErrorMessage(string error)
    {
        this.settingErrorMsg.InnerHtml = error;
        this.settingError.Visible = !String.IsNullOrEmpty(error);
    }
    private void SetPasswordErrorMessage(string error)
    {
        this.passwordErrorMsg.InnerHtml = error;
        this.passwordError.Visible = !String.IsNullOrEmpty(error);
    }   
    protected void grdViewDeviceMovementHistory_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            if (e.Row.RowIndex == 0)
            {
                e.Row.Style.Add("height", "60px");
                e.Row.VerticalAlign = VerticalAlign.Bottom;
            }
        }
    }
    private void CommitWarehouseMovementSetting()
    {
        WarehouseMovementSetting curWmSetting = (from wms in idc.WarehouseMovementSettings where wms.Active == true orderby wms.ModDate descending select wms).FirstOrDefault();
        if (curWmSetting != null)
        {
            curWmSetting.Active = false;
            curWmSetting.ModDate = DateTime.Now;
            curWmSetting.ModUser = this.UserName;
            idc.SubmitChanges();
        }

        // Insert WarehouseMovementSetting.
        WarehouseMovementSetting whMovementSetting = new WarehouseMovementSetting()
        {
            FromDeviceAnalysisStatusID = int.Parse(ddlFromStaus.SelectedValue),
            ToDeviceAnalysisStatusID = int.Parse(ddlToStatus.SelectedValue),
            AddUser = this.UserName,
            AddDate = DateTime.Now,
            ModUser = this.UserName,
            ModDate = DateTime.Now,
            Active = true
        };
        idc.WarehouseMovementSettings.InsertOnSubmit(whMovementSetting);
        idc.SubmitChanges();
    }    
    private void OpenAdminSettingDialog()
    {
        SetSettingErrorMessage("");

        LoadDDLWarehouse(this.ddlFromWarehouse);
        LoadDDLWarehouse(this.ddlToWarehouse);

        LoadDDLStatus(ddlFromStaus, ddlFromWarehouse.SelectedValue);
        LoadDDLStatus(ddlToStatus, ddlToWarehouse.SelectedValue);

        // Disable Confirm button until all drop boxes are selected
        EnableConfirmButton();
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), "warehouseMovementSettingDialog", "openDialog('warehouseMovementSettingDialog')", true);

        SetFocus(this.ddlFromWarehouse);
    }
    protected void btnAdminSetting_Click(object sender, EventArgs e)
    {
        SetPasswordErrorMessage("");
        this.txtPassword.Text = "";
        SetFocus(this.txtPassword);
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), "passwordDialog", "openDialog('passwordDialog')", true);
    }
    protected void btnAccept_Click(object sender, EventArgs e)
    {
        try
        {
            if (passInputsValidation())
            {
                WarehouseMovementSetting curWmSetting = (from wms in idc.WarehouseMovementSettings where wms.Active == true orderby wms.ModDate descending select wms).FirstOrDefault();

                deviceInventory.DeviceAnalysisStatus = curWmSetting.DeviceAnalysisStatus;

                // Insert DeviceMovementHistory.
                DeviceMovementHistory diMovementHistory = new DeviceMovementHistory()
                {
                    WarehouseMovementID = curWmSetting.WarehouseMovementID,
                    DeviceID = deviceInventory.DeviceID,
                    CreatedBy = this.UserName,
                    CreatedDate = DateTime.Now
                };
                idc.DeviceMovementHistories.InsertOnSubmit(diMovementHistory);

                idc.SubmitChanges();

                //Update device audit
                DeviceManager myDeviceManager = new DeviceManager();
                myDeviceManager.InsertDeviceInventoryAudit(deviceInventory.DeviceID, deviceInventory.DeviceAnalysisStatus.DeviceAnalysisStatusID, false, "Device Status Movement", "Device Status Movement Page");

                // refresh gridview
                grdViewDeviceMovementHistory.DataBind();

                // ******************** done **************************//                
                resetSerialNo();
                SetFocus(this.txtSerialNo);
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage(ex.Message);
        }
    }
    protected void btnExport_Click(object sender, EventArgs e)
    {        
        try
        {            
            if (passExportValidation())
            {                
                DateTime fromDate = DateTime.Parse(this.txtFrom.Text);
                DateTime toDate = DateTime.Parse(this.txtTo.Text).AddDays(1);

                string url = "DeviceWarehouseTransferReport.aspx?fromDate=" + this.txtFrom.Text + "&toDate=" + this.txtTo.Text;
                ScriptManager.RegisterStartupScript(this, typeof(string), "OPEN_WINDOW", "window.open( '" + url + "', 'WarehouseTransferReport', 'height=450,width=800,status=yes,toolbar=yes,menubar=no,location=no,resize=yes' );", true);                
            }            
        }
        catch (Exception ex)
        {
            this.SetErrorMessage(ex.ToString());
        }
    }
    protected void btnMainErrorOK_Click(object sender, EventArgs e)
    {
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), "mainErrorDialog", "closeDialog('mainErrorDialog')", true);
        SetFocus(this.txtSerialNo);
    }
    protected void btnScanErrorOK_Click(object sender, EventArgs e)
    {
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), "scanErrorDialog", "closeDialog('scanErrorDialog')", true);
        SetFocus(this.txtSerialNo);
    }    
    protected void btnWarehouseMovementSetting_Click(object sender, EventArgs e)
    {
        try
        {
            if(passSettingInputsValidation())
            {
                CommitWarehouseMovementSetting();
                resetControls();
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), "warehouseMovementSettingDialog", "closeDialog('warehouseMovementSettingDialog')", true);
            }            
        }
        catch (Exception ex)
        {
            // Display the system generated error message.            
            SetSettingErrorMessage(ex.Message);
        }
    }
    protected void btnPasswordOK_Click(object sender, EventArgs e)
    {
        try
        {             
            if (passPasswordInputsValidation())
            {
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), "passwordDialog", "closeDialog('passwordDialog')", true);
                OpenAdminSettingDialog();
            }            
        }
        catch (Exception ex)
        {
            // Display the system generated error message.            
            SetPasswordErrorMessage(ex.Message);
        }
    }    
}