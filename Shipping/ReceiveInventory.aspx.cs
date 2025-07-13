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
using Instadose.Device;
using System.Text;

public partial class Instadose_Shipping_ReceiveInventory : System.Web.UI.Page
{
    // Turn on/off email to all others
    bool DevelopmentServer = false;

    // Create the database reference
    InsDataContext idc = new InsDataContext();
    AppDataContext adc = new AppDataContext();

    // Create data table for devices
    DataTable deviceTable = null;
    
    // String to hold the current username
    string UserName = "Unknown";

    protected void Page_Load(object sender, EventArgs e)
    {
        // Auto set if a development site
        if (Request.Url.Authority.ToString().Contains(".mirioncorp.com"))
            DevelopmentServer = true;
        
        // Try to set the username
        try
        {
            this.UserName = User.Identity.Name.Split('\\')[1];
            this.lblusername.Text = this.UserName;

            this.pnlSuccessMessages.Visible = false;
            this.lblSuccess.Text = "";
            this.pnlErrorMessages.Visible = false;
            this.lblErrorReview.Text = "";

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
        // this.btnFinish.ForeColor = System.Drawing.Color.Black ;
        this.btnFinish.Enabled  = true;
        this.LoadDeviceDataTable(serialNumberList);

        // Render the grid.
        this.gvReview.DataSource = deviceTable;
        this.gvReview.DataBind();
    }

    private void ValidateInput(List<string> serialNumberList)
    {
        // Store a list of error'd serials
        Dictionary<string, string> errorList = new Dictionary<string, string>();
       
        // Ensure that LEAST one Device was added/searched for.
        if (serialNumberList.Count > 0)
        {
            // Loop through each Serial Number.
            foreach (string serialNumber in serialNumberList)
            {
                string serialNo = serialNumber.Trim();

                if (serialNo == string.Empty) continue;

                // Find if the Device exists in the DB from DeviceInventory table based on Serial Number entered.
                DeviceInventory deviceInventory = (from d in idc.DeviceInventories where d.SerialNo == serialNo select d).FirstOrDefault();

                if (deviceInventory == null)
                {
                    errorList.Add(serialNo, "The badge could not be found.  Please check the serial number is correct.");
                    continue;
                }

                // 1/22/2015, tdo 
                // Find if the Device has already been added/returned into Return Inventory & with an Active status of TRUE.
                rma_ReturnInventory returnInventory = getReturnInventory(serialNo);

                // If returnInventory IS NOT NULL that means the Device has already been scanned and waiting for TechOps review.
                if (returnInventory != null)
                {
                    errorList.Add(serialNo, "Device has already been scanned and added into receiving inventory. No other receive can be made.");
                    continue;
                }

                // 12/07/2015
                // Find the CURRENT assigned AccountID based on DeviceID from deviceInventory.
                // This is done by ORDERING BY AssignmentDate DESCENDING.
                AccountDevice accountDevice = (from ad in idc.AccountDevices
                                               where ad.DeviceID == deviceInventory.DeviceID
                                               && ad.CurrentDeviceAssign == true
                                               orderby ad.AssignmentDate descending
                                               select ad).FirstOrDefault();
                if (accountDevice == null)
                {
                    errorList.Add(serialNo, "Device does not belong to any account.");
                    continue;
                }
                
                // 1/22/2015, tdo   
                // Find Serial Number in rma_ReturnDevices table that are Active, but not Returned/Received.
                rma_ReturnDevice returnDevice = (from rd in adc.rma_ReturnDevices
                                                 where rd.SerialNo == serialNo && rd.Active == true && rd.Received == false
                                                 orderby rd.ReturnDevicesID descending
                                                 select rd).FirstOrDefault();

                // DeviceID/SerialNo is found in either rma_ReturnDevices OR AccountDevices.
                // AND the DeviceID/SerialNo DOES EXIST in the DeviceInventory AND HAS NOT been returned/expecting to be returned.
                if ((accountDevice != null || returnDevice != null) && deviceInventory != null)
                {
                    int accountNo = 0;
                    string requestNo = "";

                    if (returnDevice != null)
                    {
                        // Get the current AccountID (from the AccountDevices table)
                        // to match the AccountID in the rma_Returns table.
                        int x = 0;
                        x = (from rt in adc.rma_Returns
                             where rt.ReturnID == returnDevice.ReturnID
                             && rt.AccountID == accountDevice.AccountID
                             select rt.AccountID).FirstOrDefault();

                        // If NO AccountID matches meaning 0 or NULL, then use the AccountID from the AccountDevices table.
                        if (x == null || x == 0)
                        {
                            accountNo = accountDevice.AccountID;
                        }
                        // Else, use the AccountID referenced in the rma_Returns table.
                        else
                        {
                            accountNo = x;
                        }

                        requestNo = returnDevice.ReturnID.ToString();
                    }
                    else
                    {
                        accountNo = (accountDevice != null) ? accountDevice.AccountID : 0;
                        requestNo = "None";
                    }

                    // Create the data row to be added to the data table.
                    DataRow dr = deviceTable.NewRow();
                    dr["SerialNo"] = serialNumber;
                    dr["AccountNo"] = accountNo;
                    dr["RequestNo"] = requestNo;
                    dr["DepartmentRequest"] = "";

                    // Get Request Department
                    if (requestNo != "None")
                    {
                        rma_ref_Department returnDepartment = getRequestDepartment(requestNo);
                        string ReqDepartment = returnDepartment.Name.ToString();
                        dr["DepartmentRequest"] = ReqDepartment;
                    }

                    // Device has an account association.
                    //if (requestNo == "None" )
                    if (requestNo == "None" && accountDevice.AccountID != 0) // 9/1/2011
                    {
                        dr["Action"] = "A request will be generated for this account.";
                        dr["Type"] = "This return was unexpected. A return was generated for this account.";

                        // Modified on 12/16/2014, by: TDo
                        // Requested by: LP. To remove the warning msg when unexpected devices return by Philips or CareStream

                        // if return by IIM Account and without RMA request
                        string AccountIDToCheck = "7257";

                        if (AccountIDToCheck.IndexOf(accountDevice.AccountID.ToString()) >= 0)
                        {
                            // If the device returns to "Tech Ops" department, we know this is a Recall then go ahead to perform return.
                            // Else then do not allow to perform return
                            if (dr["DepartmentRequest"].ToString() != "Tech Ops")
                            {
                                //this.btnFinish.ForeColor = System.Drawing.Color.Red;
                                this.btnFinish.Enabled = false;
                                errorList.Add(serialNumber, "UNAUTHORIZED DOSIMETER RETURN: Dosimeter belongs to Instadose In-transit Monitoring Account #7257. Do not proceed. Refer to Instadose Technical Department.");
                            }
                        }
                    }
                    else
                    {
                        dr["Action"] = "The device will be checked in.";
                        dr["Type"] = "Device has been received for a return.";
                    }

                    // Add the row to the DataTable.
                    deviceTable.Rows.Add(dr);

                }
            }

            // Once all of the devices have been added to the data table,
            // display the results to the screen.
            string errorText = "";
            this.pnlErrorMessages.Visible = false;

            foreach (KeyValuePair<string, string> error in errorList)
            {
                errorText += error.Key + ": " + error.Value + "<br /> ";
            }

            // Display the error message if errors exist.
            if (errorText != "")
            {
                this.lblErrorReview.Text = "The following error(s) occurred: <br />" + errorText;
                this.pnlErrorMessages.Visible = true;
            }

            // Sort the contents of the data table.
            deviceTable.DefaultView.Sort = "AccountNo, SerialNo";
        }
    }

    protected void btnBack_Click(object sender, EventArgs e)
    {
        // Hide the second panel and display the primary panel.
        this.plAdd.Visible = true;
        this.plReview.Visible = false;
    }

    private bool PassValidation(ref string pErrorMsg)
    {
        bool pass = true;
        Hashtable snHash = new Hashtable();
        string duplicateSN = "";
        try
        {
            for (int i = 0; i < this.gvReview.Rows.Count; i++)
            {
                GridViewRow gvRow = gvReview.Rows[i];                
                string serialNo = gvRow.Cells[0].Text.Replace("\n", "").ToString().Trim();
                if (!snHash.ContainsKey(serialNo))
                {
                    snHash.Add(serialNo, "");
                }
                else
                {
                    duplicateSN += serialNo + ", ";                    
                    pass = false;
                }
            }
            if (duplicateSN.Length > 0)
            {
                duplicateSN = "Please make sure there is no duplicate S/N: " + duplicateSN.Substring(0, duplicateSN.Length - 2);
            }
        }
        catch (Exception ex)
        {
            duplicateSN = ex.ToString();
            pass = false;
        }
        pErrorMsg = duplicateSN;
        return pass;
    }

    protected void btnFinish_Click(object sender, EventArgs e)
    {
        // Prevent double click on the submit button
        btnFinish.Enabled = false;

        string errorMsg = "";
        // must disable the button and enable the button at the end.
        // validate before commit.
        // Prevent double scan on a serial number
        if (! PassValidation(ref errorMsg))
        {
            this.pnlSuccessMessages.Visible = false;
            this.lblSuccess.Text = "";
            this.pnlErrorMessages.Visible = true;
            this.lblErrorReview.Text = errorMsg;            

            //ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "alert('Please check. Double scan on some serial number!!!')", true);            
            return;
        }        

        // Create the entries. Commit Mfg receive
        this.Commit();

        // Create upload attachment record to unique requestID
        if (FileUpload1.HasFile)
            this.CommitUploadAttachFile(1);

        if (FileUpload2.HasFile)
            this.CommitUploadAttachFile(2);

        // Send Email
        // this.CommitSendEmailNote(); By Craig, turn off the RMA EMAIL functionality 1/28/2014        

        this.txtSerialNos.Text = "";

        // Hide the first panel and display the secondary panel.
        this.plAdd.Visible = true;
        this.plReview.Visible = false;        
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
            // Find the return inventory, filter out completed items,             
            // since a device could come through the return process many times
            // throughout the devices life cycle
        return (from rd in adc.rma_ReturnDevices
                join rt in adc.rma_Returns on rd.ReturnID equals rt.ReturnID
                join ri in adc.rma_ReturnInventories on rd.ReturnID equals ri.ReturnID
                where rd.SerialNo == serialNumber && ri.SerialNo == serialNumber
                && rd.Active == true && rt.Active == true && ri.Active == true
                && ri.CommittedToFinance == null
                orderby rt.CreatedDate descending
                select ri).FirstOrDefault();             
    }

    private rma_ReturnDevice getReturnDevice(string serialNumber)
    {
        // Create the object to return.
        return (from rd in adc.rma_ReturnDevices
                            where rd.SerialNo == serialNumber && rd.Active== true
                            select rd).FirstOrDefault();
    }

    private rma_ref_Department getRequestDepartment(string requestNo)
    { 
        int myRequestNo = 0;
        int.TryParse(requestNo, out myRequestNo);
        // find the return department request
        return (from rt in adc.rma_Returns
                            join rtT in adc.rma_ref_ReturnTypes
                            on rt.ReturnTypeID equals rtT.ReturnTypeID
                            join rtD in adc.rma_ref_Departments
                            on rtT.DepartmentID equals rtD.DepartmentID
                            where rt.ReturnID == myRequestNo
                            select rtD).FirstOrDefault();
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
        deviceTable = new DataTable();

        deviceTable.Columns.Add("SerialNo", typeof(string));
        deviceTable.Columns.Add("AccountNo", typeof(int));
        deviceTable.Columns.Add("RequestNo", typeof(string));
        deviceTable.Columns.Add("Action", typeof(string));
        deviceTable.Columns.Add("Type", typeof(string));
        deviceTable.Columns.Add("DepartmentRequest", typeof(string));

        // Ensure that LEAST one Device was added/searched for.
        if (serialNumberList.Count > 0)
        {
            // Loop through each Serial Number.
            foreach (string serialNumber in serialNumberList)
            {
                string serialNo = serialNumber.Trim();

                if (serialNo == string.Empty) continue;

                // Find if the Device exists in the DB from DeviceInventory table based on Serial Number entered.
                DeviceInventory deviceInventory = (from d in idc.DeviceInventories where d.SerialNo == serialNo select d).FirstOrDefault();

                if (deviceInventory == null)
                {
                    errorList.Add(serialNo, "The badge could not be found.  Please check the serial number is correct.");
                    continue;
                }

                // 1/22/2015, tdo 
                // Find if the Device has already been added/returned into Return Inventory & with an Active status of TRUE.
                rma_ReturnInventory returnInventory = getReturnInventory(serialNo);

                // If returnInventory IS NOT NULL that means the Device has already been scanned and waiting for TechOps review.
                if (returnInventory != null)
                {
                    errorList.Add(serialNo, "Device has already been scanned and added into receiving inventory. No other receive can be made.");
                    continue;
                }

                // 12/07/2015
                // Find the CURRENT assigned AccountID based on DeviceID from deviceInventory.
                // This is done by ORDERING BY AssignmentDate DESCENDING.
                AccountDevice accountDevice = (from ad in idc.AccountDevices
                                               where ad.DeviceID == deviceInventory.DeviceID
                                               && ad.CurrentDeviceAssign == true
                                               orderby ad.AssignmentDate descending
                                               select ad).FirstOrDefault();
                if (accountDevice == null)
                {
                    errorList.Add(serialNo, "Device does not belong to any account.");
                    continue;
                }
                                                        
                // 1/22/2015, tdo   
                // Find Serial Number in rma_ReturnDevices table that are Active, but not Returned/Received.
                rma_ReturnDevice returnDevice = (from rd in adc.rma_ReturnDevices
                                                 where rd.SerialNo == serialNo && rd.Active == true && rd.Received == false
                                                 orderby rd.ReturnDevicesID descending 
                                                 select rd).FirstOrDefault();                         

                // DeviceID/SerialNo is found in either rma_ReturnDevices OR AccountDevices.
                // AND the DeviceID/SerialNo DOES EXIST in the DeviceInventory AND HAS NOT been returned/expecting to be returned.
                if ((accountDevice != null || returnDevice != null) && deviceInventory != null )
                {
                    int accountNo = 0;
                    string requestNo = "";

                    if (returnDevice != null)
                    {
                        // Get the current AccountID (from the AccountDevices table)
                        // to match the AccountID in the rma_Returns table.
                        int x = 0;
                        x = (from rt in adc.rma_Returns
                             where rt.ReturnID == returnDevice.ReturnID
                             && rt.AccountID == accountDevice.AccountID
                             select rt.AccountID).FirstOrDefault();
                        
                        // If NO AccountID matches meaning 0 or NULL, then use the AccountID from the AccountDevices table.
                        if (x == null || x == 0)
                        {
                            accountNo = accountDevice.AccountID;
                        }
                        // Else, use the AccountID referenced in the rma_Returns table.
                        else
                        {
                            accountNo = x;
                        }

                        requestNo = returnDevice.ReturnID.ToString();
                    }
                    else
                    {
                        accountNo = (accountDevice != null) ? accountDevice.AccountID : 0;
                        requestNo = "None";
                    }                    
                    
                    // Create the data row to be added to the data table.
                    DataRow dr = deviceTable.NewRow();
                    dr["SerialNo"] = serialNumber;
                    dr["AccountNo"] = accountNo;
                    dr["RequestNo"] = requestNo;
                    dr["DepartmentRequest"] = ""; 

                    // Get Request Department
                    if (requestNo != "None")
                    {
                        rma_ref_Department returnDepartment = getRequestDepartment(requestNo);
                        string ReqDepartment = returnDepartment.Name.ToString();
                        dr["DepartmentRequest"] = ReqDepartment; 
                    }                    

                    // Device has an account association.
                    //if (requestNo == "None" )
                    if (requestNo == "None" && accountDevice.AccountID != 0) // 9/1/2011
                    {
                        dr["Action"] = "A request will be generated for this account.";
                        dr["Type"] = "This return was unexpected. A return was generated for this account.";

                        // Modified on 12/16/2014, by: TDo
                        // Requested by: LP. To remove the warning msg when unexpected devices return by Philips or CareStream

                        // if return by IIM Account and without RMA request
                        string AccountIDToCheck = "7257";

                        if (AccountIDToCheck.IndexOf(accountDevice.AccountID.ToString()) >= 0)
                        {
                            // If the device returns to "Tech Ops" department, we know this is a Recall then go ahead to perform return.
                            // Else then do not allow to perform return
                            if (dr["DepartmentRequest"].ToString() != "Tech Ops")
                            {
                                //this.btnFinish.ForeColor = System.Drawing.Color.Red;
                                this.btnFinish.Enabled = false;
                                errorList.Add(serialNumber, "UNAUTHORIZED DOSIMETER RETURN: Dosimeter belongs to Instadose In-transit Monitoring Account #7257. Do not proceed. Refer to Instadose Technical Department.");
                            }                                                            
                        }
                    }
                    else
                    {
                        dr["Action"] = "The device will be checked in.";
                        dr["Type"] = "Device has been received for a return.";
                    }

                    // Add the row to the DataTable.
                    deviceTable.Rows.Add(dr);
                    
                }
            }

            // Once all of the devices have been added to the data table,
            // display the results to the screen.
            string errorText = "";
            this.pnlErrorMessages.Visible = false;

            foreach (KeyValuePair<string, string> error in errorList)
            {
                errorText += error.Key + ": " + error.Value + "<br /> ";
            }

            // Display the error message if errors exist.
            if (errorText != "")
            {
                this.lblErrorReview.Text = "The following error(s) occurred: <br />" + errorText;
                this.pnlErrorMessages.Visible = true;
            }

            // Sort the contents of the data table.
            deviceTable.DefaultView.Sort = "AccountNo, SerialNo";
        }
    }

    private void CommitUploadAttachFile(int UploadControlNo)
    {
        // Get Upload infor and convert to byte format
        HttpPostedFile myUploadFile = null;

        if (UploadControlNo == 1)
            myUploadFile = FileUpload1.PostedFile;
        
        else if (UploadControlNo == 2)
            myUploadFile = FileUpload2.PostedFile;

        //find upload file length and convert it to byte array
        int intContentLength = myUploadFile.ContentLength;

        //create byte array
        Byte[] docContent = new Byte[intContentLength];

        //read upload file in byte array
        myUploadFile.InputStream.Read(docContent, 0, intContentLength);

        string fileName = System.IO.Path.GetFileName(myUploadFile.FileName);
        string mimeType = myUploadFile.ContentType;

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
            int ChkRequestID = (from rD in adc.rma_ReturnDevices
                               join rR in adc.rma_Returns
                               on rD.ReturnID equals rR.ReturnID
                               where rD.SerialNo == ChkSerialNo
                               && rR.AccountID == ChkAccountID
                               && rD.Active == true
                               && rR.Active == true
                               select rD.ReturnID).FirstOrDefault();

            // Create the data row to be added to the data table.
            DataRow dr = NewdtDeviceDataTable.NewRow();
            dr["SerialNo"] = ChkSerialNo;
            dr["AccountNo"] = ChkAccountID;
            dr["RequestNo"] = ChkRequestID;

            // Add the row to the DataTable.
            NewdtDeviceDataTable.Rows.Add(dr);
        }
        // *************************************************************************************
     


        var query = (from r in NewdtDeviceDataTable.AsEnumerable()
                     select r.Field<string>(2)).Distinct().ToList();

        // save uploadfile to each returnID
        foreach (var v in query)
        {
            int ReqID;
            if (int.TryParse(v, out ReqID))
            {
                // Add the document.
                Document document = new Document()
                {
                    CreatedBy = this.UserName,
                    CreatedDate = DateTime.Now,
                    Active = true,
                    FileName = fileName,
                    Description = "RequestID: " + ReqID.ToString(),
                    DocumentCategory = "Return",
                    DocumentGUID = Guid.NewGuid(),
                    DocumentContent = docContent,
                    MIMEType = mimeType
                };

                // Save the document
                idc.Documents.InsertOnSubmit(document);
                idc.SubmitChanges();

                // insert TransLog, update Retrun Status
                var writeTransLogUPD = adc.sp_rma_process(ReqID, 0, 0,
                              " ", this.UserName, DateTime.Now, "UPLOAD FILE", "Upload file: "
                              + fileName + document.DocumentID.ToString(), 2);
            }
        }
    } // end upload attach file

