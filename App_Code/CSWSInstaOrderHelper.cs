using Instadose;
using Instadose.API;
using Instadose.API.DA;
using Instadose.API.Reports;
using Instadose.Data;
using Mirion.DSD.GDS.API;
using Mirion.DSD.GDS.API.Contexts;
using Mirion.DSD.GDS.API.DataTypes;
using Mirion.DSD.GDS.API.Helpers;
using Mirion.DSD.GDS.API.Models;
using Mirion.DSD.GDS.API.Requests;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;
using System.Web.Services;
using Telerik.Web.UI;

public class CSWSInstaOrderHelper
{    
    public string Assign(string assignedAccount, string assignedLocation, int assignedWearerID, string serial, string bodyRegion, bool isPrimary, string initials)
    {
        try
        {
            BadgeRequests br = new BadgeRequests();
            var response = br.AssignInsBadge(assignedAccount, assignedLocation, assignedWearerID, serial, bodyRegion, isPrimary, initials, true);
            var returnString = "{ \"response\": \"Success\" }";
            if (response != 0)
                returnString = string.Format("{{ \"response\":\"Error\", \"Message\":\"{0}\"}}", HError.GetFormattedError(Math.Abs(response)));

            return returnString;
        }
        catch (Exception ex)
        {
            return string.Format("{{ \"response\":\"Error\", \"Message\":\"{0}\"}}", ex.Message);
        }
    }
    public int Unassign(string assignedAccount, string assignedLocation, int assignedWearerID, string serial, string initials, string reason, int reasonID, bool deactivate = false, string doseWeight = "")
    {
        var idc = new Instadose.Data.InsDataContext();
        //Get who the badge is assigned to
        var userid = (from u in idc.DeviceInventories
                      where u.SerialNo == serial
                      select u.UserDevices.Where(d => d.Active).Select(d => d.UserID).FirstOrDefault()).FirstOrDefault();

        BadgeRequests br = new BadgeRequests();
        var response = br.UnassignInsBadge(assignedAccount, assignedLocation, assignedWearerID, serial, initials);
        if (response == 0)
        {
            if (doseWeight != "")
            {
                WearerRequests wr = new WearerRequests();
                var sb = new StringBuilder();
                var dw = doseWeight.Substring(0, 1);
                var dwt = doseWeight.Substring(1, 1);
                sb.Append(string.Format("{0},{1},{2}", assignedWearerID, dw, dwt));
                var res = wr.SetWearerEDE(assignedAccount, assignedLocation, sb.ToString(), initials, DateTime.Now);
            }
            if (deactivate)
                DeactivateDevice(assignedAccount, assignedWearerID, userid, serial, initials, reason, reasonID);
        }
        return response;
    }
    private int DeactivateDevice(string account, int wearerID, int userid, string serial, string initials, string reason, int reasonID)
    {
        BadgeRequests br = new BadgeRequests();
        var response = br.DeactivateAccountDevice(account, wearerID, userid, serial, initials, null, reason, reasonID);
        return response;
    }
    public int Deactivate(string assignedAccount, int assignedWearerID, int userID, string serial, string reason, int reasonID, string initials)
    {
        var repsonse = DeactivateDevice(assignedAccount, assignedWearerID, userID, serial, initials, reason, reasonID);
        return repsonse;
    }
    public int Reactivate(string assignedAccount, string serial, string initials)
    {
        BadgeRequests br = new BadgeRequests();
        var response = br.ActivateAccountDevice(assignedAccount, serial, initials);
        return response;
    }
    public string ChangeBodyRegion(string account, string location, int wearerID, string serialNo, string bodyRegion, string noteInitials)
    {
        var returnString = "{ \"response\": \"Success\" }";
        InstadoseRequests ir = new InstadoseRequests();
        var response = ir.ChangeBodyRegion(account, location, wearerID, serialNo, bodyRegion, noteInitials);
        if (response != 0)
        {
            returnString = string.Format("{{ \"response\": \"Error\", \"Message\": \"{0} \"}}", HError.GetFormattedError(Math.Abs(response)));
        }
        return returnString;
    }
    //public string MigrateAccount(string account, int billingTermID, string firstname, string lastname, string gender, string ampPhone, string ampEmail, string ampUsername, string initials, float insDiscountAmount = 0)
    public string MigrateAccount(string account, int billingTermID, string firstname, string lastname, string gender, string ampPhone, string ampEmail, string ampUsername, string initials)
    {
        try
        {

            AccountRequests ar = new AccountRequests();
            WearerRequests wr = new WearerRequests();
            MigrateRequests mr = new MigrateRequests();
            var accountDetails = ar.GetAccountDetails(account);
            //Get Wearer
            GWearer wearer = new GWearer()
            {
                FirstName = firstname,
                LastName = lastname,

                //Update wearer to give them AMP+ access
                Gender = (Gender)(Enum.Parse(typeof(Gender), gender)),
                EmailAddress = ampEmail,
                UserName = ampUsername
            };
            accountDetails.BillingTermID = billingTermID;
            accountDetails.InstadoseBillFreq = billingTermID == 2 ? "A" : "Q";
            //accountDetails.InsDiscountPct = insDiscountAmount;
            var updateResponse = ar.UpdateAccount(accountDetails, "ins");
            //int responseCode = wr.UpdateWearer(wearer, initials);

            //if (responseCode < 0)
            //    throw new Exception(HError.GetFormattedError(-responseCode));
            //else
            //{
            //Migrate account to instadose
            Dictionary<int, string> responseCodes = new Dictionary<int, string>();

            responseCodes = mr.MigrateAccountToInstadose(accountDetails, wearer);
            string errMessage = "";
            //Redirect to account details.
            foreach (KeyValuePair<int, string> response in responseCodes)
            {
                if (response.Key == 0)
                    errMessage += string.Format("{0}", response.Value);
            }

            if (errMessage != "")
            {
                return "{\"response\":\"Error\",\"Message\":\"" + errMessage.Replace('"', '\'') + "\"}";
            }
            else
            {
                return "{\"response\":\"Success\"}";
            }
            //}
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
    public string GetRMAReasons()
    {
        AppDataContext adc = new AppDataContext();
        var reasons = (from r in adc.rma_ref_ReturnReasons
                       where r.DepartmentID == 2 && r.Active.Value
                       orderby r.Description
                       select new
                       {
                           r.ReasonID,
                           r.Description
                       }).ToList();
        return JsonConvert.SerializeObject(reasons);
    }
    public string GetTimezones(string account, string location)
    {
        var idc = new Instadose.Data.InsDataContext();
        var locations = (from l in idc.Locations
                         where l.Account.GDSAccount == account
                         select l);
        if (!string.IsNullOrEmpty(location))
            locations = locations.Where(l => l.GDSLocation == location);

        var locationTimezones = locations.Select(l => l.TimeZoneID).Distinct().ToList();

        var timezones = (from t in idc.TimeZones
                         where locationTimezones.Contains(t.TimeZoneID)
                         select new
                         {
                             t.TimeZoneID,
                             t.TimeZoneDesc,
                             t.TimeZoneName,
                             t.TimeZoneOffset
                         }).ToList();

        return JsonConvert.SerializeObject(timezones);
    }
    public int AddCalendar(string account, string calendarName, string calendarDesc, string frequency, string readDay, string readTime, string noteInitials = "ins", string location = null)
    {
        CalendarFrequency freq = CalendarFrequency.Monthly;
        switch (frequency)
        {
            case "B":
                freq = CalendarFrequency.Biweekly;
                break;
            case "W":
                freq = CalendarFrequency.Weekly;
                break;
            case "T":
                freq = CalendarFrequency.TwoMonthly;
                break;
            case "Q":
                freq = CalendarFrequency.Quarterly;
                break;
        }

        var detail = new NewCalendarDetails()
        {
            Name = calendarName,
            Description = calendarDesc,
            Frequency = freq
        };

        switch (freq)
        {
            case CalendarFrequency.Weekly:
            case CalendarFrequency.Biweekly:
                detail.WeeklyOptions = new NewCalendarWeekly()
                {
                    DayOfWeek = (DayOfWeek)Enum.Parse(typeof(DayOfWeek), readDay, true),
                    Time = DateTime.Parse(string.Format("{0:MM/dd/yyyy} {1}", DateTime.Today, readTime))
                };
                break;
            case CalendarFrequency.Quarterly:
                detail.QuarterlyOptions = new NewCalendarQuarterly()
                {
                    DayOfMonth = int.Parse(readDay),
                    Time = DateTime.Parse(string.Format("{0:MM/dd/yyyy} {1}", DateTime.Today, readTime))
                };
                break;
            default:
                detail.MonthlyOptions = new NewCalendarMonthly()
                {
                    DayOfMonth = int.Parse(readDay),
                    Time = DateTime.Parse(string.Format("{0:MM/dd/yyyy} {1}", DateTime.Today, readTime))
                };
                break;

        }

        InsDataContext idc = new InsDataContext();

        var insAccountID = (from a in idc.Accounts
                            where a.GDSAccount == account
                            select a.AccountID).FirstOrDefault();
        if (insAccountID == 0)
            throw new Exception("Account is not instadose ready.");

        int? insLocationID = null;
        if (!string.IsNullOrEmpty(location))
            insLocationID = idc.Locations.FirstOrDefault(l => l.AccountID == insAccountID && l.GDSLocation.Trim() == location.Trim()).LocationID;
        var daCal = new DACalendars();
        int resp = daCal.Create(detail, insAccountID, insLocationID, null, DateTime.Today);

        MiscRequests mReq = new MiscRequests();
        var responseCode = mReq.AddAuditTransaction(account, 0, "6L", noteInitials);

        return resp;
    }
    public int UpdateCalendar(int calendarID, string account, List<DateTime> scheduleDates, string[] lstLocations, string noteInitials = "ins", bool updateDates = true)
    {
        var WSCalendars = new DACalendars();
        InsDataContext idc = new InsDataContext();
        var accountID = (from a in idc.Accounts where a.GDSAccount == account select a.AccountID).FirstOrDefault();
        if (accountID == 0)
            throw new Exception("Account is not instadose ready.");

        CalendarDetails calendar = WSCalendars.Details(calendarID, accountID);
        List<AppointmentInfo> Appointments = new List<AppointmentInfo>();
        foreach (var d in scheduleDates)
        {
            Appointments.Add(new AppointmentInfo(d));
        }
        calendar.CalendarEntries.Clear();

        foreach (AppointmentInfo apt in Appointments)
        {

            calendar.CalendarEntries.Add(new CalendarEntry()
            {
                ScheduleDate = apt.Start
            });

        }
        //Get Instadose Locations
        var insLocations = idc.Locations.Where(l => l.AccountID == accountID).Select(l => l.GDSLocation).ToList();
        //Check to see if any locations being assigned do not exist in instadose
        var nonInstadoseLocations = lstLocations.Where(l => !insLocations.Any(l2 => l2 == l)).ToList();

        foreach (var loc in nonInstadoseLocations)
        {
            MigrateRequests mr = new MigrateRequests();
            var locResponse = mr.AddInsLocation(account, loc);
        }
        // Get the assigned locations based on the checked list items.
        calendar.AssignedLocationIDs = (from l in idc.Locations
                                        where l.AccountID == accountID && lstLocations.Contains(l.GDSLocation)
                                        select l.LocationID).ToList();

        int responseCode = WSCalendars.Update(calendar, updateDates);

        MiscRequests mReq = new MiscRequests();
        var response = mReq.AddAuditTransaction(account, 0, "6P", noteInitials);

        return responseCode;
    }
    public int MigrateLocationToInstadose(string account, string location)
    {
        MigrateRequests mr = new MigrateRequests();
        return mr.MigrateLocationToInstadose(account, location);
    }

    public int DeleteCalendar(int calendarID)
    {
        var WSCalendars = new DACalendars();

        int responseCode = WSCalendars.Delete(calendarID);

        if (responseCode != 0)
        {
            // Print the error message.
            throw new Exception(String.Format("{0} {1}", responseCode, HError.GetFormattedError(responseCode)));
        }

        //MiscRequests mReq = new MiscRequests();
        //responseCode = mReq.AddAuditTransaction(account, 0, "6P", noteInitials);

        return responseCode;
    }

    public string GetCalendar(string account, string location = null)
    {
        try
        {
            DACalendars calendar = new DACalendars();
            var idc = new Instadose.Data.InsDataContext();
            var accountID = idc.Accounts.FirstOrDefault(a => a.GDSAccount == account).AccountID;
            int? locationID = null;
            if (accountID == 0)
                throw new Exception("Not an instadose account");

            if (!string.IsNullOrEmpty(location))
                locationID = idc.Locations.FirstOrDefault(l => l.AccountID == accountID && l.GDSLocation.Trim() == location.Trim()).LocationID;

            CalendarList list = calendar.List(accountID, null, locationID, null);

            return JsonConvert.SerializeObject(list);
        }
        catch (Exception ex)
        {
            // Record an error with the message log.
            Basics.WriteLogEntry(ex.Message, string.Empty,
                "CSWS.Instadose.GetCalendar", Basics.MessageLogType.Critical);

            throw ex;
        }
    }

    public string GetCalendarDetails(int calendarID, string account)
    {
        var WSCalendars = new DACalendars();
        InsDataContext idc = new InsDataContext();
        var acc = (from a in idc.Accounts where a.GDSAccount == account select a).FirstOrDefault();
        if (acc == null)
            throw new Exception("Account is not instadose ready.");

        var calendars = (from acs in acc.AccountSchedules
                         where acs.ScheduleID == calendarID
                         select acs).FirstOrDefault();
        if (calendars == null)
            throw new Exception("Calendar not found.");
        var scheduleDates = calendars.Schedule.ScheduleDates.Where(d => d.Date > DateTime.Today).Select(d => new { d.ScheduleID, Date = d.Date }).ToList();

        var assignedLocations = calendars.Schedule.Locations.Select(l => new { GDSLocation = l.GDSLocation.Trim(), l.LocationName }).ToList();

        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        sb.Append("\"Dates\":" + Newtonsoft.Json.JsonConvert.SerializeObject(scheduleDates.OrderBy(d => d.Date)) + ",");
        sb.Append("\"Locations\":" + Newtonsoft.Json.JsonConvert.SerializeObject(assignedLocations));
        sb.Append("}");
        return sb.ToString();
    }

    public string GetCalendarDetail(int calendarID, string account)
    {
        InsDataContext idc = new InsDataContext();
        var accountID = idc.Accounts.FirstOrDefault(a => a.GDSAccount == account).AccountID;
        if (accountID == 0)
            throw new Exception("Not an instadose account");

        DACalendars calendar = new DACalendars();
        var details = calendar.Details(calendarID, accountID);

        return JsonConvert.SerializeObject(details);
    }

    public void GetDevices(string account, string location, int wearerID = 0)
    {
        int? tmpWearerID = null;
        if (wearerID > 0)
            tmpWearerID = wearerID;
        if (string.IsNullOrEmpty(location))
            location = null;
        BadgeRequests badgeRequest = new BadgeRequests();
        List<GBadgeSearchListItem> badgeList = badgeRequest.SearchInstadoseBadges(account, location, tmpWearerID, null, null, null, true);
        var badges = (from b in badgeList
                      orderby b.BadgeTBRBP, b.SerialNo
                      select b);
        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        sb.Append("\"data\":" + JsonConvert.SerializeObject(badges));
        sb.Append("}");
        //Context.Response.Clear();
        //Context.Response.ContentType = "application/json";
        //Context.Response.Write(sb.ToString());
        //Context.Response.End();
    }

    public void GetDeviceReads(int iDisplayStart, int iDisplayLength, int iColumns, string sSearch, bool bRegex, int iSortCol_0, string sSortDir_0, int iSortingCols, string sEcho,
                                    string account, string location = null, int wearerID = 0, int maxReads = 0)
    {
        List<ReadingHistoryItem> ReadingHistoryItems = new List<ReadingHistoryItem>();
        InsDataContext idc = new InsDataContext();
        ReadingHistory report = new ReadingHistory()
        {
            CreatedDate = DateTime.Now,
            Report = null,
            DoseUnitOfMeasure = "mrem",
            ResponseCode = -1
        };

        var acc = (from a in idc.Accounts where a.GDSAccount == account select a).FirstOrDefault();
        if (acc == null)
            throw new Exception("Account is not instadose ready.");

        Instadose.Data.Location loc = null;
        if (!string.IsNullOrEmpty(location))
            loc = (from l in acc.Locations where l.GDSLocation == location select l).FirstOrDefault();

        Instadose.Data.User user = null;
        if (wearerID > 0)
            user = (from u in loc.Users where u.GDSWearerID == wearerID select u).FirstOrDefault();

        report.DoseUnitOfMeasure = (loc != null) ? loc.LocationUOM.ToLower() : acc.AccountUOM.ToLower();

        // Can take a maximum of 10000 reads.
        if (maxReads == 0) maxReads = 10000;

        var allReadings =
            (from usd in acc.UserDeviceReads
             where
                 (!usd.InitRead) && (!usd.HasAnomaly) && usd.ReadTypeID != 21
             orderby usd.CreatedDate descending
             select new
             {
                 WearerID = usd.User.GDSWearerID,
                 Location = usd.Location.GDSLocation,
                 BodyRegion = usd.BodyRegion.BodyRegionName,
                 DeviceSerialNo = usd.DeviceInventory.SerialNo,
                 ExposureDateServer = usd.CreatedDate,
                 UserFirstName = usd.User.FirstName.ToUpper(),
                 UserLastName = usd.User.LastName.ToUpper(),
                 ELDose = usd.EyeDose.HasValue ? GetReportableDose(usd.EyeDose, report.DoseUnitOfMeasure) : "*",
                 DLDose = GetReportableDose(usd.DeepLowDose, report.DoseUnitOfMeasure),
                 SLDose = GetReportableDose(usd.ShallowLowDose, report.DoseUnitOfMeasure)
             });

        if (!string.IsNullOrEmpty(location))
            allReadings = allReadings.Where(r => r.Location == location);

        if (wearerID > 0)
            allReadings = allReadings.Where(r => r.WearerID == wearerID);

        if (!string.IsNullOrEmpty(sSearch))
        {
            allReadings = allReadings.Where(r => r.ExposureDateServer.ToShortDateString().Contains(sSearch) ||
                                                (r.Location ?? "").Contains(sSearch) ||
                                                (r.WearerID ?? 0).ToString().Contains(sSearch) ||
                                                r.UserLastName.Contains(sSearch) ||
                                                r.UserFirstName.Contains(sSearch) ||
                                                r.DeviceSerialNo.Contains(sSearch) ||
                                                r.BodyRegion.Contains(sSearch));
        }

        var totalCount = allReadings.Count();

        var response = allReadings;
        switch (iSortCol_0)
        {
            case 1:
                response = (sSortDir_0 == "asc") ? response.OrderBy(r => r.Location) : response.OrderByDescending(r => r.Location);
                break;
            case 2:
                response = (sSortDir_0 == "asc") ? response.OrderBy(r => r.WearerID) : response.OrderByDescending(r => r.WearerID);
                break;
            case 3:
                response = (sSortDir_0 == "asc") ? response.OrderBy(r => r.UserLastName) : response.OrderByDescending(r => r.UserLastName);
                break;
            case 4:
                response = (sSortDir_0 == "asc") ? response.OrderBy(r => r.UserFirstName) : response.OrderByDescending(r => r.UserFirstName);
                break;
            case 5:
                response = (sSortDir_0 == "asc") ? response.OrderBy(r => r.DeviceSerialNo) : response.OrderByDescending(r => r.DeviceSerialNo);
                break;
            case 6:
                response = (sSortDir_0 == "asc") ? response.OrderBy(r => r.BodyRegion) : response.OrderByDescending(r => r.BodyRegion);
                break;
            case 7:
                response = (sSortDir_0 == "asc") ? response.OrderBy(r => r.DLDose) : response.OrderByDescending(r => r.DLDose);
                break;
            case 8:
                response = (sSortDir_0 == "asc") ? response.OrderBy(r => r.SLDose) : response.OrderByDescending(r => r.SLDose);
                break;
            case 9:
                response = (sSortDir_0 == "asc") ? response.OrderBy(r => r.ELDose) : response.OrderByDescending(r => r.ELDose);
                break;
            default:
                response = (sSortDir_0 == "asc") ? response.OrderBy(r => r.ExposureDateServer) : response.OrderByDescending(r => r.ExposureDateServer);
                break;
        }
        var filteredCount = response.Count();
        StringBuilder sb = new StringBuilder();
        sb.Append("{");
        string jIntFormat = "\"{0}\":{1},";
        sb.Append(string.Format(jIntFormat, "recordsTotal", totalCount.ToString()));
        sb.Append(string.Format(jIntFormat, "recordsFiltered", filteredCount.ToString()));
        sb.Append(string.Format(jIntFormat, "draw", sEcho.ToString()));
        var filteredResponse = response.Skip(iDisplayStart).Take(iDisplayLength).ToList();
        sb.Append("\"data\":" + JsonConvert.SerializeObject(filteredResponse));
        sb.Append("}");
        //Context.Response.Clear();
        //Context.Response.ContentType = "application/json";
        //Context.Response.Write(sb.ToString());
        //Context.Response.End();
    }

    public string GetReportableDose(double? dose, string uom)
    {
        var roundDose = Math.Round(dose.Value, 0);
        var convDose = Instadose.Basics.ConvertDoseUnitOfMeasure(roundDose, uom);

        return convDose == 0 ? "*" : convDose.ToString();

    }

    public string GetBodyRegions()
    {
        var brBpBt = BadgeRequests.GetBodyRegionBodyPartsByBadgeType("INSTADOSE");
        JavaScriptSerializer js = new JavaScriptSerializer();

        if (brBpBt != null && brBpBt.Count > 0)
        {
            var bodyRegions = brBpBt.Select(b => new { b.BodyRegionAbbrev, b.BodyRegionDesc }).Distinct().ToList();

            return js.Serialize(new { BodyRegions = bodyRegions });
        }
        else
        {
            return js.Serialize(new { BodyRegions = (IEnumerable)null });
        }
    }

    public string GetLocationWearers(string account, string location, bool activeOnly, string searchText)
    {
        WearerRequests wearerRequest = new WearerRequests();
        List<GWearerSearchListItem> wearers = new List<GWearerSearchListItem>();

        Active? activeWearers = null;
        if (activeOnly) activeWearers = Active.Active;
        wearers = wearerRequest.SearchWearersByText(string.Empty, account, location, searchText, activeWearers);

        var result = new { WearerInfos = wearers };

        JavaScriptSerializer js = new JavaScriptSerializer();

        return js.Serialize(result);
    }

    public GLocationDetails GetLocationDetail(string account, string location)
    {
        var lr = new LocationRequests();
        var details = lr.GetLocation(account, location);
        return details;
    }

    public string IsOrderExist(string account, string location, DateTime wearDate, int? orderType , ref string ErrorMessage)
    {        
        int? instaOrderID = null;

        InstadoseOrderRequests request = new InstadoseOrderRequests();

        try
        {
            instaOrderID = request.ExistingInstaOrderID(account, location, wearDate, orderType);
        }
        catch
        {            
            ErrorMessage = "Error occured while getting previous instadose orders";
        }

        return instaOrderID.HasValue ? instaOrderID.Value.ToString() : "";        
    }

    public string GetInstadoseOrderBadgeType(int instaOrderID)
    {
        string errMsg = string.Empty;

        InstadoseOrderRequests request = new InstadoseOrderRequests();

        List<string> badgeTypes = new List<string>();

        try
        {
            badgeTypes = request.GetOrderBadgeTypes(instaOrderID);
        }
        catch
        {
            errMsg = "Error occured while getting badge type data.";
        }

        JavaScriptSerializer js = new JavaScriptSerializer();

        return js.Serialize(new
        {
            ErrorMessage = errMsg,
            BadgeTypes = badgeTypes
        });
    }

    public string AddBadgesToInstadoseOrder(int instaOrderID, bool isConsignmentOrder, string initial, List<InstadoseOrderDetailItem> items, ref string ErrorMessage)
    {
        int responseCd = 0;

        InstadoseOrderRequests orderRequest = new InstadoseOrderRequests();

        // get existing order data
        var order = orderRequest.GetGDSInstaOrderDailyWorkOrderSqlUnion(instaOrderID);

        // if order does not exist, return error message by json
        if (order == null)
        {
            ErrorMessage = "Instadose Order is not found";
            return "-1";            
        }

        // if order status is not Open (O), return error message by json
        if (!order.OrderStatus.Equals("O", StringComparison.OrdinalIgnoreCase))
        {
            ErrorMessage = "Order status is not OPEN.";
            return "-1";            
        }        

        // check whether account is active
        if (!isAccountActive(order.GDSAccount))
        {
            ErrorMessage = "Account is not active. Please reinstate the account before adding badges.";
            return "-1";            
        }

        if (!isLocationActive(order.GDSAccount, order.GDSLocation))
        {
            ErrorMessage = "Location is not active. Please reinstate the location before adding badges.";
            return "-1";            
        }

        // if user does not exist in Instadose database, insert wearers to Instadose database
        List<AddInstadoseWearerError> wearerErrors = new List<AddInstadoseWearerError>();
        migrateUsersByInstaOrder(order.GDSAccount, order.GDSLocation, initial, ref items, ref wearerErrors, 0, 0);

        if (wearerErrors != null && wearerErrors.Count > 0)
        {
            ErrorMessage = "Wearers were not migrated to Instadose Database.";
            return "-1";
        }
        else
        {
            // check whether wearers are assigned with device
            string alreadyAssignedErrorMsg = string.Empty;
            WearerRequests wearerReq = new WearerRequests();

            foreach (var orderDetail in items)
            {
                if (orderDetail.WearerNo != null)
                {
                    // use this one to make sure wearer is not already assigned to the same body region and wearer is not in the pending order
                    //if (isWearerAlreadyAssigned(order.GDSAccount, order.GDSLocation, (int)orderDetail.WearerNo, orderDetail.BodyRegion))
                    //{
                    //    GWearer wearerInfo = wearerReq.GetWearer(order.GDSAccount, order.GDSLocation, (int)orderDetail.WearerNo);                       
                    //    alreadyAssignedErrorMsg += string.IsNullOrEmpty(alreadyAssignedErrorMsg) ? string.Format("{0} {1} ({2})", wearerInfo.FirstName, wearerInfo.LastName, wearerInfo.WearerID) : string.Format(", {0} {1} ({2})", wearerInfo.FirstName, wearerInfo.LastName, wearerInfo.WearerID);
                    //}
                    if (isPendingOrderDetailsByWearer(order.GDSAccount, order.GDSLocation, (int)orderDetail.WearerNo, orderDetail.BodyRegion))
                    {
                        GWearer wearerInfo = wearerReq.GetWearer(order.GDSAccount, order.GDSLocation, (int)orderDetail.WearerNo);
                        alreadyAssignedErrorMsg += string.IsNullOrEmpty(alreadyAssignedErrorMsg) ? string.Format("{0} {1} ({2})", wearerInfo.FirstName, wearerInfo.LastName, wearerInfo.WearerID) : string.Format(", {0} {1} ({2})", wearerInfo.FirstName, wearerInfo.LastName, wearerInfo.WearerID);
                    }
                }
            }

            if (!string.IsNullOrEmpty(alreadyAssignedErrorMsg))
            {
                ErrorMessage = "Device is already assigned to following wearers: " + alreadyAssignedErrorMsg;
                return "-1";
            }

            //string details = generateInstadoseOrderDetailString(items);
            ManufacturingRequests mfgReq = new ManufacturingRequests();

            try
            {
                responseCd = mfgReq.AddDetailsToInstadoseOrder(instaOrderID, isConsignmentOrder, items);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                return "-1";
            }

            if (responseCd > 0)
            {
                ErrorMessage = HError.GetError(responseCd);
                return "-1";                
            }
            else
            {
                // insert audit data
                try
                {
                    MiscRequests mReq = new MiscRequests();
                    //var response = mReq.AddAuditTransaction(order.GDSAccount, 0, "6M", initial, "", order.GDSLocation);
                    var response = mReq.AddAuditTransaction(order.GDSAccount, 0, "6M", initial, instaOrderID.ToString(), order.GDSLocation);
                }
                catch { }
            }
        }

        return responseCd.ToString();
    }

    public string GetInstaOrderStatuses()
    {
        bool isSuccess = true;
        string errMsg = string.Empty;

        InstadoseOrderRequests request = new InstadoseOrderRequests();

        List<Mirion.DSD.GDS.API.Contexts.GDSInstaOrderStatus> statuses = new List<Mirion.DSD.GDS.API.Contexts.GDSInstaOrderStatus>();

        try
        {
            statuses = request.GetOrderStatuses();
        }
        catch (Exception ex)
        {
            isSuccess = false;
            errMsg = ex.Message;
        }

        JavaScriptSerializer js = new JavaScriptSerializer();

        return js.Serialize(new
        {
            Success = isSuccess,
            ErrorMessage = errMsg,
            Statuses = statuses
        });
    }

    public string AddOrderInstadose(InstadoseOrderItem order, string initial, List<RMAWearerInfo> rmaWearers, ref string ErrorMessage)
    {
        // check account is active
        if (!isAccountActive(order.Account))
        {
            ErrorMessage = "Account is not active. Please reinstate the account to place an order.";
            return "-1";
        }

        // check location is active
        if (!isLocationActive(order.Account, order.Location))
        {
            ErrorMessage = "Location is not active. Please reinstate the location to place an order.";
            return "-1";
        }

        // check whether Order Details are empty
        if (order.Details == null || order.Details.Count <= 0)
        {
            ErrorMessage = "Devices not found in an order.";
            return "-1";
        }

        var responseID = 0;
        string msg = string.Empty;

        MigrateRequests migrateRequest = new MigrateRequests();

        // add location to Instadose database if not exist
        try
        {
            responseID = migrateRequest.AddInsLocation(order.Account, order.Location);
        }
        catch (Exception ex)
        {
            ErrorMessage = "Error occured while adding location to Instadose database. <br>" + ex.Message;
            return "-1";
        }

        // insert location to Instadose database, if gds location does not exist in Instadose database.
        int insLocationID = LocationRequests.GetInsLocationID(order.InsAccountID, order.Location);

        // if user does not exist in Instadose database, insert wearers to Instadose database
        List<AddInstadoseWearerError> wearerErrors = new List<AddInstadoseWearerError>();

        List<InstadoseOrderDetailItem> orderDetails = order.Details;
        migrateUsersByInstaOrder(order.Account, order.Location, initial, ref orderDetails, ref wearerErrors, order.InsAccountID, insLocationID);

        order.Details = orderDetails;

        // if wearers are not inserted into instadose database, return error
        if (wearerErrors != null && wearerErrors.Count > 0)
        {
            ErrorMessage = "Wearers were not migrated to Instadose Database.";
            return "-1";
        }

        // check whether wearers are assigned with device
        string alreadyAssignedErrorMsg = string.Empty;
        WearerRequests wearerReq = new WearerRequests();

        foreach (var orderDetail in order.Details)
        {
            if (orderDetail.WearerNo != null)
            {
                // if rma wearer for replacement order exist in new order detail, skip device already assigned validation.
                if ((order.GDSInstaOrderType == 1 || order.GDSInstaOrderType == 2) && rmaWearers != null && rmaWearers.Count > 0)
                {
                    string bodyRegion = BadgeRequests.GetBodyRegionDescByAbbrev(orderDetail.BodyRegion);

                    if (string.IsNullOrEmpty(bodyRegion))
                        continue;

                    var exceptionWearer = rmaWearers.Where(w => w.WearerID == (int)orderDetail.WearerNo && w.BodyRegion == bodyRegion).FirstOrDefault();

                    if (exceptionWearer != null)
                        continue;
                }

                if (isWearerAlreadyAssigned(order.Account, order.Location, (int)orderDetail.WearerNo, orderDetail.BodyRegion))
                {
                    GWearer wearerInfo = wearerReq.GetWearer(order.Account, order.Location, (int)orderDetail.WearerNo);

                    //if (!string.IsNullOrEmpty(alreadyAssignedErrorMsg))
                    //    alreadyAssignedErrorMsg += ", ";

                    //alreadyAssignedErrorMsg += string.Format("{0} {1} ({2}) - {3}", wearerInfo.FirstName, wearerInfo.LastName, wearerInfo.WearerID, orderDetail.BodyRegion);
                    alreadyAssignedErrorMsg += string.IsNullOrEmpty(alreadyAssignedErrorMsg) ? string.Format("{0} {1} ({2})", wearerInfo.FirstName, wearerInfo.LastName, wearerInfo.WearerID) : string.Format(", {0} {1} ({2})", wearerInfo.FirstName, wearerInfo.LastName, wearerInfo.WearerID);
                }
            }
        }

        if (!string.IsNullOrEmpty(alreadyAssignedErrorMsg))
        {
            ErrorMessage = "Device is already assigned to following wearers: " + alreadyAssignedErrorMsg;
            return "-1";
        }

        // seperate orders must be placed for instadose 1 and instadose plus badges
        List<InstadoseOrderDetailItem> orderDetailsInstadose1 = new List<InstadoseOrderDetailItem>();
        List<InstadoseOrderDetailItem> orderDetailsInstadosePlus = new List<InstadoseOrderDetailItem>();

        string[] instadose1BadgeIDs = new string[] { "31" };
        // order details for Instadose 1
        orderDetailsInstadose1 = order.Details.Where(d => instadose1BadgeIDs.Contains(d.BadgeType)).ToList();
        // order details for Instadose plus
        orderDetailsInstadosePlus = order.Details.Where(d => !instadose1BadgeIDs.Contains(d.BadgeType)).ToList();

        InstadoseOrderRequests orderReq = new InstadoseOrderRequests();

        // if InstaOrderType is 4 (New order), check whether previous order exist for the account
        //if (order.GDSInstaOrderType == 4 && AccountRequests.IsOrderExist(order.Account))
        if (order.GDSInstaOrderType == 4 && orderReq.IsAnyInstaOrderExist(order.Account, order.Location))
        {
            // if order does exists, change InstaOrderType to 3 - Add-On
            order.GDSInstaOrderType = 3;
        }

        // check whether Initialization date is passed for 'New' order
        //if (order.GDSInstaOrderType == 4 && !order.InitializationDate.HasValue)
        if (order.GDSInstaOrderType == 4 && !order.InitializationDate.HasValue && !order.IsConsignmentOrder)
        {
            ErrorMessage = "Initialization is missing.";
            return "-1";
        }

        // variable to store insorderid for json object
        List<int> tmpOrderIDs = new List<int>();
        // variable to store ordered badgetype in case either instadose 1 order or instadose plus order does not processed successfully. it will be return to client side by json object
        List<string> orderedBadgeType = new List<string>();

        // Instadose 1 order by dl4 call
        if (orderDetailsInstadose1 != null && orderDetailsInstadose1.Count > 0)
        {
            // generate 'details' field for dl4 call
            //string details = generateInstadoseOrderDetailString(orderDetailsInstadose1);
            int errCd;
            int tmpOrderID = addInstadoseOrder(order, orderDetailsInstadose1, out errCd);

            if (tmpOrderID > 0)
            {
                tmpOrderIDs.Add(tmpOrderID);
                List<string> badgeTypes = orderDetailsInstadose1.Select(d => d.BadgeType).Distinct().ToList();

                if (badgeTypes != null && badgeTypes.Count > 0)
                {
                    foreach (string badgeType in badgeTypes)
                        orderedBadgeType.Add(badgeType);
                }

                // insert new audit data
                try
                {
                    MiscRequests mReq = new MiscRequests();
                    var response = mReq.AddAuditTransaction(order.Account, 0, "6A", initial, tmpOrderID.ToString(), order.Location);
                }
                catch { }
            }
            else
            {
                msg = "Error occured while placing order for Instadose 1 badges - (" + errCd + ") " + HError.GetError(errCd);
            }
        }

        // Instadose Plus order by dl4 call
        if (orderDetailsInstadosePlus != null && orderDetailsInstadosePlus.Count > 0)
        {
            // generate 'details' field for dl4 call
            //string details = generateInstadoseOrderDetailString(orderDetailsInstadosePlus);
            int errCd;
            int tmpOrderID = addInstadoseOrder(order, orderDetailsInstadosePlus, out errCd);

            if (tmpOrderID > 0)
            {
                tmpOrderIDs.Add(tmpOrderID);
                List<string> badgeTypes = orderDetailsInstadosePlus.Select(d => d.BadgeType).Distinct().ToList();

                if (badgeTypes != null && badgeTypes.Count > 0)
                {
                    foreach (string badgeType in badgeTypes)
                        orderedBadgeType.Add(badgeType);
                }

                // insert new audit data
                try
                {
                    MiscRequests mReq = new MiscRequests();
                    var response = mReq.AddAuditTransaction(order.Account, 0, "6A", initial, tmpOrderID.ToString(), order.Location);
                }
                catch { }
            }
            else
            {
                if (!string.IsNullOrEmpty(msg))
                    msg += "|";

                msg += "Error occured while placing order for Instadose+ badges - (" + errCd + ") " + HError.GetError(errCd);
            }
        }

        if (! String.IsNullOrEmpty(msg))
        {
            ErrorMessage = msg;
            return "-1";
        }

        if (tmpOrderIDs.Count() == 0)
        {
            ErrorMessage = "Unknown error. No order is created.";
            return "-1";
        }

        return string.Join(", ", tmpOrderIDs);
    }

    public string CancelOrderInstadose(int instaOrderID, string initial)
    {
        JavaScriptSerializer js = new JavaScriptSerializer();

        if (instaOrderID <= 0)
        {
            return js.Serialize(new
            {
                Success = false,
                ErrorMessage = "Instadose order number is invalid.",
                InstaOrderID = instaOrderID
            });
        }

        InstadoseOrderRequests orderReq = new InstadoseOrderRequests();
        GDSInstaOrder order = orderReq.GetGDSInstaOrder(instaOrderID);

        if (order == null)
        {
            return js.Serialize(new
            {
                Success = false,
                ErrorMessage = "Instadose order not found.",
                InstaOrderID = instaOrderID
            });
        }

        if (order.OrderStatus != "O")
        {
            return js.Serialize(new
            {
                Success = false,
                ErrorMessage = "Instadose order is not OPEN status.",
                InstaOrderID = instaOrderID
            });
        }

        if (order.GDSInstaOrderType == 1 || order.GDSInstaOrderType == 2)
        {
            return js.Serialize(new
            {
                Success = false,
                ErrorMessage = "RMA replacement order cannot be canceled.",
                InstaOrderID = instaOrderID
            });
        }

        bool isSuccess = true;
        string msg = string.Empty;

        ManufacturingRequests mfgReq = new ManufacturingRequests();

        int responseCode = mfgReq.CancelInstadoseOrder(instaOrderID);

        if (responseCode != 0)
        {
            isSuccess = false;
            msg = "UNIX update error";
        }
        else
        {
            // insert audit data
            try
            {
                //var order = orderReq.GetGDSInstaOrder(instaOrderID);
                MiscRequests mReq = new MiscRequests();
                var response = mReq.AddAuditTransaction(order.GDSAccount, 0, "6C", initial, instaOrderID.ToString(), order.GDSLocation);
            }
            catch { }
        }

        return js.Serialize(new
        {
            Success = isSuccess,
            ErrorMessage = msg,
            InstaOrderID = instaOrderID
        });
    }

    public string ReInstateInstadoseOrder(int instaOrderID, string initial)
    {
        bool isSuccess = true;
        string errMsg = string.Empty;
        const byte reInstatablePeriod_Hours = 24;

        if (instaOrderID <= 0)
        {
            isSuccess = false;
            errMsg = "Instadose order number is not valid.";
        }
        else
        {
            InstadoseOrderRequests orderReq = new InstadoseOrderRequests();

            var order = orderReq.GetGDSInstaOrderDailyWorkOrderSqlUnion(instaOrderID);

            if (order == null)
            {
                // if order does not exist, return not success
                isSuccess = false;
                errMsg = "Instadose order not found.";
            }
            else if (order.OrderStatus != "C")
            {
                // if order status is not Canceled, return not success
                isSuccess = false;
                errMsg = "Instadose order status is not valid.";
            }
            else if (((DateTime)order.OrderDate).AddHours(reInstatablePeriod_Hours) < DateTime.Now)
            {
                // Order cannot be reinstated after 24 hour is passed from OrderDate
                isSuccess = false;
                errMsg = "Canceled Instadose order must be reinstated within 24 hours from order date.";
            }
            else
            {
                if (!isAccountActive(order.GDSAccount))     // check account is active
                {
                    isSuccess = false;
                    errMsg = "Account is not active. Please reinstate the account to reinstate the order.";
                }
                else if (!isLocationActive(order.GDSAccount, order.GDSLocation))    // check location is active
                {
                    isSuccess = false;
                    errMsg = "Location is not active. Please reinstate the location to reinstate the order.";
                }
                else
                {
                    int? existingOrderID = orderReq.ExistingInstaOrderID(order.GDSAccount, order.GDSLocation, (DateTime)order.WearDate, order.GDSInstaOrderType);

                    if (existingOrderID != null)
                    {
                        // if order with the same account, location, and wear date exist, return not success
                        isSuccess = false;
                        errMsg = "Instadose order already exist by Account, Location, and Wear Date.";
                    }
                    else
                    {
                        // check order details
                        List<GDSInstaOrderDetail> details = orderReq.GetOrderDetails(instaOrderID);
                        if (details == null || details.Count <= 0)
                        {
                            isSuccess = false;
                            errMsg = "Instadose order badges not found.";
                        }
                        else
                        {
                            // check wearers
                            List<GDSInstaOrderDetail> detailsWithWearer = details.Where(od => od.GDSWearer != null && (int)od.GDSWearer > 0).ToList();

                            if (detailsWithWearer != null && detailsWithWearer.Count > 0)
                            {
                                bool unAssignSuccess = true;

                                foreach (var detail in detailsWithWearer)
                                {
                                    // if Weare is not active, un-assign wearer from order detail
                                    if (!isWearerActive(order.GDSAccount, order.GDSLocation, (int)detail.GDSWearer))
                                    {
                                        unAssignSuccess = unAssignWearerFromInstaOrderDetail(order.InstaOrderID.Value, detail.InstaOrderDetailID, order.IsFromDailyWorkOrderSql);
                                    }
                                }
                            }

                            ManufacturingRequests mfgReq = new ManufacturingRequests();

                            // update order status code to 'O'
                            int responseCode = mfgReq.UpdateInstadoseOrderStatus(instaOrderID, "O");

                            // re-instate daily
                            int dailyResponseCode = mfgReq.ReInstateInstaOrderDaily(order.GDSAccount, order.GDSLocation, instaOrderID);

                            // if re-instate daily is failed, return not success
                            if (dailyResponseCode != 1)
                            {
                                isSuccess = false;
                                errMsg = "Order is re-instated. But error occured while re-instating daily for the order. Please contact IT.";
                            }

                            // insert audit data
                            try
                            {
                                MiscRequests mReq = new MiscRequests();
                                //var response = mReq.AddAuditTransaction(order.GDSAccount, 0, "6R", initial, "", order.GDSLocation);
                                var response = mReq.AddAuditTransaction(order.GDSAccount, 0, "6R", initial, instaOrderID.ToString(), order.GDSLocation);
                            }
                            catch { }
                        }
                    }
                }
            }
        }

        JavaScriptSerializer js = new JavaScriptSerializer();

        return js.Serialize(new
        {
            Success = isSuccess,
            ErrorMessage = errMsg,
            InstaOrderID = instaOrderID
        });
    }

    public string IsReInstateOrderWearersAlreadyAssigned(int instaOrderID)
    {
        JavaScriptSerializer js = new JavaScriptSerializer();

        if (instaOrderID <= 0)
        {
            return js.Serialize(new
            {
                Success = false,
                ErrorMessage = "Instadose order number is not valid."
            });
        }

        InstadoseOrderRequests orderReq = new InstadoseOrderRequests();

        var order = orderReq.GetGDSInstaOrderDailyWorkOrderSqlUnion(instaOrderID);

        if (order == null)
        {
            return js.Serialize(new
            {
                Success = false,
                ErrorMessage = "Instadose order is not found."
            });
        }

        var details = orderReq.GetOrderDetails(instaOrderID);

        if (details == null || details.Count <= 0)
        {
            return js.Serialize(new
            {
                Success = false,
                ErrorMessage = "Instadose order items are not found."
            });
        }

        bool alreadyExist = false;
        string alreadyAssignedErrorMsg = string.Empty;
        WearerRequests wearerReq = new WearerRequests();

        foreach (var detail in details)
        {
            if (detail.GDSWearer.HasValue)
            {
                bool isAssigned = isWearerAlreadyAssigned(order.GDSAccount, order.GDSLocation, (int)detail.GDSWearer, detail.BodyRegion);

                if (isAssigned)
                {
                    alreadyExist = true;

                    GWearer wearerInfo = wearerReq.GetWearer(order.GDSAccount, order.GDSLocation, (int)detail.GDSWearer);

                    if (!string.IsNullOrEmpty(alreadyAssignedErrorMsg))
                        alreadyAssignedErrorMsg += ", ";

                    alreadyAssignedErrorMsg += string.Format("{0} {1} ({2}) - {3}", wearerInfo.FirstName, wearerInfo.LastName, wearerInfo.WearerID, detail.BodyRegion);
                }
            }
        }

        return js.Serialize(new
        {
            Success = true,
            AlreadyAssigned = alreadyExist,
            ErrorMessage = alreadyExist ? alreadyAssignedErrorMsg : ""
        });
    }

    public string UpdateInstadoseOrderStatus(int instaOrderID, string orderStatus, string initial)
    {
        bool isSuccess = true;
        string msg = string.Empty;

        if (instaOrderID <= 0)
        {
            isSuccess = false;
            msg = "Instadose order number is invalid.";
        }
        else if (string.IsNullOrEmpty(orderStatus) || orderStatus.Length != 1)
        {
            isSuccess = false;
            msg = "Instadose Order Status Code is not valid.";
        }
        else
        {
            ManufacturingRequests mfgReq = new ManufacturingRequests();

            int resopnseCode = mfgReq.UpdateInstadoseOrderStatus(instaOrderID, orderStatus);

            // insert audit data
            InstadoseOrderRequests orderReq = new InstadoseOrderRequests();

            try
            {
                var order = orderReq.GetGDSInstaOrder(instaOrderID);
                MiscRequests mReq = new MiscRequests();
                //var response = mReq.AddAuditTransaction(order.GDSAccount, 0, "6M", initial, "", order.GDSLocation);
                var response = mReq.AddAuditTransaction(order.GDSAccount, 0, "6M", initial, instaOrderID.ToString(), order.GDSLocation);
            }
            catch { }
        }

        JavaScriptSerializer js = new JavaScriptSerializer();

        return js.Serialize(new
        {
            Success = isSuccess,
            ErrorMessage = msg,
            InstaOrderID = instaOrderID
        });
    }

    public string GetInstaOrderDetails(int instaOrderID)
    {
        JavaScriptSerializer js = new JavaScriptSerializer();
        InstadoseOrderRequests request = new InstadoseOrderRequests();

        // get order
        var order = request.GetInstaOrdersDailyWorkOrderSqlAndPLOrderUnion(instaOrderID);

        if (order == null)
        {
            return js.Serialize(new
            {
                InstaOrder = (IEnumerable)null,
                InstaOrderBadges = (IEnumerable)null,
                DailyWorkOrder = (IEnumerable)null
            });
        }

        // get order detail, then create anonymous variable to pass thru json
        List<Mirion.DSD.GDS.API.Contexts.vw_GDSInstaOrderDetail> orderDetails = request.GetInstaOrderBadges(instaOrderID);
        var badges = orderDetails.Select(od => new { od.InstaOrderID, od.InstaOrderDetailID, od.BadgeType, od.BadgeDesc, od.BodyRegion, od.Color, od.GDSWearer, od.FirstName, od.LastName, od.SerialNo, od.DeviceStatus, od.Qty }).OrderBy(od => od.InstaOrderDetailID).ToList();

        // get daily work order, then create anonymous variable to pass thru json
        DailyWorkOrder workOrder = request.GetDailyWorkOrder(instaOrderID);
        var daily = workOrder == null ? null : new
        {
            IsReshipment = workOrder.isReshipment,
            ExpressShipment = workOrder.ExpressShipment,
            ShippingMethodID = workOrder.ShippingMethodID,
            SpecHandCharge = workOrder.SpecHandCharge,
            ShippingCharge = workOrder.ShippingCharge
        };

        return js.Serialize(new
        {
            InstaOrder = new
            {
                InstaOrderID = order.InstaOrderID,
                PLOrderID = order.PLOrderID,
                GDSAccount = order.GDSAccount,
                GDSLocation = order.GDSLocation,
                OrderTypeID = order.GDSInstaOrderType,
                OrderTypeName = order.OrderTypeName,
                Status = order.ShipDate != null ? "S" : order.GDSInstaOrderStatus,
                StatusName = order.ShipDate != null ? "Shipped" : order.OrderStatusName,
                MfgRunCode = order.MfgRunCode,
                WearDate = order.WearDate,
                OrderDate = order.OrderDate,
                ShipToEachWearer = order.ShipToEachWearer
            },
            InstaOrderBadges = badges,
            DailyWorkOrder = daily
        });
    }

    public string GetInstaOrders(string account, int instaOrderID, int plOrderID, string location, string gdsOrderStatus, string plOrderStatus, string orderDateFrom, string orderDateTo, string gdsInstaOrderType)
    {
        InstadoseOrderRequests request = new InstadoseOrderRequests();

        var orders = new List<vw_GDSInstaOrdersDailyWorkOrderSqlAndPLOrderUnion>();

        if (instaOrderID > 0)
        {
            orders.Add(request.GetInstaOrdersDailyWorkOrderSqlAndPLOrderUnion(instaOrderID));
        }
        else if (plOrderID > 0)
        {
            orders = request.GetInstaOrdersDailyWorkOrderSqlAndPLOrderUnionByPLOrder(plOrderID);
        }
        else
        {
            string status = gdsOrderStatus;
            bool? isShipped = false;

            if (string.IsNullOrEmpty(status))
            {
                isShipped = null;
            }
            else if (status.Equals("S", StringComparison.OrdinalIgnoreCase))
            {
                status = string.Empty;
                isShipped = true;
            }

            DateTime? orderDateRangeFrom = null;
            DateTime? orderDateRangeTo = null;
            DateTime tmp;
            if (!string.IsNullOrEmpty(orderDateFrom) && DateTime.TryParse(orderDateFrom, out tmp))
                orderDateRangeFrom = tmp;

            if (!string.IsNullOrEmpty(orderDateTo) && DateTime.TryParse(orderDateTo, out tmp))
                orderDateRangeTo = tmp;

            int? orderType = null;
            if (!string.IsNullOrEmpty(gdsInstaOrderType))
                orderType = int.Parse(gdsInstaOrderType);

            orders = request.GetInstaOrdersDaiyWorkOrderSqlUnion(account, location, status, orderDateRangeFrom, orderDateRangeTo, plOrderStatus, orderType, isShipped);
            orders = orders.OrderBy(o => o.InstaOrderID).ToList();
        }

        var odrs = orders.Select(o => new
        {
            InstaOrderID = o.InstaOrderID,
            PLOrderID = o.PLOrderID,
            GDSAccount = o.GDSAccount,
            GDSLocation = o.GDSLocation,
            OrderType = o.GDSInstaOrderType,
            OrderTypeName = o.OrderTypeName,
            MfgRunCode = o.MfgRunCode,
            WearDate = o.WearDate,
            InstadoseOne = o.InstadoseOne,
            Instadose2Elite = o.Instadose2Elite,
            InstadosePlus = o.InstadosePlus,
            Instadose2Plus = o.Instadose2Plus,
            InstaLinkUSB = o.InstaLinkUSB,
            InstaLink = o.InstaLink,
            Status = o.ShipDate == null ? o.GDSInstaOrderStatus : "S",
            StatusName = o.ShipDate == null ? o.OrderStatusName : "Shipped",
            OrderDate = o.OrderDate,
            DailyWorkOrderCount = 1,
            InitScheduleDate = o.InitScheduleDate
        });

        JavaScriptSerializer js = new JavaScriptSerializer();

        return js.Serialize(new { InstaOrders = odrs });
    }

    public string DeleteInstaOrderDetail(int instaOrderID, List<int> instaOrderDetailIDs)
    {
        JavaScriptSerializer js = new JavaScriptSerializer();

        InstadoseOrderRequests orderReq = new InstadoseOrderRequests();

        var order = orderReq.GetGDSInstaOrderDailyWorkOrderSqlUnion(instaOrderID);

        // check whether order exist
        if (order == null)
        {
            return js.Serialize(new
            {
                Success = false,
                ErrorMessage = "Instadose order not found.",
                ErrorDetails = (IEnumerable)null
            });
        }

        // check whether order status is OPEN
        if (order.OrderStatus != "O")
        {
            return js.Serialize(new
            {
                Success = false,
                ErrorMessage = "Only badge of Instadose order with OPEN status can be deleted.",
                ErrorDetails = (IEnumerable)null
            });
        }

        // check whether order is RMA replacement or new/add-on order
        if (order.GDSInstaOrderType == 1 || order.GDSInstaOrderType == 2)
        {
            return js.Serialize(new
            {
                Success = false,
                ErrorMessage = "Device of RMA replacement order can be deleted.",
                ErrorDetails = (IEnumerable)null
            });
        }

        ManufacturingRequests mfgReq = new ManufacturingRequests();

        string errMsg = string.Empty;
        List<KeyValuePair<int, string>> detailErrs = new List<KeyValuePair<int, string>>();
        int responseCd;

        foreach (int detailID in instaOrderDetailIDs)
        {
            try
            {
                if (order.IsFromDailyWorkOrderSql)
                    responseCd = mfgReq.DeleteDailyWorkOrderSqlDetail(instaOrderID, detailID);
                else
                    responseCd = mfgReq.DeleteInstaOrderDetail(instaOrderID, detailID);

                if (responseCd != 0)
                {
                    if (responseCd == 56000)
                        detailErrs.Add(new KeyValuePair<int, string>(detailID, "Badge data is not found in UNIX."));
                    else
                        detailErrs.Add(new KeyValuePair<int, string>(detailID, "Unknown error occured while deleting badge from the order."));
                }
            }
            catch
            {
                detailErrs.Add(new KeyValuePair<int, string>(detailID, "Unknown error occured while deleting badge from the order."));
            }
        }

        return js.Serialize(new
        {
            Success = true,
            ErrorMessage = string.Empty,
            ErrorDetails = detailErrs.Count > 0 ? detailErrs : (IEnumerable)null
        });
    }

    public string UnAssignWearerFromInstaOrderDetail(int instaOrderID, int instaOrderDetailID)
    {
        JavaScriptSerializer js = new JavaScriptSerializer();

        InstadoseOrderRequests orderReq = new InstadoseOrderRequests();

        var order = orderReq.GetGDSInstaOrderDailyWorkOrderSqlUnion(instaOrderID);

        // check whether order exist
        if (order == null)
        {
            return js.Serialize(new
            {
                Success = false,
                ErrorMessage = "Instadose order not found."
            });
        }

        // check whether order status is OPEN
        if (order.OrderStatus != "O")
        {
            return js.Serialize(new
            {
                Success = false,
                ErrorMessage = "Instadose order status is not OPEN."
            });
        }

        bool isSuccess = true;
        string errMsg = string.Empty;

        isSuccess = unAssignWearerFromInstaOrderDetail(instaOrderID, instaOrderDetailID, order.IsFromDailyWorkOrderSql, out errMsg);

        return js.Serialize(new
        {
            Success = isSuccess,
            ErrorMessage = errMsg
        });
    }

    public string AssignWearerToInstaOrderDetail(int instaOrderID, int instaOrderDetailID, string bodyRegion, int wearerID, string initial)
    {
        JavaScriptSerializer js = new JavaScriptSerializer();
        InstadoseOrderRequests orderReq = new InstadoseOrderRequests();

        var order = orderReq.GetGDSInstaOrderDailyWorkOrderSqlUnion(instaOrderID);

        // check whether order exist
        if (order == null)
        {
            return js.Serialize(new
            {
                Success = false,
                ErrorMessage = "Instadose order not found."
            });
        }

        // check whether order status is OPEN
        if (order.OrderStatus != "O")
        {
            return js.Serialize(new
            {
                Success = false,
                ErrorMessage = "Instadose order status is not OPEN."
            });
        }

        // add wearer to Instadose Database
        MigrateRequests migrateRequest = new MigrateRequests();

        int insUserID = 0;
        int insUserResponse = 0;

        try
        {
            insUserResponse = migrateRequest.AddInsUser(order.GDSAccount, order.GDSLocation, wearerID, initial, out insUserID);
        }
        catch
        {
            insUserResponse = -1;
        }

        if (insUserResponse != 0)
        {
            return js.Serialize(new
            {
                Success = false,
                ErrorMessage = "Error occured while make user data to Instadose database."
            });
        }

        // check whether wearer is already assigned to an Instadose badge
        bool? isAssigned = null;
        string errMsg = string.Empty;

        try
        {
            isAssigned = isWearerAlreadyAssigned(order.GDSAccount, order.GDSLocation, wearerID, bodyRegion);
        }
        catch (Exception ex)
        {
            errMsg = ex.Message;
            isAssigned = null;
        }

        if (isAssigned == null)
        {
            // if exception occured while checking wearer
            return js.Serialize(new
            {
                Success = false,
                ErrorMessage = "Error occured while checking whether a wearer is already assigned with an Instadose badge. " + errMsg
            });
        }
        else if (isAssigned == true)
        {
            // if wearer is already assigned to an Instadose badge
            return js.Serialize(new
            {
                Success = false,
                ErrorMessage = "Wearer is already assigned with an Instadose badge."
            });
        }

        ManufacturingRequests mfgReq = new ManufacturingRequests();
        int responseCd = 0;
        bool isSuccess = true;

        try
        {
            if (order.IsFromDailyWorkOrderSql)
                responseCd = mfgReq.AssignWearerToDailyWorkOrderSqlDetail(instaOrderID, instaOrderDetailID, bodyRegion, insUserID, wearerID);
            else
                responseCd = mfgReq.AssignWearerInstaOrderDetail(instaOrderID, instaOrderDetailID, bodyRegion, insUserID, wearerID);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            isSuccess = false;
        }

        if (isSuccess && responseCd == 0)
        {
            isSuccess = true;
        }
        else
        {
            if (responseCd == 5600)
                errMsg = "Badge data is not found in UNIX.";
            else
                errMsg = "Unknown error occured while assigning the wearer to the badge.";
        }

        return js.Serialize(new
        {
            Success = isSuccess,
            ErrorMessage = errMsg
        });
    }

    public string IsWearerAlreadyAssigned(string account, string location, int wearer, string bodyRegion)
    {
        JavaScriptSerializer js = new JavaScriptSerializer();

        if (string.IsNullOrEmpty(account))
        {
            return js.Serialize(new
            {
                Success = false,
                ErrorMessage = "Account is not provided.",
                IsAssigned = false
            });
        }

        if (string.IsNullOrEmpty(location))
        {
            return js.Serialize(new
            {
                Success = false,
                ErrorMessage = "Location is not provided.",
                IsAssigned = false
            });
        }

        if (wearer <= 0)
        {
            return js.Serialize(new
            {
                Success = false,
                ErrorMessage = "Wearer is not provided.",
                IsAssigned = false
            });
        }

        if (string.IsNullOrEmpty(bodyRegion))
        {
            return js.Serialize(new
            {
                Success = false,
                ErrorMessage = "Body Region is not provided.",
                IsAssigned = false
            });
        }

        bool isSuccess = true;
        string errMsg = string.Empty;
        bool isAssigned = false;

        try
        {
            isAssigned = isWearerAlreadyAssigned(account, location, wearer, bodyRegion);
        }
        catch
        {
            isSuccess = false;
            errMsg = "Error occured while checking wearer is already assigned with an Instadose badge.";
        }

        return js.Serialize(new
        {
            Success = isSuccess,
            ErrorMessage = errMsg,
            IsAssigned = isAssigned
        });
    }

    public string IsWearersAlreadyAssigned(string account, string location, List<DeviceAssignCheck> assignInfos)
    {
        JavaScriptSerializer js = new JavaScriptSerializer();

        if (string.IsNullOrEmpty(account))
        {
            return js.Serialize(new
            {
                Success = false,
                ErrorMessage = "Account is not provided.",
                AssignedWearers = (IEnumerable)null
            });
        }

        if (string.IsNullOrEmpty(location))
        {
            return js.Serialize(new
            {
                Success = false,
                ErrorMessage = "Location is not provided.",
                AssignedWearers = (IEnumerable)null
            });
        }

        if (assignInfos == null || assignInfos.Count() < 0)
        {
            return js.Serialize(new
            {
                Success = false,
                ErrorMessage = "Wearer device assignment information is not provided.",
                AssignedWearers = (IEnumerable)null
            });
        }

        List<DeviceAssignCheck> alreadyAssigned = new List<DeviceAssignCheck>();

        foreach (var assignInfo in assignInfos)
        {
            if (isWearerAlreadyAssigned(account, location, assignInfo.WearerID, assignInfo.BodyRegionCd))
                alreadyAssigned.Add(assignInfo);
        }

        return js.Serialize(new
        {
            Success = true,
            ErrorMessage = "",
            AssignedWearers = alreadyAssigned
        });
    }

    public string GetInstadoseBadgeMASSKUs()
    {
        List<Mirion.DSD.GDS.API.Contexts.Badge> badges = BadgeRequests.GetInstadoseBadges();
        JavaScriptSerializer js = new JavaScriptSerializer();

        if (badges != null)
        {
            return js.Serialize(new { BadgeMASSKU = badges.Select(b => new { b.BadgeID, b.GDSToMASSKU }).ToList() });
        }
        else
        {
            return js.Serialize(new { BadgeMASSKU = (IEnumerable)null });
        }
    }

    public string GetInstadoseColorCaseMASSKUs()
    {
        JavaScriptSerializer js = new JavaScriptSerializer();

        BadgeRequests req = new BadgeRequests();

        var badgeHolderColors = req.GetInventoryOrderBadgeHolderColors();

        if (badgeHolderColors != null && badgeHolderColors.Count > 0)
        {
            return js.Serialize(new
            {
                CaseMASSKU = badgeHolderColors.Select(b => new
                {
                    b.BadgeID,
                    b.HolderType,
                    b.Color,
                    b.GDSToMASSKU
                }).ToList()
            });
        }
        else
        {
            return js.Serialize(new
            {
                CaseMASSKU = (IEnumerable)null
            });
        }
    }

    public string GetHDNConstraints(int insAccountID, int insLocationID)
    {
        bool isSuccess = true;
        string errMsg = string.Empty;

        DAFeatures df = new DAFeatures();
        int? locationID = null;

        if (insLocationID > 0)
            locationID = insLocationID;

        HDNConstraints hdnc = null;

        try
        {
            hdnc = df.GetHDNConstraints(insAccountID, locationID, "mrem");
        }
        catch
        {
            isSuccess = false;
            errMsg = "Error occured while getting high dose notification data.";
        }

        var highDoseNotice = hdnc == null ? null : new
        {
            DeepCurrent = hdnc.DeepCurrent,
            DeepQuarterToDate = hdnc.DeepQuarterToDate,
            DeepYearToDate = hdnc.DeepYearToDate,
            ShallowCurrent = hdnc.ShallowCurrent,
            ShallowQuarterToDate = hdnc.ShallowQuarterToDate,
            ShallowYearToDate = hdnc.ShallowYearToDate,
            EyeCurrent = hdnc.EyeCurrent,
            EyeQuarterToDate = hdnc.EyeQuarterToDate,
            EyeYearToDate = hdnc.EyeYearToDate
        };

        JavaScriptSerializer js = new JavaScriptSerializer();

        return js.Serialize(new
        {
            Success = isSuccess,
            ErrorMessage = errMsg,
            HighDoseNotification = highDoseNotice
        });
    }

    public string UpdateHDNConstraints(int insAccountID, HDNConstraints hdnConstraint, string noteInitials = "ins")
    {
        bool isSuccess = true;
        string errMsg = string.Empty;
        int responseCode = 0;

        DAFeatures df = new DAFeatures();

        try
        {
            responseCode = df.SetHDNConstraints(hdnConstraint, insAccountID);
            var idc = new Instadose.Data.InsDataContext();
            var accountToUpdate = idc.Accounts.Where(a => a.AccountID == insAccountID).FirstOrDefault();
            var account = accountToUpdate.GDSAccount;
            var location = "";
            var locationID = hdnConstraint.LocationID;
            if (locationID > 0)
                location = idc.Locations.Where(l => l.LocationID == locationID).Select(l => l.GDSLocation).FirstOrDefault();

            var hdnc = df.GetHDNConstraints(insAccountID, locationID, "mrem");
            var activityCode = "6H"; //Add 
            if (hdnc.DeepQuarterToDate > 0 || hdnc.EyeQuarterToDate > 0 || hdnc.ShallowQuarterToDate > 0 ||
                hdnc.DeepCurrent > 0 || hdnc.EyeCurrent > 0 || hdnc.ShallowCurrent > 0 ||
                hdnc.DeepYearToDate > 0 || hdnc.EyeYearToDate > 0 || hdnc.ShallowYearToDate > 0
                )
            {
                activityCode = "6I";
                if (hdnConstraint.DeepQuarterToDate == 0 && hdnConstraint.EyeQuarterToDate == 0 && hdnConstraint.ShallowQuarterToDate == 0)
                    activityCode = "6J";
                accountToUpdate.HDNNotify = true;
            }
            else
            {
                accountToUpdate.HDNNotify = false;
            }
            idc.SubmitChanges();
            MiscRequests mReq = new MiscRequests();
            var response = mReq.AddAuditTransaction(account, 0, activityCode, noteInitials, "", location);
        }
        catch
        {
            isSuccess = false;
            errMsg = "Error occured while updating high dose notification data.";
            responseCode = -2;
        }

        if (responseCode != 0)
        {
            isSuccess = false;
        }

        JavaScriptSerializer js = new JavaScriptSerializer();

        return js.Serialize(new
        {
            Success = isSuccess,
            ResponseCode = responseCode,
            ErrorMessage = errMsg
        });
    }

    public string GetGDSInstaOrderType()
    {
        bool isSuccess = true;

        InstadoseOrderRequests req = new InstadoseOrderRequests();
        List<Mirion.DSD.GDS.API.Contexts.GDSInstaOrderType> types = new List<Mirion.DSD.GDS.API.Contexts.GDSInstaOrderType>();

        try
        {
            types = req.GetInstaOrderTypes();
        }
        catch
        {
            isSuccess = false;
        }

        var response = new
        {
            Success = isSuccess,
            ErrorMessage = isSuccess ? string.Empty : "Error occured while getting order types for Instadose order",
            InstaOrderTypes = isSuccess ? types.Select(ot => new { ot.OrderTypeID, ot.OrderTypeName }).ToList() : (IEnumerable)null
        };

        JavaScriptSerializer js = new JavaScriptSerializer();

        return js.Serialize(response);
    }

    public string GetAccountProducts(string account, string location, bool activeOnly)
    {
        string errMsg = string.Empty;

        List<AccountProduct> accountProducts = new List<AccountProduct>();

        try
        {
            accountProducts = InstadoseRequests.GetAccountProducts(account, activeOnly, location);
        }
        catch (Exception ex)
        {
            errMsg = ex.Message;
        }

        JavaScriptSerializer js = new JavaScriptSerializer();

        if (accountProducts != null && accountProducts.Count > 0)
        {
            return js.Serialize(new
            {
                ErrorMessage = errMsg,
                Products = accountProducts.Select(ac => new
                {
                    AccountID = ac.AccountID,
                    GDSAccount = ac.Account.GDSAccount,
                    LocationID = ac.LocationID,
                    GDSLocation = ac.Location != null ? ac.Location.GDSLocation : string.Empty,
                    ProductInventoryID = ac.ProductInventoryID,
                    Active = ac.Active,
                    SerialNo = ac.ProductInventory.SerialNo,
                    ProductID = ac.ProductInventory.ProductID,
                    ProductName = ac.ProductInventory.Product.ProductName,
                    ProductSKU = ac.ProductInventory.Product.ProductSKU,
                    ProductActive = ac.ProductInventory.Product.Active,
                    HardwareVersion = ac.ProductInventory.HardwareVersion,
                    EthMacAddress = ac.ProductInventory.EthMacAddress,
                    WifiMacAddress = ac.ProductInventory.WifiMacAddress,
                    HardwareHeartBeatsCount = ac.ProductInventory.HardwareHeartbeats.Count,
                    LastHardwareHeartBeat = ac.ProductInventory.HardwareHeartbeats
                        .OrderByDescending(hb => hb.CreatedDate)
                        .Select(hb => new { hb.HardwareHeartbeatID, hb.CreatedDate, hb.Message, hb.Indicator })
                        .FirstOrDefault(),
                    HardwareLogCount = ac.ProductInventory.HardwareLogs.Count,
                    LastHardwareLog = ac.ProductInventory.HardwareLogs
                        .OrderByDescending(hl => hl.CreatedDate)
                        .Select(hl => new { hl.HardwareLogID, hl.CreatedDate, hl.ErrorMessage })
                        .FirstOrDefault()
                })
            });
        }
        else
        {
            return js.Serialize(new
            {
                ErrorMessage = errMsg,
                Products = (IEnumerable)null
            });
        }
    }

    public string GetAccountProductHardwareInfos(int insAccountID, int productInventoryID, string infoType)
    {
        string errMsg = string.Empty;
        AccountProduct accountProduct = new AccountProduct();

        try
        {
            accountProduct = InstadoseRequests.GetAccountProduct(insAccountID, productInventoryID);
        }
        catch (Exception ex)
        {
            errMsg = string.Format("Error occured while getting data - {0}", ex.Message);
        }

        JavaScriptSerializer js = new JavaScriptSerializer();

        if (string.IsNullOrEmpty(errMsg))
        {
            var hardwareInfos = new object();

            if (infoType.Equals("HeartBeats", StringComparison.OrdinalIgnoreCase))
            {
                // HardwareHeartBeats
                hardwareInfos = accountProduct.ProductInventory.HardwareHeartbeats.OrderByDescending(hhb => hhb.CreatedDate).Select(hhb => new { hhb.HardwareHeartbeatID, hhb.Message, hhb.CreatedDate, hhb.SerialNo, hhb.Indicator }).ToList();
            }
            else if (infoType.Equals("Logs", StringComparison.OrdinalIgnoreCase))
            {
                // HardwareLogs
                hardwareInfos = accountProduct.ProductInventory.HardwareLogs.OrderByDescending(hl => hl.CreatedDate).Select(hl => new { hl.HardwareLogID, hl.ErrorMessage, hl.CreatedDate, hl.SerialNo }).ToList();
            }
            else
            {
                hardwareInfos = (IEnumerable)null;
            }

            return js.Serialize(new
            {
                ErrorMssage = errMsg,
                Account = accountProduct.Account.GDSAccount,
                Location = accountProduct.Location != null ? accountProduct.Location.GDSLocation : string.Empty,
                SerialNo = accountProduct.ProductInventory.SerialNo,
                ProductName = accountProduct.ProductInventory.Product.ProductName,
                HardwareVersion = accountProduct.ProductInventory.HardwareVersion,
                EthMacAddress = accountProduct.ProductInventory.EthMacAddress,
                WifiMacAddress = accountProduct.ProductInventory.WifiMacAddress,
                HardwareInfoType = infoType,
                HardwareInfos = hardwareInfos
            });
        }
        else
        {
            return js.Serialize(new
            {
                ErrorMessage = errMsg,
                HardwareInfoType = infoType,
                HardwareInfos = (IEnumerable)null
            });
        }
    }

    public string GetProductInventoryDetails(int productInventoryID, string infoType)
    {
        string errMsg = string.Empty;

        ProductInventory inventory = new ProductInventory();

        try
        {
            inventory = InstadoseRequests.GetProductInventory(productInventoryID);
        }
        catch (Exception ex)
        {
            errMsg = string.Format("Error occured while getting data - {0}", ex.Message);
        }

        JavaScriptSerializer js = new JavaScriptSerializer();

        if (string.IsNullOrEmpty(errMsg))
        {
            var hardwareInfos = new object();

            if (infoType.Equals("HeartBeats", StringComparison.OrdinalIgnoreCase))
            {
                // HardwareHeartBeats
                hardwareInfos = inventory.HardwareHeartbeats.OrderByDescending(hhb => hhb.CreatedDate).Select(hhb => new { hhb.HardwareHeartbeatID, hhb.Message, hhb.CreatedDate, hhb.SerialNo, hhb.Indicator }).ToList();
            }
            else if (infoType.Equals("Logs", StringComparison.OrdinalIgnoreCase))
            {
                // HardwareLogs
                hardwareInfos = inventory.HardwareLogs.OrderByDescending(hl => hl.CreatedDate).Select(hl => new { hl.HardwareLogID, hl.ErrorMessage, hl.CreatedDate, hl.SerialNo }).ToList();
            }
            else
            {
                hardwareInfos = (IEnumerable)null;
            }

            string gdsAccount = string.Empty;
            string gdsLocation = string.Empty;

            if (inventory.AccountProducts != null)
            {
                var accountProduct = inventory.AccountProducts.OrderByDescending(ap => ap.AssignmentDate).FirstOrDefault();

                if (accountProduct != null)
                {
                    gdsAccount = accountProduct.Account.GDSAccount;

                    if (accountProduct.Location != null)
                        gdsLocation = accountProduct.Location.GDSLocation;
                }
            }

            return js.Serialize(new
            {
                ErrorMssage = errMsg,
                Account = gdsAccount,
                Location = gdsLocation,
                SerialNo = inventory.SerialNo,
                ProductName = inventory.Product.ProductName,
                HardwareVersion = inventory.HardwareVersion,
                EthMacAddress = inventory.EthMacAddress,
                WifiMacAddress = inventory.WifiMacAddress,
                HardwareInfoType = infoType,
                HardwareInfos = hardwareInfos
            });
        }
        else
        {
            return js.Serialize(new
            {
                ErrorMessage = errMsg,
                HardwareInfoType = infoType,
                HardwareInfos = (IEnumerable)null
            });
        }
    }

    public string GetProductInventoryHardwareInfos(string gdsAccount, string gdsLocation, DateTime? heartbeatsFromDate, DateTime? heartBeatsToDate, DateTime? logsFromDate, DateTime? logsToDate)
    {
        string errMsg = string.Empty;

        List<vw_ProductInventoryHardwareInfo> infos = new List<vw_ProductInventoryHardwareInfo>();

        try
        {
            infos = InstadoseRequests.GetProductInventoryHardwareInfos(gdsAccount, gdsLocation, heartbeatsFromDate, heartBeatsToDate, logsFromDate, logsToDate);
        }
        catch
        {
            errMsg = "Error occured while getting Product Inventory data from database.";
        }

        // explicitly change max json length limit
        JavaScriptSerializer js = new JavaScriptSerializer() { MaxJsonLength = Int32.MaxValue };

        if (string.IsNullOrEmpty(errMsg))
        {
            return js.Serialize(new
            {
                ErrorMessage = errMsg,
                Products = infos.Select(i => new
                {
                    AccountID = i.AccountID,
                    GDSAccount = i.GDSAccount,
                    LocationID = i.LocationID,
                    GDSLocation = i.GDSLocation,
                    ProductInventoryID = i.ProductInventoryID,
                    Active = i.AccountProductActive,
                    SerialNo = i.SerialNo,
                    ProductID = i.ProductID,
                    ProductName = i.ProductName,
                    ProductSKU = i.ProductSKU,
                    ProductActive = i.ProductActive,
                    HardwareVersion = i.HardwareVersion,
                    EthMacAddress = i.EthMacAddress,
                    WifiMacAddress = i.WifiMacAddress,
                    HardwareHeartBeatsCount = i.HardwareHeartbeatsCount,
                    LastHardwareHeartBeat = new
                    {
                        HardwareHeartbeatID = i.HardwareHeartbeatID,
                        CreatedDate = i.HardwareHeartbeatCreatedDate,
                        Message = i.HardwareHeartbeatMessage,
                        Indicator = i.Indicator
                    },
                    HardwareLogCount = i.HardwareLogsCount,
                    LastHardwareLog = new
                    {
                        HardwareLogID = i.HardwareLogID,
                        CreatedDate = i.HardwareLogCreatedDate,
                        ErrorMessage = i.HardwareLogErrorMessage
                    }
                })
            });
        }
        else
        {
            return js.Serialize(new
            {
                ErrorMessage = errMsg,
                Products = (IEnumerable)null
            });
        }
    }

    public string GetProductInventoryHardwareInfosBySerialNo(string serialNo)
    {
        string errMsg = string.Empty;

        List<vw_ProductInventoryHardwareInfo> infos = new List<vw_ProductInventoryHardwareInfo>();

        try
        {
            infos = InstadoseRequests.GetProductInventoryHardwareInfosBySerialNo(serialNo);
        }
        catch (Exception ex)
        {
            errMsg = ex.Message;
        }

        JavaScriptSerializer js = new JavaScriptSerializer();

        if (string.IsNullOrEmpty(errMsg))
        {
            return js.Serialize(new
            {
                ErrorMessage = errMsg,
                Products = infos.Select(i => new
                {
                    AccountID = i.AccountID,
                    GDSAccount = i.GDSAccount,
                    LocationID = i.LocationID,
                    GDSLocation = i.GDSLocation,
                    ProductInventoryID = i.ProductInventoryID,
                    Active = i.AccountProductActive,
                    SerialNo = i.SerialNo,
                    ProductID = i.ProductID,
                    ProductName = i.ProductName,
                    ProductSKU = i.ProductSKU,
                    ProductActive = i.ProductActive,
                    HardwareVersion = i.HardwareVersion,
                    EthMacAddress = i.EthMacAddress,
                    WifiMacAddress = i.WifiMacAddress,
                    HardwareHeartBeatsCount = i.HardwareHeartbeatsCount,
                    LastHardwareHeartBeat = new
                    {
                        HardwareHeartbeatID = i.HardwareHeartbeatID,
                        CreatedDate = i.HardwareHeartbeatCreatedDate,
                        Message = i.HardwareHeartbeatMessage,
                        Indicator = i.Indicator
                    },
                    HardwareLogCount = i.HardwareLogsCount,
                    LastHardwareLog = new
                    {
                        HardwareLogID = i.HardwareLogID,
                        CreatedDate = i.HardwareLogCreatedDate,
                        ErrorMessage = i.HardwareLogErrorMessage
                    }
                })
            });
        }
        else
        {
            return js.Serialize(new
            {
                ErrorMessage = errMsg,
                Products = (IEnumerable)null
            });
        }
    }

    public string GetHardwareHeartbeatsByDateRange(DateTime? startDate, DateTime? endDate, List<string> indicators)
    {
        string errMsg = string.Empty;

        List<HardwareHeartbeat> heartBeats = new List<HardwareHeartbeat>();

        try
        {
            heartBeats = InstadoseRequests.GetHardwareHeartbeatsByDateRange(startDate, endDate, indicators);
        }
        catch (Exception ex)
        {
            errMsg = ex.Message;
        }

        JavaScriptSerializer js = new JavaScriptSerializer();

        if (string.IsNullOrEmpty(errMsg))
        {
            return js.Serialize(new
            {
                ErrorMessage = errMsg,
                Heartbeats = heartBeats.Select(hb => new
                {
                    HardwareHeartbeatID = hb.HardwareHeartbeatID,
                    ProductInventoryID = hb.ProductInventoryID,
                    GDSAccount = hb.ProductInventory.AccountProducts.Select(ap => ap.Account).FirstOrDefault().GDSAccount,
                    AccountID = hb.ProductInventory.AccountProducts.Select(ap => ap.AccountID).FirstOrDefault(),
                    GDSLocation = hb.ProductInventory.AccountProducts.Select(ap => ap.Location).FirstOrDefault().GDSLocation,
                    LocationID = hb.ProductInventory.AccountProducts.Select(ap => ap.LocationID).FirstOrDefault(),
                    ProductID = hb.ProductInventory.Product.ProductID,
                    ProductName = hb.ProductInventory.Product.ProductName,
                    ProductSKU = hb.ProductInventory.Product.ProductSKU,
                    Message = hb.Message,
                    CreatedDate = hb.CreatedDate,
                    SerialNo = hb.SerialNo,
                    Indicator = hb.Indicator
                })
            });
        }
        else
        {
            return js.Serialize(new
            {
                ErrorMessage = errMsg,
                Heartbeats = (IEnumerable)null
            });
        }
    }

    public string IsOpenInstaOrderExist(string account, string location)
    {
        InstadoseOrderRequests req = new InstadoseOrderRequests();

        List<int> openedOrderIDs = req.GetAllOpenInstaOrderIDs(account, location);

        bool isExist = false;
        string errMsg = string.Empty;

        try
        {
            isExist = openedOrderIDs != null && openedOrderIDs.Count > 0;
        }
        catch
        {
            //errMsg = ex.Message;
            errMsg = "Error occured while getting open Instadose order number.";
        }

        JavaScriptSerializer js = new JavaScriptSerializer();

        return js.Serialize(new
        {
            IsExist = isExist,
            OpenedInstaOrderIDs = openedOrderIDs,
            ErrorMessage = errMsg
        });
    }

    public string GetDailyWorkOrder(int instaOrderID)
    {
        JavaScriptSerializer js = new JavaScriptSerializer();

        if (instaOrderID <= 0)
        {
            return js.Serialize(new
            {
                Success = false,
                ErrorMessage = "Instadose Order ID is not valid.",
                DailyWorkOrder = (IEnumerable)null
            });
        }

        bool isSuccess = true;

        InstadoseOrderRequests request = new InstadoseOrderRequests();
        DailyWorkOrder workOrder = new DailyWorkOrder();

        // get DailyWorkOrder data for the order
        try
        {
            workOrder = request.GetDailyWorkOrder(instaOrderID);
        }
        catch
        {
            isSuccess = false;
        }

        // if error occured while getting data, return error
        if (!isSuccess)
        {
            return js.Serialize(new
            {
                Success = false,
                ErrorMessage = "Error occured while getting Daily for the order.",
                DailyWorkOrder = (IEnumerable)null
            });
        }

        // if DailyWorkOrder is not found, return error
        if (workOrder == null)
        {
            return js.Serialize(new
            {
                Success = false,
                ErrorMessage = "Daily is not found.",
                DailyWorkOrder = (IEnumerable)null
            });
        }

        return js.Serialize(new
        {
            Success = true,
            ErrorMessage = "",
            DailyWorkOrder = workOrder
        });
    }

    public string GetAccountInstdosePlusDevices(string account, string location)
    {
        JavaScriptSerializer js = new JavaScriptSerializer();

        if (string.IsNullOrEmpty(account))
            return js.Serialize(new { Success = false, ErrorMessage = "Account is not valid." });

        if (string.IsNullOrEmpty(location))
            return js.Serialize(new { Success = false, ErrorMessage = "Location is not valid." });

        bool isSuccess = true;
        string errMsg = string.Empty;

        BadgeRequests request = new BadgeRequests();

        List<GBadgeSearchListItem> badges = new List<GBadgeSearchListItem>();

        try
        {
            badges = request.SearchInstadoseBadges(account, location, null, "", "", null, false);
        }
        catch (Exception ex)
        {
            isSuccess = false;
            errMsg = ex.Message;
        }

        // filter Instadose Plus devices and pending badges
        if (badges != null && badges.Count > 0)
            badges = badges.Where(b => b.SerialNo != "Pending").ToList();

        return js.Serialize(new
        {
            Success = isSuccess,
            ErrorMessage = errMsg,
            Badges = badges.Select(b => new
            {
                b.Account,
                b.InsAccountID,
                b.Location,
                b.InsLocationID,
                b.SerialNo,
                b.DeviceID,
                b.Color,
                b.BT,
                b.WearerID,
                b.InsUserID,
                b.FirstName,
                b.LastName,
                b.BodyRegion,
                //b.BodyRegionID,
                b.BadgeTypeDescription,
                //b.AssignedWearerID
            })
        });
    }
    public string ProcessRMALostDevices(string account, string location, List<BadgeRequests.ReturnDevice> devices, string initial)
    {
        JavaScriptSerializer js = new JavaScriptSerializer();

        if (string.IsNullOrEmpty(account))
            return js.Serialize(new { Success = false, ErrorMessage = "Account is not valid.", ErrorDevices = (IEnumerable)null });

        int insAccountID = InstadoseRequests.GetInsAccountID(account);

        if (insAccountID <= 0)
            return js.Serialize(new { Success = false, ErrorMessage = "Account is not found.", ErrorDevices = (IEnumerable)null });

        if (string.IsNullOrEmpty(location))
            return js.Serialize(new { Success = false, ErrorMessage = "Location is not valid.", ErrorDevices = (IEnumerable)null });

        if (devices == null || devices.Count <= 0)
            return js.Serialize(new { Success = false, ErrorMessage = "Devices are not given.", ErrorDevices = (IEnumerable)null });

        BadgeRequests badgeReq = new BadgeRequests();

        Dictionary<BadgeRequests.ReturnDevice, string> errors = new Dictionary<BadgeRequests.ReturnDevice, string>();

        List<BadgeRequests.RMAReturnDevice> successes = badgeReq.DeactivateAccountDevices(account, devices, initial, out errors);

        int rmaReturnID = 0;

        if (successes != null && successes.Count > 0)
        {
            // ReturnReasonID 16 - Lost
            // ReturnTypeID 9 - Lost
            rmaReturnID = badgeReq.CreateRMA(insAccountID, successes, "Lost", "Lost", 16, 9, initial, 0, account, location);
        }

        return js.Serialize(new
        {
            Success = errors == null || errors.Count <= 0,
            ErrorMessage = "",
            ErrorDevices = errors,
            RMAReturnID = rmaReturnID
        });
    }

    public string ProcessRMARecallDevices(string account, string location, List<BadgeRequests.ReturnDevice> devices, string initial, string notes, string reason, int returnReasonID)
    {
        JavaScriptSerializer js = new JavaScriptSerializer();

        if (string.IsNullOrEmpty(account))
            return js.Serialize(new { Success = false, ErrorMessage = "Account is not valid.", ErrorDevices = (IEnumerable)null });

        int insAccountID = InstadoseRequests.GetInsAccountID(account);

        if (insAccountID <= 0)
            return js.Serialize(new { Success = false, ErrorMessage = "Account is not found.", ErrorDevices = (IEnumerable)null });

        if (string.IsNullOrEmpty(location))
            return js.Serialize(new { Success = false, ErrorMessage = "Location is not valid.", ErrorDevices = (IEnumerable)null });

        if (devices == null || devices.Count <= 0)
            return js.Serialize(new { Success = false, ErrorMessage = "Devices are not given.", ErrorDevices = (IEnumerable)null });

        BadgeRequests badgeReq = new BadgeRequests();

        List<BadgeRequests.RMAReturnDevice> rmaDevices = new List<BadgeRequests.RMAReturnDevice>();

        Dictionary<BadgeRequests.ReturnDevice, string> errors = new Dictionary<BadgeRequests.ReturnDevice, string>();

        foreach (var device in devices)
        {
            var idc = new Instadose.Data.InsDataContext();
            var deviceInfo = idc.DeviceInventories.Where(d => d.SerialNo == device.SerialNo).Select(d => d).FirstOrDefault();
            var insUserID = deviceInfo.UserDevices.Where(ud => ud.Active).Select(ud => ud.UserID).FirstOrDefault();

            if (deviceInfo == null)
            {
                errors.Add(device, "DeviceID not found by serial number and wearer number.");
            }
            else
            {
                rmaDevices.Add(new BadgeRequests.RMAReturnDevice()
                {
                    WearerID = device.WearerID,
                    SerialNo = device.SerialNo,
                    InsUserID = insUserID,
                    DeviceID = deviceInfo.DeviceID
                });
            }
        }

        if (errors.Count > 0)
            return js.Serialize(new { Success = false, ErrorMessage = "", ErrorDevices = errors });

        bool isSuccess = true;
        string errMsg = string.Empty;

        int rmaReturnID = 0;

        // ReturnTypeID 3 - Recall
        rmaReturnID = badgeReq.CreateRMA(insAccountID, rmaDevices, notes, reason, returnReasonID, 3, initial, 0, account, location);

        return js.Serialize(new
        {
            Success = isSuccess,
            ErrorMessage = errMsg,
            ErrorDevices = (IEnumerable)null,
            RMAReturnID = rmaReturnID
        });
    }

    public string GetRMAReturnReasons()
    {
        InstadoseRMARequests req = new InstadoseRMARequests();

        // Customer Service DepartmentID is 2
        List<rma_ref_ReturnReason> reasons = req.GetRMAReturnReasons(2);

        // filter out "Lost", "Cust no longer requires service", and "Others" from Return Reasons
        reasons = reasons.Where(r => r.ReasonID != 16 && r.ReasonID != 15 && r.ReasonID != 6).OrderBy(r => r.Description).ToList();

        JavaScriptSerializer js = new JavaScriptSerializer();

        return js.Serialize(new { ReturnReasons = reasons.Select(r => new { r.ReasonID, r.Description }) });
    }

    public string IsCalendarExist(string account, string location)
    {
        JavaScriptSerializer js = new JavaScriptSerializer();

        if (string.IsNullOrEmpty(account))
        {
            return js.Serialize(new
            {
                Success = false,
                IsExist = false,
                ErrorMessage = "Account it not given."
            });
        }

        if (string.IsNullOrEmpty(location))
        {
            return js.Serialize(new
            {
                Success = false,
                IsExist = false,
                ErrorMessage = "Location is not given."
            });
        }

        InstadoseOrderRequests req = new InstadoseOrderRequests();

        bool isActiveCalendarExist = false;
        string errMsg = string.Empty;

        try
        {
            isActiveCalendarExist = req.IsCalendarExist(account, location);
        }
        catch (Exception ex)
        {
            errMsg = ex.Message;
        }

        return js.Serialize(new
        {
            Success = true,
            ErrorMessage = errMsg,
            IsExist = isActiveCalendarExist
        });
    }

    /// <summary>
    /// Add Instadose Order. If return value is -1, there is an error while processing. If return value is positive int, return value is InstaOrderID.
    /// </summary>
    /// <param name="order"></param>
    /// <param name="orderDetail"></param>
    /// <param name="errorResponseCode"></param>
    /// <returns></returns>
    private int addInstadoseOrder(InstadoseOrderItem order, IList<InstadoseOrderDetailItem> orderDetails, out int errorResponseCode)
    {
        ManufacturingRequests mfgReq = new ManufacturingRequests();

        // AddInstadoseOrder response code: negative response code -> orderID, positive response code -> error code
        int responseCd = mfgReq.AddDailyWorkOrderSql(order, orderDetails, true);

        errorResponseCode = 0;
        return responseCd;
    }

    public string IsAnyOrderExist(string account, string location)
    {
        JavaScriptSerializer js = new JavaScriptSerializer();

        if (string.IsNullOrEmpty(account))
        {
            return js.Serialize(new
            {
                Success = false,
                IsExist = false,
                ErrorMessage = "Account it not given."
            });
        }

        if (string.IsNullOrEmpty(location))
        {
            return js.Serialize(new
            {
                Success = false,
                IsExist = false,
                ErrorMessage = "Location is not given."
            });
        }

        bool isSuccess = true;
        bool isExist = true;
        string errMsg = string.Empty;

        InstadoseOrderRequests orderReq = new InstadoseOrderRequests();

        try
        {
            //isExist = AccountRequests.IsOrderExist(account);
            isExist = orderReq.IsAnyInstaOrderExist(account, location);
        }
        catch (Exception ex)
        {
            isSuccess = false;
            isExist = false;
            errMsg = ex.Message;
        }

        return js.Serialize(new
        {
            Success = isSuccess,
            IsExist = isExist,
            ErrorMessage = errMsg
        });
    }

    public string GetAssignedAccountProductGroupQuantities(string account)
    {
        bool isSuccess = true;
        string errMsg = string.Empty;
        List<ChildAccountProductGroupQty> childrenAccountProductQty = new List<ChildAccountProductGroupQty>();

        AccountRequests req = new AccountRequests();

        Dictionary<string, string> children = req.GetChildrenAccountCompanyName(account);

        JavaScriptSerializer js = new JavaScriptSerializer();

        if (children == null || children.Count <= 0)
        {
            return js.Serialize(new
            {
                Success = isSuccess,
                ErrorMessage = errMsg,
                AssignedAccountProductGroupQuantities = childrenAccountProductQty
            });
        }

        InsDataContext idc = new InsDataContext();

        try
        {
            List<vw_AccountProductGroupQuantity> productGroupQty = idc.vw_AccountProductGroupQuantities.Where(pg => children.Select(c => c.Key).ToList().Contains(pg.GDSAccount) && pg.Active).ToList();

            foreach (var child in children)
            {
                int IDPlusQty = productGroupQty.Where(q => q.GDSAccount == child.Key && q.ProductGroupID == 10).Select(q => q.Quantity).FirstOrDefault() ?? 0;
                int InstaLinkQty = productGroupQty.Where(q => q.GDSAccount == child.Key && q.ProductGroupID == 5).Select(q => q.Quantity).FirstOrDefault() ?? 0;
                int InstaLinkUSBQty = productGroupQty.Where(q => q.GDSAccount == child.Key && q.ProductGroupID == 6).Select(q => q.Quantity).FirstOrDefault() ?? 0;
                int ID2Qty = productGroupQty.Where(q => q.GDSAccount == child.Key && q.ProductGroupID == 11).Select(q => q.Quantity).FirstOrDefault() ?? 0;

                childrenAccountProductQty.Add(new ChildAccountProductGroupQty
                {
                    AccountID = null,
                    GDSAccount = child.Key,
                    CompanyName = child.Value,
                    IDPlusQty = IDPlusQty,
                    InstaLinkQty = InstaLinkQty,
                    InstaLinkUSBQty = InstaLinkUSBQty,
                    ID2Qty = ID2Qty
                });
            }
        }
        catch (Exception ex)
        {
            isSuccess = false;
            errMsg = ex.Message;
        }

        return js.Serialize(new
        {
            Success = isSuccess,
            ErrorMessage = errMsg,
            AssignedAccountProductGroupQuantities = childrenAccountProductQty
        });
    }

    public string GetAvailableConsignmentAccountBadgeQuantities(string account, List<int> badgeTypes)
    {
        List<BadgeRequests.ConsinmentAccountInventoryInfo> inventoryInfos = new List<BadgeRequests.ConsinmentAccountInventoryInfo>();
        JavaScriptSerializer js = new JavaScriptSerializer();

        if (string.IsNullOrEmpty(account))
        {
            return js.Serialize(new
            {
                Success = false,
                InventoryInfos = inventoryInfos,
                ErrorMessage = "Account is not given."
            });
        }

        if (badgeTypes == null || badgeTypes.Count() <= 0)
        {
            return js.Serialize(new
            {
                Success = false,
                InventoryInfos = inventoryInfos,
                ErrorMessage = "Badge Types are not given."
            });
        }

        BadgeRequests req = new BadgeRequests();

        foreach (int badgeType in badgeTypes)
        {
            inventoryInfos.Add(new BadgeRequests.ConsinmentAccountInventoryInfo()
            {
                BadgeType = badgeType,
                HolderType = badgeType == 33 || badgeType == 34 ? null : "IN",
                AvailableQuantity = 0
            });
        }

        bool isSuccess = true;
        string errMsg = string.Empty;

        try
        {
            req.GetAvailableConsignmentAccountBadgeQuantities(account, ref inventoryInfos);
        }
        catch (Exception e)
        {
            isSuccess = false;
            errMsg = e.Message;
        }

        return js.Serialize(new
        {
            Success = isSuccess,
            InventoryInfos = inventoryInfos,
            ErrorMessage = errMsg
        });
    }
    public string GetConsignmentOrderAvailableWearDate(string account, string location, int orderType)
    {
        JavaScriptSerializer js = new JavaScriptSerializer();
        InstadoseOrderRequests request = new InstadoseOrderRequests();

        DateTime availableDt = DateTime.Now;

        try
        {
            availableDt = request.GetConsignmentOrderAvailableWearDate(account, location, orderType);
        }
        catch
        {

        }

        return js.Serialize(new
        {
            AvailableDate = availableDt.ToShortDateString()
        });
    }

    /// <summary>
    /// Generate 'details' string for dl4 call. And insert new Instadose User to Instadose database, if user does not exist in Instadose database.
    /// </summary>
    /// <param name="account">GDSAccount</param>
    /// <param name="location">GDSLocation</param>
    /// <param name="initial"></param>
    /// <param name="insAccountID">Instadose Account ID</param>
    /// <param name="insLocationID">Instadose Location ID</param>
    /// <param name="orderDetails"></param>
    /// <param name="errors"></param>
    /// <returns>'details' string for dl4 call</returns>
    //private string generateInstadoseOrderDetailString(string account, string location, string initial, int insAccountID, int insLocationID, List<InstadoseOrderDetailItem> orderDetails, ref List<AddInstadoseWearerError> errors)
    private string generateInstadoseOrderDetailString(List<InstadoseOrderDetailItem> orderDetails)
    {
        string details = string.Empty;
        MigrateRequests migrateRequest = new MigrateRequests();
        List<AddInstadoseWearerError> wearerErrors = new List<AddInstadoseWearerError>();

        // for non device badges like Color cab, bumper, case, label roll, label sheets, and etc...
        List<string> nonDeviceBadgeTypes = new List<string>();
        BadgeRequests badgeReq = new BadgeRequests();
        List<int> nonDeviceBadgeTypeIDs = badgeReq.GetNotDeviceBadgeIDs();

        if (nonDeviceBadgeTypeIDs != null && nonDeviceBadgeTypeIDs.Count > 0)
        {
            foreach (var badgeTypeID in nonDeviceBadgeTypeIDs)
            {
                nonDeviceBadgeTypes.Add(badgeTypeID.ToString());
            }
        }

        foreach (InstadoseOrderDetailItem orderDetail in orderDetails)
        {
            // string format - WearerNo,BadgeType,BodyRegion,InsUserID,SerialNo,ICCPO,SKU,Color,Quantity
            StringBuilder sb = new StringBuilder();
            sb.Append(orderDetail.WearerNo == null ? string.Empty : orderDetail.WearerNo.ToString());
            sb.Append("," + orderDetail.BadgeType + "," + orderDetail.BodyRegion + ",");
            //sb.Append(userID == 0 ? string.Empty : userID.ToString());
            sb.Append(orderDetail.UserID == null || orderDetail.UserID == 0 ? string.Empty : orderDetail.UserID.ToString());
            sb.Append("," + orderDetail.SerialNo + "," + orderDetail.ICCPO);

            string sku = string.Empty;

            if (string.IsNullOrEmpty(orderDetail.SKU))
            {
                if (orderDetail.BadgeType == "39" || orderDetail.BadgeType == "40")
                {
                    sku = getColorCaseMASSKUByBadgeTypeColor(orderDetail.BadgeType, orderDetail.Color);
                }
                else
                {
                    sku = getBadgeMASSKUByBadgeType(orderDetail.BadgeType);
                }
            }
            else
            {
                sku = orderDetail.SKU;
            }

            sb.Append("," + sku.Replace("+", "%2B"));

            sb.Append("," + orderDetail.Color + "," + orderDetail.Qty);

            if (nonDeviceBadgeTypes.Contains(orderDetail.BadgeType))
            {
                sb.Append(",0");    // apend 0 for non device type like color cap, case, label roll, and etc...
            }
            else
            {
                sb.Append(",1");    // append 1 for actual device
            }

            details += string.IsNullOrEmpty(details) ? sb.ToString() : "|" + sb.ToString();
        }

        return details;
    }

    private void migrateUsersByInstaOrder(string account, string location, string initial, ref List<InstadoseOrderDetailItem> orderDetails, ref List<AddInstadoseWearerError> errors, int insAccountID = 0, int insLocationID = 0)
    {
        // insert User to Instadose database
        if (orderDetails != null && orderDetails.Count > 0)
        {
            MigrateRequests migrateRequest = new MigrateRequests();

            int insUserResponse = 0;
            int insUserID = 0;

            for (int i = 0; i < orderDetails.Count; i++)
            {
                if (orderDetails[i].WearerNo != null && (orderDetails[i].UserID == null || orderDetails[i].UserID <= 0))
                {
                    insUserResponse = migrateRequest.AddInsUser(account, location, (int)orderDetails[i].WearerNo, initial, out insUserID, insAccountID, insLocationID);

                    if (insUserResponse == 0)
                    {
                        orderDetails[i].UserID = insUserID;
                    }
                    else
                    {
                        errors.Add(new AddInstadoseWearerError()
                        {
                            WearerNo = (int)orderDetails[i].WearerNo,
                            ResponseCode = insUserResponse,
                            ErrorMessage = HError.GetError(insUserResponse)
                        });
                    }
                }
            }
        }
    }


    private List<Mirion.DSD.GDS.API.Contexts.Badge> _badges;
    private List<Mirion.DSD.GDS.API.Contexts.BadgeHolderColor> _badgeHolderColors;

    private string getBadgeMASSKUByBadgeType(string badgeType)
    {
        if (_badges == null || _badges.Count <= 0)
            _badges = BadgeRequests.GetInstadoseBadges();

        if (_badges != null && _badges.Count > 0)
            return _badges.Where(b => b.BadgeType == badgeType).Select(b => b.GDSToMASSKU).FirstOrDefault(); //return _badges.SingleOrDefault(b => b.BadgeType == badgeType).GDSToMASSKU;
        else
            return string.Empty;
    }

    public string getColorCaseMASSKUByBadgeTypeColor(string badgeType, string color)
    {
        BadgeRequests req = new BadgeRequests();

        if (_badgeHolderColors == null || _badgeHolderColors.Count <= 0)
            _badgeHolderColors = req.GetInventoryOrderBadgeHolderColors();

        if (_badgeHolderColors != null && _badgeHolderColors.Count > 0)
        {
            return _badgeHolderColors.Where(b => b.BadgeID.ToString() == badgeType && b.Color == color).Select(b => b.GDSToMASSKU).FirstOrDefault();
        }
        else
        {
            return string.Empty;
        }
    }

    private bool unAssignWearerFromInstaOrderDetail(int instaOrderID, int instaOrderDetailID, bool isSql)
    {
        string errMsg;
        return unAssignWearerFromInstaOrderDetail(instaOrderID, instaOrderDetailID, isSql, out errMsg);
    }

    private bool unAssignWearerFromInstaOrderDetail(int instaOrderID, int instaOrderDetailID, bool isSql, out string errorMessage)
    {
        bool isSuccess = true;
        int responseCode = -1;
        string errMsg = string.Empty;

        ManufacturingRequests mfgReq = new ManufacturingRequests();

        try
        {
            if (isSql)
                responseCode = mfgReq.UnAssignWearerToDailyWorkOrderSqlDetail(instaOrderID, instaOrderDetailID);
            else
                responseCode = mfgReq.UnAssignWearerInstaOrderDetail(instaOrderID, instaOrderDetailID);
        }
        catch
        {
            isSuccess = false;
        }

        if (!isSuccess)
        {
            errMsg = "Unknown error occured while un-assigning the wearer from the badge.";
        }
        else
        {
            if (responseCode != 0)
            {
                isSuccess = false;
                errorMessage = responseCode == 56000 ? "Badge data is not found in UNIX." : "Unknown error occured while un-assigning the wearer from the badge.";
            }
        }

        errorMessage = errMsg;
        return isSuccess;
    }

    private bool isAccountActive(string account)
    {
        AccountRequests req = new AccountRequests();

        return req.IsAccountActive(account);
    }

    private bool isLocationActive(string account, string location)
    {
        LocationRequests req = new LocationRequests();

        return req.IsLocationActive(account, location);
    }

    private bool isWearerActive(string account, string location, int wearerID)
    {
        WearerRequests req = new WearerRequests();

        return req.IsWearerActive(account, location, wearerID);
    }

    private bool isWearerAlreadyAssigned(string account, string location, int wearerID, string bodyRegion)
    {
        if (string.IsNullOrEmpty(account))
            throw new Exception("Account is not provided.");

        if (string.IsNullOrEmpty(location))
            throw new Exception("Location is not provided.");

        if (wearerID <= 0)
            throw new Exception("Wearer ID is not valid.");

        if (string.IsNullOrEmpty(bodyRegion))
            throw new Exception("Body Region is not provided.");

        InstadoseOrderRequests req = new InstadoseOrderRequests();

        return req.IsWearerAssigned(account, location, wearerID, bodyRegion);
    }

    private bool isPendingOrderDetailsByWearer(string account, string location, int wearerID, string bodyRegion)
    {
        if (string.IsNullOrEmpty(account))
            throw new Exception("Account is not provided.");

        if (string.IsNullOrEmpty(location))
            throw new Exception("Location is not provided.");

        if (wearerID <= 0)
            throw new Exception("Wearer ID is not valid.");

        if (string.IsNullOrEmpty(bodyRegion))
            throw new Exception("Body Region is not provided.");

        InstadoseOrderRequests req = new InstadoseOrderRequests();
        List<GDSInstaOrderDetail> pendingList = req.GetPendingOrderDetailsByWearer(account, location, wearerID, bodyRegion);

        if (pendingList != null && pendingList.Count > 0)
            return true;
        else
            return false;
    }

    #region InstadoseMaintenance

    private List<AccountDevice> GetAccountDevices(string account = "", string serialNo = "")
    {
        var idc = new Instadose.Data.InsDataContext();
        List<AccountDevice> devices = new List<AccountDevice>();
        if (!string.IsNullOrEmpty(account))
        {
            devices = (from a in idc.AccountDevices
                       where a.Account.GDSAccount == account
                       select a).ToList();
        }
        else if (!string.IsNullOrEmpty(serialNo))
        {
            devices = (from a in idc.AccountDevices
                       where a.DeviceInventory.SerialNo == serialNo
                       select a).ToList();
        }

        return devices;
    }
    private DeviceInventory GetDeviceInventoryInfo(string serialNo)
    {
        var idc = new Instadose.Data.InsDataContext();
        var device = (from d in idc.DeviceInventories
                      where d.SerialNo == serialNo
                      select d).FirstOrDefault();

        return device;
    }
    private List<RawDeviceRead> GetRawDeviceReads(string serialNo)
    {
        var idc = new Instadose.Data.InsDataContext();
        var reads = (from r in idc.RawDeviceReads
                     orderby r.RawReadID
                     where r.SerialNo == serialNo
                     select r).ToList();
        return reads;
    }
    private List<DeviceRead> GetUserDeviceReads(string serialNo)
    {
        var idc = new Instadose.Data.InsDataContext();
        var reads = (from u in idc.UserDeviceReads
                     where u.DeviceInventory.SerialNo == serialNo
                     select new DeviceRead()
                     {
                         RID = u.RID,
                         ReadTypeName = u.ReadType.ReadTypeName,
                         SequenceNo = u.SequenceNo,
                         CreatedDate = u.CreatedDate,
                         BodyRegion = u.BodyRegion.BodyRegionName,
                         DeepDose = u.DeepDose,
                         ShallowDose = u.ShallowDose,
                         EyeDose = u.EyeDose,
                         InitRead = u.InitRead,
                         HasAnomaly = u.HasAnomaly
                     }).ToList();

        return reads;
    }
    private List<DeviceAssignment> GetUserDevices(string account, string serialNo)
    {
        var idc = new Instadose.Data.InsDataContext();
        var devices = (from d in idc.UserDevices
                       where d.DeviceInventory.SerialNo == serialNo && d.User.Account.GDSAccount == account
                       select new DeviceAssignment()
                       {
                           Active = d.Active,
                           UserID = d.UserID,
                           AssignmentDate = d.AssignmentDate,
                           BodyRegion = d.BodyRegion.BodyRegionName,
                           DeactivateDate = d.DeactivateDate,
                           DeactivationReason = d.DeactivationReason
                       }).ToList();
        return devices;
    }
    public string GetInstadoseAccountDevices(string account = "", string serialNo = "")
    {
        var devices = GetAccountDevices(account, serialNo);
        var d = devices.Select(ad => new
        {
            Account = ad.Account.GDSAccount,
            Location = ad.Location.GDSLocation,
            Active = ad.Active ? "Yes" : "No",
            CurrentDeviceAssign = ad.CurrentDeviceAssign ? "Yes" : "No",
            SerialNo = ad.DeviceInventory.SerialNo,
            Assigned = ad.AssignmentDate.ToShortDateString(),
            ServiceStartDate = ad.ServiceStartDate.HasValue ? ad.ServiceStartDate.Value.ToShortDateString() : "",
            DeactivateDate = ad.DeactivateDate.HasValue ? ad.DeactivateDate.Value.ToShortDateString() : ""
        });

        return JsonConvert.SerializeObject(d);
    }
    public string GetDeviceInfo(string account, string serialNo)
    {
        var rawDeviceReads = GetRawDeviceReads(serialNo).OrderByDescending(r => r.RawReadID).Take(10).ToList();
        var userDevices = GetUserDevices(account, serialNo);
        var userDeviceReads = GetUserDeviceReads(serialNo).Take(10).ToList();
        var ud = (from u in userDevices
                  orderby u.DeactivateDate
                  select new
                  {
                      u.Active,
                      u.UserID,
                      u.AssignmentDate,
                      u.BodyRegion,
                      u.DeactivateDate,
                      u.DeactivationReason
                  }).ToList();
        Dictionary<string, object> response = new Dictionary<string, object>
            {
                //response.Add("DeviceInfo", deviceInfo);
                { "UserDevices", ud },
                { "RawDeviceReads", rawDeviceReads },
                { "UserDeviceReads", userDeviceReads }
            };
        return JsonConvert.SerializeObject(response);
    }

    #endregion
}

public class DeviceRead
{
    public int RID { get; set; }
    public string ReadTypeName { get; set; }
    public int? SequenceNo { get; set; }
    public DateTime CreatedDate { get; set; }
    public string BodyRegion { get; set; }
    public double? DeepDose { get; set; }
    public double? ShallowDose { get; set; }
    public double? EyeDose { get; set; }
    public bool InitRead { get; set; }
    public bool HasAnomaly { get; set; }
}
public class DeviceAssignment
{
    public bool Active { get; set; }
    public int UserID { get; set; }
    public DateTime AssignmentDate { get; set; }
    public string BodyRegion { get; set; }
    public DateTime? DeactivateDate { get; set; }
    public string DeactivationReason { get; set; }
}
public class AppointmentInfo
{
    private readonly string _id;
    private DateTime _start;

    public string ID
    {
        get { return _id; }
    }

    public string Subject
    {
        get { return string.Format("Read @ {0:h:mm tt}", _start); }
        set { string bla = value; }
    }

    public DateTime Start
    {
        get { return _start; }
        set { _start = value; }
    }

    public DateTime End
    {
        get { return _start; }
        set { _start = value; }
    }

    private AppointmentInfo()
    {
        _id = Guid.NewGuid().ToString();
    }

    public AppointmentInfo(DateTime start)
        : this()
    {
        _start = start;
    }

    public AppointmentInfo(Appointment source)
        : this()
    {
        CopyInfo(source);
    }

    public void CopyInfo(Appointment source)
    {
        _start = source.Start;
    }
}
//public class InstadoseOrderItem
//{
//    public int InsAccountID { get; set; }
//    public string Account { get; set; }
//    public string Location { get; set; }
//    public DateTime WearDate { get; set; }
//    public string PONumber { get; set; }
//    public string OrderType { get; set; }
//    public string AddressPointer { get; set; }
//    public int GDSInstaOrderType { get; set; }
//    public bool ShipToWearer { get; set; }
//    public DateTime? InitializationDate { get; set; }
//    public bool IsConsignmentOrder { get; set; }
//    public List<InstadoseOrderDetailItem> Details { get; set; }
//}

//public class InstadoseOrderDetailItem
//{
//    public int? WearerNo { get; set; }
//    public string BadgeType { get; set; }
//    public string BodyRegion { get; set; }
//    public int? UserID { get; set; }
//    public string SerialNo { get; set; }
//    public string ICCPO { get; set; }
//    public string SKU { get; set; }
//    public string Color { get; set; }
//    public int Qty { get; set; }
//}

public class AddInstadoseWearerError
{
    public int WearerNo { get; set; }
    public int ResponseCode { get; set; }
    public string ErrorMessage { get; set; }
}

public class AccountProductInfo
{
    public int? AccountID { get; set; }
    public string GDSAccount { get; set; }
    public int? LocationID { get; set; }
    public string GDSLocation { get; set; }
    public int ProductInventoryID { get; set; }
    public bool? Active { get; set; }
    public string SerialNo { get; set; }
    public int ProductID { get; set; }
    public string ProductName { get; set; }
    public string ProductSKU { get; set; }
    public bool ProductActive { get; set; }
    public float HardwareVersion { get; set; }
    public string EthMacAddress { get; set; }
    public string WifiMacAddress { get; set; }
    public int HardwareHeartBeatsCount { get; set; }
    public int HardwareLogCount { get; set; }
}

public class RMAWearerInfo
{
    public int WearerID { get; set; }
    public string BodyRegion { get; set; }
}

public class DeviceAssignCheck
{
    public int WearerID { get; set; }
    public string BodyRegionCd { get; set; }
}

public class ChildAccountProductGroupQty
{
    public int? AccountID { get; set; }
    public string GDSAccount { get; set; }
    public string CompanyName { get; set; }

    public int IDPlusQty { get; set; }
    public int InstaLinkQty { get; set; }
    public int InstaLinkUSBQty { get; set; }
    public int ID2Qty { get; set; }
}