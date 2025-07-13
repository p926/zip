using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Mail;
using System.Data;

using Instadose;
using Instadose.Data;
using Instadose.Processing;
using System.IO;

using Mirion.DSD.GDS.API;
using Mirion.DSD.GDS.API.Contexts;
using Mirion.DSD.GDS.API.DataTypes;
using Mirion.DSD.GDS.API.XAVService;
using Mirion.DSD.GDS.API.Helpers;
using System.Web.Script.Serialization;
using Newtonsoft.Json;
using Mirion.DSD.Utilities.Constants;
using Mirion.DSD.GDS.API.Models;
using System.Text;
using System.Globalization;
/// <summary>
/// Initiate New Retrun Request (RMA Department)
/// </summary>
/// 

public partial class TechOps_ReturnAddNewDeviceRMA : System.Web.UI.Page
{
    // Turn on/off email to all others
    bool DevelopmentServer = false;

    //Instadose Account;
    Account account = null;

    // String to hold (if applicable) current Serial Number.
    public string serialNumberInput = "";
    public string accountInput = "";

    // String to hold the current username
    string UserName = "Unknown";

    bool isSingleFlow = true;

    // Send email to TechOps if user's email =null
    string TechOpsEmailAddress = "khindra@mirion.com";

    const string _FedExOvernightShippingMethodName = "FedEx Standard Overnight";

    // Create the database reference
    InsDataContext idc = new InsDataContext();
    AppDataContext adc = new AppDataContext();
    MASDataContext mdc = new MASDataContext();
    UnixDataClassesDataContext udc = new UnixDataClassesDataContext();

    protected void Page_Load(object sender, EventArgs e)
    {
        this.Page.Title = "Return/RMA - Technical RMA Initiation";
        Common.idc = this.idc; // Make sure Common object has correct connection

        // Auto set if a development site
        if (Request.Url.Authority.ToString().Contains(".mirioncorp.com") || Request.Url.Authority.ToString().Contains("localhost"))
            DevelopmentServer = true;

        // Try to set the username
        try
        {
            InvisibleErrors();
            InvisibleSuccess();

            this.isSingleFlow = this.rdobtnBySN.Checked;            
            if (User.Identity.Name.IndexOf('\\') > 0)
                this.UserName = User.Identity.Name.Split('\\')[1];
            else
                this.UserName = "Testing";

        }
        catch { this.UserName = "Unknown"; }

        if (Page.IsPostBack) return;

        // Request.QueryString["SerialNo"] = null: then recall made by techops
        // other: recall made by CS
        if (Request.QueryString["SerialNo"] != null)
        {
            serialNumberInput = Request.QueryString["SerialNo"];
            string actID = Request.QueryString["AccountID"];
            string locationCode = Request.QueryString["Location"];
            string applicationID = Request.QueryString["ApplicationID"];

            // Recall made by dose analysis app
            if (serialNumberInput != "0")
            {
                txtSerialNoInput.Text = serialNumberInput;
                ViewState["RecallByDepartmentID"] = 3;
                rdobtnBySN.Checked = true;
                this.isSingleFlow = this.rdobtnBySN.Checked;

                string errorMsg = "";
                ViewState["IsGDSAccount"] = identifyGDSAccountBySerialNo(serialNumberInput, ref errorMsg);

                if (errorMsg.Length > 0)
                {
                    this.VisibleErrors(errorMsg);
                    VisibleRecallBody(false);
                    return;
                }

                GetRMAInformationBySerialNo(serialNumberInput);
            }
            else // Recall made by CS
            {
                ViewState["RecallByDepartmentID"] = 2;

                // applicationID = 2: recall by CS in portal; applicationID = 1: recall by CS in CSWS;
                if (applicationID == "1")
                {
                    rdobtnByGDSAccount_CheckedChanged(null, null);
                    this.rdobtnByGDSAccount.Checked = true;
                }

                if (applicationID == "2")
                {
                    rdobtnByInstadoseAccount_CheckedChanged(null, null);
                    this.rdobtnByInstadoseAccount.Checked = true;
                }

                this.isSingleFlow = false;
                this.txtAccountInput.Text = actID;
                LoadRMAInformation(locationCode);
            }
        }
        else
        {
            ViewState["RecallByDepartmentID"] = 3;
            rdobtnBySN.Checked = true;
            this.txtSerialNoInput.Focus();
        }

        LoadDDLCountry(ddlAddrCountry);
        ddlAddrCountry.SelectedValue = "1"; // default USA
        LoadDDLState(ddlAddrState, ddlAddrCountry.SelectedValue);
    }
    protected void btnGo_Click(object sender, EventArgs e)
    {
        LoadRMAInformation();
    }

    private void LoadRMAInformation(string pLocationCode = "")
    {
        Session.Remove("SelectedDeviceList");

        if (this.txtSerialNoInput.Text == "" && this.txtAccountInput.Text == "")
        {
            return;
        }
        else
        {
            serialNumberInput = this.txtSerialNoInput.Text.Trim();
            accountInput = this.txtAccountInput.Text.Trim();

            if (this.isSingleFlow)
            {
                string errorMsg = "";
                ViewState["IsGDSAccount"] = identifyGDSAccountBySerialNo(serialNumberInput, ref errorMsg);

                if (errorMsg.Length > 0)
                {
                    this.VisibleErrors(errorMsg);
                    VisibleRecallBody(false);
                    return;
                }

                GetRMAInformationBySerialNo(serialNumberInput);
            }
            else
            {
                string errorMsg = "";
                ViewState["IsGDSAccount"] = identifyGDSAccountByAccountNo(accountInput, ref errorMsg);

                if (errorMsg.Length > 0)
                {
                    this.VisibleErrors(errorMsg);
                    VisibleRecallBody(false);
                    return;
                }

                GetRMAInformationByAccountNo(accountInput, pLocationCode);
            }

            // Load ProRate Period Radio Button for non IC Care Instadose account 
            //if (!(bool)ViewState["IsGDSAccount"] && this.account.BrandSourceID != 3)
            if (!(bool)ViewState["IsGDSAccount"])
            {
                ProRatePeriod.Visible = true;
                if (int.TryParse(ddlLocation.SelectedValue, out int tmpLocID))
                    LoadProratePeriodRadioButton(this.account, tmpLocID);
                else
                    LoadProratePeriodRadioButton(this.account, null);
            }
            else
            {
                ProRatePeriod.Visible = false;
            }
        }
    }

