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


public partial class Instadose_InformationFinder_Details_ReturnList : System.Web.UI.Page
{
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
        for (int i = 0; i < this.gvReturnDetails.Rows.Count; i++)
        {
            GridViewRow gvRow = gvReturnDetails.Rows[i];
            CheckBox findChkBox = (CheckBox)gvRow.FindControl("cbx");

            if (findChkBox.Checked == true)
            {
                int returnID = int.Parse(gvRow.Cells[1].Text);

                string FileNameStr = "";
                // 1. Generate PDF file for selected RMA
                FileNameStr = GeneratePDF(returnID);

                // 2.Send PDF to Printer
                PrinterPrintPDF(FileNameStr);
            }
        }

        
        
   
    }


    private void PrinterPrintPDF(string fileName)
    {

        // Print document
       // string printerName = @"\\gdsbu\PRT_ACC01_4200N";
      

        Process proc = new Process();
        proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        proc.StartInfo.Verb = "print";

        fileName = fileName.Replace(",", "");
        string MapFileName = Server.MapPath(fileName);

        proc.StartInfo.FileName = @"C:\Program Files\Adobe\Reader 9.0\Reader\AcroRd32.exe";
        proc.StartInfo.Arguments = @"/p /h " + MapFileName;
        proc.StartInfo.UseShellExecute = false;
        proc.StartInfo.CreateNoWindow = true;

        proc.Start();

        proc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        if (proc.HasExited == false)
        {
            proc.WaitForExit(5000);
            //proc.Kill();
        }

        proc.EnableRaisingEvents = true;
        // AcroRd32.exe
        proc.CloseMainWindow();
        proc.Close();
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
         string myPDFfilePath = Server.MapPath("~/Instadose/Reports/" + myPDFfileName);
         string myCRFileNamePath = Server.MapPath("~/Instadose/InformationFinder/Details/RptRMALetter.rpt");

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

                 //zeus connection
                 cryRpt.DataSourceConnections[0].SetConnection("166722-SQLCLUS", "IRV_LCDISBUS", "netgrid", "Mirion2009");

                 //staging server connection
                 //cryRpt.DataSourceConnections[0].SetConnection("IRV-SAPL-INSTAD", "LCDISBUS", "drp", "drp1$");

                 cryRpt.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, myPDFfilePath);

                 rtnStr = "~/Instadose/Reports/" + myPDFfileName + ",";
                 
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
