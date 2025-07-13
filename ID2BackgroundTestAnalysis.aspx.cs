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
    public partial class ID2BackgroundTestAnalysis : System.Web.UI.Page
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
                    this.txtBkgd_1stP_LR.Text = GetAppSettings("DLBkgd1stPassLR");
                    this.txtBkgd_1stP_HR.Text = GetAppSettings("DLBkgd1stPassHR");
                    this.txtBkgd_2ndP_LR.Text = GetAppSettings("DLBkgd2ndPassLR");
                    this.txtBkgd_2ndP_HR.Text = GetAppSettings("DLBkgd2ndPassHR");

                    string groupName = (from r in idc.DeviceGroups
                                        where r.DeviceGroupID == DeviceGroupID
                                        select r.DeviceGroupName).FirstOrDefault();

                    this.Header.InnerText = "Background Test Pass/Fail Criteria - Group " + groupName;

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

                    grdBackgroundTestAnalysisView.DataSource = GetBackgroundTestAnalysisReport(AnalysisType, DeviceGroupID);
                    grdBackgroundTestAnalysisView.DataBind();

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

        private DataTable GetBackgroundTestAnalysisReport(string pAnalysisType, int pDeviceGroupID)
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
                    myDT.Columns.Add("1st Pass", typeof(string));
                    myDT.Columns.Add("2nd Pass", typeof(string));
                    myDT.Columns.Add("P/F", typeof(string));
                    myDT.Columns.Add("OL(Y/N)", typeof(string));

                    myDT.Columns.Add("DL_1st", typeof(string));
                    myDT.Columns.Add("SL_1st", typeof(string));

                    myDT.Columns.Add("DL2-1", typeof(string));
                    myDT.Columns.Add("SL2-1", typeof(string));
                    myDT.Columns.Add("DL3-2", typeof(string));
                    myDT.Columns.Add("SL3-2", typeof(string));
                    myDT.Columns.Add("DL4-3", typeof(string));
                    myDT.Columns.Add("SL4-3", typeof(string));
                    myDT.Columns.Add("DL5-4", typeof(string));
                    myDT.Columns.Add("SL5-4", typeof(string));
                    myDT.Columns.Add("DL6-5", typeof(string));
                    myDT.Columns.Add("SL6-5", typeof(string));
                    myDT.Columns.Add("DL7-6", typeof(string));
                    myDT.Columns.Add("SL7-6", typeof(string));
                    myDT.Columns.Add("DL8-7", typeof(string));
                    myDT.Columns.Add("SL8-7", typeof(string));

                    string snRead1, snRead2, snRead3, snRead4, snRead5, snRead6, snRead7, snRead8 = "";
                    int curDeviceID;
                    int iter = 0;   // jump to row of a dataset
                    int rowNo = dt.Rows.Count;
                    DataRow bgRead1, bgRead2, bgRead3, bgRead4, bgRead5, bgRead6, bgRead7, bgRead8;
                    DataRow bg1, bg2, bg3, bg4, bg5, bg6, bg7, bg8;

                    // Go through dataset and add the row to myDT DataTable.
                    while (iter < rowNo)
                    {
                        bgRead1 = null;
                        bgRead2 = null;
                        bgRead3 = null;
                        bgRead4 = null;
                        bgRead5 = null;
                        bgRead6 = null;
                        bgRead7 = null;
                        bgRead8 = null;
                        curDeviceID = 0;
                        snRead1 = "";
                        snRead2 = "";
                        snRead3 = "";
                        snRead4 = "";
                        snRead5 = "";
                        snRead6 = "";
                        snRead7 = "";
                        snRead8 = "";

                        curDeviceID = Convert.ToInt32(dt.Rows[iter]["DeviceID"]);
                        snRead1 = dt.Rows[iter]["SerialNo"].ToString();
                        bgRead1 = dt.Rows[iter];

                        if (iter + 1 < rowNo)
                        {
                            snRead2 = dt.Rows[iter + 1]["SerialNo"].ToString();
                            bgRead2 = dt.Rows[iter + 1];
                        }
                        if (iter + 2 < rowNo)
                        {
                            snRead3 = dt.Rows[iter + 2]["SerialNo"].ToString();
                            bgRead3 = dt.Rows[iter + 2];
                        }
                        if (iter + 3 < rowNo)
                        {
                            snRead4 = dt.Rows[iter + 3]["SerialNo"].ToString();
                            bgRead4 = dt.Rows[iter + 3];
                        }
                        if (iter + 4 < rowNo)
                        {
                            snRead5 = dt.Rows[iter + 4]["SerialNo"].ToString();
                            bgRead5 = dt.Rows[iter + 4];
                        }
                        if (iter + 5 < rowNo)
                        {
                            snRead6 = dt.Rows[iter + 5]["SerialNo"].ToString();
                            bgRead6 = dt.Rows[iter + 5];
                        }
                        if (iter + 6 < rowNo)
                        {
                            snRead7 = dt.Rows[iter + 6]["SerialNo"].ToString();
                            bgRead7 = dt.Rows[iter + 6];
                        }
                        if (iter + 7 < rowNo)
                        {
                            snRead8 = dt.Rows[iter + 7]["SerialNo"].ToString();
                            bgRead8 = dt.Rows[iter + 7];
                        }

                        // Have all required bg reads
                        if (snRead1 == snRead2 && snRead1 == snRead3 && snRead1 == snRead4 && snRead1 == snRead5 && snRead1 == snRead6 && snRead1 == snRead7 && snRead1 == snRead8)
                        {
                            myDT.Rows.Add(GenerateDataRow(myDT, curDeviceID, snRead1, bgRead1, bgRead2, bgRead3, bgRead4, bgRead5, bgRead6, bgRead7, bgRead8));
                            iter += 8;
                        }
                        else  // missing some reads for current serial number
                        {
                            if (string.IsNullOrEmpty(bgRead1["ReadTypeName"].ToString()))  // missing all bg reads
                            {
                                myDT.Rows.Add(GenerateDataRow(myDT, curDeviceID, snRead1, null, null, null, null, null, null, null, null));
                                iter += 1;
                            }
                            else // Identify what read type is missing
                            {
                                bg1 = null;
                                bg2 = null;
                                bg3 = null;
                                bg4 = null;
                                bg5 = null;
                                bg6 = null;
                                bg7 = null;
                                bg8 = null;

                                if (snRead1 == snRead2 && snRead1 == snRead3 && snRead1 == snRead4 && snRead1 == snRead5 && snRead1 == snRead6 && snRead1 == snRead7)
                                {
                                    // Missing 1 bg reads
                                    switch (bgRead1["ReadTypeName"].ToString())
                                    {
                                        case "Background Read 1":
                                            bg1 = bgRead1;
                                            break;
                                        case "Background Read 2":
                                            bg2 = bgRead1;
                                            break;
                                        case "Background Read 3":
                                            bg3 = bgRead1;
                                            break;
                                        case "Background Read 4":
                                            bg4 = bgRead1;
                                            break;
                                        case "Background Read 5":
                                            bg5 = bgRead1;
                                            break;
                                        case "Background Read 6":
                                            bg6 = bgRead1;
                                            break;
                                        case "Background Read 7":
                                            bg7 = bgRead1;
                                            break;
                                        case "Background Read 8":
                                            bg8 = bgRead1;
                                            break;
                                    }

                                    switch (bgRead2["ReadTypeName"].ToString())
                                    {
                                        case "Background Read 1":
                                            bg1 = bgRead2;
                                            break;
                                        case "Background Read 2":
                                            bg2 = bgRead2;
                                            break;
                                        case "Background Read 3":
                                            bg3 = bgRead2;
                                            break;
                                        case "Background Read 4":
                                            bg4 = bgRead2;
                                            break;
                                        case "Background Read 5":
                                            bg5 = bgRead2;
                                            break;
                                        case "Background Read 6":
                                            bg6 = bgRead2;
                                            break;
                                        case "Background Read 7":
                                            bg7 = bgRead2;
                                            break;
                                        case "Background Read 8":
                                            bg8 = bgRead2;
                                            break;
                                    }

                                    switch (bgRead3["ReadTypeName"].ToString())
                                    {
                                        case "Background Read 1":
                                            bg1 = bgRead3;
                                            break;
                                        case "Background Read 2":
                                            bg2 = bgRead3;
                                            break;
                                        case "Background Read 3":
                                            bg3 = bgRead3;
                                            break;
                                        case "Background Read 4":
                                            bg4 = bgRead3;
                                            break;
                                        case "Background Read 5":
                                            bg5 = bgRead3;
                                            break;
                                        case "Background Read 6":
                                            bg6 = bgRead3;
                                            break;
                                        case "Background Read 7":
                                            bg7 = bgRead3;
                                            break;
                                        case "Background Read 8":
                                            bg8 = bgRead3;
                                            break;
                                    }

                                    switch (bgRead4["ReadTypeName"].ToString())
                                    {
                                        case "Background Read 1":
                                            bg1 = bgRead4;
                                            break;
                                        case "Background Read 2":
                                            bg2 = bgRead4;
                                            break;
                                        case "Background Read 3":
                                            bg3 = bgRead4;
                                            break;
                                        case "Background Read 4":
                                            bg4 = bgRead4;
                                            break;
                                        case "Background Read 5":
                                            bg5 = bgRead4;
                                            break;
                                        case "Background Read 6":
                                            bg6 = bgRead4;
                                            break;
                                        case "Background Read 7":
                                            bg7 = bgRead4;
                                            break;
                                        case "Background Read 8":
                                            bg8 = bgRead4;
                                            break;
                                    }

                                    switch (bgRead5["ReadTypeName"].ToString())
                                    {
                                        case "Background Read 1":
                                            bg1 = bgRead5;
                                            break;
                                        case "Background Read 2":
                                            bg2 = bgRead5;
                                            break;
                                        case "Background Read 3":
                                            bg3 = bgRead5;
                                            break;
                                        case "Background Read 4":
                                            bg4 = bgRead5;
                                            break;
                                        case "Background Read 5":
                                            bg5 = bgRead5;
                                            break;
                                        case "Background Read 6":
                                            bg6 = bgRead5;
                                            break;
                                        case "Background Read 7":
                                            bg7 = bgRead5;
                                            break;
                                        case "Background Read 8":
                                            bg8 = bgRead5;
                                            break;
                                    }

                                    switch (bgRead6["ReadTypeName"].ToString())
                                    {
                                        case "Background Read 1":
                                            bg1 = bgRead6;
                                            break;
                                        case "Background Read 2":
                                            bg2 = bgRead6;
                                            break;
                                        case "Background Read 3":
                                            bg3 = bgRead6;
                                            break;
                                        case "Background Read 4":
                                            bg4 = bgRead6;
                                            break;
                                        case "Background Read 5":
                                            bg5 = bgRead6;
                                            break;
                                        case "Background Read 6":
                                            bg6 = bgRead6;
                                            break;
                                        case "Background Read 7":
                                            bg7 = bgRead6;
                                            break;
                                        case "Background Read 8":
                                            bg8 = bgRead6;
                                            break;
                                    }

                                    switch (bgRead7["ReadTypeName"].ToString())
                                    {
                                        case "Background Read 1":
                                            bg1 = bgRead7;
                                            break;
                                        case "Background Read 2":
                                            bg2 = bgRead7;
                                            break;
                                        case "Background Read 3":
                                            bg3 = bgRead7;
                                            break;
                                        case "Background Read 4":
                                            bg4 = bgRead7;
                                            break;
                                        case "Background Read 5":
                                            bg5 = bgRead7;
                                            break;
                                        case "Background Read 6":
                                            bg6 = bgRead7;
                                            break;
                                        case "Background Read 7":
                                            bg7 = bgRead7;
                                            break;
                                        case "Background Read 8":
                                            bg8 = bgRead7;
                                            break;
                                    }

                                    myDT.Rows.Add(GenerateDataRow(myDT, curDeviceID, snRead1, bg1, bg2, bg3, bg4, bg5, bg6, bg7, bg8));
                                    iter += 7;
                                }
                                else if (snRead1 == snRead2 && snRead1 == snRead3 && snRead1 == snRead4 && snRead1 == snRead5 && snRead1 == snRead6)
                                {
                                    // Missing 2 bg reads
                                    switch (bgRead1["ReadTypeName"].ToString())
                                    {
                                        case "Background Read 1":
                                            bg1 = bgRead1;
                                            break;
                                        case "Background Read 2":
                                            bg2 = bgRead1;
                                            break;
                                        case "Background Read 3":
                                            bg3 = bgRead1;
                                            break;
                                        case "Background Read 4":
                                            bg4 = bgRead1;
                                            break;
                                        case "Background Read 5":
                                            bg5 = bgRead1;
                                            break;
                                        case "Background Read 6":
                                            bg6 = bgRead1;
                                            break;
                                        case "Background Read 7":
                                            bg7 = bgRead1;
                                            break;
                                        case "Background Read 8":
                                            bg8 = bgRead1;
                                            break;
                                    }

                                    switch (bgRead2["ReadTypeName"].ToString())
                                    {
                                        case "Background Read 1":
                                            bg1 = bgRead2;
                                            break;
                                        case "Background Read 2":
                                            bg2 = bgRead2;
                                            break;
                                        case "Background Read 3":
                                            bg3 = bgRead2;
                                            break;
                                        case "Background Read 4":
                                            bg4 = bgRead2;
                                            break;
                                        case "Background Read 5":
                                            bg5 = bgRead2;
                                            break;
                                        case "Background Read 6":
                                            bg6 = bgRead2;
                                            break;
                                        case "Background Read 7":
                                            bg7 = bgRead2;
                                            break;
                                        case "Background Read 8":
                                            bg8 = bgRead2;
                                            break;
                                    }

                                    switch (bgRead3["ReadTypeName"].ToString())
                                    {
                                        case "Background Read 1":
                                            bg1 = bgRead3;
                                            break;
                                        case "Background Read 2":
                                            bg2 = bgRead3;
                                            break;
                                        case "Background Read 3":
                                            bg3 = bgRead3;
                                            break;
                                        case "Background Read 4":
                                            bg4 = bgRead3;
                                            break;
                                        case "Background Read 5":
                                            bg5 = bgRead3;
                                            break;
                                        case "Background Read 6":
                                            bg6 = bgRead3;
                                            break;
                                        case "Background Read 7":
                                            bg7 = bgRead3;
                                            break;
                                        case "Background Read 8":
                                            bg8 = bgRead3;
                                            break;
                                    }

                                    switch (bgRead4["ReadTypeName"].ToString())
                                    {
                                        case "Background Read 1":
                                            bg1 = bgRead4;
                                            break;
                                        case "Background Read 2":
                                            bg2 = bgRead4;
                                            break;
                                        case "Background Read 3":
                                            bg3 = bgRead4;
                                            break;
                                        case "Background Read 4":
                                            bg4 = bgRead4;
                                            break;
                                        case "Background Read 5":
                                            bg5 = bgRead4;
                                            break;
                                        case "Background Read 6":
                                            bg6 = bgRead4;
                                            break;
                                        case "Background Read 7":
                                            bg7 = bgRead4;
                                            break;
                                        case "Background Read 8":
                                            bg8 = bgRead4;
                                            break;
                                    }

                                    switch (bgRead5["ReadTypeName"].ToString())
                                    {
                                        case "Background Read 1":
                                            bg1 = bgRead5;
                                            break;
                                        case "Background Read 2":
                                            bg2 = bgRead5;
                                            break;
                                        case "Background Read 3":
                                            bg3 = bgRead5;
                                            break;
                                        case "Background Read 4":
                                            bg4 = bgRead5;
                                            break;
                                        case "Background Read 5":
                                            bg5 = bgRead5;
                                            break;
                                        case "Background Read 6":
                                            bg6 = bgRead5;
                                            break;
                                        case "Background Read 7":
                                            bg7 = bgRead5;
                                            break;
                                        case "Background Read 8":
                                            bg8 = bgRead5;
                                            break;
                                    }

                                    switch (bgRead6["ReadTypeName"].ToString())
                                    {
                                        case "Background Read 1":
                                            bg1 = bgRead6;
                                            break;
                                        case "Background Read 2":
                                            bg2 = bgRead6;
                                            break;
                                        case "Background Read 3":
                                            bg3 = bgRead6;
                                            break;
                                        case "Background Read 4":
                                            bg4 = bgRead6;
                                            break;
                                        case "Background Read 5":
                                            bg5 = bgRead6;
                                            break;
                                        case "Background Read 6":
                                            bg6 = bgRead6;
                                            break;
                                        case "Background Read 7":
                                            bg7 = bgRead6;
                                            break;
                                        case "Background Read 8":
                                            bg8 = bgRead6;
                                            break;
                                    }

                                    myDT.Rows.Add(GenerateDataRow(myDT, curDeviceID, snRead1, bg1, bg2, bg3, bg4, bg5, bg6, bg7, bg8));
                                    iter += 6;
                                }
                                else if (snRead1 == snRead2 && snRead1 == snRead3 && snRead1 == snRead4 && snRead1 == snRead5)
                                {
                                    // Missing 3 bg reads
                                    switch (bgRead1["ReadTypeName"].ToString())
                                    {
                                        case "Background Read 1":
                                            bg1 = bgRead1;
                                            break;
                                        case "Background Read 2":
                                            bg2 = bgRead1;
                                            break;
                                        case "Background Read 3":
                                            bg3 = bgRead1;
                                            break;
                                        case "Background Read 4":
                                            bg4 = bgRead1;
                                            break;
                                        case "Background Read 5":
                                            bg5 = bgRead1;
                                            break;
                                        case "Background Read 6":
                                            bg6 = bgRead1;
                                            break;
                                        case "Background Read 7":
                                            bg7 = bgRead1;
                                            break;
                                        case "Background Read 8":
                                            bg8 = bgRead1;
                                            break;
                                    }

                                    switch (bgRead2["ReadTypeName"].ToString())
                                    {
                                        case "Background Read 1":
                                            bg1 = bgRead2;
                                            break;
                                        case "Background Read 2":
                                            bg2 = bgRead2;
                                            break;
                                        case "Background Read 3":
                                            bg3 = bgRead2;
                                            break;
                                        case "Background Read 4":
                                            bg4 = bgRead2;
                                            break;
                                        case "Background Read 5":
                                            bg5 = bgRead2;
                                            break;
                                        case "Background Read 6":
                                            bg6 = bgRead2;
                                            break;
                                        case "Background Read 7":
                                            bg7 = bgRead2;
                                            break;
                                        case "Background Read 8":
                                            bg8 = bgRead2;
                                            break;
                                    }

                                    switch (bgRead3["ReadTypeName"].ToString())
                                    {
                                        case "Background Read 1":
                                            bg1 = bgRead3;
                                            break;
                                        case "Background Read 2":
                                            bg2 = bgRead3;
                                            break;
                                        case "Background Read 3":
                                            bg3 = bgRead3;
                                            break;
                                        case "Background Read 4":
                                            bg4 = bgRead3;
                                            break;
                                        case "Background Read 5":
                                            bg5 = bgRead3;
                                            break;
                                        case "Background Read 6":
                                            bg6 = bgRead3;
                                            break;
                                        case "Background Read 7":
                                            bg7 = bgRead3;
                                            break;
                                        case "Background Read 8":
                                            bg8 = bgRead3;
                                            break;
                                    }

                                    switch (bgRead4["ReadTypeName"].ToString())
                                    {
                                        case "Background Read 1":
                                            bg1 = bgRead4;
                                            break;
                                        case "Background Read 2":
                                            bg2 = bgRead4;
                                            break;
                                        case "Background Read 3":
                                            bg3 = bgRead4;
                                            break;
                                        case "Background Read 4":
                                            bg4 = bgRead4;
                                            break;
                                        case "Background Read 5":
                                            bg5 = bgRead4;
                                            break;
                                        case "Background Read 6":
                                            bg6 = bgRead4;
                                            break;
                                        case "Background Read 7":
                                            bg7 = bgRead4;
                                            break;
                                        case "Background Read 8":
                                            bg8 = bgRead4;
                                            break;
                                    }

                                    switch (bgRead5["ReadTypeName"].ToString())
                                    {
                                        case "Background Read 1":
                                            bg1 = bgRead5;
                                            break;
                                        case "Background Read 2":
                                            bg2 = bgRead5;
                                            break;
                                        case "Background Read 3":
                                            bg3 = bgRead5;
                                            break;
                                        case "Background Read 4":
                                            bg4 = bgRead5;
                                            break;
                                        case "Background Read 5":
                                            bg5 = bgRead5;
                                            break;
                                        case "Background Read 6":
                                            bg6 = bgRead5;
                                            break;
                                        case "Background Read 7":
                                            bg7 = bgRead5;
                                            break;
                                        case "Background Read 8":
                                            bg8 = bgRead5;
                                            break;
                                    }

                                    myDT.Rows.Add(GenerateDataRow(myDT, curDeviceID, snRead1, bg1, bg2, bg3, bg4, bg5, bg6, bg7, bg8));
                                    iter += 5;
                                }
                                else if (snRead1 == snRead2 && snRead1 == snRead3 && snRead1 == snRead4)
                                {
                                    // Missing 4 bg reads
                                    switch (bgRead1["ReadTypeName"].ToString())
                                    {
                                        case "Background Read 1":
                                            bg1 = bgRead1;
                                            break;
                                        case "Background Read 2":
                                            bg2 = bgRead1;
                                            break;
                                        case "Background Read 3":
                                            bg3 = bgRead1;
                                            break;
                                        case "Background Read 4":
                                            bg4 = bgRead1;
                                            break;
                                        case "Background Read 5":
                                            bg5 = bgRead1;
                                            break;
                                        case "Background Read 6":
                                            bg6 = bgRead1;
                                            break;
                                        case "Background Read 7":
                                            bg7 = bgRead1;
                                            break;
                                        case "Background Read 8":
                                            bg8 = bgRead1;
                                            break;
                                    }

                                    switch (bgRead2["ReadTypeName"].ToString())
                                    {
                                        case "Background Read 1":
                                            bg1 = bgRead2;
                                            break;
                                        case "Background Read 2":
                                            bg2 = bgRead2;
                                            break;
                                        case "Background Read 3":
                                            bg3 = bgRead2;
                                            break;
                                        case "Background Read 4":
                                            bg4 = bgRead2;
                                            break;
                                        case "Background Read 5":
                                            bg5 = bgRead2;
                                            break;
                                        case "Background Read 6":
                                            bg6 = bgRead2;
                                            break;
                                        case "Background Read 7":
                                            bg7 = bgRead2;
                                            break;
                                        case "Background Read 8":
                                            bg8 = bgRead2;
                                            break;
                                    }

                                    switch (bgRead3["ReadTypeName"].ToString())
                                    {
                                        case "Background Read 1":
                                            bg1 = bgRead3;
                                            break;
                                        case "Background Read 2":
                                            bg2 = bgRead3;
                                            break;
                                        case "Background Read 3":
                                            bg3 = bgRead3;
                                            break;
                                        case "Background Read 4":
                                            bg4 = bgRead3;
                                            break;
                                        case "Background Read 5":
                                            bg5 = bgRead3;
                                            break;
                                        case "Background Read 6":
                                            bg6 = bgRead3;
                                            break;
                                        case "Background Read 7":
                                            bg7 = bgRead3;
                                            break;
                                        case "Background Read 8":
                                            bg8 = bgRead3;
                                            break;
                                    }

                                    switch (bgRead4["ReadTypeName"].ToString())
                                    {
                                        case "Background Read 1":
                                            bg1 = bgRead4;
                                            break;
                                        case "Background Read 2":
                                            bg2 = bgRead4;
                                            break;
                                        case "Background Read 3":
                                            bg3 = bgRead4;
                                            break;
                                        case "Background Read 4":
                                            bg4 = bgRead4;
                                            break;
                                        case "Background Read 5":
                                            bg5 = bgRead4;
                                            break;
                                        case "Background Read 6":
                                            bg6 = bgRead4;
                                            break;
                                        case "Background Read 7":
                                            bg7 = bgRead4;
                                            break;
                                        case "Background Read 8":
                                            bg8 = bgRead4;
                                            break;
                                    }

                                    myDT.Rows.Add(GenerateDataRow(myDT, curDeviceID, snRead1, bg1, bg2, bg3, bg4, bg5, bg6, bg7, bg8));
                                    iter += 4;
                                }
                                else if (snRead1 == snRead2 && snRead1 == snRead3)
                                {
                                    // Missing 5 bg reads
                                    switch (bgRead1["ReadTypeName"].ToString())
                                    {
                                        case "Background Read 1":
                                            bg1 = bgRead1;
                                            break;
                                        case "Background Read 2":
                                            bg2 = bgRead1;
                                            break;
                                        case "Background Read 3":
                                            bg3 = bgRead1;
                                            break;
                                        case "Background Read 4":
                                            bg4 = bgRead1;
                                            break;
                                        case "Background Read 5":
                                            bg5 = bgRead1;
                                            break;
                                        case "Background Read 6":
                                            bg6 = bgRead1;
                                            break;
                                        case "Background Read 7":
                                            bg7 = bgRead1;
                                            break;
                                        case "Background Read 8":
                                            bg8 = bgRead1;
                                            break;
                                    }

                                    switch (bgRead2["ReadTypeName"].ToString())
                                    {
                                        case "Background Read 1":
                                            bg1 = bgRead2;
                                            break;
                                        case "Background Read 2":
                                            bg2 = bgRead2;
                                            break;
                                        case "Background Read 3":
                                            bg3 = bgRead2;
                                            break;
                                        case "Background Read 4":
                                            bg4 = bgRead2;
                                            break;
                                        case "Background Read 5":
                                            bg5 = bgRead2;
                                            break;
                                        case "Background Read 6":
                                            bg6 = bgRead2;
                                            break;
                                        case "Background Read 7":
                                            bg7 = bgRead2;
                                            break;
                                        case "Background Read 8":
                                            bg8 = bgRead2;
                                            break;
                                    }

                                    switch (bgRead3["ReadTypeName"].ToString())
                                    {
                                        case "Background Read 1":
                                            bg1 = bgRead3;
                                            break;
                                        case "Background Read 2":
                                            bg2 = bgRead3;
                                            break;
                                        case "Background Read 3":
                                            bg3 = bgRead3;
                                            break;
                                        case "Background Read 4":
                                            bg4 = bgRead3;
                                            break;
                                        case "Background Read 5":
                                            bg5 = bgRead3;
                                            break;
                                        case "Background Read 6":
                                            bg6 = bgRead3;
                                            break;
                                        case "Background Read 7":
                                            bg7 = bgRead3;
                                            break;
                                        case "Background Read 8":
                                            bg8 = bgRead3;
                                            break;
                                    }

                                    myDT.Rows.Add(GenerateDataRow(myDT, curDeviceID, snRead1, bg1, bg2, bg3, bg4, bg5, bg6, bg7, bg8));
                                    iter += 3;
                                }
                                else if (snRead1 == snRead2)
                                {
                                    // Missing 6 bg reads
                                    switch (bgRead1["ReadTypeName"].ToString())
                                    {
                                        case "Background Read 1":
                                            bg1 = bgRead1;
                                            break;
                                        case "Background Read 2":
                                            bg2 = bgRead1;
                                            break;
                                        case "Background Read 3":
                                            bg3 = bgRead1;
                                            break;
                                        case "Background Read 4":
                                            bg4 = bgRead1;
                                            break;
                                        case "Background Read 5":
                                            bg5 = bgRead1;
                                            break;
                                        case "Background Read 6":
                                            bg6 = bgRead1;
                                            break;
                                        case "Background Read 7":
                                            bg7 = bgRead1;
                                            break;
                                        case "Background Read 8":
                                            bg8 = bgRead1;
                                            break;
                                    }

                                    switch (bgRead2["ReadTypeName"].ToString())
                                    {
                                        case "Background Read 1":
                                            bg1 = bgRead2;
                                            break;
                                        case "Background Read 2":
                                            bg2 = bgRead2;
                                            break;
                                        case "Background Read 3":
                                            bg3 = bgRead2;
                                            break;
                                        case "Background Read 4":
                                            bg4 = bgRead2;
                                            break;
                                        case "Background Read 5":
                                            bg5 = bgRead2;
                                            break;
                                        case "Background Read 6":
                                            bg6 = bgRead2;
                                            break;
                                        case "Background Read 7":
                                            bg7 = bgRead2;
                                            break;
                                        case "Background Read 8":
                                            bg8 = bgRead2;
                                            break;
                                    }

                                    myDT.Rows.Add(GenerateDataRow(myDT, curDeviceID, snRead1, bg1, bg2, bg3, bg4, bg5, bg6, bg7, bg8));
                                    iter += 2;
                                }
                                else
                                {
                                    // Missing 7 bg reads
                                    switch (bgRead1["ReadTypeName"].ToString())
                                    {
                                        case "Background Read 1":
                                            bg1 = bgRead1;
                                            break;
                                        case "Background Read 2":
                                            bg2 = bgRead1;
                                            break;
                                        case "Background Read 3":
                                            bg3 = bgRead1;
                                            break;
                                        case "Background Read 4":
                                            bg4 = bgRead1;
                                            break;
                                        case "Background Read 5":
                                            bg5 = bgRead1;
                                            break;
                                        case "Background Read 6":
                                            bg6 = bgRead1;
                                            break;
                                        case "Background Read 7":
                                            bg7 = bgRead1;
                                            break;
                                        case "Background Read 8":
                                            bg8 = bgRead1;
                                            break;
                                    }

                                    myDT.Rows.Add(GenerateDataRow(myDT, curDeviceID, snRead1, bg1, bg2, bg3, bg4, bg5, bg6, bg7, bg8));
                                    iter += 1;
                                }  // End if..else GenerateDataRow()

                            }   // End identify what read type is missing                    

                        }  // End missing some reads for current serial number

                    }  // End while loop that iterates through dataset and add the row to myDT DataTable.

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
        /// <param name="pDT"></param>
        /// <param name="pDeviceID"></param>
        /// <param name="pSerialNo"></param>
        /// <param name="pBkgd1Row"></param>
        /// <param name="pBkgd2Row"></param>
        /// <param name="pBkgd3Row"></param>
        /// <param name="pBkgd4Row"></param>
        /// <param name="pBkgd5Row"></param>
        /// <param name="pBkgd6Row"></param>
        /// <param name="pBkgd7Row"></param>
        /// <param name="pBkgd8Row"></param>
        /// <returns></returns>
        private DataRow GenerateDataRow(DataTable pDT, int pDeviceID, string pSerialNo, DataRow pBkgd1Row, DataRow pBkgd2Row, DataRow pBkgd3Row,
            DataRow pBkgd4Row, DataRow pBkgd5Row, DataRow pBkgd6Row, DataRow pBkgd7Row, DataRow pBkgd8Row)
        {
            try
            {
                double DL_1stP_LR = Convert.ToDouble(this.txtBkgd_1stP_LR.Text);
                double DL_1stP_HR = Convert.ToDouble(this.txtBkgd_1stP_HR.Text);
                double SL_1stP_LR = DL_1stP_LR;
                double SL_1stP_HR = DL_1stP_HR;

                double DL_2ndP_LR = Convert.ToDouble(this.txtBkgd_2ndP_LR.Text);
                double DL_2ndP_HR = Convert.ToDouble(this.txtBkgd_2ndP_HR.Text);
                double SL_2ndP_LR = DL_2ndP_LR;
                double SL_2ndP_HR = DL_2ndP_HR;

                double? DLDRaw_BkgdRead1, DLDRaw_BkgdRead8, SLDRaw_BkgdRead1, SLDRaw_BkgdRead8;
                double? DLDRaw_BkgdRead2, DLDRaw_BkgdRead3, SLDRaw_BkgdRead2, SLDRaw_BkgdRead3;
                double? DLDRaw_BkgdRead4, DLDRaw_BkgdRead5, SLDRaw_BkgdRead4, SLDRaw_BkgdRead5;
                double? DLDRaw_BkgdRead6, DLDRaw_BkgdRead7, SLDRaw_BkgdRead6, SLDRaw_BkgdRead7;
                double? DL_1_2, SL_1_2, DL_2_3, SL_2_3, DL_3_4, SL_3_4, DL_4_5, SL_4_5;
                double? DL_5_6, SL_5_6, DL_6_7, SL_6_7, DL_7_8, SL_7_8, DL_1_8, SL_1_8;
                DateTime? bkgdRead1ExposureDate, bkgdRead1ScheduledTime, bkgdRead8ExposureDate, bkgdRead8ScheduledTime;
                DateTime? bkgdRead2ExposureDate, bkgdRead2ScheduledTime, bkgdRead3ExposureDate, bkgdRead3ScheduledTime;
                DateTime? bkgdRead4ExposureDate, bkgdRead4ScheduledTime, bkgdRead5ExposureDate, bkgdRead5ScheduledTime;
                DateTime? bkgdRead6ExposureDate, bkgdRead6ScheduledTime, bkgdRead7ExposureDate, bkgdRead7ScheduledTime;
                DateTime? myDateTimeNull = null;
                double? myDoubleNull = null;

                string my1stPassFail = "pass";
                string my2ndPassFail = "pass";
                string myPassFail = "pass";
                string myOutlier = "N";
                bool my2ndTestContinue = true;

                // Get DL, SL values
                if (pBkgd1Row != null)
                {
                    DLDRaw_BkgdRead1 = (pBkgd1Row["DeepLowDoseRaw"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pBkgd1Row["DeepLowDoseRaw"]), 2) : myDoubleNull;
                    SLDRaw_BkgdRead1 = (pBkgd1Row["ShallowLowDoseRaw"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pBkgd1Row["ShallowLowDoseRaw"]), 2) : myDoubleNull;
                    bkgdRead1ExposureDate = (pBkgd1Row["ExposureDate"] != System.DBNull.Value) ? Convert.ToDateTime(Convert.ToDateTime(pBkgd1Row["ExposureDate"]).ToShortDateString()) : myDateTimeNull;
                    bkgdRead1ScheduledTime = (pBkgd1Row["ScheduledTime"] != System.DBNull.Value) ? Convert.ToDateTime(Convert.ToDateTime(pBkgd1Row["ScheduledTime"]).ToShortDateString()) : myDateTimeNull;
                }
                else
                {
                    DLDRaw_BkgdRead1 = null;
                    SLDRaw_BkgdRead1 = null;
                    bkgdRead1ExposureDate = null;
                    bkgdRead1ScheduledTime = null;
                }

                if (pBkgd2Row != null)
                {
                    DLDRaw_BkgdRead2 = (pBkgd2Row["DeepLowDoseRaw"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pBkgd2Row["DeepLowDoseRaw"]), 2) : myDoubleNull;
                    SLDRaw_BkgdRead2 = (pBkgd2Row["ShallowLowDoseRaw"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pBkgd2Row["ShallowLowDoseRaw"]), 2) : myDoubleNull;
                    bkgdRead2ExposureDate = (pBkgd2Row["ExposureDate"] != System.DBNull.Value) ? Convert.ToDateTime(Convert.ToDateTime(pBkgd2Row["ExposureDate"]).ToShortDateString()) : myDateTimeNull;
                    bkgdRead2ScheduledTime = (pBkgd2Row["ScheduledTime"] != System.DBNull.Value) ? Convert.ToDateTime(Convert.ToDateTime(pBkgd2Row["ScheduledTime"]).ToShortDateString()) : myDateTimeNull;
                }
                else
                {
                    DLDRaw_BkgdRead2 = null;
                    SLDRaw_BkgdRead2 = null;
                    bkgdRead2ExposureDate = null;
                    bkgdRead2ScheduledTime = null;
                }

                if (pBkgd3Row != null)
                {
                    DLDRaw_BkgdRead3 = (pBkgd3Row["DeepLowDoseRaw"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pBkgd3Row["DeepLowDoseRaw"]), 2) : myDoubleNull;
                    SLDRaw_BkgdRead3 = (pBkgd3Row["ShallowLowDoseRaw"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pBkgd3Row["ShallowLowDoseRaw"]), 2) : myDoubleNull;
                    bkgdRead3ExposureDate = (pBkgd3Row["ExposureDate"] != System.DBNull.Value) ? Convert.ToDateTime(Convert.ToDateTime(pBkgd3Row["ExposureDate"]).ToShortDateString()) : myDateTimeNull;
                    bkgdRead3ScheduledTime = (pBkgd3Row["ScheduledTime"] != System.DBNull.Value) ? Convert.ToDateTime(Convert.ToDateTime(pBkgd3Row["ScheduledTime"]).ToShortDateString()) : myDateTimeNull;
                }
                else
                {
                    DLDRaw_BkgdRead3 = null;
                    SLDRaw_BkgdRead3 = null;
                    bkgdRead3ExposureDate = null;
                    bkgdRead3ScheduledTime = null;
                }

                if (pBkgd4Row != null)
                {
                    DLDRaw_BkgdRead4 = (pBkgd4Row["DeepLowDoseRaw"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pBkgd4Row["DeepLowDoseRaw"]), 2) : myDoubleNull;
                    SLDRaw_BkgdRead4 = (pBkgd4Row["ShallowLowDoseRaw"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pBkgd4Row["ShallowLowDoseRaw"]), 2) : myDoubleNull;
                    bkgdRead4ExposureDate = (pBkgd4Row["ExposureDate"] != System.DBNull.Value) ? Convert.ToDateTime(Convert.ToDateTime(pBkgd4Row["ExposureDate"]).ToShortDateString()) : myDateTimeNull;
                    bkgdRead4ScheduledTime = (pBkgd4Row["ScheduledTime"] != System.DBNull.Value) ? Convert.ToDateTime(Convert.ToDateTime(pBkgd4Row["ScheduledTime"]).ToShortDateString()) : myDateTimeNull;
                }
                else
                {
                    DLDRaw_BkgdRead4 = null;
                    SLDRaw_BkgdRead4 = null;
                    bkgdRead4ExposureDate = null;
                    bkgdRead4ScheduledTime = null;
                }

                if (pBkgd5Row != null)
                {
                    DLDRaw_BkgdRead5 = (pBkgd5Row["DeepLowDoseRaw"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pBkgd5Row["DeepLowDoseRaw"]), 2) : myDoubleNull;
                    SLDRaw_BkgdRead5 = (pBkgd5Row["ShallowLowDoseRaw"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pBkgd5Row["ShallowLowDoseRaw"]), 2) : myDoubleNull;
                    bkgdRead5ExposureDate = (pBkgd5Row["ExposureDate"] != System.DBNull.Value) ? Convert.ToDateTime(Convert.ToDateTime(pBkgd5Row["ExposureDate"]).ToShortDateString()) : myDateTimeNull;
                    bkgdRead5ScheduledTime = (pBkgd5Row["ScheduledTime"] != System.DBNull.Value) ? Convert.ToDateTime(Convert.ToDateTime(pBkgd5Row["ScheduledTime"]).ToShortDateString()) : myDateTimeNull;
                }
                else
                {
                    DLDRaw_BkgdRead5 = null;
                    SLDRaw_BkgdRead5 = null;
                    bkgdRead5ExposureDate = null;
                    bkgdRead5ScheduledTime = null;
                }

                if (pBkgd6Row != null)
                {
                    DLDRaw_BkgdRead6 = (pBkgd6Row["DeepLowDoseRaw"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pBkgd6Row["DeepLowDoseRaw"]), 2) : myDoubleNull;
                    SLDRaw_BkgdRead6 = (pBkgd6Row["ShallowLowDoseRaw"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pBkgd6Row["ShallowLowDoseRaw"]), 2) : myDoubleNull;
                    bkgdRead6ExposureDate = (pBkgd6Row["ExposureDate"] != System.DBNull.Value) ? Convert.ToDateTime(Convert.ToDateTime(pBkgd6Row["ExposureDate"]).ToShortDateString()) : myDateTimeNull;
                    bkgdRead6ScheduledTime = (pBkgd6Row["ScheduledTime"] != System.DBNull.Value) ? Convert.ToDateTime(Convert.ToDateTime(pBkgd6Row["ScheduledTime"]).ToShortDateString()) : myDateTimeNull;
                }
                else
                {
                    DLDRaw_BkgdRead6 = null;
                    SLDRaw_BkgdRead6 = null;
                    bkgdRead6ExposureDate = null;
                    bkgdRead6ScheduledTime = null;
                }

                if (pBkgd7Row != null)
                {
                    DLDRaw_BkgdRead7 = (pBkgd7Row["DeepLowDoseRaw"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pBkgd7Row["DeepLowDoseRaw"]), 2) : myDoubleNull;
                    SLDRaw_BkgdRead7 = (pBkgd7Row["ShallowLowDoseRaw"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pBkgd7Row["ShallowLowDoseRaw"]), 2) : myDoubleNull;
                    bkgdRead7ExposureDate = (pBkgd7Row["ExposureDate"] != System.DBNull.Value) ? Convert.ToDateTime(Convert.ToDateTime(pBkgd7Row["ExposureDate"]).ToShortDateString()) : myDateTimeNull;
                    bkgdRead7ScheduledTime = (pBkgd7Row["ScheduledTime"] != System.DBNull.Value) ? Convert.ToDateTime(Convert.ToDateTime(pBkgd7Row["ScheduledTime"]).ToShortDateString()) : myDateTimeNull;
                }
                else
                {
                    DLDRaw_BkgdRead7 = null;
                    SLDRaw_BkgdRead7 = null;
                    bkgdRead7ExposureDate = null;
                    bkgdRead7ScheduledTime = null;
                }

                if (pBkgd8Row != null)
                {
                    DLDRaw_BkgdRead8 = (pBkgd8Row["DeepLowDoseRaw"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pBkgd8Row["DeepLowDoseRaw"]), 2) : myDoubleNull;
                    SLDRaw_BkgdRead8 = (pBkgd8Row["ShallowLowDoseRaw"] != System.DBNull.Value) ? Math.Round(Convert.ToDouble(pBkgd8Row["ShallowLowDoseRaw"]), 2) : myDoubleNull;
                    bkgdRead8ExposureDate = (pBkgd8Row["ExposureDate"] != System.DBNull.Value) ? Convert.ToDateTime(Convert.ToDateTime(pBkgd8Row["ExposureDate"]).ToShortDateString()) : myDateTimeNull;
                    bkgdRead8ScheduledTime = (pBkgd8Row["ScheduledTime"] != System.DBNull.Value) ? Convert.ToDateTime(Convert.ToDateTime(pBkgd8Row["ScheduledTime"]).ToShortDateString()) : myDateTimeNull;
                }
                else
                {
                    DLDRaw_BkgdRead8 = null;
                    SLDRaw_BkgdRead8 = null;
                    bkgdRead8ExposureDate = null;
                    bkgdRead8ScheduledTime = null;
                }

                // Calculate DL, SL bkgd
                if (DLDRaw_BkgdRead2.HasValue & DLDRaw_BkgdRead1.HasValue & SLDRaw_BkgdRead2.HasValue & SLDRaw_BkgdRead1.HasValue)
                {
                    DL_1_2 = Math.Round(DLDRaw_BkgdRead2.Value - DLDRaw_BkgdRead1.Value, 2);
                    SL_1_2 = Math.Round(SLDRaw_BkgdRead2.Value - SLDRaw_BkgdRead1.Value, 2);
                }
                else
                {
                    DL_1_2 = null;
                    SL_1_2 = null;
                    if (DLDRaw_BkgdRead2.HasValue & DLDRaw_BkgdRead1.HasValue)
                        DL_1_2 = Math.Round(DLDRaw_BkgdRead2.Value - DLDRaw_BkgdRead1.Value, 2);
                    if (SLDRaw_BkgdRead2.HasValue & SLDRaw_BkgdRead1.HasValue)
                        SL_1_2 = Math.Round(SLDRaw_BkgdRead2.Value - SLDRaw_BkgdRead1.Value, 2);
                }

                if (DLDRaw_BkgdRead3.HasValue & DLDRaw_BkgdRead2.HasValue & SLDRaw_BkgdRead3.HasValue & SLDRaw_BkgdRead2.HasValue)
                {
                    DL_2_3 = Math.Round(DLDRaw_BkgdRead3.Value - DLDRaw_BkgdRead2.Value, 2);
                    SL_2_3 = Math.Round(SLDRaw_BkgdRead3.Value - SLDRaw_BkgdRead2.Value, 2);
                }
                else
                {
                    DL_2_3 = null;
                    SL_2_3 = null;
                    if (DLDRaw_BkgdRead3.HasValue & DLDRaw_BkgdRead2.HasValue)
                        DL_2_3 = Math.Round(DLDRaw_BkgdRead3.Value - DLDRaw_BkgdRead2.Value, 2);
                    if (SLDRaw_BkgdRead3.HasValue & SLDRaw_BkgdRead2.HasValue)
                        SL_2_3 = Math.Round(SLDRaw_BkgdRead3.Value - SLDRaw_BkgdRead2.Value, 2);
                }

                if (DLDRaw_BkgdRead4.HasValue & DLDRaw_BkgdRead3.HasValue & SLDRaw_BkgdRead4.HasValue & SLDRaw_BkgdRead3.HasValue)
                {
                    DL_3_4 = Math.Round(DLDRaw_BkgdRead4.Value - DLDRaw_BkgdRead3.Value, 2);
                    SL_3_4 = Math.Round(SLDRaw_BkgdRead4.Value - SLDRaw_BkgdRead3.Value, 2);
                }
                else
                {
                    DL_3_4 = null;
                    SL_3_4 = null;
                    if (DLDRaw_BkgdRead4.HasValue & DLDRaw_BkgdRead3.HasValue)
                        DL_3_4 = Math.Round(DLDRaw_BkgdRead4.Value - DLDRaw_BkgdRead3.Value, 2);
                    if (SLDRaw_BkgdRead4.HasValue & SLDRaw_BkgdRead3.HasValue)
                        SL_3_4 = Math.Round(SLDRaw_BkgdRead4.Value - SLDRaw_BkgdRead3.Value, 2);
                }

                if (DLDRaw_BkgdRead5.HasValue & DLDRaw_BkgdRead4.HasValue & SLDRaw_BkgdRead5.HasValue & SLDRaw_BkgdRead4.HasValue)
                {
                    DL_4_5 = Math.Round(DLDRaw_BkgdRead5.Value - DLDRaw_BkgdRead4.Value, 2);
                    SL_4_5 = Math.Round(SLDRaw_BkgdRead5.Value - SLDRaw_BkgdRead4.Value, 2);
                }
                else
                {
                    DL_4_5 = null;
                    SL_4_5 = null;
                    if (DLDRaw_BkgdRead5.HasValue & DLDRaw_BkgdRead4.HasValue)
                        DL_4_5 = Math.Round(DLDRaw_BkgdRead5.Value - DLDRaw_BkgdRead4.Value, 2);
                    if (SLDRaw_BkgdRead5.HasValue & SLDRaw_BkgdRead4.HasValue)
                        SL_4_5 = Math.Round(SLDRaw_BkgdRead5.Value - SLDRaw_BkgdRead4.Value, 2);
                }

                if (DLDRaw_BkgdRead6.HasValue & DLDRaw_BkgdRead5.HasValue & SLDRaw_BkgdRead6.HasValue & SLDRaw_BkgdRead5.HasValue)
                {
                    DL_5_6 = Math.Round(DLDRaw_BkgdRead6.Value - DLDRaw_BkgdRead5.Value, 2);
                    SL_5_6 = Math.Round(SLDRaw_BkgdRead6.Value - SLDRaw_BkgdRead5.Value, 2);
                }
                else
                {
                    DL_5_6 = null;
                    SL_5_6 = null;
                    if (DLDRaw_BkgdRead6.HasValue & DLDRaw_BkgdRead5.HasValue)
                        DL_5_6 = Math.Round(DLDRaw_BkgdRead6.Value - DLDRaw_BkgdRead5.Value, 2);
                    if (SLDRaw_BkgdRead6.HasValue & SLDRaw_BkgdRead5.HasValue)
                        SL_5_6 = Math.Round(SLDRaw_BkgdRead6.Value - SLDRaw_BkgdRead5.Value, 2);
                }

                if (DLDRaw_BkgdRead7.HasValue & DLDRaw_BkgdRead6.HasValue & SLDRaw_BkgdRead7.HasValue & SLDRaw_BkgdRead6.HasValue)
                {
                    DL_6_7 = Math.Round(DLDRaw_BkgdRead7.Value - DLDRaw_BkgdRead6.Value, 2);
                    SL_6_7 = Math.Round(SLDRaw_BkgdRead7.Value - SLDRaw_BkgdRead6.Value, 2);
                }
                else
                {
                    DL_6_7 = null;
                    SL_6_7 = null;
                    if (DLDRaw_BkgdRead7.HasValue & DLDRaw_BkgdRead6.HasValue)
                        DL_6_7 = Math.Round(DLDRaw_BkgdRead7.Value - DLDRaw_BkgdRead6.Value, 2);
                    if (SLDRaw_BkgdRead7.HasValue & SLDRaw_BkgdRead6.HasValue)
                        SL_6_7 = Math.Round(SLDRaw_BkgdRead7.Value - SLDRaw_BkgdRead6.Value, 2);
                }

                if (DLDRaw_BkgdRead8.HasValue & DLDRaw_BkgdRead7.HasValue & SLDRaw_BkgdRead8.HasValue & SLDRaw_BkgdRead7.HasValue)
                {
                    DL_7_8 = Math.Round(DLDRaw_BkgdRead8.Value - DLDRaw_BkgdRead7.Value, 2);
                    SL_7_8 = Math.Round(SLDRaw_BkgdRead8.Value - SLDRaw_BkgdRead7.Value, 2);
                }
                else
                {
                    DL_7_8 = null;
                    SL_7_8 = null;
                    if (DLDRaw_BkgdRead8.HasValue & DLDRaw_BkgdRead7.HasValue)
                        DL_7_8 = Math.Round(DLDRaw_BkgdRead8.Value - DLDRaw_BkgdRead7.Value, 2);
                    if (SLDRaw_BkgdRead8.HasValue & SLDRaw_BkgdRead7.HasValue)
                        SL_7_8 = Math.Round(SLDRaw_BkgdRead8.Value - SLDRaw_BkgdRead7.Value, 2);
                }

                if (DLDRaw_BkgdRead8.HasValue & DLDRaw_BkgdRead1.HasValue & SLDRaw_BkgdRead8.HasValue & SLDRaw_BkgdRead1.HasValue)
                {
                    DL_1_8 = Math.Round(DLDRaw_BkgdRead8.Value - DLDRaw_BkgdRead1.Value, 2);
                    SL_1_8 = Math.Round(SLDRaw_BkgdRead8.Value - SLDRaw_BkgdRead1.Value, 2);
                }
                else
                {
                    DL_1_8 = null;
                    SL_1_8 = null;
                    if (DLDRaw_BkgdRead8.HasValue & DLDRaw_BkgdRead1.HasValue)
                        DL_1_8 = Math.Round(DLDRaw_BkgdRead8.Value - DLDRaw_BkgdRead1.Value, 2);
                    if (SLDRaw_BkgdRead8.HasValue & SLDRaw_BkgdRead1.HasValue)
                        SL_1_8 = Math.Round(SLDRaw_BkgdRead8.Value - SLDRaw_BkgdRead1.Value, 2);
                }

                // Validate 1st Pass/Fail
                if (DL_1_8.HasValue & SL_1_8.HasValue)
                {
                    if (DL_1_8.Value < DL_1stP_LR || DL_1_8.Value > DL_1stP_HR || SL_1_8.Value < SL_1stP_LR || SL_1_8.Value > SL_1stP_HR)
                    {
                        my1stPassFail = "fail";
                    }
                }
                else
                {
                    my1stPassFail = "processing";
                    myOutlier = "Y(MR)";
                }

                // Validate 2nd Pass/Fail

                if (my2ndTestContinue)
                {
                    if (DL_1_2.HasValue & SL_1_2.HasValue)
                    {
                        if (DL_1_2.Value < DL_2ndP_LR || DL_1_2.Value > DL_2ndP_HR || SL_1_2.Value < SL_2ndP_LR || SL_1_2.Value > SL_2ndP_HR)
                        {
                            my2ndPassFail = "fail";
                            my2ndTestContinue = false;
                        }
                    }
                    else
                    {
                        my2ndPassFail = "processing";
                        myOutlier = "Y(MR)";
                    }
                }

                if (my2ndTestContinue)
                {
                    if (DL_2_3.HasValue & SL_2_3.HasValue)
                    {
                        if (DL_2_3.Value < DL_2ndP_LR || DL_2_3.Value > DL_2ndP_HR || SL_2_3.Value < SL_2ndP_LR || SL_2_3.Value > SL_2ndP_HR)
                        {
                            my2ndPassFail = "fail";
                            my2ndTestContinue = false;
                        }
                    }
                    else
                    {
                        my2ndPassFail = "processing";
                        myOutlier = "Y(MR)";
                    }
                }

                if (my2ndTestContinue)
                {
                    if (DL_3_4.HasValue & SL_3_4.HasValue)
                    {
                        if (DL_3_4.Value < DL_2ndP_LR || DL_3_4.Value > DL_2ndP_HR || SL_3_4.Value < SL_2ndP_LR || SL_3_4.Value > SL_2ndP_HR)
                        {
                            my2ndPassFail = "fail";
                            my2ndTestContinue = false;
                        }
                    }
                    else
                    {
                        my2ndPassFail = "processing";
                        myOutlier = "Y(MR)";
                    }
                }

                if (my2ndTestContinue)
                {
                    if (DL_4_5.HasValue & SL_4_5.HasValue)
                    {
                        if (DL_4_5.Value < DL_2ndP_LR || DL_4_5.Value > DL_2ndP_HR || SL_4_5.Value < SL_2ndP_LR || SL_4_5.Value > SL_2ndP_HR)
                        {
                            my2ndPassFail = "fail";
                            my2ndTestContinue = false;
                        }
                    }
                    else
                    {
                        my2ndPassFail = "processing";
                        myOutlier = "Y(MR)";
                    }
                }

                if (my2ndTestContinue)
                {
                    if (DL_5_6.HasValue & SL_5_6.HasValue)
                    {
                        if (DL_5_6.Value < DL_2ndP_LR || DL_5_6.Value > DL_2ndP_HR || SL_5_6.Value < SL_2ndP_LR || SL_5_6.Value > SL_2ndP_HR)
                        {
                            my2ndPassFail = "fail";
                            my2ndTestContinue = false;
                        }
                    }
                    else
                    {
                        my2ndPassFail = "processing";
                        myOutlier = "Y(MR)";
                    }
                }

                if (my2ndTestContinue)
                {
                    if (DL_6_7.HasValue & SL_6_7.HasValue)
                    {
                        if (DL_6_7.Value < DL_2ndP_LR || DL_6_7.Value > DL_2ndP_HR || SL_6_7.Value < SL_2ndP_LR || SL_6_7.Value > SL_2ndP_HR)
                        {
                            my2ndPassFail = "fail";
                            my2ndTestContinue = false;
                        }
                    }
                    else
                    {
                        my2ndPassFail = "processing";
                        myOutlier = "Y(MR)";
                    }
                }

                if (my2ndTestContinue)
                {
                    if (DL_7_8.HasValue & SL_7_8.HasValue)
                    {
                        if (DL_7_8.Value < DL_2ndP_LR || DL_7_8.Value > DL_2ndP_HR || SL_7_8.Value < SL_2ndP_LR || SL_7_8.Value > SL_2ndP_HR)
                        {
                            my2ndPassFail = "fail";
                            my2ndTestContinue = false;
                        }
                    }
                    else
                    {
                        my2ndPassFail = "processing";
                        myOutlier = "Y(MR)";
                    }
                }                


                // Check background test pass/fail/processing
                if (my1stPassFail == "pass" && my2ndPassFail == "pass")
                {
                    myPassFail = "pass";
                }
                else if (my1stPassFail == "processing" || my2ndPassFail == "processing")
                {
                    myPassFail = "processing";
                }
                else
                {
                    myPassFail = "fail";
                }

                // Check ExposureDate & ScheduledDate does not match
                if (myOutlier == "N")
                {
                    if (bkgdRead1ExposureDate.HasValue & bkgdRead1ScheduledTime.HasValue & bkgdRead8ExposureDate.HasValue & bkgdRead8ScheduledTime.HasValue
                        & bkgdRead2ExposureDate.HasValue & bkgdRead2ScheduledTime.HasValue & bkgdRead3ExposureDate.HasValue & bkgdRead3ScheduledTime.HasValue
                        & bkgdRead4ExposureDate.HasValue & bkgdRead4ScheduledTime.HasValue & bkgdRead5ExposureDate.HasValue & bkgdRead5ScheduledTime.HasValue
                        & bkgdRead6ExposureDate.HasValue & bkgdRead6ScheduledTime.HasValue & bkgdRead7ExposureDate.HasValue & bkgdRead7ScheduledTime.HasValue)
                    {
                        if ((bkgdRead1ExposureDate.Value != bkgdRead1ScheduledTime.Value) || (bkgdRead8ExposureDate.Value != bkgdRead8ScheduledTime.Value)
                        || (bkgdRead2ExposureDate.Value != bkgdRead2ScheduledTime.Value) || (bkgdRead3ExposureDate.Value != bkgdRead3ScheduledTime.Value)
                        || (bkgdRead4ExposureDate.Value != bkgdRead4ScheduledTime.Value) || (bkgdRead5ExposureDate.Value != bkgdRead5ScheduledTime.Value)
                        || (bkgdRead6ExposureDate.Value != bkgdRead6ScheduledTime.Value) || (bkgdRead7ExposureDate.Value != bkgdRead7ScheduledTime.Value))
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

                // Establish a datarow
                DataRow dtr = pDT.NewRow();

                dtr["SerialNo"] = pSerialNo;
                dtr["1st Pass"] = my1stPassFail;
                dtr["2nd Pass"] = my2ndPassFail;
                dtr["P/F"] = myPassFail;
                dtr["OL(Y/N)"] = myOutlier;

                dtr["DL_1st"] = DL_1_8.HasValue ? DL_1_8.Value.ToString() : "";
                dtr["SL_1st"] = SL_1_8.HasValue ? SL_1_8.Value.ToString() : "";

                dtr["DL2-1"] = DL_1_2.HasValue ? DL_1_2.Value.ToString() : "";
                dtr["SL2-1"] = SL_1_2.HasValue ? SL_1_2.Value.ToString() : "";
                dtr["DL3-2"] = DL_2_3.HasValue ? DL_2_3.Value.ToString() : "";
                dtr["SL3-2"] = SL_2_3.HasValue ? SL_2_3.Value.ToString() : "";
                dtr["DL4-3"] = DL_3_4.HasValue ? DL_3_4.Value.ToString() : "";
                dtr["SL4-3"] = SL_3_4.HasValue ? SL_3_4.Value.ToString() : "";
                dtr["DL5-4"] = DL_4_5.HasValue ? DL_4_5.Value.ToString() : "";
                dtr["SL5-4"] = SL_4_5.HasValue ? SL_4_5.Value.ToString() : "";
                dtr["DL6-5"] = DL_5_6.HasValue ? DL_5_6.Value.ToString() : "";
                dtr["SL6-5"] = SL_5_6.HasValue ? SL_5_6.Value.ToString() : "";
                dtr["DL7-6"] = DL_6_7.HasValue ? DL_6_7.Value.ToString() : "";
                dtr["SL7-6"] = SL_6_7.HasValue ? SL_6_7.Value.ToString() : "";
                dtr["DL8-7"] = DL_7_8.HasValue ? DL_7_8.Value.ToString() : "";
                dtr["SL8-7"] = SL_7_8.HasValue ? SL_7_8.Value.ToString() : "";

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
                            IsPassBKGDTest = isPass
                        };
                        idc.ID2PassFails.InsertOnSubmit(myObj);
                        idc.SubmitChanges();
                    }
                    else
                    {
                        // Set pass/fail
                        myObj.IsPassBKGDTest = isPass;
                        idc.SubmitChanges();
                    }
                }
                else
                {
                    ID2PassFail myObj = new ID2PassFail
                    {
                        DeviceGroupID = this.DeviceGroupID,
                        DeviceID = pDeviceID,
                        IsPassBKGDTest = isPass
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

        protected void grdBackgroundTestAnalysisView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // for 1st pass
                switch (e.Row.Cells[1].Text.ToLower())
                {
                    case "pass":
                        e.Row.Cells[1].CssClass = "pass";
                        e.Row.Cells[1].Text = "";
                        break;                    
                    case "fail":
                        e.Row.Cells[1].CssClass = "fail";
                        e.Row.Cells[1].Text = "";
                        break;
                    case "processing":
                        e.Row.Cells[1].CssClass = "processing";
                        e.Row.Cells[1].Text = "";
                        break;
                    case "mismatchdate":
                        e.Row.Cells[1].CssClass = "mismatchdate";
                        e.Row.Cells[1].Text = "";
                        break;
                    default:
                        //e.Row.Cells[1].CssClass = "processing";
                        e.Row.Cells[1].Text = "";
                        break;
                }

                // for 2nd pass
                switch (e.Row.Cells[2].Text.ToLower())
                {
                    case "pass":
                        e.Row.Cells[2].CssClass = "pass";
                        e.Row.Cells[2].Text = "";
                        break;                    
                    case "fail":
                        e.Row.Cells[2].CssClass = "fail";
                        e.Row.Cells[2].Text = "";
                        break;
                    case "processing":
                        e.Row.Cells[2].CssClass = "processing";
                        e.Row.Cells[2].Text = "";
                        break;
                    case "mismatchdate":
                        e.Row.Cells[2].CssClass = "mismatchdate";
                        e.Row.Cells[2].Text = "";
                        break;
                    default:
                        //e.Row.Cells[2].CssClass = "processing";
                        e.Row.Cells[2].Text = "";
                        break;
                }

                // for final pass/fail
                switch (e.Row.Cells[3].Text.ToLower())
                {
                    case "pass":
                        e.Row.Cells[3].CssClass = "pass";
                        e.Row.Cells[3].Text = "";
                        break;                    
                    case "fail":
                        e.Row.Cells[3].CssClass = "fail";
                        e.Row.Cells[3].Text = "";
                        break;
                    case "processing":
                        e.Row.Cells[3].CssClass = "processing";
                        e.Row.Cells[3].Text = "";
                        break;
                    case "mismatchdate":
                        e.Row.Cells[3].CssClass = "mismatchdate";
                        e.Row.Cells[3].Text = "";
                        break;
                    default:
                        //e.Row.Cells[3].CssClass = "processing";
                        e.Row.Cells[3].Text = "";
                        break;
                }

                // For Outliers
                switch (e.Row.Cells[4].Text)
                {
                    case "N":
                        e.Row.Cells[4].CssClass = "no";
                        break;
                    default:
                        e.Row.Cells[4].CssClass = "yes";
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