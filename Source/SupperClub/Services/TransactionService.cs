using System;
using System.Web.Routing;
using SagePayMvc;
using PayPalMvc;
using SupperClub.Domain;
using SupperClub.Domain.Repository;
using log4net;
using SupperClub.WebUI;
using SupperClub.Code;
using SupperClub.Logger;

namespace SupperClub.Services
{
    public interface ITransactionService
    {
        TransactionRegistrationResponse SendSagePayTransaction(TicketBasket storeBasket, CreditCard creditCard, Address billingAddress, RequestContext context);
        TransactionRegistrationResponse SendSagePayThreeDResult(string vendorTxCode, RequestContext context, string mD, string paRes);

        SetExpressCheckoutResponse SendPayPalSetExpressCheckoutRequest(RequestContext context, TicketBasket basket, string userEmail, string serverURL);
        GetExpressCheckoutDetailsResponse SendPayPalGetExpressCheckoutDetailsRequest(RequestContext context, string token);
        DoExpressCheckoutPaymentResponse SendPayPalDoExpressCheckoutPaymentRequest(RequestContext context, TicketBasket basket, string token, string payerId);
    } 

    public class TransactionService : ITransactionService
    {
        private SagePayMvc.ITransactionRegistrar _sagePayTransactionRegistrar;
        private PayPalMvc.ITransactionRegistrar _payPalTransactionRegistrar;
        private ISupperClubRepository _supperClubRepository;
        protected static readonly ILog log = LogManager.GetLogger(typeof(TransactionService));

        public TransactionService(SagePayMvc.ITransactionRegistrar sagePayTransactionRegistrar, PayPalMvc.ITransactionRegistrar payPalTransactionRegistrar, ISupperClubRepository supperClubRepository)
        {
            _sagePayTransactionRegistrar = sagePayTransactionRegistrar;
            _payPalTransactionRegistrar = payPalTransactionRegistrar;
            _supperClubRepository = supperClubRepository;
        }

        #region SagePay

        public TransactionRegistrationResponse SendSagePayTransaction(TicketBasket storeBasket, CreditCard creditCard, Address billingAddress, RequestContext context)
        {
            // Construct a SagePay basket from our Store basket.
            // We don't use the SagePay basket directly from the application as it only requires a subset of the information
            var basket = new ShoppingBasket("Shopping Basket for " + billingAddress.Firstnames + " " + billingAddress.Surname);

            //Fill the basket. Ignore VAT multiplier
            foreach (var ticket in storeBasket.Tickets)
            {
                basket.Add(new BasketItem(1, ticket.Description, ticket.TotalPrice - ticket.DiscountAmount));
            }

            var sagePayAddress = billingAddress;

            var response = _sagePayTransactionRegistrar.Send(context, storeBasket.Id.ToString(), basket, creditCard, sagePayAddress, sagePayAddress, null);
  
            var transaction = new PaymentTransaction
            {
                VendorTxCode = storeBasket.Id.ToString(),
                VpsTxId = response.VPSTxId,
                SecurityKey = response.SecurityKey,
                RedirectUrl = response.NextURL,
                DateInitialised = DateTime.Now,
                ResponseDate = DateTime.Now,
                RequestDate = DateTime.Now,
                PaymentStatus = response.Status.ToString(),
                Payment3DStatus = response.ThreeDSecureStatus.ToString(),
                PaymentStatusDetail = response.StatusDetail
            };

            // Add Transaction
            _supperClubRepository.CreateTransction(transaction);

            return response;
        }

        public TransactionRegistrationResponse SendSagePayThreeDResult(string vendorTxCode, RequestContext context, string mD, string paRes)
        {
            var response = _sagePayTransactionRegistrar.SendThreeDResult(context, mD, paRes);

            var transaction = new PaymentTransaction
            {
                VendorTxCode = vendorTxCode,
                VpsTxId = response.VPSTxId,
                SecurityKey = response.SecurityKey,
                RedirectUrl = response.NextURL,
                DateInitialised = DateTime.Now,
                ResponseDate = DateTime.Now,
                RequestDate = DateTime.Now,
                PaymentStatus = response.Status.ToString(),
                Payment3DStatus = response.ThreeDSecureStatus.ToString(),
                PaymentStatusDetail = response.StatusDetail,
                CAVV = response.CAVV
            };

            // Add transaction record
            _supperClubRepository.CreateTransction(transaction);

            return response;
        }

