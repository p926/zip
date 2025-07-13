using System;
using System.Configuration;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;
using Instadose.Data;

public partial class InstaDose_InformationFinder_Details_Device : System.Web.UI.Page
{
    /// <summary>
    /// Limits access to the functionality based on groups.
    /// </summary>
    string[] activeDirecoryGroups = { "IRV-IT", "IRV-CustomerSvc", "IRV-Client Services" };

    // Create the database reference
    InsDataContext idc = new InsDataContext();

    // Set AccountID, SerialNo, and DeviceID.
    // These variables are not dependant upon the UserID nor LocationID.
    public string userDeviceID = "";
    public int accountID = 0;
    public string serialNo = "";
    public int deviceID = 0;
    public int userID = 0;
    public int locationID = 0;
    public int bodyRegionID = 0;
    public bool isPrimary = true;
    public bool isUserDeviceActive = true;
    public bool isAccountDeviceActive = true;
    private Account account = null;

    # region Page Load
    protected void Page_Load(object sender, EventArgs e)
    {
        // Query active directory to see if the current user belongs to a group.
        //bool belongsToGroups = ActiveDirectoryQueries.UserExistsInAnyGroup(User.Identity.Name, activeDirecoryGroups);

        //if (!belongsToGroups) Response.Redirect("../Default.aspx");

        serialNo = Request.QueryString["ID"];
        int.TryParse(Request.QueryString["AccountID"], out accountID);

        if (serialNo == null || accountID == 0) Response.Redirect("../Default.aspx");

        account = (from a in idc.Accounts where a.AccountID == accountID select a).FirstOrDefault();

        //deviceID = GetDeviceID(serialNo);
        //userID = GetUserID(serialNo);
        //locationID = GetLocationID(accountID, serialNo, userID);

        // Check to see if Account is Active (not canceled).
        // If Active = true, then enable "Edit Badge Assignment" link.
        // Else disable "Edit Badge Assignment" link.
        // User cannot/should not be able to make edits.
        if (IsAccountActive(accountID) == true)
        {
            lnkbtnEditBadgeAssignment.Visible = true;
            lnkbtnEditBadgeAssignment.Enabled = true;
        }
        else
        {
            lnkbtnEditBadgeAssignment.Visible = false;
            lnkbtnEditBadgeAssignment.Enabled = false;
        }

        if (Page.IsPostBack) return;

        if (account.CustomerType.CustomerTypeCode.Trim() == "CON")  // consignment account
        {
            lnkbtnEditBadgeAssignment.Visible = false;
        }

        ResetMessages();
        ResetAllModalControls();
        FillBadgeAssignmentDetails();
        fvBadgeDetails.Visible = true;
        BindToFormView();
    }
    # endregion

    # region Get/Set IDs and Status of ALL Parameters being used.
    /// <summary>
    /// Get the status of AccountDevice Active is TRUE/FALSE.
    /// </summary>
    /// <param name="accountid"></param>
    /// <param name="serialno"></param>
    /// <returns></returns>
    private bool GetAccountDeviceStatus(int accountid, string serialno)
    {
        bool status = (from ad in idc.AccountDevices
                       where ad.AccountID == accountid
                       && ad.DeviceInventory.SerialNo == serialno
                       && ad.CurrentDeviceAssign == true
                       select ad.Active).FirstOrDefault();

        return status;
    }

