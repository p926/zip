using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Configuration;
using System.IO;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Text.RegularExpressions;

using Instadose;
using Instadose.Data;
using Instadose.Processing;

using Instadose.Integration.PayPal;
//using com.paypal.soap.api;

using PayPal.PayPalAPIInterfaceService.Model;


public partial class Finance_ProcessCreditCard : System.Web.UI.Page
{
    // PayPal Sandbox
    // https://www.sandbox.paypal.com/
    // UserName: ddelap_1338324886_biz@mirion.com
    // Password: 352155289

    // Instadose PayPal Database Record
    // Address: https://api-3t.sandbox.paypal.com/nvp
    // UserName: ddelap_1338324886_biz_api1.mirion.com
    // Password: JYX64U8ZTL7G3DAS

    private int orderID = 0;
    private int accountID = 0;
    private InsDataContext idc;
    private decimal orderTotal = 0;
    private string totalUSD;
    string UserName = "PORTAL";
    
    public Finance_ProcessCreditCard()
    {
        idc = new InsDataContext();
    }

    protected void Page_Unload(object sender, EventArgs e)
    {
        try
        {
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
        catch { }
    }  

    protected void Page_Load(object sender, EventArgs e)
    {
        // Try to set the username
        try
        {
            this.UserName = User.Identity.Name.Split('\\')[1];

        }
        catch { this.UserName = "PORTAL"; }

        // Do nothing below if a post back has occurred.
        if (IsPostBack) return;

        divSearchToolbar.Visible = true;
        pnlPaymentProcessing.Visible = false;

        fillPaymentHistoryGridView();

        // Bind the year drop down list.
        for (int i = 0; i <= 20; i++)
        {
            int year = DateTime.Today.Year + i;
            ddlExpirationYear.Items.Add(new ListItem(year.ToString()));
        }

        // If the Order ID was passed, and is a valid number...
        // Treat as the number was entered and btnGo was pressed.
        if (Request.QueryString["OrderID"] != null &&
            int.TryParse(Request.QueryString["OrderID"], out orderID))
        {
            txtOrderID.Text = orderID.ToString();
            btnGo_Click(null, null);
            int accountID = getAccountID(orderID);
            bindCreditCards(accountID);
        }

        // If Enter key is pressed, OnClick event should trigger on txtOrderTotal.
        if (!Page.IsPostBack)
        {
            hdnfldAccordionIndex.Value = "0";
            txtOrderTotal.Attributes.Add("onKeyPress", "doClick('" + btnProcess.ClientID + "', event)");
        }
    }

    // Get AccountID on PostBack.
    private int getAccountID(int orderid)
    {
        int accountid = (from o in idc.Orders
                         where o.OrderID == orderid
                         select o.AccountID).FirstOrDefault();

        return accountid;
    }

    private void resetModalControlsToDefault()
    {
        InvisibleErrorsModal();
        txtNameOnCard.Text = "";
        txtCreditCardNumber.Text = "";
        ddlExpirationMonth.SelectedIndex = -1;
        ddlExpirationYear.SelectedIndex = -1;
        txtSecurityCode.Text = "";
    }

    // Error Message(s) based on OrderID/Number entered.
    private void InvisibleErrors()
    {      
        this.errorMsg.InnerText = "";
        this.errors.Visible = false;
    }

    private void InvisibleErrorsModal()
    {
        this.modalDialogErrorMsg.InnerText = "";
        this.modalDialogErrors.Visible = false;
    }

    private void InvisibleSuccesses()
    {
        this.successMsg.InnerText = "";
        this.successes.Visible = false;
    }

    private void VisibleErrors(string error)
    {
        this.errorMsg.InnerText = error;
        this.errors.Visible = true;
    }

    private void VisibleErrorsModal(string error)
    {
        this.modalDialogErrorMsg.InnerText = error;
        this.modalDialogErrors.Visible = true;
    }

    private void VisibleSuccesses(string error)
    {
        this.successMsg.InnerText = error;
        this.successes.Visible = true;
    }

    // Enter and Search Order Number entered.
    protected void btnGo_Click(object sender, EventArgs e)
    {
        // Clear-out old Error Messages.
        InvisibleErrors();
        InvisibleSuccesses();

        // Is the number supplied a valid Order Number?
        if (!int.TryParse(txtOrderID.Text.Trim(), out orderID))
        {
            VisibleErrors("Order not found in database!");
            return;
        }

        // Query the Order.
        Order order = (from o in idc.Orders where o.OrderID == orderID select o).FirstOrDefault();

        // If the Order was not found, throw an error message.
        if (order == null)
        {
            VisibleErrors("Order not found in database!");
            return;
        }

        // Set the Order ID value & Account ID and Name values.
        orderID = order.OrderID;
        accountID = order.AccountID;
        string accountName = order.Account.AccountName;

        // Set Account ID to Hidden Field and Name (ID) to Label in the Modal/Dialog.
        hfAccountID.Value = accountID.ToString();
        lblAccountNameAndNumber.Text = string.Format("{0} ({1})", accountName, accountID);

        // Use the process order to find the order total.
        ProcessOrders po = new ProcessOrders();

        // Update the controls with the display data.
        lblAccountName.Text = string.Format("{0} ({1})", accountName, accountID);
        lblOrderID.Text = orderID.ToString();

        // Bind Credit Card Numbers to ddlCreditCardNumbers.
        bindCreditCards(accountID);
        txtOrderTotal.Text = po.GetOrderTotal(orderID, false).Value.ToString("#.00");
        
        lblCurrencyCode.Text = order.CurrencyCode;

        //format for foreign orders.
        if (order.CurrencyCode != "USD")
        {
                       
            //Make sure it is a valid order amount.
            if (!decimal.TryParse(txtOrderTotal.Text, out orderTotal)) return;

            totalUSD = Currencies.ConvertToCurrency(orderTotal, order.CurrencyCode).ToString("#.00");

            lblCurrencyCode.Text = lblCurrencyCode.Text + " (" + totalUSD + " USD)";
        }

        lblOrderDate.Text = order.OrderDate.ToString("MM/dd/yyyy");

        // Display the Order information and Payment History.
        pnlPaymentProcessing.Visible = true;
        fillPaymentHistoryGridView();
    }    

    //------------------------- BEGIN :: Modal/Dialog "Add [New] Credit Card Information" Functions here. -------------------------//

    protected void btnSave_Click(object sender, EventArgs e)
    {
        // Clear any old/existing error messages.
        InvisibleErrorsModal();

        // Get the OrderID to identify the account the credit card should belong to.
        Order order = (from o in idc.Orders where o.OrderID == orderID select o).FirstOrDefault();
        // Validate the provided data.
        int accountID = Convert.ToInt32(hfAccountID.Value);
        int expMonth = 0;
        int expYear = 0;
        string creditCardNumber = "";
        string nameOnCard = "";
        string securityCode = "";
        string encryptedNumber = "";

        // Convert the expiration month and year to integers.
        int.TryParse(ddlExpirationMonth.SelectedValue, out expMonth);
        int.TryParse(ddlExpirationYear.SelectedValue, out expYear);

        // Trim the textbox values and put them into local variables.
        creditCardNumber = txtCreditCardNumber.Text.Trim();
        nameOnCard = txtNameOnCard.Text.Trim();
        securityCode = txtSecurityCode.Text.Trim();

        // Ensure the credit card information is provided.
        if (nameOnCard == string.Empty || creditCardNumber == string.Empty || securityCode == string.Empty)
        {
            VisibleErrorsModal("Please provide all required information before continuing.");
            return;
        }

        // Validate the credit card number.
        Instadose.Security.CreditCardType? cardType = Instadose.Security.Validation.ValidateCreditCard(creditCardNumber);
        if (!cardType.HasValue)
        {
            VisibleErrorsModal("Credit card number is invalid. Please verify and try again.");
            return;
        }

        // Encrypt the valid number.
        encryptedNumber = Instadose.Security.TripleDES.Encrypt(creditCardNumber);

        // Convert the credit card type into a credit card database ID.
        int cardTypeID = 0;
        switch (cardType.Value)
        {
            case Instadose.Security.CreditCardType.AmericanExpress:
                cardTypeID = 4;
                break;
            case Instadose.Security.CreditCardType.Discover:
                cardTypeID = 3;
                break;
            case Instadose.Security.CreditCardType.MasterCard:
                cardTypeID = 2;
                break;
            case Instadose.Security.CreditCardType.Visa:
                cardTypeID = 1;
                break;
        }

        // Error if the card type wasn't set
        if (cardTypeID == 0)
        {
            VisibleErrorsModal("Credit card type was not understood.");
            return;
        }

        // Look for existing credit cards
        AccountCreditCard card = (from acc in idc.AccountCreditCards where acc.NumberEncrypted == encryptedNumber select acc).FirstOrDefault();

        // If the card doesn't exist.
        if (card == null)
        {
            // Create the new card
            card = new AccountCreditCard()
            {
                AccountID = Convert.ToInt32(hfAccountID.Value),
                CreditCardTypeID = cardTypeID,
                NameOnCard = nameOnCard,
                NumberEncrypted = encryptedNumber,
                SecurityCode = securityCode,
                ExpMonth = expMonth,
                ExpYear = expYear,
                Active = true
            };

            idc.AccountCreditCards.InsertOnSubmit(card);
            idc.SubmitChanges();
        }
        else
        {
            card.AccountID = accountID;
            card.CreditCardTypeID = cardTypeID;
            card.NameOnCard = nameOnCard;
            card.NumberEncrypted = encryptedNumber;
            card.SecurityCode = securityCode;
            card.ExpMonth = expMonth;
            card.ExpYear = expYear;
            card.Active = true;

            // Save changes.
            idc.SubmitChanges();
        }

        // Close modal dialog.
        ScriptManager.RegisterClientScriptBlock(this, typeof(Page), UniqueID, "closeDialog('divNewCreditCardInformation')", true);

        // Reset modal controls (to defaults).
        resetModalControlsToDefault();

        // Rebind the credit card table.
        bindCreditCards(accountID, card.AccountCreditCardID);
    }

    protected void btnCancel_Click(object sender, EventArgs e)
    {
        resetModalControlsToDefault();
    }
    
    //------------------------- END :: Modal/Dialog "Add [New] Credit Card Information" Functions here. -------------------------//

    //------------------------- BEGIN :: Process Payment Information Functions here. -------------------------//
   
    protected void btnProcess_Click(object sender, EventArgs e)
    {
        // Clear any existing error messages.
        InvisibleErrors();
        InvisibleSuccesses();

        // Retrive the select account credit card.
        int accountCreditCardID = 0;
        if (!int.TryParse(ddlCreditCardNumbers.SelectedValue, out accountCreditCardID))
        {
            VisibleErrors("Please select a credit card to process the order.");
            return;
        }

        // Get the payment amount.
        decimal paymentAmount = 0;
        if (!decimal.TryParse(txtOrderTotal.Text, out paymentAmount))
        {
            VisibleErrors("Payment amount was not in the correct format.");
            return;
        }

        // Round the payment amount to 2 places.
        paymentAmount = Math.Round(paymentAmount, 2);

        orderID = Convert.ToInt32(lblOrderID.Text);

        // Get the order to display the results.
        Order order = (from o in idc.Orders where o.OrderID == orderID select o).FirstOrDefault();

        // Use the order process class to get the credit card details
        ProcessOrders po = new ProcessOrders();

        // Build the credit card package details.
        CreditCardDetailsType ccDetails = po.GetCreditCard(accountCreditCardID);

        // Send the data to PayPal.
        PayPalAPI payPal = new PayPalAPI();

        // Get the paypal Environment.
        string payPalEnvironment = Basics.GetSetting("PayPalEnvironment");
        
        //11/9/12 WK  taken out due to PayPal upgrades.
        //payPal.Profile = Instadose.Integration.PayPal.Communications.GetProfile(PayPalEnvironment);

        // Attempt to perform a direct payment transaction using PayPal
        DoDirectPaymentResponseType resp = null;

        CurrencyCodeType currencyCode = CurrencyCodeType.USD;

        try
        {
            currencyCode = (CurrencyCodeType)Enum.Parse(typeof(CurrencyCodeType), order.CurrencyCode);
        }
        catch
        {
            // Display the error message.
            VisibleErrors("The currency code provided in the order was not understood by PayPal.");

            // Exit
            return;
        }

        try
        {
            //11/9/12 WK - Paypal upgrades.
            // Call the processor 
            resp = PayPalAPI.DoDirectPayment(paymentAmount.ToString(), currencyCode, ccDetails, PaymentActionCodeType.SALE);
                       
        }
        catch (Exception ex)
        {
            // Record a credit card process failure on PayPal.
            Basics.WriteLogEntry("PayPal: " + ex.Message, Page.User.Identity.Name,
                "Instadose.Portal.Finance.ProcessCreditCard.btnProcess_Click", Basics.MessageLogType.Critical);

            // Display an error
            VisibleErrors("The payment could not be processed at this time. " +
                "Please try again in a few minutes.  If this issue continues, please contact IT.");

            // Exit the method.
            return;
        }

        // Determine if the call to paypal was successful to record the information
        if (resp.Ack == AckCodeType.SUCCESS || resp.Ack == AckCodeType.SUCCESSWITHWARNING)
        {
            // Record payment header data.
            Payment newPayment = new Payment()
            {
                OrderID = orderID,
                PaymentDate = DateTime.Now,
                Amount = paymentAmount,
                GatewayResponse = String.Empty,
                TransCode = resp.TransactionID,
                Captured = true,
                Authorized = true,
                PaymentMethodID = 1,
                AccountCreditCardID = accountCreditCardID,
                CreatedBy = UserName,
                ApplicationID = 2,
                CurrencyCode = order.CurrencyCode
            };

            // Insert the payment on submit.
            idc.Payments.InsertOnSubmit(newPayment);
            idc.SubmitChanges();

            // Get new PaymentID from Inserted record.
            int paymentid = newPayment.PaymentID;

            int accountid = getAccountID(orderID);

            // 12/09/2014, TDo
            // First insert placeholder Document record with default DocumentContent into database.
            // then PaymentReceipt report will fulfill this doc record with missing Document fields.
            // This work-around solution adapts for Orbital CC charge receipt.            

            InsertDocument(paymentid, accountid, orderID);                   

            fillPaymentHistoryGridView();                        

            lblReviewAccountNo.Text = accountid.ToString();
            if (order.InvoiceNo == null) { lblReviewInvoiceNo.Text = "-"; }
            else { lblReviewInvoiceNo.Text = order.InvoiceNo; }
            lblReviewOrderNo.Text = orderID.ToString();

            //**********************************************************
            //11/2012 WK - format for foreign orders.
            //**********************************************************
            //lblReviewTotal.Text = string.Format("{0:#.00} {1}", paymentAmount, order.CurrencyCode);

            lblReviewTotal.Text = Currencies.CurrencyToString(paymentAmount, order.CurrencyCode);
            
            // Display Successful message and transaction information, hide Order Process form.
            VisibleSuccesses("Payment has been processed successfully!");
            divSearchToolbar.Visible = false;
            pnlPaymentProcessing.Visible = false;

            string url = string.Format("PaymentReceipt.aspx?ID={0}", paymentid);
            ScriptManager.RegisterStartupScript(this, typeof(string), "OPEN_WINDOW", "window.open( '" + url + "');", true);
        }
        else
        {
            // Errors occurred during the transaction to paypal. Build the error message
            StringBuilder sb = new StringBuilder();
            sb.Append("PayPal had one or more errors while processing the payment: ");

            //11/9/2012 WK - Paypal API ugrade.
            //foreach (com.paypal.soap.api.ErrorType error in resp.Errors)
            //{
            //    sb.AppendFormat("{0}: {1}", error.ErrorCode, error.LongMessage);
            //}

            // Add to the error messages
            foreach (PayPal.PayPalAPIInterfaceService.Model.ErrorType error in resp.Errors)
                sb.AppendFormat("{0}: {1}", error.ErrorCode, error.LongMessage); 

            // Display the error message.
            VisibleErrors(sb.ToString());
        }
    }

    private void InsertDocument(int pPaymentid, int pAccountid, int pOrderID)
    {
        // Get UserName (based on System Login).
        string userName = "Unknown";

        if (Page.User.Identity.Name.Split('\\')[1] != null)
            userName = Page.User.Identity.Name.Split('\\')[1];

        Document dcmntrcrd = new Document()
        {
            DocumentGUID = Guid.NewGuid(),
            FileName = "PaymentReceipt_" + pPaymentid.ToString() + ".pdf",
            DocumentContent = Enumerable.Repeat((byte)0x20, 7).ToArray(),
            MIMEType = "application/pdf",
            DocumentCategory = "Payment Receipt",
            Description = "Credit Card Payment Receipt",
            AccountID = pAccountid,
            OrderID = pOrderID,
            CreatedBy = userName,
            CreatedDate = DateTime.Now,
            Active = true,
            ReferenceKey = pPaymentid
        };

        // Submit record to database.
        idc.Documents.InsertOnSubmit(dcmntrcrd);
        idc.SubmitChanges();    
    }

    //------------------------- END :: Process Payment Information Functions here. -------------------------//
    protected void btnProcessAnotherOrder_Click(object sender, EventArgs e)
    {
        Response.Redirect("ProcessCreditCard.aspx");
    }

    // Bind Credit Card Information to ddlCreditCardNumbers.
    private void bindCreditCards(int accountID, int accountCreditCardID = 0)
    {
        ddlCreditCardNumbers.Items.Clear();
        ddlCreditCardNumbers.Items.Add(new ListItem("-Select Credit Card-", "0"));

        // Query the credit cards for this account.
        var myCreditCards = (from acc in idc.AccountCreditCards
                             join cct in idc.CreditCardTypes on acc.CreditCardTypeID equals cct.CreditCardTypeID
                             where acc.AccountID == accountID && acc.Active == true
                             && (acc.UseOnce == false || acc.UseOnce == (bool?)null)
                             && acc.NumberEncrypted != ""
                             orderby acc.AccountCreditCardID descending
                             select new { 
                                AccountCCID = acc.AccountCreditCardID,
                                MaskedCCNumber = String.Format("{0} {1}", Common.MaskCreditCardNumber(acc.NumberEncrypted, cct.CreditCardName), formatExpirationDate(acc.ExpMonth, acc.ExpYear))
                             });

        this.ddlCreditCardNumbers.DataSource = myCreditCards;
        this.ddlCreditCardNumbers.DataTextField = "MaskedCCNumber";
        this.ddlCreditCardNumbers.DataValueField = "AccountCCID";
        this.ddlCreditCardNumbers.DataBind();
        this.ddlCreditCardNumbers.Focus();

        if (accountCreditCardID > 0)
        {
            // Select the newly added credit card.
            foreach (ListItem item in ddlCreditCardNumbers.Items)
                item.Selected = (item.Value == accountCreditCardID.ToString());
        }
    }

    // Format Expiration Date (given Expiration Month and Year).
    private string formatExpirationDate(int expmonth, int expyear)
    {
        string expirationDate = "";

        expirationDate = String.Format(" (Exp: {0}/{1})", Convert.ToString(expmonth), Convert.ToString(expyear));

        return expirationDate;
    }

    // Payment History GridView DataSource.
    public void fillPaymentHistoryGridView()
    {
        InsDataContext idc = new InsDataContext();

        var payments = (from p in idc.Payments join d in idc.Documents.DefaultIfEmpty() on p.PaymentID equals d.ReferenceKey
                        where p.OrderID == orderID && p.Captured && d.DocumentCategory == "Payment Receipt"
                        orderby p.PaymentDate descending
                        select new
                        {
                            DateOfPayment = p.PaymentDate,
                            d.DocumentGUID,
                            TotalAmount = p.Amount,
                            CreditCardNumber = Common.MaskCreditCardNumber(
                                                    p.AccountCreditCardID.HasValue ? p.AccountCreditCard.NumberEncrypted : "",
                                                    p.AccountCreditCardID.HasValue ? p.AccountCreditCard.CreditCardType.CreditCardName : ""
                                                ),
                            TypeOfCreditCard = p.AccountCreditCardID.HasValue ? p.AccountCreditCard.CreditCardType.CreditCardName : "",
                            d.ReferenceKey,
                            p.OrderID,
                            p.Order.AccountID,
                            p.Order.Account.CompanyName,
                            p.PaymentID
                        }).ToList();

        this.gvPaymentHistory.DataSource = payments;
        this.gvPaymentHistory.DataBind();
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
        gvPaymentHistory.PageIndex = 0;
    }

    protected void gvPaymentHistory_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvPaymentHistory.PageIndex = e.NewPageIndex;
        fillPaymentHistoryGridView();
    }

