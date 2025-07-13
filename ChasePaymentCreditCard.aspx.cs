using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Text;
using System.Configuration;

using Instadose;
using Instadose.Data;
using Instadose.Processing;

using Instadose.Integration.CreditCard;
using PayPal.PayPalAPIInterfaceService.Model;

//using com.paypal.soap.api;


public partial class Finance_ChaseProcessCreditCard : System.Web.UI.Page
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

	public Finance_ChaseProcessCreditCard()
		{
		idc = new InsDataContext();
		}

	protected void Page_Unload(object sender , EventArgs e)
		{
		try
			{
			GC.WaitForPendingFinalizers();
			GC.Collect();
			}
		catch { }
		}

	protected void Page_Load(object sender , EventArgs e)
		{
		// Do nothing below if a post back has occurred.
		if(IsPostBack)
			return;

		divSearchToolbar.Visible = true;
		pnlPaymentProcessing.Visible = false;

		fillPaymentHistoryGridView();

		// Bind the year drop down list.
		for(int i = 0 ; i <= 20 ; i++)
			{
			int year = DateTime.Today.Year + i;
			ddlExpirationYear.Items.Add(new ListItem(year.ToString()));
			}

		// If the Order ID was passed, and is a valid number...
		// Treat as the number was entered and btnGo was pressed.
		if(Request.QueryString["OrderID"] != null &&
				int.TryParse(Request.QueryString["OrderID"] , out orderID))
			{
			txtOrderID.Text = orderID.ToString();
			btnGo_Click(null , null);
			int accountID = getAccountID(orderID);
			bindCreditCards(accountID);
			}

		// If Enter key is pressed, OnClick event should trigger on txtOrderTotal.
		if(!Page.IsPostBack)
			{
			hdnfldAccordionIndex.Value = "0";
			txtOrderTotal.Attributes.Add("onKeyPress" , "doClick('" + btnProcess.ClientID + "', event)");
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
	protected void btnGo_Click(object sender , EventArgs e)
		{
		// Clear-out old Error Messages.
		InvisibleErrors();
		InvisibleSuccesses();

		// Is the number supplied a valid Order Number?
		if(!int.TryParse(txtOrderID.Text.Trim() , out orderID))
			{
			VisibleErrors("Order not found in database!");
			return;
			}

		// Query the Order.
		Order order = (from o in idc.Orders where o.OrderID == orderID select o).FirstOrDefault();

		// If the Order was not found, throw an error message.
		if(order == null)
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
		lblAccountNameAndNumber.Text = string.Format("{0} ({1})" , accountName , accountID);

		// Use the process order to find the order total.
		ProcessOrders po = new ProcessOrders();

		// Update the controls with the display data.
		lblAccountName.Text = string.Format("{0} ({1})" , accountName , accountID);
		lblOrderID.Text = orderID.ToString();

		// Bind Credit Card Numbers to ddlCreditCardNumbers.
		bindCreditCards(accountID);
		txtOrderTotal.Text = po.GetOrderTotal(orderID , false).Value.ToString("#.00");

		lblCurrencyCode.Text = order.CurrencyCode;

		//format for foreign orders.
		if(order.CurrencyCode != "USD")
			{

			//Make sure it is a valid order amount.
			if(!decimal.TryParse(txtOrderTotal.Text , out orderTotal))
				return;

			totalUSD = Currencies.ConvertToCurrency(orderTotal , order.CurrencyCode).ToString("#.00");

			lblCurrencyCode.Text = lblCurrencyCode.Text + " (" + totalUSD + " USD)";
			}

		lblOrderDate.Text = order.OrderDate.ToString("MM/dd/yyyy");

		// Display the Order information and Payment History.
		pnlPaymentProcessing.Visible = true;
		fillPaymentHistoryGridView();
		}

	//------------------------- BEGIN :: Modal/Dialog "Add [New] Credit Card Information" Functions here. -------------------------//

	protected void btnSave_Click(object sender , EventArgs e)
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
		int.TryParse(ddlExpirationMonth.SelectedValue , out expMonth);
		int.TryParse(ddlExpirationYear.SelectedValue , out expYear);

		// Trim the textbox values and put them into local variables.
		creditCardNumber = txtCreditCardNumber.Text.Trim();
		nameOnCard = txtNameOnCard.Text.Trim();
		securityCode = txtSecurityCode.Text.Trim();

		// Ensure the credit card information is provided.
		if(nameOnCard == string.Empty || creditCardNumber == string.Empty || securityCode == string.Empty)
			{
			VisibleErrorsModal("Please provide all required information before continuing.");
			return;
			}

		// Validate the credit card number.
		Instadose.Security.CreditCardType? cardType = Instadose.Security.Validation.ValidateCreditCard(creditCardNumber);
		if(!cardType.HasValue)
			{
			VisibleErrorsModal("Credit card number is invalid. Please verify and try again.");
			return;
			}

		// Encrypt the valid number.
		encryptedNumber = Instadose.Security.TripleDES.Encrypt(creditCardNumber);

		// Convert the credit card type into a credit card database ID.
		int cardTypeID = 0;
		switch(cardType.Value)
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
		if(cardTypeID == 0)
			{
			VisibleErrorsModal("Credit card type was not understood.");
			return;
			}

		// Look for existing credit cards
		AccountCreditCard card = (from acc in idc.AccountCreditCards where acc.NumberEncrypted == encryptedNumber select acc).FirstOrDefault();

		// If the card doesn't exist.
		if(card == null)
			{
			// Create the new card
			card = new AccountCreditCard()
			{
				AccountID = Convert.ToInt32(hfAccountID.Value) ,
				CreditCardTypeID = cardTypeID ,
				NameOnCard = nameOnCard ,
				NumberEncrypted = encryptedNumber ,
				SecurityCode = securityCode ,
				ExpMonth = expMonth ,
				ExpYear = expYear ,
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
		ScriptManager.RegisterClientScriptBlock(this , typeof(Page) , UniqueID , "closeDialog('divNewCreditCardInformation')" , true);

		// Reset modal controls (to defaults).
		resetModalControlsToDefault();

		// Rebind the credit card table.
		bindCreditCards(accountID , card.AccountCreditCardID);
		}

	protected void btnCancel_Click(object sender , EventArgs e)
		{
		resetModalControlsToDefault();
		}

	//------------------------- END :: Modal/Dialog "Add [New] Credit Card Information" Functions here. -------------------------//

	//------------------------- BEGIN :: Process Payment Information Functions here. -------------------------//

	protected void btnProcess_Click(object sender , EventArgs e)
		{
		// Clear any existing error messages.
		InvisibleErrors();
		InvisibleSuccesses();

		// Retrive the select account credit card.
		int accountCreditCardID = 0;
		if(!int.TryParse(ddlCreditCardNumbers.SelectedValue , out accountCreditCardID))
			{
			VisibleErrors("Please select a credit card to process the order.");
			return;
			}

		// Get the payment amount.
		decimal paymentAmount = 0;
		if(!decimal.TryParse(txtOrderTotal.Text , out paymentAmount))
			{
			VisibleErrors("Payment amount was not in the correct format.");
			return;
			}

		// Round the payment amount to 2 places.
		paymentAmount = Math.Round(paymentAmount , 2);

		orderID = Convert.ToInt32(lblOrderID.Text);

		// Get the order to display the results.
		Order order = (from o in idc.Orders where o.OrderID == orderID select o).FirstOrDefault();

		// Use the order process class to get the credit card details
		ProcessOrders po = new ProcessOrders();

		// Build the credit card package details.
		CreditCardDetailsType ccDetails = po.GetCreditCard(accountCreditCardID);

		// Send the data to PayPal.
		//PayPalAPI payPal = new PayPalAPI();
		CreditCardAPI creditCard = new CreditCardAPI();

		// Get the paypal Environment.
		string payPalEnvironment = Basics.GetSetting("PayPalEnvironment");

		//11/9/12 WK  taken out due to PayPal upgrades.
		//payPal.Profile = Instadose.Integration.PayPal.Communications.GetProfile(PayPalEnvironment);

		// Attempt to perform a direct payment transaction using PayPal
		//DoDirectPaymentResponseType resp = null;
		

		CurrencyCodeType currencyCode = CurrencyCodeType.USD;

		try
			{
			currencyCode = (CurrencyCodeType)Enum.Parse(typeof(CurrencyCodeType) , order.CurrencyCode);
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

			creditCard.CreditCardNumber = "4559961111111118";//ccDetails.CreditCardNumber;
			creditCard.Amount = Convert.ToDecimal(100); //paymentAmount;
			creditCard.CVV = "444"; //ccDetails.CVV2;
			creditCard.ExpDate = "1214"; //ccDetails.ExpMonth.ToString() + ccDetails.ExpYear.ToString().Substring(2,2);			
			creditCard.CardHolderName = "Sam Ayers"; //ccDetails.CardOwner.Payer;
			creditCard.CardHolderAddress = "1 Northeastern Blvd"; //ccDetails.CardOwner.Address.Street1;
			creditCard.CardHolderCity = "Bedford"; //ccDetails.CardOwner.Address.CityName;
			creditCard.CardHolderState = "NH"; //ccDetails.CardOwner.Address.StateOrProvince;
			creditCard.CardHolderZip = "03109-1234"; //ccDetails.CardOwner.Address.PostalCode;


			creditCard.ProcessCard();
			bool Response = creditCard.Approved;
			string CCStatus = creditCard.ProcessStatus;
			
			 
			//resp = PayPalAPI.DoDirectPayment(paymentAmount.ToString() 
			//, currencyCode , ccDetails , PaymentActionCodeType.SALE);

			}
		catch(Exception ex)
			{
			// Record a credit card process failure on PayPal.
			Basics.WriteLogEntry("PayPal: " + ex.Message , Page.User.Identity.Name ,
					"Instadose.Portal.Finance.ProcessCreditCard.btnProcess_Click" , Basics.MessageLogType.Critical);

			// Display an error
			VisibleErrors("The payment could not be processed at this time. " +
					"Please try again in a few minutes.  If this issue continues, please contact IT.");

			// Exit the method.
			return;
			}

		// Determine if the call to paypal was successful to record the information
		if(creditCard.Approved)
			{
			// Record payment header data.
			Payment newPayment = new Payment()
			{
				OrderID = orderID ,
				PaymentDate = DateTime.Now ,
				Amount = paymentAmount ,
				GatewayResponse = String.Empty ,
				TransCode = creditCard.ResponseCode ,
				Captured = true ,
				Authorized = true ,
				PaymentMethodID = 1 ,
				AccountCreditCardID = accountCreditCardID

			};

			// Insert the payment on submit.
			idc.Payments.InsertOnSubmit(newPayment);
			idc.SubmitChanges();

			// Get new PaymentID from Inserted record.
			int paymentid = newPayment.PaymentID;

			//We are done with the payment recording and going to clean up
			//WM 9-17-2014
			newPayment = null;
			//idc.Dispose();

			fillPaymentHistoryGridView();

			int accountid = getAccountID(orderID);

			lblReviewAccountNo.Text = accountid.ToString();
			if(order.InvoiceNo == null) { lblReviewInvoiceNo.Text = "-"; }
			else { lblReviewInvoiceNo.Text = order.InvoiceNo; }
			lblReviewOrderNo.Text = orderID.ToString();

			//**********************************************************
			//11/2012 WK - format for foreign orders.
			//**********************************************************
			//lblReviewTotal.Text = string.Format("{0:#.00} {1}", paymentAmount, order.CurrencyCode);

			lblReviewTotal.Text = Currencies.CurrencyToString(paymentAmount , order.CurrencyCode);

			// Display Successful message and transaction information, hide Order Process form.
			VisibleSuccesses("Payment has been processed successfully!");
			divSearchToolbar.Visible = false;
			pnlPaymentProcessing.Visible = false;

			string url = string.Format("PaymentReceipt.aspx?ID={0}" , paymentid);
			ScriptManager.RegisterStartupScript(this , typeof(string) , "OPEN_WINDOW" , "window.open( '" + url + "');" , true);
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
			//foreach(PayPal.PayPalAPIInterfaceService.Model.ErrorType error in resp.Errors)
			//	sb.AppendFormat("{0}: {1}" , error.ErrorCode , error.LongMessage);

			// Display the error message.
			VisibleErrors(sb.ToString());
			}

		}
	//------------------------- END :: Process Payment Information Functions here. -------------------------//
	protected void btnProcessAnotherOrder_Click(object sender , EventArgs e)
		{
		Response.Redirect("ProcessCreditCard.aspx");
		}

	// Bind Credit Card Information to ddlCreditCardNumbers.
	private void bindCreditCards(int accountID , int accountCreditCardID = 0)
		{
		ddlCreditCardNumbers.Items.Clear();
		ddlCreditCardNumbers.Items.Add(new ListItem("-Select Credit Card-" , "0"));

		// Query the credit cards for this account.
		var myCreditCards = (from acc in idc.AccountCreditCards
												 join cct in idc.CreditCardTypes on acc.CreditCardTypeID equals cct.CreditCardTypeID
												 where acc.AccountID == accountID && acc.Active == true && acc.NumberEncrypted != ""
												 orderby acc.AccountCreditCardID descending
												 select new
												 {
													 AccountCCID = acc.AccountCreditCardID ,
													 MaskedCCNumber = String.Format("{0} {1}" , Common.MaskCreditCardNumber(acc.NumberEncrypted , cct.CreditCardName) , formatExpirationDate(acc.ExpMonth , acc.ExpYear))
												 });

		this.ddlCreditCardNumbers.DataSource = myCreditCards;
		this.ddlCreditCardNumbers.DataTextField = "MaskedCCNumber";
		this.ddlCreditCardNumbers.DataValueField = "AccountCCID";
		this.ddlCreditCardNumbers.DataBind();
		this.ddlCreditCardNumbers.Focus();

		if(accountCreditCardID > 0)
			{
			// Select the newly added credit card.
			foreach(ListItem item in ddlCreditCardNumbers.Items)
				item.Selected = (item.Value == accountCreditCardID.ToString());
			}
		}

	// Format Expiration Date (given Expiration Month and Year).
	private string formatExpirationDate(int expmonth , int expyear)
		{
		string expirationDate = "";

		expirationDate = String.Format(" (Exp: {0}/{1})" , Convert.ToString(expmonth) , Convert.ToString(expyear));

		return expirationDate;
		}

	// Payment History GridView DataSource.
	public void fillPaymentHistoryGridView()
		{
		InsDataContext idc = new InsDataContext();

		var payments = (from p in idc.Payments
										join d in idc.Documents.DefaultIfEmpty() on p.PaymentID equals d.ReferenceKey
										where p.OrderID == orderID && p.Captured && d.DocumentCategory == "Payment Receipt"
										orderby p.PaymentDate descending
										select new
										{
											DateOfPayment = p.PaymentDate ,
											d.DocumentGUID ,
											TotalAmount = p.Amount ,
											CreditCardNumber = Common.MaskCreditCardNumber(
																							p.AccountCreditCardID.HasValue ? p.AccountCreditCard.NumberEncrypted : "" ,
																							p.AccountCreditCardID.HasValue ? p.AccountCreditCard.CreditCardType.CreditCardName : ""
																					) ,
											TypeOfCreditCard = p.AccountCreditCardID.HasValue ? p.AccountCreditCard.CreditCardType.CreditCardName : "" ,
											d.ReferenceKey ,
											p.OrderID ,
											p.Order.AccountID ,
											p.Order.Account.CompanyName ,
											p.PaymentID
										}).ToList();

		this.gvPaymentHistory.DataSource = payments;
		this.gvPaymentHistory.DataBind();
		idc.Dispose();
		}

	protected void imgbtnPrintReceipt_Click(object sender , ImageClickEventArgs e)
		{
		ImageButton imgbtn = (ImageButton)sender;
		string imgbtnCommandName = imgbtn.CommandName.ToString();
		string selectedPaymentID = imgbtn.CommandArgument.ToString();

		if(imgbtnCommandName != "PrintReceipt")
			return;
		if(selectedPaymentID == null)
			return;

		string url = "PaymentReceipt.aspx?ID=" + selectedPaymentID;
		ScriptManager.RegisterStartupScript(this , typeof(string) , "OPEN_WINDOW" , "window.open( '" + url + "');" , true);
		}

	protected void gvPaymentHistory_Sorting(object sender , GridViewSortEventArgs e)
		{
		gvPaymentHistory.PageIndex = 0;
		}

	protected void gvPaymentHistory_PageIndexChanging(object sender , GridViewPageEventArgs e)
		{
		gvPaymentHistory.PageIndex = e.NewPageIndex;
		fillPaymentHistoryGridView();
		}

	public CurrencyCodeType currencyCode { get; set; }
	}