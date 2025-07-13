using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Script.Serialization;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;
using Instadose;
using Instadose.Data;
using Instadose.Security;
using Mirion.DSD.DosimetryDocs;
using Mirion.DSD.DosimetryDocs.Entity;
using Mirion.DSD.GDS.API;
using Telerik.Web.UI;

public partial class IT_DosimetryDocUsers : System.Web.UI.Page
{
    private const string ApplicationID = "DosimetryDocs_5hmyuiCL8qfyWw";
    public enum UserState { DoesntExist = 0, FailValidation = 1, ExistsInSystem = 2, ExistsElsewhere = 3 }

    // Global Instadose & Documents DataContexta.
    InsDataContext idc = new InsDataContext();
    DocumentsDataContext dc = new DocumentsDataContext();

    // Error and Success messages.
    string error = "";
    string success = "";

    private string url = "";
    private string path = "";

    protected void Page_Init(object sender, EventArgs e)
    {
        rgExportToExcel.ItemCommand += rgExportToExcel_ItemCommand;
        rgExportToExcel.ExportCellFormatting += rgExportToExcel_ExportCellFormatting;
    }

    #region ON PAGE LOAD.
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.QueryString["action"] != null)
        {
            var js = new JavaScriptSerializer();

            string result = "";
            string action = Request.QueryString["action"];
            var email = Request.QueryString["email"];
            bool active = true;

            if (action == "save")
            {
                var existingEmail = Request.QueryString["existingEmail"];
                var accounts = Request.QueryString["accounts"];

                try
                {
                    if (existingEmail != "")
                    {
                        //bool active = true;
                        bool.TryParse(Request.QueryString["active"], out active);

                        updateUser(email, existingEmail, active);
                    }
                    else
                    {
                        createUser(email);
                    }

                    result = js.Serialize(updateAccounts(email, accounts));
                }
                catch (Exception ex)
                {
                    result = js.Serialize(new { error = ex.Message });
                }
            }
            else if (action == "validate-account")
            {
                int appID = 0;
                int.TryParse(Request.QueryString["appID"], out appID);
                result = js.Serialize(validateAccount(appID, Request.QueryString["account"]));
            }
            else if (action == "validate-email")
            {
                result = js.Serialize(validateEmail(email));
            }
            else if (action == "get-user")
            {
                result = getUser(email);
            }
            else if (action == "get-accounts")
            {
                result = getAccounts(email);
            }

            // Return the results.
            // Clear everything out.
            Response.Clear();
            Response.ClearHeaders();

            // Set the response headers.
            Response.ContentType = "application/json";
            Response.Write(result);
            Response.Flush();
            Response.End();
        }

        if (IsPostBack) return;

        InvisibleMainFormError();
        InvisibleMainFormSuccess();

        url = string.Format("~/Downloads/{0}.xlsx", rgExportToExcel.ExportSettings.FileName);
        path = Server.MapPath(url);

        this.rgExportToExcel.Rebind();
        this.rgDosimetryUsers.Rebind();
    }
    #endregion

    #region Modal/Dialog Functions (Get User, Account, and Validation).
    private string getUser(string email)
    {
        var js = new JavaScriptSerializer();

        var user = idc.AuthUsers.Where(u => u.UserName == email).Select(u => new
        {
            u.Active
        });

        return js.Serialize(user);
    }

    private bool validateAccount(int appID, string account)
    {
        // Validate with GDS.
        if (appID == 1)
        {
            try
            {
                var accRequest = new AccountRequests();
                return (accRequest.GetAccountDetails(account) != null);
            }
            catch
            {
                return true;
            }
        }

        // Validate with Instadose.
        if (appID == 2)
        {
            int accountID = 0;
            if (!int.TryParse(account, out accountID))
                return false;

            return (idc.Accounts.Where(a => a.AccountID == accountID).Count() == 1);
        }

        return false;
    }

    private object validateEmail(string email)
    {
        UserState state = UserState.DoesntExist;
        string applications = "";


        // Was the email address provided or is it blank/null?
        if (string.IsNullOrEmpty(email))
        {
            state = UserState.FailValidation;
        }
        else
        {
            // Is the e-mail address a valid e-mail format (i.e.: username@companyname.com)?
            var reu = new RegExUtility();
            if (!reu.ValidateEmailAddress(email))
            {
                state = UserState.FailValidation;
            }
            else
            {
                // Does the user exist in the AuthUsers table?
                var user = idc.AuthUsers.Where(u => u.UserName == email).FirstOrDefault();
                if (user == null)
                {
                    state = UserState.DoesntExist;
                }
                else
                {
                    // Does the user belong to Dosimetry Documents?
                    var userGroups = user.AuthUserApplicationGroups.ToList();
                    var existsInDosDocs = userGroups.Where(g => g.AuthApplicationGroup.ApplicationGroupName == "Dosimetry Documents").Count();

                    // Get a list of the applications the user belongs to.
                    applications = string.Join(", ", userGroups.Select(s => s.AuthApplicationGroup.ApplicationGroupName).ToArray());

                    // Does the user already exist in Dosimetry Documents?
                    if (existsInDosDocs > 0)
                    {
                        state = UserState.ExistsInSystem;
                    }
                    else
                    {
                        state = UserState.ExistsElsewhere;
                    }
                }
            }
        }

        // Return Object (state and application(s)).
        return new
        {
            state,
            applications
        };
    }

    private string getAccounts(string email)
    {
        var js = new JavaScriptSerializer();

        try
        {
            // ensure the user exists.
            var userAccounts = (from u in dc.Users
                                where u.UserName == email
                                select new
                                {
                                    AppID = u.ApplicationID,
                                    AppName = u.Application.ApplicationName,
                                    u.Account,
                                    u.Active
                                }).ToList().OrderBy(u => u.AppID).ThenBy(u => u.Account);

            return js.Serialize(userAccounts);
        }
        catch (Exception ex)
        {
            return js.Serialize(new { error = ex.Message });
        }
    }

    private string updateAccounts(string email, string accounts)
    {
        var js = new JavaScriptSerializer();
        try
        {
            // ensure the user exists.
            var userAccounts = (from u in dc.Users where u.UserName == email select u).ToList();

            // all incoming accounts should be serialized as:
            // 1,12345,true|2,G1234,true|1,1111,false|2,C2000,true|2,89800,true
            // appid,account,active
            foreach (var acc in accounts.Split('|'))
            {
                string[] row = acc.Split(',');
                int applicationID = 0;
                int.TryParse(row[0], out applicationID);

                string account = row[1];

                bool active;
                bool.TryParse(row[2], out active);

                if (account == "") continue;

                var user = userAccounts.Where(u =>
                    u.Account == account &&
                    u.ApplicationID == applicationID).FirstOrDefault();

                // if user doesn't exist, create it
                if (user == null)
                {
                    // create the user.
                    user = new Mirion.DSD.DosimetryDocs.Entity.User()
                    {
                        Account = account,
                        AcceptsEmails = true,
                        Active = active,
                        ApplicationID = applicationID,
                        CreatedBy = ActiveDirectoryQueries.GetUserName(),
                        CreatedDate = DateTime.Now,
                        UserName = email
                    };
                    dc.Users.InsertOnSubmit(user);
                }
                else
                {
                    // otherwise update the active
                    user.Active = active;
                    user.AcceptsEmails = active;
                }

                // save
                dc.SubmitChanges();
            }
            return js.Serialize(true);
        }
        catch (Exception ex)
        {
            return js.Serialize(new { error = ex.Message });
        }
    }

    private bool updateUser(string email, string existingEmail, bool active)
    {
        // Ensure the AuthUsers record exists.
        AuthUser authUserExisting = (from au in idc.AuthUsers where au.UserName == existingEmail select au).FirstOrDefault();
        if (authUserExisting == null) throw new Exception("The AuthUser could not be found.");

        // IF a new E-mail Address is being assigned...
        if (email != existingEmail)
        {
            // Ensure the E-mail Address is unique/free for use.
            AuthUser authUser = (from au in idc.AuthUsers where au.UserName == email select au).FirstOrDefault();
            if (authUser != null) throw new Exception("The e-mail address is not available.");

            authUserExisting.UserName = email;
            authUserExisting.Email = email;
            authUserExisting.ModifiedDate = DateTime.Now;
        }

        // Set the AuthUser record's Active state.
        if (authUserExisting.Active != active)
        {
            authUserExisting.Active = active;
            authUserExisting.ModifiedDate = DateTime.Now;
        }

        try
        {
            // Save changes to respective Databases.
            idc.SubmitChanges();
            return true;
        }
        catch
        {
            return false;
        }
    }

    private bool createUser(string email)
    {
        // Does the user exist?
        var authUserExists = (from au in idc.AuthUsers where au.UserName == email select au).Count();
        if (authUserExists > 0) throw new Exception("This e-mail address is already taken, they may already have an AMP+ or Instadose account. Please use another.");

        // Generate a random 20 character password and hash it.
        string password = Instadose.Basics.RandomString(20);
        string passwordHash = Hash.GetHash(password.Trim(), Hash.HashType.SHA256);
        string token = Hash.GetHash(passwordHash.Substring(0, 10), Hash.HashType.SHA256);

        var authUser = new AuthUser()
        {
            Active = true,
            CreatedDate = DateTime.Now,
            Email = email,
            UserName = email,
            ModifiedDate = DateTime.Now,
            PasswordHash = passwordHash,
            MustChangePassword = true
        };

        // Find the application group to assign the user to.
        var authApplicationGroup = (from ag in idc.AuthApplicationGroups
                                    where
                                        ag.AuthApplication.ApplicationID == ApplicationID &&
                                        ag.AuthApplicationParent.ApplicationID == ApplicationID
                                    select
                                        ag).FirstOrDefault();

        // Create an auth user application group to assign them access to the system.
        authUser.AuthUserApplicationGroups.Add(new AuthUserApplicationGroup()
        {
            AuthApplicationGroup = authApplicationGroup
        });

        try
        {
            // Add the user.
            idc.AuthUsers.InsertOnSubmit(authUser);
            idc.SubmitChanges();

            // send the new user email...
            var mSys = new MessageSystem()
            {
                Application = "DosimetryDocUsers.aspx",
                CreatedBy = ActiveDirectoryQueries.GetUserName(),
                FromAddress = "noreply@dosimetry.com",
                ToAddressList = new List<string>()
            };

            mSys.ToAddressList.Add(email);

            var fields = new Dictionary<string, string>();
            string url = string.Format("https://docs.dosimetry.com/Account/Setup/{0}/{1}", token, email);
            fields.Add("AccountSetupUrl", url);
            mSys.Send("new-dosimetry-docs-user", "", fields);

            // Show Sucess Message.
            success = "Account set-up e-mail was successfully sent.";
            VisibleMainFormSuccess(success);

            return true;
        }
        catch (Exception ex)
        {
            // Show Error Message.
            error = ex.ToString();
            VisibleMainFormError(error);

            return false;
        }
    }

    protected void btnRebind_Click(object sender, EventArgs e)
    {
        // Reset Error/Success Messages.
        InvisibleMainFormError();
        InvisibleMainFormSuccess();

        this.rgDosimetryUsers.Rebind();
        this.rgExportToExcel.Rebind();
    }
    #endregion

    # region Dosimetry Users RadGrid Functions.
    /// <summary>
    /// Get data from IRV_DosimetryDocs DB and bind to RadGrid. 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    protected void rgDosimetryUsers_NeedDataSource(object source, GridNeedDataSourceEventArgs e)
    {
        // Bind the results to the RadGrid.
        rgDosimetryUsers.DataSource = idc.AuthUsers.Select(u =>
            new
            {
                u.AuthUserApplicationGroups,
                Active = u.Active ? "Yes" : "No",
                u.UserName,
                u.SecurityQuestion1.SecurityQuestionText,
                u.SecurityAnswer1,
                u.ModifiedDate,
                u.CreatedDate,
                u.AuthUserID
                //GroupCount = u.AuthUserApplicationGroups.Count()
            })
            .Where(u => u.AuthUserApplicationGroups.Where(ag => ag.AuthApplicationGroup.AuthApplication.ApplicationID == ApplicationID).Count() == 1)
            .OrderByDescending(d => d.CreatedDate);
    }

    protected void rgDosimetryUsers_ItemCommand(object sender, Telerik.Web.UI.GridCommandEventArgs e)
    {
        this.rgDosimetryUsers.Rebind();
    }

    protected void rgDosimetryUsers_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
    {
        SortOrder(rgDosimetryUsers, e);
    }

    /// <summary>
    /// Set Sort Order based on current Sort Expression/Direction for each RadGrid.
    /// </summary>
    /// <param name="rg"></param>
    /// <param name="e"></param>
    private void SortOrder(RadGrid rg, Telerik.Web.UI.GridSortCommandEventArgs e)
    {
        GridSortExpression sortExpr = new GridSortExpression();
        switch (e.OldSortOrder)
        {
            case GridSortOrder.None:
                sortExpr.FieldName = e.SortExpression;
                sortExpr.SortOrder = GridSortOrder.Descending;

                e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                break;
            case GridSortOrder.Ascending:
                sortExpr.FieldName = e.SortExpression;
                sortExpr.SortOrder = rg.MasterTableView.AllowNaturalSort ? GridSortOrder.None : GridSortOrder.Descending;
                e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                break;
            case GridSortOrder.Descending:
                sortExpr.FieldName = e.SortExpression;
                sortExpr.SortOrder = GridSortOrder.Ascending;
                e.Item.OwnerTableView.SortExpressions.AddSortExpression(sortExpr);
                break;
        }

        e.Canceled = true;
        rg.Rebind();
    }

    /// <summary>
    /// Clear ALL filters in RadGrid.
    /// Need to fix this.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void lnkbtnClearFilters_DosimetryUsers_Click(object sender, EventArgs e)
    {
        foreach (GridColumn column in this.rgDosimetryUsers.MasterTableView.OwnerGrid.Columns)
        {
            column.CurrentFilterFunction = GridKnownFunction.NoFilter;
            column.CurrentFilterValue = string.Empty;
        }
        this.rgDosimetryUsers.MasterTableView.FilterExpression = string.Empty;
        this.rgDosimetryUsers.Rebind();
    }
    # endregion

    #region Additions as of 01/28/2014 by Anuradha Nandi.
    // Code Region for Resend Set-Up E-mail and Delete Functionalities.

    /// <summary>
    /// WebMethod that gets the Security Question ID (if one is available).
    /// Otherwise, if NULL, then outputs 0.
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    [WebMethod]
    public static int ShowHideResendAndDelete(string email, int authuserid)
    {
        InsDataContext idc = new InsDataContext();

        int? sqID = null;
        int state = 0;

        // User does not have a Security Question on record.
        var authUserRecord = (from au in idc.AuthUsers
                              where au.UserName == email
                              && au.AuthUserID == authuserid
                              select au).FirstOrDefault();

        sqID = authUserRecord.SecurityQuestionID1;

        if (sqID.HasValue) sqID = authUserRecord.SecurityQuestionID1;
        else sqID = 0;

        // Number of Application Groups User is associated with.
        int agCount = (from au in idc.AuthUsers
                       join auag in idc.AuthUserApplicationGroups on au.AuthUserID equals auag.AuthUserID
                       where au.UserName == email
                       && au.AuthUserID == authuserid
                       select auag).Count();

        // Show both Resend and Delete icons.
        if (sqID == 0 && agCount == 1)
        {
            state = 1;
        }

        // Do not show either Resend or Delete icons.
        if (sqID != 0 && agCount == 1)
        {
            state = 2;
        }

        // Show only Resend icon.
        if (sqID == 0 && agCount > 1)
        {
            state = 3;
        }

        // Do not show either Resend or Delete icons.
        if (sqID != 0 && agCount > 1)
        {
            state = 4;
        }

        return state;
    }

    /// <summary>
    /// Get AuthUserIDs based on E-mail/Username value (returns List of Integers).
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    private int GetAuthUserID(string email)
    {
        int authUserID = 0;

        var getAuthUserID = (from au in idc.AuthUsers
                             where au.UserName == email
                             select au).FirstOrDefault();

        if (getAuthUserID == null) return 0;
        else return authUserID = getAuthUserID.AuthUserID;
    }

    #region Resend Account Set-Up Email.
    /// <summary>
    /// Resend/Send Account Set-up E-mail to specified User (based on E-mail).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnYes_Click(object sender, EventArgs e)
    {
        // Generate a random 20-character Password and Hash it.
        string password = Instadose.Basics.RandomString(20);
        string passwordHash = Hash.GetHash(password.Trim(), Hash.HashType.SHA256);
        string token = Hash.GetHash(passwordHash.Substring(0, 10), Hash.HashType.SHA256);

        string email = Request.Form["email"].ToString();

        if (email == "") return;

        AuthUser authUser = (from au in idc.AuthUsers where au.UserName == email select au).FirstOrDefault();
        authUser.MustChangePassword = true;
        authUser.PasswordHash = passwordHash;
        authUser.ModifiedDate = DateTime.Now;

        // save the user.
        idc.SubmitChanges();

        // Send the new User the Set-Up E-mail.
        var mSys = new MessageSystem()
        {
            Application = "DosimetryDocUsers.aspx",
            CreatedBy = ActiveDirectoryQueries.GetUserName(),
            FromAddress = "noreply@dosimetry.com",
            ToAddressList = new List<string>()
        };

        mSys.ToAddressList.Add(email);

        var fields = new Dictionary<string, string>();
        string url = string.Format("https://docs.dosimetry.com/Account/Setup/{0}/{1}", token, email);
        fields.Add("AccountSetupUrl", url);
        mSys.Send("new-dosimetry-docs-user", "", fields);

        try
        {
            // Show Sucess Message.
            success = "Account set-up e-mail was successfully re-sent.";
            VisibleMainFormSuccess(success);

            // Close Modal/Dialog.
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "$('.ui-dialog-content').dialog('close')", true);
        }
        catch (Exception ex)
        {
            // Show Error/Failure Message.
            error = ex.ToString();
            VisibleMainFormError(error);
        }
    }
    #endregion

    #region Delete User.
    /// <summary>
    /// Delete User to specified User (based on E-mail).
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnOK_Click(object sender, EventArgs e)
    {
        string email = Page.Request.Form["email"].ToString();

        if (email == "") return;

        int authUserID = 0;

        authUserID = GetAuthUserID(email);

        if (authUserID == 0) return;

        // Delete from AuthUserSessions.
        var deleteAuthSession = (from ats in idc.AuthSessions
                                 where ats.AuthUserID == authUserID
                                 select ats);

        foreach (var record in deleteAuthSession)
        {
            if (record == null) continue;
            idc.AuthSessions.DeleteOnSubmit(record);
        }

        // Delete from AuthUserPasswordHistory.
        var deleteAuthUserPasswordHistory = (from auph in idc.AuthUserPasswordHistories
                                             where auph.AuthUserID == authUserID
                                             select auph);

        foreach (var record in deleteAuthUserPasswordHistory)
        {
            if (record == null) continue;
            idc.AuthUserPasswordHistories.DeleteOnSubmit(record);
        }

        // Delete from AuthUserApplicationGroups.
        var deleteAuthUserApplicationGroup = (from auag in idc.AuthUserApplicationGroups
                                              where auag.AuthUserID == authUserID
                                              select auag).FirstOrDefault();

        idc.AuthUserApplicationGroups.DeleteOnSubmit(deleteAuthUserApplicationGroup);

        // Delete from AuthUsers.
        var deleteAuthUser = (from au in idc.AuthUsers
                              where au.AuthUserID == authUserID
                              select au).FirstOrDefault();

        idc.AuthUsers.DeleteOnSubmit(deleteAuthUser);

        // Delete from Dosimetry Docs Users table.
        var deleteUsers = (from u in dc.Users
                           where u.UserName == email
                           select u);

        foreach (var record in deleteUsers)
        {
            if (record == null) continue;
            dc.Users.DeleteOnSubmit(record);
        }

        try
        {
            // Submit changes to database.
            idc.SubmitChanges();
            dc.SubmitChanges();

            // Close both Modals/Dialogs.
            //ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divDeleteUser')", true);
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "$('.ui-dialog-content').dialog('close')", true);

            rgDosimetryUsers.Rebind();
        }
        catch (Exception ex)
        {
            // Close both Modals/Dialogs.
            //ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divDeleteUser')", true);
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "$('.ui-dialog-content').dialog('close')", true);

            // Show Error/Failure Message.
            error = ex.ToString();
            VisibleMainFormError(error);
            return;
        }
    }
    #endregion
    #endregion

    #region Visible/Invisible Success and Failure Messages.
    // Reset Error Message (Main Form).
    private void InvisibleMainFormError()
    {
        this.spnMainFormErrorMessage.InnerText = "";
        this.divMainFormError.Visible = false;
    }

    // Set Error Message (Main Form).
    private void VisibleMainFormError(string error)
    {
        this.spnMainFormErrorMessage.InnerText = error;
        this.divMainFormError.Visible = true;
    }

    // Reset Success Message (Main Form).
    private void InvisibleMainFormSuccess()
    {
        this.spnMainFormSuccessMessage.InnerText = "";
        this.divMainFormSuccess.Visible = false;
    }

    // Set Success Message (Main Form).
    private void VisibleMainFormSuccess(string success)
    {
        this.spnMainFormSuccessMessage.InnerText = success;
        this.divMainFormSuccess.Visible = true;
    }
    #endregion

    #region Export to Excel.
    /// <summary>
    /// View Doc User Activation History from vw_DocUserActivations.
    /// The User can then filter the RadGrid then export the results to Excel.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void lnkbtnViewDocUserActivationHistory_Click(object sender, EventArgs e)
    {
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('divExportToExcel')", true);
    }

    /// <summary>
    /// Get data from IRV_DosimetryDocs DB and bind to RadGrid. 
    /// </summary>
    /// <param name="source"></param>
    /// <param name="e"></param>
    protected void rgExportToExcel_NeedDataSource(object source, GridNeedDataSourceEventArgs e)
    {
        try
        {
            DocumentsDataContext ddc = new DocumentsDataContext();
            var userActivations = (from a in ddc.vw_DocUserActivations
                                   select a).ToList();
          
            // Bind the results to the RadGrid.
            rgExportToExcel.DataSource = userActivations;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    protected void rgExportToExcel_ItemCommand(object sender, Telerik.Web.UI.GridCommandEventArgs e)
    {
        if (e.CommandName.Contains("Export"))
        {
            if (!rgExportToExcel.ExportSettings.IgnorePaging) rgExportToExcel.Rebind();
        }
    }

    /// <summary>
    /// Clear ALL filters in RadGrid.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void lnkbtnClearFilters_ExportToExcel_Click(object sender, EventArgs e)
    {
        foreach (GridColumn column in this.rgExportToExcel.MasterTableView.OwnerGrid.Columns)
        {
            column.CurrentFilterFunction = GridKnownFunction.NoFilter;
            column.CurrentFilterValue = string.Empty;
        }
        this.rgExportToExcel.MasterTableView.FilterExpression = string.Empty;
        this.rgExportToExcel.Rebind();
    }

    protected void rgExportToExcel_ExportCellFormatting(object sender, ExportCellFormattingEventArgs e)
    {
        GridItem item = e.Cell.Parent as GridItem;
        e.Cell.CssClass = (item.ItemType == GridItemType.Item) ? "excelCss" : "excelAltCss";
    }

    protected void rgExportToExcel_HTMLExporting(object sender, GridHTMLExportingEventArgs e)
    {
        string excelCss = ".table { border: 1px solid #3A6A79; font-family: Arial, Helvetical, Sans-Serif; font-size: 10pt; } ";
        excelCss += ".row { background-color: white; } ";
        string excelAltCss = ".row-alternate { background-color: #D5E3E7; }";
        string excelHeaderCss = ".heading { font-family: Tahoma, Helvetical, Arial, Sans-Serif; background-color: #3A6A79; font-weight: bold; text-align: center; color: White; } ";
        e.Styles.AppendLine(excelCss);
        e.Styles.AppendLine(excelAltCss);
        e.Styles.AppendLine(excelHeaderCss);

        foreach (TableCell cell in rgExportToExcel.MasterTableView.GetItems(GridItemType.Header)[0].Cells)
        {
            cell.CssClass = "excelHeaderCss";
        }
    }

    /// <summary>
    /// Export filtered RadGrid results to an Excel SpreadSheet.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void lnkbtnExportToExcel_Click(object sender, EventArgs e)
    {
        rgExportToExcel.ExportSettings.ExportOnlyData = true;

        rgExportToExcel.MasterTableView.ShowHeader = true;

        rgExportToExcel.ExportSettings.HideStructureColumns = true;

        rgExportToExcel.ExportSettings.FileName = "DocUserActivations";

        rgExportToExcel.ExportSettings.IgnorePaging = true;

        rgExportToExcel.ExportSettings.OpenInNewWindow = true;

        rgExportToExcel.ExportSettings.Excel.Format = Telerik.Web.UI.GridExcelExportFormat.ExcelML;

        rgExportToExcel.MasterTableView.ExportToExcel();
    }
    #endregion

    #region WebMethod to Check if User is in Dosimetry Documents Application Group.
    /// <summary>
    /// WebMethod that checks to see if the User selected (when Edit icon is clicked)
    /// is part of the Dosimetry Documents Application Group.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="authuserid"></param>
    /// <returns></returns>
    [WebMethod]
    public static bool IsUserInDosimetryDocuments(string email)
    {
        InsDataContext idcuidd = new InsDataContext();

        bool userExists;

        // Check to see if the E-mail entered already exists.
        var userRecord = (from au in idcuidd.AuthUsers
                          where au.Email == email
                          select au).FirstOrDefault();

        if (userRecord == null)
        {
            return userExists = true;   // Hide invitation.
        }
        else
        {
            // Check to see if the User is part of Dosimetry Documents (ApplicationGroupID = 9).
            int userCount = (from au in idcuidd.AuthUsers
                             join auag in idcuidd.AuthUserApplicationGroups on au.AuthUserID equals auag.AuthUserID
                             where auag.ApplicationGroupID == 9
                             && au.Email == userRecord.Email
                             select auag).Count();

            if (userCount == 0)
            {
                return userExists = false;  // Show invitation.
            }
            else
            {
                return userExists = true;   // Hide invitation.
            }
        }
    }
    #endregion
}