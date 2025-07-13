using Mirion.DSD.GDS.API.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace portal_instadose_com_v3.App_Code
{
    public class AXInvoiceCreditMemoRequests
    {
        public static void ProcessInvoiceCredit(string invoiceId)
        {
            AXInvoiceRequests invoiceReq = new AXInvoiceRequests();

            var invoice = invoiceReq.GetInvoiceSearchSummaryByInvoice(invoiceId);

            if (invoice == null)
                throw new Exception("AX Invoice Not Found.");

            var header = new AXBillableEventHeader
            {
                OrderID = AXInvoiceRequests.GetNextCreditMemoOrderID(),
                TransactionDate = DateTime.Now,
                TransactionType = "25"
            };

            var billableEvent = new AXBillableEvent(header, true);

            var detail = new AXBillableEventDetail
            {
                OrderID = header.OrderID,
                DetailID = 1,
                AvailToBill = DateTime.Now,
                InvItemDesc = "Rebill",
                AXInvNo = invoiceId,
                Reason = "ACM-0004"
            };

            billableEvent.AddDetail(detail, true);
            billableEvent.Process();
        }
    }
}