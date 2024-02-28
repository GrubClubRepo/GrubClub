using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SupperClub.Domain
{
    public class BraintreeTransaction
    {
        public int Id { get; set; }
        public string TransactionStatus { get; set; }
        public bool TransactionSuccess { get; set; }
        public bool TransactionValidVenmoSDK { get; set; }
        public decimal? Amount { get;  set; }
        public string AvsErrorResponseCode { get;  set; }
        public string AvsPostalCodeResponseCode { get;  set; }
        public string AvsStreetAddressResponseCode { get;  set; }
        public string Channel { get;  set; }
        public DateTime? TransactionCreationDate { get;  set; }
        public string CvvResponseCode { get;  set; }
        public string TransactionId { get;  set; }
        public string MerchantAccountId { get;  set; }
        public string OrderId { get;  set; }
        public string PlanId { get;  set; }
        public string ProcessorAuthorizationCode { get;  set; }
        public string ProcessorResponseCode { get;  set; }
        public string ProcessorResponseText { get;  set; }
        public string PurchaseOrderNumber { get;  set; }
        public decimal? ServiceFeeAmount { get;  set; }
        public string SettlementBatchId { get;  set; }
        public string Status { get;  set; }
        public decimal? TaxAmount { get;  set; }
        public bool? TaxExempt { get;  set; }
        public string TransactionType { get;  set; }
        public DateTime? TransactionUpdateDate { get;  set; }
    }
}
