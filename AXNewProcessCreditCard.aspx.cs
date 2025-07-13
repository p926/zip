using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Threading;

using Instadose;
using Instadose.Data;
using Instadose.Security;

using Instadose.Integration.OrbitalPaymentech;

using System.IO;
using System.Text.RegularExpressions;

using Mirion.DSD.GDS.API;
using Mirion.DSD.GDS.API.Requests;

namespace portal_instadose_com_v3.Finance
{
    public partial class AXNewProcessCreditCard : System.Web.UI.Page
    {
        private string accountID = null;
        string UserName = "Unknown";
        bool instadoseProcessing = true;
        private InsDataContext idc = new InsDataContext();
        private FinanceRequests fr = new FinanceRequests();
        
        AXInvoiceRequests axInvoiceRequests = new AXInvoiceRequests();

        #region ON PAGE UNLOAD
        protected void Page_Unload(object sender, EventArgs e)
        {
            try
            {
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
            catch
            {
                return;
            }
        }
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            //// Orbital CC process will be handled by ePay in AX. Redirect the page to finance default page
            //Response.Redirect("/Finance/Default.aspx");

            this.UserName = User.Identity.Name.IndexOf('\\') > 0 ? User.Identity.Name.Split('\\')[1] : User.Identity.Name;

            // Do nothing below if a post back has occurred.
            if (Page.IsPostBack)
            {
                accountID = this.txtAccountNumber.Text;
                divPaymentDecline.Visible = false;

                instadoseProcessing = this.rdobtnInstadose.Checked;

                return;
            }

            VisiblePanel(false);

            // Bind the year drop down list.
            for (int i = 0; i <= 10; i++)
            {
                int year = DateTime.Today.Year + i;
                this.ddlExpirationYear.Items.Add(new ListItem(year.ToString()));
            }

            // Assign Sessions to all temporary values IF Credit Card entered is being used only once.
            Session["UseOnlyOnce"] = false;
            Session["TempCreditCardNumber"] = null;
            Session["TempNameOnCard"] = null;
            Session["TempExpirationMonth"] = null;
            Session["TempExpirationYear"] = null;
            Session["TempCVV"] = null;
            Session["TempCreditCardTypeID"] = null;
        }

        #region RESET MODAL CONTROLS
        private void ResetModalControlsToDefault()
        {
            InvisibleErrorsModal();
            this.txtNameOnCard.Text = "";
            this.txtCreditCardNumber.Text = "";
            this.ddlExpirationMonth.SelectedIndex = 0;
            this.ddlExpirationYear.SelectedIndex = 0;
            this.txtSecurityCode.Text = "";
            this.chkbxUseOnlyOnce.Checked = false;
        }
        #endregion

        #region MODAL/DIALOG ERROR MESSAGE
        private void InvisibleErrorsModal()
        {
            this.modalDialogErrorMsg.InnerText = "";
            this.modalDialogErrors.Visible = false;
        }

        private void VisibleErrorsModal(string error)
        {
            this.modalDialogErrorMsg.InnerText = error;
            this.modalDialogErrors.Visible = true;
        }
        #endregion

        #region MAIN CONTENT ERROR/SUCCESS MESSAGES
        // Error Message(s) based on AccountID/Number entered.
        private void InvisibleErrors()
        {
            this.errorMsg.InnerHtml = "";
            this.errors.Visible = false;
        }

        private void VisibleErrors(string error)
        {
            this.errorMsg.InnerHtml = error;
            this.errors.Visible = true;
        }

        private void InvisibleSuccesses()
        {
            this.successMsg.InnerHtml = "";
            this.successes.Visible = false;
        }

        private void VisibleSuccesses(string msg)
        {
            this.successMsg.InnerHtml = msg;
            this.successes.Visible = true;
        }

        private string GenerateBulletMessage(List<string> pMsgList)
        {
            string errorText = "<ul type='circle'>";

            foreach (string msg in pMsgList)
            {
                errorText += "<li>" + msg + "</li>";
            }

            errorText += "</ul>";

            return errorText;
        }
        #endregion

        #region VISIBLE/INVISIBLE PANELS
        private void VisiblePanel(bool pVisible)
        {
            pnlActiveInvoicesAndCreditCardInformation.Visible = pVisible;
            pnlPaymentHistory.Visible = pVisible;
        }
        #endregion

        #region VALIDATE ACCOUNTID/NUMBER ENTERED
        private bool ValidateAccountNumberEntered(string accountID)
        {
            if (this.instadoseProcessing)
            {
                int instadoseAccountID = 0;
                if (int.TryParse(accountID, out instadoseAccountID))
                    return idc.Accounts.Any(a => a.AccountID == instadoseAccountID);
            }
            else
            {
                Int32 dealerAccts;
                if (Int32.TryParse(accountID, out dealerAccts) && dealerAccts >= 90000000 && dealerAccts <= 99999999)
                {
                    return idc.Dealers.Any(d => d.DealerID == dealerAccts && d.Active == true);
                }
                else
                {
                    var dc = new Mirion.DSD.GDS.API.Contexts.UnixDataClassesDataContext();
                    return dc.UnixAccounts.Any(a => a.GDSAccount == accountID);
                }
            }

            return false;
        }
        #endregion

        #region ON_CLICK BTNFIND
        // Enter and Search Order Number entered.
        protected void btnFind_Click(object sender, EventArgs e)
        {
            // Clear-out Error/Success Messages.
            InvisibleErrors();
            InvisibleSuccesses();

            txtUnsuccessEmail.Text = "";
            txtSuccessEmail.Text = "";
            this.divProcessPayment.Visible = false;
            this.btnPrintSummaryStatement.Enabled = false;

            // Reset all session veriables
            ResetAllSessionValues();

            // Check if AccountID entered.
            if (this.txtAccountNumber.Text == "")
            {
                VisibleErrors("Please enter an Account #.");
                this.txtAccountNumber.Focus();
                return;
            }

            // Check if a Division (Instadose or Unix) is selected.
            if (!this.rdobtnInstadose.Checked && !this.rdobtnUnix.Checked)
            {
                VisibleErrors("Please select either Instadose or Unix.");
                this.rdobtnInstadose.Focus();
                return;
            }

            // Check to see if AccountID entered is a valid Instadose or Unix Account.
            if (ValidateAccountNumberEntered(accountID))
            {
                // Display ALL information related to the AccountID.
                VisiblePanel(true);

                // Get, Set, and Format (default) lblTotal value.
                this.lblTotal.Text = string.Format("{0:0.00}", 0);

                // Get, Set, and Format (default) lblTotalToBeChargedToCC value.
                this.txtTotalToBeChargedToCC.Text = string.Format("{0:0.00}", 0);

                // GridView of Open/Active AR's.
                PopulateActiveInvoicesGridView(accountID);

                // Active Credit Cards on Account DDL.
                PopulateAccountCreditCardsDDL(accountID);

                // GridView of Payment History.
                PopulatePaymentHistoryGridView(accountID);
            }
            else
            {
                VisibleErrors("The Account # entered is invalid.");
                VisiblePanel(false);
                this.txtAccountNumber.Focus();
                return;
            }
        }
        #endregion

