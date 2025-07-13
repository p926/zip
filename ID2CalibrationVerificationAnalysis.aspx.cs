/*
 * Calibration Verification Analysis 
 * 
 *  Created By: Tdo, 03/12/2014
 *  Copyright (C) 2010 Mirion Technologies. All Rights Reserved.
 */

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

namespace portal_instadose_com_v3.TechOps
{
    public partial class ID2CalibrationVerificationAnalysis : System.Web.UI.Page
    {
        // Turn on/off email to all others
        private bool DevelopmentServer = false;
        private string AnalysisType = "";
        private int DeviceGroupID = 0;
        private int passNo;
        private int failNo;
        private int missingReadNo;
        private int incorrectReadDateNo;
        private InsDataContext idc = new InsDataContext();
        private bool existPassFailRecord;

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // Auto set if a development site
                if (Request.Url.Authority.ToString().Contains(".mirioncorp.com"))
                    DevelopmentServer = true;

                AnalysisType = Request.QueryString["AnalysisType"].ToString();
                int.TryParse(Request.QueryString["DeviceGroupID"].ToString(), out DeviceGroupID);

                if (AnalysisType == "" || DeviceGroupID == 0)
                {
                    Page.Response.Redirect("ID2Analysis.aspx");
                    return;
                }

                if (!Page.IsPostBack)
                {
                    this.txtDL_LR.Text = GetAppSettings("DLCalVerLR");
                    this.txtDL_HR.Text = GetAppSettings("DLCalVerHR");
                    this.txtSL_LR.Text = GetAppSettings("SLCalVerLR");
                    this.txtSL_HR.Text = GetAppSettings("SLCalVerHR"); ;

                    string groupName = (from r in idc.DeviceGroups
                                        where r.DeviceGroupID == DeviceGroupID
                                        select r.DeviceGroupName).FirstOrDefault();

                    this.Header.InnerText = "Calibration Verification Pass/Fail Criteria - Group " + groupName;

                    int count =
                    (from o in idc.ID2PassFails
                     where o.DeviceGroupID == DeviceGroupID
                     select o).Count();

                    if (count > 0)
                        existPassFailRecord = true;
                    else
                        existPassFailRecord = false;

                    passNo = 0;
                    failNo = 0;
                    missingReadNo = 0;
                    incorrectReadDateNo = 0;

                    grdCalibrationAnalysisView.DataSource = GetCalibrationAnalysisReport(AnalysisType, DeviceGroupID);
                    grdCalibrationAnalysisView.DataBind();

                    DisplayStatisticNumber();
                }
            }
            catch { }
        }

        private string GetAppSettings(string pAppKey)
        {
            var mySetting = (from AppSet in idc.AppSettings where AppSet.AppKey == pAppKey select AppSet).FirstOrDefault();
            return (mySetting != null) ? mySetting.Value : "";
        }

        private DataTable GetCalibrationAnalysisReport(string pAnalysisType, int pDeviceGroupID)
        {
            try
            {
                String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
                String cmdStr = "sp_GetReadDataForID2PassFailAnalysis";

                SqlConnection sqlConn = new SqlConnection(connStr);
                SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

                sqlConn.Open();

                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add("@AnalysisType", SqlDbType.NVarChar);
                sqlCmd.Parameters["@AnalysisType"].Value = pAnalysisType;

                sqlCmd.Parameters.Add("@DeviceGroupID", SqlDbType.Int);
                sqlCmd.Parameters["@DeviceGroupID"].Value = pDeviceGroupID;

                SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
                DataTable dt = new DataTable();

                sqlDA.Fill(dt);
                sqlConn.Close();

                if (dt.Rows.Count > 0)
                {
                    // -------------------------------------------- Populate the DataTable ------------------------------------------------//

                    DataTable myDT;

                    myDT = new DataTable();
                    myDT.Columns.Add("SerialNo", typeof(string));
                    myDT.Columns.Add("Exposure Date", typeof(string));
                    myDT.Columns.Add("DLDCalc Cal", typeof(string));
                    myDT.Columns.Add("DLDCalc Init", typeof(string));
                    myDT.Columns.Add("SLDCalc Cal", typeof(string));
                    myDT.Columns.Add("SLDCalc Init", typeof(string));
                    myDT.Columns.Add("DL Cal Ver", typeof(string));
                    myDT.Columns.Add("SL Cal Ver", typeof(string));
                    myDT.Columns.Add("Pass/Fail", typeof(string));
                    myDT.Columns.Add("Outliers(Y/N)", typeof(string));

                    string nextSerialNo = "";
                    string curSerialNo = "";
                    int curDiviceID;
                    int iter = 0;
                    int rowNo = dt.Rows.Count;

                    while (iter < rowNo)
                    {
                        curDiviceID = Convert.ToInt32(dt.Rows[iter]["DeviceID"]);
                        curSerialNo = dt.Rows[iter]["SerialNo"].ToString();
                        nextSerialNo = iter + 1 < rowNo ? dt.Rows[iter + 1]["SerialNo"].ToString() : "";

                        if (curSerialNo == nextSerialNo)
                        {
                            // Add the row to the DataTable.
                            myDT.Rows.Add(GenerateDataRow(myDT, curDiviceID, curSerialNo, dt.Rows[iter], dt.Rows[iter + 1]));
                            iter += 2;
                        }
                        else  // missing read for current serial number
                        {
                            // Add the row to the DataTable.

                            // Identify what read type is missing
                            if (!string.IsNullOrEmpty(dt.Rows[iter]["ReadTypeName"].ToString()))
                            {
                                if (dt.Rows[iter]["ReadTypeName"].ToString() == "Initial Read")       // missing calib read                 
                                    myDT.Rows.Add(GenerateDataRow(myDT, curDiviceID, curSerialNo, dt.Rows[iter], null));
                                else //(dt.Rows[iter]["ReadTypeName"].ToString() == "Calibration")    // missing init read
                                    myDT.Rows.Add(GenerateDataRow(myDT, curDiviceID, curSerialNo, null, dt.Rows[iter]));
                            }
                            else // missing both init and calib reads
                            {
                                myDT.Rows.Add(GenerateDataRow(myDT, curDiviceID, curSerialNo, null, null));
                            }

                            iter += 1;
                        }

                    }

                    // -------------------------------------------- Populate the DataTable ------------------------------------------------//

                    return myDT;
                }
                else
                {
                    return null;
                }

            }
            catch { return null; }
        }

        /// <summary>
        /// Generate a data row to display in analysis screen
        /// </summary>
        /// <param name="pDT">Data table</param>
        /// <param name="pDeviceID"></param>
        /// <param name="pInitReadRow">Initial Read row</param>
        /// <param name="pCalibReadRow">Calibration Read row</param>
        /// <returns></returns>    
        private DataRow GenerateDataRow(DataTable pDT, int pDeviceID, string pSerialNo, DataRow pInitReadRow, DataRow pCalibReadRow)
        {
            try
            {
                double DLLR = Convert.ToDouble(this.txtDL_LR.Text);
                double DLHR = Convert.ToDouble(this.txtDL_HR.Text);
                double SLLR = Convert.ToDouble(this.txtSL_LR.Text);
                double SLHR = Convert.ToDouble(this.txtSL_HR.Text);

                double? DLDCalc_Init, DLDCalc_Cal, SLDCalc_Init, SLDCalc_Cal;
                double? DL_Cal_Ver, SL_Cal_Ver;
                DateTime? initExposureDate, initScheduledTime, calibExposureDate, calibScheduledTime;
                string myPassFail = "pass";
                string myOutlier = "N";

                DateTime? myDateTimeNull = null;
                double? myDoubleNull = null;

                DataRow dtr = pDT.NewRow();

                if (pInitReadRow != null & pCalibReadRow != null)
                {
                    DLDCalc_Init = (pInitReadRow["DeepLowDoseCalc"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pInitReadRow["DeepLowDoseCalc"]), 2) : myDoubleNull;
                    DLDCalc_Cal = (pCalibReadRow["DeepLowDoseCalc"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pCalibReadRow["DeepLowDoseCalc"]), 2) : myDoubleNull;
                    SLDCalc_Init = (pInitReadRow["ShallowLowDoseCalc"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pInitReadRow["ShallowLowDoseCalc"]), 2) : myDoubleNull;
                    SLDCalc_Cal = (pCalibReadRow["ShallowLowDoseCalc"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pCalibReadRow["ShallowLowDoseCalc"]), 2) : myDoubleNull;
                    initExposureDate = (pInitReadRow["ExposureDate"] != System.DBNull.Value) ? Convert.ToDateTime(Convert.ToDateTime(pInitReadRow["ExposureDate"]).ToShortDateString()) : myDateTimeNull;
                    initScheduledTime = (pInitReadRow["ScheduledTime"] != System.DBNull.Value) ? Convert.ToDateTime(Convert.ToDateTime(pInitReadRow["ScheduledTime"]).ToShortDateString()) : myDateTimeNull;
                    calibExposureDate = (pCalibReadRow["ExposureDate"] != System.DBNull.Value) ? Convert.ToDateTime(Convert.ToDateTime(pCalibReadRow["ExposureDate"]).ToShortDateString()) : myDateTimeNull;
                    calibScheduledTime = (pCalibReadRow["ScheduledTime"] != System.DBNull.Value) ? Convert.ToDateTime(Convert.ToDateTime(pCalibReadRow["ScheduledTime"]).ToShortDateString()) : myDateTimeNull;

                    if (DLDCalc_Init.HasValue & DLDCalc_Cal.HasValue & SLDCalc_Init.HasValue & SLDCalc_Cal.HasValue)
                    {
                        DL_Cal_Ver = Math.Round(DLDCalc_Cal.Value - DLDCalc_Init.Value, 2);
                        SL_Cal_Ver = Math.Round(SLDCalc_Cal.Value - SLDCalc_Init.Value, 2);

                        if (DL_Cal_Ver < DLLR || DL_Cal_Ver > DLHR || SL_Cal_Ver < SLLR || SL_Cal_Ver > SLHR)
                        {
                            myPassFail = "fail";
                        }
                    }
                    else
                    {
                        DL_Cal_Ver = null;
                        SL_Cal_Ver = null;

                        if (DLDCalc_Init.HasValue & DLDCalc_Cal.HasValue)
                            DL_Cal_Ver = Math.Round(DLDCalc_Cal.Value - DLDCalc_Init.Value, 2);

                        if (SLDCalc_Init.HasValue & SLDCalc_Cal.HasValue)
                            SL_Cal_Ver = Math.Round(SLDCalc_Cal.Value - SLDCalc_Init.Value, 2);

                        myPassFail = "processing";
                        myOutlier = "Y(MR)";
                    }

                    // Check ExposureDate & ScheduledDate does not match
                    if (myOutlier == "N")
                    {
                        if (initExposureDate.HasValue & initScheduledTime.HasValue & calibExposureDate.HasValue & calibScheduledTime.HasValue)
                        {
                            if ((initExposureDate.Value != initScheduledTime.Value) || (calibExposureDate.Value != calibScheduledTime.Value))
                            {
                                myOutlier = "Y(MD)";
                                myPassFail = "mismatchdate";
                            }
                        }
                        else
                        {
                            myOutlier = "Y(MD)";
                            myPassFail = "mismatchdate";
                        }
                    }

                    dtr["SerialNo"] = pSerialNo;
                    dtr["Exposure Date"] = pCalibReadRow["ExposureDate"];
                    dtr["DLDCalc Cal"] = DLDCalc_Cal.HasValue ? DLDCalc_Cal.Value.ToString() : "";
                    dtr["DLDCalc Init"] = DLDCalc_Init.HasValue ? DLDCalc_Init.Value.ToString() : "";
                    dtr["SLDCalc Cal"] = SLDCalc_Cal.HasValue ? SLDCalc_Cal.Value.ToString() : "";
                    dtr["SLDCalc Init"] = SLDCalc_Init.HasValue ? SLDCalc_Init.Value.ToString() : "";
                    dtr["DL Cal Ver"] = DL_Cal_Ver.HasValue ? DL_Cal_Ver.Value.ToString() : "";
                    dtr["SL Cal Ver"] = SL_Cal_Ver.HasValue ? SL_Cal_Ver.Value.ToString() : "";
                    dtr["Pass/Fail"] = myPassFail;
                    dtr["Outliers(Y/N)"] = myOutlier;
                }
                else if (pInitReadRow != null & pCalibReadRow == null)
                {
                    DLDCalc_Init = (pInitReadRow["DeepLowDoseCalc"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pInitReadRow["DeepLowDoseCalc"]), 2) : myDoubleNull;
                    SLDCalc_Init = (pInitReadRow["ShallowLowDoseCalc"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pInitReadRow["ShallowLowDoseCalc"]), 2) : myDoubleNull;

                    myPassFail = "processing";

                    // Missing reads
                    myOutlier = "Y(MR)";

                    dtr["SerialNo"] = pSerialNo;
                    dtr["Exposure Date"] = "";
                    dtr["DLDCalc Cal"] = "";
                    dtr["DLDCalc Init"] = DLDCalc_Init.HasValue ? DLDCalc_Init.Value.ToString() : "";
                    dtr["SLDCalc Cal"] = "";
                    dtr["SLDCalc Init"] = SLDCalc_Init.HasValue ? SLDCalc_Init.Value.ToString() : "";
                    dtr["DL Cal Ver"] = "";
                    dtr["SL Cal Ver"] = "";
                    dtr["Pass/Fail"] = myPassFail;
                    dtr["Outliers(Y/N)"] = myOutlier;
                }
                else if (pInitReadRow == null & pCalibReadRow != null)
                {
                    DLDCalc_Cal = (pCalibReadRow["DeepLowDoseCalc"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pCalibReadRow["DeepLowDoseCalc"]), 2) : myDoubleNull;
                    SLDCalc_Cal = (pCalibReadRow["ShallowLowDoseCalc"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pCalibReadRow["ShallowLowDoseCalc"]), 2) : myDoubleNull;

                    myPassFail = "processing";

                    // Missing reads
                    myOutlier = "Y(MR)";

                    dtr["SerialNo"] = pSerialNo;
                    dtr["Exposure Date"] = pCalibReadRow["ExposureDate"];
                    dtr["DLDCalc Cal"] = DLDCalc_Cal.HasValue ? DLDCalc_Cal.Value.ToString() : "";
                    dtr["DLDCalc Init"] = "";
                    dtr["SLDCalc Cal"] = SLDCalc_Cal.HasValue ? SLDCalc_Cal.Value.ToString() : "";
                    dtr["SLDCalc Init"] = "";
                    dtr["DL Cal Ver"] = "";
                    dtr["SL Cal Ver"] = "";
                    dtr["Pass/Fail"] = myPassFail;
                    dtr["Outliers(Y/N)"] = myOutlier;
                }
                else
                {
                    myPassFail = "processing";

                    // Missing reads
                    myOutlier = "Y(MR)";

                    dtr["SerialNo"] = pSerialNo;
                    dtr["Exposure Date"] = "";
                    dtr["DLDCalc Cal"] = "";
                    dtr["DLDCalc Init"] = "";
                    dtr["SLDCalc Cal"] = "";
                    dtr["SLDCalc Init"] = "";
                    dtr["DL Cal Ver"] = "";
                    dtr["SL Cal Ver"] = "";
                    dtr["Pass/Fail"] = myPassFail;
                    dtr["Outliers(Y/N)"] = myOutlier;
                }

                // Count #pass, #fail, #missing, #NoMatchReadDate
                // if pass then isPass = true
                // if fail then iPass = false
                // if missing reads then isPass = null
                // if no match read date reads then isPass = null
                bool? isPass = null;

                switch (myPassFail)
                {
                    case "pass":
                        this.passNo++;
                        isPass = true;
                        break;
                    case "fail":
                        this.failNo++;
                        isPass = false;
                        break;
                    case "processing":
                        this.missingReadNo++;
                        break;
                    case "mismatchdate":
                        this.incorrectReadDateNo++;
                        break;
                    default:
                        break;
                }

                // If already analyzed before then check
                // if the record exists then update pass/fail
                // else then adding new record
                if (this.existPassFailRecord)
                {
                    // Determine if ID2PassFail exists or not
                    ID2PassFail myObj =
                        (from o in idc.ID2PassFails
                         where o.DeviceGroupID == this.DeviceGroupID && o.DeviceID == pDeviceID
                         select o).FirstOrDefault();

                    // If ID2PassFail doesn't exist
                    if (myObj == null)
                    {
                        myObj = new ID2PassFail
                        {
                            DeviceGroupID = this.DeviceGroupID,
                            DeviceID = pDeviceID,
                            IsPassCal = isPass
                        };
                        idc.ID2PassFails.InsertOnSubmit(myObj);
                        idc.SubmitChanges();
                    }
                    else
                    {
                        // Set pass/fail
                        myObj.IsPassCal = isPass;
                        idc.SubmitChanges();
                    }
                }
                else
                {
                    ID2PassFail myObj = new ID2PassFail
                    {
                        DeviceGroupID = this.DeviceGroupID,
                        DeviceID = pDeviceID,
                        IsPassCal = isPass
                    };
                    idc.ID2PassFails.InsertOnSubmit(myObj);
                    idc.SubmitChanges();
                }

                return dtr;
            }
            catch { return null; }
        }

        private void DisplayStatisticNumber()
        {
            DataTable myDT;
            DataRow dtr;

            myDT = new DataTable();
            myDT.Columns.Add("#Devices", typeof(string));
            myDT.Columns.Add("#Pass", typeof(string));
            myDT.Columns.Add("#Fail", typeof(string));
            myDT.Columns.Add("#Missing Read", typeof(string));
            myDT.Columns.Add("#Incorrect Read Date", typeof(string));
            myDT.Columns.Add("%Pass", typeof(string));
            myDT.Columns.Add("%Fail", typeof(string));
            //myDT.Columns.Add("%Missing Read", typeof(string));

            dtr = myDT.NewRow();

            int deviceTotal = passNo + failNo + missingReadNo + incorrectReadDateNo;
            int passFailTotal = passNo + failNo;

            dtr["#Devices"] = deviceTotal;
            dtr["#Pass"] = passNo;
            dtr["#Fail"] = failNo;
            dtr["#Missing Read"] = missingReadNo;
            dtr["#Incorrect Read Date"] = incorrectReadDateNo;
            dtr["%Pass"] = (passFailTotal > 0) ? Math.Round(Convert.ToDouble(passNo) / Convert.ToDouble(passFailTotal) * 100, 2).ToString() : "";
            dtr["%Fail"] = (passFailTotal > 0) ? Math.Round(Convert.ToDouble(failNo) / Convert.ToDouble(passFailTotal) * 100, 2).ToString() : "";
            //dtr["%Missing Read"] = Math.Round (Convert.ToDouble(missingReadNo) / Convert.ToDouble(deviceTotal) * 100, 2);
            // Add the row to the DataTable.
            myDT.Rows.Add(dtr);

            grdStatisticResultView.DataSource = myDT;
            grdStatisticResultView.DataBind();
        }

        protected void grdCalibrationAnalysisView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                switch (e.Row.Cells[8].Text.ToLower())
                {
                    case "pass":
                        e.Row.Cells[8].CssClass = "pass";
                        e.Row.Cells[8].Text = "";
                        break;
                    case "fail":
                        e.Row.Cells[8].CssClass = "fail";
                        e.Row.Cells[8].Text = "";
                        break;
                    case "processing":
                        e.Row.Cells[8].CssClass = "processing";
                        e.Row.Cells[8].Text = "";
                        break;
                    case "mismatchdate":
                        e.Row.Cells[8].CssClass = "mismatchdate";
                        e.Row.Cells[8].Text = "";
                        break;
                    default:
                        e.Row.Cells[8].CssClass = "processing";
                        e.Row.Cells[8].Text = "";
                        break;
                }

                switch (e.Row.Cells[9].Text)
                {
                    case "N":
                        e.Row.Cells[9].CssClass = "no";
                        break;
                    default:
                        e.Row.Cells[9].CssClass = "yes";
                        break;
                }
            }
        }

        protected void btnBack_Click(object sender, EventArgs e)
        {
            Page.Response.Redirect("ID2Analysis.aspx");
        }
    }
}