    /// <summary>
    /// Send email message to info@Quantumbadges.com and User's email account as well.
    /// </summary>
    private void CommitSendEmailNote()
    {
        // Exit if no rows exist;
        if(gvReview.Rows.Count == 0) return;

        // Send an email to customer service.
        int rowCount = 0;

        string accountInfoUrl = "https://" + Request.Url.Authority.ToString() + "/InformationFinder/Details/Account.aspx?ID=";
        string rmaInfoUrl = "https://" + Request.Url.Authority.ToString() + "/InformationFinder/Details/Return.aspx?ID=";

        StringBuilder returnRows = new StringBuilder();

        for (int i = 0; i < this.gvReview.Rows.Count; i++)
        {
            GridViewRow gvRow = gvReview.Rows[i];
            TextBox findTxtBox = (TextBox)gvRow.FindControl("txtNotes");
            System.Web.UI.WebControls.Label findLblAccountNo = (System.Web.UI.WebControls.Label)gvRow.FindControl("lbl_GvAccountNo");
            System.Web.UI.WebControls.Label findLblRequestNo = (System.Web.UI.WebControls.Label)gvRow.FindControl("lbl_GvRequestNo");

            int returnRequestID = 0;
            int.TryParse(findLblRequestNo.Text, out returnRequestID);

            int accountID = int.Parse(findLblAccountNo.Text);
            string serialNo = gvRow.Cells[0].Text.Replace("\n", "").ToString().Trim();
            string receiveOpsNotes = findTxtBox.Text;

            // check NEW RMA# for device return without Return Request 
            if (returnRequestID == 0)
            {
                returnRequestID = (from rD in adc.rma_ReturnDevices
                                   join rR in adc.rma_Returns
                                   on rD.ReturnID equals rR.ReturnID
                                   where rD.SerialNo == serialNo
                                   && rR.AccountID == accountID
                                   && rD.Active == true
                                   && rR.Active == true
                                   select rD.ReturnID).FirstOrDefault();
            }
            
            // Append the record.
            string returnRequest = (returnRequestID == 0) ? "N/A" : returnRequestID.ToString();

            // Create the return hyperlink
            string returnLink = "N/A";
            if (returnRequestID != 0) returnLink = string.Format("<a href='{0}{1}'>View</a>", rmaInfoUrl, returnRequestID);

            // Create the account hyperlink
            string accountLink = "N/A";
            if(accountID != 0) accountLink = string.Format("<a href='{0}{1}#returns'>{1}</a>", accountInfoUrl, accountID);

            // If the badge is broken or damaged, set the notes color to red.
            if ((receiveOpsNotes.ToUpper().IndexOf("DAMAGE") == -1) && (receiveOpsNotes.ToUpper().IndexOf("BROKEN") == -1))
                receiveOpsNotes = string.Format("<span bgcolor='orangered'>{0}</span>", receiveOpsNotes);

            returnRows.AppendLine("<tr>");
            returnRows.AppendLine("  <td align='center' vertical-align='top' style='border:1px solid #e1e0dd'>" + returnLink + "</td>");
            returnRows.AppendLine("  <td align='center' vertical-align='top' style='border:1px solid #e1e0dd'>" + returnRequestID.ToString() + "</td>");
            returnRows.AppendLine("  <td align='right' vertical-align='top' style='border:1px solid #e1e0dd'>" + accountLink + "</td>");
            returnRows.AppendLine("  <td align='left' vertical-align='top' style='border:1px solid #e1e0dd'>" + serialNo + "</td>");
            returnRows.AppendLine("  <td align='left' vertical-align='top' style='border:1px solid #e1e0dd'>" + receiveOpsNotes + "</td>");
            returnRows.AppendLine("</tr>");

            rowCount++;
        }

        string RMANumber = "";
        if (Session["returnRequestID"] != null) RMANumber = Session["returnRequestID"].ToString();

        // Create the email template fields.
        Dictionary<string, string> fields = new Dictionary<string, string>();
        fields.Add("ReturnRows", returnRows.ToString()); 
        fields.Add("UserName", UserName);

        // FLAG CS or Finance that received device/s with NO request#
        if (Session["returnRequestID"] != null)
        {
            fields.Add("AutoGeneratedMessage", "<p>Badges were mailed back <b>without</b> an RMA# and received by manufacturing.  The RMA# " + RMANumber + " has been created.</p>");
            fields.Add("SubjectMessage", "Unauthorized RMA");
        }
        else
        {
            fields.Add("AutoGeneratedMessage", "");
            fields.Add("SubjectMessage", "Authorized RMA");
        }

        MessageSystem msys = new MessageSystem()
        {
            Application = "/Shipping/ReceiveInventory.aspx",
            CreatedBy = this.UserName,
            FromAddress = "noreply@instadose.com",
            CcAddressList = new List<string>(),
            ToAddressList = new List<string>()
        };

        // Add emails
        msys.ToAddressList.Add("info@quantumbadges.com");
        msys.ToAddressList.Add("irv-financereturns@mirion.com");

        // Add the e-mail of the person who created the return
        msys.ToAddressList.Add(this.UserName + "@mirion.com");

        // Do not send the e-mail to others if on development.
        if (DevelopmentServer)
        {
            msys.ToAddressList.Clear();
            msys.ToAddressList.Add(this.UserName + "@mirion.com");
        }

        // Send the message
        msys.Send("Return Received", "", fields);

        int returnID = 0;
        if(int.TryParse(RMANumber, out returnID))

            // insert TransLog, Email sent
            if (returnID == 0)
            {
                adc.sp_rma_process(null, 0, 0,
                        " ", this.UserName, DateTime.Now, "EMAIL RECEIVING", "Receiving Email sent to ALL departmetns", 2);
            }
            else
            {
                adc.sp_rma_process(returnID, 0, 0,
                        " ", this.UserName, DateTime.Now, "EMAIL RECEIVING", "Receiving Email sent to ALL departmetns", 2);
            }
    }

