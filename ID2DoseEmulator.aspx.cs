/*
 * File: PressureTestResults
 * Author: JA
 * Created: November 2010
 * Version: 1.0
 * 
 * Maintain Instadose Financial Rate Details
 * 
 * Change History:
 *  > 1.0: Original release
 * 
 *  Copyright (C) 2010 Mirion Technologies. All Rights Reserved.
 */
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Collections;

using Instadose;
using Instadose.Data;
using Instadose.Device;

public partial class InstaDose_TechOps_ID2DoseEmulator : System.Web.UI.Page
{
  // LCDISBUS database
 

  InsDataContext idc = new InsDataContext();
  
  
  protected void Page_Load(object sender, EventArgs e)
  {
      if (!IsPostBack)
      {
          DefaultLoading();
      }

      pnlResult.Visible = false;
      InvisibleErrors();
  }  

  private void InvisibleErrors()
  {
      // Reset submission form error message      
      this.errorMsg.InnerHtml  = "";
      this.errors.Visible = false;
  }

  private void VisibleErrors(string error)
  {
      this.errorMsg.InnerHtml = error;
      this.errors.Visible = true;
  }

  /// <summary>
  /// Set default raw values for an ID2 device to calculate 100 mrem
  /// </summary>
  private void DefaultLoading()
  {
      txtDL.Text = "832468";
      txtDLT.Text = "205500";
      txtDLDac.Text = "1";
      txtDLT1.Text = "205500";

      txtDH.Text = "938578";
      txtDHT.Text = "205500";
      txtDHDac.Text = "1";
      txtDHT1.Text = "205500";

      txtSL.Text = "764850";
      txtSLT.Text = "205500";
      txtSLDac.Text = "1";
      txtSLT1.Text = "205500";

      txtSH.Text = "909466";
      txtSHT.Text = "205500";
      txtSHDac.Text = "1";
      txtSHT1.Text = "205500";
  }

