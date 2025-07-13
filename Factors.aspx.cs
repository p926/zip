using System;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Instadose;
using Instadose.Data;
using System.Collections.Generic;

public partial class TechOps_Factors : System.Web.UI.Page
{
    InsDataContext idc = new InsDataContext();

    public int accountID = 0;
    public int locationID = 0;

    // SP for Location-Level Factors as List<> Object.
    private List<sp_GetLocationFactorsResult> locationFactors;

    // String to hold the current Username.
    // Testing Git commit. Update remote MergeStaging branch
    string userName = "Unknown";

    #region ON PAGE LOAD.
    protected void Page_Load(object sender, EventArgs e)
    {
        // Try to set the Username.
        try { this.userName = User.Identity.Name.Split('\\')[1]; }
        catch { this.userName = "Unknown"; }

        Int32.TryParse(Request.QueryString["AccountID"], out accountID);

        if (accountID == 0) Response.Redirect("FactorsSearch.aspx");

        if (IsPostBack) return;

        this.ResetAllControls();

        // Get Company Name based on AccountID (QueryString).
        string companyName = (from acct in idc.Accounts where acct.AccountID == accountID select acct.CompanyName).FirstOrDefault();

        // Display Account ID/# with Company Name.
        this.lblAccountIDAndCompanyName.Text = string.Format("{0} ({1})", accountID, companyName);
        this.lblAccountIDAndCompanyName.ToolTip = string.Format("Account: {0} ({1})", accountID, companyName);

        // Load/Populate GridView with Location-Level Settings.
        this.GetLocationLevelSettings(accountID);
    }
    #endregion

    #region LOCATION-LEVEL FACTORS GRIDVIEW FUNCTION(S).
    /// <summary>
    /// Populate/Fill gvLocationSettings GridView with Location-Level Settings/Factors.
    /// </summary>
    /// <param name="accountid"></param>
    private void GetLocationLevelSettings(int accountid)
    {
        locationFactors = idc.sp_GetLocationFactors(accountid).ToList();
        this.gvLocationSettings.DataSource = locationFactors;
        this.gvLocationSettings.DataBind();
    }
    #endregion

    #region GET ACCOUNT-LEVEL FACTORS.
    /// <summary>
    /// Returns multiple values for Account-Level settings (MRD, MRDIncrement, Deep, Eye, Shallow Factors, BackgroundRate, ReadLimit).
    /// </summary>
    /// <param name="accountid"></param>
    /// <param name="mrd"></param>
    /// <param name="mrdincrement"></param>
    /// <param name="deepfactor"></param>
    /// <param name="eyefactor"></param>
    /// <param name="shallowfactor"></param>
    /// <param name="backgroundrate"></param>
    /// /// <param name="backgroundrate2"></param>
    /// <param name="readlimit"></param>
    private void GetAccountSettings(int accountid, out string companyname, out int? mrd, out int? mrdincrement, out int? mrd2, out int? mrdincrement2, out double? deepfactor, out double? eyefactor, out double? shallowfactor, out double? backgroundrate, out double? backgroundrate2, out double? readlimit, out string algorithmfactor, out int? mrdPlus, out int? mrdincrementPlus, out double? deepfactorPlus, out double? eyefactorPlus, out double? shallowfactorPlus, out double? backgroundratePlus)
    {
        var accountLevelInfo = (from acct in idc.Accounts
                                where acct.AccountID == accountid
                                select new {
                                    CompanyName = acct.CompanyName,
                                    MRD = acct.MRD,
                                    MRDIncrement = acct.MRDIncrement,                                    
                                    DeepFactor = acct.DeepFactor,
                                    EyeFactor = acct.EyeFactor,
                                    ShallowFactor = acct.ShallowFactor,
                                    BackgroundRate = acct.BackgroundRate,
                                    MRDPlus = acct.MRDPlus,
                                    MRDIncrementPlus = acct.MRDIncrementPlus,
                                    DeepFactorPlus = acct.DeepFactorPlus,
                                    EyeFactorPlus = acct.EyeFactorPlus,
                                    ShallowFactorPlus = acct.ShallowFactorPlus,
                                    BackgroundRatePlus = acct.BackgroundRatePlus,
                                    MRD2 = acct.MRD2,
                                    MRDIncrement2 = acct.MRDIncrement2,
                                    BackgroundRate2 = acct.BackgroundRate2,
                                    ReadLimit = acct.ReadLimit,
                                    AlgorithmFactor = acct.AlgorithmFactor
                                }).FirstOrDefault();

        companyname = accountLevelInfo.CompanyName;
        mrd = accountLevelInfo.MRD;
        mrdincrement = accountLevelInfo.MRDIncrement;       
        deepfactor = accountLevelInfo.DeepFactor;
        eyefactor = accountLevelInfo.EyeFactor;
        shallowfactor = accountLevelInfo.ShallowFactor;
        backgroundrate = accountLevelInfo.BackgroundRate;
        mrdPlus = accountLevelInfo.MRDPlus;
        mrdincrementPlus = accountLevelInfo.MRDIncrementPlus;
        deepfactorPlus = accountLevelInfo.DeepFactorPlus;
        eyefactorPlus = accountLevelInfo.EyeFactorPlus;
        shallowfactorPlus = accountLevelInfo.ShallowFactorPlus;
        backgroundratePlus = accountLevelInfo.BackgroundRatePlus;
        mrd2 = accountLevelInfo.MRD2;
        mrdincrement2 = accountLevelInfo.MRDIncrement2;
        backgroundrate2 = accountLevelInfo.BackgroundRate2;
        readlimit = accountLevelInfo.ReadLimit;
        algorithmfactor = accountLevelInfo.AlgorithmFactor;
    }
    #endregion

    #region GET DEVICE FIRMWARE FACTORS.
    /// <summary>
    /// Returns multiple values for DeviceFirmware settings (Deep, Eye, and Shallow Factors).
    /// </summary>
    /// <param name="deepfactor"></param>
    /// <param name="eyefactor"></param>
    /// <param name="shallowfactor"></param>
    private void GetDeviceFirmware(out double deepfactor, out double eyefactor, out double shallowfactor)
    {
        var deviceFirmwareInfo = (from dv in idc.DeviceFirmwares
                                  orderby dv.DeviceFirmwareID descending
                                  select new { 
                                      DeepFactor = dv.DeepFactor, 
                                      EyeFactor = dv.EyeFactor, 
                                      ShallowFactor = dv.ShallowFactor 
                                  }).FirstOrDefault();

        deepfactor = deviceFirmwareInfo.DeepFactor;
        eyefactor = deviceFirmwareInfo.EyeFactor;
        shallowfactor = deviceFirmwareInfo.ShallowFactor;
    }
    #endregion

    #region GET STATE & COUNTRY BACKGROUND RATES.
    /// <summary>
    /// Returns multiple values for State and Country settings (BackgroundRate).
    /// </summary>
    /// <param name="accountid"></param>
    /// <param name="statebackgroundrate"></param>
    /// <param name="countrybackgroundrate"></param>
    private void GetStateAndCountry(int accountid, out double? statebackgroundrate, out double? countrybackgroundrate)
    {
        var stateAndCountryInfo = (from loc in idc.Locations
                                   join s in idc.States on loc.ShippingStateID equals s.StateID
                                   join c in idc.Countries on loc.ShippingCountryID equals c.CountryID
                                   where loc.AccountID == accountid && loc.IsDefaultLocation == true
                                   select new { 
                                        StateBackgroundRate = s.BackgroundRate,
                                        CountryBackgroundRate = c.BackgroundRate
                                   }).FirstOrDefault();

        statebackgroundrate = stateAndCountryInfo.StateBackgroundRate;
        countrybackgroundrate = stateAndCountryInfo.CountryBackgroundRate;
    }
    #endregion

