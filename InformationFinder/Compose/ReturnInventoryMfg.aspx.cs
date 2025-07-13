using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text.RegularExpressions;

using Instadose;
using Instadose.Data;


public partial class Instadose_InformationFinder_Compose_ReturnInventoryMfg : System.Web.UI.Page
{
    // Turn on/off email to all others
    bool DevelopmentServer ;

    // Create the database reference
    InsDataContext idc = new InsDataContext();
    AppDataContext adc = new AppDataContext();

    // Create data table for devices
    DataTable dtDeviceDataTable = null;
    
// String to hold the current username
    string UserName = "Unknown";

    protected void Page_Load(object sender, EventArgs e)
    {
        Page.Title = "RMA - Mfg Receiving";
   
        // Try to set the username
        try
        {
            string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString.ToString();
            string myDatabase = Regex.Match(ConnectionString, "(Initial Catalog=|Database=){1}([A-Za-z0-9_]+)[;]?").Groups[2].ToString();
            if (myDatabase.Contains("IRV_InstaProd"))
                DevelopmentServer = false;
            else
                DevelopmentServer = true;

            InvisibleErrors();
            InvisibleMessages();
            InvisibleErrorReview();

            this.UserName = User.Identity.Name.Split('\\')[1];
            this.lblusername.Text = this.UserName;
        }
        catch { this.UserName = "Unknown"; }
    }

    protected void btnContinue_Click(object sender, EventArgs e)
    {
        // Hide the first panel and display the secondary panel.
        this.plAdd.Visible = false;
        this.plReview.Visible = true;

        // Store the list of serial numbers that the user has added.
        List<string> serialNumberList = new List<string>();
        serialNumberList.AddRange(this.txtSerialNos.Text.Split('\n'));

        // Load the serial number data
        this.btnFinish.ForeColor = System.Drawing.Color.Black ;
        this.LoadDeviceDataTable(serialNumberList);

        // Render the grid.
        this.gvReview.DataSource = dtDeviceDataTable;
        this.gvReview.DataBind();
    }

    protected void btnBack_Click(object sender, EventArgs e)
    {
        // Hide the second panel and display the primary panel.
        this.plAdd.Visible = true;
        this.plReview.Visible = false;
    }

    protected void btnFinish_Click(object sender, EventArgs e)
    {        
        // Create the entries. Commit Mfg receive
        this.Commit();

        // Create upload attachment record to unique requestID
        if (FileUpload1.HasFile)
            this.CommitUploadAttachFile(1);

        if (FileUpload2.HasFile)
            this.CommitUploadAttachFile(2);

        // Send Email
        this.CommitSendEmailNote();

        // Redirect back to first page, with a success message.
        
        this.VisibleMessages("Devices have been added to the return inventory.");   
        this.txtSerialNos.Text = "";
        this.plAdd.Visible = true;
        this.plReview.Visible = false;
    }


    private DeviceInventory getDeviceInventory(string serialNumber)
    {
        // Create the object to return.
        DeviceInventory devIn = null;
        try
        {
            // Find the inventory for the devices and the device info.
            devIn = (from di in idc.DeviceInventories
                     where di.SerialNo == serialNumber 
                     select di).First();

        }
        catch
        {
            // Set the object to null
            devIn = null;
        }

        // Return the object
        return devIn;
    }

    private AccountDevice getAccountDevice(int DeviceID)
    {
        // Create the object to return.
        AccountDevice addDev = null;
        try
        {
            // Find the account device for the devices and the device info.
            addDev = (from ad in idc.AccountDevices
                      where ad.DeviceID == DeviceID && ad.CurrentDeviceAssign == true
                      select ad).First();
        }
        catch
        {
            // Set the object to null
            addDev = null;
        }

        // Return the object
        return addDev;
    }

    private rma_ReturnInventory getReturnInventoryList(string serialNumber, int ReturnID)
    {
        // Create the object to return.
        rma_ReturnInventory returnInventory = null;
        try
        {
            // Find the return inventory filter out completed items, since
            // a device could come through the return process many times
            // throughout the devices life.
            returnInventory = (from ri in adc.rma_ReturnInventories
                               where ri.SerialNo == serialNumber
                               && ri.ReturnID == ReturnID
                               && ri.Active == true
                               select ri).First();
        }
        catch
        {
            // Set the object to null
            returnInventory = null;
        }

        // Return the object
        return returnInventory;
    }

