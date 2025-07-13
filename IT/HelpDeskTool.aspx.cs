using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Instadose;
using Instadose.Data;
using Instadose.Integration;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

public partial class IT_HelpDeskTool : System.Web.UI.Page
{
    // Create the database reference
    InsDataContext idc = new InsDataContext();
    AppDataContext adc = new AppDataContext();
    MASDataContext mdc = new MASDataContext();

    public bool isMalvernIntegration = false;
    public string UserName = "Unknown";
    public string[] ActiveDirecoryGroups = { "IRV-IT" };

    protected void Page_Load(object sender, EventArgs e)
    {
        try { UserName = Page.User.Identity.Name.Split('\\')[1]; }
        catch { UserName = "Unknown"; }

        bool belongsToGroups = true;// ActiveDirectoryQueries.UserExistsInAnyGroup(UserName, ActiveDirecoryGroups);
        // IF the User exists in the Required Group, THEN make the page content visible.
        if (belongsToGroups)
        {
            InvisibleError_Tab1();
            InvisibleMsg_Tab1();
            InvisibleError_Tab2();
            InvisibleMsg_Tab2();
            InvisibleError_Tab3();
            InvisibleMsg_Tab3();
            InvisibleError_Tab4();
            InvisibleMsg_Tab4();
            InvisibleError_Tab5();
            InvisibleMsg_Tab5();
            InvisibleError_Tab6();
            InvisibleMsg_Tab6();
            InvisibleError_Tab7();
            InvisibleMsg_Tab7();
            InvisibleError_Tab8();
            InvisibleMsg_Tab8();

            if (!this.IsPostBack)
            {
                InitiateAllControls();
                SetInitControlsDefault();

                isMalvernIntegration = Convert.ToBoolean(GetAppSettings("MalvernIntegration"));
            }

            LoadControls();

        }
        else
        {
            Response.Redirect("~/Default.aspx");
        }
    }

    private void SetInitControlsDefault()
    {
        SetControlsDefault_Tab4();
        SetControlsDefault_Tab5();
        SetControlsDefault_Tab6();
    }

    private void InitiateAllControls()
    {
    }

    private void LoadControls()
    {
    }

    //------------------------ Begin Delete Users ----------------------------//

    protected void txtAct_Tab1_TextChanged(object sender, EventArgs e)
    {
        Int32 actID_Tab1 = 0;
        if (Int32.TryParse(this.txtAct_Tab1.Text, out actID_Tab1))
            LoadUsernames_Tab1(actID_Tab1);
        else
            LoadUsernames_Tab1(0);
    }

    private void LoadUsernames_Tab1(int pAccountID)
    {
        var users = from r in idc.Users
                    join a in idc.Accounts on r.AccountID equals a.AccountID
                    where r.AccountID == pAccountID
                    && r.UserID != a.AccountAdminID
                    orderby r.UserName ascending
                    select new
                    {
                        r.UserID,
                        r.UserName
                    };

        this.lboxUsernames_Tab1.DataSource = users;
        this.lboxUsernames_Tab1.DataTextField = "UserName";
        this.lboxUsernames_Tab1.DataValueField = "UserID";
        this.lboxUsernames_Tab1.DataBind();

        ListItem item0 = new ListItem("", "0");
        this.lboxUsernames_Tab1.Items.Insert(0, item0);
    }

    private Boolean PassValidation_Tab1()
    {

        Int32 actID;
        int count = 0;

        if (this.txtAct_Tab1.Text.Trim().Length == 0 || Int32.TryParse(this.txtAct_Tab1.Text, out actID) == false)
        {
            VisibleError_Tab1("Account# is required and must be a number.");
            return false;
        }
        else
        {
            var act = (from a in idc.Accounts where a.AccountID == actID select a).FirstOrDefault();
            if (act == null)
            {
                VisibleError_Tab1("Account# is not in system.");
                return false;
            }
        }

        foreach (ListItem li in lboxUsernames_Tab1.Items)
        {
            if (li.Selected && li.Text.Trim().Length > 0)
            {
                count++;
            }
        }

        if (count == 0)
        {
            VisibleError_Tab1("Select at least one usernames to process.");
            return false;
        }

        return true;
    }

    protected void btnDelete_Tab1_Click(object sender, EventArgs e)
    {
        if (PassValidation_Tab1())
        {
            try
            {
                int accountID = int.Parse(this.txtAct_Tab1.Text.Trim());
                string selUsername = "";
                string msg = "";
                List<string> undeletedUsers = new List<string>();

                foreach (ListItem li in lboxUsernames_Tab1.Items)
                {
                    if (li.Selected == true)
                    {
                        selUsername = li.Text;
                        msg = "";
                        if (DeleteUsers(accountID, selUsername, ref msg) == false)
                        {
                            undeletedUsers.Add(selUsername + ": " + msg);
                        }
                    }
                }

                if (undeletedUsers.Count > 0)
                {
                    VisibleError_Tab1("The following users could not be deleted as they are having history records as detail below. <br /> <p>Users with history records in tables: </p> " + GenerateBulletMessage(undeletedUsers));
                }
                else
                {
                    VisibleMsg_Tab1("Delete completely.");
                }

                // Reload Usernames control
                SetControlsDefault_Tab1();
            }
            catch (Exception ex)
            {
                // Display the system generated error message.                
                VisibleError_Tab1(string.Format("An error occurred: {0}", ex.Message));
            }
        }

    }

