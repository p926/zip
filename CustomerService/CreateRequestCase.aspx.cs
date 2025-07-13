using Instadose;
using Instadose.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class CustomerService_CreateRequestCase : System.Web.UI.Page
{
    int AccountID = 0;
    int CaseID = 0;
    string OpenFrom = "";
    // Create the database reference
    InsDataContext idc = new InsDataContext();

    // String to hold the current username
    string UserName = "Unknown";

    protected void Page_Load(object sender, EventArgs e)
    {
        // Try to set the username
        try
        {
            this.UserName = User.Identity.Name.Split('\\')[1];
        }
        catch { this.UserName = "Unknown"; }

        ResetValidateNote();

        // grab QueryString Values
        if (Request.QueryString["AccountID"] != null)
            int.TryParse(Page.Request.QueryString["AccountID"].ToString(), out AccountID);

        if (Request.QueryString["CaseID"] != null)
            int.TryParse(Page.Request.QueryString["CaseID"].ToString(), out CaseID);

        if (Request.QueryString["OpenFrom"] != null)
            OpenFrom = Page.Request.QueryString["OpenFrom"].ToString();
        
        
        //load in Postback or Not Postback
        if (!IsPostBack)PageIsNotPostBackLoadInfo();
        if (IsPostBack) PageIsPostBackLoadInfo();

        
        // if caseID ==0 --> accountID required for new request 
        // if caseID != 0 --> edit case, fill form
        if (CaseID == 0)
        {
            lblCaseNumber.Text = "[New Request]";
            DivCaseHistory.Visible = false;
        }            
        else
        { 
            lblCaseNumber.Text = CaseID.ToString();
            DivCaseHistory.Visible = true;

            int? acctID = 0;

            acctID = (from c in idc.Cases where c.CaseID == CaseID select c.AccountID).FirstOrDefault();
            
            if (acctID != null) AccountID = Convert.ToInt32(acctID);

            if (!IsPostBack )
                FillFormData(CaseID);
        }

        // Determine the Back button should go back to an account or go back to all requests
        if (this.OpenFrom.Length > 0 && this.OpenFrom == "Account")
        {
            btnBacktoAccount.Visible = true;
            btnCancel.Visible = false;
        }
        else
        {
            btnBacktoAccount.Visible = false;
            btnCancel.Visible = true;
        }

    }

    protected void FillFormData(int fillCaseID)
    {    
        Case  myCase = (from a in idc.Cases 
                        where a.CaseID == fillCaseID
                        select a).FirstOrDefault();

        if (myCase == null) return;
        
        // Uncheck the box.
        chkbxSendEmail.Checked = false;

        // Fill form data
        txtRequestDate.Text = myCase.RequestDate.Value.ToShortDateString();
        ddlCaseStatus.SelectedValue = myCase.CaseStatusID.ToString();

        txtAccountID.Text = (myCase.AccountID != null) ? myCase.AccountID.ToString(): "" ;
        if (myCase.AccountID != 0)
        {
            int LoadAccountID = (myCase.AccountID != null && myCase.AccountID.ToString().Trim() != "")
                    ? int.Parse(myCase.AccountID.ToString()) : 0;
            ddlOrder.Visible = true;
            ddlAcctUserName.Visible = true;
            ddlSerialno.Visible = true;

            LoadddlOrder(LoadAccountID);
            LoadddlAcctUserName(LoadAccountID);
            LoadddlAcctSerialno(LoadAccountID);

            ddlOrder.SelectedValue = (myCase.OrderID != null) ? myCase.OrderID.ToString() : "0";
            ddlAcctUserName.SelectedValue = myCase.CreatedUserID.ToString();
            ddlSerialno.SelectedValue = (myCase.DeviceID != null) ? myCase.DeviceID.ToString() : "0";
            ddlTypeRequest.SelectedValue = myCase.RequestID.ToString();
        }

        LoadddlRequestReason(int.Parse(myCase.RequestID.ToString()));

        ddlRequestReason.SelectedValue = myCase.ReasonID.ToString();
        txtReasonNotes.Text = myCase.ReasonText.ToString();
            

        ddlForwardUser.SelectedValue = (myCase.ForwardedUserID != null) ? myCase.ForwardedUserID.ToString() : "0";
        txtResolvedDate.Text = (myCase.ResolvedDate != null ) ? myCase.ResolvedDate.Value.ToShortDateString() : "";

        if (myCase.ResolvedBy != null)
        {
            ListItem findItem = ddlResolvedBy.Items.FindByText(myCase.ResolvedBy);
            if (findItem != null)
                this.ddlResolvedBy.SelectedValue = findItem.Value;
            else
                this.ddlResolvedBy.SelectedValue = "0";
        }
        else
            ddlResolvedBy.SelectedValue = "0";

    }

    protected void DisplayAccountInfoGroup(int ViewAccountID)
    {
        if (ViewAccountID != 0)
        {
            ddlOrder.Visible = true;
            LoadddlOrder(ViewAccountID);

            ddlAcctUserName.Visible = true;
            LoadddlAcctUserName(ViewAccountID);

            ddlSerialno.Visible = true;
            LoadddlAcctSerialno(ViewAccountID);
        }
        else
        {
            ddlOrder.Visible = false;
            ddlAcctUserName.Visible = false;
            ddlSerialno.Visible = false;
        }
    }
    
    protected void PageIsPostBackLoadInfo()
    {
        // find control name which has casued postback 
        string controlID = this.Request.Params["__EVENTTARGET"];

        if (controlID.IndexOf("txtAccountID") > 0)
        {
            if (txtAccountID.Text.Trim() !="")
                int.TryParse(txtAccountID.Text.Trim().ToString(), out AccountID);
            
            //Is page postback, display 
            if (AccountID !=0)
                DisplayAccountInfoGroup(AccountID);
        }

        if (controlID.IndexOf("ddlTypeRequest") > 0)
        {
            if (int.Parse (ddlTypeRequest.SelectedValue) != 0) 
            {
                LoadddlRequestReason(int.Parse(ddlTypeRequest.SelectedValue));
            }
        }
    }

    protected void PageIsNotPostBackLoadInfo()
    {
        if (AccountID != 0)
        {
            txtAccountID.Text = AccountID.ToString();
            DisplayAccountInfoGroup(AccountID);
        }
        txtRequestDate.Text = DateTime.Now.ToShortDateString();
        LoadddlCaseStatus();
        LoadddlTypeRequest();
        LoadddlRequestReason(int.Parse(ddlTypeRequest.SelectedValue));
        LoadddlUser(ddlForwardUser);
        LoadddlUser(ddlResolvedBy);
    }

    /// <summary>
    /// Back to Request Queue
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnCancel_Click(object sender, EventArgs e)
    {
        //Page.Response.Redirect("RequestQueue.aspx");

        string url = String.Format("/InformationFinder/Account.aspx?ID={0}", AccountID);
        Response.Redirect(url);
    }

    protected void btnBacktoAccount_Click(object sender, EventArgs e)
    {
        if (this.txtAccountID.Text.Trim().Length > 0)
        {
            Page.Response.Redirect("~/InformationFinder/Details/Account.aspx?ID=" + this.txtAccountID.Text.Trim() + "#CSrequest_tab");
        }
    }

    #region  LoadDLLData
    protected void LoadddlCaseStatus()
    {
        ddlCaseStatus.DataSource = (from a in idc.CaseStatus 
                              orderby a.CaseStatusID  
                              select a);
        ddlCaseStatus.DataBind();
    }
    protected void LoadddlTypeRequest()
    {
        ddlTypeRequest.DataSource = (from a in idc.Requests 
                                     where a.Active== true
                                     orderby a.RequestDesc 
                                     select a);
        ddlTypeRequest.DataBind();
    }

    protected void LoadddlUser(DropDownList ddlName)
    {
        ddlName.DataSource = (from a in idc.Users
                                     where a.LocationID == 71
                                     orderby a.LastName, a.FirstName
                                     select new
                                     {
                                         a.UserID,
                                         FullName =
                                             ((a.LastName != null) ? a.LastName.ToString() : "")
                                             + ", " +
                                             ((a.FirstName != null) ? a.FirstName.ToString() : "")
                                     });
        ddlName.DataBind();
        ListItem firstItem = new ListItem(" ", "0");
        ddlName.Items.Insert(0, firstItem);
        ddlName.SelectedIndex = 0;
    }

    protected void LoadddlRequestReason(int RequestID)
    {
        ddlRequestReason.DataSource = (from a in idc.Reasons
                                      orderby a.ReasonDesc
                                     where a.RequestID== RequestID 
                                     select a);
        ddlRequestReason.DataBind();
    }
    
    protected void LoadddlAcctUserName(int AcctID)
    { 
        ddlAcctUserName.DataSource =(from a in idc.Users 
                                     where a.AccountID == AcctID 
                                     orderby a.LastName, a.FirstName 
                                     select new {a.UserID, FullName = 
                                         ((a.LastName != null) ? a.LastName.ToString() : "")
                                         + ", " +
                                         ((a.FirstName != null) ? a.FirstName.ToString() : "")
                                         + "  - " +
                                         a.UserID.ToString()
                                     });

        ddlAcctUserName.DataBind();
        ListItem firstItem = new ListItem("  -- Select User --", "0");
        ddlAcctUserName.Items.Insert(0, firstItem);
        ddlAcctUserName.SelectedIndex = 0;
    }
    
    protected void LoadddlAcctSerialno(int AcctID)
    {
        ddlSerialno.DataSource = (from a in idc.AccountDevices 
                                      join b in idc.DeviceInventories 
                                      on a.DeviceID  equals b.DeviceID 
                                      where a.AccountID == AcctID
                                      select new
                                      {
                                          b.DeviceID,b.SerialNo 
                                      });

        ddlSerialno.DataBind();
        ListItem firstItem = new ListItem("  -- Select Badge# --", "0");
        ddlSerialno.Items.Insert(0, firstItem);
        ddlSerialno.SelectedIndex = 0;
    }
    
    protected void LoadddlOrder(int AcctID)
    {
        ddlOrder.DataSource = OrderHelpers.GetOrders(AcctID); //OrderHelpers.GetOrderByAccount(AcctID);

        ddlOrder.DataBind();
        ListItem firstItem = new ListItem("  -- Select Order# --", "0");
        ddlOrder.Items.Insert(0, firstItem);
        ddlOrder.SelectedIndex = 0;
    }
    #endregion

    /// <summary>
    /// Custom Validator for AccountID
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void custValAccountID_ServerValidate(object sender, ServerValidateEventArgs e)
    {
        int myAccountID = 0;
        try
        {
            int.TryParse(e.Value.ToString(), out myAccountID);
            if (myAccountID != 0)
            {
                int AccountCount = (from a in idc.Accounts
                                    where a.AccountID == myAccountID
                                    select a).Count();
                if (AccountCount > 0)
                    e.IsValid = true;
                else
                {
                    e.IsValid = false;
                }
            }
            else
            {
                e.IsValid = false;
            }
        }
        catch
        {
            e.IsValid = false;
        }
    }

// ----------------------------------------------------------------
    /// <summary>
    /// Save/Update notes info.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnSaveCase_Click(object sender, EventArgs e)
    {
        bool IsNewCaseCreated = false;

        if (PassValidation())
        {
            string CaseRequestDate = txtRequestDate.Text.Trim();
            int CaseStatus = int.Parse(ddlCaseStatus.SelectedValue);

            int CaseAccountID = 0;
            if (txtAccountID.Text.Trim() != "")
                CaseAccountID = int.Parse(txtAccountID.Text.Trim());

            int CaseOrderID = 0;
            if (ddlOrder.Visible == true)
                CaseOrderID = int.Parse(ddlOrder.SelectedValue);

            int CaseAccountUserID = 0;
            if (ddlAcctUserName.Visible == true)
                CaseAccountUserID = int.Parse(ddlAcctUserName.SelectedValue);
            if (CaseAccountUserID == 0) CaseAccountUserID = 100; // set default accountUserID

            int CaseAccountDeviceID = 0;
            if (ddlSerialno.Visible == true)
                CaseAccountDeviceID = int.Parse(ddlSerialno.SelectedValue);

            string CaseAccountSerialNo = "";
            if (CaseAccountDeviceID != 0)
                CaseAccountSerialNo = ddlSerialno.SelectedItem.ToString();

            int CaseRequestTypeID = int.Parse(ddlTypeRequest.SelectedValue);

            int CaseRequestReasonID = 0;
            if (ddlRequestReason.Visible == true)
                CaseRequestReasonID = int.Parse(ddlRequestReason.SelectedValue);

            string CaseReason = "";
            if (txtReasonNotes.Visible == true)
                CaseReason = txtReasonNotes.Text;

            int CaseForwardToID = int.Parse(ddlForwardUser.SelectedValue);

            string CaseForwardToStr = "";
            if (CaseForwardToID != 0)
                CaseForwardToStr = ddlForwardUser.SelectedItem.ToString();

            string CaseNotes = txtNotes.Text;

            string CaseResolvedDate = txtResolvedDate.Text.Trim();

            int CaseResolvedByID = int.Parse(ddlResolvedBy.SelectedValue);

            string CaseResolvedByStr = "";
            if (CaseResolvedByID != 0)
                CaseResolvedByStr = ddlResolvedBy.SelectedItem.ToString();

            if (CaseID == 0) // New CS Request (Case).
            {
                Case newCase = null;
                newCase = new Case();

                newCase.AccountID = CaseAccountID;
                if (CaseOrderID != 0) newCase.OrderID = CaseOrderID;
                if (CaseAccountDeviceID != 0) newCase.DeviceID = CaseAccountDeviceID;
                newCase.CreatedBy = UserName;
                newCase.CreatedUserID = CaseAccountUserID;
                newCase.CaseStatusID = CaseStatus;
                newCase.CreatedDate = DateTime.Now;
                newCase.LastUpdatedDate = DateTime.Now;
                if (CaseForwardToID != 0) newCase.ForwardedUserID = CaseForwardToID;
                newCase.RequestDate = DateTime.Parse(CaseRequestDate);
                newCase.ResolvedBy = CaseResolvedByStr;
                if (CaseResolvedDate != "") newCase.ResolvedDate = DateTime.Parse(CaseResolvedDate);
                if (CaseRequestTypeID != 0) newCase.RequestID = CaseRequestTypeID;
                if (CaseRequestReasonID != 0) newCase.ReasonID = CaseRequestReasonID;
                newCase.ReasonText = CaseReason;

                idc.Cases.InsertOnSubmit(newCase);
                idc.SubmitChanges();

                // Get the @@identity for the Case record.
                CaseID = newCase.CaseID;

                IsNewCaseCreated = true;
            }
            else // Update CS Request (Case).
            {
                Case updateCase = null;
                updateCase = (from c in idc.Cases
                              where c.CaseID == CaseID
                              select c).FirstOrDefault();

                if (updateCase == null) return;

                updateCase.AccountID = CaseAccountID;
                if (CaseOrderID != 0) updateCase.OrderID = CaseOrderID;
                if (CaseAccountDeviceID != 0) updateCase.DeviceID = CaseAccountDeviceID;
                updateCase.CreatedBy = UserName;
                updateCase.CreatedUserID = CaseAccountUserID;
                updateCase.CaseStatusID = CaseStatus;
                updateCase.LastUpdatedDate = DateTime.Now;
                if (CaseForwardToID != 0) updateCase.ForwardedUserID = CaseForwardToID;
                updateCase.RequestDate = DateTime.Parse(CaseRequestDate);
                updateCase.ResolvedBy = CaseResolvedByStr;
                if (CaseResolvedDate != "") updateCase.ResolvedDate = DateTime.Parse(CaseResolvedDate);
                if (CaseRequestTypeID != 0) updateCase.RequestID = CaseRequestTypeID;
                if (CaseRequestReasonID != 0) updateCase.ReasonID = CaseRequestReasonID;
                updateCase.ReasonText = CaseReason;

                try
                {
                    idc.SubmitChanges();

                    // Set CaseID value.
                    CaseID = updateCase.CaseID;
                }
                catch (Exception ex)
                {
                    this.VisibleErrors(string.Format("An error occurred: {0}", ex.Message));
                }
            }

            btnSaveCase.Enabled = false;

            // Created CaseNotes record.
            Case request = (from c in idc.Cases where c.CaseID == CaseID select c).FirstOrDefault();

            if (request == null) return;

            request.CaseNotes.Add(new CaseNote()
            {
                CreatedBy = UserName,
                CreatedDate = DateTime.Now,
                CaseNote1 = CaseNotes,
                EmailNotification = chkbxSendEmail.Checked,
                ForwardTo = (ddlForwardUser.SelectedItem.ToString() != "0") ? ddlForwardUser.SelectedItem.ToString() : null,
                NoteForwarded = (ddlForwardUser.SelectedItem.ToString() != "0") ? true : false
            });

            try
            {
                idc.SubmitChanges();
            }
            catch (Exception ex)
            {
                this.VisibleErrors(string.Format("An error occurred: {0}", ex.Message));
            }

            // Only send the e-mail when a Forward To User is checked.
            if (chkbxSendEmail.Checked)
            {
                try
                {
                    // Send e-mail 1 time only when a new case is being created.
                    SendCaseEmail(request, CaseForwardToID, CaseID);

                    // Redirect back to Info Finder.
                    if (Request.QueryString["AccountID"] != null)
                    {
                        string url = String.Format("/InformationFinder/Details/Account.aspx?ID={0}", AccountID);
                        Response.Redirect(url);
                    }
                    else
                    {
                        Response.Redirect(string.Format("RequestQueue.aspx?CaseID={0}&IsNew={1}", CaseID, IsNewCaseCreated));
                    }
                }
                catch (Exception ex)
                {
                    this.VisibleErrors(string.Format("An error occurred: {0}", ex.Message));
                }
            }
        }
    }

    private void SendCaseEmail(Case request, int toUserID, int caseID)
    {
        // Build the case notes
        CaseNote note = (from cn in request.CaseNotes orderby cn.CreatedDate descending select cn).FirstOrDefault();

        string caseNotes = string.Format("<p>{0} said on <span style='font-style:italic;'>{1:d}</span>: {2}</p>", 
                note.CreatedBy, note.CreatedDate, note.CaseNote1);

        //Build the template fileds.
        Dictionary<string, string> fields = new Dictionary<string, string>();
        //fields.Add("CaseNumber", request.CaseID.ToString());
        fields.Add("CaseNumber", caseID.ToString());
        fields.Add("AccountName", (request.Account == null) ? "N/A" : request.Account.AccountName);
        fields.Add("AccountID", (!request.AccountID.HasValue) ? "N/A" : request.AccountID.Value.ToString());
        fields.Add("OrderID", (!request.OrderID.HasValue) ? "N/A" : request.OrderID.Value.ToString());
        fields.Add("Serialno", (request.DeviceInventory == null) ? "N/A" : request.DeviceInventory.SerialNo);
        fields.Add("FirstName", (request.User1 == null) ? "N/A" : request.User1.FirstName);
        fields.Add("LastName", (request.User1 == null) ? "N/A" : request.User1.LastName);
        fields.Add("UserName", (request.User1 == null) ? "N/A" : request.User1.UserName);
        fields.Add("Email", (request.User1 == null) ? "N/A" : request.User1.Email);
        fields.Add("RequestType", (request.Request == null) ? "N/A" : request.Request.RequestDesc);
        fields.Add("Reason", request.ReasonText);
        fields.Add("ReasonDesc", (request.Reason == null) ? "N/A" : request.Reason.ReasonDesc);
        fields.Add("Notes", caseNotes);
        fields.Add("CaseURL", string.Format("https://{0}/CustomerService/CreateRequestCase.aspx?CaseID={1}", Request.Url.Authority, request.CaseID));
        
        // Ensure a user will receive an email when it fails.
        if (request.User.Email == null || request.User.Email == "")
        {
            throw new Exception("Could not send an e-mail to the requested user. Their e-mail address does not exist.");
        }

        // Create the MessageSystem class.
        MessageSystem msgSys = new MessageSystem()
        {
            Application = "Create Request Case",
            CreatedBy = UserName,
            FromAddress = "noreply@instadose.com",
            ToAddressList = new List<string>()
        };

        // Add the user's email address.
        msgSys.ToAddressList.Add(request.User.Email);

        try
        {
            // Send the e-mail.
            msgSys.Send("Customer Service Request", String.Empty, fields);
        }
        catch
        {
            throw new Exception("Could not send an e-mail to the requested user.");
        }   
    }
    
    private Boolean PassValidation()
    {
        string errorString = "";
        DateTime myDate;

        if (this.txtAccountID.Text.Trim().Length == 0)
        {
            errorString = "Account Number is required.";
            this.lblAccountIDValidate.Visible = true;
            this.lblAccountIDValidate.InnerText = errorString;            
            this.VisibleErrors(errorString);
            SetFocus(this.txtAccountID);
            return false;
        }
        else if (!IsAccountExist(this.txtAccountID.Text.Trim()))
        {
            errorString = "Account Number is not exist.";
            this.lblAccountIDValidate.Visible = true;
            this.lblAccountIDValidate.InnerText = errorString;          
            this.VisibleErrors(errorString);
            SetFocus(this.txtAccountID);
            return false;
        }

        if (this.txtRequestDate.Text.Trim().Length == 0)
        {
            errorString = "Request Date is required.";
            this.lblRequestDateValidate.Visible = true;
            this.lblRequestDateValidate.InnerText = errorString;                      
            this.VisibleErrors(errorString);
            SetFocus(this.txtRequestDate);
            return false;
        }
        else
        {
            if (DateTime.TryParse(this.txtRequestDate.Text.Trim(), out myDate) == false)
            {
                errorString = "Request Date is not a date. Please enter a correct date.";
                this.lblRequestDateValidate.Visible = true;
                this.lblRequestDateValidate.InnerText = errorString;                
                this.VisibleErrors(errorString);
                SetFocus(this.txtRequestDate);
                return false;
            }
        }

        if (this.txtResolvedDate.Text.Trim().Length > 0)
        {
            if (DateTime.TryParse(this.txtResolvedDate.Text.Trim(), out myDate) == false)
            {
                errorString = "Resolved Date is not a date. Please enter a correct date.";
                this.lblResolvedDateValidate.Visible = true;
                this.lblResolvedDateValidate.InnerText = errorString;
                this.VisibleErrors(errorString);
                SetFocus(this.txtResolvedDate);
                return false;
            }
        }

        if (TrimNewLineString(this.txtReasonNotes.Text.Trim()).Length > 2000)
        {
            errorString = "The Reason Note is too long. Please re-enter it.";
            this.lblReasonNoteValidate.Visible = true;
            this.lblReasonNoteValidate.InnerText = errorString;
            this.VisibleErrors(errorString);
            SetFocus(this.txtReasonNotes);
            return false;
        }

        if (TrimNewLineString(this.txtNotes.Text.Trim()).Length > 2000)
        {
            errorString = "The Reason Note is too long. Please re-enter it.";
            this.lblNoteValidate.Visible = true;
            this.lblNoteValidate.InnerText = errorString;
            this.VisibleErrors(errorString);
            SetFocus(this.txtNotes);
            return false;
        }

        
        return true;
    }

    /// <summary>
    /// Trim the string by removing newline/enter characters.
    /// </summary>
    /// <param name="s">String to trim.</param>
    /// <returns>Returns a trimmed string.</returns>
    private String TrimNewLineString(String s)
    {

        String newString = "";
        if (s != null)
            newString = s.Replace("\r\n", " ");

        return newString;
    }

    private bool IsAccountExist(string pAccountID)
    {
        try
        {
            int AccountCount = (from a in idc.Accounts
                                where a.AccountID == int.Parse(pAccountID)
                                select a).Count();
            if (AccountCount > 0)
                return true;
            else
                return false;
        }
        catch (Exception)
        {
            return false;
        }
        
    }

    private void ResetValidateNote()
    {

        this.lblRequestDateValidate.Visible = false;
        this.lblAccountIDValidate.Visible = false;
        this.lblCaseStatusValidate.Visible = false;
        this.lblTypeRequestValidate.Visible = false;
        this.lblRequestReasonValidate.Visible = false;
       
        InvisibleErrors();
        InvisibleMessages();
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

}
