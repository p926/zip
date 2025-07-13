using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class masterpages_MirionDSDSimple : System.Web.UI.MasterPage
{
    protected override void OnPreRender(EventArgs e)
    {
        // Always redirect the user to SSL
        if (Request.ServerVariables["https"] == "off")
            Response.Redirect("https://" + Request.ServerVariables["HTTP_HOST"] + Request.ServerVariables["URL"]);

        base.OnPreRender(e);
    }

    protected void Page_Load(object sender, EventArgs e)
    {
    }
   
}
