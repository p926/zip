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

using Instadose;
using Instadose.Data;
using System.IO;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;


public partial class InstaDose_InformationFinder_GenerateOrderAck : System.Web.UI.Page
{
    int orderID;
    ReportDocument cryRpt;
    
    // String to hold the current username
    string UserName = "Unknown";

    string orderAmount;
    string shippingAmount;
   

    InsDataContext idc = new InsDataContext();

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            UserName = User.Identity.Name.Split('\\')[1];
        }
        catch { UserName = "Unknown"; }

        try
        {
            if (!IsPostBack && Request.QueryString["ID"] != null)
            {
                if (!int.TryParse(Request.QueryString["ID"].ToString(), out orderID)) return;
                this.txtOrderNo.Text = Request.QueryString["ID"].ToString();
                
                txtOrderNo.Visible = false;
                cmdGenerate.Visible = false; 

                orderAmount = Request.QueryString["OrderTotal"];
                shippingAmount = Request.QueryString["ShippingTotal"];
                
                cmdGenerate_Click(sender, e);

            }
        }
        catch(Exception ex) {  }
    }

    protected void cmdGenerate_Click(object sender, EventArgs e)
    {
        // Grab the ID if it was passed it the query string
        if (Request.QueryString["ID"] != null)
            int.TryParse(Request.QueryString["ID"], out orderID);

        // Find the ID
        if (checkExistence(orderID) == false)
        {
            Response.Write("The page could not be loaded because the OrderID is invalid.");
            return;
        }

        //Generate and save Order Acknowledgement into Documents table.
        GenerateOrderAcknowledgement(orderID);

        // display attachment
        Display_OrderAcknowledgement(orderID, orderAmount, shippingAmount);   
    }

    /// <summary>
    /// Check if OrderID exist in DB 
    /// </summary>
    /// <param name="ID"> orderID </param>
    /// <returns>True/False</returns>
    private Boolean checkExistence(int ordID)
    {
        bool rBool = false;

        int rs = (from a in idc.Orders
                  where a.OrderID == ordID
                  select a).Count();

        if (rs != 0)
            rBool = true;

        return rBool;

    }


    /// <summary>
    /// Display OrderAcknowledgement 
    /// </summary>
    /// <param name="ID">OrderID</param>
    private void Display_OrderAcknowledgement(int ordID, string orderAmt, string shippingAmt)
    {
        //string[] rptParamName = new string[2];
        //string[] rptParamVals = new string[2];

        string strOrdID = ordID.ToString();

        string myCRFileNamePath = Server.MapPath("~/InformationFinder/GenerateOrderAck.rpt");
        
        try
        {
            cryRpt = new ReportDocument();
            cryRpt.Load(myCRFileNamePath);

            //***************************************************************************
            //10/18/12 W.Kakemoto - order and shipping totals now passed in due to 
            //   foreign orders.  No more "$" used.  Currency code is now used.
            //****************************************************************************
            cryRpt.SetParameterValue(0, strOrdID);
            //cryRpt.SetParameterValue(1, orderAmt);
            //cryRpt.SetParameterValue(2, shippingAmt);
            //cryRpt.SetParameterValue(1, "0");

            string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString.ToString();
            string myDatabase = Regex.Match(ConnectionString, "(Initial Catalog=|Database=){1}([A-Za-z0-9_]+)[;]?").Groups[2].ToString();
            string myServer = Regex.Match(ConnectionString, "(Data Source=){1}([\\w\\d\\.\\$\\-]+)[;]?").Groups[2].ToString();
            string myUserID = Regex.Match(ConnectionString, "(User ID=|UID=){1}([\\w\\d\\.]+)[;]?").Groups[2].ToString();
            string myPassword = Regex.Match(ConnectionString, "(Pwd=|Password=|PWD=){1}([\\w\\d\\.\\$]+)[;]?").Groups[2].ToString();

            cryRpt.DataSourceConnections[0].SetConnection(myServer, myDatabase, myUserID, myPassword);

            //export to memory Stream
            MemoryStream oStream = new MemoryStream();
            Stream stream = cryRpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            stream.CopyTo(oStream);
            Response.Clear();
            Response.Buffer = true;
            Response.ContentType = "application/pdf";
            Response.BinaryWrite(oStream.ToArray());
            Response.End();

        }

        catch (Exception e)
        {
            Response.Write(e);
        }
      
    }

    /// <summary>
    /// GenerateOrderAcknowledgement
    ///         Generate Order Ack document and store into Document table.
    /// </summary>
    /// <param name="ordID"></param>
    private void GenerateOrderAcknowledgement(int ordID)
    {
        string[] rptParamName = new string[1];
        string[] rptParamVals = new string[1];
        string pdfFileName = "Ack_" + ordID.ToString() + ".pdf";
        string crFileNamePath = Server.MapPath("~/InformationFinder/GenerateOrderAck.rpt");

        int accountID = 0;

        // Get AccountID for order. 
        Order ord = (from o in idc.Orders
                     where o.OrderID == ordID
                     select o).FirstOrDefault();

        if (ord != null) accountID = ord.AccountID;

        //var OrdAck = (from a in idc.OrderAcknowledgements
        //              where a.AckFileName == myPDFfileName
        //              && a.Active == false && a.AccountID == myAccountID
        //              select a).FirstOrDefault();

        //9/2013 WK.
        //Deactivate previous acknowledgements for this account, order.
        checkPreviousOrderAcknowledgements(accountID, ordID);

        // Check to see if original order acknowledgement document data exists for this order.  
        Document ordAck = (from d in idc.Documents
                           where d.AccountID == accountID 
                                && d.OrderID == ordID 
                                && d.Description == "Original Order Acknowledgement"
                                && d.FileName == pdfFileName   //.MIMEType == "application/pdf"
                                && d.Active
                           select d).FirstOrDefault();

        //Store only original Order Acknowledgement into Document table.
        if (ordAck == null)
        {
            cryRpt = new ReportDocument();
            cryRpt.Load(crFileNamePath);

            cryRpt.SetParameterValue(0, ordID);

            string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString.ToString();
            string myDatabase = Regex.Match(ConnectionString, "(Initial Catalog=|Database=){1}([A-Za-z0-9_]+)[;]?").Groups[2].ToString();
            string myServer = Regex.Match(ConnectionString, "(Data Source=){1}([\\w\\d\\.\\$\\-]+)[;]?").Groups[2].ToString();
            string myUserID = Regex.Match(ConnectionString, "(User ID=|UID=){1}([\\w\\d\\.]+)[;]?").Groups[2].ToString();
            string myPassword = Regex.Match(ConnectionString, "(Pwd=|Password=|PWD=){1}([\\w\\d\\.\\$]+)[;]?").Groups[2].ToString();

            cryRpt.DataSourceConnections[0].SetConnection(myServer, myDatabase, myUserID, myPassword);

            //Create PDF file and store in Memory
            Stream oStream;
            MemoryStream mStream = new MemoryStream();
            oStream = cryRpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            oStream.CopyTo(mStream);
            byte[] streamArray = mStream.ToArray();
            

            try
            {
                // 8/2012 - New Documents table for all attachments for email, etc.
                // insert record to document table.
                Document doc = new Document()
                {
                    Active = true,
                    AccountID = accountID,
                    OrderID = orderID,
                    CreatedBy = this.UserName,
                    CreatedDate = DateTime.Now,
                    Description = "Original Order Acknowledgement",
                    DocumentCategory = "Order Acknowledgement",
                    DocumentGUID = Guid.NewGuid(),
                    FileName = pdfFileName,
                    DocumentContent = streamArray,
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
     
            }
            catch (Exception ex)
            {
                // Report the error to the message system.
                Basics.WriteLogEntry(ex.Message, Page.User.Identity.Name,
                    "Portal.InstaDose_InformationFinder_GenerateOrderAck", Basics.MessageLogType.Critical);

                Response.Write(ex);
            }
            finally
            {
                cryRpt.Close();
                cryRpt.Dispose();
            }
        }
    }

    /// <summary>
    /// 9/2013 WK
    /// checkPreviousOrderAcknowledgements - check and deactivate all previous order 
    ///         accknwoledgements for given account, order.
    /// </summary>
    /// <param name="accountID"></param>
    /// <param name="orderID"></param>
    private void checkPreviousOrderAcknowledgements(int accountID, int orderID)
    {
        // Check to see if original order acknowledgement document data exists for this order.  
        int totalOrderAck = (from d in idc.Documents
                           where d.AccountID == accountID
                                && d.OrderID == orderID
                                && d.Description == "Original Order Acknowledgement"
                                && d.Active
                           select d.DocumentID).Count();

        if (totalOrderAck > 0)
        {
            // Check to see if original order acknowledgement document data exists for this order.  
            var ordAck = (from d in idc.Documents
                          where d.AccountID == accountID
                          && d.OrderID == orderID
                          && d.Description == "Original Order Acknowledgement"
                          && d.Active
                          select d);

            //deactivate all existing order acknowledgements for this accountID and orderID.
            foreach(var d in ordAck)
            {
                d.Active = false;
                idc.SubmitChanges();
            }

        }
    }
}
