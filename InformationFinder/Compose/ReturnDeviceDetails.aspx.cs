using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Instadose;
using Instadose.Data;

public partial class InformationFinder_Compose_ReturnDeviceDetails : System.Web.UI.Page
{
    // Store the primary ID from the QueryString
    int ReturnDeviceDetailID;

    // Create a the linq app database context.
    AppDataContext adc = new AppDataContext();

    // String to hold the current username
    string UserName = "Unknown";


    protected void Page_Load(object sender, EventArgs e)
    {
        // Try to set the username
        try
        {
            this.UserName = User.Identity.Name.Split('\\')[1];
        }
        catch { this.UserName = "Unknown"; }

  
        // Check to see if the ID exists and if it is a valid integer.
        if (Request.QueryString["ID"] != null && int.TryParse(Request.QueryString["ID"], out ReturnDeviceDetailID))
        {
            // Skip everything else if the page is a post-back.
            if (IsPostBack) return;

            try
            {
                // Select the device record.
                rma_ReturnDevice returnDevice = (from rd in adc.rma_ReturnDevices
                                                 join rds in adc.rma_ref_ReturnDeviceStatus 
                                                 on rd.Status equals rds.ReturnDeviceStatusID 
                                                 where rd.ReturnDevicesID == ReturnDeviceDetailID
                                                 select rd).First();

                // Set the Account#
                this.lblAccountNo.Text = returnDevice.rma_Return.AccountID.ToString();

                // Set the RMA#
                this.lblReturnID.Text = returnDevice.rma_Return.ReturnID.ToString();

                // Set the Serial#
                this.lblSerialNo.Text = returnDevice.SerialNo;

                // Set the Status
                this.lblStatus.Text = returnDevice.rma_ref_ReturnDeviceStatus.Status.ToString();
              
                // Set the Notes
                this.txtDeviceNotes.Text = returnDevice.Notes;

                
                // If the record is inactive display the error message.
                if (!returnDevice.Active.Value)
                {
                    DisplayError("This device has been deleted from this return and is now read-only.",true);
                }
            }
            catch
            {
                // Display the error message if the detail can't load.
                DisplayError("Error: The return detail does not exist.", false);
            }

        }
        else
        {
            // Display the error message if the detail ID doesn't exist.
            DisplayError("Error finding the return detail ID.", false);
        }
    }

    /// <summary>
    /// Redirect the user back to the overview page.
    /// </summary>
    /// <param name="sender">Object sending the button click.</param>
    /// <param name="e">Event arguments from the object interaction.</param>
    protected void btnCancel_Click(object sender, EventArgs e)
    {
        if (this.lblReturnID.Text == "")
        {
            // If the RMA# couldn't be found redirect to the portal.
            Response.Redirect("../");
        }
        else
        {
            // If the RMA# was found, redirect to the overview.
            Response.Redirect("../Details/Return.aspx?ID=" + this.lblReturnID.Text);
        }
    }

    /// <summary>
    /// Save the changes to the device detail record.
    /// </summary>
    /// <param name="sender">Object sending the button click.</param>
    /// <param name="e">Event arguments from the object interaction.</param>
    protected void btnUpdate_Click(object sender, EventArgs e)
    {        
        try
        {
            // Select the return device record.
            rma_ReturnDevice rd = (from d in adc.rma_ReturnDevices
                                   where d.ReturnDevicesID == ReturnDeviceDetailID
                                   select d).First();
        
            // Set the notes field.
            rd.Notes = this.txtDeviceNotes.Text;

            // Save the changes.
            adc.SubmitChanges();

            // Insert data to Transaction Log with new ReturnID
            var writeTransLogReturn = adc.sp_rma_process(rd.ReturnID, ReturnDeviceDetailID, 0, this.txtDeviceNotes.Text,
                    this.UserName , DateTime.Now, "EDIT DEVICE", "Edit device ID: "
                    + ReturnDeviceDetailID.ToString(), 2);

            // Redirect if successful to the overview page.
            Response.Redirect("../Details/Return.aspx?ID=" + this.lblReturnID.Text);
        }
        catch
        {
            // If there was an error saving, display the error message.
            DisplayError("Error: Could not save the device details.", false);
        }

    }

    /// <summary>
    /// Delete or reactivate a record.
    /// </summary>
    /// <param name="sender">Object sending the button click.</param>
    /// <param name="e">Event arguments from the object interaction.</param>
    protected void btnDelete_Click(object sender, EventArgs e)
    {
        try
        {
            // Create the record from SQL.
            rma_ReturnDevice returnDevice = (from rd in adc.rma_ReturnDevices
                                             where rd.ReturnDevicesID == ReturnDeviceDetailID
                                             select rd).First();

            // If the button is used to delete...
            if (btnDelete.Text == "Delete")
            {
                // Deactivate the record so it can be reactivated later.
                returnDevice.Active = false;
                adc.SubmitChanges();


                // Insert Transaction Log with ReturnDevicesID
                var writeTransLogDelete = adc.sp_rma_process(int.Parse(this.lblReturnID.Text) , ReturnDeviceDetailID, 0, " ",
                        this.UserName, DateTime.Now, "DELETE DEVICE", "Delete device ID: "
                        + ReturnDeviceDetailID.ToString(), 2);

                // Return to the overview page.
                Response.Redirect("../Details/Return.aspx?ID=" + this.lblReturnID.Text);
            }
            else
            {
                // Re-activate the record if it was deleted by accident.
                returnDevice.Active = true;
                adc.SubmitChanges();

                // Reload the page using a redirect function.
                Response.Redirect("ReturnDeviceDetails.aspx?ID=" + ReturnDeviceDetailID);
            }

        }
        catch
        {
            // Display the error message.
            DisplayError("Error: Could not delete the device from the return.", false);
        }
    }
    
    /// <summary>
    /// Disable controls and display error when a critical error occurs.
    /// </summary>
    /// <param name="Message">Error message to display.</param>
    private void DisplayError(string Message, bool Reactivate)
    {
        //this.lblError.Text = Message;
        //this.lblError.Visible = true;

        this.errorMsg.InnerText = Message;
        this.errors.Visible = true;
        this.btnUpdate.Enabled = false;

        if (Reactivate)
            this.btnDelete.Text = "Reactivate";
        else
            this.btnDelete.Enabled = false;
    }
}

