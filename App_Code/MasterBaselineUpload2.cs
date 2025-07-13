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
/// Summary description for MasterBaselineUpload
/// </summary>
public class MasterBaselineUpload2
{
    private const string NEWDEVICE_GROUP = "PRECALAPP";
    private const string NEWDEVICE_STATUS = "Unknown";
    private const int NEWDEVICE_PRODUCT = 22;
    private const string NEWDEVICE_SKU = "INSTA20";
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
    private const string DeviceAnalysisName_MTC = "Missing Temp Comp";
    private const string DeviceAnalysisName_MSC = "Missing Sens Coef";    
   
    private string receiptID;
    private InsDataContext IDC = new InsDataContext();
    private CSVParser masterParser = new CSVParser();
    private CSVParser tempCompParser = new CSVParser();
    private CSVParser sensCoefParser = new CSVParser();
    private CSVParser readerParser = new CSVParser();
    private string Message = "";
    private DataTable dtValidationError = new DataTable();
    private int totalActualUploaded = 0;
    private int totalActualDeviceUploaded = 0;
    private int totalActualReaderUploaded = 0;
    public bool uploadReader = false;
    public bool uploadDevice = false;

    public MasterBaselineUpload2(StreamReader pMasterFileContent, StreamReader pTempCompFileContent, StreamReader pSensCoefFileContent, StreamReader pReaderFileContent, bool pUploadDevice, bool pUploadReader, string pReceiptID)
    {        
        this.receiptID = pReceiptID;
        uploadDevice = pUploadDevice;
        uploadReader = pUploadReader;

        if (uploadDevice)
        {
            masterParser.Parse(pMasterFileContent);
            tempCompParser.Parse(pTempCompFileContent);
            sensCoefParser.Parse(pSensCoefFileContent);
        }

        if (uploadReader)
        {
            readerParser.Parse(pReaderFileContent);
        }
        
    }

    public int TotalDeviceUploaded()
    {
        return totalActualDeviceUploaded;
    }

    public int TotalReaderUploaded()
    {
        return totalActualReaderUploaded;
    }