  protected void btnCalculate_Click(object sender, EventArgs e)
  {
      int DL, DH, SL, SH, DLT, DHT, SLT, SHT, DLDac, DHDac, SLDac, SHDac, DLT1, DHT1, SLT1, SHT1;     
      int SeqNo = 0;
      Hashtable package = new Hashtable() ;
      Hashtable calculatedDose = new Hashtable();
      DeviceInventory _device = null;      
      string errorMsg = "";  
    
      try 
	  {
          if (passValidation())
          {
              DL = int.Parse(txtDL.Text.Trim());
              DLT = int.Parse(txtDLT.Text.Trim());
              DLT1 = int.Parse(txtDLT1.Text.Trim());
              DLDac = int.Parse(txtDLDac.Text.Trim());
              DH = int.Parse(txtDH.Text.Trim());
              DHT = int.Parse(txtDHT.Text.Trim());
              DHT1 = int.Parse(txtDHT1.Text.Trim());
              DHDac = int.Parse(txtDHDac.Text.Trim());
              SL = int.Parse(txtSL.Text.Trim());
              SLT = int.Parse(txtSLT.Text.Trim());
              SLT1 = int.Parse(txtSLT1.Text.Trim());
              SLDac = int.Parse(txtSLDac.Text.Trim());
              SH = int.Parse(txtSH.Text.Trim());
              SHT = int.Parse(txtSHT.Text.Trim());
              SHT1 = int.Parse(txtSHT1.Text.Trim());
              SHDac = int.Parse(txtSHDac.Text.Trim());

              // Get the device.          
              _device = (from di in idc.DeviceInventories where di.SerialNo == sn.Text select di).FirstOrDefault();
              if (_device != null)
              {
                  if (_device.LastAckSequenceNum.HasValue) SeqNo = _device.LastAckSequenceNum.Value;
              }              

              package.Add("DL", DL);
              package.Add("DLT", DLT);
              package.Add("DLT1", DLT1);
              package.Add("DLD", DLDac);
              package.Add("DH", DH);
              package.Add("DHT", DHT);
              package.Add("DHT1", DHT1);
              package.Add("DHD", DHDac);
              package.Add("SL", SL);
              package.Add("SLT", SLT);
              package.Add("SLT1", SLT1);
              package.Add("SLD", SLDac);
              package.Add("SH", SH);
              package.Add("SHT", SHT);
              package.Add("SHT1", SHT1);
              package.Add("SHD", SHDac);
              package.Add("RecType", 0);
              package.Add("SeqNo", ++SeqNo);
              package.Add("DoseRead", System.DateTime.UtcNow);
              package.Add("AlgorithmFactor", this.txtAlgFactor.Text.Trim());

              if (Calculate(sn.Text, package, ref errorMsg, ref calculatedDose))
              {
                  string DLDoseRaw, DLDoseCalc, DLCumDose, DLDose;
                  string DHDoseRaw, DHDoseCalc, DHCumDose, DHDose;
                  string SLDoseRaw, SLDoseCalc, SLCumDose, SLDose;
                  string SHDoseRaw, SHDoseCalc, SHCumDose, SHDose;
                  string ratio = "";
                  string pathWay = "";
                  string deepDose, eyeDose, shallowDose;

                  DLDoseRaw = calculatedDose.ContainsKey("DLDoseRaw") ? calculatedDose["DLDoseRaw"].ToString() : "";
                  DLDoseCalc = calculatedDose.ContainsKey("DLDoseCalc") ? calculatedDose["DLDoseCalc"].ToString() : "";
                  DLCumDose = calculatedDose.ContainsKey("DLCumDose") ? calculatedDose["DLCumDose"].ToString() : "";
                  DLDose = calculatedDose.ContainsKey("DLDose") ? calculatedDose["DLDose"].ToString() : "";

                  DHDoseRaw = calculatedDose.ContainsKey("DHDoseRaw") ? calculatedDose["DHDoseRaw"].ToString() : "";
                  DHDoseCalc = calculatedDose.ContainsKey("DHDoseCalc") ? calculatedDose["DHDoseCalc"].ToString() : "";
                  DHCumDose = calculatedDose.ContainsKey("DHCumDose") ? calculatedDose["DHCumDose"].ToString() : "";
                  DHDose = calculatedDose.ContainsKey("DHDose") ? calculatedDose["DHDose"].ToString() : "";

                  SLDoseRaw = calculatedDose.ContainsKey("SLDoseRaw") ? calculatedDose["SLDoseRaw"].ToString() : "";
                  SLDoseCalc = calculatedDose.ContainsKey("SLDoseCalc") ? calculatedDose["SLDoseCalc"].ToString() : "";
                  SLCumDose = calculatedDose.ContainsKey("SLCumDose") ? calculatedDose["SLCumDose"].ToString() : "";
                  SLDose = calculatedDose.ContainsKey("SLDose") ? calculatedDose["SLDose"].ToString() : "";

                  SHDoseRaw = calculatedDose.ContainsKey("SHDoseRaw") ? calculatedDose["SHDoseRaw"].ToString() : "";
                  SHDoseCalc = calculatedDose.ContainsKey("SHDoseCalc") ? calculatedDose["SHDoseCalc"].ToString() : "";
                  SHCumDose = calculatedDose.ContainsKey("SHCumDose") ? calculatedDose["SHCumDose"].ToString() : "";
                  SHDose = calculatedDose.ContainsKey("SHDose") ? calculatedDose["SHDose"].ToString() : "";

                  pathWay = calculatedDose.ContainsKey("PathWay") ? calculatedDose["PathWay"].ToString() : "";
                  deepDose = calculatedDose.ContainsKey("DeepDose") ? calculatedDose["DeepDose"].ToString() : "";
                  eyeDose = calculatedDose.ContainsKey("EyeDose") ? calculatedDose["EyeDose"].ToString() : "";
                  shallowDose = calculatedDose.ContainsKey("ShallowDose") ? calculatedDose["ShallowDose"].ToString() : "";

                  double mySLDose, myDLDose;
                  if (double.TryParse(SLDose, out mySLDose) && double.TryParse(DLDose, out myDLDose))
                  {
                      ratio = Math.Round((mySLDose / myDLDose), 4).ToString();
                  }

                  lblDLDoseRaw.Text = DLDoseRaw;
                  lblDLDoseCalc.Text = DLDoseCalc;
                  lblDLCumDose.Text = DLCumDose;
                  lblDLDose.Text = DLDose;

                  lblDHDoseRaw.Text = DHDoseRaw;
                  lblDHDoseCalc.Text = DHDoseCalc;
                  lblDHCumDose.Text = DHCumDose;
                  lblDHDose.Text = DHDose;

                  lblSLDoseRaw.Text = SLDoseRaw;
                  lblSLDoseCalc.Text = SLDoseCalc;
                  lblSLCumDose.Text = SLCumDose;
                  lblSLDose.Text = SLDose;

                  lblSHDoseRaw.Text = SHDoseRaw;
                  lblSHDoseCalc.Text = SHDoseCalc;
                  lblSHCumDose.Text = SHCumDose;
                  lblSHDose.Text = SHDose;

                  lblSLD_DLD_Ratio.Text = ratio;
                  lblPathway.Text = pathWay;
                  lblDeepDose.Text = deepDose;
                  lblEyeDose.Text = eyeDose;
                  lblShallowDose.Text = shallowDose;

                  pnlResult.Visible = true;
              }
              else
              {
                  VisibleErrors(errorMsg);
              }
          } 
	  }
	  catch (Exception ex)
	  {
          VisibleErrors(ex.ToString());		  
	  }
           
  }

