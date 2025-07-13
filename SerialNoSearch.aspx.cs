using System;
using System.Linq;
using System.Data;

using Instadose;
using Instadose.Data;

public partial class TechOps_SerialNoSearch : System.Web.UI.Page
{
    // Create a DataTable for the Devices.
    DataTable dtDeviceDataTable = null;

    // Create the Database Reference.
    InsDataContext idc = new InsDataContext();

    // String to hold the current Username.
    string userName = "Unknown";

    // Default RID to 0 to pass to BadgeReview.aspx (QueryString value).
    int rID = 0;

    protected void Page_Load(object sender, EventArgs e)
    {
        // Try to set the Username.
        try { this.userName = User.Identity.Name.Split('\\')[1]; }
        catch { this.userName = "Unknown"; }

        if (!Page.IsPostBack)
        {
            InvisibleErrors();
        }

        txtSerialNumber.Focus();
    }

    private bool SerialNumberExists(string serialnumber)
    {
        int recordCount = 0;

        recordCount = (from udr in idc.UserDeviceReads
                       join di in idc.DeviceInventories on udr.DeviceID equals di.DeviceID
                       where di.SerialNo == serialnumber
                       select udr).Count();

        if (recordCount > 0) return true;
        else return false;
    }

    protected void btnGo_Click(object sender, EventArgs e)
    {
        InvisibleErrors();

        string serialNumber = txtSerialNumber.Text.Trim();
        
        // IF SerialNo NOT NULL;
        //  1.) Check to see if Serial Number is valid.
        //  2.) IF valid, set as QueryString value.
        //  3.) IF Invalid, display Error Message, clear control(s).
        if (txtSerialNumber.Text.Trim() != "")
        {
            if (SerialNumberExists(serialNumber) == true)
            {
                Response.Redirect("BadgeReview.aspx?SerialNo=" + serialNumber + "&RID=" + rID);
            }
            else 
            {
                string errorMsg = "Serial #" + serialNumber + " does not have any records.";
                VisibleErrors(errorMsg);
                txtSerialNumber.Text = String.Empty;
                SetFocus(txtSerialNumber);
            }
        }
        else 
        {
            string errorMsg = "Please enter a Serial #.";
            VisibleErrors(errorMsg);
            txtSerialNumber.Text = String.Empty;
            SetFocus(txtSerialNumber);
        }
    }

    // Reset Error Message.
    private void InvisibleErrors()
    {
        this.spnErrorMessage.InnerText = "";
        this.divErrorMessage.Visible = false;
    }

    // Set Error Message.
    private void VisibleErrors(string error)
    {
        this.spnErrorMessage.InnerText = error;
        this.divErrorMessage.Visible = true;
    }
}