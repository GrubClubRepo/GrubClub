using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Net.Mail;
using System.Web;
using log4net;
using SupperClub.Domain;
using SupperClub.Domain.Repository;
using SupperClub.Code;
using System.Web.Configuration;
using System.Linq;
using System.Net;

namespace EmailService
{
    public enum EmailTemplates
    {
        ContactUsForm = 1,
        PasswordReset = 2,
        SupperClubRegistered = 3,
        BookingConfirmedGuest = 4,
        BookingConfirmedHost = 5,
        GuestRefused = 6,
        EventCancelled = 7,
        Welcome = 8,
        NewReviewHost = 9,
        NewReviewAdmin = 10,
        BookingConfirmedGuestWalkIn = 11,
        BookingConfirmedGuestMultiMenu = 12,
        BookingConfirmedGuestWalkInMultiMenu = 13,
        WaitListRequest = 14,
        GuestInvitee = 15,
        EventCreated = 16,
        EventApprovalRequestReceivedHost = 17,
        EventApprovalConfirmationHost = 18,
        EventRejectionConfirmationHost = 19,
        PrivateGrubClubForm = 20,
        BookingCancellationNotification = 21,
        AccountVerification = 22,
        ReferralVoucher=23,
        GiftVoucher=24,
        WishListEventSellingOutNotification = 25,
        BookingConfirmationToFriends=26
    }
    public class UserList
    {
        public Guid UserId { get; set; }
        public User User  { get; set; }
        public int  numberOfGuests { get; set; }
    }
    /// <summary>
    /// Email service allows sending emails from a configured email account.
    /// </summary>
    public class EmailService
    {
        #region Private Properties and Constructors

        /// <summary>
        /// The email service name.
        /// </summary>
        private const string EmailServiceName = "Grub Club Email Web Service";
        private const string ApplicationCurrencyHtmlSymbol = "&pound;";
        private static readonly ILog log = LogManager.GetLogger(typeof(EmailService));
        private string DefaultAdminEmailAddress = WebConfigurationManager.AppSettings["DefaultAdminEmailAddress"];
        private string DefaultContactEmailAddress = WebConfigurationManager.AppSettings["DefaultContactEmailAddress"];        
        private string SendToTestAddress = null;
        private string ServerURL = ServerMethods.ServerUrl;
        private bool SendEmailToHost = bool.Parse(WebConfigurationManager.AppSettings["SendEmailToHost"]);
        private bool SendHostTestEmail = bool.Parse(WebConfigurationManager.AppSettings["SendHostTestEmail"]);
        private string UtmTagForWishList = WebConfigurationManager.AppSettings["UtmTagForWishList"];
        private string OfficeAddress = WebConfigurationManager.AppSettings["OfficeAddress"];

        protected ISupperClubRepository _supperClubRepository;
        
        public EmailService(ISupperClubRepository supperClubRepository, string sendToTestAddress = null)
        {
            _supperClubRepository = supperClubRepository;
            SendToTestAddress = sendToTestAddress;
        }

        #endregion

        #region Emails to Send
        public bool SendGiftVoucherEmail(string guestName, string email, DateTime date, string vCode,decimal Price)
        {
            EmailTemplate emt = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.GiftVoucher);

            Dictionary<string, string> replace = new Dictionary<string, string>();
            replace.Add("GuestName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(guestName));
            replace.Add("UserName", UserMethods.CurrentUser.FirstName);
            replace.Add("VCode", vCode);
            replace.Add("Price", Price.ToString());
            replace.Add("Date", date.ToShortDateString());
            replace.Add("emailAddress", email);
            replace.Add("serverURL", ServerURL);
            replace.Add("contactEmail", DefaultContactEmailAddress);

            replace.Add("OfficeAddress", OfficeAddress);

            string body = SupperClub.Code.Converter.ReplaceTemplate(emt.Body, replace);
            bool success = true;
            try
            {
                SendMail(email, emt.Subject, body, emt.Html);
            }
            catch {
                success = false;
            }
            return success;
        }
        public bool SendFriendReferralVoucherUsEmail(string userName,string friendName,string friendEmail,DateTime date,string vCode)
        {
            EmailTemplate emt = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.ReferralVoucher);

            Dictionary<string, string> replace = new Dictionary<string, string>();
            replace.Add("UserName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(friendName));
            replace.Add("Name", userName);
            replace.Add("VCode", vCode);
            replace.Add("Date", date.ToShortDateString());
            replace.Add("emailAddress", friendName);
            replace.Add("serverURL", ServerURL);
            replace.Add("contactEmail", DefaultContactEmailAddress);

            replace.Add("OfficeAddress", OfficeAddress);

           
            string body = SupperClub.Code.Converter.ReplaceTemplate(emt.Body, replace);

            return SendMail(friendEmail, emt.Subject, body, emt.Html);
        }
        public bool SendAdminContactUsEmail(string userName, string emailAddress, string message)
        {
            EmailTemplate emt = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.ContactUsForm);

            Dictionary<string, string> replace = new Dictionary<string, string>();
            replace.Add("serverURL", ServerURL);
            replace.Add("contactEmail", DefaultContactEmailAddress);            
            replace.Add("userName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(userName));
            replace.Add("time", DateTime.Now.ToString("ddd, d MMM yyyy") + " " + DateTime.Now.ToShortTimeString());
            replace.Add("emailAddress", emailAddress);
            replace.Add("message", message);

            replace.Add("OfficeAddress", OfficeAddress);

            string body = SupperClub.Code.Converter.ReplaceTemplate(emt.Body, replace);

           // return SendMail(emailToSendTo(DefaultAdminEmailAddress), emt.Subject, body, emt.Html);
            return SendMail(emailToSendTo(DefaultContactEmailAddress), emt.Subject, body, emt.Html);
        }
        public bool SendAdminPrivateGrubClubRequestEmail(string email, string numberOfGuests, string cuisine, string pricePerHead, string dateChoice, string locationChoice, string otherRequirements)
        {
            EmailTemplate emt = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.PrivateGrubClubForm);

            Dictionary<string, string> replace = new Dictionary<string, string>();
            replace.Add("serverURL", ServerURL);
            replace.Add("contactEmail", DefaultContactEmailAddress);
            replace.Add("time", DateTime.Now.ToString("ddd, d MMM yyyy") + " " + DateTime.Now.ToShortTimeString());
            replace.Add("emailAddress", email);
            replace.Add("numberOfGuests", numberOfGuests);
            replace.Add("cuisine", cuisine);
            replace.Add("pricePerHead", pricePerHead);
            replace.Add("dateChoice", dateChoice);
            replace.Add("locationChoice", locationChoice);
            replace.Add("otherRequirements", otherRequirements);
            replace.Add("currency", ApplicationCurrencyHtmlSymbol);

            replace.Add("OfficeAddress", OfficeAddress);

            string body = SupperClub.Code.Converter.ReplaceTemplate(emt.Body, replace);

            return SendMail(emailToSendTo(DefaultAdminEmailAddress), emt.Subject, body, emt.Html);
        }
        public bool SendAdminPrivateGrubClubRequestEmail(string venue, string chefName, string email, string numberOfGuests, string cuisine, string pricePerHead, string dateChoice, string locationChoice, string otherRequirements)
        {
            EmailTemplate emt = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.PrivateGrubClubForm);

            Dictionary<string, string> replace = new Dictionary<string, string>();
            replace.Add("serverURL", ServerURL);
            replace.Add("contactEmail", DefaultContactEmailAddress);
            replace.Add("time", DateTime.Now.ToString("ddd, d MMM yyyy") + " " + DateTime.Now.ToShortTimeString());
            replace.Add("emailAddress", email);
            replace.Add("numberOfGuests", numberOfGuests);
            replace.Add("cuisine", cuisine);
            replace.Add("pricePerHead", pricePerHead);
            replace.Add("dateChoice", dateChoice);
            replace.Add("locationChoice", locationChoice);
            replace.Add("otherRequirements", otherRequirements);
            replace.Add("currency", ApplicationCurrencyHtmlSymbol);
            replace.Add("Chef", chefName);

            replace.Add("OfficeAddress", OfficeAddress);

            string body = SupperClub.Code.Converter.ReplaceTemplate(emt.Body, replace);