    /// <summary>
    /// Checks to see if a UserDevice record exists for the given AccountID and Badge SerialNo.
    /// </summary>
    /// <param name="accountid"></param>
    /// <param name="serialno"></param>
    /// <returns></returns>
    private bool GetUserDeviceRecord(int accountid, string serialno)
    {
        var getUserDeviceRecord = (from ud in idc.UserDevices
                                   join ad in idc.AccountDevices on ud.DeviceID equals ad.DeviceID
                                   where ad.AccountID == accountid
                                   && ad.CurrentDeviceAssign == true && ad.Active == true
                                   && ud.DeviceInventory.SerialNo == serialno
                                   orderby ud.AssignmentDate descending
                                   select ud).FirstOrDefault();

        if (getUserDeviceRecord == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// IF Account is Inactive then "Edit Badge Assignment" link is will not be visible.
    /// </summary>
    /// <param name="accountid"></param>
    /// <returns></returns>
    private bool IsAccountActive(int accountid)
    {
        int count = (from acct in idc.Accounts
                     where acct.AccountID == accountid
                     && acct.Active == true && (!acct.RestrictOnlineAccess.HasValue || acct.RestrictOnlineAccess.Value == false)
                     select acct).Count();

        if (count == 0) return false;
        else return true;
    }

    /// <summary>
    /// Get DeviceID based on Query String values (AccountID, SerialNo).
    /// </summary>
    /// <returns></returns>
    private int GetDeviceID(string serialno)
    {
        int deviceNumber = (from di in idc.DeviceInventories
                            where di.SerialNo == serialno
                            select di.DeviceID).FirstOrDefault();
        return deviceNumber;
    }

    private string GetDeviceColor(int deviceID)
    {
        int caseCoverProductID = (from dc in idc.DeviceColorizations
                            where dc.DeviceID == deviceID
                            select dc.CaseCoverProductID).FirstOrDefault();

        string deviceColor = "";
        if (caseCoverProductID != 0)
        {
            deviceColor = (from p in idc.Products
                           where p.ProductID == caseCoverProductID
                           select p.Color).FirstOrDefault();
        }

        return deviceColor;
    }

    /// <summary>
    /// Get UserID based on SerialNo.
    /// This is used to select the correct value on the ddlWearer.
    /// </summary>
    /// <returns></returns>
    private int GetUserID(string serialno)
    {
        var getUserDeviceInfo = (from ud in idc.UserDevices
                                 where ud.DeviceInventory.SerialNo == serialno
                                 orderby ud.AssignmentDate descending
                                 select ud).FirstOrDefault();

        if (getUserDeviceInfo == null) return 0;
        else return getUserDeviceInfo.UserID;
    }

    /// <summary>
    /// Get LocationID based upon AccountID and SerialNo.
    /// This is used to select the correct value on the ddlLocation.
    /// </summary>
    /// <returns></returns>
    private int GetLocationID(int accountid, string serialno, int userid)
    {
        int getLocationID = 0;

        var acctdvcLocationID = (from ad in idc.AccountDevices
                                 where ad.AccountID == accountid
                                 && ad.DeviceInventory.SerialNo == serialno
                                 && ad.CurrentDeviceAssign == true
                                 select ad).FirstOrDefault();

        // IF AccountDevice LocationID is NULL, then get the LocationID from User-Level.
        if (acctdvcLocationID == null)
        {
            var usrLocationID = (from u in idc.Users
                                 where u.UserID == userid
                                 select u).FirstOrDefault();

            if (usrLocationID == null)
            {
                getLocationID = 0;
            }
            else
            {
                getLocationID = usrLocationID.LocationID;
            }
        }
        else
        {
            if (!acctdvcLocationID.LocationID.HasValue)
            {
                getLocationID = 0;
            }
            else
            {
                getLocationID = acctdvcLocationID.LocationID.Value;
            }
        }

        return getLocationID;

        // OLD CODE - KEEP UNTIL TESTED.
        //// IF AccountDevice LocationID is NULL, then get the LocationID from User-Level.
        //if (acctdvcLocationID.LocationID.HasValue)
        //{
        //    if (usrLocationID == null) return getLocationID = 0;
        //    else getLocationID = usrLocationID.LocationID;
        //}
        //else
        //{
        //    getLocationID = acctdvcLocationID.LocationID.Value;
        //}
    }

    /// <summary>
    /// Sets Text to Yes/No depending on Boolean.
    /// Used to display Active/Inactive status on Client-Side.
    /// </summary>
    /// <param name="boolean"></param>
    /// <returns></returns>
    public string YesNo(object boolean)
    {
        try
        {
            return Convert.ToBoolean(boolean) ? "Yes" : "No";
        }
        catch
        {
            return "No";
        }
    }

    /// <summary>
    /// Get the value of the LockLocation (TRUE/FALSE).
    /// </summary>
    /// <returns></returns>
    private bool GetIfLockLocation(int accountid)
    {
        bool result = true;

        result = (from a in idc.Accounts
                  where a.AccountID == accountid && a.Active == true
                  select a.LockLocation).FirstOrDefault();

        return result;
    }

    /// <summary>
    /// Get BodyRegionID from UserID and SerialNo (contraint by newest AssignmentDate).
    /// </summary>
    /// <param name="userID"></param>
    /// <returns></returns>
    private int GetBodyRegionID(int userid, string serialno)
    {
        if (userID != 0)
        {
            int bodyRegionID = (from ud in idc.UserDevices
                                where ud.UserID == userid
                                && ud.DeviceInventory.SerialNo == serialno
                                orderby ud.AssignmentDate descending
                                select ud.BodyRegionID).FirstOrDefault();

            return bodyRegionID;
        }
        else return 4; // Default to "Unassigned".
    }

    /// <summary>
    /// Get IfDeviceIsPrimary (based on UserID and SerialNo).
    /// </summary>
    /// <returns></returns>
    private bool GetIfDeviceIsPrimary(int userid, string serialno)
    {
        bool status = (from ud in idc.UserDevices
                       where ud.UserID == userid
                       && ud.DeviceInventory.SerialNo == serialno
                       orderby ud.AssignmentDate descending
                       select ud.PrimaryDevice).FirstOrDefault();

        return status;
    }

    /// <summary>
    /// Get Active status of Device.
    /// This function is used on the initial loading of the controls' values.
    /// It will determine whether the Status Button reads "Deactivate" or "Activate".
    /// </summary>
    /// <returns></returns>
    private bool GetUserDeviceStatus(int userid, string serialno)
    {
        bool isActive = (from ud in idc.UserDevices
                         where ud.UserID == userid
                         && ud.DeviceInventory.SerialNo == serialno
                         orderby ud.AssignmentDate descending
                         select ud.Active).FirstOrDefault();

        return isActive;
    }

    /// <summary>
    /// Compare ddlWearer.SelectedValue with hdnfldUserID.
    /// </summary>
    /// <returns></returns>
    private bool CompareUserIDs()
    {
        string originalUserID = this.hdnfldUserID.Value;
        string newUserID = this.ddlWearer.SelectedValue;

        if (originalUserID != newUserID) return false;
        else return true;
    }

    /// <summary>
    /// Compare ddlLocation.SelectedValue with hdnfldLocationID.
    /// </summary>
    /// <returns></returns>
    private bool CompareLocationIDs()
    {
        string originalLocationID = this.hdnfldLocationID.Value;
        string newLocationID = this.ddlLocation.SelectedValue;

        if (originalLocationID != newLocationID) return false;
        else return true;
    }

    /// <summary>
    /// Check to see if User already has an Active Badge assigned that is Primary.
    /// </summary>
    /// <param name="userid"></param>
    /// <returns></returns>
    private bool ValidateIsPrimary(int userid, string serialno)
    {
        int count = (from ud in idc.UserDevices
                     where ud.UserID == userid
                     && ud.DeviceInventory.SerialNo != serialno
                     && ud.PrimaryDevice == true
                     && ud.Active == true
                     select ud).Count();

        if (count > 0) return false;
        else return true;
    }

    /// <summary>
    /// Check to see if User already has an Active Badge assigned to selected BodyRegionID.
    /// BodyRegionID = 4 is valid for multiple assisgned Badges for a single User.
    /// </summary>
    /// <param name="userid"></param>
    /// <param name="bodyregionid"></param>
    /// <returns></returns>
    private bool ValidateBodyRegionID(int userid, int bodyregionid, string serialno)
    {
        int count = (from ud in idc.UserDevices
                     where ud.BodyRegionID == bodyregionid
                     && ud.UserID == userid
                     && ud.DeviceInventory.SerialNo != serialno
                     && ud.Active == true
                     select ud).Count();

        if (count > 0 && bodyregionid != 4) return false;
        else return true;
    }

    /// <summary>
    /// Should be fired (only) IF the device is being reactivated!
    /// </summary>
    /// <returns></returns>
    private bool IsDeviceAlreadyAssigned()
    {
        int count = (from ud in idc.UserDevices
                     where ud.DeviceInventory.SerialNo == serialNo
                     && ud.Active == true
                     select ud).Count();

        if (count > 0) return true;
        else return false;
    }

    /// <summary>
    /// Executed when the LockLocation flag on Account-Level is set to TRUE.
    /// </summary>
    /// <param name="locationID"></param>
    public void SetLocation(int accountid, string serialno, int locationid)
    {
        AccountDevice acctdev = (from ad in idc.AccountDevices
                                 where ad.AccountID == accountid
                                 && ad.DeviceInventory.SerialNo == serialno
                                 select ad).FirstOrDefault();

        // Update AccountDevice table with NEW LocationID.
        acctdev.LocationID = locationid;

        // Save the changes.
        idc.SubmitChanges();
    }

    /// <summary>
    /// Checks to see if the User has more than one Badge assigned.
    /// IF so and a Primary Badge is being Deactivated, then it will flag the other Badge as IsPrimary (TRUE).
    /// </summary>
    /// <param name="userID"></param>
    /// <returns></returns>
    private int SetIsPrimaryOnOtherBadge(int userid)
    {
        int count = (from ud in idc.UserDevices
                     where ud.UserID == userid && ud.Active == true
                     select ud).Count();

        return count;
    }
    # endregion

    # region Location and Wearer DDL Functions.
    /// <summary>
    /// Populate ddlLocation based upon AccountID (from Query String).
    /// </summary>
    private void FillLocationsDDL(int accountid)
    {
        ddlLocation.Items.Clear();

        var locationList = from l in idc.Locations
                           where l.AccountID == accountid
                           && l.Active == true
                           orderby l.LocationName
                           select new
                           {
                               l.LocationID,
                               l.LocationName
                           };

        this.ddlLocation.DataSource = locationList;
        this.ddlLocation.DataTextField = "LocationName";
        this.ddlLocation.DataValueField = "LocationID";
        this.ddlLocation.DataBind();

        ListItem firstItem = new ListItem("-- Select Location --", "0");
        this.ddlLocation.Items.Insert(0, firstItem);
    }

    /// <summary>
    /// Populate Wearers DDL based upon LocationID.
    /// IF, LocationID is 0/NULL, base values on AccountID.
    /// </summary>
    private void FillWearersDDL(int accountid)
    {
        ddlWearer.Items.Clear();

        int.TryParse(ddlLocation.SelectedValue, out locationID);

        // IF LocationID IS NOT "0", then selected Users from that location (only).
        if (locationID != 0)
        {
            var userList = from u in idc.Users
                           where u.LocationID == locationID
                           && u.Active == true
                           orderby u.LastName, u.FirstName ascending
                           select new
                           {
                               u.UserID,
                               FullName = u.FirstName + ' ' + u.LastName
                           };

            this.ddlWearer.DataSource = userList;
            this.ddlWearer.DataTextField = "FullName";
            this.ddlWearer.DataValueField = "UserID";
            this.ddlWearer.DataBind();

            ListItem firstItem = new ListItem("-- Select Wearer --", "0");
            this.ddlWearer.Items.Insert(0, firstItem);
        }
        else // ELSE, select User by AccountID (Account-Level).
        {
            var userList = from u in idc.Users
                           where u.AccountID == accountid
                           && u.Active == true
                           orderby u.LastName, u.FirstName ascending
                           select new
                           {
                               u.UserID,
                               FullName = u.FirstName + ' ' + u.LastName
                           };

            this.ddlWearer.DataSource = userList;
            this.ddlWearer.DataTextField = "FullName";
            this.ddlWearer.DataValueField = "UserID";
            this.ddlWearer.DataBind();

            ListItem firstItem = new ListItem("-- Select Wearer --", "0");
            this.ddlWearer.Items.Insert(0, firstItem);
        }
    }

    /// <summary>
    /// ddlWearer is populated according to the Location selected.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void ddlLocation_SelectedIndexChanged(object sender, EventArgs e)
    {
        int.TryParse(Request.QueryString["AccountID"], out accountID);
        FillWearersDDL(accountID);
    }

    protected void ddlWearer_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (CompareUserIDs() == false)
        {
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "alert('Badge #" + serialNo + " is being reassigned.\\nThis will uninitialize the badge.\\nClick OK to continue.')", true);
        }
        else
        {
            return;
        }
    }
    # endregion

