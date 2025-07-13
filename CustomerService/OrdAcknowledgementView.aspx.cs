using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Instadose;
using Instadose.Data;
using System.Text.RegularExpressions;

public partial class InstaDose_CustomerService_OrdAcknowledgementView : System.Web.UI.Page
{
    int DocumentID;
    // Create the database reference
    InsDataContext idc = new InsDataContext();

    protected void Page_Load(object sender, EventArgs e)
    {
        // is UploadID?
        if (Request.QueryString["ID"] == null)
            return;

        // Grab the UploadID if it was passed it the query string
        if (Request.QueryString["ID"] != null)
            int.TryParse(Request.QueryString["ID"], out DocumentID);

        // Find the Upload ID
        if (checkExistence(DocumentID) == false)
        {
            Response.Write("The page could not be loaded because the FileID is invalid.");
            return;
        }

        // display attachment
        Display_AttachmentFile(DocumentID);

    }

    /// <summary>
    /// Check if UploadID exist in Document table
    /// </summary>
    /// <param name="ID">DocumentID </param>
    /// <returns>True/False</returns>
    private Boolean checkExistence(int ID)
    {
        bool rBool = false;
        //var rsAttach = (from rs in idc.OrderAcknowledgements 
        //                where rs.OrderAckID == ID
        //                select rs).ToList();

        var rsAttach = (from d in idc.Documents
                        where d.DocumentID == ID
                        select d).ToList();

        if (rsAttach.Count != 0)
            rBool = true;

        return rBool;

    }


    /// <summary>
    /// Display Attachment, accept DOC/DOCX, XLS/XLSX, PDF or JPG formats.
    /// </summary>
    /// <param name="ID">DocumentID</param>
    private void Display_AttachmentFile(int ID)
    {
        if ((ID.ToString() != "") && (ID != 0))
        {
           
            // Retrieve data from DB 
            Document rsAttach = (from d in idc.Documents 
                                       where d.DocumentID  == ID
                                       select d).First();
            
            //OrderAcknowledgement rsAttach = (from rs in idc.OrderAcknowledgements
            //                                 where rs.OrderAckID == ID
            //                                 select rs).First();

            // assign value
            //string strExtension = rsAttach.AckFileExtension .ToString();
            
            string mimeType = rsAttach.MIMEType;

            string strFilename = rsAttach.FileName.ToString();
            Byte[] bytFile = (System.Byte[])rsAttach.DocumentContent.ToArray();

            Response.ContentType = mimeType;

            switch (mimeType)
            {
                case  "application/vnd.ms-word":
                    Response.ContentType = "application/vnd.ms-word";
                    Response.AddHeader("content-disposition", "attachment;filename=" + strFilename);
                    break;
                case "application/vnd.ms-excel":
                    Response.ContentType = "application/vnd.ms-excel";
                    Response.AddHeader("content-disposition", "attachment;filename=" + strFilename);
                    break;
                case  "application/pdf":
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("content-disposition", "attachment;filename=" + strFilename);
                    break;
                default:
                    Response.ContentType = "image/jpeg";
                    break;
            }

            //switch (strExtension.ToUpper())
            //{
            //    case "DOC":
            //        Response.ContentType = "application/vnd.ms-word";
            //        Response.AddHeader("content-disposition", "attachment;filename=" + strFilename);
            //        break;
            //    case "DOCX":
            //        Response.ContentType = "application/vnd.ms-word";
            //        Response.AddHeader("content-disposition", "attachment;filename=" + strFilename);
            //        break;
            //    case "XLS":
            //        Response.ContentType = "application/vnd.ms-excel";
            //        Response.AddHeader("content-disposition", "attachment;filename=" + strFilename);
            //        break;
            //    case "XLSX":
            //        Response.ContentType = "application/vnd.ms-excel";
            //        Response.AddHeader("content-disposition", "attachment;filename=" + strFilename);
            //        break;
            //    case "PDF":
            //        Response.ContentType = "application/pdf";
            //        Response.AddHeader("content-disposition", "attachment;filename=" + strFilename);
            //        break;
            //    default:
            //        Response.ContentType = "image/jpeg";
            //        break;
            //}

            // display Attachment file
            Response.Charset = "";
            Response.Cache.SetCacheability(HttpCacheability.NoCache);

            Response.Expires = 0;
            Response.Buffer = true;
            Response.Clear();
            Response.BinaryWrite(bytFile);
            Response.End();
        }

    }
}
