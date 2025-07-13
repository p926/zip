using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Instadose;
using Instadose.Data;

public partial class CustomerService_ShippingQueueClear : System.Web.UI.Page
{
    // Create the database reference
    InsDataContext idc = new InsDataContext();
    AppDataContext adc = new AppDataContext();
    public bool isMalvernIntegration = false;

    protected void Page_Load(object sender, EventArgs e)
    {
        InvisibleMergeError();
        InvisibleMergeSuccess();
        InvisibleInsertError();
        InvisibleInsertSuccess();

        if (!this.IsPostBack)
        {
            InitiateAllControls();
            SetControlsDefault();

            // 5/29/2014, Tdo. Check to see if the system is using FeDex shipment or Malvern shipment
            isMalvernIntegration = Convert.ToBoolean(GetAppSettings("MalvernIntegration"));
        }
    }

    private string GetAppSettings(string pAppKey)
    {
        var mySetting = (from AppSet in idc.AppSettings where AppSet.AppKey == pAppKey select AppSet).FirstOrDefault();
        return (mySetting != null) ? mySetting.Value : "";
    }

    //------------------------ Inser package tracking number ----------------------------//
    

    private void SetControlsDefault()
    {
        try
        {
            this.ddlUserCountryID.SelectedValue = "1";
            InitiateStateControl();
        }
        catch { }
    }

    private void InitiateAllControls()
    {
        // DONE by sqldatasource controls
        this.ddlUserCountryID.DataBind();
        ListItem item0 = new ListItem("", "0");
        this.ddlUserCountryID.Items.Insert(0, item0);
    }

    private void InitiateStateControl()
    {
        try
        {
            this.ddlUserStateID.DataBind();
            ListItem item0 = new ListItem("", "0");
            this.ddlUserStateID.Items.Insert(0, item0);
        }
        catch { }
    }

    protected void ddlUserCountryID_SelectedIndexChanged(object sender, EventArgs e)
    {
        InitiateStateControl();
    }

    private Boolean ExistPackage(string pPackageID)
    {
        try
        {
            int myPackageID = int.Parse(pPackageID);
            int count = (from o in idc.Packages
                         where o.PackageID == myPackageID && (o.TrackingNumber == null || o.TrackingNumber == "")
                         select o).Count();
            if (count > 0)
                return true;
            else
                return false;
        }
        catch
        {
            return false;
        }
    }

