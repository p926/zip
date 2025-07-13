using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using EvoPdf;
using System.Collections.Specialized;


/// <summary>
/// Generates a PDF from HTML using the standards put in place by Mirion.
/// </summary>
public class MirionPDFConverter : PdfConverter
{
    public MirionPDFConverter()
        : base()
	{
        this.LicenseKey = "Y+3+7P//7P/7+ez44vzs//3i/f7i9fX19Q==";
        this.PdfDocumentOptions.TopMargin = 36;
        this.PdfDocumentOptions.RightMargin = 36;
        this.PdfDocumentOptions.LeftMargin = 36;
        this.PdfDocumentOptions.BottomMargin = 36;
        this.PdfDocumentOptions.PdfPageSize = PdfPageSize.Letter;
        this.PdfDocumentOptions.PdfCompressionLevel = PdfCompressionLevel.Normal;
        this.PdfDocumentOptions.PdfPageOrientation = PdfPageOrientation.Portrait;
        this.PdfDocumentOptions.ShowHeader = false;
        this.PdfDocumentOptions.ShowFooter = false;
	}

    /// <summary>
    /// Write a PDF response to the screen, with a dispotiion to download it.
    /// </summary>
    /// <param name="reportName">Provide the report name. Example: Sample.aspx</param>
    /// <param name="queryString">Provide the key value pair for the querystring arguments.</param>
    /// <param name="fileName"></param>
    public void WritePdfResponse(string reportName, Dictionary<string, string> reportParams, string fileName = "file.pdf")
    {
        var p = reportParamsToQueryString(reportParams);

        // Generate the full url.
        // https://localhost/app/Reports/WebBased/DealerPORequests.aspx?OrderID=12354,12345
        string url = string.Format("{0}://{1}{2}/Reports/WebBased/{3}?{4}",
        HttpContext.Current.Request.Url.Scheme, HttpContext.Current.Request.Url.Authority,
            HttpContext.Current.Request.ApplicationPath.TrimEnd('/'), reportName, p);

        // Do the hard part- convert to PDF
        //byte[] pdfBytes = this.GetPdfBytesFromUrl(string.Format("~/Reports/WebBased/{0}?{1}", reportName, p));

        System.IO.StringWriter strw = new System.IO.StringWriter();
        HttpContext.Current.Server.Execute(string.Format("~/Reports/WebBased/{0}?{1}", reportName, p), strw);
        string htmlCode = strw.GetStringBuilder().ToString();
        strw.Close();

        byte[] pdfBytes = this.GetPdfBytesFromHtmlString(htmlCode, HttpContext.Current.Request.Url.AbsoluteUri);

        // add the Content-Type and Content-Disposition HTTP headers
        HttpContext.Current.Response.Clear();
        HttpContext.Current.Response.AddHeader("Content-Type", "application/pdf");
        HttpContext.Current.Response.AddHeader("Content-Disposition", 
            String.Format("attachment; filename={0}; size={1}", fileName, pdfBytes.Length));

        // write the PDF document bytes as attachment to HTTP response 
        HttpContext.Current.Response.BinaryWrite(pdfBytes);

        // Note: it is important to end the response, otherwise the ASP.NET
        // web page will render its content to PDF document stream
        HttpContext.Current.Response.End();
    }
    
    /// <summary>
    /// Converts a NameValueCollection into a partial query string.
    /// </summary>
    /// <param name="nvc"></param>
    /// <returns></returns>
    private string reportParamsToQueryString(Dictionary<string, string> reportParams)
    {
        if (reportParams == null) return "";
        string query = "";

        // Append the parameters to the end.
        foreach (var p in reportParams)
        {
            // Continue looping if the value is null or empty.
            if (String.IsNullOrEmpty(p.Key)) continue;

            // Build the string.
            query += String.Format("{0}={1}&", p.Key, p.Value);
        }
        if(query.Length <= 0) return "";

        return query.Substring(0, query.Length - 1);
    }
}