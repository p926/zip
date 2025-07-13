using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using Instadose;
using Instadose.Data;
using Instadose.Processing;
using System.IO;

using System.Threading;


public partial class TechOps_RecallStatus : System.Web.UI.Page
{
    int AccountID = 0;
    int OrderID = 0;
    int LocationID = 0;
    int CaseID = 0;
    // Create the database reference
    AppDataContext adc = new AppDataContext();
    InsDataContext idc = new InsDataContext();

    // String to hold the current username
    string UserName = "Unknown";
    protected void Page_PreInit(object sender, EventArgs e)
    {
        Page.EnableEventValidation = false;
    }


    protected void Page_Load(object sender, EventArgs e)
    {        
        try
        {
            this.UserName = User.Identity.Name.Split('\\')[1];

            // Show total recalls
            var RecallCount = (from ret in adc.rma_Returns
                           join retDev in adc.rma_ReturnDevices on ret.ReturnID equals retDev.ReturnID
                           where ret.ReturnTypeID == 3
                           select new { ret, retDev }).Count();

            //int RecallCount = (from rec in Recalls
            //                   join acct in idc.Accounts on rec.ret.AccountID equals acct.AccountID
            //                   join devInv in idc.DeviceInventories on rec.retDev.SerialNo equals devInv.SerialNo
            //                   where rec.ret.ReturnTypeID == 3
            //                   select rec).Count();

            lblTotalRecall.Text = "Total Recalls: " + RecallCount.ToString();           
        }
        catch { }                              
    }

   
    protected void btnShowAllRecall_Click(object sender, EventArgs e)
    {
        //gvRecalls.AllowPaging = false;
        //btnShowAllRecall.Visible = false;
        gvRecalls.AllowPaging = !gvRecalls.AllowPaging;
        gvRecalls.DataBind();
    }

    public override void VerifyRenderingInServerForm(Control control)
    {
        /* Confirms that an HtmlForm control is rendered for the specified ASP.NET
           server control at run time. */
    }

    protected void btnSaveExcel_Click(object sender, EventArgs e)
    {
        Response.Clear();
        Response.AddHeader("content-disposition", "attachment;filename=FileName.xls");
        Response.Charset = "";

        //// If you want the option to open the Excel file without saving then
        //// comment out the line below
        //// Response.Cache.SetCacheability(HttpCacheability.NoCache);
        //Response.ContentType = "application/vnd.xls";
        //System.IO.StringWriter stringWrite = new System.IO.StringWriter();
        //System.Web.UI.HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);
        //this.gvReturns .RenderControl(htmlWrite);
        //Response.Write(stringWrite.ToString());
        //Response.End();


        using (System.IO.StringWriter sw = new System.IO.StringWriter())
        {
            using (HtmlTextWriter htw = new HtmlTextWriter(sw))
            {
                //  Create a table to contain the grid
                Table table = new Table();

                //  include the gridline settings
                table.GridLines = gvRecalls.GridLines;

                //  add the header row to the table
                if (gvRecalls.HeaderRow != null)
                {
                    PrepareControlForExport(gvRecalls.HeaderRow);
                    table.Rows.Add(gvRecalls.HeaderRow);
                }

                //  add each of the data rows to the table
                foreach (GridViewRow row in gvRecalls.Rows)
                {
                    //PrepareControlForExport(row);
                    table.Rows.Add(row);
                }

                //  add the footer row to the table
                if (gvRecalls.FooterRow != null)
                {
                    //PrepareControlForExport(gvReturns.FooterRow);
                    table.Rows.Add(gvRecalls.FooterRow);
                }

                //  render the table into the htmlwriter
                table.RenderControl(htw);

                //  render the htmlwriter into the response
                HttpContext.Current.Response.Write(sw.ToString());
                HttpContext.Current.Response.End();
            }
        }

    }

    /// <summary>
    /// Replace any of the contained controls with literals
    /// </summary>
    /// <param name="control"></param>
    private static void PrepareControlForExport(Control control)
    {
        for (int i = 0; i < control.Controls.Count; i++)
        {
            Control current = control.Controls[i];
            if (current is LinkButton)
            {
                control.Controls.Remove(current);
                control.Controls.AddAt(i, new LiteralControl((current as LinkButton).Text));
            }
            else if (current is ImageButton)
            {
                control.Controls.Remove(current);
                control.Controls.AddAt(i, new LiteralControl((current as ImageButton).AlternateText));
            }
            else if (current is HyperLink)
            {
                control.Controls.Remove(current);
                control.Controls.AddAt(i, new LiteralControl((current as HyperLink).Text));
            }
            else if (current is DropDownList)
            {
                control.Controls.Remove(current);
                control.Controls.AddAt(i, new LiteralControl((current as DropDownList).SelectedItem.Text));
            }
            else if (current is CheckBox)
            {
                control.Controls.Remove(current);
                control.Controls.AddAt(i, new LiteralControl((current as CheckBox).Checked ? "True" : "False"));
            }

            if (current.HasControls())
            {
                PrepareControlForExport(current);
            }
        }
    }

}
