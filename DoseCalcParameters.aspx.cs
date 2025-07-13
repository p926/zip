using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Text.RegularExpressions;

using Instadose;
using Instadose.Data;

public partial class TechOps_DoseCalcParameters : System.Web.UI.Page
{
    // Create the database reference
    InsDataContext idc = new InsDataContext();
    AppDataContext adc = new AppDataContext();
    MASDataContext mdc = new MASDataContext();

    public string UserName = "Unknown";
    public string[] ActiveDirecoryGroups = { "IRV-IT" };

    protected void Page_Load(object sender, EventArgs e)
    {
        try { UserName = Page.User.Identity.Name.Split('\\')[1]; }
        catch { UserName = "Unknown"; }

        //bool belongsToGroups = ActiveDirectoryQueries.UserExistsInAnyGroup(UserName, ActiveDirecoryGroups);
        // IF the User exists in the Required Group, THEN make the page content visible.

        bool belongsToUsers = false;
        if (UserName == "tdo" || UserName == "khindra" || UserName == "kbennett")
        {
            belongsToUsers = true;
        }

        InvisibleError();
        InvisibleMsg();

        if (!this.IsPostBack)
        {
            InitiateAllControls();
            SetControlsDefault(belongsToUsers);
        }

        LoadControls();
                
    }

    private void InitiateAllControls()
    {
        LoadID1_ID2Parameters();
    }

    private void LoadControls()
    {        
    }

    private void SetControlsDefault(bool pFlag)
    {        
        DisableEnable_ID1_ID2_Controls(pFlag);
    }

    private void LoadID1_ID2Parameters()
    {
        List<AppSetting> myAppSetting = (from a in idc.AppSettings
                                         select a).ToList();

        foreach (AppSetting rec in myAppSetting)
        {
            switch (rec.AppKey)
            {
                case "Coefficient":
                    txtCoefficient_ID1.Text = rec.Value;
                    break;
                case "Noise":
                    txtNoise_ID1.Text = rec.Value;
                    break;
                case "MRD":
                    txtMRD_ID1.Text = rec.Value;
                    break;
                case "MRDIncrement":
                    txtMRDIncr_ID1.Text = rec.Value;
                    break;
                case "LLD":
                    txtLLD_ID1.Text = rec.Value;
                    break;
                case "FadingLimit":
                    txtFadingLimit_ID1.Text = rec.Value;
                    break;
                case "ReadLimit":
                    txtReadLimit_ID1.Text = rec.Value;
                    break;
                case "DaysSinceInitialization":
                    txtDaySinceInit_ID1.Text = rec.Value;
                    break;
                case "MRDID2":
                    txtMRD_ID2.Text = rec.Value;
                    break;
                case "MRDIncrementID2":
                    txtMRDIncr_ID2.Text = rec.Value;
                    break;
                case "DLNoiseID2":
                    txtDLNoise_ID2.Text = rec.Value;
                    break;
                case "DHNoiseID2":
                    txtDHNoise_ID2.Text = rec.Value;
                    break;
                case "SLNoiseID2":
                    txtSLNoise_ID2.Text = rec.Value;
                    break;
                case "SHNoiseID2":
                    txtSHNoise_ID2.Text = rec.Value;
                    break;
                case "DaysSinceInitializationID2":
                    txtDaySinceInit_ID2.Text = rec.Value;
                    break;
                case "AlgorithmFactorID2":
                    txtAlgorithmFactor_ID2.Text = rec.Value;
                    break;
            }
        }

    }

    private void DisableEnable_ID1_ID2_Controls(bool pFlag)
    {
        txtCoefficient_ID1.Enabled = pFlag;
        txtNoise_ID1.Enabled = pFlag;
        txtMRD_ID1.Enabled = pFlag;
        txtMRDIncr_ID1.Enabled = pFlag;
        txtLLD_ID1.Enabled = pFlag;
        txtFadingLimit_ID1.Enabled = pFlag;
        txtReadLimit_ID1.Enabled = pFlag;
        txtDaySinceInit_ID1.Enabled = pFlag;
        btnSave_ID1.Enabled = pFlag;

        txtMRD_ID2.Enabled = pFlag;
        txtMRDIncr_ID2.Enabled = pFlag;
        txtDLNoise_ID2.Enabled = pFlag;
        txtDHNoise_ID2.Enabled = pFlag;
        txtSLNoise_ID2.Enabled = pFlag;
        txtSHNoise_ID2.Enabled = pFlag;
        txtAlgorithmFactor_ID2.Enabled = pFlag;
        txtDaySinceInit_ID2.Enabled = pFlag;
        btnSave_ID2.Enabled = pFlag;
    }

