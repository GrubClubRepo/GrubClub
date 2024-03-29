﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SupperClub.Models;
using SupperClub.Domain;
using SupperClub.Domain.Repository;
using SupperClub.Services;
using System.Configuration;
using SupperClub.Code;
using SagePayMvc;
using SupperClub.Web.Helpers;
using System.Web.Configuration;
using PayPalMvc;
using PayPalMvc.Enums;
using SupperClub.Logger;
using System.Web.Script.Serialization;
using System.Web.Security;
using System.Text.RegularExpressions;
using Braintree;
using Braintree.Exceptions;
using Newtonsoft.Json;

namespace SupperClub.Controllers
{
    // Switches SSL enforcement in production vs local (or setting ForceSSL to false in web.config)
    [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
    public class BookingController : BaseController
    {

        #region Instantiate Controller

        private TicketingService _ticketingService;
        private ITransactionService _transactionService;
        private CreditCardType _testCardType;
        private string braintreeMerchantId = WebConfigurationManager.AppSettings["BraintreeMerchantId"];
        private string braintreePublicKey = WebConfigurationManager.AppSettings["BraintreePublicKey"];
        private string braintreePrivateKey = WebConfigurationManager.AppSettings["BraintreePrivateKey"];
        private string braintreeEnvironmentSetting = WebConfigurationManager.AppSettings["BraintreeEnvironment"]; //could be sandbox or live
        private Braintree.Environment braintreeEnvironment = Braintree.Environment.SANDBOX;
        private string _eventBookingCookieKey = WebConfigurationManager.AppSettings["EventBookingCookieKey"];
        private int _eventBookingCookieExpirationInMonths = int.Parse(WebConfigurationManager.AppSettings["EventBookingCookieExpirationInMonths"]);

        public BookingController(ISupperClubRepository supperClubRepository, ITransactionService transactionService)
        {
            _supperClubRepository = supperClubRepository;
            _transactionService = transactionService;
            _ticketingService = new TicketingService(_supperClubRepository);
            _testCardType = (CreditCardType)int.Parse(WebConfigurationManager.AppSettings["TestCreditCard"]);
        }

        #endregion

        #region Choose Tickets


        [Authorize]
        public ActionResult ChooseTickets()
        {
            LogMessage("Choosing Tickets");
            BookingConfirmModel confirmmodel = new BookingConfirmModel();
            if (_ticketingService.TicketBasketAlive(this.HttpContext))
            {
                ChooseTicketsModel model = new ChooseTicketsModel();
                model.BookingModel = _ticketingService.GetBookingModelFromBasket(this.HttpContext);

                int minTicket = 0;
                if (model.BookingModel.MinMaxBookingEnabled)
                {
                    minTicket = model.BookingModel.MinTicketsAllowed;
                }

                // Set up the Ticket Chooser DropDown List (currently available plus what they have already booked
                int numberAvailableTickets = _ticketingService.GetCurrentAvailableTicketsForEvent(model.BookingModel.eventId, model.BookingModel.seatingId) + model.BookingModel.numberOfTickets;
                int maxTickets = int.Parse(System.Configuration.ConfigurationManager.AppSettings["MaxTicketsAllowed"]);
                if (model.BookingModel.MinMaxBookingEnabled)
                    maxTickets = model.BookingModel.MaxTicketsAllowed;

                if (numberAvailableTickets > maxTickets) // Set a maximum in the drop down
                    numberAvailableTickets = maxTickets;
                else if (numberAvailableTickets == 0) // No more tickets, only show what the current user has allocated
                    numberAvailableTickets = model.BookingModel.numberOfTickets;

                model.NumberTicketsList = GetAvailableTickets(numberAvailableTickets, model.BookingModel.numberOfTickets, ((model.BookingModel.bookingMenuModel != null && model.BookingModel.bookingMenuModel.Count > 0) ? 0 : (model.BookingModel.MinMaxBookingEnabled ? model.BookingModel.MinTicketsAllowed : 1)), model.BookingModel.SeatSelectionInMultipleOfMin ? model.BookingModel.MinTicketsAllowed : 1);

                // return View(model);
                confirmmodel.ChooseTicketModel = model;

                CreditCardDetailsModel ccdmodel = (_testCardType == CreditCardType.NONE) ? new CreditCardDetailsModel() : GetTestCreditCardModel(_testCardType);
                CreditCardDetailsViewModel cvmodel = new CreditCardDetailsViewModel { bookingModel = model.BookingModel, creditCardDetails = ccdmodel };
                cvmodel.CreditCardTypes = GetCardTypes("");
                confirmmodel.CreditCardDetailsViewModel = cvmodel;
                Event _event = _supperClubRepository.GetEvent(model.BookingModel.eventId);

                SupperClub.Domain.SupperClub _supperclub = _supperClubRepository.GetSupperClub(_event.SupperClubId);

                ViewBag.EventHostName = _supperclub.Name;
                ViewBag.Ratings = _supperclub.AverageRank;
                ViewBag.Reviews = _supperclub.NumberOfReviews;
                ViewBag.PostCode = _event.PostCode;
                ViewBag.Byob = _event.Alcohol;
                ViewBag.StartDate = _event.Start.ToShortDateString();
                ViewBag.StartTime = _event.Start.ToShortTimeString();
                ViewBag.EventImage = _event.ImagePath;
                ViewBag.Lattitude = _event.Latitude;
                ViewBag.Longitude = _event.Longitude;

                return View("BookingPayment", confirmmodel);
            }
            else
                return RedirectToAction("SessionExpired");
        }



        [Authorize]
        public ActionResult UpdateTickets_old(int numberTicketsRequested, int seatingId, string strBmm)
        {
            //LogMessage("Updating Tickets: {0}");
            UpdateTicketsResult updateTicketsResult = new UpdateTicketsResult();
            if (_ticketingService.TicketBasketAlive(this.HttpContext))
            {
                // Service should return object as below
                BookingModel bookingModel = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                bookingModel.seatingId = seatingId;

                int currentBlockedTicketsForUser = _supperClubRepository.GetNumberTicketsInProgressForUser(bookingModel.eventId, seatingId, SupperClub.Code.UserMethods.CurrentUser.Id);
                int maxTickets = int.Parse(System.Configuration.ConfigurationManager.AppSettings["MaxTicketsAllowed"]);
                if (bookingModel.MinMaxBookingEnabled)
                    maxTickets = bookingModel.MaxTicketsAllowed;
                // In case of manual request
                if (numberTicketsRequested > maxTickets)
                    numberTicketsRequested = maxTickets;

                LogMessage(string.Format("Updating Tickets - EventID: {0}, User Id: {3}, Seating Id: {1}, Number of tickets: {2}", bookingModel.eventId, bookingModel.seatingId, bookingModel.numberOfTickets, UserMethods.CurrentUser.Id));

                if (bookingModel.bookingMenuModel != null && bookingModel.bookingMenuModel.Count > 0)
                {
                    //Separators
                    string[] stringSeparators = new string[] { "," };
                    string[] fieldSeparators = new string[] { "|" };
                    Dictionary<int, int> dict = new Dictionary<int, int>();
                    string[] tags = strBmm.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < tags.Length; i++)
                    {
                        string[] fields = tags[i].Split(fieldSeparators, StringSplitOptions.None);
                        int key = Int32.Parse(fields[0]);
                        int value = Int32.Parse(fields[1]);
                        dict.Add(key, value);
                    }
                    for (int i = 0; i < bookingModel.bookingMenuModel.Count; i++)
                    {
                        if (dict.ContainsKey(bookingModel.bookingMenuModel[i].menuOptionId))
                            bookingModel.bookingMenuModel[i].numberOfTickets = dict[bookingModel.bookingMenuModel[i].menuOptionId];
                    }
                }
                updateTicketsResult = _ticketingService.UpdateBasketTickets(bookingModel, numberTicketsRequested, this.HttpContext, currentBlockedTicketsForUser);

                // If the number available is > Max then set the number to Max - this is used for the drop down
                if (updateTicketsResult.NumberTicketsAvailable > maxTickets)
                    updateTicketsResult.NumberTicketsAvailable = maxTickets;
                else if (updateTicketsResult.NumberTicketsAvailable == 0)
                    updateTicketsResult.NumberTicketsAvailable = updateTicketsResult.NumberTicketsAllocated;
            }
            else
            {
                // Basket not alive (expired) will cause javascript redirect to session expired page
                updateTicketsResult.Success = false;
                updateTicketsResult.NumberTicketsAllocated = 0;
            }

            return Json(updateTicketsResult, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public ActionResult ChooseTickets(ChooseTicketsModel model)
        {
            LogMessage("Tickets Chosen");
            if (_ticketingService.TicketBasketAlive(this.HttpContext))
            {
                if (!string.IsNullOrEmpty(model.BookingModel.bookingRequirements))
                {
                    // Store any special requirements against the booking
                    TicketBasket tb = _ticketingService.GetTicketBasket(this.HttpContext);
                    tb.BookingRequirements = model.BookingModel.bookingRequirements;
                    _ticketingService.UpdateTicketBasket(tb, TicketBasketStatus.InProgress);
                }
                return RedirectToAction("ReviewPurchase");
            }
            else
                return RedirectToAction("SessionExpired");
        }



        #endregion

        #region Book Tickets
        [Authorize]
        public ActionResult BookTickets()
        {
            LogMessage("Booking Tickets");
            if (_ticketingService.TicketBasketAlive(this.HttpContext))
            {
                BookTicketsModel model = new BookTicketsModel();
                model.BookingModel = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                if (model.BookingModel != null)
                {
                    model.Event = _supperClubRepository.GetEvent(model.BookingModel.eventId);
                    model.BookingModel.commission = model.Event.Commission;
                    if (model.Event.Start <= DateTime.Now)
                    {
                        // can't book as the event elapsed
                        SetNotification(NotificationType.Error, "Sorry this event has already passed!", true);
                        _ticketingService.ClearBasket(this.HttpContext);
                        return RedirectToAction("DetailsByIdWithName", "Event", new { eventId = model.Event.Id, eventSeoFriendlyName = model.Event.UrlFriendlyName, hostname = model.Event.SupperClub.UrlFriendlyName });
                    }
                }
                //Get user's recent bookings
                try
                {
                    IList<EventAttendee> recentBookingDetails = _supperClubRepository.GetRecentBookingDetailsForUser(UserMethods.CurrentUser.Id);
                    if (recentBookingDetails != null && recentBookingDetails.Count > 0)
                    {
                       ViewBag.RecentBookings = recentBookingDetails;
                    }
                }
                catch(Exception ex)
                {
                    LogMessage("Exception occured while getting recent bookings for the user. Details:" + ex.Message + ex.StackTrace,LogLevel.ERROR);
                }
                int minTicket = 0;
                if (model.BookingModel.MinMaxBookingEnabled)
                {
                    minTicket = model.BookingModel.MinTicketsAllowed;
                }

                // Set up the number of total tickets available
                int numberAvailableTickets = _ticketingService.GetCurrentAvailableTicketsForEvent(model.BookingModel.eventId, model.BookingModel.seatingId) + model.BookingModel.numberOfTickets;
                int maxTickets = int.Parse(System.Configuration.ConfigurationManager.AppSettings["MaxTicketsAllowed"]);
                if (model.BookingModel.MinMaxBookingEnabled)
                    maxTickets = model.BookingModel.MaxTicketsAllowed;

                if (numberAvailableTickets > maxTickets) // Set a maximum in the drop down
                    numberAvailableTickets = maxTickets;
                else if (numberAvailableTickets == 0) // No more tickets, only show what the current user has allocated
                    numberAvailableTickets = model.BookingModel.numberOfTickets;

                model.NumberOfTicketsAvailable = numberAvailableTickets;

                //GetAvailableTickets(numberAvailableTickets, model.BookingModel.numberOfTickets, ((model.BookingModel.bookingMenuModel != null && model.BookingModel.bookingMenuModel.Count > 0) ? 0 : (model.BookingModel.MinMaxBookingEnabled ? model.BookingModel.MinTicketsAllowed : 1)), model.BookingModel.SeatSelectionInMultipleOfMin ? model.BookingModel.MinTicketsAllowed : 1);

                if (model.Event.MultiSeating)
                {
                    int defaultseatingid = 0;
                    int cntseating = 0;
                    foreach (EventSeating seat in model.Event.EventSeatings)
                    {
                        if (seat.TotalNumberOfAvailableSeats > seat.TotalNumberOfAttendeeGuests)
                        {
                            defaultseatingid = seat.Id;
                            cntseating++;
                        }
                    }
                    if (cntseating == 1)
                        ViewBag.defaultSeatinId = defaultseatingid;
                }

                return View(model);
            }
            else
                return RedirectToAction("SessionExpired");
        }

        [Authorize]
        [HttpPost]
        public ActionResult BookTickets(BookTicketsModel model)
        {
            LogMessage("Tickets Chosen");
            if (_ticketingService.TicketBasketAlive(this.HttpContext))
            {
                model.Event = _supperClubRepository.GetEvent(model.BookingModel.eventId);
                if (model.Event.Start <= DateTime.Now)
                {
                    // can't book as the event elapsed
                    SetNotification(NotificationType.Error, "Sorry this event has already passed!", true);
                    _ticketingService.ClearBasket(this.HttpContext);
                    return RedirectToAction("DetailsByIdWithName", "Event", new { eventId = model.Event.Id, eventSeoFriendlyName = model.Event.UrlFriendlyName, hostname = model.Event.SupperClub.UrlFriendlyName });
                }
                if (!string.IsNullOrEmpty(model.BookingModel.bookingRequirements))
                {
                    // Store any special requirements against the booking
                    TicketBasket tb = _ticketingService.GetTicketBasket(this.HttpContext);
                    tb.BookingRequirements = model.BookingModel.bookingRequirements;
                    _ticketingService.UpdateTicketBasket(tb, TicketBasketStatus.InProgress);
                }
                //if (SupperClub.Code.UserMethods.CurrentUser.BrainTreeTransactionCount > 0 && SupperClub.Code.UserMethods.CurrentUser.BrainTreeTransactionCount % 3 == 2)
                //    return RedirectToAction("CreditCardDetailsBraintree3D");
                //else
                //    return RedirectToAction("CreditCardDetailsBraintree");
                return RedirectToAction("CreditCardDetailsBraintree");
            }
            else
                return RedirectToAction("SessionExpired");
        }

        [Authorize]
        public JsonResult UpdateTickets(int numberTicketsRequested, int seatingId, string strBmm)
        {

            UpdateTicketsResult updateTicketsResult = new UpdateTicketsResult();
            if (_ticketingService.TicketBasketAlive(this.HttpContext))
            {
                LogMessage("Basket alive: Updating Tickets");
                // Service should return object as below
                BookingModel bookingModel = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                bookingModel.seatingId = seatingId;

                int currentBlockedTicketsForUser = _supperClubRepository.GetNumberTicketsInProgressForUser(bookingModel.eventId, seatingId, SupperClub.Code.UserMethods.CurrentUser.Id);
                int maxTickets = int.Parse(System.Configuration.ConfigurationManager.AppSettings["MaxTicketsAllowed"]);
                if (bookingModel.MinMaxBookingEnabled)
                    maxTickets = bookingModel.MaxTicketsAllowed;
                // In case of manual request
                if (numberTicketsRequested > maxTickets)
                    numberTicketsRequested = maxTickets;

                LogMessage(string.Format("Updating Tickets - EventID: {0}, User Id: {3}, Seating Id: {1}, Number of tickets: {2}", bookingModel.eventId, bookingModel.seatingId, bookingModel.numberOfTickets, UserMethods.CurrentUser.Id));

                if (bookingModel.bookingMenuModel != null && bookingModel.bookingMenuModel.Count > 0)
                {
                    //Separators
                    string[] stringSeparators = new string[] { "," };
                    string[] fieldSeparators = new string[] { "|" };
                    Dictionary<int, int> dict = new Dictionary<int, int>();
                    string[] tags = strBmm.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < tags.Length; i++)
                    {
                        string[] fields = tags[i].Split(fieldSeparators, StringSplitOptions.None);
                        int key = Int32.Parse(fields[0]);
                        int value = Int32.Parse(fields[1]);
                        dict.Add(key, value);
                    }
                    for (int i = 0; i < bookingModel.bookingMenuModel.Count; i++)
                    {
                        if (dict.ContainsKey(bookingModel.bookingMenuModel[i].menuOptionId))
                            bookingModel.bookingMenuModel[i].numberOfTickets = dict[bookingModel.bookingMenuModel[i].menuOptionId];
                    }
                }
                updateTicketsResult = _ticketingService.UpdateBasketTickets(bookingModel, numberTicketsRequested, this.HttpContext, currentBlockedTicketsForUser);

                // If the number available is > Max then set the number to Max - this is used for the drop down
                if (updateTicketsResult.NumberTicketsAvailable > maxTickets)
                    updateTicketsResult.NumberTicketsAvailable = maxTickets;
                else if (updateTicketsResult.NumberTicketsAvailable == 0)
                    updateTicketsResult.NumberTicketsAvailable = updateTicketsResult.NumberTicketsAllocated;
            }
            else
            {
                // Basket not alive (expired) will cause javascript redirect to session expired page
                updateTicketsResult.Success = false;
                updateTicketsResult.NumberTicketsAllocated = 0;
            }

            return Json(updateTicketsResult, JsonRequestBehavior.AllowGet);
        }
        #endregion

        //  #region Review of Purchase
        //     #region ReviewPurchase
        [Authorize]
        public ActionResult ReviewPurchase()
        {
            LogMessage("Review Tickets");
            if (_ticketingService.TicketBasketAlive(this.HttpContext))
            {
                BookingModel model = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                ViewBag.AllowSagePay = WebConfigurationManager.AppSettings["AllowSagePay"];
                ViewBag.AllowPayPal = WebConfigurationManager.AppSettings["AllowPayPal"];

                return View(model);
            }
            else
                return RedirectToAction("SessionExpired");
        }



        #region Review of Purchase
        #region ReviewPurchase
        [Authorize]
        public ActionResult ReviewPurchaseCommon()
        {
            LogMessage("Review Tickets");
            if (_ticketingService.TicketBasketAlive(this.HttpContext))
            {
                BookingModel model = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                ViewBag.AllowSagePay = WebConfigurationManager.AppSettings["AllowSagePay"];
                ViewBag.AllowPayPal = WebConfigurationManager.AppSettings["AllowPayPal"];
                ViewBag.AllowBrainTree = WebConfigurationManager.AppSettings["AllowBrainTree"];
                return View(model);
            }
            else
                return RedirectToAction("SessionExpired");
        }

        [Authorize]
        [HttpPost]
        public ActionResult ReviewPurchaseCommon(BookingModel model, bool confirmed = true)
        {
            //if (model.updateContactNumber && !string.IsNullOrEmpty(model.contactNumber))
            //{
            //    model.contactNumber = model.contactNumber.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("+", "").Replace("-", "").Replace(".", "");
            //    model.contactNumber = model.contactNumber.Substring(model.contactNumber.Length - 10);
            //    double number = 0;
            //    if (double.TryParse(model.contactNumber, out number))
            //    {
            //        SaveUserContactNumber(model.contactNumber);
            //    }
            //    else
            //    {
            //        SetNotification(NotificationType.Error, "Your contact number could not be validated. Please enter the correct number.", true);
            //        LogMessage("Contact number entered was not in a correct format.", LogLevel.ERROR);
            //        return RedirectToAction("ReviewPurchaseCommon");
            //    }
            //}
            LogMessage("Tickets Reviewed");
            if (_ticketingService.TicketBasketAlive(this.HttpContext))
                return RedirectToAction("CreditCardDetailsBraintree");
            else
                return RedirectToAction("SessionExpired");
        }

        [Authorize]
        [HttpPost]
        public ActionResult ReviewPurchase(BookingModel model, bool confirmed = true)
        {

            //if (model.updateContactNumber && !string.IsNullOrEmpty(model.contactNumber))
            //{
            //    model.contactNumber = model.contactNumber.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("+", "").Replace("-", "").Replace(".", "");
            //    model.contactNumber = model.contactNumber.Substring(model.contactNumber.Length - 10);
            //    double number = 0;
            //    if (double.TryParse(model.contactNumber, out number))
            //    {
            //        SaveUserContactNumber(model.contactNumber);
            //    }
            //    else
            //    {
            //        SetNotification(NotificationType.Error, "Your contact number could not be validated. Please enter the correct number.", true);
            //        LogMessage("Contact number entered was not in a correct format.", LogLevel.ERROR);
            //        return RedirectToAction("ReviewPurchase");
            //    }
            //}
            LogMessage("Tickets Reviewed");
            if (_ticketingService.TicketBasketAlive(this.HttpContext))
                return RedirectToAction("CreditCardDetails");
            else
                return RedirectToAction("SessionExpired");
        }
        #endregion

        #region Voucher Code related methods
        [Authorize]
        public JsonResult CheckVoucherCode(string voucherCode, int eventId, int totalBookings, decimal basketValue)
        {
            LogMessage("Checking voucher code validity");
            int status = 0;
            if (_ticketingService.TicketBasketAlive(this.HttpContext))
            {
                BookingModel bookingModel = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                status = _supperClubRepository.CheckVoucherCode(voucherCode, eventId, SupperClub.Code.UserMethods.CurrentUser.Id, bookingModel.numberOfTickets, bookingModel.totalDue);
            }
            return Json(status, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public ActionResult ApplyVoucherCode(int voucherId)
        {
            LogMessage("Applying Voucher. VoucherId:" + voucherId.ToString());
            ApplyVoucherCodeResult avcr = new ApplyVoucherCodeResult();
            if (_ticketingService.TicketBasketAlive(this.HttpContext))
            {
                // Service should return object as below
                BookingModel bookingModel = _ticketingService.GetBookingModelFromBasket(this.HttpContext);

                avcr = _ticketingService.ApplyVoucherCode(bookingModel, voucherId, this.HttpContext);
            }
            else
            {
                // Basket not alive (expired) will cause javascript redirect to session expired page
                avcr.Status = -1;
            }

            return Json(avcr, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult GetVoucherDetails(string voucherCode)
        {
            LogMessage("Getting Voucher Details. Voucher Code:" + voucherCode);
            Voucher voucher = new Voucher();
            voucher = _supperClubRepository.GetVoucher(voucherCode);
            return Json(new
            {
                Id = voucher.Id,
                Description = voucher.Description
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Companion E-mail Form and Contact Number Info
        [Authorize]
        public JsonResult SaveGuestEmail(string companionInfo, int eventId, int numOfTickets, int seatingId)
        {
            bool success = false;
            Guid currentUser = SupperClub.Code.UserMethods.CurrentUser.Id;

            try
            {
                if (companionInfo.Length > 0 && SupperClub.Code.UserMethods.CurrentUser.Id == currentUser)
                {
                    List<Subscriber> lstSubscriber = new List<Subscriber>();
                    //string Separators
                    string[] stringSeparators = new string[] { "," };
                    //field Separators
                    string[] fieldSeparators = new string[] { "|" };

                    if (!string.IsNullOrEmpty(companionInfo))
                    {
                        string[] tags = companionInfo.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < tags.Length; i++)
                        {
                            string[] fields = tags[i].Split(fieldSeparators, StringSplitOptions.None);
                            Subscriber _subscriber = new Subscriber();
                            _subscriber.FirstName = fields[0];
                            _subscriber.LastName = fields[1];
                            _subscriber.EmailAddress = fields[2];
                            _subscriber.SubscriberType = (int)SupperClub.Domain.SubscriberType.GuestInvitee;

                            bool validEmail = false;
                            // Validate the email first
                            try
                            {
                                var addr = new System.Net.Mail.MailAddress(_subscriber.EmailAddress);
                                validEmail = true;
                            }
                            catch
                            {
                                validEmail = false;
                            }
                            if (validEmail)
                                lstSubscriber.Add(_subscriber);
                        }
                    }
                    success = _supperClubRepository.AddGuestEmailInfo(currentUser, lstSubscriber);
                    if (!success)
                    {
                        LogMessage("SaveGuestEmail: Could not save Guest Details to Database. Companion Info:" + companionInfo, LogLevel.ERROR);
                    }
                    else
                    {
                        LogMessage("SaveGuestEmail: Saved Guest Details to Database. Companion Info:" + companionInfo, LogLevel.INFO);
                    }
                    //Send confirmation email
                    foreach (Subscriber s in lstSubscriber)
                    {
                        EmailService.EmailService emailer = new EmailService.EmailService(_supperClubRepository);
                        success = emailer.SendEmailToGuestInvitee(s.EmailAddress, eventId, currentUser, numOfTickets, seatingId);
                        BookingConfirmationToFriends confirmationdetails = new BookingConfirmationToFriends();
                        confirmationdetails.EventId = eventId;
                        confirmationdetails.FriendsMailIds = s.EmailAddress;
                        confirmationdetails.Message = string.Empty;
                        confirmationdetails.UserId = UserMethods.CurrentUser.Id;
                        confirmationdetails.CreatedDate = DateTime.Now;
                        _supperClubRepository.CreateBookingConfiramtionToFriends(confirmationdetails);
                    }
                    if (!success)
                    {
                        LogMessage("SaveGuestEmail: Could not send booking e-mail to guest's invitee.", LogLevel.ERROR);
                    }
                    else
                    {
                        LogMessage("SaveGuestEmail: E-mail sent successfully to guest's invitee.", LogLevel.INFO);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage("Error adding Guest Invitee or sending e-mail. UserId: " + currentUser.ToString() + "  Error Details:" + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
            }
            return Json(success, JsonRequestBehavior.AllowGet);
        }
        [Authorize]
        public JsonResult SaveUserContactNumber(string phoneNumber)
        {
            LogMessage("Saving user's contact number");
            bool success = false;
            Guid currentUser = SupperClub.Code.UserMethods.CurrentUser.Id;
            if (!string.IsNullOrEmpty(phoneNumber) && phoneNumber.Length > 0)
            {
                phoneNumber = phoneNumber.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("+", "").Replace("-", "").Replace(".", "");
                phoneNumber = phoneNumber.Substring(phoneNumber.Length - 10);
                double number = 0;
                bool saveNumber = false;
                if (double.TryParse(phoneNumber, out number))
                    saveNumber = true;
                if (saveNumber)
                {
                    User _user = _supperClubRepository.GetUser(currentUser);
                    if (_user != null)
                    {
                        _user.ContactNumber = phoneNumber;
                        success = _supperClubRepository.UpdateUser(_user);
                        if (!success)
                        {
                            LogMessage("SaveUserContactNumber: Could not save contact number in Database. UserId: " + currentUser.ToString() + " Contact Number: " + phoneNumber, LogLevel.ERROR);
                        }
                        else
                        {
                            LogMessage("User's contact number saved successfully.");
                            System.Web.HttpContext.Current.Session["User"] = _user;
                        }
                    }
                }
                else
                {
                    LogMessage("Contact number entered was not in a correct format. UserId: " + currentUser.ToString() + " Contact Number: " + phoneNumber, LogLevel.ERROR);
                }
            }
            return Json(success, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public JsonResult UpdateBookingRequirements(string message)
        {
            LogMessage("Updating user's message for chef");
            bool success = false;
            if (!string.IsNullOrEmpty(message))
            {
                if (_ticketingService.TicketBasketAlive(this.HttpContext))
                {
                    TicketBasket tb = _ticketingService.GetTicketBasket(this.HttpContext);
                    tb.BookingRequirements = message;
                    TicketBasket tbNew = _ticketingService.UpdateTicketBasket(tb, TicketBasketStatus.InProgress);
                    if (tbNew.BookingRequirements == message)
                    {
                        success = true;
                        LogMessage("Updated user's message for chef");
                    }
                }
            }
            return Json(success, JsonRequestBehavior.AllowGet);
        }
        #endregion
        #endregion

        #region Free Tickets

        [Authorize]
        public ActionResult FreeBooking()
        {
            LogMessage("Free Booking");
            if (_ticketingService.TicketBasketAlive(this.HttpContext))
            {
                TicketBasket ticketBasket = _ticketingService.GetTicketBasket(this.HttpContext);
                TransactionResult tr = new TransactionResult { Success = false, TransactionMessage = "", EventId = 0 };
                BookingModel bm = _ticketingService.GetBookingModelFromBasket(this.HttpContext);

                // Check if the voucher is valid
                if (bm.voucherId > 0)
                {
                    int status = 0;
                    Voucher _voucher = _supperClubRepository.GetVoucher(bm.voucherId);
                    status = _supperClubRepository.CheckVoucherCode(_voucher.Code, bm.eventId, SupperClub.Code.UserMethods.CurrentUser.Id, bm.numberOfTickets, bm.totalDue);
                    if(status <= 0 || status != bm.voucherId)
                    { 
                        tr.Success = false;
                        tr.EventId = bm.eventId;
                        string message = "Invalid Voucher Code.";
                        switch (status)
                        {
                            case -1:
                                message = "The Voucher Code you have used has expired.";
                                break;
                            case -2:
                                message = "The Voucher Code is not valid for current booking.";
                                break;
                            case -3:
                                message = "You have already used this voucher for previous bookings.";
                                break;
                            case -4:
                                message = "The Voucher Code has expired.";
                                break;
                            case -5:
                                message = "The voucher code can not be applied to your basket. Minimum basket value is more than current basket value.";
                                break;
                            case -6:
                                message = "The gift voucher does not have any balance.";
                                break;
                            default:
                                message = "Error applying voucher code. Please try again.";
                                break;
                        }
                        tr.TransactionMessage = message;
                        TempData["TransactionResult"] = tr;
                        return RedirectToAction("PostPaymentFailure");
                    }
                }
                // Add check to see if a ticket already booked with this basket id
                bool existingBooking = _supperClubRepository.CheckBookingForBasket(ticketBasket.Id);
                if(existingBooking)
                {
                    tr.Success = false;
                    tr.EventId = bm.eventId;                    
                    tr.TransactionMessage = "You have already made a booking with this basket. Please start over to book again!";
                    TempData["TransactionResult"] = tr;
                    return RedirectToAction("PostPaymentFailure");
                }
                if ((ticketBasket.TotalPrice - ticketBasket.TotalDiscount) <= 0)
                {
                    tr.Success = true;
                    tr.EventId = bm.eventId;
                    TempData["TransactionResult"] = tr;
                    _ticketingService.UpdateTicketBasket(ticketBasket, TicketBasketStatus.Complete);
                    bool success = _ticketingService.AddUserToEvent(ticketBasket, bm);
                    if (success && ticketBasket.TotalDiscount > 0)
                        _ticketingService.UpdateVoucherUsage(ticketBasket.Tickets[0].VoucherId, ticketBasket.TotalDiscount);
                    // Email - Guest: with confirmation, Host - that someone booked a ticket
                    bool hostSuccess = false;
                    if (bm.eventId == 777)
                    {
                        //create and get voucher
                        List<string> lstvalue = Session["GiftVoucher"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        string v_code = string.Empty;
                        Voucher voucher;
                        SupperClub.Code.VoucherManager vmgr = new VoucherManager();
                        voucher = vmgr.CreateVoucher("Gift Voucher", ticketBasket.TotalPrice, 2);
                        //send email
                        if (!string.IsNullOrEmpty(voucher.Code))
                        {
                            EmailService.EmailService emailer = new EmailService.EmailService(_supperClubRepository);

                            //IEnumerable<System.Web.Security.MembershipUser> members = System.Web.Security.Membership.FindUsersByEmail(ticketBasket.Name).Cast<System.Web.Security.MembershipUser>();
                            //string username = members.First().UserName;

                            // User user = _supperClubRepository.GetUser(ticketBasket.Name);
                            //    Guid userId = user.Id;

                            bool _success = emailer.SendGiftVoucherEmail(lstvalue[0], lstvalue[1], DateTime.Today.AddYears(1), voucher.Code, ticketBasket.TotalPrice);


                            UserVoucherTypeDetail detail = new UserVoucherTypeDetail();

                            detail.UserId = UserMethods.CurrentUser.Id; ;
                            detail.VType = (int)(VType.GiftVoucher);
                            detail.VoucherType = (int)(VoucherType.ValueOff);
                            detail.Name = lstvalue[0];
                            detail.FriendEmailId = lstvalue[1];
                            detail.CreatedDate = DateTime.Now;
                            detail.Value = ticketBasket.TotalPrice;
                            detail.VoucherId = voucher.Id;
                            detail.BasketId = ticketBasket.Id;

                            UserVoucherTypeDetail _detail = _supperClubRepository.CreateUserVoucherTypeDetail(detail);

                            // return View("PostPaymentSuccessGift.cshtml");
                        }
                    }
                    else
                    {
                        EmailService.EmailService es = new EmailService.EmailService(_supperClubRepository);
                        bool _emailSuccess = es.SendGuestBookedEmails(
                            UserMethods.CurrentUser.FirstName,
                            UserMethods.CurrentASPNETUser.Email,
                            ticketBasket.TotalTickets,
                            ticketBasket.TotalPrice - ticketBasket.TotalDiscount,
                            ticketBasket.BookingReference,
                            ticketBasket.BookingRequirements,
                            bm.eventId,
                            bm.seatingId,
                            bm.bookingMenuModel,
                            bm.voucherId,
                            ticketBasket.CCLastDigits,
                            bm.commission,
                            ref hostSuccess
                            );
                        if (_emailSuccess)
                            LogMessage("Guest Booking Confirmation Email: Sent successfully", LogLevel.INFO);
                        else
                            LogMessage("Guest Booking Confirmation Email: Failure sending e-mail", LogLevel.ERROR);

                        if (hostSuccess)
                            LogMessage("Host Booking Information Email: Sent successfully", SupperClub.Logger.LogLevel.INFO);
                        else
                            LogMessage("Host Booking Information Email: Failure sending e-mail", LogLevel.ERROR);
                    }
                    return RedirectToAction("PostPaymentSuccessNew");
                }
                else
                {
                    tr.Success = false;
                    tr.EventId = bm.eventId;
                    tr.TransactionMessage = "It doesn't appear that your event is actually free.";
                    TempData["TransactionResult"] = tr;
                    return RedirectToAction("PostPaymentFailure");
                }
            }
            else
                return RedirectToAction("SessionExpired");
        }

        #endregion

        #region SagePay

        #region CreditCard Details

        [Authorize]
        public ActionResult CreditCardDetailsCommon()
        {
            LogMessage("Credit Card Details");
            if (_ticketingService.TicketBasketAlive(this.HttpContext))
            {
                BookingModel bookingModel = _ticketingService.GetBookingModelFromBasket(this.HttpContext);


                CreditCardDetailsModel ccdmodel = (_testCardType == CreditCardType.NONE) ? new CreditCardDetailsModel() : GetTestCreditCardModel(_testCardType);
                CreditCardDetailsViewModelOld model = new CreditCardDetailsViewModelOld { bookingModel = bookingModel, creditCardDetails = ccdmodel };
                model.CreditCardTypes = GetCardTypes("");
                return View("SagePay/CreditCardDetailsOld", model);
            }
            else
                return RedirectToAction("SessionExpired");
        }
        [Authorize]
        [HttpPost]
        public ActionResult CreditCardDetailsCommon(CreditCardDetailsViewModelOld model)
        {
            LogMessage("Credit Card Details Submitted");
            if (_ticketingService.TicketBasketAlive(this.HttpContext))
            {
                // Validate Credit Card (check if card is Maestro they should provide StartMonth, StartYear, IssueNumber)
                if (model.creditCardDetails.CardType == "MAESTRO")
                {
                    if (string.IsNullOrEmpty(model.creditCardDetails.StartMonth.ToString()))
                        ModelState.AddModelError("creditCardDetails_StartMonth", "You must provide a Start Month for a Maestro card");

                    if (string.IsNullOrEmpty(model.creditCardDetails.StartYear.ToString()))
                        ModelState.AddModelError("creditCardDetails_StartYear", "You must provide a Start Year for a Maestro card");
                    else if (model.creditCardDetails.StartYear + 2000 < DateTime.Now.AddYears(-5).Year)
                        ModelState.AddModelError("creditCardDetails_StartYear", "You can't use a card older than 5 years");

                    if (string.IsNullOrEmpty(model.creditCardDetails.IssueNumber.ToString()))
                        ModelState.AddModelError("creditCardDetails_IssueNumber", "You must provide an Issue Number for a Maestro card");
                }

                if (model.creditCardDetails.ExpiryYear + 2000 < DateTime.Now.Year)
                    ModelState.AddModelError("creditCardDetails_ExpiryYear", "This card has expired");

                if (ModelState.IsValid)
                {
                    // Update basket with some CC details
                    TicketBasket ticketBasket = _ticketingService.GetTicketBasket(this.HttpContext);
                    ticketBasket.CCLastDigits = model.creditCardDetails.CreditCardNumber.Substring(model.creditCardDetails.CreditCardNumber.Length - 4, 4);
                    ticketBasket.CCExpiryDate = ((int)model.creditCardDetails.ExpiryMonth).ToString("00") + @"/" + ((int)model.creditCardDetails.ExpiryYear).ToString("00");
                    _ticketingService.UpdateTicketBasket(ticketBasket, TicketBasketStatus.InProgress);

                    //Generate a random code for VpsTxCode
                    //string _vpsTxCode = (Guid.NewGuid()).ToString();

                    //TransactionRegistrationResponse transactionResponse = _transactionService.SendSagePayTransaction(ticketBasket, ConvertCreditCardModel(model.creditCardDetails), ConvertAddressModel(model.creditCardDetails), ControllerContext.RequestContext, _vpsTxCode);
                    TransactionRegistrationResponse transactionResponse = _transactionService.SendSagePayTransaction(ticketBasket, ConvertCreditCardModel(model.creditCardDetails), ConvertAddressModel(model.creditCardDetails), ControllerContext.RequestContext);
                    LogMessage("CreditCardDetails Method-> VPSProtocol:" + transactionResponse.VPSProtocol + ",Status:" + transactionResponse.Status.ToString() + ",StatusDetail:" + transactionResponse.StatusDetail + ",VPSTxId:" + transactionResponse.VPSTxId +
                        ",SecurityKey: " + transactionResponse.SecurityKey + ",TxAuthNo:" + transactionResponse.TxAuthNo + ",AVSCV2:" + transactionResponse.AVSCV2 +
                        ",AddressResult: " + transactionResponse.AddressResult + ",PostCodeResult:" + transactionResponse.PostCodeResult + ",CV2Result: " + transactionResponse.CV2Result +
                        ",ThreeDSecureStatus:" + transactionResponse.ThreeDSecureStatus.ToString() + ",CAVV" + transactionResponse.CAVV +
                        ",NextUrl:" + transactionResponse.NextURL + ",MD: " + transactionResponse.MD + ",PAReq: " + transactionResponse.PAReq + ",ACSURL: " + transactionResponse.ACSURL);
                    TransactionResult tr = new TransactionResult { Success = false, TransactionMessage = "", EventId = (ticketBasket != null && ticketBasket.Tickets.Count > 0) ? ticketBasket.Tickets[0].EventId : 0 };

                    if (transactionResponse.Status == SagePayMvc.ResponseType.Ok && transactionResponse.StatusDetail.Contains("0000"))
                    {
                        tr.Success = true;
                        BookingModel bm = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                        tr.EventId = bm.eventId;
                        TempData["TransactionResult"] = tr;
                        _ticketingService.UpdateTicketBasket(ticketBasket, TicketBasketStatus.Complete);
                        bool success = _ticketingService.AddUserToEvent(ticketBasket, bm);
                        if (success && ticketBasket.TotalDiscount > 0)
                            _ticketingService.UpdateVoucherUsage(ticketBasket.Tickets[0].VoucherId, ticketBasket.TotalDiscount);
                        // Email - Guest: with confirmation, Host - that someone booked a ticket
                        bool hostSuccess = false;
                        if (bm.eventId == 777)
                        {
                            //create and get voucher
                            List<string> lstvalue = Session["GiftVoucher"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            string v_code = string.Empty;
                            Voucher voucher;
                            SupperClub.Code.VoucherManager vmgr = new VoucherManager();
                            voucher = vmgr.CreateVoucher("Gift Voucher", ticketBasket.TotalPrice, 2);
                            //send email
                            if (!string.IsNullOrEmpty(voucher.Code))
                            {
                                EmailService.EmailService emailer = new EmailService.EmailService(_supperClubRepository);

                                //IEnumerable<System.Web.Security.MembershipUser> members = System.Web.Security.Membership.FindUsersByEmail(ticketBasket.Name).Cast<System.Web.Security.MembershipUser>();
                                //string username = members.First().UserName;

                                // User user = _supperClubRepository.GetUser(ticketBasket.Name);
                                //    Guid userId = user.Id;

                                bool _success = emailer.SendGiftVoucherEmail(lstvalue[0], lstvalue[1], DateTime.Today.AddYears(1), voucher.Code, ticketBasket.TotalPrice);


                                UserVoucherTypeDetail detail = new UserVoucherTypeDetail();

                                detail.UserId = UserMethods.CurrentUser.Id; ;
                                detail.VType = (int)(VType.GiftVoucher);
                                detail.VoucherType = (int)(VoucherType.ValueOff);
                                detail.Name = lstvalue[0];
                                detail.FriendEmailId = lstvalue[1];
                                detail.CreatedDate = DateTime.Now;
                                detail.Value = ticketBasket.TotalPrice;
                                detail.VoucherId = voucher.Id;
                                detail.BasketId = ticketBasket.Id;

                                UserVoucherTypeDetail _detail = _supperClubRepository.CreateUserVoucherTypeDetail(detail);

                                // return View("PostPaymentSuccessGift.cshtml");
                            }
                        }
                        else
                        {
                            EmailService.EmailService es = new EmailService.EmailService(_supperClubRepository);
                            bool _emailSuccess = es.SendGuestBookedEmails(
                                UserMethods.CurrentUser.FirstName,
                                UserMethods.CurrentASPNETUser.Email,
                                ticketBasket.TotalTickets,
                                ticketBasket.TotalPrice - ticketBasket.TotalDiscount,
                                ticketBasket.BookingReference,
                                ticketBasket.BookingRequirements,
                                bm.eventId,
                                bm.seatingId,
                                bm.bookingMenuModel,
                                bm.voucherId,
                                ticketBasket.CCLastDigits,
                                bm.commission,
                                ref hostSuccess
                                );
                            if (_emailSuccess)
                                LogMessage("Guest Booking Confirmation Email: Sent successfully", LogLevel.INFO);
                            else
                                LogMessage("Guest Booking Confirmation Email: Failure sending e-mail", LogLevel.ERROR);

                            if (hostSuccess)
                                LogMessage("Host Booking Information Email: Sent successfully", SupperClub.Logger.LogLevel.INFO);
                            else
                                LogMessage("Host Booking Information Email: Failure sending e-mail", LogLevel.ERROR);
                        }
                        return RedirectToAction("PostPaymentSuccess");
                    }
                    else if (transactionResponse.Status == SagePayMvc.ResponseType.ThreeDAuth)
                    {
                        // Redirect to 3DSecure if required
                        ThreeDSecureModel threeDModel = new ThreeDSecureModel();
                        threeDModel.ACSURL = transactionResponse.ACSURL;
                        threeDModel.MD = transactionResponse.MD;
                        threeDModel.PAReq = transactionResponse.PAReq;
                        TempData["ThreeDSecureModel"] = threeDModel;
                        return RedirectToAction("ThreeDSecure");
                    }
                    else
                    {
                        // Something went wrong
                        BookingModel bm = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                        tr.EventId = bm.eventId;
                        log.Error("Error from SagePay. Error: " + transactionResponse.StatusDetail);
                        tr.TransactionMessage = transactionResponse.StatusDetail;
                        TempData["TransactionResult"] = tr;
                        return RedirectToAction("PostPaymentFailure");
                    }
                }
                else
                {
                    foreach (var modelStateValue in ViewData.ModelState.Values)
                    {
                        foreach (var error in modelStateValue.Errors)
                        {
                            // Log error
                            LogMessage("Validation Failed. ErrorMessage:" + error.ErrorMessage + ". Exception:" + error.Exception, LogLevel.DEBUG);
                        }
                    }
                    model.CreditCardTypes = GetCardTypes("");
                    return View("SagePay/CreditCardDetailsOld", model);
                }
            }
            else
                return RedirectToAction("SessionExpired");
        }
        [Authorize]
        public ActionResult CreditCardDetails()
        {
            LogMessage("Credit Card Details");
            if (_ticketingService.TicketBasketAlive(this.HttpContext))
            {
                BookingModel bookingModel = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                if (bookingModel.bookingMenuModel != null && bookingModel.bookingMenuModel.Count > 0)
                {
                    decimal totalCost = 0;
                    decimal totalDiscount = 0;
                    foreach (BookingMenuModel bmm in bookingModel.bookingMenuModel)
                    {
                        totalCost += SupperClub.Domain.CostCalculator.CostToGuest(bmm.baseTicketCost, bookingModel.commission) * bmm.numberOfTickets;
                        totalDiscount += bmm.discount;
                    }
                    if (totalCost - totalDiscount <= 0)
                        return RedirectToAction("FreeBooking");
                }
                else if (bookingModel.totalDue - bookingModel.discount <= 0)
                    return RedirectToAction("FreeBooking");

                Event _event = new Event();
                if (bookingModel.eventId > 0)
                    _event = _supperClubRepository.GetEvent(bookingModel.eventId);
                string contactNumber = SupperClub.Code.UserMethods.CurrentUser.ContactNumber == null ? "" : SupperClub.Code.UserMethods.CurrentUser.ContactNumber;
                CreditCardDetailsModel ccdmodel = (_testCardType == CreditCardType.NONE) ? new CreditCardDetailsModel() : GetTestCreditCardModel(_testCardType);
                CreditCardDetailsViewModel model = new CreditCardDetailsViewModel { bookingModel = bookingModel, creditCardDetails = ccdmodel, Event = _event, ContactNumber = contactNumber };
                model.CreditCardTypes = GetCardTypes("");
                return View("SagePay/CreditCardDetails", model);
            }
            else
                return RedirectToAction("SessionExpired");
        }
        [Authorize]
        [HttpPost]
        public ActionResult CreditCardDetails(CreditCardDetailsViewModel model)
        {
            LogMessage("Credit Card Details Submitted");
            
            if (_ticketingService.TicketBasketAlive(this.HttpContext))
            {
                // Validate Credit Card (check if card is Maestro they should provide StartMonth, StartYear, IssueNumber)
                if (model.creditCardDetails.CardType == "MAESTRO")
                {
                    if (string.IsNullOrEmpty(model.creditCardDetails.StartMonth.ToString()))
                        ModelState.AddModelError("creditCardDetails_StartMonth", "You must provide a Start Month for a Maestro card");

                    if (string.IsNullOrEmpty(model.creditCardDetails.StartYear.ToString()))
                        ModelState.AddModelError("creditCardDetails_StartYear", "You must provide a Start Year for a Maestro card");
                    else if (model.creditCardDetails.StartYear + 2000 < DateTime.Now.AddYears(-5).Year)
                        ModelState.AddModelError("creditCardDetails_StartYear", "You can't use a card older than 5 years");

                    if (string.IsNullOrEmpty(model.creditCardDetails.IssueNumber.ToString()))
                        ModelState.AddModelError("creditCardDetails_IssueNumber", "You must provide an Issue Number for a Maestro card");
                }

                if (model.creditCardDetails.ExpiryYear + 2000 < DateTime.Now.Year)
                    ModelState.AddModelError("creditCardDetails_ExpiryYear", "This card has expired");

                if (model.bookingModel.IsContactNumberRequired && string.IsNullOrEmpty(model.ContactNumber))
                    ModelState.AddModelError("ContactNumber", "You must provide a phone number to continue booking! Select `Use another credit card` to add it.");

                if ((model.bookingModel.IsContactNumberRequired && !string.IsNullOrEmpty(model.ContactNumber)) || !string.IsNullOrEmpty(model.ContactNumber))
                {
                    //var regex1 = @"^\(?(?:(?:0(?:0|11)\)?[\s-]?\(?|\+)44\)?[\s-]?\(?(?:0\)?[\s-]?\(?)?|0)(?:\d{2}\)?[\s-]?\d{4}[\s-]?\d{4}|\d{3}\)?[\s-]?\d{3}[\s-]?\d{3,4}|\d{4}\)?[\s-]?(?:\d{5}|\d{3}[\s-]?\d{3})|\d{5}\)?[\s-]?\d{4,5}|8(?:00[\s-]?11[\s-]?11|45[\s-]?46[\s-]?4\d))(?:(?:[\s-]?(?:x|ext\.?\s?|\#)\d+)?)$";
                    //var match = Regex.Match(model.ContactNumber, regex1, RegexOptions.IgnoreCase);
                    //if (!match.Success)
                    //{
                    //    ModelState.AddModelError("ContactNumber", "Please enter a 11 digit phone number. The number entered is not in a valid format.");
                    //}
                }
                if (ModelState.IsValid)
                {
                    // Update basket with some CC details
                    TicketBasket ticketBasket = _ticketingService.GetTicketBasket(this.HttpContext);
                    ticketBasket.CCLastDigits = model.creditCardDetails.CreditCardNumber.Substring(model.creditCardDetails.CreditCardNumber.Length - 4, 4);
                    ticketBasket.CCExpiryDate = ((int)model.creditCardDetails.ExpiryMonth).ToString("00") + @"/" + ((int)model.creditCardDetails.ExpiryYear).ToString("00");
                    _ticketingService.UpdateTicketBasket(ticketBasket, TicketBasketStatus.InProgress);

                    // Save user info
                    saveUser(model.ContactNumber, model.creditCardDetails);
                    //Generate a random code for VpsTxCode
                    //string _vpsTxCode = (Guid.NewGuid()).ToString();
                    //TransactionRegistrationResponse transactionResponse = _transactionService.SendSagePayTransaction(ticketBasket, ConvertCreditCardModel(model.creditCardDetails), ConvertAddressModel(model.creditCardDetails), ControllerContext.RequestContext, _vpsTxCode);
                    TransactionRegistrationResponse transactionResponse = _transactionService.SendSagePayTransaction(ticketBasket, ConvertCreditCardModel(model.creditCardDetails), ConvertAddressModel(model.creditCardDetails), ControllerContext.RequestContext);
                    LogMessage("CreditCardDetails Method-> VPSProtocol:" + transactionResponse.VPSProtocol + ",Status:" + transactionResponse.Status.ToString() + ",StatusDetail:" + transactionResponse.StatusDetail + ",VPSTxId:" + transactionResponse.VPSTxId +
                        ",SecurityKey: " + transactionResponse.SecurityKey + ",TxAuthNo:" + transactionResponse.TxAuthNo + ",AVSCV2:" + transactionResponse.AVSCV2 +
                        ",AddressResult: " + transactionResponse.AddressResult + ",PostCodeResult:" + transactionResponse.PostCodeResult + ",CV2Result: " + transactionResponse.CV2Result +
                        ",ThreeDSecureStatus:" + transactionResponse.ThreeDSecureStatus.ToString() + ",CAVV" + transactionResponse.CAVV +
                        ",NextUrl:" + transactionResponse.NextURL + ",MD: " + transactionResponse.MD + ",PAReq: " + transactionResponse.PAReq + ",ACSURL: " + transactionResponse.ACSURL);
                    TransactionResult tr = new TransactionResult { Success = false, TransactionMessage = "", EventId = (ticketBasket != null && ticketBasket.Tickets.Count > 0) ? ticketBasket.Tickets[0].EventId : 0 };

                    if (transactionResponse.Status == SagePayMvc.ResponseType.Ok && transactionResponse.StatusDetail.Contains("0000"))
                    {
                        tr.Success = true;
                        BookingModel bm = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                        tr.EventId = bm.eventId;
                        TempData["TransactionResult"] = tr;
                        _ticketingService.UpdateTicketBasket(ticketBasket, TicketBasketStatus.Complete);
                        bool success = _ticketingService.AddUserToEvent(ticketBasket, bm);
                        if (success && ticketBasket.TotalDiscount > 0)
                            _ticketingService.UpdateVoucherUsage(ticketBasket.Tickets[0].VoucherId, ticketBasket.TotalDiscount);
                        // Email - Guest: with confirmation, Host - that someone booked a ticket
                        bool hostSuccess = false;
                        if (bm.eventId == 777)
                        {
                            //create and get voucher
                            List<string> lstvalue = Session["GiftVoucher"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            string v_code = string.Empty;
                            Voucher voucher;
                            SupperClub.Code.VoucherManager vmgr = new VoucherManager();
                            voucher = vmgr.CreateVoucher("Gift Voucher", ticketBasket.TotalPrice, 2);
                            //send email
                            if (!string.IsNullOrEmpty(voucher.Code))
                            {
                                EmailService.EmailService emailer = new EmailService.EmailService(_supperClubRepository);

                                //IEnumerable<System.Web.Security.MembershipUser> members = System.Web.Security.Membership.FindUsersByEmail(ticketBasket.Name).Cast<System.Web.Security.MembershipUser>();
                                //string username = members.First().UserName;

                                // User user = _supperClubRepository.GetUser(ticketBasket.Name);
                                //    Guid userId = user.Id;

                                bool _success = emailer.SendGiftVoucherEmail(lstvalue[0], lstvalue[1], DateTime.Today.AddYears(1), voucher.Code, ticketBasket.TotalPrice);


                                UserVoucherTypeDetail detail = new UserVoucherTypeDetail();

                                detail.UserId = UserMethods.CurrentUser.Id; ;
                                detail.VType = (int)(VType.GiftVoucher);
                                detail.VoucherType = (int)(VoucherType.ValueOff);
                                detail.Name = lstvalue[0];
                                detail.FriendEmailId = lstvalue[1];
                                detail.CreatedDate = DateTime.Now;
                                detail.Value = ticketBasket.TotalPrice;
                                detail.VoucherId = voucher.Id;
                                detail.BasketId = ticketBasket.Id;

                                UserVoucherTypeDetail _detail = _supperClubRepository.CreateUserVoucherTypeDetail(detail);

                                // return View("PostPaymentSuccessGift.cshtml");
                            }
                        }
                        else
                        {
                            EmailService.EmailService es = new EmailService.EmailService(_supperClubRepository);
                            bool _emailSuccess = es.SendGuestBookedEmails(
                                UserMethods.CurrentUser.FirstName,
                                UserMethods.CurrentASPNETUser.Email,
                                ticketBasket.TotalTickets,
                                ticketBasket.TotalPrice - ticketBasket.TotalDiscount,
                                ticketBasket.BookingReference,
                                ticketBasket.BookingRequirements,
                                bm.eventId,
                                bm.seatingId,
                                bm.bookingMenuModel,
                                bm.voucherId,
                                ticketBasket.CCLastDigits,
                                 bm.commission,
                                ref hostSuccess
                                );
                            if (_emailSuccess)
                                LogMessage("Guest Booking Confirmation Email: Sent successfully", LogLevel.INFO);
                            else
                                LogMessage("Guest Booking Confirmation Email: Failure sending e-mail", LogLevel.ERROR);

                            if (hostSuccess)
                                LogMessage("Host Booking Information Email: Sent successfully", SupperClub.Logger.LogLevel.INFO);
                            else
                                LogMessage("Host Booking Information Email: Failure sending e-mail", LogLevel.ERROR);
                        }
                        return RedirectToAction("PostPaymentSuccess");
                    }
                    else if (transactionResponse.Status == SagePayMvc.ResponseType.ThreeDAuth)
                    {
                        // Redirect to 3DSecure if required
                        ThreeDSecureModel threeDModel = new ThreeDSecureModel();
                        threeDModel.ACSURL = transactionResponse.ACSURL;
                        threeDModel.MD = transactionResponse.MD;
                        threeDModel.PAReq = transactionResponse.PAReq;
                        TempData["ThreeDSecureModel"] = threeDModel;
                        return RedirectToAction("ThreeDSecure");
                    }
                    else
                    {
                        // Something went wrong
                        BookingModel bm = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                        tr.EventId = bm.eventId;
                        log.Error("Error from SagePay. Error: " + transactionResponse.StatusDetail);
                        tr.TransactionMessage = transactionResponse.StatusDetail;
                        TempData["TransactionResult"] = tr;
                        return RedirectToAction("PostPaymentFailure");
                    }
                }
                else
                {
                    if (model.bookingModel.eventId != null)
                        model.Event = _supperClubRepository.GetEvent(model.bookingModel.eventId);
                    if (_ticketingService.TicketBasketAlive(this.HttpContext))
                        model.bookingModel = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                    foreach (var modelStateValue in ViewData.ModelState.Values)
                    {
                        foreach (var error in modelStateValue.Errors)
                        {
                            // Log error
                            LogMessage("Validation Failed. ErrorMessage:" + error.ErrorMessage + ". Exception:" + error.Exception, LogLevel.DEBUG);
                        }
                    }
                    model.CreditCardTypes = GetCardTypes("");
                    return View("SagePay/CreditCardDetails", model);
                }
            }
            else
                return RedirectToAction("SessionExpired");
        }
       
        
        #endregion

        #region 3D Secure processing

        [Authorize]
        public ActionResult ThreeDSecureIframe(string ACSURL, string MD, string PAReq)
        {
            ThreeDSecureModel model = new ThreeDSecureModel();
            model.ACSURL = ACSURL;
            model.MD = MD;
            model.PAReq = PAReq.Replace("%2B", "+");
            model.TermUrl = string.Format(@"{0}Booking/ThreeDSecureReply", ServerMethods.ServerUrl);

            return View("SagePay/ThreeDSecureIframe", model);
        }

        [Authorize]
        public ActionResult ThreeDSecure()
        {
            LogMessage("Three D Secure");
            ThreeDSecureModel model = (ThreeDSecureModel)TempData["ThreeDSecureModel"];
            _ticketingService.TicketBasketAlive(this.HttpContext);
            // Contains iFrame for 3DSecurity check
            // Redirection will happen from the 3DSecure site back to a URL (Booking/ThreeDSecureReply)
            return View("SagePay/ThreeDSecure", model);
        }

        [Authorize]
        public ActionResult ThreeDSecureReply()
        {
            LogMessage("Three D Secure Reply");
            _ticketingService.TicketBasketAlive(this.HttpContext);
            TicketBasket ticketBasket = _ticketingService.GetTicketBasket(this.HttpContext);
            // MD value and PaRes value are included as fields in reply
            string MD = Request.Params["MD"];
            string PaRes = Request.Params["PaRes"];
            LogMessage("ThreeDSecureReply Method -> MD: " + MD + "    PaRes: " + PaRes);
            try
            {
                // Service should Post these to SagePay as MD and PARes, recieve transaction reply
                TransactionRegistrationResponse transactionResponse = _transactionService.SendSagePayThreeDResult(ticketBasket.Id.ToString(), ControllerContext.RequestContext, MD, PaRes);
                LogMessage("ThreeDSecureReply Method-> VPSTxId:" + transactionResponse.VPSTxId + ",Status:" + transactionResponse.Status.ToString() + ",StatusDetail:" + transactionResponse.StatusDetail + ",ThreeDSecureStatus:"
                        + transactionResponse.ThreeDSecureStatus.ToString() + ",NextUrl:" + transactionResponse.NextURL +
                        ",SecurityKey: " + transactionResponse.SecurityKey + ",MD: " + transactionResponse.MD + ",PAReq: " + transactionResponse.PAReq +
                        ",ACSURL: " + transactionResponse.ACSURL + ",AddressResult: " + transactionResponse.AddressResult + ",CV2Result: " + transactionResponse.CV2Result + ",PostCodeResult: " + transactionResponse.PostCodeResult);
                TransactionResult tr = new TransactionResult { Success = false, TransactionMessage = transactionResponse.StatusDetail, EventId = (ticketBasket != null && ticketBasket.Tickets.Count > 0) ? ticketBasket.Tickets[0].EventId : 0 };
                //LogMessage("ThreeDSecureReply Method -> Transaction Result Message: " + tr.TransactionMessage + "Transaction Result Status: " + tr.Success.ToString());
                if (transactionResponse.Status == SagePayMvc.ResponseType.Ok && (transactionResponse.ThreeDSecureStatus == ThreeDSecureStatus.AttemptOnly || transactionResponse.ThreeDSecureStatus == ThreeDSecureStatus.Ok))
                {
                    tr.Success = true;
                    BookingModel bookingModel = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                    tr.EventId = bookingModel.eventId;
                    TempData["TransactionResult"] = tr;
                    _ticketingService.UpdateTicketBasket(ticketBasket, TicketBasketStatus.Complete);

                    bool success = _ticketingService.AddUserToEvent(ticketBasket, bookingModel);
                    if (success && ticketBasket.TotalDiscount > 0)
                        _ticketingService.UpdateVoucherUsage(ticketBasket.Tickets[0].VoucherId, ticketBasket.TotalDiscount);

                    // Email - Guest: with confirmation, Host - that someone booked a ticket
                    bool hostSuccess = false;
                    if (bookingModel.eventId == 777)
                    {
                        //create and get voucher
                        List<string> lstvalue = Session["GiftVoucher"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        string v_code = string.Empty;
                        Voucher voucher;
                        SupperClub.Code.VoucherManager vmgr = new VoucherManager();
                        voucher = vmgr.CreateVoucher("Gift Voucher", ticketBasket.TotalPrice, 2);
                        //send email
                        if (!string.IsNullOrEmpty(voucher.Code))
                        {
                            EmailService.EmailService emailer = new EmailService.EmailService(_supperClubRepository);

                            //IEnumerable<System.Web.Security.MembershipUser> members = System.Web.Security.Membership.FindUsersByEmail(ticketBasket.Name).Cast<System.Web.Security.MembershipUser>();
                            //string username = members.First().UserName;

                            // User user = _supperClubRepository.GetUser(ticketBasket.Name);
                            //    Guid userId = user.Id;

                            bool _success = emailer.SendGiftVoucherEmail(lstvalue[0], lstvalue[1], DateTime.Today.AddYears(1), voucher.Code, ticketBasket.TotalPrice);


                            UserVoucherTypeDetail detail = new UserVoucherTypeDetail();

                            detail.UserId = UserMethods.CurrentUser.Id; ;
                            detail.VType = (int)(VType.GiftVoucher);
                            detail.VoucherType = (int)(VoucherType.ValueOff);
                            detail.Name = lstvalue[0];
                            detail.FriendEmailId = lstvalue[1];
                            detail.CreatedDate = DateTime.Now;
                            detail.Value = ticketBasket.TotalPrice;
                            detail.VoucherId = voucher.Id;
                            detail.BasketId = ticketBasket.Id;

                            UserVoucherTypeDetail _detail = _supperClubRepository.CreateUserVoucherTypeDetail(detail);

                            // return View("PostPaymentSuccessGift.cshtml");
                        }
                    }
                    else
                    {
                        EmailService.EmailService es = new EmailService.EmailService(_supperClubRepository);
                        bool _emailSuccess = es.SendGuestBookedEmails(
                            UserMethods.CurrentUser.FirstName,
                            UserMethods.CurrentASPNETUser.Email,
                            ticketBasket.TotalTickets,
                            ticketBasket.TotalPrice - ticketBasket.TotalDiscount,
                            ticketBasket.BookingReference,
                            ticketBasket.BookingRequirements,
                            bookingModel.eventId,
                            bookingModel.seatingId,
                            bookingModel.bookingMenuModel,
                            bookingModel.voucherId,
                            ticketBasket.CCLastDigits,
                            bookingModel.commission,
                            ref hostSuccess
                            );
                        if (_emailSuccess)
                            LogMessage("Guest Booking Confirmation Email: Sent successfully", LogLevel.INFO);
                        else
                            LogMessage("Guest Booking Confirmation Email: Failure sending e-mail", LogLevel.ERROR);

                        if (hostSuccess)
                            LogMessage("Host Booking Information Email: Sent successfully", SupperClub.Logger.LogLevel.INFO);
                        else
                            LogMessage("Host Booking Information Email: Failure sending e-mail", LogLevel.ERROR);
                    }
                    return RedirectToAction("PrePostPayment");
                }
                else
                {
                    BookingModel bm = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                    tr.Success = false;
                    tr.TransactionMessage = transactionResponse.StatusDetail;
                    tr.EventId = bm.eventId;
                    TempData["TransactionResult"] = tr;
                    return RedirectToAction("PrePostPayment");
                }
            }
            catch (Exception ex)
            {
                LogMessage("Error in three D secure reply to SagePay. MD=" + MD + " PaRes=" + PaRes + " ErrorMessage: " + ex.Message + "  Stack Trace: " + ex.StackTrace, LogLevel.ERROR);
                BookingModel bm = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                TransactionResult tr = new TransactionResult();
                tr.Success = false;
                tr.TransactionMessage = "Error verifying 3D secure details.";
                tr.EventId = bm.eventId;
                TempData["TransactionResult"] = tr;
                return RedirectToAction("PostPaymentFailure");
            }
        }

        [Authorize]
        public ActionResult PrePostPayment()
        {
            TransactionResult tr = new TransactionResult();
            if (Request["Success"] != null)
            {
                tr.Success = (Request["Success"].ToLower() == "true") ? true : false;
                tr.TransactionMessage = Request["TransactionMessage"];
                tr.EventId = int.Parse(Request["EventId"]);
                TempData["TransactionResult"] = tr;

                if (tr.Success)
                {
                    if (tr.EventId == 777)
                    {
                        return RedirectToAction("PostPaymentSuccess");
                    }
                    else
                    {
                        return RedirectToAction("PostPaymentSuccessNew");
                    }
                }
                else
                    return RedirectToAction("PostPaymentFailure");
            }
            else
            {
                tr = (TransactionResult)TempData["TransactionResult"];
            }
            return View("SagePay/PrePostPayment", tr);
        }

        #endregion

        #endregion

        #region BrainTree
        [Authorize]
        public ActionResult CreditCardDetailsBraintree()
        {
            LogMessage("Credit Card Details");
            if (_ticketingService.TicketBasketAlive(this.HttpContext))
            {
                BookingModel bookingModel = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                if ((bookingModel.bookingMenuModel != null && bookingModel.bookingMenuModel.Count > 0) && !(bookingModel.eventId == 777))
                {
                    decimal totalCost = 0;
                    decimal totalDiscount = 0;
                    foreach (BookingMenuModel bmm in bookingModel.bookingMenuModel)
                    {
                        totalCost += SupperClub.Domain.CostCalculator.CostToGuest(bmm.baseTicketCost, bookingModel.commission) * bmm.numberOfTickets;
                        totalDiscount += bmm.discount;
                    }
                    if (totalCost - totalDiscount <= 0)
                        return RedirectToAction("FreeBooking");
                }
                else if (bookingModel.totalDue - bookingModel.discount <= 0)
                    return RedirectToAction("FreeBooking");

                if (bookingModel.seatingId > 0)
                {
                    DateTime selectedSeating = (from item in bookingModel.bookingSeatingModel
                                          where item.seatingId == bookingModel.seatingId
                                          select item.start).FirstOrDefault();
                    ViewBag.selectedSeating = selectedSeating;
                }

                Event _event = new Event();
                if (bookingModel.eventId > 0)
                    _event = _supperClubRepository.GetEvent(bookingModel.eventId);
                string contactNumber = SupperClub.Code.UserMethods.CurrentUser.ContactNumber == null ? "" : SupperClub.Code.UserMethods.CurrentUser.ContactNumber;
                CreditCardDetailsBraintreeModel ccdmodel = (_testCardType == CreditCardType.NONE) ? new CreditCardDetailsBraintreeModel() : GetBraintreeTestCreditCardModel();
                CreditCardDetailsBraintreeViewModel model = new CreditCardDetailsBraintreeViewModel { bookingModel = bookingModel, creditCardDetails = ccdmodel, Event = _event, ContactNumber = contactNumber };
                BraintreeCustomer customer = _supperClubRepository.GetBraintreeCustomer((Guid)UserMethods.CurrentUser.Id);
                User user = UserMethods.CurrentUser;
                if (braintreeEnvironmentSetting.ToLower() == "live")
                    braintreeEnvironment = Braintree.Environment.PRODUCTION;

                var gateway = new BraintreeGateway
                {
                    Environment = braintreeEnvironment,
                    MerchantId = braintreeMerchantId,
                    PublicKey = braintreePublicKey,
                    PrivateKey = braintreePrivateKey
                };
                string clientToken = string.Empty;
                model.SelectedToken = string.Empty;
                if (customer == null)
                {
                    var request = new CustomerRequest
                    {
                        FirstName = string.IsNullOrEmpty(user.FirstName) ? "" : user.FirstName,
                        LastName = string.IsNullOrEmpty(user.LastName) ? "" : user.LastName,
                        Email = user.Email
                    };
                    Result<Customer> result = gateway.Customer.Create(request);

                    if (result != null && result.IsSuccess() && result.Target != null)
                    {
                        string customerId = result.Target.Id;
                        BraintreeCustomer bc = new BraintreeCustomer();
                        bc.UserId = user.Id;
                        bc.BraintreeCustomerId = customerId;
                        if (_supperClubRepository.CreateBrainTreeCustomer(bc))
                        {
                            var _clientToken = gateway.ClientToken.generate(
                                 new ClientTokenRequest
                                 {
                                     CustomerId = customerId,
                                     Options = new ClientTokenOptionsRequest
                                     {
                                         VerifyCard = true
                                     }
                                 });
                            if (_clientToken != null)
                                clientToken = _clientToken.ToString();
                        }
                    }
                }
                else
                {
                    //get customer card details
                    Customer _customer = gateway.Customer.Find(customer.BraintreeCustomerId);
                    if (_customer != null && _customer.CreditCards != null && _customer.CreditCards.Length > 0)
                    {
                        List<BraintreeCreditCard> lstCC = new List<BraintreeCreditCard>();
                        //Result<PaymentMethod> _delResult = null;
                        foreach (Braintree.CreditCard cc in _customer.CreditCards.OrderByDescending(y => y.IsDefault).ToArray())
                        {
                            bool addToList = false;
                            int diff = int.Parse(cc.ExpirationYear) - DateTime.Now.Year;
                            if (diff > 0)
                                addToList = true;
                            else if (diff == 0)
                            {
                                diff = int.Parse(cc.ExpirationMonth) - DateTime.Now.Month;
                                if (diff > 0)
                                    addToList = true;
                            }
                            //else
                            //{
                            //    if (gateway.PaymentMethod.Delete(cc.Token).IsSuccess())
                            //        LogMessage("Deleted expired card. Payment method Token=" + cc.Token, LogLevel.INFO );

                            //}
                            if (addToList)
                            {
                                BraintreeCreditCard bcc = new BraintreeCreditCard();
                                bcc.CardType = cc.CardType.ToString();
                                bcc.CardLastFour = cc.LastFour;
                                bcc.ExpirationMonth = cc.ExpirationMonth;
                                bcc.ExpirationYear = cc.ExpirationYear;
                                bcc.IsDefault = (bool)cc.IsDefault;
                                if (bcc.IsDefault)
                                    model.SelectedToken = cc.Token;
                                bcc.Token = cc.Token;
                                lstCC.Add(bcc);
                            }
                        }
                        model.SavedCardList = lstCC;
                    }
                    ViewBag.CardListLength = (model.SavedCardList != null && model.SavedCardList.Count > 0) ? model.SavedCardList.Count : 0;
                    model.UseSavedCard = (model.SavedCardList != null && model.SavedCardList.Count > 0) ? true : false;

                    var _clientToken = gateway.ClientToken.generate(
                                         new ClientTokenRequest
                                         {
                                             CustomerId = customer.BraintreeCustomerId
                                         });
                    if (_clientToken != null)
                        clientToken = _clientToken.ToString();
                }
                if (!string.IsNullOrEmpty(clientToken))
                {
                    LogMessage("Client token generated. Token=" + clientToken, LogLevel.INFO);
                    ViewBag.ClientToken = clientToken;
                }
                else
                {
                    LogMessage("Error generating client token", LogLevel.ERROR);
                }
                return View("Braintree/CreditCardDetails", model);
            }
            else
                return RedirectToAction("SessionExpired");
        }        
        [Authorize]
        [HttpPost]
        public ActionResult CreditCardDetailsBraintree(CreditCardDetailsBraintreeViewModel model)
        {
            LogMessage("Credit Card Details Submitted");
            TransactionResult tr = new TransactionResult();

            if (_ticketingService.TicketBasketAlive(this.HttpContext))
            {
                BookingModel bm_check = _ticketingService.GetBookingModelFromBasket(this.HttpContext);

                if (bm_check != null && bm_check.eventId != model.bookingModel.eventId)
                {
                    return RedirectToAction("UpdatedBasket");
                }
                if (!model.UseSavedCard)
                {
                    if (model.creditCardDetails.ExpiryYear + 2000 < DateTime.Now.Year)
                        ModelState.AddModelError("creditCardDetails_ExpiryYear", "This card has expired");

                    if (model.bookingModel.IsContactNumberRequired && string.IsNullOrEmpty(model.ContactNumber))
                        ModelState.AddModelError("ContactNumber", "You must provide a phone number to continue booking!");
                }
                //  if ((model.bookingModel.IsContactNumberRequired && !string.IsNullOrEmpty(model.ContactNumber)) || !string.IsNullOrEmpty(model.ContactNumber))
                //  {
                //var regex1 = @"^\(?(?:(?:0(?:0|11)\)?[\s-]?\(?|\+)44\)?[\s-]?\(?(?:0\)?[\s-]?\(?)?|0)(?:\d{2}\)?[\s-]?\d{4}[\s-]?\d{4}|\d{3}\)?[\s-]?\d{3}[\s-]?\d{3,4}|\d{4}\)?[\s-]?(?:\d{5}|\d{3}[\s-]?\d{3})|\d{5}\)?[\s-]?\d{4,5}|8(?:00[\s-]?11[\s-]?11|45[\s-]?46[\s-]?4\d))(?:(?:[\s-]?(?:x|ext\.?\s?|\#)\d+)?)$";
                //var match = Regex.Match(model.ContactNumber, regex1, RegexOptions.IgnoreCase);
                //if (!match.Success)
                //{
                //    ModelState.AddModelError("ContactNumber", "Please enter a 11 digit phone number. The number entered is not in a valid format.");
                //}
                //  }
                if (model.bookingModel.seatingId > 0)
                {
                    DateTime selectedSeating = (from item in model.bookingModel.bookingSeatingModel
                                                where item.seatingId == model.bookingModel.seatingId
                                                select item.start).FirstOrDefault();
                    ViewBag.selectedSeating = selectedSeating;
                }
                if ((!model.UseSavedCard && ModelState.IsValid && model.PaymentNonce != null) || (model.UseSavedCard && model.PaymentNonce != null))
                {   
                    if (braintreeEnvironmentSetting.ToLower() == "live")
                        braintreeEnvironment = Braintree.Environment.PRODUCTION;

                    var gateway = new BraintreeGateway
                    {
                        Environment = braintreeEnvironment,
                        MerchantId = braintreeMerchantId,
                        PublicKey = braintreePublicKey,
                        PrivateKey = braintreePrivateKey
                    };

                    // Update basket with some CC details
                    TicketBasket ticketBasket = _ticketingService.GetTicketBasket(this.HttpContext);
                   
                    if (model.UseSavedCard && !string.IsNullOrEmpty(model.SelectedToken))
                    {
                        Braintree.PaymentMethod pm = gateway.PaymentMethod.Find(model.SelectedToken);
                        Braintree.CreditCard cc = (Braintree.CreditCard)pm;
                        if (cc != null && !string.IsNullOrEmpty(cc.LastFour))
                        {
                            ticketBasket.CCLastDigits = cc.LastFour;
                            ticketBasket.CCExpiryDate = cc.ExpirationMonth + @"/" + (cc.ExpirationYear.Substring(2));
                        }
                    }
                    else if (!model.UseSavedCard)
                    {
                        ticketBasket.CCLastDigits = model.creditCardDetails.CreditCardNumber.Substring(model.creditCardDetails.CreditCardNumber.Length - 4, 4);
                        ticketBasket.CCExpiryDate = ((int)model.creditCardDetails.ExpiryMonth).ToString("00") + @"/" + ((int)model.creditCardDetails.ExpiryYear).ToString("00");
                    }
                    _ticketingService.UpdateTicketBasket(ticketBasket, TicketBasketStatus.InProgress);

                    // Save user info
                    if (!model.UseSavedCard)
                        saveUser(model.ContactNumber, model.creditCardDetails);

                    BraintreeCustomer bc = _supperClubRepository.GetBraintreeCustomer(ticketBasket.UserId);
                    if (bc == null)
                    {
                        User user = _supperClubRepository.GetUser(ticketBasket.UserId);
                        var request = new CustomerRequest
                        {
                            FirstName = string.IsNullOrEmpty(user.FirstName) ? "" : user.FirstName,
                            LastName = string.IsNullOrEmpty(user.LastName) ? "" : user.LastName,
                            Email = user.Email
                        };
                        Result<Customer> result = gateway.Customer.Create(request);

                        if (result != null && result.IsSuccess() && result.Target != null)
                        {
                            string customerId = result.Target.Id;
                            BraintreeCustomer _bc = new BraintreeCustomer();
                            _bc.UserId = user.Id;
                            _bc.BraintreeCustomerId = customerId;
                            bool _createSuccess = _supperClubRepository.CreateBrainTreeCustomer(_bc);
                            if (_createSuccess)
                            {
                                LogMessage("Braintree customer added to database successfully.", LogLevel.INFO);
                                bc = _bc;
                            }
                            else
                                LogMessage("Braintree customer could not be added to database.", LogLevel.ERROR);
                        }
                    }
                    bool transactionProcessed = false;
                    Result<Transaction> payResult = null;
                    Event _event = _supperClubRepository.GetEvent(model.bookingModel.eventId);
                    model.bookingModel.EventName = _event != null ?_event.Name : "";
                    if (!model.UseSavedCard)
                    { 
                        PaymentMethodRequest _request = new PaymentMethodRequest
                        {
                            CustomerId = bc.BraintreeCustomerId,
                            PaymentMethodNonce = model.PaymentNonce,
                            Options = new PaymentMethodOptionsRequest
                            {
                                //FailOnDuplicatePaymentMethod = true,
                                MakeDefault = true,
                                VerifyCard = true
                            }
                        };
                        Result<PaymentMethod> result1 = gateway.PaymentMethod.Create(_request);

                        if (result1.IsSuccess() && result1.Target != null && !string.IsNullOrEmpty(result1.Target.Token))
                        {
                            LogMessage("Payment method added successfully to Braintree server. Token=" + result1.Target.Token, LogLevel.INFO);
                            LogMessage("Basket Info. Total Price=" + ticketBasket.TotalPrice.ToString() + ", TotalDiscount=" + ticketBasket.TotalDiscount.ToString(), LogLevel.INFO);
                            //process transaction with Token
                            payResult = gateway.Transaction.Sale(
                                    new TransactionRequest
                                    {
                                        PaymentMethodToken = result1.Target.Token,
                                        Amount = decimal.Round(ticketBasket.TotalPrice - ticketBasket.TotalDiscount, 2, MidpointRounding.AwayFromZero),                                        
                                        Options = new TransactionOptionsRequest
                                        {
                                            SubmitForSettlement = true
                                        },
                                        CustomFields = new Dictionary<string, string>
                                            {
                                                { "event_id", model.bookingModel.eventId.ToString() },
                                                { "event_name", model.bookingModel.EventName },
                                                { "user_email", UserMethods.CurrentUser.Email },
                                                { "booking_reference", ticketBasket.BookingReference.ToString()}
                                            }
                                    });
                            if (payResult.Target != null && (payResult.Target.Status == TransactionStatus.AUTHORIZED || payResult.Target.Status == TransactionStatus.SUBMITTED_FOR_SETTLEMENT) && payResult.IsSuccess())
                                transactionProcessed = true;
                        }
                        else
                        {
                            // false
                            CreditCardVerification verification = result1.CreditCardVerification;
                            if (verification != null)
                            {
                                LogMessage("Error adding Payment method to Braintree. verificationStatus=" + verification.Status + "  verificationProcessorResponseCode=" + verification.ProcessorResponseCode + "  verificationProcessorResponseText=" + verification.ProcessorResponseText, LogLevel.ERROR);
                                int _processorCode = 0;
                                try
                                {
                                    _processorCode = int.Parse(verification.ProcessorResponseCode);
                                }
                                catch
                                {
                                    LogMessage("Error occured while trying to convert processor code to integer", LogLevel.ERROR);
                                    _processorCode = -1;
                                }
                                if (_processorCode != 0 && (_processorCode >= 2000 || _processorCode == -1))
                                {
                                    BookingModel bm = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                                    tr.Success = false;
                                    tr.TransactionMessage = "Transaction not authorized. Your bank returned the following error while processing the transaction: " + result1.Message;
                                    tr.EventId = bm.eventId;
                                    TempData["TransactionResult"] = tr;
                                    return RedirectToAction("PostPaymentFailure");
                                }

                            }
                            else
                            {
                                LogMessage("Error adding Payment method to Braintree. Message=" + result1.Message, LogLevel.ERROR);
                                BookingModel bm = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                                tr.Success = false;
                                tr.TransactionMessage = "Transaction not authorized. Your bank returned the following error while processing the transaction: " + result1.Message;
                                tr.EventId = bm.eventId;
                                TempData["TransactionResult"] = tr;
                                return RedirectToAction("PostPaymentFailure");
                            }
                        }
                    }


                    if (!transactionProcessed)
                    {
                        LogMessage("Basket Info. Total Price=" + ticketBasket.TotalPrice.ToString() + ", TotalDiscount=" + ticketBasket.TotalDiscount.ToString(), LogLevel.INFO);
                            
                        var payRequest = bc != null ? (new TransactionRequest
                        {
                            Amount = decimal.Round(ticketBasket.TotalPrice - ticketBasket.TotalDiscount, 2, MidpointRounding.AwayFromZero),
                            PaymentMethodNonce = model.PaymentNonce,
                            CustomerId = bc.BraintreeCustomerId,
                            Options = new TransactionOptionsRequest
                            {
                                SubmitForSettlement = true
                            },
                            CustomFields = new Dictionary<string, string>
                                            {
                                                { "event_id", model.bookingModel.eventId.ToString() },
                                                { "event_name", model.bookingModel.EventName },
                                                { "user_email", UserMethods.CurrentUser.Email },
                                                { "booking_reference", ticketBasket.BookingReference.ToString()}
                                            }
                        }) : (new TransactionRequest
                        {
                            Amount = decimal.Round(ticketBasket.TotalPrice - ticketBasket.TotalDiscount, 2, MidpointRounding.AwayFromZero),
                            PaymentMethodNonce = model.PaymentNonce,
                            Options = new TransactionOptionsRequest
                            {
                                SubmitForSettlement = true
                            },
                            CustomFields = new Dictionary<string, string>
                                            {
                                                { "event_id", model.bookingModel.eventId.ToString() },
                                                { "event_name", model.bookingModel.EventName },
                                                { "user_email", UserMethods.CurrentUser.Email },
                                                { "booking_reference", ticketBasket.BookingReference.ToString()}
                                            }
                        });
                        payResult = gateway.Transaction.Sale(payRequest);
                    }


                    if (payResult.Target != null && (payResult.Target.Status == TransactionStatus.AUTHORIZED || payResult.Target.Status == TransactionStatus.SUBMITTED_FOR_SETTLEMENT) && payResult.IsSuccess())
                    {
                        LogMessage("Transaction authorized.", LogLevel.INFO);
                                                
                        //TODO Add transaction logging
                        BraintreeTransaction bt = new BraintreeTransaction
                        {
                            TransactionStatus = payResult.Message == null ? (payResult.Target != null ? payResult.Target.Status.ToString() : "") : payResult.Message,
                            TransactionSuccess = payResult.IsSuccess(),
                            TransactionValidVenmoSDK = payResult.Target.CreditCard.IsVenmoSdk.Value,
                            Amount = payResult.Target.Amount,
                            AvsErrorResponseCode = payResult.Target.AvsErrorResponseCode,
                            AvsStreetAddressResponseCode = payResult.Target.AvsStreetAddressResponseCode,
                            AvsPostalCodeResponseCode = payResult.Target.AvsPostalCodeResponseCode,
                            Channel = payResult.Target.Channel,
                            TransactionCreationDate = payResult.Target.CreatedAt,
                            TransactionUpdateDate = payResult.Target.UpdatedAt,
                            CvvResponseCode = payResult.Target.CvvResponseCode,
                            TransactionId = payResult.Target.Id,
                            MerchantAccountId = payResult.Target.MerchantAccountId,
                            OrderId = payResult.Target.OrderId,
                            PlanId = payResult.Target.PlanId,
                            ProcessorAuthorizationCode = payResult.Target.ProcessorAuthorizationCode,
                            ProcessorResponseCode = payResult.Target.ProcessorResponseCode,
                            ProcessorResponseText = payResult.Target.ProcessorResponseText,
                            PurchaseOrderNumber = payResult.Target.PurchaseOrderNumber,
                            ServiceFeeAmount = payResult.Target.ServiceFeeAmount,
                            SettlementBatchId = payResult.Target.SettlementBatchId,
                            Status = payResult.Target.Status.ToString(),
                            TaxAmount = payResult.Target.TaxAmount,
                            TaxExempt = payResult.Target.TaxExempt,
                            TransactionType = payResult.Target.Type.ToString()
                        };

                        // Add Transaction
                        if (_supperClubRepository.CreateTransction(bt) != null)
                            LogMessage("Transaction added to database successfully.", LogLevel.INFO);
                        else
                            LogMessage("Transaction could not be added to database.", LogLevel.ERROR);


                        _ticketingService.UpdateTicketBasket(ticketBasket, TicketBasketStatus.Complete);

                        BookingModel bm = _ticketingService.GetBookingModelFromBasket(this.HttpContext);

                        bool success = _ticketingService.AddUserToEvent(ticketBasket, bm);
                        if (success && ticketBasket.TotalDiscount > 0)
                            _ticketingService.UpdateVoucherUsage(ticketBasket.Tickets[0].VoucherId, ticketBasket.TotalDiscount);

                        // Email - Guest: with confirmation, Host - that someone booked a ticket
                        bool hostSuccess = false;
                        if (model.bookingModel.eventId == 777)
                        {
                            //create and get voucher
                            List<string> lstvalue = Session["GiftVoucher"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            string v_code = string.Empty;
                            Voucher voucher;
                            SupperClub.Code.VoucherManager vmgr = new VoucherManager();
                            voucher = vmgr.CreateVoucher("Gift Voucher - £" + ticketBasket.TotalPrice.ToString(), ticketBasket.TotalPrice, (int)VoucherType.GiftVoucher);
                            //send email
                            if (!string.IsNullOrEmpty(voucher.Code))
                            {
                                EmailService.EmailService emailer = new EmailService.EmailService(_supperClubRepository);
                                // Send confirmation e-mail to user
                                bool _success = emailer.SendGiftVoucherEmail(UserMethods.CurrentUser.FirstName, UserMethods.CurrentASPNETUser.Email, DateTime.Today.AddYears(1), voucher.Code, ticketBasket.TotalPrice);
                                // Send confirmation e-mail to friend
                                _success =  emailer.SendGiftVoucherEmail(lstvalue[0], lstvalue[1], DateTime.Today.AddYears(1), voucher.Code, ticketBasket.TotalPrice);
                                UserVoucherTypeDetail detail = new UserVoucherTypeDetail();
                                detail.UserId = UserMethods.CurrentUser.Id; ;
                                detail.VType = (int)(VType.GiftVoucher);
                                detail.VoucherType = (int)(VoucherType.ValueOff);
                                detail.Name = lstvalue[0];
                                detail.FriendEmailId = lstvalue[1];
                                detail.CreatedDate = DateTime.Now;
                                detail.Value = ticketBasket.TotalPrice;
                                detail.VoucherId = voucher.Id;
                                detail.BasketId = ticketBasket.Id;
                                UserVoucherTypeDetail _detail = _supperClubRepository.CreateUserVoucherTypeDetail(detail);
                            }
                        }
                        else
                        {
                            EmailService.EmailService es = new EmailService.EmailService(_supperClubRepository);
                            bool _emailSuccess = es.SendGuestBookedEmails(
                                UserMethods.CurrentUser.FirstName,
                                UserMethods.CurrentASPNETUser.Email,
                                ticketBasket.TotalTickets,
                                ticketBasket.TotalPrice - ticketBasket.TotalDiscount,
                                ticketBasket.BookingReference,
                                ticketBasket.BookingRequirements,
                                model.bookingModel.eventId,
                                model.bookingModel.seatingId,
                                model.bookingModel.bookingMenuModel,
                                model.bookingModel.voucherId,
                                ticketBasket.CCLastDigits,
                                model.bookingModel.commission,
                                ref hostSuccess
                                );
                            if (_emailSuccess)
                                LogMessage("Guest Booking Confirmation Email: Sent successfully", LogLevel.INFO);
                            else
                                LogMessage("Guest Booking Confirmation Email: Failure sending e-mail", LogLevel.ERROR);

                            if (hostSuccess)
                                LogMessage("Host Booking Information Email: Sent successfully", SupperClub.Logger.LogLevel.INFO);
                            else
                                LogMessage("Host Booking Information Email: Failure sending e-mail", LogLevel.ERROR);
                        }
                        tr.Success = true;
                        tr.TransactionMessage = payResult.Message == null ? (payResult.Target != null ? payResult.Target.Status.ToString() : "") : payResult.Message;
                        tr.EventId = model.bookingModel.eventId;
                        TempData["TransactionResult"] = tr;

                        //Segment data tracking 
                        Voucher _voucher = new Voucher();
                        if (model.bookingModel.voucherId > 0)
                        {
                            _voucher = _supperClubRepository.GetVoucherDetail(model.bookingModel.voucherId);
                        }
                        bool IsFollowingCurrentChef =  _supperClubRepository.IsCurrentSupperClubUsersFavourite(_event.SupperClubId, UserMethods.CurrentUser.Id);

                        //manage event booking cookie
                        string _eventBookingCookie = Web.Helpers.Utils.CookieStore.GetCookie(_eventBookingCookieKey);
                        if (string.IsNullOrEmpty(_eventBookingCookie))
                        {
                            _eventBookingCookie = serializeCookieData(createEventBookingList(model.bookingModel.eventId));
                            Web.Helpers.Utils.CookieStore.SetCookie(_eventBookingCookieKey, _eventBookingCookie, DateTime.Now.AddMonths(_eventBookingCookieExpirationInMonths) - DateTime.Now);
                        }
                        else
                        {
                            _eventBookingCookie = serializeCookieData(updateEventBookingList(model.bookingModel.eventId, deserializeCookieData(_eventBookingCookie)));
                            Web.Helpers.Utils.CookieStore.SetCookie(_eventBookingCookieKey, _eventBookingCookie, DateTime.Now.AddMonths(_eventBookingCookieExpirationInMonths) - DateTime.Now);
                        }
                        Segment.Analytics.Client.Track(_segmentMethod.GetSegmentUser(), "Booked an event", new Segment.Model.Properties() {
                            { "GuestEmail", UserMethods.CurrentUser.Email },
                            { "GuestFirstName", UserMethods.CurrentUser.FirstName },
                            { "GuestLastName", UserMethods.CurrentUser.LastName },
                            { "EventName", model.bookingModel.EventName },
                            { "EventDate", model.bookingModel.EventDateAndTime },
                            { "EventLocation", _event.Address + (string.IsNullOrEmpty(_event.Address2) ? "":("," + _event.Address2)) },
                            { "EventPostCode", _event.PostCode },
                            { "BYOB", _event.Alcohol ? "Yes":"No" },
                            { "Charity", _event.Charity ? "Yes":"No" },
                            { "PerTicketPrice", _event.CostToGuest.ToString() },
                            { "NumberOfReviews", _event.SupperClub.NumberOfReviews.ToString() },
                            { "AverageStarRating", _event.SupperClub.AverageRank.ToString() },
                            { "BookingReference",  ticketBasket.BookingReference },
                            { "NumberOfTicketsBooked", model.bookingModel.numberOfTickets },
                            { "TotalPrice", ticketBasket.TotalPrice  },
                            { "Discount", ticketBasket.TotalDiscount },
                            { "TotalPricePaid", ticketBasket.TotalPrice - ticketBasket.TotalDiscount },
                            { "SpecialRequirements",  ticketBasket.BookingRequirements },
                            { "IsUserHostOnGC", ((UserMethods.CurrentUser.SupperClubs != null && UserMethods.CurrentUser.SupperClubs.Count > 0) ? "Yes" : "No") },
                            { "FirstTimeCustomer", ((UserMethods.CurrentUser.UserEvents == null || UserMethods.CurrentUser.PastEvents.Count == 0) ? "Yes" : "No")},
                            { "VoucherUsed", model.bookingModel.voucherId > 0 ? "Yes" : "No"},
                            { "VoucherName", model.bookingModel.voucherId > 0 ? _voucher.Description : ""},
                            { "VoucherCode", model.bookingModel.voucherId > 0 ? _voucher.Code : ""},
                            { "IsGuestFollowingThisChef", IsFollowingCurrentChef ? "Yes" : "No"},
                            { "IsEventSoldOut", _event.TotalNumberOfAvailableSeats - model.bookingModel.numberOfTickets <= 0 ? "Yes" : "No"}
                           
                        });

                        if (tr.EventId == 777)
                        {
                            return RedirectToAction("PostPaymentSuccessGC");
                        }
                        else
                        {
                            return RedirectToAction("PostPaymentSuccessNew");
                        }
                    }
                    else
                    {
                        BookingModel bm = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                        tr.Success = false;
                        tr.TransactionMessage = "Transaction not authorized." + (payResult.Message == null ? (payResult.Target != null ? payResult.Target.Status.ToString() : "") : payResult.Message);
                        tr.EventId = bm.eventId;
                        TempData["TransactionResult"] = tr;
                        // Log error
                        LogMessage(tr.TransactionMessage, LogLevel.ERROR);                        
                        return RedirectToAction("PostPaymentFailure");
                    }
                }
                else
                {
                    if (model.bookingModel.eventId != null)
                        model.Event = _supperClubRepository.GetEvent(model.bookingModel.eventId);
                    if (_ticketingService.TicketBasketAlive(this.HttpContext))
                        model.bookingModel = _ticketingService.GetBookingModelFromBasket(this.HttpContext);

                    foreach (var modelStateValue in ViewData.ModelState.Values)
                    {
                        foreach (var error in modelStateValue.Errors)
                        {
                            // Log error
                            LogMessage("Validation Failed. ErrorMessage:" + error.ErrorMessage + ". Exception:" + error.Exception, LogLevel.DEBUG);
                        }
                    }
                    return View("BrainTree/CreditCardDetails", model);
                }
            }
            else
                return RedirectToAction("SessionExpired");
        }
        [Authorize]
        public ActionResult CreditCardDetailsBraintree3D()
        {
            LogMessage("Credit Card Details");
            if (_ticketingService.TicketBasketAlive(this.HttpContext))
            {
                BookingModel bookingModel = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                if ((bookingModel.bookingMenuModel != null && bookingModel.bookingMenuModel.Count > 0) && !(bookingModel.eventId == 777))
                {
                    decimal totalCost = 0;
                    decimal totalDiscount = 0;
                    foreach (BookingMenuModel bmm in bookingModel.bookingMenuModel)
                    {
                        totalCost += SupperClub.Domain.CostCalculator.CostToGuest(bmm.baseTicketCost, bookingModel.commission) * bmm.numberOfTickets;
                        totalDiscount += bmm.discount;
                    }
                    if (totalCost - totalDiscount <= 0)
                        return RedirectToAction("FreeBooking");
                }
                else if (bookingModel.totalDue - bookingModel.discount <= 0)
                    return RedirectToAction("FreeBooking");

                Event _event = new Event();
                if (bookingModel.eventId > 0)
                    _event = _supperClubRepository.GetEvent(bookingModel.eventId);
                string contactNumber = SupperClub.Code.UserMethods.CurrentUser.ContactNumber == null ? "" : SupperClub.Code.UserMethods.CurrentUser.ContactNumber;
                CreditCardDetailsBraintreeModel ccdmodel = GetBraintreeTestCreditCardModel();
                CreditCardDetailsBraintreeViewModel model = new CreditCardDetailsBraintreeViewModel { bookingModel = bookingModel, creditCardDetails = ccdmodel, Event = _event, ContactNumber = contactNumber };
                BraintreeCustomer customer = _supperClubRepository.GetBraintreeCustomer((Guid)UserMethods.CurrentUser.Id);
                User user = UserMethods.CurrentUser;
                if (braintreeEnvironmentSetting.ToLower() == "live")
                    braintreeEnvironment = Braintree.Environment.PRODUCTION;

                var gateway = new BraintreeGateway
                {
                    Environment = braintreeEnvironment,
                    MerchantId = braintreeMerchantId,
                    PublicKey = braintreePublicKey,
                    PrivateKey = braintreePrivateKey
                };
                string clientToken = string.Empty;
                model.SelectedToken = string.Empty;
                if (customer == null)
                {
                    var request = new CustomerRequest
                    {
                        FirstName = string.IsNullOrEmpty(user.FirstName) ? "" : user.FirstName,
                        LastName = string.IsNullOrEmpty(user.LastName) ? "" : user.LastName,
                        Email = user.Email
                    };
                    Result<Customer> result = gateway.Customer.Create(request);

                    if (result != null && result.IsSuccess() && result.Target != null)
                    {
                        string customerId = result.Target.Id;
                        BraintreeCustomer bc = new BraintreeCustomer();
                        bc.UserId = user.Id;
                        bc.BraintreeCustomerId = customerId;
                        if (_supperClubRepository.CreateBrainTreeCustomer(bc))
                        {
                            var _clientToken = gateway.ClientToken.generate(
                                 new ClientTokenRequest
                                 {
                                     CustomerId = customerId,
                                     Options = new ClientTokenOptionsRequest
                                     {
                                         VerifyCard = true
                                     }
                                 });
                            if (_clientToken != null)
                                clientToken = _clientToken.ToString();
                        }
                    }
                }
                else
                {
                    //get customer card details
                    Customer _customer = gateway.Customer.Find(customer.BraintreeCustomerId);
                    if (_customer != null && _customer.CreditCards != null && _customer.CreditCards.Length > 0)
                    {
                        List<BraintreeCreditCard> lstCC = new List<BraintreeCreditCard>();
                        //Result<PaymentMethod> _delResult = null;
                        foreach (Braintree.CreditCard cc in _customer.CreditCards.OrderByDescending(y => y.IsDefault).ToArray())
                        {
                            bool addToList = false;
                            int diff = int.Parse(cc.ExpirationYear) - DateTime.Now.Year;
                            if (diff > 0)
                                addToList = true;
                            else if (diff == 0)
                            {
                                diff = int.Parse(cc.ExpirationMonth) - DateTime.Now.Month;
                                if (diff > 0)
                                    addToList = true;
                            }
                            //else
                            //{
                            //    if (gateway.PaymentMethod.Delete(cc.Token).IsSuccess())
                            //        LogMessage("Deleted expired card. Payment method Token=" + cc.Token, LogLevel.INFO );

                            //}
                            if (addToList)
                            {
                                BraintreeCreditCard bcc = new BraintreeCreditCard();
                                bcc.CardType = cc.CardType.ToString();
                                bcc.CardLastFour = cc.LastFour;
                                bcc.ExpirationMonth = cc.ExpirationMonth;
                                bcc.ExpirationYear = cc.ExpirationYear;
                                bcc.IsDefault = (bool)cc.IsDefault;
                                if (bcc.IsDefault)
                                    model.SelectedToken = cc.Token;
                                bcc.Token = cc.Token;
                                lstCC.Add(bcc);
                            }
                        }
                        model.SavedCardList = lstCC;
                    }
                    ViewBag.CardListLength = (model.SavedCardList != null && model.SavedCardList.Count > 0) ? model.SavedCardList.Count : 0;
                    model.UseSavedCard = (model.SavedCardList != null && model.SavedCardList.Count > 0) ? true : false;

                    var _clientToken = gateway.ClientToken.generate(
                                         new ClientTokenRequest
                                         {
                                             CustomerId = customer.BraintreeCustomerId
                                         });
                    if (_clientToken != null)
                        clientToken = _clientToken.ToString();
                }
                if (!string.IsNullOrEmpty(clientToken))
                {
                    LogMessage("Client token generated. Token=" + clientToken, LogLevel.INFO);
                    ViewBag.ClientToken = clientToken;
                }
                else
                {
                    LogMessage("Error generating client token", LogLevel.ERROR);
                }
                return View("Braintree/CreditCardDetails3D", model);
            }
            else
                return RedirectToAction("SessionExpired");
        }
        [Authorize]
        [HttpPost]
        public ActionResult CreditCardDetailsBraintree3D(CreditCardDetailsBraintreeViewModel model)
        {
            LogMessage("Credit Card Details Submitted");
            TransactionResult tr = new TransactionResult();

            if (_ticketingService.TicketBasketAlive(this.HttpContext))
            {
                if (model.creditCardDetails.ExpiryYear + 2000 < DateTime.Now.Year)
                    ModelState.AddModelError("creditCardDetails_ExpiryYear", "This card has expired");

                if (model.bookingModel.IsContactNumberRequired && string.IsNullOrEmpty(model.ContactNumber))
                    ModelState.AddModelError("ContactNumber", "You must provide a phone number to continue booking!");

                //  if ((model.bookingModel.IsContactNumberRequired && !string.IsNullOrEmpty(model.ContactNumber)) || !string.IsNullOrEmpty(model.ContactNumber))
                //  {
                //var regex1 = @"^\(?(?:(?:0(?:0|11)\)?[\s-]?\(?|\+)44\)?[\s-]?\(?(?:0\)?[\s-]?\(?)?|0)(?:\d{2}\)?[\s-]?\d{4}[\s-]?\d{4}|\d{3}\)?[\s-]?\d{3}[\s-]?\d{3,4}|\d{4}\)?[\s-]?(?:\d{5}|\d{3}[\s-]?\d{3})|\d{5}\)?[\s-]?\d{4,5}|8(?:00[\s-]?11[\s-]?11|45[\s-]?46[\s-]?4\d))(?:(?:[\s-]?(?:x|ext\.?\s?|\#)\d+)?)$";
                //var match = Regex.Match(model.ContactNumber, regex1, RegexOptions.IgnoreCase);
                //if (!match.Success)
                //{
                //    ModelState.AddModelError("ContactNumber", "Please enter a 11 digit phone number. The number entered is not in a valid format.");
                //}
                //  }
                if (ModelState.IsValid && model.PaymentNonce != null)
                {
                    if (braintreeEnvironmentSetting.ToLower() == "live")
                        braintreeEnvironment = Braintree.Environment.PRODUCTION;

                    var gateway = new BraintreeGateway
                    {
                        Environment = braintreeEnvironment,
                        MerchantId = braintreeMerchantId,
                        PublicKey = braintreePublicKey,
                        PrivateKey = braintreePrivateKey
                    };

                    // Update basket with some CC details
                    TicketBasket ticketBasket = _ticketingService.GetTicketBasket(this.HttpContext);
                    if (model.UseSavedCard && !string.IsNullOrEmpty(model.SelectedToken))
                    {
                        Braintree.PaymentMethod pm = gateway.PaymentMethod.Find(model.SelectedToken);
                        Braintree.CreditCard cc = (Braintree.CreditCard)pm;
                        if (cc != null && !string.IsNullOrEmpty(cc.LastFour))
                        {
                            ticketBasket.CCLastDigits = cc.LastFour;
                            ticketBasket.CCExpiryDate = cc.ExpirationMonth + @"/" + (cc.ExpirationYear.Substring(2));
                        }
                    }
                    else if (!model.UseSavedCard)
                    {
                        ticketBasket.CCLastDigits = model.creditCardDetails.CreditCardNumber.Substring(model.creditCardDetails.CreditCardNumber.Length - 4, 4);
                        ticketBasket.CCExpiryDate = ((int)model.creditCardDetails.ExpiryMonth).ToString("00") + @"/" + ((int)model.creditCardDetails.ExpiryYear).ToString("00");
                    }
                    _ticketingService.UpdateTicketBasket(ticketBasket, TicketBasketStatus.InProgress);

                    // Save user info
                    if (!model.UseSavedCard)
                        saveUser(model.ContactNumber, model.creditCardDetails);

                    BraintreeCustomer bc = _supperClubRepository.GetBraintreeCustomer(ticketBasket.UserId);
                    if (bc == null)
                    {
                        User user = _supperClubRepository.GetUser(ticketBasket.UserId);
                        var request = new CustomerRequest
                        {
                            FirstName = string.IsNullOrEmpty(user.FirstName) ? "" : user.FirstName,
                            LastName = string.IsNullOrEmpty(user.LastName) ? "" : user.LastName,
                            Email = user.Email
                        };
                        Result<Customer> result = gateway.Customer.Create(request);

                        if (result != null && result.IsSuccess() && result.Target != null)
                        {
                            string customerId = result.Target.Id;
                            BraintreeCustomer _bc = new BraintreeCustomer();
                            _bc.UserId = user.Id;
                            _bc.BraintreeCustomerId = customerId;
                            bool _createSuccess = _supperClubRepository.CreateBrainTreeCustomer(_bc);
                            if (_createSuccess)
                            {
                                LogMessage("Braintree customer added to database successfully.", LogLevel.INFO);
                                bc = _bc;
                            }
                            else
                                LogMessage("Braintree customer could not be added to database.", LogLevel.ERROR);
                        }
                    }
                    bool transactionProcessed = false;
                    Result<Transaction> payResult = null;
                    Event _event = _supperClubRepository.GetEvent(model.bookingModel.eventId);
                    model.bookingModel.EventName = _event != null ?_event.Name : "";
                    if (!model.UseSavedCard)
                    {
                        PaymentMethodRequest _request = new PaymentMethodRequest
                        {
                            CustomerId = bc.BraintreeCustomerId,
                            PaymentMethodNonce = model.PaymentNonce,
                            Options = new PaymentMethodOptionsRequest
                            {
                                //FailOnDuplicatePaymentMethod = true,
                                MakeDefault = true,
                                VerifyCard = true
                            }
                        };
                        Result<PaymentMethod> result1 = gateway.PaymentMethod.Create(_request);

                        if (result1.IsSuccess() && result1.Target != null && !string.IsNullOrEmpty(result1.Target.Token))
                        {
                            LogMessage("Payment method added successfully to Braintree server. Token=" + result1.Target.Token, LogLevel.INFO);
                            //process transaction with Token
                            payResult = gateway.Transaction.Sale(
                                    new TransactionRequest
                                    {
                                        PaymentMethodToken = result1.Target.Token,
                                        Amount = ticketBasket.TotalPrice - ticketBasket.TotalDiscount,
                                        Options = new TransactionOptionsRequest
                                        {                                            
                                            SubmitForSettlement = true
                                        },
                                        CustomFields = new Dictionary<string, string>
                                            {
                                                { "event_id", model.bookingModel.eventId.ToString() },
                                                { "event_name", model.bookingModel.EventName },
                                                { "user_email", UserMethods.CurrentUser.Email },
                                                { "booking_reference", ticketBasket.BookingReference.ToString()}
                                            }
                                    });
                            if (payResult.Target != null && (payResult.Target.Status == TransactionStatus.AUTHORIZED || payResult.Target.Status == TransactionStatus.SUBMITTED_FOR_SETTLEMENT) && payResult.IsSuccess())
                                transactionProcessed = true;
                        }
                        else
                        {
                            // false
                            CreditCardVerification verification = result1.CreditCardVerification;
                            if (verification != null)
                            {
                                LogMessage("Error adding Payment method to Braintree. verificationStatus=" + verification.Status + "  verificationProcessorResponseCode=" + verification.ProcessorResponseCode + "  verificationProcessorResponseText=" + verification.ProcessorResponseText, LogLevel.ERROR);
                                int _processorCode = 0;
                                try
                                {
                                    _processorCode = int.Parse(verification.ProcessorResponseCode);
                                }
                                catch
                                {
                                    LogMessage("Error occured while trying to convert processor code to integer" , LogLevel.ERROR);
                                    _processorCode = -1;
                                }
                                    if (_processorCode != 0 && (_processorCode >= 2000 || _processorCode == -1))
                                    {
                                        BookingModel bm = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                                        tr.Success = false;
                                        tr.TransactionMessage = "Transaction not authorized. Your bank returned the following error while processing the transaction: " + result1.Message;
                                        tr.EventId = bm.eventId;
                                        TempData["TransactionResult"] = tr;
                                        return RedirectToAction("PostPaymentFailure");
                                    }
                                
                            }
                            else
                            {
                                LogMessage("Error adding Payment method to Braintree. Message=" + result1.Message, LogLevel.ERROR);
                                BookingModel bm = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                                tr.Success = false;
                                tr.TransactionMessage = "Transaction not authorized. Your bank returned the following error while processing the transaction: " + result1.Message;
                                tr.EventId = bm.eventId;
                                TempData["TransactionResult"] = tr;
                                return RedirectToAction("PostPaymentFailure");
                                
                            }
                        }
                    }


                    if (!transactionProcessed)
                    {
                        var payRequest = bc != null ? (new TransactionRequest
                        {
                            Amount = ticketBasket.TotalPrice - ticketBasket.TotalDiscount,
                            PaymentMethodNonce = model.PaymentNonce,
                            CustomerId = bc.BraintreeCustomerId,
                            Options = new TransactionOptionsRequest
                            {
                                ThreeDSecure = new TransactionOptionsThreeDSecureRequest()
                                {
                                    Required = true
                                },
                                SubmitForSettlement = true
                            },                            
                            CustomFields = new Dictionary<string, string>
                                            {
                                                { "event_id", model.bookingModel.eventId.ToString() },
                                                { "event_name", model.bookingModel.EventName },
                                                { "user_email", UserMethods.CurrentUser.Email },
                                                { "booking_reference", ticketBasket.BookingReference.ToString()}
                                            }
                        }) : (new TransactionRequest
                        {
                            Amount = ticketBasket.TotalPrice - ticketBasket.TotalDiscount,
                            PaymentMethodNonce = model.PaymentNonce,
                            Options = new TransactionOptionsRequest
                            {
                                ThreeDSecure = new TransactionOptionsThreeDSecureRequest()
                                {
                                    Required = true
                                },
                                SubmitForSettlement = true
                            },
                            CustomFields = new Dictionary<string, string>
                                            {
                                                { "event_id", model.bookingModel.eventId.ToString() },
                                                { "event_name", model.bookingModel.EventName },
                                                { "user_email", UserMethods.CurrentUser.Email },
                                                { "booking_reference", ticketBasket.BookingReference.ToString()}
                                            }
                        });
                        payResult = gateway.Transaction.Sale(payRequest);
                    }


                    if (payResult.Target != null && (payResult.Target.Status == TransactionStatus.AUTHORIZED || payResult.Target.Status == TransactionStatus.SUBMITTED_FOR_SETTLEMENT) && payResult.IsSuccess())
                    {
                        LogMessage("Transaction authorized.", LogLevel.INFO);
                        //Log 3D secure info
                        ThreeDSecureInfo info = payResult.Target.ThreeDSecureInfo;
                        if (info == null)
                        {
                            LogMessage("The nonce was not 3D Secured", LogLevel.INFO);
                        }
                        else
                        {
                            try
                            {
                                LogMessage("Braintree 3D Secured info. info.Enrolled=" + info.Enrolled + " Status=" + info.Status + "  LiabilityShifted=" + info.LiabilityShifted.ToString() + "  LiabilityShiftPossible=" + info.LiabilityShiftPossible.ToString(), LogLevel.INFO);
                            }
                            catch
                            {
                                LogMessage("Error logging 3D secure details");
                            }

                        }
                        //TODO Add transaction logging
                        BraintreeTransaction bt = new BraintreeTransaction
                        {
                            TransactionStatus = payResult.Message == null ? (payResult.Target != null ? payResult.Target.Status.ToString() : "") : payResult.Message,
                            TransactionSuccess = payResult.IsSuccess(),
                            TransactionValidVenmoSDK = payResult.Target.CreditCard.IsVenmoSdk.Value,
                            Amount = payResult.Target.Amount,
                            AvsErrorResponseCode = payResult.Target.AvsErrorResponseCode,
                            AvsStreetAddressResponseCode = payResult.Target.AvsStreetAddressResponseCode,
                            AvsPostalCodeResponseCode = payResult.Target.AvsPostalCodeResponseCode,
                            Channel = payResult.Target.Channel,
                            TransactionCreationDate = payResult.Target.CreatedAt,
                            TransactionUpdateDate = payResult.Target.UpdatedAt,
                            CvvResponseCode = payResult.Target.CvvResponseCode,
                            TransactionId = payResult.Target.Id,
                            MerchantAccountId = payResult.Target.MerchantAccountId,
                            OrderId = payResult.Target.OrderId,
                            PlanId = payResult.Target.PlanId,
                            ProcessorAuthorizationCode = payResult.Target.ProcessorAuthorizationCode,
                            ProcessorResponseCode = payResult.Target.ProcessorResponseCode,
                            ProcessorResponseText = payResult.Target.ProcessorResponseText,
                            PurchaseOrderNumber = payResult.Target.PurchaseOrderNumber,
                            ServiceFeeAmount = payResult.Target.ServiceFeeAmount,
                            SettlementBatchId = payResult.Target.SettlementBatchId,
                            Status = payResult.Target.Status.ToString(),
                            TaxAmount = payResult.Target.TaxAmount,
                            TaxExempt = payResult.Target.TaxExempt,
                            TransactionType = payResult.Target.Type.ToString()
                        };

                        // Add Transaction
                        if (_supperClubRepository.CreateTransction(bt) != null)
                            LogMessage("Transaction added to database successfully.", LogLevel.INFO);
                        else
                            LogMessage("Transaction could not be added to database.", LogLevel.ERROR);


                        _ticketingService.UpdateTicketBasket(ticketBasket, TicketBasketStatus.Complete);

                        BookingModel bm = _ticketingService.GetBookingModelFromBasket(this.HttpContext);

                        bool success = _ticketingService.AddUserToEvent(ticketBasket, bm);
                        if (success && ticketBasket.TotalDiscount > 0)
                            _ticketingService.UpdateVoucherUsage(ticketBasket.Tickets[0].VoucherId, ticketBasket.TotalDiscount);

                        // Email - Guest: with confirmation, Host - that someone booked a ticket
                        bool hostSuccess = false;
                        if (model.bookingModel.eventId == 777)
                        {
                            //create and get voucher
                            List<string> lstvalue = Session["GiftVoucher"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            string v_code = string.Empty;
                            Voucher voucher;
                            SupperClub.Code.VoucherManager vmgr = new VoucherManager();
                            voucher = vmgr.CreateVoucher("Gift Voucher", ticketBasket.TotalPrice, 2);
                            //send email
                            if (!string.IsNullOrEmpty(voucher.Code))
                            {
                                EmailService.EmailService emailer = new EmailService.EmailService(_supperClubRepository);
                                bool _success = emailer.SendGiftVoucherEmail(lstvalue[0], lstvalue[1], DateTime.Today.AddYears(1), voucher.Code, ticketBasket.TotalPrice);

                                UserVoucherTypeDetail detail = new UserVoucherTypeDetail();
                                detail.UserId = UserMethods.CurrentUser.Id; ;
                                detail.VType = (int)(VType.GiftVoucher);
                                detail.VoucherType = (int)(VoucherType.ValueOff);
                                detail.Name = lstvalue[0];
                                detail.FriendEmailId = lstvalue[1];
                                detail.CreatedDate = DateTime.Now;
                                detail.Value = ticketBasket.TotalPrice;
                                detail.VoucherId = voucher.Id;
                                detail.BasketId = ticketBasket.Id;
                                UserVoucherTypeDetail _detail = _supperClubRepository.CreateUserVoucherTypeDetail(detail);
                            }
                        }
                        else
                        {
                            EmailService.EmailService es = new EmailService.EmailService(_supperClubRepository);
                            bool _emailSuccess = es.SendGuestBookedEmails(
                                UserMethods.CurrentUser.FirstName,
                                UserMethods.CurrentASPNETUser.Email,
                                ticketBasket.TotalTickets,
                                ticketBasket.TotalPrice - ticketBasket.TotalDiscount,
                                ticketBasket.BookingReference,
                                ticketBasket.BookingRequirements,
                                model.bookingModel.eventId,
                                model.bookingModel.seatingId,
                                model.bookingModel.bookingMenuModel,
                                model.bookingModel.voucherId,
                                ticketBasket.CCLastDigits,
                                  model.bookingModel.commission,
                                ref hostSuccess
                                );
                            if (_emailSuccess)
                                LogMessage("Guest Booking Confirmation Email: Sent successfully", LogLevel.INFO);
                            else
                                LogMessage("Guest Booking Confirmation Email: Failure sending e-mail", LogLevel.ERROR);

                            if (hostSuccess)
                                LogMessage("Host Booking Information Email: Sent successfully", SupperClub.Logger.LogLevel.INFO);
                            else
                                LogMessage("Host Booking Information Email: Failure sending e-mail", LogLevel.ERROR);
                        }
                        tr.Success = true;
                        tr.TransactionMessage = payResult.Message == null ? (payResult.Target != null ? payResult.Target.Status.ToString() : "") : payResult.Message;
                        tr.EventId = model.bookingModel.eventId;
                        TempData["TransactionResult"] = tr;
                        if (tr.EventId == 777)
                        {
                            return RedirectToAction("PostPaymentSuccess");
                        }
                        else
                        {
                            return RedirectToAction("PostPaymentSuccessNew");
                        }
                    }
                    else
                    {
                        BookingModel bm = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                        tr.Success = false;
                        tr.TransactionMessage = "Transaction not authorized." + (payResult.Message == null ? (payResult.Target != null ? payResult.Target.Status.ToString() : "") : payResult.Message);
                        tr.EventId = bm.eventId;
                        TempData["TransactionResult"] = tr;
                        return RedirectToAction("PostPaymentFailure");
                    }
                }
                else
                {
                    if (model.bookingModel.eventId != null)
                        model.Event = _supperClubRepository.GetEvent(model.bookingModel.eventId);
                    if (_ticketingService.TicketBasketAlive(this.HttpContext))
                        model.bookingModel = _ticketingService.GetBookingModelFromBasket(this.HttpContext);

                    foreach (var modelStateValue in ViewData.ModelState.Values)
                    {
                        foreach (var error in modelStateValue.Errors)
                        {
                            // Log error
                            LogMessage("Validation Failed. ErrorMessage:" + error.ErrorMessage + ". Exception:" + error.Exception, LogLevel.DEBUG);
                        }
                    }
                    return View("BrainTree/CreditCardDetails3D", model);
                }
            }
            else
                return RedirectToAction("SessionExpired");
        }
        [Authorize]
        public ActionResult GetPaymentMethodNonce(string paymentToken)
        {
            String nonce = string.Empty;
            LogMessage("GetPaymentMethodNonce. PaymentToken=" + paymentToken, LogLevel.INFO);
            if (!string.IsNullOrEmpty(paymentToken))
            {
                if (braintreeEnvironmentSetting.ToLower() == "live")
                    braintreeEnvironment = Braintree.Environment.PRODUCTION;

                var gateway = new BraintreeGateway
                {
                    Environment = braintreeEnvironment,
                    MerchantId = braintreeMerchantId,
                    PublicKey = braintreePublicKey,
                    PrivateKey = braintreePrivateKey
                };
                Result<PaymentMethodNonce> result = gateway.PaymentMethodNonce.Create(paymentToken);
                if (result != null && result.Target != null && result.Target.Nonce != null)
                {
                    nonce = result.Target.Nonce;
                    LogMessage("GetPaymentMethodNonce. Successfully generated payment nonce=" + nonce, LogLevel.INFO);
                }
            }
            return Json(nonce, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        public ActionResult DeletePaymentMethod(string paymentToken)
        {
            LogMessage("DeletePaymentMethod. PaymentToken=" + paymentToken, LogLevel.INFO);
            if (!string.IsNullOrEmpty(paymentToken))
            {
                if (braintreeEnvironmentSetting.ToLower() == "live")
                    braintreeEnvironment = Braintree.Environment.PRODUCTION;

                var gateway = new BraintreeGateway
                {
                    Environment = braintreeEnvironment,
                    MerchantId = braintreeMerchantId,
                    PublicKey = braintreePublicKey,
                    PrivateKey = braintreePrivateKey
                };
                try
                {
                    gateway.PaymentMethod.Delete(paymentToken);

                    //if (result != null && result.IsSuccess())
                    //{
                    LogMessage("DeletePaymentMethod. Successfully deleted payment method. Token=" + paymentToken, LogLevel.INFO);
                    return Json(new { Success = true, Message = "Card details deleted successfully" }, JsonRequestBehavior.AllowGet);
                    //}
                }
                catch(Exception ex)
                {
                    LogMessage("DeletePaymentMethod. Error deleting payment method. Token=" + paymentToken + " ErrorMessage=" + ex.Message + " StackTrace=" + ex.StackTrace, LogLevel.INFO);
                }
            }
            return Json(new { Success = false, Message = "Sorry, there was some error processing your request" }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region PayPal

        [Authorize]
        public ActionResult PayPalExpressCheckout()
        {
            LogMessage("Express Checkout Initiated");
            if (_ticketingService.TicketBasketAlive(this.HttpContext))
            {
                // SetExpressCheckout
                TicketBasket ticketBasket = _ticketingService.GetTicketBasket(this.HttpContext);
                SetExpressCheckoutResponse transactionResponse = _transactionService.SendPayPalSetExpressCheckoutRequest(ControllerContext.RequestContext, ticketBasket, UserMethods.CurrentUser.Email, ServerMethods.ServerUrl);
                // If Success redirect to PayPal for user to make payment
                if (transactionResponse == null || transactionResponse.ResponseStatus != PayPalMvc.Enums.ResponseType.Success)
                {
                    SetNotification(NotificationType.Error, "Sorry there was a problem with initiating a PayPal transaction. Please try again and contact an Administrator if this still doesn't work.", true);
                    LogMessage("Error initiating PayPal SetExpressCheckout transaction. Error: " + transactionResponse.ErrorToString, LogLevel.ERROR);
                    return RedirectToAction("ReviewPurchase");
                }
                return Redirect(string.Format(PayPalMvc.Configuration.Current.PayPalRedirectUrl, transactionResponse.TOKEN));
            }
            else
                return RedirectToAction("SessionExpired");
        }

        [Authorize]
        public ActionResult PayPalExpressCheckoutAuthorisedSuccess(string token, string PayerID)
        {
            LogMessage("Express Checkout Authorised");
            if (_ticketingService.TicketBasketAlive(this.HttpContext))
            {
                // GetExpressCheckoutDetails
                TempData["token"] = token;
                TempData["payerId"] = PayerID;
                GetExpressCheckoutDetailsResponse transactionResponse = _transactionService.SendPayPalGetExpressCheckoutDetailsRequest(ControllerContext.RequestContext, token);
                if (transactionResponse == null || transactionResponse.ResponseStatus != PayPalMvc.Enums.ResponseType.Success)
                {
                    SetNotification(NotificationType.Error, "Sorry there was a problem with initiating a PayPal transaction. Please try again and contact an Administrator if this still doesn't work.", true);
                    LogMessage("Error initiating PayPal GetExpressCheckoutDetails transaction. Error: " + transactionResponse.ErrorToString, LogLevel.ERROR);
                    return RedirectToAction("ReviewPurchase");
                }

                // What should we do with these details?
                return RedirectToAction("ConfirmPayPalPayment");
            }
            else
                return RedirectToAction("SessionExpired");
        }

        [Authorize]
        public ActionResult ConfirmPayPalPayment()
        {
            LogMessage("Express Checkout Confirmation");
            if (_ticketingService.TicketBasketAlive(this.HttpContext))
            {
                BookingModel model = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                return View("PayPal/ConfirmPayPalPayment", model);
            }
            else
                return RedirectToAction("SessionExpired");
        }

        [Authorize]
        [HttpPost]
        public ActionResult ConfirmPayPalPayment(bool confirmed = true)
        {
            LogMessage("Express Checkout Confirmed");
            if (_ticketingService.TicketBasketAlive(this.HttpContext))
            {
                // DoExpressCheckoutPayment
                string token = TempData["token"].ToString();
                string payerId = TempData["payerId"].ToString();
                TicketBasket ticketBasket = _ticketingService.GetTicketBasket(this.HttpContext);
                DoExpressCheckoutPaymentResponse transactionResponse = _transactionService.SendPayPalDoExpressCheckoutPaymentRequest(ControllerContext.RequestContext, ticketBasket, token, payerId);

                // Clear the session objects
                TempData["token"] = null;
                TempData["payerId"] = null;

                if (transactionResponse == null || transactionResponse.ResponseStatus != PayPalMvc.Enums.ResponseType.Success)
                {
                    if (transactionResponse != null && transactionResponse.L_ERRORCODE0 == "10486")
                    {
                        // Redirect back to PayPal in case of Error 10486 (bad funding method)
                        // https://www.x.com/developers/paypal/documentation-tools/how-to-guides/how-to-recover-funding-failure-error-code-10486-doexpresscheckout
                        LogMessage("Redirecting User back to PayPal due to 10486 error (bad funding method - typically an invalid or maxed out credit card)");
                        return Redirect(string.Format(PayPalMvc.Configuration.Current.PayPalRedirectUrl, token));
                    }
                    string errorMessage = (transactionResponse == null) ? "Null Transaction Response" : transactionResponse.ErrorToString;
                    SetNotification(NotificationType.Error, "Sorry there was a problem with taking the PayPal payment, so no money has been transferred. Please try again and contact an Administrator if this still doesn't work.", true);
                    LogMessage("Error initiating PayPal DoExpressCheckoutPayment transaction. Error: " + errorMessage, LogLevel.ERROR);
                    return RedirectToAction("ReviewPurchase");
                }

                TransactionResult tr = new TransactionResult { Success = false, TransactionMessage = "", EventId = 0 };

                if (transactionResponse.PaymentStatus == PaymentStatus.Completed)
                {
                    tr.Success = true;
                    BookingModel bm = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                    tr.EventId = bm.eventId;
                    TempData["TransactionResult"] = tr;
                    _ticketingService.UpdateTicketBasket(ticketBasket, TicketBasketStatus.Complete);
                    bool success = _ticketingService.AddUserToEvent(ticketBasket, bm);
                    if (success && ticketBasket.TotalDiscount > 0)
                        _ticketingService.UpdateVoucherUsage(ticketBasket.Tickets[0].VoucherId, ticketBasket.TotalDiscount);
                    // Email - Guest: with confirmation, Host - that someone booked a ticket
                    bool hostSuccess = false;
                    if (bm.eventId == 777)
                    {
                        //create and get voucher
                        List<string> lstvalue = Session["GiftVoucher"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        string v_code = string.Empty;
                        Voucher voucher;
                        SupperClub.Code.VoucherManager vmgr = new VoucherManager();
                        voucher = vmgr.CreateVoucher("Gift Voucher", ticketBasket.TotalPrice, 2);
                        //send email
                        if (!string.IsNullOrEmpty(voucher.Code))
                        {
                            EmailService.EmailService emailer = new EmailService.EmailService(_supperClubRepository);

                            //IEnumerable<System.Web.Security.MembershipUser> members = System.Web.Security.Membership.FindUsersByEmail(ticketBasket.Name).Cast<System.Web.Security.MembershipUser>();
                            //string username = members.First().UserName;

                            // User user = _supperClubRepository.GetUser(ticketBasket.Name);
                            //    Guid userId = user.Id;

                            bool _success = emailer.SendGiftVoucherEmail(lstvalue[0], lstvalue[1], DateTime.Today.AddYears(1), voucher.Code, ticketBasket.TotalPrice);


                            UserVoucherTypeDetail detail = new UserVoucherTypeDetail();

                            detail.UserId = UserMethods.CurrentUser.Id; ;
                            detail.VType = (int)(VType.GiftVoucher);
                            detail.VoucherType = (int)(VoucherType.ValueOff);
                            detail.Name = lstvalue[0];
                            detail.FriendEmailId = lstvalue[1];
                            detail.CreatedDate = DateTime.Now;
                            detail.Value = ticketBasket.TotalPrice;
                            detail.VoucherId = voucher.Id;
                            detail.BasketId = ticketBasket.Id;

                            UserVoucherTypeDetail _detail = _supperClubRepository.CreateUserVoucherTypeDetail(detail);

                            // return View("PostPaymentSuccessGift.cshtml");
                        }
                    }
                    else
                    {
                        EmailService.EmailService es = new EmailService.EmailService(_supperClubRepository);
                        bool _emailSuccess = es.SendGuestBookedEmails(
                            UserMethods.CurrentUser.FirstName,
                            UserMethods.CurrentASPNETUser.Email,
                            ticketBasket.TotalTickets,
                            ticketBasket.TotalPrice - ticketBasket.TotalDiscount,
                            ticketBasket.BookingReference,
                            ticketBasket.BookingRequirements,
                            bm.eventId,
                            bm.seatingId,
                            bm.bookingMenuModel,
                            bm.voucherId,
                            ticketBasket.CCLastDigits,
                            bm.commission,
                            ref hostSuccess
                            );
                        if (_emailSuccess)
                            LogMessage("Guest Booking Confirmation Email: Sent successfully", LogLevel.INFO);
                        else
                            LogMessage("Guest Booking Confirmation Email: Failure sending e-mail", LogLevel.ERROR);

                        if (hostSuccess)
                            LogMessage("Host Booking Information Email: Sent successfully", SupperClub.Logger.LogLevel.INFO);
                        else
                            LogMessage("Host Booking Information Email: Failure sending e-mail", LogLevel.ERROR);
                    }
                    return RedirectToAction("PostPaymentSuccessNew");
                }
                else
                {
                    // Something went wrong or the payment isn't complete
                    LogMessage("Error taking PayPal payment. Error: " + transactionResponse.ErrorToString + " " + transactionResponse.PaymentErrorToString, LogLevel.ERROR);
                    BookingModel bm = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                    tr.TransactionMessage = transactionResponse.PAYMENTREQUEST_0_LONGMESSAGE;
                    TempData["TransactionResult"] = tr;
                    return RedirectToAction("PostPaymentFailure");
                }
            }
            else
                return RedirectToAction("SessionExpired");
        }

        #endregion

        #region Post Payment and Cancellation
        [Authorize]
        public ActionResult PostPaymentSuccessGC()
        {
            ViewBag.Success = false;
            if (TempData["TransactionResult"] != null)
            {
                TransactionResult tr = (TransactionResult)TempData["TransactionResult"];

                if (tr.Success)
                {
                    LogMessage("Post Payment Result: Success");
                    ViewBag.Success = true;
                    // Get the successful booking data back
                    TicketBasket ticketBasket = _ticketingService.GetTicketBasket(this.HttpContext);
                    if (ticketBasket != null)
                    {
                        BookingModel bookingModel = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                        // Put this on the page for Google Conversion Tracking
                        ViewBag.SaleValue = ticketBasket.TotalPrice - ticketBasket.TotalDiscount;
                        // Google Analytics conversion tracking parameters
                        ViewBag.TransactionId = ticketBasket.Id;
                        ViewBag.TotalTickets = ticketBasket.TotalTickets;
                        ViewBag.EventId = bookingModel.eventId;
                        ViewBag.EventName = bookingModel.EventName;
                        ViewBag.SeatingId = bookingModel.seatingId;
                        ViewBag.Email = SupperClub.Code.UserMethods.CurrentUser.Email;
                        ViewBag.FirstName = SupperClub.Code.UserMethods.CurrentUser.FirstName;
                        ViewBag.LastName = SupperClub.Code.UserMethods.CurrentUser.LastName;
                        //ViewBag.UserId = SupperClub.Code.UserMethods.CurrentUser.Id;
                        ViewBag.OrderId = ticketBasket.BookingReference;
                        ViewBag.TicketUnitPrice = ticketBasket.TotalTickets == 0 ? 0 : ((ticketBasket.TotalPrice - ticketBasket.TotalDiscount) / ticketBasket.TotalTickets);
                        if (bookingModel.eventId == 777)
                        {
                            ViewBag.EventURL = ServerMethods.ServerUrl + "vouchers";
                            ViewBag.TwitterText = "Spread Grub Love! Gift your foodie friend a Grub Club Gift Voucher!";
                        }
                        else
                        {
                            Event _event = _supperClubRepository.GetEvent(bookingModel.eventId);
                            ViewBag.EventURL = ServerMethods.ServerUrl + _event.SupperClub.UrlFriendlyName + "/" + _event.UrlFriendlyName + "/" + _event.Id.ToString();
                            ViewBag.TwitterText = "This " + _event.Name + " looks amazing! Who wants to come with me?! -";
                            ViewBag.BookingReference = ticketBasket.BookingReference;
                        }

                        LogMessage("Mention me tag PostPaymentSuccess values:Email=" + ViewBag.Email + ", OrderId=" + ViewBag.OrderId + ", FirstName=" + ViewBag.FirstName + ", LastName=" + ViewBag.LastName);
                                               
                        
                        // Clear their basket
                        TicketingService.ClearBasketSession(this.HttpContext);
                    }
                    else
                    {
                        LogMessage("Post Payment Success: Could not retrieve Basket! (no cookie)", LogLevel.WARN);
                        return RedirectToAction("NoBasketCookie");
                    }
                }
                else
                    LogMessage("Post Payment Result: Success Page hit, but TransactionResult was false", LogLevel.FATAL);
            }
            else // on page refresh
            {
                LogMessage("Post Payment Success: TransactionResult was Null (possbile Post Payment page refresh)", LogLevel.WARN);
                return RedirectToAction("Index", "Home");
            }
            return View();
        }
        [Authorize]
        public ActionResult PostPaymentSuccess()
        {
            ViewBag.Success = false;
            if (TempData["TransactionResult"] != null)
            {
                TransactionResult tr = (TransactionResult)TempData["TransactionResult"];

                if (tr.Success)
                {
                    LogMessage("Post Payment Result: Success");
                    ViewBag.Success = true;
                    // Get the successful booking data back
                    TicketBasket ticketBasket = _ticketingService.GetTicketBasket(this.HttpContext);
                    if (ticketBasket != null)
                    {
                        BookingModel bookingModel = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                        // Put this on the page for Google Conversion Tracking
                        ViewBag.SaleValue = ticketBasket.TotalPrice - ticketBasket.TotalDiscount;
                        // Google Analytics conversion tracking parameters
                        ViewBag.TransactionId = ticketBasket.Id;
                        ViewBag.TotalTickets = ticketBasket.TotalTickets;
                        ViewBag.EventId = bookingModel.eventId;
                        ViewBag.EventName = bookingModel.EventName;
                        ViewBag.SeatingId = bookingModel.seatingId;
                        ViewBag.Email = SupperClub.Code.UserMethods.CurrentUser.Email;
                        ViewBag.FirstName = SupperClub.Code.UserMethods.CurrentUser.FirstName;
                        ViewBag.LastName = SupperClub.Code.UserMethods.CurrentUser.LastName;
                        //ViewBag.UserId = SupperClub.Code.UserMethods.CurrentUser.Id;
                        ViewBag.OrderId = ticketBasket.BookingReference;
                        ViewBag.TicketUnitPrice = ticketBasket.TotalTickets == 0 ? 0 : ((ticketBasket.TotalPrice - ticketBasket.TotalDiscount) / ticketBasket.TotalTickets);

                        LogMessage("Mention me tag PostPaymentSuccess values:Email=" + ViewBag.Email + ", OrderId=" + ViewBag.OrderId + ", FirstName=" + ViewBag.FirstName + ", LastName=" + ViewBag.LastName);

                        // Email - Guest: with confirmation, Host - that user has registered
                        //bool hostSuccess = false;
                        //if (bookingModel.eventId == 777)
                        //{
                        //    //create and get voucher
                        //   List<string> lstvalue = Session["GiftVoucher"].ToString().Split(new char[]{','}, StringSplitOptions.RemoveEmptyEntries).ToList();
                        //    string v_code = string.Empty;
                        //    Voucher voucher;
                        //    SupperClub.Code.VoucherManager vmgr = new VoucherManager();
                        //    voucher = vmgr.CreateVoucher("Gift Voucher", ticketBasket.TotalPrice, 2);
                        //    //send email
                        //    if (!string.IsNullOrEmpty(voucher.Code))
                        //    {
                        //    EmailService.EmailService emailer = new EmailService.EmailService(_supperClubRepository);

                        //    //IEnumerable<System.Web.Security.MembershipUser> members = System.Web.Security.Membership.FindUsersByEmail(ticketBasket.Name).Cast<System.Web.Security.MembershipUser>();
                        //    //string username = members.First().UserName;

                        //   // User user = _supperClubRepository.GetUser(ticketBasket.Name);
                        //    //    Guid userId = user.Id;

                        //        bool success = emailer.SendGiftVoucherEmail(lstvalue[0], lstvalue[1], DateTime.Today.AddYears(1), voucher.Code, ticketBasket.TotalPrice);


                        //        UserVoucherTypeDetail detail = new UserVoucherTypeDetail();

                        //        detail.UserId = UserMethods.CurrentUser.Id; ;
                        //        detail.VType = (int)(VType.GiftVoucher);
                        //        detail.VoucherType = (int)(VoucherType.ValueOff);
                        //        detail.Name = lstvalue[0];
                        //        detail.FriendEmailId = lstvalue[1];
                        //        detail.CreatedDate = DateTime.Now;
                        //        detail.Value = ticketBasket.TotalPrice;
                        //        detail.VoucherId = voucher.Id;
                        //        detail.BasketId = ticketBasket.Id;

                        //        UserVoucherTypeDetail _detail = _supperClubRepository.CreateUserVoucherTypeDetail(detail);

                        //                                     // return View("PostPaymentSuccessGift.cshtml");
                        //    }
                        //}
                        //else
                        //{
                        //    EmailService.EmailService es = new EmailService.EmailService(_supperClubRepository);
                        //    bool success = es.SendGuestBookedEmails(
                        //        UserMethods.CurrentUser.FirstName + " " + UserMethods.CurrentUser.LastName,
                        //        UserMethods.CurrentASPNETUser.Email,
                        //        ticketBasket.TotalTickets,
                        //        ticketBasket.TotalPrice - ticketBasket.TotalDiscount,
                        //        ticketBasket.BookingReference,
                        //        ticketBasket.BookingRequirements,
                        //        bookingModel.eventId,
                        //        bookingModel.seatingId,
                        //        bookingModel.bookingMenuModel,
                        //        bookingModel.voucherId,
                        //        ticketBasket.CCLastDigits,
                        //        ref hostSuccess
                        //        );
                        //    if (success)
                        //        LogMessage("Guest Booking Confirmation Email: Sent successfully", LogLevel.INFO);
                        //    else
                        //        LogMessage("Guest Booking Confirmation Email: Failure sending e-mail", LogLevel.ERROR);

                        //    if (hostSuccess)
                        //        LogMessage("Host Booking Information Email: Sent successfully", SupperClub.Logger.LogLevel.INFO);
                        //    else
                        //        LogMessage("Host Booking Information Email: Failure sending e-mail", LogLevel.ERROR);
                        //}
                        //Push notification to FB friends 
                        if (UserMethods.CurrentUser.FacebookId != null)
                        {
                            // Push Notification to user's FB Friends
                            PushNotificationService.MessageService ms = new PushNotificationService.MessageService(_supperClubRepository);
                            bool pushNotificationStatus = ms.FacebookFriendBoughtTicketNotification(UserMethods.CurrentUser.FirstName + " " + UserMethods.CurrentUser.LastName, bookingModel.EventName, bookingModel.eventId, UserMethods.CurrentUser.Id, UserMethods.CurrentUser.FacebookId);
                            if (pushNotificationStatus)
                                LogMessage("BookedTicket: Sent notification to guest's FB friends", LogLevel.INFO);
                            else
                                LogMessage("BookedTicket: Error sending notification to guest's FB friends", LogLevel.ERROR);
                        }

                        // Push Notification to Users who like this event, if number of available seats are less than 6
                        int thresholdValue = int.Parse(WebConfigurationManager.AppSettings["TicketThresholdForTriggeringNotification"]);
                        Event _event = _supperClubRepository.GetEvent(bookingModel.eventId);
                        ViewBag.EventURL = ServerMethods.ServerUrl + _event.SupperClub.UrlFriendlyName + "/" + _event.UrlFriendlyName + "/" + _event.Id.ToString();
                        if (_event != null)
                        {
                            if (_event.TotalNumberOfAvailableSeats <= thresholdValue)
                            {
                                //PushNotificationService.MessageService ms = new PushNotificationService.MessageService(_supperClubRepository);
                                //bool pushNotificationStatus = ms.FavEventBookingReminder(_event.Id, _event.Name);
                                //if (pushNotificationStatus)
                                //    LogMessage("BookedEvent: Sent notification to users about availability of tickets", LogLevel.INFO);
                                //else
                                //    LogMessage("BookedEvent: Error sending notification to users about availability of tickets", LogLevel.ERROR);

                                //Email notification
                                List<User> wishlistUsers = _supperClubRepository.GetWishListedUsers(bookingModel.eventId);
                                if (wishlistUsers != null && wishlistUsers.Count > 0)
                                {
                                    EmailService.EmailService emailService = new EmailService.EmailService(_supperClubRepository);
                                    string status = emailService.SendWishlistUsersEmails(bookingModel.eventId, wishlistUsers);
                                    LogMessage("WishListEvent: Sent notification to following users about availability of tickets. " + status, LogLevel.INFO);
                                }
                            }
                        }

                        // Clear their basket
                        TicketingService.ClearBasketSession(this.HttpContext);
                    }
                    else
                    {
                        LogMessage("Post Payment Success: Could not retrieve Basket! (no cookie)", LogLevel.WARN);
                        return RedirectToAction("NoBasketCookie");
                    }
                }
                else
                    LogMessage("Post Payment Result: Success Page hit, but TransactionResult was false", LogLevel.FATAL);
            }
            else // on page refresh
            {
                LogMessage("Post Payment Success: TransactionResult was Null (possbile Post Payment page refresh)", LogLevel.WARN);
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [Authorize]
        public ActionResult PostPaymentFailure()
        {
            ViewBag.Success = false;
            try
            {
                if (TempData["TransactionResult"] != null)
                {
                    TransactionResult tr = (TransactionResult)TempData["TransactionResult"];

                    if (!tr.Success)
                    {
                        LogMessage("Post Payment Result: Failure");
                        ViewBag.ErrorMessage = tr.TransactionMessage;
                        ViewBag.EventId = tr.EventId;
                        if (tr.EventId > 0)
                        {
                            if (_ticketingService.TicketBasketAlive(this.HttpContext))
                            {
                                // Cancel the Basket
                                TicketBasket ticketBasket = _ticketingService.GetTicketBasket(this.HttpContext);
                                _ticketingService.UpdateTicketBasket(ticketBasket, TicketBasketStatus.Cancelled);
                            }
                            _ticketingService.ClearBasket(this.HttpContext);
                        }
                        // Don't clear basket because they might want to pay with a different card (Try Again)
                    }
                    else
                        LogMessage("Post Payment Result: Failure Page hit, but TransactionResult was true", LogLevel.FATAL);
                }
                else // on page refresh
                {
                    LogMessage("Post Payment Failure: TransactionResult was Null (possbile Post Payment page refresh)", LogLevel.WARN);
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception ex)
            {
                LogMessage("Post Payment Failure Exception. Message:" + ex.Message + " StackTrace:" + ex.StackTrace, LogLevel.ERROR);
            }
            return View();
        }

        [Authorize]
        public ActionResult CancelBooking(int eventId = 0)
        {
            LogMessage("Purchase Cancelled");
            if (_ticketingService.TicketBasketAlive(this.HttpContext))
            {
                // Cancel the Basket
                TicketBasket ticketBasket = _ticketingService.GetTicketBasket(this.HttpContext);
                _ticketingService.UpdateTicketBasket(ticketBasket, TicketBasketStatus.Cancelled);

                // Get the eventId for redirect
                BookingModel bookingModel = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                eventId = bookingModel.eventId;
            }
            _ticketingService.ClearBasket(this.HttpContext);

            if (eventId == 0)
                return RedirectToAction("Index", "Home");
            else
                return RedirectToAction("Details", "Event", new { eventId = eventId });
        }

        [Authorize]
        public ActionResult SessionExpired()
        {
            LogMessage("Session Expired");
            return View();
        }
        [Authorize]
        public ActionResult UpdatedBasket()
        {
            LogMessage("User tried to book with outdated basket");
            return View();
        }
        [Authorize]
        public ActionResult NoBasketCookie()
        {
            LogMessage("Could not retrieve Basket! (no cookie)");
            return View();
        }

        [Authorize]
        public ActionResult PostPaymentSuccessNew()
        {
            // return View();

            ViewBag.Success = false;
            BookingSuccessModel bookingSuceessModel = new BookingSuccessModel();

            if (TempData["TransactionResult"] != null)
            {
                TransactionResult tr = (TransactionResult)TempData["TransactionResult"];
                BookingModel bookingModel;
                if (tr.Success)
                {
                    LogMessage("Post Payment Result: Success");
                    ViewBag.Success = true;

                    // Get the successful booking data back
                    TicketBasket ticketBasket = _ticketingService.GetTicketBasket(this.HttpContext);
                    if (ticketBasket != null)
                    {
                        bookingModel = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                        bookingSuceessModel.bookingModel = bookingModel;
                        bookingSuceessModel.BookingEvent = _supperClubRepository.GetEvent(bookingModel.eventId);
                        bookingSuceessModel.supperClubName = _supperClubRepository.GetSupperClub(bookingSuceessModel.BookingEvent.SupperClubId).Name;
                        if (bookingModel.seatingId > 0)
                        {
                            var seating = (from s in bookingSuceessModel.BookingEvent.EventSeatings
                                           where s.Id == bookingModel.seatingId
                                           select new { start = s.Start, end = s.End }).First();
                            ViewBag.StartTime = seating.start.ToShortTimeString();
                            ViewBag.EndTime = seating.end.ToShortTimeString();

                            ViewBag.CalStart = seating.start.ToString("yyyyMMdd") + "T" + seating.start.ToString("HHmmss") + "Z";
                            ViewBag.CalEnd = seating.end.ToString("yyyyMMdd") + "T" + seating.end.ToString("HHmmss") + "Z";
                                            
                          
                        }
                        // Put this on the page for Google Conversion Tracking
                        ViewBag.SaleValue = ticketBasket.TotalPrice - ticketBasket.TotalDiscount;
                        // Google Analytics conversion tracking parameters
                        ViewBag.TransactionId = ticketBasket.Id;
                        ViewBag.TotalTickets = ticketBasket.TotalTickets;
                        ViewBag.EventId = bookingModel.eventId;
                        ViewBag.EventName = bookingModel.EventName;
                        ViewBag.SeatingId = bookingModel.seatingId;
                        ViewBag.BookingRequirements = ticketBasket.BookingRequirements;
                        ViewBag.TicketUnitPrice = ticketBasket.TotalTickets == 0 ? 0 : ((ticketBasket.TotalPrice - ticketBasket.TotalDiscount) / ticketBasket.TotalTickets);
                        ViewBag.Email = SupperClub.Code.UserMethods.CurrentUser.Email;
                        ViewBag.OrderId = ticketBasket.BookingReference;
                        ViewBag.FirstName = SupperClub.Code.UserMethods.CurrentUser.FirstName;
                        ViewBag.LastName = SupperClub.Code.UserMethods.CurrentUser.LastName;
                        LogMessage("Mention me tag PostPaymentSuccessNew values:Email=" + ViewBag.Email + ", OrderId=" + ViewBag.OrderId + ", FirstName=" + ViewBag.FirstName + ", LastName=" + ViewBag.LastName);

                        //// Email - Guest: with confirmation, Host - that user has registered
                        //bool hostSuccess = false;
                        //if (bookingModel.eventId == 777)
                        //{
                        //    //create and get voucher
                        //    List<string> lstvalue = Session["GiftVoucher"].ToString().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                        //    string v_code = string.Empty;
                        //    Voucher voucher;
                        //    SupperClub.Code.VoucherManager vmgr = new VoucherManager();
                        //    voucher = vmgr.CreateVoucher("Gift Voucher", ticketBasket.TotalPrice, 2);
                        //    //send email
                        //    if (!string.IsNullOrEmpty(voucher.Code))
                        //    {
                        //        EmailService.EmailService emailer = new EmailService.EmailService(_supperClubRepository);

                        //        //IEnumerable<System.Web.Security.MembershipUser> members = System.Web.Security.Membership.FindUsersByEmail(ticketBasket.Name).Cast<System.Web.Security.MembershipUser>();
                        //        //string username = members.First().UserName;

                        //        // User user = _supperClubRepository.GetUser(ticketBasket.Name);
                        //        //    Guid userId = user.Id;

                        //        bool success = emailer.SendGiftVoucherEmail(lstvalue[0], lstvalue[1], DateTime.Today.AddYears(1), voucher.Code, ticketBasket.TotalPrice);


                        //        UserVoucherTypeDetail detail = new UserVoucherTypeDetail();

                        //        detail.UserId = UserMethods.CurrentUser.Id; ;
                        //        detail.VType = (int)(VType.GiftVoucher);
                        //        detail.VoucherType = (int)(VoucherType.ValueOff);
                        //        detail.Name = lstvalue[0];
                        //        detail.FriendEmailId = lstvalue[1];
                        //        detail.CreatedDate = DateTime.Now;
                        //        detail.Value = ticketBasket.TotalPrice;
                        //        detail.VoucherId = voucher.Id;
                        //        detail.BasketId = ticketBasket.Id;

                        //        UserVoucherTypeDetail _detail = _supperClubRepository.CreateUserVoucherTypeDetail(detail);

                        //        // return View("PostPaymentSuccessGift.cshtml");
                        //    }
                        //}
                        //else
                        //{
                        //    EmailService.EmailService es = new EmailService.EmailService(_supperClubRepository);
                        //    bool success = es.SendGuestBookedEmails(
                        //        UserMethods.CurrentUser.FirstName + " " + UserMethods.CurrentUser.LastName,
                        //        UserMethods.CurrentASPNETUser.Email,
                        //        ticketBasket.TotalTickets,
                        //        ticketBasket.TotalPrice - ticketBasket.TotalDiscount,
                        //        ticketBasket.BookingReference,
                        //        ticketBasket.BookingRequirements,
                        //        bookingModel.eventId,
                        //        bookingModel.seatingId,
                        //        bookingModel.bookingMenuModel,
                        //        bookingModel.voucherId,
                        //        ticketBasket.CCLastDigits,
                        //        ref hostSuccess
                        //        );
                        //    if (success)
                        //        LogMessage("Guest Booking Confirmation Email: Sent successfully", LogLevel.INFO);
                        //    else
                        //        LogMessage("Guest Booking Confirmation Email: Failure sending e-mail", LogLevel.ERROR);

                        //    if (hostSuccess)
                        //        LogMessage("Host Booking Information Email: Sent successfully", SupperClub.Logger.LogLevel.INFO);
                        //    else
                        //        LogMessage("Host Booking Information Email: Failure sending e-mail", LogLevel.ERROR);
                        //}
                        //Push notification to FB friends 
                        if (UserMethods.CurrentUser.FacebookId != null)
                        {
                            // Push Notification to user's FB Friends
                            PushNotificationService.MessageService ms = new PushNotificationService.MessageService(_supperClubRepository);
                            bool pushNotificationStatus = ms.FacebookFriendBoughtTicketNotification(UserMethods.CurrentUser.FirstName + " " + UserMethods.CurrentUser.LastName, bookingModel.EventName, bookingModel.eventId, UserMethods.CurrentUser.Id, UserMethods.CurrentUser.FacebookId);
                            if (pushNotificationStatus)
                                LogMessage("BookedTicket: Sent notification to guest's FB friends", LogLevel.INFO);
                            else
                                LogMessage("BookedTicket: Error sending notification to guest's FB friends", LogLevel.ERROR);
                        }

                        // Push Notification to Users who like this event, if number of available seats are less than 6
                        int thresholdValue = int.Parse(WebConfigurationManager.AppSettings["TicketThresholdForTriggeringNotification"]);
                        Event _event = _supperClubRepository.GetEvent(bookingModel.eventId);
                        ViewBag.EventURL = ServerMethods.ServerUrl + _event.SupperClub.UrlFriendlyName + "/" + _event.UrlFriendlyName + "/" + _event.Id.ToString();
                        ViewBag.TwitterText = "This " + _event.Name + " looks amazing! Who wants to come with me?! -";
                        ViewBag.BookingReference = ticketBasket.BookingReference;
                        if (_event != null)
                        {
                            if (_event.TotalNumberOfAvailableSeats <= thresholdValue && _event.TotalNumberOfAvailableSeats > 0)
                            {
                                //PushNotificationService.MessageService ms = new PushNotificationService.MessageService(_supperClubRepository);
                                //bool pushNotificationStatus = ms.FavEventBookingReminder(_event.Id, _event.Name);
                                //if (pushNotificationStatus)
                                //{
                                //    LogMessage("BookedEvent: Sent push notification to users about availability of tickets", LogLevel.INFO);
                                //    bool sts = _supperClubRepository.UpdateUsersPushNotificationForEventBookingReminder(_event.Id);
                                //    if (sts)
                                //        LogMessage("BookedEvent: Updated push notification status successfully after sending the notifications", LogLevel.INFO);
                                //    else
                                //        LogMessage("BookedEvent: Error Updating push notification status after sending the notifications", LogLevel.ERROR);
                                //}
                                //else
                                //    LogMessage("BookedEvent: Error sending notification to users about availability of tickets", LogLevel.ERROR);

                                //Email notification
                                List<User> wishlistUsers = _supperClubRepository.GetWishListedUsers(bookingModel.eventId);
                                if (wishlistUsers != null && wishlistUsers.Count > 0)
                                {
                                    EmailService.EmailService emailService = new EmailService.EmailService(_supperClubRepository);
                                    string status = emailService.SendWishlistUsersEmails(bookingModel.eventId, wishlistUsers);
                                    LogMessage("WishListEvent: Sent notification to following users about availability of tickets. " + status, LogLevel.INFO);
                                }
                            }
                        }

                        // Clear their basket
                        TicketingService.ClearBasketSession(this.HttpContext);

                    }
                    else
                    {
                        LogMessage("Post Payment Success: Could not retrieve Basket! (no cookie)", LogLevel.WARN);
                        return RedirectToAction("NoBasketCookie");
                    }
                }
                else
                    LogMessage("Post Payment Result: Success Page hit, but TransactionResult was false", LogLevel.FATAL);
            }
            else // on page refresh
            {
                LogMessage("Post Payment Success: TransactionResult was Null (possbile Post Payment page refresh)", LogLevel.WARN);
                return RedirectToAction("Index", "Home");
            }





            return View(bookingSuceessModel);


        }

        [Authorize]
        [HttpPost]
        public JsonResult SendBookingToFriends(string FriendsEmailId, string MailMessage, decimal TotalPrice, BookingModel Booking, string BookingReference)
        {
            List<string> lstEmailIds = FriendsEmailId.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            if (lstEmailIds != null && lstEmailIds.Count > 0)
            {
                TicketBasket ticketBasket = _ticketingService.GetTicketBasket(this.HttpContext);
                bool hostSuccess = false;
                EmailService.EmailService es = new EmailService.EmailService(_supperClubRepository);
                List<Subscriber> lstSubscriber = new List<Subscriber>();

                string messageToFriend = string.Format("Your friend {0} booked {1} <br/>", UserMethods.CurrentUser.FirstName + " " + UserMethods.CurrentUser.LastName,
                    Booking.EventName);
                if (!string.IsNullOrEmpty(MailMessage))
                {
                    messageToFriend = messageToFriend + "Here is the message from your friend: " + MailMessage;
                }
                foreach (string emailId in lstEmailIds)
                {
                    bool validEmail = false;
                    string _emailId = emailId.Trim();
                    // Validate the email first
                    try
                    {
                        var addr = new System.Net.Mail.MailAddress(_emailId);
                        validEmail = true;
                    }
                    catch
                    {
                        validEmail = false;
                    }
                    if (validEmail)
                    {
                        bool success = es.SendGuestBookedEmailsToFriends(MailMessage,
                            UserMethods.CurrentUser.FirstName,
                            // UserMethods.CurrentASPNETUser.Email,
                           _emailId,
                            Booking.numberOfTickets,
                            TotalPrice,
                            int.Parse(BookingReference),
                            Booking.bookingRequirements,
                            Booking.eventId,                            
                            Booking.seatingId,
                            Booking.bookingMenuModel,
                            0,
                           "XXXX",
                           Booking.commission,
                            ref hostSuccess
                            );

                        BookingConfirmationToFriends confirmationdetails = new BookingConfirmationToFriends();
                        confirmationdetails.EventId = Booking.eventId;
                        confirmationdetails.FriendsMailIds = _emailId;
                        confirmationdetails.Message = MailMessage;
                        confirmationdetails.UserId = UserMethods.CurrentUser.Id;
                        confirmationdetails.CreatedDate = DateTime.Now;
                        _supperClubRepository.CreateBookingConfiramtionToFriends(confirmationdetails);

                        Subscriber _subscriber = new Subscriber();
                        _subscriber.FirstName = string.Empty;
                        _subscriber.LastName = string.Empty;
                        _subscriber.EmailAddress = _emailId;
                        _subscriber.SubscriberType = (int)SupperClub.Domain.SubscriberType.GuestInvitee;
                        lstSubscriber.Add(_subscriber);

                        bool _success = _supperClubRepository.AddGuestEmailInfo(UserMethods.CurrentUser.Id, lstSubscriber);
                        if (!_success)
                        {
                            LogMessage("SendBookingToFriends: Could not save Guest Details to Database. Friends Emails:" + _emailId, LogLevel.ERROR);
                        }
                        else
                        {
                            LogMessage("SendBookingToFriends: Saved Guest Details to Database. Friends Emails:" + _emailId, LogLevel.INFO);
                        }
                    }
                    else
                    {
                        LogMessage("SendBookingToFriends: Could not send booking to Friends. Invalid Email Id. Friends Emails:" + _emailId, LogLevel.ERROR);
                        return Json(false, JsonRequestBehavior.AllowGet);
                    }
                }
                Segment.Analytics.Client.Track(_segmentMethod.GetSegmentUser(), "Shared booking with friends", new Segment.Model.Properties() {
                    { "EmailAddressOfFriends", FriendsEmailId  },
                    { "EmailMessage", MailMessage },
                    { "GuestEmail", UserMethods.CurrentUser.Email },
                    { "GuestFirstName", UserMethods.CurrentUser.FirstName },
                    { "GuestLastName", UserMethods.CurrentUser.LastName },
                    { "EventName", Booking.EventName},
                    { "EventDate", Booking.EventDateAndTime},
                    { "BookingReference", BookingReference },
                    { "NumberOfTicketsBooked", Booking.numberOfTickets},
                    { "TotalPrice", TotalPrice }                    
                });   
            }

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Private lists

        // TODO: Move to Repository
        private List<SelectListItem> GetCardTypes(string defaultValue)
        {
            // Values come from SagePay list: VISA, MC, DELTA, MAESTRO, UKE, AMEX, DC, JCB, LASER, PAYPAL
            List<SelectListItem> items = new List<SelectListItem>();
            items.Add(new SelectListItem { Text = "Mastercard", Value = "MC", Selected = (defaultValue == "MS") });
            items.Add(new SelectListItem { Text = "Visa", Value = "VISA", Selected = (defaultValue == "VS") });
            items.Add(new SelectListItem { Text = "Maestro", Value = "MAESTRO", Selected = (defaultValue == "MAESTRO") });
            //items.Add(new SelectListItem { Text = "American Express", Value = "AMEX", Selected = (defaultValue == "AE") });
            return items;
        }

        private List<SelectListItem> GetAvailableTickets(int numberAvailable, int numberAllocated, int min, int incrementOffeset)
        {
            List<SelectListItem> items = new List<SelectListItem>();
            for (int i = min; i <= numberAvailable; i += incrementOffeset)
                items.Add(new SelectListItem { Text = i.ToString(), Value = i.ToString(), Selected = (numberAvailable == numberAllocated) });
            return items;
        }

        private CreditCardDetailsModel GetTestCreditCardModel(CreditCardType creditCardType)
        {
            CreditCardSample sample = CreditCardSample.GetCardSample(creditCardType);
            return new CreditCardDetailsModel
            {
                CardType = sample.CardType,
                CreditCardNumber = sample.CardNumber,
                ExpiryMonth = 01,
                ExpiryYear = 15,
                CV2 = sample.CV2,
                CardHolder = "Mr P Herman",

                Address1 = sample.Address,
                Address2 = "Camden",
                City = "London",
                PostCode = sample.PostCode,
                Country = sample.Country
            };
        }
        private CreditCardDetailsBraintreeModel GetBraintreeTestCreditCardModel()
        {
            CreditCardSample sample = CreditCardSample.GetCardSample(CreditCardType.NONE);
            return new CreditCardDetailsBraintreeModel
            {
                //CardType = sample.CardType,
                CreditCardNumber = "4000000000000002",
                ExpiryMonth = 01,
                ExpiryYear = 20,
                CV2 = 123,
                CardHolder = "Mr P Herman",

                Address1 = sample.Address,
                Address2 = "Camden",
                City = "London",
                PostCode = sample.PostCode,
                Country = sample.Country
            };
        }

        #endregion

        #region Private Converters

        private SagePayMvc.CreditCard ConvertCreditCardModel(CreditCardDetailsModel creditCardModel)
        {
            if (creditCardModel != null)
            {
                try
                {
                    // Convert the Credit Card View Model to a SagePay Credit Card
                    SagePayMvc.CreditCard creditCard = new SagePayMvc.CreditCard
                    {
                        CardNumber = creditCardModel.CreditCardNumber,
                        CardType = creditCardModel.CardType,
                        ExpiryDate = ((int)creditCardModel.ExpiryMonth).ToString("00") + ((int)creditCardModel.ExpiryYear).ToString("00"),
                        CardHolder = creditCardModel.CardHolder,
                        CV2 = ((int)creditCardModel.CV2).ToString("000"),

                    };
                    if (creditCardModel.StartMonth != null)
                    {
                        creditCard.StartDate = ((int)creditCardModel.StartMonth).ToString("00") + ((int)creditCardModel.StartYear).ToString("00");
                        creditCard.IssueNumber = ((int)creditCardModel.IssueNumber).ToString("00");
                    }
                    return creditCard;
                }
                catch (Exception ex)
                {
                    LogMessage("Error Converting Credit Card View Model to a SagePay Credit Card. Error Message:" + ex.Message + "  Stack Trace:" + ex.StackTrace, LogLevel.ERROR);
                    return null;
                }
            }
            else
                return null;
        }

        private SagePayMvc.Address ConvertAddressModel(CreditCardDetailsModel creditCardModel)
        {
            // Convert the Credit Card View Billing Address to a SagePay Address
            SagePayMvc.Address creditCardAddress = new SagePayMvc.Address
            {
                Firstnames = string.IsNullOrEmpty(UserMethods.CurrentUser.FirstName) ? "first" : UserMethods.CurrentUser.FirstName,
                Surname = string.IsNullOrEmpty(UserMethods.CurrentUser.LastName) ? "surname" : UserMethods.CurrentUser.LastName,
                Address1 = creditCardModel.Address1,
                Address2 = creditCardModel.Address2,
                City = creditCardModel.City,
                Country = "GB",
                PostCode = creditCardModel.PostCode
            };
            return creditCardAddress;
        }

        #endregion

        #region Private Methods
        private Dictionary<int, DateTime> createEventBookingList(int eventId)
        {
            Dictionary<int, DateTime> _evList = new Dictionary<int, DateTime>();
            _evList.Add(eventId, DateTime.Now);
            return _evList;
        }
        private Dictionary<int, DateTime> updateEventBookingList(int eventId, Dictionary<int, DateTime> evList)
        {

            if (evList != null)
            {
                evList = cleanUpEventBookingList(evList);
                if (evList.ContainsKey(eventId))
                    evList[eventId] = DateTime.Now;
                else
                    evList.Add(eventId, DateTime.Now);
            }
            else
                evList = createEventBookingList(eventId);
            return evList;
        }
        private Dictionary<int, DateTime> cleanUpEventBookingList(Dictionary<int, DateTime> evList)
        {
            foreach (KeyValuePair<int, DateTime> ev in evList)
            {
                if (ev.Value.AddMonths(_eventBookingCookieExpirationInMonths) < DateTime.Now)
                    evList.Remove(ev.Key);
            }
            return evList;
        }
        private string serializeCookieData(Dictionary<int, DateTime> evList)
        {
            string json = JsonConvert.SerializeObject(evList, Formatting.Indented);
            return json;
        }
        private Dictionary<int, DateTime> deserializeCookieData(string evListJson)
        {
            Dictionary<int, DateTime> evList = JsonConvert.DeserializeObject<Dictionary<int, DateTime>>(evListJson);
            return evList;
        }
        private void saveUser(string phoneNumber, CreditCardDetailsModel ccModel)
        {
            LogMessage("Saving user's info");
            bool success = false;
            Guid currentUser = SupperClub.Code.UserMethods.CurrentUser.Id;
            if (!string.IsNullOrEmpty(phoneNumber) && phoneNumber.Length > 0)
            {
                phoneNumber = phoneNumber.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("+", "").Replace("-", "").Replace(".", "");
                phoneNumber = phoneNumber.Substring(phoneNumber.Length - 10);
                double number = 0;
                bool saveNumber = false;
                if (double.TryParse(phoneNumber, out number))
                    saveNumber = true;
                if (saveNumber)
                {
                    User _user = _supperClubRepository.GetUserNoTracking(currentUser);
                    if (_user != null)
                    {
                        _user.ContactNumber = phoneNumber;
                        _user.Address = ccModel.Address1 + (string.IsNullOrEmpty(ccModel.Address2) ? "" : ", " + ccModel.Address2) + ", " + ccModel.City;
                        _user.Country = ccModel.Country;
                        _user.PostCode = ccModel.PostCode;
                        success = _supperClubRepository.UpdateUser(_user);
                        if (!success)
                        {
                            LogMessage("SaveUserinfo: Could not save user info in Database. UserId: " + currentUser.ToString() + " Contact Number: " + phoneNumber, LogLevel.ERROR);
                        }
                        else
                        {
                            LogMessage("User's info saved successfully.");
                            User _updatedUser = _supperClubRepository.GetUser(currentUser);
                            System.Web.HttpContext.Current.Session["User"] = _updatedUser;
                        }
                    }
                }
            }
        }
        private void saveUser(string phoneNumber, CreditCardDetailsBraintreeModel ccModel)
        {
            LogMessage("Saving user's info");
            bool success = false;
            Guid currentUser = SupperClub.Code.UserMethods.CurrentUser.Id;
            if (!string.IsNullOrEmpty(phoneNumber) && phoneNumber.Length > 0)
            {
                phoneNumber = phoneNumber.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("+", "").Replace("-", "").Replace(".", "");
                phoneNumber = phoneNumber.Substring(phoneNumber.Length - 10);
                double number = 0;
                bool saveNumber = false;
                if (double.TryParse(phoneNumber, out number))
                    saveNumber = true;
                if (saveNumber)
                {
                    User _user = _supperClubRepository.GetUserNoTracking(currentUser);
                    if (_user != null)
                    {
                        _user.ContactNumber = phoneNumber;
                        _user.Address = ccModel.Address1 + (string.IsNullOrEmpty(ccModel.Address2) ? "" : ", " + ccModel.Address2) + ", " + ccModel.City;
                        _user.Country = ccModel.Country;
                        _user.PostCode = ccModel.PostCode;
                        success = _supperClubRepository.UpdateUser(_user);
                        if (!success)
                        {
                            LogMessage("SaveUserinfo: Could not save user info in Database. UserId: " + currentUser.ToString() + " Contact Number: " + phoneNumber, LogLevel.ERROR);
                        }
                        else
                        {
                            LogMessage("User's info saved successfully.");
                            User _updatedUser = _supperClubRepository.GetUser(currentUser);
                            System.Web.HttpContext.Current.Session["User"] = _updatedUser;
                        }
                    }
                }
            }
        }
        #endregion

    }
}
