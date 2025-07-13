using portal_instadose_com_v3.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

public partial class InstaDose_Admin_TechOps_Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string userName = ActiveDirectoryQueries.GetUserName();

        bool isCollectionUser = HDomainUser.IsCollectionUser(userName);

        if (!isCollectionUser)
        {
            liAccountRenewal.Visible = true;
            liBatchProcessing.Visible = true;
            liAXInvoiceCreditRebill.Visible = true;
            liAXOrbitalPayment.Visible = true;
            liOMockProcessCC.Visible = true;
            liGenerateReport.Visible = true;
        }
        else
        {
            liAccountRenewal.Visible = false;
            liBatchProcessing.Visible = false;
            liAXInvoiceCreditRebill.Visible = false;
            liAXOrbitalPayment.Visible = true;
            liOMockProcessCC.Visible = false;
            liGenerateReport.Visible = false;
        }
    }
}
