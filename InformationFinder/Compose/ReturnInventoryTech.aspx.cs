using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Instadose;
using Instadose.Data;

public partial class Instadose_InformationFinder_Compose_ReturnInventoryTech : System.Web.UI.Page
{
    // Create the database reference
    InsDataContext idc = new InsDataContext();
    AppDataContext adc = new AppDataContext();

    // Create data table for devices
    DataTable dtDeviceDataTable = null;

    //Create data tabe for edit devices comment 9/24/2010
    DataTable dtDeviceDataTableEdit = null;

    // String to hold the current username
    string UserName = "Unknown";

    protected void Page_Load(object sender, EventArgs e)
    {
        Page.Title = "RMA - Technical REceipt of Devices";

        // Try to set the username
        try
        {
            this.UserName = User.Identity.Name.Split('\\')[1];
            this.lblusername.Text = this.UserName;
            this.lblusernameEdit.Text = this.UserName;

            InvisibleErrors();
            InvisibleMessages();
            InvisibleErrorReview();
            InvisibleErrorReviewEdit();
        }
        catch 
        { 
            this.UserName = "Unknown";
            this.lblusername.Text = this.UserName;
            this.lblusernameEdit.Text = this.UserName;
        }
    }

    protected void btnContinue_Click(object sender, EventArgs e)
    {
        // Hide the first panel and display the secondary panel.
        this.plAdd.Visible = false;
        this.plReview.Visible = true;
        this.plReviewEdit.Visible = false;

        // Store the list of serial numbers that the user has added.
        List<string> serialNumberList = new List<string>();
        serialNumberList.AddRange(this.txtSerialNos.Text.Split('\n'));

        // Load the serial number data
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
        this.plReviewEdit.Visible = false;
    }