    protected void LoadDDLCountry(DropDownList DDLName)
    {
        DDLName.Items.Clear();

        DDLName.DataSource = (from a in idc.Countries
                              orderby a.CountryName
                              select a);
        DDLName.DataBind();

        ListItem firstItem = new ListItem("  -- Select Country --", "0");
        DDLName.Items.Insert(0, firstItem);
        DDLName.SelectedIndex = 0;
    }
    protected void LoadDDLState(DropDownList DDLName, string SelectedCountryID)
    {
        DDLName.Items.Clear();

        var states = (from a in idc.States
                      orderby a.CountryID, a.StateAbbrev
                      select a);
        var stateList = states.Select(a => new { a.StateID, StateAbbName = a.StateAbbrev.ToString() + " - " + a.StateName.ToString() }).ToList();
        if (SelectedCountryID != "0")
        {
            stateList = states.Where(a => a.CountryID == int.Parse(SelectedCountryID))
                             .Select(a => new { a.StateID, StateAbbName = a.StateAbbrev.ToString() + " - " + a.StateName.ToString() }).ToList();
        }
        DDLName.DataSource = stateList;
        DDLName.DataBind();

        ListItem firstItem = new ListItem("  -- Select State --", "0");
        DDLName.Items.Insert(0, firstItem);
    }
    protected void LoadLocation(Account pAccount)
    {
        ddlLocation.Items.Clear();

        if ((bool)ViewState["IsGDSAccount"])
        {
            List<GLocationListItem> locationList = new List<GLocationListItem>();
            LocationRequests locationRequest = new LocationRequests();

            locationList = locationRequest.GetLocations(pAccount.GDSAccount);

            var locList = (from GLocationListItem l in locationList
                           where l.Location != "00000"
                           select new
                           {
                               Location = l.Location,
                               LocationDesc = l.Location + " : " + l.LocationName + "( " + l.MfgWP + " )",
                           }).Distinct();

            ddlLocation.DataSource = locList;
            ddlLocation.DataValueField = "Location";
            ddlLocation.DataTextField = "LocationDesc";
            ddlLocation.DataBind();
        }
        else
        {
            this.ddlLocation.DataSource = (from a in idc.Locations
                                           where a.AccountID == pAccount.AccountID
                                           orderby a.LocationName
                                           select a);
            this.ddlLocation.DataValueField = "LocationID";
            this.ddlLocation.DataTextField = "LocationName";
            this.ddlLocation.DataBind();
        }
    }
    protected void LoadProduct()
    {
        this.ddlProduct.Items.Clear();

        // Do not need to include old ID1 group as we are using Instadose 1 IMI for old ID1
        List<ProductGroup> productGroupList = (from a in idc.ProductGroups
                                               where a.isDevice == true && a.ProductGroupID != 1
                                               orderby a.ProductGroupName
                                               select a).ToList();

        // Do not offer multiple recall functionality for ID1 products
        if (!this.isSingleFlow)
        {
            productGroupList = (from a in idc.ProductGroups
                                where a.isDevice == true && a.ProductGroupID != 1 && a.ProductGroupID != 9
                                orderby a.ProductGroupName
                                select a).ToList();
        }
        
        foreach (ProductGroup productGroup in productGroupList)
        {
            string productGroupName = "";
            // Customize displaying product name
            switch (productGroup.ProductGroupName)
            {
                case "Instadose 1 IMI":
                    productGroupName = "Instadose 1";
                    break;
                case "Instadose 2":
                    productGroupName = "ID2 Elite";
                    break;
                case "Instadose 2 New":
                    productGroupName = "ID2";
                    break;
                default:
                    productGroupName = productGroup.ProductGroupName;
                    break;
            }
            ListItem listItemProductGroup = new ListItem(productGroupName, productGroup.ProductGroupID.ToString());
            this.ddlProduct.Items.Add(listItemProductGroup);
        }
    }
    protected void ddlAddrCountryOnSelectedIndexChange(object sender, System.EventArgs e)
    {
        DropDownList ddlC = (DropDownList)sender;
        this.AddressVerifyRow.Visible = false;

        if (ddlC.SelectedValue == "1" || ddlC.SelectedValue == "8")   //United State or Canada
        {
            LoadDDLState(ddlAddrState, ddlC.SelectedValue);
            this.StateRow.Visible = true;

            if (ddlC.SelectedValue == "1")
            {
                this.AddressVerifyRow.Visible = true;
            }
        }
        else
        {
            ddlAddrState.Items.Clear();
            this.StateRow.Visible = false;
        }
    }
    protected void btnAssignedLocation_Click(object sender, EventArgs e)
    {
        foreach (GridViewRow row in this.gv_LocationList.Rows)
        {
            RadioButton radButton = (RadioButton)row.FindControl("radSelectLocation");

            if (radButton.Checked)
            {
                //HiddenField hidLocationID = (HiddenField)row.FindControl("hidLocationID");
                //HiddenField hidStateID = (HiddenField)row.FindControl("hidStateID");
                //HiddenField hidCountryID = (HiddenField)row.FindControl("hidCountryID");
                Label lblLocationID = (Label)row.FindControl("lblLocationID");
                Label lblStateID = (Label)row.FindControl("lblStateID");
                Label lblCountryID = (Label)row.FindControl("lblCountryID");
                Label lblTelephone = (Label)row.FindControl("lblTelephone");

                SetLocationAddress(lblLocationID.Text, row.Cells[2].Text, row.Cells[4].Text, row.Cells[5].Text, row.Cells[6].Text, row.Cells[8].Text, lblTelephone.Text, row.Cells[7].Text, lblStateID.Text, row.Cells[9].Text, lblCountryID.Text, false, false);

                //ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "$('#assignedLocationDialog').dialog('close');", true);
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('assignedLocationDialog')", true);
                break;
            }
        }
    }
    private void SetLocationAddress(string locationID, string ContactName, string address1, string address2, string city, string zip, string phone, string state, string stateID, string country, string countryID, bool isAddrValidation, bool editable)
    {
        txtLocationID.Text = locationID;
        txtAddrContactName.Text = ContactName.Trim().Replace("&nbsp;", "");
        txtAddrAddress1.Text = address1.Trim().Replace("&nbsp;", "");
        txtAddrAddress2.Text = address2.Trim().Replace("&nbsp;", "");
        txtAddrCity.Text = city.Trim().Replace("&nbsp;", "");
        txtAddrZipCode.Text = zip.Trim().Replace("&nbsp;", "");
        txtAddrPhone.Text = phone.Trim().Replace("&nbsp;", "");
        chkBoxAddrDisableVerification.Checked = isAddrValidation;

        if (countryID == "0") // GDS Address
        {
            ddlAddrCountry.SelectedValue = (from c in idc.Countries where c.PayPalCode == country select c.CountryID).FirstOrDefault().ToString();
            LoadDDLState(ddlAddrState, ddlAddrCountry.SelectedValue);
            ddlAddrState.SelectedValue = (from s in idc.States where s.StateAbbrev == state select s.StateID).FirstOrDefault().ToString();
        }
        else
        {
            ddlAddrCountry.SelectedValue = countryID;
            LoadDDLState(ddlAddrState, ddlAddrCountry.SelectedValue);
            ddlAddrState.SelectedValue = stateID;
        }

        EnableOrDisableOtherLocationFields(editable);

        this.AddressVerifyRow.Visible = editable;
    }
    protected void btnExistingLocations_Click(object sender, EventArgs e)
    {
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('assignedLocationDialog')", true);
    }
    protected void btnNewLocations_Click(object sender, EventArgs e)
    {
        SetLocationAddress("0", "", "", "", "", "", "", "", "0", "", "1", false, true);
    }
    private void LoadWearDates(string GDSaccount, string GDSlocation)
    {
        ddlWearDate.Items.Clear();

        if ((bool)ViewState["IsGDSAccount"])
        {
            LocationRequests locReqs = new LocationRequests();
            var weardates = locReqs.GetWearDates(GDSaccount, GDSlocation, null);

            weardates.OrderBy(w => w.WearDate).ToList();

            foreach (GWearDate item in weardates)
            {
                ddlWearDate.Items.Add(item.WearDate.ToShortDateString());
            }
        }
    }
    private void LoadReturnReasons()
    {
        this.ddlReturnReason.Items.Clear();
        List<rma_ref_ReturnReason> returnReasons = (from rs in adc.rma_ref_ReturnReasons
                                                   where rs.Active == true && rs.DepartmentID == 3  // RMA department =3(recall)
                                                   //&& rs.UseByDepartmentID == int.Parse(ViewState["RecallByDepartmentID"].ToString())
                                                   orderby rs.Description
                                                   select rs
                                                ).ToList();

        // Load reason by product group
        ListItem firstItem = new ListItem("-- Select Reason--", "0");
        this.ddlReturnReason.Items.Add(firstItem);
        foreach (rma_ref_ReturnReason reason in returnReasons)
        {
            int productGroupId;
            int.TryParse(ddlProduct.SelectedValue, out productGroupId);

            bool isAddReason = false;

            switch (productGroupId)
            {
                case ProductGroupIDConstants.Instadose1:
                case ProductGroupIDConstants.Instadose1IMI:
                    if (reason.ID1.HasValue && reason.ID1.Value == true)
                    {
                        isAddReason = true;
                    }
                    break;
                case ProductGroupIDConstants.InstadosePlus:
                    if (reason.IDPlus.HasValue && reason.IDPlus.Value == true)
                    {
                        isAddReason = true;
                    }
                    break;
                case ProductGroupIDConstants.InstadoseVue:
                    if (reason.ID3.HasValue && reason.ID3.Value == true)
                    {
                        isAddReason = true;
                    }
                    break;
                case ProductGroupIDConstants.InstadoseVueBeta:
                    if (reason.IDVueBeta.HasValue && reason.IDVueBeta.Value == true)
                    {
                        isAddReason = true;
                    }
                    break;
                case ProductGroupIDConstants.Instadose2:
                case ProductGroupIDConstants.Instadose2New:
                    if (reason.ID2.HasValue && reason.ID2.Value == true)
                    {
                        isAddReason = true;
                    }
                    break;
                case ProductGroupIDConstants.InstaLink:
                    if (reason.Instalink.HasValue && reason.Instalink.Value == true)
                    {
                        isAddReason = true;
                    }
                    break;
                case ProductGroupIDConstants.InstaLinkUSB:
                case ProductGroupIDConstants.InstaLink3:
                    if (reason.InstalinkUSB.HasValue && reason.InstalinkUSB.Value == true)
                    {
                        isAddReason = true;
                    }
                    break;
                default:
                    break;
            }

            if (isAddReason)
            {
                ListItem listItemReason = new ListItem(reason.Description, reason.ReasonID.ToString());
                this.ddlReturnReason.Items.Add(listItemReason);
            }
            
        }
        if (Request.QueryString["Reason"] != null)
        {
            string defaultReason = Request.QueryString["Reason"];

            ListItem defaultReasonItem = this.ddlReturnReason.Items.FindByText(defaultReason);

            if (defaultReasonItem != null)
            {
                defaultReasonItem.Selected = true;
                this.ddlReturnReason.SelectedValue = defaultReasonItem.Value;
            }
        }
    }
    private void LoadOtherLocations()
    {
        // Display All locations of an account

        if ((bool)ViewState["IsGDSAccount"])
        {
            var otherLocations = (from a in udc.Addresses
                                  where a.GDSAccount == this.account.GDSAccount && a.Company == "001" && a.AddressFormatCode != null
                                  orderby a.UNIXAddressPtr
                                  select new
                                  {
                                      LocationID = a.UNIXRecord,
                                      LocationName = a.UNIXAddressPtr,
                                      Contact = a.ContactName ?? "",
                                      Company = a.CompanyName ?? "",
                                      Address1 = (a.LongAddress1 ?? a.Address1) ?? "",
                                      Address2 = (a.LongAddress2 ?? a.Address2) ?? "",
                                      City = ((a.AddressFormatCode.HasValue && a.AddressFormatCode.Value == 'O') ? (a.LongAddress3 ?? a.Address3) : (a.LongCity ?? a.City)) ?? "",
                                      State = (a.UNIXStateCode ?? a.CountryCode) ?? "",
                                      ZipCode = (a.LongPostalCode ?? a.PostalCode) ?? "",
                                      Country = a.CountryCode ?? "US",
                                      StateID = 0,
                                      CountryID = 0,
                                      a.Telephone
                                  }).ToArray();

            gv_LocationList.DataSource = otherLocations;
            gv_LocationList.DataBind();
        }
        else
        {
            var otherLocations = (from locs in idc.Locations
                                  join s in idc.States on locs.ShippingStateID equals s.StateID
                                  join c in idc.Countries on locs.ShippingCountryID equals c.CountryID
                                  where locs.AccountID == this.account.AccountID
                                  orderby locs.LocationName
                                  select new
                                  {
                                      locs.LocationID,
                                      locs.LocationName,
                                      Contact = locs.ShippingFirstName + " " + locs.ShippingLastName,
                                      Company = locs.ShippingCompany,
                                      Address1 = locs.ShippingAddress1,
                                      Address2 = locs.ShippingAddress2,
                                      City = locs.ShippingCity,
                                      State = s.StateAbbrev,
                                      ZipCode = locs.ShippingPostalCode,
                                      Country = c.PayPalCode,
                                      StateID = locs.ShippingStateID,
                                      CountryID = locs.ShippingCountryID,
                                      Telephone = locs.ShippingTelephone
                                  }).ToArray();

            gv_LocationList.DataSource = otherLocations;
            gv_LocationList.DataBind();
        }

    }
    public void GetRMAInformationBySerialNo(string pSerialNumber)
    {
        // Clear the error.
        //this.lblError.Text = string.Empty;
        this.InvisibleErrors();
        this.InvisibleSuccess();

        // turn off email <div>         
        this.div_rmaEmailHistory.Visible = false;

        // Reset displaying of user information
        SetDisplaySingleFlowDeviceInfo(isSingleFlow);

        // Set shipping option
        SelectShippingRadioButton(this.rbtnMainAddress);

        this.rbtnMainAddress.Enabled = true;
        this.rbtnUserAddress.Enabled = true;
        this.rbtnLocationAddress.Enabled = true;
        this.rbtnOtherLocation.Enabled = true;
        SetOtherLocationVisibility(false);

        // clear the Comment/Note textbox
        this.txtNotes.Text = "";

        // clear the special instruction textbox
        this.txtSpecialInstruction.Text = "";

        // clear status label
        this.lalDeviceStatus.ForeColor = System.Drawing.Color.Black;
        this.lalDeviceStatus.Text = "";

        // turn on the "Initiate Request" button, it is closed when RMA# found on Serial#
        this.btnContinue.Visible = true;
        this.ddlReturnReason.Visible = true;
        this.lblReturnType.Visible = true;
        this.txtNotes.Enabled = true;
        this.WearDateRow.Visible = (bool)ViewState["IsGDSAccount"];
        this.ShippingMethodRow.Visible = (bool)ViewState["IsGDSAccount"];
        this.ShippingCarrierRow.Visible = (bool)ViewState["IsGDSAccount"];
        this.lblAccountNo.Visible = !(bool)ViewState["IsGDSAccount"];
        this.lblGDSAccount.Visible = (bool)ViewState["IsGDSAccount"];

        try
        {
            Account account = null;
            AccountDevice accountDevice = null;
            DeviceInventory deviceInventory = null;

            AccountProduct accountProduct = null;
            ProductInventory productInventory = null;

            var locationID = 0;

            var isInDeviceInventory = false;
            // Find account information by Serial No 
            var accountDeviceInfo = (from D1 in idc.DeviceInventories
                               join A1 in idc.AccountDevices on D1.DeviceID equals A1.DeviceID
                               join Acc in idc.Accounts on A1.AccountID equals Acc.AccountID
                               where D1.SerialNo == pSerialNumber
                                    && A1.CurrentDeviceAssign == true
                               select new { D1, A1, Acc }).FirstOrDefault();

            if (accountDeviceInfo != null && accountDeviceInfo.A1 != null)
            {
                account = accountDeviceInfo.Acc;
                accountDevice = accountDeviceInfo.A1;
                deviceInventory = accountDeviceInfo.D1;
                isInDeviceInventory = true;

                if (accountDevice.LocationID.HasValue)
                {
                    locationID = accountDevice.LocationID.Value;
                }
            }
            else
            {
                var accountProductInfo = (from D1 in idc.ProductInventories
                               join A1 in idc.AccountProducts on D1.ProductInventoryID equals A1.ProductInventoryID
                               join Acc in idc.Accounts on A1.AccountID equals Acc.AccountID
                               where D1.SerialNo == pSerialNumber
                                    && A1.CurrentDeviceAssign == true
                               select new { D1, A1, Acc }).FirstOrDefault();

                if (accountProductInfo != null && accountProductInfo.A1 != null)
                {
                    account = accountProductInfo.Acc;
                    productInventory = accountProductInfo.D1;
                    accountProduct = accountProductInfo.A1;
                    isInDeviceInventory = false;

                    if (accountProductInfo.A1.LocationID.HasValue)
                    {
                        locationID = accountProductInfo.A1.LocationID.Value;
                    }
                }
            }

            // If can not find the account associated with serial no because of CurrentDeviceAssign == false
            // Should pop-up the message ask IT for help since this device was deactivate it but Tech-ops like to treat it
            // as active and make a recall on this deactivate device....
            // ....some code over here otherwise it goes through Catch exception


            var ActiveUserDeviceInfo = (from D in idc.DeviceInventories
                                        join A1 in idc.AccountDevices on D.DeviceID equals A1.DeviceID
                                        join UD in idc.UserDevices on D.DeviceID equals UD.DeviceID
                                        join U in idc.Users on UD.UserID equals U.UserID
                                        join L in idc.Locations on U.LocationID equals L.LocationID
                                        join A in idc.Accounts on A1.AccountID equals A.AccountID
                                        join B in idc.BodyRegions on UD.BodyRegionID equals B.BodyRegionID
                                        where D.SerialNo == pSerialNumber
                                          && A1.CurrentDeviceAssign == true
                                          && UD.Active == true
                                        select new { U, B }).FirstOrDefault();

            var RecentUserDeviceLocationInfo = (from D in idc.DeviceInventories
                                                join A1 in idc.AccountDevices on D.DeviceID equals A1.DeviceID
                                                join UD in idc.UserDevices on D.DeviceID equals UD.DeviceID
                                                join U in idc.Users on UD.UserID equals U.UserID
                                                join L in idc.Locations on U.LocationID equals L.LocationID
                                                join A in idc.Accounts on A1.AccountID equals A.AccountID
                                                where D.SerialNo == pSerialNumber
                                                  && A1.CurrentDeviceAssign == true
                                                orderby UD.AssignmentDate descending
                                                select new { A, U, L }).FirstOrDefault();

            var accountDeviceLocationInfo = (from D in idc.DeviceInventories
                                             join A1 in idc.AccountDevices on D.DeviceID equals A1.DeviceID
                                             join L in idc.Locations on A1.LocationID equals L.LocationID
                                             where D.SerialNo == pSerialNumber
                                                 && A1.CurrentDeviceAssign == true
                                             orderby A1.AssignmentDate descending
                                             select new { A1, L }).FirstOrDefault();

            var accountProductLocationInfo = (from D in idc.ProductInventories
                                             join A1 in idc.AccountProducts on D.ProductInventoryID equals A1.ProductInventoryID
                                              join L in idc.Locations on A1.LocationID equals L.LocationID
                                             where D.SerialNo == pSerialNumber
                                                 && A1.CurrentDeviceAssign == true
                                             orderby A1.AssignmentDate descending
                                             select new { A1, L }).FirstOrDefault();

            if (account != null)
            {
                // Display Brand Source Logo (Quantum Products, Mirion, or ICCare)
                if (account.BrandSourceID.HasValue)
                {
                    switch (account.BrandSource.BrandName)
                    {
                        case "Quantum":
                            this.imgBrandLogo.ImageUrl = "~/images/logos/quantum.png";
                            break;
                        case "Mirion":
                            this.imgBrandLogo.ImageUrl = "~/images/logos/mirion.png";
                            break;
                        case "IC Care":
                            this.imgBrandLogo.ImageUrl = "~/images/logos/iccare.png";
                            break;
                        default:
                            this.imgBrandLogo.ImageUrl = "~/images/logos/mirion.png";
                            break;
                    }
                }
                else
                {
                    this.imgBrandLogo.ImageUrl = "~/images/logos/mirion.png";
                }

                VisibleRecallBody(true);

                this.lblAccountNo.Text = account.AccountID.ToString();
                this.lblGDSAccount.Text = account.GDSAccount;
                this.lblAccountName.Text = account.AccountName;
                this.lblReturnType.Text = "RMA/RECALL";
                this.lblSerialNo.Text = pSerialNumber;
                this.lblCreatedBy.Text = this.UserName;

                // ---------------- Load product group -------------------------- //
                int productGroupID = 0;

                // search serial number in productinventory
                var instalinkProduct = (from PI in idc.ProductInventories
                                        join P in idc.Products
                                             on PI.ProductID equals P.ProductID
                                        where PI.SerialNo == pSerialNumber && P.ProductName.Contains("INSTALINK")
                                        select new { ProductInventory = PI, ProductGroupID = P.ProductGroupID}).FirstOrDefault();

                var isInstalink = instalinkProduct != null; //pSerialNumber.Substring(0, 1) == "L";
                if (isInstalink) //Instalink or InstalinkUSB
                {
                    //productGroupID = (from PI in idc.ProductInventories
                    //                  join P in idc.Products
                    //                       on PI.ProductID equals P.ProductID
                    //                  where PI.SerialNo == pSerialNumber
                    //                  select P.ProductGroupID).FirstOrDefault();

                    productGroupID = instalinkProduct.ProductGroupID;

                    CaseCoverRow.Visible = false;
                    //WearDateRow.Visible = false;
                }
                else
                {
                    productGroupID = (from D in idc.DeviceInventories
                                      join P in idc.Products
                                           on D.ProductID equals P.ProductID
                                      where D.SerialNo == pSerialNumber
                                      select P.ProductGroupID).FirstOrDefault();
                }

                LoadProduct();
                if (productGroupID == 1) productGroupID = 9;
                this.ddlProduct.SelectedValue = productGroupID.ToString();

                // --------------------------------------- Load location ---------------------------------------//
                LoadLocation(account);
                string deviceLocation = "";

                if (RecentUserDeviceLocationInfo != null)
                {
                    if ((bool)ViewState["IsGDSAccount"])
                    {
                        deviceLocation = RecentUserDeviceLocationInfo.L.LocationCode;
                        LoadWearDates(RecentUserDeviceLocationInfo.A.GDSAccount, RecentUserDeviceLocationInfo.L.GDSLocation);
                    }
                    else
                    {
                        deviceLocation = RecentUserDeviceLocationInfo.L.LocationID.ToString();
                    }
                }
                else
                {
                    if (accountDeviceLocationInfo != null)
                    {
                        if ((bool)ViewState["IsGDSAccount"])
                        {
                            deviceLocation = accountDeviceLocationInfo.L.LocationCode;
                            LoadWearDates(accountDeviceLocationInfo.A1.Account.GDSAccount, accountDeviceLocationInfo.L.GDSLocation);
                        }
                        else
                        {
                            deviceLocation = accountDeviceLocationInfo.L.LocationID.ToString();
                        }
                    }
                    else //instalink
                    {
                        if ((bool)ViewState["IsGDSAccount"])
                        {
                            deviceLocation = accountProductLocationInfo.L.LocationCode;
                            LoadWearDates(accountProductLocationInfo.A1.Account.GDSAccount, accountProductLocationInfo.L.GDSLocation);
                        }
                        else
                        {
                            deviceLocation = accountProductLocationInfo.L.LocationID.ToString();
                        }
                    }
                }

                if (isSingleFlow)
                {
                    this.ddlLocation.SelectedIndex = this.ddlLocation.Items.IndexOf(this.ddlLocation.Items.FindByValue(deviceLocation.Trim()));
                }
                else
                {
                    this.ddlLocation.SelectedIndex = 0;
                    Load_gv_DeviceList(this.txtDeviceSearch.Text);
                }

                // ---------------- Load DDL return reason -----------------------------//
                LoadReturnReasons();

                // -------------------------------- Load other shipping location ---------------------------------//
                LoadOtherLocations();

                // Load ProRate Period Radio Button for non IC Care Instadose account 
                //if (!(bool)ViewState["IsGDSAccount"] && account.BrandSourceID != 3)
                if (!(bool)ViewState["IsGDSAccount"])
                {
                    ProRatePeriod.Visible = true;
                    if (int.TryParse(ddlLocation.SelectedValue, out int tmpLocID))
                        LoadProratePeriodRadioButton(this.account, tmpLocID);
                    else
                        LoadProratePeriodRadioButton(this.account, null);
                }
                else
                {
                    ProRatePeriod.Visible = false;
                }

                // -------------------------------- Load case cover of a recall device ---------------------------//
                this.ddlCaseCover.Items.Clear();

                int productGroupId;
                int.TryParse(ddlProduct.SelectedValue, out productGroupId);

                string caseCoverName = "";
                switch (productGroupId)
                {
                    case ProductGroupIDConstants.Instadose1:
                    case ProductGroupIDConstants.Instadose1IMI:
                        caseCoverName = "CASE CVR";
                        break;
                    case ProductGroupIDConstants.InstadosePlus:
                        caseCoverName = "COLOR CAP";
                        break;
                    case ProductGroupIDConstants.Instadose2:
                        break;
                    case ProductGroupIDConstants.Instadose2New:
                        caseCoverName = "BUMP";
                        break;
                    case ProductGroupIDConstants.InstaLink:
                        break;
                    case ProductGroupIDConstants.InstaLinkUSB:
                        break;
                    default:
                        break;
                }

                List<Product> caseCovers = new List<Product>();

                if (productGroupId == ProductGroupIDConstants.Instadose1) // load case cover of IMI-ProductGroupID = 9
                {
                    caseCovers = (from prods in idc.Products
                                  where prods.ProductGroupID == 9 && !(prods.Color == "N/A" || prods.Color == "No Color")
                                  && prods.Active == true
                                  select prods).ToList();
                }
                else if (productGroupId == ProductGroupIDConstants.Instadose2)
                {
                    // only White color
                    caseCovers = (from prods in idc.Products
                                    where prods.ProductID == 14 && !(prods.Color == "N/A" || prods.Color == "No Color")
                                    && prods.Active == true
                                  select prods).ToList();
                }

                else
                {
                    caseCovers = (from prods in idc.Products
                                  where prods.ProductGroupID == productGroupId && !(prods.Color == "N/A" || prods.Color == "No Color")
                                  && prods.Active == true
                                  select prods).ToList();
                }

                // Start loading Case Cover by product group
                ListItem firstCaseCoverItem = new ListItem("-- Select Case Cover --", "0");
                this.ddlCaseCover.Items.Add(firstCaseCoverItem);
                foreach (Product prod in caseCovers)
                {
                    ListItem caseCoverItem = new ListItem(prod.Color, prod.ProductID.ToString());
                    this.ddlCaseCover.Items.Add(caseCoverItem);
                }

                this.ddlCaseCover.SelectedValue = SeekCaseCoverProductID(pSerialNumber, productGroupId).ToString();
                this.HidCaseCover.Value = this.ddlCaseCover.SelectedValue;

                // ------------------------- Display current Device Assigned User  --------------------------------//              
                lblAssignedUser.Text = (ActiveUserDeviceInfo != null) ? "Username: <b>" + ActiveUserDeviceInfo.U.UserName.ToString() + "</b>" +
                    (ActiveUserDeviceInfo.U.GDSWearerID.HasValue ? " <br> WearerID: <b>" + ActiveUserDeviceInfo.U.GDSWearerID.Value.ToString() + "</b> " : "") +
                    " <br> Name: <b>" + ActiveUserDeviceInfo.U.FirstName.ToString() + " " + ActiveUserDeviceInfo.U.LastName.ToString() + "</b> " +
                    " <br> Body Region: <b>" + ActiveUserDeviceInfo.B.BodyRegionName + "</b> "
                    : "Not Assigned";

                // ------------------------- Display ship w/o user checkbox ------------------------------//               
                this.AssingnUserRow.Visible = (productGroupId == ProductGroupIDConstants.Instadose2 || productGroupId == ProductGroupIDConstants.InstadosePlus || productGroupId == ProductGroupIDConstants.Instadose2New) ? true : false;
                this.chkBoxWOAssingnUser.Checked = (ActiveUserDeviceInfo != null) ? false : true;
                this.chkBoxWOAssingnUser.Enabled = (ActiveUserDeviceInfo != null) ? true : false;

                // ------------------------- Display Device Service period ---------------------------------//
                this.lalServiceDate.Text = "Not Available";

                if (isInDeviceInventory)
                {
                    var devServiceDate = (from A in idc.AccountDevices
                                          join AD in idc.AccountDeviceDates
                                          on new { account.AccountID, accountDevice.DeviceID }
                                            equals new { AD.AccountID, AD.DeviceID }
                                          select new { AD }).FirstOrDefault();

                    if (devServiceDate != null)
                    {
                        this.lalServiceDate.Text = devServiceDate.AD.ServiceStartDate.ToShortDateString()
                                + " to " + devServiceDate.AD.ServiceEndDate.ToShortDateString();
                    }
                    else
                    {
                        this.lalServiceDate.Text = (accountDevice.ServiceStartDate.HasValue ? accountDevice.ServiceStartDate.Value.ToShortDateString() : "")
                                + " to " + (account.ContractEndDate.HasValue ? account.ContractEndDate.Value.ToShortDateString() : "");
                    }
                }
                else
                {
                    var productServiceDate = (from A in idc.AccountProducts
                                          join AD in idc.AccountProductDates
                                          on new { account.AccountID, accountProduct.AccountProductID }
                                            equals new { AD.AccountID, AD.AccountProductID }
                                          select new { AD }).FirstOrDefault();

                    if (productServiceDate != null)
                    {
                        this.lalServiceDate.Text = productServiceDate.AD.ServiceStartDate.ToShortDateString()
                                + " to " + productServiceDate.AD.ServiceEndDate.ToShortDateString();
                    }
                    else
                    {
                        this.lalServiceDate.Text = (accountProduct.ServiceStartDate.HasValue ? accountProduct.ServiceStartDate.Value.ToShortDateString() : "")
                                + " to " + (account.ContractEndDate.HasValue ? account.ContractEndDate.Value.ToShortDateString() : "");
                    }
                }

                // -------------------- Display all return/recall history --------------------------//
                var DeviceStatus = (from rDev in adc.rma_ReturnDevices
                                    join rStat in adc.rma_ref_ReturnDeviceStatus
                                        on rDev.Status equals rStat.ReturnDeviceStatusID
                                    join rRet in adc.rma_Returns
                                        on rDev.ReturnID equals rRet.ReturnID
                                    join rDept in adc.rma_ref_Departments
                                        on rDev.DepartmentID equals rDept.DepartmentID
                                    join rType in adc.rma_ref_ReturnTypes
                                        on rRet.ReturnTypeID equals rType.ReturnTypeID
                                    where rDev.SerialNo == pSerialNumber
                                    //&& rDev.Active == true
                                    //&& rRet.Active ==true
                                    select rDev).ToList();

                if (DeviceStatus.Count != 0)
                {
                    int dbReturnTypeDepartment = 0;
                    int dbDeviceStatus = 1;
                    int dbRequestID = 0;
                    string dbNotes = "";
                    string dbRequestCreatedDate = "";
                    string dbDeviceStatusStr = "";
                    int dbDeviceDepartment = 0;

                    int dbDeviceID = 0;

                    // display device Recall detail (allow display mutiple recalls)
                    foreach (var v in DeviceStatus)
                    {
                        dbDeviceStatusStr +=
                           "In Request#: " + v.ReturnID.ToString() +
                           "<br>Created Date: " + GetPDTtime(v.rma_Return.CreatedDate.ToString()) +
                           "<br>Created By: " + v.rma_Return.CreatedBy.ToString() +
                           "<br>At Dept: " + v.rma_ref_Department.Name.ToString() +
                           "<br>Comment: " + v.rma_ref_ReturnDeviceStatus.Status.ToString() +
                           "<br>";

                        dbDeviceStatus = v.Status;
                        dbDeviceDepartment = v.DepartmentID;
                        dbRequestID = v.ReturnID;
                        dbRequestCreatedDate = GetPDTtime(v.rma_Return.CreatedDate.ToString());
                        dbNotes = v.Notes == null ? "" : v.Notes.ToString();
                        dbDeviceID = v.ReturnDevicesID;

                        dbReturnTypeDepartment = v.rma_Return.rma_ref_ReturnType.DepartmentID;
                    }
                    this.lalDeviceStatus.Text = dbDeviceStatusStr.ToString();
                }

                // ------------------------ Display User, Location, Main shippping addresses label---------------------------//
                lblAddressUser.Text = "Not Available";
                lblAddressLocation.Text = "Not Available";
                lblAddressMain.Text = "Not Available";

                if (ActiveUserDeviceInfo != null)
                {
                    // display User's Shippping address if any
                    string userAddress = DisplayUserShippingAddress(ActiveUserDeviceInfo.U, account, deviceLocation);
                    string userLocationAddress = DisplayLocationShippingAddress(ActiveUserDeviceInfo.U, account, deviceLocation);

                    if (String.IsNullOrEmpty(userAddress))
                        lblAddressUser.Text = "Not Available";
                    else
                        lblAddressUser.Text = userAddress;

                    // display Location's Shippping address if any
                    if (String.IsNullOrEmpty(userLocationAddress))
                        lblAddressLocation.Text = "Not Available";
                    else
                        lblAddressLocation.Text = userLocationAddress;
                }
                else if (RecentUserDeviceLocationInfo != null)
                {
                    string userLocationAddress = DisplayLocationShippingAddress(RecentUserDeviceLocationInfo.U, account, deviceLocation);

                    // display Location's Shippping address if any
                    if (String.IsNullOrEmpty(userLocationAddress))
                        lblAddressLocation.Text = "Not Available";
                    else
                        lblAddressLocation.Text = userLocationAddress;
                }
                else if (accountDeviceLocationInfo != null)
                {
                    string userLocationAddress = DisplayOtherLocationShippingAddress(accountDeviceLocationInfo.L.LocationID);

                    // display Location's Shippping address if any
                    if (String.IsNullOrEmpty(userLocationAddress))
                        lblAddressLocation.Text = "Not Available";
                    else
                        lblAddressLocation.Text = userLocationAddress;
                }
                else if (locationID > 0)
                {
                    string locationAddress = DisplayOtherLocationShippingAddress(locationID);

                    // display Location's Shippping address if any
                    if (String.IsNullOrEmpty(locationAddress))
                        lblAddressLocation.Text = "Not Available";
                    else
                        lblAddressLocation.Text = locationAddress;
                }

                if (account != null  && (accountDevice != null || accountProduct != null))
                {
                    // display Account's Main Shipping Address 
                    string accountAddress = DisplayMainShippingAddress(account.AccountID);

                    if (String.IsNullOrEmpty(accountAddress))
                        lblAddressMain.Text = "Not Available";
                    else
                        lblAddressMain.Text = accountAddress;
                }

                // -------------------------------- Display Ship To location for IC Care accounts or GDSAccounts --------------------------------------//
                if (account.BrandSource.BrandName == "IC Care" || (bool)ViewState["IsGDSAccount"])
                {
                    if (ActiveUserDeviceInfo != null || RecentUserDeviceLocationInfo != null || accountDeviceLocationInfo != null || accountProductLocationInfo != null || locationID > 0)
                    {
                        SelectShippingRadioButton(this.rbtnLocationAddress);

                        this.rbtnMainAddress.Enabled = false;
                        this.rbtnUserAddress.Enabled = false;
                    }
                    else
                    {
                        SelectShippingRadioButton(this.rbtnOtherLocation);
                        SetOtherLocationVisibility(true);

                        this.rbtnMainAddress.Enabled = false;
                        this.rbtnLocationAddress.Enabled = false;
                        this.rbtnUserAddress.Enabled = false;
                    }

                    if (isInstalink)
                    {
                        rbtnMainAddress.Enabled = account != null;
                    }

                    if ((bool)ViewState["IsGDSAccount"] && !isInstalink) this.rbtnUserAddress.Enabled = true;
                }

                // ------------------------------ count device received by Mfg but not commited to Finance ----------------------------------//
                // 3/28/2014. Check if RD.Active == true
                // device already in recall and awaiting for return
                var DevRtnReqList = (from RD in adc.rma_ReturnDevices
                                     join RR in adc.rma_Returns on RD.ReturnID equals RR.ReturnID
                                     where RD.SerialNo == pSerialNumber
                                     && RD.Received == false
                                     && RD.Active == true
                                     select new { RR, RD }).ToList();

                // device receive by Mfg, wait for Tech inspection
                var RecDevInvenOpenList = (from DevInv in adc.rma_ReturnInventories
                                           where DevInv.SerialNo == pSerialNumber
                                           && DevInv.Completed == false
                                           && DevInv.CommittedToFinance == null
                                           select DevInv).ToList();

                if (DevRtnReqList.Count == 0 && RecDevInvenOpenList.Count == 0)
                {
                    if (DeviceStatus.Count == 0)
                        this.lalDeviceStatus.Text += "Not in RMA";

                    // ------------------------- Display default Notes  --------------------------------//
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(string.Format("Badge Serial #: {0}", this.lblSerialNo.Text));
                    if (ActiveUserDeviceInfo == null)
                    {
                        sb.Append(string.Format("Recalled By: {0}", this.lblCreatedBy.Text));
                    }
                    else
                    {
                        sb.AppendLine(string.Format("Recalled By: {0}", this.lblCreatedBy.Text));
                        if (ActiveUserDeviceInfo.U.GDSWearerID.HasValue)
                        {
                            sb.AppendLine(string.Format("Wearer Name : {0} {1}", ActiveUserDeviceInfo.U.FirstName, ActiveUserDeviceInfo.U.LastName));
                            sb.Append(string.Format("Wearer #: {0}", ActiveUserDeviceInfo.U.GDSWearerID.Value.ToString()));
                        }
                        else
                        {
                            sb.Append(string.Format("Wearer Name : {0} {1}", ActiveUserDeviceInfo.U.FirstName, ActiveUserDeviceInfo.U.LastName));
                        }
                    }
                    txtNotes.Text = sb.ToString();
                }
                else if (DevRtnReqList.Count != 0) // device in other recall/return request
                {
                    this.ddlReturnReason.Visible = false;
                    this.lblReturnType.Visible = false;
                    this.btnContinue.Visible = false;
                    this.lalDeviceStatus.ForeColor = System.Drawing.Color.Red;
                    // display email history
                    this.div_rmaEmailHistory.Visible = true;
                    ViewEmailHistory(DevRtnReqList[0].RR.ReturnID, DevRtnReqList[0].RD.ReturnDevicesID);
                    txtNotes.Text = DevRtnReqList[0].RR.Notes;

                    //lblError.Text += "</br>Device has already been recalled or in a return request, and is waiting to be returned.";
                    this.VisibleErrors("Device has already been recalled or in a return request, and is waiting to be returned.");
                }
                else if (RecDevInvenOpenList.Count != 0)  // item receive by Mfg and waiting to be reviewed
                {
                    this.ddlReturnReason.Visible = false;
                    this.lblReturnType.Visible = false;
                    this.btnContinue.Visible = false;
                    this.lalDeviceStatus.ForeColor = System.Drawing.Color.Red;
                    // display email history
                    this.div_rmaEmailHistory.Visible = true;
                    ViewEmailHistory(RecDevInvenOpenList[0].ReturnID, RecDevInvenOpenList[0].ReturnDeviceID);
                    txtNotes.Text = RecDevInvenOpenList[0].TechOpsNotes;

                    //lblError.Text += "</br>Device has already been received by Mfg and is waiting to be reviewed by TechOps.";
                    this.VisibleErrors("Device has already been received by Mfg and is waiting to be reviewed by TechOps.");
                }                
            }
            else
            {
                this.VisibleErrors("No device was found or device has never been assigned to any account.");
                VisibleRecallBody(false);
            }
        }

        catch (Exception ex)
        {
            //this.lblError.Text = "The Serial number is invalid. No device was found.";
            this.VisibleErrors("The Serial number is invalid. No device was found.");
            VisibleRecallBody(false);
        }
    }
    public void GetRMAInformationByAccountNo(string pAccountNumber, string pLocationCode)
    {
        // Clear the error.
        //this.lblError.Text = string.Empty;
        this.InvisibleErrors();
        this.InvisibleSuccess();

        // turn off email <div>         
        this.div_rmaEmailHistory.Visible = false;

        // Reset displaying of user information
        SetDisplaySingleFlowDeviceInfo(isSingleFlow);

        // Set shipping option
        SelectShippingRadioButton(this.rbtnLocationAddress);

        this.rbtnMainAddress.Enabled = true;
        this.rbtnUserAddress.Enabled = true;
        this.rbtnLocationAddress.Enabled = true;
        this.rbtnOtherLocation.Enabled = true;
        SetOtherLocationVisibility(false);

        // clear the Comment/Note textbox
        this.txtNotes.Text = "";

        // clear the special instruction textbox
        this.txtSpecialInstruction.Text = "";

        // clear status label
        this.lalDeviceStatus.ForeColor = System.Drawing.Color.Black;
        this.lalDeviceStatus.Text = "";

        // turn on the "Initiate Request" button, it is closed when RMA# found on Serial#
        this.btnContinue.Visible = true;
        this.ddlReturnReason.Visible = true;
        this.lblReturnType.Visible = true;
        this.txtNotes.Enabled = true;
        this.WearDateRow.Visible = (bool)ViewState["IsGDSAccount"];
        this.ShippingMethodRow.Visible = (bool)ViewState["IsGDSAccount"];
        this.ShippingCarrierRow.Visible = (bool)ViewState["IsGDSAccount"];
        this.lblAccountNo.Visible = !(bool)ViewState["IsGDSAccount"];
        this.lblGDSAccount.Visible = (bool)ViewState["IsGDSAccount"];

        try
        {
            // Display Brand Source Logo (Quantum Products, Mirion, or ICCare)
            if (this.account.BrandSourceID.HasValue)
            {
                switch (this.account.BrandSource.BrandName)
                {
                    case "Quantum":
                        this.imgBrandLogo.ImageUrl = "~/images/logos/quantum.png";
                        break;
                    case "Mirion":
                        this.imgBrandLogo.ImageUrl = "~/images/logos/mirion.png";
                        break;
                    case "IC Care":
                        this.imgBrandLogo.ImageUrl = "~/images/logos/iccare.png";
                        break;
                    default:
                        this.imgBrandLogo.ImageUrl = "~/images/logos/mirion.png";
                        break;
                }
            }
            else
            {
                this.imgBrandLogo.ImageUrl = "~/images/logos/mirion.png";
            }

            VisibleRecallBody(true);

            this.lblAccountNo.Text = this.account.AccountID.ToString();
            this.lblGDSAccount.Text = this.account.GDSAccount;
            this.lblAccountName.Text = this.account.AccountName;
            this.lblReturnType.Text = "RMA/RECALL";
            this.lblSerialNo.Text = "";
            this.lblCreatedBy.Text = this.UserName;

            // ---------------- Load product group -------------------------- //            
            LoadProduct();
            this.ddlProduct.SelectedIndex = 0;            
            // if CSWS makes a recall, the default product is Instadose Plus
            if ((bool)ViewState["IsGDSAccount"])
            {
                this.ddlProduct.SelectedValue = ProductGroupIDConstants.InstadosePlus.ToString(); // productGroupID = 10 for Instadose Plus;
            }
           
            // --------------------------------------- Load location ---------------------------------------//
            LoadLocation(this.account);
            this.ddlLocation.SelectedIndex = 0;
            if (pLocationCode != "")
            {
                this.ddlLocation.SelectedIndex = this.ddlLocation.Items.IndexOf(this.ddlLocation.Items.FindByValue(pLocationCode.Trim()));
            }

            // --------------------------------------- Load wearer date -----------------------------------//
            // if GDSAccount then ddlLocation.SelectedValue is GDSLocation, else ddlLocation.SelectedValue is LocationID //
            LoadWearDates(this.account.GDSAccount, this.ddlLocation.SelectedValue);

            // ---------------- Load DDL return reason -----------------------------//
            LoadReturnReasons();

            // -------------------------------- Load other shipping location ---------------------------------//
            LoadOtherLocations();

            // -------------------------------- Load case cover of a recall device ---------------------------//
            LoadCaseCoverSession();

            // ------------------------ Display User, Location, Main shippping addresses label---------------------------//
            lblAddressUser.Text = "";
            lblAddressLocation.Text = "Not Available";
            lblAddressMain.Text = "Not Available";

            string userLocationAddress = DisplayLocationShippingAddress(this.account, this.ddlLocation.SelectedValue);

            // display Location's Shippping address if any
            if (!String.IsNullOrEmpty(userLocationAddress))
                lblAddressLocation.Text = userLocationAddress;

            // display Account's Main Shipping Address 
            string accountAddress = DisplayMainShippingAddress(this.account.AccountID);
            if (!String.IsNullOrEmpty(accountAddress))
                lblAddressMain.Text = accountAddress;

            // -------------------------------- Display Ship To location for IC Care accounts or GDSAccounts --------------------------------------//
            if (this.account.BrandSource.BrandName == "IC Care" || (bool)ViewState["IsGDSAccount"])
            {
                SelectShippingRadioButton(this.rbtnLocationAddress);

                this.rbtnMainAddress.Enabled = false;
                this.rbtnUserAddress.Enabled = false;

                if ((bool)ViewState["IsGDSAccount"]) this.rbtnUserAddress.Enabled = true;
            }

            Load_gv_DeviceList(this.txtDeviceSearch.Text);

            //// ------------------------------ count device received by Mfg but not commited to Finance ----------------------------------//
            //// 3/28/2014. Check if RD.Active == true
            //// device already in recall and awaiting for return
            //var DevRtnReqList = (from RD in adc.rma_ReturnDevices
            //                     join RR in adc.rma_Returns on RD.ReturnID equals RR.ReturnID
            //                     where RD.SerialNo == pSerialNumber
            //                     && RD.Received == false
            //                     && RD.Active == true
            //                     select new { RR, RD }).ToList();

            //// device receive by Mfg, wait for Tech inspection
            //var RecDevInvenOpenList = (from DevInv in adc.rma_ReturnInventories
            //                           where DevInv.SerialNo == pSerialNumber
            //                           && DevInv.Completed == false
            //                           && DevInv.CommittedToFinance == null
            //                           select DevInv).ToList();

            //if (DevRtnReqList.Count == 0 && RecDevInvenOpenList.Count == 0)
            //{
            //    if (DeviceStatus.Count == 0)
            //        this.lalDeviceStatus.Text += "Not in RMA";
            //}
            //else if (DevRtnReqList.Count != 0) // device in other recall/return request
            //{
            //    this.ddlReturnReason.Visible = false;
            //    this.lblReturnType.Visible = false;
            //    this.btnContinue.Visible = false;
            //    this.lalDeviceStatus.ForeColor = System.Drawing.Color.Red;
            //    // display email history
            //    this.div_rmaEmailHistory.Visible = true;
            //    ViewEmailHistory(DevRtnReqList[0].RR.ReturnID, DevRtnReqList[0].RD.ReturnDevicesID);

            //    //lblError.Text += "</br>Device has already been recalled or in a return request, and is waiting to be returned.";
            //    this.VisibleErrors("Device has already been recalled or in a return request, and is waiting to be returned.");
            //}
            //else if (RecDevInvenOpenList.Count != 0)  // item receive by Mfg and waiting to be reviewed
            //{
            //    this.ddlReturnReason.Visible = false;
            //    this.lblReturnType.Visible = false;
            //    this.btnContinue.Visible = false;
            //    this.lalDeviceStatus.ForeColor = System.Drawing.Color.Red;
            //    // display email history
            //    this.div_rmaEmailHistory.Visible = true;
            //    ViewEmailHistory(RecDevInvenOpenList[0].ReturnID, RecDevInvenOpenList[0].ReturnDeviceID);

            //    //lblError.Text += "</br>Device has already been received by Mfg and is waiting to be reviewed by TechOps.";
            //    this.VisibleErrors("Device has already been received by Mfg and is waiting to be reviewed by TechOps.");
            //}
            //

            // ------------------------- Display default Notes  --------------------------------//      
            txtNotes.Text = string.Format("Recalled By: {0}", this.lblCreatedBy.Text);
        }

        catch (Exception ex)
        {
            //this.lblError.Text = "The Serial number is invalid. No device was found.";
            this.VisibleErrors("The Serial number is invalid. No device was found.");
            VisibleRecallBody(false);
        }
    }
    private void VisibleRecallBody(bool flag)
    {
        this.btnContinue.Enabled = flag;
        this.upnlAccountInfo.Visible = flag;
        //this.div_accountInfo.Visible = flag;
        this.div_InitiateRequestButtonRow.Visible = flag;        
    }
    private bool identifyGDSAccountBySerialNo(string pSerialNumber, ref string pErrorMsg)
    {
        // Find account information by Serial No 
        var account = GetAccountBySerialNo(pSerialNumber);

        if (account != null)
        {
            this.account = account;
            if (!string.IsNullOrEmpty(account.GDSAccount))
            {
                return true;
            }
        }
        else
        {            
            pErrorMsg = "No device was found or device has never been assigned to any account.";
        }
        return false;
    }

