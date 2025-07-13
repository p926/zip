using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Instadose.EDI;
using Instadose.EDI.Entity;
using System.Web.Script.Serialization;

public partial class CustomerService_EDIPurchaseOrders : System.Web.UI.Page
{
    private EDIHelper.EDIPageTabs CurrentTab;

    #region General Page Methods

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.QueryString["action"] != null)
        {
            performAction(Request.QueryString["action"]);
            return;
        }

        Enum.TryParse(hfActiveGrid.Value, out CurrentTab);

        if (IsPostBack) return;

        // Get the current tab
        Enum.TryParse(Request.QueryString["tab"], true, out CurrentTab);
        
        changeTabs();
    }

    protected void btnChangeTab_Click(object sender, EventArgs e)
    {
        Button btnChangeTab = (Button)sender;
        Enum.TryParse(btnChangeTab.CommandArgument, out CurrentTab);
        changeTabs();
    }

    private void changeTabs()
    {
        hfActiveGrid.Value = CurrentTab.ToString();

        //pnlInvoices.Visible = false;
        //pnlPurchaseOrders.Visible = false;
        //pnlAcknowledgements.Visible = false;

        switch (CurrentTab)
        {
            case EDIHelper.EDIPageTabs.PO:
                //btnChangeTabPO.CssClass = "btn active";
                pnlPurchaseOrders.Visible = true;
                bindPurchaseOrders();
                break;
            case EDIHelper.EDIPageTabs.POAck:
                //btnChangeTabPOAck.CssClass = "btn active";
                pnlAcknowledgements.Visible = true;
                break;
        }
    }

    private void performAction(string action)
    {
        var js = new JavaScriptSerializer();
        var db = new EDIDataContext();

        string json = "";
        try
        {
            int id = 0;
            if (!int.TryParse(Request.QueryString["id"], out id))
                throw new Exception("ID was not supplied.");

            // this action is called via ajax when user 
            // performs a review.
            if (action == "po-save")
            {
                var po = (from p in db.PurchaseOrders
                          where p.PurchaseOrderID == id
                          select p).FirstOrDefault();

                if (po == null)
                    throw new Exception("The purchase order could not be found.");
                if(Request.QueryString["reviewStatus"] == null)
                    throw new Exception("Review status was not supplied.");
                if(Request.QueryString["account"] == null)
                    throw new Exception("Account was not supplied.");
                if(Request.QueryString["poRequestNumber"] == null)
                    throw new Exception("Purchase Order request number was not supplied.");
                if(Request.QueryString["notes"] == null)
                    throw new Exception("Notes were not supplied.");

                int status = 0;
                int.TryParse(Request.QueryString["reviewStatus"], out status);

                int applicationID = 0;
                int.TryParse(Request.QueryString["applicationID"], out applicationID);

                // id, status, account, porequest#, notes
                // reviewed date, reviewed by
                po.ApplicationID = applicationID;
                po.Account = Request.QueryString["account"];
                po.PONumber = Request.QueryString["poRequestNumber"];
                po.Notes = Request.QueryString["notes"];
                po.ReviewedBy = ActiveDirectoryQueries.GetUserName();
                po.ReviewedDate = DateTime.Now;
                po.ReviewStatusID = status;

                // save the review status
                db.SubmitChanges();
                json = "0";
            }
            else if (action == "po-get")
            {
                var po = (from p in db.PurchaseOrders
                          where p.PurchaseOrderID == id
                          select new
                          {
                              id = p.PurchaseOrderID,
                              p.ApplicationID,
                              p.Account,
                              p.AddlCustomerName1,
                              p.AddlCustomerName2,
                              p.Address1,
                              p.Address2,
                              p.City,
                              p.CustomerIDCode,
                              p.CustomerName,
                              p.PODescription,
                              p.PONumber,
                              PORequestNumber = string.IsNullOrEmpty(p.PORequestNumber) ? p.AddlCustomerName1 : p.PORequestNumber,
                              p.PORequestDate,
                              p.StateProvinceCode,
                              p.PostalCode,
                              p.POTypeCode,
                              details = p.PurchaseOrderDetails.Select(d => new
                              {
                                  d.HSIProductCode,
                                  d.ItemDescription,
                                  d.PODetailLineNum,
                                  d.UnitMeasurement,
                                  d.UnitPrice,
                                  d.Quantity,
                                  Product = new
                                  {
                                      d.VendorProductCode,
                                      d.ProductSKUs.SKU,
                                      d.ProductSKUs.SKUDescription,
                                      d.ProductSKUs.Application.ApplicationName
                                  },
                              }).ToList(),
                              p.SummaryHashTotal,
                              p.SummaryLineItems,
                              p.Notes,
                              p.HasIssues,
                              p.ReviewStatusID
                          }).FirstOrDefault();

                if(po == null)
                    throw new Exception("The purchase order could not be found.");

                // serialize the po
                json = js.Serialize(po);
            }

        }
        catch (Exception ex)
        {
            json = js.Serialize(new
            {
                error = ex.Message
            });
        }
        finally
        {
            // Clear the response and print the json.
            Response.Clear();
            Response.ContentType = "application/json; charset=utf-8";
            Response.Write(json);
            Response.End();
        }
    }

    #endregion

    #region Purchase Order Controls
    private void bindPurchaseOrders()
    {
        int filterApplicationID = 0;
        int filterStatus = int.Parse(hfPOFilterStatus.Value);


        // Reset the button states
        btnPOTypeGDS.CssClass = "btn";
        btnPOTypeInstadose.CssClass = "btn";

        // set the selected button state
        switch (hfPOType.Value)
        {
            case "GDS":
                btnPOTypeGDS.CssClass = "btn active";
                filterApplicationID = 1;
                break;
            case "Instadose":
                btnPOTypeInstadose.CssClass = "btn active";
                filterApplicationID = 2;
                break;
        }

        // Reset the button states
        btnPOFilterApproved.CssClass = "btn btn-success";
        btnPOFilterErrors.CssClass = "btn btn-danger";
        btnPOFilterNonReviewed.CssClass = "btn";
        btnPOFilterRejected.CssClass = "btn btn-info";

        // set the selected button state
        switch (filterStatus)
        {
            case 1: // non-reviewed
                btnPOFilterNonReviewed.CssClass = "btn active";
                break;
            case 2: // approved
                btnPOFilterApproved.CssClass = "btn btn-success active";
                break;
            case 3: // rejected
                btnPOFilterRejected.CssClass = "btn btn-info active";
                break;
            case 4: // error
                btnPOFilterErrors.CssClass = "btn btn-danger active";
                break;
        }

        var db = new EDIDataContext();
        var purchaseOrders = (from p in db.PurchaseOrders
                              where p.ReviewStatusID == filterStatus && p.ApplicationID == filterApplicationID
                              select p).AsQueryable();
        
        // filter the invoices using the text.
        if (txtPOSearch.Text != "")
        {
            var s = txtPOSearch.Text;
            // If a date is provided...
            DateTime searchDate = DateTime.MinValue;
            if (DateTime.TryParse(s, out searchDate))
                purchaseOrders = purchaseOrders.Where(p => p.PORequestDate == searchDate);
            else
            {
                // Search on any fields...
                purchaseOrders = purchaseOrders.Where( p =>
                        p.PORequestNumber == s ||
                        p.PONumber == s ||
                        p.PODescription.Contains(s) ||
                        p.CustomerName.Contains(s) ||
                        p.Notes.Contains(s));
            }
        }

        gvPurchaseOrders.DataSource = purchaseOrders;
        gvPurchaseOrders.DataBind();
    }

    protected void btnPOFilter_Click(object sender, EventArgs e)
    {
        Button btnPOFilter = (Button)sender;
        hfPOFilterStatus.Value = btnPOFilter.CommandArgument;

        bindPurchaseOrders();
    }

    protected void btnPOSearch_Click(object sender, EventArgs e)
    {
        bindPurchaseOrders();
    }

    protected void btnPOClear_Click(object sender, EventArgs e)
    {
        txtPOSearch.Text = "";
        bindPurchaseOrders();
    }

    protected void gvPurchaseOrders_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {

    }

    protected void gvPurchaseOrders_Sorting(object sender, GridViewSortEventArgs e)
    {
    }

    protected void btnPOType_Click(object sender, EventArgs e)
    {
        Button btnPOType = (Button)sender;
        hfPOType.Value = btnPOType.CommandArgument;

        bindPurchaseOrders();
    }

    #endregion

    #region Invoice Controls
    
    protected void btnInvoiceFilter_Click(object sender, EventArgs e)
    {
        Button btnInvoiceFilter = (Button)sender;
        hfInvoiceFilterStatus.Value = btnInvoiceFilter.CommandArgument;
        
        bindInvoices();
    }

    protected void btnInvoiceSearch_Click(object sender, EventArgs e)
    {
        bindInvoices();
    }

    protected void btnInvoiceClear_Click(object sender, EventArgs e)
    {
        txtInvoiceSearch.Text = "";
        bindInvoices();
    }

    protected void btnInvoiceType_Click(object sender, EventArgs e)
    {
        Button btnInvoiceType = (Button)sender;
        hfInvoiceType.Value = btnInvoiceType.CommandArgument;

        bindInvoices();
    }


    protected void btnInvoiceReview_Click(object sender, EventArgs e)
    {
        // Get the review status from the button that was pressed.
        Button btnInvoiceReview = (Button)sender;
        int reviewStatus = int.Parse(btnInvoiceReview.CommandArgument);
        string userName = User.Identity.Name.Split('\\')[1];

        // Get a list of the invoices to approve
        var invoiceIDs = new List<int>();
        foreach (GridViewRow row in gvInvoices.Rows)
        {
            CheckBox cbStatus = (CheckBox)row.Cells[0].FindControl("cbStatus");
            if (cbStatus.Checked)
            {
                HiddenField hfInvoiceID = (HiddenField)row.Cells[0].FindControl("hfInvoiceID");
                invoiceIDs.Add(int.Parse(hfInvoiceID.Value));
            }
        }

        // Approve/reject each of the items.
        var db = new EDIDataContext();
        var invoices = (from i in db.Invoices where invoiceIDs.Contains(i.InvoiceID) select i);
        foreach (var invoice in invoices)
        {
            //invoice.ReviewedDate = DateTime.Now;
            //invoice.ReviewedBy = userName;
            //invoice.ReviewStatusID = reviewStatus;
        }
        // Save the changes.
        db.SubmitChanges();

        // Rebind
        bindInvoices();
    }

    protected void gvInvoices_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvInvoices.PageIndex = e.NewPageIndex;
        bindInvoices();
    }

    private void bindInvoices()
    {
        int filterApplicationID = 0;
        int filterStatus = int.Parse(hfInvoiceFilterStatus.Value);

        // Reset the button states
        btnInvoiceTypeGDS.CssClass = "btn";
        btnInvoiceTypeInstadose.CssClass = "btn";

        // set the selected button state
        switch (hfInvoiceType.Value)
        {
            case "GDS":
                btnInvoiceTypeGDS.CssClass = "btn active";
                filterApplicationID = 1;
                break;
            case "Instadose":
                btnInvoiceTypeInstadose.CssClass = "btn active";
                filterApplicationID = 2;
                break;
        }

        // Reset the button states
        btnInvoiceFilterApproved.CssClass = "btn btn-success";
        btnInvoiceFilterErrors.CssClass = "btn btn-danger";
        btnInvoiceFilterNonReviewed.CssClass = "btn";
        btnInvoiceFilterRejected.CssClass = "btn btn-info";
        gvInvoices.Columns[0].Visible = false; // checkbox
        gvInvoices.Columns[gvInvoices.Columns.Count - 1].Visible = false;  // error
        gvInvoices.Columns[gvInvoices.Columns.Count - 2].Visible = false;  // reviewed by
        gvInvoices.Columns[gvInvoices.Columns.Count - 3].Visible = false;  // reviewed date
        btnInvoiceReviewApprove.Visible = false;
        btnInvoiceReviewReject.Visible = false;

        // set the selected button state
        switch (filterStatus)
        {
            case 1: // non-reviewed
                btnInvoiceFilterNonReviewed.CssClass = "btn active";
                gvInvoices.Columns[0].Visible = true;
                btnInvoiceReviewApprove.Visible = true;
                btnInvoiceReviewReject.Visible = true;
                break;
            case 2: // approved
                btnInvoiceFilterApproved.CssClass = "btn btn-success active";
                gvInvoices.Columns[gvInvoices.Columns.Count - 2].Visible = true;
                gvInvoices.Columns[gvInvoices.Columns.Count - 3].Visible = true;
                break;
            case 3: // rejected
                btnInvoiceFilterRejected.CssClass = "btn btn-info active";
                gvInvoices.Columns[gvInvoices.Columns.Count - 2].Visible = true;
                gvInvoices.Columns[gvInvoices.Columns.Count - 3].Visible = true;
                break;
            case 4: // error
                btnInvoiceFilterErrors.CssClass = "btn btn-danger active";
                gvInvoices.Columns[gvInvoices.Columns.Count - 1].Visible = true;
                break;
        }

        var db = new EDIDataContext();
        var invoices = (from i in db.PurchaseOrders
                        where i.ReviewStatusID == filterStatus && i.ApplicationID == filterApplicationID
                        select i);

        // filter the invoices using the text.
        if (txtInvoiceSearch.Text != "")
        {
            var s = txtInvoiceSearch.Text;
            // If a date is provided...
            DateTime searchDate = DateTime.MinValue;
            if (DateTime.TryParse(s, out searchDate))
                invoices = invoices.Where(i => i.PORequestDate == searchDate);
            else
            {
                // Search on any fields...
                invoices = invoices.Where(
                        i => i.PONumber == s ||
                        i.PurchaseOrderID.ToString() == s ||
                        i.CustomerName.Contains(s));
            }

        }

        gvInvoices.DataSource = invoices;
        gvInvoices.DataBind();
    }

    protected void gvInvoices_Sorting(object sender, GridViewSortEventArgs e)
    {
        bindInvoices();

    }

    #endregion

    #region Validation
    /// <summary>
    /// Loop through POs to see if they can be processed
    /// </summary>
    protected void ValidatePOs()
    {
        var db = new EDIDataContext();
        List<PurchaseOrder> purchaseOrders = (from p in db.PurchaseOrders
                              where p.HasIssues
                              select p).ToList();


        foreach (PurchaseOrder po in purchaseOrders)
        {
            string errors = "";
            
            if(!string.IsNullOrEmpty(po.PORequestNumber))
            {
                //look up the POR in UNIX
                Mirion.DSD.GDS.API.ICCareRequests ir = new Mirion.DSD.GDS.API.ICCareRequests();
                string acc = null;
                if(!string.IsNullOrEmpty(po.Account))
                    acc = po.Account;

                var pors = ir.GetICCarePORequests(acc,po.PORequestNumber);

                if(acc == null)
                    po.Account = pors.FirstOrDefault().Account;

                if(pors.Count == 0)  //Error - POR not found
                    errors += "POR not found.";

                //look up POR in Instadose
                Instadose.Data.InsDataContext idc = new Instadose.Data.InsDataContext();
                var iPORs = from p in idc.POReceipts
                            where p.PONumber == po.PORequestNumber
                            select p;

            }
            else
            {
            
            }
        }
    }
    #endregion

    protected void btnValidatePO_Click(object sender, EventArgs e)
    {

    }
}