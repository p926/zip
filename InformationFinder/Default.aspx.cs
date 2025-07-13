using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Instadose;
using Instadose.Data;

public partial class InstaDose_InformationFinder_Default : System.Web.UI.Page
{
    /// <summary>
    /// Limits access to the functionality based on groups.
    /// </summary>
    string[] activeDirecoryGroups = { "IRV-IT", "IRV-CustomerSvc", "IRV-Client Services" };
    InsDataContext idc = new InsDataContext();
    AppDataContext adc = new AppDataContext();

    // From web.config, prefix to GDS Portal.
    string gdsPreUrl = System.Configuration.ConfigurationManager.AppSettings["cswebsuite_webaddress"];

    protected void Page_Load(object sender, EventArgs e)
    {
        // Search by Return Request#
        String url = "https://portal.mirioncorp.com/sites/dsd/s/technology/instadose" +
                     "/ReturnRequests/Lists/Requests/EditForm.aspx?ID=";

        // Create a Javascript to open a new parent window
        // Does cause Pop-up blocker to be triggered.
        LiteralControl lc = new LiteralControl();

        lc.Text += "<script type=\"text/javascript\">\r\n";
        lc.Text += "function openWindow(ctlId) {\r\n";
        lc.Text += "  var url='" + url + "';\r\n";
        lc.Text += "  var id = document.getElementById(ctlId).value;\r\n";
        lc.Text += "  if(id == '') {\r\n";
        lc.Text += "    return true;\r\n";
        lc.Text += "  } else {\r\n";
        lc.Text += "    document.getElementById(ctlId).value = '';\r\n";
        lc.Text += "    window.open(url + id);\r\n";
        lc.Text += "    return false;\r\n";
        lc.Text += "  }\r\n";
        lc.Text += "}\r\n";
        lc.Text += "</script>\r\n";

        Header.Controls.Add(lc);
    }

    protected void btnSearch_Click(object sender, EventArgs e)
    {
        if (this.txtName.Text == "" || txtName.Text.Length < 3) return;

        // Search by Name
        List<SearchResults> lstResults = FindObscure(this.txtName.Text);
      
        this.lblResults.Text = string.Format("Found {0} result", lstResults.Count);

        // Correct result vs. results.
        if (lstResults.Count >= 0) this.lblResults.Text += ".";
        else this.lblResults.Text += "s.";

        gvSearchResults.DataSource = lstResults;
        gvSearchResults.DataBind();

        // Show the results dialog.
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('resultsDialog')", true);
    }

    protected void btnFind_Click(object sender, EventArgs e)
    {
        string PK = "";
        int AccountNo = 0;

        if (this.txtAccountNo.Text.Trim() != string.Empty)
        {
            this.txtAccountNo.Focus();
            PK = txtAccountNo.Text.Trim();
            // Search by Account#.
            if (PK != "0")
                AccountNo = FindAccountNo(PK, "Account");
        }
        else if (this.txtCSRequestNo.Text != string.Empty)
        {
            this.txtCSRequestNo.Focus();
            PK = txtCSRequestNo.Text.Trim();
            // Search by C/S Request#.
            if (PK != "0")
                AccountNo = FindAccountNo(PK, "CSRequest");
        }
        else if (this.txtDeviceNo.Text != string.Empty)
        {
            this.txtDeviceNo.Focus();
            PK = txtDeviceNo.Text.Trim();
            // Search by Device#.
            if (PK != "0")
                AccountNo = FindAccountNo(PK, "Device");
        }
        else if (txtInvoiceNo.Text != string.Empty)
        {
            txtInvoiceNo.Focus();
            PK = txtInvoiceNo.Text.Trim();
            if (PK != "0")
            {
                PK = PK.PadLeft(7, '0');
                AccountNo = FindAccountNo(PK, "Invoice");
            }
        }
        else if (this.txtOrderNo.Text != string.Empty)
        {
            this.txtOrderNo.Focus();
            PK = txtOrderNo.Text.Trim();
            // Search by Order#.
            if (PK != "0")
                AccountNo = FindAccountNo(PK, "Order");
        }
        else if (this.txtUserName.Text != string.Empty)
        {
            this.txtUserName.Focus();
            // Search by UserName.
            List<int> accountIDs = FindAccountNosByUserName(this.txtUserName.Text.Trim());

            if (accountIDs != null)
            {
                // FUTURE: If a username is associated with
                //  an account, display a list (from AccountIDs)
                //  and allow the user to choose.
                if (accountIDs.Count > 0)
                    AccountNo = accountIDs[0];
            }
        }
        else if (this.txtPONo.Text != string.Empty)
        {
            this.txtPONo.Focus();
            PK = txtPONo.Text.Trim();
            // Search by PO Number.
            // Will return the TOP 1 AccountID that matches the PO Number entered.
            AccountNo = FindAccountNo(PK, "PONumber");
        }
        else
        {
            // Hack to fire Details.
            btnDetails_Click(sender, e);
            return;
        }

        // Evaluate if the AccountID is positive.
        if (AccountNo > 0)
        {
            // Redirect to correct page.
            Page.Response.Redirect("Details/Account.aspx?ID=" + AccountNo.ToString(), false);
            Context.ApplicationInstance.CompleteRequest();
        }
        else
        {
            this.lblFindAccountError.Text = "Could not find account.";
            this.lblFindDetailsError.Text = string.Empty;
        }
    }