    private rma_ReturnInventory getReturnInventory(string serialNumber)
    {
        // Create the object to return.
        rma_ReturnInventory returnInventory = null;
        try
        {
            // Find the return inventory filter out completed items, 
            // and with an active rma request,
            // since
            // a device could come through the return process many times
            // throughout the devices life.
            returnInventory = (from ri in adc.rma_ReturnInventories
                               join rRet in adc.rma_Returns
                                  on ri.ReturnID equals rRet.ReturnID 
                               where ri.SerialNo == serialNumber 
                                && ri.Active == true
                                && rRet.Active == true
                                && ri.Completed == false  // 3/21/2012
                                //&& ri.CommittedToFinance == null  // 7/18/2011
                               select ri).First();
        }
        catch
        {
            // Set the object to null
            returnInventory = null;
        }

        // Return the object
        return returnInventory;
    }

    private rma_ReturnDevice getReturnDevice(string serialNumber)
    {
        // Create the object to return.
        rma_ReturnDevice returnDevice = null;
        try
        {
            // Find the return device
            returnDevice = (from rd in adc.rma_ReturnDevices
                            where rd.SerialNo == serialNumber && rd.Active== true
                            select rd).First();
        }
        catch
        {
            // Set the object to null
            returnDevice = null;
        }

        // Return the object
        return returnDevice;
    }

    private rma_ref_Department getRequestDepartment(string requestNo)
    { 
        // create the object to return.
        rma_ref_Department returnDepartment = null;
        try 
        { 
            int myRequestNo = 0;
            int.TryParse(requestNo, out myRequestNo);
            // find the return department request
            returnDepartment = (from rt in adc.rma_Returns
                                join rtT in adc.rma_ref_ReturnTypes
                                on rt.ReturnTypeID equals rtT.ReturnTypeID
                                join rtD in adc.rma_ref_Departments
                                on rtT.DepartmentID equals rtD.DepartmentID
                                where rt.ReturnID == myRequestNo
                                select rtD).First();
        }
            
        catch 
        {
            // return null 
            returnDepartment = null;
        }

        //return the object
        return returnDepartment;
    }


