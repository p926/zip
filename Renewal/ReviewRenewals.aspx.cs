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
using System.Text;

public partial class InstaDose_Admin_Renewal_ReviewRenewals : System.Web.UI.Page
{

    // Create the database LINQ objects.
    InsDataContext idc = new InsDataContext();
    
    protected void Page_Load(object sender, EventArgs e)
    {
        // Prevent the page from redrawing the table if not a first time load.
        if (IsPostBack) return;

        bindBillingTermsCombo();
        bindBillingMethodCombo();
        bindBrandNameCombo();
        bindRenewalNoCombo();

        // Bind the grid
        bindRenewalGrid();
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


    /// <summary>
    /// Hide Renewal Record from grid view command
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvReviewOrders_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        // Convert the argument to the renewal Log ID
        int renewalLogID = int.Parse(e.CommandArgument.ToString());

        RenewalLogs renew = (from rl in idc.RenewalLogs where rl.RenewalLogID == renewalLogID
                             select rl).FirstOrDefault();
        if (renew != null)
            renew.Hide = true;

        idc.SubmitChanges();

        //Refresh grid.
        bindRenewalGrid();
    }

    private void bindBillingMethodCombo()
    {

        this.ddlBillingMethod.DataSource = (from idcBillingMethod in idc.PaymentMethods
                                            select new
                                            {
                                                DESC = idcBillingMethod.PaymentMethodID,
                                                ID = idcBillingMethod.PaymentMethodName
                                            }).ToList();

        this.ddlBillingMethod.DataTextField = "ID";
        this.ddlBillingMethod.DataValueField = "DESC";

        this.ddlBillingMethod.DataBind();
        ddlBillingMethod.Items.Insert(0, new ListItem("ALL", ""));
    }

