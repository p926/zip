using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Instadose;
using Instadose.Data;

/// <summary>
/// Add new Device to existing Return Request
/// Require ReturnID which is being passed in the querystring "ID"
/// </summary>
/// 
public partial class InformationFinder_Compose_ReturnAddNewDevice : System.Web.UI.Page
{
  
    int ReturnID;
    int AccountID;

    // Create a the linq app database context.
    AppDataContext adc = new AppDataContext();
    InsDataContext idc = new InsDataContext();
    
      // String to hold the current username
    string UserName = "Unknown";

    protected void Page_Load(object sender, EventArgs e)
    {
        ResetValidateNote();

        // Try to set the username
        try
        {
            this.UserName = User.Identity.Name.Split('\\')[1];
        }
        catch { this.UserName = "Unknown"; }

        // is ReturnID?
        if (Request.QueryString["ID"] == null)
            return;

        // Grab the ReturnID if it was passed it the query string
        if (Request.QueryString["ID"] != null)
            int.TryParse(Request.QueryString["ID"], out ReturnID);
        
        try
        {
            rma_Return ret = (from r in adc.rma_Returns
                              where r.ReturnID == ReturnID
                              select r).First();

            ReturnID = ret.ReturnID;
            AccountID = ret.AccountID;
            this.lblAccountNo.Text = ret.AccountID.ToString();
            this.lblReturnID.Text = ReturnID.ToString();
        }
        catch
        {
            Response.Write ("The page could not be loaded because the RMA number is invalid.");
        }

        //// Check if page is PostBack
        if (IsPostBack) 
        { 
        //    // populate Serial Number list with checked or non-checked
        //    //this.ckblSerialNumber.Items.Clear();
        //    //GenerateSerialNoCheckBoxList();
        } 
        else
          this.FillExistingPage();
    }

    private void ResetValidateNote()
    {
        this.lblNoteValidate.Visible = false;
        this.lblSerialNoValidate.Visible = false;

        InvisibleErrors();
    }

    private void FillExistingPage()
    {
        try
        {
            var rmaReturn = (from rma in adc.rma_Returns
                             join reason in adc.rma_ref_ReturnReasons 
                             on rma.Return_ReasonID equals reason.ReasonID 
                             join RtTypes  in adc.rma_ref_ReturnTypes 
                             on rma.ReturnTypeID equals RtTypes.ReturnTypeID 
                             where rma.ReturnID == ReturnID
                             select new {rma, reason, RtTypes}).First();

            AccountID = rmaReturn.rma.AccountID;

            this.lblAccountNo.Text = AccountID.ToString();
            this.lblAccountName.Text = GetAccountName(AccountID);
            this.lblCreatedBy.Text = rmaReturn.rma.CreatedBy;
            this.lblReturnID.Text = ReturnID.ToString();
            this.lblReturnType.Text = rmaReturn.RtTypes.Type.ToString();
            this.lblReturnReason.Text = rmaReturn.reason.Description.ToString();
            this.txtNotes.Text = rmaReturn.rma.Notes;
            this.btnContinue.Text = "Update";

            if (!rmaReturn.rma.Active)
            {
                //this.lblError.Text = "This return has been deleted and is in read-only mode.";
                this.VisibleErrors("This return has been deleted and is in read-only mode.");
                this.btnContinue.Enabled = false;
                //this.btnDelete.Text = "Reactivate";
            }
            GenerateSerialNoCheckBoxList();
        }
        catch
        {
            //this.lblError.Text = "The RMA number is invalid. No return was found.";
            this.VisibleErrors("The RMA number is invalid. No return was found.");
            this.btnContinue.Enabled = false;
        }
    }
    
    // -------------------------------------------------------
    protected void btnCancel_Click(object sender, EventArgs e)
    {
        Page.Response.Redirect("../Details/Return.aspx?ID=" + this.lblReturnID.Text);
    }