    #region GENERAL FUNCTIONS.
    /// <summary>
    /// Gets the Index of a column based on the column's name (used for DataBound columns).
    /// </summary>
    /// <param name="row"></param>
    /// <param name="columnname"></param>
    /// <returns></returns>
    private int GetColumnIndexByName(GridViewRow row, string columnname)
    {
        int columnIndex = 0;

        foreach (DataControlFieldCell cell in row.Cells)
        {
            if (cell.ContainingField is BoundField)
                if (((BoundField)cell.ContainingField).DataField.Equals(columnname))
                    break;
            columnIndex++; // Keep adding 1 while we don't have the correct Column Name.
        }

        return columnIndex;
    }

    #region ERROR MESSAGE
    private void InvisibleErrors()
    {
        this.divErrorMessage.Visible = false;
        this.spnErrorMessage.InnerText = "";
    }

    private void VisibleErrors(string error)
    {
        this.divErrorMessage.Visible = true;
        this.spnErrorMessage.InnerText = error;
    }
    #endregion

    #region SUCCESS MESSAGE.
    private void InvisibleSuccesses()
    {
        this.divSuccessMessage.Visible = false;
        this.spnSuccessMessage.InnerText = "";
    }

    private void VisibleSuccesses(string success)
    {
        this.divSuccessMessage.Visible = true;
        this.spnSuccessMessage.InnerText = success;
    }
    #endregion

    /// <summary>
    /// Reset all Controls (TextBoxes, Labels, Error and Success Messages).
    /// </summary>
    private void ResetAllControls()
    {
        this.InvisibleErrors();
        this.InvisibleSuccesses();

        // Account Settings Modal/Dialog.
        this.lblAccountID.Text = string.Empty;
        this.lblCompanyName.Text = string.Empty;

        this.txtMRD_Account.Text = string.Empty;
        this.lblMRDFrom_Account.Text = string.Empty;        
        this.txtMRDIncrement_Account.Text = string.Empty;
        this.lblMRDIncrementFrom_Account.Text = string.Empty;        
        this.txtDeepFactor_Account.Text = string.Empty;
        this.lblDeepFactorFrom_Account.Text = string.Empty;
        this.txtEyeFactor_Account.Text = string.Empty;
        this.lblEyeFactorFrom_Account.Text = string.Empty;
        this.txtShallowFactor_Account.Text = string.Empty;
        this.lblShallowFactorFrom_Account.Text = string.Empty;
        this.txtBackgroundRate_Account.Text = string.Empty;
        this.lblBackgroundRateFrom_Account.Text = string.Empty;

        this.txtMRDPlus_Account.Text = string.Empty;
        this.lblMRDPlusFrom_Account.Text = string.Empty;
        this.txtMRDIncrementPlus_Account.Text = string.Empty;
        this.lblMRDIncrementPlusFrom_Account.Text = string.Empty;
        this.txtDeepFactorPlus_Account.Text = string.Empty;
        this.lblDeepFactorPlusFrom_Account.Text = string.Empty;
        this.txtEyeFactorPlus_Account.Text = string.Empty;
        this.lblEyeFactorPlusFrom_Account.Text = string.Empty;
        this.txtShallowFactorPlus_Account.Text = string.Empty;
        this.lblShallowFactorPlusFrom_Account.Text = string.Empty;
        this.txtBackgroundRatePlus_Account.Text = string.Empty;
        this.lblBackgroundRatePlusFrom_Account.Text = string.Empty;

        this.txtMRD2_Account.Text = string.Empty;
        this.lblMRD2From_Account.Text = string.Empty;
        this.txtMRDIncrement2_Account.Text = string.Empty;
        this.lblMRDIncrement2From_Account.Text = string.Empty;
        this.txtBackgroundRate2_Account.Text = string.Empty;
        this.lblBackgroundRate2From_Account.Text = string.Empty;        

        this.txtReadLimit_Account.Text = string.Empty;
        this.lblReadLimitFrom_Account.Text = string.Empty;
        this.txtAlgorithmFactor_Account.Text = string.Empty;
        this.lblAlgorithmFactorFrom_Account.Text = string.Empty;

        // Location Settings Modal/Dialog.
        this.hdnfldLocationID.Value = "0";
        this.lblLocationName.Text = string.Empty;

        this.txtMRD_Location.Text = string.Empty;
        this.lblMRDFrom_Location.Text = string.Empty;        
        this.txtMRDIncrement_Location.Text = string.Empty;
        this.lblMRDIncrementFrom_Location.Text = string.Empty;        
        this.txtDeepFactor_Location.Text = string.Empty;
        this.lblDeepFactorFrom_Location.Text = string.Empty;
        this.txtEyeFactor_Location.Text = string.Empty;
        this.lblEyeFactorFrom_Location.Text = string.Empty;
        this.txtShallowFactor_Location.Text = string.Empty;
        this.lblShallowFactorFrom_Location.Text = string.Empty;
        this.txtBackgroundRate_Location.Text = string.Empty;
        this.lblBackgroundRateFrom_Location.Text = string.Empty;

        this.txtMRDPlus_Location.Text = string.Empty;
        this.lblMRDPlusFrom_Location.Text = string.Empty;
        this.txtMRDIncrementPlus_Location.Text = string.Empty;
        this.lblMRDIncrementPlusFrom_Location.Text = string.Empty;
        this.txtDeepFactorPlus_Location.Text = string.Empty;
        this.lblDeepFactorPlusFrom_Location.Text = string.Empty;
        this.txtEyeFactorPlus_Location.Text = string.Empty;
        this.lblEyeFactorPlusFrom_Location.Text = string.Empty;
        this.txtShallowFactorPlus_Location.Text = string.Empty;
        this.lblShallowFactorPlusFrom_Location.Text = string.Empty;
        this.txtBackgroundRatePlus_Location.Text = string.Empty;
        this.lblBackgroundRatePlusFrom_Location.Text = string.Empty;

        this.txtMRD2_Location.Text = string.Empty;
        this.lblMRD2From_Location.Text = string.Empty;
        this.txtMRDIncrement2_Location.Text = string.Empty;
        this.lblMRDIncrement2From_Location.Text = string.Empty;
        this.txtBackgroundRate2_Location.Text = string.Empty;
        this.lblBackgroundRate2From_Location.Text = string.Empty;        

        this.txtReadLimit_Location.Text = string.Empty;
        this.lblReadLimitFrom_Location.Text = string.Empty;
        this.txtAlgorithmFactor_Location.Text = string.Empty;
        this.lblAlgorithmFactorFrom_Location.Text = string.Empty;
    }
    #endregion

