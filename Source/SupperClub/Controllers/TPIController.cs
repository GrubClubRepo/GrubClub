using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SupperClub.Domain.Repository;
using SupperClub.Domain;
using System.Web.Security;
using SupperClub.Code;
using SupperClub.Models;
using System.Web.Configuration;
using SupperClub.Web.Helpers;
using SupperClub.Services;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using SupperClub.Logger;
using System.Web.Script.Serialization;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Braintree;
using Braintree.Exceptions;
using System.Net.Mail;
using System.Net.Sockets;
using System.Web.UI;

namespace SupperClub.Controllers
{
    [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
    public class TPIController : BaseController
    {
        private TpiTicketingService _ticketingService;
        private ITransactionService _transactionService;
        private CreditCardType _testCardType;
        private string braintreeMerchantId = WebConfigurationManager.AppSettings["BraintreeMerchantId"];
        private string braintreePublicKey = WebConfigurationManager.AppSettings["BraintreePublicKey"];
        private string braintreePrivateKey = WebConfigurationManager.AppSettings["BraintreePrivateKey"];
        private string braintreeEnvironmentSetting = WebConfigurationManager.AppSettings["BraintreeEnvironment"]; //could be sandbox or live
        private Braintree.Environment braintreeEnvironment = Braintree.Environment.SANDBOX;
        public SupperClub.Code.SimplerAES sa = new SupperClub.Code.SimplerAES();

        public TPIController(ISupperClubRepository supperClubRepository, ITransactionService transactionService)
        {
            _supperClubRepository = supperClubRepository;
            _transactionService = transactionService;
            _ticketingService = new TpiTicketingService(_supperClubRepository);
            _testCardType = (CreditCardType)int.Parse(WebConfigurationManager.AppSettings["TestCreditCard"]);
        }

        public ActionResult ShowEvents(int? sid = null, bool returnUrl = false)
        {
            TPIModel _tpiModel = new TPIModel();
            _tpiModel.EventList = new List<Event>();
            try
            {
                if (sid != null && sid > 0)
                {
                    LogMessage("TPI ShowEvents: " + sid.ToString());

                    IList<Event> _events= _supperClubRepository.GetFutureEventsForASupperClub((int)sid);
                    LogMessage("TPI ShowEvents: List fetched successfully.", LogLevel.DEBUG);
                    if (_events == null)
                    {
                        LogMessage("API ShowEvents - No Grub Clubs exist for SupperClub Id: " + sid.ToString(), LogLevel.ERROR);
                    }
                    if (_events.Count > 0)
                    {
                        foreach (Event e in _events)
                        {
                            if(e.MultiSeating)
                            {
                                int totalTicketsInProgress = 0;
                                foreach (EventSeating es in e.EventSeatings)
                                {
                                    int ticketsInProgress = _supperClubRepository.GetNumberTicketsInProgressForEvent(e.Id, es.Id);
                                    es.AvailableSeats = es.AvailableSeats - ticketsInProgress;
                                    totalTicketsInProgress += ticketsInProgress;
                                }
                                e.TPIAvailableSeats = e.TotalNumberOfAvailableSeats - totalTicketsInProgress;
                            }
                            else
                            {
                                int ticketsInProgress = _supperClubRepository.GetNumberTicketsInProgressForEvent(e.Id);
                                e.TPIAvailableSeats = e.TotalNumberOfAvailableSeats - ticketsInProgress;
                            }
                            _tpiModel.EventList.Add(e);                       
                        }
                    }
                    
                   
                }                
            }
            catch (Exception ex)
            {
                LogMessage("TPI ShowEvents: Error getting event list. ErrorMessage:" + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                //return Json(new { error = "Error occurred while getting Grub Club details." }, JsonRequestBehavior.AllowGet);
            }
            return View("ShowEvents", _tpiModel);
        }

        public ActionResult ChooseTickets(int eid = 0, int stId = 0)
        {
            TPIChooseTicketsModel model = new TPIChooseTicketsModel();
            if(eid == 0)
            {
                //Handle to show a message
                return View();
            }
            LogMessage("TPI Choose Tickets" + eid.ToString() + " UserAgent:" + Request.UserAgent + " Browser:" + Request.Browser.Browser + "  BrowserVersion:" + Request.Browser.Version);
            Event _event = _supperClubRepository.GetEvent(eid);
            List<TPIBookingMenuModel> lstBookingMenuModel = new List<TPIBookingMenuModel>();
            int menuMinTicket = 0;
            if (_event.MultiMenuOption)
            {
                var lstEmo = _event.EventMenuOptions.OrderByDescending(x => x.IsDefault);
                foreach (EventMenuOption emo in lstEmo)
                {
                    TPIBookingMenuModel _bookingMenuModel = new TPIBookingMenuModel();
                    _bookingMenuModel.menuOptionId = emo.Id;
                    _bookingMenuModel.baseTicketCost = emo.Cost;
                    _bookingMenuModel.menuTitle = emo.Title;
                    _bookingMenuModel.numberOfTickets = emo.IsDefault ? (_event.MinMaxBookingEnabled ? _event.MinTicketsAllowed : 1) : 0;
                    _bookingMenuModel.discount = 0;
                    lstBookingMenuModel.Add(_bookingMenuModel);
                }
            }
            List<TPIBookingSeatingModel> lstBookingSeatingModel = new List<TPIBookingSeatingModel>();
            if (_event.MultiSeating)
            {
                foreach (EventSeating es in _event.EventSeatings)
                {
                    if (es.AvailableSeats > 0)
                    {
                        TPIBookingSeatingModel _bookingSeatingModel = new TPIBookingSeatingModel();
                        _bookingSeatingModel.seatingId = es.Id;
                        _bookingSeatingModel.start = es.Start;
                        _bookingSeatingModel.availableSeats = es.AvailableSeats - _supperClubRepository.GetNumberTicketsInProgressForEvent(_event.Id, es.Id);
                        lstBookingSeatingModel.Add(_bookingSeatingModel);
                    }                    
                }
            }
            int numTickets = 1;
            if (_event.MinMaxBookingEnabled)
                numTickets = _event.MinTicketsAllowed;

            TPIBookingModel bm = new TPIBookingModel
            {
                eventId = eid,
                EventName = _event.Name,
                numberOfTickets = menuMinTicket > 0 ? menuMinTicket : numTickets,
                currency = "GBP",
                baseTicketCost = _event.Cost,
                totalTicketCost = _event.CostToGuest,
                totalDue = numTickets * _event.CostToGuest,
                discount = 0,
                voucherId = 0,
                voucherDescription = "",
                IsContactNumberRequired = _event.ContactNumberRequired,
                seatingId = stId,
                bookingMenuModel = lstBookingMenuModel,
                bookingSeatingModel = lstBookingSeatingModel,
                contactNumber = UserMethods.IsLoggedIn ? SupperClub.Code.UserMethods.CurrentUser.ContactNumber : "",
                updateContactNumber = false,
                MinMaxBookingEnabled = _event.MinMaxBookingEnabled,
                MinTicketsAllowed = _event.MinTicketsAllowed,
                MaxTicketsAllowed = _event.MaxTicketsAllowed,
                SeatSelectionInMultipleOfMin = _event.SeatSelectionInMultipleOfMin,
                ToggleMenuSelection = _event.ToggleMenuSelection,
                commission = _event.Commission
            };
            
            // creating a basket
            if (!_event.MultiSeating || (_event.MultiSeating && bm.seatingId > 0))
                _ticketingService.CreateTicketBasket(this.HttpContext);
            if (_event.MultiSeating && bm.seatingId > 0)
            {
                if (_ticketingService.GetCurrentAvailableTicketsForEvent(bm.eventId, bm.seatingId) <= 0)
                {
                    foreach (TPIBookingSeatingModel bsm in bm.bookingSeatingModel)
                    {
                        if (_ticketingService.GetCurrentAvailableTicketsForEvent(bm.eventId, bsm.seatingId) > 0)
                        {
                            bm.seatingId = bsm.seatingId;
                            break;
                        }
                    }
                }
            }
            // TODO to add logic to handle min tickets > 1
            if ((_event.MultiSeating && bm.seatingId == 0) || !_ticketingService.AddTicketsToBasket(bm, true))
            {
                // Wasn't able to add a ticket (none available)
                SetNotification(NotificationType.Error, "Sorry there are no tickets available!", true);
                return RedirectToAction("showevents", "tpi", new { sid = _event.SupperClubId });
            }
            model.BookingModel = bm;
            model.Event = _event;
            int maxTickets = int.Parse(System.Configuration.ConfigurationManager.AppSettings["TpiMaxTicketsAllowed"]);
            if (model.BookingModel.MinMaxBookingEnabled)
                maxTickets = model.BookingModel.MaxTicketsAllowed;

            int numberAvailableTickets = _ticketingService.GetCurrentAvailableTicketsForEvent(model.BookingModel.eventId, model.BookingModel.seatingId) + model.BookingModel.numberOfTickets;
            if (numberAvailableTickets > maxTickets) // Set a maximum in the drop down
                numberAvailableTickets = maxTickets;
            else if (numberAvailableTickets == 0) // No more tickets, only show what the current user has allocated
                numberAvailableTickets = model.BookingModel.numberOfTickets;

            model.NumberOfTicketsAvailable = numberAvailableTickets;
            ViewBag.selectedSeatingId = bm.seatingId;
                
            return View(model);            
        }

        [HttpPost]
        public ActionResult ChooseTickets(TPIChooseTicketsModel model)
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
                return RedirectToAction("LoginRegister", "tpi", new { returnUrl = (UserMethods.IsLoggedIn ? "" : Url.Action("CreditCardDetailsBraintree", "booking")) });
            }
            else
                return RedirectToAction("SessionExpired");
        }

        public JsonResult UpdateTickets(int numberTicketsRequested, int seatingId, string strBmm)
        {

            UpdateTicketsResult updateTicketsResult = new UpdateTicketsResult();
            if (_ticketingService.TicketBasketAlive(this.HttpContext))
            {
                TicketBasket tb = _ticketingService.GetTicketBasket(this.HttpContext);
                LogMessage("Basket alive: Updating Tickets");
                // Service should return object as below
                TPIBookingModel bookingModel = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                bookingModel.seatingId = seatingId;

                int currentBlockedTicketsForUser = _supperClubRepository.GetNumberTicketsInProgressForUser(bookingModel.eventId, seatingId, tb.UserId);
                int maxTickets = int.Parse(System.Configuration.ConfigurationManager.AppSettings["TpiMaxTicketsAllowed"]);
                if (bookingModel.MinMaxBookingEnabled)
                    maxTickets = bookingModel.MaxTicketsAllowed;
                // In case of manual request
                if (numberTicketsRequested > maxTickets)
                    numberTicketsRequested = maxTickets;

                LogMessage(string.Format("Updating Tickets - EventID: {0}, User Id: {3}, Seating Id: {1}, Number of tickets: {2}", bookingModel.eventId, bookingModel.seatingId, bookingModel.numberOfTickets, tb.UserId));

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
        #region Login Register
        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        public ActionResult LoginRegister(string returnUrl)
        {
            if (UserMethods.IsLoggedIn)
            {
                ViewBag.locationUrl = SupperClub.Code.ServerMethods.ServerUrl + "booking/CreditCardDetailsBrainTree";
                ViewBag.returnUrl = returnUrl;
            }
            else
            {
                ViewBag.locationUrl = SupperClub.Code.ServerMethods.ServerUrl + "tpi/TpiCreditCardDetailsNewUser?returnUrl=";
                ViewBag.returnUrl = returnUrl;
            }
            return View();
        }
        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        public ActionResult TpiCreditCardDetailsNewUser(string returnUrl)
        {
            LogMessage("TPI Login Registration Page");
            LoginRegisterModel lrm = new LoginRegisterModel();
            if (!string.IsNullOrEmpty(returnUrl))
                lrm.RedirectURL = returnUrl;
            lrm.LogOnModel = new LogOnModel();
            lrm.RegisterModel = new RegisterModel();
            lrm.ForgotPasswordModel = new ForgotPasswordModel();
            
            return View("TpiCreditCardDetailsNewUser", lrm);
        }
        #endregion
        #region Credit Card Methods
        [Authorize]
        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        public ActionResult TpiCreditCardDetailsBraintree()
        {
            LogMessage("Credit Card Details");
            if (_ticketingService.TicketBasketAlive(this.HttpContext))
            {
                TPIBookingModel tpiBookingModel = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                BookingModel bookingModel = new BookingModel();
                if (tpiBookingModel != null)
                {
                    bookingModel.baseTicketCost = tpiBookingModel.baseTicketCost;
                    if (tpiBookingModel.bookingMenuModel != null)
                    {
                        foreach (TPIBookingMenuModel bmm in tpiBookingModel.bookingMenuModel)
                        {
                            BookingMenuModel bm_bmm = new BookingMenuModel();
                            bm_bmm.baseTicketCost = bmm.baseTicketCost;
                            bm_bmm.discount = bmm.discount;
                            bm_bmm.menuOptionId = bmm.menuOptionId;
                            bm_bmm.menuTitle = bmm.menuTitle;
                            bm_bmm.numberOfTickets = bmm.numberOfTickets;
                            bookingModel.bookingMenuModel.Add(bm_bmm);
                        }
                    }
                    if (tpiBookingModel.bookingSeatingModel != null)
                    {
                        foreach (TPIBookingSeatingModel bsm in tpiBookingModel.bookingSeatingModel)
                        {
                            BookingSeatingModel bs = new BookingSeatingModel();
                            bs.seatingId = bsm.seatingId;
                            bs.start = bsm.start;
                            bookingModel.bookingSeatingModel.Add(bs);
                        }
                    }
                    bookingModel.bookingRequirements = tpiBookingModel.bookingRequirements;
                    bookingModel.commission = tpiBookingModel.commission;
                    bookingModel.contactNumber = tpiBookingModel.contactNumber;
                    bookingModel.currency = tpiBookingModel.currency;
                    bookingModel.discount = tpiBookingModel.discount;
                    bookingModel.EventDateAndTime = tpiBookingModel.EventDateAndTime;
                    bookingModel.eventId = tpiBookingModel.eventId;
                    bookingModel.EventName = tpiBookingModel.EventName;
                    bookingModel.IsContactNumberRequired = tpiBookingModel.IsContactNumberRequired;
                    bookingModel.MaxTicketsAllowed = tpiBookingModel.MaxTicketsAllowed;
                    bookingModel.MinMaxBookingEnabled = tpiBookingModel.MinMaxBookingEnabled;
                    bookingModel.MinTicketsAllowed = tpiBookingModel.MinTicketsAllowed;
                    bookingModel.numberOfTickets = tpiBookingModel.numberOfTickets;
                    bookingModel.seatingId = tpiBookingModel.seatingId;
                    bookingModel.SeatSelectionInMultipleOfMin = tpiBookingModel.SeatSelectionInMultipleOfMin;
                    bookingModel.ToggleMenuSelection = tpiBookingModel.ToggleMenuSelection;
                    bookingModel.totalDue = tpiBookingModel.totalDue;
                    bookingModel.totalTicketCost = tpiBookingModel.totalTicketCost;
                    bookingModel.updateContactNumber = tpiBookingModel.updateContactNumber;
                    bookingModel.voucherDescription = tpiBookingModel.voucherDescription;
                    bookingModel.voucherId = tpiBookingModel.voucherId;                    
                }
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
        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        [HttpPost]
        public ActionResult TpiCreditCardDetailsBraintree(CreditCardDetailsBraintreeViewModel model)
        {
            LogMessage("Credit Card Details Submitted");
            TransactionResult tr = new TransactionResult();
            TicketingService ticketingService = new TicketingService(_supperClubRepository);                
             
            if (ticketingService.TicketBasketAlive(this.HttpContext))
            {
                if (!model.UseSavedCard)
                {
                    if (model.creditCardDetails.ExpiryYear + 2000 < DateTime.Now.Year)
                        ModelState.AddModelError("creditCardDetails_ExpiryYear", "This card has expired");

                    if (model.bookingModel.IsContactNumberRequired && string.IsNullOrEmpty(model.ContactNumber))
                        ModelState.AddModelError("ContactNumber", "You must provide a phone number to continue booking!");
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
                    ticketingService.UpdateTicketBasket(ticketBasket, TicketBasketStatus.InProgress);

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
                    model.bookingModel.EventName = _event != null ? _event.Name : "";
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
                                    BookingModel bm = ticketingService.GetBookingModelFromBasket(this.HttpContext);
                                    tr.Success = false;
                                    tr.TransactionMessage = "Transaction not authorized. Your bank returned the following error while processing the transaction: " + result1.Message;
                                    tr.EventId = bm.eventId;
                                    TempData["TransactionResult"] = tr;
                                    return RedirectToAction("PostPaymentFailure","booking");
                                }

                            }
                            else
                            {
                                LogMessage("Error adding Payment method to Braintree. Message=" + result1.Message, LogLevel.ERROR);
                                BookingModel bm = ticketingService.GetBookingModelFromBasket(this.HttpContext);
                                tr.Success = false;
                                tr.TransactionMessage = "Transaction not authorized. Your bank returned the following error while processing the transaction: " + result1.Message;
                                tr.EventId = bm.eventId;
                                TempData["TransactionResult"] = tr;
                                return RedirectToAction("PostPaymentFailure","booking");
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


                        ticketingService.UpdateTicketBasket(ticketBasket, TicketBasketStatus.Complete);

                        BookingModel bm = ticketingService.GetBookingModelFromBasket(this.HttpContext);

                        bool success = ticketingService.AddUserToEvent(ticketBasket, bm);
                        if (success && ticketBasket.TotalDiscount > 0)
                            ticketingService.UpdateVoucherUsage(ticketBasket.Tickets[0].VoucherId, ticketBasket.TotalDiscount);

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
                            return RedirectToAction("PostPaymentSuccessGC","booking");
                        }
                        else
                        {
                            return RedirectToAction("PostPaymentSuccessNew","booking");
                        }
                    }
                    else
                    {
                        BookingModel bm = ticketingService.GetBookingModelFromBasket(this.HttpContext);
                        tr.Success = false;
                        tr.TransactionMessage = "Transaction not authorized." + (payResult.Message == null ? (payResult.Target != null ? payResult.Target.Status.ToString() : "") : payResult.Message);
                        tr.EventId = bm.eventId;
                        TempData["TransactionResult"] = tr;
                        // Log error
                        LogMessage(tr.TransactionMessage, LogLevel.ERROR);
                        return RedirectToAction("PostPaymentFailure","booking");
                    }
                }
                else
                {
                    if (model.bookingModel.eventId != null)
                        model.Event = _supperClubRepository.GetEvent(model.bookingModel.eventId);
                    if (ticketingService.TicketBasketAlive(this.HttpContext))
                        model.bookingModel = ticketingService.GetBookingModelFromBasket(this.HttpContext);

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
        #endregion

        #region Free Tickets

        [Authorize]
        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        public ActionResult FreeBooking()
        {
            LogMessage("Free Booking");
            if (_ticketingService.TicketBasketAlive(this.HttpContext))
            {
                TPIBookingModel tpiBookingModel = _ticketingService.GetBookingModelFromBasket(this.HttpContext);
                BookingModel bookingModel = new BookingModel();
                if (tpiBookingModel != null)
                {
                    bookingModel.baseTicketCost = tpiBookingModel.baseTicketCost;
                    if (tpiBookingModel.bookingMenuModel != null)
                    {
                        foreach (TPIBookingMenuModel bmm in tpiBookingModel.bookingMenuModel)
                        {
                            BookingMenuModel bm_bmm = new BookingMenuModel();
                            bm_bmm.baseTicketCost = bmm.baseTicketCost;
                            bm_bmm.discount = bmm.discount;
                            bm_bmm.menuOptionId = bmm.menuOptionId;
                            bm_bmm.menuTitle = bmm.menuTitle;
                            bm_bmm.numberOfTickets = bmm.numberOfTickets;
                            bookingModel.bookingMenuModel.Add(bm_bmm);
                        }
                    }
                    if (tpiBookingModel.bookingSeatingModel != null)
                    {
                        foreach (TPIBookingSeatingModel bsm in tpiBookingModel.bookingSeatingModel)
                        {
                            BookingSeatingModel bs = new BookingSeatingModel();
                            bs.seatingId = bsm.seatingId;
                            bs.start = bsm.start;
                            bookingModel.bookingSeatingModel.Add(bs);
                        }
                    }
                    bookingModel.bookingRequirements = tpiBookingModel.bookingRequirements;
                    bookingModel.commission = tpiBookingModel.commission;
                    bookingModel.contactNumber = tpiBookingModel.contactNumber;
                    bookingModel.currency = tpiBookingModel.currency;
                    bookingModel.discount = tpiBookingModel.discount;
                    bookingModel.EventDateAndTime = tpiBookingModel.EventDateAndTime;
                    bookingModel.eventId = tpiBookingModel.eventId;
                    bookingModel.EventName = tpiBookingModel.EventName;
                    bookingModel.IsContactNumberRequired = tpiBookingModel.IsContactNumberRequired;
                    bookingModel.MaxTicketsAllowed = tpiBookingModel.MaxTicketsAllowed;
                    bookingModel.MinMaxBookingEnabled = tpiBookingModel.MinMaxBookingEnabled;
                    bookingModel.MinTicketsAllowed = tpiBookingModel.MinTicketsAllowed;
                    bookingModel.numberOfTickets = tpiBookingModel.numberOfTickets;
                    bookingModel.seatingId = tpiBookingModel.seatingId;
                    bookingModel.SeatSelectionInMultipleOfMin = tpiBookingModel.SeatSelectionInMultipleOfMin;
                    bookingModel.ToggleMenuSelection = tpiBookingModel.ToggleMenuSelection;
                    bookingModel.totalDue = tpiBookingModel.totalDue;
                    bookingModel.totalTicketCost = tpiBookingModel.totalTicketCost;
                    bookingModel.updateContactNumber = tpiBookingModel.updateContactNumber;
                    bookingModel.voucherDescription = tpiBookingModel.voucherDescription;
                    bookingModel.voucherId = tpiBookingModel.voucherId;
                }
                TransactionResult tr = new TransactionResult { Success = false, TransactionMessage = "", EventId = 0 };
                BookingModel bm = bookingModel;
                TicketBasket ticketBasket = _ticketingService.GetTicketBasket(this.HttpContext);
                if ((ticketBasket.TotalPrice - ticketBasket.TotalDiscount) <= 0)
                {
                    tr.Success = true;
                    tr.EventId = bm.eventId;
                    TempData["TransactionResult"] = tr;
                    _ticketingService.UpdateTicketBasket(ticketBasket, TicketBasketStatus.Complete);
                    bool success = _ticketingService.AddUserToEvent(ticketBasket, tpiBookingModel);
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
                    return RedirectToAction("PostPaymentSuccessNew","booking");
                }
                else
                {
                    tr.Success = false;
                    tr.EventId = bm.eventId;
                    tr.TransactionMessage = "It doesn't appear that your event is actually free.";
                    TempData["TransactionResult"] = tr;
                    return RedirectToAction("PostPaymentFailure", "booking");
                }
            }
            else
                return RedirectToAction("SessionExpired");
        }

        #endregion

        #region Private Methods        
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
    }    
}