    protected void btnContinue_Click(object sender, EventArgs e)
    {
        string errorString;
        // Clear the error.
        // this.lblError.Text = string.Empty;
        //this.InvisibleErrors();

        //// Ensure the reason field is not blank.
        // Ensure at least 1 enabled serial# being selected
        int selectedSerialNo = 0;
        for (int i = 0; i < this.ckblSerialNumber.Items.Count; i++)
        {
            if (this.ckblSerialNumber.Items[i].Selected == true && this.ckblSerialNumber.Items[i].Enabled == true)
            {
                selectedSerialNo += 1;
            }
        }
        if (selectedSerialNo == 0)
        {
            //this.lblError.Text = "Must select at least ONE serial number before continuing.";
            //this.VisibleErrors("Must select at least ONE serial number before continuing.");
            //return;

            errorString = "Must select at least ONE serial number.";
            this.lblSerialNoValidate.Visible = true;
            this.lblSerialNoValidate.InnerText = errorString;
            this.VisibleErrors(errorString);
            return;

        }

        // Ensure the notes fields is not blank
        if (this.txtNotes.Text == string.Empty)
        {
            //this.lblError.Text = "The notes must be filled in before continuing.";
            //this.VisibleErrors("The notes must be filled in before continuing.");
            //return;

            errorString = "Note is required.";
            this.lblNoteValidate.Visible = true;
            this.lblNoteValidate.InnerText = errorString;
            this.VisibleErrors(errorString);
            return;
        }

        // Update return's note
        SaveReturn();
        
        // Submit All selected Serial No with ReturnID
        this.SaveSelectedSerialNo(ReturnID );

        // update Return Status
        this.UpdateReturnStatus(ReturnID);

        // Redirect back to Return Detail page
        Response.Redirect("../Details/Return.aspx?ID=" + this.ReturnID .ToString());
   

    }


    private void SaveReturn()
    {
        // No meaning by adding rma_RMAHeader
        //rma_RMAHeader rmaHead = null; 
        //rmaHead = new rma_RMAHeader()
        //{
            
        //    ReturnID = ReturnID ,
        //    Active = true,
        //    RMANumber = "0",
        //    Reason = this.lblReturnReason.Text,
        //    CreatedBy = this.lblCreatedBy.Text,
        //    CreatedDate = DateTime.Now
        //};
        //adc.rma_RMAHeaders.InsertOnSubmit(rmaHead);

        var rma = (from r in adc.rma_Returns
                   where r.ReturnID == this.ReturnID
                   select r).Single();

        rma.Notes = this.txtNotes.Text;

        adc.SubmitChanges();

    }

    private void SaveSelectedSerialNo(int myReturnID)
    {
        AppDataContext adc = new AppDataContext();

        rma_ReturnDevice retrunDevices = null;

        // insert returnDevices request to rma_retrunDevices
        for (int i = 0; i < this.ckblSerialNumber.Items.Count; i++)
        {
            if (this.ckblSerialNumber.Items[i].Selected == true && this.ckblSerialNumber.Items[i].Enabled == true)
            {
                retrunDevices = new rma_ReturnDevice();

                retrunDevices.ReturnID = myReturnID;
                retrunDevices.SerialNo = ckblSerialNumber.Items[i].Text.ToString();
                retrunDevices.MasterDeviceID = int.Parse(ckblSerialNumber.Items[i].Value.ToString());
                retrunDevices.Notes = this.txtNotes.Text;
                retrunDevices.DepartmentID = 2;
                retrunDevices.Status = 1;
                retrunDevices.Active = true;

                // Grab device UserID 
                var MyUserID = (from a in idc.UserDevices
                                where a.DeviceID == int.Parse(ckblSerialNumber.Items[i].Value.ToString())
                                && a.Active == true
                                select a.UserID).FirstOrDefault();
                if (MyUserID != null)
                    retrunDevices.UserID = MyUserID;

                adc.rma_ReturnDevices.InsertOnSubmit(retrunDevices);
                adc.SubmitChanges();

                // add transLog with new RetrunDevicesID
                var writeTransLogReturn = adc.sp_rma_process(myReturnID, retrunDevices.ReturnDevicesID, 0, this.txtNotes.Text,
                   this.UserName , DateTime.Now, "ADD DEVICE", "New Device ID: " + retrunDevices.ReturnDevicesID.ToString(), 2);
            }
        }
    }

    /// <summary>
    /// update Return Status if new devices being added to existing Request 
    /// </summary>
    /// <param name="returnId"> ReturnID </param>
    protected void UpdateReturnStatus(int returnId)
    {
        // check # remaining device in request. 
        try // update request status if necessary.
        {
            // Requery the return and check to make sure the status is currently set to "Awaiting devices.." (1)
            var returnRequest = (from r in adc.rma_Returns
                                 where r.ReturnID == returnId && r.Active == true
                                 select r).First();

            // Get the Return Request devices count...
            int returnRequestNewCount = (from r in adc.rma_Returns
                                         join d in adc.rma_ReturnDevices on r.ReturnID equals d.ReturnID
                                         where r.ReturnID == returnId && r.Active == true
                                         && d.Active == true
                                         select r).Count();


            // Get a count of the inventory items for the return, based on the return ID
            int inventoryCount = (from ri in adc.rma_ReturnInventories
                                  where ri.Active == true &&
                                  ri.ReturnID == returnId
                                  select ri).Count();

            // Do they have the same number of items?
            if (returnRequestNewCount != inventoryCount && inventoryCount != 0)
            {
                if (returnRequest.Status == 6) { returnRequest.Status = 5; }
                if (returnRequest.Status == 8) { returnRequest.Status = 7; }

                // Save changes.
                adc.SubmitChanges();

                // insert TransLog, update Retrun Status
                var writeTransLogUPD = adc.sp_rma_process(ReturnID, 0, 0,
                              " ", this.UserName, DateTime.Now, "UPDATE RETURN",
                              "Update Request status to 6-->5 or 8-->7 afater delete.", 2);
            }
        } // end try -- update request status if necessary.
        catch { }
    }           


