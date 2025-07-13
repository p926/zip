using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//****************************************************************************
// 5/24/12 Warren Kakemoto
//      New List class for Account Renewals.
//****************************************************************************
//
    [Serializable()]
    public class AccountRenewalList

    {
        public AccountRenewalList()
        {
        }

        /// <summary>
        /// Collection of Account Renewals list items.
        /// </summary>
        public List<AccountRenewalListItem> Renewals { get; set; }

        /// <summary>
        /// Call response code.
        /// </summary>
        public int ResponseCode { get; set; }

    }

    [Serializable()]
    public class AccountRenewalListItem
    {
        public AccountRenewalListItem()
        {
        }

        /// <summary>
        /// Identifier of the Account.
        /// </summary>
        public int AccountID { get; set; }

        /// <summary>
        /// Company Name.
        /// </summary>
        public string CompanyName { get; set; }
        public int OrderID { get; set; }
        public string InvoiceNo { get; set; }
        public string InvoiceAmt { get; set; }
        public string RevStartDate { get; set; }
        public string RevEndDate { get; set; }
        public string RevAmt { get; set; }

        public string OrderDate { get; set; }
 
        public string BillingTermDesc { get; set; }
        public string BillingMethod { get; set; }
        public int BrandSourceID { get; set; }
        public string BrandName { get; set; }
        public string PaymentMethodName { get; set; }
    }

/// <summary>
/// Renewal Review List for Excel Export.
/// </summary>
    [Serializable()]
    public class AccountRenewalReviewList
    {
        public AccountRenewalReviewList()
        {
        }

        /// <summary>
        /// Collection of Account Renewals list items.
        /// </summary>
        public List<AccountRenewalReviewListItem> RenewalReview { get; set; }

        /// <summary>
        /// Call response code.
        /// </summary>
        public int ResponseCode { get; set; }

    }

    [Serializable()]
    public class AccountRenewalReviewListItem
    {
        public AccountRenewalReviewListItem()
        {
        }

        /// <summary>
        /// Identifier of the Account.
        /// </summary>
        public int RenewalLogID { get; set; }   
        public string OrderID { get; set; }
        public int AccountID { get; set; }

        public string AccountName { get; set; }
        public string RenewalNo { get; set; }
        public string OrderStatus { get; set; }
        public string BrandName { get; set; }
        public string BillingTermDesc { get; set; }
        public string PaymentType { get; set; }
        public string RenewalAmount { get; set; }
        public string PaymentMethodName { get; set; }
    }