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
using portal_instadose_com_v3.Snow;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

public partial class Shipping_NewReceivingInventory : System.Web.UI.Page
{
    InsDataContext idc = new InsDataContext();
    AppDataContext adc = new AppDataContext();
    string UserName = "Unknown";
    DeviceInventory deviceInventory = null;
    ProductInventory productInventory = null;
    AccountDevice accountDevice = null;
    rma_ReturnDevice returnDevice = null;

    bool _isInstalink = false;

    // SNOW variable which determine if the task is the maximum days deadline
    // Temporarily set to 90 from 45
    int MaxDayLimitState = 90;

    // SNOW Status Codes
    int SnowStatusClosed = 3;
    int SnowStatusInprogress = 2;

    // App Insights Manual Integration
    TelemetryClient clientTelemetry = new TelemetryClient(TelemetryConfiguration.Active);

    protected void Page_PreRenderComplete(object sender, EventArgs e)
    {
        // display user shipment count in Queue
        if (grdViewReturnPackage.Rows.Count == 0) grdViewReturnPackage.Style.Add("margin", "0px 0");
        else grdViewReturnPackage.Style.Add("margin", "-1px 0");
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (User.Identity.Name.IndexOf('\\') > 0)
                this.UserName = User.Identity.Name.Split('\\')[1];
            else
                this.UserName = "Testing";

            if (!this.IsPostBack)
            {
                resetControls();
            }
        }
        catch
        {
            SetFocus(this.btnNewPackage);
            this.UserName = "Unknown";
        }
    }
    private void resetControls()
    {
        SetFocus(this.btnNewPackage);
        SetErrorMessage("");
        this.txtSerialNo.Text = "";
        this.lblCommitMsg.InnerHtml = "";

        this.btnNewPackage.Enabled = true;
        this.btnPrevPackage.Enabled = true;
        this.btnEndPackage.Enabled = false;
        this.txtSerialNo.Enabled = false;
        this.btnSubmit.Enabled = false;
    }
    private int GetNextPackageID()
    {
        var maxPackageIDItem = adc.rma_ReturnPackages.OrderByDescending(i => i.PackageID).FirstOrDefault();
        if (maxPackageIDItem == null)
        {
            return 1;
        }
        else
        {
            return maxPackageIDItem.PackageID + 1;
        }
    }

    private int GetLatestPackageID()
    {
        var maxPackageIDItem = adc.rma_ReturnPackages.OrderByDescending(i => i.PackageID).FirstOrDefault();
        if (maxPackageIDItem == null)
        {
            return 1;
        }
        else
        {
            return maxPackageIDItem.PackageID;
        }
    }
    protected void btnNewPackage_Click(object sender, EventArgs e)
    {
        this.btnNewPackage.Enabled = false;
        this.btnPrevPackage.Enabled = true;
        this.btnEndPackage.Enabled = true;
        this.txtSerialNo.Enabled = true;
        this.btnSubmit.Enabled = true;
        SetFocus(this.txtSerialNo);
        this.lblCommitMsg.InnerHtml = "";

        this.packageID.Value = GetNextPackageID().ToString();
        hidCurrentActID.Value = "";
        this.hidPreviousPackageClick.Value = "";
    }
    protected void btnPrevPackage_Click(object sender, EventArgs e)
    {
        this.btnNewPackage.Enabled = false;
        this.btnPrevPackage.Enabled = true;
        this.btnEndPackage.Enabled = true;
        this.txtSerialNo.Enabled = true;
        this.btnSubmit.Enabled = true;
        SetFocus(this.txtSerialNo);
        this.lblCommitMsg.InnerHtml = "";

        //int nextPackageID = GetNextPackageID();
        //if (nextPackageID > 1) nextPackageID -= 1;
        this.packageID.Value = GetLatestPackageID().ToString();

        int accountID = (from rp in adc.rma_ReturnPackages
                         join ri in adc.rma_ReturnInventories on rp.SerialNo equals ri.SerialNo
                         join r in adc.rma_Returns on ri.ReturnID equals r.ReturnID
                         where rp.PackageID == int.Parse(this.packageID.Value)
                         && ri.Active == true && r.Active == true
                         orderby ri.ReceivedDate descending
                         select r.AccountID).FirstOrDefault();

        hidCurrentActID.Value = accountID.ToString();
        // flag previous package for validation at commit. Once committed turn off the flag
        this.hidPreviousPackageClick.Value = "1";
    }
    protected void btnEndPackage_Click(object sender, EventArgs e)
    {
        this.btnNewPackage.Enabled = false;
        this.btnPrevPackage.Enabled = true;
        this.btnEndPackage.Enabled = true;
        this.txtSerialNo.Enabled = true;
        this.btnSubmit.Enabled = true;
        SetFocus(this.txtSerialNo);
        this.lblCommitMsg.InnerHtml = "";

        this.packageID.Value = GetNextPackageID().ToString();
        hidCurrentActID.Value = "";
        this.hidPreviousPackageClick.Value = "";
    }
    private bool passInputsValidation()
    {
        string serialNo = this.txtSerialNo.Text.Trim();

        if (serialNo.Length == 0)
        {
            this.SetErrorMessage("SerialNo is required.");
            SetFocus(this.txtSerialNo);
            return false;
        }

        _isInstalink = serialNo.Substring(0, 1) == "L";

        if (_isInstalink)
        {
            productInventory = (from di in idc.ProductInventories where di.SerialNo == serialNo select di).FirstOrDefault();
            if (productInventory == null)
            {
                this.SetErrorMessage("The Dosimeter does not exist in the system.");
                SetFocus(this.txtSerialNo);
                return false;
            }

            rma_ReturnDevice lastReceivedReturnProduct = (from rd in adc.rma_ReturnDevices
                                                          join r in adc.rma_Returns on rd.ReturnID equals r.ReturnID
                                                          where rd.SerialNo == serialNo
                                                          && rd.Active == true && r.Active == true
                                                          && rd.Received == true
                                                          && rd.NonReturnID == null
                                                          orderby rd.ReturnDevicesID descending
                                                          select rd).FirstOrDefault();
            if (lastReceivedReturnProduct != null)
            {
                int lastReceivedReturnDeviceActID = lastReceivedReturnProduct.rma_Return.AccountID;
                AccountProduct latestAccountProduct = (from ad in idc.AccountProducts
                                                       where ad.ProductInventoryID == productInventory.ProductInventoryID
                                                       orderby ad.AssignmentDate descending
                                                       select ad).FirstOrDefault();

                if (latestAccountProduct != null && lastReceivedReturnDeviceActID == latestAccountProduct.AccountID && latestAccountProduct.CurrentDeviceAssign == false)
                {
                    // Throw out error message if duplicate receiving scan.
                    string receivingDate = DateTime.MinValue.ToShortDateString();
                    rma_ReturnInventory retInventory = (from ri in adc.rma_ReturnInventories where ri.SerialNo == serialNo select ri).FirstOrDefault();
                    if (retInventory != null)
                        receivingDate = retInventory.CreatedDate.ToString();

                    string rmaInfo = "RMA#: " + lastReceivedReturnProduct.ReturnID + " , scanned on: " + receivingDate + " , BIN: "
                            + ((lastReceivedReturnProduct.rma_Return.rma_ref_ReturnReason.rma_ref_Bin == null) ? "N/A" : (String.IsNullOrEmpty(lastReceivedReturnProduct.rma_Return.rma_ref_ReturnReason.rma_ref_Bin.Bin) ? "N/A" : lastReceivedReturnProduct.rma_Return.rma_ref_ReturnReason.rma_ref_Bin.Bin));
                    this.SetErrorMessage("Dosimeter has already been scanned and added into receiving inventory. Duplicate receiving scan is not allowed.</br>" + rmaInfo);
                    SetFocus(this.txtSerialNo);
                    return false;
                }
            }

            // Find the CURRENT assigned AccountID based on DeviceID from deviceInventory.
            // This is done by ORDERING BY AssignmentDate DESCENDING.
            var accountProduct = (from ad in idc.AccountProducts
                                  where ad.ProductInventoryID == productInventory.ProductInventoryID && ad.CurrentDeviceAssign == true
                                  orderby ad.AssignmentDate descending
                                  select ad).FirstOrDefault();

            if (accountProduct == null) // Device is currently not belonging to any customer. This means it was already scanned in our warehouse, or by daily Non-return process, or CurrentDeviceAssign flag was set wrongly. 
            {
                if (lastReceivedReturnProduct != null) // device was already scanned in our warehouse before or after the existing of daily Non-return process
                {
                    // Throw out error message if duplicate receiving scan.
                    string receivingDate = DateTime.MinValue.ToShortDateString();
                    rma_ReturnInventory retInventory = (from ri in adc.rma_ReturnInventories where ri.SerialNo == serialNo select ri).FirstOrDefault();
                    if (retInventory != null)
                        receivingDate = retInventory.CreatedDate.ToString();

                    string rmaInfo = "RMA#: " + lastReceivedReturnProduct.ReturnID + " , scanned on: " + receivingDate + " , BIN: "
                            + ((lastReceivedReturnProduct.rma_Return.rma_ref_ReturnReason.rma_ref_Bin == null) ? "N/A" : (String.IsNullOrEmpty(lastReceivedReturnProduct.rma_Return.rma_ref_ReturnReason.rma_ref_Bin.Bin) ? "N/A" : lastReceivedReturnProduct.rma_Return.rma_ref_ReturnReason.rma_ref_Bin.Bin));
                    this.SetErrorMessage("Dosimeter has already been scanned and added into receiving inventory. Duplicate receiving scan is not allowed.</br>" + rmaInfo);
                    SetFocus(this.txtSerialNo);
                    return false;
                }
                else
                {
                    // CurrentDeviceAssign flag was set wrongly as 0 by manually or by very old lost orders/recalls process. Unlike the later lost orders/recall process, currentdeviceassign is still = 1 till receiving scan at our warehouse or by daily Non-return process               
                    // Checking if there is recently receiving scan by technician or by daily Non-return process.
                    rma_ReturnDevice returnDevice = (from rd in adc.rma_ReturnDevices
                                                     join r in adc.rma_Returns on rd.ReturnID equals r.ReturnID
                                                     where rd.SerialNo == serialNo
                                                     && rd.Active == true && r.Active == true
                                                     && rd.Received == true
                                                     orderby rd.ReturnDevicesID descending
                                                     select rd).FirstOrDefault();

                    // if device was done the receiving scan by technician or the daily Non-return process
                    if (returnDevice != null)
                    {
                        rma_ReturnInventory retInventory = (from ri in adc.rma_ReturnInventories where ri.SerialNo == serialNo && ri.Completed == true && ri.ReturnID == returnDevice.ReturnID orderby ri.CommittedDate descending select ri).FirstOrDefault();

                        string rmaInfo = "";
                        if (retInventory != null) // it confirms a device was done the receiving scan properly.
                        {
                            if (returnDevice.NonReturnID == null)
                            {
                                rmaInfo = "RMA#: " + retInventory.ReturnID + " , scanned on: " + retInventory.CommittedDate.ToString() + " , BIN: "
                                        + ((retInventory.rma_Return.rma_ref_ReturnReason.rma_ref_Bin == null) ? "N/A" : (String.IsNullOrEmpty(retInventory.rma_Return.rma_ref_ReturnReason.rma_ref_Bin.Bin) ? "N/A" : retInventory.rma_Return.rma_ref_ReturnReason.rma_ref_Bin.Bin));

                                this.SetErrorMessage("Dosimeter has already been scanned and added into receiving inventory. Duplicate receiving scan is not allowed.</br>" + rmaInfo);
                                SetFocus(this.txtSerialNo);
                                return false;
                            }
                        }
                        else // missing ReturnInventories record, receiving scan improperly
                        {
                            this.SetErrorMessage("Dosimeter has already been scanned and added into receiving inventory. Duplicate receiving scan is not allowed.</br>" + rmaInfo);
                            SetFocus(this.txtSerialNo);
                            return false;
                        }
                    }
                }
            }

            // Find rma_ReturnDevices wich are Active, but has not been received.
            returnDevice = (from rd in adc.rma_ReturnDevices
                            join r in adc.rma_Returns on rd.ReturnID equals r.ReturnID
                            where rd.SerialNo == serialNo && r.Active == true && rd.Active == true
                            && rd.Received == false
                            orderby rd.ReturnDevicesID descending
                            select rd).FirstOrDefault();

            // Do not allow if return by IIM Account and without RMA request
            if (returnDevice == null)
            {
                string AccountIDToCheck = "7257";

                if (accountDevice != null && AccountIDToCheck.IndexOf(accountDevice.AccountID.ToString()) >= 0)
                {
                    this.SetErrorMessage("UNAUTHORIZED DOSIMETER RETURN: Dosimeter belongs to Instadose In-transit Monitoring Account #7257. Do not proceed. Refer to Instadose Technical Department.");
                    SetFocus(this.txtSerialNo);
                    return false;
                }
            }

            // Validate BIN of the return reason to see if it was set up yet.
            int return_ReasonID = 7; //Devices arrived at facility without Request No, add by Mfg
            if (returnDevice != null)
            {
                return_ReasonID = returnDevice.rma_Return.Return_ReasonID.HasValue ? returnDevice.rma_Return.Return_ReasonID.Value : 7;
            }

            rma_ref_ReturnReason returnReason = (from rs in adc.rma_ref_ReturnReasons where rs.ReasonID == return_ReasonID select rs).FirstOrDefault();
            if (returnReason == null)
            {
                this.SetErrorMessage("The return reason for this Dosimeter does not exist in the system.");
                SetFocus(this.txtSerialNo);
                return false;
            }
            else
            {
                if (returnReason.rma_ref_Bin == null || string.IsNullOrEmpty(returnReason.rma_ref_Bin.Bin))
                {
                    this.SetErrorMessage("Scan is not allowed until the BIN for the return reason: <b>" + returnReason.Description.ToUpper() + "</b> is setup.");
                    SetFocus(this.txtSerialNo);
                    return false;
                }
            }

            // Validating of using previous package
            // Only allowing if they are the same account. Else do not allow.   
            if (this.hidPreviousPackageClick.Value == "1")
            {
                int currentScannedActID = 2;
                if (accountDevice != null)
                {
                    currentScannedActID = accountDevice.AccountID;
                }

                if (!string.IsNullOrEmpty(this.hidCurrentActID.Value) && int.Parse(this.hidCurrentActID.Value) != currentScannedActID)
                {
                    resetControls();
                    this.SetErrorMessage("Could not use the previous package on the scanned serial number: " + serialNo + " because it's account is different with the previous package's account.");
                    return false;
                }
            }
        }
        else
        {
            deviceInventory = (from di in idc.DeviceInventories where di.SerialNo == serialNo select di).FirstOrDefault();
            if (deviceInventory == null)
            {
                this.SetErrorMessage("The Dosimeter does not exist in the system.");
                SetFocus(this.txtSerialNo);
                return false;
            }

            rma_ReturnDevice lastReceivedReturnDevice = (from rd in adc.rma_ReturnDevices
                                                         join r in adc.rma_Returns on rd.ReturnID equals r.ReturnID
                                                         where rd.SerialNo == serialNo
                                                         && rd.Active == true && r.Active == true
                                                         && rd.Received == true
                                                         && rd.NonReturnID == null
                                                         orderby rd.ReturnDevicesID descending
                                                         select rd).FirstOrDefault();
            if (lastReceivedReturnDevice != null)
            {
                int lastReceivedReturnDeviceActID = lastReceivedReturnDevice.rma_Return.AccountID;
                AccountDevice latestAccountDevice = (from ad in idc.AccountDevices
                                                     where ad.DeviceID == deviceInventory.DeviceID
                                                     orderby ad.AssignmentDate descending
                                                     select ad).FirstOrDefault();

                if (latestAccountDevice != null && lastReceivedReturnDeviceActID == latestAccountDevice.AccountID && latestAccountDevice.CurrentDeviceAssign == false)
                {
                    // Throw out error message if duplicate receiving scan.
                    string receivingDate = DateTime.MinValue.ToShortDateString();
                    rma_ReturnInventory retInventory = (from ri in adc.rma_ReturnInventories where ri.ReturnDeviceID == lastReceivedReturnDevice.ReturnDevicesID select ri).FirstOrDefault();
                    if (retInventory != null)
                        receivingDate = retInventory.CreatedDate.ToString();

                    string rmaInfo = "RMA#: " + lastReceivedReturnDevice.ReturnID + " , scanned on: " + receivingDate + " , BIN: "
                            + ((lastReceivedReturnDevice.rma_Return.rma_ref_ReturnReason.rma_ref_Bin == null) ? "N/A" : (String.IsNullOrEmpty(lastReceivedReturnDevice.rma_Return.rma_ref_ReturnReason.rma_ref_Bin.Bin) ? "N/A" : lastReceivedReturnDevice.rma_Return.rma_ref_ReturnReason.rma_ref_Bin.Bin));
                    this.SetErrorMessage("Dosimeter has already been scanned and added into receiving inventory. Duplicate receiving scan is not allowed.</br>" + rmaInfo);
                    SetFocus(this.txtSerialNo);
                    return false;
                }
            }

            // Find the CURRENT assigned AccountID based on DeviceID from deviceInventory.
            // This is done by ORDERING BY AssignmentDate DESCENDING.
            accountDevice = (from ad in idc.AccountDevices
                             where ad.DeviceID == deviceInventory.DeviceID
                             && ad.CurrentDeviceAssign == true
                             orderby ad.AssignmentDate descending
                             select ad).FirstOrDefault();

            if (accountDevice == null) // Device is currently not belonging to any customer. This means it was already scanned in our warehouse, or by daily Non-return process, or CurrentDeviceAssign flag was set wrongly. 
            {
                if (lastReceivedReturnDevice != null) // device was already scanned in our warehouse before or after the existing of daily Non-return process
                {
                    // Throw out error message if duplicate receiving scan.
                    string receivingDate = DateTime.MinValue.ToShortDateString();
                    rma_ReturnInventory retInventory = (from ri in adc.rma_ReturnInventories where ri.ReturnDeviceID == lastReceivedReturnDevice.ReturnDevicesID select ri).FirstOrDefault();
                    if (retInventory != null)
                        receivingDate = retInventory.CreatedDate.ToString();

                    string rmaInfo = "RMA#: " + lastReceivedReturnDevice.ReturnID + " , scanned on: " + receivingDate + " , BIN: "
                            + ((lastReceivedReturnDevice.rma_Return.rma_ref_ReturnReason.rma_ref_Bin == null) ? "N/A" : (String.IsNullOrEmpty(lastReceivedReturnDevice.rma_Return.rma_ref_ReturnReason.rma_ref_Bin.Bin) ? "N/A" : lastReceivedReturnDevice.rma_Return.rma_ref_ReturnReason.rma_ref_Bin.Bin));
                    this.SetErrorMessage("Dosimeter has already been scanned and added into receiving inventory. Duplicate receiving scan is not allowed.</br>" + rmaInfo);
                    SetFocus(this.txtSerialNo);
                    return false;
                }
                else
                {
                    // CurrentDeviceAssign flag was set wrongly as 0 by manually or by very old lost orders/recalls process. Unlike the later lost orders/recall process, currentdeviceassign is still = 1 till receiving scan at our warehouse or by daily Non-return process               
                    // Checking if there is recently receiving scan by technician or by daily Non-return process.
                    rma_ReturnDevice returnDevice = (from rd in adc.rma_ReturnDevices
                                                     join r in adc.rma_Returns on rd.ReturnID equals r.ReturnID
                                                     where rd.SerialNo == serialNo
                                                     && rd.Active == true && r.Active == true
                                                     && rd.Received == true
                                                     orderby rd.ReturnDevicesID descending
                                                     select rd).FirstOrDefault();

                    // if device was done the receiving scan by technician or the daily Non-return process
                    if (returnDevice != null)
                    {
                        rma_ReturnInventory retInventory = (from ri in adc.rma_ReturnInventories where ri.SerialNo == serialNo && ri.Completed == true && ri.ReturnID == returnDevice.ReturnID orderby ri.CommittedDate descending select ri).FirstOrDefault();

                        string rmaInfo = "";
                        if (retInventory != null) // it confirms a device was done the receiving scan properly.
                        {
                            if (returnDevice.NonReturnID == null)
                            {
                                rmaInfo = "RMA#: " + retInventory.ReturnID + " , scanned on: " + retInventory.CommittedDate.ToString() + " , BIN: "
                                        + ((retInventory.rma_Return.rma_ref_ReturnReason.rma_ref_Bin == null) ? "N/A" : (String.IsNullOrEmpty(retInventory.rma_Return.rma_ref_ReturnReason.rma_ref_Bin.Bin) ? "N/A" : retInventory.rma_Return.rma_ref_ReturnReason.rma_ref_Bin.Bin));

                                this.SetErrorMessage("Dosimeter has already been scanned and added into receiving inventory. Duplicate receiving scan is not allowed.</br>" + rmaInfo);
                                SetFocus(this.txtSerialNo);
                                return false;
                            }
                        }
                        else // missing ReturnInventories record, receiving scan improperly
                        {
                            this.SetErrorMessage("Dosimeter has already been scanned and added into receiving inventory. Duplicate receiving scan is not allowed.</br>" + rmaInfo);
                            SetFocus(this.txtSerialNo);
                            return false;
                        }
                    }
                }
            }

            // Find rma_ReturnDevices wich are Active, but has not been received.
            returnDevice = (from rd in adc.rma_ReturnDevices
                            join r in adc.rma_Returns on rd.ReturnID equals r.ReturnID
                            where rd.SerialNo == serialNo && r.Active == true && rd.Active == true
                            && rd.Received == false
                            orderby rd.ReturnDevicesID descending
                            select rd).FirstOrDefault();

            // Do not allow if return by IIM Account and without RMA request
            if (returnDevice == null)
            {
                string AccountIDToCheck = "7257";

                if (accountDevice != null && AccountIDToCheck.IndexOf(accountDevice.AccountID.ToString()) >= 0)
                {
                    this.SetErrorMessage("UNAUTHORIZED DOSIMETER RETURN: Dosimeter belongs to Instadose In-transit Monitoring Account #7257. Do not proceed. Refer to Instadose Technical Department.");
                    SetFocus(this.txtSerialNo);
                    return false;
                }
            }

            // Validate BIN of the return reason to see if it was set up yet.
            int return_ReasonID = 7; //Devices arrived at facility without Request No, add by Mfg
            if (returnDevice != null)
            {
                return_ReasonID = returnDevice.rma_Return.Return_ReasonID.HasValue ? returnDevice.rma_Return.Return_ReasonID.Value : 7;
            }

            rma_ref_ReturnReason returnReason = (from rs in adc.rma_ref_ReturnReasons where rs.ReasonID == return_ReasonID select rs).FirstOrDefault();
            if (returnReason == null)
            {
                this.SetErrorMessage("The return reason for this Dosimeter does not exist in the system.");
                SetFocus(this.txtSerialNo);
                return false;
            }
            else
            {
                if (returnReason.rma_ref_Bin == null || string.IsNullOrEmpty(returnReason.rma_ref_Bin.Bin))
                {
                    this.SetErrorMessage("Scan is not allowed until the BIN for the return reason: <b>" + returnReason.Description.ToUpper() + "</b> is setup.");
                    SetFocus(this.txtSerialNo);
                    return false;
                }
            }

            // Validating of using previous package
            // Only allowing if they are the same account. Else do not allow.   
            if (this.hidPreviousPackageClick.Value == "1")
            {
                int currentScannedActID = 2;
                if (accountDevice != null)
                {
                    currentScannedActID = accountDevice.AccountID;
                }

                if (!string.IsNullOrEmpty(this.hidCurrentActID.Value) && int.Parse(this.hidCurrentActID.Value) != currentScannedActID)
                {
                    resetControls();
                    this.SetErrorMessage("Could not use the previous package on the scanned serial number: " + serialNo + " because it's account is different with the previous package's account.");
                    return false;
                }
            }
        }
        return true;
    }
    private rma_ref_Department getRequestDepartment(int returnID)
    {
        // find the department of rma request
        return (from rt in adc.rma_Returns
                join rtT in adc.rma_ref_ReturnTypes
                on rt.ReturnTypeID equals rtT.ReturnTypeID
                join rtD in adc.rma_ref_Departments
                on rtT.DepartmentID equals rtD.DepartmentID
                where rt.ReturnID == returnID
                select rtD).FirstOrDefault();
    }
    private void RMAReceive(ref int returnReasonID)
    {
        int returnID = 0;

        // ************************** Update/Insert ReturnDevice *******************************//                 
        if (returnDevice != null)
        {
            returnID = returnDevice.ReturnID;

            // Update received ReturnDevice
            returnDevice.Status = 4; // Device(s) Received and is waiting to be reviewed.
            returnDevice.DepartmentID = 7; // Mfg - Receiving
            returnDevice.Received = true;
            // Save changes to the database
            adc.SubmitChanges();

            // insert TransLog, update ReturnDevice            
            adc.sp_rma_process(returnID, returnDevice.ReturnDevicesID, 0, " ", this.UserName, DateTime.Now, "UPDATE DEVICE", "DeviceStatus=4, DepartmentID=7", 2);
        }
        else
        {
            // ******************** Create RMA return without Request No, add by Mfg **************************//

            // If device does not associate with any account then use Mirion act = 2
            int accountID = 2;

            if (accountDevice != null)
            {
                accountID = accountDevice.AccountID;
            }

            // Create Return Request for account and get the new generated ReturnID
            rma_Return rma = null;
            rma = new rma_Return()
            {
                AccountID = accountID,
                Active = true,
                CreatedBy = this.UserName,
                CreatedDate = DateTime.Now,
                Notes = "Auto generated by inventory entry.",
                Reason = (from rr in adc.rma_ref_ReturnReasons where rr.ReasonID == 7 select rr.Description).FirstOrDefault(),
                ReturnTypeID = 5, // Device was mailed back without Request No, Check-in by Mfg
                Status = 1, // Awaiting all devices returned.
                Return_ReasonID = 7 //Devices arrived at facility without Request No, add by Mfg
            };
            adc.rma_Returns.InsertOnSubmit(rma);
            adc.SubmitChanges();

            returnID = rma.ReturnID;

            // insert TransLog, add new Retrun
            adc.sp_rma_process(returnID, 0, 0, "Auto generated by inventory entry.", this.UserName, DateTime.Now, "ADD RETRUN", "Device was mailed back without Request number.", 2);

            // Create a ReturnDevice for no RMA device.            
            returnDevice = new rma_ReturnDevice()
            {
                Active = true,
                Notes = "Auto generated by inventory entry.",
                ReturnID = returnID,
                //MasterDeviceID = deviceInventory.DeviceID,
                SerialNo = _isInstalink ? productInventory.SerialNo : deviceInventory.SerialNo,
                DepartmentID = 7, // Mfg - Receiving
                Received = true, // indicated received device
                Status = 4 // Device(s) Received and is waiting to be reviewed.
            };

            if (_isInstalink)
                returnDevice.ProductInventoryID = productInventory.ProductInventoryID;
            else
                returnDevice.MasterDeviceID = deviceInventory.DeviceID;

            // Create the new returned device
            adc.rma_ReturnDevices.InsertOnSubmit(returnDevice);
            // Save changes to the database
            adc.SubmitChanges();

            // insert TransLog for new ReturnDevice            
            adc.sp_rma_process(returnID, returnDevice.ReturnDevicesID, 0, "Auto generated by inventory entry.", this.UserName, DateTime.Now, "ADD DEVICE", "DeviceStatus=4, DepartmentID=7", 2);
        }

        // ****************** Create rmaReturnInventory record ******************************//
        rma_ReturnInventory inventory = new rma_ReturnInventory()
        {
            Active = true,
            Approved = true,
            Completed = true,
            CreatedDate = DateTime.Now,
            ReturnID = returnID,
            ReturnDeviceID = returnDevice.ReturnDevicesID,
            SerialNo = _isInstalink ? productInventory.SerialNo : deviceInventory.SerialNo,
            ReceivedNotes = "",
            ReceivedBy = this.UserName,
            ReceivedDate = DateTime.Now,
            TechOpsNotes = "",
            TechOpsApproved = true,
            CommittedToFinance = true,
            CommittedDate = DateTime.Now
        };

        adc.rma_ReturnInventories.InsertOnSubmit(inventory);
        adc.SubmitChanges();

        // insert TransLog for adding new rma_ReturnInventories        
        adc.sp_rma_process(returnID, returnDevice.ReturnDevicesID, inventory.ReturnInventoryID, "", this.UserName, DateTime.Now, "ADD INVENTORY", "Add inventory Serial#: " + inventory.SerialNo, 2);

        // ******************** Refresh the data source *******************************//
        adc.Refresh(System.Data.Linq.RefreshMode.KeepChanges);

        // ************** Check if all of the devices been received for a rma_Return ******************//
        try
        {
            // Requery the return and check to make sure the status is currently set to "Awaiting devices.." (1)
            var rmaReturn = (from r in adc.rma_Returns
                             where r.ReturnID == returnID && r.Active == true
                             select r).FirstOrDefault();

            // Get return_reasonID of a RMA and assign it back to ref variable
            returnReasonID = rmaReturn.Return_ReasonID.HasValue ? rmaReturn.Return_ReasonID.Value : 0;

            // Count the number of devices supposed to be returned for a RMA.
            int rmaDeviceCount = (from r in adc.rma_Returns
                                  join d in adc.rma_ReturnDevices on r.ReturnID equals d.ReturnID
                                  where r.ReturnID == returnID
                                  && r.Active == true && d.Active == true
                                  select d).Count();

            // Count the number of devices received for a RMA.
            int rmaDeviceReceivedCount = (from r in adc.rma_Returns
                                          join d in adc.rma_ReturnDevices on r.ReturnID equals d.ReturnID
                                          where r.ReturnID == returnID
                                          && r.Active == true && d.Active == true
                                          && d.Received == true
                                          select d).Count();

            if (rmaDeviceCount == rmaDeviceReceivedCount)
            {
                // Yes, update the return request status to ... received.
                rmaReturn.Status = 6; // All device(s) Received by Mfg and is waiting to be reviewed. 
                rmaReturn.Completed = true;
                rmaReturn.CompletedDate = DateTime.Now;
                adc.SubmitChanges();

                // insert TransLog, update Retrun Status
                adc.sp_rma_process(returnID, 0, 0, " ", this.UserName, DateTime.Now, "UPDATE RETURN", "Update Request status to 6.", 2);

            }
            else
            {
                // No, update the return request status to partial received.
                rmaReturn.Status = 5;  //Partial device(s) have been received by Mfg                
                adc.SubmitChanges();

                // insert TransLog, update Retrun Status
                adc.sp_rma_process(returnID, 0, 0, " ", this.UserName, DateTime.Now, "UPDATE RETURN", "Update Request status to 5.", 2);
            }

            // Clean all previous rma records of this serialno
            CleanPreviousRMARecordsBySerialno(returnID, inventory.SerialNo);

            // ------------------------------- Deactivate a device off an user and account. -------------------------------------// 

            // Change the Device Analysis Status. This is for Automation Inventory
            // Need to classify if this is a Return Request or Recall Request
            string changedDeviceAnalysisStatus = "Returned-Receiving"; // For un-expected returns and Return Request 

            // If device returns to department "Tech Ops", we know this is a Recall
            if (rmaReturn.rma_ref_ReturnType.Type == "Recall")
                changedDeviceAnalysisStatus = "Recalled-Receiving";

            var refurbishList = (from rs in adc.rma_ref_ReturnReasons
                                 where rs.Active == true && rs.IsRefurReceiving == true
                                 select rs.Description).ToList();
            //string[] refurbishList = { "Broken", "Cust no longer requires service", "General Return", "General Return (Tech recvd)", "Others", "Badge Replace - Mgr Perm Needed",
            //        "Physical Damage - Bumper", "Physical Damage - Cap", "Physical Damage - Case", "Physical Damage - Clip", "Physical Damage - USB" }; // All return reasons for refurbishment RET
            if (refurbishList.Any(rmaReturn.rma_ref_ReturnReason.Description.Contains))
            {
                changedDeviceAnalysisStatus = "Refurbished-Receiving";
            }

            if (_isInstalink)
            {
                //// Insert DeviceInventoryAudit record
                //DeviceManager myDeviceMnager = new DeviceManager();
                //myDeviceMnager.InsertDeviceInventoryAudit(deviceInventory.DeviceID, deviceInventory.DeviceAnalysisStatus.DeviceAnalysisStatusID, deviceInventory.FailedCalibration, "Receive Inventory", "Portal");

                // ********** Deactivate AccountDevice ********************//
                IQueryable<AccountProduct> ADev = (from a in idc.AccountProducts
                                                   join pi in idc.ProductInventories on a.ProductInventoryID equals pi.ProductInventoryID
                                                   where a.ProductInventoryID == productInventory.ProductInventoryID && (a.Active == true || a.CurrentDeviceAssign == true)
                                                   select a).AsQueryable();
                foreach (AccountProduct ad in ADev)
                {
                    ad.Active = false;
                    ad.DeactivateDate = DateTime.Now;
                    ad.CurrentDeviceAssign = false;
                    idc.SubmitChanges();
                }
                // ********** Deactivate AccountDevice ********************//

                // insert TransLog
                adc.sp_rma_process(returnID, 0, 0, " ", this.UserName, DateTime.Now, "DEACTIVATE", "Deactive Serial# " + inventory.SerialNo, 2);
            }
            else
            {
                // Set the device firstread is NULL when it comes back to Shipping/Receiving dept. 
                // This makes sure the next wearer who receives the device must go through initialize process.
                deviceInventory.FirstReadID = null;

                DeviceAnalysisStatus returnedStatus = (from s in idc.DeviceAnalysisStatus where s.DeviceAnalysisName == changedDeviceAnalysisStatus select s).FirstOrDefault();
                if (returnedStatus != null)
                {
                    deviceInventory.DeviceAnalysisStatus = returnedStatus;
                }
                idc.SubmitChanges();


                // Insert DeviceInventoryAudit record
                DeviceManager myDeviceMnager = new DeviceManager();
                myDeviceMnager.InsertDeviceInventoryAudit(deviceInventory.DeviceID, deviceInventory.DeviceAnalysisStatus.DeviceAnalysisStatusID, deviceInventory.FailedCalibration, "Receive Inventory", "Portal");


                // ************* Deactivate UserDevice ********************** //
                IQueryable<UserDevice> UDev = (from a in idc.UserDevices
                                               where a.DeviceID == deviceInventory.DeviceID
                                               && a.Active == true
                                               select a).AsQueryable();
                foreach (UserDevice ud in UDev)
                {
                    ud.Active = false;
                    ud.DeactivateDate = DateTime.Now;
                    idc.SubmitChanges();
                }
                // ************* Deactivate UserDevice ********************** //

                // ********** Deactivate AccountDevice ********************//
                IQueryable<AccountDevice> ADev = (from a in idc.AccountDevices
                                                  where a.DeviceID == deviceInventory.DeviceID && (a.Active == true || a.CurrentDeviceAssign == true)
                                                  select a).AsQueryable();
                foreach (AccountDevice ad in ADev)
                {
                    ad.Active = false;
                    ad.DeactivateDate = DateTime.Now;
                    ad.CurrentDeviceAssign = false;
                    idc.SubmitChanges();
                }
                // ********** Deactivate AccountDevice ********************//

                // insert TransLog
                adc.sp_rma_process(returnID, 0, 0, " ", this.UserName, DateTime.Now, "DEACTIVATE", "Deactive Serial# " + deviceInventory.SerialNo, 2);
                idc.sp_InitID3ConifgurationRecord(deviceInventory.SerialNo, true);
            }


            // -------------------------------------- Deactivate a device off an user and account. ----------------------------------------------//    
        }
        catch { }
    }
    private void CleanPreviousRMARecordsBySerialno(int pReturnID, string pSerialNo)
    {
        try
        {
            // Get all RMA ReturnDevice in the past
            IQueryable<rma_ReturnDevice> rdList = (from rd in adc.rma_ReturnDevices
                                                   where rd.SerialNo == pSerialNo && rd.ReturnID < pReturnID
                                                   && rd.Received == false
                                                   select rd).AsQueryable();
            foreach (rma_ReturnDevice rd in rdList)
            {
                rd.Received = true;
                rd.Status = 4;          // Device(s) Received and is waiting to be reviewed
                rd.DepartmentID = 7;    // Mfg - Receiving                
                adc.SubmitChanges();
            }

            // Get all RMA ReturnInventory in the past
            IQueryable<rma_ReturnInventory> riList = (from ri in adc.rma_ReturnInventories
                                                      where ri.SerialNo == pSerialNo && ri.ReturnID < pReturnID
                                                      && ri.Completed == false
                                                      select ri).AsQueryable();
            foreach (rma_ReturnInventory ri in riList)
            {
                ri.TechOpsNotes = "System filled";
                ri.CommittedDate = DateTime.Now;
                ri.TechOpsApproved = true;
                ri.Approved = true;
                ri.Active = true;
                ri.Completed = true;
                ri.CommittedToFinance = true;
                adc.SubmitChanges();
            }
        }
        catch { }
    }
    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        string serialNo = this.txtSerialNo.Text.Trim();
        string CSTASKno = String.Empty;

        try
        {
            lblExtractionMessage.InnerHtml = "";
            hdnSnowSysID.Value = "";
            hdnTotalDays.Value = "";
            hdnSnowTaskStatus.Value = "";
            SetErrorMessage("");
            if (passInputsValidation())
            {
                DeviceSpecialInstruction diSpecialInstruction = (from dsi in adc.DeviceSpecialInstructions where dsi.SerialNo == serialNo && dsi.Active == true orderby dsi.CreatedDate descending select dsi).FirstOrDefault();
                if (diSpecialInstruction != null)
                {
                    lblSpecialInstruction.InnerHtml = diSpecialInstruction.SpecialInstruction;

                    if (diSpecialInstruction.SpecialInstruction.ToLower().StartsWith("cstask"))
                    {
                        CSTASKno = diSpecialInstruction.SpecialInstruction;
                        hdnSnowSysID.Value = fetchSnowTask(CSTASKno, serialNo);
                    }

                    ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('specialInstructionDialog')", true);
                }
                else
                {
                    ScanSubmit();
                }
            }
        }
        catch (Exception ex)
        {
            SetErrorMessage(ex.Message);
            var propInsight = new Dictionary<string, string>() {
                { "CustomMessage", "Error submitting serial number, btnSubmit_Click failed." }
            };
            if (deviceInventory != null) propInsight.Add("SerialNo", serialNo);
            clientTelemetry.TrackException(ex, propInsight);
        }
    }

    protected string fetchSnowTask(string CSTASKNo, string SerialNo)
    {
        var propInsight = new Dictionary<string, string>() {
                                { "CSTASKNo", CSTASKNo },
                                { "SerialNumber", SerialNo}
        };

        List<SnowTask> result = SnowAPI.GetSnowTask(CSTASKNo);
        if (result.Count > 0)
        {
            SnowTask cstask = result.First();
            int days = Convert.ToInt32((DateTime.Now - cstask.task_date).TotalDays);
            int cstaskStatus = Convert.ToInt32(cstask.state);

            hdnTotalDays.Value = days.ToString();
            hdnSnowTaskStatus.Value = cstaskStatus.ToString();

            if (cstaskStatus == SnowStatusClosed)
            {
                lblExtractionMessage.InnerHtml = $"<br/>The task is already closed.";
            }
            else
            {
                string message = days > 0 ? $"{days} days ago" : "Today";
                lblExtractionMessage.InnerHtml = $"Created {message}. {cstask.task_date.ToString("MMMM dd, yyyy")}.";
            }

            clientTelemetry.TrackTrace("Successfully fetched CSTASK from SNOW", propInsight);
            return cstask.sys_id;

        }
        else
        {
            throw new InvalidOperationException($"The service now ticket: {CSTASKNo} does not exist.");
        }

    }

    protected void updateSnowTask()
    {
        if (hdnSnowSysID.Value != "")
        {
            string serialNo = txtSerialNo.Text.Trim();
            var propInsight = new Dictionary<string, string>() {
                { "SerialNumber", serialNo},
                { "sys_id", hdnSnowSysID.Value }
            };

            var snowTaskStatus = hdnSnowTaskStatus.Value;
            if (snowTaskStatus.Trim() == "")
            {
                clientTelemetry.TrackTrace($"Invalid SNOW status code: {snowTaskStatus}, skip updating SNOW Task", propInsight);
                return;
            }

            if (Convert.ToInt32(snowTaskStatus) == SnowStatusClosed)
            {
                clientTelemetry.TrackTrace($"Snow task is already closed, skip updating SNOW Task", propInsight);
                return;
            }

            var sysId = hdnSnowSysID.Value;
            var totalDays = Convert.ToInt32(hdnTotalDays.Value);
            hdnSnowSysID.Value = "";
            hdnTotalDays.Value = "";
            // Test the limit by 45 days but use 30 days in the message
            string actionMessage = totalDays <= MaxDayLimitState ? $"Arrived within 30 days, thus, extraction has begun." : $"Over the 30 days limit and considered closed.";
            int newState = totalDays <= MaxDayLimitState ? SnowStatusInprogress : SnowStatusClosed;
            SnowAPI.UpdateSnowTask(sysId, $"Badge has arrived at the facility on {DateTime.Now.ToString("MM/dd/yyyy")}. {actionMessage}", newState);

            clientTelemetry.TrackTrace("Successfully updated CSTASK from SNOW", propInsight);
        }
    }

    protected void btnSpecialInstructionOK_Click(object sender, EventArgs e)
    {
        string serialNo = txtSerialNo.Text.Trim();

        try
        {
            deviceInventory = (from di in idc.DeviceInventories where di.SerialNo == serialNo select di).FirstOrDefault();

            // Find the CURRENT assigned AccountID
            accountDevice = (from ad in idc.AccountDevices
                             where ad.DeviceID == deviceInventory.DeviceID
                             && ad.CurrentDeviceAssign == true
                             orderby ad.AssignmentDate descending
                             select ad).FirstOrDefault();

            // Find Serial Number in rma_ReturnDevices table that are Active, but not Returned/Received.
            returnDevice = (from rd in adc.rma_ReturnDevices
                            join r in adc.rma_Returns on rd.ReturnID equals r.ReturnID
                            where rd.SerialNo == serialNo && rd.Active == true && rd.Received == false
                            && r.Active == true
                            orderby rd.ReturnDevicesID descending
                            select rd).FirstOrDefault();

            DeviceSpecialInstruction diSpecialInstruction = (from dsi in adc.DeviceSpecialInstructions where dsi.SerialNo == serialNo && dsi.Active == true orderby dsi.CreatedDate descending select dsi).FirstOrDefault();
            if (diSpecialInstruction != null)
            {
                updateSnowTask();

                diSpecialInstruction.Active = false;
                diSpecialInstruction.ModifiedDate = System.DateTime.Now;
                adc.SubmitChanges();
            }

            ScanSubmit();

            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('specialInstructionDialog')", true);

        }
        catch (Exception ex)
        {
            // Display the system generated error message.
            SetErrorMessage(ex.Message);

            var propInsight = new Dictionary<string, string>() {
                { "CustomMessage", "Error submitting serial number, btnSpecialInstructionOK_Click failed." }
            };
            if (deviceInventory != null) propInsight.Add("SerialNo", serialNo);
            clientTelemetry.TrackException(ex, propInsight);
        }
    }
    private int GetCommitPackageID()
    {
        int currentDevActID = 2;
        if (accountDevice != null)
        {
            currentDevActID = accountDevice.AccountID;
        }

        if (this.hidCurrentActID.Value != currentDevActID.ToString())
        {
            if (this.hidCurrentActID.Value != "")
            {
                this.packageID.Value = (int.Parse(this.packageID.Value) + 1).ToString();
            }
            this.hidCurrentActID.Value = currentDevActID.ToString();
        }

        return int.Parse(this.packageID.Value);
    }
    private void ScanSubmit()
    {
        int returnReasonID = 0;

        RMAReceive(ref returnReasonID);

        rma_ReturnPackage rec = new rma_ReturnPackage
        {
            //PackageID = int.Parse(this.packageID.Value),
            PackageID = GetCommitPackageID(),
            SerialNo = _isInstalink ? productInventory.SerialNo : this.deviceInventory.SerialNo,
            ReturnReasonID = returnReasonID,
            CreatedDate = System.DateTime.Now,
            CreatedBy = this.UserName
        };

        adc.rma_ReturnPackages.InsertOnSubmit(rec);
        adc.SubmitChanges();

        // refresh gridview
        grdViewReturnPackage.DataBind();

        string bin;
        bool newSerialRange = false;

        if (rec.SerialNo.StartsWith("23") && rec.SerialNo.Length == 8)
        {
            bin = ". Put in ID++ bucket.";
            newSerialRange = true;
        }
        else
            bin = ((rec.rma_ref_ReturnReason.rma_ref_Bin == null) ? "N/A" : (String.IsNullOrEmpty(rec.rma_ref_ReturnReason.rma_ref_Bin.Bin) ? "N/A" : rec.rma_ref_ReturnReason.rma_ref_Bin.Bin));

        // ******************** display message ***************//
        string returnMsg = "Scanned SN#: " + rec.SerialNo + (newSerialRange ? bin : ". BIN: " + bin);

        if (newSerialRange)
        {
            this.tdCommitMsg1.Style.Add("display", "none");
            this.tdCommitMsg2.ColSpan = 3;
            this.lblCommitMsg.Style.Add("color", "Red");
            this.lblCommitMsg.Style.Add("font-size", "x-large");
        }
        else
        {
            this.tdCommitMsg2.ColSpan = 2;
            this.tdCommitMsg1.Style.Add("display", "block");
            this.lblCommitMsg.Style.Add("color", "Blue");
            this.lblCommitMsg.Style.Add("font-size", "large");
        }

        this.lblCommitMsg.InnerHtml = returnMsg;

        var propInsight = new Dictionary<string, string>() {
               { "SerialNumber", txtSerialNo.Text},
        };
        clientTelemetry.TrackTrace("Successfully scanned and submitted the device.", propInsight);

        // ******************** done **************************//                
        this.txtSerialNo.Text = "";
        SetFocus(this.txtSerialNo);

        this.hidPreviousPackageClick.Value = "";
    }
    //private rma_ReturnInventory getReturnInventory(string serialNumber)
    //{
    //    // Get an existing ReturnInventory record which have not received by Techops yet                     
    //    return (from rd in adc.rma_ReturnDevices
    //            join rt in adc.rma_Returns on rd.ReturnID equals rt.ReturnID
    //            join ri in adc.rma_ReturnInventories on rd.ReturnID equals ri.ReturnID
    //            where rd.SerialNo == serialNumber && ri.SerialNo == serialNumber
    //            && rd.Active == true && rt.Active == true && ri.Active == true
    //            && ri.CommittedToFinance == null
    //            orderby rt.CreatedDate descending
    //            select ri).FirstOrDefault();
    //}
    //private rma_ReturnInventory getCompleteReturnInventory(string serialNumber)
    //{
    //    // Get an existing ReturnInventory record which have received by Techops                     
    //    return (from rd in adc.rma_ReturnDevices
    //            join rt in adc.rma_Returns on rd.ReturnID equals rt.ReturnID
    //            join ri in adc.rma_ReturnInventories on rd.ReturnID equals ri.ReturnID
    //            where rd.SerialNo == serialNumber && ri.SerialNo == serialNumber
    //            && rd.Active == true && rt.Active == true && ri.Active == true
    //            && ri.CommittedToFinance == true
    //            orderby rt.CreatedDate descending
    //            select ri).FirstOrDefault();
    //}
    private void SetErrorMessage(string error)
    {
        this.errorMsg.InnerHtml = error;
        this.error.Visible = !String.IsNullOrEmpty(error);
    }
    protected void grdViewReturnPackage_RowDataBound(object sender, GridViewRowEventArgs e)
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
}