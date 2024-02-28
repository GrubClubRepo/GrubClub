using PayPalMvc.Enums;
namespace PayPalMvc
{
    /// <summary>
    /// Represents a transaction registration that is sent to PayPal. 
    /// This should be serialized using the HttpPostSerializer.
    /// </summary>
    public class DoExpressCheckoutPaymentRequest : CommonRequest
    {
        readonly PaymentAction paymentAction;
        
        readonly string token;
        readonly string payerId;

        readonly string currencyCode;
        readonly decimal amount;


        public DoExpressCheckoutPaymentRequest(string token, string payerId, string currencyCode, decimal amount)
        {
            this.method = RequestType.DoExpressCheckoutPayment;
            this.paymentAction = PaymentAction.Sale;

            this.token = token;
            this.payerId = payerId;

            this.currencyCode = currencyCode;
            this.amount = amount;
        }

        public string TOKEN
        {
            get { return token; }
        }

        public string PAYERID
        {
            get { return payerId; }
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
    }
}