            return SendMail(emailToSendTo(DefaultAdminEmailAddress), emt.Subject + "-" + venue, body, emt.Html);
        }
        public bool SendAdminWaiListEmail(int eventId, string emailAddress)
        {
            EmailTemplate emt = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.WaitListRequest);

            Dictionary<string, string> replace = new Dictionary<string, string>();
            replace.Add("serverURL", ServerURL);
            replace.Add("contactEmail", DefaultContactEmailAddress);
            replace.Add("time", DateTime.Now.ToString("ddd, d MMM yyyy") + " " + DateTime.Now.ToShortTimeString());
            Event e = null;
            if (eventId != 0)
                e = _supperClubRepository.GetEvent(eventId);
            else
                e = _supperClubRepository.GetFirstActiveEvent(); // for email testing

            string eventDateTime = "";
            if(e != null)
                eventDateTime = e.Start.ToString("ddd, d MMM yyyy") + " : " + e.Start.ToShortTimeString() + " - " + e.End.ToShortTimeString();
            
            replace.Add("eventName", e.Name);
            replace.Add("eventDateTime", eventDateTime);
            replace.Add("eventId", e.Id.ToString());
            replace.Add("eventUrlFriendlyName", e.UrlFriendlyName);

            emt.Subject = "Event Waitlist Request Notification | " + e.Name;
            string body = SupperClub.Code.Converter.ReplaceTemplate(emt.Body, replace);

            return SendMail(emailToSendTo(DefaultAdminEmailAddress), emt.Subject, body, emt.Html);
        }
        public bool SendAdminBookingCancellationNotificationEmail(User user, Event _event, int menuOptionId, User currentUser, int seatingId, DateTime seatingTime, int numberOfGuests, decimal totalPrice)
        {
            EmailTemplate emt = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.BookingCancellationNotification);

            string menuOptionTitle = "";
            if(menuOptionId > 0)
            {
                EventMenuOption emo = new EventMenuOption();
                emo = _supperClubRepository.GetEventMenuOption(menuOptionId);
                if(emo != null)
                    menuOptionTitle = emo.Title;
            }

            Dictionary<string, string> replace = new Dictionary<string, string>();
            replace.Add("serverURL", ServerURL);
            replace.Add("contactEmail", DefaultContactEmailAddress);
            replace.Add("guestName", (string.IsNullOrEmpty(user.FirstName) ? "" : user.FirstName) + " " + (string.IsNullOrEmpty(user.LastName) ? "" : user.LastName));
            replace.Add("guestEmail", user.Email);
            replace.Add("adminUser", (string.IsNullOrEmpty(currentUser.FirstName) ? "" : currentUser.FirstName) + " " + (string.IsNullOrEmpty(currentUser.LastName) ? "" : currentUser.LastName));
            replace.Add("time", DateTime.Now.ToString("ddd, d MMM yyyy") + " " + DateTime.Now.ToShortTimeString());
            replace.Add("eventURL", _event.UrlFriendlyName);
            replace.Add("eventId", _event.Id.ToString());
            replace.Add("eventName", _event.Name);
            replace.Add("numberOfGuests", numberOfGuests.ToString());
            replace.Add("totalPrice", totalPrice.ToString());
            replace.Add("eventDateTime", seatingId > 0 ? (seatingTime.ToString("ddd, d MMM yyyy") + " " + seatingTime.ToShortTimeString()) : (_event.Start.ToString("ddd, d MMM yyyy") + " " + _event.Start.ToShortTimeString()));
            replace.Add("eventMenuOptionHtml", menuOptionId > 0 ? ("<p>Menu Option: " + menuOptionTitle + "</p>"):"");

            replace.Add("OfficeAddress", OfficeAddress);

            string body = SupperClub.Code.Converter.ReplaceTemplate(emt.Body, replace);

            return SendMail(emailToSendTo(DefaultAdminEmailAddress), emt.Subject, body, emt.Html);
        }
        public bool SendPasswordResetEmail(string userEmailAddress, string newPassword, bool isApi = false)
        {
            EmailTemplate emt = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.PasswordReset);

            Dictionary<string, string> replace = new Dictionary<string, string>();
            string bodyText = "To change it please login and go to <a href=\"" + ServerURL + "Account/ChangePassword\" style=\"color: #ff7500;text-decoration: none;\">" + ServerURL + "Account/ChangePassword</a>";
                
            if (isApi)
                bodyText = "Please login with new password";
            replace.Add("contactEmail", DefaultContactEmailAddress);            
            replace.Add("newPassword", newPassword);
            replace.Add("bodyText", bodyText);
            replace.Add("serverURL", ServerURL);

            replace.Add("OfficeAddress", OfficeAddress);

            string body = SupperClub.Code.Converter.ReplaceTemplate(emt.Body, replace);

            return SendMail(emailToSendTo(userEmailAddress), emt.Subject, body, emt.Html);
        }
        public bool SendAdminNewSupperClubRegisteredEmail(string userName, string userEmail, string supperClubName)
        {
            EmailTemplate emt = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.SupperClubRegistered);

            Dictionary<string, string> replace = new Dictionary<string, string>();
            replace.Add("serverURL", ServerURL);
            replace.Add("userName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(userName));
            replace.Add("userEmail", userEmail);
            replace.Add("contactEmail", DefaultContactEmailAddress);
            replace.Add("time", DateTime.Now.ToString("ddd, d MMM yyyy") + " " + DateTime.Now.ToShortTimeString());
            replace.Add("grubClubName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(supperClubName));

            replace.Add("OfficeAddress", OfficeAddress);

            string body = SupperClub.Code.Converter.ReplaceTemplate(emt.Body, replace);

            return SendMail(emailToSendTo(DefaultAdminEmailAddress), emt.Subject, body, emt.Html);
        }
        public bool SendEmailToGuestInvitee(string emailAddress, int eventId, Guid userId, int numberOfGuests, int seatingId)
        {
            Event e = null;
            User user = null;
            bool guestSuccess = false;
            if (eventId != 0)
                e = _supperClubRepository.GetEvent(eventId);
            else
                e = _supperClubRepository.GetFirstActiveEvent(); // for email testing


            user = _supperClubRepository.GetUser(userId);

            EmailTemplate emtGuest = null;
            emtGuest = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.GuestInvitee);
            
            Dictionary<string, string> replace = new Dictionary<string, string>();
            replace.Add("serverURL", ServerURL);
            replace.Add("contactEmail", DefaultContactEmailAddress);
            replace.Add("UserName", (user.FirstName == null ? "": user.FirstName) + " " + (user.LastName == null ? "": user.LastName));
            replace.Add("grubClubName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(e.SupperClub.Name));
            replace.Add("eventName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(e.Name));
            replace.Add("eventId", e.Id.ToString());
            replace.Add("eventUrlFriendlyUrl", e.UrlFriendlyName);
            // Alcohol Policy
            string policy = "Guests" + (e.Alcohol ? " can " : " CANNOT ") + " bring their own alcohol";
            replace.Add("alcoholPolicy", policy);
            // Event Location
            replace.Add("eventAddress", e.Address);
            replace.Add("eventAddress2", e.Address2);
            replace.Add("eventCity", e.City);
            replace.Add("eventPostCode", e.PostCode);
            replace.Add("eventAddressMap", (e.Address != null && e.Address != string.Empty) ? e.Address.Replace("&", "and") : "");
            replace.Add("eventAddress2Map", (e.Address2 != null && e.Address2 != string.Empty) ? e.Address2.Replace("&", "and") : "");
            
            string eventStartTime = e.Start.ToString("h:mm tt");
            string eventEndTime = e.End.ToString("h:mm tt");
            string eventDateTime = e.Start.ToString("ddd, d MMM yyyy") + " : " + e.Start.ToShortTimeString() + " - " + e.End.ToShortTimeString();
            string eventDate = e.Start.ToString("ddd, d MMM yyyy");
            
            if (e.MultiSeating)
            {
                var seatingTime = (from s in e.EventSeatings
                                   where s.Id == seatingId
                                   select new
                                   {
                                       start = s.Start,
                                       end = s.End,
                                   }).FirstOrDefault();


                if (seatingTime != null)
                {
                    eventDateTime = e.Start.ToString("ddd, d MMM yyyy") + " : " + seatingTime.start.ToShortTimeString() + " - " + seatingTime.end.ToShortTimeString();
                    if (e.WalkIn)
                    {
                        eventStartTime = seatingTime.start.ToString("h:mm tt");
                        eventEndTime = seatingTime.end.ToString("h:mm tt");
                    }
                }
            }           

            replace.Add("eventDate", e.WalkIn ? eventDate : eventDateTime);
            replace.Add("eventStartTime", eventStartTime);
            replace.Add("eventEndTime", eventEndTime);
            replace.Add("userName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(user.FirstName + " " + user.LastName));
            if (numberOfGuests > 1)
            {

                replace.Add("numberTickets", numberOfGuests.ToString() + " Tickets");
                replace.Add("numberPeople", numberOfGuests.ToString() + " People");
            }
            else
            {

                replace.Add("numberPeople", numberOfGuests.ToString() + " Person");
                replace.Add("numberTickets", numberOfGuests.ToString() + " Ticket");
            }
            replace.Add("quantity", numberOfGuests.ToString());

            replace.Add("OfficeAddress", OfficeAddress);

            replace.Add("OfficeAddress", OfficeAddress);

            string bodyGuest = SupperClub.Code.Converter.ReplaceTemplate(emtGuest.Body, replace);
            guestSuccess = SendMail(emailAddress, emtGuest.Subject, bodyGuest, emtGuest.Html);
            return guestSuccess;
        }
        public bool SendGuestBookedEmails(string userName, string userEmailAddress, int numberTickets, decimal totalPaid, int bookingReference, string bookingRequirements, int eventId, int seatingId, List<SupperClub.Models.BookingMenuModel> bmm, int voucherId, string ccLastDigits, decimal commission, ref bool hostSuccess)
        {
            Event e = null;
            
            if (eventId != 0)
                e = _supperClubRepository.GetEvent(eventId);
            else
                e = _supperClubRepository.GetFirstActiveEvent(); // for email testing

            string eventStartTime = e.Start.ToString("h:mm tt");
            string eventEndTime = e.End.ToString("h:mm tt");
            string eventDateTime = e.Start.ToString("ddd, d MMM yyyy") + " : " + e.Start.ToShortTimeString() + " - " + e.End.ToShortTimeString();
            string eventDate = e.Start.ToString("ddd, d MMM yyyy");
            string calStartDateTime = e.Start.ToString("yyyyMMdd") + "T" + e.Start.ToString("HHmmss") + "Z";
            string calEndDateTime = e.End.ToString("yyyyMMdd") + "T" + e.End.ToString("HHmmss") + "Z";
            string eventUrl = ServerURL + e.UrlFriendlyName + "/" + e.Id.ToString();
            bool dayLightSavingOn = false;
            TimeZoneInfo LondonTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            if (LondonTimeZone.IsDaylightSavingTime(DateTime.Now))
                dayLightSavingOn = true;
            if (dayLightSavingOn)
            {
                DateTime offsetStartTime = e.Start.AddHours(-1);
                DateTime offsetEndTime = e.End.AddHours(-1);
                calStartDateTime = offsetStartTime.ToString("yyyyMMdd") + "T" + offsetStartTime.ToString("HHmmss") + "Z";
                calEndDateTime = offsetEndTime.ToString("yyyyMMdd") + "T" + offsetEndTime.ToString("HHmmss") + "Z";
            }
            
            if (seatingId > 0)
            {
                var seatingTime = (from s in e.EventSeatings
                               where s.Id == seatingId
                               select new { start = s.Start, end = s.End }).First();
                if (seatingTime != null)
                {
                    eventDateTime = e.Start.ToString("ddd, d MMM yyyy") + " : " + seatingTime.start.ToShortTimeString() + " - " + seatingTime.end.ToShortTimeString();
                    if (dayLightSavingOn)
                    {
                        DateTime offsetStartTime = seatingTime.start.AddHours(-1);
                        DateTime offsetEndTime = seatingTime.end.AddHours(-1);
                        calStartDateTime = offsetStartTime.ToString("yyyyMMdd") + "T" + offsetStartTime.ToString("HHmmss") + "Z";
                        calEndDateTime = offsetEndTime.ToString("yyyyMMdd") + "T" + offsetEndTime.ToString("HHmmss") + "Z";
                    }
                    else
                    {
                        calStartDateTime = seatingTime.start.ToString("yyyyMMdd") + "T" + seatingTime.start.ToString("HHmmss") + "Z";
                        calEndDateTime = seatingTime.end.ToString("yyyyMMdd") + "T" + seatingTime.end.ToString("HHmmss") + "Z";
                    }
                    if (e.WalkIn)
                    {
                        eventStartTime = seatingTime.start.ToString("h:mm tt");
                        eventEndTime = seatingTime.end.ToString("h:mm tt");
                    }
                }
            }
            string menuRows = "";
            string menuUserName = "";
            string menutitle = "";
            string numberoftickets = "";
            string totalCost = "";
            string menuItems = "";
            string vouchercode = "";

            if (voucherId != null && voucherId > 0)
            {
                Voucher voucher = _supperClubRepository.GetVoucherDetail(voucherId);
                vouchercode = "<p class=\"text--middle opacity-7 mb\" style=\"color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 16px !important; opacity: 0.7; margin: 0 0 20px; padding: 0;\" align=\"left\">Voucher Code:"+ voucher.Code+"</p>";

            }
            if(bmm != null && bmm.Count > 0)
            {
                
                foreach(SupperClub.Models.BookingMenuModel objBmm in bmm)
                {
                    if (objBmm.numberOfTickets > 0)
                    {
                        menuItems += "<p class=\"text--normal clr-gray\" style=\"color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;\" align=\"left\">" + objBmm.numberOfTickets.ToString() + " - " + SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(objBmm.menuTitle).Replace("£", "&pound;") + "</p>";
                        
                        menuRows += "<tr><td colspan=\"2\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Helvetica', 'Arial', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; border-right-width: 1px; border-right-color: #FFCE6F; border-right-style: solid; margin: 0; padding: 0px 0px 10px 10px;\" align=\"left\" valign=\"top\">"
                            + SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(objBmm.menuTitle).Replace("£", "&pound;")
                            + "</td><td style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Helvetica', 'Arial', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; border-right-width: 1px; border-right-color: #FFCE6F; border-right-style: solid; margin: 0; padding: 0px 0px 10px 10px;\" align=\"left\" valign=\"top\">"
                            + objBmm.numberOfTickets.ToString()
                            + "</td><td style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Helvetica', 'Arial', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; border-right-width: 1px; border-right-color: #FFCE6F; border-right-style: solid; margin: 0; padding: 0px 0px 10px 10px;\" align=\"left\" valign=\"top\">" + ApplicationCurrencyHtmlSymbol
                            + ((objBmm.numberOfTickets * SupperClub.Domain.CostCalculator.CostToGuest(objBmm.baseTicketCost, commission)) - objBmm.discount).ToString("0.00") + "</td></tr>";

                        menuUserName += "<tr style=\"vertical-align: top; text-align: left; font-family: 'Source Sans Pro', sans-serif !important; padding: 0;\" align=\"left\">"
                                                    +"    <td class=\"pt- pl\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;\" align=\"left\" valign=\"top\">"
                                                     +"       <p class=\"clr--gray text--normal\" style=\"color: #a9a9a8 !important; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;\" align=\"left\">"+SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(userName)+" </p>"
                                                     +"   </td>"
                                                     +"   <td class=\"expander\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;\" align=\"left\" valign=\"top\"></td>"
                                                   +" </tr>";

                        menutitle += "<tr style=\"vertical-align: top; text-align: left; font-family: 'Source Sans Pro', sans-serif !important; padding: 0;\" align=\"left\">"
                                                                            + "<td class=\"pt- pl\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;\" align=\"left\" valign=\"top\">"
                                                                             + "<p class=\"clr--gray text--normal \" style=\"color: #a9a9a8 !important; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;\" align=\"left\">"+SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(objBmm.menuTitle).Replace("£", "&pound;")+"</p>"
                                                                              + "</a>"
                                                                              + "</td>"
                                                                               + "<td class=\"expander\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;\" align=\"left\" valign=\"top\"></td>"
                                                                            + "</tr>";
                        numberoftickets +="<tr style=\"vertical-align: top; text-align: left; font-family: 'Source Sans Pro', sans-serif !important; padding: 0;\" align=\"left\">"
                                                      + " <td class=\"pt- pl\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;\" align=\"left\" valign=\"top\">"
                                                       + "     <p class=\"clr--gray text--normal\" style=\"color: #a9a9a8 !important; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;\" align=\"left\">" + objBmm.numberOfTickets.ToString() + "</p>"
                                                       +" </td>"
                                                      +"  <td class=\"expander\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;\" align=\"left\" valign=\"top\"></td>"
                                                   +" </tr>";

                        totalCost +="<tr style=\"vertical-align: top; text-align: left; font-family: 'Source Sans Pro', sans-serif !important; padding: 0;\" align=\"left\">"
                                         +"<td class=\"pt- pl\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;\" align=\"left\" valign=\"top\">"
                                          + "<p class=\"clr--gray text--normal\" style=\"color: #a9a9a8 !important; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;\" align=\"left\">" + ((SupperClub.Domain.CostCalculator.CostToGuest(objBmm.baseTicketCost, commission)) - objBmm.discount).ToString("0.00") + "</p>"
                                            +"</td>"
                                              +"<td class=\"expander\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;\" align=\"left\" valign=\"top\"></td>"
                                                +"</tr>";

                    }
                }
                
            }
            Dictionary<string, string> replace = new Dictionary<string, string>();
            
            replace.Add("serverURL", ServerURL);
            replace.Add("contactEmail", DefaultContactEmailAddress);

            // Event Details
            replace.Add("grubClubName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(e.SupperClub.Name));
            replace.Add("eventName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(e.Name));
            replace.Add("eventUrl", eventUrl);
            replace.Add("eventDate", e.WalkIn ? eventDate: eventDateTime);
            replace.Add("eventDateTime", eventDateTime);
            replace.Add("eventStartTime", eventStartTime);
            replace.Add("eventEndTime", eventEndTime);
            replace.Add("eventImagUrl", e.ImagePath);
            replace.Add("calStartDateTime", calStartDateTime);
            replace.Add("calEndDateTime", calEndDateTime);
            replace.Add("comments", "");
            // Alcohol Policy
            string policy = "Guests" + (e.Alcohol ? " can " : " CANNOT ") + " bring their own alcohol";
            replace.Add("alcoholPolicy", policy);
            // Event Location
            replace.Add("eventAddress", e.Address);
            replace.Add("eventAddress2", e.Address2);
            replace.Add("eventCity", e.City);
            replace.Add("eventPostCode", e.PostCode);
            replace.Add("eventAddressMap", (e.Address != null && e.Address !=string.Empty)? e.Address.Replace("&","and") : "");
            replace.Add("eventAddress2Map", (e.Address2 != null && e.Address2 != string.Empty) ? e.Address2.Replace("&", "and") : "");
            // Special Requirements
            if (string.IsNullOrEmpty(bookingRequirements))
                bookingRequirements = "None";
            replace.Add("bookingRequirements", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(bookingRequirements));
            // Receipt
            replace.Add("receiptDate", DateTime.Now.ToString());
            replace.Add("bookingReference", bookingReference.ToString("00000000"));
            replace.Add("userName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(userName));
            if (numberTickets > 1)
            {

                replace.Add("numberTickets", numberTickets.ToString() + " Tickets");
                replace.Add("numberPeople", numberTickets.ToString() + " People");
            }
            else
            {

                replace.Add("numberPeople", numberTickets.ToString() + " Person");
                replace.Add("numberTickets", numberTickets.ToString() + " Ticket");
            }
            replace.Add("quantity", numberTickets.ToString());
            replace.Add("currency", ApplicationCurrencyHtmlSymbol);
            replace.Add("totalPaid", totalPaid.ToString("0.00"));
            replace.Add("menuRows", menuRows);
            replace.Add("menuUserName", menuUserName);
            replace.Add("menutitle", menutitle);
            replace.Add("numberoftickets", numberoftickets);
            replace.Add("totalCost", totalCost);
            replace.Add("menuItems", menuItems);
            replace.Add("vouchercode", vouchercode);
            replace.Add("eventCost", e.CostToGuest.ToString());
            replace.Add("imageUrl", e.ImagePath);
            replace.Add("ccLastDigits", ccLastDigits);
            replace.Add("hostUrl", e.SupperClub.UrlFriendlyName);


           
            string byob = "";
            if(e.Alcohol == true)
                 byob= " <p class=\"text--normal clr-gray mb\" style=\"color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0 0 20px; padding: 0;\" align=\"left\">"

                      +"  Bring your own bottle</p>";
            replace.Add("byob",byob);

            replace.Add("OfficeAddress", OfficeAddress);

            // Email the Guest
            EmailTemplate emtGuest = null;
            if (!e.WalkIn)
            {
                if(e.MultiMenuOption)
                    emtGuest = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.BookingConfirmedGuestMultiMenu);
                else
                    emtGuest = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.BookingConfirmedGuest);
            }
            else
            {
                if (e.MultiMenuOption)
                    emtGuest = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.BookingConfirmedGuestWalkInMultiMenu);
                else
                    emtGuest = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.BookingConfirmedGuestWalkIn);
            }

            string bodyGuest = SupperClub.Code.Converter.ReplaceTemplate(emtGuest.Body, replace);
            bool guestSuccess = SendMail(emailToSendTo(userEmailAddress), emtGuest.Subject, bodyGuest, emtGuest.Html);
            
            // Email the Host
            hostSuccess = SendHostBookingEmail(userEmailAddress, numberTickets, bookingReference, bookingRequirements, eventId, eventDateTime);

            return guestSuccess;
        }

        public bool SendGuestBookedEmailsToFriends(string friendmessage, string userName, string userEmailAddress, int numberTickets, decimal totalPaid, int bookingReference, string bookingRequirements, int eventId, int seatingId, List<SupperClub.Models.BookingMenuModel> bmm, int voucherId, string ccLastDigits, decimal commission, ref bool hostSuccess)
        {
            Event e = null;

            if (eventId != 0)
                e = _supperClubRepository.GetEvent(eventId);
            else
                e = _supperClubRepository.GetFirstActiveEvent(); // for email testing

            string eventStartTime = e.Start.ToString("h:mm tt");
            string eventEndTime = e.End.ToString("h:mm tt");
            string eventDateTime = e.Start.ToString("ddd, d MMM yyyy") + " : " + e.Start.ToShortTimeString() + " - " + e.End.ToShortTimeString();
            string eventDate = e.Start.ToString("ddd, d MMM yyyy");
            string calStartDateTime = e.Start.ToString("yyyyMMdd") + "T" + e.Start.ToString("HHmmss") + "Z";
            string calEndDateTime = e.End.ToString("yyyyMMdd") + "T" + e.End.ToString("HHmmss") + "Z";
            string eventUrl = ServerURL + e.UrlFriendlyName + "/" + e.Id.ToString();
            bool dayLightSavingOn = false;
            TimeZoneInfo LondonTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            if (LondonTimeZone.IsDaylightSavingTime(DateTime.Now))
                dayLightSavingOn = true;
            if (dayLightSavingOn)
            {
                DateTime offsetStartTime = e.Start.AddHours(-1);
                DateTime offsetEndTime = e.End.AddHours(-1);
                calStartDateTime = offsetStartTime.ToString("yyyyMMdd") + "T" + offsetStartTime.ToString("HHmmss") + "Z";
                calEndDateTime = offsetEndTime.ToString("yyyyMMdd") + "T" + offsetEndTime.ToString("HHmmss") + "Z";
            }

            if (seatingId > 0)
            {
                var seatingTime = (from s in e.EventSeatings
                                   where s.Id == seatingId
                                   select new { start = s.Start, end = s.End }).First();
                if (seatingTime != null)
                {
                    eventDateTime = e.Start.ToString("ddd, d MMM yyyy") + " : " + seatingTime.start.ToShortTimeString() + " - " + seatingTime.end.ToShortTimeString();
                    if (dayLightSavingOn)
                    {
                        DateTime offsetStartTime = seatingTime.start.AddHours(-1);
                        DateTime offsetEndTime = seatingTime.end.AddHours(-1);
                        calStartDateTime = offsetStartTime.ToString("yyyyMMdd") + "T" + offsetStartTime.ToString("HHmmss") + "Z";
                        calEndDateTime = offsetEndTime.ToString("yyyyMMdd") + "T" + offsetEndTime.ToString("HHmmss") + "Z";
                    }
                    else
                    {
                        calStartDateTime = seatingTime.start.ToString("yyyyMMdd") + "T" + seatingTime.start.ToString("HHmmss") + "Z";
                        calEndDateTime = seatingTime.end.ToString("yyyyMMdd") + "T" + seatingTime.end.ToString("HHmmss") + "Z";
                    }
                    if (e.WalkIn)
                    {
                        eventStartTime = seatingTime.start.ToString("h:mm tt");
                        eventEndTime = seatingTime.end.ToString("h:mm tt");
                    }
                }
            }
            string menuRows = "";
            string menuUserName = "";
            string menutitle = "";
            string numberoftickets = "";
            string totalCost = "";
            string menuItems = "";
            string vouchercode = "";
            
            if (voucherId != null && voucherId > 0)
            {
                Voucher voucher = _supperClubRepository.GetVoucherDetail(voucherId);
                vouchercode = "<p class=\"text--middle opacity-7 mb\" style=\"color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 16px !important; opacity: 0.7; margin: 0 0 20px; padding: 0;\" align=\"left\"> Voucher Code:" + voucher.Code + "</p>";

            }
            if (bmm != null && bmm.Count > 0)
            {
                
                foreach (SupperClub.Models.BookingMenuModel objBmm in bmm)
                {
                    if (objBmm.numberOfTickets > 0)
                    {
                        menuItems += "<p class=\"text--normal clr-gray\" style=\"color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;\" align=\"left\">" + objBmm.numberOfTickets.ToString() + " - " + SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(objBmm.menuTitle).Replace("£", "&pound;") + "</p>";

                        menuRows += "<tr><td colspan=\"2\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Helvetica', 'Arial', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; border-right-width: 1px; border-right-color: #FFCE6F; border-right-style: solid; margin: 0; padding: 0px 0px 10px 10px;\" align=\"left\" valign=\"top\">"
                            + SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(objBmm.menuTitle).Replace("£", "&pound;")
                            + "</td><td style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Helvetica', 'Arial', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; border-right-width: 1px; border-right-color: #FFCE6F; border-right-style: solid; margin: 0; padding: 0px 0px 10px 10px;\" align=\"left\" valign=\"top\">"
                            + objBmm.numberOfTickets.ToString()
                            + "</td><td style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Helvetica', 'Arial', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; border-right-width: 1px; border-right-color: #FFCE6F; border-right-style: solid; margin: 0; padding: 0px 0px 10px 10px;\" align=\"left\" valign=\"top\">" + ApplicationCurrencyHtmlSymbol
                            + ((objBmm.numberOfTickets * SupperClub.Domain.CostCalculator.CostToGuest(objBmm.baseTicketCost, commission)) - objBmm.discount).ToString("0.00") + "</td></tr>";


                        menuUserName += "<tr style=\"vertical-align: top; text-align: left; font-family: 'Source Sans Pro', sans-serif !important; padding: 0;\" align=\"left\">"
                                                    + "    <td class=\"pt- pl\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;\" align=\"left\" valign=\"top\">"
                                                     + "       <p class=\"clr--gray text--normal\" style=\"color: #a9a9a8 !important; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;\" align=\"left\">" + SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(userName) + " </p>"
                                                     + "   </td>"
                                                     + "   <td class=\"expander\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;\" align=\"left\" valign=\"top\"></td>"
                                                   + " </tr>";

                        menutitle += "<tr style=\"vertical-align: top; text-align: left; font-family: 'Source Sans Pro', sans-serif !important; padding: 0;\" align=\"left\">"
                                                                            + "<td class=\"pt- pl\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;\" align=\"left\" valign=\"top\">"
                                                                             + "<p class=\"clr--gray text--normal \" style=\"color: #a9a9a8 !important; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;\" align=\"left\">" + SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(objBmm.menuTitle).Replace("£", "&pound;") + "</p>"
                                                                              + "</a>"
                                                                              + "</td>"
                                                                               + "<td class=\"expander\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;\" align=\"left\" valign=\"top\"></td>"
                                                                            + "</tr>";
                        numberoftickets += "<tr style=\"vertical-align: top; text-align: left; font-family: 'Source Sans Pro', sans-serif !important; padding: 0;\" align=\"left\">"
                                                      + " <td class=\"pt- pl\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;\" align=\"left\" valign=\"top\">"
                                                       + "     <p class=\"clr--gray text--normal\" style=\"color: #a9a9a8 !important; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;\" align=\"left\">" + objBmm.numberOfTickets.ToString() + "</p>"
                                                       + " </td>"
                                                      + "  <td class=\"expander\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;\" align=\"left\" valign=\"top\"></td>"
                                                   + " </tr>";

                        totalCost += "<tr style=\"vertical-align: top; text-align: left; font-family: 'Source Sans Pro', sans-serif !important; padding: 0;\" align=\"left\">"
                                         + "<td class=\"pt- pl\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;\" align=\"left\" valign=\"top\">"
                                          + "<p class=\"clr--gray text--normal\" style=\"color: #a9a9a8 !important; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;\" align=\"left\">" + ((SupperClub.Domain.CostCalculator.CostToGuest(objBmm.baseTicketCost, commission)) - objBmm.discount).ToString("0.00") + "</p>"
                                            + "</td>"
                                              + "<td class=\"expander\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;\" align=\"left\" valign=\"top\"></td>"
                                                + "</tr>";
                    }
                }
                
            }
            Dictionary<string, string> replace = new Dictionary<string, string>();
            replace.Add("serverURL", ServerURL);
            replace.Add("contactEmail", DefaultContactEmailAddress);
            replace.Add("OfficeAddress", OfficeAddress);
            // Event Details
            replace.Add("grubClubName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(e.SupperClub.Name));
            replace.Add("eventName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(e.Name));
            replace.Add("eventUrl", eventUrl);
            replace.Add("eventDate", e.WalkIn ? eventDate : eventDateTime);
            replace.Add("eventDateTime", eventDateTime);
            replace.Add("eventStartTime", eventStartTime);
            replace.Add("eventEndTime", eventEndTime);
            replace.Add("calStartDateTime", calStartDateTime);
            replace.Add("calEndDateTime", calEndDateTime);
            replace.Add("comments", "");
            // Alcohol Policy
            string policy = "Guests" + (e.Alcohol ? " can " : " CANNOT ") + " bring their own alcohol";
            replace.Add("alcoholPolicy", policy);
            // Event Location
            replace.Add("eventAddress", e.Address);
            replace.Add("eventAddress2", e.Address2);
            replace.Add("eventCity", e.City);
            replace.Add("eventPostCode", e.PostCode);
            replace.Add("eventAddressMap", (e.Address != null && e.Address != string.Empty) ? e.Address.Replace("&", "and") : "");
            replace.Add("eventAddress2Map", (e.Address2 != null && e.Address2 != string.Empty) ? e.Address2.Replace("&", "and") : "");
            // Special Requirements
            if (string.IsNullOrEmpty(bookingRequirements))
                bookingRequirements = "None";
            replace.Add("bookingRequirements", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(bookingRequirements));
            string bookingEventType = "";
            if (e.WalkIn)
            {
                bookingEventType = "<p class=\"mb\" style=\"color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: center; line-height: 19px; font-size: 14px; margin-left:40px;\" align=\"center\">Feel free to arrive anytime from " + eventStartTime + ". The venue closes at " + eventEndTime + ".</p>";
            }
            else
            {
                bookingEventType = "<p class=\"mb\" style=\"color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: center; line-height: 19px; font-size: 14px; margin-left:60px;\" align=\"center\">Please be prompt, arriving early or late can affect everyone's dining experience.</p>";
            }
            replace.Add("bookingEventType", bookingEventType);
            // Receipt
            replace.Add("receiptDate", DateTime.Now.ToString());
            replace.Add("bookingReference", bookingReference.ToString("00000000"));
            replace.Add("userName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(userName));
            if (numberTickets > 1)
            {

                replace.Add("numberTickets", numberTickets.ToString() + " Tickets");
                replace.Add("numberPeople", numberTickets.ToString() + " People");
            }
            else
            {

                replace.Add("numberPeople", numberTickets.ToString() + " Person");
                replace.Add("numberTickets", numberTickets.ToString() + " Ticket");
            }

            replace.Add("quantity", numberTickets.ToString());
            replace.Add("currency", ApplicationCurrencyHtmlSymbol);
            replace.Add("totalPaid", totalPaid.ToString("0.00"));
            replace.Add("menuRows", menuRows);
            replace.Add("menuUserName", menuUserName);
            replace.Add("menutitle", menutitle);
            replace.Add("numberoftickets", numberoftickets);
            replace.Add("totalCost", totalCost);
            replace.Add("menuItems", menuItems);
            replace.Add("imageUrl", e.ImagePath);
            replace.Add("ccLastDigits", ccLastDigits);
            replace.Add("hostUrl", e.SupperClub.UrlFriendlyName);
            replace.Add("vouchercode", vouchercode);
            replace.Add("eventCost", e.CostToGuest.ToString());
            string message = "<p class=\"semibold text--large mb-\" style=\"color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: 600 !important; text-align: left; line-height: 19px; font-size: 18px !important; margin: 0 0 10px; padding: 0;\" align=\"left\">Message from " + userName + ":</p>";
            if (!string.IsNullOrEmpty(friendmessage))
            {

                message=message + "<p class=\"light text--large mb\" style=\"color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: 300 !important; text-align: left; line-height: 19px; font-size: 18px !important; margin: 0 0 20px; padding: 0;\" align=\"left\">" + friendmessage + "</p>";
                replace.Add("messageFromFriend", message);
            }
            else
            {
                replace.Add("messageFromFriend", message);
            }

            string byob = "";
            if (e.Alcohol == true)
                byob = " <p class=\"text--normal clr-gray mb\" style=\"color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0 0 20px; padding: 0;\" align=\"left\">"

                     + "  Bring your own bottle</p>";
            replace.Add("byob", byob);
           

            // Email the Guest
            EmailTemplate emtFriend = null;
            emtFriend = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.BookingConfirmationToFriends);
            //if (!e.WalkIn)
            //{
            //    if (e.MultiMenuOption)
            //        emtGuest = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.BookingConfirmedGuestMultiMenu);
            //    else
            //        emtGuest = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.BookingConfirmedGuest);
            //}
            //else
            //{
            //    if (e.MultiMenuOption)
            //        emtGuest = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.BookingConfirmedGuestWalkInMultiMenu);
            //    else
            //        emtGuest = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.BookingConfirmedGuestWalkIn);
            //}

            string bodyGuest = SupperClub.Code.Converter.ReplaceTemplate(emtFriend.Body, replace);
            bool guestSuccess = SendMail(emailToSendTo(userEmailAddress), emtFriend.Subject, bodyGuest, emtFriend.Html);

            // Email the Host
           // hostSuccess = SendHostBookingEmail(userEmailAddress, numberTickets, bookingReference, bookingRequirements, eventId, eventDateTime);

            return guestSuccess;
        }

        public bool SendHostBookingEmail(string userName, int numberTickets, int bookingReference, string bookingRequirements, int eventId, string seatingTime)
        {
            Event e = null;
            if (eventId != 0)
                e = _supperClubRepository.GetEvent(eventId);
            else
                e = _supperClubRepository.GetFirstActiveEvent(); // for email testing

            Dictionary<string, string> replace = new Dictionary<string, string>();
            replace.Add("serverURL", ServerURL);
            replace.Add("contactEmail", DefaultContactEmailAddress);
            // Event Details
            replace.Add("grubClubName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(e.SupperClub.Name));
            replace.Add("userName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(userName));
            replace.Add("eventId", e.Id.ToString());
            replace.Add("eventName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(e.Name));
            replace.Add("eventDate", seatingTime);
            replace.Add("numberTickets", numberTickets.ToString());
            replace.Add("bookingReference", bookingReference.ToString("00000000"));

            replace.Add("fname", e.SupperClub.User.FirstName);
            replace.Add("OfficeAddress", OfficeAddress);

            // Special Requirements
            replace.Add("bookingRequirements", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(bookingRequirements));

            // Email the Host
            string hostEmailAddress = e.SupperClub.User.aspnet_Users.aspnet_Membership.Email;
            EmailTemplate emtHost = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.BookingConfirmedHost);
            string bodyHost = SupperClub.Code.Converter.ReplaceTemplate(emtHost.Body, replace);
            bool hostSuccess = false;
            if (SendEmailToHost)
                hostSuccess = SendMail(emailToSendTo(hostEmailAddress), emtHost.Subject, bodyHost, emtHost.Html);
            else
                hostSuccess = true;

            try
            {
                if (SendHostTestEmail)
                    SendMail(DefaultContactEmailAddress, emtHost.Subject, bodyHost, emtHost.Html);
            }
            catch
            {
                
            }

            // Email Admin if there were special requirements
            if (!string.IsNullOrEmpty(bookingRequirements))
                SendMail(emailToSendTo(DefaultAdminEmailAddress), emtHost.Subject, bodyHost, emtHost.Html);

            return hostSuccess;
        }
        
        public bool SendGuestRejectionEmail(string userName, string userEmailAddress, string supperClubName, string eventName, string eventDate)
        {
            EmailTemplate emt = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.GuestRefused);

            Dictionary<string, string> replace = new Dictionary<string, string>();
            replace.Add("userName", userEmailAddress);
            replace.Add("serverURL", ServerURL);
            replace.Add("contactEmail", DefaultContactEmailAddress);
            replace.Add("supperClubName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(supperClubName));
            replace.Add("eventName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(eventName));
            replace.Add("eventDate", eventDate);

            replace.Add("OfficeAddress", OfficeAddress);
            string body = SupperClub.Code.Converter.ReplaceTemplate(emt.Body, replace);
            // Notify an Admin too. Use different email?
            return SendMail(emailToSendTo(DefaultAdminEmailAddress), "(Admin) " + emt.Subject, body, emt.Html);
            // Temporarily only email Admin
            // return SendMail(emailToSendTo(userEmailAddress), emt.Subject, body, emt.Html);
        }

        public bool SendEventCancelledEmails(List<string> userEmailAddresses, string supperClubName, string eventName, string eventDate, string cancellationMessage)
        {
            EmailTemplate emt = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.EventCancelled);

            Dictionary<string, string> replace = new Dictionary<string, string>();
            replace.Add("serverURL", ServerURL);
            replace.Add("contactEmail", DefaultContactEmailAddress);
            replace.Add("supperClubName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(supperClubName));
            replace.Add("eventName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(eventName));
            replace.Add("eventDate", eventDate);
            replace.Add("cancellationMessage", cancellationMessage);


            replace.Add("OfficeAddress", OfficeAddress);
            string body = SupperClub.Code.Converter.ReplaceTemplate(emt.Body, replace);

            // Temporarily only email Admin
            userEmailAddresses.Clear();
            
            return SendMailToMany(emailToSendTo(DefaultAdminEmailAddress), userEmailAddresses, emt.Subject, body, emt.Html);
        }

        public bool SendWelcomeEmail(string userName, string emailAddress)
        {
            EmailTemplate emt = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.Welcome);

            Dictionary<string, string> replace = new Dictionary<string, string>();
            replace.Add("serverURL", ServerURL);
            replace.Add("contactEmail", DefaultContactEmailAddress);
            replace.Add("userName", userName);
            replace.Add("time", DateTime.Now.ToString("ddd, d MMM yyyy") + DateTime.Now.ToShortTimeString());
            replace.Add("emailAddress", emailAddress);

            replace.Add("OfficeAddress", OfficeAddress);
            string body = SupperClub.Code.Converter.ReplaceTemplate(emt.Body, replace);

            return SendMail(emailToSendTo(emailAddress), emt.Subject, body, emt.Html);
        }

        public bool SendAccountVerificationEmail(string firstName, string emailAddress, string linktext)
        {
            EmailTemplate emt = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.AccountVerification);

            string verificationLink = ServerURL + "account/verifyaccount?" + linktext;
            Dictionary<string, string> replace = new Dictionary<string, string>();
            replace.Add("serverURL", ServerURL);
            replace.Add("contactEmail", DefaultContactEmailAddress);
            replace.Add("userName", firstName);
            replace.Add("time", DateTime.Now.ToString("ddd, d MMM yyyy") + DateTime.Now.ToShortTimeString());
            replace.Add("emailAddress", emailAddress);
            replace.Add("verificationLink", verificationLink);

            replace.Add("OfficeAddress", OfficeAddress);

            string body = SupperClub.Code.Converter.ReplaceTemplate(emt.Body, replace);

            return SendMail(emailToSendTo(emailAddress), emt.Subject, body, emt.Html);
        }

        public bool SendHostNewReviewEmail(int eventId, string review, string privateReview)
        {
            Event e = null;
            if (eventId != 0)
                e = _supperClubRepository.GetEvent(eventId);
            else
                e = _supperClubRepository.GetFirstActiveEvent(); // for email testing

            Dictionary<string, string> replace = new Dictionary<string, string>();
            replace.Add("serverURL", ServerURL);
            replace.Add("contactEmail", DefaultContactEmailAddress);
            // Event Details
            replace.Add("userName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(e.SupperClub.User.FirstName));
            replace.Add("eventId", e.Id.ToString());
            replace.Add("eventName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(e.Name));
            replace.Add("eventDate", e.Start.ToString("ddd, d MMM yyyy") + " at " + e.Start.ToShortTimeString());

            replace.Add("REVIEW", review);
            replace.Add("PRIVATEREVIEW", privateReview);
            replace.Add("OfficeAddress", OfficeAddress);

            // Email the Host
            string hostEmailAddress = e.SupperClub.User.aspnet_Users.aspnet_Membership.Email;
            EmailTemplate emtHost = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.NewReviewHost);
            string bodyHost = SupperClub.Code.Converter.ReplaceTemplate(emtHost.Body, replace);
            bool hostSuccess = false;
            if (SendEmailToHost)
                hostSuccess = SendMail(emailToSendTo(hostEmailAddress), emtHost.Subject, bodyHost, emtHost.Html);
            else
                hostSuccess = true;

            try
            {
                if (SendHostTestEmail)
                    SendMail(DefaultContactEmailAddress, emtHost.Subject, bodyHost, emtHost.Html);
            }
            catch
            {
                
            }

            return hostSuccess;
        }

        public bool SendAdminNewReviewEmail(int eventId, decimal? rating, string publicReview, string privateReview)
        {
            Event e = null;
            if (eventId != 0)
                e = _supperClubRepository.GetEvent(eventId);
            else
                e = _supperClubRepository.GetFirstActiveEvent(); // for email testing

            Dictionary<string, string> replace = new Dictionary<string, string>();
            replace.Add("serverURL", ServerURL);
            replace.Add("contactEmail", DefaultContactEmailAddress);
            // Event Details
            replace.Add("grubClubName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(e.SupperClub.Name));
            replace.Add("eventId", e.Id.ToString());
            replace.Add("eventName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(e.Name));
            replace.Add("eventDate", e.Start.ToString("ddd, d MMM yyyy") + " at " + e.Start.ToShortTimeString());

            // Review Details
            replace.Add("rating", (rating == null) ? "None" : rating.ToString());
            replace.Add("publicReview", (string.IsNullOrEmpty(publicReview) ? "None" : publicReview));
            replace.Add("privateReview", (string.IsNullOrEmpty(privateReview) ? "None" : privateReview));

            replace.Add("OfficeAddress", OfficeAddress);
            // Email Admin
            EmailTemplate emt = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.NewReviewAdmin);
            string body = SupperClub.Code.Converter.ReplaceTemplate(emt.Body, replace);
            bool success = SendMail(emailToSendTo(DefaultAdminEmailAddress), emt.Subject, body, emt.Html);

            return success;
        }

        public string SendAutomatedGuestBookedEmails(int eventId, List<User> users, string comments,int voucherId,string ccLastDigits)
        {
            Event e = null;
            bool guestSuccess = false;
            string status = "";
            if (eventId != 0)
                e = _supperClubRepository.GetEvent(eventId);
            else
                e = _supperClubRepository.GetFirstActiveEvent(); // for email testing

            string eventUrl = ServerURL + e.UrlFriendlyName + "/" + e.Id.ToString();

            // Email the Guest
            EmailTemplate emtGuest = null;
            if (!e.WalkIn)
            {
                if (e.MultiMenuOption)
                    emtGuest = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.BookingConfirmedGuestMultiMenu);
                else
                    emtGuest = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.BookingConfirmedGuest);
            }
            else
            {
                if (e.MultiMenuOption)
                    emtGuest = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.BookingConfirmedGuestWalkInMultiMenu);
                else
                    emtGuest = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.BookingConfirmedGuestWalkIn);
            }
            string vouchercode = "";

            if (voucherId != null && voucherId > 0)
            {
                Voucher voucher = _supperClubRepository.GetVoucherDetail(voucherId);
                vouchercode = "<p class=\"text--middle opacity-7 mb\" style=\"color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 16px !important; opacity: 0.7; margin: 0 0 20px; padding: 0;\" align=\"left\">Voucher Code:" + voucher.Code + "</p>";

            }
            Dictionary<string, string> replace = new Dictionary<string, string>();
            replace.Add("serverURL", ServerURL);
            replace.Add("contactEmail", DefaultContactEmailAddress);
            replace.Add("grubClubName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(e.SupperClub.Name));
            replace.Add("eventName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(e.Name));
            replace.Add("eventUrl", eventUrl);
            // Alcohol Policy
            string policy = "Guests" + (e.Alcohol ? " can " : " CANNOT ") + " bring their own alcohol";
            replace.Add("alcoholPolicy", policy);
            // Event Location
            replace.Add("eventAddress", e.Address);
            replace.Add("eventAddress2", e.Address2);
            replace.Add("eventCity", e.City);
            replace.Add("eventPostCode", e.PostCode);
            replace.Add("eventAddressMap", (e.Address != null && e.Address != string.Empty) ? e.Address.Replace("&", "and") : "");
            replace.Add("eventAddress2Map", (e.Address2 != null && e.Address2 != string.Empty) ? e.Address2.Replace("&", "and") : "");
            replace.Add("receiptDate", DateTime.Now.ToString());
            replace.Add("comments", (comments == null || comments.Length ==0) ? "" :("<p style=\"color: #ff7500;margin-top:10px;\"><b> Note: " + comments +"</b> </p>"));

            replace.Add("imageUrl", e.ImagePath);
            
            replace.Add("hostUrl", e.SupperClub.UrlFriendlyName);
           
            
            replace.Add("eventCost", e.CostToGuest.ToString());

            replace.Add("OfficeAddress", OfficeAddress);
            string byob = "";
            if (e.Alcohol == true)
                byob = " <p class=\"text--normal clr-gray mb\" style=\"color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0 0 20px; padding: 0;\" align=\"left\">"

                     + "  Bring your own bottle</p>";
            replace.Add("byob", byob);

            List<UserList> consolidatedGuests = new List<UserList>();
            if (users == null)
            {
                consolidatedGuests = (from ea in e.EventAttendees
                                          group ea by new { ea.UserId, ea.User } into gcs
                                          select new UserList { UserId = (Guid)gcs.Key.UserId, User = (User)gcs.Key.User, numberOfGuests = (int)gcs.Sum(ea => ea.NumberOfGuests) }).ToList();
                
            }
            else
            {
                consolidatedGuests = (from ea in e.EventAttendees
                                          join u in users on ea.UserId equals u.Id
                                          group ea by new { ea.UserId, ea.User } into gcs
                                          select new UserList{ UserId = (Guid)gcs.Key.UserId, User = (User)gcs.Key.User, numberOfGuests = (int)gcs.Sum(ea => ea.NumberOfGuests) }).ToList();
            }
            if (consolidatedGuests.Count() > 0)
            {
                foreach (UserList user in consolidatedGuests)
                {
                    if (user.numberOfGuests > 1)
                    {
                        if (replace.ContainsKey("numberTickets"))
                            replace["numberTickets"] = user.numberOfGuests.ToString() + " Tickets";
                        else
                            replace.Add("numberTickets", user.numberOfGuests.ToString() + " Tickets");

                        if (replace.ContainsKey("numberPeople"))
                            replace["numberPeople"] = user.numberOfGuests.ToString() + " People";
                        else
                            replace.Add("numberPeople", user.numberOfGuests.ToString() + " People");

                    }
                    else
                    {
                        if (replace.ContainsKey("numberTickets"))
                            replace["numberTickets"] = user.numberOfGuests.ToString() + " Ticket";
                        else
                            replace.Add("numberTickets", user.numberOfGuests.ToString() + " Ticket");
                        if (replace.ContainsKey("numberPeople"))
                            replace["numberPeople"] = user.numberOfGuests.ToString() + " Person";
                        else
                            replace.Add("numberPeople", user.numberOfGuests.ToString() + " Person");                        
                    }
                    if (replace.ContainsKey("quantity"))
                        replace["quantity"] = user.numberOfGuests.ToString();
                    else
                        replace.Add("quantity", user.numberOfGuests.ToString());

                    guestSuccess = SendOfflineEmail(replace, e, user, emtGuest);
                    if (guestSuccess)
                        status += user.User.Email + " : Email Sent Successfully.     \n";
                    else
                        status += user.User.Email + " : Failure Sending Email.       \n";
                }
            }
            
            return status;
        }
        public string SendWishlistUsersEmails(int eventId, List<User> users)
        {
            Event e = null;
            bool guestSuccess = false;
            string status = "";
            if (eventId != 0)
                e = _supperClubRepository.GetEvent(eventId);
            else
                e = _supperClubRepository.GetFirstActiveEvent(); // for email testing

            string utmParameters = UtmTagForWishList.Replace("[utmMediumValue]", e.SupperClub.UrlFriendlyName);
            string eventUrl = ServerURL + e.UrlFriendlyName + "/" + e.Id.ToString() + "?" + utmParameters;
            string bookingUrl = eventUrl;
            // Email the Guest
            EmailTemplate emtGuest = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.WishListEventSellingOutNotification);
            
            Dictionary<string, string> replace = new Dictionary<string, string>();
            replace.Add("serverURL", ServerURL);
            replace.Add("contactEmail", DefaultContactEmailAddress);
            replace.Add("bookingUrl", bookingUrl);
            replace.Add("eventName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(e.Name));
            replace.Add("eventUrl", eventUrl);
            replace.Add("eventCost", e.CostToGuest.ToString("0.00"));
            // Event Location
            replace.Add("eventCity", e.City);
            replace.Add("eventPostCode", e.PostCode);
            replace.Add("eventDateTime", e.Start.ToString("ddd, d MMM yyyy") + " : " + e.Start.ToShortTimeString() + " - " + e.End.ToShortTimeString());

            replace.Add("OfficeAddress", OfficeAddress);
            string bodyGuest = SupperClub.Code.Converter.ReplaceTemplate(emtGuest.Body, replace);            
                        
            foreach (User user in users)
                {
                    Dictionary<string, string> replace1 = new Dictionary<string, string>();
                    replace1.Add("userName", user.FirstName);
                    string bodyGuest1 = SupperClub.Code.Converter.ReplaceTemplate(bodyGuest, replace1);            
                    guestSuccess = SendMail(emailToSendTo(user.Email), emtGuest.Subject, bodyGuest1, emtGuest.Html);
                    if (guestSuccess)
                    {
                        status += user.Email + " : Email Sent Successfully.     \n";
                        bool sts = _supperClubRepository.UpdateUsersEmailNotificationForEventBookingReminder(e.Id, user.Id);
                        if (sts)
                            log.Info("BookedEvent: Updated e-mail notification status successfully after sending the email");
                        else
                            log.Error("BookedEvent: Error Updating e-mail notification status after sending the email");
                    }
                    else
                        status += user.Email + " : Failure Sending Email.       \n";
                }          

            return status;
        }

        private bool SendOfflineEmail(Dictionary<string, string> replace, Event e, UserList user, EmailTemplate emtGuest)
        {
            bool guestSuccess = false;
            string eventStartTime = e.Start.ToString("h:mm tt");
            string eventEndTime = e.End.ToString("h:mm tt");
            string eventDateTime = e.Start.ToString("ddd, d MMM yyyy") + " : " + e.Start.ToShortTimeString() + " - " + e.End.ToShortTimeString();
            string eventDate = e.Start.ToString("ddd, d MMM yyyy");
            string calStartDateTime = e.Start.ToString("yyyyMMdd") + "T" + e.Start.ToString("HHmmss") + "Z";
            string calEndDateTime = e.End.ToString("yyyyMMdd") + "T" + e.End.ToString("HHmmss") + "Z";
            string bookingRequirements = "None";
            string bookingReference =  "";
            string menuRows = "";
            string menuUserName = "";
            string menutitle = "";
            string numberoftickets = "";
            string totalCost = "";
            string menuItems = "";
            int voucherId=0;
            string ccLastDigits = "";
            decimal totalpaid=0;
            string vouchercodemm = "";
            string vouchercode = "";
            List<int?> lstVid = new List<int?>();
            int Tickets = 0;

            //hack for showing correct datetime in google calendar
            bool dayLightSavingOn = false;
            TimeZoneInfo LondonTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            if (LondonTimeZone.IsDaylightSavingTime(DateTime.Now))
                dayLightSavingOn = true;
            if (dayLightSavingOn)
            {
                DateTime offsetStartTime = e.Start.AddHours(-1);
                DateTime offsetEndTime = e.End.AddHours(-1);
                calStartDateTime = offsetStartTime.ToString("yyyyMMdd") + "T" + offsetStartTime.ToString("HHmmss") + "Z";
                calEndDateTime = offsetEndTime.ToString("yyyyMMdd") + "T" + offsetEndTime.ToString("HHmmss") + "Z";
            }

            if (e.MultiSeating)
            {
                #region Multi Seating
                #region Get Seatings
                var seatings = (from ea in e.EventAttendees
                                where ea.UserId == user.UserId
                                group ea by new
                                    {
                                        ea.SeatingId
                                    } into gcs
                                select new
                                {
                                    seatingId = gcs.Key.SeatingId,
                                    numberOfGuests = gcs.Sum(ea => ea.NumberOfGuests)
                                });
                #endregion

                foreach (var eas in seatings)
                {
                    var seatingTime = (from s in e.EventSeatings
                                       where s.Id == eas.seatingId
                                       select new { start = s.Start, end = s.End }).First();
                    if (seatingTime != null)
                    {
                        eventDateTime = e.Start.ToString("ddd, d MMM yyyy") + " : " + seatingTime.start.ToShortTimeString() + " - " + seatingTime.end.ToShortTimeString();
                        if (dayLightSavingOn)
                        {
                            DateTime offsetStartTime = seatingTime.start.AddHours(-1);
                            DateTime offsetEndTime = seatingTime.end.AddHours(-1);
                            calStartDateTime = offsetStartTime.ToString("yyyyMMdd") + "T" + offsetStartTime.ToString("HHmmss") + "Z";
                            calEndDateTime = offsetEndTime.ToString("yyyyMMdd") + "T" + offsetEndTime.ToString("HHmmss") + "Z";
                        }
                        else
                        {
                            calStartDateTime = seatingTime.start.ToString("yyyyMMdd") + "T" + seatingTime.start.ToString("HHmmss") + "Z";
                            calEndDateTime = seatingTime.end.ToString("yyyyMMdd") + "T" + seatingTime.end.ToString("HHmmss") + "Z";
                        }

                        if (e.WalkIn)
                        {
                            eventStartTime = seatingTime.start.ToString("h:mm tt");
                            eventEndTime = seatingTime.end.ToString("h:mm tt");
                        }
                    }
                    if (e.MultiMenuOption)
                    {
                        var bmm = (from ea in e.EventAttendees
                                   where ea.UserId == user.UserId && ea.SeatingId == eas.seatingId
                                   group ea by new
                                    {
                                        ea.UserId, ea.SeatingId, ea.MenuOptionId
                                    } into gcs
                                    select new
                                    {
                                        menuId = gcs.Key.MenuOptionId,
                                        numberOfTickets = gcs.Sum(ea => ea.NumberOfGuests)
                                    }).ToList();
                        if (bmm != null)
                        {
                            menuRows = "";
                            
                            foreach (var objBmm in bmm)
                            {
                                if (objBmm.numberOfTickets > 0)
                                {
                                    var bm = (from emo in e.EventMenuOptions
                                              where emo.Id == objBmm.menuId
                                              select new { menuTitle = emo.Title, numberOfTickets = objBmm.numberOfTickets, cost = emo.CostToGuest }).First();

                                    menuItems += "<p class=\"text--normal clr-gray\" style=\"color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;\" align=\"left\">" + bm.numberOfTickets.ToString() + " - " + SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(bm.menuTitle).Replace("£", "&pound;") + "</p>";

                                    menuRows += "<tr><td colspan=\"2\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Helvetica', 'Arial', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; border-right-width: 1px; border-right-color: #FFCE6F; border-right-style: solid; margin: 0; padding: 0px 0px 10px 10px;\" align=\"left\" valign=\"top\">" 
                                        + SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(bm.menuTitle).Replace("£", "&pound;")
                                        + "</td><td style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Helvetica', 'Arial', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; border-right-width: 1px; border-right-color: #FFCE6F; border-right-style: solid; margin: 0; padding: 0px 0px 10px 10px;\" align=\"left\" valign=\"top\">" 
                                        + bm.numberOfTickets.ToString()
                                        + "</td><td style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Helvetica', 'Arial', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; border-right-width: 1px; border-right-color: #FFCE6F; border-right-style: solid; margin: 0; padding: 0px 0px 10px 10px;\" align=\"left\" valign=\"top\">" + ApplicationCurrencyHtmlSymbol
                                        + (objBmm.numberOfTickets * bm.cost).ToString("0.00") + "</td></tr>";

                                    menuUserName += "<tr style=\"vertical-align: top; text-align: left; font-family: 'Source Sans Pro', sans-serif !important; padding: 0;\" align=\"left\">"
                                                   + "    <td class=\"pt- pl\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;\" align=\"left\" valign=\"top\">"
                                                    + "       <p class=\"clr--gray text--normal\" style=\"color: #a9a9a8 !important; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;\" align=\"left\">" + user.User.FirstName + " " + user.User.LastName + " </p>"
                                                    + "   </td>"
                                                    + "   <td class=\"expander\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;\" align=\"left\" valign=\"top\"></td>"
                                                  + " </tr>";

                                    menutitle += "<tr style=\"vertical-align: top; text-align: left; font-family: 'Source Sans Pro', sans-serif !important; padding: 0;\" align=\"left\">"
                                                                                        + "<td class=\"pt- pl\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;\" align=\"left\" valign=\"top\">"
                                                                                         + "<p class=\"clr--gray text--normal \" style=\"color: #a9a9a8 !important; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;\" align=\"left\">" + SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(bm.menuTitle).Replace("£", "&pound;") + "</p>"
                                                                                          + "</a>"
                                                                                          + "</td>"
                                                                                           + "<td class=\"expander\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;\" align=\"left\" valign=\"top\"></td>"
                                                                                        + "</tr>";
                                    numberoftickets += "<tr style=\"vertical-align: top; text-align: left; font-family: 'Source Sans Pro', sans-serif !important; padding: 0;\" align=\"left\">"
                                                                  + " <td class=\"pt- pl\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;\" align=\"left\" valign=\"top\">"
                                                                   + "     <p class=\"clr--gray text--normal\" style=\"color: #a9a9a8 !important; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;\" align=\"left\">" + bm.numberOfTickets.ToString() + "</p>"
                                                                   + " </td>"
                                                                  + "  <td class=\"expander\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;\" align=\"left\" valign=\"top\"></td>"
                                                               + " </tr>";

                                    Tickets += bm.numberOfTickets;

                                    totalCost += "<tr style=\"vertical-align: top; text-align: left; font-family: 'Source Sans Pro', sans-serif !important; padding: 0;\" align=\"left\">"
                                                     + "<td class=\"pt- pl\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;\" align=\"left\" valign=\"top\">"
                                                      + "<p class=\"clr--gray text--normal\" style=\"color: #a9a9a8 !important; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;\" align=\"left\">" + (bm.cost).ToString("0.00") + "</p>"
                                                        + "</td>"
                                                          + "<td class=\"expander\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;\" align=\"left\" valign=\"top\"></td>"
                                                            + "</tr>";
                                    totalpaid += (bm.cost) * bm.numberOfTickets;

                                }
                            }
                            
                        }
                    }
                    lstVid = e.EventAttendees.ToList<EventAttendee>().Select(v => v.VoucherId).Distinct().ToList();

                    foreach (int vid in lstVid)
                    {
                        if (vid > 0)
                        {
                            Voucher voucher = _supperClubRepository.GetVoucherDetail(int.Parse(vid.ToString()));
                            string description = "";
                            if (voucher.TypeId == 1)
                                description = voucher.OffValue.ToString() + "%";
                            else
                                description = ApplicationCurrencyHtmlSymbol + voucher.OffValue;

                            vouchercodemm += "<p class=\"text--middle opacity-7 mb\" style=\"color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; opacity: 0.7; margin: 0 0 20px; padding: 0;\" align=\"left\">Voucher Code:" + voucher.Code + "--" + " Voucher Value:" + description + "</p>";

                        }
                    }




                    var bref = (from t in user.User.Tickets
                                join tb in user.User.TicketBaskets
                                on t.BasketId equals tb.Id
                                where t.EventId == e.Id && t.UserId == user.UserId && t.SeatingId == eas.seatingId
                                select new { bookingReference = tb.BookingReference, bookingRequirements = tb.BookingRequirements, ccLastDigits = tb.CCLastDigits, voucherId = t.VoucherId }).FirstOrDefault();
                    bookingRequirements = bref.bookingRequirements;
                    bookingReference = bref.bookingReference.ToString("00000000");
                   ccLastDigits = bref.ccLastDigits;
                    voucherId = bref.voucherId;

                    
                    if (voucherId != null && voucherId > 0)
                    {
                        Voucher voucher = _supperClubRepository.GetVoucherDetail(voucherId);
                        string description = "";
                        if (voucher.TypeId == 1)
                            description = voucher.OffValue.ToString() + "%";
                        else
                            description = ApplicationCurrencyHtmlSymbol + voucher.OffValue;

                        vouchercode = "<p class=\"text--middle opacity-7 mb\" style=\"color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; opacity: 0.7; margin: 0 0 20px; padding: 0;\" align=\"left\">Voucher Code:" + voucher.Code + "--" + " Voucher Value:" + description + "</p>";

                    }
                    Dictionary<string, string> objReplace = new Dictionary<string, string>();
                    foreach (var kvp in replace)
                        objReplace.Add(kvp.Key, kvp.Value);
                    objReplace.Add("eventDate", e.WalkIn ? eventDate : eventDateTime);
                    objReplace.Add("eventDateTime", eventDateTime);
                    objReplace.Add("eventStartTime", eventStartTime);
                    objReplace.Add("eventEndTime", eventEndTime);
                    objReplace.Add("calStartDateTime", calStartDateTime);
                    objReplace.Add("calEndDateTime", calEndDateTime);
                    // Special Requirements
                    if (string.IsNullOrEmpty(bookingRequirements))
                        bookingRequirements = "None";
                    objReplace.Add("bookingRequirements", bookingRequirements);
                    // Receipt
                    objReplace.Add("bookingReference", bookingReference);
                    objReplace.Add("userName", user.User.FirstName + " " + user.User.LastName);
                 
                    objReplace.Add("currency", ApplicationCurrencyHtmlSymbol);
                    if (e.MultiMenuOption)
                    {
                        objReplace.Add("totalPaid", totalpaid.ToString("0.00"));
                        objReplace.Add("vouchercode", vouchercodemm);
                    }
                    else
                    {
                        objReplace.Add("totalPaid", (user.numberOfGuests * e.CostToGuest).ToString("0.00"));// (eas.numberOfGuests * e.CostToGuest).ToString("0.00"));
                        objReplace.Add("vouchercode", vouchercode);
                    }
                    objReplace.Add("menuRows", menuRows);
                    objReplace.Add("menuUserName", menuUserName);
                    objReplace.Add("menutitle", menutitle);
                    objReplace.Add("numberoftickets", numberoftickets);
                    objReplace.Add("totalCost", totalCost);
                    objReplace.Add("menuItems", menuItems);

                    objReplace.Add("ccLastDigits", ccLastDigits);

                    objReplace.Add("OfficeAddress", OfficeAddress);
                    
                   // objReplace.Add("vouchercode", vouchercode);
                    
                    string bodyGuest = SupperClub.Code.Converter.ReplaceTemplate(emtGuest.Body, objReplace);
                    guestSuccess = SendMail(emailToSendTo(user.User.Email), emtGuest.Subject, bodyGuest, emtGuest.Html);
                }
                #endregion
            }
            else
            {
                if (e.MultiMenuOption)
                {
                    var bmm = (from ea in e.EventAttendees
                               where ea.UserId == user.UserId
                               group ea by new
                               {
                                   ea.UserId,
                                   ea.MenuOptionId
                               } into gcs
                               select new
                               {
                                   menuId = gcs.Key.MenuOptionId,
                                   numberOfTickets = gcs.Sum(ea => ea.NumberOfGuests)
                               }).ToList();
                    if (bmm != null)
                    {
                        menuRows = "";
                        
                        foreach (var objBmm in bmm)
                        {
                            
                            if (objBmm.numberOfTickets > 0)
                            {
                                var bm = (from emo in e.EventMenuOptions
                                          where emo.Id == objBmm.menuId
                                          select new { menuTitle = emo.Title, numberOfTickets = objBmm.numberOfTickets, cost = emo.CostToGuest }).First();
                                menuItems += "<p class=\"text--normal clr-gray\" style=\"color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;\" align=\"left\">" + bm.numberOfTickets.ToString() + " - " + SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(bm.menuTitle).Replace("£", "&pound;") + "</p>";

                                menuRows += "<tr><td colspan=\"2\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Helvetica', 'Arial', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; border-right-width: 1px; border-right-color: #FFCE6F; border-right-style: solid; margin: 0; padding: 0px 0px 10px 10px;\" align=\"left\" valign=\"top\">" 
                                    + SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(bm.menuTitle).Replace("£", "&pound;")
                                    + "</td><td style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Helvetica', 'Arial', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; border-right-width: 1px; border-right-color: #FFCE6F; border-right-style: solid; margin: 0; padding: 0px 0px 10px 10px;\" align=\"left\" valign=\"top\">" 
                                    + bm.numberOfTickets.ToString()
                                    + "</td><td style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Helvetica', 'Arial', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; border-right-width: 1px; border-right-color: #FFCE6F; border-right-style: solid; margin: 0; padding: 0px 0px 10px 10px;\" align=\"left\" valign=\"top\">" + ApplicationCurrencyHtmlSymbol
                                    + (objBmm.numberOfTickets * bm.cost).ToString("0.00") + "</td></tr>";

                                menuUserName += "<tr style=\"vertical-align: top; text-align: left; font-family: 'Source Sans Pro', sans-serif !important; padding: 0;\" align=\"left\">"
                                                   + "    <td class=\"pt- pl\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;\" align=\"left\" valign=\"top\">"
                                                    + "       <p class=\"clr--gray text--normal\" style=\"color: #a9a9a8 !important; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;\" align=\"left\">" + user.User.FirstName + " " + user.User.LastName + " </p>"
                                                    + "   </td>"
                                                    + "   <td class=\"expander\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;\" align=\"left\" valign=\"top\"></td>"
                                                  + " </tr>";

                                menutitle += "<tr style=\"vertical-align: top; text-align: left; font-family: 'Source Sans Pro', sans-serif !important; padding: 0;\" align=\"left\">"
                                                                                    + "<td class=\"pt- pl\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;\" align=\"left\" valign=\"top\">"
                                                                                     + "<p class=\"clr--gray text--normal \" style=\"color: #a9a9a8 !important; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;\" align=\"left\">" + SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(bm.menuTitle).Replace("£", "&pound;") + "</p>"
                                                                                      + "</a>"
                                                                                      + "</td>"
                                                                                       + "<td class=\"expander\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;\" align=\"left\" valign=\"top\"></td>"
                                                                                    + "</tr>";
                                numberoftickets += "<tr style=\"vertical-align: top; text-align: left; font-family: 'Source Sans Pro', sans-serif !important; padding: 0;\" align=\"left\">"
                                                              + " <td class=\"pt- pl\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;\" align=\"left\" valign=\"top\">"
                                                               + "     <p class=\"clr--gray text--normal\" style=\"color: #a9a9a8 !important; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;\" align=\"left\">" + bm.numberOfTickets.ToString() + "</p>"
                                                               + " </td>"
                                                              + "  <td class=\"expander\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;\" align=\"left\" valign=\"top\"></td>"
                                                           + " </tr>";

                                totalCost += "<tr style=\"vertical-align: top; text-align: left; font-family: 'Source Sans Pro', sans-serif !important; padding: 0;\" align=\"left\">"
                                                 + "<td class=\"pt- pl\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;\" align=\"left\" valign=\"top\">"
                                                  + "<p class=\"clr--gray text--normal\" style=\"color: #a9a9a8 !important; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;\" align=\"left\">" + (bm.cost).ToString("0.00") + "</p>"
                                                    + "</td>"
                                                      + "<td class=\"expander\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;\" align=\"left\" valign=\"top\"></td>"
                                                        + "</tr>";

                                totalpaid += (bm.cost) * bm.numberOfTickets;

                            }
                        }
                        
                    }
                }

                lstVid = e.EventAttendees.ToList<EventAttendee>().Select(v => v.VoucherId).Distinct().ToList();

                foreach (int vid in lstVid)
                {
                    if (vid > 0)
                    {
                        Voucher voucher = _supperClubRepository.GetVoucherDetail(int.Parse(vid.ToString()));
                        string description = "";
                        if (voucher.TypeId == 1)
                            description = voucher.OffValue.ToString() + "%";
                        else
                            description = ApplicationCurrencyHtmlSymbol + voucher.OffValue;

                        vouchercodemm += "<p class=\"text--middle opacity-7 mb\" style=\"color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; opacity: 0.7; margin: 0 0 20px; padding: 0;\" align=\"left\">Voucher Code:" + voucher.Code + "--" + " Voucher Value:" + description + "</p>";

                    }
                }
                var bref = (from t in user.User.EventAttendees
                            join tb in user.User.TicketBaskets
                            on t.BasketId equals tb.Id
                            where t.EventId == e.Id && t.UserId == user.UserId
                            select new { bookingReference = tb.BookingReference, bookingRequirements = tb.BookingRequirements, ccLastDigits = tb.CCLastDigits, voucherId = t.VoucherId }).FirstOrDefault();
                bookingRequirements = string.IsNullOrEmpty(bref.bookingRequirements) ? "": bref.bookingRequirements;
                bookingReference = bref.bookingReference.ToString("00000000");
                ccLastDigits = bref.ccLastDigits;
                voucherId = (int)((bref.voucherId == null) ? 0: bref.voucherId);

                if (voucherId > 0)
                {
                    Voucher voucher = _supperClubRepository.GetVoucherDetail(voucherId);
                    string description = "";
                    if (voucher.TypeId == 1)
                        description = voucher.OffValue.ToString() + "%";
                    else
                        description = ApplicationCurrencyHtmlSymbol + voucher.OffValue;

                    vouchercode = "<p class=\"text--middle opacity-7 mb\" style=\"color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; opacity: 0.7; margin: 0 0 20px; padding: 0;\" align=\"left\">Voucher Code:" + voucher.Code + "--" + " Voucher Value:" + description + "</p>";

                }

                Dictionary<string, string> objReplace = new Dictionary<string, string>();
                foreach (var kvp in replace)
                    objReplace.Add(kvp.Key, kvp.Value);
                objReplace.Add("eventDate", e.WalkIn ? eventDate : eventDateTime);
                objReplace.Add("eventDateTime", eventDateTime);
                objReplace.Add("eventStartTime", eventStartTime);
                objReplace.Add("eventEndTime", eventEndTime);
                objReplace.Add("calStartDateTime", calStartDateTime);
                objReplace.Add("calEndDateTime", calEndDateTime);
                // Special Requirements
                if (string.IsNullOrEmpty(bookingRequirements))
                    bookingRequirements = "None";
                objReplace.Add("bookingRequirements", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(bookingRequirements));
                // Receipt
                objReplace.Add("bookingReference", bookingReference);
                objReplace.Add("userName", user.User.FirstName + " " + user.User.LastName);
               
                
                objReplace.Add("currency", ApplicationCurrencyHtmlSymbol);
                if (e.MultiMenuOption)
                {
                    objReplace.Add("totalPaid", totalpaid.ToString("0.00"));
                    objReplace.Add("vouchercode", vouchercodemm);
                }
                else
                {
                    objReplace.Add("totalPaid", (user.numberOfGuests * e.CostToGuest).ToString("0.00"));
                    objReplace.Add("vouchercode", vouchercode);
                }
                objReplace.Add("menuRows", menuRows);
                objReplace.Add("menuUserName",menuUserName);
                objReplace.Add("menutitle",menutitle);
                objReplace.Add("numberoftickets",numberoftickets);
                objReplace.Add("totalCost", totalCost);
                objReplace.Add("menuItems", menuItems);
                

                //if (voucherId != null && voucherId > 0)
                //{
                //    Voucher voucher = _supperClubRepository.GetVoucherDetail(voucherId);
                //    vouchercode = "<p class=\"text--middle opacity-7 mb\" style=\"color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 16px !important; opacity: 0.7; margin: 0 0 20px; padding: 0;\" align=\"left\">Voucher Code:" + voucher.Code + "</p>";

                //}

                objReplace.Add("ccLastDigits", ccLastDigits);
                
              //  objReplace.Add("vouchercode", vouchercode);

               // objReplace.Add("OfficeAddress", OfficeAddress);
                string bodyGuest = SupperClub.Code.Converter.ReplaceTemplate(emtGuest.Body, objReplace);
                guestSuccess = SendMail(emailToSendTo(user.User.Email), emtGuest.Subject, bodyGuest, emtGuest.Html);
            }
                return guestSuccess;
        }

        public bool SendGuestInviteeEmail(int eventId, Guid userId, int seatingId, string emailAddress, string comments, int numberOfGuests,int voucherId,string ccLastDigits )
        {
            Event e = null;
            User user = null;
            bool guestSuccess = false;
            if (eventId != 0)
                e = _supperClubRepository.GetEvent(eventId);
            else
                e = _supperClubRepository.GetFirstActiveEvent(); // for email testing

            string eventUrl = ServerURL + e.UrlFriendlyName + "/" + e.Id.ToString();
            user = _supperClubRepository.GetUser(userId);

            // Email the Guest
            EmailTemplate emtGuest = null;
            if (!e.WalkIn)
            {
                if (e.MultiMenuOption)
                    emtGuest = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.BookingConfirmedGuestMultiMenu);
                else
                    emtGuest = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.BookingConfirmedGuest);
            }
            else
            {
                if (e.MultiMenuOption)
                    emtGuest = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.BookingConfirmedGuestWalkInMultiMenu);
                else
                    emtGuest = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.BookingConfirmedGuestWalkIn);
            }


            Dictionary<string, string> replace = new Dictionary<string, string>();
            replace.Add("serverURL", ServerURL);
            replace.Add("contactEmail", DefaultContactEmailAddress);
            replace.Add("grubClubName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(e.SupperClub.Name));
            replace.Add("eventName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(e.Name));
            replace.Add("eventUrl", eventUrl);
            // Alcohol Policy
            string policy = "Guests" + (e.Alcohol ? " can " : " CANNOT ") + " bring their own alcohol";
            replace.Add("alcoholPolicy", policy);
            // Event Location
            replace.Add("eventAddress", e.Address);
            replace.Add("eventAddress2", e.Address2);
            replace.Add("eventCity", e.City);
            replace.Add("eventPostCode", e.PostCode);
            replace.Add("eventAddressMap", (e.Address != null && e.Address != string.Empty) ? e.Address.Replace("&", "and") : "");
            replace.Add("eventAddress2Map", (e.Address2 != null && e.Address2 != string.Empty) ? e.Address2.Replace("&", "and") : "");
            replace.Add("receiptDate", DateTime.Now.ToString());
            replace.Add("comments", (comments == null || comments.Length == 0) ? "" : ("<p style=\"color: #ff7500;margin-top:10px;\"><b> Note: " + comments + " </b></p>"));

            string eventStartTime = e.Start.ToString("h:mm tt");
            string eventEndTime = e.End.ToString("h:mm tt");
            string eventDateTime = e.Start.ToString("ddd, d MMM yyyy") + " : " + e.Start.ToShortTimeString() + " - " + e.End.ToShortTimeString();
            string eventDate = e.Start.ToString("ddd, d MMM yyyy");
            string bookingRequirements = "None";
            string bookingReference = "";
            string menuRows = "";
            string menuItems = "";
            string menuUserName = "";
            string menutitle = "";

            string numberoftickets = "";
            string totalCost = "";
            decimal totalpaid = 0;
            string vouchercodemm = "";
            List<int?> lstVid = new List<int?>();
            int Tickets = 0;

            string calStartDateTime = e.Start.ToString("yyyyMMdd") + "T" + e.Start.ToString("HHmmss") + "Z";
            string calEndDateTime = e.End.ToString("yyyyMMdd") + "T" + e.End.ToString("HHmmss") + "Z";
            // Hack for google calendar to display correct time
            bool dayLightSavingOn = false;
            TimeZoneInfo LondonTimeZone = TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time");
            if (LondonTimeZone.IsDaylightSavingTime(DateTime.Now))
                dayLightSavingOn = true;
            if (dayLightSavingOn)
            {
                DateTime offsetStartTime = e.Start.AddHours(-1);
                DateTime offsetEndTime = e.End.AddHours(-1);
                calStartDateTime = offsetStartTime.ToString("yyyyMMdd") + "T" + offsetStartTime.ToString("HHmmss") + "Z";
                calEndDateTime = offsetEndTime.ToString("yyyyMMdd") + "T" + offsetEndTime.ToString("HHmmss") + "Z";
            }

            if (e.MultiSeating)
            {                              
               var seatingTime = (from s in e.EventSeatings
                                   where s.Id == seatingId
                                   select new
                                   {
                                       start = s.Start,
                                       end = s.End,
                                   }).FirstOrDefault();

                                
                if (seatingTime != null)
                {
                    eventDateTime = e.Start.ToString("ddd, d MMM yyyy") + " : " + seatingTime.start.ToShortTimeString() + " - " + seatingTime.end.ToShortTimeString();
                    if (dayLightSavingOn)
                    {
                        DateTime offsetStartTime = seatingTime.start.AddHours(-1);
                        DateTime offsetEndTime = seatingTime.end.AddHours(-1);
                        calStartDateTime = offsetStartTime.ToString("yyyyMMdd") + "T" + offsetStartTime.ToString("HHmmss") + "Z";
                        calEndDateTime = offsetEndTime.ToString("yyyyMMdd") + "T" + offsetEndTime.ToString("HHmmss") + "Z";
                    }
                    else
                    {
                        calStartDateTime = seatingTime.start.ToString("yyyyMMdd") + "T" + seatingTime.start.ToString("HHmmss") + "Z";
                        calEndDateTime = seatingTime.end.ToString("yyyyMMdd") + "T" + seatingTime.end.ToString("HHmmss") + "Z";
                    }
                    if (e.WalkIn)
                    {
                        eventStartTime = seatingTime.start.ToString("h:mm tt");
                        eventEndTime = seatingTime.end.ToString("h:mm tt");
                    }
                }
            }
            if (e.MultiMenuOption)
            {
                var bmm = (from ea in e.EventAttendees
                            where ea.UserId == userId && ea.SeatingId == seatingId
                            group ea by new
                            {
                                ea.UserId,
                                ea.SeatingId,
                                ea.MenuOptionId,
                                
                            } into gcs
                            select new
                            {
                                menuId = gcs.Key.MenuOptionId,
                                numberOfTickets = gcs.Sum(ea => ea.NumberOfGuests),
                                totalAfterDiscount = gcs.Sum(ea => ea.TotalPrice),
                                discount = gcs.Sum(ea => ea.Discount),
                                
                            }).ToList();
                if (bmm != null)
                {
                    menuRows = "";
                   
                    foreach (var objBmm in bmm)
                    {
                        if (objBmm.numberOfTickets > 0)
                        {
                            var bm = (from emo in e.EventMenuOptions
                                        where emo.Id == objBmm.menuId
                                      select new { menuTitle = emo.Title, numberOfTickets = objBmm.numberOfTickets, cost = emo.CostToGuest, totalAfterDiscount = objBmm.totalAfterDiscount, discount = objBmm.discount}).First();

                            menuItems += "<p class=\"text--normal clr-gray\" style=\"color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;\" align=\"left\">" + bm.numberOfTickets.ToString() + " - " + SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(bm.menuTitle).Replace("£", "&pound;") + "</p>";

                            menuRows += "<tr><td colspan=\"2\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Helvetica', 'Arial', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; border-right-width: 1px; border-right-color: #FFCE6F; border-right-style: solid; margin: 0; padding: 0px 0px 10px 10px;\" align=\"left\" valign=\"top\">" 
                                + SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(bm.menuTitle).Replace("£", "&pound;") +
                                "</td><td style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Helvetica', 'Arial', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; border-right-width: 1px; border-right-color: #FFCE6F; border-right-style: solid; margin: 0; padding: 0px 0px 10px 10px;\" align=\"left\" valign=\"top\">" 
                                + bm.numberOfTickets.ToString() +
                                "</td><td style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Helvetica', 'Arial', sans-serif; font-weight: normal; line-height: 19px; font-size: 14px; border-right-width: 1px; border-right-color: #FFCE6F; border-right-style: solid; margin: 0; padding: 0px 0px 10px 10px;\" align=\"left\" valign=\"top\">" + ApplicationCurrencyHtmlSymbol
                                + (bm.totalAfterDiscount).ToString("0.00") + "</td></tr>";

                            menuUserName += "<tr style=\"vertical-align: top; text-align: left; font-family: 'Source Sans Pro', sans-serif !important; padding: 0;\" align=\"left\">"
                                                   + "    <td class=\"pt- pl\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;\" align=\"left\" valign=\"top\">"
                                                    + "       <p class=\"clr--gray text--normal\" style=\"color: #a9a9a8 !important; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;\" align=\"left\">" + user.FirstName+" "+user.LastName + " </p>"
                                                    + "   </td>"
                                                    + "   <td class=\"expander\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;\" align=\"left\" valign=\"top\"></td>"
                                                  + " </tr>";

                            menutitle += "<tr style=\"vertical-align: top; text-align: left; font-family: 'Source Sans Pro', sans-serif !important; padding: 0;\" align=\"left\">"
                                                                                + "<td class=\"pt- pl\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;\" align=\"left\" valign=\"top\">"
                                                                                 + "<p class=\"clr--gray text--normal \" style=\"color: #a9a9a8 !important; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;\" align=\"left\">" + SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(bm.menuTitle).Replace("£", "&pound;") + "</p>"
                                                                                  + "</a>"
                                                                                  + "</td>"
                                                                                   + "<td class=\"expander\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;\" align=\"left\" valign=\"top\"></td>"
                                                                                + "</tr>";
                            numberoftickets += "<tr style=\"vertical-align: top; text-align: left; font-family: 'Source Sans Pro', sans-serif !important; padding: 0;\" align=\"left\">"
                                                          + " <td class=\"pt- pl\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;\" align=\"left\" valign=\"top\">"
                                                           + "     <p class=\"clr--gray text--normal\" style=\"color: #a9a9a8 !important; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;\" align=\"left\">" + bm.numberOfTickets.ToString() + "</p>"
                                                           + " </td>"
                                                          + "  <td class=\"expander\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;\" align=\"left\" valign=\"top\"></td>"
                                                       + " </tr>";
                            Tickets += bm.numberOfTickets;

                            totalCost += "<tr style=\"vertical-align: top; text-align: left; font-family: 'Source Sans Pro', sans-serif !important; padding: 0;\" align=\"left\">"
                                             + "<td class=\"pt- pl\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 10px 0px 10px 20px;\" align=\"left\" valign=\"top\">"
                                              + "<p class=\"clr--gray text--normal\" style=\"color: #a9a9a8 !important; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0; padding: 0;\" align=\"left\">" + (bm.cost).ToString("0.00") + "</p>"
                                                + "</td>"
                                                  + "<td class=\"expander\" style=\"word-break: break-word; -webkit-hyphens: auto; -moz-hyphens: auto; hyphens: auto; border-collapse: collapse !important; vertical-align: top; text-align: left; visibility: hidden; width: 0px; color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; line-height: 19px; font-size: 14px; margin: 0; padding: 0;\" align=\"left\" valign=\"top\"></td>"
                                                    + "</tr>";
                            totalpaid += bm.totalAfterDiscount??0;

                        }
                    }
                   
                    
                }
            }
            //lstVid= e.EventAttendees.ToList<EventAttendee>().Select(v=>v.VoucherId).Distinct().ToList();

            lstVid = (from ea in e.EventAttendees
                            where ea.UserId == userId && ea.VoucherId > 0
                          select ea.VoucherId).Distinct().ToList();

            if (lstVid != null && lstVid.Count > 0)
            {
                foreach (int vid in lstVid)
                {
                    if (vid > 0)
                    {
                        //LogMessage("SendGuestInviteeEmail: VoucherId="+vid.ToString());
                        Voucher voucher = _supperClubRepository.GetVoucherDetail(int.Parse(vid.ToString()));
                        string description = "";
                        if (voucher.TypeId == 1)
                            description = voucher.OffValue.ToString() + "%";
                        else
                            description = ApplicationCurrencyHtmlSymbol + voucher.OffValue;

                        vouchercodemm += "<p class=\"text--middle opacity-7 mb\" style=\"color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; opacity: 0.7; margin: 0 0 20px; padding: 0;\" align=\"left\">Voucher Code:" + voucher.Code + "--" + " Voucher Value:" + description + "</p>";

                    }
                }
            }
            var bref = (from t in user.Tickets
                        join tb in user.TicketBaskets
                        on t.BasketId equals tb.Id
                        where t.EventId == e.Id && t.UserId == user.Id && t.SeatingId == seatingId
                        select new { bookingReference = tb.BookingReference, bookingRequirements = tb.BookingRequirements,ccLastDigits=tb.CCLastDigits,voucherId=t.VoucherId }).FirstOrDefault();
            if (bref != null)
            {
                bookingRequirements = bref.bookingRequirements;
                bookingReference = bref.bookingReference.ToString("00000000");
                ccLastDigits = bref.ccLastDigits;
                voucherId = bref.voucherId;
            }


            string vouchercode="";
            if (voucherId != null && voucherId > 0)
            {
                Voucher voucher = _supperClubRepository.GetVoucherDetail(voucherId);
                string description = "";
                if (voucher.TypeId == 1)
                    description = voucher.OffValue.ToString() + "%";
                else
                    description = ApplicationCurrencyHtmlSymbol + voucher.OffValue;

                vouchercode = "<p class=\"text--middle opacity-7 mb\" style=\"color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; opacity: 0.7; margin: 0 0 20px; padding: 0;\" align=\"left\">Voucher Code:" + voucher.Code + "--" + " Voucher Value:" + description + "</p>";

            }

            Dictionary<string, string> objReplace = new Dictionary<string, string>();
            foreach (var kvp in replace)
                objReplace.Add(kvp.Key, kvp.Value);
            objReplace.Add("eventDate", e.WalkIn ? eventDate : eventDateTime);
            objReplace.Add("eventDateTime", eventDateTime);
            objReplace.Add("eventStartTime", eventStartTime);
            objReplace.Add("eventEndTime", eventEndTime);
            objReplace.Add("calStartDateTime", calStartDateTime);
            objReplace.Add("calEndDateTime", calEndDateTime);
            // Special Requirements
            if (string.IsNullOrEmpty(bookingRequirements))
                bookingRequirements = "None";
            objReplace.Add("bookingRequirements", bookingRequirements);
            // Receipt
            objReplace.Add("bookingReference", bookingReference);
            objReplace.Add("userName", user.FirstName + " " + user.LastName);
            if (e.MultiMenuOption)
            {
                if (Tickets > 1)
                {

                    objReplace.Add("numberTickets", Tickets.ToString() + " Tickets");
                    objReplace.Add("numberPeople", Tickets.ToString() + " People");
                }
                else
                {

                    objReplace.Add("numberPeople", Tickets.ToString() + " Person");
                    objReplace.Add("numberTickets", Tickets.ToString() + " Ticket");
                }
              
            }
            else
            {
                if (numberOfGuests > 1)
                {

                    objReplace.Add("numberTickets", numberOfGuests.ToString() + " Tickets");
                    objReplace.Add("numberPeople", numberOfGuests.ToString() + " People");
                }
                else
                {

                    objReplace.Add("numberPeople", numberOfGuests.ToString() + " Person");
                    objReplace.Add("numberTickets", numberOfGuests.ToString() + " Ticket");
                  
                }
            }
            objReplace.Add("numberoftickets", numberoftickets);
           // objReplace.Add("numberTickets", numberOfGuests.ToString());
            objReplace.Add("quantity", numberOfGuests.ToString());
            objReplace.Add("currency", ApplicationCurrencyHtmlSymbol);

            if (e.MultiMenuOption)
            {
                objReplace.Add("totalPaid", totalpaid.ToString("0.00"));
                objReplace.Add("vouchercode", vouchercodemm);
            }
            else
            {
                objReplace.Add("totalPaid", (numberOfGuests * e.CostToGuest).ToString("0.00"));
                objReplace.Add("vouchercode", vouchercode);
            }
           
            objReplace.Add("menuRows", menuRows);
            objReplace.Add("menuUserName", menuUserName);
            objReplace.Add("menutitle", menutitle);
            
            objReplace.Add("totalCost", totalCost);
            objReplace.Add("menuItems", menuItems);
            
            objReplace.Add("imageUrl", e.ImagePath);
            objReplace.Add("ccLastDigits",ccLastDigits);
            objReplace.Add("hostUrl", e.SupperClub.UrlFriendlyName);
            
            objReplace.Add("eventCost", e.CostToGuest.ToString());
            string byob = "";
            if (e.Alcohol == true)
                byob = " <p class=\"text--normal clr-gray mb\" style=\"color: #222222; font-family: 'Source Sans Pro', sans-serif !important; font-weight: normal; text-align: left; line-height: 19px; font-size: 14px !important; margin: 0 0 20px; padding: 0;\" align=\"left\">"

                     + "  Bring your own bottle</p>";
            objReplace.Add("byob", byob);

            objReplace.Add("OfficeAddress", OfficeAddress);
            string bodyGuest = SupperClub.Code.Converter.ReplaceTemplate(emtGuest.Body, objReplace);
            guestSuccess = SendMail(emailAddress, emtGuest.Subject, bodyGuest, emtGuest.Html);
            return guestSuccess;
        }

        public bool SendAdminNewEventCreatedEmail(string userName, string userEmail, string userContatNumber, string supperClubName, string supperClubURL, string eventName, string eventURL, int eventId)
        {
            EmailTemplate emt = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.EventCreated);

            Dictionary<string, string> replace = new Dictionary<string, string>();
            replace.Add("serverURL", ServerURL);
            replace.Add("userName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(userName));
            replace.Add("userEmail", userEmail);
            replace.Add("phoneNumber", (string.IsNullOrEmpty(userContatNumber) ? "":" and phone number: "+ userContatNumber));
            replace.Add("contactEmail", DefaultContactEmailAddress);
            replace.Add("grubClubName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(supperClubName));
            replace.Add("grubClubURL", supperClubURL);
            replace.Add("eventName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(eventName));
            replace.Add("eventURL", eventURL);
            replace.Add("eventId", eventId.ToString());

            replace.Add("OfficeAddress", OfficeAddress);
            string body = SupperClub.Code.Converter.ReplaceTemplate(emt.Body, replace);

            return SendMail(emailToSendTo(DefaultAdminEmailAddress), emt.Subject, body, emt.Html);
        }

        public bool SendHostNewEventCreatedEmail(int supperClubId, string username)
        {
            EmailTemplate emt = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.EventApprovalRequestReceivedHost);

            SupperClub.Domain.SupperClub s = _supperClubRepository.GetSupperClub(supperClubId);
            Dictionary<string, string> replace = new Dictionary<string, string>();
            replace.Add("serverURL", ServerURL);
            replace.Add("contactEmail", DefaultContactEmailAddress);
            replace.Add("grubClubName", SupperClub.Web.Helpers.Utils.HTMLEncodeSpecialChars(s.Name));
            replace.Add("grubClubURL", ServerMethods.ServerUrl + s.UrlFriendlyName);

            replace.Add("fname", username);

            replace.Add("OfficeAddress", OfficeAddress);
            string body = SupperClub.Code.Converter.ReplaceTemplate(emt.Body, replace);
            string hostEmailAddress = s.User.aspnet_Users.aspnet_Membership.Email;

            bool hostSuccess = false;
            if (SendEmailToHost)
                hostSuccess = SendMail(emailToSendTo(hostEmailAddress), emt.Subject, body, emt.Html);
            else
                hostSuccess = true;

            try
            {
                if (SendHostTestEmail)
                    SendMail(DefaultContactEmailAddress, emt.Subject, body, emt.Html);
            }
            catch
            {  }

            return hostSuccess;
        }

        public bool SendHostNewEventApprovalEmail(int eventId)
        {
            Event e = null;
            if (eventId != 0)
                e = _supperClubRepository.GetEvent(eventId);
            else
                e = _supperClubRepository.GetFirstActiveEvent(); // for email testing
            
            Dictionary<string, string> replace = new Dictionary<string, string>();
            replace.Add("serverURL", ServerURL);
            replace.Add("contactEmail", DefaultContactEmailAddress);
            // Event URL            
            replace.Add("eventURL", ServerURL + e.UrlFriendlyName + "/" + e.Id.ToString());

            replace.Add("profileURL", ServerURL + "host/HostProfile?supperClubId=" + e.SupperClub.Id.ToString());
            replace.Add("fname", e.SupperClub.User.FirstName);

            replace.Add("OfficeAddress", OfficeAddress);

            // Email the Host
            string hostEmailAddress = e.SupperClub.User.aspnet_Users.aspnet_Membership.Email;           
            EmailTemplate emt = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.EventApprovalConfirmationHost);
            
            string body = SupperClub.Code.Converter.ReplaceTemplate(emt.Body, replace);

            bool hostSuccess = false;
            if (SendEmailToHost)
                hostSuccess = SendMail(emailToSendTo(hostEmailAddress), emt.Subject, body, emt.Html);
            else
                hostSuccess = true;

            try
            {
                if (SendHostTestEmail)
                    SendMail(DefaultContactEmailAddress, emt.Subject, body, emt.Html);
            }
            catch
            {  }


            return hostSuccess;
        }

        public bool SendHostNewEventRejectionEmail(int eventId, string username)
        {
            Event e = null;
            if (eventId != 0)
                e = _supperClubRepository.GetEvent(eventId);
            else
                e = _supperClubRepository.GetFirstActiveEvent(); // for email testing

            Dictionary<string, string> replace = new Dictionary<string, string>();
            replace.Add("serverURL", ServerURL);
            replace.Add("contactEmail", DefaultContactEmailAddress);
            // Event URL            
            replace.Add("eventURL", ServerURL + e.UrlFriendlyName + "/" + e.Id.ToString());

            replace.Add("fname", username);

            replace.Add("OfficeAddress", OfficeAddress);


            // Email the Host
            string hostEmailAddress = e.SupperClub.User.aspnet_Users.aspnet_Membership.Email;
            EmailTemplate emt = _supperClubRepository.GetEmailTemplate((int)EmailTemplates.EventRejectionConfirmationHost);

            string body = SupperClub.Code.Converter.ReplaceTemplate(emt.Body, replace);

            bool hostSuccess = false;
            if (SendEmailToHost)
                hostSuccess = SendMail(emailToSendTo(hostEmailAddress), emt.Subject, body, emt.Html);
            else
                hostSuccess = true;

            try
            {
                if (SendHostTestEmail)
                    SendMail(DefaultContactEmailAddress, emt.Subject, body, emt.Html);
            }
            catch
            {  }
            return hostSuccess;
        }
        #region Actually send the emails

        /// <summary>
        /// Sends e-mail message to a single recipient.
        /// </summary>        
        /// <param name="toAddress">Recipient address.</param>
        /// <param name="subject">E-mail subject.</param>
        /// <param name="body">E-mail body.</param>
        /// <param name="isHtml">Message is html or not.</param>        
        /// <returns>True if no error occured.</returns>
        private bool SendMail(string toAddress, string subject, string body, bool isHtml)
        {
            try
            {

              /*  if (!(toAddress.ToLower().Contains("hotmail") || toAddress.ToLower().Contains("outlook")||
                   toAddress.ToLower().Contains("live") || toAddress.ToLower().Contains("msn")||
                   toAddress.ToLower().Contains("passport") || toAddress.ToLower().Contains("eat@grubclub.com") ||
                   toAddress.ToLower().Contains("seibel-net.com")))
                {
                    SmtpClient client = new SmtpClient();
                    MailMessage msg = new MailMessage();
                    msg.Subject = subject;
                    msg.Body = body;
                    msg.IsBodyHtml = isHtml;
                    msg.BodyEncoding = System.Text.Encoding.ASCII;

                    msg.To.Add(toAddress);
                    client.Send(msg);
                    client.Dispose();
                    return true;
                }
                else
                {
                    SmtpClient client = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential("admin@grubclub.com", "supperclubliv")
                    };

                    client.EnableSsl = true;

                    MailMessage msg = new MailMessage();
                    //msg.From = new MailAddress("admin@grubclub.com");
                    msg.Subject = subject;
                    msg.Body = body;
                    msg.IsBodyHtml = isHtml;
                    msg.BodyEncoding = System.Text.Encoding.ASCII;

                    msg.To.Add(toAddress);
                    // client.EnableSsl = true;
                    client.Send(msg);
                    client.Dispose();
                    return true;

                }*/
                try
                {
                    SmtpClient client = new SmtpClient();
                    MailMessage msg = new MailMessage();
                    msg.Subject = subject;
                    msg.Body = body;
                    msg.IsBodyHtml = isHtml;
                    msg.BodyEncoding = System.Text.Encoding.ASCII;

                    msg.To.Add(toAddress);
                    client.Send(msg);
                    client.Dispose();
                    return true;
                }
                catch(Exception e)
                {
                    log.Fatal(e.Message, e);
                    SmtpClient client = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential("admin@grubclub.com", "supperclubliv")
                    };

                    client.EnableSsl = true;

                    MailMessage msg = new MailMessage();
                    msg.Subject = subject;
                    msg.Body = body;
                    msg.IsBodyHtml = isHtml;
                    msg.BodyEncoding = System.Text.Encoding.ASCII;

                    msg.To.Add(toAddress);
                    client.Send(msg);
                    client.Dispose();
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
                return false;
            }
        }

        /// <summary>
        /// Sends e-mail message to multiple BCC recipients.
        /// </summary>
        /// <param name="toAddresses">The list of BCC recipients addresses.</param>
        /// <param name="subject">E-mail subject.</param>
        /// <param name="body">E-mail body.</param>
        /// <param name="isHtml">Message is html or not.</param>    
        /// <returns>True if no error occured.</returns>
        private bool SendMailToMany(string toAddress, List<string> bccAddresses, string subject, string body, bool isHtml)
        {
            try
            {
               /* List<String> lstmailid = new List<string>();
                List<string> lsthotmailid = new List<string>();

                lsthotmailid = bccAddresses.Where(mail => (mail.ToLower().Contains("hotmail")) || (mail.ToLower().Contains("outlook")) || (mail.ToLower().Contains("live")) || (mail.ToLower().Contains("msn")) || (mail.ToLower().Contains("passport")) || (mail.ToLower().Contains("eat@grubclub.com")) || (mail.ToLower().Contains("seibel-net.com"))).ToList();
                lstmailid = bccAddresses.Where(mail => !((mail.ToLower().Contains("hotmail")) || (mail.ToLower().Contains("outlook")) || (mail.ToLower().Contains("live")) || (mail.ToLower().Contains("msn")) || (mail.ToLower().Contains("passport")) || (mail.ToLower().Contains("eat@grubclub.com")) || (mail.ToLower().Contains("seibel-net.com")))).ToList();

                if (lstmailid != null)
                {
                    SmtpClient client = new SmtpClient();
                    MailMessage msg = new MailMessage();
                    msg.Subject = subject;
                    msg.Body = body;
                    msg.IsBodyHtml = isHtml;
                    msg.BodyEncoding = System.Text.Encoding.ASCII;
                    msg.To.Add(toAddress); //Always send to self
                    foreach (string address in bccAddresses)
                    {
                        msg.Bcc.Add(new MailAddress(address)); //BCC the addressees
                    }
                    client.Send(msg);
                    //  return true;
                }

                if (lsthotmailid != null)
                {
                    SmtpClient client = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,

                        Credentials = new NetworkCredential("admin@grubclub.com", "supperclubliv")
                    };
                    client.EnableSsl = true;
                    MailMessage msg = new MailMessage();
                 //   msg.From = new MailAddress("admin@grubclub.com");
                    msg.Subject = subject;
                    msg.Body = body;
                    msg.IsBodyHtml = isHtml;
                    msg.BodyEncoding = System.Text.Encoding.ASCII;
                    msg.To.Add(toAddress); //Always send to self
                    foreach (string address in bccAddresses)
                    {
                        msg.Bcc.Add(new MailAddress(address)); //BCC the addressees
                    }
                    client.Send(msg);
                }*/
                try
                {
                    SmtpClient client = new SmtpClient();
                    MailMessage msg = new MailMessage();
                    msg.Subject = subject;
                    msg.Body = body;
                    msg.IsBodyHtml = isHtml;
                    msg.BodyEncoding = System.Text.Encoding.ASCII;
                    msg.To.Add(toAddress); //Always send to self
                    foreach (string address in bccAddresses)
                    {
                        msg.Bcc.Add(new MailAddress(address)); //BCC the addressees
                    }
                    client.Send(msg);
                    return true;
                }
                catch(Exception e)
                {
                    log.Fatal(e.Message, e);
                    SmtpClient client = new SmtpClient
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,

                        Credentials = new NetworkCredential("admin@grubclub.com", "supperclubliv")
                    };
                    client.EnableSsl = true;
                    MailMessage msg = new MailMessage();
                    //   msg.From = new MailAddress("admin@grubclub.com");
                    msg.Subject = subject;
                    msg.Body = body;
                    msg.IsBodyHtml = isHtml;
                    msg.BodyEncoding = System.Text.Encoding.ASCII;
                    msg.To.Add(toAddress); //Always send to self
                    foreach (string address in bccAddresses)
                    {
                        msg.Bcc.Add(new MailAddress(address)); //BCC the addressees
                    }
                    client.Send(msg);
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
                return false;
            }
        }


        private string emailToSendTo(string emailToSendTo)
        {
            if (SendToTestAddress == null)
                return emailToSendTo;
            else
                return SendToTestAddress;
        }

        #endregion
    }
#endregion
}