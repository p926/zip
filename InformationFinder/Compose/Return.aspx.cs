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
/// Initiate New Retrun Request (Customer Services Department)
/// Require AccountID which is being passed in the querystring "ID"
/// </summary>
/// 
public partial class InformationFinder_Compose_Return : System.Web.UI.Page
{
    int AccountID;
    int ReturnID;
    string UserName;
    
    // Create the database reference
    AppDataContext adc = new AppDataContext();
    InsDataContext idc = new InsDataContext();
   
    protected void Page_Load(object sender, EventArgs e)
    {
        Page.Title = "InstaDose Return/RMA - Customer Service Initiation";
        // Grab the AccountID if it was passed it the query string
        if (Request.QueryString["ID"] != null)
            int.TryParse(Request.QueryString["ID"], out AccountID);

        // Grab the ReturnID if it was passed it the query string
        if (Request.QueryString["ReturnID"] != null)
            int.TryParse(Request.QueryString["ReturnID"], out ReturnID);

        ResetValidateNote();

        try
        {
            this.UserName = User.Identity.Name.Split('\\')[1];
        }
        catch { this.UserName = "Unknown"; }


        // Check if page is PostBack
        if (!IsPostBack) 
        { 
            // Populate the menus with the data.
            this.FillMenus();
        } 

        // Determine if it's a new return or existing.
        if (isNewReturn())
        {
            // Fill the page with new
            this.FillNewPage();
        }
        else
        {
            this.FillExistingPage();
        }
    }

    private void ResetValidateNote()
    {
        this.lblNoteValidate.Visible = false;
        this.lblSerialNoValidate.Visible = false;
        
        InvisibleErrors();        
    }

    protected void btnCancel_Click(object sender, EventArgs e)
    {
        Page.Response.Redirect("../Details/Account.aspx?ID=" + this.lblAccountNo.Text + "#Return_tab");
    }

    protected void btnContinue_Click(object sender, EventArgs e)
    {
        string errorString;
        // Clear the error.
        //this.lblError.Text = string.Empty;
        //this.InvisibleErrors();

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

        // Submit the return and get the ID generated
        int NewReturnID   = SaveReturn();

        // Submit Selected Serial Nos with ReturnID
        this.SaveSelectedSerialNo(NewReturnID);

        // Redirect back to Account Detail page
        Response.Redirect("../Details/Account.aspx?ID=" + this.lblAccountNo.Text + "#Return_tab");
        //Response.Redirect("ReturnDetails.aspx?ID=" + this.ID.ToString());


    }

    protected void btnDelete_Click(object sender, EventArgs e)
    {  
        try
        {
            // Create the record from SQL.
            rma_Return ret = (from r in adc.rma_Returns
                              where r.ReturnID == this.ReturnID
                              select r).First();

            // If the button is used to delete...
            if (btnDelete.Text == "Delete")
            {
                // Deactivate the record so it can be reactivated later.
                ret.Active = false;
                adc.SubmitChanges();

                // Return to the overview page.
                Response.Redirect("../Details/Account.aspx?ID=" + this.lblAccountNo.Text + "#Return_tab");
            }
            else
            {
                // Re-activate the record if it was deleted by accident.
                ret.Active = true;
                adc.SubmitChanges();

                // Reload the page using a redirect function.
                Response.Redirect("Return.aspx?ID=" + AccountID );
            }

        }
        catch(Exception ex)
        {
            // Display the error message.
            //this.lblError.Text = "Could not get return information. Reason: " + ex.Message;
            this.VisibleErrors("Could not get return information. Reason: " + ex.Message);
        }
    }