    // Used to determine Redirect from Instadose Portal to GDS Portal/CS Web Suite.
    // Please Note: The "Find by an Account By:" section already has the correct redirect on Details/Accounts.aspx
    private bool IsGDSAccount(int accountid)
    {
        var isGDSAccountID = (from acct in idc.Accounts
                              where acct.AccountID == accountid
                              && acct.GDSAccount != null
                              select acct).FirstOrDefault();

        if (isGDSAccountID == null) return false;

        return true;
    }

    private string ReturnGDSManageBadgeURL(int accountid, string serialno)
    {
        string gdsManageBadgeUrl = "";

        var gdsManageBadgeIDs = (from ud in idc.UserDevices
                                 join ad in idc.AccountDevices on ud.DeviceInventory.SerialNo equals ad.DeviceInventory.SerialNo
                                 join loc in idc.Locations on ad.LocationID equals loc.LocationID
                                 join acct in idc.Accounts on loc.AccountID equals acct.AccountID
                                 where acct.AccountID == accountid 
                                 && ud.DeviceInventory.SerialNo == serialno
                                 select new {
                                    GDSAccountID = acct.GDSAccount,
                                    GDSLocationID = loc.GDSLocation,
                                    GDSWearerID = ud.User.GDSWearerID,
                                    BodyRegionID = ud.BodyRegionID,
                                    ProductID = ad.DeviceInventory.ProductID
                                 }).FirstOrDefault();

        string gdsAccountID = (gdsManageBadgeIDs.GDSAccountID != null ? gdsManageBadgeIDs.GDSAccountID : null);
        string gdsLocationID = (gdsManageBadgeIDs.GDSLocationID != null ? gdsManageBadgeIDs.GDSLocationID : null);
        int? gdsWearerID = (gdsManageBadgeIDs.GDSWearerID.HasValue ? gdsManageBadgeIDs.GDSWearerID.Value : 0);
        int gdsBadgeType = (gdsManageBadgeIDs.ProductID <= 13 ? 31 : (gdsManageBadgeIDs.ProductID > 13 ? 32 : 0));
        string gdsBodyRegion = "";
        switch (gdsManageBadgeIDs.BodyRegionID)
        {
            case 1:
                gdsBodyRegion = "TRS";
                break;
            case 2:
                gdsBodyRegion = "FTL";
                break;
            case 3:
                gdsBodyRegion = "ARE";
                break;
            case 4:
                gdsBodyRegion = "UNA";
                break;
            default:
                gdsBodyRegion = "UNA";
                break;
        }

        string gdsPostUrl = "";

        if (gdsAccountID != null) 
            gdsPostUrl = string.Format("Finder/ManageBadge.aspx?Account={0}&Location={1}&WearerID={2}&BadgeTBRBP={3}{4}", gdsAccountID, gdsLocationID, gdsWearerID.ToString(), gdsBadgeType.ToString(), gdsBodyRegion);

        // If there is no GDS information for LocationID, WearerID or BadgeType, then redirect to AccountDetails.aspx on CS Web Suite.
        if (gdsLocationID == null || gdsWearerID == 0 || gdsBadgeType == 0)
            gdsManageBadgeUrl = string.Format("{0}Details/AccountDetails.aspx?Account={1}", gdsPreUrl, accountid.ToString());
        else
            gdsManageBadgeUrl = string.Format("{0}{1}", gdsPreUrl, gdsPostUrl);

        return gdsManageBadgeUrl;
    }

