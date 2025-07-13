using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Mail;
using Instadose;
using Instadose.Data;
/// <summary>
/// Require ReturnID which is being passed in the querystring "ID"
/// </summary>
/// 
public partial class InstaDose_InformationFinder_Details_Return : System.Web.UI.Page
{
    int AccountID;
    int ReturnID;
    int ReturnTypeID;

    // String to hold the current username
    string UserName = "Unknown";

    // Create the database reference
    AppDataContext adc = new AppDataContext();
    InsDataContext idc = new InsDataContext();


    protected void Page_Load(object sender, EventArgs e)
    {
        string dddd = Request.Url.Authority.ToString();

        Page.Title = "InstaDose Return/RMA - Customer Service EDIT Return";
        // Try to set the username
        try
        {
            this.UserName = User.Identity.Name.Split('\\')[1];
        }
        catch { this.UserName = "Unknown"; }

        // Grab the ReturnID if it was passed in the query string
        //int ReturnID = 0;
        if (Request.QueryString["ID"] == null)
            return;

        if (Request.QueryString["ID"] != null)
            int.TryParse(Request.QueryString["ID"], out ReturnID);


        if (ReturnID != 0)
        {

            // Display the Clip buttion if any attachment found for returnID
            if (!IsPostBack)
            {
                fillAttachFile_BulletList();

                // Populate ddl Return Type
                this.LoadReturnTypes();

                this.btnCancel.TabIndex = 0;

            }

            // Get the Return Request devices count...
            int returnDevicesCount = (from r in adc.rma_Returns
                                      join d in adc.rma_ReturnDevices on r.ReturnID equals d.ReturnID
                                      where r.ReturnID == ReturnID
                                      && r.Active == true
                                      && d.Active == true
                                      select r).Count();

            if (returnDevicesCount > 0)
            {
                this.ibtnPDF.Visible = true;
                this.ibtnPDF.Attributes.Add("onclick", "ViewRMApdf(" + ReturnID.ToString() + ")");
            }
            else
            {
                this.ibtnPDF.Visible = false;
            }

            // find AccountID           
            var rmaReturn = (from r in adc.rma_Returns
                             where r.ReturnID == ReturnID
                             select r).FirstOrDefault();

            if (rmaReturn == null)
            {
                AccountID = 0;
                //this.btnAddDevice.Visible = false;
            }
            else
            {
                AccountID = rmaReturn.AccountID;
                ReturnTypeID = rmaReturn.ReturnTypeID;
            }
        }

    }


    private void LoadReturnTypes()
    {
        // Display DDL return Type
        //department 2 = Customer Services
        //department 3 = RMA
        List<rma_ref_ReturnType> returnTypes = (from rs in adc.rma_ref_ReturnTypes
                                                where rs.Active == true && (rs.DepartmentID == 2 || rs.DepartmentID == 3)
                                                select rs).ToList();


        ListItem listitem1st = new ListItem(" -- Select Return Type-- ", "0");
        this.ddlReturnType.Items.Add(listitem1st);

        foreach (rma_ref_ReturnType returnType in returnTypes)
        {
            ListItem listItem = new ListItem(returnType.Type, returnType.ReturnTypeID.ToString());

            this.ddlReturnType.Items.Add(listItem);
        }
    }


    public string YesNo(object boolean)
    {
        try
        {
            return Convert.ToBoolean(boolean) ? "Yes" : "No";
        }
        catch { return "No"; }
    }



    /// <summary>
    /// Display Attached files, if any
    /// </summary>
    private void fillAttachFile_BulletList()
    {
        // clear all listed data from bulletlist
        this.BulletedList1.Items.Clear();
        var rsUploadFiles = (from rs in adc.rma_UploadFiles
                             where rs.ReturnID == ReturnID
                             select rs).ToList();

        // Generate HTTP URL for attachment, force program run in HTTP url
        string HttpURL = Request.Url.ToString();
        HttpURL = HttpURL.Replace("https", "http");
        HttpURL = HttpURL.Substring(0, HttpURL.LastIndexOf("/") + 1);

        // Populate list items for Bulletedlist
        foreach (var vFiles in rsUploadFiles)
        {
            ListItem listAttachFile = new ListItem();
            listAttachFile.Value = HttpURL + "Return_viewAttachment.aspx?ID=" + vFiles.UploadFileID.ToString();
            listAttachFile.Text = vFiles.Filename.ToString();
            this.BulletedList1.Items.Add(listAttachFile);
        }

        this.BulletedList1.DisplayMode = BulletedListDisplayMode.HyperLink;

    }