    private Boolean PassValidation_ID1()
    {        
        string pattern = "^[-+]?[0-9]*\\.?[0-9]*$";

        if (this.txtCoefficient_ID1.Text.Trim().Length == 0)
        {
            VisibleError("ID1 Coefficient is required.");
            return false;
        }
        else
        {
            if (!Regex.IsMatch(this.txtCoefficient_ID1.Text.Trim(), pattern))
            {
                VisibleError("ID1 Coefficient must be a number.");
                return false;
            }
            else
            {
                if (Double.Parse(this.txtCoefficient_ID1.Text) <= 0)
                {
                    VisibleError("ID1 Coefficient must be greater or equal zero.");
                    return false;
                }
            }
        }

        if (this.txtNoise_ID1.Text.Trim().Length == 0)
        {
            VisibleError("ID1 Noise is required.");
            return false;
        }
        else
        {
            if (!Regex.IsMatch(this.txtNoise_ID1.Text.Trim(), pattern))
            {
                VisibleError("ID1 Noise must be a number.");
                return false;
            }
            else
            {
                if (Double.Parse(this.txtNoise_ID1.Text) < 0)
                {
                    VisibleError("ID1 Noise must be greater or equal zero.");
                    return false;
                }
            }
        }

        if (this.txtMRD_ID1.Text.Trim().Length == 0)
        {
            VisibleError("ID1 MRD is required.");
            return false;
        }
        else
        {
            if (!Regex.IsMatch(this.txtMRD_ID1.Text.Trim(), pattern))
            {
                VisibleError("ID1 MRD must be a number.");
                return false;
            }
            else
            {
                if (Double.Parse(this.txtMRD_ID1.Text) < 0)
                {
                    VisibleError("ID1 MRD must be greater or equal zero.");
                    return false;
                }
            }
        }

        if (this.txtMRDIncr_ID1.Text.Trim().Length == 0)
        {
            VisibleError("ID1 MRD Increment is required.");
            return false;
        }
        else
        {
            if (!Regex.IsMatch(this.txtMRDIncr_ID1.Text.Trim(), pattern))
            {
                VisibleError("ID1 MRD Increment must be a number.");
                return false;
            }
            else
            {
                if (Double.Parse(this.txtMRDIncr_ID1.Text) < 0)
                {
                    VisibleError("ID1 MRD Increment must be greater or equal zero.");
                    return false;
                }
            }
        }

        if (this.txtLLD_ID1.Text.Trim().Length == 0)
        {
            VisibleError("ID1 LLD is required.");
            return false;
        }
        else
        {
            if (!Regex.IsMatch(this.txtLLD_ID1.Text.Trim(), pattern))
            {
                VisibleError("ID1 LLD must be a number.");
                return false;
            }
            else
            {
                if (Double.Parse(this.txtLLD_ID1.Text) < 0)
                {
                    VisibleError("ID1 LLD must be greater or equal zero.");
                    return false;
                }
            }
        }

        if (this.txtFadingLimit_ID1.Text.Trim().Length == 0)
        {
            VisibleError("ID1 FadingLimit is required.");
            return false;
        }
        else
        {
            if (!Regex.IsMatch(this.txtFadingLimit_ID1.Text.Trim(), pattern))
            {
                VisibleError("ID1 FadingLimit must be a number.");
                return false;
            }
            else
            {
                if (Double.Parse(this.txtFadingLimit_ID1.Text) < 0)
                {
                    VisibleError("ID1 FadingLimit must be greater or equal zero.");
                    return false;
                }
            }
        }

        if (this.txtReadLimit_ID1.Text.Trim().Length == 0)
        {
            VisibleError("ID1 ReadLimit is required.");
            return false;
        }
        else
        {
            if (!Regex.IsMatch(this.txtReadLimit_ID1.Text.Trim(), pattern))
            {
                VisibleError("ID1 ReadLimit must be a number.");
                return false;
            }
            else
            {
                if (Double.Parse(this.txtReadLimit_ID1.Text) < 0)
                {
                    VisibleError("ID1 ReadLimit must be greater or equal zero.");
                    return false;
                }
            }
        }

        if (this.txtDaySinceInit_ID1.Text.Trim().Length == 0)
        {
            VisibleError("ID1 DaySinceInit is required.");
            return false;
        }
        else
        {
            if (!Regex.IsMatch(this.txtDaySinceInit_ID1.Text.Trim(), pattern))
            {
                VisibleError("ID1 DaySinceInit must be a number.");
                return false;
            }
            else
            {
                if (Double.Parse(this.txtDaySinceInit_ID1.Text) <= 0)
                {
                    VisibleError("ID1 DaySinceInit must be greater than zero.");
                    return false;
                }
            }
        }        
        
        return true;
    }

