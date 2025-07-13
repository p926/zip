using System;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Instadose;
using Instadose.Data;
using System.Collections.Generic;

public partial class TechOps_Factors_New : System.Web.UI.Page
{
    InsDataContext idc = new InsDataContext();

    public string account = "";
    public int accountID = 0;
    public int locationID = 0;
    public string applicationID = "";

    // SP for Location-Level Factors as List<> Object.
    private List<sp_GetLocationFactors_NewResult> locationFactors;

    // String to hold the current Username.
    string userName = "Unknown";

    #region ON PAGE LOAD.
    protected void Page_Load(object sender, EventArgs e)
    {
        // Try to set the Username.
        try { this.userName = User.Identity.Name.Split('\\')[1]; }
        catch { this.userName = "Unknown"; }

        account = Request.QueryString["AccountID"];
        applicationID = Request.QueryString["ApplicationID"];
        if (account == "0" || applicationID == "0") Response.Redirect("FactorsSearch.aspx");

        // applicationID = 2: portal acct; applicationID = 1: CSWS acct;
        if (applicationID == "2")
        {
            accountID = Int32.Parse(account);
        }
        else
        {
            accountID = (from a in idc.Accounts where a.GDSAccount == account && a.Active == true select a.AccountID).FirstOrDefault();
        }
        if (accountID == 0) Response.Redirect("FactorsSearch.aspx");

        if (IsPostBack) return;

        this.ResetAllControls();

        // Get Company Name based on AccountID (QueryString).
        string companyName = (from acct in idc.Accounts where acct.AccountID == accountID select acct.CompanyName).FirstOrDefault();

        // Display Account ID/# with Company Name.
        this.lblAccountIDAndCompanyName.Text = string.Format("{0} ({1})", account, companyName);
        this.lblAccountIDAndCompanyName.ToolTip = string.Format("Account: {0} ({1})", account, companyName);

        // Load/Populate GridView with Location-Level Settings.
        this.GetLocationLevelSettings(accountID);
    }
    #endregion