    // ****************  START PRINT RECEIPT FOR AN INVOICE IMPLEMENTATION ***********************//

    // INPUT INVOICE THROUGH textBox txtInvNo
    //protected void btnPrint_Click(object sender, EventArgs e)
    //{
    //    try
    //    {
    //        // Clear-out old Error Messages.
    //        InvisibleErrors();
    //        InvisibleSuccesses();

    //        // Is the number supplied a valid Order Number?
    //        if (txtInvNo.Text.Trim().Length == 0)
    //        {
    //            VisibleErrors("Please enter an invoice!");
    //            return;
    //        }

    //        DataTable paymentDT = new DataTable();
    //        int myAccountID = 0;
    //        int myPaymentID = 0;
    //        int myOrderID = 0;
    //        int myAccountCreditCardID = 0;
    //        string myInvoice = txtInvNo.Text.Trim();

    //        if (!GetPaymentInfo(myInvoice, ref paymentDT))
    //        {
    //            VisibleErrors("There is no credit card payment or missing card payment info for this invoice!");
    //            return;
    //        }

    //        foreach (DataRow row in paymentDT.Rows)
    //        {
    //            myAccountID = int.Parse(row["AccountID"].ToString().Trim());
    //            myPaymentID = int.Parse(row["PaymentID"].ToString().Trim());
    //            myOrderID = int.Parse(row["OrderID"].ToString().Trim());
    //            myAccountCreditCardID = int.Parse(row["AccountCreditCardID"].ToString().Trim());