        #endregion

        #region PayPal

        public SetExpressCheckoutResponse SendPayPalSetExpressCheckoutRequest(RequestContext context, TicketBasket basket, string userEmail, string serverURL)
        {
            try
            {
                LogMessage("SendPayPalSetExpressCheckoutRequest", LogLevel.DEBUG);
                string purchaseDescription = string.Format("{0} x {1}", basket.TotalTickets, basket.Tickets[0].Description);
                SetExpressCheckoutResponse response = _payPalTransactionRegistrar.SendSetExpressCheckout(context, "GBP", basket.TotalPrice - basket.TotalDiscount, purchaseDescription, basket.Id.ToString(), serverURL, userEmail);

                // Add a PayPal transaction record
                PayPalTransaction transaction = new PayPalTransaction
                {
                    RequestId = response.RequestId,
                    TrackingReference = basket.Id.ToString(),
                    RequestTime = DateTime.Now,
                    RequestStatus = response.ResponseStatus.ToString(),
                    TimeStamp = response.TIMESTAMP,
                    RequestError = response.ErrorToString,
                    Token = response.TOKEN,
                    RequestData = "Initiating Purchase Process for: " + purchaseDescription // This is just so we can track Purchase request (Event Id) against the Token for further API calls
                };

                _supperClubRepository.CreateTransction(transaction);

                return response;
            }
            catch (Exception ex)
            {
                LogException(ex.Message, ex);
            }
            return null;
        }

        public GetExpressCheckoutDetailsResponse SendPayPalGetExpressCheckoutDetailsRequest(RequestContext context, string token)
        {
            try
            {
                LogMessage("SendPayPalGetExpressCheckoutDetailsRequest", LogLevel.DEBUG);
                GetExpressCheckoutDetailsResponse response = _payPalTransactionRegistrar.SendGetExpressCheckoutDetails(context, token);

                // Add a PayPal transaction record
                PayPalTransaction transaction = new PayPalTransaction
                {
                    RequestId = response.RequestId,
                    TrackingReference = response.TrackingReference,
                    RequestTime = DateTime.Now,
                    RequestStatus = response.ResponseStatus.ToString(),
                    TimeStamp = response.TIMESTAMP,
                    RequestError = response.ErrorToString,
                    Token = response.TOKEN,
                    PayerId = response.PAYERID,
                    RequestData = response.GetExpressCheckoutDetailsToString(),
                };

                _supperClubRepository.CreateTransction(transaction);

                return response;
            }
            catch (Exception ex)
            {
                LogException(ex.Message, ex);
            }
            return null;
        }

        public DoExpressCheckoutPaymentResponse SendPayPalDoExpressCheckoutPaymentRequest(RequestContext context, TicketBasket basket, string token, string payerId)
        {
            try
            {
                LogMessage("SendPayPalDoExpressCheckoutPaymentRequest", LogLevel.DEBUG);
                DoExpressCheckoutPaymentResponse response = _payPalTransactionRegistrar.SendDoExpressCheckoutPayment(context, token, payerId, "GBP", basket.TotalPrice - basket.TotalDiscount);

                // Add a PayPal transaction record
                PayPalTransaction transaction = new PayPalTransaction
                {
                    RequestId = response.RequestId,
                    TrackingReference = basket.Id.ToString(),
                    RequestTime = DateTime.Now,
                    RequestStatus = response.ResponseStatus.ToString(),
                    TimeStamp = response.TIMESTAMP,
                    RequestError = response.ErrorToString,
                    Token = response.TOKEN,
                    RequestData = response.DoExpressCheckoutPaymentToString(),
                    PaymentTransactionId = response.PaymentTransactionId,
                    PaymentError = response.PaymentErrorToString,
                };

                _supperClubRepository.CreateTransction(transaction);

                return response;
            }
            catch (Exception ex)
            {
                LogException(ex.Message, ex);
            }
            return null;
        }

        #endregion

        #region Logging

        private static void LogMessage(string message, LogLevel level = LogLevel.INFO)
        {
            WebUILogging.LogMessage(message, log, level);
        }

        private static void LogException(string message, Exception ex)
        {
            WebUILogging.LogException(message, ex, log);
        }

        #endregion
    }
}