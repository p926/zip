using Actuate;
using Mirion.DSD.GDS.API;
using Mirion.DSD.GDS.API.Contexts;
using Mirion.DSD.GDS.API.DataTypes;
using Mirion.DSD.GDS.API.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using Mirion.DSD;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
using System.ComponentModel;

public class CSWSInstaTabHelper
{  
    public string GetNotes(string account, string location = null)
    {
        if (location == "")
            location = null;
        MiscRequests miscRequest = new MiscRequests();

        var notes = miscRequest.GetNotes(account, location);
        return Newtonsoft.Json.JsonConvert.SerializeObject(notes);
    }

    public string AddNote(string account, string location, string message, string initials)
    {
        MiscRequests mr = new MiscRequests();
        var response = mr.AddNotes(account, location, message, initials);

        return Newtonsoft.Json.JsonConvert.SerializeObject(response);
    }

    public void GetLegacyAuditTrans(int iDisplayStart, int iDisplayLength, int iColumns, string sSearch, bool bRegex, int iSortingCols, string sEcho,
                                    string account, string location = null, string wearer = null, string badgeTBRBP = null)
    {
        int? wearerID = null;
        if (!string.IsNullOrEmpty(location) && location == "null")
            location = null;
        if (!string.IsNullOrEmpty(wearer) && wearer == "null")
            wearer = null;
        if (!string.IsNullOrEmpty(badgeTBRBP) && badgeTBRBP == "null")
            badgeTBRBP = null;

        if (!string.IsNullOrEmpty(wearer))
            wearerID = int.Parse(wearer);

        var activityCode = "";
        if (!string.IsNullOrEmpty(sSearch))
        {
            if (sSearch.IndexOf("Account") > -1)
                activityCode = "1";
            if (sSearch.IndexOf("Location") > -1)
                activityCode = "2";
            if (sSearch.IndexOf("Wearer") > -1)
                activityCode = "3";
            if (sSearch.IndexOf("Badge") > -1)
                activityCode = "4";
            if (sSearch.IndexOf("Modify") > -1)
                activityCode += "M";
            if (sSearch.IndexOf("Add") > -1)
                activityCode += "A";
            if (sSearch.IndexOf("Cancel") > -1)
                activityCode += "C";
            if (sSearch.IndexOf("Transfer") > -1)
                activityCode += "X";
        }
        var endDate = DateTime.Today;
        var startDate = endDate.AddYears(-10);
        MiscRequests mr = new MiscRequests();
        var auditTrans = mr.GetLegacyAuditTransactions(account, location, wearerID, badgeTBRBP, startDate, endDate, activityCode);

        var totalCount = auditTrans.Count();
        var filteredCount = auditTrans.Count();
        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        string jIntFormat = "\"{0}\":{1},";
        sb.Append(string.Format(jIntFormat, "recordsTotal", totalCount.ToString()));
        sb.Append(string.Format(jIntFormat, "recordsFiltered", filteredCount.ToString()));
        sb.Append(string.Format(jIntFormat, "draw", sEcho.ToString()));
        var filteredResponse = auditTrans.Skip(iDisplayStart).Take(iDisplayLength).ToList();
        sb.Append("\"data\":" + JsonConvert.SerializeObject(filteredResponse));
        sb.Append("}");
        //Context.Response.Clear();

        //Context.Response.ContentType = "application/json";
        //Context.Response.Write(sb.ToString());
        //Context.Response.End();
    }

    public string CheckReassignment(string account, string location, int wearerID, string badgeTBRBP)
    {
        MiscRequests mReqs = new MiscRequests();
        var rdates = mReqs.GetReassignDates(account, location, wearerID, badgeTBRBP);

        JavaScriptSerializer js = new JavaScriptSerializer();
        return js.Serialize((rdates.Count > 0));
    }

    public string GetIndustry(char code, int ndrType)
    {
        var acctRequest = new AccountRequests();
        var custCode = acctRequest.GetCustomerCodes(code).Select(d => d.CustomerCodeID).FirstOrDefault();
        var industries = acctRequest.GetNDRIndustries(custCode, ndrType).Select(d => d.IndustryRegulatoryCode).ToList();

        JavaScriptSerializer js = new JavaScriptSerializer();
        return js.Serialize(industries);
    }

    public List<GAddress> GetAccountAddresses(string account)
    {
        AddressRequests ar = new AddressRequests();
        var addresses = ar.GetAddresses(account);

        return addresses;
    }

    public List<GQuoteListItem> GetAccountQuote(string account, string brand)
    {
        FinanceRequests fr = new FinanceRequests();

        var quote = fr.GetAccountQuote(account, brand);

        return quote;
    }

    public List<GDailyListItem> GetActivateDailies(string account)
    {
        DailyRequests dr = new DailyRequests();

        var dailies = dr.GetActivateDailies(account);

        return dailies;
    }

    public List<GLocationListItem> GetLocations(string account, bool activeOnly = false)
    {
        LocationRequests lr = new LocationRequests();

        List<GLocationListItem> locations = new List<GLocationListItem>();
        locations = lr.GetLocations(account, null, activeOnly).Where(l => l.Location != "00000").ToList();

        return locations;
    }

    public List<GWearerSearchListItem> GetWearers(string account, string location, bool activeOnly = false)
    {
        WearerRequests wr = new WearerRequests();
        Active? activeWearers = null;
        if (activeOnly) activeWearers = Active.Active;

        var wearers = wr.SearchWearersByText(null, account, location, null, activeWearers);

        return wearers;
    }

    public List<GWearDate> GetWearDates(string account, string location = null, string mfgruncode = null)
    {
        LocationRequests locReqs = new LocationRequests();
        var weardates = locReqs.GetWearDates(account, location, mfgruncode);

        return weardates.OrderBy(w => w.WearDate).ToList();
    }

    public List<KeyValuePair<int, string>> TransferWearerByLocation(string account, string location, string txfrAccount, string txfrLocation,
                                            int beginWearer, int endWearer, bool keepWearerNum, bool allWearers, bool addDailies, bool holderDef, bool reship,
                                            bool express, string noteInitials, DateTime? badgeAssignDate)
    {
        WearerRequests wr = new WearerRequests();
        var response = wr.TransferWearerByLocation(account, location, txfrAccount, txfrLocation, beginWearer, endWearer, keepWearerNum, allWearers,
                                                    addDailies, holderDef, reship, express, noteInitials, badgeAssignDate);

        return response.ToList();
    }