    # region Fill FormView and Modal/Dialog Controls.
    /// <summary>
    /// Function to BIND StoredProcedure to FormView each time an update is made on the form.
    /// </summary>
    private void BindToFormView()
    {
        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.StoredProcedure;

        // Using the vw_CreditCardExpirationsListing.
        string sqlQuery = "sp_if_GetBadgeDetailsBySerialNoAndAccountID";

        sqlCmd.Parameters.Add(new SqlParameter("@pSerialNo", Request.QueryString["ID"]));
        sqlCmd.Parameters.Add(new SqlParameter("@pAccountID", Request.QueryString["AccountID"]));

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable dtBadgeDetailsTable = new DataTable();

        dtBadgeDetailsTable = new DataTable("Badge Assignment Details");

        // Create the columns for the DataTable.
        dtBadgeDetailsTable.Columns.Add("UserDeviceID", Type.GetType("System.String"));
        dtBadgeDetailsTable.Columns.Add("SerialNumber", Type.GetType("System.String"));
        dtBadgeDetailsTable.Columns.Add("DeviceID", Type.GetType("System.Int32"));
        dtBadgeDetailsTable.Columns.Add("AccountID", Type.GetType("System.Int32"));
        dtBadgeDetailsTable.Columns.Add("AccountName", Type.GetType("System.String"));
        dtBadgeDetailsTable.Columns.Add("ProductID", Type.GetType("System.Int32"));
        dtBadgeDetailsTable.Columns.Add("ProductName", Type.GetType("System.String"));
        dtBadgeDetailsTable.Columns.Add("AccountDeviceActive", Type.GetType("System.Boolean"));
        dtBadgeDetailsTable.Columns.Add("IsInitialized", Type.GetType("System.Int32"));
        dtBadgeDetailsTable.Columns.Add("FormatIsInitialized", Type.GetType("System.Boolean"));
        dtBadgeDetailsTable.Columns.Add("UserID", Type.GetType("System.Int32"));
        dtBadgeDetailsTable.Columns.Add("UserName", Type.GetType("System.String"));
        dtBadgeDetailsTable.Columns.Add("FullName", Type.GetType("System.String"));
        dtBadgeDetailsTable.Columns.Add("LocationID", Type.GetType("System.Int32"));
        dtBadgeDetailsTable.Columns.Add("LocationName", Type.GetType("System.String"));
        dtBadgeDetailsTable.Columns.Add("BodyRegionID", Type.GetType("System.Int32"));
        dtBadgeDetailsTable.Columns.Add("BodyRegionName", Type.GetType("System.String"));
        dtBadgeDetailsTable.Columns.Add("FirmwareVersion", Type.GetType("System.Int32"));
        dtBadgeDetailsTable.Columns.Add("CalibrationVersion", Type.GetType("System.String"));
        dtBadgeDetailsTable.Columns.Add("FailedCalibration", Type.GetType("System.Boolean"));
        dtBadgeDetailsTable.Columns.Add("IsPrimary", Type.GetType("System.Boolean"));
        dtBadgeDetailsTable.Columns.Add("ManufactureDate", Type.GetType("System.String"));
        dtBadgeDetailsTable.Columns.Add("ExpirationDate", Type.GetType("System.String"));
        dtBadgeDetailsTable.Columns.Add("ServiceStartDate", Type.GetType("System.String"));
        dtBadgeDetailsTable.Columns.Add("ServiceEndDate", Type.GetType("System.String"));
        dtBadgeDetailsTable.Columns.Add("DeactivationReason", Type.GetType("System.String"));
        dtBadgeDetailsTable.Columns.Add("DeactivateDate", Type.GetType("System.String"));

        bool isInitialized = true;
        bool isPrimary = true;
        bool isActive = true;

        string userName = "";
        string fullName = "";
        string locationName = "";
        int bodyRegionID = 0;
        string bodyRegionName = "";

        while (sqlDataReader.Read())
        {
            DataRow drow = dtBadgeDetailsTable.NewRow();

            userDeviceID = sqlDataReader["UserDeviceID"].ToString() != null | sqlDataReader["UserDeviceID"].ToString() != "" ? sqlDataReader["UserDeviceID"].ToString() : "0";

            isInitialized = sqlDataReader["IsInitialized"].ToString() != "" ? true : false;
            isPrimary = Convert.ToBoolean(sqlDataReader["IsPrimary"]) == true ? true : false;
            isActive = Convert.ToBoolean(sqlDataReader["AccountDeviceActive"]) == true ? true : false;

            if (isActive == false || userDeviceID == "0")
            {
                userID = 0;
                userName = "Not Assigned";
                fullName = "Not Assigned";
                locationID = 0;
                locationName = "Not Assigned";
                bodyRegionID = 4;
                bodyRegionName = "Unassigned";
            }
            else
            {
                userID = Convert.ToInt32(sqlDataReader["UserID"]);
                userName = sqlDataReader["UserName"].ToString();
                fullName = sqlDataReader["FullName"].ToString();
                locationID = Convert.ToInt32(sqlDataReader["LocationID"]);
                locationName = sqlDataReader["LocationName"].ToString();
                bodyRegionID = Convert.ToInt32(sqlDataReader["BodyRegionID"]);
                bodyRegionName = sqlDataReader["BodyRegionName"].ToString();
            }

            // Fill row details.
            drow["UserDeviceID"] = userDeviceID;
            drow["SerialNumber"] = sqlDataReader["SerialNumber"];
            drow["DeviceID"] = sqlDataReader["DeviceID"];
            drow["AccountID"] = sqlDataReader["AccountID"];
            drow["AccountName"] = sqlDataReader["AccountName"];
            drow["ProductID"] = sqlDataReader["ProductID"];
            drow["ProductName"] = sqlDataReader["ProductName"];
            drow["AccountDeviceActive"] = sqlDataReader["AccountDeviceActive"];
            drow["IsInitialized"] = sqlDataReader["IsInitialized"];
            drow["FormatIsInitialized"] = isInitialized;
            drow["UserID"] = userID;
            drow["UserName"] = userName;
            drow["FullName"] = fullName;
            drow["LocationID"] = locationID;
            drow["LocationName"] = locationName;
            drow["BodyRegionID"] = bodyRegionID;
            drow["BodyRegionName"] = bodyRegionName;
            drow["FirmwareVersion"] = sqlDataReader["FirmwareVersion"];
            drow["CalibrationVersion"] = sqlDataReader["CalibrationVersion"];
            drow["FailedCalibration"] = sqlDataReader["FailedCalibration"];
            drow["FormatIsInitialized"] = isInitialized;
            drow["FirmwareVersion"] = sqlDataReader["FirmwareVersion"];
            drow["CalibrationVersion"] = sqlDataReader["CalibrationVersion"];
            drow["FailedCalibration"] = sqlDataReader["FailedCalibration"];
            drow["IsPrimary"] = sqlDataReader["IsPrimary"];
            drow["ManufactureDate"] = sqlDataReader["ManufactureDate"];
            drow["ExpirationDate"] = sqlDataReader["ExpirationDate"];
            drow["ServiceStartDate"] = sqlDataReader["ServiceStartDate"];
            drow["ServiceEndDate"] = sqlDataReader["ServiceEndDate"];
            drow["DeactivationReason"] = sqlDataReader["DeactivationReason"];
            drow["DeactivateDate"] = sqlDataReader["DeactivateDate"];

            // Add rows to DataTable.
            dtBadgeDetailsTable.Rows.Add(drow);
        }

        // Get UserID and LocationID and set to HiddenField values.
        hdnfldUserDeviceID.Value = userDeviceID.ToString();
        hdnfldUserID.Value = userID.ToString();
        hdnfldLocationID.Value = locationID.ToString();
        hdnfldBodyRegionID.Value = bodyRegionID.ToString();
        hdnfldIsPrimary.Value = isPrimary.ToString();

        // Bind the results to the gridview.
        fvBadgeDetails.DataSource = dtBadgeDetailsTable;
        fvBadgeDetails.DataBind();

        sqlConn.Close();
        sqlDataReader.Close();
    }

