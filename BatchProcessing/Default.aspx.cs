using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Instadose;
using Instadose.Data;
using Instadose.Integration;

public partial class InstaDose_Admin_BatchProcessing_Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        // if the page is loaded for the first time, load batches into list.
        if (!IsPostBack)
        {

            // Create MAS LINQ object
            MASDataContext MASData = new MASDataContext();

            // Query MAS table with LINQ
            var InvoiceHeaders =
                (from ih in
                    MASData.FromMAS_SO_InvoiceHeaders
                select new
                {
                    BatchNo = ih.BatchNo,
                    Text = ih.BatchNo,
                    
                }).Distinct().OrderByDescending(r => r.Text);
            
            // Set the data source and bind to listbox
            this.lstBatches.DataSource = InvoiceHeaders.ToList();
            this.lstBatches.DataTextField = "Text";
            this.lstBatches.DataValueField = "BatchNo";
            this.lstBatches.DataBind();
        }
    }
    protected void cmdViewDetails_Click(object sender, EventArgs e)
    {
        String BatchNo = this.lstBatches.SelectedValue;
        Response.Redirect("Review.aspx?BatchNo=" + BatchNo);
    }
}
