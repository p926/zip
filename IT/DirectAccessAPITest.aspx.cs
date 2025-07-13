using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Instadose.API;
using Instadose.API.DA;

public partial class IT_DirectAccessAPITest : System.Web.UI.Page
{
    protected void btnGo_Click(object sender, EventArgs e)
    {
        /////
        ///// STEP 2.
        /////

        // Convert the textbox value into a number.
        int accountID = 0;
        if (!int.TryParse(txtAccountNo.Text, out accountID)) return;

        // Construct a Direct Access Locations object to query the lists.
        DALocations daLocations = new DALocations();

        // Return a list of locations for a specific account.
        LocationList list = daLocations.List(accountID, null);

        // Bind the grid with the locations data source.
        gvLocations.DataSource = list.Locations;
        gvLocations.DataBind();

        // Step 2 is displayed.
        pnlStep2.Visible = true;

        // Clear the groups data source.
        gvGroups.DataSource = null;
        gvGroups.DataBind();
    }

    protected void gvLocations_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        /////
        ///// STEP 3.
        /////

        // If the Button to display the group is 
        // pressed perform the following action.
        if (e.CommandName == "DisplayGroup")
        {
            // Convert the textbox value into a number.
            int accountID = 0;
            if (!int.TryParse(txtAccountNo.Text, out accountID)) return;

            // Get the selected row index.
            int rowIndex = 0;
            if (!int.TryParse(e.CommandArgument.ToString(), out rowIndex)) return;

            // Get the location ID by selecting the cell the LocationID exists in.
            int locationID = 0;
            if (!int.TryParse(gvLocations.Rows[rowIndex].Cells[1].Text, out locationID)) return;
            
            // Construct a Direct Access Groups object to query the groups.
            DAGroups daGroups = new DAGroups();

            // Return a list of the groups under this location.
            GroupList list = daGroups.List(accountID, locationID, null);

            // Bind the grid with the groups data source.
            gvGroups.DataSource = list.Groups;
            gvGroups.DataBind();

            // Step 3 is displayed.
            pnlStep3.Visible = true;
        }
    }
}