    private string ReturnGDSManageWearerURL(int accountid, int userid)
    {
        string gdsManageWearerUrl = "";

        var gdsManageWearerIDs = (from u in idc.Users
                                  join loc in idc.Locations on u.LocationID equals loc.LocationID
                                  join acct in idc.Accounts on loc.AccountID equals acct.AccountID
                                  where acct.AccountID == accountid && u.UserID == userid
                                  select new
                                  {
                                     GDSAccountID = acct.GDSAccount,
                                     GDSLocationID = loc.GDSLocation,
                                     GDSWearerID = u.GDSWearerID,
                                  }).FirstOrDefault();

        string gdsAccountID = (gdsManageWearerIDs.GDSAccountID != null ? gdsManageWearerIDs.GDSAccountID : null);
        string gdsLocationID = (gdsManageWearerIDs.GDSLocationID != null ? gdsManageWearerIDs.GDSLocationID : null);
        int? gdsWearerID = (gdsManageWearerIDs.GDSWearerID.HasValue ? gdsManageWearerIDs.GDSWearerID.Value : 0);

        string gdsPostUrl = string.Format("Finder/ManageWearer.aspx?Account={0}&Location={1}&WearerID={2}", gdsAccountID, gdsLocationID, gdsWearerID.ToString());

        // If there is no GDS information for LocationID or WearerID, then redirect to AccountDetails.aspx on CS Web Suite.
        if (gdsLocationID == null || gdsWearerID == 0)
            gdsManageWearerUrl = string.Format("Details/Account.aspx?ID={0}", accountid.ToString());
        else
            gdsManageWearerUrl = string.Format("{0}{1}", gdsPreUrl, gdsPostUrl);

        return gdsManageWearerUrl;
    }

    private string ReturnGDSManageLocationURL(int accountid, int userid)
    {
        string gdsManageLocationUrl = "";

        var gdsManageLocationID = (from u in idc.Users
                                   join loc in idc.Locations on u.LocationID equals loc.LocationID
                                   join acct in idc.Accounts on loc.AccountID equals acct.AccountID
                                   where acct.AccountID == accountid && u.UserID == userid
                                   select new
                                   {
                                       GDSAccountID = acct.GDSAccount,
                                       GDSLocationID = loc.GDSLocation,
                                   }).FirstOrDefault();
         
        if (gdsManageLocationID == null) return gdsManageLocationUrl = "";

        //string gdsAccountID = (gdsManageLocationID.GDSAccountID != null ? gdsManageLocationID.GDSAccountID : null);
        //string gdsLocationID = (gdsManageLocationID.GDSLocationID != null ? gdsManageLocationID.GDSLocationID : null);

        string gdsAccountID = gdsManageLocationID.GDSAccountID.ToString();
        string gdsLocationID = gdsManageLocationID.GDSLocationID.ToString();

        string gdsPostUrl = string.Format("Finder/LocationDetails.aspx?Account={0}&Location={1}", gdsAccountID, gdsLocationID);

        if (gdsLocationID == null)
            gdsManageLocationUrl = string.Format("Details/Account.aspx?ID={0}", accountid.ToString());
        else
            gdsManageLocationUrl = string.Format("{0}{1}", gdsPreUrl, gdsPostUrl);

        return gdsManageLocationUrl;
    }

