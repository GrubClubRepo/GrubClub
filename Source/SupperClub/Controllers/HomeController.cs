﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SupperClub.Domain.Repository;
using SupperClub.Models;
using SupperClub.Domain;
using SupperClub.Code;
using SupperClub.Web.Helpers;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Xml;
using System.Web.Security;
using SupperClub.Logger;
using SupperClub.Services;
using System.Configuration;
using System.Globalization;

namespace SupperClub.Controllers
{
    [SupperClub.Web.Helpers.Utils.RequireHttpsInProduction]
    public class HomeController : BaseController
    {

        public HomeController(ISupperClubRepository supperClubRepository)
        {
            _supperClubRepository = supperClubRepository;
        }

        public ActionResult Foodies()
        {
            NewCMSHomeModel model = new NewCMSHomeModel();
            model.PageCMS = _supperClubRepository.GetCMSDetailsByPage("Foodies");
            ViewBag.Hero = model.PageCMS.Where(x => x.Section == "Hero").ToList<PageCMS>();
            ViewBag.Curated = model.PageCMS.Where(x => x.Section == "Curated").ToList<PageCMS>();
            ViewBag.OurStories = model.PageCMS.Where(x => x.Section == "OurStories").ToList<PageCMS>();
            return View("Foodies",model);
        }
        public ActionResult Home()
        {
            NewCMSHomeModel model = new NewCMSHomeModel();
            model.PageCMS = _supperClubRepository.GetCMSDetailsByPage("Home");
            ViewBag.Hero = model.PageCMS.Where(x => x.Section == "Hero").ToList<PageCMS>();
            ViewBag.Curated = model.PageCMS.Where(x => x.Section == "Curated").ToList<PageCMS>();
            ViewBag.OurStories = model.PageCMS.Where(x => x.Section == "OurStories").ToList<PageCMS>();
            return View("Home", model);
        }
        public ActionResult HomeTest()
        {
            NewCMSHomeModel model = new NewCMSHomeModel();
            model.PageCMS = _supperClubRepository.GetCMSDetailsByPage("HomeTest");
            ViewBag.Hero = model.PageCMS.Where(x => x.Section == "Hero").ToList<PageCMS>();
            ViewBag.Curated = model.PageCMS.Where(x => x.Section == "Curated").ToList<PageCMS>();
            ViewBag.OurStories = model.PageCMS.Where(x => x.Section == "OurStories").ToList<PageCMS>();
            return View("HomeTest", model);
        }
        public ActionResult UnderMaintenance()
        {
            return View();
        }
        public ActionResult News()
        {
            return View();
        }
        public ActionResult Vouchers()
        {
            return View();
        }

        public ActionResult Gallery()
        {
            return View();
        }

        public ActionResult Faqs()
        {
            return View();
        }

        public ActionResult BecomeAHost()
        {
            return View();
        }

        public ActionResult Partners()
        {
            return View();
        }
        public ActionResult OldHome()
        {
            var recentlyAddedEvents = _supperClubRepository.GetRecentlyAddedEvents();
            HomePage model = new HomePage();
            
            model.RecentlyAddedEvents = recentlyAddedEvents;            
           
            if (recentlyAddedEvents != null && recentlyAddedEvents.Count > 0)
            {
                int counter = recentlyAddedEvents.Count > 4 ? 4 : recentlyAddedEvents.Count;
                for (int i = 0; i < counter; i++)
                {
                    model.RecentlyAddedEvents[i].Description = model.RecentlyAddedEvents[i].EventDescription;
                    if (model.RecentlyAddedEvents[i].Description.Length > 270)
                    {
                        bool linkFound = false;
                        if (model.RecentlyAddedEvents[i].Description.Substring(0, 270).IndexOf("<a") == -1 || (System.Text.RegularExpressions.Regex.Matches(model.RecentlyAddedEvents[i].Description.Substring(0, 270), "<a").Count == System.Text.RegularExpressions.Regex.Matches(model.RecentlyAddedEvents[i].Description.Substring(0, 270), "</a>").Count))
                        {
                            model.RecentlyAddedEvents[i].Description = model.RecentlyAddedEvents[i].Description.Substring(0, 270) + "...";
                        }
                        else
                        {
                            int indexer = System.Text.RegularExpressions.Regex.Matches(model.RecentlyAddedEvents[i].Description.Substring(0, 270), "<a").Count;
                            int requiredStringLength = Utils.findStringNthIndex(model.RecentlyAddedEvents[i].Description, "</a>", indexer);
                            model.RecentlyAddedEvents[i].Description = model.RecentlyAddedEvents[i].Description.Substring(0, requiredStringLength + 4) + "...";
                            linkFound = true;
                        }
                        if (model.RecentlyAddedEvents[i].Description.Substring(0, 270).IndexOf("<strong>") == -1 || (System.Text.RegularExpressions.Regex.Matches(model.RecentlyAddedEvents[i].Description.Substring(0, 270), "<strong>").Count == System.Text.RegularExpressions.Regex.Matches(model.RecentlyAddedEvents[i].Description.Substring(0, 270), "</strong>").Count))
                        {
                            if (!linkFound)
                                model.RecentlyAddedEvents[i].Description = model.RecentlyAddedEvents[i].Description.Substring(0, 270) + "...";
                        }
                        else
                        {
                            if (!linkFound)
                                model.RecentlyAddedEvents[i].Description = model.RecentlyAddedEvents[i].Description.Substring(0, 270) + "</strong>...";
                            else
                                model.RecentlyAddedEvents[i].Description = model.RecentlyAddedEvents[i].Description + "</strong>...";
                        }
                    }
                }
            }
            
            //GeoLocation loc = UserMethods.GetAddressCoordinates("NW1 9SA");
            ////var results = _supperClubRepository.SearchEvent(loc);
            //Set default values
            model.HomeSearch = new SearchModel();

            // Client request to remove defaults. I'm sure he'll want to change it back.
            model.HomeSearch.StartDate = DateTime.Now;
            model.HomeSearch.Location = "";
            model.HomeSearch.Guests = 2;
                       
            return View("OldIndex", model);
        }

        [HttpPost]
        public ActionResult OldHome(HomePage model)
        {
            if (model == null)
            {
                model = new HomePage();                
            }
            if(model.HomeSearch == null)
            {
                model.HomeSearch = new SearchModel();
                //Set default values
                model.HomeSearch.StartDate = DateTime.Now;
                model.HomeSearch.Location = "";
                model.HomeSearch.Guests = 2;                               
            }
            
            return RedirectToAction("SearchResult", "Search", new { searchParam = model.HomeSearch });
        }

        public ActionResult Index()
        {
            //var model= new List<object>();
            //IList<PageCMS>  homePageCMS = _supperClubRepository.GetCMSDetailsByPage("Home");
            
            //for( int i=0; i < homePageCMS.Count; i++)
            //{
            //    var temp= homePageCMS[i];
            //    model.Add(new
            //    {

            //        section = homePageCMS[i].Section,
            //        text1 = homePageCMS[i].TextLine1,
            //        text2 = homePageCMS[i].TextLine2,
            //        link = homePageCMS[i].Link,
            //        imagePath = homePageCMS[i].ImagePath
            //    });
                
            //}
            NewCMSHomeModel model = new NewCMSHomeModel();
             model.PageCMS = _supperClubRepository.GetCMSDetailsByPage("Home");
             ViewBag.Hero = model.PageCMS.Where(x => x.Section == "Hero").ToList<PageCMS>();
             ViewBag.Curated = model.PageCMS.Where(x => x.Section == "Curated").ToList<PageCMS>();
             ViewBag.OurStories = model.PageCMS.Where(x => x.Section == "OurStories").ToList<PageCMS>();
            return View("Home",model);
        }
        public ActionResult WhatPeopleAreSaying()
        {
            PressReleaseModel model = new PressReleaseModel();

            model.PressReleases = _supperClubRepository.GetPressReleases();
            
            return View(model);

        }

private SupperClubs getSupperClubsFromSupperClub(Domain.SupperClub s1)
        {
            SupperClubs sc1 = new SupperClubs();

            sc1.Active = s1.Active;
            sc1.Description = s1.Description;
            sc1.FirstName = s1.User.FirstName;
            if (SupperClub.Code.UserMethods.IsLoggedIn)
            {
                sc1.FollowChef = _supperClubRepository.IsCurrentSupperClubUsersFavourite(s1.Id, UserMethods.CurrentUser.Id) == true ? 1 : 0;
            }
            else
            {
                sc1.FollowChef = 0;
            }
            sc1.Followers = _supperClubRepository.GetSupperClubFollowers(s1.Id);
            sc1.GrubClubName = s1.Name;
            sc1.GrubClubUrlFriendlyName = s1.UrlFriendlyName;
            sc1.ImagePath = s1.ImagePath;
            sc1.LastName = s1.User.LastName;
            sc1.Rating = s1.AverageRank??0;
            sc1.ReviewCount = s1.NumberOfReviews;
            sc1.Summary = s1.Summary;
            sc1.SupperclubId = s1.Id;

            return sc1;
        }
        public ActionResult Chefs()
        {
            ChefsModel model = new ChefsModel();

            Domain.SupperClub s1 = _supperClubRepository.GetSupperClub("a-little-lusciousness");
            Domain.SupperClub s2 = _supperClubRepository.GetSupperClub("that-hungry-chef");
            Domain.SupperClub s3 = _supperClubRepository.GetSupperClub("smoke-and-salt");
            Domain.SupperClub s4 = _supperClubRepository.GetSupperClub("a-taste");

            SupperClubs sc1 = getSupperClubsFromSupperClub(s1);
            SupperClubs sc2 = getSupperClubsFromSupperClub(s2);
            SupperClubs sc3 = getSupperClubsFromSupperClub(s3);
            SupperClubs sc4 = getSupperClubsFromSupperClub(s4);
            model.SupperClubs = new List<SupperClubs>();
            model.SupperClubs.Add(sc1);
            model.SupperClubs.Add(sc2);
            model.SupperClubs.Add(sc3);
            model.SupperClubs.Add(sc4);
            //if (SupperClub.Code.UserMethods.IsLoggedIn)
            //{
            //    var result = _supperClubRepository.GetAllSupperClubDetails(SupperClub.Code.UserMethods.UserId);
            //    model.SupperClubs = result.ToList<SupperClubs>().OrderByDescending(x => x.Rating).ThenByDescending(x => x.ReviewCount).Take(4).ToList();
            //}
            //else
            //{
            //    var result = _supperClubRepository.GetAllSupperClubDetails();
            //    model.SupperClubs = result.ToList<SupperClubs>().OrderByDescending(x => x.Rating).ThenByDescending(x => x.ReviewCount).Take(4).ToList();
            //}
            // var result = _supperClubRepository.GetAllSupperClubDetails();
            
            return View(model);
        }