    //            DisplayReceipt(myInvoice, myPaymentID);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        VisibleErrors(ex.ToString());
    //    }
    //}

    //private bool GetPaymentInfo(string pInvoiceNo, ref DataTable pDT)
    //{
    //    try
    //    {
    //        String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
    //        String cmdStr = "dbo.sp_GetPaymentInfoByInvoiceNo";

    //        SqlConnection sqlConn = new SqlConnection(connStr);
    //        SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

    //        sqlConn.Open();

    //        sqlCmd.CommandType = CommandType.StoredProcedure;

    //        sqlCmd.Parameters.Add("@InvoiceNo", SqlDbType.NVarChar, 20);
    //        sqlCmd.Parameters["@InvoiceNo"].Value = pInvoiceNo;

    //        SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
    //        DataTable dt = new DataTable();

    //        sqlDA.Fill(dt);

    //        sqlConn.Close();

    //        if (dt != null && dt.Rows.Count > 0)
    //        {
    //            int pAccountID = 0;
    //            int pPaymentID = 0;
    //            int pOrderID = 0;
    //            int pAccountCreditCardID;

    //            foreach (DataRow row in dt.Rows)
    //            {
    //                if (!int.TryParse(row["AccountID"].ToString().Trim(), out pAccountID))
    //                {
    //                    return false;
    //                }
    //                if (!int.TryParse(row["PaymentID"].ToString().Trim(), out pPaymentID))
    //                {
    //                    return false;
    //                }
    //                if (!int.TryParse(row["OrderID"].ToString().Trim(), out pOrderID))
    //                {
    //                    return false;
    //                }
    //                if (!int.TryParse(row["AccountCreditCardID"].ToString().Trim(), out pAccountCreditCardID))
    //                {
    //                    return false;
    //                }
    //            }

