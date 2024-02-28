using System;
using System.IO;
using System.Net;
using System.Text;
using log4net;
using System.Web.Mvc;
using SupperClub.Domain;
using SupperClub.Domain.Repository;
using SupperClub.Code;
using System.Web.Configuration;
using System.Linq;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace PushNotificationService
{
    public enum MessageTemplates
    {
        FB_FRIEND_INSTALLED_APP = 1,
        FB_FRIEND_BOOKED_TICKET = 2,
        FAV_CHEF_NEW_EVENT_NOTIFICATION = 3,
        FAV_EVENT_BOOKING_REMINDER = 4,
        WAITLIST_EVENT_TICKETS_AVAILABLE = 5
    }
    /// <summary>
    /// This service allows pushing notifications to iOS devices from server
    /// </summary>
    public class MessageService
    {
        #region Private Properties and Constructors        
        private const string NotificationServiceName = "Grub Club Email Web Service";
        private static readonly ILog log = LogManager.GetLogger(typeof(MessageService));
        private string UrbanAirshipAppKey = WebConfigurationManager.AppSettings["UrbanAirshipAppKey"];
        private string UrbanAirshipAppSecret = WebConfigurationManager.AppSettings["UrbanAirshipAppSecret"];
        private string UrbanAirshipMasterSecret = WebConfigurationManager.AppSettings["UrbanAirshipMasterSecret"];
        private string UrbanAirshipAPIUrl = WebConfigurationManager.AppSettings["UrbanAirshipAPIUrl"];
        //Pushwoosh API configuration
        private string PushWooshAppCode = WebConfigurationManager.AppSettings["PushWooshAppCode"];
        private string PushWooshApiUrl = WebConfigurationManager.AppSettings["PushWooshApiUrl"];
        private string PushWooshApiAction = WebConfigurationManager.AppSettings["PushWooshApiAction"];
        private string PushWooshApiToken = WebConfigurationManager.AppSettings["PushWooshApiToken"];
        protected ISupperClubRepository _supperClubRepository;

        public MessageService(ISupperClubRepository supperClubRepository)
        {
            _supperClubRepository = supperClubRepository;
        }

        #endregion

        #region Notifications to Send

        public Dictionary<string, object> testNotification(int templateId, string deviceToken, int eventId)
        {
                MessageTemplate mt = _supperClubRepository.GetMessageTemplate(templateId);
                Event _event = null;
                SupperClub.Domain.SupperClub _supperClub = null;
                if (templateId != (int)MessageTemplates.FB_FRIEND_INSTALLED_APP)
                {
                    _event = _supperClubRepository.GetEvent(eventId);
                    if(templateId == (int)MessageTemplates.FAV_CHEF_NEW_EVENT_NOTIFICATION)
                        _supperClub = _supperClubRepository.GetSupperClub(_event.SupperClubId);
                }
                //build device token string
                string[] stringSeparators = new string[] { "," };
                string[] userList = deviceToken.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                string sDeviceToken = "[";
                foreach (string ul in userList)
                {
                    if (sDeviceToken.Length > 1)
                        sDeviceToken += ",";

                    sDeviceToken += "\"" + ul + "\"";
                }
                sDeviceToken += "]";

                Dictionary<string, string> replaceAlertText = new Dictionary<string, string>();
                replaceAlertText.Add("friendName", "Olivia Sibony");
                replaceAlertText.Add("eventName", _event != null ? _event.Name : "Multi Seating Test");
                replaceAlertText.Add("eventDate",  _event != null ? _event.Start.ToString("ddd, d MMM yyyy") : "Sun, 22 Jun 2014");
                replaceAlertText.Add("supperClubName", _supperClub != null ? _supperClub.Name : "Swati test supper club");
                string alertText = SupperClub.Code.Converter.ReplaceTemplate(mt.AlertText, replaceAlertText);
                
                Dictionary<string, string> replace = new Dictionary<string, string>();
                replace.Add("ApplicationCode", PushWooshAppCode);
                replace.Add("ApiToken", PushWooshApiToken);
                replace.Add("notificationType", mt.MessageTemplateType);
                replace.Add("friendName", "Olivia Sibony");
                replace.Add("deviceTokens", sDeviceToken);
                replace.Add("alert", alertText);
                replace.Add("evenId", eventId <= 0 ?"503" : eventId.ToString());
                replace.Add("eventName", _event != null ? _event.Name : "Multi Seating Test");
                replace.Add("supperClubId", _event != null ? _event.SupperClubId.ToString() : "73");
                replace.Add("supperClubName", _supperClub != null ? _supperClub.Name : "Swati test supper club");
                //string messageBody = SupperClub.Code.Converter.ReplaceTemplate(mt.MessageBody, replace);
                //byte[] byteArray = Encoding.UTF8.GetBytes(messageBody);
                //return PushNotification(byteArray);    
                Uri url = new Uri(PushWooshApiUrl + PushWooshApiAction);
                string messageBody = SupperClub.Code.Converter.ReplaceTemplate(mt.PushwooshMessageBody, replace);
                PushNotificationLog pnl = new PushNotificationLog();
                pnl.PushNotificationTypeId = templateId;
                pnl.Sent = true;
                pnl.DeviceTokens = deviceToken;
                pnl.MessageText = alertText;
                pnl.LogDate = DateTime.Now;
                _supperClubRepository.AddPushNotificationLog(pnl);
                return (doPostRequest(url, messageBody));                
        }

        public bool FacebookFriendAppInstallationNotification(string friendName, Guid userId, string faceBookId)
        {
            List<string> userList = _supperClubRepository.GetFacebookFriendListForPushNotification(userId, faceBookId);
            string sDeviceToken = "";
            if (userList != null && userList.Count > 0)
            {
                sDeviceToken = BuildDeviceTokenList(userList);
                MessageTemplate mt = _supperClubRepository.GetMessageTemplate((int)MessageTemplates.FB_FRIEND_INSTALLED_APP);

                Dictionary<string, string> replaceAlertText = new Dictionary<string, string>();
                replaceAlertText.Add("friendName", friendName);
                string alertText = SupperClub.Code.Converter.ReplaceTemplate(mt.AlertText, replaceAlertText);

                Dictionary<string, string> replace = new Dictionary<string, string>();
                replace.Add("notificationType", mt.MessageTemplateType);
                replace.Add("friendName", friendName);
                replace.Add("deviceTokens", sDeviceToken);
                replace.Add("alert", alertText);
                //string messageBody = SupperClub.Code.Converter.ReplaceTemplate(mt.MessageBody, replace);
                //byte[] byteArray = Encoding.UTF8.GetBytes(messageBody);
                //return getStatus(PushNotification(byteArray));
                string messageBody = SupperClub.Code.Converter.ReplaceTemplate(mt.PushwooshMessageBody, replace);
                bool status = PushNotificationUsingPushWoosh(messageBody);
                PushNotificationLog pnl = new PushNotificationLog();
                pnl.PushNotificationTypeId = (int)MessageTemplates.FB_FRIEND_INSTALLED_APP;
                pnl.Sent = status;
                pnl.DeviceTokens = sDeviceToken;
                pnl.MessageText = alertText;
                pnl.LogDate = DateTime.Now;
                _supperClubRepository.AddPushNotificationLog(pnl);
                return status;
            }
            else
                return true;
        }

        public bool FacebookFriendBoughtTicketNotification(string friendName, string eventName, int eventId, Guid userId, string faceBookId)
        {
            List<string> userList = _supperClubRepository.GetFacebookFriendListForBookingPushNotification(userId, faceBookId);
            string sDeviceToken = "";
            if (userList != null && userList.Count > 0)
            {
                sDeviceToken = BuildDeviceTokenList(userList);
                MessageTemplate mt = _supperClubRepository.GetMessageTemplate((int)MessageTemplates.FB_FRIEND_BOOKED_TICKET);

                Dictionary<string, string> replaceAlertText = new Dictionary<string, string>();
                replaceAlertText.Add("friendName", friendName);
                replaceAlertText.Add("eventName", eventName);
                string alertText = SupperClub.Code.Converter.ReplaceTemplate(mt.AlertText, replaceAlertText);

                Dictionary<string, string> replace = new Dictionary<string, string>();
                replace.Add("notificationType", mt.MessageTemplateType);
                replace.Add("friendName", friendName);
                replace.Add("eventId", eventId.ToString());
                replace.Add("deviceTokens", sDeviceToken);
                replace.Add("alert", alertText);
                //string messageBody = SupperClub.Code.Converter.ReplaceTemplate(mt.MessageBody, replace);
                //byte[] byteArray = Encoding.UTF8.GetBytes(messageBody);
                //return getStatus(PushNotification(byteArray)); ;
                string messageBody = SupperClub.Code.Converter.ReplaceTemplate(mt.PushwooshMessageBody, replace);
                bool status = PushNotificationUsingPushWoosh(messageBody);
                PushNotificationLog pnl = new PushNotificationLog();
                pnl.PushNotificationTypeId = (int)MessageTemplates.FB_FRIEND_BOOKED_TICKET;
                pnl.Sent = status;
                pnl.DeviceTokens = sDeviceToken;
                pnl.MessageText = alertText;
                pnl.LogDate = DateTime.Now;
                _supperClubRepository.AddPushNotificationLog(pnl);
                return status;
            }
            else
                return true;
        }

        public bool FavChefNewEventNotification(int eventId, int supperClubId, string eventName, string supperClubName, DateTime eventDate)
        {
            List<string> userList = _supperClubRepository.GetUserListForNewEventPushNotification(supperClubId);
            string sDeviceToken = "";
            if (userList != null && userList.Count > 0)
            {

                sDeviceToken = BuildDeviceTokenList(userList);
                MessageTemplate mt = _supperClubRepository.GetMessageTemplate((int)MessageTemplates.FAV_CHEF_NEW_EVENT_NOTIFICATION);

                Dictionary<string, string> replaceAlertText = new Dictionary<string, string>();
                replaceAlertText.Add("eventName", eventName);
                replaceAlertText.Add("supperClubName", supperClubName);
                replaceAlertText.Add("eventDate", eventDate.ToString("ddd, d MMM yyyy"));
                string alertText = SupperClub.Code.Converter.ReplaceTemplate(mt.AlertText, replaceAlertText);

                Dictionary<string, string> replace = new Dictionary<string, string>();
                replace.Add("notificationType", mt.MessageTemplateType);
                replace.Add("eventId", eventId.ToString());
                replace.Add("deviceTokens", sDeviceToken);
                replace.Add("alert", alertText);
                //string messageBody = SupperClub.Code.Converter.ReplaceTemplate(mt.MessageBody, replace);
                //byte[] byteArray = Encoding.UTF8.GetBytes(messageBody);
                //return getStatus(PushNotification(byteArray));
                string messageBody = SupperClub.Code.Converter.ReplaceTemplate(mt.PushwooshMessageBody, replace);
                bool status = PushNotificationUsingPushWoosh(messageBody);
                PushNotificationLog pnl = new PushNotificationLog();
                pnl.PushNotificationTypeId = (int)MessageTemplates.FAV_CHEF_NEW_EVENT_NOTIFICATION;
                pnl.Sent = status;
                pnl.DeviceTokens = sDeviceToken;
                pnl.MessageText = alertText;
                pnl.LogDate = DateTime.Now;
                _supperClubRepository.AddPushNotificationLog(pnl);
                return status;
            }
            else
                return true;
        }

        public bool FavEventBookingReminder(int eventId, string eventName)
        {
            List<string> userList = _supperClubRepository.GetUserListForEventBookingReminderPushNotification(eventId);
            string sDeviceToken = "";
            if (userList != null && userList.Count > 0)
            {
                sDeviceToken = BuildDeviceTokenList(userList);
                MessageTemplate mt = _supperClubRepository.GetMessageTemplate((int)MessageTemplates.FAV_CHEF_NEW_EVENT_NOTIFICATION);

                Dictionary<string, string> replaceAlertText = new Dictionary<string, string>();
                replaceAlertText.Add("eventName", eventName);
                string alertText = SupperClub.Code.Converter.ReplaceTemplate(mt.AlertText, replaceAlertText);

                Dictionary<string, string> replace = new Dictionary<string, string>();
                replace.Add("notificationType", mt.MessageTemplateType);
                replace.Add("eventId", eventId.ToString());
                replace.Add("deviceTokens", sDeviceToken);
                replace.Add("alert", alertText);
                //string messageBody = SupperClub.Code.Converter.ReplaceTemplate(mt.MessageBody, replace);
                //byte[] byteArray = Encoding.UTF8.GetBytes(messageBody);
                //return(getStatus(PushNotification(byteArray)));
                string messageBody = SupperClub.Code.Converter.ReplaceTemplate(mt.PushwooshMessageBody, replace);
                bool status = PushNotificationUsingPushWoosh(messageBody);
                PushNotificationLog pnl = new PushNotificationLog();
                pnl.PushNotificationTypeId = (int)MessageTemplates.FAV_CHEF_NEW_EVENT_NOTIFICATION;
                pnl.Sent = status;
                pnl.DeviceTokens = sDeviceToken;
                pnl.MessageText = alertText;
                pnl.LogDate = DateTime.Now;
                _supperClubRepository.AddPushNotificationLog(pnl);
                return status;
            }
            else
                return true;
        }

        public bool WaitlistEventTicketAvailabilityNotification(int eventId, string eventName)
        {
            List<string> userList = _supperClubRepository.GetUserListForEventWaitListPushNotification(eventId);
            string sDeviceToken = "";
            if (userList != null && userList.Count > 0)
            {
                sDeviceToken = BuildDeviceTokenList(userList);
                MessageTemplate mt = _supperClubRepository.GetMessageTemplate((int)MessageTemplates.FAV_CHEF_NEW_EVENT_NOTIFICATION);

                Dictionary<string, string> replaceAlertText = new Dictionary<string, string>();
                replaceAlertText.Add("eventName", eventName);
                string alertText = SupperClub.Code.Converter.ReplaceTemplate(mt.AlertText, replaceAlertText);

                Dictionary<string, string> replace = new Dictionary<string, string>();
                replace.Add("notificationType", mt.MessageTemplateType);
                replace.Add("eventId", eventId.ToString());
                replace.Add("deviceTokens", sDeviceToken);
                replace.Add("alert", alertText);
                //string messageBody = SupperClub.Code.Converter.ReplaceTemplate(mt.MessageBody, replace);
                //byte[] byteArray = Encoding.UTF8.GetBytes(messageBody);
                //return getStatus(PushNotification(byteArray));
                string messageBody = SupperClub.Code.Converter.ReplaceTemplate(mt.PushwooshMessageBody, replace);
                bool status = PushNotificationUsingPushWoosh(messageBody);
                PushNotificationLog pnl = new PushNotificationLog();
                pnl.PushNotificationTypeId = (int)MessageTemplates.FAV_CHEF_NEW_EVENT_NOTIFICATION;
                pnl.Sent = status;
                pnl.DeviceTokens = sDeviceToken;
                pnl.MessageText = alertText;
                pnl.LogDate = DateTime.Now;
                _supperClubRepository.AddPushNotificationLog(pnl);
                return status;
            }
            else
                return true;
        }

        #region Actually send the request to Push Woosh
        private bool PushNotificationUsingPushWoosh(string jsonData)
        {
            Uri url = new Uri(PushWooshApiUrl + PushWooshApiAction);
            Dictionary<string, object> response = doPostRequest(url, jsonData);
            bool status = getPushwooshResponseStatus(response);
            return status;
        }
        private Dictionary<string, object> doPostRequest(Uri url, string data)
        {
            Dictionary<string, object> returnErrorResult = new Dictionary<string, object>();
            returnErrorResult.Add("status_code", "700");
            returnErrorResult.Add("status_message", "Error occured while processing request");
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.ContentType = "text/json";
            req.Method = "POST";
            using (var streamWriter = new StreamWriter(req.GetRequestStream()))
            {
                streamWriter.Write(data);
            }
            HttpWebResponse httpResponse;
            try
            {
                // Get the response.
                httpResponse = (HttpWebResponse)req.GetResponse();

                using (StreamReader streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {

                    // Read the content.
                    string responseFromServer = streamReader.ReadToEnd();
                    // Clean up the streams
                    streamReader.Close();
                    httpResponse.Close();
                    // Deserialize json
                    var jss = new JavaScriptSerializer();
                    Dictionary<string, object> result = jss.Deserialize<Dictionary<string, object>>(responseFromServer);
                    return result;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return returnErrorResult;
        }
        private bool getPushwooshResponseStatus(Dictionary<string, object> result)
        {
            try
            {
                if (result.ContainsKey("status_code") && (string)result["status_code"] == "200")
                    return true;
                else
                {
                    log.Error("Pushwoosh PushNotification Error while pushing notifications. Details:StatusCode=" + result["status_code"] + " StatusMessage=" + result["status_message"]);
                    return false;
                }
            }
            catch (Exception ex) { log.Fatal(ex.Message, ex); }
            return false;
        }
        #endregion
        #region Actually send the request to Urban Airship

        /// <summary>
        /// creates push notification to iOS device
        /// </summary>        
        /// <param name="toAddress">Recipient address.</param>                
        /// <returns>True if no error occured.</returns>
        private Dictionary<string, object> PushNotification(byte[] byteArray)
        {
            Dictionary<string, object> returnErrorResult = new Dictionary<string, object>();
            returnErrorResult.Add("ok", "false");
            returnErrorResult.Add("message", "Error occured while processing request");
            try
            {
                // Create a request using a URL that can receive a post. 
                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(UrbanAirshipAPIUrl);
                request.Credentials = new NetworkCredential(UrbanAirshipAppKey, UrbanAirshipMasterSecret);
                // Set the Method property of the request to POST.
                request.Method = "POST";
                // Create POST data and convert it to a byte array.
                //WRITE JSON DATA TO VARIABLE D
                /*
                 This data would come from individual calling method
                    string postData = "{\"aps\": {\"badge\": 1, \"alert\": \"Hello from Urban Airship!\"}, \"device_tokens\": [\"6334c016fc643baa340eca25bc661d15055a07b475e9a6108f3f644b15dd05ac\"]}";
                    byte[] byteArray = Encoding.UTF8.GetBytes(postData);
                */              
                // Set the ContentType property of the WebRequest.
                request.ContentType = "application/json";
                request.Accept = "application/vnd.urbanairship+json; version=3;";
                // Set the ContentLength property of the WebRequest.
                request.ContentLength = byteArray.Length;
                // Get the request stream.
                Stream dataStream = request.GetRequestStream();
                // Write the data to the request stream.
                dataStream.Write(byteArray, 0, byteArray.Length);
                // Close the Stream object.
                dataStream.Close();
                
                // Get the response.
                WebResponse response = request.GetResponse(); 
                // Display the status.
                //  Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                // Get the stream containing content returned by the server.
                dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                StreamReader reader = new StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();
                // Clean up the streams
                reader.Close();
                dataStream.Close();
                response.Close();
                // Deserialize json
                var jss = new JavaScriptSerializer();
                Dictionary<string, object> result = jss.Deserialize<Dictionary<string, object>>(responseFromServer);
                return result;         
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    httpResponse.StatusCode.ToString();
                    using (var streamReader = new StreamReader(response.GetResponseStream()))
                    {
                        string error = streamReader.ReadToEnd();
                        log.Error("PushNotification Error while pushing notifications. Details:" + error);
                        // Deserialize json
                        var jss = new JavaScriptSerializer();
                        Dictionary<string, object> result = jss.Deserialize<Dictionary<string, object>>(error);
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);               
            }
            return returnErrorResult;
        }
        private bool getStatus(Dictionary<string, object> result)
        {
            if (result.ContainsKey("ok") && (bool)result["ok"] == true)
                return true;
            else
                return false;
        }
        private string BuildDeviceTokenList(List<string> userList)
        {
            string sDeviceToken = "[";
            foreach (string ul in userList)
            {
                if (sDeviceToken.Length > 1)
                    sDeviceToken += ",";

                sDeviceToken += "\"" + ul + "\"";
            }
            sDeviceToken += "]";

            return sDeviceToken;
        }
        #endregion
        #endregion
    }
        
}