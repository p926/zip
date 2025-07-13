using Instadose.Data;
using Mirion.DSD.GDS.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace portal_instadose_com_v3.Helpers
{
    public static class HAXInvoice
    {
        public static DateTime? GetPreERPInvoiceDate(string invoiceNo, bool isInstadoseProcessing)
        {
            if (string.IsNullOrEmpty(invoiceNo))
                return null;

            string tmpInvoiceNo = invoiceNo.Substring(4);

            if (isInstadoseProcessing)
            {
                InsDataContext idc = new InsDataContext();

                // Instadose Invoice number is 7 character string
                tmpInvoiceNo = tmpInvoiceNo.Substring(0, 7);
                return idc.fn_GetInvoiceDate(tmpInvoiceNo);
            }
            else
            {
                if (!int.TryParse(tmpInvoiceNo, out int invNo))
                    return null;

                FinanceRequests fr = new FinanceRequests();

                return fr.GetInvoiceDate(invNo);
            }
        }
    }
}