    public string GetNextWearer(string account, string location)
    {
        WearerRequests wr = new WearerRequests();
        DataTable nextWearer = wr.GetNextWearerNumber(account, location);
        string wearerLoc = nextWearer.Rows[0]["LocationHighWearer"].ToString();
        string wearerAcct = nextWearer.Rows[0]["AccountHighWearer"].ToString();
        string nextAvailable = nextWearer.Rows[0]["NextAvailWearer"].ToString();
        string accountFirstOpen = nextWearer.Rows[0]["AccountFirstOpen"].ToString();
        Dictionary<string, string> d = new Dictionary<string, string>
            {
                { "LocationHighWearer", wearerLoc },
                { "AccountHighWearer", wearerAcct },
                { "NextAvailWearer", nextAvailable },
                { "AccountFirstOpen", accountFirstOpen }
            };
        JavaScriptSerializer js = new JavaScriptSerializer();
        return js.Serialize(d.ToList());
    }

    //public string GetBadgeCombinations(string badgeList)
    //{
    //    if (badgeList == "")
    //        return "";
    //    string[] splitBL = badgeList.Split(',');
    //    int tmpInt;
    //    if (splitBL.Length > 0 && int.TryParse(splitBL[0], out tmpInt))
    //    {
    //        int[] bl = Array.ConvertAll<string, int>(splitBL, int.Parse);
    //        DataClassesDataContext dc = new DataClassesDataContext();
    //        var badges = from brp in dc.BadgeBodyRegionParts
    //                     join bt in dc.Badges on brp.BadgeID equals bt.BadgeID
    //                     join br in dc.BodyRegions on brp.BodyRegionID equals br.BodyRegionID into btbr
    //                     where bt.Active
    //                     from b in btbr.DefaultIfEmpty()
    //                     join bp in dc.BodyParts on brp.BodyPartID equals bp.BodyPartID into bb
    //                     from btt in bb.DefaultIfEmpty()
    //                     where bl.Contains(bt.BadgeID)
    //                     select new { bt.BadgeID, bt.BadgeDesc, b.BodyRegionAbbrev, b.BodyRegionDesc, BodyPartAbbrev = btt.BodyPartAbbrev ?? string.Empty, BodyPartDesc = btt.BodyPartDesc ?? string.Empty };

    //        JavaScriptSerializer js = new JavaScriptSerializer();
    //        return js.Serialize(badges.OrderBy(b => b.BadgeID).ThenBy(b => b.BodyRegionAbbrev).ThenBy(b => b.BodyPartAbbrev).ToList());
    //    }
    //    else
    //        return "";
    //}

    //public string GetBadgeTypes()
    //{
    //    DataClassesDataContext dc = new DataClassesDataContext();
    //    var badges = from bt in dc.Badges
    //                 where bt.Active
    //                 select new { bt.BadgeID, bt.BadgeDesc };

    //    JavaScriptSerializer js = new JavaScriptSerializer();
    //    return js.Serialize(badges.ToList());
    //}

    public string GetFrequencies()
    {
        var lr = new LocationRequests();
        var freqs = lr.GetFrequencies().Where(f => f.MfgRunStatus.HasValue && f.MfgRunStatus.Value).Select(f => new { Value = f.MfgRunCode, Desc = f.MfgRunCode + " - " + f.FrequencyDesc }).Distinct();

        JavaScriptSerializer js = new JavaScriptSerializer();
        return js.Serialize(freqs.ToList());
    }

    //public string GetBadgeHolders()
    //{
    //    DataClassesDataContext dc = new DataClassesDataContext();

    //    var holders = (from h in dc.BadgeHolders
    //                   where h.Active
    //                   select new { h.BadgeID, h.HolderType, h.BadgeHolderDesc }).ToList();

    //    JavaScriptSerializer js = new JavaScriptSerializer();
    //    return js.Serialize(holders);
    //}

    //public string GetHolderColors()
    //{
    //    DataClassesDataContext dc = new DataClassesDataContext();
    //    var colors = from c in dc.BadgeHolderColors
    //                 where c.Active
    //                 select new { c.BadgeID, c.HolderType, c.Color };

    //    JavaScriptSerializer js = new JavaScriptSerializer();
    //    return js.Serialize(colors.ToList());
    //}

    public string AddBadgeColors(GBadgeColor bcolor, string initials)
    {
        ManufacturingRequests mr = new ManufacturingRequests();

        int responseCode = mr.AddBadgeColors(bcolor, initials);

        Dictionary<string, string> response = new Dictionary<string, string>();
        if (responseCode != 0)
            response.Add("Error", string.Format("{0} : {1}", responseCode.ToString(), Mirion.DSD.GDS.API.Helpers.HError.GetError(responseCode)));
        else
            response.Add("Success", responseCode.ToString());

        JavaScriptSerializer js = new JavaScriptSerializer();
        return js.Serialize(response);
    }

