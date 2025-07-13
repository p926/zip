using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Instadose;
using Instadose.Data;

public partial class DisplayDocuments : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        Guid id;

        // Ensure the GUID was passed.
        if (Request.QueryString["GUID"] == null) return;

        // Ensure the GUID is a proper Guid.
        if (!Guid.TryParse(Request.QueryString["GUID"], out id)) return;

        InsDataContext idc = new InsDataContext();
        Document document = (from d in idc.Documents
                             where d.DocumentGUID == id && d.Active == true 
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

        // Delete document after displaying it
        idc.Documents.DeleteOnSubmit(document);
        idc.SubmitChanges();

        Response.Flush();
        Response.End();
        
    }
}