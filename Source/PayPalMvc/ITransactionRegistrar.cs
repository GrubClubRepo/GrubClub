using System.Web.Routing;

namespace PayPalMvc {
	public interface ITransactionRegistrar {
		/// <summary>
		/// Sends a transaction registration to PayPal
		/// </summary>
        SetExpressCheckoutResponse SendSetExpressCheckout(RequestContext context, string currencyCode, decimal amount, string description, string trackingReference, string serverURL, string userEmail);

        /// <summary>
        /// Gets results of transaction
        /// </summary>
        GetExpressCheckoutDetailsResponse SendGetExpressCheckoutDetails(RequestContext context, string token);

        /// <summary>
        /// Requests the payment to be complete
        /// </summary>
        DoExpressCheckoutPaymentResponse SendDoExpressCheckoutPayment(RequestContext context, string token, string payerId, string currencyCode, decimal amount);
    }
}