    private Account GetAccountBySerialNo(string pSerialNumber)
    {
        // Find account information by Serial No 
        var account = (from D1 in idc.DeviceInventories
                           join A1 in idc.AccountDevices on D1.DeviceID equals A1.DeviceID
                           join Acc in idc.Accounts on A1.AccountID equals Acc.AccountID
                           where D1.SerialNo == pSerialNumber
                                && A1.CurrentDeviceAssign == true
                           select Acc ).FirstOrDefault();

        if (account == null)
        {
            account = (from pi in idc.ProductInventories
                                      join ai in idc.AccountProducts on pi.ProductInventoryID equals ai.ProductInventoryID
                                      join acc in idc.Accounts on ai.AccountID equals acc.AccountID
                                      where pi.SerialNo == pSerialNumber
                                           && ai.CurrentDeviceAssign == true
                                      select acc).FirstOrDefault();
        }

        return account;
    }
    private bool identifyGDSAccountByAccountNo(string pAccountNumber, ref string pErrorMsg)
    {
        if (rdobtnByInstadoseAccount.Checked)
        {
            this.account = (from a in idc.Accounts where a.AccountID == int.Parse(pAccountNumber) select a).FirstOrDefault();
            if (this.account != null)
            {
                if (!String.IsNullOrEmpty(this.account.GDSAccount))
                {
                    return true;
                }
                return false;
            }
        }

        if (rdobtnByGDSAccount.Checked)
        {
            this.account = (from a in idc.Accounts where a.GDSAccount == pAccountNumber select a).FirstOrDefault();
            if (this.account != null) return true;
        }

        if (this.account == null)
        {
            pErrorMsg = "Account does not exist.";
        }

        return false;
    }
    protected void btnCancel_Click(object sender, EventArgs e)
    {
        VisibleRecallBody(false);

        this.txtSerialNoInput.Text = "";
        this.txtAccountInput.Text = "";

        if (int.Parse(ViewState["RecallByDepartmentID"].ToString()) == 3)
            Page.Response.Redirect("ReturnAddNewDeviceRMA.aspx");
        else
            Page.Response.Redirect("ReturnAddNewDeviceRMA.aspx?SerialNo=0");
    }
    private bool inputValidationPass()
    {
        //Ensure Reason != nothing
        if (this.ddlReturnReason.SelectedValue.ToString() == "0")
        {
            //this.lblError.Text = "You must select a return reason.";
            this.VisibleErrors("You must select a return reason.");
            return false;
        }

        // Ensure SpecialInstructions size after trimming is still less than or equal 200 characters.
        string strSpecialInstruction = "Recall Replacement, Recall# 12345, Ship Overnight / 2nd Day. " + this.txtSpecialInstruction.Text.Trim();
        if (TrimNewLineString(strSpecialInstruction).Length > 1024)
        {
            //this.lblError.Text = "The Special Instruction is too long. Please re-enter it.";
            this.VisibleErrors("The Special Instruction is too long. Please re-enter it.");
            return false;
        }

        //  Ensure shipping address != nothing 
        if (this.rbtnOtherLocation.Checked == false && rbtnLocationAddress.Checked == false
            && rbtnMainAddress.Checked == false && rbtnUserAddress.Checked == false)
        {
            //this.lblError.Text = "You must select a shipping address.";
            this.VisibleErrors("You must select a shipping address.");
            return false;
        }

        // Ensure other location selected for shipping
        if (rbtnOtherLocation.Checked == true)
        {
            if (this.txtLocationID.Text == "0")    // add new shipping location for gdsAccount recall
            {
                bool isUS_CA = false;
                Country selCountry = (from c in idc.Countries where c.CountryID == int.Parse(ddlAddrCountry.SelectedValue) select c).FirstOrDefault();
                if (selCountry.PayPalCode == "US" || selCountry.PayPalCode == "CA")
                {
                    isUS_CA = true;
                }

                if (String.IsNullOrEmpty(this.txtAddrContactName.Text.Trim()))
                {
                    this.VisibleErrors("Contact Name is required.");
                    return false;
                }
                if (this.ddlAddrCountry.SelectedValue.ToString() == "0")
                {
                    this.VisibleErrors("Country is required.");
                    return false;
                }

                if (String.IsNullOrEmpty(this.txtAddrAddress1.Text.Trim()))
                {
                    this.VisibleErrors("Address is required.");
                    return false;
                }

                if (isUS_CA)
                {
                    if (this.ddlAddrState.SelectedValue.ToString() == "0")
                    {
                        if (isUS_CA)
                        {
                            this.VisibleErrors("State is required.");
                            return false;
                        }
                    }

                    if (String.IsNullOrEmpty(this.txtAddrCity.Text.Trim()))
                    {
                        this.VisibleErrors("City is required.");
                        return false;
                    }
                    else
                    {
                        State selState = (from s in idc.States where s.StateID == int.Parse(ddlAddrState.SelectedValue) select s).FirstOrDefault();
                        if ((selState.StateAbbrev == "AA" || selState.StateAbbrev == "AE") && this.txtAddrAddress1.Text.Trim() != "" && this.txtAddrCity.Text.Trim().ToUpper() != "APO")
                        {
                            this.VisibleErrors("City must be APO.");
                            return false;
                        }

                        if (selState.StateAbbrev == "AP" && this.txtAddrAddress1.Text.Trim() != "" && this.txtAddrCity.Text.Trim().ToUpper() != "FPO")
                        {
                            this.VisibleErrors("City must be FPO.");
                            return false;
                        }
                    }

                    if (String.IsNullOrEmpty(this.txtAddrZipCode.Text.Trim()))
                    {
                        if (isUS_CA)
                        {
                            this.VisibleErrors("Zip Code is required.");
                            return false;
                        }
                    }
                    else
                    {
                        var validZip = HRegex.CheckZip(this.txtAddrZipCode.Text.Trim());
                        if (!validZip)
                        {
                            this.VisibleErrors("Please enter a valid Zip/Postal Code");
                            return false;
                        }
                    }
                }

                if (String.IsNullOrEmpty(this.txtAddrPhone.Text.Trim()))
                {
                    this.VisibleErrors("Telephone is required.");
                    return false;
                }
            }
        }

        // Ensure other location selected for shipping
        if (rbtnUserAddress.Checked == true)
        {
            if (lblAddressUser.Text.Trim() == "Not Available")
            {
                //this.lblError.Text = "You must select a location on the location list.";
                this.VisibleErrors("There is no address on this user shipping address.");
                return false;
            }
        }

        // Ensure other location selected for shipping
        if (rbtnLocationAddress.Checked == true)
        {
            if (lblAddressLocation.Text.Trim() == "Not Available")
            {
                //this.lblError.Text = "You must select a location on the location list.";
                this.VisibleErrors("There is no address on this location shipping address.");
                return false;
            }
        }

        // Ensure other location selected for shipping
        if (rbtnMainAddress.Checked == true)
        {
            if (lblAddressMain.Text.Trim() == "Not Available")
            {
                //this.lblError.Text = "You must select a location on the location list.";
                this.VisibleErrors("There is no address on this Main shipping address.");
                return false;
            }
        }

        if ((bool)ViewState["IsGDSAccount"])
        {
            string myErrorMessage = "";
            CSWSInstaOrderHelper instaOrderHelperObj = new CSWSInstaOrderHelper();

            DateTime.TryParse(ddlWearDate.SelectedValue, out DateTime wearDate);

            // Recall: InstaOrderType = 1
            // Lost Replacement: InstaOrderType = 2
            // New: InstaOrderType = 3
            // Add On: InstaOrderType = 4
            string existInstaOrderID = instaOrderHelperObj.IsOrderExist(account.GDSAccount, this.ddlLocation.SelectedItem.Value, wearDate, 1, ref myErrorMessage);
            if (myErrorMessage.Length > 0)
            {
                this.VisibleErrors(myErrorMessage + "</br>" + "Check IT.");
                return false;
            }
            if (!string.IsNullOrEmpty(existInstaOrderID))
            {
                myErrorMessage = "Could not create a recall order." + "</br>" + "A temp order exists for GDSAccount: " + account.GDSAccount + ", GDSLocation: " + this.ddlLocation.SelectedItem.Value + ", on WearDate: " + ddlWearDate.SelectedValue + ". Temp order: " + existInstaOrderID;
                this.VisibleErrors(myErrorMessage);
                return false;
            }
        }

        if (this.isSingleFlow)
        {
            //Ensure case cover color is not blank
            if (this.ddlCaseCover.Visible && this.ddlCaseCover.SelectedValue == "0")
            {
                this.VisibleErrors("Please verify with client then select a case cover color for this replacement.");
                return false;
            }
        }
        else
        {
            if (!AnySelectedDevice())
            {
                this.VisibleErrors("Please select a serial number to recall.");
                return false;
            }

            string errorMsg = "";
            string invalidColorMsg = "";
            string invalidRMAMsg = "";
            if (!ValidDeviceList(ref invalidColorMsg, ref invalidRMAMsg))
            {
                if (invalidColorMsg.Length > 0)
                {
                    errorMsg += "Please verify with client then select a case cover color for the following Serial No:</br>";
                    errorMsg += TruncateString(invalidColorMsg.Substring(0, invalidColorMsg.Length - 1), 150);
                    this.VisibleErrors(errorMsg);
                    return false;
                }

                if (invalidRMAMsg.Length > 0)
                {
                    errorMsg += "The following Serial No have already created RMA and waiting for being returned or TechOps's review:</br>";
                    errorMsg += TruncateString(invalidRMAMsg.Substring(0, invalidRMAMsg.Length - 1), 150);
                    this.VisibleErrors(errorMsg);
                    return false;
                }
            }
        }

        bool isProductRequiresCalendar = CheckIfProductRequiresCalendar();

        if (isProductRequiresCalendar)
        {
            bool hasCalendar = CheckLocationAndAccountCalendar();

            if (!hasCalendar)
            {
                this.VisibleErrors("There is no calendar setup for this location and account.");
                return false;
            }
        }

        return true;
    }

    private bool CheckLocationAndAccountCalendar()
    {
        // check location calendar schedule
        bool hasCalendar = false;
        Location location;
        var accountID = int.Parse(this.lblAccountNo.Text);
        var isGDSAccount = ((bool)ViewState["IsGDSAccount"]);
        if (isGDSAccount)
            location = idc.Locations.FirstOrDefault(x =>  x.AccountID == accountID && x.GDSLocation == ddlLocation.SelectedValue);
        else
            location = idc.Locations.FirstOrDefault(x => x.AccountID == accountID &&  x.LocationID == int.Parse(ddlLocation.SelectedValue));

        if (location != null && location.ScheduleID.HasValue)
        {
            hasCalendar = true;
        }
        else
        {
            var account = idc.Accounts.FirstOrDefault(acc => acc.AccountID == accountID);
            hasCalendar = account != null && account.ScheduleID.HasValue;
        }

        return hasCalendar;
    }

    private bool CheckIfProductRequiresCalendar()
    {
        int.TryParse(ddlProduct.SelectedValue, out int productGroupId);
        bool isProductRequiresCalendar = productGroupId == ProductGroupIDConstants.InstadosePlus
            || productGroupId == ProductGroupIDConstants.Instadose2
            || productGroupId == ProductGroupIDConstants.InstadoseVue
            || productGroupId == ProductGroupIDConstants.InstadoseVueBeta;
        return isProductRequiresCalendar;
    }

    private bool ValidDeviceList(ref string pInvalidColorMsg, ref string pInvalidRMAMsg)
    {
        foreach (GridViewRow row in this.gv_DeviceList.Rows)
        {
            CheckBox chkBoxSelect = (CheckBox)row.FindControl("chkbxSelectDeviceList");
            DropDownList ddlColor = (DropDownList)row.FindControl("ddlColor");
            HiddenField HidDeviceID = (HiddenField)row.FindControl("HidDeviceID");
            HiddenField HidAssignedWearerID = (HiddenField)row.FindControl("HidAssignedWearerID");
            string sn = row.Cells[3].Text.Replace("\n", "").ToString().Trim();

            if (chkBoxSelect.Checked)
            {
                if (ddlColor.Visible && ddlColor.SelectedValue == "0")
                {
                    pInvalidColorMsg += sn + ",";
                }

                string rmaReturnMsg = "";
                if (isRMAExist(sn, ref rmaReturnMsg))
                {
                    pInvalidRMAMsg += rmaReturnMsg + ";";
                }
            }
        }

        if (pInvalidColorMsg.Length > 0 || pInvalidRMAMsg.Length > 0)
        {
            return false;
        }

        return true;
    }
    private bool AnySelectedDevice()
    {
        foreach (GridViewRow row in this.gv_DeviceList.Rows)
        {
            CheckBox chkBoxSelect = (CheckBox)row.FindControl("chkbxSelectDeviceList");

            if (chkBoxSelect.Checked)
            {
                return true;
            }
        }

        return false;
    }
    private bool isRMAExist(string pSerialNo, ref string pReturnMsg)
    {
        // device already in rma and awaiting for return
        var recReturnDevice = (from RD in adc.rma_ReturnDevices
                               join RR in adc.rma_Returns on RD.ReturnID equals RR.ReturnID
                               where RD.SerialNo == pSerialNo
                               && RD.Received == false
                               && RD.Active == true
                               orderby RR.CreatedDate descending
                               select new { RR, RD }).FirstOrDefault();

        // device receive by Mfg, wait for Tech inspection
        var recReturnInventory = (from DevInv in adc.rma_ReturnInventories
                                  where DevInv.SerialNo == pSerialNo
                                  && DevInv.Completed == false
                                  && DevInv.CommittedToFinance == null
                                  orderby DevInv.CreatedDate descending
                                  select DevInv).FirstOrDefault();

        if (recReturnDevice != null) // device already in rma and awaiting for return
        {
            pReturnMsg = "(SN:" + pSerialNo + ",RMA:" + recReturnDevice.RR.ReturnID.ToString() + ")";
            return true;
        }

        if (recReturnInventory != null)  // device receive by Mfg, wait for Tech inspection
        {
            pReturnMsg = "(SN:" + pSerialNo + ",RMA:" + recReturnInventory.ReturnID.ToString() + ")";
            return true;
        }

        return false;
    }
    private string TruncateString(string pString, int pLength)
    {
        string breaklineString = "";
        while (pString.Length > pLength)
        {
            breaklineString += pString.Substring(0, pLength) + "</br>";
            pString = pString.Substring(pLength);
        }
        breaklineString += pString;
        return breaklineString;
    }
    /// <summary>
    /// Create Recall, submit order to LCDIS & MAS
    /// Last Mod:11/01/2011
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnContinue_Click(object sender, EventArgs e)
    {
        btnContinue.Enabled = false;

        // Clear the error.
        //this.lblError.Text = string.Empty;
        this.InvisibleErrors();
        this.InvisibleSuccess();

        try
        {
            // reload the grid with all devices by account, location, and product.
            // and then refresh the grid with the selected recall device info
            Load_gv_DeviceList("");

            account = (from acc in idc.Accounts where acc.AccountID == int.Parse(this.lblAccountNo.Text) select acc).FirstOrDefault();

            if (inputValidationPass())
            {
                string submitError = "";
                string myInitial = this.UserName.Substring(0, 3);
                string serialNo = lblSerialNo.Text.ToString();
                string caseCoverProductID = ddlCaseCover.SelectedValue;
                bool shipWOAssignedUser = this.chkBoxWOAssingnUser.Checked;

                // tdo, 4/10/2012. Get an user who is assigned to a recall device
                int assignedUserID = 0;
                int assignedUserLocationID = 0;
                var user = (from a in idc.DeviceInventories
                            join u in idc.UserDevices on a.DeviceID equals u.DeviceID
                            join b in idc.Users on u.UserID equals b.UserID
                            where a.SerialNo == serialNo
                              && u.Active == true
                            select b).FirstOrDefault();

                if (user != null)
                {
                    assignedUserID = int.Parse(user.UserID.ToString());
                    assignedUserLocationID = int.Parse(user.LocationID.ToString());
                }

                var returnReasonID = int.Parse(this.ddlReturnReason.SelectedValue);

                // Create new RMA request
                int NewReturnID = 0;

                if ((bool)ViewState["IsGDSAccount"])
                {
                    string gdsLocation = this.ddlLocation.SelectedItem.Value;
                    string myAddressPtr = "0";
                    bool isNewAddress = false;

                    GAddress myAddress = null;

                    LocationRequests lr = new LocationRequests();
                    var location = lr.GetLocation(account.GDSAccount, gdsLocation);

                    // ship to different location
                    if (this.rbtnOtherLocation.Checked == true)
                    {
                        myAddress = new GAddress();

                        if (int.Parse(this.txtLocationID.Text) > 0)
                        {
                            Mirion.DSD.GDS.API.Contexts.Address myGDSAddress = (from a in udc.Addresses where a.UNIXRecord == int.Parse(this.txtLocationID.Text) select a).FirstOrDefault();

                            myAddressPtr = myGDSAddress.UNIXAddressPtr;

                            myAddress.Account = account.GDSAccount;
                            myAddress.Location = gdsLocation;
                            myAddress.Wearer = (int?)null;

                            myAddress.AddressPtr = myGDSAddress.UNIXAddressPtr;
                            myAddress.AddrType = AddressType.None;
                            myAddress.NumSharedAddr = 0;
                            myAddress.ShareAddress = YesNo.Yes;
                            myAddress.ContactName = myGDSAddress.ContactName ?? "";
                            myAddress.AddrFmtCode = myGDSAddress.AddressFormatCode.HasValue ? myGDSAddress.AddressFormatCode.Value == 'O' ? AddrFmtCode.Other : myGDSAddress.AddressFormatCode.Value == 'C' ? AddrFmtCode.Canada : AddrFmtCode.Domestic : AddrFmtCode.Domestic;
                            myAddress.CompanyName = myGDSAddress.CompanyName ?? "";
                            myAddress.Attention = myGDSAddress.AttentionName;
                            myAddress.FaxNumber = myGDSAddress.Fax ?? "";
                            myAddress.Email = myGDSAddress.Email ?? "";
                            myAddress.ContactPhone = myGDSAddress.Telephone ?? "";
                            myAddress.Company = myGDSAddress.Company ?? "001";
                            myAddress.AutoCorrectAddress = myGDSAddress.AutoCorrectAddress.HasValue && myGDSAddress.AutoCorrectAddress.Value ? YesNo.No : YesNo.Yes;

                            myAddress.CountryCode = myGDSAddress.CountryCode ?? "US";
                            myAddress.CountryDesc = myGDSAddress.CountryName ?? "United States";
                            myAddress.Address1 = (myGDSAddress.LongAddress1 ?? myGDSAddress.Address1) ?? "";
                            myAddress.Address2 = myGDSAddress.LongAddress2 ?? myGDSAddress.Address2 ?? "";
                            myAddress.Address4 = myGDSAddress.LongAddress4 ?? myGDSAddress.Address4 ?? "";
                            myAddress.City = ((myGDSAddress.AddressFormatCode.HasValue && myGDSAddress.AddressFormatCode.Value == 'O') ? (myGDSAddress.LongAddress3 ?? myGDSAddress.Address3) : (myGDSAddress.LongCity ?? myGDSAddress.City)) ?? "";
                            myAddress.State = (myGDSAddress.UNIXStateCode ?? myGDSAddress.CountryCode) ?? "";
                            myAddress.ZipCode = (myGDSAddress.LongPostalCode ?? myGDSAddress.PostalCode) ?? "";
                        }
                        else
                        {
                            isNewAddress = true;
                            Location devLocation = (from l in idc.Locations where l.AccountID == account.AccountID && l.GDSLocation == gdsLocation select l).FirstOrDefault();

                            Country selCountry = (from c in idc.Countries where c.CountryID == int.Parse(ddlAddrCountry.SelectedValue) select c).FirstOrDefault();

                            AddrFmtCode countryFmtCode = AddrFmtCode.Domestic;
                            if (selCountry.PayPalCode == "US")
                                countryFmtCode = AddrFmtCode.Domestic;
                            else if (selCountry.PayPalCode == "CA")
                                countryFmtCode = AddrFmtCode.Canada;
                            else
                                countryFmtCode = AddrFmtCode.Other;

                            string stateAbbrev = "";
                            if (countryFmtCode == AddrFmtCode.Other)
                            {
                                stateAbbrev = selCountry.PayPalCode;
                            }
                            else
                            {
                                if (int.Parse(ddlAddrState.SelectedValue) > 0)
                                {
                                    State selState = (from s in idc.States where s.StateID == int.Parse(ddlAddrState.SelectedValue) select s).FirstOrDefault();
                                    if (selState != null)
                                    {
                                        stateAbbrev = selState.StateAbbrev;
                                    }
                                }
                            }

                            myAddress.Account = account.GDSAccount;
                            myAddress.Location = gdsLocation;
                            myAddress.Wearer = (int?)null;

                            myAddress.AddressPtr = myAddressPtr;
                            myAddress.AddrType = AddressType.Temp;
                            myAddress.NumSharedAddr = 0;
                            myAddress.ShareAddress = YesNo.Yes;
                            myAddress.ContactName = txtAddrContactName.Text.Trim();
                            myAddress.AddrFmtCode = countryFmtCode;
                            myAddress.CompanyName = account.CompanyName;

                            if (!string.IsNullOrEmpty(location.ShippingCompany))
                                myAddress.CompanyName = location.ShippingCompany;
                            else if (!string.IsNullOrEmpty(location.Shipping2Company))
                                myAddress.CompanyName = location.Shipping2Company;

                            myAddress.Attention = txtAddrContactName.Text.Trim();
                            myAddress.FaxNumber = devLocation.ShippingFax ?? "";
                            myAddress.Email = devLocation.ShippingEmailAddress ?? "";
                            myAddress.ContactPhone = txtAddrPhone.Text.Trim();
                            myAddress.Company = "001";
                            myAddress.AutoCorrectAddress = YesNo.Yes;

                            myAddress.CountryCode = selCountry.PayPalCode;
                            myAddress.CountryDesc = selCountry.CountryName.ToUpper();
                            myAddress.Address1 = txtAddrAddress1.Text.Trim();
                            myAddress.Address2 = txtAddrAddress2.Text.Trim();
                            myAddress.Address4 = null;
                            myAddress.City = txtAddrCity.Text.Trim();
                            myAddress.State = stateAbbrev;
                            myAddress.ZipCode = txtAddrZipCode.Text.Trim();


                            AddressRequests ar = new AddressRequests();
                            myAddressPtr = ar.AddAddress(myAddress, myInitial);                            

                            myAddress.AddressPtr = myAddressPtr;

                        }
                    }

                    CSWSInstaOrderHelper instaOrderHelperObj = new CSWSInstaOrderHelper();

                    List<InstadoseOrderDetailItem> myOrderDetailItemList = new List<InstadoseOrderDetailItem>();
                    List<RMAWearerInfo> myRMAWearerInfoList = new List<RMAWearerInfo>();

                    if (this.isSingleFlow)
                    {
                        InstadoseOrderDetailItem myOrderDetailItem = new InstadoseOrderDetailItem();
                        FillGDSInstaOrderDetailItem(serialNo, int.Parse(caseCoverProductID), shipWOAssignedUser, myOrderDetailItem);
                        myOrderDetailItemList.Add(myOrderDetailItem);

                        RMAWearerInfo myRMAWearerInfo = new RMAWearerInfo();
                        FillGDSRMAWearer(serialNo, myRMAWearerInfo);
                        myRMAWearerInfoList.Add(myRMAWearerInfo);
                    }
                    else
                    {
                        FillGDSInstaOrderDetailItemAndRMAWearerLists(myOrderDetailItemList, myRMAWearerInfoList);
                    }

                    InstadoseOrderItem myOrderItem = new InstadoseOrderItem();
                    myOrderItem.InsAccountID = account.AccountID;
                    myOrderItem.Account = account.GDSAccount;
                    myOrderItem.Location = gdsLocation;

                    if (DateTime.TryParse(ddlWearDate.SelectedValue, out DateTime wearDate))
                        myOrderItem.WearDate = wearDate;

                    myOrderItem.PONumber = "";
                    myOrderItem.OrderType = "D"; // D: Daily; R: Regular
                    myOrderItem.AddressPointer = myAddressPtr;
                    myOrderItem.GDSInstaOrderType = 1;  // 1: Recall; 2: Lost; 3: New; 4: Add-on
                    myOrderItem.ShipToWearer = rbtnUserAddress.Checked; // ship to wearer or to location
                    myOrderItem.InitializationDate = null;
                    myOrderItem.IsConsignmentOrder = false;
                    myOrderItem.Details = myOrderDetailItemList;
                    myOrderItem.IsShipmentBillable = false;

                    var recallReason = adc.rma_ref_ReturnReasons.FirstOrDefault(x => x.ReasonID == returnReasonID);
                    if (recallReason != null)
                    {
                        myOrderItem.IsExpressShipment = recallReason.ShippingMethod == _FedExOvernightShippingMethodName;
                        myOrderItem.ShippingMethod = string.IsNullOrEmpty(ddlShippingCarrier.SelectedValue) ? 0 : int.Parse(ddlShippingCarrier.SelectedValue);
                    }

                    // **************************** Process Recall Order *************************

                    string orderDailyResponse = "";

                    // 1. Create Instadose order
                    string addOrderInstadoseErrorMessage = "";
                    string addInstaOrderDailyErrorMessage = "";

                    int instaOrderID = int.Parse(instaOrderHelperObj.AddOrderInstadose(myOrderItem, myInitial, myRMAWearerInfoList, ref addOrderInstadoseErrorMessage));
                    // 2. Create Instadose order daily
                    if (instaOrderID > 0)
                    {
                        int.TryParse(ddlProduct.SelectedValue, out int productGroupId);

                        foreach (var item in myOrderItem.Details)
                        {
                            if (productGroupId == ProductGroupIDConstants.InstadoseVue || productGroupId == ProductGroupIDConstants.InstadoseVueBeta)
                            {
                                var device = idc.DeviceInventories.FirstOrDefault(x => x.SerialNo == item.SerialNo);
                                if (device != null)
                                {
                                    var id3DeviceConfig = idc.ID3_DeviceConfigurations.FirstOrDefault(X => X.DeviceID == device.DeviceID);
                                    if (id3DeviceConfig != null)
                                    {
                                        id3DeviceConfig.ServiceStatus_doUpdate = true;
                                        id3DeviceConfig.ServiceStatus_ReturnForService = true;

                                        idc.SubmitChanges();
                                    }
                                }                                
                            }
                            
                            if (item.UserID.HasValue)
                                AddOrderUserAssign(instaOrderID, item.SerialNo, item.ProductID.Value, item.ShipWOAssignedUser.Value, true, item.UserID);
                        }

                        //UPDATE PLOrder Address
                        if (isNewAddress)
                        {
                            var plOrder = udc.PLOrderSQLs.FirstOrDefault(x => x.GDSInstaOrderID == instaOrderID);
                            if (plOrder != null)
                            {
                                plOrder.PLAddressPtr = myAddressPtr;
                                plOrder.CompanyName = myAddress.CompanyName;
                                plOrder.AttentionName = null;
                                plOrder.ContactName = myAddress.ContactName;
                                plOrder.Address1 = myAddress.Address1;
                                plOrder.Address2 = myAddress.Address2;
                                plOrder.Address3 = null;
                                plOrder.City = myAddress.City;
                                plOrder.PostalCode = myAddress.ZipCode;
                                plOrder.CountryCode = myAddress.CountryCode;

                                udc.SubmitChanges();
                            }
                        }
                        
                    }
                    else
                    {                        
                        throw new Exception("Create InstaOrder error:" + "</br>" + addOrderInstadoseErrorMessage);
                    }

                    // 3. Create RMA and update Return with New orderid
                    if (!SaveReturn(instaOrderID, ref NewReturnID, ref submitError))
                    {                        
                        throw new Exception("SaveReturn error:" + "</br>" + submitError);
                    }

                    // 4. ******** Add AuditTransaction in CSWS **********//
                    MiscRequests miscReq = new MiscRequests();
                    int response = miscReq.AddAuditTransaction(account.GDSAccount, 0, "6V", myInitial, NewReturnID.ToString(), gdsLocation);
                    // ************************************************//

                    // 5. ************* Insert ReturnDevice, SendNotificationEmail, Deactivate UserDevice, Deactivate AccountDevice
                    if (this.isSingleFlow)
                    {
                        if (!InsertReturnDevice_SendNotificationEmail_DeactivateUserDevice_DeactivateAccountDevice(NewReturnID, account.AccountID, assignedUserID, serialNo, shipWOAssignedUser, ref submitError))
                        {                            
                            throw new Exception(submitError);
                        }
                    }
                    else
                    {
                        foreach (GridViewRow row in this.gv_DeviceList.Rows)
                        {
                            CheckBox chkBoxSelect = (CheckBox)row.FindControl("chkbxSelectDeviceList");

                            if (chkBoxSelect.Checked)
                            {
                                HiddenField HidAssignedUserID = (HiddenField)row.FindControl("HidAssignedUserID");
                                string iterSerialNo = row.Cells[3].Text.Replace("\n", "").ToString().Trim();
                                CheckBox iterChkbxShipWOAssigned = (CheckBox)row.FindControl("chkbxShipWOAssigned");
                                int iterAssignedUserID = string.IsNullOrEmpty(HidAssignedUserID.Value) ? 0 : int.Parse(HidAssignedUserID.Value);

                                if (!InsertReturnDevice_SendNotificationEmail_DeactivateUserDevice_DeactivateAccountDevice(NewReturnID, account.AccountID, iterAssignedUserID, iterSerialNo, iterChkbxShipWOAssigned.Checked, ref submitError))
                                {                                    
                                    throw new Exception(submitError);
                                }
                            }
                        }
                    }                  

                    string successMessage = "";
                    successMessage += "The order was placed successfully. </br>";
                    successMessage += "The temporary order number is: " + instaOrderID + "</br>";
                    successMessage += "It may take a few seconds to load order information. </br>";

                    successMessage += "RMA is created successfully. RMA# is: " + NewReturnID;

                    if (addInstaOrderDailyErrorMessage.Length > 0)
                    {
                        successMessage += "</br><b>But error occured while adding the daily for the instadose order:</b></br>";
                        successMessage += addInstaOrderDailyErrorMessage + "</br></br>";
                        successMessage += "Please contact IT.";
                    }

                    // Reset controls 's values
                    this.txtNotes.Text = "";
                    this.txtSpecialInstruction.Text = "";
                    this.txtSerialNoInput.Text = "";
                    this.txtAccountInput.Text = "";
                    this.txtSerialNoInput.Focus();

                    // Display New Requst ID and invisiable Devices Info Div.                                                
                    this.VisibleSuccess(successMessage);                    
                    VisibleRecallBody(false);
                }
                else
                {
                    int shipUserid = 0;
                    int shipLocationid = 0;

                    // Grab default Location
                    Location DefLoc = (from L in idc.Locations
                                       where L.IsDefaultLocation == true
                                       && L.AccountID == account.AccountID
                                       select L).FirstOrDefault();

                    if (rbtnUserAddress.Checked == true) // Check User Address radio button
                    {
                        if (this.isSingleFlow)
                        {
                            shipUserid = assignedUserID;     // tdo, 4/10/2012
                            shipLocationid = DefLoc.LocationID;
                        }
                        else
                        {
                            shipUserid = 0;  // right now , no solution for shipping to user with multiple recall flow. Just ship to selected location
                            shipLocationid = int.Parse(this.ddlLocation.SelectedValue);
                        }
                    }

                    if (rbtnLocationAddress.Checked == true)
                    {
                        // need to add assignedUserLocationID > 0 in the condition since some recalls are created with unassigned badges
                        if (this.isSingleFlow && assignedUserLocationID > 0)
                            shipLocationid = assignedUserLocationID;     // tdo, 4/10/2012
                        else
                            shipLocationid = int.Parse(this.ddlLocation.SelectedValue);
                    }

                    if (rbtnMainAddress.Checked == true)
                    {
                        shipLocationid = DefLoc.LocationID;
                    }

                    if (this.rbtnOtherLocation.Checked == true)
                    {
                        shipLocationid = int.Parse(this.txtLocationID.Text);
                    }

                    // **************************** Process Recall Order *************************
                    // 1. Create recall order
                    int orderID = 0;
                    orderID = CreateRecallOrder(account, UserName, shipLocationid, shipUserid, ref submitError);
                    if (orderID == 0)
                    {                        
                        throw new Exception("CreateRecallOrder error:" + "</br>" + submitError);
                    }

                    // 2. *************************** Insert OrderDetail *****************************                    
                    if (this.isSingleFlow)
                    {
                        if (!CreateSingleRecallOrderDetail(orderID, serialNo, caseCoverProductID, shipWOAssignedUser, UserName, ref submitError))
                        {                            
                            throw new Exception("CreateSingleRecallOrderDetail error:" + "</br>" + submitError);
                        }
                    }
                    else
                    {
                        if (!CreateMultipleRecallOrderDetail(orderID, UserName, ref submitError))
                        {                            
                            throw new Exception("CreateMultipleRecallOrderDetail error:" + "</br>" + submitError);
                        }
                    }
                    Basics.WriteLogEntry("The recall order details were inserted completely.", UserName, "Instadose.Processing.ProcessRecall.CreateRecallOrderDetail", Basics.MessageLogType.Information);

                    // 3. ********* Send order to MAS *****************//
                    if (!SendRecallOrderToMAS(orderID, shipUserid, shipLocationid, ref submitError))
                    {
                        Basics.WriteLogEntry("The recall order was not sent to MAS.", UserName, "Instadose.Processing.ProcessRecall.ProcessRecall", Basics.MessageLogType.Critical);
                        
                        throw new Exception("SendRecallOrderToMAS error:" + "</br>" + submitError);
                    }
                    Basics.WriteLogEntry("The recall order was sent to MAS.", UserName, "Instadose.Processing.ProcessRecall.ProcessRecall", Basics.MessageLogType.Information);

                    // 4. ********* Create rma_Return *****************//     
                    if (!SaveReturn(orderID, ref NewReturnID, ref submitError))
                    {                        
                        throw new Exception("SaveReturn error:" + "</br>" + submitError);
                    }

                    // 5. ******** update special instruction for order ************//
                    string specialInstructions = TrimNewLineString("Recall Replacement, Recall# " + NewReturnID.ToString() + ", Ship Overnight / 2nd Day. " + this.txtSpecialInstruction.Text.Trim());
                    Order order = (from o in idc.Orders
                                   where o.OrderID == orderID
                                   select o).FirstOrDefault();
                    order.SpecialInstructions = specialInstructions;
                    idc.SubmitChanges();

                    // 6. ************* Insert ReturnDevice, SendNotificationEmail, Deactivate UserDevice, Deactivate AccountDevice
                    if (this.isSingleFlow)
                    {
                        if (!InsertReturnDevice_SendNotificationEmail_DeactivateUserDevice_DeactivateAccountDevice(NewReturnID, account.AccountID, assignedUserID, serialNo, shipWOAssignedUser, ref submitError))
                        {                            
                            throw new Exception(submitError);
                        }
                    }
                    else
                    {
                        foreach (GridViewRow row in this.gv_DeviceList.Rows)
                        {
                            CheckBox chkBoxSelect = (CheckBox)row.FindControl("chkbxSelectDeviceList");

                            if (chkBoxSelect.Checked)
                            {
                                HiddenField HidAssignedUserID = (HiddenField)row.FindControl("HidAssignedUserID");
                                string iterSerialNo = row.Cells[3].Text.Replace("\n", "").ToString().Trim();
                                CheckBox iterChkbxShipWOAssigned = (CheckBox)row.FindControl("chkbxShipWOAssigned");
                                int iterAssignedUserID = string.IsNullOrEmpty(HidAssignedUserID.Value) ? 0 : int.Parse(HidAssignedUserID.Value);

                                if (!InsertReturnDevice_SendNotificationEmail_DeactivateUserDevice_DeactivateAccountDevice(NewReturnID, account.AccountID, iterAssignedUserID, iterSerialNo, iterChkbxShipWOAssigned.Checked, ref submitError))
                                {                                    
                                    throw new Exception(submitError);
                                }
                            }
                        }
                    }                                  

                    string successMessage = "";
                    successMessage += "The order was placed successfully. </br>";
                    successMessage += "The order number is: " + orderID + "</br>";

                    successMessage += "RMA is created successfully. RMA# is: " + NewReturnID;

                    this.txtNotes.Text = "";
                    this.txtSpecialInstruction.Text = "";
                    this.txtSerialNoInput.Text = "";
                    this.txtAccountInput.Text = "";
                    this.txtSerialNoInput.Focus();

                    // Display New Requst ID and invisiable Devices Info Div.                                                                
                    this.VisibleSuccess(successMessage);                    
                    VisibleRecallBody(false);
                }
            }

            btnContinue.Enabled = true;
        }
        catch (Exception ex)
        {
            btnContinue.Enabled = true;
            //this.lblError.Text = "Save error." + "</br>" + ex.ToString();
            this.VisibleErrors("Commit recall error:" + "</br>" + ex.ToString());
        }
    }

