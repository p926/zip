using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Instadose;
using Instadose.Data;
using Instadose.Integration;
using Instadose.Processing;
using Instadose.Device;
using Mirion.DSD.Utilities.Constants;

public partial class CustomerService_CreateOrder : System.Web.UI.Page
{
    #region Private Properties

    // Order type determines cost and shipping
    // New = use rates, replacement = 25 USD, no charge replacement = 0, no replacement = 25 USD
    private enum OrderTypes
    {
        // 8/2013 WK - New order is the FIRST order (not broken; lost - replace/no rplace) of account only.  
        //  All ther
        // Record as: New, LostReplacement, LostReplacement, NoReplacement
        New = 0, Replacement = 1, NoChargeReplacement = 2, NoReplacement = 3
    };

    // Product types to determine which tab will used for ordering.
    private enum productTypes
    {
        Instadose1 = 0, InstadosePlus = 1, Accessories = 2, Instadose2 = 3
    };

    private Account account = null;
    private Location location = null;
    private Order order = null;
    private AccountCreditCard accountcreditcard = null;
    private OrderTypes orderType = OrderTypes.New;
    private bool displayChangeLocation = false;
    string userName = "PORTAL";

    private Boolean MalvernIntegration;
    private Boolean OrbitalIntegration;
    private Boolean isNoneICCareAddOn;
    private Boolean isFirstOrder;

    private string defaultShippingCarrier = null;

    DataTable dtOrderUserAssign;   // DataTable to store Order User Assign

    // Create the database reference
    private InsDataContext idc = new InsDataContext();
    private AppDataContext adc = new AppDataContext();

    private int iConsignmentCustomerTypeID = 47;
    private int iMaxAccessoriesCount = 1000;
    private int instadose2ProductGroupID = 11;
    private int instadose2RollProductGroupID = 16;
    private int instadose2SheetProductGroupID = 17;

    // id 2 productIDs
    const string instadose2ProductSKU = "INSTA20";
    const string instadose2BumperBlueProductSKU = "ID2BBUMP";
    const string instadose2BumperGreyProductSKU = "ID2GYBUMP";
    const string instadose2BumperGreenProductSKU = "ID2GBUMP";
    const string instadose2BumperOrangeProductSKU = "ID2OBUMP";
    const string instadose2BumperPinkProductSKU = "ID2PBUMP";
    const string instadose2BumperRedProductSKU = "ID2RBUMP";

    const int groupID1 = 9;
    const int groupID2 = 11;
    const int groupIDPlus = 10;
    const int groupLink = 5;
    const int groupUSB = 6;

    #endregion

    #region Page and Control Events    

    /// <summary>
    /// Page_Load
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (User.Identity.Name.IndexOf('\\') > 0)
                this.userName = User.Identity.Name.Split('\\')[1];
            else
                this.userName = "Testing";

            //this.InvisibleErrorMsg();

            MalvernIntegration = Convert.ToBoolean((from a in idc.AppSettings where a.AppKey == "MalvernIntegration" select a.Value).FirstOrDefault());
            OrbitalIntegration = Convert.ToBoolean((from a in idc.AppSettings where a.AppKey == "OrbitalIntegration" select a.Value).FirstOrDefault());

            // For some reason this event cannot be assigned on the ASPX page.
            //txtTransitionServiceStart.TextChanged += txtTransitionServiceStart_TextChanged;

            // Load the querystring params into hidden fields.
            if (!IsPostBack) setPageData();

            // Load the data required on the page, every time.
            loadPageData();

            if (account.isDemoAccount && account.BrandSourceID == 3)
            {
                SelectedProductName.Visible = false;
            }


            // Do not fill the controls more than once.
            if (IsPostBack) return;

            ClearOutAllSessionInfo_IDPlus();
            ClearOutAllSessionInfo_ID2();

            // Call the fill controls
            fillControls();

            // if this is a renewal order
            if (hfSelectedProductName.Value == "Renewal" && account.BrandSourceID == 3)  // ic care only
                populateRenewalGrid();


            //// enable Instadose Elite Demo order for ICCare Demo account
            ////if (account.BrandSourceID == 3 && account.CustomerTypeID == 3)
            //if (account.BrandSourceID == 3)
            //{
            //    radProductName.Items.FindByValue("Instadose Elite Demo").Attributes.Remove("style");
            //}
            //else
            //{
            //    radProductName.Items.FindByValue("Instadose Elite Demo").Attributes.Add("style", "display: none");
            //}

            // If this is the first time the page has loaded....
            changeOrderType(orderType, true);

