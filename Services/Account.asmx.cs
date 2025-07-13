using Instadose.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Data.Linq;
using Mirion.DSD.GDS.API.Requests;


namespace portal_instadose_com_v3.Services
{
    /// <summary>
    /// Summary description for Account
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class Account : System.Web.Services.WebService
    {
        [WebMethod]
        [ScriptMethod(UseHttpGet = true, ResponseFormat = ResponseFormat.Json)]
        public List<LocationListItem> GetLocationListItems(int account, bool activeOnly = false)
        {
            InsDataContext idc = new InsDataContext();

            var locations = idc.Locations.Select(l => new { l.AccountID, l.LocationID, l.LocationName, l.Active, l.ScheduleID }).Where(l => l.AccountID == account);
            if (activeOnly)
            {
                locations = locations.Where(l => l.Active);
            }

            List<LocationListItem> locationItems = new List<LocationListItem>();

            if (locations.Count() > 0)
            {
                foreach (var location in locations.ToList())
                {
                    locationItems.Add(new LocationListItem
                    {
                        AccountID = location.AccountID,
                        LocationID = location.LocationID,
                        LocationName = location.LocationName,
                        Active = location.Active,
                        CalendarID = location.ScheduleID,
                        CompanyNo = null
                    });
                }
            }

            return locationItems;
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public ARAgingAccount GetARAging(int account)
        {
            //show blue dialog For all Mirion Brands with 150+ Accept:
            string[] dialogOverrideReps = new[] { "BHEN", "BHER", "JFER", "0000" }; //Ensure these reps always get Red - Yellow - Green
            string[] dialogOverrideCustomerType = new[] { "ICC" }; //No Popup for ICCare - ICC
            var referralCode = "";

            //All Mirion Brands will be blue unless they are in the Rep Array - And 150+          
            //ICC No popup

            try
            {
                //Add business logic here
                ARAgingAccount arAgingAccount = new ARAgingAccount();
                arAgingAccount.AccountNo = account.ToString();

                InsDataContext idc = new InsDataContext();

                var CustomerType = (
                                from a in idc.Accounts
                                where a.AccountID == account

                                select new
                                {
                                    a.AccountID,
                                    a.AccountName,
                                    a.CurrencyCode,
                                    a.RestrictOnlineAccess,
                                    a.ReferralCode,
                                    a.CustomerType.CustomerTypeCode,
                                    a.BrandSource.BrandName
                                }).FirstOrDefault();

                if (CustomerType != null)
                {
                    arAgingAccount.CurrencyCode = CustomerType.CurrencyCode;
                    arAgingAccount.AccountName = CustomerType.AccountName;
                    arAgingAccount.AccountType = CustomerType.CustomerTypeCode;
                    arAgingAccount.BrandName = CustomerType.BrandName;
                    arAgingAccount.IsRestricted = CustomerType.RestrictOnlineAccess.HasValue && CustomerType.RestrictOnlineAccess.Value;

                    //Referal Code is Sales Rep. code
                    referralCode = CustomerType.ReferralCode;

                    double total = 0;
                    double total10 = 0;
                    double total30 = 0;
                    double total46 = 0;
                    double total150 = 0;

                    MASDataContext mdc = new MASDataContext();
                    mdc.CommandTimeout = 300;

                    List<int> pastduemax = new List<int>();
                    List<ARAgingInvoice> arAgingInvoices = new List<ARAgingInvoice>();

                    //Call Stored Procedure
                    var arBalance = mdc.sp_if_GetAccountInvoicesByAccountID(account).Where(b => b.Balance > 0).ToList();

                    foreach (var invoice in arBalance)
                    {
                        if (invoice.Balance > 0 && invoice.InvoiceDate != null)
                        {
                            ARAgingInvoice arAging = new ARAgingInvoice();
                            arAging.InvoiceNo = invoice.InvoiceNo;
                            arAging.InvoiceDate = Convert.ToDateTime(invoice.InvoiceDate).ToShortDateString();
                            arAging.InvoiceBalance = Convert.ToDouble(invoice.Balance);
                            total += arAging.InvoiceBalance;

                            if (Convert.ToDateTime(invoice.InvoiceDate.Value) < DateTime.Now.AddDays(-150))
                            {
                                arAging.InvoiceAge = 150;
                                total150 += Convert.ToDouble(invoice.Balance);
                            }
                            else if (Convert.ToDateTime(invoice.InvoiceDate.Value) < DateTime.Now.AddDays(-46))
                            {
                                arAging.InvoiceAge = 46;
                                total46 += Convert.ToDouble(invoice.Balance);
                            }
                            else if (Convert.ToDateTime(invoice.InvoiceDate.Value) < DateTime.Now.AddDays(-30))
                            {
                                arAging.InvoiceAge = 30;
                                total30 += Convert.ToDouble(invoice.Balance);
                            }
                            else if (Convert.ToDateTime(invoice.InvoiceDate.Value) < DateTime.Now.AddDays(-10))
                            {
                                arAging.InvoiceAge = 10;
                                total10 += Convert.ToDouble(invoice.Balance);
                            }

                            pastduemax.Add(arAging.InvoiceAge);
                            arAgingInvoices.Add(arAging);
                        }
                    }

                    // AX Invoices
                    AXInvoiceRequests axInvoiceRequests = new AXInvoiceRequests();
                    var invoices = axInvoiceRequests.GetInvoicesByAccount("INS-" + account, true);

                    if (invoices != null && invoices.Count > 0)
                    {
                        foreach (var invoice in invoices)
                        {
                            if (invoice.Balance > 0)
                            {
                                ARAgingInvoice arAging = new ARAgingInvoice();
                                arAging.InvoiceNo = invoice.InvoiceID;
                                arAging.InvoiceDate = Convert.ToDateTime(invoice.InvoiceDate).ToShortDateString();
                                arAging.InvoiceBalance = Convert.ToDouble(invoice.Balance);
                                total += arAging.InvoiceBalance;

                                if (invoice.InvoiceDate < DateTime.Now.AddDays(-150))
                                {
                                    arAging.InvoiceAge = 150;
                                    total150 += Convert.ToDouble(invoice.Balance);
                                }
                                else if (invoice.InvoiceDate < DateTime.Now.AddDays(-46))
                                {
                                    arAging.InvoiceAge = 46;
                                    total46 += Convert.ToDouble(invoice.Balance);
                                }
                                else if (invoice.InvoiceDate < DateTime.Now.AddDays(-30))
                                {
                                    arAging.InvoiceAge = 30;
                                    total30 += Convert.ToDouble(invoice.Balance);
                                }
                                else if (invoice.InvoiceDate < DateTime.Now.AddDays(-10))
                                {
                                    arAging.InvoiceAge = 10;
                                    total10 += Convert.ToDouble(invoice.Balance);
                                }

                                pastduemax.Add(arAging.InvoiceAge);
                                arAgingInvoices.Add(arAging);
                            }
                        }
                    }

                    if (arAgingInvoices.Any())
                    {
                        arAgingAccount.AgingInvoices = arAgingInvoices;
                        arAgingAccount.PastDueMax = pastduemax.Max(x => x);

                        arAgingAccount.Past10 = Math.Round(total10, 2);
                        arAgingAccount.Past30 = Math.Round(total30, 2);
                        arAgingAccount.Past46 = Math.Round(total46, 2);
                        arAgingAccount.Past150 = Math.Round(total150, 2);
                        arAgingAccount.Total = Math.Round(total, 2);
                    }
                }

                //If Mirion Brand and 150+ always show Blue Dialog
                if (arAgingAccount.BrandName.ToLower() == "mirion" && arAgingAccount.Past150 > 0)
                {
                    arAgingAccount.ShowDialog = true;
                    arAgingAccount.ShowBlueDialog = true;
                }

                //These guys will always get Red - Yellow - Green
                if (dialogOverrideReps.Any(referralCode.Contains))
                {
                    arAgingAccount.ShowDialog = true;
                    arAgingAccount.ShowBlueDialog = false;
                }

                //No popup of ICC and any others added to dialogOverrideCustomerType array
                if (dialogOverrideCustomerType.Any(arAgingAccount.AccountType.Contains))
                {
                    arAgingAccount.ShowDialog = false;
                    arAgingAccount.ShowBlueDialog = false;
                }

                return arAgingAccount;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [WebMethod]
        public string RestrictedAccountOverride(int account, string noteText, string userName)  
        {
            string result = "";
             
            try
            {
                var daAccount = new Instadose.API.DA.DAAccount();
                daAccount.OverrideRestrictedAccount(account, 30, noteText, userName);

                result = "success";
            }
            catch
            {
                throw;
            }
            return result;
        }
        [WebMethod]
        public string ReinstateOnlineAccessViaPayment(int account, string userName)
        {
            string result = "";

            try
            {
                var noteText = "Online access reinstated due to credit card payment";

                var daAccount = new Instadose.API.DA.DAAccount();
                daAccount.OverrideRestrictedAccount(account, 4, noteText, userName);

                result = "success";
            }
            catch
            {
                throw;
            }
            return result;
        }

        [WebMethod]
        [ScriptMethod(UseHttpGet = false, ResponseFormat = ResponseFormat.Json)]
        public string GetTimeZoneConversion(string datetime, string userid)
        {
            InsDataContext idc = new InsDataContext();
            int intUserid = Convert.ToInt32(userid);
            DateTime dtDatetime = Convert.ToDateTime(datetime).AddHours(12);

            var timezone = (from u in idc.Users where u.UserID == intUserid select u.Location.TimeZone.TimeZoneName).FirstOrDefault();

            DateTime adjustedDateTime = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(dtDatetime, timezone);

            return adjustedDateTime.ToString("MM/dd/yyyy HH:mm:ss");
             
        }
    }
    
}

public class ARAgingAccount 
{
    public string AccountNo { get; set; } 
    public string AccountName { get; set; }
    public string AccountType { get; set; }
    public string CurrencyCode { get; set; } 
    public List<ARAgingInvoice> AgingInvoices { get; set; }
    public double Total { get; set; } = 0;
    public double Past10 { get; set; } = 0;
    public double Past30 { get; set; } = 0;
    public double Past46 { get; set; } = 0;
    public double Past150 { get; set; } = 0;
    public int PastDueMax { get; set; } = 0;
    public string BrandName { get; set; } 
    public bool IsRestricted { get; set; }
    public bool ShowBlueDialog { get; set; } = false;
    public bool ShowDialog { get; set; } = false;
}

public class ARAgingInvoice
{
    public string InvoiceNo { get; set; }
    public string InvoiceDate { get; set; }
    public double InvoiceBalance { get; set; }
    public int InvoiceAge { get; set; } = 0;
}

public class LocationListItem
{
    public int AccountID { get; set; }
    public int LocationID { get; set; }
    public string LocationName { get; set; }
    public bool Active { get; set; }
    public int? CalendarID { get; set; }
    public string CompanyNo { get; set; }
}



