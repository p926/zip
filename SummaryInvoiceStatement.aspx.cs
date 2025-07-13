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

public partial class Finance_SummaryInvoiceStatement : System.Web.UI.Page
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
        if (Request.QueryString["documentID"] == null)
            return;

        int docID = int.Parse(Request.QueryString["documentID"]);

        InsDataContext idc = new InsDataContext();
        Document document = (from d in idc.Documents
                             where d.DocumentID == docID && d.Active == true
                             select d).FirstOrDefault();

        // Ensure the document does exist.
        if (document == null) return;

        // Clear everything out.
        Response.Clear();
        Response.ClearHeaders();

        // Set the response headers.
        Response.ContentType = document.MIMEType;

        //Content-Disposition: attachment; filename=<file name.ext>. 
        Response.AddHeader("Content-Disposition", "attachment; filename=" + document.FileName);

        // Write the file to the response.
        Response.BinaryWrite(document.DocumentContent.ToArray());

        //Then delete the document 
        idc.Documents.DeleteOnSubmit(document);
        idc.SubmitChanges();

        Response.Flush();
        Response.End();        
    }    
}