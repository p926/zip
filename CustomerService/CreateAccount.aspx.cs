// 10/4/2011 add Dealer options
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using System.Web;
using System.IO;

using Instadose;
using Instadose.Data;
using Instadose.Security;
using Instadose.Integration;
using portal_instadose_com_v3.Helpers;
using Instadose.API.DA;
using Mirion.DSD.GDS.API;
//using com.paypal.soap.api;

public partial class CustomerService_CreateAccount : System.Web.UI.Page
{
    #region Private Properties

    int AccountID = 0;
    int OrderID = 0;
    int LocationID = 0;
    int UserID = 0;

    // Create the database reference
    InsDataContext idc = new InsDataContext();

    //MASDataContext sdc = null;
    MASDataContext mdc = new MASDataContext();

    bool isCustomRate = false;

    // Create Hashtable for new register

    private Hashtable HashFormInfo;

    private DataTable dtDiscounts = new DataTable();
    private DataTable dtDiscounts2 = new DataTable();
    public DataRow drDiscounts;

    // String to hold the current username
    string UserName = "Unknown";

    /// <summary>
    /// Limits access to the functionality based on groups.
    /// </summary>
    string[] activeDirecoryGroups = { "IRV-IT", "IRV-CustomerSvc", "IRV-Client Services" };

    #endregion

    #region Public Properties
    public bool IsDoNotSendRestricted { get; private set; }
    #endregion

    #region  Page and Control Events

    protected void Page_Load(object sender, EventArgs e)
    {

        clearError();

        // Try to set the username
        try
        {
            this.UserName = User.Identity.Name.Split('\\')[1];
        }
        catch { this.UserName = "Unknown"; }

        //10/21/13 WK - Turn on once it is needed.
        //chkCustomRate.Visible = false;
        //******************************************************


        //Query active directory to see if the current user belongs to a group.
        //If not authorized,  DO NOT allow adding of discounts.
        bool belongsToGroups = ActiveDirectoryQueries.UserExistsInAnyGroup(User.Identity.Name, activeDirecoryGroups);
        if (belongsToGroups)
        {
            btnAddDiscount.Visible = true;
        }

        // 5/13/2014, Tdo. Turn on Prefer Shipping Carrier if using Malvern shipping
        if (Convert.ToBoolean(GetAppSettings("MalvernIntegration")))
            this.PreferCarrier.Visible = true;
        else
            this.PreferCarrier.Visible = false;


        if (Request.QueryString["ID"] == null) //if accountID null, preform "create new account" 
        {
            string strBrand = ddlBrand.SelectedValue;
            string strDealerID = (ddlDealer.Visible == true) ? ddlDealer.SelectedValue : "";

            //Disable email address for billing - IC Care only.
            if (ddlDealer.Visible)
                reqfldvalBillingEmailAddress.Enabled = false;

            PageIsNotPostBackLoadInfo(AccountID, UserID, LocationID, strBrand, strDealerID);
            //PageIsPostBackLoadInfo(AccountID, UserID, LocationID, strBrand, strDealerID);

            AccountToolBar.Visible = false;
            btnSave.Visible = true;
            btnUpdate.Visible = false;

            try
            {
                //Hide discounts grid until account is created.
                if (ddlRateCode.SelectedItem.Text.Contains("Select") || ddlCustomerType.SelectedItem.Text.Trim() == "CON")
                {
                    divAddAndSearchToolbar.Visible = false;
                    gvDiscounts.Visible = false;
                }
                if (ddlCustomerType.SelectedItem.Text.Trim() == "CON")
                {
                    RequiredFieldValidatorMaxIDPlusInventory.Enabled = true;
                    RegularExpressionValidatorMaxIDPlusInventory.Enabled = true;

                    RequiredFieldValidatorMaxIDLinkInventory.Enabled = true;
                    RegularExpressionValidatorMaxIDLinkInventory.Enabled = true;

                    RequiredFieldValidatorMaxIDLinkUSBInventory.Enabled = true;
                    RegularExpressionValidatorMaxIDLinkUSBInventory.Enabled = true;


                    RequiredFieldValidatorMaxID1Inventory.Enabled = true;
                    RegularExpressionValidatorMaxID1Inventory.Enabled = true;

                    RequiredFieldValidatorMaxID2Inventory.Enabled = true;
                    RegularExpressionValidatorMaxID2Inventory.Enabled = true;

                }
                //else
                //{
                //    if (ddlCustomerType.SelectedValue != "0")
                //    {
                //        lMaxIDPlusInventoryLimit.Visible = false;
                //        txtMaxIDPlusInventoryLimit.Visible = false;

                //        lMaxIDLinkInventoryLimit.Visible = false;
                //        txtMaxIDLinkInventoryLimit.Visible = false;

                //        lMaxIDLinkUSBInventoryLimit.Visible = false;
                //        txtMaxIDLinkUSBInventoryLimit.Visible = false;

                //        lMaxID1InventoryLimit.Visible = false;
                //        txtMaxID1InventoryLimit.Visible = false;

                //        lMaxID2InventoryLimit.Visible = false;
                //        txtMaxID2InventoryLimit.Visible = false;

                //        //RequiredFieldValidatorMaxInventory.ControlToValidate = "";
                //        //RequiredFieldValidatorMaxInventory.ValidationGroup = "";
                //        //RequiredFieldValidatorMaxInventory.Enabled = false;
                //        //RegularExpressionValidatorMaxInventory.Enabled = false;
                //        //RequiredFieldValidatorMaxInventory.Visible = false;
                //        //RegularExpressionValidatorMaxInventory.Visible = false;
                //    }
                //}
            }
            catch { }
        }
        else // if accountID is not null, preform "Update account"
        {
            AccountID = int.Parse(Page.Request.QueryString["ID"].ToString());
            btnSave.Visible = false;

            //verify accountID exist
            //this.idc = new InsDataContext ();
            try
            {
                //Get Account Info
                var acctCount = (from ac in idc.Accounts
                                 join ur in idc.Users on ac.AccountAdminID equals ur.UserID
                                 join lo in idc.Locations on ac.AccountID equals lo.AccountID
                                 where ac.AccountID == AccountID && lo.IsDefaultLocation == true
                                 select new
                                 {
                                     ac.AccountID,
                                     ac.AccountAdminID,
                                     lo.LocationID,
                                     ac.BrandSourceID,
                                     ac.UnixID,
                                     ac.Active
                                 }).FirstOrDefault();

                //If no admin user or location - display error message.
                if (acctCount == null)
                {
                    var acct = (from a in idc.Accounts where a.AccountID == AccountID select a).FirstOrDefault();
                    var loc = (from l in idc.Locations where l.AccountID == AccountID select l).FirstOrDefault();

                    //allow user to re-activate account.  This is required if account has no admin user. 
                    //(UserMaintenance screen).
                    this.btnDeactivate.Text = acct.Active ? " Deactivate" : " Re-Activate";

                    //No admin for this account?
                    if (acct.AccountAdminID == null)
                    {
                        this.displayError("This account has no Admin user.  Please set one.");

                        //Is account deactivated also?
                        if (!acct.Active)
                        {
                            this.displayError("This account has no Admin user and is not active.  " +
                                    "Please re-activate account and then set Admin user.");
                        }
                    }
                    else if (loc == null)  //No location set for this account?
                    {
                        this.displayError("This account has no location.  Please create one.");

                        //Is account deactivated also?
                        if (!acct.Active)
                        {
                            this.displayError("This account has no location and is not active.");
                        }
                    }

                    //disable buttons and do not allow user to update account.
                    btnUpdate.Visible = false;

                    return;
                }

                PageIsNotPostBackLoadInfo(acctCount.AccountID, int.Parse(acctCount.AccountAdminID.ToString()), acctCount.LocationID, acctCount.BrandSourceID.ToString(), acctCount.UnixID);
                //PageIsPostBackLoadInfo(acctCount.AccountID, int.Parse(acctCount.AccountAdminID.ToString()), acctCount.LocationID, acctCount.BrandSourceID.ToString(), acctCount.UnixID);

                //bypassed password validators
                RFV_password.Enabled = false;
                RFV_rePassword.Enabled = false;

                this.btnDeactivate.Text = acctCount.Active ? " Deactivate" : " Re-Activate";
                this.AccountToolBar.Visible = DisplayDeactivateButton();

                btnCancel.Text = "Back to Account";
                btnUpdate.Visible = true;

                string strDealerID = (ddlDealer.Visible == true) ? ddlDealer.SelectedValue : "";


                //11/2013 WK.
                //If IC Care and DealerID is present then load dealber address as billing address
                //and disable validation checks for billing - IC Care only.
                if (ddlBrand.SelectedValue == "3")
                {
                    if (!ddlDealer.SelectedItem.Text.Contains("Select"))
                        loadDealerBillingInfo(int.Parse(ddlDealer.SelectedValue));

                    ICCareBillingInfoValidation();

                    //*************************************************************
                    //12/2013 WK - Bug fix.  IC Care accounts are PO only!
                    //**************************************************************
                    divICCareDemoAccount.Visible = true;
                    chkICCareDemoAccount.Enabled = false;

                    divICCareSalesRepCommission.Visible = true;
                }

                //Unhide discounts grid until account is created.
                divAddAndSearchToolbar.Visible = true;
                gvDiscounts.Visible = true;

                if (!IsPostBack) loadDiscountGrid();

                //8/2013 WK - if inactive account - DO NOT allow adding of discounts and
                //  updates to account information!
                if (!acctCount.Active)
                {
                    btnUpdate.Enabled = false;
                    btnAddDiscount.Visible = false;
                }

                //******************************************************************************
                //10/2013 WK - determine if this account uses custom rate code or not.
                //******************************************************************************
                //   Implement this after 10/15/13 Instadose build
                //------------------------------------------------------------------------------
                var acctRate = (from r in idc.Rates
                                where r.CustomAccountID == AccountID
                                select r).FirstOrDefault();

                if (acctRate != null)
                {
                    //Yes!  set to this code and disable rate dropdown!
                    chkCustomRate.Checked = true;
                    chkCustomRate.Enabled = false;
                    ddlRateCode.SelectedValue = acctRate.RateID.ToString();
                    ddlRateCode.Enabled = false;
                }
                //******************************************************************************

                // make the add discount button invisible if customer type is CON
                if (ddlCustomerType.SelectedItem != null && ddlCustomerType.SelectedItem.Text.Trim() == "CON")
                {
                    //   btnAddDiscount.Visible = false;
                    //   udtpnlRateDetailsGridView.Visible = false;
                    udtpnlRateDetailsGridView.Visible = false;
                    divAddAndSearchToolbar.Visible = false;
                    ddlCustomerType.Enabled = false;
                    divMaxIDPlusInventoryLimit.Visible = true;
                    divMaxIDLinkInventoryLimit.Visible = true;
                    divMaxIDLinkUSBInventoryLimit.Visible = true;

                    //rbtnCC.Enabled = false;
                    ddlRateCode.Enabled = false;
                    chkCustomRate.Enabled = false;
                }

                // if not consignment type, remove the con from customer type dropdown
                if (ddlCustomerType.SelectedItem != null && ddlCustomerType.SelectedItem.Text.Trim() != "CON")
                {
                    ddlCustomerType.Items.Remove(ddlCustomerType.Items.FindByValue("47"));
                }

                ddlAccountType.Enabled = false;
            }

            catch
            {
                this.displayError("No such account found!");

                btnUpdate.Visible = false;
            }
        }  // end else
    }

    protected void PageIsNotPostBackLoadInfo(int AccountID, int UserID, int LocationID, string StrBrand, string strDealerID)
    {
        if (!IsPostBack)
        {
            //Reset stored session account discounts if new account.
            Session["Discounts"] = null;

            // Not page postback, Load all dropdownlist and display Main Location info
            LoadDDLCountry(ddlCountryB);
            LoadDDLCountry(ddlCountryS);
            
            LoadDDLReferral();
            LoadDDLSalesRep();
            LoadDDLCustomerType();

            LoadDDLAccountType();
            LoadDDLCustomerGroup();

            /// 8/2013 WK - New for Unix transition accounts.
            //loadDDLUnixCustomerType();
            // replace unixcustomertype with industryid 10/05/2017
            loadDDLIndustryType();


            LoadDDLRateCode(int.Parse(StrBrand));
            // Modified by Tdo, 4/2/2012. Must reload States by country
            //LoadDDLState(ddlStateB, "");
            //LoadDDLState(ddlStateS, "");            
            //LoadDDLCurrency();
            LoadDDLSecurityQuestion();
            LoadDDLPackageType();
            LoadDDLShippingOption();
            LoadDDLShippingCarrier();
            //LoadDDLLPIUserRole();
            LoadDDLSchedule();
            LoadDDLProductGroup();

            //set default value for ddl_SecurityQuestion and ddl_country
            //ddlSecurityQuestion.SelectedValue = "12";
            ddlCountryB.SelectedValue = "1"; // default USA
            ddlCountryS.SelectedValue = "1"; // Default USA
            ddlProductGroup.SelectedValue = "9"; //Default to IMI Instadose 1.0

            // Modified by Tdo, 4/2/2012. Must reload States by country
            LoadDDLState(ddlStateB, ddlCountryB.SelectedValue);
            LoadDDLState(ddlStateS, ddlCountryS.SelectedValue);

            LoadDDLLabel(ddlProductGroup.SelectedValue);
            // Set default label by ProductGroupID
            SetDefaultLabelByProductGroupID(ddlProductGroup.SelectedValue);

            LoadDDLDealer(ddlDealer);

            var mr = new MiscRequests();
            var defVal = mr.GetAppSetting("RenewalDaysPrior");
            if (string.IsNullOrEmpty(defVal))
                defVal = "60";

            // 10/3/2011 add brand
            if (StrBrand == "3") // where 3 is ICcare
            {
                ddlBrand.SelectedValue = StrBrand;
                EnableDealerFields(true);
                EnableBillingFields(false);

                // put it default value for IC CARE
                ddlReferral.SelectedValue = "D012";
                //ddlReferral.Enabled = false;

                //ddlCustomerType.SelectedValue = "DIST";
                ddlCustomerType.SelectedValue = "8";
                ddlCustomerType.Enabled = false;

                ddlBillingTerm.SelectedValue = "2";
                ddlBillingTerm.Enabled = false;

                txtBillPriorDay.Text = defVal;
                //ddlCurrency.SelectedValue = "USD";
                //ddlCurrency.Enabled = false;

                //12/2013 WK - IC Care is PO Account only.  NO credit cards!
                RequiredFieldValidatorPONumber.Enabled = false;
                reqfldvalFirstNameB.Enabled = false;
                reqfldvalLastNameB.Enabled = false;
            }
            else
            {
                txtBillPriorDay.Text = defVal;
                EnableDealerFields(false);
                EnableBillingFields(true);
                //ddlReferral.Enabled = true;
                ddlCustomerType.Enabled = true;
                //ddlBillingTerm.Enabled = true;
                //ddlCurrency.Enabled = true;
                RequiredFieldValidatorPONumber.Enabled = true;
                reqfldvalFirstNameB.Enabled = true;
                reqfldvalLastNameB.Enabled = true;
            }

            // Load Default Invoice Delivery Method
            if ((ddlCustomerType.SelectedItem != null && (ddlCustomerType.SelectedItem.Text.Trim() == "DEMO" || ddlCustomerType.SelectedItem.Text.Trim() == "TEST"))
                || (ddlCustomerGroup.SelectedItem != null && (ddlCustomerGroup.SelectedItem.Text.Trim().ToUpper() == "INTERNAL TEST" || ddlCustomerGroup.SelectedItem.Text.Trim().ToUpper() == "DEMO"))
                )
            {
                chkBoxInvDeliveryDoNotSend.Checked = true;
                chkBoxInvDeliveryDoNotSend.Enabled = true;
                chkBoxInvDeliveryPrintMail.Checked = false;
            }
            else
            {
                chkBoxInvDeliveryDoNotSend.Checked = false;
                chkBoxInvDeliveryDoNotSend.Enabled = false;
                chkBoxInvDeliveryPrintMail.Checked = true;
            }

            if (AccountID != 0 && UserID != 0 && LocationID != 0)
                LoadFillPageInfo(AccountID, UserID, LocationID);


            //**************************************************************************************
            //10/2013 W.Kakemoto
            //If Referral Code is Reseller (SalesRepDist.ChannellID = 8) AND Brand is not
            //IC Care, then load billing address from SalesRepDist and make it read only.

            var salesRep = (from a in idc.Accounts
                            join s in idc.SalesRepDistributors on a.ReferralCode equals s.SalesRepDistID
                            where s.ChannelID == 8 && a.AccountID == AccountID
                            select new { s }).FirstOrDefault();

            if (salesRep != null && ddlBrand.SelectedValue != "3") loadResellerBillingInfo(salesRep.s.SalesRepDistID);
            //**************************************************************************************

            if (AccountID <= 0)
            {
                ClientScript.RegisterStartupScript(Page.GetType(), "accountTypeChanged", "<script type='text/javascript'>accountTypeChanged()</script>");
            }
        }
    }

    protected void PageIsPostBackLoadInfo(int AccountID, int UserID, int LocationID, string StrBrand, string strDealerID)
    {
        if (IsPostBack)
        {
            //this.idc = new InsDataContext ();
            // find control name which has casued postback 
            string controlID = this.Request.Params["__EVENTTARGET"];
            //if (controlID.IndexOf("ddlCountryB") > 0)
            //{
            //    LoadDDLState(ddlStateB, ddlCountryB.SelectedValue);   // Reload all billing states by billing country
            //    LoadDDLState(ddlStateS, "0");  // Reload all states for shipping address when billing country changed. If we do not reload this ddl, the shipping state will be incorrect when user checks the check box The same as billing information.

            //    this.ddlCountryB.Focus();

            //}
            if (controlID.IndexOf("ddlCountryS") > 0)
            {
                LoadDDLState(ddlStateS, ddlCountryS.SelectedValue);
            }

            //if (controlID.IndexOf("ddlProductGroup") > 0)
            //{
            //    LoadDDLLabel(ddlProductGroup.SelectedValue);
            //    // Set default label by ProductGroupID
            //    SetDefaultLabelByProductGroupID(ddlProductGroup.SelectedValue);
            //}

            // Fill Distributor Billing info if there is a rateID assigned
            //if (controlID.IndexOf("ddlReferral") > 0)
            //{
            //    var DisBt = (from a in idc.SalesRepDistributors
            //                 join c in idc.CommissionSetups on a.SalesRepDistID equals c.SalesRepDistID
            //                 where a.SalesRepDistID == ddlReferral.SelectedValue.ToString()
            //                 select new { a, c.RateID }).FirstOrDefault();

            //    if (DisBt.RateID != null)
            //    {
            //        ddlRateCode.SelectedValue = DisBt.RateID.ToString();

            //        //Fill billing info w/ distributor info
            //        this.txtCompanyNameB.Text = DisBt.a.CompanyName;
            //        this.ddlGenderB.SelectedValue = DisBt.a.Prefix;
            //        this.txtFirstB.Text = DisBt.a.FirstName;
            //        this.txtLastB.Text = DisBt.a.LastName;
            //        this.txtAddress1B.Text = DisBt.a.Address1;
            //        this.txtAddress2B.Text = DisBt.a.Address2;
            //        this.txtAddress3B.Text = "";
            //        this.ddlCountryB.SelectedValue = DisBt.a.CountryID.ToString();
            //        // Modified by Tdo, 3/26/2013. Must reload States by country
            //        LoadDDLState(ddlStateB, ddlCountryB.SelectedValue);

            //        this.txtCityB.Text = DisBt.a.City;
            //        this.ddlStateB.SelectedValue = DisBt.a.StateID.ToString();
            //        this.txtPostalB.Text = DisBt.a.PostalCode;
            //        this.txtTelephoneB.Text = DisBt.a.Telephone;
            //        this.txtFaxB.Text = DisBt.a.Fax;

            //    }

            //    //else ddlRateCode.SelectedValue = "5";

            //    //txtDiscount.Text = CalculateDiscount();

            //}

            //if (controlID.IndexOf("txt_ServiceStartDate") > 0)
            //{
            //    try
            //    {
            //        DateTime mydate = DateTime.Parse(txt_ServiceStartDate.Text);

            //        //IC Care must start on 1st of the month.
            //        if (ddlBrand.SelectedValue != "3")  //Not IC Care.
            //            txt_ServiceEndDate.Text = (mydate.AddYears(1)).ToString("MM/dd/yyyy");
            //        else  //IC Care!
            //            txt_ServiceEndDate.Text = (mydate.AddYears(1)).ToString("MM/1/yyyy");
            //    }
            //    catch
            //    {
            //        RequiredFieldValidator_txt_ServiceStartDate.ErrorMessage = "*DateError";
            //        RequiredFieldValidator_txt_ServiceStartDate.IsValid = false;
            //    }
            //}

            //if (controlID.IndexOf("ddlBrand") > 0)
            //{
            //    StrBrand = ddlBrand.SelectedValue;
            //    Session["Brand"] = StrBrand;
            //    ddlBrand.SelectedValue = StrBrand;
            //    LoadDDLRateCode(int.Parse(StrBrand));
            //}
        }
    }

