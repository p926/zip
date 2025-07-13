using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using CrystalDecisions.CrystalReports.Engine;
using Instadose;
using Instadose.Data;
using Instadose.Device;
using Instadose.Integration;
using Telerik.Web.UI;
using Mirion.DSD.Utilities;
using System.Web;
using System.Text;
using CrystalDecisions.Shared;
using System.Net;
using Instadose.API.DA;
using Mirion.DSD.GDS.API;
using Mirion.DSD.GDS.API.Requests;
using Mirion.DSD.Utilities.Constants;

public partial class InformationFinder_Details_Account : System.Web.UI.Page
{
    InsDataContext idc = new InsDataContext();
    AppDataContext adc = new AppDataContext();
    MASDataContext mdc = new MASDataContext();

    AXInvoiceRequests axInvoiceRequests = new AXInvoiceRequests();

    ReportDocument cryRpt = new ReportDocument();
    public const string ROOTFOLDERPATH = @"C:\inetpub\sites\portal.instadose.com\InformationFinder\Details\";
    int accountID = 0;
    private int orderID = 0;
    string UserName = "Unknown";

    private int iConsignmentID = 47;

    public static string currCode;

    bool isTransitionGDSAccount = false;
    private Boolean MalvernIntegration;
    private Boolean OrbitalIntegration;
    private Account account = null;
    private Location location = null;
    private AccountCreditCard accountcreditcard = null;

    /// <summary>
    /// Limits access to the functionality based on groups.
    /// </summary>
    string[] activeDirecoryGroups = { "IRV-IT", "IRV-CustomerSvc", "IRV-Client Services" };

    private const String STR_FEDEX_URL = "http://fedex.com/Tracking?ascend_header=1&clienttype=dotcom&cntry_code=us&language=english&tracknumbers=";
    private decimal lostReplacementUnitPrice = 25;
    bool DevelopmentServer = false;


    // total count for different products
    private int total = 0;
    private int id2total = 0;
    private int id2elitetotal = 0;
    private int idplustotal = 0;
    private int idlinktotal = 0;
    private int idusbtotal = 0;

    protected void Page_Unload(object sender, EventArgs e)
    {
        try
        {
            cryRpt.Close();
            cryRpt.Dispose();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        catch { }
    }

    # region ON PAGE LOAD FUNCTION
    protected void Page_Load(object sender, EventArgs e)
    {
        rgAssociatedAccount.ItemDataBound += new GridItemEventHandler(rgAssociatedAccount_ItemDataBound);
        //rgAssociatedAccount.DeleteCommand += new GridCommandEventHandler(rgAssociatedAccount_DeleteCommand);
        //rgAssociatedAccount.ItemCommand += new GridCommandEventHandler(rgAssociatedAccount_ItemCommand);


        // OnItemDataBound = "rgAssociatedAccount_OnItemDataBound"

        if (!int.TryParse(Request.QueryString["ID"], out accountID))
        {
            Page.Response.Redirect("../Default.aspx");
            return;
        }

        // Check if running in development or staging site
        if (Request.Url.Authority.ToString().Contains(".mirioncorp.com") || Request.Url.Authority.ToString().Contains("localhost"))
            DevelopmentServer = true;

        try
        {
            UserName = Page.User.Identity.Name.Split('\\')[1];
        }
        catch { UserName = "Unknown"; }

        hfUsername.Value = UserName;

        account = (from a in idc.Accounts where a.AccountID == accountID select a).FirstOrDefault();

        if (this.ddlLocation.Items.Count > 0)
        {
            location = (from l in idc.Locations where l.LocationID == int.Parse(this.ddlLocation.SelectedValue) select l).FirstOrDefault();
        }

        if (account == null) return;
        else
        {
            isTransitionGDSAccount = (account.GDSAccount != null) ? true : false;

            // If legacy Unix account, then disable links on Orders tab.
            if (isTransitionGDSAccount)
            {
                lnkbtnCreateOrder.Enabled = false;
                lnkbtnLostDamagedOrders.Enabled = false;
                lnkbtnEmailOrderAck.Enabled = false;
                lnkbtnCreateCreditMemo.Enabled = false;
                hlAddCalendar.Enabled = false;
            }


            if (account.GDSAccount != null)
            {
                // Get the current website environment
                string url = System.Configuration.ConfigurationManager.AppSettings["cswebsuite_webaddress"];

                url += String.Format("/Finder/AccountDetails.aspx?Account={0}", account.GDSAccount);

                Response.Redirect(url);
            }

            //8/2013 WK - If inactive account disable Location, order (new/addon/lost damaged), and user creation.
            if (!account.Active || account.RestrictOnlineAccess == true)
            {
                lnkbtnCreateUser.Enabled = false;
                lnkbtnCreateOrder.Enabled = false;
                lnkbtnCreateLocation.Enabled = false;
                lnkbtnLostDamagedOrders.Enabled = false;
            }

        }

        //Save Currency Code (due to Foreign Orders).
        currCode = account.CurrencyCode;

        MalvernIntegration = Convert.ToBoolean((from a in idc.AppSettings where a.AppKey == "MalvernIntegration" select a.Value).FirstOrDefault());
        OrbitalIntegration = Convert.ToBoolean((from a in idc.AppSettings where a.AppKey == "OrbitalIntegration" select a.Value).FirstOrDefault());

        Label lblAccountType = fvAccountGeneralInformation.FindControl("lblAccountType") as Label;
        Label lblCustomerGroup = fvAccountGeneralInformation.FindControl("lblCustomerGroup") as Label;
        Label lblIsICCareCommEligible = fvAccountGeneralInformation.FindControl("lblIsICCareCommEligible") as Label;

        if (lblAccountType != null)
        {
            var accountType = account.AccountTypeID == null ? null : Mirion.DSD.GDS.API.AccountRequests.GetMRNAccountType((int)account.AccountTypeID);
            lblAccountType.Text = accountType != null ? accountType.AccountTypeName : "";
        }

        if (lblCustomerGroup != null)
        {
            var customerGroup = account.CustomerGroupID == null ? null : Mirion.DSD.GDS.API.AccountRequests.GetMRNCustomerGroup((int)account.CustomerGroupID);
            lblCustomerGroup.Text = customerGroup != null ? customerGroup.CustomerGroupName : "";
        }

        if (lblIsICCareCommEligible != null)
        {
            lblIsICCareCommEligible.Text = account.isICCareCommEligible == true ? "Yes" : "No";
        }

        if (Page.IsPostBack) return;

        //GetRecordCounts(accountID);
        int prevAccount = 0;
        if (Session["AccountID"] != null)
            prevAccount = (int)(Session["AccountID"]);
        if (prevAccount == 0)
            Session["AccountID"] = account.AccountID;
        if (prevAccount > 0 && account.AccountID != prevAccount)
        {
            Session["MostRecentTabSelected"] = "Notes";
            Session["AccountID"] = account.AccountID;
        }
        // Get the image
        Image img = fvAccountGeneralInformation.FindControl("imgBrandLogo") as Image;

        if (img != null)
        {
            img.ImageUrl = "~/images/logos/";
            // Display Brand Source Logo (Quantum Products, Mirion, or ICCare)
            if (account.BrandSourceID.HasValue)
            {
                switch (account.BrandSourceID.Value)
                {
                    case 1:
                        img.ImageUrl += "quantum.png";
                        break;
                    case 2:
                        img.ImageUrl += "mirion.png";
                        break;
                    case 3:
                        img.ImageUrl += "iccare.png";
                        break;
                    default:
                        img.ImageUrl += "mirion.png";
                        break;
                }
            }
            else
            {
                img.ImageUrl += "mirion.png";
            }
        }

        // This will get the Session variable of the clicked RadTab from RadTabStrip1_TabClick
        // and maintain the last RadTab the User was on (after navigating back to this page).
        string radTabValue = (string)(Session["MostRecentTabSelected"]);
        RadTab rt = new RadTab();
        if (Session["MostRecentTabSelected"] == null)
        {
            rt = RadTabStrip1.FindTabByValue("Notes");
        }
        else
        {
            rt = RadTabStrip1.FindTabByValue(radTabValue);
        }

        rt.SelectParents();
        rt.PageView.Selected = true;

        if (account != null && account.CustomerType.CustomerTypeID == iConsignmentID)
        {  // consignment account
            lnkbtnCreateLocation.Visible = false;
            rtCSRequests.Visible = false;
            lnkbtnLostDamagedOrders.Visible = false;
            lnkbtnIDPlusLostReplacementOrder.Visible = false;
            lnkbtnIDPlusRecallOrder.Visible = false;
            lnkbtnCreateCreditMemo.Visible = false;
            lnkbtnEmailOrderAck.Visible = false;
        }
        else
        {
            rtAssociatedAccounts.Visible = false;   // only for consignment account
        }

        rtBillingGroups.Visible = account.UseLocationBilling;

        // show calendar tab, if account has Instadose Plus devices
        //rtCalendars.Visible = HasInstadosePlusDevices(accountID);

        // Rebind all RadGrids.
        //this.rgNotes.Rebind();
        //this.rgCSRequests.Rebind();
        //this.rgOrders.Rebind();
        //this.rgInvoices.Rebind();
        //this.rgLocations.Rebind();
        //this.rgUsers.Rebind();
        //this.rgBadges.Rebind();
        //this.rgReturns.Rebind();
        //this.rgReads.Rebind();
        //this.rgDocuments.Rebind();

        // TAKEN OUT SINCE INITIAL SORT ORDER IS DETERMINED BY STOREPROCEDURES.
        // Modified By: Anuradha Nandi
        // Modified On: 10/06/2014
        // Initial Sort Order/Expressions for each RadGrid.
        //InitialSortOrder(rgNotes, "AccountID", "");
        //InitialSortOrder(rgCSRequests, "AccountID", "");
        //InitialSortOrder(rgOrders, "OrderID", "");
        //InitialSortOrder(rgInvoices, "InvoiceDate", "PayDate");
        //InitialSortOrder(rgLocations, "AccountID", "");
        //InitialSortOrder(rgUsers, "AccountID", "");
        //InitialSortOrder(rgBadges, "AccountID", "");
        //InitialSortOrder(rgReturns, "", "");
        //InitialSortOrder(rgReads, "", "");
        //InitialSortOrder(rgDocuments, "", "");

        // Show AR Indicator
        // AR Indicator should reflect only AX Invoices
        DateTime? earliestInvoiceDateWithBalance = GetEarliestAXAccountARDateWithBalance(); //GetEarliestARDateWithBalance();
        Literal ltARIndicator = fvAccountGeneralInformation.FindControl("ltARIndicator") as Literal;
        HyperLink hlARIndicator = fvAccountGeneralInformation.FindControl("hlARIndicator") as HyperLink;

        if (earliestInvoiceDateWithBalance != null)
        {
            ltARIndicator.Visible = true;
            hlARIndicator.Visible = true;

            hlARIndicator.CssClass = ARIndicatorButtonCSSClass(earliestInvoiceDateWithBalance);
            hlARIndicator.NavigateUrl = "~/Finance/AXNewProcessCreditCard.aspx?Account=" + accountID + "&System=Instadose";
        }
        else
        {
            ltARIndicator.Visible = false;
            hlARIndicator.Visible = false;
        }
        txtNote.Attributes.Add("maxlength", txtNote.MaxLength.ToString());
    }
    # endregion

    protected void RadTabStrip1_TabClick(object sender, RadTabStripEventArgs e)
    {
        Session["MostRecentTabSelected"] = e.Tab.Value;
    }

    /// <summary>
    /// Formats Boolean Values from 1 and 0 (RadGrid Checkbox Checked/Unchecked) to Yes and No respectively.
    /// </summary>
    /// <param name="boolean"></param>
    /// <returns></returns>
    public string YesNo(object boolean)
    {
        try
        {
            return Convert.ToBoolean(boolean) ? "Yes" : "No";
        }
        catch { return "No"; }
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

    # region NOTES TAB FUNCTIONS
    protected void lnkbtnCreateNote_Click(object sender, EventArgs e)
    {
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('createNoteDialog')", true);
        txtNote.Focus();
    }

    protected void rgNotes_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
    {
        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.StoredProcedure;

        // Using the vw_CreditCardExpirationsListing.
        string sqlQuery = "sp_if_GetAccountNotesByAccountID";

        sqlCmd.Parameters.Add(new SqlParameter("@pAccountID", accountID));

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable dtNotesTable = new DataTable();

        dtNotesTable = new DataTable("Get All Account-Linked Notes");

        // Create the columns for the DataTable.
        dtNotesTable.Columns.Add("AccountID", Type.GetType("System.Int32"));
        dtNotesTable.Columns.Add("CreatedDate", Type.GetType("System.DateTime"));
        dtNotesTable.Columns.Add("CreatedBy", Type.GetType("System.String"));
        dtNotesTable.Columns.Add("NoteText", Type.GetType("System.String"));
        dtNotesTable.Columns.Add("Active", Type.GetType("System.Boolean"));

        while (sqlDataReader.Read())
        {
            DataRow drow = dtNotesTable.NewRow();

            // Fill row details.
            drow["AccountID"] = sqlDataReader["AccountID"];
            drow["CreatedDate"] = sqlDataReader["CreatedDate"];
            drow["CreatedBy"] = sqlDataReader["CreatedBy"];
            drow["NoteText"] = sqlDataReader["NoteText"];
            drow["Active"] = sqlDataReader["Active"];

            // Add rows to DataTable.
            dtNotesTable.Rows.Add(drow);
        }

        // Bind the results to the gridview
        rgNotes.DataSource = dtNotesTable;

        sqlConn.Close();
        sqlDataReader.Close();
    }

    protected void rgNotes_OnItemCommand(object sender, Telerik.Web.UI.GridCommandEventArgs e)
    {
        rgNotes.Rebind();
    }

    protected void rgNotes_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
    {
        SortOrder(rgNotes, e);
    }


    protected void btnAddNote_Click(object sender, EventArgs e)
    {
        // Modal/Dialog Error Messages.
        InvisibleModalErrors_Notes();

        if (accountID == 0) return;

        string IdentityName = HttpContext.Current.Request.LogonUserIdentity.Name;
        string[] arrUserName = IdentityName.Split('\\');

        UserName = arrUserName[1];

        string noteText = txtNote.Text.Trim();

        if (txtNote.Text.Trim() != "")
        {
            AccountNote an = new AccountNote
            {
                AccountID = accountID,
                CreatedDate = DateTime.Now,
                CreatedBy = UserName,
                NoteText = noteText,
                Active = true
            };

            // Add the new object to the AccountNotes collection.
            idc.AccountNotes.InsertOnSubmit(an);

            try
            {
                idc.SubmitChanges();

                // Close Create Note Modal/Dialog.
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('createNoteDialog')", true);
                rgNotes.DataSource = null;
                rgNotes.Rebind();

                // Update Note Tab count.
                //GetRecordCounts(accountID);

                // Clear/Reset ASP.NET control.
                txtNote.Text = string.Empty;
            }
            catch (Exception ex)
            {
                // Close Create Note Modal/Dialog.
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('createNoteDialog')", true);

                // Clear/Reset ASP.NET control.
                txtNote.Text = string.Empty;

                return;
            }
        }
        else
        {
            // Display Error Message and Reset controls.
            string errorMessage = "Notes field cannot be blank.";
            VisibleModalErrors_Notes(errorMessage);
            txtNote.Focus();
            return;
        }
    }

    // Reset Error Message (Create Note Modal/Dialog).
    private void InvisibleModalErrors_Notes()
    {
        this.spnModalError_Notes.InnerText = "";
        this.divModalError_Notes.Visible = false;
    }

    // Set Error Message (Create Note Modal/Dialog).
    private void VisibleModalErrors_Notes(string error)
    {
        this.spnModalError_Notes.InnerText = error;
        this.divModalError_Notes.Visible = true;
    }

    protected void btnCancelNote_Click(object sender, EventArgs e)
    {
        // Modal/Dialog Error Messages.
        InvisibleModalErrors_Notes();

        // Clear/Reset ASP.NET control.
        txtNote.Text = string.Empty;
    }

    protected void lnkbtnClearFilters_Notes_Click(object sender, EventArgs e)
    {
        foreach (GridColumn column in rgNotes.MasterTableView.OwnerGrid.Columns)
        {
            column.CurrentFilterFunction = GridKnownFunction.NoFilter;
            column.CurrentFilterValue = string.Empty;
        }
        rgNotes.MasterTableView.FilterExpression = string.Empty;
        rgNotes.Rebind();
    }

    #endregion

    #region create consignment account tab code



    protected void rgAssociatedAccount_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
    {
        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.StoredProcedure;

        // Using the vw_CreditCardExpirationsListing.
        string sqlQuery = "sp_GetAssociatedAccountinfoByAccountID";

        sqlCmd.Parameters.Add(new SqlParameter("@pAccountID", accountID));

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable dtTable = new DataTable();

        dtTable = new DataTable("Get All Associated Account-Linked");

        // Create the columns for the DataTable.
        dtTable.Columns.Add("accountid", Type.GetType("System.Int32"));
        dtTable.Columns.Add("accountname", Type.GetType("System.String"));
        dtTable.Columns.Add("id1count", Type.GetType("System.Int32"));
        dtTable.Columns.Add("id2elitecount", Type.GetType("System.Int32"));
        dtTable.Columns.Add("id2count", Type.GetType("System.Int32"));
        dtTable.Columns.Add("idpluscount", Type.GetType("System.Int32"));

        dtTable.Columns.Add("idlinkcount", Type.GetType("System.Int32"));
        dtTable.Columns.Add("idusbcount", Type.GetType("System.Int32"));


        while (sqlDataReader.Read())
        {
            DataRow drow = dtTable.NewRow();

            // Fill row details.
            drow["accountid"] = sqlDataReader["accountid"];
            drow["accountname"] = sqlDataReader["accountname"];
            drow["id1count"] = sqlDataReader["id1count"];
            drow["id2elitecount"] = sqlDataReader["id2elitecount"];
            drow["id2count"] = sqlDataReader["id2count"];
            drow["idpluscount"] = sqlDataReader["idpluscount"];

            drow["idlinkcount"] = sqlDataReader["idlinkcount"];
            drow["idusbcount"] = sqlDataReader["idusbcount"];


            // Add rows to DataTable.
            dtTable.Rows.Add(drow);
        }

        //     var vassociatedAccount = idc.sp_GetAssociatedAccountInfoByAccountID(account.AccountID);

        // Bind the results to the gridview
        rgAssociatedAccount.DataSource = dtTable;


        //rgAssociatedAccount.DataBind();

        sqlCmd.Dispose();
        sqlConn.Close();
        sqlDataReader.Close();
        sqlConn.Dispose();

    }




    protected void rgAssociatedAccount_OnItemCommand(object sender, Telerik.Web.UI.GridCommandEventArgs e)
    {
        rgAssociatedAccount.Rebind();
    }

    protected void rgAssociatedAccount_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
    {
        SortOrder(rgAssociatedAccount, e);
    }

    protected void rgAssociatedAccount_DeleteCommand(object sender, Telerik.Web.UI.GridCommandEventArgs e)
    {

        try
        {
            GridDataItem item = e.Item as GridDataItem;

            // using columnuniquename
            string str1 = item["accountid"].Text;

            // using DataKey
            string str2 = item.GetDataKeyValue("ID").ToString();
        }
        catch (Exception ee)
        {
            string s = ee.Message;
        }
    }

    protected void rgAssociatedAccount_ItemCommand(object sender, GridCommandEventArgs e)
    {

        try
        {
            if (e.CommandName == "Delete")
            {
                GridDataItem item = e.Item as GridDataItem;

                // using columnuniquename
                string str1 = item["accountid"].Text;

                // using DataKey
                string str2 = item.GetDataKeyValue("ID").ToString();
            }
        }
        catch (Exception ee)
        {
            string s = ee.Message;
        }
    }

    protected void rgAssociatedAccount_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem dataItem = (GridDataItem)e.Item;
            int fieldValue = int.Parse(dataItem["id1count"].Text.ToString());
            total = total + fieldValue;
            fieldValue = int.Parse(dataItem["id2elitecount"].Text.ToString());
            id2elitetotal = id2elitetotal + fieldValue;
            fieldValue = int.Parse(dataItem["id2count"].Text.ToString());
            id2total = id2total + fieldValue;
            fieldValue = int.Parse(dataItem["idpluscount"].Text.ToString());
            idplustotal = idplustotal + fieldValue;
            fieldValue = int.Parse(dataItem["idlinkcount"].Text.ToString());
            idlinktotal = idlinktotal + fieldValue;
            fieldValue = int.Parse(dataItem["idusbcount"].Text.ToString());
            idusbtotal = idusbtotal + fieldValue;

        }
        if (e.Item is GridFooterItem)
        {
            GridFooterItem footerItem = (GridFooterItem)e.Item;
            footerItem["accountid"].Text = "Total:";
            footerItem["id1count"].Text = total.ToString();
            footerItem["id2elitecount"].Text = id2elitetotal.ToString();
            footerItem["id2count"].Text = id2total.ToString();
            footerItem["idpluscount"].Text = idplustotal.ToString();
            footerItem["idlinkcount"].Text = idlinktotal.ToString();
            footerItem["idusbcount"].Text = idusbtotal.ToString();
        }

        if (e.Item.ItemType == GridItemType.Item || e.Item.ItemType == GridItemType.AlternatingItem)
        {
            // Find the rows information. Here I assume that you
            // have created a class called DataSet which is filled
            // with the relevant information (number, comment and ID)
            // from the database.
            DataRowView currentRow = (DataRowView)e.Item.DataItem;

            // Find the delete button
            RadButton btnDel = (RadButton)e.Item.FindControl("btnDelete");
            if (btnDel != null)
            {
                // Now set the command argument on the delete button.
                // This is what we use to identify which entry we want to delete.
                btnDel.CommandArgument = currentRow["accountid"].ToString();
            }
        }



    }  // end procedure

    protected void lnkbtnCreateAssociatedAccount_Click(object sender, EventArgs e)
    {
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('createAssociatedAccountDialog')", true);
        txtAccountID.Focus();
    }

    protected void lnkbtnRemoveAssociatedAccount_Click(object sender, EventArgs e)
    {
        string sremoveaccountID = hselectedID.Value.ToString();

        total = 0;
        id2total = 0;
        id2elitetotal = 0;
        idplustotal = 0;
        idlinktotal = 0;
        idusbtotal = 0;

        InvisibleModalErrors_AssociatedAccount();

        if (accountID == 0) return;

        // parent account id
        if (sremoveaccountID != "")
        {
            // Add the new object to the AccountNotes collection.
            //idc.AccountNotes.InsertOnSubmit(an);

            try
            {

                int iremoveAccountID = 0;
                int.TryParse(sremoveaccountID, out iremoveAccountID);

                if (iremoveAccountID > 0)
                {  // search by account id
                    account = (from a in idc.Accounts
                               where a.AccountID == iremoveAccountID
                               && a.WarehouseID == accountID
                               select a).FirstOrDefault();

                    if (account == null)
                    {
                        string errorMessage = "Error1: no Account association is found!";
                        VisibleModalErrors_AssociatedAccount(errorMessage);
                        this.txtAccountID.Focus();
                        return;
                    }
                }
                else
                {
                    string errorMessage = "Error2: no account association found!";
                    VisibleModalErrors_AssociatedAccount(errorMessage);
                    this.txtAccountID.Focus();
                    return;
                }

                //account = (from a in idc.Accounts
                //           where a.AccountID == iremoveAccountID && a.WarehouseID != accountID
                //           select a).FirstOrDefault();

                if (account != null)
                {



                    //account = (from a in idc.Accounts
                    //           where a.AccountID == int.Parse(sremoveaccountID)
                    //           select a).FirstOrDefault();

                    // assign parent id to the user entered accout id
                    account.WarehouseID = null;


                    idc.SubmitChanges();

                    rgAssociatedAccount.DataSource = null;
                    rgAssociatedAccount.Rebind();


                }  // end if

            }
            catch (Exception ex)
            {
                // Close Create Note Modal/Dialog.
                //ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('createAssociatedAccountDialog')", true);

                // Clear/Reset ASP.NET control.
                //txtAccountID.Text = string.Empty;

                string errorMessage = "Error3: no account association found!";
                VisibleModalErrors_AssociatedAccount(errorMessage);
                this.txtAccountID.Focus();
                return;
            }
        }
        else
        {
            // Display Error Message and Reset controls.
            string errorMessage = "Error4: no account association found!";
            VisibleModalErrors_AssociatedAccount(errorMessage);
            this.txtAccountID.Focus();
            return;
        }

    }


