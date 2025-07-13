using System;
using System.Data;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Instadose.Data;
using System.Collections.Generic;
using Instadose.Integration;
using System.Text;
using System.Web;
using System.IO;
using System.Text.RegularExpressions;
using Instadose.Processing;
using portal_instadose_com_v3.Helpers;

public partial class CustomerService_DeactivateAccount : System.Web.UI.Page
{
    // Create the database reference
    InsDataContext idc = new InsDataContext();
    MASDataContext mdc = new MASDataContext();

    Mirion.DSD.GDS.API.SalesRepRequests _salesRepReq = new Mirion.DSD.GDS.API.SalesRepRequests();
    Mirion.DSD.GDS.API.AccountContractRequests _accountContractReq = new Mirion.DSD.GDS.API.AccountContractRequests();

    protected void Page_Load(object sender, EventArgs e)
    {
        // Try to set the username
        try
        {
            this.lblUserName.Text = User.Identity.Name.Split('\\')[1];
        }
        catch { this.lblUserName.Text = "Unknown"; }

        if (IsPostBack) return;

        if (Request.QueryString["ID"] != null)
        {
            // Parse the query string ID
            int accountID = 0;
            int.TryParse(Request.QueryString["ID"], out accountID);

            // Send the account ID to load the form.
            loadForm(accountID);
        }
    }