    private int SaveReturn()
    {
        rma_Return rma = null;
        //rma_ReturnDevice retrunDevices = null;

        if (isNewReturn())
        {
            rma = new rma_Return()
            {
                AccountID = AccountID,
                Active = true,
                Notes = this.txtNotes.Text,
                Reason = this.ddlReturnReason.SelectedItem.Text,
                Return_ReasonID= int.Parse(this.ddlReturnReason.SelectedValue),
                ReturnTypeID = int.Parse(this.ddlReturnType.SelectedValue),
                Status = 1,
                CreatedBy = this.UserName,
                CreatedDate = DateTime.Now

            };
            adc.rma_Returns.InsertOnSubmit(rma);
        }
        else
        {
            rma = (from r in adc.rma_Returns
                   where r.ReturnID == this.ReturnID
                   select r).First();

            rma.Notes = this.txtNotes.Text;
            rma.ReturnTypeID = int.Parse(this.ddlReturnType.SelectedValue);
        }
        adc.SubmitChanges();
        
        // Insert data to Transaction Log with new ReturnID
        var writeTransLogReturn = adc.sp_rma_process(rma.ReturnID , 0, 0, this.txtNotes.Text,
                this.UserName, DateTime.Now, "ADD RETURN", "New return ID: "
                + rma.ReturnID.ToString(), 2);
        
        // Return the ID that was generated.
        return rma.ReturnID ;
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
                // Tdo, 1/31/2014
                // update previous Customer Request to inactive
                // select all previous requests where serialno = the inserted serialno.
                List<rma_ReturnDevice> returnDevice = (from rd in adc.rma_ReturnDevices
                                                       where rd.SerialNo == ckblSerialNumber.Items[i].Text.ToString() && rd.Active == true
                                                       select rd).ToList();

                // Deactivate all previous returndevice of serialno.
                foreach (rma_ReturnDevice dbActive in returnDevice)
                {
                    dbActive.Active = false;
                }

                // Insert new returndevice
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
                if (MyUserID != null && MyUserID > 0)
                    retrunDevices.UserID = MyUserID;


                adc.rma_ReturnDevices.InsertOnSubmit(retrunDevices);
                adc.SubmitChanges();

                // add transLog with new RetrunDevicesID
                var writeTransLogReturn = adc.sp_rma_process(myReturnID, retrunDevices.ReturnDevicesID, 0, this.txtNotes.Text,
                   this.UserName, DateTime.Now, "ADD DEVICE", "New Device ID: " + retrunDevices.ReturnDevicesID.ToString(), 2);
            }   // close if check
           
        } // close For loop
       
    }



    private void FillNewPage()
    {
        
        this.lblAccountNo.Text = AccountID.ToString();
        this.lblAccountName.Text = GetAccountName(AccountID);
        this.lblCreatedBy.Text = this.UserName; //"ddelapp"; // Page.User.Identity.Name.Split('\\')[1];
        this.btnDelete.Visible = false;
    }

    private void FillExistingPage()
    {
        try
        {
            rma_Return rmaReturn = (from rma in adc.rma_Returns
                                    where rma.ReturnID == ReturnID
                                    select rma).First();

            AccountID = rmaReturn.AccountID;

            this.lblAccountNo.Text = AccountID.ToString();
            this.lblAccountName.Text = GetAccountName(AccountID);
            this.lblCreatedBy.Text = rmaReturn.CreatedBy;
            //this.txtReason.Text = rmaReturn.Reason;
            this.txtNotes.Text = rmaReturn.Notes;
            this.btnContinue.Text = "Update";

            if (!rmaReturn.Active)
            {
                //this.lblError.Text = "This return has been deleted and is in read-only mode.";
                this.VisibleErrors("This return has been deleted and is in read-only mode.");
                this.btnContinue.Enabled = false;
                this.btnDelete.Text = "Reactivate";
            }
            
            // Update the status items
            foreach(ListItem listItem in this.ddlReturnType.Items)
            {
                if (listItem.Value == rmaReturn.ReturnTypeID.ToString())
                {
                    listItem.Selected = true;
                }
                else
                {
                    listItem.Selected = false;
                }
            }
        }
        catch
        {
            //this.lblError.Text = "The RMA number is invalid. No return was found.";
            this.VisibleErrors("The RMA number is invalid. No return was found.");
            this.btnContinue.Enabled = false;
        }
    }

    private bool isNewReturn()
    {

        // Get account id
        if (Page.Request.QueryString["ID"] != null)
        {
            // An account ID was found therefore there must be new request
            int.TryParse(Page.Request.QueryString["ID"].ToString(), out AccountID);
            return true;
        }
        else
        {
            if (Page.Request.QueryString["ReturnID"] != null)
                int.TryParse(Request.QueryString["ReturnID"], out ReturnID);

            return false;
        }
    }

    private void FillMenus()
    {
        //clear all selection
        //this.ddlReturnReason.ClearSelection();
        //this.ddlReturnType.ClearSelection();

        
        // Create the data context for linq queries.
        AppDataContext adc = new AppDataContext();

        // Filter the types to only show customer service (2)
        int DepartmentID = 2;

        // Display DDL return Type. Do not allow RMA for Lost here. It is generated automatically at Lost Replacement/Lost  order
        List<rma_ref_ReturnType> returnTypes = (from rs in adc.rma_ref_ReturnTypes
                                                where rs.Active == true && rs.DepartmentID == DepartmentID
                                                    && rs.Type != "Lost (No Return)"
                                                select rs).ToList();
        foreach (rma_ref_ReturnType returnType in returnTypes)
        {
            ListItem listItem = new ListItem(returnType.Type, returnType.ReturnTypeID.ToString());

            this.ddlReturnType.Items.Add(listItem);
        }
        
        // Display DDL return reason
        List<rma_ref_ReturnReason> returnReason = (from rs in adc.rma_ref_ReturnReasons
                                                   where rs.Active == true && rs.DepartmentID == DepartmentID
                                                        && rs.Description != "Lost (No Return)"
                                                   orderby rs.Description 
                                                   select rs ).ToList();
        foreach (rma_ref_ReturnReason  Reasons in returnReason)
        {
            ListItem listItemReason = new ListItem(Reasons.Description , Reasons.ReasonID.ToString());
            this.ddlReturnReason.Items.Add(listItemReason);
        }

        // Display Serial Number CheckBoxList
        GenerateSerialNoCheckBoxList();

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
                             select new {AcctDevices, DevicesInv}).ToList();    
        
        // Get all returning devices for an account which were not received by receiving/shipping. 
        // if the device was received by receiving/shipping, then ReturnDevices.Received flag = true and 
        // UserDevice and AccountDevice for this device will be inactive and CurrentDeviceAssign = false
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


        // Get Return Type DDL selected value (DDL autopastback is on )
        int mySelectRetrunTypeValue = int.Parse(this.ddlReturnType.SelectedValue);
        bool MyCblCheck = false;
        if (mySelectRetrunTypeValue == 1)  // type=1 : Cancel Account
        {
            MyCblCheck = true;
        }

      
        // CheckBoxList -- get all devices belong to accountID
        foreach (var v in JoinAllData)
        {
            ListItem listDevices = new ListItem();
            listDevices.Value = v.Allidc.AcctDevices.DeviceID.ToString();
            listDevices.Text = v.Allidc.DevicesInv.SerialNo.ToString();
            listDevices.Selected = MyCblCheck;

            if (v.AllIdcAdc != null)
            {
                listDevices.Enabled = false;
                listDevices.Selected = true;
                listDevices.Attributes.Add("title", "This device is already initially returned in the Return #" + v.AllIdcAdc.RtnDevices.ReturnID.ToString());
            }
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

    protected void ddlReturnType_SelectedIndexChanged(object sender, EventArgs e)
    {
        this.ckblSerialNumber.Items.Clear();
        GenerateSerialNoCheckBoxList();
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

