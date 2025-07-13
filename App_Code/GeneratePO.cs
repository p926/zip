using System;
using System.Web;
using System.Linq;
using Instadose;
using Instadose.Data;
using CrystalDecisions.CrystalReports.Engine;
using System.Text.RegularExpressions;
using System.IO;

/// <summary>
/// Summary description for GeneratePO
/// </summary>
public class GeneratePO
{
    /// <summary>
    /// Generate PO-request form, store PDF in database "OrderPORequest"
    /// </summary>
    /// <param name="orderID">Order ID</param>
    /// <returns>Returns a success or failure.</returns>
    public static bool GeneratePORequestForm(int orderID, string createdBy = "Unknown")
    {
        InsDataContext idc = new InsDataContext();
        ReportDocument cryRpt = new ReportDocument();
        Stream oStream = null;
        byte[] byteArray = null;
        bool completed = false;
        try
        {
            Order order = idc.Orders.Where(o => o.OrderID == orderID && o.OrderStatusID != 10).FirstOrDefault();
            if (order == null) throw new Exception("The order could not be found.");

            //Delete out all previously stored "Original" po request in Documents table
            //for this order. (if any)
            var poAttach = (from d in idc.Documents 
                            where d.OrderID == orderID && d.Description == "Original PO Request Document Upload"
                            select d).ToList();

            idc.Documents.DeleteAllOnSubmit(poAttach);
            idc.SubmitChanges();

            // Get the variables for a connection
            string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString.ToString();
            string database = Regex.Match(ConnectionString, "(Initial Catalog=|Database=){1}([A-Za-z0-9_]+)[;]?").Groups[2].ToString();
            string server = Regex.Match(ConnectionString, "(Data Source=){1}([\\w\\d\\.\\$\\-]+)[;]?").Groups[2].ToString();
            string userID = Regex.Match(ConnectionString, "(User ID=|UID=){1}([\\w\\d\\.]+)[;]?").Groups[2].ToString();
            string password = Regex.Match(ConnectionString, "(Pwd=|Password=|PWD=){1}([\\w\\d\\.\\$]+)[;]?").Groups[2].ToString();
            
            cryRpt.Load(HttpContext.Current.Server.MapPath("~/CustomerService/CSPoRequest_ICCare.rpt"));
            cryRpt.SetParameterValue(0, orderID.ToString());
            cryRpt.DataSourceConnections[0].SetConnection(server, database, userID, password);

            //Create PDF file and store in Memory
            oStream = cryRpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            byteArray = new byte[oStream.Length];
            oStream.Read(byteArray, 0, Convert.ToInt32(oStream.Length - 1));

            //***************************************************************************************
            // 9/2012 - New Document table.  Stored PDF file here instead of OrderPORequests.
            //              Implement for Instadose 2.  Save new PO attachment as Original.
            //***************************************************************************************

            Document doc = new Document()
            {
                Active = true,
                AccountID = order.AccountID,
                OrderID = orderID,
                CreatedBy = createdBy,
                CreatedDate = DateTime.Now,
                Description = "Original PO Request Document Upload",
                DocumentCategory = "PO Request",
                DocumentGUID = Guid.NewGuid(),
                FileName = string.Format("PO Request-{0}.pdf", orderID),
                DocumentContent = byteArray,
                MIMEType = "application/pdf",
            };

            // Get the current website environment
            string siteUrl = System.Configuration.ConfigurationManager.AppSettings["api_webaddress"];

            // Construct the download path for the user.
            string downloadUrl = string.Format("{0}Support/PublicDocument.aspx?GUID={1}&Ticks={2}",
                siteUrl, doc.DocumentGUID, doc.CreatedDate.Ticks);

            // Upload the document
            idc.Documents.InsertOnSubmit(doc);
            idc.SubmitChanges();

            //***************************************************************************************
            //       End of 9/2012 - New Document Table.  
            //**************************************************************************************
            

            completed=true;
        }
        catch (Exception ex)
        {
            Basics.WriteLogEntry(ex.Message, HttpContext.Current.User.Identity.Name,
                "GeneratePO.GeneratePORequest", Basics.MessageLogType.Critical);

            completed = false;
        }
        finally
        {
            cryRpt.Close();
            cryRpt.Dispose();
            oStream.Flush();
            oStream.Close();
            oStream.Dispose();
        }
        return completed;
    }
}