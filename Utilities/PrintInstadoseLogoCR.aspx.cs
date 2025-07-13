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

public partial class Admin_Utilities_PrintInstadoseLogoCR : System.Web.UI.Page
{
    // declare variable
    ReportDocument cryRpt = new ReportDocument();

    protected void Page_Load(object sender, EventArgs e)
    {

    }
    protected void Page_Unload(object sender, EventArgs e)
    {
        try
        {
            cryRpt.Close();
            cryRpt.Dispose();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        catch { }
    }   

    protected void btn_print_Click(object sender, EventArgs e)
    {
        //// declare report parameters dimension  
        //string[] rptParamName = new string[4];
        //string[] rptParamVals = new string[4];

        //// Create the database references
        //this.bdc = new BusDataContext();
        //int orderId = 0;
        //int.TryParse(this.txtInOrderNo.Text.ToString(), out orderId);
        //// Find the return device
        //var userInfo = (from O1 in bdc.Orders
        //                join A1 in bdc.Accounts
        //                 on O1.Accountid equals A1.AccountID
        //                join U1 in bdc.Users
        //                 on A1.userID equals U1.UserID
        //                where O1.OrderId == orderId
        //                select U1).First();

        //string MyorderId = orderId.ToString().Trim();
        //string AcctIdStr = userInfo.UserID.ToString().Trim();
        //string AcctNameStr = userInfo.FirstName.ToString().Trim() + " " + userInfo.LastName.ToString().Trim();
        //string UserNameStr = userInfo.UserName.ToString().Trim();
        //string PasswordStr = Instadose.Security.TripleDES.Decrypt(userInfo.Password.ToString());

        string myFileID = DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
        // prepare report
        string myPDFfileName = "LblOrder_" + myFileID + ".pdf";
        string myPDFfilePath = Server.MapPath("~/Reports/" + myPDFfileName);
        string myCRFileNamePath = Server.MapPath("~/Admin/Utilities/PrintInstadoseLogo.rpt");

        if (File.Exists(myPDFfilePath) == true)
        {
            File.Delete(myPDFfilePath);
        }

        if (File.Exists(myPDFfilePath) == false && myFileID != "")
        {
            try
            {

                cryRpt.Load(myCRFileNamePath);

                //cryRpt.SetParameterValue(0, AcctIdStr);
                //cryRpt.SetParameterValue(1, AcctNameStr);
                //cryRpt.SetParameterValue(2, UserNameStr);
                //cryRpt.SetParameterValue(3, PasswordStr);

                //zeus database connection
                //cryRpt.DataSourceConnections[0].SetConnection("166722-SQLCLUS", "IRV_LCDISBUS", "netgrid", "Mirion2009");

                //staging server database connection
                //cryRpt.DataSourceConnections[0].SetConnection("IRV-SAPL-INSTAD", "LCDISBUS", "drp", "drp1$");

                cryRpt.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, myPDFfilePath);
                cryRpt.Close();
                cryRpt.Dispose();

                // viewing generated PDF file
                string myURL = "http://" + Request.Url.Authority.ToString() + "/Instadose/Reports/" + myPDFfileName;
                //string myURL = "http://" + Request.Url.Authority.ToString() + "/wwwroot/Instadose/Reports/" + myPDFfileName;
                
                Response.Redirect(myURL);

            }
            catch (Exception Ee)
            {
                cryRpt.Close();
                cryRpt.Dispose();
                Response.Write("Crystal Report Error<br>" + Ee);
            }
           
        }

           

    }
}
