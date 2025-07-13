using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class InstaDose_InformationFinder_Details_Case : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }
    public string YesNo(object boolean)
    {
        try
        {
            return Convert.ToBoolean(boolean) ? "Yes" : "No";
        }
        catch { return "No"; }
    }

    protected void btnBack_Click(object sender, EventArgs e)
    {
        // AccountID and ReturnID
        Label lblAccountID = (Label)fvCase.Row.FindControl("lblAccountID");
    
        Page.Response.Redirect("../Details/Account.aspx?ID=" + lblAccountID.Text.Trim() + "#CSrequest_tab");
    }
}