        #region ON_CLICK btnPrintSummaryStatement    
        protected void btnPrintSummaryStatement_Click(object sender, EventArgs e)
        {
            DataTable dtSummaryStatement = new DataTable();

            // Create the review orders datatable to hold the contents of the order.
            dtSummaryStatement = new DataTable();

            // Create the columns for the review orders datatable.
            dtSummaryStatement.Columns.Add("AccountID", Type.GetType("System.String"));
            dtSummaryStatement.Columns.Add("CompanyName", Type.GetType("System.String"));
            dtSummaryStatement.Columns.Add("InvoiceNo", Type.GetType("System.String"));
            dtSummaryStatement.Columns.Add("InvoiceDate", Type.GetType("System.String"));
            dtSummaryStatement.Columns.Add("InvoiceAmt", Type.GetType("System.String"));
            dtSummaryStatement.Columns.Add("Balance", Type.GetType("System.String"));
            dtSummaryStatement.Columns.Add("Payment", Type.GetType("System.String"));
            dtSummaryStatement.Columns.Add("OrderID", Type.GetType("System.String"));

            dtSummaryStatement.Columns.Add("Address1", Type.GetType("System.String"));
            dtSummaryStatement.Columns.Add("Address2", Type.GetType("System.String"));
            dtSummaryStatement.Columns.Add("Address3", Type.GetType("System.String"));
            dtSummaryStatement.Columns.Add("City", Type.GetType("System.String"));
            dtSummaryStatement.Columns.Add("PostalCode", Type.GetType("System.String"));
            dtSummaryStatement.Columns.Add("State", Type.GetType("System.String"));
            dtSummaryStatement.Columns.Add("Country", Type.GetType("System.String"));
            dtSummaryStatement.Columns.Add("TotalDue", Type.GetType("System.String"));
            dtSummaryStatement.Columns.Add("MirionAddressByBrandsource", Type.GetType("System.String"));

            string CrystalReportFileName = "SummaryInvStatement_Mirion.rpt";
            string mirionAddressByBransource = string.Format("104 Union Valley Road,{0}Oak Ridge, TN 37830", Environment.NewLine);

            bool printAll = true;
            bool retrieveFirstRowInfo = false;
            string address1 = "", address2 = "", address3 = "", city = "", postalCode = "", stateAbbrev = "", country = "";
            string accountID = "", companyName = "";
            decimal totalDue = 0;

            foreach (GridViewRow row in this.gvActiveInvoices.Rows)
            {
                CheckBox chkBx = (CheckBox)row.FindControl("chkbxInvoiceSelect");

                if (chkBx.Checked == true)
                {
                    printAll = false;
                    break;
                }
            }

            foreach (GridViewRow row in this.gvActiveInvoices.Rows)
            {
                if (printAll)
                {
                    totalDue += Convert.ToDecimal(((Label)row.FindControl("lblBalance")).Text);
                }
                else
                {
                    CheckBox chkBx = (CheckBox)row.FindControl("chkbxInvoiceSelect");
                    if (chkBx.Checked == true)
                    {
                        totalDue += Convert.ToDecimal(((Label)row.FindControl("lblBalance")).Text);
                    }
                }
            }

            foreach (GridViewRow row in this.gvActiveInvoices.Rows)
            {
                if (!printAll)
                {
                    CheckBox chkBx = (CheckBox)row.FindControl("chkbxInvoiceSelect");
                    if (chkBx.Checked == false) continue;
                }

                DataRow dr = dtSummaryStatement.NewRow();

                string orbitalCurrencyCode = ((HiddenField)row.FindControl("HidCurrencyCode")).Value;
                string currencyCode = (from c in idc.Currencies where c.OrbitalCurrencyCode == orbitalCurrencyCode select c.CurrencyCode).FirstOrDefault();

                if (!retrieveFirstRowInfo)
                {
                    accountID = row.Cells[1].Text.Trim();
                    companyName = row.Cells[2].Text.Trim();

                    if (this.instadoseProcessing)
                    {
                        var actAddress = (from a in idc.Locations
                                          where a.AccountID == int.Parse(txtAccountNumber.Text) && a.IsDefaultLocation == true && a.Active == true
                                          select a).FirstOrDefault();

                        if (actAddress != null)
                        {
                            address1 = string.IsNullOrEmpty(actAddress.BillingAddress1) ? "" : actAddress.BillingAddress1;
                            address2 = string.IsNullOrEmpty(actAddress.BillingAddress2) ? "" : actAddress.BillingAddress2;
                            address3 = string.IsNullOrEmpty(actAddress.BillingAddress3) ? "" : actAddress.BillingAddress3;
                            city = string.IsNullOrEmpty(actAddress.BillingCity) ? "" : actAddress.BillingCity;
                            postalCode = string.IsNullOrEmpty(actAddress.BillingPostalCode) ? "" : actAddress.BillingPostalCode;
                            stateAbbrev = string.IsNullOrEmpty(actAddress.BillingState.StateAbbrev) ? "" : actAddress.BillingState.StateAbbrev;
                            country = string.IsNullOrEmpty(actAddress.BillingCountry.CountryName) ? "" : actAddress.BillingCountry.CountryName;

                            if (actAddress.Account.BrandSource.BrandName == "IC Care")
                            {
                                CrystalReportFileName = "SummaryInvStatement_ICCare.rpt";
                                companyName = actAddress.BillingCompany;
                            }
                        }
                    }
                    else // Legacy Accts
                    {
                        AccountRequests ar = new AccountRequests();
                        Mirion.DSD.GDS.API.DataTypes.GAccountDetails unixAccount = ar.GetAccountDetails(accountID);

                        if (unixAccount != null)
                        {
                            address1 = string.IsNullOrEmpty(unixAccount.Address1) ? "" : unixAccount.Address1;
                            address2 = string.IsNullOrEmpty(unixAccount.Address2) ? "" : unixAccount.Address2;
                            address3 = string.IsNullOrEmpty(unixAccount.Address4) ? "" : unixAccount.Address4;
                            city = string.IsNullOrEmpty(unixAccount.City) ? "" : unixAccount.City;
                            postalCode = string.IsNullOrEmpty(unixAccount.ZipCode) ? "" : unixAccount.ZipCode;
                            stateAbbrev = string.IsNullOrEmpty(unixAccount.State) ? "" : unixAccount.State;
                            country = string.IsNullOrEmpty(unixAccount.CountryDesc) ? "" : unixAccount.CountryDesc;

                            if (unixAccount.BrandSourceID.ToString() == "ICCare")
                            {
                                CrystalReportFileName = "SummaryInvStatement_ICCare.rpt";
                                mirionAddressByBransource = string.Format("P.O BOX 19755{0}Irvine, CA 92623-9998", Environment.NewLine);
                            }
                        }
                    }

                    retrieveFirstRowInfo = true;
                }

                dr["AccountID"] = accountID;
                dr["CompanyName"] = companyName;
                dr["InvoiceNo"] = row.Cells[3].Text.Trim();
                dr["InvoiceDate"] = row.Cells[4].Text.Trim();
                dr["InvoiceAmt"] = string.Format("{0:N}", Convert.ToDecimal(row.Cells[5].Text.Trim())) + " " + currencyCode;
                dr["Balance"] = string.Format("{0:N}", Convert.ToDecimal(((Label)row.FindControl("lblBalance")).Text)) + " " + currencyCode;
                dr["Payment"] = string.Format("{0:N}", (Convert.ToDecimal(row.Cells[5].Text.Trim()) - Convert.ToDecimal(((Label)row.FindControl("lblBalance")).Text))) + " " + currencyCode;
                dr["OrderID"] = String.IsNullOrEmpty(((HiddenField)row.FindControl("HidOrderID")).Value) ? "" : ((HiddenField)row.FindControl("HidOrderID")).Value;

                dr["Address1"] = address1;
                dr["Address2"] = address2;
                dr["Address3"] = address3;
                dr["City"] = city;
                dr["PostalCode"] = postalCode;
                dr["State"] = stateAbbrev;
                dr["Country"] = country;
                dr["TotalDue"] = string.Format("{0:N}", totalDue) + " " + currencyCode;
                dr["MirionAddressByBrandsource"] = mirionAddressByBransource;

                // Add row to the data table.
                dtSummaryStatement.Rows.Add(dr);

            } // END FOR LOOP

            if (dtSummaryStatement.Rows.Count > 0)
            {
                ReportDocument cryRpt = new ReportDocument();
                Stream oStream = null;
                byte[] byteArray = null;

                string myCRFileNamePath = Server.MapPath(CrystalReportFileName);

                string pdfFileName = "SummaryInvoiceStatement_" + txtAccountNumber.Text + "_" + DateTime.Now.ToString("MMddyyyy") + ".pdf";

                cryRpt.Load(myCRFileNamePath);
                cryRpt.SetDataSource(dtSummaryStatement);

                oStream = cryRpt.ExportToStream(ExportFormatType.PortableDocFormat);
                byteArray = new byte[oStream.Length];
                oStream.Read(byteArray, 0, Convert.ToInt32(oStream.Length - 1));

                try
                {
                    // insert statement to document table.
                    Document doc = new Document()
                    {
                        Active = true,
                        AccountID = (int?)null,
                        OrderID = (int?)null,
                        CreatedBy = this.UserName,
                        CreatedDate = DateTime.Now,
                        Description = "Summary Invoice Statement by Account",
                        DocumentCategory = "Summary Invoice Statement",
                        DocumentGUID = Guid.NewGuid(),
                        FileName = pdfFileName,
                        DocumentContent = byteArray,
                        //MIMEType = "application/vnd.ms-excel",
                        MIMEType = "application/pdf",
                    };

                    idc.Documents.InsertOnSubmit(doc);
                    idc.SubmitChanges();

                    string url = "SummaryInvoiceStatement.aspx?documentID=" + doc.DocumentID;
                    ScriptManager.RegisterStartupScript(this, typeof(string), "OPEN_WINDOW", "window.open( '" + url + "', '_blank')", true);
                }
                catch (Exception ex)
                {
                    // Report the error to the message system.
                    Basics.WriteLogEntry(ex.Message, Page.User.Identity.Name,
                        "Portal.InstaDose_Finance_NewProcessCreditCard", Basics.MessageLogType.Critical);

                    Response.Write(ex);
                }
                finally
                {
                    cryRpt.Close();
                    cryRpt.Dispose();
                    oStream.Flush();
                    oStream.Close();
                    oStream.Dispose();
                }
            }
        }
        #endregion

