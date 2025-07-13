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

public partial class InstaDose_Finance_Renewal_ReviewLog : System.Web.UI.Page
{

    // Create the database LINQ objects.
     InsDataContext idc = new InsDataContext();

     // Build list of error messages
     List<string> ErrorMessageList = new List<string>();
    
    // Build list of OrderID, RenewalNo
    //private Dictionary<int, string> AcctsRenewalNos;
    //private Dictionary<int, int> AcctsOrderIDs;
  
    //private RenewalLogs list;
    private List<AccountRenewalReviewListItem> ItemList;
    private string currCode;
    private decimal total = 0;

    protected void Page_Load(object sender, EventArgs e)
    {
        // Prevent the page from redrawing the table if not a first time load.
        if (!IsPostBack)
        {
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
      
        // Ensure the command coming in is the renew command
        //if (e.CommandName != "Renew") return;

        // Convert the argument to the renewal Log ID
       int renewalLogID = int.Parse(e.CommandArgument.ToString());

       RenewalLogs renew = (from rl in idc.RenewalLogs
                            where rl.RenewalLogID == renewalLogID
                            select rl).FirstOrDefault();
       if (renew != null)
       {
           renew.Hide = true;
       }
        
       idc.SubmitChanges();

       //Refresh grid.
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
        ddlBrand.DataSource = (from idcBillingMethod in idc.BrandSources
                                            select new
                                            {
                                                DESC = idcBillingMethod.BrandSourceID,
                                                ID = idcBillingMethod.BrandName
                                            }).ToList();
        ddlBrand.DataTextField = "ID";
        ddlBrand.DataValueField = "DESC";
        ddlBrand.DataBind();
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
        ddlRenewalLog.DataTextField = "DESC";
        ddlRenewalLog.DataValueField = "DESC";
        ddlRenewalLog.DataBind();
        ddlRenewalLog.Items.Insert(0, new ListItem("ALL", ""));
    }

    /// <summary>
    /// Query the data source and bind the data grid.
    /// </summary>
    //private void bindRenewalGrid()
    //{
    //    // Create the sql connection from the connection string in the web.config
    //    SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
    //        ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

    //    // Create the sql command.
    //    SqlCommand sqlCmd = new SqlCommand();
    //    //sqlCmd.Connection = sqlConn;
    //    sqlCmd.CommandType = CommandType.StoredProcedure;  //.Text;
    //    sqlCmd.CommandText = "sp_RenewalGenerationReview";

    //    //string filters = "";

    //    // Set the filter parameter for billing method
    //    if (ddlBillingMethod.SelectedItem.Text != "ALL")
    //    {
    //        //filters += "([BillingMethod] = @billingMethod)";
    //        sqlCmd.Parameters.Add(new SqlParameter("@billingMethod", ddlBillingMethod.SelectedItem.Text));
    //    }

    //    // Set the filter parameter for billing term
    //    if (ddlBillingTerm.SelectedItem.Text != "ALL")
    //    {
    //        // Append the AND if needed.
    //        //if (filters != "") filters += " AND ";

    //        //filters += "([BillingTermDesc] = @billingTerm)";
    //        sqlCmd.Parameters.Add(new SqlParameter("@billingTerm", ddlBillingTerm.SelectedItem.Text));
    //    }

    //    // Set the filter parameter for the brand
    //     if (ddlBrand.SelectedItem.Text != "ALL")
    //    {
    //        // Append the AND if needed.
    //        //if (filters != "") filters += " AND ";

    //        //filters += "([BrandName] = @brandName)";
    //        sqlCmd.Parameters.Add(new SqlParameter("@brandName", ddlBrand.SelectedItem.Text));
    //    }

    //     // Set the filter parameter for the renewal no.
    //     if (ddlRenewalLog.SelectedItem.Text != "ALL")
    //     {
    //         // Append the AND if needed.
    //         //if (filters != "") filters += " AND ";

    //         //filters += "([BrandName] = @brandName)";
    //         sqlCmd.Parameters.Add(new SqlParameter("@renewalNo", ddlRenewalLog.SelectedItem.Text));
    //     }
       
    //    // Append the AND if needed.
        
    //    // Add the filters to the query if needed.
    //    //if (filters != "") query += " WHERE " + filters;

    //    // Add the order by and the direction
    //    //query += " ORDER BY " + 
    //    //gvUpcomingRenewals.CurrentSortedColumn + 
    //    //((gvUpcomingRenewals.CurrentSortDirection == SortDirection.Ascending) ? " ASC" : " DESC");

    //    // Pass the query string to the command
        
    //    sqlCmd.Connection = sqlConn;

    //    bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
    //    if (!openConn) sqlCmd.Connection.Open();

    //    //sqlCmd.CommandText = query;
    //    sqlCmd.CommandText = "sp_RenewalGenerationReview";

    //    SqlDataReader reader = sqlCmd.ExecuteReader();

    //     DataTable dtReviewOrder = new DataTable();

    //    // Create the review orders datatable to hold the contents of the order.
    //    dtReviewOrder = new DataTable("Review Table");

    //    // Create the columns for the review orders datatable.
    //    dtReviewOrder.Columns.Add("RenewalLogID", Type.GetType("System.Int32"));
    //    dtReviewOrder.Columns.Add("OrderNo", Type.GetType("System.Int32"));
    //    dtReviewOrder.Columns.Add("AccountNo", Type.GetType("System.Int32"));
    //    dtReviewOrder.Columns.Add("AccountName", Type.GetType("System.String"));
    //    dtReviewOrder.Columns.Add("RenewalNo", Type.GetType("System.String"));
    //    dtReviewOrder.Columns.Add("OrderStatus", Type.GetType("System.String"));
    //    //dtReviewOrder.Columns.Add("ProcessDate", Type.GetType("System.DateTime"));
    //    dtReviewOrder.Columns.Add("BrandName", Type.GetType("System.String"));
    //    dtReviewOrder.Columns.Add("BillingTermDesc", Type.GetType("System.String"));
    //    dtReviewOrder.Columns.Add("PaymentType", Type.GetType("System.String"));
    //    dtReviewOrder.Columns.Add("RenewalAmount", Type.GetType("System.String"));
    //    //dtReviewOrder.Columns.Add("ErrorMessage", Type.GetType("System.String"));

    //    decimal total = 0;
    //    string currCode = null;

    //    while (reader.Read())
    //    {
  
    //        // Create a new review order row.
    //        DataRow dr = dtReviewOrder.NewRow();

    //        // Fill row details.
    //        dr["RenewalLogID"] = reader["RenewalLogID"];

    //        //If error in Renewal Generation - DO NOT RELEASE!
    //        if (reader["OrderID"].ToString() == null |
    //            reader["OrderID"].ToString() == "")
    //        {
    //            dr["OrderNo"] = DBNull.Value;

    //            //Display Error Message as "Status"
    //            dr["OrderStatus"] = "FAILED - " + reader["ErrorMessage"].ToString();
    //        }
    //        else
    //        {
    //            dr["OrderNo"] = int.Parse(reader["OrderID"].ToString());
    //            dr["OrderStatus"] = reader["OrderStatusName"].ToString();
    //        }

    //        //Is there OrderID?  Not null?
    //        if (reader["OrderID"] != DBNull.Value)
    //        {
    //            //get currency code of Order ID.
    //            currCode = (from o in idc.Orders
    //                        where o.OrderID == Convert.ToInt32(reader["OrderID"])
    //                        select o.CurrencyCode).FirstOrDefault();
    //        }
    //        else //if null get currency code from AccountID.
    //        {
    //            //get currency code of Order ID.
    //            currCode = (from a in idc.Accounts
    //                        where a.AccountID == Convert.ToInt32(reader["AccountID"])
    //                        select a.CurrencyCode).FirstOrDefault();
    //        }

    //        dr["AccountNo"] = reader["AccountID"].ToString();
    //        dr["RenewalNo"] = reader["RenewalNo"].ToString();
    //        dr["AccountName"] = reader["AccountName"].ToString();

    //        //if invalid Total from database, skip!
    //        if (!Decimal.TryParse(reader["RenewalAmount"].ToString(), out total)) continue;

    //        dr["RenewalAmount"] = Currencies.CurrencyToString(total, currCode);

    //        //dr["RenewalAmount"] = Decimal.Parse(reader["RenewalAmount"].ToString());
    //        //dr["RenewalAmount"] = Decimal.Parse(reader["RenewalAmount"].ToString()).ToString("C");

    //        //OrderDate = reader["OrderDate"].ToString(),
    //        dr["BillingTermDesc"] = reader["BillingTermDesc"].ToString();
    //        dr["PaymentType"] = reader["PaymentMethodName"].ToString(); 
        
    //         //dr["BrandSourceID"] = int.Parse(reader["BrandSourceID"].ToString());
    //        dr["BrandName"] = reader["BrandName"].ToString();

    //        // Add row to the data table.
    //        dtReviewOrder.Rows.Add(dr); 
    //    }
 
    //     // Bind the results to the gridview
    //     gvReviewOrders.DataSource = dtReviewOrder;
    //     gvReviewOrders.DataBind();    
    //}

    /// <summary>
    /// Bind Renewal Grid.  Store in List for Excel export option.
    /// </summary>
    private void bindRenewalGrid()
    {
        // Create the sql connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the sql command.
        SqlCommand sqlCmd = new SqlCommand();
        //sqlCmd.Connection = sqlConn;
        sqlCmd.CommandType = CommandType.StoredProcedure; 
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
        sqlCmd.CommandText = "sp_RenewalGenerationReview";

        SqlDataReader reader = sqlCmd.ExecuteReader();

        //Load Renewal Review data to List for grid.
        AccountRenewalReviewList RenewalReviewList = new AccountRenewalReviewList();
        RenewalReviewList.RenewalReview = new List<AccountRenewalReviewListItem>();
        AccountRenewalReviewListItem RenewalItem = new AccountRenewalReviewListItem();

        string ordStatus = null;

        while (reader.Read())
        {
  
            //If error in Renewal Generation - DO NOT RELEASE!
            if (reader["OrderID"].ToString() == null |
                reader["OrderID"].ToString() == "")
            {
                //Display Error Message as "Status"
                //RenewalItem.OrderStatus = "FAILED - " + reader["ErrorMessage"].ToString();

                ordStatus = "FAILED - " + reader["ErrorMessage"].ToString();
            }
            else
            {
                ordStatus = reader["OrderStatusName"].ToString();
            }

            //Is there OrderID?  Not null?
            if (reader["OrderID"] != DBNull.Value)
            {
                //get currency code of Order ID.
                currCode = (from o in idc.Orders
                            where o.OrderID == Convert.ToInt32(reader["OrderID"])
                            select o.CurrencyCode).FirstOrDefault();
            }
            else //if null get currency code from AccountID.
            {
                //get currency code of Order ID.
                currCode = (from a in idc.Accounts
                            where a.AccountID == Convert.ToInt32(reader["AccountID"])
                            select a.CurrencyCode).FirstOrDefault();
            }
   
            //if invalid Total from database, skip!
            if (!Decimal.TryParse(reader["RenewalAmount"].ToString(), out total)) continue;
               
            RenewalItem = new AccountRenewalReviewListItem()
            {           
                // Create a new review order row.
                // Fill row details.
                RenewalLogID = int.Parse(reader["RenewalLogID"].ToString()),
                OrderID = reader["OrderID"].ToString(),
                OrderStatus = ordStatus,

                AccountID = int.Parse(reader["AccountID"].ToString()),
                RenewalNo = reader["RenewalNo"].ToString(),
                AccountName = reader["AccountName"].ToString(),

                RenewalAmount = Currencies.CurrencyToString(total, currCode),

                BillingTermDesc = reader["BillingTermDesc"].ToString(),
                PaymentType = reader["PaymentMethodName"].ToString(),
                BrandName = reader["BrandName"].ToString(),
            };

            RenewalReviewList.RenewalReview.Add(RenewalItem);
        }

        //Store in List for Excel Export option.
        ItemList = RenewalReviewList.RenewalReview;
        
        // Bind the results to the gridview
        gvReviewOrders.DataSource = RenewalReviewList.RenewalReview;
        gvReviewOrders.DataBind();
    }
         
    public override void VerifyRenderingInServerForm(Control control)
    {
        return;
    }

    /// <summary>
    /// Export to Excel - button method.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnExport_Click(object sender, EventArgs e)
    {

        // Bind the grid, to fill the Renewals list for Excel export.
        bindRenewalGrid();
        //// Create an data table used to export
        DataTable exportTable = new DataTable();

        // Create the columns for the review orders export data table.
       
        exportTable.Columns.Add("Order#", Type.GetType("System.String"));
        exportTable.Columns.Add("Account#", Type.GetType("System.Int32"));
        exportTable.Columns.Add("Account", Type.GetType("System.String"));
        exportTable.Columns.Add("Renewal#", Type.GetType("System.String"));
        exportTable.Columns.Add("Status", Type.GetType("System.String"));
        exportTable.Columns.Add("Brand", Type.GetType("System.String"));
        exportTable.Columns.Add("Term", Type.GetType("System.String"));
        exportTable.Columns.Add("Type", Type.GetType("System.String"));
        exportTable.Columns.Add("Amount", Type.GetType("System.String"));
        
        //// Add the rows from the account renewals list
        foreach (AccountRenewalReviewListItem rl in ItemList)
        {
            // Create a new table row
            DataRow dr = exportTable.NewRow();
            dr["Account#"] = rl.AccountID;
            dr["Account"] = rl.AccountName;

            if (rl.OrderStatus != null)
            {
                dr["Order#"] = rl.OrderID.ToString();
            }
            else
            {
                dr["Order#"] = "";
            }

            dr["Renewal#"] = rl.RenewalNo;
            dr["Status"] = rl.OrderStatus;
            dr["Brand"] = rl.BrandName;
            dr["Term"] = rl.BillingTermDesc; 
            dr["Type"] = rl.PaymentType;
            dr["Amount"] = rl.RenewalAmount;

            // Add the row to the table
            exportTable.Rows.Add(dr);
        }

        // Build the export table
        TableExport tableExport = new TableExport(exportTable);

        try
        {
            // Read the CSS template from file
            tableExport.Stylesheet =
                System.IO.File.ReadAllText(Server.MapPath("~/css/export/grids.css"));
        }
        catch { }
        
        // Create the export file based on the selected value
        tableExport.Export("RenewalReview", "XLS");

        ExportFile file = tableExport.File;

        // Clear everything out
        Response.Clear();
        Response.ClearHeaders();

        // Set the response headers
        Response.ContentType = file.ContentType;
        Response.AddHeader("Content-Disposition", file.ContentDisposition);

        //Write to Excel file.
        if (file.Content.GetType() == typeof(byte[]))
            Response.BinaryWrite((byte[])file.Content);
        else
            Response.Write(file.Content.ToString());

        Response.Flush();
        Response.End();
    }

    protected void gvReviewOrders_SelectedIndexChanged(object sender, EventArgs e)
    {

    }
    
}

