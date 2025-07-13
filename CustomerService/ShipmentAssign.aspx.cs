using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Collections.Generic;

using Instadose;
using Instadose.Data;

using System.Globalization;
using System.Threading;

public partial class CustomerService_ShipmentAssign : System.Web.UI.Page
{

    // String to hold the current username
    string UserName = "Unknown";

    // Create the database reference
    InsDataContext idc = new InsDataContext();
    AppDataContext adc = new AppDataContext();

    protected void Page_PreRenderComplete(object sender, EventArgs e)
    {

        this.lbl_openshipment.Text = (gv_openshipment.Rows.Count > 0) ? (gv_openshipment.Rows.Count.ToString() + " user shipments found") : "";       
       
        // display users record count
        this.lbl_shipUser.Text = gv_shipUser.Rows.Count > 0 ? (gv_shipUser.Rows.Count.ToString() + " users found") : "";
        
        // display assign shipment to User button 
        this.btn_assignShipTo.Visible = gv_shipUser.Rows.Count < 1 ? false : true;

    }

    protected void Page_Load(object sender, EventArgs e)
    {
        // Try to set the username
        try
        {
            this.UserName = User.Identity.Name.Split('\\')[1];
        }
        catch { this.UserName = "Unknown"; }

        if (!IsPostBack)
        {  

            //Find first date of the week
            CultureInfo info = Thread.CurrentThread.CurrentCulture;
            DayOfWeek firstday = info.DateTimeFormat.FirstDayOfWeek;
            DayOfWeek today = info.Calendar.GetDayOfWeek(DateTime.Now);

            int diff = today - firstday;
            DateTime firstDate = DateTime.Now.AddDays(-diff);
            
            this.txt_Startdate.Text = firstDate.ToShortDateString();
            this.txt_Enddate.Text = DateTime.Now.ToShortDateString();
        }


        if (gv_shipUser.Rows.Count < 1)
        {
            this.btn_assignShipTo.Visible = false;

        }
        else
        { 
            this.btn_assignShipTo.Visible = true;
        }        

        // --- only for LP to view the delete button.
        if (this.gv_openshipment.Rows.Count < 1 || UserName.IndexOf("lpmorgan") < 0 || UserName.IndexOf("tdo") < 0)
        //if (this.gv_openshipment.Rows.Count < 1)
        {
            this.btn_deleteUser.Visible = false;
        }
        else 
        { this.btn_deleteUser.Visible = true; }

    }

    protected void btn_deleteUser_Click(object sender, EventArgs e)
    {
        int selectUserCount = 0;
        
        //find if checkbox being checked 
        for (int i = 0; i < this.gv_openshipment.Rows.Count; i++)
        {
            GridViewRow gvRow = gv_openshipment.Rows[i];
            CheckBox findChkBx = (CheckBox)gvRow.FindControl("cbxSelectDelete");
            bool SelectUserDelete = findChkBx.Checked;

            // delete record, if checkbox checked.
            if (SelectUserDelete == true)
            { 
                int SelectUserID =int.Parse(gvRow.Cells[1].Text.Replace("\n", "").ToString().Trim());
                int SelectRefID = int.Parse(gvRow.Cells[14].Text.Replace("\n", "").ToString().Trim());

                try 
                {
                   
                    if_UserShipAssign shipAssign = (from r in adc.if_UserShipAssigns 
                                                     where r.UserShipAssignID == SelectRefID
                                                     select r).Single();
                    adc.if_UserShipAssigns.DeleteOnSubmit(shipAssign);
                    adc.SubmitChanges();
                    selectUserCount += 1;
                }
                
                catch 
                { }
                
            }
        }

        if (selectUserCount > 0)
        {
            // rebind GridView
            this.gv_openshipment.DataBind();
            this.gv_orderShipHistory.DataBind();

            // --- only for LP to view the delete button.
            if (this.gv_openshipment.Rows.Count < 1 || UserName.IndexOf("lpmorgan") < 0 || UserName.IndexOf("tdo") < 0)
            //if (this.gv_openshipment.Rows.Count < 1 )
            {
                this.btn_deleteUser.Visible = false;
            }
            else
            { this.btn_deleteUser.Visible = true; }

        }

    }
  
    protected void btn_assignShipTo_Click(object sender, EventArgs e)
    {  
        int selectUserCount = 0;
        
        //find if checkbox being checked 
        for (int i = 0; i < this.gv_shipUser.Rows.Count; i++)
        {
            GridViewRow gvRow = gv_shipUser.Rows[i];
            CheckBox findChkBx = (CheckBox)gvRow.FindControl("cbxSelect");
            bool SelectUser = findChkBx.Checked;

            // insert record, if checkbox checked.
            if (SelectUser == true)
            {
                string SelectUserID = gvRow.Cells[1].Text.Replace("\n", "").ToString().Trim();
                int myUserID = int.Parse(SelectUserID);
                
                //check if UserID already in Queue
                int ShipUserCount = (from r in adc.if_UserShipAssigns 
                                     where r.UserID== myUserID && r.ShipID ==null
                                     select r).Count();

                // userID not in shipping Queue, add new shipping order
                if (ShipUserCount == 0)
                {
                    if_UserShipAssign  OrdAssign = null;

                    OrdAssign = new if_UserShipAssign ()
                    {
                        UserID = int.Parse(SelectUserID),
                        AssignedBy = this.UserName,
                        AssignedDate = DateTime.Now

                    };

                    adc.if_UserShipAssigns.InsertOnSubmit(OrdAssign);
                    adc.SubmitChanges();

                    //count record being added
                    selectUserCount += 1;
                }

            }
        }

        if (selectUserCount > 0)
        {
            // rebind GridView
            this.gv_openshipment.DataBind();
            this.gv_orderShipHistory.DataBind();
        }
        
    }
    protected void btn_findAccountID_Click(object sender, EventArgs e)
    {
        gv_shipUser.Visible = true;
        
        this.gv_shipUser.DataBind();

        if (gv_shipUser.Rows.Count < 1)
        {
            this.btn_assignShipTo.Visible = false;
        }
        else
        { 
            this.btn_assignShipTo.Visible = true;
        }
    }


    public bool  DisplaySelectCheckBox(object myObj)
    {
        
        bool MyReturn = true;
        
        if (myObj != null)
            MyReturn = (myObj.ToString().ToUpper() == "YES") ? false : true;

        return MyReturn;

    }

    protected void btn_refreshQueue_Click(object sender, EventArgs e)
    {
        gv_openshipment.DataBind();
    }

    protected void btn_refreshRequest_Click(object sender, EventArgs e)
    {
        gv_shipUser.DataBind();
    }

    protected void btn_refreshHistory_Click(object sender, EventArgs e)
    {
        gv_orderShipHistory.DataBind();

    }










   
}