    #region LOCATION-LEVEL FACTORS GRIDVIEW FUNCTION(S).
    /// <summary>
    /// Populate/Fill gvLocationSettings_ID1 GridView with Location-Level Settings/Factors.
    /// </summary>
    /// <param name="accountid"></param>
    private void GetLocationLevelSettings(int accountid)
    {
        locationFactors = idc.sp_GetLocationFactors_New(accountid).ToList();

        this.gvLocationSettings_ID1.DataSource = locationFactors;
        this.gvLocationSettings_ID1.DataBind();

        this.gvLocationSettings_ID2.DataSource = locationFactors;
        this.gvLocationSettings_ID2.DataBind();

        this.gvLocationSettings_ID2Elite.DataSource = locationFactors;
        this.gvLocationSettings_ID2Elite.DataBind();

        this.gvLocationSettings_IDPlus.DataSource = locationFactors;
        this.gvLocationSettings_IDPlus.DataBind();

        this.gvLocationSettings_IDVue.DataSource = locationFactors;
        this.gvLocationSettings_IDVue.DataBind();
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
    private void GetAccountSettings(int accountid, out string companyname, out int? mrd, out int? mrdincrement, out int? mrd2, out int? mrdincrement2, out double? deepfactor, out double? eyefactor, out double? shallowfactor, out double? backgroundrate, out double? backgroundrate2, out double? readlimit, out string algorithmfactor, out int? mrdPlus, out int? mrdincrementPlus, out double? deepfactorPlus, out double? eyefactorPlus, out double? shallowfactorPlus, out double? backgroundratePlus, out int? mrd2New, out int? mrdincrement2New, out double? backgroundrate2New, out int? mrdVue, out double? backgroundRateVue, out double? deepfactorVue, out int? mrdincrementVue, out double? eyefactorVue, out double? shallowfactorVue)
    {
        var accountLevelInfo = (from acct in idc.Accounts
                                where acct.AccountID == accountid
                                select new
                                {
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
                                    AlgorithmFactor = acct.AlgorithmFactor,
                                    MRD2New = acct.MRD2New,
                                    MRDIncrement2New = acct.MRDIncrement2New,
                                    BackgroundRate2New = acct.BackgroundRate2New,
                                    ReadLimit = acct.ReadLimit,
                                    MRDVue = acct.MRDVue,
                                    MRDIncrementVue = acct.MRDIncrementVue,
                                    EyeFactorVue = acct.EyeFactorVue,
                                    ShallowFactorVue = acct.ShallowFactorVue,
                                    BackgroundRateVue = acct.BackgroundRateVue,
                                    DeepFactorVue = acct.DeepFactorVue
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
        algorithmfactor = accountLevelInfo.AlgorithmFactor;
        mrd2New = accountLevelInfo.MRD2New;
        mrdincrement2New = accountLevelInfo.MRDIncrement2New;
        backgroundrate2New = accountLevelInfo.BackgroundRate2New;
        readlimit = accountLevelInfo.ReadLimit;
        mrdVue = accountLevelInfo.MRDVue;
        mrdincrementVue = accountLevelInfo.MRDIncrementVue;
        eyefactorVue = accountLevelInfo.EyeFactorVue;
        shallowfactorVue = accountLevelInfo.ShallowFactorVue;
        backgroundRateVue = accountLevelInfo.BackgroundRateVue;
        deepfactorVue = accountLevelInfo.DeepFactorVue;
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
                                  select new
                                  {
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
                                   select new
                                   {
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
        this.spnErrorMessage.InnerHtml = "";
    }

    private void VisibleErrors(string error)
    {
        this.divErrorMessage.Visible = true;
        this.spnErrorMessage.InnerHtml = error;
    }
    #endregion

    #region SUCCESS MESSAGE.
    private void InvisibleSuccesses()
    {
        this.divSuccessMessage.Visible = false;
        this.spnSuccessMessage.InnerHtml = "";
    }

    private void VisibleSuccesses(string success)
    {
        this.divSuccessMessage.Visible = true;
        this.spnSuccessMessage.InnerHtml = success;
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
        this.lblAccountID_Account.Text = string.Empty;
        this.lblCompanyName.Text = string.Empty;

        this.txtMRD_Account.Text = string.Empty;
        this.txtMRDIncrement_Account.Text = string.Empty;
        this.txtDeepFactor_Account.Text = string.Empty;
        this.txtEyeFactor_Account.Text = string.Empty;
        this.txtShallowFactor_Account.Text = string.Empty;
        this.txtBackgroundRate_Account.Text = string.Empty;

        this.txtMRDPlus_Account.Text = string.Empty;
        this.txtMRDIncrementPlus_Account.Text = string.Empty;
        this.txtDeepFactorPlus_Account.Text = string.Empty;
        this.txtEyeFactorPlus_Account.Text = string.Empty;
        this.txtShallowFactorPlus_Account.Text = string.Empty;
        this.txtBackgroundRatePlus_Account.Text = string.Empty;

        this.txtMRD2_Account.Text = string.Empty;
        this.txtMRDIncrement2_Account.Text = string.Empty;
        this.txtBackgroundRate2_Account.Text = string.Empty;

        this.txtReadLimit_Account.Text = string.Empty;
        this.txtAlgorithmFactor_Account.Text = string.Empty;

        this.txtMRDVue.Text = string.Empty;
        this.txtBackgroundRateVue.Text = string.Empty;
        this.txtDeepfactorVue.Text = string.Empty;

        // Location Settings Modal/Dialog.
        this.hdnfldLocationID.Value = "0";
        this.lblAccountID_Location.Text = string.Empty;
        this.lblLocationName.Text = string.Empty;

        this.txtMRD_Location.Text = string.Empty;
        this.txtMRDIncrement_Location.Text = string.Empty;
        this.txtDeepFactor_Location.Text = string.Empty;
        this.txtEyeFactor_Location.Text = string.Empty;
        this.txtShallowFactor_Location.Text = string.Empty;
        this.txtBackgroundRate_Location.Text = string.Empty;

        this.txtMRDPlus_Location.Text = string.Empty;
        this.txtMRDIncrementPlus_Location.Text = string.Empty;
        this.txtDeepFactorPlus_Location.Text = string.Empty;
        this.txtEyeFactorPlus_Location.Text = string.Empty;
        this.txtShallowFactorPlus_Location.Text = string.Empty;
        this.txtBackgroundRatePlus_Location.Text = string.Empty;

        this.txtMRD2_Location.Text = string.Empty;
        this.txtMRDIncrement2_Location.Text = string.Empty;
        this.txtBackgroundRate2_Location.Text = string.Empty;
        this.txtAlgorithmFactor_Location.Text = string.Empty;

        this.txtMRD2New_Location.Text = string.Empty;
        this.txtMRDIncrement2New_Location.Text = string.Empty;
        this.txtBackgroundRate2New_Location.Text = string.Empty;

        this.txtReadLimit_Location.Text = string.Empty;

        this.txtMRDVue_Location.Text = string.Empty;
        this.txtMRDIncrementVue_Location.Text = string.Empty;
        this.txtEyeFactorVue_Location.Text = string.Empty;
        this.txtShallowFactorVue_Location.Text = string.Empty;
        this.txtBackgroundRateVue_Location.Text = string.Empty;
        this.txtDeepFactorVue_Location.Text = string.Empty;
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

        this.txtNotes_Account.Text = "";
        this.lblAccountID_Account.Text = this.account;
        //this.lblCompanyName.Text = acctCompanyName;
        this.lblCompanyName.Text = "Account Settings";

        // Use this variable to save the displaying factor values in the controls when first loading
        SettingFactors curDisplayFactorValues = new SettingFactors();

        string acctCompanyName = "";
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
        string acctAlgorithmFactor = null;

        int? acctMRD2New = null;
        int? acctMRDIncrement2New = null;
        double? acctBackgroundRate2New = null;

        double? acctReadLimit = null;

        int? acctMRDVue = null;
        int? acctMRDIncrementVue = null;
        double? acctbackgroundRateVue = null;
        double? acctdeepFactorVue = null;
        double? acctEyeFactorVue = null;
        double? acctShallowFactorVue = null;
        // ------------------------------------AccountSettings ----------------------------------//
        this.GetAccountSettings(this.accountID, out acctCompanyName, out acctMRD, out acctMRDIncrement, out acctMRD2, out acctMRDIncrement2, out acctDeepFactor, out acctEyeFactor, out acctShallowFactor, out acctBackgroundRate, out acctBackgroundRate2, out acctReadLimit, out acctAlgorithmFactor, out acctMRDPlus, out acctMRDIncrementPlus, out acctDeepFactorPlus, out acctEyeFactorPlus, out acctShallowFactorPlus, out acctBackgroundRatePlus, out acctMRD2New, out acctMRDIncrement2New, out acctBackgroundRate2New, out acctMRDVue, out acctbackgroundRateVue, out acctdeepFactorVue, out acctMRDIncrementVue, out acctEyeFactorVue, out acctShallowFactorVue);
        // --------------------------------------------------------------------------------------//
        // ------------------------------------ AppSettings.-------------------------------------//
        string appMRD = Basics.GetSetting("MRD");
        string appMRDIncrement = Basics.GetSetting("MRDIncrement");

        string appMRDPlus = Basics.GetSetting("MRDPlus");
        string appMRDIncrementPlus = Basics.GetSetting("MRDIncrementPlus");

        string appMRD2 = Basics.GetSetting("MRDID2");
        string appMRDIncrement2 = Basics.GetSetting("MRDIncrementID2");
        string appAlgorithmFactor = Basics.GetSetting("AlgorithmFactorID2");

        string appMRD2New = Basics.GetSetting("MRDID2New");
        string appMRDIncrement2New = Basics.GetSetting("MRDIncrementID2New");

        string appReadLimit = Basics.GetSetting("ReadLimit");

        string appMRDVue = Basics.GetSetting("MRDIDVue");
        string appMRDIncrementVue = Basics.GetSetting("MRDIncrementIDVue");

        string appBackgroundRateVue = Basics.GetSetting("ID VUE Background");
        
        // DeviceFirmware.
        double dfDeepFactor = 0;
        double dfEyeFactor = 0;
        double dfShallowFactor = 0;
        this.GetDeviceFirmware(out dfDeepFactor, out dfEyeFactor, out dfShallowFactor);

        // Control Badge, State & Country.
        double? stateBackgroundRate = null;
        double? countryBackgroundRate = null;
        double? controlBackgroundRate = null;

        // MRD Vue
        int? mrdVue = null;

        double dfDeepFactorVue = 1;
        double dfEyeFactorVue = 1;
        double dfShallowFactorVue = 1;

        this.GetStateAndCountry(accountID, out stateBackgroundRate, out countryBackgroundRate);

        var cbrDLObj = (from cbr in idc.ControlBadgeReads
                        where
                        cbr.AccountID == this.accountID
                        && !cbr.LocationID.HasValue
                        && cbr.AvgDLRate.HasValue
                        && cbr.PermEliminatedDL == false
                        && cbr.IsEliminatedDL == false
                        && cbr.ReadTypeID == 17
                        orderby cbr.DoseDate descending
                        select cbr).FirstOrDefault();
        if (cbrDLObj != null) controlBackgroundRate = cbrDLObj.AvgDLRate.Value;

        string state_country_BackgroundRate = stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
        string controlBadge_country_BackgroundRate = controlBackgroundRate.HasValue ? controlBackgroundRate.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
        // --------------------------------------------------------------------------------------//

        // ---------------------- MRD -----------------------------------------------------------//
        if (acctMRD.HasValue)
        {
            this.txtMRD_Account.Text = (acctMRD.Value).ToString();
            curDisplayFactorValues.mrd = (acctMRD.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrd = appMRD;
        }

        // MRD Increment.
        if (acctMRDIncrement.HasValue)
        {
            this.txtMRDIncrement_Account.Text = (acctMRDIncrement.Value).ToString();
            curDisplayFactorValues.mrdIncrement = (acctMRDIncrement.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrement = appMRDIncrement;
        }

        // Deep Factor.
        if (acctDeepFactor.HasValue)
        {
            this.txtDeepFactor_Account.Text = (acctDeepFactor.Value).ToString();
            curDisplayFactorValues.deepFactor = (acctDeepFactor.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.deepFactor = dfDeepFactor.ToString();
        }

        // Eye Factor.
        if (acctEyeFactor.HasValue)
        {
            this.txtEyeFactor_Account.Text = (acctEyeFactor.Value).ToString();
            curDisplayFactorValues.eyeFactor = (acctEyeFactor.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.eyeFactor = dfEyeFactor.ToString();
        }

        // Shallow Factor.
        if (acctShallowFactor.HasValue)
        {
            this.txtShallowFactor_Account.Text = (acctShallowFactor.Value).ToString();
            curDisplayFactorValues.shallowFactor = (acctShallowFactor.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.shallowFactor = dfShallowFactor.ToString();
        }

        // Background Rate.
        if (acctBackgroundRate.HasValue)
        {
            this.txtBackgroundRate_Account.Text = (acctBackgroundRate.Value).ToString();
            curDisplayFactorValues.backgroundRate = (acctBackgroundRate.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRate = state_country_BackgroundRate;
        }


        //------------------------------ MRD+ ------------------------------------------------------//
        if (acctMRDPlus.HasValue)
        {
            this.txtMRDPlus_Account.Text = (acctMRDPlus.Value).ToString();
            curDisplayFactorValues.mrdPlus = (acctMRDPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdPlus = appMRDPlus;
        }

        // MRD Increment+.
        if (acctMRDIncrementPlus.HasValue)
        {
            this.txtMRDIncrementPlus_Account.Text = (acctMRDIncrementPlus.Value).ToString();
            curDisplayFactorValues.mrdIncrementPlus = (acctMRDIncrementPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrementPlus = appMRDIncrementPlus;
        }

        // Deep Factor+.
        if (acctDeepFactorPlus.HasValue)
        {
            this.txtDeepFactorPlus_Account.Text = (acctDeepFactorPlus.Value).ToString();
            curDisplayFactorValues.deepFactorPlus = (acctDeepFactorPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.deepFactorPlus = dfDeepFactor.ToString();
        }

        // Eye Factor+.
        if (acctEyeFactorPlus.HasValue)
        {
            this.txtEyeFactorPlus_Account.Text = (acctEyeFactorPlus.Value).ToString();
            curDisplayFactorValues.eyeFactorPlus = (acctEyeFactorPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.eyeFactorPlus = dfEyeFactor.ToString();
        }

        // Shallow Factor+.
        if (acctShallowFactorPlus.HasValue)
        {
            this.txtShallowFactorPlus_Account.Text = (acctShallowFactorPlus.Value).ToString();
            curDisplayFactorValues.shallowFactorPlus = (acctShallowFactorPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.shallowFactorPlus = dfShallowFactor.ToString();
        }

        // Background Rate+.
        if (acctBackgroundRatePlus.HasValue)
        {
            this.txtBackgroundRatePlus_Account.Text = (acctBackgroundRatePlus.Value).ToString();
            curDisplayFactorValues.backgroundRatePlus = (acctBackgroundRatePlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRatePlus = controlBadge_country_BackgroundRate;
        }


        //------------------------------- MRD 2 (ID2) -------------------------------------------------------//
        if (acctMRD2.HasValue)
        {
            this.txtMRD2_Account.Text = (acctMRD2.Value).ToString();
            curDisplayFactorValues.mrd2 = (acctMRD2.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrd2 = appMRD2;
        }

        // MRD Increment 2 (ID2).
        if (acctMRDIncrement2.HasValue)
        {
            this.txtMRDIncrement2_Account.Text = (acctMRDIncrement2.Value).ToString();
            curDisplayFactorValues.mrdIncrement2 = (acctMRDIncrement2.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrement2 = appMRDIncrement2;
        }

        // Background Rate 2.
        if (acctBackgroundRate2.HasValue)
        {
            this.txtBackgroundRate2_Account.Text = (acctBackgroundRate2.Value).ToString();
            curDisplayFactorValues.backgroundRate2 = (acctBackgroundRate2.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRate2 = controlBadge_country_BackgroundRate;
        }

        // Algorithm Factor (ID2).
        if (acctAlgorithmFactor != null)
        {
            this.txtAlgorithmFactor_Account.Text = acctAlgorithmFactor;
            curDisplayFactorValues.algorithmFactor = acctAlgorithmFactor;
        }
        else
        {
            curDisplayFactorValues.algorithmFactor = appAlgorithmFactor;
        }


        //-------------------------------- MRD 2 New (ID2 New) ---------------------------------------- //
        if (acctMRD2New.HasValue)
        {
            this.txtMRD2New_Account.Text = (acctMRD2New.Value).ToString();
            curDisplayFactorValues.mrd2New = (acctMRD2New.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrd2New = appMRD2New;
        }

        // MRD Increment 2 New (ID2 New).
        if (acctMRDIncrement2New.HasValue)
        {
            this.txtMRDIncrement2New_Account.Text = (acctMRDIncrement2New.Value).ToString();
            curDisplayFactorValues.mrdIncrement2New = (acctMRDIncrement2New.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrement2New = appMRDIncrement2New;
        }

        // Background Rate 2 New.
        if (acctBackgroundRate2New.HasValue)
        {
            this.txtBackgroundRate2New_Account.Text = (acctBackgroundRate2New.Value).ToString();
            curDisplayFactorValues.backgroundRate2New = (acctBackgroundRate2New.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRate2New = controlBadge_country_BackgroundRate;
        }


        //------------------------- Read Limit ---------------------------------------------------//
        if (acctReadLimit.HasValue)
        {
            this.txtReadLimit_Account.Text = (acctReadLimit.Value).ToString();
            curDisplayFactorValues.readLimit = (acctReadLimit.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.readLimit = appReadLimit;
        }
        // ----------------------------------------------------------------------------------------//

        //------------------------------ IDVue ------------------------------------------------------//

        //------------------------------ IDVue ------------------------------------------------------//
        if (acctMRDVue.HasValue)
        {
            this.txtMRDVue.Text = (acctMRDVue.Value).ToString();
            curDisplayFactorValues.mrdVue = (acctMRDVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdVue = appMRDVue.ToString();
        }

        // MRD Increment Vue.
        if (acctMRDIncrementVue.HasValue)
        {
            this.txtMRDIncrementVue_Account.Text = (acctMRDIncrementVue.Value).ToString();
            curDisplayFactorValues.mrdIncrementVue = (acctMRDIncrementVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrementVue = appMRDIncrementVue;
        }

        // Eye Factor+.
        if (acctEyeFactorVue.HasValue)
        {
            this.txtEyeFactorVue_Account.Text = (acctEyeFactorVue.Value).ToString();
            curDisplayFactorValues.eyeFactorVue = (acctEyeFactorVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.eyeFactorVue = dfEyeFactorVue.ToString();
        }

        // Shallow Factor+.
        if (acctShallowFactorVue.HasValue)
        {
            this.txtShallowFactorVue_Account.Text = (acctShallowFactorVue.Value).ToString();
            curDisplayFactorValues.shallowFactorVue = (acctShallowFactorVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.shallowFactorVue = dfShallowFactorVue.ToString();
        }

        // Background Rate Vue.
        if (acctbackgroundRateVue.HasValue)
        {
            this.txtBackgroundRateVue.Text = (acctbackgroundRateVue.Value).ToString();
            curDisplayFactorValues.backgroundRateVue = (acctbackgroundRateVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRateVue = appBackgroundRateVue;
            //curDisplayFactorValues.backgroundRateVue = controlBadge_country_BackgroundRate;
        }

        // Deep Factor Vue
        if (acctdeepFactorVue.HasValue)
        {
            this.txtDeepfactorVue.Text = (acctdeepFactorVue.Value).ToString();
            curDisplayFactorValues.deepfactorVue = (acctdeepFactorVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.deepfactorVue = dfDeepFactorVue.ToString();
        }


        // Use this session to save all displaying factors values before commit change. Use this session to pass in previous factor value to FactorsHistoryLog table
        Session["CurrentDisplayFactorValues"] = null;
        Session["CurrentDisplayFactorValues"] = curDisplayFactorValues;

        // Placeholder will always be present, in case User decideds not to enter an Account-Level value.
        this.txtMRD_Account.Attributes.Add("placeholder", appMRD);
        this.txtMRDIncrement_Account.Attributes.Add("placeholder", appMRDIncrement);
        this.txtDeepFactor_Account.Attributes.Add("placeholder", dfDeepFactor.ToString());
        this.txtEyeFactor_Account.Attributes.Add("placeholder", dfEyeFactor.ToString());
        this.txtShallowFactor_Account.Attributes.Add("placeholder", dfShallowFactor.ToString());
        this.txtBackgroundRate_Account.Attributes.Add("placeholder", state_country_BackgroundRate);

        this.txtMRDPlus_Account.Attributes.Add("placeholder", appMRDPlus);
        this.txtMRDIncrementPlus_Account.Attributes.Add("placeholder", appMRDIncrementPlus);
        this.txtDeepFactorPlus_Account.Attributes.Add("placeholder", dfDeepFactor.ToString());
        this.txtEyeFactorPlus_Account.Attributes.Add("placeholder", dfEyeFactor.ToString());
        this.txtShallowFactorPlus_Account.Attributes.Add("placeholder", dfShallowFactor.ToString());
        this.txtBackgroundRatePlus_Account.Attributes.Add("placeholder", controlBadge_country_BackgroundRate);

        this.txtMRD2_Account.Attributes.Add("placeholder", appMRD2);
        this.txtMRDIncrement2_Account.Attributes.Add("placeholder", appMRDIncrement2);
        this.txtBackgroundRate2_Account.Attributes.Add("placeholder", controlBadge_country_BackgroundRate);
        this.txtAlgorithmFactor_Account.Attributes.Add("placeholder", appAlgorithmFactor);

        this.txtMRD2New_Account.Attributes.Add("placeholder", appMRD2New);
        this.txtMRDIncrement2New_Account.Attributes.Add("placeholder", appMRDIncrement2New);
        this.txtBackgroundRate2New_Account.Attributes.Add("placeholder", controlBadge_country_BackgroundRate);

        this.txtReadLimit_Account.Attributes.Add("placeholder", appReadLimit);

        this.txtMRDVue.Attributes.Add("placeholder", appMRDVue);
        this.txtMRDIncrementVue_Account.Attributes.Add("placeholder", appMRDIncrementVue);
        this.txtEyeFactorVue_Account.Attributes.Add("placeholder", dfEyeFactorVue.ToString());
        this.txtShallowFactorVue_Account.Attributes.Add("placeholder", dfShallowFactorVue.ToString());
        this.txtBackgroundRateVue.Attributes.Add("placeholder", appBackgroundRateVue);
        //this.txtBackgroundRateVue.Attributes.Add("placeholder", controlBadge_country_BackgroundRate);
        this.txtDeepfactorVue.Attributes.Add("placeholder", dfDeepFactorVue.ToString());



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
        // Update Account table where applicable.
        Account acct = (from a in idc.Accounts
                        where a.AccountID == this.accountID
                        select a).FirstOrDefault();

        if (acct == null) return;

        // Get current DisplayFactorValues
        SettingFactors curDisplayFactorValues = new SettingFactors();

        if (Session["CurrentDisplayFactorValues"] != null)
            curDisplayFactorValues = (SettingFactors)(Session["CurrentDisplayFactorValues"]);

        // -------------- Get prior settings values ----------------------//
        int? mrd_before = acct.MRD;
        int? mrdIncrement_before = acct.MRDIncrement;
        double? deepFactor_before = acct.DeepFactor;
        double? eyeFactor_before = acct.EyeFactor;
        double? shallowFactor_before = acct.ShallowFactor;
        double? backgroundRate_before = acct.BackgroundRate;

        int? mrdPlus_before = acct.MRDPlus;

        int? mrdIncrementPlus_before = acct.MRDIncrementPlus;
        double? deepFactorPlus_before = acct.DeepFactorPlus;
        double? eyeFactorPlus_before = acct.EyeFactorPlus;
        double? shallowFactorPlus_before = acct.ShallowFactorPlus;
        double? backgroundRatePlus_before = acct.BackgroundRatePlus;

        int? mrd2_before = acct.MRD2;
        int? mrdIncrement2_before = acct.MRDIncrement2;
        double? backgroundRate2_before = acct.BackgroundRate2;
        string algorithmFactor_before = acct.AlgorithmFactor;

        int? mrd2New_before = acct.MRD2New;
        int? mrdIncrement2New_before = acct.MRDIncrement2New;
        double? backgroundRate2New_before = acct.BackgroundRate2New;

        int? mrdVue_before = acct.MRDVue;
        int? mrdIncrementVue_before = acct.MRDIncrementVue;
        double? eyeFactorVue_before = acct.EyeFactorVue;
        double? shallowFactorVue_before = acct.ShallowFactorVue;
        double? backgroundRateVue_before = acct.BackgroundRateVue;
        double? deepFactorVue_before = acct.DeepFactorVue;

        double? readLimit_before = acct.ReadLimit;
        // ----------------------------------------------------------------- //

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
        string algorithmFactor = null;

        int? mrd2New = null;
        int? mrdIncrement2New = null;
        double? backgroundRate2New = null;

        int? MRDVue = null;
        int? mrdIncrementVue = null;
        double? eyeFactorVue = null;
        double? shallowFactorVue = null;
        double? backgroundRateVue = null;
        double? deepFactorVue = null;

        double? readLimit = null;

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
        if (this.txtAlgorithmFactor_Account.Text.Trim() != "") algorithmFactor = this.txtAlgorithmFactor_Account.Text.Trim().ToUpper();

        if (this.txtMRD2New_Account.Text.Trim() != "") mrd2New = Convert.ToInt32(this.txtMRD2New_Account.Text.Trim());
        if (this.txtMRDIncrement2New_Account.Text.Trim() != "") mrdIncrement2New = Convert.ToInt32(this.txtMRDIncrement2New_Account.Text.Trim());
        if (this.txtBackgroundRate2New_Account.Text.Trim() != "") backgroundRate2New = Convert.ToDouble(this.txtBackgroundRate2New_Account.Text.Trim());

        if (this.txtMRDVue.Text.Trim() != "") MRDVue = Convert.ToInt32(this.txtMRDVue.Text.Trim());
        if (this.txtMRDIncrementVue_Account.Text.Trim() != "") mrdIncrementVue = Convert.ToInt32(this.txtMRDIncrementVue_Account.Text.Trim());
        if (this.txtEyeFactorVue_Account.Text.Trim() != "") eyeFactorVue = Convert.ToDouble(this.txtEyeFactorVue_Account.Text.Trim());
        if (this.txtShallowFactorVue_Account.Text.Trim() != "") shallowFactorVue = Convert.ToDouble(this.txtShallowFactorVue_Account.Text.Trim());
        if (this.txtBackgroundRateVue.Text.Trim() != "") backgroundRateVue = Convert.ToDouble(this.txtBackgroundRateVue.Text.Trim());
        if (this.txtDeepfactorVue.Text.Trim() != "") deepFactorVue = Convert.ToDouble(this.txtDeepfactorVue.Text.Trim());


        if (this.txtReadLimit_Account.Text.Trim() != "") readLimit = Convert.ToDouble(this.txtReadLimit_Account.Text.Trim());

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
        if (algorithmFactor != null) acct.AlgorithmFactor = algorithmFactor;
        else acct.AlgorithmFactor = null;

        if (mrd2New.HasValue) acct.MRD2New = mrd2New.Value;
        else acct.MRD2New = null;
        if (mrdIncrement2New.HasValue) acct.MRDIncrement2New = mrdIncrement2New.Value;
        else acct.MRDIncrement2New = null;
        if (backgroundRate2New.HasValue) acct.BackgroundRate2New = backgroundRate2New.Value;
        else acct.BackgroundRate2New = null;

        if (mrd2New.HasValue) acct.MRD2New = mrd2New.Value;
        else acct.MRD2New = null;
        if (mrdIncrement2New.HasValue) acct.MRDIncrement2New = mrdIncrement2New.Value;
        else acct.MRDIncrement2New = null;
        if (backgroundRate2New.HasValue) acct.BackgroundRate2New = backgroundRate2New.Value;
        else acct.BackgroundRate2New = null;


        if (MRDVue.HasValue) acct.MRDVue = MRDVue.Value;
        else acct.MRDVue = null;
        if (mrdIncrementVue.HasValue) acct.MRDIncrementVue = mrdIncrementVue.Value;
        else acct.MRDIncrementVue = null;
        if (backgroundRateVue.HasValue) acct.BackgroundRateVue = backgroundRateVue.Value;
        else acct.BackgroundRateVue = null;
        if (deepFactorVue.HasValue) acct.DeepFactorVue = deepFactorVue.Value;
        else acct.DeepFactorVue = null;
        if (eyeFactorVue.HasValue) acct.EyeFactorVue = eyeFactorVue.Value;
        else acct.EyeFactorVue = null;
        if (shallowFactorVue.HasValue) acct.ShallowFactorVue = shallowFactorVue.Value;
        else acct.ShallowFactorVue = null;

        if (readLimit.HasValue) acct.ReadLimit = readLimit.Value;
        else acct.ReadLimit = null;

        try
        {
            idc.SubmitChanges();

            // --------------------------------------------------------------- Save Factors History Logs ---------------------------------------------------------------- //

            // Get all global settings to pass in Adj Values to Factors History Log in case we remove the account level settings
            // ------------------------------------ AppSettings.---------------------------------------------------------------//
            string appMRD = Basics.GetSetting("MRD");
            string appMRDIncrement = Basics.GetSetting("MRDIncrement");

            string appMRDPlus = Basics.GetSetting("MRDPlus");
            string appMRDIncrementPlus = Basics.GetSetting("MRDIncrementPlus");

            string appMRD2 = Basics.GetSetting("MRDID2");
            string appMRDIncrement2 = Basics.GetSetting("MRDIncrementID2");
            string appAlgorithmFactor = Basics.GetSetting("AlgorithmFactorID2");

            string appMRD2New = Basics.GetSetting("MRDID2New");
            string appMRDIncrement2New = Basics.GetSetting("MRDIncrementID2New");

            string appReadLimit = Basics.GetSetting("ReadLimit");

            string appMRDVue = Basics.GetSetting("MRDIDVue");
            string appMRDIncrementVue = Basics.GetSetting("MRDIncrementIDVue");
            string appBackgroundRateVue = Basics.GetSetting("ID VUE Background");

            // DeviceFirmware.
            double dfDeepFactor = 0;
            double dfEyeFactor = 0;
            double dfShallowFactor = 0;
            this.GetDeviceFirmware(out dfDeepFactor, out dfEyeFactor, out dfShallowFactor);

            // State & Country.
            double? stateBackgroundRate = null;
            double? countryBackgroundRate = null;
            double? acct_controlBackgroundRate = null;

            double? dfbackgroundRateVue = null;

            double dfDeepFactorVue = 1;
            double dfEyeFactorVue = 1;
            double dfShallowFactorVue = 1;


            this.GetStateAndCountry(accountID, out stateBackgroundRate, out countryBackgroundRate);

            var act_cbrDLObj = (from cbr in idc.ControlBadgeReads
                                where
                                cbr.AccountID == this.accountID
                                && !cbr.LocationID.HasValue
                                && cbr.AvgDLRate.HasValue
                                && cbr.PermEliminatedDL == false
                                && cbr.IsEliminatedDL == false
                                && cbr.ReadTypeID == 17
                                orderby cbr.DoseDate descending
                                select cbr).FirstOrDefault();
            if (act_cbrDLObj != null) acct_controlBackgroundRate = act_cbrDLObj.AvgDLRate.Value;

            string state_country_BackgroundRate = stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
            string controlBadge_country_BackgroundRate = acct_controlBackgroundRate.HasValue ? acct_controlBackgroundRate.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
            string idvue_country_BackgroundRate = dfbackgroundRateVue.HasValue ? dfbackgroundRateVue.HasValue.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.Value.ToString();

            // ----------------------------------------------------------------------------------------------------------------//

            // Insert any ID1 change
            if (acct.MRD != mrd_before)
                //InsertFactorHistoryLog(this.accountID, null, "ID1", "MRD", mrd_before.HasValue ? mrd_before.Value.ToString() : "", acct.MRD.HasValue ? acct.MRD.Value.ToString() : "", this.txtNotes_Account.Text.Trim());
                InsertFactorHistoryLog(this.accountID, null, "ID1", "MRD", curDisplayFactorValues != null ? curDisplayFactorValues.mrd : "", acct.MRD.HasValue ? acct.MRD.Value.ToString() : appMRD, this.txtNotes_Account.Text.Trim());

            if (acct.MRDIncrement != mrdIncrement_before)
                //InsertFactorHistoryLog(this.accountID, null, "ID1", "MRDIncrement", mrdIncrement_before.HasValue ? mrdIncrement_before.Value.ToString() : "", acct.MRDIncrement.HasValue ? acct.MRDIncrement.Value.ToString() : "", this.txtNotes_Account.Text.Trim());
                InsertFactorHistoryLog(this.accountID, null, "ID1", "MRDIncrement", curDisplayFactorValues != null ? curDisplayFactorValues.mrdIncrement : "", acct.MRDIncrement.HasValue ? acct.MRDIncrement.Value.ToString() : appMRDIncrement, this.txtNotes_Account.Text.Trim());

            if (acct.DeepFactor != deepFactor_before)
                //InsertFactorHistoryLog(this.accountID, null, "ID1", "DeepFactor", deepFactor_before.HasValue ? deepFactor_before.Value.ToString() : "", acct.DeepFactor.HasValue ? acct.DeepFactor.Value.ToString() : "", this.txtNotes_Account.Text.Trim());
                InsertFactorHistoryLog(this.accountID, null, "ID1", "DeepFactor", curDisplayFactorValues != null ? curDisplayFactorValues.deepFactor : "", acct.DeepFactor.HasValue ? acct.DeepFactor.Value.ToString() : dfDeepFactor.ToString(), this.txtNotes_Account.Text.Trim());

            if (acct.EyeFactor != eyeFactor_before)
                //InsertFactorHistoryLog(this.accountID, null, "ID1", "EyeFactor", eyeFactor_before.HasValue ? eyeFactor_before.Value.ToString() : "", acct.EyeFactor.HasValue ? acct.EyeFactor.Value.ToString() : "", this.txtNotes_Account.Text.Trim());
                InsertFactorHistoryLog(this.accountID, null, "ID1", "EyeFactor", curDisplayFactorValues != null ? curDisplayFactorValues.eyeFactor : "", acct.EyeFactor.HasValue ? acct.EyeFactor.Value.ToString() : dfEyeFactor.ToString(), this.txtNotes_Account.Text.Trim());

            if (acct.ShallowFactor != shallowFactor_before)
                //InsertFactorHistoryLog(this.accountID, null, "ID1", "ShallowFactor", shallowFactor_before.HasValue ? shallowFactor_before.Value.ToString() : "", acct.ShallowFactor.HasValue ? acct.ShallowFactor.Value.ToString() : "", this.txtNotes_Account.Text.Trim());
                InsertFactorHistoryLog(this.accountID, null, "ID1", "ShallowFactor", curDisplayFactorValues != null ? curDisplayFactorValues.shallowFactor : "", acct.ShallowFactor.HasValue ? acct.ShallowFactor.Value.ToString() : dfShallowFactor.ToString(), this.txtNotes_Account.Text.Trim());

            if (acct.BackgroundRate != backgroundRate_before)
                //InsertFactorHistoryLog(this.accountID, null, "ID1", "BackgroundRate", backgroundRate_before.HasValue ? backgroundRate_before.Value.ToString() : "", acct.BackgroundRate.HasValue ? acct.BackgroundRate.Value.ToString() : "", this.txtNotes_Account.Text.Trim());
                InsertFactorHistoryLog(this.accountID, null, "ID1", "BackgroundRate", curDisplayFactorValues != null ? curDisplayFactorValues.backgroundRate : "", acct.BackgroundRate.HasValue ? acct.BackgroundRate.Value.ToString() : state_country_BackgroundRate, this.txtNotes_Account.Text.Trim());

            // Insert any ID+ change
            if (acct.MRDPlus != mrdPlus_before)
                //InsertFactorHistoryLog(this.accountID, null, "ID+", "MRDPlus", mrdPlus_before.HasValue ? mrdPlus_before.Value.ToString() : "", acct.MRDPlus.HasValue ? acct.MRDPlus.Value.ToString() : "", this.txtNotes_Account.Text.Trim());
                InsertFactorHistoryLog(this.accountID, null, "ID+", "MRD", curDisplayFactorValues != null ? curDisplayFactorValues.mrdPlus : "", acct.MRDPlus.HasValue ? acct.MRDPlus.Value.ToString() : appMRDPlus, this.txtNotes_Account.Text.Trim());

            if (acct.MRDIncrementPlus != mrdIncrementPlus_before)
                //InsertFactorHistoryLog(this.accountID, null, "ID+", "MRDIncrementPlus", mrdIncrementPlus_before.HasValue ? mrdIncrementPlus_before.Value.ToString() : "", acct.MRDIncrementPlus.HasValue ? acct.MRDIncrementPlus.Value.ToString() : "", this.txtNotes_Account.Text.Trim());
                InsertFactorHistoryLog(this.accountID, null, "ID+", "MRDIncrement", curDisplayFactorValues != null ? curDisplayFactorValues.mrdIncrementPlus : "", acct.MRDIncrementPlus.HasValue ? acct.MRDIncrementPlus.Value.ToString() : appMRDIncrementPlus, this.txtNotes_Account.Text.Trim());

            if (acct.DeepFactorPlus != deepFactorPlus_before)
                //InsertFactorHistoryLog(this.accountID, null, "ID+", "DeepFactorPlus", deepFactorPlus_before.HasValue ? deepFactorPlus_before.Value.ToString() : "", acct.DeepFactorPlus.HasValue ? acct.DeepFactorPlus.Value.ToString() : "", this.txtNotes_Account.Text.Trim());
                InsertFactorHistoryLog(this.accountID, null, "ID+", "DeepFactor", curDisplayFactorValues != null ? curDisplayFactorValues.deepFactorPlus : "", acct.DeepFactorPlus.HasValue ? acct.DeepFactorPlus.Value.ToString() : dfDeepFactor.ToString(), this.txtNotes_Account.Text.Trim());

            if (acct.EyeFactorPlus != eyeFactorPlus_before)
                //InsertFactorHistoryLog(this.accountID, null, "ID+", "EyeFactorPlus", eyeFactorPlus_before.HasValue ? eyeFactorPlus_before.Value.ToString() : "", acct.EyeFactorPlus.HasValue ? acct.EyeFactorPlus.Value.ToString() : "", this.txtNotes_Account.Text.Trim());
                InsertFactorHistoryLog(this.accountID, null, "ID+", "EyeFactor", curDisplayFactorValues != null ? curDisplayFactorValues.eyeFactorPlus : "", acct.EyeFactorPlus.HasValue ? acct.EyeFactorPlus.Value.ToString() : dfEyeFactor.ToString(), this.txtNotes_Account.Text.Trim());

            if (acct.ShallowFactorPlus != shallowFactorPlus_before)
                //InsertFactorHistoryLog(this.accountID, null, "ID+", "ShallowFactorPlus", shallowFactorPlus_before.HasValue ? shallowFactorPlus_before.Value.ToString() : "", acct.ShallowFactorPlus.HasValue ? acct.ShallowFactorPlus.Value.ToString() : "", this.txtNotes_Account.Text.Trim());
                InsertFactorHistoryLog(this.accountID, null, "ID+", "ShallowFactor", curDisplayFactorValues != null ? curDisplayFactorValues.shallowFactorPlus : "", acct.ShallowFactorPlus.HasValue ? acct.ShallowFactorPlus.Value.ToString() : dfShallowFactor.ToString(), this.txtNotes_Account.Text.Trim());

            if (acct.BackgroundRatePlus != backgroundRatePlus_before)
                //InsertFactorHistoryLog(this.accountID, null, "ID+", "BackgroundRatePlus", backgroundRatePlus_before.HasValue ? backgroundRatePlus_before.Value.ToString() : "", acct.BackgroundRatePlus.HasValue ? acct.BackgroundRatePlus.Value.ToString() : "", this.txtNotes_Account.Text.Trim());     
                InsertFactorHistoryLog(this.accountID, null, "ID+", "BackgroundRate", curDisplayFactorValues != null ? curDisplayFactorValues.backgroundRatePlus : "", acct.BackgroundRatePlus.HasValue ? acct.BackgroundRatePlus.Value.ToString() : controlBadge_country_BackgroundRate, this.txtNotes_Account.Text.Trim());

            // Insert any ID2Elite change
            if (acct.MRD2 != mrd2_before)
                //InsertFactorHistoryLog(this.accountID, null, "IDElite", "MRD2", mrd2_before.HasValue ? mrd2_before.Value.ToString() : "", acct.MRD2.HasValue ? acct.MRD2.Value.ToString() : "", this.txtNotes_Account.Text.Trim());
                InsertFactorHistoryLog(this.accountID, null, "IDElite", "MRD", curDisplayFactorValues != null ? curDisplayFactorValues.mrd2 : "", acct.MRD2.HasValue ? acct.MRD2.Value.ToString() : appMRD2, this.txtNotes_Account.Text.Trim());

            if (acct.MRDIncrement2 != mrdIncrement2_before)
                //InsertFactorHistoryLog(this.accountID, null, "IDElite", "MRDIncrement2", mrdIncrement2_before.HasValue ? mrdIncrement2_before.Value.ToString() : "", acct.MRDIncrement2.HasValue ? acct.MRDIncrement2.Value.ToString() : "", this.txtNotes_Account.Text.Trim());
                InsertFactorHistoryLog(this.accountID, null, "IDElite", "MRDIncrement", curDisplayFactorValues != null ? curDisplayFactorValues.mrdIncrement2 : "", acct.MRDIncrement2.HasValue ? acct.MRDIncrement2.Value.ToString() : appMRDIncrement2, this.txtNotes_Account.Text.Trim());

            if (acct.BackgroundRate2 != backgroundRate2_before)
                //InsertFactorHistoryLog(this.accountID, null, "IDElite", "BackgroundRate2", backgroundRate2_before.HasValue ? backgroundRate2_before.Value.ToString() : "", acct.BackgroundRate2.HasValue ? acct.BackgroundRate2.Value.ToString() : "", this.txtNotes_Account.Text.Trim());
                InsertFactorHistoryLog(this.accountID, null, "IDElite", "BackgroundRate", curDisplayFactorValues != null ? curDisplayFactorValues.backgroundRate2 : "", acct.BackgroundRate2.HasValue ? acct.BackgroundRate2.Value.ToString() : controlBadge_country_BackgroundRate, this.txtNotes_Account.Text.Trim());

            if (acct.AlgorithmFactor != algorithmFactor_before)
                //InsertFactorHistoryLog(this.accountID, null, "IDElite", "AlgorithmPath", algorithmFactor_before, acct.AlgorithmFactor, this.txtNotes_Account.Text.Trim());
                InsertFactorHistoryLog(this.accountID, null, "IDElite", "AlgorithmPath", curDisplayFactorValues != null ? curDisplayFactorValues.algorithmFactor : "", !string.IsNullOrEmpty(acct.AlgorithmFactor) ? acct.AlgorithmFactor : appAlgorithmFactor, this.txtNotes_Account.Text.Trim());

            // Insert any ID2 New change
            if (acct.MRD2New != mrd2New_before)
                //InsertFactorHistoryLog(this.accountID, null, "ID2", "MRD2New", mrd2New_before.HasValue ? mrd2New_before.Value.ToString() : "", acct.MRD2New.HasValue ? acct.MRD2New.Value.ToString() : "", this.txtNotes_Account.Text.Trim());
                InsertFactorHistoryLog(this.accountID, null, "ID2", "MRD", curDisplayFactorValues != null ? curDisplayFactorValues.mrd2New : "", acct.MRD2New.HasValue ? acct.MRD2New.Value.ToString() : appMRD2New, this.txtNotes_Account.Text.Trim());

            if (acct.MRDIncrement2New != mrdIncrement2New_before)
                //InsertFactorHistoryLog(this.accountID, null, "ID2", "MRDIncrement2New", mrdIncrement2New_before.HasValue ? mrdIncrement2New_before.Value.ToString() : "", acct.MRDIncrement2New.HasValue ? acct.MRDIncrement2New.Value.ToString() : "", this.txtNotes_Account.Text.Trim());
                InsertFactorHistoryLog(this.accountID, null, "ID2", "MRDIncrement", curDisplayFactorValues != null ? curDisplayFactorValues.mrdIncrement2New : "", acct.MRDIncrement2New.HasValue ? acct.MRDIncrement2New.Value.ToString() : appMRDIncrement2New, this.txtNotes_Account.Text.Trim());

            if (acct.BackgroundRate2New != backgroundRate2New_before)
                //InsertFactorHistoryLog(this.accountID, null, "ID2", "BackgroundRate2New", backgroundRate2New_before.HasValue ? backgroundRate2New_before.Value.ToString() : "", acct.BackgroundRate2New.HasValue ? acct.BackgroundRate2New.Value.ToString() : "", this.txtNotes_Account.Text.Trim());
                InsertFactorHistoryLog(this.accountID, null, "ID2", "BackgroundRate", curDisplayFactorValues != null ? curDisplayFactorValues.backgroundRate2New : "", acct.BackgroundRate2New.HasValue ? acct.BackgroundRate2New.Value.ToString() : controlBadge_country_BackgroundRate, this.txtNotes_Account.Text.Trim());

            // Insert any IDVue New change
            if (acct.MRDVue != mrdVue_before)
                InsertFactorHistoryLog(this.accountID, null, "IDVue", "MRDVue", curDisplayFactorValues != null ? curDisplayFactorValues.mrdVue : "", acct.MRDVue.HasValue ? acct.MRDVue.Value.ToString() : appMRDVue, this.txtNotes_Account.Text.Trim());

            if (acct.MRDIncrementVue != mrdIncrementVue_before)
                //InsertFactorHistoryLog(this.accountID, null, "ID+", "MRDIncrementPlus", mrdIncrementPlus_before.HasValue ? mrdIncrementPlus_before.Value.ToString() : "", acct.MRDIncrementPlus.HasValue ? acct.MRDIncrementPlus.Value.ToString() : "", this.txtNotes_Account.Text.Trim());
                InsertFactorHistoryLog(this.accountID, null, "IDVue", "MRDIncrementVue", curDisplayFactorValues != null ? curDisplayFactorValues.mrdIncrementVue : "", acct.MRDIncrementVue.HasValue ? acct.MRDIncrementVue.Value.ToString() : appMRDIncrementVue, this.txtNotes_Account.Text.Trim());

            if (acct.EyeFactorVue != eyeFactorVue_before)
                //InsertFactorHistoryLog(this.accountID, null, "ID+", "EyeFactorPlus", eyeFactorPlus_before.HasValue ? eyeFactorPlus_before.Value.ToString() : "", acct.EyeFactorPlus.HasValue ? acct.EyeFactorPlus.Value.ToString() : "", this.txtNotes_Account.Text.Trim());
                InsertFactorHistoryLog(this.accountID, null, "IDVue", "EyeFactorVue", curDisplayFactorValues != null ? curDisplayFactorValues.eyeFactorVue : "", acct.EyeFactorVue.HasValue ? acct.EyeFactorVue.Value.ToString() : dfEyeFactorVue.ToString(), this.txtNotes_Account.Text.Trim());

            if (acct.ShallowFactorVue != shallowFactorVue_before)
                //InsertFactorHistoryLog(this.accountID, null, "ID+", "ShallowFactorPlus", shallowFactorPlus_before.HasValue ? shallowFactorPlus_before.Value.ToString() : "", acct.ShallowFactorPlus.HasValue ? acct.ShallowFactorPlus.Value.ToString() : "", this.txtNotes_Account.Text.Trim());
                InsertFactorHistoryLog(this.accountID, null, "IDVue", "ShallowFactorVue", curDisplayFactorValues != null ? curDisplayFactorValues.shallowFactorVue : "", acct.ShallowFactorVue.HasValue ? acct.ShallowFactorVue.Value.ToString() : dfShallowFactorVue.ToString(), this.txtNotes_Account.Text.Trim());

            if (acct.BackgroundRateVue != backgroundRateVue_before)
                InsertFactorHistoryLog(this.accountID, null, "IDVue", "BackgroundRateVue", curDisplayFactorValues != null ? curDisplayFactorValues.backgroundRateVue : "", acct.BackgroundRateVue.HasValue ? acct.BackgroundRateVue.Value.ToString() : appMRDIncrementVue, this.txtNotes_Account.Text.Trim());
            //InsertFactorHistoryLog(this.accountID, null, "IDVue", "BackgroundRateVue", curDisplayFactorValues != null ? curDisplayFactorValues.backgroundRateVue : "", acct.BackgroundRateVue.HasValue ? acct.BackgroundRateVue.Value.ToString() : idvue_country_BackgroundRate, this.txtNotes_Account.Text.Trim());

            if (acct.DeepFactorVue != deepFactorVue_before)
                InsertFactorHistoryLog(this.accountID, null, "IDVue", "DeepFactorVue", curDisplayFactorValues != null ? curDisplayFactorValues.deepfactorVue : "", acct.DeepFactorVue.HasValue ? acct.DeepFactorVue.Value.ToString() : dfDeepFactorVue.ToString(), this.txtNotes_Account.Text.Trim());

            // Insert Read Limit change
            if (acct.ReadLimit != readLimit_before)
                //InsertFactorHistoryLog(this.accountID, null, "All", "ReadLimit", readLimit_before.HasValue ? readLimit_before.Value.ToString() : "", acct.ReadLimit.HasValue ? acct.ReadLimit.Value.ToString() : "", this.txtNotes_Account.Text.Trim());
                InsertFactorHistoryLog(this.accountID, null, "All", "ReadLimit", curDisplayFactorValues != null ? curDisplayFactorValues.readLimit : "", acct.ReadLimit.HasValue ? acct.ReadLimit.Value.ToString() : appReadLimit, this.txtNotes_Account.Text.Trim());

            // ------------------------------------------------------------------------------------------------------------------------------------------------------------------------- //

            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divAccountSettingsDialog')", true);

            this.VisibleSuccesses("Settings for the account have been updated.");

            // Rebind GridView to display updated Inheritance values (where applicable).
            this.GetLocationLevelSettings(this.accountID);
        }
        catch (Exception ex)
        {
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divAccountSettingsDialog')", true);
            this.VisibleErrors("An error occurred while updating the account.</br>" + ex.Message);
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
    protected void gvLocationSettings_ID1_RowCommand(object sender, GridViewCommandEventArgs e)
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

        // Account Settings.
        var accountLevelInfo = (from acct in idc.Accounts
                                where acct.AccountID == this.accountID
                                select new
                                {
                                }).FirstOrDefault();

        if (accountLevelInfo == null) return;

        this.txtNotes_Location.Text = "";
        this.lblLocationName.Text = locationLevelInfo.LocationName;
        this.lblAccountID_Location.Text = this.account;

        // Use this variable to save the displaying factor values in the controls when first loading
        SettingFactors curDisplayFactorValues = new SettingFactors();

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
        string acctAlgorithmFactor = null;

        int? acctMRD2New = null;
        int? acctMRDIncrement2New = null;
        double? acctBackgroundRate2New = null;

        double? acctReadLimit = null;

        int? acctMRDVue = null;
        int? acctMRDIncrementVue = null;
        double? acctEyeFactorVue = null;
        double? acctShallowFactorVue = null;
        double? acctbackgroundRateVue = null;
        double? acctdeepFactorVue = null;

        // ------------------------------------AccountSettings ----------------------------------//
        this.GetAccountSettings(this.accountID, out acctCompanyName, out acctMRD, out acctMRDIncrement, out acctMRD2, out acctMRDIncrement2, out acctDeepFactor, out acctEyeFactor, out acctShallowFactor, out acctBackgroundRate, out acctBackgroundRate2, out acctReadLimit, out acctAlgorithmFactor, out acctMRDPlus, out acctMRDIncrementPlus, out acctDeepFactorPlus, out acctEyeFactorPlus, out acctShallowFactorPlus, out acctBackgroundRatePlus, out acctMRD2New, out acctMRDIncrement2New, out acctBackgroundRate2New, out acctMRDVue, out acctbackgroundRateVue, out acctdeepFactorVue, out acctMRDIncrementVue, out acctEyeFactorVue, out acctShallowFactorVue);
        // --------------------------------------------------------------------------------------//
        // ------------------------------------ AppSettings.-------------------------------------//
        string appMRD = Basics.GetSetting("MRD");
        string appMRDIncrement = Basics.GetSetting("MRDIncrement");

        string appMRDPlus = Basics.GetSetting("MRDPlus");
        string appMRDIncrementPlus = Basics.GetSetting("MRDIncrementPlus");

        string appMRD2 = Basics.GetSetting("MRDID2");
        string appMRDIncrement2 = Basics.GetSetting("MRDIncrementID2");
        string appAlgorithmFactor = Basics.GetSetting("AlgorithmFactorID2");

        string appMRD2New = Basics.GetSetting("MRDID2New");
        string appMRDIncrement2New = Basics.GetSetting("MRDIncrementID2New");

        string appReadLimit = Basics.GetSetting("ReadLimit");

        string appMRDVue = Basics.GetSetting("MRDIDVue");
        string appMRDIncrementVue = Basics.GetSetting("MRDIncrementIDVue");
        string appBackgroundRateVue = Basics.GetSetting("ID VUE Background");

        // DeviceFirmware.
        double dfDeepFactor = 0;
        double dfEyeFactor = 0;
        double dfShallowFactor = 0;
        this.GetDeviceFirmware(out dfDeepFactor, out dfEyeFactor, out dfShallowFactor);

        // Control Badge, State & Country.
        double? stateBackgroundRate = null;
        double? countryBackgroundRate = null;
        double? actControlBackgroundRate = null;
        double? locControlBackgroundRate = null;

        // MRD Vue
        double? dfbackgroundRateVue = null;

        double dfDeepFactorVue = 1;
        double dfEyeFactorVue = 1;
        double dfShallowFactorVue = 1;

        this.GetStateAndCountry(accountID, out stateBackgroundRate, out countryBackgroundRate);

        var act_cbrDLObj = (from cbr in idc.ControlBadgeReads
                            where
                            cbr.AccountID == this.accountID
                            && !cbr.LocationID.HasValue
                            && cbr.AvgDLRate.HasValue
                            && cbr.PermEliminatedDL == false
                            && cbr.IsEliminatedDL == false
                            && cbr.ReadTypeID == 17
                            orderby cbr.DoseDate descending
                            select cbr).FirstOrDefault();
        if (act_cbrDLObj != null) actControlBackgroundRate = act_cbrDLObj.AvgDLRate.Value;

        var loc_cbrDLObj = (from cbr in idc.ControlBadgeReads
                            where
                            cbr.AccountID == this.accountID
                            && cbr.LocationID == locationID
                            && cbr.AvgDLRate.HasValue
                            && cbr.PermEliminatedDL == false
                            && cbr.IsEliminatedDL == false
                            && cbr.ReadTypeID == 17
                            orderby cbr.DoseDate descending
                            select cbr).FirstOrDefault();
        if (loc_cbrDLObj != null) locControlBackgroundRate = loc_cbrDLObj.AvgDLRate.Value;

        string backgroundRate = acctBackgroundRate.HasValue ? acctBackgroundRate.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
        string backgroundRatePlus = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctBackgroundRatePlus.HasValue ? acctBackgroundRatePlus.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
        string backgroundRate2 = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctBackgroundRate2.HasValue ? acctBackgroundRate2.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
        string backgroundRate2New = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctBackgroundRate2New.HasValue ? acctBackgroundRate2New.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
        string backgroundRateVue = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctbackgroundRateVue.HasValue ? acctbackgroundRateVue.Value.ToString() : appBackgroundRateVue;
        //string backgroundRateVue = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctbackgroundRateVue.HasValue ? acctbackgroundRateVue.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
        // --------------------------------------------------------------------------------------//                       

        // ---------------------- MRD -----------------------------------------------------------//
        if (locationLevelInfo.MRD.HasValue)
        {
            this.txtMRD_Location.Text = (locationLevelInfo.MRD.Value).ToString();
            curDisplayFactorValues.mrd = (locationLevelInfo.MRD.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrd = acctMRD.HasValue ? acctMRD.Value.ToString() : appMRD.ToString();
        }

        // MRD Increment.
        if (locationLevelInfo.MRDIncrement.HasValue)
        {
            this.txtMRDIncrement_Location.Text = (locationLevelInfo.MRDIncrement.Value).ToString();
            curDisplayFactorValues.mrdIncrement = (locationLevelInfo.MRDIncrement.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrement = acctMRDIncrement.HasValue ? acctMRDIncrement.Value.ToString() : appMRDIncrement.ToString();
        }

        // Deep Factor.
        if (locationLevelInfo.DeepFactor.HasValue)
        {
            this.txtDeepFactor_Location.Text = (locationLevelInfo.DeepFactor.Value).ToString();
            curDisplayFactorValues.deepFactor = (locationLevelInfo.DeepFactor.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.deepFactor = acctDeepFactor.HasValue ? acctDeepFactor.Value.ToString() : dfDeepFactor.ToString();
        }

        // Eye Factor.
        if (locationLevelInfo.EyeFactor.HasValue)
        {
            this.txtEyeFactor_Location.Text = (locationLevelInfo.EyeFactor.Value).ToString();
            curDisplayFactorValues.eyeFactor = (locationLevelInfo.EyeFactor.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.eyeFactor = acctEyeFactor.HasValue ? acctEyeFactor.Value.ToString() : dfEyeFactor.ToString();
        }

        // Shallow Factor.
        if (locationLevelInfo.ShallowFactor.HasValue)
        {
            this.txtShallowFactor_Location.Text = (locationLevelInfo.ShallowFactor.Value).ToString();
            curDisplayFactorValues.shallowFactor = (locationLevelInfo.ShallowFactor.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.shallowFactor = acctShallowFactor.HasValue ? acctShallowFactor.Value.ToString() : dfShallowFactor.ToString();
        }

        // Background Rate.
        if (locationLevelInfo.BackgroundRate.HasValue)
        {
            this.txtBackgroundRate_Location.Text = (locationLevelInfo.BackgroundRate.Value).ToString();
            curDisplayFactorValues.backgroundRate = (locationLevelInfo.BackgroundRate.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRate = backgroundRate;
        }

        //------------------------------ MRD+ ------------------------------------------------------//
        if (locationLevelInfo.MRDPlus.HasValue)
        {
            this.txtMRDPlus_Location.Text = (locationLevelInfo.MRDPlus.Value).ToString();
            curDisplayFactorValues.mrdPlus = (locationLevelInfo.MRDPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdPlus = acctMRDPlus.HasValue ? acctMRDPlus.Value.ToString() : appMRDPlus.ToString();
        }

        // MRD Increment+.
        if (locationLevelInfo.MRDIncrementPlus.HasValue)
        {
            this.txtMRDIncrementPlus_Location.Text = (locationLevelInfo.MRDIncrementPlus.Value).ToString();
            curDisplayFactorValues.mrdIncrementPlus = (locationLevelInfo.MRDIncrementPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrementPlus = acctMRDIncrementPlus.HasValue ? acctMRDIncrementPlus.Value.ToString() : appMRDIncrementPlus.ToString();
        }

        // Deep Factor+.
        if (locationLevelInfo.DeepFactorPlus.HasValue)
        {
            this.txtDeepFactorPlus_Location.Text = (locationLevelInfo.DeepFactorPlus.Value).ToString();
            curDisplayFactorValues.deepFactorPlus = (locationLevelInfo.DeepFactorPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.deepFactorPlus = acctDeepFactorPlus.HasValue ? acctDeepFactorPlus.Value.ToString() : dfDeepFactor.ToString();
        }

        // Eye Factor+.
        if (locationLevelInfo.EyeFactorPlus.HasValue)
        {
            this.txtEyeFactorPlus_Location.Text = (locationLevelInfo.EyeFactorPlus.Value).ToString();
            curDisplayFactorValues.eyeFactorPlus = (locationLevelInfo.EyeFactorPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.eyeFactorPlus = acctEyeFactorPlus.HasValue ? acctEyeFactorPlus.Value.ToString() : dfEyeFactor.ToString();
        }

        // Shallow Factor+.
        if (locationLevelInfo.ShallowFactorPlus.HasValue)
        {
            this.txtShallowFactorPlus_Location.Text = (locationLevelInfo.ShallowFactorPlus.Value).ToString();
            curDisplayFactorValues.shallowFactorPlus = (locationLevelInfo.ShallowFactorPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.shallowFactorPlus = acctShallowFactorPlus.HasValue ? acctShallowFactorPlus.Value.ToString() : dfShallowFactor.ToString();
        }

        // Background Rate+.
        if (locationLevelInfo.BackgroundRatePlus.HasValue)
        {
            this.txtBackgroundRatePlus_Location.Text = (locationLevelInfo.BackgroundRatePlus.Value).ToString();
            curDisplayFactorValues.backgroundRatePlus = (locationLevelInfo.BackgroundRatePlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRatePlus = backgroundRatePlus;
        }

        //------------------------------- MRD 2 (ID2) -------------------------------------------------------//
        if (locationLevelInfo.MRD2.HasValue)
        {
            this.txtMRD2_Location.Text = (locationLevelInfo.MRD2.Value).ToString();
            curDisplayFactorValues.mrd2 = (locationLevelInfo.MRD2.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrd2 = acctMRD2.HasValue ? acctMRD2.Value.ToString() : appMRD2.ToString();
        }

        // MRD Increment 2 (ID2).
        if (locationLevelInfo.MRDIncrement2.HasValue)
        {
            this.txtMRDIncrement2_Location.Text = (locationLevelInfo.MRDIncrement2.Value).ToString();
            curDisplayFactorValues.mrdIncrement2 = (locationLevelInfo.MRDIncrement2.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrement2 = acctMRDIncrement2.HasValue ? acctMRDIncrement2.Value.ToString() : appMRDIncrement2.ToString();
        }

        // Background Rate 2.
        if (locationLevelInfo.BackgroundRate2.HasValue)
        {
            this.txtBackgroundRate2_Location.Text = (locationLevelInfo.BackgroundRate2.Value).ToString();
            curDisplayFactorValues.backgroundRate2 = (locationLevelInfo.BackgroundRate2.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRate2 = backgroundRate2;
        }

        // Algorithm Factor.
        if (locationLevelInfo.AlgorithmFactor != null)
        {
            this.txtAlgorithmFactor_Location.Text = locationLevelInfo.AlgorithmFactor;
            curDisplayFactorValues.algorithmFactor = locationLevelInfo.AlgorithmFactor;
        }
        else
        {
            curDisplayFactorValues.algorithmFactor = acctAlgorithmFactor ?? appAlgorithmFactor;
        }


        //-------------------------------- MRD 2 New (ID2 New) ---------------------------------------- //
        if (locationLevelInfo.MRD2New.HasValue)
        {
            this.txtMRD2New_Location.Text = (locationLevelInfo.MRD2New.Value).ToString();
            curDisplayFactorValues.mrd2New = (locationLevelInfo.MRD2New.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrd2New = acctMRD2New.HasValue ? acctMRD2New.Value.ToString() : appMRD2New.ToString();
        }

        // MRD Increment 2New (ID2 New).
        if (locationLevelInfo.MRDIncrement2New.HasValue)
        {
            this.txtMRDIncrement2New_Location.Text = (locationLevelInfo.MRDIncrement2New.Value).ToString();
            curDisplayFactorValues.mrdIncrement2New = (locationLevelInfo.MRDIncrement2New.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrement2New = acctMRDIncrement2New.HasValue ? acctMRDIncrement2New.Value.ToString() : appMRDIncrement2New.ToString();
        }

        // Background Rate 2 New.
        if (locationLevelInfo.BackgroundRate2New.HasValue)
        {
            this.txtBackgroundRate2New_Location.Text = (locationLevelInfo.BackgroundRate2New.Value).ToString();
            curDisplayFactorValues.backgroundRate2New = (locationLevelInfo.BackgroundRate2New.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRate2New = backgroundRate2New;
        }

        //------------------------------ IDVue ------------------------------------------------------//
        if (locationLevelInfo.MRDVue.HasValue)
        {
            this.txtMRD_Location.Text = (locationLevelInfo.MRDVue.Value).ToString();
            curDisplayFactorValues.mrdVue = (locationLevelInfo.MRDVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdVue = acctMRDVue.HasValue ? acctMRDVue.Value.ToString() : appMRDVue.ToString();
        }

        // MRD Increment Vue.
        if (locationLevelInfo.MRDIncrementVue.HasValue)
        {
            this.txtMRDIncrementVue_Location.Text = (locationLevelInfo.MRDIncrementVue.Value).ToString();
            curDisplayFactorValues.mrdIncrementVue = (locationLevelInfo.MRDIncrementVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrementVue = acctMRDIncrementVue.HasValue ? acctMRDIncrementVue.Value.ToString() : appMRDIncrementVue.ToString();
        }

        // Eye Factor Vue.
        if (locationLevelInfo.EyeFactorVue.HasValue)
        {
            this.txtEyeFactorVue_Location.Text = (locationLevelInfo.EyeFactorVue.Value).ToString();
            curDisplayFactorValues.eyeFactorVue = (locationLevelInfo.EyeFactorVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.eyeFactorVue = acctEyeFactorVue.HasValue ? acctEyeFactorVue.Value.ToString() : dfEyeFactorVue.ToString();
        }

        // Shallow Factor Vue.
        if (locationLevelInfo.ShallowFactorVue.HasValue)
        {
            this.txtShallowFactorVue_Location.Text = (locationLevelInfo.ShallowFactorVue.Value).ToString();
            curDisplayFactorValues.shallowFactorVue = (locationLevelInfo.ShallowFactorVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.shallowFactorVue = acctShallowFactorVue.HasValue ? acctShallowFactorVue.Value.ToString() : dfShallowFactorVue.ToString();
        }

        // Background Rate Vue.
        if (locationLevelInfo.BackgroundRateVue.HasValue)
        {
            this.txtBackgroundRateVue_Location.Text = (locationLevelInfo.BackgroundRateVue.Value).ToString();
            curDisplayFactorValues.backgroundRateVue = (locationLevelInfo.BackgroundRateVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRateVue = backgroundRateVue;
        }

        // Deep Factor Vue
        if (locationLevelInfo.DeepFactorVue.HasValue)
        {
            this.txtDeepFactorVue_Location.Text = (locationLevelInfo.DeepFactorVue.Value).ToString();
            curDisplayFactorValues.deepfactorVue = (locationLevelInfo.DeepFactorVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.deepfactorVue = acctdeepFactorVue.HasValue ? acctdeepFactorVue.Value.ToString() : dfDeepFactorVue.ToString();
        }

        //------------------------- Read Limit ---------------------------------------------------//
        if (locationLevelInfo.ReadLimit.HasValue)
        {
            this.txtReadLimit_Location.Text = (locationLevelInfo.ReadLimit.Value).ToString();
            curDisplayFactorValues.readLimit = (locationLevelInfo.ReadLimit.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.readLimit = acctReadLimit.HasValue ? acctReadLimit.Value.ToString() : appReadLimit.ToString();
        }
        // --------------------------------------------------------------------------------------------------------------------------------------------------------//




        // Use this session to save all displaying factors values before commit change. Use this session to pass in previous factor value to FactorsHistoryLog table
        Session["CurrentDisplayFactorValues"] = null;
        Session["CurrentDisplayFactorValues"] = curDisplayFactorValues;

        // Placeholder will always be present, in case User decideds not to enter an Location-Level value.
        this.txtMRD_Location.Attributes.Add("placeholder", acctMRD.HasValue ? acctMRD.Value.ToString() : appMRD.ToString());
        this.txtMRDIncrement_Location.Attributes.Add("placeholder", acctMRDIncrement.HasValue ? acctMRDIncrement.Value.ToString() : appMRDIncrement.ToString());
        this.txtDeepFactor_Location.Attributes.Add("placeholder", acctDeepFactor.HasValue ? acctDeepFactor.Value.ToString() : dfDeepFactor.ToString());
        this.txtEyeFactor_Location.Attributes.Add("placeholder", acctEyeFactor.HasValue ? acctEyeFactor.Value.ToString() : dfEyeFactor.ToString());
        this.txtShallowFactor_Location.Attributes.Add("placeholder", acctShallowFactor.HasValue ? acctShallowFactor.Value.ToString() : dfShallowFactor.ToString());
        this.txtBackgroundRate_Location.Attributes.Add("placeholder", backgroundRate);

        this.txtMRDPlus_Location.Attributes.Add("placeholder", acctMRDPlus.HasValue ? acctMRDPlus.Value.ToString() : appMRDPlus.ToString());
        this.txtMRDIncrementPlus_Location.Attributes.Add("placeholder", acctMRDIncrementPlus.HasValue ? acctMRDIncrementPlus.Value.ToString() : appMRDIncrementPlus.ToString());
        this.txtDeepFactorPlus_Location.Attributes.Add("placeholder", acctDeepFactorPlus.HasValue ? acctDeepFactorPlus.Value.ToString() : dfDeepFactor.ToString());
        this.txtEyeFactorPlus_Location.Attributes.Add("placeholder", acctEyeFactorPlus.HasValue ? acctEyeFactorPlus.Value.ToString() : dfEyeFactor.ToString());
        this.txtShallowFactorPlus_Location.Attributes.Add("placeholder", acctShallowFactorPlus.HasValue ? acctShallowFactorPlus.Value.ToString() : dfShallowFactor.ToString());
        this.txtBackgroundRatePlus_Location.Attributes.Add("placeholder", backgroundRatePlus);

        this.txtMRD2_Location.Attributes.Add("placeholder", acctMRD2.HasValue ? acctMRD2.Value.ToString() : appMRD2.ToString());
        this.txtMRDIncrement2_Location.Attributes.Add("placeholder", acctMRDIncrement2.HasValue ? acctMRDIncrement2.Value.ToString() : appMRDIncrement2.ToString());
        this.txtBackgroundRate2_Location.Attributes.Add("placeholder", backgroundRate2);
        this.txtAlgorithmFactor_Location.Attributes.Add("placeholder", acctAlgorithmFactor ?? appAlgorithmFactor);

        this.txtMRD2New_Location.Attributes.Add("placeholder", acctMRD2New.HasValue ? acctMRD2New.Value.ToString() : appMRD2New.ToString());
        this.txtMRDIncrement2New_Location.Attributes.Add("placeholder", acctMRDIncrement2New.HasValue ? acctMRDIncrement2New.Value.ToString() : appMRDIncrement2New.ToString());
        this.txtBackgroundRate2New_Location.Attributes.Add("placeholder", backgroundRate2New);

        this.txtReadLimit_Location.Attributes.Add("placeholder", acctReadLimit.HasValue ? acctReadLimit.Value.ToString() : appReadLimit.ToString());


        this.txtMRDVue_Location.Attributes.Add("placeholder", appMRDVue);
        this.txtMRDIncrementVue_Location.Attributes.Add("placeholder", acctMRDIncrementVue.HasValue ? acctMRDIncrementVue.Value.ToString() : appMRDIncrementVue.ToString());
        this.txtEyeFactorVue_Location.Attributes.Add("placeholder", acctEyeFactorVue.HasValue ? acctEyeFactorVue.Value.ToString() : dfEyeFactorVue.ToString());
        this.txtShallowFactorVue_Location.Attributes.Add("placeholder", acctShallowFactorVue.HasValue ? acctShallowFactorVue.Value.ToString() : dfShallowFactorVue.ToString());
        this.txtBackgroundRateVue_Location.Attributes.Add("placeholder", acctbackgroundRateVue.HasValue ? acctbackgroundRateVue.Value.ToString() : backgroundRateVue);
        this.txtDeepFactorVue_Location.Attributes.Add("placeholder", acctdeepFactorVue.HasValue ? acctdeepFactorVue.Value.ToString() : dfDeepFactorVue.ToString()); ;


        // Send Javascript to client to OPEN/DISPLAY the modal dialog.
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('divLocationSettingsDialog')", true);
    }

    protected void gvLocationSettings_ID2_RowCommand(object sender, GridViewCommandEventArgs e)
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

        // Account Settings.
        var accountLevelInfo = (from acct in idc.Accounts
                                where acct.AccountID == this.accountID
                                select new
                                {
                                }).FirstOrDefault();

        if (accountLevelInfo == null) return;

        this.txtNotes_Location.Text = "";
        this.lblLocationName.Text = locationLevelInfo.LocationName;
        this.lblAccountID_Location.Text = this.account;

        // Use this variable to save the displaying factor values in the controls when first loading
        SettingFactors curDisplayFactorValues = new SettingFactors();

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
        string acctAlgorithmFactor = null;

        int? acctMRD2New = null;
        int? acctMRDIncrement2New = null;
        double? acctBackgroundRate2New = null;

        double? acctReadLimit = null;

        int? acctMRDVue = null;
        int? acctMRDIncrementVue = null;
        double? acctEyeFactorVue = null;
        double? acctShallowFactorVue = null;
        double? acctbackgroundRateVue = null;
        double? acctdeepfactorVue = null;

        // ------------------------------------AccountSettings ----------------------------------//
        this.GetAccountSettings(this.accountID, out acctCompanyName, out acctMRD, out acctMRDIncrement, out acctMRD2, out acctMRDIncrement2, out acctDeepFactor, out acctEyeFactor, out acctShallowFactor, out acctBackgroundRate, out acctBackgroundRate2, out acctReadLimit, out acctAlgorithmFactor, out acctMRDPlus, out acctMRDIncrementPlus, out acctDeepFactorPlus, out acctEyeFactorPlus, out acctShallowFactorPlus, out acctBackgroundRatePlus, out acctMRD2New, out acctMRDIncrement2New, out acctBackgroundRate2New, out acctMRDVue, out acctbackgroundRateVue, out acctdeepfactorVue, out acctMRDIncrementVue, out acctEyeFactorVue, out acctShallowFactorVue);
        // --------------------------------------------------------------------------------------//
        // ------------------------------------ AppSettings.-------------------------------------//
        string appMRD = Basics.GetSetting("MRD");
        string appMRDIncrement = Basics.GetSetting("MRDIncrement");

        string appMRDPlus = Basics.GetSetting("MRDPlus");
        string appMRDIncrementPlus = Basics.GetSetting("MRDIncrementPlus");

        string appMRD2 = Basics.GetSetting("MRDID2");
        string appMRDIncrement2 = Basics.GetSetting("MRDIncrementID2");
        string appAlgorithmFactor = Basics.GetSetting("AlgorithmFactorID2");

        string appMRD2New = Basics.GetSetting("MRDID2New");
        string appMRDIncrement2New = Basics.GetSetting("MRDIncrementID2New");

        string appReadLimit = Basics.GetSetting("ReadLimit");

        string appMRDVue = Basics.GetSetting("MRDIDVue");
        string appMRDIncrementVue = Basics.GetSetting("MRDIncrementIDVue");
        string appBackgroundRateVue = Basics.GetSetting("ID VUE Background");

        // DeviceFirmware.
        double dfDeepFactor = 0;
        double dfEyeFactor = 0;
        double dfShallowFactor = 0;
        this.GetDeviceFirmware(out dfDeepFactor, out dfEyeFactor, out dfShallowFactor);

        // Control Badge, State & Country.
        double? stateBackgroundRate = null;
        double? countryBackgroundRate = null;
        double? actControlBackgroundRate = null;
        double? locControlBackgroundRate = null;

        double? dfbackgroundRateVue = null;

        double dfDeepFactorVue = 1;
        double dfEyeFactorVue = 1;
        double dfShallowFactorVue = 1;

        this.GetStateAndCountry(accountID, out stateBackgroundRate, out countryBackgroundRate);

        var act_cbrDLObj = (from cbr in idc.ControlBadgeReads
                            where
                            cbr.AccountID == this.accountID
                            && !cbr.LocationID.HasValue
                            && cbr.AvgDLRate.HasValue
                            && cbr.PermEliminatedDL == false
                            && cbr.IsEliminatedDL == false
                            && cbr.ReadTypeID == 17
                            orderby cbr.DoseDate descending
                            select cbr).FirstOrDefault();
        if (act_cbrDLObj != null) actControlBackgroundRate = act_cbrDLObj.AvgDLRate.Value;

        var loc_cbrDLObj = (from cbr in idc.ControlBadgeReads
                            where
                            cbr.AccountID == this.accountID
                            && cbr.LocationID == locationID
                            && cbr.AvgDLRate.HasValue
                            && cbr.PermEliminatedDL == false
                            && cbr.IsEliminatedDL == false
                            && cbr.ReadTypeID == 17
                            orderby cbr.DoseDate descending
                            select cbr).FirstOrDefault();
        if (loc_cbrDLObj != null) locControlBackgroundRate = loc_cbrDLObj.AvgDLRate.Value;

        string backgroundRate = acctBackgroundRate.HasValue ? acctBackgroundRate.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
        string backgroundRatePlus = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctBackgroundRatePlus.HasValue ? acctBackgroundRatePlus.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
        string backgroundRate2 = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctBackgroundRate2.HasValue ? acctBackgroundRate2.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
        string backgroundRate2New = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctBackgroundRate2New.HasValue ? acctBackgroundRate2New.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
        string backgroundRateVue = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctbackgroundRateVue.HasValue ? acctbackgroundRateVue.Value.ToString() : appBackgroundRateVue;
        //string backgroundRateVue = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctbackgroundRateVue.HasValue ? acctbackgroundRateVue.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
        // --------------------------------------------------------------------------------------//                       

        // ---------------------- MRD -----------------------------------------------------------//
        if (locationLevelInfo.MRD.HasValue)
        {
            this.txtMRD_Location.Text = (locationLevelInfo.MRD.Value).ToString();
            curDisplayFactorValues.mrd = (locationLevelInfo.MRD.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrd = acctMRD.HasValue ? acctMRD.Value.ToString() : appMRD.ToString();
        }

        // MRD Increment.
        if (locationLevelInfo.MRDIncrement.HasValue)
        {
            this.txtMRDIncrement_Location.Text = (locationLevelInfo.MRDIncrement.Value).ToString();
            curDisplayFactorValues.mrdIncrement = (locationLevelInfo.MRDIncrement.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrement = acctMRDIncrement.HasValue ? acctMRDIncrement.Value.ToString() : appMRDIncrement.ToString();
        }

        // Deep Factor.
        if (locationLevelInfo.DeepFactor.HasValue)
        {
            this.txtDeepFactor_Location.Text = (locationLevelInfo.DeepFactor.Value).ToString();
            curDisplayFactorValues.deepFactor = (locationLevelInfo.DeepFactor.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.deepFactor = acctDeepFactor.HasValue ? acctDeepFactor.Value.ToString() : dfDeepFactor.ToString();
        }

        // Eye Factor.
        if (locationLevelInfo.EyeFactor.HasValue)
        {
            this.txtEyeFactor_Location.Text = (locationLevelInfo.EyeFactor.Value).ToString();
            curDisplayFactorValues.eyeFactor = (locationLevelInfo.EyeFactor.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.eyeFactor = acctEyeFactor.HasValue ? acctEyeFactor.Value.ToString() : dfEyeFactor.ToString();
        }

        // Shallow Factor.
        if (locationLevelInfo.ShallowFactor.HasValue)
        {
            this.txtShallowFactor_Location.Text = (locationLevelInfo.ShallowFactor.Value).ToString();
            curDisplayFactorValues.shallowFactor = (locationLevelInfo.ShallowFactor.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.shallowFactor = acctShallowFactor.HasValue ? acctShallowFactor.Value.ToString() : dfShallowFactor.ToString();
        }

        // Background Rate.
        if (locationLevelInfo.BackgroundRate.HasValue)
        {
            this.txtBackgroundRate_Location.Text = (locationLevelInfo.BackgroundRate.Value).ToString();
            curDisplayFactorValues.backgroundRate = (locationLevelInfo.BackgroundRate.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRate = backgroundRate;
        }

        //------------------------------ MRD+ ------------------------------------------------------//
        if (locationLevelInfo.MRDPlus.HasValue)
        {
            this.txtMRDPlus_Location.Text = (locationLevelInfo.MRDPlus.Value).ToString();
            curDisplayFactorValues.mrdPlus = (locationLevelInfo.MRDPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdPlus = acctMRDPlus.HasValue ? acctMRDPlus.Value.ToString() : appMRDPlus.ToString();
        }

        // MRD Increment+.
        if (locationLevelInfo.MRDIncrementPlus.HasValue)
        {
            this.txtMRDIncrementPlus_Location.Text = (locationLevelInfo.MRDIncrementPlus.Value).ToString();
            curDisplayFactorValues.mrdIncrementPlus = (locationLevelInfo.MRDIncrementPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrementPlus = acctMRDIncrementPlus.HasValue ? acctMRDIncrementPlus.Value.ToString() : appMRDIncrementPlus.ToString();
        }

        // Deep Factor+.
        if (locationLevelInfo.DeepFactorPlus.HasValue)
        {
            this.txtDeepFactorPlus_Location.Text = (locationLevelInfo.DeepFactorPlus.Value).ToString();
            curDisplayFactorValues.deepFactorPlus = (locationLevelInfo.DeepFactorPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.deepFactorPlus = acctDeepFactorPlus.HasValue ? acctDeepFactorPlus.Value.ToString() : dfDeepFactor.ToString();
        }

        // Eye Factor+.
        if (locationLevelInfo.EyeFactorPlus.HasValue)
        {
            this.txtEyeFactorPlus_Location.Text = (locationLevelInfo.EyeFactorPlus.Value).ToString();
            curDisplayFactorValues.eyeFactorPlus = (locationLevelInfo.EyeFactorPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.eyeFactorPlus = acctEyeFactorPlus.HasValue ? acctEyeFactorPlus.Value.ToString() : dfEyeFactor.ToString();
        }

        // Shallow Factor+.
        if (locationLevelInfo.ShallowFactorPlus.HasValue)
        {
            this.txtShallowFactorPlus_Location.Text = (locationLevelInfo.ShallowFactorPlus.Value).ToString();
            curDisplayFactorValues.shallowFactorPlus = (locationLevelInfo.ShallowFactorPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.shallowFactorPlus = acctShallowFactorPlus.HasValue ? acctShallowFactorPlus.Value.ToString() : dfShallowFactor.ToString();
        }

        // Background Rate+.
        if (locationLevelInfo.BackgroundRatePlus.HasValue)
        {
            this.txtBackgroundRatePlus_Location.Text = (locationLevelInfo.BackgroundRatePlus.Value).ToString();
            curDisplayFactorValues.backgroundRatePlus = (locationLevelInfo.BackgroundRatePlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRatePlus = backgroundRatePlus;
        }

        //------------------------------- MRD 2 (ID2) -------------------------------------------------------//
        if (locationLevelInfo.MRD2.HasValue)
        {
            this.txtMRD2_Location.Text = (locationLevelInfo.MRD2.Value).ToString();
            curDisplayFactorValues.mrd2 = (locationLevelInfo.MRD2.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrd2 = acctMRD2.HasValue ? acctMRD2.Value.ToString() : appMRD2.ToString();
        }

        // MRD Increment 2 (ID2).
        if (locationLevelInfo.MRDIncrement2.HasValue)
        {
            this.txtMRDIncrement2_Location.Text = (locationLevelInfo.MRDIncrement2.Value).ToString();
            curDisplayFactorValues.mrdIncrement2 = (locationLevelInfo.MRDIncrement2.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrement2 = acctMRDIncrement2.HasValue ? acctMRDIncrement2.Value.ToString() : appMRDIncrement2.ToString();
        }

        // Background Rate 2.
        if (locationLevelInfo.BackgroundRate2.HasValue)
        {
            this.txtBackgroundRate2_Location.Text = (locationLevelInfo.BackgroundRate2.Value).ToString();
            curDisplayFactorValues.backgroundRate2 = (locationLevelInfo.BackgroundRate2.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRate2 = backgroundRate2;
        }

        // Algorithm Factor.
        if (locationLevelInfo.AlgorithmFactor != null)
        {
            this.txtAlgorithmFactor_Location.Text = locationLevelInfo.AlgorithmFactor;
            curDisplayFactorValues.algorithmFactor = locationLevelInfo.AlgorithmFactor;
        }
        else
        {
            curDisplayFactorValues.algorithmFactor = acctAlgorithmFactor ?? appAlgorithmFactor;
        }


        //-------------------------------- MRD 2 New (ID2 New) ---------------------------------------- //
        if (locationLevelInfo.MRD2New.HasValue)
        {
            this.txtMRD2New_Location.Text = (locationLevelInfo.MRD2New.Value).ToString();
            curDisplayFactorValues.mrd2New = (locationLevelInfo.MRD2New.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrd2New = acctMRD2New.HasValue ? acctMRD2New.Value.ToString() : appMRD2New.ToString();
        }

        // MRD Increment 2New (ID2 New).
        if (locationLevelInfo.MRDIncrement2New.HasValue)
        {
            this.txtMRDIncrement2New_Location.Text = (locationLevelInfo.MRDIncrement2New.Value).ToString();
            curDisplayFactorValues.mrdIncrement2New = (locationLevelInfo.MRDIncrement2New.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrement2New = acctMRDIncrement2New.HasValue ? acctMRDIncrement2New.Value.ToString() : appMRDIncrement2New.ToString();
        }

        // Background Rate 2 New.
        if (locationLevelInfo.BackgroundRate2New.HasValue)
        {
            this.txtBackgroundRate2New_Location.Text = (locationLevelInfo.BackgroundRate2New.Value).ToString();
            curDisplayFactorValues.backgroundRate2New = (locationLevelInfo.BackgroundRate2New.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRate2New = backgroundRate2New;
        }

        //------------------------- Read Limit ---------------------------------------------------//
        if (locationLevelInfo.ReadLimit.HasValue)
        {
            this.txtReadLimit_Location.Text = (locationLevelInfo.ReadLimit.Value).ToString();
            curDisplayFactorValues.readLimit = (locationLevelInfo.ReadLimit.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.readLimit = acctReadLimit.HasValue ? acctReadLimit.Value.ToString() : appReadLimit.ToString();
        }
        // --------------------------------------------------------------------------------------------------------------------------------------------------------//

        // ---------------------- MRD Vue -----------------------------------------------------------//
        if (locationLevelInfo.MRDVue.HasValue)
        {
            this.txtMRDVue_Location.Text = (locationLevelInfo.MRDVue.Value).ToString();
            curDisplayFactorValues.mrdVue = (locationLevelInfo.MRDVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdVue = acctMRDVue.HasValue ? acctMRDVue.Value.ToString() : appMRDVue.ToString();
        }

        // MRD Increment Vue.
        if (locationLevelInfo.MRDIncrementVue.HasValue)
        {
            this.txtMRDIncrementVue_Location.Text = (locationLevelInfo.MRDIncrementVue.Value).ToString();
            curDisplayFactorValues.mrdIncrementVue = (locationLevelInfo.MRDIncrementVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrementVue = acctMRDIncrementVue.HasValue ? acctMRDIncrementVue.Value.ToString() : appMRDIncrementVue.ToString();
        }

        // Eye Factor Vue.
        if (locationLevelInfo.EyeFactorVue.HasValue)
        {
            this.txtEyeFactorVue_Location.Text = (locationLevelInfo.EyeFactorVue.Value).ToString();
            curDisplayFactorValues.eyeFactorVue = (locationLevelInfo.EyeFactorVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.eyeFactorVue = acctEyeFactorVue.HasValue ? acctEyeFactorVue.Value.ToString() : dfEyeFactorVue.ToString();
        }

        // Shallow Factor Vue.
        if (locationLevelInfo.ShallowFactorVue.HasValue)
        {
            this.txtShallowFactorVue_Location.Text = (locationLevelInfo.ShallowFactorVue.Value).ToString();
            curDisplayFactorValues.shallowFactorVue = (locationLevelInfo.ShallowFactorVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.shallowFactorVue = acctShallowFactorVue.HasValue ? acctShallowFactorVue.Value.ToString() : dfShallowFactorVue.ToString();
        }

        // Background Rate Vue.
        if (locationLevelInfo.BackgroundRateVue.HasValue)
        {
            this.txtBackgroundRateVue_Location.Text = (locationLevelInfo.BackgroundRateVue.Value).ToString();
            curDisplayFactorValues.backgroundRateVue = (locationLevelInfo.BackgroundRateVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRateVue = backgroundRateVue;
        }

        // Deep Factor Vue
        if (locationLevelInfo.DeepFactorVue.HasValue)
        {
            this.txtDeepFactorVue_Location.Text = (locationLevelInfo.DeepFactorVue.Value).ToString();
            curDisplayFactorValues.deepfactorVue = (locationLevelInfo.DeepFactorVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.deepfactorVue = acctdeepfactorVue.HasValue ? acctdeepfactorVue.Value.ToString() : dfDeepFactorVue.ToString();
        }
        // Use this session to save all displaying factors values before commit change. Use this session to pass in previous factor value to FactorsHistoryLog table
        Session["CurrentDisplayFactorValues"] = null;
        Session["CurrentDisplayFactorValues"] = curDisplayFactorValues;

        // Placeholder will always be present, in case User decideds not to enter an Location-Level value.
        this.txtMRD_Location.Attributes.Add("placeholder", acctMRD.HasValue ? acctMRD.Value.ToString() : appMRD.ToString());
        this.txtMRDIncrement_Location.Attributes.Add("placeholder", acctMRDIncrement.HasValue ? acctMRDIncrement.Value.ToString() : appMRDIncrement.ToString());
        this.txtDeepFactor_Location.Attributes.Add("placeholder", acctDeepFactor.HasValue ? acctDeepFactor.Value.ToString() : dfDeepFactor.ToString());
        this.txtEyeFactor_Location.Attributes.Add("placeholder", acctEyeFactor.HasValue ? acctEyeFactor.Value.ToString() : dfEyeFactor.ToString());
        this.txtShallowFactor_Location.Attributes.Add("placeholder", acctShallowFactor.HasValue ? acctShallowFactor.Value.ToString() : dfShallowFactor.ToString());
        this.txtBackgroundRate_Location.Attributes.Add("placeholder", backgroundRate);

        this.txtMRDPlus_Location.Attributes.Add("placeholder", acctMRDPlus.HasValue ? acctMRDPlus.Value.ToString() : appMRDPlus.ToString());
        this.txtMRDIncrementPlus_Location.Attributes.Add("placeholder", acctMRDIncrementPlus.HasValue ? acctMRDIncrementPlus.Value.ToString() : appMRDIncrementPlus.ToString());
        this.txtDeepFactorPlus_Location.Attributes.Add("placeholder", acctDeepFactorPlus.HasValue ? acctDeepFactorPlus.Value.ToString() : dfDeepFactor.ToString());
        this.txtEyeFactorPlus_Location.Attributes.Add("placeholder", acctEyeFactorPlus.HasValue ? acctEyeFactorPlus.Value.ToString() : dfEyeFactor.ToString());
        this.txtShallowFactorPlus_Location.Attributes.Add("placeholder", acctShallowFactorPlus.HasValue ? acctShallowFactorPlus.Value.ToString() : dfShallowFactor.ToString());
        this.txtBackgroundRatePlus_Location.Attributes.Add("placeholder", backgroundRatePlus);

        this.txtMRD2_Location.Attributes.Add("placeholder", acctMRD2.HasValue ? acctMRD2.Value.ToString() : appMRD2.ToString());
        this.txtMRDIncrement2_Location.Attributes.Add("placeholder", acctMRDIncrement2.HasValue ? acctMRDIncrement2.Value.ToString() : appMRDIncrement2.ToString());
        this.txtBackgroundRate2_Location.Attributes.Add("placeholder", backgroundRate2);
        this.txtAlgorithmFactor_Location.Attributes.Add("placeholder", acctAlgorithmFactor ?? appAlgorithmFactor);

        this.txtMRD2New_Location.Attributes.Add("placeholder", acctMRD2New.HasValue ? acctMRD2New.Value.ToString() : appMRD2New.ToString());
        this.txtMRDIncrement2New_Location.Attributes.Add("placeholder", acctMRDIncrement2New.HasValue ? acctMRDIncrement2New.Value.ToString() : appMRDIncrement2New.ToString());
        this.txtBackgroundRate2New_Location.Attributes.Add("placeholder", backgroundRate2New);

        this.txtReadLimit_Location.Attributes.Add("placeholder", acctReadLimit.HasValue ? acctReadLimit.Value.ToString() : appReadLimit.ToString());

        this.txtMRDVue_Location.Attributes.Add("placeholder", appMRDVue);
        this.txtMRDIncrementVue_Location.Attributes.Add("placeholder", acctMRDIncrementVue.HasValue ? acctMRDIncrementVue.Value.ToString() : appMRDIncrementVue.ToString());
        this.txtEyeFactorVue_Location.Attributes.Add("placeholder", acctEyeFactorVue.HasValue ? acctEyeFactorVue.Value.ToString() : dfEyeFactorVue.ToString());
        this.txtShallowFactorVue_Location.Attributes.Add("placeholder", acctShallowFactorVue.HasValue ? acctShallowFactorVue.Value.ToString() : dfShallowFactorVue.ToString());
        this.txtBackgroundRateVue_Location.Attributes.Add("placeholder", acctbackgroundRateVue.HasValue ? acctbackgroundRateVue.Value.ToString() : backgroundRateVue); ;
        this.txtDeepFactorVue_Location.Attributes.Add("placeholder", acctdeepfactorVue.HasValue ? acctdeepfactorVue.Value.ToString() : dfDeepFactorVue.ToString());

        // Send Javascript to client to OPEN/DISPLAY the modal dialog.
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('divLocationSettingsDialog')", true);
    }

    protected void gvLocationSettings_ID2Elite_RowCommand(object sender, GridViewCommandEventArgs e)
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

        // Account Settings.
        var accountLevelInfo = (from acct in idc.Accounts
                                where acct.AccountID == this.accountID
                                select new
                                {
                                }).FirstOrDefault();

        if (accountLevelInfo == null) return;

        this.txtNotes_Location.Text = "";
        this.lblLocationName.Text = locationLevelInfo.LocationName;
        this.lblAccountID_Location.Text = this.account;

        // Use this variable to save the displaying factor values in the controls when first loading
        SettingFactors curDisplayFactorValues = new SettingFactors();

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
        string acctAlgorithmFactor = null;

        int? acctMRD2New = null;
        int? acctMRDIncrement2New = null;
        double? acctBackgroundRate2New = null;

        double? acctReadLimit = null;

        int? acctMRDVue = null;
        int? acctMRDIncrementVue = null;
        double? acctEyeFactorVue = null;
        double? acctShallowFactorVue = null;
        double? acctBackgroundRateVue = null;
        double? acctDeepFactorVue = null;


        // ------------------------------------AccountSettings ----------------------------------//
        this.GetAccountSettings(this.accountID, out acctCompanyName, out acctMRD, out acctMRDIncrement, out acctMRD2, out acctMRDIncrement2, out acctDeepFactor, out acctEyeFactor, out acctShallowFactor, out acctBackgroundRate, out acctBackgroundRate2, out acctReadLimit, out acctAlgorithmFactor, out acctMRDPlus, out acctMRDIncrementPlus, out acctDeepFactorPlus, out acctEyeFactorPlus, out acctShallowFactorPlus, out acctBackgroundRatePlus, out acctMRD2New, out acctMRDIncrement2New, out acctBackgroundRate2New, out acctMRDVue, out acctBackgroundRateVue, out acctDeepFactorVue, out acctMRDIncrementVue, out acctEyeFactorVue, out acctShallowFactorVue);
        // --------------------------------------------------------------------------------------//
        // ------------------------------------ AppSettings.-------------------------------------//
        string appMRD = Basics.GetSetting("MRD");
        string appMRDIncrement = Basics.GetSetting("MRDIncrement");

        string appMRDPlus = Basics.GetSetting("MRDPlus");
        string appMRDIncrementPlus = Basics.GetSetting("MRDIncrementPlus");

        string appMRD2 = Basics.GetSetting("MRDID2");
        string appMRDIncrement2 = Basics.GetSetting("MRDIncrementID2");
        string appAlgorithmFactor = Basics.GetSetting("AlgorithmFactorID2");

        string appMRD2New = Basics.GetSetting("MRDID2New");
        string appMRDIncrement2New = Basics.GetSetting("MRDIncrementID2New");

        string appReadLimit = Basics.GetSetting("ReadLimit");

        string appMRDVue = Basics.GetSetting("MRDIDVue");
        string appMRDIncrementVue = Basics.GetSetting("MRDIncrementIDVue");
        string appBackgroundRateVue = Basics.GetSetting("ID VUE Background");

        // DeviceFirmware.
        double dfDeepFactor = 0;
        double dfEyeFactor = 0;
        double dfShallowFactor = 0;
        this.GetDeviceFirmware(out dfDeepFactor, out dfEyeFactor, out dfShallowFactor);

        // Control Badge, State & Country.
        double? stateBackgroundRate = null;
        double? countryBackgroundRate = null;
        double? actControlBackgroundRate = null;
        double? locControlBackgroundRate = null;

        double? dfbackgroundRateVue = null;

        double dfDeepFactorVue = 1;
        double dfEyeFactorVue = 1;
        double dfShallowFactorVue = 1;

        this.GetStateAndCountry(accountID, out stateBackgroundRate, out countryBackgroundRate);

        var act_cbrDLObj = (from cbr in idc.ControlBadgeReads
                            where
                            cbr.AccountID == this.accountID
                            && !cbr.LocationID.HasValue
                            && cbr.AvgDLRate.HasValue
                            && cbr.PermEliminatedDL == false
                            && cbr.IsEliminatedDL == false
                            && cbr.ReadTypeID == 17
                            orderby cbr.DoseDate descending
                            select cbr).FirstOrDefault();
        if (act_cbrDLObj != null) actControlBackgroundRate = act_cbrDLObj.AvgDLRate.Value;

        var loc_cbrDLObj = (from cbr in idc.ControlBadgeReads
                            where
                            cbr.AccountID == this.accountID
                            && cbr.LocationID == locationID
                            && cbr.AvgDLRate.HasValue
                            && cbr.PermEliminatedDL == false
                            && cbr.IsEliminatedDL == false
                            && cbr.ReadTypeID == 17
                            orderby cbr.DoseDate descending
                            select cbr).FirstOrDefault();
        if (loc_cbrDLObj != null) locControlBackgroundRate = loc_cbrDLObj.AvgDLRate.Value;

        string backgroundRate = acctBackgroundRate.HasValue ? acctBackgroundRate.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
        string backgroundRatePlus = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctBackgroundRatePlus.HasValue ? acctBackgroundRatePlus.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
        string backgroundRate2 = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctBackgroundRate2.HasValue ? acctBackgroundRate2.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
        string backgroundRate2New = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctBackgroundRate2New.HasValue ? acctBackgroundRate2New.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
        string backgroundRateVue = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctBackgroundRateVue.HasValue ? acctBackgroundRateVue.Value.ToString() : appBackgroundRateVue;
        //string backgroundRateVue = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctBackgroundRateVue.HasValue ? acctBackgroundRateVue.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
        // --------------------------------------------------------------------------------------//                       

        // ---------------------- MRD -----------------------------------------------------------//
        if (locationLevelInfo.MRD.HasValue)
        {
            this.txtMRD_Location.Text = (locationLevelInfo.MRD.Value).ToString();
            curDisplayFactorValues.mrd = (locationLevelInfo.MRD.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrd = acctMRD.HasValue ? acctMRD.Value.ToString() : appMRD.ToString();
        }

        // MRD Increment.
        if (locationLevelInfo.MRDIncrement.HasValue)
        {
            this.txtMRDIncrement_Location.Text = (locationLevelInfo.MRDIncrement.Value).ToString();
            curDisplayFactorValues.mrdIncrement = (locationLevelInfo.MRDIncrement.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrement = acctMRDIncrement.HasValue ? acctMRDIncrement.Value.ToString() : appMRDIncrement.ToString();
        }

        // Deep Factor.
        if (locationLevelInfo.DeepFactor.HasValue)
        {
            this.txtDeepFactor_Location.Text = (locationLevelInfo.DeepFactor.Value).ToString();
            curDisplayFactorValues.deepFactor = (locationLevelInfo.DeepFactor.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.deepFactor = acctDeepFactor.HasValue ? acctDeepFactor.Value.ToString() : dfDeepFactor.ToString();
        }

        // Eye Factor.
        if (locationLevelInfo.EyeFactor.HasValue)
        {
            this.txtEyeFactor_Location.Text = (locationLevelInfo.EyeFactor.Value).ToString();
            curDisplayFactorValues.eyeFactor = (locationLevelInfo.EyeFactor.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.eyeFactor = acctEyeFactor.HasValue ? acctEyeFactor.Value.ToString() : dfEyeFactor.ToString();
        }

        // Shallow Factor.
        if (locationLevelInfo.ShallowFactor.HasValue)
        {
            this.txtShallowFactor_Location.Text = (locationLevelInfo.ShallowFactor.Value).ToString();
            curDisplayFactorValues.shallowFactor = (locationLevelInfo.ShallowFactor.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.shallowFactor = acctShallowFactor.HasValue ? acctShallowFactor.Value.ToString() : dfShallowFactor.ToString();
        }

        // Background Rate.
        if (locationLevelInfo.BackgroundRate.HasValue)
        {
            this.txtBackgroundRate_Location.Text = (locationLevelInfo.BackgroundRate.Value).ToString();
            curDisplayFactorValues.backgroundRate = (locationLevelInfo.BackgroundRate.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRate = backgroundRate;
        }

        //------------------------------ MRD+ ------------------------------------------------------//
        if (locationLevelInfo.MRDPlus.HasValue)
        {
            this.txtMRDPlus_Location.Text = (locationLevelInfo.MRDPlus.Value).ToString();
            curDisplayFactorValues.mrdPlus = (locationLevelInfo.MRDPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdPlus = acctMRDPlus.HasValue ? acctMRDPlus.Value.ToString() : appMRDPlus.ToString();
        }

        // MRD Increment+.
        if (locationLevelInfo.MRDIncrementPlus.HasValue)
        {
            this.txtMRDIncrementPlus_Location.Text = (locationLevelInfo.MRDIncrementPlus.Value).ToString();
            curDisplayFactorValues.mrdIncrementPlus = (locationLevelInfo.MRDIncrementPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrementPlus = acctMRDIncrementPlus.HasValue ? acctMRDIncrementPlus.Value.ToString() : appMRDIncrementPlus.ToString();
        }

        // Deep Factor+.
        if (locationLevelInfo.DeepFactorPlus.HasValue)
        {
            this.txtDeepFactorPlus_Location.Text = (locationLevelInfo.DeepFactorPlus.Value).ToString();
            curDisplayFactorValues.deepFactorPlus = (locationLevelInfo.DeepFactorPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.deepFactorPlus = acctDeepFactorPlus.HasValue ? acctDeepFactorPlus.Value.ToString() : dfDeepFactor.ToString();
        }

        // Eye Factor+.
        if (locationLevelInfo.EyeFactorPlus.HasValue)
        {
            this.txtEyeFactorPlus_Location.Text = (locationLevelInfo.EyeFactorPlus.Value).ToString();
            curDisplayFactorValues.eyeFactorPlus = (locationLevelInfo.EyeFactorPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.eyeFactorPlus = acctEyeFactorPlus.HasValue ? acctEyeFactorPlus.Value.ToString() : dfEyeFactor.ToString();
        }

        // Shallow Factor+.
        if (locationLevelInfo.ShallowFactorPlus.HasValue)
        {
            this.txtShallowFactorPlus_Location.Text = (locationLevelInfo.ShallowFactorPlus.Value).ToString();
            curDisplayFactorValues.shallowFactorPlus = (locationLevelInfo.ShallowFactorPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.shallowFactorPlus = acctShallowFactorPlus.HasValue ? acctShallowFactorPlus.Value.ToString() : dfShallowFactor.ToString();
        }

        // Background Rate+.
        if (locationLevelInfo.BackgroundRatePlus.HasValue)
        {
            this.txtBackgroundRatePlus_Location.Text = (locationLevelInfo.BackgroundRatePlus.Value).ToString();
            curDisplayFactorValues.backgroundRatePlus = (locationLevelInfo.BackgroundRatePlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRatePlus = backgroundRatePlus;
        }

        //------------------------------- MRD 2 (ID2) -------------------------------------------------------//
        if (locationLevelInfo.MRD2.HasValue)
        {
            this.txtMRD2_Location.Text = (locationLevelInfo.MRD2.Value).ToString();
            curDisplayFactorValues.mrd2 = (locationLevelInfo.MRD2.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrd2 = acctMRD2.HasValue ? acctMRD2.Value.ToString() : appMRD2.ToString();
        }

        // MRD Increment 2 (ID2).
        if (locationLevelInfo.MRDIncrement2.HasValue)
        {
            this.txtMRDIncrement2_Location.Text = (locationLevelInfo.MRDIncrement2.Value).ToString();
            curDisplayFactorValues.mrdIncrement2 = (locationLevelInfo.MRDIncrement2.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrement2 = acctMRDIncrement2.HasValue ? acctMRDIncrement2.Value.ToString() : appMRDIncrement2.ToString();
        }

        // Background Rate 2.
        if (locationLevelInfo.BackgroundRate2.HasValue)
        {
            this.txtBackgroundRate2_Location.Text = (locationLevelInfo.BackgroundRate2.Value).ToString();
            curDisplayFactorValues.backgroundRate2 = (locationLevelInfo.BackgroundRate2.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRate2 = backgroundRate2;
        }

        // Algorithm Factor.
        if (locationLevelInfo.AlgorithmFactor != null)
        {
            this.txtAlgorithmFactor_Location.Text = locationLevelInfo.AlgorithmFactor;
            curDisplayFactorValues.algorithmFactor = locationLevelInfo.AlgorithmFactor;
        }
        else
        {
            curDisplayFactorValues.algorithmFactor = acctAlgorithmFactor ?? appAlgorithmFactor;
        }


        //-------------------------------- MRD 2 New (ID2 New) ---------------------------------------- //
        if (locationLevelInfo.MRD2New.HasValue)
        {
            this.txtMRD2New_Location.Text = (locationLevelInfo.MRD2New.Value).ToString();
            curDisplayFactorValues.mrd2New = (locationLevelInfo.MRD2New.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrd2New = acctMRD2New.HasValue ? acctMRD2New.Value.ToString() : appMRD2New.ToString();
        }

        // MRD Increment 2New (ID2 New).
        if (locationLevelInfo.MRDIncrement2New.HasValue)
        {
            this.txtMRDIncrement2New_Location.Text = (locationLevelInfo.MRDIncrement2New.Value).ToString();
            curDisplayFactorValues.mrdIncrement2New = (locationLevelInfo.MRDIncrement2New.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrement2New = acctMRDIncrement2New.HasValue ? acctMRDIncrement2New.Value.ToString() : appMRDIncrement2New.ToString();
        }

        // Background Rate 2 New.
        if (locationLevelInfo.BackgroundRate2New.HasValue)
        {
            this.txtBackgroundRate2New_Location.Text = (locationLevelInfo.BackgroundRate2New.Value).ToString();
            curDisplayFactorValues.backgroundRate2New = (locationLevelInfo.BackgroundRate2New.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRate2New = backgroundRate2New;
        }

        //------------------------- Read Limit ---------------------------------------------------//
        if (locationLevelInfo.ReadLimit.HasValue)
        {
            this.txtReadLimit_Location.Text = (locationLevelInfo.ReadLimit.Value).ToString();
            curDisplayFactorValues.readLimit = (locationLevelInfo.ReadLimit.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.readLimit = acctReadLimit.HasValue ? acctReadLimit.Value.ToString() : appReadLimit.ToString();
        }

        // ---------------------- MRD Vue -----------------------------------------------------------//
        if (locationLevelInfo.MRDVue.HasValue)
        {
            this.txtMRDVue_Location.Text = (locationLevelInfo.MRDVue.Value).ToString();
            curDisplayFactorValues.mrdVue = (locationLevelInfo.MRDVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdVue = acctMRDVue.HasValue ? acctMRDVue.Value.ToString() : appMRDVue.ToString();
        }

        // MRD Increment Vue.
        if (locationLevelInfo.MRDIncrementVue.HasValue)
        {
            this.txtMRDIncrementVue_Location.Text = (locationLevelInfo.MRDIncrementVue.Value).ToString();
            curDisplayFactorValues.mrdIncrementVue = (locationLevelInfo.MRDIncrementVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrementVue = acctMRDIncrementVue.HasValue ? acctMRDIncrementVue.Value.ToString() : appMRDIncrementVue.ToString();
        }

        // Eye Factor Vue.
        if (locationLevelInfo.EyeFactorVue.HasValue)
        {
            this.txtEyeFactorVue_Location.Text = (locationLevelInfo.EyeFactorVue.Value).ToString();
            curDisplayFactorValues.eyeFactorVue = (locationLevelInfo.EyeFactorVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.eyeFactorVue = acctEyeFactorVue.HasValue ? acctEyeFactorVue.Value.ToString() : dfEyeFactorVue.ToString();
        }

        // Shallow Factor Vue.
        if (locationLevelInfo.ShallowFactorVue.HasValue)
        {
            this.txtShallowFactorVue_Location.Text = (locationLevelInfo.ShallowFactorVue.Value).ToString();
            curDisplayFactorValues.shallowFactorVue = (locationLevelInfo.ShallowFactorVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.shallowFactorVue = acctShallowFactorVue.HasValue ? acctShallowFactorVue.Value.ToString() : dfShallowFactorVue.ToString();
        }

        // Background Rate Vue.
        if (locationLevelInfo.BackgroundRateVue.HasValue)
        {
            this.txtBackgroundRateVue_Location.Text = (locationLevelInfo.BackgroundRateVue.Value).ToString();
            curDisplayFactorValues.backgroundRateVue = (locationLevelInfo.BackgroundRateVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRateVue = backgroundRateVue;
        }

        // Deep Factor Vue
        if (locationLevelInfo.DeepFactorVue.HasValue)
        {
            this.txtDeepFactorVue_Location.Text = (locationLevelInfo.DeepFactorVue.Value).ToString();
            curDisplayFactorValues.deepfactorVue = (locationLevelInfo.DeepFactorVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.deepfactorVue = acctDeepFactorVue.HasValue ? acctDeepFactorVue.Value.ToString() : dfDeepFactorVue.ToString();
        }

        // --------------------------------------------------------------------------------------------------------------------------------------------------------//

        // Use this session to save all displaying factors values before commit change. Use this session to pass in previous factor value to FactorsHistoryLog table
        Session["CurrentDisplayFactorValues"] = null;
        Session["CurrentDisplayFactorValues"] = curDisplayFactorValues;

        // Placeholder will always be present, in case User decideds not to enter an Location-Level value.
        this.txtMRD_Location.Attributes.Add("placeholder", acctMRD.HasValue ? acctMRD.Value.ToString() : appMRD.ToString());
        this.txtMRDIncrement_Location.Attributes.Add("placeholder", acctMRDIncrement.HasValue ? acctMRDIncrement.Value.ToString() : appMRDIncrement.ToString());
        this.txtDeepFactor_Location.Attributes.Add("placeholder", acctDeepFactor.HasValue ? acctDeepFactor.Value.ToString() : dfDeepFactor.ToString());
        this.txtEyeFactor_Location.Attributes.Add("placeholder", acctEyeFactor.HasValue ? acctEyeFactor.Value.ToString() : dfEyeFactor.ToString());
        this.txtShallowFactor_Location.Attributes.Add("placeholder", acctShallowFactor.HasValue ? acctShallowFactor.Value.ToString() : dfShallowFactor.ToString());
        this.txtBackgroundRate_Location.Attributes.Add("placeholder", backgroundRate);

        this.txtMRDPlus_Location.Attributes.Add("placeholder", acctMRDPlus.HasValue ? acctMRDPlus.Value.ToString() : appMRDPlus.ToString());
        this.txtMRDIncrementPlus_Location.Attributes.Add("placeholder", acctMRDIncrementPlus.HasValue ? acctMRDIncrementPlus.Value.ToString() : appMRDIncrementPlus.ToString());
        this.txtDeepFactorPlus_Location.Attributes.Add("placeholder", acctDeepFactorPlus.HasValue ? acctDeepFactorPlus.Value.ToString() : dfDeepFactor.ToString());
        this.txtEyeFactorPlus_Location.Attributes.Add("placeholder", acctEyeFactorPlus.HasValue ? acctEyeFactorPlus.Value.ToString() : dfEyeFactor.ToString());
        this.txtShallowFactorPlus_Location.Attributes.Add("placeholder", acctShallowFactorPlus.HasValue ? acctShallowFactorPlus.Value.ToString() : dfShallowFactor.ToString());
        this.txtBackgroundRatePlus_Location.Attributes.Add("placeholder", backgroundRatePlus);

        this.txtMRD2_Location.Attributes.Add("placeholder", acctMRD2.HasValue ? acctMRD2.Value.ToString() : appMRD2.ToString());
        this.txtMRDIncrement2_Location.Attributes.Add("placeholder", acctMRDIncrement2.HasValue ? acctMRDIncrement2.Value.ToString() : appMRDIncrement2.ToString());
        this.txtBackgroundRate2_Location.Attributes.Add("placeholder", backgroundRate2);
        this.txtAlgorithmFactor_Location.Attributes.Add("placeholder", acctAlgorithmFactor ?? appAlgorithmFactor);

        this.txtMRD2New_Location.Attributes.Add("placeholder", acctMRD2New.HasValue ? acctMRD2New.Value.ToString() : appMRD2New.ToString());
        this.txtMRDIncrement2New_Location.Attributes.Add("placeholder", acctMRDIncrement2New.HasValue ? acctMRDIncrement2New.Value.ToString() : appMRDIncrement2New.ToString());
        this.txtBackgroundRate2New_Location.Attributes.Add("placeholder", backgroundRate2New);

        this.txtReadLimit_Location.Attributes.Add("placeholder", acctReadLimit.HasValue ? acctReadLimit.Value.ToString() : appReadLimit.ToString());

        this.txtMRDVue_Location.Attributes.Add("placeholder", acctMRDVue.HasValue ? acctMRDVue.Value.ToString() : appMRDVue.ToString());
        this.txtMRDIncrementVue_Location.Attributes.Add("placeholder", acctMRDIncrementVue.HasValue ? acctMRDIncrementVue.Value.ToString() : appMRDIncrementVue.ToString());
        this.txtEyeFactorVue_Location.Attributes.Add("placeholder", acctEyeFactorVue.HasValue ? acctEyeFactorVue.Value.ToString() : dfEyeFactorVue.ToString());
        this.txtShallowFactorVue_Location.Attributes.Add("placeholder", acctShallowFactorVue.HasValue ? acctShallowFactorVue.Value.ToString() : dfShallowFactorVue.ToString());
        this.txtBackgroundRateVue_Location.Attributes.Add("placeholder", acctBackgroundRateVue.HasValue ? acctBackgroundRateVue.Value.ToString() : backgroundRateVue);
        this.txtDeepFactorVue_Location.Attributes.Add("placeholder", acctDeepFactorVue.HasValue ? acctDeepFactorVue.Value.ToString() : dfDeepFactorVue.ToString());

        // Send Javascript to client to OPEN/DISPLAY the modal dialog.
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('divLocationSettingsDialog')", true);
    }

    protected void gvLocationSettings_IDPlus_RowCommand(object sender, GridViewCommandEventArgs e)
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

        // Account Settings.
        var accountLevelInfo = (from acct in idc.Accounts
                                where acct.AccountID == this.accountID
                                select new
                                {
                                }).FirstOrDefault();

        if (accountLevelInfo == null) return;

        this.txtNotes_Location.Text = "";
        this.lblLocationName.Text = locationLevelInfo.LocationName;
        this.lblAccountID_Location.Text = this.account;

        // Use this variable to save the displaying factor values in the controls when first loading
        SettingFactors curDisplayFactorValues = new SettingFactors();

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
        string acctAlgorithmFactor = null;

        int? acctMRD2New = null;
        int? acctMRDIncrement2New = null;
        double? acctBackgroundRate2New = null;

        double? acctReadLimit = null;

        int? acctMRDVue = null;
        int? acctMRDIncrementVue = null;
        double? acctEyeFactorVue = null;
        double? acctShallowFactorVue = null;
        double? acctBackgroundRateVue = null;
        double? acctDeepFactorVue = null;

        // ------------------------------------AccountSettings ----------------------------------//
        this.GetAccountSettings(this.accountID, out acctCompanyName, out acctMRD, out acctMRDIncrement, out acctMRD2, out acctMRDIncrement2, out acctDeepFactor, out acctEyeFactor, out acctShallowFactor, out acctBackgroundRate, out acctBackgroundRate2, out acctReadLimit, out acctAlgorithmFactor, out acctMRDPlus, out acctMRDIncrementPlus, out acctDeepFactorPlus, out acctEyeFactorPlus, out acctShallowFactorPlus, out acctBackgroundRatePlus, out acctMRD2New, out acctMRDIncrement2New, out acctBackgroundRate2New, out acctMRDVue, out acctBackgroundRateVue, out acctDeepFactorVue, out acctMRDIncrementVue, out acctEyeFactorVue, out acctShallowFactorVue);
        // --------------------------------------------------------------------------------------//
        // ------------------------------------ AppSettings.-------------------------------------//
        string appMRD = Basics.GetSetting("MRD");
        string appMRDIncrement = Basics.GetSetting("MRDIncrement");

        string appMRDPlus = Basics.GetSetting("MRDPlus");
        string appMRDIncrementPlus = Basics.GetSetting("MRDIncrementPlus");

        string appMRD2 = Basics.GetSetting("MRDID2");
        string appMRDIncrement2 = Basics.GetSetting("MRDIncrementID2");
        string appAlgorithmFactor = Basics.GetSetting("AlgorithmFactorID2");

        string appMRD2New = Basics.GetSetting("MRDID2New");
        string appMRDIncrement2New = Basics.GetSetting("MRDIncrementID2New");

        string appReadLimit = Basics.GetSetting("ReadLimit");

        string appMRDVue = Basics.GetSetting("MRDIDVue");
        string appMRDIncrementVue = Basics.GetSetting("MRDIncrementIDVue");
        string appBackgroundRateVue = Basics.GetSetting("ID VUE Background");

        // DeviceFirmware.
        double dfDeepFactor = 0;
        double dfEyeFactor = 0;
        double dfShallowFactor = 0;
        this.GetDeviceFirmware(out dfDeepFactor, out dfEyeFactor, out dfShallowFactor);

        // Control Badge, State & Country.
        double? stateBackgroundRate = null;
        double? countryBackgroundRate = null;
        double? actControlBackgroundRate = null;
        double? locControlBackgroundRate = null;

        double? dfbackgroundRateVue = null;

        double dfDeepFactorVue = 1;
        double dfEyeFactorVue = 1;
        double dfShallowFactorVue = 1;

        this.GetStateAndCountry(accountID, out stateBackgroundRate, out countryBackgroundRate);

        var act_cbrDLObj = (from cbr in idc.ControlBadgeReads
                            where
                            cbr.AccountID == this.accountID
                            && !cbr.LocationID.HasValue
                            && cbr.AvgDLRate.HasValue
                            && cbr.PermEliminatedDL == false
                            && cbr.IsEliminatedDL == false
                            && cbr.ReadTypeID == 17
                            orderby cbr.DoseDate descending
                            select cbr).FirstOrDefault();
        if (act_cbrDLObj != null) actControlBackgroundRate = act_cbrDLObj.AvgDLRate.Value;

        var loc_cbrDLObj = (from cbr in idc.ControlBadgeReads
                            where
                            cbr.AccountID == this.accountID
                            && cbr.LocationID == locationID
                            && cbr.AvgDLRate.HasValue
                            && cbr.PermEliminatedDL == false
                            && cbr.IsEliminatedDL == false
                            && cbr.ReadTypeID == 17
                            orderby cbr.DoseDate descending
                            select cbr).FirstOrDefault();
        if (loc_cbrDLObj != null) locControlBackgroundRate = loc_cbrDLObj.AvgDLRate.Value;

        string backgroundRate = acctBackgroundRate.HasValue ? acctBackgroundRate.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
        string backgroundRatePlus = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctBackgroundRatePlus.HasValue ? acctBackgroundRatePlus.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
        string backgroundRate2 = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctBackgroundRate2.HasValue ? acctBackgroundRate2.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
        string backgroundRate2New = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctBackgroundRate2New.HasValue ? acctBackgroundRate2New.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
        string backgroundRateVue = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctBackgroundRateVue.HasValue ? acctBackgroundRateVue.Value.ToString() : appBackgroundRateVue;
        //string backgroundRateVue = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctBackgroundRateVue.HasValue ? acctBackgroundRateVue.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
        // --------------------------------------------------------------------------------------//                       

        // ---------------------- MRD -----------------------------------------------------------//
        if (locationLevelInfo.MRD.HasValue)
        {
            this.txtMRD_Location.Text = (locationLevelInfo.MRD.Value).ToString();
            curDisplayFactorValues.mrd = (locationLevelInfo.MRD.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrd = acctMRD.HasValue ? acctMRD.Value.ToString() : appMRD.ToString();
        }

        // MRD Increment.
        if (locationLevelInfo.MRDIncrement.HasValue)
        {
            this.txtMRDIncrement_Location.Text = (locationLevelInfo.MRDIncrement.Value).ToString();
            curDisplayFactorValues.mrdIncrement = (locationLevelInfo.MRDIncrement.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrement = acctMRDIncrement.HasValue ? acctMRDIncrement.Value.ToString() : appMRDIncrement.ToString();
        }

        // Deep Factor.
        if (locationLevelInfo.DeepFactor.HasValue)
        {
            this.txtDeepFactor_Location.Text = (locationLevelInfo.DeepFactor.Value).ToString();
            curDisplayFactorValues.deepFactor = (locationLevelInfo.DeepFactor.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.deepFactor = acctDeepFactor.HasValue ? acctDeepFactor.Value.ToString() : dfDeepFactor.ToString();
        }

        // Eye Factor.
        if (locationLevelInfo.EyeFactor.HasValue)
        {
            this.txtEyeFactor_Location.Text = (locationLevelInfo.EyeFactor.Value).ToString();
            curDisplayFactorValues.eyeFactor = (locationLevelInfo.EyeFactor.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.eyeFactor = acctEyeFactor.HasValue ? acctEyeFactor.Value.ToString() : dfEyeFactor.ToString();
        }

        // Shallow Factor.
        if (locationLevelInfo.ShallowFactor.HasValue)
        {
            this.txtShallowFactor_Location.Text = (locationLevelInfo.ShallowFactor.Value).ToString();
            curDisplayFactorValues.shallowFactor = (locationLevelInfo.ShallowFactor.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.shallowFactor = acctShallowFactor.HasValue ? acctShallowFactor.Value.ToString() : dfShallowFactor.ToString();
        }

        // Background Rate.
        if (locationLevelInfo.BackgroundRate.HasValue)
        {
            this.txtBackgroundRate_Location.Text = (locationLevelInfo.BackgroundRate.Value).ToString();
            curDisplayFactorValues.backgroundRate = (locationLevelInfo.BackgroundRate.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRate = backgroundRate;
        }

        //------------------------------ MRD+ ------------------------------------------------------//
        if (locationLevelInfo.MRDPlus.HasValue)
        {
            this.txtMRDPlus_Location.Text = (locationLevelInfo.MRDPlus.Value).ToString();
            curDisplayFactorValues.mrdPlus = (locationLevelInfo.MRDPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdPlus = acctMRDPlus.HasValue ? acctMRDPlus.Value.ToString() : appMRDPlus.ToString();
        }

        // MRD Increment+.
        if (locationLevelInfo.MRDIncrementPlus.HasValue)
        {
            this.txtMRDIncrementPlus_Location.Text = (locationLevelInfo.MRDIncrementPlus.Value).ToString();
            curDisplayFactorValues.mrdIncrementPlus = (locationLevelInfo.MRDIncrementPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrementPlus = acctMRDIncrementPlus.HasValue ? acctMRDIncrementPlus.Value.ToString() : appMRDIncrementPlus.ToString();
        }

        // Deep Factor+.
        if (locationLevelInfo.DeepFactorPlus.HasValue)
        {
            this.txtDeepFactorPlus_Location.Text = (locationLevelInfo.DeepFactorPlus.Value).ToString();
            curDisplayFactorValues.deepFactorPlus = (locationLevelInfo.DeepFactorPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.deepFactorPlus = acctDeepFactorPlus.HasValue ? acctDeepFactorPlus.Value.ToString() : dfDeepFactor.ToString();
        }

        // Eye Factor+.
        if (locationLevelInfo.EyeFactorPlus.HasValue)
        {
            this.txtEyeFactorPlus_Location.Text = (locationLevelInfo.EyeFactorPlus.Value).ToString();
            curDisplayFactorValues.eyeFactorPlus = (locationLevelInfo.EyeFactorPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.eyeFactorPlus = acctEyeFactorPlus.HasValue ? acctEyeFactorPlus.Value.ToString() : dfEyeFactor.ToString();
        }

        // Shallow Factor+.
        if (locationLevelInfo.ShallowFactorPlus.HasValue)
        {
            this.txtShallowFactorPlus_Location.Text = (locationLevelInfo.ShallowFactorPlus.Value).ToString();
            curDisplayFactorValues.shallowFactorPlus = (locationLevelInfo.ShallowFactorPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.shallowFactorPlus = acctShallowFactorPlus.HasValue ? acctShallowFactorPlus.Value.ToString() : dfShallowFactor.ToString();
        }

        // Background Rate+.
        if (locationLevelInfo.BackgroundRatePlus.HasValue)
        {
            this.txtBackgroundRatePlus_Location.Text = (locationLevelInfo.BackgroundRatePlus.Value).ToString();
            curDisplayFactorValues.backgroundRatePlus = (locationLevelInfo.BackgroundRatePlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRatePlus = backgroundRatePlus;
        }

        //------------------------------- MRD 2 (ID2) -------------------------------------------------------//
        if (locationLevelInfo.MRD2.HasValue)
        {
            this.txtMRD2_Location.Text = (locationLevelInfo.MRD2.Value).ToString();
            curDisplayFactorValues.mrd2 = (locationLevelInfo.MRD2.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrd2 = acctMRD2.HasValue ? acctMRD2.Value.ToString() : appMRD2.ToString();
        }

        // MRD Increment 2 (ID2).
        if (locationLevelInfo.MRDIncrement2.HasValue)
        {
            this.txtMRDIncrement2_Location.Text = (locationLevelInfo.MRDIncrement2.Value).ToString();
            curDisplayFactorValues.mrdIncrement2 = (locationLevelInfo.MRDIncrement2.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrement2 = acctMRDIncrement2.HasValue ? acctMRDIncrement2.Value.ToString() : appMRDIncrement2.ToString();
        }

        // Background Rate 2.
        if (locationLevelInfo.BackgroundRate2.HasValue)
        {
            this.txtBackgroundRate2_Location.Text = (locationLevelInfo.BackgroundRate2.Value).ToString();
            curDisplayFactorValues.backgroundRate2 = (locationLevelInfo.BackgroundRate2.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRate2 = backgroundRate2;
        }

        // Algorithm Factor.
        if (locationLevelInfo.AlgorithmFactor != null)
        {
            this.txtAlgorithmFactor_Location.Text = locationLevelInfo.AlgorithmFactor;
            curDisplayFactorValues.algorithmFactor = locationLevelInfo.AlgorithmFactor;
        }
        else
        {
            curDisplayFactorValues.algorithmFactor = acctAlgorithmFactor ?? appAlgorithmFactor;
        }


        //-------------------------------- MRD 2 New (ID2 New) ---------------------------------------- //
        if (locationLevelInfo.MRD2New.HasValue)
        {
            this.txtMRD2New_Location.Text = (locationLevelInfo.MRD2New.Value).ToString();
            curDisplayFactorValues.mrd2New = (locationLevelInfo.MRD2New.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrd2New = acctMRD2New.HasValue ? acctMRD2New.Value.ToString() : appMRD2New.ToString();
        }

        // MRD Increment 2New (ID2 New).
        if (locationLevelInfo.MRDIncrement2New.HasValue)
        {
            this.txtMRDIncrement2New_Location.Text = (locationLevelInfo.MRDIncrement2New.Value).ToString();
            curDisplayFactorValues.mrdIncrement2New = (locationLevelInfo.MRDIncrement2New.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrement2New = acctMRDIncrement2New.HasValue ? acctMRDIncrement2New.Value.ToString() : appMRDIncrement2New.ToString();
        }

        // Background Rate 2 New.
        if (locationLevelInfo.BackgroundRate2New.HasValue)
        {
            this.txtBackgroundRate2New_Location.Text = (locationLevelInfo.BackgroundRate2New.Value).ToString();
            curDisplayFactorValues.backgroundRate2New = (locationLevelInfo.BackgroundRate2New.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRate2New = backgroundRate2New;
        }

        //------------------------- Read Limit ---------------------------------------------------//
        if (locationLevelInfo.ReadLimit.HasValue)
        {
            this.txtReadLimit_Location.Text = (locationLevelInfo.ReadLimit.Value).ToString();
            curDisplayFactorValues.readLimit = (locationLevelInfo.ReadLimit.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.readLimit = acctReadLimit.HasValue ? acctReadLimit.Value.ToString() : appReadLimit.ToString();
        }


        // ---------------------- MRD Vue -----------------------------------------------------------//
        if (locationLevelInfo.MRDVue.HasValue)
        {
            this.txtMRDVue_Location.Text = (locationLevelInfo.MRDVue.Value).ToString();
            curDisplayFactorValues.mrdVue = (locationLevelInfo.MRDVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdVue = acctMRDVue.HasValue ? acctMRDVue.Value.ToString() : appMRDVue.ToString();
        }

        // MRD Increment Vue.
        if (locationLevelInfo.MRDIncrementVue.HasValue)
        {
            this.txtMRDIncrementVue_Location.Text = (locationLevelInfo.MRDIncrementVue.Value).ToString();
            curDisplayFactorValues.mrdIncrementVue = (locationLevelInfo.MRDIncrementVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrementVue = acctMRDIncrementVue.HasValue ? acctMRDIncrementVue.Value.ToString() : appMRDIncrementVue.ToString();
        }

        // Eye Factor Vue.
        if (locationLevelInfo.EyeFactorVue.HasValue)
        {
            this.txtEyeFactorVue_Location.Text = (locationLevelInfo.EyeFactorVue.Value).ToString();
            curDisplayFactorValues.eyeFactorVue = (locationLevelInfo.EyeFactorVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.eyeFactorVue = acctEyeFactorVue.HasValue ? acctEyeFactorVue.Value.ToString() : dfEyeFactorVue.ToString();
        }

        // Shallow Factor Vue.
        if (locationLevelInfo.ShallowFactorVue.HasValue)
        {
            this.txtShallowFactorVue_Location.Text = (locationLevelInfo.ShallowFactorVue.Value).ToString();
            curDisplayFactorValues.shallowFactorVue = (locationLevelInfo.ShallowFactorVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.shallowFactorVue = acctShallowFactorVue.HasValue ? acctShallowFactorVue.Value.ToString() : dfShallowFactorVue.ToString();
        }

        // Background Rate Vue.
        if (locationLevelInfo.BackgroundRateVue.HasValue)
        {
            this.txtBackgroundRateVue_Location.Text = (locationLevelInfo.BackgroundRateVue.Value).ToString();
            curDisplayFactorValues.backgroundRateVue = (locationLevelInfo.BackgroundRateVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRateVue = backgroundRateVue;
        }

        // Deep Factor Vue
        if (locationLevelInfo.DeepFactorVue.HasValue)
        {
            this.txtDeepFactorVue_Location.Text = (locationLevelInfo.DeepFactorVue.Value).ToString();
            curDisplayFactorValues.deepfactorVue = (locationLevelInfo.DeepFactorVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.deepfactorVue = acctDeepFactorVue.HasValue ? acctDeepFactorVue.Value.ToString() : dfDeepFactorVue.ToString();
        }


        // --------------------------------------------------------------------------------------------------------------------------------------------------------//

        // Use this session to save all displaying factors values before commit change. Use this session to pass in previous factor value to FactorsHistoryLog table
        Session["CurrentDisplayFactorValues"] = null;
        Session["CurrentDisplayFactorValues"] = curDisplayFactorValues;

        // Placeholder will always be present, in case User decideds not to enter an Location-Level value.
        this.txtMRD_Location.Attributes.Add("placeholder", acctMRD.HasValue ? acctMRD.Value.ToString() : appMRD.ToString());
        this.txtMRDIncrement_Location.Attributes.Add("placeholder", acctMRDIncrement.HasValue ? acctMRDIncrement.Value.ToString() : appMRDIncrement.ToString());
        this.txtDeepFactor_Location.Attributes.Add("placeholder", acctDeepFactor.HasValue ? acctDeepFactor.Value.ToString() : dfDeepFactor.ToString());
        this.txtEyeFactor_Location.Attributes.Add("placeholder", acctEyeFactor.HasValue ? acctEyeFactor.Value.ToString() : dfEyeFactor.ToString());
        this.txtShallowFactor_Location.Attributes.Add("placeholder", acctShallowFactor.HasValue ? acctShallowFactor.Value.ToString() : dfShallowFactor.ToString());
        this.txtBackgroundRate_Location.Attributes.Add("placeholder", backgroundRate);

        this.txtMRDPlus_Location.Attributes.Add("placeholder", acctMRDPlus.HasValue ? acctMRDPlus.Value.ToString() : appMRDPlus.ToString());
        this.txtMRDIncrementPlus_Location.Attributes.Add("placeholder", acctMRDIncrementPlus.HasValue ? acctMRDIncrementPlus.Value.ToString() : appMRDIncrementPlus.ToString());
        this.txtDeepFactorPlus_Location.Attributes.Add("placeholder", acctDeepFactorPlus.HasValue ? acctDeepFactorPlus.Value.ToString() : dfDeepFactor.ToString());
        this.txtEyeFactorPlus_Location.Attributes.Add("placeholder", acctEyeFactorPlus.HasValue ? acctEyeFactorPlus.Value.ToString() : dfEyeFactor.ToString());
        this.txtShallowFactorPlus_Location.Attributes.Add("placeholder", acctShallowFactorPlus.HasValue ? acctShallowFactorPlus.Value.ToString() : dfShallowFactor.ToString());
        this.txtBackgroundRatePlus_Location.Attributes.Add("placeholder", backgroundRatePlus);

        this.txtMRD2_Location.Attributes.Add("placeholder", acctMRD2.HasValue ? acctMRD2.Value.ToString() : appMRD2.ToString());
        this.txtMRDIncrement2_Location.Attributes.Add("placeholder", acctMRDIncrement2.HasValue ? acctMRDIncrement2.Value.ToString() : appMRDIncrement2.ToString());
        this.txtBackgroundRate2_Location.Attributes.Add("placeholder", backgroundRate2);
        this.txtAlgorithmFactor_Location.Attributes.Add("placeholder", acctAlgorithmFactor ?? appAlgorithmFactor);

        this.txtMRD2New_Location.Attributes.Add("placeholder", acctMRD2New.HasValue ? acctMRD2New.Value.ToString() : appMRD2New.ToString());
        this.txtMRDIncrement2New_Location.Attributes.Add("placeholder", acctMRDIncrement2New.HasValue ? acctMRDIncrement2New.Value.ToString() : appMRDIncrement2New.ToString());
        this.txtBackgroundRate2New_Location.Attributes.Add("placeholder", backgroundRate2New);

        this.txtReadLimit_Location.Attributes.Add("placeholder", acctReadLimit.HasValue ? acctReadLimit.Value.ToString() : appReadLimit.ToString());

        this.txtMRDVue_Location.Attributes.Add("placeholder", acctMRDVue.HasValue ? acctMRDVue.Value.ToString() : appMRDVue.ToString());
        this.txtMRDIncrementVue_Location.Attributes.Add("placeholder", acctMRDIncrementVue.HasValue ? acctMRDIncrementVue.Value.ToString() : appMRDIncrementVue.ToString());
        this.txtEyeFactorVue_Location.Attributes.Add("placeholder", acctEyeFactorVue.HasValue ? acctEyeFactorVue.Value.ToString() : dfEyeFactorVue.ToString());
        this.txtShallowFactorVue_Location.Attributes.Add("placeholder", acctShallowFactorVue.HasValue ? acctShallowFactorVue.Value.ToString() : dfShallowFactorVue.ToString());
        this.txtBackgroundRateVue_Location.Attributes.Add("placeholder", acctBackgroundRateVue.HasValue ? acctBackgroundRateVue.Value.ToString() : backgroundRateVue);
        this.txtDeepFactorVue_Location.Attributes.Add("placeholder", acctDeepFactorVue.HasValue ? acctDeepFactorVue.Value.ToString() : dfDeepFactorVue.ToString());

        // Send Javascript to client to OPEN/DISPLAY the modal dialog.
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('divLocationSettingsDialog')", true);
    }

    protected void gvLocationSettings_IDVue_RowCommand(object sender, GridViewCommandEventArgs e)
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

        // Account Settings.
        var accountLevelInfo = (from acct in idc.Accounts
                                where acct.AccountID == this.accountID
                                select new
                                {
                                }).FirstOrDefault();

        if (accountLevelInfo == null) return;

        this.txtNotes_Location.Text = "";
        this.lblLocationName.Text = locationLevelInfo.LocationName;
        this.lblAccountID_Location.Text = this.account;

        // Use this variable to save the displaying factor values in the controls when first loading
        SettingFactors curDisplayFactorValues = new SettingFactors();

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
        string acctAlgorithmFactor = null;

        int? acctMRD2New = null;
        int? acctMRDIncrement2New = null;
        double? acctBackgroundRate2New = null;

        double? acctReadLimit = null;

        int? acctMRDVue = null;
        int? acctMRDIncrementVue = null;
        double? acctEyeFactorVue = null;
        double? acctShallowFactorVue = null;
        double? acctBackgroundRateVue = null;
        double? acctDeepFactorVue = null;

        // ------------------------------------AccountSettings ----------------------------------//
        this.GetAccountSettings(this.accountID, out acctCompanyName, out acctMRD, out acctMRDIncrement, out acctMRD2, out acctMRDIncrement2, out acctDeepFactor, out acctEyeFactor, out acctShallowFactor, out acctBackgroundRate, out acctBackgroundRate2, out acctReadLimit, out acctAlgorithmFactor, out acctMRDPlus, out acctMRDIncrementPlus, out acctDeepFactorPlus, out acctEyeFactorPlus, out acctShallowFactorPlus, out acctBackgroundRatePlus, out acctMRD2New, out acctMRDIncrement2New, out acctBackgroundRate2New, out acctMRDVue, out acctBackgroundRateVue, out acctDeepFactorVue, out acctMRDIncrementVue, out acctEyeFactorVue, out acctShallowFactorVue);
        // --------------------------------------------------------------------------------------//
        // ------------------------------------ AppSettings.-------------------------------------//
        string appMRD = Basics.GetSetting("MRD");
        string appMRDIncrement = Basics.GetSetting("MRDIncrement");

        string appMRDPlus = Basics.GetSetting("MRDPlus");
        string appMRDIncrementPlus = Basics.GetSetting("MRDIncrementPlus");

        string appMRD2 = Basics.GetSetting("MRDID2");
        string appMRDIncrement2 = Basics.GetSetting("MRDIncrementID2");
        string appAlgorithmFactor = Basics.GetSetting("AlgorithmFactorID2");

        string appMRD2New = Basics.GetSetting("MRDID2New");
        string appMRDIncrement2New = Basics.GetSetting("MRDIncrementID2New");

        string appReadLimit = Basics.GetSetting("ReadLimit");

        string appMRDVue = Basics.GetSetting("MRDIDVue");
        string appMRDIncrementVue = Basics.GetSetting("MRDIncrementIDVue");
        string appBackgroundRateVue = Basics.GetSetting("ID VUE Background");

        // DeviceFirmware.
        double dfDeepFactor = 0;
        double dfEyeFactor = 0;
        double dfShallowFactor = 0;
        this.GetDeviceFirmware(out dfDeepFactor, out dfEyeFactor, out dfShallowFactor);

        // Control Badge, State & Country.
        double? stateBackgroundRate = null;
        double? countryBackgroundRate = null;
        double? actControlBackgroundRate = null;
        double? locControlBackgroundRate = null;

        double? dfbackgroundRateVue = null;

        double dfDeepFactorVue = 1;
        double dfEyeFactorVue = 1;
        double dfShallowFactorVue = 1;

        this.GetStateAndCountry(accountID, out stateBackgroundRate, out countryBackgroundRate);

        var act_cbrDLObj = (from cbr in idc.ControlBadgeReads
                            where
                            cbr.AccountID == this.accountID
                            && !cbr.LocationID.HasValue
                            && cbr.AvgDLRate.HasValue
                            && cbr.PermEliminatedDL == false
                            && cbr.IsEliminatedDL == false
                            && cbr.ReadTypeID == 17
                            orderby cbr.DoseDate descending
                            select cbr).FirstOrDefault();
        if (act_cbrDLObj != null) actControlBackgroundRate = act_cbrDLObj.AvgDLRate.Value;

        var loc_cbrDLObj = (from cbr in idc.ControlBadgeReads
                            where
                            cbr.AccountID == this.accountID
                            && cbr.LocationID == locationID
                            && cbr.AvgDLRate.HasValue
                            && cbr.PermEliminatedDL == false
                            && cbr.IsEliminatedDL == false
                            && cbr.ReadTypeID == 17
                            orderby cbr.DoseDate descending
                            select cbr).FirstOrDefault();
        if (loc_cbrDLObj != null) locControlBackgroundRate = loc_cbrDLObj.AvgDLRate.Value;

        string backgroundRate = acctBackgroundRate.HasValue ? acctBackgroundRate.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
        string backgroundRatePlus = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctBackgroundRatePlus.HasValue ? acctBackgroundRatePlus.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
        string backgroundRate2 = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctBackgroundRate2.HasValue ? acctBackgroundRate2.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
        string backgroundRate2New = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctBackgroundRate2New.HasValue ? acctBackgroundRate2New.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
        string backgroundRateVue = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctBackgroundRateVue.HasValue ? acctBackgroundRateVue.Value.ToString() : appBackgroundRateVue;
        //string backgroundRateVue = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctBackgroundRateVue.HasValue ? acctBackgroundRateVue.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
        // --------------------------------------------------------------------------------------//                       

        // ---------------------- MRD -----------------------------------------------------------//
        if (locationLevelInfo.MRD.HasValue)
        {
            this.txtMRD_Location.Text = (locationLevelInfo.MRD.Value).ToString();
            curDisplayFactorValues.mrd = (locationLevelInfo.MRD.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrd = acctMRD.HasValue ? acctMRD.Value.ToString() : appMRD.ToString();
        }

        // MRD Increment.
        if (locationLevelInfo.MRDIncrement.HasValue)
        {
            this.txtMRDIncrement_Location.Text = (locationLevelInfo.MRDIncrement.Value).ToString();
            curDisplayFactorValues.mrdIncrement = (locationLevelInfo.MRDIncrement.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrement = acctMRDIncrement.HasValue ? acctMRDIncrement.Value.ToString() : appMRDIncrement.ToString();
        }

        // Deep Factor.
        if (locationLevelInfo.DeepFactor.HasValue)
        {
            this.txtDeepFactor_Location.Text = (locationLevelInfo.DeepFactor.Value).ToString();
            curDisplayFactorValues.deepFactor = (locationLevelInfo.DeepFactor.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.deepFactor = acctDeepFactor.HasValue ? acctDeepFactor.Value.ToString() : dfDeepFactor.ToString();
        }

        // Eye Factor.
        if (locationLevelInfo.EyeFactor.HasValue)
        {
            this.txtEyeFactor_Location.Text = (locationLevelInfo.EyeFactor.Value).ToString();
            curDisplayFactorValues.eyeFactor = (locationLevelInfo.EyeFactor.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.eyeFactor = acctEyeFactor.HasValue ? acctEyeFactor.Value.ToString() : dfEyeFactor.ToString();
        }

        // Shallow Factor.
        if (locationLevelInfo.ShallowFactor.HasValue)
        {
            this.txtShallowFactor_Location.Text = (locationLevelInfo.ShallowFactor.Value).ToString();
            curDisplayFactorValues.shallowFactor = (locationLevelInfo.ShallowFactor.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.shallowFactor = acctShallowFactor.HasValue ? acctShallowFactor.Value.ToString() : dfShallowFactor.ToString();
        }

        // Background Rate.
        if (locationLevelInfo.BackgroundRate.HasValue)
        {
            this.txtBackgroundRate_Location.Text = (locationLevelInfo.BackgroundRate.Value).ToString();
            curDisplayFactorValues.backgroundRate = (locationLevelInfo.BackgroundRate.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRate = backgroundRate;
        }

        //------------------------------ MRD+ ------------------------------------------------------//
        if (locationLevelInfo.MRDPlus.HasValue)
        {
            this.txtMRDPlus_Location.Text = (locationLevelInfo.MRDPlus.Value).ToString();
            curDisplayFactorValues.mrdPlus = (locationLevelInfo.MRDPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdPlus = acctMRDPlus.HasValue ? acctMRDPlus.Value.ToString() : appMRDPlus.ToString();
        }

        // MRD Increment+.
        if (locationLevelInfo.MRDIncrementPlus.HasValue)
        {
            this.txtMRDIncrementPlus_Location.Text = (locationLevelInfo.MRDIncrementPlus.Value).ToString();
            curDisplayFactorValues.mrdIncrementPlus = (locationLevelInfo.MRDIncrementPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrementPlus = acctMRDIncrementPlus.HasValue ? acctMRDIncrementPlus.Value.ToString() : appMRDIncrementPlus.ToString();
        }

        // Deep Factor+.
        if (locationLevelInfo.DeepFactorPlus.HasValue)
        {
            this.txtDeepFactorPlus_Location.Text = (locationLevelInfo.DeepFactorPlus.Value).ToString();
            curDisplayFactorValues.deepFactorPlus = (locationLevelInfo.DeepFactorPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.deepFactorPlus = acctDeepFactorPlus.HasValue ? acctDeepFactorPlus.Value.ToString() : dfDeepFactor.ToString();
        }

        // Eye Factor+.
        if (locationLevelInfo.EyeFactorPlus.HasValue)
        {
            this.txtEyeFactorPlus_Location.Text = (locationLevelInfo.EyeFactorPlus.Value).ToString();
            curDisplayFactorValues.eyeFactorPlus = (locationLevelInfo.EyeFactorPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.eyeFactorPlus = acctEyeFactorPlus.HasValue ? acctEyeFactorPlus.Value.ToString() : dfEyeFactor.ToString();
        }

        // Shallow Factor+.
        if (locationLevelInfo.ShallowFactorPlus.HasValue)
        {
            this.txtShallowFactorPlus_Location.Text = (locationLevelInfo.ShallowFactorPlus.Value).ToString();
            curDisplayFactorValues.shallowFactorPlus = (locationLevelInfo.ShallowFactorPlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.shallowFactorPlus = acctShallowFactorPlus.HasValue ? acctShallowFactorPlus.Value.ToString() : dfShallowFactor.ToString();
        }

        // Background Rate+.
        if (locationLevelInfo.BackgroundRatePlus.HasValue)
        {
            this.txtBackgroundRatePlus_Location.Text = (locationLevelInfo.BackgroundRatePlus.Value).ToString();
            curDisplayFactorValues.backgroundRatePlus = (locationLevelInfo.BackgroundRatePlus.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRatePlus = backgroundRatePlus;
        }

        //------------------------------- MRD 2 (ID2) -------------------------------------------------------//
        if (locationLevelInfo.MRD2.HasValue)
        {
            this.txtMRD2_Location.Text = (locationLevelInfo.MRD2.Value).ToString();
            curDisplayFactorValues.mrd2 = (locationLevelInfo.MRD2.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrd2 = acctMRD2.HasValue ? acctMRD2.Value.ToString() : appMRD2.ToString();
        }

        // MRD Increment 2 (ID2).
        if (locationLevelInfo.MRDIncrement2.HasValue)
        {
            this.txtMRDIncrement2_Location.Text = (locationLevelInfo.MRDIncrement2.Value).ToString();
            curDisplayFactorValues.mrdIncrement2 = (locationLevelInfo.MRDIncrement2.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrement2 = acctMRDIncrement2.HasValue ? acctMRDIncrement2.Value.ToString() : appMRDIncrement2.ToString();
        }

        // Background Rate 2.
        if (locationLevelInfo.BackgroundRate2.HasValue)
        {
            this.txtBackgroundRate2_Location.Text = (locationLevelInfo.BackgroundRate2.Value).ToString();
            curDisplayFactorValues.backgroundRate2 = (locationLevelInfo.BackgroundRate2.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRate2 = backgroundRate2;
        }

        // Algorithm Factor.
        if (locationLevelInfo.AlgorithmFactor != null)
        {
            this.txtAlgorithmFactor_Location.Text = locationLevelInfo.AlgorithmFactor;
            curDisplayFactorValues.algorithmFactor = locationLevelInfo.AlgorithmFactor;
        }
        else
        {
            curDisplayFactorValues.algorithmFactor = acctAlgorithmFactor ?? appAlgorithmFactor;
        }


        //-------------------------------- MRD 2 New (ID2 New) ---------------------------------------- //
        if (locationLevelInfo.MRD2New.HasValue)
        {
            this.txtMRD2New_Location.Text = (locationLevelInfo.MRD2New.Value).ToString();
            curDisplayFactorValues.mrd2New = (locationLevelInfo.MRD2New.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrd2New = acctMRD2New.HasValue ? acctMRD2New.Value.ToString() : appMRD2New.ToString();
        }

        // MRD Increment 2New (ID2 New).
        if (locationLevelInfo.MRDIncrement2New.HasValue)
        {
            this.txtMRDIncrement2New_Location.Text = (locationLevelInfo.MRDIncrement2New.Value).ToString();
            curDisplayFactorValues.mrdIncrement2New = (locationLevelInfo.MRDIncrement2New.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrement2New = acctMRDIncrement2New.HasValue ? acctMRDIncrement2New.Value.ToString() : appMRDIncrement2New.ToString();
        }

        // Background Rate 2 New.
        if (locationLevelInfo.BackgroundRate2New.HasValue)
        {
            this.txtBackgroundRate2New_Location.Text = (locationLevelInfo.BackgroundRate2New.Value).ToString();
            curDisplayFactorValues.backgroundRate2New = (locationLevelInfo.BackgroundRate2New.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRate2New = backgroundRate2New;
        }

        //------------------------- Read Limit ---------------------------------------------------//
        if (locationLevelInfo.ReadLimit.HasValue)
        {
            this.txtReadLimit_Location.Text = (locationLevelInfo.ReadLimit.Value).ToString();
            curDisplayFactorValues.readLimit = (locationLevelInfo.ReadLimit.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.readLimit = acctReadLimit.HasValue ? acctReadLimit.Value.ToString() : appReadLimit.ToString();
        }


        // ---------------------- MRD Vue -----------------------------------------------------------//
        if (locationLevelInfo.MRDVue.HasValue)
        {
            this.txtMRDVue_Location.Text = (locationLevelInfo.MRDVue.Value).ToString();
            curDisplayFactorValues.mrdVue = (locationLevelInfo.MRDVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdVue = acctMRDVue.HasValue ? acctMRDVue.Value.ToString() : appMRDVue.ToString();
        }

        // MRD Increment Vue.
        if (locationLevelInfo.MRDIncrementVue.HasValue)
        {
            this.txtMRDIncrementVue_Location.Text = (locationLevelInfo.MRDIncrementVue.Value).ToString();
            curDisplayFactorValues.mrdIncrementVue = (locationLevelInfo.MRDIncrementVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.mrdIncrementVue = acctMRDIncrementVue.HasValue ? acctMRDIncrementVue.Value.ToString() : appMRDIncrementVue.ToString();
        }

        // Eye Factor Vue.
        if (locationLevelInfo.EyeFactorVue.HasValue)
        {
            this.txtEyeFactorVue_Location.Text = (locationLevelInfo.EyeFactorVue.Value).ToString();
            curDisplayFactorValues.eyeFactorVue = (locationLevelInfo.EyeFactorVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.eyeFactorVue = acctEyeFactorVue.HasValue ? acctEyeFactorVue.Value.ToString() : dfEyeFactorVue.ToString();
        }

        // Shallow Factor Vue.
        if (locationLevelInfo.ShallowFactorVue.HasValue)
        {
            this.txtShallowFactorVue_Location.Text = (locationLevelInfo.ShallowFactorVue.Value).ToString();
            curDisplayFactorValues.shallowFactorVue = (locationLevelInfo.ShallowFactorVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.shallowFactorVue = acctShallowFactorVue.HasValue ? acctShallowFactorVue.Value.ToString() : dfShallowFactorVue.ToString();
        }

        // Background Rate Vue.
        if (locationLevelInfo.BackgroundRateVue.HasValue)
        {
            this.txtBackgroundRateVue_Location.Text = (locationLevelInfo.BackgroundRateVue.Value).ToString();
            curDisplayFactorValues.backgroundRateVue = (locationLevelInfo.BackgroundRateVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.backgroundRateVue = backgroundRateVue;
        }

        // Deep Factor Vue
        if (locationLevelInfo.DeepFactorVue.HasValue)
        {
            this.txtDeepFactorVue_Location.Text = (locationLevelInfo.DeepFactorVue.Value).ToString();
            curDisplayFactorValues.deepfactorVue = (locationLevelInfo.DeepFactorVue.Value).ToString();
        }
        else
        {
            curDisplayFactorValues.deepfactorVue = acctDeepFactorVue.HasValue ? acctDeepFactorVue.Value.ToString() : dfDeepFactorVue.ToString();
        }


        // --------------------------------------------------------------------------------------------------------------------------------------------------------//

        // Use this session to save all displaying factors values before commit change. Use this session to pass in previous factor value to FactorsHistoryLog table
        Session["CurrentDisplayFactorValues"] = null;
        Session["CurrentDisplayFactorValues"] = curDisplayFactorValues;

        // Placeholder will always be present, in case User decideds not to enter an Location-Level value.
        this.txtMRD_Location.Attributes.Add("placeholder", acctMRD.HasValue ? acctMRD.Value.ToString() : appMRD.ToString());
        this.txtMRDIncrement_Location.Attributes.Add("placeholder", acctMRDIncrement.HasValue ? acctMRDIncrement.Value.ToString() : appMRDIncrement.ToString());
        this.txtDeepFactor_Location.Attributes.Add("placeholder", acctDeepFactor.HasValue ? acctDeepFactor.Value.ToString() : dfDeepFactor.ToString());
        this.txtEyeFactor_Location.Attributes.Add("placeholder", acctEyeFactor.HasValue ? acctEyeFactor.Value.ToString() : dfEyeFactor.ToString());
        this.txtShallowFactor_Location.Attributes.Add("placeholder", acctShallowFactor.HasValue ? acctShallowFactor.Value.ToString() : dfShallowFactor.ToString());
        this.txtBackgroundRate_Location.Attributes.Add("placeholder", backgroundRate);

        this.txtMRDPlus_Location.Attributes.Add("placeholder", acctMRDPlus.HasValue ? acctMRDPlus.Value.ToString() : appMRDPlus.ToString());
        this.txtMRDIncrementPlus_Location.Attributes.Add("placeholder", acctMRDIncrementPlus.HasValue ? acctMRDIncrementPlus.Value.ToString() : appMRDIncrementPlus.ToString());
        this.txtDeepFactorPlus_Location.Attributes.Add("placeholder", acctDeepFactorPlus.HasValue ? acctDeepFactorPlus.Value.ToString() : dfDeepFactor.ToString());
        this.txtEyeFactorPlus_Location.Attributes.Add("placeholder", acctEyeFactorPlus.HasValue ? acctEyeFactorPlus.Value.ToString() : dfEyeFactor.ToString());
        this.txtShallowFactorPlus_Location.Attributes.Add("placeholder", acctShallowFactorPlus.HasValue ? acctShallowFactorPlus.Value.ToString() : dfShallowFactor.ToString());
        this.txtBackgroundRatePlus_Location.Attributes.Add("placeholder", backgroundRatePlus);

        this.txtMRD2_Location.Attributes.Add("placeholder", acctMRD2.HasValue ? acctMRD2.Value.ToString() : appMRD2.ToString());
        this.txtMRDIncrement2_Location.Attributes.Add("placeholder", acctMRDIncrement2.HasValue ? acctMRDIncrement2.Value.ToString() : appMRDIncrement2.ToString());
        this.txtBackgroundRate2_Location.Attributes.Add("placeholder", backgroundRate2);
        this.txtAlgorithmFactor_Location.Attributes.Add("placeholder", acctAlgorithmFactor ?? appAlgorithmFactor);

        this.txtMRD2New_Location.Attributes.Add("placeholder", acctMRD2New.HasValue ? acctMRD2New.Value.ToString() : appMRD2New.ToString());
        this.txtMRDIncrement2New_Location.Attributes.Add("placeholder", acctMRDIncrement2New.HasValue ? acctMRDIncrement2New.Value.ToString() : appMRDIncrement2New.ToString());
        this.txtBackgroundRate2New_Location.Attributes.Add("placeholder", backgroundRate2New);

        this.txtReadLimit_Location.Attributes.Add("placeholder", acctReadLimit.HasValue ? acctReadLimit.Value.ToString() : appReadLimit.ToString());


        this.txtMRDVue_Location.Attributes.Add("placeholder", acctMRDVue.HasValue ? acctMRDVue.Value.ToString() : appMRDVue.ToString());
        this.txtMRDIncrementVue_Location.Attributes.Add("placeholder", acctMRDIncrementVue.HasValue ? acctMRDIncrementVue.Value.ToString() : appMRDIncrementVue.ToString());
        this.txtEyeFactorVue_Location.Attributes.Add("placeholder", acctEyeFactorVue.HasValue ? acctEyeFactorVue.Value.ToString() : dfEyeFactorVue.ToString());
        this.txtShallowFactorVue_Location.Attributes.Add("placeholder", acctShallowFactorVue.HasValue ? acctShallowFactorVue.Value.ToString() : dfShallowFactorVue.ToString());
        this.txtBackgroundRateVue_Location.Attributes.Add("placeholder", acctBackgroundRateVue.HasValue ? acctBackgroundRateVue.Value.ToString() : backgroundRateVue);
        this.txtDeepFactorVue_Location.Attributes.Add("placeholder", acctDeepFactorVue.HasValue ? acctDeepFactorVue.Value.ToString() : dfDeepFactorVue.ToString());

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
    protected void gvLocationSettings_ID1_RowDataBound(object sender, GridViewRowEventArgs e)
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

        //int indexOfMRDPlus = this.GetColumnIndexByName(e.Row, "MRDPlus");
        //int indexOfMRDIncrementPlus = this.GetColumnIndexByName(e.Row, "MRDIncrementPlus");
        //int indexOfDeepFactorPlus = this.GetColumnIndexByName(e.Row, "DeepFactorPlus");
        //int indexOfEyeFactorPlus = this.GetColumnIndexByName(e.Row, "EyeFactorPlus");
        //int indexOfShallowFactorPlus = this.GetColumnIndexByName(e.Row, "ShallowFactorPlus");
        //int indexOfBackgroundRatePlus = this.GetColumnIndexByName(e.Row, "BackgroundRatePlus");

        //int indexOfMRD2 = this.GetColumnIndexByName(e.Row, "MRD2");
        //int indexOfMRDIncrement2 = this.GetColumnIndexByName(e.Row, "MRDIncrement2");
        //int indexOfBackgroundRate2 = this.GetColumnIndexByName(e.Row, "BackgroundRate2");

        //int indexOfMRD2New = this.GetColumnIndexByName(e.Row, "MRD2New");
        //int indexOfMRDIncrement2New = this.GetColumnIndexByName(e.Row, "MRDIncrement2New");
        //int indexOfBackgroundRate2New = this.GetColumnIndexByName(e.Row, "BackgroundRate2New");

        int indexOfReadLimit = this.GetColumnIndexByName(e.Row, "ReadLimit");
        //int indexOfAlgorithmFactor = this.GetColumnIndexByName(e.Row, "AlgorithmFactor");

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

        //// MRD+
        //if (locationFactors[e.Row.DataItemIndex].MRDPlusFrom == 'L')
        //    e.Row.Cells[indexOfMRDPlus].CssClass = "AlteredField";

        //// MRD Increment+
        //if (locationFactors[e.Row.DataItemIndex].MRDIncrementPlusFrom == 'L')
        //    e.Row.Cells[indexOfMRDIncrementPlus].CssClass = "AlteredField";

        //// Deep Factor+
        //if (locationFactors[e.Row.DataItemIndex].DeepFactorPlusFrom == 'L')
        //    e.Row.Cells[indexOfDeepFactorPlus].CssClass = "AlteredField";

        //// Eye Factor+
        //if (locationFactors[e.Row.DataItemIndex].EyeFactorPlusFrom == 'L')
        //    e.Row.Cells[indexOfEyeFactorPlus].CssClass = "AlteredField";

        //// Shallow Factor+
        //if (locationFactors[e.Row.DataItemIndex].ShallowFactorPlusFrom == 'L')
        //    e.Row.Cells[indexOfShallowFactorPlus].CssClass = "AlteredField";

        //// Background Rate+
        //if (locationFactors[e.Row.DataItemIndex].BackgroundRatePlusFrom == 'L')
        //    e.Row.Cells[indexOfBackgroundRatePlus].CssClass = "AlteredField";

        //// MRD2 (ID2)
        //if (locationFactors[e.Row.DataItemIndex].MRD2From == 'L')
        //    e.Row.Cells[indexOfMRD2].CssClass = "AlteredField";

        //// MRD Increment 2 (ID2)
        //if (locationFactors[e.Row.DataItemIndex].MRDIncrement2From == 'L')
        //    e.Row.Cells[indexOfMRDIncrement2].CssClass = "AlteredField";

        //// Background Rate 2
        //if (locationFactors[e.Row.DataItemIndex].BackgroundRate2From == 'L')
        //    e.Row.Cells[indexOfBackgroundRate2].CssClass = "AlteredField";

        //// MRD2New (ID2)
        //if (locationFactors[e.Row.DataItemIndex].MRD2NewFrom == 'L')
        //    e.Row.Cells[indexOfMRD2New].CssClass = "AlteredField";

        //// MRD Increment 2 New (ID2)
        //if (locationFactors[e.Row.DataItemIndex].MRDIncrement2NewFrom == 'L')
        //    e.Row.Cells[indexOfMRDIncrement2New].CssClass = "AlteredField";

        //// Background Rate 2 New
        //if (locationFactors[e.Row.DataItemIndex].BackgroundRate2NewFrom == 'L')
        //    e.Row.Cells[indexOfBackgroundRate2New].CssClass = "AlteredField";

        // Read Limit
        if (locationFactors[e.Row.DataItemIndex].ReadLimitFrom == 'L')
            e.Row.Cells[indexOfReadLimit].CssClass = "AlteredField";

        //// Algorithm Factor (ID2)
        //if (locationFactors[e.Row.DataItemIndex].AlgorithmFactorFrom == 'L')
        //    e.Row.Cells[indexOfAlgorithmFactor].CssClass = "AlteredField";        
    }

    protected void gvLocationSettings_ID2_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        // Includes the last row in the GridView (per page) which is considered a "Footer".
        if (e.Row.RowType != DataControlRowType.DataRow &&
            e.Row.RowType != DataControlRowType.Footer) return;

        if (e.Row.RowIndex == -1) return;

        //int indexOfMRD = this.GetColumnIndexByName(e.Row, "MRD");
        //int indexOfMRDIncrement = this.GetColumnIndexByName(e.Row, "MRDIncrement");
        //int indexOfDeepFactor = this.GetColumnIndexByName(e.Row, "DeepFactor");
        //int indexOfEyeFactor = this.GetColumnIndexByName(e.Row, "EyeFactor");
        //int indexOfShallowFactor = this.GetColumnIndexByName(e.Row, "ShallowFactor");
        //int indexOfBackgroundRate = this.GetColumnIndexByName(e.Row, "BackgroundRate");

        //int indexOfMRDPlus = this.GetColumnIndexByName(e.Row, "MRDPlus");
        //int indexOfMRDIncrementPlus = this.GetColumnIndexByName(e.Row, "MRDIncrementPlus");
        //int indexOfDeepFactorPlus = this.GetColumnIndexByName(e.Row, "DeepFactorPlus");
        //int indexOfEyeFactorPlus = this.GetColumnIndexByName(e.Row, "EyeFactorPlus");
        //int indexOfShallowFactorPlus = this.GetColumnIndexByName(e.Row, "ShallowFactorPlus");
        //int indexOfBackgroundRatePlus = this.GetColumnIndexByName(e.Row, "BackgroundRatePlus");

        //int indexOfMRD2 = this.GetColumnIndexByName(e.Row, "MRD2");
        //int indexOfMRDIncrement2 = this.GetColumnIndexByName(e.Row, "MRDIncrement2");
        //int indexOfBackgroundRate2 = this.GetColumnIndexByName(e.Row, "BackgroundRate2");

        int indexOfMRD2New = this.GetColumnIndexByName(e.Row, "MRD2New");
        int indexOfMRDIncrement2New = this.GetColumnIndexByName(e.Row, "MRDIncrement2New");
        int indexOfBackgroundRate2New = this.GetColumnIndexByName(e.Row, "BackgroundRate2New");

        int indexOfReadLimit = this.GetColumnIndexByName(e.Row, "ReadLimit");
        //int indexOfAlgorithmFactor = this.GetColumnIndexByName(e.Row, "AlgorithmFactor");

        //// MRD
        //if (locationFactors[e.Row.DataItemIndex].MRDFrom == 'L')
        //    e.Row.Cells[indexOfMRD].CssClass = "AlteredField";

        //// MRD Increment
        //if (locationFactors[e.Row.DataItemIndex].MRDIncrementFrom == 'L')
        //    e.Row.Cells[indexOfMRDIncrement].CssClass = "AlteredField";

        //// Deep Factor
        //if (locationFactors[e.Row.DataItemIndex].DeepFactorFrom == 'L')
        //    e.Row.Cells[indexOfDeepFactor].CssClass = "AlteredField";

        //// Eye Factor
        //if (locationFactors[e.Row.DataItemIndex].EyeFactorFrom == 'L')
        //    e.Row.Cells[indexOfEyeFactor].CssClass = "AlteredField";

        //// Shallow Factor
        //if (locationFactors[e.Row.DataItemIndex].ShallowFactorFrom == 'L')
        //    e.Row.Cells[indexOfShallowFactor].CssClass = "AlteredField";

        //// Background Rate
        //if (locationFactors[e.Row.DataItemIndex].BackgroundRateFrom == 'L')
        //    e.Row.Cells[indexOfBackgroundRate].CssClass = "AlteredField";

        //// MRD+
        //if (locationFactors[e.Row.DataItemIndex].MRDPlusFrom == 'L')
        //    e.Row.Cells[indexOfMRDPlus].CssClass = "AlteredField";

        //// MRD Increment+
        //if (locationFactors[e.Row.DataItemIndex].MRDIncrementPlusFrom == 'L')
        //    e.Row.Cells[indexOfMRDIncrementPlus].CssClass = "AlteredField";

        //// Deep Factor+
        //if (locationFactors[e.Row.DataItemIndex].DeepFactorPlusFrom == 'L')
        //    e.Row.Cells[indexOfDeepFactorPlus].CssClass = "AlteredField";

        //// Eye Factor+
        //if (locationFactors[e.Row.DataItemIndex].EyeFactorPlusFrom == 'L')
        //    e.Row.Cells[indexOfEyeFactorPlus].CssClass = "AlteredField";

        //// Shallow Factor+
        //if (locationFactors[e.Row.DataItemIndex].ShallowFactorPlusFrom == 'L')
        //    e.Row.Cells[indexOfShallowFactorPlus].CssClass = "AlteredField";

        //// Background Rate+
        //if (locationFactors[e.Row.DataItemIndex].BackgroundRatePlusFrom == 'L')
        //    e.Row.Cells[indexOfBackgroundRatePlus].CssClass = "AlteredField";

        //// MRD2 (ID2)
        //if (locationFactors[e.Row.DataItemIndex].MRD2From == 'L')
        //    e.Row.Cells[indexOfMRD2].CssClass = "AlteredField";

        //// MRD Increment 2 (ID2)
        //if (locationFactors[e.Row.DataItemIndex].MRDIncrement2From == 'L')
        //    e.Row.Cells[indexOfMRDIncrement2].CssClass = "AlteredField";

        //// Background Rate 2
        //if (locationFactors[e.Row.DataItemIndex].BackgroundRate2From == 'L')
        //    e.Row.Cells[indexOfBackgroundRate2].CssClass = "AlteredField";

        // MRD2New (ID2)
        if (locationFactors[e.Row.DataItemIndex].MRD2NewFrom == 'L')
            e.Row.Cells[indexOfMRD2New].CssClass = "AlteredField";

        // MRD Increment 2 New (ID2)
        if (locationFactors[e.Row.DataItemIndex].MRDIncrement2NewFrom == 'L')
            e.Row.Cells[indexOfMRDIncrement2New].CssClass = "AlteredField";

        // Background Rate 2 New
        if (locationFactors[e.Row.DataItemIndex].BackgroundRate2NewFrom == 'L')
            e.Row.Cells[indexOfBackgroundRate2New].CssClass = "AlteredField";

        // Read Limit
        if (locationFactors[e.Row.DataItemIndex].ReadLimitFrom == 'L')
            e.Row.Cells[indexOfReadLimit].CssClass = "AlteredField";

        //// Algorithm Factor (ID2)
        //if (locationFactors[e.Row.DataItemIndex].AlgorithmFactorFrom == 'L')
        //    e.Row.Cells[indexOfAlgorithmFactor].CssClass = "AlteredField";        
    }

    protected void gvLocationSettings_ID2Elite_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        // Includes the last row in the GridView (per page) which is considered a "Footer".
        if (e.Row.RowType != DataControlRowType.DataRow &&
            e.Row.RowType != DataControlRowType.Footer) return;

        if (e.Row.RowIndex == -1) return;

        //int indexOfMRD = this.GetColumnIndexByName(e.Row, "MRD");
        //int indexOfMRDIncrement = this.GetColumnIndexByName(e.Row, "MRDIncrement");
        //int indexOfDeepFactor = this.GetColumnIndexByName(e.Row, "DeepFactor");
        //int indexOfEyeFactor = this.GetColumnIndexByName(e.Row, "EyeFactor");
        //int indexOfShallowFactor = this.GetColumnIndexByName(e.Row, "ShallowFactor");
        //int indexOfBackgroundRate = this.GetColumnIndexByName(e.Row, "BackgroundRate");

        //int indexOfMRDPlus = this.GetColumnIndexByName(e.Row, "MRDPlus");
        //int indexOfMRDIncrementPlus = this.GetColumnIndexByName(e.Row, "MRDIncrementPlus");
        //int indexOfDeepFactorPlus = this.GetColumnIndexByName(e.Row, "DeepFactorPlus");
        //int indexOfEyeFactorPlus = this.GetColumnIndexByName(e.Row, "EyeFactorPlus");
        //int indexOfShallowFactorPlus = this.GetColumnIndexByName(e.Row, "ShallowFactorPlus");
        //int indexOfBackgroundRatePlus = this.GetColumnIndexByName(e.Row, "BackgroundRatePlus");

        int indexOfMRD2 = this.GetColumnIndexByName(e.Row, "MRD2");
        int indexOfMRDIncrement2 = this.GetColumnIndexByName(e.Row, "MRDIncrement2");
        int indexOfBackgroundRate2 = this.GetColumnIndexByName(e.Row, "BackgroundRate2");

        //int indexOfMRD2New = this.GetColumnIndexByName(e.Row, "MRD2New");
        //int indexOfMRDIncrement2New = this.GetColumnIndexByName(e.Row, "MRDIncrement2New");
        //int indexOfBackgroundRate2New = this.GetColumnIndexByName(e.Row, "BackgroundRate2New");

        int indexOfReadLimit = this.GetColumnIndexByName(e.Row, "ReadLimit");
        int indexOfAlgorithmFactor = this.GetColumnIndexByName(e.Row, "AlgorithmFactor");

        //// MRD
        //if (locationFactors[e.Row.DataItemIndex].MRDFrom == 'L')
        //    e.Row.Cells[indexOfMRD].CssClass = "AlteredField";

        //// MRD Increment
        //if (locationFactors[e.Row.DataItemIndex].MRDIncrementFrom == 'L')
        //    e.Row.Cells[indexOfMRDIncrement].CssClass = "AlteredField";

        //// Deep Factor
        //if (locationFactors[e.Row.DataItemIndex].DeepFactorFrom == 'L')
        //    e.Row.Cells[indexOfDeepFactor].CssClass = "AlteredField";

        //// Eye Factor
        //if (locationFactors[e.Row.DataItemIndex].EyeFactorFrom == 'L')
        //    e.Row.Cells[indexOfEyeFactor].CssClass = "AlteredField";

        //// Shallow Factor
        //if (locationFactors[e.Row.DataItemIndex].ShallowFactorFrom == 'L')
        //    e.Row.Cells[indexOfShallowFactor].CssClass = "AlteredField";

        //// Background Rate
        //if (locationFactors[e.Row.DataItemIndex].BackgroundRateFrom == 'L')
        //    e.Row.Cells[indexOfBackgroundRate].CssClass = "AlteredField";

        //// MRD+
        //if (locationFactors[e.Row.DataItemIndex].MRDPlusFrom == 'L')
        //    e.Row.Cells[indexOfMRDPlus].CssClass = "AlteredField";

        //// MRD Increment+
        //if (locationFactors[e.Row.DataItemIndex].MRDIncrementPlusFrom == 'L')
        //    e.Row.Cells[indexOfMRDIncrementPlus].CssClass = "AlteredField";

        //// Deep Factor+
        //if (locationFactors[e.Row.DataItemIndex].DeepFactorPlusFrom == 'L')
        //    e.Row.Cells[indexOfDeepFactorPlus].CssClass = "AlteredField";

        //// Eye Factor+
        //if (locationFactors[e.Row.DataItemIndex].EyeFactorPlusFrom == 'L')
        //    e.Row.Cells[indexOfEyeFactorPlus].CssClass = "AlteredField";

        //// Shallow Factor+
        //if (locationFactors[e.Row.DataItemIndex].ShallowFactorPlusFrom == 'L')
        //    e.Row.Cells[indexOfShallowFactorPlus].CssClass = "AlteredField";

        //// Background Rate+
        //if (locationFactors[e.Row.DataItemIndex].BackgroundRatePlusFrom == 'L')
        //    e.Row.Cells[indexOfBackgroundRatePlus].CssClass = "AlteredField";

        // MRD2 (ID2)
        if (locationFactors[e.Row.DataItemIndex].MRD2From == 'L')
            e.Row.Cells[indexOfMRD2].CssClass = "AlteredField";

        // MRD Increment 2 (ID2)
        if (locationFactors[e.Row.DataItemIndex].MRDIncrement2From == 'L')
            e.Row.Cells[indexOfMRDIncrement2].CssClass = "AlteredField";

        // Background Rate 2
        if (locationFactors[e.Row.DataItemIndex].BackgroundRate2From == 'L')
            e.Row.Cells[indexOfBackgroundRate2].CssClass = "AlteredField";

        //// MRD2New (ID2)
        //if (locationFactors[e.Row.DataItemIndex].MRD2NewFrom == 'L')
        //    e.Row.Cells[indexOfMRD2New].CssClass = "AlteredField";

        //// MRD Increment 2 New (ID2)
        //if (locationFactors[e.Row.DataItemIndex].MRDIncrement2NewFrom == 'L')
        //    e.Row.Cells[indexOfMRDIncrement2New].CssClass = "AlteredField";

        //// Background Rate 2 New
        //if (locationFactors[e.Row.DataItemIndex].BackgroundRate2NewFrom == 'L')
        //    e.Row.Cells[indexOfBackgroundRate2New].CssClass = "AlteredField";

        // Read Limit
        if (locationFactors[e.Row.DataItemIndex].ReadLimitFrom == 'L')
            e.Row.Cells[indexOfReadLimit].CssClass = "AlteredField";

        // Algorithm Factor (ID2)
        if (locationFactors[e.Row.DataItemIndex].AlgorithmFactorFrom == 'L')
            e.Row.Cells[indexOfAlgorithmFactor].CssClass = "AlteredField";
    }

    protected void gvLocationSettings_IDPlus_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        // Includes the last row in the GridView (per page) which is considered a "Footer".
        if (e.Row.RowType != DataControlRowType.DataRow &&
            e.Row.RowType != DataControlRowType.Footer) return;

        if (e.Row.RowIndex == -1) return;

        //int indexOfMRD = this.GetColumnIndexByName(e.Row, "MRD");
        //int indexOfMRDIncrement = this.GetColumnIndexByName(e.Row, "MRDIncrement");
        //int indexOfDeepFactor = this.GetColumnIndexByName(e.Row, "DeepFactor");
        //int indexOfEyeFactor = this.GetColumnIndexByName(e.Row, "EyeFactor");
        //int indexOfShallowFactor = this.GetColumnIndexByName(e.Row, "ShallowFactor");
        //int indexOfBackgroundRate = this.GetColumnIndexByName(e.Row, "BackgroundRate");

        int indexOfMRDPlus = this.GetColumnIndexByName(e.Row, "MRDPlus");
        int indexOfMRDIncrementPlus = this.GetColumnIndexByName(e.Row, "MRDIncrementPlus");
        int indexOfDeepFactorPlus = this.GetColumnIndexByName(e.Row, "DeepFactorPlus");
        int indexOfEyeFactorPlus = this.GetColumnIndexByName(e.Row, "EyeFactorPlus");
        int indexOfShallowFactorPlus = this.GetColumnIndexByName(e.Row, "ShallowFactorPlus");
        int indexOfBackgroundRatePlus = this.GetColumnIndexByName(e.Row, "BackgroundRatePlus");

        //int indexOfMRD2 = this.GetColumnIndexByName(e.Row, "MRD2");
        //int indexOfMRDIncrement2 = this.GetColumnIndexByName(e.Row, "MRDIncrement2");
        //int indexOfBackgroundRate2 = this.GetColumnIndexByName(e.Row, "BackgroundRate2");

        //int indexOfMRD2New = this.GetColumnIndexByName(e.Row, "MRD2New");
        //int indexOfMRDIncrement2New = this.GetColumnIndexByName(e.Row, "MRDIncrement2New");
        //int indexOfBackgroundRate2New = this.GetColumnIndexByName(e.Row, "BackgroundRate2New");

        int indexOfReadLimit = this.GetColumnIndexByName(e.Row, "ReadLimit");
        //int indexOfAlgorithmFactor = this.GetColumnIndexByName(e.Row, "AlgorithmFactor");

        //// MRD
        //if (locationFactors[e.Row.DataItemIndex].MRDFrom == 'L')
        //    e.Row.Cells[indexOfMRD].CssClass = "AlteredField";

        //// MRD Increment
        //if (locationFactors[e.Row.DataItemIndex].MRDIncrementFrom == 'L')
        //    e.Row.Cells[indexOfMRDIncrement].CssClass = "AlteredField";

        //// Deep Factor
        //if (locationFactors[e.Row.DataItemIndex].DeepFactorFrom == 'L')
        //    e.Row.Cells[indexOfDeepFactor].CssClass = "AlteredField";

        //// Eye Factor
        //if (locationFactors[e.Row.DataItemIndex].EyeFactorFrom == 'L')
        //    e.Row.Cells[indexOfEyeFactor].CssClass = "AlteredField";

        //// Shallow Factor
        //if (locationFactors[e.Row.DataItemIndex].ShallowFactorFrom == 'L')
        //    e.Row.Cells[indexOfShallowFactor].CssClass = "AlteredField";

        //// Background Rate
        //if (locationFactors[e.Row.DataItemIndex].BackgroundRateFrom == 'L')
        //    e.Row.Cells[indexOfBackgroundRate].CssClass = "AlteredField";

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

        //// MRD2 (ID2)
        //if (locationFactors[e.Row.DataItemIndex].MRD2From == 'L')
        //    e.Row.Cells[indexOfMRD2].CssClass = "AlteredField";

        //// MRD Increment 2 (ID2)
        //if (locationFactors[e.Row.DataItemIndex].MRDIncrement2From == 'L')
        //    e.Row.Cells[indexOfMRDIncrement2].CssClass = "AlteredField";

        //// Background Rate 2
        //if (locationFactors[e.Row.DataItemIndex].BackgroundRate2From == 'L')
        //    e.Row.Cells[indexOfBackgroundRate2].CssClass = "AlteredField";

        //// MRD2New (ID2)
        //if (locationFactors[e.Row.DataItemIndex].MRD2NewFrom == 'L')
        //    e.Row.Cells[indexOfMRD2New].CssClass = "AlteredField";

        //// MRD Increment 2 New (ID2)
        //if (locationFactors[e.Row.DataItemIndex].MRDIncrement2NewFrom == 'L')
        //    e.Row.Cells[indexOfMRDIncrement2New].CssClass = "AlteredField";

        //// Background Rate 2 New
        //if (locationFactors[e.Row.DataItemIndex].BackgroundRate2NewFrom == 'L')
        //    e.Row.Cells[indexOfBackgroundRate2New].CssClass = "AlteredField";

        // Read Limit
        if (locationFactors[e.Row.DataItemIndex].ReadLimitFrom == 'L')
            e.Row.Cells[indexOfReadLimit].CssClass = "AlteredField";

        //// Algorithm Factor (ID2)
        //if (locationFactors[e.Row.DataItemIndex].AlgorithmFactorFrom == 'L')
        //    e.Row.Cells[indexOfAlgorithmFactor].CssClass = "AlteredField";        
    }

    protected void gvLocationSettings_IDVue_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        // Includes the last row in the GridView (per page) which is considered a "Footer".
        if (e.Row.RowType != DataControlRowType.DataRow &&
            e.Row.RowType != DataControlRowType.Footer) return;

        if (e.Row.RowIndex == -1) return;

        int indexOfMRDVue = this.GetColumnIndexByName(e.Row, "MRDVue");
        int indexOfMRDIncrementVue = this.GetColumnIndexByName(e.Row, "MRDIncrementVue");
        int indexOfDeepFactorVue = this.GetColumnIndexByName(e.Row, "DeepFactorVue");
        int indexOfEyeFactorVue = this.GetColumnIndexByName(e.Row, "EyeFactorVue");
        int indexOfShallowFactorVue = this.GetColumnIndexByName(e.Row, "ShallowFactorVue");
        int indexOfBackgroundRateVue = this.GetColumnIndexByName(e.Row, "BackgroundRateVue");
        int indexOfReadLimit = this.GetColumnIndexByName(e.Row, "ReadLimit");

        // MRD Vue
        if (locationFactors[e.Row.DataItemIndex].MRDVueFrom == 'L')
            e.Row.Cells[indexOfMRDVue].CssClass = "AlteredField";

        // MRD Increment Vue
        if (locationFactors[e.Row.DataItemIndex].MRDIncrementVueFrom == 'L')
            e.Row.Cells[indexOfMRDIncrementVue].CssClass = "AlteredField";

        // Deep Factor Vue
        if (locationFactors[e.Row.DataItemIndex].DeepFactorVueFrom == 'L')
            e.Row.Cells[indexOfDeepFactorVue].CssClass = "AlteredField";

        // Eye Factor Vue
        if (locationFactors[e.Row.DataItemIndex].EyeFactorVueFrom == 'L')
            e.Row.Cells[indexOfEyeFactorVue].CssClass = "AlteredField";

        // Shallow Factor Vue
        if (locationFactors[e.Row.DataItemIndex].ShallowFactorVueFrom == 'L')
            e.Row.Cells[indexOfShallowFactorVue].CssClass = "AlteredField";

        // Background Rate Vue
        if (locationFactors[e.Row.DataItemIndex].BackgroundRateVueFrom == 'L')
            e.Row.Cells[indexOfBackgroundRateVue].CssClass = "AlteredField";

        // Read Limit
        if (locationFactors[e.Row.DataItemIndex].ReadLimitFrom == 'L')
            e.Row.Cells[indexOfReadLimit].CssClass = "AlteredField";

    }

    /// <summary>
    /// Maintains GridView/DataGrid while paging.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void gvLocationSettings_ID1_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        this.GetLocationLevelSettings(this.accountID);
        this.gvLocationSettings_ID1.PageIndex = e.NewPageIndex;
        this.gvLocationSettings_ID1.DataBind();
    }

    protected void gvLocationSettings_ID2_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        this.GetLocationLevelSettings(this.accountID);
        this.gvLocationSettings_ID2.PageIndex = e.NewPageIndex;
        this.gvLocationSettings_ID2.DataBind();
    }

    protected void gvLocationSettings_ID2Elite_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        this.GetLocationLevelSettings(this.accountID);
        this.gvLocationSettings_ID2Elite.PageIndex = e.NewPageIndex;
        this.gvLocationSettings_ID2Elite.DataBind();
    }

    protected void gvLocationSettings_IDPlus_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        this.GetLocationLevelSettings(this.accountID);
        this.gvLocationSettings_IDPlus.PageIndex = e.NewPageIndex;
        this.gvLocationSettings_IDPlus.DataBind();
    }

    protected void gvLocationSettings_IDVue_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        this.GetLocationLevelSettings(this.accountID);
        this.gvLocationSettings_IDVue.PageIndex = e.NewPageIndex;
        this.gvLocationSettings_IDVue.DataBind();
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

        // Get current DisplayFactorValues
        SettingFactors curDisplayFactorValues = new SettingFactors();

        if (Session["CurrentDisplayFactorValues"] != null)
            curDisplayFactorValues = (SettingFactors)(Session["CurrentDisplayFactorValues"]);

        // -------------- Get prior settings values ----------------------//
        int? mrd_before = loc.MRD;
        int? mrdIncrement_before = loc.MRDIncrement;
        double? deepFactor_before = loc.DeepFactor;
        double? eyeFactor_before = loc.EyeFactor;
        double? shallowFactor_before = loc.ShallowFactor;
        double? backgroundRate_before = loc.BackgroundRate;

        int? mrdPlus_before = loc.MRDPlus;
        int? mrdIncrementPlus_before = loc.MRDIncrementPlus;
        double? deepFactorPlus_before = loc.DeepFactorPlus;
        double? eyeFactorPlus_before = loc.EyeFactorPlus;
        double? shallowFactorPlus_before = loc.ShallowFactorPlus;
        double? backgroundRatePlus_before = loc.BackgroundRatePlus;

        int? mrd2_before = loc.MRD2;
        int? mrdIncrement2_before = loc.MRDIncrement2;
        double? backgroundRate2_before = loc.BackgroundRate2;
        string algorithmFactor_before = loc.AlgorithmFactor;

        int? mrd2New_before = loc.MRD2New;
        int? mrdIncrement2New_before = loc.MRDIncrement2New;
        double? backgroundRate2New_before = loc.BackgroundRate2New;


        int? mrdVue_before = loc.MRDVue;
        int? mrdIncrementVue_before = loc.MRDIncrementVue;
        double? eyeFactorVue_before = loc.EyeFactorVue;
        double? shallowFactorVue_before = loc.ShallowFactorVue;
        double? backgroundRateVue_before = loc.BackgroundRateVue;
        double? deepFactorVue_before = loc.DeepFactorVue;

        double? readLimit_before = loc.ReadLimit;

        // ----------------------------------------------------------------- //

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
        string algorithmFactor = null;

        int? mrd2New = null;
        int? mrdIncrement2New = null;
        double? backgroundRate2New = null;

        int? mrdVueNew = null;
        int? mrdIncrementVueNew = null;
        double? eyeFactorVueNew = null;
        double? shallowFactorVueNew = null;
        double? backgroundRateVueNew = null;
        double? deepFactorVueNew = null;

        int? readLimit = null;

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
        if (this.txtAlgorithmFactor_Location.Text.Trim() != "") algorithmFactor = this.txtAlgorithmFactor_Location.Text.Trim().ToUpper();

        if (this.txtMRD2New_Location.Text.Trim() != "") mrd2New = Convert.ToInt32(this.txtMRD2New_Location.Text.Trim());
        if (this.txtMRDIncrement2New_Location.Text.Trim() != "") mrdIncrement2New = Convert.ToInt32(this.txtMRDIncrement2New_Location.Text.Trim());
        if (this.txtBackgroundRate2New_Location.Text.Trim() != "") backgroundRate2New = Convert.ToDouble(this.txtBackgroundRate2New_Location.Text.Trim());

        if (this.txtMRDVue_Location.Text.Trim() != "") mrdVueNew = Convert.ToInt32(this.txtMRDVue_Location.Text.Trim());
        if (this.txtMRDIncrementVue_Location.Text.Trim() != "") mrdIncrementVueNew = Convert.ToInt32(this.txtMRDIncrementVue_Location.Text.Trim());
        if (this.txtEyeFactorVue_Location.Text.Trim() != "") eyeFactorVueNew = Convert.ToDouble(this.txtEyeFactorVue_Location.Text.Trim());
        if (this.txtShallowFactorVue_Location.Text.Trim() != "") shallowFactorVueNew = Convert.ToDouble(this.txtShallowFactorVue_Location.Text.Trim());
        if (this.txtBackgroundRateVue_Location.Text.Trim() != "") backgroundRateVueNew = Convert.ToDouble(this.txtBackgroundRateVue_Location.Text.Trim());
        if (this.txtDeepFactorVue_Location.Text.Trim() != "") deepFactorVueNew = Convert.ToDouble(this.txtDeepFactorVue_Location.Text.Trim());

        if (this.txtReadLimit_Location.Text.Trim() != "") readLimit = Convert.ToInt32(this.txtReadLimit_Location.Text.Trim());

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
        if (algorithmFactor != null) loc.AlgorithmFactor = algorithmFactor;
        else loc.AlgorithmFactor = null;

        if (mrd2New.HasValue) loc.MRD2New = mrd2New.Value;
        else loc.MRD2New = null;
        if (mrdIncrement2New.HasValue) loc.MRDIncrement2New = mrdIncrement2New.Value;
        else loc.MRDIncrement2New = null;
        if (backgroundRate2New.HasValue) loc.BackgroundRate2New = backgroundRate2New.Value;
        else loc.BackgroundRate2New = null;

        if (readLimit.HasValue) loc.ReadLimit = readLimit.Value;
        else loc.ReadLimit = null;

        if (mrdVueNew.HasValue) loc.MRDVue = mrdVueNew.Value;
        else loc.MRDVue = null;
        if (mrdIncrementVueNew.HasValue) loc.MRDIncrementVue = mrdIncrementVueNew.Value;
        else loc.MRDIncrementVue = null;
        if (eyeFactorVueNew.HasValue) loc.EyeFactorVue = eyeFactorVueNew.Value;
        else loc.EyeFactorVue = null;
        if (shallowFactorVueNew.HasValue) loc.ShallowFactorVue = shallowFactorVueNew.Value;
        else loc.ShallowFactorVue = null;
        if (backgroundRateVueNew.HasValue) loc.BackgroundRateVue = backgroundRateVueNew.Value;
        else loc.BackgroundRateVue = null;
        if (deepFactorVueNew.HasValue) loc.DeepFactorVue = deepFactorVueNew.Value;
        else loc.DeepFactorVue = null;

        try
        {
            // Update Location values where applicable.
            idc.SubmitChanges();

            // --------------------------------------------------------------- Save Factors History Logs ---------------------------------------------------------------- //

            // ---------------- Get all global and account settings to pass in Adj Values to Factors History Log in case we remove the location level settings ---------- //
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
            string acctAlgorithmFactor = null;

            int? acctMRD2New = null;
            int? acctMRDIncrement2New = null;
            double? acctBackgroundRate2New = null;

            double? acctReadLimit = null;

            int? acctMRDVue = null;
            int? acctMRDIncrementVue = null;
            double? acctEyeFactorVue = null;
            double? acctShallowFactorVue = null;
            double? acctbackgroundRateVue = null;
            double? acctdeepfactorVue = null;

            // ------------------------------------ AccountSettings ----------------------------------------------------------------------------------------------------- //
            this.GetAccountSettings(this.accountID, out acctCompanyName, out acctMRD, out acctMRDIncrement, out acctMRD2, out acctMRDIncrement2, out acctDeepFactor, out acctEyeFactor, out acctShallowFactor, out acctBackgroundRate, out acctBackgroundRate2, out acctReadLimit, out acctAlgorithmFactor, out acctMRDPlus, out acctMRDIncrementPlus, out acctDeepFactorPlus, out acctEyeFactorPlus, out acctShallowFactorPlus, out acctBackgroundRatePlus, out acctMRD2New, out acctMRDIncrement2New, out acctBackgroundRate2New, out acctMRDVue, out acctbackgroundRateVue, out acctdeepfactorVue, out acctMRDIncrementVue, out acctEyeFactorVue, out acctShallowFactorVue);
            // ---------------------------------------------------------------------------------------------------------------------------------------------------------- //

            // ------------------------------------ AppSettings.--------------------------------------------------------------------------------------------------------- //
            string appMRD = Basics.GetSetting("MRD");
            string appMRDIncrement = Basics.GetSetting("MRDIncrement");

            string appMRDPlus = Basics.GetSetting("MRDPlus");
            string appMRDIncrementPlus = Basics.GetSetting("MRDIncrementPlus");

            string appMRD2 = Basics.GetSetting("MRDID2");
            string appMRDIncrement2 = Basics.GetSetting("MRDIncrementID2");
            string appAlgorithmFactor = Basics.GetSetting("AlgorithmFactorID2");

            string appMRD2New = Basics.GetSetting("MRDID2New");
            string appMRDIncrement2New = Basics.GetSetting("MRDIncrementID2New");

            string appReadLimit = Basics.GetSetting("ReadLimit");

            string appMRDVue = Basics.GetSetting("MRDIDVue");
            string appMRDIncrementVue = Basics.GetSetting("MRDIncrementIDVue");
            string appBackgroundRateVue = Basics.GetSetting("ID VUE Background");

            // DeviceFirmware.
            double dfDeepFactor = 0;
            double dfEyeFactor = 0;
            double dfShallowFactor = 0;
            this.GetDeviceFirmware(out dfDeepFactor, out dfEyeFactor, out dfShallowFactor);

            // Control Badge, State & Country.
            double? stateBackgroundRate = null;
            double? countryBackgroundRate = null;
            double? actControlBackgroundRate = null;
            double? locControlBackgroundRate = null;

            double? dfbackgroundRateVue = null;

            double dfDeepFactorVue = 1;
            double dfEyeFactorVue = 1;
            double dfShallowFactorVue = 1;

            this.GetStateAndCountry(accountID, out stateBackgroundRate, out countryBackgroundRate);

            var act_cbrDLObj = (from cbr in idc.ControlBadgeReads
                                where
                                cbr.AccountID == this.accountID
                                && !cbr.LocationID.HasValue
                                && cbr.AvgDLRate.HasValue
                                && cbr.PermEliminatedDL == false
                                && cbr.IsEliminatedDL == false
                                && cbr.ReadTypeID == 17
                                orderby cbr.DoseDate descending
                                select cbr).FirstOrDefault();
            if (act_cbrDLObj != null) actControlBackgroundRate = act_cbrDLObj.AvgDLRate.Value;

            var loc_cbrDLObj = (from cbr in idc.ControlBadgeReads
                                where
                                cbr.AccountID == this.accountID
                                && cbr.LocationID == locationID
                                && cbr.AvgDLRate.HasValue
                                && cbr.PermEliminatedDL == false
                                && cbr.IsEliminatedDL == false
                                && cbr.ReadTypeID == 17
                                orderby cbr.DoseDate descending
                                select cbr).FirstOrDefault();
            if (loc_cbrDLObj != null) locControlBackgroundRate = loc_cbrDLObj.AvgDLRate.Value;

            string backgroundRate_Str = acctBackgroundRate.HasValue ? acctBackgroundRate.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
            string backgroundRatePlus_Str = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctBackgroundRatePlus.HasValue ? acctBackgroundRatePlus.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
            string backgroundRate2_Str = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctBackgroundRate2.HasValue ? acctBackgroundRate2.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
            string backgroundRate2New_Str = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctBackgroundRate2New.HasValue ? acctBackgroundRate2New.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
            string backgroundRateVue_Str = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctbackgroundRateVue.HasValue ? acctbackgroundRateVue.Value.ToString() : appBackgroundRateVue;
            //string backgroundRateVue_Str = locControlBackgroundRate.HasValue ? locControlBackgroundRate.Value.ToString() : actControlBackgroundRate.HasValue ? actControlBackgroundRate.Value.ToString() : acctbackgroundRateVue.HasValue ? acctbackgroundRateVue.Value.ToString() : stateBackgroundRate.HasValue ? stateBackgroundRate.Value.ToString() : countryBackgroundRate.ToString();
            // ----------------------------------------------------------------------------------------------------------------//

            // Insert any ID1 change
            if (loc.MRD != mrd_before)
                //InsertFactorHistoryLog(this.accountID, locationID, "ID1", "MRD", mrd_before.HasValue ? mrd_before.Value.ToString() : "", loc.MRD.HasValue ? loc.MRD.Value.ToString() : "", this.txtNotes_Location.Text.Trim());
                InsertFactorHistoryLog(this.accountID, locationID, "ID1", "MRD", curDisplayFactorValues != null ? curDisplayFactorValues.mrd : "", loc.MRD.HasValue ? loc.MRD.Value.ToString() : (acctMRD.HasValue ? acctMRD.Value.ToString() : appMRD.ToString()), this.txtNotes_Location.Text.Trim());

            if (loc.MRDIncrement != mrdIncrement_before)
                //InsertFactorHistoryLog(this.accountID, locationID, "ID1", "MRDIncrement", mrdIncrement_before.HasValue ? mrdIncrement_before.Value.ToString() : "", loc.MRDIncrement.HasValue ? loc.MRDIncrement.Value.ToString() : "", this.txtNotes_Location.Text.Trim());
                InsertFactorHistoryLog(this.accountID, locationID, "ID1", "MRDIncrement", curDisplayFactorValues != null ? curDisplayFactorValues.mrdIncrement : "", loc.MRDIncrement.HasValue ? loc.MRDIncrement.Value.ToString() : (acctMRDIncrement.HasValue ? acctMRDIncrement.Value.ToString() : appMRDIncrement.ToString()), this.txtNotes_Location.Text.Trim());

            if (loc.DeepFactor != deepFactor_before)
                //InsertFactorHistoryLog(this.accountID, locationID, "ID1", "DeepFactor", deepFactor_before.HasValue ? deepFactor_before.Value.ToString() : "", loc.DeepFactor.HasValue ? loc.DeepFactor.Value.ToString() : "", this.txtNotes_Location.Text.Trim());
                InsertFactorHistoryLog(this.accountID, locationID, "ID1", "DeepFactor", curDisplayFactorValues != null ? curDisplayFactorValues.deepFactor : "", loc.DeepFactor.HasValue ? loc.DeepFactor.Value.ToString() : (acctDeepFactor.HasValue ? acctDeepFactor.Value.ToString() : dfDeepFactor.ToString()), this.txtNotes_Location.Text.Trim());

            if (loc.EyeFactor != eyeFactor_before)
                //InsertFactorHistoryLog(this.accountID, locationID, "ID1", "EyeFactor", eyeFactor_before.HasValue ? eyeFactor_before.Value.ToString() : "", loc.EyeFactor.HasValue ? loc.EyeFactor.Value.ToString() : "", this.txtNotes_Location.Text.Trim());
                InsertFactorHistoryLog(this.accountID, locationID, "ID1", "EyeFactor", curDisplayFactorValues != null ? curDisplayFactorValues.eyeFactor : "", loc.EyeFactor.HasValue ? loc.EyeFactor.Value.ToString() : (acctEyeFactor.HasValue ? acctEyeFactor.Value.ToString() : dfEyeFactor.ToString()), this.txtNotes_Location.Text.Trim());

            if (loc.ShallowFactor != shallowFactor_before)
                //InsertFactorHistoryLog(this.accountID, locationID, "ID1", "ShallowFactor", shallowFactor_before.HasValue ? shallowFactor_before.Value.ToString() : "", loc.ShallowFactor.HasValue ? loc.ShallowFactor.Value.ToString() : "", this.txtNotes_Location.Text.Trim());
                InsertFactorHistoryLog(this.accountID, locationID, "ID1", "ShallowFactor", curDisplayFactorValues != null ? curDisplayFactorValues.shallowFactor : "", loc.ShallowFactor.HasValue ? loc.ShallowFactor.Value.ToString() : (acctShallowFactor.HasValue ? acctShallowFactor.Value.ToString() : dfShallowFactor.ToString()), this.txtNotes_Location.Text.Trim());

            if (loc.BackgroundRate != backgroundRate_before)
                //InsertFactorHistoryLog(this.accountID, locationID, "ID1", "BackgroundRate", backgroundRate_before.HasValue ? backgroundRate_before.Value.ToString() : "", loc.BackgroundRate.HasValue ? loc.BackgroundRate.Value.ToString() : "", this.txtNotes_Location.Text.Trim());
                InsertFactorHistoryLog(this.accountID, locationID, "ID1", "BackgroundRate", curDisplayFactorValues != null ? curDisplayFactorValues.backgroundRate : "", loc.BackgroundRate.HasValue ? loc.BackgroundRate.Value.ToString() : backgroundRate_Str, this.txtNotes_Location.Text.Trim());

            // Insert any ID+ change
            if (loc.MRDPlus != mrdPlus_before)
                //InsertFactorHistoryLog(this.accountID, locationID, "ID+", "MRDPlus", mrdPlus_before.HasValue ? mrdPlus_before.Value.ToString() : "", loc.MRDPlus.HasValue ? loc.MRDPlus.Value.ToString() : "", this.txtNotes_Location.Text.Trim());
                InsertFactorHistoryLog(this.accountID, locationID, "ID+", "MRD", curDisplayFactorValues != null ? curDisplayFactorValues.mrdPlus : "", loc.MRDPlus.HasValue ? loc.MRDPlus.Value.ToString() : (acctMRDPlus.HasValue ? acctMRDPlus.Value.ToString() : appMRDPlus.ToString()), this.txtNotes_Location.Text.Trim());

            if (loc.MRDIncrementPlus != mrdIncrementPlus_before)
                //InsertFactorHistoryLog(this.accountID, locationID, "ID+", "MRDIncrementPlus", mrdIncrementPlus_before.HasValue ? mrdIncrementPlus_before.Value.ToString() : "", loc.MRDIncrementPlus.HasValue ? loc.MRDIncrementPlus.Value.ToString() : "", this.txtNotes_Location.Text.Trim());
                InsertFactorHistoryLog(this.accountID, locationID, "ID+", "MRDIncrement", curDisplayFactorValues != null ? curDisplayFactorValues.mrdIncrementPlus : "", loc.MRDIncrementPlus.HasValue ? loc.MRDIncrementPlus.Value.ToString() : (acctMRDIncrementPlus.HasValue ? acctMRDIncrementPlus.Value.ToString() : appMRDIncrementPlus.ToString()), this.txtNotes_Location.Text.Trim());

            if (loc.DeepFactorPlus != deepFactorPlus_before)
                //InsertFactorHistoryLog(this.accountID, locationID, "ID+", "DeepFactorPlus", deepFactorPlus_before.HasValue ? deepFactorPlus_before.Value.ToString() : "", loc.DeepFactorPlus.HasValue ? loc.DeepFactorPlus.Value.ToString() : "", this.txtNotes_Location.Text.Trim());
                InsertFactorHistoryLog(this.accountID, locationID, "ID+", "DeepFactor", curDisplayFactorValues != null ? curDisplayFactorValues.deepFactorPlus : "", loc.DeepFactorPlus.HasValue ? loc.DeepFactorPlus.Value.ToString() : (acctDeepFactorPlus.HasValue ? acctDeepFactorPlus.Value.ToString() : dfDeepFactor.ToString()), this.txtNotes_Location.Text.Trim());

            if (loc.EyeFactorPlus != eyeFactorPlus_before)
                //InsertFactorHistoryLog(this.accountID, locationID, "ID+", "EyeFactorPlus", eyeFactorPlus_before.HasValue ? eyeFactorPlus_before.Value.ToString() : "", loc.EyeFactorPlus.HasValue ? loc.EyeFactorPlus.Value.ToString() : "", this.txtNotes_Location.Text.Trim());
                InsertFactorHistoryLog(this.accountID, locationID, "ID+", "EyeFactor", curDisplayFactorValues != null ? curDisplayFactorValues.eyeFactorPlus : "", loc.EyeFactorPlus.HasValue ? loc.EyeFactorPlus.Value.ToString() : (acctEyeFactorPlus.HasValue ? acctEyeFactorPlus.Value.ToString() : dfEyeFactor.ToString()), this.txtNotes_Location.Text.Trim());

            if (loc.ShallowFactorPlus != shallowFactorPlus_before)
                //InsertFactorHistoryLog(this.accountID, locationID, "ID+", "ShallowFactorPlus", shallowFactorPlus_before.HasValue ? shallowFactorPlus_before.Value.ToString() : "", loc.ShallowFactorPlus.HasValue ? loc.ShallowFactorPlus.Value.ToString() : "", this.txtNotes_Location.Text.Trim());
                InsertFactorHistoryLog(this.accountID, locationID, "ID+", "ShallowFactor", curDisplayFactorValues != null ? curDisplayFactorValues.shallowFactorPlus : "", loc.ShallowFactorPlus.HasValue ? loc.ShallowFactorPlus.Value.ToString() : (acctShallowFactorPlus.HasValue ? acctShallowFactorPlus.Value.ToString() : dfShallowFactor.ToString()), this.txtNotes_Location.Text.Trim());

            if (loc.BackgroundRatePlus != backgroundRatePlus_before)
                //InsertFactorHistoryLog(this.accountID, locationID, "ID+", "BackgroundRatePlus", backgroundRatePlus_before.HasValue ? backgroundRatePlus_before.Value.ToString() : "", loc.BackgroundRatePlus.HasValue ? loc.BackgroundRatePlus.Value.ToString() : "", this.txtNotes_Location.Text.Trim());
                InsertFactorHistoryLog(this.accountID, locationID, "ID+", "BackgroundRate", curDisplayFactorValues != null ? curDisplayFactorValues.backgroundRatePlus : "", loc.BackgroundRatePlus.HasValue ? loc.BackgroundRatePlus.Value.ToString() : backgroundRatePlus_Str, this.txtNotes_Location.Text.Trim());

            // Insert any ID2Elite change
            if (loc.MRD2 != mrd2_before)
                //InsertFactorHistoryLog(this.accountID, locationID, "IDElite", "MRD2", mrd2_before.HasValue ? mrd2_before.Value.ToString() : "", loc.MRD2.HasValue ? loc.MRD2.Value.ToString() : "", this.txtNotes_Location.Text.Trim());
                InsertFactorHistoryLog(this.accountID, locationID, "IDElite", "MRD", curDisplayFactorValues != null ? curDisplayFactorValues.mrd2 : "", loc.MRD2.HasValue ? loc.MRD2.Value.ToString() : (acctMRD2.HasValue ? acctMRD2.Value.ToString() : appMRD2.ToString()), this.txtNotes_Location.Text.Trim());

            if (loc.MRDIncrement2 != mrdIncrement2_before)
                //InsertFactorHistoryLog(this.accountID, locationID, "IDElite", "MRDIncrement2", mrdIncrement2_before.HasValue ? mrdIncrement2_before.Value.ToString() : "", loc.MRDIncrement2.HasValue ? loc.MRDIncrement2.Value.ToString() : "", this.txtNotes_Location.Text.Trim());
                InsertFactorHistoryLog(this.accountID, locationID, "IDElite", "MRDIncrement", curDisplayFactorValues != null ? curDisplayFactorValues.mrdIncrement2 : "", loc.MRDIncrement2.HasValue ? loc.MRDIncrement2.Value.ToString() : (acctMRDIncrement2.HasValue ? acctMRDIncrement2.Value.ToString() : appMRDIncrement2.ToString()), this.txtNotes_Location.Text.Trim());

            if (loc.BackgroundRate2 != backgroundRate2_before)
                //InsertFactorHistoryLog(this.accountID, locationID, "IDElite", "BackgroundRate2", backgroundRate2_before.HasValue ? backgroundRate2_before.Value.ToString() : "", loc.BackgroundRate2.HasValue ? loc.BackgroundRate2.Value.ToString() : "", this.txtNotes_Location.Text.Trim());
                InsertFactorHistoryLog(this.accountID, locationID, "IDElite", "BackgroundRate", curDisplayFactorValues != null ? curDisplayFactorValues.backgroundRate2 : "", loc.BackgroundRate2.HasValue ? loc.BackgroundRate2.Value.ToString() : backgroundRate2_Str, this.txtNotes_Location.Text.Trim());

            if (loc.AlgorithmFactor != algorithmFactor_before)
                //InsertFactorHistoryLog(this.accountID, locationID, "IDElite", "AlgorithmPath", algorithmFactor_before, loc.AlgorithmFactor, this.txtNotes_Location.Text.Trim());
                InsertFactorHistoryLog(this.accountID, locationID, "IDElite", "AlgorithmPath", curDisplayFactorValues != null ? curDisplayFactorValues.algorithmFactor : "", loc.AlgorithmFactor ?? (acctAlgorithmFactor ?? appAlgorithmFactor), this.txtNotes_Location.Text.Trim());

            // Insert any ID2 New change
            if (loc.MRD2New != mrd2New_before)
                //InsertFactorHistoryLog(this.accountID, locationID, "ID2", "MRD2New", mrd2New_before.HasValue ? mrd2New_before.Value.ToString() : "", loc.MRD2New.HasValue ? loc.MRD2New.Value.ToString() : "", this.txtNotes_Location.Text.Trim());
                InsertFactorHistoryLog(this.accountID, locationID, "ID2", "MRD", curDisplayFactorValues != null ? curDisplayFactorValues.mrd2New : "", loc.MRD2New.HasValue ? loc.MRD2New.Value.ToString() : (acctMRD2New.HasValue ? acctMRD2New.Value.ToString() : appMRD2New.ToString()), this.txtNotes_Location.Text.Trim());

            if (loc.MRDIncrement2New != mrdIncrement2New_before)
                //InsertFactorHistoryLog(this.accountID, locationID, "ID2", "MRDIncrement2New", mrdIncrement2New_before.HasValue ? mrdIncrement2New_before.Value.ToString() : "", loc.MRDIncrement2New.HasValue ? loc.MRDIncrement2New.Value.ToString() : "", this.txtNotes_Location.Text.Trim());
                InsertFactorHistoryLog(this.accountID, locationID, "ID2", "MRDIncrement", curDisplayFactorValues != null ? curDisplayFactorValues.mrdIncrement2New : "", loc.MRDIncrement2New.HasValue ? loc.MRDIncrement2New.Value.ToString() : (acctMRDIncrement2New.HasValue ? acctMRDIncrement2New.Value.ToString() : appMRDIncrement2New.ToString()), this.txtNotes_Location.Text.Trim());

            if (loc.BackgroundRate2New != backgroundRate2New_before)
                //InsertFactorHistoryLog(this.accountID, locationID, "ID2", "BackgroundRate2New", backgroundRate2New_before.HasValue ? backgroundRate2New_before.Value.ToString() : "", loc.BackgroundRate2New.HasValue ? loc.BackgroundRate2New.Value.ToString() : "", this.txtNotes_Location.Text.Trim());
                InsertFactorHistoryLog(this.accountID, locationID, "ID2", "BackgroundRate", curDisplayFactorValues != null ? curDisplayFactorValues.backgroundRate2New : "", loc.BackgroundRate2New.HasValue ? loc.BackgroundRate2New.Value.ToString() : backgroundRate2New_Str, this.txtNotes_Location.Text.Trim());

            // Insert Read Limit change
            if (loc.ReadLimit != readLimit_before)
                //InsertFactorHistoryLog(this.accountID, locationID, "All", "ReadLimit", readLimit_before.HasValue ? readLimit_before.Value.ToString() : "", loc.ReadLimit.HasValue ? loc.ReadLimit.Value.ToString() : "", this.txtNotes_Location.Text.Trim());
                InsertFactorHistoryLog(this.accountID, locationID, "All", "ReadLimit", curDisplayFactorValues != null ? curDisplayFactorValues.readLimit : "", loc.ReadLimit.HasValue ? loc.ReadLimit.Value.ToString() : (acctReadLimit.HasValue ? acctReadLimit.Value.ToString() : appReadLimit.ToString()), this.txtNotes_Location.Text.Trim());

            // MRD Vue
            if (loc.MRDVue != mrdVue_before)
                InsertFactorHistoryLog(this.accountID, locationID, "IDVue", "MRDVue", curDisplayFactorValues != null ? curDisplayFactorValues.mrdVue : "", loc.MRDVue.HasValue ? loc.MRDVue.Value.ToString() : (acctMRDVue.HasValue ? acctReadLimit.Value.ToString() : appMRDVue.ToString()), this.txtNotes_Location.Text.Trim());

            if (loc.MRDIncrementVue != mrdIncrementVue_before)
                //InsertFactorHistoryLog(this.accountID, locationID, "ID+", "MRDIncrementPlus", mrdIncrementPlus_before.HasValue ? mrdIncrementPlus_before.Value.ToString() : "", loc.MRDIncrementPlus.HasValue ? loc.MRDIncrementPlus.Value.ToString() : "", this.txtNotes_Location.Text.Trim());
                InsertFactorHistoryLog(this.accountID, locationID, "IDVue", "MRDIncrementVue", curDisplayFactorValues != null ? curDisplayFactorValues.mrdIncrementVue : "", loc.MRDIncrementVue.HasValue ? loc.MRDIncrementVue.Value.ToString() : (acctMRDIncrementVue.HasValue ? acctMRDIncrementVue.Value.ToString() : appMRDIncrementVue.ToString()), this.txtNotes_Location.Text.Trim());

            if (loc.EyeFactorVue != eyeFactorVue_before)
                //InsertFactorHistoryLog(this.accountID, locationID, "ID+", "EyeFactorPlus", eyeFactorPlus_before.HasValue ? eyeFactorPlus_before.Value.ToString() : "", loc.EyeFactorPlus.HasValue ? loc.EyeFactorPlus.Value.ToString() : "", this.txtNotes_Location.Text.Trim());
                InsertFactorHistoryLog(this.accountID, locationID, "IDVue", "EyeFactorVue", curDisplayFactorValues != null ? curDisplayFactorValues.eyeFactorVue : "", loc.EyeFactorVue.HasValue ? loc.EyeFactorVue.Value.ToString() : (acctEyeFactorVue.HasValue ? acctEyeFactorVue.Value.ToString() : dfEyeFactorVue.ToString()), this.txtNotes_Location.Text.Trim());

            if (loc.ShallowFactorVue != shallowFactorVue_before)
                //InsertFactorHistoryLog(this.accountID, locationID, "ID+", "ShallowFactorPlus", shallowFactorPlus_before.HasValue ? shallowFactorPlus_before.Value.ToString() : "", loc.ShallowFactorPlus.HasValue ? loc.ShallowFactorPlus.Value.ToString() : "", this.txtNotes_Location.Text.Trim());
                InsertFactorHistoryLog(this.accountID, locationID, "IDVue", "ShallowFactorVue", curDisplayFactorValues != null ? curDisplayFactorValues.shallowFactorVue : "", loc.ShallowFactorVue.HasValue ? loc.ShallowFactorVue.Value.ToString() : (acctShallowFactorVue.HasValue ? acctShallowFactorVue.Value.ToString() : dfShallowFactorVue.ToString()), this.txtNotes_Location.Text.Trim());

            if (loc.BackgroundRateVue != backgroundRateVue_before)
                InsertFactorHistoryLog(this.accountID, locationID, "IDVue", "BackgroundRateVue", curDisplayFactorValues != null ? curDisplayFactorValues.backgroundRateVue : "", loc.BackgroundRateVue.HasValue ? loc.BackgroundRateVue.Value.ToString() : backgroundRateVue_Str, this.txtNotes_Location.Text.Trim());

            if (loc.DeepFactorVue != deepFactorVue_before)
                InsertFactorHistoryLog(this.accountID, locationID, "IDVue", "DeepFactorVue", curDisplayFactorValues != null ? curDisplayFactorValues.deepfactorVue : "", loc.DeepFactorVue.HasValue ? loc.DeepFactorVue.Value.ToString() : (acctdeepfactorVue.HasValue ? acctdeepfactorVue.Value.ToString() : dfDeepFactorVue.ToString()), this.txtNotes_Location.Text.Trim());
            // --------------------------------------------------------------------------- //

            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divLocationSettingsDialog')", true);

            this.VisibleSuccesses(string.Format("Settings for {0} were updated.", locationName));

            // Rebind GridView to display updated Inheritance values (where applicable).
            this.GetLocationLevelSettings(this.accountID);
        }
        catch
        {
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divLocationSettingsDialog')", true);
            this.VisibleErrors(string.Format("An error occurred while updating {0}.", locationName));
            return;
        }
    }
    #endregion

    #region PRINT FACTOR HISTORY LOG DIALOG
    protected void btnFactorsHistoryPrint_Click(object sender, EventArgs e)
    {
        try
        {
            string url = "FactorsHistoryLogReport.aspx?AccountID=" + this.accountID.ToString();
            ScriptManager.RegisterStartupScript(this, typeof(string), "OPEN_WINDOW", "window.open( '" + url + "', 'FactorsHistoryLogReport', 'height=600,width=800,status=yes,toolbar=yes,menubar=no,location=no,resize=yes' );", true);
        }
        catch
        {
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divFactorsHistoryDialog')", true);
            this.VisibleErrors(string.Format("An error occurred while printing factors history log of account: {0}.", this.account));
            return;
        }
    }
    protected void lnkbtnFactorHistoryLog_Click(object sender, EventArgs e)
    {
        //Response.Redirect("FactorsHistory.aspx?AccountID=" + this.accountID );
        this.lblAccountID.Text = this.account;

        var factorsHistoryLog = (from fhl in idc.FactorHistoryLogs
                                 join l in idc.Locations on fhl.LocationID equals l.LocationID into Joined_fhl_l
                                 where fhl.AccountID == this.accountID
                                 orderby fhl.AddDate descending
                                 from fhl_l in Joined_fhl_l.DefaultIfEmpty()
                                 select new
                                 {
                                     Date = fhl.AddDate.ToShortDateString(),
                                     Time = fhl.AddDate.ToShortTimeString(),
                                     Name = fhl.AddUser,
                                     ProductType = fhl.ProductType,
                                     FactorField = fhl.FactorName,
                                     PrevValue = fhl.PrevValue ?? "",
                                     AdjValue = fhl.AdjValue ?? "",
                                     LocationName = fhl_l.LocationName ?? "",
                                     Note = fhl.Note
                                 }).ToArray();

        gv_FactorHistoryLog.DataSource = factorsHistoryLog;
        gv_FactorHistoryLog.DataBind();

        // Send Javascript to client to OPEN/DISPLAY the modal dialog.
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('divFactorsHistoryDialog')", true);
    }
    protected void gv_FactorHistoryLog_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            if (e.Row.RowIndex == 0)
            {
                e.Row.Style.Add("height", "60px");
                e.Row.VerticalAlign = VerticalAlign.Bottom;
            }
        }
    }
    #endregion

    private bool InsertFactorHistoryLog(int pAccountID, int? PLocationID, string pProductType, string pFactorName, string pPrevValue, string pAdjValue, string pNote)
    {
        try
        {
            FactorHistoryLog fhl = new FactorHistoryLog
            {
                AccountID = pAccountID,
                LocationID = PLocationID,
                AddUser = this.userName,
                AddDate = System.DateTime.Now,
                ProductType = pProductType,
                FactorName = pFactorName,
                PrevValue = string.IsNullOrEmpty(pPrevValue) ? "" : pPrevValue,
                AdjValue = string.IsNullOrEmpty(pAdjValue) ? "" : pAdjValue,
                Note = string.IsNullOrEmpty(pNote) ? null : pNote
            };

            idc.FactorHistoryLogs.InsertOnSubmit(fhl);
            idc.SubmitChanges();

            return true;
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    protected void btnBackToFactorsSearch_Click(object sender, EventArgs e)
    {
        Response.Redirect("FactorsSearch.aspx");
    }
}

public class SettingFactors
{
    public string mrd { get; set; }
    public string mrdIncrement { get; set; }
    public string deepFactor { get; set; }
    public string eyeFactor { get; set; }
    public string shallowFactor { get; set; }
    public string backgroundRate { get; set; }

    public string mrdPlus { get; set; }
    public string mrdIncrementPlus { get; set; }
    public string deepFactorPlus { get; set; }
    public string eyeFactorPlus { get; set; }
    public string shallowFactorPlus { get; set; }
    public string backgroundRatePlus { get; set; }

    public string mrd2 { get; set; }
    public string mrdIncrement2 { get; set; }
    public string backgroundRate2 { get; set; }
    public string algorithmFactor { get; set; }

    public string mrd2New { get; set; }
    public string mrdIncrement2New { get; set; }
    public string backgroundRate2New { get; set; }

    public string readLimit { get; set; }

    public string mrdVue { get; set; }
    public string mrdIncrementVue { get; set; }
    public string eyeFactorVue { get; set; }
    public string shallowFactorVue { get; set; }
    public string backgroundRateVue { get; set; }
    public string deepfactorVue { get; set; }

}