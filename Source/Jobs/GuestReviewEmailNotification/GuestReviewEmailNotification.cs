using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GuestReviewEmailNotification.DLL;
using GuestReviewEmailNotification.BLL;
using GuestReviewEmailNotification.Helpers;
using System.Net;
using System.IO;

namespace GuestReviewEmailNotification
{
    class ReviewEmailNotification
    {
        static void Main(string[] args)
        {
            try
            {
                GetUserEventInfo _getUserEventInfo = new GetUserEventInfo();
                EmailHelper _emailHelper = new EmailHelper();

                #region Send reminder e-mail for submitting reviews
                try
                {
                    CommonHelper.Log("Started review reminder e-mail notification job", LogLevel.Log, NotificationType.Review);
                    //Get email template
                    EmailTemplate reviewReminderTemplate = _emailHelper.GetTemplate(Configuration.ReviewReminderEmailTemplateId);

                    //Get User event list for sending review request
                    List<UserEventInfo> _lstUserEventInfo = _getUserEventInfo.GetReviewReminderEmailUserList(Configuration.ReminderCount);
                    if (_lstUserEventInfo != null && _lstUserEventInfo.Count > 0)
                    {
                        //Send e-mails to registered guests
                        foreach (UserEventInfo item in _lstUserEventInfo)
                        {
                            Dictionary<string, string> replace = new Dictionary<string, string>();
                            replace.Add("contactEmail", Configuration.DefaultContactEmailAddress);
                            string emailAddress = "";
                            replace.Add("guestName", CommonHelper.HTMLEncodeSpecialChars(item.User.FirstName));
                            emailAddress = item.User.EmailAddress;
                            replace.Add("eventName", CommonHelper.HTMLEncodeSpecialChars(item.Name));
                            replace.Add("eventUrl", Configuration.ServerURL + item.SupperClubUrlFriendlyName + "/" + item.UrlFriendlyName + "/" + item.EventId.ToString());
                            replace.Add("utmParameters", Configuration.ReviewUtmParameters.Replace("[utmMediumValue]", item.SupperClubUrlFriendlyName));
                            replace.Add("utmMediumValue", item.SupperClubUrlFriendlyName);
                            replace.Add("officeAddress", Configuration.OfficeAddress);
                            ReviewAuthorisationModel auth = new ReviewAuthorisationModel();
                            string authKey = string.Empty;
                            if (!item.IsFriend)
                                authKey = Configuration.ReviewURL + auth.Encrypt(item.EventId, item.User.UserId.ToString());
                            else
                            {
                                if (item.User.UserId == null)
                                    authKey = Configuration.ReviewURL + auth.Encrypt(item.EventId, true, emailAddress);
                                else
                                    authKey = Configuration.ReviewURL + auth.Encrypt(item.EventId, item.User.UserId.ToString());
                            }
                            replace.Add("reviewUrl", authKey);
                            string body = CommonHelper.ReplaceTemplate(reviewReminderTemplate.Body, replace);
                            bool success = _emailHelper.SendMail(emailAddress, reviewReminderTemplate.Subject, body, reviewReminderTemplate.IsHtml);
                            if (success)
                            {
                                CommonHelper.Log("Sent review reminder e-mail to " + emailAddress, LogLevel.Log, NotificationType.ReviewReminder, emailAddress);
                                bool logged = _emailHelper.UpdateReviewEmailNotificationLog(item.RowId);
                                if(logged)
                                    CommonHelper.Log("Updated review reminder notification log for RowId=" + item.RowId.ToString(), LogLevel.Log, NotificationType.Review, emailAddress);
                                else
                                    CommonHelper.Log("Failed to update review reminder notification log for RowId=" + item.RowId.ToString(), LogLevel.Log, NotificationType.Review, emailAddress);
                            }
                            else
                                CommonHelper.Log("Sent review reminder e-mail failed for " + emailAddress, LogLevel.Error, NotificationType.ReviewReminder, emailAddress);
                        }                        
                    }
                    CommonHelper.Log("Finished reminder e-mail notification job for Review submision", LogLevel.Log, NotificationType.ReviewReminder);
                }
                catch (Exception ex)
                {
                    CommonHelper.Log(ex.Message + ex.StackTrace, LogLevel.Error, NotificationType.ReviewReminder);
                }
                #endregion

                #region Send E-mail Notifications for submitting reviews
                try
                {
                    CommonHelper.Log("Started review e-mail notification job", LogLevel.Log, NotificationType.Review);
                    //Get email template
                    EmailTemplate reviewTemplate = _emailHelper.GetTemplate(Configuration.ReviewEmailTemplateId);

                    //Get User event list for sending review request
                    List<UserEventInfo> _lstUserEventInfo = _getUserEventInfo.GetEventReviewEmailUserList(Configuration.Offset);
                    if (_lstUserEventInfo != null && _lstUserEventInfo.Count > 0)
                    {
                        //Send e-mails to registered guests
                        foreach (UserEventInfo item in _lstUserEventInfo)
                        {
                            Dictionary<string, string> replace = new Dictionary<string, string>();
                            replace.Add("contactEmail", Configuration.DefaultContactEmailAddress);
                            string emailAddress = "";
                            replace.Add("guestName", CommonHelper.HTMLEncodeSpecialChars(item.User.FirstName));
                            emailAddress = item.User.EmailAddress;
                            replace.Add("eventName", CommonHelper.HTMLEncodeSpecialChars(item.Name));
                            replace.Add("utmMediumValue", item.SupperClubUrlFriendlyName);
                            replace.Add("utmParameters", Configuration.ReviewUtmParameters.Replace("[utmMediumValue]", item.SupperClubUrlFriendlyName));
                            replace.Add("eventUrl", Configuration.ServerURL + item.SupperClubUrlFriendlyName + "/" + item.UrlFriendlyName + "/" + item.EventId.ToString());
                            replace.Add("officeAddress", Configuration.OfficeAddress);
                            ReviewAuthorisationModel auth = new ReviewAuthorisationModel();
                            string authKey = string.Empty;
                            if (!item.IsFriend)
                                authKey = Configuration.ReviewURL + auth.Encrypt(item.EventId, item.User.UserId.ToString());
                            else
                            {
                                if (item.User.UserId == null)
                                    authKey = Configuration.ReviewURL + auth.Encrypt(item.EventId, true, emailAddress);
                                else
                                    authKey = Configuration.ReviewURL + auth.Encrypt(item.EventId, item.User.UserId.ToString());
                            }
                            replace.Add("reviewUrl", authKey);
                            string body = CommonHelper.ReplaceTemplate(reviewTemplate.Body, replace);
                            bool success = _emailHelper.SendMail(emailAddress, reviewTemplate.Subject, body, reviewTemplate.IsHtml);
                            if (success)
                            {
                                CommonHelper.Log("Sent review e-mail to " + emailAddress + ", ReviewURL=" + authKey, LogLevel.Log, NotificationType.Review, emailAddress);
                                bool logged = _emailHelper.AddReviewEmailNotificationLog(item.EventId, emailAddress, item.IsFriend, item.User.UserId);
                                if(logged)
                                    CommonHelper.Log("Added review notification log for eventId=" + item.EventId.ToString() + ", UserId=" + (item.User.UserId == null ? "null" : item.User.UserId.ToString()) + ", e-mail=" + emailAddress, LogLevel.Log, NotificationType.Review, emailAddress);
                                else
                                    CommonHelper.Log("Failed to add review notification log for eventId=" + item.EventId.ToString() + ", UserId=" + (item.User.UserId == null ? "null" : item.User.UserId.ToString()) + ", e-mail=" + emailAddress, LogLevel.Log, NotificationType.Review, emailAddress);
                            }
                            else
                                CommonHelper.Log("Sent review e-mail failed for " + emailAddress, LogLevel.Error, NotificationType.Review, emailAddress);
                        }                        
                    }
                    CommonHelper.Log("Finished e-mail notification job for Review submision e-mails", LogLevel.Log, NotificationType.Review);
                }
                catch (Exception ex)
                {
                    CommonHelper.Log(ex.Message + ex.StackTrace, LogLevel.Error, NotificationType.Review);
                }
                #endregion
               
            }
            catch(Exception e)
            {
                CommonHelper.Log(e.Message + e.StackTrace, LogLevel.Error);
            }
        }
    }
}