        #region POPULATE GVACTIVEINVOICES (GET DATA)
        /// <summary>
        /// For entered/passed AccountID, display ALL Active/Open Account AR History.
        /// </summary>
        /// <param name="accountid"></param>
        private void PopulateActiveInvoicesGridView(string AccountID)
        {
            gvActiveInvoices.DataSource = null;
            gvActiveInvoices.DataBind();

            string axAccountID = "";
            string companyName = "";

            if (instadoseProcessing)
            {
                axAccountID = "INS-" + AccountID;
                companyName = (from a in idc.Accounts where a.AccountID == Convert.ToInt32(AccountID) select a.CompanyName).FirstOrDefault();
            }
            else
            {
                axAccountID = "MRN-" + AccountID;

                var acctReq = new AccountRequests();
                companyName = acctReq.GetCompanyName(AccountID).ToUpper();
            }

            List<InvoiceItems> invoiceItems = new List<InvoiceItems>();

            // AX Invoices
            var invoices = axInvoiceRequests.GetInvoicesByAccount(axAccountID);

            if (invoices != null && invoices.Count > 0)
                invoices = invoices.Where(i => i.InvoiceAmount > 0 && i.Balance != null && i.Balance != 0).ToList();

            if (invoices != null && invoices.Count > 0)
            {
                if (invoiceItems == null)
                    invoiceItems = new List<InvoiceItems>();

                foreach (var invoice in invoices)
                {
                    string invoicePrefix = invoice.InvoiceID.Substring(0, 4);
                    string invoiceDt = invoice.InvoiceDate.ToShortDateString();

                    if (invoicePrefix == "MRN-" || invoicePrefix == "INS-")
                    {
                        DateTime? tmpInvDate = Helpers.HAXInvoice.GetPreERPInvoiceDate(invoice.InvoiceID, instadoseProcessing);
                        if (tmpInvDate != null)
                            invoiceDt = ((DateTime)tmpInvDate).ToShortDateString();
                    }

                    invoiceItems.Add(new InvoiceItems
                    {
                        AccountID = invoice.Account,
                        CompanyName = companyName,
                        InvoiceNo = invoice.InvoiceID,
                        InvoiceDate = invoiceDt,    //invoice.InvoiceDate.ToShortDateString(),
                        InvoiceAmt = invoice.InvoiceAmount,
                        Balance = (decimal)invoice.Balance,
                        CurrencyCode = (from c in idc.Currencies where c.CurrencyCode == invoice.CurrencyCode select c).FirstOrDefault().OrbitalCurrencyCode,
                        ExponentCode = (from c in idc.Currencies where c.CurrencyCode == invoice.CurrencyCode select c).FirstOrDefault().OrbitalExponentCode,
                        OrderID = new int?(),
                        DaysSinceInvoice = (DateTime.Today - invoice.InvoiceDate).Days
                    });
                }
            }

            if (invoiceItems == null || invoiceItems.Count == 0)
            {
                this.totalBar.Visible = false;
                this.spnCreditCardInformation.Visible = false;
                this.btnPrintSummaryStatement.Enabled = false;

                this.txtAccountNumber.Focus();
                //return;
            }
            else
            {
                this.totalBar.Visible = true;
                this.spnCreditCardInformation.Visible = true;
                this.btnPrintSummaryStatement.Enabled = true;

                invoiceItems = invoiceItems.OrderByDescending(i => DateTime.Parse(i.InvoiceDate)).ToList();

                gvActiveInvoices.DataSource = invoiceItems;
                gvActiveInvoices.DataBind();
            }

            // Set Account ID to Hidden Field and Name (ID) to Label in the Modal/Dialog.
            this.hdnfldAccountID.Value = AccountID.ToString();
            this.lblAccountNumber.Text = AccountID.ToString();
            this.lblCompanyName.Text = companyName.ToUpper();
        }

        private class InvoiceItems
        {
            public string AccountID { get; set; }
            public string CompanyName { get; set; }
            public string InvoiceNo { get; set; }
            public string InvoiceDate { get; set; }
            public decimal? InvoiceAmt { get; set; }
            public decimal? Balance { get; set; }
            public string CurrencyCode { get; set; }
            public string ExponentCode { get; set; }
            public int? OrderID { get; set; }
            public int DaysSinceInvoice { get; set; }
        }

        /// <summary>
        /// Based on selected CheckBox CHECKED, "Balance" value is displayed in "Payment" column's TextBox.
        /// Also, "Total" is kept track of and displayed as CheckBoxes are CHECKED and UNCHECKED.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void chkbxInvoiceSelect_CheckedChanged(object sender, EventArgs e)
        {
            // Clear-out Error/Success Messages.
            InvisibleErrors();
            InvisibleSuccesses();

            decimal totalUSD = 0;

            foreach (GridViewRow row in gvActiveInvoices.Rows)
            {
                CheckBox chkbx = (CheckBox)row.FindControl("chkbxInvoiceSelect");
                Label lbl = (Label)(row.FindControl("lblBalance"));
                TextBox txtPayment = (TextBox)(row.FindControl("txtPayment"));
                string myInvoiceNumber = row.Cells[3].Text.Trim();
                string myInvoiceAmt = row.Cells[5].Text.Trim();

                decimal amount = 0;

                if (chkbx != null && chkbx.Checked)
                {
                    // Display SELECTED Amount in GridView TextBox (only).
                    txtPayment.Text = lbl.Text;

                    // Get SELECTED Amount(s) and display in GridView Footer "Total" Label.
                    decimal.TryParse(lbl.Text, out amount);
                    totalUSD += amount;
                }
                else
                {
                    // Clear "Payment" column value.
                    txtPayment.Text = string.Empty;
                }
            }

            // Get, Set, and Format (default) lblTotal value.
            this.lblTotal.Text = string.Format("{0:0.00}", totalUSD);

            // Get, Set, and Format (default) lblTotalToBeChargedToCC value.
            this.txtTotalToBeChargedToCC.Text = string.Format("{0:0.00}", totalUSD);

            if (txtTotalToBeChargedToCC.Text == "0.00")
            {
                this.btnProcesssInvoicePayment.Enabled = false;
            }
            else
            {
                this.btnProcesssInvoicePayment.Enabled = true;
            }
        }

        protected void txtPayment_TextChanged(object sender, EventArgs e)
        {
            // Clear-out Error/Success Messages.
            InvisibleErrors();
            InvisibleSuccesses();

            decimal totalUSD = 0;

            foreach (GridViewRow row in gvActiveInvoices.Rows)
            {
                CheckBox chkbx = (CheckBox)row.FindControl("chkbxInvoiceSelect");
                TextBox txtPayment = (TextBox)(row.FindControl("txtPayment"));
                Label txtBalance = (Label)(row.FindControl("lblBalance"));

                string myInvoiceNumber = row.Cells[3].Text.Trim();
                string myInvoiceAmt = row.Cells[5].Text.Trim();

                decimal amount = 0;

                if (chkbx != null && chkbx.Checked)
                {
                    //----------------------- Begin Validate Payment TextBox value ---------------------------------//
                    // we make sure that the value in the payment about is a number.
                    if (!IsNumeric(txtPayment.Text))
                    {
                        VisibleErrors("The selected invoice does not have a valid amount.");
                        chkbx.Checked = false;
                        txtPayment.Text = "";
                        return;
                    }

                    // Check if the amount to pay is less or equal the remaining Balance of the Invoice.
                    decimal paymentAmount = 0;
                    decimal remainingBalance = 0;
                    paymentAmount = Convert.ToDecimal(txtPayment.Text);
                    remainingBalance = Convert.ToDecimal(txtBalance.Text);

                    if (paymentAmount > remainingBalance)
                    {
                        VisibleErrors("The amount being paid exceed the remaining balance.");
                        chkbx.Checked = false;
                        txtPayment.Text = "";
                        return;
                    }

                    // Check if the amount to pay today is less or equal the remaining Balance of the Invoice.                
                    decimal pendingPostAmount = 0;
                    var sum = (from p in idc.Payments where p.InvoiceNo == myInvoiceNumber && p.PaymentDate.Value >= DateTime.Today select p.Amount).Sum();
                    pendingPostAmount = sum.HasValue ? sum.Value : 0;

                    if ((paymentAmount + pendingPostAmount) > remainingBalance)
                    {
                        VisibleErrors("The amount being paid exceed the remaining balance. Please check the payment history. You might incur a duplicate payment.");
                        chkbx.Checked = false;
                        txtPayment.Text = "";
                        return;
                    }

                    //-----------------------End Validate Payment TextBox value ---------------------------------//   

                    // Get SELECTED Amount(s) and display in GridView Footer "Total" Label.
                    decimal.TryParse(txtPayment.Text, out amount);
                    totalUSD += amount;
                }
            }

            // Get, Set, and Format (default) lblTotal value.
            this.lblTotal.Text = string.Format("{0:0.00}", totalUSD);

            // Get, Set, and Format (default) lblTotalToBeChargedToCC value.
            this.txtTotalToBeChargedToCC.Text = string.Format("{0:0.00}", totalUSD);

            if (txtTotalToBeChargedToCC.Text == "0.00")
            {
                this.btnProcesssInvoicePayment.Enabled = false;
            }
            else
            {
                this.btnProcesssInvoicePayment.Enabled = true;
            }
        }