    protected void btnDetails_Click(object sender, EventArgs e)
    {
        int primaryKey = 0;
        bool result = true;
        string errorMessage = string.Empty;

        if (this.txtCSRequestNoDetails.Text.Trim() != string.Empty)
        {
            result = int.TryParse(this.txtCSRequestNoDetails.Text.Trim(), out primaryKey);

            if (result == true)
            {
                var caseInfo = (from c in idc.Cases
                                where c.CaseID == primaryKey
                                select c).FirstOrDefault();

                if (caseInfo != null)
                {
                    Response.Redirect(string.Format("Details/Case.aspx?ID={0}", primaryKey.ToString()), false);
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
            else
                errorMessage = "Could not find details.";
        }
        else if (this.txtOrderNoDetails.Text.Trim() != string.Empty)
        {
            result = int.TryParse(this.txtOrderNoDetails.Text.Trim(), out primaryKey);

            if (result == true)
            {
                var orderInfo = (from o in idc.Orders
                                 where o.OrderID == primaryKey
                                 select o).FirstOrDefault();

                if (orderInfo != null)
                {
                    Response.Redirect(string.Format("../CustomerService/ReviewOrder.aspx?ID={0}", primaryKey.ToString()), false);
                    Context.ApplicationInstance.CompleteRequest();
                }
            }
            else
                errorMessage = "Could not find details.";
        }
        else if (this.txtReturnRequestNoDetails.Text.Trim() != string.Empty)
        {
            result = int.TryParse(this.txtReturnRequestNoDetails.Text.Trim(), out primaryKey);

            if (result == true)
            {
                var rmaInfo = (from r in adc.rma_Returns
                                  where r.ReturnID == primaryKey
                                  select r).FirstOrDefault();

                if (rmaInfo != null)
                    Response.Redirect(string.Format("Details/Return.aspx?ID={0}", primaryKey.ToString()));
            }
            else
                errorMessage = "Could not find details.";
        }
        else if (this.txtDeviceNoDetails.Text.Trim() != string.Empty)
        {
            string serialNumber = txtDeviceNoDetails.Text.Trim();

            var deviceInfo = (from acct in idc.Accounts
                              join ad in idc.AccountDevices on acct.AccountID equals ad.AccountID
                              where ad.DeviceInventory.SerialNo == serialNumber
                              && ad.CurrentDeviceAssign == true
                              select acct).FirstOrDefault();

            if (deviceInfo != null)
            {
                int accountID = deviceInfo.AccountID;

                if (IsGDSAccount(accountID) == false)
                    Response.Redirect(string.Format("Details/Device.aspx?ID={0}&AccountID={1}", serialNumber, accountID.ToString()), false);
                else
                    Response.Redirect(ReturnGDSManageBadgeURL(accountID, serialNumber), false);
                Context.ApplicationInstance.CompleteRequest();
            }
            else
                errorMessage = "Could not find details.";
        }
        else if (this.txtTrackingNumber.Text.Trim() != string.Empty)
        {
            string trackingNumber = txtTrackingNumber.Text.Trim();

            if (string.IsNullOrEmpty(trackingNumber)) return;            

            var lstResults = (from p in idc.Packages
                              join o in idc.Orders on p.OrderID equals o.OrderID
                              join a in idc.Accounts on o.AccountID equals a.AccountID
                              where p.TrackingNumber.Contains(trackingNumber)
                              select new
                              {
                                  a.AccountID,
                                  o.OrderID,
                                  p.TrackingNumber
                              }).ToList();


            this.lblShipmentTrackingSearchResults.Text = string.Format("Found {0} result", lstResults.Count);

            // Correct result vs. results.
            if (lstResults.Count > 0)
            {
                if (lstResults.Count == 1) this.lblShipmentTrackingSearchResults.Text += ".";
                else this.lblShipmentTrackingSearchResults.Text += "s.";

                gvTrackingSearchResults.DataSource = lstResults;
                gvTrackingSearchResults.DataBind();

                // Show the results dialog.
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('shipmentTrackingNumberSearchResultsDialog')", true);
            }
            else
                errorMessage = "Could not find details.";

        }
        else if (this.txtUserNameDetails.Text != string.Empty)
        {
            // Search by User Name (returns UserID).
            primaryKey = FindUserID(this.txtUserNameDetails.Text);

            var userInfo = (from u in idc.Users
                            where u.UserID == primaryKey
                            select u).FirstOrDefault();

            if (userInfo != null)
            {
                int accountID = userInfo.AccountID;

                if (IsGDSAccount(accountID) == false)
                    Response.Redirect(string.Format("Details/UserMaintenance.aspx?AccountID={0}&UserID={1}", accountID.ToString(), primaryKey.ToString()),false);
                else
                    Response.Redirect(ReturnGDSManageWearerURL(accountID, primaryKey),false);
                Context.ApplicationInstance.CompleteRequest();
            }
            else
                errorMessage = "Could not find details.";
        }

        this.lblFindDetailsError.Text = errorMessage;
    }

    private int FindAccountNo(string pk, string method)
    {
        try
        {
            string connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            string cmdStr = "sp_if_FindAccountNo";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.Add("@pPrimaryKey", SqlDbType.NVarChar, 50);
            sqlCmd.Parameters.Add("@pMethod", SqlDbType.NVarChar, 10);

            sqlCmd.Parameters["@pPrimaryKey"].Value = pk;
            sqlCmd.Parameters["@pMethod"].Value = method;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);

            sqlConn.Close();

            return int.Parse(dt.Rows[0][0].ToString());
        }
        catch { return 0; }
    }

    private List<int> FindAccountNosByUserName(string username)
    {
        try
        {
            string connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            string cmdStr = "sp_if_FindAccountNoByUserName";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.Add("@UserName", SqlDbType.NVarChar, 50);

            sqlCmd.Parameters["@UserName"].Value = username;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);

            sqlConn.Close();

            // Fill the list with AccountIDs.
            List<int> AccountNoList = new List<int>();

            int tmpAccountNo = 0;

            foreach (DataRow dr in dt.Rows)
            {
                tmpAccountNo = int.Parse(dr["AccountNo"].ToString());
                if (tmpAccountNo > 0)
                    AccountNoList.Add(tmpAccountNo);
            }

            return AccountNoList;
        }
        catch { return null; }
    }

