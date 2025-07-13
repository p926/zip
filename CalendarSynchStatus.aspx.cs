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

public partial class TechOps_CalendarSynchStatus : System.Web.UI.Page
{
    // Turn on/off email to all others
    bool DevelopmentServer = false;

    InsDataContext idc = new InsDataContext();
    string UserName = "Unknown";
    Int32 scheduleReadGroupID = 0;
    private DeviceInventory _device = null;
    private ScheduleReadGroup _scheduleReadGroup = null;
    private Hashtable statisticHash = new Hashtable();
    private int noSynched = 0;
    private int noNotSynched = 0;

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
            DisplayScheduleInfo();

            DisplayGridByReceiptID(scheduleReadGroupID);

            DisplayStatisticNumber();
        }
        catch { }
    }

    protected void grdScheduleReadGroupsView_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            if (e.Row.Cells[2].Text.ToLower() == "complete")
            {
                e.Row.Cells[2].CssClass = "synch";
                e.Row.Cells[2].Text = "";
                noSynched++;
            }
            else
            {
                e.Row.Cells[2].CssClass = "notsynch";
                e.Row.Cells[2].Text = "";
                noNotSynched++;
            }
        }
    }

    private void DisplayGridByReceiptID(int pScheduleReadGroupID)
    {

        IQueryable pth = from a in idc.ScheduleReadGroups
                         join di in idc.DeviceInventories on a.DeviceGroupID equals di.InitialGroupID
                         where a.ScheduleReadGroupID == pScheduleReadGroupID
                         select new
                         {
                             di.SerialNo,
                             di.ScheduleSyncDate,
                             di.ScheduleSyncStatus
                         };

        grdScheduleReadGroupsView.DataSource = pth;
        grdScheduleReadGroupsView.DataBind();
    }

    private void DisplayStatisticNumber()
    {
        DataTable myDT;
        DataRow dtr;

        myDT = new DataTable();
        myDT.Columns.Add("#Synched", typeof(string));
        myDT.Columns.Add("#Not Synched", typeof(string));

        dtr = myDT.NewRow();

        dtr["#Synched"] = this.noSynched;
        dtr["#Not Synched"] = this.noNotSynched;

        // Add the row to the DataTable.
        myDT.Rows.Add(dtr);

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