        private DataTable GetActiveInvoicesDataTableforGrid(DataTable pDT)
        {
            DataTable dtInvoices = new DataTable();

            // Create the review orders datatable to hold the contents of the order.
            dtInvoices = new DataTable("Invoice Table");

            // Create the columns for the review orders datatable.
            dtInvoices.Columns.Add("AccountID", Type.GetType("System.String"));
            dtInvoices.Columns.Add("CompanyName", Type.GetType("System.String"));
            dtInvoices.Columns.Add("InvoiceNo", Type.GetType("System.String"));
            dtInvoices.Columns.Add("InvoiceDate", Type.GetType("System.DateTime"));
            dtInvoices.Columns.Add("InvoiceAmt", Type.GetType("System.String"));
            dtInvoices.Columns.Add("Balance", Type.GetType("System.String"));
            dtInvoices.Columns.Add("CurrencyCode", Type.GetType("System.String"));
            dtInvoices.Columns.Add("ExponentCode", Type.GetType("System.String"));
            dtInvoices.Columns.Add("OrderID", Type.GetType("System.String"));


            foreach (DataRow row in pDT.Rows)
            {
                // Create a new review order row.
                DataRow dr = dtInvoices.NewRow();

                dr["AccountID"] = row["Account"].ToString();
                dr["CompanyName"] = row["CompanyName"].ToString();
                dr["InvoiceNo"] = row["InvoiceNo"].ToString();
                dr["InvoiceDate"] = row["InvoiceDate"].ToString();
                dr["InvoiceAmt"] = Convert.ToDouble(row["InvoiceAmt"]).ToString("F", CultureInfo.InvariantCulture);
                dr["Balance"] = Convert.ToDouble(row["InvoiceBalance"]).ToString("F", CultureInfo.InvariantCulture);

                Currency myCurrency = (from c in idc.Currencies where c.CurrencyCode == row["CurrencyCode"].ToString() select c).FirstOrDefault();
                if (myCurrency != null)
                {
                    dr["CurrencyCode"] = myCurrency.OrbitalCurrencyCode;
                    dr["ExponentCode"] = myCurrency.OrbitalExponentCode;
                }
                else
                {
                    dr["CurrencyCode"] = "";
                    dr["ExponentCode"] = "";
                }

                dr["OrderID"] = "";

                // Add row to the data table.
                dtInvoices.Rows.Add(dr);
            }

            dtInvoices.DefaultView.Sort = "InvoiceDate DESC";
            return dtInvoices;
        }

        private string getCompanyName(DataTable pDT)
        {
            return pDT.Rows[0]["CompanyName"].ToString();
        }

        #endregion

