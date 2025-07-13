using System;
using System.IO;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Collections;
using System.Configuration;
using System.Data.SqlClient;

using Instadose;
using Instadose.Data;
using Mirion.DSD.Consumer;

/// <summary>
/// Summary description for MasterBaselineUploadIDPlus
/// </summary>
public class MasterBaselineUploadIDPlus
{
    private const string NEWDEVICE_GROUP = "PRECALAPP";
    private const string NEWDEVICE_STATUS = "Unknown";
    private const int NEWDEVICE_PRODUCT = 30;            // will change later for the real ItemNumber
    private const string NEWDEVICE_SKU = "INSTA PLUS";     // will change later for the real ItemNumber
    private const string NEWMATERIAL_SKU = "PCBA ID PLUS";
    private const string NEWDEVICE_COLOR = "No Color";
    private const float NEWDEVICE_HARDWAREVERSION = 2;
    private const double BLTempDiff = 1000.0;
    private const int DLLowRange = 145000;
    private const int DHLowRange = 145000;
    private const int DLHighRange = 195000;
    private const int DHHighRange = 225000;
    private const int DLTDHTLowRange = 28000;
    private const int DLTDHTHighRange = 35000;

    private const string DeviceAnalysisName_RFT = "Ready for Testing";
    private const string DeviceAnalysisName_MM = "Missing Master Data";
    private const string DeviceAnalysisName_MB = "Missing Baseline";
    private const string DeviceAnalysisName_BLTM = "BLT Malfunction";
    private const string DeviceAnalysisName_DOR = "Dose Out of Range";
    
    private string receiptID;
    private InsDataContext IDC = new InsDataContext();
    private CSVParser masterParser = new CSVParser();    
    private string Message = "";
    private DataTable dtValidationError = new DataTable();
    private List<string> snInMAS = new List<string>();

    public MasterBaselineUploadIDPlus(StreamReader pMasterFileContent, string pReceiptID)
    {        
        this.receiptID = pReceiptID;
        masterParser.Parse(pMasterFileContent);
        
    }

    public DataTable ValidationErrorMessage()
    {
        return this.dtValidationError;
    }

    private void InitiateValidationErrorTable()
    {
        dtValidationError = null;
        dtValidationError = new DataTable();

        // if Commit = false, it means it needs to correct the data before continuing upload.
        // if Commit = true, it means it does not need to correct the data and continuing upload.
        dtValidationError.Columns.Add("Commit", typeof(bool)); 
        dtValidationError.Columns.Add("Sn", typeof(string));
        dtValidationError.Columns.Add("MacAddress", typeof(string));
        dtValidationError.Columns.Add("ErrorMessage", typeof(string));
        dtValidationError.Columns.Add("ValidationType", typeof(string));
    }

    private void InsertValidationErrorTable(bool pFlag, string pSn, string pMacAddress, string pErrorMessage, string pValidationType)
    {
        DataRow row = dtValidationError.NewRow();

        row["Commit"] = pFlag;
        row["Sn"] = pSn;
        row["MacAddress"] = pMacAddress;
        row["ErrorMessage"] = pErrorMessage;
        row["ValidationType"] = pValidationType;

        // Add the row to the DataTable.
        dtValidationError.Rows.Add(row);
    }