    private void Commit()
    {
        // Get the serial numbers information and the Notes from Receiving
        // from GridView  to commit to the inventory

        // store RetrunID and accountID in session variable for mutiple devices receive 
        // which without returnID but same accountID
        Session.Contents.RemoveAll();  // clear all session variables.
        Session["accountID"] = 0;

        for (int i = 0; i < this.gvReview.Rows.Count; i++)
        {
            GridViewRow gvRow = gvReview.Rows[i];
            TextBox findTxtBox = (TextBox)gvRow.FindControl("txtNotes");

            string receiveOpsNotes = findTxtBox.Text;

            int returnRequestID = 0;
            int newReturnDevicesID = 0;
            int newReturnInventoryID = 0;

            string serialNo = gvRow.Cells[0].Text.Replace("\n", "").ToString().Trim();
            string returnedDept = gvRow.Cells[3].Text.Replace("\n", "").ToString().Trim();

            System.Web.UI.WebControls.Label findLblAccountNo = (System.Web.UI.WebControls.Label)gvRow.FindControl("lbl_GvAccountNo");
            int accountID = int.Parse(findLblAccountNo.Text);

            System.Web.UI.WebControls.Label findLblRequestNo = (System.Web.UI.WebControls.Label)gvRow.FindControl("lbl_GvRequestNo");
            int.TryParse(findLblRequestNo.Text, out returnRequestID);
           
            // 1/22/2015, tdo. The grid view is only containing devices which have not been scanned
            // Make sure the device not exist in ReturnInventory 
            // rma_ReturnInventory riTest = getReturnInventory(serialNo);
            // if (riTest == null)
            if (true)
            {
                //store ReturnID and accountID in session variables for processing mutiple returning devices in the same account but have not generated RMAs yet.
                if (Session["returnRequestID"] != null
                    && Session["accountID"].ToString() == accountID.ToString()
                    && returnRequestID == 0)
                {
                    int.TryParse(Session["returnRequestID"].ToString(), out returnRequestID);
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
                if (returnRequestCount == 0 && accountID.ToString() != Session["accountID"].ToString())
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
                        Reason = (from rr in adc.rma_ref_ReturnReasons where rr.ReasonID == 7 select rr.Description).FirstOrDefault(),
                        ReturnTypeID = 5, // Device was mailed back without Request No, Check-in by Mfg
                        Status = 1, // Awaiting all devices returned.
                        Return_ReasonID = 7 //Devices arrived at facility without Request No, add by Mfg
                    };
                    adc.rma_Returns.InsertOnSubmit(rma);
                    adc.SubmitChanges();

                    // insert TransLog, add new Retrun
                    returnRequestID = rma.ReturnID;
                    adc.sp_rma_process(returnRequestID, 0, 0,
                              "Auto generated by inventory entry.", this.UserName, DateTime.Now, "ADD RETRUN",
                              "Device was mailed back without Request number.", 2);

                    // store RetrunID and accountID in session variable for mutiple devices which without returnID but same accountID
                    Session["returnRequestID"] = rma.ReturnID;
                    Session["accountID"] = accountID;
                }

                // serial number exist in the return request details?
                int returnDeviceCount = (from rd in adc.rma_ReturnDevices
                                         where rd.ReturnID == returnRequestID 
                                         && rd.Active == true
                                         && rd.SerialNo == serialNo
                                         && rd.Received == false 
                                         select rd).Count();


                rma_ReturnDevice returnDevice = null;

                // Grab the return Device or create it (returnDevices table)
                if (returnDeviceCount >= 1) // If the device was found...
                {
                    // Yes, update it to received.
                    returnDevice = (from rd in adc.rma_ReturnDevices
                                    where rd.ReturnID == returnRequestID 
                                    && rd.Active == true
                                    && rd.SerialNo == serialNo  
                                    && rd.Received == false 
                                    orderby rd.ReturnDevicesID descending 
                                    select rd).First();

                    returnDevice.Status = 4; // Device(s) Received and is waiting to be reviewed.
                    returnDevice.DepartmentID = 7; // Mfg - Receiving
                    returnDevice.Received = true;
                    // Save changes to the database
                    adc.SubmitChanges();

                    // insert TransLog, update devices status
                    newReturnDevicesID = returnDevice.ReturnDevicesID;
                    adc.sp_rma_process(returnRequestID, newReturnDevicesID, 0,
                              " ", this.UserName, DateTime.Now, "UPDATE DEVICE",
                              "DeviceStatus=4, DepartmentID=7", 2);

                }
                else
                {
                    // No, create a return request detail it and update it to received.
                    var MDeviceID = (from a in idc.DeviceInventories
                                     where a.SerialNo == serialNo
                                     select a).First();

                    returnDevice = new rma_ReturnDevice()
                    {
                        Active = true,
                        DepartmentID = 7, // Device is with Shipping Receiving
                        Notes = "Auto generated by inventory entry.",
                        ReturnID = returnRequestID,
                        MasterDeviceID = MDeviceID.DeviceID,
                        SerialNo = serialNo,
                        Received= true, // indicated received device
                        Status = 4 // Device(s) Received and is waiting to be reviewed.
                    };

                    // Create the new returned device
                    adc.rma_ReturnDevices.InsertOnSubmit(returnDevice);
                    // Save changes to the database
                    adc.SubmitChanges();

                    // insert TransLog, add new devices
                    newReturnDevicesID = returnDevice.ReturnDevicesID;
                    adc.sp_rma_process(returnRequestID, newReturnDevicesID, 0,
                              "Auto generated by inventory entry.", this.UserName, DateTime.Now, "ADD DEVICE",
                              "DeviceStatus=4, DepartmentID=7", 2);
                }

                // Create the return inventory entry. (Return Inventory table)
                rma_ReturnInventory inventory = new rma_ReturnInventory()
                {
                    Active = true,
                    Approved = false,
                    Completed = false,
                    CreatedDate = DateTime.Now,
                    ReturnID = returnRequestID,
                    ReturnDeviceID = returnDevice.ReturnDevicesID,
                    SerialNo = serialNo,
                    ReceivedNotes= receiveOpsNotes,
                    ReceivedBy = this.UserName,
                    ReceivedDate = DateTime.Now,
                    TechOpsApproved = false
                };

                // Add the new inventory item.
                adc.rma_ReturnInventories.InsertOnSubmit(inventory);
                adc.SubmitChanges();

                // insert TransLog, add new devices
                newReturnInventoryID = inventory.ReturnInventoryID;
                adc.sp_rma_process(returnRequestID, newReturnDevicesID, newReturnInventoryID,
                               receiveOpsNotes, this.UserName, DateTime.Now, "ADD INVENTORY", "Add inventory Serial#: " + serialNo, 2);

                // Refresh the data source
                adc.Refresh(System.Data.Linq.RefreshMode.KeepChanges);
                
                try
                {
                    // Have all of the devices been received in the return request?

                    // Requery the return and check to make sure the status is currently set to "Awaiting devices.." (1)
                    var returnRequest = (from r in adc.rma_Returns
                                         where r.ReturnID == returnRequestID && r.Active == true
                                         select r).FirstOrDefault();

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
                        adc.sp_rma_process(returnRequestID, 0, 0,
                                      " ", this.UserName, DateTime.Now, "UPDATE RETURN", "Update Request status to 6.", 2);

                    }
                    else
                    {
                        // No, update the return request status to partial received.
                        returnRequest.Status = 5;  //Partial device(s) have been received by Mfg

                        // save changes.
                        adc.SubmitChanges();

                        // insert TransLog, update Retrun Status
                        adc.sp_rma_process(returnRequestID, 0, 0,
                                      " ", this.UserName, DateTime.Now, "UPDATE RETURN", "Update Request status to 5.", 2);
                    }

                    // 1/22/2015 tdo
                    // Clean all previous rma records of this serialno
                    CleanPreviousRMARecordsBySerialno(returnRequestID, serialNo);
                }

                catch { }

                // ----------- Deactivate a device off an user and account. -----------------//              
                try
                {
                    DeviceInventory deviceInventory = getDeviceInventory(serialNo);

                    // Set the device firstread is NULL when it comes back to Shipping/Receiving dept. 
                    // This makes sure the next wearer who receives the device must go through initialize process.
                    deviceInventory.FirstReadID = null;

                    // Change the Device Analysis Status. This is for Automation Inventory
                    // Need to classify if this is a Return Request or Recall Request
                    string changedDeviceAnalysisStatus = "Returned-Receiving"; // For un-expected returns and Return Request 

                    // If device returns to department "Tech Ops", we know this is a Recall
                    if (returnedDept == "Tech Ops")
                        changedDeviceAnalysisStatus = "Recalled-Receiving";
                    

                    DeviceAnalysisStatus returnedStatus = (from s in idc.DeviceAnalysisStatus where s.DeviceAnalysisName == changedDeviceAnalysisStatus select s).FirstOrDefault();
                    if (returnedStatus != null)
                    {
                        deviceInventory.DeviceAnalysisStatus = returnedStatus;
                    }
                    
                    idc.SubmitChanges();


                    // Insert DeviceInventoryAudit record
                    DeviceManager myDeviceMnager = new DeviceManager();
                    myDeviceMnager.InsertDeviceInventoryAudit(deviceInventory.DeviceID, deviceInventory.DeviceAnalysisStatus.DeviceAnalysisStatusID, deviceInventory.FailedCalibration, "Receive Inventory", "Portal");
                    

                    // ************* Deactivate UserDevice ********************** //
                    IQueryable<UserDevice> UDev = (from a in idc.UserDevices
                                       join ad in idc.AccountDevices on a.DeviceID equals ad.DeviceID
                                       where a.DeviceID == deviceInventory.DeviceID
                                       && a.Active == true
                                       && (ad.Active == true || ad.CurrentDeviceAssign == true)
                                       select a).AsQueryable();
                    foreach (UserDevice ud in UDev)
                    {
                        ud.DeactivateDate = DateTime.Now;
                        ud.Active = false;
                        idc.SubmitChanges();
                    }
                    // ************* Deactivate UserDevice ********************** //
                                        

                    // ********** Deactivate AccountDevice ********************//
                    IQueryable<AccountDevice> ADev = (from a in idc.AccountDevices
                                                      where a.DeviceID == deviceInventory.DeviceID && (a.Active == true || a.CurrentDeviceAssign == true )
                                                      select a).AsQueryable();
                    foreach (AccountDevice ad in ADev)
                    {
                        ad.Active = false;
                        ad.DeactivateDate = DateTime.Now;
                        ad.CurrentDeviceAssign = false;
                        idc.SubmitChanges();
                    }
                    // ********** Deactivate AccountDevice ********************//

                    // insert TransLog
                    adc.sp_rma_process(returnRequestID, 0, 0, " ", this.UserName, DateTime.Now, "DEACTIVATE", "Deactive Serial# " + serialNo, 2);
                    idc.sp_InitID3ConifgurationRecord(serialNo, true);
                }
                catch { }
                // ----------- Deactivate a device off an user and account. -----------------//                             

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
        if (requestNo == "None") return System.Drawing.Color.OrangeRed;
        else return System.Drawing.Color.Transparent;
    }

