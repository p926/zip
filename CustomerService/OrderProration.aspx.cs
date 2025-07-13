using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace portal_instadose_com_v3.CustomerService
{
    public partial class OrderProration : System.Web.UI.Page
    {
        int accountID = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            int.TryParse(Request.QueryString[""], out accountID);

            if (accountID == 0) Response.Redirect(string.Format("~/InformationFinder/Details/Account.aspx?ID={0}#Order_tab", accountID));

            // Modal/Dialog to set Order # and Shipping Date.

            
            if (Page.IsPostBack) return;



        }

        // Returns Proration value based on OrderID and ShippingDate.
        private void ReturnProrationRate(int orderid, DateTime shippingdate, out decimal prorationrate)
        {
            prorationrate = 0;

            
        }

        // Apply Proration Rate to ADDON devices.
    }
}