    // Preload the form
    private void loadForm(int accountID)
    {
        ddlReason.Items.Clear();
        lblCompanyName.Text = "";
        this.formError.Visible = false;

        // Query the account record for the supplied account.
        Account account = (from a in idc.Accounts where a.AccountID == accountID select a).FirstOrDefault();

        if (account == null)
        {
            txtNote.Text = "";
            btnSaveInfo.Enabled = false;
            lblErrorMessage.Text = "The account provided does not exist.";
            formError.Visible = true;
            txtAccountID.Focus();
        }
        else
        {
            // Set the label.
            btnSaveInfo.Text = account.Active ? "Deactivate" : "Activate";
            lblCompanyName.Text = account.CompanyName;
            txtAccountID.Text = account.AccountID.ToString();
            btnSaveInfo.Enabled = true;
            
            // Load the reason dropdown.
            loadReasonDropDown(account.Active);

            if (!account.Active)
            {
                // Customer Group
                pnCustomerGroup.Visible = true;
                phInvoiceDeliveryMethods.Visible = true;
                loadDDLCustomerGroup();
                ddlCustomerGroup.SelectedValue = account.CustomerGroupID.ToString();

                phInvoiceDeliveryEDI.Visible = account.BrandSourceID == 3;

                // Load Invoice Delivery Mothod
                if (account.BillingGroup != null)
                {
                    chkBoxInvDeliveryPrintMail.Checked = account.BillingGroup.useMail;

                    if (account.BillingGroup.useEmail1 || account.BillingGroup.useEmail2)
                    {
                        chkBoxInvDeliveryEmail.Checked = true;
                        //txtInvDeliveryPrimaryEmail.Enabled = true;
                        //txtInvDeliverySecondaryEmail.Enabled = true;
                        //txtInvDeliveryPrimaryEmail.Text = acctInfo.BillingGroup.Email1;
                        //txtInvDeliverySecondaryEmail.Text = acctInfo.BillingGroup.Email2;
                    }
                    else
                    {
                        chkBoxInvDeliveryEmail.Checked = false;
                        //txtInvDeliveryPrimaryEmail.Enabled = false;
                        //txtInvDeliverySecondaryEmail.Enabled = false;
                        //txtInvDeliveryPrimaryEmail.Text = "";
                        //txtInvDeliverySecondaryEmail.Text = "";
                    }
                    txtInvDeliveryPrimaryEmail.Text = string.IsNullOrEmpty(account.BillingGroup.Email1) ? "" : account.BillingGroup.Email1;
                    txtInvDeliverySecondaryEmail.Text = string.IsNullOrEmpty(account.BillingGroup.Email2) ? "" : account.BillingGroup.Email2;

                    if (account.BillingGroup.useFax)
                    {
                        chkBoxInvDeliveryFax.Checked = true;
                        //txtInvDeliveryPrimaryFax.Enabled = true;
                        //txtInvDeliveryPrimaryFax.Text = acctInfo.BillingGroup.Fax;
                    }
                    else
                    {
                        chkBoxInvDeliveryFax.Checked = false;
                        //txtInvDeliveryPrimaryFax.Enabled = false;
                        //txtInvDeliveryPrimaryFax.Text = "";
                    }
                    txtInvDeliveryPrimaryFax.Text = string.IsNullOrEmpty(account.BillingGroup.Fax) ? "" : account.BillingGroup.Fax;

                    chkBoxInvDeliveryEDI.Checked = account.BillingGroup.useEDI;
                    string clientName = "";
                    if (account.BillingGroup.EDIClientID.HasValue)
                    {
                        clientName = idc.Clients.Where(d => d.ClientID == account.BillingGroup.EDIClientID.Value).Select(d => d.ClientName).FirstOrDefault();
                    }
                    txtInvDeliveryEDIClientID.Text = clientName;

                    if (account.BillingGroup.useSpecialDelivery)
                    {
                        chkBoxInvDeliveryUpload.Checked = true;
                        //fileUploadInvDeliveryUpload.Enabled = true;
                        //txtInvDeliveryUploadInstruction.Enabled = true;
                        //txtInvDeliveryUploadInstruction.Text = acctInfo.BillingGroup.SpecialDeliveryText;
                    }
                    else
                    {
                        chkBoxInvDeliveryUpload.Checked = false;
                        //fileUploadInvDeliveryUpload.Enabled = false;
                        //txtInvDeliveryUploadInstruction.Enabled = false;
                        //txtInvDeliveryUploadInstruction.Text = "";
                    }
                    txtInvDeliveryUploadInstruction.Text = string.IsNullOrEmpty(account.BillingGroup.SpecialDeliveryText) ? "" : account.BillingGroup.SpecialDeliveryText;

                    chkBoxInvDeliveryDoNotSend.Checked = !account.BillingGroup.DeliverInvoice;
                    if ((account.CustomerTypeID == 3 || account.CustomerTypeID == 4) || (account.CustomerGroupID != null && (account.CustomerGroupID == 3 || account.CustomerGroupID == 4))
                        )
                    {
                        chkBoxInvDeliveryDoNotSend.Enabled = true;
                    }
                    else
                    {
                        chkBoxInvDeliveryDoNotSend.Enabled = false;
                    }
                }
                else
                {
                    chkBoxInvDeliveryDoNotSend.Checked = false;
                    chkBoxInvDeliveryDoNotSend.Enabled = false;
                    chkBoxInvDeliveryPrintMail.Checked = true;
                }

                // generate Contract Start Date Dropdown
                DateTime today = DateTime.Today;
                DateTime firstDateOfMonth = new DateTime(today.Year, today.Month, 1);
                ddlContractStartDate.Items.Add(new ListItem(firstDateOfMonth.ToShortDateString()));
                ddlContractStartDate.Items.Add(new ListItem(firstDateOfMonth.AddMonths(1).ToShortDateString()));

                // toggle Service Start Date for Contract
                var contract = GetAccountContract(account.AccountID);

                if (contract == null)
                {
                    hdnCurrentContractId.Value = "";
                    pnContractStartDate.Visible = true;
                }
                else
                {
                    hdnCurrentContractId.Value = contract.MRNAccountContractID.ToString();
                    pnContractStartDate.Visible = false;
                }
            }
            else
            {
                pnCustomerGroup.Visible = false;
                phInvoiceDeliveryMethods.Visible = false;
            }
            
            // load Sales Rep Dropdown for activate account, if sales rep is inactive
            if (!account.Active && account.SalesRepID.HasValue)
            {
                bool isSalesRepActive = _salesRepReq.IsSalesRepActive((int)account.SalesRepID);

                if (!isSalesRepActive)
                {
                    pnSalesRep.Visible = true;
                    loadSalesRepDropDown();
                }
            }
        }


        if (account == null)
        {

            ddlRateCode.DataSource = (from a in idc.Rates
                                      where //a.BrandSourceID == BrandID
                                      a.Active && a.ExpirationDate >= DateTime.Now
                                      orderby a.RateDesc
                                      select a);
        }
        else
        {
            ddlRateCode.DataSource = (from a in idc.Rates
                                      join b  in idc.Accounts on a.BrandSourceID equals b.BrandSourceID
                                      where b.AccountID == accountID
                                       && a.Active && a.ExpirationDate >= DateTime.Now
                                      orderby a.RateDesc
                                      select a);


        }

        if (ddlRateCode.DataSource != null)
        {
            ddlRateCode.DataBind();

            ListItem firstItem = new ListItem("-- Select Rate --", "0");
            ddlRateCode.Items.Insert(0, firstItem);
        }

        if (account.DefaultRateID > 0)
            ddlRateCode.SelectedValue = account.DefaultRateID.ToString();
    }

    private void loadSalesRepDropDown()
    {
        var salesReps = _salesRepReq.GetNonICCareSaleReps(true).Select(sr => new { sr.SalesRepID, TextField = sr.FirstName + " " + sr.LastName }).ToList();
        salesReps.Insert(0, new { SalesRepID = 0, TextField = "-- Select Sales Rep --" });
        ddlSalesRep.DataSource = salesReps;
        ddlSalesRep.DataTextField = "TextField";
        ddlSalesRep.DataValueField = "SalesRepID";
        ddlSalesRep.DataBind();
    }

