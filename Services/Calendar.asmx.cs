using Instadose.API;
using Instadose.API.DA;
using Instadose.Data;
using Mirion.DSD.GDS.API;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Services;
using Telerik.Web.UI;

namespace portal_instadose_com_v3.Services
{
    /// <summary>
    /// Summary description for Calendar
    /// </summary>
    [WebService(Namespace = "http://portal.instadose.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class Calendar : System.Web.Services.WebService
    {
        [WebMethod]
        public int AddCalendar(int account, string calendarName, string calendarDesc, string frequency, string readDay, string readTime, string noteInitials = "ins", int? location = null)
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
                                where a.AccountID == account
                                select a.AccountID).FirstOrDefault();

            if (insAccountID == 0)
            {
                throw new Exception("Account does not exist.");
            }

            int? insLocationID = null;

            if (location.HasValue)
            {
                insLocationID = idc.Locations.FirstOrDefault(l => l.AccountID == insAccountID && l.LocationID == location).LocationID;
            }

            var daCal = new DACalendars();
            int resp = daCal.Create(detail, insAccountID, insLocationID, null, DateTime.Today);

            //MiscRequests mReq = new MiscRequests();
            //var responseCode = mReq.AddAuditTransaction(account, 0, "6L", noteInitials);

            return resp;
        }

        [WebMethod]
        public int UpdateCalendar(int calendarID, int account, List<DateTime> scheduleDates, int[] lstLocations, string noteInitials = "ins")
        {
            var WSCalendars = new DACalendars();
            InsDataContext idc = new InsDataContext();

            var accountID = (from a in idc.Accounts where a.AccountID == account select a.AccountID).FirstOrDefault();
            if (accountID == 0)
                throw new Exception("Account does not exist.");

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

            // Get the assigned locations based on the checked list items.
            calendar.AssignedLocationIDs = (from l in idc.Locations
                                            where l.AccountID == accountID && lstLocations.Contains(l.LocationID)
                                            select l.LocationID).ToList();

            int responseCode = WSCalendars.Update2(calendar);

            //MiscRequests mReq = new MiscRequests();
            //var response = mReq.AddAuditTransaction(account, 0, "6P", noteInitials);

            return responseCode;
        }

        [WebMethod]
        public string GetCalendarDetails(int calendarID, int account)
        {
            var WSCalendars = new DACalendars();
            InsDataContext idc = new InsDataContext();

            var acc = (from a in idc.Accounts where a.AccountID == account select a).FirstOrDefault();
            if (acc == null)
            {
                throw new Exception("Account does not exist.");
            }

            var calendars = (from acs in acc.AccountSchedules
                             where acs.ScheduleID == calendarID
                             select acs).FirstOrDefault();

            if (calendars == null)
            {
                throw new Exception("Calendar not found.");
            }

            var scheduleDates = calendars.Schedule.ScheduleDates.Where(d => d.Date > DateTime.Today).Select(d => new { d.ScheduleID, Date = d.Date }).ToList();

            var assignedLocations = calendars.Schedule.Locations.Select(l => new { LocationID = l.LocationID, l.LocationName }).ToList();

            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("\"Dates\":" + Newtonsoft.Json.JsonConvert.SerializeObject(scheduleDates.OrderBy(d => d.Date)) + ",");
            sb.Append("\"Locations\":" + Newtonsoft.Json.JsonConvert.SerializeObject(assignedLocations));
            sb.Append("}");
            return sb.ToString();
        }

        [WebMethod]
        public string GetCalendarDetail(int calendarID, int account)
        {
            InsDataContext idc = new InsDataContext();

            var acc = idc.Accounts.FirstOrDefault(a => a.AccountID == account);
            if (acc == null)
                throw new Exception("Account does not exist.");

            DACalendars calendar = new DACalendars();
            var details = calendar.Details(calendarID, account);

            return JsonConvert.SerializeObject(details);
        }

        [WebMethod]
        public int GetScheduleCounts(int account)
        {
            InsDataContext idc = new InsDataContext();

            var acc = idc.Accounts.FirstOrDefault(a => a.AccountID == account);
            if (acc == null)
                throw new Exception("Account does not exist.");

            return acc.AccountSchedules.Count(s => s.Active);
        }
    }

    [Serializable()]
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
}
