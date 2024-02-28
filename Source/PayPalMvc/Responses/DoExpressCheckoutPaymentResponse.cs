using PayPalMvc.Enums;
namespace PayPalMvc {
	/// <summary>
	/// Response received from a transaction registration
	/// </summary>
    public class DoExpressCheckoutPaymentResponse : CommonPaymentResponse
    {
        public PaymentStatus PAYMENTINFO_0_PAYMENTSTATUS { get; set; }

        public string PAYMENTINFO_0_PAYMENTTYPE { get; set; }
        
        public string PAYMENTINFO_0_TRANSACTIONID { get; set; } 

        public string PAYMENTINFO_0_TRANSACTIONTYPE { get; set; }

        public string PAYMENTINFO_0_ORDERTIME { get; set; }

        public string PAYMENTINFO_0_AMT { get; set; }

        public string PAYMENTINFO_0_CURRENCYCODE { get; set; }

        public string PAYMENTINFO_0_FEEAMT { get; set; }

        // Readable properties
        public PaymentStatus PaymentStatus { get { return PAYMENTINFO_0_PAYMENTSTATUS; } }
        public decimal Amount { get { return decimal.Parse(PAYMENTINFO_0_AMT); } }
        public string PaymentTransactionId { get { return PAYMENTINFO_0_TRANSACTIONID; } } // Stored

        public string DoExpressCheckoutPaymentToString() // Stored
        {
            return string.Format("Payment Status: [{0}] Payment Type: [{1}] Transaction Type: [{2}] Order Time: [{3}] Currency Code: [{4}] Amount: [{5}] Fees: [{6}]", PaymentStatus, PAYMENTINFO_0_PAYMENTTYPE, PAYMENTINFO_0_TRANSACTIONTYPE, PAYMENTINFO_0_ORDERTIME, PAYMENTINFO_0_CURRENCYCODE, PAYMENTINFO_0_AMT, PAYMENTINFO_0_FEEAMT);
        }
	}
}