    //            pDT = dt;
    //            return true;
    //        }
    //        else
    //            return false;
    //    }
    //    catch
    //    {
    //        return false;
    //    }
    //}

    //private void DisplayReceipt(string pInvoice, int pPaymentID)
    //{
    //    Payment pmt = (from p in idc.Payments where p.PaymentID == pPaymentID select p).FirstOrDefault();

    //    // If the Payment was found, update InvoiceNo.
    //    if (pmt != null)
    //    {
    //        pmt.InvoiceNo = pInvoice;
    //        // Save changes.
    //        idc.SubmitChanges();

    //        GenerateAndDisplayReceipt(pPaymentID);
    //    }
    //    else
    //    {
    //        throw new Exception("Payment#: " + pPaymentID + " is not found!");
    //    }
    //}

    //private void GenerateAndDisplayReceipt(int pPaymentID)
    //{
    //    // Get AccountID based on PaymentID (Query String value).
    //    var payment = (from p in idc.Payments
    //                   where p.PaymentID == pPaymentID
    //                   select new
    //                   {
    //                       AccountID = p.Order.AccountID,
    //                       OrderID = p.OrderID,
    //                       CompanyName = p.Order.Account.CompanyName,
    //                       EncCreditCardNum = p.AccountCreditCard.NumberEncrypted,
    //                       CardType = p.AccountCreditCard.CreditCardType.CreditCardName
    //                   }).FirstOrDefault();

