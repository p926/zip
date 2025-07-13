/*
 * File: UserMaintenance
 * Author: TDO
 * Created: Jan 24, 2012
 * 
 * Maintain User of an account
 * 
 *  Copyright (C) 2010 Mirion Technologies. All Rights Reserved.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Net.Mail;
using Instadose.Data;
using Instadose.Security;
using System.Text.RegularExpressions;
using System.Configuration;
using Instadose.Security.Helpers;


public partial class InformationFinder_Details_UserMaintenance : System.Web.UI.Page
{
    private int userID = 0;
    private int accountID = 0;
    private Account account = null;

    // Create the database reference
    InsDataContext idc = new InsDataContext();

    private bool IsAccountActive(int accountid)
    {
        int count = (from acct in idc.Accounts
                     where acct.AccountID == accountid
                     && acct.Active == true
                     select acct).Count();

        if (count == 0) return false;
        else return true;
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (int.TryParse(Request.QueryString["userID"], out userID))
        {
            // If the user exists, query off of the user ID
            accountID = (from u in idc.Users where u.UserID == userID select u.AccountID).FirstOrDefault();

            var acct = (from a in idc.Accounts where a.AccountID == accountID select a).FirstOrDefault();
        }
        else
        {
            // Require the account ID to be passed.
            int.TryParse(Request.QueryString["accountID"], out accountID);
        }

        // If no account ID was passed, return the default screen.
        if (accountID == 0)
        {
            Page.Response.Redirect("../Default.aspx");
            return;
        }

        ResetValidateNote();

        if (!this.IsPostBack)
        {
            InitiateAllControls();
            SetControlsDefault();
            SetValuesToControls();
            InitialSetDisableInputControls();
        }

        // Check to see if Account is Active (not canceled).
        // If Active = true, then enable Save button, Else disable Save button.
        // User cannot make edits.
        if (IsAccountActive(accountID) == true)
        {
            btnSave.Enabled = true;
        }
        else
        {
            displayError("Account is inactive.  Please re-activate in order to save user information.");
            btnSave.Enabled = false;
        }
    }

    private void ResetValidateNote()
    {

        this.lblUserFirstNameValidate.Visible = false;
        this.lblUserLastNameValidate.Visible = false;
        this.lblUserLastRemindedValidate.Visible = false;
        this.lblUserBirthDateValidate.Visible = false;
        this.lblUserSecurityQuestionAnswerValidate.Visible = false;
        this.lblUserSecurityQuestionValidate.Visible = false;
        this.lblUserEmailValidate.Visible = false;
        this.lblUserNameValidate.Visible = false;
        this.lblUserPasswordValidate.Visible = false;
        this.lblUserGroupValidate.Visible = false;

        this.error.Visible = false;

    }

    private void InitialSetEnableInputControls()
    {
       
    }

    private void InitialSetDisableInputControls()
    {
        this.ddlUserRole.DataBind();
        ddlUserRole_SelectedIndexChanged(null, null);

        if (IsAdminUser(this.userID))
        {
            this.ddlUserRole.Enabled = false;
            this.chkBoxIsPrimaryUser.Enabled = false;
        }

        if (this.userID > 0)
        {
            this.LogIn.Visible = true;
            this.DosimeterAssignment.Visible = true;
        }
        else
        {
            this.LogIn.Visible = false;
            this.DosimeterAssignment.Visible = false;
        }


        account = (from a in idc.Accounts where a.AccountID == accountID select a).FirstOrDefault();

        if (account != null && this.account.CustomerType.CustomerTypeCode.Trim() == "CON")  // consignment account
        {
            this.DosimeterAssignment.Visible = false;
        }
    }

    private void SetControlsDefault()
    {
        try
        {
            this.ddlUserCountryID.SelectedValue = "1";
            this.ddlUserRole.SelectedValue = "1";
            this.ddlUserEmployeeType.SelectedValue = "SSN";
            this.chkBoxUserActive.Checked = true;
            this.chkBoxUserMustChangePassword.Checked = true;
            this.chkBoxUserOkToEmail.Checked = true;
            this.chkBoxUserOkToEmailHDN.Checked = true;
            this.radGenderMale.Checked = true;
            InitiateStateControl();
            InitiateGroupControl();
        }
        catch { }

    }

    private void InitiateAllControls()
    {
        // DONE by sqldatasource controls
        this.ddlUserLocation.DataBind();
        this.ddlUserRole.DataBind();

        this.ddlUserCountryID.DataBind();
        ListItem item0 = new ListItem("", "0");
        this.ddlUserCountryID.Items.Insert(0, item0);

        this.ddlUserSecurityQuestion.DataBind();
        ListItem item1 = new ListItem("", "0");
        this.ddlUserSecurityQuestion.Items.Insert(0, item1);
    }

    private void InitiateStateControl()
    {
        try
        {
            this.ddlUserStateID.DataBind();
            ListItem item0 = new ListItem("", "0");
            this.ddlUserStateID.Items.Insert(0, item0);
        }
        catch { }

    }

    private void InitiateGroupControl()
    {
        try
        {
            this.ddlUserGroup.DataBind();
            ListItem item0 = new ListItem("", "0");
            this.ddlUserGroup.Items.Insert(0, item0);
        }
        catch { }
    }

    protected void ddlUserLocation_SelectedIndexChanged(object sender, EventArgs e)
    {
        InitiateGroupControl();
    }

    protected void ddlUserCountryID_SelectedIndexChanged(object sender, EventArgs e)
    {
        InitiateStateControl();
    }

    protected void ddlUserRole_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (this.ddlUserRole.SelectedItem.Text == "Administrator")
        {
            this.chkBoxIsPrimaryUser.Enabled = true;
        }
        else
        {
            this.chkBoxIsPrimaryUser.Checked = false;
            this.chkBoxIsPrimaryUser.Enabled = false;
        }
    }

    private void SetValuesToControls()
    {
        if (this.userID == 0)    // adding mode
        {
            
            this.lblAccountID.Text = this.accountID == 0 ? "" : accountID.ToString();
            this.lblUserID.Text = this.userID == 0 ? "" : userID.ToString();
            SetFocus(this.ddlUserPrefix);
        }
        else    // updating mode           
        {
            var myUserInfo = (from u in idc.Users
                              join a in idc.AuthUsers on u.UserName equals a.UserName
                              where u.UserID == this.userID
                              select new { u, a }).FirstOrDefault();

            if (myUserInfo == null)
            {
                this.displayError("User Authentication error. Check with I.T.");
                return ;
            }

            this.lblAccountID.Text = this.accountID == 0 ? "" : accountID.ToString();
            this.lblUserID.Text = this.userID == 0 ? "" : userID.ToString();            
            
            this.ddlUserLocation.SelectedValue = (myUserInfo.u.LocationID == null ? "0" : myUserInfo.u.LocationID.ToString());
            InitiateGroupControl();
            this.ddlUserGroup.SelectedValue = (myUserInfo.u.GroupID == null ? "0" : myUserInfo.u.GroupID.ToString());
            this.ddlUserRole.SelectedValue = (myUserInfo.u.UserRoleID == null ? "0" : myUserInfo.u.UserRoleID.ToString());
            this.chkBoxIsPrimaryUser.Checked = IsAdminUser(this.userID);
            this.ddlUserPrefix.SelectedValue = (myUserInfo.u.Prefix == null ? "" : myUserInfo.u.Prefix.ToString());
            this.txtUserFirstName.Text = (myUserInfo.u.FirstName == null ? "" : myUserInfo.u.FirstName.ToString());
            this.txtUserMiddleName.Text = (myUserInfo.u.MiddleName == null ? "" : myUserInfo.u.MiddleName.ToString());
            this.txtUserLastName.Text = (myUserInfo.u.LastName == null ? "" : myUserInfo.u.LastName.ToString());

            //this.radUserGender.SelectedValue = (myUserInfo.Gender == null ? "U" : myUserInfo.Gender.ToString());
            if (myUserInfo.u.Gender != null && myUserInfo.u.Gender != 'U')
            {
                if (myUserInfo.u.Gender == 'M')
                    this.radGenderMale.Checked = true;
                else
                    this.radGenderFemale.Checked = true;
            }
            else
            {
                this.radGenderMale.Checked = false;
                this.radGenderFemale.Checked = false;
            }
            
            this.txtUserBirthDate.Text = (myUserInfo.u.BirthDate == null ? "" : myUserInfo.u.BirthDate.Value.ToShortDateString());
            this.txtUserName.Text = (myUserInfo.u.UserName == null ? "" : myUserInfo.u.UserName.ToString());

            // Do not display password
            //this.txtUserPassword.Text = (myUserInfo.Password == null ? "" : DecryptPassword(myUserInfo.Password));
            this.txtUserPassword.Text = "";

            this.txtUserEmail.Text = (myUserInfo.u.Email == null ? "" : myUserInfo.u.Email.ToString());
            this.ddlUserSecurityQuestion.SelectedValue = (myUserInfo.a.SecurityQuestionID1 == null ? "0" : myUserInfo.a.SecurityQuestionID1.ToString());
            this.txtUserSecurityQuestionAnswer.Text = (myUserInfo.a.SecurityAnswer1 == null ? "" : myUserInfo.a.SecurityAnswer1.ToString());
            this.ddlUserEmployeeType.SelectedValue = (myUserInfo.u.UserEmployeeType == null ? "" : myUserInfo.u.UserEmployeeType.ToString());
            this.txtUserEmployeeID.Text = (myUserInfo.u.UserEmployeeID == null ? "" : myUserInfo.u.UserEmployeeID.ToString());
            this.ddlUserMovable.SelectedValue = (myUserInfo.u.MovableUser == null ? "0" : myUserInfo.u.MovableUser.ToString());
            this.chkBoxUserActive.Checked = (myUserInfo.u.Active == null ? false : Convert.ToBoolean(myUserInfo.u.Active));
            this.chkBoxUserAutoDeviceAssign.Checked = (myUserInfo.u.AutoDeviceAssign == null ? false : Convert.ToBoolean(myUserInfo.u.AutoDeviceAssign));
            this.chkBoxUserMustChangePassword.Checked = (myUserInfo.a.MustChangePassword == null ? false : Convert.ToBoolean(myUserInfo.a.MustChangePassword));
            this.chkBoxUserOkToEmail.Checked = (myUserInfo.u.OkToEmail == null ? false : Convert.ToBoolean(myUserInfo.u.OkToEmail));
            this.chkBoxUserOkToEmailHDN.Checked = (myUserInfo.u.OkToEmailHDN == null ? false : Convert.ToBoolean(myUserInfo.u.OkToEmailHDN));
            this.txtUserTelephone.Text = (myUserInfo.u.Telephone == null ? "" : myUserInfo.u.Telephone.ToString());
            this.txtUserFax.Text = (myUserInfo.u.Fax == null ? "" : myUserInfo.u.Fax.ToString());
            this.txtUserAddress1.Text = (myUserInfo.u.Address1 == null ? "" : myUserInfo.u.Address1.ToString());
            this.txtUserAddress2.Text = (myUserInfo.u.Address2 == null ? "" : myUserInfo.u.Address2.ToString());
            this.txtUserAddress3.Text = (myUserInfo.u.Address3 == null ? "" : myUserInfo.u.Address3.ToString());
            this.txtUserCity.Text = (myUserInfo.u.City == null ? "" : myUserInfo.u.City.ToString());
            this.ddlUserCountryID.SelectedValue = (myUserInfo.u.CountryID == null ? "0" : myUserInfo.u.CountryID.ToString());
            InitiateStateControl();
            this.ddlUserStateID.SelectedValue = (myUserInfo.u.StateID == null ? "0" : myUserInfo.u.StateID.ToString());
            this.txtUserPostalCode.Text = (myUserInfo.u.PostalCode == null ? "" : myUserInfo.u.PostalCode.ToString());
            this.txtUserLastReminded.Text = (myUserInfo.u.LastReminded == null ? "" : myUserInfo.u.LastReminded.Value.ToShortDateString());

            SetFocus(this.ddlUserPrefix);
        }

    }

    private Boolean PassValidation()
    {
        DateTime myDate;
        string errorString = "";
        //double myDouble;
        //int myInt;

        string regExEmail = @"^(?("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))$";
        string regExUserName = @"^[a-zA-Z0-9._-]{5,50}$"; //"^[a-zA-Z''-'\s]{1,50}"

        if (this.txtUserFirstName.Text.Trim().Length == 0)
        {
            errorString = "First Name is required.";
            this.lblUserFirstNameValidate.Visible = true;
            this.lblUserFirstNameValidate.InnerText = errorString;
            this.displayError(errorString);
            return false;
        }

        if (this.txtUserLastName.Text.Trim().Length == 0)
        {
            errorString = "Last Name is required.";
            this.lblUserLastNameValidate.Visible = true;
            this.lblUserLastNameValidate.InnerText = errorString;
            this.displayError(errorString);
            return false;
        }

        if (this.txtUserBirthDate.Text.Trim().Length > 0)
        {
            if (DateTime.TryParse(this.txtUserBirthDate.Text.Trim(), out myDate) == false)
            {
                errorString = "Date of Birth is not a date. Please enter a correct date.";
                this.lblUserBirthDateValidate.Visible = true;
                this.lblUserBirthDateValidate.InnerText = errorString;
                this.displayError(errorString);
                return false;
            }
        }

        if (this.txtUserLastReminded.Text.Trim().Length > 0)
        {
            if (DateTime.TryParse(this.txtUserLastReminded.Text.Trim(), out myDate) == false)
            {
                errorString = "Last Reminded Date is not a date. Please enter a correct date.";
                this.lblUserLastRemindedValidate.Visible = true;
                this.lblUserLastRemindedValidate.InnerText = errorString;
                this.displayError(errorString);
                return false;
            }
        }
       

        if (this.txtUserSecurityQuestionAnswer.Text.Trim().Length == 0 && this.ddlUserSecurityQuestion.SelectedItem.Text.Length > 0)
        {
            errorString = "Please enter Security Answer.";
            this.lblUserSecurityQuestionAnswerValidate.Visible = true;
            this.lblUserSecurityQuestionAnswerValidate.InnerText = errorString;
            this.displayError(errorString);
            return false;
        }

        if (this.txtUserSecurityQuestionAnswer.Text.Trim().Length > 0 && this.ddlUserSecurityQuestion.SelectedItem.Text.Length == 0)
        {
            errorString = "Please select a Security Question.";
            this.lblUserSecurityQuestionValidate.Visible = true;
            this.lblUserSecurityQuestionValidate.InnerText = errorString;
            this.displayError(errorString);
            return false;
        }


        if (this.ddlUserRole.SelectedItem.Text == "GroupAdmin" && this.ddlUserGroup.SelectedItem.Text == "")
        {
            errorString = "Must select a group for GroupAdmin role.";
            this.lblUserGroupValidate.Visible = true;
            this.lblUserGroupValidate.InnerText = errorString;
            this.displayError(errorString);
            return false;
        }
   

        // Create a regular expression for the email.
        Regex regEmail = new Regex(regExEmail);

        if (this.txtUserEmail.Text.Trim().Length == 0)
        {
            errorString = "Email address is required.";
            this.lblUserEmailValidate.Visible = true;
            this.lblUserEmailValidate.InnerText = errorString;
            this.displayError(errorString);
            return false;
        }
        else if (this.txtUserEmail.Text.Trim().Length > 50)
        {
            errorString = "Email address is too large.";
            this.lblUserEmailValidate.Visible = true;
            this.lblUserEmailValidate.InnerText = errorString;
            this.displayError(errorString);
            return false;
        }
        else if (!regEmail.IsMatch(this.txtUserEmail.Text.Trim()))
        {
            errorString = "Email address is not valid.";
            this.lblUserEmailValidate.Visible = true;
            this.lblUserEmailValidate.InnerText = errorString;
            this.displayError(errorString);
            return false;
        }


        // Create a regular expression for the username
        Regex regUser = new Regex(regExUserName);

        if (this.txtUserName.Text.Trim().Length == 0)
        {
            errorString = "UserName is required.";
            this.lblUserNameValidate.Visible = true;
            this.lblUserNameValidate.InnerText = errorString;
            this.displayError(errorString);
            return false;
        }
        else if (this.txtUserName.Text.Trim().Length > 50)
        {
            errorString = "UserName is too large.";
            this.lblUserNameValidate.Visible = true;
            this.lblUserNameValidate.InnerText = errorString;
            this.displayError(errorString);
            return false;
        }
        else if (this.txtUserName.Text.Trim().Length < 5)
        {
            errorString = "UserName is too small.";
            this.lblUserNameValidate.Visible = true;
            this.lblUserNameValidate.InnerText = errorString;
            this.displayError(errorString);
            return false;
        }
        else if (GetFormerUserName(this.userID).Trim().ToLower() != this.txtUserName.Text.Trim().ToLower())
        {
            if (ValidUserName(this.txtUserName.Text.Trim()) != 1)
            {
                errorString = "UserName is not available.";
                this.lblUserNameValidate.Visible = true;
                this.lblUserNameValidate.InnerText = errorString;
                this.displayError(errorString);
                return false;
            }
        }
        else if (!regUser.IsMatch(this.txtUserName.Text.Trim()))
        {
            // And if the username is not an email address
            if (!regEmail.IsMatch(this.txtUserName.Text.Trim()))
            {
                errorString = "UserName cannot contain special characters unless it is an email address.";
                this.lblUserNameValidate.Visible = true;
                this.lblUserNameValidate.InnerText = errorString;
                this.displayError(errorString);
                return false;
            }
        }
        
        if (this.userID == 0 && this.txtUserPassword.Text.Trim().Length == 0) 
        {
            errorString = "Password is required.";
            this.lblUserPasswordValidate.Visible = true;
            this.lblUserPasswordValidate.InnerText = errorString;
            this.displayError(errorString);
            return false;          
        }
        else if (this.txtUserPassword.Text.Trim() != "")
        {
            if (!CheckValidPassword(this.txtUserPassword.Text.Trim()))
            {
                errorString = "Password must have between 8 and 20 characters. Password must have at least one uppercase letter, one lowercase letter, and one number.";
                this.lblUserNameValidate.Visible = true;
                this.lblUserNameValidate.InnerText = errorString;
                this.displayError(errorString);
                return false;
            }
            else if (this.txtUserPassword.Text.Trim().Length > 20)
            {
                errorString = "Password is too large.";
                this.lblUserPasswordValidate.Visible = true;
                this.lblUserPasswordValidate.InnerText = errorString;
                this.displayError(errorString);
                return false;
            }
            else if (this.txtUserPassword.Text.Trim().Length < 8)
            {
                errorString = "Password must be 8 characters length at least.";
                this.lblUserPasswordValidate.Visible = true;
                this.lblUserPasswordValidate.InnerText = errorString;
                this.displayError(errorString);
                return false;
            }
        }
        

        return true;
    }

    protected void CloseWindow()
    {
        ClientScript.RegisterStartupScript(this.GetType(), "closePage", "<script type='text/JavaScript'>window.close();</script>");
    }

    protected void btnBack_Click(object sender, EventArgs e)
    {
        Page.Response.Redirect("../Details/Account.aspx?ID=" + this.accountID.ToString() + "#User_tab");
    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        if (PassValidation() == true)
        {
            // Declare variables.
            int? myIntNull = null;
            DateTime? myDateTimeNull = null;
            string myStringNull = null;
            char myGender;

            // Declare DataContexts.
            User myUser = null;
            AuthUser myAuthUser = null;
            Account myAccount = null;
            UserDevice myUserDevice = null;
            AccountDevice myAccountDevice = null;

            int myUserID = this.userID;
            int myAccountID = this.accountID;
            int? myLocationID = (int.Parse(this.ddlUserLocation.SelectedValue) == 0 ? myIntNull : int.Parse(this.ddlUserLocation.SelectedValue));
            int? myGroupID = (int.Parse(this.ddlUserGroup.SelectedValue) == 0 ? myIntNull : int.Parse(this.ddlUserGroup.SelectedValue));
            string myPrefix = (this.ddlUserPrefix.SelectedValue == "" ? myStringNull : this.ddlUserPrefix.SelectedValue);
            string myFirstName = (this.txtUserFirstName.Text.Trim().Length == 0 ? myStringNull : this.txtUserFirstName.Text.Trim());
            string myMiddleName = (this.txtUserMiddleName.Text.Trim().Length == 0 ? myStringNull : this.txtUserMiddleName.Text.Trim());
            string myLastName = (this.txtUserLastName.Text.Trim().Length == 0 ? myStringNull : this.txtUserLastName.Text.Trim());
            string myUserName = (this.txtUserName.Text.Trim().Length == 0 ? myStringNull : this.txtUserName.Text.Trim());
            DateTime? myBirthDate = (this.txtUserBirthDate.Text.Trim().Length == 0 ? myDateTimeNull : DateTime.Parse(this.txtUserBirthDate.Text));

            //string myGender = (this.radUserGender.SelectedValue == "" ? myStringNull : this.radUserGender.SelectedValue);           
            if (this.radGenderMale.Checked)
                myGender = 'M';
            else if (this.radGenderFemale.Checked)
                myGender = 'F';
            else
                myGender = 'U';

            string myAddress1 = (this.txtUserAddress1.Text.Trim().Length == 0 ? myStringNull : this.txtUserAddress1.Text.Trim());
            string myAddress2 = (this.txtUserAddress2.Text.Trim().Length == 0 ? myStringNull : this.txtUserAddress2.Text.Trim());
            string myAddress3 = (this.txtUserAddress3.Text.Trim().Length == 0 ? myStringNull : this.txtUserAddress3.Text.Trim());
            string myCity = (this.txtUserCity.Text.Trim().Length == 0 ? myStringNull : this.txtUserCity.Text.Trim());
            int? myStateID = (int.Parse(this.ddlUserStateID.SelectedValue) == 0 ? myIntNull : int.Parse(this.ddlUserStateID.SelectedValue));
            string myPostalCode = (this.txtUserPostalCode.Text.Trim().Length == 0 ? myStringNull : this.txtUserPostalCode.Text.Trim());
            int? myCountryID = (int.Parse(this.ddlUserCountryID.SelectedValue) == 0 ? myIntNull : int.Parse(this.ddlUserCountryID.SelectedValue));
            string myEmail = (this.txtUserEmail.Text.Trim().Length == 0 ? myStringNull : this.txtUserEmail.Text.Trim());
            string myTelephone = (this.txtUserTelephone.Text.Trim().Length == 0 ? myStringNull : this.txtUserTelephone.Text.Trim());
            string myFax = (this.txtUserFax.Text.Trim().Length == 0 ? myStringNull : this.txtUserFax.Text.Trim());
            string myPassword = (this.txtUserPassword.Text.Trim().Length == 0 ? myStringNull : HashPassword(this.txtUserPassword.Text.Trim()));
            Boolean myMustChangePassword = this.chkBoxUserMustChangePassword.Checked;
            int? mySecurityQuestionID = (int.Parse(this.ddlUserSecurityQuestion.SelectedValue) == 0 ? myIntNull : int.Parse(this.ddlUserSecurityQuestion.SelectedValue));
            string mySecurityQuestionAnswer = (this.txtUserSecurityQuestionAnswer.Text.Trim().Length == 0 ? "" : this.txtUserSecurityQuestionAnswer.Text.Trim());
            DateTime myCreatedDate = DateTime.Today;
            Boolean myActive = this.chkBoxUserActive.Checked;
            Boolean myOkToEmail = this.chkBoxUserOkToEmail.Checked;
            Boolean myOkToEmailHDN = this.chkBoxUserOkToEmailHDN.Checked;
            string myUserEmployeeType = (this.ddlUserEmployeeType.SelectedValue == "" ? myStringNull : this.ddlUserEmployeeType.SelectedValue);
            string myUserEmployeeID = (this.txtUserEmployeeID.Text.Trim().Length == 0 ? myStringNull : this.txtUserEmployeeID.Text.Trim());
            int? myUserRoleID = (int.Parse(this.ddlUserRole.SelectedValue) == 0 ? myIntNull : int.Parse(this.ddlUserRole.SelectedValue));
            Boolean myAutoDeviceAssign = this.chkBoxUserAutoDeviceAssign.Checked;
            int? myMovableUser = (int.Parse(this.ddlUserMovable.SelectedValue) == 0 ? myIntNull : int.Parse(this.ddlUserMovable.SelectedValue));
            DateTime? myLastReminded = (this.txtUserLastReminded.Text.Trim().Length == 0 ? myDateTimeNull : DateTime.Parse(this.txtUserLastReminded.Text));

            if (myUserID == 0)  // ADD NEW User.
            {
                try
                {
                    // Create the User record.
                    myUser = new User
                    {
                        AccountID = myAccountID,
                        LocationID = Convert.ToInt32(myLocationID),
                        GroupID = myGroupID,
                        Prefix = myPrefix,
                        FirstName = myFirstName,
                        MiddleName = myMiddleName,
                        LastName = myLastName,
                        UserName = myUserName,
                        BirthDate = myBirthDate,
                        Gender = Convert.ToChar(myGender),
                        Address1 = myAddress1,
                        Address2 = myAddress2,
                        Address3 = myAddress3,
                        City = myCity,
                        StateID = myStateID,
                        PostalCode = myPostalCode,
                        CountryID = myCountryID,
                        Email = myEmail,
                        Telephone = myTelephone,
                        Fax = myFax,
                        CreatedDate = myCreatedDate,
                        LastReminded = myLastReminded,
                        Active = myActive,
                        OkToEmail = myOkToEmail,
                        OkToEmailHDN = myOkToEmailHDN,
                        UserEmployeeID = myUserEmployeeID,
                        UserEmployeeType = myUserEmployeeType,
                        UserRoleID = Convert.ToInt32(myUserRoleID),
                        AutoDeviceAssign = myAutoDeviceAssign,
                        MovableUser = Convert.ToInt32(myMovableUser)
                    };

                    // Create the AuthUser record.
                    myAuthUser = new AuthUser()
                    {
                        Active = true,
                        MustChangePassword = true,
                        CreatedDate = myCreatedDate,
                        ModifiedDate = myCreatedDate,
                        UserName = myUserName,
                        Email = myEmail,
                        PasswordHash = myPassword,
                        SecurityQuestionID1 = mySecurityQuestionID,
                        SecurityAnswer1 = mySecurityQuestionAnswer
                    };

                    // Add the User to the Instadose Application Group.
                    // This grants the User access to Instadose applications.
                    myAuthUser.AuthUserApplicationGroups.Add(new AuthUserApplicationGroup()
                    {
                        ApplicationGroupID = (from a in idc.AuthApplicationGroups
                                              where a.ApplicationGroupName == "Instadose"
                                              select a.AuthApplicationGroupID).FirstOrDefault()
                    });

                    idc.Users.InsertOnSubmit(myUser);
                    idc.AuthUsers.InsertOnSubmit(myAuthUser);
                    idc.SubmitChanges();
                    myUserID = myUser.UserID;

                    // Reset AccountAdminID of an Account IF Primary Admin. User CHECKED TRUE.
                    ResetAccountAdminID(myUserID);

                    // Close page and return Account Details.
                    btnBack_Click(sender, e);
                }
                catch (Exception ex)
                {
                    // Display the System-Generated Error Message.
                    displayError(ex.Message);
                }
            }
            else    // UPDATE EXISTING User record.
            {
                try
                {
                    // Update User record
                    myUser = (from r in idc.Users
                              where r.UserID == myUserID
                              select r).FirstOrDefault();

                    if (myUser == null) return;

                    string oldUserName = myUser.UserName;
                    int oldLocationID = myUser.LocationID;

                    myUser.AccountID = myAccountID;
                    myUser.LocationID = Convert.ToInt32(myLocationID);
                    myUser.GroupID = myGroupID;
                    myUser.Prefix = myPrefix;
                    myUser.FirstName = myFirstName;
                    myUser.MiddleName = myMiddleName;
                    myUser.LastName = myLastName;
                    myUser.UserName = myUserName;
                    myUser.BirthDate = myBirthDate;
                    myUser.Gender = Convert.ToChar(myGender);
                    myUser.Address1 = myAddress1;
                    myUser.Address2 = myAddress2;
                    myUser.Address3 = myAddress3;
                    myUser.City = myCity;
                    myUser.StateID = myStateID;
                    myUser.PostalCode = myPostalCode;
                    myUser.CountryID = myCountryID;
                    myUser.Email = myEmail;
                    myUser.Telephone = myTelephone;
                    myUser.Fax = myFax;
                    myUser.LastReminded = myLastReminded;
                    myUser.Active = myActive;
                    myUser.OkToEmail = myOkToEmail;
                    myUser.OkToEmailHDN = myOkToEmailHDN;
                    myUser.UserEmployeeID = myUserEmployeeID;
                    myUser.UserEmployeeType = myUserEmployeeType;
                    myUser.UserRoleID = Convert.ToInt32(myUserRoleID);
                    myUser.AutoDeviceAssign = myAutoDeviceAssign;
                    myUser.MovableUser = Convert.ToInt32(myMovableUser);

                    // Update AuthUser record.
                    myAuthUser = (from a in idc.AuthUsers
                                  where a.UserName == oldUserName && a.AuthUserApplicationGroups.Any(l=>l.ApplicationGroupID == 3)
                                  select a).FirstOrDefault();
                    
                    myAuthUser.Active = myActive;
                    myAuthUser.MustChangePassword = myMustChangePassword;
                    myAuthUser.ModifiedDate = DateTime.Now;
                    myAuthUser.UserName = myUserName;
                    myAuthUser.Email = myEmail;

                    if (myPassword != null)
                    {
                        myAuthUser.AuthUserPasswordHistory.Add(new AuthUserPasswordHistory()
                        {
                            PasswordHash = myAuthUser.PasswordHash,
                            ChangedDate = DateTime.Now
                        });

                        myAuthUser.PasswordHash = myPassword;
                    }
                        

                    myAuthUser.SecurityQuestionID1 = mySecurityQuestionID;
                    myAuthUser.SecurityAnswer1 = mySecurityQuestionAnswer;
                    myAuthUser.ResetToken = null;
                    myAuthUser.ResetTokenDate = null;

                    // Modified 02/27/2015 by Anuradha Nandi
                    // Update AccountDevices table with new LocationID based on the following requirements;
                    // 1.) Check to see if Account is flagged for LockLocation AND LoctionID is NOT NULL in AccountDevices table.
                    myAccount = (from acct in idc.Accounts
                                 where acct.AccountID == myAccountID
                                 && acct.LockLocation == true
                                 select acct).FirstOrDefault();

                    if (myAccount != null)
                    {
                        // 2.) Get the DeviceID associated with myUserID from the UserDevices table.

                        // Select the active Devices that belong to this User.
                        var myDevices = (from ud in idc.UserDevices
                                         where ud.UserID == myUserID && ud.Active == true
                                         select ud).ToList();                       

                        if (myDevices.Count > 0)
                        {
                            // LOOP through each and DEACTIVATE.
                            foreach (var device in myDevices)
                            {
                                // 3.) Use both myAccountID and myDeviceID to find out whether the LocationID is NOT NULL in the AccountDevices table.
                                myAccountDevice = (from ad in idc.AccountDevices
                                                   where ad.AccountID == myAccountID
                                                   && ad.DeviceID == device.DeviceID
                                                   && ad.Active == true
                                                   && ad.LocationID.HasValue
                                                   select ad).FirstOrDefault();

                                if (myAccountDevice != null)
                                {                                    
                                    // UPDATE record with new LocationID (myLocationID).
                                    myAccountDevice.LocationID = myLocationID;
                                }
                            }                                                       
                        }
                    }

                    idc.SubmitChanges();

                    // If the user is now DEACTIVATED, Unassign their Device.
                    if (myActive == false)
                    {
                        // Select the Devices that belong to this User.
                        var myDevices = (from ud in idc.UserDevices
                                         where ud.UserID == myUserID && ud.Active == true
                                         select ud).ToList();

                        // LOOP through each and DEACTIVATE.
                        foreach (var device in myDevices)
                        {
                            device.Active = false;
                            device.DeactivateDate = DateTime.Now;
                        }
                    }
                    else
                    {
                        // Get the new Location 
                        var newLocation = idc.Locations.FirstOrDefault(l => l.LocationID == myLocationID);
                        
                        // Check if there are any changes in the user's location before updating/resetting the user's device(S)
                        if (oldLocationID != newLocation.LocationID)
                        {
                            // Select the Devices that belong to this User.
                            var myDevices = (from ud in idc.UserDevices
                                             where ud.UserID == myUserID && ud.Active == true
                                             select ud).ToList();

                            // LOOP through each devices and UPDATE the device schedule based on the new Location
                            foreach (var device in myDevices)
                            {
                                // do not update device inventory for ID3 devices
                                var deviceInventory = idc.DeviceInventories.FirstOrDefault(di => di.DeviceID == device.DeviceID);
                                if (deviceInventory.Product.ProductGroupID != Instadose.Device.CONSTS.INSTADOSE_3)
                                {
                                    device.DeviceInventory.ScheduleID = newLocation.ScheduleID;
                                    device.DeviceInventory.ScheduleSyncStatus = "PENDING";
                                    SetDeviceProfile(device.DeviceInventory);
                                }                                

                                //  Update AccountDevices.LocationID
                                var accountDevice = idc.AccountDevices.FirstOrDefault(ac => ac.AccountID == myAccountID && ac.DeviceID == device.DeviceID && ac.Active);
                                if (accountDevice != null)
                                {
                                    accountDevice.LocationID = myLocationID;
                                }
                            }
                            idc.SubmitChanges();
                        }
                    }

                    // Reset AccountAdminID of an Account IF Primary Admin. User CHECKED TRUE.
                    ResetAccountAdminID(myUserID);

                    // Close page and return Account Details.
                    btnBack_Click(sender, e);
                }
                catch (Exception ex)
                {
                    // Display the System-Generated Error Message.
                    this.displayError(ex.Message);
                }
            }
        }
    }

    protected void btnUserChangePassword_Click(object sender, EventArgs e)
    {
        this.txtUserPassword.Text = GenerateValidPassword(12); // RandomString(8);
        this.chkBoxUserMustChangePassword.Checked = true;
    }

    private void SetDeviceProfile(DeviceInventory device)
    {
        device.ScheduleSyncDate = null;
        device.ScheduleTimeOffset = null;
        device.AdvertTime = 30;
        device.ConnectionTime = 30;
        device.DeviceModeID = 3;
        device.CommRetries = 15;
        device.CommInterval = 15;
        device.DiagInterval = 15;
        device.DiagAdvInterval = 15;
        if (!string.IsNullOrEmpty(device.SerialNo))
        {
            var profileOffset = int.Parse(device.SerialNo.Substring(device.SerialNo.Length - 1));
            device.PendingDeviceProfileID = profileOffset == 0 ? 70 : 60 + profileOffset;
        }
    }

    private string GenerateValidPassword(int length)
    {
        string password;
        do
        {
            //password = System.Web.Security.Membership.GeneratePassword(length, 0);
            password = RandomString(length);
        } while (!CheckValidPassword(password));

        return password;
    }

    private bool CheckValidPassword(string password)
    {
        Regex regPassword = new Regex("^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9]).{8,20}$");
        return regPassword.IsMatch(password);
    }

    public string YesNo(object boolean)
    {
        try
        {
            return Convert.ToBoolean(boolean) ? "Yes" : "No";
        }
        catch { return "No"; }
    }

    private string DecryptPassword(string password)
    {
        try
        {
            return TripleDES.Decrypt(password);
        }
        catch { return ""; }
    }

    private string HashPassword(string password)
    {
        try
        {
            return Hash.GetHash(password, Hash.HashType.SHA256);
        }
        catch { return ""; }
    }

    /// <summary>
    /// Is this Account Admin user (Accounts.AccountAdminID)?
    /// </summary>
    /// <param name="userID"></param>
    /// <returns></returns>
    private bool IsAdminUser(int userID)
    {
        try
        {
            int numUser = (from a in idc.Accounts
                           where (a.AccountAdminID == userID && a.AccountID == this.accountID)
                           select a).Count();

            return (numUser == 0 ? false : true);
        }
        catch { return false; }
    }

    private void ResetAccountAdminID(int pUserID)
    {
        if (this.chkBoxIsPrimaryUser.Checked)
        {
            Account a = (from r in idc.Accounts
                         where r.AccountID == this.accountID
                         select r).First();

            a.AccountAdminID = pUserID;

            idc.SubmitChanges();
        }
    }

    /// <summary>
    /// Determine if a username already exists.
    /// </summary>
    private int ValidUserName(string userName)
    {
        // -1 Error
        //  0 Not Valid 
        //  1 Valid
        try
        {
            // Try to find a number of users with this username
            int userCount = (from u in idc.AuthUsers where u.UserName.Trim().ToLower() == userName.Trim().ToLower() select u).Count();

            //int userCount = (from u in idc.Users where u.UserName.Trim().ToLower() == userName.Trim().ToLower() select u).Count();

            // If any number other than zero, the username is in use.
            return (userCount == 0) ? 1 : 0;
        }
        catch(Exception ex)
        {
            return -1;
        }
    }

    /// <summary>
    /// Determine if a username already exists.
    /// </summary>
    private string GetFormerUserName(int pUserID)
    {
        try
        {
            var user = (from u in idc.Users where u.UserID == pUserID select u).FirstOrDefault();

            if (user != null)
                return user.UserName;
            else
                return "";
        }
        catch
        {
            return "";
        }
    }

    /// <summary>
    /// Generate a random string
    /// </summary>
    /// <param name="size">Length of the string</param>
    /// <returns>Random string.</returns>
    public static string RandomString(int size)
    {
        Random _rng = new Random();
        string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        char[] buffer = new char[size];

        for (int i = 0; i < size; i++)
        {
            buffer[i] = _chars[_rng.Next(_chars.Length)];
        }
        return new string(buffer);
    }

    /// <summary>
    /// Perform a sudo authentication as this user.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnAuthenticate_Click(object sender, EventArgs e)
    {
        InsDataContext idc = new InsDataContext();

        if (this.userID > 0)
        {
            // Get the username of this user.
            string userName = (from u in idc.Users where u.UserID == this.userID select u.UserName).FirstOrDefault();


            string applicationID = ConfigurationManager.AppSettings["api_wid"];
            byte[] applicationKey = ConvertHelper.HexToBytes(ConfigurationManager.AppSettings["api_psk"]);
            byte[] applicationIV = ConvertHelper.HexToBytes(ConfigurationManager.AppSettings["api_iv"]);

            string encryptedToken = Authenticate.Sudo(applicationID, userName);

            if (encryptedToken == string.Empty)
            {
                this.displayError("An error occurred while authenticating the user.");
            }
            else
            {
                // Build the javascript
                string link = string.Format("{0}Login.aspx?EncryptedToken={1}", ConfigurationManager.AppSettings["api_webaddress"], encryptedToken);
                string script = string.Format("openWindow('{0}')", link);

                ClientScript.RegisterStartupScript(typeof(string), "openWindow", script, true);
            }
        }

    }

    private void displayError(string error)
    {
        //errorMsg.InnerText = string.Format("An error was encountered: {0}", error);
        errorMsg.InnerText = error;
        this.error.Visible = true;
    }
       
    private void clearError()
    {
        errorMsg.InnerText = "";
        this.error.Visible = false;
    }

   
}