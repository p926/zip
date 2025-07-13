using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Collections.Generic;

using Instadose;
using Instadose.Data;
using Instadose.Processing;

using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.IO;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Threading;

public partial class Manufacturing_ShippingQueue : System.Web.UI.Page
{
    // String to hold the current username
    string UserName = "Unknown";

    // Create the database reference
    InsDataContext idc = new InsDataContext();
    MASDataContext mdc = new MASDataContext();
    AppDataContext adc = new AppDataContext();

    //Crystal report
    ReportDocument cryRpt;

    protected void Page_PreRenderComplete(object sender, EventArgs e)
    {
        // display user shipment count in Queue
        this.lbl_OpenPickSheet.Text = (gv_PickSheet.Rows.Count > 0) ?
            (gv_PickSheet.Rows.Count.ToString() + "  records found.") : "";


        this.lblShippingHistoryResult.Text = (gv_ShippingHistory.Rows.Count > 0) ?
            (gv_ShippingHistory.Rows.Count.ToString() + " shipping records found.") : "";        
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        // Try to set the username
        try
        {
            this.UserName = User.Identity.Name.Split('\\')[1];
        }
        catch { this.UserName = "Unknown"; }

        if (!IsPostBack)
        {
            // 2/3/2015, tdo. Requested by Gordon.
            ////Find first date of the week
            //CultureInfo info = Thread.CurrentThread.CurrentCulture;
            //DayOfWeek firstday = info.DateTimeFormat.FirstDayOfWeek;
            //DayOfWeek today = info.Calendar.GetDayOfWeek(DateTime.Now);

            //int diff = today - firstday;
            //DateTime firstDate = DateTime.Now.AddDays(-diff);
            //this.txt_Startdate.Text = firstDate.ToShortDateString();

            this.txt_Startdate.Text = DateTime.Now.ToShortDateString();
            this.txt_Enddate.Text = DateTime.Now.AddDays(1).ToShortDateString();
            this.BindOrderCountSummary();
        }

    }


    #region PickSheetFunctions

    protected void btn_refreshPickSheet_Click(object sender, EventArgs e)
    {
        gv_PickSheet.DataBind();
        //gv_PackingList.DataBind();
        //gv_ShippingList.DataBind();
        //gv_openshipment.DataBind();       
    }

    protected void btnPrintPickSheets_Click(object sender, EventArgs e)
    {
        lbl_PrintPickSheet.Visible = false;
        string PSMessage = "Print Pick Sheets for Order# ";
        string PSOrderID = "";
        for (int i = 0; i < this.gv_PickSheet.Rows.Count; i++)
        {
            GridViewRow gvRow = gv_PickSheet.Rows[i];
            CheckBox findChkBx = (CheckBox)gvRow.FindControl("chkbxSelectPickSheet");
            bool SelectedOrder = findChkBx.Checked;

            if (SelectedOrder == true)
            {
                int SelectedOrderID = int.Parse(((HiddenField)gvRow.FindControl("HidOrderID")).Value);

                PSOrderID += SelectedOrderID.ToString() + ",";

                // update order status ID to "6-Picked" 
                Order Ord = null;
                Ord = (from a in idc.Orders
                       where a.OrderID == SelectedOrderID
                       select a).First();

                Ord.OrderStatusID = 6;
                idc.SubmitChanges();
            }

        } // End For Loop

        if (PSOrderID != "")
        {
            PSOrderID = PSOrderID.Substring(0, PSOrderID.Length - 1);

            string url = "ShippingViewCrystalReport.aspx?id=" + PSOrderID + "&report=Picking";
            ScriptManager.RegisterStartupScript(this, typeof(string), "OPEN_WINDOW", "window.open( '" + url + "', 'picking', 'height=450,width=800,status=yes,toolbar=yes,menubar=no,location=no,resize=yes' );", true);

            lbl_PrintPickSheet.Visible = true;
            lbl_PrintPickSheet.Text = PSMessage + PSOrderID;

            //gv_PackingList.DataBind();
        }
        gv_PickSheet.DataBind();
    }

    protected void gv_PickSheet_Sorting(object sender, GridViewSortEventArgs e)
    {
        SqlOpenPickSheet.FilterExpression = ViewState["PickingSheetsFilter"] != null ? ViewState["PickingSheetsFilter"].ToString() : null;
        gv_PickSheet.DataBind();
    }

    #endregion


    #region PackingListFunctions

    protected void btn_refreshPackingList_Click(object Sender, EventArgs e)
    {
        //gv_PickSheet.DataBind();
        gv_PackingList.DataBind();
        //gv_ShippingList.DataBind();
        //gv_openshipment.DataBind();        
    }

