/*
 * Maintain PO Receipts
 * 
 *  Created By: Tdo, 10/16/2012
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

public partial class Shipping_POReceipt : System.Web.UI.Page
{
    // Turn on/off email to all others
    bool DevelopmentServer = false;

    InsDataContext idc = new InsDataContext();
    DataTable dtNewBoxDetail;   // DataTable to store the receipt box detail input
    DataTable dtNewDetail;   // DataTable to store the receipt detail input
    DataTable dtItemNumbers; // DataTable to store the ItemNumber by PO Number
    string UserName = "Unknown";
    bool belongsToGroups = false;
    const string ID1PlugItemNumber = "PCBA ID PLUS";
    const string ID2ItemNumber = "PCBA ID2";
    const string ID1ItemNumber = "PCBA";

    protected void Page_Load(object sender, EventArgs e)
    {
        try
        {
            /// <summary>
            /// Limits access to the functionality based on Active Directory Groups.
            /// </summary>
            string[] ActiveDirecoryGroups = { "IRV-IT", "IRV-Instadose Technical" };
            belongsToGroups = ActiveDirectoryQueries.UserExistsInAnyGroup(User.Identity.Name, ActiveDirecoryGroups);

            // Auto set if a development site
            if (Request.Url.Authority.ToString().Contains(".mirioncorp.com") || Request.Url.Authority.ToString().Contains("localhost"))
            {
                DevelopmentServer = true;
                belongsToGroups = true;
            }
                            
            if (User.Identity.Name.IndexOf('\\') > 0)
                this.UserName = User.Identity.Name.Split('\\')[1];
            else
                this.UserName = "Testing";

            if (!this.IsPostBack)
            {
                InitiateAllControls();
                SetControlsDefault();
            }

        }
        catch { this.UserName = "Unknown"; }

    }

    private void SetControlsDefault()
    {
    }

    private void InitiateAllControls()
    {
        InitiatePONumberControl();
        InitiateVendorSearchControl();
    }

    private void InitiatePONumberControl()
    {
        DataTable myOpenPOs = GetAllOpenPONumber();

        this.ddlPONumber.DataSource = myOpenPOs;
        this.ddlPONumber.DataTextField = "PurchaseOrderNumber";
        this.ddlPONumber.DataValueField = "PurchaseOrderNumber";
        this.ddlPONumber.DataBind();

        ListItem item0 = new ListItem("", "0000000");
        this.ddlPONumber.Items.Insert(0, item0);
    }

    private void InitiateVendorSearchControl()
    {
        DataTable myVendorNumber = GetAllVendorNumber();

        this.ddlVendorSearch.DataSource = myVendorNumber;
        this.ddlVendorSearch.DataTextField = "VendorNumber";
        this.ddlVendorSearch.DataValueField = "VendorNumber";
        this.ddlVendorSearch.DataBind();

        ListItem item0 = new ListItem("", " ");
        this.ddlVendorSearch.Items.Insert(0, item0);
    }

    protected void ddlPONumber_SelectedIndexChanged(object sender, EventArgs e)
    {
        string selPONumber = this.ddlPONumber.SelectedItem.Value;
        string vendorNumber = "";
        string vendorName = "";

        GetVendorNumberByPONumber(selPONumber, ref vendorNumber, ref vendorName);

        this.txtVendor.Text = vendorNumber;
        this.txtVendorName.Text = vendorName;
    }

    private DataTable GetAllOpenPONumber()
    {
        try
        {
            String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            String cmdStr = "sp_GetAllOpenPOs";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);

            sqlConn.Close();

            return dt;
        }
        catch { return null; }
    }

    private void GetVendorNumberByPONumber(string pPONumber, ref string pVendorNumber, ref string pVendorName)
    {
        try
        {
            String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            String cmdStr = "sp_GetVendorNumberByPONumber";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.Add("@PONumber", SqlDbType.NVarChar, 7);

            sqlCmd.Parameters["@PONumber"].Value = pPONumber;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);

            sqlConn.Close();

            if (dt != null)
            {
                pVendorNumber = (dt.Rows[0]["VendorNumber"] != null) ? dt.Rows[0]["VendorNumber"].ToString() : "";
                pVendorName = (dt.Rows[0]["VendorName"] != null) ? dt.Rows[0]["VendorName"].ToString() : "";
            }
            else
            {
                pVendorNumber = "";
                pVendorName = "";
            }
        }
        catch
        {
            pVendorNumber = "";
            pVendorName = "";
        }

    }

    private DataTable GetAllItemNumberByPONumber(string pPONumber)
    {
        try
        {
            String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            String cmdStr = "sp_GetAllItemNumberByPONumber";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.Add("@PONumber", SqlDbType.NVarChar, 7);

            sqlCmd.Parameters["@PONumber"].Value = pPONumber;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);

            sqlConn.Close();

            return dt;
        }
        catch { return null; }
    }

    private DataTable GetAllVendorNumber()
    {
        try
        {
            String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            String cmdStr = "sp_GetAllVendorNumber";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);

            sqlConn.Close();

            return dt;
        }
        catch { return null; }
    }




    // ------------------------- Modal Dialog Section FUNCTIONS here ------------------------------------- //

    private void InvisibleErrors_poReceiptDialog()
    {
        // Reset submission form error message      
        this.poReceiptDialogErrorMsg.InnerText = "";
        this.poReceiptDialogError.Visible = false;
    }

    private void VisibleErrors_poReceiptDialog(string error)
    {
        this.poReceiptDialogErrorMsg.InnerText = error;
        this.poReceiptDialogError.Visible = true;
    }

    private void InvisibleMessages_poReceiptDialog()
    {
        // Reset submission form error message      
        this.submitMsg.InnerText = "";
        this.messages.Visible = false;
    }

    private void VisibleMessages_poReceiptDialog(string error)
    {
        this.submitMsg.InnerText = error;
        this.messages.Visible = true;
    }

    private bool passInputsValidation_poReceiptDialog()
    {
        string errorString = "";
        int myInt;
        DateTime myDate;

        if (this.ddlPONumber.SelectedItem.Text == "")
        {
            errorString = "PO# is required.";
            this.VisibleErrors_poReceiptDialog(errorString);
            SetFocus(this.ddlPONumber);
            return false;
        }

        if (this.txtVendor.Text.Trim().Length == 0)
        {
            errorString = "Vendor is required.";
            this.VisibleErrors_poReceiptDialog(errorString);
            SetFocus(this.txtVendor);
            return false;
        }

        if (this.txtShipmentID.Text.Trim().Length == 0)
        {
            errorString = "Shipment ID is required.";
            this.VisibleErrors_poReceiptDialog(errorString);
            SetFocus(this.txtShipmentID);
            return false;
        }
        else
        {
            if ((this.txtReceiptID.Text.Trim().Length == 0) && IsShipmentIDDuplicate(this.txtShipmentID.Text.Trim()))
            {
                errorString = "Shipment ID can not be duplicated. Re-enter Shipment ID";
                this.VisibleErrors_poReceiptDialog(errorString);
                SetFocus(this.txtShipmentID);
                return false;
            }
        }

        if (this.txtReceiptDate.Text.Trim().Length == 0)
        {
            errorString = "Receipt Date is required.";
            this.VisibleErrors_poReceiptDialog(errorString);
            SetFocus(this.txtReceiptDate);
            return false;
        }
        else
        {
            if (DateTime.TryParse(this.txtReceiptDate.Text.Trim(), out myDate) == false)
            {
                errorString = "Receipt Date is incorrect format. Re-enter Receipt Date.";
                this.VisibleErrors_poReceiptDialog(errorString);
                SetFocus(this.txtReceiptDate);
                return false;
            }
        }

        if (this.txtCount.Text.Trim().Length == 0)
        {
            errorString = "Box Count is required.";
            this.VisibleErrors_poReceiptDialog(errorString);
            SetFocus(this.txtCount);
            return false;
        }
        else
        {
            if (int.TryParse(this.txtCount.Text.Trim(), out myInt) == false)
            {
                errorString = "Box Count must be a number. Re-enter Box Count.";
                this.VisibleErrors_poReceiptDialog(errorString);
                SetFocus(this.txtCount);
                return false;
            }
            else
            {
                if (myInt <= 0)
                {
                    errorString = "Box Count must be greater than 0. Re-enter Box Count.";
                    this.VisibleErrors_poReceiptDialog(errorString);
                    SetFocus(this.txtCount);
                    return false;
                }
            }
        }

        if (this.txtPackingSlipTotal.Text.Trim().Length == 0)
        {
            errorString = "Qty Total is required.";
            this.VisibleErrors_poReceiptDialog(errorString);
            SetFocus(this.txtPackingSlipTotal);
            return false;
        }
        else
        {
            if (int.TryParse(this.txtPackingSlipTotal.Text.Trim(), out myInt) == false)
            {
                errorString = "Qty Total must be a number. Re-enter Qty Total.";
                this.VisibleErrors_poReceiptDialog(errorString);
                SetFocus(this.txtPackingSlipTotal);
                return false;
            }
            else
            {
                if (myInt <= 0)
                {
                    errorString = "Qty Total must be greater than 0. Re-enter Qty Total.";
                    this.VisibleErrors_poReceiptDialog(errorString);
                    SetFocus(this.txtPackingSlipTotal);
                    return false;
                }
            }
        }

        if (ValidateReceiptDetailRow(ref errorString) == false)
        {
            this.VisibleErrors_poReceiptDialog(errorString);
            return false;
        }

        if (ValidateTotalReceived(ref errorString) == false)
        {
            this.VisibleErrors_poReceiptDialog(errorString);
            return false;
        }

        return true;
    }

    private void EnableControls(bool flag)
    {
        this.ddlPONumber.Enabled = flag;
        this.txtShipmentID.Enabled = flag;
        this.txtReceiptDate.Enabled = flag;
        this.txtCount.Enabled = flag;
        this.txtPackingSlipTotal.Enabled = flag;
        this.grdViewEditDetail.Enabled = flag;
        this.btnMoreDetail.Enabled = flag;
        this.txtReceiptID.Enabled = false;
        this.txtVendor.Enabled = false;
        this.txtVendorName.Enabled = false;
    }

    private void VisibleControls(bool flag)
    {
        this.btnMoreDetail.Visible = flag;
        this.EditDetail.Visible = !flag;
        this.receiptID.Visible = !flag;
    }

    private void SetDefaultValues_poReceiptDialog()
    {
        this.ddlPONumber.SelectedIndex = 0;
        this.txtReceiptID.Text = "";
        this.txtShipmentID.Text = "";
        this.txtReceiptDate.Text = DateTime.Today.ToShortDateString();
        this.txtCount.Text = "";
        this.txtPackingSlipTotal.Text = "";
        this.txtVendor.Text = "";
        this.txtVendorName.Text = "";
        this.chkReceiveDoc.Checked = false;
        this.chkBoxLabel.Checked = false;
        this.grdViewEditDetail.DataSource = null;
        this.grdViewEditDetail.DataBind();
    }

    private void SetValuesToControls_poReceiptDialog()
    {

        string receiptID = (Session["selectedReceiptID"] != null) ? Convert.ToString(Session["selectedReceiptID"]) : "";

        SetFocus(this.ddlPONumber);

        if (receiptID.Length > 0)    // edit mode
        {
            POReceipt myPOReceipt = (from a in idc.POReceipts
                                     where a.ReceiptID == int.Parse(receiptID)
                                     select a).FirstOrDefault();

            if (myPOReceipt.UploadInMAS)
            {
                EnableControls(false);
                VisibleControls(false);
            }
            else
            {
                EnableControls(true);
                this.btnMoreDetail.Visible = true;
                this.EditDetail.Visible = true;
                this.receiptID.Visible = true;
            }

            this.ddlPONumber.SelectedValue = myPOReceipt.PONumber;
            this.txtReceiptID.Text = myPOReceipt.ReceiptID.ToString();
            this.txtShipmentID.Text = myPOReceipt.ShipmentID;
            this.txtReceiptDate.Text = myPOReceipt.ReceiptDate.ToShortDateString();
            this.txtCount.Text = myPOReceipt.BoxesRecd.ToString();
            this.txtPackingSlipTotal.Text = GetTotalPerPackingSlipByReceiptID(int.Parse(receiptID)).ToString();

            string vendorNumber = "";
            string vendorName = "";

            GetVendorNumberByPONumber(myPOReceipt.PONumber, ref vendorNumber, ref vendorName);
            this.txtVendorName.Text = vendorName;
            this.txtVendor.Text = myPOReceipt.Vendor;

            SetDetailGridByReceiptID(int.Parse(receiptID));
        }
        else
        {
            EnableControls(true);
            VisibleControls(true);

            this.chkReceiveDoc.Checked = true;
            this.chkBoxLabel.Checked = true;
            //EditBox, EditDetail visibility depends on the input of the number of boxes 
        }

    }

    protected void btnNewPOReceipt_Click(object sender, EventArgs e)
    {
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('poReceiptDialog')", true);
    }

    protected void btnCancelPOReceipt_Click(object sender, EventArgs e)
    {
        InvisibleErrors_poReceiptDialog();
        InvisibleMessages_poReceiptDialog();

        SetDefaultValues_poReceiptDialog();
        // delete session variable
        if (Session["selectedReceiptID"] != null)
            Session.Remove("selectedReceiptID");
    }

    protected void btnLoadPOReceipt_Click(object sender, EventArgs e)
    {
        SetDefaultValues_poReceiptDialog();
        SetValuesToControls_poReceiptDialog();
    }

    protected void btnEditPOReceipt_Click(object sender, EventArgs e)
    {
        LinkButton btn = (LinkButton)sender;
        string myCmdname = btn.CommandName.ToString();
        Session["selectedReceiptID"] = btn.CommandArgument.ToString();

        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "openDialog('poReceiptDialog')", true);
    }

    protected void btnAddPOReceipt_Click(object sender, EventArgs e)
    {
        InvisibleErrors_poReceiptDialog();
        InvisibleMessages_poReceiptDialog();

        if (passInputsValidation_poReceiptDialog())
        {

            bool editMode = (Session["selectedReceiptID"] != null) ? true : false;

            string myPONumber = this.ddlPONumber.SelectedValue;
            string myVendor = this.txtVendor.Text.Trim();
            int myReceiptID = (this.txtReceiptID.Text.Length > 0) ? int.Parse(this.txtReceiptID.Text) : 0;
            string myShipmentID = this.txtShipmentID.Text.Trim();
            DateTime myReceiptDate = DateTime.Parse(this.txtReceiptDate.Text.Trim());
            int myBoxesCount = int.Parse(this.txtCount.Text);
            string myAddUser = UserName;
            string myModUser = UserName;
            DateTime myAddDate = DateTime.Today;
            DateTime myModDate = DateTime.Today;
            bool myMASUpload = false;
            string curItemNumber;
            int curQtyRecd;
            int curQtyPerPackingSlip;
            int curReceiptDetailID;
            Hashtable boxesHash = new Hashtable();
            Hashtable receiptDetailIDHash = new Hashtable();

            if (!editMode)  // add new po receipt
            {
                try
                {
                    // Commit dbo.POReceipts record
                    POReceipt rec = new POReceipt
                    {
                        ReceiptDate = myReceiptDate,
                        ShipmentID = myShipmentID,
                        Vendor = myVendor,
                        PONumber = myPONumber,
                        BoxesRecd = myBoxesCount,
                        UploadInMAS = myMASUpload,
                        AddUser = myAddUser,
                        AddDate = myAddDate,
                        ModUser = myModUser,
                        ModDate = myModDate
                    };

                    idc.POReceipts.InsertOnSubmit(rec);
                    idc.SubmitChanges();

                    myReceiptID = rec.ReceiptID;


                    foreach (GridViewRow rowitem in grdViewEditDetail.Rows)
                    {
                        TextBox findQtyPerPackingSlip = (TextBox)rowitem.FindControl("txtRDQtyPerPackingSlip");
                        TextBox findQtyRecd = (TextBox)rowitem.FindControl("txtRDQtyRecd");
                        DropDownList findItemNumber = (DropDownList)rowitem.FindControl("ddlRDItemNumber");

                        curItemNumber = findItemNumber.SelectedValue;

                        if (!int.TryParse(findQtyPerPackingSlip.Text.Trim(), out curQtyPerPackingSlip))
                        {
                            curQtyPerPackingSlip = 0;
                        }
                        if (!int.TryParse(findQtyRecd.Text.Trim(), out curQtyRecd))
                        {
                            curQtyRecd = 0;
                        }

                        if (curItemNumber.Length > 0 && curQtyPerPackingSlip > 0 && curQtyRecd > 0)
                        {
                            // Commit dbo.POReceiptDetails record
                            POReceiptDetail rd = new POReceiptDetail
                            {
                                ReceiptID = myReceiptID,
                                ItemNumber = curItemNumber,
                                QtyPerPackingSlip = curQtyPerPackingSlip,
                                QtyRecd = curQtyRecd
                            };

                            idc.POReceiptDetails.InsertOnSubmit(rd);
                            idc.SubmitChanges();
                        }
                    }

                    // Sending email
                    SendEmail(myPONumber, myVendor, myReceiptID.ToString(), myReceiptDate.ToShortDateString());

                    // refresh gridview
                    grdPOReceiptsView.DataBind();

                    if (chkBoxLabel.Checked)
                        PrintBoxLabel(myReceiptID.ToString(), myBoxesCount.ToString());

                    if (chkReceiveDoc.Checked)
                        PrintPOReceipt(myReceiptID.ToString());


                    // seems like the close dialog functionality does not work when adding local printing function
                    ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('poReceiptDialog')", true);

                    VisibleMessages_poReceiptDialog("Commit successfully!");

                }
                catch (Exception ex)
                {
                    // Display the system generated error message.
                    this.VisibleErrors_poReceiptDialog(string.Format("An error occurred: {0}", ex.Message));
                }
            }
            else
            {
                try
                {
                    // Edit existing POReceipt
                    POReceipt rec = (from p in idc.POReceipts
                                     where p.ReceiptID == myReceiptID
                                     select p).FirstOrDefault();

                    if (rec != null)
                    {
                        if (!rec.UploadInMAS)
                        {
                            // Edit existing POReceipt
                            rec.ReceiptDate = myReceiptDate;
                            rec.ShipmentID = myShipmentID;
                            rec.Vendor = myVendor;
                            rec.PONumber = myPONumber;
                            rec.BoxesRecd = myBoxesCount;
                            rec.ModUser = myModUser;
                            rec.ModDate = myModDate;
                            idc.SubmitChanges();

                            // Add/Edit POReceiptDetail
                            foreach (GridViewRow rowitem in grdViewEditDetail.Rows)
                            {
                                Label findReceiptDetailID = (Label)rowitem.FindControl("lblReceiptDetailID");
                                TextBox findQtyPerPackingSlip = (TextBox)rowitem.FindControl("txtRDQtyPerPackingSlip");
                                TextBox findQtyRecd = (TextBox)rowitem.FindControl("txtRDQtyRecd");
                                DropDownList findItemNumber = (DropDownList)rowitem.FindControl("ddlRDItemNumber");

                                curItemNumber = findItemNumber.SelectedValue;

                                if (!int.TryParse(findQtyPerPackingSlip.Text.Trim(), out curQtyPerPackingSlip))
                                {
                                    curQtyPerPackingSlip = 0;
                                }
                                if (!int.TryParse(findQtyRecd.Text.Trim(), out curQtyRecd))
                                {
                                    curQtyRecd = 0;
                                }
                                if (!int.TryParse(findReceiptDetailID.Text.Trim(), out curReceiptDetailID))
                                {
                                    curReceiptDetailID = 0;
                                }

                                if (curItemNumber.Length > 0 && curQtyPerPackingSlip > 0 && curQtyRecd > 0)
                                {
                                    // Add POReceiptDetail
                                    if (curReceiptDetailID == 0)
                                    {
                                        POReceiptDetail rd = new POReceiptDetail
                                        {
                                            ReceiptID = myReceiptID,
                                            ItemNumber = curItemNumber,
                                            QtyPerPackingSlip = curQtyPerPackingSlip,
                                            QtyRecd = curQtyRecd
                                        };

                                        idc.POReceiptDetails.InsertOnSubmit(rd);
                                        idc.SubmitChanges();
                                    }
                                    else // Edit POReceiptDetail
                                    {
                                        POReceiptDetail rd = (from p in idc.POReceiptDetails
                                                              where p.ReceiptDetailID == curReceiptDetailID
                                                              select p).FirstOrDefault();
                                        if (rd != null)
                                        {
                                            rd.ItemNumber = curItemNumber;
                                            rd.QtyPerPackingSlip = curQtyPerPackingSlip;
                                            rd.QtyRecd = curQtyRecd;
                                            idc.SubmitChanges();
                                        }
                                    }
                                }
                            }
                        }


                        // refresh gridview
                        grdPOReceiptsView.DataBind();

                        // delete session variable
                        if (Session["selectedReceiptID"] != null)
                            Session.Remove("selectedReceiptID");

                        // ---------------------------- Printing section -----------------------------//                    
                        if (chkBoxLabel.Checked)
                            PrintBoxLabel(myReceiptID.ToString(), myBoxesCount.ToString());

                        if (chkReceiveDoc.Checked)
                            PrintPOReceipt(myReceiptID.ToString());

                        // seems like the close dialog functionality does not work when adding local printing function
                        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('poReceiptDialog')", true);
                        // ---------------------------- Printing section -----------------------------//

                        VisibleMessages_poReceiptDialog("Commit successfully!");

                    }

                }
                catch (Exception ex)
                {
                    // Display the system generated error message.
                    this.VisibleErrors_poReceiptDialog(string.Format("An error occurred: {0}", ex.Message));
                }

            }

        }

    }

    // ------------------------- Modal Dialog Section FUNCTIONS here ------------------------------------- //

    private void PrintPOReceipt(string pReceiptID)
    {
        string url = "POReceiptReport.aspx?receiptID=" + pReceiptID;
        ScriptManager.RegisterStartupScript(this, typeof(string), "OPEN_WINDOW", "window.open( '" + url + "', 'POReceipt', 'height=450,width=800,status=yes,toolbar=yes,menubar=no,location=no,resize=yes' );", true);
    }

    private void PrintBoxLabel(string pReceiptID, string pNCopies)
    {
        string functionCall = "executePrint(" + pReceiptID + "," + pNCopies + ")";
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, functionCall, true);
        //ClientScript.RegisterStartupScript(Page.GetType(), "clientScript", "<script type='text/javascript'>executePrint('500004')</script>");
    }

    protected void txtCount_TextChanged(object sender, EventArgs e)
    {
        int count;
        if (int.TryParse(this.txtCount.Text, out count) && count > 0)
        {
            // Populate the default Detail grid the first time user enters Box Count
            if (grdViewEditDetail.Rows.Count == 0)
            {
                GenerateEditDetailGrid(1);
            }
            this.EditDetail.Visible = true;
        }
        else
        {
            this.EditDetail.Visible = false;
        }
    }

    private void DefineDetailDataTable()
    {
        dtNewDetail = new DataTable();
        dtNewDetail.Columns.Add("ReceiptDetailID", typeof(string));
        dtNewDetail.Columns.Add("ItemNumber", typeof(string));
        dtNewDetail.Columns.Add("QtyPerPackingSlip", typeof(string));
        dtNewDetail.Columns.Add("QtyRecd", typeof(string));
    }

    protected void btnMoreDetail_Click(object sender, EventArgs e)
    {
        DataRow dtr;
        string curQtyPerPackingSlip, curItemNumber, curQtyRecd, curReceiptDetailID;
        try
        {

            DefineDetailDataTable();

            foreach (GridViewRow rowitem in grdViewEditDetail.Rows)
            {
                Label findReceiptDetailID = (Label)rowitem.FindControl("lblReceiptDetailID");
                TextBox findQtyPerPackingSlip = (TextBox)rowitem.FindControl("txtRDQtyPerPackingSlip");
                TextBox findQtyRecd = (TextBox)rowitem.FindControl("txtRDQtyRecd");
                DropDownList findItemNumber = (DropDownList)rowitem.FindControl("ddlRDItemNumber");

                curReceiptDetailID = findReceiptDetailID.Text;
                curQtyPerPackingSlip = findQtyPerPackingSlip.Text;
                curItemNumber = findItemNumber.SelectedValue;
                curQtyRecd = findQtyRecd.Text;

                dtr = dtNewDetail.NewRow();

                dtr["ReceiptDetailID"] = curReceiptDetailID;
                dtr["ItemNumber"] = curItemNumber; //findItemNumber.SelectedValue;
                dtr["QtyPerPackingSlip"] = curQtyPerPackingSlip;
                dtr["QtyRecd"] = curQtyRecd;
                // Add the row to the DataTable.
                dtNewDetail.Rows.Add(dtr);
            }

            dtr = dtNewDetail.NewRow();

            dtr["ReceiptDetailID"] = "";
            dtr["ItemNumber"] = "";
            dtr["QtyPerPackingSlip"] = "";
            dtr["QtyRecd"] = "";
            // Add the row to the DataTable.
            dtNewDetail.Rows.Add(dtr);

            this.grdViewEditDetail.DataSource = dtNewDetail;
            this.grdViewEditDetail.DataBind();

        }
        catch (Exception ex)
        {
        }
    }

    protected void grdViewEditDetail_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            System.Web.UI.WebControls.Label l = (System.Web.UI.WebControls.Label)e.Row.FindControl("lblRDItemNumber");
            DropDownList ddl = (DropDownList)e.Row.FindControl("ddlRDItemNumber");

            //DataTable dtItemNumbers = GetAllItemNumberByPONumber(this.ddlPONumber.SelectedItem.Value);
            if (dtItemNumbers == null)
                dtItemNumbers = GetAllItemNumberByPONumber(this.ddlPONumber.SelectedItem.Value);

            ddl.DataSource = dtItemNumbers;
            ddl.DataBind();

            ddl.SelectedValue = l.Text;
        }
    }

    private void GenerateEditDetailGrid(int count)
    {
        DefineDetailDataTable();

        for (int i = 0; i < count; i++)
        {
            DataRow dtr = dtNewDetail.NewRow();

            dtr["ReceiptDetailID"] = "";
            dtr["ItemNumber"] = "";
            dtr["QtyPerPackingSlip"] = "";
            dtr["QtyRecd"] = "";
            // Add the row to the DataTable.
            dtNewDetail.Rows.Add(dtr);
        }

        this.grdViewEditDetail.DataSource = dtNewDetail;
        this.grdViewEditDetail.DataBind();
    }

    private void SetDetailGridByReceiptID(int receiptID)
    {

        IQueryable pth = from a in idc.POReceiptDetails
                         where a.ReceiptID == receiptID
                         select new
                         {
                             a.ReceiptDetailID,
                             a.ItemNumber,
                             a.QtyPerPackingSlip,
                             a.QtyRecd
                         };

        grdViewEditDetail.DataSource = pth;
        grdViewEditDetail.DataBind();
    }

    private int GetTotalPerPackingSlipByReceiptID(int pReceiptID)
    {
        try
        {
            int sumPackingSlip = 0;

            sumPackingSlip = (from a in idc.POReceiptDetails
                              where a.ReceiptID == pReceiptID
                              select a.QtyPerPackingSlip).Sum();

            return sumPackingSlip;
        }
        catch
        {
            return 0;
        }
    }

    private bool IsShipmentIDDuplicate(string pShipmentID)
    {
        try
        {
            int count = (from a in idc.POReceipts where a.ShipmentID == pShipmentID select a).Count();

            if (count > 0) return true;

            return false;
        }
        catch
        {
            return false;
        }

    }

    private bool ValidateReceiptDetailRow(ref string pMessage)
    {
        string curQtyPerPackingSlip;
        string curItemNumber;
        string curQtyRecd;
        int qty;
        int boxCount = 0;

        try
        {
            foreach (GridViewRow rowitem in grdViewEditDetail.Rows)
            {
                TextBox findQtyPerPackingSlip = (TextBox)rowitem.FindControl("txtRDQtyPerPackingSlip");
                TextBox findQtyRecd = (TextBox)rowitem.FindControl("txtRDQtyRecd");
                DropDownList findItemNumber = (DropDownList)rowitem.FindControl("ddlRDItemNumber");

                curQtyPerPackingSlip = findQtyPerPackingSlip.Text.Trim();
                curItemNumber = findItemNumber.SelectedValue;
                curQtyRecd = findQtyRecd.Text.Trim();

                if (curQtyPerPackingSlip.Length <= 0 && curItemNumber.Length <= 0 && curQtyRecd.Length <= 0) continue;

                if (curQtyPerPackingSlip.Length <= 0 || curItemNumber.Length <= 0 || curQtyRecd.Length <= 0)
                {
                    pMessage = "Check Receipt Details Section. Item Number, Qty Per PackingSlip and Actual Qty Received must be provided.";
                    return false;
                }
                else
                {
                    if (int.TryParse(curQtyPerPackingSlip, out qty) == false)
                    {
                        pMessage = "Check Receipt Details Section. Qty Per PackingSlip must be a number.";
                        return false;
                    }
                    else
                    {
                        if (qty <= 0)
                        {
                            pMessage = "Check Receipt Details Section. Qty Per PackingSlip must be greater than zero.";
                            return false;
                        }
                        else
                        {
                            boxCount += 1;
                        }
                    }

                    if (int.TryParse(curQtyRecd, out qty) == false)
                    {
                        pMessage = "Check Receipt Details Section. Actual Qty Received must be a number.";
                        return false;
                    }
                    else
                    {
                        if (qty <= 0)
                        {
                            pMessage = "Check Receipt Details Section. Actual Qty Received must be greater than zero.";
                            return false;
                        }
                    }
                }
            }

            if (boxCount == 0)
            {
                pMessage = "Check Receipt Details Section. You must provide Receipt Details information.";
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            pMessage = ex.ToString();
            return false;
        }

    }

    private bool ValidateTotalReceived(ref string pMessage)
    {
        string curQtyPerPackingSlip;
        string curItemNumber;
        string curQtyRecd;
        int qty;
        int totalPerPackingSlip = 0;

        try
        {
            foreach (GridViewRow rowitem in grdViewEditDetail.Rows)
            {
                TextBox findQtyPerPackingSlip = (TextBox)rowitem.FindControl("txtRDQtyPerPackingSlip");
                TextBox findQtyRecd = (TextBox)rowitem.FindControl("txtRDQtyRecd");
                DropDownList findItemNumber = (DropDownList)rowitem.FindControl("ddlRDItemNumber");

                curQtyPerPackingSlip = findQtyPerPackingSlip.Text.Trim();
                curItemNumber = findItemNumber.SelectedValue;
                curQtyRecd = findQtyRecd.Text.Trim();

                if (curQtyPerPackingSlip.Length <= 0 && curItemNumber.Length <= 0 && curQtyRecd.Length <= 0) continue;

                if (curQtyPerPackingSlip.Length > 0 && curItemNumber.Length > 0 && curQtyRecd.Length > 0)
                {
                    if (int.TryParse(curQtyPerPackingSlip, out qty))
                    {
                        totalPerPackingSlip += qty;
                    }
                }
            }

            if (totalPerPackingSlip != int.Parse(this.txtPackingSlipTotal.Text))
            {
                pMessage = "Qty Total does not match with the total of Qty Per PackingSlip.";
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            pMessage = ex.ToString();
            return false;
        }

    }

    private void SendEmail(string pPONumber, string pVendor, string pReceiptID, string pReceiptDate)
    {
        string curItemNumber;
        int curQtyPerPackingSlip;
        int curQtyRecd;

        try
        {
            // Generate email content
            string bodyText = "<html><body>";

            bodyText += "Shipment is received by Shipping/Receiving Department. Details as below:";

            bodyText += "<br><font size='-1'>Receipt#: <b>" + pReceiptID + "</b></font>";

            bodyText += "<br><font size='-1'>Receipt Date: <b>" + pReceiptDate + "</b></font>";

            bodyText += "<br><font size='-1'>Vendor#: <b>" + pVendor + "</b></font>";

            bodyText += "<br><font size='-1'>PO#: <b>" + pPONumber + "</b></font>";

            bodyText += "<br><br><font size='-1'>Quantity by ItemNumber:</font>";


            foreach (GridViewRow rowitem in grdViewEditDetail.Rows)
            {
                TextBox findQtyPerPackingSlip = (TextBox)rowitem.FindControl("txtRDQtyPerPackingSlip");
                TextBox findQtyRecd = (TextBox)rowitem.FindControl("txtRDQtyRecd");
                DropDownList findItemNumber = (DropDownList)rowitem.FindControl("ddlRDItemNumber");

                curItemNumber = findItemNumber.SelectedValue;

                if (!int.TryParse(findQtyPerPackingSlip.Text.Trim(), out curQtyPerPackingSlip))
                {
                    curQtyPerPackingSlip = 0;
                }
                if (!int.TryParse(findQtyRecd.Text.Trim(), out curQtyRecd))
                {
                    curQtyRecd = 0;
                }

                if (curItemNumber.Length > 0 && curQtyPerPackingSlip > 0 && curQtyRecd > 0)
                {
                    bodyText += "<br><font size='-1'>" + curItemNumber + ": <b>" + curQtyRecd + "</b></font>";
                }

                curItemNumber = "";
                curQtyPerPackingSlip = 0;
                curQtyRecd = 0;
            }

            if (DevelopmentServer == false)
            {
                bodyText += "<br><br><br><br><font size='1'>* This email is automatically generated by Shipping/Receiving Department, from user: <b>" + this.UserName + "</b></font>";
            }
            else
            {
                bodyText += "<br><br><br><br><font size='1'>* This email is automatically generated by Shipping/Receiving Department for testing purpose only, from user: <b>" + this.UserName + "</b></font>";
            }

            bodyText += "</body></html>";

            // Send  email.
            string smtpServer = string.IsNullOrEmpty(System.Configuration.ConfigurationManager.AppSettings["SmtpServer"]) ? "10.200.2.16" : System.Configuration.ConfigurationManager.AppSettings["SmtpServer"];
            SmtpClient client = new SmtpClient(smtpServer);
            MailMessage mail = new MailMessage();
            mail.IsBodyHtml = true;

            mail.From = new MailAddress("noreply@instadose.com", "Shipping/Receiving Portal Email");
            mail.Subject = "PO Receive Notification";
            mail.Body = bodyText;

            // email recipients To or CC or Bcc 
            //string userEmail = this.UserName + "@mirion.com";
            //mail.To.Add(userEmail);

            if (DevelopmentServer == false)
            {
                mail.To.Add("DSD-PONotificationGroup@mirion.com");                
            }
            else
            {
                string userEmail = this.UserName + "@mirion.com";
                mail.To.Add(userEmail);
                //mail.CC.Add("gsubagyo@mirion.com");
                //mail.CC.Add("mgodha@mirion.com");  
                //mail.CC.Add(userEmail);
            }

            client.Send(mail);

        }
        catch { }
    }

    /// <summary>
    /// Only display the release link for ID1+ product. The rest is hidden.
    /// if the ID1+ receipt has not uploaded to MAS then displaying "N/A"
    /// else if already released then displaying "Released"
    /// else displaying "Release" link for user to perform the entire receipt to WIP
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    protected void grdPOReceiptsView_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        try
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                LinkButton lnkBtnReleaseToWIP = (LinkButton)e.Row.FindControl("btnReleaseToWIP");
                int receiptID = int.Parse(lnkBtnReleaseToWIP.CommandArgument);

                var receiptDetail = (from r in idc.POReceipts
                                     join rd in idc.POReceiptDetails
                                     on r.ReceiptID equals rd.ReceiptID
                                     where r.ReceiptID == receiptID
                                     select new { r.UploadInMAS, rd }).ToList();

                if (receiptDetail.Count() > 0)
                {
                    foreach (var d in receiptDetail)
                    {
                        bool alreadyMASUpload = d.UploadInMAS;
                        string itemNumber = d.rd.ItemNumber;

                        //if ((itemNumber.Contains(ID1PlugItemNumber) || itemNumber.Contains(ID2ItemNumber)) && belongsToGroups)
                        if (belongsToGroups)
                        {
                            lnkBtnReleaseToWIP.Visible = true;      

                            if (!alreadyMASUpload)
                            {
                                lnkBtnReleaseToWIP.Text = "N/A";
                                lnkBtnReleaseToWIP.Enabled = false;
                                break;   // exit the loop
                            }
                            else
                            {
                                if (IsAllInQA(receiptID))   // ALL in QA warehouse
                                {
                                    lnkBtnReleaseToWIP.Text = "Perform Release";
                                    lnkBtnReleaseToWIP.Enabled = true;
                                }
                                else
                                {
                                    lnkBtnReleaseToWIP.Text = "Released";
                                    lnkBtnReleaseToWIP.Enabled = false;
                                    break;    // exit the loop
                                }
                            }
                        }
                        else
                        {
                            lnkBtnReleaseToWIP.Visible = false;
                            break;
                        }
                    }
                }
                else
                {
                    lnkBtnReleaseToWIP.Visible = false;
                }
            }
        }
        catch { }
    }

    private bool IsAllInQA(int pReceiptID)
    {
        try
        {
            var warehouseList = (from di in idc.DeviceInventories
                                 join s in idc.DeviceAnalysisStatus on di.DeviceAnalysisStatusID equals s.DeviceAnalysisStatusID
                                 where di.ReceiptID == pReceiptID
                                 select s.Warehouse).Distinct().ToList();

            if (warehouseList.Count != 1)
            {
                return false;
            }
            else
            {
                if (warehouseList[0] == "QA")
                    return true;
                else
                    return false;
            }
        }
        catch { return false; }
    }

    protected void btnReleaseToWIP_Click(object sender, EventArgs e)
    {
        try
        {
            LinkButton btn = (LinkButton)sender;
            string myCmdname = btn.CommandName.ToString();
            int receiptID = int.Parse(btn.CommandArgument.ToString());

            // Get all of the devices by receiptID.
            var devices = (from di in idc.DeviceInventories where di.ReceiptID == receiptID select di);

            // Get designate WIP warehouse
            int deviceAnalysisStatusID = (from das in idc.DeviceAnalysisStatus where das.DeviceAnalysisName == "Assigned" select das.DeviceAnalysisStatusID).FirstOrDefault();

            if (deviceAnalysisStatusID > 0)
            {
                DeviceManager myDeviceManager = new DeviceManager();

                // Loop through each of the devices in the group and set to WIP warehouse
                foreach (var device in devices)
                {
                    device.DeviceAnalysisStatusID = deviceAnalysisStatusID;
                    // Save the changes
                    idc.SubmitChanges();

                    // Insert device audit info.                                               
                    myDeviceManager.InsertDeviceInventoryAudit(device.DeviceID, deviceAnalysisStatusID, device.FailedCalibration, "Release To WIP", "Receipt of Good Entry");
                }                
            }

            // refresh gridview
            grdPOReceiptsView.DataBind();
        }
        catch { }
    }

}