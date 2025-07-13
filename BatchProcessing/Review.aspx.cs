using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Instadose;
using Instadose.Data;
using Instadose.Processing;

public partial class InstaDose_Admin_BatchProcessing_Review : System.Web.UI.Page
{
    #region Page Properties

    // The current batch number from the MAS table.
    private String SelectedBatchNo = String.Empty;

    // Create the database LINQ objects.
    MASDataContext mdc = new MASDataContext();
    InsDataContext idc = new InsDataContext();
  
    #endregion

    #region Object Events

    /// <summary>
    /// Generate the tables if a new load page.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void Page_Load(object sender, EventArgs e)
    {
        // Check to ensure there was a post from a previous page.
        if (Request.QueryString["BatchNo"] != null)
        {
            // Retrieve the value of BatchNo
            SelectedBatchNo = Request.QueryString["BatchNo"];

            //Turn off error message.
            this.plErrorMessage.Visible = false;

            if (SelectedBatchNo == String.Empty)
            {
                //Turn on error message.
                this.plErrorMessage.Visible = true;

                // Display the error message no batch found.
                this.lblErrorMessage.InnerText += "Batch number is blank. ";
            }
            else
            {
                //Turn off error message.
                this.plErrorMessage.Visible = false;

                // Display the batch number on the screen.
                this.lblBatchNo.Text = SelectedBatchNo;
            }
        }
        else
        {
            //Turn on error message.
            this.plErrorMessage.Visible = true;

            // Set the error message.
            this.lblErrorMessage.InnerText += "Please start at the batch processing page. ";
        }

        // Prevent the page from redrawing the table if not a first time load.
        if (IsPostBack) return;

        // Clear RunId to ensure a new one will be generated. Used for tracking an order in the MessageLog
        Basics.RunID = null;

        // If a batch number is found find the order and display to the screen.
        if (this.SelectedBatchNo != String.Empty)
        {
            // Build the batches table from MAS data.
            DataTable tblBatches = this.BuildBatchTable(SelectedBatchNo);

            // Ensure the batches table generated properly, if not, end the process.
            if (tblBatches == null)
            {
                // Display a message saying there was an error generating batches table.
                this.lblErrorMessage.InnerText = "Oops, something went wrong while generating the batches table. ";
                this.plErrorMessage.Visible = true;

                // Hide the reviews orders panel, since nothing could be processed.
                this.plReviewOrders.Visible = false;

                // Return to cancel.
                return;
            }

            // Ensure the batches table is filled with rows.
            if (tblBatches.Rows.Count > 0)
            {
                // Bind the totals to the grid.
                this.gvReviewOrders.DataSource = tblBatches;
                this.gvReviewOrders.DataBind();
            }
            else
            {
                // Display a message saying the information couldn't be found.
                this.lblErrorMessage.InnerText = "That's weird, I couldn't find the details for this batch. ";
                this.plErrorMessage.Visible = true;

                // Hide the reviews orders panel, since nothing could be processed.
                this.plReviewOrders.Visible = false;
            }
        }

        // Check to see if an error message is present to display the error label.
        //if (this.lblErrorMessage.InnerText != "Error: ")
        //    this.plErrorMessage.Visible = true;
    }

    /// <summary>
    /// Process the selected orders from the dtReviewOrder.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnProcessOrders_Click(object sender, EventArgs e)
    {
        // Hide the review orders panel.
        this.plReviewOrders.Visible = false;

        // Get the order ids from the grid view that are checked.
        List<int> ProcessOrderIds = this.GetOrderIds();
        if(ProcessOrderIds == null) return;

        // Hold the orderIds of the orders that couldn't process.
        List<int> FailedOrderIds = new List<int>();

        // Get the current username
        String UserName = HttpContext.Current.User.Identity.Name;

        // Set the Batch number selected from the drop down menu.
        String BatchNo = this.lblBatchNo.Text;

        // Build the process orders object that will handle everything.
        ProcessOrders Orders = new ProcessOrders(ProcessOrderIds, BatchNo, UserName);

        // Set payments required flag.
        Orders.PaymentsRequired = this.rbPaymentsRequired.Checked;

        // Process each of the orders.
        Orders.Process();

        // Get the failure data for each order.
        DataTable dtResultsTable = BuildResultsTable(Orders.Results);

        // Get the log details
        if (Basics.RunID.HasValue)
        {
            // Render the messages to the screen.
            this.lblMessageLog.Text = this.BuildMessageLog(Basics.RunID.Value);

            // Show log panel
            this.plLogDetails.Visible = true;
        }

        if (dtResultsTable != null)
        {
            // Set the data source to the results table.
            this.gvResultsTable.DataSource = dtResultsTable;
            this.gvResultsTable.DataBind();

            // Show the panel to display the grid.
            this.plResultsTable.Visible = true;
        }
        else // Error obtaining the failure table.
        {
            // Failed to create the error datatable.
            Basics.WriteLogEntry("Could not create failed orders table.", UserName,
                "InstaDose_Admin_BatchProcessing_Review.btnProcessOrders_Click", Basics.MessageLogType.Critical);

        }
    }

