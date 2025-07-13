using Instadose.Data;
using System.Linq;

namespace portal_instadose_com_v3.Helpers
{
    public static class HDealers
    {
        public static string GetDealerName(string dealerID, string defaultCompanyName)
        {
            if (string.IsNullOrEmpty(dealerID) || !int.TryParse(dealerID, out int dlrID))
                return defaultCompanyName;

            InsDataContext idc = new InsDataContext();

            string companyName = idc.Dealers.Where(d => d.DealerID == dlrID).Select(d => d.DealerName).FirstOrDefault();

            if (string.IsNullOrEmpty(companyName))
                return defaultCompanyName;
            else
                return companyName;
        }
    }
}