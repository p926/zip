using Mirion.DSD.GDS.API.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace portal_instadose_com_v3.InformationFinder.Details
{
    public partial class AXViewInvoice : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var invoice = Request.QueryString["Invoice"];

            if (string.IsNullOrEmpty(invoice))
                return;

            AXInvoiceRequests axInvoiceRequests = new AXInvoiceRequests();

            var axInvoice = axInvoiceRequests.GetInvoice(invoice);

            if (string.IsNullOrEmpty(axInvoice.FilePath))
                CloseWindow();

            string filePath = axInvoice.FilePath.Replace("\\\\", "\\");
            filePath = "\\" + filePath;

            WebClient client = new WebClient();

            Byte[] buffer = client.DownloadData(filePath);

            if (buffer == null)
                CloseWindow();

            Response.ContentType = "application/pdf";
            Response.AddHeader("content-length", buffer.Length.ToString());
            Response.BinaryWrite(buffer);
        }

        private void CloseWindow()
        {
            Page.ClientScript.RegisterStartupScript(GetType(), "openEmptyWarning", "openEmptyWarning();", true);

        }
    }
}