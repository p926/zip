using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Instadose;
using Instadose.Data;
using System.Text.RegularExpressions;

public partial class Instadose_InformationFinder_Details_Return_ViewAttachment : System.Web.UI.Page
{
    int UploadID;

    // Create the linq to the App database.
    AppDataContext adc = new AppDataContext();

    protected void Page_Load(object sender, EventArgs e)
    {
        // is UploadID?
        if (Request.QueryString["ID"] == null)
            return;

        // Grab the UploadID if it was passed it the query string
        if (Request.QueryString["ID"] != null)
            int.TryParse(Request.QueryString["ID"], out UploadID);

        // Find the Upload ID
        if (checkExistence(UploadID) == false)
        {
            Response.Write("The page could not be loaded because the FileID is invalid.");
            return;
        }

        // display attachment
        Display_AttachmentFile(UploadID);

    }

    /// <summary>
    /// Check if UploadID exist in DB table rma_uploadFile
    /// </summary>
    /// <param name="ID"> UploadID </param>
    /// <returns>True/False</returns>
    private Boolean checkExistence(int ID)
    {
        bool rBool = false;
        var rsAttach = (from rs in adc.rma_UploadFiles
                        where rs.UploadFileID == ID
                        select rs).ToList();

        if (rsAttach.Count != 0)
            rBool = true;

        return rBool;
    }

    /// <summary>
    /// Display Attachment, accept DOC/DOCX, XLS/XLSX, PDF or JPG formats.
    /// </summary>
    /// <param name="ID">Upload ID</param>
    private void Display_AttachmentFile(int ID)
    {
        if ((ID.ToString() != "") && (ID != 0))
        {
            // Retrieve data from DB 
            rma_UploadFile rsAttach = (from rs in adc.rma_UploadFiles
                                       where rs.UploadFileID == ID
                                       select rs).First();

            // assign value
            string strExtension = rsAttach.FileExtension.ToString();
            string strFilename = rsAttach.Filename.ToString();
            Byte[] bytFile = (System.Byte[])rsAttach.FileContent.ToArray();


            switch (strExtension.ToUpper())
            {
                case "DOC":
                    Response.ContentType = "application/vnd.ms-word";
                    Response.AddHeader("content-disposition", "attachment;filename=" + strFilename);
                    break;
                case "DOCX":
                    Response.ContentType = "application/vnd.ms-word";
                    Response.AddHeader("content-disposition", "attachment;filename=" + strFilename);
                    break;
                case "XLS":
                    Response.ContentType = "application/vnd.ms-excel";
                    Response.AddHeader("content-disposition", "attachment;filename=" + strFilename);
                    break;
                case "XLSX":
                    Response.ContentType = "application/vnd.ms-excel";
                    Response.AddHeader("content-disposition", "attachment;filename=" + strFilename);
                    break;
                case "PDF":
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
            Response.End();
        }

    }
}