    #region ACCOUNT-LEVEL SETTINGS FUNCTIONS (MODAL/DIALOG).
    /// <summary>
    /// ON_CLICK of "Account Settings" link in toolbar.
    /// Populates TextBox Controls with Account-Level settings (if available).
    /// If Account-Level does not have values for respective settings, then a Placeholder value will appear.
    /// Next to each Placeholder values, a Label will denote from where the value is derived (i.e. "(from Application)").
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void lnkbtnAccountSettings_Click(object sender, EventArgs e)
    {
        this.ResetAllControls();

        int.TryParse(Request.QueryString["AccountID"], out accountID);

        string acctCompanyName = "";
        int? acctMRD = null;
        int? acctMRDIncrement = null;        
        double? acctDeepFactor = null;
        double? acctEyeFactor = null;
        double? acctShallowFactor = null;
        double? acctBackgroundRate = null;

        int? acctMRD2 = null;
        int? acctMRDIncrement2 = null;
        double? acctBackgroundRate2 = null;

        int? acctMRDPlus = null;
        int? acctMRDIncrementPlus = null;
        double? acctDeepFactorPlus = null;
        double? acctEyeFactorPlus = null;
        double? acctShallowFactorPlus = null;
        double? acctBackgroundRatePlus = null;

        double? acctReadLimit = null;
        string acctAlgorithmFactor = null;
        this.GetAccountSettings(accountID, out acctCompanyName, out acctMRD, out acctMRDIncrement, out acctMRD2, out acctMRDIncrement2, out acctDeepFactor, out acctEyeFactor, out acctShallowFactor, out acctBackgroundRate, out acctBackgroundRate2, out acctReadLimit, out acctAlgorithmFactor, out acctMRDPlus, out acctMRDIncrementPlus, out acctDeepFactorPlus, out acctEyeFactorPlus, out acctShallowFactorPlus, out acctBackgroundRatePlus);

        this.lblAccountID.Text = accountID.ToString();
        this.lblCompanyName.Text = acctCompanyName;

        // AppSettings.
        int appMRD = int.Parse(Basics.GetSetting("MRD"));
        int appMRDIncrement = int.Parse(Basics.GetSetting("MRDIncrement"));
        int appMRDPlus = int.Parse(Basics.GetSetting("MRDPlus"));
        int appMRDIncrementPlus = int.Parse(Basics.GetSetting("MRDIncrementPlus"));
        int appMRD2 = int.Parse(Basics.GetSetting("MRDID2"));
        int appMRDIncrement2 = int.Parse(Basics.GetSetting("MRDIncrementID2"));
        double appReadLimit = double.Parse(Basics.GetSetting("ReadLimit"));
        string appAlgorithmFactor = Basics.GetSetting("AlgorithmFactorID2");
                
        // DeviceFirmware.
        double dfDeepFactor = 0;
        double dfEyeFactor = 0;
        double dfShallowFactor = 0;
        this.GetDeviceFirmware(out dfDeepFactor, out dfEyeFactor, out dfShallowFactor);

        // State & Country.
        double? stateBackgroundRate = null;
        double? countryBackgroundRate = null;
        this.GetStateAndCountry(accountID, out stateBackgroundRate, out countryBackgroundRate);

        // MRD.
        if (acctMRD.HasValue)
            this.txtMRD_Account.Text = (acctMRD.Value).ToString();
        else
            this.lblMRDFrom_Account.Text = "(from Application)";

        // MRD Increment.
        if (acctMRDIncrement.HasValue)
            this.txtMRDIncrement_Account.Text = (acctMRDIncrement.Value).ToString();
        else
            this.lblMRDIncrementFrom_Account.Text = "(from Application)";

        // Deep Factor.
        if (acctDeepFactor.HasValue)
            this.txtDeepFactor_Account.Text = (acctDeepFactor.Value).ToString();
        else
            this.lblDeepFactorFrom_Account.Text = "(from Firmware)";

        // Eye Factor.
        if (acctEyeFactor.HasValue)
            this.txtEyeFactor_Account.Text = (acctEyeFactor.Value).ToString();
        else
            this.lblEyeFactorFrom_Account.Text = "(from Firmware)";

        // Shallow Factor.
        if (acctShallowFactor.HasValue)
            this.txtShallowFactor_Account.Text = (acctShallowFactor.Value).ToString();
        else
            this.lblShallowFactorFrom_Account.Text = "(from Firmware)";

        // Background Rate.
        if (acctBackgroundRate.HasValue)
            this.txtBackgroundRate_Account.Text = (acctBackgroundRate.Value).ToString();
        else
        {
            if (!stateBackgroundRate.HasValue)
                this.lblBackgroundRateFrom_Account.Text = "(from Country)";
            else
                this.lblBackgroundRateFrom_Account.Text = "(from State)";
        }

        // MRD+.
        if (acctMRDPlus.HasValue)
            this.txtMRDPlus_Account.Text = (acctMRDPlus.Value).ToString();
        else
            this.lblMRDPlusFrom_Account.Text = "(from Application)";

        // MRD Increment+.
        if (acctMRDIncrementPlus.HasValue)
            this.txtMRDIncrementPlus_Account.Text = (acctMRDIncrementPlus.Value).ToString();
        else
            this.lblMRDIncrementPlusFrom_Account.Text = "(from Application)";

        // Deep Factor+.
        if (acctDeepFactorPlus.HasValue)
            this.txtDeepFactorPlus_Account.Text = (acctDeepFactorPlus.Value).ToString();
        else
            this.lblDeepFactorPlusFrom_Account.Text = "(from Firmware)";

        // Eye Factor+.
        if (acctEyeFactorPlus.HasValue)
            this.txtEyeFactorPlus_Account.Text = (acctEyeFactorPlus.Value).ToString();
        else
            this.lblEyeFactorPlusFrom_Account.Text = "(from Firmware)";

        // Shallow Factor+.
        if (acctShallowFactorPlus.HasValue)
            this.txtShallowFactorPlus_Account.Text = (acctShallowFactorPlus.Value).ToString();
        else
            this.lblShallowFactorPlusFrom_Account.Text = "(from Firmware)";

        // Background Rate+.
        if (acctBackgroundRatePlus.HasValue)
            this.txtBackgroundRatePlus_Account.Text = (acctBackgroundRatePlus.Value).ToString();
        else
        {
            if (!stateBackgroundRate.HasValue)
                this.lblBackgroundRatePlusFrom_Account.Text = "(from Country)";
            else
                this.lblBackgroundRatePlusFrom_Account.Text = "(from State)";
        }

        // MRD 2 (ID2).
        if (acctMRD2.HasValue)
            this.txtMRD2_Account.Text = (acctMRD2.Value).ToString();
        else
            this.lblMRD2From_Account.Text = "(from Application)";

        // MRD Increment 2 (ID2).
        if (acctMRDIncrement2.HasValue)
            this.txtMRDIncrement2_Account.Text = (acctMRDIncrement2.Value).ToString();
        else
            this.lblMRDIncrement2From_Account.Text = "(from Application)";        

        // Background Rate 2.
        if (acctBackgroundRate2.HasValue)
            this.txtBackgroundRate2_Account.Text = (acctBackgroundRate2.Value).ToString();
        else
        {
            if (!stateBackgroundRate.HasValue)
                this.lblBackgroundRate2From_Account.Text = "(from Country)";
            else
                this.lblBackgroundRate2From_Account.Text = "(from State)";
        }

        // Read Limit.
        if (acctReadLimit.HasValue)
            this.txtReadLimit_Account.Text = (acctReadLimit.Value).ToString();
        else
            this.lblReadLimitFrom_Account.Text = "(from Application)";

        // Algorithm Factor (ID2).
        if (acctAlgorithmFactor != null)
            this.txtAlgorithmFactor_Account.Text = acctAlgorithmFactor;
        else
            this.lblAlgorithmFactorFrom_Account.Text = "(from Application)";

        // Placeholder will always be present, in case User decideds not to enter an Account-Level value.
        this.txtMRD_Account.Attributes.Add("placeholder", appMRD.ToString());
        this.txtMRDIncrement_Account.Attributes.Add("placeholder", appMRDIncrement.ToString());        
        this.txtDeepFactor_Account.Attributes.Add("placeholder", dfDeepFactor.ToString());
        this.txtEyeFactor_Account.Attributes.Add("placeholder", dfEyeFactor.ToString());
        this.txtShallowFactor_Account.Attributes.Add("placeholder", dfShallowFactor.ToString());
        this.txtBackgroundRate_Account.Attributes.Add("placeholder", stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString());

        this.txtMRDPlus_Account.Attributes.Add("placeholder", appMRDPlus.ToString());
        this.txtMRDIncrementPlus_Account.Attributes.Add("placeholder", appMRDIncrementPlus.ToString());
        this.txtDeepFactorPlus_Account.Attributes.Add("placeholder", dfDeepFactor.ToString());
        this.txtEyeFactorPlus_Account.Attributes.Add("placeholder", dfEyeFactor.ToString());
        this.txtShallowFactorPlus_Account.Attributes.Add("placeholder", dfShallowFactor.ToString());
        this.txtBackgroundRatePlus_Account.Attributes.Add("placeholder", stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString());

        this.txtMRD2_Account.Attributes.Add("placeholder", appMRD2.ToString());
        this.txtMRDIncrement2_Account.Attributes.Add("placeholder", appMRDIncrement2.ToString());
        this.txtBackgroundRate2_Account.Attributes.Add("placeholder", stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString());
        
        this.txtReadLimit_Account.Attributes.Add("placeholder", appReadLimit.ToString());
        this.txtAlgorithmFactor_Account.Attributes.Add("placeholder", appAlgorithmFactor);

        // Send Javascript to client to OPEN/DISPLAY the modal dialog.
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('divAccountSettingsDialog')", true);
    }

