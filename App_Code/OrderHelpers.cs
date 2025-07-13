using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Instadose.Data;
using Instadose.Integration;

using Instadose.Integration.OrbitalPaymentech;

/// <summary>
/// Summary description for OrderHelpers
/// </summary>
public static class OrderHelpers
{
    /// <summary>
    /// Process a credit card authorization through Orbital.
    /// </summary>
    /// <param name="creditCardNumber"></param>
    /// <param name="transactionID"></param>
    /// <param name="response"></param>
    public static void ProcessOrbitalAuthorization(int accountCreditCardID, out string transactionID, out string response)
    {
        InsDataContext idc = new InsDataContext();

        int lastOrderID = GetLastOrderID();

        AccountCreditCard AcctCC = (from a in idc.AccountCreditCards
                                    where a.AccountCreditCardID == accountCreditCardID
                                    select a).FirstOrDefault();

        // Ensure the credit card was found.
        if (AcctCC == null) throw new Exception("The account credit card could not be found.");

        int accountID = AcctCC.AccountID;

        // Get the default location for the account in the business app.
        Location location =
            (from l in idc.Locations
             where l.AccountID == accountID
             && l.IsDefaultLocation == true
             select l).FirstOrDefault();

        // Ensure default location was found.
        if (location == null) throw new Exception("The default location for an account could not be found.");


        OrbitalPaymenTechAPI creditCard = new OrbitalPaymenTechAPI();
        CreditCardTransactionInfo myTransaction = new CreditCardTransactionInfo();

        // Fill the credit card details to submit.
        OrderHelpers.GetCreditCard(AcctCC, location, myTransaction);

        myTransaction.Amount = Math.Round(Convert.ToDouble("0.01"), 2);
        myTransaction.OrbitalOrderNumber = (lastOrderID + 1).ToString() + "-" + accountID.ToString();

        creditCard.DoAuthorization(myTransaction);

        bool success = myTransaction.Success;
        string errorMsg = myTransaction.ErrorMsg;
        string transCode = myTransaction.TransCode;

        transactionID = string.Empty;
        response = string.Empty;

        // Determine if the call to Orbital was successful to record the information
        if (success)
        {
            transactionID = transCode;
        }
        else
        {
            // Report errors from Orbital
            response = "The following error(s) occured during the authorize transaction: <br />" + errorMsg;
        }
    }

    /// <summary>
    /// Process a credit card direct payment through Orbital.
    /// </summary>
    /// <param name="creditCardNumber"></param>
    /// <param name="transactionID"></param>
    /// <param name="response"></param>
    public static void ProcessOrbitalDirectPayment(int accountCreditCardID, out string transactionID, out string response)
    {
        InsDataContext idc = new InsDataContext();

        int lastOrderID = GetLastOrderID();

        AccountCreditCard AcctCC = (from a in idc.AccountCreditCards
                                    where a.AccountCreditCardID == accountCreditCardID
                                    select a).FirstOrDefault();

        // Ensure the credit card was found.
        if (AcctCC == null) throw new Exception("The account credit card could not be found.");   

        int accountID = AcctCC.AccountID;
                
        // Get the default location for the account in the business app.
        Location location =
            (from l in idc.Locations
             where l.AccountID == accountID
             && l.IsDefaultLocation == true
             select l).FirstOrDefault();

        // Ensure default location was found.
        if (location == null) throw new Exception("The default location for an account could not be found.");

        
        OrbitalPaymenTechAPI creditCard = new OrbitalPaymenTechAPI();
        CreditCardTransactionInfo myTransaction = new CreditCardTransactionInfo();
        
        // Fill the credit card details to submit.
        OrderHelpers.GetCreditCard(AcctCC, location, myTransaction);

        myTransaction.Amount = Math.Round(Convert.ToDouble("0.01"), 2);
        myTransaction.OrbitalOrderNumber = (lastOrderID + 1).ToString() + "-" + accountID.ToString();

        creditCard.DoDirectPayment(myTransaction);

        bool success = myTransaction.Success;
        string errorMsg = myTransaction.ErrorMsg;
        string transCode = myTransaction.TransCode;
       
        transactionID = string.Empty;
        response = string.Empty;

        // Determine if the call to Orbital was successful to record the information
        if (success)
        {
            transactionID = transCode;
        }
        else
        {
            // Report errors from Orbital
            response = "The following error(s) occured during the direct payment transaction: <br />" + errorMsg;
        }
    }