    #endregion

    #region Process Order Functions

    /// <summary>
    /// Build the datatable to display in the grid view.
    /// </summary>
    /// <param name="BatchNo">Batch number from MAS.</param>
    /// <returns>Fill DataTable</returns>
    private DataTable BuildBatchTable(String BatchNo)
    {
        try
        {
            // Create a temp datatable
            DataTable dtReviewOrder = new DataTable();

            // Create the review orders datatable to hold the contents of the order.
            dtReviewOrder = new DataTable("Review Table");

            //********************************************************************
            //11/12/2012 WK - Multi-currency formatting added.
            //********************************************************************
            // Create the columns for the review orders datatable.
            dtReviewOrder.Columns.Add("Process", Type.GetType("System.Boolean"));
            dtReviewOrder.Columns.Add("OrderNo", Type.GetType("System.Int32"));
            dtReviewOrder.Columns.Add("Type", Type.GetType("System.String"));
            dtReviewOrder.Columns.Add("InvoiceNo", Type.GetType("System.Int32"));
            dtReviewOrder.Columns.Add("AccountNo", Type.GetType("System.Int32"));
            dtReviewOrder.Columns.Add("BillToName", Type.GetType("System.String"));
            dtReviewOrder.Columns.Add("Total", Type.GetType("System.String"));
            dtReviewOrder.Columns.Add("Payments", Type.GetType("System.String"));
            dtReviewOrder.Columns.Add("Balance", Type.GetType("System.String"));

            // Load the invoice headers into the list.
            List<FromMAS_SO_InvoiceHeader> InvoiceHeaders = (from b in mdc.FromMAS_SO_InvoiceHeaders
                                                             where b.BatchNo == this.SelectedBatchNo
                                                             select b).ToList();

            // Get a list of the orders in the batch.
            var invoices = (from b in mdc.FromMAS_SO_InvoiceHeaders
                            where b.BatchNo == this.SelectedBatchNo 
                            select b).ToList();
            
            // Loop through the invoice header (orders) to fill line items.
            foreach (var invoice in invoices)
            {
                // Get the order for this invoice...
                var order = idc.Orders.Where(o => o.OrderID == int.Parse(invoice.FOB) && o.OrderStatusID != 10).FirstOrDefault();

                // Skip the order if it doesn't exist.
                if(order == null) continue;

                // Get the first payment.
                var payment = order.Payments.FirstOrDefault();

                // Get order total by summing the order shipping, tax, detail prices and anything else.
                decimal orderTotal = order.OrderDetails.Sum(od => od.Quantity * od.Price) + order.ShippingCharge;

                // Create a new review order row.
                DataRow dr = dtReviewOrder.NewRow();
                dr["Process"] = true;
                dr["OrderNo"] = invoice.FOB;
                dr["Type"] = (payment == null) ? "Unknown" : payment.PaymentMethod.PaymentMethodName;
                dr["InvoiceNo"] = invoice.InvoiceNo;
                dr["AccountNo"] = invoice.CustomerNo;
                dr["BillToName"] = invoice.BillToName;
                dr["Payments"] = Currencies.CurrencyToString(invoice.DepositAmt.Value, order.CurrencyCode);
                dr["Total"] = Currencies.CurrencyToString(orderTotal, order.CurrencyCode);
                dr["Balance"] = Currencies.CurrencyToString(orderTotal - invoice.DepositAmt.Value, order.CurrencyCode); 

                // Add row to the data table.
                dtReviewOrder.Rows.Add(dr);
            }

            return dtReviewOrder;
        }
        catch (Exception ex)
        {
            Basics.WriteLogEntry(ex.Message, HttpContext.Current.User.Identity.Name,
                "InstaDose_Admin_BatchProcessing_Review.BuildBatchTable", Basics.MessageLogType.Critical);

            return null;
        }
    }

