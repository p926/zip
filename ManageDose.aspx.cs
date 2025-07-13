using System;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Instadose.Data;
using Mirion.DSD.GDS.API.Contexts;


public partial class TechOps_ManageDose : System.Web.UI.Page
{
    // Create the database reference
    InsDataContext idc = new InsDataContext();
    UnixDataClassesDataContext udc = new UnixDataClassesDataContext();


    /// <summary>
    /// Limits access to the functionality based on groups.
    /// </summary>
    string[] activeDirecoryGroups = { "IRV-IT" };
    string UserName = "Unknown";
    string[] edeReadTypeList = { "User Read", "Estimate", "Adjustment", "Add Dose Record" };
    string[] edeProductList = { "Instadose 2", "Instadose Plus", "Instadose 2 New" };
    //int? userID;
    //bool UserHasEDE;

    public bool YesNo()
    {
        if (this.ddlReadFilter.SelectedItem.Value == "UserRead")
            return true;
        else
            return false;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        // Query active directory to see if the current user belongs to a group.
        //bool belongsToGroups = ActiveDirectoryQueries.UserExistsInAnyGroup(User.Identity.Name, activeDirecoryGroups);
        //this.UserName = User.Identity.Name.Split('\\')[1];

        // Take this security check off for now by Devin.

        //// If the user is not Daniela and does not exists in the required group, then do not show the page.
        //if (!belongsToGroups && this.UserName != "dgonzalez")
        //{
        //    Response.Write("Not Authorized to View this page!");
        //    Response.End();
        //}
        //else
        //{            
        //    if (!this.IsPostBack)
        //    {
        //        InitiateAllControls();                
        //    } 
        //}

        if (!this.IsPostBack)
        {            
            InitiateAllControls();
        } 

    }

    private void InitiateAllControls()
    {
        InitiateRegionControl();
    }

    private void InitiateRegionControl()
    {
        var regions = from r in idc.BodyRegions
                      orderby r.BodyRegionName
                      select new
                      {
                          r.BodyRegionID,
                          r.BodyRegionName
                      };

        this.ddlRegion.DataSource = regions;
        this.ddlRegion.DataTextField = "BodyRegionName";
        this.ddlRegion.DataValueField = "BodyRegionID";
        this.ddlRegion.DataBind();
    }

    protected void btnGo_Click(object sender, EventArgs e)
    {
        if (this.ddlDisplay.SelectedItem.Value == "Doses")
        {
            grdAuditReadingView.Visible = false;
            grdReadingView.Visible = true;

            grdReadingView.DataBind();
        }
        else
        {
            grdReadingView.Visible = false;
            grdAuditReadingView.Visible = true;
            
            grdAuditReadingView.DataBind();
        }
    }


    // ------------------------- Modal Dialog Section FUNCTIONS here ------------------------------------- //

    private void InitialSetEnableInputControls()
    {
        this.ddlReadType.Enabled = true;
        this.txtExposureDate.Enabled = true;
        this.txtExposureTime.Enabled = true;
        this.ddlRegion.Enabled = true;
    }

    private void InitialSetDisableInputControls()
    {
        //this.ddlReadType.Enabled = false;
    }
      
    private void InitiateReadTypeControl(string mode)
    {
        if (mode == "Edit")
        {
            var readTypes = from r in idc.ReadTypes
                            orderby r.ReadTypeName
                            select new
                            {
                                r.ReadTypeID,
                                r.ReadTypeName
                            };

            this.ddlReadType.DataSource = readTypes;
            this.ddlReadType.DataTextField = "ReadTypeName";
            this.ddlReadType.DataValueField = "ReadTypeID";
            this.ddlReadType.DataBind();
        }
        else
        {
            var reads = new string[] {"Lifetime","Year to Date","Estimate","ADD Dose Record"};

            var readTypes = from r in idc.ReadTypes
                            where reads.Contains(r.ReadTypeName)
                            select new
                            {
                                r.ReadTypeID,
                                r.ReadTypeName
                            };

            this.ddlReadType.DataSource = readTypes;
            this.ddlReadType.DataTextField = "ReadTypeName";
            this.ddlReadType.DataValueField = "ReadTypeID";
            this.ddlReadType.DataBind();
        }        
    }
                          
    private void InVisibleErrors_ReadDialog()
    {
        // Reset submission form error message      
        this.readDialogErrorMsg.InnerText = "";
        this.readDialogErrors.Visible = false;
    }

    private void VisibleErrors_ReadDialog(string error)
    {
        this.readDialogErrorMsg.InnerText = error;
        this.readDialogErrors.Visible = true;
    }