    private bool InsertNoteToAccount(string pAccountType, string pNote, string pAccountID, string pLocation, string pInitUserName, ref string pSubmitError)
    {
        bool commitSuccess = true;
        try
        {                        
            if (!string.IsNullOrEmpty(pNote))
            {
                if (pAccountType == "GDSAccount")
                {
                    // Perform the request.
                    MiscRequests miscReq = new MiscRequests();
                    int response = miscReq.AddNotes(pAccountID, pLocation, pNote, pInitUserName);

                    // If an error occurred display it, otherwise close the window
                    if (response != 0)
                    {
                        pSubmitError = string.Format("{0}: {1}", response, HError.GetError((response)));
                        commitSuccess = false;
                    }
                }
                else
                {
                    AccountNote an = new AccountNote
                    {
                        AccountID = int.Parse(pAccountID),
                        CreatedDate = DateTime.Now,
                        CreatedBy = pInitUserName,
                        NoteText = pNote,
                        Active = true
                    };

                    // Add the new object to the AccountNotes collection.
                    idc.AccountNotes.InsertOnSubmit(an);
                    idc.SubmitChanges();
                }                           
            }
        }
        catch (Exception ex)
        {
            pSubmitError = ex.Message;
            commitSuccess = false;
        }
        return commitSuccess;
    }

    /// <summary>
    /// Create new ReturnID, insert data to rma_retruns. Return true if committing successful
    /// </summary>
    /// <param name="pReturnID">Return generated ReturnID</param>
    /// <param name="pError">Return error</param>
    /// <returns>return true if committing successful</returns>
    private bool SaveReturn(int pOrderID, ref int pReturnID, ref string pError)
    {
        try
        {
            pError = "";
            rma_Return rma = null;
            rma = new rma_Return()
            {
                AccountID = int.Parse(this.lblAccountNo.Text),
                Active = true,
                Notes = this.txtNotes.Text.Trim(),
                Reason = this.ddlReturnReason.SelectedItem.Text,
                Return_ReasonID = int.Parse(this.ddlReturnReason.SelectedValue),
                ReturnTypeID = 3, //Type RMA = 3
                Status = 1,
                CreatedBy = this.UserName,
                CreatedDate = DateTime.Now,
                OrderID = pOrderID
            };
            adc.rma_Returns.InsertOnSubmit(rma);
            adc.SubmitChanges();

            // Return the ID that was generated.
            pReturnID = rma.ReturnID;

            // Insert data to Transaction Log with new ReturnID 
            var writeTransLogReturn = adc.sp_rma_process(rma.ReturnID, 0, 0, this.txtNotes.Text,
                    this.UserName, DateTime.Now, "ADD RETURN RMA", "Add retrunID: "
                    + rma.ReturnID.ToString(), 2);

            // return Notes to Header table
            rma_RMAHeader header = null;
            header = new rma_RMAHeader()
            {
                RMANumber = pOrderID.ToString().PadLeft(7, '0'),
                ReturnID = rma.ReturnID,
                Reason = this.txtNotes.Text.Trim(),
                Active = true,
                CreatedBy = this.UserName,
                CreatedDate = DateTime.Now
            };
            adc.rma_RMAHeaders.InsertOnSubmit(header);
            adc.SubmitChanges();           

            // Insert Note to Acct
            if ((bool)ViewState["IsGDSAccount"])
            {
                string gdsLocation = this.ddlLocation.SelectedItem.Value;
                string myInitial = this.UserName.Substring(0, 3);

                if (!InsertNoteToAccount("GDSAccount", this.txtNotes.Text.Trim(), account.GDSAccount, gdsLocation, myInitial, ref pError))
                {                    
                    return false;
                }
            }
            else
            {
                if (!InsertNoteToAccount("InstaAccount", this.txtNotes.Text.Trim(), account.AccountID.ToString(), "", this.UserName, ref pError))
                {
                    return false;
                }
            }

            // Insert data to Transaction Log with new ReturnID 
            var writeTransLogReturnHeader = adc.sp_rma_process(rma.ReturnID, 0, 0, this.txtNotes.Text,
                this.UserName, DateTime.Now, "ADD RETURN HEADER", "Add retrunID: " + rma.ReturnID.ToString() + ", RMAHeaderID: " + header.RMAHeaderID, 2);

            return true;
        }
        catch (Exception ex)
        {
            // Return the ID that was generated.
            pError = ex.ToString();
            pReturnID = 0;
            return false;
        }

    }

    private bool SaveSelectedSerialNo(int pReturnID, int pUserID, string sn, bool pShipWOAssignUser, ref string pError)
    {
        try
        {
            pError = "";

            // update existing Customer Request to inactive
            // select all existing requests where serialno = the inserted serialno.
            List<rma_ReturnDevice> returnDevice = (from rd in adc.rma_ReturnDevices
                                                   where rd.SerialNo == sn && rd.Active == true
                                                   select rd).ToList();

            // Deactivate all records.
            foreach (rma_ReturnDevice dbActive in returnDevice)
            {
                dbActive.Active = false;
            }


            // Find account information by Serial No 
            int myDeviceID = (from D1 in idc.DeviceInventories
                              where D1.SerialNo == sn
                              select D1.DeviceID).FirstOrDefault();


            // insert new returnDevices request to rma_retrunDevices with Department=3 (RMA)

            rma_ReturnDevice retrunDevices = null;
            retrunDevices = new rma_ReturnDevice()
            {
                ReturnID = pReturnID,
                SerialNo = sn,
                Notes = this.txtNotes.Text.Trim(),
                MasterDeviceID = myDeviceID,
                DepartmentID = 3, // RMA Department = 3
                Status = 1,
                Active = true,
                UserID = (pShipWOAssignUser) ? 0 : (pUserID != 0) ? pUserID : (int?)null
            };
            adc.rma_ReturnDevices.InsertOnSubmit(retrunDevices);
            adc.SubmitChanges();

            // Insert data to Transaction Log with new ReturnDevicesID
            var writeTransLogReturn = adc.sp_rma_process(pReturnID, retrunDevices.ReturnDevicesID, 0, this.txtNotes.Text,
                    this.UserName, DateTime.Now, "ADD DEVICE RMA", "Add deviceID: "
                    + retrunDevices.ReturnDevicesID.ToString() + "Serial# " + sn, 2);

            return true;
        }
        catch (Exception ex)
        {
            pError = ex.ToString();
            return false;
        }

    }

    private string GetAccountName(int AccountID)
    {
        string AccountName = "Unknown";
        try
        {
            AccountName = (from a in idc.Accounts
                           where a.AccountID == AccountID
                           select a.AccountName).FirstOrDefault();
        }
        catch (Exception ex)
        {
            //this.lblError.Text = "The account was not found.";
            this.VisibleErrors("The account was not found.");
            this.btnContinue.Enabled = false;
        }

        return AccountName;
    }

    protected void btn_sendEmailCS_Click(object sender, EventArgs e)
    {
        //// inVisible lblEmailMessage
        //this.lblEmailMessage.Visible = false;


        //// Get the RMA information and the Tech Notes from GridView 
        //// then generate the HTML email string
        //int EreturnRequestID = 0;
        //int emailRowCount = 0;
        //string ContentText = "";

        //// HTML format
        //string URLAccountInfo = "http://portal.instadose.com/InformationFinder/Details/Account.aspx?ID=";
        //string URLRmaInfo = "http://portal.instadose.com/InformationFinder/Details/Return.aspx?ID=";

        //string bodyText = "<html><body>";
        //bodyText += "<b>RMA’s for the following devices have been initiated. Please complete the RMA process<br><br><br>Details listed below:</b>";
        //bodyText += "<table border='1'>";
        //bodyText += "<tr><td align='center' width='60'><b>RMA#</b></td>";
        //bodyText += "<td align='center'width='60'><b>Account#</b></td>";
        //bodyText += "<td align='center'width='60'><b>Serial#</b></td>";
        //bodyText += "<td align='center'width='60'><b>LastName</b></td>";
        //bodyText += "<td align='center'width='60'><b>FirstName</b></td>";
        //bodyText += "<td align='center'width='250'><b>Notes</b></td></tr>";

        //for (int i = 0; i < this.gv_rmaList.Rows.Count; i++)
        //{
        //    GridViewRow gvRow = gv_rmaList.Rows[i];
        //    TextBox findTxtBox = (TextBox)gvRow.FindControl("Notes");
        //    CheckBox findChkBx = (CheckBox)gvRow.FindControl("cbxEmail");

        //    string EtechOpsNotes = findTxtBox.Text;
        //    bool EboolChkEmail = findChkBx.Checked;
        //    string EserialNo = gvRow.Cells[3].Text.Replace("\n", "").ToString().Trim();
        //    int EAccountID = int.Parse(gvRow.Cells[2].Text);
        //    int.TryParse(gvRow.Cells[1].Text, out EreturnRequestID);

        //    // if Check box send email is True, send email
        //    if (EboolChkEmail == true)
        //    {
        //        // Find return device information
        //        var adcRtnDevice = (from RD1 in adc.rma_ReturnDevices 
        //                           where RD1.ReturnID == EreturnRequestID && RD1.SerialNo==EserialNo
        //                           && RD1.Active == true
        //                           select RD1).FirstOrDefault();



        //        // Find the device's User Information
        //        // in SQL has to join AccountDevice and DeviceInventory then left join USERs
        //        // LINQ to join AccountDevice and DeviceInventory to get USERID
        //        var BdcAcctInfo = (from A1 in idc.AccountDevices
        //                     join D1 in idc.DeviceInventories
        //                     on A1.DeviceID equals D1.DeviceID
        //                     where A1.AccountID == EAccountID && D1.SerialNo== EserialNo
        //                     select new { A1, D1 }).ToList();

        //        // LINQ account-devices left join with Users
        //        var BdcUser = (from Acc in BdcAcctInfo
        //                   join U1 in idc.Users 
        //                   on Acc.A1.UserID equals U1.UserID 
        //                   into join2 
        //                   from U1 in join2.DefaultIfEmpty()
        //                   select new{Acc,U1}).FirstOrDefault();

        //        string FirstName = "";
        //        string LastName = "";
        //        if (BdcUser.U1 != null)
        //        {
        //             FirstName = BdcUser.U1.FirstName.ToString();
        //             LastName = BdcUser.U1.LastName.ToString();
        //        }

        //        ContentText += "<tr>";
        //        if (EreturnRequestID != 0)
        //        {
        //            bodyText += "<td align='center'><a href='" + URLRmaInfo + EreturnRequestID.ToString().Trim() + "'> " + EreturnRequestID.ToString().Trim() + "</a> </td>";
        //        }
        //        else
        //        {
        //            bodyText += "<td align='center'> N/A </td>";
        //        }
        //        bodyText += "<td td align='center' valign='top'><a href='" + URLAccountInfo + EAccountID.ToString().Trim() + "#Return_tab'> " + EAccountID.ToString().Trim() + "</a> </td>";
        //        bodyText += "<td td align='center' valign='top'> " + EserialNo.Trim().PadRight(10, ' ') + " </td>";
        //        bodyText += "<td td align='left' valign='top'> " + LastName + " </td>";
        //        bodyText += "<td td align='left' valign='top'> " + FirstName  + " </td>";
        //        bodyText += "<td td align='left' valign='top'> " + EtechOpsNotes + " </td>";
        //        bodyText += "</tr>";


        //        emailRowCount++;

        //        // 10/8/2010
        //        // Insert to Transaction Log with RMA Email Notification for each select RMA#
        //        var writeTransLogReturn = adc.sp_rma_process(EreturnRequestID,
        //            adcRtnDevice.ReturnDevicesID, 0, EtechOpsNotes,
        //            this.UserName, DateTime.Now, "EMAIL TECH TO CS",
        //            "Email from Tech Ops to CS. Serial# " + EserialNo , 2);


        //    }
        //} // END for Loop

        //bodyText += "</table>";
        //bodyText += "<table><tr><td>";
        //bodyText += "<br><br><font size='-1'>This email is generated by RMA Portal. <br>User: <b>"+ this.UserName + "</b></font>";
        //bodyText += "</td></tr></table>";
        //bodyText += "</body></html>";


        //if (emailRowCount > 0)
        //{
        //    // Send  email.
        //    SmtpClient client = new SmtpClient("172.16.121.69");
        //    MailMessage mail = new MailMessage();
        //    mail.IsBodyHtml = true;

        //    mail.From = new MailAddress("noreply@instadose.com", "RMA Portal Email");
        //    mail.Subject = "Initiated RMA/RECALL";
        //    mail.Body = bodyText;

        //    // email recipients To or CC or Bcc 


        //    if (DevelopmentServer == false)
        //    {
        //        mail.To.Add("info@QuantumBadges.com");
        //        mail.CC.Add("MWilliams@mirion.com");
        //        mail.CC.Add("dgonzalez@mirion.com");
        //        mail.Bcc.Add("gchu@mirion.com");
        //    }
        //    else
        //    { 
        //        string userEmail = this.UserName + "@mirion.com";
        //        mail.To.Add(userEmail);
        //        //mail.To.Add("gchu@mirion.com");
        //    }


        //    try
        //    {
        //        client.Send(mail);
        //        this.lblEmailMessage.Visible = true;
        //        this.lblEmailMessage.Text = "Email Sent!";
        //    }
        //    catch
        //    {
        //        this.lblEmailMessage.Visible = true;
        //        this.lblEmailMessage.Text = "Connection errors! Please send Email again.. ";
        //    }
        //}
        //else
        //{
        //    this.lblEmailMessage.Visible = true;
        //    this.lblEmailMessage.Text = "Email can't be sent due to No RMA have been selected!";
        //}

    } // End SendEmail Click

    //display all RMA info, allow Tech Ops send email notifications to CS

    // email notification history
    private void ViewEmailHistory(int myReturnID, int myReturnDeviceID)
    {
        // Create Email history data table
        var query = (from L1 in adc.rma_TransLogs
                     where L1.ReturnID == myReturnID
                     && L1.ReturnDevicesID == myReturnDeviceID
                     && L1.LogAction == "EMAIL TECH TO CS"
                     orderby L1.CreatedDate
                     select L1);
        // List Email History
        gv_EmailHistory.DataSource = query;
        gv_EmailHistory.DataBind();
    }
    // END email notification history

    // Live server is running Central time, Need function convert to PDT 
    public string GetPDTtime(string myDateTime_b)
    {
        string ReturnStr = DateTime.Now.ToString();

        if (myDateTime_b.ToString() != "")
        {
            DateTime myDateTime = DateTime.Parse(myDateTime_b);
            ReturnStr = myDateTime.AddHours(-2).ToString(); // minus 2 hrs
        }

        return ReturnStr;
    }
    // END Live server is running Central time, Need function convert to PDT

    /// <summary>
    /// Send email with template converted string
    /// </summary>
    /// <param name="HtmlString"></param>
    /// <param name="TextString"></param>
    /// <param name="MessageSubject"></param>
    /// <param name="ToaddressList"></param>
    /// <param name="AllFieldsDict"></param>
    /// <returns></returns>
    protected bool SendRecallMessage(string EmailFormatType,
        List<string> ToaddressList, Dictionary<string, string> AllFieldsDict)
    {

        bool myreturn = false;

        MessageSystem SendMessage = new MessageSystem();
        SendMessage.Application = "RMA_Recall";
        SendMessage.CreatedBy = UserName;
        SendMessage.FromAddress = "noreply@instadose.com";

        //To Address List
        if (DevelopmentServer == false)
        {
            SendMessage.ToAddressList = ToaddressList;
        }
        else
        {
            SendMessage.ToAddressList = new List<string>();
            SendMessage.ToAddressList.Add("tdo@mirion.com");
        }

        SendMessage.CcAddressList = new List<string>();

        // Build the template fields and Send the Mesasge
        try
        {
            int MySendID;

            if (EmailFormatType == "Admin")
                MySendID = SendMessage.Send("Admin Return Request", "", AllFieldsDict);
            else
                MySendID = SendMessage.Send("Wearer Return Request", "", AllFieldsDict);

            myreturn = true;
        }
        catch
        {
            myreturn = false;
        }

        return myreturn;

    }

    /// <summary>
    /// Display User Shipping Address in DB
    /// </summary>
    /// <param name="mySerialNo"></param>
    /// <returns>Address String</returns>
    public string DisplayUserShippingAddress(User pUser, Account pAccount, string pLocation)
    {
        string AddressInfo = "";

        if ((bool)ViewState["IsGDSAccount"])
        {
            if (pUser.GDSWearerID.HasValue)
            {
                WearerRequests wr = new WearerRequests();
                var w = wr.GetWearer(pAccount.GDSAccount, pLocation, pUser.GDSWearerID.Value);

                if (w != null && w.WearerAddress != null)
                {
                    AddressInfo = w.WearerAddress.ContactName + "</br>";
                    AddressInfo += w.WearerAddress.Address1 + "</br>";
                    if (w.WearerAddress.Address2 != null && w.WearerAddress.Address2.Length > 0)
                        AddressInfo += w.WearerAddress.Address2 + "</br>";
                    AddressInfo += w.WearerAddress.City + ", " + w.WearerAddress.State + " "
                        + w.WearerAddress.ZipCode + "</br>" + w.WearerAddress.CountryCode + "</br>";
                }
            }
        }
        else
        {
            var ShipAddress = (from a in idc.Users
                               join s in idc.States
                               on a.StateID equals s.StateID
                               join c in idc.Countries
                               on a.CountryID equals c.CountryID
                               where a.UserID == pUser.UserID
                               select new { a, s.StateAbbrev, c.CountryCode }).FirstOrDefault();

            if (ShipAddress != null)
            {
                AddressInfo = ShipAddress.a.FirstName + " " + ShipAddress.a.LastName + "</br>";
                AddressInfo += ShipAddress.a.Address1 + "</br>";
                if (ShipAddress.a.Address2 != null && ShipAddress.a.Address2.Length > 0)
                    AddressInfo += ShipAddress.a.Address2 + "</br>";
                AddressInfo += ShipAddress.a.City + ", " + ShipAddress.StateAbbrev + " "
                    + ShipAddress.a.PostalCode + "</br>" + ShipAddress.CountryCode + "</br>";
            }
            else
                AddressInfo = "";
        }

        return AddressInfo;
    }
    public string DisplayLocationShippingAddress(User pUser, Account pAccount, string pLocation)
    {
        string AddressInfo = "";

        if ((bool)ViewState["IsGDSAccount"])
        {
            CSWSInstaOrderHelper instaOrderHelperObj = new CSWSInstaOrderHelper();
            GLocationDetails locationDetail = instaOrderHelperObj.GetLocationDetail(pAccount.GDSAccount, pLocation);

            if (locationDetail != null)
            {
                AddressInfo = locationDetail.ShippingCompany + "</br>";
                AddressInfo += locationDetail.ShippingContact + "</br>";
                AddressInfo += locationDetail.ShippingAddress1 + "</br>";
                if (!String.IsNullOrEmpty(locationDetail.ShippingAddress2))
                    AddressInfo += locationDetail.ShippingAddress2 + "</br>";
                AddressInfo += locationDetail.ShippingCity + ", " + locationDetail.ShippingState + " "
                    + locationDetail.ShippingZipCode + "</br>" + locationDetail.ShippingCountryCode + "</br>";
            }
        }
        else
        {
            var ShipAddress = (from a in idc.Users
                               join b in idc.Locations
                               on a.LocationID equals b.LocationID
                               join s in idc.States
                               on b.ShippingStateID equals s.StateID
                               join c in idc.Countries
                               on b.ShippingCountryID equals c.CountryID
                               where a.UserID == pUser.UserID
                               select new { b, s.StateAbbrev, c.CountryCode }).FirstOrDefault();

            if (ShipAddress != null)
            {
                AddressInfo = ShipAddress.b.ShippingCompany + "</br>";
                AddressInfo += ShipAddress.b.ShippingFirstName + " " + ShipAddress.b.ShippingLastName + "</br>";
                AddressInfo += ShipAddress.b.ShippingAddress1 + "</br>";
                if (ShipAddress.b.ShippingAddress2 != null && ShipAddress.b.ShippingAddress2.Length > 0)
                    AddressInfo += ShipAddress.b.ShippingAddress2 + "</br>";
                AddressInfo += ShipAddress.b.ShippingCity + ", " + ShipAddress.StateAbbrev + " "
                    + ShipAddress.b.ShippingPostalCode + "</br>" + ShipAddress.CountryCode + "</br>";
            }
        }

        return AddressInfo;
    }
    public string DisplayLocationShippingAddress(Account pAccount, string pLocation)
    {
        string AddressInfo = "";

        if ((bool)ViewState["IsGDSAccount"])
        {
            CSWSInstaOrderHelper instaOrderHelperObj = new CSWSInstaOrderHelper();
            GLocationDetails locationDetail = instaOrderHelperObj.GetLocationDetail(pAccount.GDSAccount, pLocation);

            if (locationDetail != null)
            {
                AddressInfo = locationDetail.ShippingCompany + "</br>";
                AddressInfo += locationDetail.ShippingContact + "</br>";
                AddressInfo += locationDetail.ShippingAddress1 + "</br>";
                if (!String.IsNullOrEmpty(locationDetail.ShippingAddress2))
                    AddressInfo += locationDetail.ShippingAddress2 + "</br>";
                AddressInfo += locationDetail.ShippingCity + ", " + locationDetail.ShippingState + " "
                    + locationDetail.ShippingZipCode + "</br>" + locationDetail.ShippingCountryCode + "</br>";
            }
        }
        else
        {
            var ShipAddress = (from b in idc.Locations
                               join s in idc.States
                               on b.ShippingStateID equals s.StateID
                               join c in idc.Countries
                               on b.ShippingCountryID equals c.CountryID
                               where b.LocationID == int.Parse(pLocation)
                               select new { b, s.StateAbbrev, c.CountryCode }).FirstOrDefault();

            if (ShipAddress != null)
            {
                AddressInfo = ShipAddress.b.ShippingCompany + "</br>";
                AddressInfo += ShipAddress.b.ShippingFirstName + " " + ShipAddress.b.ShippingLastName + "</br>";
                AddressInfo += ShipAddress.b.ShippingAddress1 + "</br>";
                if (ShipAddress.b.ShippingAddress2 != null && ShipAddress.b.ShippingAddress2.Length > 0)
                    AddressInfo += ShipAddress.b.ShippingAddress2 + "</br>";
                AddressInfo += ShipAddress.b.ShippingCity + ", " + ShipAddress.StateAbbrev + " "
                    + ShipAddress.b.ShippingPostalCode + "</br>" + ShipAddress.CountryCode + "</br>";
            }
        }

        return AddressInfo;
    }
    public string DisplayOtherLocationShippingAddress(int myLocationID)
    {
        string AddressInfo = "";

        var ShipAddress = (from b in idc.Locations
                           join s in idc.States
                           on b.ShippingStateID equals s.StateID
                           join c in idc.Countries
                           on b.ShippingCountryID equals c.CountryID
                           where b.LocationID == myLocationID
                           select new { b, s.StateAbbrev, c.CountryCode }).FirstOrDefault();

        if (ShipAddress != null)
        {
            AddressInfo = ShipAddress.b.ShippingCompany + "</br>";
            AddressInfo += ShipAddress.b.ShippingFirstName + " " + ShipAddress.b.ShippingLastName + "</br>";
            AddressInfo += ShipAddress.b.ShippingAddress1 + "</br>";
            if (ShipAddress.b.ShippingAddress2 != null && ShipAddress.b.ShippingAddress2.Length > 0)
                AddressInfo += ShipAddress.b.ShippingAddress2 + "</br>";
            AddressInfo += ShipAddress.b.ShippingCity + ", " + ShipAddress.StateAbbrev + " "
                + ShipAddress.b.ShippingPostalCode + "</br>" + ShipAddress.CountryCode + "</br>";
        }
        else
            AddressInfo = "";

        return AddressInfo;
    }
    public string DisplayMainShippingAddress(int myAccountID)
    {
        string AddressInfo = "";

        var ShipAddress = (from a in idc.Accounts
                           join b in idc.Locations
                           on a.AccountID equals b.AccountID
                           join s in idc.States
                           on b.ShippingStateID equals s.StateID
                           join c in idc.Countries
                           on b.ShippingCountryID equals c.CountryID
                           where a.AccountID == myAccountID
                           && b.IsDefaultLocation == true
                           select new { b, s.StateAbbrev, c.CountryCode }).FirstOrDefault();

        if (ShipAddress != null)
        {
            AddressInfo = ShipAddress.b.ShippingCompany + "</br>";
            AddressInfo += ShipAddress.b.ShippingFirstName + " " + ShipAddress.b.ShippingLastName + "</br>";
            AddressInfo += ShipAddress.b.ShippingAddress1 + "</br>";
            if (ShipAddress.b.ShippingAddress2 != null && ShipAddress.b.ShippingAddress2.Length > 0)
                AddressInfo += ShipAddress.b.ShippingAddress2 + "</br>";
            AddressInfo += ShipAddress.b.ShippingCity + ", " + ShipAddress.StateAbbrev + " "
                + ShipAddress.b.ShippingPostalCode + "</br>" + ShipAddress.CountryCode + "</br>";
        }
        else
            AddressInfo = "";

        return AddressInfo;
    }
    //----------------------------------------------------
    private int CreateRecallOrder(Account pAccount, string pUserName, int pShipLocationID, int MyShipUserID, ref string pError)
    {
        try
        {
            pError = "";

            // Set the currency code (Default set to USD) *can combine with DInfo with a left join*
            String currencyCode = "USD";
            String referralCode = string.IsNullOrEmpty(pAccount.ReferralCode) ? "0000" : pAccount.ReferralCode;
            currencyCode = pAccount.CurrencyCode;
            String BillingMethodStr = (pAccount.BillingTerm.ToString() == "Purchase Order") ? "PO" : "CC";

            // submit Order to business apps Orders table
            Order NewOrder = null;
            NewOrder = new Order();

            NewOrder.OrderType = "RECALL REPLACEMENT";
            NewOrder.PONumber = "REPLACEDIS";
            NewOrder.AccountID = pAccount.AccountID;

            Location orderLocation = null;

            if (pShipLocationID > 0)
            {
                orderLocation = (from l in idc.Locations where l.LocationID == pShipLocationID select l).FirstOrDefault();
            }
            else
            {
                // Grab account default Location
                orderLocation = (from L in idc.Locations where L.IsDefaultLocation == true && L.AccountID == pAccount.AccountID select L).FirstOrDefault();
            }

            // Ship to a location
            NewOrder.LocationID = orderLocation.LocationID;

            // indicate shipping to user's address.
            NewOrder.ShippingUserID = (rbtnUserAddress.Checked == true && MyShipUserID != 0) ? MyShipUserID : (int?)null;

            NewOrder.BrandSourceID = int.Parse(pAccount.BrandSourceID.ToString());

            var shippingMethodId = GetShippingMethodID(pAccount.AccountID);
            var returnReasonID = int.Parse(this.ddlReturnReason.SelectedValue);
            var recallReason = adc.rma_ref_ReturnReasons.FirstOrDefault(x => x.ReasonID == returnReasonID);
            if (recallReason != null)
            {
                var shippingMethod = idc.ShippingMethods.FirstOrDefault(x => x.ShippingMethodDesc == recallReason.ShippingMethod);
                if (shippingMethod != null)
                    shippingMethodId = shippingMethod.ShippingMethodID;
            }

            NewOrder.ShippingMethodID = shippingMethodId;
            NewOrder.OrderDate = DateTime.Now;
            NewOrder.CreatedBy = pUserName;
            NewOrder.OrderStatusID = 3; // Order is created, released to MAS, in fullfillment.
            NewOrder.ReferralCode = referralCode;
            NewOrder.SoftraxStatus = false;
            NewOrder.CurrencyCode = currencyCode;
            NewOrder.Discount = double.Parse("0");
            //if (ddlCoupon.SelectedValue.ToString() != "0")
            //    NewOrder.CouponID = int.Parse(ddlCoupon.SelectedValue.ToString());
            NewOrder.OrderSource = "Business App";
            NewOrder.SpecialInstructions = "";
            NewOrder.PORequired = false;
            NewOrder.ShippingCharge = decimal.Parse("0");

            // Set Account ContractStartDate and ContractEndDate to Order.
            NewOrder.ContractStartDate = GetContractStartDate(pAccount, orderLocation);
            NewOrder.ContractEndDate = GetContractEndDate(pAccount, orderLocation);

            //if (pAccount.BrandSourceID == 3)
            //{
            //    NewOrder.StxContractStartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);  // first day of month of today
            //    NewOrder.StxContractEndDate = NewOrder.ContractEndDate;
            //}
            //else
            //{
            //    NewOrder.StxContractStartDate = GetDefaultSoftTraxStartDate(false, NewOrder.ContractStartDate, NewOrder.ContractEndDate, DateTime.Now);
            //    NewOrder.StxContractEndDate = NewOrder.ContractEndDate;

            //    // if a quarterly account set the account date to the end of the quarter
            //    if (pAccount.BillingTermID == 1)
            //    {
            //        // Set the contract end date to the quarter.
            //        NewOrder.StxContractEndDate = Common.calculateQuarterlyServiceEnd(NewOrder.ContractStartDate, NewOrder.ContractEndDate, NewOrder.StxContractStartDate.Value);
            //    }

            //    // set stx contract dates by selected service start date
            //    string proRatePeriod = radProratePeriod.SelectedItem.Text;
            //    string[] startEndProRatePeriod = proRatePeriod.Split('-');
            //    NewOrder.StxContractStartDate = DateTime.Parse(startEndProRatePeriod[0].Trim());
            //    NewOrder.StxContractEndDate = DateTime.Parse(startEndProRatePeriod[1].Trim());
            //}

            // set stx contract dates by selected service start date
            string proRatePeriod = radProratePeriod.SelectedItem.Text;
            string[] startEndProRatePeriod = proRatePeriod.Split('-');
            NewOrder.StxContractStartDate = DateTime.ParseExact(startEndProRatePeriod[0].Trim(), "MM/dd/yyyy", CultureInfo.InvariantCulture);
            NewOrder.StxContractEndDate = DateTime.ParseExact(startEndProRatePeriod[1].Trim(), "MM/dd/yyyy", CultureInfo.InvariantCulture);

            NewOrder.ACMIntegrationStatusID = 1;
            idc.Orders.InsertOnSubmit(NewOrder);
            idc.SubmitChanges();

            Basics.WriteLogEntry("New order for RMA Recall to Orders table, ID=" + NewOrder.OrderID.ToString(), pUserName,
                "Instadose.Processing.ProcessRecall.CreateRecallOrder", Basics.MessageLogType.Information);

            return NewOrder.OrderID;

        }
        catch (Exception ex)
        {
            pError = ex.Message;
            Basics.WriteLogEntry(ex.Message, pUserName,
                "Instadose.Processing.ProcessRecall.CreateRecallOrder", Basics.MessageLogType.Critical);
            return 0;
        }
    }

