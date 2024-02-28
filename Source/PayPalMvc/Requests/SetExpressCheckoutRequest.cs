using PayPalMvc.Enums;
namespace PayPalMvc
{
    /// <summary>
    /// Represents a transaction registration that is sent to PayPal. 
    /// This should be serialized using the HttpPostSerializer.
    /// </summary>
    public class SetExpressCheckoutRequest : CommonRequest
    {
        readonly PaymentAction paymentAction;
        
        readonly string currencyCode;
        readonly decimal amount;
        readonly string paymentDescription;
        readonly string trackingReference;

        readonly string serverURL;
        readonly string email;


        public SetExpressCheckoutRequest(string currencyCode, decimal amount, string paymentDescription, string trackingReference, string serverURL, string userEmail = "")
        {
            this.method = RequestType.SetExpressCheckout;
            this.paymentAction = PaymentAction.Sale;

            this.currencyCode = currencyCode;
            this.amount = amount;
            this.serverURL = serverURL;
            this.paymentDescription = paymentDescription;
            this.trackingReference = trackingReference;
            this.email = userEmail;
        }

        public string PAYMENTREQUEST_0_CURRENCYCODE
        {
            get { return currencyCode; }
        }

        public string PAYMENTREQUEST_0_AMT
        {
            get { return amount.ToString("f2"); }
        }

        public string PAYMENTREQUEST_0_PAYMENTACTION 
        {
            get { return paymentAction.ToString(); }
        }

        public string PAYMENTREQUEST_0_DESC
        {
            get { return paymentDescription; }
        }

        public string PAYMENTREQUEST_0_INVNUM
        {
            get { return trackingReference; }
        }

        public string RETURNURL
        {
            get { return serverURL + Configuration.ReturnAction; }
        }

        public string CANCELURL
        {
            get { return serverURL + Configuration.CancelAction; }
        }

        public string EMAIL
        {
            get { return email; }
        }
    }
}