    /// <summary>
    /// ON_CLICK will Update Account-Level Settings for given AccountID.
    /// If any TextBoxes are left blank (only Placeholder value), then those values will be left NULL in the Accounts table.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnUpdate_Account_Click(object sender, EventArgs e)
    {
        int.TryParse(Request.QueryString["AccountID"], out accountID);

        // Update Account table where applicable.
        Account acct = (from a in idc.Accounts
                        where a.AccountID == accountID
                        select a).FirstOrDefault();

        if (acct == null) return;

        // Set all values to NULL (nullable variables).
        int? mrd = null;
        int? mrdIncrement = null;
        double? deepFactor = null;
        double? eyeFactor = null;
        double? shallowFactor = null;
        double? backgroundRate = null;

        int? mrdPlus = null;
        int? mrdIncrementPlus = null;
        double? deepFactorPlus = null;
        double? eyeFactorPlus = null;
        double? shallowFactorPlus = null;
        double? backgroundRatePlus = null;

        int? mrd2 = null;
        int? mrdIncrement2 = null;        
        double? backgroundRate2 = null;

        double? readLimit = null;
        string algorithmFactor = null;

        if (this.txtMRD_Account.Text.Trim() != "") mrd = Convert.ToInt32(this.txtMRD_Account.Text.Trim());
        if (this.txtMRDIncrement_Account.Text.Trim() != "") mrdIncrement = Convert.ToInt32(this.txtMRDIncrement_Account.Text.Trim());       
        if (this.txtDeepFactor_Account.Text.Trim() != "") deepFactor = Convert.ToDouble(this.txtDeepFactor_Account.Text.Trim());
        if (this.txtEyeFactor_Account.Text.Trim() != "") eyeFactor = Convert.ToDouble(this.txtEyeFactor_Account.Text.Trim());
        if (this.txtShallowFactor_Account.Text.Trim() != "") shallowFactor = Convert.ToDouble(this.txtShallowFactor_Account.Text.Trim());
        if (this.txtBackgroundRate_Account.Text.Trim() != "") backgroundRate = Convert.ToDouble(this.txtBackgroundRate_Account.Text.Trim());

        if (this.txtMRDPlus_Account.Text.Trim() != "") mrdPlus = Convert.ToInt32(this.txtMRDPlus_Account.Text.Trim());
        if (this.txtMRDIncrementPlus_Account.Text.Trim() != "") mrdIncrementPlus = Convert.ToInt32(this.txtMRDIncrementPlus_Account.Text.Trim());
        if (this.txtDeepFactorPlus_Account.Text.Trim() != "") deepFactorPlus = Convert.ToDouble(this.txtDeepFactorPlus_Account.Text.Trim());
        if (this.txtEyeFactorPlus_Account.Text.Trim() != "") eyeFactorPlus = Convert.ToDouble(this.txtEyeFactorPlus_Account.Text.Trim());
        if (this.txtShallowFactorPlus_Account.Text.Trim() != "") shallowFactorPlus = Convert.ToDouble(this.txtShallowFactorPlus_Account.Text.Trim());
        if (this.txtBackgroundRatePlus_Account.Text.Trim() != "") backgroundRatePlus = Convert.ToDouble(this.txtBackgroundRatePlus_Account.Text.Trim());

        if (this.txtMRD2_Account.Text.Trim() != "") mrd2 = Convert.ToInt32(this.txtMRD2_Account.Text.Trim());
        if (this.txtMRDIncrement2_Account.Text.Trim() != "") mrdIncrement2 = Convert.ToInt32(this.txtMRDIncrement2_Account.Text.Trim());
        if (this.txtBackgroundRate2_Account.Text.Trim() != "") backgroundRate2 = Convert.ToDouble(this.txtBackgroundRate2_Account.Text.Trim());

        if (this.txtReadLimit_Account.Text.Trim() != "") readLimit = Convert.ToDouble(this.txtReadLimit_Account.Text.Trim());
        if (this.txtAlgorithmFactor_Account.Text.Trim() != "") algorithmFactor = this.txtAlgorithmFactor_Account.Text.Trim();

        // If TextBox is string.Empty/"", then insert NULL for Account-Level setting(s).
        if (mrd.HasValue) acct.MRD = mrd.Value;
        else acct.MRD = null;
        if (mrdIncrement.HasValue) acct.MRDIncrement = mrdIncrement.Value;
        else acct.MRDIncrement = null;        
        if (deepFactor.HasValue) acct.DeepFactor = deepFactor.Value;
        else acct.DeepFactor = null;
        if (eyeFactor.HasValue) acct.EyeFactor = eyeFactor.Value;
        else acct.EyeFactor = null;
        if (shallowFactor.HasValue) acct.ShallowFactor = shallowFactor.Value;
        else acct.ShallowFactor = null;
        if (backgroundRate.HasValue) acct.BackgroundRate = backgroundRate.Value;
        else acct.BackgroundRate = null;

        if (mrdPlus.HasValue) acct.MRDPlus = mrdPlus.Value;
        else acct.MRDPlus = null;
        if (mrdIncrementPlus.HasValue) acct.MRDIncrementPlus = mrdIncrementPlus.Value;
        else acct.MRDIncrementPlus = null;
        if (deepFactorPlus.HasValue) acct.DeepFactorPlus = deepFactorPlus.Value;
        else acct.DeepFactorPlus = null;
        if (eyeFactorPlus.HasValue) acct.EyeFactorPlus = eyeFactorPlus.Value;
        else acct.EyeFactorPlus = null;
        if (shallowFactorPlus.HasValue) acct.ShallowFactorPlus = shallowFactorPlus.Value;
        else acct.ShallowFactorPlus = null;
        if (backgroundRatePlus.HasValue) acct.BackgroundRatePlus = backgroundRatePlus.Value;
        else acct.BackgroundRatePlus = null;

        if (mrd2.HasValue) acct.MRD2 = mrd2.Value;
        else acct.MRD2 = null;
        if (mrdIncrement2.HasValue) acct.MRDIncrement2 = mrdIncrement2.Value;
        else acct.MRDIncrement2 = null;
        if (backgroundRate2.HasValue) acct.BackgroundRate2 = backgroundRate2.Value;
        else acct.BackgroundRate2 = null;

        if (readLimit.HasValue) acct.ReadLimit = readLimit.Value;
        else acct.ReadLimit = null;
        if (algorithmFactor != null) acct.AlgorithmFactor = algorithmFactor;
        else acct.AlgorithmFactor = null;

        try
        {
            idc.SubmitChanges();

            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divAccountSettingsDialog')", true);
            
            this.VisibleSuccesses("Settings for the account have been updated.");
            
            // Rebind GridView to display updated Inheritance values (where applicable).
            this.GetLocationLevelSettings(accountID);
        }
        catch
        {
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divAccountSettingsDialog')", true);
            this.VisibleErrors("An error occurred while updating the account.");            
            return;
        }
    }
    #endregion

