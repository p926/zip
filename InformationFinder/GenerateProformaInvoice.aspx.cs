using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.IO;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

public partial class InstaDose_InformationFinder_GenerateProformaInvoice : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            if (!IsPostBack && Request.QueryString["ID"] != null)
            {
                this.txtOrderNo.Text = Request.QueryString["ID"];
                cmdGenerate_Click(sender, e); // use Crystal Report
            }
        }
        catch { }
    }

    protected void cmdGenerate_Click(object sender, EventArgs e)
    {
        int orderNo = 0;
        int.TryParse(this.txtOrderNo.Text, out orderNo);

        // generate PDF file using Crystal Report
        if (orderNo > 0)
        {

            ReportDocument cryRpt;

            string myCRFileNamePath = Server.MapPath("~/InformationFinder/GenerateProformaInvoice.rpt");
            try
            {
                cryRpt = new ReportDocument();
                cryRpt.Load(myCRFileNamePath);

                cryRpt.SetParameterValue(0, orderNo.ToString());

                string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString.ToString();
                string myDatabase = Regex.Match(ConnectionString, "(Initial Catalog=|Database=){1}([A-Za-z0-9_]+)[;]?").Groups[2].ToString();
                string myServer = Regex.Match(ConnectionString, "(Data Source=){1}([\\w\\d\\.\\$\\-]+)[;]?").Groups[2].ToString();
                string myUserID = Regex.Match(ConnectionString, "(User ID=|UID=){1}([\\w\\d\\.]+)[;]?").Groups[2].ToString();
                string myPassword = Regex.Match(ConnectionString, "(Pwd=|Password=|PWD=){1}([\\w\\d\\.\\$]+)[;]?").Groups[2].ToString();

                cryRpt.DataSourceConnections[0].SetConnection(myServer, myDatabase, myUserID, myPassword);

                //export to memory Stream
                Stream oStream = null;
                oStream = cryRpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                byte[] streamArray = new byte[oStream.Length];
                oStream.Read(streamArray, 0, Convert.ToInt32(oStream.Length - 1));
                Response.Clear();
                Response.Buffer = true;
                Response.ContentType = "application/pdf";
                Response.BinaryWrite(streamArray);
                Response.End();
                //this.lblResponse.Text = "The invoice has been created: <a target='_blank' href='ProformaInvoices/" + myPDFfileName + "'>" + myPDFfileName + "</a>";
            }

            catch (Exception ex)
            {
                this.lblResponse.Text += " The invoice was not generated.<br/>";
                Response.Write(ex);
            }
        }

    }
}