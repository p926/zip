using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Instadose;
using Instadose.Data;
using System.Text.RegularExpressions;
using System.IO;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Diagnostics;


public partial class Instadose_InformationFinder_Details_ViewAllRmaLetter : System.Web.UI.Page
{
    // Turn on/off development server and Live server
    bool DevelopmentServer = false;


    int AccountID;
    int ReturnID;
    // String to hold the current username
    string UserName = "Unknown";


    protected void Page_Load(object sender, EventArgs e)
    {
        Page.Title = "InstaDose Return/RMA - All Return List";
        // Try to set the username
        try
        {
            this.UserName = User.Identity.Name.Split('\\')[1];
        }
        catch { this.UserName = "Unknown"; }
    }

    protected void btnPrint_Click(object sender, EventArgs e)
    {
        // 1. Generate PDF for each selected RMA
        int gvRowCount = gvReturnDetails.Rows.Count;
        string FileNameStr = "";
        int FileGenerateCount = 0;
        for (int i = 0; i < gvRowCount; i++)
        {
            GridViewRow gvRow = gvReturnDetails.Rows[i];
            CheckBox findChkBox = (CheckBox)gvRow.FindControl("cbx");

            if (findChkBox.Checked == true)
            {
                int returnID = int.Parse(gvRow.Cells[1].Text);

                FileNameStr += GeneratePDF(returnID) + ",";
                FileGenerateCount += 1;
            }
        }

        // 2. Merge File into ONE pdf
        string DisplayFileName;
        if (FileGenerateCount > 1)
        {
           DisplayFileName = MergeAllPDF(FileNameStr);
        }
        else
        {  
            //display the only generated file
            DisplayFileName = FileNameStr.Replace(",","");
        }

        // viewing generated PDF file

        if (DevelopmentServer == false)
        {
            string myURL = "http://" + Request.Url.Authority.ToString() + "/INStadose-v2/Reports/" + DisplayFileName;
            Response.Redirect(myURL);
        }
        else
        {
            string myURL = "http://" + Request.Url.Authority.ToString() + "/wwwroot/Instadose/Reports/" + DisplayFileName;
            Response.Redirect(myURL);
        }
   
    }
    private string  MergeAllPDF(string FileNameStr)
    {
        string FileDateTime = DateTime.Now.Year.ToString() + DateTime.Now.Month.ToString() +  DateTime.Now.Day.ToString () + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString ();
        string MergeFileName = "RMA_MergeFile" + FileDateTime + ".pdf";
        string MergefilePath = Server.MapPath("~/Reports/" + MergeFileName);
        
        string[] fileName = FileNameStr.Split(',');

        int fileCount = fileName.Count() -1;
        int totalNumberOfPage = 0;

        if (fileCount > 0)
        {
            int fileArrayNo = 0 ; 
                   
            //start to read the 1st file index[0]
            PdfReader reader = new PdfReader(Server.MapPath("~/Reports/" + fileName[0]));
            int numberOfPage = reader.NumberOfPages;

            //creation of document object
            iTextSharp.text.Document doc = new iTextSharp.text.Document(reader.GetPageSizeWithRotation(1));

            //writer that listen to the document
            PdfWriter writer = PdfWriter.GetInstance(doc, new FileStream(MergefilePath, FileMode.Create));

            //open file[0]
            doc.Open();

            PdfContentByte cb = writer.DirectContent;
            PdfImportedPage page;

            int rotation;
            totalNumberOfPage += numberOfPage;

            // Add content, process all other files in string
            while (fileArrayNo < fileCount)
            {
                int i = 0;
                while (i < numberOfPage)
                {
                    i += 1;
                    doc.SetPageSize(reader.GetPageSizeWithRotation(i));
                    doc.NewPage();

                    // write/append file[i] content to file[0]
                    page = writer.GetImportedPage(reader, i);
                    rotation = reader.GetPageRotation(i);

                    if (rotation == 90 || rotation == 270)
                    {
                        cb.AddTemplate(page, 0, -1.0F, 1.0F, 0, 0, reader.GetPageSizeWithRotation(i).Height);
                    }
                    else
                        cb.AddTemplate(page, 1.0F, 0, 0, 1.0F, 0, 0);

                }

                fileArrayNo += 1;
                if (fileArrayNo < fileCount)
                {
                    reader = new PdfReader(Server.MapPath("~/Reports/" + fileName[fileArrayNo]));
                    numberOfPage = reader.NumberOfPages;
                    totalNumberOfPage += numberOfPage;

                }
            }
            doc.Close();

        }

        return (MergeFileName);
    }


    /// <summary>
    /// Generate PDF file  
    /// </summary>
    /// <param name="ID">RMA ID</param>
    /// <returns></returns>
    private string GeneratePDF(int ID)
     {
         string rtnStr = "";

         ReportDocument cryRpt;

         string[] rptParamName = new string[2];
         string[] rptParamVals = new string[2];

         string myRMA = ID.ToString();

         string myPDFfileName = "RMA_" + myRMA + ".pdf";
         string myPDFfilePath = Server.MapPath("~/Reports/" + myPDFfileName);
         string myCRFileNamePath = Server.MapPath("~/InformationFinder/Details/RptRMALetter.rpt");

         if (File.Exists(myPDFfilePath) == true)
         {
             File.Delete(myPDFfilePath);
         }

         if (File.Exists(myPDFfilePath) == false && myRMA != "")
         {
             try
             {
                 cryRpt = new ReportDocument();
                 cryRpt.Load(myCRFileNamePath);

                 cryRpt.SetParameterValue(0, myRMA);
                 cryRpt.SetParameterValue(1, "0");

                 if (DevelopmentServer == false)
                 {
                     //zeus connection
                     cryRpt.DataSourceConnections[0].SetConnection("166722-SQLCLUS", "IRV_LCDISBUS", "netgrid", "Mirion2009");
                 }
                 else
                 { 
                     //staging server connection
                     cryRpt.DataSourceConnections[0].SetConnection("IRV-SAPL-INSTAD", "LCDISBUS", "drp", "drp1$");
                 }
                 
                 cryRpt.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, myPDFfilePath);

                 //rtnStr = "~/Reports/" + myPDFfileName + ",";
                 rtnStr =  myPDFfileName ;
                 
             }

             catch (Exception e)
             {
                 Response.Write(e);
             }
         }
         return rtnStr;

     }// END generate PDF


    /// <summary>
    /// Display max 5 records of Serial No
    /// put "..." at the end of string if more than 5 serial# 
    /// </summary>
    /// <param name="SerialNoString"></param>
    /// <returns> Serial# String </returns>
    public string FuncTrimSerialNo(string SerialNoString)
    {
        string myReturn = SerialNoString;

        if (SerialNoString.LastIndexOf(",") >= 25)
        {
            int Endplace = SerialNoString.IndexOf(",", 20);
            myReturn = SerialNoString.Substring(0, Endplace) + "...";

        }
        return myReturn;

    }
   
}