    private List<SearchResults> FindObscure(string search)
    {
        /*
        string connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
        string cmdStr = "sp_if_FindAccountNoName";

        SqlConnection sqlConn = new SqlConnection(connStr);
        SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

        sqlConn.Open();

        sqlCmd.CommandType = CommandType.StoredProcedure;
        sqlCmd.Parameters.Add("@Name", SqlDbType.NVarChar, 255);

        sqlCmd.Parameters["@Name"].Value = search;

        SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
        DataTable dt = new DataTable();

        sqlDA.Fill(dt);

        sqlConn.Close();
        */
        var idc = new InsDataContext();
        var lstResults = (from s in idc.sp_if_FindAccountNoName(search)
                          select new SearchResults
                          {
                              ID = s.ID.Value,
                              AccountID = s.AccountID.Value,
                              Name = s.Name,
                              BrandName = s.BrandName,
                              Type = s.Type,
                              Active = s.Active.Value,
                              ActiveText = s.Active.HasValue && s.Active.Value ? "Yes" : "No",
                              Hyperlink = GetSearchHyperlink(s.Type, s.AccountID.Value, s.ID.Value),
                              IconPath = GetSearchIcon(s.Type)
                          }).ToList();
        // Fill the list with results
        /*
        List < SearchResults > lstResults = new List<SearchResults>();

        string imgFormat = "/images/icons/{0}.png";

        foreach (DataRow dr in dt.Rows)
        {
            SearchResultType result = SearchResultType.Unknown;

            SearchResults sr = new SearchResults()
            {
                ID = int.Parse(dr["ID"].ToString()),
                AccountID = int.Parse(dr["AccountID"].ToString()),
                Name = dr["Name"].ToString(),
                BrandName = dr["BrandName"].ToString(),
                Type = dr["Type"].ToString(),
                Active = bool.Parse(dr["Active"].ToString()),
            };

            sr.ActiveText = (sr.Active) ? "Yes" : "No";

            Enum.TryParse(dr["Type"].ToString(), out result);
            sr.ResultType = result;

            // Based on AccountID and UserID, get SerialNo.
            string serialNumber = (from ud in idc.UserDevices
                                   join ad in idc.AccountDevices on ud.DeviceID equals ad.DeviceID
                                   where ad.AccountID == sr.AccountID
                                   && ud.UserID == sr.ID
                                   && ad.CurrentDeviceAssign == true
                                   && ud.Active == true
                                   select ad.DeviceInventory.SerialNo).FirstOrDefault();

            // Set the Hyperlink and Icon Path.
            // Here there will need to be a check to see if the Account is GDS and redirect appropriately.
            // Note: Where the Hyperlinks directs the user to the Details/Account.aspx the GDS redirect is not necessary.
            switch (sr.ResultType)
            {
                case SearchResultType.Account:
                    sr.Hyperlink = string.Format("~/InformationFinder/Details/Account.aspx?ID={0}", sr.AccountID);
                    sr.IconPath = string.Format(imgFormat, "building");
                    break;
                // GDS Redirect might be required.
                case SearchResultType.Device:
                    if (IsGDSAccount(sr.AccountID) == false)
                        sr.Hyperlink = string.Format("~/InformationFinder/Details/Device.aspx?AccountID={0}&ID={1}", sr.AccountID, sr.ID);
                    else
                        sr.Hyperlink = ReturnGDSManageBadgeURL(sr.AccountID, serialNumber);
                    sr.IconPath = string.Format(imgFormat, "information");
                    break;
                case SearchResultType.Group:
                    sr.Hyperlink = string.Format("~/InformationFinder/Details/Account.aspx?ID={0}", sr.AccountID);
                    sr.IconPath = string.Format(imgFormat, "group");
                    break;
                // GDS Redirect might be required.
                case SearchResultType.Location:
                    if (IsGDSAccount(sr.AccountID) == false)
                        sr.Hyperlink = string.Format("~/InformationFinder/Details/EditLocation.aspx?accountID={0}&locationID={1}", sr.AccountID, sr.ID);
                    else
                        sr.Hyperlink = ReturnGDSManageLocationURL(sr.AccountID, sr.ID);
                    sr.IconPath = string.Format(imgFormat, "map");
                    break;
                // GDS Redirect might be required.
                case SearchResultType.User:
                    if (IsGDSAccount(sr.AccountID) == false)
                        sr.Hyperlink = string.Format("~/InformationFinder/Details/UserMaintenance.aspx?accountID={0}&userID={1}", sr.AccountID, sr.ID);
                    else
                        sr.Hyperlink = ReturnGDSManageWearerURL(sr.AccountID, sr.ID);
                    sr.IconPath = string.Format(imgFormat, "user");
                    break;
                // GDS Redirect might be required.
                case SearchResultType.PONumber:
                    if (IsGDSAccount(sr.AccountID) == false)
                        sr.Hyperlink = string.Format("~/CustomerService/ReviewOrder.aspx?ID={0}", sr.ID);
                    else
                        // Directs User to Instadose Portal Account Details page, which will redirect to CS Web Suite/GDS Portal.
                        sr.Hyperlink = string.Format("~/InformationFinder/Details/Account.aspx?ID={0}", sr.AccountID);
                    sr.IconPath = string.Format(imgFormat, "note");
                    break;
                case SearchResultType.Unknown:
                    sr.Hyperlink = string.Format("~/InformationFinder/Details/Account.aspx?ID={0}", sr.AccountID);
                    sr.IconPath = string.Format(imgFormat, "delete");
                    break;
            }

            lstResults.Add(sr);
        }
        */
        return lstResults;
    }