    public int TotalUploaded()
    {
        return totalActualUploaded;
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
            if (masterParser.Data == null)
            {
                InsertValidationErrorTable(false, "", "Could not parse Master file.", "MasterFileParsingValidation");
                return false;
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "MasterFileParsingValidation: " + ex.ToString(), "MasterFileParsingValidation");
            return false;
        }
        return true;
    }

    // --------------------- PARSE Temperature Compensation ---------------------------------//
    private bool TempCompensationFileParsingValidation()
    {
        try
        {
            if (tempCompParser.Data == null)
            {
                InsertValidationErrorTable(false, "", "Could not parse Temperature Compensation file.", "TempCompensationFileParsingValidation");
                return false;
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "TempCompensationFileParsingValidation: " + ex.ToString(), "TempCompensationFileParsingValidation");
            return false;
        }
        return true;
    }

    // --------------------- PARSE Sensitivity Coefficient ---------------------------------//
    private bool SensitivityCoefficientFileParsingValidation()
    {
        try
        {
            if (sensCoefParser.Data == null)
            {
                InsertValidationErrorTable(false, "", "Could not parse Sensitivity Coefficient file.", "SensitivityCoefficientFileParsingValidation");
                return false;
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "SensitivityCoefficientFileParsingValidation: " + ex.ToString(), "SensitivityCoefficientFileParsingValidation");
            return false;
        }
        return true;
    }

    //  -------------------- PARSE READER -------------------------------//
    private bool ReaderFileParsingValidation()
    {
        try
        {
            if (readerParser.Data == null)
            {
                InsertValidationErrorTable(false, "", "Could not parse Instalink file.", "ReaderFileParsingValidation");
                return false;
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "ReaderFileParsingValidation: " + ex.ToString(), "ReaderFileParsingValidation");
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
            if (masterParser.Data.Columns["LOT/BOX"] == null) dataSchemaIsCorrect = false;
            if (masterParser.Data.Columns["PURCHASE ORDER"] == null) dataSchemaIsCorrect = false;
            if (masterParser.Data.Columns["INVOICE"] == null) dataSchemaIsCorrect = false;
            if (masterParser.Data.Columns["NOTE"] == null) dataSchemaIsCorrect = false;
            if (masterParser.Data.Columns["InvoiceDate"] == null) dataSchemaIsCorrect = false;

            if (masterParser.Data.Columns["ReadVolt0"] == null) dataSchemaIsCorrect = false;
            if (masterParser.Data.Columns["ReadVolt1"] == null) dataSchemaIsCorrect = false;
            if (masterParser.Data.Columns["ReadVolt2"] == null) dataSchemaIsCorrect = false;
            if (masterParser.Data.Columns["ReadVolt3"] == null) dataSchemaIsCorrect = false;
            if (masterParser.Data.Columns["ReadVolt4"] == null) dataSchemaIsCorrect = false;
            if (masterParser.Data.Columns["ReadVolt5"] == null) dataSchemaIsCorrect = false;
            if (masterParser.Data.Columns["ReadVolt6"] == null) dataSchemaIsCorrect = false;
            if (masterParser.Data.Columns["ReadVolt7"] == null) dataSchemaIsCorrect = false;

            // the following fields are optional. They are for manufacturing testing.
            if (masterParser.Data.Columns["BDAddress"] == null) dataSchemaIsCorrect = false;
            if (masterParser.Data.Columns["PanelID"] == null) dataSchemaIsCorrect = false;
            if (masterParser.Data.Columns["Position"] == null) dataSchemaIsCorrect = false;

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

    //  -------------------- Temperature Compensation DATA SCHEMA -----------------------------//
    private bool TempCompensationFileDataSchemaValidation()
    {
        bool dataSchemaIsCorrect = true;
        try
        {
            // Check each column exists in the data table that was returned.
            if (tempCompParser.Data.Columns["sn"] == null) dataSchemaIsCorrect = false;
            if (tempCompParser.Data.Columns["Temp_id"] == null) dataSchemaIsCorrect = false;
            if (tempCompParser.Data.Columns["DL_R'"] == null) dataSchemaIsCorrect = false;
            if (tempCompParser.Data.Columns["DL_Tref"] == null) dataSchemaIsCorrect = false;
            if (tempCompParser.Data.Columns["DL_K1"] == null) dataSchemaIsCorrect = false;
            if (tempCompParser.Data.Columns["DL_K2"] == null) dataSchemaIsCorrect = false;
            if (tempCompParser.Data.Columns["DH_R'"] == null) dataSchemaIsCorrect = false;
            if (tempCompParser.Data.Columns["DH_Tref"] == null) dataSchemaIsCorrect = false;
            if (tempCompParser.Data.Columns["DH_K1"] == null) dataSchemaIsCorrect = false;
            if (tempCompParser.Data.Columns["DH_K2"] == null) dataSchemaIsCorrect = false;
            if (tempCompParser.Data.Columns["SL_R'"] == null) dataSchemaIsCorrect = false;
            if (tempCompParser.Data.Columns["SL_Tref"] == null) dataSchemaIsCorrect = false;
            if (tempCompParser.Data.Columns["SL_K1"] == null) dataSchemaIsCorrect = false;
            if (tempCompParser.Data.Columns["SL_K2"] == null) dataSchemaIsCorrect = false;
            if (tempCompParser.Data.Columns["SH_R'"] == null) dataSchemaIsCorrect = false;
            if (tempCompParser.Data.Columns["SH_Tref"] == null) dataSchemaIsCorrect = false;
            if (tempCompParser.Data.Columns["SH_K1"] == null) dataSchemaIsCorrect = false;
            if (tempCompParser.Data.Columns["SH_K2"] == null) dataSchemaIsCorrect = false;
            if (tempCompParser.Data.Columns["Q"] == null) dataSchemaIsCorrect = false;

            if (!dataSchemaIsCorrect)
            {
                InsertValidationErrorTable(false, "", "Temperature Compensation file data schema does not match the required format.", "TempCompensationFileDataSchemaValidation");
                return false;
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "TempCompensationFileDataSchemaValidation: " + ex.ToString(), "TempCompensationFileDataSchemaValidation");
            return false;
        }
        return true;
    }

    //  -------------------- Sensitivity Coefficient DATA SCHEMA -----------------------------//
    private bool SensitivityCoefficientFileDataSchemaValidation()
    {
        bool dataSchemaIsCorrect = true;
        try
        {
            // Check each column exists in the data table that was returned.
            if (sensCoefParser.Data.Columns["sn"] == null) dataSchemaIsCorrect = false;
            if (sensCoefParser.Data.Columns["DL_S2"] == null) dataSchemaIsCorrect = false;
            if (sensCoefParser.Data.Columns["DL_S1"] == null) dataSchemaIsCorrect = false;
            if (sensCoefParser.Data.Columns["DL_S0"] == null) dataSchemaIsCorrect = false;
            if (sensCoefParser.Data.Columns["DH_S2"] == null) dataSchemaIsCorrect = false;
            if (sensCoefParser.Data.Columns["DH_S1"] == null) dataSchemaIsCorrect = false;
            if (sensCoefParser.Data.Columns["DH_S0"] == null) dataSchemaIsCorrect = false;
            if (sensCoefParser.Data.Columns["SL_S2"] == null) dataSchemaIsCorrect = false;
            if (sensCoefParser.Data.Columns["SL_S1"] == null) dataSchemaIsCorrect = false;
            if (sensCoefParser.Data.Columns["SL_S0"] == null) dataSchemaIsCorrect = false;
            if (sensCoefParser.Data.Columns["SH_S2"] == null) dataSchemaIsCorrect = false;
            if (sensCoefParser.Data.Columns["SH_S1"] == null) dataSchemaIsCorrect = false;
            if (sensCoefParser.Data.Columns["SH_S0"] == null) dataSchemaIsCorrect = false;

            if (!dataSchemaIsCorrect)
            {
                InsertValidationErrorTable(false, "", "Sensitivity Coefficient file data schema does not match the required format.", "SensitivityCoefficientFileDataSchemaValidation");
                return false;
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "SensitivityCoefficientFileDataSchemaValidation: " + ex.ToString(), "SensitivityCoefficientFileDataSchemaValidation");
            return false;
        }
        return true;
    }

    //  -------------------- READER DATA SCHEMA -----------------------------//
    private bool ReaderFileDataSchemaValidation()
    {
        bool dataSchemaIsCorrect = true;
        try
        {
            // Check each column exists in the data table that was returned.
            if (readerParser.Data.Columns["sn"] == null) dataSchemaIsCorrect = false;
            if (readerParser.Data.Columns["mfg_date"] == null) dataSchemaIsCorrect = false;
            if (readerParser.Data.Columns["hw_version"] == null) dataSchemaIsCorrect = false;
            if (readerParser.Data.Columns["TYPE"] == null) dataSchemaIsCorrect = false;
            if (readerParser.Data.Columns["LOT/BOX"] == null) dataSchemaIsCorrect = false;
            if (readerParser.Data.Columns["PURCHASE ORDER"] == null) dataSchemaIsCorrect = false;
            if (readerParser.Data.Columns["INVOICE"] == null) dataSchemaIsCorrect = false;
            if (readerParser.Data.Columns["NOTE"] == null) dataSchemaIsCorrect = false;
            if (readerParser.Data.Columns["InvoiceDate"] == null) dataSchemaIsCorrect = false;
                       
            if (!dataSchemaIsCorrect)
            {
                InsertValidationErrorTable(false, "", "Instalink file data schema does not match the required format.", "ReaderFileDataSchemaValidation");
                return false;
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "ReaderFileDataSchemaValidation: " + ex.ToString(), "ReaderFileDataSchemaValidation");
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

    //  -------------------- Temp Compensation ROW MISSING DATA --------------------------//
    private bool TempCompensationRowDataValidation()
    {
        bool pass = true;
        try
        {
            // Loop through each of the rows
            foreach (DataRow tempCompensationRow in tempCompParser.Data.Rows)
            {
                // Skip if it's a blank row
                if (isTempCompensationRowBlank(tempCompensationRow)) continue;

                if (isTempCompensationRowMissingData(tempCompensationRow))
                {
                    string message = "TempCompensation file, S/N: " + tempCompensationRow["sn"].ToString().Trim() + " is missing data.";
                    InsertValidationErrorTable(false, tempCompensationRow["sn"].ToString().Trim(), message, "TempCompensationRowDataValidation");
                    pass = false;
                }
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "TempCompensationRowDataValidation: " + ex.ToString(), "TempCompensationRowDataValidation");
            pass = false;
        }
        return pass;
    }

    //  -------------------- Sens Coefficient ROW MISSING DATA --------------------------//
    private bool SensCoefficientRowDataValidation()
    {
        bool pass = true;
        try
        {
            // Loop through each of the rows
            foreach (DataRow sensCoefRow in sensCoefParser.Data.Rows)
            {
                // Skip if it's a blank row
                if (isSensCoefficientRowBlank(sensCoefRow)) continue;

                if (isSensCoefficientRowMissingData(sensCoefRow))
                {
                    string message = "Sensitivity Coefficient file, S/N: " + sensCoefRow["sn"].ToString().Trim() + " is missing data.";
                    InsertValidationErrorTable(false, sensCoefRow["sn"].ToString().Trim(), message, "SensCoefficientRowDataValidation");
                    pass = false;
                }
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "SensCoefficientRowDataValidation: " + ex.ToString(), "SensCoefficientRowDataValidation");
            pass = false;
        }
        return pass;
    }

    //  -------------------- READER ROW MISSING DATA --------------------------//
    private bool ReaderRowDataValidation()
    {
        bool pass = true;
        try
        {
            // Loop through each of the rows
            foreach (DataRow readerRow in readerParser.Data.Rows)
            {
                // Skip if it's a blank row
                if (isReaderRowBlank(readerRow)) continue;

                if (isReaderRowMissingData(readerRow))
                {
                    string message = "Instalink file, S/N: " + readerRow["sn"].ToString().Trim() + " is missing data.";
                    InsertValidationErrorTable(false, readerRow["sn"].ToString().Trim(), message, "ReaderRowDataValidation");
                    pass = false;
                }
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "ReaderRowDataValidation: " + ex.ToString(), "ReaderRowDataValidation");
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
                    if (mfgDate.AddMonths(6) < System.DateTime.Today)
                    {
                        string message = "Master file, S/N: " + masterRow["sn"].ToString().Trim() + " is out-of-date Manufacturing Date. Uploaded anyway.";
                        InsertValidationErrorTable(true, masterRow["sn"].ToString().Trim(), message, "MasterRowManufacturingDateValidation");
                        pass = false;
                    }
                }
                else
                {
                    string message = "Master file, S/N: " + masterRow["sn"].ToString().Trim() + " is wrong Manufacturing Date format. Uploaded anyway.";
                    InsertValidationErrorTable(true, masterRow["sn"].ToString().Trim(), message, "MasterRowManufacturingDateValidation");
                    pass = false;
                }

            }

        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "MasterRowManufacturingDateValidation: " + ex.ToString(), "MasterRowManufacturingDateValidation");
            //pass = false;
            return false;
        }

        //return pass;        
        return true;
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

    //  -------------------- CHECK READER ROW DUPLICATE S/N -------------------//
    private bool ReaderRowDuplicateValidation()
    {
        bool pass = true;
        Hashtable snHash = new Hashtable();
        try
        {
            // Loop through each of the rows
            foreach (DataRow readerRow in readerParser.Data.Rows)
            {
                // Skip if it's a blank row
                if (isReaderRowBlank(readerRow)) continue;

                if (!snHash.ContainsKey(readerRow["sn"].ToString().Trim()))
                {
                    snHash.Add(readerRow["sn"].ToString().Trim(), "");
                }
                else
                {
                    string message = "Instalink file, S/N: " + readerRow["sn"].ToString().Trim() + " is duplicated.";
                    InsertValidationErrorTable(false, readerRow["sn"].ToString().Trim(), message, "ReaderRowDuplicateValidation");
                    pass = false;
                }
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "ReaderRowDuplicateValidation: " + ex.ToString(), "ReaderRowDuplicateValidation");
            pass = false;
        }
        return pass;
    }

    // --------------------- CHECK TEMP COMPENSATION ROW DUPLICATE, if more than 6 rows-------------------//
    private bool TempCompensationRowDuplicateValidation()
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

                foreach (DataRow tempCompensationRow in tempCompParser.Data.Rows)
                {
                    // Skip if it's a blank row
                    if (isTempCompensationRowBlank(tempCompensationRow)) continue;

                    // If the record belonds to this record
                    if (masterRow["sn"].ToString().Trim() == tempCompensationRow["sn"].ToString().Trim())
                    {
                        count++;
                    }
                    // Get out of the TempCompensation iteration when count > 6
                    if (count > 6) break;
                }

                // if count > 6 it means some TempCompensations duplicated
                if (count > 6)
                {
                    string message = "Temperature Compensation file, S/N: " + masterRow["sn"].ToString().Trim() + " is duplicated.";
                    InsertValidationErrorTable(false, masterRow["sn"].ToString().Trim(), message, "TempCompensationRowDuplicateValidation");
                    pass = false;
                }
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "TempCompensationRowDuplicateValidation: " + ex.ToString(), "TempCompensationRowDuplicateValidation");
            pass = false;
        }
        return pass;
    }

    // --------------------- CHECK SENSITIVITY COEFFICIENT ROW DUPLICATE, if more than 1 row ---------------------//
    private bool SensCoefficientRowDuplicateValidation()
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

                foreach (DataRow sensCoefficientRow in sensCoefParser.Data.Rows)
                {
                    // Skip if it's a blank row
                    if (isSensCoefficientRowBlank(sensCoefficientRow)) continue;

                    // If the record belonds to this record
                    if (masterRow["sn"].ToString().Trim() == sensCoefficientRow["sn"].ToString().Trim())
                    {
                        count++;
                    }
                    // Get out of the SensCoefficient iteration when count > 1
                    if (count > 1) break;
                }

                // if count > 6 it means some SensCoefficients duplicated
                if (count > 1)
                {
                    string message = "Sensitivity Coefficient file, S/N: " + masterRow["sn"].ToString().Trim() + " is duplicated.";
                    InsertValidationErrorTable(false, masterRow["sn"].ToString().Trim(), message, "SensCoefficientRowDuplicateValidation");
                    pass = false;
                }
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "SensCoefficientRowDuplicateValidation: " + ex.ToString(), "SensCoefficientRowDuplicateValidation");
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

    //  -------------------- CHECK MASTER ROW S/N HAS CORRECT FORMAT -------------------//
    private bool ReaderSerialNoFormatValidation()
    {
        bool pass = true;

        try
        {
            // Loop through each of the rows
            foreach (DataRow readerRow in readerParser.Data.Rows)
            {
                // Skip if it's a blank row
                if (isReaderRowBlank(readerRow)) continue;

                // Ensure the serial number was grabbed.
                string serialNo = readerRow["sn"].ToString().Trim();

                // Do not upload onto MAS if the device's serialno is out of format.
                if (serialNo.Length > 10)
                {
                    string message = "Instalink file, S/N: " + serialNo + " is out of the format. Max 10 characters.";
                    InsertValidationErrorTable(false, serialNo, message, "ReaderSerialNoFormatValidation");
                    pass = false;
                }
            }

        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "ReaderSerialNoFormatValidation: " + ex.ToString(), "ReaderSerialNoFormatValidation");
            pass = false;
        }
        return pass;
    }

    //  -------------------- CHECK READER ROW S/N EXIST IN INVENTORY -------------------//
    private bool ReaderSerialNoExistValidation()
    {
        bool pass = true;

        try
        {
            // Get the device inventory records
            IQueryable<ProductInventory> productInventory =
                (from di in IDC.ProductInventories  select di).AsQueryable();

            // Loop through each of the rows
            foreach (DataRow readerRow in readerParser.Data.Rows)
            {
                // Skip if it's a blank row
                if (isReaderRowBlank(readerRow)) continue;

                // Ensure the serial number was grabbed.
                string serialNo = readerRow["sn"].ToString().Trim();

                // Perform check
                int exists = (from di in productInventory where di.SerialNo == serialNo select di.ProductInventoryID ).Count();

                // Do not add ProductInventory record if the device exists.
                if (exists > 0)
                {
                    string message = "Instalink file, S/N: " + readerRow["sn"].ToString().Trim() + " is already existed in Product Inventory.";
                    InsertValidationErrorTable(false, readerRow["sn"].ToString().Trim(), message, "ReaderSerialNoExistValidation");
                    pass = false;
                }
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "ReaderSerialNoExistValidation: " + ex.ToString(), "ReaderSerialNoExistValidation");
            pass = false;
        }
        return pass;
    }

    //  -------------------- CHECK Temp Compensation ROW S/N EXIST IN INVENTORY -------------------//
    private bool TempCompensationSerialNoExistValidation()
    {
        bool pass = true;

        try
        {
            // Get the device inventory records
            IQueryable<DeviceInventory> deviceInventory =
                (from di in IDC.DeviceInventories select di).AsQueryable();

            foreach (DataRow tempCompensationRow in tempCompParser.Data.Rows)
            {
                // Skip if it's a blank row
                if (isTempCompensationRowBlank(tempCompensationRow)) continue;

                // Ensure the serial number was grabbed.
                string serialNo = tempCompensationRow["sn"].ToString().Trim();

                // Perform check
                int exists = (from di in deviceInventory where di.SerialNo == serialNo select di.DeviceID).Count();

                // Do not add DeviceInventory record if the device exists.
                if (exists > 0)
                {
                    string message = "Temperature Compensation file, S/N: " + tempCompensationRow["sn"].ToString().Trim() + " is already existed in Device Inventory.";
                    InsertValidationErrorTable(false, tempCompensationRow["sn"].ToString().Trim(), message, "TempCompensationSerialNoExistValidation");
                    pass = false;
                }
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "TempCompensationSerialNoExistValidation: " + ex.ToString(), "TempCompensationSerialNoExistValidation");
            pass = false;
        }
        return pass;
    }

    //  -------------------- CHECK Sensitivity Coefficient ROW S/N EXIST IN INVENTORY -------------------//
    private bool SensCoefficientSerialNoExistValidation()
    {
        bool pass = true;

        try
        {
            // Get the device inventory records
            IQueryable<DeviceInventory> deviceInventory =
                (from di in IDC.DeviceInventories select di).AsQueryable();

            foreach (DataRow sensCoefficientRow in sensCoefParser.Data.Rows)
            {
                // Skip if it's a blank row
                if (isSensCoefficientRowBlank(sensCoefficientRow)) continue;

                // Ensure the serial number was grabbed.
                string serialNo = sensCoefficientRow["sn"].ToString().Trim();

                // Perform check
                int exists = (from di in deviceInventory where di.SerialNo == serialNo select di.DeviceID).Count();

                // Do not add DeviceInventory record if the device exists.
                if (exists > 0)
                {
                    string message = "Sensitivity Coefficient file, S/N: " + sensCoefficientRow["sn"].ToString().Trim() + " is already existed in Device Inventory.";
                    InsertValidationErrorTable(false, sensCoefficientRow["sn"].ToString().Trim(), message, "SensCoefficientSerialNoExistValidation");
                    pass = false;
                }
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "SensCoefficientSerialNoExistValidation: " + ex.ToString(), "SensCoefficientSerialNoExistValidation");
            pass = false;
        }
        return pass;
    }



    // --------------------- Upload anyway but still validate -----------------------------------//

    // --------------------- CHECK a S/N has temp_ID0,temp_ID1,temp_ID2,temp_ID3,temp_ID4,temp_ID5 (missing any [0..5] Temp Comp rows)  ---------------------//
    private bool TotalTempCompensationBySnValidation()
    {
        bool pass = true;
        bool temp_id0 = false;
        bool temp_id1 = false;
        bool temp_id2 = false;
        bool temp_id3 = false;
        bool temp_id4 = false;
        bool temp_id5 = false;

        try
        {
            // Loop through each of the rows
            foreach (DataRow masterRow in masterParser.Data.Rows)
            {
                // Skip if it's a blank row
                if (isMasterRowBlank(masterRow)) continue;

                temp_id0 = false;
                temp_id1 = false;
                temp_id2 = false;
                temp_id3 = false;
                temp_id4 = false;
                temp_id5 = false;

                foreach (DataRow tempCompensationRow in tempCompParser.Data.Rows)
                {
                    // Skip if it's a blank row
                    if (isTempCompensationRowBlank(tempCompensationRow)) continue;

                    // If the record belonds to this record
                    if (masterRow["sn"].ToString().Trim() == tempCompensationRow["sn"].ToString().Trim())
                    {
                        int tempID = -1;
                        int.TryParse(tempCompensationRow["Temp_id"].ToString().Trim(), out tempID);

                        if (tempID == 0) temp_id0 = true;
                        if (tempID == 1) temp_id1 = true;
                        if (tempID == 2) temp_id2 = true;
                        if (tempID == 3) temp_id3 = true;
                        if (tempID == 4) temp_id4 = true;
                        if (tempID == 5) temp_id5 = true;
                    }

                    // Get out of the iteration for this master file
                    if (temp_id0 && temp_id1 && temp_id2 && temp_id3 && temp_id4 && temp_id5) break;
                }

                // if the not all 6 records were found for a file, exist
                if (!temp_id0 || !temp_id1 || !temp_id2 || !temp_id3 || !temp_id4 || !temp_id5)
                {
                    string message = "Temperature Compensation file, S/N: " + masterRow["sn"].ToString().Trim() + " is missing TempCompensation. Uploaded anyway.";
                    InsertValidationErrorTable(true, masterRow["sn"].ToString().Trim(), message, "TotalTempCompensationBySnValidation");
                    pass = false;
                }
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "TotalTempCompensationBySnValidation: " + ex.ToString(), "TotalTempCompensationBySnValidation");
            return false;
        }

        //return pass;
        return true; // No raising warning message. Upload anyway.
    }

    // --------------------- CHECK MASTER MATCH WITH TempCompensation --------------------//
    private bool MasterMatchTempCompensationValidation()
    {
        bool pass = true;
        bool match = false;
        List<string> MMList = new List<string>();

        try
        {
            // Loop through each of the rows
            foreach (DataRow tempCompensationRow in tempCompParser.Data.Rows)
            {
                match = false;

                // Skip if it's a blank row
                if (isTempCompensationRowBlank(tempCompensationRow)) continue;

                foreach (DataRow masterRow in masterParser.Data.Rows)
                {
                    // Skip if it's a blank row
                    if (isMasterRowBlank(masterRow)) continue;

                    // If the record belongs to this record
                    if (masterRow["sn"].ToString().Trim() == tempCompensationRow["sn"].ToString().Trim())
                    {
                        match = true;
                        break;
                    }
                }

                // if not found in the master
                if (!match)
                {
                    // This makes sure we do not save the same error 6 times due to 6 TempCompensation records for a s/n 
                    if (!MMList.Contains(tempCompensationRow["sn"].ToString().Trim()))
                    {
                        string message = "Master file is missing a record for S/N: " + tempCompensationRow["sn"].ToString().Trim() + ". Uploaded anyway.";
                        InsertValidationErrorTable(true, tempCompensationRow["sn"].ToString().Trim(), message, "MasterMatchTempCompensationValidation");
                        pass = false;

                        MMList.Add(tempCompensationRow["sn"].ToString().Trim());
                    }
                }
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "MasterMatchTempCompensationValidation: " + ex.ToString(), "MasterMatchTempCompensationValidation");
            return false;
        }

        //return pass;
        return true; // No raising warning message. Upload anyway.
    }

    // --------------------- CHECK MASTER MATCH WITH Sensitivity Coefficient --------------------//
    private bool MasterMatchSensitivityCoefValidation()
    {
        bool pass = true;
        bool match = false;

        try
        {
            // Loop through each of the rows
            foreach (DataRow sensCoefRow in sensCoefParser.Data.Rows)
            {
                match = false;

                // Skip if it's a blank row
                if (isSensCoefficientRowBlank(sensCoefRow)) continue;

                foreach (DataRow masterRow in masterParser.Data.Rows)
                {
                    // Skip if it's a blank row
                    if (isMasterRowBlank(masterRow)) continue;

                    // If the record belongs to this record
                    if (masterRow["sn"].ToString().Trim() == sensCoefRow["sn"].ToString().Trim())
                    {
                        match = true;
                        break;
                    }
                }

                // if not found in the master
                if (!match)
                {
                    string message = "Master file is missing a record for S/N: " + sensCoefRow["sn"].ToString().Trim() + ". Uploaded anyway.";
                    InsertValidationErrorTable(true, sensCoefRow["sn"].ToString().Trim(), message, "MasterMatchSensitivityCoefValidation");
                    pass = false;
                }
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "MasterMatchSensitivityCoefValidation: " + ex.ToString(), "MasterMatchSensitivityCoefValidation");
            return false;
        }

        //return pass;
        return true; // No raising warning message. Upload anyway.
    }

    // --------------------- CHECK Sensitivity Coef match with TempCompensation --------------------//
    private bool SensCoefficientMatchTempCompensationValidation()
    {
        bool pass = true;
        bool match = false;
        List<string> MMList = new List<string>();

        try
        {
            // Loop through each of the rows
            foreach (DataRow tempCompensationRow in tempCompParser.Data.Rows)
            {
                match = false;

                // Skip if it's a blank row
                if (isTempCompensationRowBlank(tempCompensationRow)) continue;

                foreach (DataRow sensCoefRow in sensCoefParser.Data.Rows)
                {
                    // Skip if it's a blank row
                    if (isSensCoefficientRowBlank(sensCoefRow)) continue;

                    // If the record belongs to this record
                    if (sensCoefRow["sn"].ToString().Trim() == tempCompensationRow["sn"].ToString().Trim())
                    {
                        match = true;
                        break;
                    }
                }

                // if not found in Sensitivity Coefficient table
                if (!match)
                {
                    // This makes sure we do not save the same error 6 times due to 6 TempCompensation records for a s/n 
                    if (!MMList.Contains(tempCompensationRow["sn"].ToString().Trim()))
                    {
                        string message = "Sensitivity Coefficient file is missing a record for S/N: " + tempCompensationRow["sn"].ToString().Trim() + ". Uploaded anyway.";
                        InsertValidationErrorTable(true, tempCompensationRow["sn"].ToString().Trim(), message, "SensCoefficientMatchTempCompensationValidation");
                        pass = false;

                        MMList.Add(tempCompensationRow["sn"].ToString().Trim());
                    }
                }
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "SensCoefficientMatchTempCompensationValidation: " + ex.ToString(), "SensCoefficientMatchTempCompensationValidation");
            return false;
        }

        //return pass;
        return true; // No raising warning message. Upload anyway.
    }

    // ----- CHECK THE TOTAL OF DEVICE UPLOADED WITH THE TOTAL OF DEVICE RECEIVED AT SHIPPING/RECEIVING -------//
    private bool TotalColorValidation()
    {
        bool pass = true;
        try
        {
            int id2Total = 0;
            int instalinkTotal = 0;
            int instalinkUSBTotal = 0;

            // Calculate total device being uploaded

            if (this.uploadDevice)
            {
                foreach (DataRow masterRow in masterParser.Data.Rows)
                {
                    // Skip if it's a blank row
                    if (isMasterRowBlank(masterRow)) continue;

                    id2Total++;
                }
            }

            if (this.uploadReader)
            {
                foreach (DataRow readerRow in readerParser.Data.Rows)
                {
                    // Skip if it's a blank row
                    if (isReaderRowBlank(readerRow)) continue;

                    switch (readerRow["TYPE"].ToString().Trim().ToUpper())
                    {
                        case "INSTALINK":
                            instalinkTotal++;
                            break;
                        case "INSTALINKUSB":
                            instalinkUSBTotal++;
                            break;
                        default:
                            break;
                    }
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

                    if (itemNumber.Contains("FDA20"))
                    {
                        if (qtyRecd != id2Total)
                        {
                            if (id2Total > lessQtyRecd && id2Total < moreQtyRecd)
                            {
                                string message = "The total of ID2 between the Master file and Receipt Header is not in-synched.";
                                InsertValidationErrorTable(true, "", message, "TotalColorValidation");      // Upload anyway.                                
                            }
                            else
                            {
                                string message = "The different total of ID2 between the Master file and Receipt Header is greater than 10%.";
                                InsertValidationErrorTable(false, "", message, "TotalColorValidation");
                                pass = false;
                            }
                        }
                    }
                    if (itemNumber.Contains("INSTALINK"))
                    {
                        if (qtyRecd != instalinkTotal)
                        {
                            if (instalinkTotal > lessQtyRecd && instalinkTotal < moreQtyRecd)
                            {
                                string message = "The total of Instalink between the Instalink file and Receipt Header is not in-synched.";
                                InsertValidationErrorTable(true, "", message, "TotalColorValidation");
                                // No raising warning message. Upload anyway.                                
                            }
                            else
                            {
                                string message = "The different total of Instalink between the Instalink file and Receipt Header is greater than 10%.";
                                InsertValidationErrorTable(false, "", message, "TotalColorValidation");
                                pass = false;
                            }
                        }
                    }
                    if (itemNumber.Contains("INSTALINKUSB"))
                    {
                        if (qtyRecd != instalinkUSBTotal)
                        {
                            if (instalinkUSBTotal > lessQtyRecd && instalinkUSBTotal < moreQtyRecd)
                            {
                                string message = "The total of InstalinkUSB between the Instalink file and Receipt Header is not in-synched.";
                                InsertValidationErrorTable(true, "", message, "TotalColorValidation");
                                // No raising warning message. Upload anyway.                                
                            }
                            else
                            {
                                string message = "The different total of InstalinkUSB between the Instalink file and Receipt Header is greater than 10%.";
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

            // Loop through master file

            if (this.uploadDevice)
            {
                foreach (DataRow masterRow in masterParser.Data.Rows)
                {
                    // Skip if it's a blank row
                    if (isMasterRowBlank(masterRow)) continue;
                    // count the number of s/n uploaded
                    uploadTotal += 1;
                }
            }
           
            // Loop through instalink, instalinkUSB file
            if (uploadReader)
            {
                foreach (DataRow readerRow in readerParser.Data.Rows)
                {
                    // Skip if it's a blank row
                    if (isReaderRowBlank(readerRow)) continue;
                    // count the number of s/n uploaded
                    uploadTotal += 1;
                }
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
                InsertValidationErrorTable(false, "", message, "TotalBackOrderValidation");
                pass = false;
            }
            else if (uploadTotal > totalBackOrderQty)
            {
                string message = "The total of uploaded is greater than the BackOrder Quantity but less than 10%.";
                InsertValidationErrorTable(true, "", message, "TotalBackOrderValidation");  // Upload anyway.                      
            }
        }
        catch (Exception ex)
        {
            InsertValidationErrorTable(false, "", "TotalBackOrderValidation: " + ex.ToString(), "TotalBackOrderValidation");
            pass = false;
        }

        return pass;
    }

    // --------------------- Upload anyway but still validate -----------------------------------//

    public bool MasterBaselineUploadValidation()
    {
        InitiateValidationErrorTable();

        try
        {
            //if (!IsMASAvailableForPOUpload())
            //{
            //    return false;
            //}

            if (uploadDevice)
            {
                if (!MasterFileParsingValidation())
                {
                    return false;
                }

                if (!TempCompensationFileParsingValidation())
                {
                    return false;
                }

                if (!SensitivityCoefficientFileParsingValidation())
                {
                    return false;
                }

                if (!MasterFileDataSchemaValidation())
                {
                    return false;
                }

                if (!TempCompensationFileDataSchemaValidation())
                {
                    return false;
                }

                if (!SensitivityCoefficientFileDataSchemaValidation())
                {
                    return false;
                }

                if (!MasterRowDataValidation())
                {
                    return false;
                }

                if (!TempCompensationRowDataValidation())
                {
                    return false;
                }

                if (!SensCoefficientRowDataValidation())
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

                if (!TempCompensationRowDuplicateValidation())
                {
                    return false;
                }

                if (!SensCoefficientRowDuplicateValidation())
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

                if (!TempCompensationSerialNoExistValidation())
                {
                    return false;
                }

                if (!SensCoefficientSerialNoExistValidation())
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

                if (!MasterMatchTempCompensationValidation())
                {
                    return false;
                }

                if (!MasterMatchSensitivityCoefValidation())
                {
                    return false;
                }

                if (!TotalTempCompensationBySnValidation())
                {
                    return false;
                }

                if (!SensCoefficientMatchTempCompensationValidation())
                {
                    return false;
                }
            }

            if (uploadReader)
            {
                if (!ReaderFileParsingValidation())
                {
                    return false;
                }

                if (!ReaderFileDataSchemaValidation())
                {
                    return false;
                }

                if (!ReaderRowDataValidation())
                {
                    return false;
                }

                if (!ReaderRowDuplicateValidation())
                {
                    return false;
                }

                if (!ReaderSerialNoFormatValidation())
                {
                    return false;
                }

                if (!ReaderSerialNoExistValidation())
                {
                    return false;
                }                                
            }

            if (!TotalBackOrderValidation())
            {
                return false;
            }

            // Tdo, 1/31/2014. Right now let remove this constraint by Kip's request
            //if (!TotalColorValidation())
            //{
            //    return false;
            //}
           
            // --------------------------------------------------------------------------------------------//           
        }
        catch (Exception ex)
        {
            return false;
        }

        return true;
    }

    public int ActualNumberOfUploadedDeviceByItemNumber(string pItemNumber)
    {
        int retCount = 0;
        try
        {
            int id2Total = 0;
            int instalinkTotal = 0;
            int instalinkUSBTotal = 0;

            if (this.uploadDevice)
            {
                foreach (DataRow masterRow in masterParser.Data.Rows)
                {
                    // Skip if it's a blank row
                    if (isMasterRowBlank(masterRow)) continue;

                    id2Total++;
                }
            }

            if (this.uploadReader)
            {
                foreach (DataRow readerRow in readerParser.Data.Rows)
                {
                    // Skip if it's a blank row
                    if (isReaderRowBlank(readerRow)) continue;

                    switch (readerRow["TYPE"].ToString().Trim().ToUpper())
                    {
                        case "INSTALINK":
                            instalinkTotal++;
                            break;
                        case "INSTALINKUSB":
                            instalinkUSBTotal++;
                            break;
                        default:
                            break;
                    }
                }
            }
           
            if (pItemNumber.Contains("FDA20"))
            {
                retCount = id2Total;
            }
            if (pItemNumber.Contains("INSTALINK"))
            {
                retCount = instalinkTotal;
            }
            if (pItemNumber.Contains("INSTALINKUSB"))
            {
                retCount = instalinkUSBTotal;
            }           

            return retCount;
        }
        catch
        {
            return retCount;
        }
    }

    public void Execute(ref string resultMessages)
    {
        string Message = "";

        try
        {            
            // ------------------------------------ VERIFY MASTER & INSERT DEVICE INVENTORY --------------------------------------//

            // Get the device inventory records
            //IQueryable<DeviceInventory> deviceInventory =
            //    (from di in IDC.DeviceInventories select di).AsQueryable();

            int noUpload = 0;               // Number of devices + reader uploaded
            int noReaderUpload = 0;         // Number of readers uploaded
            int noDeviceUpload = 0;         // Number of devices uploaded
            int noSkip = 0;                 // Number of devices skipped because already uploaded            
            int noRFT = 0;                  // Number of good devices uploaded, (Ready for Testing)
            int noMM = 0;                   // Number of bad devices uploaded, (Missing Master record)
            int noMTC = 0;                  // Number of bad devices uploaded, (Missing Temperature Compensation records)
            int noMSC = 0;                  // Number of bad devices uploaded, (Missing Sensitivity Coefficient records)   
            int noReceived = 0;             // Number of devices + reader received per receipt
            int noMfgDateOutofDate = 0;     // Number of good devices uploaded, (but Manufacturing Date is out of date)

            // Set a createdDate
            DateTime createdDate = DateTime.Now;            

            // Get the default group
            DeviceGroup defaultGroup = (from dg in IDC.DeviceGroups where dg.DeviceGroupName == NEWDEVICE_GROUP select dg).FirstOrDefault();

            // Get the default status
            DeviceAnalysisStatus defaultStatus = (from ds in IDC.DeviceAnalysisStatus where ds.DeviceAnalysisName == NEWDEVICE_STATUS select ds).FirstOrDefault();

            // Create a new firmware version, that will be replaced once set.
            DeviceFirmware deviceFirmware = new DeviceFirmware();

            if (this.uploadDevice)
            {

                // Store the new devices
                List<DeviceInventory> devicesToUpload = new List<DeviceInventory>();

                // Loop through each row of Master file
                foreach (DataRow row in masterParser.Data.Rows)
                {
                    // Skip if the serial no is blank
                    if (isMasterRowBlank(row)) continue;

                    // Ensure the serial number was grabbed.
                    string serialNo = row["sn"].ToString().Trim();

                    noDeviceUpload++;

                    // Parse the manufacturing date
                    string mfgDay = row["mfg_date"].ToString().Trim().Substring(6, 2);
                    string mfgMonth = row["mfg_date"].ToString().Trim().Substring(4, 2);
                    string mfgYear = row["mfg_date"].ToString().Trim().Substring(0, 4);

                    // Parse the firmware version
                    string fwMajor;
                    string fwMinor;

                    // Somehow If the hw_version string from master file contain 1 digit only, assuming fwMinor = 0               
                    if (row["hw_version"].ToString().Trim().Length >= 3)
                    {
                        fwMajor = row["hw_version"].ToString().Trim().Substring(0, 1);
                        fwMinor = row["hw_version"].ToString().Trim().Substring(2, 1);
                    }
                    else
                    {
                        fwMajor = row["hw_version"].ToString().Trim().Substring(0, 1);
                        fwMinor = "0";
                    }

                    // Convert the firmware version to a double.
                    double firmwareVersion = 1;
                    double.TryParse(string.Format("{0}.{1}", fwMajor, fwMinor), out firmwareVersion);

                    // If the firmware does not match the existing record, requery.
                    if (deviceFirmware.FirmwareVersion != firmwareVersion.ToString())
                    {
                        deviceFirmware = (from df in IDC.DeviceFirmwares where df.FirmwareVersion == firmwareVersion.ToString() select df).FirstOrDefault();
                    }

                    // Grab lot/box number.
                    string boxNumber = row["LOT/BOX"].ToString().Trim();

                    // Grab all Initial Read Voltages
                    int readVolt0 = Convert.ToInt32(double.Parse(row["ReadVolt0"].ToString()));
                    int readVolt1 = Convert.ToInt32(double.Parse(row["ReadVolt1"].ToString()));
                    int readVolt2 = Convert.ToInt32(double.Parse(row["ReadVolt2"].ToString()));
                    int readVolt3 = Convert.ToInt32(double.Parse(row["ReadVolt3"].ToString()));
                    int readVolt4 = Convert.ToInt32(double.Parse(row["ReadVolt4"].ToString()));
                    int readVolt5 = Convert.ToInt32(double.Parse(row["ReadVolt5"].ToString()));
                    int readVolt6 = Convert.ToInt32(double.Parse(row["ReadVolt6"].ToString()));
                    int readVolt7 = Convert.ToInt32(double.Parse(row["ReadVolt7"].ToString()));

                    double? deepLowSensCoeff0 = null;
                    double? deepLowSensCoeff1 = null;
                    double? deepLowSensCoeff2 = null;
                    double? deepHighSensCoeff0 = null;
                    double? deepHighSensCoeff1 = null;
                    double? deepHighSensCoeff2 = null;
                    double? shallowLowSensCoeff0 = null;
                    double? shallowLowSensCoeff1 = null;
                    double? shallowLowSensCoeff2 = null;
                    double? shallowHighSensCoeff0 = null;
                    double? shallowHighSensCoeff1 = null;
                    double? shallowHighSensCoeff2 = null;

                    // Grab additional fields for manufacturing testing.
                    int? panelID;
                    int? position;
                    string bDAddress = row["BDAddress"].ToString().Trim();

                    if (bDAddress == "???" || bDAddress == "-1" || bDAddress == "") bDAddress = null;

                    if (row["PanelID"].ToString().Trim() == "???" || row["PanelID"].ToString().Trim() == "-1" || row["PanelID"].ToString().Trim() == "")
                        panelID = (int?)null;
                    else
                        panelID = int.Parse(row["PanelID"].ToString().Trim());

                    if (row["Position"].ToString().Trim() == "???" || row["Position"].ToString().Trim() == "-1" || row["Position"].ToString().Trim() == "")
                        position = (int?)null;
                    else
                        position = int.Parse(row["Position"].ToString().Trim());                    

                    // Grab all sensitivity coefficients
                    foreach (DataRow sRow in sensCoefParser.Data.Rows)
                    {
                        // Skip if the serial no is blank
                        if (isSensCoefficientRowBlank(sRow)) continue;

                        if (serialNo == sRow["sn"].ToString().Trim())
                        {
                            deepLowSensCoeff0 = Math.Round(double.Parse(sRow["DL_S0"].ToString()), 2);
                            deepLowSensCoeff1 = Math.Round(double.Parse(sRow["DL_S1"].ToString()), 2);
                            deepLowSensCoeff2 = Math.Round(double.Parse(sRow["DL_S2"].ToString()), 5);
                            deepHighSensCoeff0 = Math.Round(double.Parse(sRow["DH_S0"].ToString()), 2);
                            deepHighSensCoeff1 = Math.Round(double.Parse(sRow["DH_S1"].ToString()), 2);
                            deepHighSensCoeff2 = Math.Round(double.Parse(sRow["DH_S2"].ToString()), 5);
                            shallowLowSensCoeff0 = Math.Round(double.Parse(sRow["SL_S0"].ToString()), 2);
                            shallowLowSensCoeff1 = Math.Round(double.Parse(sRow["SL_S1"].ToString()), 2);
                            shallowLowSensCoeff2 = Math.Round(double.Parse(sRow["SL_S2"].ToString()), 5);
                            shallowHighSensCoeff0 = Math.Round(double.Parse(sRow["SH_S0"].ToString()), 2);
                            shallowHighSensCoeff1 = Math.Round(double.Parse(sRow["SH_S1"].ToString()), 2);
                            shallowHighSensCoeff2 = Math.Round(double.Parse(sRow["SH_S2"].ToString()), 5);
                            break;
                        }
                    }

                    // Create a new device record
                    DeviceInventory device = new DeviceInventory()
                    {
                        SerialNo = serialNo,
                        DeviceFirmware = deviceFirmware,
                        DeviceAnalysisStatus = defaultStatus,
                        CurrentGroup = defaultGroup,
                        InitialGroup = defaultGroup,
                        CalibrationMajor = 0,
                        CalibrationMinor = 0,
                        FailedCalibration = false,
                        POBoxNumber = boxNumber,
                        ReceiptID = int.Parse(this.receiptID),
                        Product = (from p in IDC.Products where p.ProductSKU == NEWDEVICE_SKU && p.Color == NEWDEVICE_COLOR select p).FirstOrDefault(),
                        HardwareVersion = NEWDEVICE_HARDWAREVERSION,

                        InitDeviceVoltage0 = readVolt0,
                        InitDeviceVoltage1 = readVolt1,
                        InitDeviceVoltage2 = readVolt2,
                        InitDeviceVoltage3 = readVolt3,
                        InitDeviceVoltage4 = readVolt4,
                        InitDeviceVoltage5 = readVolt5,
                        InitDeviceVoltage6 = readVolt6,
                        InitDeviceVoltage7 = readVolt7,

                        DeepLowSensCoeff0 = deepLowSensCoeff0,
                        DeepLowSensCoeff1 = deepLowSensCoeff1,
                        DeepLowSensCoeff2 = deepLowSensCoeff2,
                        DeepHighSensCoeff0 = deepHighSensCoeff0,
                        DeepHighSensCoeff1 = deepHighSensCoeff1,
                        DeepHighSensCoeff2 = deepHighSensCoeff2,
                        ShallowLowSensCoeff0 = shallowLowSensCoeff0,
                        ShallowLowSensCoeff1 = shallowLowSensCoeff1,
                        ShallowLowSensCoeff2 = shallowLowSensCoeff2,
                        ShallowHighSensCoeff0 = shallowHighSensCoeff0,
                        ShallowHighSensCoeff1 = shallowHighSensCoeff1,
                        ShallowHighSensCoeff2 = shallowHighSensCoeff2,

                        DebugDCA = true,

                        BDAddress = bDAddress,
                        PanelID = panelID,
                        Position = position 
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
                        case DeviceAnalysisName_MTC:
                            noMTC++;
                            break;
                        case DeviceAnalysisName_MSC:
                            noMSC++;
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


                // Missing master list
                List<string> MMList = new List<string>();

                // Loop through all s/n, which is in Sensitivity Coef but not in Master, and upload it anyway
                foreach (DataRow error in this.dtValidationError.Rows)
                {
                    bool uploadAnyway = (bool)error["Commit"];
                    string sn = error["Sn"].ToString().Trim();
                    string validType = error["ValidationType"].ToString();

                    if (validType == "MasterMatchSensitivityCoefValidation" && uploadAnyway)
                    {
                        if (!MMList.Contains(sn))
                        {
                            noDeviceUpload++;

                            double? deepLowSensCoeff0 = null;
                            double? deepLowSensCoeff1 = null;
                            double? deepLowSensCoeff2 = null;
                            double? deepHighSensCoeff0 = null;
                            double? deepHighSensCoeff1 = null;
                            double? deepHighSensCoeff2 = null;
                            double? shallowLowSensCoeff0 = null;
                            double? shallowLowSensCoeff1 = null;
                            double? shallowLowSensCoeff2 = null;
                            double? shallowHighSensCoeff0 = null;
                            double? shallowHighSensCoeff1 = null;
                            double? shallowHighSensCoeff2 = null;

                            // Grab all sensitivity coefficients
                            foreach (DataRow sRow in sensCoefParser.Data.Rows)
                            {
                                // Skip if the serial no is blank
                                if (isSensCoefficientRowBlank(sRow)) continue;

                                if (sn == sRow["sn"].ToString().Trim())
                                {
                                    deepLowSensCoeff0 = Math.Round(double.Parse(sRow["DL_S0"].ToString()), 2);
                                    deepLowSensCoeff1 = Math.Round(double.Parse(sRow["DL_S1"].ToString()), 2);
                                    deepLowSensCoeff2 = Math.Round(double.Parse(sRow["DL_S2"].ToString()), 5);
                                    deepHighSensCoeff0 = Math.Round(double.Parse(sRow["DH_S0"].ToString()), 2);
                                    deepHighSensCoeff1 = Math.Round(double.Parse(sRow["DH_S1"].ToString()), 2);
                                    deepHighSensCoeff2 = Math.Round(double.Parse(sRow["DH_S2"].ToString()), 5);
                                    shallowLowSensCoeff0 = Math.Round(double.Parse(sRow["SL_S0"].ToString()), 2);
                                    shallowLowSensCoeff1 = Math.Round(double.Parse(sRow["SL_S1"].ToString()), 2);
                                    shallowLowSensCoeff2 = Math.Round(double.Parse(sRow["SL_S2"].ToString()), 5);
                                    shallowHighSensCoeff0 = Math.Round(double.Parse(sRow["SH_S0"].ToString()), 2);
                                    shallowHighSensCoeff1 = Math.Round(double.Parse(sRow["SH_S1"].ToString()), 2);
                                    shallowHighSensCoeff2 = Math.Round(double.Parse(sRow["SH_S2"].ToString()), 5);
                                    break;
                                }
                            }

                            // Create a new device record, without all 7 initial voltages and BDAddress, PanelID, Position
                            DeviceInventory device = new DeviceInventory()
                            {
                                SerialNo = sn,
                                DeviceAnalysisStatus = defaultStatus,
                                CurrentGroup = defaultGroup,
                                InitialGroup = defaultGroup,
                                CalibrationMajor = 0,
                                CalibrationMinor = 0,
                                FailedCalibration = false,
                                ReceiptID = int.Parse(this.receiptID),
                                Product = (from p in IDC.Products where p.ProductSKU == NEWDEVICE_SKU && p.Color == NEWDEVICE_COLOR select p).FirstOrDefault(),
                                HardwareVersion = NEWDEVICE_HARDWAREVERSION,

                                DeepLowSensCoeff0 = deepLowSensCoeff0,
                                DeepLowSensCoeff1 = deepLowSensCoeff1,
                                DeepLowSensCoeff2 = deepLowSensCoeff2,
                                DeepHighSensCoeff0 = deepHighSensCoeff0,
                                DeepHighSensCoeff1 = deepHighSensCoeff1,
                                DeepHighSensCoeff2 = deepHighSensCoeff2,
                                ShallowLowSensCoeff0 = shallowLowSensCoeff0,
                                ShallowLowSensCoeff1 = shallowLowSensCoeff1,
                                ShallowLowSensCoeff2 = shallowLowSensCoeff2,
                                ShallowHighSensCoeff0 = shallowHighSensCoeff0,
                                ShallowHighSensCoeff1 = shallowHighSensCoeff1,
                                ShallowHighSensCoeff2 = shallowHighSensCoeff2,

                                DebugDCA = true
                            };

                            MMList.Add(sn);

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
                }

                // Loop through all s/n, which is in Temp Compensation but not in Master, and upload it anyway
                foreach (DataRow error in this.dtValidationError.Rows)
                {
                    bool uploadAnyway = (bool)error["Commit"];
                    string sn = error["Sn"].ToString().Trim();
                    string validType = error["ValidationType"].ToString();

                    if (validType == "MasterMatchTempCompensationValidation" && uploadAnyway)
                    {
                        if (!MMList.Contains(sn))
                        {
                            noDeviceUpload++;

                            // Create a new device record, without all 7 initial voltages, all sensitivity coef, and BDAddress, PanelID, Position
                            DeviceInventory device = new DeviceInventory()
                            {
                                SerialNo = sn,
                                DeviceAnalysisStatus = defaultStatus,
                                CurrentGroup = defaultGroup,
                                InitialGroup = defaultGroup,
                                CalibrationMajor = 0,
                                CalibrationMinor = 0,
                                FailedCalibration = false,
                                ReceiptID = int.Parse(this.receiptID),
                                Product = (from p in IDC.Products where p.ProductSKU == NEWDEVICE_SKU && p.Color == NEWDEVICE_COLOR select p).FirstOrDefault(),
                                HardwareVersion = NEWDEVICE_HARDWAREVERSION,

                                DebugDCA = true
                            };

                            MMList.Add(sn);

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
                }

                // ----------------------------- VERIFY & UPLOAD Temperature Compensation --------------------------------//

                int tempCompSkipped = 0;
                int tempCompAdded = 0;

                // Loop through each row of baselines
                foreach (DataRow row in tempCompParser.Data.Rows)
                {
                    // Skip if the serial no is blank
                    if (isTempCompensationRowBlank(row)) continue;

                    // Ensure the serial number was grabbed.
                    string serialNo = row["sn"].ToString().Trim();

                    int tempId = 0;
                    if (!int.TryParse(row["Temp_id"].ToString().Trim(), out tempId)) continue;

                    // Find the device record in master
                    DeviceInventory device = (from di in devicesToUpload where di.SerialNo == serialNo select di).FirstOrDefault();

                    if (device == null) // Already exist.
                    {
                        tempCompSkipped++;
                    }
                    else
                    {
                        tempCompAdded++;

                        DeviceCompensationID2 deviceCompensation = new DeviceCompensationID2();

                        deviceCompensation.TemperatureID = tempId;

                        deviceCompensation.DeepLowVoltNotTempComp = Convert.ToInt32(double.Parse(row["DL_R'"].ToString()));
                        deviceCompensation.DeepLowRefTemp = Convert.ToInt32(double.Parse(row["DL_Tref"].ToString()));
                        deviceCompensation.DeepLowTempCoeff1 = Math.Round(double.Parse(row["DL_K1"].ToString()), 2);
                        deviceCompensation.DeepLowTempCoeff2 = double.Parse(row["DL_K2"].ToString());

                        deviceCompensation.DeepHighVoltNotTempComp = Convert.ToInt32(double.Parse(row["DH_R'"].ToString()));
                        deviceCompensation.DeepHighRefTemp = Convert.ToInt32(double.Parse(row["DH_Tref"].ToString()));
                        deviceCompensation.DeepHighTempCoeff1 = Math.Round(double.Parse(row["DH_K1"].ToString()), 2);
                        deviceCompensation.DeepHighTempCoeff2 = double.Parse(row["DH_K2"].ToString());

                        deviceCompensation.ShallowLowVoltNotTempComp = Convert.ToInt32(double.Parse(row["SL_R'"].ToString()));
                        deviceCompensation.ShallowLowRefTemp = Convert.ToInt32(double.Parse(row["SL_Tref"].ToString()));
                        deviceCompensation.ShallowLowTempCoeff1 = Math.Round(double.Parse(row["SL_K1"].ToString()), 2);
                        deviceCompensation.ShallowLowTempCoeff2 = double.Parse(row["SL_K2"].ToString());

                        deviceCompensation.ShallowHighVoltNotTempComp = Convert.ToInt32(double.Parse(row["SH_R'"].ToString()));
                        deviceCompensation.ShallowHighRefTemp = Convert.ToInt32(double.Parse(row["SH_Tref"].ToString()));
                        deviceCompensation.ShallowHighTempCoeff1 = Math.Round(double.Parse(row["SH_K1"].ToString()), 2);
                        deviceCompensation.ShallowHighTempCoeff2 = double.Parse(row["SH_K2"].ToString());

                        deviceCompensation.Q = double.Parse(row["Q"].ToString());

                        // Add baseline record
                        device.DeviceCompensationID2s.Add(deviceCompensation);
                    }
                }

                //Message = string.Format("Records to Upload: {0} Skipped Records: {1}", tempCompAdded, tempCompSkipped);

                // UPLOAD ALL DEVICES AND Temperature Compensation DATA
                IDC.DeviceInventories.InsertAllOnSubmit(devicesToUpload);
                IDC.SubmitChanges();

            }
            
            // --------------------- Loop through each row of Instalink file ------------------------//

            if (this.uploadReader)
            {

                // Store the new devices
                List<ProductInventory> instalinkToUpload = new List<ProductInventory>();

                // Loop through each row of instalink file
                foreach (DataRow row in readerParser.Data.Rows)
                {
                    // Skip if the row is blank
                    if (isReaderRowBlank(row)) continue;

                    // Ensure the serial number was grabbed.
                    string serialNo = row["sn"].ToString().Trim();
                    string type = row["TYPE"].ToString().Trim();
                    // Grab lot/box number.
                    string boxNumber = row["LOT/BOX"].ToString().Trim();

                    // Parse the manufacturing date
                    string mfgDay = row["mfg_date"].ToString().Trim().Substring(6, 2);
                    string mfgMonth = row["mfg_date"].ToString().Trim().Substring(4, 2);
                    string mfgYear = row["mfg_date"].ToString().Trim().Substring(0, 4);

                    // Parse the firmware version
                    string fwMajor;
                    string fwMinor;

                    // Somehow If the hw_version string from master file contain 1 digit only, assuming fwMinor = 0               
                    if (row["hw_version"].ToString().Trim().Length >= 3)
                    {
                        fwMajor = row["hw_version"].ToString().Trim().Substring(0, 1);
                        fwMinor = row["hw_version"].ToString().Trim().Substring(2, 1);
                    }
                    else
                    {
                        fwMajor = row["hw_version"].ToString().Trim().Substring(0, 1);
                        fwMinor = "0";
                    }

                    // Convert the firmware version to a double.
                    double firmwareVersion = 1;
                    double.TryParse(string.Format("{0}.{1}", fwMajor, fwMinor), out firmwareVersion);

                    // If the firmware does not match the existing record, requery.
                    if (deviceFirmware.FirmwareVersion != firmwareVersion.ToString())
                    {
                        deviceFirmware = (from df in IDC.DeviceFirmwares where df.FirmwareVersion == firmwareVersion.ToString() select df).FirstOrDefault();
                    }                    

                    ProductInventory device = new ProductInventory()
                    {
                        SerialNo = serialNo,
                        DeviceAnalysisStatus = defaultStatus,
                        DeviceFirmware = deviceFirmware,
                        ReceiptID = int.Parse(this.receiptID),
                        POBoxNumber = boxNumber,
                        Product = (from p in IDC.Products where p.ProductSKU == type select p).FirstOrDefault(),
                        HardwareVersion = NEWDEVICE_HARDWAREVERSION
                    };

                    // Set the manufacturing date.
                    DateTime mfgDate;
                    if (DateTime.TryParse(string.Format("{0}/{1}/{2}", mfgMonth, mfgDay, mfgYear), out mfgDate))
                        device.MfgDate = mfgDate;

                    device.DeviceAnalysisStatus = (from ds in IDC.DeviceAnalysisStatus where ds.DeviceAnalysisName == DeviceAnalysisName_RFT select ds).FirstOrDefault();

                    // Add to the list
                    instalinkToUpload.Add(device);

                    noReaderUpload++;
                }

                // UPLOAD all instalink and instalinkUSB
                IDC.ProductInventories.InsertAllOnSubmit(instalinkToUpload);
                IDC.SubmitChanges();

            }
            
            // ---------------------------------------------------------------------------------------//

            noUpload = noDeviceUpload + noReaderUpload;

            noReceived = GetTotalReceived(int.Parse(this.receiptID));
            noMfgDateOutofDate = NumberOutOfDateMfgDate(this.dtValidationError);

            // Collect all data for the print out report.
            resultMessages += noReceived + "_";
            resultMessages += noUpload + "_";
            resultMessages += noSkip + "_";
            resultMessages += noRFT + "_";
            resultMessages += noMM + "_";
            resultMessages += noMTC + "_";
            resultMessages += noMSC + "_";
            resultMessages += noMfgDateOutofDate + "_";
            resultMessages += TotalUploadTotalReceiveNotInSynched(this.dtValidationError) + "_";        // Check if total uploaded and total received are matched. 
            resultMessages += TotalBackOrderTotalUploadNotInSynched(this.dtValidationError) + "_";      // Check if total uploaded and total back order are matched.  

            //Message = string.Format("Completed.");

            this.totalActualDeviceUploaded = noDeviceUpload;
            this.totalActualReaderUploaded = noReaderUpload;
            this.totalActualUploaded = noUpload;

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
                if (validType == "MasterMatchTempCompensationValidation")
                {
                    deviceAnalysisName = DeviceAnalysisName_MM;
                    break;
                }
                else if (validType == "TotalTempCompensationBySnValidation")
                {
                    deviceAnalysisName = DeviceAnalysisName_MTC;
                    break;
                }
                else if (validType == "SensCoefficientMatchTempCompensationValidation")
                {
                    deviceAnalysisName = DeviceAnalysisName_MSC;
                    break;
                }               
            }
        }

        return deviceAnalysisName;
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
   
    private bool isMasterRowBlank(DataRow pMasterRow)
    {
        try
        {
            if (string.IsNullOrEmpty(pMasterRow["sn"].ToString()) && string.IsNullOrEmpty(pMasterRow["mfg_date"].ToString())
                        && string.IsNullOrEmpty(pMasterRow["hw_version"].ToString()) 
                        && string.IsNullOrEmpty(pMasterRow["LOT/BOX"].ToString()) && string.IsNullOrEmpty(pMasterRow["PURCHASE ORDER"].ToString())
                        && string.IsNullOrEmpty(pMasterRow["INVOICE"].ToString()) && string.IsNullOrEmpty(pMasterRow["NOTE"].ToString())
                        && string.IsNullOrEmpty(pMasterRow["InvoiceDate"].ToString())
                        && string.IsNullOrEmpty(pMasterRow["ReadVolt0"].ToString()) && string.IsNullOrEmpty(pMasterRow["ReadVolt1"].ToString())
                        && string.IsNullOrEmpty(pMasterRow["ReadVolt2"].ToString()) && string.IsNullOrEmpty(pMasterRow["ReadVolt3"].ToString())
                        && string.IsNullOrEmpty(pMasterRow["ReadVolt4"].ToString()) && string.IsNullOrEmpty(pMasterRow["ReadVolt5"].ToString())
                        && string.IsNullOrEmpty(pMasterRow["ReadVolt6"].ToString()) && string.IsNullOrEmpty(pMasterRow["ReadVolt7"].ToString())
                        && string.IsNullOrEmpty(pMasterRow["BDAddress"].ToString()) && string.IsNullOrEmpty(pMasterRow["PanelID"].ToString())
                        && string.IsNullOrEmpty(pMasterRow["Position"].ToString())
                )
            {
                return true;
            }
        }
        catch
        { }
        return false;
    }

    private bool isTempCompensationRowBlank(DataRow pTempCompensationRow)
    {
        try
        {
            if (string.IsNullOrEmpty(pTempCompensationRow["sn"].ToString()) && string.IsNullOrEmpty(pTempCompensationRow["Temp_id"].ToString())
                   && string.IsNullOrEmpty(pTempCompensationRow["DL_R'"].ToString()) && string.IsNullOrEmpty(pTempCompensationRow["DL_Tref"].ToString())
                   && string.IsNullOrEmpty(pTempCompensationRow["DL_K1"].ToString()) && string.IsNullOrEmpty(pTempCompensationRow["DL_K2"].ToString())
                   && string.IsNullOrEmpty(pTempCompensationRow["DH_R'"].ToString()) && string.IsNullOrEmpty(pTempCompensationRow["DH_Tref"].ToString())
                   && string.IsNullOrEmpty(pTempCompensationRow["DH_K1"].ToString()) && string.IsNullOrEmpty(pTempCompensationRow["DH_K2"].ToString())
                   && string.IsNullOrEmpty(pTempCompensationRow["SL_R'"].ToString()) && string.IsNullOrEmpty(pTempCompensationRow["SL_Tref"].ToString())
                   && string.IsNullOrEmpty(pTempCompensationRow["SL_K1"].ToString()) && string.IsNullOrEmpty(pTempCompensationRow["SL_K2"].ToString())
                   && string.IsNullOrEmpty(pTempCompensationRow["SH_R'"].ToString()) && string.IsNullOrEmpty(pTempCompensationRow["SH_Tref"].ToString())
                   && string.IsNullOrEmpty(pTempCompensationRow["SH_K1"].ToString()) && string.IsNullOrEmpty(pTempCompensationRow["SH_K2"].ToString()) 
                   && string.IsNullOrEmpty(pTempCompensationRow["Q"].ToString())
                )
            {
                return true;
            }
        }
        catch
        { }
        return false;
    }

    private bool isSensCoefficientRowBlank(DataRow pSensCoefficientRow)
    {
        try
        {
            if (string.IsNullOrEmpty(pSensCoefficientRow["sn"].ToString()) && string.IsNullOrEmpty(pSensCoefficientRow["DL_S2"].ToString())
                   && string.IsNullOrEmpty(pSensCoefficientRow["DL_S1"].ToString()) && string.IsNullOrEmpty(pSensCoefficientRow["DL_S0"].ToString())
                   && string.IsNullOrEmpty(pSensCoefficientRow["DH_S2"].ToString()) && string.IsNullOrEmpty(pSensCoefficientRow["DH_S1"].ToString())
                   && string.IsNullOrEmpty(pSensCoefficientRow["DH_S0"].ToString()) && string.IsNullOrEmpty(pSensCoefficientRow["SL_S2"].ToString())
                   && string.IsNullOrEmpty(pSensCoefficientRow["SL_S1"].ToString()) && string.IsNullOrEmpty(pSensCoefficientRow["SL_S0"].ToString())
                   && string.IsNullOrEmpty(pSensCoefficientRow["SH_S2"].ToString()) && string.IsNullOrEmpty(pSensCoefficientRow["SH_S1"].ToString())
                   && string.IsNullOrEmpty(pSensCoefficientRow["SH_S0"].ToString()) 
                )
            {
                return true;
            }
        }
        catch
        { }
        return false;
    }

    private bool isReaderRowBlank(DataRow pReaderRow)
    {
        try
        {
            if (string.IsNullOrEmpty(pReaderRow["sn"].ToString()) && string.IsNullOrEmpty(pReaderRow["mfg_date"].ToString())
                        && string.IsNullOrEmpty(pReaderRow["hw_version"].ToString())
                        && string.IsNullOrEmpty(pReaderRow["LOT/BOX"].ToString()) && string.IsNullOrEmpty(pReaderRow["PURCHASE ORDER"].ToString())
                        && string.IsNullOrEmpty(pReaderRow["INVOICE"].ToString()) && string.IsNullOrEmpty(pReaderRow["NOTE"].ToString())
                        && string.IsNullOrEmpty(pReaderRow["InvoiceDate"].ToString()) && string.IsNullOrEmpty(pReaderRow["TYPE"].ToString()))
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
                    || string.IsNullOrEmpty(pMasterRow["hw_version"].ToString()) 
                    || string.IsNullOrEmpty(pMasterRow["LOT/BOX"].ToString()) || string.IsNullOrEmpty(pMasterRow["PURCHASE ORDER"].ToString())
                    || string.IsNullOrEmpty(pMasterRow["INVOICE"].ToString()) || string.IsNullOrEmpty(pMasterRow["NOTE"].ToString())
                    || string.IsNullOrEmpty(pMasterRow["InvoiceDate"].ToString())
                    || string.IsNullOrEmpty(pMasterRow["ReadVolt0"].ToString()) || string.IsNullOrEmpty(pMasterRow["ReadVolt1"].ToString())
                    || string.IsNullOrEmpty(pMasterRow["ReadVolt2"].ToString()) || string.IsNullOrEmpty(pMasterRow["ReadVolt3"].ToString())
                    || string.IsNullOrEmpty(pMasterRow["ReadVolt4"].ToString()) || string.IsNullOrEmpty(pMasterRow["ReadVolt5"].ToString())
                    || string.IsNullOrEmpty(pMasterRow["ReadVolt6"].ToString()) || string.IsNullOrEmpty(pMasterRow["ReadVolt7"].ToString()) )
            {
                return true;
            }
        }
        catch
        { }
        return false;
    }

    private bool isTempCompensationRowMissingData(DataRow pTempCompensationRow)
    {
        try
        {
            if (string.IsNullOrEmpty(pTempCompensationRow["sn"].ToString()) || string.IsNullOrEmpty(pTempCompensationRow["Temp_id"].ToString())
                    || string.IsNullOrEmpty(pTempCompensationRow["DL_R'"].ToString()) || string.IsNullOrEmpty(pTempCompensationRow["DL_Tref"].ToString())
                    || string.IsNullOrEmpty(pTempCompensationRow["DL_K1"].ToString()) || string.IsNullOrEmpty(pTempCompensationRow["DL_K2"].ToString())
                    || string.IsNullOrEmpty(pTempCompensationRow["DH_R'"].ToString()) || string.IsNullOrEmpty(pTempCompensationRow["DH_Tref"].ToString())
                    || string.IsNullOrEmpty(pTempCompensationRow["DH_K1"].ToString()) || string.IsNullOrEmpty(pTempCompensationRow["DH_K2"].ToString())
                    || string.IsNullOrEmpty(pTempCompensationRow["SL_R'"].ToString()) || string.IsNullOrEmpty(pTempCompensationRow["SL_Tref"].ToString())
                    || string.IsNullOrEmpty(pTempCompensationRow["SL_K1"].ToString()) || string.IsNullOrEmpty(pTempCompensationRow["SL_K2"].ToString())
                    || string.IsNullOrEmpty(pTempCompensationRow["SH_R'"].ToString()) || string.IsNullOrEmpty(pTempCompensationRow["SH_Tref"].ToString())
                    || string.IsNullOrEmpty(pTempCompensationRow["SH_K1"].ToString()) || string.IsNullOrEmpty(pTempCompensationRow["SH_K2"].ToString())
                    || string.IsNullOrEmpty(pTempCompensationRow["Q"].ToString())
                )
            {
                return true;
            }
        }            
        catch
        { }
        return false;
    }

    private bool isSensCoefficientRowMissingData(DataRow pSensCoefficientRow)
    {
        try
        {
            if (string.IsNullOrEmpty(pSensCoefficientRow["sn"].ToString()) || string.IsNullOrEmpty(pSensCoefficientRow["DL_S2"].ToString())
                    || string.IsNullOrEmpty(pSensCoefficientRow["DL_S1"].ToString()) || string.IsNullOrEmpty(pSensCoefficientRow["DL_S0"].ToString())
                    || string.IsNullOrEmpty(pSensCoefficientRow["DH_S2"].ToString()) || string.IsNullOrEmpty(pSensCoefficientRow["DH_S1"].ToString())
                    || string.IsNullOrEmpty(pSensCoefficientRow["DH_S0"].ToString()) || string.IsNullOrEmpty(pSensCoefficientRow["SL_S2"].ToString())
                    || string.IsNullOrEmpty(pSensCoefficientRow["SL_S1"].ToString()) || string.IsNullOrEmpty(pSensCoefficientRow["SL_S0"].ToString())
                    || string.IsNullOrEmpty(pSensCoefficientRow["SH_S2"].ToString()) || string.IsNullOrEmpty(pSensCoefficientRow["SH_S1"].ToString())
                    || string.IsNullOrEmpty(pSensCoefficientRow["SH_S0"].ToString()) 
                )
            {
                return true;
            }
        }
        catch
        { }
        return false;
    }

    private bool isReaderRowMissingData(DataRow pReaderRow)
    {
        try
        {
            if (string.IsNullOrEmpty(pReaderRow["sn"].ToString()) || string.IsNullOrEmpty(pReaderRow["mfg_date"].ToString())
                    || string.IsNullOrEmpty(pReaderRow["hw_version"].ToString())
                    || string.IsNullOrEmpty(pReaderRow["LOT/BOX"].ToString()) || string.IsNullOrEmpty(pReaderRow["PURCHASE ORDER"].ToString())
                    || string.IsNullOrEmpty(pReaderRow["INVOICE"].ToString()) || string.IsNullOrEmpty(pReaderRow["NOTE"].ToString())
                    || string.IsNullOrEmpty(pReaderRow["InvoiceDate"].ToString()) || string.IsNullOrEmpty(pReaderRow["TYPE"].ToString()))
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

    private string TotalUploadTotalReceiveNotInSynched(DataTable pValidationErrorDT)
    {
        foreach (DataRow error in pValidationErrorDT.Rows)
        {
            bool uploadAnyway = (bool)error["Commit"];
            string sn = error["Sn"].ToString().Trim();
            string validType = error["ValidationType"].ToString();
            if (validType == "TotalColorValidation" && uploadAnyway)
            {
                return "Yes";
            }
        }

        return "No";
    }

    private string TotalBackOrderTotalUploadNotInSynched(DataTable pValidationErrorDT)
    {
        foreach (DataRow error in pValidationErrorDT.Rows)
        {
            bool uploadAnyway = (bool)error["Commit"];
            string sn = error["Sn"].ToString().Trim();
            string validType = error["ValidationType"].ToString();
            if (validType == "TotalBackOrderValidation" && uploadAnyway)
            {
                return "Yes";
            }
        }
        return "No";
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