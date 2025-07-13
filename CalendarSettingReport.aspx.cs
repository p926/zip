/*
 * Maintain Cal Reads Calendar Setting
 * 
 *  Created By: Tdo, 07/22/2013
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

public partial class TechOps_CalendarSettingReport : System.Web.UI.Page
{
    // Turn on/off email to all others
    bool DevelopmentServer = false;

    InsDataContext idc = new InsDataContext();
    string UserName = "Unknown";
    Int32 scheduleReadGroupID = 0;
    private DeviceInventory _device = null;
    private ScheduleReadGroup _scheduleReadGroup = null;
    private const int DefaultExtendDateRange = 2;            // Rules: Extend date range for accepting scheduled reads.
    private Hashtable statisticHash = new Hashtable();      // Used to store #Pass, #Fail, #Not Read for each ReadType

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            // Auto set if a development site
            if (Request.Url.Authority.ToString().Contains(".mirioncorp.com"))
                DevelopmentServer = true;

            this.UserName = User.Identity.Name.Split('\\')[1];

            int.TryParse(Request.QueryString["ScheduleReadGroupID"], out scheduleReadGroupID);

            if (scheduleReadGroupID == 0)
            {
                Page.Response.Redirect("CalendarSetting.aspx");
                return;
            }

            if (!Page.IsPostBack)
            {
                DisplayAllData();
            }

        }
        catch { this.UserName = "Unknown"; }

    }

    private void DisplayAllData()
    {
        try
        {
            //bool newDataProcess = Convert.ToBoolean((from a in idc.AppSettings where a.AppKey == "NewCalendarSettingResultProcess" select a.Value).FirstOrDefault());

            int scheduledReadsCount = (from a in idc.ScheduleDeviceReads 
                                       join b in idc.ScheduleReadGroups on a.ScheduleID equals b.ScheduleID
                                       where b.ScheduleReadGroupID == scheduleReadGroupID                                       
                                       select a).Count();

            DisplayScheduleInfo();

            if (scheduledReadsCount > 0)
            {
                grdScheduleReadGroupsView.DataSource = GetScheduleReadsReport_New(scheduleReadGroupID);
                grdScheduleReadGroupsView.DataBind();

                DisplayStatisticNumber_New();                
            }
            else
            {
                grdScheduleReadGroupsView.DataSource = GetScheduleReadsReport(scheduleReadGroupID);
                grdScheduleReadGroupsView.DataBind();

                DisplayStatisticNumber();
            }           
        }
        catch { }
    }

    protected void grdScheduleReadGroupsView_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            for (int i = 1; i < e.Row.Cells.Count; i++)
            {
                switch (e.Row.Cells[i].Text.ToLower())
                {
                    case "pass":
                        e.Row.Cells[i].CssClass = "pass";
                        e.Row.Cells[i].Text = "";
                        break;
                    case "processing":
                        e.Row.Cells[i].CssClass = "processing";
                        e.Row.Cells[i].Text = "";
                        break;
                    case "fail":
                        e.Row.Cells[i].CssClass = "fail";
                        e.Row.Cells[i].Text = "";
                        break;
                    default:
                        e.Row.Cells[i].CssClass = "processing";
                        e.Row.Cells[i].Text = "";
                        break;
                }
            }
        }
    }

    private DataTable GetScheduleReadsReport(int pScheduleReadGroupID)
    {
        try
        {
            String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            String cmdStr = "sp_GetDevicesAndReadTypesByScheduleReadGroup";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;

            sqlCmd.Parameters.Add("@ScheduleReadGroupID", SqlDbType.Int);
            sqlCmd.Parameters["@ScheduleReadGroupID"].Value = pScheduleReadGroupID;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);
            sqlConn.Close();

            // -------------------------------------------- Populate the DataTable ------------------------------------------------//

            DateTime scheduledStartDate, scheduledEndDateRange, acceptableScheduledEndDateRange;    // scheduled Startdatetime and Enddatetime range for all scheduled reads on a device                       
            string curSerialNo;
            string curReadType;
            ArrayList columns = new ArrayList();    // contain serialno + all readtypes

            foreach (DataColumn col in dt.Columns)
            {
                columns.Add(col.ColumnName);
            }

            foreach (DataRow row in dt.Rows)
            {
                curSerialNo = row[0].ToString();

                // Get the device.
                _device = (from di in idc.DeviceInventories where di.SerialNo == curSerialNo select di).FirstOrDefault();

                scheduledEndDateRange = _device.ScheduleTimeOffset.HasValue ? _scheduleReadGroup.Schedule.ScheduleEndDate.AddMinutes(_device.ScheduleTimeOffset.Value) : _scheduleReadGroup.Schedule.ScheduleEndDate;
                scheduledEndDateRange = TimeZoneInfo.ConvertTimeFromUtc(scheduledEndDateRange, TimeZoneInfo.Local);
                // allow to accept some reads behind schedule in certain amount of days
                acceptableScheduledEndDateRange = scheduledEndDateRange.AddDays(DefaultExtendDateRange);

                // go through each readtype to get the read result
                for (int i = 1; i < columns.Count; i++)
                {
                    curReadType = columns[i].ToString();
                    scheduledStartDate = GetDeviceScheduledStartDate(_device, curReadType);
                    row[i] = GetScheduledReadResult(curSerialNo, curReadType, scheduledStartDate, acceptableScheduledEndDateRange);
                }
            }

            // -------------------------------------------- Populate the DataTable ------------------------------------------------//

            return dt;
        }
        catch { return null; }
    }

    private DataTable GetScheduleReadsReport_New(int pScheduleReadGroupID)
    {
        try
        {
            String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            String cmdStr = "sp_GenerateScheduledDeviceReadsResultByScheduleReadGroup";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;

            sqlCmd.Parameters.Add("@ScheduleReadGroupID", SqlDbType.Int);
            sqlCmd.Parameters["@ScheduleReadGroupID"].Value = pScheduleReadGroupID;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);
            sqlConn.Close();            

            return dt;
        }
        catch { return null; }
    }

    /// <summary>
    /// Get the date and time a device scheduled to read
    /// </summary>
    /// <param name="pDevice"></param>
    /// <param name="pReadType"></param>
    /// <returns></returns>
    private DateTime GetDeviceScheduledStartDate(DeviceInventory pDevice, string pReadType)
    {
        ScheduleReadGroup myScheduleReadGroup = (from srg in idc.ScheduleReadGroups
                                                 join rt in idc.ReadTypes on srg.ReadTypeID equals rt.ReadTypeID
                                                 where srg.ScheduleID == _scheduleReadGroup.ScheduleID
                                                        && rt.ReadTypeName == pReadType
                                                 select srg).FirstOrDefault();
        DateTime startDateTime = DateTime.Parse(myScheduleReadGroup.StartDate.ToShortDateString() + " " + myScheduleReadGroup.StartTime);
        startDateTime = pDevice.ScheduleTimeOffset.HasValue ? startDateTime.AddMinutes(pDevice.ScheduleTimeOffset.Value) : startDateTime;
        return startDateTime;
    }

    /// <summary>
    /// Check if scheduled readtype for a device is read 
    /// </summary>
    /// <param name="pSerialNo"></param>
    /// <param name="pReadType"></param>
    /// <param name="pStartDateTime"></param>
    /// <param name="pAcceptableScheduledEndDateRange"></param>
    /// <returns></returns>
    private string GetScheduledReadResult(string pSerialNo, string pReadType, DateTime pStartDateTime, DateTime pAcceptableScheduledEndDateRange)
    {
        int count = (from udr in idc.UserDeviceReads
                     join di in idc.DeviceInventories on udr.DeviceID equals di.DeviceID
                     join rt in idc.ReadTypes on udr.ReadTypeID equals rt.ReadTypeID
                     where di.SerialNo == pSerialNo
                            && rt.ReadTypeName == pReadType
                            && udr.DeviceReadAppID == 2
                            && udr.CreatedDate >= pStartDateTime
                            && udr.CreatedDate <= pAcceptableScheduledEndDateRange
                     select udr).Count();

        if (!this.statisticHash.ContainsKey(pReadType))
        {
            // status[0] : Processing
            // status[1] : Pass
            // status[2] : Fail
            int[] status = new int[] { 0, 0, 0 };
            this.statisticHash.Add(pReadType, status);
        }

        if (count > 0)
        {
            // cast to type int[]
            (this.statisticHash[pReadType] as int[])[1] = (this.statisticHash[pReadType] as int[])[1] + 1;
            return "Pass";
        }
        else
        {
            // cast to type int[]
            (this.statisticHash[pReadType] as int[])[0] = (this.statisticHash[pReadType] as int[])[0] + 1;
            return "Processing";
        }
    }

    private void PassFailCounts(int pScheduleReadGroupID)
    {
        String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
        String cmdStr = "sp_GetScheduledDeviceReadsByScheduleReadGroup";

        SqlConnection sqlConn = new SqlConnection(connStr);
        SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

        sqlConn.Open();

        sqlCmd.CommandType = CommandType.StoredProcedure;

        sqlCmd.Parameters.Add("@ScheduleReadGroupID", SqlDbType.Int);
        sqlCmd.Parameters["@ScheduleReadGroupID"].Value = pScheduleReadGroupID;

        SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
        DataTable dt = new DataTable();

        sqlDA.Fill(dt);
        sqlConn.Close();

        string readType = "";
        string result = "";

        foreach (DataRow row in dt.Rows)
        {
            readType = row[1].ToString();
            result = row[2].ToString();

            if (!this.statisticHash.ContainsKey(readType))
            {
                // status[0] : Processing
                // status[1] : Pass
                // status[2] : Fail
                int[] status = new int[] { 0, 0, 0 };
                this.statisticHash.Add(readType, status);
            }

            if (result.ToUpper() == "PASS")
            {
                // cast to type int[]
                (this.statisticHash[readType] as int[])[1] = (this.statisticHash[readType] as int[])[1] + 1;
            }
            else
            {
                // cast to type int[]
                (this.statisticHash[readType] as int[])[0] = (this.statisticHash[readType] as int[])[0] + 1;
            }            
        }        
    }

    private void DisplayStatisticNumber()
    {       
        DataTable myDT;
        DataRow dtr;

        myDT = new DataTable();
        myDT.Columns.Add("ReadType", typeof(string));
        myDT.Columns.Add("#Pass", typeof(string));
        myDT.Columns.Add("#Fail", typeof(string));
        myDT.Columns.Add("#Not Read", typeof(string));

        foreach (DictionaryEntry entry in statisticHash)
        {
            dtr = myDT.NewRow();

            dtr["ReadType"] = entry.Key;
            dtr["#Pass"] = (entry.Value as int[])[1];
            dtr["#Fail"] = (entry.Value as int[])[2];
            dtr["#Not Read"] = (entry.Value as int[])[0];
            // Add the row to the DataTable.
            myDT.Rows.Add(dtr);
        }

        grdStatisticResultView.DataSource = myDT;
        grdStatisticResultView.DataBind();
    }

    private void DisplayStatisticNumber_New()
    {
        PassFailCounts(scheduleReadGroupID);

        DataTable myDT;
        DataRow dtr;

        myDT = new DataTable();
        myDT.Columns.Add("ReadType", typeof(string));
        myDT.Columns.Add("#Pass", typeof(string));
        myDT.Columns.Add("#Fail", typeof(string));
        myDT.Columns.Add("#Not Read", typeof(string));

        foreach (DictionaryEntry entry in statisticHash)
        {
            dtr = myDT.NewRow();

            dtr["ReadType"] = entry.Key;
            dtr["#Pass"] = (entry.Value as int[])[1];
            dtr["#Fail"] = (entry.Value as int[])[2];
            dtr["#Not Read"] = (entry.Value as int[])[0];
            // Add the row to the DataTable.
            myDT.Rows.Add(dtr);
        }

        grdStatisticResultView.DataSource = myDT;
        grdStatisticResultView.DataBind();
    }

    private void DisplayScheduleInfo()
    {
        _scheduleReadGroup = (from srg in idc.ScheduleReadGroups
                              where srg.ScheduleReadGroupID == this.scheduleReadGroupID
                              select srg).FirstOrDefault();
        string myGroup = _scheduleReadGroup.DeviceGroup.DeviceGroupName;
        string myScheduleName = _scheduleReadGroup.Schedule.ScheduleName;
        string myStartDate = _scheduleReadGroup.StartDate.ToShortDateString();
        string myStartTime = _scheduleReadGroup.StartTime;

        group.Text = myGroup;
        scheduleName.Text = myScheduleName;
        startDate.Text = myStartDate;
        startTime.Text = myStartTime;

    }

    protected void btnBack_Click(object sender, EventArgs e)
    {
        Page.Response.Redirect("CalendarSetting.aspx");
    }

    protected void btn_refresh_Click(object sender, EventArgs e)
    {
        DisplayAllData();
    }
}