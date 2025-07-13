using CrystalDecisions.CrystalReports.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class Reports_Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            txtStartDate.Text = string.Format("{0:MM/dd/yyyy}", DateTime.Now.AddMonths(-3));
            txtEndDate.Text = string.Format("{0:MM/dd/yyyy}", DateTime.Now);
        }
    }

    protected void btnGenerate_Click(object sender, EventArgs e)
    {
        //try
        //{
        // Ensure the div containing the error label is hidden
        divFormError.Visible = false;

        // Build the crystal report and load the report file.
        ReportDocument report = new ReportDocument();
        try
        {
            report.Load(Server.MapPath("~/Reports/NotReadTheirDeviceRPT_US.rpt"));

            int accountID = 0;
            int locationID = 0;
            int groupID = 0;
            int userID = 0;
            DateTime startDate = DateTime.MinValue;
            DateTime endDate = DateTime.MaxValue;
            Stream oStream = null;

            // Pass the parameters
            if (int.TryParse(txtAccountID.Text, out accountID))
                report.SetParameterValue("@pAccountID", accountID);

            if (int.TryParse(txtLocationID.Text, out locationID))
                report.SetParameterValue("@pLocationID", locationID);
            else
                report.SetParameterValue("@pLocationID", DBNull.Value);

            if (int.TryParse(txtGroupID.Text, out groupID))
                report.SetParameterValue("@pGroupID", groupID);
            else
                report.SetParameterValue("@pGroupID", DBNull.Value);

            if (int.TryParse(txtUserID.Text, out userID))
                report.SetParameterValue("@pUserID", userID);
            else
                report.SetParameterValue("@pUserID", DBNull.Value);

            if (DateTime.TryParse(txtStartDate.Text, out startDate))
                report.SetParameterValue("@pStartDate", startDate);

            if (DateTime.TryParse(txtEndDate.Text, out endDate))
                report.SetParameterValue("@pEndDate", endDate);

            // Build the connection string.
            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString.ToString();
            string connDB = Regex.Match(connStr, "(Initial Catalog=|Database=){1}([A-Za-z0-9_]+)[;]?").Groups[2].ToString();
            string connServer = Regex.Match(connStr, "(Data Source=){1}([\\w\\d\\.\\$\\-]+)[;]?").Groups[2].ToString();
            string connUserID = Regex.Match(connStr, "(User ID=|UID=){1}([\\w\\d\\.]+)[;]?").Groups[2].ToString();
            string connPassword = Regex.Match(connStr, "(Pwd=|Password=|PWD=){1}([\\w\\d\\.\\$]+)[;]?").Groups[2].ToString();
            report.DataSourceConnections[0].SetConnection(connServer, connDB, connUserID, connPassword);
            string fileName = string.Format("{0} {1:mm-dd-yyyy}.pdf", accountID, DateTime.Now);
            // Export to Memory Stream.        
            report.ExportToHttpResponse(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, HttpContext.Current.Response, true, fileName);

        }
        catch (Exception ex)
        {
            // Handle Error
            //lblError.Text = string.Format("{0}", ex.Message);
            //divFormError.Visible = true;
        }
        finally
        {
            report.Close();
            report.Dispose();
        }
    }
}