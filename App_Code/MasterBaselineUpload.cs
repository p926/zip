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
using Instadose.Device;
using Mirion.DSD.Consumer;

/// <summary>
/// Summary description for MasterBaselineUpload
/// </summary>
public class MasterBaselineUpload
{
    private const string NEWDEVICE_GROUP = "PRECALAPP";
    private const string NEWDEVICE_STATUS = "Unknown";
    private const float NEWDEVICE_HARDWAREVERSION = 1;
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

    //private string masterPath;
    //private string baselinePath;
    //StreamReader masterFileContent;
    //StreamReader baselineFileContent;
    private string receiptID;
    private InsDataContext IDC = new InsDataContext();
    private CSVParser masterParser = new CSVParser();
    private CSVParser baselineParser = new CSVParser();
    private string Message = "";
    private DataTable dtValidationError = new DataTable();

    public MasterBaselineUpload(StreamReader pMasterFileContent, StreamReader pBaselineFileContent, string pReceiptID)
    {
        //this.masterFileContent = pMasterFileContent;
        //this.baselineFileContent = pBaselineFileContent;
        this.receiptID = pReceiptID;
        masterParser.Parse(pMasterFileContent);
        baselineParser.Parse(pBaselineFileContent);
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
        dtValidationError.Columns.Add("ErrorMessage", typeof(string));
        dtValidationError.Columns.Add("ValidationType", typeof(string));
    }