    //    if (payment.EncCreditCardNum == null || payment.CardType == null)
    //        throw new Exception("Payment is not a credit card payment type.");

    //    // Mask the credit card number.
    //    string maskedCardNum = Common.MaskCreditCardNumber(payment.EncCreditCardNum, payment.CardType);

    //    if (generatePDFDocument(pPaymentID, maskedCardNum, payment.AccountID, (payment.OrderID.HasValue) ? payment.OrderID.Value : 0, payment.CompanyName))
    //    {
    //        // Look for existing Document.
    //        Document doc = (from d in idc.Documents where d.ReferenceKey == pPaymentID select d).FirstOrDefault();

    //        // Open ViewDocument to display receipt
    //        string url = string.Format("/Finance/DisplayDocuments.aspx?GUID={0}", doc.DocumentGUID);
    //        ScriptManager.RegisterStartupScript(this, typeof(string), DateTime.Now.Ticks.ToString(), "window.open( '" + url + "');", true);


    //        //string url = string.Format("PaymentReceipt.aspx?ID={0}", pPaymentID);
    //        //ScriptManager.RegisterStartupScript(this, typeof(string), "OPEN_WINDOW", "window.open( '" + url + "');", true);                   
    //    }
    //}

    //private bool generatePDFDocument(int paymentID, string maskedCreditCardNumber, int accountID, int orderID, string companyName)
    //{
    //    string pdfFileName = "PaymentReceipt_" + paymentID.ToString() + ".pdf";
    //    string myCRFileNamePath = Server.MapPath("PaymentReceipt.rpt");
    //    MemoryStream oStream = new MemoryStream();
    //    ReportDocument cryRpt = new ReportDocument();
    //    bool success = false;

