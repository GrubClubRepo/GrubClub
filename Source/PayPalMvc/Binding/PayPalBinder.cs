using System.Collections.Generic;
using System.Web.Mvc;
using PayPalMvc.Enums;

namespace PayPalMvc {
	/// <summary>
	/// IModelBinder implementation for deserializing a notification post into a PayPalResponse object.
    /// NOT IMPLEMENTED
	/// </summary>
	public class PayPalBinder : IModelBinder {
		// Common Responses
        const string Status = "ACK";
        const string TransactionId = "CORRELATIONID";
        const string TimeStamp = "TIMESTAMP";
        const string Version = "VERSION";
        const string Build = "BUILD";
        // Error Responses
        const string ErrorCode = "L_ERRORCODE0";
        const string ErrorShortMessage = "L_SHORTMESSAGE0";
        const string ErrorLongMessage = "L_LONGMESSAGE0";
        const string ErrorSevertityCode = "L_SEVERITYCODE0";
        // SetExpressCheckoutResponse Responses
        const string Token = "TOKEN";
        // GetExpressCheckoutDetails Responses

        // DoExpressCheckoutPayment Responses

		public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext) {
			var response = new PayPalResponse();
			// Common Response fields
            response.Status = GetStatus(bindingContext.ValueProvider);
            response.TransactionId = GetFormField(TransactionId, bindingContext.ValueProvider);
            response.TimeStamp = GetFormField(TimeStamp, bindingContext.ValueProvider);
            response.Version = GetFormField(Version, bindingContext.ValueProvider);
            response.Build = GetFormField(Build, bindingContext.ValueProvider);
            response.ErrorCode = GetFormField(Build, bindingContext.ValueProvider);
            response.ErrorShortMessage = GetFormField(ErrorShortMessage, bindingContext.ValueProvider);
            response.ErrorLongMessage = GetFormField(ErrorLongMessage, bindingContext.ValueProvider);
            response.ErrorSevertityCode = GetFormField(ErrorSevertityCode, bindingContext.ValueProvider);
            // SetExpressCheckoutResponse fields
            response.Token = GetFormField(Token, bindingContext.ValueProvider);

            // GetExpressCheckoutDetails fields

            // DoExpressCheckoutPayment fields

			return response;
		}

		ResponseType GetStatus(IValueProvider valueProvider) {
            string value = GetFormField(Status, valueProvider);
			return ResponseTypes.ConvertStringToPayPalResponseType(value);
		}

		string GetFormField(string key, IValueProvider provider) {
			ValueProviderResult result = provider.GetValue(key);
            
			if(result != null) {
				return (string)result.ConvertTo(typeof(string));
			}

			return null;
		}
	}
}