  private bool Calculate(string sn, Hashtable package, ref string errorMsg, ref Hashtable calculatedDose)
  {
      string readType = "Dry Read";

      // Create the ID2 Dose Calcs.
      ID2Calculations calc = new ID2Calculations();
      // Initiate Result object
      CalculationResult result = new CalculationResult();

      result = calc.PerformSingleCalculation_Dummy(sn, package, 0, readType, ref calculatedDose);

      if (result.ErrorMessage != "")
      {
          errorMsg = result.ErrorMessage;
          return false;
      }

      return true;
  }

  private bool passValidation()
  {      
      int myInt;
      bool isFail = false;

      string errorText = "The following error(s) occurred: <ul type='circle'>";

      // -------------- Algorithm Factor Valid --------------------//
      if (this.txtAlgFactor.Text.Trim().Length == 0)
      {
          errorText += "<li>" + "Algorithm Factor is required. It should be G or BG" + "</li>";
          isFail = true;
      }
      else if (this.txtAlgFactor.Text.Trim().ToUpper() != "G" && this.txtAlgFactor.Text.Trim().ToUpper() != "BG")
      {
          errorText += "<li>" + "Algorithm Factor should be G or BG" + "</li>";
          isFail = true;
      }
      
      // -------------- Algorithm Factor Valid --------------------//

      // -------------- DL/DH/SL/SH Valid --------------------//
      if (this.txtDL.Text.Trim().Length == 0)
      {          
          errorText += "<li>" + "DL is required." + "</li>";
          isFail = true;
      }
      else if (! int.TryParse(this.txtDL.Text, out myInt))
      {          
          errorText += "<li>" + "DL must be numeric." + "</li>";
          isFail = true;
      }
      else if (myInt <= 0)
      {          
          errorText += "<li>" + "DL must be greater than 0." + "</li>";
          isFail = true;
      }

      if (this.txtDH.Text.Trim().Length == 0)
      {          
          errorText += "<li>" + "DH is required." + "</li>";
          isFail = true;
      }
      else if (!int.TryParse(this.txtDH.Text, out myInt))
      {         
          errorText += "<li>" + "DH must be numeric." + "</li>";
          isFail = true;
      }
      else if (myInt <= 0)
      {
          errorText += "<li>" + "DH must be greater than 0." + "</li>";
          isFail = true;
      }

      if (this.txtSL.Text.Trim().Length == 0)
      {         
          errorText += "<li>" + "SL is required." + "</li>";
          isFail = true;
      }
      else if (!int.TryParse(this.txtSL.Text, out myInt))
      {         
          errorText += "<li>" + "SL must be numeric." + "</li>";
          isFail = true;
      }
      else if (myInt <= 0)
      {
          errorText += "<li>" + "SL must be greater than 0." + "</li>";
          isFail = true;
      }

      if (this.txtSH.Text.Trim().Length == 0)
      {          
          errorText += "<li>" + "SH is required." + "</li>";
          isFail = true;
      }
      else if (!int.TryParse(this.txtSH.Text, out myInt))
      {          
          errorText += "<li>" + "SH must be numeric." + "</li>";
          isFail = true;
      }
      else if (myInt <= 0)
      {
          errorText += "<li>" + "SH must be greater than 0." + "</li>";
          isFail = true;
      }

      // -------------- DLT/DHT/SLT/SHT Valid --------------------//

      if (this.txtDLT.Text.Trim().Length == 0)
      {          
          errorText += "<li>" + "DLT is required." + "</li>";
          isFail = true;
      }
      else if (!int.TryParse(this.txtDLT.Text, out myInt))
      {          
          errorText += "<li>" + "DLT must be numeric." + "</li>";
          isFail = true;
      }
      else if (myInt <= 0)
      {
          errorText += "<li>" + "DLT must be greater than 0." + "</li>";
          isFail = true;
      }

      if (this.txtDHT.Text.Trim().Length == 0)
      {
          errorText += "<li>" + "DHT is required." + "</li>";
          isFail = true;
      }
      else if (!int.TryParse(this.txtDHT.Text, out myInt))
      {
          errorText += "<li>" + "DHT must be numeric." + "</li>";
          isFail = true;
      }
      else if (myInt <= 0)
      {
          errorText += "<li>" + "DHT must be greater than 0." + "</li>";
          isFail = true;
      }

      if (this.txtSLT.Text.Trim().Length == 0)
      {
          errorText += "<li>" + "SLT is required." + "</li>";
          isFail = true;
      }
      else if (!int.TryParse(this.txtSLT.Text, out myInt))
      {
          errorText += "<li>" + "SLT must be numeric." + "</li>";
          isFail = true;
      }
      else if (myInt <= 0)
      {
          errorText += "<li>" + "SLT must be greater than 0." + "</li>";
          isFail = true;
      }

      if (this.txtSHT.Text.Trim().Length == 0)
      {
          errorText += "<li>" + "SHT is required." + "</li>";
          isFail = true;
      }
      else if (!int.TryParse(this.txtSHT.Text, out myInt))
      {
          errorText += "<li>" + "SHT must be numeric." + "</li>";
          isFail = true;
      }
      else if (myInt <= 0)
      {
          errorText += "<li>" + "SHT must be greater than 0." + "</li>";
          isFail = true;
      }

      // -------------- DLDac/DHDac/SLDac/SHDac Valid --------------------//

      if (this.txtDLDac.Text.Trim().Length == 0)
      {
          errorText += "<li>" + "DLDac is required." + "</li>";
          isFail = true;
      }
      else if (!int.TryParse(this.txtDLDac.Text, out myInt))
      {
          errorText += "<li>" + "DLDac must be numeric." + "</li>";
          isFail = true;
      }
      else if (myInt < 0 || myInt > 7)
      {
          errorText += "<li>" + "DLDac number must be between 0 and 7." + "</li>";
          isFail = true;
      }

      if (this.txtDHDac.Text.Trim().Length == 0)
      {
          errorText += "<li>" + "DHDac is required." + "</li>";
          isFail = true;
      }
      else if (!int.TryParse(this.txtDHDac.Text, out myInt))
      {
          errorText += "<li>" + "DHDac must be numeric." + "</li>";
          isFail = true;
      }
      else if (myInt < 0 || myInt > 7)
      {
          errorText += "<li>" + "DHDac number must be between 0 and 7." + "</li>";
          isFail = true;
      }

      if (this.txtSLDac.Text.Trim().Length == 0)
      {
          errorText += "<li>" + "SLDac is required." + "</li>";
          isFail = true;
      }
      else if (!int.TryParse(this.txtSLDac.Text, out myInt))
      {
          errorText += "<li>" + "SLDac must be numeric." + "</li>";
          isFail = true;
      }
      else if (myInt < 0 || myInt > 7)
      {
          errorText += "<li>" + "SLDac number must be between 0 and 7." + "</li>";
          isFail = true;
      }

      if (this.txtSHDac.Text.Trim().Length == 0)
      {
          errorText += "<li>" + "SHDac is required." + "</li>";
          isFail = true;
      }
      else if (!int.TryParse(this.txtSHDac.Text, out myInt))
      {
          errorText += "<li>" + "SHDac must be numeric." + "</li>";
          isFail = true;
      }
      else if (myInt < 0 || myInt > 7)
      {
          errorText += "<li>" + "SHDac number must be between 0 and 7." + "</li>";
          isFail = true;
      }

      // -------------- DLT1/DHT1/SLT1/SHT1 Valid --------------------//

      if (this.txtDLT1.Text.Trim().Length == 0)
      {
          errorText += "<li>" + "DLT1 is required." + "</li>";
          isFail = true;
      }
      else if (!int.TryParse(this.txtDLT1.Text, out myInt))
      {
          errorText += "<li>" + "DLT1 must be numeric." + "</li>";
          isFail = true;
      }
      else if (myInt <= 0)
      {
          errorText += "<li>" + "DLT1 must be greater than 0." + "</li>";
          isFail = true;
      }

      if (this.txtDHT1.Text.Trim().Length == 0)
      {
          errorText += "<li>" + "DHT1 is required." + "</li>";
          isFail = true;
      }
      else if (!int.TryParse(this.txtDHT1.Text, out myInt))
      {
          errorText += "<li>" + "DHT1 must be numeric." + "</li>";
          isFail = true;
      }
      else if (myInt <= 0)
      {
          errorText += "<li>" + "DHT1 must be greater than 0." + "</li>";
          isFail = true;
      }

      if (this.txtSLT1.Text.Trim().Length == 0)
      {
          errorText += "<li>" + "SLT1 is required." + "</li>";
          isFail = true;
      }
      else if (!int.TryParse(this.txtSLT1.Text, out myInt))
      {
          errorText += "<li>" + "SLT1 must be numeric." + "</li>";
          isFail = true;
      }
      else if (myInt <= 0)
      {
          errorText += "<li>" + "SLT1 must be greater than 0." + "</li>";
          isFail = true;
      }

      if (this.txtSHT1.Text.Trim().Length == 0)
      {
          errorText += "<li>" + "SHT1 is required." + "</li>";
          isFail = true;
      }
      else if (!int.TryParse(this.txtSHT1.Text, out myInt))
      {
          errorText += "<li>" + "SHT1 must be numeric." + "</li>";
          isFail = true;
      }
      else if (myInt <= 0)
      {
          errorText += "<li>" + "SHT1 must be greater than 0." + "</li>";
          isFail = true;
      }

      errorText += "</ul>";

      if (isFail)
      {
          VisibleErrors(errorText);
          return false;
      }
      else
      {
          return true;
      }

  }

}










  