        #region POPULATE ACCOUNT CREDIT CARD DDL
        private void PopulateAccountCreditCardsDDL(string accountID, int accountCreditCardID = 0)
        {
            try
            {
                // Load drop down list credit cards
                this.ddlAccountCreditCards.Items.Clear();

                if (this.instadoseProcessing)
                {
                    // Query the Account for Active Credit Cards.
                    var getInstadoseAccountCreditCards = (from acc in idc.AccountCreditCards
                                                          join cct in idc.CreditCardTypes on acc.CreditCardTypeID equals cct.CreditCardTypeID
                                                          where acc.AccountID == Convert.ToInt32(accountID)
                                                          && acc.Active == true
                                                          && (acc.UseOnce == false || acc.UseOnce == (bool?)null)
                                                          && acc.NumberEncrypted != ""
                                                          && (Convert.ToDateTime(acc.ExpMonth.ToString() + "/1/" + acc.ExpYear.ToString()) > DateTime.Now.AddDays(-30))
                                                          orderby acc.AccountCreditCardID descending
                                                          select new
                                                          {
                                                              AccountCCID = acc.AccountCreditCardID,
                                                              MaskedCCNumber = String.Format("{0} {1}", Common.MaskCreditCardNumber(acc.NumberEncrypted, cct.CreditCardName), FormatExpirationDate(acc.ExpMonth, acc.ExpYear)),
                                                              Expired = (Convert.ToDateTime(acc.ExpMonth.ToString() + "/27/" + acc.ExpYear.ToString()) < DateTime.Now.AddDays(30))
                                                          });

                    this.ddlAccountCreditCards.DataSource = getInstadoseAccountCreditCards;
                    this.ddlAccountCreditCards.DataValueField = "AccountCCID";
                    this.ddlAccountCreditCards.DataTextField = "MaskedCCNumber";
                    this.ddlAccountCreditCards.DataBind();

                    this.ddlAccountCreditCards.Items.Insert(0, new ListItem("-Select Credit Card-", "0"));
                    this.ddlAccountCreditCards.Focus();

                    // Color (red) the ListItem(s) that is/are within the 30 days of Expiring.
                    int counter = 1;
                    foreach (var obj in getInstadoseAccountCreditCards)
                    {
                        if (obj.Expired.ToString().ToLower() == "true")
                        {
                            this.ddlAccountCreditCards.Items[counter].Attributes.Add("style", "background: #FF0000; color: #FFFFFF;");
                        }
                        counter++;
                    }
                }
                else
                {
                    // Query the Account for Active Credit Cards.
                    var getUnixAccountCreditCards = (from acc in idc.AccountCreditCards
                                                     join cct in idc.CreditCardTypes on acc.CreditCardTypeID equals cct.CreditCardTypeID
                                                     where acc.GDSAccount == accountID
                                                     && acc.Active == true
                                                     && (acc.UseOnce == false || acc.UseOnce == (bool?)null)
                                                     && acc.NumberEncrypted != ""
                                                     && (Convert.ToDateTime(acc.ExpMonth.ToString() + "/1/" + acc.ExpYear.ToString()) > DateTime.Now.AddDays(-30))
                                                     orderby acc.AccountCreditCardID descending
                                                     select new
                                                     {
                                                         AccountCCID = acc.AccountCreditCardID,
                                                         MaskedCCNumber = String.Format("{0} {1}", Common.MaskCreditCardNumber(acc.NumberEncrypted, cct.CreditCardName), FormatExpirationDate(acc.ExpMonth, acc.ExpYear)),
                                                         Expired = (Convert.ToDateTime(acc.ExpMonth.ToString() + "/27/" + acc.ExpYear.ToString()) < DateTime.Now.AddDays(30))
                                                     });

                    this.ddlAccountCreditCards.DataSource = getUnixAccountCreditCards;
                    this.ddlAccountCreditCards.DataValueField = "AccountCCID";
                    this.ddlAccountCreditCards.DataTextField = "MaskedCCNumber";
                    this.ddlAccountCreditCards.DataBind();

                    this.ddlAccountCreditCards.Items.Insert(0, new ListItem("-Select Credit Card-", "0"));
                    this.ddlAccountCreditCards.Focus();

                    // Color (red) the ListItem(s) that is/are within the 30 days of Expiring.
                    int counter = 1;
                    foreach (var obj in getUnixAccountCreditCards)
                    {
                        if (obj.Expired.ToString().ToLower() == "true")
                        {
                            this.ddlAccountCreditCards.Items[counter].Attributes.Add("style", "background: #FF0000; color: #FFFFFF;");
                        }
                        counter++;
                    }
                }  // if..else..

                // Select the newly added credit card.
                if (accountCreditCardID > 0)
                {
                    foreach (ListItem item in ddlAccountCreditCards.Items)
                    {
                        if (item.Value == accountCreditCardID.ToString())
                        {
                            this.ddlAccountCreditCards.SelectedValue = item.Value;
                            //item.Selected = (item.Value == accountCreditCardID.ToString());
                            ddlAccountCreditCards_SelectedIndexChanged(null, null);
                            break;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                VisibleErrors(ex.ToString());
            }
        }

        // Format Expiration Date (given Expiration Month and Year).
        private string FormatExpirationDate(int expmonth, int expyear)
        {
            string expirationDate = "";

            expirationDate = String.Format(" (Exp: {0}/{1})", Convert.ToString(expmonth), Convert.ToString(expyear));

            return expirationDate;
        }
        #endregion

        #region POPULATE GVPAYMENTHISTORY (GET DATA)

        private void PopulatePaymentHistoryGridView(string accountID)
        {
            try
            {
                this.gvPaymentHistory.DataSource = null;
                this.gvPaymentHistory.DataBind();

                if (this.instadoseProcessing)
                {
                    var ccpayments = (from a in idc.sp_AccountPaymentHistory(accountID, "instadose")
                                      select new
                                      {
                                          CompanyName = a.CompanyName,
                                          DateOfPayment = a.DateOfPayment,
                                          CreditCardNumber = Common.MaskCreditCardNumber(a.EncryptedCreditCardNumber, a.TypeOfCreditCard),
                                          TypeOfCreditCard = a.TypeOfCreditCard,
                                          Amount = a.Amount,
                                          InvoiceNo = a.InvoiceNo,
                                          DocumentID = a.DocumentID,
                                          DocumentGUID = a.DocumentGUID,
                                          PaymentID = a.PaymentID,
                                      }).OrderByDescending(c => DateTime.Parse(c.DateOfPayment)).ToList();

                    this.gvPaymentHistory.DataSource = ccpayments;
                    this.gvPaymentHistory.DataBind();
                }
                else
                {
                    var ccpayments = (from a in idc.sp_AccountPaymentHistory(accountID, "unix")
                                      select new
                                      {
                                          CompanyName = a.CompanyName,
                                          DateOfPayment = a.DateOfPayment,
                                          CreditCardNumber = Common.MaskCreditCardNumber(a.EncryptedCreditCardNumber, a.TypeOfCreditCard),
                                          TypeOfCreditCard = a.TypeOfCreditCard,
                                          Amount = a.Amount,
                                          InvoiceNo = a.InvoiceNo,
                                          DocumentID = a.DocumentID,
                                          DocumentGUID = a.DocumentGUID,
                                          PaymentID = a.PaymentID,
                                      }).OrderByDescending(c => DateTime.Parse(c.DateOfPayment)).ToList();

                    this.gvPaymentHistory.DataSource = ccpayments;
                    this.gvPaymentHistory.DataBind();
                }

            }
            catch (Exception ex)
            {
                VisibleErrors(ex.ToString());
                return;
            }
        }

        protected void imgbtnPrintReceipt_Click(object sender, ImageClickEventArgs e)
        {
            ImageButton imgbtn = (ImageButton)sender;
            string imgbtnCommandName = imgbtn.CommandName.ToString();
            string selectedPaymentID = imgbtn.CommandArgument.ToString();

            if (imgbtnCommandName != "PrintReceipt") return;

            if (selectedPaymentID == null) return;

            string url = "PaymentReceipt.aspx?ID=" + selectedPaymentID;
            ScriptManager.RegisterStartupScript(this, typeof(string), "OPEN_WINDOW", "window.open( '" + url + "');", true);
        }

        protected void gvPaymentHistory_Sorting(object sender, GridViewSortEventArgs e)
        {
            this.gvPaymentHistory.PageIndex = 0;
        }

        protected void gvPaymentHistory_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            this.gvPaymentHistory.PageIndex = e.NewPageIndex;
            PopulatePaymentHistoryGridView(accountID);
        }
        #endregion

        #region ON_CLICK BTNSAVE (IN MODAL/DIALOG)

        protected void btnSave_Click(object sender, EventArgs e)
        {
            // Clear any old/existing Error Message.
            InvisibleErrorsModal();

            int expMonth = 0;
            int expYear = 0;
            string creditCardNumber = "";
            string nameOnCard = "";
            string securityCode = "";
            string encryptedNumber = "";

            // Convert the Expiration Month and Year to Integers.
            int.TryParse(this.ddlExpirationMonth.SelectedValue, out expMonth);
            int.TryParse(this.ddlExpirationYear.SelectedValue, out expYear);

            // Trim the TextBox values and put them into local variables.
            creditCardNumber = this.txtCreditCardNumber.Text.Trim();
            nameOnCard = this.txtNameOnCard.Text.Trim();
            securityCode = this.txtSecurityCode.Text.Trim();

            // Tdo, 03/31/2015. Right now bypass the credit card verification code
            //// Ensure the Credit Card information is provided.
            //if (nameOnCard == string.Empty || creditCardNumber == string.Empty || securityCode == string.Empty)
            //{
            //    VisibleErrorsModal("Please provide all required information before continuing.");
            //    return;
            //}
            // Ensure the Credit Card information is provided.
            if (nameOnCard == string.Empty || creditCardNumber == string.Empty)
            {
                VisibleErrorsModal("Please provide all required information before continuing.");
                return;
            }

            // Validate the Credit Card Number.
            Instadose.Security.CreditCardType? cardType = Instadose.Security.Validation.ValidateCreditCard(creditCardNumber);
            if (!cardType.HasValue)
            {
                VisibleErrorsModal("Credit card number is invalid. Please verify and try again.");
                return;
            }

            // Validate CC expiration.
            DateTime ccExpireDate = Convert.ToDateTime(expMonth.ToString() + "/01/" + expYear.ToString());
            DateTime curMonthYearDate = Convert.ToDateTime(DateTime.Today.Month.ToString() + "/01/" + DateTime.Today.Year.ToString());
            if (ccExpireDate.Ticks < curMonthYearDate.Ticks)
            {
                VisibleErrorsModal("Expiration date is in the past. Please verify and try again.");
                return;
            }

            // Convert the Credit Card Type into a CreditCardTypeID in the database.
            int cardTypeID = 0;
            string creditCardType = "";
            switch (cardType.Value)
            {
                case Instadose.Security.CreditCardType.AmericanExpress:
                    cardTypeID = 4;
                    creditCardType = "American Express";
                    break;
                case Instadose.Security.CreditCardType.Discover:
                    cardTypeID = 3;
                    creditCardType = "Discover";
                    break;
                case Instadose.Security.CreditCardType.MasterCard:
                    cardTypeID = 2;
                    creditCardType = "MasterCard";
                    break;
                case Instadose.Security.CreditCardType.Visa:
                    cardTypeID = 1;
                    creditCardType = "Visa";
                    break;
            }

            // Display Error Message if the Card Type was not set.
            if (cardTypeID == 0)
            {
                VisibleErrorsModal("Credit card type was not understood.");
                return;
            }

            // Tdo, 03/31/2015. Right now bypass the credit card verification code
            // Check the CSV code
            if (!string.IsNullOrEmpty(securityCode))
            {
                if (creditCardType == "Discover" || creditCardType == "MasterCard" || creditCardType == "Visa")
                {
                    if (securityCode.Length != 3)
                    {
                        VisibleErrorsModal("CVV/CVC must be 3 characters length.");
                        return;
                    }
                }

                if (creditCardType == "American Express")
                {
                    if (securityCode.Length != 4)
                    {
                        VisibleErrorsModal("CVV/CVC must be 4 characters length.");
                        return;
                    }
                }
            }

            // Encrypt the Valid Credit Card Number.
            encryptedNumber = Instadose.Security.TripleDES.Encrypt(creditCardNumber);

            if (!chkbxUseOnlyOnce.Checked) // Save to Database.
            {
                // Look for existing Credit Cards with same Number.
                AccountCreditCard card = new AccountCreditCard();

                if (this.instadoseProcessing)
                {
                    card = (from acc in idc.AccountCreditCards where acc.NumberEncrypted == encryptedNumber && (acc.UseOnce == false || acc.UseOnce == (bool?)null) && acc.AccountID == Convert.ToInt32(accountID) select acc).FirstOrDefault();
                }
                else
                {
                    card = (from acc in idc.AccountCreditCards where acc.NumberEncrypted == encryptedNumber && (acc.UseOnce == false || acc.UseOnce == (bool?)null) && acc.GDSAccount == accountID select acc).FirstOrDefault();
                }

                // If the Credit Card DOES NOT exist.
                if (card == null)
                {
                    // CREATE a new Credit Card record for the Account.
                    card = new AccountCreditCard()
                    {
                        AccountID = this.instadoseProcessing ? Convert.ToInt32(accountID) : 2,
                        CreditCardTypeID = cardTypeID,
                        NameOnCard = nameOnCard,
                        NumberEncrypted = encryptedNumber,
                        SecurityCode = securityCode,
                        ExpMonth = expMonth,
                        ExpYear = expYear,
                        Active = true,
                        GDSAccount = this.instadoseProcessing ? null : accountID.ToUpper()
                    };

                    idc.AccountCreditCards.InsertOnSubmit(card);
                    idc.SubmitChanges();
                }
                else // UPDATE existing Credit Card.
                {
                    card.AccountID = this.instadoseProcessing ? Convert.ToInt32(accountID) : 2;
                    card.CreditCardTypeID = cardTypeID;
                    card.NameOnCard = nameOnCard;
                    card.NumberEncrypted = encryptedNumber;
                    card.SecurityCode = securityCode;
                    card.ExpMonth = expMonth;
                    card.ExpYear = expYear;
                    card.Active = true;
                    card.GDSAccount = this.instadoseProcessing ? null : accountID.ToUpper();

                    // Save changes.
                    idc.SubmitChanges();
                }

                // Rebind the Account Credit Cards DDL.
                if (this.txtAccountNumber.Text != "") { PopulateAccountCreditCardsDDL(accountID, card.AccountCreditCardID); }

                Session["UseOnlyOnce"] = false;
            }
            else // Use Only Once (save to temporary variables/HiddenFields).
            {
                Session["UseOnlyOnce"] = true;
                Session["TempCreditCardNumber"] = encryptedNumber;
                Session["TempNameOnCard"] = nameOnCard;
                Session["TempExpirationMonth"] = expMonth.ToString();
                Session["TempExpirationYear"] = expYear.ToString();
                Session["TempCVV"] = securityCode;
                Session["TempCreditCardTypeID"] = cardTypeID;

                // Display Credit Card information in "Process Payment" section.
                this.divProcessPayment.Visible = true;
                this.lblCreditCardBeingUsed.Text = String.Format("{0} {1}", Common.MaskCreditCardNumber(encryptedNumber, creditCardType), FormatExpirationDate(expMonth, expYear));
            }

            // Close modal dialog.
            ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divNewCreditCardInformation')", true);

            // Reset modal controls (to defaults).
            ResetModalControlsToDefault();
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            ResetModalControlsToDefault();
        }

        #endregion

        #region ON_CLICK BTNPROCESSINVOICEPAYMENT
        /// <summary>
        /// Only display the "Process Payment" section if the following are true;
        /// 1.) Total > 0, Invoice(s) is/are selected from GridView.
        /// 2.) A valid Credit Card is selected from ddlAccountCreditCards.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ddlAccountCreditCards_SelectedIndexChanged(object sender, EventArgs e)
        {
            decimal getTotal = 0;
            decimal.TryParse(txtTotalToBeChargedToCC.Text, out getTotal);

            bool useOnlyOnce = Convert.ToBoolean(Session["UseOnlyOnce"]);

            if (useOnlyOnce == false && this.ddlAccountCreditCards.SelectedValue == "0")
            {
                this.divProcessPayment.Visible = false;
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "alert('Please select a Credit Card.')", true);
                return;
            }

            if (getTotal == 0)
            {
                // Reset ddlAccountCreditCards to "0".
                this.ddlAccountCreditCards.SelectedValue = "0";
                this.divProcessPayment.Visible = false;
                ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "alert('Please select Invoices to be charged.')", true);
                return;
            }

            if (this.ddlAccountCreditCards.SelectedValue != "0" && getTotal != 0)
            {
                this.divProcessPayment.Visible = true;
                this.lblCreditCardBeingUsed.Text = this.ddlAccountCreditCards.SelectedItem.Text;
            }
        }

