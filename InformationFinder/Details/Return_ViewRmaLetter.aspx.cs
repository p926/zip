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

public partial class InformationFinder_Details_Return_ViewAttachment : System.Web.UI.Page
{
    int ReturnID;
    // Create the linq to the App database.
    AppDataContext adc = new AppDataContext();
    InsDataContext idc = new InsDataContext();
    ReportDocument cryRpt = new ReportDocument();

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

    protected void Page_Load(object sender, EventArgs e)
    {
        // is ID?
        if (Request.QueryString["ID"] == null)
            return;

        // Grab the ID if it was passed it the query string
        if (Request.QueryString["ID"] != null)
            int.TryParse(Request.QueryString["ID"], out ReturnID);

        // Find the ID
        if (checkExistence(ReturnID) == false)
        {
            Response.Write("The page could not be loaded because the FileID is invalid.");
            return;
        }

        // 3/28/2014, tdo. Check if rma_ReturnDevices.Active = true
        // 9/22/2011 Grap account info, and location id
        var rsReturnAcct = (from rs in adc.rma_Returns
                            join rd in adc.rma_ReturnDevices on rs.ReturnID equals rd.ReturnID
                            where rs.ReturnID == ReturnID
                            && rd.Active == true
                            select new { rs.AccountID, rd.MasterDeviceID }).ToList();

        // Go to previous page
        if (rsReturnAcct.Count() == 0)
        {
            //ScriptManager.RegisterStartupScript(this, this.GetType(), "Close_Window", "self.close();", true);
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "Close_Window", "self.close();", true);
            //Page.Response.Redirect("Return.aspx?ID=" + ReturnID.ToString() + "#Device_tab");
        }
        else
        {
            var DefalutAcctinfo = (from a in idc.Accounts
                                   join b in idc.Locations
                                   on a.AccountID equals b.AccountID
                                   where b.AccountID == rsReturnAcct[0].AccountID
                                   && b.IsDefaultLocation == true
                                   select new { a.BrandSourceID, b.LocationID }).FirstOrDefault();

            int myLocatioinID = DefalutAcctinfo.LocationID;

            // if recall ONE device, try find the user's location
            if (rsReturnAcct.Count() == 1)
            {
                var DeviceUserLocation = (from a in idc.UserDevices
                                          join b in idc.Users
                                          on a.UserID equals b.UserID
                                          where a.DeviceID == rsReturnAcct[0].MasterDeviceID
                                          && b.AccountID == rsReturnAcct[0].AccountID  // 11/03/2014, tdo
                                          orderby a.AssignmentDate descending
                                          select b.LocationID).FirstOrDefault();   // make sure to pick the latest UserDevice assignment record. 11/20/20112, TDO
                if (DeviceUserLocation > 0)
                {
                    myLocatioinID = DeviceUserLocation;
                }
            }

            // display attachment

            Display_RMAletter(ReturnID.ToString(), myLocatioinID.ToString(),
                int.Parse(DefalutAcctinfo.BrandSourceID.ToString()));

        }

    }

    /// <summary>
    /// Check if Return exist in DB table rma_Returns
    /// </summary>
    /// <param name="ID"> REturnID </param>
    /// <returns>True/False</returns>
    private Boolean checkExistence(int ID)
    {
        bool rBool = false;

        var rsReturn = (from rs in adc.rma_Returns
                        where rs.ReturnID == ID
                        select rs).ToList();

        if (rsReturn.Count != 0)
            rBool = true;

        return rBool;

    }

    /// <summary>
    /// Display RMA Letter
    /// </summary>
    /// <param name="myRMA">RMA number</param>
    /// <param name="myLocation">Account LocationID</param>
    /// <param name="myBrand">What brand?</param>
    private void Display_RMAletter(string myRMA, string myLocation, int myBrand)
    {
        string[] rptParamName = new string[2];
        string[] rptParamVals = new string[2];
        Stream oStream = null;

        //string myPDFfilePath =  Server.MapPath("~/Reports/RMA_") + myRMA + "_" + DateTime.Now.Second.ToString() + ".pdf";
        string myPDFfileName = "RMA_" + myRMA + ".pdf";
        string myPDFfilePath = Server.MapPath("~/Reports/" + myPDFfileName);
        string myCRFileNamePath = "";

        // use IC Care crystal report (1=Quantum, 2=Mirio, 3=Iccare
        if (myBrand == 3)
            myCRFileNamePath = Server.MapPath("~/InformationFinder/Details/RptRMALetter_ICCare.rpt");
        else
            myCRFileNamePath = Server.MapPath("~/InformationFinder/Details/RptRMALetter.rpt");

        if (File.Exists(myPDFfilePath) == true)
        {
            File.Delete(myPDFfilePath);
        }

        if (File.Exists(myPDFfilePath) == false && myRMA != "")
        {

            try
            {
                cryRpt.Load(myCRFileNamePath);

                cryRpt.SetParameterValue(0, myRMA);
                cryRpt.SetParameterValue(1, myLocation);

                string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.AppConnectionString"].ConnectionString.ToString();
                string myDatabase = Regex.Match(ConnectionString, "(Initial Catalog=|Database=){1}([A-Za-z0-9_]+)[;]?").Groups[2].ToString();
                string myServer = Regex.Match(ConnectionString, "(Data Source=){1}([\\w\\d\\.\\$\\-]+)[;]?").Groups[2].ToString();
                string myUserID = Regex.Match(ConnectionString, "(User ID=|UID=){1}([\\w\\d\\.]+)[;]?").Groups[2].ToString();
                string myPassword = Regex.Match(ConnectionString, "(Pwd=|Password=|PWD=){1}([\\w\\d\\.\\$]+)[;]?").Groups[2].ToString();

                cryRpt.DataSourceConnections[0].SetConnection(myServer, myDatabase, myUserID, myPassword);
                //cryRpt.ExportToDisk(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat, myPDFfilePath);

                //// viewing generated PDF file
                //string myURL = "http://" + Request.Url.Authority.ToString() + "/Instadose/Reports/" + myPDFfileName;
                //Response.Redirect(myURL);


                //export to memory Stream                
                oStream = cryRpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                byte[] streamArray = new byte[oStream.Length];
                oStream.Read(streamArray, 0, Convert.ToInt32(oStream.Length - 1));

                Response.Clear();
                Response.Buffer = true;
                Response.ContentType = "application/pdf";
                Response.BinaryWrite(streamArray);
                Response.End();

            }
            catch (Exception e)
            {
                oStream.Flush();
                oStream.Close();
                oStream.Dispose();
                Response.Write(e);
            }
            finally
            {
                cryRpt.Close();
                cryRpt.Dispose();
            }
        }
    }
}
