using CrystalDecisions.CrystalReports.Engine;
using Instadose;
using Instadose.Data;
using Instadose.Integration.OrbitalPaymentech;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
//using com.paypal.soap.api;



public partial class Finance_MockProcessCreditCard : System.Web.UI.Page
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
    
    public Finance_MockProcessCreditCard()
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
        //// Orbital CC process will be handled by ePay in AX. Redirect the page to finance default page
        //Response.Redirect("/Finance/Default.aspx");

        // Try to set the username
        try
        {
            this.UserName = User.Identity.Name.Split('\\')[1];

        }
        catch { this.UserName = "PORTAL"; }

        // Do nothing below if a post back has occurred.
        if (IsPostBack) return;
        
        pnlPaymentProcessing.Visible = false;

        fillPaymentHistoryGridView();

        // Bind the year drop down list.
        for (int i = 0; i <= 20; i++)
        {
            int year = DateTime.Today.Year + i;
            ddlExpirationYear.Items.Add(new ListItem(year.ToString()));
        }        

        // If Enter key is pressed, OnClick event should trigger on txtPaymentAmount.
        if (!Page.IsPostBack)
        {
            hdnfldMockInvoiceID.Value = "0";
            txtPaymentAmount.Attributes.Add("onKeyPress", "doClick('" + btnProcess.ClientID + "', event)");
        }
    }   

    // Error Message(s) based on OrderID/Number entered.
    private void InvisibleErrors()
    {      
        this.errorMsg.InnerHtml  = "";
        this.errors.Visible = false;
    }
   
    private void InvisibleSuccesses()
    {
        this.successMsg.InnerHtml = "";
        this.successes.Visible = false;
    }

    private void VisibleErrors(string error)
    {
        this.errorMsg.InnerHtml = error;
        this.errors.Visible = true;
    }   

    private void VisibleSuccesses(string error)
    {
        this.successMsg.InnerHtml = error;
        this.successes.Visible = true;
    }

    private DataTable GetMockInvoiceDetail(string pInvoice)
    {
        try
        {
            String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            String cmdStr = "sp_GetMockInvoiceInfo";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.Add("@Invoice", SqlDbType.NVarChar, 20);

            sqlCmd.Parameters["@Invoice"].Value = pInvoice;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);

            sqlConn.Close();

            return dt;
        }
        catch { return null; }
    }

    private DataTable GetMockInvoicePaymentHistory(string pInvoice)
    {
        try
        {
            String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            String cmdStr = "sp_MockInvoicePaymentHistory";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;
            sqlCmd.Parameters.Add("@pMockInvoiceNo", SqlDbType.NVarChar, 20);

            sqlCmd.Parameters["@pMockInvoiceNo"].Value = pInvoice;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);

            sqlConn.Close();

            return dt;
        }
        catch { return null; }
    }

    private int InsertMockPayment(int MockInvoiceID, DateTime PaymentDate, double Amount, string CurrencyCode, string TransCode, string PaymentMethod, string CreditCardNumberEncrypted, string CreditCardSecurityCode, int ExpMonth, int ExpYear, string CreditCardType, string NameOnCard, string CreatedBy)
    {
        try
        {
            String connStr = ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString;
            String cmdStr = "sp_InsertMockPayment";

            SqlConnection sqlConn = new SqlConnection(connStr);
            SqlCommand sqlCmd = new SqlCommand(cmdStr, sqlConn);

            sqlConn.Open();

            sqlCmd.CommandType = CommandType.StoredProcedure;

            sqlCmd.Parameters.Add("@MockInvoiceID", SqlDbType.Int);
            sqlCmd.Parameters["@MockInvoiceID"].Value = MockInvoiceID;

            sqlCmd.Parameters.Add("@PaymentDate", SqlDbType.DateTime);
            sqlCmd.Parameters["@PaymentDate"].Value = PaymentDate;

            sqlCmd.Parameters.Add("@Amount", SqlDbType.Money);
            sqlCmd.Parameters["@Amount"].Value = Convert.ToDecimal( Amount);

            sqlCmd.Parameters.Add("@CurrencyCode", SqlDbType.NVarChar, 3);
            sqlCmd.Parameters["@CurrencyCode"].Value = CurrencyCode;

            sqlCmd.Parameters.Add("@TransCode", SqlDbType.NVarChar, 50);
            sqlCmd.Parameters["@TransCode"].Value = TransCode;

            sqlCmd.Parameters.Add("@PaymentMethod", SqlDbType.NVarChar, 10);
            sqlCmd.Parameters["@PaymentMethod"].Value = PaymentMethod;

            sqlCmd.Parameters.Add("@CreditCardNumberEncrypted", SqlDbType.NVarChar, 150);
            sqlCmd.Parameters["@CreditCardNumberEncrypted"].Value = CreditCardNumberEncrypted;

            sqlCmd.Parameters.Add("@CreditCardSecurityCode", SqlDbType.NVarChar, 4);
            sqlCmd.Parameters["@CreditCardSecurityCode"].Value = CreditCardSecurityCode;

            sqlCmd.Parameters.Add("@ExpMonth", SqlDbType.Int);
            sqlCmd.Parameters["@ExpMonth"].Value = ExpMonth;

            sqlCmd.Parameters.Add("@ExpYear", SqlDbType.Int);
            sqlCmd.Parameters["@ExpYear"].Value = ExpYear;

            sqlCmd.Parameters.Add("@CreditCardType", SqlDbType.NVarChar, 20);
            sqlCmd.Parameters["@CreditCardType"].Value = CreditCardType;

            sqlCmd.Parameters.Add("@NameOnCard", SqlDbType.NVarChar, 65);
            sqlCmd.Parameters["@NameOnCard"].Value = CreditCardSecurityCode;

            sqlCmd.Parameters.Add("@CreatedBy", SqlDbType.NVarChar, 100);
            sqlCmd.Parameters["@CreatedBy"].Value = CreatedBy;

            SqlDataAdapter sqlDA = new SqlDataAdapter(sqlCmd);
            DataTable dt = new DataTable();

            sqlDA.Fill(dt);

            sqlConn.Close();

            if (dt == null || dt.Rows.Count < 1)
            {
                VisibleErrors("Error insert MockPayment record!");
                return 0;
            }
            else
            {
                if (int.Parse(dt.Rows[0][0].ToString()) == 0)
                {
                    VisibleErrors("Error insert MockPayment record!");
                    return 0;
                }
                else
                {
                    return int.Parse(dt.Rows[0][0].ToString());
                }
            }
            
        }
        catch (Exception ex) 
        {
            VisibleErrors(ex.ToString());
            return 0;         
        }
    }

    // Enter and Search Order Number entered.
    protected void btnGo_Click(object sender, EventArgs e)
    {
        // Clear-out old Error Messages.
        InvisibleErrors();
        InvisibleSuccesses();       

        if (string.IsNullOrEmpty(txtInvoiceID.Text.Trim()))
        {
            VisibleErrors("Please enter invoice number!");
            pnlPaymentProcessing.Visible = false;
            return;
        }

        DataTable invoiceDetail = GetMockInvoiceDetail(txtInvoiceID.Text.Trim());
        if (invoiceDetail == null || invoiceDetail.Rows.Count < 1)
        {
            VisibleErrors("Invoice not found in database!");
            pnlPaymentProcessing.Visible = false;
            return;
        }                       

        // Update the controls with the display data.
        lblCompanyName.Text = invoiceDetail.Rows[0]["CompanyName"].ToString();
        lblInvoiceID.Text = invoiceDetail.Rows[0]["MockInvoiceNo"].ToString();
        lblInvoiceAmount.Text = Convert.ToDecimal(invoiceDetail.Rows[0]["InvoiceAmt"]).ToString("#0.00");
        lblCurrencyCode.Text = invoiceDetail.Rows[0]["CurrencyCode"].ToString() ;
        lblCurrencyCode1.Text = invoiceDetail.Rows[0]["CurrencyCode"].ToString();
        lblInvoiceDate.Text = Convert.ToDateTime(invoiceDetail.Rows[0]["InvoiceDate"]).ToString("MM/dd/yyyy");
        txtPaymentAmount.Text = string.Format("{0:0.00}", 0);
        txtEmailAddr.Text = this.UserName + "@mirion.com";

        hdnfldMockInvoiceID.Value = invoiceDetail.Rows[0]["MockInvoiceID"].ToString(); ;

        // Display the Order information and Payment History.        
        fillPaymentHistoryGridView();

        pnlPaymentProcessing.Visible = true;

    }

    protected void btnProcess_Click(object sender, EventArgs e)
    {
        // Clear-out Error/Success Messages.
        InvisibleErrors();
        InvisibleSuccesses();

        if (PassPaymentValidation())
        {
            List<string> errorMsgList = new List<string>();
            List<string> successMsgList = new List<string>();

            // Orbital Transaction request
            OrbitalPaymenTechAPI creditCard = new OrbitalPaymenTechAPI();
            CreditCardTransactionInfo myTransaction = new CreditCardTransactionInfo();

            // Get  credit card type.
            Instadose.Security.CreditCardType? cardType = Instadose.Security.Validation.ValidateCreditCard(txtCreditCardNumber.Text.Trim());
            string creditCardType = "";
            switch (cardType.Value)
            {
                case Instadose.Security.CreditCardType.AmericanExpress:                    
                    creditCardType = "American Express";
                    break;
                case Instadose.Security.CreditCardType.Discover:                    
                    creditCardType = "Discover";
                    break;
                case Instadose.Security.CreditCardType.MasterCard:                    
                    creditCardType = "MasterCard";
                    break;
                case Instadose.Security.CreditCardType.Visa:                    
                    creditCardType = "Visa";
                    break;
            }

            string myOrbitalCurrencyCode = "";
            string myOrbitalCurrencyExponent = "";

            Currency myCurrency = (from c in idc.Currencies where c.CurrencyCode == lblCurrencyCode.Text select c).FirstOrDefault();
            if (myCurrency != null)
            {
                myOrbitalCurrencyCode = myCurrency.OrbitalCurrencyCode;
                myOrbitalCurrencyExponent = myCurrency.OrbitalExponentCode;
            }

            string myUniqueOrbitalOrderNumber = lblInvoiceID.Text;   // use invoice# as unique Orbital order#            
            
            myTransaction.CreditCardNumber = txtCreditCardNumber.Text.Trim();
            myTransaction.CreditCardSecurityCode = txtSecurityCode.Text.Trim();
            myTransaction.CreditCardExpireMonth = this.ddlExpirationMonth.SelectedValue.PadLeft(2, '0');
            myTransaction.CreditCardExpireYear = this.ddlExpirationYear.SelectedValue;
            myTransaction.CreditCardType = creditCardType;
            myTransaction.CardHolderName = txtNameOnCard.Text.Trim();                              

            myTransaction.Amount = Math.Round(Convert.ToDouble(txtPaymentAmount.Text.Trim()), 2);
            myTransaction.CurrencyCode = myOrbitalCurrencyCode;     // Bristish pound, Canadian, USD, Australian,...
            myTransaction.CurrencyExponent = myOrbitalCurrencyExponent; // 1 or 2

            myTransaction.AccountType = 1;    // 2: Instadose, 1: Unix          
            myTransaction.OrbitalOrderNumber = myUniqueOrbitalOrderNumber;
            
            creditCard.DoDirectPayment(myTransaction);

            bool success = myTransaction.Success;
            string errorMsg = myTransaction.ErrorMsg;
            string transCode = myTransaction.TransCode;

            if (success)
            {               
                // create receipt doc, generate sucess message with invoice no & transaction code
                successMsgList.Add("The transaction is approved for invoice: " + myUniqueOrbitalOrderNumber + ". Transaction Code: " + transCode);
                
                // Encrypt the Valid Credit Card Number.
                string encryptedCreditCardNumber = Instadose.Security.TripleDES.Encrypt(myTransaction.CreditCardNumber);

                // Record Mock payment 
                int mockPaymentID = InsertMockPayment(int.Parse(hdnfldMockInvoiceID.Value), DateTime.Now, myTransaction.Amount, lblCurrencyCode.Text
                    , transCode, "Credit Card", encryptedCreditCardNumber, myTransaction.CreditCardSecurityCode
                    , int.Parse(myTransaction.CreditCardExpireMonth), int.Parse(myTransaction.CreditCardExpireYear)
                    , myTransaction.CreditCardType, myTransaction.CardHolderName, this.UserName);

                if (mockPaymentID > 0)
                {
                    // Mask the credit card number.
                    string maskedCardNum = Common.MaskCreditCardNumber(encryptedCreditCardNumber, myTransaction.CreditCardType);

                    generatePDF(mockPaymentID, maskedCardNum, lblCompanyName.Text);

                    EmailTransactionSuccess(myUniqueOrbitalOrderNumber, DateTime.Now, myTransaction.Amount, lblCurrencyCode.Text, mockPaymentID, "Credit Card");
                }
                else
                {
                    successMsgList.Add("Please check IT. The payment record could not be saved!!!");
                }
                
                VisibleSuccesses(GenerateBulletMessage(successMsgList));
                // Reload history payment of an account
                // Display the Order information and Payment History.        
                fillPaymentHistoryGridView();
                resetControlsToDefault();                
            }
            else
            {
                // generate error message with invoice no & error message                    
                errorMsgList.Add("The transaction is declined for invoice: " + myUniqueOrbitalOrderNumber + ". Reason: " + errorMsg);
                VisibleErrors("Orbital Paymentech has incurred the following errors while processing the payment: <br />" + GenerateBulletMessage(errorMsgList));                
            }
                   
        }        
    }

    private bool generatePDF(int mockPaymentID, string maskedCreditCardNumber, string companyName)
    {
        string pdfFileName = "MockPaymentReceipt_" + mockPaymentID.ToString() + ".pdf";
        string myCRFileNamePath = Server.MapPath("MockPaymentReceipt.rpt");
        ReportDocument cryRpt = new ReportDocument();
        bool success = false;
        Stream oStream = null;
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
                DocumentCategory = "Mock Payment Receipt",
                Description = "Credit Card Payment Receipt",
                AccountID = (int?)null,
                OrderID = (int?)null,
                CreatedBy = this.UserName,
                CreatedDate = DateTime.Now,
                Active = true,
                ReferenceKey = mockPaymentID,
                CompanyName = companyName,
                GDSAccount = null
            };

            // Submit record to database.
            idc.Documents.InsertOnSubmit(dcmntrcrd);
            idc.SubmitChanges();

            // Get new PaymentID from Inserted record and generate payment receipt
            int docID = dcmntrcrd.DocumentID;
            if (docID <= 0) return false;

            cryRpt.Load(myCRFileNamePath);

            cryRpt.SetParameterValue("@pPaymentID", mockPaymentID);
            cryRpt.SetParameterValue("@pMaskedCreditCardNumber", maskedCreditCardNumber);

            string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString.ToString();
            string myDatabase = Regex.Match(connStr, "(Initial Catalog=|Database=){1}([A-Za-z0-9_]+)[;]?").Groups[2].ToString();
            string myServer = Regex.Match(connStr, "(Data Source=){1}([\\w\\d\\.\\$\\-]+)[;]?").Groups[2].ToString();
            string myUserID = Regex.Match(connStr, "(User ID=|UID=){1}([\\w\\d\\.]+)[;]?").Groups[2].ToString();
            string myPassword = Regex.Match(connStr, "(Pwd=|Password=|PWD=){1}([\\w\\d\\.\\$]+)[;]?").Groups[2].ToString();

            cryRpt.DataSourceConnections[0].SetConnection(myServer, myDatabase, myUserID, myPassword);

            // Export to Memory Stream.        
            oStream = cryRpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
            byte[] byteArray = new byte[oStream.Length];
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

    private bool EmailTransactionSuccess(string pInvoiceNo, DateTime pPaymentDate, double pAmount, string pCurrencyCode, int pPaymentID, string pPaymentMethod)
    {
        if (txtEmailAddr.Text.Trim().Length > 0)
        {
            bool RtnResponse = false;
            string toEmail = txtEmailAddr.Text.Trim();

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
            string amountStr = "$" + pAmount.ToString() + " " + pCurrencyCode;

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

    private bool PassPaymentValidation()
    {
        if (!ValidEmailAddressValidation(txtEmailAddr))
        {
            return false;
        }

        if (!ValidExpirationDate())
        {
            return false;
        }

        string creditCardType = "";

        if (!ValidCreditCardNumber(ref creditCardType))
        {
            return false;
        }

        if (!ValidCVV(creditCardType))
        {
            return false;
        }

        if (!ValidPaymentAmount())
        {
            return false;
        }

        return true;
    }

    private bool IsNumeric(String numberString)
    {
        Double d;
        return Double.TryParse(numberString, out d);
    }

    private bool ValidPaymentAmount()
    {
        // we make sure that the value in the payment about is a number.
        if (!IsNumeric(txtPaymentAmount.Text))
        {
            VisibleErrors("The payment amount is not valid.");            
            return false;
        }

        // Check if the amount to pay is less than the remaining Balance of the Invoice.
        decimal paymentAmount = 0;
        decimal remainingBalance = 0;
        paymentAmount = Convert.ToDecimal(txtPaymentAmount.Text);
        remainingBalance = Convert.ToDecimal(lblInvoiceAmount.Text);

        if (paymentAmount <= 0)
        {
            VisibleErrors("The payment must be greater than 0.");
            return false;
        }

        if (paymentAmount > remainingBalance)
        {
            VisibleErrors("The amount being paid exceed the remaining balance.");            
            return false;
        }
        return true;
    }

    private bool ValidEmailAddressValidation(TextBox pEmailTextBox)
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

    private bool ValidExpirationDate()
    {
        int expMonth = 0;
        int expYear = 0;
        // Convert the Expiration Month and Year to Integers.
        int.TryParse(this.ddlExpirationMonth.SelectedValue, out expMonth);
        int.TryParse(this.ddlExpirationYear.SelectedValue, out expYear);

        // Validate CC expiration.
        DateTime ccExpireDate = Convert.ToDateTime(expMonth.ToString() + "/01/" + expYear.ToString());
        DateTime curMonthYearDate = Convert.ToDateTime(DateTime.Today.Month.ToString() + "/01/" + DateTime.Today.Year.ToString());
        if (ccExpireDate.Ticks < curMonthYearDate.Ticks)
        {
            VisibleErrors("Expiration date is in the past. Please verify and try again.");
            return false;
        }
        return true;
    }

    private bool ValidCreditCardNumber(ref string creditCardType)
    {
        // Validate the Credit Card Number.
        Instadose.Security.CreditCardType? cardType = Instadose.Security.Validation.ValidateCreditCard(txtCreditCardNumber.Text.Trim());
        if (!cardType.HasValue)
        {
            VisibleErrors("Credit card number is invalid. Please verify and try again.");
            return false;
        }
        else
        {
            // Convert the Credit Card Type into a CreditCardTypeID in the database.
            int cardTypeID = 0;            
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
                VisibleErrors("Credit card type was not understood.");
                return false;
            }
        }

        return true;
    }

    private bool ValidCVV(string creditCardType)
    {
        // Check the CSV code
        
        if (creditCardType == "Discover" || creditCardType == "MasterCard" || creditCardType == "Visa")
        {
            if (this.txtSecurityCode.Text.Trim().Length != 3)
            {
                VisibleErrors("CVV/CVC must be 3 characters length.");
                return false;
            }
        }

        if (creditCardType == "American Express")
        {
            if (this.txtSecurityCode.Text.Trim().Length != 4)
            {
                VisibleErrors("CVV/CVC must be 4 characters length.");
                return false;
            }
        }
        return true;
    }

    private void resetControlsToDefault()
    {        
        txtNameOnCard.Text = "";
        txtCreditCardNumber.Text = "";
        ddlExpirationMonth.SelectedIndex = 0;
        ddlExpirationYear.SelectedIndex = 0;
        txtSecurityCode.Text = "";        
        txtPaymentAmount.Text = string.Format("{0:0.00}", 0);
        txtEmailAddr.Text = this.UserName + "@mirion.com";
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
        try
        {
            this.gvPaymentHistory.DataSource = null;
            this.gvPaymentHistory.DataBind();

            DataTable payHistory = GetMockInvoicePaymentHistory(txtInvoiceID.Text.Trim());
            if (payHistory == null || payHistory.Rows.Count < 1)
            {
                return;
            }
            else
            {
                foreach (DataRow  row in payHistory.Rows)
                {
                    row["CreditCardNumber"] = Common.MaskCreditCardNumber(row["EncryptedCreditCardNumber"].ToString(), row["TypeOfCreditCard"].ToString());
                }                

                this.gvPaymentHistory.DataSource = payHistory;
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
        gvPaymentHistory.PageIndex = 0;
    }

    protected void gvPaymentHistory_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gvPaymentHistory.PageIndex = e.NewPageIndex;
        fillPaymentHistoryGridView();
    }





    
}