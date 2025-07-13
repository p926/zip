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

public partial class TechOps_CalendarSetting : System.Web.UI.Page
{
    // Turn on/off email to all others
    bool DevelopmentServer = false;

    //const int readPer15Sec = 1;
    //const int readInterval = 15;
    const int readPerMinute = 10;
    InsDataContext idc = new InsDataContext();    
    DataTable dtCalendarDetail;   // DataTable to store the calendar detail input    
    string UserName = "Unknown";
    ListItemCollection startTimeItems = new ListItemCollection();

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            // Auto set if a development site
            if (Request.Url.Authority.ToString().Contains(".mirioncorp.com"))
                DevelopmentServer = true;

            this.UserName = User.Identity.Name.Split('\\')[1];
            PopulateStartTimeCollection();

            if (!this.IsPostBack)
            {                
                InitiateAllControls();
                SetControlsDefault();               
            }
            
        }
        catch { this.UserName = "Unknown"; }
        
    }

    private void SetControlsDefault()
    {        
    }

    private void InitiateAllControls()
    {
        InitiateGroupsControl();        
    }

    private void PopulateStartTimeCollection()
    {
        startTimeItems.Add(new ListItem("", ""));
        startTimeItems.Add(new ListItem("12:00 AM", "12:00 AM"));
        startTimeItems.Add(new ListItem("12:15 AM", "12:15 AM"));
        startTimeItems.Add(new ListItem("12:30 AM", "12:30 AM"));
        startTimeItems.Add(new ListItem("12:45 AM", "12:45 AM"));
        startTimeItems.Add(new ListItem("1:00 AM", "1:00 AM"));
        startTimeItems.Add(new ListItem("1:15 AM", "1:15 AM"));
        startTimeItems.Add(new ListItem("1:30 AM", "1:30 AM"));
        startTimeItems.Add(new ListItem("1:45 AM", "1:45 AM"));
        startTimeItems.Add(new ListItem("2:00 AM", "2:00 AM"));
        startTimeItems.Add(new ListItem("2:15 AM", "2:15 AM"));
        startTimeItems.Add(new ListItem("2:30 AM", "2:30 AM"));
        startTimeItems.Add(new ListItem("2:45 AM", "2:45 AM"));
        startTimeItems.Add(new ListItem("3:00 AM", "3:00 AM"));
        startTimeItems.Add(new ListItem("3:15 AM", "3:15 AM"));
        startTimeItems.Add(new ListItem("3:30 AM", "3:30 AM"));
        startTimeItems.Add(new ListItem("3:45 AM", "3:45 AM"));
        startTimeItems.Add(new ListItem("4:00 AM", "4:00 AM"));
        startTimeItems.Add(new ListItem("4:15 AM", "4:15 AM"));
        startTimeItems.Add(new ListItem("4:30 AM", "4:30 AM"));
        startTimeItems.Add(new ListItem("4:45 AM", "4:45 AM"));
        startTimeItems.Add(new ListItem("5:00 AM", "5:00 AM"));
        startTimeItems.Add(new ListItem("5:15 AM", "5:15 AM"));
        startTimeItems.Add(new ListItem("5:30 AM", "5:30 AM"));
        startTimeItems.Add(new ListItem("5:45 AM", "5:45 AM"));
        startTimeItems.Add(new ListItem("6:00 AM", "6:00 AM"));
        startTimeItems.Add(new ListItem("6:15 AM", "6:15 AM"));
        startTimeItems.Add(new ListItem("6:30 AM", "6:30 AM"));
        startTimeItems.Add(new ListItem("6:45 AM", "6:45 AM"));
        startTimeItems.Add(new ListItem("7:00 AM", "7:00 AM"));
        startTimeItems.Add(new ListItem("7:15 AM", "7:15 AM"));
        startTimeItems.Add(new ListItem("7:30 AM", "7:30 AM"));
        startTimeItems.Add(new ListItem("7:45 AM", "7:45 AM"));
        startTimeItems.Add(new ListItem("8:00 AM", "8:00 AM"));
        startTimeItems.Add(new ListItem("8:15 AM", "8:15 AM"));
        startTimeItems.Add(new ListItem("8:30 AM", "8:30 AM"));
        startTimeItems.Add(new ListItem("8:45 AM", "8:45 AM"));
        startTimeItems.Add(new ListItem("9:00 AM", "9:00 AM"));
        startTimeItems.Add(new ListItem("9:15 AM", "9:15 AM"));
        startTimeItems.Add(new ListItem("9:30 AM", "9:30 AM"));
        startTimeItems.Add(new ListItem("9:45 AM", "9:45 AM"));
        startTimeItems.Add(new ListItem("10:00 AM", "10:00 AM"));
        startTimeItems.Add(new ListItem("10:15 AM", "10:15 AM"));
        startTimeItems.Add(new ListItem("10:30 AM", "10:30 AM"));
        startTimeItems.Add(new ListItem("10:45 AM", "10:45 AM"));
        startTimeItems.Add(new ListItem("11:00 AM", "11:00 AM"));
        startTimeItems.Add(new ListItem("11:15 AM", "11:15 AM"));
        startTimeItems.Add(new ListItem("11:30 AM", "11:30 AM"));
        startTimeItems.Add(new ListItem("11:45 AM", "11:45 AM"));
        startTimeItems.Add(new ListItem("12:00 PM", "12:00 PM"));
        startTimeItems.Add(new ListItem("12:15 PM", "12:15 PM"));
        startTimeItems.Add(new ListItem("12:30 PM", "12:30 PM"));
        startTimeItems.Add(new ListItem("12:45 PM", "12:45 PM"));
        startTimeItems.Add(new ListItem("1:00 PM", "1:00 PM"));
        startTimeItems.Add(new ListItem("1:15 PM", "1:15 PM"));
        startTimeItems.Add(new ListItem("1:30 PM", "1:30 PM"));
        startTimeItems.Add(new ListItem("1:45 PM", "1:45 PM"));
        startTimeItems.Add(new ListItem("2:00 PM", "2:00 PM"));
        startTimeItems.Add(new ListItem("2:15 PM", "2:15 PM"));
        startTimeItems.Add(new ListItem("2:30 PM", "2:30 PM"));
        startTimeItems.Add(new ListItem("2:45 PM", "2:45 PM"));
        startTimeItems.Add(new ListItem("3:00 PM", "3:00 PM"));
        startTimeItems.Add(new ListItem("3:15 PM", "3:15 PM"));
        startTimeItems.Add(new ListItem("3:30 PM", "3:30 PM"));
        startTimeItems.Add(new ListItem("3:45 PM", "3:45 PM"));
        startTimeItems.Add(new ListItem("4:00 PM", "4:00 PM"));
        startTimeItems.Add(new ListItem("4:15 PM", "4:15 PM"));
        startTimeItems.Add(new ListItem("4:30 PM", "4:30 PM"));
        startTimeItems.Add(new ListItem("4:45 PM", "4:45 PM"));
        startTimeItems.Add(new ListItem("5:00 PM", "5:00 PM"));
        startTimeItems.Add(new ListItem("5:15 PM", "5:15 PM"));
        startTimeItems.Add(new ListItem("5:30 PM", "5:30 PM"));
        startTimeItems.Add(new ListItem("5:45 PM", "5:45 PM"));
        startTimeItems.Add(new ListItem("6:00 PM", "6:00 PM"));
        startTimeItems.Add(new ListItem("6:15 PM", "6:15 PM"));
        startTimeItems.Add(new ListItem("6:30 PM", "6:30 PM"));
        startTimeItems.Add(new ListItem("6:45 PM", "6:45 PM"));
        startTimeItems.Add(new ListItem("7:00 PM", "7:00 PM"));
        startTimeItems.Add(new ListItem("7:15 PM", "7:15 PM"));
        startTimeItems.Add(new ListItem("7:30 PM", "7:30 PM"));
        startTimeItems.Add(new ListItem("7:45 PM", "7:45 PM"));
        startTimeItems.Add(new ListItem("8:00 PM", "8:00 PM"));
        startTimeItems.Add(new ListItem("8:15 PM", "8:15 PM"));
        startTimeItems.Add(new ListItem("8:30 PM", "8:30 PM"));
        startTimeItems.Add(new ListItem("8:45 PM", "8:45 PM"));
        startTimeItems.Add(new ListItem("9:00 PM", "9:00 PM"));
        startTimeItems.Add(new ListItem("9:15 PM", "9:15 PM"));
        startTimeItems.Add(new ListItem("9:30 PM", "9:30 PM"));
        startTimeItems.Add(new ListItem("9:45 PM", "9:45 PM"));
        startTimeItems.Add(new ListItem("10:00 PM", "10:00 PM"));
        startTimeItems.Add(new ListItem("10:15 PM", "10:15 PM"));
        startTimeItems.Add(new ListItem("10:30 PM", "10:30 PM"));
        startTimeItems.Add(new ListItem("10:45 PM", "10:45 PM"));
        startTimeItems.Add(new ListItem("11:00 PM", "11:00 PM"));
        startTimeItems.Add(new ListItem("11:15 PM", "11:15 PM"));
        startTimeItems.Add(new ListItem("11:30 PM", "11:30 PM"));
        startTimeItems.Add(new ListItem("11:45 PM", "11:45 PM"));
    }

    private void InitiateGroupsControl()
    {        
        var groups = from r in idc.DeviceGroups
                     orderby r.DeviceGroupID descending
                     select new
                     {
                         r.DeviceGroupID,
                         r.DeviceGroupName
                     };

        this.ddlDeviceGroup.DataSource = groups;
        this.ddlDeviceGroup.DataTextField = "DeviceGroupName";
        this.ddlDeviceGroup.DataValueField = "DeviceGroupID";
        this.ddlDeviceGroup.DataBind();

        ListItem item0 = new ListItem("", "0");
        this.ddlDeviceGroup.Items.Insert(0, item0);
    }
      
    // ------------------------- Modal Dialog Section FUNCTIONS here ------------------------------------- //

    private void InvisibleErrors()
    {
        // Reset submission form error message      
        this.DialogErrorMsg.InnerText = "";
        this.DialogError.Visible = false;
    }

    private void VisibleErrors(string error)
    {
        this.DialogErrorMsg.InnerText = error;
        this.DialogError.Visible = true;
    }

    private void InvisibleMessages()
    {
        // Reset submission form error message      
        this.submitMsg.InnerText = "";
        this.messages.Visible = false;
    }

    private void VisibleMessages(string error)
    {
        this.submitMsg.InnerText = error;
        this.messages.Visible = true;
    }

    private bool passInputsValidation_ScheduleReadGroupDialog()
    {
        string errorString = "";
        
        if (this.ddlDeviceGroup.SelectedItem.Text == "")
        {
            errorString = "Group is required.";
            this.VisibleErrors(errorString);
            SetFocus(this.ddlDeviceGroup);
            return false;
        }

        if (this.txtScheduleName.Text.Trim() == "")
        {
            errorString = "Schedule Name is required.";
            this.VisibleErrors(errorString);
            SetFocus(this.txtScheduleName);
            return false;
        }
                      
        if (ValidateCalendarDetailRow(ref errorString) == false)
        {
            this.VisibleErrors(errorString);
            return false;
        }
        
        return true;
    }

    private void EnableControls(bool flag)
    {
        this.ddlDeviceGroup.Enabled = flag;
        this.txtScheduleName.Enabled = flag;
        this.txtScheduleDesc.Enabled = flag;     
        this.grdViewEditDetail.Enabled = flag;
        this.btnAnotherRead.Enabled = flag;        
    }

    private void VisibleControls(bool flag)
    {
        this.btnAnotherRead.Visible = flag;
        this.EditDetail.Visible = flag;        
    }

    private void SetDefaultValues_ScheduleReadGroupDialog()
    {
        this.chkBoxDeActive.Checked = false;
        this.ddlDeviceGroup.SelectedIndex = 0;        
        this.txtScheduleName.Text = "";
        this.txtScheduleDesc.Text = ""; 
        this.grdViewEditDetail.DataSource = null;
        this.grdViewEditDetail.DataBind();                        
    }

    private void SetValuesToControls_ScheduleReadGroupDialog()
    {        
        string scheduleReadGroupID = (Session["selectedScheduleReadGroupID"] != null) ? Convert.ToString (Session["selectedScheduleReadGroupID"]) : "";

        SetFocus(this.ddlDeviceGroup);
        
        if (scheduleReadGroupID.Length > 0)    // edit mode
        {
            ScheduleReadGroup myScheduleReadGroup = (from a in idc.ScheduleReadGroups 
                                     where a.ScheduleReadGroupID == int.Parse(scheduleReadGroupID)
                                     select a).FirstOrDefault();

            EnableControls(false);

            if (IsScheduleAlreadyPerformed(int.Parse(scheduleReadGroupID)))
                this.chkBoxDeActive.Enabled = false;
            else
                this.chkBoxDeActive.Enabled = true;

            this.chkBoxDeActive.Visible = true;
            this.btnAnotherRead.Visible = false;
            this.EditDetail.Visible = true;            

            this.ddlDeviceGroup.SelectedValue = myScheduleReadGroup.DeviceGroupID.ToString() ;            
            this.txtScheduleName.Text = myScheduleReadGroup.Schedule.ScheduleName;
            this.txtScheduleDesc.Text = myScheduleReadGroup.Schedule.ScheduleDesc ;   

            SetDetailGridByscheduleReadGroupID(int.Parse(scheduleReadGroupID));
            
        }
        else
        {           

            EnableControls(true);

            this.chkBoxDeActive.Visible = false;            

            VisibleControls(true);

            GenerateEditDetailGrid(1);
            
        }

    }

    private bool IsScheduleAlreadyPerformed(int pScheduleReadGroupID)
    {
        ScheduleReadGroup myScheduleReadGroup = (from a in idc.ScheduleReadGroups
                                                 where a.ScheduleReadGroupID == pScheduleReadGroupID
                                                 select a).FirstOrDefault();

        DateTime myScheduleStartDateTime = myScheduleReadGroup.Schedule.ScheduleStartDate;

        if (myScheduleStartDateTime <= DateTime.UtcNow)
            return true;
        else
            return false;
    }

    protected void btnNewScheduleReadGroup_Click(object sender, EventArgs e)
    {        
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('ScheduleReadGroupDialog')", true);        
    }

    protected void btnCancelScheduleReadGroup_Click(object sender, EventArgs e)
    {
        InvisibleErrors();
        InvisibleMessages();

        SetDefaultValues_ScheduleReadGroupDialog();
        // delete session variable
        if (Session["selectedScheduleReadGroupID"] != null)
            Session.Remove("selectedScheduleReadGroupID");
    }

    protected void btnLoadScheduleReadGroup_Click(object sender, EventArgs e)
    {
        SetDefaultValues_ScheduleReadGroupDialog();
        SetValuesToControls_ScheduleReadGroupDialog();
    }

    protected void btnEditScheduleReadGroup_Click(object sender, EventArgs e)
    {
        LinkButton btn = (LinkButton)sender;
        string myCmdname = btn.CommandName.ToString();
        Session["selectedScheduleReadGroupID"] = btn.CommandArgument.ToString();

        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('ScheduleReadGroupDialog')", true);

    }

    protected void btnAddScheduleReadGroup_Click(object sender, EventArgs e)
    {
        InvisibleErrors();
        InvisibleMessages();
        
        bool editMode = (Session["selectedScheduleReadGroupID"] != null) ? true : false;
            
        if (!editMode)  // add new dbo.ScheduleReadGroup
        {
            if (passInputsValidation_ScheduleReadGroupDialog())
            {
                int myDeviceGroupID = int.Parse(this.ddlDeviceGroup.SelectedValue);
                string myScheduleName = this.txtScheduleName.Text.Trim();
                string myScheduleDesc = this.txtScheduleDesc.Text.Trim();
                int myScheduleID = 0;
                List<DateTime> myCalendarDate = new List<DateTime>();

                try
                {
                    // Get a list of scheduled days
                    foreach (GridViewRow rowitem in grdViewEditDetail.Rows)
                    {
                        TextBox findStartDate = (TextBox)rowitem.FindControl("txtStartDate");
                        DropDownList findStartTime = (DropDownList)rowitem.FindControl("ddlStartTime");
                        DropDownList findReadType = (DropDownList)rowitem.FindControl("ddlReadType");                        

                        if (findStartDate.Text.Trim().Length <= 0 && findStartTime.SelectedItem.Text.Trim().Length <= 0 && findReadType.SelectedValue == "0") continue;

                        DateTime startDateTime = DateTime.Parse(findStartDate.Text + " " + findStartTime.SelectedValue);
                        // Convert the date from local to server time.
                        startDateTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(startDateTime, "Pacific Standard Time", TimeZoneInfo.Local.Id);

                        myCalendarDate.Add(startDateTime);
                    }

                    // Create Schedule
                    CalendarManager myCalendarMnager = new CalendarManager();
                    if (myCalendarDate.Count > 0)
                    {
                        myScheduleID = myCalendarMnager.CreateCalReadCalendar(myCalendarDate, myScheduleName, myScheduleDesc);
                    }

                    // Go through each readtype to create ScheduleReadGroup
                    foreach (GridViewRow rowitem in grdViewEditDetail.Rows)
                    {
                        TextBox findStartDate = (TextBox)rowitem.FindControl("txtStartDate");
                        DropDownList findStartTime = (DropDownList)rowitem.FindControl("ddlStartTime");
                        DropDownList findReadType = (DropDownList)rowitem.FindControl("ddlReadType");
                        TextBox findReadPercentage = (TextBox)rowitem.FindControl("txtReadPercentage");

                        if (findStartDate.Text.Trim().Length <= 0 && findStartTime.SelectedItem.Text.Trim().Length <= 0 && findReadType.SelectedValue == "0") continue;

                        DateTime myStartDate = DateTime.Parse(findStartDate.Text);
                        String myStartTime = findStartTime.SelectedValue;
                        String myReadPercentage = findReadPercentage.Text;
                        int myReadTypeID = int.Parse(findReadType.SelectedValue);

                        ScheduleReadGroup rec = new ScheduleReadGroup
                        {
                            DeviceGroupID = myDeviceGroupID,
                            ReadTypeID = myReadTypeID,
                            ScheduleID = myScheduleID,
                            StartDate = myStartDate,
                            StartTime = myStartTime,
                            ReadPercentage = (myReadPercentage.Length == 0) ? (int?)null : int.Parse(myReadPercentage)
                        };
                        idc.ScheduleReadGroups.InsertOnSubmit(rec);
                        idc.SubmitChanges();
                    }

                    // Set scheduledID for every device in group
                    SetScheduleForGroup(myScheduleID, myDeviceGroupID);

                    // refresh gridview
                    grdScheduleReadGroupsView.DataBind();

                    ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('ScheduleReadGroupDialog')", true);

                    VisibleMessages("Commit successfully!");

                }
                catch (Exception ex)
                {
                    // Display the system generated error message.
                    this.VisibleErrors(string.Format("An error occurred: {0}", ex.Message));
                }
            }                
        }
        else
        {
            try
            {
                // De-activate a calendar setting
                if (this.chkBoxDeActive.Checked)
                {
                    int myDeviceGroupID = int.Parse(this.ddlDeviceGroup.SelectedValue);

                    // Delete a scheduling setup
                    DeleteCalendarSetup(Convert.ToInt32(Session["selectedScheduleReadGroupID"]), myDeviceGroupID);

                    // refresh gridview
                    grdScheduleReadGroupsView.DataBind();

                    VisibleMessages("Delete successfully!");
                }

                // delete session variable
                if (Session["selectedScheduleReadGroupID"] != null)
                    Session.Remove("selectedScheduleReadGroupID");
                                        
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('ScheduleReadGroupDialog')", true);                                        

            }
            catch (Exception ex)
            {
                // Display the system generated error message.
                this.VisibleErrors(string.Format("An error occurred: {0}", ex.Message));
            }

        }        

    }

    // ------------------------- Modal Dialog Section FUNCTIONS here ------------------------------------- //

    /// <summary>
    /// By Craig, if a device is already assigned to an account, do not allow to unassign calendar from device by TechOps.
    /// </summary>
    /// <param name="pScheduleReadGroupID"></param>
    /// <param name="pDeviceGroupID"></param>
    private void DeleteCalendarSetup(int pScheduleReadGroupID, int pDeviceGroupID)
    {
        ScheduleReadGroup myScheduleReadGroup = (from a in idc.ScheduleReadGroups
                                                 where a.ScheduleReadGroupID == pScheduleReadGroupID
                                                 select a).FirstOrDefault();        
       
        List<DeviceInventory> myDevices = (from a in idc.DeviceInventories
                                           where a.InitialGroupID == pDeviceGroupID
                                           select a).ToList();

        List<int> myAssignedDevices = (from a in idc.DeviceInventories
                                       join b in idc.AccountDevices on a.DeviceID equals b.DeviceID
                                       where a.InitialGroupID == pDeviceGroupID && b.CurrentDeviceAssign == true
                                       select a.DeviceID).ToList();
        
        CalendarManager myCalendarMnager = new CalendarManager();

        // Unassign a Schedule from devices
        if (myAssignedDevices.Count > 0)
        {
            foreach (DeviceInventory device in myDevices)
            {
                if (!myAssignedDevices.Contains(device.DeviceID))
                {
                    myCalendarMnager.UnassignCalendarFromDevice(device.DeviceID);
                }                
            }
        }
        else
        {
            foreach (DeviceInventory device in myDevices)
            {
                myCalendarMnager.UnassignCalendarFromDevice(device.DeviceID);
            }
        }        

        // De-activate the schedule
        Schedule schedule = (from s in idc.Schedules where s.ScheduleID == myScheduleReadGroup.ScheduleID select s).FirstOrDefault();
        schedule.Active = false;

        // Save the changes.
        idc.SubmitChanges();
    }

    /// <summary>
    /// By Craig, if a device is already assigned to an account, do not overide a new schedule for it by TechOps.
    /// </summary>
    /// <param name="pScheduleID"></param>
    /// <param name="pDeviceGroupID"></param>
    private void SetScheduleForGroup(int pScheduleID, int pDeviceGroupID)
    {
        List<DeviceInventory> myDevices = (from a in idc.DeviceInventories
                                           where a.InitialGroupID == pDeviceGroupID
                                           select a).ToList();

        List<int> myAssignedDevices = (from a in idc.DeviceInventories
                                             join b in idc.AccountDevices on a.DeviceID equals b.DeviceID 
                                            where a.InitialGroupID == pDeviceGroupID && b.CurrentDeviceAssign == true
                                           select a.DeviceID).ToList();
       
        int count = 0;
        int timeOffset = 0; // the unit is in seconds

        if (myAssignedDevices.Count > 0)
        {
            foreach (DeviceInventory device in myDevices)
            {
                if (!myAssignedDevices.Contains(device.DeviceID))
                {
                    device.ScheduleID = pScheduleID;
                    device.ScheduleTimeOffset = (timeOffset == 0) ? (int?)null : timeOffset;
                    device.ScheduleSyncDate = null;
                    device.ScheduleSyncStatus = "PENDING";

                    count++;

                    if (count % readPerMinute == 0)
                    {
                        timeOffset++;
                    }
                }            
            }
        }
        else
        {
            foreach (DeviceInventory device in myDevices)
            {
                device.ScheduleID = pScheduleID;
                device.ScheduleTimeOffset = (timeOffset == 0) ? (int?)null : timeOffset;
                device.ScheduleSyncDate = null;
                device.ScheduleSyncStatus = "PENDING";

                count++;

                if (count % readPerMinute == 0)
                {
                    timeOffset++;
                }
            }
        }
        
        idc.SubmitChanges();
    }

    private void SetDetailGridByscheduleReadGroupID(int pScheduleReadGroupID)
    {
        ScheduleReadGroup myScheduleReadGroup = (from a in idc.ScheduleReadGroups
                                                 where a.ScheduleReadGroupID == pScheduleReadGroupID
                                                 select a).FirstOrDefault();

        IQueryable pth = from a in idc.ScheduleReadGroups
                         where a.ScheduleID == myScheduleReadGroup.ScheduleID 
                         select new
                         {
                             a.ReadTypeID,
                             StartDate = a.StartDate.ToShortDateString() ,
                             a.StartTime,
                             a.ReadPercentage
                         };

        grdViewEditDetail.DataSource = pth;
        grdViewEditDetail.DataBind();
    }

    private bool ValidateCalendarDetailRow(ref string pMessage)
    {
        string curReadTypeID;
        string curStartDate;
        string curStartTime;
        string curReadPercentage;
        int percentage;
        DateTime myDate;
        int noCalendarDate = 0;
        List<string> ScheduledDateTimeList = new List<string>();

        try
        {
            foreach (GridViewRow rowitem in grdViewEditDetail.Rows)
            {
                TextBox findStartDate = (TextBox)rowitem.FindControl("txtStartDate");
                DropDownList findStartTime = (DropDownList)rowitem.FindControl("ddlStartTime");
                TextBox findReadPercentage = (TextBox)rowitem.FindControl("txtReadPercentage");
                DropDownList findReadType = (DropDownList)rowitem.FindControl("ddlReadType");

                curStartDate = findStartDate.Text.Trim();
                curReadTypeID = findReadType.SelectedValue;
                curStartTime = findStartTime.SelectedItem.Text.Trim();
                curReadPercentage = findReadPercentage.Text.Trim();

                if (curStartDate.Length <= 0 && curStartTime.Length <= 0 && curReadTypeID == "0") continue;

                if (curStartDate.Length <= 0 || curStartTime.Length <= 0 || curReadTypeID == "0" )
                {
                    pMessage = "Read Type, Start Date and Start Time must be provided.";
                    return false;
                }
                else
                {
                    if (DateTime.TryParse(curStartDate, out myDate) == false)
                    {                       
                        pMessage = "Start Date is incorrect format.";
                        return false;
                    }

                    if (curReadPercentage.Length > 0)
                    {
                        if (int.TryParse(curReadPercentage, out percentage) == false)
                        {
                            pMessage = "Read Percentage must be a number.";
                            return false;
                        }
                        else
                        {
                            if (percentage <= 0)
                            {
                                pMessage = "Read Percentage must be greater than zero.";
                                return false;
                            }                           
                        }
                    }
                  
                    String curStartDateTime = curStartDate + " " + curStartTime;
                    if (!ScheduledDateTimeList.Contains(curStartDateTime))
                    {
                        ScheduledDateTimeList.Add(curStartDateTime);
                    }
                    else
                    {
                        pMessage = "The schedule date and time: " + curStartDateTime.ToString() + " is duplicated. Please correct it.";
                        return false;
                    }

                    //DateTime curStartDateTime = DateTime.Parse(curStartDate + " " + curStartTime);
                    //DateTime curUTCStartDateTime = curStartDateTime.ToUniversalTime(); //convert to UTC in order to check if the date is already scheduled (ScheduleDates.Date)
                    
                    //int count = (from sd in idc.ScheduleDates
                    //             join s in idc.Schedules on sd.ScheduleID equals s.ScheduleID 
                    //             where s.Active == true
                    //                 && sd.Date == curUTCStartDateTime
                    //                 select sd).Count();
                    //if (count > 0)
                    //{
                    //    pMessage = "The schedule date and time: " + curStartDateTime.ToString() + " is reserved by different schedule. Please select another date and time.";
                    //    return false;
                    //}

                }

                // at here all validation for a row passed
                noCalendarDate++;
            }

            if (noCalendarDate == 0)
            {
                pMessage = "Need at least one scheduled read.";
                return false;
            }
            
            return true;
        }
        catch (Exception ex)
        {
            pMessage = ex.ToString();
            return false;
        }

    }
    
    private void DefineDetailDataTable()
    {
        dtCalendarDetail = new DataTable();       
        dtCalendarDetail.Columns.Add("ReadTypeID", typeof(string));
        dtCalendarDetail.Columns.Add("StartDate", typeof(string));
        dtCalendarDetail.Columns.Add("StartTime", typeof(string));
        dtCalendarDetail.Columns.Add("ReadPercentage", typeof(string));
    }

    protected void btnAnotherRead_Click(object sender, EventArgs e)
    {
        DataRow dtr;
        string curReadTypeID, curReadPercentage;
        string curStartDate = "";
        string curStartTime = "";

        InvisibleErrors();
        InvisibleMessages();

        try
        {

            DefineDetailDataTable();

            foreach (GridViewRow rowitem in grdViewEditDetail.Rows)
            {
                TextBox findStartDate = (TextBox)rowitem.FindControl("txtStartDate");
                DropDownList findStartTime = (DropDownList)rowitem.FindControl("ddlStartTime");
                TextBox findReadPercentage = (TextBox)rowitem.FindControl("txtReadPercentage");
                DropDownList findReadType = (DropDownList)rowitem.FindControl("ddlReadType");

                curReadTypeID = findReadType.SelectedValue;
                curStartDate = findStartDate.Text;
                curStartTime = findStartTime.SelectedItem.Text;
                curReadPercentage = findReadPercentage.Text;

                dtr = dtCalendarDetail.NewRow();

                dtr["ReadTypeID"] = curReadTypeID;
                dtr["StartDate"] = curStartDate; 
                dtr["StartTime"] = curStartTime;
                dtr["ReadPercentage"] = curReadPercentage;
                // Add the row to the DataTable.
                dtCalendarDetail.Rows.Add(dtr);
            }

            // Automatically populate the read time
            string nextStartDate = "";
            string nextStartTime = "";
            if (curStartDate.Length > 0 && curStartTime.Length > 0)
            {
                CalculateNextStartDateStartTime(curStartDate, curStartTime, out nextStartDate, out nextStartTime);
            }
            
            dtr = dtCalendarDetail.NewRow();

            dtr["ReadTypeID"] = "0";
            dtr["StartDate"] = nextStartDate;
            dtr["StartTime"] = nextStartTime;
            dtr["ReadPercentage"] = "";
            // Add the row to the DataTable.
            dtCalendarDetail.Rows.Add(dtr);

            this.grdViewEditDetail.DataSource = dtCalendarDetail;
            this.grdViewEditDetail.DataBind();

        }
        catch (Exception ex)
        {
        }
    }

    /// <summary>
    /// Calculate for the next Start Date and Start Time
    /// </summary>
    /// <param name="pStartDate"></param>
    /// <param name="pStartTime"></param>
    /// <param name="pNextStartDate"></param>
    /// <param name="pNextStartTime"></param>
    private void CalculateNextStartDateStartTime(string pStartDate, string pStartTime, out string pNextStartDate, out string pNextStartTime)
    {
        DateTime startDateTime = DateTime.Parse(pStartDate + " " + pStartTime);

        // Get the number of devices in group, exclude the failed one.
        // By the business logic, if one fails a test the current group will be changed to a different group. Such as fail group.
        int noDivices = (from di in idc.DeviceInventories
                         where di.InitialGroupID == int.Parse(this.ddlDeviceGroup.SelectedValue)
                            && di.InitialGroupID == di.CurrentGroupID 
                            && di.FailedCalibration == false                            
                     select di).Count();

        double noMinutesPerCalendarRead = Convert.ToDouble(noDivices) / Convert.ToDouble(readPerMinute);
        //double noMinutesPerCalendarRead = ((Convert.ToDouble(noDivices) * readInterval) / Convert.ToDouble(readPer15Sec)) / 60;

        noMinutesPerCalendarRead = Math.Ceiling(noMinutesPerCalendarRead);

        DateTime nextStartDateTime = startDateTime.AddMinutes(noMinutesPerCalendarRead);
        string nextStartDate = nextStartDateTime.ToShortDateString();
        int hrs = nextStartDateTime.Hour;
        int mins = nextStartDateTime.Minute;
        DateTime nextStartDateTimeRounded = GetRoundedDateTime(nextStartDate, hrs, mins);
        pNextStartDate = nextStartDateTimeRounded.ToShortDateString();
        pNextStartTime = nextStartDateTimeRounded.ToShortTimeString();
    }

    /// <summary>
    ///  round in every 15'
    /// </summary>
    /// <param name="pNextStartDate"></param>
    /// <param name="hrs"></param>
    /// <param name="mins"></param>
    /// <returns></returns>
    private DateTime GetRoundedDateTime(string pNextStartDate, int hrs, int mins)
    {
        DateTime myDate = DateTime.Parse(pNextStartDate);

        if ((15 - mins) >= 0)
            mins = 15;
        else if ((30 - mins) >= 0)
            mins = 30;
        else if ((45 - mins) >= 0)
            mins = 45;
        else if ((60 - mins) >= 0)
            mins = 60;

        if (mins == 60)
        {
            hrs++;
            mins = 0;
        }

        if (hrs == 24)
        {
            myDate = myDate.AddDays(1);
            hrs = 0;
            mins = 0;
        }

        return myDate.AddHours(hrs).AddMinutes(mins);
    }

    protected void grdViewEditDetail_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            System.Web.UI.WebControls.Label lblReadTypeID = (System.Web.UI.WebControls.Label)e.Row.FindControl("lblReadTypeID");
            DropDownList ddl = (DropDownList)e.Row.FindControl("ddlReadType");

            System.Web.UI.WebControls.Label lblStartTime = (System.Web.UI.WebControls.Label)e.Row.FindControl("lblStartTime");
            DropDownList ddlSTime = (DropDownList)e.Row.FindControl("ddlStartTime");
            
            // POPULATE readtype control
            var readTypes = from r in idc.ReadTypes 
                         orderby r.ReadTypeName ascending 
                         select new
                         {
                             r.ReadTypeID ,
                             r.ReadTypeName
                         };

            ddl.DataSource = readTypes;
            ddl.DataTextField = "ReadTypeName";
            ddl.DataValueField = "ReadTypeID";
            ddl.DataBind();

            ListItem item0 = new ListItem("", "0");
            ddl.Items.Insert(0, item0);

            ddl.SelectedValue = lblReadTypeID.Text;

            // POPULATE StartTime control
            ddlSTime.DataSource = startTimeItems;
            ddlSTime.DataBind();

            ddlSTime.SelectedValue = lblStartTime.Text;
        }
    }

    private void GenerateEditDetailGrid(int count)
    {
        DefineDetailDataTable();

        for (int i = 0; i < count; i++)
        {
            DataRow dtr = dtCalendarDetail.NewRow();
           
            dtr["ReadTypeID"] = "0";
            dtr["StartDate"] = "";
            dtr["StartTime"] = "";
            dtr["ReadPercentage"] = "";
            // Add the row to the DataTable.
            dtCalendarDetail.Rows.Add(dtr);
        }

        this.grdViewEditDetail.DataSource = dtCalendarDetail;
        this.grdViewEditDetail.DataBind();
    }          
   
}