    private string GetSearchHyperlink(string type, int accountID, int id)
    {
        var result = SearchResultType.Unknown;
        Enum.TryParse(type, out result);
        var hyperlink = "";
        switch (result)
        {
            case SearchResultType.Account:
                hyperlink = string.Format("~/InformationFinder/Details/Account.aspx?ID={0}", accountID.ToString());
                break;
            // GDS Redirect might be required.
            case SearchResultType.Device:
                hyperlink = string.Format("~/InformationFinder/Details/Device.aspx?AccountID={0}&ID={1}", accountID.ToString(), id.ToString());
                break;
            case SearchResultType.Group:
                hyperlink = string.Format("~/InformationFinder/Details/Account.aspx?ID={0}", accountID.ToString());
                break;
            // GDS Redirect might be required.
            case SearchResultType.Location:
                hyperlink = string.Format("~/InformationFinder/Details/EditLocation.aspx?accountID={0}&locationID={1}", accountID.ToString(), id.ToString());
                break;
            // GDS Redirect might be required.
            case SearchResultType.User:
                hyperlink = string.Format("~/InformationFinder/Details/UserMaintenance.aspx?accountID={0}&userID={1}", accountID.ToString(), id.ToString());
                break;
            // GDS Redirect might be required.
            case SearchResultType.PONumber:
                hyperlink = string.Format("~/CustomerService/ReviewOrder.aspx?ID={0}", id.ToString());
                break;
            case SearchResultType.Unknown:
                hyperlink = string.Format("~/InformationFinder/Details/Account.aspx?ID={0}", accountID.ToString());
                break;
        }

        return hyperlink;
    }