    private DateTime GetContractStartDate(Account account, Location location)
    {
        // If an account is set up for location billing, then contract start date = location.ContractStartDate. Else then contract start date = account.ContractStartDate
        if (account.UseLocationBilling && location.ContractStartDate.HasValue)
            return location.ContractStartDate.Value;
        else
            return account.ContractStartDate.Value;
    }

    private DateTime GetContractEndDate(Account account, Location location)
    {
        // If an account is set up for location billing, then contract end date = location.ContractEndDate. Else then contract end date = account.ContractEndDate
        if (account.UseLocationBilling && location.ContractEndDate.HasValue)
            return location.ContractEndDate.Value;
        else
            return account.ContractEndDate.Value;
    }

    private DateTime GetDefaultSoftTraxStartDate(bool pIsFirstOrder, DateTime pContractStartDate, DateTime pContractEndDate, DateTime pOrderCreatedDate)
    {
        try
        {
            if (pIsFirstOrder)
                return pContractStartDate;
            else
            {
                if (pOrderCreatedDate > pContractEndDate)  // an account expire. Account contract end date < today
                {
                    return pContractStartDate;
                }
                else
                {
                    DateTime startQuarterDate;
                    int qtrNo = Common.calculateNumberOfQuarterService(pContractStartDate, pContractEndDate, pOrderCreatedDate, out startQuarterDate);
                    return startQuarterDate;
                }
            }
        }
        catch (Exception)
        {
            return pContractStartDate;
        }
    }

    private bool CreateSingleRecallOrderDetail(int pOrderID, string pSerialNumber, string pCaseCoverProductID, bool pShipWOAssignedUser, string pUserName, ref string pError)
    {
        try
        {
            pError = "";

            int productGroupId;
            int.TryParse(ddlProduct.SelectedValue, out productGroupId);

            int productID = 0;
            if (productGroupId == ProductGroupIDConstants.Instadose2 || productGroupId == ProductGroupIDConstants.InstaLink || productGroupId == ProductGroupIDConstants.InstaLinkUSB || productGroupId == ProductGroupIDConstants.InstaLink3) // No need case cover. Color will be No Color
            {
                int recallProductID = 0;

                if (productGroupId == ProductGroupIDConstants.Instadose2)
                {
                    recallProductID = (from di in idc.DeviceInventories
                                       where di.SerialNo == pSerialNumber
                                       select di.ProductID).FirstOrDefault().Value;
                }
                else
                {
                    recallProductID = (from p in idc.Products
                                       where p.ProductGroupID == productGroupId && p.Active == true && p.SoldOnWeb == true
                                       select p.ProductID).FirstOrDefault();
                }

                productID = recallProductID;

                // adding specific product to OrderDetails
                OrderDetail NewINSTAOrdDetail = new OrderDetail();
                NewINSTAOrdDetail.OrderID = pOrderID;
                NewINSTAOrdDetail.Price = 0;
                NewINSTAOrdDetail.Quantity = 1;
                NewINSTAOrdDetail.OrderDetailDate = DateTime.Now;
                NewINSTAOrdDetail.ProductID = recallProductID;

                idc.OrderDetails.InsertOnSubmit(NewINSTAOrdDetail);
            }
            else
            {
                string recallProductSKU = "";
                switch (productGroupId)
                {
                    case ProductGroupIDConstants.Instadose1:
                    case ProductGroupIDConstants.Instadose1IMI:
                        recallProductSKU = "INSTA10-B";
                        break;
                    case ProductGroupIDConstants.InstadosePlus:
                        recallProductSKU = "INSTA PLUS";
                        break;
                    case ProductGroupIDConstants.InstadoseVue:
                        recallProductSKU = Instadose3Constants.ProductSKUINSTAID3;
                        break;
                    case ProductGroupIDConstants.InstadoseVueBeta:
                        recallProductSKU = InstadoseVueBetaConstants.ProductSKUINSTAIDVueB;
                        break;
                    case ProductGroupIDConstants.Instadose2New:
                        recallProductSKU = "INSTA ID2";
                        break;
                    default:
                        break;
                }

                int nocolorProductID = (from p in idc.Products
                                        where p.ProductSKU == recallProductSKU && p.Active == true && p.SoldOnWeb == true
                                        select p.ProductID).FirstOrDefault();

                // adding specific product to OrderDetails
                OrderDetail NewINSTAOrdDetail = new OrderDetail();
                NewINSTAOrdDetail.OrderID = pOrderID;
                NewINSTAOrdDetail.Price = 0;
                NewINSTAOrdDetail.Quantity = 1;
                NewINSTAOrdDetail.OrderDetailDate = DateTime.Now;
                NewINSTAOrdDetail.ProductID = nocolorProductID;

                idc.OrderDetails.InsertOnSubmit(NewINSTAOrdDetail);

                // adding case cover to OrderDetails
                OrderDetail NewCaseCVROrdDetail = new OrderDetail();
                NewCaseCVROrdDetail.OrderID = pOrderID;
                NewCaseCVROrdDetail.Price = 0;
                NewCaseCVROrdDetail.Quantity = 1;
                NewCaseCVROrdDetail.OrderDetailDate = DateTime.Now;
                NewCaseCVROrdDetail.ProductID = int.Parse(pCaseCoverProductID);

                productID = NewCaseCVROrdDetail.ProductID;
                idc.OrderDetails.InsertOnSubmit(NewCaseCVROrdDetail);               
            }

            AddOrderUserAssign(pOrderID, pSerialNumber, productID, pShipWOAssignedUser, false, null);
            idc.SubmitChanges();

            Basics.WriteLogEntry("New order detail for RMA Recall to OrderDetail table, OrderID=" + pOrderID.ToString(), pUserName,
              "Instadose.Processing.ProcessRecall.CreateSingleRecallOrderDetail", Basics.MessageLogType.Information);

            return true;

        }
        catch (Exception ex)
        {
            pError = ex.Message;
            Basics.WriteLogEntry(ex.Message, pUserName,
                "Instadose.Processing.ProcessRecall.CreateSingleRecallOrderDetail", Basics.MessageLogType.Critical);
            return false;
        }
    }

    private void AddOrderUserAssign(int pOrderID, string pSerialNumber, int productID, bool pShipWOAssignedUser, bool isGdsInstaOrder, int? userID)
    {
        OrderUserAssign newOrderUserAssign = new OrderUserAssign();
        newOrderUserAssign.OrderID = pOrderID;
        newOrderUserAssign.ProductID = productID;
        newOrderUserAssign.IsGDSInstaOrder = isGdsInstaOrder;
        newOrderUserAssign.UserID = userID;

        var diad = (from di in idc.DeviceInventories
                    join ad in idc.AccountDevices on di.DeviceID equals ad.DeviceID
                    where di.SerialNo == pSerialNumber
                    select ad).FirstOrDefault();
        if (diad != null)
        {
            newOrderUserAssign.DeviceLocationID = diad.LocationID ?? (int?)null;
            newOrderUserAssign.RecalledDeviceID = diad.DeviceID;
            newOrderUserAssign.RecalledDeviceAssignmentDate = diad.AssignmentDate;
        }
        // Insert OrderUserAssign if device currently is assigned to a user.                  
        var ud = (from a in idc.DeviceInventories
                  join ad in idc.AccountDevices on a.DeviceID equals ad.DeviceID
                  join u in idc.UserDevices on a.DeviceID equals u.DeviceID
                  where a.SerialNo == pSerialNumber
                        && ad.CurrentDeviceAssign == true
                        && u.Active == true
                  select u).FirstOrDefault();

        if (ud != null)
        {
            newOrderUserAssign.UserID = ud.UserID;
            newOrderUserAssign.BodyRegionID = ud.BodyRegionID;
            newOrderUserAssign.PrimaryDevice = ud.PrimaryDevice;
        }
        else
        {
            newOrderUserAssign.BodyRegionID = 4; // unassigned
        }


        idc.OrderUserAssigns.InsertOnSubmit(newOrderUserAssign);
        idc.SubmitChanges();
    }

    private bool CreateMultipleRecallOrderDetail(int pOrderID, string pUserName, ref string pError)
    {
        try
        {
            pError = "";

            int productGroupId;
            int.TryParse(ddlProduct.SelectedValue, out productGroupId);

            if (productGroupId == ProductGroupIDConstants.Instadose2 || productGroupId == ProductGroupIDConstants.InstaLink || productGroupId == ProductGroupIDConstants.InstaLinkUSB || productGroupId == ProductGroupIDConstants.InstaLink3) // No need case cover
            {
                int recallProductID = 0;

                if (productGroupId == ProductGroupIDConstants.Instadose2)
                {
                    // ID2 Elite has the same ProductSKU for White color, so select the first productID for ID2 Elite
                    recallProductID = (from p in idc.Products
                                       where p.ProductGroupID == productGroupId && p.Active == true && p.SoldOnWeb == true
                                       select p.ProductID).FirstOrDefault();
                }
                else
                {
                    recallProductID = (from p in idc.Products
                                       where p.ProductGroupID == productGroupId && p.Active == true && p.SoldOnWeb == true
                                       select p.ProductID).FirstOrDefault();
                }

                int qty = 0;
                foreach (GridViewRow row in this.gv_DeviceList.Rows)
                {
                    CheckBox chkBoxSelect = (CheckBox)row.FindControl("chkbxSelectDeviceList");

                    if (chkBoxSelect.Checked)
                    {
                        qty += 1;

                        // JT: Untested
                        string iterSerialNo = row.Cells[3].Text.Replace("\n", "").ToString().Trim();
                        CheckBox iterChkbxShipWOAssigned = (CheckBox)row.FindControl("chkbxShipWOAssigned");
                        HiddenField hidAssignedUserID = (HiddenField)row.FindControl("HidAssignedUserID");

                        int? userID = null;
                        if (!string.IsNullOrEmpty(hidAssignedUserID.Value))
                            userID = int.Parse(hidAssignedUserID.Value);

                        AddOrderUserAssign(pOrderID, iterSerialNo, recallProductID, iterChkbxShipWOAssigned.Checked, false, userID);
                    }
                }

                // adding specific product to OrderDetails
                OrderDetail NewINSTAOrdDetail = new OrderDetail();
                NewINSTAOrdDetail.OrderID = pOrderID;
                NewINSTAOrdDetail.Price = 0;
                NewINSTAOrdDetail.Quantity = qty;
                NewINSTAOrdDetail.OrderDetailDate = DateTime.Now;
                NewINSTAOrdDetail.ProductID = recallProductID;

                idc.OrderDetails.InsertOnSubmit(NewINSTAOrdDetail);
            }
            else
            {
                int totalRecallDevices = 0;

                int totalBlack = 0;
                int totalSilver = 0;
                int totalBlue = 0;
                int totalGreen = 0;
                int totalPink = 0;
                int totalGrey = 0;
                int totalOrange = 0;
                int totalRed = 0;

                // ************* Add assigned user to OrderUserAssign table and count total for each color ************* //
                foreach (GridViewRow row in this.gv_DeviceList.Rows)
                {
                    CheckBox chkBoxSelect = (CheckBox)row.FindControl("chkbxSelectDeviceList");

                    if (chkBoxSelect.Checked)
                    {
                        string iterSerialNo = row.Cells[3].Text.Replace("\n", "").ToString().Trim();
                        CheckBox iterChkbxShipWOAssigned = (CheckBox)row.FindControl("chkbxShipWOAssigned");
                        DropDownList ddlCaseCoverProductID = (DropDownList)row.FindControl("ddlColor");
                        HiddenField hidAssignedUserID = (HiddenField)row.FindControl("HidAssignedUserID");

                        int? userID = null;
                        if (!string.IsNullOrEmpty(hidAssignedUserID.Value))
                            userID = int.Parse(hidAssignedUserID.Value);

                        AddOrderUserAssign(pOrderID, iterSerialNo, int.Parse(ddlCaseCoverProductID.SelectedValue), iterChkbxShipWOAssigned.Checked, false, userID);                        

                        string color = (from p in idc.Products
                                        where p.ProductID == int.Parse(ddlCaseCoverProductID.SelectedValue)
                                        select p.Color).FirstOrDefault();

                        switch (color.ToUpper())
                        {
                            case "BLACK":
                                totalBlack++;
                                break;
                            case "SILVER":
                                totalSilver++;
                                break;
                            case "BLUE":
                                totalBlue++;
                                break;
                            case "GREY":
                                totalGrey++;
                                break;
                            case "GREEN":
                                totalGreen++;
                                break;
                            case "ORANGE":
                                totalOrange++;
                                break;
                            case "PINK":
                                totalPink++;
                                break;
                            case "RED":
                                totalRed++;
                                break;
                        }
                    }
                }

                // ******************** adding No Color product to OrderDetails ************************//
                string nocolorProductSKU = "";
                switch (productGroupId)
                {
                    case ProductGroupIDConstants.Instadose1:
                    case ProductGroupIDConstants.Instadose1IMI:
                        nocolorProductSKU = "INSTA10-B";
                        break;
                    case ProductGroupIDConstants.InstadosePlus:
                        nocolorProductSKU = "INSTA PLUS";
                        break;
                    case ProductGroupIDConstants.InstadoseVue:
                        nocolorProductSKU = Instadose3Constants.ProductSKUINSTAID3;
                        break;
                    case ProductGroupIDConstants.InstadoseVueBeta:
                        nocolorProductSKU = InstadoseVueBetaConstants.ProductSKUINSTAIDVueB;
                        break;
                    case ProductGroupIDConstants.Instadose2New:
                        nocolorProductSKU = "INSTA ID2";
                        break;
                    default:
                        break;
                }

                int nocolorProductID = (from p in idc.Products
                                        where p.ProductSKU == nocolorProductSKU && p.Active == true && p.SoldOnWeb == true
                                        select p.ProductID).FirstOrDefault();

                totalRecallDevices = totalBlack + totalSilver + totalBlue + totalGrey + totalGreen + totalOrange + totalPink + totalRed;

                OrderDetail NewINSTAOrdDetail = new OrderDetail();
                NewINSTAOrdDetail.OrderID = pOrderID;
                NewINSTAOrdDetail.Price = 0;
                NewINSTAOrdDetail.Quantity = totalRecallDevices;
                NewINSTAOrdDetail.OrderDetailDate = DateTime.Now;
                NewINSTAOrdDetail.ProductID = nocolorProductID;

                idc.OrderDetails.InsertOnSubmit(NewINSTAOrdDetail);

                // ****************** Adding color product for each color to OrderDetails ************************** //
                if (totalBlue > 0)
                {
                    string colorProductSKU = "";
                    int productGroupID2 = productGroupId;
                    switch (productGroupId)
                    {
                        case ProductGroupIDConstants.Instadose1:
                        case ProductGroupIDConstants.Instadose1IMI:
                            colorProductSKU = "CASE CVR BLUE";
                            productGroupID2 = ProductGroupIDConstants.Instadose1IMI;
                            break;
                        case ProductGroupIDConstants.InstadosePlus:
                            colorProductSKU = "COLOR CAP BLUE";
                            break;
                        case ProductGroupIDConstants.InstadoseVue:
                            colorProductSKU = Instadose3Constants.CollarBlueProductSKU;
                            break;
                        case ProductGroupIDConstants.InstadoseVueBeta:
                            nocolorProductSKU = InstadoseVueBetaConstants.CollarBlueProductSKU;
                            break;
                        case ProductGroupIDConstants.Instadose2New:
                            colorProductSKU = "ID2BBUMP";
                            break;
                        default:
                            break;
                    }

                    int colorProductID = (from p in idc.Products where p.ProductSKU == colorProductSKU && p.ProductGroupID == productGroupID2 select p.ProductID).FirstOrDefault();
                    // Add the order details to the order.
                    // adding COLOR case cover to OrderDetails
                    OrderDetail NewCaseCVROrdDetail = new OrderDetail();
                    NewCaseCVROrdDetail.OrderID = pOrderID;
                    NewCaseCVROrdDetail.Price = 0;
                    NewCaseCVROrdDetail.Quantity = totalBlue;
                    NewCaseCVROrdDetail.OrderDetailDate = DateTime.Now;
                    NewCaseCVROrdDetail.ProductID = colorProductID;

                    idc.OrderDetails.InsertOnSubmit(NewCaseCVROrdDetail);
                }

                if (totalGrey > 0)
                {
                    string colorProductSKU = "";
                    int productGroupID2 = productGroupId;
                    switch (productGroupId)
                    {
                        case ProductGroupIDConstants.Instadose1:
                        case ProductGroupIDConstants.Instadose1IMI:
                            colorProductSKU = "";   // No grey color for ID1
                            productGroupID2 = ProductGroupIDConstants.Instadose1IMI;
                            break;
                        case ProductGroupIDConstants.InstadosePlus:
                            colorProductSKU = "COLOR CAP GREY";
                            break;
                        case ProductGroupIDConstants.InstadoseVue:
                            colorProductSKU = Instadose3Constants.CollarGreyProductSKU;
                            break;
                        case ProductGroupIDConstants.InstadoseVueBeta:
                            nocolorProductSKU = InstadoseVueBetaConstants.CollarGreyProductSKU;
                            break;
                        case ProductGroupIDConstants.Instadose2New:
                            colorProductSKU = "ID2GYBUMP";
                            break;
                        default:
                            break;
                    }

                    int colorProductID = (from p in idc.Products where p.ProductSKU == colorProductSKU && p.ProductGroupID == productGroupID2 select p.ProductID).FirstOrDefault();
                    // Add the order details to the order.
                    // adding COLOR case cover to OrderDetails
                    OrderDetail NewCaseCVROrdDetail = new OrderDetail();
                    NewCaseCVROrdDetail.OrderID = pOrderID;
                    NewCaseCVROrdDetail.Price = 0;
                    NewCaseCVROrdDetail.Quantity = totalGrey;
                    NewCaseCVROrdDetail.OrderDetailDate = DateTime.Now;
                    NewCaseCVROrdDetail.ProductID = colorProductID;

                    idc.OrderDetails.InsertOnSubmit(NewCaseCVROrdDetail);
                }

                if (totalGreen > 0)
                {
                    string colorProductSKU = "";
                    int productGroupID2 = productGroupId;
                    switch (productGroupId)
                    {
                        case ProductGroupIDConstants.Instadose1:
                        case ProductGroupIDConstants.Instadose1IMI:
                            colorProductSKU = "CASE CVR GREEN";
                            productGroupID2 = ProductGroupIDConstants.Instadose1IMI;
                            break;
                        case ProductGroupIDConstants.InstadosePlus:
                            colorProductSKU = "COLOR CAP GRN";
                            break;
                        case ProductGroupIDConstants.InstadoseVue:
                            colorProductSKU = Instadose3Constants.CollarGreenProductSKU;
                            break;
                        case ProductGroupIDConstants.InstadoseVueBeta:
                            nocolorProductSKU = InstadoseVueBetaConstants.CollarGreenProductSKU;
                            break;
                        case ProductGroupIDConstants.Instadose2New:
                            colorProductSKU = "ID2GBUMP";
                            break;
                        default:
                            break;
                    }

                    int colorProductID = (from p in idc.Products where p.ProductSKU == colorProductSKU && p.ProductGroupID == productGroupID2 select p.ProductID).FirstOrDefault();
                    // Add the order details to the order.
                    // adding COLOR case cover to OrderDetails
                    OrderDetail NewCaseCVROrdDetail = new OrderDetail();
                    NewCaseCVROrdDetail.OrderID = pOrderID;
                    NewCaseCVROrdDetail.Price = 0;
                    NewCaseCVROrdDetail.Quantity = totalGreen;
                    NewCaseCVROrdDetail.OrderDetailDate = DateTime.Now;
                    NewCaseCVROrdDetail.ProductID = colorProductID;

                    idc.OrderDetails.InsertOnSubmit(NewCaseCVROrdDetail);
                }

                if (totalOrange > 0)
                {
                    string colorProductSKU = "";
                    int productGroupID2 = productGroupId;
                    switch (productGroupId)
                    {
                        case ProductGroupIDConstants.Instadose1:
                        case ProductGroupIDConstants.Instadose1IMI:
                            colorProductSKU = "";   // No Orange color for ID1
                            productGroupID2 = ProductGroupIDConstants.Instadose1IMI;
                            break;
                        case ProductGroupIDConstants.InstadosePlus:
                            colorProductSKU = "COLOR CAP ORANG";
                            break;
                        case ProductGroupIDConstants.InstadoseVue:
                            colorProductSKU = Instadose3Constants.CollarOrangeProductSKU;
                            break;
                        case ProductGroupIDConstants.InstadoseVueBeta:
                            nocolorProductSKU = InstadoseVueBetaConstants.CollarOrangeProductSKU;
                            break;
                        case ProductGroupIDConstants.Instadose2New:
                            colorProductSKU = "ID2OBUMP";
                            productGroupID2 = 11;
                            break;
                        default:
                            break;
                    }

                    int colorProductID = (from p in idc.Products where p.ProductSKU == colorProductSKU && p.ProductGroupID == productGroupID2 select p.ProductID).FirstOrDefault();
                    // Add the order details to the order.
                    // adding COLOR case cover to OrderDetails
                    OrderDetail NewCaseCVROrdDetail = new OrderDetail();
                    NewCaseCVROrdDetail.OrderID = pOrderID;
                    NewCaseCVROrdDetail.Price = 0;
                    NewCaseCVROrdDetail.Quantity = totalOrange;
                    NewCaseCVROrdDetail.OrderDetailDate = DateTime.Now;
                    NewCaseCVROrdDetail.ProductID = colorProductID;

                    idc.OrderDetails.InsertOnSubmit(NewCaseCVROrdDetail);
                }

                if (totalPink > 0)
                {
                    string colorProductSKU = "";
                    int productGroupID2 = productGroupId;
                    switch (productGroupId)
                    {
                        case ProductGroupIDConstants.Instadose1:
                        case ProductGroupIDConstants.Instadose1IMI:
                            colorProductSKU = "CASE CVR PINK";
                            productGroupID2 = ProductGroupIDConstants.Instadose1IMI;
                            break;
                        case ProductGroupIDConstants.InstadosePlus:
                            colorProductSKU = "COLOR CAP PINK";
                            break;
                        case ProductGroupIDConstants.InstadoseVue:
                            colorProductSKU = Instadose3Constants.CollarPinkProductSKU;
                            break;
                        case ProductGroupIDConstants.InstadoseVueBeta:
                            nocolorProductSKU = InstadoseVueBetaConstants.CollarPinkProductSKU;
                            break;
                        case ProductGroupIDConstants.Instadose2New:
                            colorProductSKU = "ID2PBUMP";
                            break;
                        default:
                            break;
                    }

                    int colorProductID = (from p in idc.Products where p.ProductSKU == colorProductSKU && p.ProductGroupID == productGroupID2 select p.ProductID).FirstOrDefault();
                    // Add the order details to the order.
                    // adding COLOR case cover to OrderDetails
                    OrderDetail NewCaseCVROrdDetail = new OrderDetail();
                    NewCaseCVROrdDetail.OrderID = pOrderID;
                    NewCaseCVROrdDetail.Price = 0;
                    NewCaseCVROrdDetail.Quantity = totalPink;
                    NewCaseCVROrdDetail.OrderDetailDate = DateTime.Now;
                    NewCaseCVROrdDetail.ProductID = colorProductID;

                    idc.OrderDetails.InsertOnSubmit(NewCaseCVROrdDetail);
                }

                if (totalRed > 0)
                {
                    string colorProductSKU = "";
                    int productGroupID2 = productGroupId;
                    switch (productGroupId)
                    {
                        case ProductGroupIDConstants.Instadose1:
                        case ProductGroupIDConstants.Instadose1IMI:
                            colorProductSKU = "";   // No Red color for ID1
                            productGroupID2 = ProductGroupIDConstants.Instadose1IMI;
                            break;
                        case ProductGroupIDConstants.InstadosePlus:
                            colorProductSKU = "COLOR CAP RED";
                            break;
                        case ProductGroupIDConstants.InstadoseVue:
                            colorProductSKU = Instadose3Constants.CollarRedProductSKU;
                            break;
                        case ProductGroupIDConstants.InstadoseVueBeta:
                            nocolorProductSKU = InstadoseVueBetaConstants.CollarRedProductSKU;
                            break;
                        case ProductGroupIDConstants.Instadose2New:
                            colorProductSKU = "ID2RBUMP";
                            break;
                        default:
                            break;
                    }

                    int colorProductID = (from p in idc.Products where p.ProductSKU == colorProductSKU && p.ProductGroupID == productGroupID2 select p.ProductID).FirstOrDefault();
                    // Add the order details to the order.
                    // adding COLOR case cover to OrderDetails
                    OrderDetail NewCaseCVROrdDetail = new OrderDetail();
                    NewCaseCVROrdDetail.OrderID = pOrderID;
                    NewCaseCVROrdDetail.Price = 0;
                    NewCaseCVROrdDetail.Quantity = totalRed;
                    NewCaseCVROrdDetail.OrderDetailDate = DateTime.Now;
                    NewCaseCVROrdDetail.ProductID = colorProductID;

                    idc.OrderDetails.InsertOnSubmit(NewCaseCVROrdDetail);
                }

                if (totalBlack > 0)
                {
                    string colorProductSKU = "";
                    int productGroupID2 = productGroupId;
                    switch (productGroupId)
                    {
                        case ProductGroupIDConstants.Instadose1:
                        case ProductGroupIDConstants.Instadose1IMI:
                            colorProductSKU = "CASE CVR BLK";
                            productGroupID2 = ProductGroupIDConstants.Instadose1IMI;
                            break;
                        case ProductGroupIDConstants.InstadosePlus:
                            colorProductSKU = "";       // No Black color for ID+
                            break;
                        case ProductGroupIDConstants.Instadose2New:
                            colorProductSKU = "";       // No Black color for new ID2
                            break;
                        default:
                            break;
                    }

                    int colorProductID = (from p in idc.Products where p.ProductSKU == colorProductSKU && p.ProductGroupID == productGroupID2 select p.ProductID).FirstOrDefault();
                    // Add the order details to the order.
                    // adding COLOR case cover to OrderDetails
                    OrderDetail NewCaseCVROrdDetail = new OrderDetail();
                    NewCaseCVROrdDetail.OrderID = pOrderID;
                    NewCaseCVROrdDetail.Price = 0;
                    NewCaseCVROrdDetail.Quantity = totalBlack;
                    NewCaseCVROrdDetail.OrderDetailDate = DateTime.Now;
                    NewCaseCVROrdDetail.ProductID = colorProductID;

                    idc.OrderDetails.InsertOnSubmit(NewCaseCVROrdDetail);
                }

                if (totalSilver > 0)
                {
                    string colorProductSKU = "";
                    int productGroupID2 = productGroupId;
                    switch (productGroupId)
                    {
                        case ProductGroupIDConstants.Instadose1:
                        case ProductGroupIDConstants.Instadose1IMI:
                            colorProductSKU = "CASE CVR SLVR";
                            productGroupID2 = ProductGroupIDConstants.Instadose1IMI;
                            break;
                        case ProductGroupIDConstants.InstadosePlus:
                            colorProductSKU = "";       // No Silver color for ID+
                            break;
                        case ProductGroupIDConstants.Instadose2New:
                            colorProductSKU = "";       // No Silver color for new ID2
                            break;
                        default:
                            break;
                    }

                    int colorProductID = (from p in idc.Products where p.ProductSKU == colorProductSKU && p.ProductGroupID == productGroupID2 select p.ProductID).FirstOrDefault();
                    // Add the order details to the order.
                    // adding COLOR case cover to OrderDetails
                    OrderDetail NewCaseCVROrdDetail = new OrderDetail();
                    NewCaseCVROrdDetail.OrderID = pOrderID;
                    NewCaseCVROrdDetail.Price = 0;
                    NewCaseCVROrdDetail.Quantity = totalSilver;
                    NewCaseCVROrdDetail.OrderDetailDate = DateTime.Now;
                    NewCaseCVROrdDetail.ProductID = colorProductID;

                    idc.OrderDetails.InsertOnSubmit(NewCaseCVROrdDetail);
                }
            }

            idc.SubmitChanges();

            Basics.WriteLogEntry("New order detail for RMA Recall to OrderDetail table, OrderID=" + pOrderID.ToString(), pUserName,
              "Instadose.Processing.ProcessRecall.CreateMultipleRecallOrderDetail", Basics.MessageLogType.Information);

            return true;

        }
        catch (Exception ex)
        {
            pError = ex.Message;
            Basics.WriteLogEntry(ex.Message, pUserName,
                "Instadose.Processing.ProcessRecall.CreateMultipleRecallOrderDetail", Basics.MessageLogType.Critical);
            return false;
        }
    }
    private string GetAppSettings(string pAppKey)
    {
        var mySetting = (from AppSet in idc.AppSettings where AppSet.AppKey == pAppKey select AppSet).FirstOrDefault();
        return (mySetting != null) ? mySetting.Value : "";
    }

