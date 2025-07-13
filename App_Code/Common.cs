using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Globalization;

using System.Data;
using Instadose.Data;

/// <summary>
/// Common - commonly used methods, properties, etc. used through out Portal.Instadose.
/// </summary>
public class Common
{
    // Create the database referenceerrorMsg
    public static InsDataContext idc = new InsDataContext();
    
    //Fedex variables - US Dollar.
    public static CultureInfo USCulture = new CultureInfo("en-US");
    
    //*******************************************************************
    //Foreign currencies supported.
    //*******************************************************************
    //Euro
    public static CultureInfo EuroCulture = new CultureInfo("fr-FR");
 
    //Australian Dollar.
    public static CultureInfo AusCulture = new CultureInfo("en-AU");
    //Canada Dollar.
    public static CultureInfo CanCulture = new CultureInfo("en-CA");
    //UK Pounds.
    public static CultureInfo UKCulture = new CultureInfo("en-GB");
 
    /// <summary>
    /// Use by Fedex GetRate
    /// </summary>
    /// <param name="s">Numeric string </param>
    /// <returns>Decimal format</returns>
    public static decimal ParseUSDecimal(string s)
    {
        decimal usd = default(decimal);
        System.Decimal.TryParse(s, NumberStyles.Number, USCulture, out usd);
        return usd;
    }

    public static decimal ParseForeignDecimal(string s, string currencyCode)
    {
        decimal foreignCurrency = default(decimal);

        switch (currencyCode)
        {
            //Australia Dollar?
            case("AUD"):

                System.Decimal.TryParse(s, NumberStyles.Number, AusCulture, out foreignCurrency);
                return foreignCurrency;
                
            //Canadian Dollar?
            case("CAD"):
                
                System.Decimal.TryParse(s, NumberStyles.Number, CanCulture, out foreignCurrency);
                return foreignCurrency;
                
            //British Pound.
            case ("GBP"):
                 
                System.Decimal.TryParse(s, NumberStyles.Number, UKCulture, out foreignCurrency);
                return foreignCurrency;
                            
            //Euro?
            case ("EUR"):

                System.Decimal.TryParse(s, NumberStyles.Number, EuroCulture, out foreignCurrency);
                return foreignCurrency;
                
            default:
                return 0;
        }

    }


    /// <summary>
    /// Convert the usd to the desired currency and the reserve
    /// </summary>
    /// <param name="pAmount">Amount to convert</param>
    /// <param name="pFromCurrency">from currency</param>
    /// <param name="pToCurrency">to currency</param>
    /// <returns>converted amount in string format</returns>
    public static string ConverttoCurrency(decimal pAmount, string pFromCurrency, string pToCurrency)
    {
        try
        {
            if (pToCurrency.ToUpper() == "USD" || pAmount == 0)
            {
                return string.Format("{0:0.00}", pAmount);
            }
            else
            {
                var exchange_rate = idc.sp_GetCurrencyRate(pFromCurrency, pToCurrency, System.DateTime.Today).First();

                if (exchange_rate != null)
                {
                    return string.Format("{0:0.00}", Convert.ToDecimal(exchange_rate.exch_rate) * pAmount);
                }
                else
                {
                    return "";
                }
            }
        }
        catch
        {
            return "";
        }

    }

    /// <summary>
    /// Convert the usd to the desired currency and the reserve
    /// </summary>
    /// <param name="pAmount">Amount to convert</param>
    /// <param name="pFromCurrency">from currency</param>
    /// <param name="pToCurrency">to currency</param>
    /// <returns>converted amount in string format</returns>
    public static string ConverttoForeignCurrency(decimal pAmount, string pFromCurrency, string pToCurrency)
    {
        try
        {
            if (pToCurrency.ToUpper() == "USD" || pAmount == 0)
            {
                return string.Format("{0:0.00}", pAmount);
            }
            else
            {
                var exchange_rate = idc.sp_GetCurrencyRate(pFromCurrency, pToCurrency, System.DateTime.Today).First();

                if (exchange_rate != null)
                {
                    return string.Format("{0:0.00}", pAmount / Convert.ToDecimal(exchange_rate.exch_rate));
                }
                else
                {
                    return "";
                }
            }
        }
        catch
        {
            return "";
        }

    }

    /// <summary>
    /// RoundDecimal2
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string RoundDecimal2(string value)
    {
        string decimal2value = null;

        double val;
        if (Double.TryParse(value, out val))
        {
            string.Format("{0:0.00}", Math.Round(val, 2));
            decimal2value = Convert.ToString(val);
        }

        return decimal2value;
    }

    ///// <summary>
    ///// CleanString - replaces ' with non-blank up to max length.
    ///// </summary>
    ///// <param name="s"></param>
    ///// <param name="MaxLength"></param>
    ///// <returns></returns>
    //public static String CleanString(String s, int MaxLength)
    //{
    //    String newString = "";
    //    if (s != null)
    //        newString = s.Replace("'", "");

    //    if (newString.Length > MaxLength)
    //        newString = newString.Substring(0, MaxLength);

    //    return newString;

    //}