        [HttpPost]

        public ActionResult Chefs_Init(string Option, string Keyword)
        {
            IList<SupperClub.Domain.SupperClubs> displayResults;
            if (Option == "2")
            {
                displayResults = new List<SupperClubs>();               
                //IList<PopularEvent> popularEvents= new List<PopularEvent>();
                //popularEvents = _supperClubRepository.GetAllActivePopularEvents();
                //List<int> lstSupperclubIds = popularEvents.Select(x => x.Event.SupperClubId).ToList();
                List<SupperClub.Domain.SupperClubs> lstSupperClubs = _supperClubRepository.GetAllSupperClubDetails(Keyword).ToList<SupperClubs>().Where(y => y.Active && y.HasFutureEvents).OrderByDescending(x => x.FutureEventCount).ThenByDescending(x => x.Rating).ThenByDescending(x => x.ReviewCount).ToList();
                displayResults = lstSupperClubs.ToList();
                //foreach (int id in lstSupperclubIds)
                //{
                //    SupperClub.Domain.SupperClubs sclub = new SupperClubs();
                //    sclub = lstSupperClubs.Where(x => x.SupperclubId == id).FirstOrDefault();
                //    if (sclub != null)
                //    {
                //        displayResults.Add(sclub);
                //        lstSupperClubs.Remove(sclub);
                //    }
                //}
               
                //SupperClub.Domain.SupperClubs st = new Domain.SupperClubs();
                //st.GrubClubName = "NewLine";
                //displayResults.Add(st);
                //foreach (SupperClubs s in lstSupperClubs)
                //    displayResults.Add(s);
            }
            else
            {
                var result = _supperClubRepository.GetAllSupperClubDetails(Keyword);
                if (Option == "0")
                {
                    displayResults = result.ToList<SupperClubs>().Where(y => y.Active).OrderByDescending(x => x.Rating).ThenByDescending(x => x.ReviewCount).ToList();
                }
                else
                {
                    displayResults = result.ToList<SupperClubs>().Where(y => y.Active).OrderBy(x => x.FirstName).ToList();
                }
            }
            return Json(displayResults, JsonRequestBehavior.AllowGet);
        }


        //[HttpPost]
        //public ActionResult Home_Init()
        //{
        //    LogMessage("Get popular event list");
            
        //    //List<PopularEvent> lstpopulareEvents = _supperClubRepository.GetAllActivePopularEvents().ToList<PopularEvent>();
        //    //List<PopularEventRank> lstpopularEventAdminRank = new List<PopularEventRank>();
        //    //foreach (PopularEvent e in lstpopulareEvents)
        //    //{
        //    //    PopularEventRank _adminEvent = new PopularEventRank();
        //    //    _adminEvent.Event = e.Event;
        //    //    _adminEvent.EventId = e.EventId;
        //    //    _adminEvent.Rank = e.Rank;
        //    //    _adminEvent.Status = e.Status;
        //    //    _adminEvent.AdminRank = _supperClubRepository.GetAdminRankByEventId(e.EventId);
        //    //    string eventURL = _supperClubRepository.GetAdminUrlByEventId(e.EventId);
        //    //    if (string.IsNullOrEmpty(eventURL))
        //    //        _adminEvent.EventUrl = "/" + e.Event.UrlFriendlyName + "/" + e.EventId;
        //    //    else
        //    //        _adminEvent.EventUrl = "/" + eventURL;
        //    //    lstpopularEventAdminRank.Add(_adminEvent);
        //    //}

        //    //var displayResults = new List<object>();

        //    IList<PopularEventAdmin> displayResults = new List<PopularEventAdmin>();
        //    string cacheKey = "PopularEvents";
        //    if (System.Web.HttpRuntime.Cache[cacheKey] != null)
        //    {
        //        displayResults = (IList<PopularEventAdmin>)System.Web.HttpRuntime.Cache[cacheKey];
        //        if (displayResults == null || displayResults.Count <= 0)
        //        {
        //            System.Web.HttpRuntime.Cache.Remove("PopularEvents");
        //            RedirectToAction("Home_Init");
        //        }
        //    }
        //    else
        //    {
        //        displayResults = _supperClubRepository.GetPopularEventDetailsWithAdminRanks();
        //        if(displayResults != null && displayResults.Count > 0)
        //            System.Web.HttpRuntime.Cache.Insert(cacheKey, displayResults, null, DateTime.Now.AddMinutes(60), TimeSpan.Zero);
        //    }
        //    IList<int> userFavouriteEventIds = new List<int>();
        //    if (SupperClub.Code.UserMethods.IsLoggedIn)
        //    {
        //        userFavouriteEventIds = _supperClubRepository.GetFavouriteEventIdsForAUser(UserMethods.CurrentUser.Id);
        //    }
        //    var results = new List<object>();
        //    if (displayResults != null && displayResults.Count > 0)
        //    {
        //        foreach (PopularEventAdmin _event in displayResults)
        //        {
        //            Event tempEvent = _event.popularEvent.Event;
        //            results.Add(new
        //            {
        //                EventId = tempEvent.Id,
        //                EventName = tempEvent.Name,
        //                EventDate = tempEvent.Start.ToString("ddd, d MMM"),
        //                Cost = tempEvent.CostToGuest.ToString(),
        //                GrubClubUrlFriendlyName = tempEvent.SupperClub.UrlFriendlyName,
        //                TotalSeats = tempEvent.TotalEventGuests,
        //                GuestsAttending = tempEvent.TotalNumberOfAttendeeGuests,
        //                EventImage = tempEvent.ImagePath,
        //                Rating = tempEvent.SupperClub.AverageRank,
        //                ReviewCount = tempEvent.SupperClub.NumberOfReviews,
        //                EventUrlFriendlyName = tempEvent.UrlFriendlyName,
        //                EarlyBird = tempEvent.EarlyBird == null ? 0 : tempEvent.EarlyBird,
        //                BrandNew = (tempEvent.SupperClub.Events.Count <= 2) ? "1" : "0",
        //                AvailableSeats = tempEvent.TotalNumberOfAvailableSeats,
        //                WishEvent = SupperClub.Code.UserMethods.IsLoggedIn ? (userFavouriteEventIds.Contains(tempEvent.Id) ? "1" : "0") : "0",
        //                EventUrl = _event.EventUrl == null ? ("/" + tempEvent.SupperClub.UrlFriendlyName + "/" + tempEvent.UrlFriendlyName + "/" + tempEvent.Id.ToString()) : _event.EventUrl
        //                // AdminRank = (AdminRank == null) ? 0 : AdminRank
        //            });
        //        }
        //    }
        //    return Json(results, JsonRequestBehavior.AllowGet);
            
        //}

