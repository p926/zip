using Mirion.DSD.GDS.API;
using Mirion.DSD.GDS.API.Contexts;
using Mirion.DSD.GDS.API.DataTypes;
using Mirion.DSD.GDS.API.Requests;
using Newtonsoft.Json;
using portal_instadose_com_v3.App_Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;

namespace portal_instadose_com_v3.Services
{
    /// <summary>
    /// Summary description for AXCreditRebill
    /// </summary>
    [WebService(Namespace = "http://portal.instadose.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [System.Web.Script.Services.ScriptService]
    public class AXCreditRebill : System.Web.Services.WebService
    {
        [WebMethod]
        public string GetInvoice(string invoiceId)
        {
            if (string.IsNullOrEmpty(invoiceId))
                throw new Exception("Invoice ID is not provided.");

            // get invoice data
            AXInvoiceInfo invoice = GetAXInvoiceById(invoiceId);

            return JsonConvert.SerializeObject(invoice);
        }

        [WebMethod]
        public string ProcessCredit(string invoiceId, string userId)
        {
            bool isSuccess = true;

            bool isInvoiceExist = IsMRNBillableEventInvoiceExist(invoiceId);

            if (!isInvoiceExist)
                throw new Exception("Invoice not found");

            AXInvoiceRequests aXInvoiceRequests = new AXInvoiceRequests();

            try
            {
                AXInvoiceCreditMemoRequests.ProcessInvoiceCredit(invoiceId);

                aXInvoiceRequests.InsertAXCreditLog(invoiceId, userId, "");
            }
            catch (Exception ex)
            {
                isSuccess = false;
                throw new Exception(ex.Message);
            }

            return isSuccess ? "true" : "false";
        }

        [WebMethod]
        public string ProcessRebill(string invoiceId, List<AXProcessCreditMemoDetails> details, string userId)
        {
            if (IsInvoiceCreditExist(invoiceId))
                throw new Exception("Invoice Credit already processed.");

            if (IsInvoiceRebillExist(invoiceId))
                throw new Exception("Invoice Rebill already processed.");

            bool isInvoiceExist = IsMRNBillableEventInvoiceExist(invoiceId);

            if (!isInvoiceExist)
                throw new Exception("Invoice not found.");

            if (details == null || details.Count <= 0)
                throw new Exception("Invoice Detail is not provided.");

            AXBillableEventRequests billableEventRequests = new AXBillableEventRequests();
            RebillRequests rebillRequests = new RebillRequests();

            // Copy data from BEST to Rebill tables
            try
            {
                rebillRequests.CopyRebillsFromBEST(invoiceId, userId, "");
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error occured while generating Rebill data - {0}", ex.Message));
            }

            var updatedDetails = details.Where(d => d.OriginUnitPrice != d.UnitPrice || d.OriginQuantity != d.Quantity || d.OriginShipmentQuantity != d.ShipmentQuantity).ToList();

            // update Rebill Details table by changed price, qty, shipment qty, and cancel reason
            foreach (var detail in updatedDetails)
            {
                rebillRequests.UpdateRebillDetail(invoiceId, detail.MRNOrderID, detail.MRNOrderDetailID, detail.UnitPrice, detail.Quantity, detail.ShipmentQuantity, detail.ACMCancelReasonCode);
            }

            // Update MRNOrderID in RebillHeader and RebillDetail table
            try
            {
                rebillRequests.UpdateRebillMRNOrderID(invoiceId);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error occured while updaing rebill order number - {0}", ex.Message));
            }

            // Process Credit
            try
            {
                AXInvoiceCreditMemoRequests.ProcessInvoiceCredit(invoiceId);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error occured while processing AX Credit - {0}", ex.Message));
            }

            // Copy data from Rebill to BEST tables
            try
            {
                rebillRequests.CopyBESTFromRebills(invoiceId);
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error occured while generating billable events - {0}", ex.Message));
            }

            return "true";
        }

        private AXBillableEventForInsertingDTO GenerateRebillBillableEventForInsertDTO(MRNtoACMBillableEvent originBillableEvent, List<MRNtoACMBillableEventDetail> originBillableEventDetails, List<AXProcessCreditMemoDetails> details, string originalInvoiceNumber, string newMRNOrderID)
        {
            AXBillableEventForInsertingDTO billableEvent = new AXBillableEventForInsertingDTO
            {
                ACMTxnCode = "26",
                MRNInvAccountID = originBillableEvent.MRNInvAccountID,
                MRNTransactionPartner = originBillableEvent.MRNTransactionPartner,
                BillingGroupID = originBillableEvent.BillingGroupID,
                PONumber = originBillableEvent.PONumber,
                CurrencyCode = originBillableEvent.CurrencyCode,
                MRNOrderID = newMRNOrderID,
                ACMContractID = originBillableEvent.ACMContractID,
                ContractStartDate = originBillableEvent.ContractStartDate,
                ContractEndDate = originBillableEvent.ContractEndDate,
                TransactionDate = DateTime.Now,
                MRNAccountContractID = originBillableEvent.MRNAccountContractID,
                MRNOrderIDPrefix = originBillableEvent.MRNOrderIDPrefix,
                ProcessStatus = -10,
                Details = new List<AXBillableEventDetailForInsertingDTO>()
            };

            string mrnOrderIDWithPrefix = originBillableEvent.MRNOrderIDPrefix + newMRNOrderID;

            foreach (var detail in details)
            {
                // check whether quantity or shipment quantity is not zero. If qty or shipment qty is zero, it shouldn't inserted into MRNtoACMBillableEventDetails table
                if (detail.Quantity == 0 || detail.ShipmentQuantity == 0)
                    continue;

                var originDetail = originBillableEventDetails.FirstOrDefault(d => d.MRNBillableEventDetailID == detail.MRNBillableEventDetailID);

                if (originDetail == null)
                    throw new Exception("Cannot find matching MRNtoACMBillableEventDetail data.");

                bool isUpdate = originDetail.UnitPrice != detail.UnitPrice || originDetail.Quantity != detail.Quantity || originDetail.ShipmentQty != detail.ShipmentQuantity;

                AXBillableEventDetailForInsertingDTO eventDetail = new AXBillableEventDetailForInsertingDTO
                {
                    AvailToBill = originDetail.AvailToBill,
                    MRNAccountID = originDetail.MRNAccountID,
                    MRNOrderID = mrnOrderIDWithPrefix,
                    MRNOrderDetailID = originDetail.MRNOrderDetailID,
                    LocationName = originDetail.LocationName,
                    LocationInvLine1 = originDetail.LocationInvLine1,
                    LocationInvLine2 = originDetail.LocationInvLine2,
                    MRNWearerID = originDetail.MRNWearerID,
                    SerialNo = originDetail.SerialNo,
                    AXItemID = originDetail.AXItemID,
                    InvItemDescription = originDetail.InvItemDescription,
                    ServiceStartDate = originDetail.ServiceStartDate,
                    ServiceEndDate = originDetail.ServiceEndDate,
                    WearPeriod = originDetail.WearPeriod,
                    UnitPrice = detail.UnitPrice,
                    Quantity = detail.Quantity,
                    ShipmentQty = detail.ShipmentQuantity,
                    SourceElementID = originDetail.SourceElementID,
                    AXInvoiceNumber = originalInvoiceNumber,
                    CreditAmount = null,
                    ACMCreditMethodCode = null,
                    DeliveryCountryCode = originDetail.DeliveryCountryCode,
                    DeliveryStateCode = originDetail.DeliveryStateCode,
                    AXTerritoryCode = originDetail.AXTerritoryCode,
                    AXSiteCode = originDetail.AXSiteCode,
                    SuppressShipment = originDetail.SuppressShipment,
                    CancelReason = isUpdate ? detail.ACMCancelReasonCode : null
                };

                billableEvent.Details.Add(eventDetail);
            }

            return billableEvent;
        }

        private RebillHeaderForInsertingDTO GenerateRebillHeaderForInsertDTO(MRNtoACMBillableEvent originBillableEvent, List<MRNtoACMBillableEventDetail> originBillableEventDetails, List<AXProcessCreditMemoDetails> details, string originalInvoiceNumber, string newMRNOrderID, string userID, string note)
        {
            RebillHeaderForInsertingDTO rebillHeader = new RebillHeaderForInsertingDTO
            {
                AXInvoiceNumber = originalInvoiceNumber,
                ACMTxnCode = "26",
                MRNInvAccountID = originBillableEvent.MRNInvAccountID,
                MRNTransactionPartner = originBillableEvent.MRNTransactionPartner,
                BillingGroupID = originBillableEvent.BillingGroupID,
                PONumber = originBillableEvent.PONumber,
                CurrencyCode = originBillableEvent.CurrencyCode,
                MRNOrderID = newMRNOrderID,
                ACMContractID = originBillableEvent.ACMContractID,
                ContractStartDate = originBillableEvent.ContractStartDate,
                ContractEndDate = originBillableEvent.ContractEndDate,
                TransactionDate = DateTime.Now,
                MRNAccountContractID = originBillableEvent.MRNAccountContractID,
                MRNOrderIDPrefix = originBillableEvent.MRNOrderIDPrefix,
                UserID = userID,
                RebillNote = note,
                Details = new List<RebillDetailForInsertingDTO>()
            };

            string mrnOrderIDWithPrefix = originBillableEvent.MRNOrderIDPrefix + newMRNOrderID;

            foreach (var detail in details)
            {
                var originDetail = originBillableEventDetails.FirstOrDefault(d => d.MRNBillableEventDetailID == detail.MRNBillableEventDetailID);

                if (originDetail == null)
                    throw new Exception("Cannot find matching MRNtoACMBillableEventDetail data.");

                bool isUpdate = originDetail.UnitPrice != detail.UnitPrice || originDetail.Quantity != detail.Quantity || originDetail.ShipmentQty != detail.ShipmentQuantity;

                RebillDetailForInsertingDTO eventDetail = new RebillDetailForInsertingDTO
                {
                    AvailToBill = originDetail.AvailToBill,
                    MRNAccountID = originDetail.MRNAccountID,
                    MRNOrderID = mrnOrderIDWithPrefix,
                    MRNOrderDetailID = originDetail.MRNOrderDetailID,
                    LocationName = originDetail.LocationName,
                    LocationInvLine1 = originDetail.LocationInvLine1,
                    LocationInvLine2 = originDetail.LocationInvLine2,
                    MRNWearerID = originDetail.MRNWearerID,
                    SerialNo = originDetail.SerialNo,
                    AXItemID = originDetail.AXItemID,
                    InvItemDescription = originDetail.InvItemDescription,
                    ServiceStartDate = originDetail.ServiceStartDate,
                    ServiceEndDate = originDetail.ServiceEndDate,
                    WearPeriod = originDetail.WearPeriod,
                    UnitPrice = detail.UnitPrice,
                    OldUnitPrice = originDetail.UnitPrice,
                    Quantity = detail.Quantity,
                    OldQuantity = originDetail.Quantity,
                    ShipmentQty = detail.ShipmentQuantity,
                    OldShipmentQty = originDetail.ShipmentQty,
                    SourceElementID = originDetail.SourceElementID,
                    AXInvoiceNumber = originalInvoiceNumber,
                    CreditAmount = null,
                    ACMCreditMethodCode = null,
                    DeliveryCountryCode = originDetail.DeliveryCountryCode,
                    DeliveryStateCode = originDetail.DeliveryStateCode,
                    AXTerritoryCode = originDetail.AXTerritoryCode,
                    AXSiteCode = originDetail.AXSiteCode,
                    SuppressShipment = originDetail.SuppressShipment,
                    CancelReason = isUpdate ? detail.ACMCancelReasonCode : null
                };

                rebillHeader.Details.Add(eventDetail);
            }

            return rebillHeader;
        }

        private bool IsMRNBillableEventInvoiceExist(string invoiceId)
        {
            AXInvoiceRequests invoiceReq = new AXInvoiceRequests();

            return invoiceReq.IsMRNBillableEventInvoiceExist(invoiceId);
        }

        private bool IsInvoiceCreditExist(string invoiceId)
        {
            AXInvoiceRequests invoiceReq = new AXInvoiceRequests();

            return invoiceReq.IsInvoiceCreditExist(invoiceId);
        }

        private bool IsInvoiceRebillExist(string invoiceId)
        {
            AXInvoiceRequests invoiceReq = new AXInvoiceRequests();

            return invoiceReq.IsInvoiceRebillExist(invoiceId);
        }

        private AXInvoiceInfo GetAXInvoiceById(string invoiceId)
        {
            AXInvoiceRequests invoiceReq = new AXInvoiceRequests();

            var invoice = invoiceReq.GetInvoiceSearchSummaryByInvoice(invoiceId);

            if (invoice == null)
                return null;

            var invoiceDetails = invoiceReq.GetInvoiceDetails(invoiceId);

            List<AXInvoiceInfoDetail> details = new List<AXInvoiceInfoDetail>();
            if (invoiceDetails != null && invoiceDetails.Count > 0)
            {
                details = invoiceDetails.Select(id => new AXInvoiceInfoDetail
                {
                    MRNBillableEventDetailID = id.MRNBillableEventDetailID,
                    MRNBillableEventID = id.MRNBillableEventID,
                    MRNOrderID = id.MRNOrderID,
                    MRNOrderDetailID = id.MRNOrderDetailID,
                    MRNAccountID = id.MRNAccountID,
                    LocationName = id.LocationName,
                    AXItemID = id.AXItemID,
                    InvItemDescription = id.InvItemDescription != null && id.InvItemDescription.Trim() != "" ? id.InvItemDescription : id.AXItemDescription,
                    ServiceStartDate = id.ServiceStartDate,
                    ServiceEndDate = id.ServiceEndDate,
                    WearPeriod = id.WearPeriod,
                    UnitPrice = id.UnitPrice,
                    Quantity = id.Quantity,
                    ShipmentQty = id.ShipmentQty
                }).ToList();
            }

            AXInvoiceInfo invoiceInfo = new AXInvoiceInfo
            {
                InvoiceID = invoice.INVOICEID,
                Account = invoice.INVOICEACCOUNT,
                InvoiceDate = invoice.INVOICEDATE,
                DueDate = invoice.DUEDATE,
                InvoiceAmount = invoice.INVOICEAMOUNT,
                CurrencyCode = invoice.CURRENCYCODE,
                PONumber = invoice.PURCHASEORDER,
                ContractStartDate = invoice.ACC_CONTRACTSTARTDATE,
                ContractEndDate = invoice.ACC_CONTRACTENDDATE,
                Details = details
            };

            return invoiceInfo;
        }
    }

    public class AXInvoiceInfo
    {
        public string InvoiceID { get; set; }
        public string Account { get; set; }
        public DateTime InvoiceDate { get; set; }
        public DateTime DueDate { get; set; }
        public decimal InvoiceAmount { get; set; }
        public string CurrencyCode { get; set; }
        public string PONumber { get; set; }
        public DateTime ContractStartDate { get; set; }
        public DateTime ContractEndDate { get; set; }

        public List<AXInvoiceInfoDetail> Details { get; set; }
    }

    public class AXInvoiceInfoDetail
    {
        public int MRNBillableEventDetailID { get; set; }
        public int MRNBillableEventID { get; set; }
        public string MRNOrderID { get; set; }
        public int? MRNOrderDetailID { get; set; }
        public string MRNAccountID { get; set; }
        public string LocationName { get; set; }
        public string AXItemID { get; set; }
        public string InvItemDescription { get; set; }
        public DateTime ServiceStartDate { get; set; }
        public DateTime? ServiceEndDate { get; set; }
        public string WearPeriod { get; set; }
        public decimal? UnitPrice { get; set; }
        public double? Quantity { get; set; }
        public int? ShipmentQty { get; set; }
    }

    public class AXProcessCreditMemoDetails
    {
        public int MRNBillableEventID { get; set; }
        public int MRNBillableEventDetailID { get; set; }
        public string MRNOrderID { get; set; }
        public int MRNOrderDetailID { get; set; }
        public string WearPeriod { get; set; }
        public string LocationName { get; set; }
        public string AXItemID { get; set; }
        public string ItemDescription { get; set; }
        public DateTime ServiceStartDate { get; set; }
        public DateTime? ServiceEndDate { get; set; }
        public decimal OriginUnitPrice { get; set; }
        public decimal UnitPrice { get; set; }
        public float OriginQuantity { get; set; }
        public float Quantity { get; set; }
        public int OriginShipmentQuantity { get; set; }
        public int ShipmentQuantity { get; set; }
        public string ACMCancelReasonCode { get; set; }
    }
}
