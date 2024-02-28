using System.Web.Routing;
using log4net;
using System;
using System.Web.Configuration;
using SupperClub.Logger;

namespace PayPalMvc {
	/// <summary>
	/// Default ITransactionRegistrar implementation
	/// </summary>
	public class TransactionRegistrar : ITransactionRegistrar {
		readonly Configuration configuration;
		readonly IHttpRequestSender requestSender;
        private HttpPostSerializer serializer;
        private ResponseSerializer deserializer;
        protected static readonly ILog log = LogManager.GetLogger(typeof(TransactionRegistrar));
		
        /// <summary>
		/// Creates a new instance of the TransactionRegistrar using the configuration specified in the web.conf, and an HTTP Request Sender.
		/// </summary>
		public TransactionRegistrar() : this(Configuration.Current, new HttpRequestSender()) {
		}

		/// <summary>
		/// Creates a new instance of the TransactionRegistrar
		/// </summary>
		public TransactionRegistrar(Configuration configuration, IHttpRequestSender requestSender) {
			this.configuration = configuration;
			this.requestSender = requestSender;
            this.serializer = new HttpPostSerializer();
            this.deserializer = new ResponseSerializer();
		}

        public SetExpressCheckoutResponse SendSetExpressCheckout(RequestContext context, string currencyCode, decimal amount, string description, string trackingReference, string serverURL, string userEmail)
        {
            SetExpressCheckoutRequest request = new SetExpressCheckoutRequest(currencyCode, amount, description, trackingReference, serverURL, userEmail);
            string postData = serializer.Serialize(request);
            SystemLogging.LogLongMessage("PayPal Send Request", "Serlialized Request to PayPal API: " + System.Environment.NewLine + postData, log, LogLevel.DEBUG);
            // Send the request, recieve decode and deserialise the response
            string response = requestSender.SendRequest(Configuration.Current.PayPalAPIUrl, postData);
            string decodedResponse = System.Web.HttpUtility.UrlDecode(response, System.Text.Encoding.Default);
            SystemLogging.LogLongMessage("PayPal Response Recieved", "Decoded Respose from PayPal API: " + System.Environment.NewLine + decodedResponse, log, LogLevel.DEBUG);
            return deserializer.Deserialize<SetExpressCheckoutResponse>(decodedResponse);
        }

        public GetExpressCheckoutDetailsResponse SendGetExpressCheckoutDetails(RequestContext context, string token)
        {
            GetExpressCheckoutDetailsRequest request = new GetExpressCheckoutDetailsRequest(token);
            string postData = serializer.Serialize(request);
            SystemLogging.LogLongMessage("PayPal Send Request", "Serlialized Request to PayPal API: " + System.Environment.NewLine + postData, log, LogLevel.DEBUG);
            // Send the request, recieve decode and deserialise the response
            string response = requestSender.SendRequest(Configuration.Current.PayPalAPIUrl, postData);
            string decodedResponse = System.Web.HttpUtility.UrlDecode(response, System.Text.Encoding.Default);
            SystemLogging.LogLongMessage("PayPal Response Recieved", "Decoded Respose from PayPal API: " + System.Environment.NewLine + decodedResponse, log, LogLevel.DEBUG);
            return deserializer.Deserialize<GetExpressCheckoutDetailsResponse>(decodedResponse);
        }

        public DoExpressCheckoutPaymentResponse SendDoExpressCheckoutPayment(RequestContext context, string token, string payerId, string currencyCode, decimal amount)
        {
            DoExpressCheckoutPaymentRequest request = new DoExpressCheckoutPaymentRequest(token, payerId, currencyCode, amount);
            string postData = serializer.Serialize(request);
            SystemLogging.LogLongMessage("PayPal Send Request", "Serlialized Request to PayPal API: " + System.Environment.NewLine + postData, log, LogLevel.DEBUG);
            // Send the request, recieve decode and deserialise the response
            string response = requestSender.SendRequest(Configuration.Current.PayPalAPIUrl, postData);
            string decodedResponse = System.Web.HttpUtility.UrlDecode(response, System.Text.Encoding.Default);
            SystemLogging.LogLongMessage("PayPal Response Recieved", "Decoded Respose from PayPal API: " + System.Environment.NewLine + decodedResponse, log, LogLevel.DEBUG);
            return deserializer.Deserialize<DoExpressCheckoutPaymentResponse>(decodedResponse);
        }
	}
}