    private bool passInputsValidation_ReadDialog()
    {
        string errorString = "";        
        DateTime myDate;
        Double myDouble;

        if (this.ddlReadType.SelectedItem.Text.Length == 0)
        {           
            errorString = "Read Type is required.";
            this.VisibleErrors_ReadDialog(errorString);
            SetFocus(this.ddlReadType);
            return false;
        }

        if (this.txtExposureDate.Text.Trim().Length == 0 || DateTime.TryParse(this.txtExposureDate.Text.Trim(), out myDate) == false)
        {
            errorString = "Exposure Date is required and must be a date.";
            this.VisibleErrors_ReadDialog(errorString);
            SetFocus(this.txtExposureDate);
            return false;
        }

        if (this.txtDeep.Text.Trim().Length == 0 || double.TryParse(this.txtDeep.Text.Trim(), out myDouble) == false)
        {            
            errorString = "DeepLowDose is required and must be a number.";
            this.VisibleErrors_ReadDialog(errorString);
            SetFocus(this.txtDeep);
            return false;
        }
        else if (double.TryParse(this.txtDeep.Text.Trim(), out myDouble))
        {
            if (myDouble < 0)
            {                
                errorString = "DeepLowDose must be greater than 0.";
                this.VisibleErrors_ReadDialog(errorString);
                SetFocus(this.txtDeep);
                return false;
            }
        }

        if (this.txtShallow.Text.Trim().Length == 0 || double.TryParse(this.txtShallow.Text.Trim(), out myDouble) == false)
        {            
            errorString = "ShallowLowDose is required and must be a number.";
            this.VisibleErrors_ReadDialog(errorString);
            SetFocus(this.txtShallow);
            return false;
        }
        else if (double.TryParse(this.txtShallow.Text.Trim(), out myDouble))
        {
            if (myDouble < 0)
            {                
                errorString = "ShallowLowDose must be greater than 0.";
                this.VisibleErrors_ReadDialog(errorString);
                SetFocus(this.txtShallow);
                return false;
            }
        }

        if (this.txtEye.Text.Trim().Length > 0)
        {
            if (double.TryParse(this.txtEye.Text.Trim(), out myDouble) == false)
            {                
                errorString = "EyeDose must be a number.";
                this.VisibleErrors_ReadDialog(errorString);
                SetFocus(this.txtEye);
                return false;
            }
            else
            {
                if (myDouble < 0)
                {                    
                    errorString = "EyeDose must be greater than 0.";
                    this.VisibleErrors_ReadDialog(errorString);
                    SetFocus(this.txtEye);
                    return false;
                }
            }
        }

        string doseWeightCode = "";        
        if (IsEDEAllowed(ref doseWeightCode))
        {
            string mode = (Session["mode"] != null) ? Convert.ToString(Session["mode"]) : "";

            // For DOUBLE webster and in mode Add, EDEDose input is not required. Otherwise, EDEDose is required
            if (this.txtEDE.Enabled && doseWeightCode.Substring(0, 1) == "D" && mode == "Add") 
            {                
            } 
            else
            {
                if (string.IsNullOrEmpty(this.txtEDE.Text.Trim()))
                {
                    errorString = "EDEDose is required.";
                    this.VisibleErrors_ReadDialog(errorString);
                    SetFocus(this.txtEDE);
                    return false;
                }
            }
        }
       
        if (this.txtEDE.Text.Trim().Length > 0 && this.ddlRegion.SelectedItem.Text == "Collar")
        {
            if (double.TryParse(this.txtEDE.Text.Trim(), out myDouble) == false)
            {
                errorString = "EDEDose must be a number.";
                this.VisibleErrors_ReadDialog(errorString);
                SetFocus(this.txtEDE);
                return false;
            }
            else
            {
                if (myDouble < 0)
                {
                    errorString = "EDEDose must be equal or greater than 0.";
                    this.VisibleErrors_ReadDialog(errorString);
                    SetFocus(this.txtEye);
                    return false;
                }
                else // if having EDEDose >= 0  
                {
                    if (string.IsNullOrEmpty(this.txtEDEType.Text.Trim()))
                    {
                        errorString = "You must click Calculate EDE button to populate EDE Type.";
                        this.VisibleErrors_ReadDialog(errorString);
                        SetFocus(this.btnCalculateEDE);
                        return false;
                    }

                    if (string.IsNullOrEmpty(this.txtEDEStatus.Text.Trim()))
                    {
                        errorString = "You must click Calculate EDE button to populate EDE Status.";
                        this.VisibleErrors_ReadDialog(errorString);
                        SetFocus(this.btnCalculateEDE);
                        return false;
                    }
                }
            }
        }

        if (this.ddlRegion.SelectedItem.Text.Length == 0)
        {            
            errorString = "Region is required.";
            this.VisibleErrors_ReadDialog(errorString);
            SetFocus(this.ddlRegion);
            return false;
        }

        if (string.IsNullOrEmpty(txtSerialNo.Text) || string.IsNullOrEmpty(txtAccountID.Text) || string.IsNullOrEmpty(txtUserID.Text))
        {
            errorString = "Please enter serial #.";
            this.VisibleErrors_ReadDialog(errorString);
            SetFocus(this.txtSerialNo2);
            return false;
        }

        return true;

    }

    private void SetDefaultValues_ReadDialog()
    {
        //this.txtExposureDate.Text = System.DateTime.Now.ToShortDateString();
    }