    protected void btnPrintPackingList_Click(object sender, EventArgs e)
    {
        lbl_PrintPackingList.Visible = false;

        string PSMessage = "Print Packing list for Order# ";
        string PSOrderID = "";
        string StrPackageID = "";

        for (int i = 0; i < this.gv_PackingList.Rows.Count; i++)
        {
            GridViewRow gvRow = gv_PackingList.Rows[i];
            CheckBox findChkBx = (CheckBox)gvRow.FindControl("chkbxSelectPackingList");
            bool SelectedOrder = findChkBx.Checked;

            if (SelectedOrder == true)
            {
                int SelectedOrderID = int.Parse(((HiddenField)gvRow.FindControl("HidOrderID")).Value);

                PSOrderID += SelectedOrderID.ToString() + ",";

                // update order status ID to "7-Packed" 
                Order Ord = null;
                Ord = (from a in idc.Orders
                       where a.OrderID == SelectedOrderID
                       select a).First();

                Ord.OrderStatusID = 7;
                idc.SubmitChanges();

                // generate package information and 
                var ScanSerial = (from a in mdc.FromMAS_SO_InvoiceHeaders
                                  join b in mdc.FromMAS_SO_InvoiceTierDistributions
                                      on a.InvoiceNo equals b.InvoiceNo
                                  where a.FOB == SelectedOrderID.ToString()
                                  select new { b.LotSerialNo }).ToList();

                var PackageDetail = (from a in ScanSerial
                                     join c in idc.DeviceInventories
                                     on a.LotSerialNo equals c.SerialNo
                                     join d in idc.OrderDetails
                                         on c.ProductID equals d.ProductID
                                     where d.OrderID == SelectedOrderID
                                     select new { a.LotSerialNo, d.OrderID, c.DeviceID, d.OrderDetailID }).ToList();

                var ShipAddress = (from a in idc.Locations
                                   join b in idc.Orders
                                    on a.LocationID equals b.LocationID
                                   where b.OrderID == SelectedOrderID
                                   select new { a, b }).FirstOrDefault();

                // In Recall process, when it automatically generates a Recall Replacement order, 
                // the ShippingUserID field was committed if we select ship to user address.
                var UserShipAddress = (from a in idc.Users
                                       join b in idc.Orders
                                       on a.UserID equals b.ShippingUserID
                                       join c in idc.Locations 
                                       on a.LocationID equals c.LocationID 
                                       where b.OrderID == SelectedOrderID
                                       select new { a, b, c }).FirstOrDefault();


                int NewPackageId = 0;
                string shippingFirstName, shippingLastName, shippingAddress1, shippingAddress2, shippingAddress3, shippingCity, shippingPostalCode, shippingTelephone;
                int? shippingStateID, shippingCountryID;

                // use location shipping address if UserShipaddress not found
                if (UserShipAddress == null)
                {
                    shippingFirstName = ShipAddress.a.ShippingFirstName;
                    shippingLastName = ShipAddress.a.ShippingLastName;
                    shippingAddress1 = string.IsNullOrEmpty(ShipAddress.a.ShippingAddress1) ? "" : ShipAddress.a.ShippingAddress1;
                    shippingAddress2 = string.IsNullOrEmpty(ShipAddress.a.ShippingAddress2) ? "" : ShipAddress.a.ShippingAddress2;
                    shippingAddress3 = string.IsNullOrEmpty(ShipAddress.a.ShippingAddress3) ? "" : ShipAddress.a.ShippingAddress3;
                    shippingCity = ShipAddress.a.ShippingCity;
                    shippingStateID = ShipAddress.a.ShippingStateID;
                    shippingPostalCode = ShipAddress.a.ShippingPostalCode;
                    shippingCountryID = ShipAddress.a.ShippingCountryID;
                    shippingTelephone = string.IsNullOrEmpty(ShipAddress.a.ShippingTelephone) ? "" : ShipAddress.a.ShippingTelephone;
                }
                else // use UserShippingAddress
                {
                    // if no user address then getting user's location address
                    //if (string.IsNullOrEmpty(UserShipAddress.a.Address1) && string.IsNullOrEmpty(UserShipAddress.a.Address2) && string.IsNullOrEmpty(UserShipAddress.a.Address3))
                    if (string.IsNullOrEmpty(UserShipAddress.a.Address1))
                    {
                        shippingFirstName = UserShipAddress.c.ShippingFirstName;
                        shippingLastName = UserShipAddress.c.ShippingLastName;
                        shippingAddress1 = string.IsNullOrEmpty(UserShipAddress.c.ShippingAddress1) ? "" : UserShipAddress.c.ShippingAddress1;
                        shippingAddress2 = string.IsNullOrEmpty(UserShipAddress.c.ShippingAddress2) ? "" : UserShipAddress.c.ShippingAddress2;
                        shippingAddress3 = string.IsNullOrEmpty(UserShipAddress.c.ShippingAddress3) ? "" : UserShipAddress.c.ShippingAddress3;
                        shippingCity = UserShipAddress.c.ShippingCity;
                        shippingStateID = UserShipAddress.c.ShippingStateID;
                        shippingPostalCode = UserShipAddress.c.ShippingPostalCode;
                        shippingCountryID = UserShipAddress.c.ShippingCountryID;
                        shippingTelephone = string.IsNullOrEmpty(UserShipAddress.c.ShippingTelephone) ? "" : UserShipAddress.c.ShippingTelephone;
                    }
                    else
                    {
                        shippingFirstName = UserShipAddress.a.FirstName;
                        shippingLastName = UserShipAddress.a.LastName;
                        shippingAddress1 = string.IsNullOrEmpty(UserShipAddress.a.Address1) ? "" : UserShipAddress.a.Address1;
                        shippingAddress2 = string.IsNullOrEmpty(UserShipAddress.a.Address2) ? "" : UserShipAddress.a.Address2;
                        shippingAddress3 = string.IsNullOrEmpty(UserShipAddress.a.Address3) ? "" : UserShipAddress.a.Address3;
                        shippingCity = UserShipAddress.a.City;
                        shippingStateID = UserShipAddress.a.StateID.HasValue ? UserShipAddress.a.StateID.Value : (int?)null;
                        shippingPostalCode = UserShipAddress.a.PostalCode;
                        shippingCountryID = UserShipAddress.a.CountryID.HasValue ? UserShipAddress.a.CountryID.Value : (int?)null;
                        shippingTelephone = string.IsNullOrEmpty(UserShipAddress.a.Telephone) ? "" : UserShipAddress.a.Telephone;
                    }                    
                }

                // create package for an order
                Package NP = new Package
                {
                    OrderID = SelectedOrderID,
                    TrackingNumber = "",
                    ShipDate = DateTime.Now,
                    Company = ShipAddress.a.ShippingCompany,
                    FirstName = shippingFirstName,
                    LastName = shippingLastName,
                    Address1 = shippingAddress1,
                    Address2 = shippingAddress2,
                    Address3 = shippingAddress3,
                    City = shippingCity,
                    StateID = shippingStateID,
                    PostalCode = shippingPostalCode,
                    CountryID = shippingCountryID,
                    Telephone = shippingTelephone
                };

                idc.Packages.InsertOnSubmit(NP);
                idc.SubmitChanges();
                // get the @@identity PackageID
                NewPackageId = NP.PackageID;
                StrPackageID += NewPackageId.ToString() + ",";

                // Create PackageOrderDetails by PackageID
                foreach (var packDetail in PackageDetail)
                {
                    PackageOrderDetail NPod = new PackageOrderDetail
                    {
                        PackageID = NewPackageId,
                        OrderDetailID = packDetail.OrderDetailID,
                        DeviceID = packDetail.DeviceID,
                        UserID = Ord.ShippingUserID.HasValue ? Ord.ShippingUserID.Value : (int?)null
                    };
                    idc.PackageOrderDetails.InsertOnSubmit(NPod);
                    idc.SubmitChanges();
                }

                // 1/24/2013, TDO
                // Assigning every device shipped by the order to an account. 
                // By executing this process at this step, we will remove this process in Batch Processing.
                AssignAccountDeviceForOrder(SelectedOrderID);

                // Tdo, 4/12/2012. 
                // Check if this is a RECALL REPLACEMENT order. If it is then doing user device assignment
                AssignUserDeviceForRecallReplacement(SelectedOrderID);
            }

        } // End For Loop

        if (StrPackageID != "")
        {
            StrPackageID = StrPackageID.Substring(0, PSOrderID.Length - 1);

            string url = "ShippingViewCrystalReport.aspx?id=" + StrPackageID + "&report=Packing";
            ScriptManager.RegisterStartupScript(this, typeof(string), "OPEN_WINDOW", "window.open( '" + url + "', 'picking', 'height=450,width=800,status=yes,toolbar=yes,menubar=no,location=no,resize=yes' );", true);

            lbl_PrintPackingList.Visible = true;
            lbl_PrintPackingList.Text = PSMessage +
                PSOrderID.Substring(0, PSOrderID.Length - 1);

            //gv_ShippingList.DataBind();
        }
        gv_PackingList.DataBind();

    }

