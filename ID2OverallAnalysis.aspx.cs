/*
 * ID2 Overall Analysis 
 * 
 *  Created By: Tdo, 03/25/2014
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
    public partial class ID2OverallAnalysis : System.Web.UI.Page
    {
        // Turn on/off email to all others
        private bool DevelopmentServer = false;
        private int DeviceGroupID = 0;
        private int passNo;
        private int failNo;
        private int outlierNo;
        private InsDataContext idc = new InsDataContext();

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                // Auto set if a development site
                if (Request.Url.Authority.ToString().Contains(".mirioncorp.com"))
                    DevelopmentServer = true;

                int.TryParse(Request.QueryString["DeviceGroupID"].ToString(), out DeviceGroupID);

                if (DeviceGroupID == 0)
                {
                    Page.Response.Redirect("ID2Analysis.aspx");
                    return;
                }

                if (!Page.IsPostBack)
                {
                    string groupName = (from r in idc.DeviceGroups
                                        where r.DeviceGroupID == DeviceGroupID
                                        select r.DeviceGroupName).FirstOrDefault();

                    this.Header.InnerText = "Overall Pass/Fail Analysis - Group " + groupName;

                    passNo = 0;
                    failNo = 0;
                    outlierNo = 0;

                    grdOverallAnalysisView.DataSource = GetOverallAnalysisReport(DeviceGroupID);
                    grdOverallAnalysisView.DataBind();

                    DisplayStatisticNumber();
                }
            }
            catch { }
        }

        private DataTable GetOverallAnalysisReport(int pDeviceGroupID)
        {
            try
            {
                String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
                String cmdStr = "sp_GetID2PassFailByGroup";

                SqlConnection sqlConn = new SqlConnection(connStr);
                SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

                sqlConn.Open();

                sqlCmd.CommandType = CommandType.StoredProcedure;

                sqlCmd.Parameters.Add("@DeviceGroupID", SqlDbType.Int);
                sqlCmd.Parameters["@DeviceGroupID"].Value = pDeviceGroupID;

                SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
                DataTable dt = new DataTable();

                sqlDA.Fill(dt);
                sqlConn.Close();

                if (dt.Rows.Count > 0)
                {
                    // Is Pass Calibration,Is Pass Drift, Is Pass BKGD Test = {pass, fail, outlier}
                    // Overall Result
                    foreach (DataRow row in dt.Rows)
                    {
                        if (row["Is Pass Calibration"].ToString() == "outlier" || row["Is Pass Drift"].ToString() == "outlier" || row["Is Pass BKGD Test"].ToString() == "outlier")
                        {
                            row["Overall Result"] = "Outlier";  // if missing read or incorrect read date
                            this.outlierNo++;
                        }
                        else if (row["Is Pass Calibration"].ToString() == "pass" && row["Is Pass Drift"].ToString() == "pass" && row["Is Pass BKGD Test"].ToString() == "pass")
                        {
                            row["Overall Result"] = "Pass";
                            this.passNo++;
                        }
                        else
                        {
                            if (row["Is Pass Calibration"].ToString() == "fail")
                                row["Overall Result"] = "Fail CalVer";
                            else if (row["Is Pass Drift"].ToString() == "fail")
                                row["Overall Result"] = "Fail Drift";
                            else if (row["Is Pass BKGD Test"].ToString() == "fail")
                                row["Overall Result"] = "Fail BKGD";

                            this.failNo++;
                        }
                    }

                    return dt;
                }
                else
                {
                    return null;
                }

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
            myDT.Columns.Add("#Outlier", typeof(string));
            myDT.Columns.Add("%Pass", typeof(string));
            myDT.Columns.Add("%Fail", typeof(string));
            //myDT.Columns.Add("%Missing Read", typeof(string));

            dtr = myDT.NewRow();

            int deviceTotal = passNo + failNo + outlierNo;
            int passFailTotal = passNo + failNo;

            dtr["#Devices"] = deviceTotal;
            dtr["#Pass"] = passNo;
            dtr["#Fail"] = failNo;
            dtr["#Outlier"] = outlierNo;
            dtr["%Pass"] = (passFailTotal > 0) ? Math.Round(Convert.ToDouble(passNo) / Convert.ToDouble(passFailTotal) * 100, 2).ToString() : "";
            dtr["%Fail"] = (passFailTotal > 0) ? Math.Round(Convert.ToDouble(failNo) / Convert.ToDouble(passFailTotal) * 100, 2).ToString() : "";
            //dtr["%Missing Read"] = Math.Round (Convert.ToDouble(outlierNo) / Convert.ToDouble(deviceTotal) * 100, 2);
            // Add the row to the DataTable.
            myDT.Rows.Add(dtr);

            grdStatisticResultView.DataSource = myDT;
            grdStatisticResultView.DataBind();
        }

        protected void grdOverallAnalysisView_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                switch (e.Row.Cells[1].Text.ToLower())
                {
                    case "pass":
                        e.Row.Cells[1].CssClass = "pass";
                        e.Row.Cells[1].Text = "";
                        break;
                    case "outlier":
                        e.Row.Cells[1].CssClass = "outlier";
                        e.Row.Cells[1].Text = "";
                        break;
                    case "fail":
                        e.Row.Cells[1].CssClass = "fail";
                        e.Row.Cells[1].Text = "";
                        break;
                    default:
                        e.Row.Cells[1].CssClass = "outlier";
                        e.Row.Cells[1].Text = "";
                        break;
                }

                switch (e.Row.Cells[2].Text.ToLower())
                {
                    case "pass":
                        e.Row.Cells[2].CssClass = "pass";
                        e.Row.Cells[2].Text = "";
                        break;
                    case "outlier":
                        e.Row.Cells[2].CssClass = "outlier";
                        e.Row.Cells[2].Text = "";
                        break;
                    case "fail":
                        e.Row.Cells[2].CssClass = "fail";
                        e.Row.Cells[2].Text = "";
                        break;
                    default:
                        e.Row.Cells[2].CssClass = "outlier";
                        e.Row.Cells[2].Text = "";
                        break;
                }

                switch (e.Row.Cells[3].Text.ToLower())
                {
                    case "pass":
                        e.Row.Cells[3].CssClass = "pass";
                        e.Row.Cells[3].Text = "";
                        break;
                    case "outlier":
                        e.Row.Cells[3].CssClass = "outlier";
                        e.Row.Cells[3].Text = "";
                        break;
                    case "fail":
                        e.Row.Cells[3].CssClass = "fail";
                        e.Row.Cells[3].Text = "";
                        break;
                    default:
                        e.Row.Cells[3].CssClass = "outlier";
                        e.Row.Cells[3].Text = "";
                        break;
                }

                switch (e.Row.Cells[4].Text.ToLower())
                {
                    case "pass":
                        e.Row.Cells[4].CssClass = "pass";
                        e.Row.Cells[4].Text = "";
                        break;
                    case "outlier":
                        e.Row.Cells[4].CssClass = "outlier";
                        e.Row.Cells[4].Text = "";
                        break;
                    case "fail calver":
                        e.Row.Cells[4].CssClass = "fail";
                        e.Row.Cells[4].Text = "&nbsp;&nbsp;&nbsp;&nbsp;(Fail CalVer)";
                        break;
                    case "fail bkgd":
                        e.Row.Cells[4].CssClass = "fail";
                        e.Row.Cells[4].Text = "&nbsp;&nbsp;&nbsp;&nbsp;(Fail BKGD)";
                        break;
                    case "fail drift":
                        e.Row.Cells[4].CssClass = "fail";
                        e.Row.Cells[4].Text = "&nbsp;&nbsp;&nbsp;&nbsp;(Fail Drift)";
                        break;
                    default:
                        e.Row.Cells[4].CssClass = "outlier";
                        e.Row.Cells[4].Text = "";
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