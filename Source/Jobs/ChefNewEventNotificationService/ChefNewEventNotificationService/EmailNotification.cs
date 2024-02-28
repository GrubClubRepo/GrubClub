using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChefNewEventNotificationService.DLL;
using ChefNewEventNotificationService.BLL;
using ChefNewEventNotificationService.Helpers;
using System.Net;
using System.IO;

namespace ChefNewEventNotificationService
{
    class EmailNotification
    {
        static void Main(string[] args)
        {

            try
            {
                GetUserEventInfo _getUserEventInfo = new GetUserEventInfo();
                EmailHelper _emailHelper = new EmailHelper();
                
                #region Send E-mail Notifications for Wishlited Events
                try
                {
                CommonHelper.Log("Started e-mail notification job for wishlisted events", LogLevel.Log, NotificationType.Wishlist);
                //Get email template
                EmailTemplate wishListTemplate = _emailHelper.GetTemplate(Configuration.WishListEmailTemplateId);
                
                //Get User event list for wishlisted events
                List<UserWishListEventInfo> _lstUserWishListEventInfo = _getUserEventInfo.GetUserWishListedEventDetails(Configuration.Offset);
                if (_lstUserWishListEventInfo != null && _lstUserWishListEventInfo.Count > 0)
                {
                    List<Guid> userIds = (from x in _lstUserWishListEventInfo
                                          select x.User.UserId).Distinct().ToList<Guid>();

                    foreach (Guid _userId in userIds)
                    {
                        Dictionary<string, string> replace = new Dictionary<string, string>();
                        replace.Add("contactEmail", Configuration.DefaultContactEmailAddress);
                        List<UserWishListEventInfo> _list = (from x in _lstUserWishListEventInfo
                                                             where x.User.UserId == _userId
                                                             select x).ToList<UserWishListEventInfo>();                        
                        
                        string eventList = string.Empty;
                        string eventIds = string.Empty;
                        string emailAddress = "";
                        if (_list != null && _list.Count > 0)
                        {
                            replace.Add("guestName", CommonHelper.HTMLEncodeSpecialChars(_list[0].User.FirstName));
                            replace.Add("officeAddress", Configuration.OfficeAddress);
                            emailAddress = _list[0].User.EmailAddress;    
                            foreach (UserWishListEventInfo uwlei in _list)
                            {                                
                                Dictionary<string, string> replaceEventBody = new Dictionary<string, string>();
                                string eventDateTime = uwlei.Event.Start.ToString("ddd, d MMM yyyy") + " : " + uwlei.Event.Start.ToShortTimeString() + " - " + uwlei.Event.End.ToShortTimeString();
                                replaceEventBody.Add("eventName", CommonHelper.HTMLEncodeSpecialChars(uwlei.Event.Name));
                                replaceEventBody.Add("eventCity", uwlei.Event.City);
                                replaceEventBody.Add("eventDateTime", eventDateTime);
                                replaceEventBody.Add("eventCost", uwlei.Event.Cost == 0 ? "Free" : ("&pound;" + CostCalculator.CostToGuest(uwlei.Event.Cost,uwlei.Event.Commission).ToString()));
                                replaceEventBody.Add("eventUrl", Configuration.ServerURL + uwlei.Event.SupperClubUrlFriendlyName + "/" + uwlei.Event.UrlFriendlyName + "/" + uwlei.Event.EventId.ToString());
                                replaceEventBody.Add("utmParameters", Configuration.WishListUtmParameters.Replace("[utmMediumValue]",uwlei.Event.SupperClubUrlFriendlyName));
                                eventIds += (eventIds == string.Empty? "" : ",") + uwlei.Event.EventId.ToString(); 
                                string eventBodyTemplate = CommonHelper.ReplaceTemplate(CommonHelper.WishListEventInfoBody, replaceEventBody);
                                eventList += eventBodyTemplate;
                            }                            
                        }
                        replace.Add("eventList", eventList);
                        string body = CommonHelper.ReplaceTemplate(wishListTemplate.Body, replace);
                        bool success = _emailHelper.SendMail(emailAddress, wishListTemplate.Subject, body, wishListTemplate.IsHtml);
                        if(success)
                        {
                            CommonHelper.Log("Sent wishlist notification to e-mail " + emailAddress, LogLevel.Log, NotificationType.Wishlist, emailAddress);
                            bool status = _emailHelper.UpdateEmailNotificationSentStatus(_userId, eventIds);
                        }
                        else
                            CommonHelper.Log("Send wishlist notification failed to e-mail " + emailAddress, LogLevel.Error, NotificationType.FollowChef, emailAddress);
                    }
                 }
                CommonHelper.Log("Finished e-mail notification job for wishlisted events", LogLevel.Log, NotificationType.Wishlist);                
                 }
                catch (Exception ex)
                {
                CommonHelper.Log(ex.Message + ex.StackTrace, LogLevel.Error, NotificationType.Wishlist);
                }
                #endregion

                #region Send New Event E-mail Notifications for followed chefs
                try
                {
                CommonHelper.Log("Starting New Event E-mail Notifications for followed chefs", LogLevel.Log, NotificationType.FollowChef);
                //Get email templates
                EmailTemplate followChefTemplate = _emailHelper.GetTemplate(Configuration.FollowChefEmailTemplateId);
                //Get User event list for wishlisted events
                List<UserFollowChefEventInfo> _lstUserFollowChefEventInfo = _getUserEventInfo.UserFollowChefEventDetails();
                if (_lstUserFollowChefEventInfo != null && _lstUserFollowChefEventInfo.Count > 0)
                {
                    List<Guid> userIds = (from x in _lstUserFollowChefEventInfo
                                          select x.User.UserId).Distinct().ToList<Guid>();

                    foreach (Guid _userId in userIds)
                    {
                        Dictionary<string, string> replace = new Dictionary<string, string>();
                        replace.Add("contactEmail", Configuration.DefaultContactEmailAddress);
                        replace.Add("officeAddress", Configuration.OfficeAddress);
                        string EmailAddress = string.Empty;
                        List<UserFollowChefEventInfo> _list = (from x in _lstUserFollowChefEventInfo
                                                             where x.User.UserId == _userId
                                                               select x).ToList<UserFollowChefEventInfo>();
                        string eventList = string.Empty;
                        string eventIds = string.Empty;
                        if (_list != null && _list.Count > 0)
                        {
                            replace.Add("guestName", CommonHelper.HTMLEncodeSpecialChars(_list[0].User.FirstName));
                            EmailAddress = _list[0].User.EmailAddress;
                            foreach (UserFollowChefEventInfo ufcei in _list)
                            {
                                Dictionary<string, string> replaceEventBody = new Dictionary<string, string>();
                                replaceEventBody.Add("eventName", CommonHelper.HTMLEncodeSpecialChars(ufcei.Name));
                                replaceEventBody.Add("grubClubName", CommonHelper.HTMLEncodeSpecialChars(ufcei.SupperClubName));
                                replaceEventBody.Add("eventUrl", Configuration.ServerURL + ufcei.SupperClubUrlFriendlyName + "/" + ufcei.UrlFriendlyName + "/" + ufcei.EventId.ToString());
                                replaceEventBody.Add("utmParameters", Configuration.FollowChefUtmParameters.Replace("[utmMediumValue]", ufcei.SupperClubUrlFriendlyName));
                                string eventBodyTemplate = CommonHelper.ReplaceTemplate(CommonHelper.FollowChefInfoBody, replaceEventBody);
                                eventList += eventBodyTemplate;
                                eventIds += (eventIds == string.Empty ? "" : ",") + ufcei.EventId.ToString(); 
                            }
                        }
                        replace.Add("eventList", eventList);
                        string body = CommonHelper.ReplaceTemplate(followChefTemplate.Body, replace);
                        bool success = _emailHelper.SendMail(EmailAddress, followChefTemplate.Subject, body, followChefTemplate.IsHtml);
                        if (success)
                        {
                            CommonHelper.Log("Sent new event notification to e-mail " + EmailAddress, LogLevel.Log, NotificationType.FollowChef, EmailAddress);
                            bool updateStatus = _emailHelper.UpdateEmailNotificationSentStatusForFollowChef(_userId, eventIds); 
                            if(updateStatus)
                                CommonHelper.Log("Added e-mail notification sent status for Follow Chef. UserId:" + _userId.ToString(), LogLevel.Log, NotificationType.FollowChef, EmailAddress);
                            else
                                CommonHelper.Log("Failed to add e-mail notification sent status for Follow Chef. UserId:" + _userId.ToString(), LogLevel.Log, NotificationType.FollowChef, EmailAddress);
                        }
                        else
                            CommonHelper.Log("Send new event notification failed to e-mail " + EmailAddress, LogLevel.Error, NotificationType.FollowChef, EmailAddress);
                    }                    
                }
                CommonHelper.Log("Finished New Event E-mail Notifications for followed chefs", LogLevel.Log, NotificationType.FollowChef);
                }
                catch (Exception ex)
                {
                    CommonHelper.Log(ex.Message + ex.StackTrace, LogLevel.Error);
                }
                #endregion


                //// update the table containing newly created events
                //bool _status = _getUserEventInfo.UpdateNewlyCreatedEvents();

                // Send the e-mail to sys admin if there are any errors
                if(Configuration.ErrorCounter > 0)
                {
                    bool success = _emailHelper.SendMail(Configuration.AdminEmailAddress, "There were errors while running Wishlist/Follow email job", Configuration.ErrorText , false);
                }
            }

            catch (Exception ex)
            {
                CommonHelper.Log(ex.Message + ex.StackTrace, LogLevel.Error);
            }
        }
    }
}