    #region LOCATION-LEVEL SETTINGS FUNCTIONS (MODAL/DIALOG).
    /// <summary>
    /// ON_CLICK of Pencil/Edit Icon from GridView, populate Modal Dialog with Location-Level Factors Information.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvLocationSettings_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (((WebControl)e.CommandSource).NamingContainer.GetType() == typeof(ContentPlaceHolder))
            return;

        this.ResetAllControls();
        
        GridViewRow row = (GridViewRow)((System.Web.UI.Control)e.CommandSource).NamingContainer;
        LinkButton lnkbtn = (LinkButton)row.FindControl("lnkbtnUpdateLocationSettings");
        string lnkbtnCommandName = lnkbtn.CommandName.ToString();
        int.TryParse(lnkbtn.CommandArgument.ToString(), out locationID);

        // Assign LocationID to HiddenField, will be referenced when "Update" is clicked.
        this.hdnfldLocationID.Value = locationID.ToString();

        if (lnkbtnCommandName != "UpdateLocationSettings") return;

        var locationLevelInfo = (from loc in idc.Locations
                                 where loc.LocationID == locationID
                                 select loc).FirstOrDefault();

        if (locationLevelInfo == null) return;

        this.lblLocationName.Text = locationLevelInfo.LocationName;

        // Get Account record based on AccountID.
        int.TryParse(Request.QueryString["AccountID"], out accountID);

        if (accountID == 0) return;

        // Account Settings.
        var accountLevelInfo = (from acct in idc.Accounts
                                where acct.AccountID == accountID
                                select new { 
                                    
                                }).FirstOrDefault();

        if (accountLevelInfo == null) return;

        string acctCompanyName = "";    // Not used.

        int? acctMRD = null;
        int? acctMRDIncrement = null;        
        double? acctDeepFactor = null;
        double? acctEyeFactor = null;
        double? acctShallowFactor = null;
        double? acctBackgroundRate = null;

        int? acctMRDPlus = null;
        int? acctMRDIncrementPlus = null;
        double? acctDeepFactorPlus = null;
        double? acctEyeFactorPlus = null;
        double? acctShallowFactorPlus = null;
        double? acctBackgroundRatePlus = null;

        int? acctMRD2 = null;
        int? acctMRDIncrement2 = null;
        double? acctBackgroundRate2 = null;

        double? acctReadLimit = null;
        string acctAlgorithmFactor = null;
        this.GetAccountSettings(accountID, out acctCompanyName, out acctMRD, out acctMRDIncrement, out acctMRD2, out acctMRDIncrement2, out acctDeepFactor, out acctEyeFactor, out acctShallowFactor, out acctBackgroundRate, out acctBackgroundRate2, out acctReadLimit, out acctAlgorithmFactor, out acctMRDPlus, out acctMRDIncrementPlus, out acctDeepFactorPlus, out acctEyeFactorPlus, out acctShallowFactorPlus, out acctBackgroundRatePlus);

        // AppSettings.
        int appMRD = int.Parse(Basics.GetSetting("MRD"));
        int appMRDIncrement = int.Parse(Basics.GetSetting("MRDIncrement"));
        int appMRDPlus = int.Parse(Basics.GetSetting("MRDPlus"));
        int appMRDIncrementPlus = int.Parse(Basics.GetSetting("MRDIncrementPlus"));
        int appMRD2 = int.Parse(Basics.GetSetting("MRDID2"));
        int appMRDIncrement2 = int.Parse(Basics.GetSetting("MRDIncrementID2"));
        int appReadLimit = int.Parse(Basics.GetSetting("ReadLimit"));
        string appAlgorithmFactor = Basics.GetSetting("AlgorithmFactorID2");

        // DeviceFirmware.
        double dfDeepFactor = 0;
        double dfEyeFactor = 0;
        double dfShallowFactor = 0;
        this.GetDeviceFirmware(out dfDeepFactor, out dfEyeFactor, out dfShallowFactor);

        // State & Country.
        double? stateBackgroundRate = null;
        double? countryBackgroundRate = null;
        this.GetStateAndCountry(accountID, out stateBackgroundRate, out countryBackgroundRate);

        // MRD.
        if (locationLevelInfo.MRD.HasValue)
            this.txtMRD_Location.Text = (locationLevelInfo.MRD.Value).ToString();
        else
        {
            if (!acctMRD.HasValue)
                this.lblMRDFrom_Location.Text = "(from Application)";
            else
                this.lblMRDFrom_Location.Text = "(from Account)";
        }

        // MRD Increment.
        if (locationLevelInfo.MRDIncrement.HasValue)
            this.txtMRDIncrement_Location.Text = (locationLevelInfo.MRDIncrement.Value).ToString();
        else
        {
            if (!acctMRDIncrement.HasValue)
                this.lblMRDIncrementFrom_Location.Text = "(from Application)";
            else
                this.lblMRDIncrementFrom_Location.Text = "(from Account)";
        }       

        // Deep Factor.
        if (locationLevelInfo.DeepFactor.HasValue)
            this.txtDeepFactor_Location.Text = (locationLevelInfo.DeepFactor.Value).ToString();
        else
        {
            if (!acctDeepFactor.HasValue)
                this.lblDeepFactorFrom_Location.Text = "(from Firmware)";
            else
                this.lblDeepFactorFrom_Location.Text = "(from Account)";
        }

        // Eye Factor.
        if (locationLevelInfo.EyeFactor.HasValue)
            this.txtEyeFactor_Location.Text = (locationLevelInfo.EyeFactor.Value).ToString();
        else
        {
            if (!acctEyeFactor.HasValue)
                this.lblEyeFactorFrom_Location.Text = "(from Firmware)";
            else
                this.lblEyeFactorFrom_Location.Text = "(from Account)";
        }

        // Shallow Factor.
        if (locationLevelInfo.ShallowFactor.HasValue)
            this.txtShallowFactor_Location.Text = (locationLevelInfo.ShallowFactor.Value).ToString();
        else
        {
            if (!acctShallowFactor.HasValue)
                this.lblShallowFactorFrom_Location.Text = "(from Firmware)";
            else
                this.lblShallowFactorFrom_Location.Text = "(from Account)";
        }
            