        [HttpPost]
        public ActionResult Home_Init(bool forceReload = false)
        {
            LogMessage("Get popular event list");

            //List<PopularEvent> lstpopulareEvents = _supperClubRepository.GetAllActivePopularEvents().ToList<PopularEvent>();
            //List<PopularEventRank> lstpopularEventAdminRank = new List<PopularEventRank>();
            //foreach (PopularEvent e in lstpopulareEvents)
            //{
            //    PopularEventRank _adminEvent = new PopularEventRank();
            //    _adminEvent.Event = e.Event;
            //    _adminEvent.EventId = e.EventId;
            //    _adminEvent.Rank = e.Rank;
            //    _adminEvent.Status = e.Status;
            //    _adminEvent.AdminRank = _supperClubRepository.GetAdminRankByEventId(e.EventId);
            //    string eventURL = _supperClubRepository.GetAdminUrlByEventId(e.EventId);
            //    if (string.IsNullOrEmpty(eventURL))
            //        _adminEvent.EventUrl = "/" + e.Event.UrlFriendlyName + "/" + e.EventId;
            //    else
            //        _adminEvent.EventUrl = "/" + eventURL;
            //    lstpopularEventAdminRank.Add(_adminEvent);
            //}

            //var displayResults = new List<object>();

            IList<PopularEventAdmin> displayResults = new List<PopularEventAdmin>();
            string cacheKey = "PopularEvents";
            if (!forceReload && System.Web.HttpRuntime.Cache[cacheKey] != null)
            {
                displayResults = (IList<PopularEventAdmin>)System.Web.HttpRuntime.Cache[cacheKey];
                if (displayResults == null || displayResults.Count <= 0)
                {
                    System.Web.HttpRuntime.Cache.Remove(cacheKey);
                    RedirectToAction("Home_Init", new { forceReload = true });
                }
            }
            else
            {
                displayResults = _supperClubRepository.GetPopularEventDetailsWithAdminRanks();
                if (displayResults != null && displayResults.Count > 0)
                {
                    if(System.Web.HttpRuntime.Cache[cacheKey] != null)
                        System.Web.HttpRuntime.Cache.Remove(cacheKey);
                    System.Web.HttpRuntime.Cache.Insert(cacheKey, displayResults, null, DateTime.Now.AddMinutes(60), TimeSpan.Zero);
                }
            }
            IList<int> userFavouriteEventIds = new List<int>();
            if (SupperClub.Code.UserMethods.IsLoggedIn)
            {
                userFavouriteEventIds = _supperClubRepository.GetFavouriteEventIdsForAUser(UserMethods.CurrentUser.Id);
            }
            var results = new List<object>();
            if (displayResults != null && displayResults.Count > 0)
            {
                foreach (PopularEventAdmin _event in displayResults)
                {
                    Event tempEvent = null;
                    if (_event.popularEvent.Event != null)
                        tempEvent = _event.popularEvent.Event;
                    else
                        tempEvent = _supperClubRepository.GetEvent(_event.popularEvent.EventId);
                    results.Add(new
                    {
                        EventId = tempEvent.Id,
                        EventName = tempEvent.Name,
                        EventDate = tempEvent.Start.ToString("ddd, d MMM"),
                        Cost = tempEvent.CostToGuest.ToString(),
                        GrubClubUrlFriendlyName = tempEvent.SupperClub.UrlFriendlyName,
                        TotalSeats = tempEvent.TotalEventGuests,
                        GuestsAttending = tempEvent.TotalNumberOfAttendeeGuests,
                        EventImage = tempEvent.ImagePath,
                        Rating = tempEvent.SupperClub.AverageRank,
                        ReviewCount = tempEvent.SupperClub.NumberOfReviews,
                        EventUrlFriendlyName = tempEvent.UrlFriendlyName,
                        EarlyBird = tempEvent.EarlyBird == null ? 0 : tempEvent.EarlyBird,
                        BrandNew = (tempEvent.SupperClub.Events.Count <= 2) ? "1" : "0",
                        AvailableSeats = tempEvent.TotalNumberOfAvailableSeats,
                        WishEvent = SupperClub.Code.UserMethods.IsLoggedIn ? (userFavouriteEventIds.Contains(tempEvent.Id) ? "1" : "0") : "0",
                        EventUrl = _event.EventUrl == null ? ("/" + tempEvent.SupperClub.UrlFriendlyName + "/" + tempEvent.UrlFriendlyName + "/" + tempEvent.Id.ToString()) : _event.EventUrl
                        // AdminRank = (AdminRank == null) ? 0 : AdminRank
                    });
                }
            }
            return Json(results, JsonRequestBehavior.AllowGet);

        }
        //public ActionResult Index()
        //{
        //    var recentlyAddedEvents = _supperClubRepository.GetRecentlyAddedEvents();
        //    NewHome model = new NewHome();
        //    //add press releases details
        //    model.PressReleases = _supperClubRepository.GetPressReleases();
        //    model.FavouriteEvent = _supperClubRepository.GetFavouriteEvent();

        //    model.RecentlyAddedEvents = recentlyAddedEvents;

        //    if (recentlyAddedEvents != null && recentlyAddedEvents.Count > 0)
        //    {
        //        int counter = recentlyAddedEvents.Count > 4 ? 4 : recentlyAddedEvents.Count;
        //        for (int i = 0; i < counter; i++)
        //        {
        //            if (model.RecentlyAddedEvents[i].Description.Length > 270)
        //            {
        //                bool linkFound = false;
        //                if (model.RecentlyAddedEvents[i].Description.Substring(0, 270).IndexOf("<a") == -1 || (System.Text.RegularExpressions.Regex.Matches(model.RecentlyAddedEvents[i].Description.Substring(0, 270), "<a").Count == System.Text.RegularExpressions.Regex.Matches(model.RecentlyAddedEvents[i].Description.Substring(0, 270), "</a>").Count))
        //                {
        //                    model.RecentlyAddedEvents[i].Description = model.RecentlyAddedEvents[i].Description.Substring(0, 270) + "...";
        //                }
        //                else
        //                {
        //                    int indexer = System.Text.RegularExpressions.Regex.Matches(model.RecentlyAddedEvents[i].Description.Substring(0, 270), "<a").Count;
        //                    int requiredStringLength = Utils.findStringNthIndex(model.RecentlyAddedEvents[i].Description, "</a>", indexer);
        //                    model.RecentlyAddedEvents[i].Description = model.RecentlyAddedEvents[i].Description.Substring(0, requiredStringLength + 4) + "...";
        //                    linkFound = true;
        //                }
        //                if (model.RecentlyAddedEvents[i].Description.Substring(0, 270).IndexOf("<strong>") == -1 || (System.Text.RegularExpressions.Regex.Matches(model.RecentlyAddedEvents[i].Description.Substring(0, 270), "<strong>").Count == System.Text.RegularExpressions.Regex.Matches(model.RecentlyAddedEvents[i].Description.Substring(0, 270), "</strong>").Count))
        //                {
        //                    if (!linkFound)
        //                        model.RecentlyAddedEvents[i].Description = model.RecentlyAddedEvents[i].Description.Substring(0, 270) + "...";
        //                }
        //                else
        //                {
        //                    if (!linkFound)
        //                        model.RecentlyAddedEvents[i].Description = model.RecentlyAddedEvents[i].Description.Substring(0, 270) + "</strong>...";
        //                    else
        //                        model.RecentlyAddedEvents[i].Description = model.RecentlyAddedEvents[i].Description + "</strong>...";
        //                }
        //            }
        //        }
        //    }

        //    // Check for Favourite event
        //    if (model.FavouriteEvent != null && model.FavouriteEvent.Description.Length > 270)
        //    {
        //        bool linkFound = false;
        //        if (model.FavouriteEvent.Description.Substring(0, 270).IndexOf("<a") == -1 || (System.Text.RegularExpressions.Regex.Matches(model.FavouriteEvent.Description.Substring(0, 270), "<a").Count == System.Text.RegularExpressions.Regex.Matches(model.FavouriteEvent.Description.Substring(0, 270), "</a>").Count))
        //        {
        //            model.FavouriteEvent.Description = model.FavouriteEvent.Description.Substring(0, 270) + "...";
        //        }
        //        else
        //        {
        //            int indexer = System.Text.RegularExpressions.Regex.Matches(model.FavouriteEvent.Description.Substring(0, 270), "<a").Count;
        //            int requiredStringLength = Utils.findStringNthIndex(model.FavouriteEvent.Description, "</a>", indexer);
        //            model.FavouriteEvent.Description = model.FavouriteEvent.Description.Substring(0, requiredStringLength + 4) + "...";
        //            linkFound = true;
        //        }
        //        if (model.FavouriteEvent.Description.Substring(0, 270).IndexOf("<strong>") == -1 || (System.Text.RegularExpressions.Regex.Matches(model.FavouriteEvent.Description.Substring(0, 270), "<strong>").Count == System.Text.RegularExpressions.Regex.Matches(model.FavouriteEvent.Description.Substring(0, 270), "</strong>").Count))
        //        {
        //            if (!linkFound)
        //                model.FavouriteEvent.Description = model.FavouriteEvent.Description.Substring(0, 270) + "...";
        //        }
        //        else
        //        {
        //            if (!linkFound)
        //                model.FavouriteEvent.Description = model.FavouriteEvent.Description.Substring(0, 270) + "</strong>...";
        //            else
        //                model.FavouriteEvent.Description = model.FavouriteEvent.Description + "</strong>...";
        //        }
        //    }
        //    //GeoLocation loc = UserMethods.GetAddressCoordinates("NW1 9SA");
        //    ////var results = _supperClubRepository.SearchEvent(loc);