    private void loadReasonDropDown(bool active)
    {
        ddlReason.Items.Clear();
        // Deactivate account reasons
        if (active)
        {
            ListItem item0 = new ListItem("--Select Deactivation Reason--", "0");
            this.ddlReason.Items.Insert(0, item0);

            ListItem item1 = new ListItem("Cancel of Services", "1");
            this.ddlReason.Items.Insert(1, item1);

            ListItem item2 = new ListItem("Credit Hold", "2");
            this.ddlReason.Items.Insert(2, item2);

        }


        // Re-Activate account reasons
        if (!active)
        {

            ListItem item0 = new ListItem("--Select Activation Reason--", "0");
            this.ddlReason.Items.Insert(0, item0);

            ListItem item1 = new ListItem("Payment has been Received", "1");
            this.ddlReason.Items.Insert(1, item1);

            ListItem item2 = new ListItem("Others", "2");
            this.ddlReason.Items.Insert(2, item2);
        }

    }

    private void loadDDLCustomerGroup()
    {
        ddlCustomerGroup.DataSource = Mirion.DSD.GDS.API.AccountRequests.GetMRNCustomerGroups();
        ddlCustomerGroup.DataTextField = "CustomerGroupName";
        ddlCustomerGroup.DataValueField = "CustomerGroupID";
        ddlCustomerGroup.DataBind();
    }

    protected void txtAccountID_OnTextChanged(object sender, EventArgs e)
    {
        int accountID = 0;
        int.TryParse(this.txtAccountID.Text, out accountID);

        loadForm(accountID);
    }

    protected void btnSaveInfo_Click(object sender, EventArgs e)
    {
        bool isICCareAccount = false;
        bool isDeactivating = false;
        bool isSuccess = true;

        int.TryParse(this.txtAccountID.Text, out int accountID);

        try
        {
            string notes = "Deactivate";
            
            // Disable the save button.
            formError.Visible = false;
            formSuccess.Visible = false;
            btnSaveInfo.Enabled = false;

            // Query the account record.
            Account account = (from a in idc.Accounts where a.AccountID == accountID select a).FirstOrDefault();

            isICCareAccount = account.BrandSourceID == 3;

            // Deactivate the account.
            if (account.Active) // deactive Account
            {
                isDeactivating = true;
                
                // Create the account notes.
                notes = string.Format("<div style='color:red; font-size:1.1em; font-weight:bold;'>Account Deactivated: {0}</div><p>{1}</p>",
                    ddlReason.SelectedItem.Text, txtNote.Text.Replace("\r\n", "<br />"));

                account.Active = false;
                account.StopServiceDate = DateTime.Now;
                lblSuccessMessage.Text = "The account has been deactivated.";

                idc.SubmitChanges();
            }
            else // re-activate Account
            {
                bool isInvoiceDeliveryMethodValid = ValidateInvoiceDeliveryMethodOnReinstate(out string validationErrorMessage);

                if (!isInvoiceDeliveryMethodValid)
                {
                    formError.Visible = true;
                    lblErrorMessage.Text = validationErrorMessage;

                    btnSaveInfo.Enabled = true;
                    return;
                }

                // Create the account notes.
                notes = string.Format("<div style='color:green; font-size:1.1em; font-weight:bold;'>Account Activated: {0}</div><p>{1}</p>",
                    ddlReason.SelectedItem.Text, txtNote.Text.Replace("\r\n", "<br />"));

                int idefaultRateID = 0;

                int.TryParse(ddlRateCode.SelectedValue, out idefaultRateID);

                account.Active = true;
                account.ReactivateDate = DateTime.Now;
                account.DefaultRateID = idefaultRateID;

                if (int.TryParse(ddlCustomerGroup.SelectedValue, out int customerGroupId))
                    account.CustomerGroupID = customerGroupId;
                
                if (pnSalesRep.Visible && account.SalesRepID.HasValue && !_salesRepReq.IsSalesRepActive((int)account.SalesRepID))
                {
                    int? salesRepId = null;
                    if (!string.IsNullOrEmpty(ddlSalesRep.SelectedValue) && ddlSalesRep.SelectedValue != "0" && int.TryParse(ddlSalesRep.SelectedValue, out int tmpSalesRepId))
                        salesRepId = tmpSalesRepId;
                    account.SalesRepID = salesRepId;
                }

                int returnBillingGroupID = SaveBillingGroup(false, account.AccountID);
                account.BillingGroupID = returnBillingGroupID;

                idc.SubmitChanges();

                // if contract does not exist, run renewal process
                if (pnContractStartDate.Visible)
                {
                    DateTime contractStartDt = DateTime.Parse(ddlContractStartDate.SelectedValue);
                    DateTime contractEndDt = contractStartDt.AddDays(-1).AddMonths(12);

                    //account.ContractStartDate = contractStartDt;
                    //account.ContractEndDate = contractEndDt;

                    // run renewal process
                    Renewals renewals = new Renewals();
                    renewals.UserName = lblUserName.Text;

                    RenewalStatus status = renewals.ProcessRenewalByPeriod(account.AccountID, null, contractStartDt, isICCareAccount);

                    if (!status.RenewalComplete)
                    {
                        string errMsg = "";

                        if (!status.GeneratedOrder)
                            errMsg = "Account is activated. But error occured while placing renewl order.";
                        else if (!status.UpdatedScheduledBilling)
                            errMsg = "Account is activated. But error occured while updating scheduled billing.";
                        else if (!status.PaymentProcessed)
                            errMsg = "Account is activated. But error occured while processing renewl order payment.";
                        else if (!status.UpdatedContractPeriod)
                            errMsg = "Account is activated. But error occured while updating account contract.";
                        else if (!status.UpdateAccountDeviceDate)
                            errMsg = "Account is activated. But error occured while updating account device date.";
                        else
                            errMsg = "Account is activated. But unknown error occured while processing renewal.";

                        errMsg += " - " + status.ErrorMessage;

                        lblErrorMessage.Text = errMsg;
                        formError.Visible = true;
                        btnSaveInfo.Enabled = true;

                        return;
                    }
                }

                lblSuccessMessage.Text = "The account has been activated.";
            }

            //// Add the notes to the bla bla.
            CreateNotes(account.AccountID, notes);
            
            // Show form
            formSuccess.Visible = true;
        }
        catch (Exception ex)
        {
            isSuccess = false;

            lblErrorMessage.Text = ex.Message;
            formError.Visible = true;
        }

        // delete all orders with 'Created - Need PO' status, if deactivating account is IC Care account and reason is 'Cancel of Service'
        if (isSuccess && isDeactivating && isICCareAccount && ddlReason.SelectedValue == "1")
        {
            List<DeleteOrderProcessInfo> deleteProcesses = DeleteAllNeedPOICCareOrders(accountID);

            List<int> errorOrders = new List<int>();

            if (deleteProcesses != null && deleteProcesses.Count > 0)
            {
                errorOrders = deleteProcesses.Where(p => !p.Success).Select(p => p.OrderID).ToList();
            }

            lblSuccessMessage.Text = "The account has been deactivated.";

            formSuccess.Visible = true;

            if (errorOrders != null && errorOrders.Count > 0)
            {
                lblErrorMessage.Text = "Error occured while removing following orders: <br />" + string.Join(", ", values: errorOrders);
                formError.Visible = true;
            }
        }
    }

