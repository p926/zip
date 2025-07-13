using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Web.Script.Serialization;
using Instadose.EDI;

public partial class Finance_EDIInvoices : System.Web.UI.Page
{
    #region General Page Methods

    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.QueryString["action"] != null)
        {
            performAction(Request.QueryString["action"]);
            return;
        }

        if (IsPostBack) return;
        bindInvoices();
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
        bool? transferred = null;
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
        gvInvoices.Columns[0].Visible = false; // checkbox
        gvInvoices.Columns[gvInvoices.Columns.Count - 1].Visible = false;  // error
        gvInvoices.Columns[gvInvoices.Columns.Count - 2].Visible = false;  // reviewed by
        gvInvoices.Columns[gvInvoices.Columns.Count - 3].Visible = false;  // reviewed date
        btnInvoiceReviewApprove.Visible = false;
        btnInvoiceReviewReject.Visible = false;

        // set the selected button state
        switch (filterStatus)
        {
            case 1: // transferred
                btnInvoiceFilterApproved.CssClass = "btn btn-success active";
                gvInvoices.Columns[0].Visible = true;
                btnInvoiceReviewApprove.Visible = true;
                btnInvoiceReviewReject.Visible = true;
                transferred = true;
                break;
            case 0: // failed
                btnInvoiceFilterErrors.CssClass = "btn btn-danger active";
                gvInvoices.Columns[gvInvoices.Columns.Count - 1].Visible = true;
                transferred = false;
                break;
            default: //not sent
                break;

        }

        var db = new EDIDataContext();
        var invoices = (from i in db.Invoices
                        where i.TransferSuccess == transferred && i.ApplicationID == filterApplicationID
                        select i);

        // filter the invoices using the text.
        if (txtInvoiceSearch.Text != "")
        {
            var s = txtInvoiceSearch.Text;
            // If a date is provided...
            DateTime searchDate = DateTime.MinValue;
            if (DateTime.TryParse(s, out searchDate))
            {
                invoices = invoices.Where(i => i.InvoiceDate == searchDate);
            }
            else
            {
                // Search on any fields...
                invoices = invoices.Where(
                        i => i.InvoiceNumber == s ||
                        i.PONumber == s ||
                        i.OrderID.ToString() == s ||
                        i.BillingCompanyName.Contains(s) ||
                        i.ShippingCompanyName.Contains(s));
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
}