﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Configuration;
using System.Web.Security;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;
using System.Text;
using System.Web.SessionState;

public partial class InstaDose_TechOps_PrintIt : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
      Control ctrl = (Control)Session["ctrl"];
      PrintHelper.PrintWebControl(ctrl); 

    }

    public class PrintHelper
    {

      public PrintHelper()
      {
        
      }

      public static void PrintWebControl(Control ctrl)
      {

        PrintWebControl(ctrl, string.Empty);

      }

      public static void PrintWebControl(Control ctrl, string Script)
      {

        StringWriter stringWrite = new StringWriter();
        System.Web.UI.HtmlTextWriter htmlWrite = new System.Web.UI.HtmlTextWriter(stringWrite);

        if (ctrl is WebControl)
        {

          Unit w = new Unit(100, UnitType.Percentage); 
          ((WebControl)ctrl).Width = w;

        }

        Page pg = new Page();
        pg.EnableEventValidation = false;

        if (Script != string.Empty)
        {

          pg.ClientScript.RegisterStartupScript(pg.GetType(), "PrintJavaScript", Script);

        }

        HtmlForm frm = new HtmlForm();
        pg.Controls.Add(frm);

        frm.Attributes.Add("runat", "server");
        frm.Controls.Add(ctrl);
        pg.DesignerInitialize();
        pg.RenderControl(htmlWrite);
        string strHTML = stringWrite.ToString();
        HttpContext.Current.Response.Clear();
        HttpContext.Current.Response.Write(strHTML);
        HttpContext.Current.Response.Write("<script>window.print();</script>");
        HttpContext.Current.Response.End();

      }

    } 

  


}