    protected void btnSearchAssociatedAccount_Click(object sender, EventArgs e)
    {

        InvisibleModalErrors_AssociatedAccount();

        string saddAccountID = this.txtAccountID.Text.Trim();

        gvSearchAccount.DataSource = null;
        gvSearchAccount.DataBind();



        try
        {

            int iaddAccountID = 0;
            int.TryParse(saddAccountID, out iaddAccountID);

            if (iaddAccountID == 0 && saddAccountID.Length > 0)
            {
                // Display Error Message and Reset controls.
                string errorMessage = "Error: invalid account ID entered!";
                VisibleModalErrors_AssociatedAccount(errorMessage);
                this.txtAccountID.Focus();
                return;
            }

            if (saddAccountID == "" && txtAccountName.Text.Trim() == "")
            {
                // Display Error Message and Reset controls.
                string errorMessage = "Error: please enter search parameter!";
                VisibleModalErrors_AssociatedAccount(errorMessage);
                this.txtAccountID.Focus();
                return;

            }


            // Create a Date Table to hold the contents/results.
            DataTable dtTable = new DataTable();

            dtTable = new DataTable("Get All Associated Account info");

            // Create the columns for the DataTable.
            dtTable.Columns.Add("AccountID", Type.GetType("System.Int32"));
            dtTable.Columns.Add("AccountName", Type.GetType("System.String"));
            dtTable.Columns.Add("Status", Type.GetType("System.String"));
            dtTable.Columns.Add("AssignNo", Type.GetType("System.String"));

            DataRow drow = dtTable.NewRow();

            if (iaddAccountID > 0 && txtAccountName.Text == "")
            {  // search by account id
                var result = (from a in idc.Accounts
                              where a.AccountID == iaddAccountID

                              select new
                              {
                                  AccountID = a.AccountID,
                                  AccountName = a.AccountName,
                                  Status = a.Active == true ? "Active" : "Not Active",
                                  AssignNo = a.WarehouseID,
                                  BrandSourceId = a.BrandSourceID,
                                  CustomerTypeID = a.CustomerTypeID,
                                  GDSAccount = a.GDSAccount

                              }).FirstOrDefault();

                if (result == null)
                {
                    // Display Error Message and Reset controls.
                    string errorMessage = "Error: no account found!";
                    VisibleModalErrors_AssociatedAccount(errorMessage);
                    this.txtAccountID.Focus();
                    return;
                }


                if (result.GDSAccount != null)
                {
                    // Display Error Message and 
                    string errorMessage = "GDSAccount has value and not able to retrieve the account info!";
                    VisibleModalErrors_AssociatedAccount(errorMessage);
                    this.txtAccountID.Focus();
                    return;
                }

                if (result.BrandSourceId == 3)  // check for iccare
                {
                    // Display Error Message and Reset controls.
                    string errorMessage = "Error: cannot add IC Care account!";
                    VisibleModalErrors_AssociatedAccount(errorMessage);
                    this.txtAccountID.Focus();
                    return;
                }

                if (result.CustomerTypeID == iConsignmentID)  // consignment type
                {
                    // Display Error Message and Reset controls.
                    string errorMessage = "Error: cannot add account with customer type = Consignment!";
                    VisibleModalErrors_AssociatedAccount(errorMessage);
                    this.txtAccountID.Focus();
                    return;
                }


                // Fill row details.
                drow["AccountID"] = result.AccountID;
                drow["AccountName"] = result.AccountName;
                drow["Status"] = result.Status;
                drow["AssignNo"] = (result.AssignNo == null) ? "" : result.AssignNo.ToString();

            }  // end if
            else
            {
                var result = (from a in idc.Accounts
                              where a.AccountName.Contains(txtAccountName.Text.ToString())
                              select new
                              {
                                  AccountID = a.AccountID,
                                  AccountName = a.AccountName,
                                  Status = a.Active == true ? "Active" : "Not Active",
                                  AssignNo = a.WarehouseID,
                                  BrandSourceId = a.BrandSourceID,
                                  CustomerTypeID = a.CustomerTypeID,
                                  GDSAccount = a.GDSAccount

                              }).FirstOrDefault();

                if (result == null)
                {
                    // Display Error Message and Reset controls.
                    string errorMessage = "Error: no account was found!";
                    VisibleModalErrors_AssociatedAccount(errorMessage);
                    this.txtAccountID.Focus();
                    return;

                }

                if (result.GDSAccount != null)
                {
                    // Display Error Message and 
                    string errorMessage = "GDSAccount has value and not able to retrieve the account info!";
                    VisibleModalErrors_AssociatedAccount(errorMessage);
                    this.txtAccountID.Focus();
                    return;
                }

                if (result.BrandSourceId == 3)  // check for iccare
                {
                    // Display Error Message and Reset controls.
                    string errorMessage = "Error: cannot add IC Care account!";
                    VisibleModalErrors_AssociatedAccount(errorMessage);
                    this.txtAccountID.Focus();
                    return;
                }


                if (result.CustomerTypeID == iConsignmentID)  // consignment type
                {
                    // Display Error Message and Reset controls.
                    string errorMessage = "Error: cannot add Account with customer type = consignment!";
                    VisibleModalErrors_AssociatedAccount(errorMessage);
                    this.txtAccountID.Focus();
                    return;
                }

                // Fill row details.
                drow["AccountID"] = result.AccountID;
                drow["AccountName"] = result.AccountName;
                drow["Status"] = result.Status;
                drow["AssignNo"] = (result.AssignNo == null) ? "" : result.AssignNo.ToString();
            }  // end esel


            // Add rows to DataTable.
            dtTable.Rows.Add(drow);

            gvSearchAccount.DataSource = dtTable;
            gvSearchAccount.DataBind();
            return;

        }
        catch (Exception ex)
        {
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('createAssociatedAccountDialog')", true);
            txtAccountID.Text = string.Empty;
            return;
        }

    }  // end procedure


    protected void btnAddAssociatedAccount_Click(object sender, EventArgs e)
    {

        InvisibleModalErrors_AssociatedAccount();

        if (accountID == 0) return;

        //UserName = Page.User.Identity.Name.Split('\\')[1];
        UserName = this.UserName.ToString();

        string saddAccountID = this.txtAccountID.Text.Trim();
        string saddAccountName = "";

        if (saddAccountID != "" || txtAccountName.Text.Trim() != "")
        {
            // Add the new object to the AccountNotes collection.
            //idc.AccountNotes.InsertOnSubmit(an);

            try
            {
                // DataTable dt = (DataTable) gvSearchAccount.DataSource;

                if (gvSearchAccount.Rows.Count == 0)
                {
                    // Display Error Message and Reset controls.
                    string errorMessage = "Error no account selected!";
                    VisibleModalErrors_AssociatedAccount(errorMessage);
                    this.txtAccountID.Focus();
                    return;
                }

                saddAccountID = gvSearchAccount.Rows[0].Cells[0].Text.ToString();

                int iaddAccountID = 0;
                int.TryParse(saddAccountID, out iaddAccountID);

                if (iaddAccountID == 0)
                {
                    // Display Error Message and Reset controls.
                    string errorMessage = "Error: account no is invalid!";
                    VisibleModalErrors_AssociatedAccount(errorMessage);
                    this.txtAccountID.Focus();
                    return;

                }

                account = (from a in idc.Accounts
                           where a.AccountID == int.Parse(saddAccountID)
                           select a).FirstOrDefault();

                // assign parent id to the user entered accout id
                account.WarehouseID = accountID;

                idc.SubmitChanges();

                // Close Create Modal/Dialog.
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('createAssociatedAccountDialog')", true);


                txtAccountName.Text = string.Empty;
                txtAccountID.Text = string.Empty;

                rgAssociatedAccount.DataSource = null;
                rgAssociatedAccount.Rebind();


                gvSearchAccount.DataSource = null;
                gvSearchAccount.DataBind();

            }
            catch (Exception ex)
            {
                // Close Create Note Modal/Dialog.
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('createAssociatedAccountDialog')", true);

                // Clear/Reset ASP.NET control.
                txtAccountID.Text = string.Empty;

                return;
            }
        }
        else
        {
            // Display Error Message and Reset controls.
            string errorMessage = "Error: Input field cannot be blank!";
            VisibleModalErrors_AssociatedAccount(errorMessage);
            this.txtAccountID.Focus();
            return;
        }

    }

    protected void btnCancelAssociatedAccount_Click(object sender, EventArgs e)
    {
        // Modal/Dialog Error Messages.
        InvisibleModalErrors_AssociatedAccount();

        // Clear/Reset ASP.NET control.
        txtAccountName.Text = string.Empty;
        txtAccountID.Text = string.Empty;

        DataRow drow = null;

        DataTable dtTable = new DataTable("Get All Associated Account info");

        // Create the columns for the DataTable.
        dtTable.Columns.Add("AccountID", Type.GetType("System.Int32"));
        dtTable.Columns.Add("AccountName", Type.GetType("System.String"));
        dtTable.Columns.Add("Status", Type.GetType("System.String"));
        dtTable.Columns.Add("AssignNo", Type.GetType("System.String"));

        drow = dtTable.NewRow();
        dtTable.Rows.Add(drow);


        //rgAssociatedAccount.DataSource = null;
        //rgAssociatedAccount.DataBind();

        gvSearchAccount.DataSource = null;
        gvSearchAccount.DataBind();
    }

    protected void lnkbtnClearFilters_AssociatedAccount_Click(object sender, EventArgs e)
    {
        //foreach (GridColumn column in rgNotes.MasterTableView.OwnerGrid.Columns)
        //{
        //    column.CurrentFilterFunction = GridKnownFunction.NoFilter;
        //    column.CurrentFilterValue = string.Empty;
        //}
        //rgNotes.MasterTableView.FilterExpression = string.Empty;
        //rgNotes.Rebind();
    }


    // Reset Error Message (Create AssociatedAccount Modal/Dialog).
    private void InvisibleModalErrors_AssociatedAccount()
    {
        this.spnModalError_AssociatedAccount.InnerText = "";
        this.divModalError_AssociatedAccount.Visible = false;
    }

    // Set Error Message (Create AssociatedAccount Modal/Dialog).
    private void VisibleModalErrors_AssociatedAccount(string error)
    {
        this.spnModalError_AssociatedAccount.InnerText = error;
        this.divModalError_AssociatedAccount.Visible = true;
    }

    # endregion

    #region CS REQUESTS TAB FUNCTIONS
    protected void lnkbtnCreateCSRequest_Click(object sender, EventArgs e)
    {
        string url = String.Format("/CustomerService/CreateRequestCase.aspx?AccountID={0}&OpenFrom=Account", accountID);
        Response.Redirect(url);
    }

    protected void rgCSRequests_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
    {
        Int32.TryParse(Request.QueryString["ID"], out accountID);

        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.StoredProcedure;

        // Using the vw_CreditCardExpirationsListing.
        string sqlQuery = "sp_if_GetAccountCSRequestsByAccountID";

        sqlCmd.Parameters.Add(new SqlParameter("@pAccountID", accountID));

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable dtCSRequestsTable = new DataTable();

        dtCSRequestsTable = new DataTable("Get All Account-Linked C/S Requests");

        // Create the columns for the DataTable.
        dtCSRequestsTable.Columns.Add("AccountID", Type.GetType("System.Int32"));
        dtCSRequestsTable.Columns.Add("CaseID", Type.GetType("System.Int32"));
        dtCSRequestsTable.Columns.Add("RequestDate", Type.GetType("System.DateTime"));
        dtCSRequestsTable.Columns.Add("RequestedBy", Type.GetType("System.String"));
        dtCSRequestsTable.Columns.Add("CaseStatusDesc", Type.GetType("System.String"));
        dtCSRequestsTable.Columns.Add("RequestDesc", Type.GetType("System.String"));
        dtCSRequestsTable.Columns.Add("CaseNote", Type.GetType("System.String"));

        while (sqlDataReader.Read())
        {
            DataRow drow = dtCSRequestsTable.NewRow();

            // Fill row details.
            drow["AccountID"] = sqlDataReader["AccountID"];
            drow["CaseID"] = sqlDataReader["CaseID"];
            drow["RequestDate"] = sqlDataReader["RequestDate"];
            drow["RequestedBy"] = sqlDataReader["RequestedBy"];
            drow["CaseStatusDesc"] = sqlDataReader["CaseStatusDesc"];
            drow["RequestDesc"] = sqlDataReader["RequestDesc"];
            drow["CaseNote"] = sqlDataReader["CaseNote"];

            // Add rows to DataTable.
            dtCSRequestsTable.Rows.Add(drow);
        }

        // Bind the results to the gridview
        rgCSRequests.DataSource = dtCSRequestsTable;

        sqlConn.Close();
        sqlDataReader.Close();
    }

    protected void rgCSRequests_OnItemCommand(object sender, Telerik.Web.UI.GridCommandEventArgs e)
    {
        rgCSRequests.Rebind();
    }

    protected void rgCSRequests_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
    {
        SortOrder(rgCSRequests, e);
    }

    protected void lnkbtnClearFilters_CSRequests_Click(object sender, EventArgs e)
    {
        foreach (GridColumn column in rgCSRequests.MasterTableView.OwnerGrid.Columns)
        {
            column.CurrentFilterFunction = GridKnownFunction.NoFilter;
            column.CurrentFilterValue = string.Empty;
        }
        rgCSRequests.MasterTableView.FilterExpression = string.Empty;
        rgCSRequests.Rebind();
    }
    # endregion

    # region ORDERS TAB FUNCTIONS
    protected void lnkbtnCreateOrder_Click(object sender, EventArgs e)
    {
        string url = String.Format("/CustomerService/CreateOrder.aspx?ID={0}", accountID);
        Response.Redirect(url);
    }

    protected void lnkbtnLostDamagedOrders_Click(object sender, EventArgs e)
    {
        string url = String.Format("/CustomerService/CreateOrder.aspx?ID={0}&OrderType=LOSTDAMAGED", accountID);
        Response.Redirect(url);
    }

    protected void lnkbtnEmailOrderAck_Click(object sender, EventArgs e)
    {
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('emailOrderAcknowledgementDialog')", true);
    }

    protected void lnkbtnCreateCreditMemo_Click(object sender, EventArgs e)
    {
        string url = String.Format("/Finance/CreditMemo.aspx?ID={0}", accountID);
        Response.Redirect(url);
    }

    public bool DisplayCreditMemoButton()
    {
        bool isDisplay = false;

        string[] activeDirecoryGroups = { "IRV-IT", "IRV-Accounting-RW" };
        bool belongsToGroups = ActiveDirectoryQueries.UserExistsInAnyGroup(User.Identity.Name, activeDirecoryGroups);
        // IF the User exists in the Required Group, THEN make the property/Control visible.
        if (belongsToGroups)
            isDisplay = true;

        return isDisplay;
    }

    protected void rgOrders_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
    {
        Int32.TryParse(Request.QueryString["ID"], out accountID);

        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.StoredProcedure;

        // Using the vw_CreditCardExpirationsListing.
        string sqlQuery = "sp_if_GetAccountOrdersByAccountID";

        sqlCmd.Parameters.Add(new SqlParameter("@pAccountID", accountID));

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable dtOrdersTable = new DataTable();

        dtOrdersTable = new DataTable("Get All Account-Linked Orders");

        // Create the columns for the DataTable.
        dtOrdersTable.Columns.Add("OrderID", Type.GetType("System.Int32"));
        dtOrdersTable.Columns.Add("OrderStatusID", Type.GetType("System.Int32"));
        dtOrdersTable.Columns.Add("OrderType", Type.GetType("System.String"));
        dtOrdersTable.Columns.Add("OrderDate", Type.GetType("System.DateTime"));
        dtOrdersTable.Columns.Add("CreatedBy", Type.GetType("System.String"));
        dtOrdersTable.Columns.Add("Status", Type.GetType("System.String"));
        dtOrdersTable.Columns.Add("PONumber", Type.GetType("System.String"));
        dtOrdersTable.Columns.Add("PaymentMethod", Type.GetType("System.String"));
        dtOrdersTable.Columns.Add("Quantity", Type.GetType("System.String"));
        dtOrdersTable.Columns.Add("OrderTotal", Type.GetType("System.Decimal"));
        dtOrdersTable.Columns.Add("CurrencyCode", Type.GetType("System.String"));
        dtOrdersTable.Columns.Add("USDOrderTotalWithCurrencyCode", Type.GetType("System.String"));
        dtOrdersTable.Columns.Add("DisplayReceipt", Type.GetType("System.Boolean"));

        decimal total = 0;
        int orderID = 0;
        string currencyCode = "";
        DateTime? orderDate = null;


        // Now check the AccountID's order history to see if they have had any orders that are not USD.
        // If so, display Foreign Currency and USD conversion amounts (along with additional column).
        int count = (from ord in idc.Orders
                     where ord.AccountID == accountID && ord.CurrencyCode != "USD"
                     select ord).Count();

        while (sqlDataReader.Read())
        {
            DataRow drow = dtOrdersTable.NewRow();

            if (Convert.ToDateTime(sqlDataReader["OrderDate"]) == null)
                orderDate = DateTime.Now;
            else
                orderDate = Convert.ToDateTime(sqlDataReader["OrderDate"]);

            drow["OrderID"] = sqlDataReader["OrderID"].ToString();
            drow["OrderStatusID"] = sqlDataReader["OrderStatusID"].ToString();
            drow["OrderType"] = sqlDataReader["OrderType"].ToString();
            drow["OrderDate"] = orderDate;
            drow["CreatedBy"] = sqlDataReader["CreatedBy"].ToString();
            drow["Status"] = sqlDataReader["Status"].ToString();
            drow["PONumber"] = sqlDataReader["PONumber"].ToString();
            drow["PaymentMethod"] = sqlDataReader["PaymentMethod"].ToString();
            drow["Quantity"] = sqlDataReader["Quantity"].ToString();
            drow["OrderTotal"] = sqlDataReader["OrderTotal"];
            drow["CurrencyCode"] = sqlDataReader["CurrencyCode"];
            total = Convert.ToDecimal(sqlDataReader["OrderTotal"]);
            currencyCode = sqlDataReader["CurrencyCode"].ToString();
            if (currencyCode != "USD")
                drow["USDOrderTotalWithCurrencyCode"] = string.Format("{0:0.00} USD", Currencies.ConvertToCurrency(total, currencyCode));
            else
                drow["USDOrderTotalWithCurrencyCode"] = string.Format("{0:0.00} USD", total);

            drow["DisplayReceipt"] = Convert.ToBoolean(sqlDataReader["DisplayReceipt"]);

            // Add rows to DataTable.
            dtOrdersTable.Rows.Add(drow);
        }

        // If the Account has had an Order in non-USD, display "USD Total" column only.
        if (count > 0)
        {
            (this.rgOrders.MasterTableView.GetColumn("USDOrderTotalWithCurrencyCode") as GridBoundColumn).Display = true;  // Hide column.
        }
        else
            (this.rgOrders.MasterTableView.GetColumn("USDOrderTotalWithCurrencyCode") as GridBoundColumn).Display = false;  // Show column.

        // Bind the results to the gridview
        rgOrders.DataSource = dtOrdersTable;

        sqlConn.Close();
        sqlDataReader.Close();
    }

    protected void rgOrders_OnItemCommand(object sender, Telerik.Web.UI.GridCommandEventArgs e)
    {
        rgOrders.Rebind();
    }

    protected void rgOrders_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
    {
        SortOrder(rgOrders, e);
    }

    protected void lnkbtnClearFilters_Orders_Click(object sender, EventArgs e)
    {
        foreach (GridColumn column in rgOrders.MasterTableView.OwnerGrid.Columns)
        {
            column.CurrentFilterFunction = GridKnownFunction.NoFilter;
            column.CurrentFilterValue = string.Empty;
        }
        rgOrders.MasterTableView.FilterExpression = string.Empty;
        rgOrders.Rebind();
    }


    // --------xxxxxxxxxxxx----------- ID+ LOST/REPLACEMENT CREATE ORDER ----------xxxxxxxxxxxxxxxx---------------//

    private void LoadControls_IDPlusLostReplacementRecallOrderDialog()
    {
        InvisibleErrors_IDPlusLostReplacementRecallOrderDialog();

        LoadLostReplaceDevicesSection();

        LoadPaymentMethodSection();

        LoadShippingOptionsSection();

        LoadProratePeriodRadioButton();

        // hide Recall ProRate Period radio button, if account is IC Care account
        //recallProRatePeriod.Visible = account.BrandSourceID.Value != 3;
    }

    private void LoadLostReplaceDevicesSection()
    {
        // Load Locations drop down
        this.ddlLocation.Items.Clear();
        ddlLocation.DataSource = (from a in idc.Locations where a.AccountID == this.accountID orderby a.LocationName select a);
        ddlLocation.DataBind();

        location = (from l in idc.Locations where l.LocationID == int.Parse(this.ddlLocation.SelectedValue) select l).FirstOrDefault();

        int[] recallReasonExceptionIDs = new int[] { 6, 10, 15, 16 };

        // Display DDL return reason
        this.ddlRecallReason.Items.Clear();
        //ddlRecallReason.DataSource = (from rs in adc.rma_ref_ReturnReasons where rs.Active == true && rs.DepartmentID == 3 orderby rs.Description select rs);
        ddlRecallReason.DataSource = (from rs in adc.rma_ref_ReturnReasons where rs.Active == true && rs.DepartmentID == 3 && (rs.IDPlus.HasValue && rs.IDPlus.Value) && (rs.UseByDepartmentID.HasValue && rs.UseByDepartmentID.Value == 2) orderby rs.Description select rs);
        ddlRecallReason.DataBind();

        // Reset Notes:
        this.txtRecallNote.Text = "";

        // Reset Search text
        this.txtDeviceSearch.Text = "";

        // Loading gridview
        Load_gv_DeviceList();
    }

    private void LoadPaymentMethodSection()
    {
        //rblstPayMethod.SelectedValue = account.BillingMethod;
        loadPaymentSection();

        //if (account.BillingMethod == "Purchase Order")
        //{
        //    rblstPayMethod.Items.FindByValue("Purchase Order").Enabled = true;
        //    rblstPayMethod.Items.FindByValue("Credit Card").Enabled = false;
        //}
        //else
        //{
        //    rblstPayMethod.Items.FindByValue("Purchase Order").Enabled = false;
        //    rblstPayMethod.Items.FindByValue("Credit Card").Enabled = true;
        //}
    }

    private void loadPaymentSection()
    {
        // Display the default information/controls for PO Number rdnbtn selection.
        this.txtPOno.Text = "REPLACEDIS";
        this.divPaymentPO.Visible = true;
        //this.divPaymentCC.Visible = false;
        //this.lblUpdateCCInformation.Visible = false;
        //this.lblUpdateCCInformation.Text = string.Empty;

        // NOT LONGER NEEDED, CHECKING FOR "Credit Card" SELECTION.
        // IF "Purchase Order" is selected, do not load any CC Payment Information.
        //if (this.rblstPayMethod.SelectedValue == "Purchase Order") return;

        //if (this.rblstPayMethod.SelectedValue == "Credit Card")
        //{
        //    // Load the credit card Year drop down.
        //    ddlCCYear.Items.Add(new ListItem("Year", "0"));
        //    for (int i = 0; i < 7; i++)
        //    {
        //        ddlCCYear.Items.Add(new ListItem((DateTime.Now.Year + i).ToString()));
        //    }

        //    // Get the AccountCreditCard record based on Account DataObject.
        //    accountcreditcard = GetAccountCreditCard(account);

        //    // Check to see if the Account has a valid CC on record.
        //    if (accountcreditcard != null)
        //    {
        //        // IF "Purchase Order" is not selected, show the CC Payment information.
        //        this.divPaymentPO.Visible = false;
        //        this.divPaymentCC.Visible = true;

        //        // Hide "Udpate Account CC Information" message.
        //        //this.lblUpdateCCInformation.Visible = false;
        //        //this.lblUpdateCCInformation.Text = string.Empty;

        //        switch (accountcreditcard.CreditCardTypeID)
        //        {
        //            case 1: //visa 324
        //                this.rbtCardType.SelectedIndex = 0;
        //                break;
        //            case 2: //MasterCard 325
        //                this.rbtCardType.SelectedIndex = 1;
        //                break;
        //            case 3: //Discover 330
        //                this.rbtCardType.SelectedIndex = 2;
        //                break;
        //            case 4: // AE 333
        //                this.rbtCardType.SelectedIndex = 3;
        //                break;
        //        }

        //        this.txtCCName.Text = accountcreditcard.NameOnCard;
        //        this.ddlCCMonth.SelectedIndex = accountcreditcard.ExpMonth;
        //        this.ddlCCYear.SelectedValue = accountcreditcard.ExpYear.ToString();
        //        this.txtCCcvc.Text = accountcreditcard.SecurityCode;
        //        this.txtCCno.Text = Common.MaskCreditCardNumber(accountcreditcard.NumberEncrypted, accountcreditcard.CreditCardType.CreditCardName);
        //    }
        //    else
        //    {
        //        this.divPaymentPO.Visible = false;
        //        this.divPaymentCC.Visible = false;
        //        // Display "Udpate Account CC Information" message.
        //        //this.lblUpdateCCInformation.Visible = true;
        //        //this.lblUpdateCCInformation.Text = "This Account either does not have a Credit Card on record or it has expired, please update the information.";
        //    }
        //}
    }

