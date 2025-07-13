using System;
using System.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

using Instadose;
using Instadose.Data;
using Instadose.Processing;

public partial class InstaDose_Finance_Renewal_RenewalRelease : System.Web.UI.Page
{
    // Create the database LINQ objects.
     InsDataContext idc = new InsDataContext();

     // Build list of error messages
     List<string> ErrorMessageList = new List<string>();
    
    // Build list of OrderID, RenewalNo
     private string userName = "Unknown";



     //Add Processing message to Release Orders button and disable during
     //processing.
     protected override void OnInit(EventArgs e)
     {
         this.btnReleaseOrders.Attributes.Add("onclick", "javascript:" +
                       btnReleaseOrders.ClientID +
                       ".disabled=true;this.value = 'Processing...';" +
                       this.GetPostBackEventReference(btnReleaseOrders));
         InitializeComponent();
         base.OnInit(e);
     }

     //    /// <summary>
     //    /// Required method for Designer support - do not modify
     //    /// the contents of this method with the code editor.
     //    /// </summary>
     private void InitializeComponent()
     {
         this.btnReleaseOrders.Click +=
                 new System.EventHandler(this.btnReleaseOrders_Click);
         this.Load += new System.EventHandler(this.Page_Load);
     }
    

    protected void Page_Load(object sender, EventArgs e)
    {
        // Try to set the username
        try
        {
            userName = User.Identity.Name.Split('\\')[1];
            //this.lblusername.Text = "User: " + this.UserName;
        }
        catch { userName = "Unknown"; }

        // Prevent the page from redrawing the table if not a first time load.
        if (!IsPostBack)
        {
            // Bind a javascript event to show the button is processing.
            this.btnReleaseOrders.Attributes.Add("onclick",
                "this.disabled=true;this.value = 'Processing...';" + this.GetPostBackEventReference(btnReleaseOrders));

            bindBillingTermsCombo();
            bindBillingMethodCombo();
            bindBrandNameCombo();
            bindRenewalNoCombo();

            // Bind the grid
            bindRenewalGrid();  
        }

    }

