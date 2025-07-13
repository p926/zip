/*
 * Maintain IC Care Dealers Information.
 * 
 * Change History:
 *  > 1.0: Original Release, 11/22/2011 
 *  Modified By: TDO, 09/25/2012
 *  Modified By: Anuradha Nandi, 06/13/2014
 *  Modified By: Anuradha Nandi, 08/14/2014
 *  Copyright (C) 2014 Mirion Technologies. All Rights Reserved.
 */

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using Instadose.Data;
using Mirion.DSD.GDS.API;
using Mirion.DSD.GDS.API.DataTypes;
using Mirion.DSD.GDS.API.Helpers;
using System.Data;

public partial class InstaDose_CustomerService_ICCareDealerMaintenance : System.Web.UI.Page
{
    // Create the DataContext Connection to the Server.
    InsDataContext idc = new InsDataContext();

    #region ON PAGE LOAD.
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Page.IsPostBack) return;

        this.txtSearchDealerID.Text = string.Empty;
        this.txtSearchDealerName.Text = string.Empty;
        this.PopulateDealerGroupNamesDDL();
    }
    #endregion

    #region ACTIVE/INACTIVE FOR ACTIVE COLUMN.
    /// <summary>
    /// Sets Text to Yes/No depending on Boolean.
    /// Used to display Active/Inactive status on Client-Side.
    /// </summary>
    /// <param name="boolean"></param>
    /// <returns></returns>
    public string ActiveInactive(object boolean)
    {
        try
        {
            return Convert.ToBoolean(boolean) ? "Yes" : "No";
        }
        catch
        {
            return "No";
        }
    }
    #endregion

    #region GET NEW DEALERID BASED ON LAST DEALERID.
    private int GetNewDealerID()
    {
        DealerRequests dealerRequest = new DealerRequests();

        int newestDealerID = 0;

        newestDealerID = dealerRequest.GetNextICCDealerId();

        return newestDealerID;
    }
    #endregion

    #region GET UNIX RATE CODE BASED ON BRANDSOURCEID (ICCARE).
    /// <summary>
    /// Get the UnixRateCode based upon the BrandSourceID.
    /// ICCare BrandSourceID = 3.
    /// The default is/should be (currently) RateCode = 500003.
    /// </summary>
    /// <param name="dealerid"></param>
    /// <returns></returns>
    private string GetUNIXRateCode(string dealerid)
    {
        AccountRequests accountRequest = new AccountRequests();

        GRateCode grc = new GRateCode();

        grc = accountRequest.GetRateCode("3", dealerid);

        float unixRateCode = 0;

        float.TryParse(grc.RateCode.ToString(), out unixRateCode);

        return unixRateCode.ToString();
    }
    #endregion

    #region RESET CLIENTID DROPDOWNLIST IF EDI CHECKBOX UNCHECKED.
    protected void chkbxEDIInvoiceRequired_CheckedChanged(object sender, EventArgs e)
    {
        this.ddlClientID.SelectedValue = "0";
    }
    #endregion

    #region DROPDOWNLIST FUNCTIONS.
    /// <summary>
    /// Populate ddlSearchDealerGroupNames for Search Toolbar.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PopulateDealerGroupNamesDDL()
    {
        var dealerGroups = (from dg in idc.DealerGroups
                            orderby dg.DealerGroupName ascending
                            select new
                            {
                                dg.DealerGroupName
                            });

        this.ddlSearchDealerGroups.DataSource = dealerGroups;
        this.ddlSearchDealerGroups.DataTextField = "DealerGroupName";
        this.ddlSearchDealerGroups.DataValueField = "DealerGroupName";
        this.ddlSearchDealerGroups.DataBind();
    }

    /// <summary>
    /// Select CountryName from DDL and update State DropDownList.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void ddlCountryID_SelectedIndexChanged(object sender, EventArgs e)
    {
        this.InitiateStateControl();

        // Checks to see if Country is either USA or Canada,
        // IF so, then only displays Address Lines 1 and 2 and City.
        // ELSE, displays Address Lines 1, 2, City, and 3 (this is Address4 in UNIX).
        if (this.ddlCountryID.SelectedItem.Text == "United States" || this.ddlCountryID.SelectedItem.Text == "Canada")
        {
            this.divDomesticAddress.Visible = true;
            this.divForeignAddress.Visible = false;
        }
        else
        {
            this.divDomesticAddress.Visible = false;
            this.divForeignAddress.Visible = true;
        }
    }

    /// <summary>
    /// DataBind States according to Country DropDownList value.
    /// </summary>
    private void InitiateStateControl()
    {
        try
        {
            this.ddlStateID_Domestic.DataBind();
            this.ddlStateID_Foreign.DataBind();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    /// <summary>
    /// IF a Client(ID) is selected, then automatically check the EDI Invoice Required CheckBox field.
    /// </summary>
    /// <param name="send"></param>
    /// <param name="e"></param>
    protected void ddlClientID_SelectedIndexChanged(object send, EventArgs e)
    {
        if (this.ddlClientID.SelectedValue != "0")
        {
            this.chkbxEDIInvoiceRequired.Checked = true;
        }
        else
        {
            this.chkbxEDIInvoiceRequired.Checked = false;
        }
    }

    /// <summary>
    /// Show/Hide Discount % control based on RateFieldType value selected.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void ddlRateFieldType_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (this.ddlRateFieldType.SelectedValue == "2") // Discount
        {
            this.divDiscountPercentage.Visible = true;
            this.txtDiscountField.Focus();
        }
        else
        {
            this.divDiscountPercentage.Visible = false;
            this.txtDiscountField.Text = "0";
        }
    }
    #endregion

    #region MODAL/DIALOG ERROR MESSAGE.
    private void InvisibleErrors_DealerDialog()
    {
        // Reset submission form error message.      
        this.dealerDialogErrorMsg.InnerText = "";
        this.dealerDialogError.Visible = false;
    }

    private void VisibleErrors_DealerDialog(string error)
    {
        this.dealerDialogErrorMsg.InnerText = error;
        this.dealerDialogError.Visible = true;
    }
    #endregion

    #region VALIDATE FORM CONTROLS.
    /// <summary>
    /// Checks/Validates Form Controls for required fields/values.
    /// </summary>
    /// <returns></returns>
    private bool PassInputsValidation_DealerDialog()
    {
        int myInt;
        string emailAddress = "";
        string phoneNumber = "";
        Regex regexEmail = new Regex(@"^[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-zA-Z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?\.)+[a-zA-Z0-9](?:[a-zA-Z0-9-]*[a-zA-Z0-9])?");
        Regex regexPhoneNumber = new Regex(@"^\+?(\d[\d-. ]+)?(\([\d-. ]+\))?[\d-. ]+\d$");

        // DealerID required.
        if (this.txtDealerID.Text.Trim().Length == 0)
        {
            this.VisibleErrors_DealerDialog("Dealer # is required.");
            SetFocus(this.txtDealerID);
            return false;
        }
        else
        {
            if (int.TryParse(this.txtDealerID.Text.Trim(), out myInt) == false)
            {
                this.VisibleErrors_DealerDialog("Dealer # must be a number.");
                SetFocus(this.txtDealerID);
                return false;
            }
        }

        // DealerName required.
        if (this.txtDealerName.Text.Trim().Length == 0)
        {
            this.VisibleErrors_DealerDialog("Dealer Name is required.");
            SetFocus(this.txtDealerName);
            return false;
        }

        // DealerNative required.
        if (this.txtDealerNative.Text.Trim().Length == 0)
        {
            this.VisibleErrors_DealerDialog("Dealer Native is required.");
            SetFocus(this.txtDealerNative);
            return false;
        }

        // ContactName - FirstName & LastName only (required).
        if (this.txtFirstName.Text.Trim().Length == 0)
        {
            this.VisibleErrors_DealerDialog("First Name is required.");
            SetFocus(this.txtFirstName);
            return false;
        }

        if (this.txtLastName.Text.Trim().Length == 0)
        {
            this.VisibleErrors_DealerDialog("Last Name is required.");
            SetFocus(this.txtLastName);
            return false;
        }

        // Telephone/Phone required.
        if (this.txtTelephone.Text.Trim().Length == 0)
        {
            this.VisibleErrors_DealerDialog("Phone # is required.");
            SetFocus(this.txtTelephone);
            return false;
        }

        // Validate Telephone # formatting.
        if (this.txtTelephone.Text.Trim().Length != 0)
        {
            phoneNumber = this.txtTelephone.Text.Trim();
            Match match = regexPhoneNumber.Match(phoneNumber);
            if (!match.Success)
            {
                this.VisibleErrors_DealerDialog("Incorrect Telephone # format (ex. (949)555-1212).");
                SetFocus(this.txtTelephone);
                return false;
            }
        }

        // Validate Fax # formatting.
        if (this.txtFax.Text.Trim().Length != 0)
        {
            phoneNumber = this.txtFax.Text.Trim();
            Match match = regexPhoneNumber.Match(phoneNumber);
            if (!match.Success)
            {
                this.VisibleErrors_DealerDialog("Incorrect Fax # format (ex. (949)555-1212).");
                SetFocus(this.txtFax);
                return false;
            }
        }

        // Email 1 required.
        if (this.txtEmail.Text.Trim().Length == 0)
        {
            this.VisibleErrors_DealerDialog("A (primary) E-Mail address is required.");
            SetFocus(this.txtEmail);
            return false;
        }

        // Validate E-Mail 1 formatting.
        if (this.txtEmail.Text.Trim().Length != 0)
        {
            emailAddress = this.txtEmail.Text.Trim();
            Match match = regexEmail.Match(emailAddress);
            if (!match.Success)
            {
                this.VisibleErrors_DealerDialog("Incorrect E-mail 1 format (ex. johnsmith@somecompany.com).");
                SetFocus(this.txtEmail);
                return false;
            }
        }

        // Validate E-Mail 2 formatting.
        if (this.txtEmail2.Text.Trim().Length != 0)
        {
            emailAddress = this.txtEmail2.Text.Trim();
            Match match = regexEmail.Match(emailAddress);
            if (!match.Success)
            {
                this.VisibleErrors_DealerDialog("Incorrect E-mail 2 format (ex. johnsmith@somecompany.com).");
                SetFocus(this.txtEmail2);
                return false;
            }
        }

        // Depending on Domestic (USA/Canada) or Foreign CountryID selected, validations are as follows;
        if (this.ddlCountryID.SelectedItem.Text == "United States" || this.ddlCountryID.SelectedItem.Text == "Canada")
        {
            // Address Line 1 required.
            if (this.txtAddressLine1_Domestic.Text.Trim().Length == 0)
            {
                this.VisibleErrors_DealerDialog("Address Line 1 is required.");
                SetFocus(this.txtAddressLine1_Domestic);
                return false;
            }

            // City required.
            if (this.txtCity_Domestic.Text.Trim().Length == 0)
            {
                this.VisibleErrors_DealerDialog("City is required.");
                SetFocus(this.txtCity_Domestic);
                return false;
            }

            // Postal Code required.
            if (this.txtPostalCode_Domestic.Text.Trim().Length == 0)
            {
                this.VisibleErrors_DealerDialog("Postal Code is required.");
                SetFocus(this.txtPostalCode_Domestic);
                return false;
            }
        }
        else
        {
            // Address Line 1 required.
            if (this.txtAddressLine1_Foreign.Text.Trim().Length == 0)
            {
                this.VisibleErrors_DealerDialog("Address Line 1 is required.");
                SetFocus(this.txtAddressLine1_Foreign);
                return false;
            }

            // Address Line 2 required.
            if (this.txtAddressLine2_Foreign.Text.Trim().Length == 0)
            {
                this.VisibleErrors_DealerDialog("Address Line 2 is required.");
                SetFocus(this.txtAddressLine2_Foreign);
                return false;
            }

            // Address Line 3 (City) required.
            if (this.txtCity_Foreign.Text.Trim().Length == 0)
            {
                this.VisibleErrors_DealerDialog("Address Line 3 is required.");
                SetFocus(this.txtCity_Foreign);
                return false;
            }

            // Address Line 4 required.
            if (this.txtAddressLine4_Foreign.Text.Trim().Length == 0)
            {
                this.VisibleErrors_DealerDialog("Address Line 4 is required.");
                SetFocus(this.txtAddressLine4_Foreign);
                return false;
            }

            // Postal Code required.
            if (this.txtPostalCode_Foreign.Text.Trim().Length == 0)
            {
                this.VisibleErrors_DealerDialog("Postal Code is required.");
                SetFocus(this.txtPostalCode_Foreign);
                return false;
            }
        }

        // If EDI Invoice Required is checked, then validate if a Client ID has been selected.
        if (this.chkbxEDIInvoiceRequired.Checked == true && this.ddlClientID.SelectedValue == "0")
        {
            this.VisibleErrors_DealerDialog("A Client ID must be selected.");
            SetFocus(this.ddlClientID);
            return false;
        }

        // 08/14/2014 - Taken out per CS request, no longer required.
        // Default set as 500003 (reference hdnfldUNIXRateCode value).
        // UnixRateCode (required).
        //if (this.txtUNIXRateCode.Text.Trim().Length == 0)
        //{
        //    this.VisibleErrors_DealerDialog("UNIX Rate Code is required.");
        //    SetFocus(this.txtUNIXRateCode);
        //    return false;
        //}
        // ----------------------------------------------------------------//

        int poDeliveryMethod = 0;
        int.TryParse(this.ddlPODeliveryMethod.SelectedValue, out poDeliveryMethod);

        switch (poDeliveryMethod)
        {
            case 0: // Fax required (default in UNIX).
                if (this.txtFax.Text.Trim() == "")
                {
                    this.VisibleErrors_DealerDialog("Fax # required.");
                    SetFocus(this.txtFax);
                    return false;
                }
                break;
            case 1: // Email 1 required.
                if (this.txtEmail.Text.Trim() == "")
                {
                    this.VisibleErrors_DealerDialog("Email 1 required.");
                    SetFocus(this.txtEmail);
                    return false;
                }
                break;
            case 2: // Email 2 required.
                if (this.txtEmail2.Text.Trim() == "")
                {
                    this.VisibleErrors_DealerDialog("Email 2 required.");
                    SetFocus(this.txtEmail2);
                    return false;
                }
                break;
            case 3: // Email 1 + Various (only Email 1 required).
                if (this.txtEmail.Text.Trim() == "")
                {
                    this.VisibleErrors_DealerDialog("Email 1 required.");
                    SetFocus(this.txtEmail);
                    return false;
                }
                break;
            default:
                break;
        }

        // IF Rate Field Type is "Discount" (2), THEN required Discount % field.
        if (this.ddlRateFieldType.SelectedValue == "2")
        {
            if (this.txtDiscountField.Text == "")
            {
                this.VisibleErrors_DealerDialog("Discount % required.");
                SetFocus(this.txtDiscountField);
                return false;
            }

            if (this.txtDiscountField.Text != "")
            {
                if (int.TryParse(this.txtDiscountField.Text.Trim(), out myInt) == false)
                {
                    this.VisibleErrors_DealerDialog("Invalid Discount %.");
                    SetFocus(this.txtDiscountField);
                    return false;
                }
            }
        }

        return true;
    }
    #endregion

    #region SET FORM CONTROLS TO DEFAULTS.
    /// <summary>
    /// Sets all Form Controls to their Default values.
    /// </summary>
    private void SetDefaultValues_DealerDialog()
    {
        // Reset form Error Message.
        this.InvisibleErrors_DealerDialog();

        this.hdnfldDealerID.Value = "0";
        this.hdnfldAccordionIndex.Value = "0";

        // Basic Details.
        this.txtDealerID.Text = string.Empty;
        this.txtDealerName.Text = string.Empty;
        this.txtDealerNative.Text = string.Empty;
        // 08/14/2014 - Type, Brand, and Group are now static Labels.
        // Values are set in their respective HiddenFields.
        //this.txtTypeCode.Text = string.Empty;
        //this.ddlBrandSourceID.SelectedValue = "3";  // ICCare
        //this.ddlDealerGroupID.SelectedValue = "1";  // General
        this.hdnfldTypeDefault.Value = "";
        this.lblType.Text = "None Available";
        this.hdnfldBrandSourceIDDefault.Value = "3";    // ICCare
        this.lblBrand.Text = "IC Care";
        this.hdnfldDealerGroupIDDefault.Value = "1";    // General
        this.lblGroup.Text = "General";
        // --------------------------------------------------------//
        this.lblOpenAR.Text = "None Available";         
        this.chkBoxActive.Checked = true;

        // Contact Information.
        this.txtFirstName.Text = string.Empty;
        this.txtLastName.Text = string.Empty;
        this.txtAttentionName.Text = string.Empty;
        this.txtTelephone.Text = string.Empty;
        this.txtFax.Text = string.Empty;
        this.txtEmail.Text = string.Empty;
        this.txtEmail2.Text = string.Empty;
        this.ddlCountryID.SelectedValue = "1"; // United States.
        this.divDomesticAddress.Visible = true;
        InitiateStateControl();
        // Domestic Address.
        this.txtAddressLine1_Domestic.Text = string.Empty;
        this.txtAddressLine2_Domestic.Text = string.Empty;
        this.txtCity_Domestic.Text = string.Empty;
        this.txtPostalCode_Domestic.Text = string.Empty;
        // Foreign Address.
        this.txtAddressLine1_Foreign.Text = string.Empty;
        this.txtAddressLine2_Foreign.Text = string.Empty;
        this.txtCity_Foreign.Text = string.Empty;
        this.txtAddressLine4_Foreign.Text = string.Empty;
        this.txtPostalCode_Foreign.Text = string.Empty;

        // Additional Information (UNIX Fields).
        this.ddlClientID.SelectedValue = "0";
        this.chkbxEDIInvoiceRequired.Checked = false;
        this.txtDealerNotes.Text = string.Empty;
        this.chkbxBillForShipping.Checked = true;
        this.ddlRenewalType.SelectedValue = "S";
        this.ddlPODeliveryMethod.SelectedValue = "0";
        // 08/14/2014 - UNIX Rate Code is now a static Label.
        // Value is set in hdnfldUNIXRateCodeDefault (500003).
        //this.txtUNIXRateCode.Text = "500003";   // Default UNIX Rate Code for ICCare.
        //this.txtUNIXRateCode.ReadOnly = true;
        this.hdnfldUNIXRateCodeDefault.Value = "500003";  // Default UNIX Rate Code for ICCare.
        this.lblUNIXRateCode.Text = "500003";
        // ----------------------------------------------------------------------------------//
        this.ddlRateFieldType.SelectedValue = "3";  // Custom.
        this.divDiscountPercentage.Visible = false;
        this.txtDiscountField.Text = "0";
        this.ddlReportFormatType.SelectedValue = "P";
    }
    #endregion

    #region GET FORM CONTROL VALUES (EDIT MODE).
    /// <summary>
    /// Get the values of each form control based on DealerID (Edit Mode).
    /// </summary>
    /// <param name="dealerid"></param>
    private void SetValuesToControls_DealerDialog(int dealerid)
    {
        SetFocus(this.txtDealerName);

        if (dealerid > 0)    // Edit Mode.
        {
            Dealer myDealer = (from d in idc.Dealers
                               where d.DealerID == dealerid
                               select d).FirstOrDefault();

            this.txtDealerID.Text = myDealer.DealerID.ToString();
            this.txtDealerName.Text = myDealer.DealerName;
            this.txtDealerNative.Text = myDealer.DealerNative;
            // 08/14/2014 - Type, Brand, and Group are now Labels with their values stored in their respective HiddenFields.
            // Their Labels will have the DB values displayed accordingly.
            //this.txtTypeCode.Text = myDealer.TypeCode;
            //this.ddlBrandSourceID.SelectedValue = myDealer.BrandSourceID.ToString();
            //this.ddlDealerGroupID.SelectedValue = myDealer.DealerGroupID.ToString();
            this.hdnfldTypeDefault.Value = ((myDealer.TypeCode == null) ? "" : myDealer.TypeCode);
            this.lblType.Text = ((myDealer.TypeCode == null) ? "None Available" : myDealer.TypeCode);
            this.hdnfldBrandSourceIDDefault.Value = myDealer.BrandSourceID.ToString();
            this.lblBrand.Text = myDealer.BrandSource.BrandName;
            // -----------------------------------------------------------------------------------------------------------//
            this.hdnfldDealerGroupIDDefault.Value = myDealer.DealerGroupID.ToString();
            this.lblGroup.Text = myDealer.DealerGroup.DealerGroupName;
            this.chkBoxActive.Checked = myDealer.Active;
            this.txtFirstName.Text = myDealer.FirstName;
            this.txtLastName.Text = myDealer.LastName;
            this.txtAttentionName.Text = myDealer.AttentionName;
            this.txtTelephone.Text = myDealer.Telephone;
            this.txtFax.Text = myDealer.Fax;
            this.txtEmail.Text = myDealer.Email;
            this.txtEmail2.Text = myDealer.Email2;
            this.ddlCountryID.SelectedValue = myDealer.CountryID.ToString();
            // Dependant on CountryID, populate controls as follows;
            if (this.ddlCountryID.SelectedItem.Text == "United States" || this.ddlCountryID.SelectedItem.Text == "Canada")
            {
                this.divDomesticAddress.Visible = true;
                this.divForeignAddress.Visible = false;

                // Domestic Address.
                this.txtAddressLine1_Domestic.Text = myDealer.Address1;
                this.txtAddressLine2_Domestic.Text = myDealer.Address2;
                this.txtCity_Domestic.Text = myDealer.City;
                InitiateStateControl();
                this.ddlStateID_Domestic.SelectedValue = myDealer.StateID.ToString();
                this.txtPostalCode_Domestic.Text = myDealer.PostalCode;
            }
            else
            {
                this.divDomesticAddress.Visible = false;
                this.divForeignAddress.Visible = true;

                // Foreign Address.
                this.txtAddressLine1_Foreign.Text = myDealer.Address1;
                this.txtAddressLine2_Foreign.Text = myDealer.Address2;
                this.txtCity_Foreign.Text = myDealer.City;
                this.txtAddressLine4_Foreign.Text = myDealer.Address3;
                InitiateStateControl();
                this.ddlStateID_Foreign.SelectedValue = myDealer.StateID.ToString();
                this.txtPostalCode_Foreign.Text = myDealer.PostalCode;
            }
            // UNIX-related fields.
            this.ddlClientID.SelectedValue = ((myDealer.ClientID == null || myDealer.ClientID == 0) ? "0" : myDealer.ClientID.ToString());
            if (this.ddlClientID.SelectedValue == "0")
            {
                this.chkbxEDIInvoiceRequired.Checked = false;
            }
            else
            {
                this.chkbxEDIInvoiceRequired.Checked = true;
            }
            this.txtDealerNotes.Text = (myDealer.DealerNotes == null ? "" : myDealer.DealerNotes);
            this.chkbxBillForShipping.Checked = Convert.ToBoolean(myDealer.BillForShipping);
            this.ddlRenewalType.SelectedValue = myDealer.RenewalType.ToString();
            this.ddlPODeliveryMethod.SelectedValue = (myDealer.PODeliveryMethod == null ? "0" : myDealer.PODeliveryMethod.ToString());
            // UNIX RateCode is taken directly from UNIX data (not from SQL).
            this.ddlRateFieldType.SelectedValue = myDealer.RateFieldType.ToString();
            if (myDealer.RateFieldType.ToString() == "2")
            {
                this.divDiscountPercentage.Visible = true;
            }
            else
            {
                this.divDiscountPercentage.Visible = false;
            }
            this.txtDiscountField.Text = (myDealer.DiscountField == null ? "0" : myDealer.DiscountField.ToString());
            this.ddlReportFormatType.SelectedValue = myDealer.ReportFormatType.ToString();
        }
    }

    /// <summary>
    /// When DealerID link is clicked in the GridView.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void lnkbtnEditDealer_Click(object sender, EventArgs e)
    {
        // Reset form Error Message.
        this.InvisibleErrors_DealerDialog();

        // Reset all Modal/Dialog Controls before repopulating them.
        this.SetDefaultValues_DealerDialog();

        LinkButton lnkbtn = (LinkButton)sender;
        int dealerID = 0;

        if (lnkbtn.CommandName == "EditDealer")
        {
            this.hdnfldDealerID.Value = lnkbtn.CommandArgument;
            int.TryParse(hdnfldDealerID.Value, out dealerID);

            this.SetValuesToControls_DealerDialog(dealerID);

            DealerRequests dr = new DealerRequests();
            var dealerInformation = dr.GetICCareDealers(null, null, dealerID.ToString());

            if (dealerInformation == null)
            {
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('dealerNotFoundDialog')", true);
                return;
            }
            else
            {
                int unixRateCode = 0;
                decimal openAR = 0;

                // Get UnixRateCode and OpenAR values from UNIX.
                int.TryParse(dealerInformation.Rows[0]["RateCode"].ToString(), out unixRateCode);
                decimal.TryParse(dealerInformation.Rows[0]["OpenAR"].ToString(), out openAR);

                // Get Currency based on Country.
                string currencyCode = (from c in idc.Countries
                                       where c.CountryID == Convert.ToInt32(this.ddlCountryID.SelectedValue)
                                       select c.CurrencyCode).FirstOrDefault();

                // 08/14/2014 - UNIX Rate Code must be set in both HiddenField and Label.
                // Display UNIXRateCode value.
                if (unixRateCode.ToString() == "" || unixRateCode.ToString() == null)
                {
                    //this.txtUNIXRateCode.Text = GetUNIXRateCode(dealerID.ToString());
                    this.hdnfldUNIXRateCodeDefault.Value = GetUNIXRateCode(dealerID.ToString());
                    this.lblUNIXRateCode.Text = GetUNIXRateCode(dealerID.ToString());
                }
                else
                {
                    //this.txtUNIXRateCode.Text = unixRateCode.ToString();
                    this.hdnfldUNIXRateCodeDefault.Value = unixRateCode.ToString();
                    this.lblUNIXRateCode.Text = unixRateCode.ToString();
                }
                //---------------------------------------------------------------------------//

                // Display OpenAR value along with Dealer's Country Currency.
                if (openAR.ToString() == "" || openAR.ToString() == null)
                {
                    this.lblOpenAR.Text = "None Available";
                }
                else
                {
                    this.lblOpenAR.Text = (openAR.ToString() + " " + currencyCode);
                }

                // Set the txtDealerID control to ReadOnly, this cannot/should not be edited.
                this.txtDealerID.ReadOnly = true;
                //this.txtUNIXRateCode.ReadOnly = true;
                this.txtDealerName.Focus();

                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('dealerDialog')", true);
            }
        }
        else
        {
            this.SetDefaultValues_DealerDialog();
        }
    }
    #endregion

    #region ADD/UPDATE ICCARE DEALER INFORMATION FUNCTIONS.
    protected void lnkbtnNewDealer_Click(object sender, EventArgs e)
    {
        // Reset form Error Message.
        this.InvisibleErrors_DealerDialog();

        // Reset all Modal/Dialog Controls.
        this.SetDefaultValues_DealerDialog();

        // Force Foreign Address <div> Fields visible=false.
        this.divForeignAddress.Visible = false;

        // Set Dealer #/ID control with next DealerID (from DB).
        this.txtDealerID.Text = this.GetNewDealerID().ToString();

        // txtDealerID should be ReadOnly so that it cannot/should not be edited.
        this.txtDealerID.ReadOnly = true;
        this.txtDealerName.Focus();

        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('dealerDialog')", true);
    }

    /// <summary>
    /// Modal/Dialog Add/Update button function.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnAddDealer_Click(object sender, EventArgs e)
    {
        this.InvisibleErrors_DealerDialog();

        try
        {
            if (this.PassInputsValidation_DealerDialog())
            {
                GICCareDealer getICCareDealer = new GICCareDealer();
                ICCareRequests iccRequest = new ICCareRequests();

                // Get the initials of the User who is making the changes.
                string userName = Page.User.Identity.Name.Split('\\')[1];
                string initials = userName.Substring(0, 2).ToUpper();

                // Get FirstName and LastName and combine to create ContactName.
                string firstName = "";
                string lastName = "";

                if (this.txtFirstName.Text.Trim().Length > 14) firstName = (this.txtFirstName.Text.Trim().Replace("'", "''")).Substring(0, 14);
                else firstName = this.txtFirstName.Text.Trim().Replace("'", "''");

                if (this.txtLastName.Text.Trim().Length > 14) lastName = (this.txtLastName.Text.Trim().Replace("'", "''").Substring(0, 14));
                else lastName = this.txtLastName.Text.Trim().Replace("'", "''");

                string contactName = "";
                contactName = firstName + " " + lastName;

                // formMode = true (Add new Dealer) | formMode = false (Update existing Dealer)
                bool formMode = (hdnfldDealerID.Value == "0") ? true : false;
                int myDealerID = int.Parse(this.txtDealerID.Text);
                string myDealerName = this.txtDealerName.Text.Trim().Replace("'", "''");

                // Insert/Update UNIX first.
                getICCareDealer.Active = (this.chkBoxActive.Checked == true) ? Active.Active : Active.Canceled;
                getICCareDealer.ICCareDealerID = txtDealerID.Text;
                // 08/14/2014 - Set DealerGroupID to value of hdnfldDealerGroupIDDefault.
                //getICCareDealer.DealerGroupID = Convert.ToInt32(this.ddlDealerGroupID.SelectedValue);
                getICCareDealer.DealerGroupID = Convert.ToInt32(this.hdnfldDealerGroupIDDefault.Value.ToString());
                //-----------------------------------------------------------------------------------//
                getICCareDealer.ICCareDealerName = this.txtDealerName.Text.Replace("'", "''").Trim();
                getICCareDealer.ICCareDealerNotes = (this.txtDealerNotes.Text.Trim() == "") ? null : this.txtDealerNotes.Text.Replace("'", "''").Trim();
                getICCareDealer.Attention = this.txtAttentionName.Text.Replace("'", "''").Trim();
                getICCareDealer.ContactName = contactName;
                getICCareDealer.FaxNumber = (this.txtFax.Text.Trim() == "") ? null : Regex.Replace(this.txtFax.Text.Trim().Replace(" ", ""), "[()-]", "");
                // Get CountryID/Name
                int countryID = 0;
                int.TryParse(this.ddlCountryID.SelectedValue, out countryID);
                var countryInformation = (from c in idc.Countries
                                          where c.CountryID == countryID
                                          select new
                                          {
                                              CountryCode = c.PayPalCode
                                          }).FirstOrDefault();
                getICCareDealer.CountryCode = countryInformation.CountryCode;
                getICCareDealer.CountryDesc = HConvert.ToCountryCodeDescription(countryInformation.CountryCode).ToUpper();
                // Domestic vs. Foreign Address Insert/Update.
                if (this.ddlCountryID.SelectedItem.Text == "United States" || this.ddlCountryID.SelectedItem.Text == "Canada")
                {
                    getICCareDealer.Address1 = (this.txtAddressLine1_Domestic.Text.Trim() == "") ? null : this.txtAddressLine1_Domestic.Text.Replace("'", "''").Trim();
                    getICCareDealer.Address2 = (this.txtAddressLine2_Domestic.Text.Trim() == "") ? null : this.txtAddressLine2_Domestic.Text.Replace("'", "''").Trim();
                    getICCareDealer.Address4 = null;
                    getICCareDealer.City = (this.txtCity_Domestic.Text.Trim() == "") ? null : this.txtCity_Domestic.Text.Replace("'", "''").Trim();
                    getICCareDealer.State = this.ddlStateID_Domestic.SelectedItem.Text;
                    getICCareDealer.ZipCode = (this.txtPostalCode_Domestic.Text.Trim() == "") ? null : this.txtPostalCode_Domestic.Text.Trim().Replace("-", "");
                }
                else
                {
                    getICCareDealer.Address1 = (this.txtAddressLine1_Foreign.Text.Trim() == "") ? null : this.txtAddressLine1_Foreign.Text.Replace("'", "''").Trim();
                    getICCareDealer.Address2 = (this.txtAddressLine2_Foreign.Text.Trim() == "") ? null : this.txtAddressLine2_Foreign.Text.Replace("'", "''").Trim();
                    getICCareDealer.City = (this.txtCity_Foreign.Text.Trim() == "") ? null : this.txtCity_Foreign.Text.Replace("'", "''").Trim();
                    getICCareDealer.Address4 = (this.txtAddressLine4_Foreign.Text.Trim() == "") ? null : this.txtAddressLine4_Foreign.Text.Replace("'", "''").Trim();
                    getICCareDealer.State = this.ddlStateID_Foreign.SelectedItem.Text;
                    getICCareDealer.ZipCode = (this.txtPostalCode_Foreign.Text.Trim() == "") ? null : this.txtPostalCode_Foreign.Text.Trim().Replace("-", "");
                }
                getICCareDealer.Email = (this.txtEmail.Text.Trim() == "") ? null : this.txtEmail.Text.Trim();
                getICCareDealer.Email2 = (this.txtEmail2.Text.Trim() == "") ? null : this.txtEmail2.Text.Trim();
                getICCareDealer.ContactPhone = (this.txtTelephone.Text.Trim() == "") ? null : Regex.Replace(this.txtTelephone.Text.Trim().Replace(" ", ""), "[()-]", "");
                getICCareDealer.AddrFmtCode = (countryInformation.CountryCode == "US") ? AddrFmtCode.Domestic : (countryInformation.CountryCode == "CA") ? AddrFmtCode.Canada : AddrFmtCode.Other;
                getICCareDealer.BillNoBillShipping = (this.chkbxBillForShipping.Checked == true) ? YesNo.Yes : YesNo.No;
                getICCareDealer.RenewalType = (this.ddlRenewalType.SelectedValue == "S") ? Renewal.SendPO : (this.ddlRenewalType.SelectedValue == "A") ? Renewal.AutoRenew : Renewal.NeverRenewThroughDealer;
                getICCareDealer.SendPORequest = (this.ddlPODeliveryMethod.SelectedValue == "0") ? POReqSend.Fax : (this.ddlPODeliveryMethod.SelectedValue == "1") ? POReqSend.Email1 : (this.ddlPODeliveryMethod.SelectedValue == "2") ? POReqSend.Email1AndEmail2 : POReqSend.Email1AndVarious;
                // 08/14/2014 - Set UNIXRateCode to value of hdnfldUNIXRateCodeDefault.
                //getICCareDealer.RateCode = this.txtUNIXRateCode.Text;
                getICCareDealer.RateCode = this.hdnfldUNIXRateCodeDefault.Value.ToString();
                //-----------------------------------------------------------------------//
                getICCareDealer.RateFieldFlag = (this.ddlRateFieldType.SelectedValue == "1") ? RateField.UnitPrice : (this.ddlRateFieldType.SelectedValue == "2") ? RateField.Discount : RateField.Custom;
                getICCareDealer.DiscountField = (this.txtDiscountField.Text.Trim() == "") ? "0" : this.txtDiscountField.Text;
                getICCareDealer.ReportFmtFlag = (this.ddlReportFormatType.SelectedValue == "P") ? ReportFmt.Print : ReportFmt.Spreadsheet;

                int responseCode = (formMode == true) ? iccRequest.AddICCDealer(getICCareDealer, initials, userName) : iccRequest.UpdateICCDealer(getICCareDealer, initials, userName);

                if (responseCode == 0)  // Success
                {
                    string addressLine1 = "";
                    string addressLine2 = "";
                    string city = "";
                    string addressLine4 = "";
                    int stateID = 0;
                    string postalCode = "";


                    // Set Domestic or Foreign Address information.
                    if (this.ddlCountryID.SelectedItem.Text == "United States" || this.ddlCountryID.SelectedItem.Text == "Canada")
                    {
                        // Domestic Address.
                        addressLine1 = (this.txtAddressLine1_Domestic.Text.Trim().Length == 0 ? null : this.txtAddressLine1_Domestic.Text.Trim().Replace("'", "''"));
                        addressLine2 = (this.txtAddressLine2_Domestic.Text.Trim().Length == 0 ? null : this.txtAddressLine2_Domestic.Text.Trim().Replace("'", "''"));
                        addressLine4 = null;
                        city = (this.txtCity_Domestic.Text.Trim().Length == 0 ? null : this.txtCity_Domestic.Text.Trim().Replace("'", "''"));
                        stateID = int.Parse(this.ddlStateID_Domestic.SelectedValue);
                        postalCode = (this.txtPostalCode_Domestic.Text.Trim().Length == 0 ? null : this.txtPostalCode_Domestic.Text.Trim().Replace("-", ""));

                    }
                    else
                    {
                        // Foreign Address.
                        addressLine1 = (this.txtAddressLine1_Foreign.Text.Trim().Length == 0 ? null : this.txtAddressLine1_Foreign.Text.Trim().Replace("'", "''"));
                        addressLine2 = (this.txtAddressLine2_Foreign.Text.Trim().Length == 0 ? null : this.txtAddressLine2_Foreign.Text.Trim().Replace("'", "''"));
                        city = (this.txtCity_Foreign.Text.Trim().Length == 0 ? null : this.txtCity_Foreign.Text.Trim().Replace("'", "''"));
                        addressLine4 = (this.txtAddressLine4_Foreign.Text.Trim().Length == 0 ? null : this.txtAddressLine4_Foreign.Text.Trim().Replace("'", "''"));
                        stateID = int.Parse(this.ddlStateID_Foreign.SelectedValue);
                        postalCode = (this.txtPostalCode_Foreign.Text.Trim().Length == 0 ? null : this.txtPostalCode_Foreign.Text.Trim().Replace("-", ""));
                    }

                    if (formMode == true) // ADD a new Dealer.
                    {
                        Dealer dealerInformation = new Dealer
                        {
                            DealerID = myDealerID,
                            DealerName = myDealerName,
                            DealerNative = this.txtDealerNative.Text.Trim().Replace("'", "''"),
                            Active = this.chkBoxActive.Checked,
                            // 08/14/2014 - Set TypeCode to value of hdnfldTypeDefault.
                            //TypeCode = (this.txtTypeCode.Text.Trim().Length == 0 ? null : this.txtTypeCode.Text.Trim()),
                            TypeCode = (this.hdnfldTypeDefault.Value.ToString() == "" ? null : this.hdnfldTypeDefault.Value.ToString()),
                            // 08/14/2014 - Set BrandSourceID to value of hdnfldBrandSourceIDDefault.
                            //BrandSourceID = int.Parse(this.ddlBrandSourceID.SelectedValue),
                            BrandSourceID = Convert.ToInt32(this.hdnfldBrandSourceIDDefault.Value.ToString()),
                            // 08/14/2014 - Set DealerGroupID to value of hdnfldDealerGroupIDDefault.
                            //DealerGroupID = int.Parse(this.ddlDealerGroupID.SelectedValue),
                            DealerGroupID = Convert.ToInt32(this.hdnfldDealerGroupIDDefault.Value.ToString()),
                            //-------------------------------------------------------------------------------------------------------//
                            Prefix = null,
                            FirstName = this.txtFirstName.Text.Trim().Replace("'", "''"),
                            MiddleName = null,
                            LastName = this.txtLastName.Text.Trim().Replace("'", "''"),
                            Email = (this.txtEmail.Text.Trim().Length == 0) ? null : this.txtEmail.Text.Trim(),
                            Telephone = (this.txtTelephone.Text.Trim().Length == 0 ? null : this.txtTelephone.Text.Trim()),
                            Fax = (this.txtFax.Text.Trim().Length == 0 ? null : this.txtFax.Text.Trim()),
                            Address1 = addressLine1,
                            Address2 = addressLine2,
                            City = city,
                            Address3 = addressLine4,
                            PostalCode = postalCode,
                            CountryID = int.Parse(this.ddlCountryID.SelectedValue),
                            StateID = stateID,
                            // UNIX fields.
                            ClientID = (this.ddlClientID.SelectedValue == "0") ? (int?)null : Convert.ToInt32(this.ddlClientID.SelectedValue),
                            EDIInvoiceRequired = this.chkbxEDIInvoiceRequired.Checked,
                            DealerNotes = (this.txtDealerNotes.Text.Length == 0) ? null : this.txtDealerNotes.Text.Replace("'", "''").Trim(),
                            Email2 = (this.txtEmail2.Text.Trim().Length == 0) ? null : this.txtEmail2.Text.Trim(),
                            BillForShipping = this.chkbxBillForShipping.Checked,
                            RenewalType = this.ddlRenewalType.SelectedValue,
                            PODeliveryMethod = Convert.ToInt32(this.ddlPODeliveryMethod.SelectedValue),
                            // 08/14/2014 - Set UNIXRateCode to value of hdnfldUNIXRateCodeDefault.
                            //UnixRateCode = Convert.ToInt32(this.txtUNIXRateCode.Text.Trim()),
                            UnixRateCode = Convert.ToInt32(this.hdnfldUNIXRateCodeDefault.Value.ToString()),
                            //----------------------------------------------------------------------------//
                            RateFieldType = Convert.ToInt32(this.ddlRateFieldType.SelectedValue),
                            DiscountField = (this.txtDiscountField.Text.Length == 0) ? 0 : Convert.ToInt32(this.txtDiscountField.Text.Trim()),
                            ReportFormatType = this.ddlReportFormatType.SelectedValue,
                            ContactName = contactName,
                            AttentionName = (this.txtAttentionName.Text.Length == 0) ? null : this.txtAttentionName.Text.Replace("'", "''").Trim()
                        };

                        try
                        {
                            idc.Dealers.InsertOnSubmit(dealerInformation);
                            idc.SubmitChanges();

                            // Refresh the GridView.
                            this.gvDealersView.DataBind();

                            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('dealerDialog')", true);
                        }
                        catch (Exception ex)
                        {
                            // Display the system generated error message.
                            throw new Exception(ex.Message);
                        }
                    }
                    else // UPDATE an existing Dealer.                  
                    {
                        var updateDealerInformation = (from d in idc.Dealers
                                                       where d.DealerID == myDealerID
                                                       select d).FirstOrDefault();

                        if (updateDealerInformation == null) return;

                        // Basic Details.
                        updateDealerInformation.DealerName = this.txtDealerName.Text.Trim().Replace("'", "''");
                        updateDealerInformation.DealerNative = this.txtDealerNative.Text.Trim().Replace("'", "''");
                        // 08/14/2014 - Set TypeCode to value of hdnfldTypeDefault.
                        //updateDealerInformation.TypeCode = (this.txtTypeCode.Text.Trim() == "") ? null : this.txtTypeCode.Text.Trim().ToUpper();
                        updateDealerInformation.TypeCode = (this.hdnfldTypeDefault.Value.ToString() == "" ? null : this.hdnfldTypeDefault.Value.ToString());
                        // 08/14/2014 - Set BrandSourceID to value of hdnfldBrandSourceIDDefault.
                        //updateDealerInformation.BrandSourceID = Convert.ToInt32(this.ddlBrandSourceID.SelectedValue);
                        updateDealerInformation.BrandSourceID = Convert.ToInt32(this.hdnfldBrandSourceIDDefault.Value.ToString());
                        // 08/14/2014 - Set DealerGroupID to value of hdnfldDealerGroupIDDefault.
                        //updateDealerInformation.DealerGroupID = Convert.ToInt32(this.ddlDealerGroupID.SelectedValue);
                        updateDealerInformation.DealerGroupID = Convert.ToInt32(this.hdnfldDealerGroupIDDefault.Value.ToString());
                        //-------------------------------------------------------------------------------------------------------------------------------//
                        updateDealerInformation.Active = Convert.ToBoolean(this.chkBoxActive.Checked);
                        // Contact Information.
                        updateDealerInformation.FirstName = this.txtFirstName.Text.Trim().Replace("'", "''");
                        updateDealerInformation.LastName = this.txtLastName.Text.Trim().Replace("'", "''");
                        updateDealerInformation.ContactName = contactName;
                        updateDealerInformation.AttentionName = (this.txtAttentionName.Text.Trim() == "") ? null : this.txtAttentionName.Text.Trim().Replace("'", "''");
                        updateDealerInformation.Telephone = (this.txtTelephone.Text.Trim() == "") ? null : this.txtTelephone.Text.Trim();
                        updateDealerInformation.Fax = (this.txtFax.Text.Trim() == "") ? null : this.txtFax.Text.Trim();
                        updateDealerInformation.Email = (this.txtEmail.Text.Trim() == "") ? null : this.txtEmail.Text.Trim();
                        updateDealerInformation.Email2 = (this.txtEmail2.Text.Trim() == "") ? null : this.txtEmail2.Text.Trim();
                        updateDealerInformation.CountryID = Convert.ToInt32(this.ddlCountryID.SelectedValue);
                        updateDealerInformation.Address1 = addressLine1;
                        updateDealerInformation.Address2 = addressLine2;
                        updateDealerInformation.Address3 = addressLine4;
                        updateDealerInformation.City = city;
                        updateDealerInformation.StateID = stateID;
                        updateDealerInformation.PostalCode = postalCode;
                        // Additional Information.
                        updateDealerInformation.ClientID = (this.ddlClientID.SelectedValue == "0") ? (int?)null : Convert.ToInt32(this.ddlClientID.SelectedValue);
                        updateDealerInformation.EDIInvoiceRequired = Convert.ToBoolean(this.chkbxEDIInvoiceRequired.Checked);
                        updateDealerInformation.DealerNotes = (this.txtDealerNotes.Text.Trim() == "") ? null : this.txtDealerNotes.Text.Trim().Replace("'", "''");
                        updateDealerInformation.BillForShipping = Convert.ToBoolean(this.chkbxBillForShipping.Checked);
                        updateDealerInformation.RenewalType = this.ddlRenewalType.SelectedValue;
                        updateDealerInformation.PODeliveryMethod = Convert.ToInt32(this.ddlPODeliveryMethod.SelectedValue);
                        // 08/14/2014 - Set UNIXRateCode to value of hdnfldUNIXRateCodeDefault.
                        //updateDealerInformation.UnixRateCode = Convert.ToInt32(this.txtUNIXRateCode.Text.Trim());
                        updateDealerInformation.UnixRateCode = Convert.ToInt32(this.hdnfldUNIXRateCodeDefault.Value.ToString());
                        //----------------------------------------------------------------------------------------------------//
                        updateDealerInformation.RateFieldType = Convert.ToInt32(this.ddlRateFieldType.SelectedValue);
                        updateDealerInformation.DiscountField = (this.txtDiscountField.Text.Trim() == "") ? (int?)null : Convert.ToInt32(this.txtDiscountField.Text.Trim());
                        updateDealerInformation.ReportFormatType = this.ddlReportFormatType.SelectedValue;

                        try
                        {
                            idc.SubmitChanges();

                            // Refresh the GridView.
                            this.gvDealersView.DataBind();

                            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('dealerDialog')", true);
                        }
                        catch (Exception ex)
                        {
                            // Display the system generated error message.
                            throw new Exception(ex.Message);
                        }
                    }
                }
                else
                {
                    throw new Exception(string.Format("{0}: {1}", responseCode, HError.GetError(responseCode)));
                }
            }
            else
            {
                // Error, for Validation did not pass.
                return;
            }
        }
        catch (Exception ex)
        {
            this.VisibleErrors_DealerDialog(string.Format("An error occurred: {0}", ex.Message));
        }
    }
    #endregion

    #region CANCEL ADD|EDIT MODAL/DIALOG.
    /// <summary>
    /// Modal/Dialog Cancel button function.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnCancelDealer_Click(object sender, EventArgs e)
    {
        this.InvisibleErrors_DealerDialog();
        this.SetDefaultValues_DealerDialog();
        this.divForeignAddress.Visible = false;
    }
    #endregion

    #region GOTO DEALER DOCUMENTS PAGE.
    /// <summary>
    /// Based on the DealerID, User is taken to Dealer Documents Upload page (with Dealer's Document information).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void imgbtnGotoDealerDocuments_Click(object sender, EventArgs e)
    {
        ImageButton imgbtn = (ImageButton)sender;
        int dealerID = 0;
        int.TryParse(imgbtn.CommandArgument, out dealerID);

        Response.Redirect(string.Format("DealerDocuments.aspx?DealerID={0}&Upload={1}", dealerID, "False"));
    }

    /// <summary>
    /// LinkButton takes User to general Dealer Documents Upload page.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void lnkbtnUploadDealerDocuments_Click(object sender, EventArgs e)
    {
        Response.Redirect("DealerDocuments.aspx?DealerID=0&Upload=True");
    }
    #endregion
}
