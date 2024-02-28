using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SupperClub.Domain.Repository;
using SupperClub.Domain;
using SupperClub.Data.EntityFramework;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.Data;
using CodeFirstStoredProcs;
using log4net;
using System.Data.Common;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Configuration;
using System.Text.RegularExpressions;
using SupperClub.Logger;
using Microsoft.JScript;
using System.Web.Security;

namespace SupperClub.Data
{
    public class SupperClubRepository : ISupperClubRepository, IDisposable
    {
        private static SupperClubContext db;
        private static readonly ILog log = LogManager.GetLogger(typeof(EntityFramework.SupperClubContext));

        public void Dispose()
        {
            if (db != null)
                db.Dispose();
        }

        public SupperClubRepository()
        {

            db = new SupperClubContext();
        }


        #region Events

        // Get Events
        public Event GetEvent(int eventId)
        {
            Event _event = null;
            try
            {
                var db = new SupperClubContext();
                {
                    _event = (from x in db.Events
                              where x.Id == eventId
                              select x).FirstOrDefault<Event>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return _event;
        }
        public IList<EventWaitList> GetEventWaitList(int eventId)
        {
            IList<EventWaitList> _eventWaitList = new List<EventWaitList>();
            try
            {
                var db = new SupperClubContext();
                // using (var db = new SupperClubContext())

                {
                    _eventWaitList = (from x in db.EventWaitLists
                                      where x.EventId == eventId
                                      orderby x.AddedDate
                                      select x).ToList<EventWaitList>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return _eventWaitList;
        }

        public IList<string> GetUnusedImageList(int eventId, List<string> imagesToBeDeletedEventList)
        {
            IList<string> _unusedEventImages = new List<string>();
            try
            {
                var db = new SupperClubContext();
                {
                    List<string> _usedEventImageList = (from x in db.EventImages
                                                          join y in imagesToBeDeletedEventList on x.ImagePath equals y
                            where x.EventId != eventId
                            select x.ImagePath).ToList();

                    if(_usedEventImageList != null)
                    {
                         _unusedEventImages = (from x in db.EventImages
                                                join y in imagesToBeDeletedEventList on x.ImagePath equals y
                                                where x.EventId == eventId &&  !_usedEventImageList.Contains(x.ImagePath)
                                                  select y).ToList();                       
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return _unusedEventImages;
        }


        //get event with name url
        public Event GetEventByName(string url)
        {
            Event _event = null;
            try
            {
                var db = new SupperClubContext();
                {
                    _event = (from x in db.Events
                              where x.UrlFriendlyName == url
                              select x).FirstOrDefault<Event>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return _event;
        }

        public Event GetFirstActiveEvent()
        {
            Event _event = null;
            try
            {
                var db = new SupperClubContext();
                {
                    _event = (from x in db.Events
                              where x.Status == (int)SupperClub.Domain.EventStatus.Active
                              select x).FirstOrDefault<Event>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return _event;
        }

        public IList<Event> GetAllEvents()
        {
            IList<Event> events = null;
            try
            {
                var db = new SupperClubContext();
                {
                    events = (from x in db.Events
                              select x).ToList<Event>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return events;
        }

        public IList<Event> GetNewlyCreatedEvents()
        {
            IList<Event> events = null;
            try
            {
                var db = new SupperClubContext();
                {
                    events = (from x in db.Events
                              where x.Status == (int)SupperClub.Domain.EventStatus.New
                              select x).ToList<Event>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return events;
        }

        public IList<Event> GetRecentlyAddedEvents()
        {
            IList<Event> events = null;
            try
            {
                using (var db = new SupperClubContext())
                {
                    events = (from x in db.Events
                              //join et in db.EventTags on x.Id equals et.EventId
                              where x.SupperClub.Active == true && x.Status == (int)SupperClub.Domain.EventStatus.Active && x.Start > DateTime.Now && x.Private == false //&& et.TagId == 5
                              orderby x.Start descending
                              select x).Take(20).ToList<Event>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return events;
        }

        public IList<Event> GetPastEvents(DateTime eventsAfter)
        {
            IList<Event> events = null;
            try
            {
                var db = new SupperClubContext();
                {
                    events = (from x in db.Events
                              where x.Start > eventsAfter && x.End < DateTime.Now
                              orderby x.Start descending
                              select x).ToList<Event>();

                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return events;
        }

        public EventSeating GetEventSeating(int seatingId)
        {
            EventSeating _eventSeating = null;
            try
            {
                var db = new SupperClubContext();
                {
                    _eventSeating = (from x in db.EventSeatings
                                     where x.Id == seatingId
                                     select x).FirstOrDefault<EventSeating>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return _eventSeating;
        }

        public EventMenuOption GetEventMenuOption(int menuOptionId)
        {
            EventMenuOption _eventMenuOption = null;
            try
            {
                var db = new SupperClubContext();
                {
                    _eventMenuOption = (from x in db.EventMenuOptions
                                        where x.Id == menuOptionId
                                        select x).FirstOrDefault<EventMenuOption>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return _eventMenuOption;
        }

        // Events for SupperClubs
        public IList<Event> GetPastEventsForASupperClub(int supperClubId)
        {
            IList<Event> events = null;
            try
            {
                var db = new SupperClubContext();
                {
                    events = (from x in db.Events
                              where x.SupperClubId == supperClubId && x.End < DateTime.Now
                              orderby x.Start descending
                              select x).ToList<Event>();

                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return events;
        }

        public IList<Event> GetFutureEventsForASupperClub(int supperClubId)
        {
            IList<Event> events = null;
            try
            {
                var db = new SupperClubContext();
                {
                    events = (from x in db.Events
                              where x.SupperClubId == supperClubId && x.Start > DateTime.Now
                              orderby x.Start ascending
                              select x).ToList<Event>();

                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return events;
        }

        public IList<Event> GetAllEventsForASupperClub(int supperClubId)
        {
            IList<Event> events = null;
            try
            {
                var db = new SupperClubContext();
                {
                    events = (from x in db.Events
                              where x.SupperClubId == supperClubId
                              orderby x.Start descending
                              select x).ToList<Event>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return events;
        }

        public IList<Event> GetAllActiveEventsForASupperClub(int supperClubId)
        {
            IList<Event> events = null;
            try
            {
                var db = new SupperClubContext();
                {
                    events = (from x in db.Events
                              where x.SupperClubId == supperClubId && x.Start > DateTime.Now && (x.Status == (int)EventStatus.Active || x.Status == (int)EventStatus.New)
                              orderby x.Start descending
                              select x).ToList<Event>();

                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return events;
        }
        public Event GetNextEventForASupperClub(int supperClubId)
        {
            Event _event = null;
            try
            {
                var db = new SupperClubContext();
                {
                    _event = (from x in db.Events
                              where x.SupperClubId == supperClubId && x.Start >= DateTime.Now
                              orderby x.Start
                              select x).FirstOrDefault<Event>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return _event;
        }


        // Events for Users
        public IList<Event> GetPastEventsForAUser(Guid userId)
        {
            IList<Event> events = null;
            try
            {
                var db = new SupperClubContext();
                {
                    events = (from x in db.Events
                              join ea in db.EventAttendees on x.Id equals ea.EventId
                              join u in db.Users on ea.UserId equals u.Id
                              where u.Id == userId && x.End < DateTime.Now
                              orderby x.Start descending
                              select x).Distinct().ToList<Event>();

                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return events;
        }

        public IList<Event> GetFutureEventsForAUser(Guid userId)
        {
            IList<Event> events = null;
            try
            {
                var db = new SupperClubContext();
                {
                    events = (from x in db.Events
                              join ea in db.EventAttendees on x.Id equals ea.EventId
                              join u in db.Users on ea.UserId equals u.Id
                              where u.Id == userId && x.Start > DateTime.Now
                              orderby x.Start ascending
                              select x).Distinct().ToList<Event>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return events;
        }

        public IList<MyBookedEvent> GetUserPastEvents(Guid userId)
        {
            IList<MyBookedEvent> events = null;
            IList<MyBookedEvent> distincteventResult = null;
            try
            {
                var db = new SupperClubContext();
                {
                    string completeStatus = TicketBasketStatus.Complete.ToString();
                    IList<MyBasketDetails> baskets = (from t in db.Tickets
                                                      join tb in db.TicketBaskets on t.BasketId equals tb.Id
                                                      join e in db.Events on t.EventId equals e.Id
                                                      where t.UserId == userId && tb.Status == completeStatus && e.Start < DateTime.Now
                                                      group tb by new { EventId = t.EventId, SeatingId = t.SeatingId }
                                                          into gr
                                                          select new MyBasketDetails { EventId = gr.Key.EventId, EventSeatingId = gr.Key.SeatingId, BookingReference = gr.Min(tb => tb.BookingReference) }).Distinct().ToList<MyBasketDetails>();
                    events = (from x in db.Events
                              join ea in db.EventAttendees on x.Id equals ea.EventId
                              join u in db.Users on ea.UserId equals u.Id
                              join es in db.EventSeatings on ea.SeatingId equals es.Id
                              //join r in db.Reviews on x.Id equals r.EventId
                              //join t in db.Tickets on ea.EventId equals t.EventId
                              //join tb in baskets on new { ea.EventId, ea.SeatingId } equals new { tb.EventId, tb.SeatingId }
                              where u.Id == userId && x.Start < DateTime.Now
                              orderby x.Start descending
                              group new { ea.NumberOfGuests }
                              by new
                              {
                                  EventId = x.Id,
                                  EventName = x.Name,
                                  EventImage = x.ImagePath,
                                  EventMultiSeating = x.MultiSeating,
                                  EventSeatingId = ea.SeatingId,
                                  EventStartDateTime = (x.MultiSeating ? es.Start : x.Start),
                                  EventEndDateTime = (x.MultiSeating ? es.End : x.End),
                                  EventURL = x.UrlFriendlyName,
                                  EventAddress = x.Address + (string.IsNullOrEmpty(x.Address2) ? "" : ", " + x.Address2),
                                  EventCity = x.City,
                                  EventPostCode = x.PostCode,
                                  EventLatitude = x.Latitude,
                                  EventLongitude = x.Longitude,
                                  EventCost = x.Cost,
                                  GuestName = u.FirstName + " " + u.LastName
                              } into gcs
                              select
                            new MyBookedEvent
                            {
                                EventId = gcs.Key.EventId,
                                EventName = gcs.Key.EventName,
                                EventImage = gcs.Key.EventImage,
                                EventSeatingId = gcs.Key.EventSeatingId,
                                EventStart = gcs.Key.EventEndDateTime,
                                EventEnd = gcs.Key.EventEndDateTime,
                                EventURL = gcs.Key.EventURL,
                                EventAddress = gcs.Key.EventAddress,
                                EventCity = gcs.Key.EventCity,
                                EventPostCode = gcs.Key.EventPostCode,
                                GuestName = gcs.Key.GuestName,
                                EventLatitude = gcs.Key.EventLatitude,
                                EventLongitude = gcs.Key.EventLongitude,
                                BaseCost = gcs.Key.EventCost,
                                NumberOfTickets = gcs.Sum(e => e.NumberOfGuests)
                            }).ToList<MyBookedEvent>();
                    distincteventResult = (from p in events
                                           join b in baskets on new { p.EventId, p.EventSeatingId } equals new { b.EventId, b.EventSeatingId }
                                           select new MyBookedEvent
                                           {
                                               EventId = p.EventId,
                                               EventName = p.EventName,
                                               EventImage = p.EventImage,
                                               EventSeatingId = p.EventSeatingId,
                                               EventStart = p.EventStart,
                                               EventEnd = p.EventEnd,
                                               EventURL = p.EventURL,
                                               EventAddress = p.EventAddress,
                                               EventCity = p.EventCity,
                                               EventPostCode = p.EventPostCode,
                                               GuestName = p.GuestName,
                                               EventLatitude = p.EventLatitude,
                                               EventLongitude = p.EventLongitude,
                                               BaseCost = p.BaseCost,
                                               NumberOfTickets = p.NumberOfTickets,
                                               BookingReference = b.BookingReference
                                           }).ToList<MyBookedEvent>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return distincteventResult;
        }

        public IList<MyBookedEvent> GetUserFutureEvents(Guid userId)
        {
            IList<MyBookedEvent> events = null;
            IList<MyBookedEvent> distincteventResult = null;
            try
            {
                var db = new SupperClubContext();
                {
                    string completeStatus = TicketBasketStatus.Complete.ToString();
                    IList<MyBasketDetails> baskets = (from t in db.Tickets
                                                      join tb in db.TicketBaskets on t.BasketId equals tb.Id
                                                      join e in db.Events on t.EventId equals e.Id
                                                      where t.UserId == userId && tb.Status == completeStatus && e.Start >= DateTime.Now
                                                      group tb by new { EventId = t.EventId, SeatingId = t.SeatingId }
                                                          into gr
                                                          select new MyBasketDetails { EventId = gr.Key.EventId, EventSeatingId = gr.Key.SeatingId, BookingReference = gr.Min(tb => tb.BookingReference) }).Distinct().ToList<MyBasketDetails>();
                    events = (from x in db.Events
                              join ea in db.EventAttendees on x.Id equals ea.EventId
                              join u in db.Users on ea.UserId equals u.Id
                              join es in db.EventSeatings on ea.SeatingId equals es.Id
                              //join t in db.Tickets on ea.EventId equals t.EventId
                              //join tb in baskets on new { ea.EventId, ea.SeatingId } equals new { tb.EventId, tb.SeatingId }
                              where u.Id == userId && x.Start >= DateTime.Now
                              orderby x.Start ascending
                              group new { ea.NumberOfGuests }
                              by new
                              {
                                  EventId = x.Id,
                                  EventName = x.Name,
                                  EventImage = x.ImagePath,
                                  EventMultiSeating = x.MultiSeating,
                                  EventSeatingId = ea.SeatingId,
                                  EventStartDateTime = (x.MultiSeating ? es.Start : x.Start),
                                  EventEndDateTime = (x.MultiSeating ? es.End : x.End),
                                  EventURL = x.UrlFriendlyName,
                                  EventAddress = x.Address + (string.IsNullOrEmpty(x.Address2) ? "" : ", " + x.Address2),
                                  EventCity = x.City,
                                  EventPostCode = x.PostCode,
                                  EventLatitude = x.Latitude,
                                  EventLongitude = x.Longitude,
                                  EventCost = x.Cost,
                                  GuestName = u.FirstName + " " + u.LastName
                              } into gcs
                              select
                            new MyBookedEvent
                            {
                                EventId = gcs.Key.EventId,
                                EventName = gcs.Key.EventName,
                                EventImage = gcs.Key.EventImage,
                                EventSeatingId = gcs.Key.EventSeatingId,
                                EventStart = gcs.Key.EventEndDateTime,
                                EventEnd = gcs.Key.EventEndDateTime,
                                EventURL = gcs.Key.EventURL,
                                EventAddress = gcs.Key.EventAddress,
                                EventCity = gcs.Key.EventCity,
                                EventPostCode = gcs.Key.EventPostCode,
                                GuestName = gcs.Key.GuestName,
                                EventLatitude = gcs.Key.EventLatitude,
                                EventLongitude = gcs.Key.EventLongitude,
                                BaseCost = gcs.Key.EventCost,
                                NumberOfTickets = gcs.Sum(e => e.NumberOfGuests)
                            }).ToList<MyBookedEvent>();
                    distincteventResult = (from p in events
                                           join b in baskets on new { p.EventId, p.EventSeatingId } equals new { b.EventId, b.EventSeatingId }
                                           select new MyBookedEvent
                                           {
                                               EventId = p.EventId,
                                               EventName = p.EventName,
                                               EventImage = p.EventImage,
                                               EventSeatingId = p.EventSeatingId,
                                               EventStart = p.EventStart,
                                               EventEnd = p.EventEnd,
                                               EventURL = p.EventURL,
                                               EventAddress = p.EventAddress,
                                               EventCity = p.EventCity,
                                               EventPostCode = p.EventPostCode,
                                               GuestName = p.GuestName,
                                               EventLatitude = p.EventLatitude,
                                               EventLongitude = p.EventLongitude,
                                               BaseCost = p.BaseCost,
                                               NumberOfTickets = p.NumberOfTickets,
                                               BookingReference = b.BookingReference
                                           }).ToList<MyBookedEvent>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return distincteventResult;
        }
        public IList<Review> GetSupperClubReviews(int eventId, int supperClubId)
        {
            List<Review> reviews = new List<Review>();
            try
            {
                var db = new SupperClubContext();
                {
                    //get the current event's reviews first
                    var r1 = (from x in db.Reviews
                              where x.EventId == eventId
                              select x).Distinct().OrderByDescending(x => x.DateCreated).ToList<Review>();

                    if (r1 != null && r1.Count > 0)
                        reviews.AddRange(r1);
                    else
                    {
                        var r3 = (from r in db.Reviews
                                  join e in db.Events on r.EventId equals e.Id
                                  where e.SupperClubId == supperClubId
                                  select r).Distinct().OrderByDescending(x => x.DateCreated).ToList<Review>();
                        if (r3 != null && r3.Count > 0)
                            reviews.AddRange(r3);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return (IList<Review>)reviews;
        }
        public IList<Event> GetWaitlistedEventsForAUser(Guid userId)
        {
            IList<Event> events = null;
            try
            {
                var db = new SupperClubContext();
                {
                    events = (from x in db.Events
                              join ewl in db.EventWaitLists on x.Id equals ewl.EventId
                              join u in db.Users on ewl.UserId equals u.Id
                              where u.Id == userId && x.Start > DateTime.Now
                              orderby x.Start
                              select x).Distinct().ToList<Event>();

                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return events;
        }

        public IList<Event> GetFavouriteEventsForAUser(Guid userId)
        {
            IList<Event> events = null;
            try
            {
                var db = new SupperClubContext();
                {
                    events = (from x in db.Events
                              join ewl in db.UserFavouriteEvents on x.Id equals ewl.EventId
                              join u in db.Users on ewl.UserId equals u.Id
                              where u.Id == userId
                              orderby x.Start
                              select x).Distinct().ToList<Event>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return events;
        }
        public IList<int> GetFavouriteEventIdsForAUser(Guid userId)
        {
            IList<int> events = null;
            try
            {
                var db = new SupperClubContext();
                {
                    events = (from x in db.Events
                              join ewl in db.UserFavouriteEvents on x.Id equals ewl.EventId
                              join u in db.Users on ewl.UserId equals u.Id
                              where u.Id == userId && x.Start > DateTime.Now && x.Status == (int)EventStatus.Active
                              orderby x.Start
                              select ewl.EventId).Distinct().ToList<int>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return events;
        }
        public bool IsCurrentEventUsersFavourite(int eventId, Guid userId)
        {
            bool status = false;
            try
            {
                var db = new SupperClubContext();
                {
                    var _event = (from ufsc in db.UserFavouriteEvents
                                  where ufsc.UserId == userId && ufsc.EventId == eventId
                                  select ufsc).FirstOrDefault();
                    if (_event != null)
                        status = true;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return status;
        }
        public bool IsCurrentSupperClubUsersFavourite(int supperClubId, Guid userId)
        {
            bool status = false;
            try
            {
                var db = new SupperClubContext();
                {
                    var supperClub = (from ufsc in db.UserFavouriteSupperClubs
                                      where ufsc.UserId == userId && ufsc.SupperClubId == supperClubId
                                      select ufsc).FirstOrDefault();
                    if (supperClub != null)
                        status = true;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return status;
        }
        public IList<Event> GetAllEventsForAUser(Guid userId)
        {
            IList<Event> events = null;
            try
            {
                var db = new SupperClubContext();
                {
                    events = (from x in db.Events
                              join ea in db.EventAttendees on x.Id equals ea.EventId
                              join u in db.Users on ea.UserId equals u.Id
                              where u.Id == userId
                              orderby x.Start descending
                              select x).ToList<Event>();

                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return events;
        }

        public Event GetNextEventForAUser(Guid userId)
        {
            Event _event = null;
            try
            {
                var db = new SupperClubContext();
                {
                    _event = (from x in db.Events
                              join u in db.Users on x.EventAttendees equals u.EventAttendees
                              where u.Id == userId && x.Start >= DateTime.Now
                              orderby x.Start
                              select x).FirstOrDefault<Event>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return _event;
        }

        public EventSeating GetEventSeatingForAUser(int eventId, Guid userId)
        {
            EventSeating eventSeating = null;
            try
            {
                var db = new SupperClubContext();
                {
                    eventSeating = (from x in db.EventSeatings
                                    join ea in db.EventAttendees on x.Id equals ea.SeatingId
                                    where ea.UserId == userId && ea.EventId == eventId
                                    select x).Distinct().FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return eventSeating;
        }

        public IList<EventMenuOption> GetEventMenuOptionForAUser(int eventId, Guid userId)
        {
            IList<EventMenuOption> lstMenuOption = null;

            try
            {
                var db = new SupperClubContext();
                {
                    lstMenuOption = (from x in db.EventMenuOptions
                                     join ea in db.EventAttendees on x.Id equals ea.MenuOptionId
                                     where ea.UserId == userId && ea.EventId == eventId
                                     select x).ToList<EventMenuOption>();
                }

            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return lstMenuOption;


        }
        public Event GetFavouriteEvent()
        {
            Event favEvent = null;
            try
            {
                using (var db = new SupperClubContext())
                {
                    favEvent = (from x in db.Events
                                join fe in db.FavouriteEvents on x.Id equals fe.EventId
                                select x).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return favEvent;
        }

        //get tag with URL friendly name
        public Tag GetTagByName(string url)
        {
            Tag _tag = null;
            try
            {
                var db = new SupperClubContext();
                {
                    _tag = (from x in db.Tags
                            where x.UrlFriendlyName.ToLower() == url.ToLower()
                            select x).FirstOrDefault<Tag>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return _tag;
        }

        public City GetCityByName(string url)
        {
            City _city = null;
            try
            {
                var db = new SupperClubContext();
                {
                    _city = (from x in db.Cities
                             where x.UrlFriendlyName.ToLower() == url.ToLower()
                             select x).FirstOrDefault<City>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return _city;
        }
        public Area GetAreaByName(string url)
        {
            Area _area = null;
            try
            {
                var db = new SupperClubContext();
                {
                    _area = (from x in db.Areas
                             where x.UrlFriendlyName.ToLower() == url.ToLower()
                             select x).FirstOrDefault<Area>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return _area;
        }
        //get Search Category with category name
        public IList<Tag> GetSearchCategoryTags(string categoryName)
        {
            IList<Tag> _tags = null;
            try
            {
                var db = new SupperClubContext();
                {
                    _tags = (from x in db.SearchCategories
                             join y in db.SearchCategoryTags on x.Id equals y.SearchCategoryId
                             join t in db.Tags on y.TagId equals t.Id
                             where x.UrlFriendlyName.ToLower() == categoryName.ToLower()
                             select t).ToList<Tag>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return _tags;
        }
        //get Search Category with category name
        public SearchCategory GetSearchCategoryByName(string name)
        {
            SearchCategory _searchCategory = null;
            try
            {
                var db = new SupperClubContext();
                {
                    _searchCategory = (from x in db.SearchCategories
                                       where x.UrlFriendlyName.ToLower() == name.ToLower()
                                       select x).FirstOrDefault<SearchCategory>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return _searchCategory;
        }

        public SearchCategoryTag GetSearchCategoryTagByTagId(int tagId)
        {
            SearchCategoryTag _searchCategoryTag = null;
            try
            {
                var db = new SupperClubContext();
                {
                    _searchCategoryTag = (from x in db.SearchCategoryTags
                                          where x.TagId == tagId
                                          select x).FirstOrDefault<SearchCategoryTag>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return _searchCategoryTag;
        }
        public bool RankEvent(int eventId, Guid eventAttendeeId, int ranking)
        {
            try
            {
                var db = new SupperClubContext();
                {
                    EventAttendee ea = db.EventAttendees.FirstOrDefault(x => x.UserId == eventAttendeeId && x.EventId == eventId);
                    ea.EventRanking = ranking;
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return false;
        }

        public int AddTag(string tagName, string tagSeoName)
        {
            int tagId = 0;
            try
            {
                Tag tag = new Tag();
                tag.Name = tagName;
                tag.UrlFriendlyName = tagSeoName;
                using (var db = new SupperClubContext())
                {
                    var checkName = (from x in db.Tags
                                     where x.Name.ToLower() == tag.Name.ToLower()
                                     select x).FirstOrDefault<Tag>();
                    if (checkName != null)
                    {
                        tagId = -1;
                    }
                    else
                    {
                        var checkSeo = (from x in db.Tags
                                        where x.UrlFriendlyName.ToLower() == tag.UrlFriendlyName.ToLower()
                                        select x).FirstOrDefault<Tag>();
                        if (checkSeo != null)
                        {
                            tagId = -2;
                        }
                        else
                        {
                            Tag _tag = db.Tags.Add(tag);
                            db.SaveChanges();
                            tagId = _tag.Id;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return tagId;
        }

        public int ValidateTagName(string tagName, string tagSeoName, int tagId = 0)
        {
            int _tagId = 0;
            try
            {
                using (var db = new SupperClubContext())
                {
                    Tag checkName = new Tag();
                    if (tagId == 0)
                    {
                        checkName = (from x in db.Tags
                                     where x.Name.ToLower() == tagName.ToLower()
                                     select x).FirstOrDefault<Tag>();
                    }
                    else
                    {
                        checkName = (from x in db.Tags
                                     where x.Name.ToLower() == tagName.ToLower() && x.Id != tagId
                                     select x).FirstOrDefault<Tag>();
                    }
                    if (checkName != null && checkName.Id > 0)
                    {
                        _tagId = -1;
                    }
                    else
                    {
                        Tag checkSeo = new Tag();
                        if (tagId == 0)
                        {
                            checkSeo = (from x in db.Tags
                                        where x.UrlFriendlyName.ToLower() == tagSeoName.ToLower()
                                        select x).FirstOrDefault<Tag>();
                        }
                        else
                        {
                            checkSeo = (from x in db.Tags
                                        where x.UrlFriendlyName.ToLower() == tagSeoName.ToLower() && x.Id != tagId
                                        select x).FirstOrDefault<Tag>();
                        }
                        if (checkSeo != null && checkName.Id > 0)
                        {
                            _tagId = -2;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return _tagId;
        }

        public int ValidateProfileTagName(string tagName, int tagId = 0)
        {
            int _tagId = 0;
            try
            {
                using (var db = new SupperClubContext())
                {
                    ProfileTag checkName = new ProfileTag();
                    if (tagId == 0)
                    {
                        checkName = (from x in db.ProfileTags
                                     where x.Name.ToLower() == tagName.ToLower()
                                     select x).FirstOrDefault<ProfileTag>();
                    }
                    else
                    {
                        checkName = (from x in db.ProfileTags
                                     where x.Name.ToLower() == tagName.ToLower() && x.Id != tagId
                                     select x).FirstOrDefault<ProfileTag>();
                    }
                    if (checkName != null && checkName.Id > 0)
                    {
                        _tagId = -1;
                    }
                    else
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return _tagId;
        }
        public bool CreateProfileTag(ProfileTag tag)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    db.ProfileTags.Add(tag);
                    db.SaveChanges();


                }
                return true;
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }

            return false;
        }

        public bool CreateTag(Tag tag)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    db.Tags.Add(tag);
                    db.SaveChanges();
                    var cuisine = (from x in db.Cuisines
                                   where x.Active == true && x.Name.ToLower() == tag.Name.ToLower()
                                   select x).FirstOrDefault<Cuisine>();
                    if (cuisine != null)
                    {
                        cuisine.TagId = tag.Id;
                        db.Cuisines.Attach(cuisine);
                        db.Entry<Cuisine>(cuisine).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
                return true;
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }

            return false;
        }

        public bool UpdateTag(Tag _tag)
        {
            try
            {
                var db = new SupperClubContext();
                {
                    //Update the tag table
                    db.Entry(_tag).State = EntityState.Modified;
                    db.SaveChanges();

                    return true;
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message + ex.StackTrace, ex);
            }
            return false;
        }

        public bool UpdateProfileTag(ProfileTag _tag)
        {
            try
            {
                var db = new SupperClubContext();
                {
                    //Update the tag table
                    db.Entry(_tag).State = EntityState.Modified;
                    db.SaveChanges();

                    return true;
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message + ex.StackTrace, ex);
            }
            return false;
        }
        public Tag GetTag(int tagId)
        {
            Tag _tag = null;
            try
            {
                var db = new SupperClubContext();
                {
                    _tag = (from x in db.Tags
                            where x.Id == tagId
                            select x).FirstOrDefault<Tag>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return _tag;
        }

        public ProfileTag GetProfileTag(int tagId)
        {
            ProfileTag _tag = null;
            try
            {
                var db = new SupperClubContext();
                {
                    _tag = (from x in db.ProfileTags
                            where x.Id == tagId
                            select x).FirstOrDefault<ProfileTag>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return _tag;
        }

        public string GetSupperClubProfileTags(int supperClubId)
        {

            string tags = string.Empty;
            List<int> lstTags = new List<int>();

            try
            {
                var db = new SupperClubContext();
                {
                    lstTags = (from x in db.SupperClubProfileTags
                               where x.SupperClubId == supperClubId
                               select x.ProfileTagId).ToList<int>();
                }

                foreach (int i in lstTags)
                {
                    tags += i + ",";
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return tags;


        }
        public int ValidateCategoryName(string categoryName, string categorySeoName, int categoryId = 0)
        {
            int _categoryId = 0;
            try
            {
                using (var db = new SupperClubContext())
                {
                    SearchCategory checkName = new SearchCategory();
                    if (categoryId == 0)
                    {
                        checkName = (from x in db.SearchCategories
                                     where x.Name.ToLower() == categoryName.ToLower()
                                     select x).FirstOrDefault<SearchCategory>();
                    }
                    else
                    {
                        checkName = (from x in db.SearchCategories
                                     where x.Name.ToLower() == categoryName.ToLower() && x.Id != categoryId
                                     select x).FirstOrDefault<SearchCategory>();
                    }
                    if (checkName != null && checkName.Id > 0)
                    {
                        _categoryId = -1;
                    }
                    else
                    {
                        SearchCategory checkSeo = new SearchCategory();
                        if (categoryId == 0)
                        {
                            checkSeo = (from x in db.SearchCategories
                                        where x.UrlFriendlyName.ToLower() == categorySeoName.ToLower()
                                        select x).FirstOrDefault<SearchCategory>();
                        }
                        else
                        {
                            checkSeo = (from x in db.SearchCategories
                                        where x.UrlFriendlyName.ToLower() == categorySeoName.ToLower() && x.Id != categoryId
                                        select x).FirstOrDefault<SearchCategory>();
                        }
                        if (checkSeo != null && checkName.Id > 0)
                        {
                            _categoryId = -2;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return _categoryId;
        }

        public bool CreateCategory(SearchCategory _searchCategory)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    db.SearchCategories.Add(_searchCategory);
                    db.SaveChanges();
                    //Once Search Category created with Id
                    UpdateSearchCategoryIds(_searchCategory);
                    db.SaveChanges();
                }
                return true;
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }

            return false;
        }

        public bool UpdateCategory(SearchCategory _searchCategory)
        {
            try
            {
                var db = new SupperClubContext();
                {
                    //Update the tag table
                    db.Entry(_searchCategory).State = EntityState.Modified;
                    db.SaveChanges();
                    UpdateSearchCategoryIds(_searchCategory);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message + ex.StackTrace, ex);
            }
            return false;
        }

        public SearchCategory GetSearchCategory(int searchCategoryId)
        {
            SearchCategory _searchCategory = null;
            try
            {
                var db = new SupperClubContext();
                {
                    _searchCategory = (from x in db.SearchCategories
                                       where x.Id == searchCategoryId
                                       select x).FirstOrDefault<SearchCategory>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return _searchCategory;
        }

        public UserVoucherTypeDetail CreateUserVoucherTypeDetail(UserVoucherTypeDetail userVoucherTypeDetail)
        {
            UserVoucherTypeDetail _userVoucherTypeDetail = null;
            try
            {
                using (var db = new SupperClubContext())
                {
                    _userVoucherTypeDetail = db.UserVoucherTypeDetails.Add(userVoucherTypeDetail);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }

            return _userVoucherTypeDetail;
        }

        public BookingConfirmationToFriends CreateBookingConfiramtionToFriends(BookingConfirmationToFriends bookingConfirmationToFriends)
        {
            BookingConfirmationToFriends _bookingConfirmationToFriends = null;

            try
            {
                using (var db = new SupperClubContext())
                {
                    _bookingConfirmationToFriends = db.BookingConfirmationToFriends.Add(bookingConfirmationToFriends);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }

            return _bookingConfirmationToFriends;
        }


        public SearchLogDetail CreateSeacrchLogDetail(SearchLogDetail searchLogDetail)
        {
            SearchLogDetail _searchLogDetail = null;
            try
            {
                using (var db = new SupperClubContext())
                {
                    _searchLogDetail = db.SearchLogDetails.Add(searchLogDetail);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }

            return _searchLogDetail;
        }

        public SupperclubStatusChangeLog AddSupperclubStatusChangeLog(SupperclubStatusChangeLog supperclubStatusChangeLog)
        {
            SupperclubStatusChangeLog _supperclubStatusChangeLog = null;
            try
            {
                using (var db = new SupperClubContext())
                {
                    _supperclubStatusChangeLog = db.SupperclubStatusChangeLog.Add(supperclubStatusChangeLog);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }

            return _supperclubStatusChangeLog;
        }


        public bool IsUserValidToRefer(User user)
        {
            bool valid = false;
            try
            {
                var db = new SupperClubContext();
                {
                    Ticket ticket = (from u in db.Tickets
                                     where u.UserId == user.Id
                                     select u).FirstOrDefault<Ticket>();

                    if (ticket != null)
                        valid = true;
                    else
                        valid = false;

                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return valid;
        }

        public bool IsUserValidByRefer(string email)
        {

            bool valid = false;
            try
            {
                var db = new SupperClubContext();
                {

                    UserVoucherTypeDetail detail = (from u in db.UserVoucherTypeDetails
                                                    where (u.FriendEmailId.Trim().ToLower() == email.Trim().ToLower()) && (u.VType == 1)
                                                    select u).FirstOrDefault<UserVoucherTypeDetail>();

                    if (detail != null)
                        valid = false;
                    else
                        valid = true;
                }


            }

            catch (Exception ex)
            {
                throw ex;
            }
            return valid;
        }

        public bool IsValidUser(Guid userId)
        {
            bool valid = false;
            try
            {
                var db = new SupperClubContext();
                {
                    User user = (from u in db.Users
                                 where u.Id == userId
                                 select u).FirstOrDefault<User>();

                    if (user != null)
                        valid = true;
                    else
                        valid = false;

                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }

            return valid;
        }
        public Voucher CreateVoucher(Voucher voucher)
        {
            Voucher _voucher = null;
            try
            {
                using (var db = new SupperClubContext())
                {
                    _voucher = (from v in db.Vouchers
                                where v.Code.ToLower() == voucher.Code.ToLower()
                                select v).FirstOrDefault<Voucher>();

                    if (_voucher == null)
                    {
                        _voucher = db.Vouchers.Add(voucher);
                        db.SaveChanges();
                    }
                    else
                    {
                        voucher.Id = -1;
                        return voucher;
                    }
                }
            }

            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }

            return _voucher;
        }

        public bool CheckVoucherCode(string voucherCode)
        {
            bool exist = true;
            try
            {
                using (var db = new SupperClubContext())
                {
                    Voucher _voucher = new Voucher();
                    _voucher = (from x in db.Vouchers
                                where x.Code.ToLower() == voucherCode.ToLower()
                                select x).FirstOrDefault<Voucher>();
                    if (_voucher != null)
                        exist = true;
                    else
                        exist = false;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return exist;
        }

        public IList<SupperClub.Domain.Voucher> GetAllActiveVouchers()
        {
            IList<SupperClub.Domain.Voucher> vouchers = null;
            try
            {
                var db = new SupperClubContext();
                {
                    vouchers = (from x in db.Vouchers
                                where x.Active == true
                                select x).OrderByDescending(x => x.CreatedDate).ToList<SupperClub.Domain.Voucher>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return vouchers;
        }

        public IList<SupperClub.Domain.Voucher> GetAllInactiveVouchers()
        {
            IList<SupperClub.Domain.Voucher> vouchers = null;
            try
            {
                var db = new SupperClubContext();
                {
                    vouchers = (from x in db.Vouchers
                                where x.Active == false
                                select x).OrderByDescending(x => x.CreatedDate).ToList<SupperClub.Domain.Voucher>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return vouchers;
        }

        public bool ChangeVoucherStatus(int voucherId, bool newStatus)
        {
            bool success = false;
            try
            {
                using (var db = new SupperClubContext())
                {
                    Domain.Voucher _voucher = db.Vouchers.FirstOrDefault(x => x.Id == voucherId);
                    if (_voucher != null)
                    {
                        _voucher.Active = newStatus;
                        db.SaveChanges();
                        success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return success;
        }

        public bool DeactivateVoucher(string voucherCode)
        {
            bool success = false;
            try
            {
                using (var db = new SupperClubContext())
                {
                    Domain.Voucher _voucher = db.Vouchers.FirstOrDefault(x => x.Code.ToLower() == voucherCode.ToLower());
                    if (_voucher != null)
                    {
                        _voucher.Active = false;
                        db.SaveChanges();
                        success = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return success;
        }
        public bool ValidateEventUrlFriendlyName(string urlFriendlyName)
        {
            bool isValid = false;
            try
            {
                using (var db = new SupperClubContext())
                {
                    Event checkEvent = new Event();
                    checkEvent = (from x in db.Events
                                  where x.UrlFriendlyName.ToLower() == urlFriendlyName.ToLower() && (x.Status == (int)EventStatus.Active || x.Status == (int)EventStatus.New)
                                  select x).FirstOrDefault<Event>();
                    if (checkEvent != null && checkEvent.Id > 0)
                        isValid = true;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return isValid;
        }

        public IList<Event> GetActiveFutureEvents(string urlFriendlyName)
        {
            IList<Event> futureEvents = null;
            try
            {
                var db = new SupperClubContext();
                {
                    futureEvents = (from x in db.Events
                                    where x.UrlFriendlyName.ToLower() == urlFriendlyName.ToLower() && x.Start > DateTime.Now && x.Status == (int)EventStatus.Active && x.Private == false
                                    select x).OrderBy(x => x.Start).ToList<Event>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return futureEvents;
        }

        public Event GetLastActivePastEvent(string urlFriendlyName)
        {
            Event pastEvent = null;
            try
            {
                var db = new SupperClubContext();
                {
                    pastEvent = (from x in db.Events
                                 where x.UrlFriendlyName.ToLower() == urlFriendlyName.ToLower() && x.Start < DateTime.Now && x.Status == (int)EventStatus.Active
                                 select x).OrderByDescending(x => x.Start).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return pastEvent;
        }
        public bool UpdateImageUrl(ImageType imageType, string oldImageUrl, string newImageUrl)
        {
            bool success = false;
            try
            {
                var db = new SupperClubContext();
                {

                    //Update the Image URL
                    switch(imageType)
                    {
                        case ImageType.Event:
                            EventImage ei = (from x in db.EventImages
                                 where x.ImagePath.ToLower() == oldImageUrl.ToLower()
                                                 select x).FirstOrDefault();
                                 if(ei != null)
                                 {
                                     ei.ImagePath = newImageUrl;
                                     db.Entry(ei).State = EntityState.Modified;
                                     db.SaveChanges();
                                     success = true;
                                 }
                                 break;
                        case ImageType.SupperClub:
                                 SupperClubImage sci = (from x in db.SupperClubImages
                                                  where x.ImagePath.ToLower() == oldImageUrl.ToLower()
                                                  select x).FirstOrDefault();
                                 if (sci != null)
                                 {
                                     sci.ImagePath = newImageUrl;
                                     db.Entry(sci).State = EntityState.Modified;
                                     db.SaveChanges();
                                     success = true;
                                 }
                                 break;
                    }
                    
                    return success;
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message + ex.StackTrace, ex);
            }
            return success;
        }

        public int CloneEvent(int EventId, DateTime Start, DateTime End, decimal Commission)
        {
            int newEventId = 0;
            DbConnection conn = db.Database.Connection;
            ConnectionState initialState = conn.State;
            try
            {
                if (initialState != ConnectionState.Open)
                    conn.Open();
                using (DbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.CommandText = "CloneEvent";
                    cmd.Parameters.Add(new SqlParameter("@EventId", EventId));
                    cmd.Parameters.Add(new SqlParameter("@Start", Start));
                    cmd.Parameters.Add(new SqlParameter("@End", End));
                    cmd.Parameters.Add(new SqlParameter("@Commission", Commission));
                    object obj = cmd.ExecuteScalar();
                    if (obj != null)
                    {
                        newEventId = int.Parse(obj.ToString());
                    }                    
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);                
            }
            finally
            {
                if (initialState != ConnectionState.Open)
                    conn.Close();
            }
            return newEventId;
        }

        #region Manage Events

        // Add / Remove Guests
        public bool AddUserToAnEvent(Guid userId, int eventId, int seatingId, int menuOptionId, int numberOfGuests, decimal totalBasePrice, decimal totalPrice, decimal? hostNetPrice = null, decimal? GuestBasePrice = null, decimal discount = 0, int? voucherId = 0, Guid? basketId = null, bool? AdminVoucherCode = null)
        {
            try
            {
                hostNetPrice = hostNetPrice < 0 ? 0 : hostNetPrice;
                totalPrice = totalPrice < 0 ? 0 : totalPrice;
                using (var db = new SupperClubContext())
                {
                    EventAttendee eventAttendee = new EventAttendee();
                    eventAttendee.EventId = eventId;
                    eventAttendee.UserId = userId;
                    eventAttendee.SeatingId = seatingId;
                    eventAttendee.MenuOptionId = menuOptionId;
                    eventAttendee.NumberOfGuests = numberOfGuests;
                    eventAttendee.TotalBasePrice = totalBasePrice;
                    eventAttendee.TotalPrice = totalPrice;
                    eventAttendee.BookingDate = DateTime.Now;
                    eventAttendee.HostNetPrice = hostNetPrice;
                    eventAttendee.GuestBasePrice = GuestBasePrice;
                    eventAttendee.Discount = discount;
                    eventAttendee.AdminVoucherCode = AdminVoucherCode;
                    eventAttendee.VoucherId = voucherId;
                    eventAttendee.BasketId = basketId;
                    db.EventAttendees.Add(eventAttendee);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return false;
        }

        public bool RemoveUserFromAnEvent(Guid userId, int eventId, int seatingId, int menuOptionId, Guid AdminUserId)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    List<EventAttendee> eventAttendees = (from x in db.EventAttendees
                                                          where x.UserId == userId && x.EventId == eventId && x.SeatingId == seatingId && x.MenuOptionId == menuOptionId
                                                          select x).ToList<EventAttendee>();
                    if (eventAttendees != null && eventAttendees.Count > 0)
                    {
                        List<BookingCancellationLog> lstBookingCancellationLog = new List<BookingCancellationLog>();
                        if (eventAttendees.First().BasketId != null)
                        {
                            lstBookingCancellationLog = (from x in eventAttendees
                                                         join tb in db.TicketBaskets on x.BasketId equals tb.Id
                                                         select new BookingCancellationLog
                                                         {
                                                             AdminUserId = AdminUserId,
                                                             EventId = x.EventId,
                                                             UserId = x.UserId,
                                                             SeatingId = x.SeatingId,
                                                             MenuOptionId = x.MenuOptionId,
                                                             VoucherId = (int)x.VoucherId,
                                                             BookingReference = tb.BookingReference,
                                                             NumberOfGuests = x.NumberOfGuests,
                                                             BookingDateTime = x.BookingDate,
                                                             CancelDateTime = DateTime.Now,
                                                             GuestBasePrice = x.GuestBasePrice,
                                                             TotalPrice = x.TotalPrice,
                                                             TotalBasePrice = x.TotalBasePrice,
                                                             HostNetPrice = x.HostNetPrice,
                                                             Discount = x.Discount
                                                         }).Distinct().ToList<BookingCancellationLog>();
                        }
                        else
                        {
                            var tickets = (from x in db.Tickets
                                           join tb in db.TicketBaskets on x.BasketId equals tb.Id
                                           where x.UserId == userId && x.EventId == eventId && x.SeatingId == seatingId && x.MenuOptionId == menuOptionId
                                           group tb by new
                                           {
                                               UserId = x.UserId,
                                               EventId = x.EventId,
                                               SeatingId = x.SeatingId,
                                               MenuOptionId = x.MenuOptionId,
                                               VoucherId = x.VoucherId
                                           }
                                               into gr
                                               select new { gr.Key.UserId, gr.Key.EventId, gr.Key.SeatingId, gr.Key.MenuOptionId, gr.Key.VoucherId, BookingReference = gr.Min(tb => tb.BookingReference) }).Take(1).ToList();


                            lstBookingCancellationLog = (from x in eventAttendees
                                                         join t in tickets on new { x.EventId, x.UserId, x.SeatingId, x.MenuOptionId } equals new { t.EventId, t.UserId, t.SeatingId, t.MenuOptionId }
                                                         select new BookingCancellationLog
                                                         {
                                                             AdminUserId = AdminUserId,
                                                             EventId = x.EventId,
                                                             UserId = x.UserId,
                                                             SeatingId = x.SeatingId,
                                                             MenuOptionId = x.MenuOptionId,
                                                             VoucherId = t.VoucherId,
                                                             BookingReference = t.BookingReference,
                                                             NumberOfGuests = x.NumberOfGuests,
                                                             BookingDateTime = x.BookingDate,
                                                             CancelDateTime = DateTime.Now,
                                                             GuestBasePrice = x.GuestBasePrice,
                                                             TotalPrice = x.TotalPrice,
                                                             TotalBasePrice = x.TotalBasePrice,
                                                             HostNetPrice = x.HostNetPrice,
                                                             Discount = x.Discount
                                                         }).Distinct().ToList<BookingCancellationLog>();

                        }
                        if (lstBookingCancellationLog != null && lstBookingCancellationLog.Count > 0)
                        {
                            foreach (BookingCancellationLog bcl in lstBookingCancellationLog)
                            {
                                db.BookingCancellationLogs.Add(bcl);
                            }
                            db.SaveChanges();
                        }

                        foreach (EventAttendee ea in eventAttendees)
                        {
                            db.EventAttendees.Remove(ea);
                        }
                        db.SaveChanges();
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return false;
        }

        public bool RemoveUserFromAnEvent(Guid userId, int eventId, int seatingId)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    EventAttendee eventAttendee = (from x in db.EventAttendees
                                                   where x.UserId == userId && x.EventId == eventId && x.SeatingId == seatingId
                                                   select x).SingleOrDefault<EventAttendee>();
                    if (eventAttendee != null)
                    {
                        db.EventAttendees.Remove(eventAttendee);
                        db.SaveChanges();
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return false;
        }

        public bool RemoveUserFromAnEvent(Guid userId, int eventId)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    EventAttendee eventAttendee = (from x in db.EventAttendees
                                                   where x.UserId == userId && x.EventId == eventId
                                                   select x).SingleOrDefault<EventAttendee>();
                    if (eventAttendee != null)
                    {
                        db.EventAttendees.Remove(eventAttendee);
                        db.SaveChanges();
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return false;
        }
        // Create / Update
        public Event CreateEvent(Event _event)
        {

            try
            {
                using (var db = new SupperClubContext())
                {
                    log.Debug("Started to create event in database");
                    _event.DateCreated = DateTime.Now;
                    db.Events.Add(_event);
                    db.SaveChanges();
                    log.Debug("Successfully created event in database");
                    try
                    {
                        log.Debug("Started saving other event details in database");
                        //Once event created with Id
                        UpdateMenuCuisinesDiets(_event);
                        db.SaveChanges();
                        log.Debug("Successfully saved other event details in database");
                    }
                    catch (Exception ex)
                    {
                        log.Debug("Error saving other event details in database");
                        log.Fatal("Error UpdateMenuCuisinesDiets. " + ex.Message + ex.StackTrace + ex.InnerException != null ? ex.InnerException.Message + ex.InnerException.StackTrace : "", ex);
                    }
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message + ex.StackTrace + ex.InnerException != null ? ex.InnerException.Message + ex.InnerException.StackTrace: "", ex);
            }
            return _event;
        }

        public bool UpdateEvent(Event _event, Guid userId)
        {
            try
            {
                var db = new SupperClubContext();
                {
                    //Update the event table
                    db.Entry(_event).State = EntityState.Modified;

                    //Update other tables
                    UpdateMenuCuisinesDiets(_event, userId);
                    db.SaveChanges();

                    return true;
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message + ex.StackTrace, ex);
            }
            return false;
        }

        public bool LogEventPriceChange(EventPriceChangeLog _eventPriceChangeLog)
        {
            bool success = false;
            try
            {
                using (var db = new SupperClubContext())
                {
                    _eventPriceChangeLog.Date = DateTime.Now;
                    db.EventPriceChangeLogs.Add(_eventPriceChangeLog);
                    db.SaveChanges();
                    success = true;
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return success;
        }

        public bool LogEventCommissionChange(EventCommissionChangeLog _eventCommissionChangeLog)
        {
            bool success = false;
            try
            {
                using (var db = new SupperClubContext())
                {
                    _eventCommissionChangeLog.Date = DateTime.Now;
                    db.EventCommissionChangeLogs.Add(_eventCommissionChangeLog);
                    db.SaveChanges();
                    success = true;
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return success;
        }

        public bool CancelEvent(int eventId)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    Event e = db.Events.FirstOrDefault(x => x.Id == eventId);
                    if (e == null)
                        return false;

                    e.Status = (int)SupperClub.Domain.EventStatus.Cancelled;
                    db.Entry(e).State = EntityState.Modified;
                    db.SaveChanges();
                    return true;
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return false;
        }
        public bool UpdateEventStatus(Event _event)
        {
            try
            {
                var e = new Event { Id = _event.Id, Status = _event.Status, DateApproved = _event.DateApproved };
                using (var db = new SupperClubContext())
                {
                    log.Debug("UpdateEventStatus: EventId=" + _event.Id.ToString() + " ApprovalDate=" + (_event.DateApproved == null ? "null" : _event.DateApproved.ToString()));
                    db.Events.Attach(e);
                    db.Entry(e).Property(x => x.Status).IsModified = true;
                    db.Entry(e).Property(x => x.DateApproved).IsModified = true;
                    if (db.Entry(e).Property(x => x.Status).GetValidationErrors().Count == 0 && db.Entry(e).Property(x => x.DateApproved).GetValidationErrors().Count == 0)
                        db.Configuration.ValidateOnSaveEnabled = false;
                    db.SaveChanges();
                }
                return true;
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return false;
        }
        //public bool AddEventToNewlyCreatedEvent(int eventId)
        //{
        //    try
        //    {
        //        using (var db = new SupperClubContext())
        //        {
        //            NewlyCreatedEvent _newlyCreatedEvent = new NewlyCreatedEvent();
        //            _newlyCreatedEvent.EventId = eventId;
        //            _newlyCreatedEvent.DateAdded = DateTime.Now;
        //            db.NewlyCreatedEvents.Add(_newlyCreatedEvent);
        //            db.SaveChanges();
        //            return true;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.Fatal(ex.Message, ex);
        //    }
        //    return false;
        //}
        public bool UpdateEventSeating(EventSeating _eventSeating)
        {
            try
            {
                var db = new SupperClubContext();
                {
                    //Update the event table
                    db.Entry(_eventSeating).State = EntityState.Modified;
                    db.SaveChanges();

                    return true;
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message + ex.StackTrace, ex);
            }
            return false;
        }
        public bool UpdateSupperClubImages(SupperClub.Domain.SupperClub _supperClub)
        {
            bool success = false;
            //List<SupperClubImage> imageItems = db.SupperClubImages.Where(x => x.SupperClubId == _supperClub.Id).ToList();
            //if (imageItems != null)
            //{
            //    foreach (SupperClubImage ei in imageItems)
            //    {
            //        db.SupperClubImages.Remove(ei);
            //    }
            //}
            List<SupperClubProfileTag> tagItems = db.SupperClubProfileTags.Where(x => x.SupperClubId == _supperClub.Id).ToList();
            if (tagItems != null)
            {
                foreach (SupperClubProfileTag st in tagItems)
                {
                    db.SupperClubProfileTags.Remove(st);
                }
            }

            db.SaveChanges();

            //Separators
            string[] stringSeparators = new string[] { "," };

            try
            {
                #region Images
                //Images
                try
                {
                    if (!string.IsNullOrEmpty(_supperClub.ImagePaths))
                    {
                        string[] images = _supperClub.ImagePaths.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                        var imagesToBeDeleted = from x in db.SupperClubImages
                                                where x.SupperClubId == _supperClub.Id && (!images.Any(val => x.ImagePath.Contains(val)))
                                                select x;
                        var existingImages = from x in db.SupperClubImages
                                             where x.SupperClubId == _supperClub.Id
                                             select x.ImagePath;
                        IEnumerable<string> newImages = existingImages == null ? images : images.Except(existingImages);

                        if (imagesToBeDeleted != null)
                        {
                            foreach (SupperClubImage t in imagesToBeDeleted)
                            {
                                db.SupperClubImages.Remove(t);
                            }
                            db.SaveChanges();
                        }

                        if (newImages != null)
                        {
                            foreach (string t in newImages)
                            {
                                string imageUrl = t.Trim();
                                SupperClubImage si = new SupperClubImage();
                                si.SupperClubId = _supperClub.Id;
                                si.ImagePath = imageUrl;
                                db.SupperClubImages.Add(si);
                            }
                            db.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Could not add/update supperclub image details for EventId=" + _supperClub.Id.ToString() + ". ErrorDetails: " + ex.Message + ex.StackTrace);
                }
                #endregion

                #region Tags
                if (!string.IsNullOrEmpty(_supperClub.SupperClubProfileTagList))
                {
                    string[] tags = _supperClub.SupperClubProfileTagList.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < tags.Length; i++)
                    {
                        int tagId = int.Parse(tags[i].ToString().Trim());
                        SupperClubProfileTag stag = new SupperClubProfileTag();
                        stag.SupperClubId = _supperClub.Id;
                        stag.ProfileTagId = tagId;
                        stag.DateCreated = DateTime.Now;
                        db.SupperClubProfileTags.Add(stag);
                    }
                }
                db.SaveChanges();
                #endregion

                success = true;
            }
            catch (Exception ex)
            {
                log.Error("Could not update supperclub tag details for SupperClubId=" + _supperClub.Id.ToString() + ". ErrorDetails: " + ex.Message + ex.StackTrace);
            }
            return success;
        }

        public bool UpdateSupperClubProfileTags(int Id, string ProfileTags)
        {

            bool success = false;
            try
            {
                List<SupperClubProfileTag> tagItems = db.SupperClubProfileTags.Where(x => x.SupperClubId == Id).ToList();
                if (tagItems != null)
                {
                    foreach (SupperClubProfileTag st in tagItems)
                    {
                        db.SupperClubProfileTags.Remove(st);
                    }
                }

                db.SaveChanges();

                string[] stringSeparators = new string[] { "," };

                if (!string.IsNullOrEmpty(ProfileTags))
                {
                    string[] tags = ProfileTags.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < tags.Length; i++)
                    {
                        int tagId = int.Parse(tags[i].ToString().Trim());
                        SupperClubProfileTag stag = new SupperClubProfileTag();
                        stag.SupperClubId = Id;
                        stag.ProfileTagId = tagId;
                        stag.DateCreated = DateTime.Now;
                        db.SupperClubProfileTags.Add(stag);
                    }
                }
                db.SaveChanges();

                success = true;
            }
            catch (Exception ex)
            {
                log.Error("Could not update supperclub profile tags details for supperclubid=" + Id.ToString() + ". ErrorDetails: " + ex.Message + ex.StackTrace);
            }
            return success;
        }

        public bool UpdateEventMenuOption(EventMenuOption _eventMenuOption)
        {
            try
            {
                var db = new SupperClubContext();
                {
                    //Update the event table
                    db.Entry(_eventMenuOption).State = EntityState.Modified;
                    db.SaveChanges();

                    return true;
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message + ex.StackTrace, ex);
            }
            return false;
        }

        private void UpdateMenuCuisinesDiets(Event _event, Guid? userId = null)
        {
            using (var db = new SupperClubContext())
            {
                try
                {
                    #region Delete existing records
                    //Drop existing Menu / Cuisine / Diet
                    List<Menu> menuItems = db.Menus.Where(x => x.EventId == _event.Id).ToList();
                    if (menuItems != null)
                    {
                        foreach (Menu m in menuItems)
                        {
                            db.Menus.Remove(m);
                        }
                    }

                    List<EventDiet> dietItems = db.EventDiets.Where(x => x.EventId == _event.Id).ToList();
                    if (dietItems != null)
                    {
                        foreach (EventDiet d in dietItems)
                        {
                            db.EventDiets.Remove(d);
                        }
                    }

                    List<EventCuisine> cuisineItems = db.EventCuisines.Where(x => x.EventId == _event.Id).ToList();
                    if (cuisineItems != null)
                    {
                        foreach (EventCuisine c in cuisineItems)
                        {
                            db.EventCuisines.Remove(c);
                        }
                    }

                    List<EventTag> tagItems = db.EventTags.Where(x => x.EventId == _event.Id).ToList();
                    if (tagItems != null)
                    {
                        for (int i = 0; i < tagItems.Count; i++)
                        {
                            db.EventTags.Remove(tagItems[i]);
                        }
                    }

                    List<EventCity> cityItems = db.EventCities.Where(x => x.EventId == _event.Id).ToList();
                    if (cityItems != null)
                    {
                        for (int i = 0; i < cityItems.Count; i++)
                        {
                            db.EventCities.Remove(cityItems[i]);
                        }
                    }

                    List<EventArea> areaItems = db.EventAreas.Where(x => x.EventId == _event.Id).ToList();
                    if (areaItems != null)
                    {
                        for (int i = 0; i < areaItems.Count; i++)
                        {
                            db.EventAreas.Remove(areaItems[i]);
                        }
                    }

                    //List<EventImage> imageItems = db.EventImages.Where(x => x.EventId == _event.Id).ToList();
                    //if (imageItems != null)
                    //{
                    //    foreach (EventImage t in imageItems)
                    //    {
                    //        db.EventImages.Remove(t);
                    //    }
                    //}
                    //db.SaveChanges();
                }
                catch (Exception ex)
                {
                    log.Error("Error deleting the existing items in DB for event update. Event Id=" + _event.Id.ToString() + "  Error Message=" + ex.Message + "  Stack Trace=" + ex.StackTrace);
                }
                    #endregion



                //Menu Items
                if (!string.IsNullOrEmpty(_event.Menu))
                {
                    Menu menu = new Menu();
                    menu.EventId = _event.Id;
                    menu.MenuItem = _event.Menu;
                    db.Menus.Add(menu);
                    db.SaveChanges();
                    //_event.Menus.Add(menu); this is done automatically by injection I guess
                }

                //Separators
                string[] stringSeparators = new string[] { "," };
                string[] fieldSeparators = new string[] { "|" };
                string[] dietSeparators = new string[] { "%" };

                #region Seatings
                //Event Seatings
                if (_event.MultiSeating && !string.IsNullOrEmpty(_event.Seatings))
                {
                    //Separators
                    string[] timeSeparators = new string[] { ":" };
                    string[] tags = _event.Seatings.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < tags.Length; i++)
                    {
                        string[] fields = tags[i].Split(fieldSeparators, StringSplitOptions.None);
                        EventSeating eventSeating = new EventSeating();
                        eventSeating.EventId = _event.Id;
                        string[] aStartTime = fields[0].Split(timeSeparators, StringSplitOptions.None);
                        eventSeating.Start = new DateTime(_event.Start.Year, _event.Start.Month, _event.Start.Day, int.Parse(aStartTime[0]), int.Parse(aStartTime[1]), 0);
                        eventSeating.End = eventSeating.Start.AddHours(_event.Duration);
                        eventSeating.Guests = int.Parse(fields[1]);
                        eventSeating.ReservedSeats = (fields[2] == "" ? 0 : int.Parse(fields[2]));
                        eventSeating.IsDefault = (fields[3] == "0" ? false : true);
                        eventSeating.Id = (fields[4] == "" ? 0 : int.Parse(fields[4]));
                        if (eventSeating.Id > 0)
                        {
                            EventSeating _eventSeating = GetEventSeating(eventSeating.Id);
                            if (_eventSeating != null && (eventSeating.Start != _eventSeating.Start || eventSeating.End != _eventSeating.End || eventSeating.Guests != _eventSeating.Guests || eventSeating.ReservedSeats != _eventSeating.ReservedSeats || eventSeating.IsDefault != _eventSeating.IsDefault))
                            {
                                try
                                {
                                    EventSeating es = db.EventSeatings.Where(x => x.Id == eventSeating.Id).FirstOrDefault();
                                    es.Start = eventSeating.Start;
                                    es.End = eventSeating.End;
                                    es.Guests = eventSeating.Guests;
                                    es.ReservedSeats = eventSeating.ReservedSeats;
                                    es.IsDefault = eventSeating.IsDefault;
                                    db.Entry(es).State = EntityState.Modified;
                                    db.SaveChanges();
                                }
                                catch (DbEntityValidationException dbEx)
                                {
                                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                                    {
                                        foreach (var validationError in validationErrors.ValidationErrors)
                                        {
                                            log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.Error("Could not update event seating details for EventId=" + eventSeating.EventId.ToString() + "and SeatingId=" + _eventSeating.Id.ToString() + "  Error Message=" + ex.Message + "  Stack Trace=" + ex.StackTrace);
                                }
                            }

                        }
                        else
                        {
                            eventSeating.DateCreated = DateTime.Now;
                            db.EventSeatings.Add(eventSeating);
                            db.SaveChanges();
                        }
                    }
                }
                #endregion

                #region Menu Options
                //Event Menu Options
                if (_event.MultiMenuOption && !string.IsNullOrEmpty(_event.MenuOptions))
                {
                    //Separators
                    string[] tags = _event.MenuOptions.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < tags.Length; i++)
                    {
                        string[] fields = tags[i].Split(fieldSeparators, StringSplitOptions.None);
                        EventMenuOption eventMenuOption = new EventMenuOption();
                        eventMenuOption.EventId = _event.Id;
                        eventMenuOption.Title = Microsoft.JScript.GlobalObject.unescape(fields[0]);
                        eventMenuOption.Description = Microsoft.JScript.GlobalObject.unescape(fields[1]);
                        eventMenuOption.Cost = decimal.Parse(fields[2]);
                        eventMenuOption.IsDefault = (fields[3] == "0" ? false : true);
                        eventMenuOption.Id = (fields[4] == "" ? 0 : int.Parse(fields[4]));

                        if (eventMenuOption.Id > 0)
                        {
                            EventMenuOption _eventMenuOption = GetEventMenuOption(eventMenuOption.Id);
                            if (_eventMenuOption != null && (eventMenuOption.Title != _eventMenuOption.Title || eventMenuOption.Description != _eventMenuOption.Description || eventMenuOption.Cost != _eventMenuOption.Cost || eventMenuOption.IsDefault != _eventMenuOption.IsDefault))
                            {
                                try
                                {

                                    EventMenuOption emo = db.EventMenuOptions.Where(x => x.Id == eventMenuOption.Id).FirstOrDefault();
                                    emo.Title = eventMenuOption.Title;
                                    emo.Description = eventMenuOption.Description;
                                    emo.Cost = eventMenuOption.Cost;
                                    emo.IsDefault = eventMenuOption.IsDefault;
                                    db.Entry(emo).State = EntityState.Modified;
                                    db.SaveChanges();
                                    if (eventMenuOption.Cost != _eventMenuOption.Cost)
                                    {
                                        log.Debug("Logging Event Menu Price Change. EventId=" + eventMenuOption.EventId.ToString() + " UserId=" + userId.ToString());
                                        //update
                                        EventPriceChangeLog epcl = new EventPriceChangeLog();
                                        epcl.UserId = (Guid)userId;
                                        epcl.EventId = eventMenuOption.EventId;
                                        epcl.MenuOptionId = eventMenuOption.Id;
                                        epcl.OldPrice = _eventMenuOption.Cost;
                                        epcl.NewPrice = emo.Cost;
                                        epcl.Date = DateTime.Now;
                                        bool result = LogEventPriceChange(epcl);
                                        if (result)
                                            log.Debug("Logged Event Price Change. EventId=" + eventMenuOption.EventId.ToString() + " UserId=" + userId.ToString());
                                        else
                                            log.Error("Logging Failed for Event Price Change. EventId=" + eventMenuOption.EventId.ToString() + " UserId=" + userId.ToString());
                                    }
                                }
                                catch (DbEntityValidationException dbEx)
                                {
                                    foreach (var validationErrors in dbEx.EntityValidationErrors)
                                    {
                                        foreach (var validationError in validationErrors.ValidationErrors)
                                        {
                                            log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    log.Error("Could not update event menu option details for EventId=" + _eventMenuOption.EventId.ToString() + "and SeatingId=" + _eventMenuOption.Id.ToString() + "  Error Message=" + ex.Message + "  Stack Trace=" + ex.StackTrace);
                                }
                            }
                        }
                        else
                        {
                            eventMenuOption.DateCreated = DateTime.Now;
                            db.EventMenuOptions.Add(eventMenuOption);
                            db.SaveChanges();
                        }
                    }
                }
                #endregion

                #region Cuisine
                //Cuisines
                List<int> tagIdsAdded = new List<int>();
                if (!string.IsNullOrEmpty(_event.Cuisines))
                {
                    string[] tags = _event.Cuisines.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < tags.Length; i++)
                    {
                        Cuisine cuisine = new Cuisine();
                        cuisine.Name = tags[i].Trim();
                        var tag = (from x in db.Cuisines
                                   where x.Active == true && x.Name.ToLower() == cuisine.Name.ToLower()
                                   select x).FirstOrDefault<Cuisine>();

                        if (tag != null)
                        {
                            EventCuisine ec = new EventCuisine();
                            ec.CuisineId = tag.Id;
                            ec.EventId = _event.Id;
                            _event.EventCuisines.Add(ec);

                            if (tag.TagId != null && tag.TagId > 0)
                            {
                                EventTag et = new EventTag();
                                et.EventId = _event.Id;
                                et.TagId = (int)tag.TagId;
                                _event.EventTags.Add(et);
                                tagIdsAdded.Add(et.TagId);
                            }
                        }
                        else
                        {
                            var tagExist = (from x in db.Tags
                                            where x.Name.ToLower() == cuisine.Name.ToLower()
                                            select x).FirstOrDefault<Tag>();

                            Cuisine c = new Cuisine();
                            c.Name = cuisine.Name;
                            c.Active = true;
                            if (tagExist != null)
                                c.TagId = tagExist.Id;
                            db.Cuisines.Add(c);
                            db.SaveChanges();

                            EventCuisine ec = new EventCuisine();
                            ec.CuisineId = c.Id;
                            ec.EventId = _event.Id;
                            _event.EventCuisines.Add(ec);

                            if (tagExist != null)
                            {
                                EventTag et = new EventTag();
                                et.EventId = _event.Id;
                                et.TagId = (int)tag.TagId;
                                _event.EventTags.Add(et);
                                tagIdsAdded.Add(et.TagId);
                            }
                        }
                        db.SaveChanges();
                    }
                }
                #endregion

                #region Diet
                //Diets 
                if (!string.IsNullOrEmpty(_event.Diets))
                {
                    string[] fields = _event.Diets.Split(dietSeparators, StringSplitOptions.RemoveEmptyEntries);
                    for (int j = 0; j < fields.Length; j++)
                    {
                        string[] tags = fields[j].Split(fieldSeparators, StringSplitOptions.None);
                        Diet diet = new Diet();
                        diet.Name = tags[0].Trim();
                        diet.DietTypeId = int.Parse(tags[1].Trim());
                        var tag = (from x in db.Diets
                                   where x.Name.ToLower() == diet.Name.ToLower()
                                   select x).FirstOrDefault<Diet>();
                        if (tag != null)
                        {
                            EventDiet ed = new EventDiet();
                            ed.DietId = tag.Id;
                            ed.EventId = _event.Id;
                            _event.EventDiets.Add(ed);
                            //hack for saving vegan tag if vegan diet is selected
                            if (tag.Id == 2)
                            {
                                var veganTag = (from x in db.EventTags
                                                where x.TagId == 45 && x.EventId == _event.Id
                                                select x).FirstOrDefault<EventTag>();
                                if (veganTag == null)
                                {
                                    EventTag et = new EventTag();
                                    et.TagId = 45;
                                    ed.EventId = _event.Id;
                                    _event.EventTags.Add(et);
                                }
                            }
                            //hack for saving vegetarian tag if vegetarian diet is selected
                            if (tag.Id == 1)
                            {
                                var vegetarianTag = (from x in db.EventTags
                                                     where x.TagId == 44 && x.EventId == _event.Id
                                                     select x).FirstOrDefault<EventTag>();
                                if (vegetarianTag == null)
                                {
                                    EventTag et = new EventTag();
                                    et.TagId = 44;
                                    ed.EventId = _event.Id;
                                    _event.EventTags.Add(et);
                                }
                            }
                        }
                        else
                        {
                            Diet d = new Diet();
                            d.Name = diet.Name;
                            d.DietTypeId = diet.DietTypeId;
                            db.Diets.Add(d);
                            db.SaveChanges();

                            EventDiet ed = new EventDiet();
                            ed.DietId = d.Id;
                            ed.EventId = _event.Id;
                            _event.EventDiets.Add(ed);
                        }
                        db.SaveChanges();
                    }
                }
                #endregion

                #region Tags
                //Tags
                try
                {
                    if (!string.IsNullOrEmpty(_event.EventTagList))
                    {
                        string[] tags = _event.EventTagList.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < tags.Length; i++)
                        {
                            int tagId = int.Parse(tags[i].Trim());
                            if (!tagIdsAdded.Contains(tagId))
                            {
                                EventTag et = new EventTag();
                                et.EventId = _event.Id;
                                et.TagId = tagId;
                                db.EventTags.Add(et);
                            }
                        }
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Could not add/update event tags for EventId=" + _event.Id.ToString() + ". ErrorDetails: " + ex.Message + ex.StackTrace);
                }
                #endregion

                #region Cities
                //Cities
                try
                {
                    if (!string.IsNullOrEmpty(_event.EventCityList))
                    {
                        string[] cities = _event.EventCityList.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < cities.Length; i++)
                        {
                            int cityId = int.Parse(cities[i].Trim());
                            EventCity ec = new EventCity();
                            ec.EventId = _event.Id;
                            ec.CityId = cityId;
                            db.EventCities.Add(ec);
                        }
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Could not add/update event cities for EventId=" + _event.Id.ToString() + ". ErrorDetails: " + ex.Message + ex.StackTrace);
                }
                #endregion

                #region Areas
                //Cities
                try
                {
                    if (!string.IsNullOrEmpty(_event.EventAreaList))
                    {
                        string[] areas = _event.EventAreaList.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < areas.Length; i++)
                        {
                            int areaId = int.Parse(areas[i].Trim());
                            EventArea ea = new EventArea();
                            ea.EventId = _event.Id;
                            ea.AreaId = areaId;
                            db.EventAreas.Add(ea);
                        }
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Could not add/update event areas for EventId=" + _event.Id.ToString() + ". ErrorDetails: " + ex.Message + ex.StackTrace);
                }
                #endregion

                #region Images
                //Images
                try
                {
                    if (!string.IsNullOrEmpty(_event.ImagePaths))
                    {
                        string[] images = _event.ImagePaths.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                        var imagesToBeDeleted = from x in db.EventImages
                                           where x.EventId == _event.Id && (!images.Any(val => x.ImagePath.Contains(val)))
                                           select x;
                        var existingImages = from x in db.EventImages
                                         where x.EventId == _event.Id
                                         select x.ImagePath;
                        IEnumerable<string> newImages = existingImages == null ? images : images.Except(existingImages);

                        if (imagesToBeDeleted != null)
                        {
                            foreach (EventImage t in imagesToBeDeleted)
                            {
                                db.EventImages.Remove(t);
                            }                        
                            db.SaveChanges();                            
                        }

                        if (newImages != null)
                        {
                            foreach (string t in newImages)
                            {
                                string imageUrl = t.Trim();
                                EventImage ei = new EventImage();
                                ei.EventId = _event.Id;
                                ei.ImagePath = imageUrl;
                                db.EventImages.Add(ei);
                            }
                            db.SaveChanges();
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Could not add/update event image details for EventId=" + _event.Id.ToString() + ". ErrorDetails: " + ex.Message + ex.StackTrace);
                }
                #endregion
            }
        }

        private void UpdateSearchCategoryIds(SearchCategory _searchCategory)
        {
            using (var db = new SupperClubContext())
            {
                try
                {
                    //Drop existing Tags from category
                    List<SearchCategoryTag> searchCategoryTag = db.SearchCategoryTags.Where(x => x.SearchCategoryId == _searchCategory.Id).ToList();
                    if (searchCategoryTag != null)
                    {
                        foreach (SearchCategoryTag s in searchCategoryTag)
                        {
                            db.SearchCategoryTags.Remove(s);
                        }
                    }
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    log.Error("Error deleting the existing items in DB for category create/update. SearchCategoryId=" + _searchCategory.Id.ToString() + "  Error Message=" + ex.Message + "  Stack Trace=" + ex.StackTrace);
                }

                //Separators
                string[] stringSeparators = new string[] { "," };


                //Tags
                try
                {
                    if (!string.IsNullOrEmpty(_searchCategory.TagList))
                    {
                        string[] tags = _searchCategory.TagList.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                        for (int i = 0; i < tags.Length; i++)
                        {
                            SearchCategoryTag sct = new SearchCategoryTag();
                            sct.TagId = int.Parse(tags[i].Trim());
                            sct.SearchCategoryId = _searchCategory.Id;
                            db.SearchCategoryTags.Add(sct);
                        }
                        db.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Could not add/update Search Category tags for Search Category Id=" + _searchCategory.Id.ToString() + ". ErrorDetails: " + ex.Message + ex.StackTrace);
                }
            }
        }
        #endregion

        #endregion

        #region Supperclubs

        public IList<SupperClub.Domain.SupperClub> GetAllSupperClubs()
        {
            IList<SupperClub.Domain.SupperClub> supperClubs = null;
            try
            {
                var db = new SupperClubContext();
                {
                    supperClubs = (from x in db.SupperClubs
                                   select x).OrderBy(x => x.Name).ToList<SupperClub.Domain.SupperClub>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return supperClubs;
        }

        public IList<SupperClub.Domain.SupperClubs> GetAllSupperClubDetails(string Keyword)
        {
            //  IList<SupperClub.Domain.SupperClubs> supperClubs = null;
            SearchSupperClub search = new SearchSupperClub();
            search.Name = Keyword;
            IList<SupperClubs> distinctSearchResult = null;
            try
            {
                StoredProc<SearchSupperClub> nearyBy = new StoredProc<SearchSupperClub>(typeof(SupperClubs));
                //var supperclubproc = new StoredProc<SupperClubs>();
                using (var db = new SupperClubContext())
                {

                    ResultsList results1 = db.CallStoredProc<SearchSupperClub>(nearyBy, search);
                    IList<SupperClubs> storedProcResult = results1.ToList<SupperClubs>();

                    distinctSearchResult = (from p in storedProcResult
                                            group p by new
                                            {
                                                p.HasFutureEvents,
                                                p.BrandNew,
                                                p.SupperclubId,
                                                p.FutureEventCount,
                                                p.ReviewCount,
                                                p.Rating,
                                                p.GrubClubName,
                                                p.GrubClubUrlFriendlyName,
                                                p.FirstName,
                                                p.LastName,
                                                p.Summary,
                                                p.Description,
                                                p.ImagePath,
                                                p.FollowChef,

                                            } into grp
                                            select grp.First()).ToList<SupperClubs>();

                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return distinctSearchResult;


        }

        public IList<SupperClub.Domain.SupperClubs> GetAllSupperClubDetails(Guid? UserId)
        {
            //  IList<SupperClub.Domain.SupperClubs> supperClubs = null;
            SearchSupperClub search = new SearchSupperClub();
            search.Name = "";
            search.UserId = UserId;

            IList<SupperClubs> distinctSearchResult = null;
            try
            {
                StoredProc<SearchSupperClub> nearyBy = new StoredProc<SearchSupperClub>(typeof(SupperClubs));
                //var supperclubproc = new StoredProc<SupperClubs>();
                using (var db = new SupperClubContext())
                {
                    ResultsList results1 = db.CallStoredProc<SearchSupperClub>(nearyBy, search);
                    IList<SupperClubs> storedProcResult = results1.ToList<SupperClubs>();

                    distinctSearchResult = (from p in storedProcResult
                                            group p by new
                                            {
                                                p.BrandNew,
                                                p.SupperclubId,
                                                p.ReviewCount,
                                                p.Rating,
                                                p.GrubClubName,
                                                p.GrubClubUrlFriendlyName,
                                                p.FirstName,
                                                p.LastName,
                                                p.Summary,
                                                p.Description,
                                                p.ImagePath,
                                                p.FollowChef,

                                            } into grp
                                            select grp.First()).ToList<SupperClubs>();

                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return distinctSearchResult;
        }

        public IList<SupperClub.Domain.SupperClub> GetAllActiveSupperClubs()
        {
            IList<SupperClub.Domain.SupperClub> supperClubs = null;
            try
            {
                var db = new SupperClubContext();
                {
                    supperClubs = (from x in db.SupperClubs
                                   where x.Active == true
                                   select x).OrderBy(x => x.Name).ToList<SupperClub.Domain.SupperClub>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return supperClubs;
        }

        public IList<SupperClub.Domain.Event> GetAllActiveEvents()
        {
            IList<Event> events = null;
            try
            {
                var db = new SupperClubContext();
                {
                    events = (from x in db.Events
                              where x.Start > DateTime.Now && (x.Status == (int)EventStatus.Active || x.Status == (int)EventStatus.New)
                              select x).OrderBy(x => x.SupperClubId).ToList<Event>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return events;
        }
        public IList<SupperClub.Domain.Event> GetAllApprovedEvents()
        {
            IList<Event> events = null;
            try
            {
                var db = new SupperClubContext();
                {
                    events = (from x in db.Events
                              join s in db.SupperClubs on x.SupperClubId equals s.Id
                              where x.Start > DateTime.Now && (x.Status == (int)EventStatus.Active && x.Private == false && s.Active == true)
                              select x).OrderBy(x => x.SupperClubId).ToList<Event>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return events;
        }
        public IList<SupperClub.Domain.SiteMapEventList> GetAllSEOEventUrls()
        {
            IList<SupperClub.Domain.SiteMapEventList> events = null;
            try
            {
                var db = new SupperClubContext();
                {
                    events = (from x in db.Events
                              join s in db.SupperClubs on x.SupperClubId equals s.Id
                              where x.Status == (int)EventStatus.Active && x.Private == false && s.Active == true
                              select new SiteMapEventList { SupperClubUrlFriendlyName = s.UrlFriendlyName, EventUrlFriendlyName = x.UrlFriendlyName }).Distinct().ToList<SiteMapEventList>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return events;
        }
        public IList<SupperClub.Domain.SupperClub> GetAllInactiveSupperClubs()
        {
            IList<SupperClub.Domain.SupperClub> supperClubs = null;
            try
            {
                var db = new SupperClubContext();
                {
                    supperClubs = (from x in db.SupperClubs
                                   where x.Active == false
                                   select x).ToList<SupperClub.Domain.SupperClub>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return supperClubs;
        }

        public IList<SupperClub.Domain.SearchCategory> GetAllSearchCategories()
        {
            IList<SupperClub.Domain.SearchCategory> searchCategories = null;
            try
            {
                var db = new SupperClubContext();
                {
                    searchCategories = (from x in db.SearchCategories
                                        select x).ToList<SupperClub.Domain.SearchCategory>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return searchCategories;
        }
        public IList<SupperClub.Domain.SiteMapCityCategoryTagList> GetAllSearchCategoryTagCityList()
        {
            IList<SupperClub.Domain.SiteMapCityCategoryTagList> list = null;
            try
            {
                var db = new SupperClubContext();
                {
                    list = (from e in db.Events
                            join s in db.SupperClubs on e.SupperClubId equals s.Id
                            join et in db.EventTags on e.Id equals et.EventId
                            join t in db.Tags on et.TagId equals t.Id
                            join sct in db.SearchCategoryTags on et.TagId equals sct.TagId
                            join sc in db.SearchCategories on sct.SearchCategoryId equals sc.Id
                            join ec in db.EventCities on e.Id equals ec.EventId
                            join c in db.Cities on ec.CityId equals c.Id
                            where e.Start > DateTime.Now && (e.Status == (int)EventStatus.Active && e.Private == false && s.Active == true)
                            //group t by new
                            //{
                            //    categoryName = sc.Name,
                            //    categoryUrlFriendlyName = sc.UrlFriendlyName,
                            //    tagName = t.Name,
                            //    tagUrlFriendlyName = t.UrlFriendlyName,
                            //    cityName = c.Name,
                            //    cityUrlFriendlyName = c.UrlFriendlyName
                            //}
                            //    into gr
                            select new SiteMapCityCategoryTagList
                            {
                                CategoryUrlFriendlyName = sc.UrlFriendlyName,
                                TagUrlFriendlyName = t.UrlFriendlyName,
                                CityUrlFriendlyName = c.UrlFriendlyName
                            }).Distinct().ToList<SiteMapCityCategoryTagList>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return list;
        }

        public IList<SiteMapCityAreaCategoryTagList> GetAllSearchCategoryTagCityAreaList()
        {
            IList<SupperClub.Domain.SiteMapCityAreaCategoryTagList> list = null;
            try
            {
                var db = new SupperClubContext();
                {
                    list = (from e in db.Events
                            join s in db.SupperClubs on e.SupperClubId equals s.Id
                            join et in db.EventTags on e.Id equals et.EventId
                            join t in db.Tags on et.TagId equals t.Id
                            join sct in db.SearchCategoryTags on et.TagId equals sct.TagId
                            join sc in db.SearchCategories on sct.SearchCategoryId equals sc.Id
                            join ec in db.EventCities on e.Id equals ec.EventId
                            join c in db.Cities on ec.CityId equals c.Id
                            join ea in db.EventAreas on e.Id equals ea.EventId
                            join a in db.Areas on ea.AreaId equals a.Id
                            where e.Start > DateTime.Now && (e.Status == (int)EventStatus.Active && e.Private == false && s.Active == true)
                            //group t by new
                            //{
                            //    categoryName = sc.Name,
                            //    categoryUrlFriendlyName = sc.UrlFriendlyName,
                            //    tagName = t.Name,
                            //    tagUrlFriendlyName = t.UrlFriendlyName,
                            //    cityName = c.Name,
                            //    cityUrlFriendlyName = c.UrlFriendlyName,
                            //    areaName = a.Name,
                            //    areaUrlFriendlyName = a.UrlFriendlyName
                            //}
                            //    into gr
                            select new SiteMapCityAreaCategoryTagList
                            {
                                CategoryUrlFriendlyName = sc.UrlFriendlyName,
                                TagUrlFriendlyName = t.UrlFriendlyName,
                                CityUrlFriendlyName = c.UrlFriendlyName,
                                AreaUrlFriendlyName = a.UrlFriendlyName
                            }).Distinct().ToList<SiteMapCityAreaCategoryTagList>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return list;
        }
        public IList<SupperClub.Domain.City> GetAllCities()
        {
            IList<SupperClub.Domain.City> cities = null;
            try
            {
                var db = new SupperClubContext();
                {
                    cities = (from x in db.Cities
                              join ec in db.EventCities on x.Id equals ec.CityId
                              join e in db.Events on ec.EventId equals e.Id
                              join s in db.SupperClubs on e.SupperClubId equals s.Id
                              where e.Status == (int)EventStatus.Active && e.Start > DateTime.Now && e.Private == false && s.Active == true
                              select x).Distinct().ToList<SupperClub.Domain.City>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return cities;
        }
        public IList<SupperClub.Domain.Area> GetAllAreas()
        {
            IList<SupperClub.Domain.Area> areas = null;
            try
            {
                var db = new SupperClubContext();
                {
                    areas = (from x in db.Areas
                             join ea in db.EventAreas on x.Id equals ea.AreaId
                             join e in db.Events on ea.EventId equals e.Id
                             join s in db.SupperClubs on e.SupperClubId equals s.Id
                             where e.Status == (int)EventStatus.Active && e.Start > DateTime.Now && e.Private == false && s.Active == true
                             select x).Distinct().ToList<SupperClub.Domain.Area>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return areas;
        }
        public IList<SupperClub.Domain.Tag> GetAllTagsWithoutCategory()
        {
            IList<SupperClub.Domain.Tag> tags = null;
            try
            {
                var db = new SupperClubContext();
                {
                    var _tags = (from x in db.Tags
                                 where !((from y in db.SearchCategoryTags
                                          select y.TagId).Distinct())
                                         .Contains(x.Id)
                                 select x).ToList();
                    if (_tags != null && _tags.Count > 0)
                    {
                        tags = (from x in _tags
                                join et in db.EventTags on x.Id equals et.TagId
                                join e in db.Events on et.EventId equals e.Id
                                join s in db.SupperClubs on e.SupperClubId equals s.Id
                                where e.Status == (int)EventStatus.Active && e.Start > DateTime.Now && e.Private == false && s.Active == true
                                select x).Distinct().ToList<Tag>();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return tags;
        }

        public IList<SupperClub.Domain.PopularEvent> GetAllActivePopularEvents()
        {
            IList<SupperClub.Domain.PopularEvent> popularEvents = null;
            try
            {
                var db = new SupperClubContext();
                {
                    popularEvents = (from x in db.PopularEvents
                                     where x.Status != (int)PopularEventStatus.New
                                     select x).OrderBy(c => c.Status).ThenBy(n => n.Rank).ToList<SupperClub.Domain.PopularEvent>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return popularEvents;
        }

        public IList<SupperClub.Domain.PopularEvent> GetActivePopularEventsForEventPage(int eventId)
        {
            IList<SupperClub.Domain.PopularEvent> popularEvents = null;
            try
            {
                var db = new SupperClubContext();
                {
                    popularEvents = ((from x in db.PopularEvents
                                      where x.Status != (int)PopularEventStatus.New && x.EventId != eventId
                                      select x).OrderBy(c => c.Status).ThenBy(n => n.Rank)).Take(4).ToList<SupperClub.Domain.PopularEvent>();

                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return popularEvents;
        }
        public SupperClub.Domain.SupperClub GetSupperClub(int supperClubId)
        {
            SupperClub.Domain.SupperClub _supperClub = null;
            try
            {
                var db = new SupperClubContext();
                {
                    _supperClub = (from x in db.SupperClubs
                                   where x.Id == supperClubId
                                   select x).FirstOrDefault<SupperClub.Domain.SupperClub>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return _supperClub;
        }

        public SupperClub.Domain.SupperClub GetSupperClub(string hostname)
        {
            SupperClub.Domain.SupperClub _supperClub = null;
            try
            {
                var db = new SupperClubContext();
                {
                    _supperClub = (from x in db.SupperClubs
                                   where (x.Name == hostname || x.UrlFriendlyName == hostname)
                                   select x).FirstOrDefault<SupperClub.Domain.SupperClub>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return _supperClub;
        }
        public SupperClub.Domain.SupperClub GetSupperClubForUser(string email)
        {
            SupperClub.Domain.SupperClub _supperClub = null;
            try
            {
                var db = new SupperClubContext();
                {
                    _supperClub = (from x in db.SupperClubs
                                   join y in db.aspnet_Memberships on x.UserId equals y.UserId
                                   where y.LoweredEmail == email.ToLower()
                                   select x).FirstOrDefault<SupperClub.Domain.SupperClub>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return _supperClub;
        }
        public IList<Event> GetAllEventsWithReview(string hostname)
        {
            IList<Event> events = null;
            try
            {
                var db = new SupperClubContext();
                {
                    events = (from x in db.Events
                              join y in db.SupperClubs on x.SupperClubId equals y.Id
                              join z in db.Reviews on x.Id equals z.EventId
                              where y.UrlFriendlyName == hostname && x.Start < DateTime.Now && x.Status == (int)EventStatus.Active && x.Private == false
                              select x).Distinct().OrderByDescending(z => z.DateCreated).ToList<Event>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return events;
        }

        public SupperClub.Domain.SupperClub GetSupperClubForUser(Guid userId)
        {
            SupperClub.Domain.SupperClub _supperClub = null;
            try
            {
                using (var db = new SupperClubContext())
                {
                    _supperClub = db.SupperClubs.AsNoTracking().Include("SupperClubImages").FirstOrDefault(x => x.UserId == userId);
                    
                    //_supperClub = (from x in db.SupperClubs
                    //               where x.UserId == userId
                    //               select x).FirstOrDefault<SupperClub.Domain.SupperClub>();
                }
                return _supperClub;
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return _supperClub;
        }

        public int GetSupperClubFollowers(int supperClubId)
        {
            int followerCount = 0;
            try
            {
                var db = new SupperClubContext();

                var _supperClub = (from x in db.UserFavouriteSupperClubs
                                   where x.SupperClubId == supperClubId
                                   select x).ToList<UserFavouriteSupperClub>();
                if (_supperClub != null)
                    followerCount = _supperClub.Distinct().Count();
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return followerCount;
        }

        public IList<User> GetSupperClubFollowersList(int supperClubId)
        {
            IList<User> usrList = null;
            try
            {
                var db = new SupperClubContext();

                usrList = (from x in db.UserFavouriteSupperClubs
                           join u in db.Users on x.UserId equals u.Id
                           where x.SupperClubId == supperClubId
                           select u).ToList<User>();
                //if (_users != null)
                //    followerCount = _users.Distinct().Count();
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return usrList;
        }
        public int IsExistSupperClub(string supperClubName, Guid userId)
        {
            int exist = 0;
            try
            {
                using (var db = new SupperClubContext())
                {
                    SupperClub.Domain.SupperClub _supperClub = new SupperClub.Domain.SupperClub();
                    _supperClub = (from x in db.SupperClubs
                                   where x.Name.ToLower() == supperClubName.ToLower()
                                   select x).FirstOrDefault<SupperClub.Domain.SupperClub>();
                    if (_supperClub != null)
                        exist = 2;

                    _supperClub = (from x in db.SupperClubs
                                   where x.UserId == userId
                                   select x).FirstOrDefault<SupperClub.Domain.SupperClub>();
                    if (_supperClub != null)
                        exist = 1;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return exist;
        }

        public IList<SupperClub.Domain.SupperClub> GetUserFavouriteSupperClubs(Guid userId)
        {
            IList<SupperClub.Domain.SupperClub> scList = null;
            try
            {
                var db = new SupperClubContext();

                scList = (from x in db.UserFavouriteSupperClubs
                           join s in db.SupperClubs on x.SupperClubId equals s.Id
                           where x.UserId == userId
                           select s).ToList<SupperClub.Domain.SupperClub>();
                //if (_users != null)
                //    followerCount = _users.Distinct().Count();
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return scList;
        }

        public int GetUserFavouriteSupperClubCount(Guid userId)
        {
            int count = 0;
            try
            {
                var db = new SupperClubContext();

                count = (from x in db.UserFavouriteSupperClubs
                          join s in db.SupperClubs on x.SupperClubId equals s.Id
                          where x.UserId == userId
                          select s).Distinct().Count();
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return count;
        }

        #region Manage Supperclubs

        public SupperClub.Domain.SupperClub RegisterSupperClub(SupperClub.Domain.SupperClub supperClub, string email)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    log.Debug("Starting to save SupperClub in database");
                    db.SupperClubs.Add(supperClub);
                    db.SaveChanges();
                    log.Debug("SupperClub created successfully");
                    log.Debug("Adding host role to profile");
                    System.Web.Security.Roles.AddUserToRole(email, "Host");
                    log.Debug("Added host role to profile successfully");
                    //Once Supperclub created with Id
                    try
                    {
                        UpdateSupperClubImages(supperClub);
                        db.SaveChanges();
                    }                    
                    catch (Exception ex)
                    {
                        log.Fatal("Error updating or adding the images to supperclub. " + ex.Message + ex.InnerException.Message + ex.StackTrace, ex);
                    }
                    return supperClub;
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message + ex.StackTrace + ex.InnerException != null ? ex.InnerException.StackTrace : "", ex);
            }
            return null;
        }

        public bool UpdateSupperClub(SupperClub.Domain.SupperClub supperClub)
        {
            try
            {
                var db = new SupperClubContext();
                {
                    //Update the SupperClub table
                    db.SupperClubs.Attach(supperClub);
                    db.Entry<SupperClub.Domain.SupperClub>(supperClub).State = EntityState.Modified;
                    //Update other tables
                    UpdateSupperClubImages(supperClub);
                    db.SaveChanges();

                    return true;
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message + ex.StackTrace + ex.InnerException != null ? ex.InnerException.StackTrace : "", ex);
            }
            return false;
        }

        public bool? FlipActivationForSupperClub(int supperClubId)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    Domain.SupperClub sc = db.SupperClubs.FirstOrDefault(x => x.Id == supperClubId);
                    sc.Active = !sc.Active;
                    db.SaveChanges();
                    return sc.Active;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return null;
        }

        #endregion

        #endregion

        #region User Management

        public bool CreateUser(User user)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    db.Users.Add(user);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }

            return false;
        }

        public bool AddUserDevice(UserDevice userDevice)
        {
            try
            {
                //set all notification true by default
                userDevice.FavChefNewEventNotification = true;
                userDevice.FavEventBookingReminder = true;
                userDevice.FbFriendBookedTicket = true;
                userDevice.FbFriendInstalledApp = true;
                userDevice.WaitlistEventTicketsAvailable = true;

                using (var db = new SupperClubContext())
                {
                    var ud = (from x in db.UserDevices
                              where x.DeviceId == userDevice.DeviceId && x.UserId == userDevice.UserId
                              select x).FirstOrDefault<UserDevice>();
                    if (ud == null)
                    {
                        db.UserDevices.Add(userDevice);
                        db.SaveChanges();
                    }
                    return true;
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }

            return false;
        }

        public bool CheckUserDevice(string deviceId, Guid userId)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    var ud = (from x in db.UserDevices
                              where x.DeviceId == deviceId && x.UserId == userId
                              select x).FirstOrDefault<UserDevice>();
                    if (ud != null)
                        return true;
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }

            return false;
        }
        public User GetUser(Guid? userId)
        {
            try
            {
                var db = new SupperClubContext();
                return db.Users.FirstOrDefault(x => x.Id == userId);
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return null;
        }
        public User GetUserNoTracking(Guid userId)
        {
            try
            {
                var db = new SupperClubContext();
                return db.Users.AsNoTracking().FirstOrDefault(x => x.Id == userId);
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return null;
        }
        public bool UpdateUser(User user)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    var v = db.Users.Find(user.Id);
                    try
                    {
                        db.Users.Attach(user);
                        db.Entry(user).State = EntityState.Modified;
                    }
                    catch
                    {
                        db.Entry(v).CurrentValues.SetValues(user);
                    }
                    db.SaveChanges();
                }
                return true;
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return false;
        }

        public IList<User> GetAllUsers()
        {
            IList<User> users = null;
            try
            {
                var db = new SupperClubContext();
                {
                    users = (from x in db.Users
                             select x).OrderBy(x => x.aspnet_Users.UserName).ToList<User>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return users;
        }

        public bool? FlipUserLock(Guid userId)
        {
            try
            {
                var db = new SupperClubContext();
                {
                    aspnet_Membership a = db.aspnet_Memberships.FirstOrDefault(x => x.UserId == userId);
                    a.IsLockedOut = !a.IsLockedOut;
                    a.FailedPasswordAttemptCount = 0;
                    db.SaveChanges();
                    return a.IsLockedOut;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return null;
        }

        public User GetUserByFbId(string fbId)
        {
            try
            {
                var db = new SupperClubContext();
                var user = (from x in db.Users.AsNoTracking()
                            where x.FacebookId == fbId
                            select x).FirstOrDefault();


                return user;

            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return null;
        }

        public bool RankUser(int eventId, Guid eventAttendeeId, int ranking)
        {
            try
            {
                var db = new SupperClubContext();
                {
                    EventAttendee ea = db.EventAttendees.FirstOrDefault(x => x.UserId == eventAttendeeId && x.EventId == eventId);
                    ea.UserRanking = ranking;
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return false;
        }

        public bool AddEventsToFavourite(List<UserFavouriteEvent> userFavouriteEvents)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    if (userFavouriteEvents != null && userFavouriteEvents.Count > 0)
                    {
                        foreach (UserFavouriteEvent userFavouriteEvent in userFavouriteEvents)
                        {
                            var ufe = (from x in db.UserFavouriteEvents
                                       where x.UserId == userFavouriteEvent.UserId && x.EventId == userFavouriteEvent.EventId
                                       select x).FirstOrDefault<UserFavouriteEvent>();
                            if (ufe == null)
                                db.UserFavouriteEvents.Add(userFavouriteEvent);
                        }
                        db.SaveChanges();
                        return true;
                    }
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }

            return false;
        }

        public bool RemoveEventsFromFavourite(int[] eventIds, Guid userId)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    if (eventIds != null && eventIds.Length > 0)
                    {
                        foreach (int eventId in eventIds)
                        {
                            UserFavouriteEvent ufe = (from x in db.UserFavouriteEvents
                                                      where x.UserId == userId && x.EventId == eventId
                                                      select x).SingleOrDefault<UserFavouriteEvent>();
                            if (ufe != null)
                                db.UserFavouriteEvents.Remove(ufe);
                        }
                        db.SaveChanges();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return false;
        }

        public bool AddEventToFavourite(UserFavouriteEvent userFavouriteEvent)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    if (userFavouriteEvent != null)
                    {
                        var ufe = (from x in db.UserFavouriteEvents
                                   where x.UserId == userFavouriteEvent.UserId && x.EventId == userFavouriteEvent.EventId
                                   select x).FirstOrDefault<UserFavouriteEvent>();
                        if (ufe == null)
                            db.UserFavouriteEvents.Add(userFavouriteEvent);

                        db.SaveChanges();
                        return true;
                    }
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }

            return false;
        }

        public bool RemoveEventFromFavourite(int eventId, Guid userId)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    UserFavouriteEvent ufe = (from x in db.UserFavouriteEvents
                                              where x.UserId == userId && x.EventId == eventId
                                              select x).SingleOrDefault<UserFavouriteEvent>();
                    if (ufe != null)
                        db.UserFavouriteEvents.Remove(ufe);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return false;
        }
        public bool AddSupperClubsToFavourite(List<UserFavouriteSupperClub> userFavouriteSupperClubs)
        {

            try
            {
                using (var db = new SupperClubContext())
                {
                    if (userFavouriteSupperClubs != null && userFavouriteSupperClubs.Count > 0)
                    {
                        foreach (UserFavouriteSupperClub userFavouriteSupperClub in userFavouriteSupperClubs)
                        {
                            var ufs = (from x in db.UserFavouriteSupperClubs
                                       where x.UserId == userFavouriteSupperClub.UserId && x.SupperClubId == userFavouriteSupperClub.SupperClubId
                                       select x).FirstOrDefault<UserFavouriteSupperClub>();
                            if (ufs == null)
                            {
                                userFavouriteSupperClub.CreateDate = DateTime.Now;
                                db.UserFavouriteSupperClubs.Add(userFavouriteSupperClub);
                            }
                        }
                        db.SaveChanges();
                        return true;
                    }
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }

            return false;
        }

        public bool RemoveSupperClubsFromFavourite(int[] supperClubIds, Guid userId)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    if (supperClubIds != null && supperClubIds.Length > 0)
                    {
                        foreach (int supperClubId in supperClubIds)
                        {
                            UserFavouriteSupperClub ufs = (from x in db.UserFavouriteSupperClubs
                                                           where x.UserId == userId && x.SupperClubId == supperClubId
                                                           select x).SingleOrDefault<UserFavouriteSupperClub>();
                            if (ufs != null)
                                db.UserFavouriteSupperClubs.Remove(ufs);
                        }
                        db.SaveChanges();
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return false;
        }

        public bool AddSupperClubToFavourite(UserFavouriteSupperClub userFavouriteSupperClub)
        {

            try
            {
                using (var db = new SupperClubContext())
                {
                    if (userFavouriteSupperClub != null)
                    {
                        var ufs = (from x in db.UserFavouriteSupperClubs
                                   where x.UserId == userFavouriteSupperClub.UserId && x.SupperClubId == userFavouriteSupperClub.SupperClubId
                                   select x).FirstOrDefault<UserFavouriteSupperClub>();
                        if (ufs == null)
                        {
                            userFavouriteSupperClub.CreateDate = DateTime.Now;
                            db.UserFavouriteSupperClubs.Add(userFavouriteSupperClub);
                        }

                        db.SaveChanges();
                        return true;
                    }
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }

            return false;
        }
        public bool RemoveSupperClubFromFavourite(int supperClubId, Guid userId)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    UserFavouriteSupperClub ufs = (from x in db.UserFavouriteSupperClubs
                                                   where x.UserId == userId && x.SupperClubId == supperClubId
                                                   select x).SingleOrDefault<UserFavouriteSupperClub>();
                    if (ufs != null)
                        db.UserFavouriteSupperClubs.Remove(ufs);

                    db.SaveChanges();
                    return true;

                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return false;
        }
        public bool AddUserFacebookFriend(List<UserFacebookFriend> userFacebookFriend)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    if (userFacebookFriend != null && userFacebookFriend.Count > 0)
                    {
                        foreach (UserFacebookFriend uff in userFacebookFriend)
                        {
                            var _friend = (from x in db.UserFacebookFriends
                                           where x.UserId == uff.UserId && x.UserFacebookId == uff.UserFacebookId && x.FriendFacebookId == uff.FriendFacebookId
                                           select x).FirstOrDefault<UserFacebookFriend>();
                            if (_friend == null)
                                db.UserFacebookFriends.Add(uff);
                        }
                    }
                    db.SaveChanges();
                }
                return true;
            }
            catch (Exception ex)
            {
                log.Error("Could not add user's facebook friend. ErrorDetails: " + ex.Message + ex.StackTrace);
            }
            return false;
        }

        public UserDevice GetUserDevice(string deviceToken)
        {
            try
            {
                var db = new SupperClubContext();
                var userDevice = (from x in db.UserDevices.AsNoTracking()
                                  where x.DeviceId == deviceToken
                                  select x).FirstOrDefault();
                return userDevice;
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return null;
        }

        public bool UpdateNotificationState(UserDevice userDevice)
        {
            try
            {
                var db = new SupperClubContext();
                {
                    //db.UserDevices.Attach(userDevice);
                    db.Entry(userDevice).State = EntityState.Modified;
                    db.SaveChanges();
                }
                return true;
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return false;
        }

        public List<User> GetWishListedUsers(int eventId)
        {
            List<User> users = null;
            try
            {
                var db = new SupperClubContext();
                {
                    users = (from x in db.UserFavouriteEvents
                             join y in db.Users on x.UserId equals y.Id
                             join ea in db.EventAttendees on new { x.EventId, x.UserId } equals new { ea.EventId, ea.UserId } into gj
                             from subEventAttendee in gj.DefaultIfEmpty()
                             where x.EventId == eventId && x.EmailNotificationSent == false && subEventAttendee.UserId == null
                             select y).Distinct().ToList<User>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return users;
        }

        public bool IsExistUserId(Guid tempUserId)
        {
            bool isExist = false;
            try
            {
                using (var db = new SupperClubContext())
                {
                    var user = (from x in db.TempUserIdLogs
                                where x.UserId == tempUserId
                                select x).FirstOrDefault();
                    if (user != null)
                        isExist = true;
                    else
                    {
                        var _user = (from x in db.Users.AsNoTracking()
                                     where x.Id == tempUserId
                                     select x).FirstOrDefault();
                        if (_user != null)
                            isExist = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return isExist;
        }        
        public bool AddTempUserId(Guid tempUserId)
        {
            bool status = false;
            try
            {
                using (var db = new SupperClubContext())
                {
                    TempUserIdLog tUserId = new TempUserIdLog();
                    tUserId.UserId = tempUserId;
                    tUserId.DateCreated = DateTime.Now;
                    tUserId = db.TempUserIdLogs.Add(tUserId);
                    db.SaveChanges();
                    if (tUserId.Id > 0)
                        status = true;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }

            return status;
        }
        public bool RemoveTempUserId(Guid tempUserId)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    TempUserIdLog tUserId = (from x in db.TempUserIdLogs
                                              where x.UserId == tempUserId
                                              select x).SingleOrDefault<TempUserIdLog>();
                    if (tUserId != null)
                        db.TempUserIdLogs.Remove(tUserId);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return false;
        }
        public bool IsExistSegmentUserId(Guid tempUserId)
        {
            bool isExist = false;
            try
            {
                using (var db = new SupperClubContext())
                {
                    var user = (from x in db.SegmentUsers
                                where x.SegmentUserId == tempUserId
                                select x).FirstOrDefault();
                    if (user != null)
                        isExist = true;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return isExist;
        }
        public SegmentUser GetSegmentUser(Guid userId)
        {
            SegmentUser _segmentUser = null;
            try
            {
                using (var db = new SupperClubContext())
                {
                    var user = (from x in db.SegmentUsers
                                where x.UserId  == userId
                                select x).FirstOrDefault();
                    if (user != null)
                        _segmentUser  = user;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return _segmentUser;
        }
        public SegmentUser AddSegmentUser(SegmentUser _segmentUser)
        {
            SegmentUser savedSegmentUser = null;
            try
            {
                using (var db = new SupperClubContext())
                {
                    _segmentUser.DateCreated = DateTime.Now;
                    savedSegmentUser = db.SegmentUsers.Add(_segmentUser);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }

            return savedSegmentUser;
        }
        #endregion

        #region Administration

        public EmailTemplate GetEmailTemplate(int templateId)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    EmailTemplate emt = db.EmailTemplates.FirstOrDefault(x => x.Id == templateId);
                    emt.Body = Regex.Replace(emt.Body, @"\t|\n|\r", " ");
                    emt.Body = emt.Body.Trim();
                    return emt;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return null;
        }

        public MessageTemplate GetMessageTemplate(int templateId)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    MessageTemplate mt = db.MessageTemplates.FirstOrDefault(x => x.Id == templateId);
                    mt.MessageBody = Regex.Replace(mt.MessageBody, @"\t|\n|\r", " ");
                    mt.MessageBody = mt.MessageBody.Trim();
                    return mt;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return null;
        }

        public List<Log> GetLog(int days, LogLevel logLevel, string filter)
        {
            try
            {
                var db = new SupperClubContext();
                DateTime startDate = DateTime.Now.AddDays(-days);
                List<Log> logs = db.Logs.Where(x => x.Date > startDate).OrderByDescending(x => x.Date).ToList();

                if (!string.IsNullOrEmpty(filter))
                    logs = logs.Where(x => x.Message.Contains(filter)).ToList();

                switch (logLevel)
                {
                    case LogLevel.DEBUG:
                        // this is everything, nothing to do here
                        break;
                    case LogLevel.INFO:
                        logs = logs.Where(x => x.Level == LogLevel.INFO.ToString() || x.Level == LogLevel.WARN.ToString() || x.Level == LogLevel.ERROR.ToString() || x.Level == LogLevel.FATAL.ToString()).ToList();
                        break;
                    case LogLevel.WARN:
                        logs = logs.Where(x => x.Level == LogLevel.WARN.ToString() || x.Level == LogLevel.ERROR.ToString() || x.Level == LogLevel.FATAL.ToString()).ToList();
                        break;
                    case LogLevel.ERROR:
                        logs = logs.Where(x => x.Level == LogLevel.ERROR.ToString() || x.Level == LogLevel.FATAL.ToString()).ToList();
                        break;
                    case LogLevel.FATAL:
                        logs = logs.Where(x => x.Level == LogLevel.FATAL.ToString()).ToList();
                        break;
                    default:
                        logs = null;
                        break;
                }

                return logs;
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return null;
        }

        public Log GetLogEvent(int logId)
        {
            try
            {
                var db = new SupperClubContext();
                return db.Logs.FirstOrDefault(x => x.ID == logId);
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return null;
        }

        public bool PurgeLog(int olderThanDay)
        {
            try
            {
                var db = new SupperClubContext();
                DateTime olderThanDate = DateTime.Now.AddDays(-olderThanDay);
                var oldLogs = db.Logs.Where(x => x.Date < olderThanDate);
                foreach (Log l in oldLogs)
                    db.Logs.Remove(l);
                db.SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return false;
        }

        public List<Report> GetReportList(int reportTypeEnumId)
        {
            try
            {
                var db = new SupperClubContext();
                return db.Reports.Where(x => x.ReportType == reportTypeEnumId).ToList<Report>();
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return null;
        }

        public Report GetReport(int reportId)
        {
            try
            {
                var db = new SupperClubContext();
                return db.Reports.FirstOrDefault(x => x.Id == reportId);
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return null;
        }

        public DataTable RunReportQuery(string sql, List<Tuple<string, string>> parameters)
        {
            try
            {
                return ExecuteSqlQuery(sql, parameters, 120);
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return null;
        }

        static DataTable ExecuteSqlQuery(string sql, List<Tuple<string, string>> parameters, int timeOut = 0)
        {
            DbConnection conn = db.Database.Connection;
            ConnectionState initialState = conn.State;
            try
            {
                if (initialState != ConnectionState.Open)
                    conn.Open();
                using (DbCommand cmd = conn.CreateCommand())
                {
                    if (sql.StartsWith("EXEC"))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        sql = sql.Replace("EXEC", "").Trim();
                    }
                    cmd.CommandText = sql;
                    if (timeOut > 0)
                        cmd.CommandTimeout = timeOut;
                    if (parameters != null && parameters.Count > 0)
                    {
                        foreach (Tuple<string, string> p in parameters)
                        {
                            cmd.Parameters.Add(new SqlParameter(p.Item1, p.Item2));
                        }
                    }
                    DbDataReader dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                    DataTable dt = new DataTable();
                    dt.Load(dr);
                    return dt;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
                return null;
            }
            finally
            {
                if (initialState != ConnectionState.Open)
                    conn.Close();
            }
        }

        public List<User> GetUser(List<string> userEmailList)
        {
            List<User> users = null;

            try
            {
                var db = new SupperClubContext();
                {
                    users = (from x in db.Users
                             join y in db.aspnet_Memberships on x.Id equals y.UserId
                             where userEmailList.Contains(y.Email.ToLower())
                             select x).ToList<User>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return users;
        }

        public User GetUser(string email)
        {
            User user = null;

            try
            {
                var db = new SupperClubContext();
                {
                    user = (from x in db.Users
                            join y in db.aspnet_Memberships on x.Id equals y.UserId
                            where y.LoweredEmail == email.ToLower()
                            select x).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return user;
        }
        #endregion

        #region Search
        public IList<SearchResult> SearchEvent(Search search, SearchFilter searchFilter)
        {
            IList<SearchResult> searchResult = null;
            IList<SearchResult> distinctSearchResult = null;
            try
            {
                StoredProc<Search> nearyBy = new StoredProc<Search>(typeof(SearchResult));
                using (var db = new SupperClubContext())
                {

                    ResultsList results1 = db.CallStoredProc<Search>(nearyBy, search);
                    //IList<SearchResult> storedProcResult = results1.ToList<SearchResult>();
                    searchResult = results1.ToList<SearchResult>();
                    //if (searchFilter.DietIds.Count > 0)
                    //    searchResult = storedProcResult.Where(x => searchFilter.DietIds.Contains(x.DietId)).ToList<SearchResult>();
                    //else
                    //    searchResult = storedProcResult;
                    //if (searchFilter.CuisineIds.Count > 0)
                    //    searchResult = searchResult.Where(x => searchFilter.CuisineIds.Contains(x.CuisineId)).ToList<SearchResult>();

                    distinctSearchResult = (from p in searchResult
                                            group p by new
                                            {
                                                p.EventId,
                                                p.EventName,
                                                p.EventDescription,
                                                p.EventImage,

                                                p.EventDateTime,
                                                p.EventStart,
                                                p.EventEnd,

                                                p.lat,
                                                p.lng,
                                                p.EventUrlFriendlyName,
                                                p.Cost,
                                                p.TotalSeats,
                                                p.GuestsAttending,

                                                p.Distance,
                                                p.BrandNew,
                                                p.SupperclubId,
                                                p.ReviewCount,
                                                p.Rating,
                                                p.EarlyBird,
                                                p.EarlyBirdPrice,
                                                p.WishEvent,
                                                p.DietId,
                                                p.CuisineId,
                                                p.TagId,

                                            } into grp
                                            select grp.First()).ToList<SearchResult>();

                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return distinctSearchResult;
        }

        public IList<EventListResult> SearchEvents(EventList eventList)
        {
            IList<EventListResult> searchResult = null;
            try
            {
                StoredProc<EventList> nearyBy = new StoredProc<EventList>(typeof(EventListResult));
                using (var db = new SupperClubContext())
                {

                    ResultsList results1 = db.CallStoredProc<EventList>(nearyBy, eventList);
                    searchResult = results1.ToList<EventListResult>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return searchResult;
        }

        public IList<EventListingResult> GetEvents(EventListing eventListing, EventListingFilter eventListingFilter, bool searchAll)
        {
            IList<EventListingResult> eventListingResult = null;
            IList<EventListingResult> distincteventListingResult = null;
            int resultsToReturn = 0;
            int resultPageIndex = 0;
            try
            {
                StoredProc<EventListing> nearyBy = new StoredProc<EventListing>(typeof(EventListingResult));
                using (var db = new SupperClubContext())
                {
                    resultsToReturn = eventListing.ResultsPerPage;
                    resultPageIndex = eventListing.PageIndex - 1;
                    eventListing.ResultsPerPage = 1000;
                    eventListing.PageIndex = 1;

                    ResultsList results1 = db.CallStoredProc<EventListing>(nearyBy, eventListing);
                    IList<EventListingResult> storedProcResult = results1.ToList<EventListingResult>();
                    if (eventListingFilter.DietIds != null && eventListingFilter.DietIds.Count > 0)
                        eventListingResult = storedProcResult.Where(x => eventListingFilter.DietIds.Contains(x.DietId)).ToList<EventListingResult>();
                    else
                        eventListingResult = storedProcResult;
                    if (eventListingFilter.CuisineIds != null && eventListingFilter.CuisineIds.Count > 0)
                        eventListingResult = eventListingResult.Where(x => eventListingFilter.CuisineIds.Contains(x.CuisineId)).ToList<EventListingResult>();

                    eventListingResult = (from p in eventListingResult
                                          group p by new
                                          {
                                              p.EventId,
                                              p.EventName,
                                              p.EventDescription,
                                              p.EventImage,
                                              p.EventStart,
                                              p.EventEnd,
                                              p.EventLatitude,
                                              p.EventLongitude,
                                              p.EventCity,
                                              p.EventPostCode,
                                              p.Charity,
                                              p.Alcohol,
                                              p.EventUrlFriendlyName,
                                              p.Cost,
                                              p.TotalSeats,
                                              p.GuestsAttending,
                                              p.Distance,
                                              p.IsFavourite
                                          } into grp
                                          select grp.First()).ToList<EventListingResult>();

                    if (resultsToReturn > 0)
                        eventListingResult = eventListingResult.Skip(resultsToReturn * resultPageIndex).Take(resultsToReturn).ToList<EventListingResult>();

                    //distincteventListingResult = (from p in eventListingResult
                    //group p by new
                    //{                                                
                    //    p.EventId,
                    //    p.EventName,
                    //    p.EventDescription,
                    //    p.EventImage,
                    //    p.EventStart,
                    //    p.EventEnd,
                    //    p.lat,
                    //    p.lng,
                    //    p.AddressCity,
                    //    p.AddressPostCode,
                    //    p.Charity,
                    //    p.Alcohol,
                    //    p.EventUrlFriendlyName,
                    //    p.Cost,
                    //    p.TotalSeats,
                    //    p.GuestsAttending,
                    //    p.Distance
                    //} into grp
                    //select grp.First()).ToList<EventListingResult>();
                    distincteventListingResult = eventListingResult;

                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return distincteventListingResult;
        }

        public IList<PriceRange> GetPriceRange()
        {
            IList<PriceRange> priceRange = null;
            try
            {
                using (var db = new SupperClubContext())
                {
                    priceRange = (from x in db.PriceRanges
                                  select x).ToList<PriceRange>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return priceRange;
        }

        public PriceRange GetMinMaxPriceRange(int[] priceRangeIds)
        {
            PriceRange priceRange = new PriceRange();
            using (var db = new SupperClubContext())
            {
                var s = (from x in db.PriceRanges
                         where priceRangeIds.Contains(x.Id)
                         select x);
                var maxMin = new
                {
                    MaxPrice = s.Max(x => x.MaxPrice),
                    MinPrice = s.Min(x => x.MinPrice)
                };

                priceRange.MaxPrice = maxMin.MaxPrice;
                priceRange.MinPrice = maxMin.MinPrice;
            }

            return priceRange;
        }

        public IList<Diet> GetDiets()
        {
            IList<Diet> diets = null;
            try
            {
                using (var db = new SupperClubContext())
                {

                    diets = (from x in db.Diets
                             where x.DietTypeId == (int)DietType.Allergy || x.DietTypeId == (int)DietType.Diet
                             select x).ToList<Diet>();

                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return diets;
        }

        public IList<Diet> GetStandardDiets()
        {
            IList<Diet> diets = null;
            try
            {
                using (var db = new SupperClubContext())
                {

                    diets = (from x in db.Diets
                             where x.DietTypeId == (int)DietType.Diet
                             select x).ToList<Diet>();

                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return diets;
        }

        public IList<Diet> GetAllergyDiets()
        {
            IList<Diet> diets = null;
            try
            {
                using (var db = new SupperClubContext())
                {

                    diets = (from x in db.Diets
                             where x.DietTypeId == (int)DietType.Allergy
                             select x).ToList<Diet>();

                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return diets;
        }

        public IList<Diet> GetOtherAllergyDiets(int eventId)
        {
            IList<Diet> diets = null;
            try
            {
                using (var db = new SupperClubContext())
                {

                    diets = (from x in db.Diets
                             join y in db.EventDiets on x.Id equals y.DietId
                             where y.EventId == eventId && x.DietTypeId == (int)DietType.Others
                             select x).ToList<Diet>();

                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return diets;
        }

        public List<int> GetAllAllergyDiets()
        {
            List<int> dietIds = null;
            try
            {
                using (var db = new SupperClubContext())
                {

                    dietIds = (from x in db.Diets
                               where x.DietTypeId == (int)DietType.Allergy || x.DietTypeId == (int)DietType.Others
                               select x.Id).ToList<int>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return dietIds;
        }

        public IList<Tag> GetTags()
        {
            IList<Tag> tags = null;
            try
            {
                using (var db = new SupperClubContext())
                {

                    tags = (from x in db.Tags

                            select x).ToList<Tag>();

                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return tags;
        }

        public IList<ProfileTag> GetProfileTags()
        {
            IList<ProfileTag> tags = null;
            try
            {
                using (var db = new SupperClubContext())
                {

                    tags = (from x in db.ProfileTags
                            select x).ToList<ProfileTag>();

                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return tags;
        }

        public IList<Tag> GetTagsWithoutCuisine()
        {
            IList<Tag> tags = null;
            try
            {
                using (var db = new SupperClubContext())
                {

                    tags = (from y in db.Tags
                            join x in db.Cuisines on y.Id equals x.TagId
                             into gj
                            from subCuisine in gj.DefaultIfEmpty()
                            where subCuisine.TagId == null && y.Private == false
                            orderby y.Name ascending
                            select y).ToList<Tag>();

                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return tags;
        }

        public IList<Tag> GetActiveEventTags()
        {
            IList<Tag> tags = null;
            try
            {
                using (var db = new SupperClubContext())
                {

                    tags = (from x in db.Tags
                            join et in db.EventTags on x.Id equals et.TagId
                            join e in db.Events on et.EventId equals e.Id
                            join s in db.SupperClubs on e.SupperClubId equals s.Id
                            where s.Active == true && e.Status == (int)SupperClub.Domain.EventStatus.Active && e.Private == false && e.Start > DateTime.Now
                            orderby x.Name ascending
                            select x).Distinct().ToList<Tag>();

                    IList<Cuisine> cuisines = (from x in db.Cuisines
                                               join ec in db.EventCuisines on x.Id equals ec.CuisineId
                                               join e in db.Events on ec.EventId equals e.Id
                                               join s in db.SupperClubs on e.SupperClubId equals s.Id
                                               where x.Active == true && s.Active == true && e.Status == (int)SupperClub.Domain.EventStatus.Active && e.Private == false && e.Start > DateTime.Now
                                               orderby x.Name ascending
                                               select x).Distinct().ToList<Cuisine>();
                    if (cuisines != null && cuisines.Count > 1)
                    {
                        tags = (from t in tags
                                join c in cuisines on t.Id equals c.TagId into temp
                                from j in temp.DefaultIfEmpty()
                                select t).Distinct().ToList<Tag>();
                    }

                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }

            return tags;
        }

        public GeoLocation GetGeoLocation(string postcode, string city, string address, string address2)
        {
            GeoLocation gl = null;
            try
            {
                using (var db = new SupperClubContext())
                {
                    if (!string.IsNullOrEmpty(address) && !string.IsNullOrEmpty(address2))
                    {
                        Event e = (from x in db.Events
                              where x.Address.ToLower() == address.ToLower()
                              && x.Address2.ToLower() == address2.ToLower()
                              && x.City.ToLower() == city.ToLower()
                              && x.PostCode.ToLower() == postcode.ToLower()
                              select x).FirstOrDefault();
                        if(e != null)
                            gl = new GeoLocation(1, e.Latitude, e.Longitude);
                    }
                    else if (!string.IsNullOrEmpty(address) && string.IsNullOrEmpty(address2))
                    {
                        Event e = (from x in db.Events
                              where x.Address.ToLower() == address.ToLower()
                              && x.City.ToLower() == city.ToLower()
                              && x.PostCode.ToLower() == postcode.ToLower()
                              select x).FirstOrDefault();
                        if (e != null)
                            gl = new GeoLocation(1, e.Latitude, e.Longitude);
                    }
                    else if (string.IsNullOrEmpty(address) && string.IsNullOrEmpty(address2) && !string.IsNullOrEmpty(postcode))
                    {
                        Event e = (from x in db.Events
                              where x.City.ToLower() == city.ToLower()
                              && x.PostCode.ToLower() == postcode.ToLower()
                              select x).FirstOrDefault();
                        if (e != null)
                            gl = new GeoLocation(1, e.Latitude, e.Longitude);
                    }
                    else
                    {
                        Event e = (from x in db.Events
                              where x.City.ToLower() == city.ToLower()
                              select x).FirstOrDefault();
                        if (e != null)
                            gl = new GeoLocation(1, e.Latitude, e.Longitude);
                    }                
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }

            return gl;
        }

        public IList<City> GetCities()
        {
            IList<City> cities = null;
            try
            {
                using (var db = new SupperClubContext())
                {

                    cities = (from x in db.Cities
                              select x).ToList<City>();

                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return cities;
        }
        public IList<Area> GetAreas()
        {
            IList<Area> areas = null;
            try
            {
                using (var db = new SupperClubContext())
                {

                    areas = (from x in db.Areas
                             select x).ToList<Area>();

                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return areas;
        }

        public IList<SearchCategory> GetSearchCategories()
        {
            IList<SearchCategory> searchCategories = null;
            try
            {
                using (var db = new SupperClubContext())
                {

                    searchCategories = (from x in db.SearchCategories
                                        select x).ToList<SearchCategory>();

                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return searchCategories;
        }
        public IList<Cuisine> GetCuisines()
        {
            IList<Cuisine> cuisines = null;
            try
            {
                using (var db = new SupperClubContext())
                {

                    cuisines = (from x in db.Cuisines
                                where x.Active == true
                                orderby x.Name ascending
                                select x).ToList<Cuisine>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return cuisines;
        }

        public IList<Cuisine> GetAvailableCuisines(int tagId)
        {
            IList<Cuisine> cuisines = null;
            try
            {
                using (var db = new SupperClubContext())
                {
                    if (tagId == 0)
                    {
                        cuisines = (from x in db.Cuisines
                                    join ec in db.EventCuisines on x.Id equals ec.CuisineId
                                    join e in db.Events on ec.EventId equals e.Id
                                    join s in db.SupperClubs on e.SupperClubId equals s.Id
                                    where x.Active == true && s.Active == true && e.Status == (int)SupperClub.Domain.EventStatus.Active && e.Private == false && e.Start > DateTime.Now
                                    orderby x.Name ascending
                                    select x).Distinct().ToList<Cuisine>();
                    }
                    else
                    {
                        cuisines = (from x in db.Cuisines
                                    join ec in db.EventCuisines on x.Id equals ec.CuisineId
                                    join e in db.Events on ec.EventId equals e.Id
                                    join s in db.SupperClubs on e.SupperClubId equals s.Id
                                    join t in db.EventTags on e.Id equals t.EventId
                                    where x.Active == true && t.TagId == tagId && s.Active == true && e.Status == (int)SupperClub.Domain.EventStatus.Active && e.Private == false && e.Start > DateTime.Now
                                    orderby x.Name ascending
                                    select x).Distinct().ToList<Cuisine>();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return cuisines;
        }

        public bool CheckTagCategoryCombinationValidity(string categoryname, string tagname)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    var ud = (from x in db.SearchCategories
                              join y in db.SearchCategoryTags on x.Id equals y.SearchCategoryId
                              join z in db.Tags on y.TagId equals z.Id
                              where x.UrlFriendlyName.ToLower() == categoryname && z.UrlFriendlyName.ToLower() == tagname
                              select y).FirstOrDefault<SearchCategoryTag>();
                    if (ud != null)
                        return true;
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }

            return false;
        }
        #endregion

        #region Ticket and Basket

        public TicketBasket CreateBasket(Guid basketId, Guid userId, string name)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    // Empty any old baskets for this user
                    List<TicketBasket> oldBaskets = db.TicketBaskets.AsNoTracking().Where(x => x.UserId == userId && x.Status == "InProgress").ToList();
                    foreach (TicketBasket tb in oldBaskets)
                        if (tb.Tickets.Count > 0)
                            RemoveFromBasket(tb.Tickets);

                    // Create new basket
                    TicketBasket basket = new TicketBasket();
                    basket.Id = basketId;
                    basket.DateCreated = DateTime.Now;
                    basket.LastUpdated = DateTime.Now;
                    basket.Status = TicketBasketStatus.InProgress.ToString();
                    basket.UserId = userId;
                    basket.Name = name;
                    // Add the new basket
                    db.TicketBaskets.Add(basket);
                    db.SaveChanges();

                    return basket;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return null;
        }

        public TicketBasket UpdateTicketBasket(TicketBasket ticketBasket, TicketBasketStatus status)
        {
            if (ticketBasket == null) return null;

            try
            {
                using (var db = new SupperClubContext())
                {
                    ticketBasket.Status = status.ToString();
                    ticketBasket.LastUpdated = DateTime.Now;

                    db.TicketBaskets.Attach(ticketBasket);
                    db.Entry(ticketBasket).State = EntityState.Modified;
                    db.SaveChanges();

                    return ticketBasket;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return null;
        }

        public TicketBasket GetBasket(Guid id)
        {
            TicketBasket basket = null;
            try
            {
                using (var db = new SupperClubContext())
                {
                    basket = (from x in db.TicketBaskets.Include("Tickets").AsNoTracking()
                              where x.Id == id
                              select x).FirstOrDefault<SupperClub.Domain.TicketBasket>();

                    if (basket != null)
                    {
                        basket.LastUpdated = DateTime.Now;
                        db.Entry(basket).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return basket;
        }

        public TicketBasket GetExistingBasket(Guid id)
        {
            TicketBasket basket = null;
            try
            {
                using (var db = new SupperClubContext())
                {
                    basket = (from x in db.TicketBaskets.Include("Tickets").AsNoTracking()
                              where x.Id == id
                              select x).FirstOrDefault<SupperClub.Domain.TicketBasket>();

                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return basket;
        }
        public TicketBasket AddToBasket(List<Ticket> tickets)
        {
            var db = new SupperClubContext();
            TicketBasket basket = null;
            try
            {
                Guid basketId = tickets[0].BasketId;
                foreach (Ticket ticket in tickets)
                {
                    db.Tickets.Add(ticket);
                    db.SaveChanges();
                }
                basket = GetBasket(basketId);
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return basket;
        }
        
        public TicketBasket RemoveFromBasket(List<Ticket> tickets)
        {
            var db = new SupperClubContext();
            TicketBasket basket = null;
            try
            {
                Guid basketId = tickets[0].BasketId;

                int numberTickets = tickets.Count;
                for (int i = numberTickets - 1; i >= 0; i--)
                {
                    // Attach only if required
                    if (!db.Tickets.Local.Any(e => e.Id == tickets[i].Id))
                        db.Tickets.Attach(tickets[i]);

                    db.Tickets.Remove(tickets[i]);
                    db.SaveChanges();
                }

                basket = GetBasket(basketId);
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return basket;
        }
        public bool CheckBookingForBasket(Guid basketId)
        {
            bool result = true;
            using (var db = new SupperClubContext())
            {
                EventAttendee ea = (from x in db.EventAttendees
                          where x.BasketId == basketId
                          select x).FirstOrDefault<SupperClub.Domain.EventAttendee>();

                if (ea == null)
                {
                    result = false;
                }
            }
            return result;
        }
        public bool UpdateTicketUser(Guid basketId, Guid userId)
        {
            List<Ticket> tickets = new List<Ticket>();
            try
            {
                using (var db = new SupperClubContext())
                {
                    tickets = (from x in db.Tickets
                               join tb in db.TicketBaskets on x.BasketId equals tb.Id
                               where tb.Id == basketId
                               select x).ToList();
                    if (tickets != null && tickets.Count > 0)
                    {
                        foreach (Ticket t in tickets)
                        {
                            t.UserId = userId;
                            db.Tickets.Attach(t);
                            db.Entry(t).State = EntityState.Modified;
                            db.SaveChanges();
                        }
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return false;
        }
        public int GetNumberTicketsInProgressForEvent(int eventId)
        {
            List<Ticket> tickets = new List<Ticket>();
            using (var db = new SupperClubContext())
            {
                string inProgressStatus = TicketBasketStatus.InProgress.ToString();
                tickets = (from x in db.Tickets
                           join tb in db.TicketBaskets on x.BasketId equals tb.Id
                           where tb.Status == inProgressStatus && x.EventId == eventId
                           select x).ToList();
                return tickets.Count;
            }
        }
        public int GetNumberTicketsInProgressForEvent(int eventId, int seatingId)
        {
            List<Ticket> tickets = new List<Ticket>();
            using (var db = new SupperClubContext())
            {
                string inProgressStatus = TicketBasketStatus.InProgress.ToString();
                tickets = (from x in db.Tickets
                           join tb in db.TicketBaskets on x.BasketId equals tb.Id
                           where tb.Status == inProgressStatus && x.EventId == eventId && x.SeatingId == seatingId
                           select x).ToList();
                return tickets.Count;
            }
        }
        public int GetNumberTicketsInProgressForUser(int eventId, int seatingId, Guid userId)
        {
            List<Ticket> tickets = new List<Ticket>();
            using (var db = new SupperClubContext())
            {
                string inProgressStatus = TicketBasketStatus.InProgress.ToString();
                tickets = (from x in db.Tickets
                           join tb in db.TicketBaskets on x.BasketId equals tb.Id
                           where tb.Status == inProgressStatus && x.EventId == eventId && x.SeatingId == seatingId && x.UserId == userId
                           select x).ToList();
                return tickets.Count;
            }
        }

        public int GetPreviousEventTicketsForUser(Guid userId, int eventId, int seatingId, int menuOptionId)
        {
            using (var db = new SupperClubContext())
            {
                EventAttendee ea = db.EventAttendees.FirstOrDefault(x => x.UserId == userId && x.EventId == eventId && x.SeatingId == seatingId && x.MenuOptionId == menuOptionId);
                if (ea != null)
                    return ea.NumberOfGuests;
                return 0;
            }
        }
        public int GetPreviousEventTicketsForUser(Guid userId, int eventId, int seatingId)
        {
            using (var db = new SupperClubContext())
            {
                EventAttendee ea = db.EventAttendees.FirstOrDefault(x => x.UserId == userId && x.EventId == eventId && x.SeatingId == seatingId);
                if (ea != null)
                    return ea.NumberOfGuests;
                return 0;
            }
        }
        public string GetBookingRequirements(int eventId, Guid userId)
        {
            string bookingRequirements = string.Empty;
            try
            {
                using (var db = new SupperClubContext())
                {
                    List<string> brList = (from ea in db.EventAttendees
                                           join tb in db.TicketBaskets on ea.BasketId equals tb.Id
                                           where ea.EventId == eventId && ea.UserId == userId
                                           select tb.BookingRequirements).Distinct().ToList<string>();
                    if (brList != null && brList.Count > 0)
                    {
                        foreach (string br in brList)
                        {
                            if (!string.IsNullOrEmpty(br))
                                bookingRequirements = bookingRequirements + "*" + br;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return bookingRequirements;
        }
        public Tuple<int, int> CleanUpAbandonedBaskets(int olderThanMinutes)
        {
            using (var db = new SupperClubContext())
            {
                List<TicketBasket> abandonedTicketBaskets = new List<TicketBasket>();
                DateTime expired = DateTime.Now.AddMinutes(-olderThanMinutes);
                string inProgressStatus = TicketBasketStatus.InProgress.ToString();
                abandonedTicketBaskets = db.TicketBaskets.Where(x => x.LastUpdated < expired && x.Status == inProgressStatus).ToList();
                int numberAbandonedBaskets = abandonedTicketBaskets.Count;
                int numberAbandonedTickets = 0;
                foreach (TicketBasket tb in abandonedTicketBaskets)
                {
                    tb.Status = TicketBasketStatus.Expired.ToString();
                    if (tb.Tickets != null)
                    {
                        int numberTickets = tb.Tickets.Count;
                        numberAbandonedTickets += numberTickets;
                        for (int i = numberTickets - 1; i >= 0; i--)
                            db.Entry<Ticket>(tb.Tickets[i]).State = EntityState.Deleted;
                    }
                }
                db.SaveChanges();
                return new Tuple<int, int>(numberAbandonedBaskets, numberAbandonedTickets);
            }
        }

        public bool CleanUpBaskets(Guid BasketId)
        {
            bool status = false;
            try
            {
                using (var db = new SupperClubContext())
                {
                    List<TicketBasket> abandonedTicketBaskets = new List<TicketBasket>();
                    string inProgressStatus = TicketBasketStatus.InProgress.ToString();
                    abandonedTicketBaskets = db.TicketBaskets.Where(x => x.Id == BasketId && x.Status == inProgressStatus).ToList();
                    foreach (TicketBasket tb in abandonedTicketBaskets)
                    {
                        tb.Status = TicketBasketStatus.Expired.ToString();
                    }
                    db.SaveChanges();
                    status = true;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return status;
        }

        public bool CheckInUserToEvent(int bookingReferenceNum, Guid userId)
        {
            bool status = false;
            try
            {
                using (var db = new SupperClubContext())
                {
                    TicketBasket tb = new TicketBasket();
                    tb = db.TicketBaskets.Where(x => x.BookingReference == bookingReferenceNum && x.UserId == userId).FirstOrDefault();
                    tb.CheckedIn = true;
                    tb.CheckInDate = DateTime.Now;
                    db.TicketBaskets.Attach(tb);
                    db.Entry(tb).State = EntityState.Modified;
                    db.SaveChanges();
                    status = true;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return status;
        }

        public bool ResetCheckInToEvent(int bookingReferenceNum, Guid userId)
        {
            bool status = false;
            try
            {
                using (var db = new SupperClubContext())
                {
                    TicketBasket tb = new TicketBasket();
                    tb = db.TicketBaskets.Where(x => x.BookingReference == bookingReferenceNum && x.UserId == userId).FirstOrDefault();
                    tb.CheckedIn = false;
                    tb.CheckInDate = DateTime.Now;
                    db.TicketBaskets.Attach(tb);
                    db.Entry(tb).State = EntityState.Modified;
                    db.SaveChanges();
                    status = true;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return status;
        }

        public int CheckVoucherCode(string voucherCode, int eventId, Guid userId, int totalBookings, decimal basketValue)
        {
            int status = (int)VoucherState.Invalid;
            try
            {
                using (var db = new SupperClubContext())
                {
                    Voucher v = db.Vouchers.Where(x => x.Code.ToLower() == voucherCode.ToLower()).FirstOrDefault();
                    if (v == null)
                        return (int)VoucherState.Invalid;
                    else
                    {
                        // Check if voucher has expired
                        if ((v.ExpiryDate != null && v.ExpiryDate <= DateTime.Now) || (v.StartDate != null && v.StartDate >= DateTime.Now) || !v.Active)
                            return (int)VoucherState.Expired;

                        // Check if voucher has reached usage cap
                        if (v.UsageCap > 0 && v.NumberOfTimesUsed >= v.UsageCap)
                            return (int)VoucherState.UsageCapReached;

                        // Check if user can use this voucher
                        if (v.UniqueUserRedeemLimit > 0)
                        {
                            string inProgressStatus = TicketBasketStatus.Complete.ToString();
                            var ticketBaskets = (from x in db.Tickets
                                                 join tb in db.TicketBaskets on x.BasketId equals tb.Id
                                                 where tb.Status == inProgressStatus && x.VoucherId == v.Id && x.UserId == userId
                                                 select tb).Distinct().ToList();
                            if (ticketBaskets != null && ticketBaskets.Count >= v.UniqueUserRedeemLimit)
                                return (int)VoucherState.RedemptionLimitExhausted;
                        }

                        // Check balance if it is a gift voucher
                        if (v.TypeId == (int)VoucherType.GiftVoucher && (v.AvailableBalance == null || (v.AvailableBalance != null && v.AvailableBalance <= 0)))
                            return (int)VoucherState.NoBalance;

                        // Check if voucher is available for all events
                        if ((v.TotalBooking != null && v.TotalBooking > 0 && totalBookings < v.TotalBooking) || (v.MinBookingAmount > 0 && basketValue < v.MinBookingAmount))
                            return (int)VoucherState.BasketValueLowerThanRequired;

                        // Check if voucher is available for all events
                        if (v.IsGlobal)
                            return v.Id;

                        EventVoucher ev = db.EventVouchers.Where(x => x.EventId == eventId && x.VoucherId == v.Id).FirstOrDefault();
                        if (ev == null)
                        {
                            Event _event = db.Events.Where(x => x.Id == eventId).FirstOrDefault();
                            if (_event != null)
                            {
                                SupperClubVoucher scv = db.SupperClubVouchers.Where(x => x.SupperClubId == _event.SupperClubId && x.VoucherId == v.Id).FirstOrDefault();
                                if (scv != null)
                                    return scv.VoucherId;
                            }
                            return (int)VoucherState.Invalid;
                        }
                        else
                            return ev.VoucherId;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return status;
        }

        public Voucher GetVoucher(int voucherId)
        {
            Voucher voucher = null;
            try
            {
                using (var db = new SupperClubContext())
                {
                    voucher = db.Vouchers.Where(x => x.Id == voucherId && x.Active == true).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return voucher;
        }
        public Voucher GetVoucherDetail(int voucherId)
        {
            Voucher voucher = null;
            try
            {
                using (var db = new SupperClubContext())
                {
                    voucher = db.Vouchers.Where(x => x.Id == voucherId).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return voucher;
        }
        public Voucher GetVoucher(string voucherCode)
        {
            Voucher voucher = null;
            try
            {
                using (var db = new SupperClubContext())
                {
                    voucher = db.Vouchers.Where(x => x.Code.ToLower() == voucherCode.ToLower()).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return voucher;
        }

        public bool UpdateVoucher(Voucher voucher)
        {
            try
            {
                var db = new SupperClubContext();
                //Update the vocuher table
                db.Entry(voucher).State = EntityState.Modified;
                db.SaveChanges();
                return true;
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return false;
        }

        public IList<Diet> GetEventDiets(int eventId)
        {
            IList<Diet> diets = null;
            try
            {
                using (var db = new SupperClubContext())
                {

                    diets = (from x in db.EventDiets
                             join y in db.Diets on x.DietId equals y.Id
                             where x.EventId == eventId
                             select y).ToList<Diet>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return diets;
        }
        #endregion

        #region Transactions

        public PaymentTransaction CreateTransction(PaymentTransaction transaction)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    PaymentTransaction _transaction = db.PaymentTransactions.Add(transaction);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                // Added transaction object detail to exception to find out which DB field is causing the exception
                log.Fatal("Exception Message: " + ex.Message + " Transaction Object Details-> CAVV: " + transaction.CAVV + " Id: " + transaction.Id + " Payment3DStatus: " + transaction.Payment3DStatus + " PaymentStatus: " + transaction.PaymentStatus + " PaymentStatusDetail: " + transaction.PaymentStatusDetail + " RequestDate: " + transaction.RequestDate + " ResponseDate: " + transaction.ResponseDate + " SecurityKey: " + transaction.SecurityKey + " VendorTxCode: " + transaction.VendorTxCode + " VpsTxId: " + transaction.VpsTxId + " RedirectUrl: " + transaction.RedirectUrl, ex);
            }
            return transaction;
        }

        public PayPalTransaction CreateTransction(PayPalTransaction transaction)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    PayPalTransaction _transaction = db.PayPalTransactions.Add(transaction);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return transaction;
        }

        public BraintreeTransaction CreateTransction(BraintreeTransaction transaction)
        {
            BraintreeTransaction _transaction = new BraintreeTransaction();
            try
            {
                using (var db = new SupperClubContext())
                {
                    _transaction = db.BrainTreeTransactions.Add(transaction);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return _transaction;
        }
        public bool CreateBrainTreeCustomer(BraintreeCustomer customer)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    BraintreeCustomer _customer = db.BrainTreeCustomers.Add(customer);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return false;
        }
        public BraintreeCustomer GetBraintreeCustomer(Guid UserId)
        {
            BraintreeCustomer _customer = null;
            try
            {
                using (var db = new SupperClubContext())
                {
                    _customer = (from x in db.BrainTreeCustomers
                                 where x.UserId == UserId
                                 select x).FirstOrDefault<SupperClub.Domain.BraintreeCustomer>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return _customer;
        }
        public PaymentTransaction GetPaymentTransaction(string VendorTxCode)
        {
            PaymentTransaction transaction = null;
            try
            {
                using (var db = new SupperClubContext())
                {
                    transaction = (from x in db.PaymentTransactions
                                   where x.VendorTxCode == VendorTxCode
                                   select x).FirstOrDefault<SupperClub.Domain.PaymentTransaction>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return transaction;
        }

        #endregion

        #region Misc

        public bool AddSubscriber(Subscriber subscriber)
        {
            try
            {
                if (subscriber != null)
                {
                    using (var db = new SupperClubContext())
                    {
                        var s = (from x in db.Subscribers
                                 where x.EmailAddress.ToLower() == subscriber.EmailAddress.ToLower() && x.SubscriberType == subscriber.SubscriberType
                                 select x).FirstOrDefault<Subscriber>();
                        var user = (from x in db.Users
                                    join y in db.aspnet_Memberships on x.Id equals y.UserId
                                    where y.LoweredEmail == subscriber.EmailAddress.ToLower()
                                    select x).FirstOrDefault<User>();
                        if (s == null)
                        {
                            if (user != null)
                            {
                                subscriber.UserId = user.Id;
                                subscriber.FirstName = string.IsNullOrEmpty(subscriber.FirstName) ? user.FirstName : subscriber.FirstName;
                                subscriber.LastName = string.IsNullOrEmpty(subscriber.LastName) ? user.LastName : subscriber.LastName;
                            }
                            else
                            {
                                subscriber.UserId = null;
                            }
                            db.Subscribers.Add(subscriber);
                            db.SaveChanges();
                            return true;
                        }
                        else
                            return true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return false;
        }

        public int AddToWaitList(EventWaitList eventWaitList)
        {
            int status = 0;
            try
            {
                if (eventWaitList != null)
                {
                    using (var db = new SupperClubContext())
                    {
                        var wl = (from x in db.EventWaitLists
                                  where x.Email.ToLower() == eventWaitList.Email.ToLower() && x.EventId == eventWaitList.EventId
                                  select x).FirstOrDefault<EventWaitList>();
                        var user = (from x in db.Users
                                    join y in db.aspnet_Memberships on x.Id equals y.UserId
                                    where y.LoweredEmail == eventWaitList.Email.ToLower()
                                    select x).FirstOrDefault<User>();

                        if (wl == null)
                        {
                            if (user != null)
                                eventWaitList.UserId = user.Id;
                            EventWaitList _eventWaitList = db.EventWaitLists.Add(eventWaitList);
                            status = 1;
                        }
                        else
                        {
                            if (user != null)
                            {
                                wl.UserId = user.Id;
                                db.Entry(wl).State = EntityState.Modified;
                            }
                            status = 2;
                        }

                        var s = (from x in db.Subscribers
                                 where x.EmailAddress.ToLower() == eventWaitList.Email.ToLower()
                                 select x).FirstOrDefault<Subscriber>();
                        if (s == null)
                        {
                            Subscriber sub = new Subscriber();
                            if (user != null)
                            {
                                sub.UserId = user.Id;
                                sub.FirstName = user.FirstName;
                                sub.LastName = user.LastName;
                            }
                            else
                            {
                                sub.UserId = null;
                                sub.FirstName = "";
                                sub.LastName = "";
                            }
                            sub.EmailAddress = eventWaitList.Email;
                            sub.SubscriptionDateTime = DateTime.Now;
                            sub.SubscriberType = (int)SupperClub.Domain.SubscriberType.WaitListRequest;
                            s = db.Subscribers.Add(sub);
                            db.SaveChanges();
                        }
                        else
                        {
                            if (user != null)
                            {
                                s.UserId = user.Id;
                                s.FirstName = !string.IsNullOrEmpty(user.FirstName) ? user.FirstName : s.FirstName;
                                s.LastName = !string.IsNullOrEmpty(user.LastName) ? user.LastName : s.LastName;
                                s.SubscriptionDateTime = DateTime.Now;
                                db.Entry(s).State = EntityState.Modified;
                            }
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return status;
        }

        public bool RemoveFromWaitList(int eventId, Guid userId)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    EventWaitList ewl = (from x in db.EventWaitLists
                                         where x.UserId == userId && x.EventId == eventId
                                         select x).SingleOrDefault<EventWaitList>();
                    if (ewl != null)
                    {
                        db.EventWaitLists.Remove(ewl);
                        db.SaveChanges();
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return false;
        }

        public bool IsUserAddedToWaitList(Guid userId, int eventId)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    EventWaitList ewl = (from x in db.EventWaitLists
                                         where x.UserId == userId && x.EventId == eventId
                                         select x).SingleOrDefault<EventWaitList>();
                    if (ewl != null)
                        return true;
                    else
                        return false;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return false;
        }

        public bool AddGuestEmailInfo(Guid currentUser, List<Subscriber> lstSubscriber)
        {
            try
            {
                if (lstSubscriber.Count > 0)
                {
                    using (var db = new SupperClubContext())
                    {
                        foreach (Subscriber _subscriber in lstSubscriber)
                        {
                            var s = (from x in db.Subscribers
                                     where x.EmailAddress.ToLower() == _subscriber.EmailAddress.ToLower() && x.SubscriberType == (int)SupperClub.Domain.SubscriberType.GuestInvitee
                                     select x).FirstOrDefault<Subscriber>();
                            var user = (from x in db.Users
                                        join y in db.aspnet_Memberships on x.Id equals y.UserId
                                        where y.LoweredEmail == _subscriber.EmailAddress.ToLower()
                                        select x).FirstOrDefault<User>();

                            if (s != null)
                            {
                                s.FirstName = string.IsNullOrEmpty(_subscriber.FirstName) ? s.FirstName : _subscriber.FirstName;
                                s.LastName = string.IsNullOrEmpty(_subscriber.LastName) ? s.LastName : _subscriber.LastName;
                                if (user != null)
                                    s.UserId = user.Id;
                                db.Entry(s).State = EntityState.Modified;

                                var uc = (from x in db.UserInvitees
                                          where x.SubscriberId == s.Id && x.UserId == currentUser
                                          select x).FirstOrDefault<UserInvitee>();
                                if (uc == null)
                                {
                                    UserInvitee ui = new UserInvitee();
                                    ui.SubscriberId = s.Id;
                                    ui.UserId = currentUser;
                                    db.UserInvitees.Add(ui);
                                }
                            }
                            else
                            {
                                Subscriber sub = new Subscriber();
                                if (user != null)
                                {
                                    sub.UserId = user.Id;
                                    sub.FirstName = string.IsNullOrEmpty(_subscriber.FirstName) ? user.FirstName : _subscriber.FirstName;
                                    sub.LastName = string.IsNullOrEmpty(_subscriber.LastName) ? user.LastName : _subscriber.LastName;
                                }
                                else
                                {
                                    sub.UserId = null;
                                    sub.FirstName = _subscriber.FirstName;
                                    sub.LastName = _subscriber.LastName;
                                }
                                sub.EmailAddress = _subscriber.EmailAddress;
                                sub.SubscriptionDateTime = DateTime.Now;
                                sub.SubscriberType = (int)SupperClub.Domain.SubscriberType.GuestInvitee;

                                db.Entry(sub).State = System.Data.Entity.EntityState.Added;
                                db.SaveChanges();

                                UserInvitee ui = new UserInvitee();
                                ui.SubscriberId = sub.Id;
                                ui.UserId = currentUser;
                                db.UserInvitees.Add(ui);
                            }
                        }
                        db.SaveChanges();
                    }
                    return true;
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Could not add User Invitee info." + ex.Message + ex.StackTrace + " InnerException:" + ex.InnerException == null ? "" : (ex.InnerException.Message + ex.InnerException.StackTrace));
            }
            return false;
        }

        public bool AddGuestName(string email, string name)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    var s = (from x in db.Subscribers
                             where x.EmailAddress.ToLower() == email.ToLower()
                             select x).FirstOrDefault<Subscriber>();
                    if (s == null)
                    {
                        var bcf = (from x in db.BookingConfirmationToFriends
                                   where x.FriendsMailIds.ToLower() == email.ToLower()
                                   select x).FirstOrDefault<BookingConfirmationToFriends>();
                        if (bcf != null)
                        {
                            Subscriber ss = new Subscriber();
                            ss.EmailAddress = bcf.FriendsMailIds;
                            ss.FirstName = name;
                            ss.LastName = string.Empty;
                            ss.UserId = null;
                            ss.SubscriberType = (int)SubscriberType.GuestInvitee;
                            ss.SubscriptionDateTime = DateTime.Now;
                            ss = db.Subscribers.Add(ss);
                            db.SaveChanges();

                            UserInvitee ui = new UserInvitee();
                            ui.UserId = bcf.UserId;
                            ui.SubscriberId = ss.Id;
                            db.UserInvitees.Add(ui);
                            db.SaveChanges();
                        }
                        else
                        {
                            Subscriber ss = new Subscriber();
                            ss.EmailAddress = bcf.FriendsMailIds;
                            ss.FirstName = name;
                            ss.LastName = string.Empty;
                            ss.UserId = null;
                            ss.SubscriberType = (int)SubscriberType.ReviewEmail;
                            ss.SubscriptionDateTime = DateTime.Now;
                            ss = db.Subscribers.Add(ss);
                            db.SaveChanges();
                        }
                    }
                    else
                    {
                        s.FirstName = name;
                        db.Entry(s).State = EntityState.Modified;
                        db.SaveChanges();
                    }
                    return true;
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }

            return false;
        }

        public string GetGuestName(string email)
        {
            string guestName = "Guest";
            try
            {
                var db = new SupperClubContext();
                {
                    var s = (from x in db.Subscribers
                             where x.EmailAddress.ToLower() == email.ToLower()
                             select x).FirstOrDefault<Subscriber>();
                    if (s != null)
                        guestName = s.FirstName;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return guestName;
        }
        public UrlRewrite GetUrl(string urlRewrite)
        {
            UrlRewrite url = null;
            try
            {
                var db = new SupperClubContext();
                {
                    url = (from x in db.UrlRewrites
                           where x.RewriteUrl == urlRewrite
                           select x).FirstOrDefault<UrlRewrite>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return url;
        }

        public List<UrlRewrite> GetUrlRewrites()
        {
            List<UrlRewrite> urls = null;
            try
            {
                var db = new SupperClubContext();
                {
                    urls = (from u in db.UrlRewrites where u.Active == true select u).ToList<UrlRewrite>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return urls;
        }

        public bool AddPushNotificationLog(PushNotificationLog _pushNotificationLog)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    if (_pushNotificationLog != null)
                    {
                        db.PushNotificationLogs.Add(_pushNotificationLog);
                        db.SaveChanges();
                        return true;
                    }
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }

            return false;
        }
        public IList<TileTag> GetTileTags()
        {
            IList<TileTag> tileTags = null;
            try
            {
                using (var db = new SupperClubContext())
                {
                    tileTags = (from x in db.TileTags.Include("Tag").Include("Image").Include("Tile")
                                where x.Active == true
                                orderby x.LastUpdatedDate descending
                                select x).Take(6).ToList<TileTag>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return tileTags;
        }

        public IList<PressRelease> GetPressReleases()
        {
            IList<PressRelease> pressReleases = null;
            try
            {
                using (var db = new SupperClubContext())
                {
                    pressReleases = (from x in db.PressReleases
                                     select x).ToList<PressRelease>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return pressReleases;
        }

        public List<String> GetFacebookFriendListForPushNotification(Guid userId, string faceBookId)
        {
            List<String> friendList = null;
            try
            {
                using (var db = new SupperClubContext())
                {
                    friendList = (from x in db.UserFacebookFriends
                                  join u in db.Users on x.UserFacebookId equals u.FacebookId
                                  join ud in db.UserDevices on u.Id equals ud.UserId
                                  where ud.FbFriendInstalledApp == true && x.UserId == userId && x.UserFacebookId == faceBookId
                                  select ud.DeviceId).ToList<string>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return friendList;
        }

        public List<String> GetFacebookFriendListForBookingPushNotification(Guid userId, string faceBookId)
        {
            List<String> friendList = null;
            try
            {
                using (var db = new SupperClubContext())
                {
                    friendList = (from x in db.UserFacebookFriends
                                  join u in db.Users on x.UserFacebookId equals u.FacebookId
                                  join ud in db.UserDevices on u.Id equals ud.UserId
                                  where ud.FbFriendBookedTicket == true && x.UserId == userId && x.UserFacebookId == faceBookId
                                  select ud.DeviceId).ToList<string>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return friendList;
        }

        public List<String> GetUserListForNewEventPushNotification(int supperClubId)
        {
            List<String> userList = null;
            try
            {
                using (var db = new SupperClubContext())
                {
                    userList = (from x in db.UserFavouriteSupperClubs
                                join u in db.Users on x.UserId equals u.Id
                                join ud in db.UserDevices on u.Id equals ud.UserId
                                where ud.FavChefNewEventNotification == true && x.SupperClubId == supperClubId
                                select ud.DeviceId).ToList<string>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return userList;
        }
        public List<string> GetUserListForEventBookingReminderPushNotification(int eventId)
        {
            List<String> userList = null;
            try
            {
                using (var db = new SupperClubContext())
                {
                    userList = (from x in db.UserFavouriteEvents
                                join u in db.Users on x.UserId equals u.Id
                                join ud in db.UserDevices on u.Id equals ud.UserId
                                join ea in db.EventAttendees on new { x.EventId, x.UserId } equals new { ea.EventId, ea.UserId } into gj
                                from subEventAttendee in gj.DefaultIfEmpty()
                                where ud.FavEventBookingReminder == true && x.EventId == eventId && x.PushNotificationSent == false && subEventAttendee.UserId == null
                                select ud.DeviceId).ToList<string>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return userList;
        }
        public bool UpdateUsersPushNotificationForEventBookingReminder(int eventId)
        {
            bool status = false;
            List<int> userList = null;
            try
            {
                using (var db = new SupperClubContext())
                {
                    userList = (from x in db.UserFavouriteEvents
                                join u in db.Users on x.UserId equals u.Id
                                join ud in db.UserDevices on u.Id equals ud.UserId
                                join ea in db.EventAttendees on new { x.EventId, x.UserId } equals new { ea.EventId, ea.UserId } into gj
                                from subEventAttendee in gj.DefaultIfEmpty()
                                where ud.FavEventBookingReminder == true && x.EventId == eventId && x.PushNotificationSent == false && subEventAttendee.UserId == null
                                select x.Id).Distinct().ToList<int>();
                    if (userList != null)
                    {
                        db.UserFavouriteEvents
                       .Where(x => userList.Contains(x.Id))
                       .ToList()
                       .ForEach(a => a.PushNotificationSent = true);
                    }
                    db.SaveChanges();
                    status = true;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return status;
        }

        public bool UpdateUsersEmailNotificationForEventBookingReminder(int eventId, Guid userId)
        {
            bool status = false;
            try
            {
                using (var db = new SupperClubContext())
                {
                    UserFavouriteEvent ufe = db.UserFavouriteEvents.FirstOrDefault(x => x.EventId == eventId && x.UserId == userId);
                    if (ufe == null)
                        return status;

                    ufe.EmailNotificationSent = true;
                    db.Entry(ufe).State = EntityState.Modified;
                    db.SaveChanges();
                    status = true;
                }
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        log.Fatal("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return status;

        }
        public List<string> GetUserListForEventWaitListPushNotification(int eventId)
        {
            List<String> userList = null;
            try
            {
                using (var db = new SupperClubContext())
                {
                    userList = (from x in db.EventWaitLists
                                join u in db.Users on x.UserId equals u.Id
                                join ud in db.UserDevices on u.Id equals ud.UserId
                                where ud.WaitlistEventTicketsAvailable == true && x.EventId == eventId && x.NotificationSent == false
                                select ud.DeviceId).ToList<string>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return userList;
        }

        public IList<EventAttendee> GetRecentBookingDetailsForUser(Guid userId)
        {
            List<EventAttendee> recentBookings = null;
            DateTime cutOffTime = DateTime.Now.AddHours(-24);
            try
            {
                using (var db = new SupperClubContext())
                {
                    recentBookings = (from x in db.EventAttendees.AsNoTracking().Include("Event")
                                where x.UserId == userId && x.BookingDate >= cutOffTime
                                select x).ToList<EventAttendee>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return recentBookings;
        }
        
        #endregion

        #region Reviews

        public Review GetReview(int reviewId)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    Review r = db.Reviews.AsNoTracking().Include("Event").FirstOrDefault(x => x.Id == reviewId);
                    return r;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return null;
        }

        public Review GetReview(int eventId, Guid userId)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    Review r = db.Reviews.AsNoTracking().Include("Event").FirstOrDefault(x => x.EventId == eventId && x.UserId == userId);
                    return r;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return null;
        }

        public List<Review> GetPublishedReviewsForEvent(int eventId)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    List<Review> rs = db.Reviews.Where(x => x.Event.Id == eventId && x.Publish == true).ToList();
                    return rs;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return null;
        }

        public List<Review> GetAllReviewsForEvent(int eventId)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    List<Review> rs = db.Reviews.Where(x => x.Event.Id == eventId).ToList();
                    return rs;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return null;
        }

        public bool AddReview(Review review)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    Review _review = db.Reviews.AsNoTracking().FirstOrDefault(x => x.EventId == review.EventId && x.UserId == review.UserId);
                    if (_review != null)
                    {
                        review.Id = _review.Id;
                        review.DateCreated = _review.DateCreated == null ? DateTime.Now : _review.DateCreated;
                        review.AdminResponse = _review.AdminResponse;
                        review.AdminResponseDate = _review.AdminResponseDate;
                        review.HostResponse = _review.HostResponse;
                        review.HostResponseDate = _review.HostResponseDate;
                        review.PrivateReview = _review.PrivateReview;
                        db.Entry(review).State = EntityState.Modified;
                    }
                    else
                    {
                        review.DateCreated = DateTime.Now;
                        review.AdminResponseDate = null;
                        review.HostResponseDate = null;
                        db.Reviews.Add(review);
                    }
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message + "Review date:" + (review != null ? (review.DateCreated == null ? "null" : review.DateCreated.ToString()) : "Review object is null"), ex);
            }
            return false;
        }

        public bool UpdateReview(Review review)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    db.Reviews.Attach(review);
                    db.Entry<Review>(review).State = EntityState.Modified;
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return false;
        }

        public bool? FlipPublishOrUnpublish(int reviewId)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    Review r = db.Reviews.FirstOrDefault(x => x.Id == reviewId);
                    r.Publish = !r.Publish;
                    db.SaveChanges();
                    return r.Publish;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return null;
        }

        public bool DeleteReview(int reviewId)
        {
            try
            {
                using (var db = new SupperClubContext())
                {
                    Review r = db.Reviews.FirstOrDefault(x => x.Id == reviewId);
                    db.Reviews.Remove(r);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return false;
        }


        #endregion

        #region CMS

        public PageCMS AddUpdatePageCMS(PageCMS pageCMS)
        {
            PageCMS _pageCMS = null;

            try
            {
                using (var db = new SupperClubContext())
                {
                    _pageCMS = db.PageCMS.Add(pageCMS);
                    db.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }

            return _pageCMS;

        }

        public IList<PageCMS> GetCMSDetailsByPage(string pageName)
        {
            IList<PageCMS> pageCms = null;
            try
            {
                using (var db = new SupperClubContext())
                {

                    pageCms = (from x in db.PageCMS
                               where x.Page == pageName

                               select x).ToList<PageCMS>();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
            return pageCms;
        }

        public bool UpdatePageCMS(PageCMS pageCMS)
        {
            try
            {
                var db = new SupperClubContext();
                //Update the SupperClub table
                db.Entry(pageCMS).State = EntityState.Modified;
                //Update other tables

                db.SaveChanges();

                return true;
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
                return false;
            }
        }

        public int GetAdminRankByEventId(int EventId)
        {
            int rank = 9999;
            try
            {
                using (var db = new SupperClubContext())
                {
                    rank = (from adminrank in db.PopularEventAdminRank
                            where adminrank.EventId == EventId
                            select adminrank.Rank).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);

            }
            if (rank == 0 || rank == null)
                rank = 9999;
            return rank;
        }

        public string GetAdminUrlByEventId(int EventId)
        {

            string eventUrl = string.Empty;
            try
            {
                using (var db = new SupperClubContext())
                {
                    eventUrl = (from adminrank in db.PopularEventAdminRank
                                where adminrank.EventId == EventId
                                select adminrank.EventUrl).FirstOrDefault();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);

            }
            return eventUrl;

        }

        public IList<PopularEventAdmin> GetPopularEventDetailsWithAdminRanks()
        {
            IList<PopularEventAdmin> events = (from p in db.PopularEvents.Include("Events")
                                               join e in db.Events.Include("SupperClubs") on p.EventId equals e.Id
                                               join pa in db.PopularEventAdminRank on p.EventId equals pa.EventId into gj
                                               from subRank in gj.DefaultIfEmpty()
                                               where e.Start > DateTime.Now && e.Status == (int)EventStatus.Active && e.Private == false
                                               select new PopularEventAdmin
                                               {
                                                   popularEvent = p,
                                                   AdminRank = (subRank.Rank == null || subRank.Rank == 0) ? 0 : subRank.Rank,
                                                   EventUrl = subRank.EventUrl
                                               }).Distinct().OrderBy(x => x.AdminRank == 0).ThenBy(c => c.AdminRank).ThenBy(x => x.popularEvent.Status).ThenBy(y => y.popularEvent.Rank).ToList<PopularEventAdmin>();
            return events;
        }
        public IList<PopularEventAdmin> GetPopularEventsWithAdminRank()
        {


            //List<PopularEventAdmin> PopularEventAdmin = new List<PopularEventAdmin>();


            //List<PopularEventAdminRank> PopularEventAdminRank = new List<PopularEventAdminRank>();

            //using (var db = new SupperClubContext())
            //{
            //    IList<PopularEvent> _eventlst = GetAllActivePopularEvents();
            //    foreach (PopularEvent e in _eventlst)
            //    {
            //        PopularEventAdminRank rank = new PopularEventAdminRank();
            //        PopularEventAdmin eAdmin = new PopularEventAdmin();

            //        eAdmin.popularEvent = e;
            //        int adminrank = GetAdminRankByEventId(e.EventId);
            //        eAdmin.AdminRank = (adminrank == 9999) ? 0 : adminrank;

            //        string eventUrl = GetAdminUrlByEventId(e.EventId);
            //        if (string.IsNullOrEmpty(eventUrl))
            //        {
            //            eventUrl = e.Event.SupperClub.UrlFriendlyName + "/" + e.Event.UrlFriendlyName + "/" + e.EventId;
            //        }
            //        eAdmin.EventUrl = eventUrl;

            //        PopularEventAdmin.Add(eAdmin);
            //        rank.EventId = e.EventId;
            //        rank.Rank = (eAdmin.AdminRank == null) ? 0 : eAdmin.AdminRank;
            //        rank.EventUrl = eventUrl;
            //        PopularEventAdminRank.Add(rank);
            //    }
            //}

            //return PopularEventAdmin;
            IList<PopularEventAdmin> events = (from p in db.PopularEvents.Include("Events")
                                               join e in db.Events.Include("SupperClubs") on p.EventId equals e.Id
                                               join pa in db.PopularEventAdminRank on p.EventId equals pa.EventId into gj
                                               from subRank in gj.DefaultIfEmpty()
                                               where e.Start > DateTime.Now && e.Status == (int)EventStatus.Active && e.Private == false && p.Status != (int)PopularEventStatus.New
                                               select new PopularEventAdmin
                                               {
                                                   popularEvent = p,
                                                   AdminRank = (subRank.Rank == null || subRank.Rank == 0) ? 0 : subRank.Rank,
                                                   EventUrl = (string.IsNullOrEmpty(subRank.EventUrl)) ? e.SupperClub.UrlFriendlyName + "/" + e.UrlFriendlyName + "/" + e.Id.ToString() : subRank.EventUrl,
                                               }).Distinct().OrderBy(x => x.AdminRank == 0).ThenBy(c => c.AdminRank).ThenBy(x => x.popularEvent.Status).ThenBy(y => y.popularEvent.Rank).ToList<PopularEventAdmin>();
            return events;
        }

        public PopularEventAdminRank CreatePopularEventAdminRank(PopularEventAdminRank popularEventsAdmin)
        {
            PopularEventAdminRank _popularEventsAdmin = null;
            try
            {
                using (var db = new SupperClubContext())
                {
                    _popularEventsAdmin = db.PopularEventAdminRank.Add(popularEventsAdmin);
                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }

            return _popularEventsAdmin;
        }


        public void UpdatePopularEventAdminRank(IList<PopularEventAdminRank> PopularEventsAdmin)
        {


            try
            {

                using (var db = new SupperClubContext())
                {

                    db.PopularEventAdminRank.ToList().ForEach((r) => db.PopularEventAdminRank.Remove(r));

                    db.SaveChanges();
                    foreach (PopularEventAdminRank rank in PopularEventsAdmin)
                    {

                        db.PopularEventAdminRank.Add(rank);


                    }

                    db.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                log.Fatal(ex.Message, ex);
            }
        }


        #endregion



    }


}
