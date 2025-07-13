using CrystalDecisions.CrystalReports.Engine;
using Instadose;
using Instadose.Data;
using Instadose.Integration.OrbitalPaymentech;
using Instadose.Security;
using Mirion.DSD.GDS.API.Requests;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace portal_instadose_com_v3.Services
{
    /// <summary>
    /// Summary description for Invoice
    /// </summary>
    [WebService(Namespace = "http://portal.instadose.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.Web.Script.Services.ScriptService]
    public class Invoice : System.Web.Services.WebService
    {
        [WebMethod]
        public void SayHello(string name, string callback)
        {
            Context.Response.Clear();
            Context.Response.ContentType = "application/json";
            Context.Response.Write(callback + "({'Hello':'" + name + "'})");
            Context.Response.End();
        }

        [WebMethod]
        public void Payment(string accountID, string companyName, string email, string ccNumber, string expMonth, string expYear, string cvv,
                                string holderName, int cardTypeID, string invoices, string currencyCode, string username, string callback)
        {
            var sb = new StringBuilder();
            try
            {
                List<string> errorMsgList = new List<string>();
                List<string> successMsgList = new List<string>();

                var cc = new CreditCardTransactionInfo()
                {
                    CreditCardNumber = ccNumber,
                    CreditCardExpireMonth = expMonth.PadLeft(2, '0'),
                    CreditCardExpireYear = expYear,
                    CreditCardSecurityCode = cvv,
                    CardHolderName = holderName,
                    CreditCardType = GetCardType(cardTypeID),
                    AccountType = 1
                };

                var successCount = 0;
                var accountCreditCardID = AddCreditCard(cc, accountID, cardTypeID, true);

                // Orbital Transaction request
                OrbitalPaymenTechAPI creditCard = new OrbitalPaymenTechAPI();
                var idc = new Instadose.Data.InsDataContext();
                Currency myCurrency = (from c in idc.Currencies where c.CurrencyCode == currencyCode select c).FirstOrDefault();
                var arrInvoices = invoices.Split(',');
                foreach (var i in arrInvoices)
                {
                    var keyValue = i.Split('=');
                    var invoice = keyValue[0].PadLeft(8,'0');
                    var amount = keyValue[1];
                    cc.Amount = Math.Round(Convert.ToDouble(amount), 2);
                    cc.OrbitalOrderNumber = invoice;
                    cc.CurrencyCode = myCurrency.OrbitalCurrencyCode;
                    cc.CurrencyExponent = myCurrency.OrbitalExponentCode;
                    creditCard.DoDirectPayment(cc);

                    var success = cc.Success;
                    var erroMessage = cc.ErrorMsg;
                    var transCode = cc.TransCode;
                    var paymentDate = DateTime.Now;
                    if (success)
                    {
                        successCount++;
                        successMsgList.Add("The transaction is approved for invoice: " + invoice + ". Transaction Code: " + transCode);
                        int paymentID = RecordPaymentHeader(cc, accountCreditCardID, paymentDate, username);

                        string encryptedCreditCardNumber = Instadose.Security.TripleDES.Encrypt(cc.CreditCardNumber);
                        string maskedCardNum = Common.MaskCreditCardNumber(encryptedCreditCardNumber, cc.CreditCardType);

                        GenerateReceipt(paymentID, maskedCardNum, accountID, companyName, username, cc.AccountType);

                        EmailTransactionSuccess(email, accountID, cc.OrbitalOrderNumber, paymentDate, cc.Amount, currencyCode, paymentID, "Credit Card", username);
                    }
                    else
                    {
                        errorMsgList.Add("The transaction is declined for invoice: " + invoice + ". Reason: " + erroMessage);
                    }

                }

                sb.Append("{'SuccessMessages':[");
                if (successMsgList.Count > 0)
                {
                    var first = true;
                    foreach (var m in successMsgList)
                    {
                        if (!first)
                            sb.Append(",");
                        sb.Append("'" + m + "'");
                        first = false;
                    }
                }
                sb.Append("],'ErrorMessages':[");
                if (errorMsgList.Count > 0)
                {
                    // there is not any transaction proceeded successfully then delete the inserted AccountCreditCard record for UseOnlyOnce CC process option
                    if (successCount == 0)
                    {
                        RemoveAccountCreditCard(accountCreditCardID);
                    }
                    var first = true;
                    foreach (var m in errorMsgList)
                    {
                        if (!first)
                            sb.Append(",");
                        sb.Append("'" + m + "'");
                        first = false;
                    }
                }
                sb.Append("]}");
            }
            catch (Exception ex)
            {
                sb.Append("{'ErrorMessage':'" + ex.Message.Replace(Environment.NewLine,"  ") + "'}");
            }
            
            Context.Response.Clear();
            Context.Response.ContentType = "application/json";
            Context.Response.Write(callback + "(" + sb.ToString() + ")");
            Context.Response.End();
        }

        [WebMethod]
        public void InsPayment(string accountID, string companyName, string email, string ccNumber, string expMonth, string expYear, string cvv,
                                string holderName, int cardTypeID, string invoices, string currencyCode, string username, bool useonce) 
        {
            var sb = new StringBuilder();
            try
            {
                List<string> errorMsgList = new List<string>();
                List<string> successMsgList = new List<string>();

                var cc = new CreditCardTransactionInfo()
                {
                    CreditCardNumber = ccNumber,
                    CreditCardExpireMonth = expMonth.PadLeft(2, '0'),
                    CreditCardExpireYear = expYear,
                    CreditCardSecurityCode = cvv,
                    CardHolderName = holderName,
                    CreditCardType = GetCardType(cardTypeID),
                    AccountType = 2
                };

                var successCount = 0;
                var accountCreditCardID = AddCreditCard(cc, accountID, cardTypeID, useonce);

                // Orbital Transaction request
                OrbitalPaymenTechAPI creditCard = new OrbitalPaymenTechAPI();
                var idc = new Instadose.Data.InsDataContext();
                Currency myCurrency = (from c in idc.Currencies where c.CurrencyCode == currencyCode select c).FirstOrDefault();
                var arrInvoices = invoices.Split(',');
                foreach (var i in arrInvoices)
                {
                    var keyValue = i.Split('=');
                    var invoice = keyValue[0].PadLeft(7, '0');
                    var amount = keyValue[1];
                    cc.Amount = Math.Round(Convert.ToDouble(amount), 2);
                    cc.OrbitalOrderNumber = invoice;
                    cc.CurrencyCode = myCurrency.OrbitalCurrencyCode;
                    cc.CurrencyExponent = myCurrency.OrbitalExponentCode;
                    creditCard.DoDirectPayment(cc);

                    var success = cc.Success;
                    var erroMessage = cc.ErrorMsg;
                    var transCode = cc.TransCode;
                    var paymentDate = DateTime.Now;
                    if (success)
                    {
                        successCount++;
                        successMsgList.Add("The transaction is approved for invoice: " + invoice + ". Transaction Code: " + transCode);
                        int paymentID = RecordPaymentHeader(cc, accountCreditCardID, paymentDate, username);

                        string encryptedCreditCardNumber = Instadose.Security.TripleDES.Encrypt(cc.CreditCardNumber);
                        string maskedCardNum = Common.MaskCreditCardNumber(encryptedCreditCardNumber, cc.CreditCardType);

                        GenerateReceipt(paymentID, maskedCardNum, accountID, companyName, username, cc.AccountType);

                        EmailTransactionSuccess(email, accountID, cc.OrbitalOrderNumber, paymentDate, cc.Amount, currencyCode, paymentID, "Credit Card", username);
                    }
                    else
                    {
                        errorMsgList.Add("The transaction is declined for invoice: " + invoice + ". Reason: " + erroMessage);
                    }

                }

                sb.Append("{\"SuccessMessages\":[");
                if (successMsgList.Count > 0)
                {
                    var first = true;
                    foreach (var m in successMsgList)
                    {
                        if (!first)
                            sb.Append(",");
                        sb.Append("\"" + m + "\"");
                        first = false;
                    }
                }
                sb.Append("],\"ErrorMessages\":[");
                if (errorMsgList.Count > 0)
                {
                    // there is not any transaction proceeded successfully then delete the inserted AccountCreditCard record for UseOnlyOnce CC process option
                    if (successCount == 0)
                    {
                        RemoveAccountCreditCard(accountCreditCardID);
                    }
                    var first = true;
                    foreach (var m in errorMsgList)
                    {
                        if (!first)
                            sb.Append(",");
                        sb.Append("\"" + m + "\"");
                        first = false;
                    }
                }
                sb.Append("]}");
               
            }
            catch (Exception ex)
            {
                sb.Append("{\"ErrorMessage\":\"" + ex.Message.Replace(Environment.NewLine, "  ") + "\"}");
            }

            Context.Response.Clear();
            Context.Response.ContentType = "application/json";
            Context.Response.Write(sb.ToString());
            Context.Response.End();
        }

        [WebMethod]
        public int ValidateCreditCard(string creditCardNumber)
        {
            int result = 0;

            // Validate the credit card number.
            var cardType = Validation.ValidateCreditCard(creditCardNumber);

            if (cardType.HasValue)
            {
                result = Convert.ToInt32(cardType);
            }
            return result;
        }

        [WebMethod]
        public string GetAXInvoiceOrderList(string invoiceID)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();

            if (string.IsNullOrEmpty(invoiceID))
            {
                return js.Serialize(new
                {
                    Success = false,
                    Message = "Invoice ID is missing."
                });
            }

            var axInvoiceReq = new AXInvoiceRequests();

            List<string> list = new List<string>();
            List<string> instaOrderIDs = new List<string>();

            bool isSuccess = true;
            string errMsg = "";
            try
            {
                list = axInvoiceReq.GetOrderIDsByInvoice(invoiceID);
            }
            catch (Exception ex)
            {
                isSuccess = false;
                errMsg = ex.Message;
            }

            if (list != null && list.Count > 0)
            {
                foreach (var orderID in list)
                {
                    instaOrderIDs.Add(orderID.Substring(4));
                }
            }

            return js.Serialize(new
            {
                Success = isSuccess,
                Message = errMsg,
                Orders = instaOrderIDs.OrderBy(o => o).ToList()
            });
        }

        private bool RemoveAccountCreditCard(int accountCreditCardID)
        {
            var idc = new InsDataContext();
            var acctCC = (from a in idc.AccountCreditCards where a.AccountCreditCardID == accountCreditCardID select a).FirstOrDefault();

            idc.AccountCreditCards.DeleteOnSubmit(acctCC);

            // Save the changes.
            idc.SubmitChanges();

            return true;
        }

        private bool GenerateReceipt(int paymentID, string maskeCardNum, string accountID, string companyName, string username, int applicationID)
        {
            string pdfFileName = "PaymentReceipt_" + paymentID.ToString() + ".pdf";
            string myCRFileNamePath = Server.MapPath("/Finance/PaymentReceipt.rpt");
            Stream oStream = null;
            ReportDocument cryRpt = new ReportDocument();
            bool success = false;

            try
            {
                // First insert Document record with default DocumentContent into database.
                // then PaymentReceipt report will use this doc record to generate DocumentContent.
                // This work-around solution is adapting for both Instadose and Unix CC processing.
                var idc = new InsDataContext();
                Document dcmntrcrd = new Document()
                {
                    DocumentGUID = Guid.NewGuid(),
                    FileName = pdfFileName,
                    DocumentContent = Enumerable.Repeat((byte)0x20, 7).ToArray(),
                    MIMEType = "application/pdf",
                    DocumentCategory = "Payment Receipt",
                    Description = "Credit Card Payment Receipt",
                    AccountID = applicationID == 2 ? int.Parse(accountID) : new int?(),
                    OrderID = null,
                    CreatedBy = username,
                    CreatedDate = DateTime.Now,
                    Active = true,
                    ReferenceKey = paymentID,
                    CompanyName = companyName,
                    GDSAccount = applicationID == 1 ? accountID.ToUpper() : null
                };

                // Submit record to database.
                idc.Documents.InsertOnSubmit(dcmntrcrd);
                idc.SubmitChanges();

                // Get new PaymentID from Inserted record and generate payment receipt
                int docID = dcmntrcrd.DocumentID;
                if (docID <= 0) return false;

                cryRpt.Load(myCRFileNamePath);

                cryRpt.SetParameterValue("@pPaymentID", paymentID);
                cryRpt.SetParameterValue("@pMaskedCreditCardNumber", maskeCardNum);

                string connStr = System.Configuration.ConfigurationManager.ConnectionStrings["Instadose.Properties.Settings.InsConnectionString"].ConnectionString.ToString();
                string myDatabase = Regex.Match(connStr, "(Initial Catalog=|Database=){1}([A-Za-z0-9_]+)[;]?").Groups[2].ToString();
                string myServer = Regex.Match(connStr, "(Data Source=){1}([\\w\\d\\.\\$\\-]+)[;]?").Groups[2].ToString();
                string myUserID = Regex.Match(connStr, "(User ID=|UID=){1}([\\w\\d\\.]+)[;]?").Groups[2].ToString();
                string myPassword = Regex.Match(connStr, "(Pwd=|Password=|PWD=){1}([\\w\\d\\.\\$]+)[;]?").Groups[2].ToString();

                cryRpt.DataSourceConnections[0].SetConnection(myServer, myDatabase, myUserID, myPassword);

                // Export to Memory Stream.        
                oStream = cryRpt.ExportToStream(CrystalDecisions.Shared.ExportFormatType.PortableDocFormat);
                byte[] streamArray = new byte[oStream.Length];
                oStream.Read(streamArray, 0, Convert.ToInt32(oStream.Length - 1));

                // Update inserted doc with real DocumentContent
                // Look for existing Document.
                Document doc = (from d in idc.Documents where d.DocumentID == docID select d).FirstOrDefault();

                if (doc != null)
                {
                    doc.DocumentContent = streamArray;
                    // Save changes.
                    idc.SubmitChanges();

                    success = true;

                    // Report the error to the message system.
                    Basics.WriteLogEntry("PaymentReceipt was generated successful.", username, "Portal.InstaDose_GenerateReceipt", Basics.MessageLogType.Information);
                }
                else
                {
                    success = false;

                    // Report the error to the message system.
                    Basics.WriteLogEntry("PaymentReceipt was not generated successful.", username, "Portal.InstaDose_GenerateReceipt", Basics.MessageLogType.Critical);
                }
            }
            catch (Exception ex)
            {
                // Report the error to the message system.
                Basics.WriteLogEntry(ex.Message, username, "Portal.InstaDose_GenerateReceipt", Basics.MessageLogType.Critical);
                success = false;

                throw ex;
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

        private bool EmailTransactionSuccess(string email, string accountID, string invoice, DateTime paymentDate, double amount, string currencyCode, int paymentID, string paymentMethod, string username)
        {
            bool RtnResponse = false;
            string toEmail = email;

            //// Create the template.
            MessageSystem msgSystem = new MessageSystem()
            {
                Application = "Instadose.com Credit Card Process Acknowledgement",
                CreatedBy = username,
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
            //string amountStr = "$" + amount.ToString("##.00") + " " + currencyCode;
            string amountStr = amount.ToString("##.00") + " " + currencyCode;

            fields.Add("InvoiceNumber", invoice);
            fields.Add("TransactionDate", String.Format("{0:MMM dd, yyyy}", paymentDate));
            fields.Add("PaymentAmount", amountStr);
            fields.Add("PaymentMethod", paymentMethod);
            fields.Add("ConfirmationNumber", paymentID.ToString());

            // MUST BE EXACT with EmailTemplate (TemplateName)! 
            int response = msgSystem.Send("Payment Confirmation", "", fields);

            if (response != 0) RtnResponse = true; //success!

            return RtnResponse;
        }

        private int RecordPaymentHeader(CreditCardTransactionInfo cc, int accountCreditCardID, DateTime paymentDate, string username)
        {
            var idc = new InsDataContext();
            Payment newPayment = new Payment()
            {
                OrderID = null,
                InvoiceNo = cc.OrbitalOrderNumber,
                PaymentDate = paymentDate,
                Amount = Convert.ToDecimal(cc.Amount),
                GatewayResponse = String.Empty,
                TransCode = cc.TransCode,
                Captured = true,
                Authorized = true,
                PaymentMethodID = 1,
                AccountCreditCardID = accountCreditCardID,
                CreatedBy = username,
                CurrencyCode = (from c in idc.Currencies where c.OrbitalCurrencyCode == cc.CurrencyCode select c.CurrencyCode).FirstOrDefault(),
                ApplicationID = cc.AccountType   // 2: Instadose, 1: Unix
            };

            idc.Payments.InsertOnSubmit(newPayment);
            idc.SubmitChanges();

            return newPayment.PaymentID;
        }

        private string GetCardType(int cardTypeID)
        {
            var idc = new InsDataContext();
            return (from cct in idc.CreditCardTypes where cct.CreditCardTypeID == cardTypeID select cct.CreditCardName.Trim()).FirstOrDefault();
        }

        private int AddCreditCard(CreditCardTransactionInfo cc, string accountID, int cardTypeID, bool useOnce = true)
        {
            var idc = new InsDataContext();

            AccountCreditCard newACC = new AccountCreditCard()
            {
                AccountID = cc.AccountType == 1 ? 2 : int.Parse(accountID),
                CreditCardTypeID = cardTypeID,
                NameOnCard = cc.CardHolderName,
                NumberEncrypted = Instadose.Security.TripleDES.Encrypt(cc.CreditCardNumber),
                SecurityCode = cc.CreditCardSecurityCode,
                ExpMonth = Int32.Parse(cc.CreditCardExpireMonth),
                ExpYear = Int32.Parse(cc.CreditCardExpireYear),
                Active = true,
                UseOnce = useOnce,
                GDSAccount = cc.AccountType == 1 ? accountID.ToUpper() : null
            };

            // Insert the AccountCreditCard on submit.
            idc.AccountCreditCards.InsertOnSubmit(newACC);
            idc.SubmitChanges();

            return newACC.AccountCreditCardID;
        }
    }
}
