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

public partial class Shipping_SpecialInstruction : System.Web.UI.Page
{
    InsDataContext idc = new InsDataContext();
    AppDataContext adc = new AppDataContext();
    string UserName = "Unknown";
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            this.UserName = User.Identity.Name.Split('\\')[1];

            if (!this.IsPostBack)
            {
                SetFocus(this.txtSerialNo);
                SetErrorMessage("");
                SetSuccessMessage("");
            }

        }
        catch
        {
            SetFocus(this.txtSerialNo);
            this.UserName = "Unknown";
        }
    }

    private void resetControls()
    {
        SetFocus(this.txtSerialNo);
        SetErrorMessage("");
        SetSuccessMessage("");

        this.txtSerialNo.Text = "";
        this.txtSpecialInstruction.Text = "";
    }

    protected void btnReset_Click(object sender, EventArgs e)
    {
        resetControls();
    }
    protected void btnSubmit_Click(object sender, EventArgs e)
    {
        try
        {
            SetErrorMessage("");
            SetSuccessMessage("");

            if (passInputsValidation())
            {                
                DeviceSpecialInstruction rec = new DeviceSpecialInstruction
                {
                    SerialNo = this.txtSerialNo.Text.Trim(),
                    SpecialInstruction = this.txtSpecialInstruction.Text.Trim(),
                    CreatedDate = System.DateTime.Now,
                    CreatedBy = this.UserName,
                    Active = true
                };

                adc.DeviceSpecialInstructions.InsertOnSubmit(rec);
                adc.SubmitChanges();

                string successMessage = "";
                successMessage += "Commit successfully.</br>";
                successMessage += "SN: " + this.txtSerialNo.Text.Trim() + "</br>";
                successMessage += "Special instruction: " + this.txtSpecialInstruction.Text.Trim();

                SetSuccessMessage(successMessage);

                SetFocus(this.txtSerialNo);
                this.txtSerialNo.Text = "";
                this.txtSpecialInstruction.Text = "";
            }            
        }
        catch(Exception ex)
        {
            SetErrorMessage(ex.Message);
        }        
    }

    private DeviceSpecialInstruction GetDeviceInstruction(string serialno, string spInstuction)
    {
        DeviceSpecialInstruction siDevice = (from ad in adc.DeviceSpecialInstructions
                       where ad.SerialNo == serialno
                       && ad.SpecialInstruction == spInstuction
                       select ad).FirstOrDefault();

        return siDevice;
    }

    private bool passInputsValidation()
    {        
        if (this.txtSerialNo.Text.Trim().Length == 0)
        {            
            this.SetErrorMessage("SerialNo is required.");
            SetFocus(this.txtSerialNo);
            return false;
        }
        else
        {
            DeviceInventory deviceInventory = (from di in idc.DeviceInventories where di.SerialNo == this.txtSerialNo.Text.Trim() select di).FirstOrDefault();
            if (deviceInventory == null)
            {
                this.SetErrorMessage("The SerialNo does not exist in the system.");
                SetFocus(this.txtSerialNo);
                return false;
            }
        }

        if (this.txtSpecialInstruction.Text.Trim().Length == 0)
        {
            this.SetErrorMessage("Special instruction is required.");
            SetFocus(this.txtSpecialInstruction);
            return false;
        }

        DeviceSpecialInstruction siDevice = GetDeviceInstruction(this.txtSerialNo.Text.Trim(), this.txtSpecialInstruction.Text.Trim());
        if (siDevice != null)
        {
            SetErrorMessage("Special instruction [" + this.txtSpecialInstruction.Text.Trim() + "] for Serial# [" + this.txtSerialNo.Text.Trim() + "] that was created [" + siDevice.CreatedDate + "] already exist.");
            return false;
        }

        return true;
    }

    private void SetErrorMessage(string error)
    {            
        this.errorMsg.InnerHtml = error;
        this.error.Visible = !String.IsNullOrEmpty(error);
    }   
    private void SetSuccessMessage(string message)
    {
        this.successMsg.InnerHtml = message;
        this.success.Visible = !String.IsNullOrEmpty(message);
    }
}