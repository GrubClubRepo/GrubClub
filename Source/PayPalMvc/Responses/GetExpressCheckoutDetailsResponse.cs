using PayPalMvc.Enums;
namespace PayPalMvc {
	/// <summary>
	/// Response received from a transaction registration
	/// </summary>
    public class GetExpressCheckoutDetailsResponse : CommonPaymentResponse
    {
        public CheckoutStatus CHECKOUTSTATUS { get; set; }

        /// <summary>
        /// Should be the same as the baskedId we passed in as the tracking reference for the transaction
        /// </summary>
        public string PAYMENTREQUEST_0_INVNUM { get; set; }

        public string PAYERSTATUS { get; set; }

        public string PAYERID { get; set; } // Stored

        public string EMAIL { get; set; }

        public string SALUTATION { get; set; }

        public string FIRSTNAME { get; set; }

        public string MIDDLENAME { get; set; }

        public string LASTNAME { get; set; }

        // For readability and storage

        public string TrackingReference { get { return PAYMENTREQUEST_0_INVNUM; } } // Stored

        public string GetExpressCheckoutDetailsToString() // Stored
        {
            return string.Format("Checkout Status: [{0}] Payer Status: [{1}] Name: [{2} {3} {4} {5}] PayPal Email: [{6}]", CHECKOUTSTATUS.ToString(), PAYERSTATUS, SALUTATION, FIRSTNAME, MIDDLENAME, LASTNAME, EMAIL);
        }
	}
}