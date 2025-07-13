/*
 * File: OthersListing
 * Author: JA
 * Created: May, 2011
 * Version: 1.0
 * 
 * Present 'Other's listing for Kip
 * 
 * Change History:
 *  > 1.0: Original release
 * 
 *  Copyright (C) 2010 Mirion Technologies. All Rights Reserved.
 *  Modified by: Tdo, 9/6/2012
 *  Version: 2.0
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

public partial class InstaDose_TechOps_OthersListing : System.Web.UI.Page
{
  InsDataContext idc = new InsDataContext();

  protected void Page_Load(object sender, EventArgs e)
  {
    if (!IsPostBack)
    {
        DateTime tDay = DateTime.Today;
        this.txtStartdate.Text = tDay.ToShortDateString ();                
    }
   
  }

}










  