    private void AssignAccountDeviceForOrder(Int32 SelectedOrderID)
    {
        ProcessOrders Orders = new ProcessOrders(this.UserName);
        ProcessResponse DeviceAssignmentCompleted = Orders.DeviceAssignment(SelectedOrderID);
    }

    public bool DisplayPackingListCheckBox(string orderID)
    {
        Boolean isDisplay = false;

        try
        {
            var OrdDetCount = (from a in idc.OrderDetails
                               where a.ProductID <= 5  // only count productid from 1 to 5
                               && a.OrderID == int.Parse(orderID)
                               group a by new { a.OrderID } into g
                               select new { myOrder = g.Key.OrderID, mySum = g.Sum(a => a.Quantity) }).First();

            int myOrderQtyCount = OrdDetCount.mySum;

            int MasDataCount = (from a in mdc.FromMAS_SO_InvoiceHeaders
                                join b in mdc.FromMAS_SO_InvoiceTierDistributions
                                on a.InvoiceNo equals b.InvoiceNo
                                where a.FOB == orderID
                                select b.LotSerialNo).Count();

            if (myOrderQtyCount == MasDataCount)
                isDisplay = true;
        }
        catch
        {
            isDisplay = false;
        }

        return isDisplay;
    }


    protected void btnReprintPickingSheet_Click(object sender, EventArgs e)
    {
        string StrReprintPickingOrdID = txtReprintPickingSheet.Text.Trim();
        if (StrReprintPickingOrdID != "")
        {
            txtReprintPickingSheet.Text = "";
            string url = "ShippingViewCrystalReport.aspx?id=" + StrReprintPickingOrdID + "&report=Picking";
            ScriptManager.RegisterStartupScript(this, typeof(string), "OPEN_WINDOW", "window.open( '" + url + "', 'picking', 'height=450,width=800,status=yes,toolbar=yes,menubar=no,location=no,resize=yes' );", true);

            lbl_PrintPackingList.Visible = true;
            lbl_PrintPackingList.Text = "Reprint Picking Sheet for order# " + StrReprintPickingOrdID;

        }
    }

    #endregion


    #region ShippingList & Instructions

    protected void btn_refreshShippingInstruction_Click(object Sender, EventArgs e)
    {
        //gv_PickSheet.DataBind();
        //gv_PackingList.DataBind();
        gv_ShippingList.DataBind();
        gv_openshipment.DataBind();
    }


    protected void btnRePrintPackingList_Click(object sender, EventArgs e)
    {
        string StrReprintPackageID = txtReprintPacking.Text.Trim();
        if (StrReprintPackageID != "")
        {

            txtReprintPacking.Text = "";
            string url = "ShippingViewCrystalReport.aspx?id=" + StrReprintPackageID + "&report=Packing";
            ScriptManager.RegisterStartupScript(this, typeof(string), "OPEN_WINDOW", "window.open( '" + url + "', 'picking', 'height=450,width=800,status=yes,toolbar=yes,menubar=no,location=no,resize=yes' );", true);

            lbl_RePrintPackingList.Visible = true;
            lbl_RePrintPackingList.Text = "Reprint Packing List for Package# " +
                StrReprintPackageID;
        }
    }