        //    model.Location = "London, UK";

        //  //  return View("Index", model);
        //    return View("Home", model);
        //}

        [HttpPost]
        public ActionResult Index(NewHome model)
        {
            if (model == null)
            {
                model = new NewHome();
                model.Location = "London";
            }
            SearchModel sm = new SearchModel();
            sm.Location = model.Location;
            if (!string.IsNullOrEmpty(model.DateString))
            {
                //Separator
                string[] fieldSeparators = new string[] { "|" };
                string[] fields = model.DateString.Split(fieldSeparators, StringSplitOptions.None);
                DateTime StartDate = string.IsNullOrEmpty(fields[0]) ? DateTime.Now : DateTime.Parse(fields[0]);
                DateTime EndDate = string.IsNullOrEmpty(fields[1]) ? StartDate.AddDays(30) : DateTime.Parse(fields[1]);
                int dayOffset = (EndDate - StartDate).Days;
                sm.StartDate = StartDate;
                sm.EndDateOffset = dayOffset;
            }
            else
            {
                sm.StartDate = DateTime.Now;
                sm.EndDateOffset = 100;
            }

            return RedirectToAction("SearchResult", "Search", new { searchParam = model });
        }

        public ActionResult NewHomePage()
        {
            var tileTags = _supperClubRepository.GetTileTags();
            NewHomePage model = new NewHomePage();
            model.TileCollection = tileTags;
            model.PressReleases = _supperClubRepository.GetPressReleases();
            
            //Set default values
            model.HomeSearch = new SearchModel();

            // Client request to remove defaults. I'm sure he'll want to change it back.
            model.HomeSearch.StartDate = DateTime.Now;
            model.HomeSearch.Location = "";
            model.HomeSearch.Guests = 2;

            return View("NewHome", model);
        }

        [HttpPost]
        public ActionResult NewHomePage(NewHomePage model)
        {
            if (model == null)
                model = new NewHomePage();
            if (model.HomeSearch == null)
            {
                model.HomeSearch = new SearchModel();
                //Set default values
                model.HomeSearch.StartDate = DateTime.Now;                
            }
            model.HomeSearch.Location = "";
            model.HomeSearch.Guests = 2;
            return RedirectToAction("SearchResult", "Search", new { searchParam = model.HomeSearch });
        }

        public ActionResult ViewQuickLink(string viewname)
        {
            var url = _supperClubRepository.GetUrl(viewname);
            return View(url.ActualUrl);
        }
        public ActionResult ViewQuickLinkOld(string viewname)
        {
            return RedirectToActionPermanent("ViewQuickLink", new { viewname = viewname });
        }

        public ActionResult FoodiesRedirect()
        {
            return RedirectToActionPermanent("Foodies");
        }
        public ActionResult AboutUsRedirect()
        {
            return RedirectToActionPermanent("ViewQuickLink", new { viewname = "about-us" });
        }
        public ActionResult HowItWorksRedirect()
        {
            return RedirectToActionPermanent("ViewQuickLink", new { viewname = "how-it-works" });
        }
        public ActionResult ReferAFriendRedirect()
        {
            return RedirectToActionPermanent("ViewQuickLink", new { viewname = "refer-a-friend" });
        }
        public ActionResult ContactUsRedirect()
        {
            return RedirectToActionPermanent("ContactUs");
        }
        public ActionResult GiftVoucherRedirect()
        {
            return RedirectToActionPermanent("GiftVoucher");
        }
        public ActionResult ReferralVoucherRedirect()
        {
            return RedirectToActionPermanent("ReferralVoucher");
        }
        public ActionResult ChefsRedirect()
        {
            return RedirectToActionPermanent("Chefs");
        } 
        public ActionResult ContactUs(string messageText = "I've got some feedback and it's about ")
        {
            //ContactUsModel model = new ContactUsModel();
            //if (messageText != null)
            //    model.Message = messageText;
            //return View(model);
            return View("Contact");
        }
        [HttpPost]
        public ActionResult ContactUs(ContactUsModel model)
        {
            EmailService.EmailService emailer = new EmailService.EmailService(_supperClubRepository);
            bool success = emailer.SendAdminContactUsEmail(model.Name, model.Email, model.Message);
            if (success)
            {
                SetNotification(NotificationType.Success, "Your message has been sent.");
                return View("Contact");
            }
            else
            {
                SetNotification(NotificationType.Error, "Your message was not sent, please try again.", true, false);
                return View(model);
            }
        }

        public ActionResult EmailSignup()
        {
            NewsletterSubscriberModel nsm = new NewsletterSubscriberModel();
            nsm.Status = "";
            return View(nsm);
        }

        [HttpPost]
        public ActionResult EmailSignup(NewsletterSubscriberModel model)
        {
            bool success = addSubscriber(model.Email, SupperClub.Domain.SubscriberType.SocialMedia, model.FirstName, model.LastName);
            if (success)
            {
                //SetNotification(NotificationType.Success, "Welcome to the Club! We'll keep you updated about our new events and any other news!");
                model.Status = "Welcome to the Club! We'll keep you updated about our new events and any other news!";                   
            }
            else
            {
                //SetNotification(NotificationType.Error, "Looks like there was a problem adding your email address, please try again!", true, false);
                model.Status = "Looks like there was a problem adding your email address, please try again!";                
            }
            return View(model);
        }

        public ActionResult SiteMapOld()
        {
            Dictionary<string, string> scUrls = new Dictionary<string,string>();
            foreach(SupperClub.Domain.SupperClub sc in _supperClubRepository.GetAllActiveSupperClubs())
                scUrls.Add(sc.Name, sc.UrlFriendlyName);
            ViewBag.SupperClubsFriendlyUrls = scUrls;

            Dictionary<string, string> otherUrls = new Dictionary<string, string>();
            foreach (UrlRewrite ur in _supperClubRepository.GetUrlRewrites())
                otherUrls.Add(ur.ActualUrl, ur.RewriteUrl);
            ViewBag.OtherFriendlyUrls = otherUrls;

            return View("Sitemap");
        }