        private bool PassPaymentValidation()
        {
            var isValid = true;
            foreach (GridViewRow row in this.gvActiveInvoices.Rows)
            {
                CheckBox chkbx = (CheckBox)row.FindControl("chkbxInvoiceSelect");
                TextBox txtPayment = (TextBox)(row.FindControl("txtPayment"));
                Label txtBalance = (Label)(row.FindControl("lblBalance"));

                string myInvoiceNumber = row.Cells[3].Text.Trim();
                string myInvoiceAmt = row.Cells[5].Text.Trim();

                if (chkbx.Checked == true)
                {
                    // Check if the (current pay amount + already paid amount) <= Invoice amount
                    decimal alreadyPaid = 0;
                    var sum = (from p in idc.Payments where p.InvoiceNo == myInvoiceNumber select p.Amount).Sum();
                    alreadyPaid = sum.HasValue ? sum.Value : 0;

                    decimal paymentAmount = 0;
                    decimal remainingBalance = 0;
                    paymentAmount = Convert.ToDecimal(txtPayment.Text);
                    remainingBalance = Convert.ToDecimal(txtBalance.Text);

                    if (Convert.ToDecimal(myInvoiceAmt) < (alreadyPaid + paymentAmount))
                    {
                        VisibleErrors("Invoice#: " + myInvoiceNumber + " has the amount being paid exceed the actual remaining balance.");
                        chkbx.Checked = false;
                        txtPayment.Text = "";
                        isValid = false;
                        return false;
                    }

                    // Check if the amount to pay today is less or equal the remaining Balance of the Invoice.                
                    decimal pendingPostAmount = 0;
                    var todaySum = (from p in idc.Payments where p.InvoiceNo == myInvoiceNumber && p.PaymentDate.Value >= DateTime.Today select p.Amount).Sum();
                    pendingPostAmount = todaySum.HasValue ? todaySum.Value : 0;

                    if ((paymentAmount + pendingPostAmount) > remainingBalance)
                    {
                        VisibleErrors("Invoice#: " + myInvoiceNumber + " has the amount being paid exceed the actual remaining balance. Please check the payment history. You might incur a duplicate payment.");
                        chkbx.Checked = false;
                        txtPayment.Text = "";
                        isValid = false;
                        return false;
                    }
                }
            }

            if (!PassEmailAddressValidation(txtSuccessEmail))
            {
                isValid = false;
                return false;
            }

            return isValid;
        }

        protected void btnProcesssInvoicePayment_Click(object sender, EventArgs e)
        {
            try
            {
                // Clear-out Error/Success Messages.
                InvisibleErrors();
                InvisibleSuccesses();

                List<string> errorMsgList = new List<string>();
                List<string> successMsgList = new List<string>();

                // First check to see that the amount being charged is > 0.       
                double getTotal = 0;
                double.TryParse(this.txtTotalToBeChargedToCC.Text, out getTotal);

                if (getTotal == 0)
                {
                    // Display Warning Pop-Up that no Invoices were selected to be charged.
                    ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "alert('Please select Invoices to be charged.')", true);
                    return;
                }

                if (!PassPaymentValidation())
                {
                    return;
                }

                // Here we put in all the Process Credit Card code.        
                string creditCardNumber = string.Empty;
                string expireMonth = string.Empty;
                string expireYear = string.Empty;
                string CVV = string.Empty;
                string cardHolderName = string.Empty;
                int creditCardTypeID = 0;
                int accountCreditCardID = 0;
                int successCount = 0;

                bool useOnlyOnce = Convert.ToBoolean(Session["UseOnlyOnce"]);

                if (useOnlyOnce == false) // Set the Credit Card information from the Database.
                {
                    int.TryParse(this.ddlAccountCreditCards.SelectedValue.ToString(), out accountCreditCardID);

                    var linqCreditCard = (from acc in idc.AccountCreditCards
                                          where acc.AccountCreditCardID == accountCreditCardID
                                          select new
                                          {
                                              CreditCardNumber = acc.NumberEncrypted,
                                              ExpirationMonth = acc.ExpMonth.ToString(),
                                              ExpirationYear = acc.ExpYear.ToString(),
                                              CVV = acc.SecurityCode,
                                              NameOnCard = acc.NameOnCard,
                                              CreditCardTypeID = acc.CreditCardTypeID
                                          }).FirstOrDefault();

                    creditCardNumber = TripleDES.Decrypt(linqCreditCard.CreditCardNumber);
                    expireMonth = linqCreditCard.ExpirationMonth.PadLeft(2, '0');
                    expireYear = linqCreditCard.ExpirationYear;
                    CVV = linqCreditCard.CVV;
                    cardHolderName = linqCreditCard.NameOnCard;
                    creditCardTypeID = linqCreditCard.CreditCardTypeID;
                }
                else // Set the Credit Card information from the HiddenFields.
                {
                    creditCardNumber = TripleDES.Decrypt(Session["TempCreditCardNumber"].ToString());
                    expireMonth = Session["TempExpirationMonth"].ToString().PadLeft(2, '0');
                    //// Dates are recorded "0114" for Jan-2014, we have to add the leading zero for single digit months
                    //if (expireMonth.Length == 1) { expireMonth = "0" + expireMonth; };
                    expireYear = Session["TempExpirationYear"].ToString();
                    CVV = Session["TempCVV"].ToString();
                    cardHolderName = Session["TempNameOnCard"].ToString();
                    creditCardTypeID = Convert.ToInt32(Session["TempCreditCardTypeID"]);

                    // Add one time credit card use. We are going to use this AccountCreditCard info for credit card receipt.
                    // Record AccountCreditCard.
                    // If instadose then AccountID = instadose accountID else AccountID = 2 (mirion account)
                    // If instadose then GDSAccount = null else GDSAccount = Unix accountID
                    AccountCreditCard newACC = new AccountCreditCard()
                    {
                        AccountID = this.instadoseProcessing ? Convert.ToInt32(accountID) : 2,
                        CreditCardTypeID = creditCardTypeID,
                        NameOnCard = cardHolderName,
                        NumberEncrypted = Instadose.Security.TripleDES.Encrypt(creditCardNumber),
                        SecurityCode = CVV,
                        ExpMonth = Int32.Parse(Session["TempExpirationMonth"].ToString()),
                        ExpYear = Int32.Parse(expireYear),
                        Active = true,
                        UseOnce = true,
                        GDSAccount = this.instadoseProcessing ? null : accountID.ToUpper()
                    };

                    // Insert the AccountCreditCard on submit.
                    idc.AccountCreditCards.InsertOnSubmit(newACC);
                    idc.SubmitChanges();

                    // Get new AccountCreditCardID from Inserted record 
                    accountCreditCardID = newACC.AccountCreditCardID;
                }
                bool? accountRestricted = false;
                var udc = new Mirion.DSD.GDS.API.Contexts.UnixDataClassesDataContext();
                //Is account restricted
                if (this.instadoseProcessing)
                {
                    accountRestricted = idc.Accounts.First(a => a.AccountID == Convert.ToInt32(accountID)).RestrictOnlineAccess;
                }
                else
                {
                    accountRestricted = udc.UnixAccounts.First(a => a.GDSAccount == accountID).RestrictOnlineAccess;
                }
                // Orbital Transaction request
                OrbitalPaymenTechAPI creditCard = new OrbitalPaymenTechAPI();
                CreditCardTransactionInfo myTransaction = new CreditCardTransactionInfo();

                myTransaction.CreditCardNumber = creditCardNumber;
                myTransaction.CreditCardSecurityCode = CVV;
                myTransaction.CreditCardExpireMonth = expireMonth;
                myTransaction.CreditCardExpireYear = expireYear;
                myTransaction.CreditCardType = (from cct in idc.CreditCardTypes where cct.CreditCardTypeID == creditCardTypeID select cct.CreditCardName.Trim()).FirstOrDefault();
                myTransaction.CardHolderName = cardHolderName;

                myTransaction.AccountType = this.instadoseProcessing ? 2 : 1;    // 2: Instadose, 1: Unix

                if (instadoseProcessing)
                {
                    FillCreditCardHolderAddress(myTransaction, accountCreditCardID);
                }
                var invoicePastDueCount = 0;
                var pastDuePaidCount = 0;
                // Gets each CHECKED Invoice to process (separately?).
                // This login needs to be defined, will code accordingly.
                // For not the FOREACH LOOP does get each Invoice record.
                foreach (GridViewRow row in this.gvActiveInvoices.Rows)
                {
                    CheckBox chkBx = (CheckBox)row.FindControl("chkbxInvoiceSelect");
                    var invoicePastDue = false;
                    try
                    {
                        invoicePastDue = int.Parse(((HiddenField)row.FindControl("HidDays")).Value) >= 150;
                        if (invoicePastDue)
                            invoicePastDueCount++;
                    }
                    catch
                    {

                    }
                    if (chkBx.Checked == true)
                    {
                        TextBox amount = (TextBox)row.FindControl("txtPayment");

                        string myCurrencyCode = ((HiddenField)row.FindControl("HidCurrencyCode")).Value;
                        string myCurrencyExponent = ((HiddenField)row.FindControl("HidExponentCode")).Value;

                        string myInvoiceNumber = row.Cells[3].Text.Trim();
                        string myCompanyName = row.Cells[2].Text.Trim();
                        int? myOrderID = (int?)null;          // Since Unix does not have OrderID so OrderID = null

                        // Since AX Invoice contains multiple orders, OrderID should be null for now
                        //if (instadoseProcessing)
                        //    myOrderID = int.Parse(((HiddenField)row.FindControl("HidOrderID")).Value);

                        myTransaction.Amount = Math.Round(Convert.ToDouble(amount.Text), 2);
                        myTransaction.OrbitalOrderNumber = myInvoiceNumber;

                        myTransaction.CurrencyCode = myCurrencyCode;     // Bristish pound, Canadian, USD, Australian,...
                        myTransaction.CurrencyExponent = myCurrencyExponent; // 1 or 2

                        creditCard.DoDirectPayment(myTransaction);

                        bool success = myTransaction.Success;
                        string errorMsg = myTransaction.ErrorMsg;
                        string transCode = myTransaction.TransCode;

                        if (success)
                        {
                            successCount++;

                            // create receipt doc, generate sucess message with invoice no & transaction code
                            successMsgList.Add("The transaction is approved for invoice: " + myInvoiceNumber + ". Transaction Code: " + transCode);

                            // Record payment header data.
                            Payment newPayment = new Payment()
                            {
                                OrderID = myOrderID,
                                InvoiceNo = myInvoiceNumber,
                                PaymentDate = DateTime.Now,
                                Amount = Math.Round(decimal.Parse(amount.Text), 2),
                                GatewayResponse = String.Empty,
                                TransCode = transCode,
                                Captured = true,
                                Authorized = true,
                                PaymentMethodID = 1,
                                AccountCreditCardID = accountCreditCardID,
                                CreatedBy = this.UserName,
                                CurrencyCode = (from c in idc.Currencies where c.OrbitalCurrencyCode == myCurrencyCode select c.CurrencyCode).FirstOrDefault(),
                                ApplicationID = this.instadoseProcessing ? 2 : 1    // 2: Instadose, 1: Unix
                            };

                            // Insert the payment on submit.
                            idc.Payments.InsertOnSubmit(newPayment);
                            idc.SubmitChanges();

                            // Get new PaymentID from Inserted record and generate payment receipt
                            int paymentid = newPayment.PaymentID;

                            // Encrypt the Valid Credit Card Number.
                            string encryptedCreditCardNumber = Instadose.Security.TripleDES.Encrypt(myTransaction.CreditCardNumber);

                            // Mask the credit card number.
                            string maskedCardNum = Common.MaskCreditCardNumber(encryptedCreditCardNumber, myTransaction.CreditCardType);

                            generatePDF(paymentid, maskedCardNum, accountID, myOrderID, myCompanyName);

                            EmailTransactionSuccess(accountID, this.instadoseProcessing, newPayment.InvoiceNo, newPayment.PaymentDate.Value, newPayment.Amount.Value, newPayment.CurrencyCode, newPayment.PaymentID, "Credit Card");

                            try
                            {
                                if (invoicePastDue)
                                {
                                    var balanceDueField = ((HiddenField)row.Cells[0].FindControl("HidBalanceDue")).Value;
                                    var balanceDue = double.Parse(balanceDueField.Trim());
                                    if (balanceDue == myTransaction.Amount)
                                        pastDuePaidCount++;
                                }
                            }
                            catch
                            {

                            }
                        }
                        else
                        {
                            // generate error message with invoice no & error message                    
                            errorMsgList.Add("The transaction is declined for invoice: " + myInvoiceNumber + ". Reason: " + errorMsg);
                        }
                    }  // if...then selected check box
                } // end request for loop

                if (successMsgList.Count > 0)
                {
                    // Reload all invoices and history payment of an account
                    btnFind_Click(sender, e);
                    VisibleSuccesses(GenerateBulletMessage(successMsgList));

                    if (accountRestricted.HasValue && accountRestricted.Value && pastDuePaidCount == invoicePastDueCount)
                        UpdateRestrictedAccount();
                }

                if (errorMsgList.Count > 0)
                {
                    // there is not any transaction proceeded successfully then delete the inserted AccountCreditCard record for UseOnlyOnce CC process option
                    if (useOnlyOnce && successCount == 0)
                    {
                        // Query the order.
                        var acctCC = (from a in idc.AccountCreditCards where a.AccountCreditCardID == accountCreditCardID select a).FirstOrDefault();

                        idc.AccountCreditCards.DeleteOnSubmit(acctCC);

                        // Save the changes.
                        idc.SubmitChanges();
                    }

                    // Reload all invoices and history payment of an account
                    btnFind_Click(null, null);
                    VisibleErrors("Orbital Paymentech has incurred the following errors while processing the payment: <br />" + GenerateBulletMessage(errorMsgList));

                    // Turn on decline email functionality
                    divPaymentDecline.Visible = true;
                }
            }
            catch (Exception ex)
            {
                // Report the error to the message system.
                Basics.WriteLogEntry(ex.Message, Page.User.Identity.Name, "Portal.InstaDose_Finance_NewProcessCreditCard", Basics.MessageLogType.Critical);
                VisibleErrors(ex.Message);
            }
        }

