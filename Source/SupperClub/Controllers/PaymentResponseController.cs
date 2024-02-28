using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SupperClub.Domain.Repository;
using SagePayMvc;
using SagePayMvc.ActionResults;

namespace SupperClub.Controllers
{
    [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
    public class PaymentResponseController : BaseController
    {
        public PaymentResponseController(ISupperClubRepository supperClubRepository)
        {
            _supperClubRepository = supperClubRepository;
        }

        public ActionResult Notify(SagePayResponse response)
        {
            // SagePay should have sent back the order ID
            if (string.IsNullOrEmpty(response.VendorTxCode))
            {
                return new ErrorResult();
            }

            // Get the order out of our "database"
            var basket = _supperClubRepository.GetBasket(new Guid(response.VendorTxCode));
            var transaction = _supperClubRepository.GetPaymentTransaction(basket.Id.ToString());

            // IF there was no matching order, send a TransactionNotfound error
            if (basket == null)
            {
                return new TransactionNotFoundResult(response.VendorTxCode);
            }

            // Check if the signature is valid.
            // Note that we need to look up the vendor name from our configuration.
            if (!response.IsSignatureValid(transaction.SecurityKey, SagePayMvc.Configuration.Current.VendorName))
            {
                return new InvalidSignatureResult(response.VendorTxCode);
            }

            // All good - tell SagePay it's safe to charge the customer.
            return new ValidOrderResult(basket.Id.ToString(), response);
        }

        public ActionResult Failed(string vendorTxCode)
        {
            return View();
        }

        public ActionResult Success(string vendorTxCode)
        {
            return View();
        } 

    }
}