    /// <summary>
    /// Obtain the Credit card on file for an account.
    /// </summary>
    /// <param name="AccountId">Account ID from the business app.</param>
    /// <returns>Return the credit card or null if an error occurs.</returns>
    public static void GetCreditCard(AccountCreditCard pAcctCC, Location pLocation, CreditCardTransactionInfo pOrbitalTransaction)
    {
        InsDataContext idc = new InsDataContext();                      

        Currency myCurrency = (from c in idc.Currencies where c.CurrencyCode == pLocation.Account.CurrencyCode select c).FirstOrDefault();

        // Ensure the account currency was found.
        if (myCurrency == null) throw new Exception("The account currency could not be found.");

        pOrbitalTransaction.CreditCardNumber = Instadose.Security.TripleDES.Decrypt(pAcctCC.NumberEncrypted);
        pOrbitalTransaction.CreditCardSecurityCode = pAcctCC.SecurityCode;
        pOrbitalTransaction.CreditCardExpireMonth = pAcctCC.ExpMonth.ToString().PadLeft(2, '0'); // 09
        pOrbitalTransaction.CreditCardExpireYear = pAcctCC.ExpYear.ToString();        // 2015
        pOrbitalTransaction.CreditCardType = pAcctCC.CreditCardType.CreditCardName;

        pOrbitalTransaction.CardHolderName = pAcctCC.NameOnCard;

        pOrbitalTransaction.CurrencyCode = myCurrency.OrbitalCurrencyCode;     // Bristish pound, Canadian, USD, Australian,...  Orbital currency code/Orbital currency exponent
        pOrbitalTransaction.CurrencyExponent = myCurrency.OrbitalExponentCode; // 1 or 2

        //convert selected billing countryID to PayPalCode
        string CountryCodeStr = "US";

        var myPayPalCode = (from a in idc.Countries
                            where a.CountryID == pLocation.BillingCountryID
                            select a.PayPalCode).FirstOrDefault();

        if (myPayPalCode != null) CountryCodeStr = myPayPalCode;

        string myStateAbb = "";
        var myState = (from a in idc.States
                       where a.StateID == pLocation.BillingStateID
                       select a.StateAbbrev).FirstOrDefault();

        if (myState != null) myStateAbb = myState;

        string zipCode = pLocation.BillingPostalCode.Trim();
        string[] zipCodeSplit = zipCode.Split(new string[] { "_", "-", " " }, StringSplitOptions.RemoveEmptyEntries);

        pOrbitalTransaction.CardHolderAddress = pLocation.BillingAddress1.Trim();
        pOrbitalTransaction.CardHolderAddress1 = (pLocation.BillingAddress2 != null) ? pLocation.BillingAddress2.Trim() : "";
        pOrbitalTransaction.CardHolderCity = pLocation.BillingCity.Trim();
        if (pOrbitalTransaction.CardHolderCity.Length > 20)
            pOrbitalTransaction.CardHolderCity = pOrbitalTransaction.CardHolderCity.Substring(0, 20);
        pOrbitalTransaction.CardHolderState = myStateAbb;
        pOrbitalTransaction.CardHolderZipCode = zipCodeSplit.Count() > 0 ? zipCodeSplit[0].Trim() : "";  // "92831"
        pOrbitalTransaction.CardHolderCountry = CountryCodeStr;              
    }

