using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Net;
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
using System.Text;
using System.Web.UI;
using Newtonsoft.Json;

namespace SupperClub.Controllers
{
    [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
    public class EventController : BaseController
    {
        private string _eventViewCookieKey = WebConfigurationManager.AppSettings["EventViewCookieKey"];
        private int _eventViewCookieExpirationInMonths = int.Parse(WebConfigurationManager.AppSettings["EventViewCookieExpirationInMonths"]);
        private decimal _commissionPercentage = decimal.Parse(WebConfigurationManager.AppSettings["CommissionPercent"]);

        public EventController(ISupperClubRepository supperClubRepository)
        {
            _supperClubRepository = supperClubRepository;
        }

        #region All User Event Actions
        public ActionResult Details(int? eventId = null)
        {
            if (eventId == null)
            {
                LogMessage("Tried to visit Event/Details with no eventId Paramter");
                return RedirectToAction("Index", "Search");
            }
            LogMessage("View Event: " + eventId.ToString());

            Event _event = _supperClubRepository.GetEvent((int)eventId);

            if (_event == null)
            {
                LogMessage("Event did not exist for Id: " + eventId.ToString(), LogLevel.ERROR);
                return RedirectToAction("Index", "Search");
            }
            // For Diets checkboxes
            ViewBag.StandardDiets = _supperClubRepository.GetStandardDiets();
            ViewBag.AllergyDiets = _supperClubRepository.GetAllergyDiets();
            ViewBag.OtherAllergyDiets = _supperClubRepository.GetOtherAllergyDiets(_event.Id);
            // For Cuisine Drop Down
            ViewBag.Cuisines = new SelectList(_supperClubRepository.GetCuisines(), "Id", "Name");
            // For SupperClub Drop Down
            ViewBag.SupperClubs = new SelectList(_supperClubRepository.GetAllActiveSupperClubs(), "Id", "Name");
            // For Tags list
            ViewBag.Tags = _supperClubRepository.GetTagsWithoutCuisine();
            // For city list
            ViewBag.Cities = _supperClubRepository.GetCities();
            // For Tags list
            ViewBag.Areas = _supperClubRepository.GetAreas();
            // For Event Description
            string[] separator = new string[] { "|&|" };
            ViewBag.EventDescriptionList = _event.Description.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            ViewBag.DescriptionArrayLength = ViewBag.EventDescriptionList.Length;

            //return RedirectToActionPermanent("DetailsByIdWithName", "Event", new { eventid = eventId, eventSeoFriendlyName = _event.UrlFriendlyName });
            _event.CostTextDisplay = string.Format("{0}", _event.Cost.ToString("F4"));

            if (_event.MultiSeating)
            {
                int _totalAvailableSeats = 0;
                foreach (EventSeating es in _event.EventSeatings)
                {
                    es.AvailableSeats = es.TotalNumberOfAvailableSeats - _supperClubRepository.GetNumberTicketsInProgressForEvent((int)eventId, es.Id);
                    _totalAvailableSeats += es.AvailableSeats;
                }
                ViewBag.AvailableSeats = _totalAvailableSeats.ToString();
            }
            else
                ViewBag.AvailableSeats = (_event.TotalNumberOfAvailableSeats - _supperClubRepository.GetNumberTicketsInProgressForEvent((int)eventId)).ToString();
            return View(_event);
        }

        public ActionResult Details1(int? eventId = null)
        {
            EventViewModel _eventViewModel = new EventViewModel();
            _eventViewModel.NumberOfTickets = 1;
            if (eventId == null)
            {
                LogMessage("Tried to visit Event/Details with no eventId Paramter");
                return RedirectToAction("Index", "Search");
            }
            LogMessage("View Event: " + eventId.ToString());

            Event _event = _supperClubRepository.GetEvent((int)eventId);
            if (_event == null)
            {
                LogMessage("Event did not exist for Id: " + eventId.ToString(), LogLevel.ERROR);
                return RedirectToAction("Index", "Search");
            }
            ViewBag.ShortEventDescription = HtmlContentFormatter.GetSubString(_event.EventDescription, 800);
            // For Diets checkboxes
            ViewBag.StandardDiets = _supperClubRepository.GetStandardDiets();
            ViewBag.AllergyDiets = _supperClubRepository.GetAllergyDiets();
            ViewBag.OtherAllergyDiets = _supperClubRepository.GetOtherAllergyDiets(_event.Id);
            // For Cuisine Drop Down
            ViewBag.Cuisines = new SelectList(_supperClubRepository.GetCuisines(), "Id", "Name");
            ViewBag.EventURL = SupperClub.Code.ServerMethods.ServerUrl + _event.SupperClub.UrlFriendlyName + "/" + _event.Id.ToString();
            //List<Review> rl = new List<Review>();
            //foreach (Event e in _event.SupperClub.Events)
            //    {
            //        if (e.Reviews != null && e.Reviews.Count > 0)
            //        {
            //            foreach (Review r in e.Reviews)
            //            {                            
            //                rl.Add(r);
            //            }
            //        }
            //    }
            IList<Review> lstReviews = _supperClubRepository.GetSupperClubReviews((int)eventId, _event.SupperClubId);
            ViewBag.SupperClubReviews = lstReviews;// _supperClubRepository.GetSupperClubReviews((int)eventId, _event.SupperClubId);
            if (UserMethods.IsLoggedIn)
            {
                ViewBag.IsEventWishListed = _supperClubRepository.IsCurrentEventUsersFavourite(_event.Id, UserMethods.CurrentUser.Id);
                ViewBag.IsFollowingChef = _supperClubRepository.IsCurrentSupperClubUsersFavourite(_event.SupperClubId, UserMethods.CurrentUser.Id);
                if (_event.NumberOfAttendeeGuests(SupperClub.Code.UserMethods.UserId) > 0)
                    ViewBag.SpecialRequirements = _supperClubRepository.GetBookingRequirements(_event.Id, UserMethods.CurrentUser.Id);

            }
            if (_event.MultiSeating)
            {
                int _totalAvailableSeats = 0;
                foreach (EventSeating es in _event.EventSeatings)
                {
                    es.AvailableSeats = es.TotalNumberOfAvailableSeats - _supperClubRepository.GetNumberTicketsInProgressForEvent((int)eventId, es.Id);
                    _totalAvailableSeats += es.AvailableSeats;
                }
                ViewBag.AvailableSeats = _totalAvailableSeats.ToString();
            }
            else
                ViewBag.AvailableSeats = (_event.TotalNumberOfAvailableSeats - _supperClubRepository.GetNumberTicketsInProgressForEvent((int)eventId)).ToString();
            _eventViewModel.Event = _event;
            //return RedirectToActionPermanent("DetailsByIdWithName", "Event", new { eventid = eventId, eventSeoFriendlyName = _event.UrlFriendlyName });
            //_event.CostTextDisplay = string.Format("{0}", _event.Cost.ToString("F"));

            //ViewBag.AvailableSeats = (_event.TotalNumberOfAvailableSeats - _supperClubRepository.GetNumberTicketsInProgressForEvent((int)eventId)).ToString();
            


            ViewBag.Reviews = lstReviews.OrderByDescending(a => a.DateCreated).Distinct().ToList<Review>();
            return View(_eventViewModel);
        }
        /*
        public ActionResult newEventPage(int? eventId = null)
        {
            if (eventId == null)
            {
                LogMessage("Tried to visit Event/Details with no eventId Paramter");
                return RedirectToAction("Index", "Search");
            }
            LogMessage("View Event: " + eventId.ToString());
            Event _event = _supperClubRepository.GetEvent((int)eventId);
            return View("newEventPage", _event);
        }
         */
        public ActionResult angularEvent()
        {
            return View("angulartest");
        }
        public JsonResult getEventDetails(int? eventId = null)
        {
            if (eventId != null && eventId > 0)
            {
                LogMessage("Web API Get Event Details: " + eventId.ToString());

                Event _event = _supperClubRepository.GetEvent((int)eventId);
                if (_event == null)
                {
                    LogMessage("Event did not exist for Id: " + eventId.ToString(), LogLevel.ERROR);
                }
                var jsonSeating = new List<object>();
                if (_event.MultiSeating)
                {
                    foreach (EventSeating es in _event.EventSeatings)
                    {
                        jsonSeating.Add(new
                        {
                            seatingId = es.Id,
                            seatingStartTime = es.Start,
                            seatingEndTime = es.End,
                            seatingGuests = es.Guests,
                            seatingAvailableSeats = es.AvailableSeats
                        });
                    }
                }
                var jsonMenuOption = new List<object>();
                if (_event.MultiMenuOption)
                {
                    foreach (EventMenuOption em in _event.EventMenuOptions)
                    {
                        jsonMenuOption.Add(new
                        {
                            menuId = em.Id,
                            menuTitle = em.Title,
                            menuDescription = em.Description,
                            menuPrice = em.CostToGuest
                        });
                    }
                }
                var serializer = new JavaScriptSerializer();
                return Json(new
                {
                    name = _event.Name,
                    startDateTime = _event.Start,
                    endDateTime = _event.End,
                    description = _event.EventDescription,
                    ticketCost = _event.CostToGuest,
                    address = _event.Address,
                    addressLine2 = _event.Address2,
                    addressCity = _event.City,
                    addressPostCode = _event.PostCode,
                    latitude = _event.Latitude,
                    longitude = _event.Longitude,
                    grubclubId = _event.SupperClub.Id,
                    grubclubName = _event.SupperClub.Name,
                    totalGuests = _event.TotalEventGuests,
                    availableSeats = _event.TotalNumberOfAvailableSeats,
                    imageURL = Url.Content(ServerMethods.EventImagePath + _event.ImagePath),
                    isMultiSeating = _event.MultiSeating,
                    multiSeatingDetails = _event.MultiSeating ? serializer.Serialize(jsonSeating) : "",
                    isMultiMenu = _event.MultiMenuOption,
                    multiMenuDetails = _event.MultiMenuOption ? serializer.Serialize(jsonMenuOption) : "",
                    alcohol = _event.Alcohol,
                    menu = _event.Menu,
                    diet = _event.Diets,
                    cuisine = _event.Cuisines,
                    minSeatingTime = _event.MinSeatingTime
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                LogMessage("Request failed: Event Id not passed", LogLevel.ERROR);
                return Json(new { error = "Request failed: Event Id not passed. Please pass Event Id as part of request." }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DetailsByIdAndName(int? eventId = null, string eventSeoFriendlyName = null)
        {
            if (eventId == null)
            {
                LogMessage("Tried to visit Event/Details with no eventId Parameter");
                return RedirectToAction("Index", "Search");
            }
            LogMessage("View Event: " + eventId.ToString());

            Event _event = _supperClubRepository.GetEvent((int)eventId);
            if (_event == null)
            {
                LogMessage("Event did not exist for Id: " + eventId.ToString(), LogLevel.ERROR);
                return RedirectToAction("Index", "Search");
            }

            if ((eventSeoFriendlyName == null && _event != null) || (eventSeoFriendlyName != null && _event != null && _event.UrlFriendlyName != eventSeoFriendlyName))
                return RedirectToActionPermanent("DetailsByIdWithName", new { eventId = eventId, eventSeoFriendlyName = _event.UrlFriendlyName, hostname = _event.SupperClub.UrlFriendlyName });

            return RedirectToActionPermanent("DetailsByIdWithName", new { eventId = eventId, eventSeoFriendlyName = _event.UrlFriendlyName, hostname = _event.SupperClub.UrlFriendlyName });
        }

        public ActionResult DetailsByIdWithName(int? eventId = null, string eventSeoFriendlyName = null, string hostname = null)
        {
            //if (eventId == null)
            //{
            //    LogMessage("Tried to visit Event/Details with no eventId Parameter");
            //    return RedirectToAction("Index", "Search");
            //}
            //LogMessage("View Event: " + eventId.ToString());

            //Event _event = _supperClubRepository.GetEvent((int)eventId);
            //if (_event == null)
            //{
            //    LogMessage("Event did not exist for Id: " + eventId.ToString(), LogLevel.ERROR);
            //    return RedirectToAction("Index", "Search");
            //}

            //if ((eventSeoFriendlyName == null && _event != null) || (eventSeoFriendlyName != null && _event != null && _event.UrlFriendlyName != eventSeoFriendlyName))
            //    return RedirectToActionPermanent("DetailsByIdWithName", new { eventId = eventId, eventSeoFriendlyName = _event.UrlFriendlyName });

            //// For Diets checkboxes
            //ViewBag.StandardDiets = _supperClubRepository.GetStandardDiets();
            //ViewBag.AllergyDiets = _supperClubRepository.GetAllergyDiets();
            //ViewBag.OtherAllergyDiets = _supperClubRepository.GetOtherAllergyDiets(_event.Id);
            //// For Cuisine Drop Down
            //ViewBag.Cuisines = new SelectList(_supperClubRepository.GetCuisines(), "Id", "Name");
            //// For SupperClub Drop Down
            //ViewBag.SupperClubs = new SelectList(_supperClubRepository.GetAllActiveSupperClubs(), "Id", "Name");
            //// For Tags list
            //ViewBag.Tags = _supperClubRepository.GetTags();

            //_event.CostTextDisplay = string.Format("{0}", _event.Cost.ToString("F"));

            //if (_event.MultiSeating)
            //{
            //    int _totalAvailableSeats = 0;
            //    foreach (EventSeating es in _event.EventSeatings)
            //    {
            //        es.AvailableSeats = es.TotalNumberOfAvailableSeats - _supperClubRepository.GetNumberTicketsInProgressForEvent((int)eventId, es.Id);
            //        _totalAvailableSeats += es.AvailableSeats;
            //    }
            //    ViewBag.AvailableSeats = _totalAvailableSeats.ToString();
            //}
            //else
            //    ViewBag.AvailableSeats = (_event.TotalNumberOfAvailableSeats - _supperClubRepository.GetNumberTicketsInProgressForEvent((int)eventId)).ToString();
            //return View("Details", _event);
            EventViewModel _eventViewModel = new EventViewModel();
            _eventViewModel.NumberOfTickets = 1;
            if (eventId == null)
            {
                LogMessage("Tried to visit Event/Details with no eventId Paramter");
                return RedirectToAction("Index", "Search");
            }
            LogMessage("View Event: " + eventId.ToString());
            try
            {
                Event _event = _supperClubRepository.GetEvent((int)eventId);
                if (_event == null)
                {
                    LogMessage("Event did not exist for Id: " + eventId.ToString(), LogLevel.ERROR);
                    return RedirectToAction("Index", "Search");
                }

                if (Request.UserAgent.ToLower().Contains("googlebot"))
                    return RedirectToActionPermanent("MasterDetailsByName", new { hostname = _event.SupperClub.UrlFriendlyName, eventSeoFriendlyName = _event.UrlFriendlyName });

                if (((eventSeoFriendlyName == null || hostname == null) && _event != null) || (hostname != null && eventSeoFriendlyName != null && _event != null && (_event.UrlFriendlyName != eventSeoFriendlyName || _event.SupperClub.UrlFriendlyName != hostname)))
                    return RedirectToActionPermanent("DetailsByIdWithName", new { eventId = eventId, eventSeoFriendlyName = _event.UrlFriendlyName, hostname = _event.SupperClub.UrlFriendlyName });

                // For Diets checkboxes
                ViewBag.StandardDiets = _supperClubRepository.GetStandardDiets();
                ViewBag.AllergyDiets = _supperClubRepository.GetAllergyDiets();
                ViewBag.OtherAllergyDiets = _supperClubRepository.GetOtherAllergyDiets(_event.Id);
                // For Cuisine Drop Down
                ViewBag.Cuisines = new SelectList(_supperClubRepository.GetCuisines(), "Id", "Name");
                ViewBag.EventURL = HttpUtility.UrlEncode(SupperClub.Code.ServerMethods.ServerUrl + _event.SupperClub.UrlFriendlyName + "/" + _event.UrlFriendlyName + "/" + _event.Id.ToString());
                string _imgUrl = SupperClub.Code.ServerMethods.EventImagePath.Remove(0, 2);
                ViewBag.ImageURL = HttpUtility.UrlEncode(SupperClub.Code.ServerMethods.ServerUrl + _imgUrl + _event.ImagePath);
                ViewBag.TwitterText = "This " + _event.Name + " looks amazing! Who wants to come with me?! -";
                ViewBag.ShortEventDescription = HtmlContentFormatter.GetSubString(_event.EventDescription, 800);

                if (string.IsNullOrEmpty(_event.SupperClub.Description))
                    _event.SupperClub.Description = string.Empty;

                if (_event.EventRecommendations == null || _event.EventRecommendations.Count == 0)
                {
                    IList<PopularEvent> lpe = _supperClubRepository.GetActivePopularEventsForEventPage(_event.Id);
                    if (lpe != null)
                    {
                        foreach (PopularEvent pe in lpe)
                        {
                            EventRecommendation er = new EventRecommendation();
                            er.EventId = _event.Id;
                            er.RecommendedEventId = pe.EventId;
                            er.RecommendedEvent = pe.Event;
                            _event.EventRecommendations.Add(er);
                        }
                    }
                }
                if (UserMethods.IsLoggedIn)
                {
                    ViewBag.IsEventWishListed = _supperClubRepository.IsCurrentEventUsersFavourite(_event.Id, UserMethods.CurrentUser.Id);
                    ViewBag.IsFollowingChef = _supperClubRepository.IsCurrentSupperClubUsersFavourite(_event.SupperClubId, UserMethods.CurrentUser.Id);

                    int userseating = 0;
                    if (_event.MultiSeating)
                    {
                        //foreach (Event e in UserMethods.CurrentUser.FutureEvents)
                        //{

                        //    if (_event.Seatings.Contains(e.DefaultSeatingId.ToString()))
                        //    {
                        //        userseating = e.DefaultSeatingId;
                        //        break;
                        //    }

                        //}

                        EventSeating es = _supperClubRepository.GetEventSeatingForAUser(_event.Id, UserMethods.CurrentUser.Id);
                        if (es != null)
                        {
                            userseating = es.Id;
                            ViewBag.DefaultSeatingId = userseating;
                        }
                    }

                    string userMenuOption = string.Empty;
                    if (_event.MultiMenuOption)
                    {

                        //foreach (Event e in UserMethods.CurrentUser.FutureEvents)
                        //{
                        //    if (_event.MenuOptions.Contains(e.DefaultMenuOptionId.ToString()))
                        //    {
                        //        userMenuOption = e.DefaultMenuOptionId;
                        //        break;
                        //    }

                        //}
                        IList<EventMenuOption> lstUserMenus = _supperClubRepository.GetEventMenuOptionForAUser(_event.Id, UserMethods.CurrentUser.Id);

                        foreach (EventMenuOption em in lstUserMenus)
                            userMenuOption += em.Title + "<br/>";

                        ViewBag.DefaultMenuOption = userMenuOption;
                    }

                }
                if (_event.MultiSeating)
                {
                    int _totalAvailableSeats = 0;
                    foreach (EventSeating es in _event.EventSeatings)
                    {
                        es.AvailableSeats = es.TotalNumberOfAvailableSeats - _supperClubRepository.GetNumberTicketsInProgressForEvent((int)eventId, es.Id);
                        _totalAvailableSeats += es.AvailableSeats;
                    }
                    ViewBag.AvailableSeats = _totalAvailableSeats.ToString();
                }
                else
                    ViewBag.AvailableSeats = (_event.TotalNumberOfAvailableSeats - _supperClubRepository.GetNumberTicketsInProgressForEvent((int)eventId)).ToString();
                _eventViewModel.Event = _event;
                IList<Review> lstReviews = _supperClubRepository.GetSupperClubReviews((int)eventId, _event.SupperClubId);
                ViewBag.Reviews = lstReviews.OrderByDescending(a => a.DateCreated).Distinct().ToList<Review>();
                string eventTag = string.Empty;
                foreach(EventTag et in _event.EventTags)
                {
                    eventTag += et.Tag.Name + ",";
                }
                Segment.Analytics.Client.Track(_segmentMethod.GetSegmentUser(), "View Event", new Segment.Model.Properties() {
                    { "EventName", _event.Name  },
                    { "ChefProfileName", _event.SupperClub.Name },
                    { "EventLocation", _event.Address + (string.IsNullOrEmpty(_event.Address2) ? "":("," + _event.Address2)) },
                    { "EventPostCode", _event.PostCode },
                    { "EventDate", _event.Start.ToString()},
                    { "BYOB", _event.Alcohol ? "Yes":"No" },
                    { "Charity", _event.Charity ? "Yes":"No" },
                    { "Price", _event.CostToGuest.ToString() },
                    { "NumberOfReviews", lstReviews.Count.ToString() },
                    { "AverageStarRating", _event.SupperClub.AverageRank.ToString() },
                    { "NumberOfPeopleAlreadyOnTable", _event.TotalEventGuests.ToString() },
                    { "Tags", eventTag },
                    { "DayOfTheWeek", _event.Start.DayOfWeek },
                    { "DateApproved", _event.DateApproved },
                    { "IsUserLoggedIn", UserMethods.CurrentUser != null ? "Yes" : "No" },
                    { "DidUserBoughtAnEventInPast", UserMethods.CurrentUser != null ? (UserMethods.CurrentUser.EventAttendees != null ? "Yes" : "No") : "Info not available" },
                    { "PageViewDate", DateTime.Now.ToString() }
                });
                //manage event view cookie
                string _eventViewCookie = Web.Helpers.Utils.CookieStore.GetCookie(_eventViewCookieKey);
                if(string.IsNullOrEmpty(_eventViewCookie))
                {
                    _eventViewCookie = serializeCookieData(createEventViewList((int)eventId));
                    Web.Helpers.Utils.CookieStore.SetCookie(_eventViewCookieKey, _eventViewCookie, DateTime.Now.AddMonths(_eventViewCookieExpirationInMonths) - DateTime.Now);
                }
                else
                {
                    _eventViewCookie = serializeCookieData(updateEventViewList((int)eventId,deserializeCookieData(_eventViewCookie)));
                    Web.Helpers.Utils.CookieStore.SetCookie(_eventViewCookieKey, _eventViewCookie, DateTime.Now.AddMonths(_eventViewCookieExpirationInMonths) - DateTime.Now);
                }
                return View("Details1", _eventViewModel);
            }
            catch (Exception ex)
            {
                LogMessage("Error getting event details. EventId=" + eventId.ToString() + " Error:" + ex.Message + " Stack Trace:" + ex.StackTrace, LogLevel.ERROR);
                return RedirectToAction("Index", "Search");
            }            
            //return RedirectToActionPermanent("DetailsByIdWithName", "Event", new { eventid = eventId, eventSeoFriendlyName = _event.UrlFriendlyName });
            //_event.CostTextDisplay = string.Format("{0}", _event.Cost.ToString("F"));

            //ViewBag.AvailableSeats = (_event.TotalNumberOfAvailableSeats - _supperClubRepository.GetNumberTicketsInProgressForEvent((int)eventId)).ToString();

        }

        public ActionResult MasterDetailsByName(string hostname = null, string eventSeoFriendlyName = null)
        {
            MasterEventModel _masterEvent = new MasterEventModel();
            if (string.IsNullOrEmpty(eventSeoFriendlyName))
            {
                LogMessage("MasterDetailsByName: Tried to visit Event/Details with no eventSeoFriendlyName Parameter", LogLevel.ERROR);
                return RedirectToAction("Index", "Search");
            }
            LogMessage("MasterDetailsByName: SEO name=" + eventSeoFriendlyName);

            Event _event = _supperClubRepository.GetEventByName(eventSeoFriendlyName);
            if (_event != null && _event.Id > 0)
            {
                if (string.IsNullOrEmpty(hostname) || hostname != _event.SupperClub.UrlFriendlyName)
                    return RedirectToActionPermanent("MasterDetailsByName", new { hostname = _event.SupperClub.UrlFriendlyName, eventSeoFriendlyName = _event.UrlFriendlyName });
                if (UserMethods.IsLoggedIn)
                {
                    ViewBag.IsFollowingChef = _supperClubRepository.IsCurrentSupperClubUsersFavourite(_event.SupperClubId, UserMethods.CurrentUser.Id);
                }
                if (Request.UrlReferrer != null)
                {
                    Uri uriAddress = new Uri(Request.UrlReferrer.ToString());
                    if(uriAddress.AbsolutePath.Contains("/pop-up-restaurants"))
                    {
                        Segment.Analytics.Client.Track(_segmentMethod.GetSegmentUser(),
                            "Clicked on View Other Dates",
                            new Segment.Model.Properties() {
                            { "EventLink", ServerMethods.ServerUrl + hostname + "/" + eventSeoFriendlyName  }                            
                            });
                    }
                }
                IList<Event> futureEvents = _supperClubRepository.GetActiveFutureEvents(eventSeoFriendlyName);
                if (futureEvents != null && futureEvents.Count > 0)
                {
                    _masterEvent.MasterEvent = futureEvents[0];
                    _masterEvent.FutureEventList = new List<ChildEvent>();
                    foreach (Event e in futureEvents)
                    {
                        ChildEvent ce = new ChildEvent();
                        ce.EventId = e.Id;
                        ce.EventUrl = ServerMethods.ServerUrl + e.SupperClub.UrlFriendlyName + "/" + e.UrlFriendlyName + "/" + e.Id.ToString();
                        ce.EventDate = e.Start;
                        ce.Soldout = e.TotalNumberOfAvailableSeats <= 0 ? true : false;
                        _masterEvent.FutureEventList.Add(ce);
                    }
                    var priceList = (from f in futureEvents select new { f.Cost, f.Commission }).Distinct().ToList();
                    decimal minPrice = 0;
                    decimal comm = 0;
                    if (priceList.Count > 1)
                    {
                        minPrice = priceList.Min(x => x.Cost);
                        comm = priceList.Where(x => x.Cost == minPrice).Select(x => x.Commission).FirstOrDefault();
                    }

                    else
                    {
                        minPrice = priceList[0].Cost;
                        comm = priceList[0].Commission;
                    }
                    _masterEvent.LowestPrice = "£" + CostCalculator.CostToGuest(minPrice, comm).ToString("0.00") + (priceList.Count > 1 ? "onwards" : "");

                   
                }
                else
                {
                    //Event pastEvent = _supperClubRepository.GetLastActivePastEvent(eventSeoFriendlyName);
                    //if (pastEvent == null)
                    //{
                    //    LogMessage("MasterDetailsByName: No Past/future events found with this name. UrlFriendlyName=" + eventSeoFriendlyName, LogLevel.ERROR);
                    //    return RedirectToAction("Index", "Search");
                    //}
                    //else
                    //{
                    //    _masterEvent.MasterEvent = pastEvent;
                    //    _masterEvent.LowestPrice = "£" + CostCalculator.CostToGuest(pastEvent.Cost, pastEvent.Commission).ToString("0.00");
                    //}
                    return RedirectToAction("DetailsByName", "Host", new { hostname = hostname });
                }
            }
            else
            {
                LogMessage("MasterDetailsByName: Invalid eventSeoFriendlyName Parameter");
                return RedirectToAction("Index", "Search");
            }
            return View("MasterDetails", _masterEvent);
        }
        [Authorize]
        [HttpPost]
        public JsonResult AddEventToFavourite(int eventId)
        {
            bool status = false;
            try
            {
                status = addEventToUserList(eventId);
                if (status)
                {
                    LogMessage("Added event to user's favourites.");
                    //Segment Tracking
                    Event _event = _supperClubRepository.GetEvent(eventId);
                    bool bookedTicket = _event.EventAttendees.Where(ea => ea.UserId == UserMethods.CurrentUser.Id).ToList() == null ? false : true;
                    if (_event != null)
                    {
                        Segment.Analytics.Client.Track(_segmentMethod.GetSegmentUser(), 
                            "Wishlist Event", 
                            new Segment.Model.Properties() {
                            { "EventName", _event.Name },
                            { "UserBookedThisEvent", bookedTicket ? "Yes" : "No" },                  
                            { "WishlistDateTime", DateTime.Now.ToString() }
                            });
                    }
                    UserMethods.CurrentUser.WishListedEventIds.Add(eventId);
                    UserFavouriteEvent ufe = new UserFavouriteEvent();
                    ufe.EventId = eventId;
                    ufe.UserId = UserMethods.CurrentUser.Id;
                    UserMethods.CurrentUser.WishlistedEvents.Add(ufe);
                    return Json(new { success = status, message = "Added event to user's favourite event list." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogMessage("Failed to add event to user's favourite event list.");
                    return Json(new { success = status, message = "Failed to add event to user's favourite event list." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage("Error adding event to user's favourite event list.");
                return Json(new { success = false, message = "Error adding event to user's favourite event list." }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        public ActionResult WishListEvent(int eventId, string rurl)
        {
            bool jr = addEventToUserList(eventId);
            if (jr)
            {
                UserMethods.CurrentUser.WishListedEventIds.Add(eventId);
                UserFavouriteEvent ufe = new UserFavouriteEvent();
                ufe.EventId = eventId;
                ufe.UserId = UserMethods.CurrentUser.Id;
                UserMethods.CurrentUser.WishlistedEvents.Add(ufe);
            }
            if (!string.IsNullOrEmpty(rurl))
                return Redirect(rurl);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        [HttpPost]
        public JsonResult RemoveEventFromFavourite(int eventId)
        {
            bool status = false;
            try
            {
                status = _supperClubRepository.RemoveEventFromFavourite(eventId, (Guid)UserMethods.UserId);
                if (status)
                {
                    LogMessage("Removed event from user's favourites.");
                    UserMethods.CurrentUser.WishListedEventIds.Remove(eventId);
                    UserMethods.CurrentUser.WishlistedEvents = UserMethods.CurrentUser.WishlistedEvents.Where(x => x.EventId != eventId).ToList();
                    return Json(new { success = true, message = "Removed event from user's favourite event list." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    LogMessage("Failed to remove event from user's favourite event list.");
                    return Json(new { success = false, message = "Failed to remove event from user's favourite event list." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage("Error removing event from user's favourite event list.");
                return Json(new { success = false, message = "Error removing event from user's favourite event list." }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult LoadReviews(int? eventId, int? supperClubId)
        {
            if (eventId != null && supperClubId != null)
            {
                return Json(new { reviews = GetReviews((int)eventId, (int)supperClubId) }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                LogMessage("EventController-> LoadReviews: Did not load the reviews as eventId or supperClubId is null", LogLevel.ERROR);
                return Json(new { reviews = eventId }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult ChefEventReviews()
        {
            var listReviews = new List<object>();
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            IList<Review> lstReviews = _supperClubRepository.GetSupperClubReviews(3476, 594);
            for (int i = 0; i < lstReviews.Count; i++)
            {
                var tempReview = lstReviews[i];
                string guestName = "Guest";
                if (tempReview.User != null)
                    guestName = tempReview.User.FirstName;
                else if (!string.IsNullOrEmpty(tempReview.GuestName))
                    guestName = tempReview.GuestName;
                else if (!string.IsNullOrEmpty(tempReview.Email))
                    guestName = _supperClubRepository.GetGuestName(tempReview.Email);
                try
                {
                    listReviews.Add(new
                    {
                        eventId = tempReview.EventId,
                        reviewId = tempReview.Id,
                        dateCreated = tempReview.DateCreated.ToString("ddd, d MMM yyyy"),
                        dateCreatedSEO = tempReview.DateCreated.ToString("yyyy-MM-ddThh:mm"),
                        rating = tempReview.Rating == null ? 0 : tempReview.Rating,
                        reviewText = tempReview.PublicReview == null ? "" : tempReview.PublicReview,
                        userName = guestName,
                        reviewTitle = tempReview.Title == null ? "" : tempReview.Title,
                        hostResponse = tempReview.HostResponse,
                        hostResponseDate = tempReview.HostResponseDate.ToString("ddd, d MMM yyyy"),
                        adminResponse = tempReview.AdminResponse,
                        adminResponseDate = tempReview.AdminResponseDate.ToString("ddd, d MMM yyyy"),
                        isHost = (tempReview.Event.SupperClub.UserId == UserMethods.UserId) ? true : false,
                        isAdmin = UserMethods.IsAdmin,
                        isReviewUser = (tempReview.UserId == UserMethods.UserId) ? true : false


                    });
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
           
            
            ViewBag.Reviews = lstReviews;//.OrderByDescending(a=>a.DateCreated).Distinct().ToList<Review>();
            return View();
                
        }

        [Authorize(Roles = "Host, Admin")]
        public ActionResult PrivateReviews(int eventId)
        {
            PrivateReviewModel model = new PrivateReviewModel();

            model.Reviews = new List<Review>();

            try
            {
                SupperClub.Domain.Event _event = _supperClubRepository.GetEvent(eventId);
                Event e = new Event();


                if (_event != null && (UserMethods.IsAdmin || UserMethods.UserId == _event.SupperClub.UserId))
                {
                    List<Review> lstReview = _event.Reviews.Where(x => x.PrivateReview != null).ToList();

                    foreach (Review tempReview in lstReview)
                    {

                        Review rv = new Review();

                        rv.DateCreated = tempReview.DateCreated;
                        rv.Rating = tempReview.Rating == null ? 0 : tempReview.Rating;
                        rv.PrivateReview = tempReview.PrivateReview;
                        if (tempReview.User != null)
                            rv.GuestName = tempReview.User.FirstName;
                        else
                            rv.GuestName = "Guest"; ;

                        model.Reviews.Add(rv);

                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage(ex.Message);
            }
            return View(model);
        }


        private List<object> GetReviews(int eventId, int supperClubId)
        {
            var listReviews = new List<object>();
            var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();
            IList<Review> lstReviews = _supperClubRepository.GetSupperClubReviews(eventId, supperClubId);
            for (int i = 0; i < lstReviews.Count; i++)
            {
                var tempReview = lstReviews[i];
                string guestName = "Guest";
                if (tempReview.User != null)
                    guestName = tempReview.User.FirstName;
                else if (!string.IsNullOrEmpty(tempReview.GuestName))
                    guestName = tempReview.GuestName;
                else if (!string.IsNullOrEmpty(tempReview.Email))
                    guestName = _supperClubRepository.GetGuestName(tempReview.Email);
                try
                {
                    listReviews.Add(new
                    {
                        eventId = tempReview.EventId,
                        reviewId = tempReview.Id,
                        dateCreated = tempReview.DateCreated.ToString("ddd, d MMM yyyy"),
                        dateCreatedSEO = tempReview.DateCreated.ToString("yyyy-MM-ddThh:mm"),
                        rating = tempReview.Rating == null ? 0 : tempReview.Rating,
                        reviewText = tempReview.PublicReview == null ? "" : tempReview.PublicReview,
                        userName = guestName,
                        reviewTitle = tempReview.Title == null ? "" : tempReview.Title,
                        hostResponse = tempReview.HostResponse,
                        hostResponseDate = tempReview.HostResponseDate.ToString("ddd, d MMM yyyy"),
                        adminResponse = tempReview.AdminResponse,
                        adminResponseDate = tempReview.AdminResponseDate.ToString("ddd, d MMM yyyy"),
                        isHost = (tempReview.Event.SupperClub.UserId == UserMethods.UserId) ? true : false,
                        isAdmin = UserMethods.IsAdmin,
                        isReviewUser = (tempReview.UserId == UserMethods.UserId) ? true : false


                    });
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return listReviews;
        }
        //public string GetReviewHtml(int eventId, int start, int end)
        //{
        //    IList<Review> lr = _supperClubRepository.GetSupperClubReviews(eventId);

        //    string str = RenderPartialToString(this, "_Review", lr.ToList(), ViewData, TempData);
        //    return str;
        //}
        public static string RenderPartialToString(Controller controller, string partialViewName, List<Review> model, ViewDataDictionary viewData, TempDataDictionary tempData)
        {
            ViewEngineResult result = ViewEngines.Engines.FindPartialView(controller.ControllerContext, partialViewName);

            if (result.View != null)
            {
                string strOutput = "";
                foreach (Review r in model)
                {
                    controller.ViewData.Model = r;
                    StringBuilder sb = new StringBuilder();
                    using (StringWriter sw = new StringWriter(sb))
                    {
                        using (HtmlTextWriter output = new HtmlTextWriter(sw))
                        {
                            ViewContext viewContext = new ViewContext(controller.ControllerContext, result.View, viewData, tempData, output);
                            result.View.Render(viewContext, output);
                        }
                    }
                    strOutput += sb.ToString();
                }

                return strOutput;
            }

            return String.Empty;
        }
        #endregion

        #region Host Actions
        [Authorize(Roles = "Host")]
        public ActionResult AddImage()
        {
            return View();
        }
        [Authorize(Roles = "Host")]
        public ActionResult MyHostedEvents()
        {
            ViewBag.Title = "My Events - Hosting";
            ViewBag.MyEventType = "MyHostedEvents";
            Dictionary<string, IList<Event>> model = new Dictionary<string, IList<Event>>();
            model.Add("past", _supperClubRepository.GetPastEventsForASupperClub((int)UserMethods.SupperClubId));
            model.Add("future", _supperClubRepository.GetFutureEventsForASupperClub((int)UserMethods.SupperClubId));

            return View("MyEvents", model);
        }

        [Authorize(Roles = "Host")]
        public ActionResult CreateEvent(int? supperClubId = null)
        {
            LogMessage("Started Create Event");
            Event model = new Event();
            if (supperClubId != null)
                model.SupperClubId = (int)supperClubId;
            else
                model.SupperClubId = (int)UserMethods.SupperClubId;

            //Defaults for all new events
            DateTime now = DateTime.Now.AddDays(1);

            ViewBag.StandardDiets = _supperClubRepository.GetStandardDiets();
            ViewBag.AllergyDiets = _supperClubRepository.GetAllergyDiets();
            ViewBag.Cuisines = new SelectList(_supperClubRepository.GetCuisines(), "Id", "Name");
            ViewBag.Tags = _supperClubRepository.GetTagsWithoutCuisine();

            var sc = _supperClubRepository.GetSupperClubForUser((Guid)UserMethods.UserId);
            List<Event> lstEvents = _supperClubRepository.GetAllEventsForASupperClub(model.SupperClubId).ToList();
            if (lstEvents != null && lstEvents.Count > 1)
            {
                //Check for previous event and set defaults to match
                Event latestEvent = lstEvents.OrderByDescending(e => e.DateCreated).First();
                model.Start = new DateTime(now.Year, now.Month, now.Day, latestEvent.Start.Hour, latestEvent.Start.Minute, 0);
                model.StartTime = model.Start;
                model.End = model.Start.AddHours((latestEvent.End - latestEvent.Start).TotalHours);
                model.Duration = (latestEvent.End - latestEvent.Start).TotalHours;

                model.Description = latestEvent.Description;
                model.HouseRule = latestEvent.HouseRule;
                model.Guests = latestEvent.Guests;
                model.ReservedSeats = latestEvent.ReservedSeats;
                model.Cost = latestEvent.Cost;
                model.EventTagList = latestEvent.EventTagList;

                //model.Cuisines = latestEvent.Cuisines;
                model.Cuisines = string.Empty;
                model.Diets = latestEvent.Diets;
                model.Menu = latestEvent.Menu;
                //model.MenuOptions = latestEvent.MenuOptions;
                //model.Seatings = latestEvent.Seatings;
                model.MultiSeating = false;
                model.MultiMenuOption = false;

                model.Alcohol = latestEvent.Alcohol;
                model.Charity = latestEvent.Charity;
                model.Private = latestEvent.Private;
                model.WalkIn = false;
                model.ContactNumberRequired = false;
                model.CostTextDisplay = string.Format("{0}", latestEvent.Cost.ToString("F4"));
                model.HomeEvent = false;
                model.Status = (int)EventStatus.New;
                model.Commission = _commissionPercentage;

                // For Event Description
                string[] separator = new string[] { "|&|" };
                ViewBag.EventDescriptionList = latestEvent.Description.Split(separator, StringSplitOptions.RemoveEmptyEntries);
            }
            else
            {
                //Set intial defaults
                model.Start = new DateTime(now.Year, now.Month, now.Day, 19, 0, 0);
                model.StartTime = model.Start;
                model.End = model.Start.AddHours(2);
                model.Duration = 2;
                model.Private = false;
                model.MultiSeating = false;
                model.MultiMenuOption = false;
                model.WalkIn = false;
                model.ContactNumberRequired = false;

                model.Guests = 4;
                model.ReservedSeats = 0;
                model.Cost = 20;
                model.CostTextDisplay = "20.00";
                model.HomeEvent = false;
                model.Status = (int)EventStatus.New;
                model.Commission = _commissionPercentage;
            }

            return View("CreateEvent", model);
        }

        [Authorize(Roles = "Host")]
        [HttpPost, ValidateInput(false)]
        public ActionResult CreateEvent(Event model)
        {
            LogMessage("Begin Event Creation");

            //string newImagePath = null;
            //if (model.File != null)
            //{
            //    newImagePath = ImageScaler.ConvertImage(model.File);
            //    if (newImagePath != null)
            //        model.ImagePath = newImagePath; // only update the image if conversion was successful
            //    else
            //        ModelState.AddModelError("File", "There was a problem with your image file, please try another file");
            //}

            //GeoLocation geolocation = Utils.GetAddressCoordinates(model.Address + " " + model.Address2 + " " + model.PostCode + " " + model.City);
            // Changed geo to show address on map only based on zipcode - On Liv's request
            GeoLocation geolocation = SupperClub.Code.GeoMethods.GetGeoLocation(model.PostCode, model.City);
            if (geolocation == null)
            {
                ModelState.AddModelError("Address", "We couldn't find your address on a map!");
                LogMessage("Error fetching lat long.", LogLevel.ERROR);
            }

            if (model.EventTagList == null || model.EventTagList.Length == 0 || model.EventTagList == ",")
                ModelState.AddModelError("Tag", "The event has no tags, please set event tags!");
            model.Status = (int)EventStatus.New;

            if (ModelState.IsValid)
            {
                model.ContactNumberRequired = true;
                model.SupperClubId = (int)UserMethods.SupperClubId;
                model.Cost = Convert.ToDecimal(model.CostTextDisplay.Replace("£", "").Trim());
                //model.Description = model.EventDescription;
                //Combine Start and StartTime for db into start
                model.Start = new DateTime(model.Start.Year, model.Start.Month, model.Start.Day, model.StartTime.Hour, model.StartTime.Minute, 0);
                model.End = model.Start.AddHours(model.Duration);
                
                string[] images = null;
                if (model.ImagePaths.Length > 0 && (!model.ImagePaths.ToLower().Contains("error")))
                {
                    string[] images1 = model.ImagePaths.Trim().TrimEnd(',').Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    if (images1.Length > 0)
                        model.ImagePath = images1[images1.Length - 1];
                    images = images1;
                }
                else
                {
                    model.ImagePath = null;
                    LogMessage("Error uploading image. Error description:" + model.ImagePaths, LogLevel.ERROR);
                }
                model.ImagePath = string.IsNullOrEmpty(model.ImagePath) ? "defaultEventImage.png" : model.ImagePath;

                // Add London by default as city tag
                model.EventCityList = "1";
                if (geolocation != null)
                {
                    model.Longitude = geolocation.Longitude;
                    model.Latitude = geolocation.Latitude;
                }
                model.Cuisines = model.Cuisines.Trim().TrimEnd(',');

                var supperClub = _supperClubRepository.GetSupperClub(model.SupperClubId);
                model.UrlFriendlyName = SupperClub.Web.Helpers.Utils.GetSeoFriendlyUrl(supperClub.Name) + "-" + SupperClub.Web.Helpers.Utils.GetSeoFriendlyUrl(model.Name);
                model.Status = (int)EventStatus.New;
                LogMessage("Create event variable info logging. Address:" + model.Address + ",Address2:" + (string.IsNullOrEmpty(model.Address2) ? string.Empty : model.Address2) + ",City:" + model.City + ",Postcode:" + model.PostCode);

                var _event = _supperClubRepository.CreateEvent(model);

                if (_event.Id > 0)
                {
                    if (images != null && images.Length > 0)
                        MoveImage(images);

                    LogMessage("Created Event: " + _event.Id);
                    EmailService.EmailService es = new EmailService.EmailService(_supperClubRepository);
                    bool success = es.SendAdminNewEventCreatedEmail(UserMethods.CurrentUser.FirstName + " " + UserMethods.CurrentUser.LastName, UserMethods.CurrentUser.aspnet_Users.LoweredUserName, UserMethods.CurrentUser.ContactNumber, supperClub.Name, supperClub.UrlFriendlyName, _event.Name, _event.UrlFriendlyName, _event.Id);
                    if (!success)
                        LogMessage("Error sending event creation notification e-mail to admin", LogLevel.ERROR);

                    //Send email to Host for event approval request acknowledgement
                    bool sendStatus = es.SendHostNewEventCreatedEmail(supperClub.Id, UserMethods.CurrentUser.FirstName);

                    if (!sendStatus)
                        LogMessage("Error sending event approval request acknowledgement e-mail to host", LogLevel.ERROR);

                    TempData["supperClubURL"] = ServerMethods.ServerUrl + supperClub.UrlFriendlyName;

                    //// Push Notification to chef's followers
                    //PushNotificationService.MessageService ms = new PushNotificationService.MessageService(_supperClubRepository);
                    //bool pushNotificationStatus = ms.FavChefNewEventNotification(_event.Id, _event.SupperClubId, _event.Name, supperClub.Name, _event.Start);
                    //if (pushNotificationStatus)
                    //    LogMessage("CreateEvent: Sent notification to chef's followers", LogLevel.INFO);
                    //else
                    //    LogMessage("CreateEvent: Error sending notification to chef's followers", LogLevel.ERROR);

                    return RedirectToAction("ThankYou", "Event");
                }
                else
                {
                    LogMessage("Problems with Event Creation");
                    string errorMsg = "There were problems creating your event, please fix them and try again.";
                    SetNotification(NotificationType.Error, errorMsg);
                    ViewBag.Cuisines = new SelectList(_supperClubRepository.GetCuisines(), "Id", "Name");
                    ViewBag.StandardDiets = _supperClubRepository.GetStandardDiets();
                    ViewBag.AllergyDiets = _supperClubRepository.GetAllergyDiets();
                    ViewBag.Tags = _supperClubRepository.GetTagsWithoutCuisine();
                    return View(model);
                }
            }
            else
            {
                LogMessage("Problems with Event Creation");
                string errorMsg = "There were problems creating your event, please fix them and try again.";
                foreach (ModelState modelState in ViewData.ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        errorMsg = errorMsg + " " + error.ErrorMessage;
                    }
                }
                SetNotification(NotificationType.Error, errorMsg);

                ViewBag.Cuisines = new SelectList(_supperClubRepository.GetCuisines(), "Id", "Name");
                ViewBag.StandardDiets = _supperClubRepository.GetStandardDiets();
                ViewBag.AllergyDiets = _supperClubRepository.GetAllergyDiets();
                ViewBag.Tags = _supperClubRepository.GetTagsWithoutCuisine();

                return View(model);
            }
        }

        [Authorize(Roles = "Host")]
        public ActionResult CloneEvent(int? eventId = null)
        {
            LogMessage("Started Cloning the event");
            EventCloneModel model = new EventCloneModel();
            if (eventId != null)
            {
                model.EventId = (int)eventId;
                LogMessage("The event being cloned. EventId=" + model.EventId.ToString());
                //Defaults for all new events
                DateTime now = DateTime.Now.AddDays(1);

                Event latestEvent = _supperClubRepository.GetEvent((int)eventId);
                if (latestEvent != null)
                {
                    //Check for previous event and set defaults to match
                    model.EventDate = new DateTime(now.Year, now.Month, now.Day, latestEvent.Start.Hour, latestEvent.Start.Minute, 0);
                    model.StartTime = latestEvent.Start;
                    model.Duration = (latestEvent.End - latestEvent.Start).TotalHours;
                    if (latestEvent.MultiSeating)
                    {
                        model.MultiSeating = true;
                        model.Duration = 2;
                    }
                    else
                        model.MultiSeating = false;
                }
            }
            else
            {
                ViewBag.NoEventIdFound = true;
            }         
            return View("CloneEvent", model);
        }
        [Authorize(Roles = "Host")]
        [HttpPost]
        public ActionResult CloneEvent(EventCloneModel model)
        {
            LogMessage("Begin Clone Event: " + model.EventId.ToString());
            if (ModelState.IsValid)
            {
                model.StartTime = new DateTime(model.EventDate.Year, model.EventDate.Month, model.EventDate.Day, model.StartTime.Hour, model.StartTime.Minute, 0);                    
                DateTime EndTime = model.StartTime.AddHours(model.Duration);                
                int _eventId = _supperClubRepository.CloneEvent(model.EventId, new DateTime(model.EventDate.Year, model.EventDate.Month, model.EventDate.Day, model.StartTime.Hour, model.StartTime.Minute, 0), EndTime, _commissionPercentage);

                if (_eventId > 0)
                {
                    LogMessage("Cloned Event: " + _eventId.ToString());
                    Event newEvent = _supperClubRepository.GetEvent(_eventId);
                    EmailService.EmailService es = new EmailService.EmailService(_supperClubRepository);
                    bool success = es.SendAdminNewEventCreatedEmail(newEvent.SupperClub.User.FirstName + " " + newEvent.SupperClub.User.LastName, newEvent.SupperClub.User.Email, newEvent.SupperClub.User.ContactNumber, newEvent.SupperClub.Name, newEvent.SupperClub.UrlFriendlyName, newEvent.Name, newEvent.UrlFriendlyName, newEvent.Id);
                    if (!success)
                        LogMessage("Error sending event creation notification e-mail to admin", LogLevel.ERROR);

                    //Send email to Host for event approval request acknowledgement
                    bool sendStatus = es.SendHostNewEventCreatedEmail(newEvent.SupperClub.Id, newEvent.SupperClub.User.FirstName);
                    if (!sendStatus)
                        LogMessage("Error sending event approval request acknowledgement e-mail to host", LogLevel.ERROR);

                    ViewBag.Status = true;
                    ViewBag.EventUrl = ServerMethods.ServerUrl + newEvent.SupperClub.UrlFriendlyName + "/" + newEvent.UrlFriendlyName + "/" + newEvent.Id.ToString();
                    return View(model);
                }
                else
                {
                    LogMessage("Problems while cloning event");
                    string errorMsg = "There were problems copying your event, please fix them and try again.";
                    SetNotification(NotificationType.Error, errorMsg);
                    ViewBag.Status = false;
                    return View(model);
                }
            }
            else
            {
                LogMessage("Problems while cloning event");
                string errorMsg = "There were problems copying your event, please fix them and try again.";
                foreach (ModelState modelState in ViewData.ModelState.Values)
                {
                    foreach (ModelError error in modelState.Errors)
                    {
                        errorMsg = errorMsg + " " + error.ErrorMessage;
                    }
                }
                SetNotification(NotificationType.Error, errorMsg);
                ViewBag.Status = false;
                return View(model);
            }
        }
        [Authorize(Roles = "Host")]
        public ActionResult UploadImages(IEnumerable<HttpPostedFileBase> files)
        {
            var result = new List<string>();
            if (files != null)
            {
                foreach (HttpPostedFileBase file in files)
                {
                    try
                    {
                        //.. standard file upload code
                        String img = ImageScaler.ConvertImage(file);
                        if (img != null)
                            result.Add(img);
                        else
                            LogMessage("Problem Uploading Images for Event.", LogLevel.ERROR);
                    }
                    catch (Exception ex)
                    {
                        LogMessage("Problem Uploading Images for Event. Error Details: " + ex.StackTrace, LogLevel.ERROR);
                    }
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [Authorize]
        [HttpPost]
        public JsonResult SaveUploadedFile()
        {
            bool isSavedSuccessfully = true;
            string fName = "";
            try
            {
                foreach (string fileName in Request.Files)
                {
                    HttpPostedFileBase file = Request.Files[fileName];
                    //Save file content goes here
                    if (file != null && file.ContentLength > 0)
                    {

                        string img = ImageScaler.ConvertImage(file);
                        if (string.IsNullOrEmpty(img))
                        {
                            LogMessage("Problem Uploading Images for Event.", LogLevel.ERROR);
                            isSavedSuccessfully = false;
                        }
                        else
                        {
                            fName += (fName.Length > 0 ? "," : "") + img;
                        }                            
                    }
                }

            }
            catch (Exception ex)
            {
                LogMessage("Problem Uploading Images for Event. Error Details: " + ex.Message + ex.StackTrace, LogLevel.ERROR);
                isSavedSuccessfully = false;
            }


            if (isSavedSuccessfully)
            {
                return Json(new { Message = fName }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { Message = "Error in saving file" }, JsonRequestBehavior.AllowGet);
            }
        }

        [Authorize]
        [HttpPost]
        public void saveUpdatedImage()
        {
            string localFileDest = string.Empty;
            try
            {
                string ImageId = Request["imageId"]; // an encoded reference to the file that you're updating perhaps
                string url = Request["url"]; // the url of the saved file
                if (!string.IsNullOrEmpty(ImageId))
                {
                    string[] stringSeparators = new string[] { "_" };
                    string[] imagePath = ImageId.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                    if (imagePath.Length > 1)
                    {
                        string folderPathIdentifier = imagePath[0];
                        string fileName = imagePath[1];
                        string folderPath = string.Empty;
                        switch (folderPathIdentifier)
                        {
                            case "t":
                                folderPath = Server.MapPath(WebConfigurationManager.AppSettings["TempImagePath"]);
                                break;
                            case "e":
                                folderPath = Server.MapPath(WebConfigurationManager.AppSettings["EventImagePath"]);
                                break;
                            case "s":
                                folderPath = Server.MapPath(WebConfigurationManager.AppSettings["SupperClubImagePath"]);
                                break;
                        }
                        if (!string.IsNullOrEmpty(folderPath))
                        {
                            localFileDest = folderPath + fileName;
                            WebClient client = new WebClient();
                            client.DownloadFile(url, localFileDest);
                        }
                    }
                    else
                        LogMessage("Problem updating image. Error Details: Invalid ImgId passed back from aviary. ImagId = " + ImageId, LogLevel.ERROR);
                }
                else
                    LogMessage("Problem updating image. Error Details: ImgId not passed back from aviary. ImagId = " + ImageId, LogLevel.ERROR);
            }
            catch (Exception ex)
            {
                LogMessage("exception occued updating image. Error Details: " + ex.Message + ex.StackTrace, LogLevel.ERROR);
            }
        }

        [Authorize]
        [HttpPost]
        public JsonResult saveUpdatedImageEdit(string url, string imageId)
        {
            string localFileDest = string.Empty;
            try
            {
                string oldImageId = imageId;
                string newImageId = string.Empty;
                if (!string.IsNullOrEmpty(oldImageId))
                {
                    string[] stringSeparators = new string[] { "_" };
                    string[] imagePath = oldImageId.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                    if (imagePath.Length > 1)
                    {
                        string folderPathIdentifier = imagePath[0];
                        string oldFileName = imagePath[1];
                        string[] extensionSeparators = new string[] { "." };
                        string[] extension = oldImageId.Split(extensionSeparators, StringSplitOptions.RemoveEmptyEntries);
                        string fileExtension = extension[extension.Length-1];
                        string newFileName = Guid.NewGuid().ToString() + "." + fileExtension;
                        string folderPath = string.Empty;
                        switch (folderPathIdentifier)
                        {
                            case "t":
                                folderPath = Server.MapPath(WebConfigurationManager.AppSettings["TempImagePath"]);
                                break;
                            case "e":
                                folderPath = Server.MapPath(WebConfigurationManager.AppSettings["EventImagePath"]);
                                break;
                            case "s":
                                folderPath = Server.MapPath(WebConfigurationManager.AppSettings["SupperClubImagePath"]);
                                break;
                        }
                        if (!string.IsNullOrEmpty(folderPath))
                        {
                            localFileDest = folderPath + newFileName;
                            WebClient client = new WebClient();
                            client.DownloadFile(url, localFileDest);
                            newImageId = newFileName;
                            oldImageId = oldFileName;
                            //delete the old image from folder and also update the image in database
                            bool success = _supperClubRepository.UpdateImageUrl(folderPathIdentifier == "s" ? ImageType.SupperClub : ImageType.Event, oldImageId, newFileName);
                            if (success)
                            {
                                if (System.IO.File.Exists(folderPath + oldFileName))
                                {
                                    System.IO.File.Delete(folderPath + oldFileName);
                                }
                            }                                
                        }
                        var data = new { oldImageId = oldImageId, newImageId = newImageId, folderPathIdentifier = folderPathIdentifier };
                        return Json(data, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        LogMessage("Problem updating image. Error Details: Invalid ImgId passed back from aviary. ImagId = " + oldImageId, LogLevel.ERROR);
                        var data = new { error = "Problem updating image. Error Details: Invalid ImgId passed back"};
                        return Json(data, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                { 
                    LogMessage("Problem updating image. Error Details: ImgId not passed back from aviary. ImagId = " + oldImageId, LogLevel.ERROR);
                    var data = new { error = "Problem updating image. Error Details: ImgId not passed back from aviary." };
                    return Json(data, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage("exception occured updating image. Error Details: " + ex.Message + ex.StackTrace, LogLevel.ERROR);
                var data = new { error = "Problem updating image. An exception occued updating image." };
                return Json(data, JsonRequestBehavior.AllowGet);                
            }
        }
        public ActionResult UploadifyImages()
        {
            var result = new List<string>();
            if (Request.Files != null)
            {
                foreach (string file in Request.Files)
                {
                    try
                    {
                        HttpPostedFileBase postedFile = (HttpPostedFileBase)Request.Files[file];
                        //.. standard file upload code
                        String img = ImageScaler.ConvertImage(postedFile);
                        if (img != null)
                            result.Add(img);
                        else
                            LogMessage("Problem Uploading Images for Event.", LogLevel.ERROR);
                    }
                    catch (Exception ex)
                    {
                        LogMessage("Problem Uploading Images for Event. Error Details: " + ex.StackTrace, LogLevel.ERROR);
                    }
                }
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [Authorize(Roles = "Host")]
        public ActionResult ThankYou()
        {
            var data = TempData["supperClubURL"];
            ViewBag.SupperClubURL = data;
            return View();
        }

        [Authorize(Roles = "Host")]
        [HttpPost, ValidateInput(false)]
        public ActionResult UpdateDetails(Event model)
        {
            LogMessage("Begin Update Event Details: " + model.Id.ToString());
            Event e = _supperClubRepository.GetEvent(model.Id);

            if (model.CostTextDisplay != null)
                model.Cost = Convert.ToDecimal(model.CostTextDisplay.Replace("£", "").Trim());

            bool isPriceChanged = false;
            EventPriceChangeLog epcl = new EventPriceChangeLog();

            if (!e.MultiMenuOption && e.Cost != model.Cost)
            {
                isPriceChanged = true;
                epcl.OldPrice = e.Cost;
                epcl.NewPrice = model.Cost;
                epcl.UserId = UserMethods.CurrentUser.Id;
                epcl.EventId = e.Id;
                epcl.MenuOptionId = 0;
            }

            bool isCommissionChanged = false;
            EventCommissionChangeLog eccl = new EventCommissionChangeLog();

            if (e.Commission != model.Commission)
            {
                isCommissionChanged = true;
                eccl.OldCommission = e.Commission;
                eccl.NewCommission = model.Commission;
                eccl.UserId = UserMethods.CurrentUser.Id;
                eccl.EventId = e.Id;
                eccl.OldCost = e.Cost;
                eccl.NewCost = model.Cost;
            }

            //Create a list of images to be deleted
            string[] stringSeparators = new string[] { "," };
            if (string.IsNullOrEmpty(model.ImagePaths.Trim()))
                model.ImagePaths = e.ImagePaths;
            string[] imagePaths = model.ImagePaths.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
            List<string> imagesToBeDeletedEventList = (from x in e.EventImages
                                        where (!imagePaths.Any(val => x.ImagePath.Contains(val)))
                                        select x.ImagePath).ToList<string>();
            //Check if these images are being used in any other event
            var imagesToBeDeleted = _supperClubRepository.GetUnusedImageList(model.Id, imagesToBeDeletedEventList);
            //string newImagePath = null;
            //if (model.File != null)
            //{
            //    newImagePath = ImageScaler.ConvertImage(model.File);
            //    if (newImagePath != null)
            //        model.ImagePath = newImagePath; // only update the image if conversion was successful
            //}
            string[] images = null;
            if (!string.IsNullOrEmpty(model.ImagePaths))
            {
                images = model.ImagePaths.Trim().TrimEnd(',').Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                model.ImagePath = images[images.Length - 1];
            }

            bool addressNotUpdated = false;
            if (e.PostCode != model.PostCode || e.City != model.City)
            {
                // GeoLocation geolocation = Utils.GetAddressCoordinates(model.Address + " " + model.Address2 + " " + model.PostCode + " " + model.City);
                // Changed geo to show address on map only based on zipcode - On Liv's request
                GeoLocation geolocation = SupperClub.Code.GeoMethods.GetGeoLocation(model.PostCode, model.City);
                if (geolocation != null)
                {
                    model.Longitude = geolocation.Longitude;
                    model.Latitude = geolocation.Latitude;                    
                }
                else
                    addressNotUpdated = true;
            }
            else
            {
                model.Longitude = e.Longitude;
                model.Latitude = e.Latitude;
            }
            if (string.IsNullOrEmpty(model.EventTagList))
                ModelState.AddModelError("Tag", "The event has no tags, please set event tags!");

            if (model.Cuisines != null)
                model.Cuisines = model.Cuisines.Trim().TrimEnd(',');

            model.Start = new DateTime(model.Start.Year, model.Start.Month, model.Start.Day, model.StartTime.Hour, model.StartTime.Minute, 0);
            model.End = model.Start.AddHours(model.Duration);
            if (UserMethods.IsAdmin)
            {
                model.Name = model.Name;
                var supperClub = _supperClubRepository.GetSupperClub(model.SupperClubId);
                model.UrlFriendlyName = SupperClub.Web.Helpers.Utils.GetSeoFriendlyUrl(supperClub.Name) + "-" + SupperClub.Web.Helpers.Utils.GetSeoFriendlyUrl(model.Name);
            }

            bool reservedSeatsNotUpdated = false;
            // Do a check against the any tickets in progress that need to count against reserved tickets
            int numberTicketsInProgress = _supperClubRepository.GetNumberTicketsInProgressForEvent(model.Id);

            if (model.ReservedSeats > model.Guests || (model.ReservedSeats > model.Guests - model.TotalNumberOfAttendeeGuests - numberTicketsInProgress))
            {
                reservedSeatsNotUpdated = true;
                model.ReservedSeats = e.ReservedSeats;
            }

            bool reservedSeatsChanged = false;
            if ((model.TotalNumberOfAvailableSeats - numberTicketsInProgress) < 0) // compare new with old || More Reserved Seats than Total Seats
            {
                reservedSeatsChanged = true;
                model.ReservedSeats = model.Guests - model.TotalNumberOfAttendeeGuests - numberTicketsInProgress; // The minimum avialable seats to reserve
            }

            bool success = _supperClubRepository.UpdateEvent(model, UserMethods.CurrentUser.Id);            

            if (success)
            {
                LogMessage("Updated Event Details: " + model.Id.ToString());
                string notificationMessage = "Your Event was updated successfully.";
                if (images != null && images.Length > 0)
                    MoveImage(images);

                //Physically delete files from server                
                if (imagesToBeDeleted != null)
                {
                    try
                    {
                        foreach (string t in imagesToBeDeleted)
                        {
                            string imagePathName = Server.MapPath(WebConfigurationManager.AppSettings["EventImagePath"]) + t;
                            if (System.IO.File.Exists(imagePathName))
                            {
                                System.IO.File.Delete(imagePathName);
                            }
                            else
                            {
                                string imagePathNameTemp = Server.MapPath(WebConfigurationManager.AppSettings["TempImagePath"]) + t;
                                if (System.IO.File.Exists(imagePathNameTemp))
                                {
                                    System.IO.File.Delete(imagePathNameTemp);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogMessage("Error deleting file from server. Error details:" + ex.Message + ex.StackTrace);
                    }
                }
                if (reservedSeatsChanged || reservedSeatsNotUpdated ||
                    //(model.File != null && newImagePath == null) || 
                    addressNotUpdated)
                {
                    notificationMessage = "We updated your event details, but there was a problem.";
                    if (reservedSeatsChanged)
                        notificationMessage = notificationMessage + " The number of Reserved Seats had to be changed because of an in progress purchase.";
                    if (reservedSeatsNotUpdated)
                        notificationMessage = notificationMessage + " The number of Reserved Seats were not updated as it was more than total seats available.";

                    //if (model.File != null && newImagePath == null) // there was a file, but it didn't convert successfully
                    //    notificationMessage = notificationMessage + " There was a problem with your image file, please try another file.";
                    //else 
                    if (addressNotUpdated)
                        notificationMessage = notificationMessage + " The address of the venue could not be updated as we could not find the address on the map.";

                    SetNotification(Models.NotificationType.Warning, notificationMessage, true);
                }
                else
                    SetNotification(Models.NotificationType.Success, notificationMessage, true);

                if (isPriceChanged)
                {
                    LogMessage("Logging Event Price Change. EventId=" + model.Id.ToString() + " UserId=" + UserMethods.CurrentUser.Id.ToString());
                    //update
                    bool result = _supperClubRepository.LogEventPriceChange(epcl);
                    if (result)
                        LogMessage("Logged Event Price Change. EventId=" + model.Id.ToString() + " UserId=" + UserMethods.CurrentUser.Id.ToString());
                    else
                        LogMessage("Logging Failed for Event Price Change. EventId=" + model.Id.ToString() + " UserId=" + UserMethods.CurrentUser.Id.ToString(), LogLevel.ERROR);
                }

                if (isCommissionChanged)
                {
                    LogMessage("Logging Event Price Change. EventId=" + model.Id.ToString() + " UserId=" + UserMethods.CurrentUser.Id.ToString());
                    //update
                    bool result = _supperClubRepository.LogEventCommissionChange(eccl);
                    if (result)
                        LogMessage("Logged Event Price Change. EventId=" + model.Id.ToString() + " UserId=" + UserMethods.CurrentUser.Id.ToString());
                    else
                        LogMessage("Logging Failed for Event Price Change. EventId=" + model.Id.ToString() + " UserId=" + UserMethods.CurrentUser.Id.ToString(), LogLevel.ERROR);
                }

            }
            else
            {
                LogMessage("Problem Updating Event Details: " + model.Id.ToString(), LogLevel.ERROR);
                SetNotification(Models.NotificationType.Error, "There was a problem updating your event. Please try again.", true);
            }

            return RedirectToAction("Details", "Event", new { eventId = model.Id });
        }

        [Authorize(Roles = "Host")]
        public ActionResult CancelEvent(int eventId, string message)
        {
            LogMessage("Begin Event Cancel: " + eventId.ToString());
            Event _event = _supperClubRepository.GetEvent(eventId);
            if (_event.SupperClub.UserId == UserMethods.UserId || UserMethods.IsAdmin) // Only the host or an Admin can cancel the event
            {
                bool updateSuccess = _supperClubRepository.CancelEvent(eventId);
                if (updateSuccess)
                {
                    LogMessage("Event Cancelled: " + eventId.ToString());
                    EmailService.EmailService es = new EmailService.EmailService(_supperClubRepository);
                    List<string> addresses = new List<string>();
                    foreach (EventAttendee attendee in _event.EventAttendees)
                    {
                        MembershipUser muser = Membership.GetUser(attendee.UserId);
                        addresses.Add(muser.Email);
                    }
                    bool success = es.SendEventCancelledEmails(addresses, _event.SupperClub.Name, _event.Name, _event.Start.ToString("ddd, d MMM yyyy") + " : " + _event.Start.ToShortTimeString(), message);
                    SetNotification(NotificationType.Success, "Your event has been cancelled", true);
                }
                else
                    SetNotification(NotificationType.Error, "There was a problem cancelling your event. Please try again.", true);
            }
            return RedirectToAction("DetailsByIdWithName", "Event", new { eventId = eventId, eventSeoFriendlyName = _event.UrlFriendlyName, hostname = _event.SupperClub.UrlFriendlyName });
        }

        [Authorize(Roles = "Host")]
        public JsonResult RejectAttendee(int eventId, Guid eventAttendeeId, int seatingId, int menuOptionId, bool notiFyGuest = false, int numberOfGuests = 0, decimal totalPrice = 0)
        {
            Event _event = _supperClubRepository.GetEvent(eventId);
            DateTime seatingTime = new DateTime();
            if (seatingId > 0)
            {
                seatingTime = (from s in _event.EventSeatings
                               where s.Id == seatingId
                               select s.Start).First();
            }

            bool removeSuccess = _supperClubRepository.RemoveUserFromAnEvent(eventAttendeeId, eventId, seatingId, menuOptionId, UserMethods.CurrentUser.Id);
            if (removeSuccess)
            {
                EmailService.EmailService es = new EmailService.EmailService(_supperClubRepository);

                if (notiFyGuest)
                {
                    SupperClub.Domain.User user = _supperClubRepository.GetUser(eventAttendeeId);
                    es.SendGuestRejectionEmail(user.FirstName, user.Email, _event.SupperClub.Name, _event.Name, _event.Start.ToString("ddd, d MMM yyyy") + " : " + ((seatingId == 0 || seatingTime == null) ? _event.Start.ToShortTimeString() : seatingTime.ToShortTimeString()));
                }
                else
                {
                    SupperClub.Domain.User user = _supperClubRepository.GetUser(eventAttendeeId);
                    es.SendAdminBookingCancellationNotificationEmail(user, _event, menuOptionId, UserMethods.CurrentUser, seatingId, seatingTime, numberOfGuests, totalPrice);
                }
            }

            return Json(removeSuccess, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Host")]
        public JsonResult RankAttendee(int eventId, Guid eventAttendeeId, int ranking)
        {
            bool rankSuccess = _supperClubRepository.RankUser(eventId, eventAttendeeId, ranking);
            return Json(rankSuccess, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "Admin")]
        public ActionResult SendBookingEmail(int eventId, Guid userId, int seatingId, int numberOfGuests)
        {
            SendBookingEmailModel model = new SendBookingEmailModel(eventId, userId, seatingId, numberOfGuests);
            return View(model);
        }
        [Authorize(Roles = "Admin")]
        public JsonResult AddNewTag(string tagName, string tagSeoName)
        {
            int tagId = 0;
            tagSeoName = SupperClub.Web.Helpers.Utils.GetSeoFriendlyUrl(tagSeoName);
            tagId = _supperClubRepository.AddTag(tagName, tagSeoName);
            return Json(tagId, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult SendBookingEmail(SendBookingEmailModel model)
        {
            if (model.EmailAddress.Trim() == string.Empty)
            {
                SetNotification(Models.NotificationType.Error, "Please enter e-mail address.", true);
                return RedirectToAction("SendBookingEmail");
            }
            EmailService.EmailService es = new EmailService.EmailService(_supperClubRepository);
            bool success = es.SendGuestInviteeEmail(model.EventId, model.UserId, model.SeatingId, model.EmailAddress, model.Comments, model.NumberOfTickets, 0, "XXXX");
            model.ShowStatus = true;
            if (success)
                model.SendStatus = "Email Sent Successfully.";
            else
                model.SendStatus = "Failure Sending Email!";

            return View(model);
        }
        [Authorize(Roles = "Host")]
        public JsonResult GetSupperClubDetails()
        {
            int supperClubId = (int)UserMethods.SupperClubId;
            SupperClub.Domain.SupperClub supperClub = _supperClubRepository.GetSupperClub(supperClubId);
            var supClub = new { Address = supperClub.Address, Address2 = supperClub.Address2, City = supperClub.City, PostCode = supperClub.PostCode };
            return Json(supClub, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Host")]
        public ActionResult GuestList(int eventId)
        {
            Event model = _supperClubRepository.GetEvent(eventId);
            if (UserMethods.SupperClubId != model.SupperClubId && !UserMethods.IsAdmin)
                return RedirectToAction("Index", "Home");

            return View(model);
        }

        [Authorize(Roles = "Host")]
        public ActionResult CheckSeatingOverlap(DateTime startDate, DateTime endDate, int eventId)
        {
            CheckSeatingOverlap checkSeatingOverlap = new CheckSeatingOverlap();
            Event _event = _supperClubRepository.GetEvent(eventId);
            if (_event != null && _event.MultiSeating)
            {
                foreach (EventSeating es in _event.EventSeatings)
                {
                    if ((es.Start <= startDate && es.End >= startDate) || (es.Start <= endDate && es.End >= endDate))
                    {
                        checkSeatingOverlap.Exists = true;
                        break;
                    }
                }
            }
            return Json(checkSeatingOverlap, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Host")]
        public JsonResult GetCostToGuest(decimal baseCost, decimal commission)
        {
            GetCostToGuest getCostToGuest = new GetCostToGuest();
            getCostToGuest.DisplayCost = SupperClub.Domain.CostCalculator.CostToGuest(baseCost, commission);
            return Json(getCostToGuest, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "Host")]
        public ActionResult DownloadGuestList(int eventId)
        {
            Event model = _supperClubRepository.GetEvent(eventId);
            if (UserMethods.SupperClubId != model.SupperClubId && !UserMethods.IsAdmin)
                return RedirectToAction("Index", "Home");
            //Get the report and generate it returning the server filename
            Report report = _supperClubRepository.GetReport(18);
            List<Tuple<string, string>> sqlParameters = new List<Tuple<string, string>>();
            sqlParameters.Add(new Tuple<string, string>("@EventId", eventId.ToString()));

            string ReportFolderPath = System.Web.HttpContext.Current.Server.MapPath(WebConfigurationManager.AppSettings["ReportFolderPath"]);

            ReportGenerator reportGenerator = new ReportGenerator();
            string fileStream = reportGenerator.GenerateReport(report, ReportFolderPath, sqlParameters);
            if (fileStream == null)
            {
                SetNotification(NotificationType.Error, "There was a problem downloading the guest list. Check the System Log for details. Event Id: " + eventId.ToString(), true);
            }

            //Set the name of the file to be generated
            DateTime now = DateTime.Now;
            string fileDownloadName = now.Year.ToString() + "." + now.Month.ToString() + "." + now.Day.ToString() + "-" + now.Hour.ToString() + "." + now.Minute.ToString();
            fileDownloadName = model.Name.Replace(" ", "_") + "_GuestList_" + fileDownloadName + ".csv";
            string mimeType = "csv";

            return File(fileStream, mimeType, fileDownloadName);
        }

        [Authorize(Roles = "Host")]
        public ActionResult WaitList(int eventId)
        {
            Event e = _supperClubRepository.GetEvent(eventId);
            if (UserMethods.SupperClubId != e.SupperClubId && !UserMethods.IsAdmin)
                return RedirectToAction("Index", "Home");
            IList<EventWaitList> model = _supperClubRepository.GetEventWaitList(eventId);
            return View(model);
        }
        [Authorize(Roles = "Host")]
        public ActionResult DownloadWaitList(int eventId)
        {
            Event model = _supperClubRepository.GetEvent(eventId);
            if (UserMethods.SupperClubId != model.SupperClubId && !UserMethods.IsAdmin)
                return RedirectToAction("Index", "Home");
            //Get the report and generate it returning the server filename
            Report report = _supperClubRepository.GetReport(36);
            List<Tuple<string, string>> sqlParameters = new List<Tuple<string, string>>();
            sqlParameters.Add(new Tuple<string, string>("@EventId", eventId.ToString()));

            string ReportFolderPath = System.Web.HttpContext.Current.Server.MapPath(WebConfigurationManager.AppSettings["ReportFolderPath"]);

            ReportGenerator reportGenerator = new ReportGenerator();
            string fileStream = reportGenerator.GenerateReport(report, ReportFolderPath, sqlParameters);
            if (fileStream == null)
            {
                SetNotification(NotificationType.Error, "There was a problem downloading the guest list. Check the System Log for details. Event Id: " + eventId.ToString(), true);
            }

            //Set the name of the file to be generated
            DateTime now = DateTime.Now;
            string fileDownloadName = now.Year.ToString() + "." + now.Month.ToString() + "." + now.Day.ToString() + "-" + now.Hour.ToString() + "." + now.Minute.ToString();
            fileDownloadName = model.Name.Replace(" ", "_") + "_WaitingList_" + fileDownloadName + ".csv";
            string mimeType = "csv";

            return File(fileStream, mimeType, fileDownloadName);
        }

        #region Static Page
        [OutputCache(Duration = 60 * 60 * 24 * 365, Location = System.Web.UI.OutputCacheLocation.Any)]
        [Authorize(Roles = "Host")]
        public ActionResult MarketingTips()
        {
            return View();
        }
        #endregion
        #endregion

        #region Guest Actions


        [Authorize]
        public ActionResult MyEvents()
        {
            ViewBag.Title = "My Events - Attending";
            ViewBag.MyEventType = "MyEvents";
            Dictionary<string, IList<Event>> model = new Dictionary<string, IList<Event>>();
            model.Add("past", _supperClubRepository.GetPastEventsForAUser((Guid)UserMethods.UserId));
            model.Add("future", _supperClubRepository.GetFutureEventsForAUser((Guid)UserMethods.UserId));
            foreach (Event e in model["past"])
            {
                if (e.MultiSeating)
                {
                    EventSeating es = _supperClubRepository.GetEventSeatingForAUser(e.Id, (Guid)UserMethods.UserId);
                    if (es != null)
                    {
                        e.Start = es.Start;
                        e.End = es.End;
                    }
                }
            }
            foreach (Event e in model["future"])
            {
                if (e.MultiSeating)
                {
                    EventSeating es = _supperClubRepository.GetEventSeatingForAUser(e.Id, (Guid)UserMethods.UserId);
                    if (es != null)
                    {
                        e.Start = es.Start;
                        e.End = es.End;
                    }
                }
            }
            return View("MyEvents", model);
        }

        [Authorize]
        public ActionResult BookEvent(int eventId)
        {
            if (eventId == 777)
            {

                return RedirectToAction("GiftVoucher", "Home", new { utm_source = "gifthp", utm_medium = "gift", utm_campaign = "gift" });


            }
            else
            {
                if (!UserMethods.IsLoggedIn)
                    return RedirectToAction("LogOn", "Account", new { returnUrl = "" });

                LogMessage("Booking Event: " + eventId.ToString() + " UserAgent:" + Request.UserAgent + " Browser:" + Request.Browser.Browser + "  BrowserVersion:" + Request.Browser.Version);
                Event _event = _supperClubRepository.GetEvent(eventId);
                List<BookingMenuModel> lstBookingMenuModel = new List<BookingMenuModel>();
                int menuMinTicket = 0;
                if (_event.MultiMenuOption)
                {
                    var lstEmo = _event.EventMenuOptions.OrderByDescending(x => x.IsDefault);
                    foreach (EventMenuOption emo in lstEmo)
                    {
                        BookingMenuModel _bookingMenuModel = new BookingMenuModel();
                        _bookingMenuModel.menuOptionId = emo.Id;
                        _bookingMenuModel.baseTicketCost = emo.Cost;
                        _bookingMenuModel.menuTitle = emo.Title;
                        _bookingMenuModel.numberOfTickets = emo.IsDefault ? (_event.MinMaxBookingEnabled ? _event.MinTicketsAllowed : 1) : 0;
                        _bookingMenuModel.discount = 0;
                        //_bookingMenuModel.minMaxBookingEnabled = emo.MinMaxBookingEnabled;
                        //_bookingMenuModel.minTicketAllowed = emo.MinTicketsAllowed;
                        //_bookingMenuModel.maxTicketAllowed = emo.MaxTicketsAllowed;
                        //_bookingMenuModel.seatSelectionInMultipleOfMin = emo.SeatSelectionInMultipleOfMin;
                        //if (emo.IsDefault)
                        //    menuMinTicket = emo.MinTicketsAllowed;
                        lstBookingMenuModel.Add(_bookingMenuModel);
                    }
                }
                List<BookingSeatingModel> lstBookingSeatingModel = new List<BookingSeatingModel>();
                bool isDefaultSeatingSoldOut = false;
                if (_event.MultiSeating)
                {
                    foreach (EventSeating es in _event.EventSeatings)
                    {
                        if (es.AvailableSeats > 0 && es.AvailableSeats - _supperClubRepository.GetNumberTicketsInProgressForEvent(_event.Id, es.Id) > 0)
                        {
                            BookingSeatingModel _bookingSeatingModel = new BookingSeatingModel();
                            _bookingSeatingModel.seatingId = es.Id;
                            _bookingSeatingModel.start = es.Start;
                            //_bookingSeatingModel.minMaxBookingEnabled = es.MinMaxBookingEnabled;
                            //_bookingSeatingModel.minTicketAllowed = es.MinTicketsAllowed;
                            //_bookingSeatingModel.maxTicketAllowed = es.MaxTicketsAllowed;
                            //_bookingSeatingModel.seatSelectionInMultipleOfMin = es.SeatSelectionInMultipleOfMin;
                            lstBookingSeatingModel.Add(_bookingSeatingModel);
                        }
                        else
                        {
                            if (es.Id == _event.DefaultSeatingId)
                                isDefaultSeatingSoldOut = true;
                        }
                    }
                }
                int numTickets = 1;
                //if(_event.MultiSeating)
                //{
                //    EventSeating es = _supperClubRepository.GetEventSeating((isDefaultSeatingSoldOut && lstBookingSeatingModel.Count > 0) ? lstBookingSeatingModel[0].seatingId : _event.DefaultSeatingId);
                //    if(es.MinGuestCountRequired)
                //        numTickets = es.MinTicketsAllowed;              
                //}
                //else
                //{
                //    if(_event.MinMaxBookingEnabled)
                //        numTickets = _event.MinTicketsAllowed;
                //}

                if (_event.MinMaxBookingEnabled)
                    numTickets = _event.MinTicketsAllowed;

                BookingModel bm = new BookingModel
                {
                    eventId = eventId,
                    numberOfTickets = menuMinTicket > 0 ? menuMinTicket : numTickets,
                    currency = "GBP",
                    baseTicketCost = _event.Cost,
                    totalTicketCost = _event.CostToGuest,
                    totalDue = numTickets * _event.CostToGuest,
                    discount = 0,
                    voucherId = 0,
                    voucherDescription = "",
                    IsContactNumberRequired = _event.ContactNumberRequired,
                    seatingId = (isDefaultSeatingSoldOut ? (lstBookingSeatingModel.Count > 0 ? lstBookingSeatingModel[0].seatingId : 0) : _event.DefaultSeatingId),
                    bookingMenuModel = lstBookingMenuModel,
                    bookingSeatingModel = lstBookingSeatingModel,
                    contactNumber = SupperClub.Code.UserMethods.CurrentUser.ContactNumber,
                    updateContactNumber = false,
                    MinMaxBookingEnabled = _event.MinMaxBookingEnabled,
                    MinTicketsAllowed = _event.MinTicketsAllowed,
                    MaxTicketsAllowed = _event.MaxTicketsAllowed,
                    SeatSelectionInMultipleOfMin = _event.SeatSelectionInMultipleOfMin,
                    ToggleMenuSelection = _event.ToggleMenuSelection,
                    commission = _event.Commission

                };
                TicketingService basketService = new TicketingService(_supperClubRepository);
                // creating a basket
                if (!_event.MultiSeating || (_event.MultiSeating && bm.seatingId > 0))
                    basketService.CreateTicketBasket(this.HttpContext);
                if (_event.MultiSeating && bm.seatingId > 0)
                {
                    if (basketService.GetCurrentAvailableTicketsForEvent(bm.eventId, bm.seatingId) <= 0)
                    {
                        foreach (BookingSeatingModel bsm in bm.bookingSeatingModel)
                        {
                            if (basketService.GetCurrentAvailableTicketsForEvent(bm.eventId, bsm.seatingId) > 0)
                            {
                                bm.seatingId = bsm.seatingId;
                                break;
                            }
                        }
                    }
                }
                // TODO to add logic to handle min tickets > 1
                if ((_event.MultiSeating && bm.seatingId == 0) || !basketService.AddTicketsToBasket(bm, true))
                {
                    // Wasn't able to add a ticket (none available)
                    SetNotification(NotificationType.Error, "Sorry there are no tickets available!", true);
                    return RedirectToAction("DetailsByIdWithName", "Event", new { eventId = eventId, eventSeoFriendlyName = _event.UrlFriendlyName, hostname = _event.SupperClub.UrlFriendlyName });
                }
                return RedirectToAction("BookTickets", "Booking");
            }
        }

        // This has custom authorisation using the UserId passed in via link OR can be accessed by a logged in user
        public ActionResult AddReview(int? eventId = null, string authToken = null)
        {
            ViewBag.Title = "Add Review";
            Guid? userId = null;
            string email = string.Empty;

            if (authToken == null)
            {
                if (eventId == null)
                    return RedirectToAction("Index", "Home");
                //Only allow logged in Users to leave reviews 
                else if (!UserMethods.IsLoggedIn)
                    return RedirectToAction("Logon", "Account", new { returnUrl = "/event/addreview?eventid=" + eventId.ToString() });
                else
                    userId = UserMethods.UserId;
                //// Check user actually went to the event *Not implemented as requested by Sid*
                //Event _event = _supperClubRepository.GetEvent((int)eventId);
                //if (_event.NumberOfAttendeeGuests(UserMethods.UserId) == 0)
                //{
                //    SetNotification(NotificationType.Error, "Looks like you're trying to review an Event you didn't attend.", true);
                //    return RedirectToAction("MyEvents", "Event");
                //}

                // Any user can leave a review for any event (will be null if not logged in)
                //userId = UserMethods.UserId;
            }
            else
            {
                ReviewAuthorisationModel auth = new ReviewAuthorisationModel();
                bool valid = auth.Decrypt(authToken);
                if (!valid)
                {
                    LogLongMessage("Add Review Auth Token was not valid. Token: " + authToken, "Token: " + authToken, LogLevel.ERROR);
                    SetNotification(NotificationType.Error, "Your review authorisation key wasn't valid.", true, true); // hide main container
                    return View(); // notification has hidden main container
                }

                if (!auth.AllUsers) // Must be specific user token
                    userId = Guid.Parse(auth.UserId);
                else if (!auth.AllowGuests && !UserMethods.IsLoggedIn) // Guests not allowed and user not logged in
                    return RedirectToAction("Logon", "Account", new { returnUrl = "" });
                else if (UserMethods.IsLoggedIn) // If Guests are or are not allowed but the user is logged in let's use their id
                    userId = UserMethods.UserId;
                else // Guests allowed
                {
                    userId = null;
                    email = auth.Email;
                }

                eventId = auth.EventId;
            }

            if (userId != null) // Check this user hasn't already left a review for this event
            {
                Review r = _supperClubRepository.GetReview((int)eventId, (Guid)userId);
                if (r != null)
                    return RedirectToAction("EditReview", new { eventId = eventId, userId = (Guid)userId });
            }

            Review model = new Review();
            model.UserId = userId;
            model.EventId = (int)eventId;
            model.Publish = false;
            model.Email = email;
            model.Rating = 0;
            model.FoodRating = 0;
            model.AmbienceRating = 0;

            LogMessage("Started Adding Review for Event: " + eventId.ToString());
            model.Event = _supperClubRepository.GetEvent(model.EventId);
            if (model.Event != null)
            {
                model.Event.Description = model.Event.EventDescription;
                if (model.Event.Description.Length > 200)
                {
                    bool linkFound = false;
                    if (model.Event.Description.Substring(0, 200).IndexOf("<a") == -1 || (System.Text.RegularExpressions.Regex.Matches(model.Event.Description.Substring(0, 200), "<a").Count == System.Text.RegularExpressions.Regex.Matches(model.Event.Description.Substring(0, 200), "</a>").Count))
                    {
                        model.Event.Description = model.Event.Description.Substring(0, 200) + "...";
                    }
                    else
                    {
                        int indexer = System.Text.RegularExpressions.Regex.Matches(model.Event.Description.Substring(0, 200), "<a").Count;
                        int requiredStringLength = Utils.findStringNthIndex(model.Event.Description, "</a>", indexer);
                        model.Event.Description = model.Event.Description.Substring(0, requiredStringLength + 4) + "...";
                        linkFound = true;
                    }
                    if (model.Event.Description.Substring(0, 200).IndexOf("<strong>") == -1 || (System.Text.RegularExpressions.Regex.Matches(model.Event.Description.Substring(0, 200), "<strong>").Count == System.Text.RegularExpressions.Regex.Matches(model.Event.Description.Substring(0, 200), "</strong>").Count))
                    {
                        if (!linkFound)
                            model.Event.Description = model.Event.Description.Substring(0, 200) + "...";
                    }
                    else
                    {
                        if (!linkFound)
                            model.Event.Description = model.Event.Description.Substring(0, 200) + "</strong>...";
                        else
                            model.Event.Description = model.Event.Description + "</strong>...";
                    }
                }
            }
            else
            {
                LogMessage("AddReview: Invalid Event Id. EventId=" + eventId.ToString(), LogLevel.ERROR);
                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        // Do not Authorise this POST see custom auth above
        [HttpPost]
        public ActionResult AddReview(Review model)
        {
            if (model != null)
            {
                LogMessage("Begin Post Adding Review for Event: " + model.EventId.ToString());
                if (!string.IsNullOrEmpty(model.PublicReview)) // Don't publish if it's just a private review
                    model.Publish = true;
                if (UserMethods.CurrentUser != null)
                {
                    model.UserId = UserMethods.CurrentUser.Id;
                    model.Email = UserMethods.CurrentUser.Email;
                }
                model.DateCreated = DateTime.Now;
                bool success = _supperClubRepository.AddReview(model);
                if (success)
                {
                    LogMessage("Added Review for Event: " + model.EventId.ToString());
                    if (model.Publish)
                        SetNotification(NotificationType.Success, "Your review has been posted below", true);
                    else
                        SetNotification(NotificationType.Success, "Your review has been posted successfully", true);

                    Event _event = _supperClubRepository.GetEvent(model.EventId);
                    model.Event = _event;
                    EmailService.EmailService ems = new EmailService.EmailService(_supperClubRepository);
                    if (_event != null)
                    {
                        Segment.Analytics.Client.Track(_segmentMethod.GetSegmentUser(), "Add Review", new Segment.Model.Properties() {
                            { "OverallRating", model.Rating },
                            { "FoodRating", model.FoodRating },
                            { "Ambience", model.AmbienceRating},
                            { "Comments", model.PublicReview},
                            { "PrivateComments", model.PrivateReview},
                            { "EventName", _event.Name},
                            { "ProfileName", _event.SupperClub.Name},
                            { "EventDate", _event.Start.ToString()},
                            { "ReviewDate", DateTime.Now.ToString()}
                        });
                    }
                    if (model.Publish) // Only email host if review is published
                    {
                        bool hostmail = ems.SendHostNewReviewEmail(model.EventId, model.PublicReview,model.PrivateReview);

                        if (hostmail)
                        {
                            LogMessage("Review email sent to host " + model.Event.SupperClub.User.Email + " successfully, EventId:" + model.EventId.ToString());
                        }
                        else
                        {
                            LogMessage("Review email sent to host" + model.Event.SupperClub.User.Email + " failed, EventId:" + model.EventId.ToString());
                        }
                    }
                    bool adminmail = ems.SendAdminNewReviewEmail(model.EventId, model.Rating, model.PublicReview, model.PrivateReview);
                    if (adminmail)
                    {
                        LogMessage("Review email sent to admin successfully, EventId:" + model.EventId);
                    }
                    else
                    {
                        LogMessage("Review email sent to admin failed, EventId:" + model.EventId);
                    }
                    if (model.UserId == null && !string.IsNullOrEmpty(model.Email) )
                    {
                        if (string.IsNullOrEmpty(model.GuestName))
                            model.GuestName = "Guest";
                        try
                        {
                            bool update = _supperClubRepository.AddGuestName(model.Email, model.GuestName);
                            if (update)
                                LogMessage("Successfully added guest name for email:" + model.Email);
                            else
                                LogMessage("Could not add guest name for email:" + model.Email, LogLevel.ERROR);
                        }
                        catch(Exception ex)
                        {
                            LogMessage("Add Review: Error adding guest name. Details:" + ex.Message + ex.StackTrace, LogLevel.ERROR);
                        }
                    }
                }
                else
                {
                    LogMessage("Could not Add Review for Event: " + model.EventId.ToString());
                    SetNotification(NotificationType.Error, "Couldn't post your review. Please try again.", true);
                }
                return RedirectToAction("DetailsByIdWithName", "Event", new { eventId = model.EventId });
            }
            LogMessage("Could not load Add Review Page because the model is null.");
            return RedirectToAction("DetailsByIdWithName", "Event", new { eventId = model.EventId });
        }

        // This has custom authorisation using the UserId passed in via link OR can be accessed by a logged in user
        public ActionResult AddReview2(int? eventId = null, string authToken = null)
        {
            ViewBag.Title = "Add Review";
            Guid? userId = null;

            if (authToken == null)
            {
                if (eventId == null)
                    return RedirectToAction("Index", "Home");
                //Only allow logged in Users to leave reviews 
                else if (!UserMethods.IsLoggedIn)
                    return RedirectToAction("Logon", "Account", new { returnUrl = "" });

                //// Check user actually went to the event *Not implemented as requested by Sid*
                //Event _event = _supperClubRepository.GetEvent((int)eventId);
                //if (_event.NumberOfAttendeeGuests(UserMethods.UserId) == 0)
                //{
                //    SetNotification(NotificationType.Error, "Looks like you're trying to review an Event you didn't attend.", true);
                //    return RedirectToAction("MyEvents", "Event");
                //}

                // Any user can leave a review for any event (will be null if not logged in)
                //userId = UserMethods.UserId;
            }
            else
            {
                ReviewAuthorisationModel auth = new ReviewAuthorisationModel();
                bool valid = auth.Decrypt(authToken);
                if (!valid)
                {
                    LogLongMessage("Add Review Auth Token was not valid.", "Token: " + authToken, LogLevel.ERROR);
                    SetNotification(NotificationType.Error, "Your review authorisation key wasn't valid.", true, true); // hide main container
                    return View(); // notification has hidden main container
                }

                if (!auth.AllUsers) // Must be specific user token
                    userId = Guid.Parse(auth.UserId);
                else if (!auth.AllowGuests && !UserMethods.IsLoggedIn) // Guests not allowed and user not logged in
                    return RedirectToAction("Logon", "Account", new { returnUrl = "" });
                else if (UserMethods.IsLoggedIn) // If Guests are or are not allowed but the user is logged in let's use their id
                    userId = UserMethods.UserId;
                else // Guests allowed
                    userId = null;

                eventId = auth.EventId;
            }

            if (userId != null) // Check this user hasn't already left a review for this event
            {
                Review r = _supperClubRepository.GetReview((int)eventId, (Guid)userId);
                if (r != null)
                    return RedirectToAction("EditReview", new { eventId = eventId });
            }

            Review model = new Review();
            model.UserId = userId;
            model.EventId = (int)eventId;
            model.Publish = false;
            LogMessage("Started Adding Review for Event: " + eventId.ToString());
            model.Event = _supperClubRepository.GetEvent(model.EventId);
            return View(model);
        }

        // Do not Authorise this POST see custom auth above
        [HttpPost]
        public ActionResult AddReview2(Review model)
        {
            LogMessage("Begin Post Adding Review for Event: " + model.EventId.ToString());
            if (!string.IsNullOrEmpty(model.PublicReview)) // Don't publish if it's just a private review
                model.Publish = true;

            bool success = _supperClubRepository.AddReview(model);
            if (success)
            {
                LogMessage("Added Review for Event: " + model.EventId.ToString());
                SetNotification(NotificationType.Success, "Your review has been posted below", true);
                EmailService.EmailService ems = new EmailService.EmailService(_supperClubRepository);
                Event _event = _supperClubRepository.GetEvent(model.EventId);
                if (_event != null)
                {
                    Segment.Analytics.Client.Track(_segmentMethod.GetSegmentUser(), "Add Review", new Segment.Model.Properties() {
                        { "OverallRating", model.Rating },
                        { "FoodRating", model.FoodRating },
                        { "Ambience", model.AmbienceRating},
                        { "Comments", model.PublicReview},
                        { "PrivateComments", model.PrivateReview},
                        { "EventName", _event.Name},
                        { "ProfileName", _event.SupperClub.Name},
                        { "EventDate", _event.Start.ToString()},
                        { "ReviewDate", DateTime.Now.ToString()}
                    });
                }
                if (model.Publish) // Only email host if review is published
                    ems.SendHostNewReviewEmail(model.EventId, model.PublicReview,model.PrivateReview);

                ems.SendAdminNewReviewEmail(model.EventId, model.Rating, model.PublicReview, model.PrivateReview);
            }
            else
            {
                LogMessage("Could not Add Review for Event: " + model.EventId.ToString());
                SetNotification(NotificationType.Error, "Couldn't post your review. Please try again.", true);
            }
            return RedirectToAction("Details", "Event", new { eventId = model.EventId });
        }

        [Authorize]
        public ActionResult EditReview(int eventId, Nullable<Guid> userId = null)
        {
            LogMessage("Started Editing Review for Event: " + eventId.ToString());
            ViewBag.Title = "Edit Review";
            Review model = _supperClubRepository.GetReview(eventId, userId == null ? (Guid)UserMethods.UserId : (Guid)userId);
            if (model == null) // There is no review for this user for this event to edit
            {
                LogMessage("User did not have a review for Event: " + eventId.ToString(), LogLevel.WARN);
                SetNotification(NotificationType.Error, "It doesn't look like you had a review for that event to edit", true);
                return RedirectToAction("DetailsByIdWithName", "Event", new { eventId = eventId });
            }
            model.Event = _supperClubRepository.GetEvent(model.EventId);
            return View("addreview1", model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult EditReview(Review model)
        {
            LogMessage("Begin Post Edit Review for Event: " + model.EventId.ToString());
            if (!string.IsNullOrEmpty(model.PublicReview)) // Don't publish if it's just a private review
                model.Publish = true;
            bool success = _supperClubRepository.UpdateReview(model);
            if (success)
            {
                LogMessage("Edited Review for Event: " + model.EventId.ToString());
                SetNotification(NotificationType.Success, "Your review has been updated.", true);
                return RedirectToAction("Details", "Event", new { eventId = model.EventId });
            }
            else
            {
                LogMessage("Could not Edit Review for Event: " + model.EventId.ToString());
                SetNotification(NotificationType.Error, "Couldn't update your review. Please try again.", true);
                return RedirectToAction("EditReview", "Event", new { eventId = model.EventId });
            }
        }

        [Authorize]
        public ActionResult EditReviewResponse(int eventId, int reviewId)
        {
            LogMessage("Started Editing Review Response for Event: " + eventId.ToString());
            ViewBag.Title = "Edit Review Response";
            if (eventId == 0 || reviewId == 0)
            {
                LogMessage("Invalid querystring parameters.EventId: " + eventId.ToString() + " Review Id:" + reviewId, LogLevel.WARN);
                SetNotification(NotificationType.Error, "Invalid querystring parameters. Could not fetch the review to edit.", true);
                return RedirectToAction("DetailsByIdWithName", "Event", new { eventId = eventId });
            }
            else
            {
                Review model = _supperClubRepository.GetReview(reviewId);
                if (model == null) // There is no review for this user for this event to edit
                {
                    LogMessage("User did not have a review for Event: " + eventId.ToString(), LogLevel.WARN);
                    SetNotification(NotificationType.Error, "It doesn't look like there is a review to edit", true);
                    return RedirectToAction("DetailsByIdWithName", "Event", new { eventId = eventId });
                }
                if (model.AdminResponse == null)
                    model.AdminResponse = "";
                if (model.HostResponse == null)
                    model.HostResponse = "";
                model.Event = _supperClubRepository.GetEvent(model.EventId);
                return View("EditReviewResponse", model);
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult EditReviewResponse(Review model)
        {
            LogMessage("Begin Post Edit Review Response for Event: " + model.EventId.ToString());

            if (model.HostResponse != null && model.HostResponse.Length > 0)
            {
                model.HostResponseDate = DateTime.Now;
            }

            if (SupperClub.Code.UserMethods.IsAdmin)
            {
                if (model.AdminResponse != null && model.AdminResponse.Length > 0)
                    model.AdminResponseDate = DateTime.Now;
            }
            bool success = _supperClubRepository.UpdateReview(model);
            if (success)
            {
                LogMessage("Edited Review Response for Event: " + model.EventId.ToString());
                SetNotification(NotificationType.Success, "Your review response has been updated.", true);
                model.Event = _supperClubRepository.GetEvent(model.EventId);
                if (SupperClub.Code.UserMethods.UserId == model.Event.SupperClub.UserId)
                {
                    ViewData["Role"] = "h";
                }
                return RedirectToAction("Details", "Event", new { eventId = model.EventId });
            }
            else
            {
                LogMessage("Could not Edit Review Response for Event: " + model.EventId.ToString());
                SetNotification(NotificationType.Error, "Couldn't update your review response. Please try again.", true);
                return RedirectToAction("EditReviewResponse", "Event", new { eventId = model.EventId, reviewId = model.Id });
            }
        }
        #endregion

        #region Private Image Processing

        private void MoveImage(string[] imageNames)
        {
            foreach (string imageName in imageNames)
            {
                string oldPathAndName = Server.MapPath(WebConfigurationManager.AppSettings["TempImagePath"]) + imageName;
                string newPathAndName = Server.MapPath(WebConfigurationManager.AppSettings["EventImagePath"]) + imageName;

                if (System.IO.File.Exists(oldPathAndName))
                {
                    System.IO.File.Copy(oldPathAndName, newPathAndName);
                    System.IO.File.Delete(oldPathAndName);
                }
            }
        }

        private Bitmap CreateImage(Bitmap original, int x, int y, int width, int height)
        {
            var img = new Bitmap(width, height);

            using (var g = Graphics.FromImage(img))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(original, new Rectangle(0, 0, width, height), x, y, width, height, GraphicsUnit.Pixel);
            }

            return img;
        }

        #endregion

        #region Private Methods
        private bool addEventToUserList(int eventId)
        {
            UserFavouriteEvent ufe = new UserFavouriteEvent();
            ufe.EventId = eventId;
            ufe.UserId = UserMethods.CurrentUser.Id;
            bool status = false;
            try
            {
                status = _supperClubRepository.AddEventToFavourite(ufe);
            }
            catch (Exception ex)
            {
                LogMessage("Error adding event to user's favourite event list.");
            }
            return status;
        }

        private Dictionary<int,DateTime> createEventViewList(int eventId)
        {
            Dictionary<int, DateTime> _evList = new Dictionary<int, DateTime>();
            _evList.Add(eventId, DateTime.Now);
            return _evList;
        }
        private Dictionary<int, DateTime> updateEventViewList(int eventId, Dictionary<int, DateTime> evList)
        {

            if (evList != null)
            {
                evList = cleanUpEventViewList(evList);
                if (evList.ContainsKey(eventId))
                    evList[eventId] = DateTime.Now;
                else
                    evList.Add(eventId, DateTime.Now);
            }
            else
                evList = createEventViewList(eventId);
            return evList;
        }
        private Dictionary<int, DateTime> cleanUpEventViewList(Dictionary<int, DateTime> evList)
        {
            foreach(KeyValuePair<int, DateTime> ev in evList)
            {
                if (ev.Value.AddMonths(_eventViewCookieExpirationInMonths) < DateTime.Now)
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
            Dictionary<int, DateTime> evList = JsonConvert.DeserializeObject<Dictionary<int,DateTime>>(evListJson);
            return evList;
        }
        #endregion
    }
}