    /// <summary>
    /// Choose free shipping method for Recall order
    /// </summary>
    /// <param name="pAccountID"></param>
    /// <returns></returns>
    private int GetShippingMethodID(int pAccountID)
    {
        int preferShippingCarrierID = 0;
        try
        {
            if (Convert.ToBoolean(GetAppSettings("MalvernIntegration")))
            {
                string preferShippingCarrier = "";
                preferShippingCarrier = (from a in idc.Accounts where a.AccountID == pAccountID select a.ShippingCarrier).FirstOrDefault();

                if (string.IsNullOrEmpty(preferShippingCarrier)) preferShippingCarrier = "";

                switch (preferShippingCarrier.ToUpper())
                {
                    case "":
                        preferShippingCarrierID = (from a in idc.ShippingMethods where a.ShippingMethodDesc == "Free Shipping Ground" select a.ShippingMethodID).FirstOrDefault();
                        break;
                    case "DHL GLOBAL MAIL":
                        preferShippingCarrierID = (from a in idc.ShippingMethods where a.ShippingMethodDesc == "DHL Free Shipping" select a.ShippingMethodID).FirstOrDefault();
                        break;
                    case "DHL INTERNATIONAL":
                        preferShippingCarrierID = (from a in idc.ShippingMethods where a.ShippingMethodDesc == "DHL International Free Shipping" select a.ShippingMethodID).FirstOrDefault();
                        break;
                    case "FEDEX":
                        preferShippingCarrierID = (from a in idc.ShippingMethods where a.ShippingMethodDesc == "FedEx Free Shipping" select a.ShippingMethodID).FirstOrDefault();
                        break;
                    case "UPS":
                        preferShippingCarrierID = (from a in idc.ShippingMethods where a.ShippingMethodDesc == "UPS Free Shipping" select a.ShippingMethodID).FirstOrDefault();
                        break;
                    case "US Postal Services":
                        preferShippingCarrierID = (from a in idc.ShippingMethods where a.ShippingMethodDesc == "USPS Free Shipping" select a.ShippingMethodID).FirstOrDefault();
                        break;
                    default:
                        preferShippingCarrierID = (from a in idc.ShippingMethods where a.ShippingMethodDesc == "Free Shipping Ground" select a.ShippingMethodID).FirstOrDefault();
                        break;
                }
            }
            else
            {
                preferShippingCarrierID = 11; // Old FedEx Free shipping        
            }
        }
        catch (Exception ex)
        {
        }

        return preferShippingCarrierID;
    }

    private void EnableOrDisableOtherLocationFields(bool isEnabled)
    {
        txtLocationID.Enabled = isEnabled;
        txtAddrContactName.Enabled = isEnabled;
        txtAddrAddress1.Enabled = isEnabled;
        txtAddrAddress2.Enabled = isEnabled;
        txtAddrCity.Enabled = isEnabled;
        txtAddrZipCode.Enabled = isEnabled;
        txtAddrPhone.Enabled = isEnabled;
        ddlAddrCountry.Enabled = isEnabled;
        ddlAddrState.Enabled = isEnabled;
        chkBoxAddrDisableVerification.Enabled = isEnabled;
    }

    /// <summary>
    ///  Send an existing order from the business app to MAS200/MAS90 SQL tables.
    /// </summary>
    /// <param name="OrderId"></param>    
    /// <returns></returns>
    public bool SendRecallOrderToMAS(int OrderId, int Userid, int Locationid, ref string pError)
    {
        try
        {
            pError = "";

            // Grab the order header
            Order order = (from o in idc.Orders
                           where o.OrderID == OrderId
                           select o).FirstOrDefault();

            // Grab the details of the order
            IQueryable<OrderDetail> order_detail = (from od in idc.OrderDetails
                                                    where od.OrderID == OrderId
                                                    select od).AsQueryable();
            // Grab the account
            Account account = (from a in idc.Accounts
                               where a.AccountID == order.AccountID
                               select a).FirstOrDefault();

            // Grab default location
            var location = (from l in idc.Locations
                            where l.AccountID == account.AccountID && l.IsDefaultLocation == true
                            select l).FirstOrDefault();

            // Select the administrator primary email address on the account.
            var AdminEmail = (from u in idc.Users
                              where u.UserID == account.AccountAdminID
                              select u.Email).FirstOrDefault();

            // grab pick location address
            var EPlocationShipAddress = (from l in idc.Locations
                                         where l.LocationID == Locationid
                                         select l).FirstOrDefault();

            // Grab Users Address 
            var EPUserShipAddress = (from u in idc.Users
                                     where u.UserID == Userid
                                     select u).FirstOrDefault();

            String paymentType = "CC";

            //if (order.PaymentMethod.Value == 0) // 0 indicates a PayPal payment
            //{
            //    // Set the payment type to a credit card.
            //    paymentType = "CC";

            //    // If may not find a payment, if the credit card process failed.
            //    try
            //    {
            //        // Select the payment transaction code in the payments table.
            //        paymentTypeRefNo = (from p in busData.Payments
            //                            where p.PaymentsID == order.PaymentID
            //                            select p.TranCode).FirstOrDefault();
            //    }
            //    catch
            //    {
            //        // If payment not found, add a comment.
            //        Comments = "Payment not captured.";
            //    }
            //}

            //Basics.WriteLogEntry("Preparing to send order to MAS. OrderID=" + OrderId.ToString(), UserName,
            //    "Instadose.Processing.ProcessRecall.SendRecallOrderToMAS", Basics.MessageLogType.Information);


            //Send to MAS SO SalesOrderDetails whatever records in orderdetails
            //EXCLUDE CREDIT MEMOS order 
            foreach (OrderDetail od in order_detail)
            {
                string mySKU = od.Product.ProductSKU;

                ToMas_SO_SalesOrderDetail newSalesOrderDetail = new ToMas_SO_SalesOrderDetail()
                {
                    SalesOrderNo = OrderId.ToString().PadLeft(7, '0'),
                    ItemCode = mySKU,
                    ItemType = " ",
                    WarehouseCode = "INV",
                    QuantityOrderedOriginal = od.Quantity,
                    OriginalUnitPrice = od.Price,
                    UnitCost = od.Price
                };

                this.mdc.ToMas_SO_SalesOrderDetails.InsertOnSubmit(newSalesOrderDetail);
                // Send to MAS
                this.mdc.SubmitChanges();
            }

            Basics.WriteLogEntry("The details have been sent to MAS. OrderID=" + OrderId.ToString(), UserName,
                "Instadose.Processing.ProcessRecall.SendRecallOrderToMAS", Basics.MessageLogType.Information);

            /* SET THE MAS CUSTOMER */
            ToMASAR_Customer newCustomer = new ToMASAR_Customer()
            {
                ARDivisionNo = "00",
                CustomerNo = account.AccountID.ToString().PadLeft(7, '0'),
                CustomerName = CleanString(account.CompanyName, 30),
                AddressLine1 = CleanString(location.BillingAddress1, 30),
                AddressLine2 = CleanString(location.BillingAddress2, 30),
                AddressLine3 = CleanString(location.BillingAddress3, 30),
                City = CleanString(location.BillingCity, 20),
                State = (location != null && location.BillingStateID != null) ? (from a in idc.States where a.StateID == location.BillingStateID select a.StateAbbrev).FirstOrDefault() : string.Empty,
                ZipCode = CleanString(location.BillingPostalCode, 10),
                CountryCode = (location != null && location.BillingCountryID != null) ? (from a in idc.Countries where a.CountryID == location.BillingCountryID select a.CountryCode).FirstOrDefault() : string.Empty,
                TelephoneNo = CleanString(location.BillingTelephone, 17),
                TelephoneExt = String.Empty,
                FaxNo = CleanString(location.BillingFax, 17),
                EmailAddress = CleanString(AdminEmail, 50),
                CustomerType = (from a in idc.CustomerTypes where a.CustomerTypeID == account.CustomerTypeID select a.CustomerTypeCode).FirstOrDefault(),
                SalespersonNo = CleanString(account.ReferralCode, 4),
                SalespersonDivisionNo = "00",
                Currency_Code = CleanString(order.CurrencyCode, 3)
            };
            this.mdc.ToMASAR_Customers.InsertOnSubmit(newCustomer);
            // Send to MAS
            this.mdc.SubmitChanges();

            Basics.WriteLogEntry("The customer record has been sent to MAS. OrderID=" + OrderId.ToString(), UserName,
                "Instadose.Processing.ProcessRecall.SendRecallOrderToMAS", Basics.MessageLogType.Information);

            /* SET THE MAS ORDER HEADER */
            ToMAS_SO_SalesOrderHeader soh = new ToMAS_SO_SalesOrderHeader();
            soh.ARDivisionNo = "00";
            soh.OrderStatus = "";
            soh.SalesOrderNo = OrderId.ToString().PadLeft(7, '0');
            soh.FOB = OrderId.ToString();
            soh.OrderDate = DateTime.Today.ToString("MM/dd/yy");
            soh.CustomerNo = account.AccountID.ToString().PadLeft(7, '0');
            soh.CustomerPONo = CleanString(order.PONumber, 15);
            //soh.CustomerPONo = "REPLACEDIS";
            soh.BillToName = CleanString(location.BillingCompany, 30);
            soh.BillToAddress1 = CleanString(location.BillingAddress1, 30);
            soh.BillToAddress2 = CleanString(location.BillingAddress2, 30);
            soh.BillToAddress3 = CleanString(location.BillingAddress3, 30);
            soh.BillToCity = CleanString(location.BillingCity, 20);
            soh.BillToState = (location != null && location.BillingStateID != null) ? (from a in idc.States where a.StateID == location.BillingStateID select a.StateAbbrev).FirstOrDefault() : string.Empty;
            soh.BillToZipCode = CleanString(location.BillingPostalCode, 10);
            soh.BillToCountryCode = (location != null && location.BillingCountryID != null) ? (from a in idc.Countries where a.CountryID == location.BillingCountryID select a.CountryCode).FirstOrDefault() : string.Empty;

            // check if enterprise user address is empty?
            if (EPUserShipAddress != null)
            {
                soh.ShipToName = CleanString(EPUserShipAddress.FirstName + " " + EPUserShipAddress.LastName, 30);
                soh.ShipToAddress1 = CleanString(EPUserShipAddress.Address1, 30);
                soh.ShipToAddress2 = CleanString(EPUserShipAddress.Address2, 30);
                soh.ShipToAddress3 = CleanString(EPUserShipAddress.Address3, 30);
                soh.ShipToCity = CleanString(EPUserShipAddress.City, 20);
                soh.ShipToState = (EPUserShipAddress != null && EPUserShipAddress.StateID != null) ? (from a in idc.States where a.StateID == EPUserShipAddress.StateID select a.StateAbbrev).FirstOrDefault() : string.Empty;
                soh.ShipToZipCode = CleanString(EPUserShipAddress.PostalCode, 10);
                soh.ShipToCountryCode = (EPUserShipAddress != null && EPUserShipAddress.CountryID != null) ? (from a in idc.Countries where a.CountryID == EPUserShipAddress.CountryID select a.CountryCode).FirstOrDefault() : string.Empty;
            }
            else if (EPlocationShipAddress != null)
            {
                soh.ShipToName = CleanString(EPlocationShipAddress.ShippingFirstName + " " + EPlocationShipAddress.ShippingLastName, 30);
                soh.ShipToAddress1 = CleanString(EPlocationShipAddress.ShippingAddress1, 30);
                soh.ShipToAddress2 = CleanString(EPlocationShipAddress.ShippingAddress2, 30);
                soh.ShipToAddress3 = CleanString(EPlocationShipAddress.ShippingAddress3, 30);
                soh.ShipToCity = CleanString(EPlocationShipAddress.ShippingCity, 20);
                soh.ShipToState = (EPlocationShipAddress != null && EPlocationShipAddress.ShippingStateID != null) ? (from a in idc.States where a.StateID == EPlocationShipAddress.ShippingStateID select a.StateAbbrev).FirstOrDefault() : string.Empty;
                soh.ShipToZipCode = CleanString(EPlocationShipAddress.ShippingPostalCode, 10);
                soh.ShipToCountryCode = (EPlocationShipAddress != null && EPlocationShipAddress.ShippingCountryID != null) ? (from a in idc.Countries where a.CountryID == EPlocationShipAddress.ShippingCountryID select a.CountryCode).FirstOrDefault() : string.Empty;
            }
            else
            {
                // use default shipping information
                soh.ShipToName = CleanString(location.ShippingFirstName + " " + location.ShippingLastName, 30);
                soh.ShipToAddress1 = CleanString(location.ShippingAddress1, 30);
                soh.ShipToAddress2 = CleanString(location.ShippingAddress2, 30);
                soh.ShipToAddress3 = CleanString(location.ShippingAddress3, 30);
                soh.ShipToCity = CleanString(location.ShippingCity, 20);
                soh.ShipToState = (location != null && location.ShippingStateID != null) ? (from a in idc.States where a.StateID == location.ShippingStateID select a.StateAbbrev).FirstOrDefault() : string.Empty;
                soh.ShipToZipCode = CleanString(location.ShippingPostalCode, 10);
                soh.ShipToCountryCode = (location != null) ? (from a in idc.Countries where a.CountryID == location.ShippingCountryID select a.CountryCode).FirstOrDefault() : string.Empty;
            }

            soh.WarehouseCode = "INV";
            soh.Comment = "REPLACEDIS";  //REPLACEDIS
            soh.TaxSchedule = Basics.GetSetting("TaxSchedule");
            soh.TermsCode = "95";  // only for RMA recall
            soh.PaymentType = CleanString(paymentType, 5);
            //soh.PaymentType = "CC";
            soh.OtherPaymentTypeRefNo = "RMA ORDER";
            //soh.OtherPaymentTypeRefNo = CleanString(paymentTypeRefNo, 24);
            soh.Weight = 0;
            soh.TaxableAmt = 0;
            soh.NonTaxableAmt = 0;
            soh.FreightAmt = 0;
            soh.DepositAmt = 0;
            soh.SalespersonNo = CleanString(account.ReferralCode, 4);
            soh.SalespersonDivisionNo = "00";
            soh.Currency_Code = CleanString(order.CurrencyCode, 3);

            Basics.WriteLogEntry("The order header record is about to send. OrderID=" + OrderId.ToString(), UserName,
                "Instadose.Processing.ProcessRecall.SendRecallOrderToMAS", Basics.MessageLogType.Information);

            this.mdc.ToMAS_SO_SalesOrderHeaders.InsertOnSubmit(soh);

            // Send to MAS
            this.mdc.SubmitChanges();

            Basics.WriteLogEntry("The order header record has been sent to MAS. OrderID=" + OrderId.ToString(), UserName,
                "Instadose.Processing.ProcessRecall.SendRecallOrderToMAS", Basics.MessageLogType.Information);

            return true;
        }
        catch (Exception ex)
        {
            Basics.WriteLogEntry(ex.Message, UserName,
                "Instadose.Processing.ProcessRecall.SendRecallOrderToMAS ", Basics.MessageLogType.Critical);
            pError = ex.ToString();
            return false;
        }

    }

    /// <summary>
    /// Clean a string and remove special characters to ensure the data can be stored correctly for MAS.
    /// </summary>
    /// <param name="s">String to clean.</param>
    /// <param name="MaxLength">Maximum length of the string.</param>
    /// <returns>Returns a cleaned and trimmed string.</returns>
    private String CleanString(String s, int MaxLength)
    {

        String newString = "";
        if (s != null)
            newString = s.Replace("'", "");

        if (newString.Length > MaxLength)
            newString = newString.Substring(0, MaxLength);

        return newString;
    }

    /// <summary>
    /// Trim the string by removing newline/enter characters.
    /// </summary>
    /// <param name="s">String to trim.</param>
    /// <returns>Returns a trimmed string.</returns>
    private String TrimNewLineString(String s)
    {

        String newString = "";
        if (s != null)
            newString = s.Replace("\r\n", " ");

        return newString;
    }
    protected void rbtnOtherLocation_CheckedChanged(object sender, EventArgs e)
    {
        SetOtherLocationVisibility(true);
        // Initialize AddAddress 
        SetLocationAddress("0", "", "", "", "", "", "", "", "0", "", "1", false, false);
    }
    protected void rbtnUserAddress_CheckedChanged(object sender, EventArgs e)
    {
        SetOtherLocationVisibility(false);
    }
    protected void rbtnLocationAddress_CheckedChanged(object sender, EventArgs e)
    {
        SetOtherLocationVisibility(false);
    }
    protected void rbtnMainAddress_CheckedChanged(object sender, EventArgs e)
    {
        SetOtherLocationVisibility(false);
    }
    private void SelectShippingRadioButton(RadioButton pCheckedRadioButton)
    {
        if (pCheckedRadioButton == this.rbtnMainAddress)
        {
            this.rbtnMainAddress.Checked = true;
            this.rbtnUserAddress.Checked = false;
            this.rbtnLocationAddress.Checked = false;
            this.rbtnOtherLocation.Checked = false;
        }

        if (pCheckedRadioButton == this.rbtnUserAddress)
        {
            this.rbtnMainAddress.Checked = false;
            this.rbtnUserAddress.Checked = true;
            this.rbtnLocationAddress.Checked = false;
            this.rbtnOtherLocation.Checked = false;
        }

        if (pCheckedRadioButton == this.rbtnLocationAddress)
        {
            this.rbtnMainAddress.Checked = false;
            this.rbtnUserAddress.Checked = false;
            this.rbtnLocationAddress.Checked = true;
            this.rbtnOtherLocation.Checked = false;
        }

        if (pCheckedRadioButton == this.rbtnOtherLocation)
        {
            this.rbtnMainAddress.Checked = false;
            this.rbtnUserAddress.Checked = false;
            this.rbtnLocationAddress.Checked = false;
            this.rbtnOtherLocation.Checked = true;
        }
    }
    private void SetDisplaySingleFlowDeviceInfo(bool isSingleFlow)
    {
        this.SingleFlowRecall.Visible = isSingleFlow;
        this.MultipleFlowRecall.Visible = !isSingleFlow;
        this.ddlLocation.Enabled = !isSingleFlow;
        this.ddlProduct.Enabled = !isSingleFlow;
    }
    private void SetOtherLocationVisibility(bool pVisibility)
    {
        this.btnExistingLocations.Visible = pVisibility;
        this.AddAddress.Visible = pVisibility;
        this.btnNewLocation.Visible = pVisibility;

        if (pVisibility)
        {
            this.btnNewLocation.Visible = (bool)ViewState["IsGDSAccount"];
            this.AddressVerifyRow.Visible = (bool)ViewState["IsGDSAccount"];
        }
    }
    private string GetHistoryAssignCaseCover(string pSerialNo)
    {
        var DevProduct = (from dc in idc.DeviceColorizations
                          join d in idc.DeviceInventories on dc.DeviceID equals d.DeviceID
                          join p in idc.Products on dc.CaseCoverProductID equals p.ProductID
                          where d.SerialNo == pSerialNo
                          orderby dc.AddDate descending
                          select p).FirstOrDefault();
        if (DevProduct != null)
            return DevProduct.ProductSKU;
        else
            return "";
    }
    private int SeekCaseCoverProductID(string pSerialNo, int pProductGroupID)
    {
        if (pProductGroupID == 2 || pProductGroupID == 5 || pProductGroupID == 6 || pProductGroupID == 21) // no case cover
        {
            this.ddlCaseCover.Enabled = false;
            return 0;
        }
        else
        {
            // Matching color based upon recalled Selmic or IMI device
            string caseCVRProductSKU = "";

            // Grab the account and product info
            var DInfo = (from a in idc.DeviceInventories
                         join b in idc.AccountDevices
                           on a.DeviceID equals b.DeviceID
                         join c in idc.Accounts
                           on b.AccountID equals c.AccountID
                         where a.SerialNo == pSerialNo && b.CurrentDeviceAssign == true
                         select new { a, b, c, }).FirstOrDefault();

            int accountID = 0;
            if (DInfo != null)
            {
                accountID = DInfo.c.AccountID;
            }

            switch (DInfo.a.Product.ProductSKU.ToUpper())
            {
                case "INSTA10BLU":
                    caseCVRProductSKU = "CASE CVR BLUE";
                    break;
                case "INSTA10BLK":
                    caseCVRProductSKU = "CASE CVR BLK";
                    break;
                case "INSTA10SLV":
                    caseCVRProductSKU = "CASE CVR SLVR";
                    break;
                case "INSTA10GRN":
                    caseCVRProductSKU = "CASE CVR GREEN";
                    break;
                case "INSTA10PNK":
                    caseCVRProductSKU = "CASE CVR PINK";
                    break;
                case "INSTA10":
                case "INSTA10-B":
                case "INSTA PLUS":
                case "INSTA ID2":
                case "INSTA VUE":
                    caseCVRProductSKU = GetHistoryAssignCaseCover(pSerialNo);
                    break;
                
            }

            if (caseCVRProductSKU != "")
            {
                if (pProductGroupID == 1) pProductGroupID = 9;
                // TDO, 02/25/2019. Allow to change color for ID1 also
                //this.ddlCaseCover.Enabled = false;                
                var prod = (from p in idc.Products where p.ProductSKU == caseCVRProductSKU && p.ProductGroupID == pProductGroupID select p).FirstOrDefault();
                int prodID = prod.ProductID;

                //Not Instadose 1, check if current color is gray
                if (pProductGroupID != 1 && pProductGroupID != 9 && (caseCVRProductSKU.ToUpper().Contains("GREY") || caseCVRProductSKU.ToUpper().Contains("GRAY") 
                    || prod.Color.ToUpper().Contains("GREY") || prod.Color.ToUpper().Contains("GRAY")))
                {
                    // Check if latest record in userdevices (active or inactive) for the account is in color gray, return prodID = 0 to default to Blue
                    var currentDeviceColorization = (from ud in idc.UserDevices
                                                     join u in idc.Users
                                                     on ud.UserID equals u.UserID
                                                     join di in idc.DeviceInventories
                                                     on ud.DeviceID equals di.DeviceID
                                                     join dc in idc.DeviceColorizations
                                                     on di.DeviceID equals dc.DeviceID
                                                     where di.SerialNo == pSerialNo && u.AccountID == accountID
                                                     orderby ud.AssignmentDate descending
                                                     select new { dc }
                                                     ).FirstOrDefault();

                    if (currentDeviceColorization != null && currentDeviceColorization.dc.CaseCoverProductID == prodID)
                    {
                        var blueProduct = GetBlueCaseCoverProductID(pProductGroupID);
                        return blueProduct.ProductID;
                    }
                }

                return prodID;
            }
            else
            {
                //this.ddlCaseCover.Enabled = true;
                return 0;
            }
        }
    }

    private List<DeviceColorization> GetDeviceColorization(List<string> badges, int accountID)
    {
        // Check if latest record in userdevices (active or inactive) for the account is in color gray, return prodID = 0 to default to Blue
        int batchSize = 1500;

        List<string> allBadges = badges.ToList(); // Ensure it's a list
        List<DeviceColorization> finalResults = new List<DeviceColorization>();

        for (int i = 0; i < allBadges.Count; i += batchSize)
        {
            var batch = allBadges.Skip(i).Take(batchSize).ToList(); // Takes 2000, then whatever is left

            var batchResults = (from ud in idc.UserDevices
                                join u in idc.Users on ud.UserID equals u.UserID
                                join di in idc.DeviceInventories on ud.DeviceID equals di.DeviceID
                                join dc in idc.DeviceColorizations on di.DeviceID equals dc.DeviceID
                                where batch.Contains(di.SerialNo) && u.AccountID == accountID
                                orderby ud.AssignmentDate descending
                                group dc by di.SerialNo into g
                                select g.FirstOrDefault())
                                .ToList();

            finalResults.AddRange(batchResults); // Add results to the final list
        }

        return finalResults; ;
    }

    private Product GetBlueCaseCoverProductID(int pProductGroupID)
    {
        Product blueProductID = (from p in idc.Products where p.ProductSKU.ToUpper().Contains("BLUE") && p.ProductGroupID == pProductGroupID select p).FirstOrDefault();
        return blueProductID;
    }