    /// <summary>
    /// Get Badge Assignment details from Database (based upong Query String values).
    /// </summary>
    private void FillBadgeAssignmentDetails()
    {
        // First clear all Modal Controls and Messages.
        ResetMessages();
        ResetAllModalControls();

        int.TryParse(Request.QueryString["AccountID"], out accountID);
        serialNo = Request.QueryString["ID"];
        userID = GetUserID(serialNo);
        locationID = GetLocationID(accountID, serialNo, userID);
        bodyRegionID = GetBodyRegionID(userID, serialNo);
        isPrimary = GetIfDeviceIsPrimary(userID, serialNo);
        isUserDeviceActive = GetUserDeviceStatus(userID, serialNo);
        isAccountDeviceActive = GetAccountDeviceStatus(accountID, serialNo);

        // Fill Serial Number.
        lblEditSerialNo.Text = serialNo;

        // REQUIRE LOCATION IF LOCKLOCATION IS TRUE in Accounts table!
        if (GetIfLockLocation(accountID) == true) lblRequired.Visible = true;
        else lblRequired.Visible = false;

        if (isAccountDeviceActive == true)
        {
            lnkbtnEditBadgeAssignment.Visible = true;
            lnkbtnEditBadgeAssignment.Enabled = true;
            lnkbtnChangeBadgeStatus.Text = "Deactivate Badge";
            lnkbtnChangeBadgeStatus.CssClass = "Icon Remove";

            int devID = GetDeviceID(serialNo);
            string devColor = GetDeviceColor(devID);
            if (!IsDeviceAlreadyAssigned() && (devColor.ToLower() == "grey" || devColor.ToLower() == "gray"))
            {
                lnkbtnEditBadgeAssignment.Visible = false;
                lnkbtnEditBadgeAssignment.Enabled = false;
            }

            // Place code here to account for instances where there is no UserDevice record.
            // This may be taken-out/changed once table column changes are made to the database.
            if (GetUserDeviceRecord(accountID, serialNo) == true)
            {
                // Modal Controls.
                lblActivateDeactivateMessage.Text = string.Format("Badge #{0} will be DEACTIVATED. Please enter the reason.", serialNo);
                divDeactivateReason.Visible = true;
                btnActivateDeactivate.Text = "Deactivate";
            }
            else
            {
                // Modal Controls without Reason.
                lblActivateDeactivateMessage.Text = string.Format("Badge #{0} will be DEACTIVATED.", serialNo);
                divDeactivateReason.Visible = false;
                btnActivateDeactivate.Text = "Deactivate";
            }
        }
        else
        {
            lnkbtnEditBadgeAssignment.Visible = false;
            lnkbtnEditBadgeAssignment.Enabled = false;
            lnkbtnChangeBadgeStatus.Text = "Activate Badge";
            lnkbtnChangeBadgeStatus.CssClass = "Icon Add";

            // Modal Controls.
            requiredFieldSpan.Visible = false;

            lblActivateDeactivateMessage.Text = string.Format("Badge #{0} will be ACTIVATED.", serialNo);
            divDeactivateReason.Visible = false;
            btnActivateDeactivate.Text = "Activate";
        }

        // IF Active is TRUE, lblStatus is "Deactivate" and fill controls accordingly.
        // ELSE lblStatus is "Activate" and default all controls to value "0"/Checked = false.
        if (isUserDeviceActive == true)
        {
            // Fill Location.
            FillLocationsDDL(accountID);
            ddlLocation.SelectedValue = (locationID.ToString() != "0") ? locationID.ToString() : "0";   // default to supposed location

            // Fill Wearers.
            // Fixed by TDO on 3/21/2012.
            FillWearersDDL(accountID);
            ddlWearer.SelectedValue = (userID.ToString() != "0") ? userID.ToString() : "0";

            // Fill Body Region.
            ddlBodyRegion.SelectedValue = bodyRegionID.ToString();

            chkbxIsPrimary.Checked = isPrimary;

            btnStatus.Text = "Unassign Badge";
        }
        else
        {
            // Initiate all controls as being Enabled = false.
            // Once Active button is clicked, then they will be activated.
            FillLocationsDDL(accountID);
            ddlLocation.SelectedValue = (locationID.ToString() != "0") ? locationID.ToString() : "0";   // default to supposed location
            //ddlLocation.SelectedValue = "0";  //
            ddlLocation.Enabled = true;
            FillWearersDDL(accountID);
            ddlWearer.SelectedValue = "0";
            ddlWearer.Enabled = true;
            ddlBodyRegion.SelectedValue = "4";
            ddlBodyRegion.Enabled = true;
            chkbxIsPrimary.Checked = false;
            chkbxIsPrimary.Enabled = true;
            btnStatus.Visible = false;
            btnStatus.Enabled = false;
        }
    }
    #endregion