    #endregion


    #region Shipping History Functions


    protected void btn_btnFindRecordDate_Click(object sender, EventArgs e)
    {
        gv_ShippingHistory.DataBind();
    }

    #endregion


    protected void GeneratePDF(string strOrder, string CrystalReportFileName)
    {
        string myCRFileNamePath = Server.MapPath("~/CustomerService/" + CrystalReportFileName); ;
        try
        {
            cryRpt = new ReportDocument();
            cryRpt.Load(myCRFileNamePath);

            cryRpt.SetParameterValue(0, strOrder);

            string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString.ToString();
            string myDatabase = Regex.Match(ConnectionString, "(Initial Catalog=|Database=){1}([A-Za-z0-9_]+)[;]?").Groups[2].ToString();
            string myServer = Regex.Match(ConnectionString, "(Data Source=){1}([\\w\\d\\.\\$\\-]+)[;]?").Groups[2].ToString();
            string myUserID = Regex.Match(ConnectionString, "(User ID=|UID=){1}([\\w\\d\\.]+)[;]?").Groups[2].ToString();
            string myPassword = Regex.Match(ConnectionString, "(Pwd=|Password=|PWD=){1}([\\w\\d\\.\\$]+)[;]?").Groups[2].ToString();

            cryRpt.DataSourceConnections[0].SetConnection(myServer, myDatabase, myUserID, myPassword);
            //cryRpt.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, myPDFfilePath);

            //// viewing generated PDF file
            //string myURL = "http://" + Request.Url.Authority.ToString() + "/Instadose/Reports/" + myPDFfileName;
            //Response.Redirect(myURL);

            //export to memory Stream
            var oStream = cryRpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            byte[] streamArray = new byte[oStream.Length];
            oStream.Read(streamArray, 0, Convert.ToInt32(oStream.Length - 1));

            Response.Clear();
            Response.Buffer = true;
            Response.ContentType = "application/pdf";
            Response.BinaryWrite(streamArray);
            Response.End();

        }

        catch (Exception e)
        {
            //Response.Write(e);
        }

    }

    public string GenerateBillingInfo_CleanString(string billingInfo)
    {        
        return billingInfo.Replace("\\r\\n", "\r\n");
    }

    public string GenerateBillingInfo(string strOrderID)
    {
        string returnStr = "";
        var bInfo = (from a in idc.Orders
                     join b in idc.Locations
                     on a.LocationID equals b.LocationID
                     join c in idc.States
                     on b.BillingStateID equals c.StateID
                     join d in idc.Countries
                     on b.BillingCountryID equals d.CountryID
                     where a.OrderID == int.Parse(strOrderID)
                     select new { b, c, d }).FirstOrDefault();
        if (bInfo != null)
        {
            returnStr = bInfo.b.BillingCompany + "\r\n";
            returnStr += bInfo.b.BillingFirstName + " ";
            returnStr += bInfo.b.BillingLastName + "\r\n";
            returnStr += bInfo.b.BillingAddress1 + "\r\n";

            returnStr += (bInfo.b.BillingAddress2 != null && bInfo.b.BillingAddress2 != "") ?
                         bInfo.b.BillingAddress2 + "\r\n" : "";

            returnStr += (bInfo.b.BillingAddress3 != null && bInfo.b.BillingAddress3 != "") ?
                bInfo.b.BillingAddress3 + "\r\n" : "";

            returnStr += bInfo.b.BillingCity + ", ";
            returnStr += bInfo.c.StateAbbrev + " ";
            returnStr += bInfo.b.BillingPostalCode + "\r\n";
            returnStr += bInfo.d.CountryName + "\r\n";

        }


        return returnStr;
    }

    public string GeneratePackShippingInfo(string strPackageID)
    {
        string returnStr = "";
        var pInfo = (from b in idc.Packages
                     join c in idc.States
                     on b.StateID equals c.StateID
                     join d in idc.Countries
                     on b.CountryID equals d.CountryID
                     where b.PackageID == int.Parse(strPackageID)
                     select new { b, c, d }).FirstOrDefault();

        if (pInfo != null)
        {
            returnStr = pInfo.b.Company + "\r\n";
            returnStr += pInfo.b.FirstName + " ";
            returnStr += pInfo.b.LastName + "\r\n";
            returnStr += pInfo.b.Address1 + "\r\n";

            returnStr += (pInfo.b.Address2 != null && pInfo.b.Address2 != "") ?
                         pInfo.b.Address2 + "\r\n" : "";

            returnStr += (pInfo.b.Address3 != null && pInfo.b.Address3 != "") ?
                pInfo.b.Address3 + "\r\n" : "";

            returnStr += pInfo.b.City + ", ";
            returnStr += pInfo.c.StateAbbrev + " ";
            returnStr += pInfo.b.PostalCode + "\r\n";
            returnStr += pInfo.d.CountryName + "\r\n";

        }


        return returnStr;
    }