        private void UpdateRestrictedAccount()
        {
            var account = txtAccountNumber.Text;
            var overrideDate = DateTime.Today.AddDays(4);
            if (this.instadoseProcessing)
            {
                var accountID = int.Parse(account);
                var accountRecord = idc.Accounts.Where(a => a.AccountID == accountID).FirstOrDefault();
                accountRecord.RestrictOnlineAccess = false;
                var tempOverride = new RestrictedAccountOverride()
                {
                    AccountID = accountID,
                    ExpirationDate = overrideDate
                };
                idc.RestrictedAccountOverrides.InsertOnSubmit(tempOverride);
                idc.SubmitChanges();
            }
            else
            {
                var ar = new AccountRequests();
                var restrictAccess = Mirion.DSD.GDS.API.Helpers.YesNo.No;
                ar.UpdateOnlineAccess(account, restrictAccess);
                ar.InsertRestrictedAccountOverride(account, overrideDate);
            }
        }

        private void FillCreditCardHolderAddress(CreditCardTransactionInfo pMyTransaction, int pAccountCreditCardID)
        {
            try
            {
                AccountCreditCard AcctCC = (from a in idc.AccountCreditCards
                                            where a.AccountCreditCardID == pAccountCreditCardID
                                            select a).FirstOrDefault();

                if (AcctCC == null)
                {
                    Basics.WriteLogEntry("Credit card type is NULL.", UserName,
                        "Portal.InstaDose_Finance_NewProcessCreditCard", Basics.MessageLogType.Critical);

                    return;
                }

                // Get the account info.
                Account account = (from a in idc.Accounts
                                   where a.AccountID == AcctCC.AccountID
                                   select a).FirstOrDefault();
                if (account == null)
                {
                    Basics.WriteLogEntry("Account links to credit card does not exist.", UserName,
                        "Portal.InstaDose_Finance_NewProcessCreditCard", Basics.MessageLogType.Critical);

                    return;
                }

                // Get the default location for the account in the business app.
                Location location = (from l in idc.Locations
                                     where l.AccountID == account.AccountID
                                     && l.IsDefaultLocation == true
                                     select l).FirstOrDefault();
                if (location == null)
                {
                    Basics.WriteLogEntry("Default location does not exist.", UserName,
                        "Portal.InstaDose_Finance_NewProcessCreditCard", Basics.MessageLogType.Critical);

                    return;
                }

                //convert selected billing countryID to PayPalCode
                string CountryCodeStr = "US";

                var myPayPalCode = (from a in idc.Countries
                                    where a.CountryID == location.BillingCountryID
                                    select a.PayPalCode).FirstOrDefault();

                if (myPayPalCode != null) CountryCodeStr = myPayPalCode;

                string myStateAbb = "";
                var myState = (from a in idc.States
                               where a.StateID == location.BillingStateID
                               select a.StateAbbrev).FirstOrDefault();

                if (myState != null) myStateAbb = myState;

                string zipCode = location.BillingPostalCode.Trim();
                string[] zipCodeSplit = zipCode.Split(new string[] { "_", "-", " " }, StringSplitOptions.RemoveEmptyEntries);

                pMyTransaction.CardHolderAddress = location.BillingAddress1.Trim();
                if (pMyTransaction.CardHolderAddress.Length > 30)
                    pMyTransaction.CardHolderAddress = pMyTransaction.CardHolderAddress.Substring(0, 30);
                pMyTransaction.CardHolderAddress1 = (location.BillingAddress2 != null) ? location.BillingAddress2.Trim() : "";
                if (pMyTransaction.CardHolderAddress1.Length > 30)
                    pMyTransaction.CardHolderAddress1 = pMyTransaction.CardHolderAddress1.Substring(0, 30);
                pMyTransaction.CardHolderCity = location.BillingCity.Trim();
                if (pMyTransaction.CardHolderCity.Length > 20)
                    pMyTransaction.CardHolderCity = pMyTransaction.CardHolderCity.Substring(0, 20);
                pMyTransaction.CardHolderState = myStateAbb;
                pMyTransaction.CardHolderZipCode = zipCodeSplit.Count() > 0 ? zipCodeSplit[0].Trim() : "";  // "92831"
                pMyTransaction.CardHolderCountry = CountryCodeStr;
            }
            catch (Exception ex)
            {
                Basics.WriteLogEntry(ex.Message, UserName,
                    "Portal.InstaDose_Finance_NewProcessCreditCard", Basics.MessageLogType.Critical);

                return;
            }
        }