    public string GetReportUrl(string reportType = "", string account = "", string location = "", string wearer = "", string wearerID = "",
                                string year = "", string startDate = "", string endDate = "", string doseWeight = "", string doseWeightType = "",
                                string supDOB = "1", string supID = "1")
    {

        StringBuilder sb = new StringBuilder("");
        try
        {
            Instadose.Data.InsDataContext idc = new Instadose.Data.InsDataContext();

            var reportConfig = (from c in idc.ReportConfigs
                                where c.ReportName == reportType
                                select c).FirstOrDefault();

            if (reportConfig != null)
            {
                string reportPath = reportConfig.ReportPath;
                string reportName = reportType;
                string reportURL = reportConfig.ReportURL;
                string userName = reportConfig.UserName;
                string password = reportConfig.Password;
                string vp = "GDSRPT11";

                Dictionary<string, string> Arguments = new Dictionary<string, string>();
                if (account != "") Arguments.Add("pAccount", account);
                if (location != "") Arguments.Add("pLocation", location);
                if (wearer != "") Arguments.Add("pWearerID", wearer);
                if (wearerID != "") Arguments.Add("pWearerSSNID", wearerID);
                if (year != "") Arguments.Add("pYear", year);
                if (startDate != "") Arguments.Add("pStartDate", startDate);
                if (endDate != "") Arguments.Add("pEndDate", endDate);
                if (doseWeight != "") Arguments.Add("pDoseWeight", doseWeight);
                if (doseWeightType != "") Arguments.Add("pDoseWeightType", doseWeightType);
                Arguments.Add("pSuppressDOB", supDOB);
                Arguments.Add("pSuppressID", supID);

                string RequestType = "immediate";
                string RepositoryType = "Enterprise";
                bool InvokeSubmit = true;

                // Create the encoder object used to encrypt parts of the actuate string.
                Encode encoder = new Encode();

                // Build the token. The token consists of properties used when calling the report along with the executable name.
                StringBuilder token = new StringBuilder("__executableName=" + reportPath);

                // Build the string with each of the arguments
                foreach (KeyValuePair<string, string> argument in Arguments)
                    token.AppendFormat("&{0}={1}", argument.Key, argument.Value);

                // Encrypt the required data
                string encodedUsername = encoder.EncodeStr(userName);
                string encodedPassword = encoder.EncodeStr(password);
                string encodedToken = encoder.EncodeStr(token.ToString());
                string encodedTime = encoder.EncodeStr(DateTime.Now.ToUniversalTime().ToString("s")); // yyyy-MM-ddTHH:mm:ss

                // Declare the string builder
                sb = new StringBuilder(reportURL);
                sb.AppendFormat("?requesttype={0}", RequestType);

                // Include the token if it is not empty
                if (token.Length > 0)
                    sb.AppendFormat("&{0}", token.ToString());

                sb.AppendFormat("&repositoryType={0}", RepositoryType);
                sb.AppendFormat("&invokeSubmit={0}", InvokeSubmit.ToString().ToLower());
                sb.AppendFormat("&UserID={0}", userName);
                sb.AppendFormat("&Password={0}", password);
                sb.AppendFormat("&__vp={0}", vp);
                sb.AppendFormat("&actuatenm={0}", encodedUsername);
                sb.AppendFormat("&actuatecnf={0}", encodedPassword);
                sb.AppendFormat("&tm={0}", encodedTime);
                sb.AppendFormat("&token={0}", encodedToken);
                //Comment
            }
        }
        catch (Exception ex)
        {
            sb = new StringBuilder(ex.Message);
        }
        return sb.ToString();
    }

    public string GetReportDL4(string reportType)
    {
        string url = "";
        if (reportType == "MR")
            url = "";

        return url;
    }