    private bool DeleteUsers(int pAccountID, string pUsername, ref string pMsg)
    {
        try
        {
            String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            String cmdStr = "dbo.sp_DeleteUser";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;

            sqlCmd.Parameters.Add("@AccountID", SqlDbType.Int);
            sqlCmd.Parameters["@AccountID"].Value = pAccountID;

            sqlCmd.Parameters.Add("@Username", SqlDbType.NVarChar, 50);
            sqlCmd.Parameters["@Username"].Value = pUsername;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);

            sqlConn.Close();

            if (dt != null && dt.Rows.Count > 0 && dt.Rows[0][0] != null)
            {
                if (dt.Rows[0][0].ToString().Trim() == "1")
                {
                    pMsg = "";
                    return true;
                }
                else
                {
                    pMsg = dt.Rows[0][0].ToString().Trim();
                    return false;
                }
            }
            else
            {
                pMsg = "Transact-SQL error!!!";
                return false;
            }
        }
        catch (Exception ex)
        {
            pMsg = ex.ToString();
            return false;
        }
    }

    private void SetControlsDefault_Tab1()
    {
        this.txtAct_Tab1.Text = string.Empty;
        this.lboxUsernames_Tab1.Items.Clear();
        this.lboxUsernames_Tab1.DataSource = null;
        this.lboxUsernames_Tab1.DataBind();
    }

    //------------------------ End Delete Users ----------------------------//

    //------------------------ Begin Set Account Admin Users ----------------------------//

    protected void txtAct_Tab2_TextChanged(object sender, EventArgs e)
    {
        Int32 actID_Tab2 = 0;
        if (Int32.TryParse(this.txtAct_Tab2.Text, out actID_Tab2))
        {
            LoadUsernames_Tab2(actID_Tab2);
            var adminUser = (from a in idc.Accounts
                             join u in idc.Users on a.AccountAdminID equals u.UserID
                             where a.AccountID == actID_Tab2
                             select u).FirstOrDefault();

            txtCurrentActAdmin_Tab2.Text = adminUser == null ? string.Empty : adminUser.UserName;
        }
        else
        {
            LoadUsernames_Tab2(0);
            txtCurrentActAdmin_Tab2.Text = string.Empty;
        }
    }

    private void LoadUsernames_Tab2(int pAccountID)
    {
        var users = from r in idc.Users
                    where r.AccountID == pAccountID && r.Active == true
                    orderby r.UserName ascending
                    select new
                    {
                        r.UserID,
                        r.UserName
                    };

        this.ddlUsernames_Tab2.DataSource = users;
        this.ddlUsernames_Tab2.DataTextField = "UserName";
        this.ddlUsernames_Tab2.DataValueField = "UserID";
        this.ddlUsernames_Tab2.DataBind();

        ListItem item0 = new ListItem("", "0");
        this.ddlUsernames_Tab2.Items.Insert(0, item0);
    }

    private Boolean PassValidation_Tab2()
    {

        Int32 actID;

        if (this.txtAct_Tab2.Text.Trim().Length == 0 || Int32.TryParse(this.txtAct_Tab2.Text, out actID) == false)
        {
            VisibleError_Tab2("Account# is required and must be a number.");
            return false;
        }
        else
        {
            var act = (from a in idc.Accounts where a.AccountID == actID select a).FirstOrDefault();
            if (act == null)
            {
                VisibleError_Tab2("Account# is not in system.");
                return false;
            }
        }

        if (this.ddlUsernames_Tab2.SelectedItem.Value == "0")
        {
            VisibleError_Tab2("Select a new account admin user.");
            return false;
        }
        else
        {
            var myUser = (from a in idc.Users where a.UserID == int.Parse(this.ddlUsernames_Tab2.SelectedItem.Value) && a.Active == true select a).FirstOrDefault();
            if (myUser == null)
            {
                VisibleError_Tab2("Could not set inactive user as new account admin user.");
                return false;
            }
        }

        return true;
    }

    protected void btnOK_Tab2_Click(object sender, EventArgs e)
    {
        if (PassValidation_Tab2())
        {

            try
            {
                int accountID = int.Parse(this.txtAct_Tab2.Text.Trim());
                int selUserID = int.Parse(this.ddlUsernames_Tab2.SelectedItem.Value);

                string msg = "";

                if (SetAccountAdminUser(accountID, selUserID, ref msg) == false)
                {
                    VisibleError_Tab2("Unsuccessfully set up " + this.ddlUsernames_Tab2.SelectedItem.Text + " as account admin user!!! <br /> <p>" + msg + "</p>");
                }
                else
                {
                    VisibleMsg_Tab2("Account admin user is set successfully.");
                }

                // Reload controls
                SetControlsDefault_Tab2();
            }
            catch (Exception ex)
            {
                // Display the system generated error message.                
                VisibleError_Tab2(string.Format("An error occurred: {0}", ex.Message));
            }

        }
    }

    private void SetControlsDefault_Tab2()
    {
        this.txtAct_Tab2.Text = string.Empty;
        this.txtCurrentActAdmin_Tab2.Text = string.Empty;
        this.ddlUsernames_Tab2.Items.Clear();
        this.ddlUsernames_Tab2.DataSource = null;
        this.ddlUsernames_Tab2.DataBind();
    }

    private bool SetAccountAdminUser(int pAccountID, int pUserID, ref string pMsg)
    {
        try
        {
            String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            String cmdStr = "dbo.sp_SetAccountAdminUser";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;

            sqlCmd.Parameters.Add("@AccountID", SqlDbType.Int);
            sqlCmd.Parameters["@AccountID"].Value = pAccountID;

            sqlCmd.Parameters.Add("@UserID", SqlDbType.Int);
            sqlCmd.Parameters["@UserID"].Value = pUserID;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);

            sqlConn.Close();

            if (dt != null && dt.Rows.Count > 0 && dt.Rows[0][0] != null)
            {
                if (dt.Rows[0][0].ToString().Trim() == "1")
                {
                    pMsg = "";
                    return true;
                }
                else
                {
                    pMsg = dt.Rows[0][0].ToString().Trim();
                    return false;
                }
            }
            else
            {
                pMsg = "Transact-SQL error!!!";
                return false;
            }
        }
        catch (Exception ex)
        {
            pMsg = ex.ToString();
            return false;
        }
    }

    //------------------------ End Set Account Admin Users ----------------------------//

    //------------------------ Begin Set Account HDN ----------------------------//

    private Boolean PassValidation_Tab3()
    {
        Int32 actID;

        if (this.txtAct_Tab3.Text.Trim().Length == 0 || Int32.TryParse(this.txtAct_Tab3.Text, out actID) == false)
        {
            VisibleError_Tab3("Account# is required and must be a number.");
            return false;
        }
        else
        {
            var act = (from a in idc.Accounts where a.AccountID == actID select a).FirstOrDefault();
            if (act == null)
            {
                VisibleError_Tab3("Account# is not in system.");
                return false;
            }
        }

        return true;
    }

    protected void btnOK_Tab3_Click(object sender, EventArgs e)
    {
        if (PassValidation_Tab3())
        {
            try
            {
                int accountID = int.Parse(this.txtAct_Tab3.Text.Trim());

                string msg = "";

                if (SetAccountHDN(accountID, ref msg) == false)
                {
                    VisibleError_Tab3("Could not turn on Account HDN functionality for account " + accountID.ToString() + "<br />" + msg);
                }
                else
                {
                    VisibleMsg_Tab3("Account HDN functionality is on.");
                }

                // Relaod control
                SetControlsDefault_Tab3();
            }
            catch (Exception ex)
            {
                // Display the system generated error message.                
                VisibleError_Tab3(string.Format("An error occurred: {0}", ex.Message));
            }

        }
    }

    private void SetControlsDefault_Tab3()
    {
        this.txtAct_Tab3.Text = String.Empty;
    }

    private bool SetAccountHDN(int pAccountID, ref string pMsg)
    {
        try
        {
            String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            String cmdStr = "dbo.sp_SetAccountHDN";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;

            sqlCmd.Parameters.Add("@AccountID", SqlDbType.Int);
            sqlCmd.Parameters["@AccountID"].Value = pAccountID;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);

            sqlConn.Close();

            if (dt != null && dt.Rows.Count > 0 && dt.Rows[0][0] != null)
            {
                if (dt.Rows[0][0].ToString().Trim() == "1")
                {
                    pMsg = "";
                    return true;
                }
                else
                {
                    pMsg = dt.Rows[0][0].ToString().Trim();
                    return false;
                }
            }
            else
            {
                pMsg = "Transact-SQL error!!!";
                return false;
            }
        }
        catch (Exception ex)
        {
            pMsg = ex.ToString();
            return false;
        }
    }

    //------------------------ End Set Account HDN ----------------------------//

    //------------------------ Begin Reverse S/N from RMA ----------------------------//

    protected void btnSearch_Tab4_Click(object sender, EventArgs e)
    {
        string serialNo = this.txtSerialNo_Tab4.Text.Trim();
        if (serialNo.Length == 0)
        {
            VisibleError_Tab4("SN# is required.");
            this.btnOK_Tab4.Enabled = false;
            return;
        }
        else
        {
            
            bool deviceExists = CheckSerialNoExists(serialNo);
            if (!deviceExists)
            {
                VisibleError_Tab4("SN# is not in system. Please correct it.");
                this.btnOK_Tab4.Enabled = false;
                return;
            }
        }

        string DeleteSerialNo = this.txtSerialNo_Tab4.Text.Trim();

        var rmaInfo = (from r in adc.rma_Returns
                       join d in adc.rma_ReturnDevices on r.ReturnID equals d.ReturnID
                       where d.SerialNo == DeleteSerialNo && d.Active == true
                       orderby r.CreatedDate descending
                       select new { r, d }).FirstOrDefault();

        if (rmaInfo == null)
        {
            VisibleError_Tab4("You have not generated RMA for this device yet.");
            this.btnOK_Tab4.Enabled = false;
            return;
        }
        else
        {
            if (rmaInfo.d.Status == 4 || rmaInfo.d.Status == 7)
            {
                VisibleError_Tab4("Device was already recieved in-house.");
                this.btnOK_Tab4.Enabled = false;
                return;
            }
        }

        int rmaAccountID = rmaInfo.r.AccountID;

        //------------------------------------ Loading ------------------------------------------------------//
        String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
        String cmdStr = "dbo.sp_GetRMADeviceDetail";

        SqlConnection sqlConn = new SqlConnection(connStr);
        SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

        sqlConn.Open();

        sqlCmd.CommandType = CommandType.StoredProcedure;

        sqlCmd.Parameters.Add("@SerialNo", SqlDbType.NVarChar, 50);
        sqlCmd.Parameters["@SerialNo"].Value = this.txtSerialNo_Tab4.Text.Trim();

        SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
        DataTable dt = new DataTable();

        sqlDA.Fill(dt);

        sqlConn.Close();

        if (dt != null && dt.Rows.Count > 0 && dt.Rows[0][0] != null)
        {
            this.fvRMAInformation.DataSource = dt;
            this.fvRMAInformation.DataBind();

            this.fvRMAInformation.Visible = true;
            this.mainForm_Tab4.Visible = true;
            this.btnOK_Tab4.Enabled = true;

            this.txtAct_Tab4.Text = rmaAccountID.ToString();
            LoadUsernames_Tab4(rmaAccountID);
        }
        else
        {
            this.fvRMAInformation.Visible = false;
            this.mainForm_Tab4.Visible = false;
            this.btnOK_Tab4.Enabled = false;

            this.txtAct_Tab4.Text = "";
            LoadUsernames_Tab4(0);
        }

    }

    private bool CheckSerialNoExists(string serialNo)
    {
        if (string.IsNullOrEmpty(serialNo))
            return false;

        var deviceExists = false;

        //var isInstalink = serialNo.Substring(0, 1) == "L";
        var instalinkProduct = (from PI in idc.ProductInventories
                                join P in idc.Products
                                    on PI.ProductID equals P.ProductID
                                where PI.SerialNo == serialNo && P.ProductName.Contains("INSTALINK")
                                select PI).FirstOrDefault();

        var isInstalink = instalinkProduct != null;

        if (isInstalink)
        {            
            deviceExists = true;
            var item = radReverseType_Tab4.Items.FindByValue("2");//need to remove this 
            radReverseType_Tab4.Items.Remove(item);

        }
        else
        {
            var device = (from d in idc.DeviceInventories where d.SerialNo == serialNo select d).FirstOrDefault();
            deviceExists = device != null;
        }

        return deviceExists;
    }

    private void LoadUsernames_Tab4(int pAccountID)
    {
        var users = from r in idc.Users
                    where r.AccountID == pAccountID && r.Active == true
                    orderby r.UserName ascending
                    select new
                    {
                        r.UserID,
                        r.UserName
                    };

        this.ddlUsers_Tab4.DataSource = users;
        this.ddlUsers_Tab4.DataTextField = "UserName";
        this.ddlUsers_Tab4.DataValueField = "UserID";
        this.ddlUsers_Tab4.DataBind();

        ListItem item0 = new ListItem("", "0");
        this.ddlUsers_Tab4.Items.Insert(0, item0);
    }

    private void EnableDisableControls_Tab4()
    {
        if (this.radReverseType_Tab4.SelectedItem != null)
        {
            if (this.radReverseType_Tab4.SelectedItem.Value == "1")
            {
                ddlUsers_Tab4.Enabled = false;
                this.actIDControl.Visible = false;
                this.usernameControl.Visible = false;
            }
            else
            {
                ddlUsers_Tab4.Enabled = true;
                this.actIDControl.Visible = true;
                this.usernameControl.Visible = true;
            }
        }
    }

    protected void radReverseType_Tab4_SelectedIndexChanged(object sender, EventArgs e)
    {
        EnableDisableControls_Tab4();
    }

    private Boolean PassValidation_Tab4()
    {
        if (this.txtSerialNo_Tab4.Text.Trim().Length == 0)
        {
            VisibleError_Tab4("SN# is required.");
            return false;
        }
        else
        {
            var isSerialNoExists = CheckSerialNoExists(this.txtSerialNo_Tab4.Text.Trim());
            if (!isSerialNoExists)
            {
                VisibleError_Tab4("SN# is not in system. Please correct it.");
                return false;
            }
        }

        string DeleteSerialNo = this.txtSerialNo_Tab4.Text.Trim();

        rma_ReturnDevice returnDevice = (from r in adc.rma_Returns
                                         join d in adc.rma_ReturnDevices on r.ReturnID equals d.ReturnID
                                         where d.SerialNo == DeleteSerialNo && d.Active == true
                                         orderby r.CreatedDate descending
                                         select d).FirstOrDefault();
        if (returnDevice == null)
        {
            VisibleError_Tab4("You have not generated RMA for this device yet.");
            return false;
        }
        else
        {
            if (returnDevice.Status == 4 || returnDevice.Status == 7)
            {
                VisibleError_Tab4("Device was already recieved in-house.");
                return false;
            }
        }

        if (this.radReverseType_Tab4.SelectedItem.Value == "2")
        {
            Int32 actID;

            if (this.txtAct_Tab4.Text.Trim().Length == 0 || Int32.TryParse(this.txtAct_Tab4.Text, out actID) == false)
            {
                VisibleError_Tab4("Account# is required and must be a number.");
                return false;
            }
            else
            {
                var act = (from a in idc.Accounts where a.AccountID == actID select a).FirstOrDefault();
                if (act == null)
                {
                    VisibleError_Tab4("Account# is not in system.");
                    return false;
                }
            }

            if (this.ddlUsers_Tab4.SelectedItem.Value == "0")
            {
                VisibleError_Tab4("Please select a user to reverse.");
                return false;
            }
        }

        return true;
    }

    protected void btnOK_Tab4_Click(object sender, EventArgs e)
    {
        if (PassValidation_Tab4())
        {
            try
            {
                string DeleteSerialNo = this.txtSerialNo_Tab4.Text.Trim();

                // Create the record table
                rma_ReturnDevice returnDevice = (from r in adc.rma_Returns
                                                 join d in adc.rma_ReturnDevices on r.ReturnID equals d.ReturnID
                                                 where d.SerialNo == DeleteSerialNo && d.Active == true
                                                 orderby r.CreatedDate descending
                                                 select d).FirstOrDefault();

                if (returnDevice == null) return;

                // Deactivate rma device record.
                returnDevice.Notes = "Delete by user.";
                returnDevice.Active = false;
                returnDevice.Status = 8;
                adc.SubmitChanges();

                // Count number of devices for this return
                int returnRequestNewCount = (from r in adc.rma_Returns
                                             join d in adc.rma_ReturnDevices on r.ReturnID equals d.ReturnID
                                             where r.ReturnID == returnDevice.ReturnID && r.Active == true
                                             && d.Active == true
                                             select r).Count();

                // Count number of devices already recieved for this return
                int inventoryCount = (from ri in adc.rma_ReturnInventories
                                      where ri.Active == true && ri.ReturnID == returnDevice.ReturnID
                                      select ri).Count();

                // Do they have the same number of items?
                if (returnRequestNewCount == inventoryCount && inventoryCount != 0)
                {
                    // Requery the return and check to make sure the status is currently set to "Awaiting devices.." (1)
                    var returnRequest = (from r in adc.rma_Returns
                                         where r.ReturnID == returnDevice.ReturnID && r.Active == true
                                         select r).First();

                    if (returnRequest.Status != 5 && returnRequest.Status != 7) { returnRequest.Status = 1; }

                    // Save changes.
                    adc.SubmitChanges();
                }

                if (this.radReverseType_Tab4.SelectedItem.Value == "2")
                {
                    int actID = int.Parse(this.txtAct_Tab4.Text.Trim());
                    int userID = int.Parse(this.ddlUsers_Tab4.SelectedItem.Value);

                    //var isInstalink = DeleteSerialNo.Substring(0, 1) == "L";

                    var instalinkProduct = (from PI in idc.ProductInventories
                                            join P in idc.Products
                                                on PI.ProductID equals P.ProductID
                                            where PI.SerialNo == DeleteSerialNo && P.ProductName.Contains("INSTALINK")
                                            select PI).FirstOrDefault();

                    var isInstalink = instalinkProduct != null;

                    if (isInstalink)
                    {
                        ProductInventory device = instalinkProduct; (from d in idc.ProductInventories where d.SerialNo == DeleteSerialNo select d).FirstOrDefault();

                        AccountProduct ad = (from a in idc.AccountProducts where a.AccountID == actID && a.ProductInventoryID == device.ProductInventoryID select a).FirstOrDefault();
                        if (ad != null)
                        {
                            ad.Active = true;
                            ad.CurrentDeviceAssign = true;
                            ad.ServiceStartDate = DateTime.Now;
                        }
                    }
                    else
                    {                        
                        DeviceInventory device = (from d in idc.DeviceInventories where d.SerialNo == DeleteSerialNo select d).FirstOrDefault();

                        AccountDevice ad = (from a in idc.AccountDevices where a.AccountID == actID && a.DeviceID == device.DeviceID select a).FirstOrDefault();
                        if (ad != null)
                        {
                            ad.Active = true;
                            ad.CurrentDeviceAssign = true;
                            ad.ServiceStartDate = DateTime.Now;
                        }

                        UserDevice ud = (from u in idc.UserDevices where u.UserID == userID && u.DeviceID == device.DeviceID select u).FirstOrDefault();
                        if (ud != null)
                        {
                            ud.Active = true;
                            ud.DeactivateDate = (DateTime?)null;

                            // Re-assign FirstReadID = lastInitReadID, so the device will not be required to initialize.
                            int lastInitReadID = (from r in idc.UserDeviceReads
                                                  where r.DeviceID == device.DeviceID && r.AccountID == actID
                                                  && r.InitRead == true && r.HasAnomaly == false
                                                  orderby r.ExposureDate descending
                                                  select r.RID).FirstOrDefault();
                            if (lastInitReadID != null && lastInitReadID > 0)
                            {
                                device.FirstReadID = lastInitReadID;
                            }

                        }
                    }
                    

                    // Save changes.
                    idc.SubmitChanges();
                }

                VisibleMsg_Tab4("Reverse S/N from RMA is completed.");

                // Reload control
                SetControlsDefault_Tab4();
            }
            catch (Exception ex)
            {
                // Display the system generated error message.                
                VisibleError_Tab4(string.Format("An error occurred: {0}", ex.Message));
            }
        }
    }

    private void SetControlsDefault_Tab4()
    {
        this.txtSerialNo_Tab4.Text = String.Empty;

        if (this.radReverseType_Tab4.SelectedItem != null)
        {
            this.radReverseType_Tab4.SelectedItem.Value = "1";
        }

        EnableDisableControls_Tab4();

        this.fvRMAInformation.Visible = false;
        this.mainForm_Tab4.Visible = false;
        this.btnOK_Tab4.Enabled = false;
    }

    public bool DisplayRMAinfo(string ReturnTypeID)
    {
        if (ReturnTypeID == "3")
        {
            return true;
        }
        return false;
    }

    public string DisplayBillingAddress(string strOrderID)
    {
        string BillingAddressInfo = "";

        // Create the record table
        var BillShip = (from a in idc.Orders
                        join b in idc.Locations
                        on a.LocationID equals b.LocationID
                        join c in idc.States
                        on b.BillingStateID equals c.StateID
                        join d in idc.Countries
                        on b.BillingCountryID equals d.CountryID
                        where a.OrderID == int.Parse(strOrderID)
                        select new { b, c.StateAbbrev, d.CountryName }).FirstOrDefault();
        if (BillShip != null)
        {
            BillingAddressInfo = "<b><u>Replacement Bill To:</u></b></br>";
            BillingAddressInfo += BillShip.b.BillingCompany + "</br>";
            BillingAddressInfo += BillShip.b.BillingFirstName + " " + BillShip.b.BillingLastName + "</br>";
            BillingAddressInfo += BillShip.b.BillingAddress1 + "</br>";
            BillingAddressInfo += BillShip.b.BillingAddress2 + "</br>";
            BillingAddressInfo += BillShip.b.BillingAddress3 + "</br>";
            BillingAddressInfo += BillShip.b.BillingCity + ", "
                + BillShip.StateAbbrev + " " + BillShip.b.BillingPostalCode
                + " " + BillShip.CountryName + "</br>";
        }
        else
            BillingAddressInfo = "";

        return BillingAddressInfo;
    }

    public string DisplayShippingAddress(string strOrderID)
    {
        string ShippingAddressInfo = "";

        // Create the record table
        var BillShip = (from a in idc.Packages
                        join c in idc.States
                        on a.StateID equals c.StateID
                        join d in idc.Countries
                        on a.CountryID equals d.CountryID
                        where a.OrderID == int.Parse(strOrderID)
                        select new { a, c.StateAbbrev, d.CountryName }).FirstOrDefault();
        if (BillShip != null)
        {
            ShippingAddressInfo = "<b><u>Replacement Ship To:</u></b></br>";
            ShippingAddressInfo += BillShip.a.Company + "</br>";
            ShippingAddressInfo += BillShip.a.FirstName + " " + BillShip.a.LastName + "</br>";
            ShippingAddressInfo += BillShip.a.Address1 + "</br>";
            ShippingAddressInfo += BillShip.a.Address2 + "</br>";
            ShippingAddressInfo += BillShip.a.Address3 + "</br>";
            ShippingAddressInfo += BillShip.a.City + ", "
                + BillShip.StateAbbrev + " " + BillShip.a.PostalCode + " " +
                BillShip.CountryName + "</br>";
        }
        else
            ShippingAddressInfo = "";

        return ShippingAddressInfo;
    }

    //------------------------ End Reverse S/N from RMA ----------------------------//

    //------------------------ Begin Assign/Transfer Devices to New Account ----------------------------//

    protected void radLinkType_Tab6_SelectedIndexChanged(object sender, EventArgs e)
    {
        bool isActLock = false;
        Int32 actID;

        if (this.radLinkType_Tab6.SelectedItem != null)
        {
            if (this.radLinkType_Tab6.SelectedItem.Value == "1")
            {
                this.fromAccount.Visible = false;
            }
            else
            {
                this.fromAccount.Visible = true;
            }

            if (Int32.TryParse(this.txtToAccount_Tab6.Text, out actID))
            {
                LoadLocations_Tab6(actID);

                isActLock = (from a in idc.Accounts
                             where a.AccountID == actID
                             select a.LockLocation).FirstOrDefault();

                if (this.radLinkType_Tab6.SelectedItem.Value == "1" && isActLock == false)
                {
                    this.toLocation.Visible = false;
                }
                else
                {
                    this.toLocation.Visible = true;
                }
            }
            else
            {
                LoadLocations_Tab6(0);
                this.toLocation.Visible = false;
            }

        }
    }

    protected void txtToAccount_Tab6_TextChanged(object sender, EventArgs e)
    {
        Int32 actID = 0;
        bool isActLock = false;

        if (Int32.TryParse(this.txtToAccount_Tab6.Text, out actID))
        {
            LoadLocations_Tab6(actID);

            isActLock = (from a in idc.Accounts
                         where a.AccountID == actID
                         select a.LockLocation).FirstOrDefault();

            if (this.radLinkType_Tab6.SelectedItem.Value == "1" && isActLock == false)
            {
                this.toLocation.Visible = false;
            }
            else
            {
                this.toLocation.Visible = true;
            }
        }
        else
        {
            LoadLocations_Tab6(0);
            this.toLocation.Visible = false;
        }
    }

    private Boolean PassValidation_Tab6()
    {

        Int32 actID;

        if (this.txtSerialNo_Tab6.Text.Trim().Length == 0)
        {
            VisibleError_Tab6("SN# is required.");
            return false;
        }
        else
        {
            List<string> unKnownSN = new List<string>();
            List<string> serialNoList = new List<string>();
            serialNoList.AddRange(this.txtSerialNo_Tab6.Text.Split(new string[] { "\n", "\r\n", ",", ";", "\"", " " }, StringSplitOptions.RemoveEmptyEntries));

            foreach (string serialNo in serialNoList)
            {
                var device = (from d in idc.DeviceInventories where d.SerialNo == serialNo select d).FirstOrDefault();
                if (device == null)
                {
                    unKnownSN.Add(serialNo);
                }
            }

            if (unKnownSN.Count > 0)
            {
                string listOfUnknownSN = string.Join(", ", unKnownSN);
                VisibleError_Tab6("The following SN# are not in system. Please correct it. <br /> <p>" + listOfUnknownSN + "</p>");
                return false;
            }
        }
       

        if (this.radLinkType_Tab6.SelectedItem.Value == "2")
        {
            if (this.txtFromAccount_Tab6.Text.Trim().Length == 0 || Int32.TryParse(this.txtFromAccount_Tab6.Text, out actID) == false)
            {
                VisibleError_Tab6("From Act# is required and must be a number.");
                return false;
            }
            else
            {
                var act = (from a in idc.Accounts where a.AccountID == actID select a).FirstOrDefault();
                if (act == null)
                {
                    VisibleError_Tab3("From Act# is not in system.");
                    return false;
                }
            }
        }

        if (this.txtToAccount_Tab6.Text.Trim().Length == 0 || Int32.TryParse(this.txtToAccount_Tab6.Text, out actID) == false)
        {
            VisibleError_Tab6("To Act# is required and must be a number.");
            return false;
        }
        else
        {
            var act = (from a in idc.Accounts where a.AccountID == actID select a).FirstOrDefault();
            if (act == null)
            {
                VisibleError_Tab3("To Act# is not in system.");
                return false;
            }
        }

        if ((toLocation.Visible || this.radLinkType_Tab6.SelectedItem.Value == "2")
            && (this.ddlToLocation_Tab6.SelectedIndex <= 0 || this.ddlToLocation_Tab6.SelectedItem == null || this.ddlToLocation_Tab6.SelectedItem.Value == "0"))
        {
            VisibleError_Tab6("To Location is required.");
            return false;
        }

        return true;
    }

    private void LoadLocations_Tab6(int pAccountID)
    {
        var locs = from r in idc.Locations
                   where r.AccountID == pAccountID
                   orderby r.LocationName ascending
                   select new
                   {
                       r.LocationID,
                       r.LocationName
                   };

        this.ddlToLocation_Tab6.DataSource = locs;
        this.ddlToLocation_Tab6.DataTextField = "LocationName";
        this.ddlToLocation_Tab6.DataValueField = "LocationID";
        this.ddlToLocation_Tab6.DataBind();

        ListItem item0 = new ListItem("", "0");
        this.ddlToLocation_Tab6.Items.Insert(0, item0);
    }

    protected void btnProcess_Tab6_Click(object sender, EventArgs e)
    {
        if (PassValidation_Tab6())
        {
            try
            {
                string msg = "";
                List<string> errorDevices = new List<string>();
                int isLinkOnly = this.radLinkType_Tab6.SelectedItem.Value == "1" ? 1 : 0;
                int toAccountID = int.Parse(this.txtToAccount_Tab6.Text.Trim());
                int fromAccountID = this.txtFromAccount_Tab6.Text.Trim().Length == 0 ? 0 : int.Parse(this.txtFromAccount_Tab6.Text.Trim());
                int toLocationID = int.Parse(this.ddlToLocation_Tab6.SelectedItem.Value);

                List<string> serialNoList = new List<string>();
                serialNoList.AddRange(this.txtSerialNo_Tab6.Text.Split(new string[] { "\n", "\r\n", ",", ";", "\"", " " }, StringSplitOptions.RemoveEmptyEntries));

                foreach (string serialNo in serialNoList)
                {
                    msg = "";
                    if (LinkDevices(serialNo, isLinkOnly, fromAccountID, toAccountID, toLocationID, ref msg) == false)
                    {
                        errorDevices.Add(serialNo);
                    }
                }

                if (errorDevices.Count > 0)
                {
                    string listOfErrorDevices = string.Join(", ", errorDevices);

                    VisibleError_Tab6("The below devices could not be done. Please see IT for help. <br/> <p>" + listOfErrorDevices + "</p>");
                }
                else
                {
                    VisibleMsg_Tab6("Link Device is completed.");
                }

                // Reloads controls
                SetControlsDefault_Tab6();
            }
            catch (Exception ex)
            {
                // Display the system generated error message.                
                VisibleError_Tab6(string.Format("An error occurred: {0}", ex.Message));
            }

        }
    }

    private void SetControlsDefault_Tab6()
    {
        if (this.radLinkType_Tab6.SelectedItem != null)
        {
            this.radLinkType_Tab6.SelectedItem.Value = "1";
        }
        this.txtSerialNo_Tab6.Text = string.Empty;
        this.txtToAccount_Tab6.Text = string.Empty;
        this.txtFromAccount_Tab6.Text = string.Empty;
        this.ddlToLocation_Tab6.Items.Clear();
        this.ddlToLocation_Tab6.DataSource = null;
        this.ddlToLocation_Tab6.DataBind();

        radLinkType_Tab6_SelectedIndexChanged(null, null);
    }

    private bool LinkDevices(string pSerialNo, int isLinkOnly, int pFromAccountID, int pToAccountID, int pLocationID, ref string pMsg)
    {
        try
        {
            String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            String cmdStr = "dbo.sp_LinkDevices";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;

            sqlCmd.Parameters.Add("@SerialNo", SqlDbType.NVarChar, 10);
            sqlCmd.Parameters["@SerialNo"].Value = pSerialNo;

            sqlCmd.Parameters.Add("@IsLinkOnly", SqlDbType.Int);
            sqlCmd.Parameters["@IsLinkOnly"].Value = isLinkOnly;

            sqlCmd.Parameters.Add("@PrevAccountID", SqlDbType.Int);
            sqlCmd.Parameters["@PrevAccountID"].Value = pFromAccountID;

            sqlCmd.Parameters.Add("@AcctID", SqlDbType.Int);
            sqlCmd.Parameters["@AcctID"].Value = pToAccountID;

            sqlCmd.Parameters.Add("@MoveToLocID", SqlDbType.Int);
            sqlCmd.Parameters["@MoveToLocID"].Value = pLocationID;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);

            sqlConn.Close();

            if (dt != null && dt.Rows.Count > 0 && dt.Rows[0][0] != null)
            {
                if (dt.Rows[0][0].ToString().Trim() == "1")
                {
                    pMsg = "";
                    return true;
                }
                else
                {
                    pMsg = dt.Rows[0][0].ToString().Trim();
                    return false;
                }
            }
            else
            {
                pMsg = "Transact-SQL error!!!";
                return false;
            }
        }
        catch (Exception ex)
        {
            pMsg = ex.ToString();
            return false;
        }
    }

    //------------------------ End Assign/Transfer Devices to New Account ----------------------------//

    //------------------------ Begin Delete Order ----------------------------//
    /// <summary>
    /// Populates FormView with (General) Order Information.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnSearchOrderNumber_Click(object sender, EventArgs e)
    {
        int orderID = 0;

        if (this.txtEnterOrderNumber.Text.Trim() == "") return;

        int.TryParse(this.txtEnterOrderNumber.Text.Trim(), out orderID);

        Order orderRecord = (from o in idc.Orders
                             where o.OrderID == orderID && o.OrderStatusID != 10
                             select o).FirstOrDefault();

        if (orderRecord == null)
        {
            // Error Message & Disable FormView, GridViews, and Delete Order Button.
            VisibleError_Tab5("The Order # you entered has either been deleted or does not exist.");
            this.fvOrderInformation.Visible = false;
            this.gvOrderDetails.Visible = false;
            this.gvPackageDetails.Visible = false;
            this.btnDeleteOrder.Enabled = false;
        }
        else
        {
            // Modified 09/24/2015-09/25/2015 by Anuradha Nandi
            // Per Craig Yurosko these are the following requirements for Deleting Orders;
            // 1.) IC Care (BrandSourceID = 3), CREATED - Needs PO (OrderStatusID = 1), and RENEWAL (OrderType = "RENEWAL") --> OK TO DELETE.
            // 2.) Mirion (BrandSourceID = 1), CREATED - NO PO (OrderStatusID = 2), and RENEWAL (OrderType = "RENEWAL") --> CHECK MAS FIRST.
            //     a.) Pop-Up/Modal that requests User to check MAS to make sure if it is OK to delete the order first.
            //     b.) User then clicks YES/NO and proceeds accordingly.
            // 3.) Mirion or IC Care, RENEWAL or Otherwise, and FULFILLMENT (OrderStatusID = 3) --> OK TO DELETE.
            // 4.) SHIPPED (OrderStatusID = 4) --> DO NOT DELETE.
            // 5.) PROCESSED (OrderStatusID = 5) --> DO NOT DELETE.
            // 6.) PICKED (OrderStatusID = 6) --> DO NOT DELETE.
            // 7.) PACKED (OrderStatusID = 7) --> DO NOT DELETE.
            // 8.) RENEWAL HOLD (OrderStatusID = 8) --> OK TO DELETE.

            // Modified 10/05/2015 by Anuradha Nandi
            // The requirements have changed - allow for deletion for all IC CARE and MIRION Orders.
            // The Business Rules have not changed for the following; SHIPPED, PROCESSED, PICKED, and PACKED (User cannot deleted them directly in Portal).
            // The unique Alert/Additional Instructions Message will be handled on the btnDeleteOrder_Click function.

            string orderType = orderRecord.OrderType;
            int orderStatusID = orderRecord.OrderStatusID;
            int? brandSourceID = ((orderRecord.Account.BrandSourceID.HasValue) ? orderRecord.Account.BrandSourceID.Value : 0);

            bool allowOrderDeletion = false;
            string alertMessage = "";

            if (orderStatusID == 1 && brandSourceID == 3)
            {
                allowOrderDeletion = true;
            }

            if (orderStatusID == 2 && brandSourceID == 2)
            {
                allowOrderDeletion = true;
            }

            if (orderStatusID == 3)
            {
                allowOrderDeletion = true;
            }

            if (orderStatusID == 4 || orderStatusID == 5 || orderStatusID == 6 || orderStatusID == 7)
            {
                allowOrderDeletion = false;
                alertMessage = string.Format("Order #{0} is currently in the status of {1} and cannot be canceled. Please contact I.T. for further assistance.", orderID.ToString(), orderRecord.OrderStatus.OrderStatusName.ToUpper());
            }

            if (orderStatusID == 8)
            {
                allowOrderDeletion = true;
            }
            //If this order contains ID+ or ID2 and order is in fulfillment do not allow cancel
            if(orderStatusID > 2 && orderRecord.OrderDetails.Any(ord => ord.Product.ProductGroupID > 9))
            {
                allowOrderDeletion = false;
                alertMessage = string.Format("Order #{0} is currently in the status of {1} and cannot be canceled.", orderID.ToString(), orderRecord.OrderStatus.OrderStatusName.ToUpper());
            }
            if (orderStatusID >= 7)
            {
                allowOrderDeletion = false;
                alertMessage = "Cannot cancel instadose orders that have been processed or fulfilled.";
            }
            // Based on the results of the allowOrderDeletion variable above do the following;

            if (allowOrderDeletion == false)
            {
                // Display Alert Message & Disable FormView, GridViews, and Delete Order Button.
                VisibleError_Tab5(alertMessage);
                this.fvOrderInformation.Visible = false;
                this.gvOrderDetails.Visible = false;
                this.gvPackageDetails.Visible = false;
                this.btnDeleteOrder.Enabled = false;
            }
            else
            {
                // Create the SQL Connection from the connection string in the web.config
                SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
                    ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

                // Create the SQL Command.
                SqlCommand sqlCmd = new SqlCommand();
                sqlCmd.CommandType = CommandType.StoredProcedure;

                // Using the sp_if_GetOrderHeaderByNo.
                string sqlQuery = "sp_if_GetOrderHeaderByNo";

                sqlCmd.Parameters.Add(new SqlParameter("@OrderNo", orderID));

                // Pass the SQL Query String to the SQL Command.
                sqlCmd.Connection = sqlConn;
                bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
                if (!openConn) sqlCmd.Connection.Open();
                sqlCmd.CommandText = sqlQuery;
                SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

                // Create a Date Table to hold the contents/results.
                DataTable dtOrderInformationTable = new DataTable();

                dtOrderInformationTable = new DataTable("Get Order Information");

                // Create the columns for the DataTable.
                dtOrderInformationTable.Columns.Add("OrderNo", Type.GetType("System.Int32"));
                dtOrderInformationTable.Columns.Add("AccountNo", Type.GetType("System.Int32"));
                dtOrderInformationTable.Columns.Add("AccountName", Type.GetType("System.String"));
                dtOrderInformationTable.Columns.Add("Status", Type.GetType("System.String"));
                dtOrderInformationTable.Columns.Add("Source", Type.GetType("System.String"));
                dtOrderInformationTable.Columns.Add("CreatedDate", Type.GetType("System.DateTime"));
                dtOrderInformationTable.Columns.Add("TrackingNumber", Type.GetType("System.String"));
                dtOrderInformationTable.Columns.Add("PaymentMethod", Type.GetType("System.String"));
                dtOrderInformationTable.Columns.Add("ShipDate", Type.GetType("System.String"));
                dtOrderInformationTable.Columns.Add("PONumber", Type.GetType("System.String"));
                dtOrderInformationTable.Columns.Add("Coupon", Type.GetType("System.String"));
                dtOrderInformationTable.Columns.Add("CurrencyCode", Type.GetType("System.String"));
                dtOrderInformationTable.Columns.Add("OrderShipping", Type.GetType("System.Decimal"));
                dtOrderInformationTable.Columns.Add("Referral", Type.GetType("System.String"));
                dtOrderInformationTable.Columns.Add("OrderTax", Type.GetType("System.Decimal"));
                dtOrderInformationTable.Columns.Add("SoftTraxIntegration", Type.GetType("System.Boolean"));
                dtOrderInformationTable.Columns.Add("MiscCredit", Type.GetType("System.Decimal"));
                dtOrderInformationTable.Columns.Add("OrderSubtotal", Type.GetType("System.Decimal"));
                dtOrderInformationTable.Columns.Add("SpecialInstructions", Type.GetType("System.String"));
                dtOrderInformationTable.Columns.Add("OrderTotal", Type.GetType("System.Decimal"));
                // Formatted Currency amounts.
                dtOrderInformationTable.Columns.Add("FormattedShipping", Type.GetType("System.String"));
                dtOrderInformationTable.Columns.Add("FormattedTax", Type.GetType("System.String"));
                dtOrderInformationTable.Columns.Add("FormattedCredit", Type.GetType("System.String"));
                dtOrderInformationTable.Columns.Add("FormattedSubtotal", Type.GetType("System.String"));
                dtOrderInformationTable.Columns.Add("FormattedTotal", Type.GetType("System.String"));

                while (sqlDataReader.Read())
                {
                    DataRow drow = dtOrderInformationTable.NewRow();

                    decimal shipping = 0;
                    decimal tax = 0;
                    decimal credit = 0;
                    decimal subtotal = 0;
                    decimal total = 0;

                    decimal.TryParse(sqlDataReader["OrderShipping"].ToString(), out shipping);
                    decimal.TryParse(sqlDataReader["OrderTax"].ToString(), out tax);
                    decimal.TryParse(sqlDataReader["MiscCredit"].ToString(), out credit);
                    decimal.TryParse(sqlDataReader["OrderSubtotal"].ToString(), out subtotal);
                    decimal.TryParse(sqlDataReader["OrderTotal"].ToString(), out total);

                    string currencyCode = sqlDataReader["CurrencyCode"].ToString();

                    // Fill row details.
                    drow["OrderNo"] = sqlDataReader["OrderNo"];
                    drow["AccountNo"] = sqlDataReader["AccountNo"];
                    drow["AccountName"] = sqlDataReader["AccountName"];
                    drow["Status"] = sqlDataReader["Status"];
                    drow["Source"] = sqlDataReader["Source"];
                    drow["CreatedDate"] = sqlDataReader["CreatedDate"];
                    drow["TrackingNumber"] = sqlDataReader["TrackingNumber"];
                    drow["PaymentMethod"] = sqlDataReader["PaymentMethod"];
                    drow["ShipDate"] = sqlDataReader["ShipDate"];
                    drow["PONumber"] = sqlDataReader["PONumber"];
                    drow["Coupon"] = sqlDataReader["Coupon"];
                    drow["FormattedShipping"] = Currencies.CurrencyToString(shipping, currencyCode);
                    drow["Referral"] = sqlDataReader["Referral"];
                    drow["FormattedTax"] = Currencies.CurrencyToString(tax, currencyCode);
                    drow["SoftTraxIntegration"] = sqlDataReader["SoftTraxIntegration"];
                    drow["FormattedCredit"] = Currencies.CurrencyToString(credit, currencyCode);
                    drow["FormattedSubtotal"] = Currencies.CurrencyToString(subtotal, currencyCode);
                    drow["SpecialInstructions"] = sqlDataReader["SpecialInstructions"];
                    drow["FormattedTotal"] = Currencies.CurrencyToString(total, currencyCode);

                    // Add rows to DataTable.
                    dtOrderInformationTable.Rows.Add(drow);
                }

                // Assign values to webpage controls (labels).
                this.fvOrderInformation.DataSource = dtOrderInformationTable;
                this.fvOrderInformation.DataBind();

                this.fvOrderInformation.Visible = true;
                this.gvOrderDetails.Visible = true;
                this.gvPackageDetails.Visible = true;
                this.btnDeleteOrder.Enabled = true;

                // Order Details GridView.
                BindOrderDetailsGridView(orderID);

                // Package Details GridView.
                BindPackageDetailsGridView(orderID);

                sqlConn.Close();
                sqlDataReader.Close();
            }
        }
    }

    public string YesNo(object boolean)
    {
        try
        {
            return Convert.ToBoolean(boolean) ? "Yes" : "No";
        }
        catch { return "No"; }
    }

    /// <summary>
    /// Populate Order Details GridView.
    /// </summary>
    private void BindOrderDetailsGridView(int orderid)
    {
        string price = "";
        string amount = "";

        //find currency code.
        string currencyCode = (from o in idc.Orders
                               where o.OrderID == orderid
                               select o.CurrencyCode).FirstOrDefault();

        if (currencyCode == null) return;

        // Create the SQL Connection from the connection string in the web.config.
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.StoredProcedure;

        sqlCmd.Parameters.Add(new SqlParameter("@OrderNo", orderid));

        // Pass the query string to the command

        sqlCmd.Connection = sqlConn;

        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        //**********************************************************************************
        // 8/2013 WK - AccountDiscounts is in place but not used since it will NOT
        //             be accurate.   Product discounts need to be included in OrderDetails
        //             if it is be used.
        //**********************************************************************************
        sqlCmd.CommandText = "sp_if_GetOrderDetailsByOrderNo";

        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        DataTable dtOrderDetailsTable = new DataTable();

        // Create the Order Details DataTable to hold the contents of the Order Details.
        dtOrderDetailsTable = new DataTable("Order Details Table");

        // Create the columns for the Order Details DataTable.
        dtOrderDetailsTable.Columns.Add("SKU", Type.GetType("System.String"));
        dtOrderDetailsTable.Columns.Add("ProductName", Type.GetType("System.String"));
        dtOrderDetailsTable.Columns.Add("ProductVariant", Type.GetType("System.String"));
        dtOrderDetailsTable.Columns.Add("Price", Type.GetType("System.String"));
        dtOrderDetailsTable.Columns.Add("Quantity", Type.GetType("System.Int32"));
        dtOrderDetailsTable.Columns.Add("LineTotal", Type.GetType("System.String"));

        while (sqlDataReader.Read())
        {
            // Create a new review order row.
            DataRow dr = dtOrderDetailsTable.NewRow();

            // Fill row details.
            dr["SKU"] = sqlDataReader["SKU"].ToString();
            dr["ProductName"] = sqlDataReader["ProductName"].ToString();
            dr["ProductVariant"] = sqlDataReader["ProductVariant"].ToString();
            //Format price.
            price = sqlDataReader["Price"].ToString();
            price = Currencies.CurrencyToString(Convert.ToDecimal(price), currencyCode);
            dr["Price"] = price;
            dr["Quantity"] = sqlDataReader["Quantity"].ToString();
            //Format each product amount.
            amount = sqlDataReader["LineTotal"].ToString();
            amount = Currencies.CurrencyToString(Convert.ToDecimal(amount), currencyCode);
            dr["LineTotal"] = amount;

            // Add row to the data table.
            dtOrderDetailsTable.Rows.Add(dr);
        }

        this.gvOrderDetails.Columns[3].HeaderText = "Unit Price".PadLeft(10);

        this.gvOrderDetails.DataSource = dtOrderDetailsTable;
        this.gvOrderDetails.DataBind();

        sqlConn.Close();
        sqlDataReader.Close();
    }

    /// <summary>
    /// Populate Package Details GridView.
    /// </summary>
    /// <param name="orderid"></param>
    private void BindPackageDetailsGridView(int orderid)
    {
        // Create the SQL Connection from the connection string in the web.config
        SqlConnection sqlConn = new SqlConnection(ConfigurationManager.ConnectionStrings
            ["Instadose.Properties.Settings.InsConnectionString"].ConnectionString);

        // Create the SQL Command.
        SqlCommand sqlCmd = new SqlCommand();
        sqlCmd.CommandType = CommandType.Text;

        string sqlQuery = "SELECT * FROM Packages WHERE OrderID = " + orderid;

        // Pass the SQL Query String to the SQL Command.
        sqlCmd.Connection = sqlConn;
        bool openConn = (sqlCmd.Connection.State == ConnectionState.Open);
        if (!openConn) sqlCmd.Connection.Open();
        sqlCmd.CommandText = sqlQuery;
        SqlDataReader sqlDataReader = sqlCmd.ExecuteReader();

        // Create a Date Table to hold the contents/results.
        DataTable dtPackageDetailsDateTable = new DataTable();

        dtPackageDetailsDateTable = new DataTable("Get Package Details");

        // Create the columns for the DataTable.
        dtPackageDetailsDateTable.Columns.Add("PackageID", Type.GetType("System.Int32"));
        dtPackageDetailsDateTable.Columns.Add("OrderID", Type.GetType("System.Int32"));
        dtPackageDetailsDateTable.Columns.Add("TrackingNumber", Type.GetType("System.String"));
        dtPackageDetailsDateTable.Columns.Add("ShipDate", Type.GetType("System.DateTime"));

        while (sqlDataReader.Read())
        {
            DataRow drow = dtPackageDetailsDateTable.NewRow();

            // Fill row details.
            drow["PackageID"] = sqlDataReader["PackageID"];
            drow["OrderID"] = sqlDataReader["OrderID"];
            drow["TrackingNumber"] = sqlDataReader["TrackingNumber"];
            drow["ShipDate"] = sqlDataReader["ShipDate"];

            // Add rows to DataTable.
            dtPackageDetailsDateTable.Rows.Add(drow);
        }

        // Bind the results to the GridView.
        this.gvPackageDetails.DataSource = dtPackageDetailsDateTable;
        this.gvPackageDetails.DataBind();

        sqlConn.Close();
        sqlDataReader.Close();
    }

    /// <summary>
    /// Populates Packing/Shipping Information GridView.
    /// Tracking Information, Shipped Date, and Shipping Address.
    /// </summary>
    /// <param name="packageid"></param>
    /// <returns></returns>
    public string GeneratePackShippingInfo(string packageid)
    {
        string returnPackingString = "";

        var packingInfo = (from b in idc.Packages
                           join c in idc.States
                           on b.StateID equals c.StateID
                           join d in idc.Countries
                           on b.CountryID equals d.CountryID
                           where b.PackageID == int.Parse(packageid)
                           select new { b, c, d }).FirstOrDefault();

        if (packingInfo == null) return "";

        returnPackingString = (packingInfo.b.Company != null && packingInfo.b.Company != "") ? packingInfo.b.Company + "</br>" : "";
        returnPackingString += packingInfo.b.FirstName + " ";
        returnPackingString += packingInfo.b.LastName + "</br>";
        returnPackingString += packingInfo.b.Address1;
        returnPackingString += (packingInfo.b.Address2 != null && packingInfo.b.Address2 != "") ?
                     ", " + packingInfo.b.Address2 : "";
        returnPackingString += (packingInfo.b.Address3 != null && packingInfo.b.Address3 != "") ?
                     ", " + packingInfo.b.Address3 : "";
        returnPackingString += "</br>";
        returnPackingString += packingInfo.b.City + ", ";
        returnPackingString += packingInfo.c.StateAbbrev + " ";
        returnPackingString += packingInfo.b.PostalCode + "</br>";
        returnPackingString += packingInfo.d.CountryName + "</br>";

        return returnPackingString;
    }

    /// <summary>
    /// Populates Packing/Shipping Information GridView.
    /// List of SerialNumbers (Badges/Devices) that were part of the Order.
    /// </summary>
    /// <param name="packageid"></param>
    /// <returns></returns>
    public string GeneratePackSerialNo(string packageid)
    {
        string returnSerialNumberList = "";

        var serialNumbers = (from a in idc.PackageOrderDetails
                             join b in idc.DeviceInventories
                             on a.DeviceID equals b.DeviceID
                             where a.PackageID == int.Parse(packageid)
                             select b.SerialNo).ToList();

        if (serialNumbers.Count == 0) return "";

        returnSerialNumberList += "Total: " + serialNumbers.Count.ToString() + "</br>";

        foreach (var v in serialNumbers)
        {
            returnSerialNumberList += "#" + v.ToString() + ", ";
        }
        int lastIndex = returnSerialNumberList.LastIndexOf(",");
        if (lastIndex > 0)
            returnSerialNumberList = returnSerialNumberList.Substring(0, lastIndex);

        return returnSerialNumberList;
    }

    protected void btnDeleteOrder_Click(object sender, EventArgs e)
    {
        int orderID = 0;

        if (int.TryParse(this.txtEnterOrderNumber.Text.Trim(), out orderID))
        {
            try
            {
                // Modified 09/29/2015 by Anuradha Nandi
                // Additional Instructions have been added to the Success Message once the Order has been deleted.
                // To be followed-up/through by User who is deleting the order.

                // GET OrderType, OrderStatusID, and BrandSourceID.
                var getOrderDetails = (from o in idc.Orders
                                       where o.OrderID == orderID
                                       select o).FirstOrDefault();
                if (getOrderDetails == null)
                    throw new Exception(string.Format("Order #{0} not found.",orderID.ToString()));

                string additionalInstructions = "";

                // SET Addition Instructions text based on the follow scenarios;
                // Modified 10/05/2015 by Anuradha Nandi
                // IF the Order is IC CARE in the Status of CREATED-NEEDS PO and Order Type is RENEWAL then no actions needs to be taken after deleting in Portal.
                // However, if the Order Type is ADDON, NEW, etc., THEN the Order can be deleted in Portal, but the User needs to make sure it is cleared/deleted in MAS200.
                // Also, update Efren German in Mfg. & Shipping of the change.
                if (getOrderDetails.OrderStatusID == 1 && getOrderDetails.BrandSourceID == 3)
                {
                    string orderTypeICCare = "";
                    orderTypeICCare = getOrderDetails.OrderType.ToUpper();

                    if (orderTypeICCare == "RENEWAL")
                    {
                        additionalInstructions = string.Format("Order #{0} is an IC CARE RENEWAL in the status of CREATED-NEEDS PO. No further action is needed.", orderID.ToString());
                    }
                    else
                    {
                        additionalInstructions = string.Format("Order #{0} is an IC CARE {1} in the status of CREATED-NEEDS PO.<br />Please make sure to delete the order in MAS200 and inform Efren German in Shipping & Manufacturing of the changes.", orderID.ToString(), orderTypeICCare);
                    }
                }

                // Modified 10/05/2015 by Anuradha Nandi
                // IF the Order is MIRION in the Status of CREATED-NO PO and Order Type is RENEWAL, ADDON, NEW, etc. 
                // THEN the Order can be deleted in Portal, but the User needs to make sure it is cleared/deleted in MAS200.
                // Also, update Efren German in Mfg. & Shipping of the change.
                if (getOrderDetails.OrderStatusID == 2 && getOrderDetails.BrandSourceID == 2)
                {
                    string orderTypeMirion = "";
                    orderTypeMirion = getOrderDetails.OrderType.ToUpper();

                    if (orderTypeMirion == "RENEWAL")
                    {
                        additionalInstructions = string.Format("Order #{0} is an MIRION RENEWAL in the status of CREATED-NO PO.<br />Please make sure to delete the order in MAS200 and inform Efren German in Shipping & Manufacturing of the changes.", orderID.ToString());
                    }
                    else
                    {
                        additionalInstructions = string.Format("Order #{0} is an MIRION {1} in the status of CREATED-NO PO.<br />Please make sure to delete the order in MAS200 and inform Efren German in Shipping & Manufacturing of the changes.", orderID.ToString(), orderTypeMirion);
                    }
                }

                if (getOrderDetails.OrderStatusID > 2 && getOrderDetails.OrderDetails.Any(ord => ord.Product.ProductGroupID > 9))
                {
                    additionalInstructions = string.Format("Order #{0} is an {1} {2} in the status of FULFILLMENT or later and cannot be canceled. No further action is needed.", orderID.ToString(), getOrderDetails.BrandSource.BrandName.ToUpper(), getOrderDetails.OrderType.ToUpper());
                    throw new Exception("Cannot cancel non-instadse 1 orders that are in the process of being fulfilled.");
                }
                else
                {
                    if (getOrderDetails.OrderStatusID == 3)
                    {
                        additionalInstructions = string.Format("Order #{0} is a {1} {2} in the status of FULFILLMENT.<br />Please make sure to delete the order in MAS200 and inform Efren German in Shipping & Manufacturing of the changes.", orderID.ToString(), getOrderDetails.BrandSource.BrandName.ToUpper(), getOrderDetails.OrderType.ToUpper());
                    }

                    if (getOrderDetails.OrderStatusID == 8)
                    {
                        additionalInstructions = string.Format("Order #{0} is an {1} {2} in the status of RENEWAL HOLD. No further action is needed.", orderID.ToString(), getOrderDetails.BrandSource.BrandName.ToUpper(), getOrderDetails.OrderType.ToUpper());
                    }

                    if (getOrderDetails.OrderStatusID >= 7)
                        throw new Exception("Cannot cancel instadose 1 orders that have been processed or fulfilled.");
                }

                List<string> tableNames = new List<string>();

                string successMessage = "The Order records has been successfully deleted. The following tables were effected; ";

                // Check each table affected by either OrderID or OrderDetailsID and delete the record.
                // This has to be done in a specific order so as to ensure no orphan records remain.

                // Modified 09/24/2015 by Thinh Do, Yong Lee, and Anuradha Nandi
                // After researching the Database Tables; RenewalLogs, RenewalThreadLog, and RenewalThreadLogDetail,
                // it was discovered that RenewalLog and RenewalThreadLogDetail has a 1:1 Relationhip, but
                // RenewalThreadLog and RenewalThreadLogDetail has a 1:N Relationship.
                // This why it could not delete the Renewal Orders properly when trying to delete all (3) related table.
                // Per Thinh Do, we will ignore deletion from the RenewalThreadLog table and only delete records from the following;
                // 1.) RenewalThreadLogDetail
                // 2.) RenewalLog

                // Cancel Order
                var orderRecords = idc.Orders.Where(o => o.OrderID == orderID && o.OrderStatusID != 10).FirstOrDefault();

                if (orderRecords != null)
                {
                    orderRecords.OrderStatusID = 10;

                    tableNames.Add("Orders"); // Add Orders table to List.
                }
                

                // Delete Renewal Thread Log Detail.
                var renewalThreadLogDetailRecords = (from rtld in idc.RenewalThreadLogDetails
                                                     join rl in idc.RenewalLogs on rtld.RenewalLogID equals rl.RenewalLogID
                                                     where rl.OrderID == orderID
                                                     select rtld).ToList();

                if (renewalThreadLogDetailRecords.Count != 0)
                {
                    idc.RenewalThreadLogDetails.DeleteAllOnSubmit(renewalThreadLogDetailRecords);

                    tableNames.Add("RenewalThreadLogDetail"); // Add RenewalThreadLogDetail table to List.
                }

                // Delete Renewal Logs.
                if (orderRecords.RenewalLogs.Count != 0)
                {
                    idc.RenewalLogs.DeleteAllOnSubmit(orderRecords.RenewalLogs);

                    tableNames.Add("RenewalLogs"); // Add RenewalLogs table to List.
                }
                
                // Delete ToMas_SO_SalesOrderDetail
                var toMas_SO_SalesOrderDetailRecords = (from sod in mdc.ToMas_SO_SalesOrderDetails where sod.SalesOrderNo == MAS.CleanString(orderID.ToString().PadLeft(7, '0'), 7) select sod).ToList();

                if (toMas_SO_SalesOrderDetailRecords.Count != 0)
                {
                    mdc.ToMas_SO_SalesOrderDetails.DeleteAllOnSubmit(toMas_SO_SalesOrderDetailRecords);

                    tableNames.Add("ToMas_SO_SalesOrderDetail"); // Add ToMas_SO_SalesOrderDetail table to List.
                }

                // Delete ToMAS_SO_SalesOrderHeader
                var toMAS_SO_SalesOrderHeaderRecords = (from soh in mdc.ToMAS_SO_SalesOrderHeaders where soh.FOB == orderID.ToString() select soh).ToList();

                if (toMAS_SO_SalesOrderHeaderRecords.Count != 0)
                {
                    mdc.ToMAS_SO_SalesOrderHeaders.DeleteAllOnSubmit(toMAS_SO_SalesOrderHeaderRecords);

                    tableNames.Add("ToMAS_SO_SalesOrderHeader"); // Add ToMAS_SO_SalesOrderHeader table to List.
                }
                
                // Delete related Cases
                var caseNotes = (from cn in idc.CaseNotes where cn.Case.OrderID == orderID select cn).ToList();
                if (orderRecords.Cases.Count != 0)
                {
                    idc.CaseNotes.DeleteAllOnSubmit(caseNotes);
                    idc.Cases.DeleteAllOnSubmit(orderRecords.Cases);

                    tableNames.Add("Cases"); // Add Cases table to List.
                }

                // Deactivate documents
                if (orderRecords.Documents.Count != 0)
                {
                    foreach (var documents in orderRecords.Documents)
                    {
                        documents.Active = false;
                    }

                    tableNames.Add("Documents"); // Add Documents table to List.
                }

                var orderUserAssignRecords = idc.OrderUserAssigns.Where(ord => ord.OrderID == orderID).ToList();
                // Delete OrderUserAssign
                if (orderUserAssignRecords.Count != 0)
                {
                    idc.OrderUserAssigns.DeleteAllOnSubmit(orderUserAssignRecords);

                    tableNames.Add("OrderUserAssigns"); // Add OrderUserAssigns table to List.
                }

                var payments = idc.Payments.Where(p => p.OrderID == orderID).ToList();
                // Delete Payments
                if (payments.Count != 0)
                {
                    idc.Payments.DeleteAllOnSubmit(payments);

                    tableNames.Add("Payments"); // Add Payments table to List.
                }

                var renewallBillingRecords = idc.RenewalBillingDevices.Where(rbd => rbd.OrderID == orderID).ToList();
                // Delete Renewal Billing Devices
                if (renewallBillingRecords.Count != 0)
                {
                    idc.RenewalBillingDevices.DeleteAllOnSubmit(renewallBillingRecords);

                    tableNames.Add("RenewalBillingDevices"); // Add RenewalBillingDevices table to List.
                }

                var scheduleBillings = idc.ScheduledBillings.Where(s => s.OrderID == orderID).ToList();
                // Delete Scheduled Billings
                if (scheduleBillings.Count != 0)
                {
                    idc.ScheduledBillings.DeleteAllOnSubmit(scheduleBillings);

                    tableNames.Add("ScheduledBillings"); // Add ScheduledBillings table to List.
                }
                
                // Delete Account Device Credits
                var accountDeviceCreditRecords = (from adc in idc.AccountDeviceCredits
                                                  where adc.OrderDetail.OrderID == orderID
                                                  select adc).ToList();

                if (accountDeviceCreditRecords.Count != 0)
                {
                    idc.AccountDeviceCredits.DeleteAllOnSubmit(accountDeviceCreditRecords);

                    tableNames.Add("AccountDeviceCredits"); // Add AccountDeviceCredits table to List.
                }

                var packages = idc.Packages.Where(p => p.OrderID == orderID).ToList();
                // Delete Packages
                if (packages.Count != 0)
                {
                    idc.Packages.DeleteAllOnSubmit(packages);
                    tableNames.Add("Packages"); // Add Packages table to List.
                }

                // Delete Package Order Details
                var packageOrderDetailRecords = idc.PackageOrderDetails.Where(pod=> pod.OrderDetail.OrderID == orderID).ToList();

                if (packageOrderDetailRecords.Count != 0)
                {
                    idc.PackageOrderDetails.DeleteAllOnSubmit(packageOrderDetailRecords);

                    tableNames.Add("PackageOrderDetails"); // Add PackageOrderDetails table to List.
                }

                // Update InstaWorkOrders, InstaWorkOrderChunks, InstaWorkOrderChunkDetails
                var instaWorkOrders = idc.InstaWorkOrders.Where(iwo => iwo.OrderNo == orderID && iwo.InstaWorkOrderType == 1).ToList(); // 1 = INSTA
                if (instaWorkOrders.Count != 0)
                {
                    foreach (var instaWorkOrder in instaWorkOrders)
                    {
                        instaWorkOrder.isComplete = true;

                        var instaWorkOrderChunks = (from a in idc.InstaWorkOrders
                                             join b in idc.InstaWorkOrderChunks
                                             on a.InstaWorkOrderID equals b.InstaWorkOrderID
                                             where a.OrderNo == orderID
                                             select b).ToList();

                        foreach (var instaWorkOrderChunk in instaWorkOrderChunks)
                        {
                            instaWorkOrderChunk.isComplete = true;
                        }

                        tableNames.Add("InstaWorkOrderChunks"); // Add InstaWorkOrderChunks table to List.

                        var instaWorkOrderChunkDetails = idc.InstaWorkOrderChunkDetails.Where(iwo => iwo.OrderNo == orderID).ToList();
                        foreach (var instaWorkOrderChunkDetail in instaWorkOrderChunkDetails)
                        {
                            instaWorkOrderChunkDetail.isCompleted = true;
                        }

                        tableNames.Add("InstaWorkOrderChunkDetails"); // Add InstaWorkOrderChunkDetails table to List.

                    }

                    tableNames.Add("InstaWorkOrders"); // Add InstaWorkOrders table to List.
                }

                string listOfTableNames = string.Join(", ", tableNames);

                idc.SubmitChanges();

                VisibleMsg_Tab5(successMessage + listOfTableNames + "<br /><u>Additional Instructions</u>: " + additionalInstructions);

                if (Page.IsPostBack)
                {
                    SetControlsDefault_Tab5();
                }
            }
            catch (Exception ex)
            {
                VisibleError_Tab5(ex.Message);
                return;
            }
        }
        else
        {
            VisibleError_Tab5("Please enter a valid Order #.");
            return;
        }
    }

    private void SetControlsDefault_Tab5()
    {
        // Reset OrderID/Number Search TextBox.
        this.txtEnterOrderNumber.Text = string.Empty;

        // Make all information invisible.
        this.fvOrderInformation.Visible = false;
        this.gvOrderDetails.Visible = false;
        this.gvPackageDetails.Visible = false;
        this.btnDeleteOrder.Enabled = false;
    }

    //------------------------ End Delete Order ----------------------------//       

    //-- Restrict Online Access --//
    protected void txtAct_Tab7_TextChanged(object sender, EventArgs e)
    {
        Int32 actID_Tab7 = 0;

        if (Int32.TryParse(this.txtAct_Tab7.Text, out actID_Tab7))
            LoadOnlineAccessRestriction_Tab7(actID_Tab7);
        else
            LoadOnlineAccessRestriction_Tab7(0);
    }

    protected void btnRestrict_Tab7_Click(object sender, EventArgs e)
    {
        if (PassValidation_Tab7())
        {
            try
            {
                int accountID = int.Parse(this.txtAct_Tab7.Text.Trim());

                bool restrictAccess = RestrictOnlineAccess(accountID, chkRestrictOnlineAccess_Tab7.Checked, out string errorMessage);

                if (restrictAccess)
                {
                    VisibleMsg_Tab7("Online Access Restriction is updated.");
                }
                else
                {
                    VisibleError_Tab7(string.Format("An error occured: {0}", errorMessage));
                }

                SetControlsDefault_Tab7();
            }
            catch (Exception ex)
            {
                VisibleError_Tab7(string.Format("An error occured: {0}", ex.Message));
            }
        }
    }

    private Boolean PassValidation_Tab7()
    {

        Int32 actID;
        if (this.txtAct_Tab7.Text.Trim().Length == 0 || Int32.TryParse(this.txtAct_Tab7.Text, out actID) == false)
        {
            VisibleError_Tab7("Account# is required and must be a number.");
            return false;
        }
        else
        {
            var act = (from a in idc.Accounts where a.AccountID == actID select a).FirstOrDefault();
            if (act == null)
            {
                VisibleError_Tab7("Account# is not in system.");
                return false;
            }
        }

        return true;
    }

    private void LoadOnlineAccessRestriction_Tab7(int pAccountID)
    {
        var account = idc.Accounts.Where(a => a.AccountID == pAccountID).Select(a => new { a.AccountID, a.RestrictOnlineAccess }).FirstOrDefault();

        if (account != null)
            chkRestrictOnlineAccess_Tab7.Checked = account.RestrictOnlineAccess == true;
    }

    private bool RestrictOnlineAccess(int pAccountID, bool restrictAccess, out string errorMessage)
    {
        try
        {
            var account = idc.Accounts.FirstOrDefault(a => a.AccountID == pAccountID);

            if (account != null)
            {
                account.RestrictOnlineAccess = restrictAccess;

                idc.SubmitChanges();
            }

            errorMessage = "";
            return true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }

    private void SetControlsDefault_Tab7()
    {
        txtAct_Tab7.Text = string.Empty;
        chkRestrictOnlineAccess_Tab7.Checked = false;
    }

    //-- End Restrict Online Access --//
    //-- Reminder Days --//
    protected void txtAct_Tab8_TextChanged(object sender, EventArgs e)
    {
        if (rbInsta.Checked)
        {
            Int32 actID_Tab8 = 0;

            if (Int32.TryParse(this.txtAct_Tab8.Text, out actID_Tab8))
                LoadInstaReminderDays_Tab8(actID_Tab8);
            else
                LoadInstaReminderDays_Tab8(0);
        }
        else
        {
            LoadGDSReminderDays_Tab8(this.txtAct_Tab8.Text);
        }
    }

    protected void txtReminderDays_Tab8_TextChanged(object sender, EventArgs e)
    {
        Int32 reminderDays;
        if (this.txtReminderDays_Tab8.Text.Trim().Length == 0 || Int32.TryParse(this.txtReminderDays_Tab8.Text, out reminderDays) == false)
        {
            VisibleError_Tab8("Reminder days is required and must be a number.");
        }
    }

    protected void btnSave_Tab8_Click(object sender, EventArgs e)
    {
        if (PassValidation_Tab8())
        {
            try
            {
                int accountID = int.Parse(this.txtAct_Tab8.Text.Trim());
                int reminderDays = int.Parse(this.txtReminderDays_Tab8.Text.Trim());
                string errorMessage = "";

                bool saveStatus = false;
                if (rbInsta.Checked)
                    saveStatus = SaveInstaReminderDays(accountID, reminderDays, out errorMessage);
                else
                    saveStatus = SaveGDSReminderDays(this.txtAct_Tab8.Text.Trim(), reminderDays, out errorMessage);

                if (saveStatus)
                {
                    VisibleMsg_Tab8("Reminder days is updated.");
                }
                else
                {
                    VisibleError_Tab8(string.Format("An error occured: {0}", errorMessage));
                }

                SetControlsDefault_Tab8();
            }
            catch (Exception ex)
            {
                VisibleError_Tab8(string.Format("An error occured: {0}", ex.Message));
            }
        }
    }

    private void LoadInstaReminderDays_Tab8(int pAccountID)
    {
        txtReminderDays_Tab8.Text = "";
        var account = idc.Accounts.Where(a => a.AccountID == pAccountID).Select(a => new { a.AccountID, a.ReminderDays }).FirstOrDefault();
        if (account != null)
            txtReminderDays_Tab8.Text = account.ReminderDays.ToString();
    }

    private void LoadGDSReminderDays_Tab8(string pAccountID)
    {
        txtReminderDays_Tab8.Text = "";
        var account = idc.Accounts.Where(a => a.GDSAccount == pAccountID).Select(a => new { a.GDSAccount, a.ReminderDays }).FirstOrDefault();
        if (account != null)
            txtReminderDays_Tab8.Text = account.ReminderDays.ToString();
    }

    private Boolean PassValidation_Tab8()
    {
        if (rbInsta.Checked)
        {
            Int32 actID;
            if (this.txtAct_Tab8.Text.Trim().Length == 0 || Int32.TryParse(this.txtAct_Tab8.Text, out actID) == false)
            {
                VisibleError_Tab8("Account# is required and must be a number.");
                return false;
            }
            else
            {
                var act = (from a in idc.Accounts where a.AccountID == actID select a).FirstOrDefault();
                if (act == null)
                {
                    VisibleError_Tab8("Account# is not in system.");
                    return false;
                }
            }
        }
        else
        {
            if (this.txtAct_Tab8.Text.Trim().Length == 0)
            {
                VisibleError_Tab8("Account# is required.");
                return false;
            }
            else
            {
                var act = (from a in idc.Accounts where a.GDSAccount == this.txtAct_Tab8.Text select a).FirstOrDefault();
                if (act == null)
                {
                    VisibleError_Tab8("GDS Account is not in system.");
                    return false;
                }
            }
        }

        Int32 reminderDays;
        if (this.txtReminderDays_Tab8.Text.Trim().Length == 0 || Int32.TryParse(this.txtReminderDays_Tab8.Text, out reminderDays) == false)
        {
            VisibleError_Tab8("Reminder days is required and must be a number.");
            return false;
        }

        return true;
    }

    private bool SaveInstaReminderDays(int pAccountID, int reminderDays, out string errorMessage)
    {
        try
        {
            errorMessage = "";
            var account = idc.Accounts.FirstOrDefault(a => a.AccountID == pAccountID);

            if (account != null)
            {
                account.ReminderDays = reminderDays;

                var accUsers = idc.Users.Where(u => u.AccountID == pAccountID).Where(u => u.OkToEmail == false).ToList();

                foreach (var accUser in accUsers)
                {
                    accUser.OkToEmail = true;
                }

                idc.SubmitChanges();
            }

             return true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }

    private bool SaveGDSReminderDays(string pAccountID, int reminderDays, out string errorMessage)
    {
        try
        {
            errorMessage = "";
            var account = idc.Accounts.FirstOrDefault(a => a.GDSAccount == pAccountID);

            if (account != null)
            {
                account.ReminderDays = reminderDays;

                var accUsers = idc.Users.Where(u => u.AccountID == account.AccountID).Where(u => u.OkToEmail == false).ToList();
                foreach (var accUser in accUsers)
                {
                    accUser.OkToEmail = true;
                }

                idc.SubmitChanges();
            }

            return true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            return false;
        }
    }

    private void SetControlsDefault_Tab8()
    {
        txtAct_Tab8.Text = string.Empty;
        txtReminderDays_Tab8.Text = string.Empty;
    }
    //-- End Reminder Days --//

    // -------------------------------------- Error/Commit msg ---------------------------------//

    private void InvisibleError_Tab1()
    {
        // Reset submission form error message      
        this.ErrorMsg_Tab1.InnerHtml = "";
        this.Error_Tab1.Visible = false;
    }

    private void VisibleError_Tab1(string error)
    {
        this.ErrorMsg_Tab1.InnerHtml = error;
        this.Error_Tab1.Visible = true;
    }

    private void InvisibleMsg_Tab1()
    {
        // Reset submission form error message      
        this.SuccessMsg_Tab1.InnerHtml = "";
        this.Success_Tab1.Visible = false;
    }

    private void VisibleMsg_Tab1(string error)
    {
        this.SuccessMsg_Tab1.InnerHtml = error;
        this.Success_Tab1.Visible = true;
    }
    // -----------
    private void InvisibleError_Tab2()
    {
        // Reset submission form error message      
        this.ErrorMsg_Tab2.InnerText = "";
        this.Error_Tab2.Visible = false;
    }

    private void VisibleError_Tab2(string error)
    {
        this.ErrorMsg_Tab2.InnerText = error;
        this.Error_Tab2.Visible = true;
    }

    private void InvisibleMsg_Tab2()
    {
        // Reset submission form error message      
        this.SuccessMsg_Tab2.InnerText = "";
        this.Success_Tab2.Visible = false;
    }

    private void VisibleMsg_Tab2(string error)
    {
        this.SuccessMsg_Tab2.InnerText = error;
        this.Success_Tab2.Visible = true;
    }

    // -----------
    private void InvisibleError_Tab3()
    {
        // Reset submission form error message      
        this.ErrorMsg_Tab3.InnerText = "";
        this.Error_Tab3.Visible = false;
    }

    private void VisibleError_Tab3(string error)
    {
        this.ErrorMsg_Tab3.InnerText = error;
        this.Error_Tab3.Visible = true;
    }

    private void InvisibleMsg_Tab3()
    {
        // Reset submission form error message      
        this.SuccessMsg_Tab3.InnerText = "";
        this.Success_Tab3.Visible = false;
    }

    private void VisibleMsg_Tab3(string error)
    {
        this.SuccessMsg_Tab3.InnerText = error;
        this.Success_Tab3.Visible = true;
    }

    // -----------
    private void InvisibleError_Tab4()
    {
        // Reset submission form error message      
        this.ErrorMsg_Tab4.InnerHtml = "";
        this.Error_Tab4.Visible = false;
    }

    private void VisibleError_Tab4(string error)
    {
        this.ErrorMsg_Tab4.InnerHtml = error;
        this.Error_Tab4.Visible = true;
    }

    private void InvisibleMsg_Tab4()
    {
        // Reset submission form error message      
        this.SuccessMsg_Tab4.InnerText = "";
        this.Success_Tab4.Visible = false;
    }

    private void VisibleMsg_Tab4(string error)
    {
        this.SuccessMsg_Tab4.InnerText = error;
        this.Success_Tab4.Visible = true;
    }

    // -----------
    private void InvisibleError_Tab5()
    {
        // Reset submission form error message      
        this.ErrorMsg_Tab5.InnerHtml = "";
        this.Error_Tab5.Visible = false;
    }

    private void VisibleError_Tab5(string error)
    {
        this.ErrorMsg_Tab5.InnerHtml = error;
        this.Error_Tab5.Visible = true;
    }

    private void InvisibleMsg_Tab5()
    {
        // Reset submission form error message      
        this.SuccessMsg_Tab5.InnerHtml = "";
        this.Success_Tab5.Visible = false;
    }

    private void VisibleMsg_Tab5(string error)
    {
        this.SuccessMsg_Tab5.InnerHtml = error;
        this.Success_Tab5.Visible = true;
    }

    // -----------
    private void InvisibleError_Tab6()
    {
        // Reset submission form error message      
        this.ErrorMsg_Tab6.InnerHtml = "";
        this.Error_Tab6.Visible = false;
    }

    private void VisibleError_Tab6(string error)
    {
        this.ErrorMsg_Tab6.InnerHtml = error;
        this.Error_Tab6.Visible = true;
    }

    private void InvisibleMsg_Tab6()
    {
        // Reset submission form error message      
        this.SuccessMsg_Tab6.InnerHtml = "";
        this.Success_Tab6.Visible = false;
    }

    private void VisibleMsg_Tab6(string error)
    {
        this.SuccessMsg_Tab6.InnerHtml = error;
        this.Success_Tab6.Visible = true;
    }

    //----
    private void InvisibleError_Tab7()
    {
        // Reset submission form error message      
        this.ErrorMsg_Tab7.InnerHtml = "";
        this.Error_Tab7.Visible = false;
    }

    private void VisibleError_Tab7(string error)
    {
        this.ErrorMsg_Tab7.InnerHtml = error;
        this.Error_Tab7.Visible = true;
    }

    private void InvisibleMsg_Tab7()
    {
        // Reset submission form error message      
        this.SuccessMsg_Tab7.InnerHtml = "";
        this.Success_Tab7.Visible = false;
    }

    private void VisibleMsg_Tab7(string error)
    {
        this.SuccessMsg_Tab7.InnerHtml = error;
        this.Success_Tab7.Visible = true;
    }
    //----
    private void InvisibleError_Tab8()
    {
        // Reset submission form error message      
        this.ErrorMsg_Tab8.InnerHtml = "";
        this.Error_Tab8.Visible = false;
    }

    private void VisibleError_Tab8(string error)
    {
        this.ErrorMsg_Tab8.InnerHtml = error;
        this.Error_Tab8.Visible = true;
    }

    private void InvisibleMsg_Tab8()
    {
        // Reset submission form error message      
        this.SuccessMsg_Tab8.InnerHtml = "";
        this.Success_Tab8.Visible = false;
    }

    private void VisibleMsg_Tab8(string error)
    {
        this.SuccessMsg_Tab8.InnerHtml = error;
        this.Success_Tab8.Visible = true;
    }

    // -------------------------------------- Error/Commit msg ---------------------------------//

    // ----------------------- Common functions ---------------------------//

    private string GetAppSettings(string pAppKey)
    {
        var mySetting = (from AppSet in idc.AppSettings where AppSet.AppKey == pAppKey select AppSet).FirstOrDefault();
        return (mySetting != null) ? mySetting.Value : "";
    }

    private string GenerateBulletMessage(List<string> pMsgList)
    {
        string errorText = "<ul type='circle'>";

        foreach (string msg in pMsgList)
        {
            errorText += "<li>" + msg + "</li>";
        }

        errorText += "</ul>";

        return errorText;
    }
    // ----------------------- Common functions ---------------------------//
    protected void rbInsta_CheckedChanged(object sender, EventArgs e)
    {
        if (rbInsta.Checked)
        {
            txtReminderDays_Tab8.Text = "";
            txtAct_Tab8_TextChanged(null, null);
        }
    }

    protected void rbGds_CheckedChanged(object sender, EventArgs e)
    {
        if (rbGds.Checked)
        {
            txtReminderDays_Tab8.Text = "";
            txtReminderDays_Tab8_TextChanged(null, null);
        }
    }
}