    private void InsertValidationErrorTable(bool pFlag, string pSn, string pErrorMessage, string pValidationType)
    {
        DataRow row = dtValidationError.NewRow();

        row["Commit"] = pFlag;
        row["Sn"] = pSn;
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
                InsertValidationErrorTable(false, "", "Please ask Finance to post the previous PO uploads before doing your next PO upload.", "IsMASAvailableForPOUpload");
                return false;
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "IsMASAvailableForPOUpload: " + ex.ToString(), "IsMASAvailableForPOUpload");
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
                InsertValidationErrorTable(false, "", "Could not parse Master file.", "MasterFileParsingValidation");                
                return false;
            }
        }
        catch(Exception ex)
        {
            InsertValidationErrorTable(false, "", "MasterFileParsingValidation: " + ex.ToString(), "MasterFileParsingValidation");
            return false;
        }
        return true;
    }

    // --------------------- PARSE BASELINE ---------------------------------//
    private bool BaselineFileParsingValidation()
    {
        try
        {
            //baselineParser.Parse(this.baselineFileContent);
            if (baselineParser.Data == null)
            {
                InsertValidationErrorTable(false, "", "Could not parse Baseline file.", "BaselineFileParsingValidation");
                return false;
            }
        }
        catch(Exception ex)
        {
            InsertValidationErrorTable(false, "", "BaselineFileParsingValidation: " + ex.ToString(), "BaselineFileParsingValidation");
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
            if (masterParser.Data.Columns["sn"] == null) dataSchemaIsCorrect = false;
            if (masterParser.Data.Columns["mfg_date"] == null) dataSchemaIsCorrect = false;
            if (masterParser.Data.Columns["hw_version"] == null) dataSchemaIsCorrect = false;
            if (masterParser.Data.Columns["COLOR"] == null) dataSchemaIsCorrect = false;
            if (masterParser.Data.Columns["LOT/BOX"] == null) dataSchemaIsCorrect = false;
            if (masterParser.Data.Columns["PURCHASE ORDER"] == null) dataSchemaIsCorrect = false;
            if (masterParser.Data.Columns["INVOICE"] == null) dataSchemaIsCorrect = false;
            if (masterParser.Data.Columns["NOTE"] == null) dataSchemaIsCorrect = false;
            if (masterParser.Data.Columns["InvoiceDate"] == null) dataSchemaIsCorrect = false;

            if (!dataSchemaIsCorrect)
            {                
                InsertValidationErrorTable(false, "", "Master file data schema does not match the required format.", "MasterFileDataSchemaValidation");
                return false;
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "MasterFileDataSchemaValidation: " + ex.ToString(), "MasterFileDataSchemaValidation");
            return false;
        }
        return true;
    }

    //  -------------------- BASELINE DATA SCHEMA -----------------------------//
    private bool BaselineFileDataSchemaValidation()
    {
        bool dataSchemaIsCorrect = true;
        try
        {
            // Check each column exists in the data table that was returned.
            if (baselineParser.Data.Columns["sn"] == null) dataSchemaIsCorrect = false;
            if (baselineParser.Data.Columns["temp_id"] == null) dataSchemaIsCorrect = false;
            if (baselineParser.Data.Columns["dl"] == null) dataSchemaIsCorrect = false;
            if (baselineParser.Data.Columns["dlt"] == null) dataSchemaIsCorrect = false;
            if (baselineParser.Data.Columns["dh"] == null) dataSchemaIsCorrect = false;
            if (baselineParser.Data.Columns["dht"] == null) dataSchemaIsCorrect = false;
            if (baselineParser.Data.Columns["d_cal_dose"] == null) dataSchemaIsCorrect = false;
            if (baselineParser.Data.Columns["sl"] == null) dataSchemaIsCorrect = false;
            if (baselineParser.Data.Columns["slt"] == null) dataSchemaIsCorrect = false;
            if (baselineParser.Data.Columns["sh"] == null) dataSchemaIsCorrect = false;
            if (baselineParser.Data.Columns["sht"] == null) dataSchemaIsCorrect = false;
            if (baselineParser.Data.Columns["s_cal_dose"] == null) dataSchemaIsCorrect = false;

            if (!dataSchemaIsCorrect)
            {
                InsertValidationErrorTable(false, "", "Baseline file data schema does not match the required format.", "BaselineFileDataSchemaValidation");
                return false;
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "BaselineFileDataSchemaValidation: " + ex.ToString(), "BaselineFileDataSchemaValidation");
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
                    string message = "Master file, S/N: " + masterRow["sn"].ToString().Trim() + " is missing data.";
                    InsertValidationErrorTable(false, masterRow["sn"].ToString().Trim(), message, "MasterRowDataValidation");
                    pass = false;
                }
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "MasterRowDataValidation: " + ex.ToString(), "MasterRowDataValidation");
            pass = false;
        }
        return pass;
    }

    //  -------------------- CHECK MASTER ROW HW VERSION ----------------------//
    private bool MasterRowHwVersionValidation()
    {
        bool pass = true;
        Hashtable hwHash = new Hashtable();

        try
        {
            // Loop through each of the rows            
            foreach (DataRow masterRow in masterParser.Data.Rows)
            {
                // Skip if it's a blank row
                if (isMasterRowBlank(masterRow)) continue;

                if (!hwHash.ContainsKey(masterRow["hw_version"].ToString().Trim()))
                {
                    hwHash.Add(masterRow["hw_version"].ToString().Trim(), "");
                }                
            }

            if (hwHash.Count > 1)
            {
                string message = "Hardware version is not consistent.";
                InsertValidationErrorTable(false, "", message, "MasterRowHwVersionValidation");
                pass = false;       
            }

        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "MasterRowHwVersionValidation: " + ex.ToString(), "MasterRowHwVersionValidation");
            pass = false;
        }
        return pass;
    }

    //  ---------- CHECK MASTER ROW Manufacturing Date if the date is 6 months old from the receipt date -----------------//
    private bool MasterRowManufacturingDateValidation()
    {
        bool pass = true;        

        try
        {
            DateTime receiptDate = (from a in IDC.POReceipts
                                     where a.ReceiptID == Convert.ToInt32(receiptID)
                                     select a.ReceiptDate).FirstOrDefault();

            // Loop through each of the rows            
            foreach (DataRow masterRow in masterParser.Data.Rows)
            {
                // Skip if it's a blank row
                if (isMasterRowBlank(masterRow)) continue;

                // Parse the manufacturing date
                string mfgDay = masterRow["mfg_date"].ToString().Trim().Substring(6, 2);
                string mfgMonth = masterRow["mfg_date"].ToString().Trim().Substring(4, 2);
                string mfgYear = masterRow["mfg_date"].ToString().Trim().Substring(0, 4);

                DateTime mfgDate;
                if (DateTime.TryParse(string.Format("{0}/{1}/{2}", mfgMonth, mfgDay, mfgYear), out mfgDate))
                {                    
                    if (mfgDate.AddMonths(6) < receiptDate)
                    {
                        string message = "Master file, S/N: " + masterRow["sn"].ToString().Trim() + " is out-of-date Manufacturing Date.";
                        InsertValidationErrorTable(true, masterRow["sn"].ToString().Trim(), message, "MasterRowManufacturingDateValidation");
                        pass = false;
                    }
                }
                else
                {
                    string message = "Master file, S/N: " + masterRow["sn"].ToString().Trim() + " is wrong Manufacturing Date format.";
                    InsertValidationErrorTable(true, masterRow["sn"].ToString().Trim(), message, "MasterRowManufacturingDateValidation");
                    pass = false;
                }
                    
            }            

        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "MasterRowManufacturingDateValidation: " + ex.ToString(), "MasterRowManufacturingDateValidation");
            pass = false;
        }

        //return pass;
        return true; // No raising warning message. Upload anyway.
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

                if (!snHash.ContainsKey(masterRow["sn"].ToString().Trim()))
                {
                    snHash.Add(masterRow["sn"].ToString().Trim(), "");
                }
                else
                {                    
                    string message = "Master file, S/N: " + masterRow["sn"].ToString().Trim() + " is duplicated.";
                    InsertValidationErrorTable(false, masterRow["sn"].ToString().Trim(), message, "MasterRowDuplicateValidation");
                    pass = false;                        
                }
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "MasterRowDuplicateValidation: " + ex.ToString(), "MasterRowDuplicateValidation");
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
                string serialNo = masterRow["sn"].ToString().Trim();

                // Do not upload onto MAS if the device's serialno is out of format.
                if (serialNo.Length > 10)
                {
                    string message = "Master file, S/N: " + serialNo + " is out of the format. Max 10 characters.";
                    InsertValidationErrorTable(false, serialNo, message, "MasterSerialNoFormatValidation");
                    pass = false;
                }
            }

        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "MasterSerialNoFormatValidation: " + ex.ToString(), "MasterSerialNoFormatValidation");
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
            // Get the device inventory records
            IQueryable<DeviceInventory> deviceInventory =
                (from di in IDC.DeviceInventories select di).AsQueryable();

            // Loop through each of the rows
            foreach (DataRow masterRow in masterParser.Data.Rows)
            {
                // Skip if it's a blank row
                if (isMasterRowBlank(masterRow)) continue;

                // Ensure the serial number was grabbed.
                string serialNo = masterRow["sn"].ToString().Trim();

                // Perform check
                int exists = (from di in deviceInventory where di.SerialNo == serialNo select di.DeviceID).Count();

                // Do not add DeviceInventory record if the device exists.
                if (exists > 0)
                {
                    string message = "Master file, S/N: " + masterRow["sn"].ToString().Trim() + " is already existed in Device Inventory.";
                    InsertValidationErrorTable(false, masterRow["sn"].ToString().Trim(), message, "MasterSerialNoExistValidation");
                    pass = false;
                }                
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "MasterSerialNoExistValidation: " + ex.ToString(), "MasterSerialNoExistValidation");
            pass = false;
        }
        return pass;
    }

    //  -------------------- CHECK BASELINE ROW S/N EXIST IN INVENTORY -------------------//
    private bool BaselineSerialNoExistValidation()
    {
        bool pass = true;

        try
        {
            // Get the device inventory records
            IQueryable<DeviceInventory> deviceInventory =
                (from di in IDC.DeviceInventories select di).AsQueryable();

            foreach (DataRow baselineRow in baselineParser.Data.Rows)
            {
                // Skip if it's a blank row
                if (isBaselineRowBlank(baselineRow)) continue;

                // Ensure the serial number was grabbed.
                string serialNo = baselineRow["sn"].ToString().Trim();

                // Perform check
                int exists = (from di in deviceInventory where di.SerialNo == serialNo select di.DeviceID).Count();

                // Do not add DeviceInventory record if the device exists.
                if (exists > 0)
                {
                    string message = "Baseline file, S/N: " + baselineRow["sn"].ToString().Trim() + " is already existed in Device Inventory.";
                    InsertValidationErrorTable(false, baselineRow["sn"].ToString().Trim(), message, "BaselineSerialNoExistValidation");
                    pass = false;
                }
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "BaselineSerialNoExistValidation: " + ex.ToString(), "BaselineSerialNoExistValidation");
            pass = false;
        }
        return pass;
    }

    // --------------------- CHECK BASELINE ROW DUPLICATE ---------------------//
    private bool BaselineRowDuplicateValidation()
    {
        bool pass = true;
        int count = 0;

        try
        {
            // Loop through each of the rows
            foreach (DataRow masterRow in masterParser.Data.Rows)
            {
                // Skip if it's a blank row
                if (isMasterRowBlank(masterRow)) continue;

                count = 0;
                
                foreach (DataRow baselineRow in baselineParser.Data.Rows)
                {
                    // Skip if it's a blank row
                    if (isBaselineRowBlank(baselineRow)) continue;

                    // If the record belonds to this record
                    if (masterRow["sn"].ToString().Trim() == baselineRow["sn"].ToString().Trim())
                    {
                        count ++;
                    }
                    // Get out of the baseline iteration when count > 5
                    if (count > 5) break;
                }

                // if count > 5 it means some baselines duplicated
                if (count > 5)
                {
                    string message = "Baseline file, S/N: " + masterRow["sn"].ToString().Trim() + " is duplicated.";
                    InsertValidationErrorTable(false, masterRow["sn"].ToString().Trim(), message, "BaselineRowDuplicateValidation");
                    pass = false;
                }
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "BaselineRowDuplicateValidation: " + ex.ToString(), "BaselineRowDuplicateValidation");
            pass = false;
        }
        return pass;
    }

    //  -------------------- BASELINE ROW MISSING DATA --------------------------//
    private bool BaselineRowDataValidation()
    {
        bool pass = true;
        try
        {
            // Loop through each of the rows
            foreach (DataRow baselineRow in baselineParser.Data.Rows)
            {
                // Skip if it's a blank row
                if (isBaselineRowBlank(baselineRow)) continue;

                if (isBaselineRowMissingData(baselineRow))
                {
                    string message = "Baseline file, S/N: " + baselineRow["sn"].ToString().Trim() + " is missing data.";
                    InsertValidationErrorTable(false, baselineRow["sn"].ToString().Trim(), message, "BaselineRowDataValidation");
                    pass = false;
                }
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "BaselineRowDataValidation: " + ex.ToString(), "BaselineRowDataValidation");
            pass = false;
        }
        return pass;
    }

    // --------------------- CHECK a S/N has bl1,bl2,bl3,bl4,bl5  ---------------------//
    private bool TotalBaselineBySnValidation()
    {
        bool pass = true;
        bool baseline1 = false;
        bool baseline2 = false;
        bool baseline3 = false;
        bool baseline4 = false;
        bool baseline5 = false;

        try
        {
            // Loop through each of the rows
            foreach (DataRow masterRow in masterParser.Data.Rows)
            {
                // Skip if it's a blank row
                if (isMasterRowBlank(masterRow)) continue;

                 baseline1 = false;
                 baseline2 = false;
                 baseline3 = false;
                 baseline4 = false;
                 baseline5 = false;

                foreach (DataRow baselineRow in baselineParser.Data.Rows)
                {
                    // Skip if it's a blank row
                    if (isBaselineRowBlank(baselineRow)) continue;

                    // If the record belonds to this record
                    if (masterRow["sn"].ToString().Trim() == baselineRow["sn"].ToString().Trim())
                    {
                        int tempID = 0;
                        int.TryParse(baselineRow["temp_id"].ToString().Trim(), out tempID);

                        if (tempID == 1) baseline1 = true;
                        if (tempID == 2) baseline2 = true;
                        if (tempID == 3) baseline3 = true;
                        if (tempID == 4) baseline4 = true;
                        if (tempID == 5) baseline5 = true;
                    }

                    // Get out of the iteration for this master file
                    if (baseline1 && baseline2 && baseline3 && baseline4 && baseline5) break;
                }

                // if the not all 5 records were found for a file, exist
                if (!baseline1 || !baseline2 || !baseline3 || !baseline4 || !baseline5)
                {
                    string message = "Baseline file, S/N: " + masterRow["sn"].ToString().Trim() + " is missing baseline.";
                    InsertValidationErrorTable(true, masterRow["sn"].ToString().Trim(), message, "TotalBaselineBySnValidation");
                    pass = false;
                }
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "TotalBaselineBySnValidation: " + ex.ToString(), "TotalBaselineBySnValidation");
            pass = false;
        }

        //return pass;
        return true; // No raising warning message. Upload anyway.
    }

    // --------------------- CHECK MASTER MATCH WITH BASELINE --------------------//
    private bool MasterMatchBaselineValidation()
    {
        bool pass = true;
        bool match = false;
        List<string> MMList = new List<string>();

        try
        {            
            // Loop through each of the rows
            foreach (DataRow baselineRow in baselineParser.Data.Rows)
            {
                match = false;

                // Skip if it's a blank row
                if (isBaselineRowBlank(baselineRow)) continue;

                foreach (DataRow masterRow in masterParser.Data.Rows)
                {
                    // Skip if it's a blank row
                    if (isMasterRowBlank(masterRow)) continue;

                    // If the record belongs to this record
                    if (masterRow["sn"].ToString().Trim() == baselineRow["sn"].ToString().Trim())
                    {
                        match = true;
                        break;
                    }
                }

                // if not found in the master
                if (!match)
                {   
                    // This makes sure we do not save the same 5 error rows for the same s/n
                    if (! MMList.Contains(baselineRow["sn"].ToString().Trim()))
                    {
                        string message = "Master file is missing a record for S/N: " + baselineRow["sn"].ToString().Trim();
                        InsertValidationErrorTable(true, baselineRow["sn"].ToString().Trim(), message, "MasterMatchBaselineValidation");
                        pass = false;

                        MMList.Add(baselineRow["sn"].ToString().Trim());  
                    }                                           
                }
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "MasterMatchBaselineValidation: " + ex.ToString(), "MasterMatchBaselineValidation");
            pass = false;
        }

        //return pass;
        return true; // No raising warning message. Upload anyway.
    }

    // ----- CHECK THE TOTAL'S BY COLOR WITH THE RECEIPT HEADER INFORMATION -------//
    private bool TotalColorValidation()
    {
        bool pass = true;
        try
        {
            int blkTotal = 0;
            int grnTotal = 0;
            int bluTotal = 0;
            int pnkTotal = 0;
            int slvTotal = 0;

            foreach (DataRow masterRow in masterParser.Data.Rows)
            {
                // Skip if it's a blank row
                if (isMasterRowBlank(masterRow)) continue;

                switch (masterRow["COLOR"].ToString().Trim().ToUpper())
                {
                    case "BLACK":
                        blkTotal++;
                        break;
                    case "SILVER":
                        slvTotal++;
                        break;
                    case "PINK":
                        pnkTotal++;
                        break;
                    case "GREEN":
                        grnTotal++;
                        break;
                    case "BLUE":
                        bluTotal++;
                        break;
                    default:
                        break;
                }
            }

            var myPOReceiptDetail = (from a in IDC.POReceiptDetails
                                     where a.ReceiptID == Convert.ToInt32(receiptID)
                                     select a).ToList();

            if (myPOReceiptDetail.Count() > 0)
            {
                string itemNumber = "";
                int qtyRecd;
                double lessQtyRecd, moreQtyRecd;

                foreach (var item in myPOReceiptDetail)
                {
                    itemNumber = item.ItemNumber.Trim();
                    qtyRecd = item.QtyRecd;
                    lessQtyRecd = qtyRecd * 0.9;
                    moreQtyRecd = qtyRecd * 1.1;

                    if (itemNumber.Contains("BLK"))
                    {
                        if (qtyRecd != blkTotal)
                        {
                            if (blkTotal > lessQtyRecd && blkTotal < moreQtyRecd)
                            {
                                string message = "Black color between Master file and Receipt Header is not in-synched.";
                                InsertValidationErrorTable(true, "", message, "TotalColorValidation");
                                // No raising warning message. Upload anyway.                                
                            }
                            else
                            {
                                string message = "The difference of Black color between Master file and Receipt Header is greater than 10%.";
                                InsertValidationErrorTable(false, "", message, "TotalColorValidation");
                                pass = false;
                            }                            
                        }
                    }
                    if (itemNumber.Contains("BLU"))
                    {
                        if (qtyRecd != bluTotal)
                        {
                            if (bluTotal > lessQtyRecd && bluTotal < moreQtyRecd)
                            {
                                string message = "Blue color between Master file and Receipt Header is not in-synched.";
                                InsertValidationErrorTable(true, "", message, "TotalColorValidation");                                
                                // No raising warning message. Upload anyway.                                
                            }
                            else
                            {
                                string message = "The difference of Blue color between Master file and Receipt Header is greater than 10%.";
                                InsertValidationErrorTable(false, "", message, "TotalColorValidation");
                                pass = false;
                            }                             
                        }
                    }
                    if (itemNumber.Contains("GRN"))
                    {
                        if (qtyRecd != grnTotal)
                        {
                            if (grnTotal > lessQtyRecd && grnTotal < moreQtyRecd)
                            {
                                string message = "Green color between Master file and Receipt Header is not in-synched.";
                                InsertValidationErrorTable(true, "", message, "TotalColorValidation");                                
                                // No raising warning message. Upload anyway.                                
                            }
                            else
                            {
                                string message = "The difference of Green color between Master file and Receipt Header is greater than 10%.";
                                InsertValidationErrorTable(false, "", message, "TotalColorValidation");
                                pass = false;
                            }                               
                        }
                    }
                    if (itemNumber.Contains("PNK"))
                    {
                        if (qtyRecd != pnkTotal)
                        {
                            if (pnkTotal > lessQtyRecd && pnkTotal < moreQtyRecd)
                            {
                                string message = "Pink color between Master file and Receipt Header is not in-synched.";
                                InsertValidationErrorTable(true, "", message, "TotalColorValidation");                                
                                // No raising warning message. Upload anyway.                                
                            }
                            else
                            {
                                string message = "The difference of Pink color between Master file and Receipt Header is greater than 10%.";
                                InsertValidationErrorTable(false, "", message, "TotalColorValidation");
                                pass = false;
                            }                                
                        }
                    }
                    if (itemNumber.Contains("SLV"))
                    {
                        if (qtyRecd != slvTotal)
                        {
                            if (slvTotal > lessQtyRecd && slvTotal < moreQtyRecd)
                            {
                                string message = "Silver color between Master file and Receipt Header is not in-synched.";
                                InsertValidationErrorTable(true, "", message, "TotalColorValidation");                               
                                // No raising warning message. Upload anyway.                                
                            }
                            else
                            {
                                string message = "The difference of Silver color between Master file and Receipt Header is greater than 10%.";
                                InsertValidationErrorTable(false, "", message, "TotalColorValidation");
                                pass = false;
                            }                                  
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "TotalColorValidation: " + ex.ToString(), "TotalColorValidation");
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

            int totalBackOrderQty = GetTotalBackOrderQuantityByPONumber(myPOReceipt.PONumber);

            // if upload greater than total of BackOrderQty by 10% then will not upload the file until the files are fixed.
            if (uploadTotal > totalBackOrderQty * 1.1)
            {
                string message = "The total of uploaded is greater than the BackOrder Quantity by 10%.";
                InsertValidationErrorTable(false, "", message, "TotalBackOrderValidation");
                pass = false;                          
            }
            else if (uploadTotal > totalBackOrderQty)
            {
                string message = "The total of uploaded is greater than the BackOrder Quantity but less than 10%.";
                InsertValidationErrorTable(true, "", message, "TotalBackOrderValidation");
                // No raising warning message. Upload anyway.      
            }                                  
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "TotalBackOrderValidation: " + ex.ToString(), "TotalBackOrderValidation");
            pass = false;
        }

        return pass;
    }

    // ---------------- CHECK baseline temp between 1-2, 2-3, 3-4 to ensure at least 1000 difference----------------//
    private bool BaselineTemperatureValidation()
    {
        bool pass = true;
        try
        {
            double dlt1, dlt2, dlt3, dlt4, dht1, dht2, dht3, dht4;            

            // Loop through each of the rows
            foreach (DataRow masterRow in masterParser.Data.Rows)
            {
                // Skip if it's a blank row
                if (isMasterRowBlank(masterRow)) continue;

                dlt1 = 0.0;
                dlt2 = 0.0;
                dlt3 = 0.0;
                dlt4 = 0.0;
                dht1 = 0.0;
                dht2 = 0.0;
                dht3 = 0.0;
                dht4 = 0.0;               

                foreach (DataRow baselineRow in baselineParser.Data.Rows)
                {
                    // Skip if it's a blank row
                    if (isBaselineRowBlank(baselineRow)) continue;

                    // If the record belonds to this record
                    if (masterRow["sn"].ToString().Trim() == baselineRow["sn"].ToString().Trim())
                    {
                        int tempID = 0;
                        int.TryParse(baselineRow["temp_id"].ToString().Trim(), out tempID);

                        if (tempID == 1)
                        {
                            dlt1 = double.Parse(baselineRow["dlt"].ToString());
                            dht1 = double.Parse(baselineRow["dht"].ToString());
                        }

                        if (tempID == 2)
                        {
                            dlt2 = double.Parse(baselineRow["dlt"].ToString());
                            dht2 = double.Parse(baselineRow["dht"].ToString());
                        }

                        if (tempID == 3)
                        {
                            dlt3 = double.Parse(baselineRow["dlt"].ToString());
                            dht3 = double.Parse(baselineRow["dht"].ToString());
                        }

                        if (tempID == 4)
                        {
                            dlt4 = double.Parse(baselineRow["dlt"].ToString());
                            dht4 = double.Parse(baselineRow["dht"].ToString());
                        }
                        
                    }

                    // Get out of the baseline iteration when it reads all dlt1,dlt2,dlt3,dlt4,dht1,dht2,dht3,dht4
                    if ((dlt1 > 0) && (dlt2 > 0) && (dlt3 > 0) && (dlt4 > 0) && (dht1 > 0) && (dht2 > 0) && (dht3 > 0) && (dht4 > 0)) break;
                }

                // if baseline temperature check shows it fails then adding to error message.
                if (((dlt1 - dlt2) < BLTempDiff) || ((dlt2 - dlt3) < BLTempDiff) || ((dlt3 - dlt4) < BLTempDiff) 
                    || ((dht1 - dht2) < BLTempDiff) || ((dht2 - dht3) < BLTempDiff) || ((dht3 - dht4) < BLTempDiff) )
                {
                    string message = "Baseline file, S/N: " + masterRow["sn"].ToString().Trim() + " is BLT malfunction.";
                    InsertValidationErrorTable(true, masterRow["sn"].ToString().Trim(), message, "BaselineTemperatureValidation");
                    pass = false;
                }
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "BaselineTemperatureValidation: " + ex.ToString(), "BaselineTemperatureValidation");
            pass = false;
        }

        //return pass;
        return true; // No raising warning message. Upload anyway.
    }

    // ---------------- CHECK baseline2 is out of range ----------------//
    private bool Baseline2Validation()
    {
        bool pass = true;
        try
        {
            foreach (DataRow baselineRow in baselineParser.Data.Rows)
            {
                // Skip if it's a blank row
                if (isBaselineRowBlank(baselineRow)) continue;

                baselineRow["sn"].ToString().Trim();

                int tempID = 0;
                int.TryParse(baselineRow["temp_id"].ToString().Trim(), out tempID);

                // Check if baseline2 values are out of range
                if (tempID == 2)
                {
                    if (!isBaseline2Normal(baselineRow["dl"].ToString(), baselineRow["dh"].ToString(), baselineRow["dlt"].ToString(), baselineRow["dht"].ToString()))
                    {
                        string message = "Baseline file, S/N: " + baselineRow["sn"].ToString().Trim() + " is out of range BL2.";
                        InsertValidationErrorTable(true, baselineRow["sn"].ToString().Trim(), message, "Baseline2Validation");
                        pass = false;
                    }                    
                }                
            }
            
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "Baseline2Validation: " + ex.ToString(), "Baseline2Validation");
            pass = false;
        }

        //return pass;
        return true; // No raising warning message. Upload anyway.
    }



    public bool MasterBaselineUploadValidation()
    {
        InitiateValidationErrorTable();
        
        try
        {
            if (!IsMASAvailableForPOUpload())
            {               
                return false;
            }

            if (!MasterFileParsingValidation())
            {               
                return false;
            }

            if (!BaselineFileParsingValidation())
            {                
                return false;
            }

            if (!MasterFileDataSchemaValidation())
            {                
                return false;
            }

            if (!BaselineFileDataSchemaValidation())
            {                
                return false;
            }

            if (!MasterRowDataValidation())
            {                
                return false;
            }

            if (!BaselineRowDataValidation())
            {
                return false;
            }

            if (!MasterRowHwVersionValidation())
            {               
                return false;
            }

            if (!MasterRowDuplicateValidation())
            {                
                return false;
            }

            if (!BaselineRowDuplicateValidation())
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

            if (!BaselineSerialNoExistValidation())
            {                
                return false;
            }

            if (!TotalBackOrderValidation())
            {
                return false;
            }   
           
            if (!TotalColorValidation())
            {
                return false;
            }   


            // Any validation functions below this line always return TRUE because we like to upload it anyway eventhough it fails. 
            // However, the failed validation info of each s/n will be used to update the device analysis status and establish a summary report.
            // --------------------------------------------------------------------------------------------//
           
            if (!MasterRowManufacturingDateValidation())
            {                
                return false;
            }
                        
            if (!MasterMatchBaselineValidation())
            {                
                return false;
            }

            if (!TotalBaselineBySnValidation())
            {
                return false;
            }

            if (!BaselineTemperatureValidation())
            {                
                return false;
            }
            
            if (!Baseline2Validation())
            {                
                return false;
            }
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
        string Message = "";

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

            // Create a new firmware version, that will be replaced once set.
            DeviceFirmware deviceFirmware = new DeviceFirmware();

            // Loop through each row of Master file
            foreach (DataRow row in masterParser.Data.Rows)
            {
                // Skip if the serial no is blank
                if (isMasterRowBlank(row)) continue;

                // Ensure the serial number was grabbed.
                string serialNo = row["sn"].ToString().Trim();

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
                string mfgDay = row["mfg_date"].ToString().Trim().Substring(6, 2);
                string mfgMonth = row["mfg_date"].ToString().Trim().Substring(4, 2);
                string mfgYear = row["mfg_date"].ToString().Trim().Substring(0, 4);

                // Parse the firmware version
                string fwMajor = row["hw_version"].ToString().Trim().Substring(0, 1);
                string fwMinor = row["hw_version"].ToString().Trim().Substring(1, 1);

                // Convert the firmware version to a double.
                string firmwareVersion = "1";
                //double.TryParse(string.Format("{0}.{1}", fwMajor, fwMinor), out firmwareVersion);

                // If the firmware does not match the existing record, requery.
                if (deviceFirmware.FirmwareVersion != firmwareVersion)
                {
                    deviceFirmware = (from df in IDC.DeviceFirmwares where df.FirmwareVersion == firmwareVersion select df).FirstOrDefault();
                }

                // Grab lot/box number.
                string boxNumber = row["LOT/BOX"].ToString().Trim();

                // Create a new device record
                DeviceInventory device = new DeviceInventory()
                {
                    SerialNo = serialNo,
                    DeviceFirmware = deviceFirmware,
                    HardwareVersion = NEWDEVICE_HARDWAREVERSION,
                    DeviceAnalysisStatus = defaultStatus,
                    CurrentGroup = defaultGroup,
                    InitialGroup = defaultGroup,
                    CalibrationMajor = 0,
                    CalibrationMinor = 0,
                    FailedCalibration = false,
                    ReceiptID = int.Parse(this.receiptID),
                    POBoxNumber = boxNumber
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
                deviceAudit.Step = "Master Baseline Upload";
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

                    // Create a new device record
                    DeviceInventory device = new DeviceInventory()
                    {
                        SerialNo = sn,
                        HardwareVersion = NEWDEVICE_HARDWAREVERSION,
                        DeviceAnalysisStatus = defaultStatus,
                        CurrentGroup = defaultGroup,
                        InitialGroup = defaultGroup,
                        CalibrationMajor = 0,
                        CalibrationMinor = 0,
                        FailedCalibration = false,
                        ReceiptID = int.Parse(this.receiptID)
                    };

                    // Set InventorySubStatus = "Missing Master"
                    noMM++;
                    device.DeviceAnalysisStatus = (from ds in IDC.DeviceAnalysisStatus where ds.DeviceAnalysisName == DeviceAnalysisName_MM select ds).FirstOrDefault();

                    // Insert device audit info, device currently in QA
                    DeviceInventoryAudit deviceAudit = new DeviceInventoryAudit();
                    deviceAudit.DeviceAnalysisStatus = device.DeviceAnalysisStatus;
                    deviceAudit.IsFail = false;
                    deviceAudit.Step = "Master Baseline Upload";
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


            // ---------------------------------- VERIFY & UPLOAD BASELINE --------------------------------------//
            int baselineSkipped = 0;
            int baselineAdded = 0;

            // Loop through each row of baselines
            foreach (DataRow row in baselineParser.Data.Rows)
            {
                // Skip if the serial no is blank
                if (isBaselineRowBlank(row)) continue;

                // Ensure the serial number was grabbed.
                string serialNo = row["sn"].ToString().Trim();

                int tempId = 0;
                if (!int.TryParse(row["temp_id"].ToString().Trim(), out tempId)) continue;

                // Find the device record in master
                DeviceInventory device = (from di in devicesToUpload where di.SerialNo == serialNo select di).FirstOrDefault();

                if (device == null) // Already exist.
                {
                    baselineSkipped++;
                }
                else
                {                    
                    baselineAdded++;

                    DeviceBaseline baseline = new DeviceBaseline();

                    baseline.CreatedDate = createdDate;
                    baseline.BaselineReadCount = tempId;

                    baseline.DeepHigh = (int)double.Parse(row["dh"].ToString());
                    baseline.DeepHighTemp = (int)double.Parse(row["dht"].ToString());
                    baseline.DeepLow = (int)double.Parse(row["dl"].ToString());
                    baseline.DeepLowTemp = (int)double.Parse(row["dlt"].ToString());
                    baseline.DCDose = (int)double.Parse(row["d_cal_dose"].ToString());

                    baseline.ShallowHigh = (int)double.Parse(row["sh"].ToString());
                    baseline.ShallowHighTemp = (int)double.Parse(row["sht"].ToString());
                    baseline.ShallowLow = (int)double.Parse(row["sl"].ToString());
                    baseline.ShallowLowTemp = (int)double.Parse(row["slt"].ToString());
                    baseline.SCDose = (int)double.Parse(row["s_cal_dose"].ToString());

                    // Add baseline record
                    device.DeviceBaselines.Add(baseline);

                    
                }
            }


            Message = string.Format("Records to Upload: {0} Skipped Records: {1}", baselineAdded, baselineSkipped);


            // UPLOAD ALL DEVICES AND BASELINES DATA
            IDC.DeviceInventories.InsertAllOnSubmit(devicesToUpload);
            IDC.SubmitChanges();


            Message = string.Format("Completed.");

        }
        catch (Exception ex)
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
        int retCount = 0;
        try
        {
            //masterParser.Parse(this.masterFileContent);            
            
            int blkTotal = 0;
            int grnTotal = 0;
            int bluTotal = 0;
            int pnkTotal = 0;
            int slvTotal = 0;

            foreach (DataRow masterRow in masterParser.Data.Rows)
            {
                // Skip if it's a blank row
                if (isMasterRowBlank(masterRow)) continue;

                switch (masterRow["COLOR"].ToString().Trim().ToUpper())
                {
                    case "BLACK":
                        blkTotal++;
                        break;
                    case "SILVER":
                        slvTotal++;
                        break;
                    case "PINK":
                        pnkTotal++;
                        break;
                    case "GREEN":
                        grnTotal++;
                        break;
                    case "BLUE":
                        bluTotal++;
                        break;
                    default:
                        break;
                }
            }

            if (pItemNumber.Contains("BLK"))
            {
                retCount = blkTotal;
            }
            if (pItemNumber.Contains("BLU"))
            {
                retCount = bluTotal;
            }
            if (pItemNumber.Contains("GRN"))
            {
                retCount = grnTotal;
            }
            if (pItemNumber.Contains("PNK"))
            {
                retCount = pnkTotal;
            }
            if (pItemNumber.Contains("SLV"))
            {
                retCount = slvTotal;
            }

            return retCount;
        }
        catch
        {
            return retCount;
        }
    }

    //public void Execute(int pReceiptID, Hashtable  pBadBL2List, ref string resultMessages)
    //{
    //    string Message = "";
        
    //    try
    //    {            
    //        masterParser.Parse(this.masterPath);
    //        baselineParser.Parse(this.baselinePath);
    //        // ------------------------------------ VERIFY MASTER & INSERT DEVICEI NVENTORY --------------------------------------//

    //        // Get the device inventory records
    //        IQueryable<DeviceInventory> deviceInventory =
    //            (from di in IDC.DeviceInventories select di).AsQueryable();

    //        int skippingBadDevices = 0;
    //        int skippingDuplicateDevices = 0;
    //        int newGoodDevices = 0;
    //        int newBadDevices = 0;

    //        // Set a createdDate
    //        DateTime createdDate = DateTime.Now;

    //        // Store the new devices
    //        List<DeviceInventory> devicesToUpload = new List<DeviceInventory>();

    //        // Get the default group
    //        DeviceGroup defaultGroup = (from dg in IDC.DeviceGroups where dg.DeviceGroupName == NEWDEVICE_GROUP select dg).FirstOrDefault();

    //        // Get the default status
    //        DeviceAnalysisStatus defaultStatus = (from ds in IDC.DeviceAnalysisStatus where ds.DeviceAnalysisName == NEWDEVICE_STATUS select ds).FirstOrDefault();

    //        // Create a new firmware version, that will be replaced once set.
    //        DeviceFirmware deviceFirmware = new DeviceFirmware();

    //        // Loop through each of the rows
    //        foreach (DataRow row in masterParser.Data.Rows)
    //        {
    //            // Skip if the serial no is blank
    //            if (isMasterRowBlank(row)) continue;

    //            // Ensure the serial number was grabbed.
    //            string serialNo = row["sn"].ToString().Trim();

    //            // if BL2 is bad and we do not like to upload it. Skipping it.
    //            if (pBadBL2List.ContainsKey(serialNo) && (bool)pBadBL2List[serialNo] == false)
    //            {
    //                skippingBadDevices++;
    //            }
    //            else
    //            {
    //                // Perform check
    //                int exists = (from di in deviceInventory where di.SerialNo == serialNo select di.DeviceID).Count();

    //                // Do not add DeviceInventory record if the device exists. Skipping it.
    //                if (exists > 0)
    //                    skippingDuplicateDevices++;
    //                else // Create the new record if it does not exist
    //                {
    //                    // if BL2 is bad but we still like to upload it. 
    //                    if (pBadBL2List.ContainsKey(serialNo) && (bool)pBadBL2List[serialNo] == true)
    //                        newBadDevices++;
    //                    else
    //                        newGoodDevices++;

    //                    // Parse the manufacturing date
    //                    string mfgDay = row["mfg_date"].ToString().Trim().Substring(6, 2);
    //                    string mfgMonth = row["mfg_date"].ToString().Trim().Substring(4, 2);
    //                    string mfgYear = row["mfg_date"].ToString().Trim().Substring(0, 4);

    //                    // Parse the firmware version
    //                    string fwMajor = row["hw_version"].ToString().Trim().Substring(0, 1);
    //                    string fwMinor = row["hw_version"].ToString().Trim().Substring(1, 1);

    //                    // Convert the firmware version to a double.
    //                    double firmwareVersion = 1;
    //                    double.TryParse(string.Format("{0}.{1}", fwMajor, fwMinor), out firmwareVersion);

    //                    // If the firmware does not match the existing record, requery.
    //                    if (deviceFirmware.FirmwareVersion != firmwareVersion)
    //                    {
    //                        deviceFirmware = (from df in IDC.DeviceFirmwares where df.FirmwareVersion == firmwareVersion select df).FirstOrDefault();
    //                    }

    //                    // Grab lot/box number.
    //                    string boxNumber = row["LOT/BOX"].ToString().Trim();

    //                    // Create a new device record
    //                    DeviceInventory device = new DeviceInventory()
    //                    {
    //                        SerialNo = serialNo,
    //                        DeviceFirmware = deviceFirmware,
    //                        DeviceAnalysisStatus = defaultStatus,
    //                        CurrentGroup = defaultGroup,
    //                        InitialGroup = defaultGroup,
    //                        CalibrationMajor = 0,
    //                        CalibrationMinor = 0,
    //                        FailedCalibration = false,
    //                        ReceiptID = pReceiptID,
    //                        POBoxNumber = boxNumber
    //                    };

    //                    // Set the manufacturing date.
    //                    DateTime mfgDate;
    //                    if (DateTime.TryParse(string.Format("{0}/{1}/{2}", mfgMonth, mfgDay, mfgYear), out mfgDate))
    //                        device.MfgDate = mfgDate;

    //                    // Add to the list
    //                    devicesToUpload.Add(device);
    //                }
    //            }
    //        }

    //        int totalDevice = newGoodDevices + newBadDevices + skippingBadDevices + skippingDuplicateDevices;

    //        resultMessages += totalDevice + "_";
    //        resultMessages += newGoodDevices + "_";
    //        resultMessages += newBadDevices + "_";
    //        resultMessages += skippingBadDevices + "_";
    //        resultMessages += skippingDuplicateDevices + "_";

    //        // ---------------------------------- VERIFY BASELINE --------------------------------------//
    //        int baselineSkipped = 0;
    //        int baselineAdded = 0;

    //        // Loop through each of the rows
    //        foreach (DataRow row in baselineParser.Data.Rows)
    //        {
    //            // Skip if the serial no is blank
    //            if (isBaselineRowBlank(row)) continue;

    //            // Ensure the serial number was grabbed.
    //            string serialNo = row["sn"].ToString().Trim();

    //            int tempId = 0;
    //            if (!int.TryParse(row["temp_id"].ToString().Trim(), out tempId)) continue;                                

    //            // Find the device record in master
    //            DeviceInventory device = (from di in devicesToUpload where di.SerialNo == serialNo select di).FirstOrDefault();

    //            if (device == null) // SerialNo in baseline file not found in master file. This check is also done at validation already.
    //            {
    //                baselineSkipped++;
    //            }
    //            else
    //            {
    //                // if BL2 is bad and we do not like to upload it
    //                if (pBadBL2List.ContainsKey(serialNo) && (bool)pBadBL2List[serialNo] == false)
    //                {
    //                }
    //                else // upload it does not matter if BL2 is bad or good
    //                {
    //                    // Check if baseline2 values are out of range, then update DeviceInvStatus for the device. (Inventory Automation)
    //                    if (tempId == 2)
    //                    {
    //                        // if BL2 is bad and we like to upload it too
    //                        if (pBadBL2List.ContainsKey(serialNo) && (bool)pBadBL2List[serialNo] == true)
    //                        {
    //                            // Set InventorySubStatus = "Remove from Testing"
    //                            device.DeviceInventorySubStatus = (from ds in IDC.DeviceInventorySubStatus where ds.DeviceInvSubStatusName == "Remove from Testing" select ds).FirstOrDefault();
    //                        }
    //                        else // if BL2 is good
    //                        {
    //                            // Set InventorySubStatus = "Ready for Testing"
    //                            device.DeviceInventorySubStatus = (from ds in IDC.DeviceInventorySubStatus where ds.DeviceInvSubStatusName == "Ready for Testing" select ds).FirstOrDefault();
    //                        }

    //                        //if (!isBaseline2Normal(row["dl"].ToString(), row["dh"].ToString(), row["dlt"].ToString(), row["dht"].ToString()))
    //                        //{
    //                        //    // Set InventorySubStatus = "Remove from Testing"
    //                        //    device.DeviceInventorySubStatus = (from ds in IDC.DeviceInventorySubStatus where ds.DeviceInvSubStatusName == "Remove from Testing" select ds).FirstOrDefault();
    //                        //}
    //                        //else
    //                        //{
    //                        //    // Set InventorySubStatus = "Ready for Testing"
    //                        //    device.DeviceInventorySubStatus = (from ds in IDC.DeviceInventorySubStatus where ds.DeviceInvSubStatusName == "Ready for Testing" select ds).FirstOrDefault();
    //                        //}
    //                    }
                        

    //                    baselineAdded++;

    //                    DeviceBaseline baseline = new DeviceBaseline();

    //                    baseline.CreatedDate = createdDate;
    //                    baseline.BaselineReadCount = tempId;

    //                    baseline.DeepHigh = (int)double.Parse(row["dh"].ToString());
    //                    baseline.DeepHighTemp = (int)double.Parse(row["dht"].ToString());
    //                    baseline.DeepLow = (int)double.Parse(row["dl"].ToString());
    //                    baseline.DeepLowTemp = (int)double.Parse(row["dlt"].ToString());
    //                    baseline.DCDose = (int)double.Parse(row["d_cal_dose"].ToString());

    //                    baseline.ShallowHigh = (int)double.Parse(row["sh"].ToString());
    //                    baseline.ShallowHighTemp = (int)double.Parse(row["sht"].ToString());
    //                    baseline.ShallowLow = (int)double.Parse(row["sl"].ToString());
    //                    baseline.ShallowLowTemp = (int)double.Parse(row["slt"].ToString());
    //                    baseline.SCDose = (int)double.Parse(row["s_cal_dose"].ToString());

    //                    // Add baseline record
    //                    device.DeviceBaselines.Add(baseline);

    //                }
                                       
    //            }
    //        }


    //        Message = string.Format("Records to Upload: {0} Skipped Records: {1}", baselineAdded, baselineSkipped);


    //        // UPLOAD ALL DEVICES AND BASELINES DATA
    //        IDC.DeviceInventories.InsertAllOnSubmit(devicesToUpload);
    //        IDC.SubmitChanges();


    //        Message = string.Format("Completed.");

    //    }
    //    catch (Exception ex)
    //    {
    //        Message = ex.ToString();
    //    }
    //}

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
            if (string.IsNullOrEmpty(pMasterRow["sn"].ToString()) && string.IsNullOrEmpty(pMasterRow["mfg_date"].ToString())
                        && string.IsNullOrEmpty(pMasterRow["hw_version"].ToString()) && string.IsNullOrEmpty(pMasterRow["COLOR"].ToString())
                        && string.IsNullOrEmpty(pMasterRow["LOT/BOX"].ToString()) && string.IsNullOrEmpty(pMasterRow["PURCHASE ORDER"].ToString())
                        && string.IsNullOrEmpty(pMasterRow["INVOICE"].ToString()) && string.IsNullOrEmpty(pMasterRow["NOTE"].ToString())
                        && string.IsNullOrEmpty(pMasterRow["InvoiceDate"].ToString()))
            {
                return true;
            }
        }
        catch
        {}
        return false;        
    }

    private bool isBaselineRowBlank(DataRow pBaselineRow)
    {
        try
        {
            if (string.IsNullOrEmpty(pBaselineRow["sn"].ToString()) && string.IsNullOrEmpty(pBaselineRow["temp_id"].ToString())
                   && string.IsNullOrEmpty(pBaselineRow["dl"].ToString()) && string.IsNullOrEmpty(pBaselineRow["dlt"].ToString())
                   && string.IsNullOrEmpty(pBaselineRow["dh"].ToString()) && string.IsNullOrEmpty(pBaselineRow["dht"].ToString())
                   && string.IsNullOrEmpty(pBaselineRow["d_cal_dose"].ToString()) && string.IsNullOrEmpty(pBaselineRow["sl"].ToString())
                   && string.IsNullOrEmpty(pBaselineRow["slt"].ToString()) && string.IsNullOrEmpty(pBaselineRow["sh"].ToString())
                   && string.IsNullOrEmpty(pBaselineRow["sht"].ToString()) && string.IsNullOrEmpty(pBaselineRow["s_cal_dose"].ToString()))
            {
                return true;
            }
        }
        catch
        { }
        return false;
    }

    private bool isMasterRowMissingData(DataRow pMasterRow)
    {
        try
        {
            if (string.IsNullOrEmpty(pMasterRow["sn"].ToString()) || string.IsNullOrEmpty(pMasterRow["mfg_date"].ToString())
                    || string.IsNullOrEmpty(pMasterRow["hw_version"].ToString()) || string.IsNullOrEmpty(pMasterRow["COLOR"].ToString())
                    || string.IsNullOrEmpty(pMasterRow["LOT/BOX"].ToString()) || string.IsNullOrEmpty(pMasterRow["PURCHASE ORDER"].ToString())
                    || string.IsNullOrEmpty(pMasterRow["INVOICE"].ToString()) || string.IsNullOrEmpty(pMasterRow["NOTE"].ToString())
                    || string.IsNullOrEmpty(pMasterRow["InvoiceDate"].ToString()))
            {
                return true;
            }
        }
        catch
        { }
        return false;
    }

    private bool isBaselineRowMissingData(DataRow pBaselineRow)
    {
        try
        {
            if (string.IsNullOrEmpty(pBaselineRow["sn"].ToString()) || string.IsNullOrEmpty(pBaselineRow["temp_id"].ToString())
                    || string.IsNullOrEmpty(pBaselineRow["dl"].ToString()) || string.IsNullOrEmpty(pBaselineRow["dlt"].ToString())
                    || string.IsNullOrEmpty(pBaselineRow["dh"].ToString()) || string.IsNullOrEmpty(pBaselineRow["dht"].ToString())
                    || string.IsNullOrEmpty(pBaselineRow["d_cal_dose"].ToString()) || string.IsNullOrEmpty(pBaselineRow["sl"].ToString())
                    || string.IsNullOrEmpty(pBaselineRow["slt"].ToString()) || string.IsNullOrEmpty(pBaselineRow["sh"].ToString())
                    || string.IsNullOrEmpty(pBaselineRow["sht"].ToString()) || string.IsNullOrEmpty(pBaselineRow["s_cal_dose"].ToString()))
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

}