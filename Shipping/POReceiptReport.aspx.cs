using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Instadose;
using Instadose.Data;
using System.Text.RegularExpressions;
using System.IO;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Collections;
using System.Configuration;
using System.Data.SqlClient;

public partial class Shipping_POReceiptReport : System.Web.UI.Page
{
    ReportDocument cryRpt = new ReportDocument();

    protected void Page_Unload(object sender, EventArgs e)
    {
        try
        {
            cryRpt.Close();
            cryRpt.Dispose();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        catch { }
    }   

    protected void Page_Load(object sender, EventArgs e)
    {
        // Grab the ID if it was passed it the query string
        if (Request.QueryString["receiptID"] == null)
            return;

        int receiptID = int.Parse (Request.QueryString["receiptID"]);

        GeneratePDF(receiptID, "POReceiptReport.rpt");        
    }

    protected void GeneratePDF(int pReceiptID, string pCrystalReportFileName)
    {        
        string myCRFileNamePath = Server.MapPath(pCrystalReportFileName);
        Stream oStream = null;

        try
        {
            cryRpt = new ReportDocument();
            cryRpt.Load(myCRFileNamePath);

            cryRpt.SetDataSource(GetReportData(pReceiptID));

            //export to memory Stream            
            cryRpt.ExportToHttpResponse(ExportFormatType.PortableDocFormat, HttpContext.Current.Response, false, "po_receipt");
        }

        catch (Exception e)
        {
            Response.Write(e.Message);
        }
        finally
        { 
            cryRpt.Close();
            cryRpt.Dispose();
        }
    }

    private DataTable GetReportData(int pReceiptID)
    {
        try
        {
            String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            String cmdStr = "sp_GeneratePOReceiptRpt";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.Add("@ReceiptID", SqlDbType.Int);

            sqlCmd.Parameters["@ReceiptID"].Value = pReceiptID;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);

            sqlConn.Close();

            return dt;
        }
        catch { return null; }
    }
}