    private AccountCreditCard GetAccountCreditCard(Account acct)
    {
        // Load Account Credit Card information, check if CC is current/valid then Enable/Disable rblstPayMethod List Items accordingly.
        int currentMonth = 0;
        int currentYear = 0;
        currentMonth = Convert.ToInt32(DateTime.Now.Month);
        currentYear = Convert.ToInt32(DateTime.Now.Year);

        AccountCreditCard acctcc = (from acc in acct.AccountCreditCards
                                    where acc.Active == true
                                    && (acc.UseOnce == false || acc.UseOnce == (bool?)null)
                                    && ((acc.ExpYear > currentYear) || (acc.ExpYear == currentYear && acc.ExpMonth >= currentMonth))
                                    orderby acc.AccountCreditCardID descending
                                    select acc).FirstOrDefault();

        return acctcc;
    }

    private void LoadShippingOptionsSection()
    {
        // Reset
        this.txtSpecialInstruction.Text = "";

        // Load package types.
        this.ddlPackageType.Items.Clear();
        ddlPackageType.DataSource = (from c in idc.PackageTypes orderby c.PackageDesc select c);
        ddlPackageType.DataBind();

        // Load the shipping option drop down.
        this.ddlShippingOption.Items.Clear();
        ddlShippingOption.DataSource = (from c in idc.ShippingOptions orderby c.ShippingOptionDesc select c);
        ddlShippingOption.DataBind();

        if (MalvernIntegration)
        {
            // enable/disable controls
            divMalvernCarrier.Visible = true;
            divMalvernShipMethod.Visible = true;
            divFedex.Visible = false;

            //Shipping International or Domestic
            int international = 0;
            // if (ddlShippingCountry.SelectedItem.Text != "United States")
            if (location.ShippingCountry.CountryName != "United States")
            {
                international = 1;
            }

            this.ddShippingCarrier.Items.Clear();

            string defaultShippingCarrier = this.account.ShippingCarrier; //(from a in idc.Accounts where a.AccountID == this.accountID select a.ShippingCarrier).FirstOrDefault();            
            if (!string.IsNullOrEmpty(defaultShippingCarrier))
            {
                ddShippingCarrier.Items.Insert(0, new ListItem(defaultShippingCarrier, "0"));
                ddShippingCarrier.SelectedIndex = 0;
            }
            else
            {
                ddShippingCarrier.Items.Insert(0, new ListItem("N/A", "0"));
                ddShippingCarrier.SelectedIndex = 0;
            }

            this.ddlShippingMethodMalvern.Items.Clear();

            if (defaultShippingCarrier != null && defaultShippingCarrier.Length > 0)
            {

                var sMethods = from r in idc.ShippingMethods
                               where r.CarrierDesc == defaultShippingCarrier && r.InternationalShipping == international
                               orderby r.ShippingMethodDesc ascending
                               select new
                               {
                                   ShippingMethodID = r.ShippingMethodID,
                                   ShippingMethodDesc = r.ShippingMethodDesc + " " + r.ShippingMethodDetails
                               };

                ddlShippingMethodMalvern.DataSource = sMethods;
                ddlShippingMethodMalvern.DataTextField = "ShippingMethodDesc";
                ddlShippingMethodMalvern.DataValueField = "ShippingMethodID";
                ddlShippingMethodMalvern.DataBind();
            }
            else
            {
                var sMethods = from r in idc.ShippingMethods
                               where r.PrefShipping == 0 && r.InternationalShipping == international
                               orderby r.ShippingMethodDesc ascending
                               select new
                               {
                                   ShippingMethodID = r.ShippingMethodID,
                                   ShippingMethodDesc = r.ShippingMethodDesc + " " + r.ShippingMethodDetails

                               };

                ddlShippingMethodMalvern.DataSource = sMethods;
                ddlShippingMethodMalvern.DataTextField = "ShippingMethodDesc";
                ddlShippingMethodMalvern.DataValueField = "ShippingMethodID";
                ddlShippingMethodMalvern.DataBind();
            }

            ddlShippingMethodMalvern.DataBind();
        }
        else // Load the shipping method drop down for Fedex.
        {
            // enable/disable controls
            divMalvernCarrier.Visible = false;
            divMalvernShipMethod.Visible = false;
            divFedex.Visible = true;

            this.ddlShippingMethod.Items.Clear();

            ddlShippingMethod.DataSource = (from a in idc.ShippingMethods
                                            where a.PrefShipping == null && a.ShippingMethodDesc != "Renewal"
                                            orderby a.ShippingMethodDesc
                                            select a);
            ddlShippingMethod.DataBind();
        }
    }

    private void Load_gv_DeviceList()
    {
        //var searchResult = idc.sp_GetDevicesByLostReplacementOrderSearch(this.accountID, int.Parse(ddlLocation.SelectedValue), this.txtDeviceSearch.Text).ToList();
        var searchResult = idc.sp_GetDevicesByLostReplacementOrderSearchByProductGroupID(this.accountID, int.Parse(ddlLocation.SelectedValue), int.Parse(rblProductGroup.SelectedValue), this.txtDeviceSearch.Text).ToList();

        gv_DeviceList.DataSource = searchResult;
        gv_DeviceList.DataBind();
    }

    private void ReLoad_gv_DeviceList(string pReplacedSN, string pUserID)
    {
        foreach (GridViewRow row in this.gv_DeviceList.Rows)
        {
            string sn = row.Cells[4].Text.Replace("\n", "").ToString().Trim();
            if (sn == pReplacedSN.Trim())
            {
                ImageButton img = (ImageButton)row.FindControl("imgAssignedWearerForDevice");
                HiddenField hidAssignedUserID = (HiddenField)row.FindControl("HidAssignedUserID");
                Label fullName = (Label)row.FindControl("lblFullName");

                img.ImageUrl = "~/images/icons/user_edit.png";
                hidAssignedUserID.Value = pUserID;
                fullName.Text = (from u in idc.Users where u.UserID == int.Parse(pUserID) select u.FirstName + " " + u.LastName).FirstOrDefault();
            }
        }
    }

    protected void gv_DeviceList_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            if (e.Row.RowIndex == 0)
            {
                e.Row.Style.Add("height", "60px");
                e.Row.VerticalAlign = VerticalAlign.Bottom;
            }

            ImageButton img = (ImageButton)e.Row.FindControl("imgAssignedWearerForDevice");
            HiddenField hidAssignedUserID = (HiddenField)e.Row.FindControl("HidAssignedUserID");

            if (string.IsNullOrEmpty(hidAssignedUserID.Value)) img.ImageUrl = "~/images/icons/user_add.png";
            else img.ImageUrl = "~/images/icons/user_edit.png";

            // Load BodyRegion drop down list
            System.Web.UI.WebControls.Label l = (System.Web.UI.WebControls.Label)e.Row.FindControl("lblBodyRegionID");
            DropDownList ddl = (DropDownList)e.Row.FindControl("ddlBodyRegion");

