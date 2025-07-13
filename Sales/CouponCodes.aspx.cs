/*
 * File: Rates Maintenance
 * Author: TDO
 * Created: Dec 21, 2011
 * 
 * Maintain Instadose Coupon Table
 * 
 *  Copyright (C) 2010 Mirion Technologies. All Rights Reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Instadose.Data;

public partial class Sales_CouponCodes : System.Web.UI.Page
{
    // Create the database reference
    InsDataContext idc = new InsDataContext();

    protected void Page_Load(object sender, EventArgs e)
    {
        ResetValidateNote();

        if (!this.IsPostBack)
        {
            InitiateAllControls();
            SetControlsDefault();
        }
    }

    public string GetEditImageUrl()
    {
        if (this.radShow.SelectedItem.Value == "Active")
            return "../css/dsd-default/images/icons/note_edit.png";
        else
            return "../css/dsd-default/images/icons/note.png";
    }

    public string GetEditToolTip()
    {
        if (this.radShow.SelectedItem.Value == "Active")
            return "Edit/Deactivate Coupon Code";
        else
            return "Open Coupon Code";
    }

    private void ResetValidateNote()
    {
        this.lblCouponCodeValidate.Visible = false;
        this.lblCouponDescValidate.Visible = false;
        this.lblEffectiveDateValidate.Visible = false;
        this.lblExpirationDateValidate.Visible = false;
        this.lblFreeShippingValidate.Visible = false;
        this.lblProductVersionValidate.Visible = false;
        this.lblBrandSourceValidate.Visible = false;
        this.lblRateValidate.Visible = false;

        this.lblValidationNote.Text = "";
        this.lblValidationNote.Visible = false;

        this.lblCommitConfirm.Text = "";
        this.lblCommitConfirm.Visible = false;

    }

    private void InitialSetEnableInputControls()
    {       
        this.txtCouponCode.Enabled = true;       
        this.txtCouponDesc.Enabled = true;        
        this.txtEffectiveDate.Enabled = true;        
        this.txtExpirationDate.Enabled = true;        
        this.txtProductVersion.Enabled = true;        
        this.chkFreeShipping.Enabled = true;        
        this.ddlBrandSource.Enabled = true;       
        this.ddlRate.Enabled = true;
    }

    private void InitialSetDisableInputControls()
    {
        this.txtCouponCode.Enabled = false;
        this.txtCouponDesc.Enabled = false;
        this.txtEffectiveDate.Enabled = false;
        this.txtExpirationDate.Enabled = false;
        this.txtProductVersion.Enabled = false;
        this.chkFreeShipping.Enabled = false;
        this.ddlBrandSource.Enabled = false;
        this.ddlRate.Enabled = false;
    }

    private void SetControlsDefault()
    {
    }

    private void InitiateAllControls()
    {
        InitiateBrandSourceControl();
        InitiateRateControl();
    }

    private void InitiateRateControl()
    {
        var rates = from r in idc.Rates
                    orderby r.RateCode
                    select new
                    {
                        r.RateID,
                        r.RateCode
                    };

        this.ddlRate.DataSource = rates;
        this.ddlRate.DataTextField = "RateCode";
        this.ddlRate.DataValueField = "RateID";
        this.ddlRate.DataBind();

        ListItem item0 = new ListItem("", "0");
        this.ddlRate.Items.Insert(0, item0);
    }

    private void InitiateBrandSourceControl()
    {
        var brands = from r in idc.BrandSources
                     orderby r.BrandName
                     select new
                     {
                         r.BrandSourceID,
                         r.BrandName
                     };

        this.ddlBrandSource.DataSource = brands;
        this.ddlBrandSource.DataTextField = "BrandName";
        this.ddlBrandSource.DataValueField = "BrandSourceID";
        this.ddlBrandSource.DataBind();

    }

    protected void btnGridEdit_Click(object sender, EventArgs e)
    {
        ImageButton btn = (ImageButton)sender;
        string myCmdname = btn.CommandName.ToString();
        string selCouponID = btn.CommandArgument.ToString();

        this.btnSave.Text = "Update";
        SetValuesToControls(selCouponID, "Update");
        LoadPopUpByMode("Update");

        if (this.radShow.SelectedItem.Value == "Active")
        {
            this.btnSave.Visible = true;
            this.btnDelete.Visible = true;
            InitialSetEnableInputControls();
        }
        else
        {
            this.btnSave.Visible = false;
            this.btnDelete.Visible = false;
            InitialSetDisableInputControls();
        }            
    }

    protected void btnAddRecord_Click(object sender, EventArgs e)
    {
        this.btnSave.Text = "Save";
        this.btnSave.Visible = true;
        this.btnDelete.Visible = false;
        SetValuesToControls("0", "Save");
        LoadPopUpByMode("Save");
        InitialSetEnableInputControls();       
    }

    private void LoadPopUpByMode(string mode)
    {
        if (mode == "Save")
            this.PopupAddHeader.InnerText = "Add Coupon Code";
        else
            this.PopupAddHeader.InnerText = "Edit/Deactivate Coupon Code";

        this.ModalPopup.Show();
    }

    private void SetValuesToControls(string selCouponID, string mode)
    {

        Coupon myCoupon = (from o in idc.Coupons
                           where o.CouponID == int.Parse(selCouponID)
                           select o).FirstOrDefault();

        if (mode == "Save")    // adding mode
        {
            this.SelectedCouponID.Value = "0";
            this.txtCouponCode.Text = "";           
            this.txtCouponDesc.Text = "";           
            this.txtEffectiveDate.Text = "";            
            this.txtExpirationDate.Text = "";           
            this.txtProductVersion.Text = "";            
            this.chkFreeShipping.Checked = false;            
            this.ddlBrandSource.SelectedIndex = 0;           
            this.ddlRate.SelectedIndex = 0;
            
            SetFocus(this.txtCouponCode);
        }
        else if (mode == "Update")    // updating mode           
        {
            this.SelectedCouponID.Value = selCouponID;
            this.txtCouponCode.Text = myCoupon.CouponCode;            
            this.txtCouponDesc.Text = (myCoupon.CouponDesc == null ? "" : myCoupon.CouponDesc);           
            this.txtEffectiveDate.Text = (myCoupon.EffectiveDate == null ? "" : myCoupon.EffectiveDate.Value.ToShortDateString());            
            this.txtExpirationDate.Text = (myCoupon.ExpirationDate == null ? "" : myCoupon.ExpirationDate.Value.ToShortDateString());           
            this.txtProductVersion.Text = (myCoupon.ProductVersion == null ? "" : myCoupon.ProductVersion.Value.ToString());            
            this.chkFreeShipping.Checked = myCoupon.FreeShipping;            
            this.ddlBrandSource.SelectedValue = myCoupon.BrandSourceID.ToString();            
            this.ddlRate.SelectedValue = (myCoupon.RateID == null ? "0" : myCoupon.RateID.ToString());
            
            SetFocus(this.txtCouponCode);
        }

    }
    
    private Boolean PassValidation()
    {
        DateTime myDate;
        if (this.txtCouponCode.Text.Length == 0)
        {
            this.lblCouponCode.Visible = true;
            this.lblValidationNote.Text = "Coupon Code is required.";
            this.lblValidationNote.Visible = true;
            return false;
        }

        if (this.txtEffectiveDate.Text.Trim().Length > 0)
        {
            if (DateTime.TryParse(this.txtEffectiveDate.Text.Trim(), out myDate) == false)
            {
                this.lblEffectiveDateValidate.Visible = true;
                this.lblValidationNote.Text = "Effective Date is not a date. Please enter a correct date.";
                this.lblValidationNote.Visible = true;
                return false;
            }
        }

        if (this.txtExpirationDate.Text.Trim().Length > 0)
        {
            if (DateTime.TryParse(this.txtExpirationDate.Text.Trim(), out myDate) == false)
            {
                this.lblExpirationDateValidate.Visible = true;
                this.lblValidationNote.Text = "Expiration Date is not a date. Please enter a correct date.";
                this.lblValidationNote.Visible = true;
                return false;
            }
        }

        return true;
    }

   
    // Insert,Update process over here
    protected void btnSave_Click(object sender, EventArgs e)
    {

        if (PassValidation())
        {
            Int32? myNullRateID = null;
            DateTime? myNullEffectiveDate = null;
            DateTime? myNullExpirationDate = null;
            float? myNullProductVersion = null;
            string myNullCouponDesc = null;


            int myCouponID = int.Parse(this.SelectedCouponID.Value);

            string myCouponCode = this.txtCouponCode.Text;
            int myBrandSourceID = int.Parse(this.ddlBrandSource.SelectedItem.Value);
            Boolean myFreeShipping = this.chkFreeShipping.Checked;

            Int32? myRateID = (int.Parse(this.ddlRate.SelectedItem.Value) == 0 ? myNullRateID : int.Parse(this.ddlRate.SelectedItem.Value));
            float? myProductVersion = (this.txtProductVersion.Text.Trim().Length == 0 ? myNullProductVersion : float.Parse(this.txtProductVersion.Text));
            string myCouponDesc = (this.txtCouponDesc.Text.Trim().Length == 0 ? myNullCouponDesc : this.txtCouponDesc.Text);
            DateTime? myEffectiveDate = (this.txtEffectiveDate.Text.Trim().Length == 0 ? myNullEffectiveDate : DateTime.Parse(this.txtEffectiveDate.Text));
            DateTime? myExpirationDate = (this.txtExpirationDate.Text.Trim().Length == 0 ? myNullExpirationDate : DateTime.Parse(this.txtExpirationDate.Text));

            switch (this.btnSave.Text)
            {
                case "Save":

                    try
                    {
                        Coupon d = new Coupon
                        {
                            CouponCode = myCouponCode,
                            RateID = myRateID,
                            CouponDesc = myCouponDesc,
                            EffectiveDate = myEffectiveDate,
                            ExpirationDate = myExpirationDate,
                            FreeShipping = myFreeShipping,
                            BrandSourceID = myBrandSourceID,
                            ProductVersion = myProductVersion,
                            Active = true
                        };

                        idc.Coupons.InsertOnSubmit(d);
                        idc.SubmitChanges();

                        this.lblCommitConfirm.Text = "Insert successful!";
                        this.lblCommitConfirm.ForeColor = System.Drawing.Color.Blue;
                        this.lblCommitConfirm.Visible = true;

                    }
                    catch (Exception ex)
                    {
                        this.lblCommitConfirm.Text = "Insert failed!" + "<br/>" + ex.Message;
                        this.lblCommitConfirm.ForeColor = System.Drawing.Color.Red;
                        this.lblCommitConfirm.Visible = true;
                        this.ModalPopup.Show();
                    }

                    this.ModalPopup.Show();
                    break;

                case "Update":

                    try
                    {
                        Coupon c =
                          (from r in idc.Coupons 
                           where r.CouponID == myCouponID
                           select r).First();

                        c.CouponCode = myCouponCode;
                        c.RateID = myRateID;
                        c.CouponDesc = myCouponDesc;
                        c.EffectiveDate = myEffectiveDate;
                        c.ExpirationDate = myExpirationDate;
                        c.FreeShipping = myFreeShipping;
                        c.BrandSourceID = myBrandSourceID;
                        c.ProductVersion = myProductVersion;

                        idc.SubmitChanges();

                        this.lblCommitConfirm.Text = "Update successful!";
                        this.lblCommitConfirm.ForeColor = System.Drawing.Color.Blue;
                        this.lblCommitConfirm.Visible = true;

                    }
                    catch (Exception ex)
                    {
                        this.lblCommitConfirm.Text = "Update failed!" + "<br/>" + ex.Message;
                        this.lblCommitConfirm.ForeColor = System.Drawing.Color.Red;
                        this.lblCommitConfirm.Visible = true;
                        this.ModalPopup.Show();
                    }

                    break;
            }

            // refresh the gridview
            this.grdView.DataBind();
        }
        else
        {
            // stay the pop-up window if validation failed.
            this.ModalPopup.Show();
        }

    }

    // Delete process over here
    protected void btnDelete_Click(object sender, EventArgs e)
    {
        int myCouponID = int.Parse(this.SelectedCouponID.Value);

        try
        {
            Coupon c =
              (from r in idc.Coupons
               where r.CouponID == myCouponID
               select r).First();

            if (c != null)
            {
                c.Active = false;

                idc.SubmitChanges();

                this.lblCommitConfirm.Text = "Deactivate successful!";
                this.lblCommitConfirm.ForeColor = System.Drawing.Color.Blue;
                this.lblCommitConfirm.Visible = true;

            }
            else
            {
                this.lblCommitConfirm.Text = "Deactivate failed! Can not find a record.";
                this.lblCommitConfirm.ForeColor = System.Drawing.Color.Red;
                this.lblCommitConfirm.Visible = true;
                this.ModalPopup.Show();

            }

        }
        catch (Exception ex)
        {
            this.lblCommitConfirm.Text = "Deactivate failed!" + "<br/>" + ex.Message;
            this.lblCommitConfirm.ForeColor = System.Drawing.Color.Red;
            this.lblCommitConfirm.Visible = true;
            this.ModalPopup.Show();
        }

        // refresh the gridview
        this.grdView.DataBind();

    }

}