        // Background Rate.
        if (locationLevelInfo.BackgroundRate.HasValue)
            this.txtBackgroundRate_Location.Text = (locationLevelInfo.BackgroundRate.Value).ToString();
        else
        {
            if (!acctBackgroundRate.HasValue)
            {
                if (!stateBackgroundRate.HasValue)
                    this.lblBackgroundRateFrom_Location.Text = "(from Country)";
                else
                    this.lblBackgroundRateFrom_Location.Text = "(from State)";
            }
            else
                this.lblBackgroundRateFrom_Location.Text = "(from Account)";
        }

        // MRD+.
        if (locationLevelInfo.MRDPlus.HasValue)
            this.txtMRDPlus_Location.Text = (locationLevelInfo.MRDPlus.Value).ToString();
        else
        {
            if (!acctMRDPlus.HasValue)
                this.lblMRDPlusFrom_Location.Text = "(from Application)";
            else
                this.lblMRDPlusFrom_Location.Text = "(from Account)";
        }

        // MRD Increment+.
        if (locationLevelInfo.MRDIncrementPlus.HasValue)
            this.txtMRDIncrementPlus_Location.Text = (locationLevelInfo.MRDIncrementPlus.Value).ToString();
        else
        {
            if (!acctMRDIncrementPlus.HasValue)
                this.lblMRDIncrementPlusFrom_Location.Text = "(from Application)";
            else
                this.lblMRDIncrementPlusFrom_Location.Text = "(from Account)";
        }

        // Deep Factor+.
        if (locationLevelInfo.DeepFactorPlus.HasValue)
            this.txtDeepFactorPlus_Location.Text = (locationLevelInfo.DeepFactorPlus.Value).ToString();
        else
        {
            if (!acctDeepFactorPlus.HasValue)
                this.lblDeepFactorPlusFrom_Location.Text = "(from Firmware)";
            else
                this.lblDeepFactorPlusFrom_Location.Text = "(from Account)";
        }

        // Eye Factor+.
        if (locationLevelInfo.EyeFactorPlus.HasValue)
            this.txtEyeFactorPlus_Location.Text = (locationLevelInfo.EyeFactorPlus.Value).ToString();
        else
        {
            if (!acctEyeFactorPlus.HasValue)
                this.lblEyeFactorPlusFrom_Location.Text = "(from Firmware)";
            else
                this.lblEyeFactorPlusFrom_Location.Text = "(from Account)";
        }

        // Shallow Factor+.
        if (locationLevelInfo.ShallowFactorPlus.HasValue)
            this.txtShallowFactorPlus_Location.Text = (locationLevelInfo.ShallowFactorPlus.Value).ToString();
        else
        {
            if (!acctShallowFactorPlus.HasValue)
                this.lblShallowFactorPlusFrom_Location.Text = "(from Firmware)";
            else
                this.lblShallowFactorPlusFrom_Location.Text = "(from Account)";
        }

        // Background Rate+.
        if (locationLevelInfo.BackgroundRatePlus.HasValue)
            this.txtBackgroundRatePlus_Location.Text = (locationLevelInfo.BackgroundRatePlus.Value).ToString();
        else
        {
            if (!acctBackgroundRatePlus.HasValue)
            {
                if (!stateBackgroundRate.HasValue)
                    this.lblBackgroundRatePlusFrom_Location.Text = "(from Country)";
                else
                    this.lblBackgroundRatePlusFrom_Location.Text = "(from State)";
            }
            else
                this.lblBackgroundRatePlusFrom_Location.Text = "(from Account)";
        }

        // MRD 2 (ID2).
        if (locationLevelInfo.MRD2.HasValue)
            this.txtMRD2_Location.Text = (locationLevelInfo.MRD2.Value).ToString();
        else
        {
            if (!acctMRD2.HasValue)
                this.lblMRD2From_Location.Text = "(from Application)";
            else
                this.lblMRD2From_Location.Text = "(from Account)";
        }

        // MRD Increment 2 (ID2).
        if (locationLevelInfo.MRDIncrement2.HasValue)
            this.txtMRDIncrement2_Location.Text = (locationLevelInfo.MRDIncrement2.Value).ToString();
        else
        {
            if (!acctMRDIncrement2.HasValue)
                this.lblMRDIncrement2From_Location.Text = "(from Application)";
            else
                this.lblMRDIncrement2From_Location.Text = "(from Account)";
        }

        // Background Rate 2.
        if (locationLevelInfo.BackgroundRate2.HasValue)
            this.txtBackgroundRate2_Location.Text = (locationLevelInfo.BackgroundRate2.Value).ToString();
        else
        {
            if (!acctBackgroundRate2.HasValue)
            {
                if (!stateBackgroundRate.HasValue)
                    this.lblBackgroundRate2From_Location.Text = "(from Country)";
                else
                    this.lblBackgroundRate2From_Location.Text = "(from State)";
            }
            else
                this.lblBackgroundRate2From_Location.Text = "(from Account)";
        }

        // Read Limit.
        if (locationLevelInfo.ReadLimit.HasValue)
            this.txtReadLimit_Location.Text = (locationLevelInfo.ReadLimit.Value).ToString();
        else
        {
            if (!acctReadLimit.HasValue)
                this.lblReadLimitFrom_Location.Text = "(from Application)";
            else
                this.lblReadLimitFrom_Location.Text = "(from Account)";
        }

        // Algorithm Factor.
        if (locationLevelInfo.AlgorithmFactor != null)
            this.txtAlgorithmFactor_Location.Text = locationLevelInfo.AlgorithmFactor;
        else
        {
            if (acctAlgorithmFactor == null)
                this.lblAlgorithmFactorFrom_Location.Text = "(from Application)";
            else
                this.lblAlgorithmFactorFrom_Location.Text = "(from Account)";
        }
            
        // Placeholder will always be present, in case User decideds not to enter an Location-Level value.
        this.txtMRD_Location.Attributes.Add("placeholder", acctMRD.HasValue ? acctMRD.Value.ToString() : appMRD.ToString());
        this.txtMRDIncrement_Location.Attributes.Add("placeholder", acctMRDIncrement.HasValue ? acctMRDIncrement.Value.ToString() : appMRDIncrement.ToString());        
        this.txtDeepFactor_Location.Attributes.Add("placeholder", acctDeepFactor.HasValue ? acctDeepFactor.Value.ToString() : dfDeepFactor.ToString());
        this.txtEyeFactor_Location.Attributes.Add("placeholder", acctEyeFactor.HasValue ? acctEyeFactor.Value.ToString() : dfEyeFactor.ToString());
        this.txtShallowFactor_Location.Attributes.Add("placeholder", acctShallowFactor.HasValue ? acctShallowFactor.Value.ToString() : dfShallowFactor.ToString());
        this.txtBackgroundRate_Location.Attributes.Add("placeholder", acctBackgroundRate.HasValue ? acctBackgroundRate.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString());