    //  -------------------- CHECK ANY PO WAITING FOR POSTING IN MAS -------------------------------//
    private bool IsMASAvailableForPOUpload()
    {        
        try
        {
            String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            String cmdStr = "sp_GetAllPOEntries";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();
            sqlCmd.CommandType = CommandType.StoredProcedure;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);
            sqlConn.Close();

            if (dt != null && dt.Rows.Count > 0)
            {
                InsertValidationErrorTable(false, "", "", "Please ask Finance to post the previous PO uploads before doing your next PO upload.", "IsMASAvailableForPOUpload");
                return false;
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "","", "IsMASAvailableForPOUpload: " + ex.ToString(), "IsMASAvailableForPOUpload");
            return false;
        }
        return true;
    }

    //  -------------------- PARSE MASTER -------------------------------//
    private bool MasterFileParsingValidation()
    {
        try
        {
            
            //masterParser.Parse(this.masterFileContent);
            if (masterParser.Data == null)
            {
                InsertValidationErrorTable(false, "","", "Could not parse Master file.", "MasterFileParsingValidation");                
                return false;
            }
        }
        catch(Exception ex)
        {
            InsertValidationErrorTable(false, "","", "MasterFileParsingValidation: " + ex.ToString(), "MasterFileParsingValidation");
            return false;
        }
        return true;
    }    

    //  -------------------- MASTER DATA SCHEMA -----------------------------//
    private bool MasterFileDataSchemaValidation()
    {
        bool dataSchemaIsCorrect = true;
        try
        {
            // Check each column exists in the data table that was returned.
            if (masterParser.Data.Columns["PO"] == null) dataSchemaIsCorrect = false;
            if (masterParser.Data.Columns["Invoice Date"] == null) dataSchemaIsCorrect = false;
            if (masterParser.Data.Columns["Invoice No."] == null) dataSchemaIsCorrect = false;
            if (masterParser.Data.Columns["Shipping Invoice No."] == null) dataSchemaIsCorrect = false;
            if (masterParser.Data.Columns["Shipment Date (YYMMDD)"] == null) dataSchemaIsCorrect = false;
            if (masterParser.Data.Columns["PCBA Serial"] == null) dataSchemaIsCorrect = false;
            if (masterParser.Data.Columns["MAC Address"] == null) dataSchemaIsCorrect = false; 

            if (!dataSchemaIsCorrect)
            {                
                InsertValidationErrorTable(false, "","", "Master file data schema does not match the required format.", "MasterFileDataSchemaValidation");
                return false;
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "","", "MasterFileDataSchemaValidation: " + ex.ToString(), "MasterFileDataSchemaValidation");
            return false;
        }
        return true;
    }
    
    //  -------------------- MASTER ROW MISSING DATA --------------------------//
    private bool MasterRowDataValidation()
    {
        bool pass = true;
        try
        {
            // Loop through each of the rows
            foreach (DataRow masterRow in masterParser.Data.Rows)
            {
                // Skip if it's a blank row
                if (isMasterRowBlank(masterRow)) continue;

                if (isMasterRowMissingData(masterRow))
                {                    
                    string message = "Master file, S/N: " + masterRow["PCBA Serial"].ToString().Trim() + " is missing data.";
                    InsertValidationErrorTable(false, masterRow["PCBA Serial"].ToString().Trim(), masterRow["MAC Address"].ToString().Trim(), message, "MasterRowDataValidation");
                    pass = false;
                }
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "","", "MasterRowDataValidation: " + ex.ToString(), "MasterRowDataValidation");
            pass = false;
        }
        return pass;
    }

    //  -------------------- CHECK MASTER ROW DUPLICATE S/N -------------------//
    private bool MasterRowDuplicateValidation()
    {
        bool pass = true;
        Hashtable snHash = new Hashtable();
        try
        {
            // Loop through each of the rows
            foreach (DataRow masterRow in masterParser.Data.Rows)
            {
                // Skip if it's a blank row
                if (isMasterRowBlank(masterRow)) continue;

                if (!snHash.ContainsKey(masterRow["PCBA Serial"].ToString().Trim()))
                {
                    snHash.Add(masterRow["PCBA Serial"].ToString().Trim(), "");
                }
                else
                {                    
                    string message = "Master file, S/N: " + masterRow["PCBA Serial"].ToString().Trim() + " is duplicated.";
                    InsertValidationErrorTable(false, masterRow["PCBA Serial"].ToString().Trim(), masterRow["MAC Address"].ToString().Trim(), message, "MasterRowDuplicateValidation");
                    pass = false;                        
                }
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "","", "MasterRowDuplicateValidation: " + ex.ToString(), "MasterRowDuplicateValidation");
            pass = false;
        }
        return pass;
    }

    //  -------------------- CHECK MASTER ROW S/N HAS CORRECT FORMAT -------------------//
    private bool MasterSerialNoFormatValidation()
    {
        bool pass = true;

        try
        {
            // Loop through each of the rows
            foreach (DataRow masterRow in masterParser.Data.Rows)
            {
                // Skip if it's a blank row
                if (isMasterRowBlank(masterRow)) continue;

                // Ensure the serial number was grabbed.
                string serialNo = masterRow["PCBA Serial"].ToString().Trim();

                // Do not upload onto MAS if the device's serialno is out of format.
                if (Int32.Parse(serialNo) >= 30000000 || Int32.Parse(serialNo) < 20000000)
                {
                    string message = "Master file, S/N: " + serialNo + " is out of the format 2xxxxxxx.";
                    InsertValidationErrorTable(false, serialNo, masterRow["MAC Address"].ToString().Trim(), message, "MasterSerialNoFormatValidation");
                    pass = false;
                }
            }

        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false,"", "", "MasterSerialNoFormatValidation: " + ex.ToString(), "MasterSerialNoFormatValidation");
            pass = false;
        }
        return pass;
    }

    //  -------------------- CHECK MASTER ROW S/N EXIST IN INVENTORY -------------------//
    private bool MasterSerialNoExistValidation()
    {
        bool pass = true;
        
        try
        {
            //// Get the list of s/n in MAS
            //if (snInMAS.Count() == 0)
            //{
            //    String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            //    String cmdStr = "sp_GetAllSerialNoInMASByItemNumber";

            //    SqlConnection sqlConn = new SqlConnection(connStr);
            //    SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            //    sqlConn.Open();
            //    sqlCmd.CommandType = CommandType.StoredProcedure;

            //    sqlCmd.Parameters.Add("@ItemNumber", SqlDbType.VarChar, 15);

            //    sqlCmd.Parameters["@ItemNumber"].Value = NEWMATERIAL_SKU;

            //    SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            //    DataTable dt = new DataTable();

            //    sqlDA.Fill(dt);
            //    sqlConn.Close();

            //    if (dt != null && dt.Rows.Count > 0)
            //    {
            //        snInMAS = dt.AsEnumerable().Select(x => x["LotSerialNumber"].ToString().Trim()).ToList() ;                   
            //    }
            //}

            //if (this.snInMAS.Count() > 0)
            //{
            //    // Loop through each of the rows
            //    foreach (DataRow masterRow in masterParser.Data.Rows)
            //    {
            //        // Skip if it's a blank row
            //        if (isMasterRowBlank(masterRow)) continue;

            //        // Ensure the serial number was grabbed.
            //        string serialNo = masterRow["PCBA Serial"].ToString().Trim();                    

            //        // Do not upload onto MAS if the device is already existed.
            //        if (snInMAS.Contains(serialNo))
            //        {
            //            string message = "Master file, S/N: " + masterRow["PCBA Serial"].ToString().Trim() + " is already existed in MAS.";
            //            InsertValidationErrorTable(false, masterRow["PCBA Serial"].ToString().Trim(),masterRow["MAC Address"].ToString().Trim(), message, "MasterSerialNoExistValidation");
            //            pass = false;
            //        }
            //    }
            //}            

            string serialNo = "";
            int exists = 0;
            string message = "";

            // Get the device inventory records
            IQueryable<DeviceInventory> deviceInventory = (from di in IDC.DeviceInventories select di).AsQueryable();

            // Loop through each of the rows
            foreach (DataRow masterRow in masterParser.Data.Rows)
            {
                // Skip if it's a blank row
                if (isMasterRowBlank(masterRow)) continue;

                // Ensure the serial number was grabbed.
                serialNo = masterRow["PCBA Serial"].ToString().Trim();
                exists = (from di in deviceInventory where di.SerialNo == serialNo select di.DeviceID).Count();

                // Do not upload onto DeviceInventory if the device is already existed.
                if (exists > 0)
                {
                    message = "Master file, S/N: " + masterRow["PCBA Serial"].ToString().Trim() + " is already existed in MAS.";
                    InsertValidationErrorTable(false, masterRow["PCBA Serial"].ToString().Trim(), masterRow["MAC Address"].ToString().Trim(), message, "MasterSerialNoExistValidation");
                    pass = false;
                }
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "","", "MasterSerialNoExistValidation: " + ex.ToString(), "MasterSerialNoExistValidation");
            pass = false;
        }
        return pass;
    }

    // ----- CHECK Back Order Quantity against Upload Quantity -------//
    private bool TotalBackOrderValidation()
    {
        bool pass = true;
        try
        {
            int uploadTotal = 0;
           
            foreach (DataRow masterRow in masterParser.Data.Rows)
            {
                // Skip if it's a blank row
                if (isMasterRowBlank(masterRow)) continue;
                // count the number of s/n uploaded
                uploadTotal += 1;
            }
            
            var myPOReceipt = (from a in IDC.POReceipts
                               where a.ReceiptID == Convert.ToInt32(receiptID)
                               select a).FirstOrDefault();            
            
            //int totalBackOrderQty = GetTotalBackOrderQuantityByPONumber(myPOReceipt.PONumber);
            int totalOrderQty = GetTotalOrderQuantityByPONumber(myPOReceipt.PONumber);
            int totalReceived = (from a in IDC.POReceipts
                                 join b in IDC.DeviceInventories on a.ReceiptID equals b.ReceiptID
                                 where a.PONumber == myPOReceipt.PONumber
                                 select b).Count();
            int totalBackOrderQty = totalOrderQty - totalReceived;

            // if upload greater than total of BackOrderQty by 10% then will not upload the file until the files are fixed.
            if (uploadTotal > totalBackOrderQty * 1.1)
            {
                string message = "The total of uploaded is greater than the BackOrder Quantity by 10%.";
                InsertValidationErrorTable(false, "","", message, "TotalBackOrderValidation");
                pass = false;                          
            }
            else if (uploadTotal > totalBackOrderQty)
            {
                string message = "The total of uploaded is greater than the BackOrder Quantity but less than 10%.";
                InsertValidationErrorTable(true, "","", message, "TotalBackOrderValidation");
                // No raising warning message. Upload anyway.      
            }                                  
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "","", "TotalBackOrderValidation: " + ex.ToString(), "TotalBackOrderValidation");
            pass = false;
        }

        return pass;
    }



    public bool MasterBaselineUploadValidation()
    {
        InitiateValidationErrorTable();
        
        try
        {
            //if (!IsMASAvailableForPOUpload())
            //{               
            //    return false;
            //}

            if (!MasterFileParsingValidation())
            {               
                return false;
            }

            if (!MasterFileDataSchemaValidation())
            {                
                return false;
            }            

            if (!MasterRowDataValidation())
            {                
                return false;
            }                      

            if (!MasterRowDuplicateValidation())
            {                
                return false;
            }

            if (!MasterSerialNoFormatValidation())
            {
                return false;
            }

            if (!MasterSerialNoExistValidation())
            {                
                return false;
            }           

            if (!TotalBackOrderValidation())
            {
                return false;
            }                         

            // Any validation functions below this line always return TRUE because we like to upload it anyway eventhough it fails. 
            // However, the failed validation info of each s/n will be used to update the device analysis status and establish a summary report.
            // --------------------------------------------------------------------------------------------//                                                                    
            //                                                                                             //
            //                                                                                             // 
            // --------------------------------------------------------------------------------------------//           
        }
        catch (Exception ex)
        {
            return false;
        }

        return true;
    }

    public void Execute(ref string resultMessages)
    {        
        try
        {
            //masterParser.Parse(this.masterFileContent);
            //baselineParser.Parse(this.baselineFileContent);
            // ------------------------------------ VERIFY MASTER & INSERT DEVICEI NVENTORY --------------------------------------//

            // Get the device inventory records
            IQueryable<DeviceInventory> deviceInventory =
                (from di in IDC.DeviceInventories select di).AsQueryable();
           
            int noReceived = 0; // Number of devices received per receipt
            int noUpload = 0;   // Number of devices uploaded
            int noSkip = 0;     // Number of devices skipped because already uploaded            
            int noRFT = 0;      // Number of good devices uploaded, (Ready for Testing)
            int noMM = 0;       // Number of bad devices uploaded, (Missing Master record)
            int noMB = 0;       // Number of bad devices uploaded, (Missing Baseline records)
            int noBLTM = 0;     // Number of bad devices uploaded, (BLT malfunction)
            int noDOR = 0;      // Number of bad devices uploaded, (Dose Out of Range)
            int noMfgDateOutofDate = 0;      // Number of good devices uploaded, (but Manufacturing Date is out of date)
            int noColorNotInSynched = 0;     // Number of colors not in-synched  between uploading files and Receipt Header Information   
            string colorNotInSynched = "";     // colors not in-synched  between uploading files and Receipt Header Information  

            // Set a createdDate
            DateTime createdDate = DateTime.Now;

            // Store the new devices
            List<DeviceInventory> devicesToUpload = new List<DeviceInventory>();

            // Get the default group
            DeviceGroup defaultGroup = (from dg in IDC.DeviceGroups where dg.DeviceGroupName == NEWDEVICE_GROUP select dg).FirstOrDefault();

            // Get the default status
            DeviceAnalysisStatus defaultStatus = (from ds in IDC.DeviceAnalysisStatus where ds.DeviceAnalysisName == NEWDEVICE_STATUS select ds).FirstOrDefault();

            // Convert the firmware version to a double.
            string firmwareVersion = "0.65";            

            // Create a new firmware version, that will be replaced once set.
            DeviceFirmware deviceFirmware = new DeviceFirmware();

            // Loop through each row of Master file
            foreach (DataRow row in masterParser.Data.Rows)
            {
                // Skip if the serial no is blank
                if (isMasterRowBlank(row)) continue;

                // Ensure the serial number was grabbed.
                string serialNo = row["PCBA Serial"].ToString().Trim();
                string address = row["MAC Address"].ToString().Trim();
                string detectorSN = row["DETECTOR SN"] == null ? "" : row["DETECTOR SN"].ToString().Trim();
                string boardVersion = row["Board Version"] == null ? null : row["Board Version"].ToString().Trim();

                // Do not need to perform the check for device already existed in Device Inventory/MAS overhere.
                // It is carried on in the validation to make sure no duplicate S/N submit to MAS.

                //int exists = (from di in deviceInventory where di.SerialNo == serialNo select di.DeviceID).Count();

                //// Do not add DeviceInventory record if the device exists. Skipping it.
                //if (exists > 0)
                //    noSkip ++;
                //else 
                //{
                //    // Create the new record if it does not exist  
                //    //noUpload++;
                //}

                noUpload++;

                // Parse the manufacturing date
                string mfgDay = row["Shipment Date (YYMMDD)"].ToString().Trim().Substring(4, 2);
                string mfgMonth = row["Shipment Date (YYMMDD)"].ToString().Trim().Substring(2, 2);
                string mfgYear = row["Shipment Date (YYMMDD)"].ToString().Trim().Substring(0, 2);                
                    
                // If the firmware does not match the existing record, requery.
                if (deviceFirmware.FirmwareVersion != firmwareVersion)
                {
                    deviceFirmware = (from df in IDC.DeviceFirmwares where df.FirmwareVersion == firmwareVersion select df).FirstOrDefault();
                }                

                // Create a new device record
                DeviceInventory device = new DeviceInventory()
                {
                    SerialNo = serialNo,
                    Product = (from p in IDC.Products where p.ProductSKU == NEWDEVICE_SKU && p.Color == NEWDEVICE_COLOR select p).FirstOrDefault(),
                    DeviceFirmware = deviceFirmware,
                    HardwareVersion = NEWDEVICE_HARDWAREVERSION,
                    DeviceAnalysisStatus = defaultStatus,
                    CurrentGroup = defaultGroup,
                    InitialGroup = defaultGroup,
                    CalibrationMajor = 0,
                    CalibrationMinor = 0,
                    FailedCalibration = false,
                    ReceiptID = int.Parse(this.receiptID),
                    POBoxNumber = "100",
                    BDAddress = address,
                    DetectorSN = detectorSN,
                    BoardVersion = boardVersion
                };

                // Set the manufacturing date.
                DateTime mfgDate;
                if (DateTime.TryParse(string.Format("{0}/{1}/{2}", mfgMonth, mfgDay, mfgYear), out mfgDate))
                    device.MfgDate = mfgDate;

                // Get device analysis status for a S/N
                string myDeviceAnalysisName = LookupDeviceAnalysisName(serialNo, this.dtValidationError);

                switch (myDeviceAnalysisName)
                {
                    case DeviceAnalysisName_RFT:
                        noRFT++;
                        break;
                    case DeviceAnalysisName_MM:
                        noMM++;
                        break;
                    case DeviceAnalysisName_MB:
                        noMB++;
                        break;
                    case DeviceAnalysisName_BLTM:
                        noBLTM++;
                        break;
                    case DeviceAnalysisName_DOR:
                        noDOR++;
                        break;
                }

                device.DeviceAnalysisStatus = (from ds in IDC.DeviceAnalysisStatus where ds.DeviceAnalysisName == myDeviceAnalysisName select ds).FirstOrDefault();


                // Insert device audit info, device currently in QA
                DeviceInventoryAudit deviceAudit = new DeviceInventoryAudit();
                deviceAudit.DeviceAnalysisStatus = device.DeviceAnalysisStatus;
                deviceAudit.IsFail = false;
                deviceAudit.Step = "Master Upload";
                deviceAudit.Application = "Master Baseline Upload";
                deviceAudit.TimeStamp = DateTime.Now;
                // Add DeviceInventoryAudits record
                device.DeviceInventoryAudits.Add(deviceAudit);

                // Add to the list
                devicesToUpload.Add(device);      

            }

            // Loop through all s/n, which is in Baselines but not in Master, and upload it anyway
            foreach (DataRow error in this.dtValidationError.Rows)
            {
                bool uploadAnyway = (bool) error["Commit"];
                string sn = error["Sn"].ToString().Trim();
                //string address = error["MacAddress"].ToString().Trim();
                string validType = error["ValidationType"].ToString();

                if (validType == "MasterMatchBaselineValidation" && uploadAnyway)
                {
                    // Do not need to perform the check for device already existed in Device Inventory/MAS overhere.
                    // It is taken care by the validation to make sure no duplicate S/N submit to MAS.
        
                    //int exists = (from di in deviceInventory where di.SerialNo == sn select di.DeviceID).Count();

                    //// Do not add DeviceInventory record if the device exists. Skipping it.
                    //if (exists > 0)
                    //{
                    //    noSkip++;
                    //}
                    //else 
                    //{
                    //    // Create the new record if it does not exist
                    //    //  noUpload++;
                    //}

                    noUpload++;

                    // If the firmware does not match the existing record, requery.
                    if (deviceFirmware.FirmwareVersion != firmwareVersion)
                    {
                        deviceFirmware = (from df in IDC.DeviceFirmwares where df.FirmwareVersion == firmwareVersion select df).FirstOrDefault();
                    }      

                    // Create a new device record
                    DeviceInventory device = new DeviceInventory()
                    {
                        SerialNo = sn,
                        Product = (from p in IDC.Products where p.ProductSKU == NEWDEVICE_SKU && p.Color == NEWDEVICE_COLOR select p).FirstOrDefault(),
                        DeviceFirmware = deviceFirmware,
                        HardwareVersion = NEWDEVICE_HARDWAREVERSION,
                        DeviceAnalysisStatus = defaultStatus,
                        CurrentGroup = defaultGroup,
                        InitialGroup = defaultGroup,
                        CalibrationMajor = 0,
                        CalibrationMinor = 0,
                        FailedCalibration = false,
                        ReceiptID = int.Parse(this.receiptID),
                        POBoxNumber = "100"
                    };                    

                    // Set InventorySubStatus = "Missing Master"
                    noMM++;
                    device.DeviceAnalysisStatus = (from ds in IDC.DeviceAnalysisStatus where ds.DeviceAnalysisName == DeviceAnalysisName_MM select ds).FirstOrDefault();


                    // Insert device audit info, device currently in QA
                    DeviceInventoryAudit deviceAudit = new DeviceInventoryAudit();
                    deviceAudit.DeviceAnalysisStatus = device.DeviceAnalysisStatus;
                    deviceAudit.IsFail = false;
                    deviceAudit.Step = "Master Upload";
                    deviceAudit.Application = "Master Baseline Upload";
                    deviceAudit.TimeStamp = DateTime.Now;
                    // Add DeviceInventoryAudits record
                    device.DeviceInventoryAudits.Add(deviceAudit);

                    // Add to the list
                    devicesToUpload.Add(device);

                }                
            }

            noReceived = GetTotalReceived(int.Parse(this.receiptID));
            noMfgDateOutofDate = NumberOutOfDateMfgDate(this.dtValidationError);
            noColorNotInSynched = NumberofColorsNotInSynched(this.dtValidationError);
            colorNotInSynched = ColorsNotInSynched(this.dtValidationError);

            // Collect all data for the print out report.
            resultMessages += noReceived + "_";
            resultMessages += noUpload + "_";
            resultMessages += noSkip + "_";
            resultMessages += noRFT + "_";
            resultMessages += noMM + "_";
            resultMessages += noMB + "_";
            resultMessages += noBLTM + "_";
            resultMessages += noDOR + "_";
            resultMessages += noMfgDateOutofDate + "_";

            //resultMessages += noColorNotInSynched + "_"; 
            resultMessages += (colorNotInSynched.Length > 0) ? colorNotInSynched + "_" : " " + "_";             

            // UPLOAD ALL DEVICES DATA
            IDC.DeviceInventories.InsertAllOnSubmit(devicesToUpload);
            IDC.SubmitChanges();           

        }
        catch(Exception ex)
        {
            throw ex;
        }
    }

    private string LookupDeviceAnalysisName(string pSerialNo, DataTable pValidationErrorDT)
    {        
        string deviceAnalysisName = DeviceAnalysisName_RFT;

        foreach (DataRow error in pValidationErrorDT.Rows)
        {
            bool uploadAnyway = (bool)error["Commit"];
            string sn = error["Sn"].ToString().Trim();
            string validType = error["ValidationType"].ToString();
            if (sn == pSerialNo && uploadAnyway)
            {
                if (validType == "MasterMatchBaselineValidation")
                {
                    deviceAnalysisName = DeviceAnalysisName_MM;
                    break;
                }
                else if (validType == "TotalBaselineBySnValidation")
                {
                    deviceAnalysisName = DeviceAnalysisName_MB;
                    break;
                }
                else if (validType == "BaselineTemperatureValidation")
                {
                    deviceAnalysisName = DeviceAnalysisName_BLTM;
                    break;
                }
                else if (validType == "Baseline2Validation")
                {
                    deviceAnalysisName = DeviceAnalysisName_DOR;
                    break;
                }                
            }
        }

        return deviceAnalysisName;
    }

    private int GetTotalReceived(int pReceiptID)
    {
        try
        {
            int sumReceived = 0;

            sumReceived = (from a in IDC.POReceiptDetails
                           where a.ReceiptID == pReceiptID
                           select a.QtyRecd).Sum();

            return sumReceived;
        }
        catch
        {
            return 0;
        }
    }

    private int NumberOutOfDateMfgDate(DataTable pValidationErrorDT)
    {
        int count = 0;

        foreach (DataRow error in pValidationErrorDT.Rows)
        {
            bool uploadAnyway = (bool)error["Commit"];
            string sn = error["Sn"].ToString().Trim();
            string validType = error["ValidationType"].ToString();
            if (validType == "MasterRowManufacturingDateValidation" && uploadAnyway)
            {
                count++;
            }
        }

        return count;
    }

    private int NumberofColorsNotInSynched(DataTable pValidationErrorDT)
    {
        int count = 0;

        foreach (DataRow error in pValidationErrorDT.Rows)
        {
            bool uploadAnyway = (bool)error["Commit"];
            string sn = error["Sn"].ToString().Trim();
            string validType = error["ValidationType"].ToString();
            if (validType == "TotalColorValidation" && uploadAnyway)
            {
                count++;
            }
        }

        return count;
    }

    private string ColorsNotInSynched(DataTable pValidationErrorDT)
    {
        string colorsStr = "";

        foreach (DataRow error in pValidationErrorDT.Rows)
        {
            bool uploadAnyway = (bool)error["Commit"];
            string eMesg = error["ErrorMessage"].ToString().Trim();
            string validType = error["ValidationType"].ToString();
            if (validType == "TotalColorValidation" && uploadAnyway)
            {
                if (eMesg.Contains("Black"))
                    colorsStr += ",Black";
                if (eMesg.Contains("Blue"))
                    colorsStr += ",Blue";
                if (eMesg.Contains("Green"))
                    colorsStr += ",Green";
                if (eMesg.Contains("Silver"))
                    colorsStr += ",Silver";
                if (eMesg.Contains("Pink"))
                    colorsStr += ",Pink";
            }
        }

        if (colorsStr.Length > 0)
            colorsStr = colorsStr.Substring(1);

        return colorsStr;
    }

    public int ActualNumberOfUploadedDeviceByItemNumber(string pItemNumber)
    {
        int count = 0;
        try
        {            
            foreach (DataRow masterRow in masterParser.Data.Rows)
            {
                // Skip if it's a blank row
                if (isMasterRowBlank(masterRow)) continue;

                count++;
            }

            if (pItemNumber.Contains(NEWMATERIAL_SKU))
            {
                return count;
            }
            else
            {
                return 0;
            }            
        }
        catch
        {
            return 0;
        }
    }

    private bool isBaseline2Normal(string pDL, string pDH, string pDLT, string pDHT)
    {
        try
        {            
            double dl = 0;
            double dh = 0;
            double dlt = 0; 
            double dht = 0;

            if (double.TryParse(pDL, out dl))
            {
                if ((int)dl < DLLowRange || (int)dl > DLHighRange)
                {
                    return false;
                }                
            }

            if (double.TryParse(pDH, out dh))
            {
                if ((int)dh < DHLowRange || (int)dh > DHHighRange)
                {
                    return false;
                }
            }

            if (double.TryParse(pDLT, out dlt))
            {
                if ((int)dlt < DLTDHTLowRange || (int)dlt > DLTDHTHighRange)
                {
                    return false;
                }
            }

            if (double.TryParse(pDHT, out dht))
            {
                if ((int)dht < DLTDHTLowRange || (int)dht > DLTDHTHighRange)
                {
                    return false;
                }
            }

        }
        catch
        { return false; }

        return true; 
    }

    private bool isMasterRowBlank(DataRow pMasterRow)
    {
        try
        {
            if (string.IsNullOrEmpty(pMasterRow["PO"].ToString()) && string.IsNullOrEmpty(pMasterRow["Invoice Date"].ToString())
                        && string.IsNullOrEmpty(pMasterRow["Invoice No."].ToString()) && string.IsNullOrEmpty(pMasterRow["Shipping Invoice No."].ToString())
                        && string.IsNullOrEmpty(pMasterRow["Shipment Date (YYMMDD)"].ToString()) && string.IsNullOrEmpty(pMasterRow["PCBA Serial"].ToString())
                        && string.IsNullOrEmpty(pMasterRow["MAC Address"].ToString()))
            {
                return true;
            }
        }
        catch
        {}
        return false;        
    }

    private bool isMasterRowMissingData(DataRow pMasterRow)
    {
        try
        {
            if (string.IsNullOrEmpty(pMasterRow["PO"].ToString()) || string.IsNullOrEmpty(pMasterRow["Invoice Date"].ToString())
                    || string.IsNullOrEmpty(pMasterRow["Invoice No."].ToString()) || string.IsNullOrEmpty(pMasterRow["Shipping Invoice No."].ToString())
                    || string.IsNullOrEmpty(pMasterRow["Shipment Date (YYMMDD)"].ToString()) || string.IsNullOrEmpty(pMasterRow["PCBA Serial"].ToString())
                    || string.IsNullOrEmpty(pMasterRow["MAC Address"].ToString()))
            {
                return true;
            }
        }
        catch
        { }
        return false;
    }

    private int GetTotalBackOrderQuantityByPONumber(string pPONumber)
    {
        try
        {
            String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            String cmdStr = "dbo.sp_GetTotalBackOrderQuantityByPONumber";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.Add("@PONumber", SqlDbType.NVarChar, 7);

            sqlCmd.Parameters["@PONumber"].Value = pPONumber;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);

            sqlConn.Close();

            if (dt != null && dt.Rows.Count > 0 && dt.Rows[0][0] != null)
            {
                double myTotalBackOrder;

                if (!double.TryParse(dt.Rows[0][0].ToString(), out myTotalBackOrder))
                {
                    myTotalBackOrder = 0;
                }

                return Convert.ToInt32(myTotalBackOrder);
            }
            else
            {
                return 0;
            }
        }
        catch { return 0; }
    }

    private int GetTotalOrderQuantityByPONumber(string pPONumber)
    {
        try
        {
            String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            String cmdStr = "dbo.sp_GetTotalOrderQuantityByPONumber";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.Add("@PONumber", SqlDbType.NVarChar, 7);

            sqlCmd.Parameters["@PONumber"].Value = pPONumber;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);

            sqlConn.Close();

            if (dt != null && dt.Rows.Count > 0 && dt.Rows[0][0] != null)
            {
                double totalOrderQty;

                if (!double.TryParse(dt.Rows[0][0].ToString(), out totalOrderQty))
                {
                    totalOrderQty = 0;
                }

                return Convert.ToInt32(totalOrderQty);
            }
            else
            {
                return 0;
            }
        }
        catch { return 0; }
    }
}