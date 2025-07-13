using CrystalDecisions.CrystalReports.Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace portal_instadose_com_v3.InformationFinder.Details
{
    public partial class ViewInvoice : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ReportDocument reportDocument = new ReportDocument();
            try
            {
                var invoice = Request.QueryString["Invoice"];

                reportDocument.Load(Path.Combine(Server.MapPath("~/Templates"), "Instadose_Invoice.rpt"));
                reportDocument.SetParameterValue("@pInvoiceNo", invoice);
                var isAttachment = false;

                string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.MASConnectionString"].ConnectionString.ToString();
                string connDB = Regex.Match(connStr, "(Initial Catalog=|Database=){1}([A-Za-z0-9_]+)[;]?").Groups[2].ToString();
                string connServer = Regex.Match(connStr, "(Data Source=){1}([\\w\\d\\.\\$\\-]+)[;]?").Groups[2].ToString();
                string connUserID = Regex.Match(connStr, "(User ID=|UID=){1}([\\w\\d\\.]+)[;]?").Groups[2].ToString();
                string connPassword = Regex.Match(connStr, "(Pwd=|Password=|PWD=){1}([\\w\\d\\.\\$]+)[;]?").Groups[2].ToString();
                reportDocument.DataSourceConnections[0].SetConnection(connServer, connDB, connUserID, connPassword);
                CrystalDecisions.Shared.ExportFormatType exportAs = CrystalDecisions.Shared.ExportFormatType.PortableDocFormat;

                reportDocument.ExportToHttpResponse(exportAs, System.Web.HttpContext.Current.Response, isAttachment, "Instadose_Invoice");
            }
            catch(Exception ex)
            {
                Response.Write(ex.Message);
            }
            finally
            {
                reportDocument.Close();
                reportDocument.Dispose();
            }
        }
    }
}