    protected void btnFinish_Click(object sender, EventArgs e)
    {
        // Create the entries.
         this.Commit();

        // Create upload attachment record to unique requestID
        if (FileUpload1.HasFile)
            this.CommitUploadAttachFile(1);

        if (FileUpload2.HasFile)
            this.CommitUploadAttachFile(2);

        // Send Email
        this.CommitSendEmailNote();

        // Redirect back to first page, with a success message.        
        this.VisibleMessages("Devices have been added and approved.");  
        this.txtSerialNos.Text = "";
        this.plAdd.Visible = true;
        this.plReview.Visible = false;
        this.plReviewEdit.Visible = false;
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
                      where ad.DeviceID  == DeviceID
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
                               && ri.ReturnID==ReturnID 
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


    private rma_ReturnInventory getReturnInventory( string serialNumber)
    {
        // Create the object to return.
        rma_ReturnInventory returnInventory = null;
        try
        {
            // Find the return inventory filter out completed items, 
            // and with an active rma request, since
            // a device could come through the return process many times
            // throughout the devices life.
            returnInventory = (from ri in adc.rma_ReturnInventories
                               join rRet in adc.rma_Returns
                                  on ri.ReturnID equals rRet.ReturnID
                               where ri.SerialNo == serialNumber
                                && ri.Completed == true && ri.Approved==true
                                && ri.Active == true
                                && rRet.Active == true  // in an Active Request.
                                //&& ri.CommittedToFinance == null // tdo, 4/3/2012
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
                            join rds in adc.rma_ref_ReturnDeviceStatus 
                            on rd.Status equals rds.ReturnDeviceStatusID 
                            where rd.SerialNo == serialNumber && rd.Active == true
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
        dtDeviceDataTable.Columns.Add("RequestType", typeof(string));
        dtDeviceDataTable.Columns.Add("DeviveStatus", typeof(string));
        dtDeviceDataTable.Columns.Add("Action", typeof(string));
        dtDeviceDataTable.Columns.Add("Type", typeof(string));
        dtDeviceDataTable.Columns.Add("DepartmentRequest", typeof(string));

        // Ensure that least one device was added.
        if (serialNumberList.Count > 0)
        {
            // Loop through each serial number.
            foreach (string serialNumber in serialNumberList)
            {
               
                if (serialNumber.Trim() != "")
                {
                    // Find the inventory for the devices and the device info.
                    DeviceInventory deviceInventory = getDeviceInventory(serialNumber.Trim());

                    // Find if the device has been assigned to any account.
                    AccountDevice accountDevice = getAccountDevice(deviceInventory.DeviceID);

                    // Find if the device has already been RECEIVED and reviewed by Tech Ops yet
                    rma_ReturnInventory returnInventory = getReturnInventory(serialNumber.Trim());

                   
                    // Find if the device has already initially requested return and waiting for receiving.
                    // This is when the device has not RECEIVED by receiving dept.
                    rma_ReturnDevice initReturnDevice = (from rd in adc.rma_ReturnDevices
                                                     where rd.SerialNo == serialNumber.Trim()
                                                     && rd.Active == true
                                                     && rd.Received == false
                                                     select rd).FirstOrDefault();

                    // Find if device was received and scanned by Receiving Department
                    // Tech-Ops only picks returned device after it is received and scanned by receiving dept.
                    rma_ReturnDevice returnDevice = null;
                    returnDevice = (from rd in adc.rma_ReturnDevices
                                    join rinv in adc.rma_ReturnInventories
                                    on rd.ReturnDevicesID equals rinv.ReturnDeviceID
                                    where rd.SerialNo == serialNumber.Trim()
                                    && rd.Active == true
                                    && rd.Received == true
                                    && rinv.Completed == false
                                    select rd).FirstOrDefault();

                    // tdo, 4/3/2012
                    if (returnDevice != null)
                    {
                        // Get the default or standards
                        int accountNo = 0;

                        accountNo = (from rt in adc.rma_Returns
                                     where rt.ReturnID == returnDevice.ReturnID
                                     select rt.AccountID).First();
                        string requestNo = returnDevice.ReturnID.ToString();
                        string DeviceStatus = returnDevice.rma_ref_ReturnDeviceStatus.Status.ToString();
                        string requestType = (from r in adc.rma_Returns
                                              join rt in adc.rma_ref_ReturnTypes on r.ReturnTypeID equals rt.ReturnTypeID
                                              where r.ReturnID == returnDevice.ReturnID
                                              select rt.Type).FirstOrDefault(); 

                        // Create the data row to be added to the data table.
                        DataRow dr = dtDeviceDataTable.NewRow();

                        dr["SerialNo"] = serialNumber;
                        dr["AccountNo"] = accountNo;
                        dr["RequestNo"] = requestNo;
                        dr["RequestType"] = (requestType=="Recall") ? "Recall": "Return";
                        dr["DeviveStatus"] = DeviceStatus;

                        // Get Request Department
                        rma_ref_Department returnDepartment = getRequestDepartment(requestNo);
                        string ReqDepartment = returnDepartment.Name.ToString();
                        dr["DepartmentRequest"] = ReqDepartment;
                        dr["Action"] = "Will be checked in, then approved and completed process.";
                        dr["Type"] = "Device has been received for a return.";

                        // Add the row to the DataTable.
                        dtDeviceDataTable.Rows.Add(dr);
                    }
                    else if(deviceInventory == null)  // Could not find the serial number.
                        errorList.Add(serialNumber, "Serial number does not exist in the Instadose system.");
                    else if(accountDevice == null)    // Device has not ever assigned to any account.
                        errorList.Add(serialNumber, "Device does not belong to any account.");
                    else if (initReturnDevice != null)    // Device has not received by Receiving Department.
                        errorList.Add(serialNumber, "Device has not received by Receiving Department.");
                    else if (returnInventory != null)    // It is showing device has already been RECEIVED and reviewed by Tech Ops.
                        errorList.Add(serialNumber, "Device is already in Tech Ops' INV or Device has not received by Receiving Department.");
                    else // Possibly the device was not initially returned and has not received by Receiving Department.
                        errorList.Add(serialNumber, "Device has not received by Receiving Department.");


                    
                    //// If the device was found and received/scanned by receiving department and 
                    //if (deviceInventory != null && returnDevice != null && returnInventory == null)
                    //{
   
                    //    // Get the default or standards
                    //    int accountNo = 0;

                    //    accountNo = (from rt in adc.rma_Returns
                    //                 where rt.ReturnID == returnDevice.ReturnID
                    //                 select rt.AccountID).First();
                    //    string requestNo = returnDevice.ReturnID.ToString() ;
                    //    string DeviceStatus = returnDevice.rma_ref_ReturnDeviceStatus.Status.ToString() ;

                    //    //if (returnDevice != null)
                    //    //    accountNo = (from rt in adc.rma_Returns
                    //    //                 where rt.ReturnID == returnDevice.ReturnID
                    //    //                 select rt.AccountID).First();
                    //    //else
                    //    //    accountNo = (accountDevice != null) ? accountDevice.AccountID : 0;

                    //    ////int accountNo = (accountDevice != null) ? accountDevice.AccountID.Value : 0;
                    //    //string requestNo = (returnDevice != null) ? returnDevice.ReturnID.ToString() : "None";
                    //    //string DeviceStatus = (returnDevice != null) ? returnDevice.rma_ref_ReturnDeviceStatus.Status.ToString() : "None";



                    //    // Create the data row to be added to the data table.
                    //    DataRow dr = dtDeviceDataTable.NewRow();

                    //    dr["SerialNo"] = serialNumber;
                    //    dr["AccountNo"] = accountNo;
                    //    dr["RequestNo"] = requestNo;
                    //    dr["DeviveStatus"] = DeviceStatus;

                    //    // Get Request Department
                    //    rma_ref_Department returnDepartment = getRequestDepartment(requestNo);
                    //    string ReqDepartment = returnDepartment.Name.ToString();
                    //    dr["DepartmentRequest"] = ReqDepartment;

                    //    //if (requestNo != "None")
                    //    //{
                    //    //    rma_ref_Department returnDepartment = getRequestDepartment(requestNo);
                    //    //    string ReqDepartment = returnDepartment.Name.ToString();
                    //    //    dr["DepartmentRequest"] = ReqDepartment;
                    //    //}
                    //    //else
                    //    //{ dr["DepartmentRequest"] = ""; }
                    //    ////  ENDGet Request Department


                    //    dr["Action"] = "Will be checked in, then approved and completed process.";
                    //    dr["Type"] = "Device has been received for a return.";
                    //    //if (returnInventory != null)
                    //    //{
                    //    //    //dr["Action"] = "Will be approved and completed process";
                    //    //    //dr["Type"] = "";
                    //    //}
                    //    //// Notify the user of what will happen to the device.
                    //    //else if (accountNo == 0)
                    //    //{
                    //    //    // Device does not have an account association.
                    //    //    dr["Action"] = "A general request will be created for this device.";
                    //    //    dr["Type"] = "Device does not belong to any account.";
                    //    //}
                    //    //else
                    //    //{
                    //    //    // Device has an account association.
                    //    //    if (requestNo == "None")
                    //    //    {
                    //    //        dr["Action"] = "A request will be generated for this account, and Will be checked in, then approved and completed process.";
                    //    //        dr["Type"] = "This return was unexpected. A return was generated for this account.";
                    //    //    }
                    //    //    else
                    //    //    {
                    //    //        dr["Action"] = "Will be checked in, then approved and completed process.";
                    //    //        dr["Type"] = "Device has been received for a return.";
                    //    //    }
                    //    //}

                    //    // Add the row to the DataTable.
                    //    dtDeviceDataTable.Rows.Add(dr);
                    //}

                    //else if (deviceInventory != null && returnInventory != null)
                    //    errorList.Add(serialNumber, "Device has already been approved. No other changes can be made.");
                    
                    //else if (accountDevice == null && returnDevice == null)
                    //    errorList.Add(serialNumber, "Device does not assign to any account & device has not received by Receiving Department.");

                    //else if (accountDevice == null || returnDevice == null)
                    //{
                    //    if (accountDevice == null)
                    //    // Device does not have an account association.
                    //    errorList.Add(serialNumber, "Device does not belong to any account.");
                    //    if (returnDevice == null)
                    //        errorList.Add(serialNumber, "Device has not received by Receiving Department. Or device is already in Tech Ops' INV.");
                    //}

                    //else // Could not find the serial number.
                    //    errorList.Add(serialNumber, "Serial number does not exist in the Instadose system.");
                    
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
        // DataTable to store the serial# and returnID data
        DataTable NewdtDeviceDataTable = new DataTable();

        NewdtDeviceDataTable.Columns.Add("SerialNo", typeof(string));
        NewdtDeviceDataTable.Columns.Add("AccountNo", typeof(int));
        NewdtDeviceDataTable.Columns.Add("RequestNo", typeof(string));

        for (int i = 0; i < this.gvReview.Rows.Count; i++)
        {
            GridViewRow gvRow = gvReview.Rows[i];

            string ChkSerialNo = gvRow.Cells[0].Text.Replace("\n", "").ToString().Trim();
            int ChkAccountID = int.Parse(gvRow.Cells[1].Text);
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

        // HTML format
        string bodyText = "<html><body>";

        string URLAccountInfo = "http://" + Request.Url.Authority.ToString() + "/InformationFinder/Details/Account.aspx?ID=";
        string URLRmaInfo = "http://" + Request.Url.Authority.ToString() + "/InformationFinder/Details/Return.aspx?ID=";

        bodyText += "<b>New devices have been received and added to the return inventory by Tech.<br><br><br>Details listed below:</b>";
        bodyText += "<table border='1'>";
        bodyText += "<tr><td align='center' width='60'><b>RMA#</b></td>";
        bodyText += "<td align='center'width='60'><b>Account#</b></td>";
        bodyText += "<td align='center'width='60'><b>Serial#</b></td>";
        bodyText += "<td align='center'width='220'><b>Notes</b></td></tr>";

        for (int i = 0; i < this.gvReview.Rows.Count; i++)
        {
            GridViewRow gvRow = gvReview.Rows[i];
            TextBox findTxtBox = (TextBox)gvRow.FindControl("txtNotes");
            string techOpsNotes = findTxtBox.Text;
            int returnRequestID = 0;

            string serialNo = gvRow.Cells[0].Text.Replace("\n", "").ToString().Trim();
            int accountID = int.Parse(gvRow.Cells[1].Text);
            int.TryParse(gvRow.Cells[2].Text, out returnRequestID);

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
                bodyText += "<td align='center'><a href='" + URLRmaInfo + returnRequestID.ToString().Trim() + "'> " + returnRequestID.ToString().Trim() + "</a> </td>";
            }
            else
            {
                bodyText += "<td align='center'> N/A </td>";
            }

            bodyText += "<td td align='center' valign='top'><a href='" + URLAccountInfo + accountID.ToString().Trim() + "#Return_tab'> " + accountID.ToString().Trim() + "</a> </td>";
            bodyText += "<td td align='center' valign='top'> " + serialNo.Trim().PadRight(10, ' ') + " </td>";
            bodyText += "<td td align='left' valign='top'> " + techOpsNotes + " </td>";
            bodyText += "</tr>";
            rowCount++;
        }

        bodyText += "</table>";
        bodyText += "</body></html>";

       if (rowCount > 0)
        {
            // Send the email.
            string smtpServer = string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["SmtpServer"]) ? "10.200.2.16" : System.Configuration.ConfigurationManager.AppSettings["SmtpServer"];
            SmtpClient client = new SmtpClient(smtpServer);

            // email to user
            string userEmail;
            userEmail = this.UserName + "@mirion.com";
            MailMessage msg2 = new System.Net.Mail.MailMessage("noreply@instadose.com", userEmail, "Devices received and added to Return Inventory by Tech.", bodyText);
            
            msg2.IsBodyHtml = true;

            try
            {
                client.Send(msg2);
            }
            catch { }
        }
    }

    /// <summary>
    /// Commits the devices to the inventory.
    /// </summary>
    private void Commit()
    {
        // Get the serial numbers information and the Tech input Notes 
        // from GridView  to commit to the inventory
        
        for (int i = 0; i < this.gvReview.Rows.Count; i++)
        {
            GridViewRow gvRow = gvReview.Rows[i];
            TextBox findTxtBox = (TextBox)gvRow.FindControl("txtNotes");
            CheckBox findChkBx = (CheckBox ) gvRow.FindControl("ChkBxApprove");

            CheckBox findChkBxVisual = (CheckBox)gvRow.FindControl("ChkBxVisualInspect");
            CheckBox findChkBxData = (CheckBox)gvRow.FindControl("ChkBxDataInspect");

            string techOpsNotes = findTxtBox.Text;
            
            int returnRequestID = 0;
            int newReturnDevicesID = 0;
            int newReturnInventoryID = 0;

            string serialNo = gvRow.Cells[0].Text.Replace("\n","").ToString().Trim();
            int accountID = int.Parse(gvRow.Cells[1].Text);
            int.TryParse(gvRow.Cells[2].Text, out returnRequestID);
            bool boolChkApprove = findChkBx.Checked;
            bool boolChkVisualInspect = findChkBxVisual.Checked;
            bool boolChkDataInspect = findChkBxData.Checked;

            // Check status of Device, received? or not received?
            rma_ReturnInventory riTest = getReturnInventoryList(serialNo, returnRequestID );
            
            if (riTest != null) // not Null, status = already received by Mfg
            {
                // -------------- update Device with Tech ops notes (rma_returnInventory ) -------------------//
                rma_ReturnInventory returnInventory = null;

                returnInventory = (from ri in adc.rma_ReturnInventories
                                   where ri.ReturnID == returnRequestID &&
                                   ri.SerialNo == serialNo && ri.Active == true
                                   select ri).First();
                              
                returnInventory.TechOpsNotes = techOpsNotes;
                returnInventory.TechOpsReviewer = this.UserName;
                returnInventory.TechOpsReviewDate = DateTime.Now;

                returnInventory.VisualInspectPass = boolChkVisualInspect;
                returnInventory.DataInspectPass = boolChkDataInspect;
                returnInventory.TechOpsApproved = boolChkApprove;
                returnInventory.Approved = boolChkApprove;

                returnInventory.Active = true;
                returnInventory.Completed = true;
                returnInventory.CommittedToFinance = true;

                returnInventory.CommittedDate = DateTime.Now;

                //save change to database
                adc.SubmitChanges();
                // -------------- update Device with Tech ops notes (rma_returnInventory ) -------------------//

                // --------------- insert TransLog, update return devices status -------------------------------------//
                newReturnDevicesID = returnInventory.ReturnDeviceID;
                var writeTransLogUpdInv = adc.sp_rma_process(returnRequestID, newReturnDevicesID,
                    returnInventory.ReturnInventoryID, techOpsNotes,
                    this.UserName, DateTime.Now, "UPDATE INV TECH", "Approved=" + boolChkApprove.ToString() , 2);
                
                // Update return device status to received by tech.
                var UpdateReturnDevice = (from rd in adc.rma_ReturnDevices
                                where rd.ReturnID == returnRequestID &&
                                rd.SerialNo == serialNo && rd.Active == true
                                select rd).First();

                UpdateReturnDevice.Status = 7; // Device is received and inspected by Tech .
                UpdateReturnDevice.DepartmentID = 8; // Device is in Tech Receiving location

                adc.SubmitChanges();
                
                // insert TransLog, update devices status
                newReturnDevicesID = UpdateReturnDevice.ReturnDevicesID;
                var writeTransLogUpdDev = adc.sp_rma_process(returnRequestID, newReturnDevicesID, 0,
                          " ", this.UserName, DateTime.Now, "UPDATE DEVICE TECH",
                          "Status=7, DepartmentID=8", 2);
                // --------------- insert TransLog, update return devices status -------------------------------------//



                // Have all of the devices been received in the return request?
                // ******************** Update return status to: All Device(s) received/Partiall device(s) received status ************
                try
                {
                    // Requery the return and check to make sure the status is currently set to "Awaiting devices.." (1)
                    var returnRequest = (from r in adc.rma_Returns
                                         where r.ReturnID == returnRequestID && r.Active == true
                                         select r).First();

                    // Get the Return Request devices count...
                    int returnRequestCount = (from r in adc.rma_Returns
                                              join d in adc.rma_ReturnDevices
                                              on r.ReturnID equals d.ReturnID
                                              where r.ReturnID == returnRequestID && r.Active == true
                                              && d.Active == true
                                              select r).Count();


                    // Get a count of the inventory items for the return, based on the return ID
                    // and received/reviewed by Tech department 
                    int inventoryCount = (from ri in adc.rma_ReturnInventories
                                          where ri.Active == true &&
                                          ri.ReturnID == returnRequestID
                                          && ri.TechOpsReviewer != null
                                          select ri).Count();

                    // Do they have the same number of items?
                    if (returnRequestCount == inventoryCount)
                    {
                        // Yes, update the return request status to ... received.
                        returnRequest.Status = 8; // All Device(s) received and inspected by Tech. The return is complete.

                        // Save changes.
                        adc.SubmitChanges();

                        // insert TransLog, update Retrun Status
                        var writeTransLogUPD = adc.sp_rma_process(returnRequestID, 0, 0,
                                      " ", this.UserName, DateTime.Now, "UPDATE RETURN", "Update Request status to 8, RMA completed.", 2);

                    }
                    else
                    {
                        // No, update the return request status to partial received.
                        returnRequest.Status = 7;  // Partiall device(s) received and inspected by Tech. 

                        // save changes.
                        adc.SubmitChanges();

                        // insert TransLog, update Retrun Status
                        var writeTransLogUPD = adc.sp_rma_process(returnRequestID, 0, 0,
                                      " ", this.UserName, DateTime.Now, "UPDATE RETURN", "Update Request status to 7.", 2);
                    }
                }

                catch { }
                // ******************** Update return status to: All Device(s) received/Partiall device(s) received status ************

                // ************* Deactivate the userdevice and accountdevice record **************************
                // and remove a device off the account
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
                                          where a.DeviceID == deviceInventory.DeviceID && a.CurrentDeviceAssign == true
                                          select a).FirstOrDefault();
                    if (ADev != null)
                    {
                        ADev.Active = false;    //Deactivate
                        ADev.DeactivateDate = DateTime.Now;
                        ADev.CurrentDeviceAssign = false;      //Remove off from Account
                        idc.SubmitChanges();
                    }

                    // insert TransLog
                    var writeTransLogUPD = adc.sp_rma_process(returnRequestID, 0, 0,
                                    " ", this.UserName, DateTime.Now, "DEACTIVATE", "Deactive Serial# " + serialNo, 2);

                }
                catch { }
                // ************* Deactivate the userdevice and accountdevice record **************************

            }

            else // --------- it seems the code in this else block is never stepped through. -------------------
            {    
                // is Null, status = not receive by Mfg.. 
                // 1. check if return request has been created?
                // 2. check device has been added to return request? 
                // 3. Create new entry in inventory. 
               
                // Create new entry by Tech with approved and completed
                // Get the Return Request devices count...
                int curReturnRequestCount = (from r in adc.rma_Returns
                                          join d in adc.rma_ReturnDevices
                                          on r.ReturnID equals d.ReturnID
                                          where r.AccountID == accountID && r.Active == true
                                          && d.SerialNo == serialNo && d.Active == true
                                          && d.Received == false 
                                          select r).Count();

                // If a Return Request has not been created for the account that is Open
                if (curReturnRequestCount <= 0)
                {
                    // Create Return Request for account and get the new generated ReturnID
                    rma_Return rma = null;
                    rma = new rma_Return()
                    {
                        AccountID = accountID,
                        Active = true,
                        CreatedBy = this.UserName,
                        CreatedDate = DateTime.Now,
                        Notes = "Auto generated by tech inventory entry.",
                        Reason = "Devices arrived at facility without Request No., by Tech",
                        Return_ReasonID = 8, //Devices arrived at facility without Request No, by Tech
                        ReturnTypeID = 6, // Device was mailed back without Request No, Check-in by Tech Ops
                        Status = 1 // Awaiting all devices returned.

                    };
                    adc.rma_Returns.InsertOnSubmit(rma);
                    adc.SubmitChanges();

                    // insert TransLog, add new Retrun
                    returnRequestID = rma.ReturnID;
                    var writeTransLogAddDev = adc.sp_rma_process(returnRequestID, 0, 0,
                              "Auto generated by tech inventory entry.", this.UserName, 
                              DateTime.Now, "ADD RETRUN TECH",
                              "Tech Create Return Request", 2);
                } // end if curReturnRequestCount <= 0


                // serial number exist in the return request details?. It seems returnDeviceCount always = 0
                int returnDeviceCount = (from rd in adc.rma_ReturnDevices
                                         where rd.ReturnID == returnRequestID &&
                                         rd.SerialNo == serialNo && rd.Active== true
                                         && rd.Received == false 
                                         select rd).Count();

                rma_ReturnDevice returnDevice = null;

                // Grab the return Device or create it (returnDevices table)
                if (returnDeviceCount >= 1)
                {
                    // Yes, device was found. 
                    // Update it to received.
                    returnDevice = (from rd in adc.rma_ReturnDevices
                                    where rd.ReturnID == returnRequestID &&
                                    rd.SerialNo == serialNo && rd.Active== true
                                    && rd.Received == false 
                                    select rd).First();

                    returnDevice.Status = 7; // Device is received and inspected by Tech .
                    returnDevice.DepartmentID = 8; // Device is in Tech Receiving location
                    returnDevice.Received = true; 
                    adc.SubmitChanges();

                    // insert TransLog, update devices status
                    newReturnDevicesID = returnDevice.ReturnDevicesID;
                    var writeTransLogUpdDev = adc.sp_rma_process(returnRequestID, newReturnDevicesID, 0,
                              " ", this.UserName, DateTime.Now, "UPDATE DEVICE TECH", 
                              "Status=7, DepartmentID=8", 2);
                }
                else
                {
                    // No, device not found in retrun request.
                    // Create a return request detail it and update it to received. 

                    // find out masterDeviceID 
                    var MDeviceID = (from a in idc.DeviceInventories
                                     where a.SerialNo == serialNo
                                     select a).First();

                    returnDevice = new rma_ReturnDevice()
                    {
                        Active = true,
                        DepartmentID = 8, //Device is in Tech Receiving location
                        Notes = "Auto generated by tech inventory entry.",
                        ReturnID = returnRequestID,
                        MasterDeviceID = MDeviceID.DeviceID ,
                        SerialNo = serialNo,
                        Received = true, // indicated received device
                        Status = 7 // Device is received and inspected by Tech.
                    };

                    // Create the new returned device
                    adc.rma_ReturnDevices.InsertOnSubmit(returnDevice);
                    // Save changes to the database
                    adc.SubmitChanges();

                    // insert TransLog, add new devices
                    newReturnDevicesID = returnDevice.ReturnDevicesID;
                    var writeTransLogAddDev = adc.sp_rma_process(returnRequestID, newReturnDevicesID, 0,
                              "Auto generated by tech inventory entry.", this.UserName, 
                              DateTime.Now, "ADD DEVICE TECH",
                              "DeviceStatus=7 Department=8", 2);

                } // end if else returnDeviceCount >= 1



                // Create the return inventory entry. (Return Inventory table)
                rma_ReturnInventory inventory = new rma_ReturnInventory()
                {
                    CreatedDate = DateTime.Now,
                    ReturnID = returnRequestID,
                    ReturnDeviceID = returnDevice.ReturnDevicesID,
                    SerialNo = serialNo,
                    ReceivedBy = this.UserName,
                    ReceivedDate = DateTime.Now,
                    TechOpsNotes = techOpsNotes,
                    TechOpsReviewer = this.UserName,
                    TechOpsReviewDate= DateTime.Now,

                    VisualInspectPass = boolChkVisualInspect,   
                    DataInspectPass = boolChkDataInspect,
                    TechOpsApproved = boolChkApprove,
                    Approved = boolChkApprove,

                    Active = true,
                    Completed = true,
                    CommittedToFinance = true ,     
            
                    CommittedDate = DateTime.Now                
                };

                // Add the new invetory item.
                adc.rma_ReturnInventories.InsertOnSubmit(inventory);

                //save change to database
                adc.SubmitChanges();

                // insert TransLog, update devices status
                newReturnInventoryID = inventory.ReturnInventoryID;
                var writeTransLogUpdInv2 = adc.sp_rma_process(returnRequestID, newReturnDevicesID,
                    newReturnInventoryID, techOpsNotes,
                    this.UserName, DateTime.Now, "ADD INV TECH", "Approved=" + boolChkApprove.ToString(), 2);

                // Refresh the data source
                adc.Refresh(System.Data.Linq.RefreshMode.KeepChanges);




                // Have all of the devices been received in the return request?
                // ******************** Update return status to: All Device(s) received/Partiall device(s) received status ************
                try
                {
                    // Requery the return and check to make sure the status is currently set to "Awaiting devices.." (1)
                    var returnRequest = (from r in adc.rma_Returns
                                         where r.ReturnID == returnRequestID && r.Active == true
                                         select r).First();

                    // Get the Return Request devices count...
                    int returnRequestCount = (from r in adc.rma_Returns
                                              join d in adc.rma_ReturnDevices
                                              on r.ReturnID equals d.ReturnID
                                              where r.ReturnID == returnRequestID && r.Active == true
                                              && d.Active == true
                                              select r).Count();


                    // Get a count of the inventory items for the return, based on the return ID
                    // and received/reviewed by Tech department 
                    int inventoryCount = (from ri in adc.rma_ReturnInventories
                                          where ri.Active == true &&
                                          ri.ReturnID == returnRequestID
                                          && ri.TechOpsReviewer != null
                                          select ri).Count();

                    // Do they have the same number of items?
                    if (returnRequestCount == inventoryCount)
                    {
                        // Yes, update the return request status to ... received.
                        returnRequest.Status = 8; // All Device(s) received and inspected by Tech. The return is complete.

                        // Save changes.
                        adc.SubmitChanges();

                        // insert TransLog, update Retrun Status
                        var writeTransLogUPD = adc.sp_rma_process(returnRequestID, 0, 0,
                                      " ", this.UserName, DateTime.Now, "UPDATE RETURN", "Update Request status to 8, RMA completed.", 2);

                    }
                    else
                    {
                        // No, update the return request status to partial received.
                        returnRequest.Status = 7;  // Partiall device(s) received and inspected by Tech. 

                        // save changes.
                        adc.SubmitChanges();

                        // insert TransLog, update Retrun Status
                        var writeTransLogUPD = adc.sp_rma_process(returnRequestID, 0, 0,
                                      " ", this.UserName, DateTime.Now, "UPDATE RETURN", "Update Request status to 7.", 2);
                    }
                }

                catch { }
                // ******************** Update return status to: All Device(s) received/Partiall device(s) received status ************

                // ************* Deactivate the userdevice and accountdevice record **************************
                // and remove a device off the account
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
                                          where a.DeviceID == deviceInventory.DeviceID && a.CurrentDeviceAssign == true
                                          select a).FirstOrDefault();
                    if (ADev != null)
                    {
                        ADev.Active = false;    //Deactivate
                        ADev.CurrentDeviceAssign = false;      //Remove off from Account
                        ADev.DeactivateDate = DateTime.Now;
                        idc.SubmitChanges();
                    }

                    // insert TransLog
                    var writeTransLogUPD = adc.sp_rma_process(returnRequestID, 0, 0,
                                    " ", this.UserName, DateTime.Now, "DEACTIVATE", "Deactive Serial# " + serialNo, 2);

                }
                catch { }
                // ************* Deactivate the userdevice and accountdevice record **************************
            
            } // end if riTest == null




        } // end for loop

    }


    protected void btnEdit_Click(object sender, EventArgs e)
    {
        // Hide the first panel and display the secondary panel.
        this.plAdd.Visible = false;
        this.plReview.Visible = false;
        this.plReviewEdit.Visible = true;

        // Store the list of serial numbers that the user has added.
        List<string> serialNumberList = new List<string>();
        serialNumberList.AddRange(this.txtSerialNos.Text.Split('\n'));

        // Load the serial number data and existing comment
        this.LoadDeviceDataTableEdit(serialNumberList);

        // Render the grid.
        this.gvReviewEdit.DataSource = dtDeviceDataTableEdit;
        this.gvReviewEdit.DataBind();
    }

    protected void btnFinishEdit_Click(object sender, EventArgs e)
    {
        // Create the entries.
        this.CommitEdit();

        // Redirect back to first page, with a success message.
        
        this.VisibleMessages("The Tech Ops Notes have been updated.");  
        this.txtSerialNos.Text = "";
        this.plAdd.Visible = true;
        this.plReview.Visible = false;
        this.plReviewEdit.Visible = false;

    }


    /// <summary>
    /// Load the data table with from the serial numbers to be edited.
    /// </summary>
    /// <param name="serialNumberList"></param>
    private void LoadDeviceDataTableEdit(List<string> serialNumberList)
    {
        // Store a list of error'd serials
        Dictionary<string, string> errorList = new Dictionary<string, string>();

        // DataTable to store the serial data
        dtDeviceDataTableEdit = new DataTable();
        dtDeviceDataTableEdit.Columns.Add("ReturnInventoryID", typeof(int));
        dtDeviceDataTableEdit.Columns.Add("SerialNo", typeof(string));
        dtDeviceDataTableEdit.Columns.Add("AccountNo", typeof(int));
        dtDeviceDataTableEdit.Columns.Add("RequestNo", typeof(string));
        dtDeviceDataTableEdit.Columns.Add("RequestType", typeof(string));
        dtDeviceDataTableEdit.Columns.Add("Reviewer", typeof(string));
        dtDeviceDataTableEdit.Columns.Add("ReviewDate", typeof(string));
        dtDeviceDataTableEdit.Columns.Add("TechNotes", typeof(string));
        

        // Ensure that least one device was added.
        if (serialNumberList.Count > 0)
        {
            // Loop through each serial number.
            foreach (string serialNumber in serialNumberList)
            {
                
                if (serialNumber.Trim() != "")
                {

                    // Find the inventory for the devices and the device info.
                    DeviceInventory deviceInventory = getDeviceInventory(serialNumber.Trim());

                    var RtnInvenList = (from ri in adc.rma_ReturnInventories
                                        join rRet in adc.rma_Returns
                                           on ri.ReturnID equals rRet.ReturnID
                                        where ri.SerialNo == serialNumber.Trim()
                                         && ri.Active == true
                                         && rRet.Active == true  // in an Active Request.
                                         orderby ri.ReturnID
                                        select new {ri, rRet }).ToList();

                    if (RtnInvenList.Count > 0)
                    {
                         
                        foreach (var v in RtnInvenList)
                        {
                            // Get the default or standards
                            int accountNo = v.rRet.AccountID;
                            string requestNo = v.ri.ReturnID.ToString();
                            string requestType = v.rRet.rma_ref_ReturnType.Type;

                            string Reviewer = (v.ri.TechOpsReviewer != null) ? v.ri.TechOpsReviewer.ToString() : "";
                            string ReviewDate = (v.ri.TechOpsReviewDate != null) ? v.ri.TechOpsReviewDate.ToString() : "";


                            string serialNo = v.ri.SerialNo;
                            int ReturnInventoryID = v.ri.ReturnInventoryID;
                            string TechNotes = v.ri.TechOpsNotes;

                            // Create the data row to be modified to the data table.
                            DataRow drE = dtDeviceDataTableEdit.NewRow();
                            drE["ReturnInventoryID"] = ReturnInventoryID;
                            drE["SerialNo"] = serialNo;
                            drE["AccountNo"] = accountNo;
                            drE["RequestNo"] = requestNo;
                            drE["RequestType"] = (requestType == "Recall") ? "Recall" : "Return"; 
                            drE["Reviewer"] = Reviewer;
                            drE["ReviewDate"] = ReviewDate;
                            drE["TechNotes"] = TechNotes;
                            // Add the row to the DataTable.
                            dtDeviceDataTableEdit.Rows.Add(drE);
                        }
                    }

                    else
                    {
                        errorList.Add(serialNumber, "Serial number does not exist in the Return Inventory system.");
                    }
                }//end else SerialNumber trim ==""
            } //end for each serial no

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
                this.VisibleErrorReviewEdit("The following error(s) occurred: " + errorText);
            }        

            // Sort the contents of the data table.
            dtDeviceDataTableEdit.DefaultView.Sort = "AccountNo, SerialNo";

        }
    } //end LoadDeviceDataTableEdit

