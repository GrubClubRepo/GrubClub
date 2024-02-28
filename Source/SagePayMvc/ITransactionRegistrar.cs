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

using System.Web.Routing;

namespace SagePayMvc {
	public interface ITransactionRegistrar {
		/// <summary>
		/// Sends a transaction registration to SagePay and receives a TransactionRegistrationResponse
		/// </summary>
		TransactionRegistrationResponse Send(RequestContext context, string vendorTxCode, ShoppingBasket basket, CreditCard creditCard,
		                                     Address billingAddress, Address deliveryAddress, string customerEmail, PaymentFormProfile paymentFormProfile = PaymentFormProfile.Normal, string currencyCode = "GBP");
        TransactionRegistrationResponse Send(RequestContext context, string vendorTxCode, ShoppingBasket basket, CreditCard creditCard,
                                                    Address billingAddress, Address deliveryAddress, string customerEmail, string mD, string paRes, PaymentFormProfile paymentFormProfile = PaymentFormProfile.Normal, string currencyCode = "GBP");
        TransactionRegistrationResponse SendThreeDResult(RequestContext context, string mD, string paRes);
    }
}