    public string GenerateShippingInfo(string strOrderID)
    {
        string returnStr = "";
        var bInfo = (from a in idc.Orders
                     join b in idc.Locations
                     on a.LocationID equals b.LocationID
                     join c in idc.States
                     on b.ShippingStateID equals c.StateID
                     join d in idc.Countries
                     on b.ShippingCountryID equals d.CountryID
                     where a.OrderID == int.Parse(strOrderID)
                     select new { a, b, c, d }).FirstOrDefault();

        var uInfo = (from a in idc.Orders
                     join u in idc.Users
                     on a.ShippingUserID equals u.UserID
                     join c in idc.States
                     on u.StateID equals c.StateID
                     join d in idc.Countries
                     on u.CountryID equals d.CountryID
                     where a.OrderID == int.Parse(strOrderID)
                     select new { a, u, c, d }).FirstOrDefault();


        if (bInfo != null)
        {
            returnStr = bInfo.b.ShippingCompany + "\r\n";
            returnStr += bInfo.b.ShippingFirstName + " ";
            returnStr += bInfo.b.ShippingLastName + "\r\n";
            returnStr += bInfo.b.ShippingAddress1 + "\r\n";

            returnStr += (bInfo.b.ShippingAddress2 != null && bInfo.b.ShippingAddress2 != "") ?
                         bInfo.b.ShippingAddress2 + "\r\n" : "";

            returnStr += (bInfo.b.ShippingAddress3 != null && bInfo.b.ShippingAddress3 != "") ?
                bInfo.b.ShippingAddress3 + "\r\n" : "";

            returnStr += bInfo.b.ShippingCity + ", ";
            returnStr += bInfo.c.StateAbbrev + " ";
            returnStr += bInfo.b.ShippingPostalCode + "\r\n";
            returnStr += bInfo.d.CountryName + "\r\n";
        }

        // if ShippingUserID !=null, then use User Shipping Address: mainly for RMA use
        if (uInfo != null)
        {
            returnStr = uInfo.u.FirstName + " ";
            returnStr += uInfo.u.LastName + "\r\n";
            returnStr += uInfo.u.Address1 + "\r\n";

            returnStr += (uInfo.u.Address2 != null && uInfo.u.Address2 != "") ?
                         uInfo.u.Address2 + "\r\n" : "";

            returnStr += (uInfo.u.Address3 != null && uInfo.u.Address3 != "") ?
                uInfo.u.Address3 + "\r\n" : "";

            returnStr += uInfo.u.City + ", ";
            returnStr += uInfo.c.StateAbbrev + " ";
            returnStr += uInfo.u.PostalCode + "\r\n";
            returnStr += uInfo.d.CountryName + "\r\n";
        }

        //5/24/12 WK - Test - Put back when done!
        //if (bInfo.a.LabelTypeID != null)
        //{
        //    returnStr += "\r\nLabel->" + 
        //        (from a in idc.LabelTypes 
        //            where a.LabelTypeID == bInfo.a.LabelTypeID 
        //            select a.LabelDesc).First();

        //}

        //*** end of 5/24/12 WK

        if (bInfo.a.PackageTypeID != null)
        {
            returnStr += "\r\nPackage->" +
                    (from a in idc.PackageTypes
                     where a.PackageTypeID == bInfo.a.PackageTypeID
                     select a.PackageDesc).First();
        }

        if (bInfo.a.ShippingOptionID != null)
        {
            returnStr += "\r\nOption->" +
                    (from a in idc.ShippingOptions
                     where a.ShippingOptionID == bInfo.a.ShippingOptionID
                     select a.ShippingOptionDesc).First();
        }

        if (bInfo.a.SpecialInstructions != null && bInfo.a.SpecialInstructions != "")
        {
            returnStr += "\r\nOthers->" + bInfo.a.SpecialInstructions;

        }


        return returnStr;
    }

    public string GenerateShipProduct(string strOrderID)
    {
        string returnStr = "";
        var ODetail = (from a in idc.OrderDetails
                       join b in idc.Products
                       on a.ProductID equals b.ProductID
                       where a.OrderID == int.Parse(strOrderID)
                       && a.ProductID != 11
                       select new { a, b }).ToList();
        if (ODetail.Count > 0)
        {
            foreach (var p in ODetail)
            {
                //string mySKU = p.b.ProductName.ToString() + " " + p.b.Color.ToString() + "<br/>";
                string mySKU = p.b.Color.ToString() + "<br/>";
                returnStr += "<b>" + p.a.Quantity.ToString() + "</b>-" + mySKU;
            }
        }


        return returnStr;
    }

    public string GenerateScanedSerialNo(string strOrderID)
    {
        string returnStr = "";
        var Ser = (from a in mdc.FromMAS_SO_InvoiceHeaders
                   join b in mdc.FromMAS_SO_InvoiceTierDistributions
                   on a.InvoiceNo equals b.InvoiceNo
                   where a.FOB == strOrderID
                   select b.LotSerialNo).ToList();
        if (Ser.Count > 0)
        {
            //returnStr = "Scanned: <b>" + ScanSerialNo.Count.ToString() + "</b><br>";
            int myNumberCount = 1;
            foreach (var v in Ser)
            {
                returnStr += "#" + myNumberCount.ToString() + "-" + v.ToString() + "\r\n";
                myNumberCount += 1;
            }
        }
        return returnStr;
    }

    public string GeneratePackSerialNo(string strPackageID)
    {
        string returnStr = "";
        var Ser = (from a in idc.PackageOrderDetails
                   join b in idc.DeviceInventories
                   on a.DeviceID equals b.DeviceID
                   where a.PackageID == int.Parse(strPackageID)
                   select b.SerialNo).ToList();
        if (Ser.Count > 0)
        {
            //returnStr += "Total: " + Ser.Count.ToString() + "\r\n";
            int myNumberCount = 1;
            foreach (var v in Ser)
            {
                returnStr += "#" + myNumberCount.ToString() + "-" + v.ToString() + "\r\n";
                myNumberCount += 1;
            }
        }
        return returnStr;
    }