    private Boolean PassValidation()
    {
        DateTime myDate;
        double myShippingCost;

        if (this.txtPackageID.Text.Trim().Length == 0)
        {            
            VisibleInsertError("Package# is required.");
            SetFocus(this.txtPackageID);
            return false;
        }
        else
        {
            if (!ExistPackage(this.txtPackageID.Text.Trim()))
            {                
                VisibleInsertError("Package# never exist or already have a tracking#.");
                SetFocus(this.txtPackageID);
                return false;
            }
        }

        if (this.txtTrackingNo.Text.Trim().Length == 0)
        {            
            VisibleInsertError("Tracking# is required.");
            SetFocus(this.txtTrackingNo);
            return false;
        }


        if (this.txtShipDate.Text.Trim().Length == 0)
        {           
            VisibleInsertError("Ship Date is required.");
            SetFocus(this.txtShipDate);
            return false;
        }
        else
        {
            if (DateTime.TryParse(this.txtShipDate.Text.Trim(), out myDate) == false)
            {                
                VisibleInsertError("Ship Date is not a date. Please enter a correct date.");
                SetFocus(this.txtShipDate);
                return false;
            }
        }

        if (this.txtShippingCost.Text.Trim().Length > 0)
        {
            if (double.TryParse(this.txtShippingCost.Text, out myShippingCost) == false)
            {                
                VisibleInsertError("Shipping Cost must be a number.");
                SetFocus(this.txtShippingCost);
                return false;
            }
            else
            {
                if (myShippingCost < 0)
                {                    
                    VisibleInsertError("Shipping Cost must be greater than 0.");
                    SetFocus(this.txtShippingCost);
                    return false;
                }
            }
        }

        return true;
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        try
        {
            if (PassValidation())
            {
                Int32? myIntNull = null;
                DateTime? myDateTimeNull = null;
                decimal? myDecimalNull = null;
                string myStringNull = null;

                int myPackageID = int.Parse(this.txtPackageID.Text.Trim());
                string myTrackingNumber = (this.txtTrackingNo.Text.Trim().Length == 0 ? myStringNull : this.txtTrackingNo.Text.Trim());
                DateTime myShipDate = DateTime.Parse(this.txtShipDate.Text);
                decimal? myShippingCost = (this.txtShippingCost.Text.Trim().Length == 0 ? myDecimalNull : decimal.Parse(this.txtShippingCost.Text));

                string myCompanyName = (this.txtCompanyName.Text.Trim().Length == 0 ? myStringNull : this.txtCompanyName.Text.Trim());
                string myContactName = (this.txtContactName.Text.Trim().Length == 0 ? myStringNull : this.txtContactName.Text.Trim());
                string myAddress1 = (this.txtUserAddress1.Text.Trim().Length == 0 ? myStringNull : this.txtUserAddress1.Text.Trim());
                string myAddress2 = (this.txtUserAddress2.Text.Trim().Length == 0 ? myStringNull : this.txtUserAddress2.Text.Trim());
                string myCity = (this.txtUserCity.Text.Trim().Length == 0 ? myStringNull : this.txtUserCity.Text.Trim());
                string myState = (int.Parse(this.ddlUserStateID.SelectedValue) == 0 ? myStringNull : this.ddlUserStateID.SelectedItem.Text);
                string myPostalCode = (this.txtUserPostalCode.Text.Trim().Length == 0 ? myStringNull : this.txtUserPostalCode.Text.Trim());
                string myCountry = (int.Parse(this.ddlUserCountryID.SelectedValue) == 0 ? myStringNull : this.ddlUserCountryID.SelectedItem.Text);

                if (isMalvernIntegration)
                {
                    UpdateShippingInfo d = new UpdateShippingInfo
                    {
                        PackageID = myPackageID,
                        TrackingNumber = myTrackingNumber,
                        ShipDate = myShipDate,
                        Company = myCompanyName,
                        ContactName = myContactName,
                        Address1 = myAddress1,
                        Address2 = myAddress2,
                        City = myCity,
                        State = myState,
                        PostalCode = myPostalCode,
                        Country = myCountry,
                        ActualShippingCost = myShippingCost
                    };

                    adc.UpdateShippingInfos.InsertOnSubmit(d);
                }
                else
                {
                    if_ShipPackage d = new if_ShipPackage
                    {
                        PackageID = myPackageID,
                        TrackingNumber = myTrackingNumber,
                        ShipDate = myShipDate,
                        Company = myCompanyName,
                        ContactName = myContactName,
                        Address1 = myAddress1,
                        Address2 = myAddress2,
                        City = myCity,
                        State = myState,
                        PostalCode = myPostalCode,
                        Country = myCountry,
                        ShippingCost = myShippingCost
                    };

                    adc.if_ShipPackages.InsertOnSubmit(d);
                }
                
                adc.SubmitChanges();
                
                VisibleInsertSuccess("Update Tracking succeeded.");

            }
        }
        catch (Exception ex)
        {
            // Display the system generated error message.            
            VisibleInsertError(string.Format("An error occurred: {0}", ex.Message));
        }

    }
    //------------------------ Inser package tracking number ----------------------------//

    //------------------------ Merge Packages ----------------------------//
    

    private Boolean MPassValidation()
    {

        Int64  myInt;

        if (this.txtMPackageID.Text.Trim().Length == 0)
        {            
            VisibleMergeError("Package# is required.");
            return false;
        }

        if (this.txtMTrackingNo.Text.Trim().Length == 0 || Int64.TryParse(this.txtMTrackingNo.Text, out myInt) == false)
        {           
            VisibleMergeError("Tracking# is required and must be a number.");
            return false;
        }
        else if (!MExistTrackingNoByPackageID(this.txtMPackageID.Text.Trim(), this.txtMTrackingNo.Text.Trim()))
        {           
            VisibleMergeError("Invalid tracking#.");
            return false;
        }

        return true;
    }

    private Boolean MExistTrackingNoByPackageID(string pPackageID, string pTrackingNo)
    {
        try
        {
            int myPackageID = int.Parse(pPackageID);
            int count = 0;

            if (isMalvernIntegration)
            {
                count = (from o in adc.UpdateShippingInfos
                         where o.PackageID == myPackageID && o.TrackingNumber == pTrackingNo
                         select o).Count();
            }
            else
            {
                count = (from o in adc.if_ShipPackages
                             where o.PackageID == myPackageID && o.TrackingNumber == pTrackingNo
                             select o).Count();
            }
            
            if (count > 0)
                return true;
            else
                return false;
        }
        catch
        {
            return false;
        }
    }

    private Boolean MExistPackage(string pPackageID)
    {
        try
        {
            int myPackageID = int.Parse(pPackageID);
            int count = (from o in idc.Packages
                         where o.PackageID == myPackageID && (o.TrackingNumber == null || o.TrackingNumber == "")
                         select o).Count();
            if (count > 0)
                return true;
            else
                return false;
        }
        catch
        {
            return false;
        }
    }

