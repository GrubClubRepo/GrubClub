using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using PayPalMvc.Enums;

namespace PayPalMvc {
	/// <summary>
	/// Object that represents a notification POST from PayPal
    /// NOT IMPLEMENTED
	/// </summary>
	[ModelBinder(typeof (PayPalBinder))]
	public class PayPalResponse {
		// Commone Response
        public ResponseType Status { get; set; }
        public string TransactionId { get; set; }
        public string TimeStamp { get; set; }
        public string Version { get; set; }
        public string Build { get; set; }
        // Error Response
        public string ErrorCode { get; set; }
        public string ErrorShortMessage { get; set; }
        public string ErrorLongMessage { get; set; }
        public string ErrorSevertityCode { get; set; }
        // SetExpressCheckoutResponse Responses
		public string Token { get; set; }
        // GetExpressCheckoutDetails Responses

        // DoExpressCheckoutPayment fields

		/// <summary>
		/// Was the transaction successful?
		/// </summary>
		public virtual bool WasTransactionSuccessful {
			get {
				return (Status == ResponseType.Success ||
				        Status == ResponseType.SuccessWithWarning);
			}
		}
	}
}