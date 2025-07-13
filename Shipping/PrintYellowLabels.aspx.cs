using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;

using Instadose;
using Instadose.Data;
using Instadose.Security;

using System.IO;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

using System.Collections.Generic;
using iTextSharp.text;
using iTextSharp.text.pdf;


public partial class Instadose_Shipping_PrintYellowLabels : System.Web.UI.Page
{    
    
    // Create the database reference
    InsDataContext idc = null;

    // declare variable
    ReportDocument crDoc;

    // Create data table for devices
    DataTable dtDataTable = null;

    protected void Page_Load(object sender, EventArgs e)
    {
        this.idc = new InsDataContext();
    }

    protected void btnBack_Click(object sender, EventArgs e)
    {
        // Hide the second panel and display the primary panel.
        this.plAdd.Visible = true;
        this.plReview.Visible = false;
    }

    protected void btnContinue_Click(object sender, EventArgs e)
    {
        // Hide the first panel and display the secondary panel.
        this.plAdd.Visible = false;
        this.plReview.Visible = true;
        this.pnlErrorMessages.Visible = false;
        this.pnlSuccessMessages.Visible = false;

        // Store the list of serial numbers that the user has added.
        List<string> NumberList = new List<string>();
        NumberList.AddRange(this.txtSerialNos.Text.Split('\n'));
        
        // Load the serial number data
        this.LoadDataTable(NumberList);

        // Render the grid.
        this.gvReview.DataSource = dtDataTable;
        this.gvReview.DataBind();
    }

    private void LoadDataTable(List<string> orderNumbers)
    {
        // Store a list of error'd serials
        Dictionary<string, string> errorList = new Dictionary<string, string>();

        // DataTable to store the serial data
        dtDataTable = new DataTable();
        dtDataTable.Columns.Add("OrderNo", typeof(int));
        dtDataTable.Columns.Add("AccountNo", typeof(int));
        dtDataTable.Columns.Add("AccountName", typeof(string));
        dtDataTable.Columns.Add("UserName", typeof(string));
        dtDataTable.Columns.Add("Password", typeof(string));

        if (orderNumbers.Count > 0)
        {
            // Loop through each serial number.
            foreach (string OrderNumber in orderNumbers)
            {
                // Trim the order and make sure it's not empty.
                if (OrderNumber.Trim() == "") continue;
                
                int orderNo = 0;
                int.TryParse(OrderNumber.Trim(), out orderNo);

                // Find the return device
                Order order = idc.Orders.Where(o => o.OrderID == orderNo && o.OrderStatusID != 10).FirstOrDefault();

                // Ensure the order was found.
                if (order == null)
                {
                    errorList.Add(orderNo.ToString(), "The order was not found.");
                    continue;
                }

                // Ensure an account administrator exists.
                if (order.Account.Administrator == null)
                {
                    errorList.Add(orderNo.ToString(), "This account does not have an administrator set.");
                    continue;
                }

                // Create a data row
                DataRow dr = dtDataTable.NewRow();
                dr["OrderNo"] = order.OrderID;
                dr["AccountNo"] = order.AccountID;
                dr["AccountName"] = order.Account.AccountName;
                dr["UserName"] = order.Account.Administrator.UserName;
                dr["Password"] = "Cannot Decrypt";

                // Add the row to the DataTable.
                dtDataTable.Rows.Add(dr);
            }
        }

        string errorText = "";
        foreach (KeyValuePair<string, string> error in errorList)
            errorText += string.Format("<li>Order {0}: {1}</li>", error.Key, error.Value);

        // Display the error message if errors exist.
        if (errorList.Count > 0)
        {
            this.lblErrorReview.Text = "<ul>" + errorText + "</ul>";
            this.pnlErrorMessages.Visible = true;
        }
    }

    protected void btnPrintGroup_Click(object sender, EventArgs e)
    {
        if (gvReview.Rows.Count == 0) return;

        // Error list
        Dictionary<int, string> errorList = new Dictionary<int, string>();
        List<Stream> streams = new List<Stream>();

        // Loop through the rows
        for (int i = 0; i < gvReview.Rows.Count; i++)
        {
            // declare report parameters dimension
            GridViewRow row = gvReview.Rows[i];

            // Get the order ID, if it doesn't exist, skip to the next row.
            int orderID = 0;
            if (!int.TryParse(row.Cells[0].Text, out orderID)) continue;

            // Select the order record.
            var order = idc.Orders.Where(o => o.OrderID == orderID && o.OrderStatusID != 10).FirstOrDefault();

            if (order == null)
            {
                errorList.Add(orderID, "The order could not be found.");
                continue;
            }
                                
            try
            {
                crDoc = new ReportDocument();
                crDoc.Load(Server.MapPath("~/Shipping/YellowLabel.rpt"));

                crDoc.SetParameterValue(0, order.AccountID);
                crDoc.SetParameterValue(1, order.Account.AccountName);
                crDoc.SetParameterValue(2, order.Account.Administrator.UserName);
                crDoc.SetParameterValue(3, "Password Not Found");

                streams.Add(crDoc.ExportToStream(ExportFormatType.PortableDocFormat));
            }
            catch (Exception ex)
            {
                errorList.Add(orderID, "Error creating label: " + ex.Message);
            }
        }


        // Combine the PDFs and put it into a stream
        MemoryStream stream = CombineMultiplePDFs(streams);

        Response.Clear();
        Response.Buffer = true;
        Response.ContentType = "application/pdf";
        Response.AddHeader("Content-Disposition", "inline");

        Response.OutputStream.Write(stream.GetBuffer(), 0, stream.GetBuffer().Length);
        Response.OutputStream.Flush();
        Response.OutputStream.Close();

        stream.Close();
    }

    public MemoryStream CombineMultiplePDFs(List<Stream> streams)
    {
        int pageOffset = 0;
        int f = 0;
        ArrayList master = new ArrayList();
        iTextSharp.text.Document document = null;
        PdfCopy writer = null;

        MemoryStream outStream = new MemoryStream();

        foreach(Stream stream in streams)
        {
            // we create a reader for a certain document
            PdfReader reader = new PdfReader(stream);
            reader.ConsolidateNamedDestinations();
            // we retrieve the total number of pages
            int n = reader.NumberOfPages;
            pageOffset += n;
            if (f == 0)
            {
                // step 1: creation of a document-object
                document = new iTextSharp.text.Document(reader.GetPageSizeWithRotation(1));

                // step 2: we create a writer that listens to the document
                writer = new PdfCopy(document, outStream);// new FileStream(outFile, FileMode.Create));

                // step 3: we open the document
                document.Open();
            }
            // step 4: we add content
            for (int i = 1; i <= n; i++)
            {
                if (writer != null)
                {
                    PdfImportedPage page = writer.GetImportedPage(reader, i);
                    writer.AddPage(page);
                }
            }
            PRAcroForm form = reader.AcroForm;
            if (form != null && writer != null)
            {
                writer.CopyAcroForm(reader);
            }
            f++;
        }
        // step 5: we close the document
        if (document != null) document.Close();

        return outStream;
    }
}