   /// <summary>
    /// RemoveCurrencySymbol
   /// </summary>
   /// <param name="s"></param>
   /// <param name="CurrCode"></param>
   /// <param name="MaxLength"></param>
   /// <returns></returns>
    public static String RemoveCurrencySymbol(String s, string CurrCode, int MaxLength)
    {
        string currSymbol="";

        if (CurrCode != "USD")
        {
            //find currency code.
            currSymbol = (from c in idc.Currencies
                           where c.CurrencyCode == CurrCode
                           select c.Symbol).FirstOrDefault();
        }
        
        String newString = "";
        if (s != null)
        {
            newString = s.Replace("$", "");
        }
        else
        {
            newString = s.Replace(currSymbol, "");
        }

        if (newString.Length > MaxLength)
            newString = newString.Substring(0, MaxLength);

        return newString;

    }

    /// <summary>
    /// Method to Mask and format Credit Card Number (from Encrypted CC in Database).
    /// </summary>
    /// <param name="encCreditCardNum">The encrypted credit card number from the database.</param>
    /// <param name="creditCardType">Credit card types: American Express, Visa, MasterCard and Discover.</param>
    /// <returns></returns>
    public static string MaskCreditCardNumber(string encCreditCardNum, string creditCardType)
    {
        try
        {
            string creditcardnumber = "";
            creditcardnumber = Instadose.Security.TripleDES.Decrypt(encCreditCardNum);

            // Set a basic mask for the credit card.
            string cardNumberMask = "**** **** **** ";
            // If the type is American Express, use a different mask.
            if (creditCardType == "American Express") cardNumberMask = "**** ****** *";

            // Show only the last 4 digits of the card.
            creditcardnumber = cardNumberMask + creditcardnumber.Substring(creditcardnumber.Length - 4, 4);

            return creditcardnumber;
        }
        catch { return ""; }
    }

    /// <summary>
    /// Get app setting value by appkey
    /// </summary>
    /// <param name="pAppKey"></param>
    /// <returns></returns>
    public static string GetAppSettings(string pAppKey)
    {
        var mySetting = (from AppSet in idc.AppSettings where AppSet.AppKey == pAppKey select AppSet).FirstOrDefault();
        return (mySetting != null) ? mySetting.Value : "";
    }

    /// <summary>
    /// Get estimated shipping date
    /// </summary>
    /// <param name="shippingMethodID"></param>
    /// <returns></returns>
    public static int getShippingOffsetDays(int shippingMethodID)
    {
        if (Convert.ToBoolean(GetAppSettings("MalvernIntegration")))
        {
            if (shippingMethodID > 0)
                return (from a in idc.ShippingMethods where a.ShippingMethodID == shippingMethodID select a.TransitTime.Value).FirstOrDefault();
            else
                return 3;
        }
        else
        {
            switch (shippingMethodID)
            {
                case 1: // Ground +1????
                case 8:
                case 9:
                    return 1;
                case 2: // 2 Day +2
                case 7:
                case 10:
                case 11:
                    return 2;
                case 3: // 3 Day +3
                case 6:
                    return 3;
                case 4: // Ground +7
                case 5:
                case 12:
                    return 7;
                default: // Unknown +3
                    return 3;
            }       
        }
        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="contractStartDate"></param>
    /// <param name="contractEndDate"></param>
    /// <param name="serviceStartDate"></param>
    /// <returns></returns>
    public static DateTime calculateQuarterlyServiceEnd(DateTime contractStartDate, DateTime contractEndDate, DateTime serviceStartDate)
    {
        // Change by TDO, on 03/11/2019. Change the logic to calculate the end of each quarter.
        // The old rule: the end date of quarter = contractStartDate.AddMonths(3)
        // The new rule: the end date of quarter = contractStartDate.AddMonths(3) - 1 day
        DateTime endQrt1 = contractStartDate.AddMonths(3).AddDays(-1);
        DateTime endQrt2 = contractStartDate.AddMonths(6).AddDays(-1);
        DateTime endQrt3 = contractStartDate.AddMonths(9).AddDays(-1);

        if (serviceStartDate < contractStartDate ||
           (serviceStartDate <= endQrt1 && serviceStartDate >= contractStartDate))
            return endQrt1;

        else if (serviceStartDate <= endQrt2 && serviceStartDate > endQrt1)
            return endQrt2;

        else if (serviceStartDate <= endQrt3 && serviceStartDate > endQrt2)
            return endQrt3;

        else if (serviceStartDate <= contractEndDate && serviceStartDate > endQrt3)
            return contractEndDate;

        return serviceStartDate.AddMonths(3).AddDays(-1);
    }

    public static int calculateNumberOfQuarterService(DateTime contractStartDate, DateTime contractEndDate, DateTime serviceStartDate, out DateTime startQuarterDate)
    {
        DateTime startQrt2 = contractStartDate.AddMonths(3);
        DateTime startQrt3 = contractStartDate.AddMonths(6);
        DateTime startQrt4 = contractStartDate.AddMonths(9);

        if (serviceStartDate < startQrt2)
        {
            startQuarterDate = contractStartDate;
            return 4;
        }
        else if (serviceStartDate < startQrt3)
        {
            startQuarterDate = startQrt2;
            return 3;
        }
        else if (serviceStartDate < startQrt4)
        {
            startQuarterDate = startQrt3;
            return 2;
        }
        else
        {
            startQuarterDate = startQrt4;
            return 1;
        }
    }

}

public class OrderAssignUserInfo
{
    public string UserID { get; set; }
    public string BodyRegionID { get; set; }    
    public string Color { get; set; }    
}