        public ActionResult SiteMap()
        {
            XmlDocument xd = null;
            string sSiteMapFilePath = HttpRuntime.AppDomainAppPath + "sitemap\\sitemap.xml";
            FileInfo fi = new FileInfo(sSiteMapFilePath);
            // List of links
            Dictionary<string, string> scUrls = new Dictionary<string, string>();
            foreach (SupperClub.Domain.SupperClub sc in _supperClubRepository.GetAllActiveSupperClubs())
            {
                try
                {
                    scUrls.Add(sc.Name, sc.UrlFriendlyName == null ? "" : sc.UrlFriendlyName.ToLower());
                }
                catch (Exception ex) { LogMessage("Error adding SupperClub to SiteMap. Error:" + ex.Message + " StackTrace:" + ex.StackTrace + " SupperClubId:" + sc.Id, LogLevel.ERROR); }
            }
            ViewBag.SupperClubsFriendlyUrls = scUrls;

            // List of Category pages
            Dictionary<string, string> cUrls = new Dictionary<string, string>();
            Dictionary<string, string> ctUrls = new Dictionary<string, string>();
            foreach (SupperClub.Domain.SearchCategory c in _supperClubRepository.GetAllSearchCategories())
            {
                cUrls.Add(c.Name, c.UrlFriendlyName == null ? "" : c.UrlFriendlyName.ToLower());
                if(c.SearchCategoryTags != null)
                {
                    foreach(SearchCategoryTag t in c.SearchCategoryTags)
                    {
                        try
                        {
                        ctUrls.Add(t.Tag.Name, t.Tag.UrlFriendlyName == null ? "" : c.UrlFriendlyName.ToLower() + "/" + t.Tag.UrlFriendlyName.ToLower());
                        }
                        catch (Exception ex) { LogMessage("Error adding SearchCategoryTag to SiteMap. Error:" + ex.Message + " StackTrace:" + ex.StackTrace + " SearchCategory Tag Id:" + t.Tag.Id, LogLevel.ERROR); }
                    }
                }                
            }
            foreach(Tag t in _supperClubRepository.GetAllTagsWithoutCategory())
            {
                ctUrls.Add(t.Name, t.UrlFriendlyName == null ? "" : t.UrlFriendlyName.ToLower());
            }

            ViewBag.CategoryUrls = cUrls;
            ViewBag.CategoryTagUrls = ctUrls;

            //Category Tag with City Urls
            Dictionary<string, string> cityCatTagUrls = new Dictionary<string, string>();
            IList<SupperClub.Domain.SiteMapCityCategoryTagList> cityCatTagList = _supperClubRepository.GetAllSearchCategoryTagCityList();
            foreach (SiteMapCityCategoryTagList cct in cityCatTagList)
            {
                string url =  (cct.CategoryUrlFriendlyName == null ? "" : cct.CategoryUrlFriendlyName.ToLower()) + "/" + (cct.TagUrlFriendlyName == null ? "" : cct.TagUrlFriendlyName.ToLower()) + "/" + (cct.CityUrlFriendlyName == null ? "" : cct.CityUrlFriendlyName.ToLower());
                cityCatTagUrls.Add(url, url);
            }
            ViewBag.CityCategoryTagUrls = cityCatTagUrls;

            //Category Tag with City/Area Urls
            Dictionary<string, string> cityAreaCatTagUrls = new Dictionary<string, string>();
            IList<SupperClub.Domain.SiteMapCityAreaCategoryTagList> cityAreaCatTagList = _supperClubRepository.GetAllSearchCategoryTagCityAreaList();
            foreach (SiteMapCityAreaCategoryTagList cct in cityAreaCatTagList)
            {
                string url = (cct.CategoryUrlFriendlyName == null ? "" : cct.CategoryUrlFriendlyName.ToLower()) + "/" + (cct.TagUrlFriendlyName == null ? "" : cct.TagUrlFriendlyName.ToLower()) + "/" + (cct.CityUrlFriendlyName == null ? "" : cct.CityUrlFriendlyName.ToLower()) + "/" + (cct.AreaUrlFriendlyName == null ? "" : cct.AreaUrlFriendlyName.ToLower());
                cityAreaCatTagUrls.Add(url, url);
            }
            ViewBag.CityAreaCategoryTagUrls = cityAreaCatTagUrls;

            Dictionary<string, string> eventUrls = new Dictionary<string, string>();
            IList<SiteMapEventList> activeEventList = _supperClubRepository.GetAllSEOEventUrls();            
            foreach (SiteMapEventList e in activeEventList)
            {
                try
                {
                    string url = e.SupperClubUrlFriendlyName.ToLower() + "/"+ e.EventUrlFriendlyName.ToLower();
                    eventUrls.Add(url, url);
                }
                catch (Exception ex)
                { LogMessage("Error adding Event to SiteMap. Error:" + ex.Message + " StackTrace:" + ex.StackTrace + " Event:" + e.EventUrlFriendlyName, LogLevel.ERROR); }
            }
            ViewBag.EventFriendlyUrls = eventUrls;

            Dictionary<string, string> otherUrls = new Dictionary<string, string>();
            otherUrls.Add("Home Page", "");
            otherUrls.Add("Search Page", "pop-up-restaurants");
            foreach (UrlRewrite ur in _supperClubRepository.GetUrlRewrites())
                otherUrls.Add(ur.ActualUrl, ur.RewriteUrl.ToLower());

            otherUrls.Add("Chefs Page", "Chefs");
            otherUrls.Add("Contact Us Page", "contact-us");
            otherUrls.Add("Blog", "blog/");
            ViewBag.OtherFriendlyUrls = otherUrls;

            // List of Location pages
            Dictionary<string, string> cityUrls = new Dictionary<string, string>();
            Dictionary<string, string> caUrls = new Dictionary<string, string>();
            foreach (SupperClub.Domain.City ct in _supperClubRepository.GetAllCities())
            {
                string url = "pop-up-restaurants/" + ct.UrlFriendlyName;
                cityUrls.Add(ct.Name, url);                
            }
            foreach (Area a in _supperClubRepository.GetAllAreas())
            {
                try
                {
                    string url = "pop-up-restaurants/" + a.City.UrlFriendlyName + "/" + a.UrlFriendlyName;
                    caUrls.Add(a.Name,  url);
                }
                catch (Exception ex) { LogMessage("Error adding CityArea to SiteMap. Error:" + ex.Message + " StackTrace:" + ex.StackTrace + " City Area Id:" + a.Id, LogLevel.ERROR); }
            }
           
            ViewBag.CityUrls = cityUrls;
            ViewBag.CityArealUrls = caUrls;

            int timeinterval = int.Parse(System.Web.Configuration.WebConfigurationManager.AppSettings["SitemapUpdateInterval"]);
            if ((fi.Exists && fi.LastWriteTime < DateTime.Now.AddHours(-1 * timeinterval)) || !fi.Exists)
            {
                try
                {
                     // only allow it to be written once an hour in case someone spams this page (so it doesnt crash the site)
                        xd = new XmlDocument();
                        XmlNode rootNode = xd.CreateElement("urlset");

                        // add namespace
                        XmlAttribute attrXmlNS = xd.CreateAttribute("xmlns");
                        attrXmlNS.InnerText = "https://www.sitemaps.org/schemas/sitemap/0.9";
                        rootNode.Attributes.Append(attrXmlNS);

                        List<string> NavItems = new List<string>();
                        foreach (KeyValuePair<string, string> kvp in otherUrls)
                            NavItems.Add(kvp.Value);

                        foreach (KeyValuePair<string, string> kvp in scUrls)
                            NavItems.Add(kvp.Value);
                        
                        foreach (KeyValuePair<string, string> kvp in scUrls)
                            NavItems.Add(kvp.Value + "/reviews");

                        foreach (KeyValuePair<string, string> kvp in eventUrls)
                            NavItems.Add(kvp.Value);

                        if (cUrls != null && cUrls.Count > 0)
                        {
                            foreach (KeyValuePair<string, string> kvp in cUrls)
                                NavItems.Add(kvp.Value);
                        }
                        if (ctUrls != null && ctUrls.Count > 0)
                        {
                            foreach (KeyValuePair<string, string> kvp in ctUrls)
                                NavItems.Add(kvp.Value);
                        }
                        if (cityCatTagUrls != null && cityCatTagUrls.Count > 0)
                        {
                            foreach (KeyValuePair<string, string> kvp in cityCatTagUrls)
                                NavItems.Add(kvp.Value);
                        }
                        if (cityAreaCatTagUrls != null && cityAreaCatTagUrls.Count > 0)
                        {
                            foreach (KeyValuePair<string, string> kvp in cityAreaCatTagUrls)
                                NavItems.Add(kvp.Value);
                        }
                        if (cityUrls != null && cityUrls.Count > 0)
                        {
                            foreach (KeyValuePair<string, string> kvp in cityUrls)
                                NavItems.Add(kvp.Value);
                        }
                        if (caUrls != null && caUrls.Count > 0)
                        {
                            foreach (KeyValuePair<string, string> kvp in caUrls)
                                NavItems.Add(kvp.Value);
                        }
                        
                        foreach (string url in NavItems)
                        {
                            rootNode.AppendChild(GenerateUrlNode(ref xd, "https://grubclub.com/" + url, DateTime.Now, "daily", "0.80"));
                        }

                        // ADD THE REST OF YOUR URL'S HERE

                        // append all nodes to the xmldocument and save it to sitemap.xml
                        xd.AppendChild(rootNode);
                        xd.InsertBefore(xd.CreateXmlDeclaration("1.0", "UTF-8", null), rootNode);

                        if (!fi.Exists)
                        {
                            //create directory if it does not exists
                            try
                            {
                                string subPath = HttpRuntime.AppDomainAppPath + ""; // your code goes here
                                bool IsExists = System.IO.Directory.Exists(subPath);
                                if (!IsExists)
                                    System.IO.Directory.CreateDirectory(subPath);
                            }
                            catch (Exception ex)
                            {
                                LogMessage("Error creating Sitemap Folder. Message: " + ex.Message + "  Stack Trace: " + ex.StackTrace, Logger.LogLevel.ERROR);
                            }
                            // Create the sitemap xml file if it doesn't exist
                            FileStream fs = null;
                            try
                            {
                                fs = new FileStream(sSiteMapFilePath, FileMode.OpenOrCreate);
                                if (fs != null)
                                    xd.Save(fs);
                            }
                            catch (Exception ex)
                            {
                                LogMessage("Error creating or opening sitemap file. Message: " + ex.Message + "  Stack Trace: " + ex.StackTrace, Logger.LogLevel.ERROR);
                            }
                            finally
                            {
                                fs.Close();
                            }
                        }
                        else
                            xd.Save(sSiteMapFilePath);

                        // PING SEARCH ENGINES TO LET THEM KNOW YOU UPDATED YOUR SITEMAP
                        bool submitSitemap = bool.Parse(System.Web.Configuration.WebConfigurationManager.AppSettings["SubmitSitemap"]);
                        if (submitSitemap)
                        {
                            // resubmit to google
                            try
                            {
                                System.Net.HttpWebRequest reqGoogle = (HttpWebRequest)System.Net.WebRequest.Create("https://www.google.com/webmasters/sitemaps/ping?sitemap=" + HttpUtility.UrlEncode(string.Format(@"{0}sitemap.xml", ServerMethods.SiteMapServerUrl)));
                                HttpWebResponse responseGoogle = (HttpWebResponse)reqGoogle.GetResponse();
                                if ((int)responseGoogle.StatusCode == 200)
                                    LogMessage("Sitemap Submitted successfully to Google Search engine", Logger.LogLevel.INFO);
                                else
                                    LogMessage("Error updating sitemap to Google Search engine. Status code returned: " + responseGoogle.StatusCode.ToString(), Logger.LogLevel.ERROR);
                            }
                            catch (Exception ex)
                            {
                                LogMessage("Error occured while submitting sitemap to Google. Message: " + ex.Message + "   Stack Trace: " + ex.StackTrace, Logger.LogLevel.ERROR);
                            }

                            // resubmit to ask
                            //try
                            //{
                            //    System.Net.HttpWebRequest reqAsk = (HttpWebRequest)System.Net.WebRequest.Create("https://submissions.ask.com/ping?sitemap=" + HttpUtility.UrlEncode(string.Format(@"{0}sitemap.xml", ServerMethods.SiteMapServerUrl)));
                            //    HttpWebResponse responseAsk = (HttpWebResponse)reqAsk.GetResponse();
                            //    if ((int)responseAsk.StatusCode == 200)
                            //        LogMessage("Sitemap Submitted to Ask Search engine", Logger.LogLevel.INFO);
                            //    else
                            //        LogMessage("Error updating sitemap to Ask Search engine. Status code returned: " + responseAsk.StatusCode.ToString(), Logger.LogLevel.ERROR);
                            //}
                            //catch (Exception ex)
                            //{
                            //    LogMessage("Error occured while submitting sitemap to Ask. Message: " + ex.Message + "   Stack Trace: " + ex.StackTrace, Logger.LogLevel.ERROR);
                            //}

                            // resubmit to yahoo
                            //try
                            //{
                            //    System.Net.HttpWebRequest reqYahoo = (HttpWebRequest)System.Net.WebRequest.Create("https://search.yahooapis.com/SiteExplorerService/V1/updateNotification?appid=YahooDemo&url=" + HttpUtility.UrlEncode(string.Format(@"{0}sitemap.xml", ServerMethods.SiteMapServerUrl)));
                            //    HttpWebResponse responseYahoo = (HttpWebResponse)reqYahoo.GetResponse();
                            //    if ((int)responseYahoo.StatusCode == 200)
                            //        LogMessage("Sitemap Submitted to Yahoo Search engine", Logger.LogLevel.INFO);
                            //    else
                            //        LogMessage("Error updating sitemap to Yahoo Search engine. Status code returned: " + responseYahoo.StatusCode.ToString(), Logger.LogLevel.ERROR);
                            //}
                            //catch (Exception ex)
                            //{
                            //    LogMessage("Error occured while submitting sitemap to Yahoo. Message: " + ex.Message + "   Stack Trace: " + ex.StackTrace, Logger.LogLevel.ERROR);
                            //}

                            // resubmit to bing
                            try
                            {
                                System.Net.HttpWebRequest reqBing = (HttpWebRequest)System.Net.WebRequest.Create("https://www.bing.com/webmaster/ping.aspx?siteMap=" + HttpUtility.UrlEncode(string.Format(@"{0}sitemap.xml", ServerMethods.SiteMapServerUrl)));
                                HttpWebResponse responseBing = (HttpWebResponse)reqBing.GetResponse();
                                if ((int)responseBing.StatusCode == 200)
                                    LogMessage("Sitemap Submitted to Bing Search engine", Logger.LogLevel.INFO);
                                else
                                    LogMessage("Error updating sitemap to Bing Search engine. Status code returned: " + responseBing.StatusCode.ToString(), Logger.LogLevel.ERROR);
                            }
                            catch (Exception ex)
                            {
                                LogMessage("Error occured while submitting sitemap to Bing. Message: " + ex.Message + "   Stack Trace: " + ex.StackTrace, Logger.LogLevel.ERROR);
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        LogMessage("Error occured while writing sitemap xml. " + ex.Message + "   Stack Trace: " + ex.StackTrace, Logger.LogLevel.ERROR);
                    }
                }
            return View();
        }       

        [HttpPost]
        public ActionResult Subscribe(string emailAddress, string location)
        {
            bool success = false;
            success = addSubscriber(emailAddress, SupperClub.Domain.SubscriberType.NewsLetter);
            Segment.Analytics.Client.Track(_segmentMethod.GetSegmentUser(), "Newsletter Subscription", new Segment.Model.Properties() {
                    { "email", emailAddress },
                    { "location", location }});           
            return Json(success, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult PrivateGrubClubRequest(string email, string numberOfGuests, string cuisine, string pricePerHead, string dateChoice, string locationChoice, string otherRequirements)
        {
            EmailService.EmailService emailer = new EmailService.EmailService(_supperClubRepository);
            bool success = emailer.SendAdminPrivateGrubClubRequestEmail(email, numberOfGuests, cuisine, pricePerHead, dateChoice,locationChoice, otherRequirements);  
            return Json(success, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]

        public ActionResult PrivateGrubClubSubmit(string venue, string chefName, string email, string numberOfGuests, string cuisine, string pricePerHead, string dateChoice, string locationChoice, string otherRequirements)
        {
            EmailService.EmailService emailer = new EmailService.EmailService(_supperClubRepository);
            bool success = emailer.SendAdminPrivateGrubClubRequestEmail(venue, chefName, email, numberOfGuests, cuisine, pricePerHead, dateChoice, locationChoice, otherRequirements);
            return Json(success, JsonRequestBehavior.AllowGet);
        }
        // Duration = seconds * minutes * hours
        [OutputCache(Duration = 60*60*1, Location = System.Web.UI.OutputCacheLocation.Server, VaryByParam = "none")]
        public ActionResult GetTwitterFeed()
        {
            try
            {
                // This ActionResult is cached for all users so we don't hit the 150 hits/hour limit
                //var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.twitter.com/1/statuses/user_timeline/Grub_Club.json?count=2&callback=?");
                //var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                //using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                //{
                //    var responseText = streamReader.ReadToEnd();
                //    return Json(responseText, JsonRequestBehavior.AllowGet);
                //}
                // get these from somewhere nice and secure...
                var key = System.Web.Configuration.WebConfigurationManager.AppSettings["twitterConsumerKey"];
                var secret = System.Web.Configuration.WebConfigurationManager.AppSettings["twitterConsumerSecret"];
                
                var server = System.Web.HttpContext.Current.Server;
                var bearerToken = server.UrlEncode(key) + ":" + server.UrlEncode(secret);
                var b64Bearer = Convert.ToBase64String(System.Text.Encoding.Default.GetBytes(bearerToken));
                using (var wc = new WebClient())
                {
                    wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded;charset=UTF-8");
                    wc.Headers.Add("Authorization", "Basic " + b64Bearer);
                    var tokenPayload = wc.UploadString("https://api.twitter.com/oauth2/token", "grant_type=client_credentials");
                    var rgx = new System.Text.RegularExpressions.Regex("\"access_token\"\\s*:\\s*\"([^\"]*)\"");
                    // you can store this accessToken and just do the next bit if you want
                    var accessToken = rgx.Match(tokenPayload).Groups[1].Value;
                    wc.Headers.Clear();
                    wc.Headers.Add("Authorization", "Bearer " + accessToken);

                    const string url = "https://api.twitter.com/1.1/statuses/user_timeline.json?screen_name=Grub_Club&count=2";
                    // ...or you could pass through the query string and use this handler as if it was the old user_timeline.json
                    // but only with YOUR Twitter account

                    var tweets = wc.DownloadString(url);
                    return Json(tweets, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                LogMessage("Failed to get Twitter Feed. Exception: " + ex.Message, Logger.LogLevel.WARN);
                return Json("None", JsonRequestBehavior.AllowGet);
            }
        }

        private bool IsUserValidByRefer(string email)
        {
            User user= _supperClubRepository.GetUser(email);
            if(user != null)
            {
                if (!_supperClubRepository.IsUserValidToRefer(user))
                {
                    return _supperClubRepository.IsUserValidByRefer(email);
                }
                else
                    return false;
               // return _supperClubRepository.IsUserValidByRefer(email);
            }
            else
            {
                return _supperClubRepository.IsUserValidByRefer(email);
            }

        }

        public ActionResult ThankYou()
        {
            return View();
        }

        [HttpPost]
       //public ActionResult ReferalVoucher(string email, string friends)
       // {
       //     if (Membership.FindUsersByEmail(email).Count != 1)
       //     {
       //         SetNotification(NotificationType.Error, "We don't have that email address the system, you sure it was the right one?", false, true, false);
       //         return View();
       //     }
       //     else
       //     {
       //        // IEnumerable<MembershipUser> members = Membership.FindUsersByEmail(email).Cast<MembershipUser>();

       //       //  string username = members.First().UserName;

       //       //  User user1= UserMethods.GetSpecificUser(email);


       //         bool success = false;
       //         List<string> lstfriends = new List<string>();

       //         //lstemails = friendemail.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
       //         lstfriends = friends.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries).ToList();
       //         User user = _supperClubRepository.GetUser(email);
       //         Guid userId = user.Id;
       //         string successusers = string.Empty;
       //         string failureusers = string.Empty;
       //         string failureProcess = string.Empty;
       //         if (userId != null && _supperClubRepository.IsUserValidToRefer(user))
       //         {
       //             foreach (string friend in lstfriends)
       //             {
       //                 // List<string> frienddetail = new List<string>();
       //                 string mailId = friend.Split(new char[] { '~' }, StringSplitOptions.RemoveEmptyEntries)[1];
       //                 string name = friend.Split(new char[] { '~' }, StringSplitOptions.RemoveEmptyEntries)[0];
       //                 if (IsUserValidByRefer(mailId))
       //                 {
       //                     EmailService.EmailService emailer = new EmailService.EmailService(_supperClubRepository);
       //                     Voucher voucher = CreateReferalVoucher(email, mailId);

       //                     if (!string.IsNullOrEmpty(voucher.Code))
       //                     {
       //                         UserVoucherTypeDetail detail = new UserVoucherTypeDetail();

       //                         detail.UserId = userId;
       //                         detail.VType = (int)(VType.Referral);
       //                         detail.VoucherType = (int)(VoucherType.PercentageOff);
       //                         detail.Name = name;
       //                         detail.FriendEmailId = mailId;
       //                         detail.CreatedDate = DateTime.Now;
       //                         detail.Value = Convert.ToDecimal("10.00");
       //                         detail.VoucherId = voucher.Id;
       //                         detail.BasketId = null;

       //                         UserVoucherTypeDetail _detail = _supperClubRepository.CreateUserVoucherTypeDetail(detail);

       //                         success = emailer.SendFriendReferralVoucherUsEmail(user.FirstName+' '+user.LastName,name,mailId, DateTime.Now.AddDays(30), voucher.Code);
       //                         if (success)
       //                         {
       //                             successusers = successusers + name + ",";
       //                         }
       //                         else
       //                         {
       //                             failureProcess = failureProcess + name + ",";
                                        
       //                         }
       //                     }

       //                 }
       //                 else
       //                 {
       //                     failureusers = failureusers + name + ",";
       //                 }
       //             }
       //         }

       //         else
       //         {
       //             return Json("User is not valid to refer, book an event to refer.", JsonRequestBehavior.AllowGet);
       //         }
       //         successusers = (!string.IsNullOrEmpty(successusers)) ? successusers.Substring(0, successusers.Length - 1).ToString() : successusers;
       //         failureusers = (!string.IsNullOrEmpty(failureusers)) ? failureusers.Substring(0, failureusers.Length - 1).ToString() : failureusers;

       //         SetNotification(NotificationType.Success, successusers, false, false, true);
       //         SetNotification(NotificationType.Error, failureusers, false, false, true);
       //         if (!string.IsNullOrEmpty(failureusers))
       //         {
       //             string message = string.Empty;
       //             if (!string.IsNullOrEmpty(successusers))
       //                 message = successusers + " - user received referal voucher.\n";
       //             if (!string.IsNullOrEmpty(failureProcess))
       //                 message = failureProcess + "- failed to send referral voucher code.\n";

       //             message = message + failureusers + "- failed sending -user already received referral voucher code or part of grubclub users.";

       //             return Json(message, JsonRequestBehavior.AllowGet);
       //         }
       //         else
       //         {
       //             return Json(true, JsonRequestBehavior.AllowGet);
       //           //  return View("ThankYou");
       //         }
       //     }

       // }

       
        public ActionResult ReferralVoucher(string email, string friendemail, string friendName)
        {
            if (Membership.FindUsersByEmail(email).Count != 1)
            {
             //   SetNotification(NotificationType.Error, "We don't have that email address the system, you sure it was the right one?", false, true, false);
                return Json(email+"- is not a registered User", JsonRequestBehavior.AllowGet);
            }
            else
            {
                IEnumerable<MembershipUser> members = Membership.FindUsersByEmail(email).Cast<MembershipUser>();
                string username = members.First().UserName;


                bool success = false;
                List<string> lstemails = new List<string>();
                lstemails = friendemail.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                User user = _supperClubRepository.GetUser(email);
                Guid userId = user.Id;
                string successusers = string.Empty;
                string failureusers = string.Empty;
                if (userId != null && _supperClubRepository.IsUserValidToRefer(user))
                {
                    foreach (string mailId in lstemails)
                    {

                        User userfriend = _supperClubRepository.GetUser(mailId);
                        if (userfriend != null)
                        {
                            if (!(_supperClubRepository.IsUserValidToRefer(userfriend)))
                            {
                                if (_supperClubRepository.IsUserValidByRefer(mailId))
                                {
                                    EmailService.EmailService emailer = new EmailService.EmailService(_supperClubRepository);
                                    Voucher voucher = CreateReferalVoucher(email, mailId);

                                    if (!string.IsNullOrEmpty(voucher.Code))
                                    {
                                        UserVoucherTypeDetail detail = new UserVoucherTypeDetail();

                                        detail.UserId = userId;
                                        detail.VType = (int)(VType.Referral);
                                        detail.VoucherType = (int)(VoucherType.PercentageOff);
                                        detail.Name = friendName;
                                        detail.FriendEmailId = mailId;
                                        detail.CreatedDate = DateTime.Now;
                                        detail.Value = Convert.ToDecimal("10.00");
                                        detail.VoucherId = voucher.Id;
                                        detail.BasketId = null;

                                        UserVoucherTypeDetail _detail = _supperClubRepository.CreateUserVoucherTypeDetail(detail);

                                        success = emailer.SendFriendReferralVoucherUsEmail(user.FirstName + " " + user.LastName, friendName, mailId, DateTime.Today.AddMonths(2), voucher.Code);
                                       if (success)
                                        successusers = successusers + mailId + ",";
                                    else
                                        failureusers = friendName+ " - Failed to send referral voucher";
                                    }
                                }
                                    else
                                    {
                                        failureusers = friendName+"- Failed Sending, user has already received referral voucher";
                                    }
                                }
                            else
                            {
                                failureusers= friendName+"- Failed Sending, user has already attended grubclubs";
                            }
                                     
                        }
                        else
                        {
                            if (_supperClubRepository.IsUserValidByRefer(mailId))
                            {
                                EmailService.EmailService emailer = new EmailService.EmailService(_supperClubRepository);
                                Voucher voucher = CreateReferalVoucher(email, mailId);

                                if (!string.IsNullOrEmpty(voucher.Code))
                                {
                                    UserVoucherTypeDetail detail = new UserVoucherTypeDetail();

                                    detail.UserId = userId;
                                    detail.VType = (int)(VType.Referral);
                                    detail.VoucherType = (int)(VoucherType.PercentageOff);
                                    detail.Name = friendName;
                                    detail.FriendEmailId = mailId;
                                    detail.CreatedDate = DateTime.Now;
                                    detail.Value = Convert.ToDecimal("10.00");
                                    detail.VoucherId = voucher.Id;
                                    detail.BasketId = null;

                                    UserVoucherTypeDetail _detail = _supperClubRepository.CreateUserVoucherTypeDetail(detail);

                                    success = emailer.SendFriendReferralVoucherUsEmail(user.FirstName + " " + user.LastName, friendName, mailId, DateTime.Today.AddMonths(2), voucher.Code);
                                    if (success)
                                        successusers = successusers + mailId + ",";
                                    else
                                        failureusers = friendName+ " - Failed to send referral voucher";
                                }
                            }

                                else
                                {
                                    failureusers = friendName+"- Failed Sending, user has already received referral voucher";
                                }
                            }
                        }


                       
                        if (string.IsNullOrEmpty(failureusers))

                            return Json(true, JsonRequestBehavior.AllowGet);
                        else
                            return Json(failureusers, JsonRequestBehavior.AllowGet);
                   // }
                }// referree valid
                else
                {
                    return Json(user.FirstName+" "+ user.LastName+" is not eligible to refer a friend, Please book an event to Refer",JsonRequestBehavior.AllowGet);

                }
            }
            }
    

        

        private void setHttpContextForGiftvoucher()
        {
            LogOnModel model = new LogOnModel();
            model.UserName = "giftvoucher@grubclub.com";
            model.Password = "grubclub";
            model.redirectUrl ="/home/giftvoucher" ;
            model.RememberMe = false;

            FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);

        }


        [HttpPost]
        public ActionResult GiftVoucher(string name, string email, string voucher)
        {
            TicketingService basketService = new TicketingService(_supperClubRepository);
            LogMessage("Buying Gift Voucher. UserAgent:" + Request.UserAgent + " Browser:" + Request.Browser.Browser + "  BrowserVersion:" + Request.Browser.Version);

            BookingModel bm = new BookingModel
            {
                eventId = 777,
                numberOfTickets = 0,
                currency = "GBP",
                baseTicketCost = Convert.ToDecimal(voucher),
                totalTicketCost = Convert.ToDecimal(voucher),
                totalDue = Convert.ToDecimal(voucher),
                discount = 0,
                IsContactNumberRequired = true,
                seatingId = 0,
                bookingMenuModel = null,
                bookingSeatingModel = null,
                contactNumber =SupperClub.Code.UserMethods.CurrentUser.ContactNumber ,
                updateContactNumber = false,
                MinMaxBookingEnabled = false,
                MinTicketsAllowed = 0,
                MaxTicketsAllowed = 0,
                SeatSelectionInMultipleOfMin = false,
                ToggleMenuSelection = false
            };
            Session["GiftVoucher"] = name + "," + email;
           
            basketService.CreateTicketBasket(this.HttpContext);
         
            basketService.AddGiftVoucherToBasket(bm, Convert.ToDecimal(voucher), true);

            return Json(true, JsonRequestBehavior.AllowGet);
        }

        public ActionResult BuyGiftVoucher(string name, string email, string voucher)
        {
            if (!UserMethods.IsLoggedIn)
                return RedirectToAction("LogOn", "Account", new { returnUrl = "" });
            decimal voucherValue = 0;
            bool success = decimal.TryParse(voucher, out voucherValue);
            LogMessage("Buying Gift Voucher. UserAgent:" + Request.UserAgent + " Browser:" + Request.Browser.Browser + "  BrowserVersion:" + Request.Browser.Version);

            if (success && voucherValue > 0)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return RedirectToAction("GiftVoucher", "Home", new { validName = false });
                }
                else
                {
                    bool validEmail = false;
                    // Validate the email first
                    try
                    {
                        var addr = new System.Net.Mail.MailAddress(email);
                        validEmail = true;
                    }
                    catch
                    {
                        validEmail = false;
                    }
                    if (!validEmail)
                    {
                        return RedirectToAction("GiftVoucher", "Home", new { validName = true, validEmail = false });
                    }
                    else
                    {
                        TicketingService basketService = new TicketingService(_supperClubRepository);

                        BookingModel bm = new BookingModel
                        {
                            eventId = 777,
                            numberOfTickets = 0,
                            currency = "GBP",
                            baseTicketCost = voucherValue,
                            totalTicketCost = voucherValue,
                            totalDue = voucherValue,
                            discount = 0,
                            IsContactNumberRequired = true,
                            seatingId = 0,
                            bookingMenuModel = null,
                            bookingSeatingModel = null,
                            contactNumber = SupperClub.Code.UserMethods.CurrentUser.ContactNumber,
                            updateContactNumber = false,
                            MinMaxBookingEnabled = false,
                            MinTicketsAllowed = 0,
                            MaxTicketsAllowed = 0,
                            SeatSelectionInMultipleOfMin = false,
                            ToggleMenuSelection = false
                        };
                        Session["GiftVoucher"] = name + "," + email;

                        basketService.CreateTicketBasket(this.HttpContext);

                        basketService.AddGiftVoucherToBasket(bm, voucherValue, true);

                        return RedirectToAction("ReviewPurchaseCommon", "Booking");
                    }
                }
            }
            else
                return RedirectToAction("GiftVoucher", "Home");
        }
        public ActionResult ReferralVoucher()
        {
            return View();
        }

        public ActionResult GiftVoucher(bool validEmail=true, bool validName=true)
        {
            //if (!UserMethods.IsLoggedIn)
            // //   return RedirectToAction("LogOn", "Account", new { returnUrl = "/Home/Giftvoucher&utm_source=homepage&utm_medium=gift&utm_campaign=gift&utm_source=homepage&utm_medium=gift&utm_campaign=gift" });

            //  //  string  qstring="&utm_source=homepage&utm_medium=gift&utm_campaign=gift";

            //   //string qstring="/Home/Giftvoucher&utm_source=homepage&utm_medium=gift&utm_campaign=gift&utm_source=homepage&utm_medium=gift&utm_campaign=gift";
            //    return RedirectToAction("LogOn", "Account", new { returnUrl = "/home%2Fgiftvoucher%3Futm_source%3Dgifthp%26utm_medium%3Dgift%26utm_campaign%3Dgift" });
            ViewBag.ErrorNumber = 0;
            if (!validName)
                ViewBag.ErrorNumber = 1;
            if (!validEmail)
                ViewBag.ErrorNumber = 2;
            return View();
        }

       

        #region Static Public Pages
        [OutputCache(Duration = 60 * 60 * 24 * 365, Location = System.Web.UI.OutputCacheLocation.Any)]
        public ActionResult FacebookChannel()
        {
            return View();
        }

        public ActionResult HowItWorks()
        {
            return View();
        }

        public ActionResult ReferAFriend()
        {
            return View();
        }
        public ActionResult AboutUs()
        {
          //  return View();
            return View("About");
        }

        public ActionResult FAQsGuest()
        {
            return View();
        }

        public ActionResult FAQsHost()
        {
            return View();
        }

        public ActionResult HowItWorksGuests()
        {
            return View();
        }
     
        public ActionResult HowItWorksHosts()
        {
            return View();
        }

        public ActionResult HowToHost()
        {
            return View();
        }

        public ActionResult TermsAndConditions()
        {
            return View();
        }
        public ActionResult Experiences()
        {
            return RedirectPermanent("http://private-events.grubclub.com/");
        }
        public ActionResult NotesFromASupperClub()
        {
            return View();
        }        

        public ActionResult WhatIsAGrubClub()
        {
            return View();
        }

      
        #endregion

        #region Error Pages
        public ActionResult Error()
        {
            return View("../Shared/Error");
        }

        public ActionResult TestError()
        {
            string ob = null;
            ob.Contains("Nothing");
            return View();
        }

        public ActionResult FourOhFour()
        {     
            return View("../Shared/404_New");
        }
        #endregion

        #region Private Methods
        private static XmlNode GenerateUrlNode(ref XmlDocument xd, string sLoc, DateTime dLastMod, string sChangeFreq, string sPriority)
        {
            // XmlDocument xd = null;

            XmlNode nodeUrl = xd.CreateElement("url");

            XmlNode nodeLoc = xd.CreateElement("loc");
            nodeLoc.InnerText = sLoc;
            nodeUrl.AppendChild(nodeLoc);

            XmlNode nodeLastMod = xd.CreateElement("lastmod");
            nodeLastMod.InnerText = dLastMod.ToString("yyyy-MM-ddThh:mm:ss+00:00");
            nodeUrl.AppendChild(nodeLastMod);

            //XmlNode nodeChangeFreq = xd.CreateElement("changefreq");
            //nodeChangeFreq.InnerText = sChangeFreq;
            //nodeUrl.AppendChild(nodeChangeFreq);

            XmlNode nodePriority = xd.CreateElement("priority");
            nodePriority.InnerText = sPriority;
            nodeUrl.AppendChild(nodePriority);

            return nodeUrl;
        }

        private bool addSubscriber(string emailAddress, SupperClub.Domain.SubscriberType subType, string firstName = null, string lastName = null)
        {
            bool success = false;
            // Validate the email first
            try
            {
                var addr = new System.Net.Mail.MailAddress(emailAddress);
                success = true;
            }
            catch
            {
                success = false;
            }

            // If it's valid try add it to the database
            if (success)
            {
                Subscriber subscriber = new Subscriber();
                subscriber.SubscriptionDateTime = DateTime.Now;
                subscriber.EmailAddress = emailAddress;                
                subscriber.SubscriberType = (int)subType;

                success = _supperClubRepository.AddSubscriber(subscriber);
            }
            return success;
        }


        private Voucher CreateReferalVoucher(string referalEmail, string referedemail)
        {
           
            LogMessage("Begin Voucher Creation");
            Voucher voucher = new Voucher();
            string code = string.Empty;
            try
            {

             SupperClub.Code.VoucherManager vmgr = new VoucherManager();
             string description = "Referral Voucher Referred By:"+referalEmail;
            voucher = vmgr.CreateVoucher(description, Convert.ToDecimal("10.00"), 1);
           
            }
            catch (Exception ex)
            {
             
                LogMessage("Problem while creating voucher. Error Details: " + ex.StackTrace, LogLevel.ERROR);
                SetNotification(Models.NotificationType.Warning, ex.Message, true);
            }

            return voucher;
        }
        #endregion
    }
}