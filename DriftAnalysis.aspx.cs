using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Instadose;
using Instadose.Data;
using Instadose.Device.Calibration;

public partial class TechOps_DriftAnalysis : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        InvisibleErrors();
        InvisibleMessages();
        this.InventoryToolBar.Visible = false;
    }

    protected void ddlGroupName_SelectedIndexChanged(object sender, EventArgs e)
    {
        this.gvDevices.DataBind();

        if (this.gvDevices.Rows.Count > 0)
        {
            pnlDevices.Visible = true;

            InsDataContext idc = new InsDataContext();

            var passedDrift = (from di in idc.DeviceInventories
                               where
                                   di.CurrentGroupID  == int.Parse (ddlGroupName.SelectedValue) &&
                                   (di.DeviceAnalysisStatus.DeviceAnalysisName == "Drift" ||
                                   di.DeviceAnalysisStatus.DeviceAnalysisName == "Passed-Drift" ||                                   
                                    di.DeviceAnalysisStatus.DeviceAnalysisName == "Committed")
                               select di).Count();

            if (passedDrift > 0)
            {
                this.InventoryToolBar.Visible = true;
            }
        }
        else
        {
            pnlDevices.Visible = false;
        }
    }

    protected void btnPerformDriftAnalysis_Click(object sender, EventArgs e)
    {
        this.InventoryToolBar.Visible = true;

        // Execute a drift analysis
        int response = DriftAnalysis.Execute(ddlGroupName.SelectedItem.Text, HttpContext.Current.User.Identity.Name.ToString());

        if (response == 0)
            this.VisibleMessages("The drift analysis have been performed.");            
        else
            this.VisibleErrors(string.Format("Error {0}: An error occurring during drift analysis.", response));
        
        Rerender();
    }

    protected void btnResetGroup_Click(object sender, EventArgs e)
    {
        // Reset the group in the event it needs to be adjusted.
        int response = DriftAnalysis.ResetGroup(ddlGroupName.SelectedItem.Text);

        if (response == 0)
            this.VisibleMessages("The group has been reset.");
        else
            this.VisibleErrors(string.Format("Error {0}: An error occurring during resetting the group.", response));

        Rerender();
    }

    protected void btnAccept_Click(object sender, EventArgs e)
    {
        // Accept the group
        int response = DriftAnalysis.AcceptGroup(ddlGroupName.SelectedItem.Text);

        if (response == 0)
            this.VisibleMessages("The group has been accepted.");
        else
            this.VisibleErrors(string.Format("Error {0}: An error occurring during accepting the group.", response));

        Rerender();
    }

    protected void btnCommitInventory_Click(object sender, EventArgs e)
    {
        // Commit the inventory
        int response = DriftAnalysis.CommitToMAS(ddlGroupName.SelectedItem.Text);

        if (response == 0)
            this.VisibleMessages("The group has been committed to inventory.");
        else
            this.VisibleErrors(string.Format("Error {0}: An error occurring during committing the group to inventory.", response));

        Rerender();
    }

    protected void btnDriftReport_Click(object sender, EventArgs e)
    {
        // Load the Drift report
        ActuateReport report = new ActuateReport("rpt_driftCalc");
        report.Arguments.Add("pGroupName", ddlGroupName.SelectedItem.Text);

        // Launch the report
        OpenWindow(report.GetReportUri(), "_blank", "menubar=1,resizable=1,scrollbars=1,width=1024,height=600");
    }

    protected void btnInventoryReport_Click(object sender, EventArgs e)
    {
        // Load the Drift Commit Inventory report
        ActuateReport report = new ActuateReport("rpt_driftcommitInv");
        report.Arguments.Add("pGroupName", ddlGroupName.SelectedItem.Text);

        // Launch the report
        OpenWindow(report.GetReportUri(), "_blank", "menubar=1,resizable=1,scrollbars=1,width=1024,height=600");
    }
   
    private void Rerender()
    {
        gvDevices.DataBind();
    }


    private void OpenWindow(string url, string target, string features)
    {
        if (target == null) target = string.Empty;
        if (features == null) features = string.Empty;
        string script = string.Format("window.open(\"{0}\", \"{1}\", \"{2}\");", url, target, features);

        ScriptManager.RegisterClientScriptBlock(Page, typeof(Page), "OpenWindow", script, true);
    }

    private void InvisibleErrors()
    {
        // Reset submission form error message      
        this.errorMsg.InnerText = "";
        this.errors.Visible = false;
    }

    private void VisibleErrors(string error)
    {
        this.errorMsg.InnerText = error;
        this.errors.Visible = true;
    }

    private void InvisibleMessages()
    {
        // Reset submission form error message      
        this.submitMsg.InnerText = "";
        this.messages.Visible = false;
    }

    private void VisibleMessages(string error)
    {
        this.submitMsg.InnerText = error;
        this.messages.Visible = true;
    }
    
}
