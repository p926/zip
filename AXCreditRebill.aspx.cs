using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using System.Data;

using Mirion.DSD.GDS.API;

using AXData;
using Mirion.DSD.GDS.API.Contexts;
using System.Web.Script.Serialization;
using Mirion.DSD.GDS.API.Requests;
using Instadose.API.DA;
using Telerik.Web.UI;

namespace portal_instadose_com_v3.Finance
{
    public partial class AXCreditRebill : System.Web.UI.Page
    {
        private string _userName = "Unknown";

        #region Events
        protected void Page_Load(object sender, EventArgs e)
        {
            // Try to set the username
            try
            {
                _userName = User.Identity.Name.Split('\\')[1];
            }
            catch
            {
                _userName = "Unknown";
            }

            var authorizedUsers = DAMisc.GetPortalAppSetting("AXCreditRebill");
            var arrUserNames = authorizedUsers.Split(';');

            if (!arrUserNames.Contains(_userName, StringComparer.OrdinalIgnoreCase))
            {
                Response.Write("Not Authorized to View this page!");
                Response.End();
            }

            hdnUserID.Value = _userName;

            ToggleError(false);

            if (!IsPostBack)
            {
                var cancelReasons = GetACMCancelReasons();

                if (cancelReasons != null && cancelReasons.Count > 0)
                {
                    JavaScriptSerializer js = new JavaScriptSerializer();
                    var methods = cancelReasons.Select(cm => new { cm.CancelReasonCode, cm.Description }).ToList();

                    hdnACMCancelReasons.Value = js.Serialize(methods);
                }

                Session["AXInvoices"] = new List<vw_ax_CreditableInvoice>();
                Session["sortDirection"] = SortDirection.Ascending;
            }
        }

        protected void lnkbtnDisplay_Click(object sender, EventArgs e)
        {
            ToggleError(false);
            Session["AXInvoices"] = new List<vw_ax_CreditableInvoice>();

            bool isPortalAccount = ddlAccountType.SelectedValue == "portal";
            string accountId = txtAccountID.Text.Trim();
            string poNumber = txtPONumber.Text.Trim();

            if (string.IsNullOrEmpty(accountId) && string.IsNullOrEmpty(poNumber))
            {
                ToggleError(true, "Please enter account or order number.");
                BindAXInvoiceGridBySession(0);
                return;
            }

            //// Check wither account is IC Care account.
            //if (!string.IsNullOrEmpty(accountId))
            //{
            //    bool isICCareAccount = IsICCareAccount(accountId, isPortalAccount);

            //    if (isICCareAccount)
            //    {
            //        ToggleError(true, "Credit or Rebill cannot be processed for IC Care Account.");
            //        BindAXInvoiceGridBySession(0);
            //        return;
            //    }
            //}

            // Check whether account is IC Care or Prebill legacy account.
            if (!string.IsNullOrEmpty(accountId))
            {
                // check whether account exist
                if (!IsAccountExist(accountId, isPortalAccount))
                {
                    ToggleError(true, "Account not found.");
                    BindAXInvoiceGridBySession(0);
                    return;
                }

                bool isAllowedAccount = IsCreditRebillAllowedAccount(accountId, isPortalAccount);

                if (!isAllowedAccount)
                {
                    ToggleError(true, "Credit or Rebill cannot be processed for IC Care or Prebill Account.");
                    BindAXInvoiceGridBySession(0);
                    return;
                }
            }

            var invoices = GetInvoicesByAccount(accountId, poNumber, isPortalAccount);

            //// filter IC Care Account invoices from the invoices by PO Number
            //if (invoices != null && invoices.Count > 0 && string.IsNullOrEmpty(accountId) && !string.IsNullOrEmpty(poNumber))
            //{
            //    List<string> icCareAccounts = new List<string>();
            //    var accountIDs = invoices.Select(i => i.INVOICEACCOUNT).Distinct().ToList();

            //    if (accountIDs != null && accountIDs.Count > 0)
            //    {
            //        foreach (var id in accountIDs)
            //        {
            //            if (IsICCareAccount(id, isPortalAccount))
            //                icCareAccounts.Add(id);
            //        }
            //    }

            //    invoices = invoices.Where(i => !icCareAccounts.Contains(i.INVOICEACCOUNT)).ToList();
            //}

            // filter IC Care or Prebill legacy Account invoices from the invoices by PO Number
            if (invoices != null && invoices.Count > 0 && string.IsNullOrEmpty(accountId) && !string.IsNullOrEmpty(poNumber))
            {
                List<string> notAllowedAccounts = new List<string>();
                var accountIDs = invoices.Select(i => i.INVOICEACCOUNT).Distinct().ToList();

                if (accountIDs != null && accountIDs.Count > 0)
                {
                    foreach (var id in accountIDs)
                    {
                        if (!IsCreditRebillAllowedAccount(id.Substring(4), isPortalAccount))
                            notAllowedAccounts.Add(id);
                    }
                }

                invoices = invoices.Where(i => !notAllowedAccounts.Contains(i.INVOICEACCOUNT)).ToList();
            }

            if (invoices != null && invoices.Count > 0)
                invoices = invoices.OrderByDescending(i => i.INVOICEID).ToList();

            Session["AXInvoices"] = invoices ?? new List<vw_ax_CreditableInvoice>();

            BindAXInvoiceGridBySession(0);
        }