    private void SetValuesToControls_ReadDialog()
    {
        int selRID = (Session["selectedRID"] != null) ? Convert.ToInt32(Session["selectedRID"]) : 0;
        string mode = (Session["mode"] != null) ? Convert.ToString(Session["mode"]) : "";

        UserDeviceRead myRead = (from o in idc.UserDeviceReads
                                 where o.RID == selRID
                                 select o).FirstOrDefault();

        bool hasRead = selRID > 0 && myRead != null;

        txtSerialNo.Visible = hasRead;
        txtSerialNo2.Visible = !hasRead;
        btnLoadSerialNo.Visible = !hasRead;

        if (hasRead)
        {            
            if (mode == "Add")    // adding mode
            {
                this.txtRID.Text = "";
                this.txtAccountID.Text = myRead.AccountID.ToString();
                this.txtSerialNo.Text = myRead.DeviceInventory.SerialNo;
                this.txtUserID.Text = myRead.UserID.Value.ToString();
                this.txtName.Text = myRead.User.LastName + ", " + myRead.User.FirstName;
                this.txtExposureDate.Text = DateTime.Today.ToShortDateString();
                this.txtExposureTime.Text = DateTime.Now.ToShortTimeString();
                this.ddlReadType.SelectedIndex = 0;
                this.txtDeep.Text = "";
                this.txtEye.Text = "";
                this.txtShallow.Text = "";
                //this.txtEDE.Text = "";
                //this.txtEDEType.Text = "";
                //this.txtEDEStatus.Text = "";
                this.ddlRegion.SelectedIndex = 0;
                this.chkAnomaly.Checked = false;                

                SetFocus(this.txtDeep);
            }
            else if (mode == "Edit")    // updating mode           
            {
                this.txtRID.Text = myRead.RID.ToString();
                this.txtAccountID.Text = myRead.AccountID.ToString();
                this.txtSerialNo.Text = myRead.DeviceInventory.SerialNo;
                this.txtUserID.Text = myRead.UserID.Value.ToString();
                this.txtName.Text = myRead.User.LastName + ", " + myRead.User.FirstName;
                this.txtExposureDate.Text = (myRead.ExposureDate == null ? "" : myRead.ExposureDate.ToShortDateString());
                //Added Time Value
                this.txtExposureTime.Text = (myRead.ExposureDate == null ? "" : myRead.ExposureDate.ToString("HH:mm:ss"));
                this.ddlReadType.SelectedValue = (myRead.ReadTypeID == null ? "0" : myRead.ReadTypeID.ToString());
                this.txtDeep.Text = (myRead.DeepLowDose == null ? "" : myRead.DeepLowDose.ToString());
                this.txtEye.Text = (myRead.EyeDose == null ? "" : myRead.EyeDose.ToString());
                this.txtShallow.Text = (myRead.ShallowLowDose == null ? "" : myRead.ShallowLowDose.ToString());
                //this.txtEDE.Text = myRead.EDEDose.HasValue ? myRead.EDEDose.Value.ToString() : "";
                //this.txtEDEType.Text = myRead.EDEDWC;  //EDETypeLookUp(myRead.EDEDWC);
                //this.txtEDEStatus.Text = myRead.EDEStatus;
                this.ddlRegion.SelectedValue = (myRead.BodyRegionID == null ? "0" : myRead.BodyRegionID.ToString());
                this.chkAnomaly.Checked = myRead.HasAnomaly;
                
                // IF ReadTypeID is for "Lifetime", "Year to Date" or "Estimate" enable DDL, ELSE disable DDL.
                if (myRead.ReadTypeID == 12 || myRead.ReadTypeID == 13 || myRead.ReadTypeID == 14 || myRead.ReadTypeID == 151)
                {
                    ddlReadType.Items.Clear();
                    ddlReadType.Items.Add(new ListItem("Lifetime", "12"));
                    ddlReadType.Items.Add(new ListItem("Year to Date", "13"));
                    ddlReadType.Items.Add(new ListItem("Estimate", "14"));
                    ddlReadType.Items.Add(new ListItem("Add Dose Record", "151"));
                    ddlReadType.SelectedValue = myRead.ReadTypeID.ToString();
                    ddlReadType.Enabled = true;
                }
                else
                {
                    ddlReadType.Enabled = false;
                }

                SetFocus(this.txtDeep);
            }

            EnableDisableEDE();
        }
        else
        {
            this.txtRID.Text = string.Empty;
            this.txtAccountID.Text = txtAccountIDSearch.Text;
            this.txtSerialNo.Text = txtSerialNoSearch.Text;
            this.txtSerialNo2.Text = this.txtSerialNo.Text;

            this.txtUserID.Text = string.Empty;
            this.txtName.Text = string.Empty;
            this.txtExposureDate.Text = DateTime.Today.ToShortDateString();
            this.txtExposureTime.Text = DateTime.Now.ToShortTimeString();
            this.ddlReadType.SelectedIndex = 0;
            this.txtDeep.Text = "";
            this.txtEye.Text = "";
            this.txtShallow.Text = "";
            this.ddlRegion.SelectedIndex = 0;
            this.chkAnomaly.Checked = false;

            if (!string.IsNullOrEmpty(this.txtSerialNo2.Text.Trim()))
            {
                LoadSerialNo();
            }

            SetFocus(this.txtSerialNo2);
        }
    }

    private string EDETypeLookUp(string pUNIXDoseWeightTypeCode)
    {
        string edeType = "";
        try
        {
            if(!string.IsNullOrEmpty(pUNIXDoseWeightTypeCode))
            {
                edeType = (from dwt in udc.DoseWeightTypes
                           where dwt.UNIXDoseWeightTypeCode == pUNIXDoseWeightTypeCode
                           select dwt.DoseWeightDesc).FirstOrDefault();
            }                
        }
        catch 
        {            
        }

        return edeType;
    }

