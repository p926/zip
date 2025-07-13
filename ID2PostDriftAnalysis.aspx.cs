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
    public partial class ID2PostDriftAnalysis : System.Web.UI.Page
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
                    string groupName = (from r in idc.DeviceGroups
                                        where r.DeviceGroupID == DeviceGroupID
                                        select r.DeviceGroupName).FirstOrDefault();

                    this.Header.InnerText = "Post Drift Pass/Fail Criteria - Group " + groupName;

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

                    grdPostDriftAnalysisView.DataSource = GetPostDriftAnalysisReport(AnalysisType, DeviceGroupID);
                    grdPostDriftAnalysisView.DataBind();

                    DisplayStatisticNumber();
                }

            }
            catch { }
        }

        private DataTable GetPostDriftAnalysisReport(string pAnalysisType, int pDeviceGroupID)
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
                    double DL_Drift_Avg, DL_Drift_STDEV, SL_Drift_Avg, SL_Drift_STDEV;
                    GetAvgAndStandardDeviationValues(dt, out DL_Drift_Avg, out DL_Drift_STDEV, out SL_Drift_Avg, out SL_Drift_STDEV);

                    // Display avg and standard deviation values for DL Drift & SL Drift
                    this.txtDL_Drift_Avg.Text = DL_Drift_Avg.ToString();
                    this.txtDL_Drift_STDEV.Text = DL_Drift_STDEV.ToString();
                    this.txtDL_Drift_2STDEV.Text = Math.Round(DL_Drift_STDEV * 2, 2).ToString();
                    this.txtSL_Drift_Avg.Text = SL_Drift_Avg.ToString();
                    this.txtSL_Drift_STDEV.Text = SL_Drift_STDEV.ToString();
                    this.txtSL_Drift_2STDEV.Text = Math.Round(SL_Drift_STDEV * 2, 2).ToString();

                    // -------------------------------------------- Populate the DataTable ------------------------------------------------//

                    DataTable myDT;

                    myDT = new DataTable();
                    myDT.Columns.Add("SerialNo", typeof(string));
                    myDT.Columns.Add("Exposure Date", typeof(string));
                    myDT.Columns.Add("DLDCalc Drift", typeof(string));
                    myDT.Columns.Add("DLDCalc Cal", typeof(string));
                    myDT.Columns.Add("SLDCalc Drift", typeof(string));
                    myDT.Columns.Add("SLDCalc Cal", typeof(string));
                    myDT.Columns.Add("DL Drift", typeof(string));
                    myDT.Columns.Add("SL Drift", typeof(string));
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
                            myDT.Rows.Add(GenerateDataRow(myDT, curDiviceID, curSerialNo, DL_Drift_Avg, DL_Drift_STDEV, SL_Drift_Avg, SL_Drift_STDEV, dt.Rows[iter], dt.Rows[iter + 1]));
                            iter += 2;
                        }
                        else  // missing read for current serial number
                        {
                            // Add the row to the DataTable.

                            // Identify what read type is missing
                            if (!string.IsNullOrEmpty(dt.Rows[iter]["ReadTypeName"].ToString()))
                            {
                                if (dt.Rows[iter]["ReadTypeName"].ToString() == "Calibration")       // missing Post Drift read                 
                                    myDT.Rows.Add(GenerateDataRow(myDT, curDiviceID, curSerialNo, DL_Drift_Avg, DL_Drift_STDEV, SL_Drift_Avg, SL_Drift_STDEV, dt.Rows[iter], null));
                                else //(dt.Rows[iter]["ReadTypeName"].ToString() == "Post Drift")    // missing Calibration read
                                    myDT.Rows.Add(GenerateDataRow(myDT, curDiviceID, curSerialNo, DL_Drift_Avg, DL_Drift_STDEV, SL_Drift_Avg, SL_Drift_STDEV, null, dt.Rows[iter]));
                            }
                            else // missing both calib and post drift reads
                            {
                                myDT.Rows.Add(GenerateDataRow(myDT, curDiviceID, curSerialNo, DL_Drift_Avg, DL_Drift_STDEV, SL_Drift_Avg, SL_Drift_STDEV, null, null));
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

        private void GetAvgAndStandardDeviationValues(DataTable pDt, out double pDL_Drift_Avg, out double pDL_Drift_STDEV, out double pSL_Drift_Avg, out double pSL_Drift_STDEV)
        {
            try
            {
                List<double> DLDriftList = new List<double>();
                List<double> SLDriftList = new List<double>();
                DataRow curCalibReadRow, curDriftReadRow;
                double? curDLDCalc_Drift, curDLDCalc_Cal, curSLDCalc_Drift, curSLDCalc_Cal;
                DateTime? driftExposureDate, driftScheduledTime, calibExposureDate, calibScheduledTime;
                double curDL_Drift, curSL_Drift;
                double? myDoubleNull = null;
                DateTime? myDateTimeNull = null;

                string nextSerialNo = "";
                string curSerialNo = "";
                int iter = 0;
                int rowNo = pDt.Rows.Count;

                // go through dataset to establish DLDriftList and SLDriftList
                while (iter < rowNo)
                {
                    curSerialNo = pDt.Rows[iter]["SerialNo"].ToString();
                    nextSerialNo = iter + 1 < rowNo ? pDt.Rows[iter + 1]["SerialNo"].ToString() : "";

                    // Add DL Drift and SL Drift to list.  Do not include it to avg and standard deviation formula if incorrect read dates
                    if (curSerialNo == nextSerialNo)   // calibration and post drift are present. 
                    {
                        curCalibReadRow = pDt.Rows[iter];
                        curDriftReadRow = pDt.Rows[iter + 1];

                        curDLDCalc_Drift = (curDriftReadRow["DeepLowDoseCalc"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(curDriftReadRow["DeepLowDoseCalc"]), 2) : myDoubleNull;
                        curDLDCalc_Cal = (curCalibReadRow["DeepLowDoseCalc"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(curCalibReadRow["DeepLowDoseCalc"]), 2) : myDoubleNull; ;
                        curSLDCalc_Drift = (curDriftReadRow["ShallowLowDoseCalc"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(curDriftReadRow["ShallowLowDoseCalc"]), 2) : myDoubleNull;
                        curSLDCalc_Cal = (curCalibReadRow["ShallowLowDoseCalc"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(curCalibReadRow["ShallowLowDoseCalc"]), 2) : myDoubleNull;

                        driftExposureDate = (curDriftReadRow["ExposureDate"] != System.DBNull.Value) ? Convert.ToDateTime(Convert.ToDateTime(curDriftReadRow["ExposureDate"]).ToShortDateString()) : myDateTimeNull;
                        driftScheduledTime = (curDriftReadRow["ScheduledTime"] != System.DBNull.Value) ? Convert.ToDateTime(Convert.ToDateTime(curDriftReadRow["ScheduledTime"]).ToShortDateString()) : myDateTimeNull;
                        calibExposureDate = (curCalibReadRow["ExposureDate"] != System.DBNull.Value) ? Convert.ToDateTime(Convert.ToDateTime(curCalibReadRow["ExposureDate"]).ToShortDateString()) : myDateTimeNull;
                        calibScheduledTime = (curCalibReadRow["ScheduledTime"] != System.DBNull.Value) ? Convert.ToDateTime(Convert.ToDateTime(curCalibReadRow["ScheduledTime"]).ToShortDateString()) : myDateTimeNull;

                        // Only include DL Drift and SL Drift to a list to calculate the avg and standard deviation if read date is matched with scheduled date.
                        if (driftExposureDate.HasValue & driftScheduledTime.HasValue & calibExposureDate.HasValue & calibScheduledTime.HasValue)
                        {
                            if ((driftExposureDate.Value == driftScheduledTime.Value) && (calibExposureDate.Value == calibScheduledTime.Value))
                            {
                                if (curDLDCalc_Drift.HasValue & curDLDCalc_Cal.HasValue)
                                {
                                    curDL_Drift = Math.Round(curDLDCalc_Drift.Value - curDLDCalc_Cal.Value, 2);
                                    DLDriftList.Add(curDL_Drift);
                                }

                                if (curSLDCalc_Drift.HasValue & curSLDCalc_Cal.HasValue)
                                {
                                    curSL_Drift = Math.Round(curSLDCalc_Drift.Value - curSLDCalc_Cal.Value, 2);
                                    SLDriftList.Add(curSL_Drift);
                                }
                            }
                        }     
                      
                        iter += 2;
                    }
                    else  // missing some reads (calibration or post drift) for current serial number. This sn is missing reads. Do not include it to avg and standard deviation formula
                    {
                        iter += 1;
                    }
                }

                // Calculate avg and standard deviation for DL Drift and SL Drift
                double DL_Drift_Avg = DLDriftList.Average();
                double sumOfSquaresOfDifferencesDL = DLDriftList.Select(val => (val - DL_Drift_Avg) * (val - DL_Drift_Avg)).Sum();
                double DL_Drift_STDEV = Math.Sqrt(sumOfSquaresOfDifferencesDL / (DLDriftList.Count-1));

                double SL_Drift_Avg = SLDriftList.Average();
                double sumOfSquaresOfDifferencesSL = SLDriftList.Select(val => (val - SL_Drift_Avg) * (val - SL_Drift_Avg)).Sum();
                double SL_Drift_STDEV = Math.Sqrt(sumOfSquaresOfDifferencesSL / (SLDriftList.Count-1));

                pDL_Drift_Avg = Math.Round(DL_Drift_Avg, 2);
                pDL_Drift_STDEV = Math.Round(DL_Drift_STDEV, 2);
                pSL_Drift_Avg = Math.Round(SL_Drift_Avg, 2);
                pSL_Drift_STDEV = Math.Round(SL_Drift_STDEV, 2);
            }
            catch (Exception)
            {
                throw;
            }

        }

        /// <summary>
        /// Generate a data row to display in analysis screen
        /// </summary>
        /// <param name="pDT">Data table structure</param>
        /// <param name="pDeviceID"></param>
        /// <param name="pSerialNo">Serial No</param>
        /// <param name="pDL_Drift_Avg">Deep Low Drift Avg</param>
        /// <param name="pDL_Drift_STDEV">Deep Low Drift Standard Deviation</param>
        /// <param name="pSL_Drift_Avg">Shallow Low Drift Avg</param>
        /// <param name="pSL_Drift_STDEV">Shallow Low Drift Standard Deviation</param>
        /// <param name="pCalibReadRow">Calibration Read row</param>
        /// <param name="pDriftReadRow">Post Drift Read row</param>
        /// <returns></returns>    
        private DataRow GenerateDataRow(DataTable pDT, int pDeviceID, string pSerialNo, double pDL_Drift_Avg, double pDL_Drift_STDEV, double pSL_Drift_Avg, double pSL_Drift_STDEV, DataRow pCalibReadRow, DataRow pDriftReadRow)
        {
            try
            {
                double DLDrift_LR, DLDrift_HR, SLDrift_LR, SLDrift_HR;
                double DL_Drift_Avg, DL_Drift_STDEV, DL_Drift_2STDEV, SL_Drift_Avg, SL_Drift_STDEV, SL_Drift_2STDEV;

                DL_Drift_Avg = pDL_Drift_Avg;
                SL_Drift_Avg = pSL_Drift_Avg;
                DL_Drift_STDEV = pDL_Drift_STDEV;
                SL_Drift_STDEV = pSL_Drift_STDEV;

                DL_Drift_2STDEV = 2 * DL_Drift_STDEV;
                SL_Drift_2STDEV = 2 * SL_Drift_STDEV;

                DLDrift_LR = DL_Drift_Avg - DL_Drift_2STDEV;
                DLDrift_HR = DL_Drift_Avg + DL_Drift_2STDEV;
                SLDrift_LR = SL_Drift_Avg - SL_Drift_2STDEV;
                SLDrift_HR = SL_Drift_Avg + SL_Drift_2STDEV;

                double? DLDCalc_Drift, DLDCalc_Cal, SLDCalc_Drift, SLDCalc_Cal, DL_Drift, SL_Drift;
                DateTime? driftExposureDate, driftScheduledTime, calibExposureDate, calibScheduledTime;
                string myPassFail = "pass";
                string myOutlier = "N";

                DateTime? myDateTimeNull = null;
                double? myDoubleNull = null;

                DataRow dtr = pDT.NewRow();

                if (pCalibReadRow != null & pDriftReadRow != null)
                {
                    DLDCalc_Drift = (pDriftReadRow["DeepLowDoseCalc"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pDriftReadRow["DeepLowDoseCalc"]), 2) : myDoubleNull;
                    DLDCalc_Cal = (pCalibReadRow["DeepLowDoseCalc"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pCalibReadRow["DeepLowDoseCalc"]), 2) : myDoubleNull;
                    SLDCalc_Drift = (pDriftReadRow["ShallowLowDoseCalc"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pDriftReadRow["ShallowLowDoseCalc"]), 2) : myDoubleNull;
                    SLDCalc_Cal = (pCalibReadRow["ShallowLowDoseCalc"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pCalibReadRow["ShallowLowDoseCalc"]), 2) : myDoubleNull;
                    driftExposureDate = (pDriftReadRow["ExposureDate"] != System.DBNull.Value) ? Convert.ToDateTime(Convert.ToDateTime(pDriftReadRow["ExposureDate"]).ToShortDateString()) : myDateTimeNull;
                    driftScheduledTime = (pDriftReadRow["ScheduledTime"] != System.DBNull.Value) ? Convert.ToDateTime(Convert.ToDateTime(pDriftReadRow["ScheduledTime"]).ToShortDateString()) : myDateTimeNull;
                    calibExposureDate = (pCalibReadRow["ExposureDate"] != System.DBNull.Value) ? Convert.ToDateTime(Convert.ToDateTime(pCalibReadRow["ExposureDate"]).ToShortDateString()) : myDateTimeNull;
                    calibScheduledTime = (pCalibReadRow["ScheduledTime"] != System.DBNull.Value) ? Convert.ToDateTime(Convert.ToDateTime(pCalibReadRow["ScheduledTime"]).ToShortDateString()) : myDateTimeNull;

                    if (DLDCalc_Cal.HasValue & DLDCalc_Drift.HasValue & SLDCalc_Cal.HasValue & SLDCalc_Drift.HasValue)
                    {
                        DL_Drift = Math.Round(DLDCalc_Drift.Value - DLDCalc_Cal.Value, 2);
                        SL_Drift = Math.Round(SLDCalc_Drift.Value - SLDCalc_Cal.Value, 2);

                        if (DL_Drift < DLDrift_LR || DL_Drift > DLDrift_HR || SL_Drift < SLDrift_LR || SL_Drift > SLDrift_HR)
                        {
                            myPassFail = "fail";
                        }
                    }
                    else
                    {
                        DL_Drift = null;
                        SL_Drift = null;

                        if (DLDCalc_Cal.HasValue & DLDCalc_Drift.HasValue)
                            DL_Drift = Math.Round(DLDCalc_Drift.Value - DLDCalc_Cal.Value, 2);

                        if (SLDCalc_Cal.HasValue & SLDCalc_Drift.HasValue)
                            SL_Drift = Math.Round(SLDCalc_Drift.Value - SLDCalc_Cal.Value, 2);

                        myPassFail = "processing";
                        myOutlier = "Y(MR)";
                    }

                    // Check ExposureDate & ScheduledDate does not match
                    if (myOutlier == "N")
                    {
                        if (driftExposureDate.HasValue & driftScheduledTime.HasValue & calibExposureDate.HasValue & calibScheduledTime.HasValue)
                        {
                            if ((driftExposureDate.Value != driftScheduledTime.Value) || (calibExposureDate.Value != calibScheduledTime.Value))
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
                    dtr["Exposure Date"] = pDriftReadRow["ExposureDate"];
                    dtr["DLDCalc Drift"] = DLDCalc_Drift.HasValue ? DLDCalc_Drift.Value.ToString() : "";
                    dtr["DLDCalc Cal"] = DLDCalc_Cal.HasValue ? DLDCalc_Cal.Value.ToString() : "";
                    dtr["SLDCalc Drift"] = SLDCalc_Drift.HasValue ? SLDCalc_Drift.Value.ToString() : "";
                    dtr["SLDCalc Cal"] = SLDCalc_Cal.HasValue ? SLDCalc_Cal.Value.ToString() : "";
                    dtr["DL Drift"] = DL_Drift.HasValue ? DL_Drift.Value.ToString() : "";
                    dtr["SL Drift"] = SL_Drift.HasValue ? SL_Drift.Value.ToString() : "";
                    dtr["Pass/Fail"] = myPassFail;
                    dtr["Outliers(Y/N)"] = myOutlier;
                }
                else if (pCalibReadRow != null & pDriftReadRow == null)
                {
                    DLDCalc_Cal = (pCalibReadRow["DeepLowDoseCalc"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pCalibReadRow["DeepLowDoseCalc"]), 2) : myDoubleNull;
                    SLDCalc_Cal = (pCalibReadRow["ShallowLowDoseCalc"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pCalibReadRow["ShallowLowDoseCalc"]), 2) : myDoubleNull;

                    myPassFail = "processing";

                    // Missing reads
                    myOutlier = "Y(MR)";

                    dtr["SerialNo"] = pSerialNo;
                    dtr["Exposure Date"] = "";
                    dtr["DLDCalc Drift"] = "";
                    dtr["DLDCalc Cal"] = DLDCalc_Cal.HasValue ? DLDCalc_Cal.Value.ToString() : "";
                    dtr["SLDCalc Drift"] = "";
                    dtr["SLDCalc Cal"] = SLDCalc_Cal.HasValue ? SLDCalc_Cal.Value.ToString() : "";
                    dtr["DL Drift"] = "";
                    dtr["SL Drift"] = "";
                    dtr["Pass/Fail"] = myPassFail;
                    dtr["Outliers(Y/N)"] = myOutlier;
                }
                else if (pCalibReadRow == null & pDriftReadRow != null)
                {
                    DLDCalc_Drift = (pDriftReadRow["DeepLowDoseCalc"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pDriftReadRow["DeepLowDoseCalc"]), 2) : myDoubleNull;
                    SLDCalc_Drift = (pDriftReadRow["ShallowLowDoseCalc"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pDriftReadRow["ShallowLowDoseCalc"]), 2) : myDoubleNull;

                    myPassFail = "processing";

                    // Missing reads
                    myOutlier = "Y(MR)";

                    dtr["SerialNo"] = pSerialNo;
                    dtr["Exposure Date"] = "";
                    dtr["DLDCalc Drift"] = DLDCalc_Drift.HasValue ? DLDCalc_Drift.Value.ToString() : "";
                    dtr["DLDCalc Cal"] = "";
                    dtr["SLDCalc Drift"] = SLDCalc_Drift.HasValue ? SLDCalc_Drift.Value.ToString() : "";
                    dtr["SLDCalc Cal"] = "";
                    dtr["DL Drift"] = "";
                    dtr["SL Drift"] = "";
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
                    dtr["DLDCalc Drift"] = "";
                    dtr["DLDCalc Cal"] = "";
                    dtr["SLDCalc Drift"] = "";
                    dtr["SLDCalc Cal"] = "";
                    dtr["DL Drift"] = "";
                    dtr["SL Drift"] = "";
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

                // Already analyzed before then check
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
                            IsPassDrift = isPass
                        };
                        idc.ID2PassFails.InsertOnSubmit(myObj);
                        idc.SubmitChanges();
                    }
                    else
                    {
                        // Set pass/fail
                        myObj.IsPassDrift = isPass;
                        idc.SubmitChanges();
                    }
                }
                else
                {
                    ID2PassFail myObj = new ID2PassFail
                    {
                        DeviceGroupID = this.DeviceGroupID,
                        DeviceID = pDeviceID,
                        IsPassDrift = isPass
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
            //dtr["%Missing Read"] = Math.Round(Convert.ToDouble(missingReadNo) / Convert.ToDouble(deviceTotal) * 100, 2);
            // Add the row to the DataTable.
            myDT.Rows.Add(dtr);

            grdStatisticResultView.DataSource = myDT;
            grdStatisticResultView.DataBind();
        }

        protected void grdPostDriftAnalysisView_RowDataBound(object sender, GridViewRowEventArgs e)
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