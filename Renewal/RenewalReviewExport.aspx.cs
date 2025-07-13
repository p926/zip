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

public partial class InstaDose_Finance_Renewal_RenewalReviewExport : System.Web.UI.Page
{

    // Create the database LINQ objects.
     InsDataContext idc = new InsDataContext();
     //DataTable results;
     public static DataTable ExcelResults;

     
     private AccountRenewalList list;
     private List<AccountRenewalListItem> ItemList;

    protected void Page_Load(object sender, EventArgs e)
    {
        // Prevent the page from redrawing the table if not a first time load.
        if (!IsPostBack)
        {
            bindBillingTermsCombo();
            bindBillingMethodCombo();
            bindBrandNameCombo();

            // Bind the grid
            bindRenewalGrid();  
        }

    }
    /// <summary>
    /// Fired when the page has been changed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvUpcomingRenewals_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvUpcomingRenewals.PageIndex = e.NewPageIndex;
        bindRenewalGrid();
    }

   
    /// <summary>
    /// Hides the review and completed panels and displays the renewals table.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnBack_Click(object sender, EventArgs e)
    {
       // pnlCompleted.Visible = false;
     //   pnlRenewalsTable.Visible = true;
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
        gvUpcomingRenewals.PageIndex = 0;

        // Bind the gridview
        bindRenewalGrid();
    }

    /// <summary>
    /// When the column is being sorted.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvUpcomingRenewals_Sorting(object sender, GridViewSortEventArgs e)
    {
        gvUpcomingRenewals.PageIndex = 0;
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
        sqlCmd.CommandType = CommandType.Text;

        //*** 5/25/12 WK - View debugged and modified.
        // Create the query for the view.
        //string query = "SELECT * FROM [CMY_Renewal_Verification]";

        string query = "SELECT * FROM [vw_RenewalVerification]";
        string filters = "";

        // Set the filter parameter for billing method
        if (ddlBillingMethod.SelectedValue != "")
        {
            filters += "([BillingMethod] = @billingMethod)";
            sqlCmd.Parameters.Add(new SqlParameter("billingMethod", ddlBillingMethod.SelectedItem.Text));
        }

        // Set the filter parameter for billing term
        if (ddlBillingTerm.SelectedValue != "")
        {
            // Append the AND if needed.
            if (filters != "") filters += " AND ";

            filters += "([BillingTermDesc] = @billingTerm)";
            sqlCmd.Parameters.Add(new SqlParameter("billingTerm", ddlBillingTerm.SelectedItem.Text));
        }

        // Set the filter parameter for the brand
         if (ddlBrand.SelectedValue != "")
        {
            // Append the AND if needed.
            if (filters != "") filters += " AND ";

            filters += "([BrandName] = @brandName)";
            sqlCmd.Parameters.Add(new SqlParameter("brandName", ddlBrand.SelectedItem.Value));
        }

       
        // Append the AND if needed.
        
        // Add the filters to the query if needed.
      if (filters != "") query += " WHERE " + filters;

        // Add the order by and the direction
        query += " ORDER BY " + gvUpcomingRenewals.CurrentSortedColumn + ((gvUpcomingRenewals.CurrentSortDirection == SortDirection.Ascending) ? " ASC" : " DESC");

        // Pass the query string to the command
        
        sqlCmd.Connection = sqlConn;

        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();

        sqlCmd.CommandText = query;

        SqlDataReader reader = sqlCmd.ExecuteReader();

        AccountRenewalList RenewalList = new AccountRenewalList();
        RenewalList.Renewals = new List<AccountRenewalListItem>();

         while (reader.Read())
         {
  
             //_money = Convert.ToDecimal(reader["InvoiceAmt"].ToString());
             //_strmoney = String.Format("{0:C}", reader["InvoiceAmt"].ToString());

             AccountRenewalListItem RenewalItem = new AccountRenewalListItem()
             {
                AccountID = int.Parse(reader["AccountID"].ToString()),
                CompanyName = reader["CompanyName"].ToString(),
                OrderID = int.Parse(reader["OrderID"].ToString()),
                InvoiceNo = reader["InvoiceNo"].ToString(),
                InvoiceAmt = Decimal.Parse(reader["InvoiceAmt"].ToString()).ToString("C"),
                RevStartDate = reader["RevStartDate"].ToString(),
                RevEndDate = reader["RevEndDate"].ToString(),
 
                RevAmt = Decimal.Parse(reader["RevAmt"].ToString()).ToString("C"),

                OrderDate = reader["OrderDate"].ToString(),
                BillingTermDesc = reader["BillingTermDesc"].ToString(),
                BillingMethod = reader["BillingMethod"].ToString(), 
                BrandSourceID = int.Parse(reader["BrandSourceID"].ToString()),
                BrandName = reader["BrandName"].ToString(),
             };


            RenewalList.Renewals.Add(RenewalItem);
        }

         ItemList = RenewalList.Renewals;

         // Bind the results to the gridview
         gvUpcomingRenewals.DataSource = RenewalList.Renewals;
         gvUpcomingRenewals.DataBind();

        //// Create a data table to place the results
        //results = new DataTable();

        //// Retrieve the data from SQL
        //using (SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd))
        //{
        //    sqlDA.Fill(results);
        //}
         

    }

    protected void btnConvert_Click(object sender, EventArgs e)
    {
        
        // Bind the grid, to fill the Renewals list for Excel export.
        bindRenewalGrid();

        //// Create an data table used to export
        DataTable exportTable = new DataTable();

        
        //Create columns for this DataTable
        DataColumn colAccountID = new DataColumn("Account#");
        DataColumn colCompName = new DataColumn("Company Name");
        DataColumn colOrderID = new DataColumn("Order#");
        DataColumn colnvoiceNo = new DataColumn("Invoice#");
        DataColumn colInvoiceAmt = new DataColumn("Amount");
        DataColumn colRevStartDate = new DataColumn("Contract Start");
        DataColumn colRevEndDate = new DataColumn("Contract Date");
        DataColumn colRevAmt = new DataColumn("Rev.Amt");
        DataColumn colOrderDate = new DataColumn("Ordered");

        DataColumn colBillingTermDesc = new DataColumn("Term");
        DataColumn colBillingMethod= new DataColumn("Method");
        //DataColumn colBrandSourceID= new DataColumn("BrandSourceID");
        DataColumn colBrandName = new DataColumn("Brand");
        
        //Define DataType of the Columns
        colAccountID.DataType = System.Type.GetType("System.String");
        colCompName.DataType = System.Type.GetType("System.String");
        colOrderID.DataType = System.Type.GetType("System.String");
        colnvoiceNo.DataType = System.Type.GetType("System.String");
        colInvoiceAmt.DataType = System.Type.GetType("System.String");
        colRevStartDate.DataType = System.Type.GetType("System.String");
        colRevEndDate.DataType = System.Type.GetType("System.String");
        colRevAmt.DataType = System.Type.GetType("System.String");
        colOrderDate.DataType = System.Type.GetType("System.String");

        colBillingTermDesc.DataType = System.Type.GetType("System.String");
        colBillingMethod.DataType = System.Type.GetType("System.String");
        //colBrandSourceID.DataType = System.Type.GetType("System.String");
        colBrandName.DataType = System.Type.GetType("System.String");
        
        //Add All These Columns into exportTable
        exportTable.Columns.Add(colAccountID);
        exportTable.Columns.Add(colCompName);
        exportTable.Columns.Add(colOrderID);
        exportTable.Columns.Add(colnvoiceNo);
        exportTable.Columns.Add(colInvoiceAmt);
        exportTable.Columns.Add(colRevStartDate);
        exportTable.Columns.Add(colRevEndDate);
        exportTable.Columns.Add(colRevAmt);
        exportTable.Columns.Add(colOrderDate);
        exportTable.Columns.Add(colBillingTermDesc);
        exportTable.Columns.Add(colBillingMethod);
        //exportTable.Columns.Add(colBrandSourceID);
        exportTable.Columns.Add(colBrandName);
       

        //// Add the rows from the account renewals list
        foreach (AccountRenewalListItem rl in ItemList)
        {
            // Create a new table row
            DataRow dr = exportTable.NewRow();
            dr[colAccountID] = rl.AccountID;
            dr[colCompName] = rl.CompanyName;
            dr[colOrderID] = rl.OrderID;
            dr[colnvoiceNo] = rl.InvoiceNo;
            dr[colInvoiceAmt] = rl.InvoiceAmt;
            dr[colRevStartDate] = rl.RevStartDate;
            dr[colRevEndDate] = rl.RevEndDate;
            dr[colRevAmt] = rl.RevAmt;
            dr[colOrderDate] = rl.OrderDate;
            dr[colBillingTermDesc] = rl.BillingTermDesc;
            dr[colBillingMethod] = rl.BillingMethod;
            //dr[colBrandSourceID] = rl.BrandSourceID;
            dr[colBrandName] = rl.BrandName;

            //dr[5] = ll.Active ? GetLocalResourceObject("Yes").ToString() : GetLocalResourceObject("No").ToString();

            // Add the row to the table
            exportTable.Rows.Add(dr);
        }

        // Build the export table
        TableExport tableExport = new TableExport(exportTable);

        try
        {
            // Read the CSS template from file
            tableExport.Stylesheet =
                System.IO.File.ReadAllText(Server.MapPath("~/Templates/export/grids.css"));
        }
        catch { }


        //// Create the export file based on the selected value
        tableExport.Export("ReviewRenewals", "XLS");

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

    public override void VerifyRenderingInServerForm(Control control)
    {
        return;
    }
}