    protected void btnMSave_Click(object sender, EventArgs e)
    {
        if (MPassValidation())
        {

            try
            {
                int shippedPackageID = int.Parse(this.txtMPackageID.Text.Trim());
                string myTrackingNo = this.txtMTrackingNo.Text.Trim();

                if (isMalvernIntegration)
                {
                    UpdateShippingInfo myShipPackage = (from o in adc.UpdateShippingInfos
                                                    where o.PackageID == shippedPackageID && o.TrackingNumber == myTrackingNo
                                                    select o).FirstOrDefault();

                    // Store the list of packageIDs that the user has entered.
                    List<string> packageIDList = new List<string>();
                    packageIDList.AddRange(this.txtMMultiPackageID.Text.Split('\n'));

                    if (packageIDList.Count > 0)
                    {
                        // Loop through each packageID.
                        foreach (string curPackageID in packageIDList)
                        {
                            if (curPackageID.Trim() == "")
                            {
                            }
                            else
                            {
                                if (MExistPackage(curPackageID.Trim()))
                                {
                                    if (!MExistTrackingNoByPackageID(curPackageID.Trim(), myTrackingNo))
                                    {
                                        UpdateShippingInfo d = new UpdateShippingInfo
                                        {
                                            PackageID = int.Parse(curPackageID.Trim()),
                                            TrackingNumber = myShipPackage.TrackingNumber,
                                            ShipDate = myShipPackage.ShipDate,
                                            Company = myShipPackage.Company,
                                            ContactName = myShipPackage.ContactName,
                                            Address1 = myShipPackage.Address1,
                                            Address2 = myShipPackage.Address2,
                                            City = myShipPackage.City,
                                            State = myShipPackage.State,
                                            PostalCode = myShipPackage.PostalCode,
                                            Country = myShipPackage.Country,
                                            ActualShippingCost = myShipPackage.ActualShippingCost
                                        };

                                        adc.UpdateShippingInfos.InsertOnSubmit(d);
                                        adc.SubmitChanges();
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if_ShipPackage myShipPackage = (from o in adc.if_ShipPackages
                                                    where o.PackageID == shippedPackageID && o.TrackingNumber == myTrackingNo
                                                    select o).FirstOrDefault();


                    // Store the list of packageIDs that the user has entered.
                    List<string> packageIDList = new List<string>();
                    packageIDList.AddRange(this.txtMMultiPackageID.Text.Split('\n'));

                    if (packageIDList.Count > 0)
                    {
                        // Loop through each packageID.
                        foreach (string curPackageID in packageIDList)
                        {
                            if (curPackageID.Trim() == "")
                            {
                            }
                            else
                            {
                                if (MExistPackage(curPackageID.Trim()))
                                {
                                    if (!MExistTrackingNoByPackageID(curPackageID.Trim(), myTrackingNo))
                                    {
                                        if_ShipPackage d = new if_ShipPackage
                                        {
                                            PackageID = int.Parse(curPackageID.Trim()),
                                            TrackingNumber = myShipPackage.TrackingNumber,
                                            ShipDate = myShipPackage.ShipDate,
                                            Company = myShipPackage.Company,
                                            ContactName = myShipPackage.ContactName,
                                            Address1 = myShipPackage.Address1,
                                            Address2 = myShipPackage.Address2,
                                            City = myShipPackage.City,
                                            State = myShipPackage.State,
                                            PostalCode = myShipPackage.PostalCode,
                                            Country = myShipPackage.Country,
                                            ShippingCost = myShipPackage.ShippingCost
                                        };

                                        adc.if_ShipPackages.InsertOnSubmit(d);
                                        adc.SubmitChanges();
                                    }
                                }
                            }
                        }
                    }
                }                
                
                VisibleMergeSuccess("Merge succeeded.");
            }
            catch (Exception ex)
            {
                // Display the system generated error message.                
                VisibleMergeError(string.Format("An error occurred: {0}", ex.Message));
            }

        }
    }
    //------------------------ Merge Packages ----------------------------//

    private void InvisibleMergeError()
    {
        // Reset submission form error message      
        this.mergeErrorMsg.InnerText = "";
        this.mergeError.Visible = false;
    }

    private void VisibleMergeError(string error)
    {
        this.mergeErrorMsg.InnerText = error;
        this.mergeError.Visible = true;
    }

    private void InvisibleMergeSuccess()
    {
        // Reset submission form error message      
        this.mergeSuccessMsg.InnerText = "";
        this.mergeSuccess.Visible = false;
    }

    private void VisibleMergeSuccess(string error)
    {
        this.mergeSuccessMsg.InnerText = error;
        this.mergeSuccess.Visible = true;
    }

    private void InvisibleInsertError()
    {
        // Reset submission form error message      
        this.insertErrorMsg.InnerText = "";
        this.insertError.Visible = false;
    }

    private void VisibleInsertError(string error)
    {
        this.insertErrorMsg.InnerText = error;
        this.insertError.Visible = true;
    }

    private void InvisibleInsertSuccess()
    {
        // Reset submission form error message      
        this.insertSuccessMsg.InnerText = "";
        this.insertSuccess.Visible = false;
    }

    private void VisibleInsertSuccess(string error)
    {
        this.insertSuccessMsg.InnerText = error;
        this.insertSuccess.Visible = true;
    }

}