        this.txtMRDPlus_Location.Attributes.Add("placeholder", acctMRDPlus.HasValue ? acctMRDPlus.Value.ToString() : appMRDPlus.ToString());
        this.txtMRDIncrementPlus_Location.Attributes.Add("placeholder", acctMRDIncrementPlus.HasValue ? acctMRDIncrementPlus.Value.ToString() : appMRDIncrementPlus.ToString());
        this.txtDeepFactorPlus_Location.Attributes.Add("placeholder", acctDeepFactorPlus.HasValue ? acctDeepFactorPlus.Value.ToString() : dfDeepFactor.ToString());
        this.txtEyeFactorPlus_Location.Attributes.Add("placeholder", acctEyeFactorPlus.HasValue ? acctEyeFactorPlus.Value.ToString() : dfEyeFactor.ToString());
        this.txtShallowFactorPlus_Location.Attributes.Add("placeholder", acctShallowFactorPlus.HasValue ? acctShallowFactorPlus.Value.ToString() : dfShallowFactor.ToString());
        this.txtBackgroundRatePlus_Location.Attributes.Add("placeholder", acctBackgroundRatePlus.HasValue ? acctBackgroundRatePlus.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString());

        this.txtMRD2_Location.Attributes.Add("placeholder", acctMRD2.HasValue ? acctMRD2.Value.ToString() : appMRD2.ToString());
        this.txtMRDIncrement2_Location.Attributes.Add("placeholder", acctMRDIncrement2.HasValue ? acctMRDIncrement2.Value.ToString() : appMRDIncrement2.ToString());
        this.txtBackgroundRate2_Location.Attributes.Add("placeholder", acctBackgroundRate2.HasValue ? acctBackgroundRate2.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString());

        this.txtReadLimit_Location.Attributes.Add("placeholder", acctReadLimit.HasValue ? acctReadLimit.Value.ToString() : appReadLimit.ToString());
        this.txtAlgorithmFactor_Location.Attributes.Add("placeholder", acctAlgorithmFactor != null ? acctAlgorithmFactor : appAlgorithmFactor);

