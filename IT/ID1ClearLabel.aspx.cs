using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Instadose;
using Instadose.Data;

public partial class IT_ID1ClearLabel : System.Web.UI.Page
{
    InsDataContext idc = new InsDataContext();

    protected void Page_Load(object sender, EventArgs e)
    {        
        InvisibleErrors();
        InvisibleSuccess();                        
    }

    private void InitiateControls(int pAcctID)
    {
        var acctName = (from a in idc.Accounts
                        where a.AccountID == pAcctID
                        select a.AccountName).FirstOrDefault();

        txtAccountName.Text = string.IsNullOrEmpty(acctName) ? "" : acctName;        

        var locs = from l in idc.Locations 
                     where l.Active == true
                     && l.AccountID == pAcctID
                     orderby l.LocationName 
                     select new
                     {
                         l.LocationID ,
                         l.LocationName 
                     };

        this.ddlUserLocation.DataSource = locs;
        this.ddlUserLocation.DataTextField = "LocationName";
        this.ddlUserLocation.DataValueField = "LocationID";
        this.ddlUserLocation.DataBind();

        if (locs.Count() > 0)
        {
            ListItem item0 = new ListItem("All", "0");
            this.ddlUserLocation.Items.Insert(0, item0);
        }
        
    }

    protected void txtAccountID_TextChanged(object sender, EventArgs e)
    {
        Int32 actID = 0;
        if (Int32.TryParse(this.txtAccountID.Text, out actID))
            InitiateControls(actID);
        else
            InitiateControls(0);                
    }

    protected void ddlUserLocation_SelectedIndexChanged(object sender, EventArgs e)
    {
        this.grdUsers.DataBind();
    }

    protected void btnPrint_Click(object sender, EventArgs e)
    {
        try
        {
            string labelName = "";
            string labelQuantity;
            string labelNameStr, labelQuantityStr, printSetStr, printerStr;
            bool standardLabel = true;

            if (this.ddlLabelFormat.SelectedValue == "1")
            {
                labelName = (standardLabel == true) ? "ID1ClearLabel_FirstAndLast.alf" : "ID1ClearLabel_FirstAndLast.alf";
            }

            if (this.ddlLabelFormat.SelectedValue == "2")
            {
                labelName = (standardLabel == true) ? "ID1ClearLabel_CompanyAndFullname.alf" : "ID1ClearLabel_CompanyAndFullname.alf";
            }

            if (this.ddlLabelFormat.SelectedValue == "3")
            {
                labelName = (standardLabel == true) ? "ID1ClearLabel_CompanyOnly.alf" : "ID1ClearLabel_CompanyOnly.alf";
            }
            

            int quantity = 1;
            labelQuantity = quantity.ToString();

            labelNameStr = "LABELNAME=" + labelName;
            labelQuantityStr = "LABELQUANTITY=" + labelQuantity;
            printSetStr = "PRINTSET=5,16,16,16,12,0,0,0,2G";
            printerStr = "PRINTER=QuickLabel Kiaro;Kiaro!";

            string curTime = DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString();
            string filename = "id1ClearLabel" + "_" + DateTime.Now.Ticks.ToString() + ".acf";

            string commandFilePath = @"\\irv-sapl-file1.mirion.local\POReceipt$\PrintTemplates\QuickCommand\MyCommandFiles\" + filename;
            string monitorFilePath = @"\\irv-sapl-file1.mirion.local\POReceipt$\PrintTemplates\QuickCommand\Monitor\" + filename;

            //IQueryable<User> userList = (from a in idc.Users
            //                             where a.AccountID == Int32.Parse(txtAccountID.Text)
            //                             && a.Active == true
            //                             orderby a.FirstName , a.LastName 
            //                             select a).AsQueryable();

            //foreach (User u in userList)
            //{
            //    file.WriteLine(labelNameStr);
            //    file.WriteLine("Firstname=" + "\"" + u.FirstName + "\"");
            //    file.WriteLine("Lastname=" + "\"" + u.LastName + "\"");
            //    file.WriteLine(labelQuantityStr);
            //}

            // set data based upon diff format
            if (this.ddlLabelFormat.SelectedValue == "1")
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(commandFilePath))
                {
                    for (int i = 0; i < this.grdUsers.Rows.Count; i++)
                    {
                        GridViewRow gvRow = grdUsers.Rows[i];
                        CheckBox findChkBx = (CheckBox)gvRow.FindControl("chkbxSelect");
                        bool SelectedSerial = findChkBx.Checked;

                        if (SelectedSerial == true)
                        {
                            string firstname = gvRow.Cells[1].Text.Replace("\n", "").ToString().Trim();
                            string lastname = gvRow.Cells[3].Text.Replace("\n", "").ToString().Trim();

                            file.WriteLine(labelNameStr);
                            file.WriteLine("Firstname=" + "\"" + firstname + "\"");
                            file.WriteLine("Lastname=" + "\"" + lastname + "\"");
                            file.WriteLine(labelQuantityStr);
                        }
                    }

                    // printset and printer string will be put at the end of file
                    file.WriteLine(printSetStr);
                    file.WriteLine(printerStr);
                }
            }