        private void ResetAllSessionValues()
        {
            Session["UseOnlyOnce"] = false;
            Session["TempCreditCardNumber"] = null;
            Session["TempNameOnCard"] = null;
            Session["TempExpirationMonth"] = null;
            Session["TempExpirationYear"] = null;
            Session["TempCVV"] = null;
            Session["TempCreditCardTypeID"] = null;
        }

        private bool IsNumeric(String numberString)
        {
            Double d;
            return Double.TryParse(numberString, out d);
        }

        private bool generatePDF(int paymentID, string maskedCreditCardNumber, string accountID, int? orderID, string companyName)
        {
            string pdfFileName = "PaymentReceipt_" + paymentID.ToString() + ".pdf";
            string myCRFileNamePath = Server.MapPath("PaymentReceipt.rpt");
            Stream oStream = null;
            ReportDocument cryRpt = new ReportDocument();
            byte[] byteArray = null;
            bool success = false;

            // Get UserName (based on System Login).
            string userName = "Unknown";
            if (this.UserName == "" && Page.User.Identity.Name.IndexOf('\\') > 0)
            {
                if (Page.User.Identity.Name.Split('\\')[1] != null)
                    userName = Page.User.Identity.Name.Split('\\')[1];
            }
            else
            {
                userName = this.UserName;
            }
            try
            {
                // First insert Document record with default DocumentContent into database.
                // then PaymentReceipt report will use this doc record to generate DocumentContent.
                // This work-around solution is adapting for both Instadose and Unix CC processing.

                Document dcmntrcrd = new Document()
                {
                    DocumentGUID = Guid.NewGuid(),
                    FileName = pdfFileName,
                    DocumentContent = Enumerable.Repeat((byte)0x20, 7).ToArray(),
                    MIMEType = "application/pdf",
                    DocumentCategory = "Payment Receipt",
                    Description = "Credit Card Payment Receipt",
                    AccountID = this.instadoseProcessing ? Convert.ToInt32(accountID) : (int?)null,
                    OrderID = orderID,
                    CreatedBy = userName,
                    CreatedDate = DateTime.Now,
                    Active = true,
                    ReferenceKey = paymentID,
                    CompanyName = companyName,
                    GDSAccount = this.instadoseProcessing ? null : accountID.ToUpper()
                };

                // Submit record to database.
                idc.Documents.InsertOnSubmit(dcmntrcrd);
                idc.SubmitChanges();

                // Get new PaymentID from Inserted record and generate payment receipt
                int docID = dcmntrcrd.DocumentID;
                if (docID <= 0) return false;

                cryRpt.Load(myCRFileNamePath);

                cryRpt.SetParameterValue("@pPaymentID", paymentID);
                cryRpt.SetParameterValue("@pMaskedCreditCardNumber", maskedCreditCardNumber);

                string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString.ToString();
                string myDatabase = Regex.Match(connStr, "(Initial Catalog=|Database=){1}([A-Za-z0-9_]+)[;]?").Groups[2].ToString();
                string myServer = Regex.Match(connStr, "(Data Source=){1}([\\w\\d\\.\\$\\-]+)[;]?").Groups[2].ToString();
                string myUserID = Regex.Match(connStr, "(User ID=|UID=){1}([\\w\\d\\.]+)[;]?").Groups[2].ToString();
                string myPassword = Regex.Match(connStr, "(Pwd=|Password=|PWD=){1}([\\w\\d\\.\\$]+)[;]?").Groups[2].ToString();

                cryRpt.DataSourceConnections[0].SetConnection(myServer, myDatabase, myUserID, myPassword);

                // Export to Memory Stream.        
                oStream = cryRpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                byteArray = new byte[oStream.Length];
                oStream.Read(byteArray, 0, Convert.ToInt32(oStream.Length - 1));

                // Update inserted doc with real DocumentContent
                // Look for existing Document.
                Document doc = (from d in idc.Documents where d.DocumentID == docID select d).FirstOrDefault();

                if (doc != null)
                {
                    doc.DocumentContent = byteArray;
                    // Save changes.
                    idc.SubmitChanges();

                    success = true;

                    // Report the error to the message system.
                    Basics.WriteLogEntry("PaymentReceipt was generated successful.", Page.User.Identity.Name, "Portal.InstaDose_Finance_PaymentReceipt", Basics.MessageLogType.Information);
                }
                else
                {
                    success = false;

                    // Report the error to the message system.
                    Basics.WriteLogEntry("PaymentReceipt was not generated successful.", Page.User.Identity.Name, "Portal.InstaDose_Finance_PaymentReceipt", Basics.MessageLogType.Critical);
                }
            }
            catch (Exception ex)
            {
                // Report the error to the message system.
                Basics.WriteLogEntry(ex.Message, Page.User.Identity.Name, "Portal.InstaDose_Finance_PaymentReceipt", Basics.MessageLogType.Critical);
                success = false;

                VisibleErrors(ex.Message);
            }
            finally
            {
                cryRpt.Close();
                cryRpt.Dispose();

                oStream.Flush();
                oStream.Close();
                oStream.Dispose();
            }

            return success;
        }
        #endregion
        
        #region SEND MAIL WITH IMAGE
        private bool EmailTransactionSuccess(string AccountID, bool pInstadoseProcessing, string pInvoiceNo, DateTime pPaymentDate, decimal pAmount, string pCurrencyCode, int pPaymentID, string pPaymentMethod)
        {
            if (txtSuccessEmail.Text.Trim().Length > 0)
            {
                bool RtnResponse = false;
                string toEmail = txtSuccessEmail.Text;

                //// Create the template.
                MessageSystem msgSystem = new MessageSystem()
                {
                    Application = "Instadose.com Credit Card Process Acknowledgement",
                    CreatedBy = this.UserName,
                    FromAddress = "noreply@instadose.com",
                    ToAddressList = new List<string>()
                };

                // Add the email
                msgSystem.ToAddressList.Add(toEmail);

                // Replace the fields in the template
                Dictionary<string, string> fields = new Dictionary<string, string>();

                // Add the fields to replace.
                // MUST BE EXACT with email template stored EmailTemplates table.  Check
                // description for fields that need to have values for your document.
                //string amountStr = "$" + pAmount.ToString() + " " + pCurrencyCode;
                string amountStr = pAmount.ToString() + " " + pCurrencyCode;

                fields.Add("InvoiceNumber", pInvoiceNo);
                fields.Add("TransactionDate", String.Format("{0:MMM dd, yyyy}", pPaymentDate));
                fields.Add("PaymentAmount", amountStr);
                fields.Add("PaymentMethod", pPaymentMethod);
                fields.Add("ConfirmationNumber", pPaymentID.ToString());

                // MUST BE EXACT with EmailTemplate (TemplateName)! 
                int response = msgSystem.Send("Payment Confirmation", "", fields);

                if (response != 0) RtnResponse = true; //success!

                return RtnResponse;
            }
            else
            {
                return true;
            }

        }

        private bool EmailTransactionFail(string AccountID, bool pInstadoseProcessing)
        {
            if (txtUnsuccessEmail.Text.Trim().Length > 0)
            {
                bool RtnResponse = false;

                string toEmail = txtUnsuccessEmail.Text;

                //// Create the template.
                MessageSystem msgSystem = new MessageSystem()
                {
                    Application = "Instadose.com Credit Card Process Acknowledgement",
                    CreatedBy = this.UserName,
                    FromAddress = "noreply@instadose.com",
                    ToAddressList = new List<string>()
                };

                // Add the email
                msgSystem.ToAddressList.Add(toEmail);

                // Replace the fields in the template
                Dictionary<string, string> fields = new Dictionary<string, string>();

                // Add the fields to replace.
                // MUST BE EXACT with email template stored EmailTemplates table.  Check
                // description for fields that need to have values for your document.

                fields.Add("SalutationName", "Valued Customer");

                // MUST BE EXACT with EmailTemplate (TemplateName)! 
                int response = msgSystem.Send("Payment Declined", "", fields);

                if (response != 0) RtnResponse = true; //success!

                return RtnResponse;
            }
            else
            {
                return true;
            }

        }

        protected void btnCCDeclineEmail_Click(object sender, EventArgs e)
        {
            // Clear-out Error/Success Messages.
            InvisibleErrors();
            InvisibleSuccesses();

            if (!PassEmailAddressValidation(txtUnsuccessEmail))
            {
                return;
            }

            EmailTransactionFail(accountID, this.instadoseProcessing);
        }

        private bool PassEmailAddressValidation(TextBox pEmailTextBox)
        {
            if (pEmailTextBox.Text.Trim().Length > 0)
            {
                string regExEmail = @"^(?("")("".+?""@)|(([0-9a-zA-Z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-zA-Z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-zA-Z][-\w]*[0-9a-zA-Z]\.)+[a-zA-Z]{2,6}))$";

                // Create a regular expression for the email.
                Regex regEmail = new Regex(regExEmail);

                if (!regEmail.IsMatch(pEmailTextBox.Text.Trim()))
                {
                    VisibleErrors("Email address is not valid.");
                    pEmailTextBox.Focus();
                    return false;
                }
            }

            return true;
        }
        #endregion
    }
}