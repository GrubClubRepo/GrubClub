#region License

// Copyright 2009 The Sixth Form College Farnborough (http://www.farnborough.ac.uk)
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://github.com/JeremySkinner/SagePayMvc

#endregion

namespace SagePayMvc.Internal
{
    /// <summary>
    /// Represents a transaction registration that is sent to SagePay. 
    /// This should be serialized using the HttpPostSerializer.
    /// </summary>
    public class TransactionRegistration
    {
        readonly ShoppingBasket basket;
        readonly Address billingAddress;
        readonly Address deliveryAddress;
        readonly CreditCard creditcardDetail;
        readonly string customerEMail;
        readonly string vendorName;
        //readonly string profile;
        readonly string currency;

        //const string NormalFormMode = "NORMAL";
        //const string LowProfileFormMode = "LOW";
        public TransactionRegistration(string vendorTxCode, ShoppingBasket basket, string notificationUrl,
                                       CreditCard creditCard, Address billingAddress, Address deliveryAddress, string customerEmail,
                                       string vendorName, PaymentFormProfile paymentFormProfile, string currencyCode)
        {
            VendorTxCode = vendorTxCode;
            NotificationURL = notificationUrl;
            this.basket = basket;
            this.billingAddress = billingAddress;
            this.deliveryAddress = deliveryAddress;
            customerEMail = customerEmail;
            this.vendorName = vendorName;
            this.currency = currencyCode;
            this.creditcardDetail = creditCard;

        }

        public TransactionRegistration(string vendorTxCode, ShoppingBasket basket, string notificationUrl,
                                       CreditCard creditCard, Address billingAddress, Address deliveryAddress, string customerEmail,
                                       string vendorName, PaymentFormProfile paymentFormProfile, string currencyCode, string mD, string paRes)
        {
            VendorTxCode = vendorTxCode;
            NotificationURL = notificationUrl;
            this.basket = basket;
            this.billingAddress = billingAddress;
            this.deliveryAddress = deliveryAddress;
            customerEMail = customerEmail;
            this.vendorName = vendorName;
            this.currency = currencyCode;
            this.creditcardDetail = creditCard;
            MD = mD;
            PARes = paRes;
           
        }

        

        public string VPSProtocol
        {
            get {
                    var config = Configuration.Current;
                    return config.ProtocolVersion; 
                }
        }

        public string TxType
        {
            get { return "PAYMENT"; }
        }

        public string Vendor
        {
            get { return vendorName; }
        }

        public string VendorTxCode { get; private set; }

        [Format("f2")]
        public decimal Amount
        {
            get { return basket.Total; }
        }

        public string Currency
        {
            get { return currency; }
        }

        public string Description
        {
            get { return basket.Name; }
        }

        [Unencoded]
        public string NotificationURL { get; private set; }

        #region Credit Card Info
        public string CardHolder
        {
            get { return ( creditcardDetail == null) ? string.Empty : creditcardDetail.CardHolder; }
        }
        public string CardNumber
        {
            get { return (creditcardDetail == null) ? string.Empty : creditcardDetail.CardNumber; }
        }
        [Optional]
        public string StartDate
        {
            get { return (creditcardDetail == null) ? string.Empty : creditcardDetail.StartDate; }
        }
        public string ExpiryDate
        {
            get { return (creditcardDetail == null) ? string.Empty : creditcardDetail.ExpiryDate; }
        }
        [Optional]
        public string IssueNumber
        {
            get { return (creditcardDetail == null) ? string.Empty : creditcardDetail.IssueNumber; }
        }
        [Optional]
        public string CV2
        {
            get { return (creditcardDetail == null) ? string.Empty : creditcardDetail.CV2; }
        }
        public string CardType
        {
            get { return (creditcardDetail == null) ? string.Empty : creditcardDetail.CardType; }
        }
        #endregion

        #region Billing Address
        public string BillingSurname
        {
            get { return (billingAddress == null) ? string.Empty : billingAddress.Surname; }
        }

        public string BillingFirstnames
        {
            get { return (billingAddress == null) ? string.Empty : billingAddress.Firstnames; }
        }

        public string BillingAddress1
        {
            get { return (billingAddress == null) ? string.Empty : billingAddress.Address1; }
        }

        [Optional]
        public string BillingAddress2
        {
            get { return (billingAddress == null) ? string.Empty : billingAddress.Address2; }
        }

        public string BillingCity
        {
            get { return (billingAddress == null) ? string.Empty : billingAddress.City; }
        }

        public string BillingPostCode
        {
            get { return (billingAddress == null) ? string.Empty : billingAddress.PostCode; }
        }

        public string BillingCountry
        {
            get { return (billingAddress == null) ? string.Empty : billingAddress.Country; }
        }

        [Optional]
        public string BillingState
        {
            get { return (billingAddress == null) ? string.Empty : billingAddress.State; }
        }

        [Optional]
        public string BillingPhone
        {
            get { return (billingAddress == null) ? string.Empty : billingAddress.Phone; }
        }
        #endregion

        #region Delivery Address
        public string DeliverySurname
        {
            get { return (deliveryAddress == null) ? string.Empty : deliveryAddress.Surname; }
        }

        public string DeliveryFirstnames
        {
            get { return (deliveryAddress == null) ? string.Empty : deliveryAddress.Firstnames; }
        }

        public string DeliveryAddress1
        {
            get { return (deliveryAddress == null) ? string.Empty : deliveryAddress.Address1; }
        }

        [Optional]
        public string DeliveryAddress2
        {
            get { return (deliveryAddress == null) ? string.Empty : deliveryAddress.Address2; }
        }

        public string DeliveryCity
        {
            get { return (deliveryAddress == null) ? string.Empty : deliveryAddress.City; }
        }

        public string DeliveryPostCode
        {
            get { return (deliveryAddress == null) ? string.Empty : deliveryAddress.PostCode; }
        }

        public string DeliveryCountry
        {
            get { return (deliveryAddress == null) ? string.Empty : deliveryAddress.Country; }
        }

        [Optional]
        public string DeliveryState
        {
            get { return (deliveryAddress == null) ? string.Empty : deliveryAddress.State; }
        }

        [Optional]
        public string DeliveryPhone
        {
            get { return (deliveryAddress == null) ? string.Empty : deliveryAddress.Phone; }
        }
        #endregion

        public string CustomerEMail
        {
            get { return customerEMail; }
        }

        public string Basket
        {
            get { return basket.ToString(); }
        }

        [Optional]
        public int GiftAidPayment
        {
            get { return 0; }
        }

        [Optional]
        public int ApplyAVSCV2
        {
            get { return 0; }
        }

        [Optional]
        public int Apply3DSecure
        {
            get { return 0; }
        }

        [Optional]
        public string ClientIPAddress
        {
            get { return string.Empty; }
        }

        [Optional]
        public string AccountType
        {
            get { return "E"; }
        }

        [Optional]
        public int BillingAgreement
        {
            get { return 0; }

        }

        [Optional]
        public string MD
        {
            get;
            private set;
        }
        [Optional]
        public string PARes
        {
            get;
            private set;
        }


    }
}