    # region Unassign/Assign Badge.
    /// <summary>
    /// Deactivate/Activate Badge.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnStatus_Click(object sender, EventArgs e)
    {
        int.TryParse(Request.QueryString["AccountID"], out accountID);
        serialNo = Request.QueryString["ID"];
        userID = GetUserID(serialNo);

        if (btnStatus.Text == "Unassign Badge")
        {
            UnassignBadge(accountID, serialNo, userID);
        }
    }

    /// <summary>
    /// When Unassigning Badge from current User the following must be done;
    /// 1.) Make sure another Badge is assigned as Primary, if the current Badge being unassigned is Primary.
    /// 2.) Set FirstReadID in DeviceInventory to NULL.
    /// 3.) Make current UserDeviceID record Active = FALSE.
    /// 4.) Make sure AccountDevice record is Active = FALSE.
    /// 5.) DeactivateDate = DateTime.Now
    /// 6.) DeactivateReason = "Badge has been unassigned..."
    /// </summary>
    /// <param name="accountid"></param>
    /// <param name="serialno"></param>
    /// <param name="userid"></param>
    private void UnassignBadge(int accountid, string serialno, int userid)
    {
        string errorMessage = "";
        string successMessage = "";
        bool getIsPrimaryState = true;
        //int.TryParse(hdnfldUserDeviceID.Value, out userDeviceID.Value);
        bool.TryParse(hdnfldIsPrimary.Value, out getIsPrimaryState);

        UserDevice usrdev = (from ud in idc.UserDevices
                             join ad in idc.AccountDevices on ud.DeviceID equals ad.DeviceID
                             where ud.DeviceInventory.SerialNo == serialno
                             && ad.AccountID == accountid
                             orderby ud.AssignmentDate descending
                             select ud).FirstOrDefault();

        if (usrdev == null) return;

        AccountDevice acctdev = (from ad in idc.AccountDevices
                                 where ad.AccountID == accountid
                                 && ad.DeviceInventory.SerialNo == serialno
                                 select ad).FirstOrDefault();

        if (acctdev == null) return;

        // If current Badge being unassigned is a Primary Device, then;
        // 1.) Check if the User has other active badges.
        // 2.) If any of the badges are not Primary (they should not be), then assign Primary to the most recent one.
        if (getIsPrimaryState == true)
        {
            bool valIsPrimary = ValidateIsPrimary(userid, serialno);

            UserDevice usrdvc2 = (from ud in idc.UserDevices
                                  where ud.UserID == userid
                                  && ud.DeviceInventory.SerialNo != serialno
                                  && ud.PrimaryDevice == false
                                  && ud.Active == true
                                  select ud).FirstOrDefault();

            if (usrdvc2 == null) usrdev.PrimaryDevice = false;
            else
            {
                if (valIsPrimary == true) usrdvc2.PrimaryDevice = true;
            }
        }
        else
        {
            usrdev.PrimaryDevice = false;
        }

        // Unassign Badge from current User.
        // UserDevice table.
        usrdev.Active = false;
        usrdev.DeactivateDate = DateTime.Now;

        // Get User's Full Name based on UserID.
        string fullName = "";
        var userFullName = (from u in idc.Users
                            where u.UserID == userid
                            select new
                            {
                                FullName = (u.FirstName + ' ' + u.LastName)
                            }).FirstOrDefault();

        if (userFullName == null) fullName = "Unknown";
        else fullName = userFullName.FullName;

        usrdev.DeactivationReason = string.Format("Badge #{0} has been unassigned from User #{1} ({2}).", serialno, userid, fullName);

        // DeviceInventory table.
        //usrdev.DeviceInventory.FirstReadID = null;

        // Save to database.
        try
        {
            // Save the changes.
            idc.SubmitChanges();
            successMessage = "User's Badge was unassigned successfully!";
            this.VisibleSuccess_Main(successMessage);
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('editBadgeAssignmentModal')", true);
            BindToFormView();
            FillBadgeAssignmentDetails();
        }
        catch (Exception ex)
        {
            errorMessage = "User's Badge could not be unassigned.";
            this.VisibleError_Main(errorMessage);
            return;
        }
    }