    /// <summary>
    /// Load the data table with from the serial numbers to add to inventory.
    /// </summary>
    /// <param name="serialNumberList"></param>
    private void LoadDeviceDataTable(List<string> serialNumberList)
    {
        // Store a list of error'd serials
        Dictionary<string, string> errorList = new Dictionary<string, string>();

        // DataTable to store the serial data
        dtDeviceDataTable = new DataTable();

        dtDeviceDataTable.Columns.Add("SerialNo", typeof(string));
        dtDeviceDataTable.Columns.Add("AccountNo", typeof(int));
        dtDeviceDataTable.Columns.Add("RequestNo", typeof(string));
        dtDeviceDataTable.Columns.Add("Action", typeof(string));
        dtDeviceDataTable.Columns.Add("Type", typeof(string));
        dtDeviceDataTable.Columns.Add("DepartmentRequest", typeof(string));

        // Ensure that at least one device was added.
        if (serialNumberList.Count > 0)
        {

            int accountNo ;
            string requestNo;
            string reqDepartment ;
            string reqAction ;
            string reqType ;
            bool allowAdd ;

            // Loop through each serial number.
            foreach (string serialNumber in serialNumberList)
            {
                
                if (serialNumber.Trim() != "")
                {
                    // Find the device info.
                    DeviceInventory deviceInventory = getDeviceInventory(serialNumber.Trim());

                    // Find if the device is currently assigned to an account
                    AccountDevice accountDevice = null;
                    if (deviceInventory != null)
                    {
                        accountDevice = getAccountDevice(deviceInventory.DeviceID);
                    }


                    // Find if the device has already been RECEIVED/SCANNED by receiving dept and waiting for tech ops review.
                    rma_ReturnInventory returnInventory = getReturnInventory(serialNumber.Trim());

                    
                   
                    // Find if the device has already initially requested return and waiting for receiving.
                    // This is when the device has not RECEIVED by receiving dept.
                    rma_ReturnDevice returnDevice = (from rd in adc.rma_ReturnDevices
                                                     where rd.SerialNo == serialNumber.Trim()
                                                     && rd.Active == true
                                                     && rd.Received == false
                                                     select rd).FirstOrDefault();
                    
                    // device find in ReturnDevice but not AccountDevice
                    if ((accountDevice != null || returnDevice != null) && (deviceInventory != null) && (returnInventory == null) ) // 9/1/2011
                    {
                        // Set the default or standards
                         accountNo = 0;
                         requestNo = "None";
                         reqDepartment = "";
                         reqAction = "";
                         reqType = "";
                         allowAdd = true;

                        if (returnDevice != null)   // already initially returned.
                        {
                            accountNo = (from rt in adc.rma_Returns
                                         where rt.ReturnID == returnDevice.ReturnID
                                         select rt.AccountID).First();
                            requestNo = returnDevice.ReturnID.ToString();

                            // Get Request Department
                            rma_ref_Department returnDepartment = getRequestDepartment(requestNo);
                            reqDepartment = returnDepartment.Name.ToString();

                            reqAction = "The device will be checked in.";
                            reqType = "Device has been received for a return.";
                        }
                        else      // never initially returned.
                        {
                            //accountNo = (accountDevice != null) ? accountDevice.AccountID : 0;
                            accountNo = accountDevice.AccountID;
                            requestNo = "None";
                            reqDepartment = "";

                            reqAction = "A request will be generated for this account.";
                            reqType = "This return was unexpected. A return was generated for this account.";

                            // if return by Philips or CareStream and without RMA request
                            string AccountIDToCheck = "4640,4710";

                            if (AccountIDToCheck.IndexOf(accountNo.ToString()) >= 0)
                            {
                                allowAdd = false;
                                //this.btnFinish.ForeColor = System.Drawing.Color.Red;
                                errorList.Add(serialNumber, "Enterprise account - Unauthorized return, please pass package to LP or Gordon.");
                            }
                        }

                        //// Get Request Department
                        //if (requestNo != "None")
                        //{
                        //    rma_ref_Department returnDepartment = getRequestDepartment(requestNo);
                        //    reqDepartment = returnDepartment.Name.ToString();
                        //}
                            

                        // Device has an account association.
                        //if (requestNo == "None" )
                        //if (requestNo == "None" && accountDevice != null) // 9/1/2011
                        //{
                        //    reqAction = "A request will be generated for this account.";
                        //    reqType = "This return was unexpected. A return was generated for this account.";

                        //    // if return by Philips or CareStream and without RMA request
                        //    string AccountIDToCheck = "4640,4710";

                        //    if (AccountIDToCheck.IndexOf(accountDevice.AccountID.ToString()) >= 0)
                        //    {
                        //        this.btnFinish.ForeColor = System.Drawing.Color.Red;
                        //        errorList.Add(serialNumber, "Enterprise account - Unauthorized return, please do NOT check in and pass package to LP or Gordon.");
                        //    }
                        //}
                        //else
                        //{
                        //    reqAction = "The device will be checked in.";
                        //    reqType = "Device has been received for a return.";
                        //}


                        if (allowAdd)
                        {
                            // Create the data row to be added to the data table.
                            DataRow dr = dtDeviceDataTable.NewRow();

                            dr["SerialNo"] = serialNumber;
                            dr["AccountNo"] = accountNo;
                            dr["RequestNo"] = requestNo;
                            dr["DepartmentRequest"] = reqDepartment;
                            dr["Action"] = reqAction;
                            dr["Type"] = reqType;

                            // Add the row to the DataTable.
                            dtDeviceDataTable.Rows.Add(dr);
                        }
                        
                    }

                    else if (deviceInventory != null && returnInventory != null)
                    {
                        errorList.Add(serialNumber, "Device has already been added into receiving inventory and waiting for tech ops review. No other receive can be made on this device.");
                    }

                    else if (accountDevice == null)
                    {
                        // Device does not have an account association.
                        errorList.Add(serialNumber, "Device does not belong to any account.");
                    }

                    else // Could not find the serial number.
                    {
                        errorList.Add(serialNumber, "Serial number does not exist in the Instadose system.");
                    }
                }
            }

            // Once all of the devices have been added to the data table,
            // display the results to the screen.            
    

            string errorText = "<ul type='circle'>";
            foreach (KeyValuePair<string, string> error in errorList)
            {
                errorText += "<li>" + error.Key + " - " + error.Value + "</li>";
            }   
            errorText += "</ul>";

            // Display the error message if errors exist.
            if (errorList.Count > 0)
            {                
                this.VisibleErrorReview("The following error(s) occurred: " + errorText);
            }                    
            
            // Sort the contents of the data table.
            dtDeviceDataTable.DefaultView.Sort = "AccountNo, SerialNo";
        }
    }