    /// <summary>
    /// Build the table that represents the results of the batch.
    /// </summary>
    /// <param name="ProcessResultsList">List results.</param>
    /// <returns>DataTable with the results of a process.</returns>
    private DataTable BuildResultsTable(List<ProcessResults> ProcessResultsList)
    {
        try
        {
            // Create a datatable to store the results in.
            DataTable dtResults = new DataTable();
            dtResults.Columns.Add("OrderNo", Type.GetType("System.Int32"));
            dtResults.Columns.Add("Type", Type.GetType("System.String"));
            dtResults.Columns.Add("Total", Type.GetType("System.Decimal"));
            dtResults.Columns.Add("Payment", Type.GetType("System.String"));
            dtResults.Columns.Add("Tracking", Type.GetType("System.String"));            
            dtResults.Columns.Add("SetRenewalDates", Type.GetType("System.String"));
            dtResults.Columns.Add("Softrax", Type.GetType("System.String"));
            dtResults.Columns.Add("Process", Type.GetType("System.String"));

            // Loop through each result
            // build the datatable with explaination
            foreach (ProcessResults result in ProcessResultsList)
            {
                // Create a new row to add to the table.
                DataRow dr = dtResults.NewRow();

                // Get the order for this invoice...
                var order = idc.Orders.Where(o => o.OrderID == result.OrderId && o.OrderStatusID != 10).FirstOrDefault();

                // Skip the order if it doesn't exist.
                if (order == null) continue;

                // Get order total by summing the order shipping, tax, detail prices and anything else.
                decimal orderTotal = order.OrderDetails.Sum(od => od.Quantity * od.Price) + order.ShippingCharge;

                // Set the rows properties.
                dr["OrderNo"] = result.OrderId;
                dr["Type"] = result.PaymentType;
                dr["Payment"] = result.PaymentCompleted.ToString();
                dr["Total"] = orderTotal;
                dr["Tracking"] = result.UpdatedTrackingNumberCompleted.ToString();                
                dr["SetRenewalDates"] = result.RenewalDateAssignmentCompleted.ToString();
                dr["Softrax"] = result.SoftraxCompleted.ToString();
                dr["Process"] = result.ProcessCompleted.ToString();

                // Add the row to the table.
                dtResults.Rows.Add(dr);
            }

            // Return datatable results.
            return dtResults;
        }
        catch (Exception ex)
        {
            Basics.WriteLogEntry(ex.Message, HttpContext.Current.User.Identity.Name,
                "InstaDose_Admin_BatchProcessing_Review.BuildResultsTable", Basics.MessageLogType.Critical);

            // Return nothing because of the error.
            return null;
        }
    }

    /// <summary>
    /// Get the order ids from grid view and fills a list.
    /// </summary>
    /// <returns>A list of order ids.</returns>
    private List<int> GetOrderIds()
    {
        try
        {
            // Build list of orders to process
            List<int> ProcessOrderIds = new List<int>();

            // Loop through the grid to get the order Ids
            foreach (GridViewRow gvr in this.gvReviewOrders.Rows)
            {
                // Find the checkbox
                CheckBox cbRow = (CheckBox)gvr.Cells[0].FindControl("cbRow");

                // Determine if the checkbox was checked.
                if (cbRow.Checked)
                {
                    int OrderId = 0;
                    HyperLink hl = (HyperLink)gvr.Cells[1].FindControl("hlOrderLink");

                    // Convert the cell text (Order#) to an int.
                    if (int.TryParse(hl.Text, out OrderId))
                        ProcessOrderIds.Add(OrderId);
                }
            }

            return ProcessOrderIds;
        }
        catch (Exception ex)
        {
            Basics.WriteLogEntry(ex.Message, HttpContext.Current.User.Identity.Name,
                "InstaDose_Admin_BatchProcessing_Review.GetOrderIds", Basics.MessageLogType.Critical);

            return null;
        }
    }

    /// <summary>
    /// Generate a message log that the process created.
    /// </summary>
    /// <param name="RunId">Guid from the Basics.RunID</param>
    /// <returns>String of messages</returns>
    private String BuildMessageLog(Guid RunId)
    {

        // Create the header.
        System.Text.StringBuilder sb =
            new System.Text.StringBuilder("<div class=\"msg_title\">----- Run ID: " + RunId + " -----</div>");

        InsDataContext idc = new InsDataContext();

        // Gather the message log results
        List<MessageLog> msgLog =
            (from ml in idc.MessageLogs
             where ml.RunID == RunId
             select ml).ToList();

        // Loop through each message and append them to the string
        foreach (MessageLog log in msgLog)
        {
            // To highlight the messages differently use a switch
            switch (log.LogType)
            {
                case "Information":
                    sb.Append("<div class=\"msg_information\">" + log.MessageDesc  + "</div>");
                    break;
                case "Notice":
                    sb.Append("<div class=\"msg_notice\">" + log.MessageDesc + "</div>");
                    break;
                case "Critical":
                    sb.Append("<div class=\"msg_critical\">" + log.MessageDesc + "</div>");
                    break;
                default:
                    sb.Append("<div class=\"msg\">" + log.MessageDesc + "</div>");
                    break;
            }
        }

        sb.Append("<div class=\"msg_title\">----- Log End -----</div>");

        // return message log to the messagelog label.
        return sb.ToString();
    }

    #endregion

    protected void rbPaymentsRequired_CheckedChanged(object sender, EventArgs e)
    {

    }
    protected void rbPaymentsNotRequired_CheckedChanged(object sender, EventArgs e)
    {

    }
}