    /// <summary>
    /// This will execute when the "Assign Badge" button has been initiated.
    /// </summary>
    /// <param name="accountid"></param>
    /// <param name="serialno"></param>
    /// <param name="userid"></param>
    /// <param name="locationid"></param>
    /// <param name="bodyregionid"></param>
    /// <param name="isprimary"></param>
    private void AssignBadge()
    {
        string errorMessage = "";
        string successMessage = "";

        int.TryParse(Request.QueryString["AccountID"], out accountID);
        serialNo = Request.QueryString["ID"];
        int.TryParse(ddlWearer.SelectedValue, out userID);
        int.TryParse(ddlBodyRegion.SelectedValue, out bodyRegionID);
        isPrimary = chkbxIsPrimary.Checked;

        User userRecord = (from u in idc.Users where u.UserID == userID select u).FirstOrDefault();

        // Check if the user device was not found, throw an error.
        int deviceId = GetDeviceID(serialNo);
        UserDevice userDevice = (from ud in idc.UserDevices where ud.DeviceID == deviceId && ud.Active orderby ud.AssignmentDate descending select ud).FirstOrDefault();

        // Ensure the device is not assigned to anyone
        if (userDevice != null)
        {
            errorMessage = "This device is already assigned to someone.";
            this.VisibleError_Modal(errorMessage);
            this.ddlWearer.Focus();
            return;
        }

        // Check if the device is assigned to the previous user.
        UserDevice prevUserDevice = (from ud in idc.UserDevices
                             join ad in idc.AccountDevices on ud.DeviceID equals ad.DeviceID
                             where ud.DeviceInventory.SerialNo == serialNo
                             && ad.AccountID == accountID
                             orderby ud.Active descending, ud.AssignmentDate descending, ud.UserDeviceID descending
                             select ud).FirstOrDefault();

        // Create new record in UserDevice table.
        UserDevice usrdvc = new UserDevice();

        usrdvc.UserID = userID;

        usrdvc.DeviceID = deviceId;
        usrdvc.AssignmentDate = DateTime.Now;

        // Check if BodyRegionID is OK to assign to.
        bool valBodyRegionID = ValidateBodyRegionID(userID, bodyRegionID, serialNo);

        if (valBodyRegionID == false)
        {
            errorMessage = "This User already has a Badge assigned to the selected Body Region.";
            this.VisibleError_Modal(errorMessage);
            this.ddlBodyRegion.Focus();
            return;
        }
        else
        {
            usrdvc.BodyRegionID = Convert.ToInt32(ddlBodyRegion.SelectedValue);
        }

        // Check if PrimaryDevice is OK to assign to.
        bool valIsPrimary = ValidateIsPrimary(userID, serialNo);

        if (valIsPrimary == false && chkbxIsPrimary.Checked == true)
        {
            errorMessage = "This User already has a Primary Badge assigned.";
            this.VisibleError_Modal(errorMessage);
            chkbxIsPrimary.Focus();
            return;
        }
        else if (valIsPrimary == true && chkbxIsPrimary.Checked == false)
        {
            errorMessage = "This User requires a Primary Badge to be assigned.";
            this.VisibleError_Modal(errorMessage);
            chkbxIsPrimary.Focus();
            return;
        }
        else
        {
            usrdvc.PrimaryDevice = chkbxIsPrimary.Checked;
        }

        usrdvc.DeactivateDate = null;
        usrdvc.DeactivationReason = null;
        usrdvc.Active = true;

        idc.UserDevices.InsertOnSubmit(usrdvc);
        idc.SubmitChanges();

        // check if the device is assigned to the previous user, if true do not reset FirstReadID. Otherwise, set to NULL.
        if (prevUserDevice == null || (prevUserDevice != null && prevUserDevice.UserID != userRecord.UserID))
        {
            // DeviceInventory table's FirstReadID = null.
            usrdvc.DeviceInventory.FirstReadID = null;
        }

        idc.SubmitChanges();

        // AccountDevice table's CurrentDeviceAssign = true.
        AccountDevice acctdvc = (from ad in idc.AccountDevices
                                 where ad.AccountID == accountID
                                 && ad.DeviceInventory.SerialNo == serialNo
                                 select ad).FirstOrDefault();

        if (acctdvc == null) return;

        acctdvc.CurrentDeviceAssign = true;
        var hasChanges = false;
        //if (usrdvc.DeviceInventory.ScheduleID != userRecord.Location.ScheduleID)
        //{
        // do not update device inventory for ID3 devices                 
        var deviceInventory = idc.DeviceInventories.FirstOrDefault(di => di.DeviceID == usrdvc.DeviceInventory.DeviceID);
        if (deviceInventory.Product.ProductGroupID != Instadose.Device.CONSTS.INSTADOSE_3)
        {
            usrdvc.DeviceInventory.ScheduleSyncStatus = "PENDING";
            usrdvc.DeviceInventory.ScheduleID = userRecord.Location.ScheduleID ?? userRecord.Account.ScheduleID;
        }            
        hasChanges = true;
        //}

        idc.SubmitChanges();

        try
        {            
            if (deviceInventory.Product.ProductGroupID != Instadose.Device.CONSTS.INSTADOSE_3)
            {
                var resetDevice = idc.sp_SetDeviceProfile(usrdvc.DeviceInventory.DeviceID, usrdvc.DeviceInventory.SerialNo);
            }

            successMessage = "Badge was assigned successfully!";
            this.VisibleSuccess_Main(successMessage);
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('editBadgeAssignmentModal')", true);
            // Reassign HiddenFields with new values.
            var newUserDeviceInfo = (from ud in idc.UserDevices
                                     orderby ud.AssignmentDate descending
                                     select ud).FirstOrDefault();

            // Assign newest paramters to HiddenFields.
            hdnfldUserDeviceID.Value = newUserDeviceInfo.UserDeviceID.ToString();
            hdnfldUserID.Value = newUserDeviceInfo.UserID.ToString();
            if (acctdvc.LocationID == null) hdnfldLocationID.Value = newUserDeviceInfo.User.LocationID.ToString();
            else hdnfldLocationID.Value = acctdvc.LocationID.ToString();
            hdnfldBodyRegionID.Value = newUserDeviceInfo.BodyRegionID.ToString();
            hdnfldIsPrimary.Value = newUserDeviceInfo.PrimaryDevice.ToString();

            BindToFormView();
            FillBadgeAssignmentDetails();
        }
        catch (Exception ex)
        {
            errorMessage = "Badge could not be assigned.";
            this.VisibleError_Main(errorMessage);
            return;
        }
    }
    #endregion

    # region Edit (UPDATE)/Change (ADD) Badge Assignment Functions.
    /// <summary>
    /// Update UserDevice record (only).
    /// When updating Badge Assignment information only the following are affected;
    /// 1.) Body Region.
    /// 2.) Primary
    /// The User and Location are not being changed.
    /// </summary>
    /// <param name="userid"></param>
    /// <param name="bodyregionid"></param>
    private void UpdateUserDevice(int accountid, int userid, int bodyregionid, string serialno)
    {
        string errorMessage = "";
        string successMessage = "";
        //int.TryParse(hdnfldUserDeviceID.Value, out userDeviceID);

        UserDevice usrdev = (from ud in idc.UserDevices
                             join ad in idc.AccountDevices on ud.DeviceID equals ad.DeviceID
                             where ud.DeviceInventory.SerialNo == serialno
                             && ad.AccountID == accountid
                             orderby ud.AssignmentDate descending
                             select ud).FirstOrDefault();

        if (usrdev == null) return;

        // make sure userdevice record is not deactivated, must create new record is already deactivated
        if (usrdev.DeactivateDate.HasValue)
        {
            AssignBadge();
            return;
        }

        AccountDevice acctdev = (from ad in idc.AccountDevices
                                 where ad.AccountID == accountid
                                 && ad.DeviceInventory.SerialNo == serialno
                                 select ad).FirstOrDefault();

        if (acctdev == null) return;

        bool valBodyRegionID = ValidateBodyRegionID(userid, bodyregionid, serialno);

        if (valBodyRegionID == false)
        {
            errorMessage = "This User already has a Badge assigned to the selected Body Region.";
            this.VisibleError_Modal(errorMessage);
            this.ddlBodyRegion.Focus();
            return;
        }
        else
        {
            usrdev.BodyRegionID = Convert.ToInt32(ddlBodyRegion.SelectedValue);
        }

        bool valIsPrimary = ValidateIsPrimary(userid, serialno);

        if (valIsPrimary == false && chkbxIsPrimary.Checked == true)
        {
            errorMessage = "This User already has a Primary Badge assigned.";
            this.VisibleError_Modal(errorMessage);
            return;
        }
        else if (valIsPrimary == true && chkbxIsPrimary.Checked == false)
        {
            errorMessage = "This User requires a Primary Badge to be assigned.";
            this.VisibleError_Modal(errorMessage);
            return;
        }
        else
        {
            usrdev.PrimaryDevice = chkbxIsPrimary.Checked;
        }

        // AccountDevices table.
        acctdev.Active = true;
        acctdev.ServiceStartDate = DateTime.Now;

        // UserDevices table.
        usrdev.Active = true;

        try
        {
            // Save the changes.
            idc.SubmitChanges();
            successMessage = "User's Badge information was updated successfully!";
            this.VisibleSuccess_Main(successMessage);
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('editBadgeAssignmentModal')", true);
            BindToFormView();
            FillBadgeAssignmentDetails();
        }
        catch (Exception ex)
        {
            errorMessage = "User's Badge information could not be updated.";
            this.VisibleError_Main(errorMessage);
            return;
        }
    }