    private void CommitUploadAttachFile(int UploadControlNo)
    {
        // Get Upload infor and convert to byte format
        HttpPostedFile myUploadFile = null;
        if (UploadControlNo == 1)
        {
            myUploadFile = FileUpload1.PostedFile;
        }

        if (UploadControlNo == 2)
        {
            myUploadFile = FileUpload2.PostedFile;
        }


        //find upload file length and convert it to byte array
        int intContentLength = myUploadFile.ContentLength;

        //create byte array
        Byte[] bytimage = new Byte[intContentLength];

        //read upload file in byte array
        myUploadFile.InputStream.Read(bytimage, 0, intContentLength);

        string myFileName = System.IO.Path.GetFileName(myUploadFile.FileName);
        string myFileExtension = myFileName.Split('.')[1];


        // *************************************************************************************
        // Devices may returned without return request ID,
        // Need to re-generate the DeviceDataTable with new created ReturnID
        // DataTable to store the serial data
        DataTable  NewdtDeviceDataTable = new DataTable();

        NewdtDeviceDataTable.Columns.Add("SerialNo", typeof(string));
        NewdtDeviceDataTable.Columns.Add("AccountNo", typeof(int));
        NewdtDeviceDataTable.Columns.Add("RequestNo", typeof(string));
        for (int i = 0; i < this.gvReview.Rows.Count; i++)
        {
            GridViewRow gvRow = gvReview.Rows[i];
            
            string ChkSerialNo = gvRow.Cells[0].Text.Replace("\n", "").ToString().Trim();

            System.Web.UI.WebControls.Label findLblAccountNo = (System.Web.UI.WebControls.Label)gvRow.FindControl("lbl_GvAccountNo");
            int ChkAccountID = int.Parse(findLblAccountNo.Text);
            //int ChkAccountID = int.Parse(gvRow.Cells[1].Text);

            int ChkRequestID = 0;
            try
            {
                var ChkRequest = (from rD in adc.rma_ReturnDevices
                               join rR in adc.rma_Returns on rD.ReturnID equals rR.ReturnID
                               where rD.SerialNo == ChkSerialNo
                               && rR.AccountID == ChkAccountID
                               && rD.Active == true
                               && rR.Active == true
                               orderby rR.CreatedDate descending
                               select rD).First();
                ChkRequestID = ChkRequest.ReturnID;
            }
            catch { ChkRequestID = 0; }

            // Create the data row to be added to the data table.
            DataRow dr = NewdtDeviceDataTable.NewRow();
            dr["SerialNo"] = ChkSerialNo;
            dr["AccountNo"] = ChkAccountID;
            dr["RequestNo"] = ChkRequestID;

            // Add the row to the DataTable.
            NewdtDeviceDataTable.Rows.Add(dr);
        }
        // *************************************************************************************
     


        // find distinct retrunID (group method)
        //var query = (from r in dtDeviceDataTable.AsEnumerable()
        //             group r by r.Field<string>(2) into groupedTable
        //             select new {id = groupedTable.Key}).ToList();

        // find distinct retrunID (distinct method)
        //var query = (from r in dtDeviceDataTable.AsEnumerable()
        //             select r.Field<string>(2)).Distinct().ToList();

        var query = (from r in NewdtDeviceDataTable.AsEnumerable()
                     select r.Field<string>(2)).Distinct().ToList();

        // save uploadfile to each returnID
        foreach (var v in query)
        {
            int ReqID;
            if (int.TryParse(v, out ReqID))
            {
                rma_UploadFile dbUploadFile = new rma_UploadFile()
                {
                    ReturnID = ReqID,
                    Filename = myFileName,
                    FileExtension = myFileExtension,
                    FileContent = bytimage,
                    CreatedBy = this.UserName,
                    CreatedDate = DateTime.Now,
                    Active = true
                };

                // Save the record to the rma_uploadFile.
                adc.rma_UploadFiles.InsertOnSubmit(dbUploadFile);
                // Save the changes.
                adc.SubmitChanges();

                // insert TransLog, update Retrun Status
                var writeTransLogUPD = adc.sp_rma_process(ReqID, 0, 0,
                              " ", this.UserName, DateTime.Now, "UPLOAD FILE", "Upload file: "
                              + myFileName + dbUploadFile.UploadFileID.ToString(), 2);
            }
        }
    } // end upload attach file



