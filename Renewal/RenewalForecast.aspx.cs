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

public partial class InstaDose_Finance_Renewal_RenewalForecast : System.Web.UI.Page
{

    // Create the database LINQ objects.
     InsDataContext idc = new InsDataContext();
     //DataTable results;
     public static DataTable ExcelResults;

     
     private RenewalForecastList list;
     private List<RenewalForecastListItem> ItemList;

    protected void Page_Load(object sender, EventArgs e)
    {
        // Prevent the page from redrawing the table if not a first time load.
        if (!IsPostBack)
        {
            // Bind the grid
            bindRenewalGrid();  
        }



    }
    /// <summary>
    /// Fired when the page has been changed.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvForecastRenewals_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvForecastRenewals.PageIndex = e.NewPageIndex;
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
        gvForecastRenewals.PageIndex = 0;

        // Bind the gridview
        bindRenewalGrid();
    }

    /// <summary>
    /// When the column is being sorted.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvForecastRenewals_Sorting(object sender, GridViewSortEventArgs e)
    {
        gvForecastRenewals.PageIndex = 0;
        bindRenewalGrid();
    }

    //private void bindBillingMethodCombo()
    //{

    //    this.ddlBillingMethod.DataSource = (from idcBillingMethod in idc.PaymentMethods
    //                                       select new { DESC = idcBillingMethod.PaymentMethodID,
    //                                                    ID = idcBillingMethod.PaymentMethodName}).ToList();

    //    this.ddlBillingMethod.DataTextField = "ID";
    //    this.ddlBillingMethod.DataValueField = "DESC";

    //    this.ddlBillingMethod.DataBind();
    //    ddlBillingMethod.Items.Insert(0, new ListItem("ALL", "")); 
    //}

    //private void bindBillingTermsCombo()
    //{

    //    this.ddlBillingTerm.DataSource = (from idcBillingMethod in idc.BillingTerms
    //                                        select new
    //                                        {
    //                                            DESC = idcBillingMethod.BillingTermID,
    //                                            ID = idcBillingMethod.BillingTermDesc
    //                                        }).ToList();

    //    this.ddlBillingTerm.DataTextField = "ID";
    //    this.ddlBillingTerm.DataValueField = "DESC";

    //    this.ddlBillingTerm.DataBind();
    //    ddlBillingTerm.Items.Insert(0, new ListItem("ALL", "")); 
    //}

    //private void bindBrandNameCombo()
    //{

    //    this.ddlBrand.DataSource = (from idcBillingMethod in idc.BrandSources
    //                                        select new
    //                                        {
    //                                            DESC = idcBillingMethod.BrandSourceID,
    //                                            ID = idcBillingMethod.BrandName
    //                                        }).ToList();
    //    this.ddlBrand.DataTextField = "ID";
    //    this.ddlBrand.DataValueField = "DESC";
    //    this.ddlBrand.DataBind();
    //    ddlBrand.Items.Insert(0, new ListItem("ALL", "")); 
    //}

    /// <summary>
    /// Query the data source and bind the data grid.
    /// </summary>
    private void bindRenewalGrid()
    {
        try
        {

            // Create the sql connection from the connection string in the web.config
            SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
                ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

            // Create the sql command.
            SqlCommand sqlCmd = new SqlCommand();
            //sqlCmd.Connection = sqlConn;
           // sqlCmd.CommandType = CommandType.Text;
            sqlCmd.CommandType = CommandType.StoredProcedure;

            //*** 5/25/12 WK - View debugged and modified.
            // Create the query for the view.

            //string query = "EXEC sp_GetRenewalsForecast ";
           
            //      EXEC  sp_GetRenewalsForecast
            //@pForcastBeginPeriod = '9/1/2012',
            //@pForcastEndPeriod = '12/31/2012'

            string filters = "";

            sqlCmd.CommandText = "sp_GetRenewalsForecast";
             
            //  sp_GetRenewalsForecast
            //@pForcastBeginPeriod = '9/1/2012',
            //@pForcastEndPeriod = '12/31/2012'

            //// Set the filter parameter for billing method
            //if (ddlBillingMethod.SelectedValue != "")
            //{
            //    filters += "([BillingMethod] = @billingMethod)";
            //    sqlCmd.Parameters.Add(new SqlParameter("billingMethod", ddlBillingMethod.SelectedItem.Text));
            //}

            //// Set the filter parameter for billing term
            //if (ddlBillingTerm.SelectedValue != "")
            //{
            //    // Append the AND if needed.
            //    if (filters != "") filters += " AND ";

            //    filters += "([BillingTermDesc] = @billingTerm)";
            //    sqlCmd.Parameters.Add(new SqlParameter("billingTerm", ddlBillingTerm.SelectedItem.Text));
            //}

            //// Set the filter parameter for the brand
            // if (ddlBrand.SelectedValue != "")
            //{
            //    // Append the AND if needed.
            //    if (filters != "") filters += " AND ";

            //    filters += "([BrandName] = @brandName)";
            //    sqlCmd.Parameters.Add(new SqlParameter("brandName", ddlBrand.SelectedItem.Value));
            //}

            // Period From and To
            if (txtPeriodFrom.Text != "" && txtPeriodTo.Text != "")
            {
                DateTime dtPeriodFrom = DateTime.Now;
                DateTime dtPeriodTo = DateTime.Now;
                //string strFrom = txtPeriodFrom.Text;
                //string strTo = txtPeriodTo.Text;

                // Ensure the date time stuff can be parsed.
                if (DateTime.TryParse(txtPeriodFrom.Text, out dtPeriodFrom) && DateTime.TryParse(txtPeriodTo.Text, out dtPeriodTo))
                {
                    if (dtPeriodFrom <= dtPeriodTo)
                    {
                        // Append the AND if needed.
                        if (filters != "") filters += " AND ";

                        //filters += "@pForcastBeginPeriod = '" + strFrom + "' ";
                        //filters += filters += "@pForcastEndPeriod = '" + strTo + "'";
                        //filters += filters += "'" + strFrom + "',";
                        //filters += filters += "'" + strTo + "'";
       
                        sqlCmd.Parameters.Add(new SqlParameter("@pForcastBeginPeriod", dtPeriodFrom));
                        sqlCmd.Parameters.Add(new SqlParameter("@pForcastEndPeriod", dtPeriodTo));
                    }
                    else
                    {
                        // From date must be BEFORE the to date.
                    }
                }
                else
                {
                    // Dates are not in the correct format.
                }
            }

            // Append the AND if needed.

            // Add the filters to the query if needed.
            //if (filters != "") query += " WHERE " + filters;
            //if (filters != "") query += filters;

            // Add the order by and the direction
            //string sortQuery = null;

            //sortQuery += " ORDER BY " + gvForecastRenewals.CurrentSortedColumn + 
            //    ((gvForecastRenewals.CurrentSortDirection == SortDirection.Ascending) ? " ASC" : " DESC");
 
            //sqlCmd.Parameters.Add(new SqlParameter("@sortQuery", sortQuery));
 
            // Pass the query string to the command

            sqlCmd.Connection = sqlConn;

            bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
            if (!openConn) sqlCmd.Connection.Open();

            //sqlCmd.CommandText = query;
            sqlCmd.CommandText = "sp_GetRenewalsForecast";

            SqlDataReader reader = sqlCmd.ExecuteReader();

            RenewalForecastList RenewalList = new RenewalForecastList();
            RenewalList.RenewalForecasts = new List<RenewalForecastListItem>();
            
            //string _strTotal = null;
            decimal total = 0;

            while (reader.Read())
            {

                //_money = Convert.ToDecimal(reader["InvoiceAmt"].ToString());
                //_strmoney = String.Format("{0:C}", reader["InvoiceAmt"].ToString());
                //_strTotal = String.Format("{0.00}", reader["RenewalTotal"].ToString());

                //f (!Decimal.TryParse(reader["RenewalTotal"].ToString().ToString("0.00"),
                //      out _strTotal)) continue;
                
                if (!Decimal.TryParse(reader["RenewalTotal"].ToString(), out total)) continue;
                
                RenewalForecastListItem RenewalItem = new RenewalForecastListItem()
                {
                    AccountID = int.Parse(reader["AccountID"].ToString()),
                    AccountName = reader["AccountName"].ToString(),
                    BrandName = reader["BrandName"].ToString(),
                    DeviceCount = int.Parse(reader["DeviceCount"].ToString()),
                    RenewalYear = reader["RenewalYear"].ToString(),
                    RenewalMonth = reader["RenewalMonth"].ToString(),
                    RenewalRateID = int.Parse(reader["RenewalRateID"].ToString()),
                    RenewalRate = reader["RenewalRate"].ToString(),

                    //RenewalTotal = Decimal.Parse(reader["RenewalTotal"].ToString()).ToString("C"),
                    //RenewalTotal = Decimal.Parse(reader["RenewalTotal"].ToString()).ToString("{0.00}"),

                    //RenewalTotal = _strTotal + " " +
                    //                 reader["CurrencyCode"].ToString(),
                    //CurrencyCode = reader["CurrencyCode"].ToString(),

                    RenewalTotal = Currencies.CurrencyToString(total,
                                        reader["CurrencyCode"].ToString()),

                    BrandSourceID = int.Parse(reader["BrandSourceID"].ToString()),
                    RenewalDate = reader["RenewalDate"].ToString(),

                };


                RenewalList.RenewalForecasts.Add(RenewalItem);
            }

            ItemList = RenewalList.RenewalForecasts;
                   

            // If search filter is not blank look for stuff
            //if (txtFilter.Text != string.Empty)
            //    ItemList = ItemList.FindAll(FindLocation);

            // Sort the list based on the column
            switch (gvForecastRenewals.CurrentSortedColumn)
            {
                case "AccountID":
                    // Sort the list by AccountID
                    ItemList.Sort(delegate(RenewalForecastListItem i1, RenewalForecastListItem i2) { return i1.AccountID.CompareTo(i2.AccountID); });
                    break;
                case "AccountName":
                    // Sort the list by AccountName
                    ItemList.Sort(delegate(RenewalForecastListItem i1, RenewalForecastListItem i2) { return i1.AccountName.CompareTo(i2.AccountName); });
                    break;
                case "RenewalYear":
                    // Sort the list by RenewalYear
                    ItemList.Sort(delegate(RenewalForecastListItem i1, RenewalForecastListItem i2) { return i1.RenewalYear.CompareTo(i2.RenewalYear); });
                    break;
                case "Brand":
                    // Sort the list by brand
                    ItemList.Sort(delegate(RenewalForecastListItem i1, RenewalForecastListItem i2) { return i1.BrandName.CompareTo(i2.BrandName); });
                    break;

                //9/9/2012 WK - Need work to get Month and Total to sort correctly.  String values - need to sort by numeric.
                case "RenewalMonth":
                    // Sort the list by RenewalMonth
                    ItemList.Sort(delegate(RenewalForecastListItem i1, RenewalForecastListItem i2) { return i1.RenewalMonth.CompareTo(i2.RenewalMonth); });
                    break;
                //
                //case "RenewalTotal":
                //    // Sort the list by RenewalTotal
                //    ItemList.Sort(delegate(RenewalForecastListItem i1, RenewalForecastListItem i2) { return i1.RenewalTotal.CompareTo(i2.RenewalTotal); });
                //    break;
            }
        
            // Flip the list
            if (gvForecastRenewals.CurrentSortDirection == SortDirection.Descending) ItemList.Reverse();

            // Bind the results to the gridview
            gvForecastRenewals.DataSource = RenewalList.RenewalForecasts;
            gvForecastRenewals.DataBind();

        }
        catch (Exception ex)
        {
        }

    }

    /// <summary>
    /// When the text is change in the from period textbox.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void txtPeriodFrom_TextChange(object sender, EventArgs e)
    {
        // Clear the style.
        txtPeriodFrom.CssClass = "";

        // Exit if there is no text.
        if (txtPeriodFrom.Text == "") return;

        // Test the see if the date is valid
        DateTime fromDate = DateTime.Now;
        if (!DateTime.TryParse(txtPeriodFrom.Text, out fromDate))
        {
            txtPeriodFrom.CssClass = "invalidText";
            return;
        }

        // Test to ensure the date is less than To Period
        DateTime toDate = DateTime.Now;
        if (!DateTime.TryParse(txtPeriodTo.Text, out toDate)) return;

        // Ensure the from date is less than to date
        if (fromDate > toDate)
        {
            txtPeriodFrom.CssClass = "invalidText";
            return;
        }

        //// Reset the page to 0
        //this.gvForecastRenewals.PageIndex = 0;

        //// Rebind the renewals grid.
        //bindRenewalGrid();
    }

    /// <summary>
    /// When the text is change in the to period textbox.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void txtPeriodTo_TextChange(object sender, EventArgs e)
    {
        // Clear the style.
        txtPeriodTo.CssClass = "";

        // Exit if there is no text.
        if (txtPeriodTo.Text == "") return;

        // Test the see if the date is valid
        DateTime toDate = DateTime.Now;
        if (!DateTime.TryParse(txtPeriodTo.Text, out toDate))
        {
            txtPeriodTo.CssClass = "invalidText";
            return;
        }

        // Test to ensure the date is less than From Period
        DateTime fromDate = DateTime.Now;
        if (!DateTime.TryParse(txtPeriodFrom.Text, out fromDate)) return;

        // Ensure the from date is less than to date
        if (fromDate > toDate)
        {
            txtPeriodTo.CssClass = "invalidText";
            return;
        }

        //// Reset the page to 0
        //this.gvForecastRenewals.PageIndex = 0;

        //// Rebind the renewals grid.
        //bindRenewalGrid();
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        // Reset the page to 0
        this.gvForecastRenewals.PageIndex = 0;

        // Bind the grid, to fill the Renewals list for Excel export.
        bindRenewalGrid();
    }

    protected void btnConvert_Click(object sender, EventArgs e)
    {
        
        // Bind the grid, to fill the Renewals list for Excel export.
        bindRenewalGrid();

        //// Create an data table used to export
        DataTable exportTable = new DataTable();

        
        //Create columns for this DataTable
        DataColumn colAccountID = new DataColumn("Account#");
        DataColumn colAccountName = new DataColumn("AccountName");
        DataColumn colRenewalMonth = new DataColumn("RenewalMonth");
        DataColumn colRenewalYear = new DataColumn("RenewalYear");

        DataColumn colRenewalTotal = new DataColumn("RenewalTotal");
        DataColumn colCurrencyCode = new DataColumn("CurrencyCode");

        //DataColumn colBillingTermDesc = new DataColumn("Term");
        //DataColumn colBillingMethod= new DataColumn("Method");
        //DataColumn colBrandSourceID= new DataColumn("BrandSourceID");
        DataColumn colBrandName = new DataColumn("Brand");
        
        //Define DataType of the Columns
        colAccountID.DataType = System.Type.GetType("System.String");
        colAccountName.DataType = System.Type.GetType("System.String");
        colBrandName.DataType = System.Type.GetType("System.String");
        colRenewalMonth.DataType = System.Type.GetType("System.String");
        colRenewalYear.DataType = System.Type.GetType("System.String");
        colRenewalTotal.DataType = System.Type.GetType("System.String");
        colCurrencyCode.DataType = System.Type.GetType("System.String");

        //Add All These Columns into exportTable
        exportTable.Columns.Add(colAccountID);
        exportTable.Columns.Add(colAccountName);
        exportTable.Columns.Add(colBrandName);
        exportTable.Columns.Add(colRenewalMonth);
        exportTable.Columns.Add(colRenewalYear);
        exportTable.Columns.Add(colRenewalTotal);
        exportTable.Columns.Add(colCurrencyCode);
        
        //Any renewals found?  If not, return!
        try
        {
            //// Add the rows from the account renewals list
            foreach (RenewalForecastListItem rl in ItemList)
            {
                // Create a new table row
                DataRow dr = exportTable.NewRow();
                dr[colAccountID] = rl.AccountID;
                dr[colAccountName] = rl.AccountName;
                dr[colBrandName] = rl.BrandName;
                dr[colRenewalMonth] = rl.RenewalMonth;
                dr[colRenewalYear] = rl.RenewalYear;
                dr[colRenewalTotal] = rl.RenewalTotal;
                dr[colCurrencyCode] = rl.CurrencyCode;

                //dr[5] = ll.Active ? GetLocalResourceObject("Yes").ToString() : GetLocalResourceObject("No").ToString();

                // Add the row to the table
                exportTable.Rows.Add(dr);
            }

            // Build the export table
            TableExport tableExport = new TableExport(exportTable);

            //11/12/12 WK - Bug fix.  Wrong grid css directory.
            try
            {
                // Read the CSS template from file
                tableExport.Stylesheet =
                    System.IO.File.ReadAllText(Server.MapPath("~/css/export/grids.css"));
            }
            catch { }


            //// Create the export file based on the selected value
            tableExport.Export("Renewalforecast", "XLS");

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
        //Any renewals found?  If not, return!
        catch { }

    }

    public override void VerifyRenderingInServerForm(Control control)
    {
        return;
    }
}