    /// <summary>
    /// Create NEW UserDevice record and UPDATE AccountDevice record.
    /// </summary>
    /// <param name="deviceID"></param>
    /// <param name="userid"></param>
    /// <param name="bodyregionid"></param>
    /// <param name="isprimary"></param>
    private void AddUserDeviceRecord(int accountid, string serialno, int userid, int bodyregionid, bool isprimary)
    {
        string errorMessage = "";
        string successMessage = "";

        User userRecord = (from u in idc.Users where u.UserID == userID select u).FirstOrDefault();

        AccountDevice acctdev = (from ad in idc.AccountDevices
                                 where ad.AccountID == accountid
                                 && ad.DeviceInventory.SerialNo == serialno
                                 select ad).FirstOrDefault();

        if (acctdev == null) return;

        DeviceInventory devinv = (from di in idc.DeviceInventories
                                  where di.SerialNo == serialno
                                  select di).FirstOrDefault();

        if (devinv == null) return;

        bool valBodyRegionID = ValidateBodyRegionID(userid, bodyregionid, serialno);
        bool valIsPrimary = ValidateIsPrimary(userid, serialno);

        UserDevice usrdev = null;

        int newUserID = Convert.ToInt32(ddlWearer.SelectedValue);

        if (valBodyRegionID == true && valIsPrimary == true)
        {
            // First, Unassign current Badge Assignment.
            UnassignBadge(accountid, serialno, userid);

            // AccountDevices table.
            acctdev.CurrentDeviceAssign = true;
            acctdev.AssignmentDate = DateTime.Now;
            acctdev.Active = true;
            acctdev.ServiceStartDate = DateTime.Now;

            // DeviceInventory table.
            devinv.FirstRead = null;

            usrdev = new UserDevice
            {
                UserID = newUserID,
                DeviceID = devinv.DeviceID,
                AssignmentDate = DateTime.Now,
                DeactivateDate = null,
                BodyRegionID = bodyregionid,
                PrimaryDevice = isprimary,
                Active = true
            };
        }
        else
        {
            // First, Unassign current Badge Assignment.
            UnassignBadge(accountid, serialno, userid);

            // AccountDevices table.
            acctdev.CurrentDeviceAssign = true;
            acctdev.AssignmentDate = DateTime.Now;
            acctdev.Active = true;
            acctdev.ServiceStartDate = DateTime.Now;

            // DeviceInventory table.
            devinv.FirstRead = null;

            usrdev = new UserDevice
            {
                UserID = newUserID,
                DeviceID = devinv.DeviceID,
                AssignmentDate = DateTime.Now,
                DeactivateDate = null,
                BodyRegionID = 4,
                PrimaryDevice = false,
                Active = true
            };
        }

        try
        {
            idc.UserDevices.InsertOnSubmit(usrdev);
            var hasChanges = false;
            // do not update device inventory for ID3 devices                 
            var deviceInventory = idc.DeviceInventories.FirstOrDefault(di => di.DeviceID == devinv.DeviceID);           
            if (devinv.ScheduleID != userRecord.Location.ScheduleID)
            {
                if (deviceInventory.Product.ProductGroupID != Instadose.Device.CONSTS.INSTADOSE_3)
                {
                    devinv.ScheduleSyncStatus = "PENDING";
                    devinv.ScheduleID = userRecord.Location.ScheduleID ?? userRecord.Account.ScheduleID;
                    hasChanges = true;
                }                    
            }

            // Save the changes.
            idc.SubmitChanges();

            // do not reset device profile for ID3 devices                    
            if (deviceInventory.Product.ProductGroupID != Instadose.Device.CONSTS.INSTADOSE_3)
            {
                var resetDevice = idc.sp_SetDeviceProfile(devinv.DeviceID, devinv.SerialNo);
            }

            successMessage = "Badge was reassinged successfully!";
            this.VisibleSuccess_Main(successMessage);
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('editBadgeAssignmentModal')", true);
            BindToFormView();
            FillBadgeAssignmentDetails();
        }
        catch (Exception ex)
        {
            errorMessage = "Badge could not be reassigned.";
            this.VisibleError_Main(errorMessage);
            return;
        }
    }

    /// <summary>
    /// UPDATE/ADD information to DB.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnSave_Click(object sender, EventArgs e)
    {
        // Prevent double click on the Save button
        btnSave.Enabled = false;

        // General varialbes.
        int.TryParse(Request.QueryString["AccountID"], out accountID);
        serialNo = Request.QueryString["ID"];
        bool lockLocation = GetIfLockLocation(accountID);
        bool sameUserID = CompareUserIDs();
        bool sameLocationID = CompareLocationIDs();
        string errorMessage = "";

        // Set Update & Add parameters (variables).
        int.TryParse(this.ddlWearer.SelectedValue, out userID);
        int.TryParse(this.ddlLocation.SelectedValue, out locationID);
        int.TryParse(this.ddlBodyRegion.SelectedValue, out bodyRegionID);
        isPrimary = chkbxIsPrimary.Checked;

        if (lockLocation == true)
        {
            if (locationID != 0)
            {
                SetLocation(accountID, serialNo, locationID);
            }
            else
            {
                errorMessage = "Please select a Location.";
                this.VisibleError_Modal(errorMessage);
                ddlLocation.Focus();
                btnSave.Enabled = true;
                return;
            }
        }

        if (userID == 0 || ddlWearer.SelectedValue == "0")
        {
            errorMessage = "A Wearer must be selected.";
            this.VisibleError_Modal(errorMessage);
            this.ddlWearer.Focus();
            btnSave.Enabled = true;
            return;
        }

        // UPDATE.
        if (sameUserID == true && sameLocationID == true) // UPDATE.
        {
            UpdateUserDevice(accountID, userID, bodyRegionID, serialNo);
        }

        // ADD.
        if (sameUserID == false)
        {
            if (btnStatus.Visible == false)
            {
                AssignBadge();

                // If reassignment is successful, then...
                // force the Status button to show and prime for Unassignment.
                btnStatus.Visible = true;
                btnStatus.Enabled = true;
                btnStatus.Text = "Unassign Badge";
            }
            else
            {
                // INSERT the new record in both AccountDevices and UserDevices tables.
                AddUserDeviceRecord(accountID, serialNo, userID, bodyRegionID, isPrimary);

                // If reassignment is successful, then...
                // force the Status button to show and prime for Unassignment.
                btnStatus.Visible = true;
                btnStatus.Enabled = true;
                btnStatus.Text = "Unassign Badge";
            }
        }
        btnSave.Enabled = true;
    }

