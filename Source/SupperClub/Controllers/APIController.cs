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


namespace SupperClub.Controllers
{
    [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
    public class APIController : BaseController
    {
        private string braintreeMerchantId = WebConfigurationManager.AppSettings["BraintreeMerchantId"];
        private string braintreePublicKey = WebConfigurationManager.AppSettings["BraintreePublicKey"];
        private string braintreePrivateKey = WebConfigurationManager.AppSettings["BraintreePrivateKey"];
        private string braintreeEnvironmentSetting = WebConfigurationManager.AppSettings["BraintreeEnvironment"]; //could be sandbox or live
        private Braintree.Environment braintreeEnvironment = Braintree.Environment.SANDBOX;
        public SupperClub.Code.SimplerAES sa = new SupperClub.Code.SimplerAES();

        public APIController(ISupperClubRepository supperClubRepository)
        {
            _supperClubRepository = supperClubRepository;
        }

        public JsonResult getEventDetails(int? eventId = null, string userId = null)
        {
            Guid? _userId = null; 
            if(!string.IsNullOrEmpty(userId)) 
                _userId = new Guid(sa.Decrypt(userId));
            string userid = (_userId == null ? "" : "UserId=" + _userId.ToString() + ". ");
            try
            {
                if (eventId != null && eventId > 0)
                {
                    LogMessage(userid + "API getEventDetails: " + eventId.ToString());

                    Event _event = _supperClubRepository.GetEvent((int)eventId);
                    if (_event == null)
                    {
                        LogMessage(userid + "API getEventDetails - Event did not exist for Id: " + eventId.ToString(), LogLevel.ERROR);
                        return Json(new { error = "Request failed: Event does not exist for Id: " + eventId.ToString() }, JsonRequestBehavior.AllowGet);
                    }
                    List<string> jsonImages = new List<string>();
                    if (_event.EventImages != null && _event.EventImages.Count > 0)
                    {
                        foreach (EventImage ei in _event.EventImages)
                            jsonImages.Add(ServerMethods.ServerUrl + ServerMethods.EventImagePath.Replace("~/", "") + ei.ImagePath);
                    }
                    else
                    {
                        jsonImages.Add(ServerMethods.ServerUrl + ServerMethods.EventImagePath.Replace("~/", "") + _event.ImagePath);
                    }
                    IList<MultiSeatingDetails> jsonSeating = new List<MultiSeatingDetails>();
                    if (_event.MultiSeating)
                    {
                        foreach (EventSeating es in _event.EventSeatings)
                        {
                            jsonSeating.Add(new MultiSeatingDetails
                            {
                                SeatingId = es.Id,
                                SeatingStartTime = es.Start,
                                SeatingEndTime = es.End,
                                SeatingGuests = es.Guests,
                                SeatingAvailableSeats = es.AvailableSeats
                            });
                        }
                    }
                    IList<MultiMenuDetails> jsonMenuOption = new List<MultiMenuDetails>();
                    if (_event.MultiMenuOption)
                    {
                        foreach (EventMenuOption em in _event.EventMenuOptions)
                        {
                            jsonMenuOption.Add(new MultiMenuDetails
                            {
                                MenuOptionId = em.Id,
                                MenuTitle = em.Title.Replace("\u0027", "'"),
                                MenuDescription = em.Description.Replace("\u0027", "'"),
                                MenuOptionCost = em.CostToGuest
                            });
                        }
                    }
                    
                    // Return Event Reviews if event was in past
                    IList<EventReview> jsonReviewList = new List<EventReview>();
                    if (_event.Start < DateTime.Now)
                    {
                        if (_event.Reviews.Count > 0)
                        {
                            List<Review> reviews = (from r in _event.Reviews
                                                    where r.Publish == true
                                                    select r).ToList <Review>();
                            if (reviews != null && reviews.Count > 0)
                            {
                                foreach (Review rv in reviews)
                                {
                                    jsonReviewList.Add(new EventReview
                                    {
                                        ReviewId = rv.Id,
                                        Anonymous = rv.Anonymous,
                                        GuestName = (rv.User != null ? rv.User.FirstName + " " + rv.User.LastName : "Guest"),                                        
                                        Rating = (int)rv.Rating,
                                        Title = rv.Title,
                                        PublicReview = rv.PublicReview,                                        
                                        ReviewCreateDate = rv.DateCreated,
                                        HostResponse = rv.HostResponse,
                                        HostResponseDate = rv.HostResponseDate,
                                        AdminResponse = rv.AdminResponse,
                                        AdminResponseDate = rv.AdminResponseDate
                                    });
                                }
                            }
                        }
                    }

                    // Return Guest List if current user is host of event
                    IList<Guest> jsonGuestList = new List<Guest>();
                    if (userid != null && _userId == _event.SupperClub.UserId)
                    {
                        foreach (EventAttendee ea in _event.EventAttendees)
                        {
                            var br = (from t in ea.User.Tickets
                                     join tb in ea.User.TicketBaskets
                                     on t.BasketId equals tb.Id
                                      where t.EventId == ea.EventId && t.UserId == ea.UserId && t.SeatingId == ea.SeatingId //&& t.MenuOptionId == ea.MenuOptionId
                                          select new { bookingReference = tb.BookingReference, bookingRequirements = tb.BookingRequirements, checkedIn = tb.CheckedIn, checkInDate = tb.CheckInDate }).Distinct().ToList();
                            int temp = 0;
                            string requirementSummary = "";
                            bool checkInStatus = false;
                            DateTime? checkInDateTime = null;
                            if(br != null && br.Count > 0)
                            {
                                temp = br[0].bookingReference;
                                //Fix for check-in date and status
                                checkInStatus = br[0].checkedIn;
                                checkInDateTime = br[0].checkInDate;
                                foreach (var b in br)
                                {
                                    if (b.bookingReference > temp)
                                    {
                                        temp = b.bookingReference;
                                        checkInStatus = b.checkedIn;
                                        checkInDateTime = b.checkInDate;
                                    }
                                    if (!string.IsNullOrEmpty(b.bookingRequirements))
                                        requirementSummary += (requirementSummary.Length > 0 ? ", " + b.bookingRequirements : b.bookingRequirements);
                                }
                                jsonGuestList.Add(new Guest
                                {
                                    BookingReference = temp > 0 ? temp.ToString() : "",
                                    GuestUserId = sa.Encrypt(ea.UserId.ToString()),
                                    GuestName = ea.User.FirstName + " " + ea.User.LastName,
                                    GuestEmail = ea.User.Email,
                                    EventSeatingId = ea.SeatingId,
                                    EventMenuOptionId = ea.MenuOptionId,
                                    NumberOfTickets = ea.NumberOfGuests,
                                    SpecialRequirements = requirementSummary,
                                    CheckInStatus = checkInStatus,
                                    CheckInDate = checkInDateTime
                                });
                            }                            
                        }
                    }
                    bool isAddedToWaitList = false;
                    if (_userId != null && _event.TotalNumberOfAvailableSeats <= 0)
                    {
                        isAddedToWaitList = _supperClubRepository.IsUserAddedToWaitList((Guid)_userId, (int)eventId);
                    }

                    // GrubClub images
                    List<string> jsonGCImages = new List<string>();
                    if (_event.SupperClub.SupperClubImages != null && _event.SupperClub.SupperClubImages.Count > 0)
                    {
                        foreach (SupperClubImage si in _event.SupperClub.SupperClubImages)
                            jsonGCImages.Add(ServerMethods.ServerUrl + ServerMethods.SupperClubImagePath.Replace("~/", "") + si.ImagePath);
                    }
                    else
                    {
                        jsonGCImages.Add(ServerMethods.ServerUrl + ServerMethods.SupperClubImagePath.Replace("~/", "") + _event.SupperClub.ImagePath);
                    }

                    var serializer = new JavaScriptSerializer();
                    LogMessage(userid + "API getEventDetails: Details fetched successfully.", LogLevel.DEBUG);
                    EventDetails ed = new EventDetails
                    {
                        EventId = _event.Id,
                        EventName = _event.Name.Replace("\u0027", "'"),
                        EventStart = _event.Start,
                        EventEnd = _event.End,
                        EventDescription = _event.EventDescription.Replace("\u0027", "'"),
                        EventURL = ServerMethods.ServerUrl + _event.UrlFriendlyName + "/" + _event.Id.ToString(),
                        BaseCost = _event.Cost,
                        Cost = _event.CostToGuest,
                        EventMinMaxBookingEnabled = _event.MinMaxBookingEnabled,
                        EventMinTicketsAllowed = _event.MinTicketsAllowed,
                        EventMaxTicketsAllowed = _event.MaxTicketsAllowed,
                        EventSeatSelectionInMultipleOfMinEnabled = _event.SeatSelectionInMultipleOfMin,
                        EventToggleMenuSelectionEnabled = _event.ToggleMenuSelection,
                        EventAddress = _event.Address + (string.IsNullOrEmpty(_event.Address2) ? "" : ", " + _event.Address2),
                        EventCity = _event.City,
                        EventPostCode = _event.PostCode,
                        EventLatitude = _event.Latitude,
                        EventLongitude = _event.Longitude,
                        GrubClubId = _event.SupperClub.Id,
                        GrubClubName = _event.SupperClub.Name.Replace("\u0027", "'"),
                        GrubClubDescription = string.IsNullOrEmpty(_event.SupperClub.Description) ? (string.IsNullOrEmpty(_event.SupperClub.Summary) ? "" : _event.SupperClub.Summary) : _event.SupperClub.Description.Replace("\u0027", "'"),
                        GrubClubImage = jsonGCImages,
                        GrubClubURL = ServerMethods.ServerUrl + _event.SupperClub.UrlFriendlyName,
                        TotalSeats = _event.TotalEventGuests,
                        AvailableSeats = _event.TotalNumberOfAvailableSeats,
                        EventImage = jsonImages,
                        ContactNumberRequired = _event.ContactNumberRequired,
                        MultiSeating = _event.MultiSeating,
                        EventSeatingDetails = jsonSeating,
                        MultiMenu = _event.MultiMenuOption,
                        EventMenuDetails = jsonMenuOption,
                        Alcohol = _event.Alcohol,
                        Charity = _event.Charity,
                        EventMenu = _event.Menu,
                        EventDiet = _event.Diets,
                        EventCuisine = _event.Cuisines,
                        NumberOfVotes = _event.NumberGuestRanks,
                        AverageRating = _event.AverageRank,
                        EventReviews = jsonReviewList,
                        MinSeatingTime = _event.MinSeatingTime,
                        UserAddedToWaitList = isAddedToWaitList,
                        EventGuestList = jsonGuestList
                    };
                    return Json(ed, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogMessage(userid + "Request failed: Event Id not passed", LogLevel.ERROR);
                    return Json(new { error = "Request failed: Event Id not passed. Please pass Event Id as part of request." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage(userid + "API getEventDetails: Error returning results. ErrorMessage:" + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                return Json(new { error = "Error occurred while getting event details." }, JsonRequestBehavior.AllowGet);
            }
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        [HttpPost]
        public JsonResult addEventsToFavourite(string userId, int[] eventIds = null)
        {
            Guid? _userId = null;
            if (!string.IsNullOrEmpty(userId))
                _userId = new Guid(sa.Decrypt(userId));
            string userid = (_userId == null ? "" : "UserId=" + _userId.ToString() + ". ");
            try
            {
                if (_userId == null)
                {
                    LogMessage(userid + "API addEventsToFavourite: UserId is not passed.", LogLevel.ERROR);
                    return Json(new { success = false, message = "Request failed: UserId not passed." }, JsonRequestBehavior.AllowGet);
                }
                if (!_supperClubRepository.IsValidUser((Guid)_userId))
                {
                    LogMessage(userid + "API addEventsToFavourite: Invalid UserId. UserId=" + _userId.ToString() , LogLevel.ERROR);
                    return Json(new { success = false, message = "Request failed: Invalid UserId" }, JsonRequestBehavior.AllowGet);
                }
                if (eventIds == null)
                {
                    LogMessage(userid + "API removeEventsFromFavourite: EventIds not passed.", LogLevel.ERROR);
                    return Json(new { success = false, message = "Request failed: Event Ids not passed." }, JsonRequestBehavior.AllowGet);
                }
                LogMessage(userid + "API addEventsToFavourite: Adding Events to user's favourite list.");
                List<UserFavouriteEvent> lufe = new List<UserFavouriteEvent>();
                foreach (int eventId in eventIds)
                {
                    UserFavouriteEvent ufe = new UserFavouriteEvent();
                    ufe.EventId = (int)eventId;
                    ufe.UserId = (Guid)_userId;
                    lufe.Add(ufe);
                }
                bool status = _supperClubRepository.AddEventsToFavourite(lufe);
                if (status)
                {
                    LogMessage(userid + "API addEventsToFavourite: Added events to user's favourites.");
                    return Json(new { success = true, message = "Added events to user's favourite event list." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogMessage(userid + "API addEventsToFavourite: Failed to add events to user's favourite event list.");
                    return Json(new { success = false, message = "Failed to add events to user's favourite event list." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage(userid + "API addEventsToFavourite: Error while adding events to user's favourites. Error Details:" + ex.Message + "  StackTrace:" + ex.StackTrace);
                return Json(new { success = false, message = "Error occurred while adding events to user's favourite event list." }, JsonRequestBehavior.AllowGet);
            }
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        [HttpPost]
        public JsonResult removeEventsFromFavourite(string userId, int[] eventIds = null)
        {
            Guid? _userId = null;
            if (!string.IsNullOrEmpty(userId))
                _userId = new Guid(sa.Decrypt(userId));
            string userid = (_userId == null ? "" : "UserId=" + _userId.ToString() + ". ");
            try
            {
                if (_userId == null)
                {
                    LogMessage(userid + "API removeEventsFromFavourite: UserId is not passed.", LogLevel.ERROR);
                    return Json(new { success = false, message = "Request failed: UserId not passed." }, JsonRequestBehavior.AllowGet);
                }
                if (!_supperClubRepository.IsValidUser((Guid)_userId))
                {
                    LogMessage(userid + "API removeEventsFromFavourite: Invalid UserId. UserId=" + _userId.ToString(), LogLevel.ERROR);
                    return Json(new { success = false, message = "Request failed: Invalid UserId" }, JsonRequestBehavior.AllowGet);
                }
                if (eventIds == null)
                {
                    LogMessage(userid + "API removeEventsFromFavourite: EventId is not passed.", LogLevel.ERROR);
                    return Json(new { success = false, message = "Request failed: Event Ids not passed." }, JsonRequestBehavior.AllowGet);
                }
                LogMessage(userid + "API removeEventsFromFavourite: Removing Event from user's favourite list.");

                bool status = _supperClubRepository.RemoveEventsFromFavourite(eventIds, (Guid)_userId);
                if (status)
                {
                    LogMessage(userid + "API removeEventsFromFavourite: Removed event from user's favourites.");
                    return Json(new { success = true, message = "Removed event from user's favourite event list." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogMessage(userid + "API removeEventsFromFavourite: Failed to remove event from user's favourite event list.");
                    return Json(new { success = false, message = "Failed to remove event from user's favourite event list." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage(userid + "API removeEventsFromFavourite: Error while removing event from user's favourites. Error Details:" + ex.Message + "  StackTrace:" + ex.StackTrace);
                return Json(new { success = false, message = "Error occurred while removing event from user's favourite event list." }, JsonRequestBehavior.AllowGet);
            }
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        [HttpPost]
        public JsonResult addGrubclubsToFavourite(string userId, int[] grubClubIds = null)
        {
            Guid? _userId = null;
            if (!string.IsNullOrEmpty(userId))
                _userId = new Guid(sa.Decrypt(userId));
            string userid = (_userId == null ? "" : "UserId=" + _userId.ToString() + ". ");
            try
            {
                if (_userId == null)
                {
                    LogMessage(userid + "API addGrubclubsToFavourite: UserId is not passed.", LogLevel.ERROR);
                    return Json(new { success = false, message = "Request failed: UserId not passed." }, JsonRequestBehavior.AllowGet);
                }
                if (!_supperClubRepository.IsValidUser((Guid)_userId))
                {
                    LogMessage(userid + "API addGrubclubsToFavourite: Invalid UserId. UserId=" + _userId.ToString(), LogLevel.ERROR);
                    return Json(new { success = false, message = "Request failed: Invalid UserId" }, JsonRequestBehavior.AllowGet);
                }
                if (grubClubIds == null)
                {
                    LogMessage(userid + "API addGrubclubsToFavourite: EventId is not passed.", LogLevel.ERROR);
                    return Json(new { success = false, message = "Request failed: Grub Club Ids not passed." }, JsonRequestBehavior.AllowGet);
                }
                LogMessage(userid + "API addGrubclubsToFavourite: Adding Grub Clubs to user's favourite list.");
                List<UserFavouriteSupperClub> lufs = new List<UserFavouriteSupperClub>();
                foreach(int grubClubId in grubClubIds)
                {
                    UserFavouriteSupperClub ufs = new UserFavouriteSupperClub();
                    ufs.SupperClubId = (int)grubClubId;
                    ufs.UserId = (Guid)_userId;
                    lufs.Add(ufs);
                }
                bool status = _supperClubRepository.AddSupperClubsToFavourite(lufs);
                if (status)
                {
                    LogMessage(userid + "API addGrubclubsToFavourite: Added Grub Clubs to user's favourites.");
                    return Json(new { success = true, message = "Added Grub Clubs to user's favourite Grub Club list." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogMessage(userid + "API addGrubclubsToFavourite: Failed to add Grub Clubs to user's favourite Grub Club list.");
                    return Json(new { success = false, message = "Failed to add Grub Clubs to user's favourite Grub Club list." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage(userid + "API addGrubclubsToFavourite: Error while adding Grub Clubs to user's favourites. Error Details:" + ex.Message + "  StackTrace:" + ex.StackTrace);
                return Json(new { success = false, message = "Error occurred while adding Grub Clubs to user's favourite Grub Club list." }, JsonRequestBehavior.AllowGet);
            }
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        [HttpPost]
        public JsonResult removeGrubClubsFromFavourite(string userId, int[] grubClubIds = null)
        {
            Guid? _userId = null;
            if (!string.IsNullOrEmpty(userId))
                _userId = new Guid(sa.Decrypt(userId));
            string userid = (_userId == null ? "" : "UserId=" + _userId.ToString() + ". ");
            try
            {
                if (_userId == null)
                {
                    LogMessage(userid + "API removeGrubClubsFromFavourite: UserId is not passed.", LogLevel.ERROR);
                    return Json(new { success = false, message = "Request failed: UserId not passed." }, JsonRequestBehavior.AllowGet);
                }
                if (!_supperClubRepository.IsValidUser((Guid)_userId))
                {
                    LogMessage(userid + "API removeGrubClubsFromFavourite: Invalid UserId. UserId=" + _userId.ToString(), LogLevel.ERROR);
                    return Json(new { success = false, message = "Request failed: Invalid UserId" }, JsonRequestBehavior.AllowGet);
                }
                if (grubClubIds == null)
                {
                    LogMessage(userid + "API removeGrubClubsFromFavourite: GrubClubIds not passed.", LogLevel.ERROR);
                    return Json(new { success = false, message = "Request failed: Grub Club Ids not passed." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogMessage(userid + "API removeGrubClubsFromFavourite: Removing GrubClubs from user's favourite list.");

                    bool status = _supperClubRepository.RemoveSupperClubsFromFavourite(grubClubIds, (Guid)_userId);
                    if (status)
                    {
                        LogMessage(userid + "API removeGrubClubsFromFavourite: Removed Grub Clubs from user's favourites.");
                        return Json(new { success = true, message = "Removed Grub Clubs from user's favourite Grub Club list." }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        LogMessage(userid + "API removeGrubClubsFromFavourite: Failed to remove Grub Clubs from user's favourite Grub Club list.");
                        return Json(new { success = false, message = "Failed to remove Grub Clubs from user's favourite Grub Club list." }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage(userid + "API removeGrubClubsFromFavourite: Error while removing Grub Clubs from user's favourites. Error Details:" + ex.Message + "  StackTrace:" + ex.StackTrace);
                return Json(new { success = false, message = "Error occurred while removing Grub Clubs from user's favourite Grub Club list." }, JsonRequestBehavior.AllowGet);
            }
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        public JsonResult addToWaitList(int? eventId, string userId)
        {
            Guid? _userId = null;
            if (!string.IsNullOrEmpty(userId))
                _userId = new Guid(sa.Decrypt(userId)); ;
            string userid = (_userId == null ? "" : "UserId=" + _userId.ToString() + ". ");
            try
            {
                if (eventId == null || eventId == 0 || _userId == null)
                {
                    LogMessage(userid + "API addToWaitList: EventId or UserId is not passed.", LogLevel.ERROR);
                    return Json(new { success = false, message = "Request failed: Event Id or UserId not passed." }, JsonRequestBehavior.AllowGet); 
                }
                else
                {
                    if (!_supperClubRepository.IsValidUser((Guid)_userId))
                    {
                        LogMessage(userid + "API addToWaitList: Invalid UserId. UserId=" + _userId.ToString(), LogLevel.ERROR);
                        return Json(new { success = false, message = "Request failed: Invalid UserId" }, JsonRequestBehavior.AllowGet);
                    }
                    LogMessage(userid + "API addToWaitList: Adding User to Event Wait List. Event Id:" + eventId.ToString() + "  UserId:" + _userId.ToString());

                    User _user = _supperClubRepository.GetUser(_userId);
                    SupperClub.Domain.EventWaitList ewl = new EventWaitList();
                    ewl.UserId = _userId;
                    ewl.Email = _user.Email;
                    ewl.EventId = (int)eventId;
                    int wlStatus = _supperClubRepository.AddToWaitList(ewl);
                    if (wlStatus > 0)
                    {
                        LogMessage(userid + "API addToWaitList: Added User to Event Wait List. Event Id:" + eventId.ToString() + "  UserId:" + _userId.ToString());
                        return Json(new { success = true, message = "Added user to Event Waiting List" }, JsonRequestBehavior.AllowGet);
                    }
                    else                        
                    {
                        LogMessage(userid + "API addToWaitList: Failed to add user to Event Wait List. Event Id:" + eventId.ToString() + "  UserId:" + _userId.ToString());
                        return Json(new { success = false, message = "Failed to add user to Event Waiting List" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage(userid + "API addToWaitList: Error while adding user to Event Wait List. Event Id: " + eventId + "  UserId:" + _userId.ToString() + "  Error Details:" + ex.Message + "  StackTrace:" + ex.StackTrace);
                return Json(new { success = false, message = "Error occurred while adding user to Event Waiting List" }, JsonRequestBehavior.AllowGet);
            }            
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        public JsonResult removeFromWaitList(int? eventId, string userId)
        {
            Guid? _userId = null;
            if (!string.IsNullOrEmpty(userId))
                _userId = new Guid(sa.Decrypt(userId));
            string userid = (_userId == null ? "" : "UserId=" + _userId.ToString() + ". ");
            try
            {
                if (eventId == null || eventId == 0 || _userId == null)
                {
                    LogMessage(userid + "API removeFromWaitList: EventId or UserId is not passed.", LogLevel.ERROR);
                    return Json(new { success = false, message = "Request failed: Event Id or UserId not passed." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (!_supperClubRepository.IsValidUser((Guid)_userId))
                    {
                        LogMessage(userid + "API removeFromWaitList: Invalid UserId. UserId=" + _userId.ToString(), LogLevel.ERROR);
                        return Json(new { success = false, message = "Request failed: Invalid UserId" }, JsonRequestBehavior.AllowGet);
                    }
                    LogMessage(userid + "API removeFromWaitList: Removing User from Event Wait List. Event Id:" + eventId.ToString() + "  UserId:" + _userId.ToString());

                    bool wlStatus = _supperClubRepository.RemoveFromWaitList((int)eventId, (Guid)_userId);
                    if (wlStatus)
                    {
                        LogMessage(userid + "API removeFromWaitList: Removed User from Event Waiting List. Event Id:" + eventId.ToString() + "  UserId:" + _userId.ToString());
                        return Json(new { success = wlStatus, message = "Removed user from Event Waiting List" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        LogMessage(userid + "API removeFromWaitList: Failed to removing user from Event Waiting List. Event Id:" + eventId.ToString() + "  UserId:" + _userId.ToString());
                        return Json(new { success = wlStatus, message = "Failed to remove user from Event Waiting List" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage(userid + "API removeFromWaitList: Error while removing user from Event Waiting List. Event Id: " + eventId + "  UserId:" + _userId.ToString() + "  Error Details:" + ex.Message + "  StackTrace:" + ex.StackTrace);
                return Json(new { success = false, message = "Error while removing user from Event Waiting List" }, JsonRequestBehavior.AllowGet);
            }
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        public JsonResult getMyEvents(string userId)
        {
            Guid? _userId = null;
            if (!string.IsNullOrEmpty(userId))
                _userId = new Guid(sa.Decrypt(userId));
            string userid = (_userId == null ? "" : "UserId=" + _userId.ToString() + ". ");
            try
            {
                if (_userId == null)
                {
                    LogMessage(userid + "API getMyEvents: UserId is not passed.", LogLevel.ERROR);
                    return Json(new { error = "Request failed: UserId not passed." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (!_supperClubRepository.IsValidUser((Guid)_userId))
                    {
                        LogMessage(userid + "API getMyEvents: Invalid UserId. UserId=" + _userId.ToString(), LogLevel.ERROR);
                        return Json(new { error = "Request failed: Invalid UserId" }, JsonRequestBehavior.AllowGet);
                    }
                    LogMessage(userid + "API getMyEvents: Getting User's booked event list.");
                    var pastEvents = _supperClubRepository.GetUserPastEvents((Guid)_userId);
                    var futureEvents = _supperClubRepository.GetUserFutureEvents((Guid)_userId);

                    MyBookedEventsWithMetaData eventList = new MyBookedEventsWithMetaData();
                    eventList.PastEvents = pastEvents;
                    eventList.FutureEvents = futureEvents;

                    foreach (MyBookedEvent el in eventList.PastEvents)
                    {
                        Event _event = _supperClubRepository.GetEvent(el.EventId);
                        el.EventRating = _event.AverageRank;
                        el.EventImage = ServerMethods.ServerUrl + ServerMethods.EventImagePath.Replace("~/", "") + el.EventImage;
                        el.EventURL = ServerMethods.ServerUrl + el.EventURL + "/" + el.EventId.ToString();
                        el.EventName = el.EventName.Replace("\u0027", "'");
                    }

                    foreach (MyBookedEvent el in eventList.FutureEvents)
                    {
                        el.EventImage = ServerMethods.ServerUrl + ServerMethods.EventImagePath.Replace("~/", "") + el.EventImage;
                        el.EventURL = ServerMethods.ServerUrl + el.EventURL + "/" + el.EventId.ToString();
                        el.EventName = el.EventName.Replace("\u0027", "'");
                    }
                    LogMessage(userid + "API getMyEvents: Results returned successfully.", LogLevel.DEBUG);
                    return Json(eventList, JsonRequestBehavior.AllowGet);                    
                }
            }
            catch (Exception ex)
            {
                LogMessage(userid + "API getMyEvents: Error getting user's booked events" + "  UserId:" + _userId.ToString() + "  Error Details:" + ex.Message + "  StackTrace:" + ex.StackTrace);
                return Json(new { error = "Error occurred while getting user's booked event" }, JsonRequestBehavior.AllowGet);
            }
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        public JsonResult getMyWaitlistedEvents(string userId)
        {
            Guid? _userId = null;
            if (!string.IsNullOrEmpty(userId))
                _userId = new Guid(sa.Decrypt(userId));
            string userid = (_userId == null ? "" : "UserId=" + _userId.ToString() + ". ");
            try
            {
                if (_userId == null)
                {
                    LogMessage(userid + "API getMyWaitlistedEvents: UserId is not passed.", LogLevel.ERROR);
                    return Json(new { error = "Request failed: UserId not passed." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (!_supperClubRepository.IsValidUser((Guid)_userId))
                    {
                        LogMessage(userid + "API getMyWaitlistedEvents: Invalid UserId. UserId=" + _userId.ToString(), LogLevel.ERROR);
                        return Json(new { error = "Request failed: Invalid UserId" }, JsonRequestBehavior.AllowGet);
                    }
                    LogMessage(userid + "API getMyWaitlistedEvents: Getting User's waitlisted events.");
                    var waitListEvents = _supperClubRepository.GetWaitlistedEventsForAUser((Guid)_userId);

                    MyWaitlistedEventsWithMetaData eventList = new MyWaitlistedEventsWithMetaData();
                    IList<MyWaitlistedEvent> myWaitListedEvents = new List<MyWaitlistedEvent>();

                    foreach (Event el in waitListEvents)
                    {
                        MyWaitlistedEvent mwle = new MyWaitlistedEvent();
                        mwle.EventId = el.Id;
                        mwle.EventName = el.Name.Replace("\u0027", "'");
                        mwle.EventImage = ServerMethods.ServerUrl + ServerMethods.EventImagePath.Replace("~/", "") + el.ImagePath; 
                        mwle.EventURL = ServerMethods.ServerUrl + el.UrlFriendlyName + "/" + el.Id.ToString();
                        mwle.Cost = el.CostToGuest;
                        mwle.BaseCost = el.Cost;
                        mwle.EventStart = el.Start;
                        mwle.EventEnd = el.End;
                        mwle.EventAddress = el.Address + (string.IsNullOrEmpty(el.Address2) ? "" : " " + el.Address2);
                        mwle.EventCity = el.City;
                        mwle.EventPostCode = el.PostCode;
                        mwle.EventLatitude = el.Latitude;
                        mwle.EventLongitude = el.Longitude;
                        string esd = Regex.Replace(el.EventDescription, "<.+?>", string.Empty);
                        mwle.EventShortDescription = esd.Substring(0, esd.Length > 120 ? 120 : esd.Length);
                        mwle.EventDescription = esd;
                        myWaitListedEvents.Add(mwle);
                    }
                    eventList.WaitListedEvents = myWaitListedEvents;
                    LogMessage(userid + "API getMyWaitlistedEvents: Results returned successfully.", LogLevel.DEBUG);
                    return Json(eventList, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage(userid + "API getMyWaitlistedEvents: Error getting user's waitlisted events" + "  UserId:" + _userId.ToString() + "  Error Details:" + ex.Message + "  StackTrace:" + ex.StackTrace);
                return Json(new { error = "Error occurred while getting user's waitlisted event" }, JsonRequestBehavior.AllowGet);
            }
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        public JsonResult getMyFavouriteEvents(string userId)
        {
            Guid? _userId = null;
            if (!string.IsNullOrEmpty(userId))
                _userId = new Guid(sa.Decrypt(userId));
            string userid = (_userId == null ? "" : "UserId=" + _userId.ToString() + ". ");
            try
            {
                if (_userId == null)
                {
                    LogMessage(userid + "API getMyFavouriteEvents: UserId is not passed.", LogLevel.ERROR);
                    return Json(new { error = "Request failed: UserId not passed." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (!_supperClubRepository.IsValidUser((Guid)_userId))
                    {
                        LogMessage(userid + "API getMyFavouriteEvents: Invalid UserId. UserId=" + _userId.ToString(), LogLevel.ERROR);
                        return Json(new { error = "Request failed: Invalid UserId" }, JsonRequestBehavior.AllowGet);
                    }
                    LogMessage(userid + "API getMyFavouriteEvents: Getting User's favourite events.");
                    var favouriteEvents = _supperClubRepository.GetFavouriteEventsForAUser((Guid)_userId);

                    MyFavouriteEventsWithMetaData eventList = new MyFavouriteEventsWithMetaData();
                    IList<MyWaitlistedEvent> myfavouriteEvents = new List<MyWaitlistedEvent>();

                    foreach (Event el in favouriteEvents)
                    {
                        MyWaitlistedEvent mwle = new MyWaitlistedEvent();
                        mwle.EventId = el.Id;
                        mwle.EventName = el.Name.Replace("\u0027", "'");
                        mwle.EventImage = ServerMethods.ServerUrl + ServerMethods.EventImagePath.Replace("~/", "") + el.ImagePath;
                        mwle.EventURL = ServerMethods.ServerUrl + el.UrlFriendlyName + "/" + el.Id.ToString();
                        mwle.Cost = el.CostToGuest;
                        mwle.BaseCost = el.Cost;
                        mwle.EventStart = el.Start;
                        mwle.EventEnd = el.End;
                        mwle.EventAddress = el.Address + (string.IsNullOrEmpty(el.Address2) ? "" : " " + el.Address2);
                        mwle.EventCity = el.City;
                        mwle.EventPostCode = el.PostCode;
                        mwle.EventLatitude = el.Latitude;
                        mwle.EventLongitude = el.Longitude;
                        string esd = Regex.Replace(el.EventDescription, "<.+?>", string.Empty);
                        mwle.EventShortDescription = esd.Substring(0, esd.Length > 120 ? 120 : esd.Length);
                        mwle.EventDescription = esd;
                        mwle.TotalSeats = el.TotalEventGuests;
                        mwle.GuestsAttending = el.TotalNumberOfAttendeeGuests;
                        myfavouriteEvents.Add(mwle);
                    }
                    eventList.FavouriteEvents = myfavouriteEvents;
                    LogMessage(userid + "API getMyFavouriteEvents: Results returned successfully.", LogLevel.DEBUG);
                    return Json(eventList, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage(userid + "API getMyFavouriteEvents: Error getting user's favourite events" + "  UserId:" + _userId.ToString() + "  Error Details:" + ex.Message + "  StackTrace:" + ex.StackTrace);
                return Json(new { error = "Error occurred while getting user's favourite events" }, JsonRequestBehavior.AllowGet);
            }
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        public JsonResult getNotificationStates(string deviceToken)
        {
            string userid = "";
            try
            {
                if (string.IsNullOrEmpty(deviceToken))
                {
                    LogMessage(userid + "API getNotificationStates: deviceToken is not passed.", LogLevel.ERROR);
                    return Json(new { error = "Request failed: deviceToken not passed." }, JsonRequestBehavior.AllowGet);
                }
                LogMessage(userid + "API getNotificationStates: Getting User's push notification subscription status");                        
                UserDevice ud = _supperClubRepository.GetUserDevice(deviceToken);
                if (ud == null)
                {
                    LogMessage(userid + "API getNotificationStates: Invalid Device Token", LogLevel.ERROR);
                    return Json(new { error = "Request failed: Invalid Device Token" }, JsonRequestBehavior.AllowGet);
                }
                userid = "UserId=" + ud.UserId.ToString() + ". ";
                NotificationStatesWithMetaData notificationList = new NotificationStatesWithMetaData();
                NotificationState nsFbFriendInstalledApp = new NotificationState();
                nsFbFriendInstalledApp.NotificationType = "FB_FRIEND_INSTALLED_APP";
                nsFbFriendInstalledApp.State = ud.FbFriendInstalledApp;
                        
                NotificationState nsFbFriendBookedTicket = new NotificationState();
                nsFbFriendBookedTicket.NotificationType = "FB_FRIEND_BOOKED_TICKET";
                nsFbFriendBookedTicket.State = ud.FbFriendBookedTicket;

                NotificationState nsFavChefNewEventNotification = new NotificationState();
                nsFavChefNewEventNotification.NotificationType = "FAV_CHEF_NEW_EVENT_NOTIFICATION";
                nsFavChefNewEventNotification.State = ud.FavChefNewEventNotification;

                NotificationState nsFavEventBookingReminder = new NotificationState();
                nsFavEventBookingReminder.NotificationType = "FAV_EVENT_BOOKING_REMINDER";
                nsFavEventBookingReminder.State = ud.FavEventBookingReminder;

                NotificationState nsWaitlistEventTicketsAvailable = new NotificationState();
                nsWaitlistEventTicketsAvailable.NotificationType = "WAITLIST_EVENT_TICKETS_AVAILABLE";
                nsWaitlistEventTicketsAvailable.State = ud.WaitlistEventTicketsAvailable;

                notificationList.Notifications = new List<NotificationState>();
                notificationList.Notifications.Add(nsFbFriendInstalledApp);
                notificationList.Notifications.Add(nsFbFriendBookedTicket);
                notificationList.Notifications.Add(nsFavChefNewEventNotification);
                notificationList.Notifications.Add(nsFavEventBookingReminder);
                notificationList.Notifications.Add(nsWaitlistEventTicketsAvailable);

                LogMessage(userid + "API getNotificationStates: Fetched states successfully.", LogLevel.DEBUG);
                return Json(notificationList, JsonRequestBehavior.AllowGet);                
            }
            catch (Exception ex)
            {
                LogMessage(userid + "API getNotificationStates: Error push notification subscription status. Error Details:" + ex.Message + "  StackTrace:" + ex.StackTrace);
                return Json(new { error = "Error occurred while getting user's push notification status" }, JsonRequestBehavior.AllowGet);
            }
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        [HttpPost]
        public JsonResult setNotificationState(string deviceToken, string notificationType, bool? state)
        {
            string userid = "";
            try
            {
                if (string.IsNullOrEmpty(deviceToken))
                {
                    LogMessage(userid + "API setNotificationState: deviceToken is not passed.", LogLevel.ERROR);
                    return Json(new { success = false, message = "Request failed: deviceToken not passed." }, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(notificationType))
                {
                    LogMessage(userid + "API setNotificationState: notificationType is not passed.", LogLevel.ERROR);
                    return Json(new { success = false, message = "Request failed: notificationType not passed." }, JsonRequestBehavior.AllowGet);
                }
                if (state == null)
                {
                    LogMessage(userid + "API setNotificationState: state is not passed.", LogLevel.ERROR);
                    return Json(new { success = false, message = "Request failed: state not passed." }, JsonRequestBehavior.AllowGet);
                }
                if (notificationType != PushNotificationService.MessageTemplates.FAV_CHEF_NEW_EVENT_NOTIFICATION.ToString() 
                    && notificationType != PushNotificationService.MessageTemplates.FAV_EVENT_BOOKING_REMINDER.ToString() 
                    && notificationType != PushNotificationService.MessageTemplates.FB_FRIEND_BOOKED_TICKET.ToString() 
                    && notificationType != PushNotificationService.MessageTemplates.FB_FRIEND_INSTALLED_APP.ToString() 
                    && notificationType != PushNotificationService.MessageTemplates.WAITLIST_EVENT_TICKETS_AVAILABLE.ToString())
                {
                    LogMessage(userid + "API setNotificationState: Invalid notificationType", LogLevel.ERROR);
                    return Json(new { success = false, message = "Request failed: Invalid notificationType" }, JsonRequestBehavior.AllowGet);
                }                
                LogMessage(userid + "API setNotificationState: Setting user's push notification subscription status");                        
                UserDevice ud = _supperClubRepository.GetUserDevice(deviceToken);
                if (ud == null)
                {
                    LogMessage(userid + "API setNotificationState: Invalid Device Token", LogLevel.ERROR);
                    return Json(new { success = false, message = "Request failed: Invalid Device Token" }, JsonRequestBehavior.AllowGet);
                }
                switch(notificationType)
                {
                    case "FB_FRIEND_INSTALLED_APP":
                        ud.FbFriendInstalledApp = (bool)state;
                        break;
                    case "FB_FRIEND_BOOKED_TICKET":
                        ud.FbFriendBookedTicket = (bool)state;
                        break;
                    case "FAV_CHEF_NEW_EVENT_NOTIFICATION":
                        ud.FavChefNewEventNotification = (bool)state;
                        break;
                    case "FAV_EVENT_BOOKING_REMINDER":
                        ud.FavEventBookingReminder = (bool)state;
                        break;
                    case "WAITLIST_EVENT_TICKETS_AVAILABLE":
                        ud.WaitlistEventTicketsAvailable = (bool)state;
                        break;
                }
                bool success = _supperClubRepository.UpdateNotificationState(ud);
                if(success)
                    return Json(new { success = success, message = "OK" }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { success = false, message = "Database error setting the notification state" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogMessage(userid + "API setNotificationState: Error setting the notification state. Error Details:" + ex.Message + "  StackTrace:" + ex.StackTrace);
                return Json(new { success = false, message = "Error occurred while setting the notification state" }, JsonRequestBehavior.AllowGet);
            }
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        public JsonResult addEventReview(int? eventId, string userId, dynamic reviewDetails = null)
        {
            Guid? _userId = null;
            if (!string.IsNullOrEmpty(userId))
                _userId = new Guid(sa.Decrypt(userId));
            string userid = (_userId == null ? "" : "UserId=" + _userId.ToString() + ". ");
            try
            {
                if (eventId == null || eventId <= 0)
                {
                    LogMessage(userid + "API addEventReview: EventId is not passed.", LogLevel.ERROR);
                    return Json(new { Success = false, Message = "Request failed: Event Id not passed." }, JsonRequestBehavior.AllowGet);
                }
                if (_userId == null)
                {
                    LogMessage(userid + "API addEventReview: UserId is not passed.", LogLevel.ERROR);
                    return Json(new { Success = false, Message = "Request failed: UserId not passed." }, JsonRequestBehavior.AllowGet);
                }
                if (!_supperClubRepository.IsValidUser((Guid)_userId))
                {
                    LogMessage(userid + "API addEventReview: Invalid UserId. UserId=" + _userId.ToString(), LogLevel.ERROR);
                    return Json(new { Success = false, Message = "Request failed: Invalid UserId" }, JsonRequestBehavior.AllowGet);
                }
                Event _event = _supperClubRepository.GetEvent((int)eventId);
                if(_event == null)
                {
                    LogMessage(userid + "API addEventReview: Invalid Event Id", LogLevel.ERROR);
                    return Json(new { Success = false, Message = "Request failed: Invalid Event Id" }, JsonRequestBehavior.AllowGet);
                }
                if(_event.MinSeatingTime > DateTime.Now)
                {
                    LogMessage(userid + "API addEventReview: Can not submit review for future event", LogLevel.ERROR);
                    return Json(new { Success = false, Message = "Request failed: Review can not be submitted or a future event." }, JsonRequestBehavior.AllowGet);
                }
                User _user = _supperClubRepository.GetUser(_userId);
                if(_user == null)
                {
                    LogMessage(userid + "API addEventReview: Invalid UserId", LogLevel.ERROR);
                    return Json(new { Success = false, Message = "Request failed: Invalid UserId" }, JsonRequestBehavior.AllowGet);
                }
                
                if (reviewDetails == null)
                {
                    LogMessage(userid + "API addEventReview - review details were not passed.", LogLevel.ERROR);
                    return Json(new { Success = false, Message = "Request failed: review details not passed." }, JsonRequestBehavior.AllowGet);
                }
                LogMessage(userid + "API addEventReview: Adding event review. Event Id:" + eventId.ToString());

                Review review = new Review();
                JavaScriptSerializer jss = new JavaScriptSerializer();
                Dictionary<string,object> _review = new Dictionary<string,object>();
                try
                {
                    _review = jss.Deserialize<dynamic>(reviewDetails);
                }
                catch
                {
                    LogMessage(userid + "API addEventReview - Error parsing review details json", LogLevel.ERROR);
                    return Json(new { Success = false, Message = "Request failed: Error parsing review details json" }, JsonRequestBehavior.AllowGet);
                }
                review.EventId = (int) eventId;
                review.UserId = (Guid) _userId;
                review.Rating = decimal.Parse(_review["rating"].ToString());
                review.Title = _review["reviewTitle"].ToString();
                review.PublicReview = _review["reviewText"].ToString();
                review.Anonymous = (bool)_review["postAsAnonymous"];
                //review.PrivateReview = "";                        
                review.DateCreated = DateTime.Now;
                review.Publish = true;

                bool success = _supperClubRepository.AddReview(review);
                
                if (success)
                {
                    //EmailService.EmailService ems = new EmailService.EmailService(_supperClubRepository);
                    //ems.SendHostNewReviewEmail(review.EventId);
                    //ems.SendAdminNewReviewEmail(review.EventId, review.Rating, review.PublicReview, review.PrivateReview);

                    LogMessage(userid + "API addEventReview: Added review successfully. Event Id:" + eventId.ToString());
                    return Json(new { Success = true, Message = "Added event review successfully" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogMessage(userid + "API addEventReview: Failed to add user to Event Wait List. Event Id:" + eventId.ToString());
                    return Json(new { Success = false, Message = "Failed to add event review" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage(userid + "API addEventReview: Error while adding event review. Event Id: " + eventId + "  Error Details:" + ex.Message + "  StackTrace:" + ex.StackTrace);
                return Json(new { Success = false, Message = "Error occurred while adding event review" }, JsonRequestBehavior.AllowGet);
            }
        }
        
        public JsonResult getGrubclubDetails(int? grubclubId = null, string userId = null)
        {
            Guid? _userId = null;
            if (!string.IsNullOrEmpty(userId))
                _userId = new Guid(sa.Decrypt(userId));
            string userid = (_userId == null ? "" : "UserId=" + _userId.ToString() + ". ");
            try
            {
                if (grubclubId != null && grubclubId > 0)
                {
                    LogMessage(userid + "API getGrubclubDetails: " + grubclubId.ToString());

                    SupperClub.Domain.SupperClub _supperClub = _supperClubRepository.GetSupperClub((int)grubclubId);
                    if (_supperClub == null)
                    {
                        LogMessage(userid + "API getGrubclubDetails - Grub Club did not exist for Id: " + grubclubId.ToString(), LogLevel.ERROR);
                        return Json(new { error = "Request failed: Grub Club does not exist for Id: " + grubclubId.ToString() }, JsonRequestBehavior.AllowGet);
                    }
                    var jsonFutureEvents = new List<object>();
                    if (_supperClub.Events.Count > 0)
                    {
                        foreach (Event e in _supperClub.Events)
                        {
                            if (e.Start > DateTime.Now)
                            {
                                jsonFutureEvents.Add(new
                                {
                                    eventId = e.Id,
                                    eventName = e.Name.Replace("\u0027", "'"),
                                    eventDate = e.Start
                                });
                            }
                        }
                    }
                    var serializer = new JavaScriptSerializer();                    
                    LogMessage(userid + "API getGrubclubDetails: Details fetched successfully.", LogLevel.DEBUG);
                    return Json(new
                    {
                        name = _supperClub.Name.Replace("\u0027", "'"),
                        description = _supperClub.Description,
                        summary = _supperClub.Summary,
                        grubclubId = _supperClub.Id,
                        imageURL = ServerMethods.ServerUrl + ServerMethods.SupperClubImagePath.Replace("~/", "") + _supperClub.ImagePath,
                        pageUrl = ServerMethods.ServerUrl + _supperClub.UrlFriendlyName,
                        futureEvents = jsonFutureEvents.Count > 0 ? serializer.Serialize(jsonFutureEvents) : "",
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogMessage(userid + "Request failed: Grub Club Id not passed", LogLevel.ERROR);
                    return Json(new { error = "Request failed: Grub Club Id not passed. Please pass Grub Club Id as part of request." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage(userid + "API getGrubclubDetails: Error returning results. ErrorMessage:" + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                return Json(new { error = "Error occurred while getting Grub Club details." }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult getSupperClubEventList(int? sid = null, bool returnUrl = false)
        {
            try
            {
                if (sid != null && sid > 0)
                {
                    LogMessage("API getSupperClubEventList: " + sid.ToString());

                    IList<Event> _events= _supperClubRepository.GetFutureEventsForASupperClub((int)sid);
                    if (_events == null)
                    {
                        LogMessage("API getSupperClubEventList - Grub Club did not exist for Id: " + sid.ToString(), LogLevel.ERROR);
                        return Json(new { error = "Request failed: Grub Club does not exist for Id: " + sid.ToString() }, JsonRequestBehavior.AllowGet);
                    }
                    var jsonFutureEvents = new List<object>();
                    if (_events.Count > 0)
                    {
                        foreach (Event e in _events)
                        {
                            jsonFutureEvents.Add(new
                                {
                                    eventId = e.Id,
                                    eventName = e.Name.Replace("\u0027", "'"),
                                    eventDate = e.Start,
                                    eventTime = e.EventSeatings.Count > 0 ? e.MinSeatingTime.ToShortTimeString() + " Onwards" : e.Start.ToShortTimeString(),
                                    eventCost = e.CostToGuest,
                                    availableTicketCount = e.TotalNumberOfAvailableSeats,
                                    eventUrl = (returnUrl ? ServerMethods.ServerUrl + e.SupperClub.UrlFriendlyName + "/" + e.UrlFriendlyName + "/" + e.Id.ToString(): "")
                                });                           
                        }
                    }
                    var serializer = new JavaScriptSerializer();
                    LogMessage("API getSupperClubEventList: List fetched successfully.", LogLevel.DEBUG);
                    return Json(new
                    {
                        futureEvents = jsonFutureEvents.Count > 0 ? serializer.Serialize(jsonFutureEvents) : "",
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogMessage("Request failed: Grub Club Id not passed", LogLevel.ERROR);
                    return Json(new { error = "Request failed: Grub Club Id not passed. Please pass Grub Club Id as part of request." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage("API getSupperClubEventList: Error returning results. ErrorMessage:" + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                return Json(new { error = "Error occurred while getting Grub Club details." }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult getEvents(DateTime? startDate = null, double? lat = null, double? lng = null, string location = "", int pageIndex = 0, int resultsPerPage = 0, string userId = null, int? maxSeatsAvailable = null, int offset = 100)
        {
            Guid? _userId = null;
            if (!string.IsNullOrEmpty(userId))
                _userId = new Guid(sa.Decrypt(userId));
            string userid = (_userId == null ? "" : "UserId=" + _userId.ToString() + ". ");
            if (!startDate.HasValue)
                startDate = DateTime.Now;
            try
            {
                LogMessage(userid + "Get API Get Event Listings.");
            
                EventListing eventListing = new EventListing();

                eventListing.MinPrice = 0;
                eventListing.MaxPrice = 1000000;
                GeoLocation geolocation = null;
                if (_userId != null && !_supperClubRepository.IsValidUser((Guid)_userId))
                {                    
                    eventListing.UserId = _userId;
                }

                if (lat != null && lng != null)
                {
                    eventListing.Latitude = (double)lat;
                    eventListing.Longitude = (double)lng;
                    geolocation.Distance = eventListing.Distance;
                    geolocation.Latitude = eventListing.Latitude;
                    geolocation.Longitude = eventListing.Longitude;
                }
                else
                {
                    if (string.IsNullOrEmpty(location))
                    {
                        location = "London";
                        geolocation = new GeoLocation(1, 51.5073509, -0.1277583);
                    }
                    else
                        geolocation = SupperClub.Code.GeoMethods.GetGeoLocation(string.Empty, location);

                    if (geolocation != null)
                    {
                        eventListing.Latitude = geolocation.Latitude;
                        eventListing.Longitude = geolocation.Longitude;
                    }
                }

                if (pageIndex < 1)
                    pageIndex = 1;
                if (resultsPerPage < 1)
                    resultsPerPage = 20;
                if (maxSeatsAvailable != null)
                    eventListing.MaxSeatsAvailable = maxSeatsAvailable;
                eventListing.PageIndex = pageIndex;
                eventListing.ResultsPerPage = resultsPerPage;

                EventListingFilter filter = new EventListingFilter();
               
                try
                {
                    eventListing.Distance = int.Parse(System.Configuration.ConfigurationManager.AppSettings["GoogleMapsSearchDistance"]);
                }
                catch
                {
                    LogMessage(userid + "API: Could not fetch GoogleMapsSearchDistance from config. Used default value.", LogLevel.ERROR);
                    eventListing.Distance = 10;
                }

                eventListing.Charity = null;
                eventListing.Alcohol = null;
                //eventListing.QueryTag = string.Empty;
                eventListing.StartDate = (DateTime)startDate;
                eventListing.Offset = offset;

                var result = _supperClubRepository.GetEvents(eventListing, filter, false);
                if (result == null || result.Count == 0)
                {
                    LogMessage(userid + "API Event Listing: No result returned.Input Parameters: Latitude="+ lat.ToString()+ ",Longitude=" + lng.ToString() + ",Location=" + location + ",PageIndex="+pageIndex.ToString()+",ResultsPerPage="+ resultsPerPage.ToString(), LogLevel.INFO);
                    return Json(new { result = "No Events Found." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    EventListingWithMetaData displayResults = new EventListingWithMetaData();
                    displayResults.EventListingResults = result.OrderBy(x => x.EventStart).ToList();
                    displayResults.ResultCount = result.Count;
                    displayResults.SearchLocation = geolocation;
                    foreach (EventListingResult el in displayResults.EventListingResults)
                    {
                        el.EventImage = ServerMethods.ServerUrl + ServerMethods.EventImagePath.Replace("~/","") + el.EventImage;
                        el.EventURL = ServerMethods.ServerUrl + el.EventUrlFriendlyName + "/" + el.EventId.ToString();
                        el.EventDescription = el.EventDescription.Replace("\u0027", "'");
                        el.EventName = el.EventName.Replace("\u0027", "'");
                        string esd = Regex.Replace(el.EventDescription, "<.+?>", string.Empty);
                        el.EventShortDescription = esd.Substring(0, esd.Length > 120 ? 120 : esd.Length);
                    }
                    LogMessage(userid + "API Event Listing: Results returned successfully.", LogLevel.DEBUG);
                    return Json(displayResults, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage(userid + "API Event Listing: No result returned. ErrorMessage:" + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                return Json(new { error = "Error occurred while searching for event." }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult searchAllEvents(DateTime? startDate = null, double? lat = null, double? lng = null, string location = "", string priceRange = null, string diet = null, string cuisine = null, bool? charity = null, bool? alcohol = null, int pageIndex = 0, int resultsPerPage = 0, string userId = null, int offset = 100)
        {
            Guid? _userId = null;
            if (!string.IsNullOrEmpty(userId))
                _userId = new Guid(sa.Decrypt(userId));
            string userid = (_userId == null ? "" : "UserId=" + _userId.ToString() + ". ");
            if (!startDate.HasValue)
                startDate = DateTime.Now;
            try
            {
                LogMessage(userid + "API searchAllEvents.");
            
                EventListing eventListing = new EventListing();
                EventListingFilter filter = new EventListingFilter();

                eventListing.StartDate = (DateTime)startDate;
                eventListing.MinPrice = 0;
                eventListing.MaxPrice = 1000000;
                eventListing.Alcohol = alcohol;
                eventListing.Charity = charity;
                eventListing.Offset = offset;

                //Separators
                string[] stringSeparators = new string[] { "," };

                //Price Filter
                if (!string.IsNullOrEmpty(priceRange))
                {
                    string[] sPriceOptions = priceRange.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                    int[] iPriceOptions = null;
                    if (sPriceOptions.Length > 0)
                    {
                        iPriceOptions = new int[sPriceOptions.Length];
                        for (int i = 0; i < sPriceOptions.Length; i++)
                            iPriceOptions[i] = Int32.Parse(sPriceOptions[i]);
                    }
                    if (iPriceOptions != null)
                    {
                        PriceRange pr = new PriceRange();
                        pr = _supperClubRepository.GetMinMaxPriceRange(iPriceOptions);
                        eventListing.MinPrice = pr.MinPrice;
                        eventListing.MaxPrice = pr.MaxPrice;
                    }
                }

                //Diet Filter
                if (!string.IsNullOrEmpty(diet))
                {
                    string[] dietList = diet.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                    if (dietList.Length > 0)
                    {
                        filter.DietIds = new List<int>();
                        for (int i = 0; i < dietList.Length; i++)
                            filter.DietIds.Add(Int32.Parse(dietList[i]));
                    }
                }

                //Cuisine Filter
                if (!string.IsNullOrEmpty(cuisine))
                {
                    string[] cuisineList = cuisine.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                    if (cuisineList.Length > 0)
                    {
                        filter.CuisineIds = new List<int>();
                        for (int i = 0; i < cuisineList.Length; i++)
                            filter.CuisineIds.Add(Int32.Parse(cuisineList[i]));
                    }
                }

                try
                {
                    eventListing.Distance = int.Parse(System.Configuration.ConfigurationManager.AppSettings["GoogleMapsSearchDistance"]);
                }
                catch
                {
                    LogMessage(userid + "API searchAllEvents: Could not fetch GoogleMapsSearchDistance from config. Used default value.", LogLevel.ERROR);
                    eventListing.Distance = 10;
                }

                //Geo Location 
                GeoLocation geolocation = null; 
                if (lat != null && lng != null)
                {
                    eventListing.Latitude = (double)lat;
                    eventListing.Longitude = (double)lng;
                    geolocation = new GeoLocation(eventListing.Distance, eventListing.Latitude, eventListing.Longitude);
                    //geolocation.Distance = eventListing.Distance;                    
                    //geolocation.Latitude = eventListing.Latitude;
                    //geolocation.Longitude = eventListing.Longitude;
                }
                else
                {
                    if (string.IsNullOrEmpty(location))
                    {
                        location = "London";
                        geolocation = new GeoLocation(1, 51.5073509, -0.1277583);
                    }
                    else
                        geolocation = SupperClub.Code.GeoMethods.GetGeoLocation(string.Empty, location);

                    if (geolocation != null)
                    {
                        eventListing.Latitude = geolocation.Latitude;
                        eventListing.Longitude = geolocation.Longitude;
                    }
                    else
                    {
                        eventListing.Latitude = 51.5073346;
                        eventListing.Longitude = -0.1276831;
                    }
                }

                if (pageIndex < 1)
                    pageIndex = 1;
                if (resultsPerPage < 1)
                    resultsPerPage = 20;
                eventListing.PageIndex = pageIndex;                
                eventListing.ResultsPerPage = resultsPerPage;

                if (_userId != null && !_supperClubRepository.IsValidUser((Guid)_userId))
                {
                    eventListing.UserId = _userId;
                }

                //eventListing.StartDate = DateTime.Now;
                //eventListing.QueryTag = string.Empty;
                var result = _supperClubRepository.GetEvents(eventListing, filter, true);
                if (result == null || result.Count == 0)
                {
                    LogMessage(userid + "API searchAllEvents: No result returned.Input Parameters: Latitude=" + lat.ToString() + ",Longitude=" + lng.ToString() + ",Location=" + location + ",PageIndex=" + pageIndex.ToString() + ",ResultsPerPage=" + resultsPerPage.ToString() + ",Charity=" + charity.ToString() + ",Alcohol=" + alcohol.ToString(), LogLevel.INFO);
                    return Json(new { result = "No Events Found." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    EventListingWithMetaData displayResults = new EventListingWithMetaData();
                    displayResults.EventListingResults = result.OrderBy(x => x.EventStart).ToList();
                    displayResults.ResultCount = result.Count;
                    displayResults.SearchLocation = geolocation;
                    foreach (EventListingResult el in displayResults.EventListingResults)
                    {
                        el.EventImage = ServerMethods.ServerUrl + ServerMethods.EventImagePath.Replace("~/", "") + el.EventImage;
                        el.EventURL = ServerMethods.ServerUrl + el.EventUrlFriendlyName + "/" + el.EventId.ToString();
                        el.EventDescription = el.EventDescription.Replace("\u0027", "'");
                        el.EventName = el.EventName.Replace("\u0027", "'");
                        string esd = Regex.Replace(el.EventDescription, "<.+?>", string.Empty);
                        el.EventShortDescription = esd.Substring(0, esd.Length > 120 ? 120 : esd.Length);
                    }
                    LogMessage(userid + "API searchAllEvents: Results returned successfully.", LogLevel.DEBUG);
                    return Json(displayResults, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage(userid + "API EsearchAllEvents: No result returned. ErrorMessage:" + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                return Json(new { error = "Error occurred while searching for event." }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult getPriceRanges()
        {
            try
            {
                IList<PriceRange> prl = null;
                prl = _supperClubRepository.GetPriceRange();
                if (prl != null && prl.Count > 0)
                {
                    var jsonPriceRange = new List<object>();
                    foreach (PriceRange pr in prl)
                    {
                        jsonPriceRange.Add(new
                        {
                            PriceRangeId = pr.Id,
                            PriceRangeName = pr.PriceName,
                            MinPrice = pr.MinPrice,
                            MaxPrice = pr.MaxPrice
                        });
                    }
                    var serializer = new JavaScriptSerializer();
                    LogMessage("API getPriceRanges: Details fetched successfully.", LogLevel.DEBUG);                    
                    return Json(jsonPriceRange, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogMessage("API getPriceRanges: Could not fetch the Price Range Options.", LogLevel.ERROR);
                    return Json(new { error = "Request failed: Could not fetch the Price Range Options." }, JsonRequestBehavior.AllowGet);
                }
             }
             catch (Exception ex)
             {
                 LogMessage("API getPriceRanges: Error getting Price Range Options. ErrorMessage:" + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                 return Json(new { error = "Error occurred while getting Price Range Options." }, JsonRequestBehavior.AllowGet);
             }
        }

        public JsonResult getCuisines()
        {
            try
            {
                IList<Cuisine> cl = null;
                cl = _supperClubRepository.GetAvailableCuisines(0);
                if (cl != null && cl.Count > 0)
                {
                    var jsonCuisineOptions = new List<object>();
                    foreach (Cuisine c in cl)
                    {
                        jsonCuisineOptions.Add(new
                        {
                            CuisineId = c.Id,
                            CuisineName = c.Name.Replace("\u0027", "'")
                        });
                    }
                    LogMessage("API getCuisines: Details fetched successfully.", LogLevel.DEBUG);
                    return Json(jsonCuisineOptions, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogMessage("API getCuisines: Could not fetch the cuisine Options.", LogLevel.ERROR);
                    return Json(new { error = "Request failed: Could not fetch the cuisine options." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage("API getCuisines: Error getting cuisine options. ErrorMessage:" + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                return Json(new { error = "Error occurred while getting cuisine options." }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult getDiets()
        {
            try
            {
                IList<Diet> dl = null;
                dl = _supperClubRepository.GetDiets();
                if (dl != null && dl.Count > 0)
                {
                    var jsonDietOptions = new List<object>();
                    foreach (Diet d in dl)
                    {
                        jsonDietOptions.Add(new
                        {
                            DietId = d.Id,
                            DietName = d.Name.Replace("\u0027", "'")
                        });
                    }
                    LogMessage("API getDiets: Details fetched successfully.", LogLevel.DEBUG);
                    return Json(jsonDietOptions, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogMessage("API getDiets: Could not fetch the diet Options.", LogLevel.ERROR);
                    return Json(new { error = "Request failed: Could not fetch the diet options." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage("API getDiets: Error getting diet options. ErrorMessage:" + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                return Json(new { error = "Error occurred while getting diet options." }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult getTags()
        {
            try
            {
                IList<Tag> tl = null;
                tl = _supperClubRepository.GetTags();
                if (tl != null && tl.Count > 0)
                {
                    var jsonTagList = new List<object>();
                    foreach (Tag t in tl)
                    {
                        jsonTagList.Add(new
                        {
                            TagId = t.Id,
                            TagName = t.Name.Replace("\u0027", "'")
                        });
                    }
                    var serializer = new JavaScriptSerializer();
                    LogMessage("API getTags: Details fetched successfully.", LogLevel.DEBUG);
                    return Json(jsonTagList, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogMessage("API getTags: Could not fetch the tags.", LogLevel.ERROR);
                    return Json(new { error = "Request failed: Could not fetch the tags." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage("API getTags: Error getting tags. ErrorMessage:" + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                return Json(new { error = "Error occurred while getting tags." }, JsonRequestBehavior.AllowGet);
            }
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        public JsonResult getUserDetails(string userId = null)
        {
            Guid? _userId = null;
            if (!string.IsNullOrEmpty(userId))
                _userId = new Guid(sa.Decrypt(userId));
            string userid = (_userId == null ? "" : "UserId=" + _userId.ToString() + ". ");
            
            try
            {
                if (_userId == null)
                {
                    LogMessage("API getUserDetails: User Id was not passed.", LogLevel.ERROR);
                    return Json(new { error = "Request Failed: User Id was not passed" }, JsonRequestBehavior.AllowGet);
                }
                if (!_supperClubRepository.IsValidUser((Guid)_userId))
                {
                    LogMessage(userid + "API getUserDetails: Invalid UserId. UserId=" + _userId.ToString(), LogLevel.ERROR);
                    return Json(new { error = "Request failed: Invalid UserId" }, JsonRequestBehavior.AllowGet);
                }
                User _user = null;
                _user = _supperClubRepository.GetUser((Guid)_userId);
                if (_user == null)
                {
                    LogMessage("API getUserDetails: Invalid User Id", LogLevel.ERROR);
                    return Json(new { error = "Request Failed: Invalid User Id" }, JsonRequestBehavior.AllowGet);
                }
                LogMessage("API getUserDetails: Fetched the user details successfully.", LogLevel.DEBUG);
                return Json(new {
                    UserId = sa.Encrypt(_userId.ToString()),
                    Email = _user.Email,
                    FirstName = _user.FirstName,
                    LastName = _user.LastName,
                    Gender = _user.Gender,
                    DateOfBirth = _user.DateOfBirth,
                    Address = _user.Address,
                    Postcode = _user.PostCode,
                    DietaryPreferences = _user.DietaryPreferences,
                    ContactNumber = _user.ContactNumber
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogMessage("API getUserDetails: Error getting user details. ErrorMessage:" + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                return Json(new { error = "Error occurred while getting user details." }, JsonRequestBehavior.AllowGet);
            }
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        [HttpPost]
        public JsonResult updateUserDetails(string userId, string email= null, string firstName = null, string lastName = null, string gender = null, string dateOfBirth = null, string address = null, string postCode = null, string dietaryPreferences = null, string contactNumber = null, string oldPassword = null, string newPassword = null)
        {
            Guid? _userId = null;
            if (!string.IsNullOrEmpty(userId))
                _userId = new Guid(sa.Decrypt(userId));
            string userid = (_userId == null ? "" : "UserId=" + _userId.ToString() + ". ");
            bool pwdChangeStatus = false;
            string pwdChangeMessage = "";
            try
            {
                if (_userId == null)
                {
                    LogMessage("API updateUserDetails: User Id was not passed.", LogLevel.ERROR);
                    return Json(new { Success = false, PasswordChange = pwdChangeStatus, Message = "Request Failed: User Id was not passed", PasswordStatus = pwdChangeMessage  }, JsonRequestBehavior.AllowGet);
                }
                if (!_supperClubRepository.IsValidUser((Guid)_userId))
                {
                    LogMessage(userid + "API updateUserDetails: Invalid UserId. UserId=" + _userId.ToString(), LogLevel.ERROR);
                    return Json(new { Success = false, PasswordChange = pwdChangeStatus, Message = "Request failed: Invalid UserId" }, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(email))
                {
                    LogMessage("API updateUserDetails: Email address was not passed.", LogLevel.ERROR);
                    return Json(new { Success = false, PasswordChange = pwdChangeStatus, Message = "Request Failed: Email address was not passed", PasswordStatus = pwdChangeMessage }, JsonRequestBehavior.AllowGet);
                }
                // Base64 decode the password field
                if (!string.IsNullOrEmpty(oldPassword))
                {
                    Byte[] byteData = Convert.FromBase64String(oldPassword);
                    oldPassword = Encoding.UTF8.GetString(byteData);
                }

                if (!string.IsNullOrEmpty(newPassword))
                {
                    Byte[] byteData = Convert.FromBase64String(newPassword);
                    newPassword = Encoding.UTF8.GetString(byteData);
                }
                if (!string.IsNullOrEmpty(oldPassword) && !string.IsNullOrEmpty(newPassword))
                {
                    // ChangePassword will throw an exception rather
                    // than return false in certain failure scenarios.
                    try
                    {

                        MembershipUser currentUser = Membership.GetUser(email, true /* userIsOnline */);
                        pwdChangeStatus = currentUser.ChangePassword(oldPassword, newPassword);
                        LogMessage("API updateUserDetails: Changed Password");
                        if (pwdChangeStatus)
                            pwdChangeMessage = "Password changed successfully.";
                        else
                            pwdChangeMessage = "Error: Password could not be reset. Make sure your old password is correct and new password is at least 6 characters long.";
                    }
                    catch (Exception ex)
                    {
                        pwdChangeStatus = false;
                        pwdChangeMessage = "Error changing password. Error Message: " + ex.Message;
                    }                    
                }
               
                User _user = null;
                _user = _supperClubRepository.GetUser((Guid)_userId);
                if (_user == null)
                {
                    LogMessage("API updateUserDetails: Invalid User Id", LogLevel.ERROR);
                    return Json(new { Success = false, PasswordChange = pwdChangeStatus, Message = "Request Failed: Invalid User Id", PasswordStatus = pwdChangeMessage }, JsonRequestBehavior.AllowGet);
                }
                if (postCode != null && postCode.Length > 0 && !isValidPostCode(postCode))
                {
                    LogMessage("API updateUserDetails: Invalid Post Code", LogLevel.ERROR);
                    return Json(new { Success = false, PasswordChange = pwdChangeStatus, Message = "Request Failed: Invalid Post Code", PasswordStatus = pwdChangeMessage }, JsonRequestBehavior.AllowGet);                    
                }
                bool convertSuccess = false;
                DateTime dob = new DateTime();
                    
                if (dateOfBirth != null && dateOfBirth.Length > 0)
                {
                    string ukFormat = "dd/MM/yyyy";
                    CultureInfo culture = CultureInfo.CreateSpecificCulture("en-GB");                      

                    convertSuccess = DateTime.TryParseExact(dateOfBirth, ukFormat, culture, DateTimeStyles.None, out dob);
                    if (!convertSuccess)
                    {
                        LogMessage("API updateUserDetails: Invalid Date", LogLevel.ERROR);
                        return Json(new { Success = false, PasswordChange = pwdChangeStatus, Message = "Request Failed: Date of Birth could not be parsed", PasswordStatus = pwdChangeMessage }, JsonRequestBehavior.AllowGet);
                    }
                }

                _user.FirstName = string.IsNullOrEmpty(firstName) ? _user.FirstName : firstName;
                _user.LastName = string.IsNullOrEmpty(lastName) ? _user.LastName : lastName;
                _user.Gender = string.IsNullOrEmpty(gender) ? _user.Gender : gender;
                _user.DateOfBirth = dateOfBirth == null || !convertSuccess ? _user.DateOfBirth : dob;
                _user.Address = string.IsNullOrEmpty(address) ? _user.Address : address;
                _user.PostCode = string.IsNullOrEmpty(postCode) ? _user.PostCode : postCode;
                _user.DietaryPreferences = string.IsNullOrEmpty(dietaryPreferences) ? _user.DietaryPreferences : dietaryPreferences;

                if (!string.IsNullOrEmpty(contactNumber))
                {
                    if(contactNumber.Length >= 10)
                    {
                        contactNumber = contactNumber.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("+", "").Replace("-", "").Replace(".", "");
                        contactNumber = contactNumber.Substring(contactNumber.Length - 10);
                        double number = 0;
                        bool saveNumber = false;
                        if (double.TryParse(contactNumber, out number))
                            saveNumber = true;
                        if (saveNumber)
                            _user.ContactNumber = contactNumber;
                    }
                    else
                    {
                        LogMessage("API updateUserDetails: Invalid contact number", LogLevel.ERROR);
                        return Json(new { Success = false, PasswordChange = pwdChangeStatus, Message = "Request Failed: Invalid contact number", PasswordStatus = pwdChangeMessage }, JsonRequestBehavior.AllowGet);
                    }
                }
                bool success = _supperClubRepository.UpdateUser(_user);
                if (success)
                {
                    LogMessage("API updateUserDetails: Successfully updated user details.", LogLevel.INFO);
                    return Json(new { Success = success, PasswordChange = pwdChangeStatus, Message = "Successfully updated user details.", PasswordStatus = pwdChangeMessage }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogMessage("API updateUserDetails: Error updating user details.", LogLevel.ERROR);
                    return Json(new { Success = success, PasswordChange = pwdChangeStatus, Message = "Error occurred while getting user details.", PasswordStatus = pwdChangeMessage }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage("API updateUserDetails: Error updating user details. ErrorMessage:" + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                return Json(new { Success = false, PasswordChange = pwdChangeStatus, Message = "Error occurred while getting user details.", PasswordStatus = pwdChangeMessage }, JsonRequestBehavior.AllowGet);
            }
        }
        
        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        [HttpPost]
        public JsonResult login(string email, string password, string deviceToken = null)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    LogMessage("API login: Error logging in. Email or Password was not passed or blank. Email:" + email + "  Password:" + password, LogLevel.ERROR);
                    return Json(new { error = "Error: Email address or password was not passed or blank." }, JsonRequestBehavior.AllowGet);
                }
                string username = email;
                // If the user has tried to log in using their email address grab the user name
                if (Membership.FindUsersByEmail(email).Count == 1)
                {
                    IEnumerable<MembershipUser> members = Membership.FindUsersByEmail(email).Cast<MembershipUser>();
                    username = members.First().UserName;
                }
                if (Membership.FindUsersByName(username).Count != 1)
                {
                    LogMessage("API login: Error logging in. Could not find email address. Email address not registered. Email:" + email, LogLevel.ERROR);
                    return Json(new { error = "Could not find email address. Email address not registered" }, JsonRequestBehavior.AllowGet);
                }
                // If we get this far the username definitely exists
                if (UserMethods.IsSpecificUserLockedOut(username))
                {
                    LogMessage("API login: Error logging in. User account has been locked. Email:" + email, LogLevel.ERROR);
                    return Json(new { error = "User account has been locked" }, JsonRequestBehavior.AllowGet);
                }
                Byte[] byteData = Convert.FromBase64String(password);
                password = Encoding.UTF8.GetString(byteData);

                if (Membership.ValidateUser(email, password))
                {
                    User u = _supperClubRepository.GetUser(email);
                    bool addDevice = false;
                    if (!string.IsNullOrEmpty(deviceToken))
                    {
                        bool deviceExist = _supperClubRepository.CheckUserDevice(deviceToken, (Guid)u.Id);
                        if (!deviceExist)
                        {
                            // Add user device
                            UserDevice ud = new UserDevice();
                            ud.DeviceId = deviceToken;
                            ud.UserId = u.Id;
                            addDevice = _supperClubRepository.AddUserDevice(ud);
                            if (!addDevice)
                            {
                                LogMessage("API login: Error adding user device", LogLevel.ERROR);
                            }
                        }
                    }

                    LogMessage("API login: User details fetched successfully. Email:" + email, LogLevel.INFO);
                    return Json(new
                    {
                        UserId = sa.Encrypt(u.Id.ToString()),
                        FirstName = u.FirstName,
                        LastName = u.LastName,
                        FaceBookId = u.FacebookId,
                        ContactNumber = u.ContactNumber,
                        AddDeviceStatus = addDevice
                    } , JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogMessage("API login: Error logging in. Incorrect password. Email:" + email, LogLevel.ERROR);
                    return Json(new { error = "Incorrect password" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage("API login: Error logging in. Email:" + email + "  ErrorMessage:" + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                return Json(new { error = "Login was unsuccessful. Error occurred while logging in." }, JsonRequestBehavior.AllowGet);
            }
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        [HttpPost]
        public JsonResult register(string email, string password, string firstName, string lastName, string contactNumber, string gender, string deviceToken = null)
        {
            try
            {
                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
                {
                    LogMessage("API register: Error registering new user. Email/Password/Name was not passed or blank. Email:" + email + ",  Password:" + password + ",  First Name:" + firstName + ",  Last Name:" + lastName, LogLevel.ERROR);
                    return Json(new { error = "Request Failed: Email address/Password/Name was not passed or blank." }, JsonRequestBehavior.AllowGet);
                }
                bool validEmail = false;
                // Validate the email first
                try
                {
                    var addr = new System.Net.Mail.MailAddress(email);
                    validEmail = Code.EmailValidator.Validate(email);                    
                }
                catch
                {
                    validEmail = false;
                }
                if (!validEmail)
                {
                    LogMessage("API register: Invalid e-mail Id", LogLevel.ERROR);
                    return Json(new { error = "Request Failed: Invalid e-mail address" }, JsonRequestBehavior.AllowGet);
                }

                // Base64 decode the password field
                Byte[] byteData = Convert.FromBase64String(password);
                password = Encoding.UTF8.GetString(byteData);

                // Attempt to register the user
                MembershipCreateStatus createStatus;
                MembershipUser membershipUser = Membership.CreateUser(email, password, email, null, null, true, null, out createStatus);
                bool success = false;
                if (membershipUser != null)
                {
                    User _user = new User();
                    _user.Id = (Guid)membershipUser.ProviderUserKey;
                    _user.FirstName = firstName;
                    _user.LastName = lastName;
                    if (!string.IsNullOrEmpty(contactNumber))
                        _user.ContactNumber = contactNumber;
                    if (!string.IsNullOrEmpty(gender))
                        _user.Gender = gender;
                    success = _supperClubRepository.CreateUser(_user);
                    if (success)
                        AddToRole(membershipUser, "Guest");

                    bool addDevice = false;
                    if (!string.IsNullOrEmpty(deviceToken))
                    {
                        bool deviceExist = _supperClubRepository.CheckUserDevice(deviceToken, _user.Id);
                        if (!deviceExist)
                        {
                            // Add user device
                            UserDevice ud = new UserDevice();
                            ud.DeviceId = deviceToken;
                            ud.UserId = _user.Id;
                            addDevice = _supperClubRepository.AddUserDevice(ud);
                            if (!addDevice)
                            {
                                LogMessage("API register: Error adding user device", LogLevel.ERROR);
                            }
                        }
                    }
                    if (createStatus == MembershipCreateStatus.Success && success)
                    {
                        if (Membership.ValidateUser(email, password))
                        {
                            EmailService.EmailService em = new EmailService.EmailService(_supperClubRepository);
                            em.SendWelcomeEmail(firstName, membershipUser.Email);                                                       
                        }
                        LogMessage("API register: Registration complete. Logged In: " + email); 
                        return Json(new
                        {
                            RegisterSuccess = true,
                            UserId = sa.Encrypt(_user.Id.ToString()),
                            Email = email,
                            FirstName = firstName,
                            LastName = lastName,
                            ContactNumber = contactNumber,
                            AddDeviceStatus = addDevice
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        LogMessage("API register: Error creating new user account", LogLevel.ERROR);
                        return Json(new { error = "Registration was unsuccessful. " + ErrorCodeToString(createStatus) }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    LogMessage("API register: Error creating new user account", LogLevel.ERROR);
                    return Json(new { error = "Registration was unsuccessful. " + ErrorCodeToString(createStatus) }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage("API register: Error registering new user. ErrorMessage:" + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                return Json(new { error = "Registration was unsuccessful. Error occurred while registering user." }, JsonRequestBehavior.AllowGet);
            }
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        [HttpPost]
        public JsonResult changePassword(string email, string oldPassword, string newPassword)
        {
            try
            {
                if (string.IsNullOrEmpty(email))
                {
                    LogMessage("API changePassword: Error changing password. Email was not passed or blank. Email:" + email, LogLevel.ERROR);
                    return Json(new { error = "Request Failed: Email address was not passed or blank." }, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(oldPassword))
                {
                    LogMessage("API changePassword: Error changing password. Old password was not passed or blank. Old Password:" + oldPassword, LogLevel.ERROR);
                    return Json(new { error = "Request Failed: Old password was not passed or blank." }, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(newPassword))
                {
                    LogMessage("API changePassword: Error changing password. New Password was not passed or blank. New Password:" + newPassword, LogLevel.ERROR);
                    return Json(new { error = "Request Failed: New password was not passed or blank." }, JsonRequestBehavior.AllowGet);
                }
                bool validEmail = false;
                // Validate the email first
                try
                {
                    var addr = new System.Net.Mail.MailAddress(email);
                    validEmail = Code.EmailValidator.Validate(email);
                }
                catch
                {
                    validEmail = false;
                }
                if (!validEmail)
                {
                    LogMessage("API changePassword: Invalid e-mail Id", LogLevel.ERROR);
                    return Json(new { error = "Request Failed: Invalid e-mail address" }, JsonRequestBehavior.AllowGet);
                }

                // Base64 decode the password field
                Byte[] byteData = Convert.FromBase64String(oldPassword);
                oldPassword = Encoding.UTF8.GetString(byteData);

                byteData = Convert.FromBase64String(newPassword);
                newPassword = Encoding.UTF8.GetString(byteData);
                // Attempt to register the user
                // ChangePassword will throw an exception rather
                // than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    MembershipUser currentUser = Membership.GetUser(email, true /* userIsOnline */);
                    changePasswordSucceeded = currentUser.ChangePassword(oldPassword, newPassword);
                    LogMessage("API changePassword: Changed Password");
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    return Json(new { status = true, error = "Password changed successfully." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogMessage("API changePassword: Error changing password.", LogLevel.ERROR);
                    return Json(new { status = false, message = "Error changing password. Please try again." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage("API changePassword: Error changing password. ErrorMessage:" + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                return Json(new { status = false, error = "Error changing password. Please try again." }, JsonRequestBehavior.AllowGet);
            }
        }
        
        public ActionResult Index()
        {
            return View();
        }
        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        [HttpPost]
        public JsonResult fbLogin(string fbId, string email, string deviceToken = null, dynamic fbJson = null, bool encoded = false)
        {
            try
            {
                if (string.IsNullOrEmpty(fbId) || string.IsNullOrEmpty(email) || fbJson == null )
                {
                    LogMessage("API fbLogin failed: Null or empty FBId/Email address/fbJson", LogLevel.ERROR);
                    return Json(new { error = "fbLogin failed. fbId, Email address or fbJson is not supplied or it is blank." }, JsonRequestBehavior.AllowGet);
                }
                
                bool addDevice = false;

                dynamic fbJs = null;
                try
                {
                    fbJs = fbJson;
                }
                catch
                {
                    fbJs = fbJson[0];
                }

                if (encoded)
                {
                    try
                    {
                        // Base64 decode
                        Byte[] byteData = Convert.FromBase64String(fbJs);
                        fbJs = Encoding.UTF8.GetString(byteData);
                    }
                    catch
                    {
                        LogMessage("API fbLogin failed: Error decoding fbJson", LogLevel.ERROR);
                        return Json(new { error = "fbLogin failed: Error decoding fbJson." }, JsonRequestBehavior.AllowGet);
                    }
                }

                var jss = new JavaScriptSerializer();
                Dictionary<string, object> result = new Dictionary<string, object>();
                try
                {
                    result = jss.Deserialize<Dictionary<string, object>>(fbJs);
                }
                catch
                {
                    LogMessage("API fbLogin failed: Error parsing fbJson", LogLevel.ERROR);
                    return Json(new { error = "fbLogin failed: Error parsing fbJson." }, JsonRequestBehavior.AllowGet);
                }

                //check to see if the user exists already
                User currentUser = _supperClubRepository.GetUserByFbId(fbId);
                if (currentUser == null)
                {
                    User _user = _supperClubRepository.GetUser(email);
                    if (_user == null)
                    {
                        MembershipCreateStatus createStatus;
                        string newPassword = PasswordGenerator.GeneratePassword();
                        MembershipUser membershipUser = Membership.CreateUser(email, newPassword, email, null, null, true, null, out createStatus);
                        bool success = AddFBUser(membershipUser, fbJs, result);

                        if (createStatus == MembershipCreateStatus.Success && success)
                        {
                            if (!string.IsNullOrEmpty(deviceToken))
                            {
                                bool deviceExist = _supperClubRepository.CheckUserDevice(deviceToken, (Guid)membershipUser.ProviderUserKey);
                                if (!deviceExist)
                                {
                                    // Add user device
                                    UserDevice ud = new UserDevice();
                                    ud.DeviceId = deviceToken;
                                    ud.UserId = (Guid)membershipUser.ProviderUserKey;
                                    addDevice = _supperClubRepository.AddUserDevice(ud);
                                    if (!addDevice)
                                        LogMessage("API fbLogin: Error adding user device", LogLevel.ERROR);
                                    else
                                        LogMessage("API fbLogin: Added user device successfully", LogLevel.INFO);
                                }
                            }
                            if (result.ContainsKey("friends"))
                            {
                                try
                                {
                                    List<UserFacebookFriend> friendsCollection = new List<UserFacebookFriend>();
                                    System.Collections.ArrayList arrFriends = (System.Collections.ArrayList)result["friends"];
                                    for (int i = 0; i < arrFriends.Count; i++)
                                    {
                                        UserFacebookFriend _userFacebookFriend = new UserFacebookFriend();
                                        _userFacebookFriend.UserId = (Guid)membershipUser.ProviderUserKey;
                                        _userFacebookFriend.UserFacebookId = fbId;
                                        _userFacebookFriend.FriendFacebookId = ((Dictionary<string, object>)arrFriends[i])["id"].ToString();
                                        friendsCollection.Add(_userFacebookFriend);
                                    }
                                    bool addFacebookFriendStatus = _supperClubRepository.AddUserFacebookFriend(friendsCollection);
                                    if (addFacebookFriendStatus)
                                    {
                                        LogMessage("API fbLogin: Saved user's facebook friends in database", LogLevel.INFO);
                                        List<string> lstFriend = _supperClubRepository.GetFacebookFriendListForPushNotification((Guid)membershipUser.ProviderUserKey, fbId);
                                        // Push Notification to Friends
                                        PushNotificationService.MessageService ms = new PushNotificationService.MessageService(_supperClubRepository);
                                        ms.FacebookFriendAppInstallationNotification(result["first_name"].ToString() + " " + result["last_name"].ToString(), (Guid)membershipUser.ProviderUserKey, fbId);
                                    }
                                    else
                                        LogMessage("API fbLogin: Error saving user's facebook friends in database. fbJson:" + fbJson, LogLevel.ERROR);
                                }
                                catch
                                {
                                    LogMessage("API fbLogin: Error saving user's facebook friends info. fbJson:" + fbJson, LogLevel.ERROR);
                                }
                            }
                            if (Membership.ValidateUser(email, newPassword))
                            {
                                EmailService.EmailService em = new EmailService.EmailService(_supperClubRepository);
                                em.SendWelcomeEmail(result["first_name"].ToString(), membershipUser.Email);
                            }
                        }
                        else
                        {
                            LogMessage("API fbLogin: Error creating user account. fbJson:" + fbJson, LogLevel.ERROR);
                            return Json(new { error = "FB Login was unsuccessful. Error occurred while registering user." }, JsonRequestBehavior.AllowGet);
                        }
                        LogMessage("API fbLogin: FB Registration complete. Logged In: " + email);
                        return Json(new
                        {
                            Register = true,
                            Login = true,
                            UserId = sa.Encrypt(((Guid)membershipUser.ProviderUserKey).ToString()),
                            FacebookId = fbId,
                            Email = email,
                            FirstName = result["first_name"].ToString(),
                            LastName = result["last_name"].ToString(),
                            AddDeviceStatus = addDevice
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        if(_user.FacebookId != fbId)
                        {
                            _user.FacebookId = fbId;
                           if (_user.FBUserOnly)
                            {
                                LogMessage("API fbLogin: currentUser.FBUserOnly block", Logger.LogLevel.DEBUG);
                    
                                ConvertFBUserToDomainUser(_user, fbJs);
                                LogMessage("API fbLogin: currentUser.FBUserOnly block -  Converted fb user to domain user", Logger.LogLevel.DEBUG);
                    
                                bool _updateSuccess = _supperClubRepository.UpdateUser(_user);
                                if (_updateSuccess)
                                    LogMessage("API fbLogin: currentUser.FBUserOnly block -  Updated user. userid=" + _user.Id.ToString(), Logger.LogLevel.DEBUG);
                                else
                                    LogMessage("API fbLogin: currentUser.FBUserOnly block -  Error updating user. userid=" + _user.Id.ToString(), Logger.LogLevel.ERROR); 
                            }
                            else if (!_user.FBUserOnly)
                            {
                                LogMessage("API fbLogin: in !currentUser.FBUserOnly block", Logger.LogLevel.DEBUG);
                                try
                                {
                                    _user.FBUserOnly = false;
                                    _user.FBJson = fbJs.ToString();
                                    LogMessage("API fbLogin: in !currentUser.FBUserOnly block sending details to DB for update", Logger.LogLevel.DEBUG);
                                    bool _updateSuccess = _supperClubRepository.UpdateUser(_user);
                                    if (_updateSuccess)
                                        LogMessage("API fbLogin: currentUser.FBUserOnly block -  Updated updated current user details to DB successfully. userid=" + _user.Id.ToString(), Logger.LogLevel.DEBUG);
                                    else
                                        LogMessage("API fbLogin: currentUser.FBUserOnly block -  Error updating user. userid=" + _user.Id.ToString(), Logger.LogLevel.ERROR);
                                   
                                }
                                catch (Exception ex)
                                {
                                    LogMessage("API fbLogin: in !currentUser.FBUserOnly block ERROR: Exception Details: " + ex.Message + "  Inner Exception: " + (ex.InnerException != null ? ex.InnerException.StackTrace : "No Inner Exception.") + "  Stack Trace Info: " + ex.StackTrace + "  User Agent: " + Request.UserAgent, Logger.LogLevel.ERROR);
                                }
                            }
                           if (!string.IsNullOrEmpty(deviceToken))
                           {
                               bool deviceExist = _supperClubRepository.CheckUserDevice(deviceToken, _user.Id);
                               if (!deviceExist)
                               {
                                   // Add user device
                                   UserDevice ud = new UserDevice();
                                   ud.DeviceId = deviceToken;
                                   ud.UserId = _user.Id;
                                   addDevice = _supperClubRepository.AddUserDevice(ud);
                                   if (!addDevice)
                                       LogMessage("API fbLogin: Error adding user device", LogLevel.ERROR);
                                   else
                                       LogMessage("API fbLogin: Added user device successfully", LogLevel.INFO);
                               }
                           }
                           if (result.ContainsKey("friends"))
                           {
                               try
                               {
                                   List<UserFacebookFriend> friendsCollection = new List<UserFacebookFriend>();
                                   System.Collections.ArrayList arrFriends = (System.Collections.ArrayList)result["friends"];
                                   for (int i = 0; i < arrFriends.Count; i++)
                                   {
                                       UserFacebookFriend _userFacebookFriend = new UserFacebookFriend();
                                       _userFacebookFriend.UserId = _user.Id;
                                       _userFacebookFriend.UserFacebookId = fbId;
                                       _userFacebookFriend.FriendFacebookId = ((Dictionary<string, object>)arrFriends[i])["id"].ToString();
                                       friendsCollection.Add(_userFacebookFriend);
                                   }
                                   bool addFacebookFriendStatus = _supperClubRepository.AddUserFacebookFriend(friendsCollection);
                                   if (addFacebookFriendStatus)
                                   {
                                       LogMessage("API fbLogin: Saved user's facebook friends in database", LogLevel.INFO);
                                       List<string> lstFriend = _supperClubRepository.GetFacebookFriendListForPushNotification(_user.Id, fbId);
                                       // Push Notification to Friends
                                       PushNotificationService.MessageService ms = new PushNotificationService.MessageService(_supperClubRepository);
                                       ms.FacebookFriendAppInstallationNotification(result["first_name"].ToString() + " " + result["last_name"].ToString(), _user.Id, fbId);
                                   }
                                   else
                                       LogMessage("API fbLogin: Error saving user's facebook friends in database. fbJson:" + fbJson, LogLevel.ERROR);
                               }
                               catch
                               {
                                   LogMessage("API fbLogin: Error saving user's facebook friends info. fbJson:" + fbJson, LogLevel.ERROR);
                               }
                           }
                        }
                     }                    
                    LogMessage("API fbLogin: FB login complete. Logged In: " + email);
                    return Json(new
                    {
                        Register = false,
                        Login = true,
                        UserId = sa.Encrypt(_user.Id.ToString()),
                        FacebookId = fbId,
                        Email = email,
                        FirstName = result["first_name"].ToString(),
                        LastName = result["last_name"].ToString(),
                        AddDeviceStatus = addDevice
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    currentUser.FBJson = fbJs.ToString();
                    
                    bool success = _supperClubRepository.UpdateUser(currentUser);
                    if (!success)
                    {
                        LogMessage("API fbLogin: Error updating user details." , LogLevel.ERROR);
                    }
                    if (!string.IsNullOrEmpty(deviceToken))
                    {
                        bool deviceExist = _supperClubRepository.CheckUserDevice(deviceToken, currentUser.Id);
                        if (!deviceExist)
                        {
                            // Add user device
                            UserDevice ud = new UserDevice();
                            ud.DeviceId = deviceToken;
                            ud.UserId = currentUser.Id;
                            addDevice = _supperClubRepository.AddUserDevice(ud);
                            if (!addDevice)
                                LogMessage("API fbLogin: Error adding user device", LogLevel.ERROR);
                            else
                                LogMessage("API fbLogin: Added user device successfully", LogLevel.INFO);
                        }
                    }
                    if (result.ContainsKey("friends"))
                    {
                        try
                        {
                            List<UserFacebookFriend> friendsCollection = new List<UserFacebookFriend>();
                            System.Collections.ArrayList arrFriends = (System.Collections.ArrayList)result["friends"];
                            for (int i = 0; i < arrFriends.Count; i++)
                            {
                                UserFacebookFriend _userFacebookFriend = new UserFacebookFriend();
                                _userFacebookFriend.UserId = currentUser.Id;
                                _userFacebookFriend.UserFacebookId = fbId;
                                _userFacebookFriend.FriendFacebookId = ((Dictionary<string, object>)arrFriends[i])["id"].ToString();
                                friendsCollection.Add(_userFacebookFriend);
                            }
                            bool addFacebookFriendStatus = _supperClubRepository.AddUserFacebookFriend(friendsCollection);
                            if (addFacebookFriendStatus)
                                LogMessage("API fbLogin: Saved user's facebook friends in database", LogLevel.INFO);
                            else
                                LogMessage("API fbLogin: Error saving user's facebook friends in database. fbJson:" + fbJson, LogLevel.ERROR);
                        }
                        catch
                        {
                            LogMessage("API fbLogin: Error saving user's facebook friends info. fbJson:" + fbJson, LogLevel.ERROR);
                        }
                    }
                    return Json(new
                    {
                        Register = false,
                        Login = true,
                        UserId = sa.Encrypt(currentUser.Id.ToString()),
                        FacebookId = fbId,
                        Email = email,
                        FirstName = currentUser.FirstName,
                        LastName = currentUser.LastName,
                        ContactNumber = currentUser.ContactNumber,
                        AddDeviceStatus = addDevice
                    }, JsonRequestBehavior.AllowGet);                    
                }
            }
            catch (Exception ex)
            {
                LogMessage("API fbLogin: Error logging in user. ErrorMessage:" + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                return Json(new { error = "FB Login was unsuccessful. Error occurred while registering or logging in user." }, JsonRequestBehavior.AllowGet);
            }
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        [HttpPost]
        public JsonResult resetPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                LogMessage("API resetPassword failed: email address not passed", LogLevel.ERROR);
                return Json(new { status = false, message = "Reset Password failed. E-mail address not passed." }, JsonRequestBehavior.AllowGet);
            }
            if (Membership.FindUsersByEmail(email).Count != 1)
            {
                LogMessage("API resetPassword failed: email address does not exist in the system. Email=" + email, LogLevel.ERROR);
                return Json(new { status = false, message = "Reset Password failed. E-mail address does not exist in the system." }, JsonRequestBehavior.AllowGet);
            }

            MembershipUser aspnetuser = Membership.GetUser(email);
            // Reset a new custom generated password
            string resetPassword = aspnetuser.ResetPassword();
            string newPassword = PasswordGenerator.GeneratePassword();
            try
            {
                // ChangePassword will throw an exception rather
                // than return false in certain failure scenarios.
                aspnetuser.ChangePassword(resetPassword, newPassword);
                EmailService.EmailService es = new EmailService.EmailService(_supperClubRepository);
                bool success = es.SendPasswordResetEmail(aspnetuser.UserName, newPassword, true);
                if (success)
                {
                    LogMessage("API resetPassword: Password reset successfully. E-mail with a new password has been sent.", LogLevel.DEBUG);
                    return Json(new { status= true, message = "Password reset successfully. E-mail with a new password has been sent." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogMessage("API resetPassword: Password reset successfully but e-mail with new password could not be sent.", LogLevel.ERROR);
                    return Json(new { status= false, message = "Password reset successfully but e-mail with new password could not be sent." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage("API resetPassword: error resetting password. Error Message:"+ ex.Message + " Stack Trace:" + ex.StackTrace, LogLevel.ERROR);
                return Json(new { status = false, message = "Password could not be reset." }, JsonRequestBehavior.AllowGet);
            }
       }

        public JsonResult getAvailableTicketsForEvent(int? eventId, int seatingId = 0, string userId = null)
        {
            Guid? _userId = null;
            if (!string.IsNullOrEmpty(userId))
                _userId = new Guid(sa.Decrypt(userId));
            string userid = (_userId == null ? "" : "UserId=" + _userId.ToString() + ". ");
            try
            {
                if (eventId != null && eventId > 0)
                {
                    LogMessage(userid + "API getAvailableTicketsForEvent: " + eventId.ToString());
                    int numOfTickets = getTicketsAvailable((int)eventId, seatingId);
                    if (numOfTickets != -100)
                    {
                        LogMessage(userid + "API getAvailableTicketsForEvent: count fetched successfully.", LogLevel.DEBUG);
                        return Json(new { numberOfTickets = numOfTickets }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        LogMessage(userid + "API getAvailableTicketsForEvent: Error getting ticket availability.", LogLevel.ERROR);
                        return Json(new { error = "Error occurred while getting ticket availability." }, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    LogMessage(userid + "Request failed: Event Id not passed", LogLevel.ERROR);
                    return Json(new { error = "Request failed: Event Id not passed. Please pass Event Id as part of request." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage(userid + "API getAvailableTicketsForEvent Exception while processing request. Exception Details - Message:" + ex.Message + " StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                return Json(new { error = "Error occurred while processing request" }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult testPushNotification(string notificationType, string token, int eventId = 0)
        {
            try
            {
                if (string.IsNullOrEmpty(notificationType) || string.IsNullOrEmpty(token))
                {
                    LogMessage("API testPushNotification: notificationType or deviceToken is not passed", LogLevel.ERROR);
                    return Json(new { success = false, message = "Request Failed: notificationType or token is not passed." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    if (notificationType != "FB_FRIEND_INSTALLED_APP" && eventId <= 0)
                    {
                        LogMessage("API testPushNotification: eventId is not passed", LogLevel.ERROR);
                        return Json(new { success = false, message = "Request Failed: eventId is not passed. It is required for notification type \"" + notificationType + "\"" }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        LogMessage("API testPushNotification: Satrting to push notification. NotificationType=" + notificationType + " Device Token=" + token);
                        int templateId = 0;
                        switch (notificationType)
                        {
                            case "FB_FRIEND_INSTALLED_APP":
                                templateId = 1;
                                break;
                            case "FB_FRIEND_BOOKED_TICKET":
                                templateId = 2;
                                break;
                            case "FAV_CHEF_NEW_EVENT_NOTIFICATION":
                                templateId = 3;
                                break;
                            case "FAV_EVENT_BOOKING_REMINDER":
                                templateId = 4;
                                break;
                            case "WAITLIST_EVENT_TICKETS_AVAILABLE":
                                templateId = 5;
                                break;
                        }
                        PushNotificationService.MessageService ms = new PushNotificationService.MessageService(_supperClubRepository);
                        Dictionary<string, object> status = ms.testNotification(templateId, token, eventId);
                        return Json(status, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage("API testPushNotification Exception while processing request. Exception Details - Message:" + ex.Message + " StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                return Json(new { success = false, message = "Error occurred while processing request" }, JsonRequestBehavior.AllowGet);
            }
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        [HttpPost]
        public JsonResult saveUserContactNumber(string contactNumber, string userId)
        {
            Guid? _userId = null;
            if (!string.IsNullOrEmpty(userId))
                _userId = new Guid(sa.Decrypt(userId));
            string userid = (_userId == null ? "" : "UserId=" + _userId.ToString() + ". ");
            bool success = false;

            try
            {
                if (_userId == null)
                {
                    LogMessage(userid + "API saveUserContactNumber - userId was not passed.", LogLevel.ERROR);
                    return Json(new { error = "Request failed: userId was not passed." }, JsonRequestBehavior.AllowGet);
                }
                if (!_supperClubRepository.IsValidUser((Guid)_userId))
                {
                    LogMessage(userid + "API saveUserContactNumber: Invalid UserId. UserId=" + _userId.ToString(), LogLevel.ERROR);
                    return Json(new { error = "Request failed: Invalid UserId" }, JsonRequestBehavior.AllowGet);
                }
                User _user = _supperClubRepository.GetUser(_userId);
                if (_user == null)
                {
                    LogMessage(userid + "API saveUserContactNumber - Invalid userId", LogLevel.ERROR);
                    return Json(new { error = "Request failed: Invalid User Id " + _userId.ToString() }, JsonRequestBehavior.AllowGet);
                }
                else if (string.IsNullOrEmpty(contactNumber) || contactNumber.Length < 10 || !isValidPhoneNumber(contactNumber))
                {
                    LogMessage(userid + "API saveUserContactNumber - Invalid phone number " + contactNumber, LogLevel.ERROR);
                    return Json(new { error = "Request failed: Invalid Phone Number " + contactNumber }, JsonRequestBehavior.AllowGet);
                }
                else
                {

                    contactNumber = contactNumber.Replace(" ", "").Replace("(", "").Replace(")", "").Replace("+", "").Replace("-", "").Replace(".", "");
                    contactNumber = contactNumber.Substring(contactNumber.Length - 10);
                    double number = 0;
                    bool saveNumber = false;
                    if (double.TryParse(contactNumber, out number))
                        saveNumber = true;
                    if (saveNumber)
                    {
                        _user.ContactNumber = contactNumber;
                        success = _supperClubRepository.UpdateUser(_user);
                        if (!success)
                        {
                            LogMessage(userid + "API saveUserContactNumber: Could not save contact number in Database. Contact Number:" + contactNumber, LogLevel.ERROR);
                            return Json(new { error = "Error occurred while saving contact number " + contactNumber }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new {
                                userId = sa.Encrypt(_user.Id.ToString()),
                            userFirstName = _user.FirstName,
                            userLastName = _user.LastName,
                            userEmail = _user.Email,
                            userFBId = _user.FacebookId,
                            userContactNumber = _user.ContactNumber
                            }, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        LogMessage(userid + "API saveUserContactNumber: Contact number entered was not in a correct format. Contact Number:" + contactNumber, LogLevel.ERROR);
                        return Json(new { error = "Error occurred while saving contact number " + contactNumber }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage(userid + "API saveUserContactNumber: Error occurred while saving contact number. " + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                return Json(new { error = "Error occurred while saving contact number." }, JsonRequestBehavior.AllowGet);
            }
        }
        
        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        [HttpPost]
        public JsonResult createTicketBasket(string userId, int? eventId, dynamic bookingDetails, string specialRequirements = null, int seatingId = 0)
        {
            Guid? _userId = null;
            if (!string.IsNullOrEmpty(userId))
                _userId = new Guid(sa.Decrypt(userId));
            string userid = (_userId == null ? "" : "UserId=" + _userId.ToString() + ". ");
            
            try
            {
                LogMessage(userid + "API createTicketBasket request");
                Event _event = null;
                if (_userId == null)
                {
                    LogMessage(userid + "API createTicketBasket - userId was not passed.", LogLevel.ERROR);
                    return Json(new { error = "Request failed: userId was not passed." }, JsonRequestBehavior.AllowGet);
                }
                if (!_supperClubRepository.IsValidUser((Guid)_userId))
                {
                    LogMessage(userid + "API createTicketBasket: Invalid UserId. UserId=" + _userId.ToString(), LogLevel.ERROR);
                    return Json(new { error = "Request failed: Invalid UserId" }, JsonRequestBehavior.AllowGet);
                }
                if (eventId == null || eventId <= 0)
                {
                    LogMessage(userid + "API createTicketBasket - event Id was not passed.", LogLevel.ERROR);
                    return Json(new { error = "Request failed: Event Id was not passed." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _event = _supperClubRepository.GetEvent((int)eventId);
                    if (_event == null)
                    {
                        LogMessage(userid + "API createTicketBasket - invalid event Id was passed.", LogLevel.ERROR);
                        return Json(new { error = "Request failed: Invalid Event Id." }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        if (_event.Start < DateTime.Now)
                        {
                            LogMessage(userid + "API createTicketBasket - event has already elapsed.", LogLevel.ERROR);
                            return Json(new { error = "Request failed: Basket cannot be created for past event." }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                if (seatingId > 0)
                {
                    if (!_event.MultiSeating)
                    {
                        LogMessage(userid + "API createTicketBasket - Invalid Seating Id. This event does not have multiple seatings.", LogLevel.ERROR);
                        return Json(new { error = "Request failed: Invalid Seating Id " + seatingId + ". This event does not have multiple seatings." }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        EventSeating es = _supperClubRepository.GetEventSeating(seatingId);
                        if (es == null || es.EventId != eventId)
                        {
                            LogMessage(userid + "API createTicketBasket - Invalid Seating Id" + seatingId, LogLevel.ERROR);
                            return Json(new { error = "Request failed: Invalid Seating Id " + seatingId + " or this Seating Id is not linked to this event." }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                else
                {
                    if (_event.MultiSeating)
                    {
                        LogMessage(userid + "API createTicketBasket - Invalid Seating Id. This event has multiple seatings.", LogLevel.ERROR);
                        return Json(new { error = "Request failed: Invalid Seating Id " + seatingId + ". This event has multiple seatings." }, JsonRequestBehavior.AllowGet);
                    }
                }
                User _user = _supperClubRepository.GetUser(_userId);
                if (_user == null)
                {
                    LogMessage(userid + "API createTicketBasket - User Id not exist for Id: " + _userId.ToString(), LogLevel.ERROR);
                    return Json(new { error = "Request failed: Invalid User Id " + _userId.ToString() }, JsonRequestBehavior.AllowGet);
                }
                if (bookingDetails == null)
                {
                    LogMessage(userid + "API createTicketBasket - booking details were not passed.", LogLevel.ERROR);
                    return Json(new { error = "Request failed: booking details not passed." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    List<BookingParameterModel> bd = new List<BookingParameterModel>();
                    JavaScriptSerializer jss = new JavaScriptSerializer();
                    dynamic bDetails = null;
                    try
                    {
                        bDetails = bookingDetails;
                    }
                    catch
                    {
                        bDetails = bookingDetails[0];
                    }
                    
                    //var d = jss.Deserialize<dynamic>(bDetails);

                    dynamic d = null;
                    try
                    {
                        d = jss.Deserialize<dynamic>(bDetails);
                    }
                    catch
                    {
                        LogMessage("API createTicketBasket failed: Error parsing bookingDetails", LogLevel.ERROR);
                        return Json(new { error = "createTicketBasket failed: Error parsing bookingDetails." }, JsonRequestBehavior.AllowGet);
                    }
                    foreach (var t in d)
                    {
                        BookingParameterModel bpm = new BookingParameterModel();
                        bpm.menuOptionId = (int)t["menuOptionId"];
                        bpm.numberOfTickets = (int)t["numberOfTickets"];

                        if (bpm.menuOptionId > 0 && !_event.MultiMenuOption)
                        {
                            LogMessage(userid + "API createTicketBasket - Invalid Menu Option Id. This event does not have multiple menu options.", LogLevel.ERROR);
                            return Json(new { error = "Request failed: Invalid Menu Option Id " + bpm.menuOptionId + ". This event does not have multiple menu options." }, JsonRequestBehavior.AllowGet);                                
                        }
                        else
                        {
                            if (bpm.menuOptionId > 0)
                            {
                                EventMenuOption emo = _supperClubRepository.GetEventMenuOption(bpm.menuOptionId);
                                if (emo == null || emo.EventId != eventId)
                                {
                                    LogMessage(userid + "API createTicketBasket - Invalid Menu Option Id.", LogLevel.ERROR);
                                    return Json(new { error = "Request failed: Invalid Menu Option Id " + bpm.menuOptionId }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                    bpm.baseTicketCost = emo.Cost;
                            }
                            else
                                bpm.baseTicketCost = _event.Cost;

                            bd.Add(bpm);
                        }
                    }

                    int TicketBasketSessionTimeOut = int.Parse(WebConfigurationManager.AppSettings["TicketBasketSessionTimeOut"]);
                    Tuple<int, int> result = _supperClubRepository.CleanUpAbandonedBaskets(TicketBasketSessionTimeOut + 1);
                    LogMessage(string.Format(userid + "API createTicketBasket: Flushed {0} Expired Baskets ({1} tickets)", result.Item1, result.Item2), LogLevel.DEBUG);
                    Guid BasketId = Guid.NewGuid();

                    TicketBasket basket = _supperClubRepository.CreateBasket(BasketId, (Guid)_userId, _user.Email);
                    basket.BookingRequirements = specialRequirements;
                    _supperClubRepository.UpdateTicketBasket(basket, TicketBasketStatus.InProgress);
                    LogMessage(string.Format(userid + "API createTicketBasket: Basket Created. Basket Id: {0}", BasketId), LogLevel.DEBUG);

                    AddTicketsStatusModel statusModel = new AddTicketsStatusModel();
                    statusModel = addTicketsToBasket(bd, (int)eventId, BasketId, true, (Guid)_userId, seatingId, _event.Commission);

                    
                    if (!statusModel.Success)
                    {
                        LogMessage(userid + "API createTicketBasket - "+ statusModel.Message, LogLevel.ERROR);
                        _supperClubRepository.CleanUpBaskets(BasketId);
                        return Json(new { error = statusModel.Message}, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        LogMessage(userid + string.Format("API createTicketBasket: Added {1} ticket/s to Basket Id: {0}", BasketId, statusModel.NumberOfTicketsAdded), LogLevel.DEBUG);
                        return Json(new
                        {
                            BasketId = BasketId,
                            NumberOfTicketsInBasket = statusModel.NumberOfTicketsAdded,
                            TicketBasketSessionTimeOut = TicketBasketSessionTimeOut,
                            BasketLastUpdateDate = DateConverter.GetUTCTime(basket.LastUpdated),
                            UserId = sa.Encrypt(_userId.ToString()),
                            UserEmail = _user.Email
                        }, JsonRequestBehavior.AllowGet);   
                    }
                }                    
            }    
            catch (Exception ex)
            {
                LogMessage(userid + "API createTicketBasket: Error creating ticket basket." + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                return Json(new { error = "Error occurred while creating ticket basket." }, JsonRequestBehavior.AllowGet);
            }
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        [HttpPost]
        public JsonResult updateTicketBasket(string userId, int? eventId, Guid? basketId, dynamic bookingDetails, int numberOfTicketsRequested, string specialRequirements = null, int seatingId = 0, int voucherId = 0)
        {
            Guid? _userId = null;
            if (!string.IsNullOrEmpty(userId))
                _userId = new Guid(sa.Decrypt(userId));
            string userid = (_userId == null ? "" : "UserId=" + _userId.ToString() + ". ");
            AddTicketsStatusModel statusModel = new AddTicketsStatusModel();
            statusModel.NumberOfTicketsAdded = 0;
            try
            {
                LogMessage(userid + "API updateTicketBasket request");
                Event _event = null;
                if (_userId == null)
                {
                    LogMessage(userid + "API updateTicketBasket - userId was not passed.", LogLevel.ERROR);
                    statusModel.Message = "Request failed: userId was not passed.";
                    return Json(statusModel, JsonRequestBehavior.AllowGet);
                }
                if (!_supperClubRepository.IsValidUser((Guid)_userId))
                {
                    LogMessage(userid + "API updateTicketBasket: Invalid UserId. UserId=" + _userId.ToString(), LogLevel.ERROR);
                    statusModel.Message = "Request failed: Invalid UserId.";
                    return Json(statusModel, JsonRequestBehavior.AllowGet);                   
                }
                if (eventId == null || eventId <= 0)
                {
                    LogMessage(userid + "API updateTicketBasket - event Id was not passed.", LogLevel.ERROR);
                    statusModel.Message = "Request failed: Event Id was not passed.";
                    return Json(statusModel, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _event = _supperClubRepository.GetEvent((int)eventId);
                    if (_event == null)
                    {
                        LogMessage(userid + "API updateTicketBasket - invalid event Id was passed.", LogLevel.ERROR);
                        statusModel.Message = "Request failed: Invalid Event Id.";
                        return Json(statusModel, JsonRequestBehavior.AllowGet);                        
                    }
                    else
                    {
                        if (_event.Start < DateTime.Now)
                        {
                            LogMessage(userid + "API updateTicketBasket - event has already elapsed.", LogLevel.ERROR);
                            statusModel.Message = "Request failed: Basket can not be updated for past event.";
                            return Json(statusModel, JsonRequestBehavior.AllowGet);  
                        }
                    }
                }
                if (seatingId > 0)
                {
                    if (!_event.MultiSeating)
                    {
                        LogMessage(userid + "API updateTicketBasket - Invalid Seating Id. This event does not have multiple seatings.", LogLevel.ERROR);
                        statusModel.Message = "Request failed: Invalid Seating Id " + seatingId + ". This event does not have multiple seatings.";
                        return Json(statusModel, JsonRequestBehavior.AllowGet);                          
                    }
                    else
                    {
                        EventSeating es = _supperClubRepository.GetEventSeating(seatingId);
                        if (es == null || es.EventId != eventId)
                        {
                            LogMessage(userid + "API updateTicketBasket - Invalid Seating Id" + seatingId, LogLevel.ERROR);
                            statusModel.Message = "Request failed: Invalid Seating Id " + seatingId + " or this Seating Id is not linked to this event.";
                            return Json(statusModel, JsonRequestBehavior.AllowGet); 
                        }
                    }
                }
                else
                {
                    if (_event.MultiSeating)
                    {
                        LogMessage(userid + "API updateTicketBasket - Invalid Seating Id. This event has multiple seatings.", LogLevel.ERROR);
                        statusModel.Message = "Request failed: Invalid Seating Id " + seatingId + ". This event has multiple seatings.";
                        return Json(statusModel, JsonRequestBehavior.AllowGet); 
                    }
                }
                User _user = _supperClubRepository.GetUser(_userId);
                if (_user == null)
                {
                    LogMessage(userid + "API updateTicketBasket - User Id does not exist for Id: " + _userId.ToString(), LogLevel.ERROR);
                    statusModel.Message = "Request failed: Invalid User Id ";
                    return Json(statusModel, JsonRequestBehavior.AllowGet);
                }
                if (basketId == null)
                {
                    LogMessage(userid + "API updateTicketBasket - basketId was not passed.", LogLevel.ERROR);
                    statusModel.Message = "Request failed: basketId was not passed.";
                    return Json(statusModel, JsonRequestBehavior.AllowGet);
                }
                TicketBasket _ticketBasket = _supperClubRepository.GetExistingBasket((Guid)basketId);
                if (_ticketBasket == null)
                {
                    LogMessage(userid + "API updateTicketBasket - Basket Id does not exist for Id: " + basketId.ToString(), LogLevel.ERROR);
                    statusModel.Message = "Request failed: Invalid Basket Id " + basketId.ToString();
                    return Json(statusModel, JsonRequestBehavior.AllowGet);
                }
                else 
                {
                    int TicketBasketSessionTimeOut = int.Parse(WebConfigurationManager.AppSettings["TicketBasketSessionTimeOut"]);
                    DateTime expired = DateTime.Now.AddMinutes(-TicketBasketSessionTimeOut);
                    string inProgressStatus = TicketBasketStatus.InProgress.ToString();
                    if (_ticketBasket.LastUpdated < expired && _ticketBasket.Status == inProgressStatus)
                    {
                        Tuple<int, int> result = _supperClubRepository.CleanUpAbandonedBaskets(TicketBasketSessionTimeOut);
                        LogMessage(string.Format(userid + "API updateTicketBasket: Flushed {0} Expired Baskets ({1} tickets)", result.Item1, result.Item2), LogLevel.DEBUG);                    
                        LogMessage(userid + "API updateTicketBasket - Basket Expired.", LogLevel.ERROR);
                        statusModel.Message = "Request failed: Ticket basket has expired." + basketId.ToString();
                        return Json(statusModel, JsonRequestBehavior.AllowGet);
                    }
                }               

                _ticketBasket.BookingRequirements = specialRequirements;
                TicketBasket tb = _supperClubRepository.UpdateTicketBasket(_ticketBasket, TicketBasketStatus.InProgress);
                _ticketBasket = tb;

                if (bookingDetails == null)
                {
                    LogMessage(userid + "API updateTicketBasket - booking details were not passed.", LogLevel.ERROR);
                    statusModel.Message = "Request failed: booking details not passed.";
                    return Json(statusModel, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    List<BookingParameterModel> bd = new List<BookingParameterModel>();
                    JavaScriptSerializer jss = new JavaScriptSerializer();
                    dynamic bDetails = null;
                    try
                    {
                        bDetails = bookingDetails;
                    }
                    catch
                    {
                        bDetails = bookingDetails[0];
                    }

                    //var d = jss.Deserialize<dynamic>(bDetails);

                    dynamic d = null;
                    try
                    {
                        d = jss.Deserialize<dynamic>(bDetails);
                    }
                    catch
                    {
                        LogMessage("API updateTicketBasket failed: Error parsing bookingDetails", LogLevel.ERROR);
                        statusModel.Message = "Request failed: Error parsing bookingDetails.";
                        return Json(statusModel, JsonRequestBehavior.AllowGet);                        
                    }
                    foreach (var t in d)
                    {
                        BookingParameterModel bpm = new BookingParameterModel();
                        bpm.menuOptionId = t["menuOptionId"];
                        bpm.numberOfTickets = t["numberOfTickets"];

                        if (bpm.menuOptionId > 0 && !_event.MultiMenuOption)
                        {
                            LogMessage(userid + "API updateTicketBasket - Invalid Menu Option Id. This event does not have multiple menu options.", LogLevel.ERROR);
                            statusModel.Message = "Request failed: Invalid Menu Option Id " + bpm.menuOptionId + ". This event does not have multiple menu options.";
                            return Json(statusModel, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            if (bpm.menuOptionId > 0)
                            {
                                EventMenuOption emo = _supperClubRepository.GetEventMenuOption(bpm.menuOptionId);
                                if (emo == null || emo.EventId != eventId)
                                {
                                    LogMessage(userid + "API updateTicketBasket - Invalid Menu Option Id.", LogLevel.ERROR);
                                    statusModel.Message = "Request failed: Invalid Menu Option Id " + bpm.menuOptionId;
                                    return Json(statusModel, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    bpm.baseTicketCost = emo.Cost;                                    
                                }
                            }
                            else
                            {
                                bpm.baseTicketCost = _event.Cost;                                
                            }
                            bd.Add(bpm);
                        }
                    }
                    int currentBlockedTicketsForUser = _supperClubRepository.GetNumberTicketsInProgressForUser((int)eventId, seatingId, (Guid)_userId);
                    int availableTicketsForCurrentUser = getTicketsAvailable((int)eventId, seatingId) + currentBlockedTicketsForUser;
                    if (numberOfTicketsRequested > availableTicketsForCurrentUser)
                    {
                        statusModel.Success = false;
                        if (seatingId > 0)
                            statusModel.Message = "Sorry but we were not able to allocate the number of tickets you wanted for this seating. You can try to book a different seating!";
                        else
                            statusModel.Message = "Sorry but we were not able to allocate the number of tickets you wanted. They've been snapped up!";
                    }
                    else
                    {
                        availableTicketsForCurrentUser = numberOfTicketsRequested;
                        int numberTicketsToRemove = _ticketBasket.Tickets.Count;
                        if (numberTicketsToRemove > 0)
                        {
                            TicketBasket basket = _supperClubRepository.RemoveFromBasket(_ticketBasket.Tickets);
                            LogMessage(string.Format("Removed {0} tickets from Basket Id: {1}", numberTicketsToRemove, basketId), LogLevel.DEBUG);
                        }
                        statusModel = addTicketsToBasket(bd, (int)eventId, (Guid)basketId, false, (Guid)_userId, seatingId, _event.Commission, voucherId);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage("API updateTicketBasket: Error updating ticket basket." + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                statusModel.Success = false;
                statusModel.Message = "Error occurred updating the ticket basket.";
            }
            return Json(statusModel, JsonRequestBehavior.AllowGet);
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        [HttpPost]
        public JsonResult applyVoucherCode(string userId, int? eventId, Guid? basketId, string voucherCode)
        {
            Guid? _userId = null;
            if (!string.IsNullOrEmpty(userId))
                _userId = new Guid(sa.Decrypt(userId));
            string userid = (_userId == null ? "" : "UserId=" + _userId.ToString() + ". ");

            try
            {
                LogMessage(userid + "API applyVoucherCode request");
                Event _event = null;
                if (_userId == null)
                {
                    LogMessage(userid + "API applyVoucherCode - userId was not passed.", LogLevel.ERROR);
                    return Json(new { Status = false, VoucherId = 0, Description = "Request failed: userId was not passed." }, JsonRequestBehavior.AllowGet);
                }
                if (!_supperClubRepository.IsValidUser((Guid)_userId))
                {
                    LogMessage(userid + "API applyVoucherCode: Invalid UserId. UserId=" + _userId.ToString(), LogLevel.ERROR);
                    return Json(new { Status = false, VoucherId = 0, Description = "Request failed: Invalid UserId" }, JsonRequestBehavior.AllowGet);
                }
                if (eventId == null || eventId <= 0)
                {
                    LogMessage(userid + "API applyVoucherCode - event Id was not passed.", LogLevel.ERROR);
                    return Json(new { Status = false, VoucherId = 0, Description = "Request failed: Event Id was not passed." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    _event = _supperClubRepository.GetEvent((int)eventId);
                    if (_event == null)
                    {
                        LogMessage(userid + "API applyVoucherCode - invalid event Id was passed.", LogLevel.ERROR);
                        return Json(new { Status = false, VoucherId = 0, Description = "Request failed: Invalid Event Id." }, JsonRequestBehavior.AllowGet);
                    }
                }
                User _user = _supperClubRepository.GetUser(_userId);
                if (_user == null)
                {
                    LogMessage(userid + "API applyVoucherCode - User Id not exist for Id: " + _userId.ToString(), LogLevel.ERROR);
                    return Json(new { Status = false, VoucherId = 0, Description = "Request failed: Invalid User Id " + _userId.ToString() }, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(voucherCode))
                {
                    LogMessage(userid + "API applyVoucherCode - Voucher Code was not passed.", LogLevel.ERROR);
                    return Json(new { Status = false, VoucherId = 0, Description = "Request failed: Voucher Code not passed." }, JsonRequestBehavior.AllowGet);
                }
                if (basketId == null)
                {
                    LogMessage(userid + "API applyVoucherCode - basketId was not passed.", LogLevel.ERROR);
                    return Json(new { Status = false, VoucherId = 0, Description = "Request failed: basketId was not passed." }, JsonRequestBehavior.AllowGet);
                }
                TicketBasket _ticketBasket = _supperClubRepository.GetExistingBasket((Guid)basketId);
                if (_ticketBasket == null)
                {
                    LogMessage(userid + "API applyVoucherCode - Basket Id does not exist for Id: " + basketId.ToString(), LogLevel.ERROR);
                    return Json(new { Status = false, VoucherId = 0, Description = "Request failed: Invalid Basket Id" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    int TicketBasketSessionTimeOut = int.Parse(WebConfigurationManager.AppSettings["TicketBasketSessionTimeOut"]);
                    DateTime expired = DateTime.Now.AddMinutes(-TicketBasketSessionTimeOut);
                    string inProgressStatus = TicketBasketStatus.InProgress.ToString();
                    if (_ticketBasket.LastUpdated < expired && _ticketBasket.Status == inProgressStatus)
                    {
                        Tuple<int, int> result = _supperClubRepository.CleanUpAbandonedBaskets(TicketBasketSessionTimeOut);
                        LogMessage(string.Format(userid + "API applyVoucherCode: Flushed {0} Expired Baskets ({1} tickets)", result.Item1, result.Item2), LogLevel.DEBUG);
                        LogMessage(userid + "API applyVoucherCode - Basket Expired.", LogLevel.ERROR);
                        return Json(new { Status = false, VoucherId = 0, Description = "Request failed: Ticket basket has expired" }, JsonRequestBehavior.AllowGet);
                    }
                }

                int voucherId = _supperClubRepository.CheckVoucherCode(voucherCode, (int)eventId, (Guid)_userId, _ticketBasket.TotalTickets, _ticketBasket.TotalPrice);
                string errorMessage = "";
                if (voucherId <= 0)
                {
                    switch (voucherId)
                    {
                        case 0:
                            errorMessage = "Invalid Voucher Code. Please enter a valid voucher code.";
                            break;
                        case -1:
                            errorMessage = "This Voucher Code has expired.";
                            break;
                        case -2:
                            errorMessage = "This Voucher Code is not valid for current booking.";
                            break;
                        case -3:
                            errorMessage = "You have already used this voucher for previous bookings.";
                            break;
                        case -4:
                            errorMessage = "This Voucher Code has expired.";
                            break;
                        case -5:
                            errorMessage = "This voucher code can not be applied to your basket. Minimum basket value is more than current basket value.";
                            break;
                        case -6:
                            errorMessage = "This Gift Voucher does not have any balance left.";
                            break;
                        default:
                            errorMessage = "Error applying voucher code. Please try again.";
                            break;
                    }
                    LogMessage(userid + "API applyVoucherCode - Voucher can not be applied to the basket. Reason: " + errorMessage, LogLevel.ERROR);

                    if (voucherId == 0)
                        return Json(new { Status = false, VoucherId = 0, Description = "", ErrorMessage = errorMessage }, JsonRequestBehavior.AllowGet);
                    else
                    {
                        Voucher voucher = _supperClubRepository.GetVoucher(voucherCode);
                        if(voucher != null)
                            return Json(new { Status = false, VoucherId = voucher.Id, Description = voucher.Description, ErrorMessage = errorMessage }, JsonRequestBehavior.AllowGet);
                        else
                            return Json(new { Status = false, VoucherId = 0, Description = "", ErrorMessage = errorMessage }, JsonRequestBehavior.AllowGet);
                    }
                }
                Voucher _voucher = _supperClubRepository.GetVoucher(voucherId);
                return Json(new { Status = true, VoucherId = voucherId, Description = _voucher.Description, ErrorMessage = "" }, JsonRequestBehavior.AllowGet);
                
            }
            catch (Exception ex)
            {
                LogMessage(userid + "API applyVoucherCode: Error applying voucher code to basket" + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                return Json(new { Status = false, VoucherId = 0, Description = "", ErrorMessage = "Error occurred while applying voucher code to basket." }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult getVoucherDetails(string voucherCode, string userId, Guid basketId, int eventId = 0)
        {            
            try
            {
                if (string.IsNullOrEmpty(voucherCode))
                {
                    LogMessage("API getVoucherDetails - Voucher Code was not passed.", LogLevel.ERROR);
                    return Json(new { Error = "Request failed: Voucher Code not passed." }, JsonRequestBehavior.AllowGet);
                }
                if (eventId <= 0)
                {
                    LogMessage("API getVoucherDetails - Event Id was not passed.", LogLevel.ERROR);
                    return Json(new { Error = "Request failed: Event Id not passed." }, JsonRequestBehavior.AllowGet);
                }
                Guid? _userId = null;
                if (!string.IsNullOrEmpty(userId))
                    _userId = new Guid(sa.Decrypt(userId));
                if (_userId == null)
                {
                    LogMessage("API getVoucherDetails - User Id was not passed.", LogLevel.ERROR);
                    return Json(new { Error = "Request failed: User Id not passed." }, JsonRequestBehavior.AllowGet);
                }
                if (!_supperClubRepository.IsValidUser((Guid)_userId))
                {
                    LogMessage("API getVoucherDetails: Invalid UserId. UserId=" + _userId.ToString(), LogLevel.ERROR);
                    return Json(new { Error = "Request failed: Invalid UserId" }, JsonRequestBehavior.AllowGet);
                }
                if (basketId == null)
                {
                    LogMessage("API getVoucherDetails - Basket Id was not passed.", LogLevel.ERROR);
                    return Json(new { Error = "Request failed: Basket Id not passed." }, JsonRequestBehavior.AllowGet);
                }
                TicketBasket _ticketBasket = _supperClubRepository.GetExistingBasket((Guid)basketId);
                if (_ticketBasket == null)
                {
                    LogMessage("API getVoucherDetails - Basket Id does not exist for Id: " + basketId.ToString(), LogLevel.ERROR);
                    return Json(new { Error = "Request failed: Invalid Basket Id" }, JsonRequestBehavior.AllowGet);
                }
                int TicketBasketSessionTimeOut = int.Parse(WebConfigurationManager.AppSettings["TicketBasketSessionTimeOut"]);
                DateTime expired = DateTime.Now.AddMinutes(-TicketBasketSessionTimeOut);
                string inProgressStatus = TicketBasketStatus.InProgress.ToString();
                if (_ticketBasket.LastUpdated < expired && _ticketBasket.Status == inProgressStatus)
                {
                    Tuple<int, int> result = _supperClubRepository.CleanUpAbandonedBaskets(TicketBasketSessionTimeOut);
                    LogMessage(string.Format("API getVoucherDetails: Flushed {0} Expired Baskets ({1} tickets)", result.Item1, result.Item2), LogLevel.DEBUG);
                    LogMessage("API getVoucherDetails - Basket Expired.", LogLevel.ERROR);
                    return Json(new { Error = "Request failed: Ticket basket has expired" }, JsonRequestBehavior.AllowGet);
                }
                int voucherId = _supperClubRepository.CheckVoucherCode(voucherCode, eventId, (Guid)_userId, _ticketBasket.TotalTickets, _ticketBasket.TotalPrice);
                string errorMessage = "";
                if (voucherId <= 0)
                {
                    switch (voucherId)
                    {
                        case 0:
                            errorMessage = "Invalid Voucher Code. Please enter a valid voucher code.";
                            break;
                        case -1:
                            errorMessage = "This Voucher Code has expired.";
                            break;
                        case -2:
                            errorMessage = "This Voucher Code is not valid for current booking.";
                            break;
                        case -3:
                            errorMessage = "You have already used this voucher for previous bookings.";
                            break;
                        case -4:
                            errorMessage = "This Voucher Code has expired.";
                            break;
                        case -5:
                            errorMessage = "This voucher code can not be applied to your basket. Minimum basket value is more than current basket value.";
                            break;
                        case -6:
                            errorMessage = "This Gift Voucher does not have any balance left.";
                            break;
                        default:
                            errorMessage = "Error applying voucher code. Please try again.";
                            break;
                    }
                    return Json(new { IsValid = false, IsActive = false, VoucherId = 0, VoucherType = "", VoucherDescription = errorMessage, Value = 0 }, JsonRequestBehavior.AllowGet);                
                }
                
                Voucher _voucher = _supperClubRepository.GetVoucher(voucherId);
                if (_voucher != null)
                    return Json(new { IsValid = true, IsActive = true, VoucherId = _voucher.Id, VoucherType = (_voucher.TypeId == 1 ? "PERCENT_OFF" : "VALUE_OFF"), VoucherDescription = _voucher.Description, Value = _voucher.OffValue }, JsonRequestBehavior.AllowGet);
                else
                    return Json(new { IsValid = false, IsActive = false, VoucherId = 0, VoucherType = "", VoucherDescription = "", Value = 0 }, JsonRequestBehavior.AllowGet);
                                
                //if (voucherCode == "PERCENT10")
                //{
                //    return Json(new { IsValid = true, IsActive = true, VoucherId = 1, VoucherType = "PERCENT_OFF", VoucherDescription = "10% OFF", Value = 10 }, JsonRequestBehavior.AllowGet);
                //}S
                //if (voucherCode == "VALUE5")
                //{
                //    return Json(new { IsValid = true, IsActive = false, VoucherId = 2, VoucherType = "VALUE_OFF", VoucherDescription = "£5 OFF", Value = 5 }, JsonRequestBehavior.AllowGet);
                //}
                //if (voucherCode == "INACTIVE")
                //{
                //    return Json(new { IsValid = true, IsActive = false, VoucherId = 3, VoucherType = "VALUE_OFF", VoucherDescription = "£5 OFF", Value = 5 }, JsonRequestBehavior.AllowGet);
                //}
                //else
                //{
                //    return Json(new { IsValid = false, IsActive = false, VoucherId = 0, VoucherType = "", VoucherDescription = "", Value = 0}, JsonRequestBehavior.AllowGet);
                //}
            }
            catch (Exception ex)
            {
                LogMessage("API getVoucherDetails: Error getting voucher details" + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                return Json(new { Error = "Error occurred while getting voucher details" }, JsonRequestBehavior.AllowGet);
            }
        }
        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        [HttpPost]
        public JsonResult clientToken(string userId)
        {
            Guid? _userId = null;
            if (!string.IsNullOrEmpty(userId))
                _userId = new Guid(sa.Decrypt(userId));
            string userid = (_userId == null ? "" : "UserId=" + _userId.ToString() + ". ");

            try
            {
                LogMessage(userid + "API clientToken request");
                if (_userId == null)
                {
                    LogMessage(userid + "API clientToken - userId was not passed.", LogLevel.ERROR);
                    return Json(new { Success = false, ClientToken = "Request failed: userId was not passed." }, JsonRequestBehavior.AllowGet);
                }
                if (!_supperClubRepository.IsValidUser((Guid)_userId))
                {
                    LogMessage(userid + "API clientToken: Invalid UserId. UserId=" + _userId.ToString(), LogLevel.ERROR);
                    return Json(new { Success = false, ClientToken = "Request failed: Invalid UserId" }, JsonRequestBehavior.AllowGet);
                }
                User user = _supperClubRepository.GetUser((Guid)_userId);
                BraintreeCustomer customer = _supperClubRepository.GetBraintreeCustomer((Guid)_userId);
                if (braintreeEnvironmentSetting.ToLower() == "live")
                    braintreeEnvironment = Braintree.Environment.PRODUCTION;

                var gateway = new BraintreeGateway
                {
                    Environment = braintreeEnvironment,
                    MerchantId = braintreeMerchantId,
                    PublicKey = braintreePublicKey,
                    PrivateKey = braintreePrivateKey
                };
                if(customer == null)
                {
                    if (user != null)
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
                                var clientToken = gateway.ClientToken.generate(
                                     new ClientTokenRequest
                                     {
                                         CustomerId = customerId,
                                         Options = new ClientTokenOptionsRequest
                                         {
                                             VerifyCard = true
                                         }
                                     });
                                if (clientToken != null)
                                {
                                    LogMessage(userid + "API clientToken - Client token generated. Token=" + clientToken.ToString());
                                    return Json(new { Success = true, CustomerSuccess = true, ClientToken = clientToken }, JsonRequestBehavior.AllowGet);
                                }
                                else
                                {
                                    LogMessage("API clientToken: Error generating client token", LogLevel.ERROR);
                                    return Json(new { Success = false, CustomerSuccess = true, ClientToken = "Error occurred while generating client token" }, JsonRequestBehavior.AllowGet);
                                }
                            }                            
                        }                        
                    }                    
                    var _clientToken = gateway.ClientToken.generate();
                    if (_clientToken != null)
                    {
                        LogMessage(userid + "API clientToken - Client token generated. Token=" + _clientToken.ToString());
                        return Json(new { Success = true, CustomerSuccess = false, ClientToken = _clientToken }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        LogMessage("API clientToken: Error generating client token", LogLevel.ERROR);
                        return Json(new { Success = false, CustomerSuccess = false, ClientToken = "Error occurred while generating client token" }, JsonRequestBehavior.AllowGet);
                    }                   
                }
                else
                {
                    var clientToken = gateway.ClientToken.generate(
                                         new ClientTokenRequest
                                         {
                                             CustomerId = customer.BraintreeCustomerId
                                         });
                    if (clientToken != null)
                    {
                        LogMessage(userid + "API clientToken - Client token generated. Token=" + clientToken.ToString());
                        return Json(new { Success = true, CustomerSuccess = true, ClientToken = clientToken }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        LogMessage("API clientToken: Error generating client token", LogLevel.ERROR);
                        return Json(new { Success = false, CustomerSuccess = true, ClientToken = "Error occurred while generating client token" }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage("API clientToken: Error generating client token" + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                return Json(new { Success = false, ClientToken = "Error occurred while generating client token" }, JsonRequestBehavior.AllowGet);
            }
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        [HttpPost]
        public JsonResult paymentMethod(string paymentMethodNonce, Guid? basketId)
        {
            string userid = "";
            User user = null;
            StatusModel statusModel = new StatusModel();
            try
            {
                LogMessage(userid + "API paymentMethod request");

                if (basketId == null)
                {
                    LogMessage(userid + "API paymentMethod - basketId was not passed.", LogLevel.ERROR);
                    statusModel.Message = "Request failed: basketId was not passed.";
                    return Json(statusModel, JsonRequestBehavior.AllowGet);
                }
                TicketBasket ticketBasket = _supperClubRepository.GetBasket((Guid)basketId);
                if (ticketBasket == null)
                {
                    LogMessage(userid + "API paymentMethod - Invalid Basket Id", LogLevel.ERROR);
                    statusModel.Message = "Request failed: Invalid Basket Id";
                    return Json(statusModel, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    Guid userId = ticketBasket.UserId;
                    user = _supperClubRepository.GetUser(userId);
                    userid = (user.Id == null ? "" : "UserId=" + user.Id.ToString() + ". ");
                    if (ticketBasket.Status != TicketBasketStatus.InProgress.ToString())
                    {
                        LogMessage(userid + "API paymentMethod - This is not an active Basket" + basketId.ToString(), LogLevel.ERROR);
                        statusModel.Message = "Request failed: This is not an active Basket. BasketId:" + basketId.ToString();
                        return Json(statusModel, JsonRequestBehavior.AllowGet);
                    }
                    int TicketBasketSessionTimeOut = int.Parse(WebConfigurationManager.AppSettings["TicketBasketSessionTimeOut"]);
                    DateTime expired = DateTime.Now.AddMinutes(-TicketBasketSessionTimeOut);
                    string inProgressStatus = TicketBasketStatus.InProgress.ToString();
                    if (ticketBasket.LastUpdated < expired && ticketBasket.Status == inProgressStatus)
                    {
                        Tuple<int, int> tplResult = _supperClubRepository.CleanUpAbandonedBaskets(TicketBasketSessionTimeOut);
                        LogMessage(string.Format(userid + "API paymentMethod: Flushed {0} Expired Baskets ({1} tickets)", tplResult.Item1, tplResult.Item2), LogLevel.DEBUG);
                        LogMessage(userid + "API paymentMethod - Basket Expired.", LogLevel.ERROR);
                        statusModel.Message = "Request failed: Ticket basket has expired. BasketId:" + basketId.ToString();
                        return Json(statusModel, JsonRequestBehavior.AllowGet);
                    }
                }
                if (braintreeEnvironmentSetting.ToLower() == "live")
                    braintreeEnvironment = Braintree.Environment.PRODUCTION;

                var gateway = new BraintreeGateway
                {
                    Environment = braintreeEnvironment,
                    MerchantId = braintreeMerchantId,
                    PublicKey = braintreePublicKey,
                    PrivateKey = braintreePrivateKey
                };
                BraintreeCustomer bc = _supperClubRepository.GetBraintreeCustomer(ticketBasket.UserId);

                var request = bc != null ? (new TransactionRequest
                {
                    Amount = ticketBasket.TotalPrice - ticketBasket.TotalDiscount,
                    PaymentMethodNonce = paymentMethodNonce,
                    CustomerId = bc.BraintreeCustomerId,
                    Options = new TransactionOptionsRequest
                    {
                        SubmitForSettlement = true
                    }
                }) : (new TransactionRequest
                {
                    Amount = ticketBasket.TotalPrice - ticketBasket.TotalDiscount,
                    PaymentMethodNonce = paymentMethodNonce,
                    Options = new TransactionOptionsRequest
                    {
                        SubmitForSettlement = true
                    }
                });

                Result<Transaction> result = gateway.Transaction.Sale(request);

                if (result.Target != null && (result.Target.Status == TransactionStatus.AUTHORIZED || result.Target.Status == TransactionStatus.SUBMITTED_FOR_SETTLEMENT) && result.IsSuccess())
                {
                    LogMessage(userid + "API paymentMethod transaction authorized.", LogLevel.INFO);
                    
                    //TODO Add transaction logging
                    BraintreeTransaction bt = new BraintreeTransaction
                    {
                        TransactionStatus = result.Message == null ? (result.Target != null ? result.Target.Status.ToString() : "") : result.Message,
                        TransactionSuccess = result.IsSuccess(),
                        TransactionValidVenmoSDK = result.Target.CreditCard.IsVenmoSdk.Value,
                        Amount = result.Target.Amount,
                        AvsErrorResponseCode = result.Target.AvsErrorResponseCode,
                        AvsStreetAddressResponseCode = result.Target.AvsStreetAddressResponseCode,
                        AvsPostalCodeResponseCode = result.Target.AvsPostalCodeResponseCode,
                        Channel = result.Target.Channel,
                        TransactionCreationDate = result.Target.CreatedAt,
                        TransactionUpdateDate = result.Target.UpdatedAt,
                        CvvResponseCode = result.Target.CvvResponseCode,
                        TransactionId = result.Target.Id,
                        MerchantAccountId = result.Target.MerchantAccountId,
                        OrderId = result.Target.OrderId,
                        PlanId = result.Target.PlanId,
                        ProcessorAuthorizationCode = result.Target.ProcessorAuthorizationCode,
                        ProcessorResponseCode = result.Target.ProcessorResponseCode,
                        ProcessorResponseText = result.Target.ProcessorResponseText,
                        PurchaseOrderNumber = result.Target.PurchaseOrderNumber,
                        ServiceFeeAmount = result.Target.ServiceFeeAmount,
                        SettlementBatchId = result.Target.SettlementBatchId,
                        Status = result.Target.Status.ToString(),
                        TaxAmount = result.Target.TaxAmount,
                        TaxExempt = result.Target.TaxExempt,
                        TransactionType = result.Target.Type.ToString()
                    };

                    // Add Transaction
                    if (_supperClubRepository.CreateTransction(bt) != null)
                        LogMessage(userid + "API useCard transaction added to database successfully.", LogLevel.INFO);
                    else
                        LogMessage(userid + "API useCard transaction could not be added to database.", LogLevel.ERROR);


                   TicketingService _ticketingService = new TicketingService(_supperClubRepository);
                   _ticketingService.UpdateTicketBasket(ticketBasket, TicketBasketStatus.Complete);

                   List<APIBookingModel> abm = (ticketBasket.Tickets.GroupBy(t => new { t.BasketId, t.EventId, t.SeatingId, t.MenuOptionId, t.BasePrice }).Select(t =>
                                                    new APIBookingModel { UserId = (Guid)user.Id, BasketId = t.Key.BasketId,  EventId = t.Key.EventId, SeatingId = t.Key.SeatingId, MenuOptionId = t.Key.MenuOptionId, BasePrice = t.Key.BasePrice, NumberOfTickets = t.Count() }).Distinct().ToList<APIBookingModel>());

                   if (abm.Count > 0)
                   {
                       if (_ticketingService.AddUserToEvent(abm))
                       {
                           LogMessage(userid + "API paymentMethod user added to event successfully.", LogLevel.INFO);
                           if (ticketBasket.TotalDiscount > 0)
                               _ticketingService.UpdateVoucherUsage(ticketBasket.Tickets[0].VoucherId, ticketBasket.TotalDiscount);
                       }
                       else
                           LogMessage(userid + "API paymentMethod user could not be added to event.", LogLevel.ERROR);
                   }

                   List<BookingMenuModel> lbmm = new List<BookingMenuModel>();
                   foreach (APIBookingModel a in abm)
                   {
                       if (a.MenuOptionId > 0)
                       {
                           BookingMenuModel bmm = new BookingMenuModel();
                           bmm.menuOptionId = a.MenuOptionId;
                           bmm.menuTitle = a.MenuTitle;
                           bmm.baseTicketCost = a.BasePrice;
                           bmm.numberOfTickets = a.NumberOfTickets;
                           lbmm.Add(bmm);
                       }
                   }
                   bool hostSuccess = false;
                   EmailService.EmailService es = new EmailService.EmailService(_supperClubRepository);
                   bool success = es.SendGuestBookedEmails(
                       user.FirstName,
                       user.Email,
                       ticketBasket.TotalTickets,
                       ticketBasket.TotalPrice - ticketBasket.TotalDiscount,
                       ticketBasket.BookingReference,
                       ticketBasket.BookingRequirements,
                       ticketBasket.Tickets[0].EventId,
                       ticketBasket.Tickets[0].SeatingId,
                       lbmm,
                       ticketBasket.Tickets[0].VoucherId,
                       ticketBasket.CCLastDigits,
                       ticketBasket.Tickets[0].CommissionMultiplier,
                       ref hostSuccess);

                   statusModel.GuestEmailSentStatus = success;

                   if (success)
                       LogMessage("API paymentMethod Guest Booking Confirmation Email: Sent successfully", LogLevel.INFO);
                   else
                   {
                       LogMessage("API paymentMethod Guest Booking Confirmation Email: Failure sending e-mail", LogLevel.ERROR);
                       statusModel.Message = statusModel.Message + "    Booking transaction successful but error while sending Guest Booking Confirmation Email";
                   }

                   if (hostSuccess)
                       LogMessage("API paymentMethod Host Booking Information Email: Sent successfully", SupperClub.Logger.LogLevel.INFO);
                   else
                   {
                       LogMessage("API paymentMethod Host Booking Information Email: Failure sending e-mail", LogLevel.ERROR);
                       statusModel.Message = statusModel.Message + "    Booking transaction successful but error while sending Host Booking Confirmation Email";
                   }

                   //Push notification to FB friends 
                   if (user.FacebookId != null)
                   {
                       // Push Notification to Friends
                       Event _event = _supperClubRepository.GetEvent(ticketBasket.Tickets[0].EventId);
                       PushNotificationService.MessageService ms = new PushNotificationService.MessageService(_supperClubRepository);
                       ms.FacebookFriendBoughtTicketNotification(user.FirstName + " " + user.LastName, _event.Name, _event.Id, user.Id, user.FacebookId);
                   }
                   statusModel.Success = result.IsSuccess();
                   //statusModel.ValidVenmoSDK = result.Target != null ? result.Target.CreditCard.IsVenmoSdk.Value : false;
                   statusModel.Message = result.Message == null ? (result.Target != null ? result.Target.Status.ToString() : "") : result.Message;                
                   statusModel.BookingReferenceNumber = ticketBasket.BookingReference;
               }
               else
               {
                   LogMessage("API paymentMethod: Transaction not authorized. result.IsSuccess() value=" + result.IsSuccess().ToString(), LogLevel.ERROR);
                   statusModel.Success = false; // result.IsSuccess();
                   statusModel.ValidVenmoSDK = result.Target != null ? result.Target.CreditCard.IsVenmoSdk.Value : false;
                    statusModel.Message = "Transaction not authorized." + (result.Message == null ? (result.Target != null ? result.Target.Status.ToString() : "") : result.Message);
               }

               return Json(new { Success = statusModel.Success, Message = statusModel.Message, BookingReferenceNumber = statusModel.BookingReferenceNumber }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                LogMessage("API paymentMethod: Error processing transaction" + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                return Json(new { Success = false, Message = "Error occurred while processing the transaction", BookingReferenceNumber=0 }, JsonRequestBehavior.AllowGet);
            }
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        [HttpPost]
        public JsonResult saveCard(string encryptedCardNumber, string encryptedExpirationMonth, string encryptedExpirationYear, string venmoSdkSession, Guid? basketId)
        {
            string userid = "";
            User user = null;
            StatusModel statusModel = new StatusModel();
            statusModel.Success = false;
            statusModel.ValidVenmoSDK = false;
            statusModel.GuestEmailSentStatus = false;
            try
            {
                LogMessage(userid + "API saveCard request");

                if (basketId == null)
                {
                    LogMessage(userid + "API saveCard - basketId was not passed.", LogLevel.ERROR);
                    statusModel.Message = "Request failed: basketId was not passed.";
                    return Json(statusModel, JsonRequestBehavior.AllowGet);
                }
                TicketBasket ticketBasket = _supperClubRepository.GetBasket((Guid)basketId);
                if (ticketBasket == null)
                {
                    LogMessage(userid + "API saveCard - Invalid Basket Id", LogLevel.ERROR);
                    statusModel.Message = "Request failed: Invalid Basket Id";
                    return Json(statusModel, JsonRequestBehavior.AllowGet);
                }
                else 
                {
                    Guid userId = ticketBasket.UserId;
                    user = _supperClubRepository.GetUser(userId);
                    userid = (user.Id == null ? "" : "UserId=" + user.Id.ToString() + ". "); 
                    if (ticketBasket.Status != TicketBasketStatus.InProgress.ToString())
                    {
                        LogMessage(userid + "API saveCard - This is not an active Basket" + basketId.ToString(), LogLevel.ERROR);
                        statusModel.Message = "Request failed: This is not an active Basket. BasketId:" + basketId.ToString();
                        return Json(statusModel, JsonRequestBehavior.AllowGet);
                    }
                    int TicketBasketSessionTimeOut = int.Parse(WebConfigurationManager.AppSettings["TicketBasketSessionTimeOut"]);
                    DateTime expired = DateTime.Now.AddMinutes(-TicketBasketSessionTimeOut);
                    string inProgressStatus = TicketBasketStatus.InProgress.ToString();
                    if (ticketBasket.LastUpdated < expired && ticketBasket.Status == inProgressStatus)
                    {
                        Tuple<int, int> tplResult = _supperClubRepository.CleanUpAbandonedBaskets(TicketBasketSessionTimeOut);
                        LogMessage(string.Format(userid + "API saveCard: Flushed {0} Expired Baskets ({1} tickets)", tplResult.Item1, tplResult.Item2), LogLevel.DEBUG);
                        LogMessage(userid + "API saveCard - Basket Expired.", LogLevel.ERROR);
                        statusModel.Message = "Request failed: Ticket basket has expired. BasketId:"  + basketId.ToString();
                        return Json(statusModel, JsonRequestBehavior.AllowGet);
                    }
                }
                if (string.IsNullOrEmpty(encryptedCardNumber))
                {
                    LogMessage(userid + "API saveCard - encrypted Card Number was not passed.", LogLevel.ERROR);
                    statusModel.Message = "Request failed: encryptedCardNumber was not passed.";
                    return Json(statusModel, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(encryptedExpirationMonth))
                {
                    LogMessage(userid + "API saveCard - encrypted Expiration Month was not passed.", LogLevel.ERROR);
                    statusModel.Message = "Request failed: encryptedExpirationMonth was not passed.";
                    return Json(statusModel, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(encryptedExpirationYear))
                {
                    LogMessage(userid + "API saveCard - encrypted Expiration Year was not passed.", LogLevel.ERROR);
                    statusModel.Message = "Request failed: encryptedExpirationYear was not passed.";
                    return Json(statusModel, JsonRequestBehavior.AllowGet);
                }
                if (string.IsNullOrEmpty(venmoSdkSession))
                {
                    LogMessage(userid + "API saveCard - Venmo Sdk Session was not passed.", LogLevel.ERROR);
                    statusModel.Message = "Request failed: venmoSdkSession was not passed.";
                    return Json(statusModel, JsonRequestBehavior.AllowGet);
                }

                if (braintreeEnvironmentSetting.ToLower() == "live")
                    braintreeEnvironment = Braintree.Environment.PRODUCTION;

                var gateway = new BraintreeGateway
                {
                    Environment = braintreeEnvironment,
                    MerchantId = braintreeMerchantId,
                    PublicKey = braintreePublicKey,
                    PrivateKey = braintreePrivateKey
                };
                LogMessage(userid + "API saveCard. Input parameters - CardNumber:" + encryptedCardNumber + ",ExpirationMonth:" + encryptedExpirationMonth + ",ExpirationYear:" + encryptedExpirationYear + ",venmoSdkSession:" + venmoSdkSession);
                var request = new TransactionRequest()
                {
                    CreditCard = new TransactionCreditCardRequest()
                    {
                        Number = encryptedCardNumber,
                        ExpirationMonth = encryptedExpirationMonth,
                        ExpirationYear = encryptedExpirationYear
                    },
                    Options = new TransactionOptionsRequest
                    {
                        VenmoSdkSession = venmoSdkSession //SandboxValues.VenmoSdk.SESSION                    
                    },
                    Amount = ticketBasket.TotalPrice - ticketBasket.TotalDiscount
                };

               Result<Transaction> result = gateway.Transaction.Sale(request);

               if (result.Target != null && result.Target.Status == TransactionStatus.AUTHORIZED && result.IsSuccess())
                {
                    LogMessage(userid + "API saveCard transaction authorized.", LogLevel.INFO);
                    
                    //TODO Add transaction logging
                    BraintreeTransaction bt = new BraintreeTransaction
                    {
                        TransactionStatus = result.Message == null ? (result.Target != null ? result.Target.Status.ToString() : "") : result.Message,
                        TransactionSuccess = result.IsSuccess(),
                        TransactionValidVenmoSDK = result.Target.CreditCard.IsVenmoSdk.Value,
                        Amount = result.Target.Amount,
                        AvsErrorResponseCode = result.Target.AvsErrorResponseCode,
                        AvsStreetAddressResponseCode = result.Target.AvsStreetAddressResponseCode,
                        AvsPostalCodeResponseCode = result.Target.AvsPostalCodeResponseCode,
                        Channel = result.Target.Channel,
                        TransactionCreationDate = result.Target.CreatedAt,
                        TransactionUpdateDate = result.Target.UpdatedAt,
                        CvvResponseCode = result.Target.CvvResponseCode,
                        TransactionId = result.Target.Id,
                        MerchantAccountId = result.Target.MerchantAccountId,
                        OrderId = result.Target.OrderId,
                        PlanId = result.Target.PlanId,
                        ProcessorAuthorizationCode = result.Target.ProcessorAuthorizationCode,
                        ProcessorResponseCode = result.Target.ProcessorResponseCode,
                        ProcessorResponseText = result.Target.ProcessorResponseText,
                        PurchaseOrderNumber = result.Target.PurchaseOrderNumber,
                        ServiceFeeAmount = result.Target.ServiceFeeAmount,
                        SettlementBatchId = result.Target.SettlementBatchId,
                        Status = result.Target.Status.ToString(),
                        TaxAmount = result.Target.TaxAmount,
                        TaxExempt = result.Target.TaxExempt,
                        TransactionType = result.Target.Type.ToString()
                    };

                    // Add Transaction
                    if (_supperClubRepository.CreateTransction(bt) != null)
                        LogMessage(userid + "API useCard transaction added to database successfully.", LogLevel.INFO);
                    else
                        LogMessage(userid + "API useCard transaction could not be added to database.", LogLevel.ERROR);


                   TicketingService _ticketingService = new TicketingService(_supperClubRepository);
                   _ticketingService.UpdateTicketBasket(ticketBasket, TicketBasketStatus.Complete);

                   List<APIBookingModel> abm = (ticketBasket.Tickets.GroupBy(t => new { t.EventId, t.CommissionMultiplier, t.SeatingId, t.MenuOptionId, t.BasePrice }).Select(t =>
                                                    new APIBookingModel { UserId = (Guid)user.Id, EventId = t.Key.EventId, commission=t.Key.CommissionMultiplier , SeatingId = t.Key.SeatingId, MenuOptionId = t.Key.MenuOptionId, BasePrice = t.Key.BasePrice, NumberOfTickets = t.Count() }).Distinct().ToList<APIBookingModel>());

                   if (abm.Count > 0)
                   {
                       if (_ticketingService.AddUserToEvent(abm))
                       {
                           LogMessage(userid + "API saveCard user added to event successfully.", LogLevel.INFO);
                           if (ticketBasket.TotalDiscount > 0)
                               _ticketingService.UpdateVoucherUsage(ticketBasket.Tickets[0].VoucherId, ticketBasket.TotalDiscount);
                       }
                       else
                           LogMessage(userid + "API saveCard user could not be added to event.", LogLevel.ERROR);
                   }

                   List<BookingMenuModel> lbmm = new List<BookingMenuModel>();
                   foreach (APIBookingModel a in abm)
                   {
                       if (a.MenuOptionId > 0)
                       {
                           BookingMenuModel bmm = new BookingMenuModel();
                           bmm.menuOptionId = a.MenuOptionId;
                           bmm.menuTitle = a.MenuTitle;
                           bmm.baseTicketCost = a.BasePrice;
                           bmm.numberOfTickets = a.NumberOfTickets;
                           lbmm.Add(bmm);
                       }
                   }
                   bool hostSuccess = false;
                   EmailService.EmailService es = new EmailService.EmailService(_supperClubRepository);
                   bool success = es.SendGuestBookedEmails(
                       user.FirstName,
                       user.Email,
                       ticketBasket.TotalTickets,
                       ticketBasket.TotalPrice - ticketBasket.TotalDiscount,
                       ticketBasket.BookingReference,
                       ticketBasket.BookingRequirements,
                       ticketBasket.Tickets[0].EventId,
                       ticketBasket.Tickets[0].SeatingId,
                       lbmm,
                       ticketBasket.Tickets[0].VoucherId,
                       ticketBasket.CCLastDigits,
                       ticketBasket.Tickets[0].CommissionMultiplier,
                       ref hostSuccess);

                   statusModel.GuestEmailSentStatus = success;

                   if (success)
                       LogMessage("API saveCard Guest Booking Confirmation Email: Sent successfully", LogLevel.INFO);
                   else
                   {
                       LogMessage("API saveCard Guest Booking Confirmation Email: Failure sending e-mail", LogLevel.ERROR);
                       statusModel.Message = statusModel.Message + "    Booking transaction successful but error while sending Guest Booking Confirmation Email";
                   }

                   if (hostSuccess)
                       LogMessage("API saveCard Host Booking Information Email: Sent successfully", SupperClub.Logger.LogLevel.INFO);
                   else
                   {
                       LogMessage("API saveCard Host Booking Information Email: Failure sending e-mail", LogLevel.ERROR);
                       statusModel.Message = statusModel.Message + "    Booking transaction successful but error while sending Host Booking Confirmation Email";
                   }

                   //Push notification to FB friends 
                   if (user.FacebookId != null)
                   {
                       // Push Notification to Friends
                       Event _event = _supperClubRepository.GetEvent(ticketBasket.Tickets[0].EventId);
                       PushNotificationService.MessageService ms = new PushNotificationService.MessageService(_supperClubRepository);
                       ms.FacebookFriendBoughtTicketNotification(user.FirstName + " " + user.LastName, _event.Name, _event.Id, user.Id, user.FacebookId);
                   }
                   statusModel.Success = result.IsSuccess();
                   statusModel.ValidVenmoSDK = result.Target != null ? result.Target.CreditCard.IsVenmoSdk.Value : false;
                   statusModel.Message = result.Message == null ? (result.Target != null ? result.Target.Status.ToString() : "") : result.Message;                
                   statusModel.BookingReferenceNumber = ticketBasket.BookingReference;
               }
               else
               {
                   LogMessage("API saveCard: Transaction not authorized.", LogLevel.ERROR);
                   statusModel.Success = result.IsSuccess();
                   statusModel.ValidVenmoSDK = result.Target != null ? result.Target.CreditCard.IsVenmoSdk.Value : false;
                   statusModel.Message = result.Message == null ? (result.Target != null ? result.Target.Status.ToString() : "") : result.Message;
               }
            }
            catch (Exception ex)
            {
                LogMessage("API saveCard: Error while performing transaction." + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                statusModel.Success = false;
                statusModel.Message = statusModel.Message + "Error occurred during transaction processing.";
            }
            return Json(statusModel, JsonRequestBehavior.AllowGet);
        }             

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        [HttpPost]
        public JsonResult useCard(string venmoSdkPaymentMethodCode, Guid? basketId)
        {
            string userid = "";
            User user = null;
            StatusModel statusModel = new StatusModel();
            statusModel.Success = false;
            statusModel.ValidVenmoSDK = false;
            statusModel.GuestEmailSentStatus = false;
            try
            {
                LogMessage(userid + "API useCard request");

                if (basketId == null)
                {
                    LogMessage(userid + "API useCard - basketId was not passed.", LogLevel.ERROR);
                    statusModel.Message = "Request failed: basketId was not passed.";
                    return Json(statusModel, JsonRequestBehavior.AllowGet);
                }
                TicketBasket ticketBasket = _supperClubRepository.GetBasket((Guid)basketId);
                if (ticketBasket == null)
                {
                    LogMessage(userid + "API useCard - Invalid Basket Id", LogLevel.ERROR);
                    statusModel.Message = "Request failed: Invalid Basket Id";
                    return Json(statusModel, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    Guid userId = ticketBasket.UserId;
                    user = _supperClubRepository.GetUser(userId);
                    userid = (user.Id == null ? "" : "UserId=" + user.Id.ToString() + ". ");
                    if (ticketBasket.Status != TicketBasketStatus.InProgress.ToString())
                    {
                        LogMessage(userid + "API useCard - This is not an active Basket" + basketId.ToString(), LogLevel.ERROR);
                        statusModel.Message = "Request failed: This is not an active Basket. BasketId:" + basketId.ToString();
                        return Json(statusModel, JsonRequestBehavior.AllowGet);
                    }
                    int TicketBasketSessionTimeOut = int.Parse(WebConfigurationManager.AppSettings["TicketBasketSessionTimeOut"]);
                    DateTime expired = DateTime.Now.AddMinutes(-TicketBasketSessionTimeOut);
                    string inProgressStatus = TicketBasketStatus.InProgress.ToString();
                    if (ticketBasket.LastUpdated < expired && ticketBasket.Status == inProgressStatus)
                    {
                        Tuple<int, int> tplResult = _supperClubRepository.CleanUpAbandonedBaskets(TicketBasketSessionTimeOut);
                        LogMessage(string.Format(userid + "API useCard: Flushed {0} Expired Baskets ({1} tickets)", tplResult.Item1, tplResult.Item2), LogLevel.DEBUG);
                        LogMessage(userid + "API useCard - Basket Expired.", LogLevel.ERROR);
                        statusModel.Message = "Request failed: Ticket basket has expired. BasketId:" + basketId.ToString();
                        return Json(statusModel, JsonRequestBehavior.AllowGet);
                    }
                }

                if (string.IsNullOrEmpty(venmoSdkPaymentMethodCode))
                {
                    LogMessage(userid + "API useCard - Venmo Sdk Payment Method Code was not passed.", LogLevel.ERROR);
                    statusModel.Message = "Request failed: venmoSdkPaymentMethodCode was not passed.";
                    return Json(statusModel, JsonRequestBehavior.AllowGet);
                }

                if (braintreeEnvironmentSetting.ToLower() == "live")
                    braintreeEnvironment = Braintree.Environment.PRODUCTION;

                var gateway = new BraintreeGateway
                {
                    Environment = braintreeEnvironment,
                    MerchantId = braintreeMerchantId,
                    PublicKey = braintreePublicKey,
                    PrivateKey = braintreePrivateKey
                };
                var request = new TransactionRequest
                {
                    Amount = ticketBasket.TotalPrice - ticketBasket.TotalDiscount,
                    VenmoSdkPaymentMethodCode = venmoSdkPaymentMethodCode
                };
                
                Result<Transaction> result = gateway.Transaction.Sale(request);

                if (result.Target != null && result.Target.Status == TransactionStatus.AUTHORIZED && result.IsSuccess())
                {
                    LogMessage(userid + "API useCard transaction authorized.", LogLevel.INFO);
                    
                    //TODO Add transaction logging
                    BraintreeTransaction bt = new BraintreeTransaction
                    {
                        TransactionStatus = result.Message == null ? (result.Target != null ? result.Target.Status.ToString() : "") : result.Message,
                        TransactionSuccess = result.IsSuccess(),
                        TransactionValidVenmoSDK = result.Target.CreditCard.IsVenmoSdk.Value,
                        Amount = result.Target.Amount,
                        AvsErrorResponseCode = result.Target.AvsErrorResponseCode,
                        AvsStreetAddressResponseCode = result.Target.AvsStreetAddressResponseCode,
                        AvsPostalCodeResponseCode = result.Target.AvsPostalCodeResponseCode,
                        Channel = result.Target.Channel,
                        TransactionCreationDate = result.Target.CreatedAt,
                        CvvResponseCode = result.Target.CvvResponseCode,
                        TransactionId = result.Target.Id,
                        MerchantAccountId = result.Target.MerchantAccountId,
                        OrderId = result.Target.OrderId,
                        PlanId = result.Target.PlanId,
                        ProcessorAuthorizationCode = result.Target.ProcessorAuthorizationCode,
                        ProcessorResponseCode = result.Target.ProcessorResponseCode,
                        ProcessorResponseText = result.Target.ProcessorResponseText,
                        PurchaseOrderNumber = result.Target.PurchaseOrderNumber,
                        ServiceFeeAmount = result.Target.ServiceFeeAmount,
                        SettlementBatchId = result.Target.SettlementBatchId,
                        Status = result.Target.Status.ToString(),
                        TaxAmount = result.Target.TaxAmount,
                        TaxExempt = result.Target.TaxExempt,
                        TransactionType = result.Target.Type.ToString()
                    };                        

                    // Add Transaction
                    if(_supperClubRepository.CreateTransction(bt) != null)
                        LogMessage(userid + "API useCard transaction added to database successfully.", LogLevel.INFO);
                    else
                        LogMessage(userid + "API useCard transaction could not be added to database.", LogLevel.ERROR);

                    TicketingService _ticketingService = new TicketingService(_supperClubRepository);
                    _ticketingService.UpdateTicketBasket(ticketBasket, TicketBasketStatus.Complete);

                    List<APIBookingModel> abm = (ticketBasket.Tickets.GroupBy(t => new { t.EventId, t.SeatingId, t.MenuOptionId, t.VoucherId, t.BasePrice, t.DiscountAmount }).Select(t =>
                                                     new APIBookingModel { UserId = (Guid)user.Id, EventId = t.Key.EventId, SeatingId = t.Key.SeatingId, MenuOptionId = t.Key.MenuOptionId, VoucherId = t.Key.VoucherId, BasePrice = t.Key.BasePrice, DiscountAmount = t.Key.DiscountAmount, NumberOfTickets = t.Count(), BasketId =(Guid)basketId}).Distinct().ToList<APIBookingModel>());

                    if (abm.Count > 0)
                    {
                        if (_ticketingService.AddUserToEvent(abm))
                        {
                            LogMessage(userid + "API useCard user added to event successfully.", LogLevel.INFO);
                            if (ticketBasket.TotalDiscount > 0)
                                _ticketingService.UpdateVoucherUsage(ticketBasket.Tickets[0].VoucherId, ticketBasket.TotalDiscount);
                        }
                        else
                            LogMessage(userid + "API useCard user could not be added to event.", LogLevel.ERROR);
                    }

                    statusModel.Success = result.IsSuccess();
                    statusModel.ValidVenmoSDK = result.Target != null ? result.Target.CreditCard.IsVenmoSdk.Value : false;
                    statusModel.Message = result.Message == null ? (result.Target != null ? result.Target.Status.ToString() : "") : result.Message;
                    statusModel.BookingReferenceNumber = ticketBasket.BookingReference;

                    List<BookingMenuModel> lbmm = new List<BookingMenuModel>();
                    
                    foreach (APIBookingModel a in abm)
                    {
                        if (a.MenuOptionId > 0)
                        {
                            BookingMenuModel bmm = new BookingMenuModel();
                            EventMenuOption emo = _supperClubRepository.GetEventMenuOption(a.MenuOptionId);
                            bmm.menuOptionId = a.MenuOptionId;
                            bmm.menuTitle = emo.Title;
                            bmm.baseTicketCost = a.BasePrice;
                            bmm.numberOfTickets = a.NumberOfTickets;
                            lbmm.Add(bmm);
                        }
                    }

                    bool hostSuccess = false;
                    EmailService.EmailService es = new EmailService.EmailService(_supperClubRepository);
                    bool success = es.SendGuestBookedEmails(
                        user.FirstName,
                        user.Email,
                        ticketBasket.TotalTickets,
                        ticketBasket.TotalPrice - ticketBasket.TotalDiscount,
                        ticketBasket.BookingReference,
                        ticketBasket.BookingRequirements,
                        ticketBasket.Tickets[0].EventId,
                        ticketBasket.Tickets[0].SeatingId,
                        lbmm,
                        ticketBasket.Tickets[0].VoucherId,
                        ticketBasket.CCLastDigits,
                        ticketBasket.Tickets[0].CommissionMultiplier,
                        ref hostSuccess);
                    
                    statusModel.GuestEmailSentStatus = success;
                    
                    if (success)
                        LogMessage("API useCard Guest Booking Confirmation Email: Sent successfully", LogLevel.INFO);
                    else
                    {
                        LogMessage("API useCard Guest Booking Confirmation Email: Failure sending e-mail", LogLevel.ERROR);
                        statusModel.Message = statusModel.Message + "    Booking transaction successful but error while sending Guest Booking Confirmation Email";
                    }

                    if (hostSuccess)
                        LogMessage("API useCard Host Booking Information Email: Sent successfully", SupperClub.Logger.LogLevel.INFO);
                    else
                    {
                        LogMessage("API useCard Host Booking Information Email: Failure sending e-mail", LogLevel.ERROR);
                        statusModel.Message = statusModel.Message + "    Booking transaction successful but error while sending Booking Confirmation Email to host";
                    }

                    //Push notification to FB friends 
                    if (user.FacebookId != null)
                    {
                        // Push Notification to Friends
                        Event _event = _supperClubRepository.GetEvent(ticketBasket.Tickets[0].EventId);
                        PushNotificationService.MessageService ms = new PushNotificationService.MessageService(_supperClubRepository);
                        ms.FacebookFriendBoughtTicketNotification(user.FirstName + " " + user.LastName, _event.Name, _event.Id, user.Id, user.FacebookId);
                    }
                }
                else
                {
                    LogMessage("API useCard: Transaction not authorized.", LogLevel.ERROR);
                    statusModel.Success = result.IsSuccess();
                    statusModel.ValidVenmoSDK = result.Target != null ? result.Target.CreditCard.IsVenmoSdk.Value : false;
                    statusModel.Message = result.Message == null ? (result.Target != null ? result.Target.Status.ToString() : "") : result.Message;
                }
            }
            catch(Exception ex)
            {
                LogMessage("API useCard: Error while performing transaction." + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                statusModel.Success = false;
                statusModel.Message = statusModel.Message + "Error occurred during transaction processing.";
            }
            return Json(statusModel, JsonRequestBehavior.AllowGet);
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        [HttpPost]
        public JsonResult processFreeBooking(Guid? basketId, int voucherId = 0)
        {
            string userid = "";
            User user = null;
            StatusModel statusModel = new StatusModel();
            statusModel.Success = false;
            statusModel.ValidVenmoSDK = false;
            statusModel.GuestEmailSentStatus = false;
            try
            {
                LogMessage(userid + "API processFreeBooking request");

                if (basketId == null)
                {
                    LogMessage(userid + "API processFreeBooking - basketId was not passed.", LogLevel.ERROR);
                    statusModel.Message = "Request failed: basketId was not passed.";
                    return Json(statusModel, JsonRequestBehavior.AllowGet);
                }
                TicketBasket ticketBasket = _supperClubRepository.GetBasket((Guid)basketId);
                if (ticketBasket == null)
                {
                    LogMessage(userid + "API processFreeBooking - Invalid Basket Id", LogLevel.ERROR);
                    statusModel.Message = "Request failed: Invalid Basket Id";
                    return Json(statusModel, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    Guid userId = ticketBasket.UserId;
                    user = _supperClubRepository.GetUser(userId);
                    userid = (user.Id == null ? "" : "UserId=" + user.Id.ToString() + ". ");
                    if (ticketBasket.Status != TicketBasketStatus.InProgress.ToString())
                    {
                        LogMessage(userid + "API processFreeBooking - This is not an active Basket" + basketId.ToString(), LogLevel.ERROR);
                        statusModel.Message = "Request failed: This is not an active Basket. BasketId:" + basketId.ToString();
                        return Json(statusModel, JsonRequestBehavior.AllowGet);
                    }
                    int TicketBasketSessionTimeOut = int.Parse(WebConfigurationManager.AppSettings["TicketBasketSessionTimeOut"]);
                    DateTime expired = DateTime.Now.AddMinutes(-TicketBasketSessionTimeOut);
                    string inProgressStatus = TicketBasketStatus.InProgress.ToString();
                    if (ticketBasket.LastUpdated < expired && ticketBasket.Status == inProgressStatus)
                    {
                        Tuple<int, int> tplResult = _supperClubRepository.CleanUpAbandonedBaskets(TicketBasketSessionTimeOut);
                        LogMessage(string.Format(userid + "API processFreeBooking: Flushed {0} Expired Baskets ({1} tickets)", tplResult.Item1, tplResult.Item2), LogLevel.DEBUG);
                        LogMessage(userid + "API processFreeBooking - Basket Expired.", LogLevel.ERROR);
                        statusModel.Message = "Request failed: Ticket basket has expired. BasketId:" + basketId.ToString();
                        return Json(statusModel, JsonRequestBehavior.AllowGet);
                    }
                }
                
                TicketingService _ticketingService = new TicketingService(_supperClubRepository);
                //if (voucherId > 0)
                //{
                //    Voucher voucher = _supperClubRepository.GetVoucher(voucherId);
                //    decimal discount = 0;
                //    decimal discountPerTicket = 0;
                //    if(voucher.TypeId == (int)VoucherType.PercentageOff)
                //    {
                //        discount = ticketBasket.TotalPrice * (voucher.OffValue / 100);
                //        discountPerTicket = discount/ticketBasket.TotalTickets;
                //    }
                //    else if (voucher.TypeId == (int)VoucherType.ValueOff)
                //    {
                //        discount = voucher.OffValue / 100;
                //        discountPerTicket = discount / ticketBasket.TotalTickets;
                //    }
                //    List<Ticket> tickets = new List<Ticket>();                   
                   
                //    for (int i = 0; i < ticketBasket.TotalTickets; i++)
                //    {
                //        Ticket ticket = new Ticket(ticketBasket.Tickets[i].EventId, ticketBasket.Tickets[i].BasePrice, ticketBasket.Tickets[i].BasketId, ticketBasket.Tickets[i].UserId, ticketBasket.Tickets[i].SeatingId, ticketBasket.Tickets[i].MenuOptionId, voucherId, discountPerTicket);
                //        ticket.Description = string.Format("GrubClub ticket (Event Id: {0}, Seating Id: {1}, Menu Option Id: {2})", ticketBasket.Tickets[i].EventId, ticketBasket.Tickets[i].SeatingId, ticketBasket.Tickets[i].MenuOptionId);
                //        tickets.Add(ticket);
                //    }
                //    bool successRemoveBasket = _ticketingService.RemoveTicketsFromBasket(ticketBasket.Tickets);

                //    ticketBasket = _supperClubRepository.AddToBasket(tickets);
                //}

                _ticketingService.UpdateTicketBasket(ticketBasket, TicketBasketStatus.Complete);

                List<APIBookingModel> abm = (ticketBasket.Tickets.GroupBy(t => new { t.EventId, t.SeatingId, t.MenuOptionId, t.BasePrice }).Select(t =>
                                                    new APIBookingModel { UserId = (Guid)user.Id, EventId = t.Key.EventId, SeatingId = t.Key.SeatingId, MenuOptionId = t.Key.MenuOptionId, BasePrice = t.Key.BasePrice, NumberOfTickets = t.Count() }).Distinct().ToList<APIBookingModel>());

                //if (ticketBasket.TotalPrice > 0)
                //{
                //    LogMessage(userid + "API processFreeBooking - Ticket basket price is not zero", LogLevel.ERROR);
                //    statusModel.Message = "Request failed: basket value is not zero. BasketId:" + basketId.ToString();
                //    return Json(statusModel, JsonRequestBehavior.AllowGet);
                //}

                if (abm.Count > 0)
                {
                    if (_ticketingService.AddUserToEvent(abm))
                    {
                        LogMessage(userid + "API processFreeBooking user added to event successfully.", LogLevel.INFO);
                        if (ticketBasket.TotalDiscount > 0)
                            _ticketingService.UpdateVoucherUsage(ticketBasket.Tickets[0].VoucherId, ticketBasket.TotalDiscount);
                    }
                    else
                        LogMessage(userid + "API processFreeBooking user could not be added to event.", LogLevel.ERROR);
                }

                statusModel.BookingReferenceNumber = ticketBasket.BookingReference;
                statusModel.Success = true;

                List<BookingMenuModel> lbmm = new List<BookingMenuModel>();

                foreach (APIBookingModel a in abm)
                {
                    if (a.MenuOptionId > 0)
                    {
                        BookingMenuModel bmm = new BookingMenuModel();
                        EventMenuOption emo = _supperClubRepository.GetEventMenuOption(a.MenuOptionId);
                        bmm.menuOptionId = a.MenuOptionId;
                        bmm.menuTitle = emo.Title;
                        bmm.baseTicketCost = a.BasePrice;
                        bmm.numberOfTickets = a.NumberOfTickets;
                        lbmm.Add(bmm);
                    }
                }

                bool hostSuccess = false;
                EmailService.EmailService es = new EmailService.EmailService(_supperClubRepository);
                bool success = es.SendGuestBookedEmails(
                    user.FirstName,
                    user.Email,
                    ticketBasket.TotalTickets,
                    ticketBasket.TotalPrice - ticketBasket.TotalDiscount,
                    ticketBasket.BookingReference,
                    ticketBasket.BookingRequirements,
                    ticketBasket.Tickets[0].EventId,
                    ticketBasket.Tickets[0].SeatingId,
                    lbmm,
                    ticketBasket.Tickets[0].VoucherId,
                    ticketBasket.CCLastDigits,
                    ticketBasket.Tickets[0].CommissionMultiplier,
                    ref hostSuccess);

                statusModel.GuestEmailSentStatus = success;

                if (success)
                    LogMessage("API processFreeBooking Guest Booking Confirmation Email: Sent successfully", LogLevel.INFO);
                else
                {
                    LogMessage("API processFreeBooking Guest Booking Confirmation Email: Failure sending e-mail", LogLevel.ERROR);
                    statusModel.Message = statusModel.Message + "    Booking transaction successful but error while sending Guest Booking Confirmation Email";
                }

                if (hostSuccess)
                    LogMessage("API processFreeBooking Host Booking Information Email: Sent successfully", SupperClub.Logger.LogLevel.INFO);
                else
                {
                    LogMessage("API processFreeBooking Host Booking Information Email: Failure sending e-mail", LogLevel.ERROR);
                    statusModel.Message = statusModel.Message + "    Booking transaction successful but error while sending Guest Booking Confirmation Email";
                }

                //Push notification to FB friends 
                if (user.FacebookId != null)
                {
                    // Push Notification to Friends
                    Event _event = _supperClubRepository.GetEvent(ticketBasket.Tickets[0].EventId);
                    PushNotificationService.MessageService ms = new PushNotificationService.MessageService(_supperClubRepository);
                    ms.FacebookFriendBoughtTicketNotification(user.FirstName + " " + user.LastName, _event.Name, _event.Id, user.Id, user.FacebookId);
                }            
            }
            catch (Exception ex)
            {
                LogMessage("API processFreeBooking: Error while performing transaction." + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                statusModel.Success = false;
                statusModel.Message = statusModel.Message + "Error occurred during free booking transaction processing.";
            }
            return Json(statusModel, JsonRequestBehavior.AllowGet);
        }             

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        [HttpPost]
        public JsonResult checkInUserToEvent(int? bookingReferenceNum, string userId)
        {
            Guid? _userId = null;
            if (!string.IsNullOrEmpty(userId))
                _userId = new Guid(sa.Decrypt(userId));
            string userid = (_userId == null ? "" : "UserId=" + _userId.ToString() + ". ");

            try
            {
                LogMessage(userid + "API checkInUserToEvent request");

                if (_userId == null)
                {
                    LogMessage(userid + "API checkInUserToEvent - userId was not passed.", LogLevel.ERROR);
                    return Json(new { error = "Request failed: userId was not passed." }, JsonRequestBehavior.AllowGet);
                }
                if (!_supperClubRepository.IsValidUser((Guid)_userId))
                {
                    LogMessage(userid + "API checkInUserToEvent: Invalid UserId. UserId=" + _userId.ToString(), LogLevel.ERROR);
                    return Json(new { error = "Request failed: Invalid UserId" }, JsonRequestBehavior.AllowGet);
                }
                if (bookingReferenceNum == null || bookingReferenceNum <= 0)
                {
                    LogMessage(userid + "API checkInUserToEvent - Booking Reference Number was not passed.", LogLevel.ERROR);
                    return Json(new { error = "Request failed: Booking Reference Number was not passed." }, JsonRequestBehavior.AllowGet);
                }
                bool status = _supperClubRepository.CheckInUserToEvent((int)bookingReferenceNum, (Guid)_userId);

                if (!status)
                {
                    LogMessage(userid + "API checkInUserToEvent - Checking in user to event failed", LogLevel.ERROR);
                    return Json(new { error = "Error occurred while checking in user to event." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogMessage(userid + string.Format("API checkInUserToEvent: Successfully checked in user to event"), LogLevel.DEBUG);
                    return Json(new
                    {
                        checkInStatus = "success",
                        checkInDate = DateTime.Now
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage(userid + "API checkInUserToEvent: Error checking in user to event." + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                return Json(new { error = "Error occurred while checking in user to event." }, JsonRequestBehavior.AllowGet);
            }           
        }

        [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
        [HttpPost]
        public JsonResult resetCheckInToEvent(int? bookingReferenceNum, string userId)
        {
            Guid? _userId = null;
            if (!string.IsNullOrEmpty(userId))
                _userId = new Guid(sa.Decrypt(userId));
            string userid = (_userId == null ? "" : "UserId=" + _userId.ToString() + ". ");

            try
            {
                LogMessage(userid + "API resetCheckInToEvent request");

                if (_userId == null)
                {
                    LogMessage(userid + "API resetCheckInToEvent - userId was not passed.", LogLevel.ERROR);
                    return Json(new { ResetCheckInStatus = false, Error = "Request failed: userId was not passed." }, JsonRequestBehavior.AllowGet);
                }
                if (!_supperClubRepository.IsValidUser((Guid)_userId))
                {
                    LogMessage(userid + "API resetCheckInToEvent: Invalid UserId. UserId=" + _userId.ToString(), LogLevel.ERROR);
                    return Json(new { ResetCheckInStatus = false, Error = "Request failed: Invalid UserId" }, JsonRequestBehavior.AllowGet);
                }
                if (bookingReferenceNum == null || bookingReferenceNum <= 0)
                {
                    LogMessage(userid + "API resetCheckInToEvent - Booking Reference Number was not passed.", LogLevel.ERROR);
                    return Json(new { ResetCheckInStatus = false, Error = "Request failed: Booking Reference Number was not passed." }, JsonRequestBehavior.AllowGet);
                }
                bool status = _supperClubRepository.ResetCheckInToEvent((int)bookingReferenceNum, (Guid)_userId);

                if (!status)
                {
                    LogMessage(userid + "API resetCheckInToEvent - Resetting check-in to event failed", LogLevel.ERROR);
                    return Json(new { ResetCheckInStatus = false, Error = "Error occurred while resetting check-in to event." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogMessage(userid + string.Format("API resetCheckInToEvent: Successfully reset check-in user to event"), LogLevel.DEBUG);
                    return Json(new
                    {
                        ResetCheckInStatus = true                        
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage(userid + "API resetCheckInToEvent: Error resetting check-in to even." + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                return Json(new { ResetCheckInStatus = false, Error = "Error occurred while checking in user to event." }, JsonRequestBehavior.AllowGet);
            }
        }
        #region Login Related Private Methods
        private void ConvertFBUserToDomainUser(User user, dynamic model)
        {
            try
            {
                user.FirstName = model.first_name;
                user.LastName = model.last_name;

                if (model.gender == "male")
                    user.Gender = "Male";
                else if (model.gender == "female")
                    user.Gender = "Female";
                else
                    user.Gender = "Unknown";

                user.FBJson = model.ToString();
                user.FBUserOnly = true;
                user.FacebookId = model.id;

                if (model.location != null)
                {
                    string location = model.location.name;
                    if (!string.IsNullOrEmpty(location))
                    {
                        user.Address = location.IndexOf(",") == -1 ? "" : location.Substring(0, location.IndexOf(",")).Trim();
                        user.Country = (location.IndexOf(",") == -1 || location.IndexOf(",") == location.Length) ? "" : location.Substring(location.IndexOf(",") + 1).Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex.Message, ex);
            }
        }
        private void AddToRole(MembershipUser user, string role)
        {
            if (!Roles.IsUserInRole(user.UserName, role))
            {
                OnBeforeAddUserToRole(role);
                if (!Roles.IsUserInRole(user.UserName, role))
                    System.Web.Security.Roles.AddUserToRole(user.UserName, role);
            }
        }

        private void OnBeforeAddUserToRole(string role)
        {
            if (!Roles.RoleExists(role))
                Roles.CreateRole(role);
        }

        private bool AddFBUser(MembershipUser membershipUser, dynamic fbjs, Dictionary<string, object> model)
        {
            try
            {
                if (membershipUser == null)
                    return false;

                User _user = new User();
                _user = ConvertFBUserToDomainUser(_user, fbjs, model);
                _user.Id = (Guid)membershipUser.ProviderUserKey;
                model["Role"] = "Guest";
                bool success = _supperClubRepository.CreateUser(_user);
                if (success)
                    AddToRole(membershipUser, model["Role"].ToString());
                return success;
            }
            catch (Exception ex)
            {
                LogException(ex.Message, ex);
            }
            return false;
        }

        private User ConvertFBUserToDomainUser(User user, dynamic fbjs, Dictionary<string, object> model)
        {
            try
            {
                user.FirstName = model.ContainsKey("first_name") ? model["first_name"].ToString() : "";
                user.LastName = model.ContainsKey("last_name") ? model["last_name"].ToString(): "";

                if (model.ContainsKey("gender") && (model["gender"].ToString() == "male" || model["gender"].ToString() == "female"))
                    user.Gender = model["gender"].ToString();
                else
                    user.Gender = "Unknown";

                user.FBJson = fbjs.ToString();
                user.FBUserOnly = true;
                user.FacebookId = model.ContainsKey("id") ? model["id"].ToString() : user.FacebookId;

                if (model.ContainsKey("location") && model["location"] != null)
                {
                    string location = model["location"].ToString();
                    var jss = new JavaScriptSerializer();
                    Dictionary<string, object> loc = jss.Deserialize<Dictionary<string, object>>(location);
                    string locationInfo = loc["name"].ToString();
                    if (!string.IsNullOrEmpty(locationInfo))
                    {
                        user.Address = locationInfo.IndexOf(",") == -1 ? "" : locationInfo.Substring(0, locationInfo.IndexOf(",")).Trim();
                        user.Country = (locationInfo.IndexOf(",") == -1 || locationInfo.IndexOf(",") == locationInfo.Length) ? "" : locationInfo.Substring(locationInfo.IndexOf(",") + 1).Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex.Message, ex);
            }
            return user;
        }
        #endregion

        #region Create User Error Status Codes
        private static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "E-mail address already exists. Please enter a different e-mail address or log in using your old one.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled.";

                default:
                    return "An unknown error occurred.";
            }
        }
        #endregion

        #region Booking related private methods
        private int getTicketsAvailable(int eventId, int seatingId)
        {
            int numberOfTickets = -100;
            try
            {
                if (seatingId == 0)
                {
                    Event _event = _supperClubRepository.GetEvent(eventId);
                    int ticketsInProgress = _supperClubRepository.GetNumberTicketsInProgressForEvent(eventId);
                    numberOfTickets = _event.TotalNumberOfAvailableSeats - ticketsInProgress;
                }
                else
                {
                    EventSeating _eventSeating = _supperClubRepository.GetEventSeating(seatingId);
                    int ticketsInProgress = _supperClubRepository.GetNumberTicketsInProgressForEvent(eventId, seatingId);
                    numberOfTickets = _eventSeating.TotalNumberOfAvailableSeats - ticketsInProgress;
                }
            }
            catch (Exception ex)
            {
                LogMessage("API getTicketsAvailable: Error getting ticket availability." + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);                
            }
            return numberOfTickets;
        }

        private AddTicketsStatusModel addTicketsToBasket(List<BookingParameterModel> bookingDetails, int eventId, Guid basketId, bool newBasket, Guid userId, int seatingId, decimal commisssion, int voucherId = 0)
        {
            AddTicketsStatusModel atsm = new AddTicketsStatusModel();
            string userid = userId.ToString();
            try
            {
                List<Ticket> tickets = new List<Ticket>();
                Voucher voucher = null;
                if (voucherId > 0)
                {
                    voucher = _supperClubRepository.GetVoucher(voucherId);
                }
                decimal totalPrice = CostCalculator.CostToGuest(bookingDetails.Select(c => c.baseTicketCost * c.numberOfTickets).Sum(),commisssion);
                int numOfTickets = bookingDetails.Select(n => n.numberOfTickets).Sum();
                decimal discount_pd = 0;
                if (voucherId > 0 && voucher.TypeId == (int)VoucherType.PartialPercentOff)
                {
                    decimal t = bookingDetails.OrderBy(b => b.baseTicketCost).Take((int)voucher.FreeBooking).ToList().Select(c => c.baseTicketCost).Sum();                    

                    discount_pd = ((decimal)voucher.OffValue * t) / (numOfTickets * 100);
                }
                foreach (BookingParameterModel bpm in bookingDetails)
                {
                    decimal discount = 0;                            
                    if (bpm != null)
                    {
                        if (bpm.numberOfTickets < 0)
                        {
                            LogMessage(userid + "API addTicketsToBasket - number of tickets were not passed.", LogLevel.ERROR);
                            atsm.Success = false;
                            atsm.Message = "Request failed: number of tickets parameter not passed.";
                            return atsm;
                        }
                        LogMessage(userid + string.Format("API addTicketsToBasket: Adding {1} ticket/s to Basket Id: {0}", basketId, bpm.numberOfTickets), LogLevel.DEBUG);
                        
                        if (voucherId > 0 && voucher != null && bpm.numberOfTickets > 0)
                        {
                            if (voucher.TypeId == (int)VoucherType.PercentageOff)
                            {
                                decimal x = (decimal)voucher.OffValue / 100;
                                discount = x * SupperClub.Domain.CostCalculator.CostToGuest(bpm.baseTicketCost, commisssion);
                            }
                            else if (voucher.TypeId == (int)VoucherType.ValueOff)
                            {
                                discount = ((decimal)voucher.OffValue * SupperClub.Domain.CostCalculator.CostToGuest(bpm.baseTicketCost, commisssion)) / totalPrice;
                            }
                            else if (voucher.TypeId == (int)VoucherType.PartialPercentOff)
                            {
                                discount = discount_pd;
                            }
                            if (discount > SupperClub.Domain.CostCalculator.CostToGuest(bpm.baseTicketCost, commisssion))
                                discount = SupperClub.Domain.CostCalculator.CostToGuest(bpm.baseTicketCost, commisssion);
                            bpm.discount = bpm.numberOfTickets * discount;
                        }
                        for (int i = 0; i < bpm.numberOfTickets; i++)
                        {
                            Ticket ticket = new Ticket((int)eventId, bpm.baseTicketCost, basketId, userId, seatingId, bpm.menuOptionId, commisssion, voucherId, discount);
                            ticket.Description = string.Format("Mobile App: GrubClub ticket (Event Id: {0}, Seating Id: {1}, Menu Option Id: {2})", eventId, seatingId, bpm.menuOptionId);
                            tickets.Add(ticket);
                        }                        
                    }
                }
                TicketBasket basket = new TicketBasket();
                basket = _supperClubRepository.AddToBasket(tickets);
                if (basket.Id == basketId && tickets.Count > 0)
                {
                    LogMessage(userid + string.Format("API addTicketsToBasket: Added {1} ticket/s to Basket Id: {0}", basket.Id, tickets.Count), LogLevel.DEBUG);                   
                    atsm.Success = true;
                    atsm.Message = "Tickets added to basket";
                    atsm.NumberOfTicketsAdded = tickets.Count;
                    atsm.Discount = basket.TotalDiscount;
                    atsm.TotalAfterDiscount = (basket.TotalPrice - basket.TotalDiscount) < 0 ? 0 : (basket.TotalPrice - basket.TotalDiscount);
                }
                else
                {
                    atsm.Success = false;
                    atsm.Message = "No tickets to add to basket.";                    
                }
            }
            catch (Exception ex)
            {
                LogMessage("API getTicketsAvailable: Error adding tickets to basket." + ex.Message + "  StackTrace:" + ex.StackTrace, LogLevel.ERROR);
                atsm.Success = false;
                atsm.Message = "Error occurred while adding tickets to basket.";
            }
            return atsm;
        }

        private bool isValidPhoneNumber(string phoneNumber)
        {
            Match match = Regex.Match(phoneNumber.Trim(), @"^\(?(?:(?:0(?:0|11)\)?[\s-]?\(?|\+)44\)?[\s-]?\(?(?:0\)?[\s-]?\(?)?|0)(?:\d{2}\)?[\s-]?\d{4}[\s-]?\d{4}|\d{3}\)?[\s-]?\d{3}[\s-]?\d{3,4}|\d{4}\)?[\s-]?(?:\d{5}|\d{3}[\s-]?\d{3})|\d{5}\)?[\s-]?\d{4,5}|8(?:00[\s-]?11[\s-]?11|45[\s-]?46[\s-]?4\d))(?:(?:[\s-]?(?:x|ext\.?\s?|\#)\d+)?)$", RegexOptions.IgnoreCase);
            return match.Success;
        }
        private bool isValidPostCode(string postcode)
        {
            Match match1 = Regex.Match(postcode.Trim(), @"^([Gg][Ii][Rr] 0[Aa]{2})|((([A-Za-z][0-9]{1,2})|(([A-Za-z][A-Ha-hJ-Yj-y][0-9]{1,2})|(([A-Za-z][‌​0-9][A-Za-z])|([A-Za-z][A-Ha-hJ-Yj-y][0-9]?[A-Za-z])))) {0,1}[0-9][A-Za-z]{2})$", RegexOptions.IgnoreCase);
            Match match2 = Regex.Match(postcode.Trim(), @"^((([A-PR-UWYZ][0-9])|([A-PR-UWYZ][0-9][0-9])|([A-PR-UWYZ][A-HK-Y][0-9])|([A-PR-UWYZ][A-HK-Y][0-9][0-9])|([A-PR-UWYZ][0-9][A-HJKSTUW])|([A-PR-UWYZ][A-HK-Y][0-9][ABEHMNPRVWXY]))) | ^((GIR)[ ]?(0AA))$|^(([A-PR-UWYZ][0-9])[ ]?([0-9][ABD-HJLNPQ-UW-Z]{0,2}))$|^(([A-PR-UWYZ][0-9][0-9])[ ]?([0-9][ABD-HJLNPQ-UW-Z]{0,2}))$|^(([A-PR-UWYZ][A-HK-Y0-9][0-9])[ ]?([0-9][ABD-HJLNPQ-UW-Z]{0,2}))$|^(([A-PR-UWYZ][A-HK-Y0-9][0-9][0-9])[ ]?([0-9][ABD-HJLNPQ-UW-Z]{0,2}))$|^(([A-PR-UWYZ][0-9][A-HJKS-UW0-9])[ ]?([0-9][ABD-HJLNPQ-UW-Z]{0,2}))$|^(([A-PR-UWYZ][A-HK-Y0-9][0-9][ABEHMNPRVWXY0-9])[ ]?([0-9][ABD-HJLNPQ-UW-Z]{0,2}))$", RegexOptions.IgnoreCase);
            if (match1.Success || match2.Success)
                return true;
            else
                return false;
        }
        #endregion
    }    
}
