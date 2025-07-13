using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;

using Instadose;
using Instadose.Data;

public partial class TechOps_AssignGroupColor : System.Web.UI.Page
{
    // Create the database reference
    InsDataContext idc = new InsDataContext();
    AppDataContext adc = new AppDataContext();  

    // String to hold the current username
    string UserName = "Unknown";

    protected void Page_Load(object sender, EventArgs e)
    {       
        // Try to set the username
        try
        {
            this.UserName = User.Identity.Name.Split('\\')[1];
           
            InvisibleErrors();
            InvisibleMessages();
            if (!this.IsPostBack )
                LoadControls();
        }
        catch 
        { 
            this.UserName = "Unknown";           
        }
    }

    private void LoadControls()
    {
        var groups = from r in idc.DeviceGroups                     
                     orderby r.DeviceGroupID descending 
                     select new
                     {
                         r.DeviceGroupID ,
                         r.DeviceGroupName 
                     };

        this.ddlGroup.DataSource = groups;
        this.ddlGroup.DataTextField = "DeviceGroupName";
        this.ddlGroup.DataValueField = "DeviceGroupID";
        this.ddlGroup.DataBind();

        ListItem item0 = new ListItem("", "0");
        this.ddlGroup.Items.Insert(0, item0);
        // ------------------------------------//

        var profiles = from r in idc.DeviceProfiles 
                     orderby r.DeviceProfileName ascending
                     select new
                     {
                         r.DeviceProfileID ,
                         r.DeviceProfileName 
                     };

        this.ddlDeviceProfile.DataSource = profiles;
        this.ddlDeviceProfile.DataTextField = "DeviceProfileName";
        this.ddlDeviceProfile.DataValueField = "DeviceProfileID";
        this.ddlDeviceProfile.DataBind();

        ListItem item1 = new ListItem("", "0");
        this.ddlDeviceProfile.Items.Insert(0, item1);        
    }
      
    //Commit Edit action, save update Comment 
    private void Commit()
    {
        int myDeviceGroupID;
        string myGroupName = "";
        List<string> serialNumberList = new List<string>();
        serialNumberList.AddRange(this.txtSerialNos.Text.Split('\n'));

        if (serialNumberList.Count > 0)
        {
            myGroupName = (this.txtGroup.Text.Trim().Length > 0 ? this.txtGroup.Text.Trim() : this.ddlGroup.SelectedItem.Text);

            DeviceGroup group = (from g in idc.DeviceGroups where g.DeviceGroupName == myGroupName select g).FirstOrDefault();

            if (group == null)
            {
                group = new DeviceGroup()
                {
                    DeviceGroupName = myGroupName
                };
                idc.DeviceGroups.InsertOnSubmit(group);
                idc.SubmitChanges();

                myDeviceGroupID = group.DeviceGroupID;
            }
            else
            {
                myDeviceGroupID = group.DeviceGroupID;
            }

            // Loop through each serial number.
            foreach (string serialNumber in serialNumberList)
            {
                if (serialNumber.Trim().Length > 0)
                {
                    // Grab the record from SQL
                    DeviceInventory device = (from di in idc.DeviceInventories where di.SerialNo == serialNumber.Trim() select di).FirstOrDefault();

                    if (device != null)
                    {
                        device.DeviceAnalysisStatus = (from ds in idc.DeviceAnalysisStatus where ds.DeviceAnalysisName == "Assigned" select ds).FirstOrDefault();
                        device.CurrentGroupID = myDeviceGroupID;
                        device.InitialGroupID = myDeviceGroupID;
                        device.PendingDeviceProfileID = int.Parse(this.ddlDeviceProfile.SelectedItem.Value);
                        device.ScheduleID = (int?)null;
                        device.ScheduleSyncDate = (DateTime?)null;
                        device.ScheduleSyncStatus = null;
                        device.ScheduleTimeOffset = (int?)null;

                        idc.SubmitChanges();

                        if (this.chkBoxDeleteReads.Checked)
                        {
                            CleanAllReadRecords(device.DeviceID);
                        }                        
                    }                    
                }
            }            
        }
    }

    private void CleanAllReadRecords(int pDeviceID)
    {
        var removeList = (from udr in idc.UserDeviceReads
                      where udr.DeviceID == pDeviceID
                      select udr).ToList();

        if (removeList != null)
        {
            idc.UserDeviceReads.DeleteAllOnSubmit(removeList.ToList());
            idc.SubmitChanges();
        }
    }

    protected void btnAssign_Click(object sender, EventArgs e)
    {
        if (PassValidation())
        {
            Commit();

            // Display Successful message and reset all controls
            this.VisibleMessages("Badges have been assigned.");
            this.txtSerialNos.Text = "";
            this.txtGroup.Text = "";
            this.ddlGroup.SelectedValue = "0";
            this.ddlDeviceProfile.SelectedValue = "0";
            this.chkBoxDeleteReads.Checked = false;
        }
    }

    private void InvisibleErrors()
    {
        // Reset submission form error message      
        this.errorMsg.InnerHtml = "";
        this.errors.Visible = false;
    }

    private void VisibleErrors(string error)
    {
        this.errorMsg.InnerHtml  = error;
        this.errors.Visible = true;
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

    private bool PassValidation()
    {       
        string errorMessage = "";

        if (this.txtGroup.Text.Trim().Length == 0 & this.ddlGroup.SelectedValue == "0" )
        {
            VisibleErrors("Group name is required.");
            return false;
        }

        if (this.txtGroup.Text.Trim().Length > 0 & this.ddlGroup.SelectedValue != "0")
        {
            VisibleErrors("Can not both enter and select a Group Name at the same time.");
            return false;
        }

        if (this.ddlDeviceProfile.SelectedItem.Text == "")
        {
            VisibleErrors("Please select a device profile for device.");
            return false;
        }

        if (! ValidBadges(ref errorMessage))
        {
            VisibleErrors(errorMessage);
            return false;
        }

        return true;
    }

    private bool ValidBadges(ref string pErrorMessage)
    {
        bool isValid = true;
        List<string> serialNumberList = new List<string>();
        serialNumberList.AddRange(this.txtSerialNos.Text.Split('\n'));

        pErrorMessage = "The following badges are not in the system: <ul type='circle'>";
           
        if (serialNumberList.Count > 0)
        {
            // Loop through each serial number.
            foreach (string serialNumber in serialNumberList)
            {
                if (serialNumber.Trim().Length > 0)
                {
                    // Grab the record from SQL
                    DeviceInventory device = (from di in idc.DeviceInventories where di.SerialNo == serialNumber.Trim() select di).FirstOrDefault();

                    if (device == null)
                    {
                        pErrorMessage += "<li>" + serialNumber.Trim() + "</li>";
                        isValid = false;
                    }
                }
            }
            pErrorMessage += "</ul>";
        }

        return isValid;
    }
   
   
}