    protected void ddlCaseCover_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (btnContinue.Visible && (ddlCaseCover.SelectedItem.Text.ToUpper() == "GREY" || ddlCaseCover.SelectedItem.Text.ToUpper() == "GRAY") && (!String.IsNullOrEmpty(lblAssignedUser.Text) && lblAssignedUser.Text.ToUpper() != "NOT ASSIGNED"))
        {
            VisibleErrors("Cannot assign gray color to the wearer");
            ddlCaseCover.SelectedValue = HidCaseCover.Value;
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
        this.errorMsg.InnerHtml = error;
        this.errors.Visible = true;
    }
    private void InvisibleSuccess()
    {
        // Reset submission form error message      
        this.successMsg.InnerHtml = "";
        this.success.Visible = false;
    }
    private void VisibleSuccess(string success)
    {
        this.successMsg.InnerHtml = success;
        this.success.Visible = true;
    }
    private void FillGDSRMAWearer(string pSerialNumber, RMAWearerInfo rmaWearerInfo)
    {
        var wearerInfo = (from a in idc.DeviceInventories
                          join u in idc.UserDevices on a.DeviceID equals u.DeviceID
                          join usr in idc.Users on u.UserID equals usr.UserID
                          where a.SerialNo == pSerialNumber
                              && u.Active == true
                          select new { u, usr }).FirstOrDefault();

        if (wearerInfo != null)
        {
            string bodyRegionName = (from a in idc.BodyRegions where a.BodyRegionID == wearerInfo.u.BodyRegionID select a.BodyRegionName).FirstOrDefault();

            rmaWearerInfo.WearerID = wearerInfo.usr.GDSWearerID.Value;
            rmaWearerInfo.BodyRegion = bodyRegionName;
        }
        else
        {
            rmaWearerInfo.WearerID = 0;
            rmaWearerInfo.BodyRegion = "";
        }
    }
    private void FillGDSInstaOrderDetailItem(string pSerialNumber, int coverProductID, bool pShipWOAssignUser, InstadoseOrderDetailItem myOrderDetailItem)
    {
        string sku = "";
        string badgeType = "";

        int productGroupId;
        int.TryParse(ddlProduct.SelectedValue, out productGroupId);

        int productID = 0;

        if (productGroupId == ProductGroupIDConstants.Instadose2 || productGroupId == ProductGroupIDConstants.InstaLink || productGroupId == ProductGroupIDConstants.InstaLinkUSB || productGroupId == ProductGroupIDConstants.InstaLink3) // No need case cover
        {
            Product product = null;
            if (productGroupId == ProductGroupIDConstants.Instadose2)
            {
                product = (from di in idc.DeviceInventories
                           join p in idc.Products on di.ProductID equals p.ProductID
                           where di.SerialNo == pSerialNumber
                           select p).FirstOrDefault();
                badgeType = "32";
            }
            else
            {
                product = (from pi in idc.ProductInventories
                           join p in idc.Products on pi.ProductID equals p.ProductID
                           where pi.SerialNo == pSerialNumber
                           select p).FirstOrDefault();

                badgeType = GetBadgeTypeByProductGroupId(productGroupId).ToString();
                //if (productGroupId == ProductGroupIDConstants.InstaLink) badgeType = "34";
                //if (productGroupId == ProductGroupIDConstants.InstaLinkUSB) badgeType = "33";
            }

            sku = product.ProductSKU;
            productID = product.ProductID;
        }
        else
        {
            switch (productGroupId)
            {
                case ProductGroupIDConstants.Instadose1:
                case ProductGroupIDConstants.Instadose1IMI:
                    sku = "INSTA10-B";
                    break;
                case ProductGroupIDConstants.InstadosePlus:
                    sku = "INSTA PLUS";
                    badgeType = "37";
                    break;
                case ProductGroupIDConstants.InstadoseVue:
                    sku = Instadose3Constants.ProductSKUINSTAID3;
                    badgeType = Instadose3Constants.GDSBadgeType;
                    break;
                case ProductGroupIDConstants.InstadoseVueBeta:
                    sku = InstadoseVueBetaConstants.ProductSKUINSTAIDVueB;
                    badgeType = InstadoseVueBetaConstants.GDSBadgeType;
                    break;
                case ProductGroupIDConstants.Instadose2New:
                    sku = "INSTA ID2";
                    badgeType = "38";
                    break;
                default:
                    break;
            }
        }

        string color = "No Color";

        Product colorProduct = (from p in idc.Products where p.ProductID == coverProductID select p).FirstOrDefault();
        if (colorProduct != null)
            color = colorProduct.Color;

        color = color.ToUpper();        

        if (!pShipWOAssignUser)
        {
            var wearerInfo = (from a in idc.DeviceInventories
                              join u in idc.UserDevices on a.DeviceID equals u.DeviceID
                              join usr in idc.Users on u.UserID equals usr.UserID
                              where a.SerialNo == pSerialNumber
                                  && u.Active == true
                              select new { u, usr }).FirstOrDefault();

            if (wearerInfo != null)
            {
                string bodyRegionAbbrev = (from a in udc.BodyRegions where a.InstaBodyRegionID == wearerInfo.u.BodyRegionID select a.BodyRegionAbbrev).FirstOrDefault();

                myOrderDetailItem.WearerNo = wearerInfo.usr.GDSWearerID;
                myOrderDetailItem.BodyRegion = bodyRegionAbbrev;
                myOrderDetailItem.UserID = wearerInfo.usr.UserID;
            }
            else
            {
                myOrderDetailItem.WearerNo = null;
                myOrderDetailItem.BodyRegion = "UNA";
                myOrderDetailItem.UserID = null;
            }
        }
        else
        {
            myOrderDetailItem.WearerNo = null;
            myOrderDetailItem.BodyRegion = "UNA";
            myOrderDetailItem.UserID = null;
        }

        myOrderDetailItem.ShipWOAssignedUser = pShipWOAssignUser;
        myOrderDetailItem.ProductID = coverProductID > 0 ? coverProductID : productID;
        myOrderDetailItem.SerialNo = pSerialNumber;
        myOrderDetailItem.BadgeType = badgeType;
        myOrderDetailItem.ICCPO = "";
        myOrderDetailItem.SKU = sku;
        myOrderDetailItem.Color = color;
        myOrderDetailItem.Qty = 1;

    }
    private void FillGDSInstaOrderDetailItemAndRMAWearerLists(List<InstadoseOrderDetailItem> pOrderDetailItemList, List<RMAWearerInfo> pRMAWearerInfoList)
    {
        // Load pRMAWearerInfoList & pOrderDetailItemList
        foreach (GridViewRow row in this.gv_DeviceList.Rows)
        {
            CheckBox chkBoxSelect = (CheckBox)row.FindControl("chkbxSelectDeviceList");

            if (chkBoxSelect.Checked)
            {
                // ------------------ Load pRMAWearerInfoList ------------------//
                HiddenField HidAssignedWearerID = (HiddenField)row.FindControl("HidAssignedWearerID");
                string bodyRegionName = row.Cells[2].Text.Replace("\n", "").ToString().Trim();

                

                RMAWearerInfo rmaWearerInfo = new RMAWearerInfo();

                if (string.IsNullOrEmpty(HidAssignedWearerID.Value))
                {
                    rmaWearerInfo.WearerID = 0;
                    rmaWearerInfo.BodyRegion = "";
                }
                else
                {
                    rmaWearerInfo.WearerID = int.Parse(HidAssignedWearerID.Value);
                    rmaWearerInfo.BodyRegion = bodyRegionName;
                }
                pRMAWearerInfoList.Add(rmaWearerInfo);
                // ------------------ Load pRMAWearerInfoList ------------------//

                // ------------------ Load pOrderDetailItemList ----------------//
                DropDownList ddlCaseCoverProductID = (DropDownList)row.FindControl("ddlColor");
                CheckBox chkbxShipWOAssigned = (CheckBox)row.FindControl("chkbxShipWOAssigned");
                string sn = row.Cells[3].Text.Replace("\n", "").ToString().Trim();

                InstadoseOrderDetailItem myOrderDetailItem = new InstadoseOrderDetailItem();
                FillGDSInstaOrderDetailItem(sn, int.Parse(ddlCaseCoverProductID.SelectedValue), chkbxShipWOAssigned.Checked, myOrderDetailItem);
                pOrderDetailItemList.Add(myOrderDetailItem);
                // ------------------ Load pOrderDetailItemList ----------------//
            }
        }
    }
    protected void btnAddressVerify_Click(object sender, EventArgs e)
    {
        if (String.IsNullOrEmpty(this.txtAddrAddress1.Text.Trim()))
        {
            this.VisibleErrors("Address is required.");
            return;
        }

        if (String.IsNullOrEmpty(this.txtAddrCity.Text.Trim()))
        {
            this.VisibleErrors("City is required.");
            return;
        }

        if (this.ddlAddrState.SelectedValue.ToString() == "0")
        {
            this.VisibleErrors("State is required.");
            return;
        }

        if (String.IsNullOrEmpty(this.txtAddrZipCode.Text.Trim()))
        {
            this.VisibleErrors("Zip Code is required.");
            return;
        }

        State selState = (from s in idc.States where s.StateID == int.Parse(ddlAddrState.SelectedValue) select s).FirstOrDefault();

        GAddress address = new GAddress()
        {
            Address1 = txtAddrAddress1.Text.Trim(),
            Address2 = txtAddrAddress2.Text.Trim(),
            City = txtAddrCity.Text.Trim(),
            State = selState.StateAbbrev,
            ZipCode = txtAddrZipCode.Text.Trim()
        };

        AddressRequests addReqs = new AddressRequests();
        List<AddressKeyFormat> response = addReqs.ValidateAddress(address);

        List<GAddress> addresses = new List<GAddress>();

        foreach (AddressKeyFormat ct in response)
        {
            var suggestedAddress = new GAddress()
            {
                Address1 = ct.AddressLine[0],
                Address2 = (ct.AddressLine.Length > 1) ? ct.AddressLine[1] : address.Address2,
                City = ct.PoliticalDivision2,
                State = ct.PoliticalDivision1,
                ZipCode = ct.PostcodePrimaryLow + "-" + ct.PostcodeExtendedLow
            };
            suggestedAddress.NumSharedAddr = (suggestedAddress.Address1.Split(' ')[0].Contains("-")) ? 1 : 0;
            addresses.Add(suggestedAddress);
        }
        addresses = addresses.OrderBy(a => a.NumSharedAddr).ThenBy(a => a.Address1).ToList();

        rdoEnteredAddress.Text = AssembleFullAddress(address);

        rdoVerifiedAddressList.Items.Clear();
        foreach (GAddress iterAddress in addresses)
        {
            string fullAddress = AssembleFullAddress(iterAddress);

            ListItem addressItem = new ListItem(fullAddress);

            if (iterAddress.NumSharedAddr == 1)
            {
                addressItem.Enabled = false;
                addressItem.Attributes.Add("Title", "Address contains a range, please update the address and verify again.");
            }
            else
            {
                addressItem.Enabled = true;
            }

            rdoVerifiedAddressList.Items.Add(addressItem);
        }

        if (addresses.Count > 0)
        {
            rdoVerifiedAddressList.SelectedIndex = 0;
            rdoEnteredAddress.Checked = false;
        }
        else
        {
            rdoEnteredAddress.Checked = true;
            rdoVerifiedAddressList.SelectedIndex = -1;
        }

        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('addressCorrectionDialog')", true);
    }
    private string AssembleFullAddress(GAddress address)
    {
        string commaString = ", ";
        string newline = "<br>";
        string fullAddress = "";
        fullAddress += address.Address1.Length > 0 ? address.Address1 : "";
        fullAddress += address.Address2.Length > 0 ? commaString + address.Address2 : "";
        fullAddress += address.City.Length > 0 ? commaString + address.City : "";
        fullAddress += address.State.Length > 0 ? commaString + address.State : "";
        fullAddress += address.ZipCode.Length > 0 ? " " + address.ZipCode : "";
        return fullAddress;
    }
    protected void btnPreferAddressSelect_Click(object sender, EventArgs e)
    {
        if (!rdoEnteredAddress.Checked)
        {
            string selAddress = rdoVerifiedAddressList.SelectedItem.Text;
            string[] splitAddress = selAddress.Split(',');

            string[] splitAddressRemaining = splitAddress.Skip(1).ToArray();
            string[] addr2 = splitAddressRemaining.Take(splitAddressRemaining.Count() - 2).ToArray();

            this.txtAddrAddress1.Text = splitAddress[0].Trim();

            if (addr2.Length > 0)
            {
                this.txtAddrAddress2.Text = string.Join(",", addr2).Trim();
            }

            this.txtAddrCity.Text = splitAddress[splitAddress.Count() - 2].Trim();
            string[] splitStateZipCode = splitAddress[splitAddress.Count() - 1].Trim().Split(' ');
            this.txtAddrZipCode.Text = splitStateZipCode[1];
            this.ddlAddrState.SelectedValue = (from s in idc.States where s.StateAbbrev == splitStateZipCode[0] select s.StateID).FirstOrDefault().ToString();
        }

        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('addressCorrectionDialog')", true);
    }
    protected void RdoEnteredAddress_CheckedChanged1(object sender, EventArgs e)
    {
        rdoVerifiedAddressList.SelectedIndex = -1;
    }
    protected void RdoVerifiedAddressList_SelectedIndexChanged1(object sender, EventArgs e)
    {
        rdoEnteredAddress.Checked = false;
    }
    protected void gv_LocationList_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.Header)
        {
            if ((bool)ViewState["IsGDSAccount"])
                e.Row.Cells[1].Text = "Ptr";
            else
                e.Row.Cells[1].Text = "Location";
        }
    }
    protected void gv_DeviceList_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            int productGroupId;
            int.TryParse(ddlProduct.SelectedValue, out productGroupId);

            var isInstaLink = (productGroupId == ProductGroupIDConstants.InstaLink || productGroupId == ProductGroupIDConstants.InstaLinkUSB || productGroupId == ProductGroupIDConstants.InstaLink3);

            if (e.Row.RowIndex == 0 && !isInstaLink)
            {
                e.Row.Style.Add("height", "60px");
                e.Row.VerticalAlign = VerticalAlign.Bottom;
            }

            // Load Case cover drop down list
            Label lblColorProductID = (Label)e.Row.FindControl("lblColorProductID");
            DropDownList ddlColor = (DropDownList)e.Row.FindControl("ddlColor");
            CheckBox chkbxShipWOAssigned = (CheckBox)e.Row.FindControl("chkbxShipWOAssigned");

            ddlColor.Items.Clear();
            ddlColor.DataSource = Session["CaseCovers"];
            ddlColor.DataBind();
            ListItem firstItem = new ListItem("-- Select Case Cover --", "0");
            ddlColor.Items.Insert(0, firstItem);

            

            // if old ID1 then using ID1 IMI case cover product
            if (!string.IsNullOrEmpty(lblColorProductID.Text) && int.Parse(lblColorProductID.Text) < 6)
            {
                switch (lblColorProductID.Text)
                {
                    case "1":
                        ddlColor.SelectedValue = "26";
                        break;
                    case "2":
                        ddlColor.SelectedValue = "25";
                        break;
                    case "3":
                        ddlColor.SelectedValue = "29";
                        break;
                    case "4":
                        ddlColor.SelectedValue = "27";
                        break;
                    case "5":
                        ddlColor.SelectedValue = "28";
                        break;
                    default:
                        break;
                }
            }
            else
            {
                if (isInstaLink)
                {
                    ddlColor.Visible = false;
                    gv_DeviceList.Columns[1].Visible = false;
                    gv_DeviceList.Columns[2].Visible = false;
                    gv_DeviceList.CssClass = string.Empty;
                    //gv_DeviceList.Columns[4].Visible = false;
                    //gv_DeviceList.Columns[5].HeaderStyle.Width = Unit.Pixel(656);
                    gv_DeviceList.Width = Unit.Percentage(100);
                    gv_DeviceList.HeaderStyle.CssClass = string.Empty;
                }
                else if (productGroupId == ProductGroupIDConstants.Instadose2) // no case cover
                {
                    ddlColor.SelectedValue = "0";
                    ddlColor.Enabled = false;
                }
                else
                {
                    ddlColor.SelectedValue = lblColorProductID.Text;
                }
            }

            // Set ShipWOAssignedUser check box
            chkbxShipWOAssigned.Enabled = (productGroupId == ProductGroupIDConstants.Instadose2 || productGroupId == ProductGroupIDConstants.InstadosePlus || productGroupId == ProductGroupIDConstants.Instadose2New) ? true : false;
        }
    }
    protected void rdobtnBySN_CheckedChanged(object sender, EventArgs e)
    {
        this.SingleFlowRecall.Visible = true;
        this.txtSerialNoInput.Visible = true;
        this.MultipleFlowRecall.Visible = false;
        this.txtAccountInput.Visible = false;

        this.txtSerialNoInput.Text = "";
        this.txtSerialNoInput.Focus();
        this.txtAccountInput.Text = "";
        VisibleRecallBody(false);
    }
    protected void rdobtnByInstadoseAccount_CheckedChanged(object sender, EventArgs e)
    {
        this.SingleFlowRecall.Visible = false;
        this.txtSerialNoInput.Visible = false;
        this.MultipleFlowRecall.Visible = true;
        this.txtAccountInput.Visible = true;

        this.txtSerialNoInput.Text = "";
        this.txtAccountInput.Text = "";
        this.txtAccountInput.Focus();
        VisibleRecallBody(false);
    }
    protected void rdobtnByGDSAccount_CheckedChanged(object sender, EventArgs e)
    {
        this.SingleFlowRecall.Visible = false;
        this.txtSerialNoInput.Visible = false;
        this.MultipleFlowRecall.Visible = true;
        this.txtAccountInput.Visible = true;

        this.txtSerialNoInput.Text = "";
        this.txtAccountInput.Text = "";
        this.txtAccountInput.Focus();
        VisibleRecallBody(false);
    }
    protected void ddlProduct_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Remove previously selected badges and recall reason
        RemoveSelectedBadgesFromNotes("Badge Serial #:");
        RemoveSelectedBadgesFromNotes("Recall Reason:");
        LoadReturnReasons();
        LoadCaseCoverSession();
        // Loading gridview
        Load_gv_DeviceList(this.txtDeviceSearch.Text);        
    }
    protected void ddlLocation_SelectedIndexChanged(object sender, EventArgs e)
    {
        // reload wearerdates by selected location
        if ((bool)ViewState["IsGDSAccount"])
        {
            LoadWearDates(this.lblGDSAccount.Text, this.ddlLocation.SelectedValue);
        }

        // display Location's Shippping address by selected location
        account = (from acc in idc.Accounts where acc.AccountID == int.Parse(this.lblAccountNo.Text) select acc).FirstOrDefault();
        string locationShippingAddress = DisplayLocationShippingAddress(account, this.ddlLocation.SelectedValue);
        if (!String.IsNullOrEmpty(locationShippingAddress))
            lblAddressLocation.Text = locationShippingAddress;

        // Remove previously selected badges
        RemoveSelectedBadgesFromNotes("Badge Serial #:");
        // Reload case cover session
        LoadCaseCoverSession();
        // Loading gridview
        Load_gv_DeviceList(this.txtDeviceSearch.Text);
        
        // Reload ProRate Period Drop Down List, for non IC Care Instadose Account
        if (!(bool)ViewState["IsGDSAccount"] && account.BrandSourceID != 3)
        {
            ProRatePeriod.Visible = true;
            if (int.TryParse(ddlLocation.SelectedValue, out int tmpLocID))
                LoadProratePeriodRadioButton(account, tmpLocID);
            else
                LoadProratePeriodRadioButton(account, null);
        }
        else
        {
            ProRatePeriod.Visible = false;
        }
    }

    protected void ddlReturnReason_SelectedIndexChanged(object sender, EventArgs e)
    {
        string shippingMethod = GetShippingMethodByReturnReasonID(ddlReturnReason.SelectedValue);
        ddlShippingMethod.SelectedValue = shippingMethod;

        int.TryParse(this.ddlReturnReason.SelectedValue, out int returnReasonID);

        string[] lines = txtNotes.Text.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string match = Array.Find(lines, n => n.Contains("Recall Reason:"));
        var notes = string.Join(Environment.NewLine, lines.Where(a => a != match));
        StringBuilder sb = new StringBuilder();        
        if (returnReasonID == 0) 
        {
            sb.Append(notes);
        }
        else
        {
            if (!string.IsNullOrEmpty(notes)) sb.AppendLine(notes);
            sb.Append(string.Format("Recall Reason: {0}", this.ddlReturnReason.SelectedItem.Text));
        }        
        txtNotes.Text = sb.ToString();
    }

    private string GetShippingMethodByReturnReasonID(string returnReasonValue)
    {
        var shippingMethod = "0"; //0 - ground, 1 - express

        if (!string.IsNullOrEmpty(returnReasonValue))
        {
            if (int.TryParse(returnReasonValue, out int reasonID))
            {
                var returnReason = adc.rma_ref_ReturnReasons.FirstOrDefault(x => x.ReasonID == reasonID);
                if (returnReason != null && returnReason.ShippingMethod == _FedExOvernightShippingMethodName)
                {
                    shippingMethod = "1"; // express
                }
            }
        }

        return shippingMethod;
    }

    private void LoadCaseCoverSession()
    {
        int productGroupId;
        int.TryParse(ddlProduct.SelectedValue, out productGroupId);

        string caseCoverName = "";
        switch (productGroupId)
        {
            case ProductGroupIDConstants.Instadose1:
            case ProductGroupIDConstants.Instadose1IMI:
                caseCoverName = "CASE CVR";
                break;
            case ProductGroupIDConstants.InstadosePlus:
                caseCoverName = "COLOR CAP";
                break;
            case ProductGroupIDConstants.InstadoseVue:
                break;
            case ProductGroupIDConstants.Instadose2:
                break;
            case ProductGroupIDConstants.InstadoseVueBeta:
                break;
            case ProductGroupIDConstants.Instadose2New:
                caseCoverName = "BUMP";
                break;
            case ProductGroupIDConstants.InstaLink:
                break;
            case ProductGroupIDConstants.InstaLinkUSB:
                break;
            default:
                break;
        }

        if (productGroupId == ProductGroupIDConstants.Instadose1) // load case cover of IMI-ProductGroupID = 9
        {
            Session["CaseCovers"] = (from prods in idc.Products
                                     where prods.ProductGroupID == 9 && !(prods.Color == "N/A" || prods.Color == "No Color")
                                        && prods.Active == true
                                     select prods);
        }
        else if (productGroupId == ProductGroupIDConstants.Instadose2)
        {
            // only White color
            Session["CaseCovers"] = (from prods in idc.Products
                                     where prods.ProductID == 14 && !(prods.Color == "N/A" || prods.Color == "No Color")
                                        && prods.Active == true
                                     select prods);
        }
        else if (productGroupId == ProductGroupIDConstants.InstadoseVue)
        {
            // only White color
            Session["CaseCovers"] = (from prods in idc.Products
                                     where prods.ProductGroupID == productGroupId
                                        && prods.Color != "N/A" && prods.Color != "No Color"
                                        && prods.Active == true
                                     select prods);
        }
        else
        {
            Session["CaseCovers"] = (from prods in idc.Products
                                     where prods.ProductGroupID == productGroupId && !(prods.Color == "N/A" || prods.Color == "No Color")
                                        && prods.Active == true
                                     select prods);
        }
    }
    private void Load_gv_DeviceList(string pSearchText)
    {
        int.TryParse(ddlProduct.SelectedValue, out int productGroupID);
        var isInstalinkDevice = productGroupID == ProductGroupIDConstants.InstaLink 
            || productGroupID == ProductGroupIDConstants.InstaLinkUSB
            || productGroupID == ProductGroupIDConstants.InstaLink3;

        if (isInstalinkDevice)
        {
            Load_gv_ProductList(pSearchText);
            return;
        }
        else
        {
            if ((bool)ViewState["IsGDSAccount"])
            {
                BadgeRequests request = new BadgeRequests();
                List<GBadgeSearchListItem> badges = new List<GBadgeSearchListItem>();

                badges = request.SearchInstadoseBadges(this.lblGDSAccount.Text, ddlLocation.SelectedValue, null, "", "", null, false);
                badges = badges.Where(x => x.SerialNo != "Pending").ToList();

                // Get latest userdevice (active or not) for the badges - as basis if color dropdown will be changed to blue as default if gray is currently assigned to the user
                var accountDetail = idc.Accounts.FirstOrDefault(x => x.GDSAccount == this.lblGDSAccount.Text);
                int accountID = accountDetail != null ? accountDetail.AccountID : 0;

                List<DeviceColorization> result = new List<DeviceColorization>();
                if (this.isSingleFlow)
                {
                    List<string> serialNos = new List<string>() { txtSerialNoInput.Text};
                    result = GetDeviceColorization(serialNos, accountID);
                }
                else
                {
                    result = GetDeviceColorization(badges.Select(x => x.SerialNo).ToList(), accountID);
                }
                
                var blueProduct = GetBlueCaseCoverProductID(productGroupID);
                foreach (var badge in badges)
                {
                    var item = result.Where(x => x.DeviceID == badge.DeviceID).FirstOrDefault();
                    if (item != null && (item.Product.Color.ToUpper() == "GREY" || item.Product.Color.ToUpper() == "GRAY"))
                    {
                        badge.Color = blueProduct != null ? blueProduct.Color : item.Product.Color;
                    }
                }

                // Filtering by product
                int selBadgeType;
                selBadgeType = GetBadgeTypeByProductGroupId(ddlProduct.SelectedValue);

                badges = badges.FindAll(x => x.BadgeType == selBadgeType);

                // Filtering by search text
                if (pSearchText.Length > 0)
                {
                    badges = badges.FindAll(x => x.SerialNo.Contains(pSearchText.Trim()) || (x.WearerInfo != null && x.WearerInfo.ToLower().Contains(pSearchText.Trim().ToLower())));
                }

                // Create the review orders datatable to hold the contents of the order.
                DataTable dtAccountDevice = new DataTable("AccountDevice Badge");

                // Create the columns for the review orders datatable.
                dtAccountDevice.Columns.Add("DeviceID", Type.GetType("System.String"));
                dtAccountDevice.Columns.Add("UserID", Type.GetType("System.String"));
                dtAccountDevice.Columns.Add("GDSWearerID", Type.GetType("System.String"));
                dtAccountDevice.Columns.Add("ColorProductID", Type.GetType("System.String"));
                dtAccountDevice.Columns.Add("BodyRegionName", Type.GetType("System.String"));
                dtAccountDevice.Columns.Add("SerialNo", Type.GetType("System.String"));
                dtAccountDevice.Columns.Add("FullName", Type.GetType("System.String"));

                foreach (GBadgeSearchListItem item in badges)
                {
                    DataRow dr = dtAccountDevice.NewRow();

                    dr["DeviceID"] = item.DeviceID;
                    dr["UserID"] = item.InsUserID;
                    dr["GDSWearerID"] = item.WearerID;
                    dr["ColorProductID"] = GetColorProductID(item.Color, ddlProduct.SelectedValue);
                    dr["BodyRegionName"] = item.BodyRegion;
                    dr["SerialNo"] = item.SerialNo;
                    dr["FullName"] = item.WearerInfo;

                    // Add row to the data table.
                    dtAccountDevice.Rows.Add(dr);
                }

                DataView dvAccountDevice = dtAccountDevice.DefaultView;
                dvAccountDevice.Sort = "SerialNo";
                gv_DeviceList.DataSource = dvAccountDevice;
                gv_DeviceList.DataBind();
            }
            else
            {
                var searchResult = idc.sp_GetDevicesByProduct_Location_Search(int.Parse(lblAccountNo.Text), int.Parse(ddlLocation.SelectedValue), int.Parse(ddlProduct.SelectedValue), pSearchText.Trim()).ToList();

                int accountID = 0;
                bool isValidAccountNo = Int32.TryParse(this.lblAccountNo.Text, out accountID);
                var result = GetDeviceColorization(searchResult.Select(x => x.SerialNo).ToList(), accountID);
                var blueProduct = GetBlueCaseCoverProductID(productGroupID);

                foreach (var badge in searchResult)
                {
                    var item = result.Where(x => x.DeviceID == badge.DeviceID).FirstOrDefault();
                    if (item != null && (item.Product.Color.ToUpper() == "GREY" || item.Product.Color.ToUpper() == "GRAY"))
                    {
                        badge.ColorProductID = blueProduct != null ? blueProduct.ProductID : item.Product.ProductID;
                    }
                }

                gv_DeviceList.DataSource = searchResult;
                gv_DeviceList.DataBind();
            }

            // Go through each row of grid to set the recall info of a device if it has been selected in previous search.
            setGridWithSelectedDeviceInfo();
        }
    }

    private void Load_gv_ProductList(string pSearchText)
    {
        if ((bool)ViewState["IsGDSAccount"])
        {
            BadgeRequests request = new BadgeRequests();

            var gdsAccount = this.lblGDSAccount.Text;

            var trimmedSearch = pSearchText.Trim();
            var hasSearchText = !string.IsNullOrEmpty(trimmedSearch);

            int.TryParse(ddlProduct.SelectedValue, out int productGroupID);

            var gdsLocation = ddlLocation.SelectedValue;
            var hasLocation = !string.IsNullOrEmpty(gdsLocation);

            var devices = (from ap in idc.AccountProducts
                           join a in idc.Accounts on ap.AccountID equals a.AccountID
                           join l in idc.Locations on ap.LocationID equals l.LocationID
                           join pi in idc.ProductInventories on ap.ProductInventoryID equals pi.ProductInventoryID
                           join p in idc.Products on pi.ProductID equals p.ProductID
                            where a.GDSAccount == gdsAccount
                                && p.ProductGroupID == productGroupID
                                && pi.SerialNo != "Pending"
                                && (!hasLocation || l.GDSLocation == gdsLocation)
                                && (!hasSearchText || pi.SerialNo.StartsWith(trimmedSearch))
                            select new
                            {
                                InsAccountID = a.AccountID,
                                Account = a.GDSAccount,
                                InsLocationID = ap.LocationID,
                                Location = l.GDSLocation.Trim(),
                                ProductID = pi.ProductID,
                                ProductName = p.ProductName,
                                ProductSKU = p.ProductSKU,
                                SerialNo = pi.SerialNo
                            });


            // Create the review orders datatable to hold the contents of the order.
            DataTable dtAccountDevice = new DataTable("Account Products");

            // Create the columns for the review orders datatable.
            dtAccountDevice.Columns.Add("DeviceID", Type.GetType("System.String"));
            dtAccountDevice.Columns.Add("UserID", Type.GetType("System.String"));
            dtAccountDevice.Columns.Add("GDSWearerID", Type.GetType("System.String"));
            dtAccountDevice.Columns.Add("ColorProductID", Type.GetType("System.String"));
            dtAccountDevice.Columns.Add("BodyRegionName", Type.GetType("System.String"));
            dtAccountDevice.Columns.Add("SerialNo", Type.GetType("System.String"));
            dtAccountDevice.Columns.Add("FullName", Type.GetType("System.String"));

            foreach (var item in devices)
            {
                DataRow dr = dtAccountDevice.NewRow();

                dr["DeviceID"] = item.ProductID;
                dr["UserID"] = string.Empty;
                dr["GDSWearerID"] = string.Empty;
                dr["ColorProductID"] = item.ProductID;
                dr["BodyRegionName"] = string.Empty;
                dr["SerialNo"] = item.SerialNo;
                dr["FullName"] = string.Empty;

                // Add row to the data table.
                dtAccountDevice.Rows.Add(dr);
            }

            DataView dvAccountDevice = dtAccountDevice.DefaultView;
            dvAccountDevice.Sort = "SerialNo";
            gv_DeviceList.DataSource = dvAccountDevice;
            gv_DeviceList.DataBind();
        }
        else
        {
            var searchResult = idc.sp_GetDevicesByProduct_Location_Search(int.Parse(lblAccountNo.Text), int.Parse(ddlLocation.SelectedValue), int.Parse(ddlProduct.SelectedValue), pSearchText.Trim()).ToList();

            gv_DeviceList.DataSource = searchResult;
            gv_DeviceList.DataBind();
        }

        // Go through each row of grid to set the recall info of a device if it has been selected in previous search.
        setGridWithSelectedDeviceInfo();
    }

    private int GetBadgeTypeByProductGroupId(string sProductGroupId)
    {
        int productGroupId;
        var isNumericProductGroupId = int.TryParse(sProductGroupId, out productGroupId);
        if (isNumericProductGroupId)
            return GetBadgeTypeByProductGroupId(productGroupId);
        return 0;
    }
    private int GetBadgeTypeByProductGroupId(int productGroupId)
    {
        var productGroup = idc.ProductGroups.FirstOrDefault(x => x.ProductGroupID == productGroupId);

        if (productGroup != null)
            return Convert.ToInt32(productGroup.GDSBadgeType);
        else
            return 33; //InstaLink USB 
    }

    private String GetColorProductID(String pColor, string pProductGroupID)
    {
        string colorProductID = string.Empty;
        int productGroupId;
        var isNumericProductGroupId = int.TryParse(pProductGroupID, out productGroupId);
        if (!isNumericProductGroupId)
            return string.Empty;

        switch (productGroupId)
        {
            case ProductGroupIDConstants.Instadose1IMI:
                switch (pColor)
                {
                    case "No Color":
                        colorProductID = "0";
                        break;
                    case "Black":
                        colorProductID = "25";
                        break;
                    case "Blue":
                        colorProductID = "26";
                        break;
                    case "Green":
                        colorProductID = "27";
                        break;
                    case "Pink":
                        colorProductID = "28";
                        break;
                    case "Silver":
                        colorProductID = "29";
                        break;
                    default:
                        colorProductID = "0";
                        break;
                }
                break;
            case ProductGroupIDConstants.Instadose2:
                colorProductID = "0";
                break;
            case ProductGroupIDConstants.InstadosePlus:
                switch (pColor)
                {
                    case "No Color":
                        colorProductID = "0";
                        break;
                    case "Blue":
                        colorProductID = "31";
                        break;
                    case "Grey":
                        colorProductID = "32";
                        break;
                    case "Green":
                        colorProductID = "33";
                        break;
                    case "Orange":
                        colorProductID = "34";
                        break;
                    case "Pink":
                        colorProductID = "35";
                        break;
                    case "Red":
                        colorProductID = "36";
                        break;
                    default:
                        colorProductID = "0";
                        break;
                }
                break;
            case ProductGroupIDConstants.InstadoseVue:
                var product = idc.Products.FirstOrDefault(x => x.ProductGroupID == productGroupId && x.Color == pColor);
                if (product != null)
                    colorProductID = product.ProductID.ToString();
                else
                    colorProductID = "0";
                break;
            case ProductGroupIDConstants.InstadoseVueBeta:
                var productB = idc.Products.FirstOrDefault(x => x.ProductGroupID == productGroupId && x.Color == pColor);
                if (productB != null)
                    colorProductID = productB.ProductID.ToString();
                else
                    colorProductID = "0";
                break;
            case ProductGroupIDConstants.Instadose2New:
                switch (pColor)
                {
                    case "No Color":
                        colorProductID = "0";
                        break;
                    case "Blue":
                        colorProductID = "45";
                        break;
                    case "Grey":
                        colorProductID = "46";
                        break;
                    case "Green":
                        colorProductID = "47";
                        break;
                    case "Orange":
                        colorProductID = "48";
                        break;
                    case "Pink":
                        colorProductID = "49";
                        break;
                    case "Red":
                        colorProductID = "50";
                        break;
                    default:
                        colorProductID = "0";
                        break;
                }
                break;
            case ProductGroupIDConstants.InstaLink:
                colorProductID = "0";
                break;
            case ProductGroupIDConstants.InstaLinkUSB:
                colorProductID = "0";
                break;
            default:
                colorProductID = "0";
                break;
        }

        return colorProductID;
    }
    protected void txtDeviceSearch_TextChanged(object sender, EventArgs e)
    {
        // Loading gridview
        Load_gv_DeviceList(this.txtDeviceSearch.Text);        
    }
    private bool InsertReturnDevice_SendNotificationEmail_DeactivateUserDevice_DeactivateAccountDevice(int pNewReturnID, int pAccountID, int pAssignedUserID, string pSerialNo, bool pShipWOAssignedUser, ref string pSubmitError)
    {
        bool isSendEmail = false;
        var device = idc.DeviceInventories.FirstOrDefault(di => di.SerialNo == pSerialNo);

        // Insert data to Transaction Log with new ReturnDevicesID
        var writeTransLogReturn = adc.sp_rma_process(pNewReturnID, 0, 0, " ",
                this.UserName, DateTime.Now, "RECALL ORDER",
                "Preparing order to BizApps & MAS. Ser# " + pSerialNo, 2);

        // Save Serial No with ReturnID. Also save userID if the device was assigned to this user prior making Recall.
        if (!SaveSelectedSerialNo(pNewReturnID, pAssignedUserID, pSerialNo, pShipWOAssignedUser, ref pSubmitError))
        {
            pSubmitError = "Save Selected SerialNo to rma_ReturnDevice error:" + "</br>" + pSubmitError;
            return false;
        }

        // 4. ************ send email, deactivate UserDevice *******************//
        try
        {
            if (device != null &&
                (device.Product.ProductGroup.ProductGroupID == ProductGroupIDConstants.Instadose1 || device.Product.ProductGroup.ProductGroupID == ProductGroupIDConstants.Instadose1IMI))
                isSendEmail = true;
        }
        catch(Exception ex)
        {
            Basics.WriteLogEntry("Error while processing " + pSerialNo + ": " + ex.Message, this.UserName, "Portal.AddRMA.EmailAndDeactivate", Basics.MessageLogType.Notice);
        }

        // Get active UserDevice detail 
        var Uid = (from UD in idc.UserDevices
                   join DI in idc.DeviceInventories on UD.DeviceID equals DI.DeviceID
                   where DI.SerialNo == pSerialNo
                   && UD.Active == true
                   select UD).ToArray();

        // ************* Send email to assigned Wearer and Account Admin *********************//
        //Message Subject
        string MsgTitle = "Instadose Recall Notification";
        //Wearer name
        string WearerFirstLastName = "";

        //*************Send email to user and Account Admin and deactivate UserDevice * ********************//
        if (Uid.Length >= 1)
        {
            for (int i = 0; i < Uid.Length; i++)
            {
                if (isSendEmail)
                {
                    //grap User's email and name
                    User Uinfo = null;
                    Uinfo = (from ui in idc.Users
                             where ui.UserID == Uid[i].UserID
                             select ui).FirstOrDefault();

                    string WearerEmail = "";

                    if (Uinfo != null)
                    {

                        WearerFirstLastName = Uinfo.FirstName + " " + Uinfo.LastName;
                        WearerEmail = (Uinfo.Email != null) ? Uinfo.Email.ToString() : TechOpsEmailAddress;       // send to Tech-ops manager = Dani if user does not have email address                                         

                        //Send To addressList 
                        List<string> ToAddList = new List<string>();
                        ToAddList.Add(WearerEmail);

                        //Build the template fileds.
                        Dictionary<string, string> WearerFields = new Dictionary<string, string>();
                        WearerFields.Add("EmailTitle", MsgTitle);
                        WearerFields.Add("WearerName", WearerFirstLastName);
                        WearerFields.Add("SerialNo", pSerialNo);

                        //*** send email to WearerEmail -- email class 
                        bool sendWearerEmail = SendRecallMessage("Wearer", ToAddList, WearerFields);

                        // insert TransLog, update devices status
                        var writeTransLogEmailWearer = adc.sp_rma_process(pNewReturnID, 0, 0,
                                  " ", this.UserName, DateTime.Now, "RECALL ORDER",
                                  "Email Wearer: " + sendWearerEmail.ToString(), 2);

                        try
                        {
                            //grap account administrator
                            var AdminInfo = (from a in idc.Accounts
                                             join u in idc.Users
                                                on a.AccountAdminID equals u.UserID
                                             where a.AccountID == Uinfo.AccountID
                                             select new { u.FirstName, u.LastName, u.Email }).ToArray();

                            if (AdminInfo.Length >= 1)
                            {
                                for (int j = 0; j < AdminInfo.Length; j++)
                                {
                                    string AdminFirstLastName = AdminInfo[j].FirstName + " " + AdminInfo[j].LastName;
                                    string AdminEmail = (AdminInfo[j].Email != null) ? AdminInfo[j].Email.ToString() : TechOpsEmailAddress;      // send to Tech-ops manager = Dani if Admin account does not have email address

                                    if (WearerEmail != AdminEmail)
                                    {
                                        //Send To addressList 
                                        List<string> AdminToAddList = new List<string>();
                                        AdminToAddList.Add(AdminEmail);

                                        //Build the template fileds.
                                        Dictionary<string, string> AdminFields = new Dictionary<string, string>();
                                        AdminFields.Add("EmailTitle", MsgTitle);
                                        AdminFields.Add("WearerName", WearerFirstLastName);
                                        AdminFields.Add("SerialNo", pSerialNo);
                                        AdminFields.Add("AdministratorName", AdminFirstLastName);

                                        //*** send email to Administrator -- email class 
                                        bool sendAdminEmail = SendRecallMessage("Admin", AdminToAddList, AdminFields);
                                        // insert TransLog, update devices status
                                        var writeTransLogEmailAdmin = adc.sp_rma_process(pNewReturnID, 0, 0,
                                                " ", this.UserName, DateTime.Now, "RECALL ORDER",
                                                "Email Admin: " + sendAdminEmail.ToString(), 2);
                                    }
                                }
                            }
                        }
                        catch
                        {
                            var writeTransLogEmailAdmin = adc.sp_rma_process(pNewReturnID, 0, 0,
                                               " ", this.UserName, DateTime.Now, "RECALL ORDER",
                                               "Email Admin: ERROR ", 2);
                        }

                    }
                }

                // insert TransLog
                var writeTransLogUPD = adc.sp_rma_process(pNewReturnID, 0, 0,
                              " ", this.UserName, DateTime.Now, "DEACTIVATE", "Deactive Serial# " + pSerialNo, 2);
                // ************** remove DeviceID from User ************//
            }
        }
        else // ************* Send email to Account Admin only *********************//
        {
            if (isSendEmail)
            {
                try
                {
                    //grap account administrator
                    var AdminInfo = (from a in idc.Accounts
                                     join u in idc.Users
                                        on a.AccountAdminID equals u.UserID
                                     where a.AccountID == pAccountID
                                     select new { u.FirstName, u.LastName, u.Email }).ToArray();

                    if (AdminInfo.Length >= 1)
                    {
                        for (int j = 0; j < AdminInfo.Length; j++)
                        {
                            string AdminFirstLastName = AdminInfo[j].FirstName + " " + AdminInfo[j].LastName;
                            string AdminEmail = (AdminInfo[j].Email != null) ? AdminInfo[j].Email.ToString() : TechOpsEmailAddress;      // send to Tech-ops manager = Dani if Admin account does not have email address

                            //Send To addressList 
                            List<string> AdminToAddList = new List<string>();
                            AdminToAddList.Add(AdminEmail);

                            //Build the template fileds.
                            Dictionary<string, string> AdminFields = new Dictionary<string, string>();
                            AdminFields.Add("EmailTitle", MsgTitle);
                            AdminFields.Add("WearerName", WearerFirstLastName);
                            AdminFields.Add("SerialNo", pSerialNo);
                            AdminFields.Add("AdministratorName", AdminFirstLastName);

                            //*** send email to Administrator -- email class 
                            bool sendAdminEmail = SendRecallMessage("Admin", AdminToAddList, AdminFields);
                            // insert TransLog, update devices status
                            var writeTransLogEmailAdmin = adc.sp_rma_process(pNewReturnID, 0, 0,
                                    " ", this.UserName, DateTime.Now, "RECALL ORDER",
                                    "Email Admin: " + sendAdminEmail.ToString(), 2);
                        }
                    }
                }
                catch
                {
                    var writeTransLogEmailAdmin = adc.sp_rma_process(pNewReturnID, 0, 0,
                                       " ", this.UserName, DateTime.Now, "RECALL ORDER",
                                       "Email Admin: ERROR ", 2);
                }
            }
        }
        // ******************** Done sending email **************************** //

        // Insert data to Transaction Log with new ReturnDevicesID
        var writeTransLogReturn2 = adc.sp_rma_process(pNewReturnID, 0, 0, " ",
                this.UserName, DateTime.Now, "RECALL ORDER",
                "Order have been sent to Biz & MAS. Ser# " + pSerialNo, 2);

        return true;
    }
    
    protected void ddlColor_SelectedIndexChanged(object sender, EventArgs e)
    {
        List<RecallDeviceInfo> selectedDeviceList = new List<RecallDeviceInfo>();

        if (Session["SelectedDeviceList"] != null)
            selectedDeviceList = (List<RecallDeviceInfo>)(Session["SelectedDeviceList"]);

        DropDownList ddlColor = (DropDownList)sender;
        GridViewRow row = (GridViewRow)ddlColor.NamingContainer;

        CheckBox chkBoxSelect = (CheckBox)row.FindControl("chkbxSelectDeviceList");
        CheckBox chkbxShipWOAssigned = (CheckBox)row.FindControl("chkbxShipWOAssigned");
        HiddenField HidAssignedWearerID = (HiddenField)row.FindControl("HidAssignedWearerID");
        HiddenField HidAssignedUserID = (HiddenField)row.FindControl("HidAssignedUserID");
        Label lblColorProductID = (Label)row.FindControl("lblColorProductID");

        string sn = row.Cells[3].Text.Replace("\n", "").ToString().Trim();

        var selDevice = selectedDeviceList.SingleOrDefault(x => x.SerialNo == sn);

        if (chkBoxSelect.Checked)
        {
            if (selDevice == null)
            {
                RecallDeviceInfo selectedDevice = new RecallDeviceInfo();
                selectedDevice.SerialNo = sn;
                selectedDevice.ColorProductID = ddlColor.SelectedValue;
                selectedDevice.ShipWOAssigned = chkbxShipWOAssigned.Checked;
                selectedDeviceList.Add(selectedDevice);
            }
            else
            {
                selDevice.ColorProductID = ddlColor.SelectedValue;
                selDevice.ShipWOAssigned = chkbxShipWOAssigned.Checked;
            }

            if ((ddlColor.SelectedItem.Text.ToUpper() == "GREY" || ddlColor.SelectedItem.Text.ToUpper() == "GRAY") && (!String.IsNullOrEmpty(HidAssignedWearerID.Value) || !String.IsNullOrEmpty(HidAssignedUserID.Value)))
            {
                VisibleErrors("Cannot assign gray color to the wearer");
                ddlColor.SelectedValue = lblColorProductID.Text;
                selDevice.ColorProductID = ddlColor.SelectedValue;
            }
        }
        else
        {
            if (selDevice != null)
                selectedDeviceList.Remove(selDevice);
        }

        // Use this session to save all selected SN in the grid for recall
        Session["SelectedDeviceList"] = selectedDeviceList;
    }

    protected void chkbxShipWOAssigned_CheckedChanged(object sender, EventArgs e)
    {
        List<RecallDeviceInfo> selectedDeviceList = new List<RecallDeviceInfo>();

        if (Session["SelectedDeviceList"] != null)
            selectedDeviceList = (List<RecallDeviceInfo>)(Session["SelectedDeviceList"]);

        CheckBox chkbxShipWOAssigned = (CheckBox)sender;
        GridViewRow row = (GridViewRow)chkbxShipWOAssigned.NamingContainer;

        DropDownList ddlColor = (DropDownList)row.FindControl("ddlColor");
        CheckBox chkBoxSelect = (CheckBox)row.FindControl("chkbxSelectDeviceList");
        string sn = row.Cells[3].Text.Replace("\n", "").ToString().Trim();

        var selDevice = selectedDeviceList.SingleOrDefault(x => x.SerialNo == sn);

        if (chkBoxSelect.Checked)
        {
            if (selDevice == null)
            {
                RecallDeviceInfo selectedDevice = new RecallDeviceInfo();
                selectedDevice.SerialNo = sn;
                selectedDevice.ColorProductID = ddlColor.SelectedValue;
                selectedDevice.ShipWOAssigned = chkbxShipWOAssigned.Checked;
                selectedDeviceList.Add(selectedDevice);
            }
            else
            {
                selDevice.ColorProductID = ddlColor.SelectedValue;
                selDevice.ShipWOAssigned = chkbxShipWOAssigned.Checked;
            }
        }
        else
        {
            if (selDevice != null)
                selectedDeviceList.Remove(selDevice);
        }

        // Use this session to save all selected SN in the grid for recall
        Session["SelectedDeviceList"] = selectedDeviceList;
    }

    protected void chkbxSelectDeviceList_CheckedChanged(object sender, EventArgs e)
    {
        List<RecallDeviceInfo> selectedDeviceList = new List<RecallDeviceInfo>();

        if (Session["SelectedDeviceList"] != null)
            selectedDeviceList = (List<RecallDeviceInfo>)(Session["SelectedDeviceList"]);

        CheckBox chkBoxSelect = (CheckBox)sender;
        GridViewRow row = (GridViewRow)chkBoxSelect.NamingContainer;

        DropDownList ddlColor = (DropDownList)row.FindControl("ddlColor");
        CheckBox chkbxShipWOAssigned = (CheckBox)row.FindControl("chkbxShipWOAssigned");

        Label lblColorProductID = (Label)row.FindControl("lblColorProductID");
        HiddenField HidAssignedWearerID = (HiddenField)row.FindControl("HidAssignedWearerID");
        HiddenField HidAssignedUserID = (HiddenField)row.FindControl("HidAssignedUserID");

        string sn = row.Cells[3].Text.Replace("\n", "").ToString().Trim();

        var selDevice = selectedDeviceList.SingleOrDefault(x => x.SerialNo == sn);

        if (chkBoxSelect.Checked)
        {
            if (selDevice == null)
            {
                RecallDeviceInfo selectedDevice = new RecallDeviceInfo();
                selectedDevice.SerialNo = sn;
                selectedDevice.ColorProductID = ddlColor.SelectedValue;
                selectedDevice.ShipWOAssigned = chkbxShipWOAssigned.Checked;
                selectedDeviceList.Add(selectedDevice);
            }
            else
            {
                selDevice.ColorProductID = ddlColor.SelectedValue;
                selDevice.ShipWOAssigned = chkbxShipWOAssigned.Checked;
            }

            if ((ddlColor.SelectedItem.Text.ToUpper() == "GREY" || ddlColor.SelectedItem.Text.ToUpper() == "GRAY") && (!String.IsNullOrEmpty(HidAssignedWearerID.Value) || !String.IsNullOrEmpty(HidAssignedUserID.Value)))
            {
                VisibleErrors("Cannot assign gray color to the wearer");
                ddlColor.SelectedValue = lblColorProductID.Text;
            }
        }
        else
        {
            if (selDevice != null)
                selectedDeviceList.Remove(selDevice);
        }

        // Add/Remove selected badge from notes
        SetNotes(row, chkBoxSelect.Checked);
        // Use this session to save all selected SN in the grid for recall
        Session["SelectedDeviceList"] = selectedDeviceList;
    }

    protected void chkbxHeaderDeviceList_CheckedChanged(object sender, EventArgs e)
    {
        CheckBox cb = (CheckBox)sender;
        List<RecallDeviceInfo> selectedDeviceList = new List<RecallDeviceInfo>();

        if (Session["SelectedDeviceList"] != null)
            selectedDeviceList = (List<RecallDeviceInfo>)(Session["SelectedDeviceList"]);

        foreach (GridViewRow row in this.gv_DeviceList.Rows)
        {
            CheckBox chkBoxSelect = (CheckBox)row.FindControl("chkbxSelectDeviceList");
            DropDownList ddlColor = (DropDownList)row.FindControl("ddlColor");
            CheckBox chkbxShipWOAssigned = (CheckBox)row.FindControl("chkbxShipWOAssigned");
            string sn = row.Cells[3].Text.Replace("\n", "").ToString().Trim();

            chkBoxSelect.Checked = cb.Checked;
            var selDevice = selectedDeviceList.SingleOrDefault(x => x.SerialNo == sn);

            if (chkBoxSelect.Checked)
            {
                if (selDevice == null)
                {
                    RecallDeviceInfo selectedDevice = new RecallDeviceInfo();
                    selectedDevice.SerialNo = sn;
                    selectedDevice.ColorProductID = ddlColor.SelectedValue;
                    selectedDevice.ShipWOAssigned = chkbxShipWOAssigned.Checked;
                    selectedDeviceList.Add(selectedDevice);
                }
                else
                {
                    selDevice.ColorProductID = ddlColor.SelectedValue;
                    selDevice.ShipWOAssigned = chkbxShipWOAssigned.Checked;
                }
            }
            else
            {
                if (selDevice != null)
                    selectedDeviceList.Remove(selDevice);
            }

            // Add/Remove selected badge from notes
            SetNotes(row, chkBoxSelect.Checked);
        }

        // Use this session to save all selected SN in the grid for recall
        Session["SelectedDeviceList"] = selectedDeviceList;
    }
    /// <summary>
    /// Go through each row of grid to set the recall info of a device if it has been selected in previous search
    /// </summary>
    private void setGridWithSelectedDeviceInfo()
    {        
        if (Session["SelectedDeviceList"] != null)
        {
            List<RecallDeviceInfo> selectedDeviceList = new List<RecallDeviceInfo>();
            selectedDeviceList = (List<RecallDeviceInfo>)(Session["SelectedDeviceList"]);
            if (selectedDeviceList.Count > 0)
            {
                foreach (GridViewRow row in this.gv_DeviceList.Rows)
                {
                    CheckBox chkBoxSelect = (CheckBox)row.FindControl("chkbxSelectDeviceList");
                    DropDownList ddlColor = (DropDownList)row.FindControl("ddlColor");
                    CheckBox chkbxShipWOAssigned = (CheckBox)row.FindControl("chkbxShipWOAssigned");
                    string sn = row.Cells[3].Text.Replace("\n", "").ToString().Trim();

                    var selDevice = selectedDeviceList.SingleOrDefault(x => x.SerialNo == sn);

                    if (selDevice != null)
                    {
                        chkBoxSelect.Checked = true;
                        ddlColor.SelectedValue = selDevice.ColorProductID;
                        chkbxShipWOAssigned.Checked = selDevice.ShipWOAssigned;

                        // Add/Remove selected badge from notes
                        SetNotes(row, chkBoxSelect.Checked);
                    }
                }
            }
        }
    }

    private void LoadProratePeriodRadioButton(Account account, int? locationID)
    {
        Location location = new Location();

        if (locationID != null && locationID > 0)
        {
            location = (from l in idc.Locations where l.LocationID == locationID select l).FirstOrDefault();
        }
        else
        {
            // Grab account default Location
            location = (from L in idc.Locations where L.IsDefaultLocation == true && L.AccountID == this.account.AccountID select L).FirstOrDefault();
        }

        DateTime orderCreatedDate = DateTime.Now;
        DateTime startQuarterDate, nextQuarterDate;
        DateTime contractEndDate = GetContractEndDate(account, location);

        int qtrNo = Common.calculateNumberOfQuarterService(GetContractStartDate(account, location), contractEndDate, orderCreatedDate, out startQuarterDate);
        nextQuarterDate = startQuarterDate.AddMonths(3);

        radProratePeriod.Items.Clear();

        if (account.BillingTermID == 2) // yearly
        {
            radProratePeriod.Items.Add(new ListItem(string.Format("{0:MM/dd/yyyy}", startQuarterDate) + " - " + string.Format("{0:MM/dd/yyyy}", contractEndDate), startQuarterDate.ToShortDateString()));
            if (nextQuarterDate < contractEndDate)
            {
                radProratePeriod.Items.Add(new ListItem(string.Format("{0:MM/dd/yyyy}", nextQuarterDate) + " - " + string.Format("{0:MM/dd/yyyy}", contractEndDate), nextQuarterDate.ToShortDateString()));
                radProratePeriod.Items[1].Enabled = false;
            }
        }
        else // quarterly
        {
            radProratePeriod.Items.Add(new ListItem(string.Format("{0:MM/dd/yyyy}", startQuarterDate) + " - " + string.Format("{0:MM/dd/yyyy}", startQuarterDate.AddMonths(3).AddDays(-1)), startQuarterDate.ToShortDateString()));
            if (nextQuarterDate < contractEndDate)
            {
                radProratePeriod.Items.Add(new ListItem(string.Format("{0:MM/dd/yyyy}", nextQuarterDate) + " - " + string.Format("{0:MM/dd/yyyy}", nextQuarterDate.AddMonths(3).AddDays(-1)), nextQuarterDate.ToShortDateString()));
                radProratePeriod.Items[1].Enabled = false;
            }
        }

        if (nextQuarterDate < contractEndDate && nextQuarterDate.Subtract(orderCreatedDate).Days < 30)
        {
            radProratePeriod.Items[1].Enabled = true;
        }

        radProratePeriod.SelectedIndex = 0;

        //if (order != null)
        //{
        //    ListItem item = radProratePeriod.Items.FindByValue(order.StxContractStartDate.HasValue ? order.StxContractStartDate.Value.ToShortDateString() : DateTime.Now.ToShortDateString());
        //    if (item != null)
        //        radProratePeriod.SelectedIndex = radProratePeriod.Items.IndexOf(item);
        //}
    }

    private void SetNotes(GridViewRow row, bool selected)
    {
        if (row != null)
        {
            string sn = row.Cells[3].Text.Replace("\n", "").ToString().Trim();
            string wearer = HttpUtility.HtmlDecode(row.Cells[5].Text.Replace("\n", "").ToString().Trim());

            if (selected)
            {
                if (!txtNotes.Text.Contains(sn))
                {
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine(txtNotes.Text);
                    sb.Append(string.Format("Badge Serial #: {0}, Wearer : {1}", sn, wearer));
                    txtNotes.Text = sb.ToString();
                }                
            }
            else
            {
                string[] lines = txtNotes.Text.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                string match = Array.Find(lines, n => n.Contains(sn));
                var notes = string.Join(Environment.NewLine, lines.Where(a => a != match));
                StringBuilder sb = new StringBuilder();
                sb.Append(notes);
                txtNotes.Text = sb.ToString();
            }
        }
    }

    private void RemoveSelectedBadgesFromNotes(string noteTextToRemove)
    {
        string[] lines = txtNotes.Text.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        string[] match = Array.FindAll(lines, n => n.Contains(noteTextToRemove));        
        var notes = string.Join(Environment.NewLine, lines.Except(match));
        StringBuilder sb = new StringBuilder();
        sb.Append(notes);
        txtNotes.Text = sb.ToString();
    }
}

public class RecallDeviceInfo
{
    public string SerialNo { get; set; }
    public string ColorProductID { get; set; }
    public bool ShipWOAssigned { get; set; }
}