    private Boolean PassValidation_ID2()
    {        
        string pattern = "^[-+]?[0-9]*\\.?[0-9]*$";

        if (this.txtMRD_ID2.Text.Trim().Length == 0)
        {
            VisibleError("ID2 MRD is required.");
            return false;
        }
        else
        {
            if (!Regex.IsMatch(this.txtMRD_ID2.Text.Trim(), pattern))
            {
                VisibleError("ID2 MRD must be a number.");
                return false;
            }
            else
            {
                if (Double.Parse(this.txtMRD_ID2.Text) < 0)
                {
                    VisibleError("ID2 MRD must be greater or equal zero.");
                    return false;
                }
            }
        }

        if (this.txtMRDIncr_ID2.Text.Trim().Length == 0)
        {
            VisibleError("ID2 MRD Increment is required.");
            return false;
        }
        else
        {
            if (!Regex.IsMatch(this.txtMRDIncr_ID2.Text.Trim(), pattern))
            {
                VisibleError("ID2 MRD Increment must be a number.");
                return false;
            }
            else
            {
                if (Double.Parse(this.txtMRDIncr_ID2.Text) < 0)
                {
                    VisibleError("ID2 MRD Increment must be greater or equal zero.");
                    return false;
                }
            }
        }

        if (this.txtDLNoise_ID2.Text.Trim().Length == 0)
        {
            VisibleError("ID2 DLNoise is required.");
            return false;
        }
        else
        {
            if (!Regex.IsMatch(this.txtDLNoise_ID2.Text.Trim(), pattern))
            {
                VisibleError("ID2 DLNoise must be a number.");
                return false;
            }
            else
            {
                if (Double.Parse(this.txtDLNoise_ID2.Text) < 0)
                {
                    VisibleError("ID2 DLNoise must be greater or equal zero.");
                    return false;
                }
            }
        }

        if (this.txtDHNoise_ID2.Text.Trim().Length == 0)
        {
            VisibleError("ID2 DHNoise is required.");
            return false;
        }
        else
        {
            if (!Regex.IsMatch(this.txtDHNoise_ID2.Text.Trim(), pattern))
            {
                VisibleError("ID2 DHNoise must be a number.");
                return false;
            }
            else
            {
                if (Double.Parse(this.txtDHNoise_ID2.Text) < 0)
                {
                    VisibleError("ID2 DHNoise must be greater or equal zero.");
                    return false;
                }
            }
        }

        if (this.txtSLNoise_ID2.Text.Trim().Length == 0)
        {
            VisibleError("ID2 SLNoise is required.");
            return false;
        }
        else
        {
            if (!Regex.IsMatch(this.txtSLNoise_ID2.Text.Trim(), pattern))
            {
                VisibleError("ID2 SLNoise must be a number.");
                return false;
            }
            else
            {
                if (Double.Parse(this.txtSLNoise_ID2.Text) < 0)
                {
                    VisibleError("ID2 SLNoise must be greater or equal zero.");
                    return false;
                }
            }
        }

        if (this.txtSHNoise_ID2.Text.Trim().Length == 0)
        {
            VisibleError("ID2 SHNoise is required.");
            return false;
        }
        else
        {
            if (!Regex.IsMatch(this.txtSHNoise_ID2.Text.Trim(), pattern))
            {
                VisibleError("ID2 SHNoise must be a number.");
                return false;
            }
            else
            {
                if (Double.Parse(this.txtSHNoise_ID2.Text) < 0)
                {
                    VisibleError("ID2 SHNoise must be greater or equal zero.");
                    return false;
                }
            }
        }  
       
        if (this.txtAlgorithmFactor_ID2.Text.Trim().Length == 0)
        {
            VisibleError("ID2 AlgorithmFactor is required.");
            return false;
        }
        else
        {
            if (this.txtAlgorithmFactor_ID2.Text.Trim().ToUpper() != "G" && this.txtAlgorithmFactor_ID2.Text.Trim().ToUpper() != "BG")
            {
                VisibleError("ID2 AlgorithmFactor must be G for Gamma or BG for Beta Gamma.");
                return false;
            }
        }

        if (this.txtDaySinceInit_ID2.Text.Trim().Length == 0)
        {
            VisibleError("ID2 DaySinceInit is required.");
            return false;
        }
        else
        {
            if (!Regex.IsMatch(this.txtDaySinceInit_ID2.Text.Trim(), pattern))
            {
                VisibleError("ID2 DaySinceInit must be a number.");
                return false;
            }
            else
            {
                if (Double.Parse(this.txtDaySinceInit_ID2.Text) <= 0)
                {
                    VisibleError("ID2 DaySinceInit must be greater than zero.");
                    return false;
                }
            }
        }  
        
        return true;
    }    