    protected void btnCancel_Click(object sender, EventArgs e)
    {
        int accountID = 0;
        int.TryParse(Request.QueryString["ID"], out accountID);

        if (accountID != 0)
            Page.Response.Redirect("~/CustomerService/CreateAccount.aspx?ID=" + accountID.ToString());
        else
            Page.Response.Redirect("/InformationFinder/");
    }

    private List<DeleteOrderProcessInfo> DeleteAllNeedPOICCareOrders(int accountID)
    {
        Account account = idc.Accounts.SingleOrDefault(a => a.AccountID == accountID);

        if (account != null)
        {
            return DeleteAllNeedPOICCareOrders(account);
        }
        else
        {
            return null;
        }
    }

    private List<DeleteOrderProcessInfo> DeleteAllNeedPOICCareOrders(Account account)
    {
        // check whether account exist
        if (account == null)
            return null;

        // check wether account is IC Care account
        if (account.BrandSourceID != 3)
            return null;

        // find 'IC Care' & 'Created - Needs PO' status orders for the account
        var orders = idc.Orders.Where(o => o.AccountID == account.AccountID && o.OrderStatusID == 1 && o.BrandSourceID == 3).ToList();

        if (orders == null || orders.Count <= 0)
            return null;

        List<DeleteOrderProcessInfo> deleteProcesses = new List<DeleteOrderProcessInfo>();

        foreach (var order in orders)
        {
            deleteProcesses.Add(DeleteOrder_ICCareNeedPOOrder(order.OrderID));
        }

        return deleteProcesses;
    }

