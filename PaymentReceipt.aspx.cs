using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;
using System.IO;

using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

using Instadose;
using Instadose.Data;
using Instadose.Processing;

public partial class Finance_PaymentReceipt : System.Web.UI.Page
{
    private const string DOCUMENT_CATEGORY = "Payment Receipt";
    InsDataContext idc;

    protected void Page_Load(object sender, EventArgs e)
    {
        int paymentID = 0;

        // Grab the ID & Credit Card Number if it was passed it the Query String.
        if (Request.QueryString["ID"] != null)
            int.TryParse(Request.QueryString["ID"], out paymentID);

        if (paymentID == 0)
        {
            Response.Write("Payment ID was not supplied.");
            return;
        }

        idc = new InsDataContext();

        // Get AccountID based on PaymentID (Query String value).
        var payment = (from p in idc.Payments
                       where p.PaymentID == paymentID
                       select new
                       {
                           AccountID = p.Order.AccountID,
                           OrderID = p.OrderID,
                           EncCreditCardNum = p.AccountCreditCard.NumberEncrypted,
                           CardType = p.AccountCreditCard.CreditCardType.CreditCardName
                       }).FirstOrDefault();

        if (payment == null) throw new Exception("Payment record was not found.");


        if (payment.EncCreditCardNum == null || payment.CardType == null)
            throw new Exception("Payment is not a credit card payment type.");

        // Mask the credit card number.
        string maskedCardNum = Common.MaskCreditCardNumber(payment.EncCreditCardNum, payment.CardType);

        // Generate the PDF.
        bool shouldDownload = generatePDF(paymentID, maskedCardNum, payment.AccountID, (payment.OrderID.HasValue) ? payment.OrderID.Value : 0);


        if (shouldDownload)
        {
            // Download the PDF receipt to the screen.
            downloadDocument(paymentID);
        }
    }

    private bool generatePDF(int paymentid, string maskedcreditcardnumber, int accountid, int orderid)
    {
        string pdfFileName = "PaymentReceipt_" + paymentid.ToString() + ".pdf";
        string myCRFileNamePath = Server.MapPath("PaymentReceipt.rpt");
        Stream oStream = null;
        ReportDocument cryRpt = new ReportDocument();
        bool success = false;

        // Get UserName (based on System Login).
        string userName = "Unknown";

        if (Page.User.Identity.Name.Split('\\')[1] != null)
            userName = Page.User.Identity.Name.Split('\\')[1];

        try
        {
            cryRpt.Load(myCRFileNamePath);

            cryRpt.SetParameterValue("@pPaymentID", paymentid);
            cryRpt.SetParameterValue("@pMaskedCreditCardNumber", maskedcreditcardnumber);

            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString.ToString();
            string myDatabase = Regex.Match(connStr, "(Initial Catalog=|Database=){1}([A-Za-z0-9_]+)[;]?").Groups[2].ToString();
            string myServer = Regex.Match(connStr, "(Data Source=){1}([\\w\\d\\.\\$\\-]+)[;]?").Groups[2].ToString();
            string myUserID = Regex.Match(connStr, "(User ID=|UID=){1}([\\w\\d\\.]+)[;]?").Groups[2].ToString();
            string myPassword = Regex.Match(connStr, "(Pwd=|Password=|PWD=){1}([\\w\\d\\.\\$]+)[;]?").Groups[2].ToString();

            cryRpt.DataSourceConnections[0].SetConnection(myServer, myDatabase, myUserID, myPassword);

            // Export to Memory Stream.        
            oStream = cryRpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);

            byte[] streamArray = new byte[oStream.Length];
            oStream.Read(streamArray,0,Convert.ToInt32(oStream.Length-1));

            // 12/09/2014, Tdo
            // Update inserted doc with real DocumentContent
            // Look for existing Document.
            Document doc = (from d in idc.Documents where d.ReferenceKey == paymentid select d).FirstOrDefault();

            if (doc != null)
            {
                doc.DocumentContent = streamArray;
                // Save changes.
                idc.SubmitChanges();

                success = true;

                // Report the error to the message system.
                Basics.WriteLogEntry("PaymentReceipt was generated successful.", Page.User.Identity.Name, "Portal.InstaDose_Finance_PaymentReceipt", Basics.MessageLogType.Information);
            }
            else
            {
                success = false;

                // Report the error to the message system.
                Basics.WriteLogEntry("PaymentReceipt was not generated successful.", Page.User.Identity.Name, "Portal.InstaDose_Finance_PaymentReceipt", Basics.MessageLogType.Critical);
            }

            //// Insert Document record into database.
            //Document dcmntrcrd = new Document()
            //{
            //    DocumentGUID = Guid.NewGuid(),
            //    FileName = pdfFileName,
            //    DocumentContent = streamArray,
            //    MIMEType = "application/pdf",
            //    DocumentCategory = DOCUMENT_CATEGORY,
            //    Description = "Credit Card Payment Receipt",
            //    AccountID = accountid,
            //    OrderID = orderid,
            //    CreatedBy = userName,
            //    CreatedDate = DateTime.Now,
            //    Active = true,
            //    ReferenceKey = paymentid
            //};

            //// Submit record to database.
            //idc.Documents.InsertOnSubmit(dcmntrcrd);
            //idc.SubmitChanges();

            //// Was the process completed?
            //success = true;

        }
        catch (Exception ex)
        {

            // Report the error to the message system.
            Basics.WriteLogEntry(ex.Message, Page.User.Identity.Name, "Portal.InstaDose_Finance_PaymentReceipt", Basics.MessageLogType.Critical);

            Response.Write(ex);

            success = false;
        }
        finally
        {
            cryRpt.Close();
            cryRpt.Dispose();

            oStream.Flush();
            oStream.Close();
            oStream.Dispose();
        }

        return success;
    }

    private void downloadDocument(int paymentID)
    {
        // Query the document.
        Document document = (from d in idc.Documents
                             where d.ReferenceKey == paymentID &&
                                   d.DocumentCategory == DOCUMENT_CATEGORY
                             select d).FirstOrDefault();

        // Write the document to the screen.
        Response.Clear();
        Response.Buffer = true;
        Response.ContentType = document.MIMEType;
        Response.BinaryWrite(document.DocumentContent.ToArray());
        Response.End();
    }

}