        // Send Javascript to client to OPEN/DISPLAY the modal dialog.
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('divLocationSettingsDialog')", true);
    }

    /// <summary>
    /// Function codes Settings/Factors that ARE NOT inherited from another table.
    /// Location-Level are bold green text.
    /// Non-Location-Level are normal black text.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvLocationSettings_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        // Includes the last row in the GridView (per page) which is considered a "Footer".
        if (e.Row.RowType != DataControlRowType.DataRow &&
            e.Row.RowType != DataControlRowType.Footer) return;

        if (e.Row.RowIndex == -1) return;

        int indexOfMRD = this.GetColumnIndexByName(e.Row, "MRD");
        int indexOfMRDIncrement = this.GetColumnIndexByName(e.Row, "MRDIncrement");        
        int indexOfDeepFactor = this.GetColumnIndexByName(e.Row, "DeepFactor");
        int indexOfEyeFactor = this.GetColumnIndexByName(e.Row, "EyeFactor");
        int indexOfShallowFactor = this.GetColumnIndexByName(e.Row, "ShallowFactor");
        int indexOfBackgroundRate = this.GetColumnIndexByName(e.Row, "BackgroundRate");

        int indexOfMRDPlus = this.GetColumnIndexByName(e.Row, "MRDPlus");
        int indexOfMRDIncrementPlus = this.GetColumnIndexByName(e.Row, "MRDIncrementPlus");
        int indexOfDeepFactorPlus = this.GetColumnIndexByName(e.Row, "DeepFactorPlus");
        int indexOfEyeFactorPlus = this.GetColumnIndexByName(e.Row, "EyeFactorPlus");
        int indexOfShallowFactorPlus = this.GetColumnIndexByName(e.Row, "ShallowFactorPlus");
        int indexOfBackgroundRatePlus = this.GetColumnIndexByName(e.Row, "BackgroundRatePlus");

        int indexOfMRD2 = this.GetColumnIndexByName(e.Row, "MRD2");
        int indexOfMRDIncrement2 = this.GetColumnIndexByName(e.Row, "MRDIncrement2");
        int indexOfBackgroundRate2 = this.GetColumnIndexByName(e.Row, "BackgroundRate2");

        int indexOfReadLimit = this.GetColumnIndexByName(e.Row, "ReadLimit");
        int indexOfAlgorithmFactor = this.GetColumnIndexByName(e.Row, "AlgorithmFactor");
        
        // MRD
        if (locationFactors[e.Row.DataItemIndex].MRDFrom == 'L')
            e.Row.Cells[indexOfMRD].CssClass = "AlteredField";

        // MRD Increment
        if (locationFactors[e.Row.DataItemIndex].MRDIncrementFrom == 'L')
            e.Row.Cells[indexOfMRDIncrement].CssClass = "AlteredField";        

        // Deep Factor
        if (locationFactors[e.Row.DataItemIndex].DeepFactorFrom == 'L')
            e.Row.Cells[indexOfDeepFactor].CssClass = "AlteredField";

        // Eye Factor
        if (locationFactors[e.Row.DataItemIndex].EyeFactorFrom == 'L')
            e.Row.Cells[indexOfEyeFactor].CssClass = "AlteredField";

        // Shallow Factor
        if (locationFactors[e.Row.DataItemIndex].ShallowFactorFrom == 'L')
            e.Row.Cells[indexOfShallowFactor].CssClass = "AlteredField";

        // Background Rate
        if (locationFactors[e.Row.DataItemIndex].BackgroundRateFrom == 'L')
            e.Row.Cells[indexOfBackgroundRate].CssClass = "AlteredField";

        // MRD+
        if (locationFactors[e.Row.DataItemIndex].MRDPlusFrom == 'L')
            e.Row.Cells[indexOfMRDPlus].CssClass = "AlteredField";

        // MRD Increment+
        if (locationFactors[e.Row.DataItemIndex].MRDIncrementPlusFrom == 'L')
            e.Row.Cells[indexOfMRDIncrementPlus].CssClass = "AlteredField";

        // Deep Factor+
        if (locationFactors[e.Row.DataItemIndex].DeepFactorPlusFrom == 'L')
            e.Row.Cells[indexOfDeepFactorPlus].CssClass = "AlteredField";

        // Eye Factor+
        if (locationFactors[e.Row.DataItemIndex].EyeFactorPlusFrom == 'L')
            e.Row.Cells[indexOfEyeFactorPlus].CssClass = "AlteredField";

        // Shallow Factor+
        if (locationFactors[e.Row.DataItemIndex].ShallowFactorPlusFrom == 'L')
            e.Row.Cells[indexOfShallowFactorPlus].CssClass = "AlteredField";

        // Background Rate+
        if (locationFactors[e.Row.DataItemIndex].BackgroundRatePlusFrom == 'L')
            e.Row.Cells[indexOfBackgroundRatePlus].CssClass = "AlteredField";

        // MRD2 (ID2)
        if (locationFactors[e.Row.DataItemIndex].MRD2From == 'L')
            e.Row.Cells[indexOfMRD2].CssClass = "AlteredField";

        // MRD Increment 2 (ID2)
        if (locationFactors[e.Row.DataItemIndex].MRDIncrement2From == 'L')
            e.Row.Cells[indexOfMRDIncrement2].CssClass = "AlteredField";

        // Background Rate 2
        if (locationFactors[e.Row.DataItemIndex].BackgroundRate2From == 'L')
            e.Row.Cells[indexOfBackgroundRate2].CssClass = "AlteredField";

        // Read Limit
        if (locationFactors[e.Row.DataItemIndex].ReadLimitFrom == 'L')
            e.Row.Cells[indexOfReadLimit].CssClass = "AlteredField";

        // Algorithm Factor (ID2)
        if (locationFactors[e.Row.DataItemIndex].AlgorithmFactorFrom == 'L')
            e.Row.Cells[indexOfAlgorithmFactor].CssClass = "AlteredField";
    }

    /// <summary>
    /// Maintains GridView/DataGrid while paging.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvLocationSettings_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        this.GetLocationLevelSettings(accountID);
        this.gvLocationSettings.PageIndex = e.NewPageIndex;
        this.gvLocationSettings.DataBind();
    }

    /// <summary>
    /// ON_CLICK will Update Location-Level Settings for given LocationID.
    /// If any TextBoxes are left blank (only Placeholder value), then those values will be left NULL in the Locations table.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void btnUpdate_Location_Click(object sender, EventArgs e)
    {
        // Get LocationID.
        int.TryParse(this.hdnfldLocationID.Value, out locationID);

        if (locationID == 0) return;

        // Get Location Name.
        string locationName = this.lblLocationName.Text;

        // Get Location record from table.
        Location loc = (from l in idc.Locations
                        where l.LocationID == locationID
                        select l).FirstOrDefault();

        if (loc == null) return;

        // Set all values to NULL (nullable variables).
        int? mrd = null;
        int? mrdIncrement = null;        
        double? deepFactor = null;
        double? eyeFactor = null;
        double? shallowFactor = null;
        double? backgroundRate = null;

        int? mrdPlus = null;
        int? mrdIncrementPlus = null;
        double? deepFactorPlus = null;
        double? eyeFactorPlus = null;
        double? shallowFactorPlus = null;
        double? backgroundRatePlus = null;

        int? mrd2 = null;
        int? mrdIncrement2 = null;
        double? backgroundRate2 = null;

        int? readLimit = null;
        string algorithmFactor = null;

        if (this.txtMRD_Location.Text.Trim() != "") mrd = Convert.ToInt32(this.txtMRD_Location.Text.Trim());
        if (this.txtMRDIncrement_Location.Text.Trim() != "") mrdIncrement = Convert.ToInt32(this.txtMRDIncrement_Location.Text.Trim());        
        if (this.txtDeepFactor_Location.Text.Trim() != "") deepFactor = Convert.ToDouble(this.txtDeepFactor_Location.Text.Trim());
        if (this.txtEyeFactor_Location.Text.Trim() != "") eyeFactor = Convert.ToDouble(this.txtEyeFactor_Location.Text.Trim());
        if (this.txtShallowFactor_Location.Text.Trim() != "") shallowFactor = Convert.ToDouble(this.txtShallowFactor_Location.Text.Trim());
        if (this.txtBackgroundRate_Location.Text.Trim() != "") backgroundRate = Convert.ToDouble(this.txtBackgroundRate_Location.Text.Trim());

        if (this.txtMRDPlus_Location.Text.Trim() != "") mrdPlus = Convert.ToInt32(this.txtMRDPlus_Location.Text.Trim());
        if (this.txtMRDIncrementPlus_Location.Text.Trim() != "") mrdIncrementPlus = Convert.ToInt32(this.txtMRDIncrementPlus_Location.Text.Trim());
        if (this.txtDeepFactorPlus_Location.Text.Trim() != "") deepFactorPlus = Convert.ToDouble(this.txtDeepFactorPlus_Location.Text.Trim());
        if (this.txtEyeFactorPlus_Location.Text.Trim() != "") eyeFactorPlus = Convert.ToDouble(this.txtEyeFactorPlus_Location.Text.Trim());
        if (this.txtShallowFactorPlus_Location.Text.Trim() != "") shallowFactorPlus = Convert.ToDouble(this.txtShallowFactorPlus_Location.Text.Trim());
        if (this.txtBackgroundRatePlus_Location.Text.Trim() != "") backgroundRatePlus = Convert.ToDouble(this.txtBackgroundRatePlus_Location.Text.Trim());

        if (this.txtMRD2_Location.Text.Trim() != "") mrd2 = Convert.ToInt32(this.txtMRD2_Location.Text.Trim());
        if (this.txtMRDIncrement2_Location.Text.Trim() != "") mrdIncrement2 = Convert.ToInt32(this.txtMRDIncrement2_Location.Text.Trim());
        if (this.txtBackgroundRate2_Location.Text.Trim() != "") backgroundRate2 = Convert.ToDouble(this.txtBackgroundRate2_Location.Text.Trim());

        if (this.txtReadLimit_Location.Text.Trim() != "") readLimit = Convert.ToInt32(this.txtReadLimit_Location.Text.Trim());
        if (this.txtAlgorithmFactor_Location.Text.Trim() != "") algorithmFactor = this.txtAlgorithmFactor_Location.Text.Trim();

        // If TextBox is string.Empty/"", then insert NULL for Location-Level setting(s).
        if (mrd.HasValue) loc.MRD = mrd.Value;
        else loc.MRD = null;
        if (mrdIncrement.HasValue) loc.MRDIncrement = mrdIncrement.Value;
        else loc.MRDIncrement = null;       
        if (deepFactor.HasValue) loc.DeepFactor = deepFactor.Value;
        else loc.DeepFactor = null;
        if (eyeFactor.HasValue) loc.EyeFactor = eyeFactor.Value;
        else loc.EyeFactor = null;
        if (shallowFactor.HasValue) loc.ShallowFactor = shallowFactor.Value;
        else loc.ShallowFactor = null;
        if (backgroundRate.HasValue) loc.BackgroundRate = backgroundRate.Value;
        else loc.BackgroundRate = null;

        if (mrdPlus.HasValue) loc.MRDPlus = mrdPlus.Value;
        else loc.MRDPlus = null;
        if (mrdIncrementPlus.HasValue) loc.MRDIncrementPlus = mrdIncrementPlus.Value;
        else loc.MRDIncrementPlus = null;
        if (deepFactorPlus.HasValue) loc.DeepFactorPlus = deepFactorPlus.Value;
        else loc.DeepFactorPlus = null;
        if (eyeFactorPlus.HasValue) loc.EyeFactorPlus = eyeFactorPlus.Value;
        else loc.EyeFactorPlus = null;
        if (shallowFactorPlus.HasValue) loc.ShallowFactorPlus = shallowFactorPlus.Value;
        else loc.ShallowFactorPlus = null;
        if (backgroundRatePlus.HasValue) loc.BackgroundRatePlus = backgroundRatePlus.Value;
        else loc.BackgroundRatePlus = null;

        if (mrd2.HasValue) loc.MRD2 = mrd2.Value;
        else loc.MRD2 = null;
        if (mrdIncrement2.HasValue) loc.MRDIncrement2 = mrdIncrement2.Value;
        else loc.MRDIncrement2 = null;
        if (backgroundRate2.HasValue) loc.BackgroundRate2 = backgroundRate2.Value;
        else loc.BackgroundRate2 = null;

        if (readLimit.HasValue) loc.ReadLimit = readLimit.Value;
        else loc.ReadLimit = null;
        if (algorithmFactor != null) loc.AlgorithmFactor = algorithmFactor;
        else loc.AlgorithmFactor = null;

        try
        {
            // Update Location values where applicable.
            idc.SubmitChanges();

            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divLocationSettingsDialog')", true);
            
            this.VisibleSuccesses(string.Format("Settings for {0} were updated.", locationName));

            // Rebind GridView to display updated Inheritance values (where applicable).
            this.GetLocationLevelSettings(accountID);
        }
        catch
        {
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divLocationSettingsDialog')", true);
            this.VisibleErrors(string.Format("An error occurred while updating {0}.", locationName));
            return;
        }
    }
    #endregion

    protected void btnBackToFactorsSearch_Click(object sender, EventArgs e)
    {
        Response.Redirect("FactorsSearch.aspx");
    }
}