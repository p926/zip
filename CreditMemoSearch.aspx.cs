using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Collections;
using System.Globalization;
using System.Configuration;
using Instadose;
using Instadose.Data;


public partial class InstaDose_Finance_CreditMemoSearch : System.Web.UI.Page
{
    /// <summary>
    /// Limits access to the functionality based on groups.
    /// </summary>
    string[] activeDirecoryGroups = { "IRV-IT", "IRV-LCDISMAS" };

    // Create data table for devices
    DataTable dtDeviceDataTable = null;

    // Create the database reference
    InsDataContext idc = new InsDataContext();
    MASDataContext mdc = new MASDataContext();

    // String to hold the current username
    string UserName = "Unknown";

    protected void Page_Load(object sender, EventArgs e)
    {
        // Query active directory to see if the current user belongs to a group.
        bool belongsToGroups = ActiveDirectoryQueries.UserExistsInAnyGroup(User.Identity.Name, activeDirecoryGroups);

        //// If the user exists in the required group make the property visible.
        //if (belongsToGroups)
        //    divCreditMemoSearch.Visible = true;

        // Try to set the Username.
        try { this.UserName = User.Identity.Name.Split('\\')[1]; }
        catch { this.UserName = "Unknown"; }

    }

    protected void btnGo_Click(object sender, EventArgs e)
    {
        InvisibleErrors();

        int AccountID = 0;
        if (txtAccountNumber.Text.Trim() != "") // if AccountID not null, preform "Edit" order 
        {

            if (int.TryParse(this.txtAccountNumber.Text.Trim(), out AccountID) == true)
            {
                Account acct = (from a in idc.Accounts
                                where a.AccountID == AccountID
                                select a).FirstOrDefault();
                if (acct != null) { Response.Redirect("CreditMemo.aspx?id=" + AccountID); }
                else { VisibleErrors("Account not found in database!"); }                    
            }
            else { VisibleErrors("Account should be all digit numbers!"); }          
        }
        else { VisibleErrors("Account not found in database!"); }
    }

    private void InvisibleErrors()
    {
        // Reset submission form error message.      
        this.errorMsg.InnerText = "";
        this.errors.Visible = false;
    }

    private void VisibleErrors(string error)
    {
        this.errorMsg.InnerText = error;
        this.errors.Visible = true;
    }
}