    protected void LoadFillPageInfo(int AccountID, int UserID, int LocationID)
    {
        try
        {
            var acctInfo = (from ac in idc.Accounts
                            where ac.AccountID == AccountID
                            select ac).FirstOrDefault();

            var UserInfo = (from u in idc.Users
                            join a in idc.AuthUsers on u.UserName equals a.UserName
                            where u.UserID == UserID
                            select new { u, a }).FirstOrDefault();

            var LocInfo = (from lo in idc.Locations
                           where lo.LocationID == LocationID
                           select lo).FirstOrDefault();

            var acctCCInfo = (from cc in idc.AccountCreditCards
                              join cct in idc.CreditCardTypes
                              on cc.CreditCardTypeID equals cct.CreditCardTypeID
                              where cc.AccountID == AccountID && cc.Active == true
                              select new { cc, cct }).FirstOrDefault();

            ddlBillingTerm.Enabled = false;
            ddlBrand.Enabled = false;
            // account information
            this.ddlBrand.SelectedValue = acctInfo.BrandSourceID.ToString();
            this.txtAccountName.Text = acctInfo.AccountName;
            this.ddlReferral.SelectedValue = acctInfo.ReferralCode;
            ddlSalesRep.SelectedValue = acctInfo.SalesRepID.ToString();
            this.txtCompanyName.Text = acctInfo.CompanyName;
            this.ddlCustomerType.SelectedValue = acctInfo.CustomerTypeID.ToString();

            ddlAccountType.SelectedValue = acctInfo.AccountTypeID.ToString();
            ddlCustomerGroup.SelectedValue = acctInfo.CustomerGroupID.ToString();

            chkICCareDemoAccount.Checked = acctInfo.isDemoAccount;
            chkICCareSalesRepCommission.Checked = acctInfo.isICCareCommEligible == true;

            // consignment customer settings logic
            if (this.ddlCustomerType.SelectedItem.Text.Trim() == "CON" && ddlReferral.SelectedValue == "0000")
                ddlReferral.Enabled = false;
            else
                ddlReferral.Enabled = true;

            //if (acctInfo.UnixCustomerTypeID != null)
            //    ddlUnixCustomerType.SelectedValue = acctInfo.UnixCustomerTypeID.ToString();
            //else
            //    ddlUnixCustomerType.SelectedValue = "0";

            if (acctInfo.IndustryID != null)
                ddlIndustryType.SelectedValue = acctInfo.IndustryID.ToString();
            else
                ddlIndustryType.SelectedValue = "0";

            ddlPaymentTerm.SelectedValue = acctInfo.PaymentTerms.ToString();

            this.ddlDealer.SelectedValue = acctInfo.UnixID;

            //this.ddlRateCode.SelectedValue = (acctInfo.Rate!=null)?acctInfo.Rate.ToString():"0";

            if (ddlRateCode.SelectedItem.Text.Contains("Select"))
            {
                this.ddlRateCode.SelectedValue = (acctInfo.DefaultRateID != null) ? acctInfo.DefaultRateID.ToString() : "0";

                //9/2013 WK - check rate code to see if is current (not expired) and active.
                checkRateCode(acctInfo.DefaultRateID);
            }
            else
            {
                int? rateID = int.Parse(ddlRateCode.SelectedValue);
                checkRateCode(rateID);
            }

            this.txtDiscount.Text = (acctInfo.Discount != null) ? acctInfo.Discount.ToString() : "0";

            this.ddlPackageType.SelectedValue = (acctInfo.PackageTypeID != null) ? acctInfo.PackageTypeID.ToString() : "0";
            this.ddlShippingOption.SelectedValue = (acctInfo.ShippingOptionID != null) ? acctInfo.ShippingOptionID.ToString() : "0";
            this.ddlShippingCarrier.SelectedValue = (acctInfo.ShippingCarrier != null && acctInfo.ShippingCarrier.Length > 0) ? acctInfo.ShippingCarrier : "-- Select Shipping Carrier --";
            this.txtSpecialInstruction.Text = (acctInfo.SpecialInstructions != null) ? acctInfo.SpecialInstructions.ToString() : "";

            this.chkBoxIncludeLPI.Checked = acctInfo.IncludeLPI;
            //this.ddlLPIUserRole.SelectedValue = (acctInfo.LPIUserRoleID != null) ? acctInfo.LPIUserRoleID.ToString() : "0";
            this.ddlSchedule.SelectedValue = (acctInfo.ScheduleID != null) ? acctInfo.ScheduleID.ToString() : "0";
            this.ddlProductGroup.SelectedValue = (acctInfo.WebProductGroupID != null) ? acctInfo.WebProductGroupID.ToString() : "0";
            LoadDDLLabel(this.ddlProductGroup.SelectedValue);

            // ------ Set default ProductGroupID and LabelID based upon what has been set in Account ------//
            var AccountPGDetail = (from acct in idc.AccountPGDetails
                                   where acct.AccountID == AccountID
                                   select acct).FirstOrDefault();
            if (AccountPGDetail != null)
            {
                this.ddlProductGroup.SelectedValue = AccountPGDetail.ProductGroupID.ToString();
                LoadDDLLabel(this.ddlProductGroup.SelectedValue);
                this.ddlLabel.SelectedValue = AccountPGDetail.LabelID.ToString();
                this.chkBoxDeviceAssign.Checked = AccountPGDetail.OrderDevAssign;
                this.chkBoxDeviceInitialize.Checked = AccountPGDetail.OrderDevInitialize;
            }
            // ------ Set default ProductGroupID and LabelID based upon what has been set in Account ------//

            // accoutn Admin information
            this.txtLoginid.Text = UserInfo.u.UserName;
            this.ddlSecurityQuestion.SelectedValue = (UserInfo.a.SecurityQuestionID1 == null ? "0" : UserInfo.a.SecurityQuestionID1.ToString());
            //If security question does not exist then look up from old ones
            if (this.ddlSecurityQuestion.SelectedValue == "0" && UserInfo.a.SecurityQuestionID1.HasValue)
            {
                this.ddlSecurityQuestion.SelectedItem.Text = UserInfo.a.SecurityQuestion1.SecurityQuestionText;
                this.ddlSecurityQuestion.SelectedItem.Value = UserInfo.a.SecurityQuestion1.SecurityQuestionID.ToString();
            }
            this.txtSecurityA.Text = (UserInfo.a.SecurityAnswer1 == null ? "" : UserInfo.a.SecurityAnswer1.ToString());
            this.ddlGenderA.SelectedValue = UserInfo.u.Prefix;
            this.txtFirstA.Text = UserInfo.u.FirstName;
            this.txtLastA.Text = UserInfo.u.LastName;
            this.txtEmail.Text = UserInfo.u.Email;
            this.txtTelephone.Text = UserInfo.u.Telephone;
            this.txtFax.Text = UserInfo.u.Fax;
            if (UserInfo.u.Gender.ToString() == "F") this.rbtnFemale.Checked = true;

            //Billing Information
            this.txtCompanyNameB.Text = LocInfo.BillingCompany;
            this.ddlGenderB.SelectedValue = LocInfo.BillingNamePrefix;
            this.txtFirstB.Text = LocInfo.BillingFirstName;
            this.txtLastB.Text = LocInfo.BillingLastName;
            this.txtAddress1B.Text = LocInfo.BillingAddress1;
            this.txtAddress2B.Text = LocInfo.BillingAddress2;
            this.txtAddress3B.Text = LocInfo.BillingAddress3;
            this.ddlCountryB.SelectedValue = LocInfo.BillingCountryID.ToString();
            // Modified by Tdo, 4/2/2012. Must reload States by country
            LoadDDLState(ddlStateB, ddlCountryB.SelectedValue);

            this.txtCityB.Text = LocInfo.BillingCity;
            this.ddlStateB.SelectedValue = LocInfo.BillingStateID.ToString();
            this.txtPostalB.Text = LocInfo.BillingPostalCode;
            this.txtTelephoneB.Text = LocInfo.BillingTelephone;
            this.txtFaxB.Text = LocInfo.BillingFax;
            this.txtEmailAddressB.Text = LocInfo.BillingEmailAddress;

            //Shipping Information
            this.txtCompanyNameS.Text = LocInfo.ShippingCompany;
            this.ddlGenderS.SelectedValue = LocInfo.ShippingNamePrefix;
            this.txtFirstS.Text = LocInfo.ShippingFirstName;
            this.txtLastS.Text = LocInfo.ShippingLastName;
            this.txtAddress1S.Text = LocInfo.ShippingAddress1;
            this.txtAddress2S.Text = LocInfo.ShippingAddress2;
            this.txtAddress3S.Text = LocInfo.ShippingAddress3;
            this.ddlCountryS.SelectedValue = LocInfo.ShippingCountryID.ToString();
            // Modified by Tdo, 4/2/2012. Must reload States by country
            LoadDDLState(ddlStateS, ddlCountryS.SelectedValue);

            this.txtCityS.Text = LocInfo.ShippingCity;
            this.ddlStateS.SelectedValue = LocInfo.ShippingStateID.ToString();
            this.txtPostalS.Text = LocInfo.ShippingPostalCode;
            this.txtTelephoneS.Text = LocInfo.ShippingTelephone;
            this.txtFaxS.Text = LocInfo.ShippingFax;
            this.txtEmailAddressS.Text = LocInfo.ShippingEmailAddress;

            var mr = new MiscRequests();
            var defVal = mr.GetAppSetting("RenewalDaysPrior");
            if (string.IsNullOrEmpty(defVal))
                defVal = "60";

            //Billing Method
            this.txtBillPriorDay.Text = (acctInfo.BillingDaysPrior != null) ? acctInfo.BillingDaysPrior.ToString() : defVal;

            //this.ddlCurrency.SelectedValue =(from c in idc.Countries where c.CountryID == LocInfo.BillingCountryID select c.CurrencyCode).First();

            // load account currency value
            // ddlCurrency.SelectedValue = acctInfo.CurrencyCode;

            txtPOno.Text = acctInfo.RenewalPONumber;
            txtPOBeginDate.Text = acctInfo.RenewalPOStartDate.HasValue ? ((DateTime)(acctInfo.RenewalPOStartDate)).ToString("MM/dd/yyyy") : null;
            txtPOEndDate.Text = acctInfo.RenewalPOEndDate.HasValue ? ((DateTime)(acctInfo.RenewalPOEndDate)).ToString("MM/dd/yyyy") : null;

            // customer type consignment rules
            if (ddlCustomerType.SelectedItem.Text.Trim() == "CON") // && acctInfo.MaxQtyConsignBadges != null)
            {
                var MaxIDPlusInfo = (from max in idc.ConsignmentInventoryLimits
                                     where max.AccountID == AccountID && max.ProductGroupID == 10   // 10 is idplus productgroupid
                                     select max).FirstOrDefault();
                if (MaxIDPlusInfo != null)
                    txtMaxIDPlusInventoryLimit.Text = MaxIDPlusInfo.MaxQuantity.ToString();


                var MaxIDLinkInfo = (from max in idc.ConsignmentInventoryLimits
                                     where max.AccountID == AccountID && max.ProductGroupID == 5   // 5 is idlink productgroupid
                                     select max).FirstOrDefault();
                if (MaxIDLinkInfo != null)
                    txtMaxIDLinkInventoryLimit.Text = MaxIDLinkInfo.MaxQuantity.ToString();


                var MaxIDLinkUSBInfo = (from max in idc.ConsignmentInventoryLimits
                                        where max.AccountID == AccountID && max.ProductGroupID == 6   // 6 is idlink USB productgroupid
                                        select max).FirstOrDefault();
                if (MaxIDLinkUSBInfo != null)
                    txtMaxIDLinkUSBInventoryLimit.Text = MaxIDLinkUSBInfo.MaxQuantity.ToString();

                var MaxID1Info = (from max in idc.ConsignmentInventoryLimits
                                  where max.AccountID == AccountID && max.ProductGroupID == 9   // 9 is id1 productgroupid
                                  select max).FirstOrDefault();
                if (MaxID1Info != null)
                    txtMaxID1InventoryLimit.Text = MaxID1Info.MaxQuantity.ToString();

                var MaxID2Info = (from max in idc.ConsignmentInventoryLimits
                                  where max.AccountID == AccountID && max.ProductGroupID == 11   // 11 is id2 productgroupid
                                  select max).FirstOrDefault();
                if (MaxID2Info != null)
                    txtMaxID2InventoryLimit.Text = MaxID2Info.MaxQuantity.ToString();

            }
            
            if (ddlCustomerType.SelectedItem.Text.Trim() == "CON")
            {
                RequiredFieldValidatorMaxIDPlusInventory.Enabled = true;
                RegularExpressionValidatorMaxIDPlusInventory.Enabled = true;

                RequiredFieldValidatorMaxIDLinkInventory.Enabled = true;
                RegularExpressionValidatorMaxIDLinkInventory.Enabled = true;

                RequiredFieldValidatorMaxIDLinkUSBInventory.Enabled = true;
                RegularExpressionValidatorMaxIDLinkUSBInventory.Enabled = true;


                RequiredFieldValidatorMaxID1Inventory.Enabled = true;
                RegularExpressionValidatorMaxID1Inventory.Enabled = true;

                RequiredFieldValidatorMaxID2Inventory.Enabled = true;
                RegularExpressionValidatorMaxID2Inventory.Enabled = true;
            }
            else
            {
                divMaxIDPlusInventoryLimit.Visible = false;
                divMaxID1InventoryLimit.Visible = false;
                divMaxIDLinkInventoryLimit.Visible = false;
                divMaxIDLinkUSBInventoryLimit.Visible = false;
                divMaxID2InventoryLimit.Visible = false;
                //lMaxInventoryLimit.Visible = false;
                //txtMaxInventoryLimit.Visible = false;
            }

            this.ddlBillingTerm.SelectedValue = acctInfo.BillingTermID.ToString();

            //1/2014 WK - Set location billing if necessary.
            chkUseLocationBilling.Checked = acctInfo.UseLocationBilling;

            if (acctInfo.ContractStartDate != null)
                txt_ServiceStartDate.Text = ((DateTime)(acctInfo.ContractStartDate)).ToString("MM/dd/yyyy");

            if (acctInfo.ContractEndDate != null)
                txt_ServiceEndDate.Text = ((DateTime)(acctInfo.ContractEndDate)).ToString("MM/dd/yyyy");
            //txt_Startdate.Enabled = false;
            //txt_Enddate.Enabled = false;

            // disable Service Start and End date, if order exist
            txt_ServiceStartDate.Enabled = !isOrderExist();
            
            // Load Invoice Delivery Mothod
             if (acctInfo.BillingGroup != null)
            {
                chkBoxInvDeliveryPrintMail.Checked = acctInfo.BillingGroup.useMail;

                if (acctInfo.BillingGroup.useEmail1 || acctInfo.BillingGroup.useEmail2)
                {
                    chkBoxInvDeliveryEmail.Checked = true;
                    //txtInvDeliveryPrimaryEmail.Enabled = true;
                    //txtInvDeliverySecondaryEmail.Enabled = true;
                    //txtInvDeliveryPrimaryEmail.Text = acctInfo.BillingGroup.Email1;
                    //txtInvDeliverySecondaryEmail.Text = acctInfo.BillingGroup.Email2;
                }
                else
                {
                    chkBoxInvDeliveryEmail.Checked = false;
                    //txtInvDeliveryPrimaryEmail.Enabled = false;
                    //txtInvDeliverySecondaryEmail.Enabled = false;
                    //txtInvDeliveryPrimaryEmail.Text = "";
                    //txtInvDeliverySecondaryEmail.Text = "";
                }
                txtInvDeliveryPrimaryEmail.Text = string.IsNullOrEmpty(acctInfo.BillingGroup.Email1) ? "" : acctInfo.BillingGroup.Email1;
                txtInvDeliverySecondaryEmail.Text = string.IsNullOrEmpty(acctInfo.BillingGroup.Email2) ? "" : acctInfo.BillingGroup.Email2;

                if (acctInfo.BillingGroup.useFax)
                {
                    chkBoxInvDeliveryFax.Checked = true;
                    //txtInvDeliveryPrimaryFax.Enabled = true;
                    //txtInvDeliveryPrimaryFax.Text = acctInfo.BillingGroup.Fax;
                }
                else
                {
                    chkBoxInvDeliveryFax.Checked = false;
                    //txtInvDeliveryPrimaryFax.Enabled = false;
                    //txtInvDeliveryPrimaryFax.Text = "";
                }
                txtInvDeliveryPrimaryFax.Text = string.IsNullOrEmpty(acctInfo.BillingGroup.Fax) ? "" : acctInfo.BillingGroup.Fax;

                chkBoxInvDeliveryEDI.Checked = acctInfo.BillingGroup.useEDI;
                string clientName = "";
                if (acctInfo.BillingGroup.EDIClientID.HasValue)
                {
                    clientName = idc.Clients.Where(d => d.ClientID == acctInfo.BillingGroup.EDIClientID.Value).Select(d => d.ClientName).FirstOrDefault();
                }
                txtInvDeliveryEDIClientID.Text = clientName;

                if (acctInfo.BillingGroup.useSpecialDelivery)
                {
                    chkBoxInvDeliveryUpload.Checked = true;
                    //fileUploadInvDeliveryUpload.Enabled = true;
                    //txtInvDeliveryUploadInstruction.Enabled = true;
                    //txtInvDeliveryUploadInstruction.Text = acctInfo.BillingGroup.SpecialDeliveryText;
                }
                else
                {
                    chkBoxInvDeliveryUpload.Checked = false;
                    //fileUploadInvDeliveryUpload.Enabled = false;
                    //txtInvDeliveryUploadInstruction.Enabled = false;
                    //txtInvDeliveryUploadInstruction.Text = "";
                }
                txtInvDeliveryUploadInstruction.Text = string.IsNullOrEmpty(acctInfo.BillingGroup.SpecialDeliveryText) ? "" : acctInfo.BillingGroup.SpecialDeliveryText;

                chkBoxInvDeliveryDoNotSend.Checked = !acctInfo.BillingGroup.DeliverInvoice;
                if (RestrictControls())
                {
                    chkBoxInvDeliveryDoNotSend.Enabled = true;
                }
                else
                {
                    chkBoxInvDeliveryDoNotSend.Enabled = false;
                }                
            }
        }
        catch
        {
            this.displayError("Error found, No such account found!");
        }

    }

    // put default value for IC CARE
    protected void ddlBrand_SelectedIndexChanged(object sender, EventArgs e)
    {
        DropDownList ddl = (DropDownList)sender;

        LoadDDLRateCode(int.Parse(ddl.SelectedValue));

        var mr = new MiscRequests();
        var defVal = mr.GetAppSetting("RenewalDaysPrior");
        if (string.IsNullOrEmpty(defVal))
            defVal = "60";

        if (ddlBrand.SelectedValue == "3") // where 3 is ICcare
        {
            EnableDealerFields(true);
            EnableBillingFields(false);
            ddlReferral.SelectedValue = "D012";

            ddlAccountType.SelectedValue = "2"; // account type "Distributor"
            ddlAccountType.Enabled = false;

            ddlCustomerType.SelectedValue = "8"; // where 8 = "DIST"
            ddlCustomerType.Enabled = false;

            ddlBillingTerm.SelectedValue = "2";
            ddlBillingTerm.Enabled = false;

            txtBillPriorDay.Text = defVal;
            //ddlCurrency.SelectedValue = "USD";
            //ddlCurrency.Enabled = false;

            RequiredFieldValidatorPONumber.Enabled = false;
            reqfldvalFirstNameB.Enabled = false;
            reqfldvalLastNameB.Enabled = false;

            divMaxIDPlusInventoryLimit.Visible = false;
            divMaxID1InventoryLimit.Visible = false;
            divMaxIDLinkInventoryLimit.Visible = false;
            divMaxIDLinkUSBInventoryLimit.Visible = false;
            divMaxID2InventoryLimit.Visible = false;

            divICCareDemoAccount.Visible = true;

            divICCareSalesRepCommission.Visible = true;
        }
        else
        {
            ddlReferral.SelectedValue = "0";

            ddlAccountType.Enabled = true;
            ddlAccountType.SelectedIndex = 0;

            //ddlCustomerType.SelectedValue = "1";
            ddlCustomerType.SelectedIndex = 0;
            ddlCustomerType.Enabled = true;

            ddlBillingTerm.SelectedValue = "0";
            ddlBillingTerm.Enabled = true;

            txtBillPriorDay.Text = defVal;

            EnableDealerFields(false);
            EnableBillingFields(true);
            //ddlCurrency.Enabled = true;
            RequiredFieldValidatorPONumber.Enabled = true;
            reqfldvalFirstNameB.Enabled = true;
            reqfldvalLastNameB.Enabled = true;

            divMaxIDPlusInventoryLimit.Visible = true;
            divMaxID1InventoryLimit.Visible = true;
            divMaxIDLinkInventoryLimit.Visible = true;
            divMaxIDLinkUSBInventoryLimit.Visible = true;
            divMaxID2InventoryLimit.Visible = true;

            divMaxIDPlusInventoryLimit.Style.Add("display", "none");
            divMaxID1InventoryLimit.Style.Add("display", "none");
            divMaxIDLinkInventoryLimit.Style.Add("display", "none");
            divMaxIDLinkUSBInventoryLimit.Style.Add("display", "none");
            divMaxID2InventoryLimit.Style.Add("display", "none");

            divICCareDemoAccount.Visible = false;

            divICCareSalesRepCommission.Visible = false;
        }

    }

    protected void ddlProductGroup_SelectedIndexChanged(object sender, EventArgs e)
    {
        LoadDDLLabel(ddlProductGroup.SelectedValue);
        // Set default label by ProductGroupID
        SetDefaultLabelByProductGroupID(ddlProductGroup.SelectedValue);
    }

    protected void ddlRateCode_SelectedIndexChanged(object sender, EventArgs e)
    {
        int? rateID = 0;

        if (!ddlRateCode.SelectedItem.Text.Contains("Select"))
        {
            rateID = int.Parse(ddlRateCode.SelectedValue);
            checkRateCode(rateID);
        }
        else
            this.lblInvalidRate.Text = "Please select rate code.";

    }

    protected void chkCustomRate_CheckedChanged(object sender, EventArgs e)
    {
        int brandSourceID = int.Parse(ddlBrand.SelectedValue.ToString());

        LoadDDLRateCode(brandSourceID);
    }

    protected void txtDiscount_TextChanged(object sender, EventArgs e)
    {
        decimal discountRate = 0;

        if (txtDiscount.Text.Length > 6)
        {
            displayError("Invalid discount rate.");
        }

        if (decimal.TryParse(txtDiscount.Text, out discountRate))
        {
            if (discountRate > 100)
                VisibleErrors_discountsDialog("Invalid discount rate.");
        }
    }

    protected void txt_ServiceStartDate_TextChanged(object sender, EventArgs e)
    {
        try
        {
            //IC Care service start must be on the 1st of the month!
            if (ddlBrand.SelectedValue == "3")
                txt_ServiceStartDate.Text = string.Format("{0:MM/1/yyyy}",
                                                DateTime.Parse(txt_ServiceStartDate.Text));

            DateTime mydate = DateTime.Parse(txt_ServiceStartDate.Text);

            ////IC Care must start on 1st of the month.
            //if (ddlBrand.SelectedValue != "3")  //Not IC Care.
            //    txt_ServiceEndDate.Text = (mydate.AddYears(1)).ToString("MM/dd/yyyy");
            //else  //IC Care!
            //    txt_ServiceEndDate.Text = (mydate.AddYears(1)).ToString("MM/1/yyyy");

            txt_ServiceEndDate.Text = mydate.AddDays(-1).AddYears(1).ToString("MM/dd/yyyy");
        }
        catch
        {
            RequiredFieldValidator_txt_ServiceStartDate.ErrorMessage = "*DateError";
            RequiredFieldValidator_txt_ServiceStartDate.IsValid = false;
        }
    }

    #endregion

    /// <summary>
    /// Form's "Save" "Update" and "Cancel" _click event
    /// </summary>
    #region ButtonClickEvent


    /// <summary>
    /// Cancel Order entry, redirect back to Account Info page
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnCancel_Click(object sender, EventArgs e)
    {

        if (Request.QueryString["ID"] == null)
        {
            Page.Response.Redirect("../InformationFinder/Default.aspx");
        }
        else
        {
            AccountID = int.Parse(Page.Request.QueryString["ID"].ToString());
            Page.Response.Redirect("../InformationFinder/Details/Account.aspx?ID=" + this.AccountID.ToString());
        }

    }

    /// <summary>
    /// Save button_click Action 
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (!validPODates())
        {
            return;
        }

        if (!chkBoxInvDeliveryPrintMail.Checked && !chkBoxInvDeliveryEmail.Checked && !chkBoxInvDeliveryFax.Checked && !chkBoxInvDeliveryUpload.Checked && !chkBoxInvDeliveryDoNotSend.Checked)
        {
            //=-=-=-= Show Error message in validation summary.
            CustomValidator val = new CustomValidator();
            val.ErrorMessage = string.Format("{0} - {1}", "Invoice Delivery Method", "Must check at least one of delivery method check box.");
            val.IsValid = false;
            val.ValidationGroup = "CSRegisterForm";
            this.Page.Validators.Add(val);

            return;
        }

        //idc = new InsDataContext ();
        btnSave.Enabled = false;        

        // Check if UserName has been used by other user
        int LoginIDcount = CheckUserNameAvailability(this.txtLoginid.Text.Trim(), UserID);

        bool isShippingCountry_StateMatched = IsShippingCountryStateMatched();