    /// <summary>
    /// Add a credit card to the account or update it if needed.
    /// </summary>
    /// <param name="accountID">Account the card belongs to.</param>
    /// <param name="nameOnCard">Fullname on the card</param>
    /// <param name="creditCardNumber">Credit card number must not contain whitespace or dashes.</param>
    /// <param name="securityCode">CVC or CCV or CVV2</param>
    /// <param name="expMonth">Expiration month 1 - 12</param>
    /// <param name="expYear">Full expiration year</param>
    public static int AddCreditCard(int accountID, string nameOnCard, string creditCardNumber, string securityCode, int expMonth, int expYear)
    {
        InsDataContext idc = new InsDataContext();

        // Ensure the credit card information is provided.
        if (expMonth == 0 || expYear == 0 || nameOnCard == string.Empty || creditCardNumber == string.Empty || securityCode == string.Empty)
            throw new Exception("Please provide all required information before continuing.");

        // Validate the credit card number.
        Instadose.Security.CreditCardType? cardType = Instadose.Security.Validation.ValidateCreditCard(creditCardNumber);
        if (!cardType.HasValue)
            throw new Exception("The credit card number supplied is invalid.");

        // Encrypt the valid number.
        string encryptedNumber = Instadose.Security.TripleDES.Encrypt(creditCardNumber);

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
        
        // Look for existing credit cards
        AccountCreditCard card = (from acc in idc.AccountCreditCards where acc.NumberEncrypted == encryptedNumber select acc).FirstOrDefault();

        // If the card doesn't exist.
        if (card == null)
        {
            // Create the new card
            card = new AccountCreditCard();

            // Add the account credit card to the database.
            idc.AccountCreditCards.InsertOnSubmit(card);
        }

        // Set or update the properties of the card on the table.
        card.AccountID = accountID;
        card.Active = true;
        card.CreditCardTypeID = cardTypeID;
        card.ExpMonth = expMonth;
        card.ExpYear = expYear;
        card.NameOnCard = nameOnCard;
        card.NumberEncrypted = encryptedNumber;
        card.SecurityCode = securityCode;

        // Save changes.
        idc.SubmitChanges();

        return card.AccountCreditCardID;
    }


    /// <summary>
    /// Update a credit card's expriation date when it already exists.
    /// </summary>
    /// <param name="accountCreditCardID"></param>
    /// <param name="expMonth"></param>
    /// <param name="expYear"></param>
    public static void UpdateCreditCardExpiration(int accountCreditCardID, int expMonth, int expYear)
    {
        InsDataContext idc = new InsDataContext();

        // Ensure the credit card information is provided.
        if (expMonth == 0 || expYear == 0)
            throw new Exception("Please provide all required information before continuing.");

        // Look for existing credit cards
        AccountCreditCard card = (from acc in idc.AccountCreditCards where acc.AccountCreditCardID == accountCreditCardID select acc).FirstOrDefault();

        // If the card doesn't exist.
        if (card == null)
            throw new Exception("The credit card ID supplied is invalid.");

        // Set or update the properties of the card on the table.
        card.ExpMonth = expMonth;
        card.ExpYear = expYear;

        // Save changes.
        idc.SubmitChanges();
    }

    /// <summary>
    /// cancelOrder - Delete new order (only) if user cancels.
    /// </summary>
    /// <param name="orderID"></param>
    /// <returns></returns>
    public static void DeleteOrder(int orderID)
    {
        InsDataContext idc = new InsDataContext();

        // Query the order.
        var order = idc.Orders.Where(o => o.OrderID == orderID && o.OrderStatusID != 10).FirstOrDefault();

        // Query the order.
        var schedBilling = (from sb in idc.ScheduledBillings where sb.OrderID == orderID select sb).FirstOrDefault();
  
        //Delete all documents dealing with this order.
        var ordDocs = from d in idc.Documents
                      where d.OrderID == orderID
                      select d;

        foreach (var d in ordDocs)
        {
            idc.Documents.DeleteOnSubmit(d);
        }

        //delete RenewalLog (Foreign Key requirement),
        idc.RenewalLogs.DeleteAllOnSubmit(order.RenewalLogs);

        // Delete the order details.
        idc.Payments.DeleteAllOnSubmit(order.Payments);
        idc.OrderDetails.DeleteAllOnSubmit(order.OrderDetails);
        idc.Documents.DeleteAllOnSubmit(order.Documents);
        //Update Scheduled Billing if necessary.  Cancel OrderID!
        if (schedBilling != null)
            schedBilling.OrderID = null;
       
        idc.Orders.DeleteOnSubmit(order);

        // Save the changes.
        idc.SubmitChanges();
    }

   
    /// <summary>
    /// Get the shipping cost for the current order.
    /// </summary>
    /// <param name="state">The ship to state.</param>
    /// <param name="country">The ship to country</param>
    /// <param name="postalCode">The ship to postal code</param>
    /// <param name="shippingMethodID">Shipping Method ID corrosponding to the Instadose database.</param>
    /// <param name="unitsOrdered">Total badges ordered</param>
    /// <returns></returns>
    public static decimal? GetShippingCost(string state, string country, string postalCode, int shippingMethodID, int unitsOrdered)
    {
        decimal shippingCost = 0.00M;
        decimal weight = 0.2M * unitsOrdered;

        InsDataContext idc = new InsDataContext();
        ShippingMethod method = (from sm in idc.ShippingMethods where sm.ShippingMethodID == shippingMethodID select sm).FirstOrDefault();

        if (method == null) return shippingCost;

        // Do not continue if there is no shipping cost.
        if (method.ShippingMethodDesc == "Free Shipping" || method.ShippingMethodDesc == "Renewal") return shippingCost;

        // Get the FedEx rate table.
        FedExGetRates rate = new FedExGetRates(state, country, postalCode, weight.ToString(), method.ShippingMethodDesc);

        // Ensure the rates table found records.
        if (rate.GetShippingMethodsRates.Count <= 0) return null;

        // If a message was returned return a blank cost.
        if (!rate.GetShippingMethodsRates.Contains("TotalNetCharge")) return null;

        decimal.TryParse(rate.GetShippingMethodsRates["TotalNetCharge"].ToString(),
            NumberStyles.Number, new CultureInfo("en-US"), out shippingCost);

        return shippingCost;
    }