    protected void btnLoadRead_Click(object sender, EventArgs e)
    {
        string mode = (Session["mode"] != null) ? Convert.ToString(Session["mode"]) : "";

        InVisibleErrors_ReadDialog();
        InVisibleEDEErrorNote();

        InitialSetEnableInputControls();
        InitiateReadTypeControl(mode);

        SetDefaultValues_ReadDialog();
        SetValuesToControls_ReadDialog();
    }

    protected void btnGridEdit_Click(object sender, EventArgs e)
    {
        ImageButton btn = (ImageButton)sender;
        string myCmdname = btn.CommandName.ToString();
        Session["selectedRID"] = btn.CommandArgument.ToString();
        Session["mode"] = "Edit";

        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('ReadDetailDialog')", true);
        
    }

    protected void btnGridAdd_Click(object sender, EventArgs e)
    {
        ImageButton btn = (ImageButton)sender;
        string myCmdname = btn.CommandName.ToString();
        Session["selectedRID"] = btn.CommandArgument.ToString();
        Session["mode"] = "Add";

        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('ReadDetailDialog')", true);
              
    }

    protected void btnShowAddReadDialog_Click(object sender, EventArgs e)
    {
        Session["selectedRID"] = null;
        Session["mode"] = "Add";

        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('ReadDetailDialog')", true);
    }

    protected void btnLoadSerialNo_Click(object sender, EventArgs e)
    {
        LoadSerialNo();
    }