    private DeleteOrderProcessInfo DeleteOrder_ICCareNeedPOOrder(int orderID)
    {
        DeleteOrderProcessInfo processInfo = new DeleteOrderProcessInfo()
        {
            OrderID = orderID,
            Success = true,
            ErrorMessage = string.Empty,
            IsNonRenewalOrder = false
        };

        if (orderID <= 0)
        {
            return processInfo;
        }

        if (orderID > 0)
        {
            // GET OrderType, OrderStatusID, and BrandSourceID.
            Order order = idc.Orders.SingleOrDefault(o => o.OrderID == orderID);

            if (order.OrderStatusID != 1 || order.BrandSourceID != 3)
            {
                return processInfo;
            }

            processInfo.IsNonRenewalOrder = order.OrderType.ToUpper() != "RENEWAL";
            //processInfo.AdditionalInstruction = "Please make sure to delete the order in MAS200 and inform Efren German in Shipping & Manufacturing of the changes.";
                        
            // Check each table affected by either OrderID or OrderDetailsID and delete the record.
            // This has to be done in a specific order so as to ensure no orphan records remain.

            // Modified 09/24/2015 by Thinh Do, Yong Lee, and Anuradha Nandi
            // After researching the Database Tables; RenewalLogs, RenewalThreadLog, and RenewalThreadLogDetail,
            // it was discovered that RenewalLog and RenewalThreadLogDetail has a 1:1 Relationhip, but
            // RenewalThreadLog and RenewalThreadLogDetail has a 1:N Relationship.
            // This why it could not delete the Renewal Orders properly when trying to delete all (3) related table.
            // Per Thinh Do, we will ignore deletion from the RenewalThreadLog table and only delete records from the following;
            // 1.) RenewalThreadLogDetail
            // 2.) RenewalLog

            // Delete Renewal Thread Log Detail.
            var renewalThreadLogDetailRecords = (from rtld in idc.RenewalThreadLogDetails
                                                 join rl in idc.RenewalLogs on rtld.RenewalLogID equals rl.RenewalLogID
                                                 where rl.OrderID == orderID
                                                 select rtld).ToList();

            if (renewalThreadLogDetailRecords.Count != 0)
            {
                foreach (var renewalThreadLogDetail in renewalThreadLogDetailRecords)
                {
                    idc.RenewalThreadLogDetails.DeleteOnSubmit(renewalThreadLogDetail);
                }

                //tableNames.Add("RenewalThreadLogDetail"); // Add RenewalThreadLogDetail table to List.
            }

            // GET Renewal Log Record(s) for a given OrderID.
            var renewalLogRecords = (from rl in idc.RenewalLogs
                                     where rl.OrderID == orderID
                                     select rl).ToList();

            // Delete Renewal Logs.
            if (renewalLogRecords.Count != 0)
            {
                foreach (var renewalLog in renewalLogRecords)
                {
                    idc.RenewalLogs.DeleteOnSubmit(renewalLog);
                }
            }

            // Delete ToMas_SO_SalesOrderDetail
            var toMas_SO_SalesOrderDetailRecords = (from sod in mdc.ToMas_SO_SalesOrderDetails where sod.SalesOrderNo == MAS.CleanString(orderID.ToString().PadLeft(7, '0'), 7) select sod).ToList();

            if (toMas_SO_SalesOrderDetailRecords.Count != 0)
            {
                foreach (var toMas_SO_SalesOrderDetail in toMas_SO_SalesOrderDetailRecords)
                {
                    mdc.ToMas_SO_SalesOrderDetails.DeleteOnSubmit(toMas_SO_SalesOrderDetail);
                }
            }

            // Delete ToMAS_SO_SalesOrderHeader
            var toMAS_SO_SalesOrderHeaderRecords = (from soh in mdc.ToMAS_SO_SalesOrderHeaders where soh.FOB == orderID.ToString() select soh).ToList();

            if (toMAS_SO_SalesOrderHeaderRecords.Count != 0)
            {
                foreach (var toMAS_SO_SalesOrderHeader in toMAS_SO_SalesOrderHeaderRecords)
                {
                    mdc.ToMAS_SO_SalesOrderHeaders.DeleteOnSubmit(toMAS_SO_SalesOrderHeader);
                }
            }

            // Delete related Cases
            var caseRecords = (from c in idc.Cases where c.OrderID == orderID select c).ToList();

            if (caseRecords.Count != 0)
            {
                foreach (var cases in caseRecords)
                {
                    int caseID = cases.CaseID;

                    var caseNoteRecords = (from cn in idc.CaseNotes where cn.CaseID == caseID select cn).ToList();

                    foreach (var caseNotes in caseNoteRecords)
                    {
                        idc.CaseNotes.DeleteOnSubmit(caseNotes);
                    }

                    idc.Cases.DeleteOnSubmit(cases);
                }
            }

            // Delete related Documents
            var documentRecords = (from d in idc.Documents where d.OrderID == orderID select d).ToList();

            if (documentRecords.Count != 0)
            {
                foreach (var documents in documentRecords)
                {
                    idc.Documents.DeleteOnSubmit(documents);
                }
            }

            // Order PO Requests
            // PO's are now in Documents table.

            // Delete OrderUserAssign
            var orderUserAssignRecords = (from oua in idc.OrderUserAssigns where oua.OrderID == orderID select oua).ToList();

            if (orderUserAssignRecords.Count != 0)
            {
                foreach (var orderUserAssigns in orderUserAssignRecords)
                {
                    idc.OrderUserAssigns.DeleteOnSubmit(orderUserAssigns);
                }
            }

            // Delete Payments
            var paymentRecord = (from py in idc.Payments where py.OrderID == orderID select py).ToList();

            if (paymentRecord.Count != 0)
            {
                foreach (var payment in paymentRecord)
                {
                    idc.Payments.DeleteOnSubmit(payment);
                }
            }

            // Delete Renewal Billing Devices
            var renewalBillingDeviceRecords = (from rbd in idc.RenewalBillingDevices where rbd.OrderID == orderID select rbd).ToList();

            if (renewalBillingDeviceRecords.Count != 0)
            {
                foreach (var renewalBillingDevices in renewalBillingDeviceRecords)
                {
                    idc.RenewalBillingDevices.DeleteOnSubmit(renewalBillingDevices);
                }
            }

            // Delete Scheduled Billings
            var scheduledBillingRecords = (from sb in idc.ScheduledBillings where sb.OrderID == orderID select sb).ToList();

            if (scheduledBillingRecords.Count != 0)
            {
                foreach (var scheduledBillings in scheduledBillingRecords)
                {
                    idc.ScheduledBillings.DeleteOnSubmit(scheduledBillings);
                }
            }

            // Delete Account Device Credits
            var accountDeviceCreditRecords = (from adc in idc.AccountDeviceCredits
                                              where adc.OrderDetail.OrderID == orderID
                                              select adc).ToList();

            if (accountDeviceCreditRecords.Count != 0)
            {
                foreach (var accountDeviceCredits in accountDeviceCreditRecords)
                {
                    idc.AccountDeviceCredits.DeleteOnSubmit(accountDeviceCredits);
                }
            }

            // Delete Packages
            var packageRecords = (from pc in idc.Packages where pc.OrderID == orderID select pc).ToList();

            if (packageRecords.Count != 0)
            {
                foreach (var packages in packageRecords)
                {
                    idc.Packages.DeleteOnSubmit(packages);
                }
            }

            // Delete Package Order Details
            var packageOrderDetailRecords = (from pod in idc.PackageOrderDetails
                                             where pod.OrderDetail.OrderID == orderID
                                             select pod).ToList();

            if (packageOrderDetailRecords.Count != 0)
            {
                foreach (var packageOrderDetails in packageOrderDetailRecords)
                {
                    idc.PackageOrderDetails.DeleteOnSubmit(packageOrderDetails);
                }
            }

            // Delete Order Details
            var orderDetailRecords = (from od in idc.OrderDetails where od.OrderID == orderID select od).ToList();

            if (orderDetailRecords.Count != 0)
            {
                foreach (var orderDetails in orderDetailRecords)
                {
                    idc.OrderDetails.DeleteOnSubmit(orderDetails);
                }
            }

            // Delete DeviceColorization
            var deviceColorizationRecords = (from dc in idc.DeviceColorizations where dc.OrderID == orderID select dc).ToList();

            if (deviceColorizationRecords.Count != 0)
            {
                foreach (var deviceColor in deviceColorizationRecords)
                {
                    idc.DeviceColorizations.DeleteOnSubmit(deviceColor);
                }
            }

            // Delete Orders
            var orderRecords = idc.Orders.Where(o => o.OrderID == orderID && o.OrderStatusID != 10).FirstOrDefault();
            
            if (orderRecords != null)
            {
                idc.Orders.DeleteOnSubmit(orderRecords);
            }

            try
            {
                idc.SubmitChanges();
            }
            catch (Exception ex)
            {
                processInfo.Success = false;
                processInfo.ErrorMessage = ex.Message;
            }
        }
        else
        {
            processInfo.Success = false;
            processInfo.ErrorMessage = "Order does not found.";
        }

        return processInfo;
    }

