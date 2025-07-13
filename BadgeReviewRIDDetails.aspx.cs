using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Telerik.Web.UI;

namespace portal_instadose_com_v3.TechOps
{
    public partial class BadgeReviewRIDDetails : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        
        }

        protected void rgReads_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
        {
            if (string.IsNullOrEmpty(Request.QueryString["RID"]))
                return;

            long rid = long.Parse(Request.QueryString["RID"]);
            (sender as RadGrid).DataSource = GetReadDetailsData(rid);
        }

        protected void rgExceptions_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
        {
            if (string.IsNullOrEmpty(Request.QueryString["serialNumber"]))
                return;

            string serialNumber = Request.QueryString["serialNumber"];
            (sender as RadGrid).DataSource = GetDeviceExceptions(serialNumber);
        }

        protected void rgDailyMotions_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
        {
            if (string.IsNullOrEmpty(Request.QueryString["serialNumber"]))
                return;

            string serialNumber = Request.QueryString["serialNumber"];
            (sender as RadGrid).DataSource = GetDeviceDailyMotions(serialNumber);
        }

        private DataTable GetReadDetailsData(long rid)
        {
            // Create the SQL Connection from the connection string in the web.config
            SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
                ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

            // Create the SQL Command.
            SqlCommand sqlCmd = new SqlCommand();
            sqlCmd.CommandType = CommandType.Text;

            sqlCmd.Connection = sqlConn;
            bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
            if (!openConn) sqlCmd.Connection.Open();

            string sqlQuery = string.Format("SELECT * FROM [vw_UserDeviceReads] where RID = {0}", rid);
            sqlCmd.CommandText = sqlQuery;

            // Create a Date Table to hold the contents/results.
            DataTable dt = new DataTable("Get RID Details");
            dt.Load(sqlCmd.ExecuteReader());

            return dt;
        }

        private DataTable GetDeviceExceptions(string serialNumber)
        {
            // Create the SQL Connection from the connection string in the web.config
            SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
                ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

            // Create the SQL Command.
            SqlCommand sqlCmd = new SqlCommand();
            sqlCmd.CommandType = CommandType.Text;

            sqlCmd.Connection = sqlConn;
            bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
            if (!openConn) sqlCmd.Connection.Open();

            var query = "SELECT TOP 1 SerialNo,ExceptionDate,CreatedDate,Code" +
                ", case when Code in (3, 4, 5, 6) then ((AdditionalInfo/100) - 273.15)*9/5 + 32" +
                "   else AdditionalInfo end AdditionalInfo " +
                ",ExceptionDesc,InfoDesc" +
                " FROM [dbo].[vw_ID3Exceptions] e " +
                " WHERE SerialNo = {0} order by ExceptionDate desc";

            string sqlQuery = string.Format(query, serialNumber);
            sqlCmd.CommandText = sqlQuery;

            // Create a Date Table to hold the contents/results.
            DataTable dt = new DataTable("Get Device Exceptions");
            dt.Load(sqlCmd.ExecuteReader());

            return dt;
        }

        private DataTable GetDeviceDailyMotions(string serialNumber)
        {
            // Create the SQL Connection from the connection string in the web.config
            SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
                ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

            // Create the SQL Command.
            SqlCommand sqlCmd = new SqlCommand();
            sqlCmd.CommandType = CommandType.Text;

            sqlCmd.Connection = sqlConn;
            bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
            if (!openConn) sqlCmd.Connection.Open();

            string sqlQuery = string.Format("SELECT top 1 * FROM [dbo].[ID3_DailyMotion] where SerialNo = {0}  order by MotionDetectedDate desc", serialNumber);
            sqlCmd.CommandText = sqlQuery;

            // Create a Date Table to hold the contents/results.
            DataTable dt = new DataTable("Get Device Exceptions");
            dt.Load(sqlCmd.ExecuteReader());

            return dt;
        }
    }
}