    protected void btnCancel_Click(object sender, EventArgs e)
    {
        ResetMessages();
        ResetAllModalControls();
        BindToFormView();
        FillBadgeAssignmentDetails();
    }
    #endregion

    # region Activate/Deactivate Account Devices (Badges).
    /// <summary>
    /// Deactivate Badge w/ associated UserID.
    /// Must include a reason.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnActivateDeactivate_Click(object sender, EventArgs e)
    {
        int.TryParse(Request.QueryString["AccountID"], out accountID);
        serialNo = Request.QueryString["ID"];
        //int.TryParse(hdnfldUserDeviceID.Value, out userDeviceID);

        AccountDevice acctdvc = (from ad in idc.AccountDevices
                                 where ad.AccountID == accountID
                                 && ad.DeviceInventory.SerialNo == serialNo
                                 select ad).FirstOrDefault();

        if (acctdvc == null) return;

        UserDevice usrdvc = (from ud in idc.UserDevices
                             join ad in idc.AccountDevices on ud.DeviceID equals ad.DeviceID
                             where ud.DeviceInventory.SerialNo == serialNo
                             && ad.AccountID == accountID
                             orderby ud.AssignmentDate descending
                             select ud).FirstOrDefault();

        string reason = txtDeactivateReason.Text.Replace("'", "''");

        // Deactivate Badge.
        if (acctdvc.Active == true)
        {
            if (usrdvc != null)
            {
                if (txtDeactivateReason.Text == "")
                {
                    // Error Message is displayed on Main Form.
                    string errorMsg = "Please enter a reason.";
                    VisibleError_Reason(errorMsg);
                    txtDeactivateReason.Focus();
                    return;
                }
                else
                {
                    // AccountDevice table.
                    acctdvc.Active = false;
                    acctdvc.DeactivateDate = DateTime.Now;

                    // UserDevice table.
                    usrdvc.Active = false;
                    usrdvc.PrimaryDevice = false;
                    usrdvc.DeactivateDate = DateTime.Now;
                    usrdvc.DeactivationReason = reason;
                }
            }
            else
            {
                // AccountDevice table.
                acctdvc.Active = false;
                acctdvc.DeactivateDate = DateTime.Now;
            }

            try
            {
                // Save to database.
                idc.SubmitChanges();
                string successMsg = "Badge #" + serialNo + " was successfully deactivated.";
                VisibleSuccess_Main(successMsg);
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('activateDeactivateModal')", true);
                BindToFormView();
                FillBadgeAssignmentDetails();
            }
            catch (Exception ex)
            {
                // Error Message is displayed on Main Form.
                string errorMsg = "Badge #" + serialNo + " could not be deactivated.";
                VisibleError_Main(errorMsg);
                return;
            }
        }
        //Activate Badge.
        else
        {
            acctdvc.Active = true;
            acctdvc.CurrentDeviceAssign = true;
            acctdvc.ServiceStartDate = DateTime.Now;

            try
            {
                // Save to database.
                idc.SubmitChanges();
                string successMsg = "Badge #" + serialNo + " was successfully activated.";
                VisibleSuccess_Main(successMsg);
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('activateDeactivateModal')", true);
                BindToFormView();
                FillBadgeAssignmentDetails();
            }
            catch (Exception ex)
            {
                // Error Message is displayed on Main Form.
                string errorMsg = "Badge #" + serialNo + " could not be activated.";
                VisibleError_Main(errorMsg);
                return;
            }
        }
    }

    protected void btnActivateDeactivateCancel_Click(object sender, EventArgs e)
    {
        ResetMessages();
        ResetAllModalControls();
        BindToFormView();
        FillBadgeAssignmentDetails();
    }
    # endregion

    /// <summary>
    /// Redirects User back to Account Details page.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnBackToAccount_Click(object sender, EventArgs e)
    {
        Page.Response.Redirect(string.Format("Account.aspx?ID={0}", accountID));
    }

    # region Error/Success Messages and Reset Controls Functions.
    private void InvisibleError_Modal()
    {
        this.editBadgeAssignmentErrorMsg.InnerText = "";
        this.editBadgeAssignmentError.Visible = false;
    }

    private void VisibleError_Modal(string errorMsg)
    {
        this.editBadgeAssignmentErrorMsg.InnerText = errorMsg;
        this.editBadgeAssignmentError.Visible = true;
    }

    private void InvisibleError_Main()
    {
        this.mainErrorMsg.InnerText = "";
        this.divMainError.Visible = false;
    }

    private void VisibleError_Main(string errorMsg)
    {
        this.mainErrorMsg.InnerText = errorMsg;
        this.divMainError.Visible = true;
    }

    private void InvisibleSuccess_Main()
    {
        this.mainSuccessMsg.InnerText = "";
        this.divMainSuccess.Visible = false;
    }

    private void VisibleSuccess_Main(string successMsg)
    {
        this.mainSuccessMsg.InnerText = successMsg;
        this.divMainSuccess.Visible = true;
    }

    private void InvisibleError_Reason()
    {
        this.spnActivateDeactivateErrorMsg.InnerText = "";
        this.divActiveDeactivateError.Visible = false;
    }

    private void VisibleError_Reason(string errorMsg)
    {
        this.spnActivateDeactivateErrorMsg.InnerText = errorMsg;
        this.divActiveDeactivateError.Visible = true;
    }

    private void ResetMessages()
    {
        InvisibleError_Main();
        InvisibleSuccess_Main();
        InvisibleError_Modal();
        InvisibleError_Reason();
    }

    private void ResetAllModalControls()
    {
        lblEditSerialNo.Text = string.Empty;
        ddlLocation.Enabled = true;
        ddlLocation.Items.Clear();
        ListItem locationFirstItem = new ListItem("-- Select Location --", "0");
        ddlLocation.Items.Add(locationFirstItem);
        ddlWearer.Enabled = true;
        ddlWearer.Items.Clear();
        ListItem wearerFirstItem = new ListItem("-- Select Wearer --", "0");
        ddlWearer.Items.Add(wearerFirstItem);
        ddlBodyRegion.Enabled = true;
        ddlBodyRegion.SelectedIndex = 0;
        chkbxIsPrimary.Enabled = true;
        chkbxIsPrimary.Checked = false;
        txtDeactivateReason.Text = string.Empty;
        btnStatus.Visible = true;
        btnStatus.Enabled = true;
        btnStatus.Text = string.Empty;
    }
    # endregion
}
