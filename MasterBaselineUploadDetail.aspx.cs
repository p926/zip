/*
 * Maintain PO Receipts
 * 
 *  Created By: Tdo, 10/16/2012
 *  Copyright (C) 2010 Mirion Technologies. All Rights Reserved.
 */

using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Collections;
using System.Diagnostics;

using Instadose;
using Instadose.Data;
using ReadWriteCsv;
using Mirion.DSD.Consumer;

public partial class TechOps_MasterBaselineUploadDetail : System.Web.UI.Page
{
    private bool _refreshState;
    private bool _isRefresh;
    public bool IsRefresh
    {
        get
        {
            return _isRefresh;
        }
    }
    protected override void LoadViewState(object savedState)
    {
        object[] allStates = (object[])savedState;
        base.LoadViewState(allStates[0]);
        _refreshState = (bool)allStates[1];
        _isRefresh = _refreshState == (bool)Session["__ISREFRESH"];
    }
    protected override object SaveViewState()
    {
        Session["__ISREFRESH"] = _refreshState;
        object[] allStates = new object[2];
        allStates[0] = base.SaveViewState();
        allStates[1] = !_refreshState;
        return allStates;
    }

    InsDataContext idc = new InsDataContext();    
    string UserName = "Unknown";
    string ReceiptID;
    string PO_ReceiptFile = @"\\irv-sapl-file1.mirion.local\POReceipt$\PO_Receipt.csv";
    string PO_ReceiptDDFile = @"\\irv-sapl-file1.mirion.local\POReceipt$\PO_ReceiptDD.csv";
    MasterBaselineUpload mbu;
    StreamReader masterFileContent ;
    StreamReader copyMasterFileContent;
    StreamReader baselineFileContent ;
    bool DevelopmentServer = false;  

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {            
            InvisibleErrors();

            // Auto set if a development site
            if (Request.Url.Authority.ToString().Contains(".mirioncorp.com"))
            {
                DevelopmentServer = true;
                PO_ReceiptFile = @"\\irv-sapl-file1.mirion.local\POReceipt$\PO_Receipt_STG.csv";
                PO_ReceiptDDFile = @"\\irv-sapl-file1.mirion.local\POReceipt$\PO_ReceiptDD_STG.csv";
            }

            if (User.Identity.Name.IndexOf('\\') > 0)
                this.UserName = User.Identity.Name.Split('\\')[1];
            else
                this.UserName = "Testing";

            // Grab the ReturnID if it was passed in the query string
            //int ReturnID = 0;
            if (Request.QueryString["receiptID"] == null)
            {
                Page.Response.Redirect("MasterBaselineFileUpload.aspx");
            }
            else
            {
                ReceiptID = Request.QueryString["receiptID"].ToString();
            }

            // Get upload file paths
            //masterFileName = Server.MapPath(".\\UploadFile\\" + MasterFile.FileName.ToString());
            //baselineFileName = Server.MapPath(".\\UploadFile\\" + BaselineFile.FileName.ToString());

            //// Delete all previous uploaded files under Tech_Ops\UploadFile folder. We do not need it after uploading to MAS
            //string uploadFileDirectory = Server.MapPath(".\\UploadFile");
            //string[] files = Directory.GetFiles(uploadFileDirectory);
            //foreach (string file in files)
            //{
            //    File.Delete(file);
            //}   
                        
        }
        catch 
        { this.UserName = "Unknown"; }
      
    }
    
    protected void btnCancel_Click(object sender, EventArgs e)
    {
        Page.Response.Redirect("MasterBaselineFileUpload.aspx");
    }

    protected void btnUpload_Click(object sender, EventArgs e)
    {
        try
        {
            InvisibleErrors();

            if (!IsRefresh)  
            {
                if (passInputsValidation())
                {
                    string resultMessages = "";

                    uploadMasterBaseline(ref resultMessages);

                    submitToMas();

                    setUploadToMasFlag();

                    VisibleErrors("Completed");

                    this.btnUpdate.Enabled = false;                    

                    PrintOutputReport(ReceiptID, resultMessages);

                    //Page.Response.Redirect("MasterBaselineFileUpload.aspx"); Upload to MAS will not work if redirect page
                }
                else
                {
                    //ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('myDialog')", true);
                }
            }            
        }
        catch (Exception ex)
        {
            VisibleErrors(ex.ToString());
        }
    }

    private void setUploadToMasFlag()
    {        
        POReceipt myPOReceipt = (from po in idc.POReceipts 
                                where po.ReceiptID == int.Parse(this.ReceiptID)
                                select po).FirstOrDefault();
        if (myPOReceipt != null)
        {
            myPOReceipt.UploadInMAS = true;
            myPOReceipt.ModUser = UserName;
            myPOReceipt.ModDate = DateTime.Today;
            idc.SubmitChanges();
        }
    }

    //private void uploadMasterBaseline(ref string resultMessages)
    //{
    //    bool check;
    //    string sn;
    //    Hashtable badBL2List = new Hashtable();
 
    //    foreach (GridViewRow rowitem in gvFailBL2.Rows)
    //    {                                   
    //        CheckBox select = (CheckBox)rowitem.FindControl("cbRow");
    //        check = select.Checked;

    //        sn = rowitem.Cells[1].Text;

    //        badBL2List.Add(sn, check);                   
    //    }

    //    MasterBaselineUpload mbu = new MasterBaselineUpload(this.masterFileName, this.baselineFileName, this.ReceiptID);
    //    mbu.Execute(int.Parse(this.ReceiptID), badBL2List, ref resultMessages);
    //}

    private void uploadMasterBaseline(ref string resultMessages)
    {                        
        try
        {
            mbu.Execute(ref resultMessages);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    private void submitToMas()
    {
        try
        {            
            Hashtable lineIndexItemNumberPair = new Hashtable();

            WritePO_ReceiptFile(PO_ReceiptFile, ref lineIndexItemNumberPair);

            WritePO_ReceiptDDFile(PO_ReceiptDDFile, lineIndexItemNumberPair);

            RunBatch();            

            CopyFilesToSubFolder(PO_ReceiptFile, PO_ReceiptDDFile, ReceiptID);            
            
        }
        catch (Exception ex)
        {
            VisibleErrors(ex.ToString());
        }
    }

    private void RunBatch()
    {
        try
        {
            if (DevelopmentServer)
            {
                ClientScript.RegisterStartupScript(Page.GetType(), "clientScript", "<script type='text/javascript'>executeBatch_STG()</script>");
            }
            else
            {
                ClientScript.RegisterStartupScript(Page.GetType(), "clientScript", "<script type='text/javascript'>executeBatch()</script>");
            }            
        }
        catch (Exception ex)
        {
            throw new Exception("Run batch file error. " + ex.ToString());
        }

    }   

    private void InvisibleErrors()
    {
        // Reset submission form error message      
        this.errorMsg.InnerHtml = "";
        this.errors.Visible = false;
    }

    private void VisibleErrors(string error)
    {
        this.errorMsg.InnerHtml = error;
        this.errors.Visible = true;
    }

    private bool passInputsValidation()
    {
        if (!MasterFile.HasFile || !BaselineFile.HasFile)
        {
            VisibleErrors("Master file & Baseline file are required!");
            return false;
        }

        Stream fileContentStream = MasterFile.FileContent;
        Stream copyFileContentStream = new MemoryStream();
        fileContentStream.CopyTo(copyFileContentStream);

        fileContentStream.Position = 0;
        copyFileContentStream.Position = 0;

        masterFileContent = new StreamReader(fileContentStream);
        copyMasterFileContent = new StreamReader(copyFileContentStream);
        baselineFileContent = new StreamReader(BaselineFile.FileContent);
                        
        mbu = new MasterBaselineUpload(masterFileContent, baselineFileContent, this.ReceiptID);
        
        // if not pass validation
        if (!mbu.MasterBaselineUploadValidation())
        {           
            VisibleErrors(GenerateMessage(mbu.ValidationErrorMessage()));

            return false;                         
        }

        return true;
    }

    private string GenerateMessage(DataTable pDtErrorMessages)
    {        
        string errorText = "<ul type='circle'>";
        foreach (DataRow error in pDtErrorMessages.Rows)
        {
            bool uploadAnyway = (bool)error["Commit"];
            // Generate a warning message if the flag = false. Otherwise, the process will upload anyway and set device's location to DEFECT
            if (! uploadAnyway)    
            {
                errorText += "<li>" + error["ErrorMessage"] + "</li>"; 
            }            
        }
        errorText += "</ul>";

        return "The following error(s) occurred: " + errorText;
       
    }

    private bool IsBL2Error(DataTable pDtErrorMessages)
    {
        foreach (DataRow error in pDtErrorMessages.Rows)
        {
            if (error["ValidationType"].ToString() == "Baseline2Validation")
            {
                return true;
            }
        }
        return false;
    }

    private string GetMatchItemNumber(string pColor, Hashtable pLineIndexItemNumberPair)
    {
        string abbrColorCode = GetColorCode(pColor);
        foreach (string key in pLineIndexItemNumberPair.Keys)
        {
            if (key.ToUpper().Contains(abbrColorCode))
                return key;
        }
        return "";
    }

    private string GetColorCode(string pColor)
    {
        switch (pColor.ToUpper())
        {
            case "BLACK":
                return "BLK";
            case "SILVER":
                return "SLV";
            case "PINK":
                return "PNK";
            case "GREEN":
                return "GRN";
            case "BLUE":
                return "BLU";
            default:
                return "";
        }
    }

    private void CopyFilesToSubFolder(string pPOReceiptFile, string pPOReceiptDDFile, string ReceiptID)
    {        
        try
        {
            string newPOReceiptFileName, newPOReceiptDDFileName;            

            newPOReceiptFileName = @"\\irv-sapl-file1.mirion.local\POReceipt$\POReceiptStorage\PO_Receipt_" + ReceiptID + ".csv";
            newPOReceiptDDFileName = @"\\irv-sapl-file1.mirion.local\POReceipt$\POReceiptStorage\PO_ReceiptDD_" + ReceiptID  + ".csv";

            if (File.Exists(newPOReceiptFileName))
            {
                File.Delete(newPOReceiptFileName); // Delete the old file
            }
            if (File.Exists(newPOReceiptDDFileName))
            {
                File.Delete(newPOReceiptDDFileName); // Delete the old file
            }

            if (File.Exists(pPOReceiptFile))
            {
                //File.Move(pPOReceiptFile, newPOReceiptFileName); // Move to subfolder 
                File.Copy(pPOReceiptFile, newPOReceiptFileName);
            }

            if (File.Exists(pPOReceiptDDFile))
            {
                //File.Move(pPOReceiptDDFile, newPOReceiptDDFileName); // Move to subfolder 
                File.Copy(pPOReceiptDDFile, newPOReceiptDDFileName);
            }
                                              
        }
        catch (IOException ex)
        {
            throw new Exception("Failed copy files to subfolder. " + ex.ToString());
        }
    }

    private void WritePO_ReceiptFile(string pPathFile, ref Hashtable pLineIndexItemNumberPair)
    {
        Hashtable QtyReceivedHash = new Hashtable() ;
        string myReceiptNo, myRegularLineItem, myReceiptDate, myPONumber, myVendorNumber;
        double myUnitCost, myExtendedCost, myNonTaxAmount;
        string myItemNumber, myPOLineIndex;
        int myQtyReceived, myQtyOrdered, myQtyBalance;
        int myLineIndex;
        string myPOOrderStatus;
        DataTable entryLineDT;
        int isUploadOpenStatusPO = 0;    // Normally, the upload process works only for "New" and "BackOrdered" status PO. However, sometimes the new generated PO has a "Open" status.

        var myPOReceipt = (from a in idc.POReceipts
                               where a.ReceiptID == Convert.ToInt32(ReceiptID)
                               select a).FirstOrDefault();

        var myPOReceiptDetail = (from a in idc.POReceiptDetails
                                 where a.ReceiptID == Convert.ToInt32(ReceiptID)
                               select a).ToList();

        var mySetting = (from AppSet in idc.AppSettings where AppSet.AppKey == "UploadOpenStatusPOActive" select AppSet).FirstOrDefault();
        isUploadOpenStatusPO = (mySetting != null) ? int.Parse(mySetting.Value) : 0;

        if (myPOReceipt == null)
        {
            throw new Exception("Write PO_Receipt file error. No receipt record found for receipt#: " + ReceiptID);
        }

        if (myPOReceiptDetail.Count() > 0)
        {
            foreach (var item in myPOReceiptDetail)
            {
                // Get the received Qty by ItemNumber/Color from an uploaded Master file/Baseline file, not from POReceiptDetail record.
                //QtyReceivedHash.Add(item.ItemNumber.Trim(), item.QtyRecd);    
                QtyReceivedHash.Add(item.ItemNumber.Trim(), mbu.ActualNumberOfUploadedDeviceByItemNumber(item.ItemNumber.Trim()));                
            }
        }


        myRegularLineItem = "L";
        myReceiptNo = ReceiptID;
        myPONumber = myPOReceipt.PONumber;
        myVendorNumber = myPOReceipt.Vendor;
        myReceiptDate = myPOReceipt.ReceiptDate.Year.ToString() + myPOReceipt.ReceiptDate.Month.ToString().PadLeft(2, '0') + myPOReceipt.ReceiptDate.Day.ToString().PadLeft(2, '0');

        entryLineDT = GetAllEntryLinesByPONumber(myPONumber);
        myPOOrderStatus = GetPOOrderStatus(myPONumber);

        if (entryLineDT == null)
        {
            throw new Exception("Write PO_Receipt file error. Check the MAS system. No entry lines for PO#: " + myPONumber);
        }

        try
        {
            // Delete the existing old file
            if (File.Exists(pPathFile))
            {
                File.Delete(pPathFile); // Delete the old file
            }

            // the start line index will be the latest lineindex in ReceiptEntryLineDetail + 1
            myLineIndex = GetInitialLineIndex();

            // Create a new file
            using (CsvFileWriter writer = new CsvFileWriter(pPathFile))
            {

                myNonTaxAmount = GetNonTaxAmount(entryLineDT, QtyReceivedHash, myPOReceipt.ReceiptDate);

                foreach (DataRow r in entryLineDT.Rows)
                {
                    CsvRow row = new CsvRow();

                    myItemNumber = r["ItemNumber"].ToString().Trim();
                    myPOLineIndex = r["LineIndex"].ToString().Trim();

                    
                    // Convert the UnitCost from Europe to USD
                    // myUnitCost = double.Parse(r["UnitCost"].ToString().Trim());
                    // myUnitCost = Convert.ToDouble(Convert.ToDecimal(r["UnitCost"]) * Currencies.GetExchangeRate("EUR", "USD", myPOReceipt.ReceiptDate));
                    // 04/15/2013, TDO. Get the converted UnitCost from Monica's linked object view instead to resolve the issue that UnitCost keeps changing overtime.
                    myUnitCost = GetConvertedUnitCostByItemNumber(myItemNumber);

                    myQtyReceived = QtyReceivedHash.Contains(myItemNumber) ? int.Parse(QtyReceivedHash[myItemNumber].ToString()) : 0;

                    if (isUploadOpenStatusPO == 1)
                    {

                        if (myPOOrderStatus.ToUpper() == "N" || myPOOrderStatus.ToUpper() == "O")
                        {
                            myQtyOrdered = Convert.ToInt32(double.Parse(r["QtyOrdered"].ToString().Trim()));
                        }
                        else if (myPOOrderStatus.ToUpper() == "B")
                        {
                            myQtyOrdered = Convert.ToInt32(double.Parse(r["QtyBckordrd"].ToString().Trim()));
                        }
                        else
                        {
                            myQtyOrdered = 0;
                        }
                    }
                    else
                    {
                        if (myPOOrderStatus.ToUpper() == "N")
                        {
                            myQtyOrdered = Convert.ToInt32(double.Parse(r["QtyOrdered"].ToString().Trim()));
                        }
                        else if (myPOOrderStatus.ToUpper() == "B")
                        {
                            myQtyOrdered = Convert.ToInt32(double.Parse(r["QtyBckordrd"].ToString().Trim()));
                        }
                        else
                        {
                            myQtyOrdered = 0;
                        }                    
                    }
                    
                    myQtyBalance = myQtyOrdered - myQtyReceived;
                    myExtendedCost = myQtyReceived * myUnitCost;

                    row.Add(myRegularLineItem);
                    row.Add(myReceiptNo);
                    row.Add(myReceiptDate);
                    row.Add(myPONumber);
                    row.Add(myVendorNumber);
                    row.Add(myItemNumber);
                    row.Add(myPOLineIndex);
                    row.Add(myUnitCost.ToString());
                    row.Add(myQtyReceived.ToString());
                    row.Add(myQtyOrdered.ToString());
                    row.Add(myQtyBalance.ToString());
                    row.Add(myExtendedCost.ToString());
                    row.Add(myNonTaxAmount.ToString());

                    writer.WriteRow(row);

                    myLineIndex += 1;
                    pLineIndexItemNumberPair.Add(myItemNumber, myLineIndex.ToString().PadLeft(5, '0'));


                    myItemNumber = "";
                    myPOLineIndex = "";
                    myUnitCost = 0;
                    myQtyReceived = 0;
                    myQtyOrdered = 0;
                    myQtyBalance = 0;
                    myExtendedCost = 0;
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Write PO_Receipt file error. " + ex.ToString());
        }

    }

    private void WritePO_ReceiptDDFile(string pPathFile, Hashtable pLineIndexItemNumberPair)
    {
        string myReceiptNo;
        string myQtyReceived;
        string myItemNumber, mySerialNo, myLineIndex, myColor;
        string myLine;        
        System.IO.StreamReader streamReader;

        myReceiptNo = ReceiptID;
        myQtyReceived = "1";

        try
        {
            // Delete the existing old file
            if (File.Exists(pPathFile))
            {
                File.Delete(pPathFile); // Delete the old file
            }

            // Create a new file
            using (CsvFileWriter writer = new CsvFileWriter(pPathFile))
            {                
                streamReader = copyMasterFileContent;

                //Read each line of the downloaded file
                if (streamReader.Peek() != -1)
                {
                    myLine = streamReader.ReadLine(); //ignore the first line
                    while (streamReader.Peek() != -1)
                    {
                        myLine = streamReader.ReadLine();
                        if (myLine.Length > 0)
                        {
                            string[] columns = myLine.Split(',');

                            mySerialNo = columns[0];
                            myColor = columns[3];
                            myItemNumber = GetMatchItemNumber(myColor, pLineIndexItemNumberPair);
                            myLineIndex = pLineIndexItemNumberPair.ContainsKey(myItemNumber) ? (string)pLineIndexItemNumberPair[myItemNumber] : "";

                            CsvRow row = new CsvRow();

                            row.Add(myReceiptNo);
                            row.Add(myLineIndex);
                            row.Add(myItemNumber);
                            row.Add(mySerialNo);
                            row.Add(myQtyReceived);

                            writer.WriteRow(row);
                        }

                        mySerialNo = "";
                        myColor = "";
                        myItemNumber = "";
                        myLineIndex = "";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Write PO_ReceiptDD file error. " + ex.ToString());
        }        
    }

    private double GetNonTaxAmount(DataTable pEntryLineDT, Hashtable pQtyReceivedHash, DateTime pReceiptDate)
    {
        try
        {
            if (pEntryLineDT.Rows.Count > 0)
            {
                double myExtendedCost = 0;
                double myUnitCost = 0;
                int myQtyReceived = 0;
                string myItemNumber = "";

                foreach (DataRow r in pEntryLineDT.Rows)
                {
                    myItemNumber = r["ItemNumber"].ToString().Trim();
                    myQtyReceived = pQtyReceivedHash.Contains(myItemNumber) ? int.Parse(pQtyReceivedHash[myItemNumber].ToString()) : 0;

                    // Convert the UnitCost from Europe to USD
                    // myUnitCost = double.Parse(r["UnitCost"].ToString().Trim());                    
                    // myUnitCost = Convert.ToDouble(Convert.ToDecimal(r["UnitCost"]) * Currencies.GetExchangeRate("EUR", "USD", pReceiptDate)); 
                    // 04/15/2013, TDO. Get the converted UnitCost from Monica's linked object view instead to resolve the issue that UnitCost keeps changing overtime.
                    myUnitCost = GetConvertedUnitCostByItemNumber(myItemNumber);
                    
                    myExtendedCost += (myQtyReceived * myUnitCost);
                    
                    myUnitCost = 0;
                    myQtyReceived = 0;
                    myItemNumber = "";
                }
                return myExtendedCost;
            }
            else
            {
                return 0;
            }
        }
        catch
        {
            return 0;
        }
    } 

    private double GetConvertedUnitCostByItemNumber( string pItemNumber)
    {
        double myUnitCost = 0;
        try
        {
            String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            String cmdStr = "dbo.sp_GetConvertedPOUnitCostByItemNumber";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;  
          
            sqlCmd.Parameters.Add("@ItemNumber", SqlDbType.NVarChar, 20);
            sqlCmd.Parameters["@ItemNumber"].Value = pItemNumber;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);

            sqlConn.Close();

            if (dt != null && dt.Rows.Count > 0 && dt.Rows[0]["UnitCost"] != null)
            {               
                if (!double.TryParse(dt.Rows[0]["UnitCost"].ToString().Trim(), out myUnitCost))
                {
                    myUnitCost = 0;
                }                
            }
            return myUnitCost;
        }
        catch
        {
            return myUnitCost;
        }
    }     

    private DataTable GetAllEntryLinesByPONumber(string pPONumber)
    {
        try
        {
            String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            String cmdStr = "dbo.sp_GetPOEntryLineByPONumber";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.Add("@PONumber", SqlDbType.NVarChar, 7);

            sqlCmd.Parameters["@PONumber"].Value = pPONumber;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);

            sqlConn.Close();

            return dt;
        }
        catch { return null; }
    }

    private void PrintOutputReport(string pReceiptID, string pResultMessages)
    {        
        string url = "MasterBaselineUploadReport.aspx?receiptID=" + pReceiptID + "&resultMessages=" + pResultMessages;
        ScriptManager.RegisterStartupScript(this, typeof(string), "OPEN_WINDOW", "window.open( '" + url + "', 'MasterBaselineUploadReport', 'height=450,width=800,status=yes,toolbar=yes,menubar=no,location=no,resize=yes' );", true);        
    }

    private DataTable GetPOReceiptInfo(int pReceiptID)
    {
        try
        {
            String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            String cmdStr = "dbo.sp_GetPOReceiptInfo";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.Add("@ReceiptID", SqlDbType.Int);

            sqlCmd.Parameters["@ReceiptID"].Value = pReceiptID;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);

            sqlConn.Close();

            return dt;
        }
        catch { return null; }
    }

    private void GenerateReportData(ref DataTable poInfoDT, string resultMessage)
    {
        try
        {
            if (poInfoDT.Rows.Count > 0)
            {
                poInfoDT.Rows[0]["Output"] = resultMessage;
            }
        }
        catch
        {
        }
               
    }

    private int GetTotalSerialUploadedinMAS(int pReceiptID)
    {
        try
        {
            String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            String cmdStr = "dbo.sp_GetAllSerialInMASByReceiptID";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.Add("@ReceiptID", SqlDbType.Int);

            sqlCmd.Parameters["@ReceiptID"].Value = pReceiptID;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);

            sqlConn.Close();

            return dt.Rows.Count ;
        }
        catch { return 0; }
    }

    private string GetPOOrderStatus(string pPONumber)
    {
        try
        {
            String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            String cmdStr = "dbo.sp_GetPOEntryHeaderByPONumber";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.Add("@PONumber", SqlDbType.NVarChar, 7);

            sqlCmd.Parameters["@PONumber"].Value = pPONumber;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);

            sqlConn.Close();

            if (dt != null && dt.Rows.Count > 0 && dt.Rows[0]["OrderStatus"] != null)
            {
                return dt.Rows[0]["OrderStatus"].ToString().Trim();
            }
            else
            {
                return "";
            }        
        }
        catch { return ""; }
    }

    private int GetInitialLineIndex()
    {
        try
        {
            String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            String cmdStr = "dbo.sp_GetLatestReceiptEntryLineIndex";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;            

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);

            sqlConn.Close();

            if (dt != null && dt.Rows.Count > 0 && dt.Rows[0]["LineIndex"] != null)
            {
                int myLineIndex;

                if (!int.TryParse(dt.Rows[0]["LineIndex"].ToString().Trim(), out myLineIndex))
                {
                    myLineIndex = 0;
                }

                return myLineIndex;
            }
            else
            {
                return 0;
            }           
        }
        catch { return 0; }
    }     

}