    //    // Get UserName (based on System Login).
    //    string userName = "Unknown";

    //    if (Page.User.Identity.Name.Split('\\')[1] != null)
    //        userName = Page.User.Identity.Name.Split('\\')[1];

    //    try
    //    {
    //        // First insert Document record with default DocumentContent into database.
    //        // then PaymentReceipt report will use this doc record to generate DocumentContent.
    //        // This work-around solution is adapting for both Instadose and Unix CC processing.

    //        Document dcmntrcrd = new Document()
    //        {
    //            DocumentGUID = Guid.NewGuid(),
    //            FileName = pdfFileName,
    //            DocumentContent = Enumerable.Repeat((byte)0x20, 7).ToArray(),
    //            MIMEType = "application/pdf",
    //            DocumentCategory = "Payment Receipt",
    //            Description = "Credit Card Payment Receipt",
    //            AccountID = accountID,
    //            OrderID = orderID,
    //            CreatedBy = userName,
    //            CreatedDate = DateTime.Now,
    //            Active = true,
    //            ReferenceKey = paymentID,
    //            CompanyName = companyName,
    //            GDSAccount = null
    //        };

    //        // Submit record to database.
    //        idc.Documents.InsertOnSubmit(dcmntrcrd);
    //        idc.SubmitChanges();