    //Implemented for Malavern Intergration for Flat Shipping Rates from Database - Hbabu 04/22/14 
    public static decimal? GetShippingFlatRates(string state, string country, string postalCode, string shippingMethodCode, int unitsOrdered)
    {
        decimal shippingCost = 0.00M;
        decimal weight = 0.2M * unitsOrdered;
        int TotalLargeBox = 0;
        int TotalotherBox = 0;


        //Based on the Flat Rates - hbabu 07/17/2014

        if (state != "HI" && state != "AK")
        {
            state = "ALL";
        }

        // Small Box: 0-6 Devices, Medium Box: 7-21 Devices, Large Box: 22 - 45 Devices.  Provided by Craig on 07/21/2014 
        if (unitsOrdered > 0 && unitsOrdered <= 6)
            unitsOrdered = 6;
        else if (unitsOrdered > 6 && unitsOrdered <= 21)
            unitsOrdered = 21;
        else if (unitsOrdered > 21 && unitsOrdered <= 45)
            unitsOrdered = 45;
        else if (unitsOrdered > 45)
        {   
            TotalLargeBox = unitsOrdered / 45;
            TotalotherBox = unitsOrdered % 45;
            unitsOrdered = 45;
        }

        InsDataContext idc = new InsDataContext();
        Instadose.Data.ShippingFlatRate FlatRate = (from sf in idc.ShippingFlatRates where sf.ShippingMethodID == Convert.ToInt32(shippingMethodCode) && sf.Quantity >= unitsOrdered && sf.StateCode == state select sf).First();

        if (FlatRate == null)
            return shippingCost;
        else
            shippingCost = FlatRate.Rate;  // ?? 0.00M;

        //Calculate Total Boxes and Type of Boxes based on Quantity - Hbabu 07/17/2014
        if (TotalLargeBox > 1)
        {
            shippingCost = shippingCost * TotalLargeBox;
        }
        
        if (TotalotherBox >= 1)
        {
            if (TotalotherBox > 0 && TotalotherBox <= 6)
                TotalotherBox = 6;
            else if (TotalotherBox > 6 && TotalotherBox <= 21)
                TotalotherBox = 21;
            else if (TotalotherBox > 21 && TotalotherBox <= 45)
                TotalotherBox = 45;
            
            Instadose.Data.ShippingFlatRate FlatRateBox = (from sf in idc.ShippingFlatRates where sf.ShippingMethodID == Convert.ToInt32(shippingMethodCode) && sf.Quantity >= TotalotherBox && sf.StateCode == state select sf).First();
            shippingCost = shippingCost + FlatRateBox.Rate;
        }
        return shippingCost;
    }
    
    public static Order GetOrderByID(int orderID)
    {
        var idc = new InsDataContext();

        return idc.Orders.Where(o => o.OrderID == orderID && o.OrderStatusID != 10).FirstOrDefault();
    }

    public static Order GetOrderByAccount(int acctID)
    {
        throw new NotImplementedException();
    }

    public static List<Order> GetOrders(int accountID)
    {
        var idc = new InsDataContext();

        return idc.Orders.Where(o => o.AccountID == accountID && o.OrderStatusID != 10).ToList();
    }

    public static int GetLastOrderID()
    {
        var idc = new InsDataContext();

        return idc.Orders.Where(o => o.OrderStatusID != 10).OrderByDescending(o => o.OrderID).FirstOrDefault().OrderID;
    }
}