    private string GetSearchIcon(string type)
    {
        var result = SearchResultType.Unknown;
        Enum.TryParse(type, out result);
        var iconPath = "";
        string imgFormat = "/images/icons/{0}.png";
        switch (result)
        {
            case SearchResultType.Account:
                iconPath = string.Format(imgFormat, "building");
                break;
            // GDS Redirect might be required.
            case SearchResultType.Device:
                iconPath = string.Format(imgFormat, "information");
                break;
            case SearchResultType.Group:
                iconPath = string.Format(imgFormat, "group");
                break;
            // GDS Redirect might be required.
            case SearchResultType.Location:
                iconPath = string.Format(imgFormat, "map");
                break;
            // GDS Redirect might be required.
            case SearchResultType.User:
                iconPath = string.Format(imgFormat, "user");
                break;
            // GDS Redirect might be required.
            case SearchResultType.PONumber:
                iconPath = string.Format(imgFormat, "note");
                break;
            case SearchResultType.Unknown:
                iconPath = string.Format(imgFormat, "delete");
                break;
        }

        return iconPath;
    }

    private int FindUserID(string username)
    {
        try
        {
            string connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            string cmdStr = "sp_if_FindUserNoByName";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.Add("@UserName", SqlDbType.NVarChar, 50);

            sqlCmd.Parameters["@UserName"].Value = username;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);

            sqlConn.Close();

            string strUserNo = dt.Rows[0]["UserNo"].ToString();

            return Convert.ToInt32(strUserNo);
        }
        catch { return 0; }
    }

    private enum SearchResultType
    {
        Account,
        Location,
        Group,
        User,
        Device,
        PONumber,
        Unknown
    };

    // 02/23/2015 by Anuradha Nandi - Added the "Type" attribute to display so "Active" flag is more clear to CS User.
    [Serializable()]
    private struct SearchResults
    {
        public int ID { get; set; }
        public int AccountID { get; set; }
        public string Hyperlink { get; set; }
        public string IconPath { get; set; }
        public bool Active { get; set; }
        public string ActiveText { get; set; }
        public string Name { get; set; }
        public string BrandName { get; set; }
        public string Type { get; set; }
        public SearchResultType ResultType { get; set; }
    }
}
