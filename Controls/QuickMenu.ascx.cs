using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Instadose;
using Instadose.Data;

public partial class Controls_QuickMenu : System.Web.UI.UserControl
{
    string[] activeDirectoryGroups = { "IRV-IT", "IRV-CustomerSvc", "IRV-Client Services" };

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            AppDataContext adc = new AppDataContext();

            // Query active directory to see what groups this user belongs to.
            List<String> userGroups = ActiveDirectoryQueries.GetADUserGroups(Page.User.Identity.Name);

            // User group name is the key in the if_Groups table to get the list of of links
            string userGroupName = "General";

            // Determine the group name mapping for each user group.
            if (userGroups.Contains("IRV-CustomerSvc") || userGroups.Contains("IRV-Client Services")) userGroupName = "IRV-CustomerSvc";  // Customer Service
            else if (userGroups.Contains("IRV-IT")) userGroupName = "IRV-IT";   // IT
            else if (userGroups.Contains("IRV-TechSupport-RW")) userGroupName = "IRV-TechSupport-RW"; // Tech Ops
            else if (userGroups.Contains("IRV-Accounting-RW")) userGroupName = "IRV-Accounting-RW"; // Accounting/Finance

            // Select the department name from the group name provided 
            lblDepartmentName.Text = (from g in adc.if_Groups where g.GroupName == userGroupName select g.Department).FirstOrDefault() + " Links";

            // Query the links for the user's group.
            bulletList.DataSource = (from gl in adc.if_GroupLinks
                                     join g in adc.if_Groups on gl.GroupID equals g.GroupID
                                     join l in adc.if_Links on gl.LinkID equals l.LinkID
                                     where g.GroupName == userGroupName
                                     orderby l.FileDescription
                                     select new { Title = l.FileDescription, Link = l.FilePath });

            // Bind the elements
            bulletList.DataBind();

            // Display this control only if there are items to display.
            this.Visible = (bulletList.Items.Count != 0);
        }
        catch(Exception ex)
        {
            Basics.WriteLogEntry(ex.Message, Page.User.Identity.Name, 
                "QuickMenu.ascx", Basics.MessageLogType.Critical);
        }

    }
}