        bool isValidConsignmentAccount = true;

        // Check if Referral selected.
        string mySalesRepDistID = this.ddlReferral.SelectedValue;

        bool isValidInvoiceDeliveryMethodFax = true;
        bool isValidInvoiceDeliveryMethodPrimaryEmail = true;

        if (chkBoxInvDeliveryEmail.Checked && string.IsNullOrEmpty(txtInvDeliveryPrimaryEmail.Text.Trim()))
            isValidInvoiceDeliveryMethodPrimaryEmail = false;

        if (chkBoxInvDeliveryFax.Checked && string.IsNullOrEmpty(txtInvDeliveryPrimaryFax.Text.Trim()))
            isValidInvoiceDeliveryMethodFax = false;

        if (ddlCustomerType.SelectedItem.Text.Trim() == "CON")
        {  // validation if account type is consignment type
            isValidConsignmentAccount = validateConsignmentAccount();
        }

        if (LoginIDcount == 0 && mySalesRepDistID != "0" && isShippingCountry_StateMatched && isValidConsignmentAccount && isValidInvoiceDeliveryMethodFax && isValidInvoiceDeliveryMethodPrimaryEmail)
        {
            //Default timezone to 5.
            int TimeZoneID = 5;
            double TimeZoneHr = 0;
            int SelectBrandSource = int.Parse(ddlBrand.SelectedValue);
            //Grab the shipping state to find the timezone.
            var ShipState = (from s in idc.States where s.StateID == int.Parse(ddlStateS.SelectedValue) select s).FirstOrDefault();
            TimeZoneID = (ShipState != null) ? ShipState.TimeZoneID : 5;
            //TimeZoneHr = (ShipState.TimeZone.HasValue) ? ShipState.TimeZone.Value  : 0;

            //Default UOM to 'mrem'
            string UOM = "mrem";
            //Grab the shipping country to find the UOM
            var ShipCountry = (from c in idc.Countries where c.CountryID == int.Parse(ddlCountryS.SelectedValue) select c).First();
            UOM = ShipCountry.DefaultUOM.ToString();

            // save data to accounts table
            int NewAccountID = SaveToAccount(TimeZoneID, TimeZoneHr, UOM);
            AccountID = NewAccountID;

            // save data to location table
            if (NewAccountID != 0)
            {
                //save/generate schedule billing info if billing term = Quarterly
                if (ddlBillingTerm.SelectedValue == "1")
                {
                    DateTime myStartDate = DateTime.Parse(txt_ServiceStartDate.Text.Trim());
                    int myBillingDatePiror = int.Parse(txtBillPriorDay.Text.Trim());
                    //int myBillingSchedulePeriod = 91;

                    bool boolScheduleBilling = false;

                    //1/2014 WK - If new account (no orders) then create Schedule billing records.
                    if (isNewAccount(NewAccountID))
                    {
                        //boolScheduleBilling = SaveScheduledBilling(NewAccountID, myStartDate,
                        //                            myBillingDatePiror, myBillingSchedulePeriod);
                        boolScheduleBilling = SaveScheduledBilling(NewAccountID, myStartDate);

                        if (boolScheduleBilling == false)
                            this.displayError(this.errorMsg.InnerText + "Schedule Billing failed.");
                    }
                }
                else //If yearly, then delete any new (OrderID = null) scheduled billing records.
                {
                    bool boolScheduleBilling = deleteScheduledBilling(NewAccountID);

                    if (boolScheduleBilling == false)
                        this.displayError(this.errorMsg.InnerText + "Deleting previous schedule billing records failed.");
                }

                int NewLocationID = SaveToLocation(NewAccountID, TimeZoneID, TimeZoneHr, UOM);
                if (NewLocationID != 0)
                {
                    //save data to Users table, and update Account & Location with newUserID
                    int NewUserID = SaveToUser(NewAccountID, NewLocationID);

                    if (NewUserID != 0)
                    {
                        //Send email to new account user? ****
                        bool boolSendRegisterEmail = SendEmailToNewUser(NewAccountID, NewUserID, SelectBrandSource);
                        if (boolSendRegisterEmail == false)
                            this.displayError(this.errorMsg.InnerText + "New account registeration email failed.");

                        //Save stored session account discounts.
                        saveToAccountDiscount(NewAccountID);

                        //Save data to Mas200 ToMasAR_Customer
                        bool boolToMas = SaveToTOmasAR_Customer(NewAccountID, NewUserID);
                        if (boolToMas == false)
                            this.displayError(this.errorMsg.InnerText + "MAS Account creation errors.");

                        // Display register confirmation session
                        btnSave.Visible = false;
                        ConstructRegisterHashTable();
                        DisplayRegisterConfirmation(NewAccountID, "Thank you, account has been created!");

                    }
                    else
                    {
                        this.displayError(this.errorMsg.InnerText + "User creation errors.");
                    }//end if userid = 0
                }
                else
                {
                    this.displayError(this.errorMsg.InnerText + "Location creation errors.");
                }//end if locationid = 0

                int returnBillingGroupID = SaveBillingGroup(true, NewAccountID);

                if (returnBillingGroupID == 0)
                    this.displayError(this.errorMsg.InnerText + "Billing Group creation errors.");

                // create consignmentInventoryMax records
                if (ddlCustomerType.SelectedItem.Text.Trim() == "CON")
                {
                    int iresult = 0;
                    int iMaxIDPlusInventoryLimit = (this.txtMaxIDPlusInventoryLimit.Text.Trim() == "") ? 0 :
                        int.Parse(this.txtMaxIDPlusInventoryLimit.Text.Trim());
                    int iProductGroupID = 10;   // 10 is IDPlus productGroupID

                    iresult = SaveToConsignmentInventoryLimits(NewAccountID, iProductGroupID, iMaxIDPlusInventoryLimit);
                    if (iresult == 0)
                        this.displayError(this.errorMsg.InnerText + "Account creation error : unable to add Max ID Plus Inventory Limit amount!");


                    int iMaxID1InventoryLimit = (this.txtMaxID1InventoryLimit.Text.Trim() == "") ? 0 :
                    int.Parse(this.txtMaxID1InventoryLimit.Text.Trim());
                    iProductGroupID = 9;   // 9 is ID 1 productGroupID
                    iresult = SaveToConsignmentInventoryLimits(NewAccountID, iProductGroupID, iMaxID1InventoryLimit);
                    if (iresult == 0)
                        this.displayError(this.errorMsg.InnerText + "Account creation error : unable to add Max ID 1 Inventory Limit amount!");


                    int iMaxIDLinkInventoryLimit = (this.txtMaxIDLinkInventoryLimit.Text.Trim() == "") ? 0 :
                    int.Parse(this.txtMaxIDLinkInventoryLimit.Text.Trim());
                    iProductGroupID = 5;   // 5 is ID Link productGroupID
                    iresult = SaveToConsignmentInventoryLimits(NewAccountID, iProductGroupID, iMaxIDLinkInventoryLimit);
                    if (iresult == 0)
                        this.displayError(this.errorMsg.InnerText + "Account creation error : unable to add Max ID Link Inventory Limit amount!");


                    int iMaxIDLinkUSBInventoryLimit = (this.txtMaxIDLinkUSBInventoryLimit.Text.Trim() == "") ? 0 :
                        int.Parse(this.txtMaxIDLinkUSBInventoryLimit.Text.Trim());
                    iProductGroupID = 6;   // 6 is ID Link USB productGroupID
                    iresult = SaveToConsignmentInventoryLimits(NewAccountID, iProductGroupID, iMaxIDLinkUSBInventoryLimit);
                    if (iresult == 0)
                        this.displayError(this.errorMsg.InnerText + "Account creation error : unable to add Max ID Link USB Inventory Limit amount!");



                    int iMaxID2InventoryLimit = (this.txtMaxID2InventoryLimit.Text.Trim() == "") ? 0 :
                                        int.Parse(this.txtMaxID2InventoryLimit.Text.Trim());
                    iProductGroupID = 11;   // 11 is ID 2 productGroupID
                    iresult = SaveToConsignmentInventoryLimits(NewAccountID, iProductGroupID, iMaxID2InventoryLimit);
                    if (iresult == 0)
                        this.displayError(this.errorMsg.InnerText + "Account creation error : unable to add Max ID 2 Inventory Limit amount!");
                }

                // create Account Contract
                Mirion.DSD.GDS.API.AccountContractRequests accountContractReq = new Mirion.DSD.GDS.API.AccountContractRequests();
                DateTime contractStartDt = DateTime.Parse(txt_ServiceStartDate.Text);
                DateTime contractEndDt = contractStartDt.AddMonths(12).AddDays(-1); //DateTime.Parse(txt_ServiceEndDate.Text);
                int? brandSourceId = null;
                if (int.TryParse(ddlBrand.SelectedValue, out int tmpBrandSourceId))
                    brandSourceId = tmpBrandSourceId;

                accountContractReq.InsertInstaAccountContract(NewAccountID, contractStartDt, contractEndDt, brandSourceId);
            }
            else
            {
                this.displayError(this.errorMsg.InnerText + "Account creation errors.");
            }//end if accountid=0
        } // LoginID Check count
        else
        {
            if (LoginIDcount != 0)
                this.CustomValidator1.IsValid = false;
            if (!isShippingCountry_StateMatched)
                this.CustomValidator2.IsValid = false;
            //if (mySalesRepDistID == "0")
            //    this.CustomValidator2.IsValid = false;
            if (!isValidInvoiceDeliveryMethodFax)
                RequiredFieldValidatorInvDeliveryPrimaryFax.IsValid = false;

            if (!isValidInvoiceDeliveryMethodPrimaryEmail)
                RequiredFieldValidatorInvDeliveryPrimaryEmail.IsValid = false;
        }