    // Tdo, 4/12/2012. Check if this order is a RECALL REPLACEMENT order. 
    // If it is then doing user device assignment
    private void AssignUserDeviceForRecallReplacement(int pOrderID)
    {

        Order myOrder = (from a in idc.Orders
                         where a.OrderID == pOrderID
                         select a).FirstOrDefault();

        if (myOrder.OrderType.ToUpper() == "RECALL REPLACEMENT" && myOrder.PONumber.ToUpper() == "REPLACEDIS")
        {
            // This order is a recall replacement order. Get the userID to assign to a device if existing
            string specialInstruction = myOrder.SpecialInstructions;
            int returnID = ReadRecallReturnID(specialInstruction);

            // the Initialize return process committed UserID to rma_ReturnDevices if the user was assigned to a device
            // The committing UserID to rma_ReturnDevices record only happens in Recall Process.
            rma_ReturnDevice returnDevice = (from rd in adc.rma_ReturnDevices
                                             where rd.ReturnID == returnID
                                             select rd).FirstOrDefault();
            int userID = (returnDevice.UserID != null) ? Convert.ToInt32(returnDevice.UserID) : 0;

            // Assign user to device.
            if (userID > 0)
            {
                // Getting the body region for the original badge
                int orgBodyRegionID = (from ud in idc.UserDevices
                                       where ud.UserID == userID && ud.DeactivateDate != null
                                       orderby ud.DeactivateDate descending
                                       select ud.BodyRegionID).FirstOrDefault();


                // Getting a new S/N to associate with a user who had a recalled device
                string serialNo = (from a in mdc.FromMAS_SO_InvoiceTierDistributions
                                   join b in mdc.FromMAS_SO_InvoiceHeaders
                                   on a.InvoiceNo equals b.InvoiceNo
                                   where b.FOB == pOrderID.ToString()
                                   orderby a.ID descending
                                   select a.LotSerialNo).FirstOrDefault();
                if (serialNo != null)
                {
                    int DeviceID = (from AD in idc.DeviceInventories
                                    where AD.SerialNo == serialNo
                                    select AD.DeviceID).First();
                    if (DeviceID > 0)
                    {
                        UserDevice uDevice = new UserDevice();
                        uDevice.UserID = userID;
                        uDevice.DeviceID = DeviceID;
                        uDevice.AssignmentDate = DateTime.Now;
                        uDevice.BodyRegionID = orgBodyRegionID;    // By Defect# 39 on Ontime, the body region defaults to ‘torso’ for the replacement badge is no longer applied. The body region will be based upon the original body region.
                        uDevice.PrimaryDevice = true;
                        uDevice.Active = true;
                        idc.UserDevices.InsertOnSubmit(uDevice);
                        idc.SubmitChanges();
                    }
                }

            }
        }
    }