    public string GetResults(string action, string parameters)
    {
        try
        {
            string wAction = "GDSAPI";
            WebAction webaction;
            if (Enum.TryParse(action, out webaction))
                wAction = Mirion.DSD.GDS.API.Helpers.HEnum.GetGDSValue(webaction);
            if (action.ToLower() == wAction.ToLower())
                wAction = "GDSAPI";
            GDSRequest request = new GDSRequest()
            {
                GDSOption = wAction
            };
            return request.GetJSON(action, parameters);
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
    }

    public List<string> GetWebActions()
    {
        try
        {
            string[] values = Enum.GetNames(typeof(WebAction));

            var filteredValues = values.Where(v => v.StartsWith("Get") || v.StartsWith("Search") || v.StartsWith("Check")).OrderBy(v => v).ToList();

            List<string> d = new List<string>();

            foreach (string str in filteredValues)
            {
                d.Add(string.Format("{0}:{1}", str, Mirion.DSD.GDS.API.Helpers.HEnum.GetActionDescription((WebAction)Enum.Parse(typeof(WebAction), str))));
            }

            return d;
        }
        catch (Exception ex)
        {
            var err = new List<string>
                {
                    ex.Message
                };
            return err;
        }
    }

    public string GenerateReport(string email, string badges, DateTime startdate, DateTime enddate, string details = "")
    {
        GenerateReportRequest rr = new GenerateReportRequest();
        var tmpDetails = details == "on";
        var responseCode = rr.GenerateReport(email, badges, startdate, enddate, tmpDetails);

        Dictionary<string, string> response = new Dictionary<string, string>();
        JavaScriptSerializer js = new JavaScriptSerializer();
        if (responseCode == 0)
            response.Add("Error", "Nothing happened.");
        else if (responseCode <= 50003)
            response.Add("Error", string.Format("{0} : {1}", responseCode.ToString(), Mirion.DSD.GDS.API.Helpers.HError.GetError(responseCode)));
        else
            response.Add("Success", string.Format("Report Spawned on port #{0}", responseCode.ToString().Substring(2)));

        return js.Serialize(response);
    }

    public string GetDocument(string guid)
    {
        Guid id = Guid.Parse(guid);
        Instadose.Data.InsDataContext idc = new Instadose.Data.InsDataContext();
        var document = (from d in idc.Documents
                        where d.DocumentGUID == id && d.Active == true
                        select d.DocumentContent).FirstOrDefault();

        return document.ToString();
    }

    public string GetMfgRunCode(string account, string location)
    {
        var request = new LocationRequests();
        var detail = request.GetLocationInfo(account, location);

        var info = new MfgRunCodeInfo()
        {
            MfgRunCode = detail == null ? string.Empty : detail.MfgRunCode
        };
        if (!string.IsNullOrEmpty(info.MfgRunCode))
        {
            var lr = new LocationRequests();
            info.WearPeriod = lr.GetFrequencies(info.MfgRunCode).Where(f => f.MfgRunStatus.HasValue && f.MfgRunStatus.Value).Select(f => f.WearPeriod).FirstOrDefault();
        }

        JavaScriptSerializer js = new JavaScriptSerializer();
        return js.Serialize(info);
    }

    public string AddInstaOrderDaily(ref string errorMessage, string account, List<string> wearDates, int instaOrderID, GAddress address, string initials, bool holder, bool reShip, bool expressShip, string shippingMethod, bool specHandCharge = true, bool shippingCharge = true, string location = "", string wearerID = "", string badgeTBRBP = "", string shippingAccount = "", string notes = "")
    {
        int responseCode = 0;

        List<DailyError> errors = new List<DailyError>();

        DateTime tmpDate = DateTime.Today;
        DailyRequests dailyRequest = new DailyRequests();
        GDailyDetails daily = new GDailyDetails()
        {
            Account = account,
            BadgeTBRBP = badgeTBRBP,
            NoteInitials = initials,
            Location = location,
            SystemDateTime = DateTime.Today,
            WearerList = wearerID,
            Status = "O",
            HolderFlag = holder ? YesNo.Yes : YesNo.No,
            ReShip = reShip ? YesNo.Yes : YesNo.No,
            ExpressShip = expressShip ? YesNo.Yes : YesNo.No,
            DailySpecInst = notes,
            ShippingAccountNo = shippingAccount
        };
        int tmpInt;
        if (int.TryParse(shippingMethod, out tmpInt))
            daily.ShippingMethod = tmpInt;
        else
            daily.ShippingMethod = null;
        daily.ShippingCharge = shippingCharge ? YesNo.Yes : YesNo.No;
        daily.SpecHandCharge = specHandCharge ? YesNo.Yes : YesNo.No;

        if (address != null)
        {
            if (!string.IsNullOrEmpty(address.Address1))
            {
                AddressRequests ar = new AddressRequests();
                var addrResponseCode = ar.UpdateAddress(address, initials);
                if (addrResponseCode == "0" && address.AddressPtr != "" && address.AddressPtr != "0000") //The address was not updated and it exists
                    addrResponseCode = address.AddressPtr;
                if (addrResponseCode.Length != 4)
                    responseCode = int.Parse(addrResponseCode);
                if (responseCode != 0)
                {
                    errorMessage = HError.GetError(responseCode);
                    errors.Add(new DailyError() { Account = account, Location = location, WearerID = wearerID, BadgeTBRBP = badgeTBRBP, WearDate = tmpDate.ToShortDateString(), ResponseCode = responseCode.ToString(), ErrorMessage = HError.GetError(responseCode) });
                }                    
                else
                    daily.AddressPtr = addrResponseCode;
            }
        }

        if (responseCode == 0)
        {
            foreach (string wearDate in wearDates)
            {
                if (DateTime.TryParse(wearDate, out tmpDate))
                {
                    daily.WearDate = tmpDate;

                    responseCode = dailyRequest.AddInstaOrderDaily(daily, instaOrderID);

                    if (responseCode != 0)
                    {
                        errorMessage = "(" + responseCode + ") " + HError.GetError(responseCode);
                        errors.Add(new DailyError() { Account = account, Location = location, WearerID = wearerID, BadgeTBRBP = badgeTBRBP, WearDate = tmpDate.ToShortDateString(), ResponseCode = responseCode.ToString(), ErrorMessage = HError.GetError(responseCode) });
                    }                        
                }
                else
                {
                    errorMessage = "Invalid Wear Date";
                    errors.Add(new DailyError() { Account = account, Location = location, WearerID = wearerID, BadgeTBRBP = badgeTBRBP, WearDate = wearDate, ResponseCode = "-1", ErrorMessage = "Invalid Wear Date" });
                }
            }
        }

        return responseCode.ToString();
    }

    public string AddDaily(string account, List<string> wearDates, GAddress address, string initials, bool holder, bool reShip, bool expressShip, string shippingMethod, bool specHandCharge = true, bool shippingCharge = true, string location = "", string wearerID = "", string badgeTBRBP = "", string shippingAccount = "", string notes = "")
    {
        int responseCode = 0;

        List<DailyError> errors = new List<DailyError>();

        DateTime tmpDate = DateTime.Today;
        DailyRequests dailyRequest = new DailyRequests();
        GDailyDetails daily = new GDailyDetails()
        {
            Account = account,
            BadgeTBRBP = badgeTBRBP,
            NoteInitials = initials,
            Location = location,
            SystemDateTime = DateTime.Today,
            WearerList = wearerID,
            Status = "O",
            HolderFlag = holder ? YesNo.Yes : YesNo.No,
            ReShip = reShip ? YesNo.Yes : YesNo.No,
            ExpressShip = expressShip ? YesNo.Yes : YesNo.No,
            DailySpecInst = notes,
            ShippingAccountNo = shippingAccount
        };
        int tmpInt;
        if (int.TryParse(shippingMethod, out tmpInt))
            daily.ShippingMethod = tmpInt;
        else
            daily.ShippingMethod = null;
        daily.ShippingCharge = shippingCharge ? YesNo.Yes : YesNo.No;
        daily.SpecHandCharge = specHandCharge ? YesNo.Yes : YesNo.No;

        if (address != null)
        {
            if (!string.IsNullOrEmpty(address.Address1))
            {
                AddressRequests ar = new AddressRequests();
                var addrResponseCode = ar.UpdateAddress(address, initials);
                if (addrResponseCode == "0" && address.AddressPtr != "" && address.AddressPtr != "0000") //The address was not updated and it exists
                    addrResponseCode = address.AddressPtr;
                if (addrResponseCode.Length != 4)
                    responseCode = int.Parse(addrResponseCode);
                if (responseCode != 0)
                    errors.Add(new DailyError() { Account = account, Location = location, WearerID = wearerID, BadgeTBRBP = badgeTBRBP, WearDate = tmpDate.ToShortDateString(), ResponseCode = responseCode.ToString(), ErrorMessage = HError.GetError(responseCode) });
                else
                    daily.AddressPtr = addrResponseCode;
            }
        }

        if (responseCode == 0)
        {
            foreach (string wearDate in wearDates)
            {
                if (DateTime.TryParse(wearDate, out tmpDate))
                {
                    daily.WearDate = tmpDate;

                    responseCode = dailyRequest.AddDaily(daily);

                    if (responseCode != 0)
                        errors.Add(new DailyError() { Account = account, Location = location, WearerID = wearerID, BadgeTBRBP = badgeTBRBP, WearDate = tmpDate.ToShortDateString(), ResponseCode = responseCode.ToString(), ErrorMessage = HError.GetError(responseCode) });
                }
                else
                {
                    errors.Add(new DailyError() { Account = account, Location = location, WearerID = wearerID, BadgeTBRBP = badgeTBRBP, WearDate = wearDate, ResponseCode = "-1", ErrorMessage = "Invalid Wear Date" });
                }
            }
        }

        KeyValuePair<bool, List<DailyError>> response = new KeyValuePair<bool, List<DailyError>>(errors.Count == 0, errors);

        JavaScriptSerializer js = new JavaScriptSerializer();
        return js.Serialize(response);
    }

    public string AddRangeDaily(string account, string wearDate, GAddress address, string initials, bool holder, bool reShip, bool expressShip, string location, int lowWearerID, int highWearerID,
                             string shippingMethod, bool specHandCharge = true, bool shippingCharge = true, string badgeTBRBP = "", string shippingAccount = "", string notes = "")
    {
        KeyValuePair<int, string> response = new KeyValuePair<int, string>();

        DailyRequests request = new DailyRequests();
        GDailyRangeDetails daily = new GDailyRangeDetails();
        JavaScriptSerializer js = new JavaScriptSerializer();
        DateTime tmpDate;
        if (!DateTime.TryParse(wearDate, out tmpDate))
        {
            response = new KeyValuePair<int, string>(-1, "Wear Date is invalid.");
        }
        else
        {
            int responseCode = 0;
            daily.Account = account;
            daily.Location = location;
            daily.WearDate = tmpDate;
            daily.LowWearerID = lowWearerID;
            daily.HighWearerID = highWearerID;
            daily.HolderFlag = holder ? YesNo.Yes : YesNo.No;
            daily.ReShip = reShip ? YesNo.Yes : YesNo.No;
            daily.ExpressShip = expressShip ? YesNo.Yes : YesNo.No;
            daily.NoteInitials = initials;
            daily.DailySpecInst = notes;
            daily.ShippingAccountNo = shippingAccount;
            int tmpInt;
            if (int.TryParse(shippingMethod, out tmpInt))
                daily.ShippingMethod = tmpInt;
            else
                daily.ShippingMethod = null;
            daily.ShippingCharge = shippingCharge ? YesNo.Yes : YesNo.No;
            daily.SpecHandCharge = specHandCharge ? YesNo.Yes : YesNo.No;

            if (address != null)
            {
                if (!string.IsNullOrEmpty(address.Address1))
                {
                    AddressRequests ar = new AddressRequests();
                    var addrResponseCode = ar.UpdateAddress(address, initials);
                    if (addrResponseCode == "0" && address.AddressPtr != "" && address.AddressPtr != "0000") //The address was not updated and it exists
                        addrResponseCode = address.AddressPtr;
                    if (addrResponseCode.Length != 4)
                        responseCode = int.Parse(addrResponseCode);
                    if (responseCode != 0)
                        return js.Serialize(new KeyValuePair<int, string>(responseCode, HError.GetError(responseCode)));
                    else
                        daily.AddressPtr = addrResponseCode;
                }
            }
            if (responseCode == 0)
            {
                responseCode = request.AddRangeDaily(daily);

                if (responseCode == 0)
                    response = new KeyValuePair<int, string>(responseCode, "Success");
                else
                    response = new KeyValuePair<int, string>(responseCode, HError.GetError(responseCode));
            }
        }
        return js.Serialize(response);
    }

    public string UpdateDaily(string account, string wearDate, GAddress address, string systemDateTime, string initials, string newWearDate, bool holder, bool reShip, bool expressShip,
                                string shippingMethod, bool specHandCharge = true, bool shippingCharge = true, string location = "", string wearerID = "", string badgeTBRBP = "", string shippingAccount = "", string notes = "")
    {
        DateTime? tmpWearDate = string.IsNullOrEmpty(wearDate) ? null : (DateTime?)DateTime.Parse(wearDate);
        DateTime? tmpSystemDateTime = string.IsNullOrEmpty(systemDateTime) ? null : (DateTime?)DateTime.Parse(systemDateTime);
        DateTime? tmpNewWearDate = string.IsNullOrEmpty(newWearDate) ? null : (DateTime?)DateTime.Parse(newWearDate);
        JavaScriptSerializer js = new JavaScriptSerializer();
        int tmpWearerID;
        int responseCode = 0;
        string addressPtr = "";
        if (string.IsNullOrEmpty(wearerID))
        {
            tmpWearerID = 0;
        }
        else
        {
            if (!int.TryParse(wearerID, out tmpWearerID))
                tmpWearerID = 0;
        }

        if (address != null)
        {
            if (!string.IsNullOrEmpty(address.Address1))
            {
                AddressRequests ar = new AddressRequests();
                var addrResponseCode = ar.UpdateAddress(address, initials);
                if (addrResponseCode == "0" && address.AddressPtr != "" && address.AddressPtr != "0000") //The address was not updated and it exists
                    addrResponseCode = address.AddressPtr;
                if (addrResponseCode.Length != 4)
                    responseCode = int.Parse(addrResponseCode);
                if (responseCode != 0)
                    return js.Serialize(new KeyValuePair<int, string>(responseCode, HError.GetError(responseCode)));
                else
                    addressPtr = addrResponseCode;
            }
        }

        // status must not be updated.
        responseCode = this.updateDaily(initials, account, location, tmpWearerID, badgeTBRBP, tmpWearDate, tmpSystemDateTime, tmpNewWearDate, holder, reShip, expressShip, string.Empty,
            addressPtr, shippingMethod, shippingAccount, shippingCharge, specHandCharge, notes);
        KeyValuePair<int, string> response;

        if (responseCode == 0)
            response = new KeyValuePair<int, string>(responseCode, "Success");
        else if (responseCode == -1)
            response = new KeyValuePair<int, string>(responseCode, "Daily does not exist.");
        else
            response = new KeyValuePair<int, string>(responseCode, HError.GetError(responseCode));


        return js.Serialize(response);
    }

    public string UpdateInstaOrderDaily(int instaOrderID, string account, string location, string wearDate, GAddress address, string systemDateTime, string initials, string newWearDate, bool holder, bool reShip, bool expressShip, string shippingMethod, bool specHandCharge = true, bool shippingCharge = true, string shippingAccount = "", string notes = "")
    {
        JavaScriptSerializer js = new JavaScriptSerializer();

        DateTime? tmpSystemDateTime = string.IsNullOrEmpty(systemDateTime) ? null : (DateTime?)DateTime.Parse(systemDateTime);
        DateTime? tmpWearDate = string.IsNullOrEmpty(wearDate) ? null : (DateTime?)DateTime.Parse(wearDate);
        DateTime? tmpNewWearDate = string.IsNullOrEmpty(newWearDate) ? null : (DateTime?)DateTime.Parse(newWearDate);

        int responseCode = 0;
        string addressPtr = "";

        if (address != null)
        {
            if (!string.IsNullOrEmpty(address.Address1))
            {
                AddressRequests ar = new AddressRequests();
                var addrResponseCode = ar.UpdateAddress(address, initials);
                if (addrResponseCode == "0" && address.AddressPtr != "" && address.AddressPtr != "0000") //The address was not updated and it exists
                    addrResponseCode = address.AddressPtr;
                if (addrResponseCode.Length != 4)
                    responseCode = int.Parse(addrResponseCode);
                if (responseCode != 0)
                    return js.Serialize(new { Success = false, ResponseCode = responseCode, ErrorMessage = HError.GetError(responseCode) });
                else
                    addressPtr = addrResponseCode;
            }
        }

        responseCode = updateInstaOrderDaily(initials, instaOrderID, account, location, tmpWearDate, tmpSystemDateTime, tmpNewWearDate, holder, reShip, expressShip, string.Empty, addressPtr, shippingMethod, shippingAccount, shippingCharge, specHandCharge, notes);

        bool isSuccess = responseCode == 0;
        string errMsg = string.Empty;

        if (!isSuccess)
        {
            if (responseCode == -1)
                errMsg = "Daily does not exist.";
            else if (responseCode == -2)
                errMsg = "Daily is not OPEN.";
            else
                errMsg = HError.GetError(responseCode);
        }

        return js.Serialize(new
        {
            Success = isSuccess,
            ResponseCode = responseCode,
            ErrorMessage = errMsg
        });
    }

    public string CancelDailies(List<DailyDetail> dailies)
    {
        int responseCode = -1;

        List<DailyError> errors = new List<DailyError>();
        DateTime? tmpWearDate;
        DateTime? tmpSystemDateTime;

        foreach (DailyDetail daily in dailies)
        {
            tmpWearDate = string.IsNullOrEmpty(daily.WearDate) ? null : (DateTime?)DateTime.Parse(daily.WearDate);
            tmpSystemDateTime = string.IsNullOrEmpty(daily.SystemDateTime) ? null : (DateTime?)DateTime.Parse(daily.SystemDateTime);

            responseCode = this.updateDaily(daily.Initials, daily.Account, daily.Location, daily.WearerID, daily.BadgeTBRBP, tmpWearDate, tmpSystemDateTime, tmpWearDate, null, null, null, "C", "");

            if (responseCode != 0)
            {
                errors.Add(new DailyError() { Account = daily.Account, Location = daily.Location, WearerID = daily.WearerID.ToString(), BadgeTBRBP = daily.BadgeTBRBP, WearDate = daily.WearDate, ResponseCode = responseCode.ToString(), ErrorMessage = HError.GetError(responseCode) });
            }
        }

        KeyValuePair<bool, List<DailyError>> response = new KeyValuePair<bool, List<DailyError>>(errors.Count == 0, errors);

        JavaScriptSerializer js = new JavaScriptSerializer();
        return js.Serialize(response);
    }

    public string GetDailies(string account, string location = null, int? wearerid = null, string badgeTBRBP = null)
    {
        DailyRequests dr = new DailyRequests();
        var dailies = dr.GetDailies(account, location, wearerid, badgeTBRBP);
        List<Daily> listDaily = new List<Daily>();
        foreach (var d in dailies)
        {
            Daily daily = new Daily();
            daily = (Daily)d;
            var rowData = new Dictionary<string, DailyRow>();
            var dailyRow = new DailyRow()
            {
                Details = d
            };
            rowData.Add("daily", dailyRow);
            daily.DT_RowData = rowData;
            listDaily.Add(daily);
        }
        return JsonConvert.SerializeObject(listDaily);
    }

    public string GetAccountAddress(string account)
    {
        AccountAddress addr = new AccountAddress();

        AccountRequests request = new AccountRequests();
        GAccountDetails accountDetail = request.GetAccountDetails(account);

        KeyValuePair<bool, AccountAddress> response;

        if (accountDetail == null)
        {
            response = new KeyValuePair<bool, AccountAddress>(false, addr);
        }
        else
        {
            addr.Address1 = accountDetail.Address1;
            addr.Address2 = accountDetail.Address2;
            addr.City = accountDetail.City;
            addr.Address4 = accountDetail.Address4;
            addr.State = accountDetail.State;
            addr.Zipcode = accountDetail.ZipCode;
            addr.CountryCode = accountDetail.CountryCode;

            response = new KeyValuePair<bool, AccountAddress>(true, addr);
        }

        JavaScriptSerializer js = new JavaScriptSerializer();
        return js.Serialize(response);
    }
    public string GetWearerIDLabels(string account, string location, bool activeOnly)
    {
        WearerRequests request = new WearerRequests();
        Active? activeWearers = null;

        if (activeOnly)
            activeWearers = Active.Active;

        List<GWearerSearchListItem> wearers = request.SearchWearersByText(null, account, location, null, activeWearers);

        List<KeyValuePair<int, string>> responses = new List<KeyValuePair<int, string>>();

        if (wearers != null && wearers.Count() > 0)
        {
            foreach (GWearerSearchListItem wearer in wearers)
            {
                responses.Add(new KeyValuePair<int, string>(wearer.WearerID, wearer.label));
            }
        }

        JavaScriptSerializer js = new JavaScriptSerializer();
        return js.Serialize(responses);
    }

    public string GetDates()
    {
        var today = DateTime.Now;
        var utcToday = today.ToUniversalTime();
        var st = today.AddMonths(-8).ToUniversalTime();
        var time = DateTime.Now;
        var tzs = new List<TimeZoneResponse>();
        var idc = new Instadose.Data.InsDataContext();
        var timezones = (from t in idc.TimeZones
                         select t);
        tzs.Add(new TimeZoneResponse
        {
            TimeZoneID = 0,
            TimeZoneName = "Default",
            TimeZoneOffset = "-",
            TimeZoneTime = time,
            FormattedTimeZoneTime = time.ToString("MM/dd/yyyy hh:mm:ss"),
            StandardTime = st
        });
        foreach (var tz in timezones)
        {
            var tzTime = TimeZoneInfo.ConvertTimeFromUtc(utcToday, TimeZoneInfo.FindSystemTimeZoneById(tz.TimeZoneName));
            var stTime = TimeZoneInfo.ConvertTimeFromUtc(st, TimeZoneInfo.FindSystemTimeZoneById(tz.TimeZoneName));
            var tzr = new TimeZoneResponse()
            {
                TimeZoneID = tz.TimeZoneID,
                TimeZoneName = tz.TimeZoneName,
                TimeZoneOffset = tz.TimeZoneOffset,
                TimeZoneTime = tzTime,
                FormattedTimeZoneTime = tzTime.ToString("MM/dd/yyyy hh:mm:ss"),
                StandardTime = stTime,
                ActualOffset = utcToday.Subtract(tzTime)
            };
            tzs.Add(tzr);
        }

        return JsonConvert.SerializeObject(tzs);
    }
    public class TimeZoneResponse
    {
        public int TimeZoneID { get; set; }
        public string TimeZoneName { get; set; }
        public string TimeZoneOffset { get; set; }
        public DateTime TimeZoneTime { get; set; }
        public string FormattedTimeZoneTime { get; set; }
        public DateTime StandardTime { get; set; }
        public TimeSpan ActualOffset { get; set; }
    }
    #region Private Methods
    private int updateDaily(string initials, string account, string location, int wearerID, string badgeTBRBP, DateTime? wearDate, DateTime? systemDateTime, DateTime? newWearDate,
                            bool? holder, bool? reShip, bool? expressShip, string status, string addressPtr, string shippingMethod = "1", string shippingAccount = "", bool shippingCharge = true, bool specHandCharge = true, string notes = "")
    {
        DailyRequests request = new DailyRequests();

        string tmpLocation = string.IsNullOrEmpty(location) ? null : location;
        int? tmpWearerID = wearerID == 0 ? null : (int?)wearerID;
        string tmpBadgeTBRBP = string.IsNullOrEmpty(badgeTBRBP) ? null : badgeTBRBP;

        GDailyDetails daily = request.GetDailies(account, tmpLocation, tmpWearerID, tmpBadgeTBRBP, wearDate, systemDateTime).FirstOrDefault();

        if (daily == null)
        {
            return -1;
        }
        else
        {
            // Account, Location, Wearer, Badge cannot be changed.
            if (daily.WearDate != newWearDate)  // update new wear date, if existing wear date and new wear date is different.
                daily.NewWearDate = newWearDate;
            if (holder != null)
                daily.HolderFlag = (bool)holder ? YesNo.Yes : YesNo.No;
            if (reShip != null)
                daily.ReShip = (bool)reShip ? YesNo.Yes : YesNo.No;
            if (expressShip != null)
                daily.ExpressShip = (bool)expressShip ? YesNo.Yes : YesNo.No;
            if (!string.IsNullOrEmpty(status))
                daily.Status = status;
            daily.AddressPtr = addressPtr;
            int tmpInt;
            if (int.TryParse(shippingMethod, out tmpInt))
                daily.ShippingMethod = tmpInt;
            else
                daily.ShippingMethod = null;
            daily.ShippingAccountNo = shippingAccount;
            daily.ShippingCharge = shippingCharge ? YesNo.Yes : YesNo.No;
            daily.SpecHandCharge = specHandCharge ? YesNo.Yes : YesNo.No;
            daily.DailySpecInst = notes;

            return request.UpdateDaily(daily, initials);
        }
    }

    /// <summary>
    /// Update Daily for Instadose Order. Returns 0 for success, -1 for DailyWorkOrder not found, -2 for DailyWorkOrder status is not OPEN, and else for error
    /// </summary>
    /// <param name="initials"></param>
    /// <param name="instaOrderID"></param>
    /// <param name="account"></param>
    /// <param name="location"></param>
    /// <param name="wearDate"></param>
    /// <param name="systemDateTime"></param>
    /// <param name="newWearDate"></param>
    /// <param name="holder"></param>
    /// <param name="reShip"></param>
    /// <param name="expressShip"></param>
    /// <param name="status"></param>
    /// <param name="addressPtr"></param>
    /// <param name="shippingMethod"></param>
    /// <param name="shippingAccount"></param>
    /// <param name="shippingCharge"></param>
    /// <param name="specHandCharge"></param>
    /// <param name="notes"></param>
    /// <returns></returns>
    private int updateInstaOrderDaily(string initials, int instaOrderID, string account, string location, DateTime? wearDate, DateTime? systemDateTime, DateTime? newWearDate, bool? holder, bool? reShip, bool? expressShip, string status, string addressPtr, string shippingMethod = "1", string shippingAccount = "", bool shippingCharge = true, bool specHandCharge = true, string notes = "")
    {
        Mirion.DSD.GDS.API.Requests.InstadoseOrderRequests orderRequest = new Mirion.DSD.GDS.API.Requests.InstadoseOrderRequests();

        DailyWorkOrder workOrder = orderRequest.GetDailyWorkOrder(instaOrderID);

        if (workOrder == null)
            return -1;

        if (workOrder.DailyStatus != "O")
            return -2;

        DailyRequests request = new DailyRequests();

        // convert sql DailyWorkOrder data to GDailyDetails
        GDailyDetails daily = new GDailyDetails()
        {
            Account = workOrder.GDSAccount,
            Location = workOrder.GDSLocation,
            WearerID = 0,
            BadgeTBRBP = null,
            WearDate = wearDate,
            SystemDateTime = workOrder.UNIXSystemDate,
            StatusDateTime = workOrder.ModifiedDate,
            Status = workOrder.DailyStatus,
            NoteInitials = initials,
            MfgRunCode = workOrder.MfgRunCode,
            WearPeriod = workOrder.WearPeriod,
            HolderFlag = workOrder.HolderRequired == true ? YesNo.Yes : YesNo.No,
            ExpressShip = workOrder.ExpressShipment == true ? YesNo.Yes : YesNo.No,
            ReShip = workOrder.isReshipment == true ? YesNo.Yes : YesNo.No,
            ShippingAccountNo = workOrder.ShippingAccountNo,
            ShippingCharge = workOrder.ShippingCharge ? YesNo.Yes : YesNo.No,
            AddressPtr = workOrder.UNIXAddressPtr,
            DailySpecInst = workOrder.SpecialInstructions,
            SpecHandCharge = workOrder.SpecHandCharge ? YesNo.Yes : YesNo.No,
            ShippingMethod = workOrder.ShippingMethodID,
            WearerList = string.Empty,
            HasInsta = YesNo.Yes,
            InstaOrderID = workOrder.GDSInstaOrderID
        };

        // Account, Location, Wearer, Badge cannot be changed.
        if (daily.WearDate != newWearDate)  // update new wear date, if existing wear date and new wear date is different.
            daily.NewWearDate = newWearDate;
        if (holder != null)
            daily.HolderFlag = (bool)holder ? YesNo.Yes : YesNo.No;
        if (reShip != null)
            daily.ReShip = (bool)reShip ? YesNo.Yes : YesNo.No;
        if (expressShip != null)
            daily.ExpressShip = (bool)expressShip ? YesNo.Yes : YesNo.No;
        if (!string.IsNullOrEmpty(status))
            daily.Status = status;
        daily.AddressPtr = addressPtr;
        int tmpInt;
        if (int.TryParse(shippingMethod, out tmpInt))
            daily.ShippingMethod = tmpInt;
        else
            daily.ShippingMethod = null;
        daily.ShippingAccountNo = shippingAccount;
        daily.ShippingCharge = shippingCharge ? YesNo.Yes : YesNo.No;
        daily.SpecHandCharge = specHandCharge ? YesNo.Yes : YesNo.No;
        daily.DailySpecInst = notes;

        return request.UpdateDaily(daily, initials);
    }

    public string GetInstructionTypes()
    {
        MiscRequests mr = new MiscRequests();
        var types = mr.GetInstructionTypes();

        return JsonConvert.SerializeObject(types);
    }

    public string GetSpecialInstructions(string account, string location, string type)
    {
        MiscRequests mr = new MiscRequests();
        var instructions = mr.GetSpecialInstructions(account, location, type);

        return JsonConvert.SerializeObject(instructions);
    }

    public string UpdateSpecialInstruction(string account, string location, string type, string text)
    {
        if (string.IsNullOrEmpty(type))
            throw new Exception("Type is invalid");
        MiscRequests mr = new MiscRequests();
        var si = new GSpecialInstructions();
        si.Account = account;
        si.Location = location;
        si.Type = type;
        si.Text = text;

        var response = mr.UpdateSpecialInstructions(si);

        return JsonConvert.SerializeObject(response);
    }

    public string GetUserRole(string userName)
    {
        //var userName = "";
        if (userName == "")
        {
            try
            {
                userName = HUserIdentity.GetUserName();
            }
            catch
            {
                userName = "Unknown";
            }
        }
        List<string> belongsToGroups = new List<string>();

        try
        {
            belongsToGroups = ActiveDirectoryQueries.GetADUserGroups(userName);
        }
        catch
        {
            belongsToGroups = new List<string>();
        }

        return JsonConvert.SerializeObject(belongsToGroups);
    }

    #endregion

    #region Classes
    private class DailyError
    {
        public string Account { get; set; }
        public string Location { get; set; }
        public string WearerID { get; set; }
        public string BadgeTBRBP { get; set; }
        public string WearDate { get; set; }
        public string ResponseCode { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class DailyDetail
    {
        public string Account { get; set; }
        public string Location { get; set; }
        public int WearerID { get; set; }
        public string BadgeTBRBP { get; set; }
        public string WearDate { get; set; }
        public string SystemDateTime { get; set; }
        public string Initials { get; set; }
    }

    private class Daily : GDailyDetails
    {
        public Dictionary<string, DailyRow> DT_RowData { get; set; }
    }

    private class DailyRow
    {
        public GDailyDetails Details { get; set; }
    }
    private class MfgRunCodeInfo
    {
        public string MfgRunCode { get; set; }
        public string WearPeriod { get; set; }
    }

    private class AccountAddress
    {
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string Address4 { get; set; }
        public string State { get; set; }
        public string Zipcode { get; set; }
        public string CountryCode { get; set; }
    }
    #endregion

    public class HUserIdentity
    {
        public static string GetUserName()
        {
            string IdentityName = HttpContext.Current.User.Identity.Name;

            string userName = "Unknown";
            if (!string.IsNullOrEmpty(IdentityName))
            {
                string[] arrUserName = IdentityName.Split('\\');
                if (arrUserName.Count() >= 2)
                    userName = arrUserName[1];
            }

            return userName;
        }

        public static string GetUserInitials()
        {
            string userName = GetUserName();

            string initials = string.Empty;

            if (!string.IsNullOrEmpty(userName) && userName != "Unknown")
                initials = userName.Substring(0, 3);

            return initials;
        }

        //public static List<UserApplicationRole> GetUserApplicationRoles()
        //{
        //    string initials = GetUserInitials();

        //    List<UserApplicationRole> userAppRoles = new List<UserApplicationRole>();

        //    if (!string.IsNullOrEmpty(initials))
        //    {
        //        DataClassesDataContext db = new DataClassesDataContext();
        //        CSR csr = db.CSRs.FirstOrDefault(c => c.UserInitials == initials && c.ApplicationDesc == "CSWS");

        //        if (csr != null && csr.CSRRoles != null)
        //        {
        //            List<CSRRole> roles = csr.CSRRoles.ToList();

        //            foreach (CSRRole role in roles)
        //            {
        //                UserApplicationRole appRole = (UserApplicationRole)Enum.ToObject(typeof(UserApplicationRole), role.ApplicationRoleID);

        //                if (Enum.IsDefined(typeof(UserApplicationRole), appRole))
        //                    userAppRoles.Add(appRole);
        //            }
        //        }
        //    }

        //    return userAppRoles;
        //}
    }

    public enum UserApplicationRole : int
    {
        CSRTier1 = 1,
        CSRTier2 = 2,
        Collections = 3,
        Finance = 4,
        DataManagement = 5
    }

    public enum DoseAdjustmentType : int
    {
        Adjustment = 1,
        Estimate = 2,
        Spare = 3,
        Lifetime = 4,
        YearToDate = 5,
        QuarterToDate = 6
    }

    public enum DoseAdjustmentStatus : int
    {
        CSWaiting = 1,
        CSApproved = 2,
        CSRejected = 3,
        DMWaiting = 4,
        DMApproved = 5,
        DMRejected = 6,
        Complete = 7,
        UNIXUpdateError = 8,
        ConfirmationSent = 9
    }

    public enum SqlReportRequestStatus : byte
    {
        Waiting = 1,
        Success = 2,
        Error = 3,
        NoRecord = 4
    }

    public enum SqlReportParamPromptType
    {
        [Description("TB")]
        TextBox,
        [Description("DD")]
        DropDown,
        [Description("DP")]
        DatePicker,
        [Description("CB")]
        CheckBox
    }
}