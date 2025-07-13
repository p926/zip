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

public partial class Shipping_ReceivingBinManagement : System.Web.UI.Page
{
    InsDataContext idc = new InsDataContext();
    AppDataContext adc = new AppDataContext();
    string UserName = "Unknown";
    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            this.UserName = User.Identity.Name.Split('\\')[1];            

            if (Page.IsPostBack) return;

            SetErrorMessage("");
            SetSuccessMessage("");

            string page = @"/Shipping/ReceivingBinManagement.aspx";
            string accessiblePeople = (from ap in idc.AuthPages where ap.Page == page && ap.Active == true select ap.Who).FirstOrDefault();

            // IF page is setup for accessibility and the access person is not setup yet then do not allow to access the page.
            if (accessiblePeople.Length > 0 && !accessiblePeople.ToUpper().Contains(this.UserName.ToUpper()))
            {
                Page.Response.Redirect("Default.aspx");
            }
        }
        catch { this.UserName = "Unknown"; }
    }    
    protected void grdViewReturnReason_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {            
            // Load Bin drop down list
            System.Web.UI.WebControls.Label l = (System.Web.UI.WebControls.Label)e.Row.FindControl("lblBinID");
            DropDownList ddl = (DropDownList)e.Row.FindControl("ddlBin");

            ddl.DataSource = (from a in adc.rma_ref_Bins select a);
            ddl.DataBind();

            ListItem firstItem = new ListItem("", "0");
            ddl.Items.Insert(0, firstItem);
           
            ddl.SelectedValue = l.Text;
        }
    }
    protected void btnSave_Click(object sender, EventArgs e)
    {
        try
        {
            SetErrorMessage("");
            SetSuccessMessage("");

            foreach (GridViewRow row in this.grdViewReturnReason.Rows)
            {
                int rowIndex = row.RowIndex;
                int reasonID = Convert.ToInt32(grdViewReturnReason.DataKeys[rowIndex].Values[0]);
                DropDownList ddlBin = (DropDownList)row.FindControl("ddlBin");

                rma_ref_ReturnReason returnReason = (from rr in adc.rma_ref_ReturnReasons
                                                     where rr.ReasonID == reasonID
                                                     select rr).FirstOrDefault();

                if (returnReason != null)
                {
                    returnReason.BinID = ddlBin.SelectedValue == "0" ? (int?) null : int.Parse(ddlBin.SelectedValue);
                    adc.SubmitChanges();                    
                }            

            }

            // refresh gridview
            grdViewReturnReason.DataBind();

            SetSuccessMessage("Commit successfully!");

        }
        catch (Exception ex)
        {
            // Display the system generated error message.            
            SetErrorMessage(string.Format("An error occurred: {0}", ex.Message));
        }        
    }

    private void SetErrorMessage(string error)
    {
        this.errorMsg.InnerHtml = error;
        this.error.Visible = !String.IsNullOrEmpty(error);
    }
    private void SetSuccessMessage(string message)
    {
        this.successMsg.InnerHtml = message;
        this.success.Visible = !String.IsNullOrEmpty(message);
    }

}