        protected void rgInvoices_NeedDataSource(object sender, Telerik.Web.UI.GridNeedDataSourceEventArgs e)
        {
            rgInvoices.DataSource = Session["AXInvoices"];
        }

        protected void rgInvoices_ItemDataBound(object sender, Telerik.Web.UI.GridItemEventArgs e)
        {
            if (e.Item is GridDataItem)
            {
                GridDataItem item = e.Item as GridDataItem;
                vw_ax_CreditableInvoice invoice = item.DataItem as vw_ax_CreditableInvoice;

                if (invoice != null)
                {
                    HyperLink hlCreditAXInvoice = item.FindControl("hlCreditAXInvoice") as HyperLink;
                    HyperLink hlRebillAXInvoice = item.FindControl("hlRebillAXInvoice") as HyperLink;
                    HyperLink hlAXInvoiceDetail = item.FindControl("hlAXInvoiceDetail") as HyperLink;
                    Literal ltInvoiceAmount = item.FindControl("ltInvoiceAmount") as Literal;

                    hlCreditAXInvoice.NavigateUrl = string.Format("javascript:ManageAXCreditRebill.openCreditInvoiceConfirm('{0}');", invoice.INVOICEID);
                    hlRebillAXInvoice.NavigateUrl = string.Format("javascript:ManageAXCreditRebill.openInvoiceDetailsDialog('{0}');", invoice.INVOICEID);

                    hlAXInvoiceDetail.Text = invoice.INVOICEID;
                    hlAXInvoiceDetail.NavigateUrl = string.Format("javascript:ManageAXCreditRebill.openInvoiceInfoDialog('{0}');", invoice.INVOICEID);

                    ltInvoiceAmount.Text = string.Format("{0} {1}", invoice.INVOICEAMOUNT.ToString("0.00"), invoice.CURRENCYCODE);
                }
            }
        }

        protected void lnkbtnClearFilters_Invoices_Click(object sender, EventArgs e)
        {
            foreach (GridColumn column in rgInvoices.MasterTableView.OwnerGrid.Columns)
            {
                column.CurrentFilterFunction = GridKnownFunction.NoFilter;
                column.CurrentFilterValue = string.Empty;
            }

            rgInvoices.MasterTableView.FilterExpression = string.Empty;
            rgInvoices.Rebind();
        }
        #endregion

        private void BindAXInvoiceGridBySession(int pageIdx)
        {
            rgInvoices.CurrentPageIndex = pageIdx;
            rgInvoices.DataSource = Session["AXInvoices"];
            rgInvoices.DataBind();
        }

        private void ToggleError(bool show, string errorMessage = "")
        {
            pnErrorMessage.Visible = show;
            lblError.Text = errorMessage;
        }

        private List<vw_ax_CreditableInvoice> GetInvoicesByAccount(string accountID, string poNumber, bool isPortalAccount)
        {
            AXInvoiceRequests invoiceReq = new AXInvoiceRequests();

            List<vw_ax_CreditableInvoice> invoices = new List<vw_ax_CreditableInvoice>();

            try
            {
                invoices = invoiceReq.GetNonCreditRebilledInvoiceSearchSummaryByAccountPONumber(accountID, poNumber, isPortalAccount);
            }
            catch (Exception ex)
            {
                ToggleError(true, string.Format("Error occured while getting invoices - {0}", ex.Message));
            }

            // filter Credit Memo Invoices which has prefix 'CM-'
            if (invoices != null && invoices.Count > 0)
                invoices = invoices.Where(i => !i.INVOICEID.StartsWith("CM-")).ToList();

            return invoices;
        }

        private List<ACMCancelReason> GetACMCancelReasons()
        {
            AXBillableEventRequests req = new AXBillableEventRequests();

            return req.GetACMCancelReasons(true);
        }

        private bool IsAccountExist(string accountId, bool isPortalAccount)
        {
            if (string.IsNullOrEmpty(accountId))
                return false;

            if (isPortalAccount)
            {
                if (int.TryParse(accountId, out int accnId))
                    return DAAccount.IsAccountExist(accnId);
                else
                    return false;
            }
            else
            {
                AccountRequests acctReq = new AccountRequests();
                return acctReq.IsAccountExist(accountId);
            }
        }

        private bool IsICCareAccount(string accountId, bool isPortalAccount)
        {
            bool isICCareAccount = false;

            if (!string.IsNullOrEmpty(accountId))
            {
                if (isPortalAccount)
                {
                    if (int.TryParse(accountId, out int accnID))
                        isICCareAccount = DAAccount.IsICCareAccount(accnID);
                }
                else
                {
                    AccountRequests acctReq = new AccountRequests();
                    isICCareAccount = acctReq.IsICCareAccount(accountId);
                }
            }

            return isICCareAccount;
        }

        /// <summary>
        /// Check BillingModel data in MRNtoAXAccounts table in Dosimetry DB. If BillingModel is not PA or IC, the account is allowed to be creditted/rebilled.
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="isPortalAccount"></param>
        /// <returns></returns>
        private bool IsCreditRebillAllowedAccount(string accountId, bool isPortalAccount)
        {
            if (string.IsNullOrEmpty(accountId))
                return false;

            int sourceSystem = isPortalAccount ? 2 : 1;

            string billingModel = AccountRequests.GetMRNAccountBillingModel(accountId, sourceSystem);

            if (string.IsNullOrEmpty(billingModel) || billingModel == "PA" || billingModel == "IC")
                return false;

            return true;
        }
    }
}