    private void bindBillingTermsCombo()
    {

        this.ddlBillingTerm.DataSource = (from idcBillingMethod in idc.BillingTerms
                                          select new
                                          {
                                              DESC = idcBillingMethod.BillingTermID,
                                              ID = idcBillingMethod.BillingTermDesc
                                          }).ToList();

        this.ddlBillingTerm.DataTextField = "ID";
        this.ddlBillingTerm.DataValueField = "DESC";

        this.ddlBillingTerm.DataBind();
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
        sqlCmd.CommandText = "sp_RenewalGenerationReview";

        //string filters = "";

        // Set the filter parameter for billing method
        if (ddlBillingMethod.SelectedItem.Text != "ALL")
        {
            //filters += "([BillingMethod] = @billingMethod)";
            sqlCmd.Parameters.Add(new SqlParameter("@billingMethod", ddlBillingMethod.SelectedItem.Text));
        }

        // Set the filter parameter for billing term
        if (ddlBillingTerm.SelectedItem.Text != "ALL")
        {
            // Append the AND if needed.
            //if (filters != "") filters += " AND ";

            //filters += "([BillingTermDesc] = @billingTerm)";
            sqlCmd.Parameters.Add(new SqlParameter("@billingTerm", ddlBillingTerm.SelectedItem.Text));
        }

        // Set the filter parameter for the brand
        if (ddlBrand.SelectedItem.Text != "ALL")
        {
            // Append the AND if needed.
            //if (filters != "") filters += " AND ";

            //filters += "([BrandName] = @brandName)";
            sqlCmd.Parameters.Add(new SqlParameter("@brandName", ddlBrand.SelectedItem.Text));
        }

        // Set the filter parameter for the renewal no.
        if (ddlRenewalLog.SelectedItem.Text != "ALL")
        {
            // Append the AND if needed.
            //if (filters != "") filters += " AND ";

            //filters += "([BrandName] = @brandName)";
            sqlCmd.Parameters.Add(new SqlParameter("@renewalNo", ddlRenewalLog.SelectedItem.Text));
        }

        sqlCmd.Connection = sqlConn;

        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();

        //sqlCmd.CommandText = query;
        sqlCmd.CommandText = "sp_RenewalGenerationReview";

        SqlDataReader reader = sqlCmd.ExecuteReader();

        DataTable dtReviewOrder = new DataTable();

        // Create the review orders datatable to hold the contents of the order.
        dtReviewOrder = new DataTable("Review Table");

        // Create the columns for the review orders datatable.
        dtReviewOrder.Columns.Add("RenewalLogID", Type.GetType("System.Int32"));
        dtReviewOrder.Columns.Add("OrderNo", Type.GetType("System.Int32"));
        dtReviewOrder.Columns.Add("AccountNo", Type.GetType("System.Int32"));
        dtReviewOrder.Columns.Add("AccountName", Type.GetType("System.String"));
        dtReviewOrder.Columns.Add("RenewalNo", Type.GetType("System.String"));
        dtReviewOrder.Columns.Add("OrderStatus", Type.GetType("System.String"));
        //dtReviewOrder.Columns.Add("ProcessDate", Type.GetType("System.DateTime"));
        dtReviewOrder.Columns.Add("BrandName", Type.GetType("System.String"));
        dtReviewOrder.Columns.Add("BillingTermDesc", Type.GetType("System.String"));
        dtReviewOrder.Columns.Add("PaymentType", Type.GetType("System.String"));
        dtReviewOrder.Columns.Add("RenewalAmount", Type.GetType("System.Decimal"));
        //dtReviewOrder.Columns.Add("ErrorMessage", Type.GetType("System.String"));

        while (reader.Read())
        {

            // Create a new review order row.
            DataRow dr = dtReviewOrder.NewRow();

            // Fill row details.
            dr["RenewalLogID"] = reader["RenewalLogID"];

            //If error in Renewal Generation - DO NOT RELEASE!
            if (reader["OrderID"].ToString() == null |
                reader["OrderID"].ToString() == "")
            {
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
    }


    /// <summary>
    /// Release selected renewal hold(s) and process.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnReleaseOrders_Click(object sender, EventArgs e)
    {
        this.plErrorMessage.Visible = false;
        
        // Construct the renewal class with the user's name.
        Renewals renewal = new Renewals()
        {
            UserName = ActiveDirectoryQueries.GetUserName()
        };

        var statuses = new List<RenewalStatus>();
        // Loop through a list of selected Orders
        foreach (var orderID in getOrderIDs())
        {
            // Perform the Release of a renewal on hold.
            var status = renewal.ReleaseRenewalHold(orderID);
            
            //@todo anything special for IC CARE?!

        }

        BindRenewalReviewGrid();
    }

    private void BindRenewalReviewGrid()
    {
        // Build list of orders that were processed.
        List<int> OrderIDList = new List<int>();

        OrderIDList = getOrderIDs();

        var results = (from rl in idc.RenewalLogs
                       join acc in idc.Accounts on rl.AccountID equals acc.AccountID
                       join os in idc.OrderStatus on rl.OrderStatusID equals os.OrderStatusID
                       join bs in idc.BrandSources on rl.BrandSourceID equals bs.BrandSourceID
                       join bt in idc.BillingTerms on rl.BillingTermID equals bt.BillingTermID
                       join pm in idc.PaymentMethods on rl.PaymentMethodID equals pm.PaymentMethodID
                       where OrderIDList.Contains((int)rl.OrderID)
                       select new
                       {
                           rl.AccountID,
                           acc.AccountName,
                           rl.OrderID,
                           OrderStatusName = rl.OrderStatusID == 3 ? "ORDER RELEASED" : "RELEASE FAILED",
                           //rl.DateProcessed,
                           rl.RenewalAmount,
                           bs.BrandName,
                           bt.BillingTermDesc,
                           pm.PaymentMethodName
                       });

        gvReview.DataSource = results;
        gvReview.DataBind();

        pnlRelease.Visible = false;
        pnlReview.Visible = true;

    }


    #region Process Order Functions


    private List<int> getOrderIDs()
    {
        // Build list of orders to process
        var orderIDs = new List<int>();

        // Loop through the grid to get the order Ids
        foreach (GridViewRow row in this.gvReviewOrders.Rows)
        {
            // Find the checkbox and hidden field.
            var cbRow = (CheckBox)row.Cells[0].FindControl("cbRow");
            var hfOrderID = (HiddenField)row.Cells[1].FindControl("hfOrderID");

            // Determine if the checkbox was checked.
            if (!cbRow.Checked) continue;

            // Get the order ID from the hidden control.
            orderIDs.Add(int.Parse(hfOrderID.Value));
        }

        return orderIDs;
    }

    /// <summary>
    /// Generate a message log that the process created.
    /// </summary>
    /// <param name="RunId">Guid from the Basics.RunID</param>
    /// <returns>String of messages</returns>
    private string buildMessageLog(Guid RunId)
    {
        // Create the header.
        StringBuilder sb = new StringBuilder("<div class=\"msg_title\">----- Run ID: " + RunId + " -----</div>");

        // Gather the message log results
        var msgLog = (from ml in idc.MessageLogs  where ml.RunID == RunId select ml).ToList();

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