    //        // Get new PaymentID from Inserted record and generate payment receipt
    //        int docID = dcmntrcrd.DocumentID;
    //        if (docID <= 0) return false;

    //        cryRpt.Load(myCRFileNamePath);

    //        cryRpt.SetParameterValue("@pPaymentID", paymentID);
    //        cryRpt.SetParameterValue("@pMaskedCreditCardNumber", maskedCreditCardNumber);

    //        string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString.ToString();
    //        string myDatabase = Regex.Match(connStr, "(Initial Catalog=|Database=){1}([A-Za-z0-9_]+)[;]?").Groups[2].ToString();
    //        string myServer = Regex.Match(connStr, "(Data Source=){1}([\\w\\d\\.\\$\\-]+)[;]?").Groups[2].ToString();
    //        string myUserID = Regex.Match(connStr, "(User ID=|UID=){1}([\\w\\d\\.]+)[;]?").Groups[2].ToString();
    //        string myPassword = Regex.Match(connStr, "(Pwd=|Password=|PWD=){1}([\\w\\d\\.\\$]+)[;]?").Groups[2].ToString();

    //        cryRpt.DataSourceConnections[0].SetConnection(myServer, myDatabase, myUserID, myPassword);

    //        // Export to Memory Stream.        
    //        oStream = (MemoryStream)cryRpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);