            ddl.DataSource = (from a in idc.BodyRegions select a);
            ddl.DataBind();
            ddl.SelectedValue = l.Text;
        }
    }

    //protected void rblstPayMethod_SelectedIndexChanged(object sender, EventArgs e)
    //{
    //    loadPaymentSection();
    //}

    protected void ddlLocation_OnSelectedIndexChange(object sender, EventArgs e)
    {
        location = (from l in idc.Locations where l.LocationID == int.Parse(this.ddlLocation.SelectedValue) select l).FirstOrDefault();

        LoadShippingOptionsSection();

        // Reset Notes:
        this.txtRecallNote.Text = "";
        // Reset Search text
        this.txtDeviceSearch.Text = "";
        // Loading gridview
        Load_gv_DeviceList();
    }

    protected void rblLostRMAReason_OnSelectedIndexChange(object sender, EventArgs e)
    {
        if (rblLostRMAReason.SelectedValue == "L")
            this.txtPOno.Text = "REPLACEDIS";
        else
            this.txtPOno.Text = "BROKEN CLIP";
    }

    protected void rblProductGroup_OnSelectedIndexChange(object sender, EventArgs e)
    {
        Load_gv_DeviceList();
    }

    protected void txtDeviceSearch_TextChanged(object sender, EventArgs e)
    {
        // Loading gridview
        Load_gv_DeviceList();
    }

    protected void lnkbtnIDPlusLostReplacementOrder_Click(object sender, EventArgs e)
    {
        // Do loading, set default
        this.HidRMAType.Value = "LostReplace";
        this.RMADevices.InnerText = "Lost/Replacement Devices";
        LoadControls_IDPlusLostReplacementRecallOrderDialog();

        this.lostRMAType.Visible = true;

        this.recallReason.Visible = false;
        this.recallNote.Visible = false;

        this.PaymentSection.Visible = true;
        this.ShippingSection.Visible = true;

        // Send JavaScript to client to close/display the modal.
        string dialogTitle = "ID+ Lost/Replacement Order";
        string script = "openIDPlusLostReplacementRecallOrderDialog('" + dialogTitle + "')";
        ScriptManager.RegisterClientScriptBlock(Page, typeof(Page), "openModal", script, true);
    }

    //protected void lnkbtnIDPlusRecallOrder_Click(object sender, EventArgs e)
    //{
    //    // Do loading, set default
    //    this.HidRMAType.Value = "Recall";
    //    this.RMADevices.InnerText = "Recall Devices";
    //    LoadControls_IDPlusLostReplacementRecallOrderDialog();

    //    this.lostRMAType.Visible = false;

    //    this.recallReason.Visible = true;
    //    this.recallNote.Visible = true;

    //    this.PaymentSection.Visible = false;
    //    this.ShippingSection.Visible = false;

    //    // Send JavaScript to client to close/display the modal.
    //    string dialogTitle = "ID+ Recall Order";
    //    string script = "openIDPlusLostReplacementRecallOrderDialog('" + dialogTitle + "')";
    //    ScriptManager.RegisterClientScriptBlock(Page, typeof(Page), "openModal", script, true);
    //}
    protected void lnkbtnIDPlusRecallOrder_Click(object sender, EventArgs e)
    {
        string url = String.Format("../../TechOps/ReturnAddNewDeviceRMA.aspx?SerialNo={0}&ApplicationID={1}&AccountID={2}&Location={3}", 0, 2, accountID,"");
        Response.Redirect(url);
    }

    protected void btnCreateLostReplacementRecallOrder_Click(object sender, EventArgs e)
    {
        try
        {
            InvisibleErrors_IDPlusLostReplacementRecallOrderDialog();

            if (passInputsValidation_IDPlusLostReplacementRecallOrderDialog())
            {
                int rmaReturnID = 0;
                int orderID = 0;

                if (this.HidRMAType.Value == "Recall")
                {
                    ProcessOrder(ref orderID);
                    rmaReturnID = ProcessRMA(this.HidRMAType.Value);

                    if (orderID > 0 && rmaReturnID > 0)
                    {
                        // ******** update order with SpecialInstructions ************//                        
                        Order order = idc.Orders.Where(o => o.OrderID == orderID && o.OrderStatusID != 10).FirstOrDefault();
                        order.SpecialInstructions = TrimNewLineString("Recall Replacement, Recall# " + rmaReturnID.ToString() + ", Ship Overnight / 2nd Day. " + this.txtSpecialInstruction.Text.Trim());
                        idc.SubmitChanges();

                        // ******** update RMA with OrderID ************//
                        rma_Return RMA = (from r in adc.rma_Returns where r.ReturnID == rmaReturnID select r).FirstOrDefault();
                        RMA.OrderID = orderID;

                        // ******** Insert RMAheader record *********//
                        rma_RMAHeader RHead = null;
                        RHead = new rma_RMAHeader()
                        {
                            ReturnID = rmaReturnID,
                            RMANumber = orderID.ToString().PadLeft(7, '0'),
                            Reason = "Recall Order# " + orderID.ToString() + "  generated and submitted to MAS.",
                            Active = true,
                            CreatedBy = this.UserName,
                            CreatedDate = DateTime.Now
                        };
                        adc.rma_RMAHeaders.InsertOnSubmit(RHead);
                        adc.SubmitChanges();

                        // insert TransLog, update devices status
                        var writeTransLogUpdDev = adc.sp_rma_process(rmaReturnID, 0, 0, " ", this.UserName, DateTime.Now, "RECALL ORDER",
                                "Update OrderID: " + orderID.ToString() + "Insert RMAHeaderID: " + RHead.RMAHeaderID, 2);

                        // Insert data to Transaction Log with new ReturnDevicesID
                        var writeTransLogReturn2 = adc.sp_rma_process(rmaReturnID, 0, 0, " ", this.UserName, DateTime.Now, "RECALL ORDER",
                                "Order have been sent to Biz & MAS. Order# " + orderID.ToString(), 2);
                    }
                }
                else
                {
                    ProcessOrder(ref orderID);
                    rmaReturnID = ProcessRMA(this.HidRMAType.Value);

                    if (orderID > 0 && rmaReturnID > 0)
                    {
                        // ******** update RMA with OrderID ************//
                        rma_Return RMA = (from r in adc.rma_Returns where r.ReturnID == rmaReturnID select r).FirstOrDefault();
                        RMA.OrderID = orderID;
                        adc.SubmitChanges();
                    }
                }

                // Send JavaScript to client to close/display the modal.
                string script = "closeDialog('IDPlusLostReplacementRecallOrderDialog')";
                ScriptManager.RegisterClientScriptBlock(Page, typeof(Page), "closeModal", script, true);

                // reload page
                Response.Redirect(Request.RawUrl);
            }
        }
        catch (Exception ex)
        {
            this.VisibleErrors_IDPlusLostReplacementRecallOrderDialog(ex.Message);
        }
    }

    private void RecallSendEmail(string pSn, int pReturnID)
    {
        // Send email to TechOps if user's email =null
        string TechOpsEmailAddress = "khindra@mirion.com";
        //Message Subject
        string MsgTitle = "Instadose Recall Notification";
        //Wearer name
        string WearerFirstLastName = "";

        // Get active UserDevice detail 
        var userDeviceList = (from UD in idc.UserDevices
                              join di in idc.DeviceInventories on UD.DeviceID equals di.DeviceID
                              where di.SerialNo == pSn
                              && UD.Active == true
                              select UD).ToList();

        // ************* Send email to assigned Wearer and Account Admin *********************//
        if (userDeviceList.Count > 0)
        {
            foreach (UserDevice ud in userDeviceList)
            {
                //grap User email and name
                User Uinfo = (from ui in idc.Users where ui.UserID == ud.UserID select ui).FirstOrDefault();

                string WearerEmail = "";

                if (Uinfo != null)
                {
                    // send email to assigned user
                    try
                    {
                        WearerFirstLastName = Uinfo.FirstName + " " + Uinfo.LastName;
                        WearerEmail = (Uinfo.Email != null) ? Uinfo.Email.ToString() : TechOpsEmailAddress;       // send to Tech-ops manager (= Kevin Hindra) if user does not have email address                                         

                        //Send To addressList 
                        List<string> ToAddList = new List<string>();
                        ToAddList.Add(WearerEmail);

                        //Build the template fileds.
                        Dictionary<string, string> WearerFields = new Dictionary<string, string>();
                        WearerFields.Add("EmailTitle", MsgTitle);
                        WearerFields.Add("WearerName", WearerFirstLastName);
                        WearerFields.Add("SerialNo", pSn);

                        //*** send email to WearerEmail -- email class 
                        bool sendWearerEmail = SendRecallMessage("Wearer", ToAddList, WearerFields);

                        // insert TransLog, update devices status
                        var writeTransLogEmailWearer = adc.sp_rma_process(pReturnID, 0, 0, " ", this.UserName, DateTime.Now, "RECALL ORDER", "Email Wearer: " + sendWearerEmail.ToString(), 2);
                    }
                    catch
                    {
                        var writeTransLogEmailAdmin = adc.sp_rma_process(pReturnID, 0, 0, " ", this.UserName, DateTime.Now, "RECALL ORDER", "Email Wearer: ERROR ", 2);
                        throw new Exception("RECALL ORDER with returnID: " + pReturnID + ", for SN: " + pSn + ", Email Wearer: ERROR");
                    }

                    // send email to account admin
                    try
                    {
                        var adminUsers = (from a in idc.Accounts
                                          join u in idc.Users on a.AccountAdminID equals u.UserID
                                          where a.AccountID == Uinfo.AccountID
                                          select u).ToList();

                        if (adminUsers.Count > 0)
                        {
                            foreach (User u in adminUsers)
                            {
                                string AdminFirstLastName = u.FirstName + " " + u.LastName;
                                string AdminEmail = (u.Email != null) ? u.Email.ToString() : TechOpsEmailAddress;      // send to Tech-ops manager (= Kevin Hindra) if Admin account does not have email address

                                if (WearerEmail != AdminEmail)
                                {
                                    //Send To addressList 
                                    List<string> AdminToAddList = new List<string>();
                                    AdminToAddList.Add(AdminEmail);

                                    //Build the template fileds.
                                    Dictionary<string, string> AdminFields = new Dictionary<string, string>();
                                    AdminFields.Add("EmailTitle", MsgTitle);
                                    AdminFields.Add("WearerName", WearerFirstLastName);
                                    AdminFields.Add("SerialNo", pSn);
                                    AdminFields.Add("AdministratorName", AdminFirstLastName);

                                    //*** send email to Administrator -- email class 
                                    bool sendAdminEmail = SendRecallMessage("Admin", AdminToAddList, AdminFields);

                                    // insert TransLog, update devices status
                                    var writeTransLogEmailAdmin = adc.sp_rma_process(pReturnID, 0, 0, " ", this.UserName, DateTime.Now, "RECALL ORDER", "Email Admin: " + sendAdminEmail.ToString(), 2);
                                }
                            }
                        }
                    }
                    catch
                    {
                        var writeTransLogEmailAdmin = adc.sp_rma_process(pReturnID, 0, 0, " ", this.UserName, DateTime.Now, "RECALL ORDER", "Email Admin: ERROR ", 2);
                        throw new Exception("RECALL ORDER with returnID: " + pReturnID + ", for SN: " + pSn + ", Email Admin: ERROR");
                    }
                }
            }
        }
        else // ************* SN is not assigned to anyone, Send email to Account Admin only *********************//
        {
            // send email to account admin
            try
            {
                var adminUsers = (from a in idc.Accounts
                                  join u in idc.Users on a.AccountAdminID equals u.UserID
                                  where a.AccountID == this.account.AccountID
                                  select u).ToList();

                if (adminUsers.Count > 0)
                {
                    foreach (User u in adminUsers)
                    {
                        string AdminFirstLastName = u.FirstName + " " + u.LastName;
                        string AdminEmail = (u.Email != null) ? u.Email.ToString() : TechOpsEmailAddress;      // send to Tech-ops manager( = Kevin Hindra) if Admin account does not have email address

                        //Send To addressList 
                        List<string> AdminToAddList = new List<string>();
                        AdminToAddList.Add(AdminEmail);

                        //Build the template fileds.
                        Dictionary<string, string> AdminFields = new Dictionary<string, string>();
                        AdminFields.Add("EmailTitle", MsgTitle);
                        AdminFields.Add("WearerName", WearerFirstLastName);
                        AdminFields.Add("SerialNo", pSn);
                        AdminFields.Add("AdministratorName", AdminFirstLastName);

                        //*** send email to Administrator -- email class 
                        bool sendAdminEmail = SendRecallMessage("Admin", AdminToAddList, AdminFields);
                        // insert TransLog, update devices status
                        var writeTransLogEmailAdmin = adc.sp_rma_process(pReturnID, 0, 0, " ", this.UserName, DateTime.Now, "RECALL ORDER", "Email Admin: " + sendAdminEmail.ToString(), 2);
                    }
                }
            }
            catch
            {
                var writeTransLogEmailAdmin = adc.sp_rma_process(pReturnID, 0, 0, " ", this.UserName, DateTime.Now, "RECALL ORDER", "Email Admin: ERROR ", 2);
                throw new Exception("RECALL ORDER with returnID: " + pReturnID + ", for SN: " + pSn + ", Email Admin: ERROR");
            }
        }

    }

    /// <summary>
    /// Send email with template converted string
    /// </summary>
    /// <param name="HtmlString"></param>
    /// <param name="TextString"></param>
    /// <param name="MessageSubject"></param>
    /// <param name="ToaddressList"></param>
    /// <param name="AllFieldsDict"></param>
    /// <returns></returns>
    protected bool SendRecallMessage(string EmailFormatType,
        List<string> ToaddressList, Dictionary<string, string> AllFieldsDict)
    {

        bool myreturn = false;

        MessageSystem SendMessage = new MessageSystem();
        SendMessage.Application = "RMA_Recall";
        SendMessage.CreatedBy = UserName;
        SendMessage.FromAddress = "noreply@instadose.com";

        //To Address List
        if (DevelopmentServer == false)
        {
            SendMessage.ToAddressList = ToaddressList;
        }
        else
        {
            SendMessage.ToAddressList = new List<string>();
            SendMessage.ToAddressList.Add("tdo@mirion.com");
            SendMessage.ToAddressList.Add(this.UserName + "@mirion.com");
        }

        SendMessage.CcAddressList = new List<string>();

        // Build the template fields and Send the Mesasge
        try
        {
            int MySendID;

            if (EmailFormatType == "Admin")
                MySendID = SendMessage.Send("Admin Return Request", "", AllFieldsDict);
            else
                MySendID = SendMessage.Send("Wearer Return Request", "", AllFieldsDict);

            myreturn = true;
        }
        catch
        {
            myreturn = false;
        }

        return myreturn;
    }

    private int ProcessRMA(string pRMAType)
    {
        int NewReturnID = CreateReturn(pRMAType);

        CreateReturnDevices(NewReturnID, pRMAType);

        return NewReturnID;
    }

    private int CreateReturn(string pRMAType)
    {
        try
        {
            int returnTypeID = 0, returnReasonID = 0;
            string reason = "";
            string rmaNote = "";

            if (pRMAType == "Recall")
            {
                returnTypeID = 3;
                reason = this.ddlRecallReason.SelectedItem.Text;
                returnReasonID = int.Parse(this.ddlRecallReason.SelectedValue);
                rmaNote = txtRecallNote.Text.Trim();
            }
            else
            {
                if (rblLostRMAReason.SelectedValue == "L")
                {
                    returnTypeID = 9;   // 'Lost (No Return)' from rma_ref_ReturnTypes table in LCDISBUS db
                    reason = "Lost (No Return)";
                    returnReasonID = 16;    // 'Lost (No Return)' from rma_ref_ReturnReason table in LCDISBUS db
                }
                else
                {
                    returnTypeID = 8;
                    reason = "Broken";
                    returnReasonID = 10;    // 'Broken' from rma_ref_ReturnReason table in LCDISBUS db
                }

                //rma_ref_ReturnType returnType = (from rs in adc.rma_ref_ReturnTypes
                //                                 where rs.Active == true && rs.DepartmentID == 2 && rs.Type == "Lost (No Return)"
                //                                 select rs).FirstOrDefault();

                //rma_ref_ReturnReason returnReason = (from rs in adc.rma_ref_ReturnReasons
                //                                     where rs.Active == true && rs.DepartmentID == 2 && rs.Description == "Lost (No Return)"
                //                                     select rs).FirstOrDefault();
                //returnTypeID = returnType.ReturnTypeID;
                //reason = returnReason.Description;
                //returnReasonID = returnReason.ReasonID;

                rmaNote = "Generated by Lost-Replacement order.";
            }

            rma_Return rma = null;

            rma = new rma_Return()
            {
                AccountID = this.accountID,
                Active = true,
                Notes = rmaNote,
                Reason = reason,
                Return_ReasonID = returnReasonID,
                ReturnTypeID = returnTypeID,
                Status = 1,
                CreatedBy = this.UserName,
                CreatedDate = DateTime.Now

            };
            adc.rma_Returns.InsertOnSubmit(rma);
            adc.SubmitChanges();

            // Insert data to Transaction Log with new ReturnID
            var writeTransLogReturn = adc.sp_rma_process(rma.ReturnID, 0, 0, rmaNote,
                    this.UserName, DateTime.Now, "ADD RETURN", "New return ID: " + rma.ReturnID.ToString(), 2);

            // return Notes to Header table
            rma_RMAHeader header = null;
            header = new rma_RMAHeader()
            {
                RMANumber = "0",
                ReturnID = rma.ReturnID,
                Reason = rmaNote,
                Active = true,
                CreatedBy = this.UserName,
                CreatedDate = DateTime.Now
            };
            adc.rma_RMAHeaders.InsertOnSubmit(header);
            adc.SubmitChanges();

            // Insert data to Transaction Log with new ReturnID 
            var writeTransLogReturnHeader = adc.sp_rma_process(rma.ReturnID, 0, 0, rmaNote,
                    this.UserName, DateTime.Now, "ADD RETURN HEADER", "Add retrunID: " + rma.ReturnID.ToString(), 2);

            // Return the ID that was generated.
            return rma.ReturnID;
        }
        catch (Exception ex)
        {
            throw ex;
        }

    }

    private void CreateReturnDevices(int pReturnID, string pRMAType)
    {
        try
        {
            string rmaNote = "";
            int rmaDepartment = 0;

            if (pRMAType == "Recall")
            {
                rmaNote = txtRecallNote.Text.Trim();
                rmaDepartment = 3;
            }
            else
            {
                rmaNote = "Generated by Lost-Replacement order.";
                rmaDepartment = 2;
            }

            AppDataContext adc = new AppDataContext();

            rma_ReturnDevice retrunDevices = null;

            foreach (GridViewRow row in this.gv_DeviceList.Rows)
            {
                CheckBox chkLostDevice = (CheckBox)row.FindControl("chkbxSelectDeviceList");
                CheckBox chkLostReplacementDevice = (CheckBox)row.FindControl("chkbxSelectReplaceDeviceList");
                HiddenField hidDeviceID = (HiddenField)row.FindControl("HidDeviceID");
                HiddenField hidAssignedUserID = (HiddenField)row.FindControl("HidAssignedUserID");

                string color = row.Cells[2].Text.Replace("\n", "").ToString().Trim();
                string sn = row.Cells[4].Text.Replace("\n", "").ToString().Trim();

                if (chkLostDevice.Checked)
                {
                    List<rma_ReturnDevice> returnDevice = (from rd in adc.rma_ReturnDevices
                                                           where rd.SerialNo == sn && rd.Active == true
                                                           select rd).ToList();

                    // Deactivate all previous returndevice of serialno.
                    foreach (rma_ReturnDevice dbActive in returnDevice)
                    {
                        dbActive.Active = false;
                    }

                    // Insert new returndevice
                    retrunDevices = new rma_ReturnDevice();

                    retrunDevices.ReturnID = pReturnID;
                    retrunDevices.SerialNo = sn;
                    retrunDevices.MasterDeviceID = int.Parse(hidDeviceID.Value);
                    retrunDevices.Notes = rmaNote;
                    retrunDevices.DepartmentID = rmaDepartment;
                    retrunDevices.Status = 1;
                    retrunDevices.Active = true;

                    // Grab device UserID 
                    var MyUserID = (from a in idc.UserDevices
                                    where a.DeviceID == int.Parse(hidDeviceID.Value)
                                    && a.Active == true
                                    select a.UserID).FirstOrDefault();
                    if (MyUserID != null && MyUserID > 0)
                        retrunDevices.UserID = MyUserID;


                    adc.rma_ReturnDevices.InsertOnSubmit(retrunDevices);
                    adc.SubmitChanges();

                    // add transLog with new RetrunDevicesID
                    var writeTransLogReturn = adc.sp_rma_process(pReturnID, retrunDevices.ReturnDevicesID, 0, rmaNote,
                       this.UserName, DateTime.Now, "ADD RETURN DEVICE", "New ReturnDevice ID: " + retrunDevices.ReturnDevicesID.ToString(), 2);

                    // if it is a lost then update device status = "Lost Not Returned"
                    if (rblLostRMAReason.SelectedValue == "L")
                    {
                        DeviceInventory deviceInventory = (from di in idc.DeviceInventories where di.SerialNo == sn select di).FirstOrDefault();

                        DeviceAnalysisStatus lostStatus = (from s in idc.DeviceAnalysisStatus where s.DeviceAnalysisName == "Lost-NotReturned" select s).FirstOrDefault();
                        if (lostStatus != null)
                        {
                            deviceInventory.DeviceAnalysisStatus = lostStatus;
                            idc.SubmitChanges();
                        }
                        
                        //Update device audit
                        DeviceManager myDeviceManager = new DeviceManager();
                        myDeviceManager.InsertDeviceInventoryAudit(deviceInventory.DeviceID, deviceInventory.DeviceAnalysisStatus.DeviceAnalysisStatusID, false, "RMA Lost Order Creation", "Account Page");
                    }

                    // if recall then sending out email to assigned users and admin
                    // at this phase, no sending out recall email for ID+. Recall ID+ email' s content need to change by return reason accordingly.
                    // if (pRMAType == "Recall") RecallSendEmail(sn, pReturnID);

                    DeactivateDevice(int.Parse(hidDeviceID.Value), sn, pReturnID);
                }
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    private void DeactivateDevice(int pDeviceID, string pSN, int pReturnID)
    {
        // ----------- Deactivate a device off an user and account. -----------------//              
        try
        {
            // ************* Deactivate UserDevice ********************** //
            UserDevice UDev = (from a in idc.UserDevices
                               join ad in idc.AccountDevices on a.DeviceID equals ad.DeviceID
                               where a.DeviceID == pDeviceID
                               && a.Active == true
                               && ad.CurrentDeviceAssign == true
                               select a).FirstOrDefault();
            if (UDev != null)
            {
                UDev.Active = false;
                UDev.DeactivateDate = DateTime.Now;
                idc.SubmitChanges();
            }
            // ************* Deactivate UserDevice ********************** //

            // ********** Deactivate AccountDevice ********************//
            IQueryable<AccountDevice> ADev = (from a in idc.AccountDevices
                                              where a.DeviceID == pDeviceID && a.Active == true
                                              select a).AsQueryable();
            foreach (AccountDevice ad in ADev)
            {
                ad.Active = false;
                ad.DeactivateDate = DateTime.Now;
                idc.SubmitChanges();
            }
            // ********** Deactivate AccountDevice ********************//

            // insert TransLog
            adc.sp_rma_process(pReturnID, 0, 0, " ", this.UserName, DateTime.Now, "DEACTIVATE", "ReturnID: " + pReturnID.ToString() + ", Deactive Serial# " + pSN, 2);
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    private bool getOrderTotals(out decimal subtotal, out decimal? shippingCost,
        out decimal couponDiscount, out decimal grandTotal, out int totalReplacement)
    {
        subtotal = 0.0M;
        couponDiscount = 0.0M;
        grandTotal = 0.0M;
        shippingCost = 0.0M;

        totalReplacement = 0;

        try
        {
            int totalUnits = 0;

            foreach (GridViewRow row in this.gv_DeviceList.Rows)
            {
                CheckBox chkLostDevice = (CheckBox)row.FindControl("chkbxSelectDeviceList");
                CheckBox chkLostReplacementDevice = (CheckBox)row.FindControl("chkbxSelectReplaceDeviceList");
                string sn = row.Cells[4].Text.Replace("\n", "").ToString().Trim();

                if (chkLostDevice.Checked) totalUnits++;

                if (chkLostReplacementDevice.Checked) totalReplacement++;

            }

            subtotal = (totalUnits * lostReplacementUnitPrice);

            if (totalReplacement == 0)
            {
                shippingCost = 0.00M;
            }
            else
            {
                // Calculate the shipping total.
                string shippingState = this.location.ShippingState.StateAbbrev;
                string shippingCountry = this.location.ShippingCountry.PayPalCode;
                string shippingPostalCode = this.location.ShippingPostalCode;

                int shippingMethod;
                if (MalvernIntegration)
                {
                    shippingMethod = int.Parse(ddlShippingMethodMalvern.SelectedItem.Value);
                    if (ddlShippingMethodMalvern.SelectedItem.Text.ToLower().Contains("free") || ddlShippingMethodMalvern.SelectedItem.Text.ToLower().Contains("renewal"))
                    {
                        shippingMethod = 0;
                    }
                }
                else
                {
                    shippingMethod = int.Parse(ddlShippingMethod.SelectedItem.Value);
                }

                if (MalvernIntegration)
                {
                    if (shippingMethod != 0)
                    {
                        //Based on the Preferred Shiping need to call either Malvern API or Database for Flat Rates                         
                        if (string.IsNullOrEmpty(this.account.ShippingCarrier))
                        {
                            //Call database for Flat Rates
                            shippingCost = OrderHelpers.GetShippingFlatRates(shippingState, shippingCountry, shippingPostalCode, ddlShippingMethodMalvern.SelectedItem.Value, totalReplacement);
                        }
                        else
                        {
                            //Get Carrier Code and Shipping Method code from Database to submit for Malavern API           
                            ShippingMethod method = (from sm in idc.ShippingMethods where sm.ShippingMethodID == shippingMethod select sm).FirstOrDefault();
                            //Calculate weight
                            decimal weight = 0.2M * totalReplacement;

                            //Get 3 Character Country Code based on the Country ID
                            // Tdo, 9/8/2014. Using numericcountrycode instead of countrycode
                            //string sCountryCode = (from c in idc.Countries where c.CountryID == Convert.ToUInt32(ddlShippingCountry.SelectedItem.Value) select c.CountryCode).FirstOrDefault();                             
                            string nCountryCode = string.IsNullOrEmpty(this.location.ShippingCountry.NumericCountryCode) ? this.location.ShippingCountry.CountryCode : this.location.ShippingCountry.NumericCountryCode;

                            //Call Malvern API for rates 
                            MalvernGetRates MRates = new MalvernGetRates();
                            //string request = "0,\"003\"1,\"" + lblAccountID.Text + "\"12,\"" + txtShippingFirstName.Text + " " + txtShippingLastName.Text + "\"11,\"" + txtShippingCompany.Text + "\"13,\"" + txtShippingAddress1.Text + "\"14,\"" + txtShippingAddress2.Text + "\"15,\"" + txtShippingCity.Text + "\"16,\"" + ddlShippingState.SelectedItem.Text + "\"17,\"" + txtShippingPostalCode.Text + "\"50,\"" + sCountryCode + "\"18,\"\"21,\"" + weight + "\"1033,\"" + method.CarrierCode + " " + method.ShippingMethodCode + "\"99,\"\"";
                            string request = "0,\"003\"1,\"" + lblAccountID.Text + "\"12,\"" + this.location.ShippingFirstName + " " + this.location.ShippingLastName + "\"11,\"" + this.location.ShippingCompany + "\"13,\"" + this.location.ShippingAddress1 + "\"14,\"" + this.location.ShippingAddress2 + "\"15,\"" + this.location.ShippingCity + "\"16,\"" + shippingState + "\"17,\"" + shippingPostalCode + "\"50,\"" + nCountryCode + "\"18,\"\"21,\"" + weight + "\"1033,\"" + method.CarrierCode + " " + method.ShippingMethodCode + "\"99,\"\"";
                            System.Diagnostics.Debug.WriteLine(request);
                            string MalvernIPAddress = (from a in idc.AppSettings where a.AppKey == "MalvernIPAddress" select a.Value).FirstOrDefault();
                            string Malvernport = (from a in idc.AppSettings where a.AppKey == "MalvernPortAddress" select a.Value).FirstOrDefault();
                            shippingCost = MRates.MalvernGetRatesInfo(request, MalvernIPAddress, Malvernport);
                        }
                    }
                    else
                    {
                        shippingCost = 0.00M;
                    }
                }
                else
                {
                    // Get the shipping cost and determine if it's valid or not.
                    shippingCost = OrderHelpers.GetShippingCost(shippingState, shippingCountry, shippingPostalCode, shippingMethod, totalReplacement);
                }


                // If the shipping cost does have a value show it.
                if (shippingCost.HasValue)
                {
                    string additionalShippingChargePct;

                    if (MalvernIntegration)
                        additionalShippingChargePct = (from a in idc.AppSettings
                                                       where a.AppKey == "AdditionalMalvernShippingChargePct"
                                                       select a.Value).FirstOrDefault();
                    else
                        additionalShippingChargePct = (from a in idc.AppSettings
                                                       where a.AppKey == "AdditionalShippingChargePct"
                                                       select a.Value).FirstOrDefault();

                    if (!string.IsNullOrEmpty(additionalShippingChargePct))
                    {
                        shippingCost = shippingCost.Value * (1 + (Convert.ToDecimal(additionalShippingChargePct) / 100));
                    }

                }
            }

            // Set the grand total.
            grandTotal = subtotal + (shippingCost.HasValue ? shippingCost.Value : 0) - couponDiscount;

            return true;
        }
        catch
        {
            return false;
        }
    }

    private int GetShippingMethodID(int totalReplacement)
    {
        int shippingMethodID = 0;

        if (MalvernIntegration)
        {
            if (totalReplacement > 0)
                shippingMethodID = int.Parse(ddlShippingMethodMalvern.SelectedValue);
            else
            {
                int freeShippingMethodID = 0;

                switch (ddShippingCarrier.SelectedItem.Text.ToUpper())
                {
                    case "FEDEX":
                        freeShippingMethodID = (from a in idc.ShippingMethods where a.ShippingMethodDesc.Trim() == "FedEx Free Shipping" select a.ShippingMethodID).FirstOrDefault();
                        break;
                    case "DHL GLOBAL MAIL":
                        freeShippingMethodID = (from a in idc.ShippingMethods where a.ShippingMethodDesc.Trim() == "DHL Free Shipping" select a.ShippingMethodID).FirstOrDefault();
                        break;
                    case "UPS":
                        freeShippingMethodID = (from a in idc.ShippingMethods where a.ShippingMethodDesc.Trim() == "UPS Free Shipping" select a.ShippingMethodID).FirstOrDefault();
                        break;
                    case "US POSTAL SERVICES":
                        freeShippingMethodID = (from a in idc.ShippingMethods where a.ShippingMethodDesc.Trim() == "USPS Free Shipping" select a.ShippingMethodID).FirstOrDefault();
                        break;
                    case "DHL INTERNATIONAL":
                        freeShippingMethodID = (from a in idc.ShippingMethods where a.ShippingMethodDesc.Trim() == "DHL International Free Shipping" select a.ShippingMethodID).FirstOrDefault();
                        break;
                    case "N/A":
                        freeShippingMethodID = (from a in idc.ShippingMethods where a.ShippingMethodDesc.Trim() == "Free Shipping Ground" select a.ShippingMethodID).FirstOrDefault();
                        break;
                }
                shippingMethodID = freeShippingMethodID;
            }
        }
        else
        {
            if (totalReplacement > 0)
                shippingMethodID = int.Parse(ddlShippingMethod.SelectedValue);
            else
                shippingMethodID = 11;    // free FedEx shipping method
        }

        return shippingMethodID;
    }

    /// <summary>
    /// Trim the string by removing newline/enter characters.
    /// </summary>
    /// <param name="s">String to trim.</param>
    /// <returns>Returns a trimmed string.</returns>
    private String TrimNewLineString(String s)
    {

        String newString = "";
        if (s != null)
            newString = s.Replace("\r\n", " ");

        return newString;
    }

    private void ProcessOrder(ref int pOrderID)
    {
        try
        {
            var productGroupID = Convert.ToInt32(rblProductGroup.SelectedValue);            

            decimal subtotal = 0.00M;
            decimal couponDiscount = 0.00M;
            decimal grandTotal = 0.00M;
            decimal? shippingCost = 0.0M;
            int totalReplacement = 0;

            Order order = new Order();
            idc.Orders.InsertOnSubmit(order);

            if (this.HidRMAType.Value != "Recall")
            {
                if (rblLostRMAReason.SelectedValue == "L")
                    lostReplacementUnitPrice = 25;
                else
                    lostReplacementUnitPrice = 0;

                // Fill the order total variables.
                if (!getOrderTotals(out subtotal, out shippingCost, out couponDiscount, out grandTotal, out totalReplacement))
                {
                    throw new Exception("An error occurred while calculating the order totals.");
                }

                // If can not find shipping cost for the shipping method, throw an error.
                if (!shippingCost.HasValue)
                {
                    throw new Exception("Can not find shipping cost for this shipping method, please select a different shipping method.");
                }
                else
                {
                    if (MalvernIntegration && shippingCost.Value == 0 && totalReplacement > 0)
                    {
                        string errorMsg = "";
                        if (!ddlShippingMethodMalvern.SelectedItem.Text.ToLower().Contains("free"))
                        {
                            if (ddlShippingMethodMalvern.SelectedItem.Text.ToLower().Contains("parcel ground") && totalReplacement > 5)
                                errorMsg = "This shipping method is limitted and only accepts maximum weight of 5 devices, please select a different shipping method.";
                            else if (ddlShippingMethodMalvern.SelectedItem.Text.ToLower().Contains("parcel plug ground") && totalReplacement <= 5)
                                errorMsg = "This shipping method is limitted and only accepts minimum weight of 6 devices, please select a different shipping method.";
                            else if (ddlShippingMethodMalvern.SelectedItem.Text.ToLower().Contains("first class mail") && totalReplacement >= 5)
                                errorMsg = "This shipping method is limitted and only accepts maximum weight of 4 devices, please select a different shipping method.";
                            else
                                errorMsg = "Can not find shipping cost for this shipping method, please select a different shipping method.";

                            throw new Exception(errorMsg);
                        }
                    }
                }

                // Delete any payments made so far--
                idc.Payments.DeleteAllOnSubmit(order.Payments);

                // Create a payment record.
                Payment payment = new Payment()
                {
                    Authorized = true,
                    PaymentDate = DateTime.Now
                };
                
                // Set the payment
                payment.PaymentMethodID = 2; // Purchase Order
                payment.GatewayResponse = "POPAYMENT";
                payment.Captured = true;
                payment.Amount = grandTotal;
                payment.CreatedBy = this.UserName;
                payment.ApplicationID = 2;
                payment.CurrencyCode = this.account.CurrencyCode;
 
                order.Payments.Add(payment);
            }

            bool poIsRequired = false;
            int? packageTypeID = (int?)null;
            int? shippingOptionID = (int?)null;
            string orderTypeStr = "";
            string referralCode = "";
            decimal shippingCharge = 0.0M;
            double? discount = (double?)null;

            if (this.HidRMAType.Value != "Recall")
            {
                // If the brand is IC Care, then a PO is required.                
                poIsRequired = (account.BrandSourceID.Value == 3);

                // Set the package type                
                if (int.Parse(ddlPackageType.SelectedValue) != 0) packageTypeID = int.Parse(ddlPackageType.SelectedValue);

                // Set the shipping option                
                if (int.Parse(ddlShippingOption.SelectedValue) != 0) shippingOptionID = int.Parse(ddlShippingOption.SelectedValue);

                orderTypeStr = "LOST REPLACEMENT";
                referralCode = string.IsNullOrEmpty(this.account.ReferralCode) ? "0000" : account.BrandSourceID.Value == 3 ? "D012" : account.ReferralCode;
                shippingCharge = shippingCost.HasValue ? shippingCost.Value : 0.00M;
                discount = account.Discount.HasValue ? account.Discount.Value : (double?)null;
            }
            else
            {
                poIsRequired = false;

                orderTypeStr = "RECALL REPLACEMENT";
                referralCode = string.IsNullOrEmpty(this.account.ReferralCode) ? "0000" : this.account.ReferralCode;
                shippingCharge = decimal.Parse("0");
                discount = double.Parse("0");
            }

            // Set the order properties.
            order.AccountID = account.AccountID;
            order.OrderType = orderTypeStr;
            order.PONumber = this.txtPOno.Text;
            order.LocationID = location.LocationID;
            order.BrandSourceID = account.BrandSourceID.Value;
            order.PackageTypeID = packageTypeID;
            order.ShippingOptionID = shippingOptionID;
            order.ShippingMethodID = GetShippingMethodID(totalReplacement);
            order.OrderDate = DateTime.Now;
            order.CreatedBy = this.UserName; //ActiveDirectoryQueries.GetUserName();
            order.OrderStatusID = 3;
            order.ReferralCode = referralCode;
            order.SoftraxStatus = false;
            order.CurrencyCode = string.IsNullOrEmpty(this.account.CurrencyCode) ? "USD" : this.account.CurrencyCode;
            order.Rate = account.Rate;
            order.Discount = discount;
            order.OrderSource = "Business App";
            order.SpecialInstructions = txtSpecialInstruction.Text;
            order.PORequired = poIsRequired;
            order.ShippingCharge = shippingCharge;

            order.ContractStartDate = GetContractStartDate();
            order.ContractEndDate = GetContractEndDate();

            // set stx contract dates by selected service start date
            string proRatePeriod = radProratePeriod.SelectedItem.Text;
            string[] startEndProRatePeriod = proRatePeriod.Split('-');
            order.StxContractStartDate = DateTime.Parse(startEndProRatePeriod[0].Trim());
            order.StxContractEndDate = DateTime.Parse(startEndProRatePeriod[1].Trim());

            // ------------------------------------- Start adding order details records -------------------------------------- //
            int totalLostDevices = 0;
            int totalReplaceDevices = 0;
            int totalBlue = 0;
            int totalGrey = 0;
            int totalGreen = 0;
            int totalOrange = 0;
            int totalPink = 0;
            int totalRed = 0;

            foreach (GridViewRow row in this.gv_DeviceList.Rows)
            {
                CheckBox chkLostDevice = (CheckBox)row.FindControl("chkbxSelectDeviceList");
                CheckBox chkLostReplacementDevice = (CheckBox)row.FindControl("chkbxSelectReplaceDeviceList");
                HiddenField hidDeviceID = (HiddenField)row.FindControl("HidDeviceID");
                HiddenField hidAssignedUserID = (HiddenField)row.FindControl("HidAssignedUserID");
                DropDownList ddlBodyRegion = (DropDownList)row.FindControl("ddlBodyRegion");

                string color = row.Cells[2].Text.Replace("\n", "").ToString().Trim();
                string sn = row.Cells[4].Text.Replace("\n", "").ToString().Trim();

                if (chkLostDevice.Checked) totalLostDevices++;

                if (chkLostReplacementDevice.Checked)
                {
                    int colorCapProductID = 0;
                    int assignedUserID = string.IsNullOrEmpty(hidAssignedUserID.Value) ? 0 : int.Parse(hidAssignedUserID.Value);
                    int deviceID = int.Parse(hidDeviceID.Value);
                    string productSku = GetProductSkuByProductGroupIDAndColor(productGroupID, color.ToUpper());
                    switch (color.ToUpper())
                    {
                        case "BLUE":
                            totalBlue++;
                            break;
                        case "GREY":
                            totalGrey++;
                            break;
                        case "GREEN":
                            totalGreen++;
                            break;
                        case "ORANGE":
                            totalOrange++;
                            break;
                        case "PINK":
                            totalPink++;
                            break;
                        case "RED":
                            totalRed++;
                            break;
                    }
                    colorCapProductID = (from p in idc.Products where p.ProductSKU == productSku && p.ProductGroupID == productGroupID select p.ProductID).FirstOrDefault();

                    // Insert OrderUserAssign
                    if (assignedUserID > 0)
                    {
                        order.OrderUserAssigns.Add(new OrderUserAssign()
                        {
                            UserID = assignedUserID,
                            ProductID = colorCapProductID,
                            BodyRegionID = int.Parse(ddlBodyRegion.SelectedValue)
                        });
                    }

                }
            }

            // --------------------------- Add case cover color into order deatils ------------------------------------//
            if (totalBlue > 0)
            {
                AddProductDetail(productGroupID, order, "BLUE", totalBlue);
            }

            if (totalGrey > 0)
            {
                AddProductDetail(productGroupID, order, "GREY", totalGrey);                
            }

            if (totalGreen > 0)
            {
                AddProductDetail(productGroupID, order, "GREEN", totalGreen);                
            }

            if (totalOrange > 0)
            {
                AddProductDetail(productGroupID, order, "ORANGE", totalOrange);                
            }

            if (totalPink > 0)
            {
                AddProductDetail(productGroupID, order, "PINK", totalPink);                
            }

            if (totalRed > 0)
            {
                AddProductDetail(productGroupID, order, "RED", totalRed);                
            }

            // ----------------------For a replacement order, add a lost badge line item.--------------------------//
            if (this.HidRMAType.Value != "Recall")
            {
                int instadPlusLostBadgeProductID = (from p in idc.Products where p.ProductSKU == "/LOST BADGE" && p.ProductGroupID == productGroupID select p.ProductID).FirstOrDefault();
                order.OrderDetails.Add(new OrderDetail()
                {
                    Price = lostReplacementUnitPrice,
                    ProductID = instadPlusLostBadgeProductID,                 // add /LOST BADGE to orderdetails
                    Quantity = totalLostDevices,
                    OrderDetailDate = DateTime.Now
                });
            }

            // ---------------------- Add the ID+ Instadose SKU product to orderdetail ------------------------------//            
            totalReplaceDevices = totalBlue + totalGrey + totalGreen + totalOrange + totalPink + totalRed;

            if (totalReplaceDevices > 0)
            {
                var productSku = productGroupID == ProductGroupIDConstants.InstadosePlus ? "INSTA PLUS" : "INSTA ID2";
                int instaProductID = (from p in idc.Products where p.ProductSKU == productSku select p.ProductID).FirstOrDefault();

                order.OrderDetails.Add(new OrderDetail()
                {
                    Price = 0.00M,
                    Quantity = totalReplaceDevices,
                    OrderDetailDate = DateTime.Now,
                    ProductID = instaProductID,
                    LabelID = null,
                    OrderDevInitialize = false,
                    OrderDevAssign = false
                });
            }
            order.ACMIntegrationStatusID = 1;
            idc.SubmitChanges();

            // get the @@identity OrderId
            pOrderID = order.OrderID;

            // ------------------------------------- End adding order details records -------------------------------------- //            

            // Send an order to MAS.
            if (this.HidRMAType.Value != "Recall")
            {
                if (!Instadose.Integration.MAS.SendOrderToMAS(order.OrderID, this.UserName))
                {
                    throw new Exception("The order has been created, but the records could not be sent to MAS. Please contact IT for assistance.");
                }
            }
            else
            {
                int shipUserID = 0;
                int shipLocationID = this.location.LocationID;

                if (!Instadose.Integration.MAS.SendRecallOrderToMAS(order.OrderID, shipUserID, shipLocationID, this.UserName))
                {
                    throw new Exception("The order has been created, but the records could not be sent to MAS. Please contact IT for assistance.");
                }
            }

        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    private void AddProductDetail(int productGroupID, Order order, string color, int quantity)
    {
        string productSku = GetProductSkuByProductGroupIDAndColor(productGroupID, color);
        int colorCapProductID = (from p in idc.Products where p.ProductSKU == productSku && p.ProductGroupID == productGroupID select p.ProductID).FirstOrDefault();
        // Add the order details to the order.
        order.OrderDetails.Add(new OrderDetail()
        {
            // Set the SKU price to $0 when not new.
            // Price = skuPrice,
            Price = 0,
            ProductID = colorCapProductID,
            Quantity = quantity,
            OrderDetailDate = DateTime.Now
        });
    }

    private string GetProductSkuByProductGroupIDAndColor(int productGroupID, string color)
    {
        string productSku = string.Empty;

        if (productGroupID == ProductGroupIDConstants.Instadose2New)
        {
            switch (color.ToUpper())
            {
                case "BLUE":
                    productSku = "ID2BBUMP";
                    break;
                case "GREY":
                    productSku = "ID2GYBUMP";
                    break;
                case "GREEN":
                    productSku = "ID2GBUMP";
                    break;
                case "ORANGE":
                    productSku = "ID2OBUMP";
                    break;
                case "PINK":
                    productSku = "ID2PBUMP";
                    break;
                case "RED":
                    productSku = "ID2RBUMP";
                    break;
            }
        }
        else if (productGroupID == ProductGroupIDConstants.InstadosePlus)
        {
            switch (color.ToUpper())
            {
                case "BLUE":
                    productSku = "COLOR CAP BLUE";
                    break;
                case "GREY":
                    productSku = "COLOR CAP GREY";
                    break;
                case "GREEN":
                    productSku = "COLOR CAP GRN";
                    break;
                case "ORANGE":
                    productSku = "COLOR CAP ORANG";
                    break;
                case "PINK":
                    productSku = "COLOR CAP PINK";
                    break;
                case "RED":
                    productSku = "COLOR CAP RED";
                    break;
            }
        }

        return productSku;
    }

    private DateTime GetContractStartDate()
    {
        // If an account is set up for location billing, then contract start date = location.ContractStartDate. Else then contract start date = account.ContractStartDate
        if (account.UseLocationBilling && location.ContractStartDate.HasValue)
            return location.ContractStartDate.Value;
        else
            return account.ContractStartDate.Value;
    }

    private DateTime GetContractEndDate()
    {
        // If an account is set up for location billing, then contract end date = location.ContractEndDate. Else then contract end date = account.ContractEndDate
        if (account.UseLocationBilling && location.ContractEndDate.HasValue)
            return location.ContractEndDate.Value;
        else
            return account.ContractEndDate.Value;
    }

    private DateTime GetDefaultSoftTraxStartDate(bool pIsFirstOrder, DateTime pOrderCreatedDate)
    {
        try
        {
            if (pIsFirstOrder)
                return GetContractStartDate();
            else
            {
                DateTime startQuarterDate;
                int qtrNo = Common.calculateNumberOfQuarterService(GetContractStartDate(), GetContractEndDate(), pOrderCreatedDate, out startQuarterDate);
                return startQuarterDate;
            }
        }
        catch (Exception)
        {
            return GetContractStartDate();
        }
    }

    private bool validateGridViewInput(ref string pErrorMsg)
    {
        int selTotal = 0;
        foreach (GridViewRow row in this.gv_DeviceList.Rows)
        {
            CheckBox chkLostDevice = (CheckBox)row.FindControl("chkbxSelectDeviceList");
            CheckBox chkLostReplacementDevice = (CheckBox)row.FindControl("chkbxSelectReplaceDeviceList");
            string color = row.Cells[2].Text.Replace("\n", "").Replace("&nbsp;", "").ToString().Trim();

            string sn = row.Cells[4].Text.Replace("\n", "").ToString().Trim();

            if (chkLostDevice.Checked) selTotal++;

            if (!chkLostDevice.Checked && chkLostReplacementDevice.Checked)
            {
                pErrorMsg = "Please check both check boxes for replaced SN: " + sn;
                return false;
            }

            if (chkLostReplacementDevice.Checked)
            {
                if (string.IsNullOrEmpty(color) || color.ToUpper() == "N/A")
                {
                    pErrorMsg = "See IT for help. No color for replaced SN: " + sn;
                    return false;
                }
            }
        }

        if (selTotal == 0)
        {
            pErrorMsg = "No device have been selected, please select at least one device to create an order.";
            return false;
        }

        return true;
    }

    private bool validatePaymentInfo(ref string pErrorMsg)
    {
        //if (this.rblstPayMethod.SelectedValue == "Credit Card")
        //{
        //    if (txtCCno.Text == "")
        //    {
        //        pErrorMsg = "The credit card number is required.";
        //        return false;
        //    }

        //    if (txtCCName.Text == "")
        //    {
        //        pErrorMsg = "The name on the card is required.";
        //        return false;
        //    }

        //    int expMonth = 0;
        //    int expYear = 0;
        //    int.TryParse(ddlCCMonth.SelectedValue, out expMonth);
        //    int.TryParse(ddlCCYear.SelectedValue, out expYear);

        //    if (expMonth == 0)
        //    {
        //        pErrorMsg = "Month is required.";
        //        return false;
        //    }

        //    if (expYear == 0)
        //    {
        //        pErrorMsg = "Year is required.";
        //        return false;
        //    }

        //    int lastDayOfMonth = DateTime.DaysInMonth(expYear, expMonth);
        //    DateTime ccExpiredDate = Convert.ToDateTime(expMonth.ToString() + "/" + lastDayOfMonth.ToString() + "/" + expYear.ToString());
        //    if (ccExpiredDate < DateTime.Today)
        //    {
        //        pErrorMsg = "The credit card expiration is expired.";
        //        return false;
        //    }

        //    if (txtCCcvc.Text == "")
        //    {
        //        pErrorMsg = "Card security code is required.";
        //        return false;
        //    }
        //}
        //else
        //{
            if (txtPOno.Text == "")
            {
                pErrorMsg = "A PO number is required.";
                return false;
            }
            else if (txtPOno.Text.Length > 15)
            {
                pErrorMsg = "PO Number is max 15 characters or numerics.";
                return false;
            }
        //}
        return true;
    }

    private bool passInputsValidation_IDPlusLostReplacementRecallOrderDialog()
    {
        string errorString = "";

        if (this.HidRMAType.Value == "Recall")
        {
            if (string.IsNullOrEmpty(txtRecallNote.Text))
            {
                this.VisibleErrors_IDPlusLostReplacementRecallOrderDialog("Recall Note is required.");
                return false;
            }
        }
        else
        {
            if (validatePaymentInfo(ref errorString) == false)
            {
                this.VisibleErrors_IDPlusLostReplacementRecallOrderDialog(errorString);
                return false;
            }
        }

        if (validateGridViewInput(ref errorString) == false)
        {
            this.VisibleErrors_IDPlusLostReplacementRecallOrderDialog(errorString);
            return false;
        }

        return true;
    }

    private void InvisibleErrors_IDPlusLostReplacementRecallOrderDialog()
    {
        this.IDPlusLostReplacementRecallOrderDialogErrorMsg.InnerHtml = "";
        this.IDPlusLostReplacementRecallOrderDialogError.Visible = false;
    }

    private void VisibleErrors_IDPlusLostReplacementRecallOrderDialog(string error)
    {
        this.IDPlusLostReplacementRecallOrderDialogErrorMsg.InnerHtml = error;
        this.IDPlusLostReplacementRecallOrderDialogError.Visible = true;
    }

    protected void imgAssignedWearerForDevice_Click(object sender, ImageClickEventArgs e)
    {
        ImageButton imgbtn = (ImageButton)sender;
        string imgbtnCommandName = imgbtn.CommandName.ToString();
        string serialNo = imgbtn.CommandArgument.ToString();

        loadAssignWearerWindow(serialNo);
    }

    private void loadAssignWearerWindow(string pSerialNo)
    {
        this.lblSerialNo.Text = pSerialNo;

        gv_WearerList.DataSource = null;
        gv_WearerList.DataBind();

        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "$('#assignedWearerDialog').dialog('open');", true);
    }

    protected void rbtnSearchBy_SelectedIndexChanged(object sender, EventArgs e)
    {
        switch (this.rbtnSearchBy.SelectedValue)
        {
            case "all":
            case "assigned":
            case "un-assigned":
                searchFilter.Visible = false;
                break;
            case "lastName":
                searchFilter.Visible = true;
                break;
        }
    }

    protected void btnWearerSearch_Click(object sender, EventArgs e)
    {
        var searchResult = idc.sp_GetUsersBySearch(account.AccountID, location.LocationID, this.rbtnSearchBy.SelectedValue, this.txtOrderAssignSearch.Text).ToList();

        gv_WearerList.DataSource = searchResult;
        gv_WearerList.DataBind();
    }

    protected void btnAssignedWearer_Click(object sender, EventArgs e)
    {
        foreach (GridViewRow row in this.gv_WearerList.Rows)
        {
            RadioButton radButton = (RadioButton)row.FindControl("radSelectUser");

            if (radButton.Checked)
            {
                string replacedSN = this.lblSerialNo.Text;
                HiddenField hidUserID = (HiddenField)row.FindControl("hidUserID");

                ReLoad_gv_DeviceList(replacedSN, hidUserID.Value);

                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID,
                        "$('#assignedWearerDialog').dialog('close');", true);
                break;
            }
        }
    }

    // ---------xxxxxxxxxxxxxxx---------- ID+ LOST/REPLACEMENT CREATE ORDER ------------xxxxxxxxxxxxxxxxxxx-------------//

    # endregion

    # region E-MAIL ORDER ACKNOWLEDGEMENT MODAL/DIALOG FUNCTIONS
    private void InvisibleErrors_orderAckDialog()
    {
        // Reset submission form error message
        this.orderAckDialogErrors.Visible = false;
        this.orderAckDialogErrorMsg.InnerHtml = "";
    }

    private void VisibleErrors_orderAckDialog(string error)
    {
        //errorMsg.InnerText = string.Format("An error was encountered: {0}", error);
        this.orderAckDialogErrorMsg.InnerHtml = error;
        this.orderAckDialogErrors.Visible = true;
    }

    private void InvisibleMsgs_orderAckDialog()
    {
        // Reset submission form error message
        this.orderAckDialogMsgs.Visible = false;
        this.orderAckDialogMsg.InnerHtml = "";
    }

    private void VisibleMsgs_orderAckDialog(string message)
    {
        //errorMsg.InnerText = string.Format("An error was encountered: {0}", error);
        this.orderAckDialogMsg.InnerHtml = message;
        this.orderAckDialogMsgs.Visible = true;
    }

    private void SetDefaultValues_orderAckDialog()
    {
        LoadEmailOrderAcknowledgement();
    }

    private bool passInputsValidation_orderAckDialog()
    {
        string regExEmail = @"^(?("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))$";
        string errorString = "";


        // Create a regular expression for the email.
        Regex regEmail = new Regex(regExEmail);


        if (this.txtEmail.Text.Trim().Length == 0)
        {
            errorString = "Email address is required.";
            this.VisibleErrors_orderAckDialog(errorString);
            return false;
        }
        else if (this.txtEmail.Text.Trim().Length > 50)
        {
            errorString = "Email address is too large.";
            this.VisibleErrors_orderAckDialog(errorString);
            return false;
        }
        else if (!regEmail.IsMatch(this.txtEmail.Text.Trim()))
        {
            errorString = "Email address is not valid.";
            this.VisibleErrors_orderAckDialog(errorString);
            return false;
        }

        return true;
    }

    protected void btnLoadEmailOrderAck_Click(object sender, EventArgs e)
    {
        LoadEmailOrderAcknowledgement();
    }

    protected void btnLoadEmailOrderAckSuccess_Click(object sender, EventArgs e)
    {
        LoadEmailOrderAcknowledgement();
    }

    private void LoadEmailOrderAcknowledgement()
    {
        string errorString = "";
        //btnExecuteSendEmail.Visible = true;

        this.InvisibleErrors_orderAckDialog();

        var AcctInfo = (from a in idc.Accounts
                        join u in idc.Users
                        on a.AccountAdminID equals u.UserID
                        where a.AccountID == this.accountID
                        select u.Email).First();
        // 9/2013 WK 
        //**********************************************************************************
        // Load ACTIVE order acknowledgements for this account only for modal email dropdown!
        //**********************************************************************************
        var OrderAckInfo = (from d in idc.Documents
                            where d.AccountID == accountID
                                 && d.Description == "Original Order Acknowledgement"
                                 && d.Active
                                 && d.OrderID != null
                            orderby d.OrderID descending
                            select d.OrderID).ToList();

        ddlEmailOrder.DataSource = OrderAckInfo;
        ddlEmailOrder.DataBind();

        if (ddlEmailOrder.Items.Count == 0)
        {
            //btnExecuteSendEmail.Visible = false;

            errorString = "Unable to email since there are no order acknowledgements for this account.  Please create one.";
            this.VisibleErrors_orderAckDialog(errorString);
        }

        this.txtEmail.Text = AcctInfo;
        this.lblAccountID.Text = this.accountID.ToString();
    }

    protected void btnCancelSendEmail_Click(object sender, EventArgs e)
    {
        InvisibleErrors_orderAckDialog();
        InvisibleMsgs_orderAckDialog();

        SetDefaultValues_orderAckDialog();
    }

    protected void btnCloseEmail_Click(object sender, EventArgs e)
    {
        InvisibleErrors_orderAckDialog();
        InvisibleMsgs_orderAckDialog();

        SetDefaultValues_orderAckDialog();
    }

    protected void btnCloseWindow_Click(object sender, EventArgs e)
    {
        InvisibleErrors_orderAckDialog();
        InvisibleMsgs_orderAckDialog();

        SetDefaultValues_orderAckDialog();
    }

    protected void ddlEmailOrder_SelectedIndexChanged(object sender, EventArgs e)
    {

    }

    /// <summary>
    /// GenerateOrderAcknowledgement
    /// Check Document table.  Create and save original order acknowledgment document
    /// if it does not exist in Document table.    
    /// </summary>
    /// <param name="accountID"></param>
    /// <param name="ordID"></param>
    /// <returns></returns>
    protected string GenerateOrderAcknowledgement(int accountID, int orderID)
    {
        Stream oStream = null;

        string[] rptParamName = new string[1];
        string[] rptParamVals = new string[1];
        string pdfFileName = "Ack_" + orderID.ToString() + ".pdf";
        string crFileNamePath = Server.MapPath("../GenerateOrderAck.rpt");

        // 9/2013 WK
        //*******************************************************************************************
        // Check to see if original order acknowledgement document data exists for this account.  
        // Also, extract PDF filename only for ACTIVE order acknowledgement.
        //********************************************************************************************
        Document ordAck = (from d in idc.Documents
                           where d.AccountID == accountID && d.OrderID == orderID
                                && d.Description == "Original Order Acknowledgement"
                                && d.FileName == pdfFileName
                                && d.Active
                           select d).FirstOrDefault();

        if (ordAck == null)
        {
            try
            {
                cryRpt.Load(crFileNamePath);

                cryRpt.SetParameterValue(0, orderID);

                string ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString.ToString();
                string myDatabase = Regex.Match(ConnectionString, "(Initial Catalog=|Database=){1}([A-Za-z0-9_]+)[;]?").Groups[2].ToString();
                string myServer = Regex.Match(ConnectionString, "(Data Source=){1}([\\w\\d\\.\\$\\-]+)[;]?").Groups[2].ToString();
                string myUserID = Regex.Match(ConnectionString, "(User ID=|UID=){1}([\\w\\d\\.]+)[;]?").Groups[2].ToString();
                string myPassword = Regex.Match(ConnectionString, "(Pwd=|Password=|PWD=){1}([\\w\\d\\.\\$]+)[;]?").Groups[2].ToString();

                cryRpt.DataSourceConnections[0].SetConnection(myServer, myDatabase, myUserID, myPassword);

                //Create PDF file and store in Memory               
                oStream = cryRpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                byte[] streamArray = new byte[oStream.Length];
                oStream.Read(streamArray, 0, Convert.ToInt32(oStream.Length - 1));

                // insert record to database

                // 8/2012 - New Documents table for all attachments for email, etc.
                // insert record to document table.
                Document doc = new Document()
                {
                    Active = true,
                    AccountID = accountID,
                    OrderID = orderID,
                    CreatedBy = this.UserName,
                    CreatedDate = DateTime.Now,
                    Description = "Original Order Acknowledgement",
                    DocumentCategory = "Order Acknowledgement",
                    DocumentGUID = Guid.NewGuid(),
                    FileName = pdfFileName,
                    DocumentContent = streamArray,
                    MIMEType = "application/pdf",
                };

                // Get the current website environment
                string siteUrl = System.Configuration.ConfigurationManager.AppSettings["api_webaddress"];

                // Construct the download path for the user.
                string downloadUrl = string.Format("{0}Support/PublicDocument.aspx?GUID={1}&Ticks={2}",
                    siteUrl, doc.DocumentGUID, doc.CreatedDate.Ticks);

                // Upload the document
                idc.Documents.InsertOnSubmit(doc);
                idc.SubmitChanges();
            }
            catch (Exception ex)
            {
                // Report the error to the message system.
                Basics.WriteLogEntry(ex.Message, Page.User.Identity.Name,
                    "Portal.InstaDose_InformationFinder_GenerateOrderAck", Basics.MessageLogType.Critical);

                Response.Write(ex);
            }
            finally
            {
                cryRpt.Close();
                cryRpt.Dispose();
                oStream.Flush();
                oStream.Close();
                oStream.Dispose();
            }

        }
        return pdfFileName;
    }

    /// <summary>
    /// EmailOrderAcknowledgement
    /// Email original Order Acknowledgement to customer.
    /// </summary>
    /// <param name="AdminFLName"></param>
    /// <param name="EmailAddress"></param>
    /// <param name="AccountID"></param>
    /// <param name="OriginalOrderID"></param>
    /// <param name="AckFileName"></param>
    /// <returns></returns>
    protected bool EmailOrderAcknowledgement(string AdminFLName, string EmailAddress, int AccountID, int originalOrderID, string AckFileName)
    {
        bool RtnResponse = false;

        //**********************************************************************************
        // 8/2012 - New email code for Instadose 2 using new Document table
        //      and Instadose.Message class.
        //
        // 9/2013 WK - query only Active acknowledgement file for the given account, 
        //          order only!
        //**********************************************************************************

        // Retrieve email acknowledgement from Document table. 
        Document doc = (from d in idc.Documents
                        where d.AccountID == AccountID
                            && d.OrderID == originalOrderID
                            && d.Active
                        select d).FirstOrDefault();

        // Get the current website environment
        string siteUrl = System.Configuration.ConfigurationManager.AppSettings["api_webaddress"];

        // Construct the download path for the user.  GUID+Ticks link via Email.
        string downloadUrl = string.Format("{0}Support/PublicDocument.aspx?GUID={1}&Ticks={2}",
            siteUrl, doc.DocumentGUID, doc.CreatedDate.Ticks);

        // Stop if noting found 
        if (doc == null)
        {
            RtnResponse = false;
        }
        else
        {
            //// Create the template.
            MessageSystem msgSystem = new MessageSystem()
            {
                Application = "Instadose.com Order Acknowledgement",
                CreatedBy = this.UserName,
                FromAddress = "noreply@instadose.com",
                ToAddressList = new List<string>()
            };

            // Add the email
            msgSystem.ToAddressList.Add(EmailAddress);

            // Replace the fields in the template
            Dictionary<string, string> fields = new Dictionary<string, string>();

            // Add the fields to replace.
            // MUST BE EXACT with email template stored EmailTemplates table.  Check
            // description for fields that need to have values for your document.

            fields.Add("OrderID", originalOrderID.ToString());
            fields.Add("FullName", AdminFLName);
            fields.Add("DownloadUrl", downloadUrl);

            // Send using the order acknowledgement template, with no brand. 
            // MUST BE EXACT with EmailTemplate (TemplateName)! 
            int response = msgSystem.Send("Order Acknowledgement", "", fields);

            if (response != 0) RtnResponse = true; //success!

        }

        return RtnResponse;
    }

    protected void btnExecuteSendEmail_Click(object sender, EventArgs e)
    {
        string ResponseMessageStr = "";
        int AccountID = this.accountID;
        int OrderID = this.orderID;
        int originalOrderID = 0;

        string EmailAddress = this.txtEmail.Text.Trim();

        //If no orders with acknowledgement - do not email.
        if (ddlEmailOrder.Items.Count == 0) return;

        if (passInputsValidation_orderAckDialog())
        {
            ResponseMessageStr += "Account ID:" + AccountID.ToString()
                    + ", Email: " + EmailAddress.ToString();
            try
            {

                // Grab the originial order 
                //var order = (from o in idc.Orders
                //             where o.AccountID == AccountID
                //             orderby o.OrderID
                //             select o).First();

                //originalOrderID = order.OrderID;

                //***********************************************************************
                //9/2013 WK - extract any order that has Active acknowledgement stored
                //      in Documents table using dropdown on the modal email form.
                //************************************************************************
                //Set selected order acknowledgement to be emailed.
                OrderID = int.Parse(ddlEmailOrder.SelectedItem.Text);

                //Generate OrderAck and save in database
                string AckOrdFileName = GenerateOrderAcknowledgement(AccountID, OrderID);

                try
                {
                    // Grab Account Admin First and Last name
                    var AdminInfo = (from u in idc.Users
                                     join a in idc.Accounts
                                     on u.UserID equals a.AccountAdminID
                                     where a.AccountID == AccountID && a.AccountAdminID != null
                                     select new
                                     {
                                         AdminName =
                                           ((u.FirstName != null) ? u.FirstName.ToString() : "")
                                           + " " +
                                           ((u.LastName != null) ? u.LastName.ToString() : "")
                                     }).ToArray();

                    string AdminName = (AdminInfo.Length == 0) ? "Administrator" : AdminInfo[0].AdminName.ToString();

                    bool SendEmailResponse = EmailOrderAcknowledgement(AdminName, EmailAddress,
                            AccountID, OrderID, AckOrdFileName);

                    if (SendEmailResponse == true)
                    {
                        ResponseMessageStr += "</br> <ul><li>Email sent!</li>";
                        // insert a Note to indicate email has been sent.
                        AccountNote AcctNote = null;
                        AcctNote = new AccountNote()
                        {
                            AccountID = AccountID,
                            CreatedDate = DateTime.Now,
                            CreatedBy = UserName,
                            NoteText = "Email Order Acknowledgement to " + EmailAddress,
                            Active = true
                        };
                        idc.AccountNotes.InsertOnSubmit(AcctNote);
                        idc.SubmitChanges();

                        ResponseMessageStr += "<li>Note Added!</li></ul>";

                        VisibleMsgs_orderAckDialog(string.Format("{0}", ResponseMessageStr));
                        //VisibleMsgs_form("Order Acknowledgement has been emailed.");

                        this.lblSentEmail.Text = "An email has been sent to " + EmailAddress;
                        this.lblSentEmail.Visible = true;

                        string script = "orderAckSent = true;" +
                            "$('#emailOrderAcknowledgementDialog').dialog('close');" +
                            "$('#emailOrderAcknowledgementSuccessDialog').dialog('open');";

                        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID,
                                script, true);

                    }
                    else
                    {
                        ResponseMessageStr += "<br /> <ul><li>Email Process failed!</li></ul>";
                        this.VisibleErrors_orderAckDialog(string.Format("{0}", ResponseMessageStr));
                    }
                }
                catch
                {
                    ResponseMessageStr += "<br /> <ul><li>Email failed, Check email address!</li></ul>";
                    this.VisibleErrors_orderAckDialog(string.Format("{0}", ResponseMessageStr));
                }
            }
            catch
            {
                ResponseMessageStr += "<br /> <ul><li>Account Not found!</li></ul>";// +ex.ToString();
                this.VisibleErrors_orderAckDialog(string.Format("{0}", ResponseMessageStr));
            }

        }
    }
    # endregion

    # region INVOICES TAB FUNCTIONS
    protected void rgInvoices_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
    {
        // For MAS Invoices
        Int32.TryParse(Request.QueryString["ID"], out accountID);

        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.MASConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.StoredProcedure;

        // Using the vw_CreditCardExpirationsListing.
        string sqlQuery = "sp_if_GetAccountInvoicesByAccountID";

        sqlCmd.Parameters.Add(new SqlParameter("@pAccountID", accountID));

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        List<InvoiceGridItem> invoiceItems = new List<InvoiceGridItem>();

        decimal outstandingBalance = 0;

        decimal amount = 0;
        decimal payments = 0;
        decimal adjustments = 0;
        decimal credits = 0;
        decimal balance = 0;

        string currencyCode = "";

        while (sqlDataReader.Read())
        {
            // Get Outstanding Balance (Example: 10.00 USD will become 10.00)
            outstandingBalance += formatMoneyValues(sqlDataReader["Balance"].ToString());

            // Get Currency Code (Example: USD).
            currencyCode = sqlDataReader["CurrencyCode"].ToString();

            amount = formatMoneyValues(sqlDataReader["Amount"].ToString());
            payments = formatMoneyValues(sqlDataReader["Payments"].ToString());
            adjustments = formatMoneyValues(sqlDataReader["Adjustments"].ToString());
            credits = formatMoneyValues(sqlDataReader["Credits"].ToString());
            balance = formatMoneyValues(sqlDataReader["Balance"].ToString());

            invoiceItems.Add(new InvoiceGridItem
            {
                Account = sqlDataReader["AccountID"].ToString(),
                InvoiceID = sqlDataReader["InvoiceNo"].ToString(),
                OrderID = sqlDataReader["OrderID"].ToString(),
                //InvoiceDate = (DateTime)sqlDataReader["InvoiceDate"],
                InvoiceDate = string.IsNullOrEmpty(sqlDataReader["InvoiceDate"].ToString()) ? new DateTime() : (DateTime)sqlDataReader["InvoiceDate"],
                LastTransaction = string.IsNullOrEmpty(sqlDataReader["PayDate"].ToString()) ? (DateTime?)null : (DateTime)sqlDataReader["PayDate"],
                InvoiceAmount = amount,
                AdjustmentAmount = adjustments,
                CreditAmount = credits,
                CurrencyCode = currencyCode,
                PaymentAmount = payments,
                Balance = balance,
                FilePath = "",
                IsAXInvoice = false
            });
        }

        sqlConn.Close();
        sqlDataReader.Close();

        // TODO: Yong - uncomment below lines after figure out how to not display duplicate invoices
        //// For AX Invoices
        //string axAccount = string.Format("INS-{0}", Request.QueryString["ID"]);

        //List<Mirion.DSD.GDS.API.DataTypes.AXInvoice> invoices = axInvoiceRequests.GetInvoicesByAccount(axAccount);

        //if (invoices != null && invoices.Count > 0)
        //{
        //    currencyCode = invoices[0].CurrencyCode;

        //    foreach (var invoice in invoices)
        //    {
        //        if (invoice.Balance != null)
        //            outstandingBalance += (decimal)invoice.Balance;

        //        invoiceItems.Add(new InvoiceGridItem
        //        {
        //            Account = invoice.Account,
        //            InvoiceID = invoice.InvoiceID,
        //            OrderID = null,
        //            InvoiceDate = invoice.InvoiceDate,
        //            LastTransaction = invoice.LastTransaction,
        //            InvoiceAmount = invoice.InvoiceAmount,
        //            AdjustmentAmount = invoice.AdjustmentAmount,
        //            CreditAmount = invoice.CreditAmount,
        //            CurrencyCode = invoice.CurrencyCode,
        //            PaymentAmount = invoice.PaymentAmount,
        //            Balance = invoice.Balance,
        //            FilePath = invoice.FilePath,
        //            IsAXInvoice = true
        //        });
        //    }
        //}

        //if (invoiceItems != null && invoiceItems.Count > 0)
        //    invoiceItems = invoiceItems.OrderByDescending(i => i.InvoiceDate).ToList();

        rgInvoices.DataSource = invoiceItems;

        string completeDisplayOfOutstandingBalance = string.Format("{0:#,0.00}", outstandingBalance) + " " + currencyCode;
        lblOutstandingBalance.Text = completeDisplayOfOutstandingBalance;
    }

    private class InvoiceGridItem
    {
        public string Account { get; set; }
        public string InvoiceID { get; set; }
        public string OrderID { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime? LastTransaction { get; set; }
        public decimal InvoiceAmount { get; set; }
        public decimal AdjustmentAmount { get; set; }
        public decimal CreditAmount { get; set; }
        public string CurrencyCode { get; set; }
        public decimal? PaymentAmount { get; set; }
        public decimal? Balance { get; set; }
        public string FilePath { get; set; }

        public bool IsAXInvoice { get; set; }
    }

    protected void rgInvoices_OnItemCommand(object sender, Telerik.Web.UI.GridCommandEventArgs e)
    {
        rgInvoices.Rebind();
    }

    protected void rgInvoices_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
    {
        SortOrder(rgInvoices, e);
    }

    protected void rgInvoices_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            //GridDataItem item = (GridDataItem)e.Item;
            //// Create TableCell object for the following:
            ////  1.) Actual InvoiceNo --> LinkButton to Invoice.
            ////  2.) Pre-Pay/Other --> Label only.
            //string invoiceNo = item.GetDataKeyValue("InvoiceNo").ToString();

            //var lnkbtn = (HyperLink)item.FindControl("lnkInvoiceNumber");
            //Label lbl = (Label)item.FindControl("lblInvoiceNumber");

            //if (invoiceNo == null) return;

            //// IF the InvoiceNo is any of the following (or Contains any of the following), then only display the Label (no LinkButton).
            //if (invoiceNo.Contains("Pre-Payment") || invoiceNo.Contains("Pre-Pay Adj") || invoiceNo.Contains("Credit Memo") || invoiceNo.Contains("Debit Memo"))
            //{
            //    lnkbtn.Visible = false;
            //    lbl.Visible = true;
            //}
            //else
            //{
            //    lnkbtn.Visible = true;
            //    lbl.Visible = false;
            //}

            GridDataItem item = e.Item as GridDataItem;
            var invoice = item.DataItem as InvoiceGridItem;

            ImageButton imgbtnViewInvoiceDetails = item.FindControl("imgbtnViewInvoiceDetails") as ImageButton;
            HyperLink lnkInvoiceNumber = item.FindControl("lnkInvoiceNumber") as HyperLink;
            Label lblInvoiceNumber = item.FindControl("lblInvoiceNumber") as Label;
            HyperLink hyprlnkOrderID_Invoices = item.FindControl("hyprlnkOrderID_Invoices") as HyperLink;
            HyperLink hlInvoiceOrders = item.FindControl("hlInvoiceOrders") as HyperLink;

            if (!invoice.IsAXInvoice)
            {
                imgbtnViewInvoiceDetails.Visible = true;

                if (invoice.InvoiceID.Contains("Pre-Payment") || invoice.InvoiceID.Contains("Pre-Pay Adj") || invoice.InvoiceID.Contains("Credit Memo") || invoice.InvoiceID.Contains("Debit Memo"))
                {
                    lnkInvoiceNumber.Visible = false;
                    lblInvoiceNumber.Visible = true;
                }
                else
                {
                    lnkInvoiceNumber.NavigateUrl = string.Format("ViewInvoice.aspx?Invoice={0}", invoice.InvoiceID);

                    lnkInvoiceNumber.Visible = true;
                    lblInvoiceNumber.Visible = false;
                }

                hyprlnkOrderID_Invoices.Visible = true;
                hlInvoiceOrders.Visible = false;
            }
            else
            {
                imgbtnViewInvoiceDetails.Visible = false;

                if (!string.IsNullOrEmpty(invoice.FilePath))
                {
                    lnkInvoiceNumber.NavigateUrl = string.Format("AXViewInvoice.aspx?Invoice={0}", invoice.InvoiceID);

                    lnkInvoiceNumber.Visible = true;
                    lblInvoiceNumber.Visible = false;
                }
                else
                {
                    lnkInvoiceNumber.Visible = false;
                    lblInvoiceNumber.Visible = true;
                }

                hyprlnkOrderID_Invoices.Visible = false;
                hlInvoiceOrders.Visible = true;
                hlInvoiceOrders.NavigateUrl = string.Format("javascript: openAXInvoiceOrderDialog('{0}');", invoice.InvoiceID);
            }
        }
    }

    #region VIEW INVOICE DETAILS GRIDVIEW IN MODAL/DIALOG.
    // Opens Modal/Dialog for Invoice Details.
    protected void imgbtnViewInvoiceDetails_OnClick(object sender, EventArgs e)
    {
        GridDataItem item = (sender as ImageButton).NamingContainer as GridDataItem;
        ImageButton imgBtn = (ImageButton)item.FindControl("imgbtnViewInvoiceDetails");
        string invoiceNo = item.GetDataKeyValue("InvoiceID").ToString();

        if (imgBtn.CommandName != "OpenDialog") return;
        if (invoiceNo == "" || invoiceNo == null) return;

        // Store InvoiceNo in HiddenField.
        this.hdnfldInvoiceNo.Value = invoiceNo;

        BindToGVInvoiceDetails(invoiceNo);

        // Send JavaScript to client to close/display the modal.
        string script = "openInvoiceDetailsDialog('" + invoiceNo + "')";
        ScriptManager.RegisterClientScriptBlock(Page, typeof(Page), "openModal", script, true);
    }

    protected void BindToGVInvoiceDetails(string invoiceno)
    {
        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.MASConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.StoredProcedure;

        // Using the vw_CreditCardExpirationsListing.
        string sqlQuery = "sp_if_GetAllInvoiceDetails";

        sqlCmd.Parameters.Add(new SqlParameter("@pInvoiceNo", invoiceno));

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable dtInvoicesDetailsTable = new DataTable();

        dtInvoicesDetailsTable = new DataTable("Get Invoice Details");

        // Create the columns for the DataTable.
        dtInvoicesDetailsTable.Columns.Add("InvoiceNo", Type.GetType("System.String"));
        dtInvoicesDetailsTable.Columns.Add("TransactionDate", Type.GetType("System.DateTime"));
        dtInvoicesDetailsTable.Columns.Add("TransactionType", Type.GetType("System.String"));
        dtInvoicesDetailsTable.Columns.Add("CheckNo", Type.GetType("System.String"));
        dtInvoicesDetailsTable.Columns.Add("AmountWithCurrency", Type.GetType("System.String"));

        while (sqlDataReader.Read())
        {
            DataRow drow = dtInvoicesDetailsTable.NewRow();

            // Fill row details.
            drow["InvoiceNo"] = sqlDataReader["InvoiceNo"];
            drow["TransactionDate"] = sqlDataReader["TransactionDate"];
            drow["TransactionType"] = sqlDataReader["TransactionType"];
            drow["CheckNo"] = sqlDataReader["CheckNo"];
            drow["AmountWithCurrency"] = sqlDataReader["AmountWithCurrency"];

            // Add rows to DataTable.
            dtInvoicesDetailsTable.Rows.Add(drow);
        }

        // Bind the results to the gridview
        this.gvViewInvoiceDetails.DataSource = dtInvoicesDetailsTable;
        this.gvViewInvoiceDetails.DataBind();

        sqlConn.Close();
        sqlDataReader.Close();
    }

    protected void gvViewInvoiceDetails_Sorting(object sender, GridViewSortEventArgs e)
    {
        this.gvViewInvoiceDetails.PageIndex = 0;
    }

    protected void gvViewInvoiceDetails_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        if (this.hdnfldInvoiceNo.Value == "0") return;

        this.gvViewInvoiceDetails.PageIndex = e.NewPageIndex;
        BindToGVInvoiceDetails(this.hdnfldInvoiceNo.Value);
    }

    protected void btnOK_Click(object sender, EventArgs e)
    {
        // Send JavaScript to client to close/display the modal.
        string script = "closeDialog('invoiceDetailsDialog')";
        ScriptManager.RegisterClientScriptBlock(Page, typeof(Page), "closeModal", script, true);
    }
    #endregion

    // Example: string balance = 3.00 AUD, 3.11 USD. The function will return 3.00
    private decimal formatMoneyValues(string balance)
    {
        try
        {
            string balance1 = balance.Split(',')[0];
            string balance2 = balance1.Split(' ')[0];
            return decimal.Parse(balance2);
        }
        catch
        {
            return 0;
        }
    }

    protected void lnkbtnClearFilters_Invoices_Click(object sender, EventArgs e)
    {
        foreach (GridColumn column in rgInvoices.MasterTableView.OwnerGrid.Columns)
        {
            column.CurrentFilterFunction = GridKnownFunction.NoFilter;
            column.CurrentFilterValue = string.Empty;
        }
        rgInvoices.MasterTableView.FilterExpression = string.Empty;
        rgInvoices.Rebind();
    }

    protected void lbtnLoadInvoice_Click(object sender, EventArgs e)
    {
        LinkButton lbtnLoadInvoice = (LinkButton)sender;

        ActuateReport report = new ActuateReport("rpt_InstadoseInv");
        report.Arguments.Add("InvoiceNum", lbtnLoadInvoice.Text);

        // Generate the launch script.
        string script = string.Format("window.open(\"{0}\", \"{1}\", \"{2}\");", report.GetReportUri(), "_blank", "menubar=1,resizable=1,scrollbars=1,width=1024,height=600");

        // Launch the report.
        ScriptManager.RegisterClientScriptBlock(Page, typeof(Page), "OpenWindow", script, true);
    }
    /*
    protected void lbtnLoadInvoice_Click(object sender, EventArgs e)
    {
        GridDataItem item = (sender as LinkButton).NamingContainer as GridDataItem;
        LinkButton imgBtn = (LinkButton)item.FindControl("lnkbtnInvoiceNumber");
        string invoiceNo = item.GetDataKeyValue("InvoiceNo").ToString();

        if (invoiceNo == "" || invoiceNo == null) return;

        // Create a file name for the file you want to create.
        string pdfFileName = string.Format("InstadoseInvoice_{0}.pdf", invoiceNo);

        string fullPath = System.IO.Path.Combine(ROOTFOLDERPATH, pdfFileName);

        if (InstadoseInvoicePDFGenerated(invoiceNo, pdfFileName, fullPath))
        {
            // Open/Display Instadose Invoice PDF.
            System.Diagnostics.Process.Start(fullPath);
        }
        else
        {
            // JavaScript Error Message.
            Exception ex = new Exception();
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "Notify", "alert('" + ex.Message + "');", true);
            return;
        }
    }
    */
    /// <summary>
    /// Using the Invoice Number (invoiceNo) from the Link in the RadGrid, 
    /// GENERATE the PDF for the Instadose Invoice and SAVE to the C:/TEMP PDFS/ Folder.
    /// </summary>
    /// <param name="invoiceno"></param>
    /// <returns></returns>
    private bool InstadoseInvoicePDFGenerated(string invoiceno, string pdffilename, string fullpath)
    {
        Stream oStream = null;

        SqlConnectionStringBuilder sqlConnBuilder = new SqlConnectionStringBuilder(ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.MASConnectionString"].ConnectionString);

        string myServer = sqlConnBuilder.DataSource;
        string myDatabase = sqlConnBuilder.InitialCatalog;
        string myUserID = sqlConnBuilder.UserID;
        string myPassword = sqlConnBuilder.Password;

        try
        {
            // Load Crystal Report.
            cryRpt.Load(Server.MapPath("~/InformationFinder/Details/Instadose_Invoice.rpt"));

            // Database Logon/Credentials.
            cryRpt.DataSourceConnections[0].SetConnection(myServer, myDatabase, myUserID, myPassword);
            //cryRpt.SetDatabaseLogon(myUserID, myPassword, myServer, myDatabase);

            // Set Crystal Report Parameter(s). 
            cryRpt.SetParameterValue("@pInvoiceNo", invoiceno);

            // Export to Memory Stream.        
            oStream = cryRpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            byte[] streamArray = new byte[oStream.Length];
            oStream.Read(streamArray, 0, Convert.ToInt32(oStream.Length - 1));

            //Save to TEMP REPORTS Folder.
            if (streamArray.Length > 0)
            {
                if (!Directory.Exists(Path.GetDirectoryName(ROOTFOLDERPATH)))
                    Directory.CreateDirectory(Path.GetDirectoryName(ROOTFOLDERPATH));

                File.WriteAllBytes(fullpath, streamArray);
            }

            return true;
        }
        catch
        {
            return false;
        }
        finally
        {
            cryRpt.Close();
            cryRpt.Dispose();

            oStream.Flush();
            oStream.Close();
            oStream.Dispose();
        }
    }

    /// <summary>
    /// Close Instadose Invoice Dialog/Modal.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnClose_Click(object sender, EventArgs e)
    {
        // Send JavaScript to client to close/display the modal.
        string script = "closeDialog('instadoseInvoiceDialog')";
        ScriptManager.RegisterClientScriptBlock(Page, typeof(Page), "closeModal", script, true);
    }

    protected void lnkbtnInvoiceExport_Click(object sender, EventArgs e)
    {
        Response.Redirect(string.Format("AccountInvoicesExport.aspx?ID={0}", accountID));
    }

    # endregion

    # region LOCATIONS TAB FUNCTIONS
    protected void rgLocations_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
    {
        Int32.TryParse(Request.QueryString["ID"], out accountID);

        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.StoredProcedure;

        // Using the sp_if_GetAccountLocationsByAccountID.
        string sqlQuery = "sp_if_GetAccountLocationsByAccountID";

        sqlCmd.Parameters.Add(new SqlParameter("@pAccountID", accountID));

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable dtLocationsTable = new DataTable();

        dtLocationsTable = new DataTable("Get All Account-Linked Locations");

        // Create the columns for the DataTable.
        dtLocationsTable.Columns.Add("AccountID", Type.GetType("System.Int32"));
        dtLocationsTable.Columns.Add("LocationID", Type.GetType("System.Int32"));
        dtLocationsTable.Columns.Add("LocationName", Type.GetType("System.String"));
        dtLocationsTable.Columns.Add("ShippingAddress", Type.GetType("System.String"));
        dtLocationsTable.Columns.Add("City", Type.GetType("System.String"));
        dtLocationsTable.Columns.Add("StateAbbrev", Type.GetType("System.String"));
        dtLocationsTable.Columns.Add("Active", Type.GetType("System.Boolean"));
        dtLocationsTable.Columns.Add("BillingGroupID", Type.GetType("System.Int32"));

        while (sqlDataReader.Read())
        {
            DataRow drow = dtLocationsTable.NewRow();

            // Fill row details.
            drow["AccountID"] = sqlDataReader["AccountID"];
            drow["LocationID"] = sqlDataReader["LocationID"];
            drow["LocationName"] = sqlDataReader["LocationName"];
            drow["ShippingAddress"] = sqlDataReader["ShippingAddress"];
            drow["City"] = sqlDataReader["City"];
            drow["StateAbbrev"] = sqlDataReader["StateAbbrev"];
            drow["Active"] = sqlDataReader["Active"];
            drow["BillingGroupID"] = sqlDataReader["BillingGroupID"];

            // Add rows to DataTable.
            dtLocationsTable.Rows.Add(drow);
        }

        // Bind the results to the gridview
        rgLocations.DataSource = dtLocationsTable;

        displayActiveInactiveTotalLocationsCount(accountID);

        sqlConn.Close();
        sqlDataReader.Close();
    }

    protected void rgLocations_OnItemCommand(object sender, Telerik.Web.UI.GridCommandEventArgs e)
    {
        rgLocations.Rebind();
    }

    protected void rgLocations_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
    {
        SortOrder(rgLocations, e);
    }

    protected void lnkbtnCreateLocation_Click(object sender, EventArgs e)
    {
        string url = String.Format("EditLocation.aspx?AccountID={0}", accountID);
        Response.Redirect(url);
    }

    private void displayActiveInactiveTotalLocationsCount(int accountid)
    {
        int activeLocationsCount = (from l in idc.Locations
                                    where l.AccountID == accountid && l.Active == true
                                    select l).Count();

        int allLocationsCount = (from l in idc.Locations
                                 where l.AccountID == accountid
                                 select l).Count();

        int inactiveLocationsCount = allLocationsCount - activeLocationsCount;
    }

    protected void lnkbtnClearFilters_Locations_Click(object sender, EventArgs e)
    {
        foreach (GridColumn column in rgLocations.MasterTableView.OwnerGrid.Columns)
        {
            column.CurrentFilterFunction = GridKnownFunction.NoFilter;
            column.CurrentFilterValue = string.Empty;
        }
        rgLocations.MasterTableView.FilterExpression = string.Empty;
        rgLocations.Rebind();
    }
    # endregion

    # region USERS TAB FUNCTIONS
    protected void rgUsers_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
    {
        Int32.TryParse(Request.QueryString["ID"], out accountID);

        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.StoredProcedure;

        // Using the sp_if_GetAccountUsersByAccountID.
        string sqlQuery = "sp_if_GetAccountUsersByAccountID";

        sqlCmd.Parameters.Add(new SqlParameter("@pAccountID", accountID));

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable dtUsersTable = new DataTable();

        dtUsersTable = new DataTable("Get All Account-Linked Users");

        // Create the columns for the DataTable.
        dtUsersTable.Columns.Add("AccountID", Type.GetType("System.Int32"));
        dtUsersTable.Columns.Add("UserID", Type.GetType("System.Int32"));
        dtUsersTable.Columns.Add("UserName", Type.GetType("System.String"));
        dtUsersTable.Columns.Add("UserRoleName", Type.GetType("System.String"));
        dtUsersTable.Columns.Add("FullName", Type.GetType("System.String"));
        dtUsersTable.Columns.Add("Email", Type.GetType("System.String"));
        dtUsersTable.Columns.Add("LocationName", Type.GetType("System.String"));
        dtUsersTable.Columns.Add("Active", Type.GetType("System.Boolean"));

        while (sqlDataReader.Read())
        {
            DataRow drow = dtUsersTable.NewRow();

            // Fill row details.
            drow["AccountID"] = sqlDataReader["AccountID"];
            drow["UserID"] = sqlDataReader["UserID"];
            drow["UserName"] = sqlDataReader["UserName"];
            drow["UserRoleName"] = sqlDataReader["UserRoleName"];
            drow["FullName"] = sqlDataReader["FullName"];
            drow["Email"] = sqlDataReader["Email"];
            drow["LocationName"] = sqlDataReader["LocationName"];
            drow["Active"] = sqlDataReader["Active"];

            // Add rows to DataTable.
            dtUsersTable.Rows.Add(drow);
        }

        // Bind the results to the gridview
        rgUsers.DataSource = dtUsersTable;

        sqlConn.Close();
        sqlDataReader.Close();
    }

    protected void rgUsers_OnItemCommand(object sender, Telerik.Web.UI.GridCommandEventArgs e)
    {
        rgUsers.Rebind();
    }

    protected void rgUsers_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
    {
        SortOrder(rgUsers, e);
    }

    protected void lnkbtnCreateUser_Click(object sender, EventArgs e)
    {
        string url = String.Format("UserMaintenance.aspx?AccountID={0}", accountID);
        Response.Redirect(url);
    }

    private void displayActiveInactiveTotalUsersCount(int accountid)
    {
        int activeUsersCount = (from u in idc.Users
                                where u.AccountID == accountid && u.Active == true
                                select u).Count();

        int allUsersCount = (from u in idc.Users
                             where u.AccountID == accountid
                             select u).Count();

        int inactiveUsersCount = allUsersCount - activeUsersCount;

        //lblTotalActiveInactiveUsers.Text = String.Format("Active: {0:#,###}/{1:#,###} Total: {2:#,###}", activeUsersCount.ToString(), inactiveUsersCount.ToString(), allUsersCount.ToString());
    }

    protected void lnkbtnClearFilters_Users_Click(object sender, EventArgs e)
    {
        foreach (GridColumn column in rgUsers.MasterTableView.OwnerGrid.Columns)
        {
            column.CurrentFilterFunction = GridKnownFunction.NoFilter;
            column.CurrentFilterValue = string.Empty;
        }
        rgUsers.MasterTableView.FilterExpression = string.Empty;
        rgUsers.Rebind();
    }

    protected void lnkbtnUserExport_Click(object sender, EventArgs e)
    {
        Response.Redirect(string.Format("AccountUsersExport.aspx?ID={0}", accountID));
    }
    # endregion

    # region BADGES TAB FUNCTIONS
    protected void rgBadges_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
    {
        Int32.TryParse(Request.QueryString["ID"], out accountID);

        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.StoredProcedure;

        // Using the sp_if_GetAccountUsersByAccountID.
        string sqlQuery = "sp_if_GetAccountBadgesByAccountID";

        sqlCmd.Parameters.Add(new SqlParameter("@pAccountID", accountID));

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable dtBadgesTable = new DataTable();

        dtBadgesTable = new DataTable("Get All Account-Linked Badges");

        // Create the columns for the DataTable.
        dtBadgesTable.Columns.Add("AccountID", Type.GetType("System.Int32"));
        dtBadgesTable.Columns.Add("SerialNumber", Type.GetType("System.String"));
        dtBadgesTable.Columns.Add("UserID", Type.GetType("System.Int32"));
        dtBadgesTable.Columns.Add("FullName", Type.GetType("System.String"));
        dtBadgesTable.Columns.Add("LocationName", Type.GetType("System.String"));
        dtBadgesTable.Columns.Add("BodyRegion", Type.GetType("System.String"));
        dtBadgesTable.Columns.Add("ProductColor", Type.GetType("System.String"));
        dtBadgesTable.Columns.Add("OrderID", Type.GetType("System.String"));
        dtBadgesTable.Columns.Add("ServiceStart", Type.GetType("System.DateTime"));
        dtBadgesTable.Columns.Add("ServiceEnd", Type.GetType("System.DateTime"));
        dtBadgesTable.Columns.Add("Initialized", Type.GetType("System.Int32"));
        dtBadgesTable.Columns.Add("FormattedInitialized", Type.GetType("System.Boolean"));
        dtBadgesTable.Columns.Add("Active", Type.GetType("System.Boolean"));

        bool isInitialized = true;

        while (sqlDataReader.Read())
        {
            DataRow drow = dtBadgesTable.NewRow();

            isInitialized = sqlDataReader["Initialized"].ToString() != "" ? true : false;

            // Fill row details.
            drow["AccountID"] = sqlDataReader["AccountID"];
            drow["SerialNumber"] = sqlDataReader["SerialNumber"];
            drow["UserID"] = sqlDataReader["UserID"];
            drow["FullName"] = sqlDataReader["FullName"];
            drow["LocationName"] = sqlDataReader["LocationName"];
            drow["BodyRegion"] = sqlDataReader["BodyRegion"];
            drow["ProductColor"] = sqlDataReader["ProductColor"];
            drow["OrderID"] = sqlDataReader["OrderID"];
            drow["ServiceStart"] = sqlDataReader["ServiceStart"];
            drow["ServiceEnd"] = sqlDataReader["ServiceEnd"];
            drow["FormattedInitialized"] = isInitialized;
            drow["Active"] = sqlDataReader["Active"];

            // Add rows to DataTable.
            dtBadgesTable.Rows.Add(drow);
        }

        // Bind the results to the gridview
        rgBadges.DataSource = dtBadgesTable;

        sqlConn.Close();
        sqlDataReader.Close();
    }

    protected void rgBadges_OnItemCommand(object sender, Telerik.Web.UI.GridCommandEventArgs e)
    {
        rgBadges.Rebind();
    }

    protected void rgBadges_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
    {
        SortOrder(rgBadges, e);
    }

    private void displayActiveInactiveTotalBadgesCount(int accountid)
    {
        int activeBadgesCount = (from ad in idc.AccountDevices
                                 where ad.AccountID == accountid &&
                                       ad.CurrentDeviceAssign == true &&
                                       ad.Active == true
                                 select ad).Count();

        int allBadgesCount = (from ad in idc.AccountDevices
                              where ad.AccountID == accountid &&
                                    ad.CurrentDeviceAssign == true
                              select ad).Count();

        int inactiveBadgesCount = allBadgesCount - activeBadgesCount;

        //lblTotalActiveInactiveBadges.Text = String.Format("Active: {0:#,###}/{1:#,###} Total: {2:#,###}", activeBadgesCount.ToString(), inactiveBadgesCount.ToString(), allBadgesCount.ToString());
    }

    protected void lnkbtnClearFilters_Badges_Click(object sender, EventArgs e)
    {
        foreach (GridColumn column in rgBadges.MasterTableView.OwnerGrid.Columns)
        {
            column.CurrentFilterFunction = GridKnownFunction.NoFilter;
            column.CurrentFilterValue = string.Empty;
        }
        rgBadges.MasterTableView.FilterExpression = string.Empty;
        rgBadges.Rebind();
    }

    protected void lnkbtnBadgeExport_Click(object sender, EventArgs e)
    {
        Response.Redirect(string.Format("AccountBadgesExport.aspx?ID={0}", accountID));
    }
    # endregion

    # region RETURNS TAB FUNCTIONS
    protected void rgReturns_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
    {
        Int32.TryParse(Request.QueryString["ID"], out accountID);

        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.AppConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandTimeout = 300;
        sqlCmd.CommandType = CommandType.StoredProcedure;

        // Using the sp_if_GetAccountUsersByAccountID.
        string sqlQuery = "sp_if_GetAccountReturnsByAccountID";

        sqlCmd.Parameters.Add(new SqlParameter("@pAccountID", accountID));

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable dtReturnsTable = new DataTable();

        dtReturnsTable = new DataTable("Get All Account-Linked Returns");

        // Create the columns for the DataTable.
        dtReturnsTable.Columns.Add("ReturnID", Type.GetType("System.Int32"));
        dtReturnsTable.Columns.Add("Status", Type.GetType("System.String"));
        dtReturnsTable.Columns.Add("CreatedDate", Type.GetType("System.DateTime"));
        dtReturnsTable.Columns.Add("AccountID", Type.GetType("System.Int32"));
        dtReturnsTable.Columns.Add("Reason", Type.GetType("System.String"));
        dtReturnsTable.Columns.Add("Type", Type.GetType("System.String"));
        dtReturnsTable.Columns.Add("DeviceCount", Type.GetType("System.Int32"));
        dtReturnsTable.Columns.Add("MYSerialNoString", Type.GetType("System.String"));
        dtReturnsTable.Columns.Add("DisplayClipButton", Type.GetType("System.Boolean"));
        dtReturnsTable.Columns.Add("DisplayPdf", Type.GetType("System.Boolean"));

        while (sqlDataReader.Read())
        {
            DataRow drow = dtReturnsTable.NewRow();

            // Fill row details.
            drow["ReturnID"] = sqlDataReader["ReturnID"];
            drow["Status"] = sqlDataReader["Status"];
            drow["CreatedDate"] = sqlDataReader["CreatedDate"];
            drow["AccountID"] = sqlDataReader["AccountID"];
            drow["Reason"] = sqlDataReader["Reason"];
            drow["Type"] = sqlDataReader["Type"];
            drow["DeviceCount"] = sqlDataReader["DeviceCount"];
            drow["MYSerialNoString"] = sqlDataReader["MYSerialNoString"];
            drow["DisplayClipButton"] = Convert.ToBoolean(sqlDataReader["UploadFileCount"]);
            drow["DisplayPdf"] = Convert.ToBoolean(sqlDataReader["DeviceCount"]);

            // Add rows to DataTable.
            dtReturnsTable.Rows.Add(drow);
        }

        // Bind the results to the gridview
        rgReturns.DataSource = dtReturnsTable;

        sqlConn.Close();
        sqlDataReader.Close();
    }

    protected void rgReturns_OnItemCommand(object sender, Telerik.Web.UI.GridCommandEventArgs e)
    {
        rgReturns.Rebind();
    }

    protected void rgReturns_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
    {
        SortOrder(rgReturns, e);
    }

    protected void lnkbtnCreateReturn_Click(object sender, EventArgs e)
    {
        string url = String.Format("../Compose/Return.aspx?ID={0}", accountID);
        Response.Redirect(url);
    }

    protected void lnkbtnClearFilters_Returns_Click(object sender, EventArgs e)
    {
        foreach (GridColumn column in rgReturns.MasterTableView.OwnerGrid.Columns)
        {
            column.CurrentFilterFunction = GridKnownFunction.NoFilter;
            column.CurrentFilterValue = string.Empty;
        }
        rgReturns.MasterTableView.FilterExpression = string.Empty;
        rgReturns.Rebind();
    }

    /// <summary>
    /// RETURNS, to display or not display the Clip graphics. 
    /// if uploadFileCount =="0"  then NOT display clip else display
    /// </summary>
    /// <param name="uploadFileCount"></param>
    /// <returns> Boolean True/False</returns>
    public bool DisplayClipButton(string uploadFileCount)
    {
        Boolean isDisplay = false;
        if (uploadFileCount != "0")
        {
            isDisplay = true;
        }
        return isDisplay;
    }

    /// <summary>
    /// RETURNS, Display max 5 records of Serial No
    /// put "..." at the end of string if more than 5 serial# 
    /// </summary>
    /// <param name="SerialNoString"></param>
    /// <returns> Serial# String </returns>
    public string FuncTrimSerialNo(string SerialNoString)
    {
        string myReturn = SerialNoString;
        if (SerialNoString.LastIndexOf(",") >= 25)
        {
            int Endplace = SerialNoString.IndexOf(",", 20);
            myReturn = SerialNoString.Substring(0, Endplace) + "...";
        }
        return myReturn;
    }
    # endregion

    # region READS TAB FUNCTIONS
    protected void rgReads_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
    {
        Int32.TryParse(Request.QueryString["ID"], out accountID);

        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.StoredProcedure;

        // Using the sp_if_GetAccountReadsByAccountID.
        string sqlQuery = "sp_if_GetAccountReadsByAccountID";

        sqlCmd.Parameters.Add(new SqlParameter("@pAccountID", accountID));

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable dtReadsTable = new DataTable();

        dtReadsTable = new DataTable("Get All Account-Linked Reads");

        // Create the columns for the DataTable.
        dtReadsTable.Columns.Add("AccountID", Type.GetType("System.Int32"));
        dtReadsTable.Columns.Add("ExposureDate", Type.GetType("System.DateTime"));
        dtReadsTable.Columns.Add("TimeZoneDesc", Type.GetType("System.String"));
        dtReadsTable.Columns.Add("CompleteExposureDate", Type.GetType("System.DateTime"));
        dtReadsTable.Columns.Add("Username", Type.GetType("System.String"));
        dtReadsTable.Columns.Add("FullName", Type.GetType("System.String"));
        dtReadsTable.Columns.Add("SerialNumber", Type.GetType("System.String"));
        dtReadsTable.Columns.Add("BodyRegion", Type.GetType("System.String"));
        dtReadsTable.Columns.Add("Deep", Type.GetType("System.Decimal"));
        dtReadsTable.Columns.Add("Shallow", Type.GetType("System.Decimal"));
        dtReadsTable.Columns.Add("Eye", Type.GetType("System.Decimal"));
        dtReadsTable.Columns.Add("UOMLocation", Type.GetType("System.String"));
        dtReadsTable.Columns.Add("Anomaly", Type.GetType("System.Boolean"));

        while (sqlDataReader.Read())
        {
            DataRow drow = dtReadsTable.NewRow();

            // Fill row details.
            drow["AccountID"] = sqlDataReader["AccountID"];
            drow["ExposureDate"] = sqlDataReader["ExposureDate"];
            drow["TimeZoneDesc"] = sqlDataReader["TimeZoneDesc"];
            drow["CompleteExposureDate"] = sqlDataReader["CompleteExposureDate"];
            drow["Username"] = sqlDataReader["Username"];
            drow["FullName"] = sqlDataReader["FullName"];
            drow["SerialNumber"] = sqlDataReader["SerialNumber"];
            drow["BodyRegion"] = sqlDataReader["BodyRegion"];
            drow["Deep"] = sqlDataReader["Deep"];
            drow["Shallow"] = sqlDataReader["Shallow"];
            drow["Eye"] = sqlDataReader["Eye"];
            drow["UOMLocation"] = sqlDataReader["UOMLocation"];
            drow["Anomaly"] = sqlDataReader["Anomaly"];

            // Add rows to DataTable.
            dtReadsTable.Rows.Add(drow);
        }

        // Bind the results to the gridview
        rgReads.DataSource = dtReadsTable;

        sqlConn.Close();
        sqlDataReader.Close();
    }

    protected void rgReads_OnItemCommand(object sender, Telerik.Web.UI.GridCommandEventArgs e)
    {
        rgReads.Rebind();
    }

    protected void rgReads_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
    {
        SortOrder(rgReads, e);
    }

    protected void lnkbtnClearFilters_Reads_Click(object sender, EventArgs e)
    {
        foreach (GridColumn column in rgReads.MasterTableView.OwnerGrid.Columns)
        {
            column.CurrentFilterFunction = GridKnownFunction.NoFilter;
            column.CurrentFilterValue = string.Empty;
        }
        rgReads.MasterTableView.FilterExpression = string.Empty;
        rgReads.Rebind();
    }

    protected void lnkbtnReadExport_Click(object sender, EventArgs e)
    {
        Response.Redirect(string.Format("AccountReadsExport.aspx?ID={0}", accountID));
    }
    # endregion

    # region DOCUMENTS TAB FUNCTIONS
    protected void rgDocuments_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
    {
        Int32.TryParse(Request.QueryString["ID"], out accountID);

        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.StoredProcedure;

        // Using the sp_if_GetAccountDocumentsByAccountID.
        string sqlQuery = "sp_if_GetAccountDocumentsByAccountID";

        sqlCmd.Parameters.Add(new SqlParameter("@pAccountID", accountID));

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable dtDocumentsTable = new DataTable();

        dtDocumentsTable = new DataTable("Get All Account-Linked Documents");

        // Create the columns for the DataTable.
        dtDocumentsTable.Columns.Add("DocumentID", Type.GetType("System.Int32"));
        dtDocumentsTable.Columns.Add("DocumentGUID", Type.GetType("System.Guid"));
        dtDocumentsTable.Columns.Add("DocumentCategory", Type.GetType("System.String"));
        dtDocumentsTable.Columns.Add("DocumentDescription", Type.GetType("System.String"));
        dtDocumentsTable.Columns.Add("AccountID", Type.GetType("System.Int32"));
        dtDocumentsTable.Columns.Add("OrderID", Type.GetType("System.Int32"));
        dtDocumentsTable.Columns.Add("CreatedBy", Type.GetType("System.String"));
        dtDocumentsTable.Columns.Add("CreatedDate", Type.GetType("System.DateTime"));

        while (sqlDataReader.Read())
        {
            DataRow drow = dtDocumentsTable.NewRow();

            // Fill row details.
            drow["DocumentID"] = sqlDataReader["DocumentID"];
            drow["DocumentGUID"] = sqlDataReader["DocumentGUID"];
            drow["DocumentCategory"] = sqlDataReader["DocumentCategory"];
            drow["DocumentDescription"] = sqlDataReader["DocumentDescription"];
            drow["AccountID"] = sqlDataReader["AccountID"];
            drow["OrderID"] = sqlDataReader["OrderID"];
            drow["CreatedBy"] = sqlDataReader["CreatedBy"];
            drow["CreatedDate"] = sqlDataReader["CreatedDate"];

            // Add rows to DataTable.
            dtDocumentsTable.Rows.Add(drow);
        }

        // Bind the results to the gridview
        rgDocuments.DataSource = dtDocumentsTable;

        sqlConn.Close();
        sqlDataReader.Close();
    }

    protected void rgDocuments_OnItemCommand(object sender, Telerik.Web.UI.GridCommandEventArgs e)
    {
        rgDocuments.Rebind();
    }

    protected void rgDocuments_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
    {
        SortOrder(rgDocuments, e);
    }

    // When Acrobat Icon is clicked within GridView (related to DocumentID (or ReferenceKey if Payment Receipt)).
    protected void rgDocuments_OnItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            GridDataItem item = (GridDataItem)e.Item;

            // Open Document Function/Code.
            ImageButton imgbtn = (ImageButton)item.FindControl("imgbtnOpenDocument");

            string imgbtnCommandName = imgbtn.CommandName.ToString();

            // Get DocumentGUID and ReferenceKey from Command Argument selected.
            string cmdArgs = imgbtn.CommandArgument;

            string documentGUID = cmdArgs;

            if (imgbtnCommandName != "OpenDocument") return;
            if (documentGUID == null) return;

            string url = "../../ViewDocuments.aspx?GUID=" + documentGUID;

            // On ImageButton_Click, DISPLAY the Document File.
            imgbtn.Attributes.Add("onclick", "openNewWindow('" + url + "'); return false;");
        }
    }

    protected void lnkbtnClearFilters_Documents_Click(object sender, EventArgs e)
    {
        foreach (GridColumn column in rgDocuments.MasterTableView.OwnerGrid.Columns)
        {
            column.CurrentFilterFunction = GridKnownFunction.NoFilter;
            column.CurrentFilterValue = string.Empty;
        }
        rgDocuments.MasterTableView.FilterExpression = string.Empty;
        rgDocuments.Rebind();
    }

    // Updated On:  11/18/2015
    // Updated By:  ANURADHA NANDI
    // Description: Per request from Customer Service, add Document Upload/Add functionality to mirror what is currently in CS Web Suite.
    #region ADD DOCUMENTS FUNCTIONS.
    protected void lnkbtnAddDocument_Click(object sender, EventArgs e)
    {
        // Reset Modal/Dialog controls.
        this.InvisibleAddDocumentErrors();
        this.ddlDocumentTypes.SelectedIndex = 0;
        this.rcbOrderNumbers.Text = null;
        this.rcbOrderNumbers.Items.Clear();
        this.flupldAddDocument.Attributes.Clear();
        this.txtDocumentDescription.Text = string.Empty;
        this.divOrderNumber.Visible = false;
        this.rcbOrderNumbers.Enabled = false;
        this.reqfldvalRCBOrderNumbers.Enabled = false;

        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('addDocumentDialog')", true);
    }

    protected void lnkbtnDeleteDocument_Click(object sender, EventArgs e)
    {
        LinkButton lnkbtn = sender as LinkButton;

        int documentID = 0;
        int.TryParse(lnkbtn.CommandArgument.ToString(), out documentID);

        var documentRecord = (from d in idc.Documents where d.DocumentID == documentID select d).FirstOrDefault();

        if (documentRecord == null) return;

        // Deactive the Document, but in case of emergency/CS error, we can Reactivate it.
        documentRecord.Active = false;

        try
        {
            idc.SubmitChanges();

            // RELOAD the Documents RadGrid.
            this.rgDocuments.DataSource = null;
            this.rgDocuments.Rebind();

            // Update Documents Tab count.
            //GetRecordCounts(accountID);
        }
        catch
        {
            return;
        }
    }

    public static int? GetDocumentOrderID(string orderid)
    {
        int i;
        if (int.TryParse(orderid, out i)) return i;
        return null;
    }

    protected void ddlDocumentTypes_SelectedIndexChanged(object sender, EventArgs e)
    {
        // Clear RadComboBox Values/Items.
        this.rcbOrderNumbers.Text = null;
        this.rcbOrderNumbers.Items.Clear();

        string valueSelected = this.ddlDocumentTypes.SelectedValue;

        if (valueSelected == "Order Acknowledgements")
        {
            this.divOrderNumber.Visible = true;
            this.rcbOrderNumbers.Enabled = true;
            this.reqfldvalRCBOrderNumbers.Enabled = true;
        }
        else
        {
            this.divOrderNumber.Visible = false;
            this.rcbOrderNumbers.Enabled = false;
            this.reqfldvalRCBOrderNumbers.Enabled = false;
        }
    }

    protected void btnAddDocument_Click(object sender, EventArgs e)
    {
        // Optional field for Customer Service, so they can associate Document to a specific OrderID.
        int? documentOrderID = null;

        bool isOrderIDValid = true;

        if (this.ddlDocumentTypes.SelectedValue == "Order Acknowledgements")
        {
            // Get OrderID from rcbOrderNumbers RadComboBox.
            documentOrderID = GetDocumentOrderID(this.rcbOrderNumbers.Text);

            // IF the Value is NOT NULL, then validate OrderID in Database and return TRUE/FALSE.
            if (documentOrderID.Value != null)
            {
                var orderRecord = (from ord in idc.Orders
                                   where ord.AccountID == accountID
                                   && ord.OrderID == documentOrderID.Value
                                   select ord).FirstOrDefault();

                if (orderRecord == null) isOrderIDValid = false;
                else isOrderIDValid = true;
            }
        }
        else
        {
            isOrderIDValid = true;
        }

        if (isOrderIDValid == true)
        {
            HttpPostedFile hpf = flupldAddDocument.PostedFile;
            string attachmentFile = Path.GetFileName(hpf.FileName);

            // CREATE the Byte Array.
            Byte[] byteImage = new Byte[hpf.ContentLength];

            // READ uploaded file to Byte Array.
            hpf.InputStream.Read(byteImage, 0, hpf.ContentLength);

            string fileExtension = attachmentFile.Split('.')[1];
            string mimeType = null;

            switch (fileExtension)
            {
                case "DOC":
                    mimeType = "application/vnd.ms-word";
                    break;
                case "DOCX":
                    mimeType = "application/vnd.ms-word";
                    break;
                case "XLS":
                    mimeType = "application/vnd.ms-excel";
                    break;
                case "XLSX":
                    mimeType = "application/vnd.ms-excel";
                    break;
                case "PDF":
                    mimeType = "application/pdf";
                    break;
                default:
                    mimeType = "image/jpeg";
                    break;
            }

            // Optional field for Customer Service, so they can associate Document to a specific OrderID.
            //int? documentOrderID = GetDocumentOrderID(this.rcbOrderNumbers.Text);

            Document doc = new Document()
            {
                DocumentGUID = Guid.NewGuid(),
                FileName = Path.GetFileName(hpf.FileName),
                DocumentContent = byteImage,
                MIMEType = mimeType,
                DocumentCategory = this.ddlDocumentTypes.SelectedValue,
                Description = string.Format("{0}", txtDocumentDescription.Text.Trim()),
                AccountID = this.accountID,
                OrderID = documentOrderID,
                CreatedBy = this.UserName,
                CreatedDate = DateTime.Now,
                Active = true
            };

            try
            {
                // SAVE the Document Record.
                idc.Documents.InsertOnSubmit(doc);
                idc.SubmitChanges();

                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('addDocumentDialog')", true);

                // RELOAD the Documents RadGrid.
                this.rgDocuments.DataSource = null;
                this.rgDocuments.Rebind();

                // Update Documents Tab count.
                //GetRecordCounts(accountID);
            }
            catch (Exception ex)
            {
                // DISPLAY Error Message on/from Server.
                VisibleAddDocumentErrors(ex.Message);
                return;
            }
        }
        else
        {
            // Display Error (OrderID is INVALID).
            // The User will have to re-enter all the Document Upload information since the File Upload Object does not maintain its value after PostBack.
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "alert('Invalid Order #. Please verify Order # and try again.')", true);
            return;
        }
    }

    private void InvisibleAddDocumentErrors()
    {
        divAddDocumentError.Visible = false;
        spnAddDocumentErrorMessage.InnerHtml = string.Empty;
    }

    private void VisibleAddDocumentErrors(string error)
    {
        divAddDocumentError.Visible = true;
        spnAddDocumentErrorMessage.InnerText = error;
    }
    # endregion
    # endregion

    #region CALENDARS TAB FUNCTIONS

    protected void rgCalendars_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
    {
        SortOrder(rgCalendars, e);
    }

    protected void rgCalendars_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
    {
        Int32.TryParse(Request.QueryString["ID"], out accountID);

        DACalendars calendar = new DACalendars();

        Instadose.API.CalendarList list = calendar.List(accountID, null, null, null);

        rgCalendars.DataSource = list.Calendars;
    }

    protected void rgCalendars_ItemDataBound(object sender, GridItemEventArgs e)
    {
        if (e.Item is GridDataItem)
        {
            var item = e.Item as GridDataItem;
            var calendar = item.DataItem as Instadose.API.CalendarListItem;

            if (calendar != null)
            {
                HyperLink hlOpenCalendarDetail = item.FindControl("hlOpenCalendarDetail") as HyperLink;
                Literal ltCalendarFrequency = item.FindControl("ltCalendarFrequency") as Literal;

                hlOpenCalendarDetail.Text = calendar.Name;
                hlOpenCalendarDetail.NavigateUrl = string.Format("javascript:ManageCalendar.openCalendarDatesDialog({0});", calendar.ID);

                ltCalendarFrequency.Text = GetCalendarFrequencyText(calendar.Frequency);
            }
        }
    }

    protected void lnkbtnClearFilters_Calendars_Click(object sender, EventArgs e)
    {
        foreach (GridColumn column in rgCalendars.MasterTableView.OwnerGrid.Columns)
        {
            column.CurrentFilterFunction = GridKnownFunction.NoFilter;
            column.CurrentFilterValue = string.Empty;
        }

        rgCalendars.MasterTableView.FilterExpression = string.Empty;
        rgCalendars.Rebind();
    }

    private string GetCalendarFrequencyText(Instadose.API.CalendarFrequency frequency)
    {
        string txt = string.Empty;

        switch (frequency)
        {
            case Instadose.API.CalendarFrequency.None:
                txt = "None";
                break;
            case Instadose.API.CalendarFrequency.Weekly:
                txt = "Weekly";
                break;
            case Instadose.API.CalendarFrequency.Biweekly:
                txt = "Every 2 Weeks";
                break;
            case Instadose.API.CalendarFrequency.Monthly:
                txt = "Monthly";
                break;
            case Instadose.API.CalendarFrequency.Quarterly:
                txt = "Quarterly";
                break;
            case Instadose.API.CalendarFrequency.TwoMonthly:
                txt = "Every 2 Months";
                break;
            default:
                txt = "None";
                break;
        }

        return txt;
    }

    private bool HasInstadosePlusDevices(int accountID)
    {
        var idc = new InsDataContext();

        return idc.AccountDevices.Any(ad => ad.DeviceInventory.Product.ProductGroupID == 10 && ad.AccountID == accountID);
    }

    #endregion

    #region BILLING GROUPS TAB FUNCTIONS
    protected void rgBillingGroups_NeedDataSource(object sender, GridNeedDataSourceEventArgs e)
    {
        Int32.TryParse(Request.QueryString["ID"], out accountID);

        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.StoredProcedure;

        // Using the sp_if_GetAccountLocationsByAccountID.
        string sqlQuery = "sp_if_GetAccountBillingGroupsByAccountID";

        sqlCmd.Parameters.Add(new SqlParameter("@pAccountID", accountID));

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable dtBillingGroupsTable = new DataTable();

        dtBillingGroupsTable = new DataTable("Get All Account-Linked Billing Groups");

        // Create the columns for the DataTable.
        dtBillingGroupsTable.Columns.Add("BillingGroupID", Type.GetType("System.Int32"));
        dtBillingGroupsTable.Columns.Add("AccountID", Type.GetType("System.Int32"));
        dtBillingGroupsTable.Columns.Add("ContactName", Type.GetType("System.String"));
        dtBillingGroupsTable.Columns.Add("CompanyName", Type.GetType("System.String"));        
        dtBillingGroupsTable.Columns.Add("useMail", Type.GetType("System.Boolean"));
        dtBillingGroupsTable.Columns.Add("useEmail", Type.GetType("System.Boolean"));
        dtBillingGroupsTable.Columns.Add("useFax", Type.GetType("System.Boolean"));
        dtBillingGroupsTable.Columns.Add("useEDI", Type.GetType("System.Boolean"));
        dtBillingGroupsTable.Columns.Add("useSpecialDelivery", Type.GetType("System.Boolean"));        
        dtBillingGroupsTable.Columns.Add("DeliverInvoice", Type.GetType("System.Boolean"));
        dtBillingGroupsTable.Columns.Add("IsAccountLevelBillingGroup", Type.GetType("System.Boolean"));

        while (sqlDataReader.Read())
        {
            DataRow drow = dtBillingGroupsTable.NewRow();

            // Fill row details.
            drow["BillingGroupID"] = sqlDataReader["BillingGroupID"];
            drow["AccountID"] = sqlDataReader["AccountID"];
            drow["ContactName"] = sqlDataReader["ContactName"];
            drow["CompanyName"] = sqlDataReader["CompanyName"];            
            drow["useMail"] = sqlDataReader["useMail"];
            drow["useEmail"] = sqlDataReader["useEmail"];
            drow["useFax"] = sqlDataReader["useFax"];
            drow["useEDI"] = sqlDataReader["useEDI"];
            drow["useSpecialDelivery"] = sqlDataReader["useSpecialDelivery"];            
            drow["DeliverInvoice"] = sqlDataReader["DeliverInvoice"];
            drow["IsAccountLevelBillingGroup"] = sqlDataReader["IsAccountLevelBillingGroup"];

            // Add rows to DataTable.
            dtBillingGroupsTable.Rows.Add(drow);
        }

        // Bind the results to the gridview
        rgBillingGroups.DataSource = dtBillingGroupsTable;        

        sqlConn.Close();
        sqlDataReader.Close();
    }

    protected void rgBillingGroups_OnItemCommand(object sender, Telerik.Web.UI.GridCommandEventArgs e)
    {
        rgBillingGroups.Rebind();
    }

    protected void rgBillingGroups_SortCommand(object sender, Telerik.Web.UI.GridSortCommandEventArgs e)
    {
        SortOrder(rgBillingGroups, e);
    }

    protected void lnkbtnCreateBillingGroup_Click(object sender, EventArgs e)
    {
        string url = String.Format("EditBillingGroup.aspx?AccountID={0}", accountID);
        Response.Redirect(url);
    }

    protected void lnkbtnClearFilters_BillingGroups_Click(object sender, EventArgs e)
    {
        foreach (GridColumn column in rgBillingGroups.MasterTableView.OwnerGrid.Columns)
        {
            column.CurrentFilterFunction = GridKnownFunction.NoFilter;
            column.CurrentFilterValue = string.Empty;
        }
        rgBillingGroups.MasterTableView.FilterExpression = string.Empty;
        rgBillingGroups.Rebind();
    }

    # endregion




    // Sends User back to Account Details page, specifically on the Device Tab.
    protected void btnBackToSearchPage_Click(object sender, EventArgs e)
    {
        Response.Redirect("../Default.aspx");
        return;
    }

    /// <summary>
    /// Do not display if badge count = 0
    /// </summary>
    /// <param name="pNumberofBadgeReturned"></param>
    /// <returns></returns>
    public bool DisplayPdf(string pNumberofBadgeReturned)
    {
        Boolean isDisplay = true;
        if (int.Parse(pNumberofBadgeReturned) == 0)
        {
            isDisplay = false;
        }
        return isDisplay;
    }


    // ****************  PRINT INVOICE ***********************//
    #region PRINT INVOICE

    public bool IsDisplayReceipt(string pPaymentMethod, string orderTotal)
    {
        Boolean isDisplay = false;
        if (pPaymentMethod == "CC" && float.Parse(orderTotal) > 0)
        {
            isDisplay = true;
        }
        return isDisplay;
    }

    // Generate Order receipt.
    protected void imgBtnOrderReceipt_OnClick(object sender, EventArgs e)
    {
        GridDataItem item = (sender as ImageButton).NamingContainer as GridDataItem;
        ImageButton imgBtn = (ImageButton)item.FindControl("imgBtnOrderReceipt");
        string orderID = item.GetDataKeyValue("OrderID").ToString();

        if (imgBtn.CommandName != "GenerateReceipt") return;
        if (orderID == "" || orderID == null) return;

        PrintReceipt(orderID);
    }

    private void PrintReceipt(string pOrderID)
    {
        try
        {
            DataTable paymentDT = new DataTable();

            paymentDT = GetPaymentInfo(pOrderID);

            if (paymentDT != null && paymentDT.Rows.Count > 0)
            {
                foreach (DataRow row in paymentDT.Rows)
                {
                    int paymentID = int.Parse(row["PaymentID"].ToString().Trim());

                    // Look for existing Document.
                    Document doc = (from d in idc.Documents where d.ReferenceKey == paymentID orderby d.CreatedDate descending select d).FirstOrDefault();

                    if (doc != null)
                    {
                        // Open ViewDocument to display receipt, do not delete the existing document
                        string url = string.Format("/Finance/DisplayReceipt.aspx?GUID={0}&DocDelete={1}", doc.DocumentGUID, "false");
                        ScriptManager.RegisterStartupScript(this, typeof(string), DateTime.Now.Ticks.ToString(), "window.open( '" + url + "');", true);
                    }
                    else
                    {
                        // Generate receipt document and then delete it after displaying it.
                        GenerateAndDisplayReceipt(paymentID);
                    }
                }
            }
            else
            {
                string script = string.Format("alert('{0}');", "Missing credit card payment info or the payment has not been processed.");
                ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "AlertBox", script, true);
                return;
            }
        }
        catch (Exception ex)
        {
            string script = string.Format("alert('{0}');", ex.ToString());
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "AlertBox", script, true);
        }
    }

    private DataTable GetPaymentInfo(string pOrderID)
    {
        DataTable dt = new DataTable();

        try
        {
            String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            String cmdStr = "dbo.sp_GetPaymentInfoByOrderID";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;

            sqlCmd.Parameters.Add("@OrderID", SqlDbType.Int);
            sqlCmd.Parameters["@OrderID"].Value = int.Parse(pOrderID);

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);


            sqlDA.Fill(dt);

            sqlConn.Close();
        }
        catch (Exception ex)
        {
            throw ex;
        }

        return dt;
    }

    private void GenerateAndDisplayReceipt(int pPaymentID)
    {
        try
        {
            // Get AccountID based on PaymentID (Query String value).
            var payment = (from p in idc.Payments
                           where p.PaymentID == pPaymentID
                           select new
                           {
                               AccountID = p.Order.AccountID,
                               OrderID = p.OrderID,
                               CompanyName = p.Order.Account.CompanyName,
                               EncCreditCardNum = p.AccountCreditCard.NumberEncrypted,
                               CardType = p.AccountCreditCard.CreditCardType.CreditCardName
                           }).FirstOrDefault();

            if (payment.EncCreditCardNum == null || payment.CardType == null)
                throw new Exception("Payment is not a credit card payment type.");

            // Mask the credit card number.
            string maskedCardNum = Common.MaskCreditCardNumber(payment.EncCreditCardNum, payment.CardType);

            if (generatePDFDocument(pPaymentID, maskedCardNum, payment.AccountID, (payment.OrderID.HasValue) ? payment.OrderID.Value : 0, payment.CompanyName))
            {
                // Look for existing Document.
                Document doc = (from d in idc.Documents where d.ReferenceKey == pPaymentID select d).FirstOrDefault();

                // Open ViewDocument to display receipt
                string url = string.Format("/Finance/DisplayReceipt.aspx?GUID={0}&DocDelete={1}", doc.DocumentGUID, "true");
                ScriptManager.RegisterStartupScript(this, typeof(string), DateTime.Now.Ticks.ToString(), "window.open( '" + url + "');", true);

                //string url = string.Format("PaymentReceipt.aspx?ID={0}", pPaymentID);
                //ScriptManager.RegisterStartupScript(this, typeof(string), "OPEN_WINDOW", "window.open( '" + url + "');", true);                   
            }
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    private bool generatePDFDocument(int paymentID, string maskedCreditCardNumber, int accountID, int orderID, string companyName)
    {
        string pdfFileName = "PaymentReceipt_" + paymentID.ToString() + ".pdf";
        string myCRFileNamePath = Server.MapPath("PaymentReceipt.rpt");
        myCRFileNamePath = myCRFileNamePath.Replace("InformationFinder\\Details", "Finance");

        Stream oStream = null;
        ReportDocument cryRpt = new ReportDocument();
        bool success = false;

        // Get UserName (based on System Login).
        string userName = "Unknown";

        if (Page.User.Identity.Name.Split('\\')[1] != null)
            userName = Page.User.Identity.Name.Split('\\')[1];

        try
        {
            // First insert Document record with default DocumentContent into database.
            // then PaymentReceipt report will use this doc record to generate DocumentContent.
            // This work-around solution is adapting for both Instadose and Unix CC processing.

            Document dcmntrcrd = new Document()
            {
                DocumentGUID = Guid.NewGuid(),
                FileName = pdfFileName,
                DocumentContent = Enumerable.Repeat((byte)0x20, 7).ToArray(),
                MIMEType = "application/pdf",
                DocumentCategory = "Payment Receipt",
                Description = "Credit Card Payment Receipt",
                AccountID = accountID,
                OrderID = orderID,
                CreatedBy = userName,
                CreatedDate = DateTime.Now,
                Active = true,
                ReferenceKey = paymentID,
                CompanyName = companyName,
                GDSAccount = null
            };

            // Submit record to database.
            idc.Documents.InsertOnSubmit(dcmntrcrd);
            idc.SubmitChanges();

            // Get new PaymentID from Inserted record and generate payment receipt
            int docID = dcmntrcrd.DocumentID;
            if (docID <= 0) return false;

            cryRpt.Load(myCRFileNamePath);

            cryRpt.SetParameterValue("@pPaymentID", paymentID);
            cryRpt.SetParameterValue("@pMaskedCreditCardNumber", maskedCreditCardNumber);

            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString.ToString();
            string myDatabase = Regex.Match(connStr, "(Initial Catalog=|Database=){1}([A-Za-z0-9_]+)[;]?").Groups[2].ToString();
            string myServer = Regex.Match(connStr, "(Data Source=){1}([\\w\\d\\.\\$\\-]+)[;]?").Groups[2].ToString();
            string myUserID = Regex.Match(connStr, "(User ID=|UID=){1}([\\w\\d\\.]+)[;]?").Groups[2].ToString();
            string myPassword = Regex.Match(connStr, "(Pwd=|Password=|PWD=){1}([\\w\\d\\.\\$]+)[;]?").Groups[2].ToString();

            cryRpt.DataSourceConnections[0].SetConnection(myServer, myDatabase, myUserID, myPassword);

            // Export to Memory Stream.        
            oStream = cryRpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            byte[] streamArray = new byte[oStream.Length];
            oStream.Read(streamArray, 0, Convert.ToInt32(oStream.Length - 1));

            // Update inserted doc with real DocumentContent
            // Look for existing Document.
            Document doc = (from d in idc.Documents where d.DocumentID == docID select d).FirstOrDefault();

            if (doc != null)
            {
                doc.DocumentContent = streamArray;
                // Save changes.
                idc.SubmitChanges();

                success = true;

                // Report the error to the message system.
                Basics.WriteLogEntry("PaymentReceipt was generated successful.", Page.User.Identity.Name, "Portal.InstaDose_Finance_PaymentReceipt", Basics.MessageLogType.Information);
            }
            else
            {
                success = false;

                // Report the error to the message system.
                Basics.WriteLogEntry("PaymentReceipt was not generated successful.", Page.User.Identity.Name, "Portal.InstaDose_Finance_PaymentReceipt", Basics.MessageLogType.Critical);
            }
        }
        catch (Exception ex)
        {
            // Report the error to the message system.
            Basics.WriteLogEntry(ex.Message, Page.User.Identity.Name, "Portal.InstaDose_Finance_PaymentReceipt", Basics.MessageLogType.Critical);
            success = false;

            throw ex;
        }
        finally
        {
            cryRpt.Close();
            cryRpt.Dispose();

            oStream.Flush();
            oStream.Close();
            oStream.Dispose();
        }

        return success;
    }

    # endregion
    // ****************  PRINT INVOICE ***********************//

    private int GetScheduleCounts()
    {
        if (accountID <= 0)
        {
            return 0;
        }

        return idc.AccountSchedules.Count(asc => asc.AccountID == accountID);
    }

    private void LoadProratePeriodRadioButton()
    {
        DateTime orderCreatedDate = DateTime.Now;
        DateTime startQuarterDate, nextQuarterDate;
        int qtrNo = Common.calculateNumberOfQuarterService(GetContractStartDate(), GetContractEndDate(), orderCreatedDate, out startQuarterDate);
        nextQuarterDate = startQuarterDate.AddMonths(3);

        if (account.BillingTermID == 2) // yearly
        {
            radProratePeriod.Items.Add(new ListItem(string.Format("{0:MM/dd/yyyy}", startQuarterDate) + " - " + string.Format("{0:MM/dd/yyyy}", GetContractEndDate()), startQuarterDate.ToShortDateString()));
            if (nextQuarterDate < GetContractEndDate())
            {
                radProratePeriod.Items.Add(new ListItem(string.Format("{0:MM/dd/yyyy}", nextQuarterDate) + " - " + string.Format("{0:MM/dd/yyyy}", GetContractEndDate()), nextQuarterDate.ToShortDateString()));
                radProratePeriod.Items[1].Enabled = false;
            }
        }
        else // quarterly
        {
            radProratePeriod.Items.Add(new ListItem(string.Format("{0:MM/dd/yyyy}", startQuarterDate) + " - " + string.Format("{0:MM/dd/yyyy}", startQuarterDate.AddMonths(3).AddDays(-1)), startQuarterDate.ToShortDateString()));
            if (nextQuarterDate < GetContractEndDate())
            {
                radProratePeriod.Items.Add(new ListItem(string.Format("{0:MM/dd/yyyy}", nextQuarterDate) + " - " + string.Format("{0:MM/dd/yyyy}", nextQuarterDate.AddMonths(3).AddDays(-1)), nextQuarterDate.ToShortDateString()));
                radProratePeriod.Items[1].Enabled = false;
            }
        }

        if (nextQuarterDate < GetContractEndDate() && nextQuarterDate.Subtract(orderCreatedDate).Days < 30)
        {
            radProratePeriod.Items[1].Enabled = true;
        }

        radProratePeriod.SelectedIndex = 0;

        //if (order != null)
        //{
        //    ListItem item = radProratePeriod.Items.FindByValue(order.StxContractStartDate.HasValue ? order.StxContractStartDate.Value.ToShortDateString() : DateTime.Now.ToShortDateString());
        //    if (item != null)
        //        radProratePeriod.SelectedIndex = radProratePeriod.Items.IndexOf(item);
        //}
    }

    private DateTime? GetEarliestARDateWithBalance()
    {
        DateTime? earliestMAS = GetEarliestMASAccountARDateWithBalance();
        DateTime? earliestAX = GetEarliestAXAccountARDateWithBalance();

        if (earliestMAS == null && earliestAX == null)
            return null;

        if (earliestMAS != null && earliestAX != null)
        {
            if (DateTime.Compare((DateTime)earliestMAS, (DateTime)earliestAX) < 0)
            {
                return (DateTime)earliestMAS;
            }
            else
            {
                return (DateTime)earliestAX;
            }
        }

        if (earliestMAS != null)
            return earliestMAS;

        return earliestAX;
    }

    private DateTime? GetEarliestMASAccountARDateWithBalance()
    {
        // Instadose Invoices
        var arHistories = idc.sp_AccountARHistory(accountID).ToList();

        if (arHistories == null || arHistories.Count <= 0)
            return null;

        arHistories = arHistories.Where(arh => arh.Balance > 0 && arh.InvoiceDate != null).ToList();

        if (arHistories == null || arHistories.Count <= 0)
            return null;

        // After ERP implementation, Credit Card Payment does not go thru MAS and UNIX. 
        // Following logic is to remove invoices which are paid by credit card after ERP.
        List<vw_AccountInvoicePayment> ccPayments = DAAccount.GetAccountInvoicePayments(accountID.ToString(), true);

        if (ccPayments != null && ccPayments.Count > 0)
        {
            for (int i = 0; i < arHistories.Count; i++)
            {
                if (arHistories[i].Balance.HasValue)
                {
                    var payment = ccPayments.FirstOrDefault(p => p.InvoiceNo == arHistories[i].InvoiceNo);
                    if (payment != null && payment.PaymentAmount.HasValue)
                    {
                        arHistories[i].Balance = (decimal)arHistories[i].Balance - (decimal)payment.PaymentAmount;
                    }
                }
            }

            arHistories = arHistories.Where(arh => arh.Balance > 0).ToList();
        }

        if (arHistories == null || arHistories.Count <= 0)
            return null;

        return arHistories.OrderBy(arh => arh.InvoiceDate).FirstOrDefault().InvoiceDate;
    }

    private DateTime? GetEarliestAXAccountARDateWithBalance()
    {
        AXInvoiceRequests aXInvoiceRequests = new AXInvoiceRequests();

        //return axInvoiceRequests.GetEarliestInvoiceWithBalance("INS-" + accountID);

        var invoices = axInvoiceRequests.GetInvoicesWithBalance("INS-" + accountID);

        if (invoices == null || invoices.Count <= 0)
            return null;

        for (int i = 0; i < invoices.Count; i++)
        {
            string invoicePrefix = invoices[i].INVOICEID.Substring(0, 4);

            if (invoicePrefix == "MRN-" || invoicePrefix == "INS-")
            {
                DateTime? tmpInvDate = portal_instadose_com_v3.Helpers.HAXInvoice.GetPreERPInvoiceDate(invoices[i].INVOICEID, true);
                if (tmpInvDate != null)
                    invoices[i].INVOICEDATE = (DateTime)tmpInvDate;
            }
        }

        return invoices.OrderBy(i => i.INVOICEDATE).FirstOrDefault().INVOICEDATE;
    }

    /// <summary>
    /// Get AR Indicator button style class by invoice date. Green: 0-60 days. Yellow: 61-149 days. Red: 150+ days
    /// </summary>
    /// <param name="earliestInvoiceDate"></param>
    /// <returns></returns>
    private string ARIndicatorButtonCSSClass(DateTime? earliestInvoiceDate)
    {
        string cssClass = "btn ar-indicator ";

        if (earliestInvoiceDate == null)
            return cssClass + "ar-indicator-green";

        int pastDays = (DateTime.Today - ((DateTime)earliestInvoiceDate).Date).Days;

        if (pastDays >= 0 && pastDays <= 60)
            return cssClass + "ar-indicator-green";

        if (pastDays > 60 && pastDays < 150)
            return cssClass + "ar-indicator-yellow";

        if (pastDays >= 150)
            return cssClass + "ar-indicator-red";

        return cssClass + "ar-indicator-green";
    }
}