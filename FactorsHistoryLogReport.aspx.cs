using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Instadose;
using Instadose.Data;
using Instadose.Device;

using System.Text.RegularExpressions;
using System.IO;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

public partial class TechOps_FactorsHistoryLogReport : System.Web.UI.Page
{
    ReportDocument cryRpt = new ReportDocument();
    protected void Page_Load(object sender, EventArgs e)
    {
        // Grab the ID if it was passed it the query string
        if (Request.QueryString["AccountID"] == null)
            return;

        int accountID = Int32.Parse(Request.QueryString["AccountID"]);

        string myCRFileNamePath = Server.MapPath("FactorsHistoryLog.rpt");

        try
        {
            cryRpt.Load(myCRFileNamePath);

            cryRpt.SetParameterValue(0, accountID);

            string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString.ToString();
            string myDatabase = Regex.Match(ConnectionString, "(Initial Catalog=|Database=){1}([A-Za-z0-9_]+)[;]?").Groups[2].ToString();
            string myServer = Regex.Match(ConnectionString, "(Data Source=){1}([\\w\\d\\.\\$\\-]+)[;]?").Groups[2].ToString();
            string myUserID = Regex.Match(ConnectionString, "(User ID=|UID=){1}([\\w\\d\\.]+)[;]?").Groups[2].ToString();
            string myPassword = Regex.Match(ConnectionString, "(Pwd=|Password=|PWD=){1}([\\w\\d\\.\\$]+)[;]?").Groups[2].ToString();

            cryRpt.DataSourceConnections[0].SetConnection(myServer, myDatabase, myUserID, myPassword);

            CrystalDecisions.Shared.ExportFormatType exportAs = CrystalDecisions.Shared.ExportFormatType.PortableDocFormat;
            cryRpt.ExportToHttpResponse(exportAs, System.Web.HttpContext.Current.Response, false, "FactorsHistoryLogReport");
        }
        catch (Exception ex)
        {
            Response.Write(ex);
        }
        finally
        {
            cryRpt.Close();
            cryRpt.Dispose();
        }

    }
}