    private void LoadSerialNo()
    { 
        string serialNo = txtSerialNo2.Text.Trim();

        var userDevice = (from ud in idc.UserDevices
                          join di in idc.DeviceInventories on ud.DeviceID equals di.DeviceID
                          where di.SerialNo == serialNo && ud.Active == true
                          orderby ud.AssignmentDate descending
                          select ud
                          ).FirstOrDefault();

        if (userDevice != null)
        {
            var user = idc.Users.FirstOrDefault(x => x.UserID == userDevice.UserID && x.Active == true);
            if (user != null)
            {
                this.txtAccountID.Text = user.AccountID.ToString();
                this.txtSerialNo.Text = serialNo;

                this.txtAccountIDSearch.Text = this.txtAccountID.Text;
                this.txtSerialNoSearch.Text = serialNo;
                this.txtUserID.Text = user.UserID.ToString();
                this.txtName.Text = user.LastName + ", " + user.FirstName;

                txtSerialNo.Visible = true;
                txtSerialNo2.Visible = false;
                btnLoadSerialNo.Visible = false;
                lblSerialNoError.InnerText = string.Empty;

                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "setSerialNoSearch('" + serialNo  + "')", true);
            }
            else
            {
                lblSerialNoError.InnerText = "Serial # is not assigned to an active user.";
            }
        }
        else
        {
            var deviceInventory = idc.DeviceInventories.FirstOrDefault(x => x.SerialNo == serialNo);
            if (deviceInventory == null)
            {
                lblSerialNoError.InnerText = "Invalid serial #.";
            }
            else
            {
                lblSerialNoError.InnerText = "Serial # is not assigned to a user.";
            }            
        }
        lblSerialNoError.Visible = !string.IsNullOrEmpty(lblSerialNoError.InnerText);
    }
    

    protected void btnCancelRead_Click(object sender, EventArgs e)
    {
        InVisibleErrors_ReadDialog();
        InVisibleEDEErrorNote();

        SetDefaultValues_ReadDialog();
        // delete session variable
        if (Session["selectedRID"] != null)
            Session.Remove("selectedRID");
        if (Session["mode"] != null)
            Session.Remove("mode");
    }

    protected void btnAddRead_Click(object sender, EventArgs e)
    {
        try
        {
            InVisibleErrors_ReadDialog();
            InVisibleEDEErrorNote();

            if (passInputsValidation_ReadDialog())
            {
                int myRID = (Session["selectedRID"] != null) ? Convert.ToInt32(Session["selectedRID"]) : 0;
                string mode = (Session["mode"] != null) ? Convert.ToString(Session["mode"]) : "";

                Double? myDouble = null;

                Double myDeepLowDose = Double.Parse(this.txtDeep.Text);
                Double? myEyeDose = (this.txtEye.Text.Trim().Length == 0) ? myDouble : Double.Parse(this.txtEye.Text.Trim());
                Double myShallowLowDose = Double.Parse(this.txtShallow.Text);

                int myReadTypeID = int.Parse(this.ddlReadType.SelectedItem.Value);

                //Add Date and Time together 
                string date = DateTime.Parse(this.txtExposureDate.Text).ToShortDateString();
                string fixtime = this.txtExposureTime.Text;
                string datetime = date + " " + fixtime;
                DateTime selectedExposureDate = DateTime.Parse(datetime);

                //DateTime selectedExposureDate = DateTime.Parse(this.txtExposureDate.Text);

                int myBodyRegionID = int.Parse(this.ddlRegion.SelectedItem.Value);

                bool myHasAnomaly = this.chkAnomaly.Checked;
                DateTime myModifiedDate = System.DateTime.Now;

                int myAccountID = int.Parse(this.txtAccountID.Text);
                int myUserID = int.Parse(this.txtUserID.Text);
                User user = (from u in idc.Users
                             where u.UserID == myUserID
                             select u).FirstOrDefault();

                int myUserLocationID = user.LocationID;
                int? myUserGroupID = user.GroupID;
                DeviceInventory device = (from d in idc.DeviceInventories
                                          where d.SerialNo == this.txtSerialNo.Text
                                          select d).FirstOrDefault();

                DateTime ExposureDate = selectedExposureDate;

                DateTime CreatedDate = TimeZoneInfo.ConvertTimeFromUtc(TimeZoneInfo.ConvertTimeToUtc(selectedExposureDate, TimeZoneInfo.FindSystemTimeZoneById(user.Location.TimeZone.TimeZoneName)), TimeZoneInfo.Local);
                if (mode == "Add")  // add new reading
                {
                    // Insert a new dose
                    UserDeviceRead udr = new UserDeviceRead
                    {
                        DeviceID = device.DeviceID,
                        UserID = myUserID,
                        AccountID = myAccountID,
                        LocationID = myUserLocationID,
                        GroupID = myUserGroupID,

                        ReadTypeID = myReadTypeID,
                        BodyRegionID = myBodyRegionID,
                        ExposureDate = ExposureDate,
                        DeepLowDose = myDeepLowDose,
                        EyeDose = myEyeDose,
                        ShallowLowDose = myShallowLowDose,
                        EDEDose = string.IsNullOrEmpty(this.txtEDE.Text) ? (Double?)null : Double.Parse(this.txtEDE.Text),
                        EDEDWC = string.IsNullOrEmpty(this.txtEDEType.Text) ? null : this.txtEDEType.Text,
                        EDEStatus = string.IsNullOrEmpty(this.txtEDEStatus.Text) ? null : this.txtEDEStatus.Text,
                        HasAnomaly = myHasAnomaly,
                        CreatedDate = CreatedDate,
                        ModifiedDate = myModifiedDate,

                        DeviceReadAppID = 2,
                        TimeZoneID = user.Location.TimeZone.TimeZoneID,
                        InitRead = false,
                        DeepLow = 0,
                        DeepLowTemp = 0,
                        DeepHigh = 0,
                        DeepHighTemp = 0,
                        DeepHighDose = 0,
                        ShallowLow = 0,
                        ShallowLowTemp = 0,
                        ShallowHigh = 0,
                        ShallowHighTemp = 0,
                        ShallowHighDose = 0,
                        BackgroundExposure = 0,
                        DeepLowDoseCalc = null,
                        ShallowLowDoseCalc = null,
                        DeepHighDoseCalc = null,
                        ShallowHighDoseCalc = null,
                        CumulativeDose = 0,
                        PriorHistory = null,
                        BackgroundAdjUsed = null,
                        DeepLowDoseFactorUsed = null,
                        ShallowLowDoseFactorUsed = null,
                        ErrorText = null,
                        CalculationVersion = null,
                        ReaderVersion = null

                    };

                    idc.UserDeviceReads.InsertOnSubmit(udr);
                    idc.SubmitChanges();

                    myRID = udr.RID;

                    // refresh gridview
                    grdReadingView.DataBind();

                    ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('ReadDetailDialog')", true);
                }
                else    // update reading
                {
                    //Write Audit trail record
                    string modUser = System.Web.HttpContext.Current.User.Identity.Name;

                    UserDeviceRead udr = (from u2 in idc.UserDeviceReads
                                          where u2.RID == myRID
                                          select u2).FirstOrDefault();
                    if (udr == null)
                        throw new Exception("Read was not found!");

                    UserDeviceReadsHistory newUserDeviceReadsHistory = new UserDeviceReadsHistory()
                    {
                        RID = myRID,
                        PriorExposureDate = udr.ExposureDate,
                        PriorAnomaly = udr.HasAnomaly,
                        PriorDeepLowDose = udr.DeepLowDose,
                        PriorEyeDose = udr.EyeDose,
                        PriorShallowLowDose = udr.ShallowLowDose,
                        PriorBodyRegionID = udr.BodyRegionID,
                        PriorReadTypeID = udr.ReadTypeID,
                        ModifiedDate = System.DateTime.Now,
                        ModifiedBy = modUser
                    };

                    idc.UserDeviceReadsHistories.InsertOnSubmit(newUserDeviceReadsHistory);

                    //Update the adjusted dose
                    udr.DeepLowDose = myDeepLowDose;
                    udr.EyeDose = myEyeDose;
                    udr.ShallowLowDose = myShallowLowDose;
                    udr.EDEDose = string.IsNullOrEmpty(this.txtEDE.Text) ? (Double?)null : Double.Parse(this.txtEDE.Text);
                    udr.EDEDWC = string.IsNullOrEmpty(this.txtEDEType.Text) ? null : this.txtEDEType.Text;
                    udr.EDEStatus = string.IsNullOrEmpty(this.txtEDEStatus.Text) ? null : this.txtEDEStatus.Text;
                    //udr.ExposureDate = myExposureDate;
                    udr.ExposureDate = ExposureDate;
                    udr.BodyRegionID = myBodyRegionID;
                    // IF the ReadTypeID is 17 (User Read), then updated ReadType to "Adjustment".
                    // ELSE maintain ReadTypeID associated to the Read ID.
                    if (udr.ReadTypeID == 17)
                    {
                        udr.ReadTypeID = (from r in idc.ReadTypes
                                          where r.ReadTypeName == "Adjustment"
                                          select r.ReadTypeID).FirstOrDefault();
                    }
                    else
                    {
                        udr.ReadTypeID = myReadTypeID;
                    }
                    udr.HasAnomaly = myHasAnomaly;
                    //udr.CreatedDate = myCreatedDate;
                    udr.CreatedDate = CreatedDate;
                    udr.ModifiedDate = myModifiedDate;


                    // if the read is changed to Anomaly then updating the lastreadID on the device.
                    if (myHasAnomaly && isLastRead(myRID, device))
                    {
                        int preLastReadID = GetPreviousLastRead(myRID, device);
                        if (preLastReadID > 0)
                            device.LastReadID = preLastReadID;
                        else
                            device.LastReadID = null;
                    }

                    idc.SubmitChanges();

                    // refresh gridview
                    grdReadingView.DataBind();

                    ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('ReadDetailDialog')", true);
                }
                
                Check_ReCalculateExistingEDEDose(myRID);                

            }
        }
        catch (Exception ex)
        {
            // Display the system generated error message.
            this.VisibleErrors_ReadDialog(string.Format("An error occurred: {0}", ex.Message));
        }        
    }
    /// <summary>
    /// check and re-calculate existing EDEDose within the period if some dose within the period which is not an EDE dose is changed.
    /// </summary>
    /// <param name="pRID">ReadID</param>
    private void Check_ReCalculateExistingEDEDose(int pRID)
    {
        string doseWeightCode = "";

        UserDeviceRead udr = (from u2 in idc.UserDeviceReads
                              where u2.RID == pRID
                              select u2).FirstOrDefault();
                
        if ((udr.BodyRegion.BodyRegionName == "Collar" || udr.BodyRegion.BodyRegionName == "Torso") 
            && IsEDEUser(udr.UserID.Value, udr.ExposureDate, ref doseWeightCode) 
            && edeReadTypeList.Any(udr.ReadType.ReadTypeName.Contains))
        {
            // Double Webster and not an EDEDose
            if (doseWeightCode.Substring(0, 1) == "D" && udr.EDEDose == null)
            {
                DateTime nextScheduleDate = (from sd in idc.ScheduleDates
                                             where sd.ScheduleID == udr.DeviceInventory.ScheduleID && sd.Date > udr.ExposureDate
                                             orderby sd.Date ascending select sd.Date).FirstOrDefault();

                if (nextScheduleDate != null)
                {                   
                    int edeDose_RID = (from cr in idc.CalendarReads
                                        join ur in idc.UserDeviceReads on cr.RID equals ur.RID
                                        join ud in idc.UserDevices on ur.DeviceID equals ud.DeviceID 
                                        where ur.EDEDose != null && ur.EDEDWC.Substring(0,1) == "D"
                                        && cr.ScheduleID == udr.DeviceInventory.ScheduleID && cr.ScheduleDate == nextScheduleDate
                                        && ud.UserID == udr.UserID && (ud.DeactivateDate == null || ud.DeactivateDate >= nextScheduleDate)
                                       select cr.RID).FirstOrDefault();

                    // if there is existing EDEDose in the period, then re-calculate it.
                    if (edeDose_RID > 0)
                    {
                        // Calculate and store EDE dose value.
                        var myEDEDoseCalc = idc.sp_CalculateAndStoreEDEDose(edeDose_RID).FirstOrDefault();
                        if (myEDEDoseCalc != null)
                        {
                            // Get ede dose value
                            // If vEDEDose = -1: implying missing DOUBLE EDE data to calculate.
                            // If vEDEDose = -2: implying incomplete EDE reads to calculate.
                            // If vEDEDose = -3: implying NOT an EDE USER read.
                            // If vEDEDose >= 0: returning EDEDose   
                            // If vEDEDose = -4: implying function call sp_CalculateAndStoreEDEDose error  (myEDEDoseCalc.EDEDose.HasValue = false)                                           
                        }
                    }
                }
            }
        }
    }

    private bool isLastRead(int myRID, DeviceInventory device)
    {
        if (device.LastReadID != null && device.LastReadID == myRID)
            return true;
        else
            return false;
    }

    private int GetPreviousLastRead(int curLastReadID, DeviceInventory device)
    {        
        int RID = (from u in idc.UserDeviceReads
               join t in idc.ReadTypes on u.ReadTypeID equals t.ReadTypeID
               where u.HasAnomaly == false && u.RID != curLastReadID
               && u.DeviceID == device.DeviceID
               && (t.ReadTypeName == "User Read" || t.ReadTypeName == "Adjustment")              
               orderby u.ExposureDate descending
               select u.RID).FirstOrDefault();

        return RID;
    }

    protected void ddlRegion_SelectedIndexChanged(object sender, EventArgs e)
    {        
        EnableDisableEDE();
    }

    protected void txtExposureDate_TextChanged(object sender, EventArgs e)
    {
        EnableDisableEDE();
    }

    protected void txtExposureTime_TextChanged(object sender, EventArgs e)
    {
        EnableDisableEDE();
    }

    protected void ddlReadType_SelectedIndexChanged(object sender, EventArgs e)
    {
        EnableDisableEDE();
    }

    // Only allow to add/edit EDEDose on BodyRegion = Collar, EDE Users and certain read types
    private bool IsEDEAllowed(ref string pDoseWeightCode)
    {
        bool isAllow = false;
        try
        {
            DateTime exposureDateTime = DateTime.Parse(this.txtExposureDate.Text + " " + this.txtExposureTime.Text);

            if (this.ddlRegion.SelectedItem.Text == "Collar" && IsEDEUser(Int32.Parse(this.txtUserID.Text), exposureDateTime, ref pDoseWeightCode) && edeReadTypeList.Any(ddlReadType.SelectedItem.Text.Contains))
            {
                isAllow = true;
            }
        }
        catch
        {
        }        
        return isAllow;
    }

    private void EnableDisableEDE()
    {
        this.txtEDE.Text = "";
        this.txtEDEStatus.Text = "";
        this.txtEDEType.Text = "";

        this.txtEDE.Enabled = false;
        this.btnCalculateEDE.Enabled = false;

        this.txtExposureDate.Enabled = true;
        this.txtExposureTime.Enabled = true;
        this.ddlRegion.Enabled = true;

        int selRID = (Session["selectedRID"] != null) ? Convert.ToInt32(Session["selectedRID"]) : 0;
        string mode = (Session["mode"] != null) ? Convert.ToString(Session["mode"]) : "";

        UserDeviceRead myRead = (from o in idc.UserDeviceReads
                                 where o.RID == selRID
                                 select o).FirstOrDefault();        

        string doseWeightCode = "";        
        if (IsEDEAllowed(ref doseWeightCode) )
        {
            // For single webster: allowing to calculate and store EDEDose in both Add and Edit mode.
            // For double webster: allowing to edit EDEDose of Calendar Reads but do not allow to change ExposureDate and BodyRegion
            // For double webster: allowing to Add EDEDose but manually entered value            

            if (doseWeightCode.Substring(0, 1) == "S") // For single webster
            {                
                // Allow to edit on EDEDose
                if (mode == "Edit" && !string.IsNullOrEmpty(myRead.EDEDWC))
                {
                    this.txtEDE.Enabled = true;
                    this.btnCalculateEDE.Enabled = true;

                    this.txtEDE.Text = myRead.EDEDose.HasValue ? myRead.EDEDose.Value.ToString() : "";
                    this.txtEDEType.Text = myRead.EDEDWC;  //EDETypeLookUp(myRead.EDEDWC);
                    this.txtEDEStatus.Text = myRead.EDEStatus;
                }   
                else if (mode == "Add" ) 
                {
                    this.txtEDE.Enabled = true;
                    this.btnCalculateEDE.Enabled = true;
                }
            }
            else  // For double webster
            {                
                // allowing to edit Double Webster EDEDose of Calendar Reads for a certain products
                if (mode == "Edit" && edeProductList.Any(myRead.DeviceInventory.Product.ProductGroup.ProductGroupName.Contains) && !string.IsNullOrEmpty(myRead.EDEDWC)) 
                {
                    this.txtEDE.Enabled = true;
                    this.btnCalculateEDE.Enabled = true;

                    this.txtEDE.Text = myRead.EDEDose.HasValue ? myRead.EDEDose.Value.ToString() : "";
                    this.txtEDEType.Text = myRead.EDEDWC;  //EDETypeLookUp(myRead.EDEDWC);
                    this.txtEDEStatus.Text = myRead.EDEStatus;

                    // Do not allow to change ExposureDate because it is a calendar day which is triggered to calculate EDE dose
                    // Do not allow to change Body Region because once it is changed by logic this double webster EDE dose will become Incomplete and have EDEDose = null. Double EDE dose calculation needs doses of both Collar and Torso
                    this.txtExposureDate.Enabled = false;
                    this.txtExposureTime.Enabled = false;
                    this.ddlRegion.Enabled = false;                    
                }
                else if (mode == "Add" && edeProductList.Any(myRead.DeviceInventory.Product.ProductGroup.ProductGroupName.Contains) && !string.IsNullOrEmpty(doseWeightCode) ) 
                {
                    //allowing to Add EDEDose but manually entered value
                    this.txtEDE.Enabled = true;
                }
            }                 
        }        
    }

    private bool IsEDEUser(int pUserID, DateTime pExposureDate, ref string pDoseWeightCode)
    {
        var userinfo = (from u in idc.Users
                        join a in idc.Accounts on u.AccountID equals a.AccountID
                        join l in idc.Locations on u.LocationID equals l.LocationID
                        where u.UserID == pUserID
                        select new
                        {
                            GDSAccount = a.GDSAccount,
                            GDSLocation = l.GDSLocation,
                            GDSWearerID = u.GDSWearerID
                        }
                        ).FirstOrDefault();

        if (userinfo != null)
        {
            var ededates = (from w in udc.WearerEDEDates
                            where
                            w.GDSAccount == userinfo.GDSAccount
                            &&
                            w.GDSLocation == userinfo.GDSLocation
                            &&
                            w.GDSWearer == userinfo.GDSWearerID
                            && 
                            w.BeginWearDate <= pExposureDate
                            &&
                            (w.EndWearDate == null || w.EndWearDate >= pExposureDate)
                            &&
                            w.DoseWeightCode != "NN"
                            && 
                            w.DoseWeightCode.Length > 0 
                            orderby w.UnixRecord 
                            select w).FirstOrDefault();
            if (ededates != null)
            {
                pDoseWeightCode = ededates.DoseWeightCode;
                return true;
            }
        }
        return false;
    }   

    protected void btnCalculateEDE_Click(object sender, EventArgs e)
    {
        InVisibleErrors_ReadDialog();
        InVisibleEDEErrorNote();

        int selRID = (Session["selectedRID"] != null) ? Convert.ToInt32(Session["selectedRID"]) : 0;
        string mode = (Session["mode"] != null) ? Convert.ToString(Session["mode"]) : "";

        UserDeviceRead myRead = (from o in idc.UserDeviceReads
                                 where o.RID == selRID
                                 select o).FirstOrDefault();

        if (myRead != null)
        {
            int userID = myRead.UserID.Value;
            int locationID = myRead.LocationID.Value;
            DateTime exposureDate = DateTime.Parse(this.txtExposureDate.Text + " " + this.txtExposureTime.Text);
            Double deepDose = Double.Parse(txtDeep.Text);
            string bodyRegion = this.ddlRegion.SelectedItem.Text;                                               // 'Collar' or 'Torso'
            string productGroup = myRead.DeviceInventory.Product.ProductGroup.ProductGroupName;                 //'Instadose 2', 'Instadose Plus', 'Instadose 2 New'
            string vUOM = myRead.Location.LocationUOM;

            // Calculate and store EDE dose value.
            var myEDEDoseCalc = idc.sp_CalculateEDEDose(myRead.RID, userID, locationID, exposureDate, deepDose, bodyRegion, productGroup, vUOM).FirstOrDefault();
            if (myEDEDoseCalc != null)
            {
                // Get ede dose value
                // If vEDEDose = -1: implying missing either Torso or Collar doses in the calendar period for calculating Double Webster.
                // If vEDEDose = -2: implying Incomplete EDE reads in the calendar period or device does not belong to EDE specified products.
                // If vEDEDose = -3: implying NOT an EDE USER read.
                // If vEDEDose >= 0: returning EDEDose   
                // If vEDEDose = -4: implying function call sp_CalculateEDEDose error     

                this.txtEDE.Text = (myEDEDoseCalc.EDEDose.HasValue ? myEDEDoseCalc.EDEDose.Value : -4).ToString();
                this.txtEDEType.Text = myEDEDoseCalc.EDEType ?? "";
                this.txtEDEStatus.Text = (myEDEDoseCalc.EDEStatus.HasValue ? myEDEDoseCalc.EDEStatus.Value : ' ').ToString();

                // if vEDEDose < 0, then display the EDEDose issue
                DisplayEDEErrorNote();
            }            
        }

    }

    private void DisplayEDEErrorNote()
    {
        if (double.Parse(this.txtEDE.Text) < 0)
        {
            string edeErrorNote = "";
            switch (double.Parse(this.txtEDE.Text))
            {
                case -1:
                    edeErrorNote = "Missing either Torso or Collar doses in the calendar period for calculating Double Webster";
                    break;
                case -2:
                    edeErrorNote = "Incomplete Double Webster EDE dose";
                    break;
                case -3:
                    edeErrorNote = "Not an EDE user";
                    break;
                case -4:
                    edeErrorNote = "Calculate EDE error. Check with IT";
                    break;
                default:
                    edeErrorNote = "EDE Dose can not be negative.";
                    break;
            }

            VisibleEDEErrorNote(edeErrorNote);
        }
    }

    private void InVisibleEDEErrorNote()
    {
        this.lblEDEErrorNote.Text = "";
        this.edeErrorNote.Visible = false;
    }

    private void VisibleEDEErrorNote(string pNote)
    {
        this.lblEDEErrorNote.Text = "(" + pNote + ")";
        this.edeErrorNote.Visible = true;
    }

    protected void txtEDE_TextChanged(object sender, EventArgs e)
    {
        InVisibleEDEErrorNote();

        int selRID = (Session["selectedRID"] != null) ? Convert.ToInt32(Session["selectedRID"]) : 0;
        string mode = (Session["mode"] != null) ? Convert.ToString(Session["mode"]) : "";

        UserDeviceRead myRead = (from o in idc.UserDeviceReads
                                 where o.RID == selRID
                                 select o).FirstOrDefault();

        string doseWeightCode = "";        
        if (IsEDEAllowed(ref doseWeightCode))
        {
            // For single webster: allowing to calculate and store EDEDose when adding or editting a read.
            // For double webster: only allowing to edit EDEDose of Calendar Reads. 
            if (!string.IsNullOrEmpty(doseWeightCode) && doseWeightCode.Substring(0, 1) == "D" && mode == "Add" && edeProductList.Any(myRead.DeviceInventory.Product.ProductGroup.ProductGroupName.Contains)) // For double webster and ADD mode
            {
                this.txtEDEStatus.Text = "";
                this.txtEDEType.Text = "";

                if (double.TryParse(this.txtEDE.Text, out double enteredEDEDose))
                {
                    // For double EDE and in Add mode, force EDEStatus = "C" if manual EDE value is entered
                    if (enteredEDEDose >= 0)
                    {
                        this.txtEDEStatus.Text = "C";
                        this.txtEDEType.Text = doseWeightCode;
                    }
                }
            }            
        }

        // if vEDEDose < 0, then display the EDEDose issue
        DisplayEDEErrorNote();
    }


    // ------------------------- Modal Dialog Section FUNCTIONS here ------------------------------------- //




}