    /// <summary>
    /// Generate CheckBoxList with checked or non-check 
    /// </summary>
    private void GenerateSerialNoCheckBoxList()
    {
        // Get the latest Devices List of the AccountID. 
        // Notice: a device can be assigned back to an account many times sometimes.
        var varAllidcData = (from AcctDevices in idc.AccountDevices
                             join DevicesInv in idc.DeviceInventories
                             on AcctDevices.DeviceID equals DevicesInv.DeviceID
                             where AcctDevices.AccountID == AccountID && AcctDevices.CurrentDeviceAssign == true
                             orderby DevicesInv.SerialNo
                             select new { AcctDevices, DevicesInv }).ToList();  

        // Get all returning devices for an account which were not received by manufacturing or tech ops. 
        var varAllAdcData = (from Rtn in adc.rma_Returns
                             join RtnDevices in adc.rma_ReturnDevices on Rtn.ReturnID equals RtnDevices.ReturnID
                             where Rtn.AccountID == AccountID
                             && RtnDevices.Active == true 
                             && RtnDevices.Received == false
                             orderby RtnDevices.SerialNo
                             select new { RtnDevices }).ToList();
        
        // account-devices left join all returning devices 
        var JoinAllData = (from Allidc in varAllidcData
                           join AllADC in varAllAdcData on Allidc.DevicesInv.SerialNo equals AllADC.RtnDevices.SerialNo into join2
                           from AllIdcAdc in join2.DefaultIfEmpty()
                           select new { Allidc, AllIdcAdc }).ToList();

        // Serial# not in any return
        var AllUncheck = (from unCheck in JoinAllData
                          where unCheck.AllIdcAdc == null
                          select unCheck).ToList();
        
        // Serial# already in some return
        var AllChecked = (from Checked in JoinAllData
                          where Checked.AllIdcAdc != null
                          select Checked).ToList();
        
        // list all non-requested serial no
        foreach (var vA in AllUncheck)
        {
            ListItem listDevices = new ListItem();
            listDevices.Value = vA.Allidc.AcctDevices.DeviceID.ToString();
            listDevices.Text = vA.Allidc.DevicesInv.SerialNo.ToString();
            listDevices.Selected = false;
            this.ckblSerialNumber.Items.Add(listDevices);
        }

        // list all requested serial no
        foreach (var vU in AllChecked)
        {
            ListItem listDevices = new ListItem();
            listDevices.Value = vU.Allidc.AcctDevices.DeviceID.ToString();
            if (vU.AllIdcAdc.RtnDevices.ReturnID == ReturnID)
            {
                listDevices.Text = vU.Allidc.DevicesInv.SerialNo.ToString() + "**";
            }
            else
                listDevices.Text = vU.Allidc.DevicesInv.SerialNo.ToString();
            
            listDevices.Selected = true;
            listDevices.Enabled = false;

            listDevices.Attributes.Add("title", "This device is already initially returned in the Return #" + vU.AllIdcAdc.RtnDevices.ReturnID.ToString());
            
            this.ckblSerialNumber.Items.Add(listDevices);
        }
    }

    private string GetAccountName(int AccountID)
    {
        string AccountName = "Unknown";
        try
        {
             AccountName = (from a in idc.Accounts
                           where a.AccountID == AccountID
                           select a.AccountName).First();
        }
        catch (Exception ex)
        {
            //this.lblError.Text = "The account was not found.";
            this.VisibleErrors("The account was not found.");
            this.btnContinue.Enabled = false;
        }

        return AccountName;
    }

    private void InvisibleErrors()
    {
        // Reset submission form error message      
        this.errorMsg.InnerText = "";
        this.errors.Visible = false;
    }

    private void VisibleErrors(string error)
    {
        this.errorMsg.InnerText = error;
        this.errors.Visible = true;
    }

}

