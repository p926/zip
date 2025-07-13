using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace portal_instadose_com_v3.App_Code
{
    public class AXBillableEvent
    {
        public AXServiceRef.AxdEntity_Staging _stage;
        public List<AXServiceRef.AxdEntity_StageDetail> _details;

        public AXBillableEvent(AXBillableEventHeader header, bool isInvCred = false)
        {
            if (!isInvCred)
            {
                _stage = new AXServiceRef.AxdEntity_Staging
                {

                    // Fill in all the fields             
                    BillingGroupId = header.BillGroupID,
                    Contract = header.ContractID,
                    ContractEndDate = header.ContractEnd,
                    ContractEndDateSpecified = true,
                    OrderCustomer = header.OrderCustomer,
                    ContractStartDate = header.ContractStart,
                    ContractStartDateSpecified = true,
                    CustomerPO = header.PO,
                    CurrencyCode = header.Currency,
                    TransactionPartner = header.Partner,
                    TransType = (AXServiceRef.AxdEnum_ACC_TransType)Enum.Parse(typeof(AXServiceRef.AxdEnum_ACC_TransType), header.TransactionType),
                    TransTypeSpecified = true,
                    ManufacturingOrderID = header.OrderID,
                    BillingCustomer = header.Account,
                    TransDate = header.TransactionDate,
                    TransDateSpecified = true
                };
            }
            else
            {
                _stage = new AXServiceRef.AxdEntity_Staging
                {

                    // Fill in all the fields             

                    TransType = AXServiceRef.AxdEnum_ACC_TransType.Credit,
                    TransTypeSpecified = true,
                    ManufacturingOrderID = header.OrderID,
                    TransDate = header.TransactionDate,
                    TransDateSpecified = true
                };
            }

            _details = new List<AXServiceRef.AxdEntity_StageDetail>();
        }

        public void AddDetail(AXBillableEventDetail detail, bool isInvCred = false)
        {
            AXServiceRef.AxdEntity_StageDetail _stageDetails;

            if (!isInvCred)
            {
                _stageDetails = new AXServiceRef.AxdEntity_StageDetail
                {
                    BillTriggerDate = detail.AvailToBill,
                    BillTriggerDateSpecified = true,
                    ManufacturingOrderLineRef = detail.DetailID,
                    ManufacturingOrderLineRefSpecified = true,
                    DeliveryCustomer = detail.Account,
                    ItemId = detail.AXItemID,
                    LocationID = detail.LocationName,
                    LocationAddressText1 = detail.InvLine1,
                    LocationAddressText2 = detail.InvLine2,
                    Price = detail.UnitPrice,
                    PriceSpecified = true,
                    Qty = detail.Qty,
                    QtySpecified = true,
                    ServiceStartDate = detail.StartDate,
                    ServiceStartDateSpecified = true,
                    ServiceEndDate = detail.EndDate,
                    ServiceEndDateSpecified = true,
                    Description = detail.InvItemDesc,
                    MirionTransactionId = detail.PKDetailID.ToString(),
                    OriginalInvoiceNumber = detail.AXInvNo,
                    ShipmentCount = detail.ShipmentQty,
                    ShipmentCountSpecified = true,
                    WearPeriod = detail.WearPeriod, // "Annual";
                    DeliveryCountry = detail.Country,
                    DeliveryState = detail.State,
                    ReasonCode = detail.Reason,
                    SiteDimValue = detail.AXSiteCode,
                    TerritoryDimValue = detail.AXTerritory,
                    SuppressShipments = detail.SuppressShipment ? AXServiceRef.AxdEnum_NoYes.Yes : AXServiceRef.AxdEnum_NoYes.No,
                    SuppressShipmentsSpecified = true,
                    ElementId = detail.SourceID,

                };
            }
            else
            {
                _stageDetails = new AXServiceRef.AxdEntity_StageDetail
                {
                    BillTriggerDate = detail.AvailToBill,
                    BillTriggerDateSpecified = true,
                    ManufacturingOrderLineRef = detail.DetailID,
                    ManufacturingOrderLineRefSpecified = true,
                    Description = detail.InvItemDesc,
                    ReasonCode = detail.Reason,
                    OriginalInvoiceNumber = detail.AXInvNo
                };
            }
            _details.Add(_stageDetails);
        }

        public string Process()
        {
            try
            {
                AXServiceRef.AxdAcc_ExternalStagingQuery stageDocument = new AXServiceRef.AxdAcc_ExternalStagingQuery();
                AXServiceRef.CallContext _callContext = new AXServiceRef.CallContext();
                AXServiceRef.EntityKey[] keys;
                _stage.StageDetail = _details.ToArray();
                stageDocument.Staging = new AXServiceRef.AxdEntity_Staging[] { _stage };

                // Create an instance of the document service client        
                using (AXServiceRef.Acc_ExternalStagingQueryServiceClient client = new AXServiceRef.Acc_ExternalStagingQueryServiceClient())
                {
                    client.ClientCredentials.Windows.ClientCredential.UserName = ConfigurationManager.AppSettings["AXWebServiceUsername"];
                    client.ClientCredentials.Windows.ClientCredential.Password = ConfigurationManager.AppSettings["AXWebServicePassword"];
                    client.ClientCredentials.Windows.ClientCredential.Domain = ConfigurationManager.AppSettings["AXWebServiceDomain"];

                    _callContext.Company = "DSD";

                    keys = client.create(_callContext, stageDocument);
                }

                if (keys.Length == 0)
                    throw new Exception("Error during transfer to AX.");

                if (keys[0].KeyData.Length == 0)
                    throw new Exception("Error with response from AX.");

                if (keys[0].KeyData[0].Field != "StageId")
                    throw new Exception(keys[0].KeyData[0].Value);

                return keys[0].KeyData[0].Value;
            }
            catch (Exception ex)
            {
                throw new Exception("Error during processing: " + ex.Message);
            }
        }
    }

    public class AXBillableEventHeader
    {
        public string TransactionType { get; set; }
        public string Account { get; set; }
        public string Partner { get; set; }
        public string BillGroupID { get; set; }
        public string PO { get; set; }
        public string Currency { get; set; }
        public string OrderID { get; set; }
        public DateTime? ContractStart { get; set; }
        public DateTime? ContractEnd { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string OrderCustomer { get; set; }
        public string ContractID { get; set; }
    }

    public class AXBillableEventDetail
    {
        public DateTime? AsOfDate { get; set; }
        public DateTime? AvailToBill { get; set; }
        public string Account { get; set; }
        public string OrderID { get; set; }
        public decimal? DetailID { get; set; }
        public string LocationName { get; set; }
        public string InvLine1 { get; set; }
        public string InvLine2 { get; set; }
        public int? WearerID { get; set; }
        public string SerialNo { get; set; }
        public string AXItemID { get; set; }
        public string InvItemDesc { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string WearPeriod { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? Qty { get; set; }
        public int? ShipmentQty { get; set; }
        public bool IsSource { get; set; }
        public string SourceID { get; set; }
        public bool IsTarget { get; set; }
        public string TargetID { get; set; }
        public string AXInvNo { get; set; }
        public double CreditAmount { get; set; }
        public string CreditMethod { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string AXTerritory { get; set; }
        public string AXSiteCode { get; set; }
        public bool SuppressShipment { get; set; }
        public int PKDetailID { get; set; }
        public string Reason { get; internal set; }
    }
}