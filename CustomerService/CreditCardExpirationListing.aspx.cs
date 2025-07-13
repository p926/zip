using System;
using System.Configuration;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data.SqlClient;

using Instadose;
using Instadose.Data;

public partial class CustomerService_CreditCardExpirationListing : System.Web.UI.Page
{
    // Build list of error messages.
    List<string> errorMessageList = new List<string>();

    // Create the database reference.
    InsDataContext idc = new InsDataContext();

    // String to hold the current Username.
    string UserName = "Unknown";

    protected void Page_PreRenderComplete(object sender, EventArgs e)
    {
        bindCreditCardExpirationsGrid();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            this.UserName = User.Identity.Name.Split('\\')[1];
        }
        catch { this.UserName = "Unknown"; }

        if (!IsPostBack)
        {
            bindCreditCardTypesDDL();
            bindCreditCardExpirationsGrid();
        }
    }

    /// <summary>
    /// Reset Error Message.
    /// </summary>
    private void InvisibleError()
    {
        // Reset submission Form Error Message.
        this.creditCardExpirationErrorMsg.InnerText = "";
        this.creditCardExpirationError.Visible = false;
    }

    /// <summary>
    /// Set Error Message.
    /// </summary>
    /// <param name="errorMsg"></param>
    private void VisibleError(string errorMsg)
    {
        this.creditCardExpirationErrorMsg.InnerText = errorMsg;
        this.creditCardExpirationError.Visible = true;
    }

    /// <summary>
    /// SQL Query and Bind to GridView.
    /// </summary>
    private void bindCreditCardExpirationsGrid()
    {
        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        //// Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.Text;

        // Using the vw_CreditCardExpirationsListing.
        string sqlQuery = "SELECT * FROM [vw_CreditCardExpirationsListing]";
        string filters = "";

        // Set the filter parameter for DealerID.
        if (this.ddlCreditCardTypes.SelectedValue != "0")
        {
            filters += "([CreditCardName] = @pCreditCardName)";
            sqlCmd.Parameters.Add(new SqlParameter("@pCreditCardName", ddlCreditCardTypes.SelectedValue));
        }

        // Set the filter parameter for the ExpirationDate Range (From - To).
        if (txtFromExpirationDate.Text != "" && txtToExpirationDate.Text != "")
        {
            DateTime dtRangeFrom = DateTime.Now;
            DateTime dtRangeTo = DateTime.Now;

            // Ensure the DateTime values can be parsed.
            if (DateTime.TryParse(txtFromExpirationDate.Text, out dtRangeFrom) && DateTime.TryParse(txtToExpirationDate.Text, out dtRangeTo))
            {
                if (dtRangeFrom <= dtRangeTo)
                {
                    DateTime from = new DateTime(dtRangeFrom.Year, dtRangeFrom.Month, 1).AddDays(0);
                    DateTime to = new DateTime(dtRangeTo.Year, dtRangeTo.Month, 1).AddMonths(1).AddDays(-1);
                    // Append AND if needed.
                    if (filters != "") filters += " AND ";
                    filters += "([DT_ExpirationDate] BETWEEN @pFromDate AND @pToDate)";
                    sqlCmd.Parameters.Add(new SqlParameter("@pFromDate", from));
                    sqlCmd.Parameters.Add(new SqlParameter("@pToDate", to));
                }
                else
                {
                    // From date must be BEFORE To date.
                    string errorMsg = "From date is greater than To date.";
                    VisibleError(errorMsg);
                }
            }
            else
            {
                // The Dates are not in the correct format.
                string errorMsg = "Dates are in an invalid format.";
                VisibleError(errorMsg);
            }
        }

        // Add the filters to the query if needed.
        if (filters != "") sqlQuery += " WHERE " + filters;

        // Add the ORDER BY and DIRECTION.
        sqlQuery += " ORDER BY " + gvCreditCardExpirations.CurrentSortedColumn + ((gvCreditCardExpirations.CurrentSortDirection == System.Web.UI.WebControls.SortDirection.Ascending) ? " ASC" : " DESC");

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader dreader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable dtCreditCardExpirations = new DataTable();

        dtCreditCardExpirations = new DataTable("Credit Card Expirations");

        // Create the columns for the DataTable.
        dtCreditCardExpirations.Columns.Add("AccountID", Type.GetType("System.Int32"));
        dtCreditCardExpirations.Columns.Add("CompanyName", Type.GetType("System.String"));
        dtCreditCardExpirations.Columns.Add("CreditCardName", Type.GetType("System.String"));
        dtCreditCardExpirations.Columns.Add("NumberEncrypted", Type.GetType("System.String"));
        dtCreditCardExpirations.Columns.Add("DT_ExpirationDate", Type.GetType("System.DateTime"));

        while (dreader.Read())
        {
            DataRow drow = dtCreditCardExpirations.NewRow();

            string creditCardName = dreader["CreditCardName"].ToString();
            string creditCardEncrypted = dreader["NumberEncrypted"].ToString();

            // Fill row details.
            drow["AccountID"] = dreader["AccountID"];
            drow["CompanyName"] = dreader["CompanyName"];
            drow["CreditCardName"] = creditCardName;
            drow["NumberEncrypted"] = Common.MaskCreditCardNumber(creditCardEncrypted, creditCardName);
            drow["DT_ExpirationDate"] = dreader["DT_ExpirationDate"];

            // Add rows to DataTable.
            dtCreditCardExpirations.Rows.Add(drow);
        }

        // Bind the results to the gridview
        gvCreditCardExpirations.DataSource = dtCreditCardExpirations;
        gvCreditCardExpirations.DataBind();
    }

    protected void txtFromExpirationDate_TextChanged(object sender, EventArgs e)
    {
        // Exit/Return if no Text.
        if (txtFromExpirationDate.Text == "") return;

        DateTime dtFromDate = DateTime.Now;
        DateTime dtToDate = DateTime.Now;

        // Test to see if DateTimes are valid.
        // From Date.
        if (!DateTime.TryParse(txtFromExpirationDate.Text, out dtFromDate)) return;
        // To Date.
        if (!DateTime.TryParse(txtToExpirationDate.Text, out dtToDate)) return;

        // Test to ensure the From Date is LESS THAN the To Date.
        if (dtFromDate > dtToDate)
        {
            // The From Date is GREATER THAN To Date.
            string errorMsg = "From date is greater than To date.";
            errorMessageList.Add(errorMsg);
        }
        else
        {
            InvisibleError();
        }

        // Reset the page to 0
        gvCreditCardExpirations.PageIndex = 0;

        // Bind the gridview
        bindCreditCardExpirationsGrid();
    }

    protected void txtToExpirationDate_TextChanged(object sender, EventArgs e)
    {
        // Exit/Return if no Text.
        if (txtToExpirationDate.Text == "") return;

        DateTime dtFromDate = DateTime.Now;
        DateTime dtToDate = DateTime.Now;

        // Test to see if DateTimes are valid.
        // From Date.
        if (!DateTime.TryParse(txtFromExpirationDate.Text, out dtFromDate)) return;
        // To Date.
        if (!DateTime.TryParse(txtToExpirationDate.Text, out dtToDate)) return;

        // Test to ensure the From Date is LESS THAN the To Date.
        if (dtFromDate > dtToDate)
        {
            // The From Date is GREATER THAN To Date.
            string errorMsg = "From date is greater than To date.";
            errorMessageList.Add(errorMsg);
        }
        else
        {
            InvisibleError();
        }

        // Reset the page to 0
        gvCreditCardExpirations.PageIndex = 0;

        // Bind the gridview
        bindCreditCardExpirationsGrid();
    }

    private void bindCreditCardTypesDDL()
    {
        var creditcardtype = from cct in idc.CreditCardTypes
                             orderby cct.CreditCardName
                             select new
                             {
                                cct.CreditCardName
                             };

        this.ddlCreditCardTypes.DataSource = creditcardtype;
        this.ddlCreditCardTypes.DataTextField = "CreditCardName";
        this.ddlCreditCardTypes.DataValueField = "CreditCardName";
        this.ddlCreditCardTypes.DataBind();
    }

    protected void gridviewBinder(object sender, EventArgs e)
    {
        // Reset the page to 0
        gvCreditCardExpirations.PageIndex = 0;

        // Bind the gridview
        bindCreditCardExpirationsGrid();
    }

    protected void gvCreditCardExpirations_Sorting(object sender, GridViewSortEventArgs e)
    {
        gvCreditCardExpirations.PageIndex = 0;
    }

    protected void gvCreditCardExpirations_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvCreditCardExpirations.PageIndex = e.NewPageIndex;
        bindCreditCardExpirationsGrid();
    }
}