    /// <summary>
    /// Fired when the page has been changed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvReviewOrders_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvReviewOrders.PageIndex = e.NewPageIndex;
        bindRenewalGrid();
    }
       
    /// <summary>
    /// Hides the review and completed panels and displays the renewals table.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnBack_Click(object sender, EventArgs e)
    {
       pnlReview.Visible = false;

       //Show grid for orders to be released.
       pnlRelease.Visible = true;

       // Force the screen to redraw.
        bindRenewalGrid();
    }

    /// <summary>
    /// Called from many objects, force binds the gridview.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gridviewBinder(object sender, EventArgs e)
    {
        // Reset the page to 0
        gvReviewOrders.PageIndex = 0;

        // Bind the gridview
        bindRenewalGrid();

        //turn off delete messages.
        InvisibleMsg();
    }

    /// <summary>
    /// When the column is being sorted.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvReviewOrders_Sorting(object sender, GridViewSortEventArgs e)
    {
        gvReviewOrders.PageIndex = 0;
        bindRenewalGrid();
               
    }

    private void bindBillingMethodCombo()
    {

        this.ddlBillingMethod.DataSource = (from idcBillingMethod in idc.PaymentMethods
                                           select new { DESC = idcBillingMethod.PaymentMethodID,
                                                        ID = idcBillingMethod.PaymentMethodName}).ToList();

        this.ddlBillingMethod.DataTextField = "ID";
        this.ddlBillingMethod.DataValueField = "DESC";

        this.ddlBillingMethod.DataBind();
        ddlBillingMethod.Items.Insert(0, new ListItem("ALL", "")); 
    }

    private void bindBillingTermsCombo()
    {
        ddlBillingTerm.DataSource = (from idcBillingMethod in idc.BillingTerms
                                            select new
                                            {
                                                DESC = idcBillingMethod.BillingTermID,
                                                ID = idcBillingMethod.BillingTermDesc
                                            }).ToList();

        ddlBillingTerm.DataTextField = "ID";
        ddlBillingTerm.DataValueField = "DESC";

        ddlBillingTerm.DataBind();
        ddlBillingTerm.Items.Insert(0, new ListItem("ALL", "")); 
    }

    private void bindBrandNameCombo()
    {

        this.ddlBrand.DataSource = (from idcBillingMethod in idc.BrandSources
                                            select new
                                            {
                                                DESC = idcBillingMethod.BrandSourceID,
                                                ID = idcBillingMethod.BrandName
                                            }).ToList();
        this.ddlBrand.DataTextField = "ID";
        this.ddlBrand.DataValueField = "DESC";
        this.ddlBrand.DataBind();
        ddlBrand.Items.Insert(0, new ListItem("ALL", "")); 
    }

    private void bindRenewalNoCombo()
    {

        this.ddlRenewalLog.DataSource = (from rl in idc.RenewalLogs
                                    select new
                                    {
                                        DESC = rl.RenewalNo
                                        //ID = rl.RenewalNo
                                    }).Distinct().ToList();
        this.ddlRenewalLog.DataTextField = "DESC";
        this.ddlRenewalLog.DataValueField = "DESC";
        this.ddlRenewalLog.DataBind();
        ddlRenewalLog.Items.Insert(0, new ListItem("ALL", ""));
    }

    /// <summary>
    /// Query the data source and bind the data grid.
    /// </summary>
    private void bindRenewalGrid()
    {
        // Create the sql connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the sql command.
        SqlCommand sqlCmd = new SqlCommand();
        //sqlCmd.Connection = sqlConn;
        sqlCmd.CommandType = CommandType.StoredProcedure;  //.Text;
        sqlCmd.CommandText = "sp_ReleaseRenewalReview";

        //string filters = "";

        // Set the filter parameter for billing method
        if (ddlBillingMethod.SelectedValue != "")
        {
            //filters += "([BillingMethod] = @billingMethod)";
            sqlCmd.Parameters.Add(new SqlParameter("@billingMethod", ddlBillingMethod.SelectedItem.Text));
        }

        // Set the filter parameter for billing term
        if (ddlBillingTerm.SelectedValue != "")
        {
            // Append the AND if needed.
            //if (filters != "") filters += " AND ";

            //filters += "([BillingTermDesc] = @billingTerm)";
            sqlCmd.Parameters.Add(new SqlParameter("@billingTerm", ddlBillingTerm.SelectedItem.Text));
        }

        // Set the filter parameter for the brand
         if (ddlBrand.SelectedValue != "")
        {
            // Append the AND if needed.
            //if (filters != "") filters += " AND ";

            //filters += "([BrandName] = @brandName)";
            sqlCmd.Parameters.Add(new SqlParameter("@brandName", ddlBrand.SelectedItem.Text));
        }

         // Set the filter parameter for the renewal no.
         if (ddlRenewalLog.SelectedValue != "")
         {
             // Append the AND if needed.
             //if (filters != "") filters += " AND ";

             //filters += "([BrandName] = @brandName)";
             sqlCmd.Parameters.Add(new SqlParameter("@renewalNo", ddlRenewalLog.SelectedItem.Text));
         }
       
        // Append the AND if needed.
        
        // Add the filters to the query if needed.
        //if (filters != "") query += " WHERE " + filters;

        // Add the order by and the direction
        //query += " ORDER BY " + 
        //gvUpcomingRenewals.CurrentSortedColumn + 
        //((gvUpcomingRenewals.CurrentSortDirection == SortDirection.Ascending) ? " ASC" : " DESC");

        // Pass the query string to the command
        
        sqlCmd.Connection = sqlConn;

        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();

        //sqlCmd.CommandText = query;
        sqlCmd.CommandText = "sp_ReleaseRenewalReview";

        SqlDataReader reader = sqlCmd.ExecuteReader();

        //AccountRenewalList RenewalList = new AccountRenewalList();
        //RenewalList.Renewals = new List<AccountRenewalListItem>();

        DataTable dtReviewOrder = new DataTable();

        // Create the review orders datatable to hold the contents of the order.
        dtReviewOrder = new DataTable("Review Table");

        // Create the columns for the review orders datatable.
        dtReviewOrder.Columns.Add("Process", Type.GetType("System.Boolean"));
        dtReviewOrder.Columns.Add("OrderNo", Type.GetType("System.Int32"));
        dtReviewOrder.Columns.Add("AccountNo", Type.GetType("System.Int32"));
        dtReviewOrder.Columns.Add("AccountName", Type.GetType("System.String"));
        dtReviewOrder.Columns.Add("LocationID", Type.GetType("System.Int32"));
        dtReviewOrder.Columns.Add("LocationName", Type.GetType("System.String"));
        dtReviewOrder.Columns.Add("RenewalNo", Type.GetType("System.String"));
        dtReviewOrder.Columns.Add("OrderStatus", Type.GetType("System.String"));
        
        //dtReviewOrder.Columns.Add("ProcessDate", Type.GetType("System.DateTime"));
        
        dtReviewOrder.Columns.Add("BrandName", Type.GetType("System.String"));
        dtReviewOrder.Columns.Add("BillingTermDesc", Type.GetType("System.String"));
        dtReviewOrder.Columns.Add("PaymentType", Type.GetType("System.String"));
        dtReviewOrder.Columns.Add("RenewalAmount", Type.GetType("System.Decimal"));
        
        //dtReviewOrder.Columns.Add("ErrorMessage", Type.GetType("System.String"));

        decimal total = 0;
 
        while (reader.Read())
        {
  
            // Create a new review order row.
            DataRow dr = dtReviewOrder.NewRow();

                // Fill row details.
                dr["Process"] = false;

                //If error in Renewal Generation - DO NOT RELEASE!
                if (reader["OrderID"].ToString() == null |
                    reader["OrderID"].ToString() == "")
                {
                    dr["Process"] = false;
                    dr["OrderNo"] = DBNull.Value;

                    //Display Error Message as "Status"
                    dr["OrderStatus"] = "FAILED - " + reader["ErrorMessage"].ToString();
                }
                else
                {
                    dr["OrderNo"] = int.Parse(reader["OrderID"].ToString());
                    dr["OrderStatus"] = reader["OrderStatusName"].ToString();  
                }

                dr["AccountNo"] = reader["AccountID"].ToString();
                dr["RenewalNo"] = reader["RenewalNo"].ToString();
                dr["AccountName"] = reader["AccountName"].ToString();

                dr["LocationID"] = reader["LocationID"].ToString();
                dr["LocationName"] = reader["LocationName"].ToString();
                
                dr["RenewalAmount"] = Decimal.Parse(reader["RenewalAmount"].ToString());
                //dr["RenewalAmount"] = Decimal.Parse(reader["RenewalAmount"].ToString()).ToString("C");

                //OrderDate = reader["OrderDate"].ToString(),
                 dr["BillingTermDesc"] = reader["BillingTermDesc"].ToString();
                 dr["PaymentType"] = reader["PaymentMethodName"].ToString(); 
                 //dr["BrandSourceID"] = int.Parse(reader["BrandSourceID"].ToString());
                 dr["BrandName"] = reader["BrandName"].ToString();
            
                // Add row to the data table.
                dtReviewOrder.Rows.Add(dr);
           }

         // Bind the results to the gridview
         gvReviewOrders.DataSource = dtReviewOrder;
         gvReviewOrders.DataBind();

         //format currency amount.  USD or foreign orders.
         formatGridAmount("ReviewOrders");
    }

    /// <summary>
    /// formatRenewalAmount - format currency amount.  USD or foreign orders.
    /// </summary>
    private void formatGridAmount(string gvName)
    {
        decimal decimalAmount;
        int orderID;
        HiddenField hidOrdID;

        //Which grid are you format the curreny amounts?
        if (gvName == "ReviewOrders")
        {
            foreach (GridViewRow gvRow in gvReviewOrders.Rows)
            {
                //CheckBox chkbox1 = (CheckBox)myRow.FindControl("cbRow");
                hidOrdID = (HiddenField)gvRow.FindControl("HidORDID");

                orderID = Convert.ToInt32(hidOrdID.Value);

                string currCode = (from o in idc.Orders
                                   where o.OrderID == orderID
                                   select o.CurrencyCode).FirstOrDefault();

                System.Web.UI.WebControls.Label lblAmt = (System.Web.UI.WebControls.Label)gvRow.FindControl("lblAmount");

                decimalAmount = Convert.ToDecimal(Currencies.RemoveCurrencySymbol(lblAmt.Text, "USD", 50));

                lblAmt.Text = Currencies.CurrencyToString(decimalAmount, currCode);
            }
        }
        else  //Review results grid.
        {
            foreach (GridViewRow gvRow in gvReview.Rows)
            {
                //CheckBox chkbox1 = (CheckBox)myRow.FindControl("cbRow");
                hidOrdID = (HiddenField)gvRow.FindControl("HidORDID");

                orderID = Convert.ToInt32(hidOrdID.Value);

                string currCode = (from o in idc.Orders
                                   where o.OrderID == orderID
                                   select o.CurrencyCode).FirstOrDefault();

                System.Web.UI.WebControls.Label lblAmt = (System.Web.UI.WebControls.Label)gvRow.FindControl("lblAmount");

                decimalAmount = Convert.ToDecimal(Currencies.RemoveCurrencySymbol(lblAmt.Text, "USD", 50));

                lblAmt.Text = Currencies.CurrencyToString(decimalAmount, currCode);
            }
        }
    }
       
    /// <summary>
    /// Release selected orders - IC Care and/or non-ICCare and process.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnReleaseOrders_Click(object sender, EventArgs e)
    {
        List<int> ordersToRelease = new List<int>();

        string orderProcessed = "Renewal Orders Released: ";
        

        for (int i = 0; i < gvReviewOrders.Rows.Count; i++)
        {
            GridViewRow gvRow = gvReviewOrders.Rows[i];
            CheckBox findChkBx = (CheckBox)gvRow.FindControl("cbRow");
            bool selectedOrder = findChkBx.Checked;
            findChkBx.Checked = false;

            // If the order was selected then continue...
            if (selectedOrder == true)
            {
                // Grabbing the orderID hidden field
                HiddenField hfOrderID = (HiddenField)gvRow.FindControl("HidORDID");
                int orderID = 0; // Store the order ID here.
                int.TryParse(hfOrderID.Value, out orderID); // Pass the order ID into the orderID int.

                // Skip the record if no order was supplied or found.
                if (orderID <= 0) continue;

                // Add the order to the Order to release list.
                ordersToRelease.Add(orderID);
            }
        }

        //Are there order IDs left to process?
        if (ordersToRelease.Count == 0) return;

        // Construct the renewal object.
        Renewals renewal = new Renewals();
        renewal.UserName = userName;

        // release each order...
        var orderStats = new List<RenewalStatus>();
        foreach (int orderID in ordersToRelease)
        {
            // Release the order and return the results into a list.
            orderStats.Add(renewal.ReleaseRenewalHold(orderID));
        }

        // Save message
        Basics.WriteLogEntry(orderProcessed, userName, 
            "Finance_Renewal_RenewalRelease_releaseOrders_click", Basics.MessageLogType.Information);

        // Rebind the review grid.
        bindRenewalReviewGrid(orderStats);
        bindErrorMessages();
    }

    /// <summary>
    /// bindRenewalReviewGrid
    /// </summary>
    private void bindRenewalReviewGrid(List<RenewalStatus> statuses)
    {
        idc = new InsDataContext();
        // Build list of orders that were processed.
        var logs = (from r in idc.RenewalLogs
                       where
                            statuses.Select(s => s.OrderID).Contains(r.OrderID.Value)
                       select new
                       {
                           r.AccountID,
                           r.Account.AccountName,
                           r.OrderID,
                           OrderStatusName = (r.Order.OrderStatusID == 4) ? "Released" : "Failed",
                           r.OrderStatusID,
                           r.RenewalAmount,
                           r.BrandSource.BrandName,
                           r.Account.BillingTerm.BillingTermDesc,
                           r.PaymentMethod.PaymentMethodName
                       }).ToList();

        // Join the logs on the status list returned from the renewals.
        var output = (from l in logs
                      join s in statuses on l.OrderID equals s.OrderID
                      select new
                      {
                          l.AccountID,
                          l.AccountName,
                          l.BillingTermDesc,
                          l.BrandName,
                          l.OrderID,
                          l.OrderStatusName,
                          l.PaymentMethodName,
                          l.RenewalAmount,
                          s.ErrorMessage
                      });

        gvReview.DataSource = output;
        gvReview.DataBind();

        pnlRelease.Visible = false;
        pnlReview.Visible = true;

        //format currency amount.  USD or foreign orders.
        formatGridAmount("Review");

    }

    /// <summary>
    /// Delete Order - used only if order has NOT been shipped, fullfilled, etc.!
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void imgBtnDelete_Click(object sender, ImageClickEventArgs e)
    {
        ImageButton btn = (ImageButton)sender;
        string btnCommandName = btn.CommandName.ToString();

        int orderID = 0;
        if (!int.TryParse(btn.CommandArgument.ToString(), out orderID)) return;

        var ordStatus = (from o in idc.Orders
                         where o.OrderID == orderID
                         select new { o }).FirstOrDefault();

        //If order has been processed - fullfilled; shipped, etc.  CANNOT DELETE!
        if (ordStatus.o.OrderStatusID > 2  && ordStatus.o.OrderStatusID != 8) return;

        //Delete order in question since it has not been shipped/fullfilled, etc.
        OrderHelpers.DeleteOrder(orderID);

        bindRenewalGrid();

    }
    
    public void bindErrorMessages()
    {
        blstErrors.DataSource = ErrorMessageList;
        blstErrors.DataBind();

        //Display only errors are present.
        if (ErrorMessageList.Count() == 0)
        {
            plErrorMessage.Visible = false;
        }
        else
        {
            plErrorMessage.Visible = true;
        }
    }

    /// <summary>
    /// Make Form message invisible.
    /// </summary>
    private void InvisibleMsg()
    {
        this.MsgText.InnerText = "";
        this.FormMsg.Visible = false;
    }

    /// <summary>
    /// Make form message visible.
    /// </summary>
    /// <param name="msg"></param>
    private void VisibleMsg(string msg)
    {
        this.MsgText.InnerText = msg;
        this.FormMsg.Visible = true;
    }

    #region Process Order Functions

       
    private List<int> getOrderIDs()
    {
        try
        {
            // Build list of orders to process
            List<int> ProcessOrderIDs = new List<int>();

            // Loop through the grid to get the order Ids
            foreach (GridViewRow gvr in this.gvReviewOrders.Rows)
            {
                // Find the checkbox
                CheckBox cbRow = (CheckBox)gvr.Cells[0].FindControl("cbRow");

                // Determine if the checkbox was checked.
                if (cbRow.Checked)
                {
                    int orderID = 0;
                    HiddenField hf = (HiddenField)gvr.FindControl("HidORDID");

                    // Convert the cell text (Order#) to an int.
                    if (int.TryParse(hf.Value.ToString(), out orderID))
                        ProcessOrderIDs.Add(orderID);
                }
            }

            return ProcessOrderIDs;
        }
        catch (Exception ex)
        {
            Basics.WriteLogEntry(ex.Message, HttpContext.Current.User.Identity.Name,
                "Finance_Renewal_RenewalRelease_GetOrderIds", Basics.MessageLogType.Critical);

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
                    sb.Append("<div class=\"msg_information\">" + log.MessageDesc + "</div>");
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

}

