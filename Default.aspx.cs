using portal_instadose_com_v3.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class InstaDose_Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string userName = ActiveDirectoryQueries.GetUserName();

        bool isCollectionUser = HDomainUser.IsCollectionUser(userName);

        if (!isCollectionUser)
        {
            liInfoFinder.Visible = true;
            liFinancial.Visible = true;
            liSales.Visible = true;
            liShippingReceiving.Visible = true;
            liTechnical.Visible = true;
            liManufacturing.Visible = true;
            liCustomerService.Visible = true;
            liIT.Visible = true;
        }
        else
        {
            liInfoFinder.Visible = false;
            liFinancial.Visible = true;
            liSales.Visible = false;
            liShippingReceiving.Visible = false;
            liTechnical.Visible = false;
            liManufacturing.Visible = false;
            liCustomerService.Visible = false;
            liIT.Visible = false;
        }
    }
}
