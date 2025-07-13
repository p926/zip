using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace portal_instadose_com_v3.Services
{
    /// <summary>
    /// Summary description for Finance
    /// </summary>
    [WebService(Namespace = "http://portal.instadose.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class Finance : System.Web.Services.WebService
    {
        [WebMethod]
        public string GetOutsideCollectors()
        {
            var idc = new Instadose.Data.InsDataContext();
            var collectors = idc.OutsideCollectors.ToList();

            return JsonConvert.SerializeObject(collectors);
        }

        [WebMethod]
        public string UpdateOutsideCollector(int accountID, int? collectorID = null)
        {
            var idc = new Instadose.Data.InsDataContext();
            var account = idc.Accounts.Where(a => a.AccountID == accountID).FirstOrDefault();
            account.OutsideCollectorID = collectorID;

            idc.SubmitChanges();

            return JsonConvert.SerializeObject(0);
        }

        [WebMethod]
        public string GetAROverrideUsers()
        {
            var dc = new Mirion.DSD.GDS.API.Contexts.UnixDataClassesDataContext();
            var list = dc.CSWSAppSettings.Where(c => c.AppSettingKey == "AROverride").ToList();

            return JsonConvert.SerializeObject(list);
        }
    }
}