        btnSave.Enabled = true;
    }

    // validate Consignment account
    protected bool validateConsignmentAccount()
    {
        bool isvalid = true;

        if (ddlBrand.SelectedItem.Value != "2")   // mirion
        {
            isvalid = false;
            this.displayError(this.errorMsg.InnerText + "Error: for Customer Type = CON, the Brand Source must = Mirion!");
        }

        if (ddlReferral.SelectedValue != "0000")   //  house account
        {
            isvalid = false;
            this.displayError(this.errorMsg.InnerText + "Error: for Customer Type = CON, the Referral must = 0000 House Account!");
        }

        return isvalid;
    }


    /// <summary>
    /// Update button_click Action
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnUpdate_Click(object sender, EventArgs e)
    {
        if (!validPODates())
        {
            return;
        }

        if (!chkBoxInvDeliveryPrintMail.Checked && !chkBoxInvDeliveryEmail.Checked && !chkBoxInvDeliveryFax.Checked && !chkBoxInvDeliveryUpload.Checked && !chkBoxInvDeliveryDoNotSend.Checked)
        {
            //=-=-=-= Show Error message in validation summary.
            CustomValidator val = new CustomValidator();
            val.ErrorMessage = string.Format("{0} - {1}", "Invoice Delivery Method", "Must check at least one of delivery method check box.");
            val.IsValid = false;
            val.ValidationGroup = "CSRegisterForm";
            this.Page.Validators.Add(val);
            return;
        }

        AccountID = int.Parse(Page.Request.QueryString["ID"].ToString());
        //verify accountID exist
        try
        {
            var acctCount = (from ac in idc.Accounts
                             join ur in idc.Users on ac.AccountAdminID equals ur.UserID
                             join lo in idc.Locations on ac.AccountID equals lo.AccountID
                             where ac.AccountID == AccountID && lo.IsDefaultLocation == true
                             select new
                             {
                                 ac.AccountID,
                                 ac.AccountAdminID,
                                 lo.LocationID,
                                 ac.BrandSourceID,
                                 ac.UnixID
                             }).FirstOrDefault();

            LocationID = acctCount.LocationID;
            UserID = int.Parse(acctCount.AccountAdminID.ToString());                        

            int LoginIDcount = CheckUserNameAvailability(this.txtLoginid.Text.Trim(), UserID);

            // Check if Referral selected.
            string mySalesRepDistID = this.ddlReferral.SelectedValue;

            bool isShippingCountry_StateMatched = IsShippingCountryStateMatched();
            bool isValidConsignmentAccount = true;
            bool isValidInvoiceDeliveryMethodFax = true;
            bool isValidInvoiceDeliveryMethodPrimaryEmail = true;

            if (chkBoxInvDeliveryEmail.Checked && string.IsNullOrEmpty(txtInvDeliveryPrimaryEmail.Text.Trim()))
                isValidInvoiceDeliveryMethodPrimaryEmail = false;

            if (chkBoxInvDeliveryFax.Checked && string.IsNullOrEmpty(txtInvDeliveryPrimaryFax.Text.Trim()))
                isValidInvoiceDeliveryMethodFax = false;

            if (ddlCustomerType.SelectedItem.Text.Trim() == "CON")
            {  // validation if account type is consignment type
                isValidConsignmentAccount = validateConsignmentAccount();
            }

            if (LoginIDcount == 0 && mySalesRepDistID != "0" && isShippingCountry_StateMatched && isValidConsignmentAccount && isValidInvoiceDeliveryMethodFax && isValidInvoiceDeliveryMethodPrimaryEmail)
            {
                //Default timezone to 5.
                int TimeZoneID = 5;
                double TimeZoneHr = 0;
                //Grab the shipping state to find the timezone.
                var ShipState = (from s in idc.States where s.StateID == int.Parse(ddlStateS.SelectedValue.ToString()) select s).FirstOrDefault();
                TimeZoneID = (ShipState != null) ? ShipState.TimeZoneID : 5;
                //TimeZoneHr = (ShipState.TimeZone.HasValue) ? ShipState.TimeZone.Value  : 0;

                //Default UOM to 'mrem'
                string UOM = "mrem";
                //Grab the shipping country to find the UOM
                var ShipCountry = (from c in idc.Countries where c.CountryID == int.Parse(ddlCountryS.SelectedValue.ToString()) select c).First();
                UOM = ShipCountry.DefaultUOM.ToString();

                //update Scheduled Billing based 
                //save/generate schedule billing info if billing term = Quarterly
                if (ddlBillingTerm.SelectedValue == "1")
                {
                    bool boolScheduleBilling = false;

                    //1/2014 WK - If new account (no orders) then create Schedule billing records.
                    if (isNewAccount(AccountID))
                    {
                        //Are there previous open scheduled billing records (OrderID = null) for this account?
                        if (!doesScheduledBillingExist(AccountID))
                        {
                            //No.  Add new scheduled billings.
                            DateTime myStartDate = DateTime.Parse(txt_ServiceStartDate.Text.Trim());
                            int myBillingDatePiror = int.Parse(txtBillPriorDay.Text.Trim());
                            //int myBillingSchedulePeriod = 91;

                            //boolScheduleBilling = SaveScheduledBilling(AccountID, myStartDate, myBillingDatePiror, myBillingSchedulePeriod);
                            boolScheduleBilling = SaveScheduledBilling(AccountID, myStartDate);

                            if (boolScheduleBilling == false)
                                this.displayError(this.errorMsg.InnerText + "Schedule Billing failed.");
                        }
                    }
                }
                else  //If yearly, then delete any new (OrderID = null) scheduled billing records.
                {
                    bool boolScheduleBilling = deleteScheduledBilling(AccountID);

                    if (boolScheduleBilling == false)
                        this.displayError(this.errorMsg.InnerText + "Deleting previous schedule billing records failed.");
                }

                // update account table
                string AccountRtnStr = UpdateAccount(AccountID, TimeZoneID, TimeZoneHr, UOM);

                // update location table
                string LocationRtnStr = UpdateLocation(LocationID, AccountID, TimeZoneID, TimeZoneHr, UOM);

                // update BillingGroup table
                int returnBillingGroupID = SaveBillingGroup(false, AccountID);

                // update users table
                string UserRtnStr = UpdateUser(UserID, AccountID, LocationID);

                // save new data to mas
                bool boodToMas = SaveToTOmasAR_Customer(AccountID, UserID);

                // display confirmation
                btnUpdate.Visible = false;

                ConstructRegisterHashTable();
                DisplayRegisterConfirmation(AccountID, "Thank you, account has been updated!");
            } // LoginID Check count
            else
            {
                if (LoginIDcount != 0)
                    this.CustomValidator1.IsValid = false;
                if (!isShippingCountry_StateMatched)
                    this.CustomValidator2.IsValid = false;
                //if (mySalesRepDistID == "0")
                //    this.CustomValidator2.IsValid = false;
                if (!isValidInvoiceDeliveryMethodFax)
                    RequiredFieldValidatorInvDeliveryPrimaryFax.IsValid = false;

                if (!isValidInvoiceDeliveryMethodPrimaryEmail)
                    RequiredFieldValidatorInvDeliveryPrimaryEmail.IsValid = false;
            }

        }
        catch
        {
            this.displayError("Not a valid account!");
            btnSave.Visible = false;
            btnUpdate.Visible = false;
        }
    }

    protected void btn_CF_order2_Click(object sender, EventArgs e)
    {
        Response.Redirect(string.Format("CreateOrder.aspx?ID={0}", hdnAccountID.Value), false);
        Context.ApplicationInstance.CompleteRequest();
    }

    protected void btn_CF_AccountInfo2_Click(object sender, EventArgs e)
    {
        Response.Redirect(string.Format("../InformationFinder/Details/Account.aspx?ID={0}", hdnAccountID.Value), false);
        Context.ApplicationInstance.CompleteRequest();
    }

    //protected void btnEditAccount_Click(object sender, EventArgs e)
    //{
    //    //this.btn_CF_order1.PostBackUrl = "CSorder.aspx?ID=" + NewAccountID.ToString();
    //    divMainForm.Visible = true;
    //    divConfirmationForm.Visible = false;
    //}

    protected void btnDeactivate_Click(object sender, EventArgs e)
    {
        Page.Response.Redirect("DeactivateAccount.aspx?ID=" + Page.Request.QueryString["ID"].ToString());
    }

    // Edit Rate Details information and update to databse.
    protected void btnEdit_Click(object sender, EventArgs e)
    {
    }

    /// <summary>
    /// Delete Order - used only if order has NOT been shipped, fullfilled, etc.!
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void imgBtnEdit_Click(object sender, ImageClickEventArgs e)
    {
        ImageButton btn = (ImageButton)sender;
        string btnCommandName = btn.CommandName.ToString();
        //double discount = 0;

        //if (!decimal.TryParse(btn.CommandArgument[1].ToString(), out discount)) return;

        int productGroupID = 0;
        if (!int.TryParse(btn.CommandArgument.ToString(), out productGroupID)) return;

        var discount = (from a in idc.AccountDiscounts
                        where a.AccountID == AccountID && a.ProductGroupID == productGroupID
                        select a.Discount).FirstOrDefault();

        hidMode.Value = "Edit";

        // Select the location.
        lblRateCode.Text = this.ddlRateCode.SelectedItem.Text;
        txtDiscount.Text = discount.ToString();

        int rateCode = 0;

        if (int.TryParse(ddlRateCode.SelectedValue, out rateCode))
        {
            ddlDiscountProductGroup.DataSource = (from pg in idc.ProductGroups
                                                  join rd in idc.RateDetails
                                                  on pg.ProductGroupID equals rd.ProductGroupID
                                                  where rd.AllowDiscount && rd.Active
                                                  && rd.RateID == rateCode
                                                  orderby pg.ProductGroupID
                                                  select new
                                                  {
                                                      ProductGroupID = pg.ProductGroupID,
                                                      ProductGroupName = pg.ProductGroupName
                                                  }).Distinct();

            ddlDiscountProductGroup.DataTextField = "ProductGroupName";
            ddlDiscountProductGroup.DataValueField = "ProductGroupID";
            ddlDiscountProductGroup.DataBind();
        }

        ddlDiscountProductGroup.SelectedValue = productGroupID.ToString();

        ddlDiscountProductGroup.Enabled = false;

        // Force the select dialog to open on initial load.
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID,
                "$('#divModalDiscount').dialog('open');", true);

    }

    #endregion

    /// <summary>
    /// Save Form data to data tables
    /// to Accounts, ScheduleBillings, Locations, Users & ToMASAR_Customer
    /// New account registeration to account administrator
    /// </summary>
    #region SaveFormData

    protected int SaveToAccount(int TimeZoneID, double TimeZoneHr, string UOM)
    {
        int ReturnNewAccountID = 0;
        int selectedDealerID = 0;
        if (ddlDealer.Visible == true)
            selectedDealerID = int.Parse(ddlDealer.SelectedValue);

        try
        {
            // Create new account 
            Account newAcct = null;
            newAcct = new Account();
            newAcct.AccountName = this.txtAccountName.Text.Trim();
            newAcct.CompanyName = this.txtCompanyName.Text.Trim();
            newAcct.CustomerTypeID = int.Parse(this.ddlCustomerType.SelectedValue);

            if (int.TryParse(ddlAccountType.SelectedValue, out int accountTypeId))
                newAcct.AccountTypeID = accountTypeId;

            if (int.TryParse(ddlCustomerGroup.SelectedValue, out int customerGroupId))
                newAcct.CustomerGroupID = customerGroupId;

            /// 8/2013 WK - New for Unix transition accounts.
            /// 
            //if (!ddlUnixCustomerType.SelectedItem.Text.Contains("Select"))
            //    newAcct.UnixCustomerTypeID = int.Parse(this.ddlUnixCustomerType.SelectedValue);

            if (!ddlIndustryType.SelectedItem.Text.Contains("Select"))
                newAcct.IndustryID = int.Parse(this.ddlIndustryType.SelectedValue);
            
            //newAcct.IndustryID = 4; // default to value
            newAcct.BrandSourceID = int.Parse(ddlBrand.SelectedValue);
            newAcct.BillingTermID = int.Parse(this.ddlBillingTerm.SelectedValue.ToString());
            newAcct.ContractStartDate = DateTime.Parse(txt_ServiceStartDate.Text);
            newAcct.ContractEndDate = DateTime.Parse(txt_ServiceEndDate.Text);

            newAcct.PaymentTerms = int.TryParse(ddlPaymentTerm.SelectedValue, out int tmpPayTerm) ? tmpPayTerm : 30;

            if (newAcct.BrandSourceID == 3)
            {
                newAcct.isDemoAccount = chkICCareDemoAccount.Checked;

                newAcct.isICCareCommEligible = chkICCareSalesRepCommission.Checked;
            }
            else
            {
                newAcct.isDemoAccount = false;
            }

            var setting = txtBillPriorDay.Text;
            if (string.IsNullOrEmpty(setting))
            {
                var mr = new MiscRequests();
                var defVal = mr.GetAppSetting("RenewalDaysPrior");
                if (!string.IsNullOrEmpty(defVal))
                    setting = defVal;
            }
                
            //1/2014 WK - Location billing.
            newAcct.UseLocationBilling = chkUseLocationBilling.Checked;

            newAcct.BillingDaysPrior = int.Parse(setting);
            newAcct.BillingMethod = "Purchase Order";
            newAcct.CreatedBy = UserName;
            newAcct.CreatedDate = DateTime.UtcNow.AddHours(TimeZoneHr);
            newAcct.RenewalDaysPrior = int.Parse(setting); //int.Parse(txtBillPriorDay.Text);
            newAcct.RenewalPONumber = MAS.CleanString(this.txtPOno.Text.Trim(), 15);
            newAcct.ReferralCode = this.ddlReferral.SelectedValue.ToString();
            newAcct.RenewalPOStartDate = String.IsNullOrEmpty(txtPOBeginDate.Text) ? (DateTime?)null : DateTime.Parse(txtPOBeginDate.Text);
            newAcct.RenewalPOEndDate = String.IsNullOrEmpty(txtPOEndDate.Text) ? (DateTime?)null : DateTime.Parse(txtPOEndDate.Text);  

            // sales rep id from MRNSalesRepID in Dosimetry DB
            int? salesRepId = null;
            if (ddlSalesRep.SelectedValue != "0" && !string.IsNullOrEmpty(ddlSalesRep.SelectedValue) && int.TryParse(ddlSalesRep.SelectedValue, out int tmpSalesRepId))
                salesRepId = tmpSalesRepId;
            newAcct.SalesRepID = salesRepId;

            newAcct.UnixID = selectedDealerID.ToString(); // 10/3/2011 as Dealers' DealerID
            newAcct.AccountUOM = UOM;

            newAcct.OkToEmail = true;
            newAcct.LockLocation = false;
            newAcct.MovableUser = 1;
            newAcct.Active = true;

            newAcct.DefaultRateID = (this.ddlRateCode.SelectedValue.ToString() == "0")
                    ? (int?)null : int.Parse(ddlRateCode.SelectedValue.ToString());

            newAcct.CurrencyCode = (this.ddlRateCode.SelectedValue.ToString() == "0")
                    ? "USD" : (from r in idc.Rates
                               where r.RateID == int.Parse(ddlRateCode.SelectedValue)
                               select r.CurrencyCode).FirstOrDefault();

            //newAcct.CurrencyCode = ddlCurrency.SelectedItem.ToString();

            newAcct.Discount = (txtDiscount.Text.Trim() == "") ? 0 : double.Parse(txtDiscount.Text.Trim());
            //newAcct.Discount = (txtDiscount.Text.Trim() == "") ? 0 : double.Parse (string.Format("{0:0.00}", txtDiscount.Text.Trim()));

            newAcct.PackageTypeID = (ddlPackageType.SelectedValue.ToString() == "0") ? (int?)null : int.Parse(ddlPackageType.SelectedValue.ToString());
            newAcct.ShippingOptionID = (ddlShippingOption.SelectedValue.ToString() == "0") ? (int?)null : int.Parse(ddlShippingOption.SelectedValue.ToString());
            newAcct.ShippingCarrier = (ddlShippingCarrier.SelectedValue.ToString() == "-- Select Shipping Carrier --") ? null : ddlShippingCarrier.SelectedValue.ToString();
            newAcct.SpecialInstructions = txtSpecialInstruction.Text;

            newAcct.IncludeLPI = this.chkBoxIncludeLPI.Checked;
            //newAcct.LPIUserRoleID = int.Parse(ddlLPIUserRole.SelectedValue.ToString());
            newAcct.LPIUserRoleID = 4; // This field will be removed of the table. For this time being, set to default = Administrator
            newAcct.ScheduleID = (this.ddlSchedule.SelectedValue.ToString() == "0") ? (int?)null : int.Parse(ddlSchedule.SelectedValue.ToString());
            newAcct.WebProductGroupID = int.Parse(this.ddlProductGroup.SelectedValue.ToString());

            //8/2013 WK - CollectorType added.  Defaulted to 1.
            newAcct.CollectorType = 1;

            // 9/2/2014 TDO - Force setting default shipping carrier = DHL International for foreign account.
            if (Convert.ToBoolean(GetAppSettings("MalvernIntegration")))
            {
                if (ddlCountryS.SelectedItem.Text.ToLower() != "united states")
                    newAcct.ShippingCarrier = "DHL International";
            }

            idc.Accounts.InsertOnSubmit(newAcct);
            idc.SubmitChanges();
            // get the @@identity account ID
            ReturnNewAccountID = newAcct.AccountID;

            //10/2013 WK - If custom rate code used then store Account ID to Rates table.
            //******************************************************************************
            //   Implement this after 10/15/13 Instadose build
            //------------------------------------------------------------------------------
            if (chkCustomRate.Checked)
            {
                int rateID = int.Parse(ddlRateCode.SelectedValue.ToString());

                // Query the database for the row to be updated.
                Rate acctRate = (from r in idc.Rates
                                 where r.RateID == rateID
                                 select r).FirstOrDefault();

                //Add new account to this custom rate.
                acctRate.CustomAccountID = ReturnNewAccountID;

                idc.SubmitChanges();
            }
            else //Null out any custom rate codes with this account ID since custom code is not set.
            {
                // Query the database for the row to be updated.
                Rate acctRate = (from r in idc.Rates
                                 where r.CustomAccountID == ReturnNewAccountID
                                 select r).FirstOrDefault();

                if (acctRate != null)
                {
                    //If there is custom rate then delete out account ID.
                    acctRate.CustomAccountID = null;

                    idc.SubmitChanges();
                }

            }
            
            // ----------------------- Insert AccountPGDetails --------------------------- //
            AccountPGDetail apgd = new AccountPGDetail
            {
                AccountID = ReturnNewAccountID,
                ProductGroupID = int.Parse(this.ddlProductGroup.SelectedValue),
                HasLabel = false,
                LabelID = int.Parse(this.ddlLabel.SelectedValue),
                OrderDevInitialize = this.chkBoxDeviceInitialize.Checked,
                OrderDevAssign = this.chkBoxDeviceAssign.Checked
            };
            idc.AccountPGDetails.InsertOnSubmit(apgd);
            idc.SubmitChanges();
            // ----------------------- Insert AccountPGDetails --------------------------- //
        }
        catch
        {
            ReturnNewAccountID = 0;
        }

        return ReturnNewAccountID;
    }

    ///// <summary>
    ///// Schedule Billing for Quarterly Account
    ///// </summary>
    ///// <param name="NewAccountID"></param>
    ///// <param name="ContractStartDate"></param>
    ///// <param name="BillingDatePiror">subtract days from beginnig of each period </param>
    ///// <param name="BillingSchedulePeriod"># of days </param>
    ///// <returns></returns>
    //protected bool SaveScheduledBilling(int NewAccountID, DateTime ContractStartDate, int BillingDatePiror, int BillingSchedulePeriod)
    //{
    //    bool myReturn = false;
    //    try
    //    {
    //        for (int i = 1; i < 4; i++)
    //        {
    //            ScheduledBilling newBill = null;
    //            newBill = new ScheduledBilling();
    //            newBill.AccountID = NewAccountID;
    //            newBill.SchedBillingDate = ContractStartDate.AddMonths(i * 3); //ContractStartDate.AddDays(BillingSchedulePeriod * i);
    //            idc.ScheduledBillings.InsertOnSubmit(newBill);
    //            idc.SubmitChanges();
    //        }
    //        myReturn = true;
    //    }
    //    catch { myReturn = false; }
    //    return myReturn;
    //}
    protected bool SaveScheduledBilling(int NewAccountID, DateTime ContractStartDate)
    {
        bool myReturn = false;
        try
        {
            for (int i = 1; i < 4; i++)
            {
                ScheduledBilling newBill = null;
                newBill = new ScheduledBilling();
                newBill.AccountID = NewAccountID;
                newBill.SchedBillingDate = ContractStartDate.AddMonths(i * 3); //ContractStartDate.AddDays(BillingSchedulePeriod * i);
                idc.ScheduledBillings.InsertOnSubmit(newBill);
                idc.SubmitChanges();
            }
            myReturn = true;
        }
        catch { myReturn = false; }
        return myReturn;
    }

    /// <summary>
    /// Check to see if account has any open scheduled billing records
    ///   (OrderID is null).
    /// </summary>
    /// <param name="accountID"></param>
    /// <returns></returns>
    protected bool doesScheduledBillingExist(int accountID)
    {
        //bool Deleted = false;
        try
        {
            int schedBillings = (from sb in idc.ScheduledBillings
                                 where sb.AccountID == AccountID
                                 && sb.OrderID == null
                                 select sb).Count();

            if (schedBillings == 0)
                return false;
            else
                return true;
        }
        catch { return false; }
    }

    /// <summary>
    /// Delete out scheduled billings if account is set to Yearly.
    /// </summary>
    /// <param name="NewAccountID"></param>
    /// <param name="ContractStartDate"></param>
    /// <param name="BillingDatePiror"></param>
    /// <param name="BillingSchedulePeriod"></param>
    /// <returns></returns>
    protected bool deleteScheduledBilling(int accountID)
    {
        bool isDeleted = false;
        try
        {
            var schedBillings = from sb in idc.ScheduledBillings
                                where sb.AccountID == AccountID
                                && sb.OrderID == null
                                select sb;

            foreach (var sb in schedBillings)
            {
                idc.ScheduledBillings.DeleteOnSubmit(sb);
            }

            isDeleted = true;
        }
        catch { isDeleted = false; }
        return isDeleted;
    }

    /// <summary>
    /// Are there any order(s) to this account (i.e. New account)?
    /// </summary>
    /// <param name="accountID"></param>
    /// <returns></returns>
    protected bool isNewAccount(int accountID)
    {
        int totalOrders = 0;
        try
        {
            totalOrders = (from o in idc.Orders
                           where o.AccountID == accountID
                           select o).Count();

            if (totalOrders == 0)
                return true;
            else
                return false;
        }
        catch { return false; }
    }

    protected int SaveToLocation(int NewAccountID, int TimeZoneID, double TimeZoneHr, string UOM)
    {
        int ReturnNewLocationID = 0;
        try
        {
            // Create the database LINQ objects.
            //this.idc = new InsDataContext ();
            Location ALoc = new Location();

            ALoc.LocationName = "Main";
            ALoc.LocationCode = "";
            ALoc.LocationUOM = UOM;
            ALoc.AccountID = NewAccountID;
            if (ddlGenderS.SelectedValue.ToString() != "")
                ALoc.ShippingNamePrefix = ddlGenderS.SelectedValue.ToString();
            ALoc.ShippingFirstName = txtFirstS.Text.Trim();
            ALoc.ShippingLastName = txtLastS.Text.Trim();
            ALoc.ShippingCompany = txtCompanyNameS.Text.Trim();
            ALoc.ShippingAddress1 = txtAddress1S.Text.Trim();
            ALoc.ShippingAddress2 = txtAddress2S.Text.Trim();
            ALoc.ShippingAddress3 = txtAddress3S.Text.Trim();
            ALoc.ShippingCity = txtCityS.Text.Trim();
            ALoc.ShippingStateID = int.Parse(ddlStateS.SelectedValue.ToString());
            ALoc.ShippingPostalCode = txtPostalS.Text.Trim();
            ALoc.ShippingCountryID = int.Parse(ddlCountryS.SelectedValue.ToString());
            ALoc.ShippingTelephone = txtTelephoneS.Text.Trim();
            ALoc.ShippingFax = txtFaxS.Text.Trim();
            ALoc.ShippingEmailAddress = txtEmailAddressS.Text.Trim();
            if (ddlGenderB.SelectedValue.ToString() != "")
                ALoc.BillingNamePrefix = ddlGenderB.SelectedValue.ToString();
            ALoc.BillingFirstName = txtFirstB.Text.Trim();
            ALoc.BillingLastName = txtLastB.Text.Trim();
            ALoc.BillingCompany = txtCompanyNameB.Text.Trim();
            ALoc.BillingAddress1 = txtAddress1B.Text.Trim();
            ALoc.BillingAddress2 = txtAddress2B.Text.Trim();
            ALoc.BillingAddress3 = txtAddress3B.Text.Trim();
            ALoc.BillingCity = txtCityB.Text.Trim();
            ALoc.BillingStateID = int.Parse(ddlStateB.SelectedValue.ToString());
            ALoc.BillingPostalCode = txtPostalB.Text.Trim();
            ALoc.BillingCountryID = int.Parse(ddlCountryB.SelectedValue.ToString());
            ALoc.BillingFax = txtFaxB.Text.Trim();
            ALoc.BillingTelephone = txtTelephoneB.Text.Trim();
            ALoc.BillingEmailAddress = txtEmailAddressB.Text.Trim();
            ALoc.TimeZoneID = TimeZoneID;
            ALoc.IsDefaultLocation = true;
            ALoc.Active = true;
            ALoc.CreatedDate = DateTime.UtcNow.AddHours(TimeZoneHr);


            idc.Locations.InsertOnSubmit(ALoc);
            idc.SubmitChanges();
            // get the @@identity 
            ReturnNewLocationID = ALoc.LocationID;

            // ----------------------- Insert LocationPGDetails --------------------------- //
            LocationPGDetail lpgd = new LocationPGDetail
            {
                AccountID = NewAccountID,
                LocationID = ReturnNewLocationID,
                ProductGroupID = int.Parse(this.ddlProductGroup.SelectedValue),
                LabelID = int.Parse(this.ddlLabel.SelectedValue)
            };
            idc.LocationPGDetails.InsertOnSubmit(lpgd);
            idc.SubmitChanges();
            // ----------------------- Insert LocationPGDetails --------------------------- //

        }
        catch
        {
            ReturnNewLocationID = 0;
            //delete Account by newAccountID
        }

        return ReturnNewLocationID;
    }


    protected int SaveToConsignmentInventoryLimits(int iAccountID, int iProductGroupID, int iMaxQuantity)
    {
        int iReturnResult = 1;

        try
        {
            // Create the database LINQ objects.
            //this.idc = new InsDataContext ();
            ConsignmentInventoryLimit ACon = new ConsignmentInventoryLimit();

            ACon.AccountID = iAccountID;
            ACon.ProductGroupID = iProductGroupID;
            ACon.MaxQuantity = iMaxQuantity;

            idc.ConsignmentInventoryLimits.InsertOnSubmit(ACon);
            idc.SubmitChanges();

        }
        catch
        {
            iReturnResult = 0;
            //delete Account by newAccountID
        }

        return iReturnResult;
    }


    protected int SaveToUser(int NewAccountID, int NewLocationID)
    {
        int ReturnNewUserId = 0;
        try
        {
            var MyGender = (rbtnMale.Checked) ? 'M' : 'F';

            // ----- Create new user -----
            User newUsr = new User();

            newUsr.AccountID = NewAccountID;
            newUsr.LocationID = NewLocationID;
            //GroupID=;

            newUsr.Prefix = (this.ddlGenderA.SelectedValue != "" ? this.ddlGenderA.SelectedValue : null);
            newUsr.FirstName = this.txtFirstA.Text.Trim();
            newUsr.LastName = this.txtLastA.Text.Trim();
            newUsr.UserName = this.txtLoginid.Text.Trim();
            //BirthDate=;
            newUsr.Gender = MyGender;
            newUsr.Address1 = txtAddress1B.Text.Trim();
            newUsr.Address2 = txtAddress2B.Text.Trim();
            newUsr.Address3 = txtAddress3B.Text.Trim();
            newUsr.City = txtCityB.Text.Trim();
            //newUsr.StateID = int.Parse(ddlStateB.SelectedValue.ToString());
            newUsr.State = idc.States.FirstOrDefault(s => s.StateID == int.Parse(ddlStateB.SelectedValue.ToString()));
            newUsr.PostalCode = txtPostalB.Text.Trim();
            //newUsr.CountryID = int.Parse(ddlCountryB.SelectedValue.ToString());
            newUsr.Country = idc.Countries.FirstOrDefault(c => c.CountryID == int.Parse(ddlCountryB.SelectedValue.ToString()));
            newUsr.Fax = txtFax.Text.Trim(); //txtFaxB.Text.Trim();
            newUsr.Telephone = txtTelephone.Text.Trim(); //txtTelephoneB.Text.Trim();
            newUsr.Email = this.txtEmail.Text.Trim();
            newUsr.CreatedDate = DateTime.Now;
            newUsr.Active = true;
            newUsr.OkToEmail = true;
            newUsr.OkToEmailHDN = false;
            newUsr.UserEmployeeType = "Employee ID";
            newUsr.UserRoleID = 4; // where 4 is administrator
            newUsr.AutoDeviceAssign = false;
            newUsr.MovableUser = 1;

            idc.Users.InsertOnSubmit(newUsr);
            idc.SubmitChanges();
            // get the @@identity 
            ReturnNewUserId = newUsr.UserID;


            // ----- Create new Authenticate User -----
            string myPassword = (this.txtPassword.Text.Trim().Length == 0 ? null : HashPassword(this.txtPassword.Text.Trim()));

            AuthUser newAuthUser = new AuthUser()
            {
                Active = true,
                MustChangePassword = false,
                CreatedDate = DateTime.Today,
                ModifiedDate = DateTime.Today,
                UserName = this.txtLoginid.Text.Trim(),
                Email = this.txtEmail.Text.Trim(),
                PasswordHash = myPassword,
                SecurityQuestionID1 = (this.ddlSecurityQuestion.SelectedValue.ToString() == "0") ? (int?)null : int.Parse(ddlSecurityQuestion.SelectedValue.ToString()),
                SecurityAnswer1 = (this.txtSecurityA.Text.Trim() == "") ? null : txtSecurityA.Text.Trim()
            };

            // ----- Add the user to the instadose application group. This grants the user access to instadose applications. ----
            newAuthUser.AuthUserApplicationGroups.Add(new AuthUserApplicationGroup()
            {
                ApplicationGroupID = (from a in idc.AuthApplicationGroups
                                      where a.ApplicationGroupName == "Instadose"
                                      select a.AuthApplicationGroupID).FirstOrDefault()
            });

            idc.AuthUsers.InsertOnSubmit(newAuthUser);
            idc.SubmitChanges();


            // ----- update Account (AccountAdminID) with new UserID -----
            Account Aacct = null;
            Aacct = (from a in idc.Accounts
                     where a.AccountID == NewAccountID
                     select a).First();

            Aacct.AccountAdminID = ReturnNewUserId;
            idc.SubmitChanges();
        }
        catch (Exception ex)
        {
            Basics.WriteLogEntry(ex.Message, "System", "Portal.CreateAccount.SaveToUser", Basics.MessageLogType.Critical);
            ReturnNewUserId = 0;
        }

        return ReturnNewUserId;
    }

    private string HashPassword(string password)
    {
        try
        {
            return Hash.GetHash(password, Hash.HashType.SHA256);
        }
        catch { return ""; }
    }

    protected bool SendEmailToNewUser(int NewAccountID, int NewUserID, int NewSourceID)
    {
        bool myReturn = false;
        try
        {
            //this.idc = new InsDataContext ();

            //grap User email, name and account name
            var Uinfo = (from ui in idc.Users
                         join ai in idc.Accounts on ui.AccountID equals ai.AccountID
                         where ui.UserID == NewUserID && ui.AccountID == NewAccountID
                         select new { ai, ui }).First();

            string myAccountName = Uinfo.ai.AccountName;
            string myAccountID = NewAccountID.ToString();
            string myFirstName = Uinfo.ui.FirstName;
            string myLastName = Uinfo.ui.LastName;
            string myEmail = Uinfo.ui.Email;
            string myUserName = Uinfo.ui.UserName;
            //string myPassword = Instadose.Security.TripleDES.Decrypt(Uinfo.ui.Password);
            string myPassword = this.txtPassword.Text.Trim();


            //Send To addressList 
            List<string> ToAddList = new List<string>();
            ToAddList.Add(myEmail);
            //ToAddList.Add("gchu@mirion.com");

            //Build the template fileds.
            Dictionary<string, string> EmailFields = new Dictionary<string, string>();
            EmailFields.Add("AccountName", myAccountName);
            EmailFields.Add("AccountID", myAccountID);
            EmailFields.Add("FirstName", myFirstName);
            EmailFields.Add("LastName", myLastName);
            EmailFields.Add("Email", myEmail);
            EmailFields.Add("UserName", myUserName);
            EmailFields.Add("Password", myPassword);

            switch (NewSourceID)
            {
                case 1:    //Quantum
                    EmailFields.Add("ContactEmail", "info@quantumbadges.com");
                    EmailFields.Add("ContactPhone", "800-359-9686");
                    EmailFields.Add("FooterCompany", "Quantum Products");
                    break;

                case 2:    //Mirion
                    EmailFields.Add("ContactEmail", "gdsinfo@mirion.com");
                    EmailFields.Add("ContactPhone", "800-251-3331");
                    EmailFields.Add("FooterCompany", "Mirion Technologies");
                    break;

                case 3:    //IC Care
                    EmailFields.Add("ContactEmail", "info@iccare.net");
                    EmailFields.Add("ContactPhone", "877-477-5486");
                    EmailFields.Add("FooterCompany", "IC Care");
                    break;

                    //default:
                    //    Console.WriteLine("Invalid selection. Please select 1, 2, or 3.");
                    //    break;
            }

            //if (NewSourceID == 3)
            //{
            //    EmailFields.Add("ContactEmail", "info@iccare.net");
            //    EmailFields.Add("ContactPhone", "877-477-5486");
            //    EmailFields.Add("FooterCompany", "IC Care");
            //}
            //else    //10/2013 WK - No more Quantum.  Either IC Care or Mirion brands.
            //{
            //    EmailFields.Add("ContactEmail", "gdsinfo@mirion.com");
            //    EmailFields.Add("ContactPhone", "800-251-3331");
            //    EmailFields.Add("FooterCompany", "Mirion Technologies");
            //}
            //else
            //{
            //    EmailFields.Add("ContactEmail", "info@quantumbadges.com");
            //    EmailFields.Add("ContactPhone", "800-359-9686");
            //    EmailFields.Add("FooterCompany", "Quantum Products");
            //}
            //**************************************************************************

            //*** send email to New User -- email class 
            myReturn = SendNewRegisteredMessage(ToAddList, EmailFields);

        }
        catch { myReturn = false; }

        return myReturn;
    }

    protected bool SendNewRegisteredMessage(List<string> ToaddressList, Dictionary<string, string> AllFieldsDict)
    {
        bool myReturn = false;
        MessageSystem SendMessage = new MessageSystem();
        SendMessage.Application = "CSRegister";
        SendMessage.CreatedBy = UserName;
        SendMessage.FromAddress = "noreply@instadose.com";

        //To Address List
        SendMessage.ToAddressList = ToaddressList;

        // Build the template fields and Send the Mesasge
        try
        {
            int MySendID = SendMessage.Send("Account Registration", "", AllFieldsDict);
            myReturn = true;
        }
        catch { myReturn = false; }
        return myReturn;
    }

    protected bool SaveToTOmasAR_Customer(int NewAccountID, int NewUserID)
    {
        bool myReturn = false;
        try
        {  //Grab Account, Location, User and SalesRep info
            var CustInfo = (from A in idc.Accounts
                            join L in idc.Locations on A.AccountID equals L.AccountID
                            join U in idc.Users on A.AccountAdminID equals U.UserID
                            join O in idc.SalesRepDistributors on A.ReferralCode equals O.SalesRepDistID
                            join C in idc.CommissionSetups on O.SalesRepDistID equals C.SalesRepDistID
                            where A.AccountID == NewAccountID && U.UserID == NewUserID
                            && L.IsDefaultLocation == true
                            select new { A, L, U, O, C }).FirstOrDefault();

            // save data to ToMasar_Customer
            this.mdc = new MASDataContext();
            ToMASAR_Customer MasCust = null;
            MasCust = new ToMASAR_Customer()
            {
                ARDivisionNo = "00",
                CustomerNo = MAS.CleanString(NewAccountID.ToString().PadLeft(7, '0'), 7),
                CustomerName = MAS.CleanString(CustInfo.L.BillingCompany, 30),
                AddressLine1 = MAS.CleanString(CustInfo.L.BillingAddress1, 30),
                AddressLine2 = MAS.CleanString(CustInfo.L.BillingAddress2, 30),
                AddressLine3 = MAS.CleanString(CustInfo.L.BillingAddress3, 30),
                City = MAS.CleanString(CustInfo.L.BillingCity, 20),
                // Mas accept state ="CA", "AL" etc
                State = (CustInfo != null && CustInfo.L != null && CustInfo.L.BillingStateID != null) ? MAS.CleanString((from s in idc.States where s.StateID == CustInfo.L.BillingStateID select s.StateAbbrev).First(), 2) : string.Empty,
                ZipCode = MAS.CleanString(CustInfo.L.BillingPostalCode, 10),
                CountryCode = (CustInfo != null && CustInfo.L != null && CustInfo.L.BillingCountryID != null) ? MAS.CleanString((from c in idc.Countries where c.CountryID == CustInfo.L.BillingCountryID select c.CountryCode).First(), 3) : string.Empty,
                TelephoneNo = MAS.CleanString(CustInfo.L.BillingTelephone, 17),
                FaxNo = MAS.CleanString(CustInfo.L.BillingFax, 17),
                EmailAddress = MAS.CleanString(CustInfo.U.Email, 50),
                //CustomerType = MAS.CleanString((from t in idc.CustomerTypes where t.CustomerTypeID == CustInfo.A.CustomerTypeID select t.CustomerTypeCode ).First(), 4),
                CustomerType = MAS.CleanString(CustInfo.A.CustomerType.CustomerTypeCode, 4),
                SalespersonNo = CustInfo.A.ReferralCode,
                SalespersonDivisionNo = CustInfo.O.ChannelID.ToString(),
                Currency_Code = CustInfo.A.CurrencyCode
            };
            mdc.ToMASAR_Customers.InsertOnSubmit(MasCust);
            mdc.SubmitChanges();
            myReturn = true;
        }
        catch { myReturn = false; }
        return myReturn;
    }

    /// <summary>
    /// saveToAccountDiscount - for new account, save account discount(s) stored in session
    ///     to AccountDiscounts table.
    /// </summary>
    /// <param name="newAccountID"></param>
    private void saveToAccountDiscount(int newAccountID)
    {
        AccountDiscount accDisc = new AccountDiscount();

        try
        {
            for (int i = 0; i < this.gvDiscounts.Rows.Count; i++)
            {
                GridViewRow gvRow = gvDiscounts.Rows[i];

                int productGroupID = int.Parse(((HiddenField)gvRow.FindControl("hidProductGroupID")).Value);
                decimal discount = Decimal.Parse(((HiddenField)gvRow.FindControl("hidDiscount")).Value);

                accDisc = new AccountDiscount()
                {
                    AccountID = newAccountID,
                    ProductGroupID = productGroupID,
                    Discount = discount,
                    CreatedBy = UserName,
                    CreatedDate = DateTime.Today,
                    Active = true,
                };

                idc.AccountDiscounts.InsertOnSubmit(accDisc);

                idc.SubmitChanges();
            }
        }
        catch
        {
        }
    }

    /// <summary>
    /// Update account method.
    /// </summary>
    /// <param name="AccountID"></param>
    /// <param name="TimeZoneID"></param>
    /// <param name="TimeZoneHr"></param>
    /// <param name="UOM"></param>
    /// <returns></returns>
    protected string UpdateAccount(int AccountID, int TimeZoneID, double TimeZoneHr, string UOM)
    {
        int selectedDealerID = 0;
        if (ddlDealer.Visible == true)
            selectedDealerID = int.Parse(ddlDealer.SelectedValue);

        string returnStr = "failure";
        try
        {
            bool isUpdateContract = !isOrderExist();

            // Update account information
            Account updAcct = null;
            updAcct = (from a in idc.Accounts
                       where a.AccountID == AccountID
                       select a).First();

            updAcct.AccountName = this.txtAccountName.Text.Trim();
            updAcct.CompanyName = this.txtCompanyName.Text.Trim();
            updAcct.CustomerTypeID = int.Parse(this.ddlCustomerType.SelectedValue);

            if (int.TryParse(ddlAccountType.SelectedValue, out int accountTypeId))
                updAcct.AccountTypeID = accountTypeId;

            if (int.TryParse(ddlCustomerGroup.SelectedValue, out int customerGroupId))
                updAcct.CustomerGroupID = customerGroupId;

            /// 8/2013 WK - New for Unix transition accounts.
            /// 

            //if (!ddlUnixCustomerType.SelectedItem.Text.Contains("Select"))
            //    updAcct.UnixCustomerTypeID = int.Parse(this.ddlUnixCustomerType.SelectedValue);
            //else
            //    updAcct.UnixCustomerTypeID = null;


            if (!ddlIndustryType.SelectedItem.Text.Contains("Select"))
                updAcct.IndustryID = int.Parse(this.ddlIndustryType.SelectedValue);
            else
                updAcct.IndustryID = null;

            updAcct.UseLocationBilling = chkUseLocationBilling.Checked;

            updAcct.BrandSourceID = int.Parse(ddlBrand.SelectedValue);
            updAcct.BillingTermID = int.Parse(this.ddlBillingTerm.SelectedValue.ToString());

            if (updAcct.BrandSourceID == 3)
                updAcct.isICCareCommEligible = chkICCareSalesRepCommission.Checked;
            else
                updAcct.isICCareCommEligible = null;

            // Service Start and End Date can be updated, if order does not exist
            if (isUpdateContract)
            {
                updAcct.ContractStartDate = DateTime.Parse(txt_ServiceStartDate.Text);
                updAcct.ContractEndDate = DateTime.Parse(txt_ServiceEndDate.Text);
            }

            updAcct.PaymentTerms = int.TryParse(ddlPaymentTerm.SelectedValue, out int tmpPayTerm) ? tmpPayTerm : 30;

            updAcct.BillingDaysPrior = int.Parse(txtBillPriorDay.Text.Trim());
            updAcct.BillingMethod = "Purchase Order";
            //updAcct.CreatedBy = UserName;
            //updAcct.CreatedDate = DateTime.UtcNow.AddHours(TimeZoneHr);

            updAcct.RenewalDaysPrior = int.Parse(txtBillPriorDay.Text);
            updAcct.RenewalPONumber = MAS.CleanString(this.txtPOno.Text.Trim(), 15);
            updAcct.RenewalPOStartDate = String.IsNullOrEmpty(txtPOBeginDate.Text) ? (DateTime?)null : DateTime.Parse(txtPOBeginDate.Text); 
            updAcct.RenewalPOEndDate = String.IsNullOrEmpty(txtPOEndDate.Text) ? (DateTime?)null : DateTime.Parse(txtPOEndDate.Text); 

            if (ddlCustomerType.SelectedItem.Text.Trim() == "CON")
            {

                int iquantity = 0;

                int.TryParse(txtMaxIDPlusInventoryLimit.Text.Trim(), out iquantity);
                UpdateConsignmentInventoryLimits(AccountID, 10, iquantity);    // id plus

                int.TryParse(txtMaxIDLinkInventoryLimit.Text.Trim(), out iquantity);
                UpdateConsignmentInventoryLimits(AccountID, 5, iquantity);    // id link

                int.TryParse(txtMaxIDLinkUSBInventoryLimit.Text.Trim(), out iquantity);
                UpdateConsignmentInventoryLimits(AccountID, 6, iquantity);    // id link USB


                int.TryParse(txtMaxID1InventoryLimit.Text.Trim(), out iquantity);
                UpdateConsignmentInventoryLimits(AccountID, 9, iquantity);    // id 1

                int.TryParse(txtMaxID2InventoryLimit.Text.Trim(), out iquantity);
                UpdateConsignmentInventoryLimits(AccountID, 11, iquantity);    // id 2
            }

            updAcct.ReferralCode = this.ddlReferral.SelectedValue.ToString();

            // sales rep id from MRNSalesRepID in Dosimetry DB
            int? salesRepId = null;
            if (ddlSalesRep.SelectedValue != "0" && !string.IsNullOrEmpty(ddlSalesRep.SelectedValue) && int.TryParse(ddlSalesRep.SelectedValue, out int tmpSalesRepId))
                salesRepId = tmpSalesRepId;
            updAcct.SalesRepID = salesRepId;

            updAcct.UnixID = selectedDealerID.ToString(); // 10/3/2011 as Dealers' DealerID
            updAcct.AccountUOM = UOM;

            // 4/20/2012. Tdo. Do not reset these values.
            //updAcct.IndustryID = 4; 
            //updAcct.OkToEmail = true;
            //updAcct.LockLocation = false; 
            //updAcct.MovableUser = 1; 
            //updAcct.Active = true;                       

            updAcct.DefaultRateID = (this.ddlRateCode.SelectedValue.ToString() == "0")
                    ? (int?)null : int.Parse(ddlRateCode.SelectedValue.ToString());

            //Is account being updated to custom rate code?
            //******************************************************************************
            //   Implement this after 10/15/13 Instadose build
            //------------------------------------------------------------------------------
            if (chkCustomRate.Checked)
            {
                int rateID = int.Parse(ddlRateCode.SelectedValue.ToString());

                // Query the database for the row to be updated.
                Rate acctRate = (from r in idc.Rates
                                 where r.RateID == rateID
                                 select r).FirstOrDefault();

                //Add new account to this custom rate.
                acctRate.CustomAccountID = AccountID;

                idc.SubmitChanges();
            }
            else //Null out any custom rate codes with this account ID since custom code is not set.
            {
                // Query the database for the row to be updated.
                Rate acctRate = (from r in idc.Rates
                                 where r.CustomAccountID == AccountID
                                 select r).FirstOrDefault();

                //Add new account to this custom rate.
                if (acctRate != null)
                {
                    acctRate.CustomAccountID = null;
                    idc.SubmitChanges();
                }
            }
            //****************************************************************************

            updAcct.CurrencyCode = (this.ddlRateCode.SelectedValue.ToString() == "0")
                    ? "USD" : (from r in idc.Rates
                               where r.RateID == int.Parse(ddlRateCode.SelectedValue)
                               select r.CurrencyCode).FirstOrDefault();

            //updAcct.CurrencyCode = ddlCurrency.SelectedItem.ToString();

            updAcct.Discount = (txtDiscount.Text.Trim() == "") ? 0 : double.Parse(txtDiscount.Text.Trim());

            updAcct.PackageTypeID = (ddlPackageType.SelectedValue.ToString() == "0") ? (int?)null : int.Parse(ddlPackageType.SelectedValue.ToString());
            updAcct.ShippingOptionID = (ddlShippingOption.SelectedValue.ToString() == "0") ? (int?)null : int.Parse(ddlShippingOption.SelectedValue.ToString());
            updAcct.ShippingCarrier = (ddlShippingCarrier.SelectedValue.ToString() == "-- Select Shipping Carrier --") ? null : ddlShippingCarrier.SelectedValue.ToString();

            updAcct.IncludeLPI = this.chkBoxIncludeLPI.Checked;
            //updAcct.LPIUserRoleID = int.Parse(ddlLPIUserRole.SelectedValue.ToString());
            updAcct.ScheduleID = (this.ddlSchedule.SelectedValue.ToString() == "0") ? (int?)null : int.Parse(ddlSchedule.SelectedValue.ToString());
            updAcct.WebProductGroupID = int.Parse(this.ddlProductGroup.SelectedValue.ToString());

            updAcct.SpecialInstructions = txtSpecialInstruction.Text;

            idc.SubmitChanges();

            // -------------- save AccountPGDetail ------------------------ //

            AccountPGDetail apgd = (from acctPGD in idc.AccountPGDetails
                                    where acctPGD.AccountID == AccountID
                                    select acctPGD).FirstOrDefault();

            if (apgd != null)
            {
                // Delete the old AccountPGDetail 
                idc.AccountPGDetails.DeleteOnSubmit(apgd);
                idc.SubmitChanges();
            }
            // Insert new AccountPGDetail                    
            AccountPGDetail myAPGD = new AccountPGDetail
            {
                AccountID = AccountID,
                ProductGroupID = int.Parse(this.ddlProductGroup.SelectedValue),
                HasLabel = false,
                LabelID = int.Parse(this.ddlLabel.SelectedValue),
                OrderDevInitialize = this.chkBoxDeviceInitialize.Checked,
                OrderDevAssign = this.chkBoxDeviceAssign.Checked
            };
            idc.AccountPGDetails.InsertOnSubmit(myAPGD);
            idc.SubmitChanges();
            // -------------- save AccountPGDetail ------------------------ //

            // ------ Reset Billing Group for all locations if account is not a location billing -----//
            if(!chkUseLocationBilling.Checked)
            {                
                IQueryable<Location> LocList = (from a in idc.Locations where a.AccountID == AccountID
                                                select a).AsQueryable();
                foreach (Location loc in LocList)
                {
                    loc.BillingGroupID = null;
                    idc.SubmitChanges();
                }                
            }

            // update MRNAccountContract
            if (isUpdateContract)
            {
                DateTime contractStartDt = DateTime.Parse(txt_ServiceStartDate.Text);
                DateTime contractEndDt = contractStartDt.AddMonths(12).AddDays(-1);

                Mirion.DSD.GDS.API.AccountContractRequests accountContractReq = new Mirion.DSD.GDS.API.AccountContractRequests();

                // Price List informations from account contract
                var currentContract = accountContractReq.GetCurrentAccountContractByAccountEx(AccountID.ToString(), 2);

                // if current account contract does not exist, try to find most rescent account contract
                if (currentContract == null)
                    currentContract = accountContractReq.GetRescentAccountContractByAccountEx(AccountID.ToString(), 2);

                if (currentContract != null)
                {
                    accountContractReq.UpdateAccountContractDates(currentContract.MRNAccountContractID, contractStartDt, contractEndDt);
                }
                else
                {
                    int? brandSourceId = null;
                    if (int.TryParse(ddlBrand.SelectedValue, out int tmpBrandSourceId))
                        brandSourceId = tmpBrandSourceId;

                    accountContractReq.InsertInstaAccountContract(AccountID, contractStartDt, contractEndDt, brandSourceId);
                }
            }

            returnStr = "Account has been updated";
        }

        catch (Exception ex)
        {
            this.displayError(ex.ToString());
        }
        return returnStr;
    }

    protected string UpdateLocation(int LocationID, int AccountID, int TimeZoneID, double TimeZoneHr, string UOM)
    {
        string returnStr = "Location update failed!";
        try
        {
            // Create the database LINQ objects.
            //this.idc = new InsDataContext ();
            Location ALoc = null;
            ALoc = (from a in idc.Locations
                    where a.LocationID == LocationID
                    select a).First();

            ALoc.LocationName = "Main";
            ALoc.LocationCode = "";
            ALoc.LocationUOM = UOM;
            ALoc.AccountID = AccountID;


            if (ddlGenderS.SelectedValue.ToString() != "")
                ALoc.ShippingNamePrefix = ddlGenderS.SelectedValue.ToString();
            ALoc.ShippingFirstName = txtFirstS.Text.Trim();
            ALoc.ShippingLastName = txtLastS.Text.Trim();
            ALoc.ShippingCompany = txtCompanyNameS.Text.Trim();
            ALoc.ShippingAddress1 = txtAddress1S.Text.Trim();
            ALoc.ShippingAddress2 = txtAddress2S.Text.Trim();
            ALoc.ShippingAddress3 = txtAddress3S.Text.Trim();
            ALoc.ShippingCity = txtCityS.Text.Trim();
            ALoc.ShippingStateID = int.Parse(ddlStateS.SelectedValue.ToString());
            ALoc.ShippingPostalCode = txtPostalS.Text.Trim();
            ALoc.ShippingCountryID = int.Parse(ddlCountryS.SelectedValue.ToString());
            ALoc.ShippingTelephone = txtTelephoneS.Text.Trim();
            ALoc.ShippingFax = txtFaxS.Text.Trim();
            ALoc.ShippingEmailAddress = this.txtEmailAddressS.Text.Trim();

            if (ddlGenderB.SelectedValue.ToString() != "")
                ALoc.BillingNamePrefix = ddlGenderB.SelectedValue.ToString();
            ALoc.BillingFirstName = txtFirstB.Text.Trim();
            ALoc.BillingLastName = txtLastB.Text.Trim();
            ALoc.BillingCompany = txtCompanyNameB.Text.Trim();
            ALoc.BillingAddress1 = txtAddress1B.Text.Trim();
            ALoc.BillingAddress2 = txtAddress2B.Text.Trim();
            ALoc.BillingAddress3 = txtAddress3B.Text.Trim();
            ALoc.BillingCity = txtCityB.Text.Trim();
            ALoc.BillingStateID = int.Parse(ddlStateB.SelectedValue.ToString());
            ALoc.BillingPostalCode = txtPostalB.Text.Trim();
            ALoc.BillingCountryID = int.Parse(ddlCountryB.SelectedValue.ToString());
            ALoc.BillingFax = txtFaxB.Text.Trim();
            ALoc.BillingTelephone = txtTelephoneB.Text.Trim();
            ALoc.BillingEmailAddress = this.txtEmailAddressB.Text.Trim();

            ALoc.TimeZoneID = TimeZoneID;
            ALoc.IsDefaultLocation = true;
            ALoc.Active = true;
            //ALoc.CreatedDate = DateTime.UtcNow.AddHours(TimeZoneHr);
            idc.SubmitChanges();

            // -------------- save LocationPGDetail ------------------------ //

            LocationPGDetail lpgd =
              (from locPGD in idc.LocationPGDetails
               where locPGD.AccountID == AccountID && locPGD.LocationID == LocationID
               select locPGD).FirstOrDefault();

            if (lpgd != null)
            {
                // Delete the old AccountPGDetail 
                idc.LocationPGDetails.DeleteOnSubmit(lpgd);
                idc.SubmitChanges();
            }
            // Insert new LocationPGDetail                    
            LocationPGDetail myLPGD = new LocationPGDetail
            {
                AccountID = AccountID,
                ProductGroupID = int.Parse(this.ddlProductGroup.SelectedValue),
                LocationID = LocationID,
                LabelID = int.Parse(this.ddlLabel.SelectedValue)
            };
            idc.LocationPGDetails.InsertOnSubmit(myLPGD);
            idc.SubmitChanges();
            // -------------- save LocationPGDetail ------------------------ //

            returnStr = "Loacation has been updated.";
        }

        catch
        {
            //delete Account by newAccountID
        }
        return returnStr;
    }

    protected string UpdateAddress(int AddressID, int AccountID)
    {
        string returnStr = "Address update failed!";
        try
        {
            // Create the database LINQ objects.
            //this.idc = new InsDataContext ();
            Address ABilling = (from a in idc.Addresses where a.AddressID == AddressID select a).FirstOrDefault();

            if (ABilling != null)
            {
                ABilling.AccountID = AccountID;
                //ABilling.AddressTypeID = (from at in idc.AXAddressTypes where at.AddressTypeName == "Billing" select at.AddressTypeID).FirstOrDefault();
                ABilling.Address1 = txtAddress1B.Text.Trim();
                ABilling.Address2 = txtAddress2B.Text.Trim();
                ABilling.Address3 = txtAddress3B.Text.Trim();
                ABilling.City = txtCityB.Text.Trim();
                ABilling.StateID = int.Parse(ddlStateB.SelectedValue.ToString());
                ABilling.PostalCode = txtPostalB.Text.Trim();
                ABilling.CountryID = int.Parse(ddlCountryB.SelectedValue.ToString());

                idc.SubmitChanges();

                returnStr = "Address has been updated.";
            }
        }
        catch (Exception EX)
        {
            //delete Account by newAccountID
        }
        return returnStr;
    }

    public bool SaveAddress(ref int pBillingAddressID, int pAccountID)
    {
        bool returnCommit = false;
        int billingAddressID = pBillingAddressID;
        try
        {
            if (billingAddressID > 0)
            {
                Address ABilling = (from a in idc.Addresses where a.AddressID == billingAddressID select a).FirstOrDefault();

                if (ABilling != null)
                {
                    ABilling.AccountID = AccountID;
                    //ABilling.AddressTypeID = (from at in idc.AXAddressTypes where at.AddressTypeName == "Billing" select at.AddressTypeID).FirstOrDefault();
                    ABilling.Address1 = txtAddress1B.Text.Trim();
                    ABilling.Address2 = txtAddress2B.Text.Trim();
                    ABilling.Address3 = txtAddress3B.Text.Trim();
                    ABilling.City = txtCityB.Text.Trim();
                    ABilling.StateID = int.Parse(ddlStateB.SelectedValue.ToString());
                    ABilling.PostalCode = txtPostalB.Text.Trim();
                    ABilling.CountryID = int.Parse(ddlCountryB.SelectedValue.ToString());

                    idc.SubmitChanges();
                }
            }
            else
            {
                Address ABilling = new Address();

                ABilling.AccountID = pAccountID;
                ABilling.AddressTypeID = (from at in idc.AXAddressTypes where at.AddressTypeName == "Billing" select at.AddressTypeID).FirstOrDefault();
                ABilling.Address1 = txtAddress1B.Text.Trim();
                ABilling.Address2 = txtAddress2B.Text.Trim();
                ABilling.Address3 = txtAddress3B.Text.Trim();
                ABilling.City = txtCityB.Text.Trim();
                ABilling.StateID = int.Parse(ddlStateB.SelectedValue.ToString());
                ABilling.PostalCode = txtPostalB.Text.Trim();
                ABilling.CountryID = int.Parse(ddlCountryB.SelectedValue.ToString());

                idc.Addresses.InsertOnSubmit(ABilling);
                idc.SubmitChanges();
                // get the @@identity 
                pBillingAddressID = ABilling.AddressID;
            }
            returnCommit = true;
        }
        catch (Exception ex)
        {
            //delete Account by newAccountID
        }
        return returnCommit;
    }

    public int SaveBillingGroup(bool isNew, int AccountID)
    {
        int returnBillingGroupID = 0;

        try
        {
            Account act = (from a in idc.Accounts where a.AccountID == AccountID select a).FirstOrDefault();

            string billingGroupCompany = act.BrandSourceID == 3 ? HDealers.GetDealerName(act.UnixID, txtCompanyNameB.Text.Trim()) : txtCompanyNameB.Text.Trim();

            if (isNew)
            {
                int billingAddressID = 0;
                if (SaveAddress(ref billingAddressID, AccountID))
                {
                    BillingGroup BGroup = new BillingGroup();

                    BGroup.AccountID = AccountID;
                    BGroup.BillingAddressID = billingAddressID;
                    BGroup.CompanyName = billingGroupCompany; //txtCompanyNameB.Text.Trim(); //txtCompanyName.Text.Trim();
                    BGroup.ContactName = txtFirstB.Text.Trim() + " " + txtLastB.Text.Trim();
                    BGroup.useMail = chkBoxInvDeliveryPrintMail.Checked;                       
                    BGroup.useEmail1 = chkBoxInvDeliveryEmail.Checked;
                    BGroup.Email1 = string.IsNullOrEmpty(txtInvDeliveryPrimaryEmail.Text) ? null : txtInvDeliveryPrimaryEmail.Text.Trim();
                    BGroup.useEmail2 = chkBoxInvDeliveryEmail.Checked && (!string.IsNullOrEmpty(txtInvDeliverySecondaryEmail.Text));
                    BGroup.Email2 = string.IsNullOrEmpty(txtInvDeliverySecondaryEmail.Text) ? null : txtInvDeliverySecondaryEmail.Text.Trim();
                    BGroup.useFax = chkBoxInvDeliveryFax.Checked;
                    BGroup.Fax = string.IsNullOrEmpty(txtInvDeliveryPrimaryFax.Text) ? null : txtInvDeliveryPrimaryFax.Text.Trim();
                    BGroup.useEDI = chkBoxInvDeliveryEDI.Checked;
                    //BGroup.EDIClientID = 1; manually update in the back end
                    BGroup.useSpecialDelivery = chkBoxInvDeliveryUpload.Checked;
                    BGroup.SpecialDeliveryText = string.IsNullOrEmpty(txtInvDeliveryUploadInstruction.Text) ? null : txtInvDeliveryUploadInstruction.Text.Trim();                    
                    BGroup.DeliverInvoice = !chkBoxInvDeliveryDoNotSend.Checked;

                    BGroup.PONumber = MAS.CleanString(this.txtPOno.Text.Trim(), 15);
                    BGroup.ACMSyncBit = 1;

                    bool uploadResult = UploadInvDeliveryMethodInstruction(AccountID);

                    if (uploadResult)
                    {
                        idc.BillingGroups.InsertOnSubmit(BGroup);
                        idc.SubmitChanges();
                        // get the @@identity 
                        returnBillingGroupID = BGroup.BillingGroupID;

                        // ----------------------- Insert BillingGroupID to account --------------------------- //
                        act.BillingGroupID = returnBillingGroupID;
                        idc.SubmitChanges();
                        // ----------------------- Insert BillingGroupID to account --------------------------- //
                    }
                }
            }
            else
            {
                if (act.BillingGroup != null)
                {
                    // update address table
                    int billingAddressID = act.BillingGroup.BillingAddressID;
                    SaveAddress(ref billingAddressID, AccountID);
                    // update BillingGroup
                    BillingGroup BGroup = act.BillingGroup;
                    BGroup.CompanyName = billingGroupCompany; //txtCompanyNameB.Text.Trim(); //txtCompanyName.Text.Trim();
                    BGroup.ContactName = txtFirstB.Text.Trim() + " " + txtLastB.Text.Trim();
                    BGroup.useMail = chkBoxInvDeliveryPrintMail.Checked;
                    BGroup.useEmail1 = chkBoxInvDeliveryEmail.Checked;
                    BGroup.Email1 = string.IsNullOrEmpty(txtInvDeliveryPrimaryEmail.Text) ? null : txtInvDeliveryPrimaryEmail.Text.Trim();
                    BGroup.useEmail2 = chkBoxInvDeliveryEmail.Checked && (!string.IsNullOrEmpty(txtInvDeliverySecondaryEmail.Text));
                    BGroup.Email2 = string.IsNullOrEmpty(txtInvDeliverySecondaryEmail.Text) ? null : txtInvDeliverySecondaryEmail.Text.Trim();
                    BGroup.useFax = chkBoxInvDeliveryFax.Checked;
                    BGroup.Fax = string.IsNullOrEmpty(txtInvDeliveryPrimaryFax.Text) ? null : txtInvDeliveryPrimaryFax.Text.Trim(); ;
                    BGroup.useEDI = chkBoxInvDeliveryEDI.Checked;
                    //BGroup.EDIClientID = 1; manually update in the back end
                    BGroup.useSpecialDelivery = chkBoxInvDeliveryUpload.Checked;
                    BGroup.SpecialDeliveryText = string.IsNullOrEmpty(txtInvDeliveryUploadInstruction.Text) ? null : txtInvDeliveryUploadInstruction.Text.Trim();                    
                    BGroup.DeliverInvoice = !chkBoxInvDeliveryDoNotSend.Checked;
                    BGroup.PONumber = MAS.CleanString(this.txtPOno.Text.Trim(), 15);

                    BGroup.ACMSyncBit = 1;

                    bool uploadResult = UploadInvDeliveryMethodInstruction(AccountID);

                    if (uploadResult)
                        idc.SubmitChanges();

                    returnBillingGroupID = act.BillingGroup.BillingGroupID;
                }
                else //if missing BillingGroup on the account then insert it
                {
                    returnBillingGroupID = SaveBillingGroup(true, AccountID);
                }
            }
        }
        catch
        {
            //delete Account by newAccountID
        }
        return returnBillingGroupID;
    }

    private bool UploadInvDeliveryMethodInstruction(int AccountID)
    {
        if (chkBoxInvDeliveryUpload.Checked && fileUploadInvDeliveryUpload.HasFile)
        {
            try
            {
                // Get Upload infor and convert to byte format        
                HttpPostedFile hpf = fileUploadInvDeliveryUpload.PostedFile;
                string attachmentFile = Path.GetFileName(hpf.FileName);

                // CREATE the Byte Array.
                Byte[] byteImage = new Byte[hpf.ContentLength];

                // Read uploaded file to byte array.
                hpf.InputStream.Read(byteImage, 0, hpf.ContentLength);
                var arrAttachment = attachmentFile.Split('.');
                string fileExtension = arrAttachment[arrAttachment.Length - 1];
                string mimeType = null;

                switch (fileExtension)
                {
                    case "DOC":
                        mimeType = "application/vnd.ms-word";
                        break;
                    case "DOCX":
                        mimeType = "application/vnd.ms-word";
                        break;
                    case "XLS":
                        mimeType = "application/vnd.ms-excel";
                        break;
                    case "XLSX":
                        mimeType = "application/vnd.ms-excel";
                        break;
                    case "PDF":
                        mimeType = "application/pdf";
                        break;
                    default:
                        mimeType = "image/jpeg";
                        break;
                }

                Document doc = new Document()
                {
                    DocumentGUID = Guid.NewGuid(),
                    FileName = Path.GetFileName(hpf.FileName),
                    DocumentContent = byteImage,
                    MIMEType = mimeType,
                    DocumentCategory = "Invoice Delivery Instruction",
                    Description = string.IsNullOrEmpty(txtInvDeliveryUploadInstruction.Text) ? null : txtInvDeliveryUploadInstruction.Text.Trim(),
                    AccountID = AccountID,
                    CreatedBy = this.UserName,
                    CreatedDate = DateTime.Now,
                    Active = true
                };

                // SAVE the Document Record.
                idc.Documents.InsertOnSubmit(doc);
                idc.SubmitChanges();
            }
            catch
            {
                return false;
            }
        }
        return true;
    }

    protected int UpdateConsignmentInventoryLimits(int iAccountID, int iProductGroupID, int iMaxQuantity)
    {
        int iReturnResult = 1;
        int icount = 0;

        try
        {
            // Create the database LINQ objects.
            //this.idc = new InsDataContext ();

            icount = (from a in idc.ConsignmentInventoryLimits
                      where a.AccountID == iAccountID && a.ProductGroupID == iProductGroupID
                      select a).Count();
            if (icount == 0)
            {

                SaveToConsignmentInventoryLimits(iAccountID, iProductGroupID, iMaxQuantity);
            }
            else
            {
                ConsignmentInventoryLimit ACon = null;
                ACon = (from a in idc.ConsignmentInventoryLimits
                        where a.AccountID == iAccountID && a.ProductGroupID == iProductGroupID
                        select a).First();
                ACon.MaxQuantity = iMaxQuantity;
                idc.SubmitChanges();
            }
        }
        catch
        {
            iReturnResult = 0;
        }
        return iReturnResult;
    }


    protected string UpdateUser(int UserID, int AccountID, int LocationID)
    {
        string returnStr = "Update User Info failed!";
        try
        {
            this.idc = new InsDataContext();

            string MyGender = (rbtnMale.Checked) ? "M" : "F";

            // --------------- Update User -------------------
            User U = null;
            U = (from a in idc.Users
                 where a.UserID == UserID
                 select a).First();

            string oldUserName = U.UserName;

            U.Prefix = (this.ddlGenderA.SelectedValue != "" ? this.ddlGenderA.SelectedValue : null);
            U.FirstName = this.txtFirstA.Text.Trim();
            U.LastName = this.txtLastA.Text.Trim();
            U.UserName = this.txtLoginid.Text.Trim();
            U.Gender = char.Parse(MyGender);
            U.Address1 = txtAddress1B.Text.Trim();
            U.Address2 = txtAddress2B.Text.Trim();
            U.Address3 = txtAddress3B.Text.Trim();
            U.City = txtCityB.Text.Trim();
            U.StateID = int.Parse(ddlStateB.SelectedValue.ToString());
            U.PostalCode = txtPostalB.Text.Trim();
            U.CountryID = int.Parse(ddlCountryB.SelectedValue.ToString());
            U.Fax = txtFaxB.Text.Trim();
            U.Telephone = txtTelephoneB.Text.Trim();
            U.Email = this.txtEmail.Text.Trim();
            U.Active = true;
            U.UserRoleID = 4; // where 4 is administrator
            //AutoDeviceAssign=false;
            //MovableUser=1;

            idc.SubmitChanges();


            // ----------------- Update AuthUser ---------------------

            AuthUser myAuthUser =
              (from a in idc.AuthUsers
               where a.UserName == oldUserName
               select a).First();

            myAuthUser.Active = true;
            myAuthUser.MustChangePassword = false;
            myAuthUser.ModifiedDate = DateTime.Now;
            myAuthUser.UserName = this.txtLoginid.Text.Trim();
            myAuthUser.Email = this.txtEmail.Text.Trim();

            if (this.txtPassword.Text.Trim() != "")
            {
                myAuthUser.PasswordHash = HashPassword(this.txtPassword.Text.Trim());
            }

            myAuthUser.SecurityQuestionID1 = (this.ddlSecurityQuestion.SelectedValue.ToString() == "0") ? (int?)null : int.Parse(ddlSecurityQuestion.SelectedValue.ToString());
            myAuthUser.SecurityAnswer1 = (this.txtSecurityA.Text.Trim() == "") ? null : txtSecurityA.Text.Trim();
            myAuthUser.ResetToken = null;
            myAuthUser.ResetTokenDate = null;

            idc.SubmitChanges();

            returnStr = "User has been updated.";

        }
        catch
        {
            //delete location by NewLocationID
            //delete accout by newAccountID
        }
        return returnStr;
    }

    #endregion

    /// <summary>
    /// Load Form's Default value, DropDownList, TextBox 
    /// </summary>
    #region Load Account Form Controls

    protected void LoadDDLReferral()
    {
        this.ddlReferral.DataSource = (from a in idc.SalesRepDistributors
                                       where a.Active == true
                                       orderby a.SalesRepDistID
                                       select new
                                       {
                                           a.SalesRepDistID,
                                           SalesCompanyName = a.SalesRepDistID.ToString() + " - "
                                           + a.CompanyName.Trim()
                                       }).Distinct();
        ddlReferral.DataBind();

        ListItem item0 = new ListItem("  -- Select Referral --", "0");
        this.ddlReferral.Items.Insert(0, item0);
    }

    private void LoadDDLSalesRep()
    {
        Mirion.DSD.GDS.API.SalesRepRequests salesRepReq = new Mirion.DSD.GDS.API.SalesRepRequests();

        var salesReps = salesRepReq.GetNonICCareSaleReps(true).Select(sr => new { sr.SalesRepID, TextField = sr.FirstName + " " + sr.LastName }).ToList();
        salesReps.Insert(0, new { SalesRepID = 0, TextField = "-- Select Sales Rep --" });
        ddlSalesRep.DataSource = salesReps;
        ddlSalesRep.DataTextField = "TextField";
        ddlSalesRep.DataValueField = "SalesRepID";
        ddlSalesRep.DataBind();
    }

    protected void LoadTXTPurchaseOrder()
    {
        this.txtPOno.Text = DateTime.Now.Date.ToString("MM") + DateTime.Now.Date.ToString("yy");

    }

    /// <summary>
    /// value = CountryID
    /// item = CountryName
    /// </summary>
    /// <param name="DDLName"></param>

    protected void LoadDDLCountry(DropDownList DDLName)
    {
        //InsDataContext  idc = new InsDataContext ();
        DDLName.DataSource = (from a in idc.Countries
                              orderby a.CountryName
                              select a);
        DDLName.DataBind();
        ListItem firstItem = new ListItem("  -- Select Country --", "0");
        DDLName.Items.Insert(0, firstItem);
        DDLName.SelectedIndex = 0;
    }

    /// <summary>
    /// value = SecurityQuestionID
    /// item = SecurityQuestionText
    /// </summary>
    protected void LoadDDLSecurityQuestion()
    {

        ddlSecurityQuestion.DataSource = (from a in idc.SecurityQuestions where a.Active select a);
        ddlSecurityQuestion.DataBind();
        ListItem firstItem = new ListItem("  -- Select One --", "0");
        ddlSecurityQuestion.Items.Insert(0, firstItem);
        ddlSecurityQuestion.SelectedIndex = 0;

    }

    protected void LoadDDLState(DropDownList DDLName, string SelectedCountryID)
    {
        ListItem firstItem = new ListItem("  -- Select State --", "0");
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
        DDLName.Items.Insert(0, firstItem);
    }

    /// <summary>
    /// value = CustomerTypeID
    /// item = CustomerTypeCode
    /// </summary>
    protected void LoadDDLCustomerType()
    {
        this.ddlCustomerType.DataSource = (from a in idc.CustomerTypes
                                           where a.Active == true
                                           orderby a.CustomerTypeDesc
                                           select a);
        ddlCustomerType.DataBind();
        ListItem firstItem = new ListItem("-- Select --", "0");
        ddlCustomerType.Items.Insert(0, firstItem);

        //ddlCustomerType.SelectedIndex = 1;
    }

    private void LoadDDLAccountType()
    {
        ddlAccountType.DataSource = Mirion.DSD.GDS.API.AccountRequests.GetMRNAccountTypes();
        ddlAccountType.DataTextField = "AccountTypeName";
        ddlAccountType.DataValueField = "AccountTypeID";
        ddlAccountType.DataBind();
    }

    private void LoadDDLCustomerGroup()
    {
        ddlCustomerGroup.DataSource = Mirion.DSD.GDS.API.AccountRequests.GetMRNCustomerGroups();
        ddlCustomerGroup.DataTextField = "CustomerGroupName";
        ddlCustomerGroup.DataValueField = "CustomerGroupID";
        ddlCustomerGroup.DataBind();
    }

    /// <summary>
    /// 8/2013 WK.
    /// value = UnixCustomerTypeID
    /// item = CustomerTypeCode
    /// </summary>
    protected void loadDDLUnixCustomerType()
    {
        //this.ddlUnixCustomerType.DataSource = (from a in idc.UnixCustomerTypes
        //                                       where a.Active == true
        //                                       orderby a.UnixCustomerDescription
        //                                       select a);

        //ddlUnixCustomerType.DataBind();

        //ddlUnixCustomerType.Items.Insert(0, new ListItem("-- Select --", "0"));

    }



    protected void loadDDLIndustryType()
    {

        this.ddlIndustryType.DataSource = (from a in idc.Industries
                                           orderby a.IndustryName
                                           select a);

        ddlIndustryType.DataBind();

        ddlIndustryType.Items.Insert(0, new ListItem("-- Select --", "0"));

    }

    //**********************************************************************************
    // 8/2013 WK - Extract active and current (not expired rate codes only!).
    //**********************************************************************************
    protected void LoadDDLRateCode(int BrandID)
    {
        //10/2013 WK - Some new accounts may use custom rate codes used only for the account.
        //   Implement after 10/15/13 Instadose build.

        //use standard rates for brand.
        if (!chkCustomRate.Checked)
        {
            //**********************************************************************************
            ddlRateCode.DataSource = (from a in idc.Rates
                                      where a.BrandSourceID == BrandID
                                      //&& a.Active && a.ExpirationDate >= DateTime.Now
                                      orderby a.RateDesc
                                      select a);

            if (ddlRateCode.DataSource != null)
            {
                ddlRateCode.DataBind();

                ListItem firstItem = new ListItem("-- Select Rate --", "0");
                ddlRateCode.Items.Insert(0, firstItem);

                if (BrandID == 1) //Quantum default
                {
                    //If new accuont and no discounts entered then default for Quantum.
                    if (AccountID == 0 && gvDiscounts.Rows.Count == 0)
                        ddlRateCode.SelectedIndex = ddlRateCode.Items.IndexOf(ddlRateCode.Items.FindByText("Quantum List - FY12"));
                }
                else if (BrandID == 2)  //Mirion
                {
                    //If new accuont and no discounts entered then default for Mirion.
                    if (AccountID == 0 && gvDiscounts.Rows.Count == 0)
                        ddlRateCode.SelectedIndex = ddlRateCode.Items.IndexOf(ddlRateCode.Items.FindByText("TBL 30 - Film Quarterly"));
                }
                else if (BrandID == 3)  //IC Care Default
                {
                    //If new accuont and no discounts entered then default for IC Care.
                    if (AccountID == 0 && gvDiscounts.Rows.Count == 0)
                        ddlRateCode.SelectedIndex = ddlRateCode.Items.IndexOf(ddlRateCode.Items.FindByText("IC Care List - 2019"));
                }

                if (!ddlRateCode.SelectedItem.Text.Contains("Select"))
                    checkRateCode(int.Parse(ddlRateCode.SelectedValue));
            }
        }
        //******************************************************************************************
        //   10/2013 WK - to be implemented after 10/15/13 Instadose build.
        //******************************************************************************************
        else
        {
            //New Account?  Select list of custom rate codes that are have NOT been set yet.

            int brandSourceID = int.Parse(ddlBrand.SelectedValue.ToString());

            if (AccountID == 0)
            {
                ddlRateCode.DataSource = (from a in idc.Rates
                                          where a.CustomRateCode == true
                                          && a.CustomAccountID == null
                                          && a.BrandSourceID == brandSourceID
                                          orderby a.RateDesc
                                          select a);

                ddlRateCode.DataBind();

                ListItem firstItem = new ListItem("-- Select Rate --", "0");
                ddlRateCode.Items.Insert(0, firstItem);
            }
            else
            {
                //Has the custom rate been previously set?

                var acctRate = (from a in idc.Rates
                                where a.CustomAccountID == AccountID
                                orderby a.RateDesc
                                select a).ToList().FirstOrDefault();

                if (acctRate == null)
                {
                    //Custom rate account has not been set.  Get list of available custom rates.
                    ddlRateCode.DataSource = (from a in idc.Rates
                                              where a.CustomRateCode == true
                                              && a.CustomAccountID == null
                                              && a.BrandSourceID == brandSourceID
                                              orderby a.RateDesc
                                              select a);
                }
                else
                {
                    ddlRateCode.DataSource = acctRate;
                }

                ddlRateCode.DataBind();

                ListItem firstItem = new ListItem("-- Select Rate --", "0");
                ddlRateCode.Items.Insert(0, firstItem);
            }
        }
    }

    protected void LoadDDLSchedule()
    {
        this.ddlSchedule.DataSource = (from c in idc.Schedules
                                       where c.Active == true
                                       orderby c.ScheduleName
                                       select c);
        this.ddlSchedule.DataTextField = "ScheduleName";
        this.ddlSchedule.DataValueField = "ScheduleID";
        this.ddlSchedule.DataBind();
        ListItem firstItem = new ListItem("  -- Select Schedule Name --", "0");
        ddlSchedule.Items.Insert(0, firstItem);
        ddlSchedule.SelectedIndex = 0;
    }

    protected void LoadDDLProductGroup()
    {
        this.ddlProductGroup.DataSource = (from c in idc.ProductGroups
                                           where c.ProductGroupName != "Instadose 1"
                                           orderby c.ProductGroupName
                                           select c);
        this.ddlProductGroup.DataTextField = "ProductGroupName";
        this.ddlProductGroup.DataValueField = "ProductGroupID";
        this.ddlProductGroup.DataBind();
        ListItem firstItem = new ListItem("  -- Select Product Group --", "0");
        ddlProductGroup.Items.Insert(0, firstItem);
        ddlProductGroup.SelectedIndex = 0;
    }

    protected void LoadDDLLabel(string productGroupID)
    {
        try
        {
            this.ddlLabel.Items.Clear();

            var label = from a in idc.DeviceLabels
                        join b in idc.LabelTypes on a.LabelTypeID equals b.LabelTypeID
                        where b.ProductGroupID == int.Parse(productGroupID)
                        orderby a.LabelDesc
                        select new
                        {
                            a.LabelID,
                            mLabelDesc = a.LabelDesc + (b.LabelTypeName == "No Label" ? "" : " -- " + b.LabelTypeName)
                        };

            this.ddlLabel.DataSource = label;
            this.ddlLabel.DataTextField = "mLabelDesc";
            this.ddlLabel.DataValueField = "LabelID";
            this.ddlLabel.DataBind();
            ListItem firstItem = new ListItem("  -- Select Label --", "0");
            ddlLabel.Items.Insert(0, firstItem);
            ddlLabel.SelectedIndex = 0;

        }
        catch
        {
            
        }

    }

    protected void SetDefaultLabelByProductGroupID(string productGroupID)
    {
        ProductGroup myProductGroup = (from a in idc.ProductGroups
                                       where a.ProductGroupID == int.Parse(productGroupID)
                                       select a).FirstOrDefault();
        if (myProductGroup != null)
        {
            this.ddlLabel.SelectedValue = (myProductGroup.DefaultLabelID != null ? myProductGroup.DefaultLabelID.ToString() : "0");
        }
    }

    protected void LoadDDLPackageType()
    {

        this.ddlPackageType.DataSource = (from c in idc.PackageTypes
                                          orderby c.PackageDesc
                                          select c);
        ddlPackageType.DataBind();
        ListItem firstItem = new ListItem("-- Select Package Type --", "0");
        ddlPackageType.Items.Insert(0, firstItem);
        ddlPackageType.SelectedIndex = 0;
    }

    protected void LoadDDLShippingOption()
    {

        this.ddlShippingOption.DataSource = (from c in idc.ShippingOptions
                                             orderby c.ShippingOptionDesc
                                             select c);
        ddlShippingOption.DataBind();
        ListItem firstItem = new ListItem("-- Shipping Option --", "0");
        ddlShippingOption.Items.Insert(0, firstItem);
        ddlShippingOption.SelectedIndex = 0;
    }

    protected void LoadDDLShippingCarrier()
    {

        this.ddlShippingCarrier.DataSource = (from c in idc.ShippingMethods
                                              where c.PrefShipping == 1
                                              select new
                                              {
                                                  Carrier = c.CarrierDesc
                                              }).Distinct();
        ddlShippingCarrier.DataTextField = "Carrier";
        ddlShippingCarrier.DataValueField = "Carrier";
        ddlShippingCarrier.DataBind();
        ListItem firstItem = new ListItem("-- Select Shipping Carrier --");
        ddlShippingCarrier.Items.Insert(0, firstItem);
        ddlShippingCarrier.SelectedIndex = 0;
    }

    protected void ddlCountryOnSelectedIndexChange(object sender, System.EventArgs e)
    {
        DropDownList ddlC = (DropDownList)sender;
        DropDownList ddlS = (ddlC.ID == "ddlCountryB") ? ddlStateB : ddlStateS;

        LoadDDLState(ddlS, ddlC.SelectedValue);
        //LoadDDLState(ddlStateS, "0"); 
    }

    protected void LoadDDLDealer(DropDownList DDLName)
    {
        DDLName.DataSource = (from a in idc.Dealers
                              where a.Active == true
                              orderby a.DealerName
                              //select new { a.DealerID, DealerIDName = a.DealerID.ToString() + " - " + a.DealerName.ToString() });
                              select new { a.DealerID, DealerIDName = a.DealerName.ToString() });

        DDLName.DataBind();
        ListItem firstItem = new ListItem("-- Select Dealer --", "0");
        DDLName.Items.Insert(0, firstItem);

    }

    protected void ddlDealerOnSelectedIndexChange(object sender, System.EventArgs e)
    {
        // fill Billing Info as Dealer Info.
        int SelectedDealerID = int.Parse(ddlDealer.SelectedValue);
        loadDealerBillingInfo(SelectedDealerID);

    }

    protected void EnableBillingFields(bool EnableFileds)
    {
        //ckbxCopyCompany.Disabled = (EnableFileds == true ? false : true);
        ckbxCopyCompany.Enabled = EnableFileds;
        txtCompanyNameB.Enabled = EnableFileds;
        ddlGenderB.Enabled = EnableFileds;
        txtFirstB.Enabled = EnableFileds;
        txtLastB.Enabled = EnableFileds;
        txtAddress1B.Enabled = EnableFileds;
        txtAddress2B.Enabled = EnableFileds;
        txtAddress3B.Enabled = EnableFileds;
        ddlCountryB.Enabled = EnableFileds;
        txtCityB.Enabled = EnableFileds;
        ddlStateB.Enabled = EnableFileds;
        txtPostalB.Enabled = EnableFileds;
        txtTelephoneB.Enabled = EnableFileds;
        txtFaxB.Enabled = EnableFileds;
        // Modified 09/17/2013 by Anuradha Nandi
        // Disables the txtEmailB TextBox when a ICCare Dealer is chosen.
        txtEmailAddressB.Enabled = EnableFileds;
    }



    /// <summary>
    /// For IC Care Dealers.
    /// </summary>
    /// <param name="EnableFileds"></param>
    protected void EnableDealerFields(bool EnableFileds)
    {
        this.dealerRow.Visible = EnableFileds;
        this.lblDealer.Visible = EnableFileds;
        this.ddlDealer.Visible = EnableFileds;
    }

    // Modified 09/17/2013 by Anuradha Nandi.
    // Includes the Dealer's E-mail Address.
    protected void loadDealerBillingInfo(int dealerID)
    {
        // fill Billing Info as Dealer Info.
        //var dealerInfo = (from a in idc.Dealers
        //                  where a.DealerID == dealerid
        //                  select a).FirstOrDefault();

        //9/2013 WK - Dev and Staging DB:  Dealers.StateID is null for many of the dealers since it is
        //  for testing.  Will ignore nulls since StateID in prod DB DOES NOT have nulls!
        var dealerInfo = (from a in idc.Dealers
                          where a.DealerID == dealerID
                          select new
                          {
                              DealerID = a.DealerID,
                              DealerName = a.DealerName,
                              Prefix = a.Prefix,
                              FirstName = a.FirstName,
                              LastName = a.LastName,
                              Address1 = a.Address1,
                              Address2 = a.Address2,
                              Address3 = a.Address3,
                              CountryID = a.CountryID,
                              StateID = a.StateID,
                              City = a.City,
                              PostalCode = a.PostalCode,
                              Telephone = a.Telephone,
                              Fax = a.Fax,
                              Email = a.Email
                          }).FirstOrDefault();

        if (dealerInfo == null) return;

        string dealerCompanyNameBilling = (dealerInfo.DealerName != null) ? dealerInfo.DealerName : "";
        string dealerFirstNameBilling = (dealerInfo.FirstName != null) ? dealerInfo.FirstName : "";
        string dealerLastNameBilling = (dealerInfo.LastName != null) ? dealerInfo.LastName : "";
        string dealerAddress1Billing = (dealerInfo.Address1 != null) ? dealerInfo.Address1 : "";
        string dealerAddress2Billing = (dealerInfo.Address2 != null) ? dealerInfo.Address2 : "";
        string dealerAddress3Billing = (dealerInfo.Address3 != null) ? dealerInfo.Address3 : "";

        string dealerPrefix = (dealerInfo.Prefix != null) ? dealerInfo.Prefix : "";

        string dealerCityBilling = (dealerInfo.City != null) ? dealerInfo.City : "";
        string dealerPostalCodeBilling = (dealerInfo.PostalCode != null) ? dealerInfo.PostalCode : "";
        string dealerTelephoneNumberBilling = (dealerInfo.Telephone != null) ? dealerInfo.Telephone : "";
        string dealerFaxBilling = (dealerInfo.Fax != null) ? dealerInfo.Fax : "";
        string dealerEmailAddressBilling = (dealerInfo.Email != null) ? dealerInfo.Email : "";

        ddlGenderB.SelectedValue = dealerPrefix;

        txtCompanyNameB.Text = dealerCompanyNameBilling;
        txtFirstB.Text = dealerFirstNameBilling;
        txtLastB.Text = dealerLastNameBilling;
        txtAddress1B.Text = dealerAddress1Billing;
        txtAddress2B.Text = dealerAddress2Billing;
        txtAddress3B.Text = dealerAddress3Billing;
        ddlCountryB.SelectedValue = dealerInfo.CountryID.ToString();

        //// Modified by Tdo, 3/26/2013. Must reload States by country
        LoadDDLState(ddlStateB, ddlCountryB.SelectedValue);

        ddlStateB.SelectedValue = dealerInfo.StateID.ToString();
        txtCityB.Text = dealerCityBilling;
        txtPostalB.Text = dealerPostalCodeBilling;
        txtTelephoneB.Text = dealerTelephoneNumberBilling;
        txtFaxB.Text = dealerFaxBilling;
        txtEmailAddressB.Text = dealerEmailAddressBilling;

        //9/2013 WK.
        //disable required billing information fields for new IC Care account.
        ICCareBillingInfoValidation();
    }

    /// <summary>
    /// Load SalesRepDist address as billing address if needed. 
    /// </summary>
    /// <param name="salesRepDistID"></param>
    protected void loadResellerBillingInfo(string salesRepDistID)
    {
        var DisBt = (from a in idc.SalesRepDistributors
                     join c in idc.CommissionSetups on a.SalesRepDistID equals c.SalesRepDistID
                     where a.SalesRepDistID == ddlReferral.SelectedValue.ToString()
                     select new { a, c.RateID }).FirstOrDefault();

        // Do not load for IC Care customers.
        if (ddlBrand.SelectedValue == "3" && DisBt.RateID == null) return;

        if (DisBt.a.ChannelID == 8 || DisBt.RateID != null)
        {
            var salesRepInfo = DisBt.a;

            txtCompanyNameB.Text = salesRepInfo.CompanyName ?? "";
            txtFirstB.Text = salesRepInfo.FirstName ?? "";
            txtLastB.Text = salesRepInfo.LastName ?? "";
            txtAddress1B.Text = salesRepInfo.Address1 ?? "";
            txtAddress2B.Text = salesRepInfo.Address2 ?? "";
            txtAddress3B.Text = "";
            txtCityB.Text = salesRepInfo.City ?? "";
            txtPostalB.Text = salesRepInfo.PostalCode ?? "";
            txtTelephoneB.Text = salesRepInfo.Telephone ?? "";
            txtFaxB.Text = salesRepInfo.Fax ?? "";
            ddlCountryB.SelectedValue = (salesRepInfo.CountryID.HasValue) ? salesRepInfo.CountryID.Value.ToString() : "0";
            LoadDDLState(ddlStateB, ddlCountryB.SelectedValue);
            ddlStateB.SelectedValue = (salesRepInfo.StateID.HasValue) ? salesRepInfo.StateID.Value.ToString() : "0";

            if (DisBt.RateID != null)
            {
                EnableBillingFields(true);
                ddlRateCode.SelectedValue = DisBt.RateID.ToString();
                ddlGenderB.SelectedValue = DisBt.a.Prefix;
            }
            else
            {
                txtEmailAddressB.Text = salesRepInfo.Email ?? "";
                EnableBillingFields(false);
                ICCareBillingInfoValidation();
            }
        }
    }

    /// <summary>
    /// If new IC Care account,  disable billing required fields.
    /// </summary>
    private void ICCareBillingInfoValidation(bool enabled = false)
    {
        reqfldvalCompanyNameB.Enabled = enabled;
        reqfldvalLastNameB.Enabled = enabled;
        reqfldvalFirstNameB.Enabled = enabled;
        reqfldvalAddress1B.Enabled = enabled;
        reqfldvalCity.Enabled = enabled;
        reqfldvalStateB.Enabled = enabled;
        reqfldvalCountryB.Enabled = enabled;
        reqfldvalPostalB.Enabled = enabled;
        reqfldvalTelephoneB.Enabled = enabled;
        reqfldvalBillingEmailAddress.Enabled = enabled;
    }

    #endregion

    #region Miscellanous Functions - Methods

    private string GetAppSettings(string pAppKey)
    {
        var mySetting = (from AppSet in idc.AppSettings where AppSet.AppKey == pAppKey select AppSet).FirstOrDefault();
        return (mySetting != null) ? mySetting.Value : "";
    }

    public bool DisplayDeactivateButton()
    {
        Boolean isDisplay = false;

        // Query active directory to see if the current user belongs to a group.
        bool belongsToGroups = ActiveDirectoryQueries.UserExistsInAnyGroup(User.Identity.Name, activeDirecoryGroups);
        if (belongsToGroups)
        {
            isDisplay = true;
        }
        return isDisplay;
    }

    protected int CheckUserNameAvailability(string LoginID, int LoginUserID)
    {
        int ReturnVal = 0;
        int ExceptionUserID = LoginUserID;
        try
        {
            //Get authuser tied to user
            if (ExceptionUserID > 0)
            {
                var username = (from a in idc.Users
                                where a.UserID == ExceptionUserID
                                select a.UserName).FirstOrDefault();

                if (username == LoginID)
                {
                    return 0;
                }
                else
                {
                    ExceptionUserID = (from a in idc.AuthUsers
                                       where a.UserName == username && a.AuthUserApplicationGroups.Any(g => g.ApplicationGroupID == 7)
                                       select a.AuthUserID).FirstOrDefault();
                }
            }
            //this.idc = new InsDataContext ();
            int UserIDCount = (from a in idc.AuthUsers
                               where a.UserName == LoginID
                               && a.AuthUserID != ExceptionUserID
                               select a).Count();
            ReturnVal = UserIDCount;
        }
        catch
        {
        }
        return ReturnVal;
    }

    protected bool IsShippingCountryStateMatched()
    {
        bool isMatched = false;
        try
        {
            //this.idc = new InsDataContext ();
            int count = (from a in idc.States
                         where a.StateID == int.Parse(ddlStateS.SelectedValue)
                               && a.CountryID == int.Parse(ddlCountryS.SelectedValue)
                         select a).Count();
            if (count > 0)
                isMatched = true;
        }
        catch
        { }
        return isMatched;
    }

    protected void ConstructRegisterHashTable()
    {
        HashFormInfo = new Hashtable();
        HashFormInfo.Add("AccountName", this.txtAccountName.Text.Trim());
        HashFormInfo.Add("CompanyName", this.txtCompanyName.Text.Trim());
        HashFormInfo.Add("Referral", this.ddlReferral.SelectedValue.ToString());
        HashFormInfo.Add("CustomerType", this.ddlCustomerType.SelectedItem.ToString());

        /// 8 /2013 WK - New for Unix transition accounts.
        ///   Default to Record 1 (QN / New).
        ///   
        //if (!ddlUnixCustomerType.SelectedItem.Text.Contains("Select"))
        //    HashFormInfo.Add("UnixCustomerType", this.ddlUnixCustomerType.SelectedItem.ToString());
        //else
        //    HashFormInfo.Add("UnixCustomerType", "");

        if (!ddlIndustryType.SelectedItem.Text.Contains("Select"))
            HashFormInfo.Add("IndustryType", this.ddlIndustryType.SelectedItem.ToString());
        else
            HashFormInfo.Add("IndustryType", "");


        //HashFormInfo.Add("Industry", "4");
        HashFormInfo.Add("RateCode", (this.ddlRateCode.SelectedValue.ToString() != "0") ? this.ddlRateCode.SelectedItem.ToString() : "N/A");

        HashFormInfo.Add("ServiceStartDate", this.txt_ServiceStartDate.Text.ToString());
        HashFormInfo.Add("ServiceEndDate", this.txt_ServiceEndDate.Text.ToString());
        HashFormInfo.Add("Discount", this.txtDiscount.Text.ToString() + "%");

        HashFormInfo.Add("LoginID", this.txtLoginid.Text.Trim());
        HashFormInfo.Add("Password", this.txtPassword.Text);
        HashFormInfo.Add("rePassword", this.txtPasswordRe.Text);
        HashFormInfo.Add("SecurityQuestion", this.ddlSecurityQuestion.SelectedItem.Text);
        HashFormInfo.Add("SecurityAnswer", this.txtSecurityA.Text);

        HashFormInfo.Add("GenderA", this.ddlGenderA.SelectedItem.ToString());
        HashFormInfo.Add("FirstNameA", this.txtFirstA.Text.Trim());
        HashFormInfo.Add("LastNameA", this.txtLastA.Text.Trim());
        HashFormInfo.Add("Gender", (rbtnMale.Checked) ? "Male" : "Female");
        HashFormInfo.Add("Email", this.txtEmail.Text.Trim());
        HashFormInfo.Add("Telephone", this.txtTelephone.Text.Trim());
        HashFormInfo.Add("Fax", this.txtFax.Text.Trim());

        HashFormInfo.Add("CompanyNameB", this.txtCompanyNameB.Text.ToString());
        HashFormInfo.Add("GenderB", this.ddlGenderB.SelectedItem.ToString());
        HashFormInfo.Add("FirstNameB", this.txtFirstB.Text.Trim());
        HashFormInfo.Add("LastNameB", this.txtLastB.Text.Trim());
        HashFormInfo.Add("Address1B", this.txtAddress1B.Text.Trim());
        HashFormInfo.Add("Address2B", this.txtAddress2B.Text.Trim());
        HashFormInfo.Add("Address3B", this.txtAddress3B.Text.Trim());
        HashFormInfo.Add("CountryB", this.ddlCountryB.SelectedItem.ToString());
        HashFormInfo.Add("CityB", this.txtCityB.Text.Trim());
        HashFormInfo.Add("StateB", this.ddlStateB.SelectedItem.ToString());
        HashFormInfo.Add("ZipB", this.txtPostalB.Text.Trim());
        HashFormInfo.Add("TelephoneB", this.txtTelephoneB.Text.Trim());
        HashFormInfo.Add("FaxB", this.txtFaxB.Text.Trim());
        HashFormInfo.Add("EmailB", this.txtEmailAddressB.Text.Trim());

        HashFormInfo.Add("CompanyNameS", this.txtCompanyNameS.Text.ToString());
        HashFormInfo.Add("GenderS", this.ddlGenderS.SelectedItem.ToString());
        HashFormInfo.Add("FirstNameS", this.txtFirstS.Text.Trim());
        HashFormInfo.Add("LastNameS", this.txtLastS.Text.Trim());
        HashFormInfo.Add("Address1S", this.txtAddress1S.Text.Trim());
        HashFormInfo.Add("Address2S", this.txtAddress2S.Text.Trim());
        HashFormInfo.Add("Address3S", this.txtAddress3S.Text.Trim());
        HashFormInfo.Add("CountryS", this.ddlCountryS.SelectedItem.ToString());
        HashFormInfo.Add("CityS", this.txtCityS.Text.Trim());
        HashFormInfo.Add("StateS", this.ddlStateS.SelectedItem.ToString());
        HashFormInfo.Add("ZipS", this.txtPostalS.Text.Trim());
        HashFormInfo.Add("TelephoneS", this.txtTelephoneS.Text.Trim());
        HashFormInfo.Add("FaxS", this.txtFaxS.Text.Trim());
        HashFormInfo.Add("EmailS", this.txtEmailAddressS.Text.Trim());

        HashFormInfo.Add("BillingMethod", "Purchase Order");
        HashFormInfo.Add("PoNumber", this.txtPOno.Text.Trim());
        HashFormInfo.Add("CCType", null);
        HashFormInfo.Add("CCName", null);
        HashFormInfo.Add("CCNumber", null);

        HashFormInfo.Add("CCExpMonth", null);
        HashFormInfo.Add("CCExpYear", null);
        HashFormInfo.Add("CCSecurityCode", null);
        HashFormInfo.Add("CCNumberEncrypted", null);
        HashFormInfo.Add("MashCCNumber", null);

        HashFormInfo.Add("BillingTermValue", this.ddlBillingTerm.SelectedValue.ToString());
        HashFormInfo.Add("BillingTermItem", this.ddlBillingTerm.SelectedItem.ToString());

        HashFormInfo.Add("Currency", (this.ddlRateCode.SelectedValue == "0") ? "USD" : (from r in idc.Rates where r.RateID == int.Parse(ddlRateCode.SelectedValue) select r.CurrencyCode).FirstOrDefault());

        HashFormInfo.Add("PageUser", UserName);

    }

    protected void DisplayRegisterConfirmation(int NewAccountID, string confirmMessage)
    {
        divMainForm.Visible = false;
        divConfirmationForm.Visible = true;

        lbl_CF_Message.Text = confirmMessage;
        lbl_CF_accountid.Text = NewAccountID.ToString();
        lbl_CF_username.Text = HashFormInfo["LoginID"].ToString();

        lbl_CF_loginid.Text = HashFormInfo["LoginID"].ToString();
        lbl_CF_AccoutName.Text = HashFormInfo["AccountName"].ToString();
        lbl_CF_Company.Text = HashFormInfo["CompanyName"].ToString();
        lbl_CF_Name.Text = HashFormInfo["GenderA"].ToString() + " " + HashFormInfo["FirstNameA"].ToString() + " " + HashFormInfo["LastNameA"].ToString();
        lbl_CF_Gender.Text = HashFormInfo["Gender"].ToString();
        lbl_CF_Telephone.Text = HashFormInfo["Telephone"].ToString();
        lbl_CF_Fax.Text = HashFormInfo["Fax"].ToString();
        lbl_CF_email.Text = HashFormInfo["Email"].ToString();
        lbl_CF_CustomerType.Text = HashFormInfo["CustomerType"].ToString();

        /// 8/2013 WK - New for Unix transition accounts.
        //lbl_CF_UnixCutomerType.Text = HashFormInfo["UnixCustomerType"].ToString();

        lbl_CF_AccountType.Text = HashFormInfo["IndustryType"].ToString();

        lbl_CF_Currency.Text = HashFormInfo["Currency"].ToString();

        lbl_CF_BCycle.Text = HashFormInfo["BillingTermItem"].ToString();
        lbl_CF_BCompany.Text = HashFormInfo["CompanyNameB"].ToString();
        lbl_CF_BName.Text = HashFormInfo["GenderB"].ToString() + " " + HashFormInfo["FirstNameB"].ToString() + " " + HashFormInfo["LastNameB"].ToString();
        lbl_CF_BTelephone.Text = HashFormInfo["TelephoneB"].ToString();
        lbl_CF_BFax.Text = HashFormInfo["FaxB"].ToString();
        lbl_CF_BEmail.Text = HashFormInfo["EmailB"].ToString();

        lbl_CF_ServiceStartDate.Text = HashFormInfo["ServiceStartDate"].ToString();
        lbl_CF_ServiceEndDate.Text = HashFormInfo["ServiceEndDate"].ToString();
        lbl_CF_RateCode.Text = HashFormInfo["RateCode"].ToString();
        lbl_CF_Discount.Text = HashFormInfo["Discount"].ToString();

        string AddressB = HashFormInfo["Address1B"].ToString() + "<br>";
        AddressB += (HashFormInfo["Address2B"].ToString().Trim() == "") ? "" : HashFormInfo["Address2B"].ToString() + "<br>";
        AddressB += (HashFormInfo["Address3B"].ToString().Trim() == "") ? "" : HashFormInfo["Address3B"].ToString() + "<br>";
        AddressB += HashFormInfo["CityB"].ToString() + ", ";
        AddressB += HashFormInfo["StateB"].ToString() + " ";
        AddressB += HashFormInfo["ZipB"].ToString() + "<br>";
        AddressB += HashFormInfo["CountryB"].ToString();
        lbl_CF_BAddress.Text = AddressB;

        lbl_CF_SCompany.Text = HashFormInfo["CompanyNameS"].ToString();
        lbl_CF_SName.Text = HashFormInfo["GenderS"].ToString() + " " + HashFormInfo["FirstNameS"].ToString() + " " + HashFormInfo["LastNameS"].ToString();
        lbl_CF_STelephone.Text = HashFormInfo["TelephoneS"].ToString();
        lbl_CF_SFax.Text = HashFormInfo["FaxS"].ToString();
        lbl_CF_SEmail.Text = HashFormInfo["EmailS"].ToString();

        string AddressS = HashFormInfo["Address1S"].ToString() + "<br>";
        AddressS += (HashFormInfo["Address2S"].ToString().Trim() == "") ? "" : HashFormInfo["Address2S"].ToString() + "<br>";
        AddressS += (HashFormInfo["Address3S"].ToString().Trim() == "") ? "" : HashFormInfo["Address3S"].ToString() + "<br>";
        AddressS += HashFormInfo["CityS"].ToString() + ", ";
        AddressS += HashFormInfo["StateS"].ToString() + " ";
        AddressS += HashFormInfo["ZipS"].ToString() + "<br>";
        AddressS += HashFormInfo["CountryS"].ToString();

        lbl_CF_SAddress.Text = AddressS;

        hdnAccountID.Value = NewAccountID.ToString();
    }

    /// <summary>
    /// Check rate code to if active and not expired.
    /// </summary>
    /// <param name="rateID"></param>
    private void checkRateCode(int? rateID)
    {
        var acctInfo = (from ac in idc.Accounts
                        where ac.AccountID == AccountID
                        select ac).FirstOrDefault();

        var rateInfo = (from r in idc.Rates
                        where r.RateID == rateID
                        select r).FirstOrDefault();

        this.lblInvalidRate.Text = "";

        if (!ddlRateCode.SelectedItem.Text.Contains("Select"))
            btnAddDiscount.Visible = true;
        else
            btnAddDiscount.Visible = false;

        //if (AccountID > 0)
        //{
        divAddAndSearchToolbar.Visible = true;
        gvDiscounts.Visible = true;
        //}

        //clearError();

        //9/2013 WK - Does this rate code even allow discounts?
        if (!checkRateCodeAllowDiscount(rateID))
        {
            this.lblInvalidRate.Text = "This rate code does not allow discounts.";

            btnAddDiscount.Visible = false;
            divAddAndSearchToolbar.Visible = false;
            gvDiscounts.Visible = false;

            //Reset stored session account discounts if new account.
            Session["Discounts"] = null;

        }
        else
        {
            //9/2013 WK - Check if rate code is active and not expired.
            if (!rateInfo.Active)
            {
                //this.displayError("Rate is not active.  Cannot add discounts.");
                this.lblInvalidRate.Text = "Rate is not active.  Cannot add discounts.";

                btnAddDiscount.Visible = false;
                divAddAndSearchToolbar.Visible = false;
                gvDiscounts.Visible = false;

                //Reset stored session account discounts if new account.
                Session["Discounts"] = null;
            }
            else if (rateInfo.ExpirationDate < DateTime.Now)
            {
                this.lblInvalidRate.Text = "Rate is expired.  Cannot add discounts.";

                //this.displayError("Rate is expired.  Cannot add discounts.");
                btnAddDiscount.Visible = false;
                divAddAndSearchToolbar.Visible = false;
                gvDiscounts.Visible = false;

                //Reset stored session account discounts if new account.
                Session["Discounts"] = null;
            }
        }
    }

    /// <summary>
    /// checkRateCodeAllowDiscount - Checks to see if ANY of the Rate detail records 
    ///     for given rate allows discount.
    /// </summary>
    /// <param name="rateID"></param>
    private bool checkRateCodeAllowDiscount(int? rateID)
    {
        int rateDiscounts = (from rd in idc.RateDetails
                             where rd.RateID == rateID
                             && rd.AllowDiscount
                             select rd).Count();

        if (rateDiscounts > 0)
            return true;
        else
            return false;
    }

    /// <summary>
    /// 9/2013 WK - update discounts in the discounts grid.
    /// </summary>
    private void updateDiscount()
    {
        Account account = (from a in idc.Accounts where a.AccountID == AccountID select a).FirstOrDefault();
        //int productGroupID = 0;
        float discount = 0;

        foreach (GridViewRow rowitem in gvDiscounts.Rows)
        {
            // Get each line item's grid row and update the price
            HiddenField hidProductGroupID = (HiddenField)rowitem.FindControl("hidProductGroupID");
            HiddenField hidDiscount = (HiddenField)rowitem.FindControl("hidDiscount");

            discount = float.Parse(hidDiscount.Value);

            Label lblDiscount = (Label)(rowitem.Cells[1].FindControl("lblDiscount"));

            lblDiscount.Text = hidDiscount.Value.ToString();
        }
    }

    private void displayError(string error)
    {
        errorMsg.InnerText = error;
        this.error.Visible = true;
    }

    private void clearError()
    {
        errorMsg.InnerText = "";
        this.error.Visible = false;
    }

    #endregion

    private void InvisibleErrors_discountsDialog()
    {
        // Reset submission form error message
        this.discountDialogErrors.Visible = false;
        this.discountDialogErrors.InnerHtml = "";
    }

    private void VisibleErrors_discountsDialog(string error)
    {
        //errorMsg.InnerText = string.Format("An error was encountered: {0}", error);
        this.discountDialogErrors.InnerHtml = error;
        this.discountDialogErrors.Visible = true;
    }

    private void InvisibleMsgs_discountsDialog()
    {
        // Reset submission form error message
        this.discountDialogErrors.Visible = false;
        this.discountDialogErrors.InnerHtml = "";
    }

    private void VisibleMsgs_discountsDialog(string message)
    {
        //errorMsg.InnerText = string.Format("An error was encountered: {0}", error);
        this.discountDialogErrors.InnerHtml = message;
        this.discountDialogErrors.Visible = true;
    }

    // 9/2013 WK.
    //Calculate discounts.
    protected void btnCalculateDiscount_Click(object sender, EventArgs e)
    {
        int productGroupID = 0;

        if (ddlProductGroup.SelectedItem.Text.Contains("Select")) return;
        else
            productGroupID = int.Parse(ddlProductGroup.SelectedValue);

        txtDiscount.Text = CalculateDiscount(productGroupID);
    }

    /// <summary>
    /// 9/2013 - Caolcuate discount if button is pushed.
    /// </summary>
    /// <param name="productGroupID"></param>
    /// <returns></returns>
    protected string CalculateDiscount(int productGroupID)
    {
        string myReturn = "0";
        string strInitQty = (txtInitialQty.Text.Trim() == "0") ? "1" : txtInitialQty.Text.Trim();
        string strTargetPrice = txtTargetPrice.Text.Trim();
        string strDiscount = txtDiscount.Text.Trim();

        var ratetable = (from a in idc.RateDetails
                         join b in idc.Products
                         on a.ProductGroupID equals b.ProductGroupID
                         where a.RateID == int.Parse(ddlRateCode.SelectedValue)
                         && a.MinQty <= int.Parse(strInitQty)
                         && a.MaxQty >= int.Parse(strInitQty)
                         && b.ProductGroupID == productGroupID
                         //&& (b.ProductID <= 5 ||
                         //b.ProductID >= 14)
                         && a.Active == true && b.Active == true
                         select a).FirstOrDefault();


        if (ratetable != null)
        {
            decimal myTargetPrice;
            if (decimal.TryParse(strTargetPrice, out myTargetPrice) == true)
            {
                decimal mydiscount = ((1 - (decimal.Parse(strTargetPrice) / decimal.Parse(ratetable.Price.ToString()))) * 100);
                myReturn = (mydiscount <= 0) ? "0" : mydiscount.ToString("#.####");
            }
        }

        return myReturn;
    }

    protected void btnDiscountSave_Click(object sender, EventArgs e)
    {
        int productGroupID = 0;
        int rateID = 0;

        if (!validateInputs_discountDialog()) return;

        AccountDiscount accDisc = new AccountDiscount();

        lblErrorMessageModal.Text = "";

        decimal discount = 0;

        productGroupID = Convert.ToInt32(ddlDiscountProductGroup.SelectedValue);
        rateID = Convert.ToInt32(ddlRateCode.SelectedValue);

        //txtDiscount.Text = string.Format(("#.####");

        if (!decimal.TryParse(txtDiscount.Text, out discount)) return;

        //9/2013 WK - Store AccountDiscount only if AccountID is known.
        //   If new account; save all Account and other necessary data before allowing adding of
        //   discount(s).
        if (AccountID > 0)
        {
            // Update account information
            Account account = null;
            account = (from a in idc.Accounts
                       where a.AccountID == AccountID
                       select a).First();


            account.DefaultRateID = (this.ddlRateCode.SelectedValue.ToString() == "0")
                                 ? (int?)null : int.Parse(ddlRateCode.SelectedValue.ToString());

            idc.SubmitChanges();

            if (hidMode.Value == "Add")
            {
                //New Product Group ID exists?  If so,  deactive previous entry.
                if (prodGroupDiscountExists(productGroupID))
                {
                    AccountDiscount acctDiscounts = (from ad in idc.AccountDiscounts
                                                     where ad.AccountID == account.AccountID &&
                                                     ad.ProductGroupID == productGroupID &&
                                                     ad.Active
                                                     select ad).FirstOrDefault();

                    // Change order to PO for re-processing.
                    if (acctDiscounts != null)
                    {
                        acctDiscounts.Active = false;
                        idc.SubmitChanges();
                    }
                    //return;
                }

                accDisc = new AccountDiscount()
                {
                    AccountID = AccountID,
                    ProductGroupID = productGroupID,
                    Discount = discount,
                    CreatedBy = UserName,
                    CreatedDate = DateTime.Today,
                    Active = true,
                };

                idc.AccountDiscounts.InsertOnSubmit(accDisc);

                idc.SubmitChanges();

                //updateDiscount();

                loadDiscountGrid();
            }

            // Close the dialog.
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID,
                    "$('#divModalDiscount').dialog('close');", true);
        }
        else
        {
            //Does discount exists? (i.e. prevent duplicate discount for new account?)
            if (doesNewAcctDiscountExists(productGroupID))  //yes!  update only.
                updateNewAcctDiscount(productGroupID, ddlDiscountProductGroup.SelectedItem.Text,
                                        discount);
            else  //No. Add new discount.
                addNewRowToGrid(productGroupID, ddlDiscountProductGroup.SelectedItem.Text,
                                    discount);

            loadDiscountGrid();

            // Close the dialog.
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID,
                    "$('#divModalDiscount').dialog('close');", true);

        }

        divAddAndSearchToolbar.Visible = true;
        btnAddDiscount.Visible = true;

        //10/2013 WK - implement after 10/15/13 build.
        //***********************************************************************
        //Disable check box if custom rate code was used for this account.
        if (chkCustomRate.Checked) chkCustomRate.Enabled = false;
        //***********************************************************************
    }

    private bool prodGroupDiscountExists(int productGroupID)
    {
        //Is there entry for this product group and account?
        int prodGroup = (from a in idc.AccountDiscounts
                         where
                           a.AccountID == AccountID &&
                           a.ProductGroupID == productGroupID
                         select a).Count();

        if (prodGroup > 0)
            return true;
        else
            return false;
    }

    protected void btnDiscountCancel_Click(object sender, EventArgs e)
    {
        loadDiscountGrid();
    }

    protected void btnAddDiscount_Click(object sender, EventArgs e)
    {
        if (ddlCustomerType.SelectedItem.Text.Trim() == "CON")
        {
            this.displayError(this.errorMsg.InnerText + "Error: for Customer Type = CON, cannot add discount!.");
            return;
        }


        //Must select rate code if account has no default rate code!
        if (ddlRateCode.SelectedItem.Text.Contains("Select")) return;

        loadDiscountSelectionDialog();

        hidProductGroupID.Value = "0";
        ddlDiscountProductGroup.Items.Insert(0, new ListItem("--Select--", ""));

        ddlDiscountProductGroup.Enabled = true;

        hidMode.Value = "Add";

        //open add discount modal form.
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('#divModalDiscount')", true);
    }

    /// <summary>
    /// Load the selection dialog and display to the user.
    /// </summary>
    private void loadDiscountSelectionDialog()
    {
        //Must select rate code if account has no default rate code!
        if (ddlRateCode.SelectedItem.Text.Contains("Select")) return;

        // Select the location.
        lblRateCode.Text = this.ddlRateCode.SelectedItem.Text;

        int rateCode = 0;

        if (int.TryParse(ddlRateCode.SelectedValue, out rateCode))
        {
            ddlDiscountProductGroup.DataSource = (from pg in idc.ProductGroups
                                                  join rd in idc.RateDetails
                                                  on pg.ProductGroupID equals rd.ProductGroupID
                                                  where rd.AllowDiscount && rd.Active
                                                  && rd.RateID == rateCode
                                                  orderby pg.ProductGroupID
                                                  select new
                                                  {
                                                      ProductGroupID = pg.ProductGroupID,
                                                      ProductGroupName = pg.ProductGroupName
                                                  }).Distinct();

            ddlDiscountProductGroup.DataTextField = "ProductGroupName";
            ddlDiscountProductGroup.DataValueField = "ProductGroupID";
            ddlDiscountProductGroup.DataBind();
        }

        InvisibleErrors_discountsDialog();
        txtDiscount.Text = "0";

        //loadDiscountGrid();

        // Force the select dialog to open on initial load.
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID,
                "$('#divModalDiscount').dialog('open');", true);

    }

    protected void addFirstDiscountToGrid(int productGroupID,
                string productGroupName, decimal discount)
    {
        DataTable dtDiscounts = new DataTable();

        // Create the review orders datatable to hold the contents of the order.
        dtDiscounts = new DataTable("Discounts Table");

        // Create the columns for the review orders datatable.
        dtDiscounts.Columns.Add("AccountID", Type.GetType("System.Int32"));
        dtDiscounts.Columns.Add("ProductGroupID", Type.GetType("System.Int32"));
        dtDiscounts.Columns.Add("ProductGroupName", Type.GetType("System.String"));
        dtDiscounts.Columns.Add("Discount", Type.GetType("System.Decimal"));
        dtDiscounts.Columns.Add("CreatedBy", Type.GetType("System.String"));
        dtDiscounts.Columns.Add("CreatedDate", Type.GetType("System.String"));

        // Create a new review order row.
        DataRow dr = dtDiscounts.NewRow();

        // Fill row details.
        dr["AccountID"] = "0";   // accountID.ToString();
        dr["ProductGroupID"] = productGroupID.ToString();
        dr["ProductGroupName"] = productGroupName.ToString();
        dr["Discount"] = discount.ToString();
        dr["CreatedBy"] = UserName;
        dr["CreatedDate"] = String.Format("{0:MM/dd/yyyy}", DateTime.Now);

        // Add row to the data table.
        dtDiscounts.Rows.Add(dr);

        gvDiscounts.DataSource = dtDiscounts;

        //Stored data table in session for new account creation.
        Session["Discounts"] = dtDiscounts;

        gvDiscounts.DataBind();

    }

    /// <summary>
    /// 9/2013 WK - for new accounts, add new discount record for grid and keep in session
    ///     until account is created.
    /// </summary>
    /// <param name="productGroupID"></param>
    /// <param name="productGroupName"></param>
    /// <param name="discount"></param>
    private void addNewRowToGrid(int productGroupID,
                string productGroupName, decimal discount)
    {
        setprevData();

        DataTable dt = new DataTable();

        if (Session["Discounts"] == null)
        {
            addFirstDiscountToGrid(productGroupID, productGroupName, discount);
        }
        else
        {
            dt = (DataTable)Session["Discounts"];

            DataRow drDiscounts = dt.NewRow();

            drDiscounts["AccountID"] = "0";   // accountID.ToString();
            drDiscounts["ProductGroupID"] = productGroupID.ToString();

            drDiscounts["ProductGroupName"] = productGroupName.ToString();

            //***************************************************************************
            //12/5/12 WK - passed back as varchar(10) - string.  Not accurate to sort.
            //  Now includes LEFT OUTER JOIN - accounts with no orders.
            //***************************************************************************
            drDiscounts["Discount"] = discount.ToString();
            drDiscounts["CreatedBy"] = UserName;
            drDiscounts["CreatedDate"] = String.Format("{0:MM/dd/yyyy}", DateTime.Now);

            dt.Rows.Add(drDiscounts);

            //9/2013 WK
            //Do not update target price since account is being created.
            //updateTargetPrices();

            Session["Discounts"] = dt;
        }
    }

    protected void setprevData()
    {
        DataTable prevdata = (DataTable)Session["Discounts"];

        for (int i = 0; i < gvDiscounts.Rows.Count; i++)
        //foreach (GridViewRow myRow in gvDiscounts.Rows)
        {
            GridViewRow gvRow = gvDiscounts.Rows[i];

            prevdata.Rows[i]["ProductGroupID"] = int.Parse(((HiddenField)gvRow.FindControl("hidProductGroupID")).Value);

            prevdata.Rows[i]["ProductGroupName"] = (string)gvRow.Cells[0].Text;
            prevdata.Rows[i]["CreatedBy"] = (string)gvRow.Cells[1].Text;
            prevdata.Rows[i]["CreatedDate"] = (string)gvRow.Cells[2].Text;

        }
    }

    /// <summary>
    /// doesNewAcctDiscountExists - For new accounts - does discount exists in the grid 
    ///     to prevent duplicates from being added to AccountDiscounts.
    /// </summary>
    /// <param name="productGroupID"></param>
    /// <returns></returns>
    protected bool doesNewAcctDiscountExists(int productGroupID)
    {
        DataTable prevdata = (DataTable)Session["Discounts"];
        HiddenField hidProductGroupID = null;

        //gvDiscounts.Columns[4].Visible = true;

        for (int i = 0; i < gvDiscounts.Rows.Count; i++)
        //foreach (GridViewRow myRow in gvDiscounts.Rows)
        {
            GridViewRow gvRow = gvDiscounts.Rows[i];

            hidProductGroupID = (HiddenField)gvRow.FindControl("hidProductGroupID");

            int checkProductGroupID = int.Parse(((HiddenField)gvRow.FindControl("hidProductGroupID")).Value);

            if (checkProductGroupID == productGroupID)
            {
                //gvDiscounts.Columns[4].Visible = false;
                return true;
            }
        }

        //gvDiscounts.Columns[4].Visible = false;
        return false;
    }

    /// <summary>
    /// update account discount in the grid (not add) for (new accounts only).
    /// </summary>
    /// <param name="productGroupID"></param>
    /// <param name="productGroupName"></param>
    /// <param name="discount"></param>
    protected void updateNewAcctDiscount(int productGroupID,
                       string productGroupName, decimal discount)
    {
        DataTable prevdata = (DataTable)Session["Discounts"];

        for (int i = 0; i < gvDiscounts.Rows.Count; i++)
        //foreach (GridViewRow myRow in gvDiscounts.Rows)
        {
            GridViewRow gvRow = gvDiscounts.Rows[i];

            int checkProductGroupID = int.Parse(((HiddenField)gvRow.FindControl("hidProductGroupID")).Value);

            if (checkProductGroupID == productGroupID)
            {
                prevdata.Rows[i]["ProductGroupID"] = productGroupID;

                prevdata.Rows[i]["ProductGroupName"] = productGroupName;
                prevdata.Rows[i]["Discount"] = discount.ToString();
                prevdata.Rows[i]["CreatedBy"] = UserName;
                prevdata.Rows[i]["CreatedDate"] = String.Format("{0:MM/dd/yyyy}", DateTime.Now);
            }
        }

    }

    /// <summary>
    /// load disconts grid from either database (existing account or session (new account)).
    /// </summary>
    private void loadDiscountGrid()
    {
        //int productGroupID = 0;
        DataTable dt = new DataTable();

        Account account = (from a in idc.Accounts where a.AccountID == AccountID select a).FirstOrDefault();

        var discounts = (from a in idc.AccountDiscounts
                         join pg in idc.ProductGroups
                            on a.ProductGroupID equals pg.ProductGroupID
                         where a.AccountID == AccountID
                            && a.Active
                         orderby a.AccountID, a.ProductGroupID
                         select new
                         {
                             a.AccountID,
                             a.ProductGroupID,
                             pg.ProductGroupName,
                             a.Discount,
                             a.CreatedBy,
                             a.CreatedDate
                         }).ToList();

        if (AccountID > 0)
        {
            gvDiscounts.DataSource = discounts;
        }
        else
        {
            dt = (DataTable)Session["Discounts"];
            gvDiscounts.DataSource = dt;
        }

        gvDiscounts.DataBind();

        if (discounts.Count > 0) ddlRateCode.Enabled = false;

        //Reset Session if blank grid.  Turn on Rate code dropdown.
        if (gvDiscounts.Rows.Count == 0)
        {
            Session["Discounts"] = null;
            ddlRateCode.Enabled = true;
        }
        else  //discounts stored in session.  Do NOT allow rate code to be changed!
            ddlRateCode.Enabled = false;

        //update discount grid with discount rate entered.
        updateDiscount();

        //updateTargetPrices();
    }

    /// <summary>
    /// validation for modal account discount form.
    /// </summary>
    /// <returns></returns>
    private bool validateInputs_discountDialog()
    {
        string regDiscountPct = @"^[-+]?\d*\.?\d*$";
        string errorString = "";

        decimal discountRate = 0;

        // Create a regular expression for the discounts.
        Regex regDiscount = new Regex(regDiscountPct);

        if (!decimal.TryParse(txtDiscount.Text, out discountRate))
        {
            errorString = "Invalid discount rate.";
            this.VisibleErrors_discountsDialog(errorString);
            return false;
        }
        else if (discountRate > 100)
        {
            errorString = "Invalid discount rate.";
            this.VisibleErrors_discountsDialog(errorString);
            txtDiscount.Text = "0";
            return false;
        }

        if (!regDiscount.IsMatch(txtDiscount.Text.Trim()))
        {
            errorString = "Invalid discount rate.";
            this.VisibleErrors_discountsDialog(errorString);
            txtDiscount.Text = "0";
            return false;
        }

        return true;
    }

    protected void ddlReferral_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddlBrand.SelectedValue != "3" && ddlCustomerType.SelectedItem.Text.Trim() != "CON")
        {
            EnableBillingFields(true);
            ICCareBillingInfoValidation(true);
        }
        loadResellerBillingInfo(ddlReferral.SelectedValue);
        copyaddress.Checked = false;
    }

    protected void copyaddress_CheckedChanged(object sender, EventArgs e)
    {
        if (copyaddress.Checked)
        {
            txtCompanyNameS.Text = txtCompanyNameB.Text;
            txtFirstS.Text = txtFirstB.Text;
            txtLastS.Text = txtLastB.Text;
            txtAddress1S.Text = txtAddress1B.Text;
            txtAddress2S.Text = txtAddress2B.Text;
            txtAddress3S.Text = txtAddress3B.Text;
            txtCityS.Text = txtCityB.Text;
            txtPostalS.Text = txtPostalB.Text;
            txtTelephoneS.Text = txtTelephoneB.Text;
            txtFaxS.Text = txtFaxB.Text;
            txtEmailAddressS.Text = txtEmailAddressB.Text;
            ddlGenderS.SelectedValue = ddlGenderB.SelectedValue;
            ddlCountryS.SelectedValue = ddlCountryB.SelectedValue;
            LoadDDLState(ddlStateS, ddlCountryS.SelectedValue);
            ddlStateS.SelectedValue = ddlStateB.SelectedValue;

        }
        else
        {
            txtCompanyNameS.Text = "";
            txtFirstS.Text = "";
            txtLastS.Text = "";
            txtAddress1S.Text = "";
            txtAddress2S.Text = "";
            txtAddress3S.Text = "";
            txtCityS.Text = "";
            txtPostalS.Text = "";
            txtTelephoneS.Text = "";
            txtFaxS.Text = "";
            txtEmailAddressS.Text = "";
            ddlGenderS.SelectedValue = "";
            ddlCountryS.SelectedValue = "1";
            LoadDDLState(ddlStateS, ddlCountryS.SelectedValue);
            ddlStateS.SelectedIndex = 0;
        }
    }

    private bool isOrderExist()
    {
        return idc.Orders.Any(o => o.AccountID == AccountID && o.OrderStatusID != 10);
    }

    private bool validPODates()
    {
        DateTime beginDate; DateTime endDate;
        bool isValidBeginDate = DateTime.TryParse(txtPOBeginDate.Text.Trim(), out beginDate);
        bool isValidEndDate = DateTime.TryParse(txtPOEndDate.Text.Trim(), out endDate);

        if (isValidBeginDate && isValidEndDate)
        {
            if (beginDate > endDate)
            {
                CustomValidator val = new CustomValidator();
                val.ErrorMessage = string.Format("{0} - {1}", "PO Begin & End Date", "PO End Date must be greater then PO Begin Date");
                val.IsValid = false;
                val.ValidationGroup = "CSRegisterForm";
                this.Page.Validators.Add(val);

                return false;
            }
        }
        else if ((String.IsNullOrEmpty(txtPOBeginDate.Text.Trim()) && !String.IsNullOrEmpty(txtPOEndDate.Text.Trim()))
            || (!String.IsNullOrEmpty(txtPOBeginDate.Text.Trim()) && String.IsNullOrEmpty(txtPOEndDate.Text.Trim())))
        {

            CustomValidator val = new CustomValidator();
            val.ErrorMessage = string.Format("{0} - {1}", "PO Begin & End Date", "PO Dates are invalid");
            val.IsValid = false;
            val.ValidationGroup = "CSRegisterForm";
            this.Page.Validators.Add(val);

            return false;
        }

        return true;

    }

    private bool RestrictControls()
    {
        var user = User.Identity.Name;
        if (user != "")
            user = user.Split('\\')[1];

        var setting = DAMisc.GetPortalAppSetting("DoNotSendInvoiceDelivery");

        var arr = setting.Split(';');

        if (!arr.Contains(user))
        {
            this.IsDoNotSendRestricted = false;
            return false;
        }
        else
        {
            this.IsDoNotSendRestricted = true;
            return true;
        }

    }
}