            //Display the dropdown controls based on the Malvern Integration flag - hbabu 05/12/14
            if (MalvernIntegration)
            {
                divMalvernCarrier.Visible = true;
                divMalvernShipMethod.Visible = true;
                divFedex.Visible = false;
            }
            else
            {
                divMalvernCarrier.Visible = false;
                divMalvernShipMethod.Visible = false;
                divFedex.Visible = true;
            }
        }
        catch (Exception ex)
        {
            btnCreate.Enabled = false;
            this.VisibleErrorMsg(ex.Message);
        }
    }

    protected void populateRenewalGrid()
    {

        // this is a renewal order
        gvProduct.Visible = false;
        gvInstadosePlus.Visible = false;
        gvInstadose2.Visible = false;
        gvAccessories.Visible = false;
        gvInstadoseEliteDemo.Visible = false;

        gvRenewal.Visible = true;

        var productsInstadose1 = (from o in idc.OrderDetails
                                  join p in idc.Products on o.ProductID equals p.ProductID
                                  where (o.OrderID == this.order.OrderID)
                                  select new
                                  {
                                      ProductID = p.ProductID,
                                      Color = p.Color,
                                      ProductName = p.ProductName,
                                      BillingTermID = account.BillingTermID,
                                      BillingTermDesc = account.BillingTerm.BillingTermDesc,
                                      ProductSKU = p.ProductSKU,
                                      Price = o.Price,
                                      ProductGroupID = p.ProductGroupID,
                                      Quantity = o.Quantity
                                  });



        // gvProduct.DataSource = null;
        gvRenewal.DataSource = productsInstadose1;
        gvRenewal.DataBind();


        foreach (GridViewRow row in gvRenewal.Rows)
        {

            TextBox txtQty = (TextBox)(row.FindControl("txtQty"));
            //TextBox txtQty = (TextBox)(row.Cells[2].FindControl("txtQty"));
            Label lblPrice = (Label)(row.Cells[3].FindControl("lblPrice"));
            Label lblTotal = (Label)(row.Cells[4].FindControl("lblTotal"));

            try
            {
                decimal dtotal = decimal.Parse(lblPrice.Text) * decimal.Parse(txtQty.Text);

                lblTotal.Text = Currencies.CurrencyToString(dtotal, account.CurrencyCode);

                lblPrice.Text = Currencies.CurrencyToString(decimal.Parse(lblPrice.Text), account.CurrencyCode);
            }
            catch { }
        }

    }

    protected void btnChangeLocation_Click(object sender, EventArgs e)
    {
        this.InvisibleErrorMsg();

        int locationID = 0;
        int.TryParse(ddlSelectLocation.SelectedValue, out locationID);
        hfLocationID.Value = locationID.ToString();
        hfSelectedProductName.Value = radProductName.SelectedValue;

        string errorString = "";

        // skip calendar validation for IC Care Demo account with Instadose Elite demo order
        //if (account.BrandSourceID != 3 || radProductName.SelectedValue != "Instadose Elite Demo")
        if (!account.isDemoAccount || account.BrandSourceID != 3)
        {
            if (validateCalendar(account.AccountID, locationID, radProductName.SelectedValue, ref errorString) == false)
            {
                btnCreate.Enabled = false;
                this.VisibleErrorMsg(errorString);
            }
        }

        // if Instadose Plus & Intadose 2 is selected, skip rate validation. it will be handle by changing quantity of device from grid
        // DA added for ID 3 - to ask Thinh if correct
        if (radProductName.SelectedValue != "Instadose Plus" && radProductName.SelectedValue != "Instadose 3")
        {
            int productGroupID = radProductName.SelectedValue == "Instadose 1" ? 9 : 0;

            // set productGoupID as 2 for IC Care Demo Account to place ID Elite Demo order
            if (account.isDemoAccount && account.BrandSourceID == 3)
            {
                productGroupID = 2;
            }

            if (radProductName.SelectedValue.Equals(String.Empty)) 
            {
                productGroupID = Instadose3Constants.ProductGroupId;
            }

            if (validateRateByAccount(account, productGroupID, ref errorString) == false)
            {
                btnCreate.Enabled = false;
                this.VisibleErrorMsg(errorString);
            }
        }

        // Change the selected location
        setLocation(locationID);
        // Display a disire product tab
        visibleProductTab(hfSelectedProductName.Value);

        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID,
                "$('#locationSelectionDialog').dialog('close');", true);
    }

    protected void btnCancel_Click(object sender, EventArgs e)
    {
        if (account != null)
            Response.Redirect(string.Format("~/InformationFinder/Details/Account.aspx?ID={0}#Order_tab", account.AccountID));
        else
            Response.Redirect("~/InformationFinder");
    }

    protected void btnCreate_Click(object sender, EventArgs e)
    {
        if (account.CustomerTypeID == iConsignmentCustomerTypeID)  // consignment account
            if (this.lblErrorMessage.Text.Contains("maximum allowable"))
                return;   // not allow to continue


        // Clear the error label.
        InvisibleErrorMsg();

        if (!Page.IsValid) return;

        try
        {
            // Create a new order.
            int orderID = ProcessOrder();

            // Create RMA for Lost/Replace ID1 order                       
            if (orderID > 0 && (this.orderType == OrderTypes.Replacement || this.orderType == OrderTypes.NoReplacement || this.orderType == OrderTypes.NoChargeReplacement))
            {
                int rmaReturnID = ProcessRMA(this.orderType, orderID);
            }

            // Redirect to the order review screen.
            Response.Redirect(string.Format("~/CustomerService/ReviewOrder.aspx?ID={0}", orderID), false);
            Context.ApplicationInstance.CompleteRequest();
        }
        catch (Exception ex)
        {
            VisibleErrorMsg(ex.Message);
            // Basics.WriteLogEntry(ex.Message, Page.User.Identity.Name,
            //     "CreateOrder.aspx", Basics.MessageLogType.Notice);
        }
    }

    protected void ddlCountry_OnSelectedIndexChange(object sender, System.EventArgs e)
    {
        DropDownList ddlCountry = (DropDownList)sender;

        int countryID = 0;
        int.TryParse(ddlCountry.SelectedValue, out countryID);

        var states = (from s in idc.States where s.CountryID == countryID select s);

        if (ddlCountry == ddlShippingCountry)
        {
            ddlShippingState.DataSource = states;
            ddlShippingState.DataBind();
        }
        else
        {
            ddlBillingState.DataSource = states;
            ddlBillingState.DataBind();
        }
    }

    protected void ddlShippingMethod_SelectedIndexChanged(object sender, EventArgs e)
    {
        this.InvisibleErrorMsg();

        if (MalvernIntegration)   // Load the shipping method drop down for Malvern - hbabu 05/12/14. 
        {

            if (!ddlShippingMethodMalvern.SelectedItem.Text.Contains("Select"))
                this.lblShippingMethod.Text = ddlShippingMethodMalvern.SelectedItem.Text;

        }
        else
        {
            if (!ddlShippingMethod.SelectedItem.Text.Contains("Select"))
                this.lblShippingMethod.Text = ddlShippingMethod.SelectedItem.Text;
        }
        updateTotals();
    }

    protected void ddlBillingTerm_SelectedIndexChanged(object sender, EventArgs e)
    {
        updateProductGridPricing();
    }

    //protected void rblstPayMethod_SelectedIndexChanged(object sender, EventArgs e)
    //{
    //	loadPaymentSection();
    //}

    protected void rbtnReplacementBrokenClip_CheckedChanged(object sender, EventArgs e)
    {
        if (rbtnReplacementBrokenClip.Checked)
            changeOrderType(OrderTypes.NoChargeReplacement, true);
    }

    protected void rbtnReplacementLostBadge_CheckedChanged(object sender, EventArgs e)
    {
        if (rbtnReplacementLostBadge.Checked)
            changeOrderType(OrderTypes.Replacement, true);
    }

    protected void rbtnNoReplacement_CheckedChanged(object sender, EventArgs e)
    {
        if (rbtnNoReplacement.Checked)
            changeOrderType(OrderTypes.NoReplacement, true);
    }

    protected void txtQty_OnTextChanged(object sender, EventArgs e)
    {
        updateProductGridPricing();
    }

    protected void txtQtyInstadosePlus_OnTextChanged(object sender, EventArgs e)
    {
        TextBox txtQty = sender as TextBox;

        if (validateIDPlus2QtyInput(10, txtQty))
            updateProductGridPricing();
    }

    protected void txtQtyInstadose2_OnTextChanged(object sender, EventArgs e)
    {
        TextBox txtQty = sender as TextBox;

        if (validateIDPlus2QtyInput(11, txtQty))
            updateProductGridPricing();
    }

    protected void txtQtyInstadose3_OnTextChanged(object sender, EventArgs e)
    {
        TextBox txtQty = sender as TextBox;

        if (validateIDPlus2QtyInput(Instadose3Constants.ProductGroupId, txtQty))
            updateProductGridPricing();
    }

    protected void radProratePeriod_SelectedIndexChanged(object sender, EventArgs e)
    {
        // UPDATE & FORMAT the Product GridView with correct Pricing.
        this.updateProductGridPricing();

        lblProratePeriod.Text = radProratePeriod.SelectedItem.Text;
        lblOrderProratePeriod.Text = radProratePeriod.SelectedItem.Text;
    }

    private bool validateIDPlus2QtyInput(int productGroupID, TextBox qtyInput)
    {
        int qty = 0;
        string errMsg = string.Empty;

        if (qtyInput != null && !string.IsNullOrEmpty(qtyInput.Text) && int.TryParse(qtyInput.Text, out qty))
        {
            if (qty > 0 && !validateRateByAccount(account, productGroupID, ref errMsg))
            {
                btnCreate.Enabled = false;
                VisibleErrorMsg(errMsg);

                return false;
            }
        }

        return true;
    }

    public bool validateRateByAccountAndProductGroup(int AccountID, int pProductGroupID, ref string pErrorMsg)
    {
        var count = (from rd in idc.RateDetails
                     join a in idc.Accounts on rd.RateID equals a.DefaultRateID
                     where rd.ProductGroupID == pProductGroupID && rd.Active == true
                     && a.AccountID == AccountID
                     select rd).Count();

        if (count == 0)
        {
            var pg = (from g in idc.ProductGroups where g.ProductGroupID == pProductGroupID select g).FirstOrDefault();
            string productName = pg.ProductGroupName == "Instadose 2 New" ? "Instadose 2" : pg.ProductGroupName;
            pErrorMsg = "Require a rate code setup for a product: " + productName;

            return false;
        }

        return true;
    }

    protected void txtID2ConQty_OnTextChanged(object sender, EventArgs e)
    {
        //updateProductGridPricing();

        InvisibleMaxErrorMsg();
        //this.lblErrorMessage.Text = "";

        decimal unitPrice = 0;
        decimal total = 0;
        Boolean iok = true;
        //int totalProductCount = 0;

        // check consignment orders....
        foreach (GridViewRow rowitem in gvInstadose2Con.Rows)
        {
            string sProductsku = rowitem.Cells[2].Text.ToString();
            TextBox txtItemQty = (TextBox)rowitem.Cells[3].FindControl("txtID2ConQty");
            Label lblPrice = (Label)rowitem.Cells[4].FindControl("lblPrice");
            Label lblTotal = (Label)rowitem.Cells[5].FindControl("lblExtended");
            HiddenField hdProductID = (HiddenField)rowitem.Cells[1].FindControl("hfProductID");
            HiddenField hfProductGroupID = (HiddenField)rowitem.Cells[1].FindControl("hfProductGroupID");

            int iproductGroupID = 0;
            int.TryParse(hfProductGroupID.Value, out iproductGroupID);



            int itemQty = 0;
            if (!int.TryParse(txtItemQty.Text, out itemQty)) txtItemQty.Text = "";
            if (itemQty < 0)
            {
                itemQty = 0;
                txtItemQty.Text = "";
            }

            if (txtItemQty.Text == "")
                continue;

            decimal.TryParse(lblPrice.Text.Split(' ')[0], out unitPrice);
            total = itemQty * unitPrice;

            // id plus caps count for current editable record

            //// check active badges total
            //int activeBadgesCount = 0;
            //int.TryParse(lblActiveBadge.Text, out activeBadgesCount);

            // Set the total
            lblTotal.Text = Currencies.CurrencyToString(total, account.CurrencyCode);


            int iproductID = 0;
            int.TryParse(hdProductID.Value, out iproductID);
            //if (order == null)
            //    totalProductCount = getTotalProductCount(0, iproductID);
            //else
            //    totalProductCount = getTotalProductCount(order.OrderID, iproductID);
            //totalProductCount = totalProductCount + itemQty;   // include the current editable amount

            // check for rate code
            string errstr = "";
            if (!validateRateByAccountAndProductGroup(account.AccountID, iproductGroupID, ref errstr))
            {
                //VisibleErrorMsg(errstr);
                VisibleMaxErrorMsg(errstr);
                iok = false;
                this.btnCreate.Enabled = false;
                return;
            }

            // check for maximum amount
            if (sProductsku == "INSTA20" || sProductsku == "INSTA ID2")
            {
                // id plus device only, check quantity

                //var MaxIDPlusInfo = (from max in idc.ConsignmentInventoryLimits
                //                     where max.AccountID == this.account.AccountID && max.ProductGroupID == instadose2ProductGroupID   // 11 is idplus productgroupid
                //                     select max).FirstOrDefault();
                int iAvailable = getAvailableCount(this.account.AccountID, instadose2ProductGroupID);


                if (itemQty > iAvailable)
                {
                    this.VisibleMaxErrorMsg("Error: the total quantity for ID 2 Badges count of " + itemQty.ToString() + " is greater than the maximum allowable ID 2 count of " + iAvailable.ToString() + "!");
                    iok = false;
                    this.btnCreate.Enabled = false;
                    return;
                }
            }
            else
            {  // id plus accessories
                if (itemQty > iMaxAccessoriesCount)
                {
                    this.VisibleMaxErrorMsg("Error: the total quantity for ID 2 Accessories " + sProductsku + " count of " + itemQty.ToString() + " is greater than the maximum allowable ID 2 Accessories count of " + iMaxAccessoriesCount.ToString() + "!");
                    iok = false;
                    this.btnCreate.Enabled = false;
                    return;
                }

            }
        }   // end for loop

        // Recalculate totals and update the associated labels.
        this.btnCreate.Enabled = true;
        if (iok)
            updateTotals();

    }

    protected void txtQty_Caps_OnTextChanged(object sender, EventArgs e)
    {
        InvisibleMaxErrorMsg();
        //this.lblErrorMessage.Text = "";

        decimal unitPrice = 0;
        decimal total = 0;
        Boolean iok = true;
        //int totalProductCount = 0;
        int iAvailable = 0;

        // check consignment orders....
        foreach (GridViewRow rowitem in gvInstadosePlusCaps.Rows)
        {
            string sProductsku = rowitem.Cells[2].Text.ToString();
            TextBox txtItemQty = (TextBox)rowitem.Cells[2].FindControl("txtQty_Caps");
            Label lblPrice = (Label)rowitem.Cells[3].FindControl("lblPrice");
            Label lblTotal = (Label)rowitem.Cells[4].FindControl("lblTotal");
            HiddenField hdProductID = (HiddenField)rowitem.Cells[1].FindControl("hfProductID");

            int itemQty = 0;
            if (!int.TryParse(txtItemQty.Text, out itemQty)) txtItemQty.Text = "";
            if (itemQty < 0)
            {
                itemQty = 0;
                txtItemQty.Text = "";
            }

            decimal.TryParse(lblPrice.Text.Split(' ')[0], out unitPrice);
            total = itemQty * unitPrice;

            // id plus caps count for current editable record

            // check active badges total
            int activeBadgesCount = 0;
            int.TryParse(lblActiveBadge.Text, out activeBadgesCount);

            // Set the total
            lblTotal.Text = Currencies.CurrencyToString(total, account.CurrencyCode);


            int iproductID = 0;
            int.TryParse(hdProductID.Value, out iproductID);
            //if (order == null)
            //    totalProductCount = getTotalProductCount(0, iproductID);
            //else
            //    totalProductCount = getTotalProductCount(order.OrderID, iproductID);
            //totalProductCount = totalProductCount + itemQty;   // include the current editable amount

            // check for maximum amount
            if (sProductsku == "INSTA PLUS")
            {
                // id plus device only, check quantity

                var MaxIDPlusInfo = (from max in idc.ConsignmentInventoryLimits
                                     where max.AccountID == this.account.AccountID && max.ProductGroupID == 10   // 10 is idplus productgroupid
                                     select max).FirstOrDefault();
                if (MaxIDPlusInfo == null || MaxIDPlusInfo.MaxQuantity == 0)
                {
                    this.VisibleMaxErrorMsg("Error: please enter a number from the Max ID Plus Inventory Limit from the Account Setup - Billing Method Information.");
                    iok = false;
                    this.btnCreate.Enabled = false;
                    return;

                }
                else
                {
                    this.InvisibleMaxErrorMsg();
                }

                if (this.account.CustomerTypeID == iConsignmentCustomerTypeID)
                {

                    iAvailable = getAvailableCount(this.account.AccountID, groupIDPlus);

                    if (iAvailable < itemQty)
                    {
                        this.VisibleErrorMsg("Error: the total quantity for ID Plus Badges count of " + itemQty.ToString() + " is greater than the maximum allowable ID Plus count of " + iAvailable.ToString() + "!");
                        this.btnCreate.Enabled = false;
                        return;
                    }
                    else
                    {
                        this.InvisibleErrorMsg();
                    }


                }
            }
            else
            {  // id plus accessories
                if (itemQty > iMaxAccessoriesCount)
                {
                    this.VisibleMaxErrorMsg("Error: the total quantity for ID Plus Accessories " + sProductsku + " count of " + itemQty.ToString() + " is greater than the maximum allowable ID Plus Accessories count of " + iMaxAccessoriesCount.ToString() + "!");
                    iok = false;
                    this.btnCreate.Enabled = false;
                    return;
                }
                else
                {
                    this.InvisibleMaxErrorMsg();
                }

            }
        }   // end for loop

        // Recalculate totals and update the associated labels.
        this.btnCreate.Enabled = true;
        if (iok)
            updateTotals();
    }


    //protected int getTotalProductCount(int iorderID, int iproductID)
    //{
    //    // retrieve all the product of the same productID, except for the current order if edit order record
    //    var ocount = (from od in idc.OrderDetails join o in idc.Orders on od.OrderID equals o.OrderID where o.AccountID == account.AccountID  && od.OrderID != iorderID && od.ProductID == iproductID
    //                  join s in idc.OrderStatus on o.OrderStatusID equals s.OrderStatusID where s.OrderStatusName != "PACKED" && s.OrderStatusName !="SHIPPED" && s.OrderStatusName != "PICKED"
    //                  select new { od.Quantity }).AsEnumerable().Sum(l => l.Quantity);
    //    return ocount;
    //}

    protected int getAvailableCount(int accountid, int productgroupid)
    {
        // retrieve all the product of the same productID, except for the current order if edit order record
        var ocount = idc.fn_getConsignmentAccountAvailableInstaQuantities(accountid, productgroupid).GetValueOrDefault();
        return ocount;
    }

    protected int getProductGroupIDByProductID(int productID)
    {

        var productGroupID = (from p in idc.Products
                              where p.ProductID == productID
                              select p.ProductGroupID).FirstOrDefault();

        return productGroupID;
    }

    protected void txtQty_Accessories_OnTextChanged(object sender, EventArgs e)
    {
        decimal unitPrice = 0.00m;
        decimal total = 0.00m;
        Boolean iok = true;
        //int totalProductCount = 0;
        int iAvailable = 0;

        InvisibleMaxErrorMsg();
        //this.lblMaxErrorMessage.Text = "";

        if (account.CustomerTypeID == iConsignmentCustomerTypeID)
        {
            // check for max limit for consignment orders....
            foreach (GridViewRow rowitem in gvAccessories.Rows)
            {
                string sProductsku = rowitem.Cells[2].Text.ToString();
                TextBox txtItemQty = (TextBox)rowitem.Cells[3].FindControl("txtQty");
                Label lblPrice = (Label)rowitem.Cells[4].FindControl("lblPrice");
                Label lblTotal = (Label)rowitem.Cells[5].FindControl("lblTotal");
                HiddenField hdProductID = (HiddenField)rowitem.Cells[1].FindControl("hfProductID");

                int itemQty = 0;
                if (!int.TryParse(txtItemQty.Text, out itemQty)) txtItemQty.Text = "";
                if (itemQty < 0)
                {
                    itemQty = 0;
                    txtItemQty.Text = "";
                }

                decimal.TryParse(lblPrice.Text.Split(' ')[0], out unitPrice);
                total = itemQty * unitPrice;

                // id plus caps count for current editable record

                // check active badges total
                int activeBadgesCount = 0;
                int.TryParse(lblActiveBadge.Text, out activeBadgesCount);

                // Set the total
                //lblTotal.Text = Currencies.CurrencyToString(total, account.CurrencyCode.ToString(), account.CurrencyCode.ToString());


                int iproductID = 0;
                int.TryParse(hdProductID.Value, out iproductID);
                //if (order == null)
                //    totalProductCount = getTotalProductCount(0, iproductID);
                //else
                //    totalProductCount = getTotalProductCount(order.OrderID, iproductID);
                //totalProductCount = totalProductCount + itemQty;   // include the current editable amount

                // check for maximum amount
                // id plus device only, check quantity

                int igroupID = getProductGroupIDByProductID(iproductID);

                var MaxIDInfo = (from max in idc.ConsignmentInventoryLimits
                                 where max.AccountID == this.account.AccountID && (max.ProductGroupID == groupLink || max.ProductGroupID == groupUSB)  // 10 is idplus productgroupid
                                 select max).FirstOrDefault();
                if (MaxIDInfo == null || MaxIDInfo.MaxQuantity == 0)
                {
                    this.VisibleMaxErrorMsg("Error: Please set up Max Inventory limit number for InstaLink and InstaLinkUSB from the Account Setup - Billing Method Information!");
                    iok = false;
                    this.btnCreate.Enabled = false;
                    return;
                }
                else
                {
                    this.InvisibleErrorMsg();
                }

                iAvailable = getAvailableCount(this.account.AccountID, igroupID);

                if (iAvailable < itemQty)
                {
                    if (igroupID == groupLink)
                        this.VisibleMaxErrorMsg("Error: the total quantity for ID Link count of " + itemQty.ToString() + " is greater than the maximum allowable ID Link count of " + iAvailable.ToString() + "!");

                    if (igroupID == groupUSB)
                        this.VisibleMaxErrorMsg("Error: the total quantity for ID Link USB count of " + itemQty.ToString() + " is greater than the maximum allowable ID Link USE count of " + iAvailable.ToString() + "!");
                    iok = false;
                    this.btnCreate.Enabled = false;
                    return;
                }
                else
                {
                    this.InvisibleErrorMsg();
                }


            }  // end for loop
        } // end if statement

        string errorMsg = "";
        if (validateAccessoriesRateCodeByAccount(radProductName.SelectedValue, ref errorMsg) == false)
        {
            btnCreate.Enabled = false;
            this.VisibleMaxErrorMsg(errorMsg);
            return;
        }

        this.btnCreate.Enabled = true;
        updateProductGridPricing();
    }


    protected void txtQty_Renewal_OnTextChanged(object sender, EventArgs e)
    {
        InvisibleErrorMsg();

        decimal unitPrice = 0;
        decimal total = 0;
        Boolean iok = true;

        foreach (GridViewRow rowitem in gvRenewal.Rows)
        {
            // Get each line item's grid row and update the price
            HiddenField productGroupID = (HiddenField)rowitem.Cells[1].FindControl("hfProductGroupID");


            // Get the price per line.
            //if (int.TryParse(productGroupID.Value, out tmpInt)) unitPrice = getUnitPrice(tmpInt);

            TextBox txtItemQty = (TextBox)rowitem.Cells[2].FindControl("txtQty");
            Label lblPrice = (Label)rowitem.Cells[3].FindControl("lblPrice");
            Label lblTotal = (Label)rowitem.Cells[4].FindControl("lblTotal");

            int itemQty = 0;
            if (!int.TryParse(txtItemQty.Text, out itemQty)) txtItemQty.Text = "";
            if (itemQty < 0)
            {
                itemQty = 0;
                txtItemQty.Text = "";
            }

            decimal.TryParse(lblPrice.Text.Split(' ')[0], out unitPrice);

            total = itemQty * unitPrice;

            // check active badges total
            int activeBadgesCount = 0;
            int.TryParse(lblActiveBadge.Text, out activeBadgesCount);

            if (itemQty > activeBadgesCount)
            {
                this.VisibleErrorMsg("Error: the quantity amount is greater than the current active Badges total!");

                iok = false;
                break;

            }


            // Set the total
            lblTotal.Text = Currencies.CurrencyToString(total, account.CurrencyCode);

        }   // for loop


        // Recalculate totals and update the associated labels.
        if (iok)
            updateTotals();
        //updateOrderSummary();



    }



    protected void txtTransitionServiceStart_TextChanged(object sender, EventArgs e)
    {
        updateProductGridPricing();
    }

    protected void gvOrderDetails_SelectedIndexChanged(object sender, EventArgs e)
    {
    }

    protected void txtSpecialInstruction_TextChanged(object sender, EventArgs e)
    {
        lblSpecialInstructions.Text = txtSpecialInstruction.Text;
    }

    protected void ddlPackageType_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (!ddlPackageType.SelectedItem.Text.Contains("Select"))
            lblPackageType.Text = ddlPackageType.SelectedItem.Text;
    }

    protected void ddlShippingOption_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (!ddlShippingOption.SelectedItem.Text.Contains("Shipping Option"))
            lblShippingOption.Text = ddlShippingOption.SelectedItem.Text;
    }

    protected void valServicePeriod_ServerValidate(object source, ServerValidateEventArgs args)
    {
        if (account.UseLocationBilling && location.ContractEndDate.HasValue)
            args.IsValid = location.ContractEndDate.Value > DateTime.Now;
        else if (!account.UseLocationBilling && account.ContractEndDate.HasValue)
        {
            if (order != null && order.OrderType == "RENEWAL" && account.BrandSourceID == 3)
                args.IsValid = true;
            else
                args.IsValid = account.ContractEndDate.Value > DateTime.Now;
        }
        else
            args.IsValid = false;
    }

    protected void gvDiscounts_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType != DataControlRowType.DataRow) return;
        // Get each line item's grid row and update the price
        HiddenField hfProductGroupID = (HiddenField)e.Row.FindControl("hfProductGroupID");
        int productGroupID = int.Parse(hfProductGroupID.Value);
        decimal targetPrice = getTargetPrice(productGroupID);
        e.Row.Cells[2].Text = Currencies.CurrencyToString(targetPrice, account.CurrencyCode);
    }

    #endregion

    #region Load and Fill Controls

    /// <summary>
    /// On page init, load the querystring params and store them on the page.
    /// </summary>
    private void setPageData()
    {
        // Load the account ID from query string
        if (Request.QueryString["ID"] != null)
            hfAccountID.Value = Request.QueryString["ID"];

        // Load the location ID from query string
        if (Request.QueryString["LocationID"] != null)
            hfLocationID.Value = Request.QueryString["LocationID"];

        // Load the order ID from query string
        if (Request.QueryString["OrderID"] != null)
        {
            hfOrderID.Value = Request.QueryString["OrderID"];
        }

        // Load the order type from query string
        if (Request.QueryString["OrderType"] != null)
        {
            var o = OrderTypes.New;
            switch (Request.QueryString["OrderType"].ToLower())
            {
                // Replacement order for broken or lost badges.
                case "new":
                case "addon":
                    o = OrderTypes.New;
                    break;
                case "lostdamaged":
                case "replacement":
                    o = OrderTypes.Replacement;
                    break;
                case "nochargereplacement":
                case "freereplacement":
                    o = OrderTypes.NoChargeReplacement;
                    break;
                case "noreplacement":
                    o = OrderTypes.NoReplacement;
                    break;
            }
            hfOrderType.Value = o.ToString();
        }
    }

    public AccountCreditCard GetAccountCreditCard(Account acct)
    {
        // Load Account Credit Card information, check if CC is current/valid then Enable/Disable rblstPayMethod List Items accordingly.
        int currentMonth = 0;
        int currentYear = 0;
        currentMonth = Convert.ToInt32(DateTime.Now.Month);
        currentYear = Convert.ToInt32(DateTime.Now.Year);

        AccountCreditCard acctcc = (from acc in acct.AccountCreditCards
                                    where acc.Active == true
                                    && (acc.UseOnce == false || acc.UseOnce == (bool?)null)
                                    && ((acc.ExpYear > currentYear) || (acc.ExpYear == currentYear && acc.ExpMonth >= currentMonth))
                                    orderby acc.AccountCreditCardID descending
                                    select acc).FirstOrDefault();

        return acctcc;
    }

    //public void ShowHideCreditCardInformation(Account acct)
    //{
    //	accountcreditcard = GetAccountCreditCard(acct);

    //	if (accountcreditcard == null)
    //	{
    //		//this.rblstPayMethod.Items[0].Enabled = false;
    //		//this.rblstPayMethod.Items[1].Enabled = true;
    //		this.lblUpdateCCInformation.Visible = true;
    //		this.lblUpdateCCInformation.Text = "This Account's Billing Method is 'Credit Card', please update Credit Card information before continuing with creating this Order.";
    //	}
    //	else
    //	{
    //		//this.rblstPayMethod.Items[0].Enabled = true;
    //		//this.rblstPayMethod.Items[1].Enabled = true;
    //		this.lblUpdateCCInformation.Visible = false;
    //		this.lblUpdateCCInformation.Text = string.Empty;
    //	}
    //}

    /// <summary>
    /// Load all of the nessesary data, order, account, location, order type
    /// Added on 03/04/2015 by Anuradha Nandi, get data for account credit card on page load.
    /// </summary>
    private void loadPageData()
    {
        // Load the account ID for a new order
        if (hfOrderID.Value == "0")
        {
            int accountID = 0;
            int.TryParse(hfAccountID.Value, out accountID);

            account = (from a in idc.Accounts where a.AccountID == accountID select a).FirstOrDefault();

            // Ensure the account ID was selected
            if (account == null) throw new Exception("The account was not supplied.");

            // Ensure the account is active.
            if (!account.Active) throw new Exception("The account is inactive, and cannot place orders.");

            //if (account.BillingMethod == "Credit Card")
            //{
            //	this.ShowHideCreditCardInformation(account);
            //}

            // Load the location from query string if provided
            if (hfLocationID.Value != "0")
            {
                int locationID = 0;
                int.TryParse(hfLocationID.Value, out locationID);
                location = account.Locations.Where(l => l.LocationID == locationID).FirstOrDefault();
                if (location == null)
                    throw new Exception("The location ID provided does not exist for this account.");
            }
            else
            {
                // Set the location to the account default unless a location was already set.
                displayChangeLocation = true;
                int locationID = 0;
                int.TryParse(hfLocationID.Value, out locationID);
                if (locationID == 0)
                {
                    locationID = (from l in account.Locations where l.IsDefaultLocation select l.LocationID).FirstOrDefault();
                    hfLocationID.Value = locationID.ToString();
                }

                // Select the default location by default.
                location = account.Locations.Where(l => l.LocationID == locationID).FirstOrDefault();
            }

            //Is rate code active and current?
            if (!account.DefaultRateID.HasValue)
                throw new Exception("This account does not have a rate code.");

            if (account.Rate == null)
                throw new Exception("The rate code does not exist.");

            if (account.Rate.ExpirationDate < DateTime.Now || !account.Rate.Active)
                throw new Exception("The rate code has expired or is no longer valid.");

            // Convert the order type to the variable.
            orderType = (OrderTypes)Enum.Parse(typeof(OrderTypes), hfOrderType.Value);
        }
        // Load the information for an existing order in case we want to modify the order
        // order should has orderstatus in {CREATED - Needs PO, CREATED - NO PO}        
        // We will not be able to modify the replacement order, no replacement order, no charge replacement order since the order status = FULLFILLMENT
        else if (hfOrderID.Value != "0")
        {
            int orderID = 0;
            int.TryParse(hfOrderID.Value, out orderID);

            order = idc.Orders.Where(o => o.OrderID == orderID && o.OrderStatusID != 10).FirstOrDefault();
            if (order == null) throw new Exception("The order does not exist.");

            // Query the account
            account = order.Account;
            location = order.Location;

            IdentifyOrderProductName(order);

            // Display a disire product tab
            visibleProductTab(hfSelectedProductName.Value);
        }
        else
        {
            throw new Exception("No order or account information was provided.");
        }

        // At this point, location, account and possibly order is not null. order is null if creating an order      

        if (account.BrandSource.BrandName != "IC Care")
        {
            if (orderType == OrderTypes.New || orderType == OrderTypes.Replacement)
            {
                if ((order == null && account.Orders.Count > 0) || (order != null && account.Orders.Count > 1))
                    this.isNoneICCareAddOn = true;
            }
        }
        else if (orderType == OrderTypes.Replacement)
        {
            if ((order == null && account.Orders.Count > 0) || (order != null && account.Orders.Count > 1))
                this.isNoneICCareAddOn = true;
        }

        if (orderType == OrderTypes.New)
        {
            if ((order == null && account.Orders.Count == 0) || (order != null && account.Orders.Count == 1))
                isFirstOrder = true;
        }
    }

    private void IdentifyOrderProductName(Order pOrder)
    {
        int instadose1Count = 0, instadose2Count = 0, instadosePlusCount = 0, instadose1RenewalCount = 0, instadosePlusCapsCount = 0, instadose2BumpCount = 0;
        int instadose3Count = 0;

        foreach (var orderDetail in pOrder.OrderDetails)
        {
            if (orderDetail.Product.ProductSKU == "INSTA10-B"
                || orderDetail.Product.ProductSKU == "INSTA10"
                || orderDetail.Product.ProductSKU == "INSTA10BLU"
                || orderDetail.Product.ProductSKU == "INSTA10BLK"
                || orderDetail.Product.ProductSKU == "INSTA10SLV"
                || orderDetail.Product.ProductSKU == "INSTA10GRN"
                || orderDetail.Product.ProductSKU == "INSTA10PNK") instadose1Count++;

            if (orderDetail.Product.ProductSKU == "INSTA20" ||
                orderDetail.Product.ProductSKU == "INSTA ID2" ||
                orderDetail.Product.ProductSKU == "LABELROLLID2" ||
                orderDetail.Product.ProductSKU == "LABELSHEETID2") instadose2Count++;

            if (orderDetail.Product.ProductSKU.IndexOf("BUMP") > -1) instadose2BumpCount++;

            if (orderDetail.Product.ProductSKU == "INSTA PLUS" ||
                orderDetail.Product.ProductSKU == "LABELROLLID+" ||
                orderDetail.Product.ProductSKU == "LABELSHEETID+") instadosePlusCount++;

            if (orderDetail.Product.ProductSKU.IndexOf("COLOR CAP") > -1) instadosePlusCapsCount++;

            if (orderDetail.Product.ProductSKU.IndexOf("RENEWAL") > -1) instadose1RenewalCount++;

            if (orderDetail.Product.ProductSKU.IndexOf("ID3") > -1) instadose3Count++;
        }


        if (instadose1Count == 0 && instadose2Count == 0 && instadosePlusCount == 0 && instadose1RenewalCount == 0 && instadosePlusCapsCount == 0 && instadose2BumpCount == 0)
            throw new Exception("Can not modify this order. It is in fulfillment or it is a lost replacement order.");

        if (instadose1Count > 0)
            hfSelectedProductName.Value = "Instadose 1";
        if (instadosePlusCount > 0 || instadose2Count > 0)
            hfSelectedProductName.Value = "Instadose Plus";
        if (instadosePlusCapsCount > 0 && account.CustomerTypeID == iConsignmentCustomerTypeID)  // consignment account order
            hfSelectedProductName.Value = "Instadose Plus";
        if (instadose2BumpCount > 0 && account.CustomerTypeID == iConsignmentCustomerTypeID)  // consignment account order
            hfSelectedProductName.Value = "Instadose Plus";
        if (instadose1RenewalCount > 0 && account.BrandSourceID == 3)  // ic care only
            hfSelectedProductName.Value = "Renewal";
        if (instadose3Count > 0)
            hfSelectedProductName.Value = Instadose3Constants.ProductName;

        //if (account.BrandSourceID == 3 && order.OrderType == "DEMO")
        //    hfSelectedProductName.Value = "Instadose Elite Demo";
    }

    private void LoadProratePeriodRadioButton()
    {
        DateTime orderCreatedDate = order == null ? DateTime.Now : order.OrderDate;
        DateTime startQuarterDate, nextQuarterDate;
        int qtrNo = Common.calculateNumberOfQuarterService(GetContractStartDate(), GetContractEndDate(), orderCreatedDate, out startQuarterDate);
        nextQuarterDate = startQuarterDate.AddMonths(3);

        if (account.BillingTermID == 2) // yearly
        {
            radProratePeriod.Items.Add(new ListItem(string.Format("{0:MM/dd/yyyy}", startQuarterDate) + " - " + string.Format("{0:MM/dd/yyyy}", GetContractEndDate()), startQuarterDate.ToShortDateString()));
            if (nextQuarterDate < GetContractEndDate())
            {
                radProratePeriod.Items.Add(new ListItem(string.Format("{0:MM/dd/yyyy}", nextQuarterDate) + " - " + string.Format("{0:MM/dd/yyyy}", GetContractEndDate()), nextQuarterDate.ToShortDateString()));
                radProratePeriod.Items[1].Enabled = false;
            }
        }
        else // quarterly
        {
            radProratePeriod.Items.Add(new ListItem(string.Format("{0:MM/dd/yyyy}", startQuarterDate) + " - " + string.Format("{0:MM/dd/yyyy}", startQuarterDate.AddMonths(3).AddDays(-1)), startQuarterDate.ToShortDateString()));
            if (nextQuarterDate < GetContractEndDate())
            {
                radProratePeriod.Items.Add(new ListItem(string.Format("{0:MM/dd/yyyy}", nextQuarterDate) + " - " + string.Format("{0:MM/dd/yyyy}", nextQuarterDate.AddMonths(3).AddDays(-1)), nextQuarterDate.ToShortDateString()));
                radProratePeriod.Items[1].Enabled = false;
            }
        }

        if (nextQuarterDate < GetContractEndDate() && nextQuarterDate.Subtract(orderCreatedDate).Days < 30)
        {
            radProratePeriod.Items[1].Enabled = true;
        }

        radProratePeriod.SelectedIndex = 0;

        if (order != null)
        {
            ListItem item = radProratePeriod.Items.FindByValue(order.StxContractStartDate.HasValue ? order.StxContractStartDate.Value.ToShortDateString() : DateTime.Now.ToShortDateString());
            if (item != null)
                radProratePeriod.SelectedIndex = radProratePeriod.Items.IndexOf(item);
        }
    }

    /// <summary>
    /// Called on page load when controls need to be populated with pre-filled data.
    /// </summary>
    private void fillControls()
    {
        // Tdo 09/21/2017. With the new design of ID+ LOST/REPLACEMENT order, the lost/replacement order for ID+ will not be created in Create Order page. 
        // Only ID1 LOST/REPLACEMENT order is created over here.
        if (this.orderType == OrderTypes.Replacement)
        {
            radProductName.Items.FindByValue("Instadose Plus").Enabled = false;
            //radProductName.Items.FindByValue("Instadose Elite Demo").Enabled = false;

            // DA added for ID3 - to task Thinh
            radProductName.Items.FindByValue("Instadose 3").Enabled = false;

            // Display serial number for Lost/Replacement ID1 order
            GenerateSerialNoCheckBoxList();
        }

        if (this.isNoneICCareAddOn)
        {
            LoadProratePeriodRadioButton();
            SelectedProratePeriod.Visible = true;

            lblProratePeriod.Text = radProratePeriod.SelectedItem.Text;
            lblOrderProratePeriod.Text = radProratePeriod.SelectedItem.Text;
            pnlProratePeriod.Visible = true;
        }

        btnCreate.Enabled = true;
        // If no location was explicity provided and this order hasn't been created
        if (displayChangeLocation)
        {
            // Show the location select dialog window
            ClientScript.RegisterStartupScript(GetType(), "od",
                "$(function() { $('#locationSelectionDialog').dialog('open'); });", true);
        }

        //// Check to see if this accounts has a rate code for Instadose Plus products.
        //int[] productIDs = new int[3] { 2, 5, 6};
        //var hasProductIDs = (from rd in account.Rate.RateDetails
        //                     where rd.Active && productIDs.Contains(rd.ProductGroupID)
        //                     select rd.ProductGroupID).ToArray();

        //// Does this account have any of these?
        //if (hasProductIDs.Count() <= 0)
        //{
        //    tabInstaPlus.Visible = false;
        //    gvInstadosePlus.Visible = false;            
        //    lblInstaPlusTotal.Visible = false;
        //    gvAccessories.Visible = false;
        //    lblAccessTotal.Visible = false;
        //}        


        ddlSelectLocation.DataSource = (from a in account.Locations
                                        orderby a.IsDefaultLocation descending, a.LocationName
                                        select new { a.LocationID, a.LocationName });
        ddlSelectLocation.DataBind();
        if (location == null)
            throw new Exception("Default location is not set.");
        ddlSelectLocation.SelectedValue = location.LocationID.ToString();

        // Load the BodyRegion drop down
        ddlSetBodyRegion.DataSource = (from a in idc.BodyRegions select a);
        ddlSetBodyRegion.DataBind();

        // Load the billing drop down
        ddlBillingCountry.DataSource = (from a in idc.Countries orderby a.CountryName select a);
        ddlBillingCountry.DataBind();
        ddlBillingCountry.Items.Insert(0, new ListItem("-- Select Country --", "0"));
        ddlBillingCountry.SelectedIndex = 0;

        // Load the shipping drop down
        ddlShippingCountry.DataSource = (from a in idc.Countries orderby a.CountryName select a);
        ddlShippingCountry.DataBind();
        ddlShippingCountry.Items.Insert(0, new ListItem("-- Select Country --", "0"));
        ddlShippingCountry.SelectedIndex = 0;

        ddlBillingState.DataSource = (from a in idc.States orderby a.CountryID, a.StateAbbrev select a);
        ddlBillingState.DataBind();

        ddlShippingState.DataSource = (from a in idc.States orderby a.CountryID, a.StateAbbrev select a);
        ddlShippingState.DataBind();

        //// Load the credit card Year drop down.
        //ddlCCYear.Items.Add(new ListItem("Year", "0"));
        //for (int i = 0; i < 7; i++)
        //	ddlCCYear.Items.Add(new ListItem((DateTime.Now.Year + i).ToString()));

        // Load the referrals drop down.
        ddlReferral.DataSource = (from s in idc.SalesRepDistributors
                                  orderby s.SalesRepDistID
                                  select new
                                  {
                                      s.SalesRepDistID,
                                      SalesCompanyName = string.Format("{0} - {1}", s.SalesRepDistID, s.CompanyName)
                                  });
        ddlReferral.DataBind();
        ddlReferral.Items.Insert(0, new ListItem("-- Select Referral --", "0"));

        // Load the currencies drop down
        ddlCurrency.DataSource = (from c in idc.Currencies orderby c.CurrencyCode select c);
        ddlCurrency.DataBind();
        ddlCurrency.SelectedValue = "USD";

        // Load package types.
        ddlPackageType.DataSource = (from c in idc.PackageTypes orderby c.PackageDesc select c);
        ddlPackageType.DataBind();
        ddlPackageType.Items.Insert(0, new ListItem("-- Select Package Type --", "0"));
        ddlPackageType.SelectedIndex = 0;

        // Load the shipping option drop down.
        ddlShippingOption.DataSource = (from c in idc.ShippingOptions
                                        orderby c.ShippingOptionDesc
                                        select c);
        ddlShippingOption.DataBind();
        ddlShippingOption.Items.Insert(0, new ListItem("-- Shipping Option --", "0"));
        ddlShippingOption.SelectedIndex = 0;

        // Load the shipping method drop down.

        /* ddlShippingMethod.DataSource = (from a in idc.ShippingMethods
                                         where a.ShippingMethodDesc != "Renewal"
                                         orderby a.ShippingMethodDesc
                                         select a);
         ddlShippingMethod.DataBind();
         ddlShippingMethod.Items.Insert(0, new ListItem("-- Select Shipping Method --", "0"));
         ddlShippingMethod.SelectedIndex = 0;
         */

        if (MalvernIntegration)   // Load the shipping method drop down for Malvern - hbabu 05/12/14.
        {

            //Shipping International or Domestic
            int international = 0;
            // if (ddlShippingCountry.SelectedItem.Text != "United States")
            if (location.ShippingCountry.CountryName != "United States")
            {
                international = 1;
            }

            //Assign Preffered Shipping carrier   - Hbabu 04/22/14
            defaultShippingCarrier = (from a in idc.Accounts where a.AccountID == this.account.AccountID select a.ShippingCarrier).FirstOrDefault();
            if (!string.IsNullOrEmpty(defaultShippingCarrier))
            {
                ddShippingCarrier.Items.Insert(0, new ListItem(defaultShippingCarrier, "0"));
                ddShippingCarrier.SelectedIndex = 0;
            }
            else
            {
                ddShippingCarrier.Items.Insert(0, new ListItem("N/A", "0"));
                ddShippingCarrier.SelectedIndex = 0;
            }


            // Load the shipping method drop down.  Based on the Perfered customer  - Hbabu 04/22/14
            if (defaultShippingCarrier != null && defaultShippingCarrier.Length > 0)
            {

                var sMethods = from r in idc.ShippingMethods
                               where r.CarrierDesc == defaultShippingCarrier && r.InternationalShipping == international
                               orderby r.ShippingMethodDesc ascending
                               select new
                               {
                                   ShippingMethodID = r.ShippingMethodID,
                                   ShippingMethodDesc = r.ShippingMethodDesc + " " + r.ShippingMethodDetails

                               };

                ddlShippingMethodMalvern.DataSource = sMethods;
                ddlShippingMethodMalvern.DataTextField = "ShippingMethodDesc";
                ddlShippingMethodMalvern.DataValueField = "ShippingMethodID";
                ddlShippingMethodMalvern.DataBind();


                /*ddlShippingMethodMalvern.DataSource = (from a in idc.VI_GetShippingMethods
													   where a.CarrierDesc == defaultShippingCarrier
													   orderby a.ShippingMethodDesc
													   select a);
				 * */
            }
            else
            {
                var sMethods = from r in idc.ShippingMethods
                               where r.PrefShipping == 0 && r.InternationalShipping == international
                               orderby r.ShippingMethodDesc ascending
                               select new
                               {
                                   ShippingMethodID = r.ShippingMethodID,
                                   ShippingMethodDesc = r.ShippingMethodDesc + " " + r.ShippingMethodDetails

                               };

                ddlShippingMethodMalvern.DataSource = sMethods;
                ddlShippingMethodMalvern.DataTextField = "ShippingMethodDesc";
                ddlShippingMethodMalvern.DataValueField = "ShippingMethodID";
                ddlShippingMethodMalvern.DataBind();


                /*ddlShippingMethodMalvern.DataSource = (from a in idc.VI_GetShippingMethods
													   where a.PrefShipping == 0
													   orderby a.ShippingMethodDesc
													   select a);*/

            }


            ddlShippingMethodMalvern.DataBind();
            ddlShippingMethodMalvern.Items.Insert(0, new ListItem("-- Select Shipping Method --", "0"));
            ddlShippingMethodMalvern.SelectedIndex = 0;

        }
        else if (hfSelectedProductName.Value == "Renewal" && account.BrandSourceID == 3)  //ic care
        {
            ddlShippingMethod.DataSource = (from a in idc.ShippingMethods
                                            where a.PrefShipping == null && a.ShippingMethodDesc == "Free Shipping"
                                            orderby a.ShippingMethodDesc
                                            select a);
            ddlShippingMethod.DataBind();
            //ddlShippingMethod.Items.Insert(0, new ListItem("-- Select Shipping Method --", "0"));
            //ddlShippingMethod.SelectedIndex = 0;

        }
        else // Load the shipping method drop down for Fedex.
        {
            ddlShippingMethod.DataSource = (from a in idc.ShippingMethods
                                            where a.PrefShipping == null && a.ShippingMethodDesc != "Renewal"
                                            orderby a.ShippingMethodDesc
                                            select a);
            ddlShippingMethod.DataBind();
            ddlShippingMethod.Items.Insert(0, new ListItem("-- Select Shipping Method --", "0"));
            ddlShippingMethod.SelectedIndex = 0;
        }


        ddlCoupon.DataBind();
        ddlCoupon.Items.Insert(0, new ListItem("-- Select a Coupon --", "0"));
        ddlCoupon.SelectedIndex = 0;

        //load discounts
        gvDiscounts.DataSource = (from ad in account.AccountDiscounts
                                  where ad.Active
                                  select new
                                  {
                                      ad.ProductGroupID,
                                      ProductGroupName =
                                        (from pg in idc.ProductGroups
                                         where pg.ProductGroupID == ad.ProductGroupID
                                         select pg.ProductGroupName).FirstOrDefault(),
                                      ad.Discount
                                  });
        gvDiscounts.DataBind();

        // Load account specific data.
        lblAccountID2.Text = account.AccountID.ToString();
        lblAccountName2.Text = account.AccountName;
        lblAccountID.Text = account.AccountID.ToString();
        lblAccountName.Text = account.AccountName;
        txtAccountID.Text = account.AccountID.ToString();
        txtAccountName.Text = account.AccountName;

        lblCreatedDate.Text = DateTime.Today.ToShortDateString();

        // Grab the account and product info
        int activeBadgesCount = (from a in idc.AccountDevices where a.Active && a.AccountID == account.AccountID select a).Count();
        lblActiveBadge.Text = string.Format("{0:#,##0}", activeBadgesCount);

        txtRateCode.Text = (account.Rate == null) ? "" : account.Rate.RateCode;
        lblRateCode.Text = txtRateCode.Text;

        // Set the referral code.
        //rblstPayMethod.SelectedValue = account.BillingMethod;
        ddlReferral.SelectedValue = account.ReferralCode;

        if (ddlReferral.SelectedValue != "")
            this.lblReferralCode.Text = ddlReferral.SelectedItem.Text;

        this.lblOrderBillingTerm.Text = lblBillingTerm.Text = account.BillingTermID.Value == 1 ? "Quarterly" : "Yearly";
        ddlCurrency.SelectedValue = account.CurrencyCode;

        ddlPackageType.SelectedValue = (account.PackageTypeID != null) ? account.PackageTypeID.ToString() : "0";

        if (!ddlPackageType.SelectedItem.Text.Contains("Select"))
            this.lblPackageType.Text = ddlPackageType.SelectedItem.Text;

        ddlShippingOption.SelectedValue = (account.ShippingOptionID != null) ? account.ShippingOptionID.ToString() : "0";

        if (ddlShippingOption.SelectedIndex != 0)
            this.lblShippingOption.Text = ddlShippingOption.SelectedItem.Text;

        this.lblSpecialInstructions.Text = txtSpecialInstruction.Text = account.SpecialInstructions;

        // Load the billing and shipping address
        fillLocationControl();

        // Load products after the billing address because triggering the load products also redoes totals and shipping.
        loadProductsGrid();

        // Load the payments section controls
        loadPaymentSection();

        // Disable billing first last name validation.
        if (account.BrandSourceID.Value == 3)
        {
            RequiredFieldValidatorFirstNameB.Enabled = false;
            RequiredFieldValidatorLastNameB.Enabled = false;

            ddlReferral.SelectedValue = "D012";
            ddlReferral.Enabled = false;

            ddlCurrency.SelectedValue = "USD";
            ddlCurrency.Enabled = false;

            //rblstPayMethod.Enabled = false;
            ddlReferral.Enabled = false;
            ddlCurrency.Enabled = false;
            ddlCoupon.Enabled = false;
        }

        // Business logic: If a Mirion Transition order... display
        // transition start period, and use that as service period start date
        // only if the account has NO orders on the account.
        // Modified by TDO, 06/26/2017
        if (account.BrandSourceID.Value == 2 && account.CustomerType.CustomerTypeDesc == "Transition" && account.Orders.Count == 0)
        {
            // Modified by TDO, 06/27/2017.
            // Do not alllow to manually set transaction period start in the order any more by Craig.
            // pnlTransitionServicePeriod.Visible = true;			
            txtTransitionServiceStart.Text = string.Format("{0:MM/dd/yyyy}", GetContractStartDate());
            lblTransitionServiceEnd.Text = string.Format("{0:MM/dd/yyyy}", GetContractEndDate());
        }

        // When creating a new order.
        if (order == null)
        {
            //8/2013 WK - If IC Care and "ADDON" order, then set PO Number to default ("NEED PO").
            // Identify the addon order as anything after the first order.
            if (account.BrandSourceID == 3 && account.Orders.Count >= 1)
                txtPOno.Text = "NEED PO";
            else
                txtPOno.Text = (account.RenewalPONumber == null) ? "" : account.RenewalPONumber;

        }
        else if (order.OrderStatusID < 3) // If the order already exists and hasn't been released.
        {
            btnCreate.Text = "Update Order";

            // Referral Value in order
            ddlReferral.SelectedValue = order.ReferralCode.ToString();
            txtPOno.Text = (order.PONumber == null) ? "" : order.PONumber;

            // Coupon Value in order
            if (order.CouponID != null) ddlCoupon.SelectedValue = order.CouponID.ToString();

            // Shipping Method
            if (MalvernIntegration)   // Load the shipping method drop down for Malvern - hbabu 05/12/14. 
            {
                ddlShippingMethodMalvern.SelectedValue = order.ShippingMethodID.ToString();
                lblShippingCharge.Text = order.ShippingCharge.ToString();

                if (!ddlShippingMethodMalvern.SelectedItem.Text.Contains("Select"))
                    this.lblShippingMethod.Text = ddlShippingMethodMalvern.SelectedItem.Text;
            }
            else
            {
                ddlShippingMethod.SelectedValue = order.ShippingMethodID.ToString();
                lblShippingCharge.Text = order.ShippingCharge.ToString();

                if (!ddlShippingMethod.SelectedItem.Text.Contains("Select"))
                    this.lblShippingMethod.Text = ddlShippingMethod.SelectedItem.Text;
            }
            // load label, package, shipping and instruction value
            ddlPackageType.SelectedValue = (order.PackageTypeID != null) ? order.PackageTypeID.ToString() : "0";
            ddlShippingOption.SelectedValue = (order.ShippingOptionID != null) ? order.ShippingOptionID.ToString() : "0";
            txtSpecialInstruction.Text = order.SpecialInstructions;

            // Fill the all Products grids with the correct quantities.
            foreach (var orderDetail in order.OrderDetails)
            {
                foreach (GridViewRow row in gvProduct.Rows)
                {
                    //Instadose 1 products.  Original order quantities, if any.
                    // If they do not match skus continue...
                    if (orderDetail.Product.ProductSKU != row.Cells[2].Text) continue;

                    TextBox txtQty = (TextBox)(row.Cells[2].FindControl("txtQty"));
                    txtQty.Text = orderDetail.Quantity.ToString();

                    Label lblFindPrice = (Label)(row.Cells[4].FindControl("lblPrice"));
                    lblFindPrice.Text = Currencies.CurrencyToString(orderDetail.Price, account.CurrencyCode);
                }


                // check for consignment orders.....
                //var vidplusOrderCount = order.OrderDetails.Where(d => d.OrderID == orderDetail.OrderID && d.ProductID == 30).Count();   // 30 is the idplus subscription product id

                if (account.CustomerTypeID == iConsignmentCustomerTypeID)
                {  // consignment orders
                    foreach (GridViewRow row in gvInstadosePlusCaps.Rows)
                    {
                        HiddenField hidProductID = (HiddenField)row.FindControl("hfProductID");
                        int tmpInt = 0;

                        // If ProductIDs do not match then continue (Instadose Plus has all same SKUs)
                        if (int.TryParse(hidProductID.Value, out tmpInt) &
                           (orderDetail.ProductID != tmpInt)) continue;

                        TextBox txtQty = (TextBox)(row.Cells[3].FindControl("txtQty_Caps"));
                        txtQty.Text = orderDetail.Quantity.ToString();

                        Label lblFindPrice = (Label)(row.Cells[4].FindControl("lblPrice"));
                        lblFindPrice.Text = Currencies.CurrencyToString(orderDetail.Price, account.CurrencyCode);
                    }


                    foreach (GridViewRow row in gvInstadose2Con.Rows)
                    {
                        HiddenField hidProductID = (HiddenField)row.FindControl("hfProductID");
                        int tmpInt = 0;

                        // If ProductIDs do not match then continue (Instadose Plus has all same SKUs)
                        if (int.TryParse(hidProductID.Value, out tmpInt) &
                           (orderDetail.ProductID != tmpInt)) continue;

                        TextBox txtQty = (TextBox)(row.Cells[3].FindControl("txtID2ConQty_Caps"));
                        txtQty.Text = orderDetail.Quantity.ToString();

                        Label lblFindPrice = (Label)(row.Cells[4].FindControl("lblPrice"));
                        lblFindPrice.Text = Currencies.CurrencyToString(orderDetail.Price, account.CurrencyCode);

                    }
                }
                else
                {
                    //Instadose Plus products.  Original order quantities, if any.
                    foreach (GridViewRow row in gvInstadosePlus.Rows)
                    {
                        HiddenField hidProductID = (HiddenField)row.FindControl("hfProductID");
                        int tmpInt = 0;

                        // If ProductIDs do not match then continue (Instadose Plus has all same SKUs)
                        if (int.TryParse(hidProductID.Value, out tmpInt) &
                           (orderDetail.ProductID != tmpInt)) continue;

                        TextBox txtQty = (TextBox)(row.Cells[2].FindControl("txtQty"));
                        txtQty.Text = orderDetail.Quantity.ToString();

                        Label lblFindPrice = (Label)(row.Cells[4].FindControl("lblPrice"));
                        lblFindPrice.Text = Currencies.CurrencyToString(orderDetail.Price, account.CurrencyCode);
                    }

                    //Instadose 2 products.  Original order quantities, if any.
                    foreach (GridViewRow row in gvInstadose2.Rows)
                    {
                        HiddenField hidProductID = (HiddenField)row.FindControl("hfProductID");
                        int tmpInt = 0;

                        // If ProductIDs do not match then continue (Instadose Plus has all same SKUs)
                        if (int.TryParse(hidProductID.Value, out tmpInt) &
                           (orderDetail.ProductID != tmpInt)) continue;

                        TextBox txtQty = (TextBox)(row.Cells[2].FindControl("txtQty"));
                        txtQty.Text = orderDetail.Quantity.ToString();

                        Label lblFindPrice = (Label)(row.Cells[4].FindControl("lblPrice"));
                        lblFindPrice.Text = Currencies.CurrencyToString(orderDetail.Price, account.CurrencyCode);
                    }
                }  // end else

                //Accessories.  Original order quantities, if any.
                foreach (GridViewRow row in gvAccessories.Rows)
                {
                    // If they do not match skus continue...
                    if (orderDetail.Product.ProductSKU != row.Cells[2].Text) continue;

                    TextBox txtQty = (TextBox)(row.Cells[2].FindControl("txtQty"));
                    txtQty.Text = orderDetail.Quantity.ToString();

                    Label lblFindPrice = (Label)(row.Cells[4].FindControl("lblPrice"));
                    lblFindPrice.Text = Currencies.CurrencyToString(orderDetail.Price, account.CurrencyCode);
                }

                // Instadose elite demo order
                //if (order.OrderType == "DEMO" && account.BrandSourceID == 3)
                if (account.isDemoAccount && account.BrandSourceID == 3)
                {
                    foreach (GridViewRow row in gvInstadoseEliteDemo.Rows)
                    {
                        HiddenField hidProductID = (HiddenField)row.FindControl("hfProductID");
                        int tmpInt = 0;

                        // If ProductIDs do not match then continue (Instadose Plus has all same SKUs)
                        if (int.TryParse(hidProductID.Value, out tmpInt) &
                           (orderDetail.ProductID != tmpInt)) continue;

                        TextBox txtQty = (TextBox)(row.Cells[2].FindControl("txtQty"));
                        txtQty.Text = orderDetail.Quantity.ToString();

                        Label lblFindPrice = (Label)(row.Cells[4].FindControl("lblPrice"));
                        lblFindPrice.Text = Currencies.CurrencyToString(orderDetail.Price, account.CurrencyCode);
                    }
                }
            }

            //Update product totals.
            updateProductGridPricing();

            //If IC Care then update service start date on Product grid(s).
            //if (account.BrandSourceID.Value == 3 && order != null)
            //if (account.BrandSourceID.Value == 3 && order != null && order.OrderType != "DEMO")
            if (account.isDemoAccount && account.BrandSourceID.Value == 3 && order != null)
            {
                GridView[] grids = new GridView[] { gvProduct, gvInstadosePlus, gvInstadose2, gvAccessories, gvInstadoseEliteDemo };

                // Loop through the grids to update the data
                foreach (GridView grid in grids)
                {
                    // Loop through the rows of the grid.
                    foreach (GridViewRow row in grid.Rows)
                    {
                        HiddenField hfProductID = (HiddenField)row.FindControl("hfProductID");
                        TextBox txtItemQty = (TextBox)row.Cells[2].FindControl("txtQty");
                        int productID = int.Parse(hfProductID.Value);

                        int itemQty = 0;
                        // If there is no quantity or the quantity does not contain an int...
                        if (!int.TryParse(txtItemQty.Text, out itemQty))
                        {
                            txtItemQty.Text = "";
                            continue;
                        }

                        // Get the order detail line item for this record...
                        var detail = order.OrderDetails.Where(d => d.ProductID == productID).FirstOrDefault();
                        if (detail == null) continue;

                    }
                }
            }
            // End the IC Care dates loop
        }
        else
        {
            throw new Exception("The order has already been released to MAS and cannot be edited.");
        }
    }

    /// <summary>
    /// Load the billing and shipping address fields.
    /// </summary>
    /// <summary>
    /// Load the billing and shipping address fields.
    /// </summary>
    private void fillLocationControl()
    {
        this.txtLocation.Text = location.LocationName;

        this.ddlBillingState.Items.Clear();
        this.ddlShippingState.Items.Clear();

        // If ICCare Brand (BrandSourceID = 3) and OrderType = LOSTDAMAGED
        if (account.BrandSourceID.Value == 3 && orderType == OrderTypes.Replacement)
        {
            this.txtBillingCompany.Text = location.ShippingCompany;
            this.txtBillingFirstName.Text = location.ShippingFirstName;
            this.txtBillingLastName.Text = location.ShippingLastName;
            this.txtBillingAddress1.Text = location.ShippingAddress1;
            this.txtBillingAddress2.Text = location.ShippingAddress2;
            this.txtBillingAddress3.Text = location.ShippingAddress3;
            this.txtBillingCity.Text = location.ShippingCity;
            this.txtBillingPostalCode.Text = location.ShippingPostalCode;
            this.ddlBillingCountry.SelectedValue = location.ShippingCountryID.ToString();

            // Fill the billing state.
            ListItem billingState = new ListItem(location.ShippingState.StateAbbrev, location.ShippingStateID.ToString());
            billingState.Selected = true;

            // Clear any old entries out.
            this.ddlBillingState.Items.Add(billingState);
        }
        else
        {
            this.txtBillingCompany.Text = location.BillingCompany;
            this.txtBillingFirstName.Text = location.BillingFirstName;
            this.txtBillingLastName.Text = location.BillingLastName;
            this.txtBillingAddress1.Text = location.BillingAddress1;
            this.txtBillingAddress2.Text = location.BillingAddress2;
            this.txtBillingAddress3.Text = location.BillingAddress3;
            this.txtBillingCity.Text = location.BillingCity;
            this.txtBillingPostalCode.Text = location.BillingPostalCode;
            this.ddlBillingCountry.SelectedValue = location.BillingCountryID.ToString();

            // Fill the billing state.
            ListItem billingState = new ListItem(location.BillingState.StateAbbrev, location.BillingStateID.ToString());
            billingState.Selected = true;

            // Clear any old entries out.
            this.ddlBillingState.Items.Add(billingState);
        }

        // Shipping Info
        this.txtShippingCompany.Text = location.ShippingCompany;
        this.txtShippingFirstName.Text = location.ShippingFirstName;
        this.txtShippingLastName.Text = location.ShippingLastName;
        this.txtShippingAddress1.Text = location.ShippingAddress1;
        this.txtShippingAddress2.Text = location.ShippingAddress2;
        this.txtShippingAddress3.Text = location.ShippingAddress3;
        this.txtShippingCity.Text = location.ShippingCity;
        this.txtShippingPostalCode.Text = location.ShippingPostalCode;
        this.ddlShippingCountry.SelectedValue = location.ShippingCountryID.ToString();

        if (ddlShippingState.SelectedValue != "")
            this.lblShippingState.Text = ddlShippingState.SelectedItem.Text;

        // Fill the shipping state.
        ListItem shippingState = new ListItem(location.ShippingState.StateAbbrev, location.ShippingState.StateID.ToString());
        shippingState.Selected = true;

        // Clear any old entries out.
        this.ddlShippingState.Items.Add(shippingState);


        // Set the Order Summary labels.
        this.lblOrderCompany.Text = this.txtBillingCompany.Text;
        this.lblShippingName.Text = this.txtShippingFirstName.Text + " " + this.txtShippingLastName.Text;
        this.lblBillingName.Text = this.txtBillingFirstName.Text + " " + this.txtBillingLastName.Text;
        this.lblBillingDayPrior.Text = account.BillingDaysPrior.ToString();

        this.lblBillingCompany.Text = this.txtBillingCompany.Text;
        this.lblBillingAddress1.Text = this.txtBillingAddress1.Text;
        this.lblBillingAddress2.Text = this.txtBillingAddress2.Text;
        this.lblBillingAddress3.Text = this.txtBillingAddress3.Text;
        this.lblBillingCity.Text = this.txtBillingCity.Text;
        this.lblBillingPostalCode.Text = this.txtBillingPostalCode.Text;
        this.lblBillingState.Text = this.ddlBillingState.SelectedItem.Text;

        this.lblShippingCompany.Text = this.txtShippingCompany.Text;
        this.lblShippingAddress1.Text = this.txtShippingAddress1.Text;
        this.lblShippingAddress2.Text = this.txtShippingAddress2.Text;
        this.lblShippingAddress3.Text = this.txtShippingAddress3.Text;
        this.lblShippingCity.Text = this.txtShippingCity.Text;
        this.lblShippingPostalCode.Text = this.txtShippingPostalCode.Text;
        this.lblShippingState.Text = this.ddlShippingState.SelectedItem.Text;

        // Default the Billing Information to be the Shipping Information.

        //if (hfSelectedProductName.Value == "Renewal"  && account.BrandSourceID == 3)
        //    lblOrderID.Text = this.order.OrderID.ToString();

        if (location.ShippingCompany == "")
        {
            divBillingCompany.Visible = false;
            divNoBillingCompany.Visible = true;
        }

        int serviceDaysRemaining = 0;
        bool contractError = false;
        if ((account.ContractStartDate.HasValue && account.ContractEndDate.HasValue) || (location.ContractStartDate.HasValue && location.ContractEndDate.HasValue))
        {
            lblServiceStartDate.Text = string.Format("{0:MM/dd/yyyy}", GetContractStartDate());
            lblServiceEndDate.Text = string.Format("{0:MM/dd/yyyy}", GetContractEndDate());
            // Find service days left from today's date to the end of contract date
            serviceDaysRemaining = GetContractEndDate().Subtract(DateTime.Today).Days;
        }
        else
        {
            lblServiceStartDate.Text = "";
            lblServiceEndDate.Text = "";
            contractError = true;
        }

        if (contractError)
        {
            lblOrderServicePeriod.Text = "The contract period is missing.";
            lblServiceDayLeft.Text = "The contract period is missing.";
            lblServiceDayLeft.CssClass = "InlineError";
        }
        else
        {
            if (serviceDaysRemaining == 1)
                lblServiceDayLeft.Text = string.Format("1 day remaining in the contract period.");
            else if (serviceDaysRemaining > 0)
                lblServiceDayLeft.Text = string.Format("{0:#,###} days remaining in the contract.", serviceDaysRemaining);
            else
            {
                lblServiceDayLeft.Text = string.Format("The contract expired {0:#,###} days ago.", Math.Abs(serviceDaysRemaining));
                lblServiceDayLeft.CssClass = "InlineError";
            }

            // Display the service period label.
            this.lblOrderServicePeriod.Text = string.Format("{0} - {1} ({2})",
                lblServiceStartDate.Text, lblServiceEndDate.Text, lblServiceDayLeft.Text);
        }
    }

    /// <summary>
    /// Load the credit card or PO section.
    /// </summary>
    private void loadPaymentSection()
    {
        // Display the default information/controls for PO Number rdnbtn selection.
        this.divPaymentPO.Visible = true;
        //this.divPaymentCC.Visible = false;
        this.lblPaymentMethod.Text = "Purchase Order"; //rblstPayMethod.SelectedValue;
                                                       //this.lblUpdateCCInformation.Visible = false;
                                                       //this.lblUpdateCCInformation.Text = string.Empty;

        // NOT LONGER NEEDED, CHECKING FOR "Credit Card" SELECTION.
        // IF "Purchase Order" is selected, do not load any CC Payment Information.
        //if (this.rblstPayMethod.SelectedValue == "Purchase Order") return;

        //if (this.rblstPayMethod.SelectedValue == "Credit Card")
        //{
        //	// Get the AccountCreditCard record based on Account DataObject.
        //	accountcreditcard = GetAccountCreditCard(account);

        //	// Check to see if the Account has a valid CC on record.
        //	if (accountcreditcard != null)
        //	{
        //		// IF "Purchase Order" is not selected, show the CC Payment information.
        //		this.divPaymentPO.Visible = false;
        //		this.divPaymentCC.Visible = true;

        //		// Hide "Udpate Account CC Information" message.
        //		this.lblUpdateCCInformation.Visible = false;
        //		this.lblUpdateCCInformation.Text = string.Empty;

        //		switch (accountcreditcard.CreditCardTypeID)
        //		{
        //			case 1: //visa 324
        //				this.rbtCardType.SelectedIndex = 0;
        //				this.lblCreditCard.Text = "Visa";
        //				break;
        //			case 2: //MasterCard 325
        //				this.rbtCardType.SelectedIndex = 1;
        //				this.lblCreditCard.Text = "MasterCard";
        //				break;
        //			case 3: //Discover 330
        //				this.rbtCardType.SelectedIndex = 2;
        //				this.lblCreditCard.Text = "Discover";
        //				break;
        //			case 4: // AE 333
        //				this.rbtCardType.SelectedIndex = 3;
        //				this.lblCreditCard.Text = "American Express";
        //				break;
        //		}

        //		this.txtCCName.Text = accountcreditcard.NameOnCard;
        //		this.ddlCCMonth.SelectedIndex = accountcreditcard.ExpMonth;
        //		this.ddlCCYear.SelectedValue = accountcreditcard.ExpYear.ToString();
        //		this.txtCCcvc.Text = accountcreditcard.SecurityCode;
        //		this.txtCCno.Text = Common.MaskCreditCardNumber(accountcreditcard.NumberEncrypted, accountcreditcard.CreditCardType.CreditCardName);
        //		this.lblCreditCard.Text = Common.MaskCreditCardNumber(accountcreditcard.NumberEncrypted, accountcreditcard.CreditCardType.CreditCardName);
        //	}
        //	else
        //	{
        //		this.divPaymentPO.Visible = false;
        //		this.divPaymentCC.Visible = false;
        //		// Display "Udpate Account CC Information" message.
        //		this.lblUpdateCCInformation.Visible = true;
        //		this.lblUpdateCCInformation.Text = "This Account either does not have a Credit Card on record or it has expired, please update the information.";
        //	}
        //}
    }

    /// <summary>
    /// Load the products into the grid.
    /// </summary>
    private void loadProductsGrid()
    {
        string[] instadoseImiProductSKUs = new string[5] { "CASE CVR BLK", "CASE CVR BLUE", "CASE CVR GREEN", "CASE CVR PINK", "CASE CVR SLVR" };
        string[] instadosePlusProductSKUs = new string[6] { "COLOR CAP BLUE", "COLOR CAP GREY", "COLOR CAP GRN", "COLOR CAP ORANG", "COLOR CAP PINK", "COLOR CAP RED" };
        string[] accessoriesProductSKUs = new string[2] { "INSTALINK", "INSTALINKUSB" };
        string[] instadoseEliteDemoProductSKUs = new string[1] { "INSTA20" };
        string[] instadose2ProductSKUs = new string[6] { "ID2BBUMP", "ID2GYBUMP", "ID2GBUMP", "ID2OBUMP", "ID2PBUMP", "ID2RBUMP" };

        //Added: JGP Instadose 3 Constants
        string[] instadose3AnnualSubscription = new string[2] { Instadose3Constants.NA, Instadose3Constants.NoColor };
        string[] instadose3ProductSKUs = new string[6] { Instadose3Constants.CollarRedProductSKU, 
                                                         Instadose3Constants.CollarBlueProductSKU, 
                                                         Instadose3Constants.CollarGreyProductSKU, 
                                                         Instadose3Constants.CollarGreenProductSKU, 
                                                         Instadose3Constants.CollarOrangeProductSKU, 
                                                         Instadose3Constants.CollarPinkProductSKU 
                                                       };

        var productsInstadose1IMI = (from p in idc.Products
                                     where (instadoseImiProductSKUs.Contains(p.ProductSKU))
                                     && p.Active == true
                                     select new
                                     {
                                         ProductID = p.ProductID,
                                         Color = p.Color,
                                         ProductName = p.ProductName,
                                         BillingTermID = account.BillingTermID,
                                         BillingTermDesc = account.BillingTerm.BillingTermDesc,
                                         ProductSKU = p.ProductSKU,
                                         Price = 0.00M,
                                         ProductGroupID = p.ProductGroupID
                                     });


        var productsInstadosePlus = (from p in idc.Products
                                     where (instadosePlusProductSKUs.Contains(p.ProductSKU))
                                     && p.Active == true
                                     select new
                                     {
                                         ProductID = p.ProductID,
                                         Color = p.Color,
                                         ProductName = p.ProductName,
                                         BillingTermID = account.BillingTermID,
                                         BillingTermDesc = account.BillingTerm.BillingTermDesc,
                                         ProductSKU = p.ProductSKU,
                                         Price = 0.00M,
                                         ProductGroupID = p.ProductGroupID
                                     });

        var productsAccessory = (from p in idc.Products
                                 where (accessoriesProductSKUs.Contains(p.ProductSKU))
                                     && p.Active == true
                                     && p.SoldOnWeb == true
                                 select new
                                 {
                                     ProductID = p.ProductID,
                                     Color = p.Color,
                                     ProductName = p.ProductName,
                                     BillingTermID = account.BillingTermID,
                                     BillingTermDesc = account.BillingTerm.BillingTermDesc,
                                     ProductSKU = p.ProductSKU,
                                     Price = 0.00M,
                                     ProductGroupID = p.ProductGroupID
                                 });

        var productsInstadoseEliteDemo = idc.Products
                                            .Where(p => instadoseEliteDemoProductSKUs.Contains(p.ProductSKU) && p.Active && p.Color == "No Color")
                                            .Select(p => new
                                            {
                                                p.ProductID,
                                                p.Color,
                                                p.ProductName,
                                                account.BillingTermID,
                                                account.BillingTerm.BillingTermDesc,
                                                p.ProductSKU,
                                                Price = 0.00M,
                                                p.ProductGroupID
                                            });

        var productsInstadose2 = (from p in idc.Products
                                  where (instadose2ProductSKUs.Contains(p.ProductSKU))
                                  && p.Active == true
                                  select new
                                  {
                                      ProductID = p.ProductID,
                                      Color = p.Color,
                                      ProductName = p.ProductName,
                                      BillingTermID = account.BillingTermID,
                                      BillingTermDesc = account.BillingTerm.BillingTermDesc,
                                      ProductSKU = p.ProductSKU,
                                      Price = 0.00M,
                                      ProductGroupID = p.ProductGroupID
                                  });

        // DA added for ID3
        var productsInstadose3 = (from p in idc.Products
                                  join pg in idc.ProductGroups on p.ProductGroupID equals pg.ProductGroupID
                                  where pg.ProductGroupID == Instadose3Constants.ProductGroupId
                                  && p.Active == true && !instadose3AnnualSubscription.Contains(p.Color)
                                  select new
                                  {
                                      ProductID = p.ProductID,
                                      Color = p.Color,
                                      ProductName = p.ProductName,
                                      BillingTermID = account.BillingTermID,
                                      BillingTermDesc = account.BillingTerm.BillingTermDesc,
                                      ProductSKU = p.ProductSKU,
                                      Price = 0.00M,
                                      ProductGroupID = p.ProductGroupID
                                  });

        // load instadose 1 badges
        gvProduct.DataSource = productsInstadose1IMI;
        gvProduct.DataBind();

        gvInstadosePlus.DataSource = productsInstadosePlus;
        gvInstadosePlus.DataBind();


        // load instalink and instalink usb
        gvAccessories.DataSource = productsAccessory;
        gvAccessories.DataBind();

        gvInstadoseEliteDemo.DataSource = productsInstadoseEliteDemo;
        gvInstadoseEliteDemo.DataBind();

        gvInstadose2.DataSource = productsInstadose2;
        gvInstadose2.DataBind();

        // DA added for ID3
        if (radProductName.SelectedValue != "Instadose Plus" && radProductName.SelectedValue != "Instadose 3")
        {
            gvInstadose3.DataSource = productsInstadose3;
            gvInstadose3.DataBind();
        }


        if (account.CustomerTypeID == iConsignmentCustomerTypeID)
        {  // consignment account 
            loadConsignmentProductsGrid();

        }

        // show startDate dropdownlist if IC Care account
        if (account.BrandSourceID.Value == 3)
        {
            pnlServiceStartDate.Visible = true;
            // Get the start date based on the account or location.
            DateTime serviceStartDate = GetContractStartDate();

            DateTime firstDayOfTheMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);

            for (int i = 0; i <= 11; i++)
            {
                var serviceDate = firstDayOfTheMonth.AddMonths(i);
                var item = new ListItem(serviceDate.ToShortDateString());
                item.Selected = (serviceDate == serviceStartDate);

                ddlServiceStartDate.Items.Add(item);
            }


            // make the start date invisible
            if (order != null && order.OrderType == "RENEWAL")
                //ddlServiceStartDate.Visible = false;
                pnlServiceStartDate.Visible = false;


        }

        // UPDATE & FORMAT the Product GridView with correct Pricing.
        this.updateProductGridPricing();
    }
    private void loadConsignmentProductsGrid()
    {
        string[] instadosePlusCapsProductSKUs = { "COLOR CAP BLUE", "COLOR CAP GREY", "COLOR CAP GRN", "COLOR CAP ORANG", "COLOR CAP PINK", "COLOR CAP RED", "INSTA PLUS", "LABELROLLID+", "LABELSHEETID+" };

        string[] instadose2ConsignmentProductSKUs = { "INSTA ID2", "ID2BBUMP", "ID2GYBUMP", "ID2GBUMP", "ID2OBUMP", "ID2PBUMP", "ID2RBUMP", "LABELROLLID2", "LABELSHEETID2" };

        var productsInstadosePlusCaps = (from p in idc.Products
                                         where (instadosePlusCapsProductSKUs.Contains(p.ProductSKU))
                                         && p.Active == true
                                         select new
                                         {
                                             ProductID = p.ProductID,
                                             Color = p.Color,
                                             ProductName = p.ProductName,
                                             BillingTermID = account.BillingTermID,
                                             BillingTermDesc = account.BillingTerm.BillingTermDesc,
                                             ProductSKU = p.ProductSKU,
                                             Price = 0.00M,
                                             ProductGroupID = p.ProductGroupID
                                         });



        decimal id2price = 0.00m;



        try
        {

            var vid2price = (from rd in idc.RateDetails
                             where rd.RateID == account.DefaultRateID && rd.ProductGroupID == instadose2ProductGroupID  // productgroupid for id2 is 11
                             select rd.Price).FirstOrDefault();
            id2price = (decimal)vid2price;
        }
        catch { }

        int[] iproductGroupArrary = new int[3] { instadose2ProductGroupID, instadose2RollProductGroupID, instadose2SheetProductGroupID };   // productgroupid for id2 is 11

        var productsInstadose2Consignment = (from p in idc.Products
                                             where (instadose2ConsignmentProductSKUs.Contains(p.ProductSKU))
                                             && p.Active == true && iproductGroupArrary.Contains(p.ProductGroupID)
                                             orderby p.ProductName, p.ProductID
                                             select new
                                             {
                                                 ProductID = p.ProductID,
                                                 Color = p.Color,
                                                 ProductName = p.ProductDesc,
                                                 BillingTermID = account.BillingTermID,
                                                 BillingTermDesc = account.BillingTerm.BillingTermDesc,
                                                 ProductSKU = p.ProductSKU,
                                                 Price = id2price,
                                                 ProductGroupID = p.ProductGroupID
                                             });


        gvInstadosePlusCaps.DataSource = productsInstadosePlusCaps;
        gvInstadosePlusCaps.DataBind();


        gvInstadose2Con.DataSource = productsInstadose2Consignment;
        gvInstadose2Con.DataBind();


        foreach (GridViewRow row in gvInstadose2Con.Rows)
        {
            //HiddenField hidProductID = (HiddenField)row.FindControl("hfProductID");
            //int tmpInt = 0;


            //int.TryParse(hidProductID.Value, out tmpInt) ;

            string ssku = row.Cells[2].Text;

            //Label lblFindPrice = (Label)(row.Cells[4].FindControl("lblPrice"));
            // customize product description
            switch (ssku)
            {
                case instadose2ProductSKU:
                    (row.Cells[0]).Text = "Instadose 2";
                    break;
                case instadose2BumperBlueProductSKU:
                    (row.Cells[0]).Text = "Color Bumper Blue";
                    break;
                case instadose2BumperGreyProductSKU:
                    (row.Cells[0]).Text = "Color Bumper Grey";
                    break;
                case instadose2BumperGreenProductSKU:
                    (row.Cells[0]).Text = "Color Bumper Green";
                    break;
                case instadose2BumperOrangeProductSKU:
                    (row.Cells[0]).Text = "Color Bumper Orange";
                    break;
                case instadose2BumperPinkProductSKU:
                    (row.Cells[0]).Text = "Color Bumper Pink";
                    break;
                case instadose2BumperRedProductSKU:
                    (row.Cells[0]).Text = "Color Bumper Red";
                    break;
            }
        }



    }
    /// <summary>
    /// Change the selected location.
    /// </summary>
    private void setLocation(int locationID)
    {
        // Exist if the location could not be parsed.
        if (locationID == 0) return;
        location = (from l in account.Locations where l.LocationID == locationID select l).FirstOrDefault();

        // Reload the billing and shipping address
        fillLocationControl();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="pSelectedProductName"></param>
    private void visibleProductTab(string pSelectedProductName)
    {
        // For IC Care Demo Account to place ID Elite Demo order
        if (account.isDemoAccount && account.BrandSourceID == 3)
        {
            tabInsta1.Visible = false;
            gvProduct.Visible = false;
            lblInstaTotal.Visible = false;

            tabInstaPlus.Visible = false;
            gvInstadosePlus.Visible = false;
            gvAccessories.Visible = false;

            tabInsta2.Visible = false;
            gvInstadose2.Visible = false;

            lblInstaPlusTotal.Visible = false;
            lblAccessTotal.Visible = false;
            lblInsta2Total.Visible = false;

            tabInstaEliteDemo.Visible = true;
            gvInstadoseEliteDemo.Visible = true;

            tabInstaPlus.Visible = false;
            tabInsta3.Visible = false;
        }
        else
        {
            if (pSelectedProductName == "Instadose 1")
            {
                tabInsta1.Visible = true;
                gvProduct.Visible = true;
                lblInstaTotal.Visible = true;

                tabInstaPlus.Visible = false;
                gvInstadosePlus.Visible = false;
                gvAccessories.Visible = false;

                tabInsta2.Visible = false;
                gvInstadose2.Visible = false;

                lblInstaPlusTotal.Visible = false;
                lblAccessTotal.Visible = false;
                lblInsta2Total.Visible = false;

                tabInstaEliteDemo.Visible = false;
                gvInstadoseEliteDemo.Visible = false;

                tabInsta3.Visible = false;
                gvInstadose3.Visible = false;

                // Grab the account and product info
                int activeBadgesCount = (from a in idc.AccountDevices
                                         where a.Active
               && a.AccountID == account.AccountID
               && (a.DeviceInventory.Product.ProductGroup1.ProductGroupName == "Instadose 1" || a.DeviceInventory.Product.ProductGroup1.ProductGroupName == "Instadose 1 IMI")
                                         select a).Count();
                lblActiveBadge.Text = string.Format("{0:#,##0}", activeBadgesCount);
            }
            else if (pSelectedProductName == "Renewal")
            {
                if (account.BrandSourceID == 3)  // ic care
                {
                    gvRenewal.Visible = true;
                    gvProduct.Visible = false;
                }
                else
                {
                    gvRenewal.Visible = false;
                    gvProduct.Visible = true;

                }

                tabInsta1.Visible = true;
                lblInstaTotal.Visible = true;

                tabInstaPlus.Visible = false;
                gvInstadosePlus.Visible = false;
                gvAccessories.Visible = false;

                tabInsta2.Visible = false;
                gvInstadose2.Visible = false;

                lblInstaPlusTotal.Visible = false;
                lblAccessTotal.Visible = false;

                tabInstaEliteDemo.Visible = false;
                gvInstadoseEliteDemo.Visible = false;

                tabInsta3.Visible = false;
                gvInstadose3.Visible = false;

                // Grab the account and product info
                // int activeBadgesCount = (from a in idc.AccountDevices
                //                          where a.Active
                //&& a.AccountID == account.AccountID
                //&& (a.DeviceInventory.Product.ProductGroup1.ProductGroupName.Trim() == "Instadose 1" || a.DeviceInventory.Product.ProductGroup1.ProductGroupName.Trim() == "Instadose 1 IMI")
                //                          select a).Count();
                int activeBadgesCount = (from a in idc.AccountDevices
                                         where a.Active && a.AccountID == account.AccountID
                                         select a).Count();
                lblActiveBadge.Text = string.Format("{0:#,##0}", activeBadgesCount);
            }
            else if (pSelectedProductName == "Instadose Plus")
            {
                tabInsta1.Visible = false;
                gvProduct.Visible = false;
                lblInstaTotal.Visible = false;

                tabInstaPlus.Visible = true;
                gvInstadosePlus.Visible = true;
                gvAccessories.Visible = true;

                tabInsta2.Visible = true;
                gvInstadose2.Visible = true;

                lblInstaPlusTotal.Visible = true;
                lblAccessTotal.Visible = true;

                lblInsta2Total.Visible = true;

                tabInstaEliteDemo.Visible = false;
                gvInstadoseEliteDemo.Visible = false;

                //tabInsta3.Visible = false;
                gvInstadose3.Visible = false;

                // Grab the account and product info
                int activeBadgesCount_IDPlus = (from a in idc.AccountDevices
                                                where a.Active
                                                    && a.AccountID == account.AccountID
                                                    && (a.DeviceInventory.Product.ProductGroup.ProductGroupName == "Instadose Plus")
                                                select a).Count();
                int activeBadgesCount_ID2 = (from a in idc.AccountDevices
                                             where a.Active
                                                 && a.AccountID == account.AccountID
                                                 && (a.DeviceInventory.Product.ProductGroupID == 11)
                                             select a).Count();

                int activeBadgesCount = activeBadgesCount_IDPlus + activeBadgesCount_ID2;

                lblActiveBadge.Text = string.Format("{0:#,##0}", activeBadgesCount);

                if (account.CustomerTypeID == iConsignmentCustomerTypeID)  // consignment account
                {
                    gvInstadosePlusCaps.Visible = true;
                    tabInstaPlusAccessories.Visible = false;
                    gvInstadosePlus.Visible = false;
                    gvInstadose2.Visible = false;
                }
                //else
                //{
                //    gvInstadosePlusCaps.Visible = false;
                //    tabInstaPlusAccessories.Visible = false;
                //}
            }
            else if (pSelectedProductName == "Instadose 3" || pSelectedProductName.Equals(String.Empty))
            {
                tabInsta3.Visible = true;
                gvInstadose3.Visible = true;

                tabInsta1.Visible = false;
                gvProduct.Visible = false;
                lblInstaTotal.Visible = false;

                tabInstaPlus.Visible = false;
                gvInstadosePlus.Visible = false;
                gvAccessories.Visible = false;

                tabInsta2.Visible = false;
                gvInstadose2.Visible = false;

                lblInstaPlusTotal.Visible = false;
                lblAccessTotal.Visible = false;
                lblInsta2Total.Visible = false;

                tabInstaEliteDemo.Visible = false;
                gvInstadoseEliteDemo.Visible = false;
            }
            else
            {
                tabInsta1.Visible = false;
                gvProduct.Visible = false;
                lblInstaTotal.Visible = false;

                tabInstaPlus.Visible = true;
                gvInstadosePlus.Visible = true;
                gvAccessories.Visible = true;

                lblInstaPlusTotal.Visible = true;
                lblAccessTotal.Visible = true;
                lblInsta2Total.Visible = true;

                tabInstaEliteDemo.Visible = false;
                gvInstadoseEliteDemo.Visible = false;

                tabInstaEliteDemo.Visible = false;

                tabInsta3.Visible = false;
                gvInstadose3.Visible = false;

                // Grab the account and product info
                int activeBadgesCount = (from a in idc.AccountDevices
                                         where a.Active
                                             && a.AccountID == account.AccountID
                                             && (a.DeviceInventory.Product.ProductGroup.ProductGroupName == "Instadose 2")
                                         select a).Count();
                lblActiveBadge.Text = string.Format("{0:#,##0}", activeBadgesCount);
            }  // end else


            if (account.CustomerTypeID == iConsignmentCustomerTypeID)
            {
                //rblstPayMethod.Enabled = false;
                if (pSelectedProductName != "Instadose 1")
                {
                    tabInsta2.Visible = true;
                    gvInstadose2Con.Visible = true;
                }

            }
        }

    }

    /// <summary>
    /// Determine order type and update prices on grid if
    /// necessary.
    /// </summary>
    /// <param name="newOrderType"></param>
    private void changeOrderType(OrderTypes newOrderType, bool updatePricing)
    {
        if (MalvernIntegration)   // Implemented for Malvern Integration - Hbabu 05/12/14
        {
            // orderTypeForm.Visible = true;
            // ddlShippingMethodMalvern.Enabled = true;
            // ddlShippingMethod.Enabled = false;

            string freeShippingMethodID = "";

            switch (ddShippingCarrier.SelectedItem.Text.ToUpper())
            {
                case "FEDEX":
                    freeShippingMethodID = (from a in idc.ShippingMethods where a.ShippingMethodDesc.Trim() == "FedEx Free Shipping" select a.ShippingMethodID).FirstOrDefault().ToString();
                    break;
                case "DHL GLOBAL MAIL":
                    freeShippingMethodID = (from a in idc.ShippingMethods where a.ShippingMethodDesc.Trim() == "DHL Free Shipping" select a.ShippingMethodID).FirstOrDefault().ToString();
                    break;
                case "UPS":
                    freeShippingMethodID = (from a in idc.ShippingMethods where a.ShippingMethodDesc.Trim() == "UPS Free Shipping" select a.ShippingMethodID).FirstOrDefault().ToString();
                    break;
                case "US POSTAL SERVICES":
                    freeShippingMethodID = (from a in idc.ShippingMethods where a.ShippingMethodDesc.Trim() == "USPS Free Shipping" select a.ShippingMethodID).FirstOrDefault().ToString();
                    break;
                case "DHL INTERNATIONAL":
                    freeShippingMethodID = (from a in idc.ShippingMethods where a.ShippingMethodDesc.Trim() == "DHL International Free Shipping" select a.ShippingMethodID).FirstOrDefault().ToString();
                    break;
                case "N/A":
                    freeShippingMethodID = (from a in idc.ShippingMethods where a.ShippingMethodDesc.Trim() == "Free Shipping Ground" select a.ShippingMethodID).FirstOrDefault().ToString();
                    break;
            }


            switch (newOrderType)
            {
                case OrderTypes.New:
                    ddlShippingMethodMalvern.Enabled = true;
                    orderTypeForm.Visible = false;
                    serialNoSection.Visible = false;
                    break;
                case OrderTypes.NoChargeReplacement:
                    txtPOno.Text = "BROKEN CLIP";
                    ddlShippingMethodMalvern.Enabled = false;
                    ddlShippingMethodMalvern.SelectedValue = freeShippingMethodID;   // free shipping
                                                                                     //ddlShippingMethodMalvern.SelectedItem.Text="Free Shipping Ground ( No Charge, 1 - 5 Days )"; //ddlShippingMethodMalvern.SelectedValue = "26";  // Free Shipping Ground.
                    rbtnReplacementBrokenClip.Checked = true;
                    orderTypeForm.Visible = true;
                    serialNoSection.Visible = true;
                    break;
                case OrderTypes.NoReplacement:
                    txtPOno.Text = "NO REPLACE";
                    ddlShippingMethodMalvern.SelectedValue = freeShippingMethodID;   // free shipping
                                                                                     //ddlShippingMethodMalvern.SelectedItem.Text="Free Shipping Ground ( No Charge, 1 - 5 Days )";   //ddlShippingMethodMalvern.SelectedValue = "26";  // Free Shipping Ground.
                    ddlShippingMethodMalvern.Enabled = true;
                    rbtnNoReplacement.Checked = true;
                    orderTypeForm.Visible = true;
                    serialNoSection.Visible = true;
                    break;
                case OrderTypes.Replacement:
                    txtPOno.Text = "REPLACEDIS";
                    rbtnReplacementLostBadge.Checked = true;
                    ddlShippingMethodMalvern.Enabled = true;
                    orderTypeForm.Visible = true;
                    serialNoSection.Visible = true;
                    break;
            }
        }
        else
        {
            switch (newOrderType)
            {
                case OrderTypes.New:
                    ddlShippingMethod.Enabled = true;
                    orderTypeForm.Visible = false;
                    serialNoSection.Visible = false;
                    break;
                case OrderTypes.NoChargeReplacement:
                    txtPOno.Text = "BROKEN CLIP";
                    ddlShippingMethod.Enabled = false;
                    ddlShippingMethod.SelectedValue = "11";  // Free Shipping.
                    rbtnReplacementBrokenClip.Checked = true;
                    orderTypeForm.Visible = true;
                    serialNoSection.Visible = true;
                    break;
                case OrderTypes.NoReplacement:
                    txtPOno.Text = "NO REPLACE";
                    ddlShippingMethod.SelectedValue = "11";  // Free Shipping.
                    ddlShippingMethod.Enabled = true;
                    rbtnNoReplacement.Checked = true;
                    orderTypeForm.Visible = true;
                    serialNoSection.Visible = true;
                    break;
                case OrderTypes.Replacement:
                    txtPOno.Text = "REPLACEDIS";
                    rbtnReplacementLostBadge.Checked = true;
                    ddlShippingMethod.Enabled = true;
                    orderTypeForm.Visible = true;
                    serialNoSection.Visible = true;
                    break;
            }
        }

        // Update the order type.
        orderType = newOrderType;
        hfOrderType.Value = orderType.ToString();

        if (updatePricing) updateProductGridPricing();
    }

    /// <summary>
    /// updateProductGrid - AutoPostBack update GridView QTY, Price, Extended, SubTotal & Total Values
    /// </summary>
    private void updateProductGridPricing()
    {
        decimal unitPrice = 0;
        int tmpInt = 0;

        // UPDATE Instadose 1 (IMI) Prices.
        foreach (GridViewRow rowitem in gvProduct.Rows)
        {
            // Get each line item's grid row and update the price
            HiddenField productGroupID = (HiddenField)rowitem.FindControl("hfProductGroupID");
            unitPrice = 0;
            tmpInt = 0;

            // GET the Price per LineItem.
            // Here we have to CONVERT the new ProductSKU to its respective OLD ProductGroupID to get the correct Price.
            // Instadose 1 (IMI) = 9 --> Instadose 1 = 1.
            if (productGroupID != null) {
                if (int.TryParse(productGroupID.Value, out tmpInt)) unitPrice = getUnitPrice(tmpInt);
            }
            //unitPrice = getUnitPrice(1);

            TextBox txtItemQty = (TextBox)rowitem.Cells[2].FindControl("txtQty");
            Label lblPrice = (Label)rowitem.Cells[3].FindControl("lblPrice");
            Label lblExtendedPrice = (Label)rowitem.Cells[4].FindControl("lblExtended");

            int itemQty = 0;
            if (!int.TryParse(txtItemQty.Text, out itemQty)) txtItemQty.Text = "";
            if (itemQty < 0)
            {
                itemQty = 0;
                txtItemQty.Text = "";
            }
            decimal extendedPrice = itemQty * unitPrice;

            // Set the price per line
            lblPrice.Text = Currencies.CurrencyToString(unitPrice, account.CurrencyCode);
            lblExtendedPrice.Text = Currencies.CurrencyToString(extendedPrice, account.CurrencyCode);

        }
        //Instadose Plus prices.
        foreach (GridViewRow rowitem in gvInstadosePlus.Rows)
        {
            // Get each line item's grid row and update the price
            HiddenField productGroupID = (HiddenField)rowitem.FindControl("hfProductGroupID");
            unitPrice = 0;
            tmpInt = 0;

            // Get the price per line.
            if (productGroupID != null)
            {
                if (int.TryParse(productGroupID.Value, out tmpInt)) unitPrice = getUnitPrice(tmpInt);
            }
            

            TextBox txtItemQty = (TextBox)rowitem.Cells[2].FindControl("txtQty");
            Label lblPrice = (Label)rowitem.Cells[3].FindControl("lblPrice");
            Label lblExtendedPrice = (Label)rowitem.Cells[4].FindControl("lblExtended");

            int itemQty = 0;
            if (!int.TryParse(txtItemQty.Text, out itemQty)) txtItemQty.Text = "";
            if (itemQty < 0)
            {
                itemQty = 0;
                txtItemQty.Text = "";
            }
            decimal extendedPrice = itemQty * unitPrice;

            // Set the price per line
            lblPrice.Text = Currencies.CurrencyToString(unitPrice, account.CurrencyCode);
            lblExtendedPrice.Text = Currencies.CurrencyToString(extendedPrice, account.CurrencyCode);
        }
        //Accessories prices.
        foreach (GridViewRow rowitem in gvAccessories.Rows)
        {
            // Get each line item's grid row and update the price
            HiddenField productGroupID = (HiddenField)rowitem.FindControl("hfProductGroupID");
            unitPrice = 0;
            tmpInt = 0;

            // Get the price per line.
            if (productGroupID != null) {
                if (int.TryParse(productGroupID.Value, out tmpInt)) unitPrice = getUnitPrice(tmpInt);
            }

            TextBox txtItemQty = (TextBox)rowitem.Cells[2].FindControl("txtQty");
            Label lblPrice = (Label)rowitem.Cells[3].FindControl("lblPrice");
            Label lblExtendedPrice = (Label)rowitem.Cells[4].FindControl("lblExtended");

            int itemQty = 0;
            if (!int.TryParse(txtItemQty.Text, out itemQty)) txtItemQty.Text = "";
            if (itemQty < 0)
            {
                itemQty = 0;
                txtItemQty.Text = "";
            }
            decimal extendedPrice = itemQty * unitPrice;

            // Set the price per line
            lblPrice.Text = Currencies.CurrencyToString(unitPrice, account.CurrencyCode);
            lblExtendedPrice.Text = Currencies.CurrencyToString(extendedPrice, account.CurrencyCode);
        }

        //Instadose Elite Demo prices.
        foreach (GridViewRow rowitem in gvInstadoseEliteDemo.Rows)
        {
            // Get each line item's grid row and update the price
            HiddenField productGroupID = (HiddenField)rowitem.FindControl("hfProductGroupID");
            unitPrice = 0;
            tmpInt = 0;

            // Get the price per line.
            if (productGroupID != null) {
                if (int.TryParse(productGroupID.Value, out tmpInt)) unitPrice = getUnitPrice(tmpInt);
            }

            TextBox txtItemQty = (TextBox)rowitem.Cells[2].FindControl("txtQty");
            Label lblPrice = (Label)rowitem.Cells[3].FindControl("lblPrice");
            Label lblExtendedPrice = (Label)rowitem.Cells[4].FindControl("lblExtended");

            int itemQty = 0;
            if (!int.TryParse(txtItemQty.Text, out itemQty)) txtItemQty.Text = "";
            if (itemQty < 0)
            {
                itemQty = 0;
                txtItemQty.Text = "";
            }
            decimal extendedPrice = itemQty * unitPrice;

            // Set the price per line
            lblPrice.Text = Currencies.CurrencyToString(unitPrice, account.CurrencyCode);
            lblExtendedPrice.Text = Currencies.CurrencyToString(extendedPrice, account.CurrencyCode);
        }

        //Instadose 2 prices.
        foreach (GridViewRow rowitem in gvInstadose2.Rows)
        {
            // Get each line item's grid row and update the price
            HiddenField productGroupID = (HiddenField)rowitem.FindControl("hfProductGroupID");
            unitPrice = 0;
            tmpInt = 0;

            // Get the price per line.
            if (productGroupID != null) {
                if (int.TryParse(productGroupID.Value, out tmpInt)) unitPrice = getUnitPrice(tmpInt);
            }            

            TextBox txtItemQty = (TextBox)rowitem.Cells[2].FindControl("txtQty");
            Label lblPrice = (Label)rowitem.Cells[3].FindControl("lblPrice");
            Label lblExtendedPrice = (Label)rowitem.Cells[4].FindControl("lblExtended");

            int itemQty = 0;
            if (!int.TryParse(txtItemQty.Text, out itemQty)) txtItemQty.Text = "";
            if (itemQty < 0)
            {
                itemQty = 0;
                txtItemQty.Text = "";
            }
            decimal extendedPrice = itemQty * unitPrice;

            // Set the price per line
            lblPrice.Text = Currencies.CurrencyToString(unitPrice, account.CurrencyCode);
            lblExtendedPrice.Text = Currencies.CurrencyToString(extendedPrice, account.CurrencyCode);
        }

        // DA added for ID3
        //Instadose 3 prices.
        foreach (GridViewRow rowitem in gvInstadose3.Rows)
        {
            // Get each line item's grid row and update the price
            HiddenField productGroupID = (HiddenField)rowitem.FindControl("hfProductGroupID");
            unitPrice = 0;
            tmpInt = 0;

            // Get the price per line.
            if (productGroupID != null)
            {
                if (int.TryParse(productGroupID.Value, out tmpInt)) unitPrice = getUnitPrice(tmpInt);
            }

            TextBox txtItemQty = (TextBox)rowitem.Cells[2].FindControl("txtQty");
            Label lblPrice = (Label)rowitem.Cells[3].FindControl("lblPrice");
            Label lblExtendedPrice = (Label)rowitem.Cells[4].FindControl("lblExtended");

            int itemQty = 0;
            if (!int.TryParse(txtItemQty.Text, out itemQty)) txtItemQty.Text = "";
            if (itemQty < 0)
            {
                itemQty = 0;
                txtItemQty.Text = "";
            }
            decimal extendedPrice = itemQty * unitPrice;

            // Set the price per line
            lblPrice.Text = Currencies.CurrencyToString(unitPrice, account.CurrencyCode);
            lblExtendedPrice.Text = Currencies.CurrencyToString(extendedPrice, account.CurrencyCode);
        }
        // Recalculate totals and update the associated labels.
        updateTotals();
    }

    /// <summary>
    /// updateProductGrid - AutoPostBack update GridView QTY, Price, Extended, SubTotal & Total Values
    /// </summary>
    private void updateOrderSummary()
    {
        // Get the price per line.
        decimal unitPrice = 0;
        int tmpInt = 0;
        int iorderDeviceCount = 0;
        //int itotalProductCount = 0;
        int iAvailable = 0;
        int ID1group = 9;

        DataTable dtOrderSummary = new DataTable();

        // Create the review orders datatable to hold the contents of the order.
        dtOrderSummary = new DataTable("Order Summary");

        // Create the columns for the review orders datatable.
        dtOrderSummary.Columns.Add("SKU", Type.GetType("System.String"));
        dtOrderSummary.Columns.Add("ProductName", Type.GetType("System.String"));
        dtOrderSummary.Columns.Add("Color", Type.GetType("System.String"));
        dtOrderSummary.Columns.Add("Price", Type.GetType("System.String"));
        dtOrderSummary.Columns.Add("Quantity", Type.GetType("System.Int32"));
        //dtOrderSummary.Columns.Add("Price", Type.GetType("System.Decimal"));   
        //dtOrderSummary.Columns.Add("SubTotal", Type.GetType("System.Decimal"));
        dtOrderSummary.Columns.Add("SubTotal", Type.GetType("System.String"));

        foreach (GridViewRow rowitem in gvProduct.Rows)
        {
            // Get each line item's grid row and update the price
            HiddenField productGroupID = (HiddenField)rowitem.FindControl("hfProductGroupID");
            Label instaColor = (Label)rowitem.FindControl("lblColor");

            unitPrice = 0;
            tmpInt = 0;

            // Get the price per line.
            if (productGroupID != null) {
                if (int.TryParse(productGroupID.Value, out tmpInt)) unitPrice = getUnitPrice(tmpInt);
            }
            //unitPrice = getUnitPrice(1);


            TextBox txtItemQty = (TextBox)rowitem.Cells[2].FindControl("txtQty");
            Label lblPrice = (Label)rowitem.Cells[3].FindControl("lblPrice");
            Label lblExtendedPrice = (Label)rowitem.Cells[4].FindControl("lblExtended");

            int itemQty = 0;
            if (!int.TryParse(txtItemQty.Text, out itemQty)) txtItemQty.Text = "";
            if (itemQty < 0)
            {
                itemQty = 0;
                txtItemQty.Text = "";
            }
            decimal extendedPrice = itemQty * unitPrice;

            // Set the price
            lblPrice.Text = Currencies.CurrencyToString(unitPrice, account.CurrencyCode);
            lblExtendedPrice.Text = Currencies.CurrencyToString(extendedPrice, account.CurrencyCode);

            if (itemQty > 0)
            {
                // Create a new review order row.
                DataRow dr = dtOrderSummary.NewRow();

                dr["ProductName"] = rowitem.Cells[0].Text;
                dr["Color"] = instaColor.Text;
                dr["SKU"] = rowitem.Cells[2].Text;
                //dr["Price"] = unitPrice;
                //dr["SubTotal"] = extendedPrice;
                dr["Price"] = lblPrice.Text;
                dr["Quantity"] = itemQty;
                dr["SubTotal"] = lblExtendedPrice.Text;

                // Add row to the data table.
                dtOrderSummary.Rows.Add(dr);

                iorderDeviceCount = iorderDeviceCount + itemQty;
            }

        }

        if (this.account.CustomerTypeID == iConsignmentCustomerTypeID && iorderDeviceCount > 0)
        {

            iAvailable = getAvailableCount(this.account.AccountID, ID1group);

            if (iAvailable < iorderDeviceCount)
            {
                this.VisibleErrorMsg("Error: the total quantity for ID 1 Badges count of " + iorderDeviceCount.ToString() + " is greater than the maximum allowable ID 1 count of " + iAvailable.ToString() + "!");
                this.btnCreate.Enabled = false;
                return;
            }
            else
            {
                this.InvisibleErrorMsg();
                this.btnCreate.Enabled = true;
            }


        }

        foreach (GridViewRow rowitem in gvInstadosePlus.Rows)
        {
            // Get each line item's grid row and update the price
            HiddenField productGroupID = (HiddenField)rowitem.FindControl("hfProductGroupID");
            Label instaColor = (Label)rowitem.FindControl("lblColor");

            unitPrice = 0;
            tmpInt = 0;

            // Get the price per line.
            if (productGroupID != null) {
                if (int.TryParse(productGroupID.Value, out tmpInt)) unitPrice = getUnitPrice(tmpInt);
            }

            TextBox txtItemQty = (TextBox)rowitem.Cells[2].FindControl("txtQty");
            Label lblPrice = (Label)rowitem.Cells[3].FindControl("lblPrice");
            Label lblExtendedPrice = (Label)rowitem.Cells[4].FindControl("lblExtended");

            int itemQty = 0;
            if (!int.TryParse(txtItemQty.Text, out itemQty)) txtItemQty.Text = "";
            if (itemQty < 0)
            {
                itemQty = 0;
                txtItemQty.Text = "";
            }
            decimal extendedPrice = itemQty * unitPrice;

            // Set the price
            lblPrice.Text = Currencies.CurrencyToString(unitPrice, account.CurrencyCode);
            lblExtendedPrice.Text = Currencies.CurrencyToString(extendedPrice, account.CurrencyCode);

            if (itemQty > 0)
            {
                // Create a new review order row.
                DataRow dr = dtOrderSummary.NewRow();

                dr["ProductName"] = rowitem.Cells[0].Text;
                dr["Color"] = instaColor.Text;
                dr["SKU"] = rowitem.Cells[2].Text;
                //dr["Price"] = unitPrice;
                //dr["SubTotal"] = extendedPrice;
                dr["Price"] = lblPrice.Text;
                dr["Quantity"] = itemQty;
                dr["SubTotal"] = lblExtendedPrice.Text;

                // Add row to the data table.
                dtOrderSummary.Rows.Add(dr);
            }
        }

        foreach (GridViewRow rowitem in gvInstadose2.Rows)
        {
            // Get each line item's grid row and update the price
            HiddenField productGroupID = (HiddenField)rowitem.FindControl("hfProductGroupID");
            Label instaColor = (Label)rowitem.FindControl("lblColor");

            unitPrice = 0;
            tmpInt = 0;

            // Get the price per line.
            if (productGroupID != null)
            {
                if (int.TryParse(productGroupID.Value, out tmpInt)) unitPrice = getUnitPrice(tmpInt);
            }

            TextBox txtItemQty = (TextBox)rowitem.Cells[2].FindControl("txtQty");
            Label lblPrice = (Label)rowitem.Cells[3].FindControl("lblPrice");
            Label lblExtendedPrice = (Label)rowitem.Cells[4].FindControl("lblExtended");

            int itemQty = 0;
            if (!int.TryParse(txtItemQty.Text, out itemQty)) txtItemQty.Text = "";
            if (itemQty < 0)
            {
                itemQty = 0;
                txtItemQty.Text = "";
            }
            decimal extendedPrice = itemQty * unitPrice;

            // Set the price
            lblPrice.Text = Currencies.CurrencyToString(unitPrice, account.CurrencyCode);
            lblExtendedPrice.Text = Currencies.CurrencyToString(extendedPrice, account.CurrencyCode);

            if (itemQty > 0)
            {
                // Create a new review order row.
                DataRow dr = dtOrderSummary.NewRow();

                dr["ProductName"] = rowitem.Cells[0].Text;
                dr["Color"] = instaColor.Text;
                dr["SKU"] = rowitem.Cells[2].Text;
                //dr["Price"] = unitPrice;
                //dr["SubTotal"] = extendedPrice;
                dr["Price"] = lblPrice.Text;
                dr["Quantity"] = itemQty;
                dr["SubTotal"] = lblExtendedPrice.Text;

                // Add row to the data table.
                dtOrderSummary.Rows.Add(dr);
            }
        }


        //int totalProductCount = 0;
        // gridview instadoseplusCaps 
        foreach (GridViewRow rowitem in gvInstadosePlusCaps.Rows)
        {
            // Get each line item's grid row and update the price
            HiddenField productGroupID = (HiddenField)rowitem.FindControl("hfProductGroupID");
            Label instaColor = (Label)rowitem.FindControl("lblColor");

            unitPrice = 0;
            tmpInt = 0;

            // price is 0 for caps
            // Get the price per line.
            //if (int.TryParse(productGroupID.Value, out tmpInt)) unitPrice = getUnitPrice(tmpInt);

            TextBox txtItemQty = (TextBox)rowitem.Cells[2].FindControl("txtQty_Caps");
            Label lblPrice = (Label)rowitem.Cells[3].FindControl("lblPrice");
            Label lblExtendedPrice = (Label)rowitem.Cells[4].FindControl("lblTotal");

            int itemQty = 0;
            if (!int.TryParse(txtItemQty.Text, out itemQty)) txtItemQty.Text = "";
            if (itemQty < 0)
            {
                itemQty = 0;
                txtItemQty.Text = "";
            }
            decimal extendedPrice = itemQty * unitPrice;

            // Set the price
            lblPrice.Text = Currencies.CurrencyToString(unitPrice, account.CurrencyCode);
            lblExtendedPrice.Text = Currencies.CurrencyToString(extendedPrice, account.CurrencyCode);

            if (itemQty > 0)
            {
                // Create a new review order row.
                DataRow dr = dtOrderSummary.NewRow();

                dr["ProductName"] = rowitem.Cells[0].Text;
                dr["Color"] = instaColor.Text;
                dr["SKU"] = rowitem.Cells[2].Text;
                //dr["Price"] = unitPrice;
                //dr["SubTotal"] = extendedPrice;
                dr["Price"] = lblPrice.Text;
                dr["Quantity"] = itemQty;
                dr["SubTotal"] = lblExtendedPrice.Text;

                // Add row to the data table.
                dtOrderSummary.Rows.Add(dr);
            }
        }


        // id 2 consignment grid
        //totalProductCount = 0;
        foreach (GridViewRow rowitem in gvInstadose2Con.Rows)
        {
            // Get each line item's grid row and update the price
            HiddenField productGroupID = (HiddenField)rowitem.FindControl("hfProductGroupID");
            Label instaColor = (Label)rowitem.FindControl("lblColor");

            unitPrice = 0;
            tmpInt = 0;

            TextBox txtItemQty = (TextBox)rowitem.Cells[3].FindControl("txtID2ConQty");
            Label lblPrice = (Label)rowitem.Cells[4].FindControl("lblPrice");
            Label lblExtendedPrice = (Label)rowitem.Cells[5].FindControl("lblExtended");

            int itemQty = 0;
            if (!int.TryParse(txtItemQty.Text, out itemQty)) txtItemQty.Text = "";
            if (itemQty < 0)
            {
                itemQty = 0;
                txtItemQty.Text = "";
            }
            decimal extendedPrice = itemQty * unitPrice;

            // Set the price
            lblPrice.Text = Currencies.CurrencyToString(unitPrice, account.CurrencyCode);
            lblExtendedPrice.Text = Currencies.CurrencyToString(extendedPrice, account.CurrencyCode);

            if (itemQty > 0)
            {
                // Create a new review order row.
                DataRow dr = dtOrderSummary.NewRow();

                dr["ProductName"] = rowitem.Cells[0].Text;
                dr["Color"] = instaColor.Text;
                dr["SKU"] = rowitem.Cells[2].Text;
                dr["Price"] = lblPrice.Text;
                dr["Quantity"] = itemQty;
                dr["SubTotal"] = lblExtendedPrice.Text;

                // Add row to the data table.
                dtOrderSummary.Rows.Add(dr);
            }
        }


        foreach (GridViewRow rowitem in gvAccessories.Rows)
        {
            // Get each line item's grid row and update the price
            HiddenField productGroupID = (HiddenField)rowitem.FindControl("hfProductGroupID");

            unitPrice = 0;
            tmpInt = 0;

            // Get the price per line.  
            if (productGroupID != null)
            {
                if (int.TryParse(productGroupID.Value, out tmpInt)) unitPrice = getUnitPrice(tmpInt);
            }

            TextBox txtItemQty = (TextBox)rowitem.Cells[2].FindControl("txtQty");
            Label lblPrice = (Label)rowitem.Cells[3].FindControl("lblPrice");
            Label lblExtendedPrice = (Label)rowitem.Cells[4].FindControl("lblExtended");

            int itemQty = 0;
            if (!int.TryParse(txtItemQty.Text, out itemQty)) txtItemQty.Text = "";
            if (itemQty < 0)
            {
                itemQty = 0;
                txtItemQty.Text = "";
            }
            decimal extendedPrice = itemQty * unitPrice;

            // Set the price
            lblPrice.Text = Currencies.CurrencyToString(unitPrice, account.CurrencyCode);
            lblExtendedPrice.Text = Currencies.CurrencyToString(extendedPrice, account.CurrencyCode);

            if (itemQty > 0)
            {
                // Create a new review order row.
                DataRow dr = dtOrderSummary.NewRow();

                dr["ProductName"] = rowitem.Cells[0].Text;
                dr["Color"] = rowitem.Cells[1].Text;
                dr["SKU"] = rowitem.Cells[2].Text;
                //dr["Price"] = unitPrice;
                //dr["SubTotal"] = extendedPrice;
                dr["Price"] = lblPrice.Text;
                dr["Quantity"] = itemQty;
                dr["SubTotal"] = lblExtendedPrice.Text;

                // Add row to the data table.
                dtOrderSummary.Rows.Add(dr);
            }

        }

        foreach (GridViewRow rowitem in gvInstadoseEliteDemo.Rows)
        {
            // Get each line item's grid row and update the price
            HiddenField productGroupID = (HiddenField)rowitem.FindControl("hfProductGroupID");
            Label instaColor = (Label)rowitem.FindControl("lblColor");

            unitPrice = 0;
            tmpInt = 0;

            // Get the price per line.
            if (productGroupID != null)
            {
                if (int.TryParse(productGroupID.Value, out tmpInt)) unitPrice = getUnitPrice(tmpInt);
            }

            TextBox txtItemQty = (TextBox)rowitem.Cells[2].FindControl("txtQty");
            Label lblPrice = (Label)rowitem.Cells[3].FindControl("lblPrice");
            Label lblExtendedPrice = (Label)rowitem.Cells[4].FindControl("lblExtended");

            int itemQty = 0;
            if (!int.TryParse(txtItemQty.Text, out itemQty)) txtItemQty.Text = "";
            if (itemQty < 0)
            {
                itemQty = 0;
                txtItemQty.Text = "";
            }
            decimal extendedPrice = itemQty * unitPrice;

            // Set the price
            lblPrice.Text = Currencies.CurrencyToString(unitPrice, account.CurrencyCode);
            lblExtendedPrice.Text = Currencies.CurrencyToString(extendedPrice, account.CurrencyCode);

            if (itemQty > 0)
            {
                // Create a new review order row.
                DataRow dr = dtOrderSummary.NewRow();

                dr["ProductName"] = rowitem.Cells[0].Text;
                dr["Color"] = instaColor.Text;
                dr["SKU"] = rowitem.Cells[2].Text;
                //dr["Price"] = unitPrice;
                //dr["SubTotal"] = extendedPrice;
                dr["Price"] = lblPrice.Text;
                dr["Quantity"] = itemQty;
                dr["SubTotal"] = lblExtendedPrice.Text;

                // Add row to the data table.
                dtOrderSummary.Rows.Add(dr);
            }
        }

        gvOrderSummary.DataSource = dtOrderSummary;
        gvOrderSummary.DataBind();

    }

    #endregion

    /// <summary>
    /// Get account target price back on ActiveDevcies and RateID
    /// </summary>
    /// <returns>Device Unit Price</returns>
    private decimal getTargetPrice(int productGroupID)
    {
        if (!account.DefaultRateID.HasValue) return 0;

        //find active badges
        int badges = (from a in idc.AccountDevices where a.Active && a.AccountID == account.AccountID select a).Count();

        // find rateID
        int totalQty = (badges == 0) ? 1 : badges;

        decimal unitPrice = (from rd in account.Rate.RateDetails
                             join r in idc.Rates
                             on rd.RateID equals r.RateID
                             where
                                 rd.MinQty <= totalQty && rd.MaxQty >= totalQty && rd.Active
                                 && rd.ProductGroupID == productGroupID
                             select rd.Price).FirstOrDefault();

        var discount = (from ad in account.AccountDiscounts
                        where ad.ProductGroupID == productGroupID && ad.Active
                        select ad.Discount).FirstOrDefault();

        // Add the discount to the unit price
        if (discount != 0) unitPrice = (unitPrice * (1 - discount / 100));

        // Divide the unit price by 4 if quarterly.
        if (account.BillingTermID.Value == 1) unitPrice = unitPrice / 4;

        return unitPrice;
    }

    private DateTime GetContractStartDate()
    {
        // If an account is set up for location billing, then contract start date = location.ContractStartDate. Else then contract start date = account.ContractStartDate
        if (account.UseLocationBilling && location.ContractStartDate.HasValue)
            return location.ContractStartDate.Value;
        else
            return account.ContractStartDate.Value;
    }

    private DateTime GetContractEndDate()
    {
        // If an account is set up for location billing, then contract end date = location.ContractEndDate. Else then contract end date = account.ContractEndDate
        if (account.UseLocationBilling && location.ContractEndDate.HasValue)
            return location.ContractEndDate.Value;
        else
            return account.ContractEndDate.Value;
    }

    /// <summary>
    /// Get pro-rated unit price base on Rate table, service date left and brand
    /// </summary>
    /// <returns>Pro-rated unit price</returns>
    private decimal getUnitPrice(int productGroupID)
    {
        if (orderType == OrderTypes.Replacement || orderType == OrderTypes.NoReplacement) return 25;
        else if (orderType == OrderTypes.NoChargeReplacement) return 0;

        int totalQty = 0;
        int orderQtyTotal = 0;
        bool isMirionTransition = false;

        //find active badges
        int qtyActive = 0;

        if (account.isDemoAccount && account.BrandSourceID == 3)
        {
            qtyActive = idc.AccountDevices.Where(ad => ad.Active && ad.AccountID == account.AccountID && ad.DeviceInventory.Product.ProductGroupID == 2).Count();
        }
        else
        {
            var activeAccountDevices = idc.AccountDevices.Where(ad => ad.Active && ad.AccountID == account.AccountID);

            if (productGroupID == 1 || productGroupID == 9)
                activeAccountDevices = activeAccountDevices.Where(ad => ad.DeviceInventory.Product.ProductGroupID == 1 || ad.DeviceInventory.Product.ProductGroupID == 9);
            else
                activeAccountDevices = activeAccountDevices.Where(ad => ad.DeviceInventory.Product.ProductGroupID == productGroupID);

            qtyActive = activeAccountDevices.Count();

            //if (hfSelectedProductName.Value == "Instadose 1")
            //{
            //    // Grab the account and product info
            //    qtyActive = (from a in account.AccountDevices
            //                 where a.Active
            //                    && (a.DeviceInventory.Product.ProductGroup.ProductGroupName == "Instadose 1" || a.DeviceInventory.Product.ProductGroup.ProductGroupName == "Instadose 1 IMI")
            //                 select a).Count();
            //}
            //else if (hfSelectedProductName.Value == "Instadose Plus")
            //{
            //    // Grab the account and product info
            //    qtyActive = (from a in idc.AccountDevices
            //                 where a.Active
            //                     && a.AccountID == account.AccountID
            //                     && (a.DeviceInventory.Product.ProductGroup.ProductGroupName == "Instadose Plus")
            //                 select a).Count();
            //}
            //else if (hfSelectedProductName.Value == "Instadose 2")
            //{
            //    qtyActive = (from a in idc.AccountDevices
            //                 where a.Active
            //                     && a.AccountID == account.AccountID
            //                     && (a.DeviceInventory.Product.ProductGroupID == 11)
            //                 select a).Count();
            //}
            //else   // instadose 2
            //{
            //    // Grab the account and product info
            //    qtyActive = (from a in idc.AccountDevices
            //                 where a.Active
            //                     && a.AccountID == account.AccountID
            //                     && (a.DeviceInventory.Product.ProductGroup.ProductGroupName == "Instadose 2")
            //                 select a).Count();
            //}
        }

        // Find service days left from today's date to the end of contract date
        TimeSpan diffResult = account.ContractEndDate.Value.Subtract(DateTime.Today);
        if (account.UseLocationBilling && location.ContractEndDate.HasValue)
            diffResult = location.ContractEndDate.Value.Subtract(DateTime.Today);

        int serviceDaysLeft = diffResult.Days;

        // tdo, 5/21/2013. Rate logic for Miiron transition account.
        // If first time order (normally the date creates an order <= TransitionServiceStart), then prorate will base on the length between ContractEndDate and TransitionServiceStart
        // else then prorate will base on the length between ContractEndDate and the date creates an order  
        // Modified by Tdo, 06/26/2017.
        if (account.BrandSourceID.Value == 2 && account.CustomerType.CustomerTypeDesc == "Transition" && account.Orders.Count == 0)
        {
            isMirionTransition = true;

            // Modified by TDO, 06/27/2017.
            // Do not alllow to manually set transaction period start in the order any more by Craig.

            //// Cast the value from a string to a datetime.
            //DateTime transitionServiceStart = DateTime.MinValue;
            //if (DateTime.TryParse(txtTransitionServiceStart.Text, out transitionServiceStart))
            //{
            //    if (DateTime.Today <= transitionServiceStart)
            //    {
            //        // Get the proration period for a transition account.
            //        diffResult = account.ContractEndDate.Value.Subtract(transitionServiceStart);
            //        serviceDaysLeft = diffResult.Days;

            //        // Throw an error when the transition service days is less than 0.
            //        if (serviceDaysLeft <= 0)
            //            throw new Exception("The transition service start date cannot be set after the end date.");
            //    }
            //}
        }

        // Throw an error when the service period has less than 0 days.
        if (serviceDaysLeft <= 0)
        {
            if (hfSelectedProductName.Value != "Renewal" || account.BrandSourceID != 3)
                throw new Exception("This account's contract has expired.  An order cannot be placed until the contract has been updated.");
        }

        if (account.Rate == null) return 0;

        // Find total Order Qty - Instadose 1 & Instadose 1 IMI.
        if (productGroupID == 1 || productGroupID == 9)
        {
            for (int i = 0; i < this.gvProduct.Rows.Count; i++)
            {
                GridViewRow gvRow = gvProduct.Rows[i];
                TextBox findQTY = (TextBox)gvRow.FindControl("txtQty");

                if (findQTY.Text != "")
                {
                    int orderQty = 0;
                    int.TryParse(findQTY.Text, out orderQty);
                    orderQtyTotal += orderQty;
                }
            }
        }
        else if (productGroupID == 10) //else if (productGroupID == 2 || productGroupID == 10)  //Total Instadose Plus on order.//else if (productGroupID == 10 || productGroupID == 11) //else if (productGroupID == 2 || productGroupID == 10)  //Total Instadose Plus on order.
        {
            for (int i = 0; i < this.gvInstadosePlus.Rows.Count; i++)
            {
                GridViewRow gvRow = gvInstadosePlus.Rows[i];
                TextBox findQTY = (TextBox)gvRow.FindControl("txtQty");

                if (findQTY.Text != "")
                {
                    int orderQty = 0;
                    int.TryParse(findQTY.Text, out orderQty);
                    orderQtyTotal += orderQty;
                }
            }
        }
        else if (productGroupID == 11)
        {
            for (int i = 0; i < this.gvInstadose2.Rows.Count; i++)
            {
                GridViewRow gvRow = gvInstadose2.Rows[i];
                TextBox findQTY = (TextBox)gvRow.FindControl("txtQty");

                if (findQTY.Text != "")
                {
                    int orderQty = 0;
                    int.TryParse(findQTY.Text, out orderQty);
                    orderQtyTotal += orderQty;
                }
            }
        }
        else if (productGroupID == 2)
        {
            for (int i = 0; i < this.gvInstadoseEliteDemo.Rows.Count; i++)
            {
                GridViewRow gvRow = gvInstadoseEliteDemo.Rows[i];
                TextBox findQTY = (TextBox)gvRow.FindControl("txtQty");

                if (findQTY.Text != "")
                {
                    int orderQty = 0;
                    int.TryParse(findQTY.Text, out orderQty);
                    orderQtyTotal += orderQty;
                }
            }
        }
        else if (productGroupID == 5 || productGroupID == 6)  //Accessories (InstaLink USB (6) orInstaLink (5) )
        {
            for (int i = 0; i < this.gvAccessories.Rows.Count; i++)
            {
                GridViewRow gvRow = gvAccessories.Rows[i];
                TextBox findQTY = (TextBox)gvRow.FindControl("txtQty");

                if (findQTY.Text != "")
                {
                    int orderQty = 0;
                    int.TryParse(findQTY.Text, out orderQty);
                    orderQtyTotal += orderQty;
                }
            }
        }
        else  //Accessories (ProductGroupID = InstaLink USB (6))
        {

            for (int i = 0; i < this.gvAccessories.Rows.Count; i++)
            {
                GridViewRow gvRow = gvAccessories.Rows[i];
                TextBox findQTY = (TextBox)gvRow.FindControl("txtQty");

                if (findQTY.Text != "")
                {
                    int orderQty = 0;
                    int.TryParse(findQTY.Text, out orderQty);
                    orderQtyTotal += orderQty;
                }
            }
        }

        totalQty = qtyActive + orderQtyTotal;

        if (totalQty == 0) totalQty = 1;

        decimal unitPrice = 0.00M;

        //Get Rate(s) for the product group.
        var detail = (from r in account.Rate.RateDetails
                      join rd in idc.Rates
                      on r.RateID equals rd.RateID
                      where r.Active
                      & r.ProductGroupID == productGroupID
                      select r).ToList();

        // If account rate details has multiple pricing, then 
        // use correct rate based on total qty of the order.
        if (detail.Count > 1)
        {
            unitPrice = (from r in detail
                         where r.MinQty <= totalQty
                         && r.MaxQty >= totalQty & r.Active
                         & r.ProductGroupID == productGroupID
                         orderby r.MaxQty descending
                         select r.Price).FirstOrDefault();
        }
        else //single rate for this product group.
        {
            unitPrice = (from r in detail select r.Price).FirstOrDefault();
        }

        //Apply Discount% to unitPrice
        decimal acctDiscount = (from ad in account.AccountDiscounts
                                where ad.ProductGroupID == productGroupID && ad.Active
                                select ad.Discount).FirstOrDefault();

        if (acctDiscount > 0)
        {
            // Apply a discount percentage to the price.
            unitPrice = (unitPrice * (1 - (decimal)acctDiscount / 100));
        }

        bool doNotProrate = false;
        // Prorate during the grace period.
        // The grace period is a 30 day window after the account has been created,		
        // Modified by TDO, 06/27/2017. Fixed Pro-Rate issue
        if (isMirionTransition)
        {
            if (account.CreatedDate.AddDays(30) <= DateTime.Now)
                doNotProrate = true;
        }

        // Do not prorate for IC Care ever or for first New order
        if (account.BrandSourceID.Value == 3 || this.isFirstOrder)
        {
            doNotProrate = true;
        }

        // Return the straight amount if not prorated.
        if (doNotProrate)
        {
            if (account.BillingTermID.Value == 2)
                return Math.Round(unitPrice, 2);
            else
                return Math.Round(unitPrice / 4, 2);
        }


        // ------------------- Unit price for none ICCare acts with New and Add-On order -------------------------//

        if (this.isNoneICCareAddOn) // none ICCare Add on. Full quarterly pro-ration
        {
            // get the quarterly price.
            decimal quarterlyPrice = unitPrice / 4;

            // If yearly pricing
            if (account.BillingTermID.Value == 2)
            {
                DateTime startQuarterDate;
                int qtrNo = Common.calculateNumberOfQuarterService(GetContractStartDate(), GetContractEndDate(), DateTime.Parse(radProratePeriod.SelectedItem.Value), out startQuarterDate);
                // Prorate the quarterly price. 
                unitPrice = quarterlyPrice * qtrNo;
            }
            else
            {
                // Do not prorate. Full quarter
                unitPrice = quarterlyPrice;
            }
        }
        else  // First New order. -------- Since no pro-rate for first New order then no longer steps in this Else statements 
        {
            // If yearly pricing
            if (account.BillingTermID.Value == 2)
            {
                // Prorate the quarterly price. 
                if (serviceDaysLeft < 365) unitPrice = unitPrice * serviceDaysLeft / 365;
            }
            else
            {
                // get the quarterly price.
                decimal quarterlyPrice = unitPrice / 4;

                if (serviceDaysLeft >= 365)
                {
                    // Do not prorate.
                    unitPrice = quarterlyPrice;
                }
                else
                {
                    // Get the days remaining in the quarter by getting the next bill date and subtracting the current date from it.
                    // If there is no next bill date get the contract end date.
                    ScheduledBilling billing = (from sb in account.ScheduledBillings
                                                where
                                                    sb.OrderID == null && sb.SchedBillingDate > DateTime.Now
                                                orderby sb.SchedBillingDate ascending
                                                select sb).FirstOrDefault();

                    // Assume there isn't another scheduled billing.
                    int daysLeftInQuarter = serviceDaysLeft;

                    // Get the days left in the quarter from the next billing.
                    if (billing != null) daysLeftInQuarter = billing.SchedBillingDate.Subtract(DateTime.Now).Days;

                    // Get the number of days left in the quarter divided by the quarter, times the quarterly unit price.
                    if (daysLeftInQuarter >= 91) // only if there are less days
                        unitPrice = quarterlyPrice;
                    else
                        unitPrice = (decimal)daysLeftInQuarter / 91M * quarterlyPrice;
                }
            }
        }

        return Math.Round(unitPrice, 2);

    }

    /// <summary>
    /// Calculate the totals for an order and return them as reference variables.
    /// </summary>
    /// <param name="subtotal"></param>
    /// <param name="shippingCost"></param>
    /// <param name="couponDiscount"></param>
    /// <param name="grandTotal"></param>
    /// <returns></returns>
    private bool getOrderTotals(out decimal subtotal, out decimal? shippingCost,
        out decimal couponDiscount, out decimal grandTotal)
    {
        subtotal = 0.0M;
        couponDiscount = 0.0M;
        grandTotal = 0.0M;
        shippingCost = 0.0M;

        try
        {
            int totalUnits = 0;
            int tmpInt = 0;
            int instadose1Total = 0;
            int instadosePlusTotal = 0;
            int instadose2Total = 0;
            int instadosePlusTabTotal = 0;
            int accessoriesTotal = 0;
            int instadoseEliteDemoTotal = 0;
            int productTotal = 0;

            // Set the unit price for the badges
            decimal productUnitPrice = 0;

            // Loop through the grid to find the quantity order for each badge.
            foreach (GridViewRow rowitem in gvProduct.Rows)
            {
                HiddenField prodGroupID = (HiddenField)rowitem.FindControl("hfProductGroupID");

                if (int.TryParse(prodGroupID.Value, out tmpInt))
                    productUnitPrice = getUnitPrice(tmpInt);
                //productUnitPrice = getUnitPrice(1);

                TextBox txtItemQty = (TextBox)rowitem.Cells[2].FindControl("txtQty");

                int itemQty = 0;
                int.TryParse(txtItemQty.Text, out itemQty);
                if (itemQty < 0) itemQty = 0;

                instadose1Total += itemQty;

                subtotal += (itemQty * productUnitPrice);
                totalUnits += itemQty;
            }


            // renewal grid
            // Loop through the grid to find the quantity order for each badge.
            foreach (GridViewRow rowitem in gvRenewal.Rows)
            {

                //HiddenField prodGroupID = (HiddenField)rowitem.Cells[1].FindControl("hfProductGroupID");

                //if (int.TryParse(prodGroupID.Value, out tmpInt))
                //    productUnitPrice = getUnitPrice(tmpInt);
                //productUnitPrice = getUnitPrice(1);


                TextBox txtItemQty = (TextBox)rowitem.Cells[2].FindControl("txtQty");

                Label lPrice = (Label)rowitem.Cells[3].FindControl("lblPrice");

                int itemQty = 0;
                int.TryParse(txtItemQty.Text, out itemQty);
                if (itemQty < 0) itemQty = 0;

                decimal.TryParse(lPrice.Text.Split(' ')[0], out productUnitPrice);

                instadose1Total += itemQty;

                subtotal += (itemQty * productUnitPrice);
                totalUnits += itemQty;
            }


            // instadoseplus Caps grid
            // Loop through the grid to find the quantity order for each badge.
            foreach (GridViewRow rowitem in gvInstadosePlusCaps.Rows)
            {

                TextBox txtItemQty = (TextBox)rowitem.Cells[3].FindControl("txtQty_Caps");

                Label lPrice = (Label)rowitem.Cells[4].FindControl("lblPrice");

                int itemQty = 0;
                int.TryParse(txtItemQty.Text, out itemQty);
                if (itemQty < 0) itemQty = 0;

                decimal.TryParse(lPrice.Text.Split(' ')[0], out productUnitPrice);

                instadosePlusTotal += itemQty;

                subtotal += (itemQty * productUnitPrice);
                totalUnits += itemQty;
            }


            // instadose 2 consignment grid
            // Loop through the grid to find the quantity order for each badge.

            foreach (GridViewRow rowitem in gvInstadose2Con.Rows)
            {

                TextBox txtItemQty = (TextBox)rowitem.Cells[3].FindControl("txtID2ConQty");

                Label lPrice = (Label)rowitem.Cells[4].FindControl("lblPrice");

                int itemQty = 0;
                int.TryParse(txtItemQty.Text, out itemQty);
                if (itemQty < 0) itemQty = 0;

                decimal.TryParse(lPrice.Text.Split(' ')[0], out productUnitPrice);

                instadose2Total += itemQty;

                subtotal += (itemQty * productUnitPrice);
                totalUnits += itemQty;
            }


            // Loop through the grid to find the quantity order for each badge.
            foreach (GridViewRow rowitem in gvInstadosePlus.Rows)
            {
                HiddenField prodGroupID = (HiddenField)rowitem.FindControl("hfProductGroupID");

                if (int.TryParse(prodGroupID.Value, out tmpInt))
                    productUnitPrice = getUnitPrice(tmpInt);

                TextBox txtItemQty = (TextBox)rowitem.Cells[2].FindControl("txtQty");

                int itemQty = 0;
                int.TryParse(txtItemQty.Text, out itemQty);
                if (itemQty < 0) itemQty = 0;

                instadosePlusTotal += itemQty;

                subtotal += (itemQty * productUnitPrice);
                totalUnits += itemQty;
            }

            // Loop through the grid to find the quantity order for each badge.
            foreach (GridViewRow rowitem in gvAccessories.Rows)
            {
                HiddenField prodGroupID = (HiddenField)rowitem.FindControl("hfProductGroupID");
                HiddenField productID = (HiddenField)rowitem.FindControl("hfProductID");

                if (int.TryParse(prodGroupID.Value, out tmpInt))
                    productUnitPrice = getUnitPrice(tmpInt);

                TextBox txtItemQty = (TextBox)rowitem.Cells[2].FindControl("txtQty");

                int itemQty = 0;
                int.TryParse(txtItemQty.Text, out itemQty);
                if (itemQty < 0) itemQty = 0;

                accessoriesTotal += itemQty;

                subtotal += (itemQty * productUnitPrice);
                totalUnits += itemQty;
            }

            // Loop through the grid to find the quantity order for each badge.
            foreach (GridViewRow rowitem in gvInstadoseEliteDemo.Rows)
            {
                HiddenField prodGroupID = (HiddenField)rowitem.FindControl("hfProductGroupID");

                if (int.TryParse(prodGroupID.Value, out tmpInt))
                    productUnitPrice = getUnitPrice(tmpInt);

                TextBox txtItemQty = (TextBox)rowitem.Cells[2].FindControl("txtQty");

                int itemQty = 0;
                int.TryParse(txtItemQty.Text, out itemQty);
                if (itemQty < 0) itemQty = 0;

                instadoseEliteDemoTotal += itemQty;

                subtotal += (itemQty * productUnitPrice);
                totalUnits += itemQty;
            }

            // Loop through the grid to find the quantity order for each badge.
            foreach (GridViewRow rowitem in gvInstadose2.Rows)
            {
                HiddenField prodGroupID = (HiddenField)rowitem.FindControl("hfProductGroupID");

                if (int.TryParse(prodGroupID.Value, out tmpInt))
                    productUnitPrice = getUnitPrice(tmpInt);

                TextBox txtItemQty = (TextBox)rowitem.Cells[2].FindControl("txtQty");

                int itemQty = 0;
                int.TryParse(txtItemQty.Text, out itemQty);
                if (itemQty < 0) itemQty = 0;

                instadose2Total += itemQty;

                subtotal += (itemQty * productUnitPrice);
                totalUnits += itemQty;
            }

            // Calculate the shipping total.
            string shippingState = ddlShippingState.SelectedItem.Text;
            string shippingCountry = (from c in idc.Countries where c.CountryID == int.Parse(ddlShippingCountry.SelectedItem.Value) select c.PayPalCode).FirstOrDefault();
            string shippingPostalCode = txtShippingPostalCode.Text;
            //int shippingMethod = int.Parse(ddlShippingMethod.SelectedItem.Value);
            int shippingMethod;
            if (MalvernIntegration)   // Load the shipping method drop down for Malvern - hbabu 05/12/14. 
            {
                shippingMethod = int.Parse(ddlShippingMethodMalvern.SelectedItem.Value);
                if (ddlShippingMethodMalvern.SelectedItem.Text.ToLower().Contains("free") || ddlShippingMethodMalvern.SelectedItem.Text.ToLower().Contains("renewal"))
                {
                    shippingMethod = 0;
                }
            }
            else
            {
                shippingMethod = int.Parse(ddlShippingMethod.SelectedItem.Value);
            }
            this.lblShippingState.Text = shippingState;

            if (MalvernIntegration)   // Load the shipping method drop down for Malvern - hbabu 05/12/14. 
            {
                try
                {
                    if (shippingMethod != 0)
                    {
                        //Based on the Preferred Shiping need to call either Malvern API or Database for Flat Rates - Hbabu 04/23/14
                        defaultShippingCarrier = (from a in idc.Accounts where a.AccountID == this.account.AccountID select a.ShippingCarrier).FirstOrDefault();
                        if (defaultShippingCarrier == null || defaultShippingCarrier.Length == 0)
                        {
                            //Call database for Flat Rates
                            shippingCost = OrderHelpers.GetShippingFlatRates(shippingState, shippingCountry, shippingPostalCode, ddlShippingMethodMalvern.SelectedItem.Value, totalUnits);
                        }
                        else
                        {
                            //Get Carrier Code and Shipping Method code from Database to submit for Malavern API           
                            ShippingMethod method = (from sm in idc.ShippingMethods where sm.ShippingMethodID == shippingMethod select sm).FirstOrDefault();
                            //Calculate weight
                            decimal weight = 0.2M * totalUnits;

                            //Get 3 Character Country Code based on the Country ID
                            // Tdo, 9/8/2014. Using numericcountrycode instead of countrycode
                            //string sCountryCode = (from c in idc.Countries where c.CountryID == Convert.ToUInt32(ddlShippingCountry.SelectedItem.Value) select c.CountryCode).FirstOrDefault(); 
                            Country sCountry = (from c in idc.Countries where c.CountryID == Convert.ToUInt32(ddlShippingCountry.SelectedItem.Value) select c).FirstOrDefault();
                            string nCountryCode = string.IsNullOrEmpty(sCountry.NumericCountryCode) ? sCountry.CountryCode : sCountry.NumericCountryCode;

                            //Call Malvern API for rates 
                            MalvernGetRates MRates = new MalvernGetRates();
                            //string request = "0,\"003\"1,\"" + lblAccountID.Text + "\"12,\"" + txtShippingFirstName.Text + " " + txtShippingLastName.Text + "\"11,\"" + txtShippingCompany.Text + "\"13,\"" + txtShippingAddress1.Text + "\"14,\"" + txtShippingAddress2.Text + "\"15,\"" + txtShippingCity.Text + "\"16,\"" + ddlShippingState.SelectedItem.Text + "\"17,\"" + txtShippingPostalCode.Text + "\"50,\"" + sCountryCode + "\"18,\"\"21,\"" + weight + "\"1033,\"" + method.CarrierCode + " " + method.ShippingMethodCode + "\"99,\"\"";
                            string request = "0,\"003\"1,\"" + lblAccountID.Text + "\"12,\"" + txtShippingFirstName.Text + " " + txtShippingLastName.Text + "\"11,\"" + txtShippingCompany.Text + "\"13,\"" + txtShippingAddress1.Text + "\"14,\"" + txtShippingAddress2.Text + "\"15,\"" + txtShippingCity.Text + "\"16,\"" + ddlShippingState.SelectedItem.Text + "\"17,\"" + txtShippingPostalCode.Text + "\"50,\"" + nCountryCode + "\"18,\"\"21,\"" + weight + "\"1033,\"" + method.CarrierCode + " " + method.ShippingMethodCode + "\"99,\"\"";
                            System.Diagnostics.Debug.WriteLine(request);
                            string MalvernIPAddress = (from a in idc.AppSettings where a.AppKey == "MalvernIPAddress" select a.Value).FirstOrDefault();
                            string Malvernport = (from a in idc.AppSettings where a.AppKey == "MalvernPortAddress" select a.Value).FirstOrDefault();
                            shippingCost = MRates.MalvernGetRatesInfo(request, MalvernIPAddress, Malvernport);
                        }
                    }
                    else
                    {
                        shippingCost = 0.00M;
                    }
                }
                catch (Exception e)
                {
                    Instadose.Basics.WriteLogEntry("Error While Processing Malavern " + e.Message, "Malvern", "Portal.instadose.com.createorder.aspx.cs.getOrderTotals", Basics.MessageLogType.Critical);
                }
            }
            else
            {
                // Get the shipping cost and determine if it's valid or not.
                shippingCost = OrderHelpers.GetShippingCost(shippingState, shippingCountry, shippingPostalCode, shippingMethod, totalUnits);
            }

            // Get the shipping cost and determine if it's valid or not.
            //shippingCost = OrderHelpers.GetShippingCost(shippingState, shippingCountry, shippingPostalCode, shippingMethod, totalUnits);

            // If the shipping cost does have a value show it.
            if (shippingCost.HasValue)
            {
                string additionalShippingChargePct;

                if (MalvernIntegration)
                    additionalShippingChargePct = (from a in idc.AppSettings
                                                   where a.AppKey == "AdditionalMalvernShippingChargePct"
                                                   select a.Value).FirstOrDefault();
                else
                    additionalShippingChargePct = (from a in idc.AppSettings
                                                   where a.AppKey == "AdditionalShippingChargePct"
                                                   select a.Value).FirstOrDefault();

                if (!string.IsNullOrEmpty(additionalShippingChargePct))
                {
                    shippingCost = shippingCost.Value * (1 + (Convert.ToDecimal(additionalShippingChargePct) / 100));
                }

            }

            // Set the grand total.
            grandTotal = subtotal + (shippingCost.HasValue ? shippingCost.Value : 0) - couponDiscount;

            //store item totals for products tab and order summary tab - product grid join table label.
            instadosePlusTabTotal = instadosePlusTotal + accessoriesTotal;
            productTotal = instadose1Total + instadosePlusTotal + accessoriesTotal + instadoseEliteDemoTotal + instadose2Total;

            //totals for Products tab - join table labels.
            this.tabInsta1.InnerHtml = string.Format("Instadose 1 ({0})", instadose1Total);
            this.tabInstaPlus.InnerHtml = string.Format("Instadose Plus ({0})", instadosePlusTabTotal);
            this.tabInsta2.InnerHtml = string.Format("Instadose 2 ({0})", instadose2Total);
            //Order summary - product totals for grid join table labels.  
            this.lblInstaTotal.Text = string.Format("Instadose 1 ({0})", instadose1Total);
            this.lblInstaPlusTotal.Text = string.Format("Instadose Plus ({0})", instadosePlusTotal);
            this.lblAccessTotal.Text = string.Format("Accessories ({0})", accessoriesTotal);
            this.lblProdOrdered.Text = string.Format("Products Ordered ({0})", productTotal);
            this.lblInsta2Total.Text = string.Format("Instadose 2 ({0})", instadose2Total);

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Update the charges on the shipping & order total section
    /// </summary>
    private void updateTotals()
    {
        decimal subtotal = 0.00M;
        decimal shippingTotal = 0.00M;
        decimal couponDiscount = 0.00M;
        decimal grandTotal = 0.00M;
        decimal? shippingCost;

        // Get the totals
        if (!getOrderTotals(out subtotal, out shippingCost, out couponDiscount, out grandTotal))
            throw new Exception("An error occurred while calculating the order totals.");

        // If the cost does have a value show it.
        if (shippingCost.HasValue)
        {
            lblShippingChargeError.Visible = false;
            lblMalvernShippingChargeError.Visible = false;
            shippingTotal = shippingCost.Value;
        }
        else
        {
            // Display an inline error message.
            /*lblShippingChargeError.Text = string.Format("'{0}' is not available.", ddlShippingMethod.SelectedItem.Text);
            lblShippingChargeError.Visible = true;
            ddlShippingMethod.SelectedIndex = 0;
            shippingTotal = 0;
             */

            if (MalvernIntegration)   // Implemented for Malvern Integration Hbabu - 05/12/14
            {
                // Display an inline error message.
                lblMalvernShippingChargeError.Text = string.Format("'{0}' is not available.", ddlShippingMethodMalvern.SelectedItem.Text);
                lblMalvernShippingChargeError.Visible = true;
                ddlShippingMethodMalvern.SelectedIndex = 0;
                shippingTotal = 0;
            }
            else
            {
                // Display an inline error message.
                lblShippingChargeError.Text = string.Format("'{0}' is not available.", ddlShippingMethod.SelectedItem.Text);
                lblShippingChargeError.Visible = true;
                ddlShippingMethod.SelectedIndex = 0;
                shippingTotal = 0;
            }
        }

        // Display the results.
        lblSubTotal.Text = Currencies.CurrencyToString(subtotal, account.CurrencyCode);
        lblOrderSubtotal.Text = lblSubTotal.Text;

        lblShippingCharge.Text = Currencies.CurrencyToString(shippingTotal, account.CurrencyCode);
        this.lblOrderShipping.Text = lblShippingCharge.Text;

        lblCouponDiscountAmount.Text = Currencies.CurrencyToString(couponDiscount, account.CurrencyCode);
        lblOrderTotal.Text = Currencies.CurrencyToString(grandTotal, account.CurrencyCode);
        lblOrderSummaryTotal.Text = lblOrderTotal.Text;

        updateOrderSummary();

    }

    /// <summary>
    /// Create or update an order's properties.
    /// </summary>
    /// <param name="orderID"></param>
    /// <returns></returns>
    private int ProcessOrder()
    {
        string errorMsg = "";
        if (!OrderPassValidation(ref errorMsg))
        {
            throw new Exception(errorMsg);
        }

        // If the order has not been created...
        if (order == null)
        {
            order = new Order();
            idc.Orders.InsertOnSubmit(order);
        }
        else
        {
            order = idc.Orders.Where(o => o.OrderID == order.OrderID && o.OrderStatusID != 10).FirstOrDefault();
            if (order == null) throw new Exception("The order ID provided does not exist.");

            // Purge existing order details
            idc.OrderDetails.DeleteAllOnSubmit(order.OrderDetails);
        }

        int tmpInt = 0;
        decimal subtotal = 0.00M;
        decimal couponDiscount = 0.00M;
        decimal grandTotal = 0.00M;
        decimal? shippingCost;
        decimal instadoseUnitPrice = 0;
        decimal instadose2UnitPrice = 0;
        decimal instadose3UnitPrice = 0;
        decimal instalinkUnitPrice = 0;
        decimal instalinkUSBUnitPrice = 0;

        int totalQuantity = 0;
        int instadoseTotalQuantity = 0;
        int instadose2TotalQuantity = 0;
        int instadose3TotalQuantity = 0;
        int instalinkTotalQuantity = 0;
        int instalinkUSBTotalQuantity = 0;
        bool selectedInstadosePlus = false;
        bool selectedInstadosePlusCaps = false;
        bool selectedInstadose2 = false;
        bool selectedInstadose2Covers = false;
        bool selectedInstadose3 = false;
        bool selectedInstadose3Covers = false;

        // Fill the order total variables.
        if (!getOrderTotals(out subtotal, out shippingCost, out couponDiscount, out grandTotal))
            throw new Exception("An error occurred while calculating the order totals.");

        // Delete any payments made so far--
        idc.Payments.DeleteAllOnSubmit(order.Payments);

        // Create a payment record.
        Payment payment = new Payment()
        {
            Authorized = true,
            PaymentDate = DateTime.Now
        };

        //// Process the credit orders
        //if (rblstPayMethod.SelectedValue == "Credit Card")
        //{
        //	int accountCreditCardID = 0;
        //	int expMonth = 0;
        //	int expYear = 0;

        //	int.TryParse(ddlCCMonth.SelectedValue, out expMonth);
        //	int.TryParse(ddlCCYear.SelectedValue, out expYear);

        //	if (expMonth == 0 || expYear == 0)
        //		throw new Exception("The credit card expiration month or year was not set.");
        //	else
        //	{
        //		int lastDayOfMonth = DateTime.DaysInMonth(expYear, expMonth);
        //		DateTime ccExpiredDate = Convert.ToDateTime(expMonth.ToString() + "/" + lastDayOfMonth.ToString() + "/" + expYear.ToString());
        //		if (ccExpiredDate < DateTime.Today)
        //			throw new Exception("The credit card expiration is expired.");
        //	}

        //	// Determine if the credit card needs to be added.
        //	if (txtCCno.Text != string.Empty && !txtCCno.Text.Contains("*"))
        //	{
        //		accountCreditCardID = OrderHelpers.AddCreditCard(account.AccountID, txtCCName.Text.Trim(), txtCCno.Text.Trim(),
        //			   txtCCcvc.Text.Trim(), expMonth, expYear);
        //	}

        //	// Get the current Account Credit Card DataObject and AccountCreditCardID.
        //	accountcreditcard = GetAccountCreditCard(account);
        //	accountCreditCardID = accountcreditcard.AccountCreditCardID;

        //	// If the expiration date has changed... update the account.
        //	if (accountcreditcard != null && (expMonth != accountcreditcard.ExpMonth || expYear != accountcreditcard.ExpYear))
        //	{
        //		// Update the credit card number.
        //		OrderHelpers.UpdateCreditCardExpiration(accountcreditcard.AccountCreditCardID, expMonth, expYear);
        //	}

        //	// Store the response from the transaction.
        //	string transactionID = "";
        //	string transactionResponse = "";

        //	if (OrbitalIntegration)
        //	{
        //		// Process Orbital credit card authorization.
        //		OrderHelpers.ProcessOrbitalAuthorization(accountCreditCardID, out transactionID, out transactionResponse);

        //		// Throw an error if the credit card can't be authorized.
        //		if (transactionID.Length == 0)
        //			throw new Exception("Credit Card Authorization Error: " + transactionResponse);
        //	}

        //	// Set the payment
        //	payment.PaymentMethodID = 1; // Credit Card
        //	payment.TransCode = transactionID;
        //	payment.GatewayResponse = transactionResponse;
        //	payment.AccountCreditCardID = accountCreditCardID;
        //	payment.Captured = false;
        //	payment.Amount = 0.01M;
        //	payment.CreatedBy = this.userName;
        //	payment.ApplicationID = 2;
        //	payment.CurrencyCode = ddlCurrency.SelectedValue;             
        //}
        //else
        //{
        // Set the payment
        payment.PaymentMethodID = 2; // Purchase Order
        payment.GatewayResponse = "POPAYMENT";
        payment.Captured = true;
        payment.Amount = grandTotal;
        payment.CreatedBy = this.userName;
        payment.ApplicationID = 2;
        payment.CurrencyCode = ddlCurrency.SelectedValue;
        //}
        // Associate the payment to the order.
        order.Payments.Add(payment);

        // If the brand is IC Care, then a PO is required.
        int orderStatusID = (account.BrandSourceID.Value == 3) ? 1 : 2;
        bool poIsRequired = (account.BrandSourceID.Value == 3);

        // Set the coupon
        int couponID = int.Parse(ddlCoupon.SelectedValue);
        if (couponID != 0) order.CouponID = couponID;

        // Set the package type
        int packageTypeID = int.Parse(ddlPackageType.SelectedValue);
        if (packageTypeID != 0) order.PackageTypeID = packageTypeID;

        // Set the shipping option
        int shippingOptionID = int.Parse(ddlShippingOption.SelectedValue);
        if (shippingOptionID != 0) order.ShippingOptionID = shippingOptionID;

        string orderTypeStr = "NEW";
        if (order.OrderID == 0)
        {
            switch (orderType)
            {
                case OrderTypes.Replacement:
                case OrderTypes.NoChargeReplacement:
                    orderTypeStr = "LOST REPLACEMENT";
                    break;
                case OrderTypes.NoReplacement:
                    orderTypeStr = "NO REPLACEMENT";
                    break;
            }
            // Should this really be called an addon order?
            if (orderType == OrderTypes.New && account.Orders.Count >= 1)
                orderTypeStr = "ADDON";
            if (account.CustomerTypeID == iConsignmentCustomerTypeID)
                orderTypeStr = "INV";
            //if (radProductName.SelectedValue == "Instadose Elite Demo")
            //    orderTypeStr = "DEMO";
        }
        else
        {
            // Do not change the order type once the order has been placed.
            orderTypeStr = order.OrderType;
        }

        // Set the order properties.
        order.AccountID = account.AccountID;
        order.OrderType = orderTypeStr;
        order.PONumber = txtPOno.Text;
        order.LocationID = location.LocationID;
        order.BrandSourceID = account.BrandSourceID.Value;
        //order.ShippingMethodID = int.Parse(ddlShippingMethod.SelectedValue);
        if (MalvernIntegration) //Implemented for Malvern Integration - Hbabu 05/12/14
        {
            order.ShippingMethodID = int.Parse(ddlShippingMethodMalvern.SelectedValue);
        }
        else
        {
            order.ShippingMethodID = int.Parse(ddlShippingMethod.SelectedValue);
        }
        order.OrderDate = DateTime.Now;
        order.CreatedBy = this.userName; //ActiveDirectoryQueries.GetUserName();


        order.OrderStatusID = orderStatusID;
        order.ReferralCode = ddlReferral.SelectedItem.Value.ToString();
        order.SoftraxStatus = false;
        order.CurrencyCode = ddlCurrency.SelectedValue;
        order.Rate = account.Rate;
        order.Discount = account.Discount; //account.Discount.Value;
        order.OrderSource = "Business App";
        order.SpecialInstructions = txtSpecialInstruction.Text;
        order.PORequired = poIsRequired;
        order.ShippingCharge = shippingCost.HasValue ? shippingCost.Value : 0.00M;
        order.ContractStartDate = GetContractStartDate();
        order.ContractEndDate = GetContractEndDate();

        // Look for the correct shipping method.
        // var offsetDays = Common.getShippingOffsetDays(order.ShippingMethodID);
        // Stx fields are the actual value sent to softrax.

        // if ic care do not update the the contract dates
        if (order.BrandSourceID != 3 || order.OrderType != "RENEWAL")
        {
            //if (this.isFirstOrder)
            //    order.StxContractStartDate = GetContractStartDate();
            //else
            //    order.StxContractStartDate = DateTime.Now.Date.AddDays(offsetDays);

            order.StxContractStartDate = GetDefaultSoftTraxStartDate(this.isFirstOrder, DateTime.Now);

            order.StxContractEndDate = GetContractEndDate();
        }

        // if a quarterly account set the account date to the end of the quarter
        if (account.BillingTermID == 1)
        {
            // Set the contract end date to the quarter.
            order.StxContractEndDate = Common.calculateQuarterlyServiceEnd(order.ContractStartDate, order.ContractEndDate, order.StxContractStartDate.Value);
        }

        if (this.isNoneICCareAddOn && order.OrderType != "RENEWAL")
        {
            string proRatePeriod = radProratePeriod.SelectedItem.Text;
            string[] startEndProRatePeriod = proRatePeriod.Split('-');

            order.StxContractStartDate = DateTime.Parse(startEndProRatePeriod[0].Trim());
            order.StxContractEndDate = DateTime.Parse(startEndProRatePeriod[1].Trim());
        }

        //// Add business logic for service start date for IC Care orders that are not replacement.
        //if (account.BrandSourceID.Value == 3 && orderType != OrderTypes.Replacement && orderType != OrderTypes.NoReplacement && order.OrderType != "RENEWAL")
        //{
        //	// Set the service start date for an IC Care order.
        //	order.StxContractStartDate = DateTime.Parse(ddlServiceStartDate.SelectedValue);

        //  // Contract date for an IC Care order is ALWAYS 1 year from the order date.
        //  order.StxContractEndDate = order.StxContractStartDate.Value.AddYears(1).AddDays(-1);

        //}

        //if (account.BrandSourceID.Value == 3 && order.OrderType != "RENEWAL")
        if (account.BrandSourceID.Value == 3 && order.OrderType != "RENEWAL" && orderType != OrderTypes.Replacement)
        {
            // Set the service start date for an IC Care order.
            order.StxContractStartDate = DateTime.Parse(ddlServiceStartDate.SelectedValue);

            //if(orderType != OrderTypes.Replacement && orderType != OrderTypes.NoReplacement && orderType != OrderTypes.NoChargeReplacement)
            //{
            //    // Contract date for an IC Care order is ALWAYS 1 year from the order date. 
            //    order.StxContractEndDate = order.StxContractStartDate.Value.AddYears(1).AddDays(-1);
            //}                       
        }
        // Add business logic for Mirion transition accounts.
        else if (account.BrandSourceID.Value == 2 && account.CustomerType.CustomerTypeDesc == "Transition" && account.Orders.Count == 0)
        {
            // Use the transition start date.
            // Modified by TDO, 06/27/2017. Apply grace period logic here to set StxContractStartDate
            // If farthur than 30 days, it Will not Pro-rate so StxContractStartDate will be account.ContractStartDate
            if (account.CreatedDate.AddDays(30) <= DateTime.Now)
            {
                DateTime tmpDate;
                if (DateTime.TryParse(txtTransitionServiceStart.Text, out tmpDate))
                    order.StxContractStartDate = tmpDate;
            }
        }

        // Loop through each row in the grid view to get the items ordered.
        GridView[] grids = new GridView[] { gvProduct, gvInstadosePlus, gvAccessories, gvRenewal, gvInstadosePlusCaps, gvInstadose2, gvInstadose2Con, gvInstadoseEliteDemo, gvInstadose3};

        // Loop through the ID1, ID+, ID2 grids to update the data
        foreach (GridView grid in grids)
        {
            foreach (GridViewRow row in grid.Rows)
            {
                int lineQty = 0;
                int productID = 0;

                TextBox txtQty = null;
                if (grid.ClientID.Contains("gvInstadosePlusCaps"))
                    txtQty = (TextBox)row.FindControl("txtQty_Caps");
                else if (grid.ClientID.Contains("gvInstadose2Con"))
                    txtQty = (TextBox)row.FindControl("txtID2ConQty");
                else
                    txtQty = (TextBox)row.FindControl("txtQty");

                HiddenField hfProductID = (HiddenField)row.FindControl("hfProductID");

                // Added to get Unit Price for /LOST BADGE items.
                // Currencies.RemoveCurrencySymbol(string, string, int) was not working.
                // Coded the functionality the long-way.
                Label lblPrice = (Label)row.FindControl("lblPrice");
                //string strLostBadgePrice = lblPrice.Text;
                string strLostBadgePrice = lblPrice.Text.Split(',')[0];
                strLostBadgePrice = strLostBadgePrice.Replace("$", "");
                strLostBadgePrice = strLostBadgePrice.Replace(ddlCurrency.SelectedValue, "");
                decimal lostBadgePrice = 0;
                decimal.TryParse(strLostBadgePrice, out lostBadgePrice);

                // Skip the row if no items were ordered or the text isn't a number.
                if (!int.TryParse(txtQty.Text, out lineQty) || lineQty == 0) continue;

                // below is for consignment order tracking...
                if (grid.ClientID.Contains("gvInstadosePlus"))
                    selectedInstadosePlus = true;
                if (grid.ClientID.Contains("gvInstadosePlusCaps"))
                    selectedInstadosePlusCaps = true;
                if (grid.ClientID.Contains("gvInstadose2"))
                    selectedInstadose2 = true;
                if (grid.ClientID.Contains("gvInstadose2Con"))
                    selectedInstadose2Covers = true;
                if (grid.ClientID.Contains("gvInstadose3"))
                    selectedInstadose3 = true;
                if (grid.ClientID.Contains("gvInstadose3Caps"))
                    selectedInstadose3Covers = true;

                if (!int.TryParse(hfProductID.Value, out productID) || productID == 0)
                {
                    if (hfSelectedProductName.Value != "Renewal" || account.BrandSourceID != 3)
                        continue;
                }

                // Add to the total quantity.
                totalQuantity += lineQty;

                HiddenField prodGroupID = (HiddenField)row.FindControl("hfProductGroupID");

                tmpInt = 0;
                Label lPrice = null;
                decimal dPrice = 0;

                /****************************************************************************
                        ProductGroupID	ProductGroupName
                        1	            Instadose 1
                        2	            Instadose 2
                        5	            InstaLink
                        6	            InstaLink USB
                        9	            Instadose 1 IMI
                        10	            Instadose Plus
                        11	            Instadose 2 New
                        12	            Label Roll
                        13	            Label Sheet
                        14	            Instadose+ Cap
                        15	            Instadose 2E Bumper
                        16              Label Roll ID2
                        17              Label Sheet ID2
                        18              Instadose 3
                 
                 ***********************************************************************************/

                if (int.TryParse(prodGroupID.Value, out tmpInt))
                {
                    if (account.CustomerTypeID == iConsignmentCustomerTypeID)
                    {
                        instadoseTotalQuantity += lineQty;
                        instadoseUnitPrice = lostBadgePrice;

                    }
                    else if (tmpInt == 9 || tmpInt == 10 || tmpInt == 2)    //productgroupid checking
                    {
                        instadoseTotalQuantity += lineQty;
                        instadoseUnitPrice = getUnitPrice(tmpInt);
                    }
                    else if (tmpInt == 11)  // for ID 2 devices
                    {
                        instadose2TotalQuantity += lineQty;
                        instadose2UnitPrice = getUnitPrice(tmpInt);
                    }
                    else if (tmpInt == Instadose3Constants.ProductGroupId)
                    {
                        instadose3TotalQuantity += lineQty;
                        instadose3UnitPrice = getUnitPrice(tmpInt);
                    }

                }
                else if (hfSelectedProductName.Value == "Renewal" && account.BrandSourceID == 3)
                {

                    instadoseTotalQuantity += lineQty;

                    // check active badges total
                    /* int activeBadgesCount = (from ad in idc.AccountDevices
                                             where ad.AccountID == account.AccountID &&
                                                   ad.CurrentDeviceAssign == true &&
                                                   ad.Active == true
                                             select ad).Count(); */

                    int activeBadgesCount = 0;
                    int.TryParse(lblActiveBadge.Text, out activeBadgesCount);

                    if (instadoseTotalQuantity > activeBadgesCount)
                    {
                        throw new Exception("Error: the quantity amount is greater than the current active Badges total!");
                    }


                    lPrice = (Label)row.FindControl("lblPrice");

                    decimal.TryParse(lPrice.Text.Split(' ')[0], out dPrice);
                    instadoseUnitPrice = dPrice;

                }
                else
                {
                    throw new Exception("An error occured, Order information, could not be saved to the Database, please contact I.T.");
                }

                // For IC Care Demo Account - placing ID Elite Demo order
                //if (radProductName.SelectedValue == "Instadose Elite Demo")
                //if (account.BrandSourceID == 3 && order.OrderType == "DEMO")
                if (account.isDemoAccount && account.BrandSourceID == 3)
                {
                    order.OrderDetails.Add(new OrderDetail()
                    {
                        // Set the SKU price to $0 when not new.
                        // Price = skuPrice,
                        Price = instadoseUnitPrice,
                        ProductID = productID,
                        Quantity = lineQty,
                        OrderDetailDate = DateTime.Now
                    });
                }
                else
                {
                    // Add the color SKU product to orderdetail when not a NoReplacement order and product line are not instalink, instalinkUSB
                    if (orderType != OrderTypes.NoReplacement && (tmpInt == 9 || tmpInt == 10 || tmpInt == 11 ||
                                                                    tmpInt == 2 || tmpInt == 12 || tmpInt == 13
                                                                    || tmpInt == 16 || tmpInt == 17 || tmpInt == Instadose3Constants.ProductGroupId))
                    {
                        //decimal skuPrice = 0;
                        //// SKU price may vary
                        //skuPrice = instadoseUnitPrice;

                        //// Price to $0 when not new or Instadose 1IMI, Instadose PlUS, Instadose 2
                        //if (orderType != OrderTypes.New || tmpInt == 9 || tmpInt == 10 || tmpInt == 2) skuPrice = 0;

                        // Add the color cover,cap, or bump to the order details of the order.
                        if (account.CustomerTypeID == iConsignmentCustomerTypeID && (productID == 30 || productID == 39))
                        {
                            order.OrderDetails.Add(new OrderDetail()
                            {
                                // Set the SKU price to $0 when not new.
                                // Price = skuPrice,
                                Price = instadoseUnitPrice,
                                ProductID = productID,
                                Quantity = lineQty,
                                OrderDetailDate = DateTime.Now
                            });
                        }
                        else
                        {
                            order.OrderDetails.Add(new OrderDetail()
                            {
                                // Set the SKU price to $0 when not new.
                                // Price = skuPrice,
                                Price = 0,
                                ProductID = productID,
                                Quantity = lineQty,
                                OrderDetailDate = DateTime.Now
                            });
                        }
                    }

                    // For a replacement order, add a lost badge line item.
                    if (orderType == OrderTypes.NoChargeReplacement || orderType == OrderTypes.NoReplacement || orderType == OrderTypes.Replacement)
                    {
                        // Add the lost badge line item
                        order.OrderDetails.Add(new OrderDetail()
                        {
                            Price = lostBadgePrice,
                            ProductID = 11,                 // add /LOST BADGE to orderdetails
                            Quantity = lineQty,
                            OrderDetailDate = DateTime.Now
                        });
                    }

                    if (order.OrderType == "RENEWAL" && account.BrandSourceID == 3)
                    {

                        order.OrderDetails.Add(new OrderDetail()
                        {
                            // Set the SKU price to $0 when not new.
                            // Price = skuPrice,
                            Price = dPrice,
                            ProductID = 12,   // renewal productype id
                            Quantity = lineQty,
                            OrderDetailDate = DateTime.Now
                        });
                    }
                }
            }
        }

        // If the quantity is 0, no badges were orders, throw an error.
        if (totalQuantity == 0) throw new Exception("No products have been selected, please add at least one product to create an order.");

        // If can not find shipping cost for the shipping method, throw an error.
        if (!shippingCost.HasValue)
            throw new Exception("Can not find shipping cost for this shipping method, please select a different shipping method.");
        else
        {
            if (MalvernIntegration && shippingCost.Value == 0)
            {
                if (!ddlShippingMethodMalvern.SelectedItem.Text.ToLower().Contains("free"))
                {
                    if (ddlShippingMethodMalvern.SelectedItem.Text.ToLower().Contains("parcel ground") && totalQuantity > 5)
                        throw new Exception("This shipping method is limitted and only accepts maximum weight of 5 devices, please select a different shipping method.");
                    else if (ddlShippingMethodMalvern.SelectedItem.Text.ToLower().Contains("parcel plug ground") && totalQuantity <= 5)
                        throw new Exception("This shipping method is limitted and only accepts minimum weight of 6 devices, please select a different shipping method.");
                    else if (ddlShippingMethodMalvern.SelectedItem.Text.ToLower().Contains("first class mail") && totalQuantity >= 5)
                        throw new Exception("This shipping method is limitted and only accepts maximum weight of 4 devices, please select a different shipping method.");
                    else
                        throw new Exception("Can not find shipping cost for this shipping method, please select a different shipping method.");
                }
            }
        }


        if (order.OrderType == "RENEWAL" && account.BrandSourceID == 3 && order.OrderDetails.Count > 0)
        {
            string RenewalLogNo = "";
            var renewal = new Renewals();
            renewal.UserName = ActiveDirectoryQueries.GetUserName();

            //Are there account IDs left to process?
            if (account.AccountID > 0)
            {
                //Generate Renewal Number.
                RenewalLogNo = renewal.GenerateRenewalNo();
                //lblRenewalNo.Text = RenewalLogNo;
            }

            // create a log for this renewal
            if (order.RenewalLogs.Count > 0)   // update
            {
                order.RenewalLogs[0].AccountID = account.AccountID;
                order.RenewalLogs[0].BillingTerm = account.BillingTerm;
                order.RenewalLogs[0].BrandSource = account.BrandSource;
                order.RenewalLogs[0].DateProcessed = DateTime.Now;
                order.RenewalLogs[0].ErrorMessage = null;
                order.RenewalLogs[0].Hide = false;
                order.RenewalLogs[0].OrderID = order.OrderID;
                order.RenewalLogs[0].OrderStatus = (order == null) ? null : order.OrderStatus;
                order.RenewalLogs[0].OrgContractStartDate = (DateTime)account.ContractStartDate;
                order.RenewalLogs[0].OrgContractEndDate = (DateTime)account.ContractEndDate;
                order.RenewalLogs[0].PaymentMethodID = payment != null ? payment.PaymentMethodID : 0;
                order.RenewalLogs[0].RenewalAmount = payment != null ? (decimal)payment.Amount : 0;
            }
            else
            {    // create new record
                idc.RenewalLogs.InsertOnSubmit(new RenewalLogs()
                {
                    AccountID = account.AccountID,
                    BillingTerm = account.BillingTerm,
                    BrandSource = account.BrandSource,
                    DateProcessed = DateTime.Now,
                    ErrorMessage = null,
                    Hide = false,
                    //LocationID = order.LocationID,
                    OrderID = order.OrderID,
                    OrderStatus = (order == null) ? null : order.OrderStatus,
                    OrgContractStartDate = (DateTime)account.ContractStartDate,
                    OrgContractEndDate = (DateTime)account.ContractEndDate,
                    PaymentMethodID = payment != null ? payment.PaymentMethodID : 0,
                    RenewalAmount = payment != null ? (decimal)payment.Amount : 0,
                    RenewalNo = RenewalLogNo
                });
            } // end if count
        } // end if ic care

        // Once everything is good. Submit the changes.
        idc.SubmitChanges();


        // Add the Instadose SKU product to orderdetail
        //string[] instadoseImiProductSKUs = new string[5] { "CASE CVR BLK", "CASE CVR BLUE", "CASE CVR GREEN", "CASE CVR PINK", "CASE CVR SLVR" };
        string[] instadoseImiProductSKUs = { "CASE CVR BLK", "CASE CVR BLUE", "CASE CVR GREEN", "CASE CVR PINK", "CASE CVR SLVR" };
        string[] instadosePlusProductSKUs = new string[6] { "COLOR CAP BLUE", "COLOR CAP GREY", "COLOR CAP GRN", "COLOR CAP ORANG", "COLOR CAP PINK", "COLOR CAP RED" };
        //string[] instadose2ProductSKUs = new string[6] { "COLOR CAP BLUE", "COLOR CAP GREY", "COLOR CAP GRN", "COLOR CAP ORANG", "COLOR CAP PINK", "COLOR CAP RED" };
        string[] instadose2ProductSKUs = new string[6] { "ID2BBUMP", "ID2GYBUMP", "ID2GBUMP", "ID2OBUMP", "ID2PBUMP", "ID2RBUMP" };
        string[] instadose3ProductSKUs = new string[6] { Instadose3Constants.CollarRedProductSKU,
                                                         Instadose3Constants.CollarBlueProductSKU,
                                                         Instadose3Constants.CollarGreyProductSKU,
                                                         Instadose3Constants.CollarGreenProductSKU,
                                                         Instadose3Constants.CollarOrangeProductSKU,
                                                         Instadose3Constants.CollarPinkProductSKU
                                                       };

        int countOrderDetailsRecords = 0;
        string populateProductSKU = "";

        if (hfSelectedProductName.Value == "Instadose 1")
        {
            countOrderDetailsRecords = (from od in idc.OrderDetails
                                        join p in idc.Products on od.ProductID equals p.ProductID
                                        where od.OrderID == order.OrderID
                                        && instadoseImiProductSKUs.Contains(p.ProductSKU)
                                        select od).Count();
            populateProductSKU = "INSTA10-B";
        }
        else if (hfSelectedProductName.Value == "Instadose Plus")
        {
            // order qty for ID Plus
            countOrderDetailsRecords = (from od in idc.OrderDetails
                                        join p in idc.Products on od.ProductID equals p.ProductID
                                        where od.OrderID == order.OrderID
                                        && instadosePlusProductSKUs.Contains(p.ProductSKU)
                                        select od).Count();

            var id2Qty = (from od in idc.OrderDetails
                          join p in idc.Products on od.ProductID equals p.ProductID
                          where od.OrderID == order.OrderID
                          && instadose2ProductSKUs.Contains(p.ProductSKU)
                          select od).Count();

            countOrderDetailsRecords += id2Qty;

            populateProductSKU = "INSTA PLUS";

            // OrderUserAssignment for ID Plus
            SaveOrderUserAssignment(order);
            // OrderUserAssignment for ID 2
            SaveOrderUserAssignment(order, false);

        }
        else if ((hfSelectedProductName.Value != "Renewal" || account.BrandSourceID != 3) && hfSelectedProductName.Value != Instadose3Constants.ProductName)  // instadose 2
        {
            countOrderDetailsRecords = (from od in idc.OrderDetails
                                        join p in idc.Products on od.ProductID equals p.ProductID
                                        where od.OrderID == order.OrderID
                                        && instadose2ProductSKUs.Contains(p.ProductSKU)
                                        select od).Count();
            populateProductSKU = "INSTA20";
        }
        else if (hfSelectedProductName.Value == Instadose3Constants.ProductName) 
        {
            countOrderDetailsRecords = (from od in idc.OrderDetails
                                        join p in idc.Products on od.ProductID equals p.ProductID
                                        where od.OrderID == order.OrderID
                                        && instadose3ProductSKUs.Contains(p.ProductSKU)
                                        select od).Count();
             
            populateProductSKU = Instadose3Constants.ProductSKUINSTA30;
        }

        // Per Business Rules, when the Order is a NO REPLACEMENT, exclude the Instadose product Line.
        // also check for consignment account with caps product selected
        if (countOrderDetailsRecords >= 1 && orderType != OrderTypes.NoReplacement && hfSelectedProductName.Value != ""
            && (account.CustomerTypeID != iConsignmentCustomerTypeID || (selectedInstadosePlusCaps == false && selectedInstadose2Covers == false) || (selectedInstadose3 == false && selectedInstadose3Covers == false))
            )
        {
            int myProductID = (from p in idc.Products
                               where p.ProductSKU == populateProductSKU
                               select p.ProductID).FirstOrDefault();

            // CREATE another OrderDetails Record for the Order's Total Quantity and Total Price for Instadose IMI (INSTA10B).
            if (orderType == OrderTypes.NoChargeReplacement || orderType == OrderTypes.Replacement)
            {
                // IF the Order is for /LOST BADGE, then the LineItem for Instadose 1 (IMI) has a $0.00 Price.
                order.OrderDetails.Add(new OrderDetail()
                {
                    Price = 0.00M,
                    Quantity = instadoseTotalQuantity,
                    OrderDetailDate = DateTime.Now,
                    ProductID = myProductID,
                    //ProductID = insta10ProductID,
                    LabelID = null,
                    OrderDevInitialize = false,
                    OrderDevAssign = false
                });
            }
            else
            {
                if (instadoseTotalQuantity > 0)
                {
                    order.OrderDetails.Add(new OrderDetail()
                    {
                        Price = instadoseUnitPrice,
                        Quantity = instadoseTotalQuantity,
                        OrderDetailDate = DateTime.Now,
                        ProductID = myProductID,
                        //ProductID = insta10ProductID,
                        LabelID = null,
                        OrderDevInitialize = false,
                        OrderDevAssign = false
                    });
                };

                if (instadose2TotalQuantity > 0)
                {
                    int id2ProductID = idc.Products.Where(p => p.ProductSKU == "INSTA ID2").Select(p => p.ProductID).FirstOrDefault();
                    order.OrderDetails.Add(new OrderDetail()
                    {
                        Price = instadose2UnitPrice,
                        Quantity = instadose2TotalQuantity,
                        OrderDetailDate = DateTime.Now,
                        ProductID = id2ProductID,
                        //ProductID = insta10ProductID,
                        LabelID = null,
                        OrderDevInitialize = false,
                        OrderDevAssign = false
                    });
                }

                if (instadose3TotalQuantity > 0)
                {
                    int id3ProductID = idc.Products.Where(p => p.ProductSKU == Instadose3Constants.ProductSKUINSTAID3).Select(p => p.ProductID).FirstOrDefault();
                    order.OrderDetails.Add(new OrderDetail()
                    {
                        Price = instadose3UnitPrice,
                        Quantity = instadose3TotalQuantity,
                        OrderDetailDate = DateTime.Now,
                        ProductID = id3ProductID,
                        LabelID = null,
                        OrderDevInitialize = false,
                        OrderDevAssign = false
                    });
                }
            }
        }
        idc.SubmitChanges();

        // Insert Accessory Instalink, InstalinkUSB into Order Detail table
        foreach (GridViewRow row in gvAccessories.Rows)
        {
            int lineQty = 0;
            int productID = 0;

            TextBox txtQty = (TextBox)row.FindControl("txtQty");
            HiddenField hfProductID = (HiddenField)row.FindControl("hfProductID");

            // Added to get Unit Price for /LOST BADGE items.
            // Currencies.RemoveCurrencySymbol(string, string, int) was not working.
            // Coded the functionality the long-way.
            Label lblPrice = (Label)row.FindControl("lblPrice");
            //string strLostBadgePrice = lblPrice.Text;
            string strLostBadgePrice = lblPrice.Text.Split(',')[0];
            strLostBadgePrice = strLostBadgePrice.Replace("$", "");
            strLostBadgePrice = strLostBadgePrice.Replace(ddlCurrency.SelectedValue, "");
            decimal lostBadgePrice = 0;
            decimal.TryParse(strLostBadgePrice, out lostBadgePrice);

            // Skip the row if no items were ordered or the text isn't a number.
            if (!int.TryParse(txtQty.Text, out lineQty) || lineQty == 0) continue;
            if (!int.TryParse(hfProductID.Value, out productID) || productID == 0) continue;

            HiddenField prodGroupID = (HiddenField)row.FindControl("hfProductGroupID");

            tmpInt = 0;

            if (int.TryParse(prodGroupID.Value, out tmpInt))
            {
                // add instakink
                if (tmpInt == 5)
                {
                    instalinkTotalQuantity = lineQty;
                    instalinkUnitPrice = getUnitPrice(tmpInt);

                    // Per Business Rules, when the Order is a NO REPLACEMENT, exclude the Instadose product Line.
                    if (orderType != OrderTypes.NoReplacement)
                    {
                        int myProductID = (from p in idc.Products
                                           where p.ProductSKU == "INSTALINK"
                                           select p.ProductID).FirstOrDefault();

                        // CREATE another OrderDetails Record for the Order's Total Quantity and Total Price for Instadose IMI (INSTA10B).
                        if (orderType == OrderTypes.NoChargeReplacement || orderType == OrderTypes.Replacement)
                        {
                            // IF the Order is for /LOST BADGE, then the LineItem for Instalink has a $0.00 Price.
                            order.OrderDetails.Add(new OrderDetail()
                            {
                                Price = 0.00M,
                                Quantity = instalinkTotalQuantity,
                                OrderDetailDate = DateTime.Now,
                                ProductID = myProductID,
                                //ProductID = insta10ProductID,
                                LabelID = null,
                                OrderDevInitialize = false,
                                OrderDevAssign = false
                            });
                        }
                        else
                        {
                            order.OrderDetails.Add(new OrderDetail()
                            {
                                Price = instalinkUnitPrice,
                                Quantity = instalinkTotalQuantity,
                                OrderDetailDate = DateTime.Now,
                                ProductID = myProductID,
                                //ProductID = insta10ProductID,
                                LabelID = null,
                                OrderDevInitialize = false,
                                OrderDevAssign = false
                            });
                        }
                        idc.SubmitChanges();
                    }
                }

                // add instakinkUSB
                if (tmpInt == 6)
                {
                    instalinkUSBTotalQuantity = lineQty;
                    instalinkUSBUnitPrice = getUnitPrice(tmpInt);

                    // Per Business Rules, when the Order is a NO REPLACEMENT, exclude the Instadose product Line.
                    if (orderType != OrderTypes.NoReplacement)
                    {
                        int myProductID = (from p in idc.Products
                                           where p.ProductSKU == "INSTALINKUSB"
                                           select p.ProductID).FirstOrDefault();

                        // CREATE another OrderDetails Record for the Order's Total Quantity and Total Price for Instadose IMI (INSTA10B).
                        if (orderType == OrderTypes.NoChargeReplacement || orderType == OrderTypes.Replacement)
                        {
                            // IF the Order is for /LOST BADGE, then the LineItem for InstalinkUSB has a $0.00 Price.
                            order.OrderDetails.Add(new OrderDetail()
                            {
                                Price = 0.00M,
                                Quantity = instalinkUSBTotalQuantity,
                                OrderDetailDate = DateTime.Now,
                                ProductID = myProductID,
                                //ProductID = insta10ProductID,
                                LabelID = null,
                                OrderDevInitialize = false,
                                OrderDevAssign = false
                            });
                        }
                        else
                        {
                            order.OrderDetails.Add(new OrderDetail()
                            {
                                Price = instalinkUSBUnitPrice,
                                Quantity = instalinkUSBTotalQuantity,
                                OrderDetailDate = DateTime.Now,
                                ProductID = myProductID,
                                //ProductID = insta10ProductID,
                                LabelID = null,
                                OrderDevInitialize = false,
                                OrderDevAssign = false
                            });
                        }
                        idc.SubmitChanges();
                    }
                }
            }
            else
            {
                throw new Exception("An error occured, Order information, could not be saved to the Database, please contact I.T.");  // never go to this block. The possible error throw is in previous for loop
            }

        }

        //----------If broken order, lost replacement order or consignment account then set OrderStatusID = 3 --------------- //
        if (orderType == OrderTypes.NoChargeReplacement || orderType == OrderTypes.Replacement
            || account.CustomerTypeID == iConsignmentCustomerTypeID)
        {
            order.OrderStatusID = 3; // Order is created, released to MAS for fullfillment.
            idc.SubmitChanges();

        }
        else if (orderType == OrderTypes.NoReplacement) //----------lost no-replacement order then set OrderStatusID = 4 ------ //
        {
            order.OrderStatusID = 4; // Order is created and set shipped
            idc.SubmitChanges();
        }

        // If broken order, lost replacement order, lost no-replacement order or consignment account, automatically send them to MAS.
        if (orderType == OrderTypes.NoChargeReplacement ||
            orderType == OrderTypes.NoReplacement ||
            orderType == OrderTypes.Replacement ||
            account.CustomerTypeID == iConsignmentCustomerTypeID)
        {
            // Send an order to MAS.
            //if (!Instadose.Integration.MAS.SendOrderToMAS(order.OrderID, ActiveDirectoryQueries.GetUserName()))
            if (!Instadose.Integration.MAS.SendOrderToMAS(order.OrderID, userName))
                throw new Exception("The order has been created, but the records could not be sent to MAS. Please contact IT for assistance.");
        }

        // Return the order ID
        return order.OrderID;
    }

    private void InvisibleErrorMsg()
    {
        // Reset submission form error message      
        this.lblErrorMessage.Text = "";
        this.errorForm.Visible = false;
    }

    private void VisibleErrorMsg(string error)
    {
        this.lblErrorMessage.Text = error;
        this.errorForm.Visible = true;

    }

    private void InvisibleMaxErrorMsg()
    {
        // Reset submission form error message      
        this.lblMaxErrorMessage.Text = "";
        this.MaxErrorForm.Visible = false;
    }

    private void VisibleMaxErrorMsg(string error)
    {
        this.lblMaxErrorMessage.Text = error;
        this.MaxErrorForm.Visible = true;

    }




    //private int getShippingOffsetDays(int shippingMethodID)
    //{
    //    switch (shippingMethodID)
    //    {
    //        case 1: // Ground +1????
    //        case 8:
    //        case 9:
    //            return 1;
    //        case 2: // 2 Day +2
    //        case 7:
    //        case 10:
    //        case 11:
    //            return 2;
    //        case 3: // 3 Day +3
    //        case 6:
    //            return 3;
    //        case 4: // Ground +7
    //        case 5:
    //        case 12:
    //            return 7;
    //        default: // Unknown +3
    //            return 3;
    //    }
    //}

    //private DateTime calculateQuarterlyServiceEnd(DateTime contractStartDate, DateTime contractEndDate, DateTime serviceStartDate)
    //{
    //    DateTime quarter1 = contractStartDate.AddMonths(3);
    //    DateTime quarter2 = contractStartDate.AddMonths(6);
    //    DateTime quarter3 = contractStartDate.AddMonths(9);

    //    if (serviceStartDate < contractStartDate ||
    //       (serviceStartDate <= quarter1 && serviceStartDate >= contractStartDate))
    //        return quarter1;

    //    else if (serviceStartDate <= quarter2 && serviceStartDate >= quarter1)
    //        return quarter2;

    //    else if (serviceStartDate <= quarter3 && serviceStartDate >= quarter2)
    //        return quarter3;

    //    else if (serviceStartDate <= contractEndDate && serviceStartDate >= quarter3)
    //        return contractEndDate;

    //    return serviceStartDate.AddMonths(3);
    //}


    // -----------------------------ID+ Order User Assignment------------------------------------//

    private void SaveOrderUserAssignment(Order pOrder, bool isIDPlus = true)
    {
        // If going to update an existing order then purging all existing OrderUserAssign by order
        if (btnCreate.Text == "Update Order")
        {
            // Purge existing order details
            idc.OrderUserAssigns.DeleteAllOnSubmit(order.OrderUserAssigns);
            idc.SubmitChanges();
        }

        List<Product> prods = new List<Product>();
        if (isIDPlus)
            prods = idc.Products.Where(p => p.ProductSKU.Contains("COLOR CAP")).ToList();   // for ID Plus
        else
            prods = idc.Products.Where(p => p.ProductGroupID == 11 && p.Color != "No Color" && p.Color != "N/A" && p.Color != null).ToList();   // for ID 2

        foreach (Product prod in prods)
        {
            List<OrderAssignUserInfo> orderAssignUserByColorList = new List<OrderAssignUserInfo>();

            if (!isIDPlus)
                orderAssignUserByColorList = (List<OrderAssignUserInfo>)Session["ID2_" + prod.Color];
            else
                orderAssignUserByColorList = (List<OrderAssignUserInfo>)Session["IDPlus_" + prod.Color];

            if (orderAssignUserByColorList != null && orderAssignUserByColorList.Count > 0)
            {
                foreach (OrderAssignUserInfo iter in orderAssignUserByColorList)
                {
                    OrderUserAssign rec = new OrderUserAssign
                    {
                        OrderID = pOrder.OrderID,
                        UserID = int.Parse(iter.UserID),
                        ProductID = prod.ProductID,
                        BodyRegionID = int.Parse(iter.BodyRegionID)
                    };

                    idc.OrderUserAssigns.InsertOnSubmit(rec);
                    idc.SubmitChanges();
                }
            }
        }
    }

    protected void gvInstadosePlus_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            System.Web.UI.WebControls.Label lblColor = (System.Web.UI.WebControls.Label)e.Row.FindControl("lblColor");
            LinkButton lButton = (LinkButton)e.Row.FindControl("btnAssignedWearerByProduct");
            ImageButton iButton = (ImageButton)e.Row.FindControl("imgAssignedWearerByProduct");
            HiddenField hidProductID = (HiddenField)e.Row.FindControl("hfProductID");

            // Loading exisitng OrderUserAssign by color into session variable
            if (order != null)
            {
                var OrderUserAssignsList = (from a in idc.OrderUserAssigns where a.OrderID == order.OrderID && a.ProductID == int.Parse(hidProductID.Value) select a).ToList();
                if (OrderUserAssignsList.Count > 0)
                {
                    lButton.Text = OrderUserAssignsList.Count.ToString();

                    // Load session variable's value for a list of assigned users by color
                    List<OrderAssignUserInfo> orderAssignUserList = new List<OrderAssignUserInfo>();

                    foreach (OrderUserAssign row in OrderUserAssignsList)
                    {
                        OrderAssignUserInfo orderAssignUser = new OrderAssignUserInfo();
                        orderAssignUser.Color = row.Product.Color;
                        orderAssignUser.UserID = row.UserID.ToString();
                        orderAssignUser.BodyRegionID = row.BodyRegionID.ToString();
                        orderAssignUserList.Add(orderAssignUser);
                    }

                    Session["IDPlus_" + lblColor.Text] = orderAssignUserList;
                }
            }

            if (lblColor != null && lblColor.Text == "Grey")
            {
                lButton.Visible = false;
                iButton.Visible = false;
                return;
            }
        }
    }

    protected void gvInstadose2_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            System.Web.UI.WebControls.Label lblColor = (System.Web.UI.WebControls.Label)e.Row.FindControl("lblColor");
            LinkButton lButton = (LinkButton)e.Row.FindControl("btnAssignedWearerByProduct");
            ImageButton iButton = (ImageButton)e.Row.FindControl("imgAssignedWearerByProduct");
            HiddenField hidProductID = (HiddenField)e.Row.FindControl("hfProductID");

            // Loading exisitng OrderUserAssign by color into session variable
            if (order != null)
            {
                var OrderUserAssignsList = (from a in idc.OrderUserAssigns where a.OrderID == order.OrderID && a.ProductID == int.Parse(hidProductID.Value) select a).ToList();
                if (OrderUserAssignsList.Count > 0)
                {
                    lButton.Text = OrderUserAssignsList.Count.ToString();

                    // Load session variable's value for a list of assigned users by color
                    List<OrderAssignUserInfo> orderAssignUserList = new List<OrderAssignUserInfo>();

                    foreach (OrderUserAssign row in OrderUserAssignsList)
                    {
                        OrderAssignUserInfo orderAssignUser = new OrderAssignUserInfo();
                        orderAssignUser.Color = row.Product.Color;
                        orderAssignUser.UserID = row.UserID.ToString();
                        orderAssignUser.BodyRegionID = row.BodyRegionID.ToString();
                        orderAssignUserList.Add(orderAssignUser);
                    }

                    Session["ID2_" + lblColor.Text] = orderAssignUserList;
                }
            }

            if (lblColor != null && lblColor.Text == "Grey")
            {
                lButton.Visible = false;
                iButton.Visible = false;
                return;
            }
        }
    }

    protected void gvInstadoseEliteDemo_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        //if (e.Row.RowType == DataControlRowType.DataRow)
        //{
        //    System.Web.UI.WebControls.Label lblColor = (System.Web.UI.WebControls.Label)e.Row.FindControl("lblColor");
        //    LinkButton lButton = (LinkButton)e.Row.FindControl("btnAssignedWearerByProduct");
        //    ImageButton iButton = (ImageButton)e.Row.FindControl("imgAssignedWearerByProduct");
        //    HiddenField hidProductID = (HiddenField)e.Row.FindControl("hfProductID");

        //    // Loading exisitng OrderUserAssign by color into session variable
        //    if (order != null)
        //    {
        //        var OrderUserAssignsList = (from a in idc.OrderUserAssigns where a.OrderID == order.OrderID && a.ProductID == int.Parse(hidProductID.Value) select a).ToList();
        //        if (OrderUserAssignsList.Count > 0)
        //        {
        //            lButton.Text = OrderUserAssignsList.Count.ToString();

        //            // Load session variable's value for a list of assigned users by color
        //            List<OrderAssignUserInfo> orderAssignUserList = new List<OrderAssignUserInfo>();

        //            foreach (OrderUserAssign row in OrderUserAssignsList)
        //            {
        //                OrderAssignUserInfo orderAssignUser = new OrderAssignUserInfo();
        //                orderAssignUser.Color = row.Product.Color;
        //                orderAssignUser.UserID = row.UserID.ToString();
        //                orderAssignUser.BodyRegionID = row.BodyRegionID.ToString();
        //                orderAssignUserList.Add(orderAssignUser);
        //            }

        //            Session[lblColor.Text] = orderAssignUserList;
        //        }
        //    }

        //    if (lblColor.Text == "Grey")
        //    {
        //        lButton.Visible = false;
        //        iButton.Visible = false;
        //        return;
        //    }
        //}
    }


    protected void gvInstadose2Con_RowDataBound(object sender, GridViewRowEventArgs e)
    {

    }

    protected void gv_WearerList_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        btnWearerSearch_Click(sender, null);
        this.gv_WearerList.PageIndex = e.NewPageIndex;
        this.gv_WearerList.DataBind();
    }

    protected void gv_WearerList_Sorting(object sender, GridViewSortEventArgs e)
    {
        this.gv_WearerList.PageIndex = 0;
    }

    protected void gv_WearerList_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            System.Web.UI.WebControls.Label l = (System.Web.UI.WebControls.Label)e.Row.FindControl("lblBodyRegionID");
            DropDownList ddl = (DropDownList)e.Row.FindControl("ddlBodyRegion");

            ddl.DataSource = (from a in idc.BodyRegions select a);
            ddl.DataBind();
            ddl.SelectedValue = l.Text;
        }
    }

    protected void gv_WearerList_Edit_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            System.Web.UI.WebControls.Label l_Edit = (System.Web.UI.WebControls.Label)e.Row.FindControl("lblBodyRegionID_Edit");
            DropDownList ddl_Edit = (DropDownList)e.Row.FindControl("ddlBodyRegion_Edit");
            CheckBox chk_Edit = (CheckBox)e.Row.FindControl("chkbxSelectWearerList_Edit");

            ddl_Edit.DataSource = (from a in idc.BodyRegions select a);
            ddl_Edit.DataBind();
            ddl_Edit.SelectedValue = l_Edit.Text;

            chk_Edit.Checked = true;
        }
    }

    protected void gvInstadose3_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            System.Web.UI.WebControls.Label lblColor = (System.Web.UI.WebControls.Label)e.Row.FindControl("lblColor");
            LinkButton lButton = (LinkButton)e.Row.FindControl("btnAssignedWearerByProduct");
            ImageButton iButton = (ImageButton)e.Row.FindControl("imgAssignedWearerByProduct");
            HiddenField hidProductID = (HiddenField)e.Row.FindControl("hfProductID");

            // Loading exisitng OrderUserAssign by color into session variable
            if (order != null)
            {
                var OrderUserAssignsList = (from a in idc.OrderUserAssigns where a.OrderID == order.OrderID && a.ProductID == int.Parse(hidProductID.Value) select a).ToList();
                if (OrderUserAssignsList.Count > 0)
                {
                    lButton.Text = OrderUserAssignsList.Count.ToString();

                    // Load session variable's value for a list of assigned users by color
                    List<OrderAssignUserInfo> orderAssignUserList = new List<OrderAssignUserInfo>();

                    foreach (OrderUserAssign row in OrderUserAssignsList)
                    {
                        OrderAssignUserInfo orderAssignUser = new OrderAssignUserInfo();
                        orderAssignUser.Color = row.Product.Color;
                        orderAssignUser.UserID = row.UserID.ToString();
                        orderAssignUser.BodyRegionID = row.BodyRegionID.ToString();
                        orderAssignUserList.Add(orderAssignUser);
                    }

                    Session["ID3_" + lblColor.Text] = orderAssignUserList;
                }
            }

            if (lblColor != null && lblColor.Text == "Grey")
            {
                lButton.Visible = false;
                iButton.Visible = false;
                return;
            }
        }
    }

    private void ReloadInstadosePlusGrid()
    {
        foreach (GridViewRow row in this.gvInstadosePlus.Rows)
        {
            System.Web.UI.WebControls.Label l = (System.Web.UI.WebControls.Label)row.FindControl("lblColor");
            LinkButton lButton = (LinkButton)row.FindControl("btnAssignedWearerByProduct");

            List<OrderAssignUserInfo> orderAssignUserByColorList = new List<OrderAssignUserInfo>();
            orderAssignUserByColorList = (List<OrderAssignUserInfo>)Session["IDPlus_" + l.Text];

            if (orderAssignUserByColorList == null || orderAssignUserByColorList.Count == 0)
            {
                lButton.Text = "";
            }
            else
            {
                lButton.Text = orderAssignUserByColorList.Count.ToString();
            }
        }
    }

    private void ReloadInstadose2Grid()
    {
        foreach (GridViewRow row in this.gvInstadose2.Rows)
        {
            System.Web.UI.WebControls.Label l = (System.Web.UI.WebControls.Label)row.FindControl("lblColor");
            LinkButton lButton = (LinkButton)row.FindControl("btnAssignedWearerByProduct");

            List<OrderAssignUserInfo> orderAssignUserByColorList = new List<OrderAssignUserInfo>();
            orderAssignUserByColorList = (List<OrderAssignUserInfo>)Session["ID2_" + l.Text];

            if (orderAssignUserByColorList == null || orderAssignUserByColorList.Count == 0)
            {
                lButton.Text = "";
            }
            else
            {
                lButton.Text = orderAssignUserByColorList.Count.ToString();
            }
        }
    }

    private string GenerateMessage(List<int> pOverAssignUserList, string pHeaderErrorMsg)
    {
        string errorText = "<ul type='circle'>";
        foreach (int iterUserID in pOverAssignUserList)
        {
            errorText += "<li>" + iterUserID.ToString() + "</li>";
        }
        errorText += "</ul>";

        return pHeaderErrorMsg + errorText;
    }

    protected void btnAssignedWearer_Click(object sender, EventArgs e)
    {
        InvisibleErrors_assignedWearerDialog();

        if (passInputsValidation_assignedWearerDialog())
        {
            List<OrderAssignUserInfo> orderAssignUserList = new List<OrderAssignUserInfo>();

            if (lblProduct.Text == "Instadose 2")
            {
                orderAssignUserList = (List<OrderAssignUserInfo>)Session["ID2_" + lblColor.Text];
            }
            else
            {
                orderAssignUserList = (List<OrderAssignUserInfo>)Session["IDPlus_" + lblColor.Text];
            }

            if (orderAssignUserList == null || orderAssignUserList.Count == 0)
            {
                orderAssignUserList = new List<OrderAssignUserInfo>();
            }

            foreach (GridViewRow row in this.gv_WearerList.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkbxSelectWearerList");

                if (chk.Checked)
                {
                    DropDownList ddl = (DropDownList)row.FindControl("ddlBodyRegion");
                    HiddenField hidUserID = (HiddenField)row.FindControl("HidUserID");
                    OrderAssignUserInfo orderAssignUser = new OrderAssignUserInfo();
                    orderAssignUser.Color = lblColor.Text;
                    orderAssignUser.UserID = hidUserID.Value;
                    orderAssignUser.BodyRegionID = ddl.SelectedValue;
                    orderAssignUserList.Add(orderAssignUser);
                }
            }

            if (lblProduct.Text == "Instadose 2")
                Session["ID2_" + lblColor.Text] = orderAssignUserList;
            else
                Session["IDPlus_" + lblColor.Text] = orderAssignUserList;

            ReloadInstadosePlusGrid();
            ReloadInstadose2Grid();

            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID,
                    "$('#assignedWearerDialog').dialog('close');", true);
        }
    }

    protected void btnAssignedWearer_Edit_Click(object sender, EventArgs e)
    {
        InvisibleErrors_assignedWearerDialog_Edit();

        if (passInputsValidation_assignedWearerDialog_Edit())
        {
            List<OrderAssignUserInfo> orderAssignUserList = new List<OrderAssignUserInfo>();

            foreach (GridViewRow row in this.gv_WearerList_Edit.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkbxSelectWearerList_Edit");

                if (chk.Checked)
                {
                    DropDownList ddl_Edit = (DropDownList)row.FindControl("ddlBodyRegion_Edit");
                    HiddenField hidUserID_Edit = (HiddenField)row.FindControl("HidUserID_Edit");
                    OrderAssignUserInfo orderAssignUser = new OrderAssignUserInfo();
                    orderAssignUser.Color = lblColor_Edit.Text;
                    orderAssignUser.UserID = hidUserID_Edit.Value;
                    orderAssignUser.BodyRegionID = ddl_Edit.SelectedValue;
                    orderAssignUserList.Add(orderAssignUser);
                }
            }

            if (lblProduct_Edit.Text == "Instadose 2")
                Session["ID2_" + lblColor_Edit.Text] = orderAssignUserList;
            else
                Session["IDPlus_" + lblColor_Edit.Text] = orderAssignUserList;

            ReloadInstadosePlusGrid();
            ReloadInstadose2Grid();

            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID,
                    "$('#assignedWearerDialog_Edit').dialog('close');", true);
        }
    }

    protected void btnWearerSearch_Click(object sender, EventArgs e)
    {
        var searchResult = idc.sp_GetUsersBySearch(account.AccountID, location.LocationID, this.rbtnSearchBy.SelectedValue, this.txtOrderAssignSearch.Text).ToList();

        gv_WearerList.DataSource = searchResult;
        gv_WearerList.DataBind();
    }

    private void DefineOrderUserAssignDataTable()
    {
        dtOrderUserAssign = new DataTable();
        dtOrderUserAssign.Columns.Add("UserID", typeof(string));
        dtOrderUserAssign.Columns.Add("Name", typeof(string));
        dtOrderUserAssign.Columns.Add("BodyRegionName", typeof(string));
        dtOrderUserAssign.Columns.Add("BodyRegionID", typeof(string));
    }

    private void loadAssignWearerWindow(string productID)
    {
        var product = (from p in idc.Products where p.ProductID == int.Parse(productID) select p).FirstOrDefault();
        lblColor.Text = product.Color;
        lblProduct.Text = product.ProductName;
        lblAssignLocation.Text = location.LocationName;

        foreach (GridViewRow row in this.gvInstadosePlus.Rows)
        {
            if (((HiddenField)row.FindControl("hfProductID")).Value == productID)
            {
                lblTotalQty.Text = ((TextBox)row.FindControl("txtQty")).Text;
                break;
            }
        }

        foreach (GridViewRow row in gvInstadose2.Rows)
        {
            if (((HiddenField)row.FindControl("hfProductID")).Value == productID)
            {
                lblTotalQty.Text = ((TextBox)row.FindControl("txtQty")).Text;
                break;
            }
        }

        int totalQty = 0;
        if (!int.TryParse(lblTotalQty.Text, out totalQty)) totalQty = 0;

        int totalAssignedWearer = 0;
        List<OrderAssignUserInfo> orderAssignUserByColorList = new List<OrderAssignUserInfo>();

        if (product.ProductGroupID == 11)
            orderAssignUserByColorList = (List<OrderAssignUserInfo>)Session["ID2_" + lblColor.Text];
        else
            orderAssignUserByColorList = (List<OrderAssignUserInfo>)Session["IDPlus_" + lblColor.Text];

        if (orderAssignUserByColorList != null && orderAssignUserByColorList.Count > 0)
            totalAssignedWearer = orderAssignUserByColorList.Count;

        if (totalQty == 0 || totalQty == totalAssignedWearer) return;   // do not open the Assign Wearer window if no qty enter and number of assigned user = qty enter              

        lblTotalAssignedWearer.Text = totalAssignedWearer.ToString();

        gv_WearerList.DataSource = null;
        gv_WearerList.DataBind();

        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "$('#assignedWearerDialog').dialog('open');", true);
    }

    private void loadAssignWearer_EditWindow(string productID)
    {
        var product = (from p in idc.Products where p.ProductID == int.Parse(productID) select p).FirstOrDefault();
        lblColor_Edit.Text = product.Color;
        lblProduct_Edit.Text = product.ProductName;

        foreach (GridViewRow row in this.gvInstadosePlus.Rows)
        {
            if (((HiddenField)row.FindControl("hfProductID")).Value == productID)
            {
                lblTotalQty_Edit.Text = ((TextBox)row.FindControl("txtQty")).Text;
                break;
            }
        }

        List<OrderAssignUserInfo> orderAssignUserByColorList = new List<OrderAssignUserInfo>();

        if (product.ProductGroupID == 11)
            orderAssignUserByColorList = (List<OrderAssignUserInfo>)Session["ID2_" + lblColor_Edit.Text];
        else
            orderAssignUserByColorList = (List<OrderAssignUserInfo>)Session["IDPlus_" + lblColor_Edit.Text];

        if (orderAssignUserByColorList == null || orderAssignUserByColorList.Count == 0)
        {
            gv_WearerList_Edit.DataSource = null;
            gv_WearerList_Edit.DataBind();

            lblTotalAssignedWearer_Edit.Text = "0";
        }
        else
        {
            DataRow dtr;

            DefineOrderUserAssignDataTable();

            foreach (OrderAssignUserInfo iter in orderAssignUserByColorList)
            {
                dtr = dtOrderUserAssign.NewRow();

                dtr["UserID"] = iter.UserID;
                dtr["Name"] = (from u in idc.Users where u.UserID == int.Parse(iter.UserID) select u.FirstName + " " + u.LastName).FirstOrDefault();
                dtr["BodyRegionName"] = (from b in idc.BodyRegions where b.BodyRegionID == int.Parse(iter.BodyRegionID) select b.BodyRegionName).FirstOrDefault();
                dtr["BodyRegionID"] = iter.BodyRegionID;

                dtOrderUserAssign.Rows.Add(dtr);
            }

            this.gv_WearerList_Edit.DataSource = dtOrderUserAssign;
            this.gv_WearerList_Edit.DataBind();

            lblTotalAssignedWearer_Edit.Text = orderAssignUserByColorList.Count.ToString();
        }

        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "$('#assignedWearerDialog_Edit').dialog('open');", true);
    }

    protected void btnAssignedWearerByProduct_Click(object sender, EventArgs e)
    {
        InvisibleErrors_assignedWearerDialog_Edit();
        LinkButton btn = (LinkButton)sender;
        string myCmdname = btn.CommandName.ToString();
        string productID = btn.CommandArgument.ToString();

        loadAssignWearer_EditWindow(productID);
    }

    protected void imgAssignedWearerByProduct_Click(object sender, ImageClickEventArgs e)
    {
        InvisibleErrors_assignedWearerDialog();

        ImageButton imgbtn = (ImageButton)sender;
        string imgbtnCommandName = imgbtn.CommandName.ToString();
        string productID = imgbtn.CommandArgument.ToString();

        loadAssignWearerWindow(productID);
    }

    protected void ddlSetBodyRegion_OnSelectedIndexChange(object sender, EventArgs e)
    {
        foreach (GridViewRow row in this.gv_WearerList.Rows)
        {
            DropDownList ddl = (DropDownList)row.FindControl("ddlBodyRegion");
            ddl.SelectedValue = ddlSetBodyRegion.SelectedValue;
        }
    }

    protected void rbtnSearchBy_SelectedIndexChanged(object sender, EventArgs e)
    {
        switch (this.rbtnSearchBy.SelectedValue)
        {
            case "all":
            case "assigned":
            case "un-assigned":
                searchFilter.Visible = false;
                //lblDash.Visible = false;
                //txtToWearerID.Visible = false;
                break;
            //case "rangeWearerID":
            //    searchFilter.Visible = true;
            //    //lblDash.Visible = true;
            //    //txtToWearerID.Visible = true;
            //    break;
            case "lastName":
                searchFilter.Visible = true;
                break;
        }
    }

    private bool passInputsValidation_assignedWearerDialog()
    {
        string errorString = "";

        if (ValidateAssignedTotal(ref errorString) == false)
        {
            this.VisibleErrors_assignedWearerDialog(errorString);
            return false;
        }

        if (ValidateAssignedUsers(ref errorString) == false)
        {
            this.VisibleErrors_assignedWearerDialog(errorString);
            return false;
        }

        return true;
    }

    private bool passInputsValidation_assignedWearerDialog_Edit()
    {
        string errorString = "";

        if (ValidateAssignedUsers_Edit(ref errorString) == false)
        {
            this.VisibleErrors_assignedWearerDialog_Edit(errorString);
            return false;
        }

        return true;
    }

    private void InvisibleErrors_assignedWearerDialog()
    {
        // Reset submission form error message      
        this.assignedWearerDialogErrorMsg.InnerHtml = "";
        this.assignedWearerDialogError.Visible = false;
    }

    private void VisibleErrors_assignedWearerDialog(string error)
    {
        this.assignedWearerDialogErrorMsg.InnerHtml = error;
        this.assignedWearerDialogError.Visible = true;
    }

    private void InvisibleErrors_assignedWearerDialog_Edit()
    {
        // Reset submission form error message      
        this.assignedWearerDialogErrorMsg_Edit.InnerHtml = "";
        this.assignedWearerDialogError_Edit.Visible = false;
    }

    private void VisibleErrors_assignedWearerDialog_Edit(string error)
    {
        this.assignedWearerDialogErrorMsg_Edit.InnerHtml = error;
        this.assignedWearerDialogError_Edit.Visible = true;
    }

    private void ClearOutAllSessionInfo_IDPlus()
    {
        var prods = (from p in idc.Products where p.ProductSKU.Contains("COLOR CAP") select p).ToList();

        foreach (Product prod in prods)
        {
            Session["IDPlus_" + prod.Color] = null;
        }
    }

    private void ClearOutAllSessionInfo_ID2()
    {
        var prods = (from p in idc.Products where p.ProductGroupID == 11 && p.Color != "No Color" && p.Color != "N/A" && p.Color != null select p).ToList();

        foreach (Product prod in prods)
        {
            Session["ID2_" + prod.Color] = null;
        }
    }

    private bool validateCalendar(int pAccountID, int pLocationID, string pProductName, ref string pErrorMsg)
    {
        if (pProductName == "Instadose Plus")
        {
            int act_ScheduleID = (from a in idc.Accounts where a.AccountID == pAccountID && a.ScheduleID != null select a).Count();
            int loc_ScheduleID = (from l in idc.Locations where l.LocationID == pLocationID && l.ScheduleID != null select l).Count();

            if (act_ScheduleID == 0 && loc_ScheduleID == 0)
            {
                Location orderLocation = (from l in idc.Locations where l.LocationID == pLocationID select l).FirstOrDefault();
                pErrorMsg = "Require calendar for location: " + orderLocation.LocationName;
                return false;
            }
        }

        return true;
    }

    private bool validateRateByAccount(Account pAccount, int pProductGroupID, ref string pErrorMsg)
    {
        var detail = (from r in pAccount.Rate.RateDetails
                      join rd in idc.Rates on r.RateID equals rd.RateID
                      where r.Active && r.ProductGroupID == pProductGroupID
                      select r).ToList();

        if (detail.Count == 0)
        {
            var pg = (from g in idc.ProductGroups where g.ProductGroupID == pProductGroupID select g).FirstOrDefault();
            string productName = pg.ProductGroupName == "Instadose 2 New" ? "Instadose 2" : pg.ProductGroupName;
            pErrorMsg = "Require a rate code setup for a product: " + productName;
            return false;
        }

        return true;
    }

    private bool validateAccessoriesRateCodeByAccount(string pProductName, ref string pErrorMsg)
    {
        foreach (GridViewRow rowitem in gvAccessories.Rows)
        {
            HiddenField productGroupID = (HiddenField)rowitem.FindControl("hfProductGroupID");
            int productGroupIDValue = 0;
            int.TryParse(productGroupID.Value, out productGroupIDValue);

            TextBox txtItemQty = (TextBox)rowitem.FindControl("txtQty");
            int itemQty = 0;
            int.TryParse(txtItemQty.Text, out itemQty);

            if (itemQty > 0)
            {
                if (validateRateByAccount(this.account, productGroupIDValue, ref pErrorMsg) == false)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private bool isAlreadyAssigned(int pUserID, int pBodyRegionID)
    {
        int cnt = (from ud in idc.UserDevices where ud.UserID == pUserID && ud.BodyRegionID == pBodyRegionID && ud.Active && ud.DeactivateDate == null select ud).Count();
        if (cnt > 0) return true;

        return false;
    }

    private bool isAssignedInPendingOrder(int pUserID, int pBodyRegionID)
    {
        var myOrderStatus = new int[] { 1, 2, 3, 6, 7 };

        var orderUserAssigns = (from o in idc.Orders
                                join oua in idc.OrderUserAssigns on o.OrderID equals oua.OrderID
                                where o.AccountID == account.AccountID && myOrderStatus.Contains(o.OrderStatusID) && oua.OrderID != (this.order != null ? this.order.OrderID : 0)
                                select oua).ToList();

        foreach (OrderUserAssign row in orderUserAssigns)
        {
            if (pUserID == row.UserID && pBodyRegionID == row.BodyRegionID)
                return true;
        }

        return false;
    }

    private bool isAlreadyAssignedInSessionVariables(int pUserID, int pBodyRegionID, string pExcluseCheckByColor = "")
    {
        var prods = (from p in idc.Products where p.ProductSKU.Contains("COLOR CAP") select p).ToList();

        foreach (Product prod in prods)
        {
            if (pExcluseCheckByColor != "" && pExcluseCheckByColor == prod.Color)
            {
                continue;
            }

            List<OrderAssignUserInfo> orderAssignUserByColorList = new List<OrderAssignUserInfo>();

            orderAssignUserByColorList = (List<OrderAssignUserInfo>)Session["IDPlus_" + prod.Color];

            if (orderAssignUserByColorList != null && orderAssignUserByColorList.Count > 0)
            {
                foreach (OrderAssignUserInfo iter in orderAssignUserByColorList)
                {
                    if (pUserID == int.Parse(iter.UserID) && pBodyRegionID == int.Parse(iter.BodyRegionID))
                        return true;
                }
            }
        }

        var id2Prods = (from p in idc.Products where p.ProductGroupID == 11 && p.Color != null && p.Color != "No Color" && p.Color != "N/A" select p).ToList();

        foreach (Product prod in id2Prods)
        {
            if (pExcluseCheckByColor != "" && pExcluseCheckByColor == prod.Color)
            {
                continue;
            }

            List<OrderAssignUserInfo> orderAssignUserByColorList = new List<OrderAssignUserInfo>();

            orderAssignUserByColorList = (List<OrderAssignUserInfo>)Session["ID2_" + prod.Color];

            if (orderAssignUserByColorList != null && orderAssignUserByColorList.Count > 0)
            {
                foreach (OrderAssignUserInfo iter in orderAssignUserByColorList)
                {
                    if (pUserID == int.Parse(iter.UserID) && pBodyRegionID == int.Parse(iter.BodyRegionID))
                        return true;
                }
            }
        }

        return false;
    }

    private bool isMultipleAssigning(int pUserID, int pBodyRegionID)
    {
        // check if already assignned in the current grid
        int cnt = 0;
        int curUserID = 0, curBodyRegionID = 0;
        foreach (GridViewRow row in this.gv_WearerList.Rows)
        {
            CheckBox chk = (CheckBox)row.FindControl("chkbxSelectWearerList");
            if (chk.Checked)
            {
                DropDownList ddl = (DropDownList)row.FindControl("ddlBodyRegion");
                HiddenField hidUserID = (HiddenField)row.FindControl("HidUserID");
                curUserID = int.Parse(hidUserID.Value);
                curBodyRegionID = int.Parse(ddl.SelectedValue);

                if (pUserID == curUserID && pBodyRegionID == curBodyRegionID) cnt++;
            }
        }
        if (cnt > 1)
            return true;

        // check if already assignned on all session variables by colors  
        if (isAlreadyAssignedInSessionVariables(pUserID, pBodyRegionID))
            return true;

        // check if already assignned to UserDevices 
        if (isAlreadyAssigned(pUserID, pBodyRegionID))
            return true;

        // check if already assignned in the order which has not shipped yet
        if (isAssignedInPendingOrder(pUserID, pBodyRegionID))
            return true;

        return false;
    }

    private bool isMultipleAssigning_Edit(int pUserID, int pBodyRegionID)
    {
        // check if already assignned in the current grid
        int cnt = 0;
        int curUserID = 0, curBodyRegionID = 0;
        foreach (GridViewRow row in this.gv_WearerList_Edit.Rows)
        {
            CheckBox chk = (CheckBox)row.FindControl("chkbxSelectWearerList_Edit");
            if (chk.Checked)
            {
                DropDownList ddl_Edit = (DropDownList)row.FindControl("ddlBodyRegion_Edit");
                HiddenField hidUserID_Edit = (HiddenField)row.FindControl("HidUserID_Edit");
                curUserID = int.Parse(hidUserID_Edit.Value);
                curBodyRegionID = int.Parse(ddl_Edit.SelectedValue);

                if (pUserID == curUserID && pBodyRegionID == curBodyRegionID) cnt++;
            }
        }
        if (cnt > 1)
            return true;

        // check if already assignned on all session variables by colors  
        if (isAlreadyAssignedInSessionVariables(pUserID, pBodyRegionID, lblColor_Edit.Text))
            return true;

        // check if already assignned to UserDevices 
        if (isAlreadyAssigned(pUserID, pBodyRegionID))
            return true;

        // check if already assignned in the order which has not shipped yet
        if (isAssignedInPendingOrder(pUserID, pBodyRegionID))
            return true;

        return false;
    }

    private bool ValidateAssignedTotal(ref string pMessage)
    {
        try
        {
            int totalQty = int.Parse(lblTotalQty.Text);
            int totalAssigned = int.Parse(lblTotalAssignedWearer.Text);
            int remaining = totalQty - totalAssigned;
            int cntSelect = 0;

            foreach (GridViewRow row in this.gv_WearerList.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkbxSelectWearerList");
                if (chk.Checked) cntSelect++;
            }

            if (cntSelect > remaining)
            {
                pMessage = "Number of assigning users is more than total quantity by " + (cntSelect - remaining).ToString();
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            pMessage = ex.ToString();
            return false;
        }
    }

    private bool ValidateAssignedUsers(ref string pMessage)
    {
        List<int> overAssignUserList = new List<int>();
        int curUserID = 0, curBodyRegionID = 0;
        try
        {
            foreach (GridViewRow row in this.gv_WearerList.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkbxSelectWearerList");
                if (chk.Checked)
                {
                    DropDownList ddl = (DropDownList)row.FindControl("ddlBodyRegion");
                    HiddenField hidUserID = (HiddenField)row.FindControl("HidUserID");
                    curUserID = int.Parse(hidUserID.Value);
                    curBodyRegionID = int.Parse(ddl.SelectedValue);

                    if (isMultipleAssigning(curUserID, curBodyRegionID))
                    {
                        if (!overAssignUserList.Contains(curUserID))
                            overAssignUserList.Add(curUserID);
                    }
                }
            }

            if (overAssignUserList.Count > 0)
            {
                string headerErrorMsg = "Multi-assignments to the same body region for a user is not allowed.<br>Please check the assignning grid, pending order, or user assign device in account.<br>UserIDs:<br>";
                pMessage = GenerateMessage(overAssignUserList, headerErrorMsg);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            pMessage = ex.ToString();
            return false;
        }
    }

    private bool ValidateAssignedUsers_Edit(ref string pMessage)
    {
        List<int> overAssignUserList = new List<int>();
        int curUserID = 0, curBodyRegionID = 0;
        try
        {
            foreach (GridViewRow row in this.gv_WearerList_Edit.Rows)
            {
                CheckBox chk = (CheckBox)row.FindControl("chkbxSelectWearerList_Edit");
                if (chk.Checked)
                {
                    DropDownList ddl_Edit = (DropDownList)row.FindControl("ddlBodyRegion_Edit");
                    HiddenField hidUserID_Edit = (HiddenField)row.FindControl("HidUserID_Edit");
                    curUserID = int.Parse(hidUserID_Edit.Value);
                    curBodyRegionID = int.Parse(ddl_Edit.SelectedValue);

                    if (isMultipleAssigning_Edit(curUserID, curBodyRegionID))
                    {
                        if (!overAssignUserList.Contains(curUserID))
                            overAssignUserList.Add(curUserID);
                    }
                }
            }

            if (overAssignUserList.Count > 0)
            {
                string headerErrorMsg = "Multi-assignments to the same body region for a user is not allowed.<br>Please check the assignning grid, pending order, or user assign device in account.<br>UserIDs:<br>";
                pMessage = GenerateMessage(overAssignUserList, headerErrorMsg);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            pMessage = ex.ToString();
            return false;
        }
    }

    private bool OrderPassValidation(ref string pErrorMsg)
    {
        if (!Instadose1GridPassValidation(ref pErrorMsg)) return false;
        if (!InstadosePlusGridPassValidation(ref pErrorMsg)) return false;

        return true;
    }

    private bool Instadose1GridPassValidation(ref string pErrorMsg)
    {
        if (this.orderType == OrderTypes.Replacement || this.orderType == OrderTypes.NoReplacement || this.orderType == OrderTypes.NoChargeReplacement)
        {
            int totalEntered = 0;
            int totalSelectedSN = 0;

            for (int i = 0; i < this.ckblSerialNumber.Items.Count; i++)
            {
                if (this.ckblSerialNumber.Items[i].Selected == true && this.ckblSerialNumber.Items[i].Enabled == true)
                {
                    totalSelectedSN++;
                }
            }

            foreach (GridViewRow rowitem in gvProduct.Rows)
            {
                TextBox txtQty = (TextBox)rowitem.FindControl("txtQty");

                if (int.TryParse(txtQty.Text, out int lineQty))
                {
                    if (lineQty > 0) totalEntered += lineQty;
                }
            }

            if (totalEntered != totalSelectedSN)
            {
                if (this.orderType == OrderTypes.Replacement || this.orderType == OrderTypes.NoReplacement)
                {
                    pErrorMsg = "The quantity of selected Lost serial numbers must be equal the total entered quantity of order.";
                }
                else
                {
                    pErrorMsg = "The quantity of selected Broken serial numbers must be equal the total entered quantity of order.";
                }
                return false;
            }
        }

        return true;
    }

    private bool InstadosePlusGridPassValidation(ref string pErrorMsg)
    {
        foreach (GridViewRow rowitem in gvInstadosePlus.Rows)
        {
            int lineQty = 0;
            int assignedNumber = 0;

            TextBox txtQty = (TextBox)rowitem.FindControl("txtQty");
            LinkButton lButton = (LinkButton)rowitem.FindControl("btnAssignedWearerByProduct");

            int.TryParse(txtQty.Text, out lineQty);
            int.TryParse(lButton.Text, out assignedNumber);

            if (assignedNumber > lineQty)
            {
                pErrorMsg = "Number of assigned wearer can not be greater than number of order quantity";
                return false;
            }
        }

        return true;
    }

    private bool Instadose2GridPassValidation(ref string pErrorMsg)
    {
        foreach (GridViewRow rowitem in gvInstadose2.Rows)
        {
            int lineQty = 0;
            int assignedNumber = 0;

            TextBox txtQty = (TextBox)rowitem.FindControl("txtQty");
            LinkButton lButton = (LinkButton)rowitem.FindControl("btnAssignedWearerByProduct");

            int.TryParse(txtQty.Text, out lineQty);
            int.TryParse(lButton.Text, out assignedNumber);

            if (assignedNumber > lineQty)
            {
                pErrorMsg = "Number of assigned wearer can not be greater than number of order quantity";
                return false;
            }
        }

        return true;
    }

    private void GenerateSerialNoCheckBoxList()
    {
        int[] id1ProductIDs = new int[2] { 1, 9 };

        // Get the latest Devices List of the AccountID. Only for ID1 
        // Notice: a device can be assigned back to an account many times sometimes.
        var varAllidcData = (from AcctDevices in idc.AccountDevices
                             join DevicesInv in idc.DeviceInventories on AcctDevices.DeviceID equals DevicesInv.DeviceID
                             join Prod in idc.Products on DevicesInv.ProductID equals Prod.ProductID
                             where AcctDevices.AccountID == this.account.AccountID && AcctDevices.CurrentDeviceAssign == true
                             && id1ProductIDs.Contains(Prod.ProductGroupID)
                             orderby DevicesInv.SerialNo
                             select new { AcctDevices, DevicesInv }).ToList();

        // Get all returning devices for an account which were not received by receiving/shipping. 
        // if the device was received by receiving/shipping, then ReturnDevices.Received flag = true and 
        // UserDevice and AccountDevice for this device will be inactive and CurrentDeviceAssign = false
        var varAllAdcData = (from Rtn in adc.rma_Returns
                             join RtnDevices in adc.rma_ReturnDevices on Rtn.ReturnID equals RtnDevices.ReturnID
                             where Rtn.AccountID == this.account.AccountID
                             && RtnDevices.Active == true
                             && RtnDevices.Received == false
                             orderby RtnDevices.SerialNo
                             select new { RtnDevices }).ToList();

        // account-devices left join all returning devices 
        var JoinAllData = (from Allidc in varAllidcData
                           join AllADC in varAllAdcData on Allidc.DevicesInv.SerialNo equals AllADC.RtnDevices.SerialNo into join2
                           from AllIdcAdc in join2.DefaultIfEmpty()
                           select new { Allidc, AllIdcAdc }).ToList();

        // CheckBoxList -- get all devices belong to accountID
        foreach (var v in JoinAllData)
        {
            ListItem listDevices = new ListItem();
            listDevices.Value = v.Allidc.AcctDevices.DeviceID.ToString();
            listDevices.Text = v.Allidc.DevicesInv.SerialNo.ToString();
            listDevices.Selected = false;

            if (v.AllIdcAdc != null)
            {
                listDevices.Enabled = false;
                listDevices.Selected = true;
                listDevices.Attributes.Add("Title", "This device is already initially returned in the Return #" + v.AllIdcAdc.RtnDevices.ReturnID.ToString());
            }
            this.ckblSerialNumber.Items.Add(listDevices);
        }

    }

    private int ProcessRMA(OrderTypes pOrderType, int pOrderID)
    {
        int NewReturnID = CreateReturn(pOrderType, pOrderID);

        if (NewReturnID > 0)
        {
            CreateReturnDevices(NewReturnID, pOrderType);
        }

        return NewReturnID;
    }

    private int CreateReturn(OrderTypes pOrderType, int pOrderID)
    {
        try
        {
            int returnTypeID = 0, returnReasonID = 0;
            string reason = "";
            string rmaNote = "Generated by Lost-Replacement order.";

            if (pOrderType == OrderTypes.Replacement || pOrderType == OrderTypes.NoReplacement)
            {
                returnTypeID = 9;   // 'Lost (No Return)' from rma_ref_ReturnTypes table in LCDISBUS db
                reason = "Lost (No Return)";
                returnReasonID = 16;    // 'Lost (No Return)' from rma_ref_ReturnReason table in LCDISBUS db
            }
            else
            {
                returnTypeID = 8;
                reason = "Broken";
                returnReasonID = 10;    // 'Broken' from rma_ref_ReturnReason table in LCDISBUS db
            }

            rma_Return rma = null;
            rma = new rma_Return()
            {
                AccountID = this.account.AccountID,
                Active = true,
                Notes = rmaNote,
                Reason = reason,
                Return_ReasonID = returnReasonID,
                ReturnTypeID = returnTypeID,
                Status = 1,
                OrderID = pOrderID,
                CreatedBy = this.userName,
                CreatedDate = DateTime.Now
            };
            adc.rma_Returns.InsertOnSubmit(rma);
            adc.SubmitChanges();

            // Insert data to Transaction Log with new ReturnID
            var writeTransLogReturn = adc.sp_rma_process(rma.ReturnID, 0, 0, rmaNote, this.userName, DateTime.Now, "ADD RETURN", "New return ID: " + rma.ReturnID.ToString(), 2);

            // return Notes to Header table
            rma_RMAHeader header = null;
            header = new rma_RMAHeader()
            {
                RMANumber = "0",
                ReturnID = rma.ReturnID,
                Reason = rmaNote,
                Active = true,
                CreatedBy = this.userName,
                CreatedDate = DateTime.Now
            };
            adc.rma_RMAHeaders.InsertOnSubmit(header);
            adc.SubmitChanges();

            // Insert data to Transaction Log with new ReturnID 
            var writeTransLogReturnHeader = adc.sp_rma_process(rma.ReturnID, 0, 0, rmaNote, this.userName, DateTime.Now, "ADD RETURN HEADER", "Add retrunID: " + rma.ReturnID.ToString(), 2);

            // Return the ID that was generated.
            return rma.ReturnID;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    private void CreateReturnDevices(int pReturnID, OrderTypes pOrderType)
    {
        try
        {
            string rmaNote = "Generated by Lost-Replacement order.";
            int rmaDepartment = 2;

            rma_ReturnDevice retrunDevices = null;

            for (int i = 0; i < this.ckblSerialNumber.Items.Count; i++)
            {
                if (this.ckblSerialNumber.Items[i].Selected == true && this.ckblSerialNumber.Items[i].Enabled == true)
                {
                    string sn = ckblSerialNumber.Items[i].Text.ToString();
                    int deviceID = int.Parse(ckblSerialNumber.Items[i].Value.ToString());

                    List<rma_ReturnDevice> returnDevice = (from rd in adc.rma_ReturnDevices
                                                           where rd.SerialNo == sn && rd.Active == true
                                                           select rd).ToList();

                    // Deactivate all previous returndevice of serialno.
                    foreach (rma_ReturnDevice dbActive in returnDevice)
                    {
                        dbActive.Active = false;
                    }

                    // Insert new returndevice
                    retrunDevices = new rma_ReturnDevice();

                    retrunDevices.ReturnID = pReturnID;
                    retrunDevices.SerialNo = sn;
                    retrunDevices.MasterDeviceID = deviceID;
                    retrunDevices.Notes = rmaNote;
                    retrunDevices.DepartmentID = rmaDepartment;
                    retrunDevices.Status = 1;
                    retrunDevices.Active = true;

                    // Grab device UserID 
                    var MyUserID = (from a in idc.UserDevices
                                    where a.DeviceID == deviceID
                                    && a.Active == true
                                    select a.UserID).FirstOrDefault();
                    if (MyUserID != null && MyUserID > 0)
                        retrunDevices.UserID = MyUserID;

                    adc.rma_ReturnDevices.InsertOnSubmit(retrunDevices);
                    adc.SubmitChanges();

                    // add transLog with new RetrunDevicesID
                    var writeTransLogReturn = adc.sp_rma_process(pReturnID, retrunDevices.ReturnDevicesID, 0, rmaNote, this.userName, DateTime.Now, "ADD RETURN DEVICE", "New ReturnDevice ID: " + retrunDevices.ReturnDevicesID.ToString(), 2);

                    // if it is a lost then update device status = "Lost Not Returned"
                    if (pOrderType == OrderTypes.Replacement || pOrderType == OrderTypes.NoReplacement)
                    {
                        DeviceInventory deviceInventory = (from di in idc.DeviceInventories where di.SerialNo == sn select di).FirstOrDefault();

                        DeviceAnalysisStatus lostStatus = (from s in idc.DeviceAnalysisStatus where s.DeviceAnalysisName == "Lost-NotReturned" select s).FirstOrDefault();
                        if (lostStatus != null)
                        {
                            deviceInventory.DeviceAnalysisStatus = lostStatus;
                            idc.SubmitChanges();
                        }

                        //Update device audit
                        DeviceManager myDeviceManager = new DeviceManager();
                        myDeviceManager.InsertDeviceInventoryAudit(deviceInventory.DeviceID, deviceInventory.DeviceAnalysisStatus.DeviceAnalysisStatusID, false, "RMA Lost Order Creation", "Create Order Page");
                    }

                    DeactivateDevice(deviceID, sn, pReturnID);
                }
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    private void DeactivateDevice(int pDeviceID, string pSN, int pReturnID)
    {
        // ----------- Deactivate a device off an user and account. -----------------//              
        try
        {
            // ************* Deactivate UserDevice ********************** //
            UserDevice UDev = (from a in idc.UserDevices
                               join ad in idc.AccountDevices on a.DeviceID equals ad.DeviceID
                               where a.DeviceID == pDeviceID
                               && a.Active == true
                               && ad.CurrentDeviceAssign == true
                               select a).FirstOrDefault();
            if (UDev != null)
            {
                UDev.Active = false;
                UDev.DeactivateDate = DateTime.Now;
                idc.SubmitChanges();
            }
            // ************* Deactivate UserDevice ********************** //

            // ********** Deactivate AccountDevice ********************//
            IQueryable<AccountDevice> ADev = (from a in idc.AccountDevices
                                              where a.DeviceID == pDeviceID && a.Active == true
                                              select a).AsQueryable();
            foreach (AccountDevice ad in ADev)
            {
                ad.Active = false;
                ad.DeactivateDate = DateTime.Now;
                idc.SubmitChanges();
            }
            // ********** Deactivate AccountDevice ********************//

            // insert TransLog
            adc.sp_rma_process(pReturnID, 0, 0, " ", this.userName, DateTime.Now, "DEACTIVATE", "ReturnID: " + pReturnID.ToString() + ", Deactive Serial# " + pSN, 2);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    private DateTime GetDefaultSoftTraxStartDate(bool pIsFirstOrder, DateTime pOrderCreatedDate)
    {
        try
        {
            if (pIsFirstOrder)
                return GetContractStartDate();
            else
            {
                DateTime startQuarterDate;
                int qtrNo = Common.calculateNumberOfQuarterService(GetContractStartDate(), GetContractEndDate(), pOrderCreatedDate, out startQuarterDate);
                return startQuarterDate;
            }
        }
        catch (Exception)
        {
            return GetContractStartDate();
        }
    }

    // --------------------------------------------------------------------------------------------//

}