    private int InsertAccountContract(Account account, DateTime contractStartDate)
    {
        DateTime contractEndDate = contractStartDate.AddMonths(12).AddDays(-1);

        return _accountContractReq.InsertInstaAccountContract(account.AccountID, contractStartDate, contractEndDate, account.BrandSourceID, false);
    }

    private Mirion.DSD.GDS.API.Contexts.MRNAccountContract GetAccountContract(int accountId)
    {
        DateTime today = DateTime.Today;
        Mirion.DSD.GDS.API.Contexts.MRNAccountContract currentContract = _accountContractReq.GetAccountContractByDate(accountId.ToString(), 2, today);

        // reinstating account may create contract with first day of next month as start date. So check whether contract exist for next month
        if (currentContract == null)
        {
            int yr = today.Year;
            int mon = today.Month;

            if (mon == 12)
            {
                yr++;
                mon = 1;
            }
            else
            {
                mon++;
            }

            DateTime nextContractStartDate = new DateTime(yr, mon, 1);
            currentContract = _accountContractReq.GetAccountContractByDate(accountId.ToString(), 2, nextContractStartDate);
        }

        return currentContract;
    }

    private int SaveBillingGroup(bool isNew, int AccountID)
    {
        int returnBillingGroupID = 0;

        try
        {
            Account act = (from a in idc.Accounts where a.AccountID == AccountID select a).FirstOrDefault();
            if (isNew)
            {
                var LocInfo = act.Locations.FirstOrDefault();

                Address billingAddr = idc.Addresses.FirstOrDefault(a => a.AddressTypeID == 1 && a.AccountID == act.AccountID);
                int billingAddressID = billingAddr == null ? 0 : billingAddr.AddressID;

                if (billingAddressID > 0)
                {
                    BillingGroup BGroup = new BillingGroup();

                    BGroup.AccountID = AccountID;
                    BGroup.BillingAddressID = billingAddressID;

                    BGroup.CompanyName = act.BrandSourceID == 3 ? HDealers.GetDealerName(act.UnixID, act.CompanyName) : act.CompanyName;
                    BGroup.ContactName = LocInfo.BillingFirstName + " " + LocInfo.BillingLastName;
                    BGroup.useMail = chkBoxInvDeliveryPrintMail.Checked;
                    BGroup.useEmail1 = chkBoxInvDeliveryEmail.Checked;
                    BGroup.Email1 = string.IsNullOrEmpty(txtInvDeliveryPrimaryEmail.Text) ? null : txtInvDeliveryPrimaryEmail.Text.Trim();
                    BGroup.useEmail2 = chkBoxInvDeliveryEmail.Checked && (!string.IsNullOrEmpty(txtInvDeliverySecondaryEmail.Text));
                    BGroup.Email2 = string.IsNullOrEmpty(txtInvDeliverySecondaryEmail.Text) ? null : txtInvDeliverySecondaryEmail.Text.Trim();
                    BGroup.useFax = chkBoxInvDeliveryFax.Checked;
                    BGroup.Fax = string.IsNullOrEmpty(txtInvDeliveryPrimaryFax.Text) ? null : txtInvDeliveryPrimaryFax.Text.Trim();
                    BGroup.useEDI = chkBoxInvDeliveryEDI.Checked;
                    //BGroup.EDIClientID = 1; manually update in the back end
                    BGroup.useSpecialDelivery = chkBoxInvDeliveryUpload.Checked;
                    BGroup.SpecialDeliveryText = string.IsNullOrEmpty(txtInvDeliveryUploadInstruction.Text) ? null : txtInvDeliveryUploadInstruction.Text.Trim();
                    BGroup.DeliverInvoice = !chkBoxInvDeliveryDoNotSend.Checked;

                    BGroup.PONumber = MAS.CleanString(act.RenewalPONumber, 15);
                    BGroup.ACMSyncBit = 1;

                    bool uploadResult = UploadInvDeliveryMethodInstruction(AccountID);

                    if (uploadResult)
                    {
                        idc.BillingGroups.InsertOnSubmit(BGroup);
                        idc.SubmitChanges();
                        // get the @@identity 
                        returnBillingGroupID = BGroup.BillingGroupID;

                        // ----------------------- Insert BillingGroupID to account --------------------------- //
                        act.BillingGroupID = returnBillingGroupID;
                        idc.SubmitChanges();
                        // ----------------------- Insert BillingGroupID to account --------------------------- //
                    }
                }
            }
            else
            {
                if (act.BillingGroup != null)
                {
                    // update address table
                    //int billingAddressID = act.BillingGroup.BillingAddressID;
                    //SaveAddress(ref billingAddressID, AccountID);
                    // update BillingGroup
                    BillingGroup BGroup = act.BillingGroup;
                    BGroup.useMail = chkBoxInvDeliveryPrintMail.Checked;
                    BGroup.useEmail1 = chkBoxInvDeliveryEmail.Checked;
                    BGroup.Email1 = string.IsNullOrEmpty(txtInvDeliveryPrimaryEmail.Text) ? null : txtInvDeliveryPrimaryEmail.Text.Trim();
                    BGroup.useEmail2 = chkBoxInvDeliveryEmail.Checked && (!string.IsNullOrEmpty(txtInvDeliverySecondaryEmail.Text));
                    BGroup.Email2 = string.IsNullOrEmpty(txtInvDeliverySecondaryEmail.Text) ? null : txtInvDeliverySecondaryEmail.Text.Trim();
                    BGroup.useFax = chkBoxInvDeliveryFax.Checked;
                    BGroup.Fax = string.IsNullOrEmpty(txtInvDeliveryPrimaryFax.Text) ? null : txtInvDeliveryPrimaryFax.Text.Trim();
                    BGroup.useEDI = chkBoxInvDeliveryEDI.Checked;
                    //BGroup.EDIClientID = 1; manually update in the back end
                    BGroup.useSpecialDelivery = chkBoxInvDeliveryUpload.Checked;
                    BGroup.SpecialDeliveryText = string.IsNullOrEmpty(txtInvDeliveryUploadInstruction.Text) ? null : txtInvDeliveryUploadInstruction.Text.Trim();
                    BGroup.DeliverInvoice = !chkBoxInvDeliveryDoNotSend.Checked;
                    
                    BGroup.ACMSyncBit = 1;

                    bool uploadResult = UploadInvDeliveryMethodInstruction(AccountID);

                    if (uploadResult)
                        idc.SubmitChanges();

                    returnBillingGroupID = act.BillingGroup.BillingGroupID;
                }
                else //if missing BillingGroup on the account then insert it
                {
                    returnBillingGroupID = SaveBillingGroup(true, AccountID);
                }
            }
        }
        catch
        {
            //delete Account by newAccountID
        }
        return returnBillingGroupID;
    }