    protected void btnSave_ID1_Click(object sender, EventArgs e)
    {
        if (PassValidation_ID1())
        {
            try
            {
                List<AppSetting> myAppSetting = (from a in idc.AppSettings
                                                 select a).ToList();

                foreach (AppSetting rec in myAppSetting)
                {
                    switch (rec.AppKey)
                    {
                        case "Coefficient":
                            rec.Value = txtCoefficient_ID1.Text;
                            break;
                        case "Noise":
                            rec.Value = txtNoise_ID1.Text;
                            break;
                        case "MRD":
                            rec.Value = txtMRD_ID1.Text;
                            break;
                        case "MRDIncrement":
                            rec.Value =txtMRDIncr_ID1.Text;
                            break;
                        case "LLD":
                            rec.Value = txtLLD_ID1.Text;
                            break;
                        case "FadingLimit":
                            rec.Value = txtFadingLimit_ID1.Text;
                            break;
                        case "ReadLimit":
                            rec.Value = txtReadLimit_ID1.Text;
                            break;
                        case "DaysSinceInitialization":
                            rec.Value = txtDaySinceInit_ID1.Text;
                            break;                        
                    }
                }

                idc.SubmitChanges();

                VisibleMsg("The update on ID1 dose calculation parameters is completed.");

                // Reload control
                this.LoadID1_ID2Parameters();
            }
            catch (Exception ex)
            {
                // Display the system generated error message.                
                VisibleError(string.Format("An error occurred: {0}", ex.Message));
            }
        }
    }

    protected void btnSave_ID2_Click(object sender, EventArgs e)
    {
        if (PassValidation_ID2())
        {
            try
            {
                List<AppSetting> myAppSetting = (from a in idc.AppSettings
                                                 select a).ToList();

                foreach (AppSetting rec in myAppSetting)
                {
                    switch (rec.AppKey)
                    {                        
                        case "MRDID2":
                            rec.Value = txtMRD_ID2.Text;
                            break;
                        case "MRDIncrementID2":
                            rec.Value = txtMRDIncr_ID2.Text;
                            break;
                        case "DLNoiseID2":
                            rec.Value = txtDLNoise_ID2.Text;
                            break;
                        case "DHNoiseID2":
                            rec.Value = txtDHNoise_ID2.Text;
                            break;
                        case "SLNoiseID2":
                            rec.Value = txtSLNoise_ID2.Text;
                            break;
                        case "SHNoiseID2":
                            rec.Value = txtSHNoise_ID2.Text;
                            break;
                        case "DaysSinceInitializationID2":
                            rec.Value = txtDaySinceInit_ID2.Text;
                            break;
                        case "AlgorithmFactorID2":
                            rec.Value = txtAlgorithmFactor_ID2.Text.ToUpper();
                            break;
                    }
                }

                idc.SubmitChanges();

                VisibleMsg("The update on ID2 dose calculation parameters is completed.");

                // Reload control
                this.LoadID1_ID2Parameters();
            }
            catch (Exception ex)
            {
                // Display the system generated error message.                
                VisibleError(string.Format("An error occurred: {0}", ex.Message));
            }
        }
    }



    private void InvisibleError()
    {
        // Reset submission form error message      
        this.ErrorMsg.InnerHtml = "";
        this.Errors.Visible = false;
    }

    private void VisibleError(string error)
    {
        this.ErrorMsg.InnerHtml = error;
        this.Errors.Visible = true;
    }

    private void InvisibleMsg()
    {
        // Reset submission form error message      
        this.SuccessMsg.InnerHtml = "";
        this.Success.Visible = false;
    }

    private void VisibleMsg(string error)
    {
        this.SuccessMsg.InnerHtml = error;
        this.Success.Visible = true;
    } 
   
}
