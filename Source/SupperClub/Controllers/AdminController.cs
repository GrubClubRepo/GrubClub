
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SupperClub.Domain.Repository;
using SupperClub.Models;
using System.Web.Security;
using SupperClub.Code;
using SupperClub.Domain;
using System.Diagnostics;
using System.Web.Configuration;
using SupperClub.Logger;
using System.Collections;

namespace SupperClub.Controllers
{
    [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
    public class AdminController : BaseController
    {
        public AdminController(ISupperClubRepository supperClubRepository)
        {
            _supperClubRepository = supperClubRepository;
        }

        private string ReportFolderPath = System.Web.HttpContext.Current.Server.MapPath(WebConfigurationManager.AppSettings["ReportFolderPath"]);
        private string _eventViewCookieKey = WebConfigurationManager.AppSettings["EventViewCookieKey"];
        private string _eventBookingCookieKey = WebConfigurationManager.AppSettings["EventBookingCookieKey"];

        public ActionResult Vouchers()
        {
            return View("Vouchers");
        }

        #region Users

        [Authorize(Roles = "Admin")]
        public ActionResult UnlockUser()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult UnlockUser(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                if (Membership.FindUsersByEmail(model.EmailAddress).Count != 1)
                {
                    SetNotification(NotificationType.Error, "We don't have that email address in our system, you sure it was the right one?", false, false, true);
                    return View(model);
                }

                MembershipUser aspnetuser = Membership.GetUser(model.EmailAddress);
                bool success = aspnetuser.UnlockUser();
                if (success)
                {
                    SetNotification(NotificationType.Success, "User has been unlocked.", false, true, true);
                    return View();
                }
                else
                {
                    SetNotification(NotificationType.Error, "Could not unlock user account. Please contact an Administrator.", false, false, true);
                    return View(model);
                }

            }
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public JsonResult LockUser(Guid userId)
        {
            bool? newState = _supperClubRepository.FlipUserLock(userId);
            return Json(newState, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult AllUsers()
        {
            AllUsersModel model = new AllUsersModel();
            model.AllUsers = _supperClubRepository.GetAllUsers();
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult UserRoleInfo()
        {
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult UserRoleInfo(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                if (Membership.FindUsersByEmail(model.EmailAddress).Count != 1)
                {
                    SetNotification(NotificationType.Error, "We don't have that email address in our system, you sure it was the right one?", false, false, true);
                    return View(model);
                }

                bool isGuest = Roles.IsUserInRole(model.EmailAddress, "Guest");
                bool isHost = Roles.IsUserInRole(model.EmailAddress, "Host");
                bool isAdmin = Roles.IsUserInRole(model.EmailAddress, "Admin");
                SupperClub.Domain.SupperClub sc = _supperClubRepository.GetSupperClubForUser(model.EmailAddress);
                if (sc != null && sc.Id > 0 && !isHost)
                    ViewBag.incompleteHostProfile = true;
                else
                    ViewBag.incompleteHostProfile = false;

                ViewBag.currentStatus = (isGuest ? "Guest":"Account does not exist") + (isHost ? ", Host" : "") + (isAdmin ? ", Admin" : "");
                ViewBag.isHost = isHost;
            }
            return View(model);
        }
        [Authorize(Roles = "Admin")]
        public JsonResult AddHostRole(string email)
        {
            try
            {
                if (!Roles.IsUserInRole(email, "Host"))
                {
                    System.Web.Security.Roles.AddUserToRole(email, "Host");
                }
                return Json(true, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region SupperClubs


        [Authorize(Roles = "Admin")]
        public ActionResult AllSupperClubs()
        {
            AllSupperClubsModel model = new AllSupperClubsModel();
            model.AllSupperClubs = _supperClubRepository.GetAllSupperClubs();
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public JsonResult LockSupperClub(int supperClubId)
        {
            bool? newState = _supperClubRepository.FlipActivationForSupperClub(supperClubId);
            return Json(newState, JsonRequestBehavior.AllowGet);
        }

        #endregion
        #region Events

        [Authorize(Roles = "Admin")]
        public ActionResult NewEvents()
        {
            NewEventsModel model = new NewEventsModel();
            model.NewEvents = _supperClubRepository.GetNewlyCreatedEvents();
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public JsonResult UpdateEventStatus(int eventId, int newStatus)
        {
            AddTicketsStatusModel atsm = new AddTicketsStatusModel();
            Event e = _supperClubRepository.GetEvent(eventId);
            if (e == null)
            {
                atsm.NumberOfTicketsAdded = 0;
                atsm.Message = "Event info could not be found!";
            }
            else
            {
                e.Status = newStatus;
                try
                {
                    e.DateApproved = DateTime.Now;
                }
                catch (Exception ex)
                {
                    LogMessage("Error assigning datetime to DateApproval Column. Message=" + ex.Message + " StackTrace=" + ex.StackTrace, LogLevel.ERROR);
                }
                bool eventUpdateStatus = _supperClubRepository.UpdateEventStatus(e);
                if (eventUpdateStatus)
                {
                    atsm.NumberOfTicketsAdded = newStatus;
                }
                else
                {
                    atsm.NumberOfTicketsAdded = 0;
                }
            }

           
            // Send e-mail notification to host about approval or rejection of their event
            string username = string.Empty;

            if (atsm.NumberOfTicketsAdded  > 0)
            {
                if (eventId > 0)
                {
                    username = e.SupperClub.User.FirstName;
                    // Event Approval Notification
                    if (atsm.NumberOfTicketsAdded == (int)EventStatus.Active)
                    {
                        // Push Notification to chef's followers                

                        PushNotificationService.MessageService ms = new PushNotificationService.MessageService(_supperClubRepository);
                        bool pushNotificationStatus = ms.FavChefNewEventNotification(e.Id, e.SupperClubId, e.Name, e.SupperClub.Name, e.Start);
                        if (pushNotificationStatus)
                            LogMessage("UpdateEventStatus: Sent notification to chef's followers", LogLevel.INFO);
                        else
                            LogMessage("UpdateEventStatus: Error sending notification to chef's followers", LogLevel.ERROR);


                        EmailService.EmailService es = new EmailService.EmailService(_supperClubRepository);
                        bool success = es.SendHostNewEventApprovalEmail(eventId);
                        atsm.Success = success;
                        if (!success)
                        {
                            LogMessage("Error sending event approval notification e-mail to host", LogLevel.ERROR);
                            atsm.Message = "Event approved successfully but there was error sending event approval notification e-mail to host";
                        }
                        else
                        {
                            atsm.Message = "The event is approved successully!";
                        }
                    }
                    else if (atsm.NumberOfTicketsAdded == (int)EventStatus.Rejected)
                    {
                        EmailService.EmailService es = new EmailService.EmailService(_supperClubRepository);
                        bool success = es.SendHostNewEventRejectionEmail(eventId, username);
                        atsm.Success = success;
                        if (!success)
                        {
                            LogMessage("Error sending event rejection notification e-mail to host", LogLevel.ERROR);
                            atsm.Message = "Event rejected Successfully but there was error sending event rejection notification e-mail to host";
                        }
                        else
                        {
                            atsm.Message = "The event is rejected successully!";
                        }
                    }
                }
            }
            return Json(atsm, JsonRequestBehavior.AllowGet);
        }

        #endregion

        [Authorize(Roles = "Admin")]
        public ActionResult FlushExpiredTicketBaskets()
        {
            int TicketBasketSessionTimeOut = int.Parse(WebConfigurationManager.AppSettings["TicketBasketSessionTimeOut"]);
            Tuple<int,int> results = _supperClubRepository.CleanUpAbandonedBaskets(TicketBasketSessionTimeOut);
            SetNotification(NotificationType.Success, string.Format("{0} expired baskets flushed with a total of {1} tickets.", results.Item1, results.Item2), true);
            return RedirectToAction("Log4Net");
        }

        #region Logging

        [Authorize(Roles = "Admin")]
        public ActionResult Log4Net(int days = 1, string logLevel = "ERROR", string filter = null)
        {
            Log4NetModel model = new Log4NetModel();
            
            // Level Selection
            LogLevel level;
            bool success = Enum.TryParse(logLevel, out level);
            if (!success)
                level = LogLevel.OFF;

            if (!string.IsNullOrEmpty(filter))
                model.FilterText = filter;

            model.LogEvents = _supperClubRepository.GetLog(days, level, filter);

            // Select List
            var logLevels = from LogLevel d in Enum.GetValues(typeof(LogLevel))
                             select new { ID = d.ToString(), Name = d.ToString() };
            ViewBag.LogLevels = new SelectList(logLevels, "ID", "Name", logLevel);
            
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult LogEvent(int logId)
        {
            Log model = new Log();
            model = _supperClubRepository.GetLogEvent(logId);
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult PurgeLog(int olderThanDays = 7)
        {
            bool success = _supperClubRepository.PurgeLog(olderThanDays);

            if (success)
                SetNotification(NotificationType.Success, "Log Purged", true);
            else
                SetNotification(NotificationType.Error, "Error purging log!", true);

            return RedirectToAction("Log4Net");
        }

        #endregion

        #region Public Administration Tools

        public ActionResult DeviceInfo(DeviceInfoModel model)
        {
            // var allProperties = Request.Browser.Capabilities["51Degrees.mobi"] as SortedList<string, List<string>>;
            model.IsMobileDevice = Request.Browser.IsMobileDevice;
            model.Manufacturer = Request.Browser.MobileDeviceManufacturer;
            model.Model = Request.Browser.MobileDeviceModel;
            model.Screen = (Request.Browser.ScreenPixelsWidth + " x " + Request.Browser.ScreenPixelsHeight);
            model.Platform = Request.Browser.Platform;
            model.Browser = Request.Browser.Browser + " " + Request.Browser.Version.ToString();
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult ShowCookie()
        {
            string viewCookieData = "No Cookie Found";
            string bookingCookieData = "No Cookie Found";
            if (System.Web.HttpContext.Current.Request.Cookies[_eventViewCookieKey] != null)
            {
                viewCookieData = Web.Helpers.Utils.CookieStore.GetCookie(_eventViewCookieKey);
            }           
            
            if (System.Web.HttpContext.Current.Request.Cookies[_eventBookingCookieKey] != null)
            {
                bookingCookieData = Web.Helpers.Utils.CookieStore.GetCookie(_eventBookingCookieKey);
            }
            ViewBag.VCI = viewCookieData;
            ViewBag.BCI = bookingCookieData;
            
            return View();
        }
        #endregion

        #region Test Emails

        [Authorize(Roles = "Admin")]
        public ActionResult TestEmail()
        {
            TestEmailModel model = new TestEmailModel();
            model.EmailAddress = WebConfigurationManager.AppSettings["DefaultAdminEmailAddress"];

            var emailTypes = from EmailService.EmailTemplates et in Enum.GetValues(typeof(EmailService.EmailTemplates))
                             select new { Id = (int)et, Name = et.ToString() };
            ViewBag.EmailTypes = new SelectList(emailTypes, "Id", "Name");

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult TestEmail(TestEmailModel model)
        {
            if (string.IsNullOrEmpty(model.EmailAddress))
                model.EmailAddress = WebConfigurationManager.AppSettings["DefaultAdminEmailAddress"];

            EmailService.EmailService em = new EmailService.EmailService(_supperClubRepository, model.EmailAddress);
            bool success = false;
            bool hostSuccess = false;
            switch ((EmailService.EmailTemplates)model.SelectedEmailTemplate)
            {
                case EmailService.EmailTemplates.ContactUsForm:
                    success = em.SendAdminContactUsEmail("TestName", "Test@Email.com", "This is a test email");
                    break;
                case EmailService.EmailTemplates.PasswordReset:
                    success = em.SendPasswordResetEmail("NA", "ThisIsANewPassword");
                    break;
                case EmailService.EmailTemplates.SupperClubRegistered:
                    success = em.SendAdminNewSupperClubRegisteredEmail("TestName", "TestEmail@Test.com", "TestSupperClubName");
                    break;
                case EmailService.EmailTemplates.BookingConfirmedGuest:
                case EmailService.EmailTemplates.BookingConfirmedHost:
                    success = em.SendGuestBookedEmails("NA", "NA", 2, 20, 1234, "Any Special Booking Requirements", 0, 0, null,0,"xxxx",decimal.Parse("0.1"), ref hostSuccess);
                    break;
                case EmailService.EmailTemplates.GuestRefused:
                    success = em.SendGuestRejectionEmail("UserName", "user@email.address.com", "TestSupperClubName", "TestEventName", "01/01/2012");
                    break;
                case EmailService.EmailTemplates.EventCancelled:
                    success = em.SendEventCancelledEmails(new List<string>(), "TestSupperClubName", "TestEventName", "01/01/2012", "This event has been cancelled");
                    break;
                case EmailService.EmailTemplates.Welcome:
                    success = em.SendWelcomeEmail("TestUserName", "NA");
                    break;
                case EmailService.EmailTemplates.NewReviewHost:
                    success = em.SendHostNewReviewEmail(0, "Test Review","TestPrivateReview");
                    break;
                case EmailService.EmailTemplates.NewReviewAdmin:
                    success = em.SendAdminNewReviewEmail(0, 3, "This is a test Public Review", "This is a test Private Review");
                    break;
            }

            if (success)
                SetNotification(NotificationType.Success, "Email sent successfully", true);
            else
                SetNotification(NotificationType.Error, "Email was not sent successfully", true);

            var emailTypes = from EmailService.EmailTemplates et in Enum.GetValues(typeof(EmailService.EmailTemplates))
                             select new { Id = (int)et, Name = et.ToString() };
            ViewBag.EmailTypes = new SelectList(emailTypes, "Id", "Name");

            return View(model);
        }

        #endregion

        #region Automated Guest Emails

        [Authorize(Roles = "Admin")]
        public ActionResult SendGuestEmail()
        {           
            SendGuestEmail model = new SendGuestEmail();
            model.SendToAll = false;
            model.UserEmailList = "";
            model.SendStatus = "";
            model.Comments = "";
            model.ShowStatus = false;
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult SendGuestEmail(SendGuestEmail model)
        {            
            EmailService.EmailService em = new EmailService.EmailService(_supperClubRepository, null);

            if (model.EventId == 0)
            {
                SetNotification(Models.NotificationType.Error, "Please specify a valid event id.", true);
                return RedirectToAction("SendGuestEmail");
            }
            if (!model.SendToAll && (model.UserEmailList == null || model.UserEmailList.Trim() == string.Empty))
            {
                SetNotification(Models.NotificationType.Error, "Please enter e-mail address.", true);
                return RedirectToAction("SendGuestEmail");
            }
            
            string sendStatus = "";
            if (!model.SendToAll)
            {
                List<string> userEmails = model.UserEmailList.Trim().ToLower().Split(',').ToList();
                List<User> users = _supperClubRepository.GetUser(userEmails);
                if (users != null && users.Count > 0)
                    sendStatus = em.SendAutomatedGuestBookedEmails(model.EventId, users, model.Comments,0,"XXXX");
                else
                    sendStatus = "None of the e-mail Ids are registered with Grubclub!";
            }
            else
            {
                sendStatus = em.SendAutomatedGuestBookedEmails(model.EventId, null, model.Comments,0,"XXXX");
            }
            model.ShowStatus = true;
            if (sendStatus.Length > 0)
                model.SendStatus = sendStatus;
            
            model.SendToAll = false;
            model.UserEmailList = "";
            
            return View(model);
        }

        #endregion

        #region Reporting

        [Authorize(Roles = "Admin")]
        public ActionResult Reports()
        {
            var reportTypes = from SupperClub.WebUI.ReportType rt in Enum.GetValues(typeof(SupperClub.WebUI.ReportType))
                              select new { Id = (int)rt, Name = rt.ToString() };
            ViewBag.ReportTypes = new SelectList(reportTypes, "Id", "Name");
            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult GetReportsList(int reportTypeEnumId)
        {
            var reportsList = new SelectList(_supperClubRepository.GetReportList(reportTypeEnumId), "Id", "Name");
            return Json(reportsList, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult GetReportParameters(int reportId)
        {
            var reportsParameters = _supperClubRepository.GetReport(reportId).Parameters;
            return Json(reportsParameters, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult RunReport(int reportId, string parameters = null)
        {
            //Get the report and generate it returning the server filename
            Report report = _supperClubRepository.GetReport(reportId);
            //Deal with optional parameters from UI
            List<Tuple<string, string>> sqlParameters = null;
            if (!string.IsNullOrEmpty(parameters))
            {
                sqlParameters = new List<Tuple<string, string>>();
                string[] splitParameters = parameters.Split(';');
                foreach (string s in splitParameters)
                {
                    string[] parameterPair = s.Trim().Split('=');
                    sqlParameters.Add(new Tuple<string, string>(parameterPair[0].Trim(), parameterPair[1].Trim()));
                }
            }

            ReportGenerator reportGenerator = new ReportGenerator();
            string fileStream = reportGenerator.GenerateReport(report, ReportFolderPath, sqlParameters);
            if (fileStream == null)
            {
                SetNotification(NotificationType.Error, "There was a problem running the report. Check the System Log for details.", true);
                return RedirectToAction("Reports");
            }

            //Set the name of the file to be generated
            DateTime now = DateTime.Now;
            string fileDownloadName = now.Year.ToString() + "." + now.Month.ToString() + "." + now.Day.ToString() + "-" + now.Hour.ToString() + "." + now.Minute.ToString();
            fileDownloadName = report.Name.Replace(" ", "_") + "_" + fileDownloadName + ".csv";
            string mimeType = "csv";

            return File(fileStream, mimeType, fileDownloadName);
        }

        #endregion

        #region Reviews

        #region Generate Review Keys

        [Authorize(Roles = "Admin")]
        public ActionResult GenerateReviewKeys(int? selectedEventId = null, int eventsDaysOld = 7)
        {
            GenerateReviewKeysModel model = new GenerateReviewKeysModel((int)eventsDaysOld);

            if (selectedEventId != null)
            {
                model.SelectedEventId = (int)selectedEventId;
                ViewBag.SelectedEvent = _supperClubRepository.GetEvent((int)selectedEventId);
            }

            ViewBag.EventsList = new SelectList(_supperClubRepository.GetPastEvents(DateTime.Now.AddDays(-model.EventsDaysOld)), "Id", "NameDateTime");
            return View("Reviews/GenerateReviewKeys", model);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult GetEventsList(DateTime eventsAfter)
        {
            // Not implemented
            var eventsList = new SelectList(_supperClubRepository.GetPastEvents(eventsAfter), "Id", "NameDateTime");
            return Json(eventsList, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult GenerateAuthToken(int eventId, bool allowGuests, string userId = null)
        {
            string authKey = "";
            ReviewAuthorisationModel auth = new ReviewAuthorisationModel();
            if(string.IsNullOrEmpty(userId))
                authKey = auth.Encrypt(eventId, allowGuests);
            else
                authKey = auth.Encrypt(eventId, userId);

            return Json(authKey, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult DecodeReviewKey(string authToken)
        {
            ReviewAuthorisationModel auth = new ReviewAuthorisationModel();
            ViewBag.DecryptSuccess = auth.Decrypt(authToken);
            return View("Reviews/DecodeReviewKey", auth);
        }

        #endregion

        #region Check Reviews

        [Authorize(Roles = "Admin")]
        public ActionResult CheckReviews(int? selectedEventId = null, int eventsDaysOld = 30)
        {
            ViewBag.EventsDaysOld = eventsDaysOld;
            ViewBag.SelectedEventId = 0;
            if (selectedEventId != null)
            {
                ViewBag.SelectedEventId = (int)selectedEventId;
                ViewBag.SelectedEvent = _supperClubRepository.GetEvent((int)selectedEventId);
            }
            ViewBag.EventsList = new SelectList(_supperClubRepository.GetPastEvents(DateTime.Now.AddDays(-(int)eventsDaysOld)), "Id", "NameDateTime");

            return View("Reviews/CheckReviews");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult CheckReview(int reviewId)
        {
            Review model = _supperClubRepository.GetReview(reviewId);
            if (model.UserId != null)
                ViewBag.UserAccount = _supperClubRepository.GetUser(model.UserId).Email;
            return View("Reviews/CheckReview", model);
        }

        [Authorize(Roles = "Admin")]
        public JsonResult FlipPublishOrUnpublish(int reviewId)
        {
            bool? newState = _supperClubRepository.FlipPublishOrUnpublish(reviewId);
            return Json(newState, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Admin")]
        public ActionResult DeleteReview(int reviewId, int eventId)
        {
            bool success = _supperClubRepository.DeleteReview(reviewId);
            if (success)
                SetNotification(NotificationType.Success, "Review Deleted", true);
            else
                SetNotification(NotificationType.Success, "Problem Deleting Review", true);

            return RedirectToAction("CheckReviews", "Admin", new { selectedEventId = eventId });
        }

        #endregion

        #endregion

        #region Tags
        [Authorize(Roles = "Admin")]
        public ActionResult CreateTag()
        {
            //Dictionary<string, int> tagPage = new Dictionary<string, int>();
            //tagPage.Add(TargetPage.Event.ToString(), (int)TargetPage.Event);
            //tagPage.Add(TargetPage.Profile.ToString(), (int)TargetPage.Profile);

            //ViewBag.TargetPage = tagPage;

            //Dictionary<string, int> tagUser = new Dictionary<string, int>();
            //tagUser.Add(TargetUser.Admin.ToString(), (int)TargetUser.Admin);
            //tagUser.Add(TargetUser.Host.ToString(), (int)TargetUser.Host);
            //tagUser.Add(TargetUser.AdminHost.ToString(), (int)TargetUser.AdminHost);

            //ViewBag.TargetUser = tagUser;

            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult CreateTag(Tag model)
        {
            LogMessage("Begin Tag Creation");
            try
            {
                int status = _supperClubRepository.ValidateTagName(model.Name, model.UrlFriendlyName);
                if (status < 0)
                {
                    if (status == -1)
                        ModelState.AddModelError("Tag Name", "A Tag with this name already exists!");
                    else if (status == -2)
                        ModelState.AddModelError("Tag SEO Friendly Name", "A Tag with same SEO name already exists!");
                }
                if (ModelState.IsValid && status == 0)
                {
                    model.UrlFriendlyName = SupperClub.Web.Helpers.Utils.GetSeoFriendlyUrl(model.UrlFriendlyName).ToLower();
                    bool success = _supperClubRepository.CreateTag(model);
                    if (success)
                    {
                        LogMessage("Created Tag Successfully");
                        SetNotification(NotificationType.Success, "Your tag has been created successfully!", true);
                    }
                }
                else
                {
                    LogMessage("Problems with Tag Creation");
                    string errorMsg = "There were problems creating your tag, please fix them and try again.";
                    foreach (ModelState modelState in ViewData.ModelState.Values)
                    {
                        foreach (ModelError error in modelState.Errors)
                        {
                            errorMsg = errorMsg + " " + error.ErrorMessage;
                        }
                    }
                    SetNotification(NotificationType.Error, errorMsg);
                }
            }
            catch (Exception ex)
            {
                LogMessage("Problem while creating Tag. Error Details: " + ex.StackTrace, LogLevel.ERROR);
                SetNotification(NotificationType.Error, "Some error occured while creating your tag, please try again!");
            }
            return View();
        }
        [Authorize(Roles = "Admin")]
        public ActionResult CreateProfileTag()
        {
            ProfileTag model = new ProfileTag();
            //Dictionary<string, int> tagPage = new Dictionary<string, int>();
            //tagPage.Add(TargetPage.Event.ToString(), (int)TargetPage.Event);
            //tagPage.Add(TargetPage.Profile.ToString(), (int)TargetPage.Profile);

            //ViewBag.TargetPage = tagPage;

            Dictionary<string, int> tagUser = new Dictionary<string, int>();
            tagUser.Add(TargetUser.Admin.ToString(), (int)TargetUser.Admin);
            tagUser.Add(TargetUser.Host.ToString(), (int)TargetUser.Host);
            tagUser.Add(TargetUser.AdminHost.ToString(), (int)TargetUser.AdminHost);

            ViewBag.TargetUser = tagUser;

            return View(model);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult CreateProfileTag(ProfileTag model)
        {
            LogMessage("Begin Profile Tag Creation");
            try
            {
                int status = _supperClubRepository.ValidateProfileTagName(model.Name);
                if (status < 0)
                {
                    if (status == -1)
                        ModelState.AddModelError("Tag Name", "A Profile Tag with this name already exists!");
                    else if (status == -2)
                        ModelState.AddModelError("Tag SEO Friendly Name", "A Profile Tag with same SEO name already exists!");
                }
                if (ModelState.IsValid && status == 0)
                {
                    //model.UrlFriendlyName = SupperClub.Web.Helpers.Utils.GetSeoFriendlyUrl(model.UrlFriendlyName).ToLower();
                    model.CreatedDate = DateTime.Now;
                    bool success = _supperClubRepository.CreateProfileTag(model);
                    if (success)
                    {
                        LogMessage("Created Profile Tag Successfully");
                        SetNotification(NotificationType.Success, "Your Profile tag has been created successfully!", true);
                    }
                }
                else
                {
                    LogMessage("Problems with Profile Tag Creation");
                    string errorMsg = "There were problems creating your Profile tag, please fix them and try again.";
                    foreach (ModelState modelState in ViewData.ModelState.Values)
                    {
                        foreach (ModelError error in modelState.Errors)
                        {
                            errorMsg = errorMsg + " " + error.ErrorMessage;
                        }
                    }
                    SetNotification(NotificationType.Error, errorMsg);
                }
            }
            catch (Exception ex)
            {
                LogMessage("Problem while creating Tag. Error Details: " + ex.StackTrace, LogLevel.ERROR);
                SetNotification(NotificationType.Error, "Some error occured while creating your tag, please try again!");
            }
            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult EditTag()
        {
            ViewBag.Tags = new SelectList(_supperClubRepository.GetTags(), "Id", "Name"); 
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult EditTag(Tag model)
        {
            LogMessage("Begin Edit Tag");
            try
            {
                int status = _supperClubRepository.ValidateTagName(model.Name, model.UrlFriendlyName, model.Id);
                if (status < 0)
                {
                    if (status == -1)
                        ModelState.AddModelError("Tag Name", "A Tag with this name already exists!");
                    else if (status == -2)
                        ModelState.AddModelError("Tag SEO Friendly Name", "A Tag with same SEO name already exists!");
                }
                if (ModelState.IsValid && status == 0)
                {
                    bool success = _supperClubRepository.UpdateTag(model);
                    if (success)
                    {
                        LogMessage("Updated Tag Successfully");
                        SetNotification(NotificationType.Success, "Your tag has been updated successfully!", true);
                    }
                }
                else
                {
                    LogMessage("Problems editing tag");
                    string errorMsg = "There were problems updating your tag, please fix them and try again.";
                    foreach (ModelState modelState in ViewData.ModelState.Values)
                    {
                        foreach (ModelError error in modelState.Errors)
                        {
                            errorMsg = errorMsg + " " + error.ErrorMessage;
                        }
                    }
                    SetNotification(NotificationType.Error, errorMsg);
                }
            }
            catch (Exception ex)
            {
                LogMessage("Problem while updating Tag. Error Details: " + ex.StackTrace, LogLevel.ERROR);
                SetNotification(NotificationType.Error, "Some error occured while updating your tag, please try again!");
            }
            ViewBag.Tags = new SelectList(_supperClubRepository.GetTags(), "Id", "Name"); 
            Tag _tag = new Tag();            
            return View(_tag);
        }


        public ActionResult EditProfileTag()
        {
            ViewBag.Tags = new SelectList(_supperClubRepository.GetProfileTags(), "Id", "Name");
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult EditProfileTag(ProfileTag model)
        {
            LogMessage("Begin Edit Tag");
            try
            {
                int status = _supperClubRepository.ValidateProfileTagName(model.Name, model.Id);
                if (status < 0)
                {
                    if (status == -1)
                        ModelState.AddModelError("Tag Name", "A Tag with this name already exists!");
                    else if (status == -2)
                        ModelState.AddModelError("Tag SEO Friendly Name", "A Tag with same SEO name already exists!");
                }
                if (ModelState.IsValid && status == 0)
                {
                    bool success = _supperClubRepository.UpdateProfileTag(model);
                    if (success)
                    {
                        LogMessage("Updated Tag Successfully");
                        SetNotification(NotificationType.Success, "Your tag has been updated successfully!", true);
                    }
                }
                else
                {
                    LogMessage("Problems editing tag");
                    string errorMsg = "There were problems updating your tag, please fix them and try again.";
                    foreach (ModelState modelState in ViewData.ModelState.Values)
                    {
                        foreach (ModelError error in modelState.Errors)
                        {
                            errorMsg = errorMsg + " " + error.ErrorMessage;
                        }
                    }
                    SetNotification(NotificationType.Error, errorMsg);
                }
            }
            catch (Exception ex)
            {
                LogMessage("Problem while updating Tag. Error Details: " + ex.StackTrace, LogLevel.ERROR);
                SetNotification(NotificationType.Error, "Some error occured while updating your tag, please try again!");
            }
            ViewBag.Tags = new SelectList(_supperClubRepository.GetProfileTags(), "Id", "Name");
            ProfileTag _tag = new ProfileTag();
            return View(_tag);
        }
        [Authorize(Roles = "Admin")]
        public JsonResult GetTag(int tagId = 0)
        {
            try
            {
                if (tagId == 0)
                {
                    LogMessage("GetTag: tagId is not passed.", LogLevel.ERROR);
                    return Json(new { error = "tagId not passed." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogMessage("Getting tag details");
                    var _tag = _supperClubRepository.GetTag(tagId);
                    LogMessage("Fetched tag details successfully.", LogLevel.DEBUG);
                    return Json(new { Name =_tag.Name,
                                     UrlFriendlyName = _tag.UrlFriendlyName,
                                     MetaTitle = _tag.MetaTitle,
                                     MetaDescription = _tag.MetaDescription,
                                     H2Tag = _tag.H2Tag,
                                     TagDescription = _tag.TagDescription,
                                     TargetUser=_tag.TargetUser,
                                     Private=_tag.Private}
                                    , JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage("GetTag: Error getting tag deatils"+ "  Error Details:" + ex.Message + "  StackTrace:" + ex.StackTrace);
                return Json(new { error = "Error occurred while getting tag details" }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize(Roles = "Admin")]
        public JsonResult GetProfileTag(int tagId = 0)
        {
            try
            {
                if (tagId == 0)
                {
                    LogMessage("GetTag: tagId is not passed.", LogLevel.ERROR);
                    return Json(new { error = "tagId not passed." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogMessage("Getting tag details");
                    var _tag = _supperClubRepository.GetProfileTag(tagId);
                    LogMessage("Fetched tag details successfully.", LogLevel.DEBUG);
                    return Json(new
                    {
                        Name = _tag.Name,
                        TargetUser = _tag.TargetUser,
                        Private = _tag.Private,
                        CreatedDate=_tag.CreatedDate
                    }
                                    , JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage("GetTag: Error getting tag deatils" + "  Error Details:" + ex.Message + "  StackTrace:" + ex.StackTrace);
                return Json(new { error = "Error occurred while getting tag details" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult SupperClubTag()
        {
          
            ViewBag.SupperClubs = new SelectList(_supperClubRepository.GetAllActiveSupperClubs(), "Id", "Name");
            IList<ProfileTag> lstProfileTags = _supperClubRepository.GetProfileTags();

            ViewBag.Tags = lstProfileTags;
           
            return View();
            
        }

        public ActionResult AddSupperClubTag(int supperClubId, string profileTags)
        {
            bool status = false;
            try
            {
                if (supperClubId > 0)
                {
                    status = _supperClubRepository.UpdateSupperClubProfileTags(supperClubId, profileTags);

                    LogMessage("Updated SupperClub Profile tags Successfully");
                    SetNotification(NotificationType.Success, "Your SupperClub Profile tags has been updated successfully!", true);

 
                }
            }
            catch (Exception ex)
            {
                LogMessage("Problem while updating SupperClub Profile tags. Error Details: " + ex.StackTrace, LogLevel.ERROR);
                SetNotification(NotificationType.Error, "Some error occured while updating your SupperClub Profile tags, please try again!");
            }
            return Json(true, JsonRequestBehavior.AllowGet);

        }
        public JsonResult GetSupperClubTag(int supperClubId=0)
        {

            try
            {
                if (supperClubId == 0)
                {
                    LogMessage("GetSupperClub: SupperClubId is not passed.", LogLevel.ERROR);
                    return Json(new { error = "SupperClubId not passed." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogMessage("Getting tag details");
                    var _tags = _supperClubRepository.GetSupperClubProfileTags(supperClubId);
                    LogMessage("Fetched tag details successfully.", LogLevel.DEBUG);
                    return Json(new
                    {
                       tags=_tags
                    }
                                    , JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage("GetTag: Error getting tag deatils" + "  Error Details:" + ex.Message + "  StackTrace:" + ex.StackTrace);
                return Json(new { error = "Error occurred while getting tag details" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Category
        [Authorize(Roles = "Admin")]
        public ActionResult CreateCategory()
        {
            ViewBag.Tags = _supperClubRepository.GetTags();
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult CreateCategory(SearchCategory model)
        {
            LogMessage("Begin Category Creation");
            try
            {
                int status = _supperClubRepository.ValidateCategoryName(model.Name, model.UrlFriendlyName);
                if (status < 0)
                {
                    if (status == -1)
                        ModelState.AddModelError("Category Name", "A category with this name already exists!");                    
                    else if (status == -2)
                        ModelState.AddModelError("Category SEO Friendly Name", "A category with same SEO name already exists!");
                }
                if (ModelState.IsValid && status == 0)
                {
                    model.UrlFriendlyName = SupperClub.Web.Helpers.Utils.GetSeoFriendlyUrl(model.UrlFriendlyName).ToLower();
                    bool success = _supperClubRepository.CreateCategory(model);
                    if (success)
                    {
                        LogMessage("Created Category Successfully");
                        SetNotification(NotificationType.Success, "Your category has been created successfully!", true);
                    }
                }
                else
                {
                    LogMessage("Problems with Category Creation");
                    string errorMsg = "There were problems creating your category, please fix them and try again.";
                    foreach (ModelState modelState in ViewData.ModelState.Values)
                    {
                        foreach (ModelError error in modelState.Errors)
                        {
                            errorMsg = errorMsg + " " + error.ErrorMessage;
                        }
                    }
                    SetNotification(NotificationType.Error, errorMsg);
                }
            }
            catch (Exception ex)
            {
                LogMessage("Problem while creating Category. Error Details: " + ex.StackTrace, LogLevel.ERROR);
                SetNotification(NotificationType.Error, "Some error occured while creating your category, please try again!");
            }
            ViewBag.Tags = _supperClubRepository.GetTags();
            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult EditCategory()
        {
            ViewBag.SearchCategories = new SelectList(_supperClubRepository.GetSearchCategories(), "Id", "Name");
            ViewBag.Tags = _supperClubRepository.GetTags();
            return View();
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult EditCategory(SearchCategory model)
        {
            LogMessage("Begin Edit Category");
            SearchCategory _searchCategory = new SearchCategory();
            try
            {
                int status = _supperClubRepository.ValidateCategoryName(model.Name, model.UrlFriendlyName, model.Id);
                if (status < 0)
                {
                    if (status == -1)
                        ModelState.AddModelError("Category Name", "A category with this name already exists!");
                    else if (status == -2)
                        ModelState.AddModelError("Category SEO Friendly Name", "A category with same SEO name already exists!");
                }
                if (ModelState.IsValid && status == 0)
                {
                    bool success = _supperClubRepository.UpdateCategory(model);
                    if (success)
                    {
                        LogMessage("Updated Category Successfully");
                        SetNotification(NotificationType.Success, "Your category has been updated successfully!", true);
                    }
                    _searchCategory.Name = "";
                    _searchCategory.UrlFriendlyName = "";
                }
                else
                {
                    LogMessage("Problems editing category");
                    string errorMsg = "There were problems updating your category, please fix them and try again.";
                    foreach (ModelState modelState in ViewData.ModelState.Values)
                    {
                        foreach (ModelError error in modelState.Errors)
                        {
                            errorMsg = errorMsg + " " + error.ErrorMessage;
                        }
                    }
                    SetNotification(NotificationType.Error, errorMsg);
                }
            }
            catch (Exception ex)
            {
                LogMessage("Problem while updating category. Error Details: " + ex.StackTrace, LogLevel.ERROR);
                SetNotification(NotificationType.Error, "Some error occured while updating your category, please try again!");
            }
            ViewBag.SearchCategories = new SelectList(_supperClubRepository.GetSearchCategories(), "Id", "Name");
            ViewBag.Tags = _supperClubRepository.GetTags();  
            return View(_searchCategory);
        }

        [Authorize(Roles = "Admin")]
        public JsonResult GetSearchCategory(int searchCategoryId = 0)
        {
            try
            {
                if (searchCategoryId == 0)
                {
                    LogMessage("GetSearchCategory: searchCategoryId is not passed.", LogLevel.ERROR);
                    return Json(new { error = "searchCategoryId not passed." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogMessage("Getting category details");
                    SearchCategory _searchCategory = _supperClubRepository.GetSearchCategory(searchCategoryId);
                    LogMessage("Fetched category details successfully.", LogLevel.DEBUG);
                    //getting weird behavior with directly using TagList so fetching tag Ids manually to make sure we get the value everytime
                    string tagList="";
                    foreach (int tagId in _searchCategory.SelectedTagIds)
                    {
                        tagList += tagId.ToString() + ",";                        
                    }
                    if (tagList.Length > 0)
                        tagList = tagList.Substring(0, tagList.Length - 1);
                    return Json(new
                    {
                        Name = _searchCategory.Name,
                        TagList = _searchCategory.TagList,
                        UrlFriendlyName = _searchCategory.UrlFriendlyName                        
                    }
                                    , JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage("GetSearchCategory: Error getting category deatils" + "  Error Details:" + ex.Message + "  StackTrace:" + ex.StackTrace);
                return Json(new { error = "Error occurred while getting category details" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
        #region cache management
        [Authorize(Roles = "Admin")]
        public JsonResult FlushAppCache()
        {
            List<string> keys = new List<string>();
            bool status = false;
            try
            {
                // retrieve application Cache enumerator
                IDictionaryEnumerator enumerator = System.Web.HttpContext.Current.Cache.GetEnumerator();

                // copy all keys that currently exist in Cache
                while (enumerator.MoveNext())
                {
                    keys.Add(enumerator.Key.ToString());
                }
                // delete every key from cache
                for (int i = 0; i < keys.Count; i++)
                {
                    System.Web.HttpContext.Current.Cache.Remove(keys[i]);
                }
                status = true;
            }
            catch(Exception ex)
            {
                log.Error("Error resetting application cache. Error Message:" + ex.Message + " Stack Trace:" + ex.StackTrace);
            }
            return Json(status, JsonRequestBehavior.AllowGet);
        }
        public ActionResult FlushCache()
        {
            return View();
        }
        
        #endregion

        #region Vouchers
        [Authorize(Roles = "Admin")]
        public ActionResult AllActiveVouchers()
        {
            AllVouchersModel model = new AllVouchersModel();
            model.AllActiveVouchers = _supperClubRepository.GetAllActiveVouchers();
            model.AllInactiveVouchers = _supperClubRepository.GetAllInactiveVouchers();
            model.VoucherCode = "";
            return View(model);
        }        
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult AllActiveVouchers(AllVouchersModel model)
        {
            string notificationMessage = "";
            bool isValid = _supperClubRepository.CheckVoucherCode(model.VoucherCode);
            if (!isValid)
            {
                notificationMessage = "Invalid voucher code! This voucher Code does not exist.";
                SetNotification(Models.NotificationType.Warning, notificationMessage, true);
            }
            else
            {
                bool success = _supperClubRepository.DeactivateVoucher(model.VoucherCode);
                notificationMessage = (success ? "Voucher deactivated successfully": "Failed to deactivate voucher. Please try again!");
                SetNotification(success? Models.NotificationType.Success : Models.NotificationType.Warning, notificationMessage, true);                
            }
            model.AllActiveVouchers = _supperClubRepository.GetAllActiveVouchers();
            model.AllInactiveVouchers = _supperClubRepository.GetAllInactiveVouchers();
            model.VoucherCode = "";
            return View(model);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public JsonResult ChangeVoucherStatus(int voucherId, bool newStatus)
        {
            bool success = _supperClubRepository.ChangeVoucherStatus(voucherId, newStatus);
            return Json(success, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult CreateVoucher()
        {
            Voucher model = new Voucher();
            model.OwnerId = (int)VoucherOwner.Admin;
            model.IsGlobal = true;
            model.NumberOfTimesUsed = 0;
            model.UniqueUserRedeemLimit = 1;
            model.UsageCap = 1;
            model.Active = true;

            int i = 0;
            string voucherCode = "";
            do
            {
                voucherCode = SupperClub.Web.Helpers.Utils.GetUniqueKey(10);
                bool isExisting = _supperClubRepository.CheckVoucherCode(voucherCode);
                if (!isExisting)
                {
                    model.Code = voucherCode;
                    i = 1;
                }
            }
            while (i == 0);

            return View(model);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult CreateVoucher(Voucher model)
        {
            LogMessage("Begin Voucher Creation");
            Voucher voucher = new Voucher();
            
            try
            {                
                //model.UniqueUserRedeemLimit = model.UsageCap;
                //model.UniqueUserRedeemLimit = 1;
                model.CreatedDate = DateTime.Now;                
                bool dtValid = true;
                string sError = "";
                if (model.StartDate != null && model.StartDate < DateTime.Now)
                {
                    dtValid = false;
                    sError = "Start date can not be in past!";
                }
                if (model.ExpiryDate != null && model.ExpiryDate < DateTime.Now)
                {
                    dtValid = false;
                    sError = "Expiry date can not be in past!";
                }
                if (model.StartDate != null && model.ExpiryDate != null && model.StartDate > model.ExpiryDate)
                {
                    dtValid = false;
                    sError = "Expiry date can not be prior to start date!";
                }
                if (string.IsNullOrEmpty(model.Code))
                {
                    dtValid = false;
                    sError = "Voucher code field can not be blank!";
                }
                if (string.IsNullOrEmpty(model.Code) || model.Code.Length < 7)
                {
                    dtValid = false;
                    sError = "Voucher code should be atleast 7 character long!";
                }
                bool isExisting = _supperClubRepository.CheckVoucherCode(model.Code);
                if (isExisting)
                {
                    dtValid = false;
                    sError = "Voucher name already exists!";
                    
                }
                if (!dtValid)
                {
                    LogMessage("Problem while creating voucher. Error Details: " + sError , LogLevel.WARN);
                    SetNotification(Models.NotificationType.Warning, sError, true);
                }
                
                if (ModelState.IsValid)
                {
                    //int i = 0;
                    //string voucherCode = "";
                    //do
                    //{
                    //    voucherCode = SupperClub.Web.Helpers.Utils.GetUniqueKey(10);
                    //    bool isExisting = _supperClubRepository.CheckVoucherCode(voucherCode);
                    //    if (!isExisting)
                    //    {
                    //        model.Code = voucherCode;
                    //        i = 1;
                    //    }
                    //}
                    //while (i == 0);

                    if(model.TypeId == (int)VoucherType.GiftVoucher)
                    {
                        model.AvailableBalance = model.OffValue;
                        model.CreatedDate = DateTime.Now;
                        model.StartDate = DateTime.Now;                        
                        model.ExpiryDate = DateTime.Today.AddYears(1);
                    }
                    voucher = _supperClubRepository.CreateVoucher(model);
                    if (voucher != null && voucher.Id > 0)
                    {
                        LogMessage("Created Voucher Successfully. VoucherId:" + model.Id.ToString() + " VoucherCode:" + model.Code);
                        SetNotification(Models.NotificationType.Success, "Voucher Created Successfully!", true);
                        ViewBag.VoucherCode = voucher.Code;
                    }
                    else
                    {
                        LogMessage("Problems with Voucher Creation. Vouchar Code already exist. Voucher Code:" + voucher.Code);
                        string errorMsg = "There were problems creating your Voucher. Vouchar Code already exists.";
                        SetNotification(Models.NotificationType.Warning, errorMsg, true);
                        ViewBag.VoucherCode = null;
                    }
                    
                }
                else
                {
                    voucher = model;
                    
                    LogMessage("Problems with Voucher Creation");
                    string errorMsg = "There were problems creating your Voucher, please fix them and try again.";
                    foreach (ModelState modelState in ViewData.ModelState.Values)
                    {
                        foreach (ModelError error in modelState.Errors)
                        {
                            errorMsg = errorMsg + " " + error.ErrorMessage;
                        }
                    }
                    SetNotification(Models.NotificationType.Warning, errorMsg, true);
                }                
            }
            catch (Exception ex)
            {
                voucher = model;
                LogMessage("Problem while creating voucher. Error Details: " + ex.StackTrace, LogLevel.ERROR);
                SetNotification(Models.NotificationType.Warning, ex.Message, true);
            }
            return View(voucher);
        }
        public JsonResult IsExistingVoucherCode(string code)
        {
                bool isExisting = _supperClubRepository.CheckVoucherCode(code);
                return Json(new { IsExisting = isExisting }, JsonRequestBehavior.AllowGet);            
        }
        #endregion

        #region CMS


        [Authorize(Roles = "Admin")]
        public ActionResult CMS()
        {
            return View("CMS/Cms");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Home()
        {
            NewCMSHomeModel model = new NewCMSHomeModel();
            model.PageCMS = _supperClubRepository.GetCMSDetailsByPage("Home");

            ViewBag.Hero = model.PageCMS.Where(x => x.Section == "Hero").ToList<PageCMS>();
            ViewBag.Curated = model.PageCMS.Where(x => x.Section == "Curated").ToList<PageCMS>();
            ViewBag.OurStories = model.PageCMS.Where(x => x.Section == "OurStories").ToList<PageCMS>();

            return View("CMS/Home");
        }
        [Authorize(Roles = "Admin")]
        [AcceptVerbs(HttpVerbs.Post)]

        //public ActionResult Home(string TextLine1, string TextLine2, string Link, string file)
        public ActionResult Home(object title)
        {
            try
            {
                string path1 = string.Empty;
                if (Request.Files["file"].ContentLength > 0)
                {
                    string extension = System.IO.Path.GetExtension(Request.Files["file"].FileName);
                    var fileName = System.IO.Path.GetFileName(Request.Files["file"].FileName);
                    path1 = string.Format("{0}\\{1}", Server.MapPath("../content/images/cms"), fileName);
                    if (System.IO.File.Exists(path1))
                        System.IO.File.Delete(path1);

                    Request.Files["file"].SaveAs(path1);
                }

                PageCMS cms = new PageCMS();
                cms.Id = int.Parse(Request.Form["heroId"].ToString());
                cms.TextLine1 = Request.Form["herotext1"].ToString();
                cms.TextLine2 = Request.Form["herotext2"] == null ? "" : Request.Form["herotext2"].ToString();
                cms.Link = Request.Form["heroLink"].ToString();
                if (!string.IsNullOrEmpty(path1))
                    cms.ImagePath = "../content/images/cms/" + System.IO.Path.GetFileName(Request.Files["file"].FileName);
                else
                    cms.ImagePath = Request.Form["heroOldFilePath"].ToString();
                cms.Page = Request.Form["heroPage"].ToString();
                cms.Section = Request.Form["heroSection"].ToString();

                bool status = _supperClubRepository.UpdatePageCMS(cms);
            }
            catch (Exception ex)
            {
                LogMessage("updating cms  failed " + "  Error Details:" + ex.Message + "  StackTrace:" + ex.StackTrace);
            }


            NewCMSHomeModel model = new NewCMSHomeModel();
            model.PageCMS = _supperClubRepository.GetCMSDetailsByPage("Home");

            ViewBag.Hero = model.PageCMS.Where(x => x.Section == "Hero").ToList<PageCMS>();
            ViewBag.Curated = model.PageCMS.Where(x => x.Section == "Curated").ToList<PageCMS>();
            ViewBag.OurStories = model.PageCMS.Where(x => x.Section == "OurStories").ToList<PageCMS>();

            return View("CMS/Home");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Foodies()
        {
            NewCMSHomeModel model = new NewCMSHomeModel();
            model.PageCMS = _supperClubRepository.GetCMSDetailsByPage("Foodies");

            ViewBag.Hero = model.PageCMS.Where(x => x.Section == "Hero").ToList<PageCMS>();
            ViewBag.Curated = model.PageCMS.Where(x => x.Section == "Curated").ToList<PageCMS>();
            ViewBag.OurStories = model.PageCMS.Where(x => x.Section == "OurStories").ToList<PageCMS>();

            return View("CMS/Foodies");
        }

        [Authorize(Roles = "Admin")]
        [AcceptVerbs(HttpVerbs.Post)]

        //public ActionResult Home(string TextLine1, string TextLine2, string Link, string file)
        public ActionResult Foodies(object title)
        {
            try
            {
                string path1 = string.Empty;
                if (Request.Files["file"].ContentLength > 0)
                {
                    string extension = System.IO.Path.GetExtension(Request.Files["file"].FileName);
                    var fileName = System.IO.Path.GetFileName(Request.Files["file"].FileName);
                    path1 = string.Format("{0}\\{1}", Server.MapPath("../content/images/cms/foodies"), fileName);
                    if (System.IO.File.Exists(path1))
                        System.IO.File.Delete(path1);

                    Request.Files["file"].SaveAs(path1);
                }

                PageCMS cms = new PageCMS();
                cms.Id = int.Parse(Request.Form["heroId"].ToString());
                cms.TextLine1 = Request.Form["herotext1"].ToString();
                cms.TextLine2 = Request.Form["herotext2"] == null ? "" : Request.Form["herotext2"].ToString();
                cms.Link = Request.Form["heroLink"].ToString();
                if (!string.IsNullOrEmpty(path1))
                    cms.ImagePath = "../content/images/cms/foodies/" + System.IO.Path.GetFileName(Request.Files["file"].FileName);
                else
                    cms.ImagePath = Request.Form["heroOldFilePath"].ToString();
                cms.Page = Request.Form["heroPage"].ToString();
                cms.Section = Request.Form["heroSection"].ToString();

                bool status = _supperClubRepository.UpdatePageCMS(cms);
            }
            catch (Exception ex)
            {
                LogMessage("updating cms  failed " + "  Error Details:" + ex.Message + "  StackTrace:" + ex.StackTrace);
            }


            NewCMSHomeModel model = new NewCMSHomeModel();
            model.PageCMS = _supperClubRepository.GetCMSDetailsByPage("Foodies");

            ViewBag.Hero = model.PageCMS.Where(x => x.Section == "Hero").ToList<PageCMS>();
            ViewBag.Curated = model.PageCMS.Where(x => x.Section == "Curated").ToList<PageCMS>();
            ViewBag.OurStories = model.PageCMS.Where(x => x.Section == "OurStories").ToList<PageCMS>();

            return View("CMS/Foodies");
        }


        [Authorize(Roles = "Admin")]
        public ActionResult PopularGrubClubs()
        {

            CMSPopularGrubClubsModel model = new CMSPopularGrubClubsModel();

            model.PopularEvents = _supperClubRepository.GetPopularEventsWithAdminRank();
            return View("CMS/PopularGrubClubs", model);
        }




        [Authorize(Roles = "Admin")]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult PopularGrubClubs(CMSPopularGrubClubsModel model)
        {
            // model.PopularEvents = new List<PopularEventAdmin>();

            List<PopularEventAdminRank> lstEvent = new List<PopularEventAdminRank>();

            model.PopularEvents = _supperClubRepository.GetPopularEventsWithAdminRank();

            for (int i = 0; i <= model.PopularEvents.Count-1; i++)
            {
                PopularEventAdminRank _event = new PopularEventAdminRank();

                _event.EventId = model.PopularEvents[i].popularEvent.EventId;//int.Parse(Request.Form.AllKeys[i]);
                _event.Rank = int.Parse(Request.Form[_event.EventId.ToString()].ToString());//int.Parse(Request.Form[Request.Form.AllKeys[i]]);
                string keyurl = "eventUrl " + _event.EventId.ToString();
                _event.EventUrl = Request.Form[keyurl].ToString();

                //   _supperClubRepository.CreatePopularEventAdminRank(_event);

                lstEvent.Add(_event);

            }
            _supperClubRepository.UpdatePopularEventAdminRank(lstEvent);
            //  _supperClubRepository.UpdatePopularEventAdminRank(model.PopularEvents);
            model.PopularEvents = _supperClubRepository.GetPopularEventsWithAdminRank();
            return View("CMS/PopularGrubClubs", model);

        }

        [Authorize(Roles = "Admin")]
        public ActionResult HomeTest()
        {
            NewCMSHomeModel model = new NewCMSHomeModel();
            model.PageCMS = _supperClubRepository.GetCMSDetailsByPage("HomeTest");

            ViewBag.Hero = model.PageCMS.Where(x => x.Section == "Hero").ToList<PageCMS>();
            ViewBag.Curated = model.PageCMS.Where(x => x.Section == "Curated").ToList<PageCMS>();
            ViewBag.OurStories = model.PageCMS.Where(x => x.Section == "OurStories").ToList<PageCMS>();

            return View("CMS/HomeTest");
        }

        [Authorize(Roles = "Admin")]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult HomeTest(object title)
        {
            try
            {
                string path1 = string.Empty;
                if (Request.Files["file"].ContentLength > 0)
                {
                    string extension = System.IO.Path.GetExtension(Request.Files["file"].FileName);
                    var fileName = System.IO.Path.GetFileName(Request.Files["file"].FileName);
                    path1 = string.Format("{0}\\{1}", Server.MapPath("../content/images/cms"), fileName);
                    if (System.IO.File.Exists(path1))
                        System.IO.File.Delete(path1);

                    Request.Files["file"].SaveAs(path1);
                }

                PageCMS cms = new PageCMS();
                cms.Id = int.Parse(Request.Form["heroId"].ToString());
                cms.TextLine1 = Request.Form["herotext1"].ToString();
                cms.TextLine2 = Request.Form["herotext2"] == null ? "" : Request.Form["herotext2"].ToString();
                cms.Link = Request.Form["heroLink"].ToString();
                if (!string.IsNullOrEmpty(path1))
                    cms.ImagePath = "../content/images/cms/" + System.IO.Path.GetFileName(Request.Files["file"].FileName);
                else
                    cms.ImagePath = Request.Form["heroOldFilePath"].ToString();
                cms.Page = Request.Form["heroPage"].ToString();
                cms.Section = Request.Form["heroSection"].ToString();

                bool status = _supperClubRepository.UpdatePageCMS(cms);
            }
            catch (Exception ex)
            {
                LogMessage("updating cms  failed " + "  Error Details:" + ex.Message + "  StackTrace:" + ex.StackTrace);
            }


            NewCMSHomeModel model = new NewCMSHomeModel();
            model.PageCMS = _supperClubRepository.GetCMSDetailsByPage("HomeTest");

            ViewBag.Hero = model.PageCMS.Where(x => x.Section == "Hero").ToList<PageCMS>();
            ViewBag.Curated = model.PageCMS.Where(x => x.Section == "Curated").ToList<PageCMS>();
            ViewBag.OurStories = model.PageCMS.Where(x => x.Section == "OurStories").ToList<PageCMS>();

            return View("CMS/HomeTest");
        }

        [Authorize(Roles = "Admin")]
        public ActionResult CMSImages()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult UploadFile()
        {


            if (Request.Files["file"].ContentLength > 0)
            {
                string extension = System.IO.Path.GetExtension(Request.Files["file"].FileName);
                var fileName = System.IO.Path.GetFileName(Request.Files["file"].FileName);
                string path1 = string.Format("{0}\\{1}", Server.MapPath("../content/images/cms"), fileName);
                if (System.IO.File.Exists(path1))
                    System.IO.File.Delete(path1);

                Request.Files["file"].SaveAs(path1);


            }

            // return  View("CMS/Home");
            return Json(true, JsonRequestBehavior.AllowGet);
        }



    }

        #endregion

    }