    private bool UploadInvDeliveryMethodInstruction(int AccountID)
    {
        if (chkBoxInvDeliveryUpload.Checked && fileUploadInvDeliveryUpload.HasFile)
        {
            try
            {
                // Get Upload infor and convert to byte format        
                HttpPostedFile hpf = fileUploadInvDeliveryUpload.PostedFile;
                string attachmentFile = Path.GetFileName(hpf.FileName);

                // CREATE the Byte Array.
                Byte[] byteImage = new Byte[hpf.ContentLength];

                // Read uploaded file to byte array.
                hpf.InputStream.Read(byteImage, 0, hpf.ContentLength);
                var arrAttachment = attachmentFile.Split('.');
                string fileExtension = arrAttachment[arrAttachment.Length - 1];
                string mimeType = null;

                switch (fileExtension)
                {
                    case "DOC":
                        mimeType = "application/vnd.ms-word";
                        break;
                    case "DOCX":
                        mimeType = "application/vnd.ms-word";
                        break;
                    case "XLS":
                        mimeType = "application/vnd.ms-excel";
                        break;
                    case "XLSX":
                        mimeType = "application/vnd.ms-excel";
                        break;
                    case "PDF":
                        mimeType = "application/pdf";
                        break;
                    default:
                        mimeType = "image/jpeg";
                        break;
                }

                Document doc = new Document()
                {
                    DocumentGUID = Guid.NewGuid(),
                    FileName = Path.GetFileName(hpf.FileName),
                    DocumentContent = byteImage,
                    MIMEType = mimeType,
                    DocumentCategory = "Invoice Delivery Instruction",
                    Description = string.IsNullOrEmpty(txtInvDeliveryUploadInstruction.Text) ? null : txtInvDeliveryUploadInstruction.Text.Trim(),
                    AccountID = AccountID,
                    CreatedBy = this.lblCompanyName.Text,
                    CreatedDate = DateTime.Now,
                    Active = true
                };

                // SAVE the Document Record.
                idc.Documents.InsertOnSubmit(doc);
                idc.SubmitChanges();
            }
            catch
            {
                return false;
            }
        }
        return true;
    }

