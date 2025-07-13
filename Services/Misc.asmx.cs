using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Services;



[WebService(Namespace = "http://portal.instadose.com/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[System.Web.Script.Services.ScriptService]
public class Misc : System.Web.Services.WebService
{

    [WebMethod]
    public void GetAccountCounts(int accountid)
    {
        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.StoredProcedure;

        // Using the vw_CreditCardExpirationsListing.
        string sqlQuery = "sp_if_GetAccountCountsByAccountID";

        sqlCmd.Parameters.Add(new SqlParameter("@pAccountID", accountid));

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable dtRecordCountTable = new DataTable();

        dtRecordCountTable = new DataTable("Get Record Counts for Tabs");

        // Create the columns for the DataTable.
        dtRecordCountTable.Columns.Add("Notes", Type.GetType("System.Int32"));
        dtRecordCountTable.Columns.Add("CSRequests", Type.GetType("System.Int32"));
        dtRecordCountTable.Columns.Add("Orders", Type.GetType("System.Int32"));
        dtRecordCountTable.Columns.Add("Invoices", Type.GetType("System.Int32"));
        dtRecordCountTable.Columns.Add("Locations", Type.GetType("System.Int32"));
        dtRecordCountTable.Columns.Add("Users", Type.GetType("System.Int32"));
        dtRecordCountTable.Columns.Add("Badges", Type.GetType("System.Int32"));
        dtRecordCountTable.Columns.Add("Returns", Type.GetType("System.Int32"));
        dtRecordCountTable.Columns.Add("Reads", Type.GetType("System.Int32"));
        dtRecordCountTable.Columns.Add("Documents", Type.GetType("System.Int32"));
        dtRecordCountTable.Columns.Add("AssociatedAccounts", Type.GetType("System.Int32"));
        // Active Records Count (for Locations, Users, and Badges).
        dtRecordCountTable.Columns.Add("ActiveLocations", Type.GetType("System.Int32"));
        dtRecordCountTable.Columns.Add("ActiveUsers", Type.GetType("System.Int32"));
        dtRecordCountTable.Columns.Add("ActiveBadges", Type.GetType("System.Int32"));
        dtRecordCountTable.Columns.Add("Calendars", Type.GetType("System.Int32"));

        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        string numberTemplate = "\"{0}\":{1},";
        while (sqlDataReader.Read())
        {
            sb.Append(string.Format(numberTemplate, "Notes", sqlDataReader["Notes"].ToString()));
            sb.Append(string.Format(numberTemplate, "CSRequests", sqlDataReader["CSRequests"].ToString()));
            sb.Append(string.Format(numberTemplate, "Orders", sqlDataReader["Orders"].ToString()));
            sb.Append(string.Format(numberTemplate, "Invoices", sqlDataReader["Invoices"].ToString()));
            sb.Append(string.Format(numberTemplate, "Locations", sqlDataReader["Locations"].ToString()));
            sb.Append(string.Format(numberTemplate, "Users", sqlDataReader["Users"].ToString()));
            sb.Append(string.Format(numberTemplate, "Badges", sqlDataReader["Badges"].ToString()));
            sb.Append(string.Format(numberTemplate, "Returns", sqlDataReader["Returns"].ToString()));
            sb.Append(string.Format(numberTemplate, "Reads", sqlDataReader["Reads"].ToString()));
            sb.Append(string.Format(numberTemplate, "Documents", sqlDataReader["Documents"].ToString()));
            sb.Append(string.Format(numberTemplate, "AssociatedAccounts", sqlDataReader["AssociatedAccounts"].ToString()));
            sb.Append(string.Format(numberTemplate, "ActiveLocations", sqlDataReader["ActiveLocations"].ToString()));
            sb.Append(string.Format(numberTemplate, "ActiveUsers", sqlDataReader["ActiveUsers"].ToString()));
            sb.Append(string.Format(numberTemplate, "Calendars", sqlDataReader["Calendars"].ToString()));
            sb.Append(string.Format("\"{0}\":{1}", "ActiveBadges", sqlDataReader["ActiveBadges"].ToString()));
        }

        sqlConn.Close();
        sqlDataReader.Close();
        sb.Append("}");

        Context.Response.Clear();
        Context.Response.ContentType = "application/json";
        Context.Response.Write(sb.ToString());
        Context.Response.End();
    }

    [WebMethod]
    public string GetUserName(string username) 
    {
      
        string userName = "Unknown";

        try
        {
            string IdentityName = HttpContext.Current.Request.LogonUserIdentity.Name;
            string[] arrUserName = IdentityName.Split('\\');

            if (arrUserName.Count() >= 2)
            {
                userName = arrUserName[1];
            }
        }
        catch
        {
            throw;
        }
        return userName;
    }

    [WebMethod]
    public string GetUserRole(string username)
    {
        List<string> belongsToGroups = new List<string>();
        string IdentityName = "";
        string userName = ""; 

        IdentityName = HttpContext.Current.Request.LogonUserIdentity.Name;

        if (!string.IsNullOrEmpty(IdentityName))
        {
            string[] arrUserName = IdentityName.Split('\\');
            if (arrUserName.Count() >= 2)
                userName = arrUserName[1];
        }
        try
        {
            belongsToGroups = ActiveDirectoryQueries.GetADUserGroups(userName);
        }
        catch
        {
            belongsToGroups = new List<string>();
        }
        return JsonConvert.SerializeObject(belongsToGroups);
    }
}
