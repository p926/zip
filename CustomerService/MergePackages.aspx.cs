using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Instadose;
using Instadose.Data;

public partial class CustomerService_MergePackages : System.Web.UI.Page
{
    AppDataContext adc = new AppDataContext();
    InsDataContext idc = new InsDataContext();

    protected void Page_Load(object sender, EventArgs e)
    {
        ResetValidateNote();
    }

    private void ResetValidateNote()
    {
        this.lblPackageIDValidate.Visible = false;
        this.lblTrackingNoValidate.Visible = false;

        this.lblError.Visible = false;
        this.lblSuccess.Visible = false;
        this.lblSuccess.Text = "";
        this.lblError.Text = "";
    }

    private Boolean PassValidation()
    {
       
        int myInt;

        if (this.txtPackageID.Text.Trim().Length == 0)
        {
            this.lblPackageIDValidate.Visible = true;
            this.lblPackageIDValidate.InnerText = "Package# is required.";
            return false;
        }

        if (this.txtTrackingNo.Text.Trim().Length == 0 || int.TryParse(this.txtTrackingNo.Text, out myInt) == false)
        {
            this.lblTrackingNoValidate.Visible = true;
            this.lblTrackingNoValidate.InnerText = "Tracking# is required and must be a number";
            return false;
        }
        else if (! ExistTrackingNoByPackageID(this.txtPackageID.Text.Trim(), this.txtTrackingNo.Text.Trim()))
        {
            this.lblTrackingNoValidate.Visible = true;
            this.lblTrackingNoValidate.InnerText = "Invalid tracking#.";
            return false;
        }

        return true;
    }

    private Boolean ExistTrackingNoByPackageID(string pPackageID, string pTrackingNo)
    {
        try
        {
            int myPackageID = int.Parse(pPackageID);
            int count = (from o in adc.if_ShipPackages 
                         where o.PackageID == myPackageID && o.TrackingNumber == pTrackingNo 
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

    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (PassValidation())
        {
          
            try
            {
                int shippedPackageID = int.Parse(this.txtPackageID.Text.Trim());
                string myTrackingNo = this.txtTrackingNo.Text.Trim();
                if_ShipPackage myShipPackage = (from o in adc.if_ShipPackages
                             where o.PackageID == shippedPackageID && o.TrackingNumber == myTrackingNo
                             select o).FirstOrDefault();


                // Store the list of packageIDs that the user has entered.
                List<string> packageIDList = new List<string>();
                packageIDList.AddRange(this.txtMultiPackageID.Text.Split('\n'));

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
                            if (ExistPackage(curPackageID.Trim()))
                            {
                                if (!ExistTrackingNoByPackageID(curPackageID.Trim(), myTrackingNo))
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


                this.lblSuccess.Text = "Merge successful.";
                this.lblSuccess.Visible = true;
            }
            catch (Exception ex)
            {
                // Display the system generated error message.
                this.lblError.Text = string.Format("An error occurred: {0}", ex.Message);
                this.lblError.Visible = true;
            }
            
        }
    }
}