    //Commit Edit action, save update Comment 
    private void CommitEdit()
    {
        // Get the serial numbers information and updated Notes from Gridview
        for (int i = 0; i < this.gvReviewEdit.Rows.Count; i++)
        {
            GridViewRow gvRow = gvReviewEdit.Rows[i];
            TextBox findTxtBox = (TextBox)gvRow.FindControl("txtNotes");

            string OpsNotes = findTxtBox.Text;
            int ReturnInventoryID = 0;
            int.TryParse(gvRow.Cells[0].Text, out ReturnInventoryID);


            //update returnInventory Comment
            if (ReturnInventoryID >= 1)
            {
                rma_ReturnInventory RInventory = null;
                RInventory = (from rinv in adc.rma_ReturnInventories
                                     where rinv.ReturnInventoryID == ReturnInventoryID
                                     select rinv).First();

                RInventory.TechOpsNotes = OpsNotes;
                RInventory.TechOpsReviewDate = DateTime.Now;
                //RInventory.TechOpsReviewer = this.UserName;
                
                // save update
                adc.SubmitChanges();

                //insert TransLog, update techopsNotes
                var writeTransLogComment = adc.sp_rma_process(RInventory.ReturnID ,RInventory.ReturnDeviceID,
                    RInventory.ReturnInventoryID,
                    OpsNotes, this.UserName, DateTime.Now, "UPDATE TECH COMMENT", "UPDATE TechOps Comment.", 2);
            }
        }
    }
    // END 9/24/2010 Add Edit Comment function for Alexender

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
        this.errorReviewMsg.InnerHtml = "";
        this.errorReview.Visible = false;
    }

    private void VisibleErrorReview(string error)
    {
        this.errorReviewMsg.InnerHtml = error;
        this.errorReview.Visible = true;
    }

    private void InvisibleErrorReviewEdit()
    {
        // Reset submission form error message      
        this.errorReviewEditMsg.InnerHtml = "";
        this.errorReviewEdit.Visible = false;
    }

    private void VisibleErrorReviewEdit(string error)
    {
        this.errorReviewEditMsg.InnerHtml = error;
        this.errorReviewEdit.Visible = true;
    }
}
