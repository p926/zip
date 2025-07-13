using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Instadose;
using Instadose.Data;
using System.Text.RegularExpressions;

public partial class InstaDose_CustomerService_PORequestView : System.Web.UI.Page
{
    int OrderID;
    int OriginalOrderID;
    int AccountID;

    // Create the database reference
    InsDataContext idc = new InsDataContext();

    protected void Page_Load(object sender, EventArgs e)
    {
        // Good OrderID?
        if (Request.QueryString["ID"] == null)
            return;

        // Grab the UploadID if it was passed it the query string
        if (Request.QueryString["ID"] != null)
            int.TryParse(Request.QueryString["ID"], out OrderID);
            
        //get AccountID for PO form display.
        AccountID = getAccountID(OrderID);

        // Find the Upload ID
        if (checkExistence(OrderID, AccountID) == false)
        {
            this.btnBack.Visible = true;
            Response.Write("The page could not be loaded because no PO attachment on file.");
            return;
        }

        // Display current OrderID attachment
        Display_AttachmentFile(OrderID, AccountID);      
    }

    /// <summary>
    /// getOriginalOrderID
    /// </summary>
    /// <param name="ordID"></param>
    /// <returns></returns>
    private int getOriginalOrderID(int ordID)
    {
        int originalOrderID = 0;
        int accountID = 0;

        // Grab the originial order 
        Order ord = (from o in idc.Orders
                     where o.OrderID == ordID
                     select o).FirstOrDefault();

        if (ord != null) accountID = ord.AccountID;

        var order = (from o in idc.Orders
                     where o.AccountID == accountID
                     orderby o.OrderID
                     select o).First();

        if (order != null) originalOrderID = order.OrderID;

        return originalOrderID;
    }

    /// <summary>
    /// getOriginalOrderID
    /// </summary>
    /// <param name="ordID"></param>
    /// <returns></returns>
    private int getAccountID(int ordID)
    {
        int accountID = 0;

        // Grab the originial order 
        Order ord = (from o in idc.Orders
                     where o.OrderID == ordID
                     select o).FirstOrDefault();

        if (ord != null) accountID = ord.AccountID;
        
        return accountID;
    }

    /// <summary>
    /// Check if UploadID exist in DB table rma_uploadFile
    /// </summary>
    /// <param name="ID"> UploadID </param>
    /// <returns>True/False</returns>
    private Boolean checkExistence(int originalOrderID, int accountID)
    {
        bool rBool = false;
 
        var rsAttach = (from d in idc.Documents
                        where d.AccountID == accountID &&
                              d.OrderID == originalOrderID
                        select d).ToList();

        if (rsAttach.Count != 0)
            rBool = true;

        return rBool;
        
    }


    /// <summary>
    /// Display Attachment, accept DOC/DOCX, XLS/XLSX, PDF or JPG formats.
    /// </summary>
    /// <param name="ID">Upload ID</param>
    private void Display_AttachmentFile(int orderID, int accountID)
    {
        if ((orderID.ToString() != "") && (orderID != 0))
        {
  
            Document rsAttach = (from d in idc.Documents
                            where 
                                  d.OrderID == orderID &&
                                  d.DocumentCategory == "PO Request"
                                  //d.Description == "Original PO Request Document Upload"
                            select d).FirstOrDefault();

            if (rsAttach == null)
                return;

            string mimeType = rsAttach.MIMEType;

            string strFilename = rsAttach.FileName.ToString();
            Byte[] bytFile = (System.Byte[])rsAttach.DocumentContent.ToArray();

            Response.ContentType = mimeType;

            switch (mimeType)
            {
                case "application/vnd.ms-word":
                    Response.ContentType = "application/vnd.ms-word";
                    Response.AddHeader("content-disposition", "attachment;filename=" + strFilename);
                    break;
                case "application/vnd.ms-excel":
                    Response.ContentType = "application/vnd.ms-excel";
                    Response.AddHeader("content-disposition", "attachment;filename=" + strFilename);
                    break;
                case "application/pdf":
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("content-disposition", "attachment;filename=" + strFilename);
                    break;
                default:
                    Response.ContentType = "image/jpeg";
                    break;
            }
            
            // display Attachment file
            Response.Charset = "";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);

            Response.Expires = 0;
            Response.Buffer = true;
            Response.Clear();
            Response.BinaryWrite(bytFile);

            //btnBack.Visible = true;
            //Response.Write("Creating PDF file.");
  
            Response.Flush();
            Response.End();
       }

        this.btnBack.Visible = true;
    }

    protected void btnBack_Click(object sender, EventArgs e)
    {
        // Reload the page using a redirect function.
        Response.Redirect("OrderQueue.aspx"); 
    }
}