    /// <summary>
    /// use a sharp background color to flag receiving department
    /// that the received device does not associated to any ACCOUNT. 
    /// </summary>
    /// <param name="accountNo"></param>
    /// <returns> String background color</returns>
    public System.Drawing.Color FuncCheckAccountNo(string accountNo)
    {
        if (accountNo == "0") return System.Drawing.Color.OrangeRed;
        else return System.Drawing.Color.Transparent;
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
                     select di).FirstOrDefault();
        }
        catch
        {
            // Set the object to null
            devIn = null;
        }

        // Return the object
        return devIn;
    }

    // 1/22/2015 tdo
    private void CleanPreviousRMARecordsBySerialno(int pReturnID, string pSerialNo)
    {
        try
        {
            // Get all RMA ReturnDevice in the past
            IQueryable<rma_ReturnDevice> rdList = (from rd in adc.rma_ReturnDevices
                                                   where rd.SerialNo == pSerialNo && rd.ReturnID < pReturnID
                                                   select rd).AsQueryable();
            foreach (rma_ReturnDevice rd in rdList)
            {
                if (!rd.Received)
                {
                    rd.Received = true;
                    rd.Status = 7;          // Device is received and inspected by Tech .
                    rd.DepartmentID = 8;    // Device is in Tech Receiving location                
                    adc.SubmitChanges();
                }                
            }

            // Get all RMA ReturnInventory in the past
            IQueryable<rma_ReturnInventory> riList = (from ri in adc.rma_ReturnInventories
                                                      where ri.SerialNo == pSerialNo && ri.ReturnID < pReturnID
                                                      select ri).AsQueryable();
            foreach (rma_ReturnInventory ri in riList)
            {
                if (!ri.Completed)
                {
                    ri.TechOpsNotes = "System filled";
                    ri.TechOpsReviewer = this.UserName;
                    ri.TechOpsReviewDate = DateTime.Now;
                    ri.CommittedDate = DateTime.Now;

                    ri.VisualInspectPass = true;
                    ri.DataInspectPass = true;
                    ri.TechOpsApproved = true;
                    ri.Approved = true;
                    ri.Active = true;
                    ri.Completed = true;
                    ri.CommittedToFinance = true;

                    adc.SubmitChanges();
                }                
            }
        }
        catch { }
        
    }

    private bool isWaitingScannedByTechOps(int pReturnID, string pSerialNo)
    {
        // Get the Return Request devices count...
        int count = (from ri in adc.rma_ReturnInventories
                                  where ri.ReturnID == pReturnID && ri.SerialNo == pSerialNo
                                    && ri.Active == true && ri.CommittedToFinance == null
                                  select ri).Count();
        if (count > 0)
            return true;
        else
            return false;
    }

}