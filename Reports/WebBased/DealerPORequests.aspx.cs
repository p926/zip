using Instadose.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Reports_WebBased_DealerPORequests : System.Web.UI.Page
{
    InsDataContext idc = new InsDataContext();
    
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.QueryString["OrderIDs"] == null)
        {
            Response.Write("Report was not provided required data.");
            return;
        }

        List<int> orderIDs = new List<int>();

        var rawOrderIDs = Request.QueryString["OrderIDs"].Split(',');
        foreach(var rawOrderID in rawOrderIDs) 
        {
            int tmpOrderID = 0;
            if (!int.TryParse(rawOrderID, out tmpOrderID))
            {
                Response.Write("One or more of the order IDs was invalid.");
                return;
            }

            // Build the list of order IDs
            orderIDs.Add(tmpOrderID);
        }

        // Query the nessesary data and render it to the page.
        var order = (from o in idc.Orders //join t2 in db.Table2 on t1.field equals t2.field
                     join dealer in idc.Dealers on o.Account.UnixID equals dealer.DealerID.ToString()
                     where orderIDs.Contains(o.OrderID)
                     select new
                     {
                         DealerID = dealer.DealerID,
                         DealerName = dealer.DealerName,
                         DealerAddress1 = dealer.Address1,
                         DealerAddress2 = dealer.Address2,
                         DealerAddress3 = dealer.Address3,
                         DealerCity = dealer.City,
                         DealerState = dealer.State.StateAbbrev,
                         DealerPostalCode = dealer.PostalCode,
                         DealerFax = dealer.Fax,
                         DealerTelephone = dealer.Telephone,
                         DealerNative = dealer.DealerNative,

                         o.AccountID,
                         o.OrderID,
                         o.OrderDate,
                         o.Location.LocationName,
                         o.Location.ShippingCompany,
                         o.Location.ShippingAddress1,
                         o.Location.ShippingAddress2,
                         o.Location.ShippingAddress3,
                         o.Location.ShippingCity,
                         ShippingState = o.Location.ShippingState.StateAbbrev,
                         o.Location.ShippingPostalCode,
                         o.Location.ShippingTelephone,
                         o.Location.ShippingFax,
                         o.PONumber,
                         LastPONumber = (from lo in o.Account.Orders 
                                         where 
                                            lo.OrderID != o.OrderID && !lo.PONumber.Contains("NEED")
                                         orderby lo.OrderID descending
                                         select lo.PONumber).FirstOrDefault(),
                         //TotalQuantity = o.OrderDetails.Sum(od => (int?)od.Quantity),

                         TotalQuantity = ((from od1 in idc.OrderDetails
                                            join p1 in idc.Products on od1.ProductID equals p1.ProductID
                                            where od1.OrderID == o.OrderID
                                            && (p1.ProductSKU == "INSTA10" || p1.ProductSKU == "INSTA10-B" || p1.ProductSKU == "INSTA PLUS")
                                            select od1).Count() >= 1) ? (o.OrderDetails.Sum(od => (int?)od.Quantity) / 2) : (o.OrderDetails.Sum(od => (int?)od.Quantity)),
                         
                         OrderTotal = o.OrderDetails.Sum(od => (int?)od.Quantity * (decimal?)od.Price) + o.ShippingCharge,
                         o.ShippingCharge,
                     });

        rptOrders.DataSource = order;
        rptOrders.DataBind();
    }

    protected void rptOrders_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        // Ensure the item is correct.
        ListItemType[] acceptableItems = new ListItemType[2] { ListItemType.Item, ListItemType.AlternatingItem }; 
        if (!acceptableItems.Contains(e.Item.ItemType)) return;
        
        int orderID = (int)DataBinder.Eval(e.Item.DataItem, "OrderID");
        int dealerID = (int)DataBinder.Eval(e.Item.DataItem, "DealerID");

        var orderDetails = (from od in idc.OrderDetails
                            join p in idc.Products on od.ProductID equals p.ProductID
                            where od.OrderID == orderID
                            && (p.ProductSKU != "INSTA10" && p.ProductSKU != "INSTA10-B" && p.ProductSKU != "INSTA PLUS")
                            select new
                            {
                                DealerSKU = (from dp in od.Product.DealerProducts where dp.DealerID == dealerID select dp.DealerSKU).SingleOrDefault(),
                                Description = od.Product.ProductDesc.Contains("IMI Cover") ? ("Annual Subscription for Instadose (RadBadge) Monitoring - " + p.Color) : (od.Product.ProductDesc.Contains("Instadose Plus Cover") ? "Annual Subscription for Instadose+ Monitoring (Bluetooth) - " + p.Color : od.Product.ProductDesc),
                                WearStartDate = od.Order.StxContractStartDate,
                                WearEndDate = od.Order.StxContractEndDate,
                                SubscriptionPeriod = "12 months",
                                Quantity = string.Format("{0:#,##0}", od.Quantity),

                                //Price = string.Format("${0:0.00}", od.Price),
                                //Total = string.Format("${0:0.00}", (od.Quantity * od.Price)),

                                Price = string.Format("${0:0.00}", (od.Product.ProductDesc.Contains("IMI Cover") || od.Product.ProductDesc.Contains("Instadose Plus Cover")) ? (from od1 in idc.OrderDetails
                                                                                                                                                                                join p1 in idc.Products on od1.ProductID equals p1.ProductID
                                                                                                                                                                                where od1.OrderID == orderID
                                                                                                                                                                                && (p1.ProductSKU == "INSTA10" || p1.ProductSKU == "INSTA10-B" || p1.ProductSKU == "INSTA PLUS")
                                                                                                                                                                                select od1.Price).FirstOrDefault() : od.Price),

                                Total = string.Format("${0:0.00}", (od.Product.ProductDesc.Contains("IMI Cover") || od.Product.ProductDesc.Contains("Instadose Plus Cover")) ? ((from od1 in idc.OrderDetails
                                                                                                                                                                                 join p1 in idc.Products on od1.ProductID equals p1.ProductID
                                                                                                                                                                                 where od1.OrderID == orderID
                                                                                                                                                                                 && (p1.ProductSKU == "INSTA10" || p1.ProductSKU == "INSTA10-B" || p1.ProductSKU == "INSTA PLUS")
                                                                                                                                                                                 select od1.Price).FirstOrDefault() * od.Quantity) : (od.Price * od.Quantity)),

                                od.OrderDetailDate
                            });


        // Get the repeater control
        Repeater rptOrderDetails = (Repeater)e.Item.FindControl("rptOrderDetails");
        rptOrderDetails.DataSource = orderDetails;
        rptOrderDetails.DataBind();
    }
    protected void rptOrderDetails_ItemDataBound(object sender, RepeaterItemEventArgs e)
    {
        if (e.Item.ItemType != ListItemType.Footer) return;
        Repeater rptOrderDetails = (Repeater)sender;
        RepeaterItem repeaterRow = (RepeaterItem)rptOrderDetails.Parent;
        Label lblOrderTotal = (Label)e.Item.FindControl("lblOrderTotal");
        Label lblShippingCharge = (Label)e.Item.FindControl("lblShippingCharge");
        Label lblPONumber = (Label)e.Item.FindControl("lblPONumber");

        if (DataBinder.Eval(repeaterRow.DataItem, "OrderTotal") == null) return;

        decimal orderTotal = (decimal)DataBinder.Eval(repeaterRow.DataItem, "OrderTotal");
        decimal shippingCharge = (decimal)DataBinder.Eval(repeaterRow.DataItem, "ShippingCharge");
        string poNumber = (string)DataBinder.Eval(repeaterRow.DataItem, "PONumber");

        lblOrderTotal.Text = string.Format("${0:#,##0.00}", orderTotal);
        lblShippingCharge.Text = string.Format("${0:#,##0.00}", shippingCharge);

    }
}