    /// <summary>
    /// Send email message to info@Quantumbadges.com and User's email account as well.
    /// </summary>
    private void CommitSendEmailNote()
    {
        // Send an email to customer service.
        int rowCount = 0;
        int returnRequestID = 0;

       

        // HTML format
        string bodyText = "<html><body>";

        string URLAccountInfo = "http://" + Request.Url.Authority.ToString() + "/InformationFinder/Details/Account.aspx?ID=";
        string URLRmaInfo = "http://" + Request.Url.Authority.ToString() + "/InformationFinder/Details/Return.aspx?ID=";

        bodyText += "<b>New devices have been received by Mfg and added to the return inventory.<br><br><br>Detail listed below:</b>";
        bodyText += "<table border='1'>";
        bodyText += "<tr><td align='center' width='60'><b>RMA#</b></td>";
        bodyText += "<td align='center'width='60'><b>Account#</b></td>";
        bodyText += "<td align='center'width='60'><b>Serial#</b></td>";
        bodyText += "<td align='center'width='220'><b>Receiving Notes</b></td></tr>";

        for (int i = 0; i < this.gvReview.Rows.Count; i++)
        {
            GridViewRow gvRow = gvReview.Rows[i];
            TextBox findTxtBox = (TextBox)gvRow.FindControl("txtNotes");
            string receiveOpsNotes = findTxtBox.Text;
            returnRequestID = 0;

            string serialNo = gvRow.Cells[0].Text.Replace("\n", "").ToString().Trim();
            
            System.Web.UI.WebControls.Label findLblAccountNo = (System.Web.UI.WebControls.Label)gvRow.FindControl("lbl_GvAccountNo");
            int accountID = int.Parse(findLblAccountNo.Text);

            //int accountID = int.Parse(gvRow.Cells[1].Text);
            System.Web.UI.WebControls.Label findLblRequestNo = (System.Web.UI.WebControls.Label)gvRow.FindControl("lbl_GvRequestNo");
            int.TryParse(findLblRequestNo.Text, out returnRequestID);
            //int.TryParse(gvRow.Cells[2].Text, out returnRequestID);

            // check NEW RMA# for device return without Return Request 
            if (returnRequestID == 0)
                try
                {
                    var RDevice = (from rD in adc.rma_ReturnDevices
                                   join rR in adc.rma_Returns
                                   on rD.ReturnID equals rR.ReturnID
                                   where rD.SerialNo == serialNo
                                   && rR.AccountID == accountID
                                   && rD.Active == true
                                   && rR.Active == true
                                   select rD).First();
                    returnRequestID = RDevice.ReturnID;
                }
                catch { returnRequestID = 0; }
            
            // Append the record.
            bodyText += "<tr>";
            if (returnRequestID != 0)
            {
                bodyText += "<td td align='center'><a href='" + URLRmaInfo + returnRequestID.ToString().Trim() + "'> " + returnRequestID.ToString().Trim() + "</a> </td>";
            }
            else
            {
                bodyText += "<td td align='center'> N/A </td>";
            }

            bodyText += "<td align='center'><a href='" + URLAccountInfo + accountID.ToString().Trim() + "#Return_tab'> " + accountID.ToString().Trim() + "</a> </td>";
            bodyText += "<td align='center'> " + serialNo.Trim().PadRight(10, ' ') + " </td>";


            // FLAG CS or Finance that received device is damage or broken
            if ((receiveOpsNotes.ToUpper().IndexOf("DAMAGE") == -1) && (receiveOpsNotes.ToUpper().IndexOf("BROKEN") == -1))
            {
                bodyText += "<td td align='left' valign='top'> " + receiveOpsNotes + " </td>";
            }
            else
            {
                bodyText += "<td td align='left' valign='top' bgcolor='orangered'> " + receiveOpsNotes + " </td>";
            }

            
            bodyText += "</tr>";
            rowCount++;
        }


        bodyText += "</table>";
        bodyText += "<table><tr><td>";

        // FLAG CS or Finance that received device/s with NO request#
        if (Session["returnRequestID"] != null) 
        {
            bodyText += "<br><br><font color='red'><b>Device was mailed back <u>without</u> RMA#, Check-in by Mfg<br> RMA# <u>" + Session["returnRequestID"].ToString() + "</u> is created by Receiving Department.</b></font>";
        }

        bodyText += "<br><br><font size='-1'>This email is generated by RMA Portal. <br>User: <b>" + this.UserName + "</b></font>";
        bodyText += "</td></tr></table>";
        bodyText += "</body></html>";


        if (rowCount > 0)
        {
            // Send email.

            string smtpServer = string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["SmtpServer"]) ? "10.200.2.16" : System.Configuration.ConfigurationManager.AppSettings["SmtpServer"];
            SmtpClient client = new SmtpClient(smtpServer);
            MailMessage mail = new MailMessage();
            mail.IsBodyHtml = true;
            
            mail.From = new MailAddress("noreply@instadose.com", "RMA Portal Email");
            
            string MailSubject = "";

            if (Session["returnRequestID"] != null)
            { MailSubject = "Devices received by Mfg -- UnAuthorized RMA#" + returnRequestID.ToString(); }
            else
            { MailSubject = "Devices received by Mfg -- RMA#" + returnRequestID.ToString();  }

            
            mail.Subject = MailSubject;
            mail.Body = bodyText;

            // email recipients To or CC or Bcc 
            string userEmail = this.UserName + "@mirion.com";
            mail.To.Add(userEmail);
            
            if (DevelopmentServer == false)
            {
                mail.To.Add("info@QuantumBadges.com");  // email to CS department
                mail.To.Add("irv-FinanceReturns@mirion.com");  // email to Finance Group 
            }

            try
            {
                client.Send(mail);

                // insert TransLog, Email sent
                var writeTransLogUPD = adc.sp_rma_process(returnRequestID, 0, 0,
                        " ", this.UserName, DateTime.Now, "EMAIL RECEIVING", "Receiving Email sent to ALL departmetns",2);
            }
            catch{}
        }
    }

    private void Commit()
    {
        // Get the serial numbers information and the Notes from Receiving
        // from GridView  to commit to the inventory

        // store RetrunID and accountID in session variable for mutiple devices receive 
        // which without returnID but same accountID


        // USE variables instead of session variables.
        //Session.Contents.RemoveAll();  // clear all session variables.
        //Session["accountID"] = 0;

        string prevAccountID = "0";
        string prevReturnRequestID = "0";

        for (int i = 0; i < this.gvReview.Rows.Count; i++)
        {
            GridViewRow gvRow = gvReview.Rows[i];
            TextBox findTxtBox = (TextBox)gvRow.FindControl("txtNotes");

            string receiveOpsNotes = findTxtBox.Text;

            int returnRequestID = 0;
            int newReturnDevicesID = 0;
            int newReturnInventoryID = 0;

            string serialNo = gvRow.Cells[0].Text.Replace("\n", "").ToString().Trim();

            System.Web.UI.WebControls.Label findLblAccountNo = (System.Web.UI.WebControls.Label)gvRow.FindControl("lbl_GvAccountNo");
            int accountID = int.Parse(findLblAccountNo.Text);

            System.Web.UI.WebControls.Label findLblRequestNo = (System.Web.UI.WebControls.Label)gvRow.FindControl("lbl_GvRequestNo");
            int.TryParse(findLblRequestNo.Text, out returnRequestID);
           
            // Check status of Device, received? or not received?
            //rma_ReturnInventory riTest = getReturnInventoryList(serialNo, returnRequestID);

            // Make sure the device not in ReturnInventory 
            rma_ReturnInventory riTest = getReturnInventory(serialNo);

            if (riTest == null)
            {
                //store RetrunID and accountID in session variable for mutiple devices which without returnID but same accountID

                //if (Session["returnRequestID"] != null && Session["accountID"].ToString() == accountID.ToString())
                //{
                //    int.TryParse(Session["returnRequestID"].ToString(), out returnRequestID);
                //}
                if (prevReturnRequestID != "0" && prevAccountID == accountID.ToString())
                {
                    int.TryParse(prevReturnRequestID, out returnRequestID);
                }

                // Get the Return Request devices count...
                int returnRequestCount = (from r in adc.rma_Returns
                                          join d in adc.rma_ReturnDevices
                                          on r.ReturnID equals d.ReturnID
                                          where r.AccountID == accountID && r.Active == true
                                            && d.SerialNo == serialNo && d.Active == true
                                            && d.Received==false 
                                          select r).Count();


                // If a Return Request has not been created for the account 
                // and accountID <> sessionID accountID value

                //if (returnRequestCount <= 0 && accountID.ToString() != Session["accountID"].ToString())
                if (returnRequestCount <= 0 && accountID.ToString() != prevAccountID)
                {
                    // Create Return Request for account and get the new generated ReturnID
                    rma_Return rma = null;
                    rma = new rma_Return()
                    {
                        AccountID = accountID,
                        Active = true,
                        CreatedBy = this.UserName,
                        CreatedDate = DateTime.Now,
                        Notes = "Auto generated by inventory entry.",
                        Reason = "Devices arrived at receiving facility without request#.",
                        ReturnTypeID = 5, // Device was mailed back without Request No, Check-in by Mfg
                        Status = 1, // Awaiting all devices returned.
                        Return_ReasonID = 7 //Devices arrived at facility without Request No, add by Mfg

                    };
                    adc.rma_Returns.InsertOnSubmit(rma);
                    adc.SubmitChanges();

                    // insert TransLog, add new Retrun
                    returnRequestID = rma.ReturnID;

                    var writeTransLogAddDev = adc.sp_rma_process(returnRequestID, 0, 0,
                              "Auto generated by inventory entry.", this.UserName, DateTime.Now, "ADD RETRUN",
                              "Device was mailed back without Request number.", 2);

                    // store RetrunID and accountID in session variable for mutiple devices which without returnID but same accountID

                    //Session["returnRequestID"] = rma.ReturnID;
                    //Session["accountID"] = accountID;
                    prevReturnRequestID = returnRequestID.ToString();
                    prevAccountID = accountID.ToString();

                }




                // serial number exist in the return request details?
                int returnDeviceCount = (from rd in adc.rma_ReturnDevices
                                         where rd.ReturnID == returnRequestID 
                                         && rd.Active == true
                                         && rd.SerialNo == serialNo
                                         && rd.Received == false 
                                         select rd).Count();


                rma_ReturnDevice returnDevice = null;

                // Set received = true if the device was initially returned.
                if (returnDeviceCount >= 1) // If the device was found...
                {
                    // Yes, update it to received.
                    returnDevice = (from rd in adc.rma_ReturnDevices
                                    where rd.ReturnID == returnRequestID &&
                                    rd.SerialNo == serialNo && rd.Active == true
                                    && rd.Received == false 
                                    select rd).First();

                    returnDevice.Status = 4; // Device(s) Received and is waiting to be reviewed.
                    returnDevice.DepartmentID = 7; // Mfg - Receiving
                    returnDevice.Received = true;

                    // Save changes to the database
                    adc.SubmitChanges();

                    // insert TransLog, update devices status
                    newReturnDevicesID = returnDevice.ReturnDevicesID;
                    var writeTransLogUpdDev = adc.sp_rma_process(returnRequestID, newReturnDevicesID, 0,
                              " ", this.UserName, DateTime.Now, "UPDATE DEVICE",
                              "DeviceStatus=4, DepartmentID=7", 2);

                }
                else    // if the device was not initially returned
                {
                    
                    var MDeviceID = (from a in idc.DeviceInventories
                                     where a.SerialNo == serialNo
                                     select a).First();

                    returnDevice = new rma_ReturnDevice()
                    {
                        Active = true,
                        Notes = "Auto generated by inventory entry.",
                        ReturnID = returnRequestID,
                        MasterDeviceID = MDeviceID.DeviceID,
                        SerialNo = serialNo,
                        DepartmentID = 7, // Device is in Receiving dept.
                        Received= true, // indicated received device
                        Status = 4 // Device(s) Received and is waiting to be reviewed.
                    };

                    // Create the new returned device
                    adc.rma_ReturnDevices.InsertOnSubmit(returnDevice);
                    // Save changes to the database
                    adc.SubmitChanges();

                    // insert TransLog, add new devices
                    newReturnDevicesID = returnDevice.ReturnDevicesID;

                    var writeTransLogAddDev = adc.sp_rma_process(returnRequestID, newReturnDevicesID, 0,
                              "Auto generated by inventory entry.", this.UserName, DateTime.Now, "ADD DEVICE",
                              "DeviceStatus=4, DepartmentID=7", 2);

                }




                // Create the return inventory entry. (Return Inventory table)
                rma_ReturnInventory inventory = new rma_ReturnInventory()
                {
                    Active = true,
                                     
                    ReturnID = returnRequestID,
                    ReturnDeviceID = returnDevice.ReturnDevicesID,
                    SerialNo = serialNo,

                    CreatedDate = DateTime.Now,
                    ReceivedNotes= receiveOpsNotes,
                    ReceivedBy = this.UserName,
                    ReceivedDate = DateTime.Now,

                    Approved = false,
                    Completed = false,
                    TechOpsApproved = false
                };

                // Add the new invetory item.
                adc.rma_ReturnInventories.InsertOnSubmit(inventory);
                // Save changes to the database
                adc.SubmitChanges();

                // insert TransLog, add new devices
                newReturnInventoryID = inventory.ReturnInventoryID;
                var writeTransLogInv = adc.sp_rma_process(returnRequestID, newReturnDevicesID, newReturnInventoryID,
                               receiveOpsNotes, this.UserName, DateTime.Now, "ADD INVENTORY", "Add inventory Serial#: " + serialNo, 2);



                // Refresh the data source
                adc.Refresh(System.Data.Linq.RefreshMode.KeepChanges);




                // Have all of the devices been received in the return request?
                try
                {
                    // Requery the return and check to make sure the status is currently set to "Awaiting devices.." (1)
                    var returnRequest = (from r in adc.rma_Returns
                                         where r.ReturnID == returnRequestID && r.Active == true
                                         select r).First();

                    // Get the Return Request devices count...
                    int returnRequestNewCount = (from r in adc.rma_Returns
                                                 join d in adc.rma_ReturnDevices
                                                 on r.ReturnID equals d.ReturnID
                                                 where r.ReturnID == returnRequestID && r.Active == true
                                                 && d.Active == true
                                                 select r).Count();


                    // Get a count of the inventory items for the return, based on the return ID
                    int inventoryCount = (from ri in adc.rma_ReturnInventories
                                          where ri.Active == true &&
                                          ri.ReturnID == returnRequestID
                                          select ri).Count();

                    // Do they have the same number of items?
                    if (returnRequestNewCount == inventoryCount)
                    {
                        // Yes, update the return request status to ... received.
                        returnRequest.Status = 6; // All device(s) Received by Mfg and is waiting to be reviewed.

                        // Save changes.
                        adc.SubmitChanges();

                        // insert TransLog, update Retrun Status
                        var writeTransLogUPD = adc.sp_rma_process(returnRequestID, 0, 0,
                                      " ", this.UserName, DateTime.Now, "UPDATE RETURN", "Update Request status to 6.", 2);

                    }
                    else
                    {
                        // No, update the return request status to partial received.
                        returnRequest.Status = 5;  //Partial device(s) have been received by Mfg

                        // save changes.
                        adc.SubmitChanges();

                        // insert TransLog, update Retrun Status
                        var writeTransLogUPD = adc.sp_rma_process(returnRequestID, 0, 0,
                                      " ", this.UserName, DateTime.Now, "UPDATE RETURN", "Update Request status to 5.", 2);
                    }
                }

                catch { }


                // Deactivate and remove from accountdevice record.
                // and un-assign user
                try
                {
                    DeviceInventory deviceInventory = getDeviceInventory(serialNo);
                    
                    // Deactivate UserDevice
                    UserDevice UDev = (from a in idc.UserDevices
                                       join ad in idc.AccountDevices 
                                            on a.DeviceID equals ad.DeviceID 
                                       where a.DeviceID == deviceInventory.DeviceID
                                       && a.Active == true && ad.CurrentDeviceAssign == true 
                                       select a).FirstOrDefault();
                    if (UDev != null)
                    {
                        UDev.DeactivateDate = DateTime.Now;
                        UDev.Active = false;
                        idc.SubmitChanges();
                    }
                    
                    //Deactivate and remove from AccountDevice
                    AccountDevice ADev = (from a in idc.AccountDevices
                                          where a.DeviceID == deviceInventory.DeviceID & a.CurrentDeviceAssign == true
                                          select a).FirstOrDefault();
                    if (ADev != null)
                    {
                        ADev.Active = false;
                        ADev.DeactivateDate = DateTime.Now;
                        ADev.CurrentDeviceAssign = true;
                        idc.SubmitChanges();
                    }

                    // insert TransLog
                    var writeTransLogUPD = adc.sp_rma_process(returnRequestID, 0, 0,
                                  " ", this.UserName, DateTime.Now, "DEACTIVATE", "Deactive Serial# " + serialNo , 2);
                  
                }
                catch { }

            } // close if riTest
        } // end Forloop
    } // END Commit()

    /// <summary>
    /// use a sharp background color to flag receiving department
    /// that the received device does not associated to any REQUEST. 
    /// </summary>
    /// <param name="requestNo"></param>
    /// <returns> String background color</returns>
    public System.Drawing.Color  FuncCheckRequestNo(string requestNo)
    {
        System.Drawing.Color  myColor = System.Drawing.Color.White  ;

        if (requestNo == "None")
        {
            myColor = System.Drawing.Color.OrangeRed;
        }
        return myColor;

    }

    /// <summary>
    /// use a sharp background color to flag receiving department
    /// that the received device does not associated to any ACCOUNT. 
    /// </summary>
    /// <param name="AccountNo"></param>
    /// <returns> String background color</returns>
    public System.Drawing.Color FuncCheckAccountNo(string AccountNo)
    {
        System.Drawing.Color myColor = System.Drawing.Color.White;

        if (AccountNo == "0")
        {
            myColor = System.Drawing.Color.OrangeRed ;
        }
        return myColor;

    }

    private void InvisibleErrors()
    {
        // Reset submission form error message      
        this.errorMsg.InnerText = "";
        this.errors.Visible = false;
    }

    private void VisibleErrors(string error)
    {
        this.errorMsg.InnerText = error;
        this.errors.Visible = true;
    }

    private void InvisibleMessages()
    {
        // Reset submission form error message      
        this.submitMsg.InnerText = "";
        this.messages.Visible = false;
    }

    private void VisibleMessages(string error)
    {
        this.submitMsg.InnerText = error;
        this.messages.Visible = true;
    }

    private void InvisibleErrorReview()
    {
        // Reset submission form error message      
        this.errorReviewMsg.InnerHtml  = "";
        this.errorReview.Visible = false;
    }

    private void VisibleErrorReview(string error)
    {
        this.errorReviewMsg.InnerHtml = error;
        this.errorReview.Visible = true;
    }

}
