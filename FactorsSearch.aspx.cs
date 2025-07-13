using System;
using System.Linq;
using System.Data;

using Instadose;
using Instadose.Data;

public partial class TechOps_FactorsSearch : System.Web.UI.Page
{
    // Create the Database Reference.
    InsDataContext idc = new InsDataContext();

    // String to hold the current Username.
    string userName = "Unknown";

    protected void Page_Load(object sender, EventArgs e)
    {
        // Try to set the Username.
        try { this.userName = User.Identity.Name.Split('\\')[1]; }
        catch { this.userName = "Unknown"; }

        if (Page.IsPostBack) return;

        InvisibleErrors();
        SetFocus(txtAccountNumberSearch);
    }

    protected void btnFind_Click(object sender, EventArgs e)
    {
        InvisibleErrors();

        if (txtAccountNumberSearch.Text == "") return;

        // applicationID = 2: portal acct; applicationID = 1: CSWS acct;
        string applicationID = "2";
        string accountID = txtAccountNumberSearch.Text.Trim();
        var acct = (from a in idc.Accounts where a.AccountID.ToString() == accountID && a.Active == true select a).FirstOrDefault();

        if (rdobtnGDSAcct.Checked)
        {
            applicationID = "1";
            acct = (from a in idc.Accounts where a.GDSAccount == accountID && a.Active == true select a).FirstOrDefault();
        }                

        if (acct == null)
        {
            VisibleErrors("Invalid Account # was entered.");
            SetFocus(txtAccountNumberSearch);
            return;
        }
        
        Response.Redirect("Factors_New.aspx?AccountID=" + accountID + "&ApplicationID=" + applicationID);
    }

    // Reset Error Message.
    private void InvisibleErrors()
    {
        this.spnErrorMessage.InnerText = "";
        this.divErrorMessage.Visible = false;
    }

    // Set Error Message.
    private void VisibleErrors(string error)
    {
        this.spnErrorMessage.InnerText = error;
        this.divErrorMessage.Visible = true;
    }
}