    protected void ibtnPDF_click(object sender, EventArgs e)
    {
        // Page.Response.Redirect("../Details/Account.aspx?ID=" + AccountID.ToString() + "#Return_tab");
    }

    protected void btnCancel_Click(object sender, EventArgs e)
    {
        Page.Response.Redirect("Account.aspx?ID=" + AccountID.ToString() + "#Return_tab");
    }


    /// <summary>
    /// Send email message to Receiving and User's email account as well.
    /// </summary>
    private void SendEmailToReceiving(int RtnID)
    {
        // List of Return Devices in Inventory
        var rsInvDev = (from ri in adc.rma_ReturnInventories
                        join rr in adc.rma_Returns
                        on ri.ReturnID equals rr.ReturnID
                        where ri.ReturnID == RtnID
                        select new { ri, rr }).ToList();

        // HTML format
        string bodyText = "<html><body>";
        string URLAccountInfo = "https://" + Request.Url.Authority.ToString() + "/InformationFinder/Details/Account.aspx?ID=";
        string URLRmaInfo = "https://" + Request.Url.Authority.ToString() + "/InformationFinder/Details/Return.aspx?ID=";        

        bodyText += "<b>Return Type has been updated by Customer Services. Please release all the devices.<br><br>Details listed below:</b>";
        bodyText += "<table border='1'>";
        bodyText += "<tr><td align='center' width='60'><b>RMA#</b></td>";
        bodyText += "<td align='center'width='60'><b>Account#</b></td>";
        bodyText += "<td align='center'width='60'><b>Serial#</b></td>";
        bodyText += "<td align='center'width='100'><b>Receive Date</b></td>";
        bodyText += "<td align='center'width='220'><b>Receiving Notes</b></td></tr>";

        // Populate list items
        if (rsInvDev.Count >= 1)
        {
            foreach (var v in rsInvDev)
            {
                bodyText += "<tr>";
                bodyText += "<td align='center'><a href='" + URLRmaInfo + v.ri.ReturnID.ToString().Trim() + "'> " + v.ri.ReturnID.ToString().Trim() + "</a> </td>";
                bodyText += "<td td align='center' valign='top'><a href='" + URLAccountInfo + v.rr.AccountID.ToString().Trim() + "#Return_tab'> " + v.rr.AccountID.ToString().Trim() + "</a> </td>";
                bodyText += "<td td align='center' valign='top'> " + v.ri.SerialNo.Trim().PadRight(10, ' ') + " </td>";
                bodyText += "<td td align='center' valign='top'> " + v.ri.ReceivedDate.ToString().Trim() + " </td>";
                bodyText += "<td td align='left' valign='top'> " + v.ri.ReceivedNotes + " </td>";
                bodyText += "</tr>";

            }
        } //END if rsInvDev is null
        bodyText += "</table>";

        bodyText += "<br><br><font size='-1'>This email is generated by RMA Portal. <br>User: <b>" + this.UserName + "</b></font>";

        bodyText += "</body></html>";

        if (rsInvDev.Count >= 1)
        {
            // Send email.
            string smtpServer = string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["SmtpServer"]) ? "10.200.2.16" : System.Configuration.ConfigurationManager.AppSettings["SmtpServer"];
            SmtpClient client = new SmtpClient(smtpServer);
            MailMessage mail = new MailMessage();
            mail.IsBodyHtml = true;

            mail.From = new MailAddress("noreply@instadose.com", "RMA Portal Email");

            string MailSubject = "Release UnAuthorized RMA# " + RtnID.ToString();

            mail.Subject = MailSubject;
            mail.Body = bodyText;

            // email recipients To or CC or Bcc 
            string userEmail = this.UserName + "@mirion.com";
            mail.To.Add(userEmail);

            // Ontime ticket# 7147. Remove Kevin from receiving this email
            //mail.CC.Add(userEmail);
            //mail.To.Add("khindra@mirion.com"); // email Arnel Habil 

            try
            {
                client.Send(mail);

                // insert TransLog, Email sent
                var writeTransLogUPD = adc.sp_rma_process(RtnID, 0, 0,
                        " ", this.UserName, DateTime.Now, "EMAIL UPDATE TYPE", "Update Return Type", 2);
            }
            catch (Exception ex)
            {
                string ErrStr = "Email error occured: " + ex.Message;
            }
        }

    } // END SendEmailToReceiving


    protected void btnUploadAttachment_Click(object sender, EventArgs e)
    {

        if (FileUpload1.HasFile)
        {
            try
            {
                this.CommitUploadAttachFile(this.ReturnID);

                //reFill attachment bulletlist
                fillAttachFile_BulletList();

                // Uploadfile Control will not clear the hasfile value after upload successfully,
                // Must use redirect to current page to avoid upload same file again when user "Refreshing" the page
                string MyCurrentURL = Request.Url.ToString() + "#Upload_tab";
                Response.Redirect(MyCurrentURL);

            }
            catch (Exception ex)
            {
                string ErrStr = "Upload status: Could not be uploaded. Error occured: " + ex.Message;
            }

        }

    }// END btnUploadAttachment_Click


    /// <summary>
    /// Upload Attachment file for Request
    /// </summary>
    /// <param name="ReqID">Request ID</param>
    private void CommitUploadAttachFile(int ReqID)
    {
        // Get Upload infor and convert to byte format
        HttpPostedFile myUploadFile = null;
        myUploadFile = FileUpload1.PostedFile;

        //find upload file length and convert it to byte array
        int intContentLength = myUploadFile.ContentLength;

        //create byte array
        Byte[] bytimage = new Byte[intContentLength];

        //read upload file in byte array
        myUploadFile.InputStream.Read(bytimage, 0, intContentLength);

        string myFileName = System.IO.Path.GetFileName(myUploadFile.FileName);
        string myFileExtension = myFileName.Split('.')[1];

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


    } // end upload attach file


    /// <summary>
    /// Grid View SelectedIndexChanged, when click on "Delete" button
    /// </summary>
    /// <param name="sender">GridView DataKeyNames="ReturnDevicesID"</param>
    /// <param name="e"></param>
    protected void gvReturnDetails_SelectedIndexChanged(object sender, EventArgs e)
    {
        //Grab selected ReturnDevicesID
        GridView gv = (GridView)sender;
        int deleteID, deleteReturnID;
        int.TryParse(gv.SelectedDataKey[0].ToString(), out deleteID);
        string DeleteSerialNo = gv.SelectedDataKey[1].ToString();
        int.TryParse(gv.SelectedDataKey[2].ToString(), out deleteReturnID);

        // Create the record table
        rma_ReturnDevice returnDevice = (from rd in adc.rma_ReturnDevices
                                         where rd.ReturnDevicesID == deleteID
                                         select rd).First();

        // Deactivate the record so it can be reactivated later.
        returnDevice.Notes = "Delete by user.";
        returnDevice.Active = false;
        returnDevice.Status = 8;        // 3/28/2014. Tdo
        adc.SubmitChanges();

        // Insert Transaction Log with ReturnDevicesID
        var writeTransLogDelete = adc.sp_rma_process(ReturnID, deleteID, 0, " ",
                this.UserName, DateTime.Now, "DELETE DEVICE", "Delete device ID: "
                + deleteID.ToString(), 2);


        // check # remaining device in request. 
        try // update request status if necessary.
        {
            // Requery the return and check to make sure the status is currently set to "Awaiting devices.." (1)
            var returnRequest = (from r in adc.rma_Returns
                                 where r.ReturnID == ReturnID && r.Active == true
                                 select r).First();

            // Get the Return Request devices count...
            int returnRequestNewCount = (from r in adc.rma_Returns
                                         join d in adc.rma_ReturnDevices
                                         on r.ReturnID equals d.ReturnID
                                         where r.ReturnID == ReturnID && r.Active == true
                                         && d.Active == true
                                         select r).Count();

            // Get a count of the inventory items for the return, based on the return ID
            int inventoryCount = (from ri in adc.rma_ReturnInventories
                                  where ri.Active == true &&
                                  ri.ReturnID == ReturnID
                                  select ri).Count();

            // 3/28/2014. Tdo
            if (returnRequestNewCount == 0)
            {
                this.ibtnPDF.Visible = false;
            }

            // Do they have the same number of items?
            if (returnRequestNewCount == inventoryCount && inventoryCount != 0)
            {
                if (returnRequest.Status == 5) { returnRequest.Status = 6; }
                if (returnRequest.Status == 7) { returnRequest.Status = 8; }

                // Save changes.
                adc.SubmitChanges();

                // insert TransLog, update Retrun Status
                var writeTransLogUPD = adc.sp_rma_process(ReturnID, 0, 0,
                              " ", this.UserName, DateTime.Now, "UPDATE RETURN",
                              "Update Request status to 6 or 8 afater delete.", 2);

            }
        } // end try -- update request status if necessary.
        catch { }

        // databind gridview
        this.gvReturnDetails.DataBind();

    }

    /// <summary>
    /// if ReturnDevices Status =="1" (Awaiting return) and Active ==1 and ReturnType != Recall then display Edit Note/Delete function
    /// </summary>
    /// <param name="status"></param>
    /// <returns></returns>
    public bool DisplayLink(string status)
    {
        Boolean isDisplay = false;
        if (status == "1" && ReturnTypeID != 3)
        {
            isDisplay = true;
        }
        return isDisplay;
    }

    /// <summary>
    /// Display Date if Date != '1/1/1900'
    /// SQL return default date '1/1/1900 12:00:00 AM' is Datetime filed is empty
    /// invisable the default datetime
    /// </summary>
    /// <param name="CheckDate"></param>
    /// <returns>String Date if > 1/1/1900 </returns>
    public String FuncDateCheck(string CheckDate)
    {
        string RtnDate = "";

        if (CheckDate != "1/1/1900 12:00:00 AM")
        {
            RtnDate = CheckDate;
        }
        return RtnDate;

    }

    /// <summary>
    /// check to see if the return type id are 1 & 2=Customer Services, 3 = RMA request
    /// if not 1, 2 or 3, display the Edit Return Type
    /// </summary>
    /// <param name="returnTypeID"></param>
    /// <returns> true/false </returns>
    public bool DisplayEditReturnTypeButton(string returnTypeID)
    {
        if (returnTypeID != "1" && returnTypeID != "2" && returnTypeID != "3")
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// check to see if the return type id are 1 & 2=Customer Services, 3 = RMA request
    /// if not 1, 2 or 3, display the Add Additional Device
    /// </summary>
    /// <param name="returnTypeID"></param>
    /// <returns> true/false </returns>
    public bool DisplayAddAdditionalDeviceButton(string returnTypeID, string returnID)
    {
        // shows "Add New device to Return"
        if (returnTypeID == "3")
        {
            return false;
        }
        else
        {
            var rmaReturn = (from r in adc.rma_Returns
                             where r.ReturnID == int.Parse(returnID)
                             select r).FirstOrDefault();

            if (rmaReturn == null)
            {
                return false;
            }
            else
            {
                // inVisible Add New Device button if Return already completed
                return (rmaReturn.Status == 8) ? false : true;
            }
        }
    }


    protected void btnAddAdditionalDevice_Click(object sender, EventArgs e)
    {
        Page.Response.Redirect("../Compose/ReturnAddNewDevice.aspx?ID=" + ReturnID.ToString());
    }

    public bool DisplayRMAinfo(string ReturnTypeID)
    {
        if (ReturnTypeID == "3")
        {
            return true;
        }
        return false;
    }

    public string DisplayBillingAddress(string strOrderID)
    {
        string BillingAddressInfo = "";

        // Create the record table
        var BillShip = (from a in idc.Orders
                        join b in idc.Locations
                        on a.LocationID equals b.LocationID
                        join c in idc.States
                        on b.BillingStateID equals c.StateID
                        join d in idc.Countries
                        on b.BillingCountryID equals d.CountryID
                        where a.OrderID == int.Parse(strOrderID)
                        select new { b, c.StateAbbrev, d.CountryName }).FirstOrDefault();
        if (BillShip != null)
        {
            BillingAddressInfo = "<b><u>Replacement Bill To:</u></b></br>";
            BillingAddressInfo += BillShip.b.BillingCompany + "</br>";
            BillingAddressInfo += BillShip.b.BillingFirstName + " " + BillShip.b.BillingLastName + "</br>";
            BillingAddressInfo += BillShip.b.BillingAddress1 + "</br>";
            BillingAddressInfo += BillShip.b.BillingAddress2 + "</br>";
            BillingAddressInfo += BillShip.b.BillingAddress3 + "</br>";
            BillingAddressInfo += BillShip.b.BillingCity + ", "
                + BillShip.StateAbbrev + " " + BillShip.b.BillingPostalCode
                + " " + BillShip.CountryName + "</br>";
        }
        else
            BillingAddressInfo = "";

        return BillingAddressInfo;
    }

    public string DisplayShippingAddress(string strOrderID)
    {
        string ShippingAddressInfo = "";

        // Create the record table
        var BillShip = (from a in idc.Packages
                        join c in idc.States
                        on a.StateID equals c.StateID
                        join d in idc.Countries
                        on a.CountryID equals d.CountryID
                        where a.OrderID == int.Parse(strOrderID)
                        select new { a, c.StateAbbrev, d.CountryName }).FirstOrDefault();
        if (BillShip != null)
        {
            ShippingAddressInfo = "<b><u>Replacement Ship To:</u></b></br>";
            ShippingAddressInfo += BillShip.a.Company + "</br>";
            ShippingAddressInfo += BillShip.a.FirstName + " " + BillShip.a.LastName + "</br>";
            ShippingAddressInfo += BillShip.a.Address1 + "</br>";
            ShippingAddressInfo += BillShip.a.Address2 + "</br>";
            ShippingAddressInfo += BillShip.a.Address3 + "</br>";
            ShippingAddressInfo += BillShip.a.City + ", "
                + BillShip.StateAbbrev + " " + BillShip.a.PostalCode + " " +
                BillShip.CountryName + "</br>";
        }
        else
            ShippingAddressInfo = "";

        return ShippingAddressInfo;
    }



    // ------------------------- Modal Dialog Return Type Section FUNCTIONS here ------------------------------------- //

    private void InvisibleErrors_ReturnTypeDialog()
    {
        // Reset submission form error message      
        this.returnTypeDialogErrorMsg.InnerText = "";
        this.returnTypeDialogErrors.Visible = false;
    }

    private void VisibleErrors_ReturnTypeDialog(string error)
    {
        this.returnTypeDialogErrorMsg.InnerText = error;
        this.returnTypeDialogErrors.Visible = true;
    }

    private bool passInputsValidation_ReturnTypeDialog()
    {
        string errorString = "";

        if (int.Parse(this.ddlReturnType.SelectedValue) == 0)
        {
            errorString = "Return Type is required.";
            this.VisibleErrors_ReturnTypeDialog(errorString);
            return false;
        }
        return true;
    }

    private void SetDefaultValues_ReturnTypeDialog()
    {
        this.ddlReturnType.SelectedValue = "0";
        this.lblDialogReturnID.Text = "";
    }

    private void SetValuesToControls_ReturnTypeDialog()
    {
        try
        {
            this.lblDialogReturnID.Text = this.ReturnID.ToString();
            SetFocus(this.ddlReturnType);

            if (this.ReturnID > 0)    // edit mode
            {
                rma_Return myReturn = (from ret in adc.rma_Returns
                                       where ret.ReturnID == this.ReturnID
                                       select ret).FirstOrDefault();
                if (myReturn != null)
                {
                    if (myReturn.ReturnTypeID == null)
                    {
                        this.ddlReturnType.SelectedValue = "0";
                    }
                    else
                    {
                        ListItem findItem = ddlReturnType.Items.FindByValue(myReturn.ReturnTypeID.ToString());
                        if (findItem != null)
                            this.ddlReturnType.SelectedValue = myReturn.ReturnTypeID.ToString();
                        else
                            this.ddlReturnType.SelectedValue = "0";
                    }

                    //this.ddlReturnType.SelectedValue = (myReturn.ReturnTypeID == null ? "0" : myReturn.ReturnTypeID.ToString());
                }
                else
                {
                    this.ddlReturnType.SelectedValue = "0";
                }
            }
        }
        catch (Exception ex)
        {
            // Display the system generated error message.
            this.VisibleErrors_ReturnTypeDialog(string.Format("An error occurred: {0}", ex.Message));
        }

    }

    protected void btnNewReturnType_Click(object sender, EventArgs e)
    {
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('returnTypeDialog')", true);
    }

    protected void btnCancelReturnType_Click(object sender, EventArgs e)
    {
        InvisibleErrors_ReturnTypeDialog();
        SetDefaultValues_ReturnTypeDialog();
    }

    protected void btnLoadReturnType_Click(object sender, EventArgs e)
    {
        SetDefaultValues_ReturnTypeDialog();
        SetValuesToControls_ReturnTypeDialog();
    }

    protected void btnEditReturnType_Click(object sender, EventArgs e)
    {
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('returnTypeDialog')", true);
    }

    protected void btnAddReturnType_Click(object sender, EventArgs e)
    {
        if (this.ReturnID > 0)
        {
            if (passInputsValidation_ReturnTypeDialog())
            {

                try
                {
                    rma_Return returns = (from R in adc.rma_Returns
                                          where R.ReturnID == this.ReturnID
                                          select R).First();

                    returns.ReturnTypeID = int.Parse(this.ddlReturnType.SelectedValue);
                    adc.SubmitChanges();

                    // Insert Transaction Log with ReturnDevicesID
                    var writeTransLogDelete = adc.sp_rma_process(this.ReturnID, 0, 0, " ",
                            this.UserName, DateTime.Now, "UPDATE RETURN TYPE", "Update return type : "
                            + this.ddlReturnType.SelectedValue, 2);

                    fvReturnHeader.DataBind();

                    // Send email to Receiving,  release the "HOLD" and transfer devices to Tech Ops.
                    SendEmailToReceiving(this.ReturnID);

                    ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('returnTypeDialog')", true);

                }
                catch (Exception ex)
                {
                    // Display the system generated error message.
                    this.VisibleErrors_ReturnTypeDialog(string.Format("An error occurred: {0}", ex.Message));
                }

            }
        }

    }

    // ------------------------- Modal Dialog Return Type Section FUNCTIONS here ------------------------------------- //

    // ------------------------- Modal Note Dialog Section Functions here ------------------------------------- //

    private void InvisibleErrors_NoteDialog()
    {
        // Reset submission form error message
        this.noteDialogErrors.Visible = false;
        this.noteDialogErrorMsg.InnerText = "";
    }

    private void VisibleErrors_NoteDialog(string error)
    {
        //errorMsg.InnerText = string.Format("An error was encountered: {0}", error);
        noteDialogErrorMsg.InnerText = error;
        this.noteDialogErrors.Visible = true;
    }

    private void SetDefaultValues_NoteDialog()
    {
        this.txtReturnNotes.Text = "";
    }

    private bool passInputsValidation_NoteDialog()
    {
        string errorString = "";

        if (this.txtReturnNotes.Text.Trim().Length == 0)
        {
            errorString = "Note is required.";
            this.VisibleErrors_NoteDialog(errorString);
            return false;
        }
        return true;
    }

    protected void btnNewNote_Click(object sender, EventArgs e)
    {
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('addNoteDialog')", true);
    }

    protected void btnCancelNote_Click(object sender, EventArgs e)
    {
        InvisibleErrors_NoteDialog();
        SetDefaultValues_NoteDialog();
    }

    protected void btnAddNote_Click(object sender, EventArgs e)
    {
        try
        {
            if (this.ReturnID > 0)
            {
                if (passInputsValidation_NoteDialog())
                {
                    rma_RMAHeader header = null;
                    header = new rma_RMAHeader()
                    {
                        RMANumber = "0",
                        ReturnID = this.ReturnID,
                        Reason = this.txtReturnNotes.Text.Trim(),
                        Active = true,
                        CreatedBy = this.UserName,
                        CreatedDate = DateTime.Now
                    };
                    adc.rma_RMAHeaders.InsertOnSubmit(header);
                    adc.SubmitChanges();

                    // Insert data to Transaction Log with new ReturnID 
                    var writeTransLogReturnHeader = adc.sp_rma_process(this.ReturnID, 0, 0, this.txtReturnNotes.Text,
                            this.UserName, DateTime.Now, "ADD RETURN HEADER", "Add retrunID: "
                            + this.ReturnID.ToString(), 2);


                    // refresh gridview
                    gvNotes.DataBind();

                    ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('addNoteDialog')", true);
                }
            }
        }
        catch (Exception ex)
        {
            this.VisibleErrors_NoteDialog(string.Format("An error occurred: {0}", ex.Message));
        }
    }

    // ------------------------- Modal Note Dialog Section Function here ------------------------------------- //

}