    private bool ValidateInvoiceDeliveryMethodOnReinstate(out string errorMessage)
    {
        bool isValid = true;

        List<string> errMsgs = new List<string>();
        
        if (!chkBoxInvDeliveryPrintMail.Checked && !chkBoxInvDeliveryEmail.Checked && !chkBoxInvDeliveryFax.Checked && !chkBoxInvDeliveryUpload.Checked && !chkBoxInvDeliveryDoNotSend.Checked)
        {
            errMsgs.Add(string.Format("{0} - {1}", "Invoice Delivery Method", "Must check at least one of delivery method check box."));
            isValid = false;
        }

        Regex emailRx = new Regex(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");

        // validate email
        if (chkBoxInvDeliveryEmail.Checked && string.IsNullOrEmpty(txtInvDeliveryPrimaryEmail.Text))
        {
            errMsgs.Add(string.Format("{0} - {1}", "Invoice Delivery Method", "Primary Email is required."));
            isValid = false;
        }

        // validate primary email
        if (!string.IsNullOrEmpty(txtInvDeliveryPrimaryEmail.Text) && !emailRx.IsMatch(txtInvDeliveryPrimaryEmail.Text.Trim()))
        {
            errMsgs.Add(string.Format("{0} - {1}", "Invoice Delivery Method", "Primary Email is not valid."));
            isValid = false;
        }

        // validate secondary email, if it is submitted
        if (!string.IsNullOrEmpty(txtInvDeliverySecondaryEmail.Text) && !emailRx.IsMatch(txtInvDeliverySecondaryEmail.Text.Trim()))
        {
            errMsgs.Add(string.Format("{0} - {1}", "Invoice Delivery Method", "Secondary Email is not valid."));
            isValid = false;
        }

        // validate whether fax number is provided
        if (chkBoxInvDeliveryFax.Checked && string.IsNullOrEmpty(txtInvDeliveryPrimaryFax.Text.Trim()))
        {
            errMsgs.Add(string.Format("{0} - {1}", "Invoice Delivery Method", "Primary Fax is required."));
            isValid = false;
        }

        // validate upload
        if (chkBoxInvDeliveryUpload.Checked && !fileUploadInvDeliveryUpload.HasFile)
        {
            errMsgs.Add(string.Format("{0} - {1}", "Invoice Delivery Method", "Instruction File is required."));
            isValid = false;
        }

        if (isValid)
        {
            errorMessage = null;
        }
        else
        {
            errorMessage = string.Join("<br />", errMsgs);
        }

        return isValid;
    }

    private void CreateNotes(int accountId, string notes)
    {
        InsDataContext dc = new InsDataContext();

        AccountNote accountNote = new AccountNote
        {
            AccountID = accountId,
            CreatedBy = lblUserName.Text,
            CreatedDate = DateTime.Now,
            NoteText = notes,
            Active = true
        };

        dc.AccountNotes.InsertOnSubmit(accountNote);
        dc.SubmitChanges();
    }

    private class DeleteOrderProcessInfo
    {
        public int OrderID { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        //public string AdditionalInstruction { get; set; }
        public bool IsNonRenewalOrder { get; set; }
    }
}