    private int ReadRecallReturnID(string specialInstruction)
    {
        try
        {
            //specialInstruction = "Recall Replacement, Recall# 12268, Ship Overnight / 2nd Day"
            int sharpSignIndex = specialInstruction.IndexOf("#");
            string subStr = specialInstruction.Substring(sharpSignIndex + 1).Trim();
            int commaIndex = subStr.IndexOf(",");
            string returnID = subStr.Substring(0, commaIndex);
            return int.Parse(returnID);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    protected void btn_ExportSelectedPickSheets_Click(object sender, EventArgs e)
    {

        GenerateExportData(false);


    }

    protected void btn_ExportAllPickSheets_Click(object sender, EventArgs e)
    {
        SqlOpenPickSheet.FilterExpression = ViewState["PickingSheetsFilter"] != null ? ViewState["PickingSheetsFilter"].ToString() : null;
        gv_PickSheet.AllowPaging = false;
        gv_PickSheet.DataBind();
        GenerateExportData(true);
        gv_PickSheet.AllowPaging = true;
        gv_PickSheet.DataBind();
    }

    protected void btnOrderSearch_Click(object sender, EventArgs e)
    {
        SqlOpenPickSheet.FilterExpression = null;
        string orderNumber = txtOrderFilterOrderNumber.Text.Trim();
        string account = txtOrderFilterAccount.Text.Trim();
        string filterExpression = String.Empty;
        ViewState["PickingSheetsFilter"] = null;
        ViewState["OrderType"] = null;

        if (!string.IsNullOrEmpty(orderNumber))
        {
            // search order by PLOrderID
            if (int.TryParse(orderNumber, out int tmpOrderNumber))
            {
                SqlOpenPickSheet.FilterExpression = String.Format("OrderId='{0}'", orderNumber);
            }
        }
        else
        {
            // create order type list by selected search filter
            string orderType = null;
            if (!string.IsNullOrEmpty(ddlOrderFilterType.SelectedItem.Text) && ddlOrderFilterType.SelectedItem.Text.ToUpper() != "ALL")
                orderType = ddlOrderFilterType.SelectedItem.Text.ToUpper();

            if (!string.IsNullOrEmpty(orderType))
            {
                filterExpression = String.Format("OrderType='{0}' ", orderType);
                ViewState["OrderType"] = orderType;
            }

            // Order Date search filter values
            DateTime tmpDt;
            DateTime? fromDt = null;
            DateTime? toDt = null;

            if (!string.IsNullOrEmpty(txtOrderFilterDateRangeFrom.Text))
            {
                if (DateTime.TryParse(txtOrderFilterDateRangeFrom.Text, out tmpDt))
                    fromDt = tmpDt;
            }

            if (!string.IsNullOrEmpty(txtOrderFilterDateRangeTo.Text))
            {
                if (DateTime.TryParse(txtOrderFilterDateRangeTo.Text, out tmpDt))
                    toDt = tmpDt;
            }

            if (fromDt.HasValue)
            {
                filterExpression += String.Format("{0} [Create Dt] >= #{1}# ", filterExpression.Trim().Length == 0 ? string.Empty : " and ", fromDt.Value.ToString("MM/dd/yyyy"));
            }

            if (toDt.HasValue)
            {
                filterExpression += String.Format("{0} [Create Dt] <= #{1}# ", filterExpression.Trim().Length == 0 ? string.Empty : " and ", toDt.Value.ToString("MM/dd/yyyy"));
            }

            if (!string.IsNullOrEmpty(account))
            {
                filterExpression += String.Format("{0} [Acct ID] = '{1}' ", filterExpression.Trim().Length == 0 ? string.Empty : " and ", account);                
            }
            
            SqlOpenPickSheet.FilterExpression = filterExpression;
            ViewState["PickingSheetsFilter"] = filterExpression;
        }

        this.BindOrderCountSummary();


    }

    protected void gv_PickSheet_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        SqlOpenPickSheet.FilterExpression = ViewState["PickingSheetsFilter"]!= null ? ViewState["PickingSheetsFilter"].ToString() : null;
        gv_PickSheet.PageIndex = e.NewPageIndex;
        gv_PickSheet.DataBind();
    }

    private void GenerateExportData(bool exportAll = false)
    {
        DataTable exportTable = new DataTable();

        // Create columns for this DataTable.
        DataColumn colOrderNo = new DataColumn("Order #");
        DataColumn colAcctNo = new DataColumn("Account #");

        DataColumn colShipVia = new DataColumn("Ship Via");
        DataColumn colOrdered = new DataColumn("Ordered");
        DataColumn colType = new DataColumn("Type");
        DataColumn colQty = new DataColumn("Qty");

        // Define DataType of the columns.
        colOrderNo.DataType = System.Type.GetType("System.String");
        colAcctNo.DataType = System.Type.GetType("System.String");

        colShipVia.DataType = System.Type.GetType("System.String");
        colOrdered.DataType = System.Type.GetType("System.String");

        colType.DataType = System.Type.GetType("System.String");
        colQty.DataType = System.Type.GetType("System.String");

        // Add All these columns into exportTable object.
        exportTable.Columns.Add(colOrderNo);
        exportTable.Columns.Add(colAcctNo);

        exportTable.Columns.Add(colShipVia);
        exportTable.Columns.Add(colOrdered);
        exportTable.Columns.Add(colType);
        exportTable.Columns.Add(colQty);

        for (int i = 0; i < this.gv_PickSheet.Rows.Count; i++)
        {
            GridViewRow gvRow = gv_PickSheet.Rows[i];
            CheckBox findChkBx = (CheckBox)gvRow.FindControl("chkbxSelectPickSheet");
            bool SelectedOrder = findChkBx.Checked;
            Label lblQty = (Label)gvRow.FindControl("lblShipProduct");

            if (exportAll || SelectedOrder == true)
            {
                // Create a new table row.
                DataRow dr = exportTable.NewRow();
                dr["Order #"] = gvRow.Cells[1].Text;
                dr["Account #"] = gvRow.Cells[2].Text;
                dr["Ship Via"] = gvRow.Cells[3].Text;
                dr["Ordered"] = gvRow.Cells[4].Text;
                dr["Type"] = gvRow.Cells[5].Text;
                dr["Qty"] = lblQty.Text;

                // Add the row to the table.
                exportTable.Rows.Add(dr);
            }

        } // End For Loop


        // Build the export table.
        TableExport tableExport = new TableExport(exportTable);

        try
        {
            // Read the CSS template from file.
            tableExport.Stylesheet =
                System.IO.File.ReadAllText(Server.MapPath("~/css/export/grids.css"));
        }
        catch (Exception ex)
        {
            throw ex;
        }

        try
        {
            string exportFilename = string.Format("{0}{1}_{2}", exportAll ? "All" : "Selected", "PickSheets", DateTime.Now.ToShortDateString());
            // Create the export file based on the selected value.
            tableExport.Export(exportFilename, "XLS");

            ExportFile file = tableExport.File;

            // Clear everything out.
            Response.Clear();
            Response.ClearHeaders();

            // Set the response headers.
            Response.ContentType = file.ContentType;
            Response.AddHeader("Content-Disposition", file.ContentDisposition);

            // Write to Excel file.
            if (file.Content.GetType() == typeof(byte[]))
            {
                Response.BinaryWrite((byte[])file.Content);
            }
            else
            {
                Response.Write(file.Content.ToString());
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }

        Response.Flush();
        Response.End();
    }

    private void BindOrderCountSummary()
    {
        try
        {
            DataView dv = (DataView)SqlOpenPickSheet.Select(DataSourceSelectArguments.Empty);
            dv.RowFilter = SqlOpenPickSheet.FilterExpression;

            bool fiteredByOrderType = dv.RowFilter.Contains("OrderType");
            string rowFilter = dv.RowFilter;
            List<OrderCountSummary> orderCountSummaries = new List<OrderCountSummary>();
            DataTable dt = new DataTable();
            List<ProductGroupSummary> pgs = new List<ProductGroupSummary>();

            dt = dv.ToTable();
            pgs = (from row in dt.AsEnumerable()
                   group row by new { OrderType = row.Field<string>("OrderType"), ProductGroupName = row.Field<string>("ProductGroupName") } into products
                   select new ProductGroupSummary()
                   {
                       OrderType = products.Key.OrderType,
                       ProductGroupName = products.Key.ProductGroupName,
                       Quantity = products.Sum(x => x.Field<int?>("Quantity"))
                   }).ToList();

        
            // assign ddl order type to a list
            foreach (ListItem item in ddlOrderFilterType.Items)
            {
                if (!String.IsNullOrEmpty(item.Value))
                {
                    string orderTypeFilter = fiteredByOrderType ? string.Empty : String.Format("OrderType = '{0}'", item.Text);
                    int rowCount = 0;
                    int id1Count = 0;
                    int idPlusCount = 0;
                    int id2Count = 0;
                    int usbCount = 0;
                    int instalinkCount = 0;
                    int id3Count = 0;
                    int vueBetaCount = 0;


                    if (String.IsNullOrEmpty(orderTypeFilter))
                    {
                        if (ViewState["OrderType"] != null && ViewState["OrderType"].ToString() != item.Text.ToUpper())
                        {
                            rowCount = 0;
                        }
                        else
                        {
                            rowCount = dv.Count;
                        }
                    }
                    else
                    {
                        string additionalFilter = String.Format("{0} {1}", (!String.IsNullOrEmpty(rowFilter) ? rowFilter + " and" : string.Empty), orderTypeFilter);
                        dv.RowFilter = additionalFilter;
                        rowCount = dv.Count;

                        dt = dv.ToTable();
                        pgs = (from row in dt.AsEnumerable()
                                     group row by new { OrderType = row.Field<string>("OrderType"), ProductGroupName = row.Field<string>("ProductGroupName") } into products
                                     select new ProductGroupSummary()
                                     {
                                         OrderType = products.Key.OrderType,
                                         ProductGroupName = products.Key.ProductGroupName,
                                         Quantity = products.Sum(x => x.Field<int?>("Quantity"))
                                     }).ToList();

                    }

                    // dt.Rows.Add(item.Text, rowCount, result.Where(x=> x.Name == "ID1").Sum, result.Where(x => x.Name == "ID+").Sum, result.Where(x => x.Name == "ID1").Sum
                    id1Count = pgs.FirstOrDefault(x => x.ProductGroupName == "ID1" && x.OrderType == item.Text.ToUpper()) != null ? pgs.FirstOrDefault(x => x.ProductGroupName == "ID1" && x.OrderType == item.Text.ToUpper()).Quantity.Value : 0;
                    idPlusCount = pgs.FirstOrDefault(x => x.ProductGroupName == "ID+" && x.OrderType == item.Text.ToUpper()) != null ? pgs.FirstOrDefault(x => x.ProductGroupName == "ID+" && x.OrderType == item.Text.ToUpper()).Quantity.Value : 0;
                    id2Count = pgs.FirstOrDefault(x => x.ProductGroupName == "ID2" && x.OrderType == item.Text.ToUpper()) != null ? pgs.FirstOrDefault(x => x.ProductGroupName == "ID2" && x.OrderType == item.Text.ToUpper()).Quantity.Value : 0;
                    usbCount = pgs.FirstOrDefault(x => x.ProductGroupName == "InstaLink USB" && x.OrderType == item.Text.ToUpper()) != null ? pgs.FirstOrDefault(x => x.ProductGroupName == "InstaLink USB" && x.OrderType == item.Text.ToUpper()).Quantity.Value : 0;
                    instalinkCount = pgs.FirstOrDefault(x => x.ProductGroupName == "InstaLink" && x.OrderType == item.Text.ToUpper()) != null ? pgs.FirstOrDefault(x => x.ProductGroupName == "InstaLink" && x.OrderType == item.Text.ToUpper()).Quantity.Value : 0;
                    id3Count = pgs.FirstOrDefault(x => x.ProductGroupName == "ID3" && x.OrderType == item.Text.ToUpper()) != null ? pgs.FirstOrDefault(x => x.ProductGroupName == "ID3" && x.OrderType == item.Text.ToUpper()).Quantity.Value : 0;
                    vueBetaCount = pgs.FirstOrDefault(x => x.ProductGroupName == "Vue-B" && x.OrderType == item.Text.ToUpper()) != null ? pgs.FirstOrDefault(x => x.ProductGroupName == "Vue-B" && x.OrderType == item.Text.ToUpper()).Quantity.Value : 0;
                    orderCountSummaries.Add(new OrderCountSummary(item.Value, item.Text, rowCount, id1Count, idPlusCount, id2Count, usbCount, instalinkCount, id3Count, vueBetaCount));

                }
            }


            rptOrderSummaries.DataSource = orderCountSummaries;
            rptOrderSummaries.DataBind();
        }
        catch (Exception ex)
        {
            string msg = ex.Message;
        }
    }

    /// <summary>
    /// Class for badge count summary on the page
    /// </summary>
    private sealed class OrderCountSummary
    {
        public string OrderTypeValue { get; set; }
        public string OrderTypeName { get; set; }
        public int OrderCount { get; set; }
        public int IDCount { get; set; }
        public int IDPlusCount { get; set; }
        public int ID2Count { get; set; }
        public int InstaLinkUSBCount { get; set; }
        public int InstaLinkCount { get; set; }

        public int ID3Count { get; set; }
        public int VueBetaCount { get; set; }
        
        public OrderCountSummary(string orderTypeValue, string orderTypeName, int orderTypeCount, int idBadgeCount, int idPlusBadgeCount, int id2BadgeCount, int instaLinkUSBCount, int instaLinkCount, int id3Count, int vueBetaCount)
        {
            OrderTypeValue = orderTypeValue;
            OrderTypeName = orderTypeName;
            OrderCount = orderTypeCount;
            IDCount = idBadgeCount;
            IDPlusCount = idPlusBadgeCount;
            ID2Count = id2BadgeCount;
            InstaLinkUSBCount = instaLinkUSBCount;
            InstaLinkCount = instaLinkCount;
            ID3Count = id3Count;
            VueBetaCount = vueBetaCount;
        }
    }

    private sealed class ProductGroupSummary
    {
        public string OrderType { get; set; }
        public string ProductGroupName { get; set; }
        public int? Quantity { get; set; }
    }
}
