using portal_instadose_com_v3.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class masterpages_MirionDSD : System.Web.UI.MasterPage
{
    protected override void OnPreRender(EventArgs e)
    {
        // Always redirect the user to SSL
        if (Request.Url.Host.ToLower() != "localhost" && !Request.IsSecureConnection)
        {
            Response.Redirect("https://" + Request.ServerVariables["HTTP_HOST"] + Request.ServerVariables["URL"]);
        }
        base.OnPreRender(e);
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        // Render the page title if it exists.
        if (SiteMap.CurrentNode != null)
        {
            this.lblPageTitle.Text = SiteMap.CurrentNode.Title;
            this.Page.Title = "Instadose: " + SiteMap.CurrentNode.Title;
        }

        // Hide Quick Menu from Collectoin Users
        string userName = ActiveDirectoryQueries.GetUserName();
        bool isCollectionUser = HDomainUser.IsCollectionUser(userName);

        if (isCollectionUser)
        {
            phTopLogoWithHiddenMenu.Visible = false;
            phTopLogoWithoutHiddenMenu.Visible = true;
            phTopHiddenMenu.Visible = false;

            phQuickMenu.Visible = false;
        }
        else
        {
            phTopLogoWithHiddenMenu.Visible = true;
            phTopLogoWithoutHiddenMenu.Visible = false;
            phTopHiddenMenu.Visible = true;

            phQuickMenu.Visible = true;
        }
    }
   
}
