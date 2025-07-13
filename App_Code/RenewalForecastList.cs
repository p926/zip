using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//****************************************************************************
// 5/24/12 Warren Kakemoto
//      New List class for Account Renewals.
//****************************************************************************
//namespace Instadose.API
//{
    [Serializable()]
    public class RenewalForecastList

    {
        public RenewalForecastList()
        {
        }

        /// <summary>
        /// Collection of Account Renewals list items.
        /// </summary>
        public List<RenewalForecastListItem> RenewalForecasts { get; set; }

        /// <summary>
        /// Call response code.
        /// </summary>
        public int ResponseCode { get; set; }

    }

    [Serializable()]
    public class RenewalForecastListItem
    {
        public RenewalForecastListItem()
        {
        }

        /// <summary>
        /// Identifier of the Account.
        /// </summary>
        public int AccountID { get; set; }

        /// <summary>
        /// Company Name.
        /// </summary>
        public string AccountName { get; set; }
        public string RenewalMonth { get; set; }
        public string RenewalYear { get; set; }
        public string RenewalTotal { get; set; }
        public string RenewalRate { get; set; }
        public int DeviceCount { get; set; }
        public int RenewalRateID { get; set; }
        public string CurrencyCode { get; set; }
 
        public string RenewalDate { get; set; }
 
        public int BrandSourceID { get; set; }
        public string BrandName { get; set; }
  
    }
//}