    //        byte[] streamArray = oStream.ToArray();
    //        streamArray = oStream.ToArray();

    //        // Update inserted doc with real DocumentContent
    //        // Look for existing Document.
    //        Document doc = (from d in idc.Documents where d.DocumentID == docID select d).FirstOrDefault();

    //        if (doc != null)
    //        {
    //            doc.DocumentContent = streamArray;
    //            // Save changes.
    //            idc.SubmitChanges();

    //            success = true;

    //            // Report the error to the message system.
    //            Basics.WriteLogEntry("PaymentReceipt was generated successful.", Page.User.Identity.Name, "Portal.InstaDose_Finance_PaymentReceipt", Basics.MessageLogType.Information);
    //        }
    //        else
    //        {
    //            success = false;

    //            // Report the error to the message system.
    //            Basics.WriteLogEntry("PaymentReceipt was not generated successful.", Page.User.Identity.Name, "Portal.InstaDose_Finance_PaymentReceipt", Basics.MessageLogType.Critical);
    //        }
    //    }
    //    catch (Exception ex)
    //    {
    //        // Report the error to the message system.
    //        Basics.WriteLogEntry(ex.Message, Page.User.Identity.Name, "Portal.InstaDose_Finance_PaymentReceipt", Basics.MessageLogType.Critical);
    //        success = false;

    //        VisibleErrors(ex.Message);
    //    }
    //    finally
    //    {
    //        cryRpt.Close();
    //        cryRpt.Dispose();

    //        oStream.Flush();
    //        oStream.Close();
    //        oStream.Dispose();
    //    }

    //    return success;
    //}

    // **************** END PRINT RECEIPT FOR AN INVOICE IMPLEMENTATION ***********************//
}