            if (this.ddlLabelFormat.SelectedValue == "2")
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(commandFilePath))
                {
                    for (int i = 0; i < this.grdUsers.Rows.Count; i++)
                    {
                        GridViewRow gvRow = grdUsers.Rows[i];
                        CheckBox findChkBx = (CheckBox)gvRow.FindControl("chkbxSelect");
                        bool SelectedSerial = findChkBx.Checked;

                        if (SelectedSerial == true)
                        {
                            string firstname = gvRow.Cells[1].Text.Replace("\n", "").ToString().Trim();
                            string lastname = gvRow.Cells[3].Text.Replace("\n", "").ToString().Trim();
                            string fullname = firstname + " " + lastname;
                            string companyname = this.txtAccountName.Text;

                            file.WriteLine(labelNameStr);
                            file.WriteLine("Companyname=" + "\"" + companyname + "\"");
                            file.WriteLine("Fullname=" + "\"" + fullname + "\"");
                            file.WriteLine(labelQuantityStr);
                        }
                    }

                    // printset and printer string will be put at the end of file
                    file.WriteLine(printSetStr);
                    file.WriteLine(printerStr);
                }
            }

            if (this.ddlLabelFormat.SelectedValue == "3")
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(commandFilePath))
                {
                    for (int i = 0; i < this.grdUsers.Rows.Count; i++)
                    {
                        GridViewRow gvRow = grdUsers.Rows[i];
                        CheckBox findChkBx = (CheckBox)gvRow.FindControl("chkbxSelect");
                        bool SelectedSerial = findChkBx.Checked;

                        if (SelectedSerial == true)
                        {
                            string firstname = gvRow.Cells[1].Text.Replace("\n", "").ToString().Trim();
                            string lastname = gvRow.Cells[3].Text.Replace("\n", "").ToString().Trim();
                            string fullname = firstname + " " + lastname;
                            string companyname = this.txtAccountName.Text;

                            file.WriteLine(labelNameStr);
                            file.WriteLine("Companyname=" + "\"" + companyname + "\"");                            
                            file.WriteLine(labelQuantityStr);
                        }
                    }

                    // printset and printer string will be put at the end of file
                    file.WriteLine(printSetStr);
                    file.WriteLine(printerStr);
                }
            }
            

            // copy command file from CommandFiles directory to Monitor directory
            if (File.Exists(commandFilePath))
            {
                File.Copy(commandFilePath, monitorFilePath, true);
            }

            VisibleSuccess("Print job successful.");
        }
        catch (Exception ex)
        {
            VisibleErrors(ex.ToString());
        }
    }

    protected void btnReset_Click(object sender, EventArgs e)
    {
        this.txtAccountID.Text = "";
        this.txtAccountID_TextChanged(null, null);
    }

    private void InvisibleErrors()
    {
        // Reset submission form error message      
        this.dialogErrorMsg.InnerText = "";
        this.dialogErrors.Visible = false;
    }

    private void VisibleErrors(string error)
    {
        this.dialogErrorMsg.InnerText = error;
        this.dialogErrors.Visible = true;
    }

    private void InvisibleSuccess()
    {
        // Reset submission form error message      
        this.dialogSuccessMsg.InnerText = "";
        this.dialogSuccess.Visible = false;
    }

    private void VisibleSuccess(string error)
    {
        this.dialogSuccessMsg.InnerText = error;
        this.dialogSuccess.Visible = true;
    }
    
    
}